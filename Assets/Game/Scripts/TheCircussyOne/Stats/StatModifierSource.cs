namespace TheCircussyOne.Stats
{
    public readonly struct StatModifierSource
    {
        public StatModifierSource(string id, string displayName = null)
        {
            Id = id;
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? id : displayName;
        }

        public string Id { get; }
        public string DisplayName { get; }

        public static StatModifierSource Runtime(string id, string displayName = null)
        {
            return new StatModifierSource(id, displayName);
        }
    }
}
