using Sirenix.OdinInspector;
using TheCircussyOne.Rules;
using UnityEngine;
using UnityEngine.Serialization;

namespace TheCircussyOne.Content
{
    [CreateAssetMenu(menuName = "The Circussy One/Content/Healing Prop Definition", fileName = "HealingPropDefinition")]
    public sealed class HealingPropDefinition : SerializedScriptableObject, IActivatableContentDefinition
    {
        private const string Tabs = "Healing Prop";
        private const int VisualFeedbackDefaultsVersion = 1;

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Core"), LabelWidth(180), ReadOnly]
        [ValidateInput(nameof(HasPropId), "Healing prop id is required.")]
        public string propId = "snack_box";

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Core"), LabelWidth(180)]
        [ValidateInput(nameof(HasDisplayName), "Display name is required.")]
        public string displayName = "Snack Box";

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Core"), LabelWidth(180), MultiLineProperty(2)]
        public string shortDescription = "A quick little snack box for patching up between swarms.";

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Availability"), LabelWidth(180)]
        public bool isActive = true;

        [HideInInspector]
        public ContentTagSet tags = ContentTagSet.With(ContentTag.Economy, ContentTag.Health);

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Classification"), InfoBox("Computed from prop role and reward. Change source fields instead.", InfoMessageType.Info), ShowInInspector, ReadOnly, LabelText("Computed Tags")]
        private string ComputedTags => ComputedContentTagRules.Format(Tags);

        [TabGroup(Tabs, "Interaction"), BoxGroup(Tabs + "/Interaction/Hold"), LabelWidth(180), Min(0.05f), SuffixLabel("sec", true)]
        public float holdSeconds = 1f;

        [TabGroup(Tabs, "Interaction"), BoxGroup(Tabs + "/Interaction/Prompt"), LabelWidth(180)]
        public string promptText = "Grab a Snack";

        [TabGroup(Tabs, "Placement"), BoxGroup(Tabs + "/Placement/Collision"), LabelWidth(180), InlineProperty]
        public WorldPropPlacementProfile placement = WorldPropPlacementProfile.HealingPropDefault();

        [TabGroup(Tabs, "Reward"), BoxGroup(Tabs + "/Reward/Pickup"), LabelWidth(180), AssetSelector]
        public HealthPickupDefinition healthPickup;

        [TabGroup(Tabs, "Reward"), BoxGroup(Tabs + "/Reward/Pickup"), LabelWidth(180), Min(1)]
        [FormerlySerializedAs("pickupCount")]
        public int minPickupCount = 1;

        [TabGroup(Tabs, "Reward"), BoxGroup(Tabs + "/Reward/Pickup"), LabelWidth(180), Min(1)]
        public int maxPickupCount = 1;

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Placeholder"), LabelWidth(180)]
        public Color visualColor = new(1f, 0.24f, 0.42f, 1f);

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Placeholder"), LabelWidth(180), AssetSelector]
        public Material visualMaterial;

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Placeholder"), LabelWidth(180), Min(0.1f)]
        public float visualScale = 0.85f;

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Glow"), LabelWidth(180)]
        public Color emissionColor = new(1f, 0.18f, 0.35f, 1f);

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Glow"), LabelWidth(180), SuffixLabel("x", true), Min(0f)]
        public float emissionStrength = 0.9f;

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Target Outline"), LabelWidth(180)]
        public Color targetOutlineColor = new(1f, 0.72f, 0.82f, 1f);

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Target Outline"), LabelWidth(180), LabelText("Target Outline Width"), SuffixLabel("px", true), Min(0f)]
        public float targetOutlineThickness = 3.25f;

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Interaction Squash"), LabelWidth(180), SuffixLabel("x", true), Min(0f)]
        [InfoBox("Plays while the player is actively holding interact on this prop. Set amplitude to 0 to disable.")]
        public float interactionSquashStretchAmplitude = 0.09f;

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Interaction Squash"), LabelWidth(180), SuffixLabel("Hz", true), Min(0f)]
        public float interactionSquashStretchFrequency = 4.5f;

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Open Disappear"), LabelWidth(180), SuffixLabel("sec", true), Min(0.01f)]
        public float openDisappearSeconds = 0.58f;

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Open Disappear"), LabelWidth(180), Range(0f, 1f)]
        public float openShrinkFinalScaleMultiplier = 0.02f;

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Open Disappear"), LabelWidth(180), InlineProperty]
        public EaseSettings openDisappearEase = EaseSettings.InBack;

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Snack Burst"), LabelWidth(180)]
        public bool snackBurstEnabled = true;

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Snack Burst"), LabelWidth(180), Min(0)]
        public int snackBurstCount = 10;

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Snack Burst"), LabelWidth(180)]
        public Color snackBurstColor = new(1f, 0.68f, 0.46f, 1f);

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Snack Burst"), LabelWidth(180), SuffixLabel("sec", true), Min(0.01f)]
        public float snackBurstLifetimeSeconds = 0.9f;

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Snack Burst"), LabelWidth(180), SuffixLabel("u/sec", true), Min(0f)]
        public float snackBurstSpeed = 1.35f;

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Snack Burst"), LabelWidth(180), Min(0f)]
        public float snackBurstGravity = 0.1f;

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Snack Burst"), LabelWidth(180), Min(0f)]
        public float snackBurstSwayStrength = 0.28f;

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Snack Burst"), LabelWidth(180), Min(0.01f)]
        public float snackBurstStartSize = 0.16f;

