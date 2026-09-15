using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;
using UnityEngine;
using UnityEngine.Rendering;

namespace TheCircussyOne.Runtime
{
    public enum GeneratedWorldDressingKind
    {
        GroundDecal,
        WallDecal
    }

    public readonly struct GeneratedWorldDressingRequest
    {
        public GeneratedWorldDressingRequest(
            GeneratedWorldDressingKind kind,
            Vector3 position,
            Vector3 normal,
            Vector3 tangent,
            Vector2 size,
            int variantIndex,
            int seed)
        {
            Kind = kind;
            Position = position;
            Normal = normal.sqrMagnitude > 0.0001f ? normal.normalized : Vector3.up;
            Tangent = tangent.sqrMagnitude > 0.0001f ? tangent.normalized : Vector3.right;
            Size = new Vector2(Mathf.Max(0.01f, size.x), Mathf.Max(0.01f, size.y));
            VariantIndex = Mathf.Max(0, variantIndex);
            Seed = seed;
        }

        public GeneratedWorldDressingKind Kind { get; }
        public Vector3 Position { get; }
        public Vector3 Normal { get; }
        public Vector3 Tangent { get; }
        public Vector2 Size { get; }
        public int VariantIndex { get; }
        public int Seed { get; }
    }

    public static class GeneratedWorldDressingPlanner
    {
        private const int GroundVariantCount = 8;
        private const int WallVariantCount = 8;

        public static IReadOnlyList<GeneratedWorldDressingRequest> BuildRequests(
            WorldGenerationResult result,
            RunWorldGenerationConfig config)
        {
            if (config == null || !config.WorldDressingEnabled || result.UsedPrototypePlacement || result.GeneratedMap == null)
            {
                return Array.Empty<GeneratedWorldDressingRequest>();
            }

            return BuildRequests(
                result.GeneratedMap,
                result.RewardSpawnRequests,
                result.Seed,
                result.ActNumber,
                config);
        }

        public static IReadOnlyList<GeneratedWorldDressingRequest> BuildRequests(
            GeneratedWorldMap map,
            IReadOnlyList<WorldRewardSpawnRequest> rewardRequests,
            int seed,
            int actNumber,
            RunWorldGenerationConfig config)
        {
            if (map == null || config == null || !config.WorldDressingEnabled)
            {
                return Array.Empty<GeneratedWorldDressingRequest>();
            }

            var requests = new List<GeneratedWorldDressingRequest>();
            int groundTarget = TargetCount(
                map,
                config.WorldDressingGroundDecalsPer100k,
                config.WorldDressingGroundDecalMaxCount);
            int wallTarget = TargetCount(
                map,
                config.WorldDressingWallDecalsPer100k,
                config.WorldDressingWallDecalMaxCount);

            AddGroundRequests(map, rewardRequests, seed, actNumber, config, groundTarget, requests);
            AddWallRequests(map, rewardRequests, seed, actNumber, config, wallTarget, requests);
            return requests;
        }

        private static void AddGroundRequests(
            GeneratedWorldMap map,
            IReadOnlyList<WorldRewardSpawnRequest> rewardRequests,
            int seed,
            int actNumber,
            RunWorldGenerationConfig config,
            int targetCount,
            List<GeneratedWorldDressingRequest> requests)
        {
            if (targetCount <= 0)
            {
                return;
            }

            var candidates = new List<ScoredGroundCandidate>();
            int planSeed = DeterministicSeed.Combine(seed, actNumber, config.WorldDressingSeedOffset, 1271);
            for (int i = 0; i < map.Samples.Count; i++)
            {
                GeneratedWorldSample sample = map.Samples[i];
                if (!IsGroundCandidate(map, sample, map.PositionForIndex(i), rewardRequests, config, planSeed, i))
                {
                    continue;
                }

                float score = DeterministicSeed.ToFloat01(DeterministicSeed.Combine(planSeed, i, 341));
                candidates.Add(new ScoredGroundCandidate(i, score));
            }

            candidates.Sort(static (a, b) => a.Score.CompareTo(b.Score));
            int emitted = 0;
            for (int i = 0; i < candidates.Count && emitted < targetCount; i++)
            {
                int sampleIndex = candidates[i].SampleIndex;
                GeneratedWorldSample sample = map.Samples[sampleIndex];
                Vector3 position = map.PositionForIndex(sampleIndex);
                int localSeed = DeterministicSeed.Combine(planSeed, sampleIndex, emitted, 991);
                float width = Mathf.Lerp(
                    config.WorldDressingGroundScaleMin,
                    config.WorldDressingGroundScaleMax,
                    DeterministicSeed.ToFloat01(localSeed + 3));
                float aspect = Mathf.Lerp(0.48f, 1.85f, DeterministicSeed.ToFloat01(localSeed + 7));
                Vector2 size = new(width, width * aspect);
                Vector3 tangent = GroundTangent(sample, localSeed);
                requests.Add(new GeneratedWorldDressingRequest(
                    GeneratedWorldDressingKind.GroundDecal,
                    position + sample.Normal * config.WorldDressingGroundSurfaceOffset,
                    sample.Normal,
                    tangent,
                    size,
                    Mathf.Abs(DeterministicSeed.Combine(localSeed, 13)) % GroundVariantCount,
                    localSeed));
                emitted++;
            }
        }

