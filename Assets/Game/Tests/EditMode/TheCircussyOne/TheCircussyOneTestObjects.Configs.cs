using UnityEngine;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;
using TheCircussyOne.Runtime;

internal static partial class TheCircussyOneTestObjects
{
    public static GameConfig CreateConfig()
    {
        var config = ScriptableObject.CreateInstance<GameConfig>();
        config.playerMoveSpeed = 8f;
        config.playerMaxHealth = 100;
        config.contactDamageCooldown = 1f;
        config.playerInputDeadzone = 0.12f;
        config.playerWalkSpeed = 3.5f;
        config.playerRunSpeed = 11.5f;
        config.playerAcceleration = 38f;
        config.playerDeceleration = 30f;
        config.playerTurnAcceleration = 24f;
        config.playerReverseSkidAngle = 125f;
        config.playerSkidFriction = 42f;
        config.playerRotationSharpness = 16f;
        config.pickupMagnetRadius = 7.5f;
        config.pickupCollectRadius = 1.2f;
        config.spawnIntervalSeconds = 1.25f;
        config.minSpawnIntervalSeconds = 0.22f;
        config.spawnRampDurationSeconds = 300f;
        config.maxEnemies = 30;
        config.maxEnemiesAtPeak = 170;
        config.enemySpawnRadius = 42f;
        config.enemyHealth = 170;
        config.enemyHealthGrowthPerMinute = 20f;
        config.enemyMoveSpeed = 2f;
        config.enemyMoveSpeedGrowthPerMinute = 0.24f;
        config.enemyTurnDegreesPerSecond = 540f;
        config.enemySeparationRadius = 1.15f;
        config.enemyPlayerStopDistance = 1.25f;
        config.enemySeparationWeight = 1.65f;
        config.enemyHurtboxRadius = 0.65f;
        config.enemyHurtboxHeightOffset = 0.95f;
        config.enemyContactHitboxSize = new Vector3(0.75f, 0.9f, 0.65f);
        config.enemyContactHitboxOffset = new Vector3(0f, 0.55f, 0.35f);
        config.enemyMovementBodyRadius = 0.52f;
        config.enemyMovementBodyHeight = 1.45f;
        config.enemyMovementBodyOffset = new Vector3(0f, 0.75f, 0f);
        config.playerEnemyBlockMaxPushPerFrame = 0.75f;
        config.enemyClimbingEnabled = true;
        config.enemyUseMoveSpeedForClimb = true;
        config.enemyClimbSpeedMultiplier = 1f;
        config.enemyFallbackClimbSpeed = 4f;
        config.enemyClimbGravity = 35f;
        config.enemyClimbTerminalFallSpeed = 28f;
        config.enemyMaxEnvironmentClimbHeight = 8f;
        config.enemyEnvironmentProbeDistance = 1.1f;
        config.enemyEnvironmentProbeIntervalFrames = 4;
        config.enemyClimbEnvironmentMask = ~0;
        config.enemyPileClimbingEnabled = true;
        config.enemyPileRadius = 2.4f;
        config.enemyPileStartCount = 6;
        config.enemyPileEnemiesPerLayer = 6;
        config.enemyPileMaxLayers = 4;
        config.enemyPileLayerHeightMultiplier = 1f;
        config.enemySupportClimbSpeedMultiplier = 0.5f;
        config.enemySupportClimbSeparationMultiplier = 0.15f;
        config.enemyContactDamage = 2;
        config.experienceCurveEarlyBase = 21;
        config.experienceCurveEarlyIncrement = 14;
        config.experienceCurveEarlyEndLevel = 10;
        config.experienceCurveMidBase = 161;
        config.experienceCurveMidIncrement = 23;
        config.experienceCurveMidEndLevel = 25;
        config.experienceCurveLateBase = 506;
        config.experienceCurveLateIncrement = 37;
        config.experienceCurveRequirementMultiplier = 3f;
        config.difficultyRampEase = EaseSettings.OutCubic;
        config.playerFacingSmoothingEase = EaseSettings.Exponential;
        config.playerInteractRequiresLineOfSight = true;
        config.playerInteractLineOfSightStartHeight = 1.1f;
        config.playerInteractLineOfSightTargetHeight = 0.65f;
        config.playerInteractLineOfSightProbeRadius = 0.08f;
        config.playerInteractLineOfSightMask = ~0;
        return config;
    }

