using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using UnityEngine;

namespace TheCircussyOne.Runtime
{
    public readonly struct WorldGenerationRequest
    {
        public WorldGenerationRequest(int worldIndex, int actNumber, int seed)
        {
            WorldIndex = worldIndex < 1 ? 1 : worldIndex;
            ActNumber = actNumber < 1 ? WorldIndex : actNumber;
            Seed = seed;
        }

        public int WorldIndex { get; }
        public int ActNumber { get; }
        public int Seed { get; }
    }

    public readonly struct WorldEnemySpawnBand
    {
        public WorldEnemySpawnBand(Vector3 center, float innerRadius, float outerRadius)
        {
            Center = center;
            InnerRadius = Mathf.Max(0f, innerRadius);
            OuterRadius = Mathf.Max(InnerRadius, outerRadius);
        }

        public Vector3 Center { get; }
        public float InnerRadius { get; }
        public float OuterRadius { get; }
    }

    [Flags]
    public enum GeneratedWorldMask
    {
        None = 0,
        Walkable = 1 << 0,
        Reachable = 1 << 1,
        RewardSafe = 1 << 2,
        SpawnSafe = 1 << 3,
        PlayerStartSafe = 1 << 4
    }

    public enum GeneratedWorldSurfaceKind
    {
        Flat = 0,
        Ramp = 1,
        Continuous = 2
    }

    public readonly struct GeneratedWorldSample
    {
        public GeneratedWorldSample(
            float height,
            Vector3 normal,
            float slopeDegrees,
            GeneratedWorldMask mask,
            float highGroundScore,
            float centerDistanceScore,
            float boundaryDistanceScore,
            GeneratedWorldSurfaceKind surfaceKind = GeneratedWorldSurfaceKind.Flat,
            Vector2 surfaceDirection = default)
        {
            Height = height;
            Normal = normal.sqrMagnitude > 0.0001f ? normal.normalized : Vector3.up;
            SlopeDegrees = Mathf.Max(0f, slopeDegrees);
            Mask = mask;
            HighGroundScore = Mathf.Clamp01(highGroundScore);
            CenterDistanceScore = Mathf.Clamp01(centerDistanceScore);
            BoundaryDistanceScore = Mathf.Clamp01(boundaryDistanceScore);
            SurfaceKind = surfaceKind;
            SurfaceDirection = surfaceDirection.sqrMagnitude > 0.0001f ? surfaceDirection.normalized : Vector2.zero;
        }

        public float Height { get; }
        public Vector3 Normal { get; }
        public float SlopeDegrees { get; }
        public GeneratedWorldMask Mask { get; }
        public float HighGroundScore { get; }
        public float CenterDistanceScore { get; }
        public float BoundaryDistanceScore { get; }
        public GeneratedWorldSurfaceKind SurfaceKind { get; }
        public Vector2 SurfaceDirection { get; }
        public bool IsRamp => SurfaceKind == GeneratedWorldSurfaceKind.Ramp;

        public bool HasMask(GeneratedWorldMask mask)
        {
            return (Mask & mask) == mask;
        }

        public GeneratedWorldSample WithMask(GeneratedWorldMask mask)
        {
            return new GeneratedWorldSample(
                Height,
                Normal,
                SlopeDegrees,
                mask,
                HighGroundScore,
                CenterDistanceScore,
                BoundaryDistanceScore,
                SurfaceKind,
                SurfaceDirection);
        }
    }

    public sealed class GeneratedWorldMap
    {
        private readonly GeneratedWorldSample[] samples;
        private readonly string[] warnings;

        public GeneratedWorldMap(
            int actNumber,
            int seed,
            int attemptSeed,
            int width,
            int depth,
            float playableRadius,
            float gridSpacing,
            float maxHeight,
            float wallHeightThreshold,
            GeneratedWorldSample[] samples,
            int playerStartIndex,
            bool usedFallback,
            IReadOnlyList<string> warnings,
            bool hasStructuralValidation = false,
            BlockyCircusGroundsSandboxValidation structuralValidation = default)
        {
            ActNumber = Mathf.Max(1, actNumber);
            Seed = seed;
            AttemptSeed = attemptSeed;
            Width = Mathf.Max(1, width);
            Depth = Mathf.Max(1, depth);
            PlayableRadius = Mathf.Max(1f, playableRadius);
            GridSpacing = Mathf.Max(0.01f, gridSpacing);
            MaxHeight = Mathf.Max(0f, maxHeight);
            WallHeightThreshold = Mathf.Max(0f, wallHeightThreshold);
            this.samples = samples ?? Array.Empty<GeneratedWorldSample>();
            PlayerStartIndex = Mathf.Clamp(playerStartIndex, 0, Mathf.Max(0, this.samples.Length - 1));
            UsedFallback = usedFallback;
            this.warnings = warnings != null ? CopyWarnings(warnings) : Array.Empty<string>();
            HasStructuralValidation = hasStructuralValidation;
            StructuralValidation = structuralValidation;
            Bounds = new Bounds(
                Vector3.up * (MaxHeight * 0.5f),
                new Vector3(PlayableRadius * 2f, Mathf.Max(1f, MaxHeight), PlayableRadius * 2f));
        }

        public int ActNumber { get; }
        public int Seed { get; }
        public int AttemptSeed { get; }
        public int Width { get; }
        public int Depth { get; }
        public float PlayableRadius { get; }
        public float GridSpacing { get; }
        public float MaxHeight { get; }
        public float WallHeightThreshold { get; }
        public int PlayerStartIndex { get; }
        public bool UsedFallback { get; }
        public Bounds Bounds { get; }
        public IReadOnlyList<GeneratedWorldSample> Samples => samples;
        public IReadOnlyList<string> Warnings => warnings;
        public bool HasStructuralValidation { get; }
        public BlockyCircusGroundsSandboxValidation StructuralValidation { get; }

        public Vector3 PlayerStartPosition => PositionForIndex(PlayerStartIndex);

        public bool TryGetSample(int x, int z, out GeneratedWorldSample sample)
        {
            if (x < 0 || z < 0 || x >= Width || z >= Depth)
            {
                sample = default;
                return false;
            }

            int index = Index(x, z);
            if (index < 0 || index >= samples.Length)
            {
                sample = default;
                return false;
            }

            sample = samples[index];
            return true;
        }

        public int Index(int x, int z)
        {
            return z * Width + x;
        }

        public Vector3 PositionForIndex(int index)
        {
            int safeIndex = Mathf.Clamp(index, 0, Mathf.Max(0, samples.Length - 1));
            int x = safeIndex % Width;
            int z = safeIndex / Width;
            return PositionFor(x, z);
        }

        public Vector3 PositionFor(int x, int z)
        {
            int safeX = Mathf.Clamp(x, 0, Width - 1);
            int safeZ = Mathf.Clamp(z, 0, Depth - 1);
            GeneratedWorldSample sample = samples.Length > 0 ? samples[Index(safeX, safeZ)] : default;
            float halfX = (Width - 1) * GridSpacing * 0.5f;
            float halfZ = (Depth - 1) * GridSpacing * 0.5f;
            return new Vector3(safeX * GridSpacing - halfX, sample.Height, safeZ * GridSpacing - halfZ);
        }

        public int CountMask(GeneratedWorldMask mask)
        {
            int count = 0;
            for (int i = 0; i < samples.Length; i++)
            {
                if (samples[i].HasMask(mask))
                {
                    count++;
                }
            }

            return count;
        }

        private static string[] CopyWarnings(IReadOnlyList<string> source)
        {
            var copy = new string[source.Count];
            for (int i = 0; i < source.Count; i++)
            {
                copy[i] = source[i];
            }

            return copy;
        }
    }

    public readonly struct GeneratedWorldLayout
    {
        public GeneratedWorldLayout(GeneratedWorldMap map)
        {
            Map = map;
        }

        public GeneratedWorldMap Map { get; }
        public bool HasMap => Map != null;
        public int ActNumber => Map != null ? Map.ActNumber : 1;
        public Bounds Bounds => Map != null ? Map.Bounds : default;
    }

    public readonly struct WorldGenerationResult
    {
        private readonly IReadOnlyList<WorldRewardSpawnRequest> rewardSpawnRequests;
        private readonly IReadOnlyList<WorldBlockingObstacleSpawnRequest> obstacleSpawnRequests;
        private readonly IReadOnlyList<WorldEnemySpawnBand> enemySpawnBands;

        public WorldGenerationResult(int worldIndex, int actNumber, int seed, bool usedPrototypePlacement)
            : this(
                worldIndex,
                actNumber,
                seed,
                usedPrototypePlacement,
                generatedRoot: null,
                rewardSpawnRequests: null,
                enemySpawnBands: null,
                playerStart: Vector3.zero,
                hasPlayerStart: false,
                headlinerAnchor: Vector3.zero,
                hasHeadlinerAnchor: false,
                stageDoorAnchor: Vector3.zero,
                hasStageDoorAnchor: false,
                bounds: default,
                generatedMap: null,
                obstacleSpawnRequests: null)
        {
        }

        public WorldGenerationResult(
            int worldIndex,
            int actNumber,
            int seed,
            bool usedPrototypePlacement,
            GameObject generatedRoot,
            IReadOnlyList<WorldRewardSpawnRequest> rewardSpawnRequests,
            IReadOnlyList<WorldEnemySpawnBand> enemySpawnBands,
            Vector3 playerStart,
            bool hasPlayerStart,
            Vector3 headlinerAnchor,
            bool hasHeadlinerAnchor,
            Vector3 stageDoorAnchor,
            bool hasStageDoorAnchor,
            Bounds bounds,
            GeneratedWorldMap generatedMap = null,
            IReadOnlyList<WorldBlockingObstacleSpawnRequest> obstacleSpawnRequests = null)
        {
            WorldIndex = worldIndex < 1 ? 1 : worldIndex;
            ActNumber = actNumber < 1 ? WorldIndex : actNumber;
            Seed = seed;
            UsedPrototypePlacement = usedPrototypePlacement;
            GeneratedRoot = generatedRoot;
            this.rewardSpawnRequests = rewardSpawnRequests;
            this.obstacleSpawnRequests = obstacleSpawnRequests;
            this.enemySpawnBands = enemySpawnBands;
            PlayerStart = playerStart;
            HasPlayerStart = hasPlayerStart;
            HeadlinerAnchor = headlinerAnchor;
            HasHeadlinerAnchor = hasHeadlinerAnchor;
            StageDoorAnchor = stageDoorAnchor;
            HasStageDoorAnchor = hasStageDoorAnchor;
            Bounds = bounds;
            GeneratedMap = generatedMap;
        }

        public int WorldIndex { get; }
        public int ActNumber { get; }
        public int Seed { get; }
        public bool UsedPrototypePlacement { get; }
        public GameObject GeneratedRoot { get; }
        public IReadOnlyList<WorldRewardSpawnRequest> RewardSpawnRequests => rewardSpawnRequests ?? Array.Empty<WorldRewardSpawnRequest>();
        public IReadOnlyList<WorldBlockingObstacleSpawnRequest> ObstacleSpawnRequests => obstacleSpawnRequests ?? Array.Empty<WorldBlockingObstacleSpawnRequest>();
        public IReadOnlyList<WorldEnemySpawnBand> EnemySpawnBands => enemySpawnBands ?? Array.Empty<WorldEnemySpawnBand>();
        public Vector3 PlayerStart { get; }
        public bool HasPlayerStart { get; }
        public Vector3 HeadlinerAnchor { get; }
        public bool HasHeadlinerAnchor { get; }
        public Vector3 StageDoorAnchor { get; }
        public bool HasStageDoorAnchor { get; }
        public Bounds Bounds { get; }
        public GeneratedWorldMap GeneratedMap { get; }
        public bool HasGeneratedMap => GeneratedMap != null;
        public bool HasGeneratedLayout => !UsedPrototypePlacement && (GeneratedRoot != null || RewardSpawnRequests.Count > 0);
    }

