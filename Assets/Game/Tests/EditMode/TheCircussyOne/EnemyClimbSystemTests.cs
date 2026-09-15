using NUnit.Framework;
using UnityEngine;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using TheCircussyOne.Runtime;

public sealed class EnemyClimbSystemTests
{
    private GameConfig _config;
    private ActorRegistry _registry;
    private FakeGameTime _time;

    [SetUp]
    public void SetUp()
    {
        TheCircussyOneTestObjects.DestroyAll();
        _config = TheCircussyOneTestObjects.CreateConfig();
        _config.enemyPileClimbingEnabled = false;
        _config.enemyEnvironmentProbeIntervalFrames = 1;
        _registry = new ActorRegistry();
        _time = new FakeGameTime { DeltaTime = 0.5f };
    }

    [TearDown]
    public void TearDown()
    {
        TheCircussyOneTestObjects.DestroyAll();
        TheCircussyOneTestObjects.Destroy(_config);
    }

    [Test]
    public void RaisesEnemyTowardWalkableSampledEnvironmentHeight()
    {
        _config.enemyMoveSpeed = 4f;
        EnemyRuntime enemy = TheCircussyOneTestObjects.CreateEnemy(_config, Vector3.zero);
        _registry.Register(enemy);
        var sampler = new FakeSurfaceSampler(0.2f);

        CreateSystem(sampler).Tick();

        Assert.That(enemy.Position.y, Is.EqualTo(0.2f).Within(0.001f));
        Assert.That(enemy.VerticalMotor.TargetHeight, Is.EqualTo(0.2f).Within(0.001f));
    }

    [Test]
    public void PassiveEnvironmentSampleDoesNotStartWallClimb()
    {
        _config.enemyMoveSpeed = 4f;
        EnemyRuntime enemy = TheCircussyOneTestObjects.CreateEnemy(_config, Vector3.zero);
        _registry.Register(enemy);
        var sampler = new FakeSurfaceSampler(2f);

        CreateSystem(sampler).Tick();

        Assert.That(enemy.Position.y, Is.EqualTo(0f).Within(0.001f));
        Assert.That(enemy.VerticalMotor.TargetHeight, Is.EqualTo(0f).Within(0.001f));
        Assert.That(enemy.VerticalMotor.IsEnvironmentClimbing, Is.False);
    }

    [Test]
    public void FallsWhenSampledTargetDrops()
    {
        _config.enemyMoveSpeed = 6f;
        _config.enemyClimbGravity = 35f;
        _config.enemyClimbTerminalFallSpeed = 28f;
        EnemyRuntime enemy = TheCircussyOneTestObjects.CreateEnemy(_config, Vector3.zero);
        enemy.View.SetHeight(2f);
        enemy.VerticalMotor.Reset(2f);
        _registry.Register(enemy);
        var sampler = new FakeSurfaceSampler(0f);
        EnemyClimbSystem system = CreateSystem(sampler);

        _time.DeltaTime = 0.1f;
        system.Tick();

        Assert.That(enemy.Position.y, Is.LessThan(2f));
        Assert.That(enemy.Position.y, Is.GreaterThan(0f));
        Assert.That(enemy.VerticalMotor.VerticalVelocity, Is.LessThan(0f));
    }

    [Test]
    public void PreservesBlockedClimbTargetUntilReached()
    {
        _config.enemyMoveSpeed = 4f;
        EnemyRuntime enemy = TheCircussyOneTestObjects.CreateEnemy(_config, Vector3.zero);
        enemy.VerticalMotor.BeginEnvironmentClimb(2f, 0.08f);
        _registry.Register(enemy);
        var sampler = new FakeSurfaceSampler(0f);

        CreateSystem(sampler).Tick();

        Assert.That(enemy.Position.y, Is.EqualTo(2f).Within(0.001f));
        Assert.That(enemy.VerticalMotor.TargetHeight, Is.EqualTo(2f).Within(0.001f));
        Assert.That(enemy.VerticalMotor.IsEnvironmentClimbing, Is.False);
        Assert.That(enemy.VerticalMotor.HasCachedEnvironmentHeight, Is.False);
    }

    [Test]
    public void EnvironmentSurfaceSamplerFindsTerrainNearCurrentEnemyHeight()
    {
        GameObject surface = TheCircussyOneTestObjects.CreateRoot("High Generated Terrain");
        var collider = surface.AddComponent<BoxCollider>();
        collider.size = new Vector3(8f, 0.2f, 8f);
        surface.transform.position = new Vector3(0f, 19.9f, 0f);
        Physics.SyncTransforms();
        var sampler = new EnemyEnvironmentSurfaceSampler();

        bool found = sampler.TrySampleHeight(
            new Vector3(0f, 20f, 0f),
            maxHeight: 8f,
            probeDistance: 1.1f,
            layerMask: GameLayers.EnvironmentMaskExcludingGameplay,
            out float height);

        Assert.That(found, Is.True);
        Assert.That(height, Is.EqualTo(20f).Within(0.001f));
    }