    public static CameraConfig CreateCameraConfig()
    {
        var config = ScriptableObject.CreateInstance<CameraConfig>();
        config.followSharpness = 9f;
        config.orbitDistance = 20f;
        config.orbitYaw = 0f;
        config.orbitPitch = 36f;
        config.minPitch = 20f;
        config.maxPitch = 58f;
        config.targetHeight = 1.35f;
        config.lookAheadDistance = 9f;
        config.topDownLookAheadDistance = 0f;
        config.lookAheadPitchBias = 1.15f;
        config.lookSensitivityMultiplier = 1f;
        config.mouseYawSensitivity = 0.08f;
        config.mousePitchSensitivity = 0.06f;
        config.gamepadYawSpeed = 130f;
        config.gamepadPitchSpeed = 90f;
        config.gamepadTriggerYawSpeed = 130f;
        config.gamepadTriggerDeadzone = 0.08f;
        config.followSmoothingEase = EaseSettings.Exponential;
        config.lookAheadPitchEase = EaseSettings.Linear;
        return config;
    }

    public static DamageFeedbackVisualConfig CreateDamageFeedbackConfig()
    {
        var config = ScriptableObject.CreateInstance<DamageFeedbackVisualConfig>();
        config.damageNumbersEnabled = true;
        config.lifetime = 0.75f;
        config.spawnHeightBias = 0.55f;
        config.spawnJitterRadius = 0.18f;
        config.floatDistance = 1.1f;
        config.baseWorldScale = 0.85f;
        config.startScale = 1f;
        config.popScale = 1.35f;
        config.endScale = 0.55f;
        config.popDurationNormalized = 0.18f;
        config.fadeStartNormalized = 0.34f;
        config.fontSize = 4.2f;
        config.numberColor = new Color(1f, 0.92f, 0.32f, 1f);
        config.outlineColor = new Color(0.07f, 0.025f, 0f, 0.94f);
        config.sortingOrder = 250;
        config.enemyFlashSeconds = 0.08f;
        config.enemyFlashColor = Color.white;
        config.damageNumberFloatEase = EaseSettings.OutCubic;
        config.damageNumberFadeEase = EaseSettings.InCubic;
        config.damageNumberShrinkEase = EaseSettings.InCubic;
        config.damageNumberPopEase = EaseSettings.OutBack;
        config.enemyFlashEase = EaseSettings.OutQuad;
        config.enemyHitPulseEase = EaseSettings.OutQuad;
        config.enemyHitPulseScale = 1.25f;
        config.enemyDeathEase = EaseSettings.InBack;
        config.enemyHealthBarDeathFadeSeconds = 0.07f;
        config.enemyHealthBarDeathFadeEase = EaseSettings.OutQuad;
        config.enemyHoverEase = EaseSettings.InOutSine;
        config.enemyHoverHeight = 0.15f;
        config.enemyHoverSeconds = 0.85f;
        config.playerDamagePulseEase = EaseSettings.OutQuad;
        config.playerDamagePulseScale = 1.18f;
        config.playerDamageFlashSeconds = 0.12f;
        config.playerDamageFlashColor = new Color(1f, 0.08f, 0.08f, 1f);
        config.playerDamageFlashEmissionStrength = 2f;
        config.playerDamageFlashEase = EaseSettings.OutQuad;
        config.playerDamageVignetteEnabled = true;
        config.playerDamageVignetteColor = new Color(0.95f, 0.02f, 0.03f, 1f);
        config.playerDamageVignetteDarkColor = Color.black;
        config.playerDamageVignetteFlashSeconds = 0.58f;
        config.playerDamageVignetteMaxOpacity = 0.32f;
        config.playerDamageVignetteDarkMaxOpacity = 0.42f;
        config.playerDamageVignetteDamageToIntensityScale = 10f;
        config.playerDamageVignetteAccumulationCap = 1.25f;
        config.playerDamageVignetteGrainStrength = 0.26f;
        config.playerDamageVignetteSplotchStrength = 0.32f;
        config.playerDamageVignetteRadius = 0.78f;
        config.playerDamageVignetteSoftness = 0.28f;
        config.playerDamageVignetteFadeEase = EaseSettings.OutCubic;
        config.playerHealingVignetteEnabled = true;
        config.playerHealingVignetteColor = new Color(0.12f, 1f, 0.34f, 1f);
        config.playerHealingVignetteDarkColor = Color.black;
        config.playerHealingVignetteFlashSeconds = 0.46f;
        config.playerHealingVignetteMaxOpacity = 0.22f;
        config.playerHealingVignetteDarkMaxOpacity = 0.10f;
        config.playerHealingVignetteHealToIntensityScale = 4f;
        config.playerHealingVignetteAccumulationCap = 0.90f;
        config.playerHealingVignetteGrainStrength = 0.12f;
        config.playerHealingVignetteSplotchStrength = 0.16f;
        config.playerHealingVignetteRadius = 0.80f;
        config.playerHealingVignetteSoftness = 0.30f;
        config.playerHealingVignetteFadeEase = EaseSettings.OutCubic;
        config.playerDamageCameraShakeEnabled = true;
        config.playerDamageCameraShakeSeconds = 0.16f;
        config.playerDamageCameraShakePositionAmplitude = 0.12f;
        config.playerDamageCameraShakeRotationDegrees = 0.45f;
        config.playerDamageCameraShakeFrequency = 18f;
        config.playerDamageCameraShakeFalloffEase = EaseSettings.OutQuad;
        config.playerWorldHealthBarEnabled = true;
        config.playerWorldHealthBarVisibleThroughPlayer = true;
        config.playerWorldHealthBarLocalOffset = new Vector3(0f, 0.22f, 0f);
        config.playerWorldHealthBarWidth = 1.45f;
        config.playerWorldHealthBarBackgroundThickness = 0.12f;
        config.playerWorldHealthBarFillThickness = 0.14f;
        config.playerWorldHealthBarBackgroundColor = new Color(0.08f, 0.05f, 0.07f, 0.85f);
        config.playerWorldHealthBarFillColor = new Color(1f, 0.12f, 0.28f, 1f);
        config.playerWorldHealthBarSortingOrderBase = 42;
        config.playerWorldHealthBarRenderQueueBase = 3020;
        config.projectileSpawnEase = EaseSettings.OutBack;
        config.projectileSpawnSeconds = 0.08f;
        config.pickupSpawnEase = EaseSettings.OutBack;
        config.pickupIdleEase = EaseSettings.InOutSine;
        config.pickupAttractEase = EaseSettings.InBack;
        config.pickupSpawnSeconds = 0.2f;
        config.pickupSpawnDelaySeconds = 0f;
        config.pickupDropBurstDistance = 0f;
        config.pickupDropBurstSeconds = 0f;
        config.pickupDropBurstEase = EaseSettings.OutCubic;
        config.pickupDropArcHeight = 0f;
        config.pickupXpDropArcHeightMultiplier = 1.65f;
        config.pickupDropBounceHeight = 0f;
        config.pickupDropBounceCount = 0;
        config.pickupDropSettleDelaySeconds = 0f;
        config.pickupGroundClearance = 0.14f;
        config.pickupGroundProbeHeight = 4f;
        config.pickupGroundProbeDepth = 10f;
        config.pickupCollisionRadius = 0.16f;
        config.pickupVisualGroundPadding = 0.04f;
        config.pickupEnvironmentMask = ~0;
        config.pickupIdleHeight = 0.35f;
        config.pickupIdleSeconds = 0.65f;
        config.pickupAttractSeconds = 0.34f;
        config.cameraSmoothingEase = EaseSettings.Exponential;
        config.playerFacingSmoothingEase = EaseSettings.Exponential;
        config.difficultyRampEase = EaseSettings.OutCubic;
        return config;
    }