    public static class GeneratedWorldRewardPlanner
    {
        private const int RewardAngularSectors = 8;
        private const int RewardRadialBands = 3;
        private const int RewardHeightBands = 3;

        public static IReadOnlyList<WorldRewardSpawnRequest> BuildRequests(
            GeneratedWorldMap map,
            RunWorldGenerationProfile profile,
            RunWorldGenerationConfig config,
            IReadOnlyList<WorldBlockingObstacleSpawnRequest> obstacleRequests = null)
        {
            if (map == null || profile == null || config == null)
            {
                return Array.Empty<WorldRewardSpawnRequest>();
            }

            RewardPlanSettings settings = RewardPlanSettings.From(profile, config);
            return BuildRequests(map, settings, obstacleRequests);
        }

        public static async UniTask<IReadOnlyList<WorldRewardSpawnRequest>> BuildRequestsAsync(
            GeneratedWorldMap map,
            RunWorldGenerationProfile profile,
            RunWorldGenerationConfig config,
            IReadOnlyList<WorldBlockingObstacleSpawnRequest> obstacleRequests = null)
        {
            if (map == null || profile == null || config == null)
            {
                return Array.Empty<WorldRewardSpawnRequest>();
            }

            RewardPlanSettings settings = RewardPlanSettings.From(profile, config);
            IReadOnlyList<WorldRewardSpawnRequest> requests =
                await UniTask.RunOnThreadPool(() => BuildRequests(map, settings, obstacleRequests));
            await UniTask.SwitchToMainThread();
            return requests;
        }

        private static IReadOnlyList<WorldRewardSpawnRequest> BuildRequests(
            GeneratedWorldMap map,
            RewardPlanSettings settings,
            IReadOnlyList<WorldBlockingObstacleSpawnRequest> obstacleRequests)
        {
            var requests = new List<WorldRewardSpawnRequest>();
            var selected = new SelectedRewardIndex(map.GridSpacing, map.AttemptSeed);
            var sectorUseCounts = new Dictionary<int, int>();
            var candidateCache = new Dictionary<RewardPlacementLane, List<Candidate>>();
            for (int i = 0; i < settings.Entries.Count; i++)
            {
                RewardPlanEntry entry = settings.Entries[i];
                if (entry.Count <= 0)
                {
                    continue;
                }

                if (!candidateCache.TryGetValue(entry.Lane, out List<Candidate> candidates))
                {
                    candidates = BuildCandidates(map, settings, entry.Lane, obstacleRequests);
                    candidateCache[entry.Lane] = candidates;
                }

                AddRequests(
                    map,
                    settings,
                    requests,
                    selected,
                    sectorUseCounts,
                    entry,
                    candidates);
            }

            return requests;
        }

        private static void AddRequests(
            GeneratedWorldMap map,
            RewardPlanSettings settings,
            List<WorldRewardSpawnRequest> requests,
            SelectedRewardIndex selected,
            Dictionary<int, int> sectorUseCounts,
            RewardPlanEntry entry,
            IReadOnlyList<Candidate> candidates)
        {
            if (entry.Count <= 0)
            {
                return;
            }

            List<ScoredCandidate> orderedCandidates = BuildOrderedCandidates(
                candidates,
                settings,
                sectorUseCounts,
                selected,
                entry);

            int emitted = 0;
            for (int i = 0; i < orderedCandidates.Count && emitted < entry.Count; i++)
            {
                Candidate candidate = orderedCandidates[i].Candidate;
                if (!selected.HasSpacing(candidate.Position, entry.Spacing))
                {
                    continue;
                }

                emitted++;
                selected.Add(candidate.Position);
                IncrementSectorUse(sectorUseCounts, candidate.SectorKey);
                requests.Add(new WorldRewardSpawnRequest(
                    $"{KindId(entry.Kind)}_{LaneId(entry.Lane)}_{emitted:00}",
                    entry.Kind,
                    targetContentId: null,
                    candidate.Position,
                    required: false,
                    repeatSequentialContent: true,
                    generated: true));
            }
        }

        private static List<ScoredCandidate> BuildOrderedCandidates(
            IReadOnlyList<Candidate> candidates,
            RewardPlanSettings settings,
            IReadOnlyDictionary<int, int> sectorUseCounts,
            SelectedRewardIndex selected,
            RewardPlanEntry entry)
        {
            var ordered = new List<ScoredCandidate>(candidates.Count);
            for (int i = 0; i < candidates.Count; i++)
            {
                Candidate candidate = candidates[i];
                float spreadPenalty = SpreadPenalty(
                    candidate,
                    settings.RewardSpreadWeight,
                    sectorUseCounts);
                float kindNoise = DeterministicSeed.ToFloat01(DeterministicSeed.Combine(
                    selected.Seed,
                    candidate.SampleIndex,
                    (int)entry.Kind,
                    (int)entry.Lane,
                    5729)) * 0.2f;
                ordered.Add(new ScoredCandidate(candidate, candidate.Score + kindNoise - spreadPenalty));
            }

            ordered.Sort(static (left, right) => right.Score.CompareTo(left.Score));
            return ordered;
        }

        private static List<Candidate> BuildCandidates(
            GeneratedWorldMap map,
            RewardPlanSettings settings,
            RewardPlacementLane lane,
            IReadOnlyList<WorldBlockingObstacleSpawnRequest> obstacleRequests)
        {
            var candidates = new List<Candidate>();
            for (int i = 0; i < map.Samples.Count; i++)
            {
                GeneratedWorldSample sample = map.Samples[i];
                if (!IsCandidateForLane(sample, lane))
                {
                    continue;
                }

                float score = sample.BoundaryDistanceScore * (lane == RewardPlacementLane.Pocket ? 0.25f : 0.08f);
                if (settings.CenterBiasEnabled)
                {
                    float centerWeight = lane == RewardPlacementLane.Pocket ? settings.RewardCenterWeight : settings.RewardCenterWeight * 0.35f;
                    score += sample.CenterDistanceScore * Mathf.Max(0f, centerWeight);
                }

                if (settings.HighGroundBiasEnabled)
                {
                    score += sample.HighGroundScore * Mathf.Max(0f, settings.RewardHighGroundWeight);
                }

                if (lane == RewardPlacementLane.Loose && sample.IsRamp)
                {
                    score += 0.18f;
                }

                Vector3 position = map.PositionForIndex(i);
                if (TooCloseToObstacles(position, obstacleRequests))
                {
                    continue;
                }

                candidates.Add(new Candidate(i, position, score, SectorKey(map, position)));
            }

            return candidates;
        }

