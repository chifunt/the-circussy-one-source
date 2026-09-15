using TheCircussyOne.Config;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheCircussyOne.Visuals
{
    internal readonly struct HudHealthProgressElements
    {
        public HudHealthProgressElements(VisualElement fill, Label label, VisualElement segments)
        {
            Fill = fill;
            Label = label;
            Segments = segments;
        }

        public VisualElement Fill { get; }
        public Label Label { get; }
        public VisualElement Segments { get; }
    }

    internal sealed class HudHealthProgressDriver
    {
        private HudBarTweenState tween;
        private float displayed01 = 1f;
        private int maxHealthForSegments = 100;
        private bool initialized;

        public int MaxHealthForSegments => maxHealthForSegments;

        public void SetHealth(HudHealthProgressElements elements, int current, int max, HudVisualConfig config, float scale)
        {
            float normalized = max <= 0 ? 0f : Mathf.Clamp01((float)current / max);
            if (ShouldAnimate(config))
            {
                HudBarTweenDriver.Start(ref tween, displayed01, normalized, config.healthBarTween);
            }
            else
            {
                SetDisplayed(elements, normalized);
            }

            SetLabel(elements, current, max);
            ConfigureSegments(elements, config, max, scale);
        }

        public void SetImmediate(HudHealthProgressElements elements, int current, int max, HudVisualConfig config, float scale)
        {
            float normalized = max <= 0 ? 0f : Mathf.Clamp01((float)current / max);
            tween = default;
            SetDisplayed(elements, normalized);
            SetLabel(elements, current, max);
            ConfigureSegments(elements, config, max, scale);
        }

        public void Tick(HudHealthProgressElements elements, float deltaTime)
        {
            HudBarTweenDriver.Tick(ref tween, value => SetDisplayed(elements, value), deltaTime);
        }

        private bool ShouldAnimate(HudVisualConfig config)
        {
            if (config == null || !initialized)
            {
                return false;
            }

            config.EnsureReadableDefaults();
            return config.healthBarTween.enabled;
        }

        private void SetDisplayed(HudHealthProgressElements elements, float normalized)
        {
            displayed01 = Mathf.Clamp01(normalized);
            initialized = true;
            if (elements.Fill != null)
            {
                elements.Fill.style.width = Length.Percent(displayed01 * 100f);
            }
        }

        private static void SetLabel(HudHealthProgressElements elements, int current, int max)
        {
            if (elements.Label != null)
            {
                elements.Label.text = $"{current} / {max}";
            }
        }

        private void ConfigureSegments(HudHealthProgressElements elements, HudVisualConfig config, int max, float scale)
        {
            maxHealthForSegments = Mathf.Max(0, max);
            HudStatusLayoutDriver.ConfigureHealthSegments(elements.Segments, config, maxHealthForSegments, scale);
        }
    }
}
