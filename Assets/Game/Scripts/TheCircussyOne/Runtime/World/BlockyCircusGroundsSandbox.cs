using System;
using System.Collections.Generic;
using TheCircussyOne.Rules;
using UnityEngine;

namespace TheCircussyOne.Runtime
{
    public enum BlockyCircusGroundsPieceKind
    {
        RaisedStage,
        RampAisle,
        VerticalLedge,
        RewardPocket,
        TreeGrove
    }

    public readonly struct BlockyCircusGroundsPiece
    {
        public BlockyCircusGroundsPiece(
            BlockyCircusGroundsPieceKind kind,
            Vector3 center,
            Vector3 size,
            Vector2 direction,
            bool walkable,
            bool ramp,
            bool rewardSafe,
            Vector2Int footprintStart = default,
            int footprintLength = 0,
            int footprintHalfWidth = 0)
        {
            Kind = kind;
            Center = center;
            Size = size;
            Direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.zero;
            Walkable = walkable;
            Ramp = ramp;
            RewardSafe = rewardSafe;
            FootprintStart = footprintStart;
            FootprintLength = Mathf.Max(0, footprintLength);
            FootprintHalfWidth = Mathf.Max(0, footprintHalfWidth);
        }

        public BlockyCircusGroundsPieceKind Kind { get; }
        public Vector3 Center { get; }
        public Vector3 Size { get; }
        public Vector2 Direction { get; }
        public bool Walkable { get; }
        public bool Ramp { get; }
        public bool RewardSafe { get; }
        public Vector2Int FootprintStart { get; }
        public int FootprintLength { get; }
        public int FootprintHalfWidth { get; }
        public bool HasFootprint => FootprintLength > 0 || FootprintHalfWidth > 0;
    }

    public readonly struct BlockyCircusGroundsSandboxSettings
    {
        public BlockyCircusGroundsSandboxSettings(
            int seed,
            int sampleWidth,
            int sampleDepth,
            float gridSpacing,
            float stageHeight,
            float constructedStep,
            float wallHeightThreshold,
            float playerSlopeLimitDegrees,
            float readableRampSlopeLimitDegrees)
        {
            Seed = seed;
            SampleWidth = Mathf.Max(9, sampleWidth | 1);
            SampleDepth = Mathf.Max(9, sampleDepth | 1);
            GridSpacing = Mathf.Max(1f, gridSpacing);
            StageHeight = Mathf.Max(1f, stageHeight);
            ConstructedStep = Mathf.Clamp(constructedStep, 0.5f, StageHeight);
            WallHeightThreshold = Mathf.Max(0.1f, wallHeightThreshold);
            PlayerSlopeLimitDegrees = Mathf.Clamp(playerSlopeLimitDegrees, 1f, 89f);
            ReadableRampSlopeLimitDegrees = Mathf.Clamp(readableRampSlopeLimitDegrees, 1f, PlayerSlopeLimitDegrees);
        }

        public int Seed { get; }
        public int SampleWidth { get; }
        public int SampleDepth { get; }
        public float GridSpacing { get; }
        public float StageHeight { get; }
        public float ConstructedStep { get; }
        public float WallHeightThreshold { get; }
        public float PlayerSlopeLimitDegrees { get; }
        public float ReadableRampSlopeLimitDegrees { get; }

        public float PlayableRadius => (Mathf.Min(SampleWidth, SampleDepth) - 1) * GridSpacing * 0.5f;

        public static BlockyCircusGroundsSandboxSettings Default(int seed)
        {
            return new BlockyCircusGroundsSandboxSettings(
                seed,
                sampleWidth: 129,
                sampleDepth: 129,
                gridSpacing: 4f,
                stageHeight: 32f,
                constructedStep: 4f,
                wallHeightThreshold: 1.25f,
                playerSlopeLimitDegrees: 42f,
                readableRampSlopeLimitDegrees: 15f);
        }
    }

    public sealed class BlockyCircusGroundsSandboxResult
    {
        public BlockyCircusGroundsSandboxResult(
            GeneratedWorldMap map,
            IReadOnlyList<BlockyCircusGroundsPiece> pieces,
            BlockyCircusGroundsSandboxValidation validation)
        {
            Map = map;
            Pieces = pieces ?? Array.Empty<BlockyCircusGroundsPiece>();
            Validation = validation;
        }

        public GeneratedWorldMap Map { get; }
        public IReadOnlyList<BlockyCircusGroundsPiece> Pieces { get; }
        public BlockyCircusGroundsSandboxValidation Validation { get; }

        public IEnumerable<BlockyCircusGroundsPiece> PiecesOfKind(BlockyCircusGroundsPieceKind kind)
        {
            for (int i = 0; i < Pieces.Count; i++)
            {
                if (Pieces[i].Kind == kind)
                {
                    yield return Pieces[i];
                }
            }
        }
    }

    public readonly struct BlockyCircusGroundsSandboxValidation
    {
        public BlockyCircusGroundsSandboxValidation(
            int acceptedRampCount,
            int rejectedRampCount,
            int rejectedRampOccupancyCount,
            int rejectedRampUnsupportedCount,
            int unreachableWalkableSampleCount,
            int unreachableRewardSampleCount,
            int trapSampleCount,
            int outerSectorCoverage,
            int outerSectorCount,
            int repairedBasinCount = 0,
            int unrepairedBasinCount = 0)
        {
            AcceptedRampCount = Mathf.Max(0, acceptedRampCount);
            RejectedRampCount = Mathf.Max(0, rejectedRampCount);
            RejectedRampOccupancyCount = Mathf.Max(0, rejectedRampOccupancyCount);
            RejectedRampUnsupportedCount = Mathf.Max(0, rejectedRampUnsupportedCount);
            UnreachableWalkableSampleCount = Mathf.Max(0, unreachableWalkableSampleCount);
            UnreachableRewardSampleCount = Mathf.Max(0, unreachableRewardSampleCount);
            TrapSampleCount = Mathf.Max(0, trapSampleCount);
            OuterSectorCoverage = Mathf.Max(0, outerSectorCoverage);
            OuterSectorCount = Mathf.Max(0, outerSectorCount);
            RepairedBasinCount = Mathf.Max(0, repairedBasinCount);
            UnrepairedBasinCount = Mathf.Max(0, unrepairedBasinCount);
        }

        public int AcceptedRampCount { get; }
        public int RejectedRampCount { get; }
        public int RejectedRampOccupancyCount { get; }
        public int RejectedRampUnsupportedCount { get; }
        public int UnreachableWalkableSampleCount { get; }
        public int UnreachableRewardSampleCount { get; }
        public int TrapSampleCount { get; }
        public int OuterSectorCoverage { get; }
        public int OuterSectorCount { get; }
        public int RepairedBasinCount { get; }
        public int UnrepairedBasinCount { get; }

        public bool RewardPlacementValid => UnreachableRewardSampleCount == 0;
        public bool PitTrapValid => TrapSampleCount == 0 && UnrepairedBasinCount == 0;
        public bool OuterSectorCoverageValid => OuterSectorCount == 0 || OuterSectorCoverage >= OuterSectorCount;
    }

    internal readonly struct BlockyCircusGroundsBasinRepairResult
    {
        public BlockyCircusGroundsBasinRepairResult(
            int detectedBasinCount,
            int rampRepairedCount,
            int filledBasinCount,
            int unrepairedBasinCount)
        {
            DetectedBasinCount = Mathf.Max(0, detectedBasinCount);
            RampRepairedCount = Mathf.Max(0, rampRepairedCount);
            FilledBasinCount = Mathf.Max(0, filledBasinCount);
            UnrepairedBasinCount = Mathf.Max(0, unrepairedBasinCount);
        }

        public int DetectedBasinCount { get; }
        public int RampRepairedCount { get; }
        public int FilledBasinCount { get; }
        public int RepairedBasinCount => RampRepairedCount + FilledBasinCount;
        public int UnrepairedBasinCount { get; }
    }

    public static class BlockyCircusGroundsGrammar
    {
        private const int BaseLandformStampCount = 24;
        private const int ConstructedBlockCount = 84;
        private const int OuterConstructedBlockCount = 56;
        private const int MiddleConstructedBlockCount = 20;
        private const int RewardPocketCount = 14;
        private const int OuterSectorCount = 16;
        private const float RewardPocketMaxSlopeDegrees = 2f;
        private const float BaseLandformHeightFraction = 0.88f;
        private const float BoundaryHeightFadeFraction = 0.04f;
        private const float MaxUnsupportedRampGapMultiplier = 0.35f;
        private const int BasinRepairMaxPasses = 4;
        private const int BasinRepairMinimumSampleCount = 4;
        private const int BasinRepairRampHalfWidth = 1;
        private const int BasinRepairRampExtraLength = 8;