        private static bool IsCandidateForLane(GeneratedWorldSample sample, RewardPlacementLane lane)
        {
            return lane switch
            {
                RewardPlacementLane.Pocket => sample.HasMask(GeneratedWorldMask.RewardSafe),
                RewardPlacementLane.Loose => sample.HasMask(GeneratedWorldMask.Reachable)
                    && !sample.HasMask(GeneratedWorldMask.RewardSafe),
                _ => false
            };
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

        private static float SpreadPenalty(
            Candidate candidate,
            float spreadWeight,
            IReadOnlyDictionary<int, int> sectorUseCounts)
        {
            if (spreadWeight <= 0f)
            {
                return 0f;
            }

            float penalty = 0f;
            if (sectorUseCounts.TryGetValue(candidate.SectorKey, out int sectorCount))
            {
                penalty += sectorCount * 0.38f;
            }

            return penalty * spreadWeight;
        }

        private static void IncrementSectorUse(IDictionary<int, int> sectorUseCounts, int sectorKey)
        {
            sectorUseCounts.TryGetValue(sectorKey, out int count);
            sectorUseCounts[sectorKey] = count + 1;
        }

        private static int SectorKey(GeneratedWorldMap map, Vector3 position)
        {
            Vector2 position2 = new(position.x, position.z);
            float normalizedAngle = (Mathf.Atan2(position2.y, position2.x) + Mathf.PI) / (Mathf.PI * 2f);
            int angularSector = Mathf.Clamp(
                Mathf.FloorToInt(normalizedAngle * RewardAngularSectors),
                0,
                RewardAngularSectors - 1);
            int radialBand = Mathf.Clamp(
                Mathf.FloorToInt(Mathf.Clamp01(position2.magnitude / Mathf.Max(1f, map.PlayableRadius)) * RewardRadialBands),
                0,
                RewardRadialBands - 1);
            int heightBand = Mathf.Clamp(
                Mathf.FloorToInt(Mathf.Clamp01(position.y / Mathf.Max(1f, map.MaxHeight)) * RewardHeightBands),
                0,
                RewardHeightBands - 1);
            return angularSector + RewardAngularSectors * (radialBand + RewardRadialBands * heightBand);
        }

        private static string KindId(WorldRewardPlacementKind kind)
        {
            return kind switch
            {
                WorldRewardPlacementKind.Chest => "chest",
                WorldRewardPlacementKind.TicketDeposit => "ticket_deposit",
                WorldRewardPlacementKind.HealingProp => "healing_prop",
                _ => "reward"
            };
        }

        private static string LaneId(RewardPlacementLane lane)
        {
            return lane == RewardPlacementLane.Loose ? "loose" : "pocket";
        }

        private enum RewardPlacementLane
        {
            Pocket = 0,
            Loose = 1
        }

        private readonly struct RewardPlanEntry
        {
            public RewardPlanEntry(WorldRewardPlacementKind kind, RewardPlacementLane lane, int count, float spacing)
            {
                Kind = kind;
                Lane = lane;
                Count = Mathf.Max(0, count);
                Spacing = Mathf.Max(0f, spacing);
            }

            public WorldRewardPlacementKind Kind { get; }
            public RewardPlacementLane Lane { get; }
            public int Count { get; }
            public float Spacing { get; }
        }

        private readonly struct RewardPlanSettings
        {
            private RewardPlanSettings(
                bool centerBiasEnabled,
                bool highGroundBiasEnabled,
                float rewardCenterWeight,
                float rewardHighGroundWeight,
                float rewardSpreadWeight,
                RewardPlanEntry[] entries)
            {
                CenterBiasEnabled = centerBiasEnabled;
                HighGroundBiasEnabled = highGroundBiasEnabled;
                RewardCenterWeight = Mathf.Max(0f, rewardCenterWeight);
                RewardHighGroundWeight = Mathf.Max(0f, rewardHighGroundWeight);
                RewardSpreadWeight = Mathf.Max(0f, rewardSpreadWeight);
                Entries = entries ?? Array.Empty<RewardPlanEntry>();
            }

            public bool CenterBiasEnabled { get; }
            public bool HighGroundBiasEnabled { get; }
            public float RewardCenterWeight { get; }
            public float RewardHighGroundWeight { get; }
            public float RewardSpreadWeight { get; }
            public IReadOnlyList<RewardPlanEntry> Entries { get; }

            public static RewardPlanSettings From(RunWorldGenerationProfile profile, RunWorldGenerationConfig config)
            {
                return new RewardPlanSettings(
                    config.centerBiasEnabled,
                    config.highGroundBiasEnabled,
                    config.rewardCenterWeight,
                    config.rewardHighGroundWeight,
                    config.RewardSpreadWeight,
                    new[]
                    {
                        new RewardPlanEntry(WorldRewardPlacementKind.Chest, RewardPlacementLane.Pocket, profile.GeneratedChestCount, config.GeneratedChestSpacing),
                        new RewardPlanEntry(WorldRewardPlacementKind.TicketDeposit, RewardPlacementLane.Pocket, profile.GeneratedTicketDepositCount, config.GeneratedTicketDepositSpacing),
                        new RewardPlanEntry(WorldRewardPlacementKind.HealingProp, RewardPlacementLane.Pocket, profile.GeneratedHealingPropCount, config.GeneratedHealingPropSpacing),
                        new RewardPlanEntry(WorldRewardPlacementKind.Chest, RewardPlacementLane.Loose, profile.GeneratedLooseChestCount, config.GeneratedLooseChestSpacing),
                        new RewardPlanEntry(WorldRewardPlacementKind.TicketDeposit, RewardPlacementLane.Loose, profile.GeneratedLooseTicketDepositCount, config.GeneratedLooseTicketDepositSpacing),
                        new RewardPlanEntry(WorldRewardPlacementKind.HealingProp, RewardPlacementLane.Loose, profile.GeneratedLooseHealingPropCount, config.GeneratedLooseHealingPropSpacing)
                    });
            }
        }

        private sealed class SelectedRewardIndex
        {
            private readonly Dictionary<Vector2Int, List<Vector3>> cells = new();
            private readonly float cellSize;

            public SelectedRewardIndex(float gridSpacing, int seed = 0)
            {
                cellSize = Mathf.Max(1f, gridSpacing);
                Seed = seed;
            }

            public int Seed { get; }

            public void Add(Vector3 position)
            {
                Vector2Int cell = CellFor(position);
                if (!cells.TryGetValue(cell, out List<Vector3> positions))
                {
                    positions = new List<Vector3>();
                    cells[cell] = positions;
                }

                positions.Add(position);
            }

            public bool HasSpacing(Vector3 position, float spacing)
            {
                if (spacing <= 0f || cells.Count == 0)
                {
                    return true;
                }

                float spacingSqr = spacing * spacing;
                Vector2 candidate = new(position.x, position.z);
                Vector2Int center = CellFor(position);
                int range = Mathf.CeilToInt(spacing / cellSize);
                for (int z = -range; z <= range; z++)
                {
                    for (int x = -range; x <= range; x++)
                    {
                        var cell = new Vector2Int(center.x + x, center.y + z);
                        if (!cells.TryGetValue(cell, out List<Vector3> positions))
                        {
                            continue;
                        }

                        for (int i = 0; i < positions.Count; i++)
                        {
                            Vector3 used = positions[i];
                            Vector2 used2 = new(used.x, used.z);
                            if ((candidate - used2).sqrMagnitude < spacingSqr)
                            {
                                return false;
                            }
                        }
                    }
                }

                return true;
            }

            private Vector2Int CellFor(Vector3 position)
            {
                return new Vector2Int(
                    Mathf.FloorToInt(position.x / cellSize),
                    Mathf.FloorToInt(position.z / cellSize));
            }
        }

        private readonly struct Candidate
        {
            public Candidate(int sampleIndex, Vector3 position, float score, int sectorKey)
            {
                SampleIndex = sampleIndex;
                Position = position;
                Score = score;
                SectorKey = sectorKey;
            }

            public int SampleIndex { get; }
            public Vector3 Position { get; }
            public float Score { get; }
            public int SectorKey { get; }
        }

        private readonly struct ScoredCandidate
        {
            public ScoredCandidate(Candidate candidate, float score)
            {
                Candidate = candidate;
                Score = score;
            }

            public Candidate Candidate { get; }
            public float Score { get; }
        }
    }

    public static class GeneratedWorldSpawnPointResolver
    {
        public static bool TryResolve(
            GeneratedWorldMap map,
            Vector3 desiredPosition,
            float searchRadius,
            out Vector3 resolvedPosition)
        {
            return TryResolve(map, desiredPosition, searchRadius, GeneratedWorldSpawnClearance.None, out resolvedPosition);
        }

        public static bool TryResolve(
            GeneratedWorldMap map,
            Vector3 desiredPosition,
            float searchRadius,
            GeneratedWorldSpawnClearance clearance,
            out Vector3 resolvedPosition)
        {
            return TryResolve(map, desiredPosition, searchRadius, clearance, limitSearchRadius: true, out resolvedPosition);
        }

        public static bool TryResolveNearest(
            GeneratedWorldMap map,
            Vector3 desiredPosition,
            GeneratedWorldSpawnClearance clearance,
            out Vector3 resolvedPosition)
        {
            return TryResolve(map, desiredPosition, searchRadius: 0f, clearance, limitSearchRadius: false, out resolvedPosition);
        }

        private static bool TryResolve(
            GeneratedWorldMap map,
            Vector3 desiredPosition,
            float searchRadius,
            GeneratedWorldSpawnClearance clearance,
            bool limitSearchRadius,
            out Vector3 resolvedPosition)
        {
            resolvedPosition = desiredPosition;
            if (map == null || (limitSearchRadius && searchRadius <= 0f))
            {
                return false;
            }

            WorldPhysicsQuery worldQuery = clearance.Query ?? new WorldPhysicsQuery();
            float searchRadiusSqr = searchRadius * searchRadius;
            Vector2 desired = new(desiredPosition.x, desiredPosition.z);
            float bestDistanceSqr = float.MaxValue;
            int bestIndex = -1;
            Vector3 bestPosition = desiredPosition;

            for (int i = 0; i < map.Samples.Count; i++)
            {
                if (!map.Samples[i].HasMask(GeneratedWorldMask.SpawnSafe))
                {
                    continue;
                }

                Vector3 position = map.PositionForIndex(i);
                Vector2 position2 = new(position.x, position.z);
                float distanceSqr = (position2 - desired).sqrMagnitude;
                if ((limitSearchRadius && distanceSqr > searchRadiusSqr) || distanceSqr >= bestDistanceSqr)
                {
                    continue;
                }

                Vector3 candidate = ResolveCandidatePosition(position, clearance, worldQuery);
                if (!IsCandidateClear(candidate, clearance, worldQuery))
                {
                    continue;
                }

                bestDistanceSqr = distanceSqr;
                bestIndex = i;
                bestPosition = candidate;
            }

            if (bestIndex < 0)
            {
                return false;
            }

            resolvedPosition = bestPosition;
            return true;
        }

        private static Vector3 ResolveCandidatePosition(
            Vector3 position,
            GeneratedWorldSpawnClearance clearance,
            WorldPhysicsQuery worldQuery)
        {
            if (!clearance.Enabled || clearance.EnvironmentMask == 0)
            {
                return position;
            }

            if (!worldQuery.TrySampleHighestSurface(
                    position,
                    clearance.ProbeHeight,
                    clearance.ProbeDepth,
                    clearance.EnvironmentMask,
                    out RaycastHit hit,
                    QueryTriggerInteraction.Ignore))
            {
                return position + Vector3.up * clearance.GroundClearance;
            }

            return hit.point + Vector3.up * clearance.GroundClearance;
        }

        private static bool IsCandidateClear(
            Vector3 rootPosition,
            GeneratedWorldSpawnClearance clearance,
            WorldPhysicsQuery worldQuery)
        {
            if (!clearance.Enabled || clearance.EnvironmentMask == 0)
            {
                return true;
            }

            float radius = Mathf.Max(0.01f, clearance.Radius);
            float height = Mathf.Max(radius * 2f, clearance.Height);
            Vector3 pointA = rootPosition + Vector3.up * radius;
            Vector3 pointB = rootPosition + Vector3.up * (height - radius);
            return worldQuery.IsCapsuleClear(
                pointA,
                pointB,
                radius,
                clearance.EnvironmentMask,
                null,
                QueryTriggerInteraction.Ignore);
        }
    }

    public readonly struct GeneratedWorldSpawnClearance
    {
        public GeneratedWorldSpawnClearance(
            float radius,
            float height,
            float groundClearance,
            float probeHeight,
            float probeDepth,
            int environmentMask,
            WorldPhysicsQuery query = null)
        {
            Radius = Mathf.Max(0f, radius);
            Height = Mathf.Max(0f, height);
            GroundClearance = Mathf.Max(0f, groundClearance);
            ProbeHeight = Mathf.Max(0.01f, probeHeight);
            ProbeDepth = Mathf.Max(0.01f, probeDepth);
            EnvironmentMask = environmentMask;
            Query = query;
        }

        public static GeneratedWorldSpawnClearance None => default;
        public float Radius { get; }
        public float Height { get; }
        public float GroundClearance { get; }
        public float ProbeHeight { get; }
        public float ProbeDepth { get; }
        public int EnvironmentMask { get; }
        public WorldPhysicsQuery Query { get; }
        public bool Enabled => Radius > 0f && Height > 0f && EnvironmentMask != 0;
    }

    public interface IRunWorldGenerator
    {
        WorldGenerationResult Generate(WorldGenerationRequest request);
    }

    public interface IAsyncRunWorldGenerator
    {
        UniTask<WorldGenerationResult> GenerateAsync(WorldGenerationRequest request, WorldLoadTimingDiagnostics timing = null);
    }

    public sealed class PrototypeRunWorldGenerator : IRunWorldGenerator, IAsyncRunWorldGenerator
    {
        public WorldGenerationResult Generate(WorldGenerationRequest request)
        {
            return new WorldGenerationResult(
                request.WorldIndex,
                request.ActNumber,
                request.Seed,
                usedPrototypePlacement: true);
        }

        public UniTask<WorldGenerationResult> GenerateAsync(WorldGenerationRequest request, WorldLoadTimingDiagnostics timing = null)
        {
            return UniTask.FromResult(Generate(request));
        }
    }

