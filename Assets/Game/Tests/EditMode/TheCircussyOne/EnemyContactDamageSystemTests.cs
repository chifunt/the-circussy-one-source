using NUnit.Framework;
using UnityEngine;
using TheCircussyOne.Config;
using TheCircussyOne.Runtime;
using TheCircussyOne.Rules;

public sealed class EnemyContactDamageSystemTests
{
    private GameConfig _config;
    private DamageFeedbackVisualConfig _damageFeedbackConfig;
    private ActorMotionVisualConfig _actorMotionConfig;
    private ActorRegistry _registry;
    private FakeGameTime _time;

    [SetUp]
    public void SetUp()
    {
        TheCircussyOneTestObjects.DestroyAll();
        _config = TheCircussyOneTestObjects.CreateConfig();
        _damageFeedbackConfig = TheCircussyOneTestObjects.CreateDamageFeedbackConfig();
        _actorMotionConfig = TheCircussyOneTestObjects.CreateActorMotionVisualConfig();
        _registry = new ActorRegistry();
        _time = new FakeGameTime { DeltaTime = 0.25f, Time = 2f };
    }

    [TearDown]
    public void TearDown()
    {
        TheCircussyOneTestObjects.DestroyAll();
        TheCircussyOneTestObjects.Destroy(_config);
        TheCircussyOneTestObjects.Destroy(_damageFeedbackConfig);
        TheCircussyOneTestObjects.Destroy(_actorMotionConfig);
    }

    [Test]
    public void VisualProximityDoesNotDamagePlayerWhenContactHitboxIsOutOfRange()
    {
        _config.enemyContactHitboxSize = new Vector3(0.1f, 0.1f, 0.1f);
        _config.enemyContactHitboxOffset = new Vector3(0f, 0.7f, 5f);
        var player = TheCircussyOneTestObjects.CreatePlayer(_config);
        var state = new GameState(_config);
        EnemyRuntime enemy = TheCircussyOneTestObjects.CreateEnemy(_config, new Vector3(0.75f, 0f, 0f));
        _registry.Register(enemy);
        Physics.SyncTransforms();

        CreateContactSystem(player, state).Tick();

        Assert.That(state.Health, Is.EqualTo(state.MaxHealth));
    }

    [Test]
    public void ContactHitboxDamagesPlayerAndRespectsCooldown()
    {
        _config.contactDamageCooldown = 1f;
        _config.enemyContactHitboxSize = new Vector3(1.1f, 0.9f, 1.1f);
        _config.enemyContactHitboxOffset = new Vector3(0f, 0.55f, 0f);
        var player = TheCircussyOneTestObjects.CreatePlayer(_config);
        var state = new GameState(_config);
        EnemyRuntime enemy = TheCircussyOneTestObjects.CreateEnemy(_config, new Vector3(0.8f, 0f, 0f));
        _registry.Register(enemy);
        Physics.SyncTransforms();
        EnemyContactDamageSystem system = CreateContactSystem(player, state);

        system.Tick();
        system.Tick();

        Assert.That(state.Health, Is.EqualTo(state.MaxHealth - _config.enemyContactDamage));
    }

    [Test]
    public void ContactDamageRecordsEnemyDisplayNameAsDamageCause()
    {
        _config.enemyContactHitboxSize = new Vector3(1.1f, 0.9f, 1.1f);
        _config.enemyContactHitboxOffset = new Vector3(0f, 0.55f, 0f);
        var player = TheCircussyOneTestObjects.CreatePlayer(_config);
        var state = new GameState(_config);
        var definition = TheCircussyOneTestObjects.CreateEnemyDefinition(_config);
        definition.displayName = "Stilt Walker";
        EnemyRuntime enemy = TheCircussyOneTestObjects.CreateEnemy(definition, new Vector3(0.8f, 0f, 0f));
        _registry.Register(enemy);
        Physics.SyncTransforms();

        CreateContactSystem(player, state).Tick();

        Assert.That(state.LastDamageCause, Is.EqualTo("Stilt Walker"));
    }

    [Test]
    public void JumpingAboveContactHitboxDoesNotDamagePlayer()
    {
        _config.enemyContactHitboxSize = new Vector3(1.1f, 0.9f, 1.1f);
        _config.enemyContactHitboxOffset = new Vector3(0f, 0.55f, 0f);
        var player = TheCircussyOneTestObjects.CreatePlayer(_config);
        player.transform.position = new Vector3(0f, 1.4f, 0f);
        var state = new GameState(_config);
        EnemyRuntime enemy = TheCircussyOneTestObjects.CreateEnemy(_config, new Vector3(0.8f, 0f, 0f));
        _registry.Register(enemy);
        Physics.SyncTransforms();

        CreateContactSystem(player, state).Tick();

        Assert.That(state.Health, Is.EqualTo(state.MaxHealth));
    }

