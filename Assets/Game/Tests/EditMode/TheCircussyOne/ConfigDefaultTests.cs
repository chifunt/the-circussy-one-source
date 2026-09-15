using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Sirenix.OdinInspector;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UIElements;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;

public sealed class ConfigDefaultTests
{
    [Test]
    public void DamageFeedbackConfigMigratesNewEaseFieldsWithoutTouchingTunedValues()
    {
        var config = ScriptableObject.CreateInstance<DamageFeedbackVisualConfig>();
        config.baseWorldScale = 1.1f;
        config.enemyFlashEase = default;
        config.enemyHealthBarDeathFadeEase = default;
        config.enemyHealthBarDeathFadeSeconds = -1f;
        config.pickupAttractEase = default;
        config.pickupDropBurstEase = default;
        config.pickupSpawnDelaySeconds = -1f;
        config.pickupDropBurstDistance = -1f;
        config.pickupDropBurstSeconds = -1f;
        config.pickupDropArcHeight = -1f;
        config.pickupXpDropArcHeightMultiplier = -1f;
        config.pickupDropBounceHeight = -1f;
        config.pickupDropBounceCount = -1;
        config.pickupGroundClearance = -1f;
        config.pickupGroundProbeHeight = 0f;
        config.pickupGroundProbeDepth = 0f;
        config.pickupCollisionRadius = -1f;
        config.pickupVisualGroundPadding = -1f;
        config.pickupEnvironmentMask = 0;
        config.playerDamageCameraShakeSeconds = -1f;
        config.playerDamageCameraShakePositionAmplitude = -1f;
        config.playerDamageCameraShakeRotationDegrees = -1f;
        config.playerDamageCameraShakeFrequency = 0f;
        config.playerDamageCameraShakeFalloffEase = default;
        config.playerDamageVignetteFlashSeconds = 0f;
        config.playerDamageVignetteMaxOpacity = 2f;
        config.playerDamageVignetteDarkMaxOpacity = 2f;
        config.playerDamageVignetteDamageToIntensityScale = 0f;
        config.playerDamageVignetteAccumulationCap = 0f;
        config.playerDamageVignetteGrainStrength = 2f;
        config.playerDamageVignetteSplotchStrength = 2f;
        config.playerDamageVignetteRadius = -1f;
        config.playerDamageVignetteSoftness = -1f;
        config.playerDamageVignetteFadeEase = default;
        config.playerHealingVignetteFlashSeconds = 0f;
        config.playerHealingVignetteMaxOpacity = 2f;
        config.playerHealingVignetteDarkMaxOpacity = 2f;
        config.playerHealingVignetteHealToIntensityScale = 0f;
        config.playerHealingVignetteAccumulationCap = 0f;
        config.playerHealingVignetteGrainStrength = 2f;
        config.playerHealingVignetteSplotchStrength = 2f;
        config.playerHealingVignetteRadius = -1f;
        config.playerHealingVignetteSoftness = -1f;
        config.playerHealingVignetteFadeEase = default;

        bool changed = config.EnsureReadableDefaults();

        Assert.That(changed, Is.True);
        Assert.That(config.baseWorldScale, Is.EqualTo(1.1f));
        Assert.That(config.enemyFlashEase.preset, Is.EqualTo(EasePreset.OutQuad));
        Assert.That(config.enemyHealthBarDeathFadeEase.preset, Is.EqualTo(EasePreset.OutQuad));
        Assert.That(config.enemyHealthBarDeathFadeSeconds, Is.EqualTo(0.07f));
        Assert.That(config.pickupAttractEase.preset, Is.EqualTo(EasePreset.InBack));
        Assert.That(config.pickupDropBurstEase.preset, Is.EqualTo(EasePreset.OutCubic));
        Assert.That(config.pickupSpawnDelaySeconds, Is.EqualTo(0.15f));
        Assert.That(config.pickupDropBurstDistance, Is.EqualTo(1.15f));
        Assert.That(config.pickupDropBurstSeconds, Is.EqualTo(0.38f));
        Assert.That(config.pickupDropArcHeight, Is.EqualTo(1.15f));
        Assert.That(config.pickupXpDropArcHeightMultiplier, Is.EqualTo(1.65f));
        Assert.That(config.pickupDropBounceHeight, Is.EqualTo(0.18f));
        Assert.That(config.pickupDropBounceCount, Is.EqualTo(1));
        Assert.That(config.pickupGroundClearance, Is.EqualTo(0.14f));
        Assert.That(config.pickupGroundProbeHeight, Is.EqualTo(4f));
        Assert.That(config.pickupGroundProbeDepth, Is.EqualTo(10f));
        Assert.That(config.pickupCollisionRadius, Is.EqualTo(0.16f));
        Assert.That(config.pickupVisualGroundPadding, Is.EqualTo(0.04f));
        Assert.That(config.pickupEnvironmentMask.value, Is.EqualTo(~0));
        Assert.That(config.damageNumberFont, Is.Null);
        Assert.That(config.sdfGradientScale, Is.EqualTo(12f));
        Assert.That(config.sdfSharpness, Is.EqualTo(0.35f));
        Assert.That(config.enemyFlashEmissionStrength, Is.EqualTo(4f));
        Assert.That(config.playerDamageCameraShakeSeconds, Is.EqualTo(0.16f));
        Assert.That(config.playerDamageCameraShakePositionAmplitude, Is.EqualTo(0.12f));
        Assert.That(config.playerDamageCameraShakeRotationDegrees, Is.EqualTo(0.45f));
        Assert.That(config.playerDamageCameraShakeFrequency, Is.EqualTo(18f));
        Assert.That(config.playerDamageCameraShakeFalloffEase.preset, Is.EqualTo(EasePreset.OutQuad));
        Assert.That(config.playerDamageVignetteEnabled, Is.True);
        Assert.That(config.playerDamageVignetteFlashSeconds, Is.EqualTo(0.58f));
        Assert.That(config.playerDamageVignetteMaxOpacity, Is.EqualTo(0.32f));
        Assert.That(config.playerDamageVignetteDarkMaxOpacity, Is.EqualTo(0.42f));
        Assert.That(config.playerDamageVignetteDamageToIntensityScale, Is.EqualTo(10f));
        Assert.That(config.playerDamageVignetteAccumulationCap, Is.EqualTo(1.25f));
        Assert.That(config.playerDamageVignetteGrainStrength, Is.EqualTo(0.26f));
        Assert.That(config.playerDamageVignetteSplotchStrength, Is.EqualTo(0.32f));
        Assert.That(config.playerDamageVignetteRadius, Is.EqualTo(0.78f));
        Assert.That(config.playerDamageVignetteSoftness, Is.EqualTo(0.28f));
        Assert.That(config.playerDamageVignetteFadeEase.preset, Is.EqualTo(EasePreset.OutCubic));
        Assert.That(config.playerHealingVignetteEnabled, Is.True);
        Assert.That(config.playerHealingVignetteFlashSeconds, Is.EqualTo(0.46f));
        Assert.That(config.playerHealingVignetteMaxOpacity, Is.EqualTo(0.22f));
        Assert.That(config.playerHealingVignetteDarkMaxOpacity, Is.EqualTo(0.10f));
        Assert.That(config.playerHealingVignetteHealToIntensityScale, Is.EqualTo(4f));
        Assert.That(config.playerHealingVignetteAccumulationCap, Is.EqualTo(0.90f));
        Assert.That(config.playerHealingVignetteGrainStrength, Is.EqualTo(0.12f));
        Assert.That(config.playerHealingVignetteSplotchStrength, Is.EqualTo(0.16f));
        Assert.That(config.playerHealingVignetteRadius, Is.EqualTo(0.80f));
        Assert.That(config.playerHealingVignetteSoftness, Is.EqualTo(0.30f));
        Assert.That(config.playerHealingVignetteFadeEase.preset, Is.EqualTo(EasePreset.OutCubic));

        UnityEngine.Object.DestroyImmediate(config);
    }

    [Test]
    public void DamageFeedbackConfigNoLongerRewritesLegacyBroadDamageVignetteValues()
    {
        var config = ScriptableObject.CreateInstance<DamageFeedbackVisualConfig>();
        config.playerDamageVignetteMaxOpacity = 0.52f;
        config.playerDamageVignetteRadius = 0.43f;
        config.playerDamageVignetteSoftness = 0.34f;

        Assert.That(config.EnsureReadableDefaults(), Is.False);

        Assert.That(config.playerDamageVignetteMaxOpacity, Is.EqualTo(0.52f));
        Assert.That(config.playerDamageVignetteRadius, Is.EqualTo(0.43f));
        Assert.That(config.playerDamageVignetteSoftness, Is.EqualTo(0.34f));

        UnityEngine.Object.DestroyImmediate(config);
    }

