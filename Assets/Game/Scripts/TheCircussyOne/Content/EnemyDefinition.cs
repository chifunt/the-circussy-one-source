using System;
using Sirenix.OdinInspector;
using UnityEngine;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;

namespace TheCircussyOne.Content
{
    public enum EnemyBehaviorType
    {
        Chaser,
        Ranged,
        Stationary,
        Support
    }

    public enum EnemyLocomotionMode
    {
        Grounded,
        Floating
    }

    public enum EnemyStackPolicy
    {
        GroundOnly,
        DensityMound,
        SupportBased
    }

    [Serializable]
    public sealed class EnemyBodyProfile
    {
        [BoxGroup("Hurtbox"), LabelWidth(180), SuffixLabel("u")]
        [Min(0.01f)] public float hurtboxRadius = 0.65f;

        [BoxGroup("Hurtbox"), LabelWidth(180), SuffixLabel("u")]
        [Min(0f)] public float hurtboxHeightOffset = 0.95f;

        [BoxGroup("Contact Damage"), LabelWidth(180), SuffixLabel("u")]
        [MinValue(0.01f)] public Vector3 contactHitboxSize = new(0.75f, 0.9f, 0.65f);

        [BoxGroup("Contact Damage"), LabelWidth(180), SuffixLabel("u")]
        public Vector3 contactHitboxOffset = new(0f, 0.55f, 0.35f);

        [BoxGroup("Movement Body"), LabelWidth(180), SuffixLabel("u")]
        [Min(0.01f)] public float movementBodyRadius = 0.52f;

        [BoxGroup("Movement Body"), LabelWidth(180), SuffixLabel("u")]
        [Min(0.01f)] public float movementBodyHeight = 1.45f;

        [BoxGroup("Movement Body"), LabelWidth(180), SuffixLabel("u")]
        public Vector3 movementBodyOffset = new(0f, 0.75f, 0f);

        public void ApplyDefaultsFromGameConfig(GameConfig config)
        {
            if (config != null)
            {
                hurtboxRadius = config.enemyHurtboxRadius;
                hurtboxHeightOffset = config.enemyHurtboxHeightOffset;
                contactHitboxSize = config.enemyContactHitboxSize;
                contactHitboxOffset = config.enemyContactHitboxOffset;
                movementBodyRadius = config.enemyMovementBodyRadius;
                movementBodyHeight = config.enemyMovementBodyHeight;
                movementBodyOffset = config.enemyMovementBodyOffset;
            }

            EnsureWorkflowDefaults();
        }

        public bool EnsureWorkflowDefaults()
        {
            bool changed = false;
            changed |= EnsureMinimum(ref hurtboxRadius, 0.65f, 0.01f);
            changed |= EnsureMinimum(ref hurtboxHeightOffset, 0.95f, 0f);
            changed |= EnsureMinimum(ref contactHitboxSize.x, 0.75f, 0.01f);
            changed |= EnsureMinimum(ref contactHitboxSize.y, 0.9f, 0.01f);
            changed |= EnsureMinimum(ref contactHitboxSize.z, 0.65f, 0.01f);
            changed |= EnsureMinimum(ref movementBodyRadius, 0.52f, 0.01f);
            changed |= EnsureMinimum(ref movementBodyHeight, 1.45f, 0.01f);
            return changed;
        }

        public EnemyBodyProfile Copy()
        {
            return new EnemyBodyProfile
            {
                hurtboxRadius = hurtboxRadius,
                hurtboxHeightOffset = hurtboxHeightOffset,
                contactHitboxSize = contactHitboxSize,
                contactHitboxOffset = contactHitboxOffset,
                movementBodyRadius = movementBodyRadius,
                movementBodyHeight = movementBodyHeight,
                movementBodyOffset = movementBodyOffset
            };
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
    }

    [Serializable]
    public sealed class EnemyLocomotionProfile
    {
        [BoxGroup("Mode"), LabelWidth(190), EnumToggleButtons]
        public EnemyLocomotionMode mode = EnemyLocomotionMode.Grounded;

        [BoxGroup("Floating Hover"), LabelWidth(190), SuffixLabel("u")]
        [ShowIf(nameof(IsFloating))]
        [Min(0f)] public float hoverHeight = 0.15f;

