using System;

namespace TheCircussyOne.Rules
{
    public static class DifficultyRules
    {
        public static float Progress01(float elapsedSeconds, float rampDurationSeconds)
        {
            if (rampDurationSeconds <= 0f)
            {
                return 1f;
            }

            return Clamp01(elapsedSeconds / rampDurationSeconds);
        }

        public static float SpawnInterval(float startInterval, float peakInterval, float elapsedSeconds, float rampDurationSeconds)
        {
            return SpawnInterval(startInterval, peakInterval, elapsedSeconds, rampDurationSeconds, EaseSettings.OutCubic);
        }

        public static float SpawnInterval(float startInterval, float peakInterval, float elapsedSeconds, float rampDurationSeconds, EaseSettings rampEase)
        {
            float t = GameEasing.Evaluate01(rampEase, Progress01(elapsedSeconds, rampDurationSeconds));
            float target = Math.Min(startInterval, peakInterval);
            return Lerp(startInterval, target, t);
        }

        public static int MaxEnemies(int startMax, int peakMax, float elapsedSeconds, float rampDurationSeconds)
        {
            float t = Progress01(elapsedSeconds, rampDurationSeconds);
            int max = (int)Math.Round(Lerp(startMax, Math.Max(startMax, peakMax), t));
            return Math.Max(1, max);
        }

        public static int EnemyHealth(int baseHealth, int level, float elapsedSeconds, float growthPerMinute)
        {
            // Level is accepted for existing call sites. Enemy scaling should come from
            // visible spawn/time/content tuning, not hidden player-level bonuses.
            _ = level;
            int timeBonus = (int)Math.Floor(Math.Max(0f, elapsedSeconds) / 60f * Math.Max(0f, growthPerMinute));
            return Math.Max(1, baseHealth + timeBonus);
        }

        public static float EnemyMoveSpeed(float baseSpeed, int level, float elapsedSeconds, float growthPerMinute)
        {
            _ = level;
            float timeBonus = Math.Max(0f, elapsedSeconds) / 60f * Math.Max(0f, growthPerMinute);
            return Math.Max(0.1f, baseSpeed + timeBonus);
        }

        private static float Lerp(float from, float to, float t)
        {
            return from + (to - from) * Clamp01(t);
        }

        private static float Clamp01(float value)
        {
            if (value <= 0f)
            {
                return 0f;
            }

            if (value >= 1f)
            {
                return 1f;
            }

            return value;
        }
    }
}
