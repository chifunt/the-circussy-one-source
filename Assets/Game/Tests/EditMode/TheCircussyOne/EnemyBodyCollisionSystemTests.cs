using NUnit.Framework;
using UnityEngine;
using TheCircussyOne.Config;
using TheCircussyOne.Runtime;
using TheCircussyOne.Visuals;

public sealed class EnemyBodyCollisionSystemTests
{
    private GameConfig _config;
    private ActorRegistry _registry;

    [SetUp]
    public void SetUp()
    {
        TheCircussyOneTestObjects.DestroyAll();
        _config = TheCircussyOneTestObjects.CreateConfig();
        _registry = new ActorRegistry();
    }

    [TearDown]
    public void TearDown()
    {
        TheCircussyOneTestObjects.DestroyAll();
        TheCircussyOneTestObjects.Destroy(_config);
    }

    [Test]
    public void EnemyMovementBodiesDoNotHardPushEachOther()
    {
        _config.enemyMovementBodyRadius = 0.6f;
        _config.enemyMovementBodyHeight = 1.45f;
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(_config);
        player.transform.position = new Vector3(20f, 0f, 0f);
        EnemyRuntime first = TheCircussyOneTestObjects.CreateEnemy(_config, Vector3.zero, "First");
        EnemyRuntime second = TheCircussyOneTestObjects.CreateEnemy(_config, new Vector3(0.5f, 0f, 0f), "Second");
        _registry.Register(first);
        _registry.Register(second);
        Vector3 firstBefore = first.Position;
        Vector3 secondBefore = second.Position;

        CreateSystem(player).Tick();

        Assert.That(first.Position, Is.EqualTo(firstBefore));
        Assert.That(second.Position, Is.EqualTo(secondBefore));
    }

    [Test]
    public void PlayerOverlappingMovementBodyIsPushedSideways()
    {
        _config.enemyMovementBodyRadius = 0.65f;
        _config.enemyMovementBodyHeight = 1.45f;
        _config.playerEnemyBlockMaxPushPerFrame = 0.75f;
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(_config);
        EnemyRuntime enemy = TheCircussyOneTestObjects.CreateEnemy(_config, new Vector3(0.45f, 0f, 0f));
        _registry.Register(enemy);
        Physics.SyncTransforms();
        Vector3 before = player.Position;

        CreateSystem(player).Tick();

        Assert.That(HorizontalDistance(player.Position, before), Is.GreaterThan(0.001f));
        Assert.That(player.Position.y, Is.EqualTo(before.y).Within(0.0001f));
    }

    [Test]
    public void PlayerAboveMovementBodyIsNotPushed()
    {
        _config.enemyMovementBodyRadius = 0.65f;
        _config.enemyMovementBodyHeight = 1.0f;
        _config.enemyMovementBodyOffset = new Vector3(0f, 0.5f, 0f);
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(_config);
        player.transform.position = new Vector3(0f, 2.1f, 0f);
        EnemyRuntime enemy = TheCircussyOneTestObjects.CreateEnemy(_config, Vector3.zero);
        _registry.Register(enemy);
        Physics.SyncTransforms();
        Vector3 before = player.Position;

        CreateSystem(player).Tick();

        Assert.That(player.Position, Is.EqualTo(before));
    }

    [Test]
    public void ElevatedMovementBodyCanBlockAnAirbornePlayer()
    {
        _config.enemyMovementBodyRadius = 0.65f;
        _config.enemyMovementBodyHeight = 1.45f;
        _config.enemyMovementBodyOffset = new Vector3(0f, 0.75f, 0f);
        _config.playerEnemyBlockMaxPushPerFrame = 0.75f;
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(_config);
        player.transform.position = new Vector3(0f, 2.1f, 0f);
        EnemyRuntime enemy = TheCircussyOneTestObjects.CreateEnemy(_config, Vector3.zero);
        enemy.View.SetHeight(2f);
        _registry.Register(enemy);
        Physics.SyncTransforms();
        Vector3 before = player.Position;

        CreateSystem(player).Tick();

        Assert.That(HorizontalDistance(player.Position, before), Is.GreaterThan(0.001f));
        Assert.That(player.Position.y, Is.EqualTo(before.y).Within(0.0001f));
    }

    [Test]
    public void PlayerPushRespectsPerFrameCap()
    {
        _config.enemyMovementBodyRadius = 0.85f;
        _config.enemyMovementBodyHeight = 1.45f;
        _config.playerEnemyBlockMaxPushPerFrame = 0.05f;
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(_config);
        EnemyRuntime enemy = TheCircussyOneTestObjects.CreateEnemy(_config, Vector3.zero);
        _registry.Register(enemy);
        Physics.SyncTransforms();
        Vector3 before = player.Position;

        CreateSystem(player).Tick();

        Assert.That(HorizontalDistance(player.Position, before), Is.LessThanOrEqualTo(_config.playerEnemyBlockMaxPushPerFrame + 0.0001f));
    }

    [Test]
    public void MultipleEnemyPushesAreAccumulatedAndCappedOncePerFrame()
    {
        _config.enemyMovementBodyRadius = 0.85f;
        _config.enemyMovementBodyHeight = 1.45f;
        _config.playerEnemyBlockMaxPushPerFrame = 0.08f;
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(_config);
        EnemyRuntime first = TheCircussyOneTestObjects.CreateEnemy(_config, new Vector3(0.35f, 0f, 0f), "First");
        EnemyRuntime second = TheCircussyOneTestObjects.CreateEnemy(_config, new Vector3(0.45f, 0f, 0.15f), "Second");
        _registry.Register(first);
        _registry.Register(second);
        Physics.SyncTransforms();
        Vector3 before = player.Position;

        CreateSystem(player).Tick();

        float displacement = HorizontalDistance(player.Position, before);
        Assert.That(displacement, Is.GreaterThan(0.001f));
        Assert.That(displacement, Is.LessThanOrEqualTo(_config.playerEnemyBlockMaxPushPerFrame + 0.0001f));
        Assert.That(player.Position.y, Is.EqualTo(before.y).Within(0.0001f));
    }

    [Test]
    public void OpposingEnemyPushesStillMovePlayerOutOfOverlap()
    {
        _config.enemyMovementBodyRadius = 0.85f;
        _config.enemyMovementBodyHeight = 1.45f;
        _config.playerEnemyBlockMaxPushPerFrame = 0.75f;
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(_config);
        EnemyRuntime left = TheCircussyOneTestObjects.CreateEnemy(_config, new Vector3(-0.25f, 0f, 0f), "Left");
        EnemyRuntime right = TheCircussyOneTestObjects.CreateEnemy(_config, new Vector3(0.25f, 0f, 0f), "Right");
        _registry.Register(left);
        _registry.Register(right);
        Physics.SyncTransforms();
        Vector3 before = player.Position;

        CreateSystem(player).Tick();

        float displacement = HorizontalDistance(player.Position, before);
        Assert.That(displacement, Is.GreaterThan(0.001f));
        Assert.That(displacement, Is.LessThanOrEqualTo(_config.playerEnemyBlockMaxPushPerFrame + 0.0001f));
        Assert.That(player.Position.y, Is.EqualTo(before.y).Within(0.0001f));
    }

    private EnemyBodyCollisionSystem CreateSystem(PlayerView player)
    {
        return new EnemyBodyCollisionSystem(_config, player, _registry);
    }

    private static float HorizontalDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }
}
