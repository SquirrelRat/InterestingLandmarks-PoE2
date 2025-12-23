using ExileCore2;
using ExileCore2.PoEMemory.Components;
using ExileCore2.PoEMemory.MemoryObjects;
using ExileCore2.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using Vector2 = System.Numerics.Vector2;

namespace InterestingLandmarks
{
    public class InterestingLandmarks : BaseSettingsPlugin<InterestingLandmarksSettings>
    {
        private bool _renderEnabled = true;
        private readonly Stopwatch _stopwatch = new();
        private List<Renderable> _cachedRenderables = new();

        private record Renderable
        {
            public Entity Entity { get; init; }
            public string Label { get; init; }
            public Color Color { get; init; }
        }

        private static readonly EntityType[] RelevantTypes =
        {
            EntityType.Chest,
            EntityType.AreaTransition,
            EntityType.IngameIcon,
            EntityType.Waypoint,
            EntityType.Monolith,
            EntityType.Shrine,
            EntityType.Breach,
            EntityType.Terrain
        };

        public override void OnLoad()
        {
            Settings.MasterToggleHotkey.OnValueChanged += () => { _renderEnabled = !_renderEnabled; };
            _stopwatch.Start();
        }

        public override void Render()
        {
            var ingameUi = GameController.Game.IngameState.IngameUi;

            if (!_renderEnabled || !Settings.Enable || GameController.Area.CurrentArea == null || GameController.Area.CurrentArea.IsTown
                || GameController.Area.CurrentArea.IsHideout || GameController.IsLoading || !GameController.InGame
                || !ingameUi.Map.LargeMap.IsVisible)
            {
                return;
            }

            if (ingameUi.InventoryPanel.IsVisible ||
                ingameUi.TreePanel.IsVisible ||
                ingameUi.AtlasTreePanel.IsVisible ||
                ingameUi.SkillsWindow.IsVisible ||
                ingameUi.WorldMap.IsVisible ||
                ingameUi.SettingsPanel.IsVisible ||
                ingameUi.StashElement.IsVisible ||
                ingameUi.GuildStashElement.IsVisible ||
                ingameUi.NpcDialog.IsVisible ||
                ingameUi.PurchaseWindow.IsVisible ||
                ingameUi.SellWindow.IsVisible ||
                ingameUi.TradeWindow.IsVisible)
            {
                return;
            }

            var playerPos = GameController.Player.GridPos;

            if (_stopwatch.ElapsedMilliseconds > Settings.UpdateInterval)
            {
                _cachedRenderables = CollectRenderables(playerPos);
                _stopwatch.Restart();
            }

            if (Settings.ShowRituals)
            {
                foreach (var entity in GameController.EntityListWrapper.ValidEntitiesByType[EntityType.Terrain])
                {
                    if (entity.Path.Contains("RitualRuneObject"))
                    {
                        var screenPos = GameController.IngameState.Data.GetGridMapScreenPosition(entity.GridPos);
                        Graphics.DrawTextWithBackground("Ritual", screenPos, Settings.RitualColor, FontAlign.Center, Color.Black);
                    }
                }
            }

            try
            {
                DrawRenderables();
            }
            catch (Exception e)
            {
                LogError($"InterestingLandmarks.Render() failed: {e.Message}");
            }
        }