    [Test]
    public void DamageFeedbackConfigPreservesTunedDamageVignetteValues()
    {
        var config = ScriptableObject.CreateInstance<DamageFeedbackVisualConfig>();
        config.playerDamageVignetteMaxOpacity = 0.25f;
        config.playerDamageVignetteDarkMaxOpacity = 0.36f;
        config.playerDamageVignetteRadius = 0.86f;
        config.playerDamageVignetteSoftness = 0.22f;

        Assert.That(config.EnsureReadableDefaults(), Is.False);

        Assert.That(config.playerDamageVignetteMaxOpacity, Is.EqualTo(0.25f));
        Assert.That(config.playerDamageVignetteDarkMaxOpacity, Is.EqualTo(0.36f));
        Assert.That(config.playerDamageVignetteRadius, Is.EqualTo(0.86f));
        Assert.That(config.playerDamageVignetteSoftness, Is.EqualTo(0.22f));

        UnityEngine.Object.DestroyImmediate(config);
    }

    [Test]
    public void GameAndCameraConfigsMigrateSmoothingDefaults()
    {
        var game = ScriptableObject.CreateInstance<GameConfig>();
        var camera = ScriptableObject.CreateInstance<CameraConfig>();
        game.difficultyRampEase = default;
        game.playerFacingSmoothingEase = default;
        game.enemyTurnDegreesPerSecond = 0f;
        game.playerGroundProbeDistance = -1f;
        game.playerVisualGroundProbeMaxSlope = -1f;
        game.playerAirborneAnimationMinAirTime = -1f;
        camera.followSmoothingEase = default;
        camera.lookAheadPitchEase = default;
        camera.farClipPlane = 240f;

        Assert.That(game.EnsureWorkflowDefaults(), Is.True);
        Assert.That(camera.EnsureWorkflowDefaults(), Is.True);
        Assert.That(game.difficultyRampEase.preset, Is.EqualTo(EasePreset.OutCubic));
        Assert.That(game.playerFacingSmoothingEase.preset, Is.EqualTo(EasePreset.Exponential));
        Assert.That(game.enemyTurnDegreesPerSecond, Is.EqualTo(540f));
        Assert.That(game.playerGroundProbeDistance, Is.EqualTo(0.35f));
        Assert.That(game.playerVisualGroundProbeMaxSlope, Is.EqualTo(75f));
        Assert.That(game.playerAirborneAnimationMinAirTime, Is.EqualTo(0.14f));
        Assert.That(camera.followSmoothingEase.preset, Is.EqualTo(EasePreset.Exponential));
        Assert.That(camera.lookAheadPitchEase.preset, Is.EqualTo(EasePreset.Linear));
        Assert.That(camera.farClipPlane, Is.EqualTo(2500f));

        UnityEngine.Object.DestroyImmediate(game);
        UnityEngine.Object.DestroyImmediate(camera);
    }

    [Test]
    public void RunWorldGenerationProfilePlateauRadiusRangeDoesNotRecurseWhenValuesAreInverted()
    {
        var profile = new RunWorldGenerationProfile
        {
            plateauRadiusMinPercent = 0.55f,
            plateauRadiusMaxPercent = 0.08f
        };

        Assert.That(profile.PlateauRadiusMinPercent, Is.EqualTo(0.08f));
        Assert.That(profile.PlateauRadiusMaxPercent, Is.EqualTo(0.5f));
    }

    [Test]
    public void ActorMotionConfigMigratesDefaultsWithoutReenablingDisabledPulses()
    {
        var config = ScriptableObject.CreateInstance<ActorMotionVisualConfig>();
        config.playerIdle.secondsPerCycle = 0f;
        config.enemyMove.amplitude = 0.08f;
        config.enemyHitPulse.enabled = false;
        config.enemyHitPulse.seconds = 0f;
        config.speedSmoothingEase = default;
        config.jumpHoldMaxSeconds = -1f;
        config.jumpHoldXZScale = 0f;
        config.jumpHoldYScale = 0f;
        config.jumpHoldReleaseSeconds = -1f;
        config.jumpHoldEase = default;
        config.playerInclineTiltMaxDegrees = -1f;
        config.enemyInclineTiltMaxDegrees = 99f;
        config.inclineTiltSmoothingSharpness = -1f;
        config.enemyInclineGroundProbeDistance = -1f;

        bool changed = config.EnsureWorkflowDefaults();

        Assert.That(changed, Is.True);
        Assert.That(config.playerIdle.secondsPerCycle, Is.EqualTo(1.35f));
        Assert.That(config.enemyMove.amplitude, Is.EqualTo(0.08f));
        Assert.That(config.enemyHitPulse.enabled, Is.False);
        Assert.That(config.enemyHitPulse.seconds, Is.EqualTo(0.16f));
        Assert.That(config.speedSmoothingEase.preset, Is.EqualTo(EasePreset.Exponential));
        Assert.That(config.jumpHoldMaxSeconds, Is.EqualTo(0.22f));
        Assert.That(config.jumpHoldXZScale, Is.EqualTo(0.92f));
        Assert.That(config.jumpHoldYScale, Is.EqualTo(1.10f));
        Assert.That(config.jumpHoldReleaseSeconds, Is.EqualTo(0.10f));
        Assert.That(config.jumpHoldEase.preset, Is.EqualTo(EasePreset.OutCubic));
        Assert.That(config.playerInclineTiltMaxDegrees, Is.EqualTo(18f));
        Assert.That(config.enemyInclineTiltMaxDegrees, Is.EqualTo(14f));
        Assert.That(config.inclineTiltSmoothingSharpness, Is.EqualTo(18f));
        Assert.That(config.enemyInclineGroundProbeDistance, Is.EqualTo(0.8f));

        UnityEngine.Object.DestroyImmediate(config);
    }

    [Test]
    public void EnemySpawnVisualConfigMigratesTerrainPlacementDefaults()
    {
        var config = ScriptableObject.CreateInstance<EnemySpawnVisualConfig>();
        config.surfaceProbeHeight = 0f;
        config.surfaceProbeDepth = 0f;
        config.surfaceMask = 0;

        bool changed = config.EnsureWorkflowDefaults();

        Assert.That(changed, Is.True);
        Assert.That(config.surfaceProbeHeight, Is.EqualTo(12f));
        Assert.That(config.surfaceProbeDepth, Is.EqualTo(24f));
        Assert.That(config.surfaceMask.value, Is.EqualTo(~0));

        UnityEngine.Object.DestroyImmediate(config);
    }

    [Test]
    public void HudVisualConfigMigratesReadableDefaultsWithoutOverwritingLabelText()
    {
        var config = ScriptableObject.CreateInstance<HudVisualConfig>();
        config.referenceDpi = -10f;
        config.fallbackDpi = 0f;
        config.referenceResolution = new Vector2(16f, 12f);
        config.globalScale = 12f;
        config.xpTrackHeight = 0f;
        config.healthWidth = -1f;
        config.killsLabelText = "KOs";
        config.previewEnabled = true;
        config.autoPreviewOnChange = true;
        config.previewHealth = -5;
        config.previewMaxHealth = 0;
        config.previewExperience = 99;
        config.previewExperienceTarget = 0;
        config.previewLevel = 0;
        config.previewKills = -1;
        config.previewRunSeconds = -10f;

        bool changed = config.EnsureReadableDefaults();

        Assert.That(changed, Is.True);
        Assert.That(config.scaleMode, Is.EqualTo(PanelScaleMode.ConstantPhysicalSize));
        Assert.That(config.referenceDpi, Is.EqualTo(96f));
        Assert.That(config.fallbackDpi, Is.EqualTo(96f));
        Assert.That(config.referenceResolution.x, Is.EqualTo(1200f));
        Assert.That(config.referenceResolution.y, Is.EqualTo(800f));
        Assert.That(config.globalScale, Is.EqualTo(1f));
        Assert.That(config.xpTrackHeight, Is.EqualTo(16f));
        Assert.That(config.healthWidth, Is.EqualTo(210f));
        Assert.That(config.killsLabelText, Is.EqualTo("KOs"));
        Assert.That(config.previewEnabled, Is.True);
        Assert.That(config.autoPreviewOnChange, Is.True);
        Assert.That(config.previewHealth, Is.EqualTo(78));
        Assert.That(config.previewMaxHealth, Is.EqualTo(100));
        Assert.That(config.previewExperience, Is.EqualTo(6));
        Assert.That(config.previewExperienceTarget, Is.EqualTo(10));
        Assert.That(config.previewLevel, Is.EqualTo(3));
        Assert.That(config.previewKills, Is.EqualTo(42));
        Assert.That(config.previewRunSeconds, Is.EqualTo(154f));

        UnityEngine.Object.DestroyImmediate(config);
    }

