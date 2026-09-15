using UnityEngine;

namespace TheCircussyOne.Rules
{
    public sealed class ActorMotionVisualState
    {
        public float TargetSpeed01;
        public float SmoothedSpeed01;
        public float Phase01;
        public float PulseElapsed = 1f;
        public float PulseSeconds;
        public float PulseScaleMultiplier = 1f;
        public float PulseXzScaleMultiplier = 1f;
        public float PulseYScaleMultiplier = 1f;
        public float PulseReboundStrength;
        public EaseSettings PulseEase = EaseSettings.OutQuad;
        public float JumpHoldElapsed;
        public float JumpHoldWeight01;

        public bool HasPulse => PulseSeconds > 0f && PulseElapsed < PulseSeconds;

        public void Reset()
        {
            TargetSpeed01 = 0f;
            SmoothedSpeed01 = 0f;
            Phase01 = 0f;
            PulseElapsed = 1f;
            PulseSeconds = 0f;
            PulseScaleMultiplier = 1f;
            PulseXzScaleMultiplier = 1f;
            PulseYScaleMultiplier = 1f;
            PulseReboundStrength = 0f;
            PulseEase = EaseSettings.OutQuad;
            JumpHoldElapsed = 0f;
            JumpHoldWeight01 = 0f;
        }
    }

    public static class ActorMotionVisualRules
    {
        public static float NormalizeSpeed(float speed, float referenceSpeed)
        {
            if (referenceSpeed <= 0f)
            {
                return 0f;
            }

            return Mathf.Clamp01(Mathf.Abs(speed) / referenceSpeed);
        }

        public static void StartPulse(ActorMotionVisualState state, float seconds, float scaleMultiplier, EaseSettings ease)
        {
            StartPulse(state, seconds, scaleMultiplier, scaleMultiplier, ease, 0f);
        }

        public static void StartPulse(
            ActorMotionVisualState state,
            float seconds,
            float xzScaleMultiplier,
            float yScaleMultiplier,
            EaseSettings ease,
            float reboundStrength)
        {
            if (state == null
                || seconds <= 0f
                || (Mathf.Abs(xzScaleMultiplier - 1f) < 0.0001f && Mathf.Abs(yScaleMultiplier - 1f) < 0.0001f))
            {
                return;
            }

            state.PulseElapsed = 0f;
            state.PulseSeconds = seconds;
            state.PulseScaleMultiplier = Mathf.Max(xzScaleMultiplier, yScaleMultiplier);
            state.PulseXzScaleMultiplier = Mathf.Max(0.01f, xzScaleMultiplier);
            state.PulseYScaleMultiplier = Mathf.Max(0.01f, yScaleMultiplier);
            state.PulseReboundStrength = Mathf.Clamp01(reboundStrength);
            state.PulseEase = ease.shape > 0f ? ease : EaseSettings.OutQuad;
        }

        public static void TickJumpHoldStretch(
            ActorMotionVisualState state,
            bool jumpHeld,
            bool rising,
            bool airborne,
            bool enabled,
            float maxSeconds,
            float releaseSeconds,
            float deltaTime)
        {
            if (state == null)
            {
                return;
            }

            float safeDeltaTime = Mathf.Max(0f, deltaTime);
            bool shouldHold = enabled && jumpHeld && rising && airborne;
            if (shouldHold)
            {
                float safeMaxSeconds = Mathf.Max(0.0001f, maxSeconds);
                state.JumpHoldElapsed = Mathf.Min(safeMaxSeconds, state.JumpHoldElapsed + safeDeltaTime);
                state.JumpHoldWeight01 = Mathf.Clamp01(state.JumpHoldElapsed / safeMaxSeconds);
                return;
            }

            state.JumpHoldElapsed = 0f;
            float safeReleaseSeconds = Mathf.Max(0f, releaseSeconds);
            state.JumpHoldWeight01 = safeReleaseSeconds <= 0f
                ? 0f
                : Mathf.Max(0f, state.JumpHoldWeight01 - safeDeltaTime / safeReleaseSeconds);
        }

        public static Vector3 TickScale(
            ActorMotionVisualState state,
            float targetSpeed01,
            float deltaTime,
            float idleAmplitude,
            float idleSecondsPerCycle,
            float idleXzCompensation,
            float moveAmplitude,
            float moveSecondsPerCycle,
            float moveXzCompensation,
            float smoothingSharpness,
            EaseSettings smoothingEase)
        {
            if (state == null)
            {
                return Vector3.one;
            }

            state.TargetSpeed01 = Mathf.Clamp01(targetSpeed01);

            float speedWeight = GameEasing.SmoothingWeight(smoothingSharpness, deltaTime, smoothingEase);
            state.SmoothedSpeed01 = Mathf.Lerp(state.SmoothedSpeed01, state.TargetSpeed01, speedWeight);

            float cycleDriver01 = Mathf.Max(state.TargetSpeed01, state.SmoothedSpeed01);
            float cycleSeconds = Mathf.Lerp(
                Mathf.Max(0.05f, idleSecondsPerCycle),
                Mathf.Max(0.05f, moveSecondsPerCycle),
                cycleDriver01);

            if (deltaTime > 0f)
            {
                state.Phase01 = Mathf.Repeat(state.Phase01 + deltaTime / cycleSeconds, 1f);
                if (state.HasPulse)
                {
                    state.PulseElapsed = Mathf.Min(state.PulseSeconds, state.PulseElapsed + deltaTime);
                }
            }

            float amplitude = Mathf.Lerp(Mathf.Max(0f, idleAmplitude), Mathf.Max(0f, moveAmplitude), state.SmoothedSpeed01);
            float xzCompensation = Mathf.Lerp(Mathf.Clamp01(idleXzCompensation), Mathf.Clamp01(moveXzCompensation), state.SmoothedSpeed01);
            if (HasDirectionalPulse(state) || state.JumpHoldWeight01 > 0f)
            {
                amplitude = 0f;
                xzCompensation = 0f;
            }

            return BodyScale(state.Phase01, amplitude, xzCompensation, PulseScale(state));
        }

