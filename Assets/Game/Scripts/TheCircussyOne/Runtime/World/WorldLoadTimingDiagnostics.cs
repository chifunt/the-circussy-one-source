using System;
using System.Diagnostics;
using TheCircussyOne.Config;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace TheCircussyOne.Runtime
{
    public sealed class WorldLoadTimingDiagnostics
    {
        public const string LogPrefix = "[LOAD-TIMING]";

        private static readonly WorldLoadTimingDiagnostics disabled = new(
            enabled: false,
            logAllStages: false,
            slowThresholdMs: 0f,
            transitionId: 0,
            worldIndex: 0,
            actNumber: 0,
            seed: 0);

        private readonly bool enabled;
        private readonly bool logAllStages;
        private readonly float slowThresholdMs;
        private readonly int transitionId;
        private readonly int worldIndex;
        private readonly int actNumber;
        private readonly int seed;
        private readonly int startFrame;

        private WorldLoadTimingDiagnostics(
            bool enabled,
            bool logAllStages,
            float slowThresholdMs,
            int transitionId,
            int worldIndex,
            int actNumber,
            int seed)
        {
            this.enabled = enabled;
            this.logAllStages = logAllStages;
            this.slowThresholdMs = Mathf.Max(0f, slowThresholdMs);
            this.transitionId = Mathf.Max(0, transitionId);
            this.worldIndex = Mathf.Max(0, worldIndex);
            this.actNumber = Mathf.Max(0, actNumber);
            this.seed = seed;
            startFrame = Time.frameCount;
        }

        public static WorldLoadTimingDiagnostics Disabled => disabled;
        public bool Enabled => enabled;
        public float SlowThresholdMs => slowThresholdMs;

        public static WorldLoadTimingDiagnostics ForWorld(
            RunWorldGenerationConfig config,
            int transitionId,
            WorldGenerationRequest request)
        {
            if (config == null || !config.WorldLoadTimingDiagnosticsEnabled)
            {
                return Disabled;
            }

            return new WorldLoadTimingDiagnostics(
                enabled: true,
                logAllStages: config.WorldLoadTimingLogAllStages,
                slowThresholdMs: config.WorldLoadTimingSlowThresholdMs,
                transitionId: transitionId,
                worldIndex: request.WorldIndex,
                actNumber: request.ActNumber,
                seed: request.Seed);
        }

        public TimingScope Stage(string stage)
        {
            return new TimingScope(this, stage);
        }

        public bool ShouldLogStage(double elapsedMs, bool slowOnly = false)
        {
            if (!enabled)
            {
                return false;
            }

            return elapsedMs >= slowThresholdMs || (!slowOnly && logAllStages);
        }

        public void LogStage(string stage, double elapsedMs, string details = null, bool slowOnly = false)
        {
            if (!ShouldLogStage(elapsedMs, slowOnly))
            {
                return;
            }

            string suffix = string.IsNullOrWhiteSpace(details) ? string.Empty : $" {details}";
            Debug.LogFormat(
                LogType.Log,
                LogOption.NoStacktrace,
                null,
                "{0}",
                $"{LogPrefix} transition={transitionId} world={worldIndex} act={actNumber} seed={seed} " +
                $"startFrame={startFrame} frame={Time.frameCount} stage={stage} ms={elapsedMs:0.###}{suffix}");
        }

        public static long Timestamp()
        {
            return Stopwatch.GetTimestamp();
        }

        public static double ElapsedMilliseconds(long startedAt)
        {
            return (Stopwatch.GetTimestamp() - startedAt) * 1000.0 / Stopwatch.Frequency;
        }

        public readonly struct TimingScope : IDisposable
        {
            private readonly WorldLoadTimingDiagnostics owner;
            private readonly string stage;
            private readonly long startedAt;

            public TimingScope(WorldLoadTimingDiagnostics owner, string stage)
            {
                this.owner = owner ?? Disabled;
                this.stage = string.IsNullOrWhiteSpace(stage) ? "Unnamed" : stage;
                startedAt = Timestamp();
            }

            public void Dispose()
            {
                owner.LogStage(stage, ElapsedMilliseconds(startedAt));
            }
        }
    }
}
