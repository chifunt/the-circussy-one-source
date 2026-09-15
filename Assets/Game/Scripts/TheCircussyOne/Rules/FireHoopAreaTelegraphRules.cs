using UnityEngine;
using TheCircussyOne.Config;

namespace TheCircussyOne.Rules
{
    public readonly struct FireHoopAreaTelegraphFrame
    {
        public FireHoopAreaTelegraphFrame(
            bool visible,
            float radius,
            float ringWidth,
            Color fillColor,
            Color ringColor,
            float groundClearance,
            float probeHeight,
            float probeDepth,
            int sampleCount,
            float refreshSeconds)
        {
            Visible = visible;
            Radius = Mathf.Max(0f, radius);
            RingWidth = Mathf.Clamp(ringWidth, 0f, Radius);
            FillColor = fillColor;
            RingColor = ringColor;
            GroundClearance = Mathf.Max(0f, groundClearance);
            ProbeHeight = Mathf.Max(0.01f, probeHeight);
            ProbeDepth = Mathf.Max(0.01f, probeDepth);
            SampleCount = Mathf.Max(8, sampleCount);
            RefreshSeconds = Mathf.Max(0.01f, refreshSeconds);
        }

        public bool Visible { get; }
        public float Radius { get; }
        public float RingWidth { get; }
        public Color FillColor { get; }
        public Color RingColor { get; }
        public float GroundClearance { get; }
        public float ProbeHeight { get; }
        public float ProbeDepth { get; }
        public int SampleCount { get; }
        public float RefreshSeconds { get; }
        public float InnerRingRadius => Mathf.Max(0f, Radius - RingWidth);
    }

    public static class FireHoopAreaTelegraphRules
    {
        public static FireHoopAreaTelegraphFrame Evaluate(float elapsedSeconds, float areaRadius, float hitIntervalSeconds, VfxVisualConfig config)
        {
            if (config == null || !config.fireHoopAreaTelegraphEnabled || areaRadius <= 0f)
            {
                return Invisible(config);
            }

            float interval = Mathf.Max(0.01f, hitIntervalSeconds);
            float normalizedPulseAge = Mathf.Clamp01(Mathf.Repeat(Mathf.Max(0f, elapsedSeconds), interval) / interval);
            float pulse = 1f - Mathf.Clamp01(GameEasing.Evaluate01(config.fireHoopAreaTelegraphPulseEase, normalizedPulseAge));

            Color fillColor = config.fireHoopAreaTelegraphFillColor;
            Color ringColor = config.fireHoopAreaTelegraphRingColor;
            float alphaBoost = Mathf.Max(0f, config.fireHoopAreaTelegraphPulseAlphaBoost) * pulse;
            fillColor.a = Mathf.Clamp01(fillColor.a + alphaBoost * 0.35f);
            ringColor.a = Mathf.Clamp01(ringColor.a + alphaBoost);

            float ringWidth = Mathf.Max(0.01f, config.fireHoopAreaTelegraphRingWidth)
                * (1f + Mathf.Max(0f, config.fireHoopAreaTelegraphPulseRingWidthBoost) * pulse);

            return new FireHoopAreaTelegraphFrame(
                true,
                areaRadius,
                ringWidth,
                fillColor,
                ringColor,
                config.fireHoopAreaTelegraphGroundClearance,
                config.fireHoopAreaTelegraphProbeHeight,
                config.fireHoopAreaTelegraphProbeDepth,
                config.fireHoopAreaTelegraphSampleCount,
                config.fireHoopAreaTelegraphRefreshSeconds);
        }

        private static FireHoopAreaTelegraphFrame Invisible(VfxVisualConfig config)
        {
            Color fillColor = config != null ? config.fireHoopAreaTelegraphFillColor : Color.clear;
            Color ringColor = config != null ? config.fireHoopAreaTelegraphRingColor : Color.clear;
            fillColor.a = 0f;
            ringColor.a = 0f;
            return new FireHoopAreaTelegraphFrame(false, 0f, 0f, fillColor, ringColor, 0f, 1f, 1f, 8, 0.1f);
        }
    }
}
