using System;
using VContainer.Unity;

namespace TheCircussyOne.Runtime
{
    public sealed class GameOverPauseSystem : IStartable, IDisposable
    {
        private readonly GameState state;
        private readonly RunPauseState pauseState;
        private readonly RunPhaseState phaseState;

        public GameOverPauseSystem(GameState state, RunPauseState pauseState, RunPhaseState phaseState = null)
        {
            this.state = state;
            this.pauseState = pauseState;
            this.phaseState = phaseState;
        }

        public void Start()
        {
            if (state == null)
            {
                return;
            }

            state.GameOver += OnGameOver;
            state.HealthChanged += OnHealthChanged;
            if (state.IsGameOver)
            {
                OnGameOver();
            }
        }

        public void Dispose()
        {
            if (state != null)
            {
                state.GameOver -= OnGameOver;
                state.HealthChanged -= OnHealthChanged;
            }
        }

        private void OnGameOver()
        {
            phaseState?.FailRun();
            pauseState?.Pause(RunPauseReasons.GameOver);
        }

        private void OnHealthChanged(int current, int max)
        {
            if (current > 0)
            {
                pauseState?.Resume(RunPauseReasons.GameOver);
            }
        }
    }
}