    [Test]
    public void HudPreviewPresetsProduceExpectedSnapshots()
    {
        var config = ScriptableObject.CreateInstance<HudVisualConfig>();
        config.previewHealth = 11;
        config.previewMaxHealth = 44;
        config.previewExperience = 3;
        config.previewExperienceTarget = 8;
        config.previewLevel = 7;
        config.previewKills = 55;
        config.previewRunSeconds = 91f;
        config.previewGameOverVisible = true;

        AssertHudSnapshot(config.GetPreviewSnapshot(HudPreviewPreset.Normal), 100, 100, 2, 10, 1, 0, 23f, false);
        AssertHudSnapshot(config.GetPreviewSnapshot(HudPreviewPreset.Damaged), 43, 100, 5, 10, 2, 18, 154f, false);
        AssertHudSnapshot(config.GetPreviewSnapshot(HudPreviewPreset.Leveling), 86, 100, 9, 10, 4, 72, 468f, false);
        AssertHudSnapshot(config.GetPreviewSnapshot(HudPreviewPreset.GameOver), 0, 100, 6, 10, 5, 103, 632f, true);
        AssertHudSnapshot(config.GetPreviewSnapshot(HudPreviewPreset.Custom), 11, 44, 3, 8, 7, 55, 91f, true);

        UnityEngine.Object.DestroyImmediate(config);
    }

