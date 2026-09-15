using TheCircussyOne.Runtime;

namespace TheCircussyOne.Rules
{
    public static class RunPhaseRules
    {
        public static bool CanTransition(RunPhase current, RunPhase next)
        {
            if (current == next)
            {
                return true;
            }

            if (next == RunPhase.Failed)
            {
                return IsRunInProgress(current);
            }

            return current switch
            {
                RunPhase.PerformerSelection => next == RunPhase.WorldActive,
                RunPhase.WorldActive => next is RunPhase.BossWarning or RunPhase.BossActive,
                RunPhase.BossWarning => next is RunPhase.BossActive or RunPhase.Failed,
                RunPhase.BossActive => next is RunPhase.BossDefeated or RunPhase.Encore or RunPhase.Failed,
                RunPhase.BossDefeated => next == RunPhase.Encore,
                RunPhase.Encore => next is RunPhase.Intermission or RunPhase.Finished or RunPhase.Failed,
                RunPhase.Intermission => next is RunPhase.NextWorldTransition or RunPhase.Failed,
                RunPhase.NextWorldTransition => next == RunPhase.WorldActive,
                _ => false
            };
        }

        public static bool IsRunInProgress(RunPhase phase)
        {
            return phase is RunPhase.WorldActive
                or RunPhase.BossWarning
                or RunPhase.BossActive
                or RunPhase.BossDefeated
                or RunPhase.Encore
                or RunPhase.Intermission
                or RunPhase.NextWorldTransition;
        }

        public static bool ShouldTickCombat(RunPhase phase)
        {
            return phase is RunPhase.WorldActive
                or RunPhase.BossWarning
                or RunPhase.BossActive
                or RunPhase.BossDefeated
                or RunPhase.Encore;
        }

        public static bool ShouldSpawnEnemies(RunPhase phase)
        {
            return phase is RunPhase.WorldActive
                or RunPhase.BossWarning
                or RunPhase.BossActive
                or RunPhase.BossDefeated
                or RunPhase.Encore;
        }

        public static bool ShouldAllowInteractions(RunPhase phase)
        {
            return phase is RunPhase.WorldActive
                or RunPhase.BossWarning
                or RunPhase.BossActive
                or RunPhase.BossDefeated
                or RunPhase.Encore;
        }

        public static bool ShouldShowWorldPrompts(RunPhase phase)
        {
            return ShouldAllowInteractions(phase);
        }

        public static bool IsTerminal(RunPhase phase)
        {
            return phase is RunPhase.Finished or RunPhase.Failed;
        }
    }
}
