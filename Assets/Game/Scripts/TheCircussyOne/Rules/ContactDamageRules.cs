namespace TheCircussyOne.Rules
{
    public static class ContactDamageRules
    {
        public static bool CanDamage(float distance, float range, float currentTime, float nextAllowedTime)
        {
            return distance <= range && currentTime >= nextAllowedTime;
        }

        public static bool CanDamage(bool isOverlapping, float currentTime, float nextAllowedTime)
        {
            return isOverlapping && currentTime >= nextAllowedTime;
        }

        public static float NextAllowedTime(float currentTime, float cooldown)
        {
            return currentTime + cooldown;
        }
    }
}
