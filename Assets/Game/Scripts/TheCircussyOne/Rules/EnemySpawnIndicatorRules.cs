using UnityEngine;

namespace TheCircussyOne.Rules
{
    public readonly struct EnemySpawnIndicatorFrame
    {
        public EnemySpawnIndicatorFrame(float outerRadius, float innerRadius, float centerRadius, Color outerColor, Color innerColor, Color centerColor, float normalizedWarning, float normalizedEmerge)
        {
            OuterRadius = outerRadius;
            InnerRadius = innerRadius;
            CenterRadius = centerRadius;
            OuterColor = outerColor;
            InnerColor = innerColor;
            CenterColor = centerColor;
            NormalizedWarning = normalizedWarning;
            NormalizedEmerge = normalizedEmerge;
        }

        public float OuterRadius { get; }
        public float InnerRadius { get; }
        public float CenterRadius { get; }
        public Color OuterColor { get; }
        public Color InnerColor { get; }
        public Color CenterColor { get; }
        public float NormalizedWarning { get; }
        public float NormalizedEmerge { get; }
    }

    public static class EnemySpawnIndicatorRules
    {
        public static float NormalizedWarning(float startedAt, float currentTime, float warningSeconds)
        {
            if (warningSeconds <= 0f)
            {
                return 1f;
            }

            return Mathf.Clamp01((currentTime - startedAt) / warningSeconds);
        }

        public static float NormalizedEmerge(float warningCompletedAt, float currentTime, float emergeSeconds)
        {
            if (emergeSeconds <= 0f)
            {
                return 1f;
            }

            return Mathf.Clamp01((currentTime - warningCompletedAt) / emergeSeconds);
        }

        public static bool WarningComplete(float normalizedWarning) => normalizedWarning >= 1f;
        public static bool EmergenceComplete(float normalizedEmerge) => normalizedEmerge >= 1f;

        public static float InnerRadius(float normalizedWarning, float startRadius, float finalRadius, EaseSettings ease)
        {
            float start = Mathf.Max(0f, startRadius);
            float end = Mathf.Max(start, finalRadius);
            return Mathf.LerpUnclamped(start, end, GameEasing.Evaluate01(ease, normalizedWarning));
        }

        public static float BodyLocalY(float normalizedEmerge, float startY, float endY, EaseSettings ease)
        {
            return Mathf.LerpUnclamped(startY, endY, GameEasing.Evaluate01(ease, normalizedEmerge));
        }

        public static EnemySpawnIndicatorFrame Evaluate(
            float normalizedWarning,
            float normalizedEmerge,
            float radius,
            float innerStartRadius,
            float centerRadiusMultiplier,
            Color outerColor,
            Color innerColor,
            Color centerColor,
            float outerPulseStrength,
            float outerPulseCycles,
            EaseSettings innerExpandEase,
            EaseSettings outerPulseEase)
        {
            float warningT = Mathf.Clamp01(normalizedWarning);
            float emergeT = Mathf.Clamp01(normalizedEmerge);
            float fade = 1f - emergeT;
            float pulseT = Mathf.PingPong(warningT * Mathf.Max(0f, outerPulseCycles), 1f);
            float pulse = GameEasing.Evaluate01(outerPulseEase, pulseT);
            float pulseMultiplier = 1f - Mathf.Clamp01(outerPulseStrength) + pulse * Mathf.Clamp01(outerPulseStrength);

            Color evaluatedOuter = outerColor;
            evaluatedOuter.a *= pulseMultiplier * fade;

            Color evaluatedInner = innerColor;
            evaluatedInner.a *= fade;

            Color evaluatedCenter = centerColor;
            evaluatedCenter.a *= fade;

            float safeRadius = Mathf.Max(0.01f, radius);
            float innerRadius = InnerRadius(warningT, innerStartRadius, safeRadius, innerExpandEase);
            float centerRadius = Mathf.Clamp01(centerRadiusMultiplier) * innerRadius;
            return new EnemySpawnIndicatorFrame(
                safeRadius,
                innerRadius,
                centerRadius,
                evaluatedOuter,
                evaluatedInner,
                evaluatedCenter,
                warningT,
                emergeT);
        }
    }
}
