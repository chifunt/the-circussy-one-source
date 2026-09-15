namespace TheCircussyOne.Rules
{
    public static class SpawnTimerRules
    {
        public static float Tick(float remaining, float deltaTime)
        {
            return remaining - deltaTime;
        }

        public static bool CanSpawn(float remaining)
        {
            return remaining <= 0f;
        }
    }
}
