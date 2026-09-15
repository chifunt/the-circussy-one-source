using NUnit.Framework;
using UnityEngine;
using TheCircussyOne.Config;
using TheCircussyOne.Runtime;
using TheCircussyOne.Visuals;

public sealed class EnemySpawnIndicatorFactoryTests
{
    private EnemySpawnVisualConfig _config;

    [SetUp]
    public void SetUp()
    {
        TheCircussyOneTestObjects.DestroyAll();
        _config = TheCircussyOneTestObjects.CreateEnemySpawnVisualConfig();
    }

    [TearDown]
    public void TearDown()
    {
        TheCircussyOneTestObjects.DestroyAll();
        TheCircussyOneTestObjects.Destroy(_config);
    }

    [Test]
    public void FactoryReusesDespawnedIndicatorView()
    {
        var factory = new EnemySpawnIndicatorFactory(
            _config,
            TheCircussyOneTestObjects.CreateEnemySpawnIndicatorPrefab(),
            TheCircussyOneTestObjects.CreateRoot("Spawn Indicators").transform);
        EnemySpawnIndicatorRuntime first = factory.Spawn(Vector3.zero, level: 1, enemyElapsedSeconds: 0f, startedAt: 0f);
        EnemySpawnIndicatorView firstView = first.View;

        factory.Despawn(first);
        EnemySpawnIndicatorRuntime second = factory.Spawn(Vector3.right, level: 1, enemyElapsedSeconds: 0f, startedAt: 1f);

        Assert.That(second.View, Is.SameAs(firstView));
        Assert.That(factory.Active, Has.Count.EqualTo(1));
        Assert.That(factory.PooledCount, Is.Zero);
    }

    [Test]
    public void FactoryPreservesSurfacePositionAndAlignsViewToSurfaceNormal()
    {
        var factory = new EnemySpawnIndicatorFactory(
            _config,
            TheCircussyOneTestObjects.CreateEnemySpawnIndicatorPrefab(),
            TheCircussyOneTestObjects.CreateRoot("Spawn Indicators").transform);
        Vector3 normal = (Quaternion.AngleAxis(30f, Vector3.right) * Vector3.up).normalized;

        EnemySpawnIndicatorRuntime runtime = factory.Spawn(new Vector3(2f, 4f, 1f), normal, level: 1, enemyElapsedSeconds: 0f, startedAt: 0f);

        Assert.That(runtime.Position.y, Is.EqualTo(4f).Within(0.0001f));
        Assert.That(Vector3.Angle(runtime.SurfaceNormal, normal), Is.LessThan(0.1f));
        Assert.That(runtime.View.transform.position.y, Is.EqualTo(4f + _config.groundYOffset).Within(0.0001f));
        Assert.That(Vector3.Angle(runtime.View.transform.up, normal), Is.LessThan(0.1f));
    }
}
