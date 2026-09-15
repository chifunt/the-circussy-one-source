using NUnit.Framework;
using UnityEngine;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Runtime;
using TheCircussyOne.Visuals;

public sealed class FactoryPoolingTests
{
    private GameConfig _config;
    private DamageFeedbackVisualConfig _damageFeedbackConfig;
    private WeaponDefinition _weapon;
    private ActorRegistry _registry;

    [SetUp]
    public void SetUp()
    {
        TheCircussyOneTestObjects.DestroyAll();
        _config = TheCircussyOneTestObjects.CreateConfig();
        _damageFeedbackConfig = TheCircussyOneTestObjects.CreateDamageFeedbackConfig();
        _weapon = TheCircussyOneTestObjects.CreateWeaponDefinition(_config);
        _registry = new ActorRegistry();
    }

    [TearDown]
    public void TearDown()
    {
        TheCircussyOneTestObjects.DestroyAll();
        TheCircussyOneTestObjects.Destroy(_config);
        TheCircussyOneTestObjects.Destroy(_damageFeedbackConfig);
        TheCircussyOneTestObjects.Destroy(_weapon);
    }

    [Test]
    public void EnemyFactoryReusesDespawnedView()
    {
        var factory = new EnemyFactory(_config, _registry, TheCircussyOneTestObjects.CreateEnemyPrefab(), TheCircussyOneTestObjects.CreateRoot("Enemies").transform);
        EnemyRuntime first = factory.Spawn(Vector3.zero, level: 1, elapsedSeconds: 0f);
        EnemyView firstView = first.View;

        factory.Despawn(first, playDeath: false);
        EnemyRuntime second = factory.Spawn(Vector3.right, level: 1, elapsedSeconds: 0f);

        Assert.That(second.View, Is.SameAs(firstView));
        Assert.That(_registry.Enemies, Has.Count.EqualTo(1));
    }

    [Test]
    public void EnemyFactoryPrewarmsInactiveViews()
    {
        var factory = new EnemyFactory(_config, _registry, TheCircussyOneTestObjects.CreateEnemyPrefab(), TheCircussyOneTestObjects.CreateRoot("Enemies").transform);

        int created = factory.Prewarm(2);
        EnemyRuntime enemy = factory.Spawn(Vector3.zero, level: 1, elapsedSeconds: 0f);

        Assert.That(created, Is.EqualTo(2));
        Assert.That(factory.PooledCount, Is.EqualTo(1));
        Assert.That(enemy.View.IsActive, Is.True);
    }

    [Test]
    public void ProjectileFactoryReusesDespawnedView()
    {
        var time = new FakeGameTime();
        var factory = new ProjectileFactory(_damageFeedbackConfig, _registry, time, TheCircussyOneTestObjects.CreateProjectilePrefab(), TheCircussyOneTestObjects.CreateRoot("Projectiles").transform);
        ProjectileRuntime first = factory.Spawn(_weapon, Vector3.zero, Vector3.forward);
        ProjectileView firstView = first.View;

        factory.Despawn(first);
        ProjectileRuntime second = factory.Spawn(_weapon, Vector3.right, Vector3.forward);

        Assert.That(second.View, Is.SameAs(firstView));
        Assert.That(_registry.Projectiles, Has.Count.EqualTo(1));
    }

    [Test]
    public void ProjectileFactoryPrewarmsAndUsesPool()
    {
        var time = new FakeGameTime();
        var factory = new ProjectileFactory(_damageFeedbackConfig, _registry, time, TheCircussyOneTestObjects.CreateProjectilePrefab(), TheCircussyOneTestObjects.CreateRoot("Projectiles").transform);

        int created = factory.Prewarm(3);
        ProjectileRuntime projectile = factory.Spawn(_weapon, Vector3.zero, Vector3.forward);

        Assert.That(created, Is.EqualTo(3));
        Assert.That(factory.PooledCount, Is.EqualTo(2));
        Assert.That(projectile.View.IsActive, Is.True);
    }

    [Test]
    public void PickupFactoryPrewarmsAndUsesPool()
    {
        var factory = new PickupFactory(_config, _registry, TheCircussyOneTestObjects.CreatePickupPrefab(), TheCircussyOneTestObjects.CreateRoot("Pickups").transform);

        int created = factory.Prewarm(4);
        PickupRuntime pickup = factory.Spawn(Vector3.zero);

        Assert.That(created, Is.EqualTo(4));
        Assert.That(factory.PooledCount, Is.EqualTo(3));
        Assert.That(pickup.View.IsActive, Is.True);
    }

    [Test]
    public void DamageNumberFactoryReusesDespawnedView()
    {
        var factory = new DamageNumberFactory(_damageFeedbackConfig, TheCircussyOneTestObjects.CreateDamageNumberPrefab(), TheCircussyOneTestObjects.CreateRoot("Damage Numbers").transform);
        DamageNumberRuntime first = factory.Spawn(new DamageNumberRequest(2, Vector3.up, Vector3.up, 1), spawnedAt: 0f, seed: 1);
        DamageNumberView firstView = first.View;

        factory.Despawn(first);
        DamageNumberRuntime second = factory.Spawn(new DamageNumberRequest(3, Vector3.right, Vector3.right, 2), spawnedAt: 0.5f, seed: 2);

        Assert.That(second.View, Is.SameAs(firstView));
        Assert.That(factory.Active, Has.Count.EqualTo(1));
        Assert.That(factory.PooledCount, Is.Zero);
    }

    [Test]
    public void DamageNumberFactoryPrewarmsAndUsesPool()
    {
        var factory = new DamageNumberFactory(_damageFeedbackConfig, TheCircussyOneTestObjects.CreateDamageNumberPrefab(), TheCircussyOneTestObjects.CreateRoot("Damage Numbers").transform);

        int created = factory.Prewarm(2);
        DamageNumberRuntime damageNumber = factory.Spawn(new DamageNumberRequest(4, Vector3.up, Vector3.up, 1), spawnedAt: 0f, seed: 1);

        Assert.That(created, Is.EqualTo(2));
        Assert.That(factory.PooledCount, Is.EqualTo(1));
        Assert.That(damageNumber.View.IsActive, Is.True);
    }
}