        private static void AddWallRequests(
            GeneratedWorldMap map,
            IReadOnlyList<WorldRewardSpawnRequest> rewardRequests,
            int seed,
            int actNumber,
            RunWorldGenerationConfig config,
            int targetCount,
            List<GeneratedWorldDressingRequest> requests)
        {
            if (targetCount <= 0)
            {
                return;
            }

            var candidates = new List<ScoredWallCandidate>();
            int planSeed = DeterministicSeed.Combine(seed, actNumber, config.WorldDressingSeedOffset, 5923);
            for (int z = 0; z < map.Depth - 1; z++)
            {
                for (int x = 0; x < map.Width - 1; x++)
                {
                    if (!CellInsidePlayableCircle(map, x, z))
                    {
                        continue;
                    }

                    DressingCell cell = CellFor(map, x, z);
                    if (x < map.Width - 2 && CellInsidePlayableCircle(map, x + 1, z))
                    {
                        DressingCell right = CellFor(map, x + 1, z);
                        AddWallCandidate(
                            map,
                            rewardRequests,
                            config,
                            planSeed,
                            cell.BottomRight,
                            cell.TopRight,
                            right.BottomLeft,
                            right.TopLeft,
                            Vector3.right,
                            Vector3.left,
                            Vector3.forward,
                            CellKey(map, x, z, 1, 0),
                            candidates);
                    }

                    if (z < map.Depth - 2 && CellInsidePlayableCircle(map, x, z + 1))
                    {
                        DressingCell top = CellFor(map, x, z + 1);
                        AddWallCandidate(
                            map,
                            rewardRequests,
                            config,
                            planSeed,
                            cell.TopLeft,
                            cell.TopRight,
                            top.BottomLeft,
                            top.BottomRight,
                            Vector3.forward,
                            Vector3.back,
                            Vector3.right,
                            CellKey(map, x, z, 0, 1),
                            candidates);
                    }
                }
            }

            candidates.Sort(static (a, b) => a.Score.CompareTo(b.Score));
            int emitted = 0;
            for (int i = 0; i < candidates.Count && emitted < targetCount; i++)
            {
                WallCandidate candidate = candidates[i].Candidate;
                int localSeed = DeterministicSeed.Combine(planSeed, candidate.CellKey, emitted, 2039);
                float width = Mathf.Lerp(
                    config.WorldDressingWallWidthMin,
                    config.WorldDressingWallWidthMax,
                    DeterministicSeed.ToFloat01(localSeed + 5));
                float requestedHeight = Mathf.Lerp(
                    config.WorldDressingWallHeightMin,
                    config.WorldDressingWallHeightMax,
                    DeterministicSeed.ToFloat01(localSeed + 9));
                float height = Mathf.Min(candidate.HeightDelta * 0.82f, requestedHeight);
                if (height < 0.35f)
                {
                    continue;
                }

                float minY = candidate.LowHeight + height * 0.5f;
                float maxY = candidate.HighHeight - height * 0.5f;
                float y = minY <= maxY
                    ? Mathf.Lerp(minY, maxY, DeterministicSeed.ToFloat01(localSeed + 17))
                    : (candidate.LowHeight + candidate.HighHeight) * 0.5f;
                Vector3 position = new Vector3(candidate.Center.x, y, candidate.Center.z)
                    + candidate.Normal * config.WorldDressingWallSurfaceOffset;
                requests.Add(new GeneratedWorldDressingRequest(
                    GeneratedWorldDressingKind.WallDecal,
                    position,
                    candidate.Normal,
                    candidate.Tangent,
                    new Vector2(width, height),
                    Mathf.Abs(DeterministicSeed.Combine(localSeed, 19)) % WallVariantCount,
                    localSeed));
                emitted++;
            }
        }

        private static void AddWallCandidate(
            GeneratedWorldMap map,
            IReadOnlyList<WorldRewardSpawnRequest> rewardRequests,
            RunWorldGenerationConfig config,
            int planSeed,
            Vector3 firstA,
            Vector3 firstB,
            Vector3 secondA,
            Vector3 secondB,
            Vector3 firstHigherNormal,
            Vector3 secondHigherNormal,
            Vector3 tangent,
            int cellKey,
            List<ScoredWallCandidate> candidates)
        {
            float firstHeight = (firstA.y + firstB.y) * 0.5f;
            float secondHeight = (secondA.y + secondB.y) * 0.5f;
            float endpointDelta = Mathf.Max(
                Mathf.Abs(firstA.y - secondA.y),
                Mathf.Abs(firstB.y - secondB.y));
            if (Mathf.Abs(firstHeight - secondHeight) <= Mathf.Max(0.5f, map.WallHeightThreshold)
                && endpointDelta <= 0.01f)
            {
                return;
            }

            if (Mathf.Abs(firstA.y - firstB.y) > 0.15f || Mathf.Abs(secondA.y - secondB.y) > 0.15f)
            {
                return;
            }

            float heightDelta = Mathf.Abs(firstHeight - secondHeight);
            if (heightDelta < Mathf.Max(0.5f, map.WallHeightThreshold))
            {
                return;
            }

            Vector3 center = (firstA + firstB + secondA + secondB) * 0.25f;
            center.y = 0f;
            if (OutsideDressingBounds(map, center, config.WorldDressingArenaEdgePadding)
                || TooCloseToPlayerStart(map, center, config.WorldDressingPlayerStartExclusionRadius)
                || TooCloseToRewards(center, rewardRequests, config.WorldDressingRewardExclusionRadius))
            {
                return;
            }

            Vector3 normal = firstHeight > secondHeight ? firstHigherNormal : secondHigherNormal;
            float lowHeight = Mathf.Min(firstHeight, secondHeight);
            float highHeight = Mathf.Max(firstHeight, secondHeight);
            float score = DeterministicSeed.ToFloat01(DeterministicSeed.Combine(planSeed, cellKey, 883));
            candidates.Add(new ScoredWallCandidate(
                new WallCandidate(center, normal, tangent, lowHeight, highHeight, heightDelta, cellKey),
                score));
        }

        private static bool IsGroundCandidate(
            GeneratedWorldMap map,
            GeneratedWorldSample sample,
            Vector3 position,
            IReadOnlyList<WorldRewardSpawnRequest> rewardRequests,
            RunWorldGenerationConfig config,
            int planSeed,
            int sampleIndex)
        {
            if (!sample.HasMask(GeneratedWorldMask.Walkable | GeneratedWorldMask.Reachable)
                || sample.Normal.y < 0.38f
                || OutsideDressingBounds(map, position, config.WorldDressingArenaEdgePadding)
                || TooCloseToPlayerStart(map, position, config.WorldDressingPlayerStartExclusionRadius)
                || TooCloseToRewards(position, rewardRequests, config.WorldDressingRewardExclusionRadius))
            {
                return false;
            }

            if (sample.IsRamp
                && DeterministicSeed.ToFloat01(DeterministicSeed.Combine(planSeed, sampleIndex, 613)) < config.WorldDressingRampCenterSkipChance)
            {
                return false;
            }

            return true;
        }