    [Test]
    public void EnvironmentProbeStaggeringDoesNotProbeEveryEnemyEveryFrame()
    {
        _config.enemyEnvironmentProbeIntervalFrames = 4;
        EnemyRuntime first = TheCircussyOneTestObjects.CreateEnemy(_config, Vector3.zero, "First");
        EnemyRuntime second = TheCircussyOneTestObjects.CreateEnemy(_config, new Vector3(1f, 0f, 0f), "Second");
        _registry.Register(first);
        _registry.Register(second);
        var sampler = new FakeSurfaceSampler(0f);
        EnemyClimbSystem system = CreateSystem(sampler);

        system.Tick();
        int firstTickCalls = sampler.CallCount;
        system.Tick();

        Assert.That(firstTickCalls, Is.EqualTo(4));
        Assert.That(sampler.CallCount, Is.EqualTo(firstTickCalls));
    }

    [Test]
    public void GroundOnlyStackPolicyDoesNotAddSupportHeight()
    {
        _config.enemyPileClimbingEnabled = true;
        _config.enemyPileRadius = 2.4f;
        _config.enemyPileStartCount = 6;
        _config.enemyPileEnemiesPerLayer = 6;
        _config.enemyPileMaxLayers = 4;
        _config.enemyPileLayerHeightMultiplier = 1f;
        EnemyDefinition definition = TheCircussyOneTestObjects.CreateEnemyDefinition(_config);
        definition.stack.policy = EnemyStackPolicy.GroundOnly;
        definition.stack.canClimbEnemies = false;
        for (int i = 0; i < 6; i++)
        {
            EnemyRuntime enemy = TheCircussyOneTestObjects.CreateEnemy(definition, new Vector3(0.1f * i, 0f, 0f), "Pile" + i, i);
            _registry.Register(enemy);
        }

        CreateSystem(new FakeSurfaceSampler(0f)).Tick();

        Assert.That(_registry.Enemies[0].StackProfile.policy, Is.EqualTo(EnemyStackPolicy.GroundOnly));
        Assert.That(_registry.Enemies[0].VerticalMotor.TargetHeight, Is.EqualTo(0f).Within(0.001f));
        Object.DestroyImmediate(definition);
    }

    [Test]
    public void ActiveSupportClimbRaisesEnemyToSupportBodyTop()
    {
        _config.enemyPileClimbingEnabled = true;
        _config.enemyPileMaxLayers = 2;
        _config.enemyPileLayerHeightMultiplier = 1f;
        _config.enemyMoveSpeed = 8f;
        EnemyDefinition definition = TheCircussyOneTestObjects.CreateEnemyDefinition(_config);
        definition.stack.policy = EnemyStackPolicy.SupportBased;
        definition.stack.canClimbEnemies = true;
        definition.stack.canBeStackedOn = true;
        definition.stack.maxStackLayers = 2;
        definition.stack.layerHeightMultiplier = 1f;
        EnemyRuntime support = TheCircussyOneTestObjects.CreateEnemy(definition, Vector3.zero, "Support", 0);
        EnemyRuntime climber = TheCircussyOneTestObjects.CreateEnemy(definition, new Vector3(0.1f, 0f, 0f), "Climber", 1);
        climber.VerticalMotor.BeginSupportClimb(1.45f, EnemySupportClimbRules.SupportReleaseTolerance, 1, support.SpawnId);
        _registry.Register(support);
        _registry.Register(climber);

        CreateSystem(new FakeSurfaceSampler(0f)).Tick();

        Assert.That(support.VerticalMotor.StackLayer, Is.EqualTo(0));
        Assert.That(climber.VerticalMotor.StackLayer, Is.EqualTo(1));
        Assert.That(climber.VerticalMotor.TargetHeight, Is.EqualTo(1.45f).Within(0.001f));
        Assert.That(climber.Position.y, Is.EqualTo(1.45f).Within(0.001f));
        Object.DestroyImmediate(definition);
    }