        private void DrawRenderables()
        {
            if (!Settings.EnableClustering)
            {
                foreach (var renderable in _cachedRenderables)
                {
                    DrawLabel(renderable.Entity.GridPos, renderable.Label, renderable.Color);
                }
                return;
            }

            var clusterRadiusSquared = (float)Settings.ClusterRadius.Value * Settings.ClusterRadius.Value;
            var processedIndices = new HashSet<int>();

            for (int i = 0; i < _cachedRenderables.Count; i++)
            {
                if (processedIndices.Contains(i)) continue;

                var current = _cachedRenderables[i];
                var cluster = new List<Renderable> { current };
                processedIndices.Add(i);

                for (int j = i + 1; j < _cachedRenderables.Count; j++)
                {
                    if (processedIndices.Contains(j)) continue;

                    var other = _cachedRenderables[j];
                    if (current.Label == other.Label &&
                        Vector2.DistanceSquared(current.Entity.GridPos, other.Entity.GridPos) < clusterRadiusSquared)
                    {
                        cluster.Add(other);
                        processedIndices.Add(j);
                    }
                }

                var label = cluster.Count > 1 ? $"{cluster[0].Label} (x{cluster.Count})" : cluster[0].Label;
                var drawPos = cluster.Count > 1 
                    ? cluster.Aggregate(Vector2.Zero, (acc, r) => acc + r.Entity.GridPos) / cluster.Count 
                    : current.Entity.GridPos;

                DrawLabel(drawPos, label, cluster[0].Color);
            }
        }

        private List<Renderable> CollectRenderables(Vector2 playerPos)
        {
            var maxRenderDistanceSquared = (float)Settings.MaxRenderDistance.Value * Settings.MaxRenderDistance.Value;
            var newRenderables = new List<Renderable>();

            foreach (var type in RelevantTypes)
            {
                if (!GameController.EntityListWrapper.ValidEntitiesByType.TryGetValue(type, out var entities))
                    continue;

                foreach (var entity in entities)
                {
                    if (entity.GridPos == Vector2.Zero)
                        continue;

                    if (Vector2.DistanceSquared(playerPos, entity.GridPos) > maxRenderDistanceSquared)
                        continue;

                    var renderableInfo = GetRenderableInfo(entity);
                    if (renderableInfo != null)
                    {
                        newRenderables.Add(renderableInfo);
                    }
                }
            }
            return newRenderables;
        }

        private Renderable GetRenderableInfo(Entity entity)
        {
            (string Label, Color Color)? info = entity.Type switch
            {
                EntityType.Chest when Settings.ShowChests => GetChestInfo(entity),
                EntityType.AreaTransition when Settings.ShowTransitions => GetTransitionInfo(entity),
                EntityType.IngameIcon when Settings.ShowPoI => GetPoIInfo(entity),
                EntityType.Waypoint when Settings.ShowWaypoints => GetWaypointInfo(entity),
                EntityType.Monolith when Settings.ShowEssence => GetEssenceInfo(entity),
                EntityType.Shrine when Settings.ShowShrine => GetShrineInfo(entity),
                EntityType.Breach when Settings.ShowBreach => ("Breach", Settings.BreachColor),
                EntityType.Terrain => GetTerrainInfo(entity),
                _ => null
            };

            return info.HasValue 
                ? new Renderable { Entity = entity, Label = info.Value.Label, Color = info.Value.Color } 
                : null;
        }

        private (string, Color)? GetChestInfo(Entity e)
        {
            var chest = e.GetComponent<Chest>();
            if (chest == null || chest.IsOpened || !e.IsTargetable || e.Path.Contains("Sanctum")) return null;

            if (e.Path.Contains("Strongbox"))
            {
                if (!Settings.ShowStrongboxes) return null;
                var (label, color) = GetStrongboxDetails(e);
                return color.HasValue ? (label, color.Value) : null;
            }

            var chestColor = GetChestColor(e);
            return chestColor.HasValue ? (e.RenderName, chestColor.Value) : null;
        }
        
        private (string label, Color? color) GetStrongboxDetails(Entity e)
        {
            string path = e.Path;
            
            if (Settings.ShowUniqueStrongbox && e.Rarity == MonsterRarity.Unique) return (e.RenderName, Settings.UniqueStrongboxColor);
            if (Settings.ShowArcanistStrongbox && path.Contains("Arcanist")) return ("Arcanist's Strongbox", Settings.ArcanistStrongboxColor);
            if (Settings.ShowCartographerStrongbox && path.Contains("Cartographer")) return ("Cartographer's Strongbox", Settings.CartographerStrongboxColor);
            if (Settings.ShowDivinerStrongbox && path.Contains("Diviner")) return ("Diviner's Strongbox", Settings.DivinerStrongboxColor);

            if (!Settings.ShowOtherStrongbox) return (null, null);

            Color? rarityColor = e.Rarity switch
            {
                MonsterRarity.White => Settings.OtherStrongboxColor,
                MonsterRarity.Magic => Settings.MagicChestColor,
                MonsterRarity.Rare => Settings.RareChestColor,
                _ => null
            };
            return (e.RenderName, rarityColor);
        }

