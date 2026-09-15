using System;
using UnityEngine;

namespace TheCircussyOne.Runtime
{
    public sealed class UnityGameTime : IGameTime
    {
        private readonly RunPauseState pauseState;
        private readonly Func<float> timeProvider;
        private readonly Func<float> deltaTimeProvider;
        private readonly float startedAt;
        private float pausedStartedAt;
        private float pausedDuration;

        public UnityGameTime()
            : this(null)
        {
        }

        public UnityGameTime(RunPauseState pauseState)
            : this(() => UnityEngine.Time.time, () => UnityEngine.Time.deltaTime, pauseState)
        {
        }

        public UnityGameTime(Func<float> timeProvider, Func<float> deltaTimeProvider, RunPauseState pauseState = null)
        {
            this.timeProvider = timeProvider ?? (() => UnityEngine.Time.time);
            this.deltaTimeProvider = deltaTimeProvider ?? (() => UnityEngine.Time.deltaTime);
            this.pauseState = pauseState;
            startedAt = this.timeProvider();
            if (pauseState != null)
            {
                pauseState.PauseChanged += OnPauseChanged;
            }
        }

        public float Time
        {
            get
            {
                float now = timeProvider();
                float currentPause = pauseState != null && pauseState.IsPaused ? now - pausedStartedAt : 0f;
                return Mathf.Max(0f, now - startedAt - pausedDuration - currentPause);
            }
        }

        public float DeltaTime => pauseState != null && pauseState.IsPaused ? 0f : deltaTimeProvider();

        private void OnPauseChanged(bool paused)
        {
            if (paused)
            {
                pausedStartedAt = timeProvider();
                return;
            }

            pausedDuration += timeProvider() - pausedStartedAt;
            pausedStartedAt = 0f;
        }
    }
}