        public static Vector3 JumpHoldStretchScale(
            ActorMotionVisualState state,
            float xzScaleMultiplier,
            float yScaleMultiplier,
            EaseSettings ease)
        {
            if (state == null || state.JumpHoldWeight01 <= 0f)
            {
                return Vector3.one;
            }

            float eased = GameEasing.Evaluate01(ease.shape > 0f ? ease : EaseSettings.OutCubic, Mathf.Clamp01(state.JumpHoldWeight01));
            float xz = Mathf.LerpUnclamped(1f, Mathf.Max(0.01f, xzScaleMultiplier), eased);
            float y = Mathf.LerpUnclamped(1f, Mathf.Max(0.01f, yScaleMultiplier), eased);
            return new Vector3(
                Mathf.Max(0.01f, xz),
                Mathf.Max(0.01f, y),
                Mathf.Max(0.01f, xz));
        }

        public static bool HasDirectionalPulse(ActorMotionVisualState state)
        {
            return state != null
                && state.HasPulse
                && Mathf.Abs(state.PulseXzScaleMultiplier - state.PulseYScaleMultiplier) > 0.0001f;
        }

        public static Vector3 BodyScale(float phase01, float amplitude, float xzCompensation, float pulseMultiplier)
        {
            return BodyScale(phase01, amplitude, xzCompensation, Vector3.one * Mathf.Max(0.01f, pulseMultiplier));
        }

        public static Vector3 BodyScale(float phase01, float amplitude, float xzCompensation, Vector3 pulseScale)
        {
            float wave = Mathf.Sin(Mathf.Repeat(phase01, 1f) * Mathf.PI * 2f);
            float stretch = wave * Mathf.Max(0f, amplitude);
            float y = Mathf.Max(0.01f, 1f + stretch);
            float xz = Mathf.Max(0.01f, 1f - stretch * Mathf.Clamp01(xzCompensation));
            return new Vector3(
                xz * Mathf.Max(0.01f, pulseScale.x),
                y * Mathf.Max(0.01f, pulseScale.y),
                xz * Mathf.Max(0.01f, pulseScale.z));
        }

        public static float PulseMultiplier(ActorMotionVisualState state)
        {
            if (state == null || !state.HasPulse)
            {
                return 1f;
            }

            float normalized = Mathf.Clamp01(state.PulseElapsed / Mathf.Max(0.0001f, state.PulseSeconds));
            float yoyo = normalized <= 0.5f
                ? normalized * 2f
                : (1f - normalized) * 2f;
            float eased = GameEasing.Evaluate01(state.PulseEase, yoyo);
            return Mathf.LerpUnclamped(1f, Mathf.Max(1f, state.PulseScaleMultiplier), eased);
        }

        public static Vector3 PulseScale(ActorMotionVisualState state)
        {
            if (state == null || !state.HasPulse)
            {
                return Vector3.one;
            }

            float normalized = Mathf.Clamp01(state.PulseElapsed / Mathf.Max(0.0001f, state.PulseSeconds));
            float yoyo = normalized <= 0.5f
                ? normalized * 2f
                : (1f - normalized) * 2f;
            float eased = GameEasing.Evaluate01(state.PulseEase, yoyo);
            Vector3 peak = new(
                Mathf.Max(0.01f, state.PulseXzScaleMultiplier),
                Mathf.Max(0.01f, state.PulseYScaleMultiplier),
                Mathf.Max(0.01f, state.PulseXzScaleMultiplier));
            Vector3 deviation = peak - Vector3.one;
            Vector3 scale = Vector3.one + deviation * eased;

            if (state.PulseReboundStrength > 0f && normalized > 0.5f)
            {
                float reboundT = (normalized - 0.5f) * 2f;
                float reboundWave = Mathf.Sin(Mathf.Clamp01(reboundT) * Mathf.PI);
                Vector3 reboundTarget = Vector3.one - deviation * state.PulseReboundStrength;
                scale = Vector3.LerpUnclamped(scale, reboundTarget, reboundWave);
            }

            return new Vector3(
                Mathf.Max(0.01f, scale.x),
                Mathf.Max(0.01f, scale.y),
                Mathf.Max(0.01f, scale.z));
        }
    }
}
