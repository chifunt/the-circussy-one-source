using UnityEngine;
using TheCircussyOne.Config;

namespace TheCircussyOne.Rules
{
    public readonly struct ProjectileExplosionVisualFrame
    {
        public ProjectileExplosionVisualFrame(bool visible, float radius, Color ringColor, Color sphereColor)
        {
            Visible = visible;
            Radius = radius;
            RingColor = ringColor;
            SphereColor = sphereColor;
        }

        public bool Visible { get; }
        public float Radius { get; }
        public Color RingColor { get; }
        public Color SphereColor { get; }
    }

    public static class ProjectileExplosionVisualRules
    {
        public static ProjectileExplosionVisualFrame Evaluate(float elapsedSeconds, float splashRadius, VfxVisualConfig config)
        {
            if (config == null || !config.projectileExplosionOverlayEnabled || splashRadius <= 0f)
            {
                return Invisible(config);
            }

            float duration = Duration(config);
            if (duration <= 0f || elapsedSeconds >= duration)
            {
                return Invisible(config);
            }

            float t = Mathf.Clamp01(elapsedSeconds / duration);
            float startFraction = Mathf.Clamp01(config.projectileExplosionOverlayStartRadiusFraction);
            float expandT = Mathf.Clamp01(GameEasing.Evaluate01(config.projectileExplosionOverlayExpandEase, t));
            float radius = Mathf.Lerp(splashRadius * startFraction, splashRadius, expandT);
            radius = Mathf.Clamp(radius, 0f, splashRadius);

            float fadeT = Mathf.Clamp01(GameEasing.Evaluate01(config.projectileExplosionOverlayFadeEase, t));
            Color ringColor = config.projectileExplosionOverlayRingColor;
            Color sphereColor = config.projectileExplosionOverlaySphereColor;
            ringColor.a *= 1f - fadeT;
            sphereColor.a *= 1f - fadeT;

            return new ProjectileExplosionVisualFrame(true, radius, ringColor, sphereColor);
        }

        public static float Duration(VfxVisualConfig config)
        {
            return config != null ? Mathf.Max(0f, config.projectileExplosionOverlayDuration) : 0f;
        }

        public static float Lifetime(VfxVisualConfig config)
        {
            if (config == null)
            {
                return 0f;
            }

            float overlayDuration = config.projectileExplosionOverlayEnabled ? Duration(config) : 0f;
            float particleLifetime = config.projectileExplosion != null && config.projectileExplosion.enabled
                ? config.projectileExplosion.duration + config.projectileExplosion.lifetime
                : 0f;
            return Mathf.Max(overlayDuration, particleLifetime);
        }

        private static ProjectileExplosionVisualFrame Invisible(VfxVisualConfig config)
        {
            Color ringColor = config != null ? config.projectileExplosionOverlayRingColor : Color.clear;
            Color sphereColor = config != null ? config.projectileExplosionOverlaySphereColor : Color.clear;
            ringColor.a = 0f;
            sphereColor.a = 0f;
            return new ProjectileExplosionVisualFrame(false, 0f, ringColor, sphereColor);
        }
    }
}
