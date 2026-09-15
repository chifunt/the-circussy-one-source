using System;
using System.Collections.Generic;
using TheCircussyOne.Runtime;
using UnityEngine;

namespace TheCircussyOne.Rules
{
    public static class RunActScheduleRules
    {
        public static int ResolveActIndex(int worldIndex, int actCount)
        {
            if (actCount <= 0)
            {
                return -1;
            }

            return Mathf.Clamp(worldIndex - 1, 0, actCount - 1);
        }

        public static float Elapsed(float currentElapsed, float deltaTime)
        {
            return Mathf.Max(0f, currentElapsed) + Mathf.Max(0f, deltaTime);
        }

        public static float Remaining(float durationSeconds, float elapsedSeconds)
        {
            return Mathf.Max(0f, Mathf.Max(0f, durationSeconds) - Mathf.Max(0f, elapsedSeconds));
        }

        public static bool IsShowtimeDue(float previousElapsedSeconds, float currentElapsedSeconds, float showtimeSeconds)
        {
            return showtimeSeconds > 0f
                && previousElapsedSeconds < showtimeSeconds
                && currentElapsedSeconds >= showtimeSeconds;
        }

        public static bool IsFinaleDue(float previousElapsedSeconds, float currentElapsedSeconds, float durationSeconds)
        {
            return durationSeconds > 0f
                && previousElapsedSeconds < durationSeconds
                && currentElapsedSeconds >= durationSeconds;
        }

        public static float PressureMultiplier(float fallbackNeutral, bool isActive, float multiplier)
        {
            if (!isActive)
            {
                return fallbackNeutral;
            }

            return multiplier > 0f ? multiplier : fallbackNeutral;
        }

        public static int PressureStep(float elapsedSeconds, float intervalSeconds)
        {
            if (elapsedSeconds <= 0f || intervalSeconds <= 0f)
            {
                return 0;
            }

            return Mathf.FloorToInt(elapsedSeconds / intervalSeconds);
        }

        public static float RepeatedPressureMultiplier(float fallbackNeutral, bool isActive, float multiplierPerStep, int steps)
        {
            if (!isActive || steps <= 0)
            {
                return fallbackNeutral;
            }

            float multiplier = multiplierPerStep > 0f ? multiplierPerStep : fallbackNeutral;
            return Mathf.Pow(multiplier, steps);
        }

        public static string ActRomanLabel(int actNumber)
        {
            return actNumber switch
            {
                1 => "ACT I",
                2 => "ACT II",
                3 => "ACT III",
                _ => $"ACT {Mathf.Max(1, actNumber)}"
            };
        }

        public static string FormatCountdown(float seconds)
        {
            int wholeSeconds = Mathf.Max(0, Mathf.CeilToInt(seconds));
            int minutes = wholeSeconds / 60;
            int remainder = wholeSeconds % 60;
            return $"{minutes:00}:{remainder:00}";
        }

        public static string FormatElapsed(float seconds)
        {
            int wholeSeconds = Mathf.Max(0, Mathf.FloorToInt(seconds));
            int minutes = wholeSeconds / 60;
            int remainder = wholeSeconds % 60;
            return $"{minutes:00}:{remainder:00}";
        }

        public static string FormatActTimer(RunActScheduleState schedule)
        {
            if (schedule == null || !schedule.HasActiveAct)
            {
                return string.Empty;
            }

            return FormatCountdown(schedule.ActRemainingSeconds);
        }

        public static string FormatEncoreTimer(RunActScheduleState schedule)
        {
            if (schedule == null || !schedule.IsEncoreActive)
            {
                return string.Empty;
            }

            if (schedule.IsEncoreGraceActive)
            {
                return $"ENCORE IN {Mathf.Max(0, Mathf.CeilToInt(schedule.EncoreGraceRemainingSeconds))}";
            }

            return FormatElapsed(schedule.EncorePressureElapsedSeconds);
        }

        public static string FormatSecondsUntil(float seconds)
        {
            if (seconds < 0f || float.IsNaN(seconds) || float.IsInfinity(seconds))
            {
                return "--:--";
            }

            return FormatCountdown(seconds);
        }

        public static string PickSubtitleVariant(IReadOnlyList<string> variants, string fallback, int seed)
        {
            int validCount = CountValidVariants(variants);
            if (validCount <= 0)
            {
                return string.IsNullOrWhiteSpace(fallback) ? string.Empty : fallback.Trim();
            }

            int targetIndex = (int)((uint)DeterministicSeed.Mix(seed) % validCount);
            int currentValidIndex = 0;
            for (int i = 0; i < variants.Count; i++)
            {
                string value = variants[i];
                if (string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                if (currentValidIndex == targetIndex)
                {
                    return value.Trim();
                }

                currentValidIndex++;
            }

            return string.IsNullOrWhiteSpace(fallback) ? string.Empty : fallback.Trim();
        }

        private static int CountValidVariants(IReadOnlyList<string> variants)
        {
            if (variants == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < variants.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(variants[i]))
                {
                    count++;
                }
            }

            return count;
        }
    }
}
