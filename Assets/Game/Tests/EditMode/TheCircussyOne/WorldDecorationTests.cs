using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Runtime;
using UnityEngine;

public sealed class WorldDecorationTests
{
    [TearDown]
    public void TearDown()
    {
        TheCircussyOneTestObjects.DestroyAll();
    }

    [Test]
    public void PlannerBuildsDeterministicRequestsForSameSeed()
    {
        RunWorldGenerationConfig config = DecorationConfig();
        GeneratedWorldMap map = MixedMap();
        WorldDecorationSpec[] specs = TestSpecs();

        IReadOnlyList<WorldDecorationSpawnRequest> first =
            GeneratedWorldDecorationPlanner.BuildRequests(map, Array.Empty<WorldRewardSpawnRequest>(), 123, 1, false, Vector3.zero, specs, config);
        IReadOnlyList<WorldDecorationSpawnRequest> second =
            GeneratedWorldDecorationPlanner.BuildRequests(map, Array.Empty<WorldRewardSpawnRequest>(), 123, 1, false, Vector3.zero, specs, config);

        Assert.That(first.Count, Is.GreaterThan(0));
        Assert.That(second.Count, Is.EqualTo(first.Count));
        for (int i = 0; i < first.Count; i++)
        {
            Assert.That(second[i].Spec.Id, Is.EqualTo(first[i].Spec.Id));
            Assert.That(second[i].Position, Is.EqualTo(first[i].Position));
            Assert.That(second[i].Rotation.eulerAngles, Is.EqualTo(first[i].Rotation.eulerAngles));
            Assert.That(second[i].Scale, Is.EqualTo(first[i].Scale));
        }

        UnityEngine.Object.DestroyImmediate(config);
    }

    [Test]
    public void PlannerVariesRequestsForDifferentSeeds()
    {
        RunWorldGenerationConfig config = DecorationConfig();
        GeneratedWorldMap map = MixedMap();
        WorldDecorationSpec[] specs = TestSpecs();

        IReadOnlyList<WorldDecorationSpawnRequest> first =
            GeneratedWorldDecorationPlanner.BuildRequests(map, Array.Empty<WorldRewardSpawnRequest>(), 123, 1, false, Vector3.zero, specs, config);
        IReadOnlyList<WorldDecorationSpawnRequest> variant =
            GeneratedWorldDecorationPlanner.BuildRequests(map, Array.Empty<WorldRewardSpawnRequest>(), 124, 1, false, Vector3.zero, specs, config);

        Assert.That(first.Count, Is.GreaterThan(0));
        Assert.That(variant.Count, Is.EqualTo(first.Count));
        Assert.That(first.Where((request, index) => request.Position != variant[index].Position || !Mathf.Approximately(request.Scale, variant[index].Scale)).Any(), Is.True);

        UnityEngine.Object.DestroyImmediate(config);
    }

    [Test]
    public void PlannerAvoidsStartRewardsAndStageDoor()
    {
        RunWorldGenerationConfig config = DecorationConfig();
        config.worldDecorationPlayerStartExclusionRadius = 8f;
        config.worldDecorationRewardExclusionRadius = 8f;
        config.worldDecorationStageDoorExclusionRadius = 8f;
        GeneratedWorldMap map = MixedMap();
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
        Vector3 stageDoor = new(-8f, 0f, 0f);

        IReadOnlyList<WorldDecorationSpawnRequest> requests =
            GeneratedWorldDecorationPlanner.BuildRequests(map, rewards, 123, 1, true, stageDoor, TestSpecs(), config);

        Assert.That(requests.Count, Is.GreaterThan(0));
        foreach (WorldDecorationSpawnRequest request in requests)
        {
            Assert.That(Distance2D(request.Position, map.PlayerStartPosition), Is.GreaterThanOrEqualTo(config.WorldDecorationPlayerStartExclusionRadius));
            Assert.That(Distance2D(request.Position, rewards[0].Position), Is.GreaterThanOrEqualTo(config.WorldDecorationRewardExclusionRadius));
            Assert.That(Distance2D(request.Position, stageDoor), Is.GreaterThanOrEqualTo(config.WorldDecorationStageDoorExclusionRadius));
        }

        UnityEngine.Object.DestroyImmediate(config);
    }

