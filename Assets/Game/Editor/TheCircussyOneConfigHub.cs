using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.VisualTests.Editor;

public sealed class TheCircussyOneConfigHub : OdinMenuEditorWindow
{
    private ConfigHubActions actions;

    [MenuItem("Tools/The Circussy One/Config Hub")]
    private static void Open()
    {
        EnsureConfigAssets();
        var window = GetWindow<TheCircussyOneConfigHub>();
        window.titleContent = new GUIContent("The Circussy One Config Hub");
        window.minSize = new Vector2(880f, 560f);
        window.Show();
    }

    protected override OdinMenuTree BuildMenuTree()
    {
        EnsureConfigAssets();
        actions ??= new ConfigHubActions();

        var tree = new OdinMenuTree(false)
        {
            DefaultMenuStyle =
            {
                Height = 28,
                IconSize = 18
            }
        };

        tree.Add("Workflow/Everyday Actions", actions);
        tree.Add("Workflow/Designer Content Workbench", actions.ContentWorkbenchLauncher);
        tree.Add("Workflow/Create Custom Weapon", actions.CustomWeaponCreator);
        tree.AddAssetAtPath("Configs/Game Balance", TheCircussyOneSceneBuilder.GameConfigPath);
        tree.AddAssetAtPath("Configs/Run Schedule", TheCircussyOneAssetPaths.RunScheduleConfigPath);
        tree.AddAssetAtPath("Configs/World Generation", TheCircussyOneAssetPaths.RunWorldGenerationConfigPath);
        tree.AddAssetAtPath("Configs/Weapons/Cannon", TheCircussyOneSceneBuilder.CannonWeaponPath);
        tree.AddAssetAtPath("Configs/Weapons/Knife Fan", TheCircussyOneSceneBuilder.KnifeFanWeaponPath);
        tree.AddAssetAtPath("Configs/Weapons/Spotlight Bolt", TheCircussyOneSceneBuilder.SpotlightBoltWeaponPath);
        tree.AddAssetAtPath("Configs/Weapons/Fire Hoop", TheCircussyOneSceneBuilder.FireHoopWeaponPath);
        tree.AddAssetAtPath("Configs/Weapons/Juggling Ball", TheCircussyOneSceneBuilder.JugglingBallWeaponPath);
        tree.AddAllAssetsAtPath("Configs/Weapons/Custom", TheCircussyOneCustomWeaponCreator.CustomWeaponFolder, typeof(WeaponDefinition), true);
        tree.AddAssetAtPath("Configs/Weapons/Weapon Catalog", TheCircussyOneSceneBuilder.WeaponCatalogPath);
        tree.AddAssetAtPath("Configs/Items/Rubber Soles", TheCircussyOneSceneBuilder.RubberSolesItemPath);
        tree.AddAssetAtPath("Configs/Items/Safety Padding", TheCircussyOneSceneBuilder.SafetyPaddingItemPath);
        tree.AddAssetAtPath("Configs/Items/Magnet Charm", TheCircussyOneSceneBuilder.MagnetCharmItemPath);
        tree.AddAssetAtPath("Configs/Items/Lucky Coin", TheCircussyOneSceneBuilder.LuckyCoinItemPath);
        tree.AddAssetAtPath("Configs/Items/Study Notes", TheCircussyOneSceneBuilder.StudyNotesItemPath);
        tree.AddAssetAtPath("Configs/Items/Tempo Bracelet", TheCircussyOneSceneBuilder.TempoBraceletItemPath);
        tree.AddAssetAtPath("Configs/Items/Powder Flask", TheCircussyOneSceneBuilder.PowderFlaskItemPath);
        tree.AddAssetAtPath("Configs/Items/Spring Boots", TheCircussyOneSceneBuilder.SpringBootsItemPath);
        tree.AddAssetAtPath("Configs/Items/Wide Lens", TheCircussyOneSceneBuilder.WideLensItemPath);
        tree.AddAssetAtPath("Configs/Items/Long Fuse", TheCircussyOneSceneBuilder.LongFuseItemPath);
        tree.AddAssetAtPath("Configs/Items/Item Catalog", TheCircussyOneSceneBuilder.ItemCatalogPath);
        tree.AddAssetAtPath("Configs/Chests/Open Chest", TheCircussyOneSceneBuilder.OpenChestPath);
        tree.AddAssetAtPath("Configs/Chests/Locked Chest", TheCircussyOneSceneBuilder.LockedChestPath);
        tree.AddAssetAtPath("Configs/Chests/Premium Chest", TheCircussyOneSceneBuilder.PremiumChestPath);
        tree.AddAssetAtPath("Configs/Chests/Special Enemy Chest", TheCircussyOneSceneBuilder.SpecialEnemyChestPath);
        tree.AddAssetAtPath("Configs/Chests/Chest Catalog", TheCircussyOneSceneBuilder.ChestCatalogPath);
        tree.AddAssetAtPath("Configs/Ticket Deposits/Small Ticket Stack", TheCircussyOneSceneBuilder.SmallTicketStackPath);
        tree.AddAssetAtPath("Configs/Ticket Deposits/Ticket Roll", TheCircussyOneSceneBuilder.TicketRollPath);
        tree.AddAssetAtPath("Configs/Ticket Deposits/Jackpot Cache", TheCircussyOneSceneBuilder.JackpotCachePath);
        tree.AddAssetAtPath("Configs/Ticket Deposits/Ticket Deposit Catalog", TheCircussyOneSceneBuilder.TicketDepositCatalogPath);
        AddWeaponUpgrade(tree, "Cannon", "Blast Radius", TheCircussyOneAssetPaths.CannonTuningUpgradePath);
        AddWeaponUpgrade(tree, "Cannon", "Payload", TheCircussyOneAssetPaths.CannonPayloadUpgradePath);
        AddWeaponUpgrade(tree, "Cannon", "Bigger Shot", TheCircussyOneAssetPaths.CannonBiggerShotUpgradePath);
        AddWeaponUpgrade(tree, "Cannon", "Quick Fuse", TheCircussyOneAssetPaths.CannonQuickFuseUpgradePath);
        AddWeaponUpgrade(tree, "Cannon", "Fast Shot", TheCircussyOneAssetPaths.CannonFastShotUpgradePath);
        AddWeaponUpgrade(tree, "Knife Fan", "Spread", TheCircussyOneAssetPaths.KnifeFanTuningUpgradePath);
        AddWeaponUpgrade(tree, "Knife Fan", "Focus", TheCircussyOneAssetPaths.KnifeFanFocusUpgradePath);
        AddWeaponUpgrade(tree, "Knife Fan", "Honed Blades", TheCircussyOneAssetPaths.KnifeFanHonedBladesUpgradePath);
        AddWeaponUpgrade(tree, "Knife Fan", "Quick Toss", TheCircussyOneAssetPaths.KnifeFanQuickTossUpgradePath);
        AddWeaponUpgrade(tree, "Knife Fan", "Fast Blades", TheCircussyOneAssetPaths.KnifeFanFastBladesUpgradePath);
        AddWeaponUpgrade(tree, "Knife Fan", "Lingering Cut", TheCircussyOneAssetPaths.KnifeFanLingeringCutUpgradePath);
        AddWeaponUpgrade(tree, "Spotlight Bolt", "Relay", TheCircussyOneAssetPaths.SpotlightBoltTuningUpgradePath);
        AddWeaponUpgrade(tree, "Spotlight Bolt", "Bright Charge", TheCircussyOneAssetPaths.SpotlightBoltBrightChargeUpgradePath);
        AddWeaponUpgrade(tree, "Spotlight Bolt", "Quick Cue", TheCircussyOneAssetPaths.SpotlightBoltQuickCueUpgradePath);
        AddWeaponUpgrade(tree, "Spotlight Bolt", "Fast Beam", TheCircussyOneAssetPaths.SpotlightBoltFastBeamUpgradePath);
        AddWeaponUpgrade(tree, "Spotlight Bolt", "Wide Beam", TheCircussyOneAssetPaths.SpotlightBoltWideBeamUpgradePath);
        AddWeaponUpgrade(tree, "Spotlight Bolt", "Lingering Glow", TheCircussyOneAssetPaths.SpotlightBoltLingeringGlowUpgradePath);
        AddWeaponUpgrade(tree, "Fire Hoop", "Fuel", TheCircussyOneAssetPaths.FireHoopTuningUpgradePath);
        AddWeaponUpgrade(tree, "Fire Hoop", "Wider Hoop", TheCircussyOneAssetPaths.FireHoopWiderUpgradePath);
        AddWeaponUpgrade(tree, "Fire Hoop", "Extra Hoop", TheCircussyOneAssetPaths.FireHoopExtraUpgradePath);
        AddWeaponUpgrade(tree, "Fire Hoop", "Quick Spin", TheCircussyOneAssetPaths.FireHoopQuickSpinUpgradePath);
        AddWeaponUpgrade(tree, "Fire Hoop", "Bigger Flame", TheCircussyOneAssetPaths.FireHoopBiggerFlameUpgradePath);
        AddWeaponUpgrade(tree, "Juggling Ball", "Bounce", TheCircussyOneAssetPaths.JugglingBallTuningUpgradePath);
        AddWeaponUpgrade(tree, "Juggling Ball", "Timing", TheCircussyOneAssetPaths.JugglingBallTimingUpgradePath);
        AddWeaponUpgrade(tree, "Juggling Ball", "Weighted Ball", TheCircussyOneAssetPaths.JugglingBallWeightedBallUpgradePath);
        AddWeaponUpgrade(tree, "Juggling Ball", "Quick Toss", TheCircussyOneAssetPaths.JugglingBallQuickTossUpgradePath);
        AddWeaponUpgrade(tree, "Juggling Ball", "Fast Roll", TheCircussyOneAssetPaths.JugglingBallFastRollUpgradePath);
        AddWeaponUpgrade(tree, "Juggling Ball", "Bigger Ball", TheCircussyOneAssetPaths.JugglingBallBiggerBallUpgradePath);
        AddWeaponUpgrade(tree, "Juggling Ball", "Long Juggle", TheCircussyOneAssetPaths.JugglingBallLongJuggleUpgradePath);
        tree.AddAllAssetsAtPath("Configs/Upgrades/Custom", TheCircussyOneCustomWeaponCreator.CustomUpgradeFolder, typeof(UpgradeDefinition), true);
        tree.AddAssetAtPath("Configs/Upgrades/Upgrade Catalog", TheCircussyOneSceneBuilder.UpgradeCatalogPath);
        tree.AddAssetAtPath("Configs/Talents/Footwork", TheCircussyOneSceneBuilder.FootworkTalentPath);
        tree.AddAssetAtPath("Configs/Talents/Stage Stamina", TheCircussyOneSceneBuilder.StageStaminaTalentPath);
        tree.AddAssetAtPath("Configs/Talents/Tough Skin", TheCircussyOneSceneBuilder.ToughSkinTalentPath);
        tree.AddAssetAtPath("Configs/Talents/Crowd Favorite", TheCircussyOneSceneBuilder.CrowdFavoriteTalentPath);
        tree.AddAssetAtPath("Configs/Talents/Quick Hands", TheCircussyOneSceneBuilder.QuickHandsTalentPath);
        tree.AddAssetAtPath("Configs/Talents/Sharp Eye", TheCircussyOneSceneBuilder.SharpEyeTalentPath);
        tree.AddAssetAtPath("Configs/Talents/Long Reach", TheCircussyOneSceneBuilder.LongReachTalentPath);
        tree.AddAssetAtPath("Configs/Talents/Lucky Break", TheCircussyOneSceneBuilder.LuckyBreakTalentPath);
        tree.AddAssetAtPath("Configs/Talents/Bigger Props", TheCircussyOneSceneBuilder.BiggerPropsTalentPath);
        tree.AddAssetAtPath("Configs/Talents/Longer Act", TheCircussyOneSceneBuilder.LongerActTalentPath);
        tree.AddAssetAtPath("Configs/Talents/Magnetic Applause", TheCircussyOneSceneBuilder.MagneticApplauseTalentPath);
        tree.AddAssetAtPath("Configs/Talents/Springboard", TheCircussyOneSceneBuilder.SpringboardTalentPath);
        tree.AddAssetAtPath("Configs/Talents/Performers/Center Ring Toss", TheCircussyOneSceneBuilder.JugglerTalentPath);
        tree.AddAssetAtPath("Configs/Talents/Performers/Clean Compile", TheCircussyOneSceneBuilder.CleanCompileTalentPath);
        tree.AddAssetAtPath("Configs/Talents/Performers/Sample Arc", TheCircussyOneSceneBuilder.SampleArcTalentPath);
        tree.AddAssetAtPath("Configs/Talents/Performers/Debug Marker", TheCircussyOneSceneBuilder.DebugMarkerTalentPath);
        tree.AddAssetAtPath("Configs/Talents/Performers/Steady Baseline", TheCircussyOneSceneBuilder.SteadyBaselineTalentPath);
        tree.AddAssetAtPath("Configs/Talents/Performers/Capsule Toss", TheCircussyOneSceneBuilder.CappaTalentPath);
        tree.AddAssetAtPath("Configs/Talents/Performers/Rubber Grin", TheCircussyOneSceneBuilder.RubberGrinTalentPath);
        tree.AddAssetAtPath("Configs/Talents/Performers/Face Forward", TheCircussyOneSceneBuilder.FaceForwardTalentPath);
        tree.AddAssetAtPath("Configs/Talents/Performers/Capsule Pop", TheCircussyOneSceneBuilder.CapsulePopTalentPath);
        tree.AddAssetAtPath("Configs/Talents/Performers/Sample Smile", TheCircussyOneSceneBuilder.SampleSmileTalentPath);
        tree.AddAssetAtPath("Configs/Talents/Performers/Iron Posture", TheCircussyOneSceneBuilder.StrongmanTalentPath);
        tree.AddAssetAtPath("Configs/Talents/Performers/Cannon Brace", TheCircussyOneSceneBuilder.CannonBraceTalentPath);
        tree.AddAssetAtPath("Configs/Talents/Performers/Heavy Lift", TheCircussyOneSceneBuilder.HeavyLiftTalentPath);
        tree.AddAssetAtPath("Configs/Talents/Performers/Steady Hands", TheCircussyOneSceneBuilder.SteadyHandsTalentPath);
        tree.AddAssetAtPath("Configs/Talents/Performers/Bulwark Step", TheCircussyOneSceneBuilder.BulwarkStepTalentPath);
        tree.AddAssetAtPath("Configs/Talents/Performers/Aerial Tempo", TheCircussyOneSceneBuilder.AcrobatTalentPath);
        tree.AddAssetAtPath("Configs/Talents/Performers/Tightrope Footwork", TheCircussyOneSceneBuilder.TightropeFootworkTalentPath);
        tree.AddAssetAtPath("Configs/Talents/Performers/Knife Flourish", TheCircussyOneSceneBuilder.KnifeFlourishTalentPath);
        tree.AddAssetAtPath("Configs/Talents/Performers/Fast Recovery", TheCircussyOneSceneBuilder.FastRecoveryTalentPath);
        tree.AddAssetAtPath("Configs/Talents/Performers/Long Stride", TheCircussyOneSceneBuilder.LongStrideTalentPath);
        tree.AddAssetAtPath("Configs/Talents/Performers/Packed House", TheCircussyOneSceneBuilder.RingmasterTalentPath);
        tree.AddAssetAtPath("Configs/Talents/Performers/Spotlight Cue", TheCircussyOneSceneBuilder.SpotlightCueTalentPath);
        tree.AddAssetAtPath("Configs/Talents/Performers/Grand Entrance", TheCircussyOneSceneBuilder.GrandEntranceTalentPath);
        tree.AddAssetAtPath("Configs/Talents/Performers/Encore Odds", TheCircussyOneSceneBuilder.EncoreOddsTalentPath);
        tree.AddAssetAtPath("Configs/Talents/Performers/Commanding Tempo", TheCircussyOneSceneBuilder.CommandingTempoTalentPath);
        tree.AddAssetAtPath("Configs/Talents/Performers/Hoop Flow", TheCircussyOneSceneBuilder.HoopFlowTalentPath);
        tree.AddAssetAtPath("Configs/Talents/Performers/Ember Step", TheCircussyOneSceneBuilder.EmberStepTalentPath);
        tree.AddAssetAtPath("Configs/Talents/Performers/Close Circle", TheCircussyOneSceneBuilder.CloseCircleTalentPath);
        tree.AddAssetAtPath("Configs/Talents/Performers/Warm Applause", TheCircussyOneSceneBuilder.WarmApplauseTalentPath);
        tree.AddAssetAtPath("Configs/Talents/Performers/Second Circle", TheCircussyOneSceneBuilder.SecondCircleTalentPath);
        tree.AddAssetAtPath("Configs/Talents/Performers/Encore Bounce", TheCircussyOneSceneBuilder.EncoreBounceTalentPath);
        tree.AddAssetAtPath("Configs/Talents/Performers/Soft Catch", TheCircussyOneSceneBuilder.SoftCatchTalentPath);
        tree.AddAssetAtPath("Configs/Talents/Performers/Ricochet Rhythm", TheCircussyOneSceneBuilder.RicochetRhythmTalentPath);
        tree.AddAssetAtPath("Configs/Talents/Performers/Carom Line", TheCircussyOneSceneBuilder.CaromLineTalentPath);
        tree.AddAssetAtPath("Configs/Talents/Performers/Packed Balls", TheCircussyOneSceneBuilder.PackedBallsTalentPath);
        tree.AddAssetAtPath("Configs/Talents/Talent Catalog", TheCircussyOneSceneBuilder.TalentCatalogPath);
        tree.AddAssetAtPath("Configs/Performers/Devsample Sam", TheCircussyOneSceneBuilder.JugglerPerformerPath);
        tree.AddAssetAtPath("Configs/Performers/Devsample Cappa", TheCircussyOneSceneBuilder.CappaPerformerPath);
        tree.AddAssetAtPath("Configs/Performers/Strongman", TheCircussyOneSceneBuilder.StrongmanPerformerPath);
        tree.AddAssetAtPath("Configs/Performers/Acrobat", TheCircussyOneSceneBuilder.AcrobatPerformerPath);
        tree.AddAssetAtPath("Configs/Performers/Ringmaster", TheCircussyOneSceneBuilder.RingmasterPerformerPath);
        tree.AddAssetAtPath("Configs/Performers/Sola Hoop Dancer", TheCircussyOneSceneBuilder.SolaHoopDancerPerformerPath);
        tree.AddAssetAtPath("Configs/Performers/Bibi Ball Juggler", TheCircussyOneSceneBuilder.BibiBallJugglerPerformerPath);
        tree.AddAssetAtPath("Configs/Performers/Performer Catalog", TheCircussyOneSceneBuilder.PerformerCatalogPath);
        tree.AddAssetAtPath("Configs/Pickups/Blue XP Gem", TheCircussyOneSceneBuilder.BlueXpGemPath);
        tree.AddAssetAtPath("Configs/Pickups/Green XP Gem", TheCircussyOneSceneBuilder.GreenXpGemPath);
        tree.AddAssetAtPath("Configs/Pickups/Red XP Gem", TheCircussyOneSceneBuilder.RedXpGemPath);
        tree.AddAssetAtPath("Configs/Pickups/XP Gem Catalog", TheCircussyOneSceneBuilder.XpGemCatalogPath);
        tree.AddAssetAtPath("Configs/Pickups/Treat", TheCircussyOneSceneBuilder.TreatPath);
        tree.AddAssetAtPath("Configs/Pickups/Health Pickup Catalog", TheCircussyOneSceneBuilder.HealthPickupCatalogPath);
        tree.AddAssetAtPath("Configs/Healing Props/Snack Box", TheCircussyOneSceneBuilder.SnackBoxPath);
        tree.AddAssetAtPath("Configs/Healing Props/Snack Cart", TheCircussyOneSceneBuilder.SnackCartPath);
        tree.AddAssetAtPath("Configs/Healing Props/Healing Prop Catalog", TheCircussyOneSceneBuilder.HealingPropCatalogPath);
        tree.AddAssetAtPath("Configs/Enemies/Normal Enemy", TheCircussyOneSceneBuilder.NormalEnemyPath);
        tree.AddAssetAtPath("Configs/Enemies/Enemy Catalog", TheCircussyOneSceneBuilder.EnemyCatalogPath);
        tree.AddAssetAtPath("Configs/Headliners/Opening Headliner Actor", TheCircussyOneSceneBuilder.OpeningHeadlinerEnemyPath);
        tree.AddAssetAtPath("Configs/Headliners/Opening Headliner", TheCircussyOneSceneBuilder.OpeningHeadlinerPath);
        tree.AddAssetAtPath("Configs/Headliners/Headliner Catalog", TheCircussyOneSceneBuilder.HeadlinerCatalogPath);
        tree.AddAssetAtPath("Configs/Camera", TheCircussyOneSceneBuilder.CameraConfigPath);
        tree.AddAssetAtPath("Configs/Damage Feedback", TheCircussyOneSceneBuilder.DamageFeedbackVisualConfigPath);
        tree.AddAssetAtPath("Configs/Actor Motion Visuals", TheCircussyOneSceneBuilder.ActorMotionVisualConfigPath);
        tree.AddAssetAtPath("Configs/Enemy Spawn Visuals", TheCircussyOneSceneBuilder.EnemySpawnVisualConfigPath);
        tree.AddAssetAtPath("Configs/VFX Visuals", TheCircussyOneSceneBuilder.VfxVisualConfigPath);
        tree.AddAssetAtPath("Configs/Game Haptics", TheCircussyOneSceneBuilder.GameHapticsConfigPath);
        tree.AddAssetAtPath("Configs/HUD Visuals", TheCircussyOneSceneBuilder.HudVisualConfigPath);
        tree.AddAssetAtPath("Configs/XP Gain Counter", TheCircussyOneSceneBuilder.XpGainCounterVisualConfigPath);
        tree.AddAssetAtPath("Configs/World Interaction Prompt", TheCircussyOneSceneBuilder.WorldInteractionPromptVisualConfigPath);
        tree.AddAssetAtPath("Configs/Grid Visuals", TheCircussyOneSceneBuilder.GridVisualConfigPath);
        tree.AddAssetAtPath("Configs/Lighting", TheCircussyOneSceneBuilder.LightingVisualConfigPath);
        return tree;
    }

