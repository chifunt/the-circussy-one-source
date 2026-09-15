using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using TheCircussyOne.Config;
using TheCircussyOne.VisualTests.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public enum ConfigHubStatusSeverity
{
    Ok,
    Warning,
    Error
}

public readonly struct ConfigHubDoctorSummary
{
    public ConfigHubDoctorSummary(int errorCount, int warningCount, int infoCount)
    {
        ErrorCount = errorCount;
        WarningCount = warningCount;
        InfoCount = infoCount;
    }

    public int ErrorCount { get; }
    public int WarningCount { get; }
    public int InfoCount { get; }
    public int TotalCount => ErrorCount + WarningCount + InfoCount;

    public ConfigHubStatusSeverity Severity =>
        ErrorCount > 0 ? ConfigHubStatusSeverity.Error :
        WarningCount > 0 ? ConfigHubStatusSeverity.Warning :
        ConfigHubStatusSeverity.Ok;

    public string Message => $"Doctor: {ErrorCount} errors, {WarningCount} warnings, {InfoCount} info";
}

public sealed class ConfigHubStatusRow
{
    private readonly Action applyAction;

    public ConfigHubStatusRow(
        string displayName,
        ConfigWorkflowDescriptor workflow,
        string assetPath,
        bool exists,
        string statusMessage,
        ConfigHubStatusSeverity severity,
        Action applyAction = null)
    {
        DisplayName = displayName;
        Workflow = workflow;
        AssetPath = assetPath;
        Exists = exists;
        StatusMessage = statusMessage;
        Severity = severity;
        this.applyAction = applyAction;
    }

    [ShowInInspector, ReadOnly, LabelText("Name"), PropertyOrder(0), GUIColor(nameof(StatusColor))]
    public string DisplayName { get; }

    public ConfigWorkflowDescriptor Workflow { get; }

    [ShowInInspector, ReadOnly, LabelText("Workflow"), PropertyOrder(1)]
    public string WorkflowLabel => Workflow.Label;

    public ConfigWorkflowKind WorkflowKind => Workflow.Kind;

    [ShowInInspector, ReadOnly, LabelText("Path"), PropertyOrder(2)]
    public string AssetPath { get; }

    [ShowInInspector, ReadOnly, LabelText("Status"), PropertyOrder(3), MultiLineProperty(2)]
    public string StatusMessage { get; }

    public bool Exists { get; }
    public ConfigHubStatusSeverity Severity { get; }
    public bool HasApplyAction => applyAction != null;

    [Button(ButtonSizes.Small), HorizontalGroup("Actions"), PropertyOrder(10)]
    public void Open()
    {
        UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AssetPath);
        if (asset == null)
        {
            Debug.LogWarning($"Could not open missing asset: {AssetPath}");
            return;
        }

        if (AssetPath.EndsWith(".unity", StringComparison.OrdinalIgnoreCase))
        {
            EditorSceneManager.OpenScene(AssetPath, OpenSceneMode.Single);
            return;
        }

        AssetDatabase.OpenAsset(asset);
    }

    [Button(ButtonSizes.Small), HorizontalGroup("Actions"), PropertyOrder(11)]
    public void Ping()
    {
        UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AssetPath);
        if (asset == null)
        {
            Debug.LogWarning($"Could not ping missing asset: {AssetPath}");
            return;
        }

        Selection.activeObject = asset;
        EditorGUIUtility.PingObject(asset);
    }

    [Button(ButtonSizes.Small), HorizontalGroup("Actions"), ShowIf(nameof(HasApplyAction)), GUIColor(nameof(ApplyButtonColor)), PropertyOrder(12)]
    public void Apply()
    {
        ExecuteApply();
    }

    public void ExecuteApply()
    {
        if (applyAction == null)
        {
            throw new InvalidOperationException($"{DisplayName} does not have a projected visual apply action.");
        }

        applyAction.Invoke();
    }

    private Color StatusColor()
    {
        return Severity switch
        {
            ConfigHubStatusSeverity.Error => ConfigInspectorStyle.CombatColor,
            ConfigHubStatusSeverity.Warning => ConfigInspectorStyle.WarningColor,
            _ => WorkflowKind switch
            {
                ConfigWorkflowKind.LiveRuntime => ConfigInspectorStyle.PlayerColor,
                ConfigWorkflowKind.ProjectedVisual => ConfigInspectorStyle.VisualColor,
                ConfigWorkflowKind.StructuralReset => ConfigInspectorStyle.WarningColor,
                _ => Color.white
            }
        };
    }

    private Color ApplyButtonColor()
    {
        return ConfigInspectorStyle.VisualColor;
    }
}