    [Test]
    public void PlannerHonorsAllowedSurfaceMasks()
    {
        RunWorldGenerationConfig config = DecorationConfig();
        GeneratedWorldMap map = MixedMap();
        var rampOnly = new[]
        {
            new WorldDecorationSpec(
                "ramp_only",
                "Ramp Only",
                WorldDecorationRole.SmallProp,
                WorldDecorationSurfaceMask.Ramp,
                WorldDecorationShape.BrokenPlank,
                1f,
                3,
                3,
                1f,
                1f,
                0f,
                true,
                89f,
                0f,
                0f,
                0f,
                0f,
                Color.white,
                Color.gray,
                Color.yellow)
        };

        IReadOnlyList<WorldDecorationSpawnRequest> requests =
            GeneratedWorldDecorationPlanner.BuildRequests(map, Array.Empty<WorldRewardSpawnRequest>(), 123, 1, false, Vector3.zero, rampOnly, config);

        Assert.That(requests.Count, Is.EqualTo(3));
        foreach (WorldDecorationSpawnRequest request in requests)
        {
            int index = NearestSampleIndex(map, request.Position);
            Assert.That(map.Samples[index].IsRamp, Is.True);
        }

        UnityEngine.Object.DestroyImmediate(config);
    }

    [Test]
    public void PlannerDoesNotExceedConfiguredMaxCount()
    {
        RunWorldGenerationConfig config = DecorationConfig();
        config.worldDecorationMaxCount = 7;
        GeneratedWorldMap map = MixedMap();
        var denseSpecs = new[]
        {
            DenseSpec("dense_crates", WorldDecorationShape.Crate),
            DenseSpec("dense_barrels", WorldDecorationShape.Barrel)
        };

        IReadOnlyList<WorldDecorationSpawnRequest> requests =
            GeneratedWorldDecorationPlanner.BuildRequests(map, Array.Empty<WorldRewardSpawnRequest>(), 123, 1, false, Vector3.zero, denseSpecs, config);

        Assert.That(requests.Count, Is.GreaterThan(0));
        Assert.That(requests.Count, Is.LessThanOrEqualTo(config.WorldDecorationMaxCount));

        UnityEngine.Object.DestroyImmediate(config);
    }

    [Test]
    public void FactoryCreatesVisualOnlyPropsWithoutColliders()
    {
        RunWorldGenerationConfig config = DecorationConfig();
        GameObject parent = TheCircussyOneTestObjects.CreateRoot("TCO World Decoration Parent");
        var request = new WorldDecorationSpawnRequest(
            TestSpecs()[0],
            Vector3.zero,
            Vector3.up,
            Quaternion.identity,
            1f,
            1);

        GameObject root = new WorldDecorationFactory().Create(parent.transform, new[] { request }, config);

        Assert.That(root, Is.Not.Null);
        Assert.That(root.transform.parent, Is.EqualTo(parent.transform));
        Assert.That(root.GetComponentsInChildren<MeshRenderer>().Length, Is.GreaterThan(0));
        Assert.That(root.GetComponentsInChildren<Collider>().Length, Is.EqualTo(0));

        UnityEngine.Object.DestroyImmediate(config);
    }