        [SerializeField, HideInInspector] private int visualFeedbackDefaultsVersion;

        [TabGroup(Tabs, "Diagnostics"), ShowInInspector, ReadOnly, LabelText("Runtime Summary"), PropertyOrder(100)]
        private string RuntimeSummary => $"{displayName} / {holdSeconds:0.##} sec / {PickupCountSummary} {(healthPickup != null ? healthPickup.DisplayName : "Missing Pickup")}";

        public string Id => propId;
        public string DisplayName => displayName;
        public ContentTagSet Tags => ComputedContentTagRules.For(this);
        public bool IsActive => isActive;

        public void ApplyDefaults(
            string id,
            string name,
            string description,
            float holdSeconds,
            string promptText,
            HealthPickupDefinition pickup,
            int pickupCount,
            Color visualColor,
            float visualScale)
        {
            ApplyDefaults(id, name, description, holdSeconds, promptText, pickup, pickupCount, pickupCount, visualColor, visualScale);
        }

        public void ApplyDefaults(
            string id,
            string name,
            string description,
            float holdSeconds,
            string promptText,
            HealthPickupDefinition pickup,
            int minPickupCount,
            int maxPickupCount,
            Color visualColor,
            float visualScale)
        {
            propId = id;
            displayName = name;
            shortDescription = description;
            this.holdSeconds = holdSeconds;
            this.promptText = promptText;
            healthPickup = pickup;
            this.minPickupCount = minPickupCount;
            this.maxPickupCount = maxPickupCount;
            this.visualColor = visualColor;
            this.visualScale = visualScale;
            placement = WorldPropPlacementProfile.HealingPropDefault();
            emissionColor = Color.Lerp(visualColor, Color.white, 0.08f);
            emissionStrength = 0.9f;
            targetOutlineColor = Color.Lerp(visualColor, Color.white, 0.55f);
            targetOutlineThickness = 3.25f;
            interactionSquashStretchAmplitude = 0.09f;
            interactionSquashStretchFrequency = 4.5f;
            ApplyVisualFeedbackDefaults();
            tags = ContentTagSet.With(ContentTag.Economy, ContentTag.Health);
            isActive = true;
            EnsureWorkflowDefaults();
        }