    public sealed class RunWorldGenerationState
    {
        public WorldGenerationResult Current { get; private set; }
        public bool HasGeneratedLayout => Current.HasGeneratedLayout;
        public bool HasGeneratedMap => Current.HasGeneratedMap;
        public GeneratedWorldMap GeneratedMap => Current.GeneratedMap;
        public IReadOnlyList<WorldRewardSpawnRequest> RewardSpawnRequests => Current.RewardSpawnRequests;
        public IReadOnlyList<WorldBlockingObstacleSpawnRequest> ObstacleSpawnRequests => Current.ObstacleSpawnRequests;

        public void Set(WorldGenerationResult result)
        {
            Current = result;
        }

        public bool TryGetPlayerStart(out Vector3 position)
        {
            position = Current.PlayerStart;
            return Current.HasPlayerStart;
        }

        public bool TryGetHeadlinerAnchor(out Vector3 position)
        {
            position = Current.HeadlinerAnchor;
            return Current.HasHeadlinerAnchor;
        }

        public bool TryGetStageDoorAnchor(out Vector3 position)
        {
            position = Current.StageDoorAnchor;
            return Current.HasStageDoorAnchor;
        }

        public void Clear()
        {
            GameObject root = Current.GeneratedRoot;
            if (root != null)
            {
                WorldRewardRuntimeObjectFactory.ReleaseRoot(root);
            }

            PrototypeArenaVisibility.SetVisible(true);
            Current = default;
        }
    }

    public sealed class RunWorldLayoutState
    {
        private static bool hasSessionMode;
        private static RunWorldLayoutMode sessionMode = RunWorldLayoutMode.Generated;

        public RunWorldLayoutState(RunWorldLayoutMode defaultMode = RunWorldLayoutMode.Generated)
        {
            Mode = hasSessionMode ? sessionMode : Sanitized(defaultMode);
        }

        public RunWorldLayoutMode Mode { get; private set; }
        public bool UseGeneratedTerrain => Mode == RunWorldLayoutMode.Generated;

        public event Action<RunWorldLayoutMode> Changed;

        public bool SetMode(RunWorldLayoutMode mode)
        {
            mode = Sanitized(mode);
            if (Mode == mode)
            {
                return false;
            }

            Mode = mode;
            sessionMode = mode;
            hasSessionMode = true;
            Changed?.Invoke(mode);
            return true;
        }

        public RunWorldLayoutMode Toggle()
        {
            RunWorldLayoutMode next = Mode == RunWorldLayoutMode.Generated
                ? RunWorldLayoutMode.Prototype
                : RunWorldLayoutMode.Generated;
            _ = SetMode(next);
            return Mode;
        }

        public static void ClearSessionOverrideForTests()
        {
            hasSessionMode = false;
            sessionMode = RunWorldLayoutMode.Generated;
        }

        private static RunWorldLayoutMode Sanitized(RunWorldLayoutMode mode)
        {
            return Enum.IsDefined(typeof(RunWorldLayoutMode), mode)
                ? mode
                : RunWorldLayoutMode.Generated;
        }
    }

    public sealed class ProceduralRunWorldGenerator : IRunWorldGenerator, IAsyncRunWorldGenerator
    {
        private const float PlayerStartGroundClearance = 0.05f;
        private const float PlayerStartClearanceRadius = 0.62f;
        private const float PlayerStartClearanceHeight = 2f;

        private readonly RunWorldGenerationState state;
        private readonly RunWorldGenerationConfig config;
        private readonly GameConfig gameConfig;
        private readonly RunWorldLayoutState layoutState;
        private readonly WorldPropPlacementService placementService;
        private readonly WorldPhysicsQuery spawnClearanceQuery = new();
        private IReadOnlyList<BlockyCircusGroundsPiece> generatedBlockyPieces = Array.Empty<BlockyCircusGroundsPiece>();

        private readonly struct MapGenerationSnapshot
        {
            public MapGenerationSnapshot(
                RunWorldGenerationProfile profile,
                int baseSeed,
                int retryCount,
                bool fallbackToFlatWorld,
                RunWorldTerrainMode terrainMode,
                float minimumReachablePercent,
                float rewardSafeMaxSlope,
                float spawnSafeMaxSlope,
                bool centerBiasEnabled,
                float playerStartCenterWeight,
                float playerSlopeLimit,
                float playerStepOffset)
            {
                Profile = profile;
                BaseSeed = baseSeed;
                RetryCount = Mathf.Max(0, retryCount);
                FallbackToFlatWorld = fallbackToFlatWorld;
                TerrainMode = terrainMode;
                MinimumReachablePercent = Mathf.Clamp(minimumReachablePercent, 1f, 100f);
                RewardSafeMaxSlope = Mathf.Clamp(rewardSafeMaxSlope, 0f, 89f);
                SpawnSafeMaxSlope = Mathf.Clamp(spawnSafeMaxSlope, 0f, 89f);
                CenterBiasEnabled = centerBiasEnabled;
                PlayerStartCenterWeight = Mathf.Max(0f, playerStartCenterWeight);
                PlayerSlopeLimit = Mathf.Clamp(playerSlopeLimit, 0f, 89f);
                PlayerStepOffset = Mathf.Max(0f, playerStepOffset);
            }

            public RunWorldGenerationProfile Profile { get; }
            public int BaseSeed { get; }
            public int RetryCount { get; }
            public bool FallbackToFlatWorld { get; }
            public RunWorldTerrainMode TerrainMode { get; }
            public float MinimumReachablePercent { get; }
            public float RewardSafeMaxSlope { get; }
            public float SpawnSafeMaxSlope { get; }
            public bool CenterBiasEnabled { get; }
            public float PlayerStartCenterWeight { get; }
            public float PlayerSlopeLimit { get; }
            public float PlayerStepOffset { get; }
        }

        public ProceduralRunWorldGenerator(
            RunWorldGenerationState state = null,
            RunWorldGenerationConfig config = null,
            GameConfig gameConfig = null,
            RunWorldLayoutState layoutState = null,
            WorldPropPlacementService placementService = null)
        {
            this.state = state;
            this.config = config != null ? config : RunWorldGenerationConfig.CreateRuntimeDefault();
            this.gameConfig = gameConfig;
            this.layoutState = layoutState;
            this.placementService = placementService;
            this.config.EnsureWorkflowDefaults();
        }

        private MapGenerationSnapshot SnapshotMapGenerationSettings(WorldGenerationRequest request)
        {
            RunWorldGenerationProfile profile = CopyProfile(config.ProfileForAct(request.ActNumber));
            int baseSeed = config.useDebugSeedOverride
                ? config.debugSeedOverride
                : request.Seed;
            float playerSlopeLimit = gameConfig != null ? gameConfig.playerControllerSlopeLimit : 45f;
            float playerStepOffset = gameConfig != null ? gameConfig.playerControllerStepOffset : 0.4f;
            return new MapGenerationSnapshot(
                profile,
                baseSeed,
                config.retryCount,
                config.fallbackToFlatWorld,
                config.terrainMode,
                config.minimumReachablePercent,
                config.rewardSafeMaxSlope,
                config.spawnSafeMaxSlope,
                config.centerBiasEnabled,
                config.playerStartCenterWeight,
                playerSlopeLimit,
                playerStepOffset);
        }

        private static RunWorldGenerationProfile CopyProfile(RunWorldGenerationProfile source)
        {
            source ??= new RunWorldGenerationProfile();
            return new RunWorldGenerationProfile
            {
                actLabel = source.actLabel,
                actNumber = source.actNumber,
                playableRadius = source.playableRadius,
                maxHeight = source.maxHeight,
                gridSpacing = source.gridSpacing,
                boundaryMargin = source.boundaryMargin,
                landformStampCount = source.landformStampCount,
                angularity = source.angularity,
                terraceSteps = source.terraceSteps,
                plateauCount = source.plateauCount,
                plateauRadiusMinPercent = source.plateauRadiusMinPercent,
                plateauRadiusMaxPercent = source.plateauRadiusMaxPercent,
                rampWidthPercent = source.rampWidthPercent,
                rampBlendPercent = source.rampBlendPercent,
                rampCount = source.rampCount,
                rampWidthMinPercent = source.rampWidthMinPercent,
                rampWidthMaxPercent = source.rampWidthMaxPercent,
                rampLengthMinPercent = source.rampLengthMinPercent,
                rampLengthMaxPercent = source.rampLengthMaxPercent,
                wallHeightThreshold = source.wallHeightThreshold,
                allowRampOverlap = source.allowRampOverlap,
                surfaceVariationStrength = source.surfaceVariationStrength,
                generatedChestCount = source.generatedChestCount,
                generatedTicketDepositCount = source.generatedTicketDepositCount,
                generatedHealingPropCount = source.generatedHealingPropCount,
                generatedLooseChestCount = source.generatedLooseChestCount,
                generatedLooseTicketDepositCount = source.generatedLooseTicketDepositCount,
                generatedLooseHealingPropCount = source.generatedLooseHealingPropCount
            };
        }

        public WorldGenerationResult Generate(WorldGenerationRequest request)
        {
            GeneratedWorldMap map = GenerateMap(request);
            if (!ShouldBuildGeneratedTerrain() || map == null)
            {
                return SetAndReturn(PrototypeResult(request, map));
            }

            GeneratedWorldSurfaceMaterialSet surfaceMaterials =
                GeneratedWorldSurfaceTextureGenerator.GenerateMaterialSet(
                    config.terrainMaterial,
                    config,
                    request.Seed,
                    request.ActNumber);
            GeneratedWorldTerrainBuildResult terrain;
            try
            {
                terrain = GeneratedWorldTerrainBuilder.Build(
                    map,
                    new GeneratedWorldTerrainBuildSettings(
                        surfaceMaterials,
                        config.ChunkQuadsPerSide));
            }
            catch (Exception exception)
            {
                surfaceMaterials.Dispose();
                Debug.LogWarning($"Generated world terrain build failed. Falling back to prototype arena. {exception.Message}");
                return SetAndReturn(PrototypeResult(request, map));
            }

            if (!terrain.Success || terrain.Root == null)
            {
                surfaceMaterials.Dispose();
                return SetAndReturn(PrototypeResult(request, map));
            }

            surfaceMaterials.AttachCleanup(terrain.Root);
            if (generatedBlockyPieces.Count > 0)
            {
                GeneratedWorldTerrainBuilder.BuildConstructedRampMeshes(
                    terrain.Root.transform,
                    map,
                    generatedBlockyPieces,
                    surfaceMaterials);
                GeneratedWorldTerrainBuilder.BuildSmoothBoundaryRim(
                    terrain.Root.transform,
                    map,
                    surfaceMaterials.RimMaterial,
                    Mathf.Max(4f, map.GridSpacing * 2.25f),
                    160);
            }

            IReadOnlyList<WorldBlockingObstacleSpawnRequest> obstacleRequests =
                GeneratedWorldObstaclePlanner.BuildRequests(map, config);
            WorldBlockingObstacleFactory.Build(
                terrain.Root.transform,
                obstacleRequests,
                config,
                placementService);

            Physics.SyncTransforms();
            Vector3 playerStart = ResolveGeneratedPlayerStart(map);

            if (config.hidePrototypeArenaWhenGenerated)
            {
                PrototypeArenaVisibility.SetVisible(false);
            }

            IReadOnlyList<WorldRewardSpawnRequest> rewardRequests =
                GeneratedWorldRewardPlanner.BuildRequests(
                    map,
                    config.ProfileForAct(request.ActNumber),
                    config,
                    obstacleRequests);

            var result = new WorldGenerationResult(
                request.WorldIndex,
                request.ActNumber,
                request.Seed,
                usedPrototypePlacement: false,
                generatedRoot: terrain.Root,
                rewardSpawnRequests: rewardRequests,
                enemySpawnBands: null,
                playerStart: playerStart,
                hasPlayerStart: true,
                headlinerAnchor: Vector3.zero,
                hasHeadlinerAnchor: false,
                stageDoorAnchor: Vector3.zero,
                hasStageDoorAnchor: false,
                bounds: terrain.Bounds,
                generatedMap: map,
                obstacleSpawnRequests: obstacleRequests);
            return SetAndReturn(result);
        }

