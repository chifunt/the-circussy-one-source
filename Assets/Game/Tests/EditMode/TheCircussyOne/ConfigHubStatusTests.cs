using System.Linq;
using NUnit.Framework;
using TheCircussyOne.Config;

public sealed class ConfigHubStatusTests
{
    [Test]
    public void StatusModelIncludesAllKnownConfigAssets()
    {
        ConfigHubStatusRow[] rows = ConfigHubStatusModel.BuildRows().ToArray();
        string[] paths = rows.Select(row => row.AssetPath).ToArray();

        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.GameConfigPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.RunScheduleConfigPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.RunWorldGenerationConfigPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.CannonWeaponPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.KnifeFanWeaponPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.SpotlightBoltWeaponPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.FireHoopWeaponPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.JugglingBallWeaponPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.WeaponCatalogPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.RubberSolesItemPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.SafetyPaddingItemPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.MagnetCharmItemPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.LuckyCoinItemPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.StudyNotesItemPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.TempoBraceletItemPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.PowderFlaskItemPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.SpringBootsItemPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.WideLensItemPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.LongFuseItemPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.ItemCatalogPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.OpenChestPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.LockedChestPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.PremiumChestPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.SpecialEnemyChestPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.ChestCatalogPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.SmallTicketStackPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.TicketRollPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.JackpotCachePath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.TicketDepositCatalogPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.BlueXpGemPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.GreenXpGemPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.RedXpGemPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.XpGemCatalogPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.TreatPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.HealthPickupCatalogPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.SnackBoxPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.SnackCartPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.HealingPropCatalogPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.WorldRewardPlacementCatalogPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.NormalEnemyPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.EnemyCatalogPath));
        foreach (string path in TheCircussyOneAssetPaths.WeaponUpgradePaths)
        {
            Assert.That(paths, Does.Contain(path));
        }

        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.UpgradeCatalogPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.FootworkTalentPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.StageStaminaTalentPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.ToughSkinTalentPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.CrowdFavoriteTalentPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.QuickHandsTalentPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.SharpEyeTalentPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.LongReachTalentPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.LuckyBreakTalentPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.BiggerPropsTalentPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.LongerActTalentPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.MagneticApplauseTalentPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.SpringboardTalentPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.JugglerTalentPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.CappaTalentPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.StrongmanTalentPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.AcrobatTalentPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.RingmasterTalentPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.HoopFlowTalentPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.EncoreBounceTalentPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.TalentCatalogPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.JugglerPerformerPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.CappaPerformerPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.StrongmanPerformerPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.AcrobatPerformerPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.RingmasterPerformerPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.SolaHoopDancerPerformerPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.BibiBallJugglerPerformerPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.PerformerCatalogPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.CameraConfigPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.ActorMotionVisualConfigPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.GridVisualConfigPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.LightingVisualConfigPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.DamageFeedbackVisualConfigPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.EnemySpawnVisualConfigPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.VfxVisualConfigPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.HudVisualConfigPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.XpGainCounterVisualConfigPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.WorldInteractionPromptVisualConfigPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.GameHapticsConfigPath));
    }

    [Test]
    public void StatusModelWorkflowKindsMatchCatalog()
    {
        ConfigHubStatusRow[] rows = ConfigHubStatusModel.BuildRows().ToArray();

        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.GameConfigPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.GameConfig.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.RunScheduleConfigPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.RunScheduleConfig.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.RunWorldGenerationConfigPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.RunWorldGenerationConfig.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.CannonWeaponPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.WeaponDefinition.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.KnifeFanWeaponPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.WeaponDefinition.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.SpotlightBoltWeaponPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.WeaponDefinition.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.FireHoopWeaponPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.WeaponDefinition.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.JugglingBallWeaponPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.WeaponDefinition.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.WeaponCatalogPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.WeaponCatalog.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.RubberSolesItemPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.ItemDefinition.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.SafetyPaddingItemPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.ItemDefinition.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.MagnetCharmItemPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.ItemDefinition.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.LuckyCoinItemPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.ItemDefinition.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.StudyNotesItemPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.ItemDefinition.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.TempoBraceletItemPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.ItemDefinition.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.PowderFlaskItemPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.ItemDefinition.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.SpringBootsItemPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.ItemDefinition.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.WideLensItemPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.ItemDefinition.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.LongFuseItemPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.ItemDefinition.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.ItemCatalogPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.ItemCatalog.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.OpenChestPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.ChestDefinition.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.LockedChestPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.ChestDefinition.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.PremiumChestPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.ChestDefinition.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.SpecialEnemyChestPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.ChestDefinition.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.ChestCatalogPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.ChestCatalog.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.SmallTicketStackPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.TicketDepositDefinition.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.TicketRollPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.TicketDepositDefinition.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.JackpotCachePath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.TicketDepositDefinition.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.TicketDepositCatalogPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.TicketDepositCatalog.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.BlueXpGemPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.XpGemDefinition.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.GreenXpGemPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.XpGemDefinition.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.RedXpGemPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.XpGemDefinition.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.XpGemCatalogPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.XpGemCatalog.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.TreatPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.HealthPickupDefinition.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.HealthPickupCatalogPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.HealthPickupCatalog.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.SnackBoxPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.HealingPropDefinition.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.SnackCartPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.HealingPropDefinition.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.HealingPropCatalogPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.HealingPropCatalog.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.WorldRewardPlacementCatalogPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.WorldRewardPlacementCatalog.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.NormalEnemyPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.EnemyDefinition.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.EnemyCatalogPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.EnemyCatalog.Kind));
        foreach (string path in TheCircussyOneAssetPaths.WeaponUpgradePaths)
        {
            Assert.That(RowFor(rows, path).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.UpgradeDefinition.Kind));
        }

        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.UpgradeCatalogPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.UpgradeCatalog.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.FootworkTalentPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.TalentDefinition.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.StageStaminaTalentPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.TalentDefinition.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.ToughSkinTalentPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.TalentDefinition.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.CrowdFavoriteTalentPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.TalentDefinition.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.QuickHandsTalentPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.TalentDefinition.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.SharpEyeTalentPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.TalentDefinition.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.LongReachTalentPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.TalentDefinition.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.LuckyBreakTalentPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.TalentDefinition.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.BiggerPropsTalentPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.TalentDefinition.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.SpringboardTalentPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.TalentDefinition.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.JugglerTalentPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.TalentDefinition.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.CappaTalentPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.TalentDefinition.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.StrongmanTalentPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.TalentDefinition.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.AcrobatTalentPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.TalentDefinition.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.RingmasterTalentPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.TalentDefinition.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.HoopFlowTalentPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.TalentDefinition.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.EncoreBounceTalentPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.TalentDefinition.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.TalentCatalogPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.TalentCatalog.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.JugglerPerformerPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.PerformerDefinition.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.CappaPerformerPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.PerformerDefinition.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.StrongmanPerformerPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.PerformerDefinition.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.AcrobatPerformerPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.PerformerDefinition.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.RingmasterPerformerPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.PerformerDefinition.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.SolaHoopDancerPerformerPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.PerformerDefinition.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.BibiBallJugglerPerformerPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.PerformerDefinition.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.PerformerCatalogPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.PerformerCatalog.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.CameraConfigPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.CameraConfig.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.ActorMotionVisualConfigPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.ActorMotionVisualConfig.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.GridVisualConfigPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.GridVisualConfig.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.LightingVisualConfigPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.LightingVisualConfig.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.DamageFeedbackVisualConfigPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.DamageFeedbackVisualConfig.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.EnemySpawnVisualConfigPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.EnemySpawnVisualConfig.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.VfxVisualConfigPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.VfxVisualConfig.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.HudVisualConfigPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.HudVisualConfig.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.XpGainCounterVisualConfigPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.XpGainCounterVisualConfig.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.WorldInteractionPromptVisualConfigPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.WorldInteractionPromptVisualConfig.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.GameHapticsConfigPath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.GameHapticsConfig.Kind));
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.ScenePath).WorkflowKind, Is.EqualTo(ConfigWorkflowCatalog.StructuralScene.Kind));
    }

    [Test]
    public void LiveRuntimeRowsDoNotExposeApplyActions()
    {
        ConfigHubStatusRow[] rows = ConfigHubStatusModel.BuildRows().ToArray();

        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.GameConfigPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.RunWorldGenerationConfigPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.CannonWeaponPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.KnifeFanWeaponPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.SpotlightBoltWeaponPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.FireHoopWeaponPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.JugglingBallWeaponPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.WeaponCatalogPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.RubberSolesItemPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.SafetyPaddingItemPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.MagnetCharmItemPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.LuckyCoinItemPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.StudyNotesItemPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.TempoBraceletItemPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.PowderFlaskItemPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.SpringBootsItemPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.WideLensItemPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.LongFuseItemPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.ItemCatalogPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.OpenChestPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.LockedChestPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.PremiumChestPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.SpecialEnemyChestPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.ChestCatalogPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.SmallTicketStackPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.TicketRollPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.JackpotCachePath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.TicketDepositCatalogPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.WorldRewardPlacementCatalogPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.BlueXpGemPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.GreenXpGemPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.RedXpGemPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.XpGemCatalogPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.NormalEnemyPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.EnemyCatalogPath).HasApplyAction, Is.False);
        foreach (string path in TheCircussyOneAssetPaths.WeaponUpgradePaths)
        {
            Assert.That(RowFor(rows, path).HasApplyAction, Is.False);
        }

        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.UpgradeCatalogPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.FootworkTalentPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.StageStaminaTalentPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.ToughSkinTalentPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.CrowdFavoriteTalentPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.QuickHandsTalentPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.SharpEyeTalentPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.LongReachTalentPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.LuckyBreakTalentPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.BiggerPropsTalentPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.SpringboardTalentPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.JugglerTalentPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.CappaTalentPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.StrongmanTalentPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.AcrobatTalentPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.RingmasterTalentPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.HoopFlowTalentPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.EncoreBounceTalentPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.TalentCatalogPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.JugglerPerformerPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.CappaPerformerPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.StrongmanPerformerPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.AcrobatPerformerPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.RingmasterPerformerPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.SolaHoopDancerPerformerPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.BibiBallJugglerPerformerPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.PerformerCatalogPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.CameraConfigPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.ActorMotionVisualConfigPath).HasApplyAction, Is.False);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.GameHapticsConfigPath).HasApplyAction, Is.False);
    }

    [Test]
    public void ProjectedVisualRowsExposeApplyActions()
    {
        ConfigHubStatusRow[] rows = ConfigHubStatusModel.BuildRows().ToArray();

        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.GridVisualConfigPath).HasApplyAction, Is.True);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.LightingVisualConfigPath).HasApplyAction, Is.True);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.DamageFeedbackVisualConfigPath).HasApplyAction, Is.True);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.EnemySpawnVisualConfigPath).HasApplyAction, Is.True);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.VfxVisualConfigPath).HasApplyAction, Is.True);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.HudVisualConfigPath).HasApplyAction, Is.True);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.XpGainCounterVisualConfigPath).HasApplyAction, Is.True);
        Assert.That(RowFor(rows, TheCircussyOneAssetPaths.WorldInteractionPromptVisualConfigPath).HasApplyAction, Is.True);
    }

    [Test]
    public void MissingFakeConfigPathProducesErrorRowWithoutExecutingFix()
    {
        ConfigHubStatusRow row = ConfigHubStatusModel.BuildRowForTests(
            "Missing Test Config",
            "Assets/Game/ScriptableObjects/Visuals/__MissingConfigHubStatusTest.asset",
            ConfigWorkflowCatalog.ProjectedVisual("Test-only missing config."),
            () => Assert.Fail("Apply action should not run while building a status row."));

        Assert.That(row.Exists, Is.False);
        Assert.That(row.Severity, Is.EqualTo(ConfigHubStatusSeverity.Error));
        Assert.That(row.StatusMessage, Does.Contain("Missing asset"));
        Assert.That(row.HasApplyAction, Is.True);
    }

    [Test]
    public void DoctorSummaryCanBeGeneratedWithoutExecutingFixes()
    {
        ConfigHubDoctorSummary summary = ConfigHubStatusModel.GetDoctorSummary();

        Assert.That(summary.TotalCount, Is.GreaterThan(0));
        Assert.That(summary.ErrorCount, Is.GreaterThanOrEqualTo(0));
        Assert.That(summary.WarningCount, Is.GreaterThanOrEqualTo(0));
        Assert.That(summary.InfoCount, Is.GreaterThanOrEqualTo(0));
        Assert.That(summary.Message, Does.Contain("Doctor:"));
    }

    [Test]
    public void ConfigHubEditorTypeCompilesWithStatusModel()
    {
        Assert.That(typeof(TheCircussyOneConfigHub), Is.Not.Null);
        Assert.That(typeof(ConfigHubStatusModel), Is.Not.Null);
        Assert.That(typeof(ConfigHubStatusRow), Is.Not.Null);
    }

    private static ConfigHubStatusRow RowFor(ConfigHubStatusRow[] rows, string path)
    {
        return rows.Single(row => row.AssetPath == path);
    }
}
