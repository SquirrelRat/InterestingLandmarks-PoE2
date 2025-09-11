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

        public override void OnLoad()
        {
            Settings.MasterToggleHotkey.OnValueChanged += () => { _renderEnabled = !_renderEnabled; };
            _stopwatch.Start();
        }

        public override void Render()
        {
            if (!_renderEnabled || !Settings.Enable || GameController.Area.CurrentArea == null || GameController.Area.CurrentArea.IsTown
                || GameController.Area.CurrentArea.IsHideout || GameController.IsLoading || !GameController.InGame
                || GameController.Game.IngameState.IngameUi.StashElement.IsVisibleLocal
                || !GameController.Game.IngameState.IngameUi.Map.LargeMap.IsVisible)
            {
                return;
            }

            var playerPos = GameController.Player.GridPos;

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
            
            if (_stopwatch.ElapsedMilliseconds > Settings.UpdateInterval)
            {
                _cachedRenderables = CollectRenderables(playerPos);
                _stopwatch.Restart();
            }

            try
            {
                if (!Settings.EnableClustering)
                {
                    foreach (var renderable in _cachedRenderables)
                    {
                        DrawLabel(renderable.Entity.GridPos, renderable.Label, renderable.Color);
                    }
                    return;
                }

                var drawnEntities = new HashSet<Entity>();
                var clusterRadiusSquared = Settings.ClusterRadius.Value * Settings.ClusterRadius.Value;
                foreach (var renderable in _cachedRenderables)
                {
                    if (drawnEntities.Contains(renderable.Entity)) continue;

                    var cluster = new List<Renderable>();
                    foreach (var otherRenderable in _cachedRenderables)
                    {
                        if (!drawnEntities.Contains(otherRenderable.Entity) && renderable.Label == otherRenderable.Label &&
                            Vector2.DistanceSquared(renderable.Entity.GridPos, otherRenderable.Entity.GridPos) < clusterRadiusSquared)
                        {
                            cluster.Add(otherRenderable);
                            drawnEntities.Add(otherRenderable.Entity);
                        }
                    }

                    if (cluster.Count > 0)
                    {
                        var avgPosX = cluster.Sum(r => r.Entity.GridPos.X) / cluster.Count;
                        var avgPosY = cluster.Sum(r => r.Entity.GridPos.Y) / cluster.Count;
                        var centerPos = new Vector2(avgPosX, avgPosY);
                        var label = cluster.Count > 1 ? $"{cluster[0].Label} (x{cluster.Count})" : cluster[0].Label;
                        DrawLabel(centerPos, label, cluster[0].Color);
                    }
                }
            }
            catch (Exception e)
            {
                LogError($"InterestingLandmarks.Render() failed: {e.Message}");
            }
        }
        
        private List<Renderable> CollectRenderables(Vector2 playerPos)
        {
            var maxRenderDistanceSquared = Settings.MaxRenderDistance.Value * Settings.MaxRenderDistance.Value;
            var newRenderables = new List<Renderable>();

            foreach (var entityList in GameController.EntityListWrapper.ValidEntitiesByType.Values)
            {
                foreach (var entity in entityList)
                {
                    if (entity.Path.Contains("RitualRuneObject"))
                    {
                        continue;
                    }
                    
                    if (entity.GridPos == Vector2.Zero || Vector2.DistanceSquared(playerPos, entity.GridPos) > maxRenderDistanceSquared)
                    {
                        continue;
                    }

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
            (string Label, Color Color)? info = null;
            switch (entity.Type)
            {
                case EntityType.Chest: if (Settings.ShowChests) info = GetChestInfo(entity); break;
                case EntityType.AreaTransition: if (Settings.ShowTransitions) info = GetTransitionInfo(entity); break;
                case EntityType.IngameIcon: if (Settings.ShowPoI) info = GetPoIInfo(entity); break;
                case EntityType.Waypoint: if (Settings.ShowWaypoints) info = GetWaypointInfo(entity); break;
                case EntityType.Monolith: if (Settings.ShowEssence) info = GetEssenceInfo(entity); break;
                case EntityType.Shrine: if (Settings.ShowShrine) info = GetShrineInfo(entity); break;
                case EntityType.Breach: if (Settings.ShowBreach) info = ("Breach", Settings.BreachColor); break;
                case EntityType.Terrain:
                    if (Settings.ShowSwitch) info = GetSwitchInfo(entity);
                    break;
            }
            
            if (info.HasValue)
            {
                return new Renderable { Entity = entity, Label = info.Value.Label, Color = info.Value.Color };
            }
            return null;
        }

        private (string, Color)? GetChestInfo(Entity e)
        {
            var chest = e.GetComponent<Chest>();
            if (chest == null || chest.IsOpened || !e.IsTargetable || e.Path.Contains("Sanctum")) return null;

            if (e.Path.Contains("Strongbox"))
            {
                if (!Settings.ShowStrongboxes) return null;
                var (label, color) = GetStrongboxDetails(e);
                if (color.HasValue) return (label, color.Value);
            }
            else
            {
                var color = GetChestColor(e);
                if (color.HasValue) return (e.RenderName, color.Value);
            }
            return null;
        }
        
        private (string label, Color? color) GetStrongboxDetails(Entity e)
        {
            string path = e.Path;
            
            if (Settings.ShowUniqueStrongbox && e.Rarity == MonsterRarity.Unique) return (e.RenderName, Settings.UniqueStrongboxColor);
            if (Settings.ShowArcanistStrongbox && path.Contains("Arcanist")) return ("Arcanist's Strongbox", Settings.ArcanistStrongboxColor);
            if (Settings.ShowCartographerStrongbox && path.Contains("Cartographer")) return ("Cartographer's Strongbox", Settings.CartographerStrongboxColor);
            if (Settings.ShowDivinerStrongbox && path.Contains("Diviner")) return ("Diviner's Strongbox", Settings.DivinerStrongboxColor);

            if (Settings.ShowOtherStrongbox)
            {
                Color? rarityColor = e.Rarity switch
                {
                    MonsterRarity.White => Settings.OtherStrongboxColor,
                    MonsterRarity.Magic => Settings.MagicChestColor,
                    MonsterRarity.Rare => Settings.RareChestColor,
                    _ => null
                };
                if (rarityColor.HasValue) return (e.RenderName, rarityColor.Value);
            }
            return (null, null);
        }

        private Color? GetChestColor(Entity chestEntity)
        {
            switch (chestEntity.Rarity)
            {
                case MonsterRarity.White:
                {
                    if (!Settings.ShowWhiteChests) return null;
                    var playerPos = GameController.Player.GridPos;
                    var chestPos = chestEntity.GridPos;
                    float distance = Vector2.Distance(playerPos, chestPos);
                    float distanceRatio = distance / Settings.MaxRenderDistance.Value;
                    float alphaMultiplier = 1.0f - distanceRatio;
                    alphaMultiplier = Math.Clamp(alphaMultiplier, 0.2f, 1.0f);
                    Color baseColor = Settings.WhiteChestColor.Value;
                    int newAlpha = (int)(baseColor.A * alphaMultiplier);
                    return Color.FromArgb(newAlpha, baseColor);
                }
                case MonsterRarity.Magic: return Settings.ShowMagicChests ? Settings.MagicChestColor.Value : null;
                case MonsterRarity.Rare: return Settings.ShowRareChests ? Settings.RareChestColor.Value : null;
                case MonsterRarity.Unique: return Settings.ShowUniqueChests ? Settings.UniqueChestColor.Value : null;
                default: return null;
            }
        }

        private (string, Color)? GetTransitionInfo(Entity e) => (e.RenderName, Settings.TransitionsColor);
        private (string, Color)? GetPoIInfo(Entity e)
        {
            if (!e.Path.Contains("Expedition") && !e.Path.Contains("Ritual"))
            {
                return (e.GetComponent<MinimapIcon>()?.Name ?? "PoI", Settings.PoIColor);
            }
            return null;
        }
        private (string, Color)? GetWaypointInfo(Entity e) => (e.GetComponent<MinimapIcon>()?.Name ?? "Waypoint", Settings.WaypointsColor);
        private (string, Color)? GetSwitchInfo(Entity e) => e.Path.Contains("Switch") ? (e.RenderName, Settings.SwitchColor) : null;
        private (string, Color)? GetEssenceInfo(Entity e)
        {
            var stateMachine = e.GetComponent<StateMachine>();
            if (stateMachine?.States == null) return null;

            bool hasEssence = false;
            foreach (var state in stateMachine.States)
            {
                if (state.Name == "num_essences" && state.Value >= 1)
                {
                    hasEssence = true;
                    break;
                }
            }
            if (!hasEssence) return null;
            
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