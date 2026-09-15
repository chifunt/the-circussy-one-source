using System;
using TheCircussyOne.Content;

namespace TheCircussyOne.Rules
{
    public static class WeaponCooldownRules
    {
        public const float MinimumFireInterval = 0.12f;

        public static float FireInterval(float baseInterval)
        {
            return FireInterval(baseInterval, MinimumFireInterval);
        }

        public static float FireInterval(float baseInterval, float minimumInterval)
        {
            return Math.Max(minimumInterval, baseInterval);
        }

        public static float FireInterval(WeaponDefinition weapon)
        {
            if (weapon == null)
            {
                return MinimumFireInterval;
            }

            return FireInterval(
                weapon.baseFireIntervalSeconds,
                weapon.minimumFireIntervalSeconds);
        }

        public static float Tick(float remaining, float deltaTime)
        {
            return Math.Max(0f, remaining - deltaTime);
        }

        public static bool CanFire(float remaining)
        {
            return remaining <= 0f;
        }
    }
}