    [Test]
    public void SystemCreatesAndClearsDecorationUnderGeneratedRoot()
    {
        RunWorldGenerationConfig config = DecorationConfig();
        GeneratedWorldMap map = MixedMap();
        GameObject generatedRoot = TheCircussyOneTestObjects.CreateRoot("TCO Generated World Root");
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
        var system = new WorldDecorationSystem(config, generationState, new WorldDecorationFactory());

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

    private static RunWorldGenerationConfig DecorationConfig()
    {
        RunWorldGenerationConfig config = ScriptableObject.CreateInstance<RunWorldGenerationConfig>();
        config.ApplyDefaultProfiles();
        config.worldDecorationDensityMultiplier = 1f;
        config.worldDecorationMaxCount = 24;
        config.worldDecorationReferenceRadius = 24f;
        config.worldDecorationPlayerStartExclusionRadius = 0f;
        config.worldDecorationRewardExclusionRadius = 0f;
        config.worldDecorationStageDoorExclusionRadius = 0f;
        config.worldDecorationArenaEdgePadding = 0f;
        config.worldDecorationRampCenterSkipChance = 0f;
        config.worldDecorationSurfaceOffset = 0.03f;
        config.worldDecorationBuildItemsPerFrame = 4;
        return config;
    }

    private static WorldDecorationSpec[] TestSpecs()
    {
        return new[]
        {
            new WorldDecorationSpec(
                "test_crates",
                "Test Crates",
                WorldDecorationRole.SmallProp,
                WorldDecorationSurfaceMask.OrganicGround | WorldDecorationSurfaceMask.TerraceTop | WorldDecorationSurfaceMask.Ramp,
                WorldDecorationShape.Crate,
                1f,
                5,
                5,
                0.8f,
                1.2f,
                360f,
                true,
                89f,
                0f,
                0f,
                0f,
                0f,
                new Color(0.4f, 0.2f, 0.1f, 1f),
                new Color(0.1f, 0.05f, 0.03f, 1f),
                new Color(0.8f, 0.5f, 0.1f, 1f)),
            new WorldDecorationSpec(
                "test_lamps",
                "Test Lamps",
                WorldDecorationRole.SmallProp,
                WorldDecorationSurfaceMask.TerraceTop | WorldDecorationSurfaceMask.HighGround,
                WorldDecorationShape.SmallLamp,
                1f,
                4,
                4,
                0.8f,
                1.2f,
                360f,
                true,
                89f,
                0f,
                0f,
                0f,
                0f,
                Color.black,
                Color.gray,
                Color.yellow)
        };
    }

    private static WorldDecorationSpec DenseSpec(string id, WorldDecorationShape shape)
    {
        return new WorldDecorationSpec(
            id,
            id,
            WorldDecorationRole.SmallProp,
            WorldDecorationSurfaceMask.OrganicGround | WorldDecorationSurfaceMask.TerraceTop | WorldDecorationSurfaceMask.Ramp,
            shape,
            1f,
            100,
            100,
            0.8f,
            1.2f,
            360f,
            true,
            89f,
            0f,
            0f,
            0f,
            0f,
            Color.white,
            Color.gray,
            Color.yellow);
    }

    private static GeneratedWorldMap MixedMap()
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
                if (x == Width / 2)
                {
                    samples[index] = Sample(2f, new Vector3(0.25f, 0.96f, 0f), GeneratedWorldSurfaceKind.Ramp);
                    continue;
                }

                if (x > Width / 2)
                {
                    samples[index] = Sample(4f, Vector3.up, GeneratedWorldSurfaceKind.Flat);
                    continue;
                }

                samples[index] = Sample(0f, Vector3.up, GeneratedWorldSurfaceKind.Continuous);
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
            playerStartIndex: (Depth / 2) * Width + 1,
            usedFallback: false,
            warnings: null);
    }

    private static GeneratedWorldSample Sample(float height, Vector3 normal, GeneratedWorldSurfaceKind surfaceKind)
    {
        return new GeneratedWorldSample(
            height,
            normal,
            Vector3.Angle(normal, Vector3.up),
            GeneratedWorldMask.Walkable
            | GeneratedWorldMask.Reachable
            | GeneratedWorldMask.RewardSafe
            | GeneratedWorldMask.SpawnSafe
            | GeneratedWorldMask.PlayerStartSafe,
            height / 4f,
            1f,
            1f,
            surfaceKind,
            surfaceKind == GeneratedWorldSurfaceKind.Ramp ? Vector2.right : Vector2.zero);
    }

    private static int NearestSampleIndex(GeneratedWorldMap map, Vector3 position)
    {
        int bestIndex = 0;
        float bestDistance = float.MaxValue;
        for (int i = 0; i < map.Samples.Count; i++)
        {
            float distance = Distance2D(map.PositionForIndex(i), position);
            if (distance >= bestDistance)
            {
                continue;
            }

            bestDistance = distance;
            bestIndex = i;
        }

        return bestIndex;
    }

    private static float Distance2D(Vector3 a, Vector3 b)
    {
        float dx = a.x - b.x;
        float dz = a.z - b.z;
        return Mathf.Sqrt(dx * dx + dz * dz);
    }
}
