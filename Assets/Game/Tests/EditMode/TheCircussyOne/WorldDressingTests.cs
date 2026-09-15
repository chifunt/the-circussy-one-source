using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Runtime;
using UnityEngine;

public sealed class WorldDressingTests
{
    [TearDown]
    public void TearDown()
    {
        TheCircussyOneTestObjects.DestroyAll();
    }

    [Test]
    public void PlannerBuildsDeterministicRequestsForSameSeed()
    {
        RunWorldGenerationConfig config = DressingConfig();
        GeneratedWorldMap map = FlatMap();

        IReadOnlyList<GeneratedWorldDressingRequest> first =
            GeneratedWorldDressingPlanner.BuildRequests(map, Array.Empty<WorldRewardSpawnRequest>(), 123, 1, config);
        IReadOnlyList<GeneratedWorldDressingRequest> second =
            GeneratedWorldDressingPlanner.BuildRequests(map, Array.Empty<WorldRewardSpawnRequest>(), 123, 1, config);

        Assert.That(first.Count, Is.GreaterThan(0));
        Assert.That(second.Count, Is.EqualTo(first.Count));
        for (int i = 0; i < first.Count; i++)
        {
            Assert.That(second[i].Kind, Is.EqualTo(first[i].Kind));
            Assert.That(second[i].Position, Is.EqualTo(first[i].Position));
            Assert.That(second[i].Normal, Is.EqualTo(first[i].Normal));
            Assert.That(second[i].Size, Is.EqualTo(first[i].Size));
            Assert.That(second[i].VariantIndex, Is.EqualTo(first[i].VariantIndex));
        }

        UnityEngine.Object.DestroyImmediate(config);
    }

    [Test]
    public void PlannerVariesRequestsForDifferentSeeds()
    {
        RunWorldGenerationConfig config = DressingConfig();
        GeneratedWorldMap map = FlatMap();

        IReadOnlyList<GeneratedWorldDressingRequest> first =
            GeneratedWorldDressingPlanner.BuildRequests(map, Array.Empty<WorldRewardSpawnRequest>(), 123, 1, config);
        IReadOnlyList<GeneratedWorldDressingRequest> variant =
            GeneratedWorldDressingPlanner.BuildRequests(map, Array.Empty<WorldRewardSpawnRequest>(), 124, 1, config);

        Assert.That(first.Count, Is.GreaterThan(0));
        Assert.That(variant.Count, Is.EqualTo(first.Count));
        Assert.That(first.Where((request, index) => request.Position != variant[index].Position).Any(), Is.True);

        UnityEngine.Object.DestroyImmediate(config);
    }

    [Test]
    public void PlannerAvoidsPlayerStartAndRewardExclusionZones()
    {
        RunWorldGenerationConfig config = DressingConfig();
        config.worldDressingPlayerStartExclusionRadius = 8f;
        config.worldDressingRewardExclusionRadius = 8f;
        GeneratedWorldMap map = FlatMap();
        var rewards = new[]
        {
            new WorldRewardSpawnRequest(
                "test_reward",
                WorldRewardPlacementKind.TicketDeposit,
                targetContentId: null,
                new Vector3(8f, 0f, 0f),
                required: false,
                generated: true)
        };

        IReadOnlyList<GeneratedWorldDressingRequest> requests =
            GeneratedWorldDressingPlanner.BuildRequests(map, rewards, 123, 1, config);

        Assert.That(requests.Count, Is.GreaterThan(0));
        foreach (GeneratedWorldDressingRequest request in requests.Where(static request => request.Kind == GeneratedWorldDressingKind.GroundDecal))
        {
            Assert.That(Distance2D(request.Position, map.PlayerStartPosition), Is.GreaterThanOrEqualTo(config.WorldDressingPlayerStartExclusionRadius));
            Assert.That(Distance2D(request.Position, rewards[0].Position), Is.GreaterThanOrEqualTo(config.WorldDressingRewardExclusionRadius));
        }

        UnityEngine.Object.DestroyImmediate(config);
    }

