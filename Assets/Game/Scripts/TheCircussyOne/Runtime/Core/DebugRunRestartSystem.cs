using VContainer.Unity;

namespace TheCircussyOne.Runtime
{
    public sealed class DebugRunRestartSystem : ITickable, ITickableWhenPaused
    {
        private readonly IInputService input;
        private readonly IRunRestarter restarter;
        private readonly RunPauseState pauseState;

        public DebugRunRestartSystem(IInputService input, IRunRestarter restarter, RunPauseState pauseState = null)
        {
            this.input = input;
            this.restarter = restarter;
            this.pauseState = pauseState;
        }

        public void Tick()
        {
            if (pauseState != null && pauseState.HasReason(RunPauseReasons.RunTransition))
            {
                return;
            }

            if (input != null && input.DebugRestartPressedThisFrame)
            {
                restarter?.RestartRun();
            }
        }
    }
}
