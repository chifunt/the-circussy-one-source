using System.Collections.Generic;
using System.IO;
using MoreMountains.Feedbacks;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.DI;
using TheCircussyOne.Runtime;
using TheCircussyOne.Visuals;

public static partial class TheCircussyOneSceneBuilder
{
    private const string ScenePath = TheCircussyOneAssetPaths.ScenePath;
    private const string PrefabFolder = TheCircussyOneAssetPaths.PrefabFolder;
    private const string VfxPrefabFolder = TheCircussyOneAssetPaths.VfxPrefabFolder;
    private const string MaterialFolder = TheCircussyOneAssetPaths.MaterialFolder;
    private const string BalanceFolder = TheCircussyOneAssetPaths.BalanceFolder;
    private const string WeaponBalanceFolder = TheCircussyOneAssetPaths.WeaponBalanceFolder;
    private const string ItemBalanceFolder = TheCircussyOneAssetPaths.ItemBalanceFolder;
    private const string PickupBalanceFolder = TheCircussyOneAssetPaths.PickupBalanceFolder;
    private const string HealingPropBalanceFolder = TheCircussyOneAssetPaths.HealingPropBalanceFolder;
    private const string UpgradeBalanceFolder = TheCircussyOneAssetPaths.UpgradeBalanceFolder;
    private const string TalentBalanceFolder = TheCircussyOneAssetPaths.TalentBalanceFolder;
    private const string PerformerBalanceFolder = TheCircussyOneAssetPaths.PerformerBalanceFolder;
    private const string EnemyBalanceFolder = TheCircussyOneAssetPaths.EnemyBalanceFolder;
    private const string CameraSettingsFolder = TheCircussyOneAssetPaths.CameraSettingsFolder;
    private const string VisualSettingsFolder = TheCircussyOneAssetPaths.VisualSettingsFolder;
    private const string VisualAssetFolder = TheCircussyOneAssetPaths.VisualAssetFolder;
    private const string ShaderFolder = TheCircussyOneAssetPaths.ShaderFolder;
    private const string GridTextureFolder = TheCircussyOneAssetPaths.GridTextureFolder;
    private const string LightingVisualFolder = TheCircussyOneAssetPaths.LightingVisualFolder;
    private const string VfxVisualFolder = TheCircussyOneAssetPaths.VfxVisualFolder;
    private const string UiFolder = TheCircussyOneAssetPaths.UiFolder;
    private const string PcRendererDataPath = TheCircussyOneAssetPaths.PcRendererDataPath;
    private const string PickupDiamondMeshPath = TheCircussyOneAssetPaths.PickupDiamondMeshPath;
    private const string WorldGridShaderPath = TheCircussyOneAssetPaths.WorldGridShaderPath;
    private const string ContactShadowShaderPath = TheCircussyOneAssetPaths.ContactShadowShaderPath;
    private const string PlayerDamageVignetteShaderPath = TheCircussyOneAssetPaths.PlayerDamageVignetteShaderPath;
    private const string InteractableSelectionMaskShaderPath = TheCircussyOneAssetPaths.InteractableSelectionMaskShaderPath;
    private const string InteractableSelectionOutlineShaderPath = TheCircussyOneAssetPaths.InteractableSelectionOutlineShaderPath;
    private const string ContactShadowTexturePath = TheCircussyOneAssetPaths.ContactShadowTexturePath;
    private const string ContactShadowMaterialPath = TheCircussyOneAssetPaths.ContactShadowMaterialPath;
    private const string LightingVolumeProfilePath = TheCircussyOneAssetPaths.LightingVolumeProfilePath;
    private const string FaceMaterialPath = TheCircussyOneAssetPaths.FaceMaterialPath;
    private const string DamageNumberPrefabPath = TheCircussyOneAssetPaths.DamageNumberPrefabPath;
    private const string XpGainCounterPrefabPath = TheCircussyOneAssetPaths.XpGainCounterPrefabPath;
    private const string WorldInteractionPromptPrefabPath = TheCircussyOneAssetPaths.WorldInteractionPromptPrefabPath;
    private const string EnemySpawnIndicatorPrefabPath = TheCircussyOneAssetPaths.EnemySpawnIndicatorPrefabPath;
    private const string FireHoopOrbitPrefabPath = TheCircussyOneAssetPaths.FireHoopOrbitPrefabPath;
    private const string PlayerMoveDustPrefabPath = TheCircussyOneAssetPaths.PlayerMoveDustPrefabPath;
    private const string PlayerJumpTrailPrefabPath = TheCircussyOneAssetPaths.PlayerJumpTrailPrefabPath;
    private const string WorldAmbientDustPrefabPath = TheCircussyOneAssetPaths.WorldAmbientDustPrefabPath;
    private const string FireHoopAreaTelegraphPrefabPath = TheCircussyOneAssetPaths.FireHoopAreaTelegraphPrefabPath;
    private const string ProjectileMuzzlePuffPrefabPath = TheCircussyOneAssetPaths.ProjectileMuzzlePuffPrefabPath;
    private const string ProjectileBounceBurstPrefabPath = TheCircussyOneAssetPaths.ProjectileBounceBurstPrefabPath;
    private const string ProjectileExplosionPrefabPath = TheCircussyOneAssetPaths.ProjectileExplosionPrefabPath;
    private const string EnemyHitSparksPrefabPath = TheCircussyOneAssetPaths.EnemyHitSparksPrefabPath;
    private const string KnifeHitSparksPrefabPath = TheCircussyOneAssetPaths.KnifeHitSparksPrefabPath;
    private const string EnemyDeathBurstPrefabPath = TheCircussyOneAssetPaths.EnemyDeathBurstPrefabPath;
    private const string XpPickupAttractTrailPrefabPath = TheCircussyOneAssetPaths.XpPickupAttractTrailPrefabPath;
    private const string XpPickupCollectPopPrefabPath = TheCircussyOneAssetPaths.XpPickupCollectPopPrefabPath;
    private const string PlayerDamageBurstPrefabPath = TheCircussyOneAssetPaths.PlayerDamageBurstPrefabPath;
    private const string PlayerJumpTakeoffPrefabPath = TheCircussyOneAssetPaths.PlayerJumpTakeoffPrefabPath;
    private const string PlayerJumpLandPrefabPath = TheCircussyOneAssetPaths.PlayerJumpLandPrefabPath;
    private const string LevelUpBurstPrefabPath = TheCircussyOneAssetPaths.LevelUpBurstPrefabPath;
    private const string DamageNumberMaterialPath = TheCircussyOneAssetPaths.DamageNumberMaterialPath;
    private const string PlayerDamageVignetteMaterialPath = TheCircussyOneAssetPaths.PlayerDamageVignetteMaterialPath;
    private const string XpGainCounterMaterialPath = TheCircussyOneAssetPaths.XpGainCounterMaterialPath;
    private const string WorldInteractionPromptMaterialPath = TheCircussyOneAssetPaths.WorldInteractionPromptMaterialPath;
    private const string WorldInteractionPromptBarMaterialPath = TheCircussyOneAssetPaths.WorldInteractionPromptBarMaterialPath;
    private const string VfxAlphaMaterialPath = TheCircussyOneAssetPaths.VfxAlphaMaterialPath;
    private const string VfxAdditiveMaterialPath = TheCircussyOneAssetPaths.VfxAdditiveMaterialPath;
    private const string HudUxmlPath = TheCircussyOneAssetPaths.HudUxmlPath;
    private const string HudUssPath = TheCircussyOneAssetPaths.HudUssPath;
    public const string HudPanelSettingsPath = TheCircussyOneAssetPaths.HudPanelSettingsPath;
    public const string GameConfigPath = TheCircussyOneAssetPaths.GameConfigPath;
    public const string RunWorldGenerationConfigPath = TheCircussyOneAssetPaths.RunWorldGenerationConfigPath;
    public const string CannonWeaponPath = TheCircussyOneAssetPaths.CannonWeaponPath;
    public const string KnifeFanWeaponPath = TheCircussyOneAssetPaths.KnifeFanWeaponPath;
    public const string SpotlightBoltWeaponPath = TheCircussyOneAssetPaths.SpotlightBoltWeaponPath;
    public const string FireHoopWeaponPath = TheCircussyOneAssetPaths.FireHoopWeaponPath;
    public const string JugglingBallWeaponPath = TheCircussyOneAssetPaths.JugglingBallWeaponPath;
    public const string WeaponCatalogPath = TheCircussyOneAssetPaths.WeaponCatalogPath;
    public const string RubberSolesItemPath = TheCircussyOneAssetPaths.RubberSolesItemPath;
    public const string SafetyPaddingItemPath = TheCircussyOneAssetPaths.SafetyPaddingItemPath;
    public const string MagnetCharmItemPath = TheCircussyOneAssetPaths.MagnetCharmItemPath;
    public const string LuckyCoinItemPath = TheCircussyOneAssetPaths.LuckyCoinItemPath;
    public const string StudyNotesItemPath = TheCircussyOneAssetPaths.StudyNotesItemPath;
    public const string TempoBraceletItemPath = TheCircussyOneAssetPaths.TempoBraceletItemPath;
    public const string PowderFlaskItemPath = TheCircussyOneAssetPaths.PowderFlaskItemPath;
    public const string SpringBootsItemPath = TheCircussyOneAssetPaths.SpringBootsItemPath;
    public const string WideLensItemPath = TheCircussyOneAssetPaths.WideLensItemPath;
    public const string LongFuseItemPath = TheCircussyOneAssetPaths.LongFuseItemPath;
    public const string ItemCatalogPath = TheCircussyOneAssetPaths.ItemCatalogPath;
    public const string OpenChestPath = TheCircussyOneAssetPaths.OpenChestPath;
    public const string LockedChestPath = TheCircussyOneAssetPaths.LockedChestPath;
    public const string PremiumChestPath = TheCircussyOneAssetPaths.PremiumChestPath;
    public const string SpecialEnemyChestPath = TheCircussyOneAssetPaths.SpecialEnemyChestPath;
    public const string ChestCatalogPath = TheCircussyOneAssetPaths.ChestCatalogPath;
    public const string SmallTicketStackPath = TheCircussyOneAssetPaths.SmallTicketStackPath;
    public const string TicketRollPath = TheCircussyOneAssetPaths.TicketRollPath;
    public const string JackpotCachePath = TheCircussyOneAssetPaths.JackpotCachePath;
    public const string TicketDepositCatalogPath = TheCircussyOneAssetPaths.TicketDepositCatalogPath;
    public const string BlueXpGemPath = TheCircussyOneAssetPaths.BlueXpGemPath;
    public const string GreenXpGemPath = TheCircussyOneAssetPaths.GreenXpGemPath;
    public const string RedXpGemPath = TheCircussyOneAssetPaths.RedXpGemPath;
    public const string XpGemCatalogPath = TheCircussyOneAssetPaths.XpGemCatalogPath;
    public const string TreatPath = TheCircussyOneAssetPaths.TreatPath;
    public const string HealthPickupCatalogPath = TheCircussyOneAssetPaths.HealthPickupCatalogPath;
    public const string SnackBoxPath = TheCircussyOneAssetPaths.SnackBoxPath;
    public const string SnackCartPath = TheCircussyOneAssetPaths.SnackCartPath;
    public const string HealingPropCatalogPath = TheCircussyOneAssetPaths.HealingPropCatalogPath;
    public const string WorldRewardPlacementCatalogPath = TheCircussyOneAssetPaths.WorldRewardPlacementCatalogPath;
    public const string CannonTuningUpgradePath = TheCircussyOneAssetPaths.CannonTuningUpgradePath;
    public const string CannonPayloadUpgradePath = TheCircussyOneAssetPaths.CannonPayloadUpgradePath;
    public const string CannonBiggerShotUpgradePath = TheCircussyOneAssetPaths.CannonBiggerShotUpgradePath;
    public const string CannonQuickFuseUpgradePath = TheCircussyOneAssetPaths.CannonQuickFuseUpgradePath;
    public const string CannonFastShotUpgradePath = TheCircussyOneAssetPaths.CannonFastShotUpgradePath;
    public const string KnifeFanTuningUpgradePath = TheCircussyOneAssetPaths.KnifeFanTuningUpgradePath;
    public const string KnifeFanFocusUpgradePath = TheCircussyOneAssetPaths.KnifeFanFocusUpgradePath;
    public const string KnifeFanHonedBladesUpgradePath = TheCircussyOneAssetPaths.KnifeFanHonedBladesUpgradePath;
    public const string KnifeFanQuickTossUpgradePath = TheCircussyOneAssetPaths.KnifeFanQuickTossUpgradePath;
    public const string KnifeFanFastBladesUpgradePath = TheCircussyOneAssetPaths.KnifeFanFastBladesUpgradePath;
    public const string KnifeFanLingeringCutUpgradePath = TheCircussyOneAssetPaths.KnifeFanLingeringCutUpgradePath;
    public const string SpotlightBoltTuningUpgradePath = TheCircussyOneAssetPaths.SpotlightBoltTuningUpgradePath;
    public const string SpotlightBoltBrightChargeUpgradePath = TheCircussyOneAssetPaths.SpotlightBoltBrightChargeUpgradePath;
    public const string SpotlightBoltQuickCueUpgradePath = TheCircussyOneAssetPaths.SpotlightBoltQuickCueUpgradePath;
    public const string SpotlightBoltFastBeamUpgradePath = TheCircussyOneAssetPaths.SpotlightBoltFastBeamUpgradePath;
    public const string SpotlightBoltWideBeamUpgradePath = TheCircussyOneAssetPaths.SpotlightBoltWideBeamUpgradePath;
    public const string SpotlightBoltLingeringGlowUpgradePath = TheCircussyOneAssetPaths.SpotlightBoltLingeringGlowUpgradePath;
    public const string FireHoopTuningUpgradePath = TheCircussyOneAssetPaths.FireHoopTuningUpgradePath;
    public const string FireHoopWiderUpgradePath = TheCircussyOneAssetPaths.FireHoopWiderUpgradePath;
    public const string FireHoopExtraUpgradePath = TheCircussyOneAssetPaths.FireHoopExtraUpgradePath;
    public const string FireHoopQuickSpinUpgradePath = TheCircussyOneAssetPaths.FireHoopQuickSpinUpgradePath;
    public const string FireHoopBiggerFlameUpgradePath = TheCircussyOneAssetPaths.FireHoopBiggerFlameUpgradePath;
    public const string JugglingBallTuningUpgradePath = TheCircussyOneAssetPaths.JugglingBallTuningUpgradePath;
    public const string JugglingBallTimingUpgradePath = TheCircussyOneAssetPaths.JugglingBallTimingUpgradePath;
    public const string JugglingBallWeightedBallUpgradePath = TheCircussyOneAssetPaths.JugglingBallWeightedBallUpgradePath;
    public const string JugglingBallQuickTossUpgradePath = TheCircussyOneAssetPaths.JugglingBallQuickTossUpgradePath;
    public const string JugglingBallFastRollUpgradePath = TheCircussyOneAssetPaths.JugglingBallFastRollUpgradePath;
    public const string JugglingBallBiggerBallUpgradePath = TheCircussyOneAssetPaths.JugglingBallBiggerBallUpgradePath;
    public const string JugglingBallLongJuggleUpgradePath = TheCircussyOneAssetPaths.JugglingBallLongJuggleUpgradePath;
    public const string UpgradeCatalogPath = TheCircussyOneAssetPaths.UpgradeCatalogPath;
    public const string FootworkTalentPath = TheCircussyOneAssetPaths.FootworkTalentPath;
    public const string StageStaminaTalentPath = TheCircussyOneAssetPaths.StageStaminaTalentPath;
    public const string ToughSkinTalentPath = TheCircussyOneAssetPaths.ToughSkinTalentPath;
    public const string CrowdFavoriteTalentPath = TheCircussyOneAssetPaths.CrowdFavoriteTalentPath;
    public const string QuickHandsTalentPath = TheCircussyOneAssetPaths.QuickHandsTalentPath;
    public const string SharpEyeTalentPath = TheCircussyOneAssetPaths.SharpEyeTalentPath;
    public const string LongReachTalentPath = TheCircussyOneAssetPaths.LongReachTalentPath;
    public const string LuckyBreakTalentPath = TheCircussyOneAssetPaths.LuckyBreakTalentPath;
    public const string BiggerPropsTalentPath = TheCircussyOneAssetPaths.BiggerPropsTalentPath;
    public const string LongerActTalentPath = TheCircussyOneAssetPaths.LongerActTalentPath;
    public const string MagneticApplauseTalentPath = TheCircussyOneAssetPaths.MagneticApplauseTalentPath;
    public const string SpringboardTalentPath = TheCircussyOneAssetPaths.SpringboardTalentPath;
    public const string JugglerTalentPath = TheCircussyOneAssetPaths.JugglerTalentPath;
    public const string CleanCompileTalentPath = TheCircussyOneAssetPaths.CleanCompileTalentPath;
    public const string SampleArcTalentPath = TheCircussyOneAssetPaths.SampleArcTalentPath;
    public const string DebugMarkerTalentPath = TheCircussyOneAssetPaths.DebugMarkerTalentPath;
    public const string SteadyBaselineTalentPath = TheCircussyOneAssetPaths.SteadyBaselineTalentPath;
    public const string CappaTalentPath = TheCircussyOneAssetPaths.CappaTalentPath;
    public const string RubberGrinTalentPath = TheCircussyOneAssetPaths.RubberGrinTalentPath;
    public const string FaceForwardTalentPath = TheCircussyOneAssetPaths.FaceForwardTalentPath;
    public const string CapsulePopTalentPath = TheCircussyOneAssetPaths.CapsulePopTalentPath;
    public const string SampleSmileTalentPath = TheCircussyOneAssetPaths.SampleSmileTalentPath;
    public const string StrongmanTalentPath = TheCircussyOneAssetPaths.StrongmanTalentPath;
    public const string CannonBraceTalentPath = TheCircussyOneAssetPaths.CannonBraceTalentPath;
    public const string HeavyLiftTalentPath = TheCircussyOneAssetPaths.HeavyLiftTalentPath;
    public const string SteadyHandsTalentPath = TheCircussyOneAssetPaths.SteadyHandsTalentPath;
    public const string BulwarkStepTalentPath = TheCircussyOneAssetPaths.BulwarkStepTalentPath;
    public const string AcrobatTalentPath = TheCircussyOneAssetPaths.AcrobatTalentPath;
    public const string TightropeFootworkTalentPath = TheCircussyOneAssetPaths.TightropeFootworkTalentPath;
    public const string KnifeFlourishTalentPath = TheCircussyOneAssetPaths.KnifeFlourishTalentPath;
    public const string FastRecoveryTalentPath = TheCircussyOneAssetPaths.FastRecoveryTalentPath;
    public const string LongStrideTalentPath = TheCircussyOneAssetPaths.LongStrideTalentPath;
    public const string RingmasterTalentPath = TheCircussyOneAssetPaths.RingmasterTalentPath;
    public const string SpotlightCueTalentPath = TheCircussyOneAssetPaths.SpotlightCueTalentPath;
    public const string GrandEntranceTalentPath = TheCircussyOneAssetPaths.GrandEntranceTalentPath;
    public const string EncoreOddsTalentPath = TheCircussyOneAssetPaths.EncoreOddsTalentPath;
    public const string CommandingTempoTalentPath = TheCircussyOneAssetPaths.CommandingTempoTalentPath;
    public const string HoopFlowTalentPath = TheCircussyOneAssetPaths.HoopFlowTalentPath;
    public const string EmberStepTalentPath = TheCircussyOneAssetPaths.EmberStepTalentPath;
    public const string CloseCircleTalentPath = TheCircussyOneAssetPaths.CloseCircleTalentPath;
    public const string WarmApplauseTalentPath = TheCircussyOneAssetPaths.WarmApplauseTalentPath;
    public const string SecondCircleTalentPath = TheCircussyOneAssetPaths.SecondCircleTalentPath;
    public const string EncoreBounceTalentPath = TheCircussyOneAssetPaths.EncoreBounceTalentPath;
    public const string SoftCatchTalentPath = TheCircussyOneAssetPaths.SoftCatchTalentPath;
    public const string RicochetRhythmTalentPath = TheCircussyOneAssetPaths.RicochetRhythmTalentPath;
    public const string CaromLineTalentPath = TheCircussyOneAssetPaths.CaromLineTalentPath;
    public const string PackedBallsTalentPath = TheCircussyOneAssetPaths.PackedBallsTalentPath;
    public const string TalentCatalogPath = TheCircussyOneAssetPaths.TalentCatalogPath;
    public const string JugglerPerformerPath = TheCircussyOneAssetPaths.JugglerPerformerPath;
    public const string CappaPerformerPath = TheCircussyOneAssetPaths.CappaPerformerPath;
    public const string StrongmanPerformerPath = TheCircussyOneAssetPaths.StrongmanPerformerPath;
    public const string AcrobatPerformerPath = TheCircussyOneAssetPaths.AcrobatPerformerPath;
    public const string RingmasterPerformerPath = TheCircussyOneAssetPaths.RingmasterPerformerPath;
    public const string SolaHoopDancerPerformerPath = TheCircussyOneAssetPaths.SolaHoopDancerPerformerPath;
    public const string BibiBallJugglerPerformerPath = TheCircussyOneAssetPaths.BibiBallJugglerPerformerPath;
    public const string PerformerCatalogPath = TheCircussyOneAssetPaths.PerformerCatalogPath;
    public const string NormalEnemyPath = TheCircussyOneAssetPaths.NormalEnemyPath;
    public const string EnemyCatalogPath = TheCircussyOneAssetPaths.EnemyCatalogPath;
    public const string OpeningHeadlinerEnemyPath = TheCircussyOneAssetPaths.OpeningHeadlinerEnemyPath;
    public const string OpeningHeadlinerPath = TheCircussyOneAssetPaths.OpeningHeadlinerPath;
    public const string HeadlinerCatalogPath = TheCircussyOneAssetPaths.HeadlinerCatalogPath;
    public const string CameraConfigPath = TheCircussyOneAssetPaths.CameraConfigPath;
    public const string GridVisualConfigPath = TheCircussyOneAssetPaths.GridVisualConfigPath;
    public const string LightingVisualConfigPath = TheCircussyOneAssetPaths.LightingVisualConfigPath;
    public const string DamageFeedbackVisualConfigPath = TheCircussyOneAssetPaths.DamageFeedbackVisualConfigPath;
    public const string ActorMotionVisualConfigPath = TheCircussyOneAssetPaths.ActorMotionVisualConfigPath;
    public const string EnemySpawnVisualConfigPath = TheCircussyOneAssetPaths.EnemySpawnVisualConfigPath;
    public const string VfxVisualConfigPath = TheCircussyOneAssetPaths.VfxVisualConfigPath;
    public const string GameHapticsConfigPath = TheCircussyOneAssetPaths.GameHapticsConfigPath;
    public const string HudVisualConfigPath = TheCircussyOneAssetPaths.HudVisualConfigPath;
    public const string XpGainCounterVisualConfigPath = TheCircussyOneAssetPaths.XpGainCounterVisualConfigPath;
    public const string WorldInteractionPromptVisualConfigPath = TheCircussyOneAssetPaths.WorldInteractionPromptVisualConfigPath;

