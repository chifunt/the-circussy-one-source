using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using UnityEngine;
using UnityEngine.Rendering;

namespace TheCircussyOne.Runtime
{
    public readonly struct WorldDecorationSpawnRequest
    {
        public WorldDecorationSpawnRequest(
            WorldDecorationSpec spec,
            Vector3 position,
            Vector3 normal,
            Quaternion rotation,
            float scale,
            int seed)
        {
            Spec = spec;
            Position = position;
            Normal = normal.sqrMagnitude > 0.0001f ? normal.normalized : Vector3.up;
            Rotation = rotation;
            Scale = Mathf.Max(0.01f, scale);
            Seed = seed;
        }

        public WorldDecorationSpec Spec { get; }
        public Vector3 Position { get; }
        public Vector3 Normal { get; }
        public Quaternion Rotation { get; }
        public float Scale { get; }
        public int Seed { get; }
    }

    public static class GeneratedWorldDecorationPlanner
    {
        public static IReadOnlyList<WorldDecorationSpawnRequest> BuildRequests(
            WorldGenerationResult result,
            IReadOnlyList<WorldDecorationSpec> specs,
            RunWorldGenerationConfig config)
        {
            if (config == null
                || !config.WorldDecorationEnabled
                || result.UsedPrototypePlacement
                || result.GeneratedMap == null
                || specs == null
                || specs.Count == 0)
            {
                return Array.Empty<WorldDecorationSpawnRequest>();
            }

            return BuildRequests(
                result.GeneratedMap,
                result.RewardSpawnRequests,
                result.ObstacleSpawnRequests,
                result.Seed,
                result.ActNumber,
                result.HasStageDoorAnchor,
                result.StageDoorAnchor,
                specs,
                config);
        }

        public static IReadOnlyList<WorldDecorationSpawnRequest> BuildRequests(
            GeneratedWorldMap map,
            IReadOnlyList<WorldRewardSpawnRequest> rewardRequests,
            int seed,
            int actNumber,
            bool hasStageDoorAnchor,
            Vector3 stageDoorAnchor,
            IReadOnlyList<WorldDecorationSpec> specs,
            RunWorldGenerationConfig config)
        {
            return BuildRequests(
                map,
                rewardRequests,
                obstacleRequests: null,
                seed,
                actNumber,
                hasStageDoorAnchor,
                stageDoorAnchor,
                specs,
                config);
        }

        public static IReadOnlyList<WorldDecorationSpawnRequest> BuildRequests(
            GeneratedWorldMap map,
            IReadOnlyList<WorldRewardSpawnRequest> rewardRequests,
            IReadOnlyList<WorldBlockingObstacleSpawnRequest> obstacleRequests,
            int seed,
            int actNumber,
            bool hasStageDoorAnchor,
            Vector3 stageDoorAnchor,
            IReadOnlyList<WorldDecorationSpec> specs,
            RunWorldGenerationConfig config)
        {
            if (map == null || config == null || !config.WorldDecorationEnabled || specs == null || specs.Count == 0)
            {
                return Array.Empty<WorldDecorationSpawnRequest>();
            }

            var requests = new List<WorldDecorationSpawnRequest>();
            var placed = new List<PlacedDecoration>();
            int maxTotal = config.WorldDecorationMaxCount;
            for (int i = 0; i < specs.Count && requests.Count < maxTotal; i++)
            {
                WorldDecorationSpec spec = specs[i];
                AddRequestsForSpec(
                    map,
                    rewardRequests,
                    obstacleRequests,
                    seed,
                    actNumber,
                    hasStageDoorAnchor,
                    stageDoorAnchor,
                    config,
                    spec,
                    i,
                    requests,
                    placed,
                    maxTotal);
            }

            return requests;
        }

