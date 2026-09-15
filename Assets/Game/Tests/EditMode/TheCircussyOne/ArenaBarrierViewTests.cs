using NUnit.Framework;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;
using TheCircussyOne.Runtime;
using TheCircussyOne.Visuals;
using UnityEngine;

public sealed class ArenaBarrierViewTests
{
    private GameObject root;
    private RunWorldGenerationConfig config;

    [SetUp]
    public void SetUp()
    {
        root = new GameObject("Arena Barrier View Test");
        config = RunWorldGenerationConfig.CreateRuntimeDefault();
        config.arenaBarrierSegmentCount = 16;
        config.arenaBarrierDynamicRingCount = 3;
        config.arenaBarrierHeight = 12f;
        config.arenaBarrierDynamicRingSpeed = 3f;
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(root);
        Object.DestroyImmediate(config);
    }

    [Test]
    public void ConfigureCreatesStaticAndDynamicShapeLayers()
    {
        ArenaBarrierView view = root.AddComponent<ArenaBarrierView>();

        view.Configure(32f, config);

        Assert.That(view.Radius, Is.EqualTo(32f).Within(0.0001f));
        Assert.That(view.StaticLineCount, Is.EqualTo(16));
        Assert.That(view.DynamicRingCount, Is.EqualTo(3));
        Assert.That(view.BaseRingRadius, Is.EqualTo(32f).Within(0.0001f));
    }

    [Test]
    public void ApplyFrameMovesDynamicRingsUpTheBarrier()
    {
        ArenaBarrierView view = root.AddComponent<ArenaBarrierView>();

        view.Configure(32f, config);
        view.ApplyFrame(1f, config);

        Assert.That(view.DynamicRingHeight(0), Is.EqualTo(3f).Within(0.0001f));
        Assert.That(view.DynamicRingHeight(1), Is.EqualTo(7f).Within(0.0001f));
        Assert.That(view.DynamicRingHeight(2), Is.EqualTo(11f).Within(0.0001f));
    }
}

public sealed class ArenaBarrierCollisionTests
{
    private RunWorldGenerationConfig config;
    private GameConfig gameConfig;
    private RunWorldGenerationState generationState;
    private ArenaBarrierSystem system;

    [SetUp]
    public void SetUp()
    {
        TheCircussyOneTestObjects.DestroyAll();
        config = RunWorldGenerationConfig.CreateRuntimeDefault();
        config.arenaBarrierSegmentCount = 32;
        config.arenaBarrierCollisionThickness = 4f;
        config.arenaBarrierCollisionDepthBelowGround = 8f;
        config.arenaBarrierCollisionTopPadding = 16f;
        config.arenaBarrierCollisionMinimumTop = 48f;
        gameConfig = ScriptableObject.CreateInstance<GameConfig>();
        generationState = new RunWorldGenerationState();
        system = new ArenaBarrierSystem(config, gameConfig, generationState, new FakeGameTime());
    }

    [TearDown]
    public void TearDown()
    {
        system?.Clear();
        Object.DestroyImmediate(config);
        Object.DestroyImmediate(gameConfig);
        TheCircussyOneTestObjects.DestroyAll();
    }

    [Test]
    public void CollisionSpecUsesVisualRadiusAndGeneratedHeight()
    {
        ArenaBarrierCollisionSpec spec = ArenaBarrierCollisionRules.BuildSpec(32f, 40f, config);

        Assert.That(spec.InnerRadius, Is.EqualTo(32f).Within(0.0001f));
        Assert.That(spec.OuterRadius, Is.EqualTo(36f).Within(0.0001f));
        Assert.That(spec.BottomY, Is.EqualTo(-8f).Within(0.0001f));
        Assert.That(spec.TopY, Is.EqualTo(56f).Within(0.0001f));
        Assert.That(spec.SegmentCount, Is.EqualTo(32));
    }

    [Test]
    public void CollisionMeshBuildsClosedAnnularWall()
    {
        var spec = new ArenaBarrierCollisionSpec(10f, 14f, -6f, 30f, 24);
        Mesh mesh = ArenaBarrierCollisionMeshBuilder.Build(spec);

        Assert.That(mesh.vertexCount, Is.EqualTo(24 * 4));
        Assert.That(mesh.triangles.Length, Is.EqualTo(24 * 24));
        Assert.That(mesh.bounds.min.y, Is.EqualTo(-6f).Within(0.001f));
        Assert.That(mesh.bounds.max.y, Is.EqualTo(30f).Within(0.001f));
        Assert.That(mesh.bounds.extents.x, Is.EqualTo(14f).Within(0.001f));
        Assert.That(mesh.bounds.extents.z, Is.EqualTo(14f).Within(0.001f));

        Object.DestroyImmediate(mesh);
    }

