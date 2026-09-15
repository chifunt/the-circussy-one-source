using NUnit.Framework;
using UnityEngine;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;

public sealed class DamageRulesTests
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
    public void ApplyDamageClampsAndReportsLethalHit()
    {
        var enemy = TheCircussyOneTestObjects.CreateEnemy(_config, Vector3.zero);

        bool killed = DamageRules.ApplyDamage(enemy, enemy.CurrentHealth);

        Assert.That(killed, Is.True);
        Assert.That(enemy.CurrentHealth, Is.Zero);
        Assert.That(enemy.IsDead, Is.True);
    }
}
