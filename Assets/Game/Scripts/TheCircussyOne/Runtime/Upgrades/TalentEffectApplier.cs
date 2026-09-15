using TheCircussyOne.Content;
using TheCircussyOne.Stats;

namespace TheCircussyOne.Runtime
{
    public sealed class TalentEffectApplier
    {
        private readonly TalentRunState runState;
        private readonly RunStats stats;
        private readonly GameState state;

        public TalentEffectApplier(TalentRunState runState, RunStats stats, GameState state)
        {
            this.runState = runState;
            this.stats = stats;
            this.state = state;
        }

        public bool Apply(TalentDefinition talent)
        {
            return Apply(talent, ContentRarity.Common);
        }

        public bool Apply(TalentDefinition talent, ContentRarity rarity)
        {
            if (talent == null || runState == null)
            {
                return false;
            }

            int currentLevel = runState.GetLevel(talent);
            if (currentLevel >= talent.EffectiveMaxLevel)
            {
                return false;
            }

            if (talent.statModifiers == null || talent.statModifiers.Count == 0)
            {
                return false;
            }

            int appliedLevel = currentLevel + 1;
            string sourceId = $"talent:{talent.Id}:{appliedLevel}";
            for (int i = 0; i < talent.statModifiers.Count; i++)
            {
                stats?.AddModifier(talent.statModifiers[i].ToRuntime(sourceId, rarity));
            }

            if (runState.IncrementLevel(talent) <= 0)
            {
                return false;
            }

            state?.RefreshDerivedStats();
            return true;
        }
    }
}
