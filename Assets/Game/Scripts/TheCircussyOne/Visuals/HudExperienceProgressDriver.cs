using TheCircussyOne.Config;
using TheCircussyOne.Rules;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheCircussyOne.Visuals
{
    internal readonly struct HudExperienceProgressElements
    {
        public HudExperienceProgressElements(VisualElement fill, VisualElement pulse)
        {
            Fill = fill;
            Pulse = pulse;
        }

        public VisualElement Fill { get; }
        public VisualElement Pulse { get; }
    }

    internal sealed class HudExperienceProgressDriver
    {
        private HudBarTweenState tween;
        private float displayed01;
        private float target01;
        private float heldRemainder01;
        private float pulseElapsed;
        private bool initialized;
        private bool levelUpHoldActive;

        public void SetExperience(HudExperienceProgressElements elements, int current, int target, HudVisualConfig config)
        {
            float normalized = target <= 0 ? 0f : Mathf.Clamp01((float)current / target);
            target01 = normalized;

            if (levelUpHoldActive)
            {
                heldRemainder01 = normalized;
                if (!HudBarTweenDriver.Start(ref tween, displayed01, 1f, CurrentTweenSettings(config)))
                {
                    SetDisplayed(elements, 1f);
                }
            }
            else if (ShouldAnimate(config))
            {
                HudBarTweenDriver.Start(ref tween, displayed01, normalized, config.xpBarTween);
            }
            else
            {
                SetDisplayed(elements, normalized);
            }
        }

        public void SetImmediate(HudExperienceProgressElements elements, int current, int target)
        {
            float normalized = target <= 0 ? 0f : Mathf.Clamp01((float)current / target);
            tween = default;
            target01 = normalized;
            heldRemainder01 = normalized;
            levelUpHoldActive = false;
            pulseElapsed = 0f;
            SetPulseOpacity(elements, 0f);
            SetDisplayed(elements, normalized);
        }

        public void BeginLevelUpHold(HudExperienceProgressElements elements, HudVisualConfig config)
        {
            levelUpHoldActive = true;
            pulseElapsed = 0f;
            heldRemainder01 = target01;
            if (!HudBarTweenDriver.Start(ref tween, displayed01, 1f, CurrentTweenSettings(config)))
            {
                SetDisplayed(elements, 1f);
            }

            SetPulseOpacity(elements, 0f);
        }

        public void EndLevelUpHold(HudExperienceProgressElements elements, HudVisualConfig config)
        {
            if (!levelUpHoldActive)
            {
                return;
            }

            levelUpHoldActive = false;
            pulseElapsed = 0f;
            SetPulseOpacity(elements, 0f);
            SetDisplayed(elements, 0f);
            if (ShouldAnimate(config))
            {
                HudBarTweenDriver.Start(ref tween, 0f, heldRemainder01, config.xpBarTween);
            }
            else
            {
                SetDisplayed(elements, heldRemainder01);
            }
        }

        public void Tick(HudExperienceProgressElements elements, float deltaTime, HudVisualConfig config)
        {
            HudBarTweenDriver.Tick(ref tween, value => SetDisplayed(elements, value), deltaTime);
            if (levelUpHoldActive && !tween.Active && displayed01 < 1f)
            {
                SetDisplayed(elements, 1f);
            }

            TickPulse(elements, deltaTime, config);
        }

        private void TickPulse(HudExperienceProgressElements elements, float deltaTime, HudVisualConfig config)
        {
            if (!levelUpHoldActive || config == null)
            {
                SetPulseOpacity(elements, 0f);
                return;
            }

            config.EnsureReadableDefaults();
            pulseElapsed += Mathf.Max(0f, deltaTime);
            float speed = Mathf.Max(0f, config.xpLevelUpPulseSpeed);
            float wave = speed <= 0f ? 1f : (Mathf.Sin(pulseElapsed * speed * Mathf.PI * 2f) + 1f) * 0.5f;
            SetPulseOpacity(elements, wave * Mathf.Clamp01(config.xpLevelUpPulseMaxOpacity));
        }

        private bool ShouldAnimate(HudVisualConfig config)
        {
            if (config == null || !initialized)
            {
                return false;
            }

            config.EnsureReadableDefaults();
            return config.xpBarTween.enabled;
        }

        private HudBarTweenSettings CurrentTweenSettings(HudVisualConfig config)
        {
            if (config == null)
            {
                return new HudBarTweenSettings(false, 0.01f, EaseSettings.Linear);
            }

            config.EnsureReadableDefaults();
            return config.xpBarTween;
        }

        private void SetDisplayed(HudExperienceProgressElements elements, float normalized)
        {
            displayed01 = Mathf.Clamp01(normalized);
            initialized = true;
            if (elements.Fill != null)
            {
                elements.Fill.style.width = Length.Percent(displayed01 * 100f);
            }
        }

        private static void SetPulseOpacity(HudExperienceProgressElements elements, float opacity)
        {
            if (elements.Pulse != null)
            {
                elements.Pulse.style.opacity = Mathf.Clamp01(opacity);
            }
        }
    }
}