        [BoxGroup("Floating Hover"), LabelWidth(190), SuffixLabel("sec")]
        [ShowIf(nameof(IsFloating))]
        [Min(0.01f)] public float hoverSeconds = 0.85f;

        [BoxGroup("Floating Hover"), LabelWidth(190)]
        [ShowIf(nameof(IsFloating))]
        public EaseSettings hoverEase = EaseSettings.InOutSine;

        [BoxGroup("Diagnostics"), ShowInInspector, ReadOnly, LabelText("Runtime Summary"), PropertyOrder(100)]
        private string RuntimeSummary => mode == EnemyLocomotionMode.Floating
            ? $"Floating / hover {hoverHeight:0.##}u every {hoverSeconds:0.##} sec"
            : "Grounded / no idle body hover";

        private bool IsFloating => mode == EnemyLocomotionMode.Floating;

        public bool EnsureWorkflowDefaults()
        {
            bool changed = false;
            if (!Enum.IsDefined(typeof(EnemyLocomotionMode), mode))
            {
                mode = EnemyLocomotionMode.Grounded;
                changed = true;
            }

            changed |= EnsureMinimum(ref hoverHeight, 0.15f, 0f);
            changed |= EnsureMinimum(ref hoverSeconds, 0.85f, 0.01f);
            return changed;
        }

        public EnemyLocomotionProfile Copy()
        {
            return new EnemyLocomotionProfile
            {
                mode = mode,
                hoverHeight = hoverHeight,
                hoverSeconds = hoverSeconds,
                hoverEase = hoverEase
            };
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
    }

    [Serializable]
    public sealed class EnemyClimbProfile
    {
        [BoxGroup("Environment"), LabelWidth(190)]
        public bool canClimbEnvironment = true;

        [BoxGroup("Motion"), LabelWidth(190)]
        [ShowIf(nameof(IsEnvironmentClimbingActive))]
        public bool useMoveSpeedForClimb = true;

        [BoxGroup("Motion"), LabelWidth(190), SuffixLabel("x")]
        [ShowIf(nameof(IsEnvironmentClimbingActive))]
        [Min(0f)] public float climbSpeedMultiplier = 1f;

        [BoxGroup("Motion"), LabelWidth(190), SuffixLabel("u/s")]
        [ShowIf(nameof(IsFallbackClimbSpeedActive))]
        [Min(0f)] public float fallbackClimbSpeed = 4f;

        [BoxGroup("Motion"), LabelWidth(190)]
        [Min(0f)] public float gravity = 35f;

        [BoxGroup("Motion"), LabelWidth(190), SuffixLabel("u/s")]
        [Min(0.1f)] public float terminalFallSpeed = 28f;

        [BoxGroup("Environment"), LabelWidth(190), SuffixLabel("u")]
        [ShowIf(nameof(IsEnvironmentClimbingActive))]
        [Min(0f)] public float maxEnvironmentClimbHeight = 8f;

        [BoxGroup("Environment"), LabelWidth(190), SuffixLabel("u")]
        [ShowIf(nameof(IsEnvironmentClimbingActive))]
        [Min(0f)] public float environmentProbeDistance = 1.1f;

        [BoxGroup("Environment"), LabelWidth(190), SuffixLabel("frames")]
        [ShowIf(nameof(IsEnvironmentClimbingActive))]
        [Min(1)] public int environmentProbeIntervalFrames = 4;

        [BoxGroup("Diagnostics"), ShowInInspector, ReadOnly, LabelText("Runtime Summary"), PropertyOrder(100)]
        private string RuntimeSummary => EnemyAuthoringApplicability.ClimbSummary(this);

        private bool IsEnvironmentClimbingActive => EnemyAuthoringApplicability.IsEnvironmentClimbingActive(this);
        private bool IsFallbackClimbSpeedActive => EnemyAuthoringApplicability.IsFallbackClimbSpeedActive(this);
        private bool ShowEnvironmentClimbingDisabledReason => !IsEnvironmentClimbingActive;
        private bool ShowFallbackClimbSpeedDisabledReason => !IsFallbackClimbSpeedActive;

