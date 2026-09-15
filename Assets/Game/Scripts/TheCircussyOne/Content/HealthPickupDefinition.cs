using Sirenix.OdinInspector;
using UnityEngine;

namespace TheCircussyOne.Content
{
    [CreateAssetMenu(menuName = "The Circussy One/Content/Health Pickup Definition", fileName = "HealthPickupDefinition")]
    public sealed class HealthPickupDefinition : SerializedScriptableObject, IActivatableContentDefinition
    {
        private const string Tabs = "Health Pickup";
        public const int DefaultHealAmount = 20;
        public const float DefaultHealPercentOfMaxHealth = 0.2f;
        public const float DefaultEmissionStrength = 1.8f;

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Core"), LabelWidth(170), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.BalanceColor"), ReadOnly]
        [ValidateInput(nameof(HasPickupId), "Health pickup id is required.")]
        public string pickupId = "treat";

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Core"), LabelWidth(170)]
        [ValidateInput(nameof(HasDisplayName), "Display name is required.")]
        public string displayName = "Treat";

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Core"), LabelWidth(170), MultiLineProperty(2)]
        public string shortDescription = "A circus treat that patches up a rough act.";

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Availability"), LabelWidth(170)]
        public bool isActive = true;

        [HideInInspector]
        public ContentTagSet tags = ContentTagSet.With(ContentTag.Economy, ContentTag.Pickup, ContentTag.Health);

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Classification"), InfoBox("Computed from pickup role and reward. Change source fields instead.", InfoMessageType.Info), ShowInInspector, ReadOnly, LabelText("Computed Tags")]
        private string ComputedTags => ComputedContentTagRules.Format(Tags);

        [TabGroup(Tabs, "Reward"), BoxGroup(Tabs + "/Reward/Healing"), LabelWidth(170)]
        [LabelText("Heal % Max HP"), SuffixLabel("x", true), Range(0.01f, 1f)]
        public float healPercentOfMaxHealth = DefaultHealPercentOfMaxHealth;

        [HideInInspector]
        public int healAmount = DefaultHealAmount;

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Style"), LabelWidth(170), EnumToggleButtons]
        public PickupVisualShape visualShape = PickupVisualShape.Heart;

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Style"), LabelWidth(170)]
        public Color color = new(1f, 0.2f, 0.42f, 1f);

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Style"), LabelWidth(170), SuffixLabel("x", true)]
        [Min(0.05f)] public float visualScale = 1.05f;

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Glow"), LabelWidth(170)]
        public Color emissionColor = new(1f, 0.18f, 0.36f, 1f);

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Glow"), LabelWidth(170), SuffixLabel("x", true)]
        [Min(0f)] public float emissionStrength = DefaultEmissionStrength;

        [TabGroup(Tabs, "Diagnostics"), ShowInInspector, ReadOnly, LabelText("Runtime Summary"), PropertyOrder(100)]
        private string RuntimeSummary => $"{displayName} / {Mathf.Max(0f, healPercentOfMaxHealth) * 100f:0.#}% Max HP / {visualShape} / Glow {Mathf.Max(0f, emissionStrength):0.##}x";

        public string Id => pickupId;
        public string DisplayName => displayName;
        public ContentTagSet Tags => ComputedContentTagRules.For(this);
        public bool IsActive => isActive;

        public void ApplyDefaults(string id, string name, string description, int amount, Color tint, float scale, float healPercent = DefaultHealPercentOfMaxHealth)
        {
            pickupId = id;
            displayName = name;
            shortDescription = description;
            healAmount = amount;
            healPercentOfMaxHealth = healPercent;
            color = tint;
            visualScale = scale;
            visualShape = PickupVisualShape.Heart;
            emissionColor = tint;
            emissionStrength = DefaultEmissionStrength;
            tags = ContentTagSet.With(ContentTag.Economy, ContentTag.Pickup, ContentTag.Health);
            isActive = true;
            EnsureWorkflowDefaults();
        }

        public bool EnsureWorkflowDefaults()
        {
            bool changed = false;
            changed |= EnsureString(ref pickupId, "treat");
            changed |= EnsureString(ref displayName, "Treat");
            changed |= EnsureString(ref shortDescription, "A heart-shaped circus treat.");
            changed |= EnsureMinimum(ref healAmount, DefaultHealAmount, 1);
            changed |= EnsureMinimum(ref healPercentOfMaxHealth, DefaultHealPercentOfMaxHealth, 0.01f);
            changed |= EnsureMinimum(ref visualScale, 1f, 0.05f);
            changed |= EnsureMinimum(ref emissionStrength, 0f, 0f);
            if (!System.Enum.IsDefined(typeof(PickupVisualShape), visualShape))
            {
                visualShape = PickupVisualShape.Heart;
                changed = true;
            }

            return changed;
        }

        public int ResolveHealAmount(int maxHealth)
        {
            int safeMaxHealth = Mathf.Max(1, maxHealth);
            float safePercent = Mathf.Max(0f, healPercentOfMaxHealth);
            return Mathf.Max(1, Mathf.CeilToInt(safeMaxHealth * safePercent));
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
            pickupId = ContentIdSuggestionRules.NormalizeBase(displayName, "health_pickup");
            EnsureWorkflowDefaults();
        }

        private bool HasPickupId(string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        private bool HasDisplayName(string value)
        {
            return !string.IsNullOrWhiteSpace(value);
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
    }
}
