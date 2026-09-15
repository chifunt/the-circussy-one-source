using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Runtime;
using TheCircussyOne.Rules;

public sealed class EnemyDirectorSystemTests
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
        _time = new FakeGameTime { DeltaTime = 0.25f };
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
    public void TickSeparatesOverlappingEnemies()
    {
        _config.enemySeparationRadius = 2f;
        _config.enemySeparationWeight = 5f;
        var player = TheCircussyOneTestObjects.CreatePlayer(_config);
        player.transform.position = new Vector3(20f, 0f, 0f);

        EnemyRuntime left = TheCircussyOneTestObjects.CreateEnemy(_config, Vector3.zero, "Left");
        EnemyRuntime right = TheCircussyOneTestObjects.CreateEnemy(_config, new Vector3(0.5f, 0f, 0f), "Right");
        _registry.Register(left);
        _registry.Register(right);

        float before = Vector3.Distance(left.Position, right.Position);
        CreateSystem(player).Tick();
        float after = Vector3.Distance(left.Position, right.Position);

        Assert.That(after, Is.GreaterThan(before));
    }

    [Test]
    public void TickClampsEnemyBeforeEnteringPlayerStopDistance()
    {
        _config.enemyMoveSpeed = 10f;
        _config.enemyTurnDegreesPerSecond = 9999f;
        _config.enemyPlayerStopDistance = 1.25f;
        _time.DeltaTime = 1f;

        var player = TheCircussyOneTestObjects.CreatePlayer(_config);
        EnemyRuntime enemy = TheCircussyOneTestObjects.CreateEnemy(_config, new Vector3(0f, 0f, 1.4f));
        enemy.View.transform.rotation = Quaternion.LookRotation(Vector3.back, Vector3.up);
        _registry.Register(enemy);

        CreateSystem(player).Tick();

        float distance = Vector3.Distance(enemy.Position, player.Position);
        Assert.That(distance, Is.GreaterThanOrEqualTo(_config.enemyPlayerStopDistance - 0.001f));
    }

    [Test]
    public void TickTurnsEnemyTowardDesiredDirectionWithoutSnapping()
    {
        _config.enemyMoveSpeed = 0f;
        _config.enemyTurnDegreesPerSecond = 90f;
        _time.DeltaTime = 0.5f;
        var player = TheCircussyOneTestObjects.CreatePlayer(_config);
        player.transform.position = new Vector3(10f, 0f, 0f);
        EnemyRuntime enemy = TheCircussyOneTestObjects.CreateEnemy(_config, Vector3.zero);
        _registry.Register(enemy);

        CreateSystem(player).Tick();

        Assert.That(Quaternion.Angle(Quaternion.identity, enemy.View.transform.rotation), Is.EqualTo(45f).Within(0.001f));
        Assert.That(Vector3.Dot(enemy.View.HorizontalForward, Vector3.right), Is.EqualTo(Mathf.Sqrt(0.5f)).Within(0.001f));
    }

    [Test]
    public void TickMovesAlongCurrentFacingAndScalesSpeedByAlignment()
    {
        _config.enemyMoveSpeed = 4f;
        _config.enemyTurnDegreesPerSecond = 90f;
        _config.enemyPlayerStopDistance = 0.1f;
        _config.enemySeparationWeight = 0f;
        _time.DeltaTime = 0.5f;
        var player = TheCircussyOneTestObjects.CreatePlayer(_config);
        player.transform.position = new Vector3(10f, 0f, 0f);
        EnemyRuntime enemy = TheCircussyOneTestObjects.CreateEnemy(_config, Vector3.zero);
        _registry.Register(enemy);

        CreateSystem(player).Tick();

        Assert.That(enemy.Position.x, Is.EqualTo(1f).Within(0.01f));
        Assert.That(enemy.Position.z, Is.EqualTo(1f).Within(0.01f));
    }

    [Test]
    public void TickDoesNotMoveWhenFacingAwayFromDesiredDirection()
    {
        _config.enemyMoveSpeed = 4f;
        _config.enemyTurnDegreesPerSecond = 1f;
        _config.enemyPlayerStopDistance = 0.1f;
        _config.enemySeparationWeight = 0f;
        _time.DeltaTime = 0.5f;
        var player = TheCircussyOneTestObjects.CreatePlayer(_config);
        player.transform.position = new Vector3(0f, 0f, -10f);
        EnemyRuntime enemy = TheCircussyOneTestObjects.CreateEnemy(_config, Vector3.zero);
        _registry.Register(enemy);

        CreateSystem(player).Tick();

        Assert.That(enemy.Position, Is.EqualTo(Vector3.zero));
    }

    [Test]
    public void TickHoldsHorizontalMovementWhenTerrainBlockRequiresClimb()
    {
        _config.enemyMoveSpeed = 4f;
        _config.enemyTurnDegreesPerSecond = 9999f;
        _config.enemyPlayerStopDistance = 0.1f;
        _config.enemySeparationWeight = 0f;
        _time.DeltaTime = 0.5f;
        var player = TheCircussyOneTestObjects.CreatePlayer(_config);
        player.transform.position = new Vector3(0f, 0f, 10f);
        EnemyRuntime enemy = TheCircussyOneTestObjects.CreateEnemy(_config, Vector3.zero);
        _registry.Register(enemy);
        var sampler = new FakeTerrainSampler(blocksHorizontal: true, targetHeight: 2f);

        CreateSystem(player, surfaceSampler: sampler).Tick();

        Assert.That(enemy.Position.x, Is.EqualTo(0f).Within(0.001f));
        Assert.That(enemy.Position.z, Is.EqualTo(0f).Within(0.001f));
        Assert.That(enemy.VerticalMotor.IsEnvironmentClimbing, Is.True);
        Assert.That(enemy.VerticalMotor.BlockedClimbTargetHeight, Is.EqualTo(2f).Within(0.001f));
    }

    [Test]
    public void TickStartsEnvironmentClimbForHighWorldTargetWithinRelativeRange()
    {
        _config.enemyMoveSpeed = 4f;
        _config.enemyTurnDegreesPerSecond = 9999f;
        _config.enemyPlayerStopDistance = 0.1f;
        _config.enemySeparationWeight = 0f;
        _time.DeltaTime = 0.5f;
        var player = TheCircussyOneTestObjects.CreatePlayer(_config);
        player.transform.position = new Vector3(0f, 27f, 10f);
        EnemyRuntime enemy = TheCircussyOneTestObjects.CreateEnemy(_config, new Vector3(0f, 27f, 0f));
        _registry.Register(enemy);
        var sampler = new FakeTerrainSampler(blocksHorizontal: true, targetHeight: 32f);

        CreateSystem(player, surfaceSampler: sampler).Tick();

        Assert.That(enemy.Position.z, Is.EqualTo(0f).Within(0.001f));
        Assert.That(enemy.VerticalMotor.IsEnvironmentClimbing, Is.True);
        Assert.That(enemy.VerticalMotor.BlockedClimbTargetHeight, Is.EqualTo(32f).Within(0.001f));
    }

    [Test]
    public void TickStartsEnvironmentClimbForTallGeneratedTerrain()
    {
        _config.enemyMoveSpeed = 4f;
        _config.enemyTurnDegreesPerSecond = 9999f;
        _config.enemyPlayerStopDistance = 0.1f;
        _config.enemySeparationWeight = 0f;
        _time.DeltaTime = 0.5f;
        var player = TheCircussyOneTestObjects.CreatePlayer(_config);
        player.transform.position = new Vector3(0f, 27f, 10f);
        EnemyRuntime enemy = TheCircussyOneTestObjects.CreateEnemy(_config, new Vector3(0f, 27f, 0f));
        _registry.Register(enemy);
        var sampler = new FakeTerrainSampler(blocksHorizontal: true, targetHeight: 36f);

        CreateSystem(player, surfaceSampler: sampler).Tick();

        Assert.That(enemy.Position.z, Is.EqualTo(0f).Within(0.001f));
        Assert.That(enemy.VerticalMotor.IsEnvironmentClimbing, Is.True);
        Assert.That(enemy.VerticalMotor.BlockedClimbTargetHeight, Is.EqualTo(36f).Within(0.001f));
    }

    [Test]
    public void TickDoesNotClimbArenaBarrierCollision()
    {
        _config.enemyMoveSpeed = 4f;
        _config.enemyTurnDegreesPerSecond = 9999f;
        _config.enemyPlayerStopDistance = 0.1f;
        _config.enemySeparationWeight = 0f;
        _time.DeltaTime = 0.5f;
        GameObject barrier = new("Arena Barrier Collision");
        try
        {
            barrier.AddComponent<ArenaBarrierCollisionMarker>();
            BoxCollider collider = barrier.AddComponent<BoxCollider>();
            collider.size = new Vector3(4f, 12f, 1f);
            collider.center = new Vector3(0f, 6f, 0f);
            Physics.SyncTransforms();

            var player = TheCircussyOneTestObjects.CreatePlayer(_config);
            player.transform.position = new Vector3(0f, 0f, 10f);
            EnemyRuntime enemy = TheCircussyOneTestObjects.CreateEnemy(_config, Vector3.zero);
            _registry.Register(enemy);
            var sampler = new RaycastBlockTerrainSampler(
                new Vector3(0f, 1f, -3f),
                Vector3.forward,
                6f,
                targetHeight: 32f);

            CreateSystem(player, surfaceSampler: sampler).Tick();

            Assert.That(enemy.Position.z, Is.EqualTo(0f).Within(0.001f));
            Assert.That(enemy.VerticalMotor.IsEnvironmentClimbing, Is.False);
        }
        finally
        {
            Object.DestroyImmediate(barrier);
        }
    }

    [Test]
    public void TickMovesAcrossWalkableBlockedHeight()
    {
        _config.enemyMoveSpeed = 4f;
        _config.enemyTurnDegreesPerSecond = 9999f;
        _config.enemyPlayerStopDistance = 0.1f;
        _config.enemySeparationWeight = 0f;
        _time.DeltaTime = 0.5f;
        var player = TheCircussyOneTestObjects.CreatePlayer(_config);
        player.transform.position = new Vector3(0f, 0f, 10f);
        EnemyRuntime enemy = TheCircussyOneTestObjects.CreateEnemy(_config, Vector3.zero);
        _registry.Register(enemy);
        var sampler = new FakeTerrainSampler(blocksHorizontal: true, targetHeight: 0.2f);

        CreateSystem(player, surfaceSampler: sampler).Tick();

        Assert.That(enemy.Position.z, Is.GreaterThan(0f));
        Assert.That(enemy.VerticalMotor.IsEnvironmentClimbing, Is.False);
    }

    [Test]
    public void TickIgnoresFarAwayInvalidBlockHitPointWhenSamplingClimbTarget()
    {
        _config.enemyMoveSpeed = 4f;
        _config.enemyTurnDegreesPerSecond = 9999f;
        _config.enemyPlayerStopDistance = 0.1f;
        _config.enemySeparationWeight = 0f;
        _time.DeltaTime = 0.5f;
        var player = TheCircussyOneTestObjects.CreatePlayer(_config);
        player.transform.position = new Vector3(-50f, 7f, -40f);
        EnemyRuntime enemy = TheCircussyOneTestObjects.CreateEnemy(_config, new Vector3(-50f, 7f, -50f));
        _registry.Register(enemy);
        var sampler = new PositionSensitiveTerrainSampler(
            localHeight: 7.1f,
            remoteHeight: 32f);

        CreateSystem(player, surfaceSampler: sampler).Tick();

        Assert.That(enemy.Position.z, Is.GreaterThan(-50f));
        Assert.That(enemy.VerticalMotor.IsEnvironmentClimbing, Is.False);
        Assert.That(sampler.SawRemoteOriginSample, Is.False);
    }

    [Test]
    public void TickLogsRampSideBlockerDiagnosticsWhenEnabled()
    {
        _config.enemyMoveSpeed = 4f;
        _config.enemyTurnDegreesPerSecond = 9999f;
        _config.enemyPlayerStopDistance = 0.1f;
        _config.enemySeparationWeight = 0f;
        _config.enemyEnvironmentBlockDiagnosticsEnabled = true;
        _time.DeltaTime = 0.5f;
        GameObject ramp = new("Diagnostic Ramp Side");
        try
        {
            GeneratedRampMarker marker = ramp.AddComponent<GeneratedRampMarker>();
            marker.Initialize(
                Vector3.zero,
                Vector3.forward,
                Vector3.right,
                rampHalfWidth: 2f,
                rampHalfLength: 5f,
                rampLowHeight: 0f,
                rampHighHeight: 2f);
            BoxCollider collider = ramp.AddComponent<BoxCollider>();
            collider.size = new Vector3(4f, 2f, 10f);
            collider.center = new Vector3(0f, 1f, 0f);
            Physics.SyncTransforms();

            var player = TheCircussyOneTestObjects.CreatePlayer(_config);
            player.transform.position = new Vector3(0f, 0f, 10f);
            EnemyRuntime enemy = TheCircussyOneTestObjects.CreateEnemy(_config, Vector3.zero);
            _registry.Register(enemy);
            var sampler = new RaycastBlockTerrainSampler(
                new Vector3(3f, 1f, 0f),
                Vector3.left,
                6f,
                targetHeight: 2f);

            LogAssert.Expect(
                LogType.Log,
                new Regex(@"\[TCO-EnemyBlockDiag\].*collider=Diagnostic Ramp Side.*rampHit=RampSide.*decision=ClimbBlocked"));

            CreateSystem(player, surfaceSampler: sampler).Tick();

            Assert.That(enemy.VerticalMotor.IsEnvironmentClimbing, Is.True);
            Assert.That(enemy.VerticalMotor.BlockedClimbTargetHeight, Is.EqualTo(1f).Within(0.001f));
        }
        finally
        {
            Object.DestroyImmediate(ramp);
        }
    }

    [Test]
    public void TickResumesHorizontalMovementAfterClimbReleaseHeight()
    {
        _config.enemyMoveSpeed = 4f;
        _config.enemyTurnDegreesPerSecond = 9999f;
        _config.enemyPlayerStopDistance = 0.1f;
        _config.enemySeparationWeight = 0f;
        _time.DeltaTime = 0.5f;
        var player = TheCircussyOneTestObjects.CreatePlayer(_config);
        player.transform.position = new Vector3(0f, 0f, 10f);
        EnemyRuntime enemy = TheCircussyOneTestObjects.CreateEnemy(_config, Vector3.zero);
        enemy.View.SetHeight(2f);
        enemy.VerticalMotor.BeginEnvironmentClimb(2f, EnemyTerrainLocomotionRules.ClimbReleaseTolerance);
        _registry.Register(enemy);
        var sampler = new FakeTerrainSampler(blocksHorizontal: false, targetHeight: 0f);

        CreateSystem(player, surfaceSampler: sampler).Tick();

        Assert.That(enemy.Position.z, Is.GreaterThan(0f));
        Assert.That(enemy.Position.y, Is.EqualTo(2f).Within(0.001f));
        Assert.That(enemy.VerticalMotor.IsEnvironmentClimbing, Is.False);
    }

    [Test]
    public void TickHoldsHorizontalMovementWhenEnemyBodyRequiresSupportClimb()
    {
        _config.enemyMoveSpeed = 4f;
        _config.enemyTurnDegreesPerSecond = 9999f;
        _config.enemyPlayerStopDistance = 0.1f;
        _config.enemySeparationWeight = 0f;
        _config.enemyPileClimbingEnabled = true;
        _config.enemyPileLayerHeightMultiplier = 1f;
        _time.DeltaTime = 0.5f;
        var player = TheCircussyOneTestObjects.CreatePlayer(_config);
        player.transform.position = new Vector3(0f, 0f, 10f);
        EnemyRuntime support = TheCircussyOneTestObjects.CreateEnemy(_config, new Vector3(0f, 0f, 0.9f), "Support", 0);
        EnemyRuntime climber = TheCircussyOneTestObjects.CreateEnemy(_config, Vector3.zero, "Climber", 1);
        _registry.Register(support);
        _registry.Register(climber);

        CreateSystem(player).Tick();

        Assert.That(climber.Position.x, Is.EqualTo(0f).Within(0.001f));
        Assert.That(climber.Position.z, Is.EqualTo(0f).Within(0.001f));
        Assert.That(climber.VerticalMotor.IsSupportClimbing, Is.True);
        Assert.That(climber.VerticalMotor.SupportClimbTargetSpawnId, Is.EqualTo(support.SpawnId));
        Assert.That(climber.VerticalMotor.SupportClimbTargetHeight, Is.EqualTo(1.45f).Within(0.001f));
    }

    [Test]
    public void TickResumesHorizontalMovementAfterSupportReleaseHeight()
    {
        _config.enemyMoveSpeed = 4f;
        _config.enemyTurnDegreesPerSecond = 9999f;
        _config.enemyPlayerStopDistance = 0.1f;
        _config.enemySeparationWeight = 0f;
        _config.enemyPileClimbingEnabled = true;
        _config.enemyPileLayerHeightMultiplier = 1f;
        _time.DeltaTime = 0.5f;
        var player = TheCircussyOneTestObjects.CreatePlayer(_config);
        player.transform.position = new Vector3(0f, 0f, 10f);
        EnemyRuntime support = TheCircussyOneTestObjects.CreateEnemy(_config, new Vector3(0f, 0f, 0.9f), "Support", 0);
        EnemyRuntime climber = TheCircussyOneTestObjects.CreateEnemy(_config, Vector3.zero, "Climber", 1);
        climber.View.SetHeight(1.45f);
        climber.VerticalMotor.BeginSupportClimb(1.45f, EnemySupportClimbRules.SupportReleaseTolerance, 1, support.SpawnId);
        _registry.Register(support);
        _registry.Register(climber);

        CreateSystem(player).Tick();

        Assert.That(climber.Position.z, Is.GreaterThan(0f));
        Assert.That(climber.Position.y, Is.EqualTo(1.45f).Within(0.001f));
        Assert.That(climber.VerticalMotor.IsSupportClimbing, Is.True);
    }

    private EnemyDirectorSystem CreateSystem(
        TheCircussyOne.Visuals.PlayerView player,
        GameState state = null,
        EnemyEnvironmentSurfaceSampler surfaceSampler = null)
    {
        state ??= new GameState(_config);
        surfaceSampler ??= new EnemyEnvironmentSurfaceSampler();
        return new EnemyDirectorSystem(_config, player, _registry, state, _time, surfaceSampler);
    }

    private sealed class PositionSensitiveTerrainSampler : EnemyEnvironmentSurfaceSampler
    {
        private readonly float localHeight;
        private readonly float remoteHeight;

        public PositionSensitiveTerrainSampler(float localHeight, float remoteHeight)
        {
            this.localHeight = localHeight;
            this.remoteHeight = remoteHeight;
        }

        public bool SawRemoteOriginSample { get; private set; }

        public override bool TryFindHorizontalBlock(
            Vector3 position,
            Vector3 direction,
            float distance,
            EnemyBodyProfile bodyProfile,
            int layerMask,
            out RaycastHit hit)
        {
            hit = default;
            return true;
        }

        public override bool TrySampleHeight(Vector3 position, float maxHeight, float probeDistance, int layerMask, out float height)
        {
            Vector2 sample = new(position.x, position.z);
            bool isOriginSample = sample.sqrMagnitude <= 4f;
            SawRemoteOriginSample |= isOriginSample;
            height = isOriginSample ? remoteHeight : localHeight;
            return true;
        }
    }

    private sealed class FakeTerrainSampler : EnemyEnvironmentSurfaceSampler
    {
        private readonly bool blocksHorizontal;
        private readonly float targetHeight;

        public FakeTerrainSampler(bool blocksHorizontal, float targetHeight)
        {
            this.blocksHorizontal = blocksHorizontal;
            this.targetHeight = targetHeight;
        }

        public override bool TryFindHorizontalBlock(
            Vector3 position,
            Vector3 direction,
            float distance,
            EnemyBodyProfile bodyProfile,
            int layerMask,
            out RaycastHit hit)
        {
            hit = default;
            return blocksHorizontal;
        }

        public override bool TrySampleHeight(Vector3 position, float maxHeight, float probeDistance, int layerMask, out float height)
        {
            height = targetHeight;
            return true;
        }
    }

    private sealed class RaycastBlockTerrainSampler : EnemyEnvironmentSurfaceSampler
    {
        private readonly Vector3 origin;
        private readonly Vector3 direction;
        private readonly float distance;
        private readonly float targetHeight;

        public RaycastBlockTerrainSampler(Vector3 origin, Vector3 direction, float distance, float targetHeight)
        {
            this.origin = origin;
            this.direction = direction;
            this.distance = distance;
            this.targetHeight = targetHeight;
        }

        public override bool TryFindHorizontalBlock(
            Vector3 position,
            Vector3 moveDirection,
            float moveDistance,
            EnemyBodyProfile bodyProfile,
            int layerMask,
            out RaycastHit hit)
        {
            return Physics.Raycast(origin, direction, out hit, distance, layerMask, QueryTriggerInteraction.Ignore);
        }

        public override bool TrySampleHeight(Vector3 position, float maxHeight, float probeDistance, int layerMask, out float height)
        {
            height = targetHeight;
            return true;
        }
    }
}