    [Test]
    public void AllConfigAssetsLoadFromCanonicalPaths()
    {
        TheCircussyOneSceneBuilder.GetOrCreateRunWorldGenerationConfig();

        Assert.That(AssetDatabase.LoadAssetAtPath<GameConfig>("Assets/Game/ScriptableObjects/Balance/GameConfig.asset"), Is.Not.Null);
        Assert.That(AssetDatabase.LoadAssetAtPath<WeaponDefinition>(TheCircussyOneAssetPaths.CannonWeaponPath), Is.Not.Null);
        Assert.That(AssetDatabase.LoadAssetAtPath<WeaponDefinition>(TheCircussyOneAssetPaths.KnifeFanWeaponPath), Is.Not.Null);
        Assert.That(AssetDatabase.LoadAssetAtPath<WeaponDefinition>(TheCircussyOneAssetPaths.SpotlightBoltWeaponPath), Is.Not.Null);
        Assert.That(AssetDatabase.LoadAssetAtPath<WeaponDefinition>(TheCircussyOneAssetPaths.FireHoopWeaponPath), Is.Not.Null);
        Assert.That(AssetDatabase.LoadAssetAtPath<WeaponDefinition>(TheCircussyOneAssetPaths.JugglingBallWeaponPath), Is.Not.Null);
        WeaponCatalog weaponCatalog = AssetDatabase.LoadAssetAtPath<WeaponCatalog>(TheCircussyOneAssetPaths.WeaponCatalogPath);
        Assert.That(weaponCatalog, Is.Not.Null);
        Assert.That(weaponCatalog.StartingWeapons.Count, Is.EqualTo(1));
        Assert.That(weaponCatalog.AvailableWeapons.Count, Is.GreaterThanOrEqualTo(5));
        Assert.That(AssetDatabase.LoadAssetAtPath<ItemDefinition>(TheCircussyOneAssetPaths.RubberSolesItemPath), Is.Not.Null);
        Assert.That(AssetDatabase.LoadAssetAtPath<ItemDefinition>(TheCircussyOneAssetPaths.SafetyPaddingItemPath), Is.Not.Null);
        Assert.That(AssetDatabase.LoadAssetAtPath<ItemDefinition>(TheCircussyOneAssetPaths.MagnetCharmItemPath), Is.Not.Null);
        Assert.That(AssetDatabase.LoadAssetAtPath<ItemDefinition>(TheCircussyOneAssetPaths.LuckyCoinItemPath), Is.Not.Null);
        Assert.That(AssetDatabase.LoadAssetAtPath<ItemDefinition>(TheCircussyOneAssetPaths.StudyNotesItemPath), Is.Not.Null);
        Assert.That(AssetDatabase.LoadAssetAtPath<ItemDefinition>(TheCircussyOneAssetPaths.TempoBraceletItemPath), Is.Not.Null);
        Assert.That(AssetDatabase.LoadAssetAtPath<ItemDefinition>(TheCircussyOneAssetPaths.PowderFlaskItemPath), Is.Not.Null);
        Assert.That(AssetDatabase.LoadAssetAtPath<ItemDefinition>(TheCircussyOneAssetPaths.SpringBootsItemPath), Is.Not.Null);
        Assert.That(AssetDatabase.LoadAssetAtPath<ItemDefinition>(TheCircussyOneAssetPaths.WideLensItemPath), Is.Not.Null);
        Assert.That(AssetDatabase.LoadAssetAtPath<ItemDefinition>(TheCircussyOneAssetPaths.LongFuseItemPath), Is.Not.Null);
        ItemCatalog itemCatalog = AssetDatabase.LoadAssetAtPath<ItemCatalog>(TheCircussyOneAssetPaths.ItemCatalogPath);
        Assert.That(itemCatalog, Is.Not.Null);
        Assert.That(itemCatalog.Items.Count, Is.GreaterThanOrEqualTo(10));
        Assert.That(AssetDatabase.LoadAssetAtPath<ChestDefinition>(TheCircussyOneAssetPaths.OpenChestPath), Is.Not.Null);
        Assert.That(AssetDatabase.LoadAssetAtPath<ChestDefinition>(TheCircussyOneAssetPaths.LockedChestPath), Is.Not.Null);
        Assert.That(AssetDatabase.LoadAssetAtPath<ChestDefinition>(TheCircussyOneAssetPaths.PremiumChestPath), Is.Not.Null);
        Assert.That(AssetDatabase.LoadAssetAtPath<ChestDefinition>(TheCircussyOneAssetPaths.SpecialEnemyChestPath), Is.Not.Null);
        ChestCatalog chestCatalog = AssetDatabase.LoadAssetAtPath<ChestCatalog>(TheCircussyOneAssetPaths.ChestCatalogPath);
        Assert.That(chestCatalog, Is.Not.Null);
        Assert.That(chestCatalog.Chests.Count, Is.GreaterThanOrEqualTo(4));
        Assert.That(AssetDatabase.LoadAssetAtPath<TicketDepositDefinition>(TheCircussyOneAssetPaths.SmallTicketStackPath), Is.Not.Null);
        Assert.That(AssetDatabase.LoadAssetAtPath<TicketDepositDefinition>(TheCircussyOneAssetPaths.TicketRollPath), Is.Not.Null);
        Assert.That(AssetDatabase.LoadAssetAtPath<TicketDepositDefinition>(TheCircussyOneAssetPaths.JackpotCachePath), Is.Not.Null);
        TicketDepositCatalog ticketDepositCatalog = AssetDatabase.LoadAssetAtPath<TicketDepositCatalog>(TheCircussyOneAssetPaths.TicketDepositCatalogPath);
        Assert.That(ticketDepositCatalog, Is.Not.Null);
        Assert.That(ticketDepositCatalog.Deposits.Count, Is.GreaterThanOrEqualTo(3));
        WorldRewardPlacementCatalog worldRewardPlacementCatalog = AssetDatabase.LoadAssetAtPath<WorldRewardPlacementCatalog>(TheCircussyOneAssetPaths.WorldRewardPlacementCatalogPath);
        Assert.That(worldRewardPlacementCatalog, Is.Not.Null);
        Assert.That(worldRewardPlacementCatalog.Placements.Count, Is.GreaterThanOrEqualTo(12));
        RunWorldGenerationConfig worldGenerationConfig = AssetDatabase.LoadAssetAtPath<RunWorldGenerationConfig>(TheCircussyOneAssetPaths.RunWorldGenerationConfigPath);
        Assert.That(worldGenerationConfig, Is.Not.Null);
        Assert.That(worldGenerationConfig.Profiles.Count, Is.GreaterThanOrEqualTo(3));
        Assert.That(AssetDatabase.LoadAssetAtPath<XpGemDefinition>(TheCircussyOneAssetPaths.BlueXpGemPath), Is.Not.Null);
        Assert.That(AssetDatabase.LoadAssetAtPath<XpGemDefinition>(TheCircussyOneAssetPaths.GreenXpGemPath), Is.Not.Null);
        Assert.That(AssetDatabase.LoadAssetAtPath<XpGemDefinition>(TheCircussyOneAssetPaths.RedXpGemPath), Is.Not.Null);
        Assert.That(AssetDatabase.LoadAssetAtPath<XpGemCatalog>(TheCircussyOneAssetPaths.XpGemCatalogPath), Is.Not.Null);
        foreach (string path in TheCircussyOneAssetPaths.WeaponUpgradePaths)
        {
            Assert.That(AssetDatabase.LoadAssetAtPath<UpgradeDefinition>(path), Is.Not.Null, path);
        }

        UpgradeCatalog upgradeCatalog = AssetDatabase.LoadAssetAtPath<UpgradeCatalog>(TheCircussyOneAssetPaths.UpgradeCatalogPath);
        Assert.That(upgradeCatalog, Is.Not.Null);
        Assert.That(upgradeCatalog.Upgrades.Count, Is.GreaterThanOrEqualTo(TheCircussyOneAssetPaths.WeaponUpgradePaths.Length));
        Assert.That(AssetDatabase.LoadAssetAtPath<TalentDefinition>(TheCircussyOneAssetPaths.FootworkTalentPath), Is.Not.Null);
        Assert.That(AssetDatabase.LoadAssetAtPath<TalentDefinition>(TheCircussyOneAssetPaths.StageStaminaTalentPath), Is.Not.Null);
        Assert.That(AssetDatabase.LoadAssetAtPath<TalentDefinition>(TheCircussyOneAssetPaths.ToughSkinTalentPath), Is.Not.Null);
        Assert.That(AssetDatabase.LoadAssetAtPath<TalentDefinition>(TheCircussyOneAssetPaths.CrowdFavoriteTalentPath), Is.Not.Null);
        Assert.That(AssetDatabase.LoadAssetAtPath<TalentDefinition>(TheCircussyOneAssetPaths.QuickHandsTalentPath), Is.Not.Null);
        Assert.That(AssetDatabase.LoadAssetAtPath<TalentDefinition>(TheCircussyOneAssetPaths.SharpEyeTalentPath), Is.Not.Null);
        Assert.That(AssetDatabase.LoadAssetAtPath<TalentDefinition>(TheCircussyOneAssetPaths.LongReachTalentPath), Is.Not.Null);
        Assert.That(AssetDatabase.LoadAssetAtPath<TalentDefinition>(TheCircussyOneAssetPaths.LuckyBreakTalentPath), Is.Not.Null);
        Assert.That(AssetDatabase.LoadAssetAtPath<TalentDefinition>(TheCircussyOneAssetPaths.BiggerPropsTalentPath), Is.Not.Null);
        Assert.That(AssetDatabase.LoadAssetAtPath<TalentDefinition>(TheCircussyOneAssetPaths.LongerActTalentPath), Is.Not.Null);
        Assert.That(AssetDatabase.LoadAssetAtPath<TalentDefinition>(TheCircussyOneAssetPaths.MagneticApplauseTalentPath), Is.Not.Null);
        Assert.That(AssetDatabase.LoadAssetAtPath<TalentDefinition>(TheCircussyOneAssetPaths.SpringboardTalentPath), Is.Not.Null);
        Assert.That(AssetDatabase.LoadAssetAtPath<TalentDefinition>(TheCircussyOneAssetPaths.JugglerTalentPath), Is.Not.Null);
        Assert.That(AssetDatabase.LoadAssetAtPath<TalentDefinition>(TheCircussyOneAssetPaths.CappaTalentPath), Is.Not.Null);
        Assert.That(AssetDatabase.LoadAssetAtPath<TalentDefinition>(TheCircussyOneAssetPaths.StrongmanTalentPath), Is.Not.Null);
        Assert.That(AssetDatabase.LoadAssetAtPath<TalentDefinition>(TheCircussyOneAssetPaths.AcrobatTalentPath), Is.Not.Null);
        Assert.That(AssetDatabase.LoadAssetAtPath<TalentDefinition>(TheCircussyOneAssetPaths.RingmasterTalentPath), Is.Not.Null);
        Assert.That(AssetDatabase.LoadAssetAtPath<TalentDefinition>(TheCircussyOneAssetPaths.HoopFlowTalentPath), Is.Not.Null);
        Assert.That(AssetDatabase.LoadAssetAtPath<TalentDefinition>(TheCircussyOneAssetPaths.EncoreBounceTalentPath), Is.Not.Null);
        TalentCatalog talentCatalog = AssetDatabase.LoadAssetAtPath<TalentCatalog>(TheCircussyOneAssetPaths.TalentCatalogPath);
        Assert.That(talentCatalog, Is.Not.Null);
        Assert.That(talentCatalog.Talents.Count, Is.EqualTo(47));
        Assert.That(AssetDatabase.LoadAssetAtPath<PerformerDefinition>(TheCircussyOneAssetPaths.JugglerPerformerPath), Is.Not.Null);
        Assert.That(AssetDatabase.LoadAssetAtPath<PerformerDefinition>(TheCircussyOneAssetPaths.CappaPerformerPath), Is.Not.Null);
        Assert.That(AssetDatabase.LoadAssetAtPath<PerformerDefinition>(TheCircussyOneAssetPaths.StrongmanPerformerPath), Is.Not.Null);
        Assert.That(AssetDatabase.LoadAssetAtPath<PerformerDefinition>(TheCircussyOneAssetPaths.AcrobatPerformerPath), Is.Not.Null);
        Assert.That(AssetDatabase.LoadAssetAtPath<PerformerDefinition>(TheCircussyOneAssetPaths.RingmasterPerformerPath), Is.Not.Null);
        Assert.That(AssetDatabase.LoadAssetAtPath<PerformerDefinition>(TheCircussyOneAssetPaths.SolaHoopDancerPerformerPath), Is.Not.Null);
        Assert.That(AssetDatabase.LoadAssetAtPath<PerformerDefinition>(TheCircussyOneAssetPaths.BibiBallJugglerPerformerPath), Is.Not.Null);
        PerformerCatalog performerCatalog = AssetDatabase.LoadAssetAtPath<PerformerCatalog>(TheCircussyOneAssetPaths.PerformerCatalogPath);
        Assert.That(performerCatalog, Is.Not.Null);
        Assert.That(performerCatalog.Performers.Count, Is.EqualTo(7));
        Assert.That(AssetDatabase.LoadAssetAtPath<CameraConfig>("Assets/Game/ScriptableObjects/Camera/CameraConfig.asset"), Is.Not.Null);
        Assert.That(AssetDatabase.LoadAssetAtPath<DamageFeedbackVisualConfig>("Assets/Game/ScriptableObjects/Visuals/DamageFeedbackVisualConfig.asset"), Is.Not.Null);
        Assert.That(AssetDatabase.LoadAssetAtPath<ActorMotionVisualConfig>("Assets/Game/ScriptableObjects/Visuals/ActorMotionVisualConfig.asset"), Is.Not.Null);
        Assert.That(AssetDatabase.LoadAssetAtPath<EnemySpawnVisualConfig>("Assets/Game/ScriptableObjects/Visuals/EnemySpawnVisualConfig.asset"), Is.Not.Null);
        Assert.That(AssetDatabase.LoadAssetAtPath<VfxVisualConfig>("Assets/Game/ScriptableObjects/Visuals/VfxVisualConfig.asset"), Is.Not.Null);
        Assert.That(AssetDatabase.LoadAssetAtPath<HudVisualConfig>("Assets/Game/ScriptableObjects/Visuals/HudVisualConfig.asset"), Is.Not.Null);
        Assert.That(AssetDatabase.LoadAssetAtPath<XpGainCounterVisualConfig>("Assets/Game/ScriptableObjects/Visuals/XpGainCounterVisualConfig.asset"), Is.Not.Null);
        Assert.That(AssetDatabase.LoadAssetAtPath<GridVisualConfig>("Assets/Game/ScriptableObjects/Visuals/GridVisualConfig.asset"), Is.Not.Null);
        Assert.That(AssetDatabase.LoadAssetAtPath<LightingVisualConfig>("Assets/Game/ScriptableObjects/Visuals/LightingVisualConfig.asset"), Is.Not.Null);
    }

    [Test]
    public void XpGainCounterConfigMigratesWorkflowDefaults()
    {
        var config = ScriptableObject.CreateInstance<XpGainCounterVisualConfig>();
        config.holdSeconds = -1f;
        config.fadeSeconds = 0f;
        config.popReturnSeconds = 0f;
        config.baseWorldScale = 0f;
        config.normalScale = 0f;
        config.popScale = 0f;
        config.popEase = default;
        config.fadeEase = default;

        bool changed = config.EnsureWorkflowDefaults();

        Assert.That(changed, Is.True);
        Assert.That(config.holdSeconds, Is.EqualTo(1f));
        Assert.That(config.fadeSeconds, Is.EqualTo(0.5f));
        Assert.That(config.popReturnSeconds, Is.EqualTo(0.18f));
        Assert.That(config.baseWorldScale, Is.EqualTo(0.78f));
        Assert.That(config.normalScale, Is.EqualTo(1f));
        Assert.That(config.popScale, Is.EqualTo(1.35f));
        Assert.That(config.popEase.preset, Is.EqualTo(EasePreset.OutBack));
        Assert.That(config.fadeEase.preset, Is.EqualTo(EasePreset.InCubic));

        UnityEngine.Object.DestroyImmediate(config);
    }