        private static void AddRequestsForSpec(
            GeneratedWorldMap map,
            IReadOnlyList<WorldRewardSpawnRequest> rewardRequests,
            IReadOnlyList<WorldBlockingObstacleSpawnRequest> obstacleRequests,
            int seed,
            int actNumber,
            bool hasStageDoorAnchor,
            Vector3 stageDoorAnchor,
            RunWorldGenerationConfig config,
            WorldDecorationSpec spec,
            int specIndex,
            List<WorldDecorationSpawnRequest> requests,
            List<PlacedDecoration> placed,
            int maxTotal)
        {
            int targetCount = ResolveTargetCount(map, spec, seed, actNumber, config);
            if (targetCount <= 0 || spec.Weight <= 0f)
            {
                return;
            }

            var candidates = new List<ScoredDecorationCandidate>();
            int planSeed = DeterministicSeed.Combine(seed, actNumber, config.WorldDecorationSeedOffset, specIndex, HashString(spec.Id));
            for (int i = 0; i < map.Samples.Count; i++)
            {
                GeneratedWorldSample sample = map.Samples[i];
                Vector3 position = map.PositionForIndex(i);
                if (!IsCandidate(
                        map,
                        sample,
                        position,
                        rewardRequests,
                        obstacleRequests,
                        hasStageDoorAnchor,
                        stageDoorAnchor,
                        config,
                        spec,
                        planSeed,
                        i))
                {
                    continue;
                }

                float score = DeterministicSeed.ToFloat01(DeterministicSeed.Combine(planSeed, i, 757));
                candidates.Add(new ScoredDecorationCandidate(i, score));
            }

            candidates.Sort(static (a, b) => a.Score.CompareTo(b.Score));
            int emitted = 0;
            for (int i = 0; i < candidates.Count && emitted < targetCount && requests.Count < maxTotal; i++)
            {
                int sampleIndex = candidates[i].SampleIndex;
                GeneratedWorldSample sample = map.Samples[sampleIndex];
                Vector3 position = map.PositionForIndex(sampleIndex);
                if (TooCloseToPlaced(position, placed, spec.MinDistanceBetweenDecorations))
                {
                    continue;
                }

                int localSeed = DeterministicSeed.Combine(planSeed, sampleIndex, emitted, 1877);
                float yaw = ResolveYaw(spec, localSeed);
                Quaternion rotation = ResolveRotation(sample.Normal, yaw, spec.AlignToSlope);
                float scale = Mathf.Lerp(
                    spec.ScaleMin,
                    spec.ScaleMax,
                    DeterministicSeed.ToFloat01(localSeed + 17));
                Vector3 normal = sample.Normal.sqrMagnitude > 0.0001f ? sample.Normal : Vector3.up;
                Vector3 spawnPosition = position + normal * config.WorldDecorationSurfaceOffset;
                requests.Add(new WorldDecorationSpawnRequest(spec, spawnPosition, normal, rotation, scale, localSeed));
                placed.Add(new PlacedDecoration(spawnPosition, spec.MinDistanceBetweenDecorations));
                emitted++;
            }
        }

        private static int ResolveTargetCount(
            GeneratedWorldMap map,
            WorldDecorationSpec spec,
            int seed,
            int actNumber,
            RunWorldGenerationConfig config)
        {
            float radiusScale = Mathf.Clamp(
                map.PlayableRadius / config.WorldDecorationReferenceRadius,
                0.75f,
                2.25f);
            float multiplier = config.WorldDecorationDensityMultiplier * radiusScale * spec.Weight;
            int min = Mathf.RoundToInt(spec.MinCount * multiplier);
            int max = Mathf.RoundToInt(spec.MaxCount * multiplier);
            if (max <= 0)
            {
                return 0;
            }

            int safeMin = Mathf.Clamp(min, 0, max);
            int safeMax = Mathf.Max(safeMin, max);
            if (safeMin == safeMax)
            {
                return safeMin;
            }

            float roll = DeterministicSeed.ToFloat01(DeterministicSeed.Combine(seed, actNumber, HashString(spec.Id), 413));
            return Mathf.RoundToInt(Mathf.Lerp(safeMin, safeMax, roll));
        }

        private static bool IsCandidate(
            GeneratedWorldMap map,
            GeneratedWorldSample sample,
            Vector3 position,
            IReadOnlyList<WorldRewardSpawnRequest> rewardRequests,
            IReadOnlyList<WorldBlockingObstacleSpawnRequest> obstacleRequests,
            bool hasStageDoorAnchor,
            Vector3 stageDoorAnchor,
            RunWorldGenerationConfig config,
            WorldDecorationSpec spec,
            int planSeed,
            int sampleIndex)
        {
            if (!sample.HasMask(GeneratedWorldMask.Walkable | GeneratedWorldMask.Reachable)
                || sample.SlopeDegrees > spec.MaxSlopeDegrees
                || OutsideDecorationBounds(map, position, config.WorldDecorationArenaEdgePadding)
                || TooClose2D(position, map.PlayerStartPosition, Mathf.Max(spec.MinDistanceFromPlayerStart, config.WorldDecorationPlayerStartExclusionRadius))
                || TooCloseToRewards(position, rewardRequests, Mathf.Max(spec.MinDistanceFromRewards, config.WorldDecorationRewardExclusionRadius))
                || TooCloseToObstacles(position, obstacleRequests)
                || (hasStageDoorAnchor && TooClose2D(position, stageDoorAnchor, Mathf.Max(spec.MinDistanceFromStageDoor, config.WorldDecorationStageDoorExclusionRadius))))
            {
                return false;
            }

            WorldDecorationSurfaceMask surfaceMask = SurfaceMaskFor(map, sample, position);
            if ((surfaceMask & spec.AllowedSurfaces) == 0)
            {
                return false;
            }

            if (sample.IsRamp
                && DeterministicSeed.ToFloat01(DeterministicSeed.Combine(planSeed, sampleIndex, 149)) < config.WorldDecorationRampCenterSkipChance)
            {
                return false;
            }

            return true;
        }

