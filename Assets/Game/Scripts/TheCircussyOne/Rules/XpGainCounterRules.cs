using UnityEngine;

namespace TheCircussyOne.Rules
{
    public sealed class XpGainCounterState
    {
        public int Amount { get; private set; }
        public float LastGainTime { get; private set; }
        public float PopStartedAt { get; private set; }
        public bool IsVisible { get; private set; }

        public bool Add(int amount, float time, float holdSeconds, float fadeSeconds)
        {
            if (amount <= 0)
            {
                return false;
            }

            if (!IsVisible || XpGainCounterRules.IsExpired(this, time, holdSeconds, fadeSeconds))
            {
                Amount = 0;
            }

            Amount += amount;
            LastGainTime = time;
            PopStartedAt = time;
            IsVisible = true;
            return true;
        }

        public void Reset()
        {
            Amount = 0;
            LastGainTime = 0f;
            PopStartedAt = 0f;
            IsVisible = false;
        }
    }

    public readonly struct XpGainCounterFrame
    {
        public XpGainCounterFrame(string text, Vector3 position, float scale, Color color, bool visible)
        {
            Text = text;
            Position = position;
            Scale = scale;
            Color = color;
            Visible = visible;
        }

        public string Text { get; }
        public Vector3 Position { get; }
        public float Scale { get; }
        public Color Color { get; }
        public bool Visible { get; }
    }

    public static class XpGainCounterRules
    {
        public static bool IsExpired(XpGainCounterState state, float currentTime, float holdSeconds, float fadeSeconds)
        {
            if (state == null || !state.IsVisible)
            {
                return true;
            }

            return currentTime >= state.LastGainTime + Mathf.Max(0f, holdSeconds) + Mathf.Max(0.01f, fadeSeconds);
        }

        public static float Alpha(float secondsSinceGain, float holdSeconds, float fadeSeconds, EaseSettings fadeEase)
        {
            float hold = Mathf.Max(0f, holdSeconds);
            float fade = Mathf.Max(0.01f, fadeSeconds);
            if (secondsSinceGain <= hold)
            {
                return 1f;
            }

            float t = Mathf.Clamp01((secondsSinceGain - hold) / fade);
            return 1f - GameEasing.Evaluate01(fadeEase, t);
        }

        public static float Scale(
            float secondsSincePop,
            float baseWorldScale,
            float normalScale,
            float popScale,
            float popReturnSeconds,
            EaseSettings popEase)
        {
            float duration = Mathf.Max(0.01f, popReturnSeconds);
            float t = Mathf.Clamp01(secondsSincePop / duration);
            float eased = GameEasing.Evaluate01(popEase, t);
            float scale = Mathf.LerpUnclamped(Mathf.Max(0.01f, popScale), Mathf.Max(0.01f, normalScale), eased);
            return Mathf.Max(0.001f, baseWorldScale) * Mathf.Max(0.001f, scale);
        }

        public static Vector3 CameraRelativePosition(
            Vector3 playerPosition,
            Vector3 cameraForward,
            Vector3 cameraRight,
            float rightOffset,
            float heightOffset,
            float forwardOffset)
        {
            Vector3 right = FlattenOrFallback(cameraRight, Vector3.right);
            Vector3 forward = FlattenOrFallback(cameraForward, Vector3.forward);
            return playerPosition + right * rightOffset + forward * forwardOffset + Vector3.up * heightOffset;
        }

        public static XpGainCounterFrame Evaluate(
            XpGainCounterState state,
            float currentTime,
            Vector3 playerPosition,
            Vector3 cameraForward,
            Vector3 cameraRight,
            float rightOffset,
            float heightOffset,
            float forwardOffset,
            float holdSeconds,
            float fadeSeconds,
            float baseWorldScale,
            float normalScale,
            float popScale,
            float popReturnSeconds,
            Color color,
            EaseSettings popEase,
            EaseSettings fadeEase)
        {
            if (state == null || !state.IsVisible || state.Amount <= 0 || IsExpired(state, currentTime, holdSeconds, fadeSeconds))
            {
                return new XpGainCounterFrame(string.Empty, Vector3.zero, 0f, Color.clear, false);
            }

            float secondsSinceGain = Mathf.Max(0f, currentTime - state.LastGainTime);
            float secondsSincePop = Mathf.Max(0f, currentTime - state.PopStartedAt);
            Color frameColor = color;
            frameColor.a *= Alpha(secondsSinceGain, holdSeconds, fadeSeconds, fadeEase);

            return new XpGainCounterFrame(
                $"+{state.Amount}",
                CameraRelativePosition(playerPosition, cameraForward, cameraRight, rightOffset, heightOffset, forwardOffset),
                Scale(secondsSincePop, baseWorldScale, normalScale, popScale, popReturnSeconds, popEase),
                frameColor,
                frameColor.a > 0.001f);
        }

        private static Vector3 FlattenOrFallback(Vector3 vector, Vector3 fallback)
        {
            vector.y = 0f;
            if (vector.sqrMagnitude <= 0.0001f)
            {
                return fallback;
            }

            return vector.normalized;
        }
    }
}
