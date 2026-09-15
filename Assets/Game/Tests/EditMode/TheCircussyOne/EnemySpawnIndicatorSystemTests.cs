using NUnit.Framework;
using UnityEngine;
using TheCircussyOne.Config;
using TheCircussyOne.Runtime;

public sealed class EnemySpawnIndicatorSystemTests
{
    private GameConfig _gameConfig;
    private DamageFeedbackVisualConfig _damageFeedbackConfig;
    private EnemySpawnVisualConfig _spawnVisualConfig;
    private ActorRegistry _registry;
    private FakeGameTime _time;
    private EnemyFactory _enemyFactory;
    private EnemySpawnIndicatorFactory _indicatorFactory;

    [SetUp]
    public void SetUp()
    {
        TheCircussyOneTestObjects.DestroyAll();
        _gameConfig = TheCircussyOneTestObjects.CreateConfig();
        _damageFeedbackConfig = TheCircussyOneTestObjects.CreateDamageFeedbackConfig();
        _spawnVisualConfig = TheCircussyOneTestObjects.CreateEnemySpawnVisualConfig();
        _registry = new ActorRegistry();
        _time = new FakeGameTime();
        _enemyFactory = new EnemyFactory(
            _gameConfig,
            _damageFeedbackConfig,
            _registry,
            TheCircussyOneTestObjects.CreateEnemyPrefab(),
            TheCircussyOneTestObjects.CreateRoot("Enemies").transform);
        _indicatorFactory = new EnemySpawnIndicatorFactory(
            _spawnVisualConfig,
            TheCircussyOneTestObjects.CreateEnemySpawnIndicatorPrefab(),
            TheCircussyOneTestObjects.CreateRoot("Spawn Indicators").transform);
    }

    [TearDown]
    public void TearDown()
    {
        TheCircussyOneTestObjects.DestroyAll();
        TheCircussyOneTestObjects.Destroy(_gameConfig);
        TheCircussyOneTestObjects.Destroy(_damageFeedbackConfig);
        TheCircussyOneTestObjects.Destroy(_spawnVisualConfig);
    }

    [Test]
    public void EnemyBecomesActiveOnlyAfterWarningAndEmergence()
    {
        CreateElevatedGround(3f);
        EnemySpawnIndicatorSystem system = CreateSystem();
        system.RequestSpawn(new Vector3(1f, 6f, -2f), level: 1, elapsedSeconds: 0f);
        Assert.That(_indicatorFactory.Active[0].Position.y, Is.EqualTo(3f).Within(0.0001f));
        Assert.That(_indicatorFactory.Active[0].View.transform.position.y, Is.EqualTo(3f + _spawnVisualConfig.groundYOffset).Within(0.0001f));

        _time.Time = 0.4f;
        system.Tick();
        Assert.That(_registry.Enemies, Is.Empty);
        Assert.That(_indicatorFactory.Active, Has.Count.EqualTo(1));

        _time.Time = _spawnVisualConfig.warningSeconds + 0.02f;
        system.Tick();
        Assert.That(_registry.Enemies, Is.Empty);
        Assert.That(_indicatorFactory.Active[0].SpawnedEnemy, Is.Not.Null);
        Assert.That(_indicatorFactory.Active[0].SpawnedEnemy.Position.y, Is.EqualTo(3f).Within(0.0001f));
        Assert.That(_indicatorFactory.Active[0].SpawnedEnemy.View.BodyLocalY, Is.LessThan(_spawnVisualConfig.emergeEndBodyY));

        _time.Time = _spawnVisualConfig.warningSeconds + _spawnVisualConfig.emergeSeconds + 0.05f;
        system.Tick();
        Assert.That(_registry.Enemies, Has.Count.EqualTo(1));
        Assert.That(_indicatorFactory.Active, Is.Empty);
        Assert.That(_registry.Enemies[0].View.BodyLocalY, Is.EqualTo(_spawnVisualConfig.emergeEndBodyY).Within(0.0001f));
    }

    [Test]
    public void DisabledConfigSpawnsImmediately()
    {
        _spawnVisualConfig.enabled = false;
        EnemySpawnIndicatorSystem system = CreateSystem();

        system.RequestSpawn(new Vector3(1f, 8f, 2f), level: 1, elapsedSeconds: 0f);

        Assert.That(_registry.Enemies, Has.Count.EqualTo(1));
        Assert.That(_registry.Enemies[0].Position.y, Is.EqualTo(8f).Within(0.0001f));
        Assert.That(_indicatorFactory.Active, Is.Empty);
    }

    private EnemySpawnIndicatorSystem CreateSystem()
    {
        return new EnemySpawnIndicatorSystem(
            _spawnVisualConfig,
            _indicatorFactory,
            _enemyFactory,
            new GameState(_gameConfig),
            _time);
    }

    private static void CreateElevatedGround(float y)
    {
        GameObject ground = TheCircussyOneTestObjects.CreateRoot("Elevated Ground");
        var collider = ground.AddComponent<BoxCollider>();
        collider.center = Vector3.zero;
        collider.size = new Vector3(20f, 0.2f, 20f);
        ground.transform.position = new Vector3(0f, y - 0.1f, 0f);
        Physics.SyncTransforms();
    }
}
