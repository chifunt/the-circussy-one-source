using System;
using TheCircussyOne.Rules;

namespace TheCircussyOne.Runtime
{
    public sealed class RunPhaseState
    {
        public RunPhaseState()
        {
        }

        public RunPhase CurrentPhase { get; private set; }
        public float PhaseElapsedSeconds { get; private set; }
        public int WorldIndex { get; private set; } = 1;

        public event Action<RunPhase, RunPhase> PhaseChanged;

        public bool Is(RunPhase phase)
        {
            return CurrentPhase == phase;
        }

        public bool TrySetPhase(RunPhase nextPhase)
        {
            if (!RunPhaseRules.CanTransition(CurrentPhase, nextPhase))
            {
                return false;
            }

            SetPhase(nextPhase);
            return true;
        }

        public void ResetToPerformerSelection()
        {
            WorldIndex = 1;
            SetPhase(RunPhase.PerformerSelection);
        }

        public void BeginWorld()
        {
            _ = TrySetPhase(RunPhase.WorldActive);
        }

        public void StartBossWarning()
        {
            _ = TrySetPhase(RunPhase.BossWarning);
        }

        public void ActivateBoss()
        {
            _ = TrySetPhase(RunPhase.BossActive);
        }

        public void MarkBossDefeated()
        {
            _ = TrySetPhase(RunPhase.BossDefeated);
        }

        public void BeginEncore()
        {
            _ = TrySetPhase(RunPhase.Encore);
        }

        public void FinishRun()
        {
            _ = TrySetPhase(RunPhase.Finished);
        }

        public void EnterIntermission()
        {
            _ = TrySetPhase(RunPhase.Intermission);
        }

        public void FailRun()
        {
            _ = TrySetPhase(RunPhase.Failed);
        }

        public void BeginNextWorld()
        {
            if (CurrentPhase == RunPhase.NextWorldTransition
                || !RunPhaseRules.CanTransition(CurrentPhase, RunPhase.NextWorldTransition))
            {
                return;
            }

            WorldIndex++;
            SetPhase(RunPhase.NextWorldTransition);
        }

        public void Tick(float deltaTime)
        {
            PhaseElapsedSeconds += Math.Max(0f, deltaTime);
        }

        private void SetPhase(RunPhase nextPhase)
        {
            if (CurrentPhase == nextPhase)
            {
                return;
            }

            RunPhase previous = CurrentPhase;
            CurrentPhase = nextPhase;
            PhaseElapsedSeconds = 0f;
            PhaseChanged?.Invoke(previous, nextPhase);
        }
    }
}