        private static WorldDecorationSurfaceMask SurfaceMaskFor(
            GeneratedWorldMap map,
            GeneratedWorldSample sample,
            Vector3 position)
        {
            WorldDecorationSurfaceMask mask = WorldDecorationSurfaceMask.None;
            if (sample.IsRamp)
            {
                mask |= WorldDecorationSurfaceMask.Ramp;
            }
            else if (sample.SurfaceKind == GeneratedWorldSurfaceKind.Continuous)
            {
                mask |= WorldDecorationSurfaceMask.OrganicGround;
            }
            else
            {
                mask |= WorldDecorationSurfaceMask.TerraceTop;
            }

            if (sample.HighGroundScore >= 0.58f)
            {
                mask |= WorldDecorationSurfaceMask.HighGround;
            }

            if (new Vector2(position.x, position.z).magnitude >= map.PlayableRadius * 0.70f)
            {
                mask |= WorldDecorationSurfaceMask.Perimeter;
            }

            return mask;
        }

        private static float ResolveYaw(WorldDecorationSpec spec, int seed)
        {
            float yaw = DeterministicSeed.ToFloat01(seed + 31) * 360f;
            if (spec.RotationVariationDegrees >= 359f)
            {
                return yaw;
            }

            float halfRange = spec.RotationVariationDegrees * 0.5f;
            return Mathf.Lerp(-halfRange, halfRange, DeterministicSeed.ToFloat01(seed + 43));
        }

        private static Quaternion ResolveRotation(Vector3 normal, float yaw, bool alignToSlope)
        {
            if (!alignToSlope)
            {
                return Quaternion.Euler(0f, yaw, 0f);
            }

            Vector3 safeNormal = normal.sqrMagnitude > 0.0001f ? normal.normalized : Vector3.up;
            return Quaternion.FromToRotation(Vector3.up, safeNormal) * Quaternion.Euler(0f, yaw, 0f);
        }

        private static bool OutsideDecorationBounds(GeneratedWorldMap map, Vector3 position, float padding)
        {
            return new Vector2(position.x, position.z).magnitude > map.PlayableRadius - Mathf.Max(0f, padding);
        }

