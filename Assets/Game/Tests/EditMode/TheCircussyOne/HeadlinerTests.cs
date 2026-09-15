using System.Linq;
using NUnit.Framework;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Runtime;
using TheCircussyOne.Visuals;
using UnityEngine;

public sealed class HeadlinerTests
{
    private GameConfig _config;
    private DamageFeedbackVisualConfig _damageFeedbackConfig;
    private ActorMotionVisualConfig _actorMotionConfig;

    [SetUp]
    public void SetUp()
    {
        TheCircussyOneTestObjects.DestroyAll();
        _config = TheCircussyOneTestObjects.CreateConfig();
        _damageFeedbackConfig = TheCircussyOneTestObjects.CreateDamageFeedbackConfig();
        _actorMotionConfig = TheCircussyOneTestObjects.CreateActorMotionVisualConfig();
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
    public void HeadlinerCatalogValidatesMissingEnemyActor()
    {
        HeadlinerDefinition headliner = CreateHeadliner(null);
        HeadlinerCatalog catalog = CreateCatalog(headliner);

        var issues = catalog.ValidateContent();

        Assert.That(issues.Any(issue => issue.Code == "headliner.missing-enemy-actor"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "headliner.no-valid-headliners"), Is.True);

        TheCircussyOneTestObjects.Destroy(headliner);
        TheCircussyOneTestObjects.Destroy(catalog);
    }

    [Test]
    public void HeadlinerCatalogFindsActSpecificHeadlinerAndFallsBackToDefault()
    {
        EnemyDefinition actor = CreateActor("opening_headliner_actor", "Opening Headliner Actor");
        HeadlinerDefinition actOne = CreateHeadliner(actor, "opening_headliner", "Opening Headliner", actNumber: 1);
        HeadlinerDefinition actTwo = CreateHeadliner(actor, "second_headliner", "Second Headliner", actNumber: 2);
        HeadlinerCatalog catalog = CreateCatalog(actOne, actTwo);

        Assert.That(catalog.ForAct(1), Is.SameAs(actOne));
        Assert.That(catalog.ForAct(2), Is.SameAs(actTwo));
        Assert.That(catalog.ForAct(3), Is.SameAs(actOne));
        Assert.That(catalog.ValidateContent().Any(issue => issue.Severity == ContentValidationSeverity.Error), Is.False);

        TheCircussyOneTestObjects.Destroy(actor);
        TheCircussyOneTestObjects.Destroy(actOne);
        TheCircussyOneTestObjects.Destroy(actTwo);
        TheCircussyOneTestObjects.Destroy(catalog);
    }

    [Test]
    public void RealHeadlinerCatalogPreventsProxyBossAutoDefeat()
    {
        RunScheduleConfig scheduleConfig = RunScheduleConfig.CreateRuntimeDefault();
        scheduleConfig.actFinaleWarningSeconds = 0f;
        scheduleConfig.actFinaleProxyBossSeconds = 0f;
        EnemyDefinition actor = CreateActor("opening_headliner_actor", "Opening Headliner Actor");
        HeadlinerDefinition headliner = CreateHeadliner(actor);
        HeadlinerCatalog catalog = CreateCatalog(headliner);
        var schedule = new RunActScheduleState(scheduleConfig);
        var phase = new RunPhaseState();
        phase.BeginWorld();
        schedule.BeginAct(1);
        var progression = new RunActProgressionSystem(schedule, phase, scheduleConfig, catalog);
        progression.Start();

        phase.StartBossWarning();
        progression.Tick(0f);
        Assert.That(phase.CurrentPhase, Is.EqualTo(RunPhase.BossActive));

        phase.Tick(10f);
        progression.Tick(10f);
        Assert.That(phase.CurrentPhase, Is.EqualTo(RunPhase.BossActive));

        progression.Dispose();
        TheCircussyOneTestObjects.Destroy(scheduleConfig);
        TheCircussyOneTestObjects.Destroy(actor);
        TheCircussyOneTestObjects.Destroy(headliner);
        TheCircussyOneTestObjects.Destroy(catalog);
    }

    [Test]
    public void HeadlinerSystemSpawnsActorAndCompletesBossPhaseOnKill()
    {
        EnemyDefinition actor = CreateActor("opening_headliner_actor", "Opening Headliner Actor");
        actor.baseHealth = 12;
        actor.baseMoveSpeed = 3.25f;
        actor.contactDamage = 5;
        HeadlinerDefinition headliner = CreateHeadliner(actor);
        HeadlinerCatalog catalog = CreateCatalog(headliner);
        var registry = new ActorRegistry();
        var enemyFactory = new EnemyFactory(
            _config,
            _damageFeedbackConfig,
            registry,
            TheCircussyOneTestObjects.CreateEnemyPrefab(),
            TheCircussyOneTestObjects.CreateRoot("Headliner Enemies").transform);
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(_config);
        RunScheduleConfig scheduleConfig = RunScheduleConfig.CreateRuntimeDefault();
        var schedule = new RunActScheduleState(scheduleConfig);
        var phase = new RunPhaseState();
        schedule.BeginAct(1);
        phase.BeginWorld();
        var damageService = new EnemyDamageService(
            _config,
            _damageFeedbackConfig,
            _actorMotionConfig,
            enemyFactory,
            null,
            new GameState(_config),
            null,
            new NullDamageNumberSpawner());
        var defeatState = new HeadlinerDefeatState();
        var system = new HeadlinerSystem(catalog, enemyFactory, player, phase, schedule, damageService, defeatState: defeatState);
        system.Start();

        phase.StartBossWarning();
        phase.ActivateBoss();

        HeadlinerRuntime active = system.ActiveHeadliner;
        Assert.That(active, Is.Not.Null);
        Assert.That(active.Definition, Is.SameAs(headliner));
        Assert.That(active.Enemy.Definition, Is.SameAs(actor));
        Assert.That(active.Enemy.MaxHealth, Is.EqualTo(12));
        Assert.That(registry.Enemies, Does.Contain(active.Enemy));
        Vector3 deathPosition = active.Enemy.Position;

        bool killed = damageService.Apply(active.Enemy, active.Enemy.CurrentHealth, active.Enemy.AimPosition, Vector3.forward);

        Assert.That(killed, Is.True);
        Assert.That(phase.CurrentPhase, Is.EqualTo(RunPhase.BossDefeated));
        Assert.That(system.ActiveHeadliner, Is.Null);
        Assert.That(defeatState.TryGetDeathPosition(out Vector3 recordedPosition), Is.True);
        Assert.That(recordedPosition, Is.EqualTo(deathPosition));

        system.Dispose();
        TheCircussyOneTestObjects.Destroy(scheduleConfig);
        TheCircussyOneTestObjects.Destroy(actor);
        TheCircussyOneTestObjects.Destroy(headliner);
        TheCircussyOneTestObjects.Destroy(catalog);
    }

    [Test]
    public void HeadlinerDefeatStateKeepsFirstRecordedPosition()
    {
        var defeatState = new HeadlinerDefeatState();
        int recordedCount = 0;
        defeatState.DeathPositionRecorded += _ => recordedCount++;

        bool firstRecorded = defeatState.RecordDeathPositionIfMissing(new Vector3(2f, 3f, 4f));
        bool secondRecorded = defeatState.RecordDeathPositionIfMissing(new Vector3(9f, 9f, 9f));

        Assert.That(firstRecorded, Is.True);
        Assert.That(secondRecorded, Is.False);
        Assert.That(defeatState.DeathPosition, Is.EqualTo(new Vector3(2f, 3f, 4f)));
        Assert.That(recordedCount, Is.EqualTo(1));
    }

    [Test]
    public void EnemyFactoryCanSpawnExplicitHeadlinerActorOutsideEnemyCatalog()
    {
        EnemyDefinition normal = CreateActor("normal_enemy", "Normal Enemy");
        normal.baseHealth = 4;
        EnemyDefinition headlinerActor = CreateActor("opening_headliner_actor", "Opening Headliner Actor");
        headlinerActor.baseHealth = 44;
        headlinerActor.baseMoveSpeed = 2.75f;
        headlinerActor.contactDamage = 9;
        EnemyCatalog normalCatalog = TheCircussyOneTestObjects.CreateEnemyCatalog(normal);
        var registry = new ActorRegistry();
        var factory = new EnemyFactory(
            _config,
            _damageFeedbackConfig,
            normalCatalog,
            registry,
            TheCircussyOneTestObjects.CreateEnemyPrefab(),
            TheCircussyOneTestObjects.CreateRoot("Explicit Headliner Enemies").transform);

        EnemyRuntime spawned = factory.Spawn(headlinerActor, Vector3.zero, level: 1, elapsedSeconds: 0f);

        Assert.That(spawned.Definition, Is.SameAs(headlinerActor));
        Assert.That(spawned.MaxHealth, Is.EqualTo(44));
        Assert.That(spawned.MoveSpeed, Is.EqualTo(2.75f).Within(0.0001f));
        Assert.That(spawned.ContactDamage, Is.EqualTo(9));
        Assert.That(registry.Enemies, Does.Contain(spawned));

        TheCircussyOneTestObjects.Destroy(normal);
        TheCircussyOneTestObjects.Destroy(headlinerActor);
        TheCircussyOneTestObjects.Destroy(normalCatalog);
    }

    private EnemyDefinition CreateActor(string id = "opening_headliner_actor", string displayName = "Opening Headliner Actor")
    {
        EnemyDefinition actor = TheCircussyOneTestObjects.CreateEnemyDefinition(_config);
        actor.enemyId = id;
        actor.displayName = displayName;
        actor.isActive = true;
        actor.baseHealth = 24;
        actor.baseMoveSpeed = 2.5f;
        actor.contactDamage = 4;
        actor.spawnWeight = 0f;
        actor.xpBudget = 0;
        actor.stack.policy = EnemyStackPolicy.GroundOnly;
        actor.stack.canClimbEnemies = false;
        actor.stack.canBeStackedOn = false;
        return actor;
    }

    private static HeadlinerDefinition CreateHeadliner(
        EnemyDefinition actor,
        string id = "opening_headliner",
        string displayName = "Opening Headliner",
        int actNumber = 1)
    {
        var headliner = ScriptableObject.CreateInstance<HeadlinerDefinition>();
        headliner.ApplyDefaults(actor);
        headliner.headlinerId = id;
        headliner.displayName = displayName;
        headliner.actNumber = actNumber;
        headliner.spawnDistanceFromPlayer = 0f;
        headliner.spawnProbeHeight = 0f;
        headliner.spawnProbeDepth = 0f;
        return headliner;
    }

    private static HeadlinerCatalog CreateCatalog(params HeadlinerDefinition[] headliners)
    {
        var catalog = ScriptableObject.CreateInstance<HeadlinerCatalog>();
        if (headliners != null)
        {
            catalog.headliners.AddRange(headliners);
        }

        return catalog;
    }
}
