using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TheCircussyOne.Config;
using TheCircussyOne.Runtime;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TheCircussyOne.Tests
{
    public sealed class BlockyCircusGroundsSandboxTests
    {
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public void BlockyCircusGroundsGrammarBuildsRequiredSandboxPieces(int seed)
        {
            BlockyCircusGroundsSandboxResult result = BlockyCircusGroundsGrammar.Generate(
                BlockyCircusGroundsSandboxSettings.Default(seed));

            Assert.That(result.Map, Is.Not.Null);
            Assert.That(result.Map.Width, Is.EqualTo(129));
            Assert.That(result.Map.Depth, Is.EqualTo(129));
            Assert.That(result.Map.GridSpacing, Is.EqualTo(4f).Within(0.001f));
            Assert.That(result.Map.PlayableRadius, Is.EqualTo(256f).Within(0.001f));
            Assert.That(result.Map.MaxHeight, Is.EqualTo(32f).Within(0.001f));
            Assert.That(BlockyCircusGroundsSandboxSettings.Default(seed).ConstructedStep, Is.EqualTo(4f).Within(0.001f));
            Assert.That(result.Map.CountMask(GeneratedWorldMask.PlayerStartSafe), Is.EqualTo(1));
            Assert.That(result.Map.PlayerStartPosition.x, Is.EqualTo(0f).Within(0.001f));
            Assert.That(result.Map.PlayerStartPosition.z, Is.EqualTo(0f).Within(0.001f));
            Assert.That(result.Map.PlayerStartPosition.y, Is.InRange(0f, result.Map.MaxHeight));

            Assert.That(result.Pieces.Count(piece => piece.Kind == BlockyCircusGroundsPieceKind.RaisedStage), Is.GreaterThanOrEqualTo(80));
            Assert.That(result.Pieces.Count(piece => piece.Kind == BlockyCircusGroundsPieceKind.RampAisle), Is.GreaterThanOrEqualTo(18));
            Assert.That(result.Pieces.Count(piece => piece.Kind == BlockyCircusGroundsPieceKind.RewardPocket), Is.GreaterThanOrEqualTo(6));
            Assert.That(result.Pieces.Count(piece => piece.Kind == BlockyCircusGroundsPieceKind.TreeGrove), Is.EqualTo(1));
            Assert.That(result.Validation.AcceptedRampCount, Is.EqualTo(result.Pieces.Count(piece => piece.Kind == BlockyCircusGroundsPieceKind.RampAisle)));
            Assert.That(result.Validation.RejectedRampCount, Is.GreaterThan(0));
            Assert.That(result.Validation.RejectedRampUnsupportedCount, Is.GreaterThan(0));
            Assert.That(result.Validation.RewardPlacementValid, Is.True);
            Assert.That(result.Validation.PitTrapValid, Is.True);
            Assert.That(result.Validation.OuterSectorCoverageValid, Is.True);

            RunWorldGenerationConfig config = RunWorldGenerationConfig.CreateRuntimeDefault();
            Assert.That(config.runtimeMeshTerrainEnabled, Is.True);
            Assert.That(config.defaultLayoutMode, Is.EqualTo(RunWorldLayoutMode.Generated));
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public void BlockyCircusGroundsGrammarKeepsRampCardinalAndWithinSlopeLimit(int seed)
        {
            BlockyCircusGroundsSandboxSettings settings = BlockyCircusGroundsSandboxSettings.Default(seed);
            BlockyCircusGroundsSandboxResult result = BlockyCircusGroundsGrammar.Generate(settings);

            BlockyCircusGroundsPiece[] rampPieces = result.Pieces
                .Where(piece => piece.Kind == BlockyCircusGroundsPieceKind.RampAisle)
                .ToArray();

            Assert.That(rampPieces, Is.Not.Empty);
            for (int i = 0; i < rampPieces.Length; i++)
            {
                BlockyCircusGroundsPiece piece = rampPieces[i];
                float slopeDegrees = Mathf.Atan2(piece.Size.y, Mathf.Max(0.001f, piece.Size.z)) * Mathf.Rad2Deg;
                Assert.That(piece.Ramp, Is.True);
                Assert.That(slopeDegrees, Is.LessThanOrEqualTo(settings.ReadableRampSlopeLimitDegrees + 0.01f));
                Assert.That(IsCardinal(piece.Direction), Is.True, piece.Direction.ToString());
            }
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public void BlockyCircusGroundsGrammarKeepsRampsOutOfHeightfieldSamples(int seed)
        {
            BlockyCircusGroundsSandboxResult result = BlockyCircusGroundsGrammar.Generate(
                BlockyCircusGroundsSandboxSettings.Default(seed));

            Assert.That(result.Pieces.Any(piece => piece.Kind == BlockyCircusGroundsPieceKind.RampAisle), Is.True);
            Assert.That(result.Map.Samples.Any(sample => sample.IsRamp), Is.False);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public void BlockyCircusGroundsGrammarMarksFlatRewardPocket(int seed)
        {
            BlockyCircusGroundsSandboxResult result = BlockyCircusGroundsGrammar.Generate(
                BlockyCircusGroundsSandboxSettings.Default(seed));

            GeneratedWorldSample[] rewardSamples = result.Map.Samples
                .Where(sample => sample.HasMask(GeneratedWorldMask.RewardSafe))
                .ToArray();

            Assert.That(rewardSamples, Is.Not.Empty);
            for (int i = 0; i < rewardSamples.Length; i++)
            {
                Assert.That(rewardSamples[i].IsRamp, Is.False);
                Assert.That(rewardSamples[i].SlopeDegrees, Is.LessThanOrEqualTo(2f));
                Assert.That(rewardSamples[i].HasMask(GeneratedWorldMask.Reachable), Is.True);
            }

            Assert.That(result.Validation.UnreachableRewardSampleCount, Is.EqualTo(0));
            Assert.That(result.Validation.UnreachableWalkableSampleCount, Is.LessThan(result.Map.Samples.Count / 3));
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public void BlockyCircusGroundsGrammarKeepsRampFootprintsSeparatedAndInsideArena(int seed)
        {
            BlockyCircusGroundsSandboxResult result = BlockyCircusGroundsGrammar.Generate(
                BlockyCircusGroundsSandboxSettings.Default(seed));

            var occupied = new HashSet<Vector2Int>();
            foreach (BlockyCircusGroundsPiece ramp in result.Pieces.Where(piece => piece.Kind == BlockyCircusGroundsPieceKind.RampAisle))
            {
                Vector2Int direction = DirectionFor(ramp.Direction);
                Vector2Int side = new Vector2Int(-direction.y, direction.x);
                int length = ramp.FootprintLength;
                int halfWidth = ramp.FootprintHalfWidth;
                Vector2Int foot = ramp.FootprintStart;
                Assert.That(ramp.HasFootprint, Is.True);

                for (int along = 0; along <= length; along++)
                {
                    for (int across = -halfWidth; across <= halfWidth; across++)
                    {
                        Vector2Int sample = foot + direction * along + side * across;
                        Vector3 position = result.Map.PositionFor(sample.x, sample.y);
                        Assert.That(new Vector2(position.x, position.z).magnitude, Is.LessThanOrEqualTo(result.Map.PlayableRadius + result.Map.GridSpacing));
                        Assert.That(occupied.Add(sample), Is.True, $"Ramp overlap at {sample} for seed {seed}");
                    }
                }
            }
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public void BlockyCircusGroundsGrammarRejectsUnsupportedFloatingRamps(int seed)
        {
            BlockyCircusGroundsSandboxSettings settings = BlockyCircusGroundsSandboxSettings.Default(seed);
            BlockyCircusGroundsSandboxResult result = BlockyCircusGroundsGrammar.Generate(settings);
            float unsupportedGapLimit = settings.GridSpacing * 0.35f;

            foreach (BlockyCircusGroundsPiece ramp in result.Pieces.Where(piece => piece.Kind == BlockyCircusGroundsPieceKind.RampAisle))
            {
                Vector2Int direction = DirectionFor(ramp.Direction);
                Vector2Int side = new Vector2Int(-direction.y, direction.x);
                Assert.That(result.Map.TryGetSample(ramp.FootprintStart.x, ramp.FootprintStart.y, out GeneratedWorldSample footSample), Is.True);

                for (int along = 0; along <= ramp.FootprintLength; along++)
                {
                    for (int across = -ramp.FootprintHalfWidth; across <= ramp.FootprintHalfWidth; across++)
                    {
                        Vector2Int sample = ramp.FootprintStart + direction * along + side * across;
                        Assert.That(result.Map.TryGetSample(sample.x, sample.y, out GeneratedWorldSample terrainSample), Is.True);
                        Assert.That(footSample.Height - terrainSample.Height, Is.LessThanOrEqualTo(unsupportedGapLimit + 0.001f));
                    }
                }
            }
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public void BlockyCircusGroundsGrammarKeepsRampCenterlinesAndHalfWidthVisible(int seed)
        {
            BlockyCircusGroundsSandboxSettings settings = BlockyCircusGroundsSandboxSettings.Default(seed);
            BlockyCircusGroundsSandboxResult result = BlockyCircusGroundsGrammar.Generate(settings);
            float cutHeightTolerance = settings.GridSpacing * 0.35f + 0.001f;

            foreach (BlockyCircusGroundsPiece ramp in result.Pieces.Where(piece => piece.Kind == BlockyCircusGroundsPieceKind.RampAisle))
            {
                Vector2Int direction = DirectionFor(ramp.Direction);
                Vector2Int side = new Vector2Int(-direction.y, direction.x);
                Assert.That(result.Map.TryGetSample(ramp.FootprintStart.x, ramp.FootprintStart.y, out GeneratedWorldSample footSample), Is.True);

                for (int along = 0; along <= ramp.FootprintLength; along++)
                {
                    float t = along / Mathf.Max(1f, ramp.FootprintLength);
                    float rampHeight = footSample.Height + ramp.Size.y * t;
                    int cutWidth = 0;
                    int totalWidth = ramp.FootprintHalfWidth * 2 + 1;
                    int maxCutWidth = totalWidth / 2;

                    for (int across = -ramp.FootprintHalfWidth; across <= ramp.FootprintHalfWidth; across++)
                    {
                        Vector2Int sample = ramp.FootprintStart + direction * along + side * across;
                        Assert.That(result.Map.TryGetSample(sample.x, sample.y, out GeneratedWorldSample terrainSample), Is.True);
                        bool cutByRaisedTerrain = terrainSample.Height > rampHeight + cutHeightTolerance;

                        if (across == 0)
                        {
                            Assert.That(cutByRaisedTerrain, Is.False, $"Ramp centerline cut at seed {seed}, along {along}");
                        }

                        if (cutByRaisedTerrain)
                        {
                            cutWidth++;
                        }
                    }

                    Assert.That(cutWidth, Is.LessThanOrEqualTo(maxCutWidth), $"Ramp width cut too far at seed {seed}, along {along}");
                }
            }
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public void BlockyCircusGroundsGrammarKeepsOuterSectorsCoveredAndAvoidsTrapSamples(int seed)
        {
            BlockyCircusGroundsSandboxResult result = BlockyCircusGroundsGrammar.Generate(
                BlockyCircusGroundsSandboxSettings.Default(seed));

            Assert.That(result.Validation.OuterSectorCoverage, Is.EqualTo(result.Validation.OuterSectorCount));
            Assert.That(result.Validation.TrapSampleCount, Is.EqualTo(0));
            Assert.That(result.Validation.UnrepairedBasinCount, Is.EqualTo(0));
        }

        [Test]
        public void BlockyCircusGroundsGrammarFillsGroundLevelTerraceBasin()
        {
            BlockyCircusGroundsSandboxSettings settings = BasinRepairSettings();
            int width = settings.SampleWidth;
            int depth = settings.SampleDepth;
            var heights = new float[width * depth];
            var flatSamples = new bool[width * depth];
            var rewardSamples = new bool[width * depth];
            var rampSamples = new bool[width * depth];

            for (int i = 0; i < flatSamples.Length; i++)
            {
                flatSamples[i] = true;
            }

            for (int z = 8; z <= 16; z++)
            {
                for (int x = 8; x <= 16; x++)
                {
                    rewardSamples[z * width + x] = true;
                }
            }

            for (int z = 7; z <= 17; z++)
            {
                for (int x = 7; x <= 17; x++)
                {
                    bool wall = x == 7 || x == 17 || z == 7 || z == 17;
                    if (wall)
                    {
                        heights[z * width + x] = 4f;
                    }
                }
            }

            Assert.That(BlockyCircusGroundsGrammar.CountEnclosedBasinsForTests(settings, heights, rampSamples, width, depth), Is.GreaterThan(0));

            BlockyCircusGroundsBasinRepairResult repair = BlockyCircusGroundsGrammar.RepairEnclosedBasinsForTests(
                settings,
                heights,
                flatSamples,
                rewardSamples,
                rampSamples,
                width,
                depth,
                out IReadOnlyList<BlockyCircusGroundsPiece> pieces);

            Assert.That(repair.DetectedBasinCount, Is.GreaterThan(0));
            Assert.That(repair.FilledBasinCount, Is.GreaterThan(0));
            Assert.That(repair.RampRepairedCount, Is.EqualTo(0));
            Assert.That(repair.UnrepairedBasinCount, Is.EqualTo(0));
            Assert.That(rampSamples.Any(sample => sample), Is.False);
            Assert.That(pieces.Any(piece => piece.Kind == BlockyCircusGroundsPieceKind.RaisedStage), Is.True);
            for (int z = 8; z <= 16; z++)
            {
                for (int x = 8; x <= 16; x++)
                {
                    int index = z * width + x;
                    Assert.That(heights[index], Is.EqualTo(4f).Within(0.001f));
                    Assert.That(flatSamples[index], Is.True);
                    Assert.That(rewardSamples[index], Is.False);
                    Assert.That(rampSamples[index], Is.False);
                }
            }

            Assert.That(BlockyCircusGroundsGrammar.CountEnclosedBasinsForTests(settings, heights, rampSamples, width, depth), Is.EqualTo(0));
        }

        [Test]
        public void BlockyCircusGroundsGrammarFillsElevatedTerraceBasinToLowestRim()
        {
            BlockyCircusGroundsSandboxSettings settings = BasinRepairSettings();
            int width = settings.SampleWidth;
            int depth = settings.SampleDepth;
            var heights = Enumerable.Repeat(8f, width * depth).ToArray();
            var flatSamples = new bool[width * depth];
            var rewardSamples = new bool[width * depth];
            var rampSamples = new bool[width * depth];

            for (int i = 0; i < flatSamples.Length; i++)
            {
                flatSamples[i] = true;
            }

            for (int z = 8; z <= 16; z++)
            {
                for (int x = 8; x <= 16; x++)
                {
                    int index = z * width + x;
                    heights[index] = 4f;
                    rewardSamples[index] = true;
                }
            }

            Assert.That(BlockyCircusGroundsGrammar.CountEnclosedBasinsForTests(settings, heights, rampSamples, width, depth), Is.GreaterThan(0));

            BlockyCircusGroundsBasinRepairResult repair = BlockyCircusGroundsGrammar.RepairEnclosedBasinsForTests(
                settings,
                heights,
                flatSamples,
                rewardSamples,
                rampSamples,
                width,
                depth,
                out IReadOnlyList<BlockyCircusGroundsPiece> pieces);

            Assert.That(repair.DetectedBasinCount, Is.GreaterThan(0));
            Assert.That(repair.FilledBasinCount, Is.GreaterThan(0));
            Assert.That(repair.RampRepairedCount, Is.EqualTo(0));
            Assert.That(repair.UnrepairedBasinCount, Is.EqualTo(0));
            Assert.That(rampSamples.Any(sample => sample), Is.False);
            Assert.That(pieces.Any(piece => piece.Kind == BlockyCircusGroundsPieceKind.RaisedStage), Is.True);
            for (int z = 8; z <= 16; z++)
            {
                for (int x = 8; x <= 16; x++)
                {
                    int index = z * width + x;
                    Assert.That(heights[index], Is.EqualTo(8f).Within(0.001f));
                    Assert.That(flatSamples[index], Is.True);
                    Assert.That(rewardSamples[index], Is.False);
                    Assert.That(rampSamples[index], Is.False);
                }
            }

            Assert.That(BlockyCircusGroundsGrammar.CountEnclosedBasinsForTests(settings, heights, rampSamples, width, depth), Is.EqualTo(0));
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public void BlockyCircusGroundsGrammarRepairsRegionalBasinsInReviewSeeds(int seed)
        {
            BlockyCircusGroundsSandboxResult result = BlockyCircusGroundsGrammar.Generate(
                BlockyCircusGroundsSandboxSettings.Default(seed));

            Assert.That(result.Validation.UnrepairedBasinCount, Is.EqualTo(0));
            Assert.That(result.Validation.PitTrapValid, Is.True);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public void BlockyCircusGroundsGrammarKeepsVisibleOrganicBaseVariationUnderBlocks(int seed)
        {
            BlockyCircusGroundsSandboxResult result = BlockyCircusGroundsGrammar.Generate(
                BlockyCircusGroundsSandboxSettings.Default(seed));

            GeneratedWorldSample[] baseSamples = result.Map.Samples
                .Where(sample => sample.SurfaceKind == GeneratedWorldSurfaceKind.Continuous
                    && !sample.HasMask(GeneratedWorldMask.RewardSafe)
                    && sample.HasMask(GeneratedWorldMask.Walkable)
                    && sample.Height > 0.25f
                    && sample.Height < result.Map.MaxHeight - 0.25f)
                .ToArray();

            int distinctRoundedHeights = baseSamples
                .Select(sample => Mathf.RoundToInt(sample.Height))
                .Distinct()
                .Count();

            Assert.That(baseSamples.Length, Is.GreaterThan(250));
            Assert.That(distinctRoundedHeights, Is.GreaterThanOrEqualTo(18));
            Assert.That(baseSamples.Max(sample => sample.Height), Is.GreaterThan(result.Map.MaxHeight * 0.72f));
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public void BlockyCircusGroundsGrammarUsesOuterRingForConstructedStages(int seed)
        {
            BlockyCircusGroundsSandboxResult result = BlockyCircusGroundsGrammar.Generate(
                BlockyCircusGroundsSandboxSettings.Default(seed));

            BlockyCircusGroundsPiece[] stages = result.Pieces
                .Where(piece => piece.Kind == BlockyCircusGroundsPieceKind.RaisedStage)
                .ToArray();

            int inner = stages.Count(piece => RadiusFraction(result.Map, piece.Center) < 0.4f);
            int middle = stages.Count(piece =>
            {
                float fraction = RadiusFraction(result.Map, piece.Center);
                return fraction >= 0.4f && fraction < 0.7f;
            });
            int outer = stages.Count(piece => RadiusFraction(result.Map, piece.Center) >= 0.7f);

            Assert.That(stages.Length, Is.GreaterThanOrEqualTo(80));
            Assert.That(inner, Is.GreaterThan(8), seed.ToString());
            Assert.That(middle, Is.GreaterThan(18), seed.ToString());
            Assert.That(outer, Is.GreaterThan(30), seed.ToString());
            Assert.That(stages.Max(piece => RadiusFraction(result.Map, piece.Center)), Is.GreaterThan(0.94f));
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public void BlockyCircusGroundsGrammarKeepsOuterTerrainVaried(int seed)
        {
            BlockyCircusGroundsSandboxResult result = BlockyCircusGroundsGrammar.Generate(
                BlockyCircusGroundsSandboxSettings.Default(seed));

            GeneratedWorldSample[] outerSamples = result.Map.Samples
                .Where((sample, index) =>
                {
                    Vector3 position = result.Map.PositionFor(index % result.Map.Width, index / result.Map.Width);
                    float radiusFraction = new Vector2(position.x, position.z).magnitude / result.Map.PlayableRadius;
                    return radiusFraction >= 0.78f
                        && radiusFraction <= 0.96f
                        && sample.SurfaceKind == GeneratedWorldSurfaceKind.Continuous
                        && sample.Height > 0.25f;
                })
                .ToArray();

            int distinctRoundedHeights = outerSamples
                .Select(sample => Mathf.RoundToInt(sample.Height))
                .Distinct()
                .Count();

            Assert.That(outerSamples.Length, Is.GreaterThan(120));
            Assert.That(distinctRoundedHeights, Is.GreaterThanOrEqualTo(10));
            Assert.That(outerSamples.Max(sample => sample.Height), Is.GreaterThan(result.Map.MaxHeight * 0.25f));
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public void BlockyCircusGroundsGrammarPutsTallerBlocksOnHigherTerrain(int seed)
        {
            BlockyCircusGroundsSandboxResult result = BlockyCircusGroundsGrammar.Generate(
                BlockyCircusGroundsSandboxSettings.Default(seed));

            BlockyCircusGroundsPiece[] stages = result.Pieces
                .Where(piece => piece.Kind == BlockyCircusGroundsPieceKind.RaisedStage)
                .OrderBy(piece => piece.Center.y)
                .ToArray();
            int bucketSize = Mathf.Max(1, stages.Length / 4);
            float lowAverage = stages.Take(bucketSize).Average(piece => piece.Center.y);
            float highAverage = stages.Skip(stages.Length - bucketSize).Average(piece => piece.Center.y);

            Assert.That(stages.Max(piece => piece.Center.y), Is.GreaterThan(result.Map.MaxHeight * 0.86f));
            Assert.That(highAverage - lowAverage, Is.GreaterThan(result.Map.MaxHeight * 0.35f));
        }

        [Test]
        public void GeneratedTerrainBuilderPreservesContinuousOrganicCellHeights()
        {
            GeneratedWorldTerrainBuildResult result = GeneratedWorldTerrainBuilder.Build(
                SmallMapWithSurfaceKind(GeneratedWorldSurfaceKind.Continuous),
                new GeneratedWorldTerrainBuildSettings(material: null, chunkQuadsPerSide: 1));

            Mesh mesh = result.Root.GetComponentInChildren<MeshFilter>().sharedMesh;
            Vector3[] vertices = mesh.vertices;

            Assert.That(result.Success, Is.True);
            Assert.That(vertices.Take(4).Select(vertex => vertex.y).Distinct().Count(), Is.GreaterThan(1));

            Object.DestroyImmediate(result.Root);
        }

        [Test]
        public void GeneratedTerrainBuilderSmoothsContinuousOrganicGroundNormals()
        {
            Vector3 smoothNormal = new Vector3(-0.45f, 1f, -0.2f).normalized;
            var samples = new[]
            {
                Sample(0f, GeneratedWorldSurfaceKind.Continuous, smoothNormal),
                Sample(1f, GeneratedWorldSurfaceKind.Continuous, smoothNormal),
                Sample(2f, GeneratedWorldSurfaceKind.Continuous, smoothNormal),
                Sample(3f, GeneratedWorldSurfaceKind.Continuous, smoothNormal)
            };

            GeneratedWorldTerrainBuildResult result = GeneratedWorldTerrainBuilder.Build(
                MapWithSamples(2, 2, samples, wallHeightThreshold: 1.25f),
                new GeneratedWorldTerrainBuildSettings(material: null, chunkQuadsPerSide: 1));

            Mesh mesh = result.Root.GetComponentInChildren<MeshFilter>().sharedMesh;

            Assert.That(result.Success, Is.True);
            Assert.That(mesh.normals.Take(4).All(normal => Vector3.Angle(normal, smoothNormal) < 0.1f), Is.True);

            Object.DestroyImmediate(result.Root);
        }

        [Test]
        public void GeneratedTerrainBuilderFlattensAuthoredBlockCells()
        {
            GeneratedWorldTerrainBuildResult result = GeneratedWorldTerrainBuilder.Build(
                SmallMapWithSurfaceKind(GeneratedWorldSurfaceKind.Flat),
                new GeneratedWorldTerrainBuildSettings(material: null, chunkQuadsPerSide: 1));

            Mesh mesh = result.Root.GetComponentInChildren<MeshFilter>().sharedMesh;
            Vector3[] vertices = mesh.vertices;

            Assert.That(result.Success, Is.True);
            Assert.That(vertices.Take(4).Select(vertex => vertex.y).Distinct().Count(), Is.EqualTo(1));

            Object.DestroyImmediate(result.Root);
        }

        [Test]
        public void GeneratedTerrainBuilderKeepsAuthoredBlockTopNormalsHardEdged()
        {
            Vector3 ignoredSmoothNormal = new Vector3(-0.45f, 1f, -0.2f).normalized;
            var samples = new[]
            {
                Sample(0f, GeneratedWorldSurfaceKind.Flat, ignoredSmoothNormal),
                Sample(1f, GeneratedWorldSurfaceKind.Flat, ignoredSmoothNormal),
                Sample(2f, GeneratedWorldSurfaceKind.Flat, ignoredSmoothNormal),
                Sample(3f, GeneratedWorldSurfaceKind.Flat, ignoredSmoothNormal)
            };

            GeneratedWorldTerrainBuildResult result = GeneratedWorldTerrainBuilder.Build(
                MapWithSamples(2, 2, samples, wallHeightThreshold: 1.25f),
                new GeneratedWorldTerrainBuildSettings(material: null, chunkQuadsPerSide: 1));

            Mesh mesh = result.Root.GetComponentInChildren<MeshFilter>().sharedMesh;

            Assert.That(result.Success, Is.True);
            Assert.That(mesh.normals.Take(4).All(normal => Vector3.Angle(normal, Vector3.up) < 0.1f), Is.True);

            Object.DestroyImmediate(result.Root);
        }

        [Test]
        public void GeneratedTerrainBuilderKeepsMixedRampSideCellsSloped()
        {
            var samples = new[]
            {
                Sample(0f, GeneratedWorldSurfaceKind.Ramp),
                Sample(0f, GeneratedWorldSurfaceKind.Continuous),
                Sample(2f, GeneratedWorldSurfaceKind.Ramp),
                Sample(0f, GeneratedWorldSurfaceKind.Continuous)
            };
            GeneratedWorldTerrainBuildResult result = GeneratedWorldTerrainBuilder.Build(
                MapWithSamples(2, 2, samples, wallHeightThreshold: 1.25f),
                new GeneratedWorldTerrainBuildSettings(material: null, chunkQuadsPerSide: 1));

            Mesh mesh = result.Root.GetComponentInChildren<MeshFilter>().sharedMesh;
            Vector3[] vertices = mesh.vertices;

            Assert.That(result.Success, Is.True);
            Assert.That(vertices.Take(4).Select(vertex => vertex.y).Distinct().Count(), Is.GreaterThan(1));

            Object.DestroyImmediate(result.Root);
        }

        [Test]
        public void GeneratedTerrainBuilderClosesMixedConstructedOrganicCells()
        {
            var samples = new[]
            {
                Sample(3f, GeneratedWorldSurfaceKind.Flat),
                Sample(0f, GeneratedWorldSurfaceKind.Continuous),
                Sample(3f, GeneratedWorldSurfaceKind.Flat),
                Sample(3f, GeneratedWorldSurfaceKind.Flat),
                Sample(2f, GeneratedWorldSurfaceKind.Continuous),
                Sample(3f, GeneratedWorldSurfaceKind.Flat)
            };
            GeneratedWorldTerrainBuildResult result = GeneratedWorldTerrainBuilder.Build(
                MapWithSamples(3, 2, samples, wallHeightThreshold: 1.25f),
                new GeneratedWorldTerrainBuildSettings(material: null, chunkQuadsPerSide: 2));

            Mesh mesh = result.Root.GetComponentInChildren<MeshFilter>().sharedMesh;
            Vector3[] vertices = mesh.vertices;

            Assert.That(result.Success, Is.True);
            Assert.That(mesh.GetTriangles(0).Length, Is.EqualTo(12));
            Assert.That(mesh.GetTriangles(1).Length, Is.GreaterThanOrEqualTo(24));
            Assert.That(vertices.Take(4).Select(vertex => vertex.y).Distinct().Count(), Is.EqualTo(1));

            Object.DestroyImmediate(result.Root);
        }

        [Test]
        public void GeneratedTerrainBuilderAddsWallsForSmallFlatHeightMismatches()
        {
            var samples = new[]
            {
                Sample(0f, GeneratedWorldSurfaceKind.Flat),
                Sample(0f, GeneratedWorldSurfaceKind.Flat),
                Sample(0.4f, GeneratedWorldSurfaceKind.Flat),
                Sample(0f, GeneratedWorldSurfaceKind.Flat),
                Sample(0f, GeneratedWorldSurfaceKind.Flat),
                Sample(0.4f, GeneratedWorldSurfaceKind.Flat)
            };
            GeneratedWorldTerrainBuildResult result = GeneratedWorldTerrainBuilder.Build(
                MapWithSamples(3, 2, samples, wallHeightThreshold: 1.25f),
                new GeneratedWorldTerrainBuildSettings(material: null, chunkQuadsPerSide: 2));

            Mesh mesh = result.Root.GetComponentInChildren<MeshFilter>().sharedMesh;

            Assert.That(result.Success, Is.True);
            Assert.That(mesh.GetTriangles(1).Length, Is.GreaterThan(0));

            Object.DestroyImmediate(result.Root);
        }

        [Test]
        public void GeneratedTerrainBuilderCutsTerrainToPlayableCircle()
        {
            GeneratedWorldMap map = FlatMap(width: 5, depth: 5, playableRadius: 2.2f);

            GeneratedWorldTerrainBuildResult result = GeneratedWorldTerrainBuilder.Build(
                map,
                new GeneratedWorldTerrainBuildSettings(material: null, chunkQuadsPerSide: 4));

            Mesh[] meshes = result.Root.GetComponentsInChildren<MeshFilter>()
                .Select(filter => filter.sharedMesh)
                .ToArray();
            int topTriangleCount = meshes.Sum(mesh =>
                mesh.GetTriangles(0).Length + (mesh.subMeshCount > 2 ? mesh.GetTriangles(2).Length : 0));
            int wallTriangleCount = meshes.Sum(mesh => mesh.GetTriangles(1).Length);
            int fullSquareTopTriangleCount = (map.Width - 1) * (map.Depth - 1) * 6;

            Assert.That(result.Success, Is.True);
            Assert.That(meshes, Is.Not.Empty);
            Assert.That(topTriangleCount, Is.LessThan(fullSquareTopTriangleCount));
            Assert.That(topTriangleCount, Is.GreaterThan(0));
            Assert.That(wallTriangleCount, Is.GreaterThan(0));

            Object.DestroyImmediate(result.Root);
        }

        [Test]
        public void GeneratedTerrainBuilderSmoothBoundaryRimUsesCollisionMesh()
        {
            GeneratedWorldMap map = FlatMap(width: 9, depth: 9, playableRadius: 8f);
            var parent = new GameObject("Boundary Rim Test Root");

            try
            {
                GameObject rim = GeneratedWorldTerrainBuilder.BuildSmoothBoundaryRim(
                    parent.transform,
                    map,
                    material: null,
                    rimWidth: 4f,
                    segmentCount: 64);

                MeshFilter meshFilter = rim.GetComponent<MeshFilter>();
                MeshCollider meshCollider = rim.GetComponent<MeshCollider>();

                Assert.That(rim, Is.Not.Null);
                Assert.That(meshFilter.sharedMesh, Is.Not.Null);
                Assert.That(meshCollider, Is.Not.Null);
                Assert.That(meshCollider.sharedMesh, Is.SameAs(meshFilter.sharedMesh));
            }
            finally
            {
                Object.DestroyImmediate(parent);
            }
        }

        [Test]
        public void BlockyCircusGroundsGrammarProducesDeterministicButSeedVariantLayouts()
        {
            BlockyCircusGroundsSandboxResult first = BlockyCircusGroundsGrammar.Generate(
                BlockyCircusGroundsSandboxSettings.Default(seed: 1));
            BlockyCircusGroundsSandboxResult repeat = BlockyCircusGroundsGrammar.Generate(
                BlockyCircusGroundsSandboxSettings.Default(seed: 1));
            BlockyCircusGroundsSandboxResult variant = BlockyCircusGroundsGrammar.Generate(
                BlockyCircusGroundsSandboxSettings.Default(seed: 2));

            BlockyCircusGroundsPiece firstStage = first.Pieces.First(piece => piece.Kind == BlockyCircusGroundsPieceKind.RaisedStage);
            BlockyCircusGroundsPiece repeatStage = repeat.Pieces.First(piece => piece.Kind == BlockyCircusGroundsPieceKind.RaisedStage);
            BlockyCircusGroundsPiece variantStage = variant.Pieces.First(piece => piece.Kind == BlockyCircusGroundsPieceKind.RaisedStage);

            AssertVector3(repeatStage.Center, firstStage.Center);
            AssertVector2(repeatStage.Direction, firstStage.Direction);
            Assert.That(
                variantStage.Center != firstStage.Center || variantStage.Direction != firstStage.Direction,
                Is.True);
        }

        [Test]
        public void BlockyCircusGroundsSandboxBuildsPreviewMeshWithSeparateWallMaterialForReviewSeeds()
        {
            for (int seed = 1; seed <= 5; seed++)
            {
                BlockyCircusGroundsSandboxResult sandbox = BlockyCircusGroundsGrammar.Generate(
                    BlockyCircusGroundsSandboxSettings.Default(seed));
                Material topMaterial = new(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"))
                {
                    name = $"TCO Sandbox Test Top Material {seed}"
                };

                GeneratedWorldTerrainBuildResult result = GeneratedWorldTerrainBuilder.Build(
                    sandbox.Map,
                    new GeneratedWorldTerrainBuildSettings(topMaterial, chunkQuadsPerSide: 16));

                MeshRenderer renderer = result.Root.GetComponentInChildren<MeshRenderer>();
                Mesh mesh = result.Root.GetComponentInChildren<MeshFilter>().sharedMesh;

                Assert.That(result.Success, Is.True, seed.ToString());
                Assert.That(result.Root, Is.Not.Null, seed.ToString());
                Assert.That(mesh.subMeshCount, Is.EqualTo(3), seed.ToString());
                Assert.That(mesh.GetTriangles(1).Length, Is.GreaterThan(0), seed.ToString());
                Assert.That(renderer.sharedMaterials.Length, Is.EqualTo(3), seed.ToString());
                Assert.That(renderer.sharedMaterials[0], Is.SameAs(topMaterial), seed.ToString());
                Assert.That(renderer.sharedMaterials[1], Is.Not.SameAs(topMaterial), seed.ToString());
                Assert.That(renderer.sharedMaterials[2], Is.Not.SameAs(topMaterial), seed.ToString());

                Object.DestroyImmediate(result.Root);
                Object.DestroyImmediate(topMaterial);
            }
        }

        [Test]
        public void BlockyCircusTerrainSandboxBuildsPlayModePersistentRampMeshes()
        {
            BlockyCircusGroundsSandboxResult sandbox = BlockyCircusGroundsGrammar.Generate(
                BlockyCircusGroundsSandboxSettings.Default(seed: 1));
            var parent = new GameObject("Sandbox Ramp Mesh Test Root");
            Material material = new(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"))
            {
                name = "Sandbox Ramp Mesh Test Material"
            };

            try
            {
                MethodInfo method = typeof(BlockyCircusTerrainSandboxWindow).GetMethod(
                    "BuildConstructedRampMeshes",
                    BindingFlags.NonPublic | BindingFlags.Static);

                Assert.That(method, Is.Not.Null);
                method.Invoke(null, new object[] { parent.transform, sandbox.Map, sandbox.Pieces, material });

                MeshFilter[] filters = parent.GetComponentsInChildren<MeshFilter>();
                MeshCollider[] colliders = parent.GetComponentsInChildren<MeshCollider>();
                GeneratedRampMarker[] markers = parent.GetComponentsInChildren<GeneratedRampMarker>();
                Assert.That(filters, Is.Not.Empty);
                Assert.That(colliders, Is.Not.Empty);
                Assert.That(markers.Length, Is.EqualTo(colliders.Length));

                foreach (MeshFilter filter in filters)
                {
                    Mesh mesh = filter.sharedMesh;
                    Assert.That(mesh, Is.Not.Null);
                    Assert.That(mesh.uv.Length, Is.EqualTo(mesh.vertexCount));
                    Assert.That(mesh.uv.Any(uv => uv.sqrMagnitude > 0.0001f), Is.True);
                    if (mesh.subMeshCount > 1 && mesh.vertexCount > 14)
                    {
                        Vector2[] uvs = mesh.uv;
                        float longSideWallU = Mathf.Max(uvs[8].x, Mathf.Max(uvs[9].x, uvs[10].x))
                            - Mathf.Min(uvs[8].x, Mathf.Min(uvs[9].x, uvs[10].x));
                        Assert.That(longSideWallU, Is.GreaterThan(0.25f));
                    }

                    Assert.That(
                        mesh.hideFlags & HideFlags.DontSave,
                        Is.EqualTo(HideFlags.None),
                        $"{filter.name} visual mesh must survive editor Play Mode reload.");
                }

                foreach (MeshCollider collider in colliders)
                {
                    Assert.That(collider.sharedMesh, Is.Not.Null);
                    Assert.That(
                        collider.sharedMesh.hideFlags & HideFlags.DontSave,
                        Is.EqualTo(HideFlags.None),
                        $"{collider.name} collider mesh must survive editor Play Mode reload.");
                }

                foreach (GeneratedRampMarker marker in markers)
                {
                    Assert.That(marker.HalfWidth, Is.GreaterThan(0f));
                    Assert.That(marker.HalfLength, Is.GreaterThan(0f));
                    Assert.That(marker.HighHeight, Is.GreaterThan(marker.LowHeight));
                    Assert.That(marker.Direction.y, Is.EqualTo(0f).Within(0.001f));
                    Assert.That(marker.Side.y, Is.EqualTo(0f).Within(0.001f));
                }
            }
            finally
            {
                Object.DestroyImmediate(parent);
                Object.DestroyImmediate(material);
            }
        }

        [Test]
        public void GeneratedWorldTerrainBuilderUsesSurfaceZoneMaterialsForConstructedRamps()
        {
            BlockyCircusGroundsSandboxResult sandbox = BlockyCircusGroundsGrammar.Generate(
                BlockyCircusGroundsSandboxSettings.Default(seed: 1));
            var parent = new GameObject("Surface Zone Ramp Test Root");
            Material[] rampMaterials = CreateMaterials("Ramp Zone", 4);
            Material[] wallMaterials = CreateMaterials("Wall Zone", 4);
            var materialSet = new GeneratedWorldSurfaceMaterialSet(
                CreateMaterials("Top Zone", 4),
                CreateMaterials("Organic Zone", 4),
                wallMaterials,
                rampMaterials,
                CreateMaterial("Rim Zone"),
                surfaceZoneSeed: 2034,
                surfaceZoneActNumber: 1,
                ownedObjects: Array.Empty<Object>());

            try
            {
                var chosenPositions = new List<Vector3>();
                var chosenZones = new HashSet<int>();
                foreach ((Vector3 position, int zone) in CandidateZonePositions(sandbox.Map, 2034, 1, 4))
                {
                    if (!chosenZones.Add(zone))
                    {
                        continue;
                    }

                    chosenPositions.Add(position);
                    if (chosenPositions.Count >= 2)
                    {
                        break;
                    }
                }

                Vector3[] positions = chosenPositions.ToArray();
                Assert.That(positions.Length, Is.EqualTo(2));

                var pieces = new List<BlockyCircusGroundsPiece>
                {
                    new(
                        BlockyCircusGroundsPieceKind.RampAisle,
                        positions[0],
                        new Vector3(8f, 3f, 20f),
                        Vector2.up,
                        walkable: true,
                        ramp: true,
                        rewardSafe: false),
                    new(
                        BlockyCircusGroundsPieceKind.RampAisle,
                        positions[1],
                        new Vector3(8f, 3f, 20f),
                        Vector2.right,
                        walkable: true,
                        ramp: true,
                        rewardSafe: false)
                };

                int built = GeneratedWorldTerrainBuilder.BuildConstructedRampMeshes(
                    parent.transform,
                    sandbox.Map,
                    pieces,
                    materialSet);

                Material[] usedRampMaterials = parent
                    .GetComponentsInChildren<MeshRenderer>()
                    .Select(renderer => renderer.sharedMaterials[0])
                    .Distinct()
                    .ToArray();

                Assert.That(built, Is.EqualTo(2));
                Assert.That(usedRampMaterials.Length, Is.EqualTo(2));
                Assert.That(usedRampMaterials.All(material => rampMaterials.Contains(material)), Is.True);
            }
            finally
            {
                Object.DestroyImmediate(parent);
                foreach (Material material in materialSet.TopMaterials
                    .Concat(materialSet.OrganicMaterials)
                    .Concat(materialSet.WallMaterials)
                    .Concat(materialSet.RampMaterials)
                    .Append(materialSet.RimMaterial))
                {
                    Object.DestroyImmediate(material);
                }
            }

            static Material[] CreateMaterials(string name, int count)
            {
                return Enumerable.Range(1, count)
                    .Select(index => CreateMaterial($"{name} {index}"))
                    .ToArray();
            }

            static Material CreateMaterial(string name)
            {
                return new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"))
                {
                    name = name
                };
            }

            static IEnumerable<(Vector3 Position, int Zone)> CandidateZonePositions(
                GeneratedWorldMap map,
                int seed,
                int actNumber,
                int zoneCount)
            {
                float radius = map.PlayableRadius * 0.8f;
                for (int i = 0; i < 16; i++)
                {
                    float angle = i * Mathf.PI * 2f / 16f;
                    Vector3 position = new(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                    yield return (
                        position,
                        GeneratedWorldTerrainBuilder.SelectSurfaceZoneIndex(
                            map,
                            position,
                            seed,
                            actNumber,
                            zoneCount));
                }
            }
        }

        [Test]
        public void BlockyCircusTerrainSandboxMovementPlaytestKeepsRuntimeRootsAtOneScale()
        {
            BlockyCircusGroundsSandboxResult sandbox = BlockyCircusGroundsGrammar.Generate(
                BlockyCircusGroundsSandboxSettings.Default(seed: 1));
            var parent = new GameObject("Sandbox Movement Playtest Root");

            try
            {
                MethodInfo method = typeof(BlockyCircusTerrainSandboxWindow).GetMethod(
                    "BuildMovementPlaytest",
                    BindingFlags.NonPublic | BindingFlags.Static);

                Assert.That(method, Is.Not.Null);
                method.Invoke(null, new object[] { parent.transform, sandbox });

                Transform movementPlaytest = parent.transform.Find("Movement Playtest");
                Assert.That(movementPlaytest, Is.Not.Null);
                AssertVector3(parent.transform.localScale, Vector3.one);
                AssertVector3(movementPlaytest.localScale, Vector3.one);

                Transform player = movementPlaytest.Find("Sandbox Playtest Player");
                Assert.That(player, Is.Not.Null);
                AssertVector3(player.localScale, Vector3.one);
            }
            finally
            {
                Object.DestroyImmediate(parent);
                GameObject camera = GameObject.Find("[Preview] Blocky Terrain Camera");
                if (camera != null)
                {
                    Object.DestroyImmediate(camera);
                }
            }
        }

        private static bool IsCardinal(Vector2 direction)
        {
            Vector2 normalized = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.zero;
            return Mathf.Abs(Mathf.Abs(normalized.x) - 1f) < 0.001f && Mathf.Abs(normalized.y) < 0.001f
                || Mathf.Abs(Mathf.Abs(normalized.y) - 1f) < 0.001f && Mathf.Abs(normalized.x) < 0.001f;
        }

        private static float RadiusFraction(GeneratedWorldMap map, Vector3 position)
        {
            return new Vector2(position.x, position.z).magnitude / Mathf.Max(0.001f, map.PlayableRadius);
        }

        private static void AssertVector3(Vector3 actual, Vector3 expected)
        {
            Assert.That(actual.x, Is.EqualTo(expected.x).Within(0.001f));
            Assert.That(actual.y, Is.EqualTo(expected.y).Within(0.001f));
            Assert.That(actual.z, Is.EqualTo(expected.z).Within(0.001f));
        }

        private static void AssertVector2(Vector2 actual, Vector2 expected)
        {
            Assert.That(actual.x, Is.EqualTo(expected.x).Within(0.001f));
            Assert.That(actual.y, Is.EqualTo(expected.y).Within(0.001f));
        }

        private static Vector2Int DirectionFor(Vector2 direction)
        {
            return Mathf.Abs(direction.x) > Mathf.Abs(direction.y)
                ? new Vector2Int(direction.x >= 0f ? 1 : -1, 0)
                : new Vector2Int(0, direction.y >= 0f ? 1 : -1);
        }

        private static BlockyCircusGroundsSandboxSettings BasinRepairSettings()
        {
            return new BlockyCircusGroundsSandboxSettings(
                seed: 101,
                sampleWidth: 25,
                sampleDepth: 25,
                gridSpacing: 4f,
                stageHeight: 16f,
                constructedStep: 4f,
                wallHeightThreshold: 1.25f,
                playerSlopeLimitDegrees: 42f,
                readableRampSlopeLimitDegrees: 15f);
        }

        private static GeneratedWorldMap SmallMapWithSurfaceKind(GeneratedWorldSurfaceKind surfaceKind)
        {
            var samples = new[]
            {
                Sample(0f, surfaceKind),
                Sample(1f, surfaceKind),
                Sample(2f, surfaceKind),
                Sample(3f, surfaceKind)
            };

            return MapWithSamples(2, 2, samples, wallHeightThreshold: 1.25f);
        }

        private static GeneratedWorldMap FlatMap(int width, int depth, float playableRadius)
        {
            GeneratedWorldSample[] samples = Enumerable.Range(0, width * depth)
                .Select(_ => Sample(1f, GeneratedWorldSurfaceKind.Flat))
                .ToArray();

            return MapWithSamples(
                width,
                depth,
                samples,
                wallHeightThreshold: 1.25f,
                playableRadius: playableRadius);
        }

        private static GeneratedWorldMap MapWithSamples(
            int width,
            int depth,
            GeneratedWorldSample[] samples,
            float wallHeightThreshold,
            float playableRadius = 4f)
        {
            return new GeneratedWorldMap(
                actNumber: 1,
                seed: 1,
                attemptSeed: 1,
                width,
                depth,
                playableRadius,
                gridSpacing: 2f,
                maxHeight: 4f,
                wallHeightThreshold,
                samples,
                playerStartIndex: 0,
                usedFallback: false,
                warnings: null);
        }

        private static GeneratedWorldSample Sample(
            float height,
            GeneratedWorldSurfaceKind surfaceKind,
            Vector3? normal = null)
        {
            return new GeneratedWorldSample(
                height,
                normal ?? Vector3.up,
                0f,
                GeneratedWorldMask.Walkable | GeneratedWorldMask.Reachable,
                0f,
                0f,
                0f,
                surfaceKind);
        }
    }
}
