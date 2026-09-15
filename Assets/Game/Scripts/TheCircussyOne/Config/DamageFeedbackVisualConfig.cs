using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using TheCircussyOne.Rules;

namespace TheCircussyOne.Config
{
    [CreateAssetMenu(menuName = "The Circussy One/Damage Feedback Visual Config", fileName = "DamageFeedbackVisualConfig")]
    [InfoBox("PROJECTED VISUAL: feedback tuning is visual-only. Apply regenerates generated damage number assets; combat damage lives in GameConfig.")]
    public sealed class DamageFeedbackVisualConfig : SerializedScriptableObject
    {
        private const string Tabs = "Damage Feedback";
        private const float DefaultDamageVignetteRedMaxOpacity = 0.32f;
        private const float DefaultDamageVignetteDarkMaxOpacity = 0.42f;
        private const float DefaultDamageVignetteGrainStrength = 0.26f;
        private const float DefaultDamageVignetteSplotchStrength = 0.32f;
        private const float DefaultDamageVignetteRadius = 0.78f;
        private const float DefaultDamageVignetteSoftness = 0.28f;
        private const float DefaultHealingVignetteGreenMaxOpacity = 0.22f;
        private const float DefaultHealingVignetteDarkMaxOpacity = 0.10f;
        private const float DefaultHealingVignetteGrainStrength = 0.12f;
        private const float DefaultHealingVignetteSplotchStrength = 0.16f;
        private const float DefaultHealingVignetteRadius = 0.80f;
        private const float DefaultHealingVignetteSoftness = 0.30f;

