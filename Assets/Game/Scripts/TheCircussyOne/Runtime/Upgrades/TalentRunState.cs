using System.Collections.Generic;
using TheCircussyOne.Content;

namespace TheCircussyOne.Runtime
{
    public sealed class TalentRunState
    {
        private readonly Dictionary<string, int> levels = new(System.StringComparer.Ordinal);

        public int GetLevel(TalentDefinition talent)
        {
            return talent == null ? 0 : GetLevel(talent.Id);
        }

        public int GetLevel(string talentId)
        {
            return !string.IsNullOrWhiteSpace(talentId) && levels.TryGetValue(talentId, out int level) ? level : 0;
        }

        public int IncrementLevel(TalentDefinition talent)
        {
            if (talent == null || string.IsNullOrWhiteSpace(talent.Id))
            {
                return 0;
            }

            int nextLevel = GetLevel(talent.Id) + 1;
            levels[talent.Id] = nextLevel;
            return nextLevel;
        }

        public void Clear()
        {
            levels.Clear();
        }
    }
}