        private static bool TooCloseToRewards(
            Vector3 position,
            IReadOnlyList<WorldRewardSpawnRequest> rewardRequests,
            float radius)
        {
            if (rewardRequests == null || radius <= 0f)
            {
                return false;
            }

            for (int i = 0; i < rewardRequests.Count; i++)
            {
                if (TooClose2D(position, rewardRequests[i].Position, radius))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TooCloseToObstacles(
            Vector3 position,
            IReadOnlyList<WorldBlockingObstacleSpawnRequest> obstacleRequests)
        {
            if (obstacleRequests == null || obstacleRequests.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < obstacleRequests.Count; i++)
            {
                WorldBlockingObstacleSpawnRequest obstacle = obstacleRequests[i];
                if (WorldPropPlacementRules.FootprintsOverlap(
                        position,
                        0f,
                        obstacle.Position,
                        obstacle.AvoidanceRadius))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TooCloseToPlaced(Vector3 position, IReadOnlyList<PlacedDecoration> placed, float radius)
        {
            if (radius <= 0f)
            {
                return false;
            }

            for (int i = 0; i < placed.Count; i++)
            {
                PlacedDecoration decoration = placed[i];
                if (TooClose2D(position, decoration.Position, Mathf.Max(radius, decoration.Radius)))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TooClose2D(Vector3 a, Vector3 b, float radius)
        {
            if (radius <= 0f)
            {
                return false;
            }

            float dx = a.x - b.x;
            float dz = a.z - b.z;
            return dx * dx + dz * dz < radius * radius;
        }

        private static int HashString(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return 0;
            }

            unchecked
            {
                int hash = 17;
                for (int i = 0; i < value.Length; i++)
                {
                    hash = hash * 31 + value[i];
                }

                return hash;
            }
        }

        private readonly struct PlacedDecoration
        {
            public PlacedDecoration(Vector3 position, float radius)
            {
                Position = position;
                Radius = Mathf.Max(0f, radius);
            }

            public Vector3 Position { get; }
            public float Radius { get; }
        }

        private readonly struct ScoredDecorationCandidate
        {
            public ScoredDecorationCandidate(int sampleIndex, float score)
            {
                SampleIndex = sampleIndex;
                Score = score;
            }

            public int SampleIndex { get; }
            public float Score { get; }
        }
    }

    public sealed class WorldDecorationSystem : IRunWorldRefreshAdapter
    {
        private readonly RunWorldGenerationConfig worldConfig;
        private readonly RunWorldGenerationState generationState;
        private readonly WorldDecorationFactory factory;
        private readonly WorldDecorationCatalog catalog;

        private GameObject activeRoot;
        private int activeRequestCount;

        public WorldDecorationSystem(
            RunWorldGenerationConfig worldConfig,
            RunWorldGenerationState generationState,
            WorldDecorationFactory factory = null,
            WorldDecorationCatalog catalog = null)
        {
            this.worldConfig = worldConfig;
            this.generationState = generationState;
            this.factory = factory ?? new WorldDecorationFactory();
            this.catalog = catalog;
        }

        public GameObject ActiveRoot => activeRoot;
        public int ActiveRequestCount => activeRequestCount;
        public string DiagnosticsName => "WorldDecoration";
        public string LoadingStage => "Scattering Props";

        public void RefreshForCurrentWorld()
        {
            Clear();
            if (!TryGetCurrentResult(out WorldGenerationResult result))
            {
                return;
            }

            WorldDecorationSpec[] specs = ResolveSpecs();
            IReadOnlyList<WorldDecorationSpawnRequest> requests =
                GeneratedWorldDecorationPlanner.BuildRequests(result, specs, worldConfig);
            activeRequestCount = requests.Count;
            if (requests.Count == 0)
            {
                return;
            }

            activeRoot = factory.Create(result.GeneratedRoot.transform, requests, worldConfig);
        }

        public async UniTask RefreshForCurrentWorldAsync(WorldLoadTimingDiagnostics timing = null)
        {
            timing ??= WorldLoadTimingDiagnostics.Disabled;
            Clear();
            if (!TryGetCurrentResult(out WorldGenerationResult result))
            {
                return;
            }

            WorldDecorationSpec[] specs = ResolveSpecs();
            IReadOnlyList<WorldDecorationSpawnRequest> requests;
            using (timing.Stage("WorldDecoration.Planning"))
            {
                requests = await UniTask.RunOnThreadPool(
                    () => GeneratedWorldDecorationPlanner.BuildRequests(result, specs, worldConfig));
                await UniTask.SwitchToMainThread();
            }

            activeRequestCount = requests.Count;
            LogRequestSummary(timing, requests);
            if (requests.Count == 0)
            {
                return;
            }

            using (timing.Stage("WorldDecoration.Build"))
            {
                activeRoot = await factory.CreateAsync(result.GeneratedRoot.transform, requests, worldConfig);
            }
        }

        private static void LogRequestSummary(
            WorldLoadTimingDiagnostics timing,
            IReadOnlyList<WorldDecorationSpawnRequest> requests)
        {
            int crates = 0;
            int barrels = 0;
            int ropes = 0;
            int planks = 0;
            int hoops = 0;
            int lamps = 0;
            int signs = 0;
            int other = 0;
            for (int i = 0; i < requests.Count; i++)
            {
                switch (requests[i].Spec.Shape)
                {
                    case WorldDecorationShape.Crate:
                        crates++;
                        break;
                    case WorldDecorationShape.Barrel:
                        barrels++;
                        break;
                    case WorldDecorationShape.RopeCoil:
                        ropes++;
                        break;
                    case WorldDecorationShape.BrokenPlank:
                        planks++;
                        break;
                    case WorldDecorationShape.Hoop:
                        hoops++;
                        break;
                    case WorldDecorationShape.SmallLamp:
                        lamps++;
                        break;
                    case WorldDecorationShape.Sign:
                        signs++;
                        break;
                    default:
                        other++;
                        break;
                }
            }

            timing.LogStage(
                "WorldDecoration.Requests",
                0d,
                $"total={requests.Count} crates={crates} barrels={barrels} ropes={ropes} planks={planks} hoops={hoops} lamps={lamps} signs={signs} other={other}");
        }

        public void Clear()
        {
            activeRequestCount = 0;
            if (activeRoot == null)
            {
                return;
            }

            factory.DestroyRoot(activeRoot);
            activeRoot = null;
        }

        private WorldDecorationSpec[] ResolveSpecs()
        {
            return catalog != null ? catalog.BuildRuntimeSpecs() : WorldDecorationDefaults.CreateSpecs();
        }

        private bool TryGetCurrentResult(out WorldGenerationResult result)
        {
            result = default;
            if (worldConfig == null || !worldConfig.WorldDecorationEnabled || generationState == null)
            {
                return false;
            }

            result = generationState.Current;
            return !result.UsedPrototypePlacement
                && result.GeneratedRoot != null
                && result.GeneratedMap != null;
        }
    }

    public sealed class WorldDecorationFactory
    {
        public GameObject Create(
            Transform parent,
            IReadOnlyList<WorldDecorationSpawnRequest> requests,
            RunWorldGenerationConfig config)
        {
            if (parent == null || requests == null || requests.Count == 0)
            {
                return null;
            }

            GameObject root = CreateRoot(parent);
            var cache = new WorldDecorationMaterialCache();
            for (int i = 0; i < requests.Count; i++)
            {
                CreateDecoration(root.transform, requests[i], cache);
            }

            AttachCleanup(root, cache);
            return root;
        }

        public async UniTask<GameObject> CreateAsync(
            Transform parent,
            IReadOnlyList<WorldDecorationSpawnRequest> requests,
            RunWorldGenerationConfig config)
        {
            if (parent == null || requests == null || requests.Count == 0)
            {
                return null;
            }

            GameObject root = CreateRoot(parent);
            var cache = new WorldDecorationMaterialCache();
            int yieldEvery = config != null ? config.WorldDecorationBuildItemsPerFrame : 48;
            for (int i = 0; i < requests.Count; i++)
            {
                CreateDecoration(root.transform, requests[i], cache);
                if ((i + 1) % yieldEvery == 0)
                {
                    await UniTask.Yield(PlayerLoopTiming.Update);
                }
            }

            AttachCleanup(root, cache);
            await UniTask.Yield(PlayerLoopTiming.Update);
            return root;
        }

        public void DestroyRoot(GameObject root)
        {
            DestroyObject(root);
        }

        private static GameObject CreateRoot(Transform parent)
        {
            var root = new GameObject("World Decoration Visual");
            root.transform.SetParent(parent, worldPositionStays: false);
            root.transform.localPosition = Vector3.zero;
            root.transform.localRotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;
            return root;
        }

        private static void CreateDecoration(
            Transform parent,
            WorldDecorationSpawnRequest request,
            WorldDecorationMaterialCache cache)
        {
            GameObject root = new GameObject(request.Spec.DisplayName);
            root.transform.SetParent(parent, worldPositionStays: false);
            root.transform.SetPositionAndRotation(request.Position, request.Rotation);
            root.transform.localScale = Vector3.one * request.Scale;

            WorldDecorationMaterialSet materials = cache.Resolve(request.Spec);
            switch (request.Spec.Shape)
            {
                case WorldDecorationShape.Barrel:
                    CreateBarrel(root.transform, materials);
                    break;
                case WorldDecorationShape.RopeCoil:
                    CreateRopeCoil(root.transform, materials);
                    break;
                case WorldDecorationShape.BrokenPlank:
                    CreateBrokenPlank(root.transform, materials);
                    break;
                case WorldDecorationShape.Hoop:
                    CreateHoop(root.transform, materials, cache);
                    break;
                case WorldDecorationShape.SmallLamp:
                    CreateSmallLamp(root.transform, materials);
                    break;
                case WorldDecorationShape.Sign:
                    CreateSign(root.transform, materials);
                    break;
                default:
                    CreateCrate(root.transform, materials);
                    break;
            }
        }

        private static void CreateCrate(Transform parent, WorldDecorationMaterialSet materials)
        {
            CreatePrimitiveChild(parent, PrimitiveType.Cube, "Crate Body", Vector3.up * 0.45f, Quaternion.identity, new Vector3(1.25f, 0.9f, 1.05f), materials.Primary);
            CreatePrimitiveChild(parent, PrimitiveType.Cube, "Crate Strap X", Vector3.up * 0.92f, Quaternion.identity, new Vector3(1.34f, 0.08f, 0.16f), materials.Secondary);
            CreatePrimitiveChild(parent, PrimitiveType.Cube, "Crate Strap Z", Vector3.up * 0.93f, Quaternion.identity, new Vector3(0.16f, 0.08f, 1.14f), materials.Secondary);
        }

        private static void CreateBarrel(Transform parent, WorldDecorationMaterialSet materials)
        {
            CreatePrimitiveChild(parent, PrimitiveType.Cylinder, "Barrel Body", Vector3.up * 0.55f, Quaternion.identity, new Vector3(0.75f, 0.55f, 0.75f), materials.Primary);
            CreatePrimitiveChild(parent, PrimitiveType.Cylinder, "Barrel Rim Top", Vector3.up * 1.10f, Quaternion.identity, new Vector3(0.82f, 0.035f, 0.82f), materials.Accent);
            CreatePrimitiveChild(parent, PrimitiveType.Cylinder, "Barrel Rim Bottom", Vector3.up * 0.08f, Quaternion.identity, new Vector3(0.82f, 0.035f, 0.82f), materials.Accent);
        }

        private static void CreateRopeCoil(Transform parent, WorldDecorationMaterialSet materials)
        {
            CreatePrimitiveChild(parent, PrimitiveType.Cylinder, "Rope Coil Base", Vector3.up * 0.055f, Quaternion.identity, new Vector3(0.95f, 0.055f, 0.95f), materials.Primary);
            CreatePrimitiveChild(parent, PrimitiveType.Cylinder, "Rope Coil Center", Vector3.up * 0.09f, Quaternion.identity, new Vector3(0.38f, 0.045f, 0.38f), materials.Secondary);
            CreatePrimitiveChild(parent, PrimitiveType.Cube, "Loose Rope End", new Vector3(0.55f, 0.12f, 0.06f), Quaternion.Euler(0f, 22f, 0f), new Vector3(0.65f, 0.06f, 0.10f), materials.Accent);
        }

        private static void CreateBrokenPlank(Transform parent, WorldDecorationMaterialSet materials)
        {
            CreatePrimitiveChild(parent, PrimitiveType.Cube, "Broken Plank A", Vector3.up * 0.08f, Quaternion.Euler(0f, -8f, 0f), new Vector3(2.20f, 0.12f, 0.34f), materials.Primary);
            CreatePrimitiveChild(parent, PrimitiveType.Cube, "Broken Plank B", new Vector3(0.36f, 0.16f, 0.24f), Quaternion.Euler(0f, 19f, 0f), new Vector3(1.10f, 0.10f, 0.28f), materials.Secondary);
        }

        private static void CreateHoop(
            Transform parent,
            WorldDecorationMaterialSet materials,
            WorldDecorationMaterialCache cache)
        {
            Mesh mesh = BuildFlatRingMesh(0.56f, 0.43f, 24);
            cache.RegisterOwned(mesh);
            GameObject hoop = new("Fallen Hoop");
            hoop.transform.SetParent(parent, worldPositionStays: false);
            hoop.transform.localPosition = Vector3.up * 0.045f;
            hoop.transform.localRotation = Quaternion.identity;
            hoop.transform.localScale = Vector3.one;
            var filter = hoop.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            var renderer = hoop.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = materials.Primary;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = true;
        }

        private static void CreateSmallLamp(Transform parent, WorldDecorationMaterialSet materials)
        {
            CreatePrimitiveChild(parent, PrimitiveType.Cylinder, "Lamp Base", Vector3.up * 0.08f, Quaternion.identity, new Vector3(0.36f, 0.08f, 0.36f), materials.Secondary);
            CreatePrimitiveChild(parent, PrimitiveType.Cylinder, "Lamp Post", Vector3.up * 0.42f, Quaternion.identity, new Vector3(0.12f, 0.36f, 0.12f), materials.Primary);
            CreatePrimitiveChild(parent, PrimitiveType.Sphere, "Lamp Lens", Vector3.up * 0.84f, Quaternion.identity, new Vector3(0.34f, 0.34f, 0.34f), materials.Accent);
        }

        private static void CreateSign(Transform parent, WorldDecorationMaterialSet materials)
        {
            CreatePrimitiveChild(parent, PrimitiveType.Cube, "Sign Post", Vector3.up * 0.38f, Quaternion.identity, new Vector3(0.12f, 0.76f, 0.12f), materials.Secondary);
            CreatePrimitiveChild(parent, PrimitiveType.Cube, "Sign Face", Vector3.up * 0.84f, Quaternion.identity, new Vector3(1.10f, 0.56f, 0.10f), materials.Primary);
            CreatePrimitiveChild(parent, PrimitiveType.Cube, "Sign Trim", Vector3.up * 1.14f, Quaternion.identity, new Vector3(1.16f, 0.07f, 0.12f), materials.Accent);
        }

        private static GameObject CreatePrimitiveChild(
            Transform parent,
            PrimitiveType primitive,
            string name,
            Vector3 localPosition,
            Quaternion localRotation,
            Vector3 localScale,
            Material material)
        {
            GameObject child = GameObject.CreatePrimitive(primitive);
            child.name = name;
            child.transform.SetParent(parent, worldPositionStays: false);
            child.transform.localPosition = localPosition;
            child.transform.localRotation = localRotation;
            child.transform.localScale = localScale;
            Collider collider = child.GetComponent<Collider>();
            if (collider != null)
            {
                DestroyObject(collider);
            }

            Renderer renderer = child.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = true;
            }

            return child;
        }

        private static Mesh BuildFlatRingMesh(float outerRadius, float innerRadius, int segments)
        {
            int safeSegments = Mathf.Max(8, segments);
            var vertices = new Vector3[safeSegments * 2];
            var normals = new Vector3[vertices.Length];
            var uvs = new Vector2[vertices.Length];
            var triangles = new int[safeSegments * 6];
            for (int i = 0; i < safeSegments; i++)
            {
                float angle = i / (float)safeSegments * Mathf.PI * 2f;
                float cos = Mathf.Cos(angle);
                float sin = Mathf.Sin(angle);
                vertices[i * 2] = new Vector3(cos * outerRadius, 0f, sin * outerRadius);
                vertices[i * 2 + 1] = new Vector3(cos * innerRadius, 0f, sin * innerRadius);
                normals[i * 2] = Vector3.up;
                normals[i * 2 + 1] = Vector3.up;
                uvs[i * 2] = new Vector2(cos * 0.5f + 0.5f, sin * 0.5f + 0.5f);
                uvs[i * 2 + 1] = new Vector2(cos * 0.35f + 0.5f, sin * 0.35f + 0.5f);

                int next = (i + 1) % safeSegments;
                int tri = i * 6;
                triangles[tri] = i * 2;
                triangles[tri + 1] = next * 2;
                triangles[tri + 2] = i * 2 + 1;
                triangles[tri + 3] = next * 2;
                triangles[tri + 4] = next * 2 + 1;
                triangles[tri + 5] = i * 2 + 1;
            }

            var mesh = new Mesh
            {
                name = "World Decoration Hoop Mesh",
                hideFlags = HideFlags.DontSave
            };
            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void AttachCleanup(GameObject root, WorldDecorationMaterialCache cache)
        {
            WorldDressingMaterialCleanup cleanup = root.AddComponent<WorldDressingMaterialCleanup>();
            cleanup.Initialize(cache.OwnedObjects);
        }

        private static void DestroyObject(UnityEngine.Object target)
        {
            RuntimeObjectFactory.Release(target);
        }
    }

    public readonly struct WorldDecorationMaterialSet
    {
        public WorldDecorationMaterialSet(Material primary, Material secondary, Material accent)
        {
            Primary = primary;
            Secondary = secondary;
            Accent = accent;
        }

        public Material Primary { get; }
        public Material Secondary { get; }
        public Material Accent { get; }
    }

    public sealed class WorldDecorationMaterialCache
    {
        private readonly Dictionary<string, WorldDecorationMaterialSet> materialsBySpec = new(StringComparer.Ordinal);
        private readonly List<UnityEngine.Object> ownedObjects = new();

        public UnityEngine.Object[] OwnedObjects => ownedObjects.ToArray();

        public WorldDecorationMaterialSet Resolve(WorldDecorationSpec spec)
        {
            string key = spec.Id;
            if (materialsBySpec.TryGetValue(key, out WorldDecorationMaterialSet existing))
            {
                return existing;
            }

            var created = new WorldDecorationMaterialSet(
                CreateMaterial($"{spec.DisplayName} Primary", spec.PrimaryColor, emissive: false, spec.Shape, 0),
                CreateMaterial($"{spec.DisplayName} Secondary", spec.SecondaryColor, emissive: false, spec.Shape, 1),
                CreateMaterial($"{spec.DisplayName} Accent", spec.AccentColor, spec.Shape == WorldDecorationShape.SmallLamp, spec.Shape, 2));
            materialsBySpec[key] = created;
            ownedObjects.Add(created.Primary);
            ownedObjects.Add(created.Secondary);
            ownedObjects.Add(created.Accent);
            return created;
        }

        public void RegisterOwned(UnityEngine.Object target)
        {
            if (target != null)
            {
                ownedObjects.Add(target);
            }
        }

        private Material CreateMaterial(
            string name,
            Color color,
            bool emissive,
            WorldDecorationShape shape,
            int channel)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("Standard")
                ?? Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Sprites/Default");
            Texture2D texture = CreateTexture($"{name} Texture", color, shape, channel, emissive);
            var material = new Material(shader)
            {
                name = name,
                mainTexture = texture,
                hideFlags = HideFlags.DontSave
            };
            SetColorIfPresent(material, "_BaseColor", color);
            SetColorIfPresent(material, "_Color", color);
            SetTextureIfPresent(material, "_BaseMap", texture);
            SetTextureIfPresent(material, "_MainTex", texture);
            if (emissive)
            {
                Color emission = color * 1.8f;
                SetColorIfPresent(material, "_EmissionColor", emission);
                material.EnableKeyword("_EMISSION");
            }

            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", emissive ? 0.32f : 0.12f);
            }

            if (material.HasProperty("_Glossiness"))
            {
                material.SetFloat("_Glossiness", emissive ? 0.32f : 0.12f);
            }

            ownedObjects.Add(texture);
            return material;
        }

        private static Texture2D CreateTexture(
            string name,
            Color baseColor,
            WorldDecorationShape shape,
            int channel,
            bool emissive)
        {
            const int Size = 96;
            var texture = new Texture2D(Size, Size, TextureFormat.RGBA32, mipChain: true)
            {
                name = name,
                hideFlags = HideFlags.DontSave,
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Trilinear,
                anisoLevel = 4
            };

            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    float u = (x + 0.5f) / Size;
                    float v = (y + 0.5f) / Size;
                    Color color = baseColor;
                    float pattern = ShapePattern(shape, channel, u, v);
                    float dirt = ValueNoise(x + channel * 37, y + (int)shape * 53, 19) * 0.28f
                        + Mathf.Pow(ValueNoise(x / 3 + (int)shape, y / 5 + channel, 109), 2.1f) * 0.24f;
                    float edgeWear = EdgeWear(u, v) * (emissive ? 0.06f : 0.22f);
                    float brush = Mathf.Sin((v * 36f + ValueNoise(x, y, 47) * 4f) * Mathf.PI) * 0.035f;

                    if (emissive)
                    {
                        color = Color.Lerp(color, Color.white, pattern * 0.12f);
                        color += new Color(brush, brush, brush, 0f);
                    }
                    else
                    {
                        color = Color.Lerp(color, Color.black, dirt + pattern * 0.10f);
                        color = Color.Lerp(color, new Color(0.92f, 0.74f, 0.38f, 1f), edgeWear);
                        color += new Color(brush, brush, brush, 0f);
                    }

                    texture.SetPixel(x, y, ClampColor(color));
                }
            }

            texture.Apply(updateMipmaps: true, makeNoLongerReadable: true);
            return texture;
        }

        private static float ShapePattern(WorldDecorationShape shape, int channel, float u, float v)
        {
            return shape switch
            {
                WorldDecorationShape.Crate or WorldDecorationShape.BrokenPlank or WorldDecorationShape.Sign
                    => BoardPattern(channel == 1 ? v : u, channel == 1 ? 5f : 7f),
                WorldDecorationShape.Barrel
                    => BoardPattern(u, 9f) * 0.72f + RingPattern(v) * 0.38f,
                WorldDecorationShape.RopeCoil
                    => Mathf.Abs(Mathf.Sin((u + v) * Mathf.PI * 10f)) * 0.35f,
                WorldDecorationShape.Hoop
                    => RingPattern(u) * 0.45f,
                _ => BoardPattern(u, 6f) * 0.35f
            };
        }

        private static float BoardPattern(float value, float count)
        {
            float within = Mathf.Repeat(value * count, 1f);
            float seamDistance = Mathf.Min(within, 1f - within);
            return 1f - Mathf.SmoothStep(0f, 0.11f, seamDistance);
        }

        private static float RingPattern(float value)
        {
            float within = Mathf.Repeat(value * 5f, 1f);
            return 1f - Mathf.SmoothStep(0f, 0.12f, Mathf.Abs(within - 0.5f));
        }

        private static float EdgeWear(float u, float v)
        {
            float edge = Mathf.Min(Mathf.Min(u, 1f - u), Mathf.Min(v, 1f - v));
            return 1f - Mathf.SmoothStep(0f, 0.16f, edge);
        }

        private static float ValueNoise(int x, int y, int salt)
        {
            return DeterministicSeed.ToFloat01(DeterministicSeed.Combine(x, y, salt));
        }

        private static void SetColorIfPresent(Material material, string property, Color value)
        {
            if (material != null && material.HasProperty(property))
            {
                material.SetColor(property, value);
            }
        }

        private static void SetTextureIfPresent(Material material, string property, Texture texture)
        {
            if (material != null && texture != null && material.HasProperty(property))
            {
                material.SetTexture(property, texture);
            }
        }

        private static Color ClampColor(Color color)
        {
            return new Color(
                Mathf.Clamp01(color.r),
                Mathf.Clamp01(color.g),
                Mathf.Clamp01(color.b),
                Mathf.Clamp01(color.a));
        }
    }
}
