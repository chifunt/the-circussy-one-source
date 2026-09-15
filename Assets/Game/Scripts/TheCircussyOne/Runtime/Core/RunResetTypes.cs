namespace TheCircussyOne.Runtime
{
    public enum RunResetKind
    {
        BeforeSceneReload = 0,
        ReturnToPerformerSelection = 10,
        StartNewRun = 20,
        WorldTransition = 30
    }

    public readonly struct RunResetContext
    {
        public RunResetContext(RunResetKind kind, RunRestartReason restartReason = RunRestartReason.StageSetup, int worldIndex = 0)
        {
            Kind = kind;
            RestartReason = restartReason;
            WorldIndex = worldIndex;
        }

        public RunResetKind Kind { get; }
        public RunRestartReason RestartReason { get; }
        public int WorldIndex { get; }
        public bool IsFullRunReset => Kind is RunResetKind.ReturnToPerformerSelection or RunResetKind.StartNewRun;
    }

    public interface IRunResettable
    {
        void ResetRunState(RunResetContext context);
    }
}