    [Test]
    public void PlannerBuildsWallDressingFromHeightBreaks()
    {
        RunWorldGenerationConfig config = DressingConfig();
        GeneratedWorldMap map = SteppedMap();

        IReadOnlyList<GeneratedWorldDressingRequest> requests =
            GeneratedWorldDressingPlanner.BuildRequests(map, Array.Empty<WorldRewardSpawnRequest>(), 123, 1, config);
        GeneratedWorldDressingRequest[] wallRequests =
            requests.Where(static request => request.Kind == GeneratedWorldDressingKind.WallDecal).ToArray();

        Assert.That(wallRequests.Length, Is.GreaterThan(0));
        Assert.That(wallRequests.All(static request => Mathf.Abs(request.Normal.y) < 0.001f), Is.True);
        Assert.That(wallRequests.All(static request => request.Position.y > 0f), Is.True);

        UnityEngine.Object.DestroyImmediate(config);
    }

    [Test]
    public void PlannerPlacesWallDressingOnRenderedWallEdges()
    {
        RunWorldGenerationConfig config = DressingConfig();
        GeneratedWorldMap map = SteppedMap();
        float renderedWallX = map.PositionFor(3, 0).x;
        float oldSampleMidpointX = (map.PositionFor(3, 0).x + map.PositionFor(4, 0).x) * 0.5f;

        IReadOnlyList<GeneratedWorldDressingRequest> requests =
            GeneratedWorldDressingPlanner.BuildRequests(map, Array.Empty<WorldRewardSpawnRequest>(), 123, 1, config);
        GeneratedWorldDressingRequest[] wallRequests =
            requests.Where(static request => request.Kind == GeneratedWorldDressingKind.WallDecal).ToArray();

        Assert.That(wallRequests.Length, Is.GreaterThan(0));
        Assert.That(
            wallRequests.All(request => Mathf.Abs(AttachedWallPositionX(request, config) - renderedWallX) < 0.01f),
            Is.True);
        Assert.That(
            wallRequests.Any(request => Mathf.Abs(AttachedWallPositionX(request, config) - oldSampleMidpointX) < 0.01f),
            Is.False);

        UnityEngine.Object.DestroyImmediate(config);
    }

    [Test]
    public void PlannerDoesNotExceedGroundAndWallCaps()
    {
        RunWorldGenerationConfig config = DressingConfig();
        config.worldDressingGroundDecalsPer100k = 50000f;
        config.worldDressingGroundDecalMaxCount = 5;
        config.worldDressingWallDecalsPer100k = 50000f;
        config.worldDressingWallDecalMaxCount = 6;
        GeneratedWorldMap map = DenseSteppedMap();

        IReadOnlyList<GeneratedWorldDressingRequest> requests =
            GeneratedWorldDressingPlanner.BuildRequests(map, Array.Empty<WorldRewardSpawnRequest>(), 123, 1, config);
        int groundCount = requests.Count(static request => request.Kind == GeneratedWorldDressingKind.GroundDecal);
        int wallCount = requests.Count(static request => request.Kind == GeneratedWorldDressingKind.WallDecal);

        Assert.That(groundCount, Is.GreaterThan(0));
        Assert.That(wallCount, Is.GreaterThan(0));
        Assert.That(groundCount, Is.LessThanOrEqualTo(config.WorldDressingGroundDecalMaxCount));
        Assert.That(wallCount, Is.LessThanOrEqualTo(config.WorldDressingWallDecalMaxCount));

        UnityEngine.Object.DestroyImmediate(config);
    }

    [Test]
    public void FactoryBuildsWallDressingAsVerticalQuads()
    {
        RunWorldGenerationConfig config = DressingConfig();
        var parent = TheCircussyOneTestObjects.CreateRoot("TCO World Dressing Parent");
        var requests = new[]
        {
            new GeneratedWorldDressingRequest(
                GeneratedWorldDressingKind.WallDecal,
                new Vector3(0f, 2f, 3f),
                Vector3.forward,
                Vector3.right,
                new Vector2(4f, 2f),
                0,
                2)
        };

        GameObject root = new WorldDressingFactory().Create(parent.transform, requests, config, 123, 1);
        Transform wallChild = root.transform.Find("Wall Dressing Decals");
        Mesh mesh = wallChild.GetComponent<MeshFilter>().sharedMesh;
        Vector3[] vertices = mesh.vertices;

        Assert.That(vertices.Select(static vertex => vertex.z), Is.All.EqualTo(3f).Within(0.001f));
        Assert.That(vertices.Max(static vertex => vertex.y) - vertices.Min(static vertex => vertex.y), Is.EqualTo(2f).Within(0.001f));
        Assert.That(vertices.Max(static vertex => vertex.x) - vertices.Min(static vertex => vertex.x), Is.EqualTo(4f).Within(0.001f));

        UnityEngine.Object.DestroyImmediate(config);
    }

