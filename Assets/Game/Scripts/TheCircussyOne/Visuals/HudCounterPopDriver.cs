using TheCircussyOne.Config;
using TheCircussyOne.Rules;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheCircussyOne.Visuals
{
    internal static class HudCounterPopDriver
    {
        public static void Start(ref HudCounterPopState state, VisualElement target, HudCounterPopSettings settings)
        {
            if (target == null || !settings.enabled)
            {
                return;
            }

            state.Active = true;
            state.Elapsed = 0f;
            ApplyScale(target, Mathf.Max(1f, settings.scale));
        }

        public static void Tick(ref HudCounterPopState state, VisualElement target, float deltaTime, HudCounterPopSettings settings)
        {
            if (target == null)
            {
                state = default;
                return;
            }

            if (!settings.enabled)
            {
                Reset(ref state, target);
                return;
            }

            if (!state.Active)
            {
                ApplyScale(target, 1f);
                return;
            }

            state.Elapsed += Mathf.Max(0f, deltaTime);
            float duration = Mathf.Max(0.01f, settings.seconds);
            if (state.Elapsed >= duration)
            {
                Reset(ref state, target);
                return;
            }

            float t = Mathf.Clamp01(state.Elapsed / duration);
            float eased = GameEasing.Evaluate01(settings.ease, t);
            float scale = Mathf.LerpUnclamped(Mathf.Max(1f, settings.scale), 1f, eased);
            ApplyScale(target, Mathf.Max(0.01f, scale));
        }

        public static void Reset(ref HudCounterPopState state, VisualElement target)
        {
            state = default;
            if (target != null)
            {
                ApplyScale(target, 1f);
            }
        }

        private static void ApplyScale(VisualElement target, float scale)
        {
            target.style.scale = new Scale(new Vector3(scale, scale, 1f));
        }
    }

    internal struct HudCounterPopState
    {
        public bool Active;
        public float Elapsed;
    }

    internal readonly struct UiPopupMotionFrame
    {
        public UiPopupMotionFrame(float scale, float opacity)
        {
            Scale = scale;
            Opacity = opacity;
        }

        public float Scale { get; }
        public float Opacity { get; }
    }

    internal sealed class UiPopupMotionDriver
    {
        private bool active;
        private float elapsedSeconds;

        public bool IsActive => active;

        public void Show(VisualElement displayRoot, VisualElement motionTarget, HudPopupMotionSettings settings)
        {
            if (displayRoot != null)
            {
                displayRoot.style.display = DisplayStyle.Flex;
            }

            motionTarget ??= displayRoot;
            if (motionTarget == null)
            {
                active = false;
                elapsedSeconds = 0f;
                return;
            }

            settings.EnsureDefaults(true, 0.2f, 0.08f, 1f, true, EaseSettings.OutBack);
            elapsedSeconds = 0f;
            active = settings.enabled;
            Apply(motionTarget, active ? Evaluate(0f, settings) : new UiPopupMotionFrame(settings.endScale, 1f));
        }

        public void HideImmediate(VisualElement displayRoot, VisualElement motionTarget)
        {
            active = false;
            elapsedSeconds = 0f;
            motionTarget ??= displayRoot;
            if (motionTarget != null)
            {
                Apply(motionTarget, new UiPopupMotionFrame(1f, 1f));
            }

            if (displayRoot != null)
            {
                displayRoot.style.display = DisplayStyle.None;
            }
        }

        public void Tick(VisualElement motionTarget, float deltaTime, HudPopupMotionSettings settings)
        {
            if (!active || motionTarget == null)
            {
                return;
            }

            settings.EnsureDefaults(true, 0.2f, 0.08f, 1f, true, EaseSettings.OutBack);
            elapsedSeconds += Mathf.Max(0f, deltaTime);
            UiPopupMotionFrame frame = Evaluate(elapsedSeconds, settings);
            Apply(motionTarget, frame);
            if (elapsedSeconds >= Mathf.Max(0.01f, settings.seconds))
            {
                active = false;
                Apply(motionTarget, new UiPopupMotionFrame(settings.endScale, 1f));
            }
        }

        public static UiPopupMotionFrame Evaluate(float elapsedSeconds, HudPopupMotionSettings settings)
        {
            settings.EnsureDefaults(true, 0.2f, 0.08f, 1f, true, EaseSettings.OutBack);
            if (!settings.enabled)
            {
                return new UiPopupMotionFrame(settings.endScale, 1f);
            }

            float duration = Mathf.Max(0.01f, settings.seconds);
            float t = Mathf.Clamp01(Mathf.Max(0f, elapsedSeconds) / duration);
            float eased = GameEasing.Evaluate01(settings.ease, t);
            float scale = Mathf.Max(0.01f, Mathf.LerpUnclamped(settings.startScale, settings.endScale, eased));
            float opacity = settings.fadeIn ? Mathf.Clamp01(t) : 1f;
            return new UiPopupMotionFrame(scale, opacity);
        }

        private static void Apply(VisualElement target, UiPopupMotionFrame frame)
        {
            target.style.scale = new Scale(new Vector3(frame.Scale, frame.Scale, 1f));
            target.style.opacity = frame.Opacity;
        }
    }
}
