using NUnit.Framework;
using TheCircussyOne.Rules;

public sealed class DifficultyRulesTests
{
    [Test]
    public void SpawnIntervalRampsTowardPeakWithoutGoingLower()
    {
        float early = DifficultyRules.SpawnInterval(1.25f, 0.22f, 0f, 300f);
        float mid = DifficultyRules.SpawnInterval(1.25f, 0.22f, 150f, 300f);
        float late = DifficultyRules.SpawnInterval(1.25f, 0.22f, 999f, 300f);

        Assert.That(early, Is.EqualTo(1.25f).Within(0.001f));
        Assert.That(mid, Is.LessThan(early));
        Assert.That(late, Is.EqualTo(0.22f).Within(0.001f));
    }

    [Test]
    public void SpawnIntervalUsesConfigurableRampEase()
    {
        float linear = DifficultyRules.SpawnInterval(1.25f, 0.22f, 150f, 300f, EaseSettings.Linear);
        float outCubic = DifficultyRules.SpawnInterval(1.25f, 0.22f, 150f, 300f, EaseSettings.OutCubic);

        Assert.That(outCubic, Is.LessThan(linear));
    }

    [Test]
    public void MaxEnemiesClimbsFromStartToPeak()
    {
        Assert.That(DifficultyRules.MaxEnemies(30, 170, 0f, 300f), Is.EqualTo(30));
        Assert.That(DifficultyRules.MaxEnemies(30, 170, 150f, 300f), Is.GreaterThan(30));
        Assert.That(DifficultyRules.MaxEnemies(30, 170, 999f, 300f), Is.EqualTo(170));
    }

    [Test]
    public void EnemyStatsGrowWithElapsedTimeAndIgnorePlayerLevel()
    {
        int health = DifficultyRules.EnemyHealth(4, level: 7, elapsedSeconds: 180f, growthPerMinute: 2f);
        float speed = DifficultyRules.EnemyMoveSpeed(2f, level: 4, elapsedSeconds: 120f, growthPerMinute: 0.24f);
        int sameHealthAtDifferentLevel = DifficultyRules.EnemyHealth(4, level: 30, elapsedSeconds: 180f, growthPerMinute: 2f);
        float sameSpeedAtDifferentLevel = DifficultyRules.EnemyMoveSpeed(2f, level: 30, elapsedSeconds: 120f, growthPerMinute: 0.24f);

        Assert.That(health, Is.EqualTo(10));
        Assert.That(speed, Is.EqualTo(2.48f).Within(0.001f));
        Assert.That(sameHealthAtDifferentLevel, Is.EqualTo(health));
        Assert.That(sameSpeedAtDifferentLevel, Is.EqualTo(speed).Within(0.001f));
    }
}