        private static int TargetCount(GeneratedWorldMap map, float per100k, int maxCount)
        {
            if (map == null || per100k <= 0f || maxCount <= 0)
            {
                return 0;
            }

            float areaUnits = Mathf.PI * map.PlayableRadius * map.PlayableRadius / 100000f;
            return Mathf.Clamp(Mathf.RoundToInt(areaUnits * per100k), 0, maxCount);
        }

        private static Vector3 GroundTangent(GeneratedWorldSample sample, int seed)
        {
            Vector3 tangent;
            if (sample.SurfaceDirection.sqrMagnitude > 0.0001f)
            {
                tangent = new Vector3(sample.SurfaceDirection.x, 0f, sample.SurfaceDirection.y);
            }
            else
            {
                float angle = DeterministicSeed.ToAngleRadians(seed);
                tangent = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            }

            tangent = Vector3.ProjectOnPlane(tangent, sample.Normal);
            if (tangent.sqrMagnitude < 0.0001f)
            {
                tangent = Vector3.Cross(sample.Normal, Vector3.forward);
            }

            if (tangent.sqrMagnitude < 0.0001f)
            {
                tangent = Vector3.right;
            }

            return tangent.normalized;
        }

        private static bool OutsideDressingBounds(GeneratedWorldMap map, Vector3 position, float padding)
        {
            var flat = new Vector2(position.x, position.z);
            return flat.magnitude > map.PlayableRadius - Mathf.Max(0f, padding);
        }

        private static bool CellInsidePlayableCircle(GeneratedWorldMap map, int x, int z)
        {
            Vector3 bottomLeft = map.PositionFor(x, z);
            Vector3 topRight = map.PositionFor(x + 1, z + 1);
            var center = new Vector2(
                (bottomLeft.x + topRight.x) * 0.5f,
                (bottomLeft.z + topRight.z) * 0.5f);
            return center.sqrMagnitude <= map.PlayableRadius * map.PlayableRadius;
        }

        private static DressingCell CellFor(GeneratedWorldMap map, int x, int z)
        {
            Vector3 bottomLeft = map.PositionFor(x, z);
            Vector3 bottomRight = map.PositionFor(x + 1, z);
            Vector3 topLeft = map.PositionFor(x, z + 1);
            Vector3 topRight = map.PositionFor(x + 1, z + 1);

            if (ShouldPreserveCellHeights(map, x, z))
            {
                return new DressingCell(bottomLeft, bottomRight, topLeft, topRight);
            }

            float flatHeight = IsMixedConstructedContinuousCell(map, x, z)
                ? ConstructedHeight(map, x, z, bottomLeft.y, bottomRight.y, topLeft.y, topRight.y)
                : DominantHeight(bottomLeft.y, bottomRight.y, topLeft.y, topRight.y);
            bottomLeft.y = flatHeight;
            bottomRight.y = flatHeight;
            topLeft.y = flatHeight;
            topRight.y = flatHeight;
            return new DressingCell(bottomLeft, bottomRight, topLeft, topRight);
        }

        private static bool ShouldPreserveCellHeights(GeneratedWorldMap map, int x, int z)
        {
            if (HasRampSample(map, x, z))
            {
                return true;
            }

            return IsContinuousSample(map, x, z)
                && IsContinuousSample(map, x + 1, z)
                && IsContinuousSample(map, x, z + 1)
                && IsContinuousSample(map, x + 1, z + 1);
        }

        private static bool HasRampSample(GeneratedWorldMap map, int x, int z)
        {
            return IsRampSample(map, x, z)
                || IsRampSample(map, x + 1, z)
                || IsRampSample(map, x, z + 1)
                || IsRampSample(map, x + 1, z + 1);
        }

        private static bool IsRampSample(GeneratedWorldMap map, int x, int z)
        {
            return map.TryGetSample(x, z, out GeneratedWorldSample sample) && sample.IsRamp;
        }

        private static bool IsContinuousSample(GeneratedWorldMap map, int x, int z)
        {
            return map.TryGetSample(x, z, out GeneratedWorldSample sample)
                && sample.SurfaceKind == GeneratedWorldSurfaceKind.Continuous;
        }

        private static bool IsMixedConstructedContinuousCell(GeneratedWorldMap map, int x, int z)
        {
            bool hasConstructed = false;
            bool hasContinuous = false;
            Consider(x, z);
            Consider(x + 1, z);
            Consider(x, z + 1);
            Consider(x + 1, z + 1);
            return hasConstructed && hasContinuous;

            void Consider(int sampleX, int sampleZ)
            {
                if (IsConstructedSample(map, sampleX, sampleZ))
                {
                    hasConstructed = true;
                }

                if (IsContinuousSample(map, sampleX, sampleZ))
                {
                    hasContinuous = true;
                }
            }
        }

        private static bool IsConstructedSample(GeneratedWorldMap map, int x, int z)
        {
            return map.TryGetSample(x, z, out GeneratedWorldSample sample)
                && (sample.SurfaceKind == GeneratedWorldSurfaceKind.Flat
                    || sample.SurfaceKind == GeneratedWorldSurfaceKind.Ramp);
        }

        private static float ConstructedHeight(
            GeneratedWorldMap map,
            int x,
            int z,
            float bottomLeft,
            float bottomRight,
            float topLeft,
            float topRight)
        {
            float height = float.MinValue;
            Consider(x, z, bottomLeft);
            Consider(x + 1, z, bottomRight);
            Consider(x, z + 1, topLeft);
            Consider(x + 1, z + 1, topRight);
            return height > float.MinValue ? height : DominantHeight(bottomLeft, bottomRight, topLeft, topRight);

            void Consider(int sampleX, int sampleZ, float sampleHeight)
            {
                if (IsConstructedSample(map, sampleX, sampleZ))
                {
                    height = Mathf.Max(height, sampleHeight);
                }
            }
        }