        public void ApplyDefaultsFromGameConfig(GameConfig config)
        {
            if (config != null)
            {
                canClimbEnvironment = config.enemyClimbingEnabled;
                useMoveSpeedForClimb = config.enemyUseMoveSpeedForClimb;
                climbSpeedMultiplier = config.enemyClimbSpeedMultiplier;
                fallbackClimbSpeed = config.enemyFallbackClimbSpeed;
                gravity = config.enemyClimbGravity;
                terminalFallSpeed = config.enemyClimbTerminalFallSpeed;
                maxEnvironmentClimbHeight = config.enemyMaxEnvironmentClimbHeight;
                environmentProbeDistance = config.enemyEnvironmentProbeDistance;
                environmentProbeIntervalFrames = config.enemyEnvironmentProbeIntervalFrames;
            }

            EnsureWorkflowDefaults();
        }

        public bool EnsureWorkflowDefaults()
        {
            bool changed = false;
            changed |= EnsureMinimum(ref climbSpeedMultiplier, 1f, 0f);
            changed |= EnsureMinimum(ref fallbackClimbSpeed, 4f, 0f);
            changed |= EnsureMinimum(ref gravity, 35f, 0f);
            changed |= EnsureMinimum(ref terminalFallSpeed, 28f, 0.1f);
            changed |= EnsureMinimum(ref maxEnvironmentClimbHeight, 8f, 0f);
            changed |= EnsureMinimum(ref environmentProbeDistance, 1.1f, 0f);
            changed |= EnsureMinimum(ref environmentProbeIntervalFrames, 4, 1);
            return changed;
        }