public static class ConfigHubStatusModel
{
    public static List<ConfigHubStatusRow> BuildRows()
    {
        return new List<ConfigHubStatusRow>
        {
            BuildRow("Game Balance", TheCircussyOneAssetPaths.GameConfigPath, ConfigWorkflowCatalog.GameConfig),
            BuildRow("Run Schedule", TheCircussyOneAssetPaths.RunScheduleConfigPath, ConfigWorkflowCatalog.RunScheduleConfig),
            BuildRow("World Generation", TheCircussyOneAssetPaths.RunWorldGenerationConfigPath, ConfigWorkflowCatalog.RunWorldGenerationConfig),
            BuildRow("Cannon Weapon", TheCircussyOneAssetPaths.CannonWeaponPath, ConfigWorkflowCatalog.WeaponDefinition),
            BuildRow("Knife Fan Weapon", TheCircussyOneAssetPaths.KnifeFanWeaponPath, ConfigWorkflowCatalog.WeaponDefinition),
            BuildRow("Spotlight Bolt Weapon", TheCircussyOneAssetPaths.SpotlightBoltWeaponPath, ConfigWorkflowCatalog.WeaponDefinition),
            BuildRow("Fire Hoop Weapon", TheCircussyOneAssetPaths.FireHoopWeaponPath, ConfigWorkflowCatalog.WeaponDefinition),
            BuildRow("Juggling Ball Weapon", TheCircussyOneAssetPaths.JugglingBallWeaponPath, ConfigWorkflowCatalog.WeaponDefinition),
            BuildRow("Weapon Catalog", TheCircussyOneAssetPaths.WeaponCatalogPath, ConfigWorkflowCatalog.WeaponCatalog),
            BuildRow("Rubber Soles Item", TheCircussyOneAssetPaths.RubberSolesItemPath, ConfigWorkflowCatalog.ItemDefinition),
            BuildRow("Safety Padding Item", TheCircussyOneAssetPaths.SafetyPaddingItemPath, ConfigWorkflowCatalog.ItemDefinition),
            BuildRow("Magnet Charm Item", TheCircussyOneAssetPaths.MagnetCharmItemPath, ConfigWorkflowCatalog.ItemDefinition),
            BuildRow("Lucky Coin Item", TheCircussyOneAssetPaths.LuckyCoinItemPath, ConfigWorkflowCatalog.ItemDefinition),
            BuildRow("Study Notes Item", TheCircussyOneAssetPaths.StudyNotesItemPath, ConfigWorkflowCatalog.ItemDefinition),
            BuildRow("Tempo Bracelet Item", TheCircussyOneAssetPaths.TempoBraceletItemPath, ConfigWorkflowCatalog.ItemDefinition),
            BuildRow("Powder Flask Item", TheCircussyOneAssetPaths.PowderFlaskItemPath, ConfigWorkflowCatalog.ItemDefinition),
            BuildRow("Spring Boots Item", TheCircussyOneAssetPaths.SpringBootsItemPath, ConfigWorkflowCatalog.ItemDefinition),
            BuildRow("Wide Lens Item", TheCircussyOneAssetPaths.WideLensItemPath, ConfigWorkflowCatalog.ItemDefinition),
            BuildRow("Long Fuse Item", TheCircussyOneAssetPaths.LongFuseItemPath, ConfigWorkflowCatalog.ItemDefinition),
            BuildRow("Item Catalog", TheCircussyOneAssetPaths.ItemCatalogPath, ConfigWorkflowCatalog.ItemCatalog),
            BuildRow("Open Chest", TheCircussyOneAssetPaths.OpenChestPath, ConfigWorkflowCatalog.ChestDefinition),
            BuildRow("Locked Chest", TheCircussyOneAssetPaths.LockedChestPath, ConfigWorkflowCatalog.ChestDefinition),
            BuildRow("Premium Chest", TheCircussyOneAssetPaths.PremiumChestPath, ConfigWorkflowCatalog.ChestDefinition),
            BuildRow("Special Enemy Chest", TheCircussyOneAssetPaths.SpecialEnemyChestPath, ConfigWorkflowCatalog.ChestDefinition),
            BuildRow("Chest Catalog", TheCircussyOneAssetPaths.ChestCatalogPath, ConfigWorkflowCatalog.ChestCatalog),
            BuildRow("Small Ticket Stack", TheCircussyOneAssetPaths.SmallTicketStackPath, ConfigWorkflowCatalog.TicketDepositDefinition),
            BuildRow("Ticket Roll", TheCircussyOneAssetPaths.TicketRollPath, ConfigWorkflowCatalog.TicketDepositDefinition),
            BuildRow("Jackpot Cache", TheCircussyOneAssetPaths.JackpotCachePath, ConfigWorkflowCatalog.TicketDepositDefinition),
            BuildRow("Ticket Deposit Catalog", TheCircussyOneAssetPaths.TicketDepositCatalogPath, ConfigWorkflowCatalog.TicketDepositCatalog),
            BuildRow("Cannon Blast Radius Upgrade", TheCircussyOneAssetPaths.CannonTuningUpgradePath, ConfigWorkflowCatalog.UpgradeDefinition),
            BuildRow("Cannon Payload Upgrade", TheCircussyOneAssetPaths.CannonPayloadUpgradePath, ConfigWorkflowCatalog.UpgradeDefinition),
            BuildRow("Cannon Bigger Shot Upgrade", TheCircussyOneAssetPaths.CannonBiggerShotUpgradePath, ConfigWorkflowCatalog.UpgradeDefinition),
            BuildRow("Cannon Quick Fuse Upgrade", TheCircussyOneAssetPaths.CannonQuickFuseUpgradePath, ConfigWorkflowCatalog.UpgradeDefinition),
            BuildRow("Cannon Fast Shot Upgrade", TheCircussyOneAssetPaths.CannonFastShotUpgradePath, ConfigWorkflowCatalog.UpgradeDefinition),
            BuildRow("Knife Fan Spread Upgrade", TheCircussyOneAssetPaths.KnifeFanTuningUpgradePath, ConfigWorkflowCatalog.UpgradeDefinition),
            BuildRow("Knife Fan Focus Upgrade", TheCircussyOneAssetPaths.KnifeFanFocusUpgradePath, ConfigWorkflowCatalog.UpgradeDefinition),
            BuildRow("Knife Fan Honed Blades Upgrade", TheCircussyOneAssetPaths.KnifeFanHonedBladesUpgradePath, ConfigWorkflowCatalog.UpgradeDefinition),
            BuildRow("Knife Fan Quick Toss Upgrade", TheCircussyOneAssetPaths.KnifeFanQuickTossUpgradePath, ConfigWorkflowCatalog.UpgradeDefinition),
            BuildRow("Knife Fan Fast Blades Upgrade", TheCircussyOneAssetPaths.KnifeFanFastBladesUpgradePath, ConfigWorkflowCatalog.UpgradeDefinition),
            BuildRow("Knife Fan Lingering Cut Upgrade", TheCircussyOneAssetPaths.KnifeFanLingeringCutUpgradePath, ConfigWorkflowCatalog.UpgradeDefinition),
            BuildRow("Spotlight Bolt Relay Upgrade", TheCircussyOneAssetPaths.SpotlightBoltTuningUpgradePath, ConfigWorkflowCatalog.UpgradeDefinition),
            BuildRow("Spotlight Bolt Bright Charge Upgrade", TheCircussyOneAssetPaths.SpotlightBoltBrightChargeUpgradePath, ConfigWorkflowCatalog.UpgradeDefinition),
            BuildRow("Spotlight Bolt Quick Cue Upgrade", TheCircussyOneAssetPaths.SpotlightBoltQuickCueUpgradePath, ConfigWorkflowCatalog.UpgradeDefinition),
            BuildRow("Spotlight Bolt Fast Beam Upgrade", TheCircussyOneAssetPaths.SpotlightBoltFastBeamUpgradePath, ConfigWorkflowCatalog.UpgradeDefinition),
            BuildRow("Spotlight Bolt Wide Beam Upgrade", TheCircussyOneAssetPaths.SpotlightBoltWideBeamUpgradePath, ConfigWorkflowCatalog.UpgradeDefinition),
            BuildRow("Spotlight Bolt Lingering Glow Upgrade", TheCircussyOneAssetPaths.SpotlightBoltLingeringGlowUpgradePath, ConfigWorkflowCatalog.UpgradeDefinition),
            BuildRow("Fire Hoop Fuel Upgrade", TheCircussyOneAssetPaths.FireHoopTuningUpgradePath, ConfigWorkflowCatalog.UpgradeDefinition),
            BuildRow("Fire Hoop Wider Upgrade", TheCircussyOneAssetPaths.FireHoopWiderUpgradePath, ConfigWorkflowCatalog.UpgradeDefinition),
            BuildRow("Fire Hoop Extra Upgrade", TheCircussyOneAssetPaths.FireHoopExtraUpgradePath, ConfigWorkflowCatalog.UpgradeDefinition),
            BuildRow("Fire Hoop Quick Spin Upgrade", TheCircussyOneAssetPaths.FireHoopQuickSpinUpgradePath, ConfigWorkflowCatalog.UpgradeDefinition),
            BuildRow("Fire Hoop Bigger Flame Upgrade", TheCircussyOneAssetPaths.FireHoopBiggerFlameUpgradePath, ConfigWorkflowCatalog.UpgradeDefinition),
            BuildRow("Juggling Ball Bounce Upgrade", TheCircussyOneAssetPaths.JugglingBallTuningUpgradePath, ConfigWorkflowCatalog.UpgradeDefinition),
            BuildRow("Juggling Ball Timing Upgrade", TheCircussyOneAssetPaths.JugglingBallTimingUpgradePath, ConfigWorkflowCatalog.UpgradeDefinition),
            BuildRow("Juggling Ball Weighted Ball Upgrade", TheCircussyOneAssetPaths.JugglingBallWeightedBallUpgradePath, ConfigWorkflowCatalog.UpgradeDefinition),
            BuildRow("Juggling Ball Quick Toss Upgrade", TheCircussyOneAssetPaths.JugglingBallQuickTossUpgradePath, ConfigWorkflowCatalog.UpgradeDefinition),
            BuildRow("Juggling Ball Fast Roll Upgrade", TheCircussyOneAssetPaths.JugglingBallFastRollUpgradePath, ConfigWorkflowCatalog.UpgradeDefinition),
            BuildRow("Juggling Ball Bigger Ball Upgrade", TheCircussyOneAssetPaths.JugglingBallBiggerBallUpgradePath, ConfigWorkflowCatalog.UpgradeDefinition),
            BuildRow("Juggling Ball Long Juggle Upgrade", TheCircussyOneAssetPaths.JugglingBallLongJuggleUpgradePath, ConfigWorkflowCatalog.UpgradeDefinition),
            BuildRow("Upgrade Catalog", TheCircussyOneAssetPaths.UpgradeCatalogPath, ConfigWorkflowCatalog.UpgradeCatalog),
            BuildRow("Footwork Talent", TheCircussyOneAssetPaths.FootworkTalentPath, ConfigWorkflowCatalog.TalentDefinition),
            BuildRow("Stage Stamina Talent", TheCircussyOneAssetPaths.StageStaminaTalentPath, ConfigWorkflowCatalog.TalentDefinition),
            BuildRow("Tough Skin Talent", TheCircussyOneAssetPaths.ToughSkinTalentPath, ConfigWorkflowCatalog.TalentDefinition),
            BuildRow("Crowd Favorite Talent", TheCircussyOneAssetPaths.CrowdFavoriteTalentPath, ConfigWorkflowCatalog.TalentDefinition),
            BuildRow("Quick Hands Talent", TheCircussyOneAssetPaths.QuickHandsTalentPath, ConfigWorkflowCatalog.TalentDefinition),
            BuildRow("Sharp Eye Talent", TheCircussyOneAssetPaths.SharpEyeTalentPath, ConfigWorkflowCatalog.TalentDefinition),
            BuildRow("Long Reach Talent", TheCircussyOneAssetPaths.LongReachTalentPath, ConfigWorkflowCatalog.TalentDefinition),
            BuildRow("Lucky Break Talent", TheCircussyOneAssetPaths.LuckyBreakTalentPath, ConfigWorkflowCatalog.TalentDefinition),
            BuildRow("Bigger Props Talent", TheCircussyOneAssetPaths.BiggerPropsTalentPath, ConfigWorkflowCatalog.TalentDefinition),
            BuildRow("Longer Act Talent", TheCircussyOneAssetPaths.LongerActTalentPath, ConfigWorkflowCatalog.TalentDefinition),
            BuildRow("Magnetic Applause Talent", TheCircussyOneAssetPaths.MagneticApplauseTalentPath, ConfigWorkflowCatalog.TalentDefinition),
            BuildRow("Springboard Talent", TheCircussyOneAssetPaths.SpringboardTalentPath, ConfigWorkflowCatalog.TalentDefinition),
            BuildRow("Center Ring Toss Talent", TheCircussyOneAssetPaths.JugglerTalentPath, ConfigWorkflowCatalog.TalentDefinition),
            BuildRow("Clean Compile Talent", TheCircussyOneAssetPaths.CleanCompileTalentPath, ConfigWorkflowCatalog.TalentDefinition),
            BuildRow("Sample Arc Talent", TheCircussyOneAssetPaths.SampleArcTalentPath, ConfigWorkflowCatalog.TalentDefinition),
            BuildRow("Debug Marker Talent", TheCircussyOneAssetPaths.DebugMarkerTalentPath, ConfigWorkflowCatalog.TalentDefinition),
            BuildRow("Steady Baseline Talent", TheCircussyOneAssetPaths.SteadyBaselineTalentPath, ConfigWorkflowCatalog.TalentDefinition),
            BuildRow("Capsule Toss Talent", TheCircussyOneAssetPaths.CappaTalentPath, ConfigWorkflowCatalog.TalentDefinition),
            BuildRow("Rubber Grin Talent", TheCircussyOneAssetPaths.RubberGrinTalentPath, ConfigWorkflowCatalog.TalentDefinition),
            BuildRow("Face Forward Talent", TheCircussyOneAssetPaths.FaceForwardTalentPath, ConfigWorkflowCatalog.TalentDefinition),
            BuildRow("Capsule Pop Talent", TheCircussyOneAssetPaths.CapsulePopTalentPath, ConfigWorkflowCatalog.TalentDefinition),
            BuildRow("Sample Smile Talent", TheCircussyOneAssetPaths.SampleSmileTalentPath, ConfigWorkflowCatalog.TalentDefinition),
            BuildRow("Iron Posture Talent", TheCircussyOneAssetPaths.StrongmanTalentPath, ConfigWorkflowCatalog.TalentDefinition),
            BuildRow("Cannon Brace Talent", TheCircussyOneAssetPaths.CannonBraceTalentPath, ConfigWorkflowCatalog.TalentDefinition),
            BuildRow("Heavy Lift Talent", TheCircussyOneAssetPaths.HeavyLiftTalentPath, ConfigWorkflowCatalog.TalentDefinition),
            BuildRow("Steady Hands Talent", TheCircussyOneAssetPaths.SteadyHandsTalentPath, ConfigWorkflowCatalog.TalentDefinition),
            BuildRow("Bulwark Step Talent", TheCircussyOneAssetPaths.BulwarkStepTalentPath, ConfigWorkflowCatalog.TalentDefinition),
            BuildRow("Aerial Tempo Talent", TheCircussyOneAssetPaths.AcrobatTalentPath, ConfigWorkflowCatalog.TalentDefinition),
            BuildRow("Tightrope Footwork Talent", TheCircussyOneAssetPaths.TightropeFootworkTalentPath, ConfigWorkflowCatalog.TalentDefinition),
            BuildRow("Knife Flourish Talent", TheCircussyOneAssetPaths.KnifeFlourishTalentPath, ConfigWorkflowCatalog.TalentDefinition),
            BuildRow("Fast Recovery Talent", TheCircussyOneAssetPaths.FastRecoveryTalentPath, ConfigWorkflowCatalog.TalentDefinition),
            BuildRow("Long Stride Talent", TheCircussyOneAssetPaths.LongStrideTalentPath, ConfigWorkflowCatalog.TalentDefinition),
            BuildRow("Packed House Talent", TheCircussyOneAssetPaths.RingmasterTalentPath, ConfigWorkflowCatalog.TalentDefinition),
            BuildRow("Spotlight Cue Talent", TheCircussyOneAssetPaths.SpotlightCueTalentPath, ConfigWorkflowCatalog.TalentDefinition),
            BuildRow("Grand Entrance Talent", TheCircussyOneAssetPaths.GrandEntranceTalentPath, ConfigWorkflowCatalog.TalentDefinition),
            BuildRow("Encore Odds Talent", TheCircussyOneAssetPaths.EncoreOddsTalentPath, ConfigWorkflowCatalog.TalentDefinition),
            BuildRow("Commanding Tempo Talent", TheCircussyOneAssetPaths.CommandingTempoTalentPath, ConfigWorkflowCatalog.TalentDefinition),
            BuildRow("Hoop Flow Talent", TheCircussyOneAssetPaths.HoopFlowTalentPath, ConfigWorkflowCatalog.TalentDefinition),
            BuildRow("Ember Step Talent", TheCircussyOneAssetPaths.EmberStepTalentPath, ConfigWorkflowCatalog.TalentDefinition),
            BuildRow("Close Circle Talent", TheCircussyOneAssetPaths.CloseCircleTalentPath, ConfigWorkflowCatalog.TalentDefinition),
            BuildRow("Warm Applause Talent", TheCircussyOneAssetPaths.WarmApplauseTalentPath, ConfigWorkflowCatalog.TalentDefinition),
            BuildRow("Second Circle Talent", TheCircussyOneAssetPaths.SecondCircleTalentPath, ConfigWorkflowCatalog.TalentDefinition),
            BuildRow("Encore Bounce Talent", TheCircussyOneAssetPaths.EncoreBounceTalentPath, ConfigWorkflowCatalog.TalentDefinition),
            BuildRow("Soft Catch Talent", TheCircussyOneAssetPaths.SoftCatchTalentPath, ConfigWorkflowCatalog.TalentDefinition),
            BuildRow("Ricochet Rhythm Talent", TheCircussyOneAssetPaths.RicochetRhythmTalentPath, ConfigWorkflowCatalog.TalentDefinition),
            BuildRow("Carom Line Talent", TheCircussyOneAssetPaths.CaromLineTalentPath, ConfigWorkflowCatalog.TalentDefinition),
            BuildRow("Packed Balls Talent", TheCircussyOneAssetPaths.PackedBallsTalentPath, ConfigWorkflowCatalog.TalentDefinition),
            BuildRow("Talent Catalog", TheCircussyOneAssetPaths.TalentCatalogPath, ConfigWorkflowCatalog.TalentCatalog),
            BuildRow("Devsample Sam Performer", TheCircussyOneAssetPaths.JugglerPerformerPath, ConfigWorkflowCatalog.PerformerDefinition),
            BuildRow("Devsample Cappa Performer", TheCircussyOneAssetPaths.CappaPerformerPath, ConfigWorkflowCatalog.PerformerDefinition),
            BuildRow("Strongman Performer", TheCircussyOneAssetPaths.StrongmanPerformerPath, ConfigWorkflowCatalog.PerformerDefinition),
            BuildRow("Acrobat Performer", TheCircussyOneAssetPaths.AcrobatPerformerPath, ConfigWorkflowCatalog.PerformerDefinition),
            BuildRow("Ringmaster Performer", TheCircussyOneAssetPaths.RingmasterPerformerPath, ConfigWorkflowCatalog.PerformerDefinition),
            BuildRow("Sola Hoop Dancer Performer", TheCircussyOneAssetPaths.SolaHoopDancerPerformerPath, ConfigWorkflowCatalog.PerformerDefinition),
            BuildRow("Bibi Ball Juggler Performer", TheCircussyOneAssetPaths.BibiBallJugglerPerformerPath, ConfigWorkflowCatalog.PerformerDefinition),
            BuildRow("Performer Catalog", TheCircussyOneAssetPaths.PerformerCatalogPath, ConfigWorkflowCatalog.PerformerCatalog),
            BuildRow("Blue XP Gem", TheCircussyOneAssetPaths.BlueXpGemPath, ConfigWorkflowCatalog.XpGemDefinition),
            BuildRow("Green XP Gem", TheCircussyOneAssetPaths.GreenXpGemPath, ConfigWorkflowCatalog.XpGemDefinition),
            BuildRow("Red XP Gem", TheCircussyOneAssetPaths.RedXpGemPath, ConfigWorkflowCatalog.XpGemDefinition),
            BuildRow("XP Gem Catalog", TheCircussyOneAssetPaths.XpGemCatalogPath, ConfigWorkflowCatalog.XpGemCatalog),
            BuildRow("Treat", TheCircussyOneAssetPaths.TreatPath, ConfigWorkflowCatalog.HealthPickupDefinition),
            BuildRow("Health Pickup Catalog", TheCircussyOneAssetPaths.HealthPickupCatalogPath, ConfigWorkflowCatalog.HealthPickupCatalog),
            BuildRow("Snack Box", TheCircussyOneAssetPaths.SnackBoxPath, ConfigWorkflowCatalog.HealingPropDefinition),
            BuildRow("Snack Cart", TheCircussyOneAssetPaths.SnackCartPath, ConfigWorkflowCatalog.HealingPropDefinition),
            BuildRow("Healing Prop Catalog", TheCircussyOneAssetPaths.HealingPropCatalogPath, ConfigWorkflowCatalog.HealingPropCatalog),
            BuildRow("World Reward Placement Catalog", TheCircussyOneAssetPaths.WorldRewardPlacementCatalogPath, ConfigWorkflowCatalog.WorldRewardPlacementCatalog),
            BuildRow("Normal Enemy", TheCircussyOneAssetPaths.NormalEnemyPath, ConfigWorkflowCatalog.EnemyDefinition),
            BuildRow("Enemy Catalog", TheCircussyOneAssetPaths.EnemyCatalogPath, ConfigWorkflowCatalog.EnemyCatalog),
            BuildRow("Opening Headliner Actor", TheCircussyOneAssetPaths.OpeningHeadlinerEnemyPath, ConfigWorkflowCatalog.EnemyDefinition),
            BuildRow("Opening Headliner", TheCircussyOneAssetPaths.OpeningHeadlinerPath, ConfigWorkflowCatalog.HeadlinerDefinition),
            BuildRow("Headliner Catalog", TheCircussyOneAssetPaths.HeadlinerCatalogPath, ConfigWorkflowCatalog.HeadlinerCatalog),
            BuildRow("Camera", TheCircussyOneAssetPaths.CameraConfigPath, ConfigWorkflowCatalog.CameraConfig),
            BuildRow("Actor Motion Visuals", TheCircussyOneAssetPaths.ActorMotionVisualConfigPath, ConfigWorkflowCatalog.ActorMotionVisualConfig),
            BuildRow("Grid Visuals", TheCircussyOneAssetPaths.GridVisualConfigPath, ConfigWorkflowCatalog.GridVisualConfig, TheCircussyOneGridVisualApplier.ApplyNow),
            BuildRow("Lighting", TheCircussyOneAssetPaths.LightingVisualConfigPath, ConfigWorkflowCatalog.LightingVisualConfig, TheCircussyOneLightingVisualApplier.ApplyNow),
            BuildRow("Damage Feedback", TheCircussyOneAssetPaths.DamageFeedbackVisualConfigPath, ConfigWorkflowCatalog.DamageFeedbackVisualConfig, TheCircussyOneDamageFeedbackVisualApplier.ApplyNow),
            BuildRow("Enemy Spawn Visuals", TheCircussyOneAssetPaths.EnemySpawnVisualConfigPath, ConfigWorkflowCatalog.EnemySpawnVisualConfig, TheCircussyOneEnemySpawnVisualApplier.ApplyNow),
            BuildRow("VFX Visuals", TheCircussyOneAssetPaths.VfxVisualConfigPath, ConfigWorkflowCatalog.VfxVisualConfig, TheCircussyOneVfxVisualApplier.ApplyNow),
            BuildRow("Game Audio", TheCircussyOneAssetPaths.GameAudioConfigPath, ConfigWorkflowCatalog.GameAudioConfig),
            BuildRow("Game Haptics", TheCircussyOneAssetPaths.GameHapticsConfigPath, ConfigWorkflowCatalog.GameHapticsConfig),
            BuildRow("HUD Visuals", TheCircussyOneAssetPaths.HudVisualConfigPath, ConfigWorkflowCatalog.HudVisualConfig, TheCircussyOneHudVisualApplier.ApplyNow),
            BuildRow("XP Gain Counter", TheCircussyOneAssetPaths.XpGainCounterVisualConfigPath, ConfigWorkflowCatalog.XpGainCounterVisualConfig, TheCircussyOneXpGainCounterVisualApplier.ApplyNow),
            BuildRow("World Interaction Prompt", TheCircussyOneAssetPaths.WorldInteractionPromptVisualConfigPath, ConfigWorkflowCatalog.WorldInteractionPromptVisualConfig, TheCircussyOneWorldInteractionPromptVisualApplier.ApplyNow),
            BuildRow("Prototype Scene", TheCircussyOneAssetPaths.ScenePath, ConfigWorkflowCatalog.StructuralScene)
        };
    }

