using System;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;
using UnityEngine;

namespace TheCircussyOne.Runtime
{
    public readonly struct RunShowtimeEvent
    {
        public RunShowtimeEvent(int actNumber, string actName, int showtimeIndex, float scheduledSeconds)
        {
            ActNumber = actNumber;
            ActName = actName ?? string.Empty;
            ShowtimeIndex = showtimeIndex;
            ScheduledSeconds = scheduledSeconds;
        }

        public int ActNumber { get; }
        public string ActName { get; }
        public int ShowtimeIndex { get; }
        public float ScheduledSeconds { get; }
    }

    public readonly struct RunActStartedEvent
    {
        public RunActStartedEvent(int actNumber, string actName, float durationSeconds)
        {
            ActNumber = actNumber;
            ActName = actName ?? string.Empty;
            DurationSeconds = durationSeconds;
        }

        public int ActNumber { get; }
        public string ActName { get; }
        public float DurationSeconds { get; }
    }

    public readonly struct RunActFinaleEvent
    {
        public RunActFinaleEvent(int actNumber, string actName, float scheduledSeconds)
        {
            ActNumber = actNumber;
            ActName = actName ?? string.Empty;
            ScheduledSeconds = scheduledSeconds;
        }

        public int ActNumber { get; }
        public string ActName { get; }
        public float ScheduledSeconds { get; }
    }

    public sealed class RunActScheduleState
    {
        private readonly RunScheduleConfig config;
        private RunScheduleActDefinition activeAct;
        private bool[] firedShowtimes = Array.Empty<bool>();
        private int activeShowtimeIndex = -1;
        private float activeShowtimeElapsedSeconds;
        private bool isEncoreActive;
        private float encoreGraceRemainingSeconds;
        private bool encorePressureStarted;

        public RunActScheduleState(RunScheduleConfig config)
        {
            this.config = config != null ? config : RunScheduleConfig.CreateRuntimeDefault();
            this.config.EnsureWorkflowDefaults();
        }

        public event Action<RunShowtimeEvent> ShowtimeStarted;
        public event Action<RunShowtimeEvent> ShowtimeEnded;
        public event Action<RunActStartedEvent> ActStarted;
        public event Action<RunActFinaleEvent> ActFinaleBecameDue;
        public event Action EncorePressureStarted;

        public bool HasActiveAct => activeAct != null;
        public int ActIndex { get; private set; } = -1;
        public int ActNumber => ActIndex >= 0 ? ActIndex + 1 : 0;
        public int ActCount => config.Acts?.Count ?? 0;
        public bool HasNextAct => HasActiveAct && ActNumber < ActCount;
        public string ActName => activeAct != null ? activeAct.DisplayName : string.Empty;
        public float ActDurationSeconds => activeAct != null ? activeAct.DurationSeconds : 0f;
        public float ActElapsedSeconds { get; private set; }
        public float ActRemainingSeconds => RunActScheduleRules.Remaining(ActDurationSeconds, ActElapsedSeconds);
        public bool IsEncoreActive => isEncoreActive;
        public float EncoreElapsedSeconds { get; private set; }
        public float EncoreGraceSeconds => Mathf.Max(0f, config.encoreGraceSeconds);
        public float EncoreGraceRemainingSeconds => isEncoreActive ? Mathf.Max(0f, encoreGraceRemainingSeconds) : 0f;
        public bool IsEncoreGraceActive => isEncoreActive && EncoreGraceRemainingSeconds > 0f;
        public bool IsEncorePressureActive => isEncoreActive && !IsEncoreGraceActive;
        public float EncorePressureElapsedSeconds => isEncoreActive
            ? Mathf.Max(0f, EncoreElapsedSeconds - EncoreGraceSeconds)
            : 0f;
        public int EncorePressureStep => RunActScheduleRules.PressureStep(
            EncorePressureElapsedSeconds,
            config.encorePressureIntervalSeconds);
        public bool IsActFinaleDue { get; private set; }
        public bool IsShowtimeActive => activeShowtimeIndex >= 0;
        public int ActiveShowtimeIndex => activeShowtimeIndex;
        public float ShowtimeRemainingSeconds => IsShowtimeActive
            ? Mathf.Max(0f, config.showtimePressureSeconds - activeShowtimeElapsedSeconds)
            : 0f;

        public float ShowtimeSpawnIntervalMultiplier => RunActScheduleRules.PressureMultiplier(
            1f,
            IsShowtimeActive,
            config.showtimeSpawnIntervalMultiplier);