    [Test]
    public void WorldInteractionPromptConfigMigratesWorkflowDefaults()
    {
        var config = ScriptableObject.CreateInstance<WorldInteractionPromptVisualConfig>();
        config.fontSize = 0f;
        config.sdfGradientScale = 0f;
        config.sdfWeightNormal = -1f;
        config.sdfWeightBold = -1f;
        config.glyphRadius = 0f;
        config.progressBarWidth = 0f;
        config.progressBarHeight = 0f;
        config.showSeconds = 0f;
        config.hideSeconds = 0f;
        config.baseWorldScale = 0f;
        config.targetScale = 0f;
        config.showEase = default;
        config.hideEase = default;
        config.rowYOffset = float.NaN;
        config.glyphTextGap = -1f;
        config.labelMaxWidth = 0f;
        config.collectingLabelMaxWidth = 0f;
        config.progressShakeMaxAmplitude = -1f;
        config.progressShakeFrequency = -1f;

        bool changed = config.EnsureWorkflowDefaults();

        Assert.That(changed, Is.True);
        Assert.That(config.fontSize, Is.EqualTo(2.85f));
        Assert.That(config.outlineWidth, Is.EqualTo(0.28f));
        Assert.That(config.boldText, Is.True);
        Assert.That(config.sdfWeightNormal, Is.EqualTo(0.32f));
        Assert.That(config.sdfWeightBold, Is.EqualTo(0.82f));
        Assert.That(config.sdfGradientScale, Is.EqualTo(20f));
        Assert.That(config.glyphRadius, Is.EqualTo(0.23f));
        Assert.That(config.progressBarWidth, Is.EqualTo(1.55f));
        Assert.That(config.progressBarHeight, Is.EqualTo(0.12f));
        Assert.That(config.showSeconds, Is.EqualTo(0.16f));
        Assert.That(config.hideSeconds, Is.EqualTo(0.12f));
        Assert.That(config.baseWorldScale, Is.EqualTo(0.82f));
        Assert.That(config.targetScale, Is.EqualTo(1f));
        Assert.That(config.showEase.preset, Is.EqualTo(EasePreset.OutBack));
        Assert.That(config.hideEase.preset, Is.EqualTo(EasePreset.InBack));
        Assert.That(config.rowYOffset, Is.EqualTo(0f));
        Assert.That(config.glyphTextGap, Is.EqualTo(0.16f));
        Assert.That(config.labelMaxWidth, Is.EqualTo(5.25f));
        Assert.That(config.collectingLabelMaxWidth, Is.EqualTo(5.25f));
        Assert.That(config.progressShakeMaxAmplitude, Is.EqualTo(0.055f));
        Assert.That(config.progressShakeFrequency, Is.EqualTo(18f));

        UnityEngine.Object.DestroyImmediate(config);
    }

    [Test]
    public void ReadexProFontAssetsAreCleanedAndAssigned()
    {
        Font uiFont = AssetDatabase.LoadAssetAtPath<Font>(TheCircussyOneAssetPaths.ReadexProSourceFontPath);
        TMP_FontAsset tmpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TheCircussyOneAssetPaths.ReadexProTmpFontAssetPath);
        HudVisualConfig hudConfig = AssetDatabase.LoadAssetAtPath<HudVisualConfig>(TheCircussyOneAssetPaths.HudVisualConfigPath);
        DamageFeedbackVisualConfig damageConfig = AssetDatabase.LoadAssetAtPath<DamageFeedbackVisualConfig>(TheCircussyOneAssetPaths.DamageFeedbackVisualConfigPath);
        XpGainCounterVisualConfig xpGainConfig = AssetDatabase.LoadAssetAtPath<XpGainCounterVisualConfig>(TheCircussyOneAssetPaths.XpGainCounterVisualConfigPath);
        WorldInteractionPromptVisualConfig worldPromptConfig = AssetDatabase.LoadAssetAtPath<WorldInteractionPromptVisualConfig>(TheCircussyOneAssetPaths.WorldInteractionPromptVisualConfigPath);
        Material damageMaterial = AssetDatabase.LoadAssetAtPath<Material>(TheCircussyOneAssetPaths.DamageNumberMaterialPath);
        Material xpGainMaterial = AssetDatabase.LoadAssetAtPath<Material>(TheCircussyOneAssetPaths.XpGainCounterMaterialPath);
        Material worldPromptMaterial = AssetDatabase.LoadAssetAtPath<Material>(TheCircussyOneAssetPaths.WorldInteractionPromptMaterialPath);

