using TheCircussyOne.Content;
using TheCircussyOne.Stats;

namespace TheCircussyOne.Runtime
{
    public sealed class StaticStatPerformerPassive : IPerformerPassive
    {
        public PerformerPassiveKind Kind => PerformerPassiveKind.StaticStatModifiers;

        public bool Apply(PerformerDefinition performer, RunStats stats, string sourceId)
        {
            if (performer == null || stats == null || performer.passiveStatModifiers == null)
            {
                return false;
            }

            bool applied = false;
            for (int i = 0; i < performer.passiveStatModifiers.Count; i++)
            {
                stats.AddModifier(performer.passiveStatModifiers[i].ToRuntime(sourceId));
                applied = true;
            }

            return applied;
        }
    }
}
