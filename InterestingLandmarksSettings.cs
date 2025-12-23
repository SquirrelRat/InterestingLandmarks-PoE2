using ExileCore2.Shared.Attributes;
using ExileCore2.Shared.Interfaces;
using ExileCore2.Shared.Nodes;
using System.Drawing;
using System.Windows.Forms;

namespace InterestingLandmarks
{
    public class InterestingLandmarksSettings : ISettings
    {
        public ToggleNode Enable { get; set; } = new ToggleNode(false);

        [Menu("General Settings", 100, CollapsedByDefault = false)]
        public EmptyNode GeneralSettings { get; set; } = new();

        [Menu("Master Toggle Hotkey", "A hotkey to quickly enable/disable all rendering.", parentIndex = 100)]
        public HotkeyNodeV2 MasterToggleHotkey { get; set; } = new HotkeyNodeV2(Keys.None);

        [Menu("Max Render Distance", "Landmarks beyond this distance from the player will not be shown.", parentIndex = 100)]
        public RangeNode<int> MaxRenderDistance { get; set; } = new RangeNode<int>(150, 50, 500);
        
        [Menu("Update Interval", "The time in milliseconds between landmark scans. Lower values are more responsive but use more resources.", parentIndex = 100)]
        public RangeNode<int> UpdateInterval { get; set; } = new RangeNode<int>(250, 100, 1000);

        [Menu("Enable Dynamic Labels", "Show more detailed labels (e.g., Essence types).", parentIndex = 100)]
        public ToggleNode EnableDynamicLabels { get; set; } = new ToggleNode(true);

        [Menu("Enable Clustering", "Group nearby landmarks of the same type into a single label.", parentIndex = 100)]
        public ToggleNode EnableClustering { get; set; } = new ToggleNode(true);

        [Menu("Cluster Radius", "How close landmarks need to be to get grouped together.", parentIndex = 100)]
        public RangeNode<int> ClusterRadius { get; set; } = new RangeNode<int>(30, 5, 100);


        [Menu("Chests", 200, CollapsedByDefault = true)]
        public EmptyNode ChestSettings { get; set; } = new();

        [Menu("Show Chests", parentIndex = 200)]
        public ToggleNode ShowChests { get; set; } = new ToggleNode(true);

        [Menu("White Chests", parentIndex = 200)]
        public ToggleNode ShowWhiteChests { get; set; } = new ToggleNode(true);
        [Menu("Magic Chests", parentIndex = 200)]
        public ToggleNode ShowMagicChests { get; set; } = new ToggleNode(true);
        [Menu("Rare Chests", parentIndex = 200)]
        public ToggleNode ShowRareChests { get; set; } = new ToggleNode(true);
        [Menu("Unique Chests", parentIndex = 200)]
        public ToggleNode ShowUniqueChests { get; set; } = new ToggleNode(true);

        [Menu("White Chest Color", parentIndex = 200)]
        public ColorNode WhiteChestColor { get; set; } = new ColorNode(Color.White);
        [Menu("Magic Chest Color", parentIndex = 200)]
        public ColorNode MagicChestColor { get; set; } = new ColorNode(Color.Blue);
        [Menu("Rare Chest Color", parentIndex = 200)]
        public ColorNode RareChestColor { get; set; } = new ColorNode(Color.Yellow);
        [Menu("Unique Chest Color", parentIndex = 200)]
        public ColorNode UniqueChestColor { get; set; } = new ColorNode(Color.Orange);

