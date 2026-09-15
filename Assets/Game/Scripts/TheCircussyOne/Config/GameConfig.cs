using Sirenix.OdinInspector;
using UnityEngine;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;

namespace TheCircussyOne.Config
{
    [CreateAssetMenu(menuName = "The Circussy One/Game Config", fileName = "GameConfig")]
    [InfoBox("LIVE RUNTIME: normal balance tuning does not need scene rebuild. Restart Play Mode to refresh start-time values; spawn-time values affect newly spawned entities.")]
    public sealed class GameConfig : SerializedScriptableObject
    {
        private const string Tabs = "Game Config";

        [TabGroup(Tabs, "Overview"), BoxGroup(Tabs + "/Overview/Designer Guide")]
        [ShowInInspector, ReadOnly, MultiLineProperty(5), HideLabel, PropertyOrder(-100)]
        private string DesignerTuningGuide =>
            "Live Runtime: balance/content values update on Play Mode restart or newly spawned entities.\n" +
            "Projected Visual: use Config Hub Apply for scene previews; no scene rebuild.\n" +
            "Structural Reset: only rebuild the prototype scene when generated objects, prefabs, UI, or DI references need recovery.\n" +
            "If a field is disabled, its current mode does not consume it at runtime.";

        [TabGroup(Tabs, "Overview"), BoxGroup(Tabs + "/Overview/Quick Status")]
        [ShowInInspector, ReadOnly, LabelText("Progression"), PropertyOrder(-90)]
        private string ProgressionOverview => $"XP curve x{experienceCurveRequirementMultiplier:0.##}; next-level targets: {ExperienceCurveTargetPreview}";

        [TabGroup(Tabs, "Overview"), BoxGroup(Tabs + "/Overview/Quick Status")]
        [ShowInInspector, ReadOnly, LabelText("Enemy Movement"), PropertyOrder(-89)]
        private string EnemyMovementOverview => $"{enemyMoveSpeed:0.##} u/s, turn {enemyTurnDegreesPerSecond:0.#} deg/sec, soft separation radius {enemySeparationRadius:0.##}u";

        [TabGroup(Tabs, "Overview"), BoxGroup(Tabs + "/Overview/Quick Status")]
        [ShowInInspector, ReadOnly, LabelText("Enemy Climb"), PropertyOrder(-88)]
        private string EnemyClimbOverview => enemyClimbingEnabled
            ? $"Environment climb on; {(enemyUseMoveSpeedForClimb ? $"move speed x{enemyClimbSpeedMultiplier:0.##}" : $"{enemyFallbackClimbSpeed:0.##} u/s fallback")}; max {enemyMaxEnvironmentClimbHeight:0.##}u"
            : "Environment climb off";

        [TabGroup(Tabs, "Overview"), BoxGroup(Tabs + "/Overview/Quick Status")]
        [ShowInInspector, ReadOnly, LabelText("Enemy Support Stacking"), PropertyOrder(-87)]
        private string EnemySupportStackingOverview => enemyPileClimbingEnabled
            ? $"Support stacking on; radius {enemyPileRadius:0.##}u; starts at {enemyPileStartCount}; max layers {enemyPileMaxLayers}; climb speed x{enemySupportClimbSpeedMultiplier:0.##}"
            : "Support stacking off";

        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Core"), LabelWidth(190), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.PlayerColor")]
        [Min(1f)] public float playerMoveSpeed = 8f;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Core"), LabelWidth(190)]
        [Min(1)] public int playerMaxHealth = 100;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Core"), LabelWidth(190), SuffixLabel("sec")]
        [Min(0.1f)] public float contactDamageCooldown = 1f;

        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/SM64 Ground Feel"), LabelWidth(190), NumericSlider(0f, 0.5f)]
        [InfoBox("Deadzone suppresses tiny analog drift before camera-relative acceleration is evaluated.")]
        public float playerInputDeadzone = 0.12f;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/SM64 Ground Feel"), LabelWidth(190), SuffixLabel("u/s")]
        [Min(0.1f)] public float playerWalkSpeed = 3.5f;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/SM64 Ground Feel"), LabelWidth(190), SuffixLabel("u/s")]
        [Min(0.1f)] public float playerRunSpeed = 11.5f;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/SM64 Ground Feel"), LabelWidth(190)]
        [Min(0.1f)] public float playerAcceleration = 38f;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/SM64 Ground Feel"), LabelWidth(190)]
        [Min(0.1f)] public float playerDeceleration = 30f;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/SM64 Ground Feel"), LabelWidth(190)]
        [Min(0.1f)] public float playerTurnAcceleration = 24f;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/SM64 Ground Feel"), LabelWidth(190), SuffixLabel("deg")]
        [NumericSlider(90f, 180f)] public float playerReverseSkidAngle = 125f;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/SM64 Ground Feel"), LabelWidth(190)]
        [Min(0.1f)] public float playerSkidFriction = 42f;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/SM64 Ground Feel"), LabelWidth(190)]
        [Min(0.1f)] public float playerRotationSharpness = 16f;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Ground Collision"), LabelWidth(190), SuffixLabel("u")]
        [InfoBox("CharacterController step offset: how high a ledge can be stepped onto without jumping.")]
        [Min(0f)] public float playerControllerStepOffset = 0.45f;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Ground Collision"), LabelWidth(190), SuffixLabel("deg")]
        [InfoBox("CharacterController slope limit: maximum floor angle the player can walk up.")]
        [NumericSlider(0f, 89f)] public float playerControllerSlopeLimit = 45f;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Ground Collision"), LabelWidth(190), SuffixLabel("u")]
        [InfoBox("Short visual grounding probe used to keep ramp/step descent from looking airborne.")]
        [NumericSlider(0f, 1f)] public float playerGroundProbeDistance = 0.35f;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Ground Collision"), LabelWidth(190), SuffixLabel("deg")]
        [InfoBox("Maximum surface angle that can still drive visual grounding and incline tilt. This can be steeper than the walkable slope limit.")]
        [NumericSlider(0f, 89f)] public float playerVisualGroundProbeMaxSlope = 75f;

        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Interactions"), LabelWidth(190), SuffixLabel("u")]
        [InfoBox("Deliberate world interactions use this separate radius. Pickup Range does not affect chests, shrines, Ticket deposits, or the Stage Door.")]
        [Min(0.1f)] public float playerInteractRadius = 2.25f;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Interactions"), LabelWidth(190), SuffixLabel("sec")]
        [InfoBox("After leaving range while holding interact, progress pauses for this grace window before cancelling.")]
        [Min(0f)] public float playerInteractOutOfRangeGraceSeconds = 0.6f;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Interactions/Line Of Sight"), LabelWidth(190)]
        [InfoBox("When enabled, terrain, terrace walls, ramp sides, and the arena barrier block deliberate interactions.")]
        public bool playerInteractRequiresLineOfSight = true;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Interactions/Line Of Sight"), LabelWidth(190), SuffixLabel("u")]
        [EnableIf(nameof(playerInteractRequiresLineOfSight))]
        [Min(0f)] public float playerInteractLineOfSightStartHeight = 1.1f;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Interactions/Line Of Sight"), LabelWidth(190), SuffixLabel("u")]
        [EnableIf(nameof(playerInteractRequiresLineOfSight))]
        [Min(0f)] public float playerInteractLineOfSightTargetHeight = 0.65f;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Interactions/Line Of Sight"), LabelWidth(190), SuffixLabel("u")]
        [EnableIf(nameof(playerInteractRequiresLineOfSight))]
        [Min(0.001f)] public float playerInteractLineOfSightProbeRadius = 0.08f;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Interactions/Line Of Sight"), LabelWidth(190)]
        [EnableIf(nameof(playerInteractRequiresLineOfSight))]
        [InfoBox("Runtime excludes Player, Enemy, Projectile, and Pickup layers from this mask so reward props and gameplay hitboxes do not block interaction visibility.")]
        public LayerMask playerInteractLineOfSightMask = ~0;

        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Jump"), LabelWidth(190)]
        [Min(1)] public int playerMaxJumpCount = 1;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Jump"), LabelWidth(190), SuffixLabel("u")]
        [Min(0.1f)] public float playerJumpHeight = 3f;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Jump"), LabelWidth(190)]
        [Min(0.1f)] public float playerGravity = 30f;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Jump"), LabelWidth(190), SuffixLabel("x")]
        [Min(0f)] public float playerFallGravityMultiplier = 1.55f;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Jump"), LabelWidth(190), SuffixLabel("x")]
        [Min(0f)] public float playerLowJumpGravityMultiplier = 2.1f;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Jump"), LabelWidth(190), SuffixLabel("u/s")]
        [Min(0.1f)] public float playerTerminalFallSpeed = 32f;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Jump Assist"), LabelWidth(190), SuffixLabel("sec")]
        [NumericSlider(0f, 0.25f)] public float playerCoyoteSeconds = 0.08f;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Jump Assist"), LabelWidth(190), SuffixLabel("sec")]
        [NumericSlider(0f, 0.25f)] public float playerJumpBufferSeconds = 0.10f;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Jump Assist"), LabelWidth(190), SuffixLabel("sec")]
        [InfoBox("Prevents tiny ramp/step grounding blips from playing landing squash/VFX.")]
        [NumericSlider(0f, 0.25f)] public float playerLandingPulseMinAirTime = 0.08f;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Jump Assist"), LabelWidth(190), SuffixLabel("sec")]
        [InfoBox("Prevents short downhill ramp probe gaps from switching the performer into the jump animation.")]
        [NumericSlider(0f, 0.35f)] public float playerAirborneAnimationMinAirTime = 0.14f;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Air Control"), LabelWidth(190)]
        [Min(0f)] public float playerAirAcceleration = 18f;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Air Control"), LabelWidth(190)]
        [Min(0f)] public float playerAirDeceleration = 6f;
        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Air Control"), LabelWidth(190)]
        [Min(0f)] public float playerAirTurnAcceleration = 12f;