        public bool EnsureWorkflowDefaults()
        {
            bool changed = false;
            changed |= EnsureString(ref propId, "snack_box");
            changed |= EnsureString(ref displayName, "Snack Box");
            changed |= EnsureString(ref shortDescription, "A snack box.");
            changed |= EnsureString(ref promptText, "Grab a Snack");
            changed |= EnsureMinimum(ref holdSeconds, 1f, 0.05f);
            changed |= EnsureMinimum(ref minPickupCount, 1, 1);
            changed |= EnsureMinimum(ref maxPickupCount, minPickupCount, 1);
            if (maxPickupCount < minPickupCount)
            {
                maxPickupCount = minPickupCount;
                changed = true;
            }

            changed |= EnsureMinimum(ref visualScale, 0.85f, 0.1f);
            changed |= EnsureMinimum(ref emissionStrength, 0f, 0f);
            changed |= EnsureMinimum(ref targetOutlineThickness, 0f, 0f);
            changed |= EnsureMinimum(ref interactionSquashStretchAmplitude, 0f, 0f);
            changed |= EnsureMinimum(ref interactionSquashStretchFrequency, 0f, 0f);
            changed |= EnsureVisualFeedbackDefaults();
            changed |= placement.EnsureDefaults(WorldPropPlacementProfile.HealingPropDefault());
            return changed;
        }

        private void OnValidate()
        {
            EnsureWorkflowDefaults();
        }

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Core"), Button("Copy ID"), PropertyOrder(20)]
        private void CopyId()
        {
            GUIUtility.systemCopyBuffer = Id;
        }

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Core"), Button("Regenerate ID From Display Name"), PropertyOrder(21)]
        private void RegenerateIdFromDisplayName()
        {
            propId = ContentIdSuggestionRules.NormalizeBase(displayName, "healing_prop");
            EnsureWorkflowDefaults();
        }

        private bool HasPropId(string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        private bool HasDisplayName(string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        private string PickupCountSummary => minPickupCount == maxPickupCount
            ? $"{Mathf.Max(1, minPickupCount)}x"
            : $"{Mathf.Max(1, minPickupCount)}-{Mathf.Max(Mathf.Max(1, minPickupCount), maxPickupCount)}x";

        private void ApplyVisualFeedbackDefaults()
        {
            openDisappearSeconds = 0.58f;
            openShrinkFinalScaleMultiplier = 0.02f;
            openDisappearEase = EaseSettings.InBack;
            snackBurstEnabled = true;
            snackBurstCount = 10;
            snackBurstColor = Color.Lerp(visualColor, Color.white, 0.12f);
            snackBurstLifetimeSeconds = 0.9f;
            snackBurstSpeed = 1.35f;
            snackBurstGravity = 0.1f;
            snackBurstSwayStrength = 0.28f;
            snackBurstStartSize = 0.16f;
            visualFeedbackDefaultsVersion = VisualFeedbackDefaultsVersion;
        }

        private bool EnsureVisualFeedbackDefaults()
        {
            if (visualFeedbackDefaultsVersion < VisualFeedbackDefaultsVersion)
            {
                ApplyVisualFeedbackDefaults();
                return true;
            }

            bool changed = false;
            changed |= EnsureMinimum(ref openDisappearSeconds, 0.58f, 0.01f);
            changed |= EnsureRange(ref openShrinkFinalScaleMultiplier, 0.02f, 0f, 1f);
            changed |= EnsureEaseDefault(ref openDisappearEase, EaseSettings.InBack);
            changed |= EnsureMinimum(ref snackBurstCount, 10, 0);
            changed |= EnsureMinimum(ref snackBurstLifetimeSeconds, 0.9f, 0.01f);
            changed |= EnsureMinimum(ref snackBurstSpeed, 1.35f, 0f);
            changed |= EnsureMinimum(ref snackBurstGravity, 0.1f, 0f);
            changed |= EnsureMinimum(ref snackBurstSwayStrength, 0.28f, 0f);
            changed |= EnsureMinimum(ref snackBurstStartSize, 0.16f, 0.01f);
            if (snackBurstColor.a <= 0f)
            {
                snackBurstColor = Color.Lerp(visualColor, Color.white, 0.12f);
                changed = true;
            }

            return changed;
        }

        private static bool EnsureString(ref string value, string fallback)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            value = fallback;
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

            value = defaultValue;
            return true;
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
    }
}
