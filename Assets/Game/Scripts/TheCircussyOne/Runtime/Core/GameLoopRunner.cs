using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;

namespace TheCircussyOne.Runtime
{
    public sealed class GameLoopRunner : MonoBehaviour
    {
        private IReadOnlyList<ITickable> tickables;
        private IReadOnlyList<IFixedTickable> fixedTickables;
        private IReadOnlyList<ILateTickable> lateTickables;
        private RunPauseState pauseState;

        public void Construct(
            IReadOnlyList<IStartable> startables,
            IReadOnlyList<ITickable> tickables,
            IReadOnlyList<IFixedTickable> fixedTickables,
            IReadOnlyList<ILateTickable> lateTickables,
            RunPauseState pauseState = null)
        {
            this.tickables = tickables;
            this.fixedTickables = fixedTickables;
            this.lateTickables = lateTickables;
            this.pauseState = pauseState;

            for (int i = 0; i < startables.Count; i++)
            {
                startables[i].Start();
            }
        }

        private void Update()
        {
            if (tickables == null)
            {
                return;
            }

            bool pausedAtPhaseStart = pauseState != null && pauseState.IsPaused;
            for (int i = 0; i < tickables.Count; i++)
            {
                ITickable tickable = tickables[i];
                if (ShouldSkipGameplayTick(pausedAtPhaseStart, tickable))
                {
                    continue;
                }

                tickable.Tick();
            }
        }

        private void FixedUpdate()
        {
            if (fixedTickables == null)
            {
                return;
            }

            bool pausedAtPhaseStart = pauseState != null && pauseState.IsPaused;
            for (int i = 0; i < fixedTickables.Count; i++)
            {
                IFixedTickable tickable = fixedTickables[i];
                if (ShouldSkipGameplayFixedTick(pausedAtPhaseStart, tickable))
                {
                    continue;
                }

                tickable.FixedTick();
            }
        }

        private void LateUpdate()
        {
            if (lateTickables == null)
            {
                return;
            }

            bool pausedAtPhaseStart = pauseState != null && pauseState.IsPaused;
            for (int i = 0; i < lateTickables.Count; i++)
            {
                ILateTickable tickable = lateTickables[i];
                if (ShouldSkipGameplayLateTick(pausedAtPhaseStart, tickable))
                {
                    continue;
                }

                tickable.LateTick();
            }
        }

        private void OnDisable()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private bool ShouldSkipGameplayTick(bool pausedAtPhaseStart, ITickable tickable)
        {
            return GameLoopRunnerRules.ShouldSkipGameplayTick(
                pauseState != null,
                pausedAtPhaseStart,
                pauseState != null && pauseState.IsPaused,
                tickable is ITickableWhenPaused);
        }

        private bool ShouldSkipGameplayFixedTick(bool pausedAtPhaseStart, IFixedTickable tickable)
        {
            return GameLoopRunnerRules.ShouldSkipGameplayTick(
                pauseState != null,
                pausedAtPhaseStart,
                pauseState != null && pauseState.IsPaused,
                tickable is IFixedTickableWhenPaused);
        }

        private bool ShouldSkipGameplayLateTick(bool pausedAtPhaseStart, ILateTickable tickable)
        {
            return GameLoopRunnerRules.ShouldSkipGameplayTick(
                pauseState != null,
                pausedAtPhaseStart,
                pauseState != null && pauseState.IsPaused,
                tickable is ILateTickableWhenPaused);
        }
    }

    public static class GameLoopRunnerRules
    {
        public static bool ShouldSkipGameplayTick(
            bool hasPauseState,
            bool pausedAtPhaseStart,
            bool pausedNow,
            bool canTickWhenPaused)
        {
            return hasPauseState
                && !canTickWhenPaused
                && (pausedAtPhaseStart || pausedNow);
        }
    }
}
