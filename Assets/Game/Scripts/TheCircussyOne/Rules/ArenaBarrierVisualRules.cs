using TheCircussyOne.Config;
using UnityEngine;

namespace TheCircussyOne.Rules
{
    public readonly struct ArenaBarrierCollisionSpec
    {
        public ArenaBarrierCollisionSpec(
            float innerRadius,
            float outerRadius,
            float bottomY,
            float topY,
            int segmentCount)
        {
            InnerRadius = Mathf.Max(0.1f, innerRadius);
            OuterRadius = Mathf.Max(InnerRadius + 0.01f, outerRadius);
            BottomY = Mathf.Min(bottomY, topY - 0.01f);
            TopY = Mathf.Max(topY, BottomY + 0.01f);
            SegmentCount = Mathf.Max(12, segmentCount);
        }

        public float InnerRadius { get; }
        public float OuterRadius { get; }
        public float BottomY { get; }
        public float TopY { get; }
        public int SegmentCount { get; }
        public float Height => TopY - BottomY;
        public float Thickness => OuterRadius - InnerRadius;
        public bool IsValid => InnerRadius > 0f && OuterRadius > InnerRadius && TopY > BottomY && SegmentCount >= 12;
    }

    public readonly struct ArenaBarrierDynamicRingFrame
    {
        public ArenaBarrierDynamicRingFrame(float height, Color color)
        {
            Height = Mathf.Max(0f, height);
            Color = color;
        }

        public float Height { get; }
        public Color Color { get; }
    }

    public static class ArenaBarrierVisualRules
    {
        public static float Radius(float baseRadius, RunWorldGenerationConfig config)
        {
            float padding = config != null ? config.arenaBarrierRadiusPadding : 0f;
            return Mathf.Max(0.1f, baseRadius + Mathf.Max(0f, padding));
        }

        public static float Height(RunWorldGenerationConfig config)
        {
            return Mathf.Max(0.1f, config != null ? config.arenaBarrierHeight : 22f);
        }

        public static ArenaBarrierDynamicRingFrame DynamicRingFrame(
            int ringIndex,
            int ringCount,
            float time,
            RunWorldGenerationConfig config)
        {
            float height = Height(config);
            int count = Mathf.Max(1, ringCount);
            float speed = Mathf.Max(0f, config != null ? config.arenaBarrierDynamicRingSpeed : 3.4f);
            float offset = height * Mathf.Repeat(ringIndex / (float)count, 1f);
            float y = Mathf.Repeat(offset + Mathf.Max(0f, time) * speed, height);
            float normalized = height <= 0f ? 0f : Mathf.Clamp01(y / height);
            EaseSettings fadeEase = config != null ? config.arenaBarrierDynamicRingFadeEase : EaseSettings.OutCubic;
            float alphaScale = 1f - GameEasing.Evaluate01(fadeEase.shape > 0f ? fadeEase : EaseSettings.OutCubic, normalized);
            Color color = config != null ? config.arenaBarrierDynamicRingColor : new Color(0.62f, 0.92f, 1f, 0.32f);
            color.a *= Mathf.Clamp01(alphaScale);
            return new ArenaBarrierDynamicRingFrame(y, color);
        }

        public static bool ShouldShowDynamicRing(RunWorldGenerationConfig config, ArenaBarrierDynamicRingFrame frame)
        {
            return config != null
                && config.arenaBarrierEnabled
                && config.arenaBarrierDynamicRingsEnabled
                && config.arenaBarrierDynamicRingSpeed > 0f
                && frame.Color.a > 0.001f;
        }
    }

    public static class ArenaBarrierCollisionRules
    {
        public static bool ShouldCreateCollision(RunWorldGenerationConfig config)
        {
            return config != null
                && config.arenaBarrierEnabled
                && config.arenaBarrierCollisionEnabled;
        }

        public static ArenaBarrierCollisionSpec BuildSpec(
            float visualRadius,
            float generatedMaxHeight,
            RunWorldGenerationConfig config)
        {
            float innerRadius = Mathf.Max(0.1f, visualRadius);
            float thickness = config != null ? config.ArenaBarrierCollisionThickness : 4f;
            float bottomY = -(config != null ? config.ArenaBarrierCollisionDepthBelowGround : 64f);
            float topY = Mathf.Max(
                config != null ? config.ArenaBarrierCollisionMinimumTop : 128f,
                Mathf.Max(0f, generatedMaxHeight) + (config != null ? config.ArenaBarrierCollisionTopPadding : 64f),
                ArenaBarrierVisualRules.Height(config));
            int segmentCount = config != null ? config.ArenaBarrierSegmentCount : 96;
            return new ArenaBarrierCollisionSpec(
                innerRadius,
                innerRadius + Mathf.Max(0.01f, thickness),
                bottomY,
                topY,
                segmentCount);
        }
    }
}