    private static void AddWeaponUpgrade(OdinMenuTree tree, string weaponName, string upgradeName, string path)
    {
        tree.AddAssetAtPath($"Configs/Upgrades/Weapons/{weaponName}/{upgradeName}", path);
    }

    private static void EnsureConfigAssets()
    {
        TheCircussyOneSceneBuilder.GetOrCreateGameConfig();
        TheCircussyOneSceneBuilder.GetOrCreateRunScheduleConfig();
        TheCircussyOneSceneBuilder.GetOrCreateRunWorldGenerationConfig();
        TheCircussyOneSceneBuilder.GetOrCreateCannonWeapon();
        TheCircussyOneSceneBuilder.GetOrCreateKnifeFanWeapon();
        TheCircussyOneSceneBuilder.GetOrCreateSpotlightBoltWeapon();
        TheCircussyOneSceneBuilder.GetOrCreateFireHoopWeapon();
        TheCircussyOneSceneBuilder.GetOrCreateJugglingBallWeapon();
        TheCircussyOneSceneBuilder.GetOrCreateWeaponCatalog();
        TheCircussyOneSceneBuilder.GetOrCreateItemCatalog();
        TheCircussyOneSceneBuilder.GetOrCreateChestCatalog();
        TheCircussyOneSceneBuilder.GetOrCreateTicketDepositCatalog();
        TheCircussyOneSceneBuilder.GetOrCreateUpgradeCatalog();
        TheCircussyOneSceneBuilder.GetOrCreateTalentCatalog();
        TheCircussyOneSceneBuilder.GetOrCreatePerformerCatalog();
        TheCircussyOneSceneBuilder.GetOrCreateXpGemCatalog();
        TheCircussyOneSceneBuilder.GetOrCreateHealthPickupCatalog();
        TheCircussyOneSceneBuilder.GetOrCreateHealingPropCatalog();
        TheCircussyOneSceneBuilder.GetOrCreateEnemyCatalog();
        TheCircussyOneSceneBuilder.GetOrCreateHeadlinerCatalog();
        TheCircussyOneSceneBuilder.GetOrCreateCameraConfig();
        TheCircussyOneSceneBuilder.GetOrCreateDamageFeedbackVisualConfig();
        TheCircussyOneSceneBuilder.GetOrCreateActorMotionVisualConfig();
        TheCircussyOneSceneBuilder.GetOrCreateEnemySpawnVisualConfig();
        TheCircussyOneSceneBuilder.GetOrCreateVfxVisualConfig();
        TheCircussyOneSceneBuilder.GetOrCreateGameHapticsConfig();
        TheCircussyOneSceneBuilder.GetOrCreateHudVisualConfig();
        TheCircussyOneSceneBuilder.GetOrCreateXpGainCounterVisualConfig();
        TheCircussyOneSceneBuilder.GetOrCreateWorldInteractionPromptVisualConfig();
        TheCircussyOneSceneBuilder.GetOrCreateGridVisualConfig();
        TheCircussyOneSceneBuilder.GetOrCreateLightingVisualConfig();
    }