        public EnemyClimbProfile Copy()
        {
            return new EnemyClimbProfile
            {
                canClimbEnvironment = canClimbEnvironment,
                useMoveSpeedForClimb = useMoveSpeedForClimb,
                climbSpeedMultiplier = climbSpeedMultiplier,
                fallbackClimbSpeed = fallbackClimbSpeed,
                gravity = gravity,
                terminalFallSpeed = terminalFallSpeed,
                maxEnvironmentClimbHeight = maxEnvironmentClimbHeight,
                environmentProbeDistance = environmentProbeDistance,
                environmentProbeIntervalFrames = environmentProbeIntervalFrames
            };
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

    [Serializable]
    public sealed class EnemyStackProfile
    {
        [BoxGroup("Policy"), LabelWidth(190)]
        [EnumToggleButtons] public EnemyStackPolicy policy = EnemyStackPolicy.GroundOnly;

        [BoxGroup("Policy"), LabelWidth(190)]
        [ShowIf(nameof(IsSupportStackingActive))]
        public bool canClimbEnemies;

        [BoxGroup("Policy"), LabelWidth(190)]
        [ShowIf(nameof(IsSupportStackingActive))]
        public bool canBeStackedOn = true;

        [BoxGroup("Support Stacking"), LabelWidth(190), SuffixLabel("u")]
        [ShowIf(nameof(IsSupportStackingActive))]
        [Min(0.1f)] public float pileRadius = 2.4f;

        [BoxGroup("Support Stacking"), LabelWidth(190)]
        [ShowIf(nameof(IsSupportStackingActive))]
        [Min(1)] public int pileStartCount = 6;

        [BoxGroup("Support Stacking"), LabelWidth(190)]
        [ShowIf(nameof(IsSupportStackingActive))]
        [Min(1)] public int pileEnemiesPerLayer = 6;

        [BoxGroup("Support Stacking"), LabelWidth(190)]
        [ShowIf(nameof(IsSupportStackingActive))]
        [Min(0)] public int maxStackLayers = 4;

        [BoxGroup("Support Stacking"), LabelWidth(190), SuffixLabel("x body height")]
        [ShowIf(nameof(IsSupportStackingActive))]
        [Min(0f)] public float layerHeightMultiplier = 1f;

        [BoxGroup("Support Stacking"), LabelWidth(190), SuffixLabel("x climb speed")]
        [ShowIf(nameof(IsSupportStackingActive))]
        [Min(0.05f)] public float supportClimbSpeedMultiplier = 0.5f;

        [BoxGroup("Support Stacking"), LabelWidth(190), SuffixLabel("x separation")]
        [ShowIf(nameof(IsSupportStackingActive))]
        [Range(0f, 1f)] public float supportClimbSeparationMultiplier = 0.15f;

        [BoxGroup("Diagnostics"), ShowInInspector, ReadOnly, LabelText("Runtime Summary"), PropertyOrder(100)]
        private string RuntimeSummary => EnemyAuthoringApplicability.StackSummary(this);

        private bool IsSupportStackingActive => EnemyAuthoringApplicability.IsSupportStackingActive(this);
        private bool ShowSupportStackingDisabledReason => !IsSupportStackingActive;

        public void ApplyDefaultsFromGameConfig(GameConfig config)
        {
            policy = EnemyStackPolicy.SupportBased;
            canClimbEnemies = true;
            canBeStackedOn = true;
            if (config != null)
            {
                pileRadius = config.enemyPileRadius;
                pileStartCount = config.enemyPileStartCount;
                pileEnemiesPerLayer = config.enemyPileEnemiesPerLayer;
                maxStackLayers = config.enemyPileMaxLayers;
                layerHeightMultiplier = config.enemyPileLayerHeightMultiplier;
                supportClimbSpeedMultiplier = config.enemySupportClimbSpeedMultiplier;
                supportClimbSeparationMultiplier = config.enemySupportClimbSeparationMultiplier;
            }

            EnsureWorkflowDefaults();
        }

        public bool EnsureWorkflowDefaults()
        {
            bool changed = false;
            changed |= EnsureMinimum(ref pileRadius, 2.4f, 0.1f);
            changed |= EnsureMinimum(ref pileStartCount, 6, 1);
            changed |= EnsureMinimum(ref pileEnemiesPerLayer, 6, 1);
            changed |= EnsureMinimum(ref maxStackLayers, 4, 0);
            changed |= EnsureMinimum(ref layerHeightMultiplier, 1f, 0f);
            changed |= EnsureMinimum(ref supportClimbSpeedMultiplier, 0.5f, 0.05f);
            changed |= EnsureRange(ref supportClimbSeparationMultiplier, 0.15f, 0f, 1f);
            if (policy == EnemyStackPolicy.GroundOnly && canClimbEnemies)
            {
                canClimbEnemies = false;
                changed = true;
            }

            return changed;
        }

        public EnemyStackProfile Copy()
        {
            return new EnemyStackProfile
            {
                policy = policy,
                canClimbEnemies = canClimbEnemies,
                canBeStackedOn = canBeStackedOn,
                pileRadius = pileRadius,
                pileStartCount = pileStartCount,
                pileEnemiesPerLayer = pileEnemiesPerLayer,
                maxStackLayers = maxStackLayers,
                layerHeightMultiplier = layerHeightMultiplier,
                supportClimbSpeedMultiplier = supportClimbSpeedMultiplier,
                supportClimbSeparationMultiplier = supportClimbSeparationMultiplier
            };
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

        private static bool EnsureMinimum(ref int value, int defaultValue, int minimum)
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

            value = defaultValue;
            return true;
        }
    }

    [CreateAssetMenu(menuName = "The Circussy One/Content/Enemy Definition", fileName = "EnemyDefinition")]
    public sealed class EnemyDefinition : SerializedScriptableObject, IActivatableContentDefinition
    {
        private const string Tabs = "Enemy";
        private const int AvailabilityDefaultsVersion = 1;

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Core"), LabelWidth(190), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.CombatColor"), PropertyOrder(0)]
        [ValidateInput(nameof(HasEnemyId), "Enemy id is required."), ReadOnly]
        [Tooltip("Stable runtime ID. Use Regenerate ID only when intentionally migrating references.")]
        public string enemyId = "normal_enemy";

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Core"), LabelWidth(190), PropertyOrder(1)]
        [ValidateInput(nameof(HasDisplayName), "Display name is required.")]
        public string displayName = "Normal Enemy";

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Classification"), LabelWidth(190)]
        public ContentRarity rarity = ContentRarity.Common;

        [HideInInspector]
        public ContentTagSet tags = ContentTagSet.With(ContentTag.Swarm, ContentTag.Physical);

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Classification"), InfoBox("Computed from mechanics and effects. Change source fields instead.", InfoMessageType.Info), ShowInInspector, ReadOnly, LabelText("Computed Tags"), PropertyOrder(10)]
        private string ComputedTags => ComputedContentTagRules.Format(Tags);

        [HideInInspector]
        public bool isActive = true;

        [TabGroup(Tabs, "Spawning"), BoxGroup(Tabs + "/Spawning/Selection"), LabelWidth(190), EnumToggleButtons]
        public EnemyBehaviorType behaviorType = EnemyBehaviorType.Chaser;

