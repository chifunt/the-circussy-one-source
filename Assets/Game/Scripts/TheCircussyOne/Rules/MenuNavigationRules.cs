using UnityEngine;

namespace TheCircussyOne.Rules
{
    public struct MenuNavigationRepeatState
    {
        public int HeldDirection;
        public float NextRepeatTime;
        public bool RequiresNeutralRelease;
    }

    public static class MenuNavigationRules
    {
        public const float DefaultStickThreshold = 0.7f;
        public const float InitialRepeatDelay = 0.34f;
        public const float RepeatInterval = 0.24f;

        public static int DirectionFromAxis(float axis, float threshold = DefaultStickThreshold)
        {
            float safeThreshold = Mathf.Clamp01(threshold);
            if (axis > safeThreshold)
            {
                return 1;
            }

            return axis < -safeThreshold ? -1 : 0;
        }

        public static bool ShouldTrigger(
            ref MenuNavigationRepeatState state,
            int direction,
            float now,
            float initialDelay = InitialRepeatDelay,
            float repeatInterval = RepeatInterval)
        {
            int normalizedDirection = NormalizeDirection(direction);
            if (normalizedDirection == 0)
            {
                Reset(ref state);
                return false;
            }

            if (state.RequiresNeutralRelease)
            {
                return false;
            }

            if (state.HeldDirection != normalizedDirection)
            {
                state.HeldDirection = normalizedDirection;
                state.NextRepeatTime = now + Mathf.Max(0f, initialDelay);
                return true;
            }

            if (now < state.NextRepeatTime)
            {
                return false;
            }

            state.NextRepeatTime = now + Mathf.Max(0f, repeatInterval);
            return true;
        }

        public static void Reset(ref MenuNavigationRepeatState state)
        {
            state.HeldDirection = 0;
            state.NextRepeatTime = 0f;
            state.RequiresNeutralRelease = false;
        }

        public static void RequireNeutralRelease(ref MenuNavigationRepeatState state)
        {
            state.HeldDirection = 0;
            state.NextRepeatTime = 0f;
            state.RequiresNeutralRelease = true;
        }

        public static int NextClampedIndex(int currentIndex, int count, int direction)
        {
            if (count <= 0)
            {
                return -1;
            }

            int normalizedDirection = NormalizeDirection(direction);
            if (normalizedDirection == 0)
            {
                return Mathf.Clamp(currentIndex, 0, count - 1);
            }

            int safeCurrentIndex = currentIndex < 0 ? 0 : Mathf.Clamp(currentIndex, 0, count - 1);
            return Mathf.Clamp(safeCurrentIndex + normalizedDirection, 0, count - 1);
        }

        private static int NormalizeDirection(int direction)
        {
            if (direction > 0)
            {
                return 1;
            }

            return direction < 0 ? -1 : 0;
        }
    }
}
