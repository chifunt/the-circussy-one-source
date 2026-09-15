using NUnit.Framework;
using UnityEngine;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Runtime;
using TheCircussyOne.Visuals;

public sealed class EnemySpawnSystemTests
{
    private GameConfig _gameConfig;
    private DamageFeedbackVisualConfig _damageFeedbackConfig;
    private EnemySpawnVisualConfig _spawnVisualConfig;
    private ActorRegistry _registry;
    private FakeGameTime _time;
    private EnemySpawnIndicatorFactory _indicatorFactory;
    private EnemySpawnIndicatorSystem _indicatorSystem;
    private EnemyFactory _enemyFactory;
    private GameState _state;

    [SetUp]
    public void SetUp()
    {
        TheCircussyOneTestObjects.DestroyAll();
        _gameConfig = TheCircussyOneTestObjects.CreateConfig();
        _damageFeedbackConfig = TheCircussyOneTestObjects.CreateDamageFeedbackConfig();
        _spawnVisualConfig = TheCircussyOneTestObjects.CreateEnemySpawnVisualConfig();
        _registry = new ActorRegistry();
        _time = new FakeGameTime { DeltaTime = 2f };
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
        _state = new GameState(_gameConfig);
        _indicatorSystem = new EnemySpawnIndicatorSystem(_spawnVisualConfig, _indicatorFactory, _enemyFactory, _state, _time);
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
    public void SpawnTimerRequestsIndicatorInsteadOfImmediateEnemy()
    {
        EnemySpawnSystem spawnSystem = CreateSpawnSystem();

        spawnSystem.Tick();

        Assert.That(_indicatorFactory.Active, Has.Count.EqualTo(1));
        Assert.That(_registry.Enemies, Is.Empty);
    }

    [Test]
    public void SpawnUsesTerrainResolvedPlayerWorldPosition()
    {
        CreateElevatedGround(2f);
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(_gameConfig);
        player.transform.position = new Vector3(3f, 4.5f, -2f);
        EnemySpawnSystem spawnSystem = CreateSpawnSystem(player);

        spawnSystem.Tick();

        Assert.That(_indicatorFactory.Active, Has.Count.EqualTo(1));
        Assert.That(_indicatorFactory.Active[0].Position.y, Is.EqualTo(2f).Within(0.0001f));
        Assert.That(_indicatorFactory.Active[0].View.transform.position.y, Is.EqualTo(2f + _spawnVisualConfig.groundYOffset).Within(0.0001f));
    }

    [Test]
    public void SpawnIndicatorSnapsToGeneratedSpawnSafeSampleBeforeGrounding()
    {
        RunWorldGenerationConfig worldConfig = RunWorldGenerationConfig.CreateRuntimeDefault();
        worldConfig.enemySpawnSafeSearchRadius = 4f;
        var generationState = new RunWorldGenerationState();
        GeneratedWorldMap map = GeneratedSpawnSafeMap();
        GameObject generatedRoot = TheCircussyOneTestObjects.CreateRoot("Generated Spawn Test Terrain");
        generationState.Set(new WorldGenerationResult(
            worldIndex: 1,
            actNumber: 1,
            seed: 123,
            usedPrototypePlacement: false,
            generatedRoot: generatedRoot,
            rewardSpawnRequests: new WorldRewardSpawnRequest[0],
            enemySpawnBands: null,
            playerStart: Vector3.zero,
            hasPlayerStart: false,
            headlinerAnchor: Vector3.zero,
            hasHeadlinerAnchor: false,
            stageDoorAnchor: Vector3.zero,
            hasStageDoorAnchor: false,
            bounds: map.Bounds,
            generatedMap: map));
        var indicatorSystem = new EnemySpawnIndicatorSystem(
            _spawnVisualConfig,
            _indicatorFactory,
            _enemyFactory,
            _state,
            _time,
            surfaceResolver: null,
            phaseState: null,
            generationState: generationState,
            worldGenerationConfig: worldConfig);

        indicatorSystem.RequestSpawn(new Vector3(9.25f, 0f, 0.35f), level: 1, elapsedSeconds: 0f);

        Assert.That(_indicatorFactory.Active, Has.Count.EqualTo(1));
        Assert.That(_indicatorFactory.Active[0].Position, Is.EqualTo(new Vector3(10f, 3.04f, 0f)));

        Object.DestroyImmediate(worldConfig);
    }

    [Test]
    public void SpawnIndicatorFallsBackToGeneratedSpawnSafeSampleInsteadOfOutsideArena()
    {
        RunWorldGenerationConfig worldConfig = RunWorldGenerationConfig.CreateRuntimeDefault();
        worldConfig.enemySpawnSafeSearchRadius = 2f;
        var generationState = new RunWorldGenerationState();
        GeneratedWorldMap map = GeneratedCenterSpawnSafeMap();
        GameObject generatedRoot = TheCircussyOneTestObjects.CreateRoot("Generated Outside Spawn Test Terrain");
        generationState.Set(new WorldGenerationResult(
            worldIndex: 1,
            actNumber: 1,
            seed: 456,
            usedPrototypePlacement: false,
            generatedRoot: generatedRoot,
            rewardSpawnRequests: new WorldRewardSpawnRequest[0],
            enemySpawnBands: null,
            playerStart: Vector3.zero,
            hasPlayerStart: false,
            headlinerAnchor: Vector3.zero,
            hasHeadlinerAnchor: false,
            stageDoorAnchor: Vector3.zero,
            hasStageDoorAnchor: false,
            bounds: map.Bounds,
            generatedMap: map));
        var indicatorSystem = new EnemySpawnIndicatorSystem(
            _spawnVisualConfig,
            _indicatorFactory,
            _enemyFactory,
            _state,
            _time,
            surfaceResolver: null,
            phaseState: null,
            generationState: generationState,
            worldGenerationConfig: worldConfig);

        indicatorSystem.RequestSpawn(new Vector3(map.PlayableRadius + 32f, 0f, 0f), level: 1, elapsedSeconds: 0f);

        Assert.That(_indicatorFactory.Active, Has.Count.EqualTo(1));
        Assert.That(new Vector2(_indicatorFactory.Active[0].Position.x, _indicatorFactory.Active[0].Position.z).magnitude, Is.LessThanOrEqualTo(map.PlayableRadius));
        Assert.That(_indicatorFactory.Active[0].Position.x, Is.EqualTo(0f).Within(0.0001f));
        Assert.That(_indicatorFactory.Active[0].Position.y, Is.EqualTo(0.04f).Within(0.0001f));
        Assert.That(_indicatorFactory.Active[0].Position.z, Is.EqualTo(0f).Within(0.0001f));

        Object.DestroyImmediate(worldConfig);
    }

    [Test]
    public void MaxEnemyGateCountsPendingIndicators()
    {
        _gameConfig.maxEnemies = 1;
        _gameConfig.maxEnemiesAtPeak = 1;
        _indicatorSystem.RequestSpawn(Vector3.zero, level: 1, elapsedSeconds: 0f);
        EnemySpawnSystem spawnSystem = CreateSpawnSystem();

        spawnSystem.Tick();

        Assert.That(_indicatorFactory.Active, Has.Count.EqualTo(1));
        Assert.That(_registry.Enemies, Is.Empty);
    }

    [Test]
    public void ShowtimePressureRaisesMaxEnemyGate()
    {
        _gameConfig.maxEnemies = 1;
        _gameConfig.maxEnemiesAtPeak = 1;
        _indicatorSystem.RequestSpawn(Vector3.zero, level: 1, elapsedSeconds: 0f);
        RunScheduleConfig scheduleConfig = RunScheduleConfig.CreateRuntimeDefault();
        scheduleConfig.showtimeMaxEnemiesMultiplier = 2f;
        scheduleConfig.showtimePressureSeconds = 30f;
        var schedule = new RunActScheduleState(scheduleConfig);
        schedule.BeginAct(1);
        schedule.Tick(119f);
        schedule.Tick(1f);
        EnemySpawnSystem spawnSystem = CreateSpawnSystem(scheduleState: schedule);

        spawnSystem.Tick();

        Assert.That(_indicatorFactory.Active, Has.Count.EqualTo(2));
        Object.DestroyImmediate(scheduleConfig);
    }

    [Test]
    public void EncoreAllowsEnemySpawning()
    {
        RunScheduleConfig scheduleConfig = RunScheduleConfig.CreateRuntimeDefault();
        var schedule = new RunActScheduleState(scheduleConfig);
        schedule.BeginAct(1);
        schedule.BeginEncore();
        EnemySpawnSystem spawnSystem = CreateSpawnSystem(scheduleState: schedule, phaseState: CreateEncorePhase());

        spawnSystem.Tick();

        Assert.That(_indicatorFactory.Active, Has.Count.EqualTo(1));
        Object.DestroyImmediate(scheduleConfig);
    }

    [Test]
    public void EncorePressureRaisesMaxEnemyGate()
    {
        _gameConfig.maxEnemies = 1;
        _gameConfig.maxEnemiesAtPeak = 1;
        _indicatorSystem.RequestSpawn(Vector3.zero, level: 1, elapsedSeconds: 0f);
        RunScheduleConfig scheduleConfig = RunScheduleConfig.CreateRuntimeDefault();
        scheduleConfig.encoreGraceSeconds = 10f;
        scheduleConfig.encorePressureIntervalSeconds = 30f;
        scheduleConfig.encoreMaxEnemiesMultiplierPerStep = 2f;
        var schedule = new RunActScheduleState(scheduleConfig);
        schedule.BeginAct(1);
        schedule.BeginEncore();
        schedule.TickEncore(40f);
        EnemySpawnSystem spawnSystem = CreateSpawnSystem(scheduleState: schedule, phaseState: CreateEncorePhase());

        spawnSystem.Tick();

        Assert.That(_indicatorFactory.Active, Has.Count.EqualTo(2));
        Object.DestroyImmediate(scheduleConfig);
    }

    [Test]
    public void EncoreGraceDoesNotRaiseMaxEnemyGate()
    {
        _gameConfig.maxEnemies = 1;
        _gameConfig.maxEnemiesAtPeak = 1;
        _indicatorSystem.RequestSpawn(Vector3.zero, level: 1, elapsedSeconds: 0f);
        RunScheduleConfig scheduleConfig = RunScheduleConfig.CreateRuntimeDefault();
        scheduleConfig.encoreGraceSeconds = 10f;
        scheduleConfig.encoreMaxEnemiesMultiplierPerStep = 2f;
        var schedule = new RunActScheduleState(scheduleConfig);
        schedule.BeginAct(1);
        schedule.BeginEncore();
        schedule.TickEncore(9.9f);
        EnemySpawnSystem spawnSystem = CreateSpawnSystem(scheduleState: schedule, phaseState: CreateEncorePhase());

        spawnSystem.Tick();

        Assert.That(_indicatorFactory.Active, Has.Count.EqualTo(1));
        Object.DestroyImmediate(scheduleConfig);
    }

    [Test]
    public void EncorePressureUsesEncoreEnemyPool()
    {
        EnemyDefinition normal = TheCircussyOneTestObjects.CreateEnemyDefinition(_gameConfig);
        normal.enemyId = "normal_enemy";
        normal.displayName = "Normal Enemy";
        EnemyDefinition encore = TheCircussyOneTestObjects.CreateEnemyDefinition(_gameConfig);
        encore.enemyId = "encore_enemy";
        encore.displayName = "Encore Enemy";
        EnemyCatalog catalog = TheCircussyOneTestObjects.CreateEnemyCatalog(normal);
        catalog.encoreEnemies.Add(encore);
        var registry = new ActorRegistry();
        var enemyFactory = new EnemyFactory(
            _gameConfig,
            _damageFeedbackConfig,
            catalog,
            registry,
            TheCircussyOneTestObjects.CreateEnemyPrefab(),
            TheCircussyOneTestObjects.CreateRoot("Encore Enemies").transform);
        var indicatorFactory = new EnemySpawnIndicatorFactory(
            _spawnVisualConfig,
            TheCircussyOneTestObjects.CreateEnemySpawnIndicatorPrefab(),
            TheCircussyOneTestObjects.CreateRoot("Encore Spawn Indicators").transform);
        var indicatorSystem = new EnemySpawnIndicatorSystem(_spawnVisualConfig, indicatorFactory, enemyFactory, _state, _time);
        RunScheduleConfig scheduleConfig = RunScheduleConfig.CreateRuntimeDefault();
        var schedule = new RunActScheduleState(scheduleConfig);
        schedule.BeginAct(1);
        schedule.BeginEncore();
        schedule.ForceEndEncoreGrace();
        EnemySpawnSystem spawnSystem = new EnemySpawnSystem(
            _gameConfig,
            TheCircussyOneTestObjects.CreatePlayer(_gameConfig),
            registry,
            indicatorSystem,
            _state,
            _time,
            CreateEncorePhase(),
            schedule);

        spawnSystem.Tick();

        Assert.That(indicatorFactory.Active, Has.Count.EqualTo(1));
        Assert.That(indicatorFactory.Active[0].EnemyDefinition, Is.SameAs(encore));

        Object.DestroyImmediate(scheduleConfig);
        TheCircussyOneTestObjects.Destroy(catalog);
        TheCircussyOneTestObjects.Destroy(normal);
        TheCircussyOneTestObjects.Destroy(encore);
    }

    [Test]
    public void ResetRunStateRestartsSpawnCooldownAndElapsedDifficulty()
    {
        _gameConfig.spawnIntervalSeconds = 1f;
        _gameConfig.minSpawnIntervalSeconds = 0.1f;
        _gameConfig.spawnRampDurationSeconds = 1f;
        EnemySpawnSystem spawnSystem = CreateSpawnSystem();

        _time.DeltaTime = 10f;
        spawnSystem.Tick();

        Assert.That(_indicatorFactory.Active, Has.Count.EqualTo(1));
        Assert.That(_indicatorFactory.Active[0].EnemyElapsedSeconds, Is.EqualTo(10f).Within(0.0001f));

        _indicatorSystem.Clear();
        spawnSystem.ResetRunState();

        _time.DeltaTime = 0.25f;
        spawnSystem.Tick();

        Assert.That(_indicatorFactory.Active, Is.Empty);

        _time.DeltaTime = 0.75f;
        spawnSystem.Tick();

        Assert.That(_indicatorFactory.Active, Has.Count.EqualTo(1));
        Assert.That(_indicatorFactory.Active[0].EnemyElapsedSeconds, Is.EqualTo(1f).Within(0.0001f));
    }

    private EnemySpawnSystem CreateSpawnSystem(PlayerView player = null, RunActScheduleState scheduleState = null, RunPhaseState phaseState = null)
    {
        player ??= TheCircussyOneTestObjects.CreatePlayer(_gameConfig);
        return new EnemySpawnSystem(_gameConfig, player, _registry, _indicatorSystem, _state, _time, phaseState, scheduleState);
    }

    private static RunPhaseState CreateEncorePhase()
    {
        var phase = new RunPhaseState();
        phase.BeginWorld();
        phase.StartBossWarning();
        phase.ActivateBoss();
        phase.MarkBossDefeated();
        phase.BeginEncore();
        return phase;
    }

    private static void CreateElevatedGround(float y)
    {
        GameObject ground = TheCircussyOneTestObjects.CreateRoot("Elevated Spawn Surface");
        var collider = ground.AddComponent<BoxCollider>();
        collider.center = Vector3.zero;
        collider.size = new Vector3(200f, 0.2f, 200f);
        ground.transform.position = new Vector3(0f, y - 0.1f, 0f);
        Physics.SyncTransforms();
    }

    private static GeneratedWorldMap GeneratedSpawnSafeMap()
    {
        const int Width = 3;
        const int Depth = 3;
        const float Spacing = 10f;
        var samples = new GeneratedWorldSample[Width * Depth];
        for (int z = 0; z < Depth; z++)
        {
            for (int x = 0; x < Width; x++)
            {
                int index = z * Width + x;
                GeneratedWorldMask mask = GeneratedWorldMask.Walkable | GeneratedWorldMask.Reachable;
                if (x == 2 && z == 1)
                {
                    mask |= GeneratedWorldMask.SpawnSafe;
                }

                samples[index] = new GeneratedWorldSample(
                    x == 2 && z == 1 ? 3f : 0f,
                    Vector3.up,
                    0f,
                    mask,
                    0f,
                    1f,
                    1f);
            }
        }

        return new GeneratedWorldMap(
            actNumber: 1,
            seed: 123,
            attemptSeed: 123,
            width: Width,
            depth: Depth,
            playableRadius: 10f,
            gridSpacing: Spacing,
            maxHeight: 3f,
            wallHeightThreshold: 1f,
            samples: samples,
            playerStartIndex: 4,
            usedFallback: false,
            warnings: null);
    }

    private static GeneratedWorldMap GeneratedCenterSpawnSafeMap()
    {
        const int Width = 5;
        const int Depth = 5;
        const float Spacing = 2f;
        var samples = new GeneratedWorldSample[Width * Depth];
        int centerIndex = (Depth / 2) * Width + Width / 2;
        for (int z = 0; z < Depth; z++)
        {
            for (int x = 0; x < Width; x++)
            {
                int index = z * Width + x;
                GeneratedWorldMask mask = GeneratedWorldMask.Walkable | GeneratedWorldMask.Reachable;
                if (index == centerIndex)
                {
                    mask |= GeneratedWorldMask.SpawnSafe | GeneratedWorldMask.PlayerStartSafe;
                }

                samples[index] = new GeneratedWorldSample(
                    0f,
                    Vector3.up,
                    0f,
                    mask,
                    0f,
                    1f,
                    1f);
            }
        }

        return new GeneratedWorldMap(
            actNumber: 1,
            seed: 456,
            attemptSeed: 456,
            width: Width,
            depth: Depth,
            playableRadius: 8f,
            gridSpacing: Spacing,
            maxHeight: 0f,
            wallHeightThreshold: 1f,
            samples: samples,
            playerStartIndex: centerIndex,
            usedFallback: false,
            warnings: null);
    }
}