        [TabGroup(Tabs, "Damage Numbers"), BoxGroup(Tabs + "/Damage Numbers/Workflow"), LabelWidth(190), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.VisualColor")]
        public bool autoApplyOnChange = true;

        [TabGroup(Tabs, "Damage Numbers"), BoxGroup(Tabs + "/Damage Numbers/Workflow"), LabelWidth(190)]
        public bool damageNumbersEnabled = true;

        [TabGroup(Tabs, "Damage Numbers"), BoxGroup(Tabs + "/Damage Numbers/Text"), LabelWidth(190), AssetSelector, PreviewField(60)]
        public TMP_FontAsset damageNumberFont;
        [TabGroup(Tabs, "Damage Numbers"), BoxGroup(Tabs + "/Damage Numbers/Text"), LabelWidth(190), SuffixLabel("TMP size")]
        [InfoBox("Very large world-space damage text can crowd health bars at the current camera distance.", InfoMessageType.Warning, nameof(HasLargeDamageFont))]
        [Min(1f)] public float fontSize = 4.2f;
        [TabGroup(Tabs, "Damage Numbers"), BoxGroup(Tabs + "/Damage Numbers/Text"), LabelWidth(190)]
        [Min(0f)] public float outlineWidth = 0.18f;
        [TabGroup(Tabs, "Damage Numbers"), BoxGroup(Tabs + "/Damage Numbers/Text"), LabelWidth(190), NumericSlider(4f, 24f)]
        [InfoBox("Higher values make the TMP distance-field edge tighter. If damage numbers look fuzzy, raise this before increasing font size.")]
        public float sdfGradientScale = 12f;
        [TabGroup(Tabs, "Damage Numbers"), BoxGroup(Tabs + "/Damage Numbers/Text"), LabelWidth(190), NumericSlider(-1f, 1f)]
        [InfoBox("Positive values sharpen the SDF edge. Too high can make curves brittle, so keep this subtle.")]
        public float sdfSharpness = 0.35f;
        [TabGroup(Tabs, "Damage Numbers"), BoxGroup(Tabs + "/Damage Numbers/Text"), LabelWidth(190)]
        [Min(0)] public int sortingOrder = 250;

        [TabGroup(Tabs, "Damage Numbers"), BoxGroup(Tabs + "/Damage Numbers/Timing"), LabelWidth(190), SuffixLabel("sec")]
        [Min(0.1f)] public float lifetime = 0.75f;
        [TabGroup(Tabs, "Damage Numbers"), BoxGroup(Tabs + "/Damage Numbers/Timing"), LabelWidth(190), NumericSlider(0.01f, 0.8f)]
        public float popDurationNormalized = 0.18f;
        [TabGroup(Tabs, "Damage Numbers"), BoxGroup(Tabs + "/Damage Numbers/Timing"), LabelWidth(190), NumericSlider(0f, 0.95f)]
        public float fadeStartNormalized = 0.34f;

        [TabGroup(Tabs, "Damage Numbers"), BoxGroup(Tabs + "/Damage Numbers/Spawn & Motion"), LabelWidth(190), SuffixLabel("u")]
        [Min(0f)] public float spawnHeightBias = 0.55f;
        [TabGroup(Tabs, "Damage Numbers"), BoxGroup(Tabs + "/Damage Numbers/Spawn & Motion"), LabelWidth(190), SuffixLabel("u")]
        [Min(0f)] public float spawnJitterRadius = 0.18f;
        [TabGroup(Tabs, "Damage Numbers"), BoxGroup(Tabs + "/Damage Numbers/Spawn & Motion"), LabelWidth(190)]
        [InfoBox("Scatter Per Hit is the default. Hybrid Merge is available for comparison and sums rapid same-enemy hits into one number.")]
        public DamageNumberReadabilityMode damageNumberReadabilityMode = DamageNumberReadabilityMode.ScatterPerHit;
        [TabGroup(Tabs, "Damage Numbers"), BoxGroup(Tabs + "/Damage Numbers/Spawn & Motion"), LabelWidth(190), SuffixLabel("u")]
        [Min(0f)] public float sameEnemyNumberSpreadRadius = 0.34f;
        [TabGroup(Tabs, "Damage Numbers"), BoxGroup(Tabs + "/Damage Numbers/Spawn & Motion"), LabelWidth(190), SuffixLabel("u")]
        [Min(0f)] public float damageNumberLateralDriftDistance = 0.20f;
        [TabGroup(Tabs, "Damage Numbers"), BoxGroup(Tabs + "/Damage Numbers/Spawn & Motion"), LabelWidth(190), SuffixLabel("sec")]
        [Min(0f)] public float damageNumberMergeWindowSeconds = 0.12f;
        [TabGroup(Tabs, "Damage Numbers"), BoxGroup(Tabs + "/Damage Numbers/Spawn & Motion"), LabelWidth(190), SuffixLabel("u")]
        [Min(0f)] public float floatDistance = 1.1f;

        [TabGroup(Tabs, "Damage Numbers"), BoxGroup(Tabs + "/Damage Numbers/Scale"), LabelWidth(190)]
        [Min(0.01f)] public float baseWorldScale = 0.85f;
        [TabGroup(Tabs, "Damage Numbers"), BoxGroup(Tabs + "/Damage Numbers/Scale"), LabelWidth(190)]
        [Min(0.01f)] public float startScale = 1f;
        [TabGroup(Tabs, "Damage Numbers"), BoxGroup(Tabs + "/Damage Numbers/Scale"), LabelWidth(190)]
        [Min(0.01f)] public float popScale = 1.35f;
        [TabGroup(Tabs, "Damage Numbers"), BoxGroup(Tabs + "/Damage Numbers/Scale"), LabelWidth(190)]
        [Min(0.01f)] public float endScale = 0.55f;

        [TabGroup(Tabs, "Damage Numbers"), BoxGroup(Tabs + "/Damage Numbers/Easing"), LabelWidth(190), InlineProperty] public EaseSettings damageNumberFloatEase = EaseSettings.OutCubic;
        [TabGroup(Tabs, "Damage Numbers"), BoxGroup(Tabs + "/Damage Numbers/Easing"), LabelWidth(190), InlineProperty] public EaseSettings damageNumberFadeEase = EaseSettings.InCubic;
        [TabGroup(Tabs, "Damage Numbers"), BoxGroup(Tabs + "/Damage Numbers/Easing"), LabelWidth(190), InlineProperty] public EaseSettings damageNumberShrinkEase = EaseSettings.InCubic;
        [TabGroup(Tabs, "Damage Numbers"), BoxGroup(Tabs + "/Damage Numbers/Easing"), LabelWidth(190), InlineProperty] public EaseSettings damageNumberPopEase = EaseSettings.OutBack;

        [TabGroup(Tabs, "Damage Numbers"), BoxGroup(Tabs + "/Damage Numbers/Colors"), LabelWidth(190), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.WarningColor")] public Color numberColor = new(1f, 0.92f, 0.32f, 1f);
        [TabGroup(Tabs, "Damage Numbers"), BoxGroup(Tabs + "/Damage Numbers/Colors"), LabelWidth(190)] public Color outlineColor = new(0.07f, 0.025f, 0f, 0.94f);

        [TabGroup(Tabs, "Enemy"), BoxGroup(Tabs + "/Enemy/Flash"), LabelWidth(190), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.CombatColor"), SuffixLabel("sec")]
        [Min(0f)] public float enemyFlashSeconds = 0.08f;
        [TabGroup(Tabs, "Enemy"), BoxGroup(Tabs + "/Enemy/Flash"), LabelWidth(190)] public Color enemyFlashColor = Color.white;
        [TabGroup(Tabs, "Enemy"), BoxGroup(Tabs + "/Enemy/Flash"), LabelWidth(190)]
        [ValidateInput(nameof(IsEnemyFlashEmissionValid), "Flash emission can be zero to disable, but cannot be negative.")]
        [Min(0f)] public float enemyFlashEmissionStrength = 4f;
        [TabGroup(Tabs, "Enemy"), BoxGroup(Tabs + "/Enemy/Flash"), LabelWidth(190), InlineProperty] public EaseSettings enemyFlashEase = EaseSettings.OutQuad;

        [HideInInspector] public EaseSettings enemyHitPulseEase = EaseSettings.OutQuad;
        [HideInInspector] [Min(1f)] public float enemyHitPulseScale = 1.25f;
        [TabGroup(Tabs, "Enemy"), BoxGroup(Tabs + "/Enemy/Death"), LabelWidth(190), InlineProperty] public EaseSettings enemyDeathEase = EaseSettings.InBack;
        [TabGroup(Tabs, "Enemy"), BoxGroup(Tabs + "/Enemy/Death"), LabelWidth(190), SuffixLabel("sec")]
        [InfoBox("Fade the enemy world health bar immediately on lethal hit so the empty background does not linger during death shrink.")]
        [Min(0f)] public float enemyHealthBarDeathFadeSeconds = 0.07f;
        [TabGroup(Tabs, "Enemy"), BoxGroup(Tabs + "/Enemy/Death"), LabelWidth(190), InlineProperty] public EaseSettings enemyHealthBarDeathFadeEase = EaseSettings.OutQuad;
        [TabGroup(Tabs, "Enemy"), BoxGroup(Tabs + "/Enemy/Hover"), LabelWidth(190), InlineProperty] public EaseSettings enemyHoverEase = EaseSettings.InOutSine;
        [TabGroup(Tabs, "Enemy"), BoxGroup(Tabs + "/Enemy/Hover"), LabelWidth(190), SuffixLabel("u")] [Min(0f)] public float enemyHoverHeight = 0.15f;
        [TabGroup(Tabs, "Enemy"), BoxGroup(Tabs + "/Enemy/Hover"), LabelWidth(190), SuffixLabel("sec")] [Min(0.01f)] public float enemyHoverSeconds = 0.85f;

        [HideInInspector] public EaseSettings playerDamagePulseEase = EaseSettings.OutQuad;
        [HideInInspector] [Min(1f)] public float playerDamagePulseScale = 1.18f;

        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Damage Flash"), LabelWidth(190), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.CombatColor"), SuffixLabel("sec")]
        [Min(0f)] public float playerDamageFlashSeconds = 0.12f;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Damage Flash"), LabelWidth(190)] public Color playerDamageFlashColor = new(1f, 0.08f, 0.08f, 1f);
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Damage Flash"), LabelWidth(190)]
        [ValidateInput(nameof(IsPlayerDamageFlashEmissionValid), "Flash emission can be zero to disable, but cannot be negative.")]
        [Min(0f)] public float playerDamageFlashEmissionStrength = 2f;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Damage Flash"), LabelWidth(190), InlineProperty] public EaseSettings playerDamageFlashEase = EaseSettings.OutQuad;

        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Damage Vignette"), LabelWidth(190), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.CombatColor")]
        [InfoBox("Visual-only full-screen shader flash. Intensity is based on final post-armor damage as a percentage of max health.")]
        public bool playerDamageVignetteEnabled = true;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Damage Vignette"), LabelWidth(190)] public Color playerDamageVignetteColor = new(0.95f, 0.02f, 0.03f, 1f);
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Damage Vignette"), LabelWidth(190)] public Color playerDamageVignetteDarkColor = Color.black;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Damage Vignette"), LabelWidth(190), SuffixLabel("sec")]
        [Min(0f)] public float playerDamageVignetteFlashSeconds = 0.58f;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Damage Vignette"), LabelWidth(190), NumericSlider(0f, 1f)]
        [LabelText("Red Max Opacity")]
        public float playerDamageVignetteMaxOpacity = DefaultDamageVignetteRedMaxOpacity;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Damage Vignette"), LabelWidth(190), NumericSlider(0f, 1f)]
        public float playerDamageVignetteDarkMaxOpacity = DefaultDamageVignetteDarkMaxOpacity;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Damage Vignette"), LabelWidth(190), SuffixLabel("x")]
        [InfoBox("Damage percent to intensity multiplier. Example: 2% damage at 10x starts near 0.20 intensity.")]
        [Min(0f)] public float playerDamageVignetteDamageToIntensityScale = 10f;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Damage Vignette"), LabelWidth(190)]
        [Min(0f)] public float playerDamageVignetteAccumulationCap = 1.25f;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Damage Vignette"), LabelWidth(190), NumericSlider(0f, 1f)]
        public float playerDamageVignetteGrainStrength = DefaultDamageVignetteGrainStrength;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Damage Vignette"), LabelWidth(190), NumericSlider(0f, 1f)]
        public float playerDamageVignetteSplotchStrength = DefaultDamageVignetteSplotchStrength;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Damage Vignette"), LabelWidth(190), NumericSlider(0f, 1f)]
        [InfoBox("Higher values keep the effect near the screen edge. Lower values make the overlay cover more of the readable center.")]
        public float playerDamageVignetteRadius = DefaultDamageVignetteRadius;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Damage Vignette"), LabelWidth(190), NumericSlider(0f, 1f)]
        public float playerDamageVignetteSoftness = DefaultDamageVignetteSoftness;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Damage Vignette"), LabelWidth(190), InlineProperty]
        public EaseSettings playerDamageVignetteFadeEase = EaseSettings.OutCubic;

        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Healing Vignette"), LabelWidth(190), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.VisualColor")]
        [InfoBox("Visual-only green edge flash for actual player healing. Full-health Treats do not trigger this.")]
        public bool playerHealingVignetteEnabled = true;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Healing Vignette"), LabelWidth(190)] public Color playerHealingVignetteColor = new(0.12f, 1f, 0.34f, 1f);
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Healing Vignette"), LabelWidth(190)] public Color playerHealingVignetteDarkColor = Color.black;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Healing Vignette"), LabelWidth(190), SuffixLabel("sec")]
        [Min(0f)] public float playerHealingVignetteFlashSeconds = 0.46f;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Healing Vignette"), LabelWidth(190), NumericSlider(0f, 1f)]
        [LabelText("Green Max Opacity")]
        public float playerHealingVignetteMaxOpacity = DefaultHealingVignetteGreenMaxOpacity;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Healing Vignette"), LabelWidth(190), NumericSlider(0f, 1f)]
        public float playerHealingVignetteDarkMaxOpacity = DefaultHealingVignetteDarkMaxOpacity;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Healing Vignette"), LabelWidth(190), SuffixLabel("x")]
        [InfoBox("Healing percent to intensity multiplier. Example: 20 HP healed out of 100 HP at 4x starts near 0.80 intensity.")]
        [Min(0f)] public float playerHealingVignetteHealToIntensityScale = 4f;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Healing Vignette"), LabelWidth(190)]
        [Min(0f)] public float playerHealingVignetteAccumulationCap = 0.90f;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Healing Vignette"), LabelWidth(190), NumericSlider(0f, 1f)]
        public float playerHealingVignetteGrainStrength = DefaultHealingVignetteGrainStrength;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Healing Vignette"), LabelWidth(190), NumericSlider(0f, 1f)]
        public float playerHealingVignetteSplotchStrength = DefaultHealingVignetteSplotchStrength;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Healing Vignette"), LabelWidth(190), NumericSlider(0f, 1f)]
        [InfoBox("Higher values keep the healing flash near the screen edge.")]
        public float playerHealingVignetteRadius = DefaultHealingVignetteRadius;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Healing Vignette"), LabelWidth(190), NumericSlider(0f, 1f)]
        public float playerHealingVignetteSoftness = DefaultHealingVignetteSoftness;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Healing Vignette"), LabelWidth(190), InlineProperty]
        public EaseSettings playerHealingVignetteFadeEase = EaseSettings.OutCubic;

        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Camera Shake"), LabelWidth(190), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.CameraColor")]
        public bool playerDamageCameraShakeEnabled = true;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Camera Shake"), LabelWidth(190), SuffixLabel("sec")]
        [Min(0f)] public float playerDamageCameraShakeSeconds = 0.16f;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Camera Shake"), LabelWidth(190), SuffixLabel("local u")]
        [Min(0f)] public float playerDamageCameraShakePositionAmplitude = 0.12f;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Camera Shake"), LabelWidth(190), SuffixLabel("deg")]
        [Min(0f)] public float playerDamageCameraShakeRotationDegrees = 0.45f;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Camera Shake"), LabelWidth(190), SuffixLabel("hz")]
        [Min(0.01f)] public float playerDamageCameraShakeFrequency = 18f;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Camera Shake"), LabelWidth(190), InlineProperty]
        public EaseSettings playerDamageCameraShakeFalloffEase = EaseSettings.OutQuad;

        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/World Health Bar"), LabelWidth(190), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.VisualColor")]
        public bool playerWorldHealthBarEnabled = true;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/World Health Bar"), LabelWidth(190)]
        [InfoBox("When enabled, the bar uses ZTest Always so it remains readable even when the player body passes in front of it.")]
        public bool playerWorldHealthBarVisibleThroughPlayer = true;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/World Health Bar"), LabelWidth(190), SuffixLabel("local u")]
        public Vector3 playerWorldHealthBarLocalOffset = new(0f, 0.22f, 0f);
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/World Health Bar"), LabelWidth(190), SuffixLabel("u")]
        [Min(0.05f)] public float playerWorldHealthBarWidth = 1.45f;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/World Health Bar"), LabelWidth(190), SuffixLabel("u")]
        [Min(0.001f)] public float playerWorldHealthBarBackgroundThickness = 0.12f;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/World Health Bar"), LabelWidth(190), SuffixLabel("u")]
        [Min(0.001f)] public float playerWorldHealthBarFillThickness = 0.14f;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/World Health Bar"), LabelWidth(190)] public Color playerWorldHealthBarBackgroundColor = new(0.08f, 0.05f, 0.07f, 0.85f);
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/World Health Bar"), LabelWidth(190)] public Color playerWorldHealthBarFillColor = new(1f, 0.12f, 0.28f, 1f);
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/World Health Bar/Advanced"), LabelWidth(190)]
        [Min(0)] public int playerWorldHealthBarSortingOrderBase = 42;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/World Health Bar/Advanced"), LabelWidth(190)]
        [Min(0)] public int playerWorldHealthBarRenderQueueBase = 3020;

        [TabGroup(Tabs, "Projectile"), BoxGroup(Tabs + "/Projectile/Spawn Pop"), LabelWidth(190), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.WarningColor"), InlineProperty] public EaseSettings projectileSpawnEase = EaseSettings.OutBack;
        [TabGroup(Tabs, "Projectile"), BoxGroup(Tabs + "/Projectile/Spawn Pop"), LabelWidth(190), SuffixLabel("sec")] [Min(0f)] public float projectileSpawnSeconds = 0.08f;

        [TabGroup(Tabs, "Pickup"), BoxGroup(Tabs + "/Pickup/Spawn"), LabelWidth(190), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.VisualColor"), InlineProperty] public EaseSettings pickupSpawnEase = EaseSettings.OutBack;
        [TabGroup(Tabs, "Pickup"), BoxGroup(Tabs + "/Pickup/Idle"), LabelWidth(190), InlineProperty] public EaseSettings pickupIdleEase = EaseSettings.InOutSine;
        [TabGroup(Tabs, "Pickup"), BoxGroup(Tabs + "/Pickup/Attract"), LabelWidth(190), InlineProperty] public EaseSettings pickupAttractEase = EaseSettings.InBack;
        [TabGroup(Tabs, "Pickup"), BoxGroup(Tabs + "/Pickup/Spawn"), LabelWidth(190), SuffixLabel("sec")] [Min(0f)] public float pickupSpawnSeconds = 0.20f;
        [TabGroup(Tabs, "Pickup"), BoxGroup(Tabs + "/Pickup/Drop Presentation"), LabelWidth(190), SuffixLabel("sec")]
        [InfoBox("Delay before fresh drops can magnet or collect, so burst rewards remain readable before attraction starts.")]
        [Min(0f)] public float pickupSpawnDelaySeconds = 0.15f;
        [TabGroup(Tabs, "Pickup"), BoxGroup(Tabs + "/Pickup/Drop Presentation"), LabelWidth(190), SuffixLabel("u")]
        [InfoBox("Outward scatter radius used by deterministic kinematic pickup tosses.")]
        [Min(0f)] public float pickupDropBurstDistance = 1.15f;
        [TabGroup(Tabs, "Pickup"), BoxGroup(Tabs + "/Pickup/Drop Presentation"), LabelWidth(190), SuffixLabel("sec")]
        [Min(0f)] public float pickupDropBurstSeconds = 0.38f;
        [TabGroup(Tabs, "Pickup"), BoxGroup(Tabs + "/Pickup/Drop Presentation"), LabelWidth(190), InlineProperty] public EaseSettings pickupDropBurstEase = EaseSettings.OutCubic;
        [TabGroup(Tabs, "Pickup"), BoxGroup(Tabs + "/Pickup/Drop Presentation"), LabelWidth(190), SuffixLabel("u")] [Min(0f)] public float pickupDropArcHeight = 1.15f;
        [TabGroup(Tabs, "Pickup"), BoxGroup(Tabs + "/Pickup/Drop Presentation"), LabelWidth(190)]
        [InfoBox("Multiplier applied only to enemy XP gem drops. Treats keep the base pickup arc so snack carts can stay readable while XP bursts pop higher.")]
        [Min(0f)] public float pickupXpDropArcHeightMultiplier = 1.65f;
        [TabGroup(Tabs, "Pickup"), BoxGroup(Tabs + "/Pickup/Drop Presentation"), LabelWidth(190), SuffixLabel("u")] [Min(0f)] public float pickupDropBounceHeight = 0.18f;
        [TabGroup(Tabs, "Pickup"), BoxGroup(Tabs + "/Pickup/Drop Presentation"), LabelWidth(190)] [Min(0)] public int pickupDropBounceCount = 1;
        [TabGroup(Tabs, "Pickup"), BoxGroup(Tabs + "/Pickup/Drop Presentation"), LabelWidth(190), SuffixLabel("sec")] [Min(0f)] public float pickupDropSettleDelaySeconds = 0.04f;
        [TabGroup(Tabs, "Pickup"), BoxGroup(Tabs + "/Pickup/Grounding"), LabelWidth(190), SuffixLabel("u")] [Min(0f)] public float pickupGroundClearance = 0.14f;
        [TabGroup(Tabs, "Pickup"), BoxGroup(Tabs + "/Pickup/Grounding"), LabelWidth(190), SuffixLabel("u")] [Min(0.01f)] public float pickupGroundProbeHeight = 4f;
        [TabGroup(Tabs, "Pickup"), BoxGroup(Tabs + "/Pickup/Grounding"), LabelWidth(190), SuffixLabel("u")] [Min(0.01f)] public float pickupGroundProbeDepth = 10f;
        [TabGroup(Tabs, "Pickup"), BoxGroup(Tabs + "/Pickup/Grounding"), LabelWidth(190), SuffixLabel("u")] [Min(0f)] public float pickupCollisionRadius = 0.16f;
        [TabGroup(Tabs, "Pickup"), BoxGroup(Tabs + "/Pickup/Grounding"), LabelWidth(190), SuffixLabel("u")]
        [InfoBox("Visual-only lift added after accounting for the gem mesh bottom. Increase this if toss/spawn tweens still look too close to the floor.")]
        [Min(0f)] public float pickupVisualGroundPadding = 0.04f;
        [TabGroup(Tabs, "Pickup"), BoxGroup(Tabs + "/Pickup/Grounding"), LabelWidth(190)]
        [InfoBox("Defaults to non-gameplay environment layers at runtime; Player, Enemy, Projectile, and Pickup are always excluded.")]
        public LayerMask pickupEnvironmentMask = ~0;
        [TabGroup(Tabs, "Pickup"), BoxGroup(Tabs + "/Pickup/Idle"), LabelWidth(190), SuffixLabel("u")] [Min(0f)] public float pickupIdleHeight = 0.35f;
        [TabGroup(Tabs, "Pickup"), BoxGroup(Tabs + "/Pickup/Idle"), LabelWidth(190), SuffixLabel("sec")] [Min(0.01f)] public float pickupIdleSeconds = 0.65f;
        [TabGroup(Tabs, "Pickup"), BoxGroup(Tabs + "/Pickup/Attract"), LabelWidth(190), SuffixLabel("sec")] [Min(0.01f)] public float pickupAttractSeconds = 0.34f;

        [TabGroup(Tabs, "Smoothing"), BoxGroup(Tabs + "/Smoothing/Linked Easing"), LabelWidth(190), InlineProperty] public EaseSettings cameraSmoothingEase = EaseSettings.Exponential;
        [TabGroup(Tabs, "Smoothing"), BoxGroup(Tabs + "/Smoothing/Linked Easing"), LabelWidth(190), InlineProperty] public EaseSettings playerFacingSmoothingEase = EaseSettings.Exponential;
        [TabGroup(Tabs, "Smoothing"), BoxGroup(Tabs + "/Smoothing/Linked Easing"), LabelWidth(190), InlineProperty] public EaseSettings difficultyRampEase = EaseSettings.OutCubic;

        [TabGroup(Tabs, "Preview"), ShowInInspector, ReadOnly, LabelText("Damage Number Summary"), PropertyOrder(100)]
        private string DamageNumberSummary => damageNumbersEnabled
            ? $"{lifetime:0.00}s, {fontSize:0.0} font, {baseWorldScale:0.00}x world scale"
            : "Disabled";

        [TabGroup(Tabs, "Preview"), ShowInInspector, ReadOnly, LabelText("Enemy Flash Summary"), PropertyOrder(101)]
        private string EnemyFlashSummary => $"{enemyFlashSeconds:0.00}s, emission {enemyFlashEmissionStrength:0.##}";

        private bool HasLargeDamageFont => ConfigValidationRules.WarningIfGreater(fontSize, 7f, "Damage number font size").Level == ConfigValidationLevel.Warning;

        public bool EnsureReadableDefaults()
        {
            bool changed = false;

            if (baseWorldScale < 0.2f)
            {
                baseWorldScale = 0.85f;
                changed = true;
            }

            if (startScale > 3f || startScale <= 0f)
            {
                startScale = 1f;
                changed = true;
            }

            if (popScale > 3f || popScale <= 0f)
            {
                popScale = 1.35f;
                changed = true;
            }

            if (endScale > 10f || endScale <= 0f)
            {
                endScale = 0.55f;
                changed = true;
            }

            changed |= EnsureEaseDefault(ref damageNumberFloatEase, EaseSettings.OutCubic);
            changed |= EnsureEaseDefault(ref damageNumberFadeEase, EaseSettings.InCubic);
            changed |= EnsureEaseDefault(ref damageNumberShrinkEase, EaseSettings.InCubic);
            changed |= EnsureEaseDefault(ref damageNumberPopEase, EaseSettings.OutBack);
            changed |= EnsureMinimum(ref sameEnemyNumberSpreadRadius, 0.34f, 0f);
            changed |= EnsureMinimum(ref damageNumberLateralDriftDistance, 0.20f, 0f);
            changed |= EnsureMinimum(ref damageNumberMergeWindowSeconds, 0.12f, 0f);
            changed |= EnsureMinimum(ref sdfGradientScale, 12f, 4f);
            changed |= EnsureRange(ref sdfSharpness, 0.35f, -1f, 1f);
            changed |= EnsureEaseDefault(ref enemyFlashEase, EaseSettings.OutQuad);
            changed |= EnsureEaseDefault(ref enemyHitPulseEase, EaseSettings.OutQuad);
            changed |= EnsureEaseDefault(ref enemyDeathEase, EaseSettings.InBack);
            changed |= EnsureEaseDefault(ref enemyHealthBarDeathFadeEase, EaseSettings.OutQuad);
            changed |= EnsureEaseDefault(ref enemyHoverEase, EaseSettings.InOutSine);
            if (enemyFlashEmissionStrength < 0f)
            {
                enemyFlashEmissionStrength = 0f;
                changed = true;
            }
            changed |= EnsureEaseDefault(ref playerDamagePulseEase, EaseSettings.OutQuad);
            changed |= EnsureEaseDefault(ref playerDamageFlashEase, EaseSettings.OutQuad);
            changed |= EnsureMinimum(ref playerDamageFlashSeconds, 0.12f, 0f);
            changed |= EnsureDamageVignetteDefaults();
            changed |= EnsureHealingVignetteDefaults();
            changed |= EnsureZeroOrPositive(ref playerDamageCameraShakeSeconds, 0.16f);
            changed |= EnsureZeroOrPositive(ref playerDamageCameraShakePositionAmplitude, 0.12f);
            changed |= EnsureZeroOrPositive(ref playerDamageCameraShakeRotationDegrees, 0.45f);
            changed |= EnsureMinimum(ref playerDamageCameraShakeFrequency, 18f, 0.01f);
            changed |= EnsureEaseDefault(ref playerDamageCameraShakeFalloffEase, EaseSettings.OutQuad);
            if (playerDamageFlashEmissionStrength < 0f)
            {
                playerDamageFlashEmissionStrength = 0f;
                changed = true;
            }
            changed |= EnsureMinimum(ref playerWorldHealthBarWidth, 1.45f, 0.05f);
            changed |= EnsureMinimum(ref playerWorldHealthBarBackgroundThickness, 0.12f, 0.001f);
            changed |= EnsureMinimum(ref playerWorldHealthBarFillThickness, 0.14f, 0.001f);
            changed |= EnsureEaseDefault(ref projectileSpawnEase, EaseSettings.OutBack);
            changed |= EnsureEaseDefault(ref pickupSpawnEase, EaseSettings.OutBack);
            changed |= EnsureEaseDefault(ref pickupIdleEase, EaseSettings.InOutSine);
            changed |= EnsureEaseDefault(ref pickupAttractEase, EaseSettings.InBack);
            changed |= EnsureEaseDefault(ref pickupDropBurstEase, EaseSettings.OutCubic);
            changed |= EnsureEaseDefault(ref cameraSmoothingEase, EaseSettings.Exponential);
            changed |= EnsureEaseDefault(ref playerFacingSmoothingEase, EaseSettings.Exponential);
            changed |= EnsureEaseDefault(ref difficultyRampEase, EaseSettings.OutCubic);

            changed |= EnsureMinimum(ref enemyHitPulseScale, 1.25f, 1f);
            changed |= EnsureMinimum(ref playerDamagePulseScale, 1.18f, 1f);
            changed |= EnsureMinimum(ref projectileSpawnSeconds, 0.08f, 0f);
            changed |= EnsureMinimum(ref pickupSpawnSeconds, 0.20f, 0f);
            changed |= EnsureMinimumAllowZero(ref pickupSpawnDelaySeconds, 0.15f);
            changed |= EnsureMinimum(ref pickupDropBurstDistance, 1.15f, 0f);
            changed |= EnsureMinimum(ref pickupDropBurstSeconds, 0.38f, 0f);
            changed |= EnsureMinimum(ref pickupDropArcHeight, 1.15f, 0f);
            changed |= EnsureMinimum(ref pickupXpDropArcHeightMultiplier, 1.65f, 0f);
            changed |= EnsureMinimum(ref pickupDropBounceHeight, 0.18f, 0f);
            changed |= EnsureMinimumInt(ref pickupDropBounceCount, 1, 0);
            changed |= EnsureMinimumAllowZero(ref pickupDropSettleDelaySeconds, 0.04f);
            changed |= EnsureMinimum(ref pickupGroundClearance, 0.14f, 0f);
            changed |= EnsureMinimum(ref pickupGroundProbeHeight, 4f, 0.01f);
            changed |= EnsureMinimum(ref pickupGroundProbeDepth, 10f, 0.01f);
            changed |= EnsureMinimum(ref pickupCollisionRadius, 0.16f, 0f);
            changed |= EnsureMinimumAllowZero(ref pickupVisualGroundPadding, 0.04f);
            changed |= EnsureLayerMask(ref pickupEnvironmentMask, ~0);
            changed |= EnsureMinimum(ref pickupIdleHeight, 0.35f, 0f);
            changed |= EnsureMinimum(ref pickupIdleSeconds, 0.65f, 0.01f);
            changed |= EnsureMinimum(ref pickupAttractSeconds, 0.34f, 0.01f);
            changed |= EnsureMinimum(ref enemyHoverHeight, 0.15f, 0f);
            changed |= EnsureMinimum(ref enemyHoverSeconds, 0.85f, 0.01f);
            changed |= EnsureMinimum(ref enemyHealthBarDeathFadeSeconds, 0.07f, 0f);

            return changed;
        }

        private bool EnsureDamageVignetteDefaults()
        {
            bool changed = false;
            if (playerDamageVignetteFlashSeconds <= 0f)
            {
                playerDamageVignetteEnabled = true;
                playerDamageVignetteFlashSeconds = 0.58f;
                changed = true;
            }

            changed |= EnsureRange(ref playerDamageVignetteMaxOpacity, DefaultDamageVignetteRedMaxOpacity, 0f, 1f);
            changed |= EnsureRange(ref playerDamageVignetteDarkMaxOpacity, DefaultDamageVignetteDarkMaxOpacity, 0f, 1f);
            changed |= EnsureMinimum(ref playerDamageVignetteDamageToIntensityScale, 10f, 0f);
            changed |= EnsureMinimum(ref playerDamageVignetteAccumulationCap, 1.25f, 0f);
            changed |= EnsureRange(ref playerDamageVignetteGrainStrength, DefaultDamageVignetteGrainStrength, 0f, 1f);
            changed |= EnsureRange(ref playerDamageVignetteSplotchStrength, DefaultDamageVignetteSplotchStrength, 0f, 1f);
            changed |= EnsureRange(ref playerDamageVignetteRadius, DefaultDamageVignetteRadius, 0f, 1f);
            changed |= EnsureRange(ref playerDamageVignetteSoftness, DefaultDamageVignetteSoftness, 0f, 1f);
            changed |= EnsureEaseDefault(ref playerDamageVignetteFadeEase, EaseSettings.OutCubic);
            if (playerDamageVignetteColor.a <= 0f)
            {
                playerDamageVignetteColor = new Color(0.95f, 0.02f, 0.03f, 1f);
                changed = true;
            }

            if (playerDamageVignetteDarkColor.a <= 0f)
            {
                playerDamageVignetteDarkColor = Color.black;
                changed = true;
            }

            return changed;
        }

        private bool EnsureHealingVignetteDefaults()
        {
            bool changed = false;
            if (playerHealingVignetteFlashSeconds <= 0f)
            {
                playerHealingVignetteEnabled = true;
                playerHealingVignetteFlashSeconds = 0.46f;
                changed = true;
            }

            changed |= EnsureRange(ref playerHealingVignetteMaxOpacity, DefaultHealingVignetteGreenMaxOpacity, 0f, 1f);
            changed |= EnsureRange(ref playerHealingVignetteDarkMaxOpacity, DefaultHealingVignetteDarkMaxOpacity, 0f, 1f);
            changed |= EnsureMinimum(ref playerHealingVignetteHealToIntensityScale, 4f, 0f);
            changed |= EnsureMinimum(ref playerHealingVignetteAccumulationCap, 0.90f, 0f);
            changed |= EnsureRange(ref playerHealingVignetteGrainStrength, DefaultHealingVignetteGrainStrength, 0f, 1f);
            changed |= EnsureRange(ref playerHealingVignetteSplotchStrength, DefaultHealingVignetteSplotchStrength, 0f, 1f);
            changed |= EnsureRange(ref playerHealingVignetteRadius, DefaultHealingVignetteRadius, 0f, 1f);
            changed |= EnsureRange(ref playerHealingVignetteSoftness, DefaultHealingVignetteSoftness, 0f, 1f);
            changed |= EnsureEaseDefault(ref playerHealingVignetteFadeEase, EaseSettings.OutCubic);
            if (playerHealingVignetteColor.a <= 0f)
            {
                playerHealingVignetteColor = new Color(0.12f, 1f, 0.34f, 1f);
                changed = true;
            }

            if (playerHealingVignetteDarkColor.a <= 0f)
            {
                playerHealingVignetteDarkColor = Color.black;
                changed = true;
            }

            return changed;
        }

        private void OnValidate()
        {
            EnsureReadableDefaults();
        }

        private bool IsEnemyFlashEmissionValid(float value)
        {
            return ConfigValidationRules.ZeroOrPositive(value, "Enemy flash emission").IsValid;
        }

        private bool IsPlayerDamageFlashEmissionValid(float value)
        {
            return ConfigValidationRules.ZeroOrPositive(value, "Player damage flash emission").IsValid;
        }

        private static bool EnsureEaseDefault(ref EaseSettings settings, EaseSettings defaultValue)
        {
            if (settings.shape > 0f)
            {
                return false;
            }

            settings = defaultValue;
            return true;
        }

        private static bool EnsureMinimum(ref float value, float defaultValue, float minimum)
        {
            if (value >= minimum && !Mathf.Approximately(value, 0f))
            {
                return false;
            }

            value = defaultValue;
            return true;
        }

        private static bool EnsureMinimumAllowZero(ref float value, float defaultValue)
        {
            if (value >= 0f)
            {
                return false;
            }

            value = defaultValue;
            return true;
        }

        private static bool EnsureMinimumInt(ref int value, int defaultValue, int minimum)
        {
            if (value >= minimum)
            {
                return false;
            }

            value = defaultValue;
            return true;
        }

        private static bool EnsureLayerMask(ref LayerMask mask, int defaultValue)
        {
            if (mask.value != 0)
            {
                return false;
            }

            mask = defaultValue;
            return true;
        }

        private static bool EnsureZeroOrPositive(ref float value, float defaultValue)
        {
            if (value >= 0f)
            {
                return false;
            }

            value = defaultValue;
            return true;
        }

        private static bool EnsureRange(ref float value, float defaultValue, float minimum, float maximum)
        {
            if (value >= minimum && value <= maximum)
            {
                return false;
            }

            value = Mathf.Clamp(defaultValue, minimum, maximum);
            return true;
        }
    }
}