    public static List<ConfigHubStatusRow> BuildGeneratedAssetRows()
    {
        return TheCircussyOneGeneratedAssetManifest.All
            .Select(BuildGeneratedAssetRow)
            .ToList();
    }

    public static ConfigHubDoctorSummary GetDoctorSummary()
    {
        List<AuthoringCheckResult> results = AuthoringDoctorRunner.RunAllChecks();
        return new ConfigHubDoctorSummary(
            results.Count(result => result.Severity == AuthoringCheckSeverity.Error),
            results.Count(result => result.Severity == AuthoringCheckSeverity.Warning),
            results.Count(result => result.Severity == AuthoringCheckSeverity.Info));
    }

    public static ConfigHubStatusRow BuildRowForTests(
        string displayName,
        string assetPath,
        ConfigWorkflowDescriptor workflow,
        Action applyAction = null)
    {
        return BuildRow(displayName, assetPath, workflow, applyAction);
    }

    private static ConfigHubStatusRow BuildRow(
        string displayName,
        string assetPath,
        ConfigWorkflowDescriptor workflow,
        Action applyAction = null)
    {
        bool exists = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath) != null;
        ConfigHubStatusSeverity severity = exists ? ConfigHubStatusSeverity.Ok : ConfigHubStatusSeverity.Error;
        string status = exists ? workflow.Summary : $"Missing asset at {assetPath}";
        return new ConfigHubStatusRow(displayName, workflow, assetPath, exists, status, severity, applyAction);
    }

    private static ConfigHubStatusRow BuildGeneratedAssetRow(GeneratedAssetManifestEntry entry)
    {
        bool exists = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(entry.Path) != null;
        ConfigHubStatusSeverity severity = exists ? ConfigHubStatusSeverity.Ok : ConfigHubStatusSeverity.Error;
        ConfigWorkflowDescriptor workflow = WorkflowFor(entry);
        string status = exists
            ? $"{entry.Owner} owns this {entry.Category}."
            : $"Missing generated asset at {entry.Path}";
        return new ConfigHubStatusRow(entry.DisplayName, workflow, entry.Path, exists, status, severity, ApplyActionFor(entry));
    }

    private static ConfigWorkflowDescriptor WorkflowFor(GeneratedAssetManifestEntry entry)
    {
        return entry.WorkflowKind switch
        {
            ConfigWorkflowKind.StructuralReset => ConfigWorkflowCatalog.StructuralScene,
            ConfigWorkflowKind.ProjectedVisual => ConfigWorkflowCatalog.ProjectedVisual($"{entry.DisplayName} is generated by {entry.Owner}."),
            _ => ConfigWorkflowCatalog.LiveRuntime($"{entry.DisplayName} is runtime-owned.")
        };
    }

    private static Action ApplyActionFor(GeneratedAssetManifestEntry entry)
    {
        if (entry.SafeAction == GeneratedAssetSafeAction.GenerateVisualTestLab)
        {
            return VisualTestLabBuilder.GenerateAllScenes;
        }

        if (entry.SafeAction == GeneratedAssetSafeAction.GenerateAuthoringShowroom)
        {
            return AuthoringShowroomBuilder.GenerateAuthoringShowroom;
        }

        if (entry.SafeAction != GeneratedAssetSafeAction.ApplyProjectedVisuals)
        {
            return null;
        }

        return entry.Owner switch
        {
            GeneratedAssetOwner.GridVisuals => TheCircussyOneGridVisualApplier.ApplyNow,
            GeneratedAssetOwner.LightingVisuals => TheCircussyOneLightingVisualApplier.ApplyNow,
            GeneratedAssetOwner.DamageFeedback => TheCircussyOneDamageFeedbackVisualApplier.ApplyNow,
            GeneratedAssetOwner.XpGainCounterVisuals => TheCircussyOneXpGainCounterVisualApplier.ApplyNow,
            GeneratedAssetOwner.WorldInteractionPromptVisuals => TheCircussyOneWorldInteractionPromptVisualApplier.ApplyNow,
            GeneratedAssetOwner.EnemySpawnVisuals => TheCircussyOneEnemySpawnVisualApplier.ApplyNow,
            GeneratedAssetOwner.VfxVisuals => TheCircussyOneVfxVisualApplier.ApplyNow,
            GeneratedAssetOwner.HudVisuals => TheCircussyOneHudVisualApplier.ApplyNow,
            _ => null
        };
    }
}
