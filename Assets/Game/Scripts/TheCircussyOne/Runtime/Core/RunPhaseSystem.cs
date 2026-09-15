using VContainer.Unity;

namespace TheCircussyOne.Runtime
{
    public sealed class RunPhaseSystem : ITickable, ITickableWhenPaused
    {
        private readonly RunPhaseState phaseState;
        private readonly IGameTime time;

        public RunPhaseSystem(RunPhaseState phaseState, IGameTime time)
        {
            this.phaseState = phaseState;
            this.time = time;
        }

        public void Tick()
        {
            phaseState?.Tick(time != null ? time.DeltaTime : 0f);
        }
    }
}