        public async UniTask<WorldGenerationResult> GenerateAsync(WorldGenerationRequest request, WorldLoadTimingDiagnostics timing = null)
        {
            timing ??= WorldLoadTimingDiagnostics.Disabled;

            MapGenerationSnapshot snapshot;
            using (timing.Stage("Generator.Snapshot"))
            {
                snapshot = SnapshotMapGenerationSettings(request);
            }

            await UniTask.Yield(PlayerLoopTiming.Update);

            GeneratedWorldMap map;
            using (timing.Stage("Generator.MapThreadPool"))
            {
                map = await UniTask.RunOnThreadPool(() => GenerateMap(request, snapshot));
                await UniTask.SwitchToMainThread();
            }

            await UniTask.Yield(PlayerLoopTiming.Update);

            if (!ShouldBuildGeneratedTerrain() || map == null)
            {
                return SetAndReturn(PrototypeResult(request, map));
            }

            GeneratedWorldSurfaceMaterialSet surfaceMaterials;
            using (timing.Stage("Generator.SurfaceTextures"))
            {
                surfaceMaterials = await GeneratedWorldSurfaceTextureGenerator.GenerateMaterialSetAsync(
                    config.terrainMaterial,
                    config,
                    request.Seed,
                    request.ActNumber,
                    timing);
            }

            await UniTask.Yield(PlayerLoopTiming.Update);

            GeneratedWorldTerrainBuildResult terrain;
            try
            {
                using (timing.Stage("Generator.TerrainBuild"))
                {
                    terrain = await GeneratedWorldTerrainBuilder.BuildAsync(
                        map,
                        new GeneratedWorldTerrainBuildSettings(
                            surfaceMaterials,
                            config.ChunkQuadsPerSide),
                        config.TerrainBuildChunksPerFrame,
                        timing);
                }
            }
            catch (Exception exception)
            {
                surfaceMaterials.Dispose();
                Debug.LogWarning($"Generated world terrain build failed. Falling back to prototype arena. {exception.Message}");
                return SetAndReturn(PrototypeResult(request, map));
            }

            if (!terrain.Success || terrain.Root == null)
            {
                surfaceMaterials.Dispose();
                return SetAndReturn(PrototypeResult(request, map));
            }

            surfaceMaterials.AttachCleanup(terrain.Root);
            if (generatedBlockyPieces.Count > 0)
            {
                using (timing.Stage("Generator.ConstructedRamps"))
                {
                    await GeneratedWorldTerrainBuilder.BuildConstructedRampMeshesAsync(
                        terrain.Root.transform,
                        map,
                        generatedBlockyPieces,
                        surfaceMaterials,
                        rampsPerFrame: config.TerrainBuildChunksPerFrame,
                        timing: timing);
                }

                await UniTask.Yield(PlayerLoopTiming.Update);

                using (timing.Stage("Generator.BoundaryRim"))
                {
                    GeneratedWorldTerrainBuilder.BuildSmoothBoundaryRim(
                        terrain.Root.transform,
                        map,
                        surfaceMaterials.RimMaterial,
                        Mathf.Max(4f, map.GridSpacing * 2.25f),
                        160);
                }

                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            IReadOnlyList<WorldBlockingObstacleSpawnRequest> obstacleRequests;
            using (timing.Stage("Generator.BlockingObstacles"))
            {
                obstacleRequests = GeneratedWorldObstaclePlanner.BuildRequests(map, config);
                await WorldBlockingObstacleFactory.BuildAsync(
                    terrain.Root.transform,
                    obstacleRequests,
                    config,
                    placementService,
                    timing);
            }

            await UniTask.Yield(PlayerLoopTiming.Update);

            using (timing.Stage("Generator.PhysicsSyncTransforms"))
            {
                Physics.SyncTransforms();
            }

            await UniTask.Yield(PlayerLoopTiming.Update);

            Vector3 playerStart;
            using (timing.Stage("Generator.ResolvePlayerStart"))
            {
                playerStart = ResolveGeneratedPlayerStart(map);
            }

            if (config.hidePrototypeArenaWhenGenerated)
            {
                using (timing.Stage("Generator.HidePrototypeArena"))
                {
                    PrototypeArenaVisibility.SetVisible(false);
                }
            }

            IReadOnlyList<WorldRewardSpawnRequest> rewardRequests;
            using (timing.Stage("Generator.RewardPlanning"))
            {
                rewardRequests = await GeneratedWorldRewardPlanner.BuildRequestsAsync(
                    map,
                    config.ProfileForAct(request.ActNumber),
                    config,
                    obstacleRequests);
            }

            await UniTask.Yield(PlayerLoopTiming.Update);

            var result = new WorldGenerationResult(
                request.WorldIndex,
                request.ActNumber,
                request.Seed,
                usedPrototypePlacement: false,
                generatedRoot: terrain.Root,
                rewardSpawnRequests: rewardRequests,
                enemySpawnBands: null,
                playerStart: playerStart,
                hasPlayerStart: true,
                headlinerAnchor: Vector3.zero,
                hasHeadlinerAnchor: false,
                stageDoorAnchor: Vector3.zero,
                hasStageDoorAnchor: false,
                bounds: terrain.Bounds,
                generatedMap: map,
                obstacleSpawnRequests: obstacleRequests);
            return SetAndReturn(result);
        }

        private Vector3 ResolveGeneratedPlayerStart(GeneratedWorldMap map)
        {
            if (map == null)
            {
                return Vector3.up * PlayerStartGroundClearance;
            }

            var clearance = new GeneratedWorldSpawnClearance(
                PlayerStartClearanceRadius,
                PlayerStartClearanceHeight,
                PlayerStartGroundClearance,
                Mathf.Max(12f, map.MaxHeight + map.GridSpacing * 2f),
                Mathf.Max(24f, map.MaxHeight + map.GridSpacing * 4f),
                GameLayers.EnvironmentMaskExcludingGameplay,
                spawnClearanceQuery);
            float searchRadius = Mathf.Max(32f, map.GridSpacing * 18f);
            return GeneratedWorldSpawnPointResolver.TryResolve(
                map,
                map.PlayerStartPosition,
                searchRadius,
                clearance,
                out Vector3 resolvedPosition)
                ? resolvedPosition
                : map.PlayerStartPosition + Vector3.up * PlayerStartGroundClearance;
        }

        private bool ShouldBuildGeneratedTerrain()
        {
            bool requestedGenerated = layoutState == null
                ? config.defaultLayoutMode == RunWorldLayoutMode.Generated
                : layoutState.UseGeneratedTerrain;
            return requestedGenerated && config.runtimeMeshTerrainEnabled;
        }

        private WorldGenerationResult SetAndReturn(WorldGenerationResult result)
        {
            state?.Set(result);
            return result;
        }

        private static WorldGenerationResult PrototypeResult(WorldGenerationRequest request, GeneratedWorldMap map)
        {
            PrototypeArenaVisibility.SetVisible(true);
            return new WorldGenerationResult(
                request.WorldIndex,
                request.ActNumber,
                request.Seed,
                usedPrototypePlacement: true,
                generatedRoot: null,
                rewardSpawnRequests: null,
                enemySpawnBands: null,
                playerStart: Vector3.zero,
                hasPlayerStart: false,
                headlinerAnchor: Vector3.zero,
                hasHeadlinerAnchor: false,
                stageDoorAnchor: Vector3.zero,
                hasStageDoorAnchor: false,
                bounds: map != null ? map.Bounds : default,
                generatedMap: map);
        }

        private GeneratedWorldMap GenerateMap(WorldGenerationRequest request)
        {
            return GenerateMap(request, SnapshotMapGenerationSettings(request));
        }

        private GeneratedWorldMap GenerateMap(WorldGenerationRequest request, MapGenerationSnapshot snapshot)
        {
            generatedBlockyPieces = Array.Empty<BlockyCircusGroundsPiece>();
            RunWorldGenerationProfile profile = snapshot.Profile;
            int baseSeed = snapshot.BaseSeed;
            int retries = snapshot.RetryCount;
            var warnings = new List<string>();

            for (int attempt = 0; attempt <= retries; attempt++)
            {
                int attemptSeed = DeterministicSeed.Combine(baseSeed, request.WorldIndex, request.ActNumber, attempt, 9137);
                GeneratedWorldMap map = BuildMap(request.ActNumber, baseSeed, attemptSeed, profile, usedFallback: false, warnings, snapshot);
                if (IsValid(map, snapshot))
                {
                    return map;
                }

                warnings.Add($"Attempt {attempt + 1} failed validation for Act {request.ActNumber}.");
            }

            if (snapshot.FallbackToFlatWorld)
            {
                warnings.Add("Generated world fell back to a flat safe map.");
                return BuildFlatFallback(request.ActNumber, baseSeed, profile, warnings, snapshot);
            }

            return BuildMap(request.ActNumber, baseSeed, DeterministicSeed.Combine(baseSeed, 991), profile, usedFallback: false, warnings, snapshot);
        }

        private GeneratedWorldMap BuildMap(
            int actNumber,
            int seed,
            int attemptSeed,
            RunWorldGenerationProfile profile,
            bool usedFallback,
            IReadOnlyList<string> warnings,
            MapGenerationSnapshot snapshot)
        {
            int width = SampleCount(profile.PlayableRadius, profile.GridSpacing);
            int depth = width;
            var heights = new float[width * depth];
            var rampSamples = new bool[width * depth];
            var rampDirections = new Vector2[width * depth];
            bool useStructuralGrammar = !usedFallback && snapshot.TerrainMode == RunWorldTerrainMode.StructuralGrammar;
            if (useStructuralGrammar)
            {
                return BuildBlockyCircusMap(actNumber, seed, attemptSeed, profile, warnings, snapshot);
            }

            generatedBlockyPieces = Array.Empty<BlockyCircusGroundsPiece>();
            LandformStamp[] stamps = useStructuralGrammar ? Array.Empty<LandformStamp>() : CreateLandforms(profile, attemptSeed);
            float slopeLimit = snapshot.PlayerSlopeLimit;
            StructuralTerrainGrammar structuralGrammar = useStructuralGrammar
                ? CreateStructuralTerrain(profile, attemptSeed, slopeLimit)
                : default;

            float minHeight = float.MaxValue;
            float maxHeight = float.MinValue;
            for (int z = 0; z < depth; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vector2 position = SamplePosition2D(x, z, width, depth, profile.GridSpacing);
                    float height;
                    bool isRamp = false;
                    if (usedFallback)
                    {
                        height = 0f;
                    }
                    else if (useStructuralGrammar)
                    {
                        height = EvaluateStructuralHeight(position, profile, structuralGrammar, attemptSeed, out isRamp, out Vector2 rampDirection);
                        rampDirections[z * width + x] = rampDirection;
                    }
                    else
                    {
                        height = EvaluateSmoothStampHeight(position, profile, stamps, attemptSeed);
                    }

                    rampSamples[z * width + x] = useStructuralGrammar && !usedFallback && isRamp;
                    heights[z * width + x] = height;
                    minHeight = Mathf.Min(minHeight, height);
                    maxHeight = Mathf.Max(maxHeight, height);
                }
            }

            if (useStructuralGrammar)
            {
                ApplyStructuralHeightFinish(heights, rampSamples, profile);
            }
            else
            {
                NormalizeHeights(heights, minHeight, maxHeight, profile);
            }

            var samples = new GeneratedWorldSample[heights.Length];
            int initialStartIndex = SelectInitialStart(heights, width, depth, profile, snapshot);
            bool[] reachable = FloodReachable(heights, width, depth, profile, initialStartIndex, snapshot);
            int playerStartIndex = SelectPlayerStart(heights, reachable, width, depth, profile, snapshot);

            float rewardSlopeLimit = Mathf.Min(slopeLimit, snapshot.RewardSafeMaxSlope);
            float spawnSlopeLimit = Mathf.Min(slopeLimit, snapshot.SpawnSafeMaxSlope);
            for (int z = 0; z < depth; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = z * width + x;
                    Vector2 position = SamplePosition2D(x, z, width, depth, profile.GridSpacing);
                    float distance = position.magnitude;
                    bool insidePlayable = distance <= profile.PlayableRadius;
                    float boundaryDistance = Mathf.Max(0f, profile.PlayableRadius - distance);
                    Vector3 normal = NormalAt(heights, width, depth, x, z, profile.GridSpacing);
                    float slope = Vector3.Angle(normal, Vector3.up);
                    bool walkable = insidePlayable && slope <= slopeLimit;
                    bool isReachable = walkable && reachable[index];
                    bool boundarySafe = boundaryDistance >= profile.BoundaryMargin;

                    GeneratedWorldMask mask = GeneratedWorldMask.None;
                    if (walkable)
                    {
                        mask |= GeneratedWorldMask.Walkable;
                    }

                    if (isReachable)
                    {
                        mask |= GeneratedWorldMask.Reachable;
                    }

                    if (isReachable && boundarySafe && slope <= rewardSlopeLimit)
                    {
                        mask |= GeneratedWorldMask.RewardSafe;
                    }

                    if (isReachable && boundarySafe && slope <= spawnSlopeLimit)
                    {
                        mask |= GeneratedWorldMask.SpawnSafe;
                    }

                    if (isReachable && boundarySafe && slope <= rewardSlopeLimit && CenterScore(position, profile) > 0.45f)
                    {
                        mask |= GeneratedWorldMask.PlayerStartSafe;
                    }

                    samples[index] = new GeneratedWorldSample(
                        heights[index],
                        normal,
                        slope,
                        mask,
                        profile.MaxHeight > 0f ? heights[index] / profile.MaxHeight : 0f,
                        CenterScore(position, profile),
                        Mathf.Clamp01(boundaryDistance / Mathf.Max(1f, profile.PlayableRadius)),
                        rampSamples[index] ? GeneratedWorldSurfaceKind.Ramp : GeneratedWorldSurfaceKind.Flat,
                        rampDirections[index]);
                }
            }