    [MenuItem("Tools/The Circussy One/Reset/Rebuild Prototype Scene")]
    public static void RebuildPrototypeScene()
    {
        EnsureGeneratedFolders();
        EnsureHudAssets();
        EnsureGameplayLayers();
        EnsureInteractableSelectionOutlineRendererFeature();

        GameConfig config = GetOrCreateGameConfig();
        RunScheduleConfig runScheduleConfig = GetOrCreateRunScheduleConfig();
        RunWorldGenerationConfig runWorldGenerationConfig = GetOrCreateRunWorldGenerationConfig();
        WeaponCatalog weaponCatalog = GetOrCreateWeaponCatalog();
        ItemCatalog itemCatalog = GetOrCreateItemCatalog();
        ChestCatalog chestCatalog = GetOrCreateChestCatalog();
        TicketDepositCatalog ticketDepositCatalog = GetOrCreateTicketDepositCatalog();
        XpGemCatalog xpGemCatalog = GetOrCreateXpGemCatalog();
        HealthPickupCatalog healthPickupCatalog = GetOrCreateHealthPickupCatalog();
        HealingPropCatalog healingPropCatalog = GetOrCreateHealingPropCatalog();
        WorldRewardPlacementCatalog worldRewardPlacementCatalog = GetOrCreateWorldRewardPlacementCatalog();
        UpgradeCatalog upgradeCatalog = GetOrCreateUpgradeCatalog();
        TalentCatalog talentCatalog = GetOrCreateTalentCatalog();
        PerformerCatalog performerCatalog = GetOrCreatePerformerCatalog();
        EnemyCatalog enemyCatalog = GetOrCreateEnemyCatalog();
        HeadlinerCatalog headlinerCatalog = GetOrCreateHeadlinerCatalog();
        EnemyDefinition normalEnemy = GetOrCreateNormalEnemy();
        CameraConfig cameraConfig = GetOrCreateCameraConfig();
        GridVisualConfig gridConfig = GetOrCreateGridVisualConfig();
        LightingVisualConfig lightingConfig = GetOrCreateLightingVisualConfig();
        DamageFeedbackVisualConfig damageFeedbackConfig = GetOrCreateDamageFeedbackVisualConfig();
        ActorMotionVisualConfig actorMotionVisualConfig = GetOrCreateActorMotionVisualConfig();
        EnemySpawnVisualConfig enemySpawnVisualConfig = GetOrCreateEnemySpawnVisualConfig();
        VfxVisualConfig vfxConfig = GetOrCreateVfxVisualConfig();
        HudVisualConfig hudConfig = GetOrCreateHudVisualConfig();
        XpGainCounterVisualConfig xpGainCounterConfig = GetOrCreateXpGainCounterVisualConfig();
        WorldInteractionPromptVisualConfig worldInteractionPromptConfig = GetOrCreateWorldInteractionPromptVisualConfig();
        GridVisualAssets gridAssets = TheCircussyOneGridVisualApplier.ApplyAssets(config, gridConfig);
        LightingVisualAssets lightingAssets = TheCircussyOneLightingVisualApplier.ApplyAssets(lightingConfig);
        Material damageNumberMaterial = TheCircussyOneDamageFeedbackVisualApplier.ApplyAssets(damageFeedbackConfig);
        Material xpGainCounterMaterial = TheCircussyOneXpGainCounterVisualApplier.ApplyAssets(xpGainCounterConfig);
        Material worldInteractionPromptMaterial = TheCircussyOneWorldInteractionPromptVisualApplier.ApplyTextMaterial(worldInteractionPromptConfig);
        Material worldInteractionPromptBarMaterial = TheCircussyOneWorldInteractionPromptVisualApplier.ApplyBarMaterial(worldInteractionPromptConfig);
        Material faceMaterial = GetOrCreateFaceMaterial();
        VfxVisualAssets vfxAssets = TheCircussyOneVfxVisualApplier.ApplyAssets(vfxConfig);
        TheCircussyOneHudVisualApplier.ApplyPanelSettings(hudConfig);

        EnemyView enemyPrefab = CreateEnemyPrefab(normalEnemy, gridAssets.enemyMaterial, lightingConfig, lightingAssets.contactShadowMaterial, faceMaterial);
        ProjectileView projectilePrefab = CreateProjectilePrefab(gridAssets.projectileMaterial, GetOrCreateJugglingBallWeapon());
        OrbitWeaponView fireHoopOrbitPrefab = CreateFireHoopOrbitPrefab(GetOrCreateFireHoopWeapon());
        PickupView pickupPrefab = CreatePickupPrefab(gridAssets.pickupMaterial, vfxAssets.xpPickupAttractTrailPrefab);
        DamageNumberView damageNumberPrefab = CreateDamageNumberPrefab(damageNumberMaterial, damageFeedbackConfig);
        XpGainCounterView xpGainCounterPrefab = CreateXpGainCounterPrefab(xpGainCounterMaterial, xpGainCounterConfig);
        WorldInteractionPromptView worldInteractionPromptPrefab = CreateWorldInteractionPromptPrefab(worldInteractionPromptMaterial, worldInteractionPromptBarMaterial, worldInteractionPromptConfig);
        EnemySpawnIndicatorView enemySpawnIndicatorPrefab = CreateEnemySpawnIndicatorPrefab(enemySpawnVisualConfig);

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        TheCircussyOneLightingVisualApplier.ApplyToNewScene(lightingConfig, lightingAssets.volumeProfile);
        CreateFloor(gridAssets.floorMaterial, config);
        PlayerView player = CreatePlayer(config, damageFeedbackConfig, gridAssets.playerMaterial, lightingConfig, lightingAssets.contactShadowMaterial, vfxAssets.playerMoveDustPrefab, vfxAssets.playerJumpTrailPrefab, faceMaterial);
        CameraView gameCamera = CreateCamera(player, cameraConfig, lightingConfig);
        HudView hud = CreateHud(hudConfig);

        Transform enemyRoot = CreateRoot("Enemies");
        Transform projectileRoot = CreateRoot("Projectiles");
        Transform orbitWeaponRoot = CreateRoot("Orbit Weapons");
        Transform pickupRoot = CreateRoot("Pickups");
        Transform damageNumberRoot = CreateRoot("Damage Numbers");
        Transform xpGainCounterRoot = CreateRoot("XP Gain Counter");
        Transform worldInteractionPromptRoot = CreateRoot("World Interaction Prompts");
        Transform enemySpawnIndicatorRoot = CreateRoot("Enemy Spawn Indicators");
        Transform vfxRoot = CreateRoot("VFX");

        MMF_Player shootFeedback;
        MMF_Player hitFeedback;
        MMF_Player playerDamageFeedback;
        CreateFeelFeedbacks(out shootFeedback, out hitFeedback, out playerDamageFeedback);

        var scopeObject = new GameObject("Game Lifetime Scope");
        scopeObject.AddComponent<GameLoopRunner>();
        scopeObject.AddComponent<CursorLockBehaviour>();
        var scope = scopeObject.AddComponent<GameLifetimeScope>();
        SetSerialized(scope, "gameConfig", config);
        SetSerialized(scope, "runScheduleConfig", runScheduleConfig);
        SetSerialized(scope, "runWorldGenerationConfig", runWorldGenerationConfig);
        SetSerialized(scope, "weaponCatalog", weaponCatalog);
        SetSerialized(scope, "itemCatalog", itemCatalog);
        SetSerialized(scope, "chestCatalog", chestCatalog);
        SetSerialized(scope, "ticketDepositCatalog", ticketDepositCatalog);
        SetSerialized(scope, "xpGemCatalog", xpGemCatalog);
        SetSerialized(scope, "healthPickupCatalog", healthPickupCatalog);
        SetSerialized(scope, "healingPropCatalog", healingPropCatalog);
        SetSerialized(scope, "worldRewardPlacementCatalog", worldRewardPlacementCatalog);
        SetSerialized(scope, "upgradeCatalog", upgradeCatalog);
        SetSerialized(scope, "talentCatalog", talentCatalog);
        SetSerialized(scope, "performerCatalog", performerCatalog);
        SetSerialized(scope, "enemyCatalog", enemyCatalog);
        SetSerialized(scope, "headlinerCatalog", headlinerCatalog);
        SetSerialized(scope, "cameraConfig", cameraConfig);
        SetSerialized(scope, "damageFeedbackConfig", damageFeedbackConfig);
        SetSerialized(scope, "actorMotionVisualConfig", actorMotionVisualConfig);
        SetSerialized(scope, "enemySpawnVisualConfig", enemySpawnVisualConfig);
        SetSerialized(scope, "vfxConfig", vfxConfig);
        SetSerialized(scope, "xpGainCounterConfig", xpGainCounterConfig);
        SetSerialized(scope, "worldInteractionPromptConfig", worldInteractionPromptConfig);
        SetSerialized(scope, "player", player);
        SetSerialized(scope, "gameCamera", gameCamera);
        SetSerialized(scope, "enemyRoot", enemyRoot);
        SetSerialized(scope, "projectileRoot", projectileRoot);
        SetSerialized(scope, "orbitWeaponRoot", orbitWeaponRoot);
        SetSerialized(scope, "pickupRoot", pickupRoot);
        SetSerialized(scope, "damageNumberRoot", damageNumberRoot);
        SetSerialized(scope, "xpGainCounterRoot", xpGainCounterRoot);
        SetSerialized(scope, "worldInteractionPromptRoot", worldInteractionPromptRoot);
        SetSerialized(scope, "enemySpawnIndicatorRoot", enemySpawnIndicatorRoot);
        SetSerialized(scope, "vfxRoot", vfxRoot);
        SetSerialized(scope, "enemyPrefab", enemyPrefab);
        SetSerialized(scope, "projectilePrefab", projectilePrefab);
        SetSerialized(scope, "fireHoopOrbitPrefab", fireHoopOrbitPrefab);
        SetSerialized(scope, "fireHoopAreaTelegraphPrefab", vfxAssets.fireHoopAreaTelegraphPrefab);
        SetSerialized(scope, "pickupPrefab", pickupPrefab);
        SetSerialized(scope, "damageNumberPrefab", damageNumberPrefab);
        SetSerialized(scope, "xpGainCounterPrefab", xpGainCounterPrefab);
        SetSerialized(scope, "worldInteractionPromptPrefab", worldInteractionPromptPrefab);
        SetSerialized(scope, "enemySpawnIndicatorPrefab", enemySpawnIndicatorPrefab);
        SetSerialized(scope, "projectileMuzzlePuffPrefab", vfxAssets.projectileMuzzlePuffPrefab);
        SetSerialized(scope, "projectileBounceBurstPrefab", vfxAssets.projectileBounceBurstPrefab);
        SetSerialized(scope, "enemyHitSparksPrefab", vfxAssets.enemyHitSparksPrefab);
        SetSerialized(scope, "knifeHitSparksPrefab", vfxAssets.knifeHitSparksPrefab);
        SetSerialized(scope, "enemyDeathBurstPrefab", vfxAssets.enemyDeathBurstPrefab);
        SetSerialized(scope, "xpPickupCollectPopPrefab", vfxAssets.xpPickupCollectPopPrefab);
        SetSerialized(scope, "playerDamageBurstPrefab", vfxAssets.playerDamageBurstPrefab);
        SetSerialized(scope, "levelUpBurstPrefab", vfxAssets.levelUpBurstPrefab);
        SetSerialized(scope, "playerJumpTakeoffPrefab", vfxAssets.playerJumpTakeoffPrefab);
        SetSerialized(scope, "playerJumpLandPrefab", vfxAssets.playerJumpLandPrefab);
        SetSerialized(scope, "projectileExplosionPrefab", vfxAssets.projectileExplosionPrefab);
        SetSerialized(scope, "worldAmbientDustPrefab", vfxAssets.worldAmbientDustPrefab);
        SetSerialized(scope, "hud", hud);
        SetSerialized(scope, "damageVignette", hud.GetComponent<DamageVignetteView>());
        SetSerialized(scope, "upgradeSelection", hud.GetComponent<UpgradeSelectionView>());
        SetSerialized(scope, "performerSelection", hud.GetComponent<PerformerSelectionView>());
        SetSerialized(scope, "shootFeedback", shootFeedback);
        SetSerialized(scope, "hitFeedback", hitFeedback);
        SetSerialized(scope, "playerDamageFeedback", playerDamageFeedback);

        EditorSceneManager.SaveScene(scene, ScenePath);
        ApplyBuildSettings();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"The Circussy One prototype scene rebuilt at {ScenePath}");
    }

    public static void ApplyBuildSettings()
    {
        var scenes = new List<EditorBuildSettingsScene>();
        AddSceneIfExists(scenes, TheCircussyOneAssetPaths.BootScenePath);
        AddSceneIfExists(scenes, TheCircussyOneAssetPaths.MainMenuScenePath);
        AddSceneIfExists(scenes, ScenePath);
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    private static void AddSceneIfExists(List<EditorBuildSettingsScene> scenes, string path)
    {
        if (File.Exists(path))
        {
            scenes.Add(new EditorBuildSettingsScene(path, true));
        }
    }

    [MenuItem("Tools/The Circussy One/Apply Grid Visual Config")]
    public static void ApplyGridVisualConfig()
    {
        TheCircussyOneGridVisualApplier.ApplyNow();
    }

    [MenuItem("Tools/The Circussy One/Apply Lighting Visual Config")]
    public static void ApplyLightingVisualConfig()
    {
        TheCircussyOneLightingVisualApplier.ApplyNow();
    }

    [MenuItem("Tools/The Circussy One/Apply Damage Feedback Visual Config")]
    public static void ApplyDamageFeedbackVisualConfig()
    {
        TheCircussyOneDamageFeedbackVisualApplier.ApplyNow();
    }

    [MenuItem("Tools/The Circussy One/Apply XP Gain Counter Visual Config")]
    public static void ApplyXpGainCounterVisualConfig()
    {
        TheCircussyOneXpGainCounterVisualApplier.ApplyNow();
    }

    [MenuItem("Tools/The Circussy One/Apply World Interaction Prompt Visual Config")]
    public static void ApplyWorldInteractionPromptVisualConfig()
    {
        TheCircussyOneWorldInteractionPromptVisualApplier.ApplyNow();
    }

    [MenuItem("Tools/The Circussy One/Apply Actor Motion Visual Config")]
    public static void ApplyActorMotionVisualConfig()
    {
        TheCircussyOneActorMotionVisualApplier.ApplyNow();
    }

    [MenuItem("Tools/The Circussy One/Apply Enemy Spawn Visual Config")]
    public static void ApplyEnemySpawnVisualConfig()
    {
        TheCircussyOneEnemySpawnVisualApplier.ApplyNow();
    }

    [MenuItem("Tools/The Circussy One/Apply VFX Visual Config")]
    public static void ApplyVfxVisualConfig()
    {
        TheCircussyOneVfxVisualApplier.ApplyNow();
    }

    [MenuItem("Tools/The Circussy One/Apply HUD Visual Config")]
    public static void ApplyHudVisualConfig()
    {
        TheCircussyOneHudVisualApplier.ApplyNow();
    }

    public static GameConfig GetOrCreateGameConfig()
    {
        return TheCircussyOneConfigRepository.GetOrCreateGameConfig();
    }

    public static RunScheduleConfig GetOrCreateRunScheduleConfig()
    {
        return TheCircussyOneConfigRepository.GetOrCreateRunScheduleConfig();
    }

    public static RunWorldGenerationConfig GetOrCreateRunWorldGenerationConfig()
    {
        return TheCircussyOneConfigRepository.GetOrCreateRunWorldGenerationConfig();
    }

    public static CameraConfig GetOrCreateCameraConfig()
    {
        return TheCircussyOneConfigRepository.GetOrCreateCameraConfig();
    }

    public static WeaponDefinition GetOrCreateCannonWeapon()
    {
        return TheCircussyOneConfigRepository.GetOrCreateCannonWeapon();
    }

    public static WeaponDefinition GetOrCreateKnifeFanWeapon()
    {
        return TheCircussyOneConfigRepository.GetOrCreateKnifeFanWeapon();
    }

    public static WeaponDefinition GetOrCreateSpotlightBoltWeapon()
    {
        return TheCircussyOneConfigRepository.GetOrCreateSpotlightBoltWeapon();
    }

    public static WeaponDefinition GetOrCreateFireHoopWeapon()
    {
        return TheCircussyOneConfigRepository.GetOrCreateFireHoopWeapon();
    }

    public static WeaponDefinition GetOrCreateJugglingBallWeapon()
    {
        return TheCircussyOneConfigRepository.GetOrCreateJugglingBallWeapon();
    }

    public static WeaponCatalog GetOrCreateWeaponCatalog()
    {
        return TheCircussyOneConfigRepository.GetOrCreateWeaponCatalog();
    }

    public static ItemDefinition GetOrCreateRubberSolesItem()
    {
        return TheCircussyOneConfigRepository.GetOrCreateRubberSolesItem();
    }

    public static ItemDefinition GetOrCreateSafetyPaddingItem()
    {
        return TheCircussyOneConfigRepository.GetOrCreateSafetyPaddingItem();
    }

    public static ItemDefinition GetOrCreateMagnetCharmItem()
    {
        return TheCircussyOneConfigRepository.GetOrCreateMagnetCharmItem();
    }

    public static ItemDefinition GetOrCreateLuckyCoinItem()
    {
        return TheCircussyOneConfigRepository.GetOrCreateLuckyCoinItem();
    }

    public static ItemDefinition GetOrCreateStudyNotesItem()
    {
        return TheCircussyOneConfigRepository.GetOrCreateStudyNotesItem();
    }

    public static ItemDefinition GetOrCreateTempoBraceletItem()
    {
        return TheCircussyOneConfigRepository.GetOrCreateTempoBraceletItem();
    }

    public static ItemDefinition GetOrCreatePowderFlaskItem()
    {
        return TheCircussyOneConfigRepository.GetOrCreatePowderFlaskItem();
    }

    public static ItemDefinition GetOrCreateSpringBootsItem()
    {
        return TheCircussyOneConfigRepository.GetOrCreateSpringBootsItem();
    }

    public static ItemDefinition GetOrCreateWideLensItem()
    {
        return TheCircussyOneConfigRepository.GetOrCreateWideLensItem();
    }

    public static ItemDefinition GetOrCreateLongFuseItem()
    {
        return TheCircussyOneConfigRepository.GetOrCreateLongFuseItem();
    }

    public static ItemCatalog GetOrCreateItemCatalog()
    {
        return TheCircussyOneConfigRepository.GetOrCreateItemCatalog();
    }

    public static ChestDefinition GetOrCreateOpenChest()
    {
        return TheCircussyOneConfigRepository.GetOrCreateOpenChest();
    }

    public static ChestDefinition GetOrCreateLockedChest()
    {
        return TheCircussyOneConfigRepository.GetOrCreateLockedChest();
    }

    public static ChestDefinition GetOrCreatePremiumChest()
    {
        return TheCircussyOneConfigRepository.GetOrCreatePremiumChest();
    }

    public static ChestDefinition GetOrCreateSpecialEnemyChest()
    {
        return TheCircussyOneConfigRepository.GetOrCreateSpecialEnemyChest();
    }

    public static ChestCatalog GetOrCreateChestCatalog()
    {
        return TheCircussyOneConfigRepository.GetOrCreateChestCatalog();
    }

    public static TicketDepositDefinition GetOrCreateSmallTicketStack()
    {
        return TheCircussyOneConfigRepository.GetOrCreateSmallTicketStack();
    }

    public static TicketDepositDefinition GetOrCreateTicketRoll()
    {
        return TheCircussyOneConfigRepository.GetOrCreateTicketRoll();
    }

    public static TicketDepositDefinition GetOrCreateJackpotCache()
    {
        return TheCircussyOneConfigRepository.GetOrCreateJackpotCache();
    }

    public static TicketDepositCatalog GetOrCreateTicketDepositCatalog()
    {
        return TheCircussyOneConfigRepository.GetOrCreateTicketDepositCatalog();
    }

    public static XpGemDefinition GetOrCreateBlueXpGem()
    {
        return TheCircussyOneConfigRepository.GetOrCreateBlueXpGem();
    }

    public static XpGemDefinition GetOrCreateGreenXpGem()
    {
        return TheCircussyOneConfigRepository.GetOrCreateGreenXpGem();
    }

    public static XpGemDefinition GetOrCreateRedXpGem()
    {
        return TheCircussyOneConfigRepository.GetOrCreateRedXpGem();
    }

    public static XpGemCatalog GetOrCreateXpGemCatalog()
    {
        return TheCircussyOneConfigRepository.GetOrCreateXpGemCatalog();
    }

    public static HealthPickupDefinition GetOrCreateTreat()
    {
        return TheCircussyOneConfigRepository.GetOrCreateTreat();
    }

    public static HealthPickupCatalog GetOrCreateHealthPickupCatalog()
    {
        return TheCircussyOneConfigRepository.GetOrCreateHealthPickupCatalog();
    }

    public static HealingPropDefinition GetOrCreateSnackBox()
    {
        return TheCircussyOneConfigRepository.GetOrCreateSnackBox();
    }

    public static HealingPropDefinition GetOrCreateSnackCart()
    {
        return TheCircussyOneConfigRepository.GetOrCreateSnackCart();
    }

    public static HealingPropCatalog GetOrCreateHealingPropCatalog()
    {
        return TheCircussyOneConfigRepository.GetOrCreateHealingPropCatalog();
    }

    public static WorldRewardPlacementCatalog GetOrCreateWorldRewardPlacementCatalog()
    {
        return TheCircussyOneConfigRepository.GetOrCreateWorldRewardPlacementCatalog();
    }

    public static EnemyDefinition GetOrCreateNormalEnemy()
    {
        return TheCircussyOneConfigRepository.GetOrCreateNormalEnemy();
    }

    public static EnemyCatalog GetOrCreateEnemyCatalog()
    {
        return TheCircussyOneConfigRepository.GetOrCreateEnemyCatalog();
    }

    public static EnemyDefinition GetOrCreateOpeningHeadlinerEnemy()
    {
        return TheCircussyOneConfigRepository.GetOrCreateOpeningHeadlinerEnemy();
    }

    public static HeadlinerDefinition GetOrCreateOpeningHeadliner()
    {
        return TheCircussyOneConfigRepository.GetOrCreateOpeningHeadliner();
    }

    public static HeadlinerCatalog GetOrCreateHeadlinerCatalog()
    {
        return TheCircussyOneConfigRepository.GetOrCreateHeadlinerCatalog();
    }

    public static UpgradeCatalog GetOrCreateUpgradeCatalog()
    {
        return TheCircussyOneConfigRepository.GetOrCreateUpgradeCatalog();
    }

    public static TalentDefinition GetOrCreateFootworkTalent()
    {
        return TheCircussyOneConfigRepository.GetOrCreateFootworkTalent();
    }

    public static TalentDefinition GetOrCreateStageStaminaTalent()
    {
        return TheCircussyOneConfigRepository.GetOrCreateStageStaminaTalent();
    }

    public static TalentDefinition GetOrCreateToughSkinTalent()
    {
        return TheCircussyOneConfigRepository.GetOrCreateToughSkinTalent();
    }

    public static TalentDefinition GetOrCreateCrowdFavoriteTalent()
    {
        return TheCircussyOneConfigRepository.GetOrCreateCrowdFavoriteTalent();
    }

    public static TalentDefinition GetOrCreateQuickHandsTalent()
    {
        return TheCircussyOneConfigRepository.GetOrCreateQuickHandsTalent();
    }

    public static TalentDefinition GetOrCreateSharpEyeTalent()
    {
        return TheCircussyOneConfigRepository.GetOrCreateSharpEyeTalent();
    }

    public static TalentDefinition GetOrCreateLongReachTalent()
    {
        return TheCircussyOneConfigRepository.GetOrCreateLongReachTalent();
    }

    public static TalentDefinition GetOrCreateLuckyBreakTalent()
    {
        return TheCircussyOneConfigRepository.GetOrCreateLuckyBreakTalent();
    }

    public static TalentDefinition GetOrCreateJugglerTalent()
    {
        return TheCircussyOneConfigRepository.GetOrCreateJugglerTalent();
    }

    public static TalentDefinition GetOrCreateCappaTalent()
    {
        return TheCircussyOneConfigRepository.GetOrCreateCappaTalent();
    }

    public static TalentDefinition GetOrCreateStrongmanTalent()
    {
        return TheCircussyOneConfigRepository.GetOrCreateStrongmanTalent();
    }

    public static TalentDefinition GetOrCreateAcrobatTalent()
    {
        return TheCircussyOneConfigRepository.GetOrCreateAcrobatTalent();
    }

    public static TalentDefinition GetOrCreateRingmasterTalent()
    {
        return TheCircussyOneConfigRepository.GetOrCreateRingmasterTalent();
    }

    public static TalentCatalog GetOrCreateTalentCatalog()
    {
        return TheCircussyOneConfigRepository.GetOrCreateTalentCatalog();
    }

    public static PerformerDefinition GetOrCreateJugglerPerformer()
    {
        return TheCircussyOneConfigRepository.GetOrCreateJugglerPerformer();
    }

    public static PerformerDefinition GetOrCreateCappaPerformer()
    {
        return TheCircussyOneConfigRepository.GetOrCreateCappaPerformer();
    }

    public static PerformerDefinition GetOrCreateStrongmanPerformer()
    {
        return TheCircussyOneConfigRepository.GetOrCreateStrongmanPerformer();
    }

    public static PerformerDefinition GetOrCreateAcrobatPerformer()
    {
        return TheCircussyOneConfigRepository.GetOrCreateAcrobatPerformer();
    }

    public static PerformerDefinition GetOrCreateRingmasterPerformer()
    {
        return TheCircussyOneConfigRepository.GetOrCreateRingmasterPerformer();
    }

    public static PerformerCatalog GetOrCreatePerformerCatalog()
    {
        return TheCircussyOneConfigRepository.GetOrCreatePerformerCatalog();
    }

    public static UpgradeDefinition GetOrCreateCannonTuningUpgrade()
    {
        return TheCircussyOneConfigRepository.GetOrCreateCannonTuningUpgrade();
    }

    public static UpgradeDefinition GetOrCreateKnifeFanTuningUpgrade()
    {
        return TheCircussyOneConfigRepository.GetOrCreateKnifeFanTuningUpgrade();
    }

    public static UpgradeDefinition GetOrCreateSpotlightBoltTuningUpgrade()
    {
        return TheCircussyOneConfigRepository.GetOrCreateSpotlightBoltTuningUpgrade();
    }

    public static UpgradeDefinition GetOrCreateFireHoopTuningUpgrade()
    {
        return TheCircussyOneConfigRepository.GetOrCreateFireHoopTuningUpgrade();
    }

    public static UpgradeDefinition GetOrCreateJugglingBallTuningUpgrade()
    {
        return TheCircussyOneConfigRepository.GetOrCreateJugglingBallTuningUpgrade();
    }

    public static GridVisualConfig GetOrCreateGridVisualConfig()
    {
        return TheCircussyOneConfigRepository.GetOrCreateGridVisualConfig();
    }

    public static LightingVisualConfig GetOrCreateLightingVisualConfig()
    {
        return TheCircussyOneConfigRepository.GetOrCreateLightingVisualConfig();
    }

    public static DamageFeedbackVisualConfig GetOrCreateDamageFeedbackVisualConfig()
    {
        return TheCircussyOneConfigRepository.GetOrCreateDamageFeedbackVisualConfig();
    }

    public static ActorMotionVisualConfig GetOrCreateActorMotionVisualConfig()
    {
        return TheCircussyOneConfigRepository.GetOrCreateActorMotionVisualConfig();
    }

    public static EnemySpawnVisualConfig GetOrCreateEnemySpawnVisualConfig()
    {
        return TheCircussyOneConfigRepository.GetOrCreateEnemySpawnVisualConfig();
    }

    public static VfxVisualConfig GetOrCreateVfxVisualConfig()
    {
        return TheCircussyOneConfigRepository.GetOrCreateVfxVisualConfig();
    }

    public static GameHapticsConfig GetOrCreateGameHapticsConfig()
    {
        return TheCircussyOneConfigRepository.GetOrCreateGameHapticsConfig();
    }

    public static HudVisualConfig GetOrCreateHudVisualConfig()
    {
        return TheCircussyOneConfigRepository.GetOrCreateHudVisualConfig();
    }

    public static XpGainCounterVisualConfig GetOrCreateXpGainCounterVisualConfig()
    {
        return TheCircussyOneConfigRepository.GetOrCreateXpGainCounterVisualConfig();
    }

    public static WorldInteractionPromptVisualConfig GetOrCreateWorldInteractionPromptVisualConfig()
    {
        return TheCircussyOneConfigRepository.GetOrCreateWorldInteractionPromptVisualConfig();
    }
























    public readonly struct GridVisualAssets
    {
        public readonly Material playerMaterial;
        public readonly Material enemyMaterial;
        public readonly Material projectileMaterial;
        public readonly Material pickupMaterial;
        public readonly Material floorMaterial;

        public GridVisualAssets(Material playerMaterial, Material enemyMaterial, Material projectileMaterial, Material pickupMaterial, Material floorMaterial)
        {
            this.playerMaterial = playerMaterial;
            this.enemyMaterial = enemyMaterial;
            this.projectileMaterial = projectileMaterial;
            this.pickupMaterial = pickupMaterial;
            this.floorMaterial = floorMaterial;
        }
    }

    public readonly struct LightingVisualAssets
    {
        public readonly Material contactShadowMaterial;
        public readonly VolumeProfile volumeProfile;

        public LightingVisualAssets(Material contactShadowMaterial, VolumeProfile volumeProfile)
        {
            this.contactShadowMaterial = contactShadowMaterial;
            this.volumeProfile = volumeProfile;
        }
    }

    public readonly struct VfxVisualAssets
    {
        public readonly Material alphaMaterial;
        public readonly Material additiveMaterial;
        public readonly ParticleSystem playerMoveDustPrefab;
        public readonly ParticleSystem playerJumpTrailPrefab;
        public readonly WorldAmbientDustView worldAmbientDustPrefab;
        public readonly FireHoopAreaTelegraphView fireHoopAreaTelegraphPrefab;
        public readonly ParticleEffectView projectileMuzzlePuffPrefab;
        public readonly ParticleEffectView projectileBounceBurstPrefab;
        public readonly ProjectileExplosionView projectileExplosionPrefab;
        public readonly ParticleEffectView enemyHitSparksPrefab;
        public readonly ParticleEffectView knifeHitSparksPrefab;
        public readonly ParticleEffectView enemyDeathBurstPrefab;
        public readonly ParticleSystem xpPickupAttractTrailPrefab;
        public readonly ParticleEffectView xpPickupCollectPopPrefab;
        public readonly ParticleEffectView playerDamageBurstPrefab;
        public readonly ParticleEffectView levelUpBurstPrefab;
        public readonly ParticleEffectView playerJumpTakeoffPrefab;
        public readonly ParticleEffectView playerJumpLandPrefab;

        public VfxVisualAssets(
            Material alphaMaterial,
            Material additiveMaterial,
            ParticleSystem playerMoveDustPrefab,
            ParticleSystem playerJumpTrailPrefab,
            WorldAmbientDustView worldAmbientDustPrefab,
            FireHoopAreaTelegraphView fireHoopAreaTelegraphPrefab,
            ParticleEffectView projectileMuzzlePuffPrefab,
            ParticleEffectView projectileBounceBurstPrefab,
            ProjectileExplosionView projectileExplosionPrefab,
            ParticleEffectView enemyHitSparksPrefab,
            ParticleEffectView knifeHitSparksPrefab,
            ParticleEffectView enemyDeathBurstPrefab,
            ParticleSystem xpPickupAttractTrailPrefab,
            ParticleEffectView xpPickupCollectPopPrefab,
            ParticleEffectView playerDamageBurstPrefab,
            ParticleEffectView levelUpBurstPrefab,
            ParticleEffectView playerJumpTakeoffPrefab,
            ParticleEffectView playerJumpLandPrefab)
        {
            this.alphaMaterial = alphaMaterial;
            this.additiveMaterial = additiveMaterial;
            this.playerMoveDustPrefab = playerMoveDustPrefab;
            this.playerJumpTrailPrefab = playerJumpTrailPrefab;
            this.worldAmbientDustPrefab = worldAmbientDustPrefab;
            this.fireHoopAreaTelegraphPrefab = fireHoopAreaTelegraphPrefab;
            this.projectileMuzzlePuffPrefab = projectileMuzzlePuffPrefab;
            this.projectileBounceBurstPrefab = projectileBounceBurstPrefab;
            this.projectileExplosionPrefab = projectileExplosionPrefab;
            this.enemyHitSparksPrefab = enemyHitSparksPrefab;
            this.knifeHitSparksPrefab = knifeHitSparksPrefab;
            this.enemyDeathBurstPrefab = enemyDeathBurstPrefab;
            this.xpPickupAttractTrailPrefab = xpPickupAttractTrailPrefab;
            this.xpPickupCollectPopPrefab = xpPickupCollectPopPrefab;
            this.playerDamageBurstPrefab = playerDamageBurstPrefab;
            this.levelUpBurstPrefab = levelUpBurstPrefab;
            this.playerJumpTakeoffPrefab = playerJumpTakeoffPrefab;
            this.playerJumpLandPrefab = playerJumpLandPrefab;
        }
    }


















































}
