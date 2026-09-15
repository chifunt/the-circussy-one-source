namespace TheCircussyOne.Rules
{
    public static class EnemySpawnRules
    {
        public static bool CanSpawn(bool isGameOver, int currentEnemies, int maxEnemies)
        {
            return !isGameOver && currentEnemies < maxEnemies;
        }
    }
}
