using TheCircussyOne.Config;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheCircussyOne.Visuals
{
    internal static class HudBarSegmentDriver
    {
        public static void ConfigureXpSegments(VisualElement segmentRoot, HudVisualConfig config, float scale)
        {
            if (segmentRoot == null || config == null)
            {
                return;
            }

            segmentRoot.Clear();
            int segments = Mathf.Clamp(config.xpSegmentCount, 1, 40);
            if (segments <= 1)
            {
                segmentRoot.style.display = DisplayStyle.None;
                return;
            }

            segmentRoot.style.display = DisplayStyle.Flex;
            for (int i = 1; i < segments; i++)
            {
                VisualElement divider = CreateDivider("xp-segment-divider");
                divider.style.left = Length.Percent((float)i / segments * 100f);
                divider.style.width = Px(config.xpSegmentWidth, scale);
                divider.style.backgroundColor = config.xpSegmentColor.WithAlpha(config.xpSegmentOpacity);
                segmentRoot.Add(divider);
            }
        }

        public static void ConfigureHealthSegments(VisualElement segmentRoot, HudVisualConfig config, int maxHealth, float scale)
        {
            if (segmentRoot == null || config == null)
            {
                return;
            }

            int clampedMaxHealth = Mathf.Max(0, maxHealth);
            segmentRoot.Clear();
            float hpPerSegment = Mathf.Max(1f, config.healthHpPerSegment);
            float width = Mathf.Max(1f, config.healthWidth * scale);
            float spacing = width * hpPerSegment / Mathf.Max(1f, clampedMaxHealth);
            if (clampedMaxHealth <= hpPerSegment || spacing < Mathf.Max(0f, config.healthMinimumSegmentPixelSpacing))
            {
                segmentRoot.style.display = DisplayStyle.None;
                return;
            }

            segmentRoot.style.display = DisplayStyle.Flex;
            for (float threshold = hpPerSegment; threshold < clampedMaxHealth; threshold += hpPerSegment)
            {
                VisualElement divider = CreateDivider("health-segment-divider");
                divider.style.left = Length.Percent(threshold / clampedMaxHealth * 100f);
                divider.style.width = Px(config.healthSegmentWidth, scale);
                divider.style.backgroundColor = config.healthSegmentColor.WithAlpha(config.healthSegmentOpacity);
                segmentRoot.Add(divider);
            }
        }

        private static VisualElement CreateDivider(string className)
        {
            var divider = new VisualElement { pickingMode = PickingMode.Ignore };
            divider.AddToClassList(className);
            divider.style.position = Position.Absolute;
            divider.style.top = 0f;
            divider.style.bottom = 0f;
            return divider;
        }

        private static float Px(float value, float scale)
        {
            return value * scale;
        }
    }
}