        private static float DominantHeight(float a, float b, float c, float d)
        {
            float average = (a + b + c + d) * 0.25f;
            float result = a;
            float bestDistance = Mathf.Abs(a - average);
            Consider(b);
            Consider(c);
            Consider(d);
            return result;

            void Consider(float value)
            {
                float distance = Mathf.Abs(value - average);
                if (distance < bestDistance || Mathf.Approximately(distance, bestDistance) && value > result)
                {
                    result = value;
                    bestDistance = distance;
                }
            }
        }

        private static int CellKey(GeneratedWorldMap map, int x, int z, int dx, int dz)
        {
            unchecked
            {
                return map.Index(x, z) * 397
                    ^ map.Index(x + dx, z + dz) * 211
                    ^ dx * 31
                    ^ dz * 73;
            }
        }

        private static bool TooCloseToPlayerStart(GeneratedWorldMap map, Vector3 position, float radius)
        {
            return radius > 0f && Distance2D(map.PlayerStartPosition, position) < radius;
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
                if (Distance2D(position, rewardRequests[i].Position) < radius)
                {
                    return true;
                }
            }

            return false;
        }

        private static float Distance2D(Vector3 a, Vector3 b)
        {
            float dx = a.x - b.x;
            float dz = a.z - b.z;
            return Mathf.Sqrt(dx * dx + dz * dz);
        }

        private readonly struct ScoredGroundCandidate
        {
            public ScoredGroundCandidate(int sampleIndex, float score)
            {
                SampleIndex = sampleIndex;
                Score = score;
            }

            public int SampleIndex { get; }
            public float Score { get; }
        }

        private readonly struct WallCandidate
        {
            public WallCandidate(
                Vector3 center,
                Vector3 normal,
                Vector3 tangent,
                float lowHeight,
                float highHeight,
                float heightDelta,
                int cellKey)
            {
                Center = center;
                Normal = normal;
                Tangent = tangent;
                LowHeight = lowHeight;
                HighHeight = highHeight;
                HeightDelta = heightDelta;
                CellKey = cellKey;
            }

            public Vector3 Center { get; }
            public Vector3 Normal { get; }
            public Vector3 Tangent { get; }
            public float LowHeight { get; }
            public float HighHeight { get; }
            public float HeightDelta { get; }
            public int CellKey { get; }
        }

        private readonly struct DressingCell
        {
            public DressingCell(Vector3 bottomLeft, Vector3 bottomRight, Vector3 topLeft, Vector3 topRight)
            {
                BottomLeft = bottomLeft;
                BottomRight = bottomRight;
                TopLeft = topLeft;
                TopRight = topRight;
            }

            public Vector3 BottomLeft { get; }
            public Vector3 BottomRight { get; }
            public Vector3 TopLeft { get; }
            public Vector3 TopRight { get; }
        }