    public static XpGainCounterVisualConfig CreateXpGainCounterConfig()
    {
        var config = ScriptableObject.CreateInstance<XpGainCounterVisualConfig>();
        config.enabled = true;
        config.holdSeconds = 1f;
        config.fadeSeconds = 0.5f;
        config.popReturnSeconds = 0.18f;
        config.baseWorldScale = 0.78f;
        config.normalScale = 1f;
        config.popScale = 1.35f;
        config.rightOffset = 1.15f;
        config.heightOffset = 1.55f;
        config.forwardOffset = 0f;
        config.ticketCounterEnabled = true;
        config.ticketRightOffset = -1.15f;
        config.ticketHeightOffset = 1.55f;
        config.ticketForwardOffset = 0f;
        config.fontSize = 3.6f;
        config.textColor = new Color(0.192f, 0.843f, 1f, 1f);
        config.outlineColor = new Color(0.015f, 0.04f, 0.06f, 0.92f);
        config.ticketTextColor = new Color(1f, 0.843f, 0.353f, 1f);
        config.popEase = EaseSettings.OutBack;
        config.fadeEase = EaseSettings.InCubic;
        return config;
    }

    public static WorldInteractionPromptVisualConfig CreateWorldInteractionPromptConfig()
    {
        var config = ScriptableObject.CreateInstance<WorldInteractionPromptVisualConfig>();
        config.enabled = true;
        config.fontSize = 2.85f;
        config.outlineWidth = 0.28f;
        config.boldText = true;
        config.sdfWeightNormal = 0.32f;
        config.sdfWeightBold = 0.82f;
        config.sdfGradientScale = 20f;
        config.sdfSharpness = 0.3f;
        config.sortingOrder = 260;
        config.worldOffset = new Vector3(0f, 0.72f, 0f);
        config.rowYOffset = 0f;
        config.glyphTextGap = 0.16f;
        config.labelMaxWidth = 5.25f;
        config.collectingLabelMaxWidth = 5.25f;
        config.glyphTextBoxSize = new Vector2(0.58f, 0.52f);
        config.labelTextBoxSize = new Vector2(3.4f, 0.72f);
        config.glyphRadius = 0.23f;
        config.progressBarWidth = 1.55f;
        config.progressBarHeight = 0.12f;
        config.progressBarYOffset = -0.36f;
        config.showSeconds = 0.16f;
        config.hideSeconds = 0.12f;
        config.baseWorldScale = 0.82f;
        config.targetScale = 1f;
        config.showEase = EaseSettings.OutBack;
        config.hideEase = EaseSettings.InBack;
        config.progressShakeMinAmplitude = 0.004f;
        config.progressShakeMaxAmplitude = 0.055f;
        config.progressShakeFrequency = 18f;
        config.progressShakeEase = EaseSettings.InQuad;
        return config;
    }