        Assert.That(uiFont, Is.Not.Null);
        Assert.That(tmpFont, Is.Not.Null);
        Assert.That(tmpFont.atlasTexture, Is.Not.Null);
        Assert.That(tmpFont.atlasPopulationMode, Is.EqualTo(AtlasPopulationMode.Static));
        Assert.That(tmpFont.HasCharacters("Player123+50->LV2\u2026"), Is.True);
        Assert.That(tmpFont.HasCharacters("EX to Collect Tickets Collecting Tickets..."), Is.True);
        Assert.That(AssetDatabase.Contains(tmpFont.atlasTexture), Is.True);
        Assert.That(hudConfig.uiFont, Is.SameAs(uiFont));
        Assert.That(damageConfig.damageNumberFont, Is.SameAs(tmpFont));
        Assert.That(xpGainConfig.font, Is.SameAs(tmpFont));
        Assert.That(worldPromptConfig.font, Is.SameAs(tmpFont));
        Assert.That(TMP_Settings.defaultFontAsset, Is.SameAs(tmpFont));
        Texture damageAtlas = damageMaterial.GetTexture("_MainTex");
        Assert.That(damageAtlas, Is.Not.Null);
        Assert.That(damageAtlas.name, Is.EqualTo(tmpFont.atlasTexture.name));
        Assert.That(AssetDatabase.GetAssetPath(damageAtlas), Is.EqualTo(AssetDatabase.GetAssetPath(tmpFont.atlasTexture)));
        Assert.That(damageMaterial.GetFloat("_GradientScale"), Is.EqualTo(damageConfig.sdfGradientScale));
        Assert.That(damageMaterial.GetFloat("_Sharpness"), Is.EqualTo(damageConfig.sdfSharpness));
        Assert.That(damageMaterial.GetFloat("_TextureWidth"), Is.EqualTo(damageAtlas.width));
        Assert.That(damageMaterial.GetFloat("_TextureHeight"), Is.EqualTo(damageAtlas.height));
        Assert.That(xpGainMaterial.GetTexture("_MainTex"), Is.EqualTo(damageAtlas));
        Assert.That(xpGainMaterial.GetColor("_FaceColor"), Is.EqualTo(Color.white));
        Assert.That(xpGainMaterial.GetColor("_GlowColor"), Is.EqualTo(Color.clear));
        Assert.That(worldPromptMaterial.GetTexture("_MainTex"), Is.EqualTo(damageAtlas));
        Assert.That(worldPromptMaterial.GetFloat("_OutlineWidth"), Is.EqualTo(worldPromptConfig.outlineWidth));
        Assert.That(worldPromptMaterial.GetFloat("_WeightNormal"), Is.EqualTo(worldPromptConfig.sdfWeightNormal));
        Assert.That(worldPromptMaterial.GetFloat("_WeightBold"), Is.EqualTo(worldPromptConfig.sdfWeightBold));
        Assert.That(AssetDatabase.IsValidFolder(TheCircussyOneAssetPaths.ReadexProFolder + "/static"), Is.False);
        Assert.That(AssetDatabase.LoadAssetAtPath<TextAsset>(TheCircussyOneAssetPaths.ReadexProFolder + "/README.txt"), Is.Null);
    }

    [Test]
    public void SceneBuilderPathWrappersUseCanonicalAssetPaths()
    {
        Assert.That(TheCircussyOneSceneBuilder.GameConfigPath, Is.EqualTo(TheCircussyOneAssetPaths.GameConfigPath));
        Assert.That(TheCircussyOneSceneBuilder.CannonWeaponPath, Is.EqualTo(TheCircussyOneAssetPaths.CannonWeaponPath));
        Assert.That(TheCircussyOneSceneBuilder.KnifeFanWeaponPath, Is.EqualTo(TheCircussyOneAssetPaths.KnifeFanWeaponPath));
        Assert.That(TheCircussyOneSceneBuilder.SpotlightBoltWeaponPath, Is.EqualTo(TheCircussyOneAssetPaths.SpotlightBoltWeaponPath));
        Assert.That(TheCircussyOneSceneBuilder.FireHoopWeaponPath, Is.EqualTo(TheCircussyOneAssetPaths.FireHoopWeaponPath));
        Assert.That(TheCircussyOneAssetPaths.FireHoopAreaTelegraphPrefabPath, Is.EqualTo("Assets/Game/Prefabs/VFX/FireHoopAreaTelegraph.prefab"));
        Assert.That(TheCircussyOneAssetPaths.WorldAmbientDustPrefabPath, Is.EqualTo("Assets/Game/Prefabs/VFX/WorldAmbientDust.prefab"));
        Assert.That(TheCircussyOneSceneBuilder.JugglingBallWeaponPath, Is.EqualTo(TheCircussyOneAssetPaths.JugglingBallWeaponPath));
        Assert.That(TheCircussyOneSceneBuilder.WeaponCatalogPath, Is.EqualTo(TheCircussyOneAssetPaths.WeaponCatalogPath));
        Assert.That(TheCircussyOneSceneBuilder.RubberSolesItemPath, Is.EqualTo(TheCircussyOneAssetPaths.RubberSolesItemPath));
        Assert.That(TheCircussyOneSceneBuilder.SafetyPaddingItemPath, Is.EqualTo(TheCircussyOneAssetPaths.SafetyPaddingItemPath));
        Assert.That(TheCircussyOneSceneBuilder.MagnetCharmItemPath, Is.EqualTo(TheCircussyOneAssetPaths.MagnetCharmItemPath));
        Assert.That(TheCircussyOneSceneBuilder.LuckyCoinItemPath, Is.EqualTo(TheCircussyOneAssetPaths.LuckyCoinItemPath));
        Assert.That(TheCircussyOneSceneBuilder.StudyNotesItemPath, Is.EqualTo(TheCircussyOneAssetPaths.StudyNotesItemPath));
        Assert.That(TheCircussyOneSceneBuilder.TempoBraceletItemPath, Is.EqualTo(TheCircussyOneAssetPaths.TempoBraceletItemPath));
        Assert.That(TheCircussyOneSceneBuilder.PowderFlaskItemPath, Is.EqualTo(TheCircussyOneAssetPaths.PowderFlaskItemPath));
        Assert.That(TheCircussyOneSceneBuilder.SpringBootsItemPath, Is.EqualTo(TheCircussyOneAssetPaths.SpringBootsItemPath));
        Assert.That(TheCircussyOneSceneBuilder.WideLensItemPath, Is.EqualTo(TheCircussyOneAssetPaths.WideLensItemPath));
        Assert.That(TheCircussyOneSceneBuilder.LongFuseItemPath, Is.EqualTo(TheCircussyOneAssetPaths.LongFuseItemPath));
        Assert.That(TheCircussyOneSceneBuilder.ItemCatalogPath, Is.EqualTo(TheCircussyOneAssetPaths.ItemCatalogPath));
        Assert.That(TheCircussyOneSceneBuilder.OpenChestPath, Is.EqualTo(TheCircussyOneAssetPaths.OpenChestPath));
        Assert.That(TheCircussyOneSceneBuilder.LockedChestPath, Is.EqualTo(TheCircussyOneAssetPaths.LockedChestPath));
        Assert.That(TheCircussyOneSceneBuilder.PremiumChestPath, Is.EqualTo(TheCircussyOneAssetPaths.PremiumChestPath));
        Assert.That(TheCircussyOneSceneBuilder.SpecialEnemyChestPath, Is.EqualTo(TheCircussyOneAssetPaths.SpecialEnemyChestPath));
        Assert.That(TheCircussyOneSceneBuilder.ChestCatalogPath, Is.EqualTo(TheCircussyOneAssetPaths.ChestCatalogPath));
        Assert.That(TheCircussyOneSceneBuilder.SmallTicketStackPath, Is.EqualTo(TheCircussyOneAssetPaths.SmallTicketStackPath));
        Assert.That(TheCircussyOneSceneBuilder.TicketRollPath, Is.EqualTo(TheCircussyOneAssetPaths.TicketRollPath));
        Assert.That(TheCircussyOneSceneBuilder.JackpotCachePath, Is.EqualTo(TheCircussyOneAssetPaths.JackpotCachePath));
        Assert.That(TheCircussyOneSceneBuilder.TicketDepositCatalogPath, Is.EqualTo(TheCircussyOneAssetPaths.TicketDepositCatalogPath));
        Assert.That(TheCircussyOneSceneBuilder.WorldRewardPlacementCatalogPath, Is.EqualTo(TheCircussyOneAssetPaths.WorldRewardPlacementCatalogPath));
        Assert.That(TheCircussyOneSceneBuilder.RunWorldGenerationConfigPath, Is.EqualTo(TheCircussyOneAssetPaths.RunWorldGenerationConfigPath));
        Assert.That(TheCircussyOneSceneBuilder.BlueXpGemPath, Is.EqualTo(TheCircussyOneAssetPaths.BlueXpGemPath));
        Assert.That(TheCircussyOneSceneBuilder.GreenXpGemPath, Is.EqualTo(TheCircussyOneAssetPaths.GreenXpGemPath));
        Assert.That(TheCircussyOneSceneBuilder.RedXpGemPath, Is.EqualTo(TheCircussyOneAssetPaths.RedXpGemPath));
        Assert.That(TheCircussyOneSceneBuilder.XpGemCatalogPath, Is.EqualTo(TheCircussyOneAssetPaths.XpGemCatalogPath));
        Assert.That(TheCircussyOneSceneBuilder.CannonTuningUpgradePath, Is.EqualTo(TheCircussyOneAssetPaths.CannonTuningUpgradePath));
        Assert.That(TheCircussyOneSceneBuilder.CannonPayloadUpgradePath, Is.EqualTo(TheCircussyOneAssetPaths.CannonPayloadUpgradePath));
        Assert.That(TheCircussyOneSceneBuilder.CannonBiggerShotUpgradePath, Is.EqualTo(TheCircussyOneAssetPaths.CannonBiggerShotUpgradePath));
        Assert.That(TheCircussyOneSceneBuilder.CannonQuickFuseUpgradePath, Is.EqualTo(TheCircussyOneAssetPaths.CannonQuickFuseUpgradePath));
        Assert.That(TheCircussyOneSceneBuilder.CannonFastShotUpgradePath, Is.EqualTo(TheCircussyOneAssetPaths.CannonFastShotUpgradePath));
        Assert.That(TheCircussyOneSceneBuilder.KnifeFanTuningUpgradePath, Is.EqualTo(TheCircussyOneAssetPaths.KnifeFanTuningUpgradePath));
        Assert.That(TheCircussyOneSceneBuilder.KnifeFanFocusUpgradePath, Is.EqualTo(TheCircussyOneAssetPaths.KnifeFanFocusUpgradePath));
        Assert.That(TheCircussyOneSceneBuilder.KnifeFanHonedBladesUpgradePath, Is.EqualTo(TheCircussyOneAssetPaths.KnifeFanHonedBladesUpgradePath));
        Assert.That(TheCircussyOneSceneBuilder.KnifeFanQuickTossUpgradePath, Is.EqualTo(TheCircussyOneAssetPaths.KnifeFanQuickTossUpgradePath));
        Assert.That(TheCircussyOneSceneBuilder.KnifeFanFastBladesUpgradePath, Is.EqualTo(TheCircussyOneAssetPaths.KnifeFanFastBladesUpgradePath));
        Assert.That(TheCircussyOneSceneBuilder.KnifeFanLingeringCutUpgradePath, Is.EqualTo(TheCircussyOneAssetPaths.KnifeFanLingeringCutUpgradePath));
        Assert.That(TheCircussyOneSceneBuilder.SpotlightBoltTuningUpgradePath, Is.EqualTo(TheCircussyOneAssetPaths.SpotlightBoltTuningUpgradePath));
        Assert.That(TheCircussyOneSceneBuilder.SpotlightBoltBrightChargeUpgradePath, Is.EqualTo(TheCircussyOneAssetPaths.SpotlightBoltBrightChargeUpgradePath));
        Assert.That(TheCircussyOneSceneBuilder.SpotlightBoltQuickCueUpgradePath, Is.EqualTo(TheCircussyOneAssetPaths.SpotlightBoltQuickCueUpgradePath));
        Assert.That(TheCircussyOneSceneBuilder.SpotlightBoltFastBeamUpgradePath, Is.EqualTo(TheCircussyOneAssetPaths.SpotlightBoltFastBeamUpgradePath));
        Assert.That(TheCircussyOneSceneBuilder.SpotlightBoltWideBeamUpgradePath, Is.EqualTo(TheCircussyOneAssetPaths.SpotlightBoltWideBeamUpgradePath));
        Assert.That(TheCircussyOneSceneBuilder.SpotlightBoltLingeringGlowUpgradePath, Is.EqualTo(TheCircussyOneAssetPaths.SpotlightBoltLingeringGlowUpgradePath));
        Assert.That(TheCircussyOneSceneBuilder.FireHoopTuningUpgradePath, Is.EqualTo(TheCircussyOneAssetPaths.FireHoopTuningUpgradePath));
        Assert.That(TheCircussyOneSceneBuilder.FireHoopWiderUpgradePath, Is.EqualTo(TheCircussyOneAssetPaths.FireHoopWiderUpgradePath));
        Assert.That(TheCircussyOneSceneBuilder.FireHoopExtraUpgradePath, Is.EqualTo(TheCircussyOneAssetPaths.FireHoopExtraUpgradePath));
        Assert.That(TheCircussyOneSceneBuilder.FireHoopQuickSpinUpgradePath, Is.EqualTo(TheCircussyOneAssetPaths.FireHoopQuickSpinUpgradePath));
        Assert.That(TheCircussyOneSceneBuilder.FireHoopBiggerFlameUpgradePath, Is.EqualTo(TheCircussyOneAssetPaths.FireHoopBiggerFlameUpgradePath));
        Assert.That(TheCircussyOneSceneBuilder.JugglingBallTuningUpgradePath, Is.EqualTo(TheCircussyOneAssetPaths.JugglingBallTuningUpgradePath));
        Assert.That(TheCircussyOneSceneBuilder.JugglingBallTimingUpgradePath, Is.EqualTo(TheCircussyOneAssetPaths.JugglingBallTimingUpgradePath));
        Assert.That(TheCircussyOneSceneBuilder.JugglingBallWeightedBallUpgradePath, Is.EqualTo(TheCircussyOneAssetPaths.JugglingBallWeightedBallUpgradePath));
        Assert.That(TheCircussyOneSceneBuilder.JugglingBallQuickTossUpgradePath, Is.EqualTo(TheCircussyOneAssetPaths.JugglingBallQuickTossUpgradePath));
        Assert.That(TheCircussyOneSceneBuilder.JugglingBallFastRollUpgradePath, Is.EqualTo(TheCircussyOneAssetPaths.JugglingBallFastRollUpgradePath));
        Assert.That(TheCircussyOneSceneBuilder.JugglingBallBiggerBallUpgradePath, Is.EqualTo(TheCircussyOneAssetPaths.JugglingBallBiggerBallUpgradePath));
        Assert.That(TheCircussyOneSceneBuilder.JugglingBallLongJuggleUpgradePath, Is.EqualTo(TheCircussyOneAssetPaths.JugglingBallLongJuggleUpgradePath));
        Assert.That(TheCircussyOneSceneBuilder.UpgradeCatalogPath, Is.EqualTo(TheCircussyOneAssetPaths.UpgradeCatalogPath));
        Assert.That(TheCircussyOneSceneBuilder.FootworkTalentPath, Is.EqualTo(TheCircussyOneAssetPaths.FootworkTalentPath));
        Assert.That(TheCircussyOneSceneBuilder.StageStaminaTalentPath, Is.EqualTo(TheCircussyOneAssetPaths.StageStaminaTalentPath));
        Assert.That(TheCircussyOneSceneBuilder.ToughSkinTalentPath, Is.EqualTo(TheCircussyOneAssetPaths.ToughSkinTalentPath));
        Assert.That(TheCircussyOneSceneBuilder.CrowdFavoriteTalentPath, Is.EqualTo(TheCircussyOneAssetPaths.CrowdFavoriteTalentPath));
        Assert.That(TheCircussyOneSceneBuilder.QuickHandsTalentPath, Is.EqualTo(TheCircussyOneAssetPaths.QuickHandsTalentPath));
        Assert.That(TheCircussyOneSceneBuilder.SharpEyeTalentPath, Is.EqualTo(TheCircussyOneAssetPaths.SharpEyeTalentPath));
        Assert.That(TheCircussyOneSceneBuilder.LongReachTalentPath, Is.EqualTo(TheCircussyOneAssetPaths.LongReachTalentPath));
        Assert.That(TheCircussyOneSceneBuilder.LuckyBreakTalentPath, Is.EqualTo(TheCircussyOneAssetPaths.LuckyBreakTalentPath));
        Assert.That(TheCircussyOneSceneBuilder.BiggerPropsTalentPath, Is.EqualTo(TheCircussyOneAssetPaths.BiggerPropsTalentPath));
        Assert.That(TheCircussyOneSceneBuilder.SpringboardTalentPath, Is.EqualTo(TheCircussyOneAssetPaths.SpringboardTalentPath));
        Assert.That(TheCircussyOneSceneBuilder.JugglerTalentPath, Is.EqualTo(TheCircussyOneAssetPaths.JugglerTalentPath));
        Assert.That(TheCircussyOneSceneBuilder.CleanCompileTalentPath, Is.EqualTo(TheCircussyOneAssetPaths.CleanCompileTalentPath));
        Assert.That(TheCircussyOneSceneBuilder.CappaTalentPath, Is.EqualTo(TheCircussyOneAssetPaths.CappaTalentPath));
        Assert.That(TheCircussyOneSceneBuilder.StrongmanTalentPath, Is.EqualTo(TheCircussyOneAssetPaths.StrongmanTalentPath));
        Assert.That(TheCircussyOneSceneBuilder.AcrobatTalentPath, Is.EqualTo(TheCircussyOneAssetPaths.AcrobatTalentPath));
        Assert.That(TheCircussyOneSceneBuilder.RingmasterTalentPath, Is.EqualTo(TheCircussyOneAssetPaths.RingmasterTalentPath));
        Assert.That(TheCircussyOneSceneBuilder.HoopFlowTalentPath, Is.EqualTo(TheCircussyOneAssetPaths.HoopFlowTalentPath));
        Assert.That(TheCircussyOneSceneBuilder.EncoreBounceTalentPath, Is.EqualTo(TheCircussyOneAssetPaths.EncoreBounceTalentPath));
        Assert.That(TheCircussyOneSceneBuilder.TalentCatalogPath, Is.EqualTo(TheCircussyOneAssetPaths.TalentCatalogPath));
        Assert.That(TheCircussyOneSceneBuilder.JugglerPerformerPath, Is.EqualTo(TheCircussyOneAssetPaths.JugglerPerformerPath));
        Assert.That(TheCircussyOneSceneBuilder.CappaPerformerPath, Is.EqualTo(TheCircussyOneAssetPaths.CappaPerformerPath));
        Assert.That(TheCircussyOneSceneBuilder.StrongmanPerformerPath, Is.EqualTo(TheCircussyOneAssetPaths.StrongmanPerformerPath));
        Assert.That(TheCircussyOneSceneBuilder.AcrobatPerformerPath, Is.EqualTo(TheCircussyOneAssetPaths.AcrobatPerformerPath));
        Assert.That(TheCircussyOneSceneBuilder.RingmasterPerformerPath, Is.EqualTo(TheCircussyOneAssetPaths.RingmasterPerformerPath));
        Assert.That(TheCircussyOneSceneBuilder.SolaHoopDancerPerformerPath, Is.EqualTo(TheCircussyOneAssetPaths.SolaHoopDancerPerformerPath));
        Assert.That(TheCircussyOneSceneBuilder.BibiBallJugglerPerformerPath, Is.EqualTo(TheCircussyOneAssetPaths.BibiBallJugglerPerformerPath));
        Assert.That(TheCircussyOneSceneBuilder.PerformerCatalogPath, Is.EqualTo(TheCircussyOneAssetPaths.PerformerCatalogPath));
        Assert.That(TheCircussyOneSceneBuilder.CameraConfigPath, Is.EqualTo(TheCircussyOneAssetPaths.CameraConfigPath));
        Assert.That(TheCircussyOneSceneBuilder.GridVisualConfigPath, Is.EqualTo(TheCircussyOneAssetPaths.GridVisualConfigPath));
        Assert.That(TheCircussyOneSceneBuilder.LightingVisualConfigPath, Is.EqualTo(TheCircussyOneAssetPaths.LightingVisualConfigPath));
        Assert.That(TheCircussyOneSceneBuilder.DamageFeedbackVisualConfigPath, Is.EqualTo(TheCircussyOneAssetPaths.DamageFeedbackVisualConfigPath));
        Assert.That(TheCircussyOneSceneBuilder.ActorMotionVisualConfigPath, Is.EqualTo(TheCircussyOneAssetPaths.ActorMotionVisualConfigPath));
        Assert.That(TheCircussyOneSceneBuilder.EnemySpawnVisualConfigPath, Is.EqualTo(TheCircussyOneAssetPaths.EnemySpawnVisualConfigPath));
        Assert.That(TheCircussyOneSceneBuilder.VfxVisualConfigPath, Is.EqualTo(TheCircussyOneAssetPaths.VfxVisualConfigPath));
        Assert.That(TheCircussyOneSceneBuilder.HudVisualConfigPath, Is.EqualTo(TheCircussyOneAssetPaths.HudVisualConfigPath));
        Assert.That(TheCircussyOneSceneBuilder.XpGainCounterVisualConfigPath, Is.EqualTo(TheCircussyOneAssetPaths.XpGainCounterVisualConfigPath));
        Assert.That(TheCircussyOneSceneBuilder.WorldInteractionPromptVisualConfigPath, Is.EqualTo(TheCircussyOneAssetPaths.WorldInteractionPromptVisualConfigPath));
        Assert.That(TheCircussyOneSceneBuilder.HudPanelSettingsPath, Is.EqualTo(TheCircussyOneAssetPaths.HudPanelSettingsPath));
    }

    [Test]
    public void VisualConfigsUseOdinSerializedScriptableObjects()
    {
        Assert.That(typeof(SerializedScriptableObject).IsAssignableFrom(typeof(GridVisualConfig)), Is.True);
        Assert.That(typeof(SerializedScriptableObject).IsAssignableFrom(typeof(LightingVisualConfig)), Is.True);
        Assert.That(typeof(SerializedScriptableObject).IsAssignableFrom(typeof(ActorMotionVisualConfig)), Is.True);
        Assert.That(typeof(SerializedScriptableObject).IsAssignableFrom(typeof(EnemySpawnVisualConfig)), Is.True);
        Assert.That(typeof(SerializedScriptableObject).IsAssignableFrom(typeof(VfxVisualConfig)), Is.True);
        Assert.That(typeof(SerializedScriptableObject).IsAssignableFrom(typeof(HudVisualConfig)), Is.True);
        Assert.That(typeof(SerializedScriptableObject).IsAssignableFrom(typeof(XpGainCounterVisualConfig)), Is.True);
        Assert.That(typeof(SerializedScriptableObject).IsAssignableFrom(typeof(WorldInteractionPromptVisualConfig)), Is.True);
    }

    [Test]
    public void ConfigSlidersUseNumericValueFields()
    {
        Type[] inspectedTypes =
        {
            typeof(GameConfig),
            typeof(CameraConfig),
            typeof(DamageFeedbackVisualConfig),
            typeof(ActorMotionVisualConfig),
            typeof(ActorSquashStretchCycleSettings),
            typeof(ActorScalePulseSettings),
            typeof(EnemySpawnVisualConfig),
            typeof(GridVisualConfig),
            typeof(GridMaterialLightingSettings),
            typeof(HudVisualConfig),
            typeof(XpGainCounterVisualConfig),
            typeof(WorldInteractionPromptVisualConfig),
            typeof(LightingVisualConfig),
            typeof(EaseSettings)
        };

        foreach (Type type in inspectedTypes)
        {
            foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                object[] attributes = field.GetCustomAttributes(inherit: false);
                bool usesOldSlider = attributes.Any(attribute =>
                    attribute is UnityEngine.RangeAttribute ||
                    string.Equals(attribute.GetType().FullName, "Sirenix.OdinInspector.PropertyRangeAttribute", StringComparison.Ordinal));

                Assert.That(usesOldSlider, Is.False, $"{type.Name}.{field.Name} should use NumericSliderAttribute so sliders show editable numeric values.");
            }
        }

        AssertNumericSlider(typeof(HudVisualConfig), nameof(HudVisualConfig.globalScale), 0.25f, 3f);
        AssertNumericSlider(typeof(XpGainCounterVisualConfig), nameof(XpGainCounterVisualConfig.sdfGradientScale), 4f, 24f);
        AssertNumericSlider(typeof(WorldInteractionPromptVisualConfig), nameof(WorldInteractionPromptVisualConfig.sdfWeightNormal), 0f, 1f);
        AssertNumericSlider(typeof(WorldInteractionPromptVisualConfig), nameof(WorldInteractionPromptVisualConfig.sdfWeightBold), 0f, 1f);
        AssertNumericSlider(typeof(WorldInteractionPromptVisualConfig), nameof(WorldInteractionPromptVisualConfig.sdfGradientScale), 4f, 24f);
        AssertNumericSlider(typeof(EaseSettings), nameof(EaseSettings.shape), 0.25f, 3f);
        AssertNumericSlider(typeof(CameraConfig), nameof(CameraConfig.lookSensitivityMultiplier), 0f, 3f);
        AssertNumericSlider(typeof(GridVisualConfig), nameof(GridVisualConfig.floorGridStrength), 0f, 1f);
        AssertDropdownEnum(typeof(EaseSettings), nameof(EaseSettings.preset));
        Assert.That(typeof(NumericSliderAttributeFloatDrawer), Is.Not.Null);
        Assert.That(typeof(NumericSliderAttributeIntDrawer), Is.Not.Null);
    }

    [Test]
    public void ConfigValidationRulesCatchRelationshipErrors()
    {
        ConfigValidationResult validPitch = ConfigValidationRules.MinLessOrEqual(20f, 58f, "Min pitch", "Max pitch");
        ConfigValidationResult invalidPitch = ConfigValidationRules.MinLessOrEqual(58f, 20f, "Min pitch", "Max pitch");
        ConfigValidationResult validGrid = ConfigValidationRules.MajorSpacing(1f, 5f);
        ConfigValidationResult invalidGrid = ConfigValidationRules.MajorSpacing(5f, 1f);

        Assert.That(validPitch.IsValid, Is.True);
        Assert.That(invalidPitch.Level, Is.EqualTo(ConfigValidationLevel.Error));
        Assert.That(validGrid.IsValid, Is.True);
        Assert.That(invalidGrid.Level, Is.EqualTo(ConfigValidationLevel.Error));
    }

    [Test]
    public void ZeroAsDisableValuesRemainValid()
    {
        var damage = ScriptableObject.CreateInstance<DamageFeedbackVisualConfig>();
        damage.enemyFlashEmissionStrength = 0f;
        damage.projectileSpawnSeconds = 0f;
        damage.playerDamageCameraShakeSeconds = 0f;
        damage.playerDamageCameraShakePositionAmplitude = 0f;
        damage.playerDamageCameraShakeRotationDegrees = 0f;

        damage.EnsureReadableDefaults();

        Assert.That(damage.enemyFlashEmissionStrength, Is.EqualTo(0f));
        Assert.That(damage.playerDamageCameraShakeSeconds, Is.EqualTo(0f));
        Assert.That(damage.playerDamageCameraShakePositionAmplitude, Is.EqualTo(0f));
        Assert.That(damage.playerDamageCameraShakeRotationDegrees, Is.EqualTo(0f));
        Assert.That(ConfigValidationRules.ZeroOrPositive(damage.enemyFlashEmissionStrength, "Enemy flash emission").IsValid, Is.True);

        UnityEngine.Object.DestroyImmediate(damage);
    }

    [Test]
    public void OdinConfigHubEditorTypeCompiles()
    {
        Assert.That(typeof(TheCircussyOneConfigHub), Is.Not.Null);
    }

    private static void AssertHudSnapshot(
        HudPreviewSnapshot snapshot,
        int currentHealth,
        int maxHealth,
        int currentExperience,
        int targetExperience,
        int level,
        int kills,
        float runSeconds,
        bool gameOverVisible)
    {
        Assert.That(snapshot.CurrentHealth, Is.EqualTo(currentHealth));
        Assert.That(snapshot.MaxHealth, Is.EqualTo(maxHealth));
        Assert.That(snapshot.CurrentExperience, Is.EqualTo(currentExperience));
        Assert.That(snapshot.TargetExperience, Is.EqualTo(targetExperience));
        Assert.That(snapshot.Level, Is.EqualTo(level));
        Assert.That(snapshot.Kills, Is.EqualTo(kills));
        Assert.That(snapshot.RunSeconds, Is.EqualTo(runSeconds));
        Assert.That(snapshot.GameOverVisible, Is.EqualTo(gameOverVisible));
    }

    private static void AssertNumericSlider(Type type, string fieldName, float min, float max)
    {
        FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null, $"{type.Name}.{fieldName}");

        NumericSliderAttribute attribute = field.GetCustomAttribute<NumericSliderAttribute>();
        Assert.That(attribute, Is.Not.Null, $"{type.Name}.{fieldName}");
        Assert.That(attribute.Min, Is.EqualTo(min));
        Assert.That(attribute.Max, Is.EqualTo(max));
    }

    private static void AssertDropdownEnum(Type type, string fieldName)
    {
        FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null, $"{type.Name}.{fieldName}");
        Assert.That(field.GetCustomAttribute<EnumToggleButtonsAttribute>(), Is.Null, $"{type.Name}.{fieldName} should use Unity/Odin's compact enum dropdown.");
    }
}
