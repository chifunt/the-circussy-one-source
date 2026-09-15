using TheCircussyOne.Config;
using UnityEngine;

namespace TheCircussyOne.Rules
{
    public enum RunTransitionPhase
    {
        Hidden,
        FadingOut,
        Holding,
        FadingIn
    }

    public static class RunTransitionRules
    {
        public static float OverlayOpacity(RunTransitionPhase phase, float elapsedSeconds, HudTransitionSettings settings)
        {
            settings.EnsureDefaults();
            return phase switch
            {
                RunTransitionPhase.FadingOut => Fade01(elapsedSeconds, settings.fadeOutSeconds, settings.fadeEase),
                RunTransitionPhase.Holding => 1f,
                RunTransitionPhase.FadingIn => 1f - Fade01(elapsedSeconds, settings.fadeInSeconds, settings.fadeEase),
                _ => 0f
            };
        }

        public static bool MinimumVisibleSatisfied(float visibleElapsedSeconds, HudTransitionSettings settings)
        {
            settings.EnsureDefaults();
            return visibleElapsedSeconds >= Mathf.Max(0f, settings.minimumVisibleSeconds);
        }

        public static Color BackdropColor(HudTransitionSettings settings)
        {
            settings.EnsureDefaults();
            return Color.black;
        }

        public static float BulbBrightness(int bulbIndex, float elapsedSeconds, HudTransitionSettings settings)
        {
            settings.EnsureDefaults();
            int count = Mathf.Max(3, settings.bulbCount);
            if (bulbIndex < 0 || bulbIndex >= count)
            {
                return 0f;
            }

            float chase = Mathf.Max(0f, settings.bulbChaseSpeed);
            int head = chase <= 0f ? 0 : Mathf.FloorToInt(elapsedSeconds * chase * count) % count;
            int distance = Mathf.Abs(bulbIndex - head);
            distance = Mathf.Min(distance, count - distance);
            return distance switch
            {
                0 => 1f,
                1 => 0.62f,
                2 => 0.34f,
                _ => 0f
            };
        }

        private static float Fade01(float elapsedSeconds, float seconds, EaseSettings ease)
        {
            float duration = Mathf.Max(0.01f, seconds);
            float t = Mathf.Clamp01(Mathf.Max(0f, elapsedSeconds) / duration);
            return Mathf.Clamp01(GameEasing.Evaluate01(ease, t));
        }
    }
}