            return new GeneratedWorldMap(
                actNumber,
                seed,
                attemptSeed,
                width,
                depth,
                profile.PlayableRadius,
                profile.GridSpacing,
                profile.MaxHeight,
                profile.WallHeightThreshold,
                samples,
                playerStartIndex,
                usedFallback,
                warnings);
        }

        private GeneratedWorldMap BuildBlockyCircusMap(
            int actNumber,
            int seed,
            int attemptSeed,
            RunWorldGenerationProfile profile,
            IReadOnlyList<string> warnings,
            MapGenerationSnapshot snapshot)
        {
            float slopeLimit = Mathf.Clamp(snapshot.PlayerSlopeLimit, 1f, 89f);
            var settings = new BlockyCircusGroundsSandboxSettings(
                attemptSeed,
                SampleCount(profile.PlayableRadius, profile.GridSpacing),
                SampleCount(profile.PlayableRadius, profile.GridSpacing),
                profile.GridSpacing,
                profile.MaxHeight > 0f ? profile.MaxHeight : 1f,
                Mathf.Max(0.5f, profile.GridSpacing),
                profile.WallHeightThreshold,
                slopeLimit,
                Mathf.Min(15f, slopeLimit));
            BlockyCircusGroundsSandboxResult sandbox = BlockyCircusGroundsGrammar.Generate(settings);
            generatedBlockyPieces = sandbox.Pieces;
            GeneratedWorldSample[] samples = CopySamples(sandbox.Map, profile);
            return new GeneratedWorldMap(
                actNumber,
                seed,
                attemptSeed,
                sandbox.Map.Width,
                sandbox.Map.Depth,
                sandbox.Map.PlayableRadius,
                sandbox.Map.GridSpacing,
                sandbox.Map.MaxHeight,
                sandbox.Map.WallHeightThreshold,
                samples,
                sandbox.Map.PlayerStartIndex,
                usedFallback: false,
                warnings,
                hasStructuralValidation: true,
                structuralValidation: sandbox.Validation);
        }

        private static GeneratedWorldSample[] CopySamples(GeneratedWorldMap map, RunWorldGenerationProfile profile)
        {
            IReadOnlyList<GeneratedWorldSample> source = map != null ? map.Samples : null;
            if (source == null || source.Count == 0)
            {
                return Array.Empty<GeneratedWorldSample>();
            }

            var copy = new GeneratedWorldSample[source.Count];
            for (int i = 0; i < source.Count; i++)
            {
                GeneratedWorldSample sample = source[i];
                GeneratedWorldMask mask = sample.Mask;
                if (!IsProfileBoundarySafe(map, profile, i))
                {
                    mask &= ~(GeneratedWorldMask.RewardSafe
                        | GeneratedWorldMask.SpawnSafe
                        | GeneratedWorldMask.PlayerStartSafe);
                }

                copy[i] = sample.WithMask(mask);
            }

            return copy;
        }

        private static bool IsProfileBoundarySafe(GeneratedWorldMap map, RunWorldGenerationProfile profile, int sampleIndex)
        {
            if (map == null || profile == null)
            {
                return true;
            }

            Vector3 position = map.PositionForIndex(sampleIndex);
            float boundaryDistance = map.PlayableRadius - new Vector2(position.x, position.z).magnitude;
            return boundaryDistance >= profile.BoundaryMargin;
        }

        private GeneratedWorldMap BuildFlatFallback(
            int actNumber,
            int seed,
            RunWorldGenerationProfile profile,
            IReadOnlyList<string> warnings,
            MapGenerationSnapshot snapshot)
        {
            return BuildMap(
                actNumber,
                seed,
                DeterministicSeed.Combine(seed, actNumber, 31337),
                profile,
                usedFallback: true,
                warnings,
                snapshot);
        }

        private static bool IsValid(GeneratedWorldMap map, MapGenerationSnapshot snapshot)
        {
            if (map == null || map.Samples.Count == 0)
            {
                return false;
            }

            int walkable = map.CountMask(GeneratedWorldMask.Walkable);
            int reachable = map.CountMask(GeneratedWorldMask.Reachable);
            int playerStarts = map.CountMask(GeneratedWorldMask.PlayerStartSafe);
            if (walkable <= 0 || reachable <= 0 || playerStarts <= 0)
            {
                return false;
            }

            float reachablePercent = reachable / (float)Mathf.Max(1, walkable) * 100f;
            return reachablePercent >= snapshot.MinimumReachablePercent;
        }

        private int SelectInitialStart(float[] heights, int width, int depth, RunWorldGenerationProfile profile, MapGenerationSnapshot snapshot)
        {
            float bestScore = float.MinValue;
            int bestIndex = (depth / 2) * width + width / 2;
            float slopeLimit = snapshot.PlayerSlopeLimit;
            for (int z = 0; z < depth; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = z * width + x;
                    Vector2 position = SamplePosition2D(x, z, width, depth, profile.GridSpacing);
                    if (position.magnitude > profile.PlayableRadius - profile.BoundaryMargin)
                    {
                        continue;
                    }

                    float slope = Vector3.Angle(NormalAt(heights, width, depth, x, z, profile.GridSpacing), Vector3.up);
                    if (slope > slopeLimit)
                    {
                        continue;
                    }

                    float score = snapshot.CenterBiasEnabled
                        ? CenterScore(position, profile) * snapshot.PlayerStartCenterWeight
                        : DeterministicSeed.ToFloat01(DeterministicSeed.Combine(index, 71));
                    score -= slope / Mathf.Max(1f, slopeLimit) * 0.1f;
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestIndex = index;
                    }
                }
            }

