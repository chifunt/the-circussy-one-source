using System;
using Sirenix.OdinInspector;
using TheCircussyOne.Stats;

namespace TheCircussyOne.Content
{
    [Serializable]
    public struct UpgradeStatModifierDefinition
    {
        public StatId statId;
        public StatModifierBucket bucket;
        public float value;
        [LabelText("Use Explicit Rarity Values")]
        public bool useExplicitRarityValues;
        [ShowIf(nameof(useExplicitRarityValues)), LabelText("Uncommon Value")]
        public float uncommonValue;
        [ShowIf(nameof(useExplicitRarityValues)), LabelText("Rare Value")]
        public float rareValue;
        [ShowIf(nameof(useExplicitRarityValues)), LabelText("Epic Value")]
        public float epicValue;
        [ShowIf(nameof(useExplicitRarityValues)), LabelText("Legendary Value")]
        public float legendaryValue;

        [ShowInInspector, ReadOnly, LabelText("Summary"), PropertyOrder(-20)]
        public string Summary => ContentModifierDisplayRules.Summary(this);

        [ShowInInspector, ReadOnly, LabelText("Rarity Preview"), PropertyOrder(-19)]
        public string RarityPreview => ContentModifierDisplayRules.RarityPreview(this);

        public UpgradeStatModifierDefinition(StatId statId, StatModifierBucket bucket, float value)
        {
            this.statId = statId;
            this.bucket = bucket;
            this.value = value;
            useExplicitRarityValues = false;
            uncommonValue = 0f;
            rareValue = 0f;
            epicValue = 0f;
            legendaryValue = 0f;
        }

        public UpgradeStatModifierDefinition(
            StatId statId,
            StatModifierBucket bucket,
            float commonValue,
            float uncommonValue,
            float rareValue,
            float epicValue,
            float legendaryValue)
        {
            this.statId = statId;
            this.bucket = bucket;
            value = commonValue;
            useExplicitRarityValues = true;
            this.uncommonValue = uncommonValue;
            this.rareValue = rareValue;
            this.epicValue = epicValue;
            this.legendaryValue = legendaryValue;
        }

        public float ValueFor(ContentRarity rarity)
        {
            if (!useExplicitRarityValues)
            {
                return ContentRarityScaling.Scale(value, rarity);
            }

            return rarity switch
            {
                ContentRarity.Uncommon => uncommonValue,
                ContentRarity.Rare => rareValue,
                ContentRarity.Epic => epicValue,
                ContentRarity.Legendary => legendaryValue,
                _ => value
            };
        }

        public StatModifier ToRuntime(string sourceId)
        {
            return new StatModifier(statId, bucket, value, sourceId);
        }

        public StatModifier ToRuntime(string sourceId, ContentRarity rarity)
        {
            return new StatModifier(statId, bucket, ValueFor(rarity), sourceId);
        }
    }
}