        [Menu("Strongboxes", 201, parentIndex = 200)]
        public EmptyNode StrongboxSettings { get; set; } = new();
        [Menu("Show Strongboxes", parentIndex = 201)]
        public ToggleNode ShowStrongboxes { get; set; } = new ToggleNode(true);
        [Menu("Arcanist's", parentIndex = 201)]
        public ToggleNode ShowArcanistStrongbox { get; set; } = new ToggleNode(true);
        [Menu("Cartographer's", parentIndex = 201)]
        public ToggleNode ShowCartographerStrongbox { get; set; } = new ToggleNode(true);
        [Menu("Diviner's", parentIndex = 201)]
        public ToggleNode ShowDivinerStrongbox { get; set; } = new ToggleNode(true);
        [Menu("Unique", parentIndex = 201)]
        public ToggleNode ShowUniqueStrongbox { get; set; } = new ToggleNode(true);
        [Menu("Other", parentIndex = 201)]
        public ToggleNode ShowOtherStrongbox { get; set; } = new ToggleNode(true);
        [Menu("Arcanist's Color", parentIndex = 201)]
        public ColorNode ArcanistStrongboxColor { get; set; } = new ColorNode(Color.Cyan);
        [Menu("Cartographer's Color", parentIndex = 201)]
        public ColorNode CartographerStrongboxColor { get; set; } = new ColorNode(Color.Orange);
        [Menu("Diviner's Color", parentIndex = 201)]
        public ColorNode DivinerStrongboxColor { get; set; } = new ColorNode(Color.Fuchsia);
        [Menu("Unique Color", parentIndex = 201)]
        public ColorNode UniqueStrongboxColor { get; set; } = new ColorNode(Color.OrangeRed);
        [Menu("Other Color", parentIndex = 201)]
        public ColorNode OtherStrongboxColor { get; set; } = new ColorNode(Color.LightGray);


        [Menu("Area Transitions", 300, CollapsedByDefault = true)]
        public EmptyNode TransitionSettings { get; set; } = new();
        [Menu("Enable", parentIndex = 300)]
        public ToggleNode ShowTransitions { get; set; } = new ToggleNode(true);
        [Menu("Color", parentIndex = 300)]
        public ColorNode TransitionsColor { get; set; } = new ColorNode(Color.White);

        [Menu("Waypoints", 400, CollapsedByDefault = true)]
        public EmptyNode WaypointSettings { get; set; } = new();
        [Menu("Enable", parentIndex = 400)]
        public ToggleNode ShowWaypoints { get; set; } = new ToggleNode(true);
        [Menu("Color", parentIndex = 400)]
        public ColorNode WaypointsColor { get; set; } = new ColorNode(Color.LightBlue);

        [Menu("Points of Interest", 500, CollapsedByDefault = true)]
        public EmptyNode PoiSettings { get; set; } = new();
        [Menu("Enable", parentIndex = 500)]
        public ToggleNode ShowPoI { get; set; } = new ToggleNode(true);
        [Menu("Color", parentIndex = 500)]
        public ColorNode PoIColor { get; set; } = new ColorNode(Color.LightGreen);

        [Menu("Essences", 600, CollapsedByDefault = true)]
        public EmptyNode EssenceSettings { get; set; } = new();
        [Menu("Enable", parentIndex = 600)]
        public ToggleNode ShowEssence { get; set; } = new ToggleNode(true);
        [Menu("Color", parentIndex = 600)]
        public ColorNode EssenceColor { get; set; } = new ColorNode(Color.Pink);

        [Menu("Switches", 700, CollapsedByDefault = true)]
        public EmptyNode SwitchSettings { get; set; } = new();
        [Menu("Enable", parentIndex = 700)]
        public ToggleNode ShowSwitch { get; set; } = new ToggleNode(true);
        [Menu("Color", parentIndex = 700)]
        public ColorNode SwitchColor { get; set; } = new ColorNode(Color.Red);

        [Menu("Shrines", 800, CollapsedByDefault = true)]
        public EmptyNode ShrineSettings { get; set; } = new();
        [Menu("Enable", parentIndex = 800)]
        public ToggleNode ShowShrine { get; set; } = new ToggleNode(true);
        [Menu("Color", parentIndex = 800)]
        public ColorNode ShrineColor { get; set; } = new ColorNode(Color.Yellow);

        [Menu("Breaches", 900, CollapsedByDefault = true)]
        public EmptyNode BreachSettings { get; set; } = new();
        [Menu("Enable", parentIndex = 900)]
        public ToggleNode ShowBreach { get; set; } = new ToggleNode(true);
        [Menu("Color", parentIndex = 900)]
        public ColorNode BreachColor { get; set; } = new ColorNode(Color.Purple);

        [Menu("Rituals", 1000, CollapsedByDefault = true)]
        public EmptyNode RitualSettings { get; set; } = new();
        [Menu("Enable", parentIndex = 1000)]
        public ToggleNode ShowRituals { get; set; } = new ToggleNode(true);
        [Menu("Color", parentIndex = 1000)]
        public ColorNode RitualColor { get; set; } = new ColorNode(Color.OrangeRed);
    }
}