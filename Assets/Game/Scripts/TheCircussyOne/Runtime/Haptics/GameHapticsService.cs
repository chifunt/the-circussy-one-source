using System;
using System.Collections.Generic;
using TheCircussyOne.Config;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

namespace TheCircussyOne.Runtime
{
    public sealed class UnityGamepadHapticsBackend : IGamepadHapticsBackend
    {
        public bool IsAvailable => Gamepad.current != null;

        public void SetMotorSpeeds(float lowFrequency, float highFrequency)
        {
            Gamepad gamepad = Gamepad.current;
            if (gamepad == null)
            {
                return;
            }

            gamepad.SetMotorSpeeds(Mathf.Clamp01(lowFrequency), Mathf.Clamp01(highFrequency));
        }

        public void StopMotorSpeeds()
        {
            Gamepad.current?.SetMotorSpeeds(0f, 0f);
        }
    }

    public sealed class GameHapticsService : IGameHaptics, IStartable, ITickable, ITickableWhenPaused, IRunResettable, IDisposable
    {
        private readonly GameHapticsConfig config;
        private readonly IGamepadHapticsBackend backend;
        private readonly Func<float> deltaTimeProvider;
        private readonly RunPauseState pauseState;
        private readonly Dictionary<GameHapticsCue, float> cooldownRemaining = new();
        private GameHapticsPattern activePattern;
        private int activePulseIndex;
        private float activePulseElapsedSeconds;
        private float activePulseDelaySeconds;
        private float activeLowFrequency;
        private float activeHighFrequency;
        private float activeIntensityMultiplier = 1f;
        private int activePriority;
        private bool started;

        [Inject]
        public GameHapticsService(GameHapticsConfig config, IGamepadHapticsBackend backend = null, RunPauseState pauseState = null)
            : this(config, backend, () => Time.unscaledDeltaTime, pauseState)
        {
        }

        public GameHapticsService(GameHapticsConfig config, IGamepadHapticsBackend backend, Func<float> deltaTimeProvider)
            : this(config, backend, deltaTimeProvider, null)
        {
        }

        private GameHapticsService(GameHapticsConfig config, IGamepadHapticsBackend backend, Func<float> deltaTimeProvider, RunPauseState pauseState)
        {
            this.config = config != null ? config : GameHapticsConfig.CreateRuntimeDefault();
            this.config.EnsureWorkflowDefaults();
            this.backend = backend ?? new UnityGamepadHapticsBackend();
            this.deltaTimeProvider = deltaTimeProvider ?? (() => Time.unscaledDeltaTime);
            this.pauseState = pauseState;
        }

        public void Start()
        {
            started = true;
            StopAll();
        }

        public void Tick()
        {
            if (pauseState != null && pauseState.IsPaused && config != null && !config.allowHapticsWhilePaused)
            {
                StopAll();
                return;
            }

            float deltaTime = Mathf.Max(0f, deltaTimeProvider());
            TickCooldowns(deltaTime);
            TickActivePattern(deltaTime);
        }

        public void Play(GameHapticsCue cue, float intensityMultiplier = 1f)
        {
            if (!started)
            {
                started = true;
            }

            if (config == null || !config.hapticsEnabled || backend == null || !backend.IsAvailable)
            {
                return;
            }

            if (pauseState != null && pauseState.IsPaused && !config.allowHapticsWhilePaused)
            {
                return;
            }

            GameHapticsPattern pattern = config.PatternFor(cue);
            if (pattern == null || !pattern.enabled || pattern.pulses == null || pattern.pulses.Length == 0)
            {
                return;
            }

            if (cooldownRemaining.TryGetValue(cue, out float cooldown) && cooldown > 0f)
            {
                return;
            }

            int priority = Mathf.Max(0, pattern.priority);
            if (activePattern != null && priority < activePriority)
            {
                return;
            }

            float multiplier = Mathf.Clamp01(IsFinite(intensityMultiplier) ? intensityMultiplier : 1f);
            if (multiplier <= 0f)
            {
                return;
            }

            activePattern = pattern;
            activePriority = priority;
            activePulseIndex = 0;
            activePulseElapsedSeconds = 0f;
            activePulseDelaySeconds = Mathf.Max(0f, pattern.pulses[0].delayBeforeSeconds);
            activeLowFrequency = 0f;
            activeHighFrequency = 0f;
            activeIntensityMultiplier = multiplier;
            cooldownRemaining[cue] = Mathf.Max(0f, pattern.cooldownSeconds);
            StartPulse(pattern.pulses[0], multiplier);
        }