    [Test]
    public void FactoryCreatesVisualOnlyMeshesWithoutColliders()
    {
        RunWorldGenerationConfig config = DressingConfig();
        var parent = TheCircussyOneTestObjects.CreateRoot("TCO World Dressing Parent");
        var requests = new[]
        {
            new GeneratedWorldDressingRequest(
                GeneratedWorldDressingKind.GroundDecal,
                Vector3.up * 0.05f,
                Vector3.up,
                Vector3.right,
                new Vector2(2f, 3f),
                0,
                1),
            new GeneratedWorldDressingRequest(
                GeneratedWorldDressingKind.WallDecal,
                new Vector3(0f, 2f, 0f),
                Vector3.forward,
                Vector3.right,
                new Vector2(2f, 2f),
                0,
                2)
        };

        GameObject root = new WorldDressingFactory().Create(parent.transform, requests, config, 123, 1);

        Assert.That(root, Is.Not.Null);
        Assert.That(root.transform.parent, Is.EqualTo(parent.transform));
        Assert.That(root.GetComponentsInChildren<MeshRenderer>().Length, Is.EqualTo(2));
        Assert.That(root.GetComponentsInChildren<Collider>().Length, Is.EqualTo(0));
        Assert.That(root.GetComponentsInChildren<MeshFilter>().All(static filter => filter.sharedMesh != null), Is.True);

        UnityEngine.Object.DestroyImmediate(config);
    }

    [Test]
    public void SystemCreatesAndClearsDressingUnderGeneratedRoot()
    {
        RunWorldGenerationConfig config = DressingConfig();
        GeneratedWorldMap map = SteppedMap();
        var generatedRoot = TheCircussyOneTestObjects.CreateRoot("TCO Generated World Root");
        var generationState = new RunWorldGenerationState();
        generationState.Set(new WorldGenerationResult(
            worldIndex: 1,
            actNumber: 1,
            seed: 123,
            usedPrototypePlacement: false,
            generatedRoot: generatedRoot,
            rewardSpawnRequests: Array.Empty<WorldRewardSpawnRequest>(),
            enemySpawnBands: null,
            playerStart: map.PlayerStartPosition,
            hasPlayerStart: true,
            headlinerAnchor: Vector3.zero,
            hasHeadlinerAnchor: false,
            stageDoorAnchor: Vector3.zero,
            hasStageDoorAnchor: false,
            bounds: map.Bounds,
            generatedMap: map));
        var system = new WorldDressingSystem(config, generationState, new WorldDressingFactory());

        system.RefreshForCurrentWorld();

        Assert.That(system.ActiveRoot, Is.Not.Null);
        Assert.That(system.ActiveRequestCount, Is.GreaterThan(0));
        Assert.That(system.ActiveRoot.transform.parent, Is.EqualTo(generatedRoot.transform));
        Assert.That(generatedRoot.transform.childCount, Is.GreaterThan(0));

        system.Clear();

        Assert.That(system.ActiveRoot, Is.Null);
        Assert.That(system.ActiveRequestCount, Is.EqualTo(0));
        Assert.That(generatedRoot.transform.childCount, Is.EqualTo(0));

        UnityEngine.Object.DestroyImmediate(config);
    }

