using NUnit.Framework;
using UnityEngine;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;

public sealed class EnemyTargetingTests
{
    private GameConfig _config;

    [SetUp]
    public void SetUp()
    {
        TheCircussyOneTestObjects.DestroyAll();
        _config = TheCircussyOneTestObjects.CreateConfig();
    }

    [TearDown]
    public void TearDown()
    {
        TheCircussyOneTestObjects.DestroyAll();
        TheCircussyOneTestObjects.Destroy(_config);
    }

    [Test]
    public void SelectsNearestAliveEnemyInRange()
    {
        var far = TheCircussyOneTestObjects.CreateEnemy(_config, new Vector3(6f, 0f, 0f), "Far Enemy");
        var near = TheCircussyOneTestObjects.CreateEnemy(_config, new Vector3(2f, 0f, 0f), "Near Enemy");
        var enemies = new[] { far, near };

        var result = EnemyTargeting.FindNearestAliveInRange(enemies, Vector3.zero, 10f);

        Assert.That(result, Is.SameAs(near));
    }

    [Test]
    public void IgnoresDeadInactiveAndOutOfRangeEnemies()
    {
        var dead = TheCircussyOneTestObjects.CreateEnemy(_config, new Vector3(1f, 0f, 0f), "Dead Enemy");
        dead.SetHealth(0);

        var inactive = TheCircussyOneTestObjects.CreateEnemy(_config, new Vector3(2f, 0f, 0f), "Inactive Enemy");
        inactive.View.Deactivate();

        var outOfRange = TheCircussyOneTestObjects.CreateEnemy(_config, new Vector3(20f, 0f, 0f), "Out Of Range Enemy");
        var valid = TheCircussyOneTestObjects.CreateEnemy(_config, new Vector3(4f, 0f, 0f), "Valid Enemy");
        var enemies = new[] { dead, inactive, outOfRange, valid };

        var result = EnemyTargeting.FindNearestAliveInRange(enemies, Vector3.zero, 10f);

        Assert.That(result, Is.SameAs(valid));
    }
}
