using UnityEngine;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;

namespace TheCircussyOne.Visuals
{
    internal sealed class ActorBodyMotionDriver
    {
        private readonly ActorMotionVisualState state = new();

        public float TargetSpeed01 => state.TargetSpeed01;

        public void SetSpeed(float normalizedSpeed, bool immediate)
        {
            float clampedSpeed = Mathf.Clamp01(normalizedSpeed);
            state.TargetSpeed01 = clampedSpeed;
            if (immediate)
            {
                state.SmoothedSpeed01 = clampedSpeed;
            }
        }

        public void StartPulse(float seconds, float scaleMultiplier, EaseSettings ease)
        {
            ActorMotionVisualRules.StartPulse(
                state,
                Mathf.Max(0f, seconds),
                Mathf.Max(1f, scaleMultiplier),
                ease);
        }

        public void StartPulse(ActorScalePulseSettings pulse)
        {
            if (!pulse.enabled)
            {
                return;
            }

            ActorMotionVisualRules.StartPulse(
                state,
                pulse.seconds,
                pulse.EffectiveXzScaleMultiplier,
                pulse.EffectiveYScaleMultiplier,
                pulse.ease,
                pulse.reboundStrength);
        }

        public void SetJumpHoldStretch(
            bool jumpHeld,
            bool rising,
            bool airborne,
            bool enabled,
            float maxSeconds,
            float releaseSeconds,
            float deltaTime)
        {
            ActorMotionVisualRules.TickJumpHoldStretch(
                state,
                jumpHeld,
                rising,
                airborne,
                enabled,
                maxSeconds,
                releaseSeconds,
                deltaTime);
        }

        public Vector3 TickScale(
            float deltaTime,
            float idleAmplitude,
            float idleSecondsPerCycle,
            float idleXzCompensation,
            float moveAmplitude,
            float moveSecondsPerCycle,
            float moveXzCompensation,
            float speedSmoothingSharpness,
            EaseSettings speedSmoothingEase,
            float jumpHoldXzScale = 1f,
            float jumpHoldYScale = 1f,
            EaseSettings jumpHoldEase = default)
        {
            Vector3 baseScale = ActorMotionVisualRules.TickScale(
                state,
                state.TargetSpeed01,
                deltaTime,
                idleAmplitude,
                idleSecondsPerCycle,
                idleXzCompensation,
                moveAmplitude,
                moveSecondsPerCycle,
                moveXzCompensation,
                speedSmoothingSharpness,
                speedSmoothingEase);
            return Vector3.Scale(
                baseScale,
                ActorMotionVisualRules.JumpHoldStretchScale(
                    state,
                    jumpHoldXzScale,
                    jumpHoldYScale,
                    jumpHoldEase));
        }

        public void Reset()
        {
            state.Reset();
        }
    }
}