    [Test]
    public void ContactDamageIsEvaluatedBeforeMovementBodyPushOut()
    {
        _config.enemyContactHitboxSize = new Vector3(1.1f, 1.2f, 1.1f);
        _config.enemyContactHitboxOffset = new Vector3(0f, 0.7f, 0f);
        _config.enemyMovementBodyRadius = 0.65f;
        _config.enemyMovementBodyHeight = 1.45f;
        _config.playerEnemyBlockMaxPushPerFrame = 0.75f;
        var player = TheCircussyOneTestObjects.CreatePlayer(_config);
        var state = new GameState(_config);
        EnemyRuntime enemy = TheCircussyOneTestObjects.CreateEnemy(_config, new Vector3(0.45f, 0f, 0f));
        _registry.Register(enemy);
        Physics.SyncTransforms();
        Vector3 before = player.Position;

        CreateContactSystem(player, state).Tick();
        new EnemyBodyCollisionSystem(_config, player, _registry).Tick();

        Assert.That(state.Health, Is.EqualTo(state.MaxHealth - _config.enemyContactDamage));
        Assert.That(HorizontalDistance(player.Position, before), Is.GreaterThan(0.001f));
    }

    [Test]
    public void MovementBodyOverlapAloneDoesNotDamagePlayer()
    {
        _config.enemyContactHitboxSize = new Vector3(0.1f, 0.1f, 0.1f);
        _config.enemyContactHitboxOffset = new Vector3(0f, 0.7f, 5f);
        _config.enemyMovementBodyRadius = 0.75f;
        _config.enemyMovementBodyHeight = 1.45f;
        var player = TheCircussyOneTestObjects.CreatePlayer(_config);
        var state = new GameState(_config);
        EnemyRuntime enemy = TheCircussyOneTestObjects.CreateEnemy(_config, new Vector3(0.4f, 0f, 0f));
        _registry.Register(enemy);
        Physics.SyncTransforms();

        CreateContactSystem(player, state).Tick();
        new EnemyBodyCollisionSystem(_config, player, _registry).Tick();

        Assert.That(state.Health, Is.EqualTo(state.MaxHealth));
    }

    [Test]
    public void ContactDamageStillAppliesWhenContactFrontProtrudesToMovementBodyFront()
    {
        _config.enemyMovementBodyRadius = 0.52f;
        _config.enemyMovementBodyOffset = new Vector3(0f, 0.75f, 0f);
        _config.enemyContactHitboxSize = new Vector3(1.1f, 1.2f, 0.2f);
        _config.enemyContactHitboxOffset = EnemyHitboxAuthoringRules.FitContactFrontToMovementBody(
            new Vector3(0f, 0.7f, 0f),
            _config.enemyContactHitboxSize,
            _config.enemyMovementBodyOffset,
            _config.enemyMovementBodyRadius);
        var player = TheCircussyOneTestObjects.CreatePlayer(_config);
        player.transform.position = new Vector3(0f, 0f, 0.9f);
        var state = new GameState(_config);
        EnemyRuntime enemy = TheCircussyOneTestObjects.CreateEnemy(_config, Vector3.zero);
        _registry.Register(enemy);
        Physics.SyncTransforms();

        CreateContactSystem(player, state).Tick();

        Assert.That(_config.HasValidEnemyContactFrontMargin, Is.True);
        Assert.That(state.Health, Is.EqualTo(state.MaxHealth - _config.enemyContactDamage));
    }

    [Test]
    public void ContactDamageUsesEnemyHeightAfterClimbing()
    {
        _config.enemyContactHitboxSize = new Vector3(1.1f, 0.9f, 1.1f);
        _config.enemyContactHitboxOffset = new Vector3(0f, 0.55f, 0f);
        var player = TheCircussyOneTestObjects.CreatePlayer(_config);
        player.transform.position = new Vector3(0f, 1.2f, 0f);
        var state = new GameState(_config);
        EnemyRuntime enemy = TheCircussyOneTestObjects.CreateEnemy(_config, Vector3.zero);
        enemy.View.SetHeight(1.2f);
        _registry.Register(enemy);
        Physics.SyncTransforms();

        CreateContactSystem(player, state).Tick();

        Assert.That(state.Health, Is.EqualTo(state.MaxHealth - _config.enemyContactDamage));
    }

    private EnemyContactDamageSystem CreateContactSystem(TheCircussyOne.Visuals.PlayerView player, GameState state)
    {
        var feedback = new FeedbackService(_config, _damageFeedbackConfig, _actorMotionConfig, player, null, null, null, null);
        return new EnemyContactDamageSystem(_config, player, _registry, state, feedback, _time);
    }

    private static float HorizontalDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }
}