        public static BlockyCircusGroundsSandboxResult Generate(BlockyCircusGroundsSandboxSettings settings)
        {
            int width = settings.SampleWidth;
            int depth = settings.SampleDepth;
            int centerX = width / 2;
            int centerZ = depth / 2;
            Vector2Int center = new Vector2Int(centerX, centerZ);
            var heights = new float[width * depth];
            var flatSamples = new bool[width * depth];
            var rewardSamples = new bool[width * depth];
            var rampSamples = new bool[width * depth];
            var pieces = new List<BlockyCircusGroundsPiece>(ConstructedBlockCount * 2);
            var reservation = new TerrainReservation(width, depth);

            ApplySmoothBase(settings, heights, width, depth);
            BlockyCircusGroundsSandboxValidation constructionValidation = AddConstructedLayer(
                settings,
                heights,
                flatSamples,
                rewardSamples,
                rampSamples,
                reservation,
                pieces,
                width,
                depth,
                center);
            BlockyCircusGroundsBasinRepairResult basinRepair = RepairEnclosedBasins(
                settings,
                heights,
                flatSamples,
                rewardSamples,
                rampSamples,
                reservation,
                pieces,
                width,
                depth);
            AddReferenceDressingPieces(settings, heights, pieces, width, depth, center);

            var samples = new GeneratedWorldSample[width * depth];
            var normals = new Vector3[width * depth];
            var slopes = new float[width * depth];
            var walkableSamples = new bool[width * depth];

            for (int z = 0; z < depth; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = z * width + x;
                    bool inside = InsidePlayable(settings, width, depth, x, z);
                    Vector3 normal = NormalAt(heights, width, depth, x, z, settings.GridSpacing);
                    float slope = Vector3.Angle(normal, Vector3.up);
                    bool walkable = inside && (rampSamples[index] || slope <= settings.PlayerSlopeLimitDegrees + 0.01f);
                    normals[index] = normal;
                    slopes[index] = slope;
                    walkableSamples[index] = walkable;
                }
            }

            bool[] reachableSamples = BuildReachableSamples(settings, heights, walkableSamples, rampSamples, width, depth, center);
            RejectUnreachableRewardSamples(rewardSamples, walkableSamples, reachableSamples);
            int trapSamples = CountTrapSamples(settings, heights, walkableSamples, reachableSamples, rampSamples, width, depth);
            if (trapSamples > 0
                && FillIsolatedTrapSamples(settings, heights, flatSamples, rewardSamples, rampSamples, reservation, walkableSamples, reachableSamples, width, depth) > 0)
            {
                RebuildSampleMetrics(settings, heights, rampSamples, width, depth, normals, slopes, walkableSamples);
                reachableSamples = BuildReachableSamples(settings, heights, walkableSamples, rampSamples, width, depth, center);
                RejectUnreachableRewardSamples(rewardSamples, walkableSamples, reachableSamples);
                trapSamples = CountTrapSamples(settings, heights, walkableSamples, reachableSamples, rampSamples, width, depth);
            }

            int unreachableWalkableSamples = 0;
            int unreachableRewardSamples = 0;
            int outerSectorCoverage = CountOuterSectorCoverage(settings, pieces);

            for (int z = 0; z < depth; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = z * width + x;
                    bool inside = InsidePlayable(settings, width, depth, x, z);
                    bool walkable = walkableSamples[index];
                    bool reachable = reachableSamples[index];
                    if (walkable && !reachable)
                    {
                        unreachableWalkableSamples++;
                    }

                    if (rewardSamples[index] && walkable && !reachable)
                    {
                        unreachableRewardSamples++;
                    }

                    GeneratedWorldMask mask = inside
                        ? GeneratedWorldMask.None
                        : GeneratedWorldMask.None;
                    if (walkable)
                    {
                        mask |= GeneratedWorldMask.Walkable;
                    }

                    if (walkable && reachable)
                    {
                        mask |= GeneratedWorldMask.Reachable | GeneratedWorldMask.SpawnSafe;
                    }

                    if (x == centerX && z == centerZ)
                    {
                        mask |= GeneratedWorldMask.PlayerStartSafe;
                    }

                    bool rewardSafe = rewardSamples[index]
                        && flatSamples[index]
                        && walkable
                        && reachable
                        && slopes[index] <= RewardPocketMaxSlopeDegrees;
                    if (rewardSafe)
                    {
                        mask |= GeneratedWorldMask.RewardSafe;
                    }

                    samples[index] = new GeneratedWorldSample(
                        heights[index],
                        normals[index],
                        slopes[index],
                        mask,
                        settings.StageHeight > 0f ? heights[index] / settings.StageHeight : 0f,
                        CenterScore(settings, width, depth, x, z),
                        BoundaryScore(settings, width, depth, x, z),
                        flatSamples[index]
                            ? GeneratedWorldSurfaceKind.Flat
                            : GeneratedWorldSurfaceKind.Continuous,
                        Vector2.zero);
                }
            }

            var validation = new BlockyCircusGroundsSandboxValidation(
                constructionValidation.AcceptedRampCount + basinRepair.RampRepairedCount,
                constructionValidation.RejectedRampCount,
                constructionValidation.RejectedRampOccupancyCount,
                constructionValidation.RejectedRampUnsupportedCount,
                unreachableWalkableSamples,
                unreachableRewardSamples,
                trapSamples,
                outerSectorCoverage,
                OuterSectorCount,
                basinRepair.RepairedBasinCount,
                basinRepair.UnrepairedBasinCount);

