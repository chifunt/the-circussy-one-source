using Sirenix.OdinInspector;
using UnityEngine;

namespace TheCircussyOne.Content
{
    [CreateAssetMenu(menuName = "The Circussy One/Content/XP Gem Definition", fileName = "XpGemDefinition")]
    public sealed class XpGemDefinition : SerializedScriptableObject, IContentDefinition
    {
        private const string Tabs = "XP Gem";
        private const int CurrentGlowDefaultsVersion = 1;
        public const int DefaultBlueXpAmount = 7;
        public const float DefaultEmissionStrength = 1.6f;

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Core"), LabelWidth(160), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.BalanceColor"), PropertyOrder(0)]
        [ValidateInput(nameof(HasGemId), "XP gem id is required."), ReadOnly]
        [Tooltip("Stable runtime ID. Use Regenerate ID only when intentionally migrating references.")]
        public string gemId = "blue_xp_gem";

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Core"), LabelWidth(160), PropertyOrder(1)]
        [ValidateInput(nameof(HasDisplayName), "Display name is required.")]
        public string displayName = "Blue XP Gem";

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Classification"), LabelWidth(160)]
        public ContentRarity rarity = ContentRarity.Common;

        [HideInInspector]
        public ContentTagSet tags = ContentTagSet.With(ContentTag.Economy, ContentTag.Pickup, ContentTag.XP);

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Classification"), InfoBox("Computed from mechanics and effects. Change source fields instead.", InfoMessageType.Info), ShowInInspector, ReadOnly, LabelText("Computed Tags"), PropertyOrder(10)]
        private string ComputedTags => ComputedContentTagRules.Format(Tags);

        [TabGroup(Tabs, "Reward"), BoxGroup(Tabs + "/Reward/XP"), LabelWidth(160)]
        [Min(1)] public int xpAmount = DefaultBlueXpAmount;

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Style"), LabelWidth(160)]
        public Color color = new(0.27f, 0.96f, 1f, 1f);

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Style"), LabelWidth(160), SuffixLabel("x")]
        [Min(0.05f)] public float visualScale = 1f;

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Glow"), LabelWidth(160)]
        public Color emissionColor = new(0.27f, 0.96f, 1f, 1f);

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Glow"), LabelWidth(160), SuffixLabel("x")]
        [Min(0f)] public float emissionStrength = DefaultEmissionStrength;

        [SerializeField, HideInInspector] private int glowDefaultsVersion = CurrentGlowDefaultsVersion;

        [TabGroup(Tabs, "Diagnostics"), ShowInInspector, ReadOnly, LabelText("Runtime Summary"), PropertyOrder(100)]
        private string RuntimeSummary => $"{displayName} / +{xpAmount} XP / {rarity} / Glow {Mathf.Max(0f, emissionStrength):0.##}x";

        public string Id => gemId;
        public string DisplayName => displayName;
        public ContentTagSet Tags => ComputedContentTagRules.For(this);

        public void ApplyDefaults(string id, string name, int amount, Color tint, ContentRarity rarityValue, float scale)
        {
            gemId = id;
            displayName = name;
            xpAmount = amount;
            color = tint;
            rarity = rarityValue;
            visualScale = scale;
            emissionColor = tint;
            emissionStrength = DefaultEmissionStrength;
            glowDefaultsVersion = CurrentGlowDefaultsVersion;
            tags = ContentTagSet.With(ContentTag.Economy, ContentTag.Pickup, ContentTag.XP);
            EnsureWorkflowDefaults();
        }

        public bool EnsureWorkflowDefaults()
        {
            bool changed = false;
            changed |= EnsureString(ref gemId, "blue_xp_gem");
            changed |= EnsureString(ref displayName, "Blue XP Gem");
            changed |= EnsureMinimum(ref xpAmount, DefaultBlueXpAmount, 1);
            changed |= EnsureMinimum(ref visualScale, 1f, 0.05f);
            changed |= EnsureGlowDefaults();
            return changed;
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
            gemId = ContentIdSuggestionRules.NormalizeBase(displayName, "xp_gem");
            EnsureWorkflowDefaults();
        }

        private bool HasGemId(string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        private bool HasDisplayName(string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        private bool EnsureGlowDefaults()
        {
            if (glowDefaultsVersion >= CurrentGlowDefaultsVersion)
            {
                return EnsureMinimum(ref emissionStrength, 0f, 0f);
            }

            emissionColor = color;
            emissionStrength = DefaultEmissionStrength;
            glowDefaultsVersion = CurrentGlowDefaultsVersion;
            return true;
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