    public static ActorMotionVisualConfig CreateActorMotionVisualConfig()
    {
        var config = ScriptableObject.CreateInstance<ActorMotionVisualConfig>();
        config.enabled = true;
        config.playerIdle = new ActorSquashStretchCycleSettings(0.018f, 1.35f, 0.5f);
        config.playerMove = new ActorSquashStretchCycleSettings(0.05f, 0.42f, 0.55f);
        config.enemyIdle = new ActorSquashStretchCycleSettings(0f, 1.2f, 0.5f);
        config.enemyMove = new ActorSquashStretchCycleSettings(0.06f, 0.38f, 0.55f);
        config.playerDamagePulse = new ActorScalePulseSettings(true, 0.20f, 1.18f, EaseSettings.OutQuad);
        config.playerJumpTakeoffPulse = new ActorScalePulseSettings(true, 0.16f, 1f, EaseSettings.OutBack, 0.86f, 1.16f, 0.12f);
        config.jumpHoldStretchEnabled = true;
        config.jumpHoldMaxSeconds = 0.22f;
        config.jumpHoldXZScale = 0.92f;
        config.jumpHoldYScale = 1.10f;
        config.jumpHoldReleaseSeconds = 0.10f;
        config.jumpHoldEase = EaseSettings.OutCubic;
        config.playerJumpLandPulse = new ActorScalePulseSettings(true, 0.20f, 1f, EaseSettings.OutBack, 1.18f, 0.82f, 0.22f);
        config.enemyHitPulse = new ActorScalePulseSettings(true, 0.16f, 1.25f, EaseSettings.OutQuad);
        config.speedSmoothingSharpness = 16f;
        config.speedSmoothingEase = EaseSettings.Exponential;
        config.inclineTiltEnabled = true;
        config.playerInclineTiltMaxDegrees = 18f;
        config.enemyInclineTiltMaxDegrees = 14f;
        config.inclineTiltSmoothingSharpness = 18f;
        config.enemyInclineGroundProbeDistance = 0.8f;
        return config;
    }

    public static EnemySpawnVisualConfig CreateEnemySpawnVisualConfig()
    {
        var config = ScriptableObject.CreateInstance<EnemySpawnVisualConfig>();
        config.enabled = true;
        config.warningSeconds = 0.85f;
        config.emergeSeconds = 0.28f;
        config.radius = 1.35f;
        config.innerStartRadius = 0.08f;
        config.groundYOffset = 0.035f;
        config.surfaceProbeHeight = 12f;
        config.surfaceProbeDepth = 24f;
        config.surfaceMask = ~0;
        config.outerThickness = 0.09f;
        config.innerThickness = 0.06f;
        config.centerRadiusMultiplier = 0.72f;
        config.outerColor = new Color(1f, 0.28f, 0.08f, 0.82f);
        config.innerColor = new Color(1f, 0.78f, 0.24f, 0.72f);
        config.centerColor = new Color(1f, 0.18f, 0.08f, 0.12f);
        config.outerPulseStrength = 0.18f;
        config.outerPulseCycles = 2f;
        config.emergeStartBodyY = -0.65f;
        config.emergeEndBodyY = 1f;
        config.innerExpandEase = EaseSettings.OutCubic;
        config.outerPulseEase = EaseSettings.InOutSine;
        config.enemyEmergeEase = EaseSettings.OutBack;
        return config;
    }

    public static VfxVisualConfig CreateVfxVisualConfig()
    {
        var config = ScriptableObject.CreateInstance<VfxVisualConfig>();
        config.EnsureWorkflowDefaults();
        return config;
    }
}
