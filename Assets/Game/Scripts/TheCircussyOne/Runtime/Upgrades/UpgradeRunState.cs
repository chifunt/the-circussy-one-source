using System.Collections.Generic;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;

namespace TheCircussyOne.Runtime
{
    public sealed class UpgradeRunState
    {
        private readonly Dictionary<string, int> levels = new(System.StringComparer.Ordinal);
        private readonly List<UpgradeChoice> activeChoices = new();

        public IReadOnlyList<UpgradeChoice> ActiveChoices => activeChoices;
        public bool HasActiveChoices => activeChoices.Count > 0;

        public int GetLevel(UpgradeDefinition definition)
        {
            return definition == null ? 0 : GetLevel(definition.Id);
        }

        public int GetLevel(string upgradeId)
        {
            return !string.IsNullOrWhiteSpace(upgradeId) && levels.TryGetValue(upgradeId, out int level) ? level : 0;
        }

        public int IncrementLevel(UpgradeDefinition definition)
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.Id))
            {
                return 0;
            }

            int nextLevel = GetLevel(definition.Id) + 1;
            levels[definition.Id] = nextLevel;
            return nextLevel;
        }

        public void SetActiveChoices(IReadOnlyList<UpgradeChoice> choices)
        {
            activeChoices.Clear();
            if (choices == null)
            {
                return;
            }

            for (int i = 0; i < choices.Count; i++)
            {
                if (choices[i].HasChoice)
                {
                    activeChoices.Add(choices[i]);
                }
            }
        }

        public void ClearActiveChoices()
        {
            activeChoices.Clear();
        }

        public void Clear()
        {
            levels.Clear();
            activeChoices.Clear();
        }
    }
}
