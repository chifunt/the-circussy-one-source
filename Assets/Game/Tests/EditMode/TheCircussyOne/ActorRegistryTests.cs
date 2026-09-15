using NUnit.Framework;
using UnityEngine;
using TheCircussyOne.Config;
using TheCircussyOne.Runtime;

public sealed class ActorRegistryTests
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
    public void RegistersEnemyAndResolvesByView()
    {
        var registry = new ActorRegistry();
        var enemy = TheCircussyOneTestObjects.CreateEnemy(_config, Vector3.zero);

        registry.Register(enemy);

        Assert.That(registry.TryGetEnemy(enemy.View, out EnemyRuntime found), Is.True);
        Assert.That(found, Is.SameAs(enemy));
        Assert.That(registry.Enemies, Has.Count.EqualTo(1));
    }

    [Test]
    public void RemoveInactiveUnregistersInactiveEnemy()
    {
        var registry = new ActorRegistry();
        var enemy = TheCircussyOneTestObjects.CreateEnemy(_config, Vector3.zero);
        registry.Register(enemy);

        enemy.View.Deactivate();
        registry.RemoveInactive();

        Assert.That(registry.Enemies, Is.Empty);
        Assert.That(registry.TryGetEnemy(enemy.View, out _), Is.False);
    }
}