        [TabGroup(Tabs, "Spawning"), BoxGroup(Tabs + "/Spawning/Selection"), LabelWidth(190)]
        [Min(0f)] public float spawnWeight = 100f;

        [TabGroup(Tabs, "Stats"), BoxGroup(Tabs + "/Stats/Combat"), LabelWidth(190)]
        [Min(1)] public int baseHealth = 40;

        [TabGroup(Tabs, "Stats"), BoxGroup(Tabs + "/Stats/Combat"), LabelWidth(190), SuffixLabel("/min")]
        [Min(0f)] public float healthGrowthPerMinute = 20f;

        [TabGroup(Tabs, "Stats"), BoxGroup(Tabs + "/Stats/Movement"), LabelWidth(190), SuffixLabel("u/s")]
        [Min(0.1f)] public float baseMoveSpeed = 2f;

        [TabGroup(Tabs, "Stats"), BoxGroup(Tabs + "/Stats/Movement"), LabelWidth(190), SuffixLabel("/min")]
        [Min(0f)] public float moveSpeedGrowthPerMinute = 0.24f;

        [TabGroup(Tabs, "Stats"), BoxGroup(Tabs + "/Stats/Movement"), LabelWidth(190), SuffixLabel("deg/sec")]
        [Min(1f)] public float turnDegreesPerSecond = 540f;

        [TabGroup(Tabs, "Stats"), BoxGroup(Tabs + "/Stats/Combat"), LabelWidth(190)]
        [Min(1)] public int contactDamage = 2;

        [TabGroup(Tabs, "Drops"), BoxGroup(Tabs + "/Drops/XP"), LabelWidth(190)]
        [Min(0)] public int xpBudget = 7;

        [TabGroup(Tabs, "Drops"), BoxGroup(Tabs + "/Drops/XP"), LabelWidth(190)]
        [EnumToggleButtons] public XpDropStyle dropStyle = XpDropStyle.Compact;

        [TabGroup(Tabs, "Body"), HideLabel, InlineProperty]
        public EnemyBodyProfile body = new();

        [TabGroup(Tabs, "Locomotion"), HideLabel, InlineProperty]
        public EnemyLocomotionProfile locomotion = new();

        [TabGroup(Tabs, "Climb"), HideLabel, InlineProperty]
        public EnemyClimbProfile climb = new();

        [TabGroup(Tabs, "Stack"), HideLabel, InlineProperty]
        public EnemyStackProfile stack = new();

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Runtime Model"), LabelWidth(190), AssetSelector]
        public GameObject worldPrefab;

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Runtime Model"), HideLabel, InlineProperty]
        public ActorModelTransformProfile modelTransform = new();

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Animations"), HideLabel, InlineProperty]
        public EnemyAnimationProfile animation = new();

        [TabGroup(Tabs, "Diagnostics"), ShowInInspector, ReadOnly, LabelText("Runtime Summary"), PropertyOrder(100)]
        private string RuntimeSummary => $"{displayName} / {behaviorType} / weight {spawnWeight:0.##} / HP {baseHealth} +{healthGrowthPerMinute:0.##}/min / {baseMoveSpeed:0.##} u/s / {stack.policy}";

        public string Id => enemyId;
        public string DisplayName => displayName;
        public ContentTagSet Tags => ComputedContentTagRules.For(this);
        public bool IsActive => isActive;

        [SerializeField, HideInInspector] private int availabilityDefaultsVersion;