            return new BlockyCircusGroundsSandboxResult(
                new GeneratedWorldMap(
                    actNumber: 1,
                    seed: settings.Seed,
                    attemptSeed: settings.Seed,
                    width,
                    depth,
                    settings.PlayableRadius,
                    settings.GridSpacing,
                    settings.StageHeight,
                    settings.WallHeightThreshold,
                    samples,
                    centerZ * width + centerX,
                    usedFallback: false,
                    warnings: null,
                    hasStructuralValidation: true,
                    structuralValidation: validation),
                pieces,
                validation);
        }

        private static void ApplySmoothBase(BlockyCircusGroundsSandboxSettings settings, float[] heights, int width, int depth)
        {
            var stamps = new BaseLandformStamp[BaseLandformStampCount];
            for (int i = 0; i < stamps.Length; i++)
            {
                int localSeed = DeterministicSeed.Combine(settings.Seed, i, 4409);
                float angle = DeterministicSeed.ToAngleRadians(localSeed);
                float radius = Mathf.Lerp(settings.PlayableRadius * 0.04f, settings.PlayableRadius * 0.98f, DeterministicSeed.ToFloat01(localSeed + 1));
                var center = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                float radiusX = Mathf.Lerp(settings.PlayableRadius * 0.14f, settings.PlayableRadius * 0.56f, DeterministicSeed.ToFloat01(localSeed + 2));
                float radiusZ = Mathf.Lerp(settings.PlayableRadius * 0.14f, settings.PlayableRadius * 0.58f, DeterministicSeed.ToFloat01(localSeed + 3));
                float rotation = DeterministicSeed.ToAngleRadians(localSeed + 4);
                float amplitude = Mathf.Lerp(settings.StageHeight * 0.16f, settings.StageHeight * 0.55f, DeterministicSeed.ToFloat01(localSeed + 5));
                if (DeterministicSeed.ToFloat01(localSeed + 6) < 0.22f)
                {
                    amplitude *= -0.32f;
                }

                stamps[i] = new BaseLandformStamp(center, radiusX, radiusZ, rotation, amplitude);
            }

            float minHeight = float.MaxValue;
            float maxHeight = float.MinValue;
            for (int z = 0; z < depth; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vector3 position = PositionFor(settings, width, depth, x, z, 0f);
                    var position2D = new Vector2(position.x, position.z);
                    float height = 0f;
                    for (int i = 0; i < stamps.Length; i++)
                    {
                        height += stamps[i].Evaluate(position2D);
                    }

                    float broadWave = Mathf.Sin((position2D.x + DeterministicSeed.ToSignedFloat(settings.Seed) * 97f) / Mathf.Max(1f, settings.PlayableRadius) * Mathf.PI * 2f)
                        * Mathf.Cos((position2D.y + DeterministicSeed.ToSignedFloat(settings.Seed + 17) * 89f) / Mathf.Max(1f, settings.PlayableRadius) * Mathf.PI * 2f);
                    height += broadWave * settings.StageHeight * 0.12f;
                    int index = z * width + x;
                    heights[index] = height;
                    minHeight = Mathf.Min(minHeight, height);
                    maxHeight = Mathf.Max(maxHeight, height);
                }
            }

            float range = Mathf.Max(0.0001f, maxHeight - minHeight);
            for (int i = 0; i < heights.Length; i++)
            {
                float normalized = Mathf.InverseLerp(minHeight, maxHeight, heights[i]);
                float height = Mathf.SmoothStep(0f, settings.StageHeight * BaseLandformHeightFraction, normalized);
                Vector3 position = PositionFor(settings, width, depth, i % width, i / width, 0f);
                float boundaryFadeDistance = Mathf.Max(settings.GridSpacing * 2f, settings.PlayableRadius * BoundaryHeightFadeFraction);
                float boundaryFade = Mathf.SmoothStep(
                    0f,
                    1f,
                    Mathf.Clamp01((settings.PlayableRadius - new Vector2(position.x, position.z).magnitude) / boundaryFadeDistance));
                heights[i] = height * boundaryFade;
            }
        }

        private static BlockyCircusGroundsSandboxValidation AddConstructedLayer(
            BlockyCircusGroundsSandboxSettings settings,
            float[] heights,
            bool[] flatSamples,
            bool[] rewardSamples,
            bool[] rampSamples,
            TerrainReservation reservation,
            List<BlockyCircusGroundsPiece> pieces,
            int width,
            int depth,
            Vector2Int center)
        {
            var baseHeights = new float[heights.Length];
            Array.Copy(heights, baseHeights, heights.Length);
            int emittedRewards = 0;
            int acceptedRampCount = 0;
            int rejectedRampCount = 0;
            int rejectedRampOccupancyCount = 0;
            int rejectedRampUnsupportedCount = 0;
            for (int i = 0; i < ConstructedBlockCount; i++)
            {
                int localSeed = DeterministicSeed.Combine(settings.Seed, i, 8123);
                ConstructedPlacement placement = ConstructedPlacementFor(settings, i, localSeed);
                float angle = placement.Angle;
                float radial = settings.PlayableRadius * placement.RadialFraction;
                Vector2 worldCenter = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radial;
                Vector2Int gridCenter = GridForWorld(settings, width, depth, worldCenter);
                int halfX = 1 + Mathf.Abs(DeterministicSeed.Combine(localSeed, 11)) % 5;
                int halfZ = 1 + Mathf.Abs(DeterministicSeed.Combine(localSeed, 13)) % 5;
                float baseHeight = AverageHeight(baseHeights, width, depth, gridCenter, halfX, halfZ);
                int randomTier = Mathf.Abs(DeterministicSeed.Combine(localSeed, 17)) % 3;
                int terrainTier = Mathf.FloorToInt(Mathf.Clamp01(baseHeight / Mathf.Max(1f, settings.StageHeight * BaseLandformHeightFraction)) * 3f);
                float extraHeight = settings.ConstructedStep * (randomTier + terrainTier + 1);
                if (i % 11 == 0)
                {
                    extraHeight += settings.ConstructedStep * 2f;
                }

                float topHeight = Mathf.Clamp(
                    QuantizeUp(baseHeight + extraHeight, settings.ConstructedStep),
                    settings.ConstructedStep,
                    settings.StageHeight);

                ApplyRectPlatform(heights, flatSamples, reservation, width, depth, gridCenter, halfX, halfZ, topHeight);
                bool rampAccepted = TryAddRampToPlatform(
                    settings,
                    heights,
                    rampSamples,
                    reservation,
                    pieces,
                    width,
                    depth,
                    gridCenter,
                    halfX,
                    halfZ,
                    center,
                    localSeed,
                    topHeight,
                    out Vector2Int direction,
                    out bool rejectedForOccupancy,
                    out bool rejectedForUnsupported);
                if (rampAccepted)
                {
                    acceptedRampCount++;
                }
                else
                {
                    rejectedRampCount++;
                    if (rejectedForOccupancy)
                    {
                        rejectedRampOccupancyCount++;
                    }

                    if (rejectedForUnsupported)
                    {
                        rejectedRampUnsupportedCount++;
                    }
                }

                bool rewardSafe = emittedRewards < RewardPocketCount
                    && rampAccepted
                    && (i % 5 == 0 || i >= ConstructedBlockCount - (RewardPocketCount - emittedRewards));
                if (rewardSafe)
                {
                    ApplyRewardPocket(rewardSamples, width, depth, gridCenter, Mathf.Max(0, Mathf.Min(halfX, halfZ) - 1));
                    emittedRewards++;
                }

                pieces.Add(new BlockyCircusGroundsPiece(
                    BlockyCircusGroundsPieceKind.RaisedStage,
                    PositionFor(settings, width, depth, gridCenter.x, gridCenter.y, topHeight),
                    new Vector3(settings.GridSpacing * (halfX * 2 + 1), 0.35f, settings.GridSpacing * (halfZ * 2 + 1)),
                    new Vector2(direction.x, direction.y),
                    walkable: true,
                    ramp: false,
                    rewardSafe: rewardSafe));

                if (rewardSafe)
                {
                    pieces.Add(new BlockyCircusGroundsPiece(
                        BlockyCircusGroundsPieceKind.RewardPocket,
                        PositionFor(settings, width, depth, gridCenter.x, gridCenter.y, topHeight + 0.1f),
                        new Vector3(settings.GridSpacing * Mathf.Max(2, Mathf.Min(halfX, halfZ)), 0.2f, settings.GridSpacing * Mathf.Max(2, Mathf.Min(halfX, halfZ))),
                        Vector2.zero,
                        walkable: true,
                        ramp: false,
                        rewardSafe: true));
                }
            }

            return new BlockyCircusGroundsSandboxValidation(
                acceptedRampCount,
                rejectedRampCount,
                rejectedRampOccupancyCount,
                rejectedRampUnsupportedCount,
                unreachableWalkableSampleCount: 0,
                unreachableRewardSampleCount: 0,
                trapSampleCount: 0,
                outerSectorCoverage: 0,
                outerSectorCount: OuterSectorCount);
        }

        private static ConstructedPlacement ConstructedPlacementFor(BlockyCircusGroundsSandboxSettings settings, int index, int seed)
        {
            float t = DeterministicSeed.ToFloat01(seed + 1);
            int outerLimit = OuterConstructedBlockCount;
            int middleLimit = OuterConstructedBlockCount + MiddleConstructedBlockCount;
            if (index < outerLimit)
            {
                int sector = index % OuterSectorCount;
                int ring = index / OuterSectorCount;
                float angle = SectorAngle(sector, OuterSectorCount, seed + ring * 211, jitterScale: 0.36f);
                float radial = Mathf.Lerp(0.66f, 0.98f, Mathf.Repeat(t + ring * 0.21f, 1f));
                return new ConstructedPlacement(angle, radial);
            }

            if (index < middleLimit)
            {
                int local = index - outerLimit;
                int sectorCount = 10;
                int sector = local % sectorCount;
                int ring = local / sectorCount;
                float angle = SectorAngle(sector, sectorCount, seed + ring * 337, jitterScale: 0.42f);
                float radial = Mathf.Lerp(0.36f, 0.68f, Mathf.Repeat(t + ring * 0.27f, 1f));
                return new ConstructedPlacement(angle, radial);
            }

            int innerLocal = index - middleLimit;
            int innerSectorCount = ConstructedBlockCount - middleLimit;
            float innerAngle = SectorAngle(innerLocal, Mathf.Max(1, innerSectorCount), seed, jitterScale: 0.48f);
            float innerRadial = Mathf.Lerp(0.08f, 0.34f, t);
            return new ConstructedPlacement(innerAngle, innerRadial);
        }

        private static float SectorAngle(int sector, int sectorCount, int seed, float jitterScale)
        {
            float jitter = DeterministicSeed.ToSignedFloat(seed + 19) * jitterScale;
            return (sector + 0.5f + jitter) / Mathf.Max(1, sectorCount) * Mathf.PI * 2f;
        }

        private static void ApplyRectPlatform(
            float[] heights,
            bool[] flatSamples,
            TerrainReservation reservation,
            int width,
            int depth,
            Vector2Int center,
            int halfX,
            int halfZ,
            float topHeight)
        {
            for (int z = -halfZ; z <= halfZ; z++)
            {
                for (int x = -halfX; x <= halfX; x++)
                {
                    Vector2Int sample = center + new Vector2Int(x, z);
                    if (TryIndex(width, depth, sample.x, sample.y, out int index))
                    {
                        if (!reservation.TryMarkPlatform(sample))
                        {
                            continue;
                        }

                        heights[index] = Mathf.Max(heights[index], topHeight);
                        flatSamples[index] = true;
                    }
                }
            }
        }

        private static float AverageHeight(float[] heights, int width, int depth, Vector2Int center, int halfX, int halfZ)
        {
            float total = 0f;
            int count = 0;
            for (int z = -halfZ; z <= halfZ; z++)
            {
                for (int x = -halfX; x <= halfX; x++)
                {
                    Vector2Int sample = center + new Vector2Int(x, z);
                    if (!TryIndex(width, depth, sample.x, sample.y, out int index))
                    {
                        continue;
                    }

                    total += heights[index];
                    count++;
                }
            }

            return count > 0 ? total / count : 0f;
        }

        private static bool TryAddRampToPlatform(
            BlockyCircusGroundsSandboxSettings settings,
            float[] heights,
            bool[] rampSamples,
            TerrainReservation reservation,
            List<BlockyCircusGroundsPiece> pieces,
            int width,
            int depth,
            Vector2Int center,
            int halfX,
            int halfZ,
            Vector2Int mapCenter,
            int seed,
            float topHeight,
            out Vector2Int acceptedDirection,
            out bool rejectedForOccupancy,
            out bool rejectedForUnsupported)
        {
            acceptedDirection = Vector2Int.zero;
            rejectedForOccupancy = false;
            rejectedForUnsupported = false;
            RampCandidate best = default;
            float bestScore = float.MinValue;
            Vector2Int[] directions = OrderedDirections(seed);
            for (int i = 0; i < directions.Length; i++)
            {
                if (!TryBuildRampCandidate(
                    settings,
                    heights,
                    reservation,
                    width,
                    depth,
                    center,
                    halfX,
                    halfZ,
                    directions[i],
                    mapCenter,
                    seed + i * 7919,
                    topHeight,
                    out RampCandidate candidate,
                    out bool occupancyRejected,
                    out bool unsupportedRejected))
                {
                    rejectedForOccupancy |= occupancyRejected;
                    rejectedForUnsupported |= unsupportedRejected;
                    continue;
                }

                if (candidate.Score > bestScore)
                {
                    best = candidate;
                    bestScore = candidate.Score;
                }
            }

            if (!best.Valid)
            {
                return false;
            }

            MarkRampSamples(rampSamples, reservation, width, depth, best);
            acceptedDirection = best.Direction;
            pieces.Add(new BlockyCircusGroundsPiece(
                BlockyCircusGroundsPieceKind.RampAisle,
                best.Center,
                best.Size,
                new Vector2(best.Direction.x, best.Direction.y),
                walkable: true,
                ramp: true,
                rewardSafe: false,
                footprintStart: best.Foot,
                footprintLength: best.Length,
                footprintHalfWidth: best.HalfWidth));
            return true;
        }

        private static bool TryBuildRampCandidate(
            BlockyCircusGroundsSandboxSettings settings,
            float[] heights,
            TerrainReservation reservation,
            int width,
            int depth,
            Vector2Int center,
            int halfX,
            int halfZ,
            Vector2Int direction,
            Vector2Int mapCenter,
            int seed,
            float topHeight,
            out RampCandidate candidate,
            out bool rejectedForOccupancy,
            out bool rejectedForUnsupported)
        {
            candidate = default;
            rejectedForOccupancy = false;
            rejectedForUnsupported = false;
            int halfAlong = direction.x != 0 ? halfX : halfZ;
            int halfAcross = direction.x != 0 ? halfZ : halfX;
            Vector2Int platformEdge = center - direction * halfAlong;
            Vector2Int exposedLip = platformEdge - direction;
            if (!TryIndex(width, depth, exposedLip.x, exposedLip.y, out _) || !InsidePlayable(settings, width, depth, exposedLip.x, exposedLip.y))
            {
                return false;
            }

            float rampSlopeLimit = settings.ReadableRampSlopeLimitDegrees;
            int maxLength = Mathf.Max(6, Mathf.CeilToInt(topHeight / Mathf.Max(1f, Mathf.Tan(rampSlopeLimit * Mathf.Deg2Rad) * settings.GridSpacing)) + 8);
            int rampHalfWidth = Mathf.Clamp(Mathf.Max(1, halfAcross / 2), 1, 3);
            Vector2Int foot = default;
            int footIndex = -1;
            int rampLength = 0;
            float footHeight = 0f;
            float heightDelta = 0f;
            float slopeDegrees = 0f;
            for (int length = 5; length <= maxLength; length++)
            {
                Vector2Int candidateFoot = exposedLip - direction * length;
                if (!TryIndex(width, depth, candidateFoot.x, candidateFoot.y, out int candidateFootIndex)
                    || !InsidePlayable(settings, width, depth, candidateFoot.x, candidateFoot.y))
                {
                    continue;
                }

                float candidateFootHeight = heights[candidateFootIndex];
                float candidateDelta = topHeight - candidateFootHeight;
                if (candidateDelta <= settings.GridSpacing * 0.15f)
                {
                    continue;
                }

                float candidateSlope = Mathf.Atan2(candidateDelta, Mathf.Max(0.001f, length * settings.GridSpacing)) * Mathf.Rad2Deg;
                if (candidateSlope > rampSlopeLimit + 0.01f)
                {
                    continue;
                }

                foot = candidateFoot;
                footIndex = candidateFootIndex;
                rampLength = length;
                footHeight = candidateFootHeight;
                heightDelta = candidateDelta;
                slopeDegrees = candidateSlope;
                break;
            }

            if (footIndex < 0)
            {
                return false;
            }

            if (!RampFootprintAvailable(
                settings,
                heights,
                reservation,
                width,
                depth,
                foot,
                direction,
                rampLength,
                rampHalfWidth,
                footHeight,
                topHeight,
                out rejectedForOccupancy,
                out rejectedForUnsupported))
            {
                return false;
            }

            Vector3 footPosition = PositionFor(settings, width, depth, foot.x, foot.y, footHeight);
            Vector3 edgePosition = PositionFor(settings, width, depth, exposedLip.x, exposedLip.y, topHeight);
            Vector3 rampCenter = Vector3.Lerp(footPosition, edgePosition, 0.5f);
            float centerDistance = Vector2.Distance(new Vector2(center.x, center.y), new Vector2(mapCenter.x, mapCenter.y));
            float directionVariety = DeterministicSeed.ToFloat01(seed + 23);
            float outwardAlignment = Mathf.Clamp01(Vector2.Dot(
                new Vector2(direction.x, direction.y).normalized,
                new Vector2(center.x - mapCenter.x, center.y - mapCenter.y).normalized) * 0.5f + 0.5f);
            float score = directionVariety * 0.25f
                + outwardAlignment * 0.3f
                + Mathf.Clamp01(centerDistance / Mathf.Max(1f, Mathf.Min(width, depth) * 0.5f)) * 0.25f
                - Mathf.Abs(slopeDegrees - rampSlopeLimit * 0.55f) * 0.005f;

            candidate = new RampCandidate(
                foot,
                exposedLip,
                direction,
                rampLength,
                rampHalfWidth,
                rampCenter,
                new Vector3(settings.GridSpacing * (rampHalfWidth * 2 + 1), heightDelta, settings.GridSpacing * rampLength),
                score);
            return true;
        }

        private static bool RampFootprintAvailable(
            BlockyCircusGroundsSandboxSettings settings,
            float[] heights,
            TerrainReservation reservation,
            int width,
            int depth,
            Vector2Int foot,
            Vector2Int direction,
            int rampLength,
            int rampHalfWidth,
            float footHeight,
            float topHeight,
            out bool rejectedForOccupancy,
            out bool rejectedForUnsupported)
        {
            rejectedForOccupancy = false;
            rejectedForUnsupported = false;
            Vector2Int side = new Vector2Int(-direction.y, direction.x);
            float unsupportedGapLimit = settings.GridSpacing * MaxUnsupportedRampGapMultiplier;
            for (int along = 0; along <= rampLength; along++)
            {
                float t = along / Mathf.Max(1f, rampLength);
                float rampHeight = Mathf.Lerp(footHeight, topHeight, t);
                for (int across = -rampHalfWidth; across <= rampHalfWidth; across++)
                {
                    Vector2Int sample = foot + direction * along + side * across;
                    if (!TryIndex(width, depth, sample.x, sample.y, out int index)
                        || !InsidePlayable(settings, width, depth, sample.x, sample.y))
                    {
                        return false;
                    }

                    TerrainReservationKind reservationKind = reservation.KindAt(sample);
                    if (reservationKind != TerrainReservationKind.Empty)
                    {
                        rejectedForOccupancy = true;
                        return false;
                    }

                    float terrainHeight = heights[index];
                    if (terrainHeight > rampHeight + settings.GridSpacing * 0.35f)
                    {
                        return false;
                    }

                    if (footHeight - terrainHeight > unsupportedGapLimit)
                    {
                        rejectedForUnsupported = true;
                        return false;
                    }
                }
            }

            return true;
        }

        private static void MarkRampSamples(
            bool[] rampSamples,
            TerrainReservation reservation,
            int width,
            int depth,
            RampCandidate candidate)
        {
            Vector2Int side = new Vector2Int(-candidate.Direction.y, candidate.Direction.x);
            for (int along = 0; along <= candidate.Length; along++)
            {
                for (int across = -candidate.HalfWidth; across <= candidate.HalfWidth; across++)
                {
                    Vector2Int sample = candidate.Foot + candidate.Direction * along + side * across;
                    if (TryIndex(width, depth, sample.x, sample.y, out int index))
                    {
                        rampSamples[index] = true;
                        reservation.MarkRamp(sample, along, across, candidate.HalfWidth);
                    }
                }
            }

            for (int across = -candidate.HalfWidth; across <= candidate.HalfWidth; across++)
            {
                Vector2Int topConnector = candidate.Lip + candidate.Direction + side * across;
                if (TryIndex(width, depth, topConnector.x, topConnector.y, out int index))
                {
                    rampSamples[index] = true;
                }
            }
        }

        private static Vector2Int[] OrderedDirections(int seed)
        {
            var directions = new[]
            {
                Vector2Int.up,
                Vector2Int.right,
                Vector2Int.down,
                Vector2Int.left
            };
            int offset = Mathf.Abs(DeterministicSeed.Combine(seed, 3449)) % directions.Length;
            for (int i = 0; i < directions.Length; i++)
            {
                int swap = Mathf.Abs(DeterministicSeed.Combine(seed, i, 8831)) % directions.Length;
                (directions[i], directions[swap]) = (directions[swap], directions[i]);
            }

            if (offset == 0)
            {
                return directions;
            }

            var ordered = new Vector2Int[directions.Length];
            for (int i = 0; i < directions.Length; i++)
            {
                ordered[i] = directions[(i + offset) % directions.Length];
            }

            return ordered;
        }

        private static void ApplyRewardPocket(bool[] rewardSamples, int width, int depth, Vector2Int pocketCenter, int radiusSamples)
        {
            for (int z = -radiusSamples; z <= radiusSamples; z++)
            {
                for (int x = -radiusSamples; x <= radiusSamples; x++)
                {
                    Vector2Int sample = pocketCenter + new Vector2Int(x, z);
                    if (TryIndex(width, depth, sample.x, sample.y, out int index))
                    {
                        rewardSamples[index] = true;
                    }
                }
            }
        }

        private static void AddReferenceDressingPieces(
            BlockyCircusGroundsSandboxSettings settings,
            float[] heights,
            List<BlockyCircusGroundsPiece> pieces,
            int width,
            int depth,
            Vector2Int center)
        {
            Vector2Int treeGridCenter = center + new Vector2Int(-24, -18);
            pieces.Add(new BlockyCircusGroundsPiece(
                BlockyCircusGroundsPieceKind.TreeGrove,
                PositionFor(settings, width, depth, treeGridCenter.x, treeGridCenter.y, HeightAt(heights, width, depth, treeGridCenter)),
                new Vector3(settings.GridSpacing * 8f, 3f, settings.GridSpacing * 8f),
                Vector2.zero,
                walkable: false,
                ramp: false,
                rewardSafe: false));
        }

        internal static BlockyCircusGroundsBasinRepairResult RepairEnclosedBasinsForTests(
            BlockyCircusGroundsSandboxSettings settings,
            float[] heights,
            bool[] flatSamples,
            bool[] rewardSamples,
            bool[] rampSamples,
            int width,
            int depth)
        {
            return RepairEnclosedBasinsForTests(
                settings,
                heights,
                flatSamples,
                rewardSamples,
                rampSamples,
                width,
                depth,
                out _);
        }

        internal static BlockyCircusGroundsBasinRepairResult RepairEnclosedBasinsForTests(
            BlockyCircusGroundsSandboxSettings settings,
            float[] heights,
            bool[] flatSamples,
            bool[] rewardSamples,
            bool[] rampSamples,
            int width,
            int depth,
            out IReadOnlyList<BlockyCircusGroundsPiece> pieces)
        {
            var repairPieces = new List<BlockyCircusGroundsPiece>();
            pieces = repairPieces;
            return RepairEnclosedBasins(
                settings,
                heights,
                flatSamples,
                rewardSamples,
                rampSamples,
                new TerrainReservation(width, depth),
                repairPieces,
                width,
                depth);
        }

        internal static int CountEnclosedBasinsForTests(
            BlockyCircusGroundsSandboxSettings settings,
            float[] heights,
            bool[] rampSamples,
            int width,
            int depth)
        {
            return FindEnclosedBasins(settings, heights, rampSamples, width, depth).Count;
        }

        private static BlockyCircusGroundsBasinRepairResult RepairEnclosedBasins(
            BlockyCircusGroundsSandboxSettings settings,
            float[] heights,
            bool[] flatSamples,
            bool[] rewardSamples,
            bool[] rampSamples,
            TerrainReservation reservation,
            List<BlockyCircusGroundsPiece> pieces,
            int width,
            int depth)
        {
            int detected = 0;
            int rampRepaired = 0;
            int filled = 0;
            int unrepaired = 0;
            for (int pass = 0; pass < BasinRepairMaxPasses; pass++)
            {
                List<BasinRegion> basins = FindEnclosedBasins(settings, heights, rampSamples, width, depth);
                if (basins.Count == 0)
                {
                    break;
                }

                detected += basins.Count;
                bool changed = false;
                for (int i = 0; i < basins.Count; i++)
                {
                    BasinRegion basin = basins[i];
                    if (TryFillBasin(
                        settings,
                        heights,
                        flatSamples,
                        rewardSamples,
                        rampSamples,
                        reservation,
                        pieces,
                        width,
                        depth,
                        basin))
                    {
                        filled++;
                        changed = true;
                        continue;
                    }

                    if (TryAddBasinEscapeRamp(
                        settings,
                        heights,
                        flatSamples,
                        rewardSamples,
                        rampSamples,
                        reservation,
                        pieces,
                        width,
                        depth,
                        basin))
                    {
                        rampRepaired++;
                        changed = true;
                        continue;
                    }

                    unrepaired++;
                }

                if (!changed)
                {
                    break;
                }
            }

            return new BlockyCircusGroundsBasinRepairResult(detected, rampRepaired, filled, unrepaired);
        }

        private static List<BasinRegion> FindEnclosedBasins(
            BlockyCircusGroundsSandboxSettings settings,
            float[] heights,
            bool[] rampSamples,
            int width,
            int depth)
        {
            bool[] walkableSamples = BuildBasinWalkableSamples(settings, heights, rampSamples, width, depth);
            var componentIds = new int[walkableSamples.Length];
            for (int i = 0; i < componentIds.Length; i++)
            {
                componentIds[i] = -1;
            }

            var basins = new List<BasinRegion>();
            var queue = new Queue<Vector2Int>();
            Vector2Int[] directions = CardinalDirections();
            float maxStepHeight = PlayerStepHeight(settings);
            int componentId = 0;

            for (int z = 1; z < depth - 1; z++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    int startIndex = z * width + x;
                    if (!walkableSamples[startIndex] || componentIds[startIndex] >= 0)
                    {
                        continue;
                    }

                    var region = new BasinRegion();
                    componentIds[startIndex] = componentId;
                    queue.Enqueue(new Vector2Int(x, z));
                    while (queue.Count > 0)
                    {
                        Vector2Int current = queue.Dequeue();
                        int currentIndex = current.y * width + current.x;
                        region.Add(current);

                        for (int i = 0; i < directions.Length; i++)
                        {
                            Vector2Int next = current + directions[i];
                            if (!TryIndex(width, depth, next.x, next.y, out int nextIndex)
                                || !walkableSamples[nextIndex]
                                || componentIds[nextIndex] >= 0)
                            {
                                continue;
                            }

                            if (!CanTraverseSampleEdge(heights, rampSamples, currentIndex, nextIndex, maxStepHeight))
                            {
                                continue;
                            }

                            componentIds[nextIndex] = componentId;
                            queue.Enqueue(next);
                        }
                    }

                    if (region.Samples.Count >= BasinRepairMinimumSampleCount
                        && IsEnclosedLowBasin(settings, heights, walkableSamples, rampSamples, componentIds, componentId, width, depth, maxStepHeight, directions, region))
                    {
                        basins.Add(region);
                    }

                    componentId++;
                }
            }

            return basins;
        }

        private static bool IsEnclosedLowBasin(
            BlockyCircusGroundsSandboxSettings settings,
            float[] heights,
            bool[] walkableSamples,
            bool[] rampSamples,
            int[] componentIds,
            int componentId,
            int width,
            int depth,
            float maxStepHeight,
            Vector2Int[] directions,
            BasinRegion region)
        {
            bool touchesBoundary = false;
            bool hasEscape = false;
            bool hasDropExit = false;

            for (int sampleIndex = 0; sampleIndex < region.Samples.Count; sampleIndex++)
            {
                Vector2Int sample = region.Samples[sampleIndex];
                int index = sample.y * width + sample.x;
                float height = heights[index];
                for (int directionIndex = 0; directionIndex < directions.Length; directionIndex++)
                {
                    Vector2Int direction = directions[directionIndex];
                    Vector2Int next = sample + direction;
                    if (!TryIndex(width, depth, next.x, next.y, out int nextIndex)
                        || !InsidePlayable(settings, width, depth, next.x, next.y))
                    {
                        touchesBoundary = true;
                        continue;
                    }

                    if (componentIds[nextIndex] == componentId)
                    {
                        continue;
                    }

                    float delta = heights[nextIndex] - height;
                    if (walkableSamples[nextIndex] && CanTraverseSampleEdge(heights, rampSamples, index, nextIndex, maxStepHeight))
                    {
                        hasEscape = true;
                    }
                    else if (delta < -maxStepHeight)
                    {
                        hasDropExit = true;
                    }
                    else if (delta > maxStepHeight)
                    {
                        region.AddHighEdge(new BasinBoundaryEdge(sample, next, direction, height, heights[nextIndex]));
                    }
                }
            }

            return !touchesBoundary
                && !hasEscape
                && !hasDropExit
                && region.HighEdges.Count > 0;
        }

        private static bool TryAddBasinEscapeRamp(
            BlockyCircusGroundsSandboxSettings settings,
            float[] heights,
            bool[] flatSamples,
            bool[] rewardSamples,
            bool[] rampSamples,
            TerrainReservation reservation,
            List<BlockyCircusGroundsPiece> pieces,
            int width,
            int depth,
            BasinRegion basin)
        {
            BasinBoundaryEdge[] edges = basin.HighEdges
                .ToArray();
            Array.Sort(edges, static (a, b) => a.HeightDelta.CompareTo(b.HeightDelta));

            for (int i = 0; i < edges.Length; i++)
            {
                BasinBoundaryEdge edge = edges[i];
                float targetHeight = edge.OutsideHeight;
                int minimumLength = Mathf.CeilToInt(edge.HeightDelta / Mathf.Max(0.001f, Mathf.Tan(settings.ReadableRampSlopeLimitDegrees * Mathf.Deg2Rad) * settings.GridSpacing));
                int maxLength = minimumLength + BasinRepairRampExtraLength;
                for (int length = Mathf.Max(3, minimumLength); length <= maxLength; length++)
                {
                    Vector2Int foot = edge.Outside - edge.Direction * length;
                    if (!TryIndex(width, depth, foot.x, foot.y, out int footIndex)
                        || !InsidePlayable(settings, width, depth, foot.x, foot.y))
                    {
                        continue;
                    }

                    float footHeight = heights[footIndex];
                    if (targetHeight - footHeight <= settings.GridSpacing * 0.15f)
                    {
                        continue;
                    }

                    if (!BasinRampFootprintAvailable(
                        settings,
                        heights,
                        reservation,
                        basin,
                        width,
                        depth,
                        foot,
                        edge.Direction,
                        length,
                        BasinRepairRampHalfWidth,
                        footHeight,
                        targetHeight))
                    {
                        continue;
                    }

                    var candidate = CreateBasinRampCandidate(settings, width, depth, foot, edge, length, footHeight, targetHeight);
                    ApplyBasinRamp(
                        settings,
                        heights,
                        flatSamples,
                        rewardSamples,
                        rampSamples,
                        reservation,
                        width,
                        depth,
                        candidate,
                        footHeight,
                        targetHeight);
                    pieces.Add(new BlockyCircusGroundsPiece(
                        BlockyCircusGroundsPieceKind.RampAisle,
                        candidate.Center,
                        candidate.Size,
                        new Vector2(candidate.Direction.x, candidate.Direction.y),
                        walkable: true,
                        ramp: true,
                        rewardSafe: false,
                        footprintStart: candidate.Foot,
                        footprintLength: candidate.Length,
                        footprintHalfWidth: candidate.HalfWidth));
                    return true;
                }
            }

            return false;
        }

        private static RampCandidate CreateBasinRampCandidate(
            BlockyCircusGroundsSandboxSettings settings,
            int width,
            int depth,
            Vector2Int foot,
            BasinBoundaryEdge edge,
            int length,
            float footHeight,
            float targetHeight)
        {
            Vector3 footPosition = PositionFor(settings, width, depth, foot.x, foot.y, footHeight);
            Vector3 topPosition = PositionFor(settings, width, depth, edge.Outside.x, edge.Outside.y, targetHeight);
            return new RampCandidate(
                foot,
                edge.Inside,
                edge.Direction,
                length,
                BasinRepairRampHalfWidth,
                Vector3.Lerp(footPosition, topPosition, 0.5f),
                new Vector3(settings.GridSpacing * (BasinRepairRampHalfWidth * 2 + 1), targetHeight - footHeight, settings.GridSpacing * length),
                0f);
        }

        private static bool BasinRampFootprintAvailable(
            BlockyCircusGroundsSandboxSettings settings,
            float[] heights,
            TerrainReservation reservation,
            BasinRegion basin,
            int width,
            int depth,
            Vector2Int foot,
            Vector2Int direction,
            int rampLength,
            int rampHalfWidth,
            float footHeight,
            float topHeight)
        {
            Vector2Int side = new Vector2Int(-direction.y, direction.x);
            float heightTolerance = settings.GridSpacing * 0.35f;
            for (int along = 0; along <= rampLength; along++)
            {
                float t = along / Mathf.Max(1f, rampLength);
                float rampHeight = Mathf.Lerp(footHeight, topHeight, t);
                for (int across = -rampHalfWidth; across <= rampHalfWidth; across++)
                {
                    Vector2Int sample = foot + direction * along + side * across;
                    if (!TryIndex(width, depth, sample.x, sample.y, out int index)
                        || !InsidePlayable(settings, width, depth, sample.x, sample.y))
                    {
                        return false;
                    }

                    if (along < rampLength && across == 0 && !basin.Contains(sample))
                    {
                        return false;
                    }

                    TerrainReservationKind reservationKind = reservation.KindAt(sample);
                    if (reservationKind == TerrainReservationKind.RampCore
                        || reservationKind == TerrainReservationKind.RampShoulder)
                    {
                        return false;
                    }

                    if (reservationKind == TerrainReservationKind.Platform && along < rampLength - 1)
                    {
                        return false;
                    }

                    float terrainHeight = heights[index];
                    if (terrainHeight > rampHeight + heightTolerance && along < rampLength)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static void ApplyBasinRamp(
            BlockyCircusGroundsSandboxSettings settings,
            float[] heights,
            bool[] flatSamples,
            bool[] rewardSamples,
            bool[] rampSamples,
            TerrainReservation reservation,
            int width,
            int depth,
            RampCandidate candidate,
            float footHeight,
            float topHeight)
        {
            Vector2Int side = new Vector2Int(-candidate.Direction.y, candidate.Direction.x);
            for (int along = 0; along <= candidate.Length; along++)
            {
                float t = along / Mathf.Max(1f, candidate.Length);
                float rampHeight = Mathf.Lerp(footHeight, topHeight, t);
                for (int across = -candidate.HalfWidth; across <= candidate.HalfWidth; across++)
                {
                    Vector2Int sample = candidate.Foot + candidate.Direction * along + side * across;
                    if (!TryIndex(width, depth, sample.x, sample.y, out int index))
                    {
                        continue;
                    }

                    heights[index] = along == candidate.Length
                        ? Mathf.Max(heights[index], rampHeight)
                        : rampHeight;
                    flatSamples[index] = false;
                    rewardSamples[index] = false;
                    rampSamples[index] = true;
                    reservation.MarkRamp(sample, along, across, candidate.HalfWidth);
                }
            }

            for (int across = -candidate.HalfWidth; across <= candidate.HalfWidth; across++)
            {
                Vector2Int topConnector = candidate.Lip + candidate.Direction + side * across;
                if (TryIndex(width, depth, topConnector.x, topConnector.y, out int index))
                {
                    rewardSamples[index] = false;
                    rampSamples[index] = true;
                }
            }
        }

        private static bool TryFillBasin(
            BlockyCircusGroundsSandboxSettings settings,
            float[] heights,
            bool[] flatSamples,
            bool[] rewardSamples,
            bool[] rampSamples,
            TerrainReservation reservation,
            List<BlockyCircusGroundsPiece> pieces,
            int width,
            int depth,
            BasinRegion basin)
        {
            if (basin.HighEdges.Count == 0)
            {
                return false;
            }

            float fillHeight = float.MaxValue;
            for (int i = 0; i < basin.HighEdges.Count; i++)
            {
                fillHeight = Mathf.Min(fillHeight, basin.HighEdges[i].OutsideHeight);
            }

            if (float.IsNaN(fillHeight) || float.IsInfinity(fillHeight))
            {
                return false;
            }

            for (int i = 0; i < basin.Samples.Count; i++)
            {
                Vector2Int sample = basin.Samples[i];
                int index = sample.y * width + sample.x;
                heights[index] = Mathf.Max(heights[index], fillHeight);
                flatSamples[index] = true;
                rewardSamples[index] = false;
                rampSamples[index] = false;
                reservation.TryMarkPlatform(sample);
            }

            Vector2Int center = new Vector2Int((basin.MinX + basin.MaxX) / 2, (basin.MinZ + basin.MaxZ) / 2);
            pieces.Add(new BlockyCircusGroundsPiece(
                BlockyCircusGroundsPieceKind.RaisedStage,
                PositionFor(settings, width, depth, center.x, center.y, fillHeight),
                new Vector3(settings.GridSpacing * (basin.MaxX - basin.MinX + 1), 0.35f, settings.GridSpacing * (basin.MaxZ - basin.MinZ + 1)),
                Vector2.zero,
                walkable: true,
                ramp: false,
                rewardSafe: false));
            return true;
        }

        private static bool[] BuildBasinWalkableSamples(
            BlockyCircusGroundsSandboxSettings settings,
            float[] heights,
            bool[] rampSamples,
            int width,
            int depth)
        {
            var walkableSamples = new bool[heights.Length];
            for (int z = 0; z < depth; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = z * width + x;
                    if (!InsidePlayable(settings, width, depth, x, z))
                    {
                        continue;
                    }

                    Vector3 normal = NormalAt(heights, width, depth, x, z, settings.GridSpacing);
                    float slope = Vector3.Angle(normal, Vector3.up);
                    walkableSamples[index] = rampSamples[index] || slope <= settings.PlayerSlopeLimitDegrees + 0.01f;
                }
            }

            return walkableSamples;
        }

        private static bool CanTraverseSampleEdge(float[] heights, bool[] rampSamples, int currentIndex, int nextIndex, float maxStepHeight)
        {
            return rampSamples[currentIndex]
                || rampSamples[nextIndex]
                || Mathf.Abs(heights[nextIndex] - heights[currentIndex]) <= maxStepHeight;
        }

        private static float PlayerStepHeight(BlockyCircusGroundsSandboxSettings settings)
        {
            return Mathf.Tan(settings.PlayerSlopeLimitDegrees * Mathf.Deg2Rad) * settings.GridSpacing + 0.1f;
        }

        private static Vector2Int[] CardinalDirections()
        {
            return new[]
            {
                Vector2Int.up,
                Vector2Int.right,
                Vector2Int.down,
                Vector2Int.left
            };
        }

        private static bool[] BuildReachableSamples(
            BlockyCircusGroundsSandboxSettings settings,
            float[] heights,
            bool[] walkableSamples,
            bool[] rampSamples,
            int width,
            int depth,
            Vector2Int center)
        {
            var reachable = new bool[walkableSamples.Length];
            if (!TryIndex(width, depth, center.x, center.y, out int startIndex) || !walkableSamples[startIndex])
            {
                return reachable;
            }

            var queue = new Queue<Vector2Int>();
            reachable[startIndex] = true;
            queue.Enqueue(center);
            float maxStepHeight = Mathf.Tan(settings.PlayerSlopeLimitDegrees * Mathf.Deg2Rad) * settings.GridSpacing + 0.1f;
            var directions = new[]
            {
                Vector2Int.up,
                Vector2Int.right,
                Vector2Int.down,
                Vector2Int.left
            };

            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();
                int currentIndex = current.y * width + current.x;
                for (int i = 0; i < directions.Length; i++)
                {
                    Vector2Int next = current + directions[i];
                    if (!TryIndex(width, depth, next.x, next.y, out int nextIndex)
                        || reachable[nextIndex]
                        || !walkableSamples[nextIndex])
                    {
                        continue;
                    }

                    float heightDelta = Mathf.Abs(heights[nextIndex] - heights[currentIndex]);
                    bool connectedByRamp = rampSamples[currentIndex] || rampSamples[nextIndex];
                    if (!connectedByRamp && heightDelta > maxStepHeight)
                    {
                        continue;
                    }

                    reachable[nextIndex] = true;
                    queue.Enqueue(next);
                }
            }

            return reachable;
        }

        private static void RebuildSampleMetrics(
            BlockyCircusGroundsSandboxSettings settings,
            float[] heights,
            bool[] rampSamples,
            int width,
            int depth,
            Vector3[] normals,
            float[] slopes,
            bool[] walkableSamples)
        {
            for (int z = 0; z < depth; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = z * width + x;
                    bool inside = InsidePlayable(settings, width, depth, x, z);
                    Vector3 normal = NormalAt(heights, width, depth, x, z, settings.GridSpacing);
                    float slope = Vector3.Angle(normal, Vector3.up);
                    normals[index] = normal;
                    slopes[index] = slope;
                    walkableSamples[index] = inside && (rampSamples[index] || slope <= settings.PlayerSlopeLimitDegrees + 0.01f);
                }
            }
        }

        private static int FillIsolatedTrapSamples(
            BlockyCircusGroundsSandboxSettings settings,
            float[] heights,
            bool[] flatSamples,
            bool[] rewardSamples,
            bool[] rampSamples,
            TerrainReservation reservation,
            bool[] walkableSamples,
            bool[] reachableSamples,
            int width,
            int depth)
        {
            float readableStepHeight = Mathf.Tan(settings.ReadableRampSlopeLimitDegrees * Mathf.Deg2Rad) * settings.GridSpacing + 0.1f;
            float trapWallDelta = settings.ConstructedStep * 0.75f;
            int filled = 0;
            Vector2Int[] directions = CardinalDirections();

            for (int z = 1; z < depth - 1; z++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    int index = z * width + x;
                    if (!walkableSamples[index]
                        || !reachableSamples[index]
                        || rampSamples[index]
                        || !InsidePlayable(settings, width, depth, x, z))
                    {
                        continue;
                    }

                    int passableExits = 0;
                    int highWalls = 0;
                    float fillHeight = float.MaxValue;
                    float height = heights[index];
                    for (int i = 0; i < directions.Length; i++)
                    {
                        Vector2Int next = new Vector2Int(x, z) + directions[i];
                        if (!TryIndex(width, depth, next.x, next.y, out int nextIndex)
                            || !walkableSamples[nextIndex]
                            || !reachableSamples[nextIndex])
                        {
                            highWalls++;
                            continue;
                        }

                        float delta = heights[nextIndex] - height;
                        if (rampSamples[nextIndex] || Mathf.Abs(delta) <= readableStepHeight)
                        {
                            passableExits++;
                        }

                        if (delta > trapWallDelta)
                        {
                            highWalls++;
                            fillHeight = Mathf.Min(fillHeight, heights[nextIndex]);
                        }
                    }

                    if (passableExits > 0 || highWalls < 4 || fillHeight == float.MaxValue)
                    {
                        continue;
                    }

                    heights[index] = Mathf.Max(heights[index], fillHeight);
                    flatSamples[index] = true;
                    rewardSamples[index] = false;
                    rampSamples[index] = false;
                    reservation.TryMarkPlatform(new Vector2Int(x, z));
                    filled++;
                }
            }

            return filled;
        }

        private static int CountTrapSamples(
            BlockyCircusGroundsSandboxSettings settings,
            float[] heights,
            bool[] walkableSamples,
            bool[] reachableSamples,
            bool[] rampSamples,
            int width,
            int depth)
        {
            float readableStepHeight = Mathf.Tan(settings.ReadableRampSlopeLimitDegrees * Mathf.Deg2Rad) * settings.GridSpacing + 0.1f;
            float trapWallDelta = settings.ConstructedStep * 0.75f;
            int trapSamples = 0;
            var directions = new[]
            {
                Vector2Int.up,
                Vector2Int.right,
                Vector2Int.down,
                Vector2Int.left
            };

            for (int z = 1; z < depth - 1; z++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    if (!TryIndex(width, depth, x, z, out int index)
                        || !walkableSamples[index]
                        || !reachableSamples[index]
                        || rampSamples[index]
                        || !InsidePlayable(settings, width, depth, x, z))
                    {
                        continue;
                    }

                    int passableExits = 0;
                    int highWalls = 0;
                    float height = heights[index];
                    for (int i = 0; i < directions.Length; i++)
                    {
                        Vector2Int next = new Vector2Int(x, z) + directions[i];
                        if (!TryIndex(width, depth, next.x, next.y, out int nextIndex)
                            || !walkableSamples[nextIndex]
                            || !reachableSamples[nextIndex])
                        {
                            highWalls++;
                            continue;
                        }

                        float delta = heights[nextIndex] - height;
                        if (rampSamples[nextIndex] || Mathf.Abs(delta) <= readableStepHeight)
                        {
                            passableExits++;
                        }

                        if (delta > trapWallDelta)
                        {
                            highWalls++;
                        }
                    }

                    if (passableExits == 0 && highWalls >= 4)
                    {
                        trapSamples++;
                    }
                }
            }

            return trapSamples;
        }

        private static void RejectUnreachableRewardSamples(bool[] rewardSamples, bool[] walkableSamples, bool[] reachableSamples)
        {
            for (int i = 0; i < rewardSamples.Length; i++)
            {
                if (rewardSamples[i] && (!walkableSamples[i] || !reachableSamples[i]))
                {
                    rewardSamples[i] = false;
                }
            }
        }

        private static int CountOuterSectorCoverage(BlockyCircusGroundsSandboxSettings settings, IReadOnlyList<BlockyCircusGroundsPiece> pieces)
        {
            var covered = new bool[OuterSectorCount];
            for (int i = 0; i < pieces.Count; i++)
            {
                BlockyCircusGroundsPiece piece = pieces[i];
                if (piece.Kind != BlockyCircusGroundsPieceKind.RaisedStage)
                {
                    continue;
                }

                Vector2 position = new Vector2(piece.Center.x, piece.Center.z);
                float radiusFraction = position.magnitude / Mathf.Max(1f, settings.PlayableRadius);
                if (radiusFraction < 0.68f)
                {
                    continue;
                }

                float angle = Mathf.Atan2(position.y, position.x);
                if (angle < 0f)
                {
                    angle += Mathf.PI * 2f;
                }

                int sector = Mathf.Clamp(Mathf.FloorToInt(angle / (Mathf.PI * 2f) * OuterSectorCount), 0, OuterSectorCount - 1);
                covered[sector] = true;
            }

            int count = 0;
            for (int i = 0; i < covered.Length; i++)
            {
                if (covered[i])
                {
                    count++;
                }
            }

            return count;
        }

        private static float HeightAt(float[] heights, int width, int depth, Vector2Int sample)
        {
            return TryIndex(width, depth, sample.x, sample.y, out int index) ? heights[index] : 0f;
        }

        private static Vector2Int GridForWorld(BlockyCircusGroundsSandboxSettings settings, int width, int depth, Vector2 position)
        {
            int x = Mathf.RoundToInt((position.x + (width - 1) * settings.GridSpacing * 0.5f) / settings.GridSpacing);
            int z = Mathf.RoundToInt((position.y + (depth - 1) * settings.GridSpacing * 0.5f) / settings.GridSpacing);
            return new Vector2Int(
                Mathf.Clamp(x, 4, width - 5),
                Mathf.Clamp(z, 4, depth - 5));
        }

        private static float QuantizeUp(float value, float step)
        {
            return Mathf.Ceil(value / Mathf.Max(0.001f, step)) * step;
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

        private static bool InsidePlayable(BlockyCircusGroundsSandboxSettings settings, int width, int depth, int x, int z)
        {
            Vector3 position = PositionFor(settings, width, depth, x, z, 0f);
            return new Vector2(position.x, position.z).magnitude <= settings.PlayableRadius + settings.GridSpacing;
        }

        private static float CenterScore(BlockyCircusGroundsSandboxSettings settings, int width, int depth, int x, int z)
        {
            Vector3 position = PositionFor(settings, width, depth, x, z, 0f);
            return 1f - Mathf.Clamp01(new Vector2(position.x, position.z).magnitude / Mathf.Max(1f, settings.PlayableRadius));
        }

        private static float BoundaryScore(BlockyCircusGroundsSandboxSettings settings, int width, int depth, int x, int z)
        {
            Vector3 position = PositionFor(settings, width, depth, x, z, 0f);
            float distance = new Vector2(position.x, position.z).magnitude;
            return Mathf.Clamp01((settings.PlayableRadius - distance) / Mathf.Max(1f, settings.PlayableRadius));
        }

        private static Vector3 PositionFor(BlockyCircusGroundsSandboxSettings settings, int width, int depth, int x, int z, float height)
        {
            float halfX = (width - 1) * settings.GridSpacing * 0.5f;
            float halfZ = (depth - 1) * settings.GridSpacing * 0.5f;
            return new Vector3(x * settings.GridSpacing - halfX, height, z * settings.GridSpacing - halfZ);
        }

        private static bool TryIndex(int width, int depth, int x, int z, out int index)
        {
            if (x < 0 || z < 0 || x >= width || z >= depth)
            {
                index = -1;
                return false;
            }

            index = z * width + x;
            return true;
        }

        private enum TerrainReservationKind
        {
            Empty,
            Platform,
            RampCore,
            RampShoulder
        }

        private readonly struct ConstructedPlacement
        {
            public ConstructedPlacement(float angle, float radialFraction)
            {
                Angle = angle;
                RadialFraction = Mathf.Clamp01(radialFraction);
            }

            public float Angle { get; }
            public float RadialFraction { get; }
        }

        private sealed class TerrainReservation
        {
            private readonly int width;
            private readonly int depth;
            private readonly TerrainReservationKind[] reservations;
            private readonly int[] rampIds;
            private readonly int[] rampAlongs;
            private readonly int[] rampHalfWidths;
            private readonly Dictionary<int, int> rampRowCutCounts = new();
            private int nextRampId;

            public TerrainReservation(int width, int depth)
            {
                this.width = width;
                this.depth = depth;
                reservations = new TerrainReservationKind[Mathf.Max(0, width * depth)];
                rampIds = new int[reservations.Length];
                rampAlongs = new int[reservations.Length];
                rampHalfWidths = new int[reservations.Length];
            }

            public TerrainReservationKind KindAt(Vector2Int sample)
            {
                return TryLocalIndex(sample.x, sample.y, out int index)
                    ? reservations[index]
                    : TerrainReservationKind.Platform;
            }

            public bool TryMarkPlatform(Vector2Int sample)
            {
                if (!TryLocalIndex(sample.x, sample.y, out int index))
                {
                    return false;
                }

                TerrainReservationKind kind = reservations[index];
                if (kind == TerrainReservationKind.RampCore)
                {
                    return false;
                }

                if (kind == TerrainReservationKind.RampShoulder)
                {
                    int totalWidth = rampHalfWidths[index] * 2 + 1;
                    int maxCutWidth = Mathf.Max(0, totalWidth / 2);
                    int key = RampRowKey(index);
                    rampRowCutCounts.TryGetValue(key, out int cutCount);
                    if (cutCount >= maxCutWidth)
                    {
                        return false;
                    }

                    rampRowCutCounts[key] = cutCount + 1;
                }

                reservations[index] = TerrainReservationKind.Platform;
                rampIds[index] = 0;
                rampAlongs[index] = 0;
                rampHalfWidths[index] = 0;
                return true;
            }

            public void MarkRamp(Vector2Int sample, int along, int across, int halfWidth)
            {
                if (TryLocalIndex(sample.x, sample.y, out int index))
                {
                    if (across == -halfWidth)
                    {
                        nextRampId++;
                    }

                    reservations[index] = across == 0 ? TerrainReservationKind.RampCore : TerrainReservationKind.RampShoulder;
                    rampIds[index] = nextRampId;
                    rampAlongs[index] = Mathf.Max(0, along);
                    rampHalfWidths[index] = Mathf.Max(0, halfWidth);
                }
            }

            private int RampRowKey(int index)
            {
                return rampIds[index] * 10000 + rampAlongs[index];
            }

            private bool TryLocalIndex(int x, int z, out int index)
            {
                if (x < 0 || z < 0 || x >= width || z >= depth)
                {
                    index = -1;
                    return false;
                }

                index = z * width + x;
                return true;
            }
        }

        private sealed class BasinRegion
        {
            private readonly HashSet<Vector2Int> sampleSet = new();

            public BasinRegion()
            {
                MinX = int.MaxValue;
                MinZ = int.MaxValue;
                MaxX = int.MinValue;
                MaxZ = int.MinValue;
            }

            public List<Vector2Int> Samples { get; } = new();
            public List<BasinBoundaryEdge> HighEdges { get; } = new();
            public int MinX { get; private set; }
            public int MinZ { get; private set; }
            public int MaxX { get; private set; }
            public int MaxZ { get; private set; }

            public void Add(Vector2Int sample)
            {
                Samples.Add(sample);
                sampleSet.Add(sample);
                MinX = Mathf.Min(MinX, sample.x);
                MinZ = Mathf.Min(MinZ, sample.y);
                MaxX = Mathf.Max(MaxX, sample.x);
                MaxZ = Mathf.Max(MaxZ, sample.y);
            }

            public bool Contains(Vector2Int sample)
            {
                return sampleSet.Contains(sample);
            }

            public void AddHighEdge(BasinBoundaryEdge edge)
            {
                HighEdges.Add(edge);
            }
        }

        private readonly struct BasinBoundaryEdge
        {
            public BasinBoundaryEdge(Vector2Int inside, Vector2Int outside, Vector2Int direction, float insideHeight, float outsideHeight)
            {
                Inside = inside;
                Outside = outside;
                Direction = direction;
                InsideHeight = insideHeight;
                OutsideHeight = outsideHeight;
            }

            public Vector2Int Inside { get; }
            public Vector2Int Outside { get; }
            public Vector2Int Direction { get; }
            public float InsideHeight { get; }
            public float OutsideHeight { get; }
            public float HeightDelta => OutsideHeight - InsideHeight;
        }

        private readonly struct RampCandidate
        {
            public RampCandidate(
                Vector2Int foot,
                Vector2Int lip,
                Vector2Int direction,
                int length,
                int halfWidth,
                Vector3 center,
                Vector3 size,
                float score)
            {
                Foot = foot;
                Lip = lip;
                Direction = direction;
                Length = Mathf.Max(1, length);
                HalfWidth = Mathf.Max(0, halfWidth);
                Center = center;
                Size = size;
                Score = score;
                Valid = true;
            }

            public Vector2Int Foot { get; }
            public Vector2Int Lip { get; }
            public Vector2Int Direction { get; }
            public int Length { get; }
            public int HalfWidth { get; }
            public Vector3 Center { get; }
            public Vector3 Size { get; }
            public float Score { get; }
            public bool Valid { get; }
        }

        private readonly struct BaseLandformStamp
        {
            private readonly Vector2 center;
            private readonly float radiusX;
            private readonly float radiusZ;
            private readonly float cos;
            private readonly float sin;
            private readonly float amplitude;

            public BaseLandformStamp(Vector2 center, float radiusX, float radiusZ, float rotation, float amplitude)
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
                float localX = delta.x * cos + delta.y * sin;
                float localZ = -delta.x * sin + delta.y * cos;
                float distance = Mathf.Sqrt(
                    localX * localX / (radiusX * radiusX)
                    + localZ * localZ / (radiusZ * radiusZ));
                float t = Mathf.Clamp01(1f - distance);
                t = t * t * (3f - 2f * t);
                return t * amplitude;
            }
        }
    }
}
