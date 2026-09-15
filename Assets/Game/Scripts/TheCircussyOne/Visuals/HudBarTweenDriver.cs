using System;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;
using UnityEngine;

namespace TheCircussyOne.Visuals
{
    internal static class HudBarTweenDriver
    {
        public static bool Start(ref HudBarTweenState state, float from, float to, HudBarTweenSettings settings)
        {
            if (!settings.enabled)
            {
                state = default;
                return false;
            }

            settings.EnsureDefaults(true, Mathf.Max(0.01f, settings.seconds), settings.ease);
            from = Mathf.Clamp01(from);
            to = Mathf.Clamp01(to);
            if (Mathf.Approximately(from, to))
            {
                state = default;
                return false;
            }

            state = new HudBarTweenState
            {
                Active = true,
                From = from,
                To = to,
                Duration = Mathf.Max(0.01f, settings.seconds),
                Ease = settings.ease
            };
            return true;
        }

        public static void Tick(ref HudBarTweenState state, Action<float> applyValue, float deltaTime)
        {
            if (!state.Active)
            {
                return;
            }

            state.Elapsed += Mathf.Max(0f, deltaTime);
            float duration = Mathf.Max(0.01f, state.Duration);
            if (state.Elapsed >= duration)
            {
                state.Active = false;
                applyValue?.Invoke(Mathf.Clamp01(state.To));
                return;
            }

            float t = Mathf.Clamp01(state.Elapsed / duration);
            float eased = GameEasing.Evaluate01(state.Ease, t);
            applyValue?.Invoke(Mathf.Clamp01(Mathf.LerpUnclamped(state.From, state.To, eased)));
        }
    }

    internal struct HudBarTweenState
    {
        public bool Active;
        public float From;
        public float To;
        public float Elapsed;
        public float Duration;
        public EaseSettings Ease;
    }
}
