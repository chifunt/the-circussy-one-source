using System;

namespace TheCircussyOne.Stats
{
    public readonly struct StatModifier
    {
        public StatModifier(StatId statId, StatModifierBucket bucket, float value, string sourceId)
        {
            StatId = statId;
            Bucket = bucket;
            Value = value;
            SourceId = string.IsNullOrWhiteSpace(sourceId) ? throw new ArgumentException("A stat modifier source id is required.", nameof(sourceId)) : sourceId;
        }

        public StatId StatId { get; }
        public StatModifierBucket Bucket { get; }
        public float Value { get; }
        public string SourceId { get; }

        public static StatModifier Flat(StatId statId, float value, string sourceId)
        {
            return new StatModifier(statId, StatModifierBucket.Flat, value, sourceId);
        }

        public static StatModifier AdditivePercent(StatId statId, float value, string sourceId)
        {
            return new StatModifier(statId, StatModifierBucket.AdditivePercent, value, sourceId);
        }

        public static StatModifier AttackSpeedBonus(StatId statId, float value, string sourceId)
        {
            return new StatModifier(statId, StatModifierBucket.AdditivePercent, value, sourceId);
        }
    }
}
