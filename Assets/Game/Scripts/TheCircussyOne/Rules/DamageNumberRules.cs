using UnityEngine;

namespace TheCircussyOne.Rules
{
    public readonly struct DamageNumberFrame
    {
        public DamageNumberFrame(Vector3 position, float scale, Color color, float normalizedAge)
        {
            Position = position;
            Scale = scale;
            Color = color;
            NormalizedAge = normalizedAge;
        }

        public Vector3 Position { get; }
        public float Scale { get; }
        public Color Color { get; }
        public float NormalizedAge { get; }
    }

    public static class DamageNumberRules
    {
        public static Vector3 SpawnPosition(Vector3 hitPosition, float heightBias, float jitterRadius, int seed)
        {
            if (jitterRadius <= 0f)
            {
                return hitPosition + Vector3.up * heightBias;
            }

            float angle = Hash01(seed, 17) * Mathf.PI * 2f;
            float radius = Mathf.Sqrt(Hash01(seed, 41)) * jitterRadius;
            Vector3 jitter = new(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
            return hitPosition + Vector3.up * heightBias + jitter;
        }

        public static Vector3 LateralDriftDirection(int seed)
        {
            float angle = Hash01(seed, 83) * Mathf.PI * 2f;
            return new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
        }

        public static float NormalizedAge(float spawnedAt, float currentTime, float lifetime)
        {
            if (lifetime <= 0f)
            {
                return 1f;
            }

            return Mathf.Clamp01((currentTime - spawnedAt) / lifetime);
        }

        public static DamageNumberFrame Evaluate(
            Vector3 startPosition,
            float normalizedAge,
            float floatDistance,
            float baseWorldScale,
            float startScale,
            float popScale,
            float endScale,
            float popDurationNormalized,
            float fadeStartNormalized,
            Color color)
        {
            return Evaluate(
                startPosition,
                normalizedAge,
                floatDistance,
                Vector3.zero,
                0f,
                baseWorldScale,
                startScale,
                popScale,
                endScale,
                popDurationNormalized,
                fadeStartNormalized,
                color);
        }

        public static DamageNumberFrame Evaluate(
            Vector3 startPosition,
            float normalizedAge,
            float floatDistance,
            Vector3 lateralDriftDirection,
            float lateralDriftDistance,
            float baseWorldScale,
            float startScale,
            float popScale,
            float endScale,
            float popDurationNormalized,
            float fadeStartNormalized,
            Color color)
        {
            return Evaluate(
                startPosition,
                normalizedAge,
                floatDistance,
                lateralDriftDirection,
                lateralDriftDistance,
                baseWorldScale,
                startScale,
                popScale,
                endScale,
                popDurationNormalized,
                fadeStartNormalized,
                color,
                EaseSettings.OutCubic,
                EaseSettings.InCubic,
                EaseSettings.InCubic,
                EaseSettings.OutBack);
        }

        public static DamageNumberFrame Evaluate(
            Vector3 startPosition,
            float normalizedAge,
            float floatDistance,
            float baseWorldScale,
            float startScale,
            float popScale,
            float endScale,
            float popDurationNormalized,
            float fadeStartNormalized,
            Color color,
            EaseSettings floatEase,
            EaseSettings fadeEase,
            EaseSettings shrinkEase,
            EaseSettings popEase)
        {
            return Evaluate(
                startPosition,
                normalizedAge,
                floatDistance,
                Vector3.zero,
                0f,
                baseWorldScale,
                startScale,
                popScale,
                endScale,
                popDurationNormalized,
                fadeStartNormalized,
                color,
                floatEase,
                fadeEase,
                shrinkEase,
                popEase);
        }

        public static DamageNumberFrame Evaluate(
            Vector3 startPosition,
            float normalizedAge,
            float floatDistance,
            Vector3 lateralDriftDirection,
            float lateralDriftDistance,
            float baseWorldScale,
            float startScale,
            float popScale,
            float endScale,
            float popDurationNormalized,
            float fadeStartNormalized,
            Color color,
            EaseSettings floatEase,
            EaseSettings fadeEase,
            EaseSettings shrinkEase,
            EaseSettings popEase)
        {
            float t = Mathf.Clamp01(normalizedAge);
            float motionT = GameEasing.Evaluate01(floatEase, t);
            Vector3 drift = lateralDriftDirection.sqrMagnitude > 0.0001f
                ? lateralDriftDirection.normalized * (motionT * Mathf.Max(0f, lateralDriftDistance))
                : Vector3.zero;
            Vector3 position = startPosition + Vector3.up * (motionT * Mathf.Max(0f, floatDistance)) + drift;
            float scale = Mathf.Max(0.001f, baseWorldScale) * Scale(t, startScale, popScale, endScale, popDurationNormalized, shrinkEase, popEase);
            Color fadedColor = color;
            fadedColor.a *= Alpha(t, fadeStartNormalized, fadeEase);
            return new DamageNumberFrame(position, scale, fadedColor, t);
        }

        public static float Alpha(float normalizedAge, float fadeStartNormalized)
        {
            return Alpha(normalizedAge, fadeStartNormalized, EaseSettings.InCubic);
        }

        public static float Alpha(float normalizedAge, float fadeStartNormalized, EaseSettings fadeEase)
        {
            float t = Mathf.Clamp01(normalizedAge);
            float fadeStart = Mathf.Clamp(fadeStartNormalized, 0f, 0.95f);
            if (t <= fadeStart)
            {
                return 1f;
            }

            float fadeT = Mathf.InverseLerp(fadeStart, 1f, t);
            return 1f - GameEasing.Evaluate01(fadeEase, fadeT);
        }

        public static float Scale(float normalizedAge, float startScale, float popScale, float endScale, float popDurationNormalized)
        {
            return Scale(normalizedAge, startScale, popScale, endScale, popDurationNormalized, EaseSettings.InCubic, EaseSettings.OutBack);
        }

        public static float Scale(
            float normalizedAge,
            float startScale,
            float popScale,
            float endScale,
            float popDurationNormalized,
            EaseSettings shrinkEase,
            EaseSettings popEase)
        {
            float t = Mathf.Clamp01(normalizedAge);
            float popDuration = Mathf.Clamp(popDurationNormalized, 0.01f, 0.8f);
            if (t <= popDuration)
            {
                return Mathf.LerpUnclamped(startScale, popScale, GameEasing.Evaluate01(popEase, t / popDuration));
            }

            float shrinkT = Mathf.InverseLerp(popDuration, 1f, t);
            return Mathf.Lerp(popScale, endScale, GameEasing.Evaluate01(shrinkEase, shrinkT));
        }

        private static float Hash01(int seed, int salt)
        {
            unchecked
            {
                uint hash = (uint)(seed * 374761393 + salt * 668265263);
                hash = (hash ^ (hash >> 13)) * 1274126177u;
                return (hash ^ (hash >> 16)) / 4294967295f;
            }
        }
    }
}
