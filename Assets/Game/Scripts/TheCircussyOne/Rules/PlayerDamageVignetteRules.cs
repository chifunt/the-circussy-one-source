using UnityEngine;
using TheCircussyOne.Config;

namespace TheCircussyOne.Rules
{
    public readonly struct PlayerDamageVignetteFrame
    {
        public PlayerDamageVignetteFrame(
            bool visible,
            float intensity,
            Color tint,
            Color darkTint,
            float maxOpacity,
            float darkMaxOpacity,
            float grainStrength,
            float splotchStrength,
            float radius,
            float softness,
            float time)
        {
            Visible = visible;
            Intensity = intensity;
            Tint = tint;
            DarkTint = darkTint;
            MaxOpacity = maxOpacity;
            DarkMaxOpacity = darkMaxOpacity;
            GrainStrength = grainStrength;
            SplotchStrength = splotchStrength;
            Radius = radius;
            Softness = softness;
            Time = time;
        }

        public bool Visible { get; }
        public float Intensity { get; }
        public Color Tint { get; }
        public Color DarkTint { get; }
        public float MaxOpacity { get; }
        public float DarkMaxOpacity { get; }
        public float GrainStrength { get; }
        public float SplotchStrength { get; }
        public float Radius { get; }
        public float Softness { get; }
        public float Time { get; }

        public static PlayerDamageVignetteFrame Hidden => new(false, 0f, Color.clear, Color.black, 0f, 0f, 0f, 0f, 0.78f, 0.28f, 0f);
    }

    public static class PlayerDamageVignetteRules
    {
        public static float DamageIntensity(int finalDamage, int maxHealth, float damageToIntensityScale)
        {
            if (finalDamage <= 0 || maxHealth <= 0 || damageToIntensityScale <= 0f)
            {
                return 0f;
            }

            return Mathf.Max(0f, (float)finalDamage / maxHealth * damageToIntensityScale);
        }

        public static float HealingIntensity(int healedAmount, int maxHealth, float healingToIntensityScale)
        {
            if (healedAmount <= 0 || maxHealth <= 0 || healingToIntensityScale <= 0f)
            {
                return 0f;
            }

            return Mathf.Max(0f, (float)healedAmount / maxHealth * healingToIntensityScale);
        }

        public static float Accumulate(float currentIntensity, float addedIntensity, float accumulationCap)
        {
            return Mathf.Clamp(currentIntensity + Mathf.Max(0f, addedIntensity), 0f, Mathf.Max(0f, accumulationCap));
        }

        public static float Fade(float startIntensity, float elapsedSeconds, float durationSeconds, EaseSettings fadeEase)
        {
            if (startIntensity <= 0f)
            {
                return 0f;
            }

            if (durationSeconds <= 0f)
            {
                return 0f;
            }

            float normalized = Mathf.Clamp01(elapsedSeconds / durationSeconds);
            float eased = GameEasing.Evaluate01(fadeEase.shape > 0f ? fadeEase : EaseSettings.OutCubic, normalized);
            return Mathf.Max(0f, startIntensity * (1f - eased));
        }

        public static PlayerDamageVignetteFrame Evaluate(float startIntensity, float elapsedSeconds, DamageFeedbackVisualConfig config, float time)
        {
            if (config == null || !config.playerDamageVignetteEnabled)
            {
                return PlayerDamageVignetteFrame.Hidden;
            }

            float intensity = Fade(startIntensity, elapsedSeconds, config.playerDamageVignetteFlashSeconds, config.playerDamageVignetteFadeEase);
            if (intensity <= 0.001f)
            {
                return PlayerDamageVignetteFrame.Hidden;
            }

            return new PlayerDamageVignetteFrame(
                true,
                Mathf.Min(intensity, Mathf.Max(0f, config.playerDamageVignetteAccumulationCap)),
                config.playerDamageVignetteColor,
                config.playerDamageVignetteDarkColor,
                Mathf.Clamp01(config.playerDamageVignetteMaxOpacity),
                Mathf.Clamp01(config.playerDamageVignetteDarkMaxOpacity),
                Mathf.Clamp01(config.playerDamageVignetteGrainStrength),
                Mathf.Clamp01(config.playerDamageVignetteSplotchStrength),
                Mathf.Clamp01(config.playerDamageVignetteRadius),
                Mathf.Clamp01(config.playerDamageVignetteSoftness),
                time);
        }

        public static PlayerDamageVignetteFrame EvaluateHealing(float startIntensity, float elapsedSeconds, DamageFeedbackVisualConfig config, float time)
        {
            if (config == null || !config.playerHealingVignetteEnabled)
            {
                return PlayerDamageVignetteFrame.Hidden;
            }

            float intensity = Fade(startIntensity, elapsedSeconds, config.playerHealingVignetteFlashSeconds, config.playerHealingVignetteFadeEase);
            if (intensity <= 0.001f)
            {
                return PlayerDamageVignetteFrame.Hidden;
            }

            return new PlayerDamageVignetteFrame(
                true,
                Mathf.Min(intensity, Mathf.Max(0f, config.playerHealingVignetteAccumulationCap)),
                config.playerHealingVignetteColor,
                config.playerHealingVignetteDarkColor,
                Mathf.Clamp01(config.playerHealingVignetteMaxOpacity),
                Mathf.Clamp01(config.playerHealingVignetteDarkMaxOpacity),
                Mathf.Clamp01(config.playerHealingVignetteGrainStrength),
                Mathf.Clamp01(config.playerHealingVignetteSplotchStrength),
                Mathf.Clamp01(config.playerHealingVignetteRadius),
                Mathf.Clamp01(config.playerHealingVignetteSoftness),
                time);
        }
    }
}