        public void StopAll()
        {
            activePattern = null;
            activePulseIndex = 0;
            activePulseElapsedSeconds = 0f;
            activePulseDelaySeconds = 0f;
            activeLowFrequency = 0f;
            activeHighFrequency = 0f;
            activeIntensityMultiplier = 1f;
            activePriority = 0;
            backend?.StopMotorSpeeds();
        }

        public void ResetRunState(RunResetContext context)
        {
            cooldownRemaining.Clear();
            StopAll();
        }

        public void Dispose()
        {
            StopAll();
        }

        private void TickCooldowns(float deltaTime)
        {
            if (deltaTime <= 0f || cooldownRemaining.Count == 0)
            {
                return;
            }

            s_staleCues.Clear();
            s_activeCues.Clear();
            foreach (KeyValuePair<GameHapticsCue, float> pair in cooldownRemaining)
            {
                s_activeCues.Add(pair);
            }

            for (int i = 0; i < s_activeCues.Count; i++)
            {
                KeyValuePair<GameHapticsCue, float> pair = s_activeCues[i];
                float remaining = pair.Value - deltaTime;
                if (remaining <= 0f)
                {
                    s_staleCues.Add(pair.Key);
                }
                else
                {
                    cooldownRemaining[pair.Key] = remaining;
                }
            }

            for (int i = 0; i < s_staleCues.Count; i++)
            {
                cooldownRemaining.Remove(s_staleCues[i]);
            }
        }

        private void TickActivePattern(float deltaTime)
        {
            if (activePattern == null || activePattern.pulses == null || activePulseIndex >= activePattern.pulses.Length)
            {
                return;
            }

            GameHapticsPulse pulse = activePattern.pulses[activePulseIndex];
            if (activePulseDelaySeconds > 0f)
            {
                activePulseDelaySeconds -= deltaTime;
                if (activePulseDelaySeconds > 0f)
                {
                    backend?.SetMotorSpeeds(0f, 0f);
                    return;
                }

                backend?.SetMotorSpeeds(activeLowFrequency, activeHighFrequency);
            }

            activePulseElapsedSeconds += deltaTime;
            float duration = Mathf.Min(
                Mathf.Max(0.001f, pulse.durationSeconds),
                config != null ? Mathf.Max(0.001f, config.maximumPulseSeconds) : 0.4f);
            if (activePulseElapsedSeconds < duration)
            {
                backend?.SetMotorSpeeds(activeLowFrequency, activeHighFrequency);
                return;
            }

            activePulseIndex++;
            activePulseElapsedSeconds = 0f;
            if (activePulseIndex >= activePattern.pulses.Length)
            {
                StopAll();
                return;
            }

            StartPulse(activePattern.pulses[activePulseIndex], activeIntensityMultiplier);
        }

        private void StartPulse(GameHapticsPulse pulse, float intensityMultiplier)
        {
            if (pulse == null)
            {
                StopAll();
                return;
            }

            float scale = Mathf.Clamp01((config != null ? config.masterScale : 1f) * (activePattern != null ? activePattern.scale : 1f) * intensityMultiplier);
            activePulseDelaySeconds = Mathf.Max(0f, pulse.delayBeforeSeconds);
            activeLowFrequency = Mathf.Clamp01(pulse.lowFrequency * scale);
            activeHighFrequency = Mathf.Clamp01(pulse.highFrequency * scale);
            if (activePulseDelaySeconds > 0f)
            {
                backend?.SetMotorSpeeds(0f, 0f);
                return;
            }

            backend?.SetMotorSpeeds(activeLowFrequency, activeHighFrequency);
        }

        private static readonly List<GameHapticsCue> s_staleCues = new();
        private static readonly List<KeyValuePair<GameHapticsCue, float>> s_activeCues = new();

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
