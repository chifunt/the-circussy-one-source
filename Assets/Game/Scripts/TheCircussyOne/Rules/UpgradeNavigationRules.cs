using TheCircussyOne.Runtime;

namespace TheCircussyOne.Rules
{
    public struct UpgradeNavigationRepeatState
    {
        public UpgradeNavigationDirection HeldDirection;
        public float NextRepeatTime;
        public bool RequiresNeutralRelease;
    }

    public static class UpgradeNavigationRules
    {
        public const float InitialRepeatDelay = 0.28f;
        public const float RepeatInterval = 0.16f;

        public static int IndexForDirection(UpgradeNavigationDirection direction)
        {
            return direction switch
            {
                UpgradeNavigationDirection.Left => 0,
                UpgradeNavigationDirection.Middle => 1,
                UpgradeNavigationDirection.Right => 2,
                _ => -1
            };
        }

        public static int NextIndex(
            int currentIndex,
            int choiceCount,
            UpgradeNavigationDirection direction)
        {
            if (choiceCount <= 0 || direction == UpgradeNavigationDirection.None)
            {
                return -1;
            }

            if (currentIndex < 0)
            {
                return IndexForDirection(direction);
            }

            if (direction == UpgradeNavigationDirection.Middle)
            {
                return currentIndex;
            }

            int offset = direction == UpgradeNavigationDirection.Left ? -1 : 1;
            return (currentIndex + offset + choiceCount) % choiceCount;
        }

        public static bool ShouldTrigger(
            ref UpgradeNavigationRepeatState state,
            UpgradeNavigationDirection direction,
            float now,
            float initialDelay = InitialRepeatDelay,
            float repeatInterval = RepeatInterval)
        {
            if (state.RequiresNeutralRelease)
            {
                if (direction != UpgradeNavigationDirection.None)
                {
                    return false;
                }

                state.RequiresNeutralRelease = false;
                state.HeldDirection = UpgradeNavigationDirection.None;
                state.NextRepeatTime = 0f;
                return false;
            }

            if (direction == UpgradeNavigationDirection.None)
            {
                state.HeldDirection = UpgradeNavigationDirection.None;
                state.NextRepeatTime = 0f;
                return false;
            }

            if (state.HeldDirection != direction)
            {
                state.HeldDirection = direction;
                state.NextRepeatTime = now + initialDelay;
                return true;
            }

            if (now < state.NextRepeatTime)
            {
                return false;
            }

            state.NextRepeatTime = now + repeatInterval;
            return true;
        }
    }
}
