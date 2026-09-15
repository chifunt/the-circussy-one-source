using NUnit.Framework;
using TheCircussyOne.Rules;

public sealed class EnemySpawnRulesTests
{
    [Test]
    public void AllowsSpawnBelowMaxWhileRunIsActive()
    {
        Assert.That(EnemySpawnRules.CanSpawn(isGameOver: false, currentEnemies: 9, maxEnemies: 10), Is.True);
    }

    [Test]
    public void BlocksSpawnWhenGameOver()
    {
        Assert.That(EnemySpawnRules.CanSpawn(isGameOver: true, currentEnemies: 0, maxEnemies: 10), Is.False);
    }

    [Test]
    public void BlocksSpawnAtMaxEnemies()
    {
        Assert.That(EnemySpawnRules.CanSpawn(isGameOver: false, currentEnemies: 10, maxEnemies: 10), Is.False);
    }
}