        public float ShowtimeMaxEnemiesMultiplier => RunActScheduleRules.PressureMultiplier(
            1f,
            IsShowtimeActive,
            config.showtimeMaxEnemiesMultiplier);

        public float EncoreSpawnIntervalMultiplier => RunActScheduleRules.RepeatedPressureMultiplier(
            1f,
            IsEncoreActive,
            config.encoreSpawnIntervalMultiplierPerStep,
            EncorePressureStep);

        public float EncoreMaxEnemiesMultiplier => RunActScheduleRules.RepeatedPressureMultiplier(
            1f,
            IsEncoreActive,
            config.encoreMaxEnemiesMultiplierPerStep,
            EncorePressureStep);

        public void Reset()
        {
            activeAct = null;
            firedShowtimes = Array.Empty<bool>();
            activeShowtimeIndex = -1;
            activeShowtimeElapsedSeconds = 0f;
            isEncoreActive = false;
            EncoreElapsedSeconds = 0f;
            encoreGraceRemainingSeconds = 0f;
            encorePressureStarted = false;
            ActIndex = -1;
            ActElapsedSeconds = 0f;
            IsActFinaleDue = false;
        }

        public void BeginAct(int worldIndex)
        {
            config.EnsureWorkflowDefaults();
            int resolvedActIndex = RunActScheduleRules.ResolveActIndex(worldIndex, config.Acts?.Count ?? 0);
            if (resolvedActIndex < 0)
            {
                Reset();
                return;
            }

            ActIndex = resolvedActIndex;
            activeAct = config.Acts[resolvedActIndex];
            int showtimeCount = activeAct?.ShowtimeSeconds?.Count ?? 0;
            firedShowtimes = showtimeCount > 0 ? new bool[showtimeCount] : Array.Empty<bool>();
            activeShowtimeIndex = -1;
            activeShowtimeElapsedSeconds = 0f;
            isEncoreActive = false;
            EncoreElapsedSeconds = 0f;
            encoreGraceRemainingSeconds = 0f;
            encorePressureStarted = false;
            ActElapsedSeconds = 0f;
            IsActFinaleDue = false;
            ActStarted?.Invoke(new RunActStartedEvent(ActNumber, ActName, ActDurationSeconds));
        }

        public void Tick(float deltaTime)
        {
            if (activeAct == null)
            {
                return;
            }

            float clampedDelta = Mathf.Max(0f, deltaTime);
            if (clampedDelta <= 0f)
            {
                return;
            }

            float previousElapsed = ActElapsedSeconds;
            ActElapsedSeconds = RunActScheduleRules.Elapsed(ActElapsedSeconds, clampedDelta);
            FireDueShowtimes(previousElapsed, ActElapsedSeconds);
            TickActiveShowtime(clampedDelta);
            FireFinaleIfDue(previousElapsed, ActElapsedSeconds);
        }

        public void BeginEncore()
        {
            if (!HasActiveAct)
            {
                return;
            }

            if (isEncoreActive)
            {
                return;
            }

            activeShowtimeIndex = -1;
            activeShowtimeElapsedSeconds = 0f;
            isEncoreActive = true;
            EncoreElapsedSeconds = 0f;
            encoreGraceRemainingSeconds = EncoreGraceSeconds;
            encorePressureStarted = false;
            TryFireEncorePressureStarted();
        }

        public void TickEncore(float deltaTime)
        {
            if (!IsEncoreActive)
            {
                return;
            }

            float clampedDelta = Mathf.Max(0f, deltaTime);
            EncoreElapsedSeconds = RunActScheduleRules.Elapsed(EncoreElapsedSeconds, clampedDelta);
            if (encoreGraceRemainingSeconds > 0f)
            {
                encoreGraceRemainingSeconds = Mathf.Max(0f, encoreGraceRemainingSeconds - clampedDelta);
            }

            TryFireEncorePressureStarted();
        }

        public void EndEncore()
        {
            isEncoreActive = false;
            EncoreElapsedSeconds = 0f;
            encoreGraceRemainingSeconds = 0f;
            encorePressureStarted = false;
        }

        public bool ForceEndEncoreGrace()
        {
            if (!IsEncoreGraceActive)
            {
                return false;
            }

            EncoreElapsedSeconds = Mathf.Max(EncoreElapsedSeconds, EncoreGraceSeconds);
            encoreGraceRemainingSeconds = 0f;
            TryFireEncorePressureStarted();
            return true;
        }