    [Test]
    public void GeneratedWorldCreatesCollisionUnderGeneratedRoot()
    {
        GameObject generatedRoot = new GameObject("TCO Test Generated Root");
        generationState.Set(GeneratedResult(generatedRoot, playableRadius: 20f, maxHeight: 24f));

        system.RefreshForCurrentWorld();

        Assert.That(system.HasView, Is.True);
        Assert.That(system.HasCollision, Is.True);
        Assert.That(system.CollisionCollider, Is.TypeOf<MeshCollider>());
        Assert.That(system.CollisionCollider.transform.parent, Is.EqualTo(generatedRoot.transform));
        Assert.That(system.CollisionCollider.GetComponentInParent<ArenaBarrierCollisionMarker>(), Is.Not.Null);
    }

    [Test]
    public void TickDoesNotCreateCollisionBeforeLifecycleRefresh()
    {
        GameObject generatedRoot = new GameObject("TCO Test Generated Root");
        generationState.Set(GeneratedResult(generatedRoot, playableRadius: 20f, maxHeight: 24f));

        system.Tick();

        Assert.That(system.HasView, Is.False);
        Assert.That(system.HasCollision, Is.False);
    }

    [Test]
    public void TickClearsDestroyedBarrierViewReference()
    {
        GameObject generatedRoot = new GameObject("TCO Test Generated Root");
        generationState.Set(GeneratedResult(generatedRoot, playableRadius: 20f, maxHeight: 24f));
        system.RefreshForCurrentWorld();
        Assert.That(system.HasView, Is.True);

        Object.DestroyImmediate(generatedRoot);

        Assert.DoesNotThrow(() => system.Tick());
        Assert.That(system.HasView, Is.False);
    }

    [Test]
    public void DisabledCollisionKeepsVisualButCreatesNoCollider()
    {
        config.arenaBarrierCollisionEnabled = false;
        GameObject generatedRoot = new GameObject("TCO Test Generated Root");
        generationState.Set(GeneratedResult(generatedRoot, playableRadius: 20f, maxHeight: 24f));

        system.RefreshForCurrentWorld();

        Assert.That(system.HasView, Is.True);
        Assert.That(system.HasCollision, Is.False);
        Assert.That(system.CollisionCollider, Is.Null);
    }

    [Test]
    public void CollisionBlocksActorSizedCapsuleAtDifferentHeights()
    {
        GameObject generatedRoot = new GameObject("TCO Test Generated Root");
        generationState.Set(GeneratedResult(generatedRoot, playableRadius: 12f, maxHeight: 24f));
        system.RefreshForCurrentWorld();
        Physics.SyncTransforms();

        var query = new WorldPhysicsQuery();

        AssertBlocksCapsuleAtHeight(query, 1f);
        AssertBlocksCapsuleAtHeight(query, 16f);
        AssertBlocksCapsuleAtHeight(query, 40f);
    }

    [Test]
    public void ClearRemovesCollision()
    {
        GameObject generatedRoot = new GameObject("TCO Test Generated Root");
        generationState.Set(GeneratedResult(generatedRoot, playableRadius: 20f, maxHeight: 24f));
        system.RefreshForCurrentWorld();

        system.Clear();

        Assert.That(system.HasView, Is.False);
        Assert.That(system.HasCollision, Is.False);
        Assert.That(system.CollisionCollider, Is.Null);
    }

    private void AssertBlocksCapsuleAtHeight(WorldPhysicsQuery query, float rootHeight)
    {
        Vector3 pointA = new(0f, rootHeight + 1.5f, 0f);
        Vector3 pointB = new(0f, rootHeight + 0.5f, 0f);
        bool hit = query.TryHitCapsulePath(
            pointA,
            pointB,
            0.5f,
            Vector3.right,
            40f,
            GameLayers.EnvironmentMaskExcludingGameplay,
            out RaycastHit result);

        Assert.That(hit, Is.True, $"Expected barrier hit at root height {rootHeight:0.##}.");
        Assert.That(result.collider, Is.SameAs(system.CollisionCollider));
    }

    private static WorldGenerationResult GeneratedResult(GameObject root, float playableRadius, float maxHeight)
    {
        var samples = new[]
        {
            new GeneratedWorldSample(
                0f,
                Vector3.up,
                0f,
                GeneratedWorldMask.Walkable | GeneratedWorldMask.Reachable | GeneratedWorldMask.PlayerStartSafe,
                0f,
                1f,
                1f)
        };
        var map = new GeneratedWorldMap(
            actNumber: 1,
            seed: 123,
            attemptSeed: 123,
            width: 1,
            depth: 1,
            playableRadius: playableRadius,
            gridSpacing: 4f,
            maxHeight: maxHeight,
            wallHeightThreshold: 1.25f,
            samples: samples,
            playerStartIndex: 0,
            usedFallback: false,
            warnings: null);

        return new WorldGenerationResult(
            worldIndex: 1,
            actNumber: 1,
            seed: 123,
            usedPrototypePlacement: false,
            generatedRoot: root,
            rewardSpawnRequests: null,
            enemySpawnBands: null,
            playerStart: Vector3.zero,
            hasPlayerStart: true,
            headlinerAnchor: Vector3.zero,
            hasHeadlinerAnchor: false,
            stageDoorAnchor: Vector3.zero,
            hasStageDoorAnchor: false,
            bounds: map.Bounds,
            generatedMap: map);
    }
}
