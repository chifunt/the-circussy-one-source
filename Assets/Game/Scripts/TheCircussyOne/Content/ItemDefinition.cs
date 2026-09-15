using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TheCircussyOne.Rules;
using TheCircussyOne.Stats;
using UnityEngine;

namespace TheCircussyOne.Content
{
    public enum ItemStackPolicy
    {
        StackLinear = 0,
        Unique = 1,
        StackWithCap = 2,
        StackDiminishing = 3,
        InvalidDuplicate = 4
    }

    [Serializable]
    public struct ItemStatModifierDefinition
    {
        public StatId statId;
        public StatModifierBucket bucket;
        public float value;

        [ShowInInspector, ReadOnly, LabelText("Summary"), PropertyOrder(-20)]
        public string Summary => ContentModifierDisplayRules.Summary(this);

        public ItemStatModifierDefinition(StatId statId, StatModifierBucket bucket, float value)
        {
            this.statId = statId;
            this.bucket = bucket;
            this.value = value;
        }

        public StatModifier ToRuntime(string sourceId)
        {
            return ToRuntime(sourceId, 1f);
        }

        public StatModifier ToRuntime(string sourceId, float multiplier)
        {
            return new StatModifier(statId, bucket, value * multiplier, sourceId);
        }
    }

    [CreateAssetMenu(menuName = "The Circussy One/Content/Item Definition", fileName = "ItemDefinition")]
    public sealed class ItemDefinition : SerializedScriptableObject, IActivatableContentDefinition
    {
        private const string Tabs = "Item";
        private const int AvailabilityDefaultsVersion = 1;

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Core"), LabelWidth(180), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.BalanceColor"), PropertyOrder(0)]
        [ValidateInput(nameof(HasItemId), "Item id is required."), ReadOnly]
        [Tooltip("Stable runtime ID. Use Regenerate ID only when intentionally migrating references.")]
        public string itemId = "rubber_soles";

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Core"), LabelWidth(180), PropertyOrder(1)]
        [ValidateInput(nameof(HasDisplayName), "Display name is required.")]
        public string displayName = "Rubber Soles";

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Core"), LabelWidth(180), MultiLineProperty(2), PropertyOrder(4)]
        public string shortDescription = "Move faster.";

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Classification"), LabelWidth(180)]
        [GUIColor(nameof(RarityAuthoringColor))]
        public ContentRarity rarity = ContentRarity.Common;

        [HideInInspector]
        public ContentTagSet tags = ContentTagSet.With(ContentTag.Movement, ContentTag.Utility);

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Classification"), InfoBox("Computed from mechanics and effects. Change source fields instead.", InfoMessageType.Info), ShowInInspector, ReadOnly, LabelText("Computed Tags"), PropertyOrder(10)]
        private string ComputedTags => ComputedContentTagRules.Format(Tags);

        [HideInInspector]
        public bool isActive = true;

        [TabGroup(Tabs, "Stacking"), BoxGroup(Tabs + "/Stacking/Rules"), LabelWidth(180), EnumToggleButtons]
        public ItemStackPolicy stackPolicy = ItemStackPolicy.StackLinear;

        [TabGroup(Tabs, "Stacking"), BoxGroup(Tabs + "/Stacking/Rules"), LabelWidth(180), ShowIf(nameof(UsesMaxStacks))]
        [Min(1)] public int maxStacks = 5;

        [TabGroup(Tabs, "Stacking"), BoxGroup(Tabs + "/Stacking/Rules"), LabelWidth(180), ShowIf(nameof(UsesEffectCapStacks))]
        [Min(1)] public int effectCapStacks = 5;

        [TabGroup(Tabs, "Stacking"), BoxGroup(Tabs + "/Stacking/Rules"), LabelWidth(180), ShowIf(nameof(UsesDiminishingFalloff))]
        [Min(0f)] public float diminishingFalloff = ItemStackRules.DefaultDiminishingFalloff;

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Card"), LabelWidth(180), AssetSelector, PreviewField(72)]
        [Tooltip("Square transparent icon used by item cards and the HUD item stack column.")]
        public Sprite iconSprite;

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Card"), LabelWidth(180)]
        [LabelText("Fallback Color")]
        public Color iconColor = new(0.9f, 0.95f, 0.82f, 1f);

        [TabGroup(Tabs, "Effects"), BoxGroup(Tabs + "/Effects/Upside Stat Modifiers"), LabelWidth(180)]
        public List<ItemStatModifierDefinition> statModifiers = new();

        [TabGroup(Tabs, "Effects"), BoxGroup(Tabs + "/Effects/Downside Stat Modifiers"), LabelWidth(180)]
        public List<ItemStatModifierDefinition> downsideStatModifiers = new();

        [TabGroup(Tabs, "Diagnostics"), ShowInInspector, ReadOnly, LabelText("Runtime Summary"), PropertyOrder(100)]
        private string RuntimeSummary => $"{displayName} [{rarity}] {StackSummary}: {shortDescription}";

        public string Id => itemId;
        public string DisplayName => displayName;
        public ContentTagSet Tags => ComputedContentTagRules.For(this);
        public bool IsActive => isActive;
        public int EffectiveMaxStacks => ItemStackRules.MaxStacks(this);
        public int EffectiveModifierStacks => ItemStackRules.EffectiveModifierStackCount(this);
        public bool HasDownside => downsideStatModifiers != null && downsideStatModifiers.Count > 0;
        public string StackSummary => ItemStackRules.StackSummary(this);
        private bool UsesMaxStacks => ItemStackRules.UsesMaxStacks(stackPolicy);
        private bool UsesEffectCapStacks => ItemStackRules.UsesEffectCap(stackPolicy);
        private bool UsesDiminishingFalloff => ItemStackRules.UsesDiminishingFalloff(stackPolicy);
        private Color RarityAuthoringColor => ContentRarityMetadata.AuthoringColor(rarity);

        [SerializeField, HideInInspector] private int availabilityDefaultsVersion;

        public void ApplyDefaults(
            string id,
            string name,
            string description,
            ContentRarity rarity,
            ContentTagSet tags,
            ItemStackPolicy stackPolicy,
            int maxStacks,
            Color iconColor,
            params ItemStatModifierDefinition[] modifiers)
        {
            itemId = id;
            displayName = name;
            shortDescription = description;
            this.rarity = rarity;
            this.tags = tags ?? ContentTagSet.With(ContentTag.Utility);
            isActive = true;
            availabilityDefaultsVersion = AvailabilityDefaultsVersion;
            this.stackPolicy = stackPolicy;
            this.maxStacks = Mathf.Max(1, maxStacks);
            effectCapStacks = this.maxStacks;
            diminishingFalloff = ItemStackRules.DefaultDiminishingFalloff;
            this.iconColor = iconColor;
            statModifiers = new List<ItemStatModifierDefinition>();
            if (modifiers != null)
            {
                statModifiers.AddRange(modifiers);
            }

            downsideStatModifiers = new List<ItemStatModifierDefinition>();
            EnsureWorkflowDefaults();
        }

        public bool EnsureWorkflowDefaults()
        {
            bool changed = false;
            changed |= EnsureString(ref itemId, "rubber_soles");
            changed |= EnsureString(ref displayName, "Rubber Soles");
            changed |= EnsureString(ref shortDescription, "Improve a passive stat.");
            changed |= EnsureAvailabilityDefaults();
            if (maxStacks < 1)
            {
                maxStacks = 1;
                changed = true;
            }

            if (effectCapStacks < 1)
            {
                effectCapStacks = Mathf.Max(1, maxStacks);
                changed = true;
            }

            if (diminishingFalloff < 0f)
            {
                diminishingFalloff = ItemStackRules.DefaultDiminishingFalloff;
                changed = true;
            }

            statModifiers ??= new List<ItemStatModifierDefinition>();
            downsideStatModifiers ??= new List<ItemStatModifierDefinition>();
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
            itemId = ContentIdSuggestionRules.NormalizeBase(displayName, "item");
            EnsureWorkflowDefaults();
        }

        private bool HasItemId(string value)
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
    }
}
