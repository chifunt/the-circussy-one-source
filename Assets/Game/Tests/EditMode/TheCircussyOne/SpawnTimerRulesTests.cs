using NUnit.Framework;
using TheCircussyOne.Rules;

public sealed class SpawnTimerRulesTests
{
    [Test]
    public void TickAllowsSpawnAtOrBelowZero()
    {
        float remaining = SpawnTimerRules.Tick(0.2f, 0.25f);

        Assert.That(SpawnTimerRules.CanSpawn(remaining), Is.True);
    }
}
