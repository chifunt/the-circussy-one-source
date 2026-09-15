using System;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;
using TheCircussyOne.Config;

namespace TheCircussyOne.Rules
{
    public enum EasePreset
    {
        Linear,
        InQuad,
        OutQuad,
        InCubic,
        OutCubic,
        InOutSine,
        InBack,
        OutBack,
        Exponential,
        OutBounce
    }

    [Serializable]
    [InlineProperty]
    public struct EaseSettings
    {
        [VerticalGroup("Ease")]
        [LabelText("Preset")]
        public EasePreset preset;

        [VerticalGroup("Ease")]
        [LabelText("Shape")]
        [NumericSlider(0.25f, 3f)]
        [SuffixLabel("shape")]
        public float shape;

        public EaseSettings(EasePreset preset, float shape = 1f)
        {
            this.preset = preset;
            this.shape = shape <= 0f ? 1f : shape;
        }

        public static EaseSettings Linear => new(EasePreset.Linear);
        public static EaseSettings InQuad => new(EasePreset.InQuad);
        public static EaseSettings OutQuad => new(EasePreset.OutQuad);
        public static EaseSettings InCubic => new(EasePreset.InCubic);
        public static EaseSettings OutCubic => new(EasePreset.OutCubic);
        public static EaseSettings InOutSine => new(EasePreset.InOutSine);
        public static EaseSettings InBack => new(EasePreset.InBack);
        public static EaseSettings OutBack => new(EasePreset.OutBack);
        public static EaseSettings Exponential => new(EasePreset.Exponential);
        public static EaseSettings OutBounce => new(EasePreset.OutBounce);
    }

    public static class GameEasing
    {
        private const float DefaultBackOvershoot = 1.70158f;

        public static float Evaluate01(EaseSettings settings, float t)
        {
            t = Mathf.Clamp01(t);
            float shape = Mathf.Max(0.25f, settings.shape <= 0f ? 1f : settings.shape);

            return settings.preset switch
            {
                EasePreset.InQuad => Mathf.Pow(t, 2f * shape),
                EasePreset.OutQuad => 1f - Mathf.Pow(1f - t, 2f * shape),
                EasePreset.InCubic => Mathf.Pow(t, 3f * shape),
                EasePreset.OutCubic => 1f - Mathf.Pow(1f - t, 3f * shape),
                EasePreset.InOutSine => ShapedSymmetric(-(Mathf.Cos(Mathf.PI * t) - 1f) * 0.5f, shape),
                EasePreset.InBack => EaseInBack(t, DefaultBackOvershoot * shape),
                EasePreset.OutBack => EaseOutBack(t, DefaultBackOvershoot * shape),
                EasePreset.Exponential => Exponential01(t, shape),
                EasePreset.OutBounce => EaseOutBounce(t),
                _ => t
            };
        }

        public static float SmoothingWeight(float sharpness, float deltaTime, EaseSettings settings)
        {
            if (sharpness <= 0f || deltaTime <= 0f)
            {
                return 0f;
            }

            float shape = Mathf.Max(0.25f, settings.shape <= 0f ? 1f : settings.shape);
            float raw = 1f - Mathf.Exp(-sharpness * deltaTime);
            return settings.preset == EasePreset.Exponential
                ? 1f - Mathf.Exp(-sharpness * shape * deltaTime)
                : Evaluate01(settings, raw);
        }

        public static Ease ToPrimeTweenEase(EaseSettings settings)
        {
            return settings.preset switch
            {
                EasePreset.InQuad => Ease.InQuad,
                EasePreset.OutQuad => Ease.OutQuad,
                EasePreset.InCubic => Ease.InCubic,
                EasePreset.OutCubic => Ease.OutCubic,
                EasePreset.InOutSine => Ease.InOutSine,
                EasePreset.InBack => Ease.InBack,
                EasePreset.OutBack => Ease.OutBack,
                EasePreset.OutBounce => Ease.OutBounce,
                _ => Ease.Linear
            };
        }

        private static float ShapedSymmetric(float value, float shape)
        {
            if (Mathf.Approximately(shape, 1f))
            {
                return value;
            }

            return value < 0.5f
                ? Mathf.Pow(value * 2f, shape) * 0.5f
                : 1f - Mathf.Pow((1f - value) * 2f, shape) * 0.5f;
        }

        private static float Exponential01(float t, float shape)
        {
            float exponent = Mathf.Max(0.01f, shape) * 6f;
            float denominator = 1f - Mathf.Exp(-exponent);
            return denominator <= 0f ? t : (1f - Mathf.Exp(-exponent * t)) / denominator;
        }

        private static float EaseInBack(float t, float overshoot)
        {
            return (overshoot + 1f) * t * t * t - overshoot * t * t;
        }

        private static float EaseOutBack(float t, float overshoot)
        {
            float shifted = t - 1f;
            return 1f + (overshoot + 1f) * shifted * shifted * shifted + overshoot * shifted * shifted;
        }

        private static float EaseOutBounce(float t)
        {
            const float n1 = 7.5625f;
            const float d1 = 2.75f;

            if (t < 1f / d1)
            {
                return n1 * t * t;
            }

            if (t < 2f / d1)
            {
                t -= 1.5f / d1;
                return n1 * t * t + 0.75f;
            }

            if (t < 2.5f / d1)
            {
                t -= 2.25f / d1;
                return n1 * t * t + 0.9375f;
            }

            t -= 2.625f / d1;
            return n1 * t * t + 0.984375f;
        }
    }
}