        private readonly struct ScoredWallCandidate
        {
            public ScoredWallCandidate(WallCandidate candidate, float score)
            {
                Candidate = candidate;
                Score = score;
            }

            public WallCandidate Candidate { get; }
            public float Score { get; }
        }
    }

    public sealed class WorldDressingSystem : IRunWorldRefreshAdapter
    {
        private readonly RunWorldGenerationConfig worldConfig;
        private readonly RunWorldGenerationState generationState;
        private readonly WorldDressingFactory factory;

        private GameObject activeRoot;
        private int activeRequestCount;

        public WorldDressingSystem(
            RunWorldGenerationConfig worldConfig,
            RunWorldGenerationState generationState,
            WorldDressingFactory factory = null)
        {
            this.worldConfig = worldConfig;
            this.generationState = generationState;
            this.factory = factory ?? new WorldDressingFactory();
        }

        public GameObject ActiveRoot => activeRoot;
        public int ActiveRequestCount => activeRequestCount;
        public string DiagnosticsName => "WorldDressing";
        public string LoadingStage => "Dressing the Ring";

        public void RefreshForCurrentWorld()
        {
            Clear();
            if (!TryGetCurrentResult(out WorldGenerationResult result))
            {
                return;
            }

            IReadOnlyList<GeneratedWorldDressingRequest> requests =
                GeneratedWorldDressingPlanner.BuildRequests(result, worldConfig);
            activeRequestCount = requests.Count;
            if (requests.Count == 0)
            {
                return;
            }

            activeRoot = factory.Create(
                result.GeneratedRoot.transform,
                requests,
                worldConfig,
                result.Seed,
                result.ActNumber);
        }

        public async UniTask RefreshForCurrentWorldAsync(WorldLoadTimingDiagnostics timing = null)
        {
            timing ??= WorldLoadTimingDiagnostics.Disabled;
            Clear();
            if (!TryGetCurrentResult(out WorldGenerationResult result))
            {
                return;
            }

            IReadOnlyList<GeneratedWorldDressingRequest> requests;
            using (timing.Stage("WorldDressing.Planning"))
            {
                requests = await UniTask.RunOnThreadPool(
                    () => GeneratedWorldDressingPlanner.BuildRequests(result, worldConfig));
                await UniTask.SwitchToMainThread();
            }

            activeRequestCount = requests.Count;
            LogRequestSummary(timing, requests);
            if (requests.Count == 0)
            {
                return;
            }

            using (timing.Stage("WorldDressing.Build"))
            {
                activeRoot = await factory.CreateAsync(
                    result.GeneratedRoot.transform,
                    requests,
                    worldConfig,
                    result.Seed,
                    result.ActNumber);
            }
        }

        private static void LogRequestSummary(
            WorldLoadTimingDiagnostics timing,
            IReadOnlyList<GeneratedWorldDressingRequest> requests)
        {
            int groundCount = 0;
            int wallCount = 0;
            for (int i = 0; i < requests.Count; i++)
            {
                if (requests[i].Kind == GeneratedWorldDressingKind.WallDecal)
                {
                    wallCount++;
                }
                else
                {
                    groundCount++;
                }
            }

            timing.LogStage(
                "WorldDressing.Requests",
                0d,
                $"total={requests.Count} ground={groundCount} wall={wallCount}");
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

        private bool TryGetCurrentResult(out WorldGenerationResult result)
        {
            result = default;
            if (worldConfig == null || !worldConfig.WorldDressingEnabled || generationState == null)
            {
                return false;
            }

            result = generationState.Current;
            return !result.UsedPrototypePlacement
                && result.GeneratedRoot != null
                && result.GeneratedMap != null;
        }
    }

    public sealed class WorldDressingFactory
    {
        private const int AtlasColumns = 4;
        private const int AtlasRows = 2;

        public GameObject Create(
            Transform parent,
            IReadOnlyList<GeneratedWorldDressingRequest> requests,
            RunWorldGenerationConfig config,
            int seed,
            int actNumber)
        {
            if (parent == null || requests == null || requests.Count == 0)
            {
                return null;
            }

            GameObject root = CreateRoot(parent);
            WorldDressingMaterialSet materials = WorldDressingTextureGenerator.GenerateMaterialSet(config, seed, actNumber);
            WorldDressingMaterialCleanup cleanup = root.AddComponent<WorldDressingMaterialCleanup>();
            cleanup.Initialize(materials.OwnedObjects);

            BuildChild(root.transform, "Ground Dressing Decals", requests, GeneratedWorldDressingKind.GroundDecal, materials.GroundMaterial, config);
            BuildChild(root.transform, "Wall Dressing Decals", requests, GeneratedWorldDressingKind.WallDecal, materials.WallMaterial, config);
            return root;
        }

        public async UniTask<GameObject> CreateAsync(
            Transform parent,
            IReadOnlyList<GeneratedWorldDressingRequest> requests,
            RunWorldGenerationConfig config,
            int seed,
            int actNumber)
        {
            if (parent == null || requests == null || requests.Count == 0)
            {
                return null;
            }

            GameObject root = CreateRoot(parent);
            WorldDressingMaterialSet materials = WorldDressingTextureGenerator.GenerateMaterialSet(config, seed, actNumber);
            WorldDressingMaterialCleanup cleanup = root.AddComponent<WorldDressingMaterialCleanup>();
            cleanup.Initialize(materials.OwnedObjects);

            await UniTask.Yield(PlayerLoopTiming.Update);
            await BuildChildAsync(root.transform, "Ground Dressing Decals", requests, GeneratedWorldDressingKind.GroundDecal, materials.GroundMaterial, config);
            await BuildChildAsync(root.transform, "Wall Dressing Decals", requests, GeneratedWorldDressingKind.WallDecal, materials.WallMaterial, config);
            await UniTask.Yield(PlayerLoopTiming.Update);
            return root;
        }

        public void DestroyRoot(GameObject root)
        {
            DestroyObject(root);
        }

        private static GameObject CreateRoot(Transform parent)
        {
            var root = new GameObject("World Dressing Visual");
            root.transform.SetParent(parent, worldPositionStays: false);
            root.transform.localPosition = Vector3.zero;
            root.transform.localRotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;
            return root;
        }

        private static void BuildChild(
            Transform parent,
            string name,
            IReadOnlyList<GeneratedWorldDressingRequest> requests,
            GeneratedWorldDressingKind kind,
            Material material,
            RunWorldGenerationConfig config)
        {
            Mesh mesh = BuildMesh(requests, kind, config);
            if (mesh == null || mesh.vertexCount == 0)
            {
                DestroyObject(mesh);
                return;
            }

            var child = new GameObject(name);
            child.transform.SetParent(parent, worldPositionStays: false);
            child.transform.localPosition = Vector3.zero;
            child.transform.localRotation = Quaternion.identity;
            child.transform.localScale = Vector3.one;
            var filter = child.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            var renderer = child.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            WorldDressingMaterialCleanup cleanup = child.AddComponent<WorldDressingMaterialCleanup>();
            cleanup.Initialize(new UnityEngine.Object[] { mesh });
        }

        private static async UniTask BuildChildAsync(
            Transform parent,
            string name,
            IReadOnlyList<GeneratedWorldDressingRequest> requests,
            GeneratedWorldDressingKind kind,
            Material material,
            RunWorldGenerationConfig config)
        {
            Mesh mesh = await BuildMeshAsync(requests, kind, config);
            if (mesh == null || mesh.vertexCount == 0)
            {
                DestroyObject(mesh);
                return;
            }

            var child = new GameObject(name);
            child.transform.SetParent(parent, worldPositionStays: false);
            child.transform.localPosition = Vector3.zero;
            child.transform.localRotation = Quaternion.identity;
            child.transform.localScale = Vector3.one;
            var filter = child.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            var renderer = child.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            WorldDressingMaterialCleanup cleanup = child.AddComponent<WorldDressingMaterialCleanup>();
            cleanup.Initialize(new UnityEngine.Object[] { mesh });
        }

        private static Mesh BuildMesh(
            IReadOnlyList<GeneratedWorldDressingRequest> requests,
            GeneratedWorldDressingKind kind,
            RunWorldGenerationConfig config)
        {
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();

            for (int i = 0; i < requests.Count; i++)
            {
                GeneratedWorldDressingRequest request = requests[i];
                if (request.Kind != kind)
                {
                    continue;
                }

                AddQuad(request, config, vertices, normals, uvs, triangles);
            }

            if (vertices.Count == 0)
            {
                return null;
            }

            var mesh = new Mesh
            {
                name = kind == GeneratedWorldDressingKind.GroundDecal
                    ? "World Dressing Ground Decals Mesh"
                    : "World Dressing Wall Decals Mesh",
                hideFlags = HideFlags.DontSave,
                indexFormat = vertices.Count > ushort.MaxValue ? IndexFormat.UInt32 : IndexFormat.UInt16
            };
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        private static async UniTask<Mesh> BuildMeshAsync(
            IReadOnlyList<GeneratedWorldDressingRequest> requests,
            GeneratedWorldDressingKind kind,
            RunWorldGenerationConfig config)
        {
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();
            int processed = 0;
            int yieldEvery = config != null ? config.WorldDressingBuildItemsPerFrame : 64;

            for (int i = 0; i < requests.Count; i++)
            {
                GeneratedWorldDressingRequest request = requests[i];
                if (request.Kind != kind)
                {
                    continue;
                }

                AddQuad(request, config, vertices, normals, uvs, triangles);
                processed++;
                if (processed % yieldEvery == 0)
                {
                    await UniTask.Yield(PlayerLoopTiming.Update);
                }
            }

            if (vertices.Count == 0)
            {
                return null;
            }

            var mesh = new Mesh
            {
                name = kind == GeneratedWorldDressingKind.GroundDecal
                    ? "World Dressing Ground Decals Mesh"
                    : "World Dressing Wall Decals Mesh",
                hideFlags = HideFlags.DontSave,
                indexFormat = vertices.Count > ushort.MaxValue ? IndexFormat.UInt32 : IndexFormat.UInt16
            };
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void AddQuad(
            GeneratedWorldDressingRequest request,
            RunWorldGenerationConfig config,
            List<Vector3> vertices,
            List<Vector3> normals,
            List<Vector2> uvs,
            List<int> triangles)
        {
            Vector3 normal = request.Normal.sqrMagnitude > 0.0001f ? request.Normal.normalized : Vector3.up;
            Vector3 right = Vector3.ProjectOnPlane(request.Tangent, normal);
            if (right.sqrMagnitude < 0.0001f)
            {
                right = Vector3.Cross(normal, Vector3.forward);
            }

            if (right.sqrMagnitude < 0.0001f)
            {
                right = Vector3.right;
            }

            right.Normalize();
            Vector3 up = request.Kind == GeneratedWorldDressingKind.WallDecal
                ? Vector3.up
                : Vector3.Cross(normal, right).normalized;
            if (request.Kind == GeneratedWorldDressingKind.WallDecal)
            {
                up = Vector3.ProjectOnPlane(up, normal).normalized;
            }

            if (up.sqrMagnitude < 0.0001f)
            {
                up = Vector3.Cross(normal, right).normalized;
            }

            float halfWidth = request.Size.x * 0.5f;
            float halfHeight = request.Size.y * 0.5f;
            Vector3 center = request.Position;
            int baseIndex = vertices.Count;
            vertices.Add(center - right * halfWidth - up * halfHeight);
            vertices.Add(center - right * halfWidth + up * halfHeight);
            vertices.Add(center + right * halfWidth + up * halfHeight);
            vertices.Add(center + right * halfWidth - up * halfHeight);

            for (int i = 0; i < 4; i++)
            {
                normals.Add(normal);
            }

            AddAtlasUvs(request.VariantIndex, config, uvs);
            triangles.Add(baseIndex);
            triangles.Add(baseIndex + 1);
            triangles.Add(baseIndex + 2);
            triangles.Add(baseIndex);
            triangles.Add(baseIndex + 2);
            triangles.Add(baseIndex + 3);
        }

        private static void AddAtlasUvs(int variantIndex, RunWorldGenerationConfig config, List<Vector2> uvs)
        {
            int variant = Mathf.Abs(variantIndex) % (AtlasColumns * AtlasRows);
            int column = variant % AtlasColumns;
            int row = variant / AtlasColumns;
            float pad = 1f / Mathf.Max(64f, config != null ? config.WorldDressingTextureResolution : 256) * 2f;
            float uMin = column / (float)AtlasColumns + pad;
            float uMax = (column + 1) / (float)AtlasColumns - pad;
            float vMin = row / (float)AtlasRows + pad;
            float vMax = (row + 1) / (float)AtlasRows - pad;
            uvs.Add(new Vector2(uMin, vMin));
            uvs.Add(new Vector2(uMin, vMax));
            uvs.Add(new Vector2(uMax, vMax));
            uvs.Add(new Vector2(uMax, vMin));
        }

        private static void DestroyObject(UnityEngine.Object target)
        {
            RuntimeObjectFactory.Release(target);
        }
    }

    public sealed class WorldDressingMaterialCleanup : MonoBehaviour
    {
        private UnityEngine.Object[] ownedObjects = Array.Empty<UnityEngine.Object>();

        public void Initialize(UnityEngine.Object[] objects)
        {
            ownedObjects = objects ?? Array.Empty<UnityEngine.Object>();
        }

        private void OnDestroy()
        {
            GeneratedWorldSurfaceMaterialSet.DestroyObjects(ownedObjects);
            ownedObjects = Array.Empty<UnityEngine.Object>();
        }
    }

    public readonly struct WorldDressingMaterialSet
    {
        public WorldDressingMaterialSet(Material groundMaterial, Material wallMaterial, UnityEngine.Object[] ownedObjects)
        {
            GroundMaterial = groundMaterial;
            WallMaterial = wallMaterial;
            OwnedObjects = ownedObjects ?? Array.Empty<UnityEngine.Object>();
        }

        public Material GroundMaterial { get; }
        public Material WallMaterial { get; }
        public UnityEngine.Object[] OwnedObjects { get; }
    }

    public static class WorldDressingTextureGenerator
    {
        private const int AtlasColumns = 4;
        private const int AtlasRows = 2;

        public static WorldDressingMaterialSet GenerateMaterialSet(RunWorldGenerationConfig config, int seed, int actNumber)
        {
            Texture2D groundTexture = GenerateAtlas(config, seed, actNumber, wall: false);
            Texture2D wallTexture = GenerateAtlas(config, seed, actNumber, wall: true);
            Material groundMaterial = CreateTransparentMaterial("World Dressing Ground Material", groundTexture);
            Material wallMaterial = CreateTransparentMaterial("World Dressing Wall Material", wallTexture);
            return new WorldDressingMaterialSet(
                groundMaterial,
                wallMaterial,
                new UnityEngine.Object[] { groundTexture, wallTexture, groundMaterial, wallMaterial });
        }

        private static Texture2D GenerateAtlas(RunWorldGenerationConfig config, int seed, int actNumber, bool wall)
        {
            int resolution = config != null ? config.WorldDressingTextureResolution : 256;
            int width = resolution;
            int height = Mathf.Max(64, resolution / 2);
            int tileWidth = width / AtlasColumns;
            int tileHeight = height / AtlasRows;
            var pixels = new Color32[width * height];
            int atlasSeed = DeterministicSeed.Combine(seed, actNumber, wall ? 7723 : 4547);
            float opacity = wall
                ? (config != null ? config.WorldDressingWallOpacity : 0.72f)
                : (config != null ? config.WorldDressingGroundOpacity : 0.58f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int tileX = Mathf.Clamp(x / tileWidth, 0, AtlasColumns - 1);
                    int tileY = Mathf.Clamp(y / tileHeight, 0, AtlasRows - 1);
                    int variant = tileY * AtlasColumns + tileX;
                    float u = (x - tileX * tileWidth + 0.5f) / tileWidth;
                    float v = (y - tileY * tileHeight + 0.5f) / tileHeight;
                    pixels[y * width + x] = wall
                        ? WallPixel(variant, u, v, atlasSeed, opacity)
                        : GroundPixel(variant, u, v, atlasSeed, opacity);
                }
            }

            var texture = new Texture2D(width, height, TextureFormat.RGBA32, mipChain: true, linear: false)
            {
                name = wall ? "World Dressing Wall Atlas" : "World Dressing Ground Atlas",
                hideFlags = HideFlags.DontSave,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                anisoLevel = 2
            };
            texture.SetPixels32(pixels);
            texture.Apply(updateMipmaps: true, makeNoLongerReadable: true);
            return texture;
        }

        private static Color32 GroundPixel(int variant, float u, float v, int seed, float opacity)
        {
            float alpha = 0f;
            Color color = new(0.35f, 0.28f, 0.18f, 1f);
            float noise = Noise(seed + variant * 53, u, v);
            switch (variant)
            {
                case 0:
                    alpha = TicketScrapAlpha(u, v, seed);
                    color = Color.Lerp(new Color(0.85f, 0.64f, 0.18f, 1f), new Color(0.72f, 0.08f, 0.05f, 1f), noise);
                    break;
                case 1:
                    alpha = IrregularBlob(u, v, seed, radius: 0.48f) * 0.75f;
                    color = new Color(0.42f, 0.31f, 0.17f, 1f);
                    break;
                case 2:
                    alpha = ScuffAlpha(u, v, seed) * 0.75f;
                    color = new Color(0.055f, 0.05f, 0.045f, 1f);
                    break;
                case 3:
                    alpha = RingMarkAlpha(u, v, seed) * 0.65f;
                    color = new Color(0.72f, 0.62f, 0.45f, 1f);
                    break;
                case 4:
                    alpha = ArrowAlpha(u, v) * 0.62f;
                    color = new Color(0.66f, 0.55f, 0.36f, 1f);
                    break;
                case 5:
                    alpha = IrregularBlob(u, v, seed + 17, radius: 0.36f) * 0.55f;
                    color = new Color(0.11f, 0.08f, 0.045f, 1f);
                    break;
                case 6:
                    alpha = TicketScrapAlpha(u * 1.7f, v * 1.35f, seed + 29) * 0.8f;
                    color = Color.Lerp(new Color(0.88f, 0.72f, 0.26f, 1f), new Color(0.08f, 0.22f, 0.24f, 1f), noise);
                    break;
                default:
                    alpha = WornPathAlpha(u, v, seed) * 0.58f;
                    color = new Color(0.19f, 0.14f, 0.08f, 1f);
                    break;
            }

            color = Color.Lerp(color, Color.black, Noise(seed + 1009, u * 5f, v * 5f) * 0.22f);
            return ToColor32(color, alpha * opacity);
        }

        private static Color32 WallPixel(int variant, float u, float v, int seed, float opacity)
        {
            float alpha = 0f;
            Color color = new(0.52f, 0.05f, 0.04f, 1f);
            float edgeWear = Mathf.Max(1f - Mathf.Min(u, 1f - u) * 12f, 1f - Mathf.Min(v, 1f - v) * 12f);
            float noise = Noise(seed + variant * 71, u, v);
            switch (variant)
            {
                case 0:
                    alpha = PosterAlpha(u, v, seed);
                    color = Color.Lerp(new Color(0.58f, 0.04f, 0.035f, 1f), new Color(0.83f, 0.56f, 0.16f, 1f), BorderAlpha(u, v));
                    break;
                case 1:
                    alpha = TrimAlpha(u, v) * 0.82f;
                    color = new Color(0.78f, 0.52f, 0.14f, 1f);
                    break;
                case 2:
                    alpha = VerticalStreakAlpha(u, v, seed) * 0.9f;
                    color = new Color(0.035f, 0.025f, 0.02f, 1f);
                    break;
                case 3:
                    alpha = Mathf.Clamp01(edgeWear) * 0.70f;
                    color = new Color(0.82f, 0.58f, 0.25f, 1f);
                    break;
                case 4:
                    alpha = FadedBandAlpha(u, v, diagonal: true) * 0.65f;
                    color = new Color(0.05f, 0.12f, 0.14f, 1f);
                    break;
                case 5:
                    alpha = IrregularBlob(u, v, seed + 43, radius: 0.42f) * 0.48f;
                    color = new Color(0.06f, 0.035f, 0.025f, 1f);
                    break;
                case 6:
                    alpha = FadedBandAlpha(u, v, diagonal: false) * 0.72f;
                    color = new Color(0.34f, 0.015f, 0.02f, 1f);
                    break;
                default:
                    alpha = PosterAlpha(u, v, seed + 89) * 0.65f;
                    color = Color.Lerp(new Color(0.08f, 0.14f, 0.15f, 1f), new Color(0.74f, 0.48f, 0.13f, 1f), BorderAlpha(u, v));
                    break;
            }

            color = Color.Lerp(color, Color.black, noise * 0.24f);
            return ToColor32(color, alpha * opacity);
        }

        private static Material CreateTransparentMaterial(string name, Texture texture)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Unlit/Transparent")
                ?? Shader.Find("Sprites/Default")
                ?? Shader.Find("Standard");
            var material = new Material(shader)
            {
                name = name,
                hideFlags = HideFlags.DontSave,
                renderQueue = (int)RenderQueue.Transparent
            };
            SetTextureIfPresent(material, "_BaseMap", texture);
            SetTextureIfPresent(material, "_MainTex", texture);
            SetColorIfPresent(material, "_BaseColor", Color.white);
            SetColorIfPresent(material, "_Color", Color.white);
            SetFloatIfPresent(material, "_Surface", 1f);
            SetFloatIfPresent(material, "_Blend", 0f);
            SetFloatIfPresent(material, "_AlphaClip", 0f);
            SetFloatIfPresent(material, "_ZWrite", 0f);
            SetFloatIfPresent(material, "_Cull", (float)CullMode.Off);
            SetFloatIfPresent(material, "_SrcBlend", (float)BlendMode.SrcAlpha);
            SetFloatIfPresent(material, "_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            return material;
        }

        private static float TicketScrapAlpha(float u, float v, int seed)
        {
            float cellU = Mathf.Repeat(u * 7f, 1f);
            float cellV = Mathf.Repeat(v * 6f, 1f);
            int cellX = Mathf.FloorToInt(u * 7f);
            int cellY = Mathf.FloorToInt(v * 6f);
            float enabled = Noise(seed + 11, cellX, cellY) > 0.78f ? 1f : 0f;
            float rect = cellU > 0.28f && cellU < 0.72f && cellV > 0.36f && cellV < 0.64f ? 1f : 0f;
            return enabled * rect;
        }

        private static float IrregularBlob(float u, float v, int seed, float radius)
        {
            Vector2 center = new(0.5f + (Noise(seed + 3, 0f, 0f) - 0.5f) * 0.12f, 0.5f + (Noise(seed + 5, 1f, 0f) - 0.5f) * 0.12f);
            float distance = Vector2.Distance(new Vector2(u, v), center);
            float edge = radius + (Noise(seed + 7, u * 5f, v * 5f) - 0.5f) * 0.18f;
            return Mathf.SmoothStep(1f, 0f, distance / Mathf.Max(0.01f, edge));
        }

        private static float ScuffAlpha(float u, float v, int seed)
        {
            float line = 0.52f + Mathf.Sin((u + Noise(seed, 0f, 0f)) * Mathf.PI * 3.2f) * 0.07f;
            float width = Mathf.Lerp(0.018f, 0.045f, Noise(seed + 2, u * 2f, v * 2f));
            float fade = Mathf.SmoothStep(0.02f, 0.28f, u) * Mathf.SmoothStep(0.98f, 0.72f, u);
            return Mathf.Clamp01(1f - Mathf.Abs(v - line) / width) * fade;
        }

        private static float RingMarkAlpha(float u, float v, int seed)
        {
            float distance = Vector2.Distance(new Vector2(u, v), new Vector2(0.5f, 0.5f));
            float ring = Mathf.Clamp01(1f - Mathf.Abs(distance - 0.34f) / 0.025f);
            float gap = Noise(seed + 41, Mathf.Floor(u * 8f), Mathf.Floor(v * 8f)) > 0.24f ? 1f : 0f;
            return ring * gap;
        }

        private static float ArrowAlpha(float u, float v)
        {
            float shaft = u > 0.18f && u < 0.68f && Mathf.Abs(v - 0.5f) < 0.035f ? 1f : 0f;
            float head = u > 0.55f && u < 0.84f && Mathf.Abs(v - 0.5f) < (0.84f - u) * 0.55f ? 1f : 0f;
            return Mathf.Max(shaft, head);
        }

        private static float WornPathAlpha(float u, float v, int seed)
        {
            float center = 0.5f + Mathf.Sin(u * Mathf.PI * 2f + Noise(seed, 0f, 1f) * 3f) * 0.12f;
            float stripe = Mathf.Clamp01(1f - Mathf.Abs(v - center) / 0.22f);
            return stripe * (0.45f + Noise(seed + 9, u * 6f, v * 6f) * 0.55f);
        }

        private static float PosterAlpha(float u, float v, int seed)
        {
            float rect = u > 0.12f && u < 0.88f && v > 0.10f && v < 0.90f ? 1f : 0f;
            float tear = Noise(seed + 31, u * 8f, v * 8f) > 0.92f ? 0f : 1f;
            return rect * tear;
        }

        private static float BorderAlpha(float u, float v)
        {
            float edge = Mathf.Min(Mathf.Min(u, 1f - u), Mathf.Min(v, 1f - v));
            return edge < 0.18f ? 1f : 0f;
        }

        private static float TrimAlpha(float u, float v)
        {
            float horizontal = Mathf.Abs(v - 0.5f) < 0.09f ? 1f : 0f;
            float line = Mathf.Abs(v - 0.36f) < 0.018f || Mathf.Abs(v - 0.64f) < 0.018f ? 1f : 0f;
            return Mathf.Max(horizontal * 0.75f, line);
        }

        private static float VerticalStreakAlpha(float u, float v, int seed)
        {
            int lane = Mathf.FloorToInt(u * 9f);
            float laneStrength = Noise(seed + 67, lane, 0f);
            float local = Mathf.Repeat(u * 9f, 1f);
            float streak = Mathf.Clamp01(1f - Mathf.Abs(local - 0.5f) / 0.22f);
            float gravity = Mathf.SmoothStep(1f, 0.15f, v);
            return laneStrength > 0.52f ? streak * gravity : 0f;
        }

        private static float FadedBandAlpha(float u, float v, bool diagonal)
        {
            float value = diagonal ? Mathf.Repeat(u + v, 1f) : v;
            return Mathf.Abs(value - 0.5f) < 0.11f ? 1f : 0f;
        }

        private static float Noise(int seed, float x, float y)
        {
            int sx = Mathf.FloorToInt(x * 4096f);
            int sy = Mathf.FloorToInt(y * 4096f);
            return DeterministicSeed.ToFloat01(DeterministicSeed.Combine(seed, sx, sy));
        }

        private static Color32 ToColor32(Color color, float alpha)
        {
            color.a = Mathf.Clamp01(alpha);
            return color;
        }

        private static void SetTextureIfPresent(Material material, string property, Texture value)
        {
            if (material != null && value != null && material.HasProperty(property))
            {
                material.SetTexture(property, value);
            }
        }

        private static void SetColorIfPresent(Material material, string property, Color value)
        {
            if (material != null && material.HasProperty(property))
            {
                material.SetColor(property, value);
            }
        }

        private static void SetFloatIfPresent(Material material, string property, float value)
        {
            if (material != null && material.HasProperty(property))
            {
                material.SetFloat(property, value);
            }
        }
    }
}