        public void ApplyDefaultsFromGameConfig(GameConfig config, XpGemCatalog xpCatalog = null)
        {
            enemyId = "normal_enemy";
            displayName = "Normal Enemy";
            rarity = ContentRarity.Common;
            tags = ContentTagSet.With(ContentTag.Swarm, ContentTag.Physical);
            isActive = true;
            availabilityDefaultsVersion = AvailabilityDefaultsVersion;
            behaviorType = EnemyBehaviorType.Chaser;
            spawnWeight = 100f;
            if (config != null)
            {
                baseHealth = config.enemyHealth;
                healthGrowthPerMinute = config.enemyHealthGrowthPerMinute;
                baseMoveSpeed = config.enemyMoveSpeed;
                moveSpeedGrowthPerMinute = config.enemyMoveSpeedGrowthPerMinute;
                turnDegreesPerSecond = config.enemyTurnDegreesPerSecond;
                contactDamage = config.enemyContactDamage;
                xpBudget = xpCatalog != null
                    ? Mathf.Max(0, xpCatalog.defaultEnemyXpBudget)
                    : XpGemDefinition.DefaultBlueXpAmount;
            }

            dropStyle = xpCatalog != null ? xpCatalog.defaultDropStyle : XpDropStyle.Compact;
            body ??= new EnemyBodyProfile();
            locomotion ??= new EnemyLocomotionProfile();
            climb ??= new EnemyClimbProfile();
            stack ??= new EnemyStackProfile();
            modelTransform ??= new ActorModelTransformProfile();
            animation ??= new EnemyAnimationProfile();
            body.ApplyDefaultsFromGameConfig(config);
            locomotion.mode = EnemyLocomotionMode.Grounded;
            locomotion.EnsureWorkflowDefaults();
            climb.ApplyDefaultsFromGameConfig(config);
            stack.ApplyDefaultsFromGameConfig(config);
            modelTransform.EnsureWorkflowDefaults();
            animation.EnsureWorkflowDefaults();
            EnsureWorkflowDefaults();
        }

        public bool EnsureWorkflowDefaults()
        {
            bool changed = false;
            changed |= EnsureString(ref enemyId, "normal_enemy");
            changed |= EnsureString(ref displayName, "Normal Enemy");
            changed |= EnsureAvailabilityDefaults();
            if (!Enum.IsDefined(typeof(EnemyBehaviorType), behaviorType))
            {
                behaviorType = EnemyBehaviorType.Chaser;
                changed = true;
            }

            changed |= EnsureMinimum(ref baseHealth, 40, 1);
            changed |= EnsureMinimum(ref spawnWeight, 100f, 0f);
            changed |= EnsureMinimum(ref healthGrowthPerMinute, 20f, 0f);
            changed |= EnsureMinimum(ref baseMoveSpeed, 2f, 0.1f);
            changed |= EnsureMinimum(ref moveSpeedGrowthPerMinute, 0.24f, 0f);
            changed |= EnsureMinimum(ref turnDegreesPerSecond, 540f, 1f);
            changed |= EnsureMinimum(ref contactDamage, 2, 1);
            changed |= EnsureMinimum(ref xpBudget, XpGemDefinition.DefaultBlueXpAmount, 0);
            body ??= new EnemyBodyProfile();
            locomotion ??= new EnemyLocomotionProfile();
            climb ??= new EnemyClimbProfile();
            stack ??= new EnemyStackProfile();
            modelTransform ??= new ActorModelTransformProfile();
            animation ??= new EnemyAnimationProfile();
            changed |= body.EnsureWorkflowDefaults();
            changed |= locomotion.EnsureWorkflowDefaults();
            changed |= climb.EnsureWorkflowDefaults();
            changed |= stack.EnsureWorkflowDefaults();
            changed |= modelTransform.EnsureWorkflowDefaults();
            changed |= animation.EnsureWorkflowDefaults();
            return changed;
        }

        private bool EnsureAvailabilityDefaults()
        {
            if (availabilityDefaultsVersion >= AvailabilityDefaultsVersion)
            {
                return false;
            }

            isActive = true;
            availabilityDefaultsVersion = AvailabilityDefaultsVersion;
            return true;
        }

        public static EnemyDefinition CreateRuntimeFallback(GameConfig config, XpGemCatalog xpCatalog = null)
        {
            var definition = CreateInstance<EnemyDefinition>();
            definition.hideFlags = HideFlags.DontSave;
            definition.ApplyDefaultsFromGameConfig(config, xpCatalog);
            return definition;
        }

        private void OnValidate()
        {
            EnsureWorkflowDefaults();
        }

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Core"), Button("Copy ID"), PropertyOrder(2)]
        private void CopyId()
        {
            GUIUtility.systemCopyBuffer = Id;
        }

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Core"), Button("Regenerate ID From Display Name"), PropertyOrder(3)]
        private void RegenerateIdFromDisplayName()
        {
            enemyId = ContentIdSuggestionRules.NormalizeBase(displayName, "enemy");
            EnsureWorkflowDefaults();
        }

        private bool HasEnemyId(string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        private bool HasDisplayName(string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        private static bool EnsureString(ref string value, string defaultValue)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            value = defaultValue;
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