        [TabGroup(Tabs, "Arena"), BoxGroup(Tabs + "/Arena/World Bounds"), LabelWidth(190), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.VisualColor"), SuffixLabel("u")]
        [Min(8f)] public float arenaRadius = 58f;
        [TabGroup(Tabs, "Arena"), BoxGroup(Tabs + "/Arena/Pickups"), LabelWidth(190), SuffixLabel("u")]
        [Min(1f)] public float pickupMagnetRadius = 7.5f;
        [TabGroup(Tabs, "Arena"), BoxGroup(Tabs + "/Arena/Pickups"), LabelWidth(190), SuffixLabel("u")]
        [ValidateInput(nameof(IsPickupCollectRadiusValid), "Collect radius should not be larger than magnet radius.")]
        [Min(0.1f)] public float pickupCollectRadius = 1.2f;

        [TabGroup(Tabs, "Enemies"), BoxGroup(Tabs + "/Enemies/Spawn Timing"), LabelWidth(190), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.CombatColor"), SuffixLabel("sec")]
        [ValidateInput(nameof(IsSpawnIntervalValid), "Spawn interval must be greater than or equal to the minimum spawn interval.")]
        [Min(0.1f)] public float spawnIntervalSeconds = 1.25f;
        [TabGroup(Tabs, "Enemies"), BoxGroup(Tabs + "/Enemies/Spawn Timing"), LabelWidth(190), SuffixLabel("sec")]
        [ValidateInput(nameof(IsMinSpawnIntervalValid), "Minimum spawn interval cannot exceed the starting spawn interval.")]
        [Min(0.1f)] public float minSpawnIntervalSeconds = 0.22f;
        [TabGroup(Tabs, "Enemies"), BoxGroup(Tabs + "/Enemies/Spawn Timing"), LabelWidth(190), SuffixLabel("sec")]
        [Min(1f)] public float spawnRampDurationSeconds = 300f;
        [TabGroup(Tabs, "Enemies"), BoxGroup(Tabs + "/Enemies/Population"), LabelWidth(190)]
        [ValidateInput(nameof(IsMaxEnemiesValid), "Starting max enemies cannot exceed peak max enemies.")]
        [Min(1)] public int maxEnemies = 30;
        [TabGroup(Tabs, "Enemies"), BoxGroup(Tabs + "/Enemies/Population"), LabelWidth(190)]
        [InfoBox("Peak enemy cap is unusually high; keep only if the current target hardware has been smoke-tested.", InfoMessageType.Warning, nameof(HasHighEnemyCap))]
        [Min(1)] public int maxEnemiesAtPeak = 170;
        [TabGroup(Tabs, "Enemies"), BoxGroup(Tabs + "/Enemies/Population"), LabelWidth(190), SuffixLabel("u")]
        [Min(1f)] public float enemySpawnRadius = 42f;
        [TabGroup(Tabs, "Enemies"), BoxGroup(Tabs + "/Enemies/Stats"), LabelWidth(190)]
        [Min(1)] public int enemyHealth = 170;
        [TabGroup(Tabs, "Enemies"), BoxGroup(Tabs + "/Enemies/Stats"), LabelWidth(190), SuffixLabel("/min")]
        [Min(0f)] public float enemyHealthGrowthPerMinute = 20f;
        [TabGroup(Tabs, "Enemies"), BoxGroup(Tabs + "/Enemies/Stats"), LabelWidth(190), SuffixLabel("u/s")]
        [InfoBox("No rebuild needed. Newly spawned enemies use this value; enemies already alive keep their spawn-time move speed.")]
        [Min(0.1f)] public float enemyMoveSpeed = 2f;
        [TabGroup(Tabs, "Enemies"), BoxGroup(Tabs + "/Enemies/Stats"), LabelWidth(190), SuffixLabel("/min")]
        [Min(0f)] public float enemyMoveSpeedGrowthPerMinute = 0.24f;
        [TabGroup(Tabs, "Enemies"), BoxGroup(Tabs + "/Enemies/Stats"), LabelWidth(190), SuffixLabel("deg/sec")]
        [InfoBox("Live runtime tuning. Enemy bodies and forward contact hitboxes turn at this maximum rate before moving forward.")]
        [Min(1f)] public float enemyTurnDegreesPerSecond = 540f;
        [TabGroup(Tabs, "Enemies"), BoxGroup(Tabs + "/Enemies/Spacing"), LabelWidth(190), SuffixLabel("u")]
        [Min(0.1f)] public float enemySeparationRadius = 1.15f;
        [TabGroup(Tabs, "Enemies"), BoxGroup(Tabs + "/Enemies/Spacing"), LabelWidth(190), SuffixLabel("u")]
        [Min(0.1f)] public float enemyPlayerStopDistance = 1.25f;
        [TabGroup(Tabs, "Enemies"), BoxGroup(Tabs + "/Enemies/Spacing"), LabelWidth(190)]
        [Min(0f)] public float enemySeparationWeight = 1.65f;
        [TabGroup(Tabs, "Enemies"), BoxGroup(Tabs + "/Enemies/Hitboxes"), LabelWidth(190), SuffixLabel("u")]
        [InfoBox("Player attacks use this generous enemy hurtbox, separate from the smaller contact hitbox that damages the player.")]
        [Min(0.01f)] public float enemyHurtboxRadius = 0.65f;
        [TabGroup(Tabs, "Enemies"), BoxGroup(Tabs + "/Enemies/Hitboxes"), LabelWidth(190), SuffixLabel("u")]
        [Min(0f)] public float enemyHurtboxHeightOffset = 0.95f;
        [TabGroup(Tabs, "Enemies"), BoxGroup(Tabs + "/Enemies/Hitboxes"), LabelWidth(190), SuffixLabel("u")]
        [InfoBox("Enemy-to-player contact damage uses this smaller forward-biased hitbox.")]
        [InfoBox("Contact damage front reach is behind the movement body front. The player may be pushed out before damage overlaps reliably.", InfoMessageType.Warning, nameof(HasBuriedContactDamageFront))]
        [MinValue(0.01f)] public Vector3 enemyContactHitboxSize = new(0.75f, 0.9f, 0.65f);
        [TabGroup(Tabs, "Enemies"), BoxGroup(Tabs + "/Enemies/Hitboxes"), LabelWidth(190), SuffixLabel("u")]
        [InfoBox("Contact damage front reach is behind the movement body front. The player may be pushed out before damage overlaps reliably.", InfoMessageType.Warning, nameof(HasBuriedContactDamageFront))]
        public Vector3 enemyContactHitboxOffset = new(0f, 0.55f, 0.35f);
        [TabGroup(Tabs, "Enemies"), BoxGroup(Tabs + "/Enemies/Movement Body"), LabelWidth(190), SuffixLabel("u")]
        [InfoBox("Player blocking uses this capsule, separate from the projectile hurtbox and damage hitbox. Enemy-enemy spacing remains soft steering.")]
        [Min(0.01f)] public float enemyMovementBodyRadius = 0.52f;
        [TabGroup(Tabs, "Enemies"), BoxGroup(Tabs + "/Enemies/Movement Body"), LabelWidth(190), SuffixLabel("u")]
        [Min(0.01f)] public float enemyMovementBodyHeight = 1.45f;
        [TabGroup(Tabs, "Enemies"), BoxGroup(Tabs + "/Enemies/Movement Body"), LabelWidth(190), SuffixLabel("u")]
        public Vector3 enemyMovementBodyOffset = new(0f, 0.75f, 0f);
        [TabGroup(Tabs, "Enemies"), BoxGroup(Tabs + "/Enemies/Movement Body"), LabelWidth(190), SuffixLabel("u/frame")]
        [Min(0f)] public float playerEnemyBlockMaxPushPerFrame = 0.75f;
        [HideInInspector] public int enemyBodySeparationIterations = 2;
        [HideInInspector] public float enemyEnemyBlockMaxPushPerPair = 0.35f;
        [TabGroup(Tabs, "Enemies"), BoxGroup(Tabs + "/Enemies/Climbing"), LabelWidth(190)]
        [InfoBox("Enemies stay kinematic and climb all non-gameplay colliders by default. Disable only for flat-ground debugging.")]
        public bool enemyClimbingEnabled = true;
        [TabGroup(Tabs, "Enemies"), BoxGroup(Tabs + "/Enemies/Climbing"), LabelWidth(190)]
        [EnableIf(nameof(IsEnemyEnvironmentClimbActive))]
        [InfoBox("Disabled because enemy climbing is off.", InfoMessageType.Info, nameof(ShowEnemyEnvironmentClimbDisabledReason))]
        public bool enemyUseMoveSpeedForClimb = true;
        [TabGroup(Tabs, "Enemies"), BoxGroup(Tabs + "/Enemies/Climbing"), LabelWidth(190), SuffixLabel("x")]
        [EnableIf(nameof(IsEnemyEnvironmentClimbActive))]
        [InfoBox("Disabled because enemy climbing is off.", InfoMessageType.Info, nameof(ShowEnemyEnvironmentClimbDisabledReason))]
        [Min(0f)] public float enemyClimbSpeedMultiplier = 1f;
        [TabGroup(Tabs, "Enemies"), BoxGroup(Tabs + "/Enemies/Climbing"), LabelWidth(190), SuffixLabel("u/s")]
        [EnableIf(nameof(IsEnemyFallbackClimbSpeedActive))]
        [InfoBox("Disabled because enemy climbing is off or climb speed is using enemy move speed.", InfoMessageType.Info, nameof(ShowEnemyFallbackClimbSpeedDisabledReason))]
        [Min(0f)] public float enemyFallbackClimbSpeed = 4f;
        [TabGroup(Tabs, "Enemies"), BoxGroup(Tabs + "/Enemies/Climbing"), LabelWidth(190)]
        [Min(0f)] public float enemyClimbGravity = 35f;
        [TabGroup(Tabs, "Enemies"), BoxGroup(Tabs + "/Enemies/Climbing"), LabelWidth(190), SuffixLabel("u/s")]
        [Min(0.1f)] public float enemyClimbTerminalFallSpeed = 28f;
        [TabGroup(Tabs, "Enemies"), BoxGroup(Tabs + "/Enemies/Climbing"), LabelWidth(190), SuffixLabel("u")]
        [EnableIf(nameof(IsEnemyEnvironmentClimbActive))]
        [InfoBox("Disabled because enemy climbing is off.", InfoMessageType.Info, nameof(ShowEnemyEnvironmentClimbDisabledReason))]
        [Min(0f)] public float enemyMaxEnvironmentClimbHeight = 8f;
        [TabGroup(Tabs, "Enemies"), BoxGroup(Tabs + "/Enemies/Climbing"), LabelWidth(190), SuffixLabel("u")]
        [EnableIf(nameof(IsEnemyEnvironmentClimbActive))]
        [InfoBox("Disabled because enemy climbing is off.", InfoMessageType.Info, nameof(ShowEnemyEnvironmentClimbDisabledReason))]
        [Min(0f)] public float enemyEnvironmentProbeDistance = 1.1f;
        [TabGroup(Tabs, "Enemies"), BoxGroup(Tabs + "/Enemies/Climbing"), LabelWidth(190), SuffixLabel("frames")]
        [EnableIf(nameof(IsEnemyEnvironmentClimbActive))]
        [InfoBox("Disabled because enemy climbing is off.", InfoMessageType.Info, nameof(ShowEnemyEnvironmentClimbDisabledReason))]
        [Min(1)] public int enemyEnvironmentProbeIntervalFrames = 4;
        [TabGroup(Tabs, "Enemies"), BoxGroup(Tabs + "/Enemies/Climbing"), LabelWidth(190)]
        [InfoBox("Runtime excludes Player, Enemy, Projectile, and Pickup layers from this mask so gameplay hitboxes are never climb surfaces.")]
        [EnableIf(nameof(IsEnemyEnvironmentClimbActive))]
        [InfoBox("Disabled because enemy climbing is off.", InfoMessageType.Info, nameof(ShowEnemyEnvironmentClimbDisabledReason))]
        public LayerMask enemyClimbEnvironmentMask = ~0;
        [TabGroup(Tabs, "Enemies"), BoxGroup(Tabs + "/Enemies/Support Stacking"), LabelWidth(190)]
        public bool enemyPileClimbingEnabled = true;
        [TabGroup(Tabs, "Enemies"), BoxGroup(Tabs + "/Enemies/Support Stacking"), LabelWidth(190), SuffixLabel("u")]
        [EnableIf(nameof(IsEnemySupportStackingActive))]
        [InfoBox("Disabled because enemy pile climbing is off.", InfoMessageType.Info, nameof(ShowEnemySupportStackingDisabledReason))]
        [Min(0.1f)] public float enemyPileRadius = 2.4f;
        [TabGroup(Tabs, "Enemies"), BoxGroup(Tabs + "/Enemies/Support Stacking"), LabelWidth(190)]
        [EnableIf(nameof(IsEnemySupportStackingActive))]
        [InfoBox("Disabled because enemy pile climbing is off.", InfoMessageType.Info, nameof(ShowEnemySupportStackingDisabledReason))]
        [Min(1)] public int enemyPileStartCount = 6;
        [TabGroup(Tabs, "Enemies"), BoxGroup(Tabs + "/Enemies/Support Stacking"), LabelWidth(190)]
        [EnableIf(nameof(IsEnemySupportStackingActive))]
        [InfoBox("Disabled because enemy pile climbing is off.", InfoMessageType.Info, nameof(ShowEnemySupportStackingDisabledReason))]
        [Min(1)] public int enemyPileEnemiesPerLayer = 6;
        [TabGroup(Tabs, "Enemies"), BoxGroup(Tabs + "/Enemies/Support Stacking"), LabelWidth(190)]
        [EnableIf(nameof(IsEnemySupportStackingActive))]
        [InfoBox("Disabled because enemy pile climbing is off.", InfoMessageType.Info, nameof(ShowEnemySupportStackingDisabledReason))]
        [Min(0)] public int enemyPileMaxLayers = 4;
        [TabGroup(Tabs, "Enemies"), BoxGroup(Tabs + "/Enemies/Support Stacking"), LabelWidth(190), SuffixLabel("x body height")]
        [EnableIf(nameof(IsEnemySupportStackingActive))]
        [InfoBox("Disabled because enemy pile climbing is off.", InfoMessageType.Info, nameof(ShowEnemySupportStackingDisabledReason))]
        [Min(0f)] public float enemyPileLayerHeightMultiplier = 1f;
        [TabGroup(Tabs, "Enemies"), BoxGroup(Tabs + "/Enemies/Support Stacking"), LabelWidth(190), SuffixLabel("x climb speed")]
        [InfoBox("Enemy-on-enemy support climbing uses this multiplier so stacking can feel heavier without slowing wall and terrain climbing.")]
        [EnableIf(nameof(IsEnemySupportStackingActive))]
        [InfoBox("Disabled because enemy pile climbing is off.", InfoMessageType.Info, nameof(ShowEnemySupportStackingDisabledReason))]
        [Min(0.05f)] public float enemySupportClimbSpeedMultiplier = 0.5f;
        [TabGroup(Tabs, "Enemies"), BoxGroup(Tabs + "/Enemies/Support Stacking"), LabelWidth(190), SuffixLabel("x separation")]
        [InfoBox("Reduces soft enemy separation only while an enemy is actively climbing another enemy, helping climbers stay attached to their support.")]
        [EnableIf(nameof(IsEnemySupportStackingActive))]
        [InfoBox("Disabled because enemy pile climbing is off.", InfoMessageType.Info, nameof(ShowEnemySupportStackingDisabledReason))]
        [NumericSlider(0f, 1f)] public float enemySupportClimbSeparationMultiplier = 0.15f;
        [TabGroup(Tabs, "Enemies"), BoxGroup(Tabs + "/Enemies/Hitbox Diagnostics"), ShowInInspector, ReadOnly, LabelText("Movement Front Reach"), SuffixLabel("u", true), PropertyOrder(90)]
        private float EnemyMovementBodyFrontReach => EnemyHitboxAuthoringRules.MovementBodyFrontReach(enemyMovementBodyOffset, enemyMovementBodyRadius);
        [TabGroup(Tabs, "Enemies"), BoxGroup(Tabs + "/Enemies/Hitbox Diagnostics"), ShowInInspector, ReadOnly, LabelText("Contact Front Reach"), SuffixLabel("u", true), PropertyOrder(91)]
        private float EnemyContactDamageFrontReach => EnemyHitboxAuthoringRules.ContactDamageFrontReach(enemyContactHitboxOffset, enemyContactHitboxSize);
        [TabGroup(Tabs, "Enemies"), BoxGroup(Tabs + "/Enemies/Hitbox Diagnostics"), ShowInInspector, ReadOnly, LabelText("Front Margin"), SuffixLabel("u", true), PropertyOrder(92)]
        private float EnemyContactFrontMargin => EnemyHitboxAuthoringRules.ContactFrontProtrusionMargin(
            enemyContactHitboxOffset,
            enemyContactHitboxSize,
            enemyMovementBodyOffset,
            enemyMovementBodyRadius);
        [TabGroup(Tabs, "Enemies"), BoxGroup(Tabs + "/Enemies/Hitbox Diagnostics"), Button("Fit Contact Front To Movement Body"), GUIColor(1f, 0.78f, 0.22f), PropertyOrder(93)]
        private void FitContactFrontToMovementBody()
        {
            enemyContactHitboxOffset = EnemyHitboxAuthoringRules.FitContactFrontToMovementBody(
                enemyContactHitboxOffset,
                enemyContactHitboxSize,
                enemyMovementBodyOffset,
                enemyMovementBodyRadius);
        }
        [TabGroup(Tabs, "Enemies"), BoxGroup(Tabs + "/Enemies/Stats"), LabelWidth(190)]
        [Min(1)] public int enemyContactDamage = 2;

        [TabGroup(Tabs, "Progression"), BoxGroup(Tabs + "/Progression/XP Sources"), ShowInInspector, ReadOnly, LabelText("XP Drop Source"), PropertyOrder(-20)]
        private string ExperienceDropSource => "Enemy XP comes from EnemyDefinition.xpBudget, XpGemCatalog.defaultEnemyXpBudget, and XpGemDefinition.xpAmount. GameConfig no longer owns per-pickup XP.";
        [TabGroup(Tabs, "Progression"), BoxGroup(Tabs + "/Progression/Experience Curve"), LabelWidth(190), LabelText("Requirement Multiplier"), SuffixLabel("x"), PropertyOrder(-10)]
        [InfoBox("Global level pacing knob. 3x means every displayed level target below costs three times the base curve.")]
        [Min(0.1f)] public float experienceCurveRequirementMultiplier = 3f;
        [TabGroup(Tabs, "Progression"), BoxGroup(Tabs + "/Progression/Experience Curve"), LabelWidth(190), LabelText("Early Base")]
        [InfoBox("Level-up XP target = piecewise base curve multiplied by Requirement Multiplier. Runtime target preview is shown below.")]
        [Min(1)] public int experienceCurveEarlyBase = 21;
        [TabGroup(Tabs, "Progression"), BoxGroup(Tabs + "/Progression/Experience Curve"), LabelWidth(190), LabelText("Early Increment"), SuffixLabel("/level")]
        [Min(0)] public int experienceCurveEarlyIncrement = 14;
        [TabGroup(Tabs, "Progression"), BoxGroup(Tabs + "/Progression/Experience Curve"), LabelWidth(190), LabelText("Early Ends At"), SuffixLabel("level")]
        [Min(1)] public int experienceCurveEarlyEndLevel = 10;
        [TabGroup(Tabs, "Progression"), BoxGroup(Tabs + "/Progression/Experience Curve"), LabelWidth(190), LabelText("Mid Base")]
        [Min(1)] public int experienceCurveMidBase = 161;
        [TabGroup(Tabs, "Progression"), BoxGroup(Tabs + "/Progression/Experience Curve"), LabelWidth(190), LabelText("Mid Increment"), SuffixLabel("/level")]
        [Min(0)] public int experienceCurveMidIncrement = 23;
        [TabGroup(Tabs, "Progression"), BoxGroup(Tabs + "/Progression/Experience Curve"), LabelWidth(190), LabelText("Mid Ends At"), SuffixLabel("level")]
        [ValidateInput(nameof(IsExperienceMidEndValid), "Mid curve end must be greater than early curve end.")]
        [Min(2)] public int experienceCurveMidEndLevel = 25;
        [TabGroup(Tabs, "Progression"), BoxGroup(Tabs + "/Progression/Experience Curve"), LabelWidth(190), LabelText("Late Base")]
        [Min(1)] public int experienceCurveLateBase = 506;
        [TabGroup(Tabs, "Progression"), BoxGroup(Tabs + "/Progression/Experience Curve"), LabelWidth(190), LabelText("Late Increment"), SuffixLabel("/level")]
        [Min(0)] public int experienceCurveLateIncrement = 37;
        [TabGroup(Tabs, "Progression"), BoxGroup(Tabs + "/Progression/Experience Curve Preview"), ShowInInspector, ReadOnly, LabelText("Target Preview"), PropertyOrder(20)]
        private string ExperienceCurveTargetPreview => $"L1 {ExperienceRequiredForNextLevel(1)} | L2 {ExperienceRequiredForNextLevel(2)} | L5 {ExperienceRequiredForNextLevel(5)} | L10 {ExperienceRequiredForNextLevel(10)} | L20 {ExperienceRequiredForNextLevel(20)} | L30 {ExperienceRequiredForNextLevel(30)}";
#if UNITY_EDITOR
        [TabGroup(Tabs, "Progression"), BoxGroup(Tabs + "/Progression/Experience Curve Preview"), Button("Open XP Pacing Calculator"), PropertyOrder(21)]
        private void OpenXpPacingCalculator()
        {
            System.Type windowType = System.Type.GetType("XpPacingCalculatorWindow, TheCircussyOne.Editor");
            System.Reflection.MethodInfo openMethod = windowType?.GetMethod("Open", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            openMethod?.Invoke(null, null);
        }
#endif
        [TabGroup(Tabs, "Feedback"), BoxGroup(Tabs + "/Feedback/Timing"), LabelWidth(190), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.VisualColor"), SuffixLabel("sec")]
        [Min(0.01f)] public float hitPulseSeconds = 0.1f;
        [TabGroup(Tabs, "Feedback"), BoxGroup(Tabs + "/Feedback/Timing"), LabelWidth(190), SuffixLabel("sec")]
        [Min(0.01f)] public float deathShrinkSeconds = 0.16f;
        [TabGroup(Tabs, "Feedback"), BoxGroup(Tabs + "/Feedback/Material Identity"), LabelWidth(190)]
        public Color enemyColor = new(0.78f, 0.12f, 0.18f, 1f);
        [TabGroup(Tabs, "Feedback"), BoxGroup(Tabs + "/Feedback/Material Identity"), LabelWidth(190)]
        public Color projectileColor = new(1f, 0.88f, 0.18f, 1f);
        [TabGroup(Tabs, "Feedback"), BoxGroup(Tabs + "/Feedback/Material Identity"), LabelWidth(190)]
        public Color pickupColor = new(0.27f, 0.96f, 1f, 1f);
        [TabGroup(Tabs, "Feedback"), BoxGroup(Tabs + "/Feedback/Material Identity"), LabelWidth(190)]
        public Color playerColor = new(0.38f, 0.2f, 0.92f, 1f);

        [TabGroup(Tabs, "Runtime"), BoxGroup(Tabs + "/Runtime/Startup Prewarm"), LabelWidth(190)]
        [InfoBox("Creates inactive pooled runtime objects while the transition overlay is visible, reducing first-use hitches after fade-in.")]
        public bool runtimePrewarmEnabled = true;
        [TabGroup(Tabs, "Runtime"), BoxGroup(Tabs + "/Runtime/Startup Prewarm"), LabelWidth(190), EnableIf(nameof(runtimePrewarmEnabled))]
        [Min(1)] public int runtimePrewarmObjectsPerFrame = 8;
        [TabGroup(Tabs, "Runtime"), BoxGroup(Tabs + "/Runtime/Startup Prewarm/Combat"), LabelWidth(190), EnableIf(nameof(runtimePrewarmEnabled))]
        [Min(0)] public int runtimePrewarmEnemyViews = 24;
        [TabGroup(Tabs, "Runtime"), BoxGroup(Tabs + "/Runtime/Startup Prewarm/Combat"), LabelWidth(190), EnableIf(nameof(runtimePrewarmEnabled))]
        [Min(0)] public int runtimePrewarmProjectileViews = 32;
        [TabGroup(Tabs, "Runtime"), BoxGroup(Tabs + "/Runtime/Startup Prewarm/Pickups"), LabelWidth(190), EnableIf(nameof(runtimePrewarmEnabled))]
        [Min(0)] public int runtimePrewarmPickupViews = 64;
        [TabGroup(Tabs, "Runtime"), BoxGroup(Tabs + "/Runtime/Startup Prewarm/Feedback"), LabelWidth(190), EnableIf(nameof(runtimePrewarmEnabled))]
        [Min(0)] public int runtimePrewarmDamageNumbers = 48;
        [TabGroup(Tabs, "Runtime"), BoxGroup(Tabs + "/Runtime/Startup Prewarm/Feedback"), LabelWidth(190), EnableIf(nameof(runtimePrewarmEnabled))]
        [Min(0)] public int runtimePrewarmProjectileExplosions = 8;
        [TabGroup(Tabs, "Runtime"), BoxGroup(Tabs + "/Runtime/Startup Prewarm/Feedback"), LabelWidth(190), EnableIf(nameof(runtimePrewarmEnabled))]
        [Min(0)] public int runtimePrewarmProjectileChainSegments = 16;
        [TabGroup(Tabs, "Runtime"), BoxGroup(Tabs + "/Runtime/Startup Prewarm/Feedback"), LabelWidth(190), EnableIf(nameof(runtimePrewarmEnabled))]
        [Min(0)] public int runtimePrewarmOrbitWeaponViews = 8;
        [TabGroup(Tabs, "Runtime"), BoxGroup(Tabs + "/Runtime/Startup Prewarm/Feedback"), LabelWidth(190), EnableIf(nameof(runtimePrewarmEnabled))]
        [Min(0)] public int runtimePrewarmFireHoopAreaTelegraphs = 2;
        [TabGroup(Tabs, "Runtime"), BoxGroup(Tabs + "/Runtime/Startup Prewarm/World Rewards"), LabelWidth(190), EnableIf(nameof(runtimePrewarmEnabled))]
        [Min(0)] public int runtimePrewarmTicketBurstVfx = 4;
        [TabGroup(Tabs, "Runtime"), BoxGroup(Tabs + "/Runtime/Startup Prewarm/World Rewards"), LabelWidth(190), EnableIf(nameof(runtimePrewarmEnabled))]
        [Min(0)] public int runtimePrewarmSnackBurstVfx = 4;
        [TabGroup(Tabs, "Runtime"), BoxGroup(Tabs + "/Runtime/Startup Prewarm/World Rewards"), LabelWidth(190), EnableIf(nameof(runtimePrewarmEnabled))]
        public bool runtimePrewarmWorldPrompt = true;
        [TabGroup(Tabs, "Runtime"), BoxGroup(Tabs + "/Runtime/Startup Prewarm/Feedback"), LabelWidth(190), EnableIf(nameof(runtimePrewarmEnabled))]
        public bool runtimePrewarmGainCounters = true;

        [TabGroup(Tabs, "Diagnostics"), BoxGroup(Tabs + "/Diagnostics/Smoothing & Easing"), LabelWidth(190), InlineProperty]
        public EaseSettings difficultyRampEase = EaseSettings.OutCubic;
        [TabGroup(Tabs, "Diagnostics"), BoxGroup(Tabs + "/Diagnostics/Smoothing & Easing"), LabelWidth(190), InlineProperty]
        public EaseSettings playerFacingSmoothingEase = EaseSettings.Exponential;

        [TabGroup(Tabs, "Diagnostics"), BoxGroup(Tabs + "/Diagnostics/Enemy Movement"), LabelWidth(190)]
        [InfoBox("Logs enemy environment blocker decisions, including generated ramp hit classification. Leave disabled unless diagnosing terrain traversal.")]
        public bool enemyEnvironmentBlockDiagnosticsEnabled;

        [TabGroup(Tabs, "Diagnostics"), ShowInInspector, ReadOnly, LabelText("Spawn Interval"), PropertyOrder(100)]
        private string SpawnIntervalSummary => $"{minSpawnIntervalSeconds:0.00}s -> {spawnIntervalSeconds:0.00}s over {spawnRampDurationSeconds:0}s";

        [TabGroup(Tabs, "Diagnostics"), ShowInInspector, ReadOnly, LabelText("Enemy Cap"), PropertyOrder(101)]
        private string EnemyCapSummary => $"{maxEnemies} starting / {maxEnemiesAtPeak} peak";

        [TabGroup(Tabs, "Diagnostics"), ShowInInspector, ReadOnly, LabelText("Player Controller"), PropertyOrder(102)]
        private string PlayerControllerSummary => $"Step {playerControllerStepOffset:0.##}u / walkable slope {playerControllerSlopeLimit:0.#} deg / visual ground slope {playerVisualGroundProbeMaxSlope:0.#} deg";

        private bool HasHighEnemyCap => ConfigValidationRules.WarningIfGreater(maxEnemiesAtPeak, 220f, "Peak enemy cap").Level == ConfigValidationLevel.Warning;
        private bool IsEnemyEnvironmentClimbActive => EnemyAuthoringApplicability.IsEnvironmentClimbingActive(enemyClimbingEnabled);
        private bool IsEnemyFallbackClimbSpeedActive => EnemyAuthoringApplicability.IsFallbackClimbSpeedActive(enemyClimbingEnabled, enemyUseMoveSpeedForClimb);
        private bool IsEnemySupportStackingActive => EnemyAuthoringApplicability.IsSupportStackingActive(enemyPileClimbingEnabled);
        private bool ShowEnemyEnvironmentClimbDisabledReason => !IsEnemyEnvironmentClimbActive;
        private bool ShowEnemyFallbackClimbSpeedDisabledReason => !IsEnemyFallbackClimbSpeedActive;
        private bool ShowEnemySupportStackingDisabledReason => !IsEnemySupportStackingActive;
        public ExperienceCurveSettings ExperienceCurve => new(
            experienceCurveEarlyEndLevel,
            experienceCurveMidEndLevel,
            experienceCurveEarlyBase,
            experienceCurveEarlyIncrement,
            experienceCurveMidBase,
            experienceCurveMidIncrement,
            experienceCurveLateBase,
            experienceCurveLateIncrement,
            experienceCurveRequirementMultiplier);

        public int ExperienceRequiredForNextLevel(int level)
        {
            return ExperienceRules.ExperienceRequiredForNextLevel(level, ExperienceCurve);
        }

        public bool HasValidEnemyContactFrontMargin => EnemyHitboxAuthoringRules.HasRequiredContactFrontMargin(
            enemyContactHitboxOffset,
            enemyContactHitboxSize,
            enemyMovementBodyOffset,
            enemyMovementBodyRadius);
        private bool HasBuriedContactDamageFront => !HasValidEnemyContactFrontMargin;

        public bool EnsureWorkflowDefaults()
        {
            bool changed = false;
            changed |= EnsureEaseDefault(ref difficultyRampEase, EaseSettings.OutCubic);
            changed |= EnsureEaseDefault(ref playerFacingSmoothingEase, EaseSettings.Exponential);
            changed |= EnsureMinimum(ref playerMaxJumpCount, 1, 1);
            changed |= EnsureMinimum(ref playerJumpHeight, 3f, 0.1f);
            changed |= EnsureMinimum(ref playerGravity, 30f, 0.1f);
            changed |= EnsureMinimum(ref playerFallGravityMultiplier, 1.55f, 0f);
            changed |= EnsureMinimum(ref playerLowJumpGravityMultiplier, 2.1f, 0f);
            changed |= EnsureMinimum(ref playerTerminalFallSpeed, 32f, 0.1f);
            changed |= EnsureMinimum(ref playerCoyoteSeconds, 0.08f, 0f);
            changed |= EnsureMinimum(ref playerJumpBufferSeconds, 0.10f, 0f);
            changed |= EnsureMinimum(ref playerLandingPulseMinAirTime, 0.08f, 0f);
            changed |= EnsureMinimum(ref playerAirborneAnimationMinAirTime, 0.14f, 0f);
            changed |= EnsureMinimum(ref playerControllerStepOffset, 0.45f, 0f);
            changed |= EnsureRange(ref playerControllerSlopeLimit, 45f, 0f, 89f);
            changed |= EnsureMinimum(ref playerGroundProbeDistance, 0.35f, 0f);
            changed |= EnsureRange(ref playerVisualGroundProbeMaxSlope, 75f, 0f, 89f);
            changed |= EnsureMinimum(ref playerInteractRadius, 2.25f, 0.1f);
            changed |= EnsureMinimum(ref playerInteractOutOfRangeGraceSeconds, 0.6f, 0f);
            changed |= EnsureMinimum(ref playerInteractLineOfSightStartHeight, 1.1f, 0f);
            changed |= EnsureMinimum(ref playerInteractLineOfSightTargetHeight, 0.65f, 0f);
            changed |= EnsureMinimum(ref playerInteractLineOfSightProbeRadius, 0.08f, 0.001f);
            changed |= EnsureMinimum(ref playerAirAcceleration, 18f, 0f);
            changed |= EnsureMinimum(ref playerAirDeceleration, 6f, 0f);
            changed |= EnsureMinimum(ref playerAirTurnAcceleration, 12f, 0f);
            changed |= EnsureMinimum(ref enemyTurnDegreesPerSecond, 540f, 1f);
            changed |= EnsureMinimum(ref enemyMovementBodyRadius, 0.52f, 0.01f);
            changed |= EnsureMinimum(ref enemyMovementBodyHeight, 1.45f, 0.01f);
            changed |= EnsureMinimum(ref playerEnemyBlockMaxPushPerFrame, 0.75f, 0f);
            changed |= EnsureMinimum(ref enemyClimbSpeedMultiplier, 1f, 0f);
            changed |= EnsureMinimum(ref enemyFallbackClimbSpeed, 4f, 0f);
            changed |= EnsureMinimum(ref enemyClimbGravity, 35f, 0f);
            changed |= EnsureMinimum(ref enemyClimbTerminalFallSpeed, 28f, 0.1f);
            changed |= EnsureMinimum(ref enemyMaxEnvironmentClimbHeight, 8f, 0f);
            changed |= EnsureMinimum(ref enemyEnvironmentProbeDistance, 1.1f, 0f);
            changed |= EnsureMinimum(ref enemyEnvironmentProbeIntervalFrames, 4, 1);
            changed |= EnsureMinimum(ref enemyPileRadius, 2.4f, 0.1f);
            changed |= EnsureMinimum(ref enemyPileStartCount, 6, 1);
            changed |= EnsureMinimum(ref enemyPileEnemiesPerLayer, 6, 1);
            changed |= EnsureMinimum(ref enemyPileMaxLayers, 4, 0);
            changed |= EnsureMinimum(ref enemyPileLayerHeightMultiplier, 1f, 0f);
            changed |= EnsureMinimum(ref enemySupportClimbSpeedMultiplier, 0.5f, 0.05f);
            changed |= EnsureRange(ref enemySupportClimbSeparationMultiplier, 0.15f, 0f, 1f);
            changed |= EnsureMinimum(ref experienceCurveEarlyBase, 21, 1);
            changed |= EnsureMinimum(ref experienceCurveEarlyIncrement, 14, 0);
            changed |= EnsureMinimum(ref experienceCurveEarlyEndLevel, 10, 1);
            changed |= EnsureMinimum(ref experienceCurveMidBase, 161, 1);
            changed |= EnsureMinimum(ref experienceCurveMidIncrement, 23, 0);
            if (experienceCurveMidEndLevel <= experienceCurveEarlyEndLevel)
            {
                experienceCurveMidEndLevel = Mathf.Max(25, experienceCurveEarlyEndLevel + 1);
                changed = true;
            }

            changed |= EnsureMinimum(ref experienceCurveLateBase, 506, 1);
            changed |= EnsureMinimum(ref experienceCurveLateIncrement, 37, 0);
            changed |= EnsureMinimum(ref experienceCurveRequirementMultiplier, 3f, 0.1f);
            changed |= EnsureMinimum(ref runtimePrewarmObjectsPerFrame, 8, 1);
            changed |= EnsureMinimum(ref runtimePrewarmEnemyViews, 24, 0);
            changed |= EnsureMinimum(ref runtimePrewarmProjectileViews, 32, 0);
            changed |= EnsureMinimum(ref runtimePrewarmPickupViews, 64, 0);
            changed |= EnsureMinimum(ref runtimePrewarmDamageNumbers, 48, 0);
            changed |= EnsureMinimum(ref runtimePrewarmProjectileExplosions, 8, 0);
            changed |= EnsureMinimum(ref runtimePrewarmProjectileChainSegments, 16, 0);
            changed |= EnsureMinimum(ref runtimePrewarmOrbitWeaponViews, 8, 0);
            changed |= EnsureMinimum(ref runtimePrewarmFireHoopAreaTelegraphs, 2, 0);
            changed |= EnsureMinimum(ref runtimePrewarmTicketBurstVfx, 4, 0);
            changed |= EnsureMinimum(ref runtimePrewarmSnackBurstVfx, 4, 0);
            if (enemyClimbEnvironmentMask.value == 0)
            {
                enemyClimbEnvironmentMask = ~0;
                changed = true;
            }
            if (playerInteractLineOfSightMask.value == 0)
            {
                playerInteractLineOfSightMask = ~0;
                changed = true;
            }

            return changed;
        }

        private void OnValidate()
        {
            EnsureWorkflowDefaults();
        }

        private bool IsSpawnIntervalValid(float value)
        {
            return ConfigValidationRules.MinLessOrEqual(minSpawnIntervalSeconds, value, "Minimum spawn interval", "Spawn interval").IsValid;
        }

        private bool IsMinSpawnIntervalValid(float value)
        {
            return ConfigValidationRules.MinLessOrEqual(value, spawnIntervalSeconds, "Minimum spawn interval", "Spawn interval").IsValid;
        }

        private bool IsMaxEnemiesValid(int value)
        {
            return value <= maxEnemiesAtPeak;
        }

        private bool IsPickupCollectRadiusValid(float value)
        {
            return value <= pickupMagnetRadius;
        }

        private bool IsExperienceMidEndValid(int value)
        {
            return value > experienceCurveEarlyEndLevel;
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
            if (value >= minimum)
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

        private static bool EnsureMinimum(ref int value, int defaultValue, int minimum)
        {
            if (value >= minimum)
            {
                return false;
            }

            value = defaultValue;
            return true;
        }
    }
}
