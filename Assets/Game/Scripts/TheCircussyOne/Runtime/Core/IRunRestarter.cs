namespace TheCircussyOne.Runtime
{
    public enum RunRestartReason
    {
        StageSetup = 0,
        RetryAfterDeath = 1
    }

    public interface IRunRestarter
    {
        void RestartRun();
        void RestartRun(RunRestartReason reason);
    }
}