    private sealed class ConfigHubActions
    {
        private bool hasDoctorSummary;
        private ConfigHubDoctorSummary doctorSummary;
        public TheCircussyOneCustomWeaponCreator CustomWeaponCreator { get; } = new();
        public ConfigHubContentWorkbenchLauncher ContentWorkbenchLauncher { get; } = new();

        [TitleGroup("Everyday Tuning")]
        [ShowInInspector, ReadOnly, MultiLineProperty(3), HideLabel]
        public string EverydayTuningGuide =>
            "Use these shortcuts for normal iteration. Content/config assets are Live Runtime; projected visuals need targeted Apply; scene reset stays in the danger zone.";

        [TitleGroup("Everyday Tuning")]
        [Button(ButtonSizes.Large), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.BalanceColor")]
        public void OpenGameBalance()
        {
            OpenAsset(TheCircussyOneAssetPaths.GameConfigPath);
        }

        [TitleGroup("Everyday Tuning")]
        [Button(ButtonSizes.Large), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.BalanceColor")]
        public void OpenRunSchedule()
        {
            OpenAsset(TheCircussyOneAssetPaths.RunScheduleConfigPath);
        }

        [TitleGroup("Everyday Tuning")]
        [Button(ButtonSizes.Large), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.BalanceColor")]
        public void OpenWorldGeneration()
        {
            OpenAsset(TheCircussyOneAssetPaths.RunWorldGenerationConfigPath);
        }

        [TitleGroup("Everyday Tuning")]
        [Button(ButtonSizes.Large), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.BalanceColor")]
        public void OpenWeaponCatalog()
        {
            OpenAsset(TheCircussyOneAssetPaths.WeaponCatalogPath);
        }

        [TitleGroup("Everyday Tuning")]
        [Button(ButtonSizes.Large), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.BalanceColor")]
        public void OpenPerformerCatalog()
        {
            OpenAsset(TheCircussyOneAssetPaths.PerformerCatalogPath);
        }

        [TitleGroup("Everyday Tuning")]
        [Button(ButtonSizes.Large), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.BalanceColor")]
        public void OpenTalentCatalog()
        {
            OpenAsset(TheCircussyOneAssetPaths.TalentCatalogPath);
        }

        [TitleGroup("Workflow Status")]
        [ShowInInspector, ReadOnly, MultiLineProperty(3), HideLabel]
        public string WorkflowStatusGuide =>
            "Status cards are informational. Open/Ping selects assets, Apply runs targeted projected visual appliers, and structural reset remains in the danger zone below.";

        [TitleGroup("Workflow Status")]
        [ShowInInspector, ListDrawerSettings(DefaultExpandedState = true, DraggableItems = false, HideAddButton = true, HideRemoveButton = true)]
        [LabelText("Config And Workflow Status")]
        public List<ConfigHubStatusRow> WorkflowStatus => ConfigHubStatusModel.BuildRows();

        [TitleGroup("Generated Asset Health")]
        [ShowInInspector, ReadOnly, MultiLineProperty(2), HideLabel]
        public string GeneratedAssetHealthGuide =>
            "Manifest-backed generated assets. Apply buttons are explicit targeted repairs; structural reset assets stay report-only here.";

        [TitleGroup("Generated Asset Health")]
        [ShowInInspector, ListDrawerSettings(DefaultExpandedState = false, DraggableItems = false, HideAddButton = true, HideRemoveButton = true)]
        [LabelText("Generated Assets")]
        public List<ConfigHubStatusRow> GeneratedAssetStatus => ConfigHubStatusModel.BuildGeneratedAssetRows();

        [TitleGroup("Workflow Status")]
        [ShowInInspector, ReadOnly, LabelText("Doctor Summary")]
        public string DoctorSummary => hasDoctorSummary ? doctorSummary.Message : "Doctor has not been run from this hub session.";

        [TitleGroup("Workflow Status")]
        [Button(ButtonSizes.Large), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.WarningColor")]
        public void RefreshDoctorSummary()
        {
            doctorSummary = ConfigHubStatusModel.GetDoctorSummary();
            hasDoctorSummary = true;
        }

        [TitleGroup("Workflow Status")]
        [Button(ButtonSizes.Large), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.WarningColor")]
        public void RunAuthoringDoctor()
        {
            TheCircussyOneAuthoringDoctorWindow.Open();
        }

        [TitleGroup("Config Assets")]
        [ShowInInspector, ReadOnly, InlineEditor(InlineEditorModes.GUIOnly)]
        [LabelText("Game")]
        public GameConfig Game => AssetDatabase.LoadAssetAtPath<GameConfig>(TheCircussyOneSceneBuilder.GameConfigPath);

        [ShowInInspector, ReadOnly, InlineEditor(InlineEditorModes.GUIOnly)]
        [LabelText("Run Schedule")]
        public RunScheduleConfig RunSchedule => AssetDatabase.LoadAssetAtPath<RunScheduleConfig>(TheCircussyOneAssetPaths.RunScheduleConfigPath);

        [ShowInInspector, ReadOnly, InlineEditor(InlineEditorModes.GUIOnly)]
        [LabelText("Weapon Catalog")]
        public WeaponCatalog WeaponCatalog => AssetDatabase.LoadAssetAtPath<WeaponCatalog>(TheCircussyOneSceneBuilder.WeaponCatalogPath);

        [ShowInInspector, ReadOnly, InlineEditor(InlineEditorModes.GUIOnly)]
        [LabelText("Item Catalog")]
        public ItemCatalog ItemCatalog => AssetDatabase.LoadAssetAtPath<ItemCatalog>(TheCircussyOneSceneBuilder.ItemCatalogPath);

        [ShowInInspector, ReadOnly, InlineEditor(InlineEditorModes.GUIOnly)]
        [LabelText("Upgrade Catalog")]
        public UpgradeCatalog UpgradeCatalog => AssetDatabase.LoadAssetAtPath<UpgradeCatalog>(TheCircussyOneSceneBuilder.UpgradeCatalogPath);

        [ShowInInspector, ReadOnly, InlineEditor(InlineEditorModes.GUIOnly)]
        [LabelText("Talent Catalog")]
        public TalentCatalog TalentCatalog => AssetDatabase.LoadAssetAtPath<TalentCatalog>(TheCircussyOneSceneBuilder.TalentCatalogPath);

        [ShowInInspector, ReadOnly, InlineEditor(InlineEditorModes.GUIOnly)]
        [LabelText("Performer Catalog")]
        public PerformerCatalog PerformerCatalog => AssetDatabase.LoadAssetAtPath<PerformerCatalog>(TheCircussyOneSceneBuilder.PerformerCatalogPath);

        [ShowInInspector, ReadOnly, InlineEditor(InlineEditorModes.GUIOnly)]
        [LabelText("XP Gem Catalog")]
        public XpGemCatalog XpGemCatalog => AssetDatabase.LoadAssetAtPath<XpGemCatalog>(TheCircussyOneSceneBuilder.XpGemCatalogPath);

        [ShowInInspector, ReadOnly, InlineEditor(InlineEditorModes.GUIOnly)]
        [LabelText("Normal Enemy")]
        public EnemyDefinition NormalEnemy => AssetDatabase.LoadAssetAtPath<EnemyDefinition>(TheCircussyOneSceneBuilder.NormalEnemyPath);

        [ShowInInspector, ReadOnly, InlineEditor(InlineEditorModes.GUIOnly)]
        [LabelText("Enemy Catalog")]
        public EnemyCatalog EnemyCatalog => AssetDatabase.LoadAssetAtPath<EnemyCatalog>(TheCircussyOneSceneBuilder.EnemyCatalogPath);

        [ShowInInspector, ReadOnly, InlineEditor(InlineEditorModes.GUIOnly)]
        [LabelText("Opening Headliner")]
        public HeadlinerDefinition OpeningHeadliner => AssetDatabase.LoadAssetAtPath<HeadlinerDefinition>(TheCircussyOneSceneBuilder.OpeningHeadlinerPath);

        [ShowInInspector, ReadOnly, InlineEditor(InlineEditorModes.GUIOnly)]
        [LabelText("Headliner Catalog")]
        public HeadlinerCatalog HeadlinerCatalog => AssetDatabase.LoadAssetAtPath<HeadlinerCatalog>(TheCircussyOneSceneBuilder.HeadlinerCatalogPath);

        [ShowInInspector, ReadOnly, InlineEditor(InlineEditorModes.GUIOnly)]
        [LabelText("Camera")]
        public CameraConfig Camera => AssetDatabase.LoadAssetAtPath<CameraConfig>(TheCircussyOneSceneBuilder.CameraConfigPath);

        [ShowInInspector, ReadOnly, InlineEditor(InlineEditorModes.GUIOnly)]
        [LabelText("Damage Feedback")]
        public DamageFeedbackVisualConfig DamageFeedback => AssetDatabase.LoadAssetAtPath<DamageFeedbackVisualConfig>(TheCircussyOneSceneBuilder.DamageFeedbackVisualConfigPath);

        [ShowInInspector, ReadOnly, InlineEditor(InlineEditorModes.GUIOnly)]
        [LabelText("Actor Motion Visuals")]
        public ActorMotionVisualConfig ActorMotionVisuals => AssetDatabase.LoadAssetAtPath<ActorMotionVisualConfig>(TheCircussyOneSceneBuilder.ActorMotionVisualConfigPath);

        [ShowInInspector, ReadOnly, InlineEditor(InlineEditorModes.GUIOnly)]
        [LabelText("Enemy Spawn Visuals")]
        public EnemySpawnVisualConfig EnemySpawnVisuals => AssetDatabase.LoadAssetAtPath<EnemySpawnVisualConfig>(TheCircussyOneSceneBuilder.EnemySpawnVisualConfigPath);

        [ShowInInspector, ReadOnly, InlineEditor(InlineEditorModes.GUIOnly)]
        [LabelText("VFX Visuals")]
        public VfxVisualConfig VfxVisuals => AssetDatabase.LoadAssetAtPath<VfxVisualConfig>(TheCircussyOneSceneBuilder.VfxVisualConfigPath);

        [ShowInInspector, ReadOnly, InlineEditor(InlineEditorModes.GUIOnly)]
        [LabelText("HUD Visuals")]
        public HudVisualConfig HudVisuals => AssetDatabase.LoadAssetAtPath<HudVisualConfig>(TheCircussyOneSceneBuilder.HudVisualConfigPath);

        [ShowInInspector, ReadOnly, InlineEditor(InlineEditorModes.GUIOnly)]
        [LabelText("XP Gain Counter")]
        public XpGainCounterVisualConfig XpGainCounter => AssetDatabase.LoadAssetAtPath<XpGainCounterVisualConfig>(TheCircussyOneSceneBuilder.XpGainCounterVisualConfigPath);

        [ShowInInspector, ReadOnly, InlineEditor(InlineEditorModes.GUIOnly)]
        [LabelText("World Interaction Prompt")]
        public WorldInteractionPromptVisualConfig WorldInteractionPrompt => AssetDatabase.LoadAssetAtPath<WorldInteractionPromptVisualConfig>(TheCircussyOneSceneBuilder.WorldInteractionPromptVisualConfigPath);

        [TitleGroup("Workflow Guide")]
        [ShowInInspector, ReadOnly, MultiLineProperty(4), HideLabel]
        public string ConfigWorkflowGuide =>
            "Live Runtime: tune config values, then press Play or restart Play Mode for start-time values.\n" +
            "Projected Visual: use targeted Apply; no scene rebuild.\n" +
            "Structural Reset: rebuild only when generated scene topology, prefabs, layers, UI, or DI wiring need recovery.";

        [TitleGroup("Everyday Workflow")]
        [Button(ButtonSizes.Large), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.VisualColor")]
        public void ApplyProjectedVisualConfigs()
        {
            TheCircussyOneGridVisualApplier.ApplyNow();
            TheCircussyOneLightingVisualApplier.ApplyNow();
            TheCircussyOneDamageFeedbackVisualApplier.ApplyNow();
            TheCircussyOneXpGainCounterVisualApplier.ApplyNow();
            TheCircussyOneWorldInteractionPromptVisualApplier.ApplyNow();
            TheCircussyOneEnemySpawnVisualApplier.ApplyNow();
            TheCircussyOneVfxVisualApplier.ApplyNow();
            TheCircussyOneHudVisualApplier.ApplyNow();
        }

        [TitleGroup("Everyday Workflow")]
        [Button(ButtonSizes.Large), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.PlayerColor")]
        public void PreviewHudInOpenScene()
        {
            TheCircussyOneHudVisualApplier.PreviewOpenScene(HudVisuals);
        }

        [TitleGroup("Everyday Workflow")]
        [Button(ButtonSizes.Large), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.WarningColor")]
        public void ClearHudPreview()
        {
            TheCircussyOneHudVisualApplier.ClearPreviewOpenScene();
        }

        [TitleGroup("Everyday Workflow")]
        [Button(ButtonSizes.Large), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.VisualColor")]
        public void GenerateVisualTestLab()
        {
            VisualTestLabBuilder.GenerateAllScenes();
        }

        [TitleGroup("Everyday Workflow")]
        [Button("Open Visual Almanac", ButtonSizes.Large), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.PlayerColor")]
        public void OpenScenarioBrowser()
        {
            VisualTestLabScenarioBrowserWindow.Open();
        }

        [TitleGroup("Everyday Workflow")]
        [Button(ButtonSizes.Large), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.PlayerColor")]
        public void OpenRunStatsDebugger()
        {
            RunStatsDebugWindow.Open();
        }

        [TitleGroup("Everyday Workflow")]
        [Button(ButtonSizes.Large), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.PlayerColor")]
        public void OpenXpPacingCalculator()
        {
            XpPacingCalculatorWindow.Open();
        }

        [TitleGroup("Everyday Workflow")]
        [Button(ButtonSizes.Large), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.VisualColor")]
        public void GenerateAuthoringShowroom()
        {
            AuthoringShowroomBuilder.GenerateAuthoringShowroom();
        }

        [TitleGroup("Everyday Workflow")]
        [Button(ButtonSizes.Large), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.PlayerColor")]
        public void OpenAuthoringShowroom()
        {
            AuthoringShowroomBuilder.OpenAuthoringShowroom();
        }

        [TitleGroup("Everyday Workflow")]
        [Button(ButtonSizes.Large), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.VisualColor")]
        public void ValidateVisualTestLab()
        {
            VisualTestLabBuilder.ValidateReferences();
        }

        [TitleGroup("Verification")]
        [Button(ButtonSizes.Large), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.PlayerColor")]
        public void OpenVerificationDashboard()
        {
            TheCircussyOneVerificationDashboardWindow.Open();
        }

        [TitleGroup("Verification")]
        [Button(ButtonSizes.Large), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.PlayerColor")]
        public void RunEditModeTests()
        {
            VerificationTestRunner.RunEditModeTests();
        }

        [TitleGroup("Verification")]
        [Button(ButtonSizes.Large), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.PlayerColor")]
        public void RunEditModeCoverage()
        {
            VerificationTestRunner.RunEditModeCoverage();
        }

        [TitleGroup("Verification")]
        [Button(ButtonSizes.Large), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.PlayerColor")]
        public void RefreshCoverageStatus()
        {
            VerificationTestRunner.RefreshCoverageStatus();
        }

        [TitleGroup("Verification")]
        [Button(ButtonSizes.Large), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.PlayerColor")]
        public void RunPlayModeVisualSmokeTest()
        {
            VerificationTestRunner.RunPlayModeVisualSmokeTests();
        }

        [TitleGroup("Visual Test Captures")]
        [Button(ButtonSizes.Large), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.WarningColor")]
        public void CaptureDamageNumbersTest()
        {
            VisualTestLabBuilder.RunDamageNumbersTest();
        }

        [TitleGroup("Scene Access")]
        [Button(ButtonSizes.Medium)]
        public void OpenPrototypeScene()
        {
            EditorSceneManager.OpenScene("Assets/Game/Scenes/TheCircussyOne.unity", OpenSceneMode.Single);
        }

        [TitleGroup("Scene Access")]
        [Button(ButtonSizes.Medium)]
        public void SelectConfigFolder()
        {
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>("Assets/Game/ScriptableObjects");
            EditorGUIUtility.PingObject(Selection.activeObject);
        }

        [TitleGroup("Generated Scene Reset")]
        [ShowInInspector, ReadOnly, MultiLineProperty(3), HideLabel]
        public string ResetWarning =>
            "Danger zone: rebuild resets generated scene structure, prefabs, runtime roots, HUD object, and DI wiring.\n" +
            "Use this for structural recovery or after code changes that alter generated topology, not for balance or lighting tweaks.";

        [TitleGroup("Generated Scene Reset")]
        [Button(ButtonSizes.Large), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.WarningColor")]
        public void ResetGeneratedPrototypeScene()
        {
            TheCircussyOneSceneBuilder.RebuildPrototypeScene();
        }

        private static void OpenAsset(string path)
        {
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);
            if (asset == null)
            {
                return;
            }

            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
            AssetDatabase.OpenAsset(asset);
        }

    }
}