            return bestIndex;
        }

        private int SelectPlayerStart(float[] heights, bool[] reachable, int width, int depth, RunWorldGenerationProfile profile, MapGenerationSnapshot snapshot)
        {
            float bestScore = float.MinValue;
            int bestIndex = SelectInitialStart(heights, width, depth, profile, snapshot);
            float rewardSlopeLimit = Mathf.Min(snapshot.PlayerSlopeLimit, snapshot.RewardSafeMaxSlope);

            for (int z = 0; z < depth; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = z * width + x;
                    if (!reachable[index])
                    {
                        continue;
                    }

                    Vector2 position = SamplePosition2D(x, z, width, depth, profile.GridSpacing);
                    float boundaryDistance = profile.PlayableRadius - position.magnitude;
                    if (boundaryDistance < profile.BoundaryMargin)
                    {
                        continue;
                    }

                    float slope = Vector3.Angle(NormalAt(heights, width, depth, x, z, profile.GridSpacing), Vector3.up);
                    if (slope > rewardSlopeLimit)
                    {
                        continue;
                    }

                    float score = snapshot.CenterBiasEnabled
                        ? CenterScore(position, profile) * snapshot.PlayerStartCenterWeight
                        : DeterministicSeed.ToFloat01(DeterministicSeed.Combine(index, 703));
                    score += Mathf.Clamp01(boundaryDistance / Mathf.Max(1f, profile.PlayableRadius)) * 0.15f;
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestIndex = index;
                    }
                }
            }

            return bestIndex;
        }

        private static bool[] FloodReachable(float[] heights, int width, int depth, RunWorldGenerationProfile profile, int startIndex, MapGenerationSnapshot snapshot)
        {
            var reachable = new bool[heights.Length];
            if (startIndex < 0 || startIndex >= heights.Length)
            {
                return reachable;
            }

            float slopeLimit = snapshot.PlayerSlopeLimit;
            float stepOffset = snapshot.PlayerStepOffset;
            float maxDelta = Mathf.Tan(slopeLimit * Mathf.Deg2Rad) * profile.GridSpacing + stepOffset;
            var queue = new Queue<int>();
            reachable[startIndex] = true;
            queue.Enqueue(startIndex);

            while (queue.Count > 0)
            {
                int index = queue.Dequeue();
                int x = index % width;
                int z = index / width;
                TryVisit(x + 1, z);
                TryVisit(x - 1, z);
                TryVisit(x, z + 1);
                TryVisit(x, z - 1);

                void TryVisit(int nx, int nz)
                {
                    if (nx < 0 || nz < 0 || nx >= width || nz >= depth)
                    {
                        return;
                    }

                    int neighbor = nz * width + nx;
                    if (reachable[neighbor])
                    {
                        return;
                    }

                    Vector2 position = SamplePosition2D(nx, nz, width, depth, profile.GridSpacing);
                    if (position.magnitude > profile.PlayableRadius)
                    {
                        return;
                    }

                    float slope = Vector3.Angle(NormalAt(heights, width, depth, nx, nz, profile.GridSpacing), Vector3.up);
                    if (slope > slopeLimit)
                    {
                        return;
                    }

                    if (Mathf.Abs(heights[neighbor] - heights[index]) > maxDelta)
                    {
                        return;
                    }

                    reachable[neighbor] = true;
                    queue.Enqueue(neighbor);
                }
            }

            return reachable;
        }

        private static int SampleCount(float radius, float spacing)
        {
            int count = Mathf.RoundToInt(radius * 2f / Mathf.Max(1f, spacing)) + 1;
            return count % 2 == 0 ? count + 1 : count;
        }

        private static Vector2 SamplePosition2D(int x, int z, int width, int depth, float spacing)
        {
            float halfX = (width - 1) * spacing * 0.5f;
            float halfZ = (depth - 1) * spacing * 0.5f;
            return new Vector2(x * spacing - halfX, z * spacing - halfZ);
        }

        private static float EvaluateSmoothStampHeight(
            Vector2 position,
            RunWorldGenerationProfile profile,
            IReadOnlyList<LandformStamp> stamps,
            int seed)
        {
            float height = 0f;
            for (int i = 0; i < stamps.Count; i++)
            {
                height += stamps[i].Evaluate(position);
            }

            float broadWave = Mathf.Sin((position.x + DeterministicSeed.ToSignedFloat(seed) * 97f) / Mathf.Max(1f, profile.PlayableRadius) * Mathf.PI * 2f)
                * Mathf.Cos((position.y + DeterministicSeed.ToSignedFloat(seed + 17) * 89f) / Mathf.Max(1f, profile.PlayableRadius) * Mathf.PI * 2f);
            height += broadWave * profile.MaxHeight * 0.08f;
            return height;
        }

        private static StructuralTerrainGrammar CreateStructuralTerrain(RunWorldGenerationProfile profile, int seed, float slopeLimitDegrees)
        {
            int plateauCount = Mathf.Max(1, profile.PlateauCount);
            var plateaus = new StructuralPlateau[plateauCount];
            float minRadius = profile.PlayableRadius * profile.PlateauRadiusMinPercent;
            float maxRadius = profile.PlayableRadius * profile.PlateauRadiusMaxPercent;
            float minRampWidth = profile.PlayableRadius * profile.RampWidthMinPercent;
            float maxRampWidth = profile.PlayableRadius * profile.RampWidthMaxPercent;
            float minRampLength = profile.PlayableRadius * profile.RampLengthMinPercent;
            float maxRampLength = profile.PlayableRadius * profile.RampLengthMaxPercent;
            float edgeBlend = profile.PlayableRadius * profile.RampBlendPercent;
            float rampSideBlend = 0f;
            float plateauBlend = Mathf.Max(profile.GridSpacing * 0.75f, edgeBlend);
            float baseAngle = QuantizedAngle(DeterministicSeed.ToAngleRadians(seed));

            for (int i = 0; i < plateauCount; i++)
            {
                int localSeed = DeterministicSeed.Combine(seed, i, 23077);
                Vector2 center;
                float height;
                if (i == 0)
                {
                    center = Vector2.zero;
                    height = 0f;
                }
                else
                {
                    float angleStep = Mathf.PI * 2f / Mathf.Max(1, plateauCount - 1);
                    float angleJitter = Mathf.Lerp(-0.22f, 0.22f, DeterministicSeed.ToFloat01(localSeed + 12));
                    float angle = QuantizedAngle(baseAngle + (i - 1) * angleStep + angleJitter);
                    float radial = Mathf.Lerp(
                        profile.PlayableRadius * 0.26f,
                        profile.PlayableRadius * 0.76f,
                        DeterministicSeed.ToFloat01(localSeed + 1));
                    center = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radial;
                    int tierCount = Mathf.Max(3, profile.TerraceSteps);
                    int tierIndex = 1 + Mathf.Abs(DeterministicSeed.Combine(localSeed, 2)) % Mathf.Max(1, tierCount - 1);
                    height = profile.MaxHeight * (tierIndex / (float)tierCount);
                }

                float baseExtent = Mathf.Lerp(minRadius, maxRadius, DeterministicSeed.ToFloat01(localSeed + 3));
                float aspect = Mathf.Lerp(0.58f, 1.55f, DeterministicSeed.ToFloat01(localSeed + 4));
                float halfX = Mathf.Max(profile.GridSpacing * 2f, baseExtent * aspect);
                float halfZ = Mathf.Max(profile.GridSpacing * 2f, baseExtent / aspect);
                plateaus[i] = new StructuralPlateau(center, new Vector2(halfX, halfZ), 0f, plateauBlend, height);
            }

            var ramps = new List<StructuralRampSegment>(Mathf.Max(0, profile.RampCount));
            int elevatedPlateauCount = Mathf.Max(0, plateaus.Length - 1);
            int rampTargetCount = Mathf.Max(elevatedPlateauCount, profile.RampCount);
            for (int i = 0; i < rampTargetCount && elevatedPlateauCount > 0; i++)
            {
                int plateauIndex = 1 + i % elevatedPlateauCount;
                StructuralPlateau plateau = plateaus[plateauIndex];
                int localSeed = DeterministicSeed.Combine(seed, plateauIndex, i, 59243);
                Vector2 direction = CardinalRampDirection(plateau.Center, localSeed, i >= elevatedPlateauCount);
                float heightDelta = plateau.Height;
                if (heightDelta <= 0.001f)
                {
                    continue;
                }

                float minimumSlopeLength = heightDelta / Mathf.Max(0.001f, Mathf.Tan(Mathf.Clamp(slopeLimitDegrees, 1f, 88f) * Mathf.Deg2Rad));
                if (minimumSlopeLength > maxRampLength)
                {
                    continue;
                }

                float randomLength = Mathf.Lerp(minRampLength, maxRampLength, DeterministicSeed.ToFloat01(localSeed + 17));
                float length = Mathf.Clamp(Mathf.Max(minimumSlopeLength, randomLength), minRampLength, maxRampLength);
                float width = Mathf.Lerp(minRampWidth, maxRampWidth, DeterministicSeed.ToFloat01(localSeed + 29));
                float endInset = plateau.HalfExtentAlong(-direction) * Mathf.Lerp(0.35f, 0.75f, DeterministicSeed.ToFloat01(localSeed + 31));
                Vector2 end = plateau.Center - direction * endInset;
                Vector2 start = end - direction * length;
                var ramp = new StructuralRampSegment(start, direction, length, width, rampSideBlend, 0f, plateau.Height);
                if (!profile.allowRampOverlap && OverlapsExistingRamp(ramp, ramps))
                {
                    continue;
                }

                ramps.Add(ramp);
            }

            return new StructuralTerrainGrammar(plateaus, ramps.ToArray());
        }

        private static float EvaluateStructuralHeight(
            Vector2 position,
            RunWorldGenerationProfile profile,
            StructuralTerrainGrammar grammar,
            int seed,
            out bool isRamp,
            out Vector2 rampDirection)
        {
            float height = 0f;
            float structureInfluence = 0f;
            isRamp = false;
            rampDirection = Vector2.zero;

            for (int i = 0; i < grammar.Ramps.Count; i++)
            {
                if (!grammar.Ramps[i].TryEvaluate(position, out float rampHeight, out float influence))
                {
                    continue;
                }

                height = Mathf.Max(height, rampHeight);
                structureInfluence = Mathf.Max(structureInfluence, influence);
                isRamp = true;
                rampDirection = grammar.Ramps[i].Direction;
            }

            for (int i = 0; i < grammar.Plateaus.Count; i++)
            {
                StructuralPlateau plateau = grammar.Plateaus[i];
                float influence = plateau.Influence(position);
                height = Mathf.Max(height, plateau.Height * influence);
                structureInfluence = Mathf.Max(structureInfluence, influence);
            }

            float variation = SurfaceVariation(position, profile, seed)
                * profile.MaxHeight
                * profile.SurfaceVariationStrength
                * Mathf.Lerp(1f, 0.25f, structureInfluence);
            height += variation;

            float boundaryFadeDistance = Mathf.Max(profile.GridSpacing * 2f, profile.BoundaryMargin);
            float boundaryFade = Mathf.SmoothStep(
                0f,
                1f,
                Mathf.Clamp01((profile.PlayableRadius - position.magnitude) / Mathf.Max(1f, boundaryFadeDistance)));
            height *= boundaryFade;
            return Mathf.Clamp(height, 0f, profile.MaxHeight);
        }

        private static float QuantizedAngle(float radians)
        {
            const int Steps = 8;
            float step = Mathf.PI * 2f / Steps;
            return Mathf.Round(radians / step) * step;
        }

        private static float SurfaceVariation(Vector2 position, RunWorldGenerationProfile profile, int seed)
        {
            float scale = Mathf.Max(profile.GridSpacing * 8f, profile.PlayableRadius * 0.12f);
            float a = Mathf.Sin((position.x + DeterministicSeed.ToSignedFloat(seed + 11) * 173f) / scale);
            float b = Mathf.Cos((position.y + DeterministicSeed.ToSignedFloat(seed + 23) * 191f) / (scale * 1.37f));
            float c = Mathf.Sin((position.x + position.y + DeterministicSeed.ToSignedFloat(seed + 37) * 149f) / (scale * 0.71f));
            return (a * 0.45f + b * 0.35f + c * 0.20f);
        }

        private static LandformStamp[] CreateLandforms(RunWorldGenerationProfile profile, int seed)
        {
            var stamps = new LandformStamp[profile.LandformStampCount];
            for (int i = 0; i < stamps.Length; i++)
            {
                int localSeed = DeterministicSeed.Combine(seed, i, 4409);
                float angle = DeterministicSeed.ToAngleRadians(localSeed);
                float radius = Mathf.Lerp(profile.PlayableRadius * 0.08f, profile.PlayableRadius * 0.72f, DeterministicSeed.ToFloat01(localSeed + 1));
                var center = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                float radiusX = Mathf.Lerp(profile.PlayableRadius * 0.18f, profile.PlayableRadius * 0.42f, DeterministicSeed.ToFloat01(localSeed + 2));
                float radiusZ = Mathf.Lerp(profile.PlayableRadius * 0.16f, profile.PlayableRadius * 0.46f, DeterministicSeed.ToFloat01(localSeed + 3));
                float rotation = DeterministicSeed.ToAngleRadians(localSeed + 4);
                float amplitude = Mathf.Lerp(profile.MaxHeight * 0.10f, profile.MaxHeight * 0.34f, DeterministicSeed.ToFloat01(localSeed + 5));
                if (DeterministicSeed.ToFloat01(localSeed + 6) < 0.28f)
                {
                    amplitude *= -0.45f;
                }

                stamps[i] = new LandformStamp(center, radiusX, radiusZ, rotation, amplitude);
            }

            return stamps;
        }

        private static void NormalizeHeights(float[] heights, float minHeight, float maxHeight, RunWorldGenerationProfile profile)
        {
            float range = Mathf.Max(0.0001f, maxHeight - minHeight);
            for (int i = 0; i < heights.Length; i++)
            {
                float normalized = Mathf.InverseLerp(minHeight, maxHeight, heights[i]);
                float height = Mathf.SmoothStep(0f, profile.MaxHeight, normalized);
                if (profile.TerraceSteps > 1 && profile.Angularity > 0f)
                {
                    float step = profile.MaxHeight / profile.TerraceSteps;
                    float terraced = Mathf.Round(height / step) * step;
                    height = Mathf.Lerp(height, terraced, profile.Angularity);
                }

                heights[i] = Mathf.Clamp(height, 0f, profile.MaxHeight);
            }
        }

        private static void ApplyStructuralHeightFinish(float[] heights, bool[] rampSamples, RunWorldGenerationProfile profile)
        {
            for (int i = 0; i < heights.Length; i++)
            {
                float height = Mathf.Clamp(heights[i], 0f, profile.MaxHeight);
                bool isRamp = rampSamples != null && i < rampSamples.Length && rampSamples[i];
                if (!isRamp && profile.TerraceSteps > 1 && profile.Angularity > 0f)
                {
                    float step = profile.MaxHeight / profile.TerraceSteps;
                    float terraced = Mathf.Round(height / step) * step;
                    height = Mathf.Lerp(height, terraced, profile.Angularity);
                }

                heights[i] = Mathf.Clamp(height, 0f, profile.MaxHeight);
            }
        }

        private static Vector3 NormalAt(float[] heights, int width, int depth, int x, int z, float spacing)
        {
            int left = Mathf.Max(0, x - 1);
            int right = Mathf.Min(width - 1, x + 1);
            int down = Mathf.Max(0, z - 1);
            int up = Mathf.Min(depth - 1, z + 1);
            float heightLeft = heights[z * width + left];
            float heightRight = heights[z * width + right];
            float heightDown = heights[down * width + x];
            float heightUp = heights[up * width + x];
            float dx = (heightRight - heightLeft) / Mathf.Max(spacing, (right - left) * spacing);
            float dz = (heightUp - heightDown) / Mathf.Max(spacing, (up - down) * spacing);
            return new Vector3(-dx, 1f, -dz).normalized;
        }

        private static Vector2 CardinalRampDirection(Vector2 plateauCenter, int seed, bool allowAnyCardinal)
        {
            if (allowAnyCardinal)
            {
                return (Mathf.Abs(DeterministicSeed.Combine(seed, 13)) % 4) switch
                {
                    0 => Vector2.right,
                    1 => Vector2.left,
                    2 => Vector2.up,
                    _ => Vector2.down
                };
            }

            if (Mathf.Abs(plateauCenter.x) >= Mathf.Abs(plateauCenter.y))
            {
                return plateauCenter.x >= 0f ? Vector2.right : Vector2.left;
            }

            return plateauCenter.y >= 0f ? Vector2.up : Vector2.down;
        }

        private static bool OverlapsExistingRamp(StructuralRampSegment ramp, IReadOnlyList<StructuralRampSegment> existingRamps)
        {
            for (int i = 0; i < existingRamps.Count; i++)
            {
                StructuralRampSegment existing = existingRamps[i];
                float minDistance = ramp.Width + existing.Width;
                if (Vector2.Distance(ramp.Center, existing.Center) < minDistance)
                {
                    return true;
                }
            }

            return false;
        }

        private static float CenterScore(Vector2 position, RunWorldGenerationProfile profile)
        {
            return 1f - Mathf.Clamp01(position.magnitude / Mathf.Max(1f, profile.PlayableRadius));
        }

        private readonly struct LandformStamp
        {
            private readonly Vector2 center;
            private readonly float radiusX;
            private readonly float radiusZ;
            private readonly float cos;
            private readonly float sin;
            private readonly float amplitude;

            public LandformStamp(Vector2 center, float radiusX, float radiusZ, float rotation, float amplitude)
            {
                this.center = center;
                this.radiusX = Mathf.Max(1f, radiusX);
                this.radiusZ = Mathf.Max(1f, radiusZ);
                cos = Mathf.Cos(rotation);
                sin = Mathf.Sin(rotation);
                this.amplitude = amplitude;
            }

            public float Evaluate(Vector2 position)
            {
                Vector2 delta = position - center;
                float localX = delta.x * cos - delta.y * sin;
                float localZ = delta.x * sin + delta.y * cos;
                float d = (localX * localX) / (radiusX * radiusX) + (localZ * localZ) / (radiusZ * radiusZ);
                if (d >= 1f)
                {
                    return 0f;
                }

                float t = 1f - d;
                return amplitude * t * t * (3f - 2f * t);
            }
        }

        private readonly struct StructuralTerrainGrammar
        {
            public StructuralTerrainGrammar(IReadOnlyList<StructuralPlateau> plateaus, IReadOnlyList<StructuralRampSegment> ramps)
            {
                Plateaus = plateaus ?? Array.Empty<StructuralPlateau>();
                Ramps = ramps ?? Array.Empty<StructuralRampSegment>();
            }

            public IReadOnlyList<StructuralPlateau> Plateaus { get; }
            public IReadOnlyList<StructuralRampSegment> Ramps { get; }
        }

        private readonly struct StructuralPlateau
        {
            private readonly Vector2 halfExtents;
            private readonly float cos;
            private readonly float sin;
            private readonly float blendRadius;

            public StructuralPlateau(Vector2 center, Vector2 halfExtents, float rotationRadians, float blendRadius, float height)
            {
                Center = center;
                this.halfExtents = new Vector2(Mathf.Max(1f, halfExtents.x), Mathf.Max(1f, halfExtents.y));
                cos = Mathf.Cos(rotationRadians);
                sin = Mathf.Sin(rotationRadians);
                this.blendRadius = Mathf.Max(1f, blendRadius);
                Height = Mathf.Max(0f, height);
            }

            public Vector2 Center { get; }
            public float Height { get; }

            public float HalfExtentAlong(Vector2 direction)
            {
                Vector2 safeDirection = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.right;
                float localX = safeDirection.x * cos - safeDirection.y * sin;
                float localZ = safeDirection.x * sin + safeDirection.y * cos;
                float xDistance = Mathf.Abs(localX) > 0.0001f ? halfExtents.x / Mathf.Abs(localX) : float.MaxValue;
                float zDistance = Mathf.Abs(localZ) > 0.0001f ? halfExtents.y / Mathf.Abs(localZ) : float.MaxValue;
                return Mathf.Min(xDistance, zDistance);
            }

            public float Influence(Vector2 position)
            {
                Vector2 delta = position - Center;
                float localX = delta.x * cos - delta.y * sin;
                float localZ = delta.x * sin + delta.y * cos;
                float outsideX = Mathf.Max(0f, Mathf.Abs(localX) - halfExtents.x);
                float outsideZ = Mathf.Max(0f, Mathf.Abs(localZ) - halfExtents.y);
                float outsideDistance = Mathf.Sqrt(outsideX * outsideX + outsideZ * outsideZ);
                if (outsideDistance <= 0f)
                {
                    return 1f;
                }

                float t = Mathf.Clamp01(outsideDistance / blendRadius);
                return 1f - t;
            }
        }

        private readonly struct StructuralRampSegment
        {
            private readonly Vector2 start;
            private readonly Vector2 direction;
            private readonly Vector2 right;
            private readonly float length;
            private readonly float startHeight;
            private readonly float endHeight;
            private readonly float width;
            private readonly float blendWidth;

            public StructuralRampSegment(Vector2 start, Vector2 direction, float length, float width, float blendWidth, float startHeight, float endHeight)
            {
                this.start = start;
                this.direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.right;
                right = new Vector2(-this.direction.y, this.direction.x);
                this.length = Mathf.Max(1f, length);
                this.startHeight = Mathf.Max(0f, startHeight);
                this.endHeight = Mathf.Max(0f, endHeight);
                this.width = Mathf.Max(1f, width);
                this.blendWidth = Mathf.Max(0f, blendWidth);
            }

            public Vector2 Center => start + direction * (length * 0.5f);
            public Vector2 Direction => direction;
            public float Width => width;

            public bool TryEvaluate(Vector2 position, out float height, out float influence)
            {
                height = 0f;
                influence = 0f;
                Vector2 delta = position - start;
                float along = Vector2.Dot(delta, direction);
                if (along < 0f || along > length)
                {
                    return false;
                }

                float sideDistance = Mathf.Abs(Vector2.Dot(delta, right));
                if (sideDistance > width + blendWidth)
                {
                    return false;
                }

                float sideInfluence = sideDistance <= width || blendWidth <= 0f
                    ? 1f
                    : 1f - Mathf.Clamp01((sideDistance - width) / blendWidth);
                float endEase = Mathf.Clamp01(along / length);
                height = Mathf.Lerp(startHeight, endHeight, endEase);
                influence = sideInfluence;
                return influence > 0f;
            }
        }
    }
}