        private void TryFireEncorePressureStarted()
        {
            if (encorePressureStarted || !IsEncorePressureActive)
            {
                return;
            }

            encorePressureStarted = true;
            EncorePressureStarted?.Invoke();
        }

        public bool TryGetNextShowtimeRemainingSeconds(out float seconds)
        {
            seconds = -1f;
            if (activeAct == null || activeAct.ShowtimeSeconds == null)
            {
                return false;
            }

            // Debug and scripted triggers reuse the normal event path so HUD and spawner listeners stay in sync.
            for (int i = 0; i < activeAct.ShowtimeSeconds.Count; i++)
            {
                if (i < firedShowtimes.Length && firedShowtimes[i])
                {
                    continue;
                }

                float remaining = activeAct.ShowtimeSeconds[i] - ActElapsedSeconds;
                if (remaining >= 0f)
                {
                    seconds = remaining;
                    return true;
                }
            }

            return false;
        }

        public bool ForceNextShowtime()
        {
            if (activeAct == null || activeAct.ShowtimeSeconds == null || IsShowtimeActive)
            {
                return false;
            }

            for (int i = 0; i < activeAct.ShowtimeSeconds.Count; i++)
            {
                if (i < firedShowtimes.Length && firedShowtimes[i])
                {
                    continue;
                }

                StartShowtime(i, activeAct.ShowtimeSeconds[i]);
                return true;
            }

            return false;
        }

        public bool ForceActFinale()
        {
            if (activeAct == null || IsActFinaleDue)
            {
                return false;
            }

            // Force the timer to the finale boundary so later schedule reads match the emitted finale state.
            ActElapsedSeconds = Mathf.Max(ActElapsedSeconds, ActDurationSeconds);
            IsActFinaleDue = true;
            ActFinaleBecameDue?.Invoke(new RunActFinaleEvent(ActNumber, ActName, ActDurationSeconds));
            return true;
        }

        private void FireDueShowtimes(float previousElapsed, float currentElapsed)
        {
            if (activeAct.ShowtimeSeconds == null)
            {
                return;
            }

            for (int i = 0; i < activeAct.ShowtimeSeconds.Count; i++)
            {
                if (i < firedShowtimes.Length && firedShowtimes[i])
                {
                    continue;
                }

                float scheduledSeconds = activeAct.ShowtimeSeconds[i];
                if (!RunActScheduleRules.IsShowtimeDue(previousElapsed, currentElapsed, scheduledSeconds))
                {
                    continue;
                }

                StartShowtime(i, scheduledSeconds);
            }
        }

        private void StartShowtime(int index, float scheduledSeconds)
        {
            if (index >= 0 && index < firedShowtimes.Length)
            {
                firedShowtimes[index] = true;
            }

            activeShowtimeIndex = index;
            activeShowtimeElapsedSeconds = 0f;
            ShowtimeStarted?.Invoke(new RunShowtimeEvent(ActNumber, ActName, index, scheduledSeconds));
        }

        private void TickActiveShowtime(float deltaTime)
        {
            if (activeShowtimeIndex < 0)
            {
                return;
            }

            activeShowtimeElapsedSeconds += deltaTime;
            if (activeShowtimeElapsedSeconds < config.showtimePressureSeconds)
            {
                return;
            }

            int endedIndex = activeShowtimeIndex;
            float scheduledSeconds = activeAct != null
                && activeAct.ShowtimeSeconds != null
                && endedIndex >= 0
                && endedIndex < activeAct.ShowtimeSeconds.Count
                    ? activeAct.ShowtimeSeconds[endedIndex]
                    : 0f;

            activeShowtimeIndex = -1;
            activeShowtimeElapsedSeconds = 0f;
            ShowtimeEnded?.Invoke(new RunShowtimeEvent(ActNumber, ActName, endedIndex, scheduledSeconds));
        }

        private void FireFinaleIfDue(float previousElapsed, float currentElapsed)
        {
            if (IsActFinaleDue || !RunActScheduleRules.IsFinaleDue(previousElapsed, currentElapsed, ActDurationSeconds))
            {
                return;
            }

            IsActFinaleDue = true;
            ActFinaleBecameDue?.Invoke(new RunActFinaleEvent(ActNumber, ActName, ActDurationSeconds));
        }
    }
}