    private static RunWorldGenerationConfig DressingConfig()
    {
        RunWorldGenerationConfig config = ScriptableObject.CreateInstance<RunWorldGenerationConfig>();
        config.ApplyDefaultProfiles();
        config.worldDressingGroundDecalsPer100k = 12000f;
        config.worldDressingGroundDecalMaxCount = 12;
        config.worldDressingWallDecalsPer100k = 12000f;
        config.worldDressingWallDecalMaxCount = 12;
        config.worldDressingPlayerStartExclusionRadius = 0f;
        config.worldDressingRewardExclusionRadius = 0f;
        config.worldDressingArenaEdgePadding = 0f;
        config.worldDressingTextureResolution = 64;
        config.worldDressingBuildItemsPerFrame = 4;
        return config;
    }

    private static GeneratedWorldMap FlatMap()
    {
        const int Width = 9;
        const int Depth = 9;
        const float Spacing = 4f;
        var samples = new GeneratedWorldSample[Width * Depth];
        for (int z = 0; z < Depth; z++)
        {
            for (int x = 0; x < Width; x++)
            {
                int index = z * Width + x;
                samples[index] = Sample(0f, Vector3.up);
            }
        }

        return new GeneratedWorldMap(
            actNumber: 1,
            seed: 7,
            attemptSeed: 7,
            width: Width,
            depth: Depth,
            playableRadius: 24f,
            gridSpacing: Spacing,
            maxHeight: 8f,
            wallHeightThreshold: 1f,
            samples: samples,
            playerStartIndex: (Depth / 2) * Width + Width / 2,
            usedFallback: false,
            warnings: null);
    }

    private static GeneratedWorldMap SteppedMap()
    {
        const int Width = 8;
        const int Depth = 6;
        const float Spacing = 4f;
        var samples = new GeneratedWorldSample[Width * Depth];
        for (int z = 0; z < Depth; z++)
        {
            for (int x = 0; x < Width; x++)
            {
                float height = x >= Width / 2 ? 8f : 0f;
                int index = z * Width + x;
                samples[index] = Sample(height, Vector3.up);
            }
        }

        return new GeneratedWorldMap(
            actNumber: 1,
            seed: 11,
            attemptSeed: 11,
            width: Width,
            depth: Depth,
            playableRadius: 24f,
            gridSpacing: Spacing,
            maxHeight: 8f,
            wallHeightThreshold: 1f,
            samples: samples,
            playerStartIndex: 0,
            usedFallback: false,
            warnings: null);
    }

    private static GeneratedWorldMap DenseSteppedMap()
    {
        const int Width = 18;
        const int Depth = 18;
        const float Spacing = 4f;
        var samples = new GeneratedWorldSample[Width * Depth];
        for (int z = 0; z < Depth; z++)
        {
            for (int x = 0; x < Width; x++)
            {
                float height = (x / 3 + z / 3) % 2 == 0 ? 0f : 8f;
                int index = z * Width + x;
                samples[index] = Sample(height, Vector3.up);
            }
        }

        return new GeneratedWorldMap(
            actNumber: 1,
            seed: 19,
            attemptSeed: 19,
            width: Width,
            depth: Depth,
            playableRadius: 42f,
            gridSpacing: Spacing,
            maxHeight: 8f,
            wallHeightThreshold: 1f,
            samples: samples,
            playerStartIndex: (Depth / 2) * Width + Width / 2,
            usedFallback: false,
            warnings: null);
    }

    private static GeneratedWorldSample Sample(float height, Vector3 normal)
    {
        return new GeneratedWorldSample(
            height,
            normal,
            0f,
            GeneratedWorldMask.Walkable
            | GeneratedWorldMask.Reachable
            | GeneratedWorldMask.RewardSafe
            | GeneratedWorldMask.SpawnSafe
            | GeneratedWorldMask.PlayerStartSafe,
            height / 8f,
            1f,
            1f);
    }

    private static float Distance2D(Vector3 a, Vector3 b)
    {
        float dx = a.x - b.x;
        float dz = a.z - b.z;
        return Mathf.Sqrt(dx * dx + dz * dz);
    }

    private static float AttachedWallPositionX(GeneratedWorldDressingRequest request, RunWorldGenerationConfig config)
    {
        return request.Position.x - request.Normal.x * config.WorldDressingWallSurfaceOffset;
    }
}
