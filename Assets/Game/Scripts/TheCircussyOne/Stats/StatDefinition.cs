namespace TheCircussyOne.Stats
{
    public readonly struct StatDefinition
    {
        public StatDefinition(
            StatId id,
            string displayName,
            string category,
            StatValueKind valueKind,
            StatStackingPolicy stackingPolicy,
            float minValue = float.NegativeInfinity,
            float maxValue = float.PositiveInfinity)
        {
            Id = id;
            DisplayName = displayName;
            Category = category;
            ValueKind = valueKind;
            StackingPolicy = stackingPolicy;
            MinValue = minValue;
            MaxValue = maxValue;
        }

        public StatId Id { get; }
        public string DisplayName { get; }
        public string Category { get; }
        public StatValueKind ValueKind { get; }
        public StatStackingPolicy StackingPolicy { get; }
        public float MinValue { get; }
        public float MaxValue { get; }
    }
}
