using System.Collections.Generic;

namespace TheCircussyOne.Stats
{
    public readonly struct StatModifierSnapshot
    {
        public StatModifierSnapshot(StatId statId, StatModifierBucket bucket, float value, string sourceId)
        {
            StatId = statId;
            Bucket = bucket;
            Value = value;
            SourceId = sourceId;
        }

        public StatId StatId { get; }
        public StatModifierBucket Bucket { get; }
        public float Value { get; }
        public string SourceId { get; }
    }

    public readonly struct StatBreakdownLine
    {
        public StatBreakdownLine(string label, float value, string detail)
        {
            Label = label;
            Value = value;
            Detail = detail;
        }

        public string Label { get; }
        public float Value { get; }
        public string Detail { get; }
    }

    public readonly struct StatBreakdown
    {
        public StatBreakdown(
            StatDefinition definition,
            float baseValue,
            float finalValue,
            IReadOnlyList<StatModifierSnapshot> modifiers,
            IReadOnlyList<StatBreakdownLine> lines)
        {
            Definition = definition;
            Id = definition.Id;
            DisplayName = definition.DisplayName;
            Category = definition.Category;
            ValueKind = definition.ValueKind;
            StackingPolicy = definition.StackingPolicy;
            MinValue = definition.MinValue;
            MaxValue = definition.MaxValue;
            BaseValue = baseValue;
            FinalValue = finalValue;
            Modifiers = modifiers;
            Lines = lines;
        }

        public StatDefinition Definition { get; }
        public StatId Id { get; }
        public string DisplayName { get; }
        public string Category { get; }
        public StatValueKind ValueKind { get; }
        public StatStackingPolicy StackingPolicy { get; }
        public float MinValue { get; }
        public float MaxValue { get; }
        public float BaseValue { get; }
        public float FinalValue { get; }
        public IReadOnlyList<StatModifierSnapshot> Modifiers { get; }
        public IReadOnlyList<StatBreakdownLine> Lines { get; }
    }
}