        private Color? GetChestColor(Entity chestEntity)
        {
            return chestEntity.Rarity switch
            {
                MonsterRarity.White when Settings.ShowWhiteChests => GetWhiteChestColor(chestEntity),
                MonsterRarity.Magic when Settings.ShowMagicChests => Settings.MagicChestColor.Value,
                MonsterRarity.Rare when Settings.ShowRareChests => Settings.RareChestColor.Value,
                MonsterRarity.Unique when Settings.ShowUniqueChests => Settings.UniqueChestColor.Value,
                _ => null
            };
        }

        private Color GetWhiteChestColor(Entity chestEntity)
        {
            var playerPos = GameController.Player.GridPos;
            var chestPos = chestEntity.GridPos;
            float maxDistSq = (float)Settings.MaxRenderDistance.Value * Settings.MaxRenderDistance.Value;
            float distSq = Vector2.DistanceSquared(playerPos, chestPos);
            
            float alphaMultiplier = 1.0f - (float)Math.Sqrt(distSq / maxDistSq);
            alphaMultiplier = Math.Clamp(alphaMultiplier, 0.2f, 1.0f);
            
            Color baseColor = Settings.WhiteChestColor.Value;
            return Color.FromArgb((int)(baseColor.A * alphaMultiplier), baseColor);
        }

        private (string, Color)? GetTerrainInfo(Entity e)
        {
            if (Settings.ShowSwitch && e.Path.Contains("Switch")) return (e.RenderName, Settings.SwitchColor);
            return null;
        }

        private (string, Color)? GetTransitionInfo(Entity e) => (e.RenderName, Settings.TransitionsColor);
        
        private (string, Color)? GetPoIInfo(Entity e)
        {
            if (e.Path.Contains("Expedition") || e.Path.Contains("Ritual")) return null;
            return (e.GetComponent<MinimapIcon>()?.Name ?? "PoI", Settings.PoIColor);
        }

        private (string, Color)? GetWaypointInfo(Entity e) => (e.GetComponent<MinimapIcon>()?.Name ?? "Waypoint", Settings.WaypointsColor);

        private (string, Color)? GetEssenceInfo(Entity e)
        {
            var stateMachine = e.GetComponent<StateMachine>();
            if (stateMachine?.States == null) return null;

            if (!stateMachine.States.Any(state => state.Name == "num_essences" && state.Value >= 1))
                return null;
            
            string label = "Essence";
            if (Settings.EnableDynamicLabels && e.Buffs != null)
            {
                var essenceNames = e.Buffs
                    .Select(b => b.Name.Replace("monster_aura_essence_", ""))
                    .Select(name => char.ToUpper(name[0]) + name[1..])
                    .ToList();
                if (essenceNames.Any())
                {
                    label = $"Essence: {string.Join(", ", essenceNames)}";
                }
            }
            return (label, Settings.EssenceColor);
        }

        private (string, Color)? GetShrineInfo(Entity e)
        {
            if (!e.IsTargetable) return null;
            string label = Settings.EnableDynamicLabels ? e.RenderName : "Shrine";
            return (label, Settings.ShrineColor);
        }

        private void DrawLabel(Vector2 gridPos, string text, Color color)
        {
            var screenPos = GameController.IngameState.Data.GetGridMapScreenPosition(gridPos);
            Color backgroundColor = Color.FromArgb(color.A, Color.Black);
            Graphics.DrawTextWithBackground(text, screenPos, color, FontAlign.Center, backgroundColor);
        }
    }
}