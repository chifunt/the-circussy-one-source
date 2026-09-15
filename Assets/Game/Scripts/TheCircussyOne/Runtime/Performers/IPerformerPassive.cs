using TheCircussyOne.Content;
using TheCircussyOne.Stats;

namespace TheCircussyOne.Runtime
{
    public interface IPerformerPassive
    {
        PerformerPassiveKind Kind { get; }
        bool Apply(PerformerDefinition performer, RunStats stats, string sourceId);
    }
}