    [Test]
    public void SupportClimbUsesStackSpeedMultiplier()
    {
        _config.enemyPileClimbingEnabled = true;
        _config.enemyPileMaxLayers = 2;
        _config.enemyPileLayerHeightMultiplier = 1f;
        _config.enemyMoveSpeed = 8f;
        _time.DeltaTime = 0.1f;
        EnemyDefinition definition = TheCircussyOneTestObjects.CreateEnemyDefinition(_config);
        definition.stack.policy = EnemyStackPolicy.SupportBased;
        definition.stack.canClimbEnemies = true;
        definition.stack.canBeStackedOn = true;
        definition.stack.maxStackLayers = 2;
        definition.stack.layerHeightMultiplier = 1f;
        definition.stack.supportClimbSpeedMultiplier = 0.25f;
        EnemyRuntime support = TheCircussyOneTestObjects.CreateEnemy(definition, Vector3.zero, "Support", 0);
        EnemyRuntime climber = TheCircussyOneTestObjects.CreateEnemy(definition, new Vector3(0.1f, 0f, 0f), "Climber", 1);
        climber.VerticalMotor.BeginSupportClimb(1.45f, EnemySupportClimbRules.SupportReleaseTolerance, 1, support.SpawnId);
        _registry.Register(support);
        _registry.Register(climber);

        CreateSystem(new FakeSurfaceSampler(0f)).Tick();

        Assert.That(climber.Position.y, Is.EqualTo(0.2f).Within(0.001f));
        Assert.That(climber.VerticalMotor.TargetHeight, Is.EqualTo(1.45f).Within(0.001f));
        Object.DestroyImmediate(definition);
    }

    [Test]
    public void RemovingSupportLetsStackedEnemyFall()
    {
        _config.enemyPileClimbingEnabled = true;
        _config.enemyPileStartCount = 1;
        _config.enemyPileEnemiesPerLayer = 1;
        _config.enemyPileMaxLayers = 2;
        _config.enemyClimbGravity = 35f;
        _config.enemyClimbTerminalFallSpeed = 28f;
        _config.enemyMoveSpeed = 8f;
        EnemyDefinition definition = TheCircussyOneTestObjects.CreateEnemyDefinition(_config);
        definition.stack.policy = EnemyStackPolicy.SupportBased;
        definition.stack.canClimbEnemies = true;
        definition.stack.canBeStackedOn = true;
        definition.stack.maxStackLayers = 1;
        definition.stack.layerHeightMultiplier = 1f;
        EnemyRuntime support = TheCircussyOneTestObjects.CreateEnemy(definition, Vector3.zero, "Support", 0);
        EnemyRuntime climber = TheCircussyOneTestObjects.CreateEnemy(definition, new Vector3(0.1f, 0f, 0f), "Climber", 1);
        climber.VerticalMotor.BeginSupportClimb(1.45f, EnemySupportClimbRules.SupportReleaseTolerance, 1, support.SpawnId);
        _registry.Register(support);
        _registry.Register(climber);
        EnemyClimbSystem system = CreateSystem(new FakeSurfaceSampler(0f));

        system.Tick();
        float stackedHeight = climber.Position.y;
        support.SetHealth(0);
        _time.DeltaTime = 0.1f;
        system.Tick();

        Assert.That(stackedHeight, Is.GreaterThan(0f));
        Assert.That(climber.VerticalMotor.StackLayer, Is.EqualTo(0));
        Assert.That(climber.Position.y, Is.LessThan(stackedHeight));
        Assert.That(climber.VerticalMotor.VerticalVelocity, Is.LessThan(0f));
        Object.DestroyImmediate(definition);
    }

    [Test]
    public void DisabledClimbingResetsEnemyHeight()
    {
        _config.enemyClimbingEnabled = false;
        EnemyRuntime enemy = TheCircussyOneTestObjects.CreateEnemy(_config, Vector3.zero);
        enemy.View.SetHeight(2f);
        enemy.VerticalMotor.Reset(2f);
        _registry.Register(enemy);

        CreateSystem(new FakeSurfaceSampler(3f)).Tick();

        Assert.That(enemy.Position.y, Is.EqualTo(0f).Within(0.001f));
        Assert.That(enemy.VerticalMotor.CurrentHeight, Is.EqualTo(0f).Within(0.001f));
    }

    private EnemyClimbSystem CreateSystem(EnemyEnvironmentSurfaceSampler sampler)
    {
        return new EnemyClimbSystem(_config, _registry, _time, sampler);
    }

    private sealed class FakeSurfaceSampler : EnemyEnvironmentSurfaceSampler
    {
        public FakeSurfaceSampler(float height)
        {
            Height = height;
        }

        public float Height { get; set; }
        public int CallCount { get; private set; }

        public override bool TrySampleHeight(Vector3 position, float maxHeight, float probeDistance, int layerMask, out float height)
        {
            CallCount++;
            height = Height;
            return true;
        }
    }
}
