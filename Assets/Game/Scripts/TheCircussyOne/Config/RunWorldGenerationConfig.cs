using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TheCircussyOne.Rules;
using UnityEngine;

namespace TheCircussyOne.Config
{
    public enum RunWorldTerrainMode
    {
        StructuralGrammar,
        SmoothStamps
    }

    public enum RunWorldLayoutMode
    {
        Generated,
        Prototype
    }

    public enum GeneratedWorldSurfaceStyleMode
    {
        SeededCircus,
        ClassicBigTop,
        CandyChecker,
        PaintedBoardwalk
    }

    [Serializable]
    public sealed class RunWorldGenerationProfile
    {
        [ReadOnly]
        public string actLabel = "Act I";

        [Min(1)]
        public int actNumber = 1;

        [Min(16f), SuffixLabel("u")]
        public float playableRadius = 256f;

        [Min(0f), SuffixLabel("u")]
        public float maxHeight = 32f;

        [Min(1f), SuffixLabel("u")]
        public float gridSpacing = 4f;

        [Min(0f), SuffixLabel("u")]
        public float boundaryMargin = 16f;

        [Min(1)]
        public int landformStampCount = 12;

        [Range(0f, 1f)]
        public float angularity = 0.08f;

        [Min(0)]
        public int terraceSteps = 0;

        [BoxGroup("Structural Grammar"), Min(1)]
        public int plateauCount = 2;

        [BoxGroup("Structural Grammar"), Range(0.03f, 0.5f), LabelText("Min Plateau Radius")]
        public float plateauRadiusMinPercent = 0.12f;

        [BoxGroup("Structural Grammar"), Range(0.03f, 0.6f), LabelText("Max Plateau Radius")]
        public float plateauRadiusMaxPercent = 0.26f;

        [BoxGroup("Structural Grammar"), Range(0.03f, 0.4f), LabelText("Ramp Width")]
        public float rampWidthPercent = 0.11f;

        [BoxGroup("Structural Grammar"), Range(0.01f, 0.3f), LabelText("Edge Blend")]
        public float rampBlendPercent = 0.06f;

        [BoxGroup("Structural Grammar"), Min(0), LabelText("Ramp Count")]
        public int rampCount = 0;

        [BoxGroup("Structural Grammar"), Range(0.02f, 0.3f), LabelText("Min Ramp Width")]
        public float rampWidthMinPercent = 0.055f;

        [BoxGroup("Structural Grammar"), Range(0.02f, 0.4f), LabelText("Max Ramp Width")]
        public float rampWidthMaxPercent = 0.12f;

        [BoxGroup("Structural Grammar"), Range(0.03f, 0.45f), LabelText("Min Ramp Length")]
        public float rampLengthMinPercent = 0.07f;

        [BoxGroup("Structural Grammar"), Range(0.03f, 0.6f), LabelText("Max Ramp Length")]
        public float rampLengthMaxPercent = 0.18f;

        [BoxGroup("Structural Grammar"), Min(0f), SuffixLabel("u"), LabelText("Wall Height Threshold")]
        public float wallHeightThreshold = 1.25f;

        [BoxGroup("Structural Grammar"), LabelText("Allow Ramp Overlap")]
        public bool allowRampOverlap = true;

        [BoxGroup("Structural Grammar"), Range(0f, 0.25f), LabelText("Surface Variation")]
        public float surfaceVariationStrength = 0.04f;

        [Min(0), LabelText("Pocket Chests")]
        public int generatedChestCount = 14;

        [Min(0), LabelText("Pocket Ticket Deposits")]
        public int generatedTicketDepositCount = 36;

        [Min(0), LabelText("Pocket Healing Props")]
        public int generatedHealingPropCount = 12;

        [Min(0), LabelText("Loose Chests")]
        public int generatedLooseChestCount = 28;

        [Min(0), LabelText("Loose Ticket Deposits")]
        public int generatedLooseTicketDepositCount = 72;

        [Min(0), LabelText("Loose Healing Props")]
        public int generatedLooseHealingPropCount = 24;

        public int ActNumber => Mathf.Max(1, actNumber);
        public float PlayableRadius => Mathf.Max(16f, playableRadius);
        public float MaxHeight => Mathf.Max(0f, maxHeight);
        public float GridSpacing => Mathf.Max(1f, gridSpacing);
        public float BoundaryMargin => Mathf.Clamp(boundaryMargin, 0f, PlayableRadius * 0.35f);
        public int LandformStampCount => Mathf.Max(1, landformStampCount);
        public float Angularity => Mathf.Clamp01(angularity);
        public int TerraceSteps => Mathf.Max(0, terraceSteps);
        public int PlateauCount => Mathf.Max(1, plateauCount);
        public float PlateauRadiusMinPercent => Mathf.Min(RawPlateauRadiusMinPercent, RawPlateauRadiusMaxPercent);
        public float PlateauRadiusMaxPercent => Mathf.Max(RawPlateauRadiusMinPercent, RawPlateauRadiusMaxPercent);
        public float RampWidthPercent => Mathf.Clamp(rampWidthPercent, 0.03f, 0.4f);
        public float RampBlendPercent => Mathf.Clamp(rampBlendPercent, 0.01f, 0.3f);
        public int RampCount => rampCount > 0 ? rampCount : Mathf.Max(1, PlateauCount - 1);
        public float RampWidthMinPercent => Mathf.Min(RawRampWidthMinPercent, RawRampWidthMaxPercent);
        public float RampWidthMaxPercent => Mathf.Max(RawRampWidthMinPercent, RawRampWidthMaxPercent);
        public float RampLengthMinPercent => Mathf.Min(RawRampLengthMinPercent, RawRampLengthMaxPercent);
        public float RampLengthMaxPercent => Mathf.Max(RawRampLengthMinPercent, RawRampLengthMaxPercent);
        public float WallHeightThreshold => Mathf.Max(0f, wallHeightThreshold);
        public float SurfaceVariationStrength => Mathf.Clamp(surfaceVariationStrength, 0f, 0.25f);
        public int GeneratedChestCount => Mathf.Max(0, generatedChestCount);
        public int GeneratedTicketDepositCount => Mathf.Max(0, generatedTicketDepositCount);
        public int GeneratedHealingPropCount => Mathf.Max(0, generatedHealingPropCount);
        public int GeneratedLooseChestCount => Mathf.Max(0, generatedLooseChestCount);
        public int GeneratedLooseTicketDepositCount => Mathf.Max(0, generatedLooseTicketDepositCount);
        public int GeneratedLooseHealingPropCount => Mathf.Max(0, generatedLooseHealingPropCount);

        private float RawPlateauRadiusMinPercent => Mathf.Clamp(plateauRadiusMinPercent, 0.03f, 0.5f);
        private float RawPlateauRadiusMaxPercent => Mathf.Clamp(plateauRadiusMaxPercent, 0.03f, 0.6f);
        private float RawRampWidthMinPercent => Mathf.Clamp(rampWidthMinPercent, 0.02f, 0.3f);
        private float RawRampWidthMaxPercent => Mathf.Clamp(rampWidthMaxPercent, 0.02f, 0.4f);
        private float RawRampLengthMinPercent => Mathf.Clamp(rampLengthMinPercent, 0.03f, 0.45f);
        private float RawRampLengthMaxPercent => Mathf.Clamp(rampLengthMaxPercent, 0.03f, 0.6f);
    }

    [CreateAssetMenu(menuName = "The Circussy One/Run World Generation Config", fileName = "RunWorldGenerationConfig")]
    [InfoBox("LIVE RUNTIME: Act/world generation settings. Generated blocky circus terrain is the default run surface; prototype arena mode is a debug fallback.")]
    public sealed class RunWorldGenerationConfig : SerializedScriptableObject
    {
        private const string Tabs = "World Generation";

        [TabGroup(Tabs, "Profiles"), BoxGroup(Tabs + "/Profiles/Act Profiles")]
        [ListDrawerSettings(DefaultExpandedState = true, DraggableItems = false)]
        public List<RunWorldGenerationProfile> actProfiles = CreateDefaultProfiles();

        [TabGroup(Tabs, "Seed"), BoxGroup(Tabs + "/Seed/Debug"), LabelWidth(220)]
        public bool useDebugSeedOverride;

        [TabGroup(Tabs, "Seed"), BoxGroup(Tabs + "/Seed/Debug"), LabelWidth(220)]
        public int debugSeedOverride = 12345;

        [TabGroup(Tabs, "Shape"), BoxGroup(Tabs + "/Shape/Terrain Grammar"), LabelWidth(220), EnumToggleButtons]
        public RunWorldTerrainMode terrainMode = RunWorldTerrainMode.StructuralGrammar;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Layout"), LabelWidth(220), EnumToggleButtons]
        public RunWorldLayoutMode defaultLayoutMode = RunWorldLayoutMode.Generated;

        [TabGroup(Tabs, "Validation"), BoxGroup(Tabs + "/Validation/Generation"), LabelWidth(220)]
        [Min(0)]
        public int retryCount = 3;

        [TabGroup(Tabs, "Validation"), BoxGroup(Tabs + "/Validation/Generation"), LabelWidth(220)]
        public bool fallbackToFlatWorld = true;

        [TabGroup(Tabs, "Validation"), BoxGroup(Tabs + "/Validation/Generation"), LabelWidth(220), SuffixLabel("%")]
        [Range(1f, 100f)]
        public float minimumReachablePercent = 55f;

        [TabGroup(Tabs, "Validation"), BoxGroup(Tabs + "/Validation/Placement Masks"), LabelWidth(220), SuffixLabel("deg")]
        [Range(0f, 89f)]
        public float rewardSafeMaxSlope = 28f;

        [TabGroup(Tabs, "Validation"), BoxGroup(Tabs + "/Validation/Placement Masks"), LabelWidth(220), SuffixLabel("deg")]
        [Range(0f, 89f)]
        public float spawnSafeMaxSlope = 34f;

        [TabGroup(Tabs, "Scoring"), BoxGroup(Tabs + "/Scoring/Candidate Bias"), LabelWidth(220)]
        public bool centerBiasEnabled = true;

        [TabGroup(Tabs, "Scoring"), BoxGroup(Tabs + "/Scoring/Candidate Bias"), LabelWidth(220)]
        public bool highGroundBiasEnabled = true;

        [TabGroup(Tabs, "Scoring"), BoxGroup(Tabs + "/Scoring/Candidate Bias"), LabelWidth(220)]
        [Range(0f, 2f)]
        public float playerStartCenterWeight = 1f;

        [TabGroup(Tabs, "Scoring"), BoxGroup(Tabs + "/Scoring/Candidate Bias"), LabelWidth(220)]
        [Range(0f, 2f)]
        public float rewardCenterWeight = 0.35f;

        [TabGroup(Tabs, "Scoring"), BoxGroup(Tabs + "/Scoring/Candidate Bias"), LabelWidth(220)]
        [Range(0f, 2f)]
        public float rewardHighGroundWeight = 0.25f;

        [TabGroup(Tabs, "Scoring"), BoxGroup(Tabs + "/Scoring/Candidate Bias"), LabelWidth(220)]
        [Range(0f, 2f)]
        public float rewardSpreadWeight = 0.65f;

        [TabGroup(Tabs, "Placement"), BoxGroup(Tabs + "/Placement/Generated Rewards"), LabelWidth(220), SuffixLabel("u")]
        [Min(0f)]
        public float generatedChestSpacing = 18f;

        [TabGroup(Tabs, "Placement"), BoxGroup(Tabs + "/Placement/Generated Rewards"), LabelWidth(220), SuffixLabel("u")]
        [Min(0f)]
        public float generatedTicketDepositSpacing = 10f;

        [TabGroup(Tabs, "Placement"), BoxGroup(Tabs + "/Placement/Generated Rewards"), LabelWidth(220), SuffixLabel("u")]
        [Min(0f)]
        public float generatedHealingPropSpacing = 14f;

        [TabGroup(Tabs, "Placement"), BoxGroup(Tabs + "/Placement/Generated Loose Rewards"), LabelWidth(220), SuffixLabel("u")]
        [Min(0f)]
        public float generatedLooseChestSpacing = 12f;

        [TabGroup(Tabs, "Placement"), BoxGroup(Tabs + "/Placement/Generated Loose Rewards"), LabelWidth(220), SuffixLabel("u")]
        [Min(0f)]
        public float generatedLooseTicketDepositSpacing = 7f;

        [TabGroup(Tabs, "Placement"), BoxGroup(Tabs + "/Placement/Generated Loose Rewards"), LabelWidth(220), SuffixLabel("u")]
        [Min(0f)]
        public float generatedLooseHealingPropSpacing = 9f;

        [TabGroup(Tabs, "Placement"), BoxGroup(Tabs + "/Placement/Enemy Spawns"), LabelWidth(220), SuffixLabel("u")]
        [Min(0f)]
        public float enemySpawnSafeSearchRadius = 32f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Terrain"), LabelWidth(220)]
        public bool runtimeMeshTerrainEnabled;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Terrain"), LabelWidth(220)]
        public bool hidePrototypeArenaWhenGenerated;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Terrain"), LabelWidth(220)]
        [Min(1)]
        public int chunkQuadsPerSide = 16;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Terrain"), LabelWidth(220), LabelText("Chunks Per Frame"), Min(1)]
        public int terrainBuildChunksPerFrame = 2;

        [TabGroup(Tabs, "Diagnostics"), BoxGroup(Tabs + "/Diagnostics/World Load Timing"), LabelWidth(220)]
        public bool worldLoadTimingDiagnosticsEnabled;

        [TabGroup(Tabs, "Diagnostics"), BoxGroup(Tabs + "/Diagnostics/World Load Timing"), LabelWidth(220), SuffixLabel("ms"), Min(0f)]
        public float worldLoadTimingSlowThresholdMs = 16.7f;

        [TabGroup(Tabs, "Diagnostics"), BoxGroup(Tabs + "/Diagnostics/World Load Timing"), LabelWidth(220)]
        public bool worldLoadTimingLogAllStages;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Terrain"), LabelWidth(220), AssetsOnly]
        public Material terrainMaterial;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Surface Textures"), LabelWidth(220), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.VisualColor")]
        public bool generatedTerrainSurfaceTexturesEnabled = true;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Surface Textures/Style"), LabelWidth(220)]
        public GeneratedWorldSurfaceStyleMode generatedTerrainSurfaceStyleMode = GeneratedWorldSurfaceStyleMode.SeededCircus;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Surface Textures/Style"), LabelWidth(220), Range(0f, 1f)]
        public float generatedTerrainSurfaceStyleVariationStrength = 0.55f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Surface Textures/Style"), LabelWidth(220), Range(0f, 1f)]
        public float generatedTerrainSurfaceColorfulness = 0.62f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Surface Textures/Style"), LabelWidth(220), Range(0f, 1f)]
        public float generatedTerrainSurfaceWearStrength = 0.68f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Surface Textures/Style"), LabelWidth(220), Min(0.25f)]
        public float generatedTerrainOrganicPatternScale = 1.65f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Surface Textures/Zones"), LabelWidth(220)]
        public bool generatedTerrainSurfaceZoneVarietyEnabled = true;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Surface Textures/Zones"), LabelWidth(220), Min(1)]
        public int generatedTerrainSurfaceZoneCount = 4;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Surface Textures/Zones"), LabelWidth(220), Range(0f, 1f)]
        public float generatedTerrainSurfaceZonePaletteStrength = 0.45f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Surface Textures/Zones"), LabelWidth(220), Range(0.25f, 4f)]
        public float generatedTerrainSurfaceZoneBlendSharpness = 1.15f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Surface Textures"), LabelWidth(220), Min(128)]
        public int generatedTerrainSurfaceTextureResolution = 512;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Surface Textures"), LabelWidth(220), Range(0f, 1f)]
        public float generatedTerrainSurfaceGritStrength = 0.30f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Surface Textures"), LabelWidth(220), Range(0f, 1f)]
        public float generatedTerrainSurfaceDirtStrength = 0.36f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Surface Textures"), LabelWidth(220), Range(0f, 1f)]
        public float generatedTerrainSurfaceGridStrength = 0.26f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Surface Textures"), LabelWidth(220), Range(0f, 1f)]
        public float generatedTerrainRampCueStrength = 0.38f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Surface Textures"), LabelWidth(220), Range(0f, 1f)]
        public float generatedTerrainOrganicPatternStrength = 0.18f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Surface Textures"), LabelWidth(220), Min(1)]
        public int generatedTerrainOrganicPatternIterations = 64;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Surface Textures"), LabelWidth(220), Range(0f, 2f)]
        public float generatedTerrainSurfaceReliefStrength = 1f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Surface Textures"), LabelWidth(220), Range(0f, 0.25f)]
        public float generatedTerrainTopTileHeightVariation = 0.075f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Surface Textures/Top Detail"), LabelWidth(220), Range(0f, 3f)]
        public float generatedTerrainTopTileReliefStrength = 1.45f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Surface Textures/Top Detail"), LabelWidth(220), Range(0f, 3f)]
        public float generatedTerrainTopTileGroutDepthStrength = 1.35f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Surface Textures/Top Detail"), LabelWidth(220), Range(0f, 2f)]
        public float generatedTerrainTopTileEdgeWearStrength = 0.55f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Surface Textures/Top Detail"), LabelWidth(220), Range(0f, 3f)]
        public float generatedTerrainTopTileSmoothnessVariationStrength = 1.20f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Surface Textures"), LabelWidth(220), Range(0f, 0.25f)]
        public float generatedTerrainWallPanelHeightVariation = 0.065f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Surface Textures/Ramp Detail"), LabelWidth(220), Range(0f, 2f)]
        public float generatedTerrainRampPlankSeamStrength = 1.35f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Surface Textures/Ramp Detail"), LabelWidth(220), Range(0f, 2f)]
        public float generatedTerrainRampEdgeWearStrength = 0.60f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Surface Textures/Ramp Detail"), LabelWidth(220), Range(0f, 2f)]
        public float generatedTerrainRampWoodGrainStrength = 1.20f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Surface Textures/Wall Detail"), LabelWidth(220), Range(0f, 2f)]
        public float generatedTerrainWallPanelSeamStrength = 1.35f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Surface Textures/Wall Detail"), LabelWidth(220), Range(0f, 2f)]
        public float generatedTerrainWallEdgeWearStrength = 0.55f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Surface Textures/Wall Detail"), LabelWidth(220), Range(0f, 2f)]
        public float generatedTerrainWallVerticalWearStrength = 1.15f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Surface Textures/Smoothness"), LabelWidth(220), Range(0f, 1f)]
        public float generatedTerrainTopSmoothnessMin = 0.08f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Surface Textures/Smoothness"), LabelWidth(220), Range(0f, 1f)]
        public float generatedTerrainTopSmoothnessMax = 0.24f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Surface Textures/Smoothness"), LabelWidth(220), Range(0f, 1f)]
        public float generatedTerrainWallSmoothnessMin = 0.05f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Surface Textures/Smoothness"), LabelWidth(220), Range(0f, 1f)]
        public float generatedTerrainWallSmoothnessMax = 0.18f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Surface Textures/Smoothness"), LabelWidth(220), Range(0f, 2f)]
        public float generatedTerrainSurfaceSmoothnessVariationStrength = 1f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Surface Textures/Tiling"), LabelWidth(220), Min(0.01f)]
        public float generatedTerrainOrganicTextureScale = 0.45f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Surface Textures/Colors"), LabelWidth(220)]
        public Color generatedTerrainTopColor = new(0.055f, 0.06f, 0.065f, 1f);

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Surface Textures/Colors"), LabelWidth(220)]
        public Color generatedTerrainWallColor = new(0.22f, 0.035f, 0.045f, 1f);

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Surface Textures/Colors"), LabelWidth(220)]
        public Color generatedTerrainRampColor = new(0.22f, 0.12f, 0.055f, 1f);

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Surface Textures/Colors"), LabelWidth(220)]
        public Color generatedTerrainRimColor = new(0.12f, 0.075f, 0.06f, 1f);

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/World Dressing"), LabelWidth(220), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.VisualColor")]
        public bool worldDressingEnabled = true;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/World Dressing/Counts"), LabelWidth(220), SuffixLabel("per 100k u2"), Min(0f)]
        public float worldDressingGroundDecalsPer100k = 46f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/World Dressing/Counts"), LabelWidth(220), SuffixLabel("decals"), Min(0)]
        public int worldDressingGroundDecalMaxCount = 420;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/World Dressing/Counts"), LabelWidth(220), SuffixLabel("per 100k u2"), Min(0f)]
        public float worldDressingWallDecalsPer100k = 22f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/World Dressing/Counts"), LabelWidth(220), SuffixLabel("decals"), Min(0)]
        public int worldDressingWallDecalMaxCount = 220;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/World Dressing/Placement"), LabelWidth(220), SuffixLabel("u"), Min(0f)]
        public float worldDressingPlayerStartExclusionRadius = 18f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/World Dressing/Placement"), LabelWidth(220), SuffixLabel("u"), Min(0f)]
        public float worldDressingRewardExclusionRadius = 8f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/World Dressing/Placement"), LabelWidth(220), SuffixLabel("u"), Min(0f)]
        public float worldDressingArenaEdgePadding = 14f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/World Dressing/Placement"), LabelWidth(220), Range(0f, 1f)]
        public float worldDressingRampCenterSkipChance = 0.72f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/World Dressing/Ground Decals"), LabelWidth(220), SuffixLabel("u"), Min(0f)]
        public float worldDressingGroundSurfaceOffset = 0.045f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/World Dressing/Ground Decals"), LabelWidth(220), SuffixLabel("u"), Min(0.01f)]
        public float worldDressingGroundScaleMin = 2.4f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/World Dressing/Ground Decals"), LabelWidth(220), SuffixLabel("u"), Min(0.01f)]
        public float worldDressingGroundScaleMax = 8.5f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/World Dressing/Ground Decals"), LabelWidth(220), Range(0f, 1f)]
        public float worldDressingGroundOpacity = 0.58f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/World Dressing/Wall Decals"), LabelWidth(220), SuffixLabel("u"), Min(0f)]
        public float worldDressingWallSurfaceOffset = 0.055f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/World Dressing/Wall Decals"), LabelWidth(220), SuffixLabel("u"), Min(0.01f)]
        public float worldDressingWallWidthMin = 2.5f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/World Dressing/Wall Decals"), LabelWidth(220), SuffixLabel("u"), Min(0.01f)]
        public float worldDressingWallWidthMax = 8.5f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/World Dressing/Wall Decals"), LabelWidth(220), SuffixLabel("u"), Min(0.01f)]
        public float worldDressingWallHeightMin = 1.4f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/World Dressing/Wall Decals"), LabelWidth(220), SuffixLabel("u"), Min(0.01f)]
        public float worldDressingWallHeightMax = 7f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/World Dressing/Wall Decals"), LabelWidth(220), Range(0f, 1f)]
        public float worldDressingWallOpacity = 0.72f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/World Dressing/Textures"), LabelWidth(220), SuffixLabel("px"), Min(64)]
        public int worldDressingTextureResolution = 256;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/World Dressing/Loading"), LabelWidth(220), Min(1)]
        public int worldDressingBuildItemsPerFrame = 64;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/World Dressing/Seed"), LabelWidth(220)]
        public int worldDressingSeedOffset = 92001;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/World Decoration"), LabelWidth(220), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.VisualColor")]
        public bool worldDecorationEnabled = true;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/World Decoration/Density"), LabelWidth(220), Range(0f, 4f)]
        public float worldDecorationDensityMultiplier = 1f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/World Decoration/Density"), LabelWidth(220), SuffixLabel("props"), Min(0)]
        public int worldDecorationMaxCount = 360;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/World Decoration/Density"), LabelWidth(220), SuffixLabel("u"), Min(1f)]
        public float worldDecorationReferenceRadius = 512f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/World Decoration/Placement"), LabelWidth(220), SuffixLabel("u"), Min(0f)]
        public float worldDecorationPlayerStartExclusionRadius = 22f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/World Decoration/Placement"), LabelWidth(220), SuffixLabel("u"), Min(0f)]
        public float worldDecorationRewardExclusionRadius = 9f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/World Decoration/Placement"), LabelWidth(220), SuffixLabel("u"), Min(0f)]
        public float worldDecorationStageDoorExclusionRadius = 18f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/World Decoration/Placement"), LabelWidth(220), SuffixLabel("u"), Min(0f)]
        public float worldDecorationArenaEdgePadding = 14f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/World Decoration/Placement"), LabelWidth(220), Range(0f, 1f)]
        public float worldDecorationRampCenterSkipChance = 0.88f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/World Decoration/Placement"), LabelWidth(220), SuffixLabel("u"), Min(0f)]
        public float worldDecorationSurfaceOffset = 0.035f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/World Decoration/Loading"), LabelWidth(220), Min(1)]
        public int worldDecorationBuildItemsPerFrame = 32;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/World Decoration/Seed"), LabelWidth(220)]
        public int worldDecorationSeedOffset = 118003;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Blocking Obstacles"), LabelWidth(220), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.VisualColor")]
        public bool worldObstacleEnabled = true;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Blocking Obstacles/Density"), LabelWidth(220), SuffixLabel("per 100k u2"), Min(0f)]
        public float worldObstacleCountPer100k = 48f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Blocking Obstacles/Density"), LabelWidth(220), SuffixLabel("props"), Min(0)]
        public int worldObstacleMaxCount = 216;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Blocking Obstacles/Placement"), LabelWidth(220), SuffixLabel("u"), Min(0f)]
        public float worldObstacleMinSpacing = 8f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Blocking Obstacles/Placement"), LabelWidth(220), SuffixLabel("u"), Min(0f)]
        public float worldObstaclePlayerStartExclusionRadius = 28f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Blocking Obstacles/Placement"), LabelWidth(220), SuffixLabel("u"), Min(0f)]
        public float worldObstacleRewardExclusionRadius = 2.5f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Blocking Obstacles/Placement"), LabelWidth(220), SuffixLabel("u"), Min(0f)]
        public float worldObstacleArenaEdgePadding = 18f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Blocking Obstacles/Placement"), LabelWidth(220), SuffixLabel("deg"), Range(0f, 45f)]
        public float worldObstacleMaxSlope = 8f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Blocking Obstacles/Placement"), LabelWidth(220), SuffixLabel("u"), Min(0f)]
        public float worldObstacleSurfaceOffset = 0.035f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Blocking Obstacles/Placement"), LabelWidth(220), SuffixLabel("deg"), Range(0f, 4f)]
        public float worldObstacleMaxTiltDegrees = 4f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Blocking Obstacles/Sizes"), LabelWidth(220), SuffixLabel("u"), Min(0.1f)]
        public float worldObstacleWidthMin = 2.2f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Blocking Obstacles/Sizes"), LabelWidth(220), SuffixLabel("u"), Min(0.1f)]
        public float worldObstacleWidthMax = 19.5f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Blocking Obstacles/Sizes"), LabelWidth(220), SuffixLabel("u"), Min(0.1f)]
        public float worldObstacleDepthMin = 2.2f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Blocking Obstacles/Sizes"), LabelWidth(220), SuffixLabel("u"), Min(0.1f)]
        public float worldObstacleDepthMax = 16.5f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Blocking Obstacles/Sizes"), LabelWidth(220), SuffixLabel("u"), Min(0.1f)]
        public float worldObstacleHeightMin = 1.3f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Blocking Obstacles/Sizes"), LabelWidth(220), SuffixLabel("u"), Min(0.1f)]
        public float worldObstacleHeightMax = 5.5f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Blocking Obstacles/Sizes"), LabelWidth(220), SuffixLabel("u"), Min(0.1f)]
        public float worldObstacleJumpableHeightMin = 0.7f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Blocking Obstacles/Sizes"), LabelWidth(220), SuffixLabel("u"), Min(0.1f)]
        public float worldObstacleJumpableHeightMax = 1.35f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Blocking Obstacles/Sizes"), LabelWidth(220), SuffixLabel("u"), Min(0.1f)]
        public float worldObstacleTallHeightMin = 9f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Blocking Obstacles/Sizes"), LabelWidth(220), SuffixLabel("u"), Min(0.1f)]
        public float worldObstacleTallHeightMax = 60f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Blocking Obstacles/Shape Mix"), LabelWidth(220), Range(0f, 1f)]
        public float worldObstacleJumpableChance = 0.32f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Blocking Obstacles/Shape Mix"), LabelWidth(220), Range(0f, 1f)]
        public float worldObstacleTallChance = 0.38f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Blocking Obstacles/Shape Mix"), LabelWidth(220), Range(0f, 1f)]
        public float worldObstacleColumnChance = 0.42f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Blocking Obstacles/Textures"), LabelWidth(220), SuffixLabel("px"), Min(64)]
        public int worldObstacleTextureResolution = 128;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Blocking Obstacles/Loading"), LabelWidth(220), Min(1)]
        public int worldObstacleBuildItemsPerFrame = 12;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Blocking Obstacles/Seed"), LabelWidth(220)]
        public int worldObstacleSeedOffset = 43111;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Arena Barrier"), LabelWidth(220), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.VisualColor")]
        public bool arenaBarrierEnabled = true;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Arena Barrier"), LabelWidth(220), SuffixLabel("u"), Min(0f)]
        public float arenaBarrierRadiusPadding = 0f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Arena Barrier"), LabelWidth(220), SuffixLabel("u"), Min(0.1f)]
        public float arenaBarrierHeight = 22f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Arena Barrier"), LabelWidth(220), SuffixLabel("segments"), Min(12)]
        public int arenaBarrierSegmentCount = 96;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Arena Barrier/Collision"), LabelWidth(220), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.VisualColor")]
        public bool arenaBarrierCollisionEnabled = true;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Arena Barrier/Collision"), LabelWidth(220), SuffixLabel("u"), Min(0.01f)]
        public float arenaBarrierCollisionThickness = 4f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Arena Barrier/Collision"), LabelWidth(220), SuffixLabel("u"), Min(0f)]
        public float arenaBarrierCollisionDepthBelowGround = 64f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Arena Barrier/Collision"), LabelWidth(220), SuffixLabel("u"), Min(0f)]
        public float arenaBarrierCollisionTopPadding = 64f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Arena Barrier/Collision"), LabelWidth(220), SuffixLabel("u"), Min(1f)]
        public float arenaBarrierCollisionMinimumTop = 128f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Arena Barrier/Static Layer"), LabelWidth(220), SuffixLabel("u"), Min(0.001f)]
        public float arenaBarrierLineThickness = 0.16f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Arena Barrier/Static Layer"), LabelWidth(220)]
        public Color arenaBarrierBottomColor = new(0.22f, 0.86f, 1f, 0.34f);

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Arena Barrier/Static Layer"), LabelWidth(220)]
        public Color arenaBarrierTopColor = new(0.74f, 0.32f, 1f, 0.04f);

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Arena Barrier/Static Layer"), LabelWidth(220)]
        public bool arenaBarrierBaseRingEnabled = true;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Arena Barrier/Static Layer"), LabelWidth(220), SuffixLabel("u"), Min(0.001f)]
        public float arenaBarrierBaseRingThickness = 0.45f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Arena Barrier/Static Layer"), LabelWidth(220)]
        public Color arenaBarrierBaseRingColor = new(0.18f, 0.84f, 1f, 0.48f);

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Arena Barrier/Dynamic Layer"), LabelWidth(220)]
        public bool arenaBarrierDynamicRingsEnabled = true;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Arena Barrier/Dynamic Layer"), LabelWidth(220), SuffixLabel("rings"), Min(1)]
        public int arenaBarrierDynamicRingCount = 5;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Arena Barrier/Dynamic Layer"), LabelWidth(220), SuffixLabel("u/sec"), Min(0f)]
        public float arenaBarrierDynamicRingSpeed = 3.4f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Arena Barrier/Dynamic Layer"), LabelWidth(220), SuffixLabel("u"), Min(0.001f)]
        public float arenaBarrierDynamicRingThickness = 0.12f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Arena Barrier/Dynamic Layer"), LabelWidth(220)]
        public Color arenaBarrierDynamicRingColor = new(0.62f, 0.92f, 1f, 0.32f);

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Arena Barrier/Dynamic Layer"), LabelWidth(220), InlineProperty]
        public EaseSettings arenaBarrierDynamicRingFadeEase = EaseSettings.OutCubic;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment"), LabelWidth(220), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.VisualColor")]
        public bool bigTopEnvironmentEnabled = true;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Shape"), LabelWidth(220), SuffixLabel("u"), Min(0f)]
        public float bigTopRadiusPadding = 24f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Shape"), LabelWidth(220), SuffixLabel("u"), Min(1f)]
        public float bigTopCurtainHeight = 72f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Shape"), LabelWidth(220), SuffixLabel("u"), Min(0f)]
        public float bigTopGroundSkirtInset = 28f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Shape"), LabelWidth(220), SuffixLabel("radius"), Range(0.05f, 1f)]
        public float bigTopDomeHeightPercent = 0.34f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Shape"), LabelWidth(220), SuffixLabel("u"), Min(1f)]
        public float bigTopMinimumDomeHeight = 120f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Shape"), LabelWidth(220), SuffixLabel("segments"), Min(16)]
        public int bigTopSegmentCount = 128;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Shape"), LabelWidth(220), SuffixLabel("rings"), Min(2)]
        public int bigTopCanopyRingCount = 8;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Cloth"), LabelWidth(220), SuffixLabel("px"), Min(128)]
        public int bigTopTextureResolution = 1024;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Cloth"), LabelWidth(220), SuffixLabel("stripes"), Min(2)]
        public int bigTopStripeCount = 24;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Cloth"), LabelWidth(220)]
        public Color bigTopRedStripeColor = new(0.66f, 0.055f, 0.04f, 1f);

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Cloth"), LabelWidth(220)]
        public Color bigTopCanvasStripeColor = new(0.82f, 0.69f, 0.48f, 1f);

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Cloth"), LabelWidth(220), Range(0f, 1f)]
        public float bigTopBrightness = 0.92f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Cloth Wear"), LabelWidth(220), Range(0f, 1f)]
        public float bigTopSeamStrength = 0.42f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Cloth Wear"), LabelWidth(220), Range(0f, 1f)]
        public float bigTopWeaveStrength = 0.18f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Cloth Wear"), LabelWidth(220), Range(0f, 1f)]
        public float bigTopGritStrength = 0.28f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Cloth Wear"), LabelWidth(220), Range(0f, 1f)]
        public float bigTopLowerDirtStrength = 0.36f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Atmosphere"), LabelWidth(220), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.VisualColor")]
        public bool bigTopPerimeterFogEnabled = true;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Atmosphere"), LabelWidth(220), SuffixLabel("radius"), Range(0.1f, 1f)]
        public float bigTopPerimeterFogInnerRadiusPercent = 0.68f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Atmosphere"), LabelWidth(220), SuffixLabel("u"), Min(0f)]
        public float bigTopPerimeterFogOuterInset = 5f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Atmosphere"), LabelWidth(220), SuffixLabel("u"), Min(0.05f)]
        public float bigTopPerimeterFogHeight = 18f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Atmosphere"), LabelWidth(220), SuffixLabel("segments"), Min(16)]
        public int bigTopPerimeterFogSegmentCount = 96;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Atmosphere"), LabelWidth(220)]
        public Color bigTopPerimeterFogColor = new(0.42f, 0.34f, 0.24f, 1f);

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Atmosphere"), LabelWidth(220), Range(0f, 1f)]
        public float bigTopPerimeterFogBottomOpacity = 0.18f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Atmosphere"), LabelWidth(220), Range(0f, 1f)]
        public float bigTopPerimeterFogTopOpacity = 0.02f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Atmosphere"), LabelWidth(220), Range(0f, 1f)]
        public float bigTopPerimeterFogNoiseStrength = 0.32f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/God Rays"), LabelWidth(220), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.LightingColor")]
        public bool bigTopGodRaysEnabled = true;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/God Rays"), LabelWidth(220), SuffixLabel("rays"), Min(0)]
        public int bigTopGodRayCount = 12;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/God Rays"), LabelWidth(220)]
        public Color bigTopGodRayColor = new(1f, 0.74f, 0.34f, 1f);

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/God Rays"), LabelWidth(220), Range(0f, 1f)]
        public float bigTopGodRayOpacity = 0.16f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/God Rays"), LabelWidth(220), SuffixLabel("dome"), Range(0f, 0.95f)]
        public float bigTopGodRayStartDomePercent = 0.18f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/God Rays"), LabelWidth(220), SuffixLabel("canopy radius"), Range(0f, 1f)]
        public float bigTopGodRayStartRadiusMinPercent = 0.55f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/God Rays"), LabelWidth(220), SuffixLabel("canopy radius"), Range(0f, 1f)]
        public float bigTopGodRayStartRadiusMaxPercent = 0.95f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/God Rays"), LabelWidth(220), SuffixLabel("arena radius"), Range(0f, 1f)]
        public float bigTopGodRayEndRadiusPercent = 0.18f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/God Rays"), LabelWidth(220), SuffixLabel("u"), Min(0f)]
        public float bigTopGodRayEndHeight = 8f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/God Rays"), LabelWidth(220), SuffixLabel("radius"), Range(0.001f, 0.15f)]
        public float bigTopGodRayTopWidthPercent = 0.018f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/God Rays"), LabelWidth(220), SuffixLabel("radius"), Range(0.001f, 0.25f)]
        public float bigTopGodRayBottomWidthPercent = 0.065f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/God Rays"), LabelWidth(220), SuffixLabel("turns"), Range(0f, 0.5f)]
        public float bigTopGodRayAngleJitter = 0.14f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/God Rays"), LabelWidth(220), Range(0f, 1f)]
        public float bigTopGodRayFlickerAmount = 0.08f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/God Rays"), LabelWidth(220), SuffixLabel("Hz"), Min(0f)]
        public float bigTopGodRayFlickerSpeed = 0.22f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/God Rays"), LabelWidth(220), Min(0.1f)]
        public float bigTopGodRayBottomFadePower = 1.8f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/God Rays"), LabelWidth(220), Min(0.1f)]
        public float bigTopGodRayEdgeFadePower = 1.75f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/God Rays"), LabelWidth(220), Range(0f, 1f)]
        public float bigTopGodRayTextureNoiseStrength = 0.12f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/God Ray Fixtures"), LabelWidth(220), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.LightingColor")]
        public bool bigTopGodRayFixturesEnabled = true;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/God Ray Fixtures"), LabelWidth(220), SuffixLabel("u"), Min(0f)]
        public float bigTopGodRayFixtureLength = 4.5f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/God Ray Fixtures"), LabelWidth(220), SuffixLabel("u"), Min(0f)]
        public float bigTopGodRayFixtureRadius = 1.45f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/God Ray Fixtures"), LabelWidth(220), SuffixLabel("u"), Min(0f)]
        public float bigTopGodRayFixtureLensRadius = 1.15f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/God Ray Fixtures"), LabelWidth(220), Min(1f)]
        public float bigTopGodRayFixtureBeamCoverage = 1.3f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/God Ray Fixtures"), LabelWidth(220)]
        public Color bigTopGodRayFixtureColor = new(0.12f, 0.075f, 0.045f, 1f);

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/God Ray Fixtures"), LabelWidth(220)]
        public Color bigTopGodRayFixtureLensColor = new(1f, 0.78f, 0.38f, 1f);

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/God Ray Fixtures"), LabelWidth(220), Min(0f)]
        public float bigTopGodRayFixtureLensEmissionStrength = 2.7f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Show Lights"), LabelWidth(220), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.LightingColor")]
        public bool bigTopShowLightsEnabled = true;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Show Lights"), LabelWidth(220), SuffixLabel("lights"), Min(0)]
        public int bigTopShowLightCount = 6;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Show Lights"), LabelWidth(220), SuffixLabel("deg"), Range(0f, 60f)]
        public float bigTopShowLightSweepDegrees = 16f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Show Lights"), LabelWidth(220), SuffixLabel("radius"), Range(0f, 0.3f)]
        public float bigTopShowLightRadiusDriftPercent = 0.08f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Show Lights"), LabelWidth(220), SuffixLabel("Hz"), Min(0f)]
        public float bigTopShowLightSpeedMinHz = 0.018f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Show Lights"), LabelWidth(220), SuffixLabel("Hz"), Min(0f)]
        public float bigTopShowLightSpeedMaxHz = 0.045f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Show Lights"), LabelWidth(220), SuffixLabel("sec"), Range(0.02f, 0.5f)]
        public float bigTopShowLightMeshRefreshInterval = 0.08f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Show Lights/Color Drift"), LabelWidth(220), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.LightingColor")]
        public bool bigTopShowLightColorVariationEnabled = true;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Show Lights/Color Drift"), LabelWidth(220)]
        public Color bigTopShowLightAccentPinkColor = new(1f, 0.68f, 0.82f, 1f);

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Show Lights/Color Drift"), LabelWidth(220)]
        public Color bigTopShowLightAccentTealColor = new(0.62f, 0.95f, 1f, 1f);

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Show Lights/Color Drift"), LabelWidth(220), Range(0f, 0.35f)]
        public float bigTopShowLightAccentStrength = 0.08f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Show Lights/Color Drift"), LabelWidth(220), Range(0f, 0.25f)]
        public float bigTopShowLightColorDriftStrength = 0.05f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Show Lights/Color Drift"), LabelWidth(220), SuffixLabel("Hz"), Min(0f)]
        public float bigTopShowLightColorDriftSpeedMinHz = 0.006f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Show Lights/Color Drift"), LabelWidth(220), SuffixLabel("Hz"), Min(0f)]
        public float bigTopShowLightColorDriftSpeedMaxHz = 0.018f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Bulb Strings"), LabelWidth(220), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.LightingColor")]
        public bool bigTopBulbStringsEnabled = true;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Bulb Strings"), LabelWidth(220), SuffixLabel("rings"), Min(0)]
        public int bigTopBulbRingCount = 3;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Bulb Strings"), LabelWidth(220), SuffixLabel("bulbs"), Min(0)]
        public int bigTopBulbsPerRing = 56;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Bulb Strings"), LabelWidth(220), SuffixLabel("strings"), Min(0)]
        public int bigTopBulbRadialStringCount = 12;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Bulb Strings"), LabelWidth(220), SuffixLabel("bulbs"), Min(0)]
        public int bigTopBulbsPerRadialString = 7;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Bulb Strings"), LabelWidth(220), SuffixLabel("u"), Min(0f)]
        public float bigTopBulbRadius = 0.75f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Bulb Strings"), LabelWidth(220)]
        public Color bigTopBulbColor = new(1f, 0.83f, 0.48f, 1f);

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Bulb Strings"), LabelWidth(220), Min(0f)]
        public float bigTopBulbEmissionStrength = 2.2f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Bulb Strings"), LabelWidth(220), Range(0f, 1f)]
        public float bigTopBulbFlickerAmount = 0.10f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Bulb Strings"), LabelWidth(220), SuffixLabel("Hz"), Min(0f)]
        public float bigTopBulbFlickerSpeed = 0.55f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Dressing"), LabelWidth(220), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.VisualColor")]
        public bool bigTopDressingEnabled = true;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Dressing/Center Hub"), LabelWidth(220)]
        public bool bigTopCenterHubEnabled = true;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Dressing/Center Hub"), LabelWidth(220), SuffixLabel("radius"), Range(0.001f, 0.08f)]
        public float bigTopCenterHubRadiusPercent = 0.018f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Dressing/Center Hub"), LabelWidth(220), SuffixLabel("u"), Min(0f)]
        public float bigTopCenterHubTubeRadius = 0.22f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Dressing/Center Hub"), LabelWidth(220), SuffixLabel("cords"), Min(0)]
        public int bigTopCenterHubCordCount = 6;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Dressing/Center Hub"), LabelWidth(220), SuffixLabel("u"), Min(0f)]
        public float bigTopCenterHubCordDrop = 12f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Dressing/Ropes"), LabelWidth(220)]
        public bool bigTopCanopyRopesEnabled = true;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Dressing/Ropes"), LabelWidth(220), SuffixLabel("u"), Min(0f)]
        public float bigTopCanopyRopeThickness = 0.08f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Dressing/Ropes"), LabelWidth(220), SuffixLabel("ropes"), Min(0)]
        public int bigTopCanopyRadialRopeCount = 12;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Dressing/Ropes"), LabelWidth(220), SuffixLabel("rings"), Min(0)]
        public int bigTopCanopyRingRopeCount = 3;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Dressing/Perimeter Poles"), LabelWidth(220)]
        public bool bigTopPerimeterPolesEnabled = true;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Dressing/Perimeter Poles"), LabelWidth(220), SuffixLabel("poles"), Min(0)]
        public int bigTopPerimeterPoleCount = 16;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Dressing/Perimeter Poles"), LabelWidth(220), SuffixLabel("radius"), Range(0.1f, 1f)]
        public float bigTopPerimeterPoleRadiusPercent = 0.93f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Dressing/Perimeter Poles"), LabelWidth(220), SuffixLabel("u"), Min(0f)]
        public float bigTopPerimeterPoleRadius = 0.55f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Dressing/Perimeter Poles"), LabelWidth(220), SuffixLabel("u"), Min(0f)]
        public float bigTopPerimeterPoleTopPadding = 4f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Dressing/Curtain Valance"), LabelWidth(220)]
        public bool bigTopCurtainValanceEnabled = true;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Dressing/Curtain Valance"), LabelWidth(220), SuffixLabel("u"), Min(0f)]
        public float bigTopCurtainValanceHeight = 3.5f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Dressing/Curtain Valance"), LabelWidth(220), SuffixLabel("u"), Min(0f)]
        public float bigTopCurtainValanceScallopDrop = 1.8f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Dressing/Curtain Valance"), LabelWidth(220), SuffixLabel("scallops"), Min(1)]
        public int bigTopCurtainValanceScallopCount = 32;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Dressing/Bunting"), LabelWidth(220)]
        public bool bigTopBuntingEnabled = true;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Dressing/Bunting"), LabelWidth(220), SuffixLabel("rings"), Min(0)]
        public int bigTopBuntingRingCount = 2;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Dressing/Bunting"), LabelWidth(220), SuffixLabel("flags"), Min(0)]
        public int bigTopBuntingFlagsPerRing = 48;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Dressing/Bunting"), LabelWidth(220), SuffixLabel("dome"), Range(0f, 0.95f)]
        public float bigTopBuntingLowerDomePercent = 0.10f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Dressing/Bunting"), LabelWidth(220), SuffixLabel("dome"), Range(0f, 0.95f)]
        public float bigTopBuntingUpperDomePercent = 0.34f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Dressing/Bunting"), LabelWidth(220), SuffixLabel("u"), Min(0f)]
        public float bigTopBuntingFlagWidth = 3.2f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Dressing/Bunting"), LabelWidth(220), SuffixLabel("u"), Min(0f)]
        public float bigTopBuntingFlagDrop = 2.4f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Dressing/Bunting"), LabelWidth(220), SuffixLabel("u"), Min(0f)]
        public float bigTopBuntingStringThickness = 0.055f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Dressing/Curtain Banners"), LabelWidth(220)]
        public bool bigTopCurtainBannersEnabled = true;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Dressing/Curtain Banners"), LabelWidth(220), SuffixLabel("banners"), Min(0)]
        public int bigTopCurtainBannerCount = 16;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Dressing/Curtain Banners"), LabelWidth(220), SuffixLabel("u"), Min(0f)]
        public float bigTopCurtainBannerWidth = 12f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Dressing/Curtain Banners"), LabelWidth(220), SuffixLabel("u"), Min(0f)]
        public float bigTopCurtainBannerHeight = 14f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Dressing/Curtain Banners"), LabelWidth(220), SuffixLabel("u"), Min(0f)]
        public float bigTopCurtainBannerTopOffset = 14f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Dressing/Curtain Banners"), LabelWidth(220), SuffixLabel("u"), Min(0f)]
        public float bigTopCurtainBannerInwardInset = 8f;

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Dressing/Colors"), LabelWidth(220)]
        public Color bigTopDressingRopeColor = new(0.46f, 0.30f, 0.16f, 1f);

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Dressing/Colors"), LabelWidth(220)]
        public Color bigTopDressingPoleColor = new(0.22f, 0.14f, 0.08f, 1f);

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Dressing/Colors"), LabelWidth(220)]
        public Color bigTopCurtainValanceColor = new(0.42f, 0.02f, 0.025f, 1f);

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Dressing/Colors"), LabelWidth(220)]
        public Color bigTopBuntingStringColor = new(0.58f, 0.39f, 0.18f, 1f);

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Dressing/Colors"), LabelWidth(220)]
        public Color bigTopBuntingPrimaryColor = new(0.78f, 0.04f, 0.035f, 1f);

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Dressing/Colors"), LabelWidth(220)]
        public Color bigTopBuntingSecondaryColor = new(0.92f, 0.67f, 0.20f, 1f);

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Dressing/Colors"), LabelWidth(220)]
        public Color bigTopCurtainBannerColor = new(0.48f, 0.025f, 0.035f, 1f);

        [TabGroup(Tabs, "Runtime Mesh"), BoxGroup(Tabs + "/Runtime Mesh/Big Top Environment/Dressing/Colors"), LabelWidth(220)]
        public Color bigTopCurtainBannerTrimColor = new(0.86f, 0.58f, 0.18f, 1f);

        public IReadOnlyList<RunWorldGenerationProfile> Profiles => actProfiles;
        public int ChunkQuadsPerSide => Mathf.Max(1, chunkQuadsPerSide);
        public int TerrainBuildChunksPerFrame => Mathf.Max(1, terrainBuildChunksPerFrame);
        public bool WorldLoadTimingDiagnosticsEnabled => worldLoadTimingDiagnosticsEnabled;
        public float WorldLoadTimingSlowThresholdMs => Mathf.Max(0f, worldLoadTimingSlowThresholdMs);
        public bool WorldLoadTimingLogAllStages => worldLoadTimingLogAllStages;
        public float GeneratedChestSpacing => Mathf.Max(0f, generatedChestSpacing);
        public float GeneratedTicketDepositSpacing => Mathf.Max(0f, generatedTicketDepositSpacing);
        public float GeneratedHealingPropSpacing => Mathf.Max(0f, generatedHealingPropSpacing);
        public float GeneratedLooseChestSpacing => Mathf.Max(0f, generatedLooseChestSpacing);
        public float GeneratedLooseTicketDepositSpacing => Mathf.Max(0f, generatedLooseTicketDepositSpacing);
        public float GeneratedLooseHealingPropSpacing => Mathf.Max(0f, generatedLooseHealingPropSpacing);
        public float EnemySpawnSafeSearchRadius => Mathf.Max(0f, enemySpawnSafeSearchRadius);
        public float RewardSpreadWeight => Mathf.Max(0f, rewardSpreadWeight);
        public int ArenaBarrierSegmentCount => Mathf.Max(12, arenaBarrierSegmentCount);
        public int ArenaBarrierDynamicRingCount => Mathf.Max(1, arenaBarrierDynamicRingCount);
        public float ArenaBarrierCollisionThickness => Mathf.Max(0.01f, arenaBarrierCollisionThickness);
        public float ArenaBarrierCollisionDepthBelowGround => Mathf.Max(0f, arenaBarrierCollisionDepthBelowGround);
        public float ArenaBarrierCollisionTopPadding => Mathf.Max(0f, arenaBarrierCollisionTopPadding);
        public float ArenaBarrierCollisionMinimumTop => Mathf.Max(1f, arenaBarrierCollisionMinimumTop);
        public float BigTopRadiusPadding => Mathf.Max(0f, bigTopRadiusPadding);
        public float BigTopCurtainHeight => Mathf.Max(1f, bigTopCurtainHeight);
        public float BigTopGroundSkirtInset => Mathf.Max(0f, bigTopGroundSkirtInset);
        public float BigTopDomeHeightPercent => Mathf.Clamp(bigTopDomeHeightPercent, 0.05f, 1f);
        public float BigTopMinimumDomeHeight => Mathf.Max(1f, bigTopMinimumDomeHeight);
        public int BigTopSegmentCount => Mathf.Max(16, bigTopSegmentCount);
        public int BigTopCanopyRingCount => Mathf.Max(2, bigTopCanopyRingCount);
        public int BigTopTextureResolution => Mathf.Clamp(bigTopTextureResolution, 128, 2048);
        public GeneratedWorldSurfaceStyleMode GeneratedTerrainSurfaceStyleMode => generatedTerrainSurfaceStyleMode;
        public float GeneratedTerrainSurfaceStyleVariationStrength => Mathf.Clamp01(generatedTerrainSurfaceStyleVariationStrength);
        public float GeneratedTerrainSurfaceColorfulness => Mathf.Clamp01(generatedTerrainSurfaceColorfulness);
        public float GeneratedTerrainSurfaceWearStrength => Mathf.Clamp01(generatedTerrainSurfaceWearStrength);
        public float GeneratedTerrainOrganicPatternScale => Mathf.Clamp(generatedTerrainOrganicPatternScale, 0.25f, 6f);
        public int GeneratedTerrainSurfaceZoneCount => generatedTerrainSurfaceZoneVarietyEnabled ? Mathf.Clamp(generatedTerrainSurfaceZoneCount, 1, 8) : 1;
        public float GeneratedTerrainSurfaceZonePaletteStrength => Mathf.Clamp01(generatedTerrainSurfaceZonePaletteStrength);
        public float GeneratedTerrainSurfaceZoneBlendSharpness => Mathf.Clamp(generatedTerrainSurfaceZoneBlendSharpness, 0.25f, 4f);
        public int GeneratedTerrainSurfaceTextureResolution => Mathf.Clamp(generatedTerrainSurfaceTextureResolution, 128, 1024);
        public float GeneratedTerrainSurfaceGritStrength => Mathf.Clamp01(generatedTerrainSurfaceGritStrength);
        public float GeneratedTerrainSurfaceDirtStrength => Mathf.Clamp01(generatedTerrainSurfaceDirtStrength);
        public float GeneratedTerrainSurfaceGridStrength => Mathf.Clamp01(generatedTerrainSurfaceGridStrength);
        public float GeneratedTerrainRampCueStrength => Mathf.Clamp01(generatedTerrainRampCueStrength);
        public float GeneratedTerrainOrganicPatternStrength => Mathf.Clamp01(generatedTerrainOrganicPatternStrength);
        public int GeneratedTerrainOrganicPatternIterations => Mathf.Clamp(generatedTerrainOrganicPatternIterations, 1, 160);
        public float GeneratedTerrainSurfaceReliefStrength => Mathf.Clamp(generatedTerrainSurfaceReliefStrength, 0f, 2f);
        public float GeneratedTerrainTopTileHeightVariation => Mathf.Clamp(generatedTerrainTopTileHeightVariation, 0f, 0.25f);
        public float GeneratedTerrainTopTileReliefStrength => Mathf.Clamp(generatedTerrainTopTileReliefStrength, 0f, 3f);
        public float GeneratedTerrainTopTileGroutDepthStrength => Mathf.Clamp(generatedTerrainTopTileGroutDepthStrength, 0f, 3f);
        public float GeneratedTerrainTopTileEdgeWearStrength => Mathf.Clamp(generatedTerrainTopTileEdgeWearStrength, 0f, 2f);
        public float GeneratedTerrainTopTileSmoothnessVariationStrength => Mathf.Clamp(generatedTerrainTopTileSmoothnessVariationStrength, 0f, 3f);
        public float GeneratedTerrainWallPanelHeightVariation => Mathf.Clamp(generatedTerrainWallPanelHeightVariation, 0f, 0.25f);
        public float GeneratedTerrainRampPlankSeamStrength => Mathf.Clamp(generatedTerrainRampPlankSeamStrength, 0f, 2f);
        public float GeneratedTerrainRampEdgeWearStrength => Mathf.Clamp(generatedTerrainRampEdgeWearStrength, 0f, 2f);
        public float GeneratedTerrainRampWoodGrainStrength => Mathf.Clamp(generatedTerrainRampWoodGrainStrength, 0f, 2f);
        public float GeneratedTerrainWallPanelSeamStrength => Mathf.Clamp(generatedTerrainWallPanelSeamStrength, 0f, 2f);
        public float GeneratedTerrainWallEdgeWearStrength => Mathf.Clamp(generatedTerrainWallEdgeWearStrength, 0f, 2f);
        public float GeneratedTerrainWallVerticalWearStrength => Mathf.Clamp(generatedTerrainWallVerticalWearStrength, 0f, 2f);
        public float GeneratedTerrainTopSmoothnessMin => Mathf.Clamp01(Mathf.Min(generatedTerrainTopSmoothnessMin, generatedTerrainTopSmoothnessMax));
        public float GeneratedTerrainTopSmoothnessMax => Mathf.Clamp01(Mathf.Max(generatedTerrainTopSmoothnessMin, generatedTerrainTopSmoothnessMax));
        public float GeneratedTerrainWallSmoothnessMin => Mathf.Clamp01(Mathf.Min(generatedTerrainWallSmoothnessMin, generatedTerrainWallSmoothnessMax));
        public float GeneratedTerrainWallSmoothnessMax => Mathf.Clamp01(Mathf.Max(generatedTerrainWallSmoothnessMin, generatedTerrainWallSmoothnessMax));
        public float GeneratedTerrainSurfaceSmoothnessVariationStrength => Mathf.Clamp(generatedTerrainSurfaceSmoothnessVariationStrength, 0f, 2f);
        public float GeneratedTerrainOrganicTextureScale => Mathf.Clamp(generatedTerrainOrganicTextureScale, 0.01f, 4f);
        public bool WorldDressingEnabled => worldDressingEnabled;
        public float WorldDressingGroundDecalsPer100k => Mathf.Max(0f, worldDressingGroundDecalsPer100k);
        public int WorldDressingGroundDecalMaxCount => Mathf.Max(0, worldDressingGroundDecalMaxCount);
        public float WorldDressingWallDecalsPer100k => Mathf.Max(0f, worldDressingWallDecalsPer100k);
        public int WorldDressingWallDecalMaxCount => Mathf.Max(0, worldDressingWallDecalMaxCount);
        public float WorldDressingPlayerStartExclusionRadius => Mathf.Max(0f, worldDressingPlayerStartExclusionRadius);
        public float WorldDressingRewardExclusionRadius => Mathf.Max(0f, worldDressingRewardExclusionRadius);
        public float WorldDressingArenaEdgePadding => Mathf.Max(0f, worldDressingArenaEdgePadding);
        public float WorldDressingRampCenterSkipChance => Mathf.Clamp01(worldDressingRampCenterSkipChance);
        public float WorldDressingGroundSurfaceOffset => Mathf.Max(0f, worldDressingGroundSurfaceOffset);
        public float WorldDressingGroundScaleMin => Mathf.Min(RawWorldDressingGroundScaleMin, RawWorldDressingGroundScaleMax);
        public float WorldDressingGroundScaleMax => Mathf.Max(RawWorldDressingGroundScaleMin, RawWorldDressingGroundScaleMax);
        public float WorldDressingGroundOpacity => Mathf.Clamp01(worldDressingGroundOpacity);
        public float WorldDressingWallSurfaceOffset => Mathf.Max(0f, worldDressingWallSurfaceOffset);
        public float WorldDressingWallWidthMin => Mathf.Min(RawWorldDressingWallWidthMin, RawWorldDressingWallWidthMax);
        public float WorldDressingWallWidthMax => Mathf.Max(RawWorldDressingWallWidthMin, RawWorldDressingWallWidthMax);
        public float WorldDressingWallHeightMin => Mathf.Min(RawWorldDressingWallHeightMin, RawWorldDressingWallHeightMax);
        public float WorldDressingWallHeightMax => Mathf.Max(RawWorldDressingWallHeightMin, RawWorldDressingWallHeightMax);
        public float WorldDressingWallOpacity => Mathf.Clamp01(worldDressingWallOpacity);
        public int WorldDressingTextureResolution => Mathf.Clamp(worldDressingTextureResolution, 64, 1024);
        public int WorldDressingBuildItemsPerFrame => Mathf.Max(1, worldDressingBuildItemsPerFrame);
        public int WorldDressingSeedOffset => worldDressingSeedOffset;
        public bool WorldDecorationEnabled => worldDecorationEnabled;
        public float WorldDecorationDensityMultiplier => Mathf.Clamp(worldDecorationDensityMultiplier, 0f, 4f);
        public int WorldDecorationMaxCount => Mathf.Max(0, worldDecorationMaxCount);
        public float WorldDecorationReferenceRadius => Mathf.Max(1f, worldDecorationReferenceRadius);
        public float WorldDecorationPlayerStartExclusionRadius => Mathf.Max(0f, worldDecorationPlayerStartExclusionRadius);
        public float WorldDecorationRewardExclusionRadius => Mathf.Max(0f, worldDecorationRewardExclusionRadius);
        public float WorldDecorationStageDoorExclusionRadius => Mathf.Max(0f, worldDecorationStageDoorExclusionRadius);
        public float WorldDecorationArenaEdgePadding => Mathf.Max(0f, worldDecorationArenaEdgePadding);
        public float WorldDecorationRampCenterSkipChance => Mathf.Clamp01(worldDecorationRampCenterSkipChance);
        public float WorldDecorationSurfaceOffset => Mathf.Max(0f, worldDecorationSurfaceOffset);
        public int WorldDecorationBuildItemsPerFrame => Mathf.Max(1, worldDecorationBuildItemsPerFrame);
        public int WorldDecorationSeedOffset => worldDecorationSeedOffset;
        public bool WorldObstacleEnabled => worldObstacleEnabled;
        public float WorldObstacleCountPer100k => Mathf.Max(0f, worldObstacleCountPer100k);
        public int WorldObstacleMaxCount => Mathf.Max(0, worldObstacleMaxCount);
        public float WorldObstacleMinSpacing => Mathf.Max(0f, worldObstacleMinSpacing);
        public float WorldObstaclePlayerStartExclusionRadius => Mathf.Max(0f, worldObstaclePlayerStartExclusionRadius);
        public float WorldObstacleRewardExclusionRadius => Mathf.Max(0f, worldObstacleRewardExclusionRadius);
        public float WorldObstacleArenaEdgePadding => Mathf.Max(0f, worldObstacleArenaEdgePadding);
        public float WorldObstacleMaxSlope => Mathf.Clamp(worldObstacleMaxSlope, 0f, 45f);
        public float WorldObstacleSurfaceOffset => Mathf.Max(0f, worldObstacleSurfaceOffset);
        public float WorldObstacleMaxTiltDegrees => Mathf.Clamp(worldObstacleMaxTiltDegrees, 0f, 4f);
        public float WorldObstacleWidthMin => Mathf.Min(RawWorldObstacleWidthMin, RawWorldObstacleWidthMax);
        public float WorldObstacleWidthMax => Mathf.Max(RawWorldObstacleWidthMin, RawWorldObstacleWidthMax);
        public float WorldObstacleDepthMin => Mathf.Min(RawWorldObstacleDepthMin, RawWorldObstacleDepthMax);
        public float WorldObstacleDepthMax => Mathf.Max(RawWorldObstacleDepthMin, RawWorldObstacleDepthMax);
        public float WorldObstacleHeightMin => Mathf.Min(RawWorldObstacleHeightMin, RawWorldObstacleHeightMax);
        public float WorldObstacleHeightMax => Mathf.Max(RawWorldObstacleHeightMin, RawWorldObstacleHeightMax);
        public float WorldObstacleJumpableHeightMin => Mathf.Min(RawWorldObstacleJumpableHeightMin, RawWorldObstacleJumpableHeightMax);
        public float WorldObstacleJumpableHeightMax => Mathf.Max(RawWorldObstacleJumpableHeightMin, RawWorldObstacleJumpableHeightMax);
        public float WorldObstacleTallHeightMin => Mathf.Min(RawWorldObstacleTallHeightMin, RawWorldObstacleTallHeightMax);
        public float WorldObstacleTallHeightMax => Mathf.Max(RawWorldObstacleTallHeightMin, RawWorldObstacleTallHeightMax);
        public float WorldObstacleJumpableChance => Mathf.Clamp01(worldObstacleJumpableChance);
        public float WorldObstacleTallChance => Mathf.Clamp01(worldObstacleTallChance);
        public float WorldObstacleColumnChance => Mathf.Clamp01(worldObstacleColumnChance);
        public int WorldObstacleTextureResolution => Mathf.Clamp(worldObstacleTextureResolution, 64, 256);
        public int WorldObstacleBuildItemsPerFrame => Mathf.Max(1, worldObstacleBuildItemsPerFrame);
        public int WorldObstacleSeedOffset => worldObstacleSeedOffset;
        public int BigTopStripeCount => Mathf.Max(2, bigTopStripeCount);
        public float BigTopBrightness => Mathf.Clamp01(bigTopBrightness);
        public float BigTopSeamStrength => Mathf.Clamp01(bigTopSeamStrength);
        public float BigTopWeaveStrength => Mathf.Clamp01(bigTopWeaveStrength);
        public float BigTopGritStrength => Mathf.Clamp01(bigTopGritStrength);
        public float BigTopLowerDirtStrength => Mathf.Clamp01(bigTopLowerDirtStrength);
        public bool BigTopPerimeterFogEnabled => bigTopPerimeterFogEnabled;
        public float BigTopPerimeterFogInnerRadiusPercent => Mathf.Clamp(bigTopPerimeterFogInnerRadiusPercent, 0.1f, 1f);
        public float BigTopPerimeterFogOuterInset => Mathf.Max(0f, bigTopPerimeterFogOuterInset);
        public float BigTopPerimeterFogHeight => Mathf.Max(0.05f, bigTopPerimeterFogHeight);
        public int BigTopPerimeterFogSegmentCount => Mathf.Max(16, bigTopPerimeterFogSegmentCount);
        public float BigTopPerimeterFogBottomOpacity => Mathf.Clamp01(bigTopPerimeterFogBottomOpacity);
        public float BigTopPerimeterFogTopOpacity => Mathf.Clamp01(bigTopPerimeterFogTopOpacity);
        public float BigTopPerimeterFogNoiseStrength => Mathf.Clamp01(bigTopPerimeterFogNoiseStrength);
        public int BigTopGodRayCount => bigTopGodRaysEnabled ? Mathf.Max(0, bigTopGodRayCount) : 0;
        public float BigTopGodRayOpacity => Mathf.Clamp01(bigTopGodRayOpacity);
        public float BigTopGodRayStartDomePercent => Mathf.Clamp(bigTopGodRayStartDomePercent, 0f, 0.95f);
        public float BigTopGodRayStartRadiusMinPercent => Mathf.Clamp01(Mathf.Min(bigTopGodRayStartRadiusMinPercent, bigTopGodRayStartRadiusMaxPercent));
        public float BigTopGodRayStartRadiusMaxPercent => Mathf.Clamp01(Mathf.Max(bigTopGodRayStartRadiusMinPercent, bigTopGodRayStartRadiusMaxPercent));
        public float BigTopGodRayEndRadiusPercent => Mathf.Clamp01(bigTopGodRayEndRadiusPercent);
        public float BigTopGodRayEndHeight => Mathf.Max(0f, bigTopGodRayEndHeight);
        public float BigTopGodRayTopWidthPercent => Mathf.Clamp(bigTopGodRayTopWidthPercent, 0.001f, 0.15f);
        public float BigTopGodRayBottomWidthPercent => Mathf.Clamp(bigTopGodRayBottomWidthPercent, 0.001f, 0.25f);
        public float BigTopGodRayAngleJitter => Mathf.Clamp(bigTopGodRayAngleJitter, 0f, 0.5f);
        public float BigTopGodRayFlickerAmount => Mathf.Clamp01(bigTopGodRayFlickerAmount);
        public float BigTopGodRayFlickerSpeed => Mathf.Max(0f, bigTopGodRayFlickerSpeed);
        public float BigTopGodRayBottomFadePower => Mathf.Max(0.1f, bigTopGodRayBottomFadePower);
        public float BigTopGodRayEdgeFadePower => Mathf.Max(0.1f, bigTopGodRayEdgeFadePower);
        public float BigTopGodRayTextureNoiseStrength => Mathf.Clamp01(bigTopGodRayTextureNoiseStrength);
        public bool BigTopGodRayFixturesEnabled => bigTopGodRayFixturesEnabled && BigTopGodRayCount > 0;
        public float BigTopGodRayFixtureLength => Mathf.Max(0f, bigTopGodRayFixtureLength);
        public float BigTopGodRayFixtureRadius => Mathf.Max(0f, bigTopGodRayFixtureRadius);
        public float BigTopGodRayFixtureLensRadius => Mathf.Max(0f, bigTopGodRayFixtureLensRadius);
        public float BigTopGodRayFixtureBeamCoverage => Mathf.Max(1f, bigTopGodRayFixtureBeamCoverage);
        public float BigTopGodRayFixtureLensEmissionStrength => Mathf.Max(0f, bigTopGodRayFixtureLensEmissionStrength);
        public bool BigTopShowLightsEnabled => bigTopShowLightsEnabled && BigTopGodRayCount > 0;
        public int BigTopShowLightCount => BigTopShowLightsEnabled ? Mathf.Clamp(bigTopShowLightCount, 0, BigTopGodRayCount) : 0;
        public float BigTopShowLightSweepDegrees => Mathf.Clamp(bigTopShowLightSweepDegrees, 0f, 60f);
        public float BigTopShowLightRadiusDriftPercent => Mathf.Clamp(bigTopShowLightRadiusDriftPercent, 0f, 0.3f);
        public float BigTopShowLightSpeedMinHz => Mathf.Min(RawBigTopShowLightSpeedMinHz, RawBigTopShowLightSpeedMaxHz);
        public float BigTopShowLightSpeedMaxHz => Mathf.Max(RawBigTopShowLightSpeedMinHz, RawBigTopShowLightSpeedMaxHz);
        public float BigTopShowLightMeshRefreshInterval => Mathf.Clamp(bigTopShowLightMeshRefreshInterval, 0.02f, 0.5f);
        public bool BigTopShowLightColorVariationEnabled => bigTopShowLightColorVariationEnabled && BigTopShowLightCount > 0;
        public float BigTopShowLightAccentStrength => Mathf.Clamp(bigTopShowLightAccentStrength, 0f, 0.35f);
        public float BigTopShowLightColorDriftStrength => Mathf.Clamp(bigTopShowLightColorDriftStrength, 0f, 0.25f);
        public float BigTopShowLightColorDriftSpeedMinHz => Mathf.Min(RawBigTopShowLightColorDriftSpeedMinHz, RawBigTopShowLightColorDriftSpeedMaxHz);
        public float BigTopShowLightColorDriftSpeedMaxHz => Mathf.Max(RawBigTopShowLightColorDriftSpeedMinHz, RawBigTopShowLightColorDriftSpeedMaxHz);
        public int BigTopBulbRingCount => bigTopBulbStringsEnabled ? Mathf.Max(0, bigTopBulbRingCount) : 0;
        public int BigTopBulbsPerRing => bigTopBulbStringsEnabled ? Mathf.Max(0, bigTopBulbsPerRing) : 0;
        public int BigTopBulbRadialStringCount => bigTopBulbStringsEnabled ? Mathf.Max(0, bigTopBulbRadialStringCount) : 0;
        public int BigTopBulbsPerRadialString => bigTopBulbStringsEnabled ? Mathf.Max(0, bigTopBulbsPerRadialString) : 0;
        public float BigTopBulbRadius => Mathf.Max(0f, bigTopBulbRadius);
        public float BigTopBulbEmissionStrength => Mathf.Max(0f, bigTopBulbEmissionStrength);
        public float BigTopBulbFlickerAmount => Mathf.Clamp01(bigTopBulbFlickerAmount);
        public float BigTopBulbFlickerSpeed => Mathf.Max(0f, bigTopBulbFlickerSpeed);
        public bool BigTopDressingEnabled => bigTopDressingEnabled;
        public bool BigTopCenterHubEnabled => bigTopDressingEnabled && bigTopCenterHubEnabled;
        public float BigTopCenterHubRadiusPercent => Mathf.Clamp(bigTopCenterHubRadiusPercent, 0.001f, 0.08f);
        public float BigTopCenterHubTubeRadius => Mathf.Max(0f, bigTopCenterHubTubeRadius);
        public int BigTopCenterHubCordCount => bigTopDressingEnabled ? Mathf.Max(0, bigTopCenterHubCordCount) : 0;
        public float BigTopCenterHubCordDrop => Mathf.Max(0f, bigTopCenterHubCordDrop);
        public bool BigTopCanopyRopesEnabled => bigTopDressingEnabled && bigTopCanopyRopesEnabled;
        public float BigTopCanopyRopeThickness => Mathf.Max(0f, bigTopCanopyRopeThickness);
        public int BigTopCanopyRadialRopeCount => bigTopDressingEnabled ? Mathf.Max(0, bigTopCanopyRadialRopeCount) : 0;
        public int BigTopCanopyRingRopeCount => bigTopDressingEnabled ? Mathf.Max(0, bigTopCanopyRingRopeCount) : 0;
        public bool BigTopPerimeterPolesEnabled => bigTopDressingEnabled && bigTopPerimeterPolesEnabled;
        public int BigTopPerimeterPoleCount => bigTopDressingEnabled ? Mathf.Max(0, bigTopPerimeterPoleCount) : 0;
        public float BigTopPerimeterPoleRadiusPercent => Mathf.Clamp(bigTopPerimeterPoleRadiusPercent, 0.1f, 1f);
        public float BigTopPerimeterPoleRadius => Mathf.Max(0f, bigTopPerimeterPoleRadius);
        public float BigTopPerimeterPoleTopPadding => Mathf.Max(0f, bigTopPerimeterPoleTopPadding);
        public bool BigTopCurtainValanceEnabled => bigTopDressingEnabled && bigTopCurtainValanceEnabled;
        public float BigTopCurtainValanceHeight => Mathf.Max(0f, bigTopCurtainValanceHeight);
        public float BigTopCurtainValanceScallopDrop => Mathf.Max(0f, bigTopCurtainValanceScallopDrop);
        public int BigTopCurtainValanceScallopCount => bigTopDressingEnabled ? Mathf.Max(1, bigTopCurtainValanceScallopCount) : 0;
        public bool BigTopBuntingEnabled => bigTopDressingEnabled && bigTopBuntingEnabled;
        public int BigTopBuntingRingCount => BigTopBuntingEnabled ? Mathf.Max(0, bigTopBuntingRingCount) : 0;
        public int BigTopBuntingFlagsPerRing => BigTopBuntingEnabled ? Mathf.Max(0, bigTopBuntingFlagsPerRing) : 0;
        public float BigTopBuntingLowerDomePercent => Mathf.Clamp(Mathf.Min(bigTopBuntingLowerDomePercent, bigTopBuntingUpperDomePercent), 0f, 0.95f);
        public float BigTopBuntingUpperDomePercent => Mathf.Clamp(Mathf.Max(bigTopBuntingLowerDomePercent, bigTopBuntingUpperDomePercent), 0f, 0.95f);
        public float BigTopBuntingFlagWidth => Mathf.Max(0f, bigTopBuntingFlagWidth);
        public float BigTopBuntingFlagDrop => Mathf.Max(0f, bigTopBuntingFlagDrop);
        public float BigTopBuntingStringThickness => Mathf.Max(0f, bigTopBuntingStringThickness);
        public bool BigTopCurtainBannersEnabled => bigTopDressingEnabled && bigTopCurtainBannersEnabled;
        public int BigTopCurtainBannerCount => BigTopCurtainBannersEnabled ? Mathf.Max(0, bigTopCurtainBannerCount) : 0;
        public float BigTopCurtainBannerWidth => Mathf.Max(0f, bigTopCurtainBannerWidth);
        public float BigTopCurtainBannerHeight => Mathf.Max(0f, bigTopCurtainBannerHeight);
        public float BigTopCurtainBannerTopOffset => Mathf.Max(0f, bigTopCurtainBannerTopOffset);
        public float BigTopCurtainBannerInwardInset => Mathf.Max(0f, bigTopCurtainBannerInwardInset);

        private float RawBigTopShowLightSpeedMinHz => Mathf.Max(0f, bigTopShowLightSpeedMinHz);
        private float RawBigTopShowLightSpeedMaxHz => Mathf.Max(0f, bigTopShowLightSpeedMaxHz);
        private float RawBigTopShowLightColorDriftSpeedMinHz => Mathf.Max(0f, bigTopShowLightColorDriftSpeedMinHz);
        private float RawBigTopShowLightColorDriftSpeedMaxHz => Mathf.Max(0f, bigTopShowLightColorDriftSpeedMaxHz);
        private float RawWorldDressingGroundScaleMin => Mathf.Max(0.01f, worldDressingGroundScaleMin);
        private float RawWorldDressingGroundScaleMax => Mathf.Max(0.01f, worldDressingGroundScaleMax);
        private float RawWorldDressingWallWidthMin => Mathf.Max(0.01f, worldDressingWallWidthMin);
        private float RawWorldDressingWallWidthMax => Mathf.Max(0.01f, worldDressingWallWidthMax);
        private float RawWorldDressingWallHeightMin => Mathf.Max(0.01f, worldDressingWallHeightMin);
        private float RawWorldDressingWallHeightMax => Mathf.Max(0.01f, worldDressingWallHeightMax);
        private float RawWorldObstacleWidthMin => Mathf.Max(0.1f, worldObstacleWidthMin);
        private float RawWorldObstacleWidthMax => Mathf.Max(0.1f, worldObstacleWidthMax);
        private float RawWorldObstacleDepthMin => Mathf.Max(0.1f, worldObstacleDepthMin);
        private float RawWorldObstacleDepthMax => Mathf.Max(0.1f, worldObstacleDepthMax);
        private float RawWorldObstacleHeightMin => Mathf.Max(0.1f, worldObstacleHeightMin);
        private float RawWorldObstacleHeightMax => Mathf.Max(0.1f, worldObstacleHeightMax);
        private float RawWorldObstacleJumpableHeightMin => Mathf.Max(0.1f, worldObstacleJumpableHeightMin);
        private float RawWorldObstacleJumpableHeightMax => Mathf.Max(0.1f, worldObstacleJumpableHeightMax);
        private float RawWorldObstacleTallHeightMin => Mathf.Max(0.1f, worldObstacleTallHeightMin);
        private float RawWorldObstacleTallHeightMax => Mathf.Max(0.1f, worldObstacleTallHeightMax);

        public static RunWorldGenerationConfig CreateRuntimeDefault()
        {
            var config = CreateInstance<RunWorldGenerationConfig>();
            config.ApplyDefaultProfiles();
            return config;
        }

        public bool EnsureWorkflowDefaults()
        {
            bool changed = false;
            if (actProfiles == null || actProfiles.Count == 0)
            {
                ApplyDefaultProfiles();
                changed = true;
            }

            changed |= EnsureProfile(0, "Act I", 1, 256f, 32f, 4f, 16f, 12, 14, 36, 12, 28, 72, 24);
            changed |= EnsureProfile(1, "Act II", 2, 384f, 48f, 6f, 24f, 14, 20, 54, 16, 40, 108, 32);
            changed |= EnsureProfile(2, "Act III", 3, 512f, 60f, 8f, 32f, 16, 28, 72, 22, 56, 144, 44);

            if (!Enum.IsDefined(typeof(RunWorldTerrainMode), terrainMode))
            {
                terrainMode = RunWorldTerrainMode.StructuralGrammar;
                changed = true;
            }

            if (!Enum.IsDefined(typeof(RunWorldLayoutMode), defaultLayoutMode))
            {
                defaultLayoutMode = RunWorldLayoutMode.Generated;
                changed = true;
            }

            if (retryCount < 0)
            {
                retryCount = 3;
                changed = true;
            }

            if (minimumReachablePercent <= 0f)
            {
                minimumReachablePercent = 55f;
                changed = true;
            }

            if (rewardSafeMaxSlope <= 0f)
            {
                rewardSafeMaxSlope = 28f;
                changed = true;
            }

            if (spawnSafeMaxSlope <= 0f)
            {
                spawnSafeMaxSlope = 34f;
                changed = true;
            }

            if (chunkQuadsPerSide <= 0)
            {
                chunkQuadsPerSide = 16;
                changed = true;
            }

            if (terrainBuildChunksPerFrame <= 0)
            {
                terrainBuildChunksPerFrame = 2;
                changed = true;
            }

            changed |= EnsureMinimum(ref generatedTerrainSurfaceTextureResolution, 512, 128);
            changed |= EnsureMinimum(ref generatedTerrainSurfaceStyleVariationStrength, 0.55f, 0f);
            changed |= EnsureMinimum(ref generatedTerrainSurfaceColorfulness, 0.62f, 0f);
            changed |= EnsureMinimum(ref generatedTerrainSurfaceWearStrength, 0.68f, 0f);
            changed |= EnsureMinimum(ref generatedTerrainOrganicPatternScale, 1.65f, 0.25f);
            changed |= EnsureMinimum(ref generatedTerrainSurfaceZoneCount, 4, 1);
            changed |= EnsureMinimum(ref generatedTerrainSurfaceZonePaletteStrength, 0.45f, 0f);
            changed |= EnsureMinimum(ref generatedTerrainSurfaceZoneBlendSharpness, 1.15f, 0.25f);
            changed |= EnsureMinimum(ref generatedTerrainSurfaceGritStrength, 0.30f, 0f);
            changed |= EnsureMinimum(ref generatedTerrainSurfaceDirtStrength, 0.36f, 0f);
            changed |= EnsureMinimum(ref generatedTerrainSurfaceGridStrength, 0.26f, 0f);
            changed |= EnsureMinimum(ref generatedTerrainRampCueStrength, 0.38f, 0f);
            changed |= EnsureMinimum(ref generatedTerrainOrganicPatternStrength, 0.18f, 0f);
            changed |= EnsureMinimum(ref generatedTerrainOrganicPatternIterations, 64, 1);
            changed |= EnsureMinimum(ref generatedTerrainSurfaceReliefStrength, 1f, 0f);
            changed |= EnsureMinimum(ref generatedTerrainTopTileHeightVariation, 0.075f, 0f);
            changed |= EnsureMinimum(ref generatedTerrainTopTileReliefStrength, 1.45f, 0f);
            changed |= EnsureMinimum(ref generatedTerrainTopTileGroutDepthStrength, 1.35f, 0f);
            changed |= EnsureMinimum(ref generatedTerrainTopTileEdgeWearStrength, 0.55f, 0f);
            changed |= EnsureMinimum(ref generatedTerrainTopTileSmoothnessVariationStrength, 1.20f, 0f);
            changed |= EnsureMinimum(ref generatedTerrainWallPanelHeightVariation, 0.065f, 0f);
            changed |= EnsureMinimum(ref generatedTerrainRampPlankSeamStrength, 1.35f, 0f);
            changed |= EnsureMinimum(ref generatedTerrainRampEdgeWearStrength, 0.60f, 0f);
            changed |= EnsureMinimum(ref generatedTerrainRampWoodGrainStrength, 1.20f, 0f);
            changed |= EnsureMinimum(ref generatedTerrainWallPanelSeamStrength, 1.35f, 0f);
            changed |= EnsureMinimum(ref generatedTerrainWallEdgeWearStrength, 0.55f, 0f);
            changed |= EnsureMinimum(ref generatedTerrainWallVerticalWearStrength, 1.15f, 0f);
            changed |= EnsureMinimum(ref generatedTerrainTopSmoothnessMin, 0.08f, 0f);
            changed |= EnsureMinimum(ref generatedTerrainTopSmoothnessMax, 0.24f, 0f);
            changed |= EnsureMinimum(ref generatedTerrainWallSmoothnessMin, 0.05f, 0f);
            changed |= EnsureMinimum(ref generatedTerrainWallSmoothnessMax, 0.18f, 0f);
            changed |= EnsureMinimum(ref generatedTerrainSurfaceSmoothnessVariationStrength, 1f, 0f);
            changed |= EnsureMinimum(ref generatedTerrainOrganicTextureScale, 0.45f, 0.01f);
            changed |= EnsureDefaultColor(ref generatedTerrainTopColor, new Color(0.055f, 0.06f, 0.065f, 1f));
            changed |= EnsureDefaultColor(ref generatedTerrainWallColor, new Color(0.22f, 0.035f, 0.045f, 1f));
            changed |= EnsureDefaultColor(ref generatedTerrainRampColor, new Color(0.22f, 0.12f, 0.055f, 1f));
            changed |= EnsureDefaultColor(ref generatedTerrainRimColor, new Color(0.12f, 0.075f, 0.06f, 1f));
            changed |= EnsureMinimum(ref worldDressingGroundDecalsPer100k, 46f, 0f);
            changed |= EnsureMinimum(ref worldDressingGroundDecalMaxCount, 420, 0);
            changed |= EnsureMinimum(ref worldDressingWallDecalsPer100k, 22f, 0f);
            changed |= EnsureMinimum(ref worldDressingWallDecalMaxCount, 220, 0);
            changed |= EnsureMinimum(ref worldDressingPlayerStartExclusionRadius, 18f, 0f);
            changed |= EnsureMinimum(ref worldDressingRewardExclusionRadius, 8f, 0f);
            changed |= EnsureMinimum(ref worldDressingArenaEdgePadding, 14f, 0f);
            changed |= EnsureMinimum(ref worldDressingGroundSurfaceOffset, 0.045f, 0f);
            changed |= EnsureMinimum(ref worldDressingGroundScaleMin, 2.4f, 0.01f);
            changed |= EnsureMinimum(ref worldDressingGroundScaleMax, 8.5f, 0.01f);
            changed |= EnsureMinimum(ref worldDressingGroundOpacity, 0.58f, 0f);
            changed |= EnsureMinimum(ref worldDressingWallSurfaceOffset, 0.055f, 0f);
            changed |= EnsureMinimum(ref worldDressingWallWidthMin, 2.5f, 0.01f);
            changed |= EnsureMinimum(ref worldDressingWallWidthMax, 8.5f, 0.01f);
            changed |= EnsureMinimum(ref worldDressingWallHeightMin, 1.4f, 0.01f);
            changed |= EnsureMinimum(ref worldDressingWallHeightMax, 7f, 0.01f);
            changed |= EnsureMinimum(ref worldDressingWallOpacity, 0.72f, 0f);
            changed |= EnsureMinimum(ref worldDressingTextureResolution, 256, 64);
            changed |= EnsureMinimum(ref worldDressingBuildItemsPerFrame, 64, 1);
            changed |= EnsureMinimum(ref worldDecorationDensityMultiplier, 1f, 0f);
            changed |= EnsureMinimum(ref worldDecorationMaxCount, 360, 0);
            changed |= EnsureMinimum(ref worldDecorationReferenceRadius, 512f, 1f);
            changed |= EnsureMinimum(ref worldDecorationPlayerStartExclusionRadius, 22f, 0f);
            changed |= EnsureMinimum(ref worldDecorationRewardExclusionRadius, 9f, 0f);
            changed |= EnsureMinimum(ref worldDecorationStageDoorExclusionRadius, 18f, 0f);
            changed |= EnsureMinimum(ref worldDecorationArenaEdgePadding, 14f, 0f);
            changed |= EnsureMinimum(ref worldDecorationSurfaceOffset, 0.035f, 0f);
            changed |= EnsureMinimum(ref worldDecorationBuildItemsPerFrame, 32, 1);
            changed |= EnsureMinimum(ref worldObstacleCountPer100k, 48f, 0f);
            changed |= EnsureMinimum(ref worldObstacleMaxCount, 216, 0);
            changed |= EnsureMinimum(ref worldObstacleMinSpacing, 8f, 0f);
            changed |= EnsureMinimum(ref worldObstaclePlayerStartExclusionRadius, 28f, 0f);
            changed |= EnsureMinimum(ref worldObstacleRewardExclusionRadius, 2.5f, 0f);
            changed |= EnsureMinimum(ref worldObstacleArenaEdgePadding, 18f, 0f);
            changed |= EnsureMinimum(ref worldObstacleMaxSlope, 8f, 0f);
            changed |= EnsureMinimum(ref worldObstacleSurfaceOffset, 0.035f, 0f);
            changed |= EnsureMinimum(ref worldObstacleWidthMin, 2.2f, 0.1f);
            changed |= EnsureMinimum(ref worldObstacleWidthMax, 19.5f, 0.1f);
            changed |= EnsureMinimum(ref worldObstacleDepthMin, 2.2f, 0.1f);
            changed |= EnsureMinimum(ref worldObstacleDepthMax, 16.5f, 0.1f);
            changed |= EnsureMinimum(ref worldObstacleHeightMin, 1.3f, 0.1f);
            changed |= EnsureMinimum(ref worldObstacleHeightMax, 5.5f, 0.1f);
            changed |= EnsureMinimum(ref worldObstacleJumpableHeightMin, 0.7f, 0.1f);
            changed |= EnsureMinimum(ref worldObstacleJumpableHeightMax, 1.35f, 0.1f);
            changed |= EnsureMinimum(ref worldObstacleTallHeightMin, 9f, 0.1f);
            changed |= EnsureMinimum(ref worldObstacleTallHeightMax, 60f, 0.1f);
            changed |= EnsureMinimum(ref worldObstacleTextureResolution, 128, 64);
            changed |= EnsureMinimum(ref worldObstacleBuildItemsPerFrame, 12, 1);

            if (worldLoadTimingSlowThresholdMs <= 0f)
            {
                worldLoadTimingSlowThresholdMs = 16.7f;
                changed = true;
            }

            if (generatedChestSpacing < 0f)
            {
                generatedChestSpacing = 18f;
                changed = true;
            }

            if (generatedTicketDepositSpacing < 0f)
            {
                generatedTicketDepositSpacing = 10f;
                changed = true;
            }

            if (generatedHealingPropSpacing < 0f)
            {
                generatedHealingPropSpacing = 14f;
                changed = true;
            }

            if (generatedLooseChestSpacing < 0f)
            {
                generatedLooseChestSpacing = 12f;
                changed = true;
            }

            if (generatedLooseTicketDepositSpacing < 0f)
            {
                generatedLooseTicketDepositSpacing = 7f;
                changed = true;
            }

            if (generatedLooseHealingPropSpacing < 0f)
            {
                generatedLooseHealingPropSpacing = 9f;
                changed = true;
            }

            if (enemySpawnSafeSearchRadius <= 0f)
            {
                enemySpawnSafeSearchRadius = 32f;
                changed = true;
            }

            if (rewardSpreadWeight < 0f)
            {
                rewardSpreadWeight = 0.65f;
                changed = true;
            }

            changed |= EnsureMinimum(ref arenaBarrierHeight, 22f, 0.1f);
            changed |= EnsureMinimum(ref arenaBarrierSegmentCount, 96, 12);
            changed |= EnsureMinimum(ref arenaBarrierCollisionThickness, 4f, 0.01f);
            changed |= EnsureMinimum(ref arenaBarrierCollisionDepthBelowGround, 64f, 0f);
            changed |= EnsureMinimum(ref arenaBarrierCollisionTopPadding, 64f, 0f);
            changed |= EnsureMinimum(ref arenaBarrierCollisionMinimumTop, 128f, 1f);
            changed |= EnsureMinimum(ref arenaBarrierLineThickness, 0.16f, 0.001f);
            changed |= EnsureMinimum(ref arenaBarrierBaseRingThickness, 0.45f, 0.001f);
            changed |= EnsureMinimum(ref arenaBarrierDynamicRingCount, 5, 1);
            changed |= EnsureMinimum(ref arenaBarrierDynamicRingSpeed, 3.4f, 0f);
            changed |= EnsureMinimum(ref arenaBarrierDynamicRingThickness, 0.12f, 0.001f);
            changed |= EnsureEaseDefault(ref arenaBarrierDynamicRingFadeEase, EaseSettings.OutCubic);
            changed |= EnsureMinimum(ref bigTopRadiusPadding, 24f, 0f);
            changed |= EnsureMinimum(ref bigTopCurtainHeight, 72f, 1f);
            changed |= EnsureMinimum(ref bigTopGroundSkirtInset, 28f, 0f);
            changed |= EnsureMinimum(ref bigTopDomeHeightPercent, 0.34f, 0.05f);
            changed |= EnsureMinimum(ref bigTopMinimumDomeHeight, 120f, 1f);
            changed |= EnsureMinimum(ref bigTopSegmentCount, 128, 16);
            changed |= EnsureMinimum(ref bigTopCanopyRingCount, 8, 2);
            changed |= EnsureMinimum(ref bigTopTextureResolution, 1024, 128);
            changed |= EnsureMinimum(ref bigTopStripeCount, 24, 2);
            changed |= EnsureMinimum(ref bigTopBrightness, 0.92f, 0f);
            changed |= EnsureDefaultColor(ref bigTopRedStripeColor, new Color(0.66f, 0.055f, 0.04f, 1f));
            changed |= EnsureDefaultColor(ref bigTopCanvasStripeColor, new Color(0.82f, 0.69f, 0.48f, 1f));
            changed |= EnsureMinimum(ref bigTopPerimeterFogInnerRadiusPercent, 0.68f, 0.1f);
            changed |= EnsureMinimum(ref bigTopPerimeterFogOuterInset, 5f, 0f);
            changed |= EnsureMinimum(ref bigTopPerimeterFogHeight, 18f, 0.05f);
            changed |= EnsureMinimum(ref bigTopPerimeterFogSegmentCount, 96, 16);
            changed |= EnsureDefaultColor(ref bigTopPerimeterFogColor, new Color(0.42f, 0.34f, 0.24f, 1f));
            changed |= EnsureMinimum(ref bigTopPerimeterFogBottomOpacity, 0.18f, 0f);
            changed |= EnsureMinimum(ref bigTopPerimeterFogTopOpacity, 0.02f, 0f);
            changed |= EnsureMinimum(ref bigTopPerimeterFogNoiseStrength, 0.32f, 0f);
            changed |= EnsureMinimum(ref bigTopGodRayCount, 12, 0);
            changed |= EnsureDefaultColor(ref bigTopGodRayColor, new Color(1f, 0.74f, 0.34f, 1f));
            changed |= EnsureMinimum(ref bigTopGodRayOpacity, 0.16f, 0f);
            changed |= EnsureMinimum(ref bigTopGodRayStartDomePercent, 0.18f, 0f);
            changed |= EnsureMinimum(ref bigTopGodRayStartRadiusMinPercent, 0.55f, 0f);
            changed |= EnsureMinimum(ref bigTopGodRayStartRadiusMaxPercent, 0.95f, 0f);
            changed |= EnsureMinimum(ref bigTopGodRayEndRadiusPercent, 0.18f, 0f);
            changed |= EnsureMinimum(ref bigTopGodRayEndHeight, 8f, 0f);
            changed |= EnsureMinimum(ref bigTopGodRayTopWidthPercent, 0.018f, 0.001f);
            changed |= EnsureMinimum(ref bigTopGodRayBottomWidthPercent, 0.065f, 0.001f);
            changed |= EnsureMinimum(ref bigTopGodRayFlickerSpeed, 0.22f, 0f);
            changed |= EnsureMinimum(ref bigTopGodRayBottomFadePower, 1.8f, 0.1f);
            changed |= EnsureMinimum(ref bigTopGodRayEdgeFadePower, 1.75f, 0.1f);
            changed |= EnsureMinimum(ref bigTopGodRayTextureNoiseStrength, 0.12f, 0f);
            changed |= EnsureMinimum(ref bigTopGodRayFixtureLength, 4.5f, 0f);
            changed |= EnsureMinimum(ref bigTopGodRayFixtureRadius, 1.45f, 0f);
            changed |= EnsureMinimum(ref bigTopGodRayFixtureLensRadius, 1.15f, 0f);
            changed |= EnsureMinimum(ref bigTopGodRayFixtureBeamCoverage, 1.3f, 1f);
            changed |= EnsureDefaultColor(ref bigTopGodRayFixtureColor, new Color(0.12f, 0.075f, 0.045f, 1f));
            changed |= EnsureDefaultColor(ref bigTopGodRayFixtureLensColor, new Color(1f, 0.78f, 0.38f, 1f));
            changed |= EnsureMinimum(ref bigTopGodRayFixtureLensEmissionStrength, 2.7f, 0f);
            changed |= EnsureMinimum(ref bigTopShowLightCount, 6, 0);
            changed |= EnsureMinimum(ref bigTopShowLightSweepDegrees, 16f, 0f);
            changed |= EnsureMinimum(ref bigTopShowLightRadiusDriftPercent, 0.08f, 0f);
            changed |= EnsureMinimum(ref bigTopShowLightSpeedMinHz, 0.018f, 0f);
            changed |= EnsureMinimum(ref bigTopShowLightSpeedMaxHz, 0.045f, 0f);
            changed |= EnsureMinimum(ref bigTopShowLightMeshRefreshInterval, 0.08f, 0.02f);
            changed |= EnsureDefaultColor(ref bigTopShowLightAccentPinkColor, new Color(1f, 0.68f, 0.82f, 1f));
            changed |= EnsureDefaultColor(ref bigTopShowLightAccentTealColor, new Color(0.62f, 0.95f, 1f, 1f));
            changed |= EnsureMinimum(ref bigTopShowLightAccentStrength, 0.08f, 0f);
            changed |= EnsureMinimum(ref bigTopShowLightColorDriftStrength, 0.05f, 0f);
            changed |= EnsureMinimum(ref bigTopShowLightColorDriftSpeedMinHz, 0.006f, 0f);
            changed |= EnsureMinimum(ref bigTopShowLightColorDriftSpeedMaxHz, 0.018f, 0f);
            changed |= EnsureMinimum(ref bigTopBulbRingCount, 3, 0);
            changed |= EnsureMinimum(ref bigTopBulbsPerRing, 56, 0);
            changed |= EnsureMinimum(ref bigTopBulbRadialStringCount, 12, 0);
            changed |= EnsureMinimum(ref bigTopBulbsPerRadialString, 7, 0);
            changed |= EnsureMinimum(ref bigTopBulbRadius, 0.75f, 0f);
            changed |= EnsureDefaultColor(ref bigTopBulbColor, new Color(1f, 0.83f, 0.48f, 1f));
            changed |= EnsureMinimum(ref bigTopBulbEmissionStrength, 2.2f, 0f);
            changed |= EnsureMinimum(ref bigTopBulbFlickerSpeed, 0.55f, 0f);
            changed |= EnsureMinimum(ref bigTopCenterHubRadiusPercent, 0.018f, 0.001f);
            changed |= EnsureMinimum(ref bigTopCenterHubTubeRadius, 0.22f, 0f);
            changed |= EnsureMinimum(ref bigTopCenterHubCordCount, 6, 0);
            changed |= EnsureMinimum(ref bigTopCenterHubCordDrop, 12f, 0f);
            changed |= EnsureMinimum(ref bigTopCanopyRopeThickness, 0.08f, 0f);
            changed |= EnsureMinimum(ref bigTopCanopyRadialRopeCount, 12, 0);
            changed |= EnsureMinimum(ref bigTopCanopyRingRopeCount, 3, 0);
            changed |= EnsureMinimum(ref bigTopPerimeterPoleCount, 16, 0);
            changed |= EnsureMinimum(ref bigTopPerimeterPoleRadiusPercent, 0.93f, 0.1f);
            changed |= EnsureMinimum(ref bigTopPerimeterPoleRadius, 0.55f, 0f);
            changed |= EnsureMinimum(ref bigTopPerimeterPoleTopPadding, 4f, 0f);
            changed |= EnsureMinimum(ref bigTopCurtainValanceHeight, 3.5f, 0f);
            changed |= EnsureMinimum(ref bigTopCurtainValanceScallopDrop, 1.8f, 0f);
            changed |= EnsureMinimum(ref bigTopCurtainValanceScallopCount, 32, 1);
            changed |= EnsureMinimum(ref bigTopBuntingRingCount, 2, 0);
            changed |= EnsureMinimum(ref bigTopBuntingFlagsPerRing, 48, 0);
            changed |= EnsureMinimum(ref bigTopBuntingLowerDomePercent, 0.10f, 0f);
            changed |= EnsureMinimum(ref bigTopBuntingUpperDomePercent, 0.34f, 0f);
            changed |= EnsureMinimum(ref bigTopBuntingFlagWidth, 3.2f, 0f);
            changed |= EnsureMinimum(ref bigTopBuntingFlagDrop, 2.4f, 0f);
            changed |= EnsureMinimum(ref bigTopBuntingStringThickness, 0.055f, 0f);
            changed |= EnsureMinimum(ref bigTopCurtainBannerCount, 16, 0);
            changed |= EnsureMinimum(ref bigTopCurtainBannerWidth, 12f, 0f);
            changed |= EnsureMinimum(ref bigTopCurtainBannerHeight, 14f, 0f);
            changed |= EnsureMinimum(ref bigTopCurtainBannerTopOffset, 14f, 0f);
            changed |= EnsureMinimum(ref bigTopCurtainBannerInwardInset, 8f, 0f);
            changed |= EnsureDefaultColor(ref bigTopDressingRopeColor, new Color(0.46f, 0.30f, 0.16f, 1f));
            changed |= EnsureDefaultColor(ref bigTopDressingPoleColor, new Color(0.22f, 0.14f, 0.08f, 1f));
            changed |= EnsureDefaultColor(ref bigTopCurtainValanceColor, new Color(0.42f, 0.02f, 0.025f, 1f));
            changed |= EnsureDefaultColor(ref bigTopBuntingStringColor, new Color(0.58f, 0.39f, 0.18f, 1f));
            changed |= EnsureDefaultColor(ref bigTopBuntingPrimaryColor, new Color(0.78f, 0.04f, 0.035f, 1f));
            changed |= EnsureDefaultColor(ref bigTopBuntingSecondaryColor, new Color(0.92f, 0.67f, 0.20f, 1f));
            changed |= EnsureDefaultColor(ref bigTopCurtainBannerColor, new Color(0.48f, 0.025f, 0.035f, 1f));
            changed |= EnsureDefaultColor(ref bigTopCurtainBannerTrimColor, new Color(0.86f, 0.58f, 0.18f, 1f));

            return changed;
        }

        public void ApplyDefaultProfiles()
        {
            actProfiles = CreateDefaultProfiles();
            useDebugSeedOverride = false;
            debugSeedOverride = 12345;
            terrainMode = RunWorldTerrainMode.StructuralGrammar;
            defaultLayoutMode = RunWorldLayoutMode.Generated;
            retryCount = 3;
            fallbackToFlatWorld = true;
            minimumReachablePercent = 55f;
            rewardSafeMaxSlope = 28f;
            spawnSafeMaxSlope = 34f;
            centerBiasEnabled = true;
            highGroundBiasEnabled = true;
            playerStartCenterWeight = 1f;
            rewardCenterWeight = 0.35f;
            rewardHighGroundWeight = 0.25f;
            rewardSpreadWeight = 0.65f;
            generatedChestSpacing = 18f;
            generatedTicketDepositSpacing = 10f;
            generatedHealingPropSpacing = 14f;
            generatedLooseChestSpacing = 12f;
            generatedLooseTicketDepositSpacing = 7f;
            generatedLooseHealingPropSpacing = 9f;
            enemySpawnSafeSearchRadius = 32f;
            runtimeMeshTerrainEnabled = true;
            hidePrototypeArenaWhenGenerated = true;
            chunkQuadsPerSide = 16;
            terrainBuildChunksPerFrame = 2;
            generatedTerrainSurfaceTexturesEnabled = true;
            generatedTerrainSurfaceStyleMode = GeneratedWorldSurfaceStyleMode.SeededCircus;
            generatedTerrainSurfaceStyleVariationStrength = 0.55f;
            generatedTerrainSurfaceColorfulness = 0.62f;
            generatedTerrainSurfaceWearStrength = 0.68f;
            generatedTerrainOrganicPatternScale = 1.65f;
            generatedTerrainSurfaceZoneVarietyEnabled = true;
            generatedTerrainSurfaceZoneCount = 4;
            generatedTerrainSurfaceZonePaletteStrength = 0.45f;
            generatedTerrainSurfaceZoneBlendSharpness = 1.15f;
            generatedTerrainSurfaceTextureResolution = 512;
            generatedTerrainSurfaceGritStrength = 0.30f;
            generatedTerrainSurfaceDirtStrength = 0.36f;
            generatedTerrainSurfaceGridStrength = 0.26f;
            generatedTerrainRampCueStrength = 0.38f;
            generatedTerrainOrganicPatternStrength = 0.18f;
            generatedTerrainOrganicPatternIterations = 64;
            generatedTerrainSurfaceReliefStrength = 1f;
            generatedTerrainTopTileHeightVariation = 0.075f;
            generatedTerrainTopTileReliefStrength = 1.45f;
            generatedTerrainTopTileGroutDepthStrength = 1.35f;
            generatedTerrainTopTileEdgeWearStrength = 0.55f;
            generatedTerrainTopTileSmoothnessVariationStrength = 1.20f;
            generatedTerrainWallPanelHeightVariation = 0.065f;
            generatedTerrainRampPlankSeamStrength = 1.35f;
            generatedTerrainRampEdgeWearStrength = 0.60f;
            generatedTerrainRampWoodGrainStrength = 1.20f;
            generatedTerrainWallPanelSeamStrength = 1.35f;
            generatedTerrainWallEdgeWearStrength = 0.55f;
            generatedTerrainWallVerticalWearStrength = 1.15f;
            generatedTerrainTopSmoothnessMin = 0.08f;
            generatedTerrainTopSmoothnessMax = 0.24f;
            generatedTerrainWallSmoothnessMin = 0.05f;
            generatedTerrainWallSmoothnessMax = 0.18f;
            generatedTerrainSurfaceSmoothnessVariationStrength = 1f;
            generatedTerrainOrganicTextureScale = 0.45f;
            generatedTerrainTopColor = new Color(0.055f, 0.06f, 0.065f, 1f);
            generatedTerrainWallColor = new Color(0.22f, 0.035f, 0.045f, 1f);
            generatedTerrainRampColor = new Color(0.22f, 0.12f, 0.055f, 1f);
            generatedTerrainRimColor = new Color(0.12f, 0.075f, 0.06f, 1f);
            worldDressingEnabled = true;
            worldDressingGroundDecalsPer100k = 46f;
            worldDressingGroundDecalMaxCount = 420;
            worldDressingWallDecalsPer100k = 22f;
            worldDressingWallDecalMaxCount = 220;
            worldDressingPlayerStartExclusionRadius = 18f;
            worldDressingRewardExclusionRadius = 8f;
            worldDressingArenaEdgePadding = 14f;
            worldDressingRampCenterSkipChance = 0.72f;
            worldDressingGroundSurfaceOffset = 0.045f;
            worldDressingGroundScaleMin = 2.4f;
            worldDressingGroundScaleMax = 8.5f;
            worldDressingGroundOpacity = 0.58f;
            worldDressingWallSurfaceOffset = 0.055f;
            worldDressingWallWidthMin = 2.5f;
            worldDressingWallWidthMax = 8.5f;
            worldDressingWallHeightMin = 1.4f;
            worldDressingWallHeightMax = 7f;
            worldDressingWallOpacity = 0.72f;
            worldDressingTextureResolution = 256;
            worldDressingBuildItemsPerFrame = 64;
            worldDressingSeedOffset = 92001;
            worldDecorationEnabled = true;
            worldDecorationDensityMultiplier = 1f;
            worldDecorationMaxCount = 360;
            worldDecorationReferenceRadius = 512f;
            worldDecorationPlayerStartExclusionRadius = 22f;
            worldDecorationRewardExclusionRadius = 9f;
            worldDecorationStageDoorExclusionRadius = 18f;
            worldDecorationArenaEdgePadding = 14f;
            worldDecorationRampCenterSkipChance = 0.88f;
            worldDecorationSurfaceOffset = 0.035f;
            worldDecorationBuildItemsPerFrame = 32;
            worldDecorationSeedOffset = 118003;
            worldObstacleEnabled = true;
            worldObstacleCountPer100k = 48f;
            worldObstacleMaxCount = 216;
            worldObstacleMinSpacing = 8f;
            worldObstaclePlayerStartExclusionRadius = 28f;
            worldObstacleRewardExclusionRadius = 2.5f;
            worldObstacleArenaEdgePadding = 18f;
            worldObstacleMaxSlope = 8f;
            worldObstacleSurfaceOffset = 0.035f;
            worldObstacleMaxTiltDegrees = 4f;
            worldObstacleWidthMin = 2.2f;
            worldObstacleWidthMax = 19.5f;
            worldObstacleDepthMin = 2.2f;
            worldObstacleDepthMax = 16.5f;
            worldObstacleHeightMin = 1.3f;
            worldObstacleHeightMax = 5.5f;
            worldObstacleJumpableHeightMin = 0.7f;
            worldObstacleJumpableHeightMax = 1.35f;
            worldObstacleTallHeightMin = 9f;
            worldObstacleTallHeightMax = 60f;
            worldObstacleJumpableChance = 0.32f;
            worldObstacleTallChance = 0.38f;
            worldObstacleColumnChance = 0.42f;
            worldObstacleTextureResolution = 128;
            worldObstacleBuildItemsPerFrame = 12;
            worldObstacleSeedOffset = 43111;
            worldLoadTimingDiagnosticsEnabled = false;
            worldLoadTimingSlowThresholdMs = 16.7f;
            worldLoadTimingLogAllStages = false;
            arenaBarrierEnabled = true;
            arenaBarrierRadiusPadding = 0f;
            arenaBarrierHeight = 22f;
            arenaBarrierSegmentCount = 96;
            arenaBarrierCollisionEnabled = true;
            arenaBarrierCollisionThickness = 4f;
            arenaBarrierCollisionDepthBelowGround = 64f;
            arenaBarrierCollisionTopPadding = 64f;
            arenaBarrierCollisionMinimumTop = 128f;
            arenaBarrierLineThickness = 0.16f;
            arenaBarrierBottomColor = new Color(0.22f, 0.86f, 1f, 0.34f);
            arenaBarrierTopColor = new Color(0.74f, 0.32f, 1f, 0.04f);
            arenaBarrierBaseRingEnabled = true;
            arenaBarrierBaseRingThickness = 0.45f;
            arenaBarrierBaseRingColor = new Color(0.18f, 0.84f, 1f, 0.48f);
            arenaBarrierDynamicRingsEnabled = true;
            arenaBarrierDynamicRingCount = 5;
            arenaBarrierDynamicRingSpeed = 3.4f;
            arenaBarrierDynamicRingThickness = 0.12f;
            arenaBarrierDynamicRingColor = new Color(0.62f, 0.92f, 1f, 0.32f);
            arenaBarrierDynamicRingFadeEase = EaseSettings.OutCubic;
            bigTopEnvironmentEnabled = true;
            bigTopRadiusPadding = 24f;
            bigTopCurtainHeight = 72f;
            bigTopGroundSkirtInset = 28f;
            bigTopDomeHeightPercent = 0.34f;
            bigTopMinimumDomeHeight = 120f;
            bigTopSegmentCount = 128;
            bigTopCanopyRingCount = 8;
            bigTopTextureResolution = 1024;
            bigTopStripeCount = 24;
            bigTopRedStripeColor = new Color(0.66f, 0.055f, 0.04f, 1f);
            bigTopCanvasStripeColor = new Color(0.82f, 0.69f, 0.48f, 1f);
            bigTopBrightness = 0.92f;
            bigTopSeamStrength = 0.42f;
            bigTopWeaveStrength = 0.18f;
            bigTopGritStrength = 0.28f;
            bigTopLowerDirtStrength = 0.36f;
            bigTopPerimeterFogEnabled = true;
            bigTopPerimeterFogInnerRadiusPercent = 0.68f;
            bigTopPerimeterFogOuterInset = 5f;
            bigTopPerimeterFogHeight = 18f;
            bigTopPerimeterFogSegmentCount = 96;
            bigTopPerimeterFogColor = new Color(0.42f, 0.34f, 0.24f, 1f);
            bigTopPerimeterFogBottomOpacity = 0.18f;
            bigTopPerimeterFogTopOpacity = 0.02f;
            bigTopPerimeterFogNoiseStrength = 0.32f;
            bigTopGodRaysEnabled = true;
            bigTopGodRayCount = 12;
            bigTopGodRayColor = new Color(1f, 0.74f, 0.34f, 1f);
            bigTopGodRayOpacity = 0.16f;
            bigTopGodRayStartDomePercent = 0.18f;
            bigTopGodRayStartRadiusMinPercent = 0.55f;
            bigTopGodRayStartRadiusMaxPercent = 0.95f;
            bigTopGodRayEndRadiusPercent = 0.18f;
            bigTopGodRayEndHeight = 8f;
            bigTopGodRayTopWidthPercent = 0.018f;
            bigTopGodRayBottomWidthPercent = 0.065f;
            bigTopGodRayAngleJitter = 0.14f;
            bigTopGodRayFlickerAmount = 0.08f;
            bigTopGodRayFlickerSpeed = 0.22f;
            bigTopGodRayBottomFadePower = 1.8f;
            bigTopGodRayEdgeFadePower = 1.75f;
            bigTopGodRayTextureNoiseStrength = 0.12f;
            bigTopGodRayFixturesEnabled = true;
            bigTopGodRayFixtureLength = 4.5f;
            bigTopGodRayFixtureRadius = 1.45f;
            bigTopGodRayFixtureLensRadius = 1.15f;
            bigTopGodRayFixtureBeamCoverage = 1.3f;
            bigTopGodRayFixtureColor = new Color(0.12f, 0.075f, 0.045f, 1f);
            bigTopGodRayFixtureLensColor = new Color(1f, 0.78f, 0.38f, 1f);
            bigTopGodRayFixtureLensEmissionStrength = 2.7f;
            bigTopShowLightsEnabled = true;
            bigTopShowLightCount = 6;
            bigTopShowLightSweepDegrees = 16f;
            bigTopShowLightRadiusDriftPercent = 0.08f;
            bigTopShowLightSpeedMinHz = 0.018f;
            bigTopShowLightSpeedMaxHz = 0.045f;
            bigTopShowLightMeshRefreshInterval = 0.08f;
            bigTopShowLightColorVariationEnabled = true;
            bigTopShowLightAccentPinkColor = new Color(1f, 0.68f, 0.82f, 1f);
            bigTopShowLightAccentTealColor = new Color(0.62f, 0.95f, 1f, 1f);
            bigTopShowLightAccentStrength = 0.08f;
            bigTopShowLightColorDriftStrength = 0.05f;
            bigTopShowLightColorDriftSpeedMinHz = 0.006f;
            bigTopShowLightColorDriftSpeedMaxHz = 0.018f;
            bigTopBulbStringsEnabled = true;
            bigTopBulbRingCount = 3;
            bigTopBulbsPerRing = 56;
            bigTopBulbRadialStringCount = 12;
            bigTopBulbsPerRadialString = 7;
            bigTopBulbRadius = 0.75f;
            bigTopBulbColor = new Color(1f, 0.83f, 0.48f, 1f);
            bigTopBulbEmissionStrength = 2.2f;
            bigTopBulbFlickerAmount = 0.10f;
            bigTopBulbFlickerSpeed = 0.55f;
            bigTopDressingEnabled = true;
            bigTopCenterHubEnabled = true;
            bigTopCenterHubRadiusPercent = 0.018f;
            bigTopCenterHubTubeRadius = 0.22f;
            bigTopCenterHubCordCount = 6;
            bigTopCenterHubCordDrop = 12f;
            bigTopCanopyRopesEnabled = true;
            bigTopCanopyRopeThickness = 0.08f;
            bigTopCanopyRadialRopeCount = 12;
            bigTopCanopyRingRopeCount = 3;
            bigTopPerimeterPolesEnabled = true;
            bigTopPerimeterPoleCount = 16;
            bigTopPerimeterPoleRadiusPercent = 0.93f;
            bigTopPerimeterPoleRadius = 0.55f;
            bigTopPerimeterPoleTopPadding = 4f;
            bigTopCurtainValanceEnabled = true;
            bigTopCurtainValanceHeight = 3.5f;
            bigTopCurtainValanceScallopDrop = 1.8f;
            bigTopCurtainValanceScallopCount = 32;
            bigTopBuntingEnabled = true;
            bigTopBuntingRingCount = 2;
            bigTopBuntingFlagsPerRing = 48;
            bigTopBuntingLowerDomePercent = 0.10f;
            bigTopBuntingUpperDomePercent = 0.34f;
            bigTopBuntingFlagWidth = 3.2f;
            bigTopBuntingFlagDrop = 2.4f;
            bigTopBuntingStringThickness = 0.055f;
            bigTopCurtainBannersEnabled = true;
            bigTopCurtainBannerCount = 16;
            bigTopCurtainBannerWidth = 12f;
            bigTopCurtainBannerHeight = 14f;
            bigTopCurtainBannerTopOffset = 14f;
            bigTopCurtainBannerInwardInset = 8f;
            bigTopDressingRopeColor = new Color(0.46f, 0.30f, 0.16f, 1f);
            bigTopDressingPoleColor = new Color(0.22f, 0.14f, 0.08f, 1f);
            bigTopCurtainValanceColor = new Color(0.42f, 0.02f, 0.025f, 1f);
            bigTopBuntingStringColor = new Color(0.58f, 0.39f, 0.18f, 1f);
            bigTopBuntingPrimaryColor = new Color(0.78f, 0.04f, 0.035f, 1f);
            bigTopBuntingSecondaryColor = new Color(0.92f, 0.67f, 0.20f, 1f);
            bigTopCurtainBannerColor = new Color(0.48f, 0.025f, 0.035f, 1f);
            bigTopCurtainBannerTrimColor = new Color(0.86f, 0.58f, 0.18f, 1f);
        }

        public RunWorldGenerationProfile ProfileForAct(int actNumber)
        {
            EnsureWorkflowDefaults();
            int safeAct = Mathf.Max(1, actNumber);
            for (int i = 0; i < actProfiles.Count; i++)
            {
                RunWorldGenerationProfile profile = actProfiles[i];
                if (profile != null && profile.ActNumber == safeAct)
                {
                    return profile;
                }
            }

            return actProfiles[Mathf.Clamp(safeAct - 1, 0, actProfiles.Count - 1)];
        }

        private static List<RunWorldGenerationProfile> CreateDefaultProfiles()
        {
            return new List<RunWorldGenerationProfile>
            {
                CreateProfile("Act I", 1, 256f, 32f, 4f, 16f, 12, 14, 36, 12, 28, 72, 24),
                CreateProfile("Act II", 2, 384f, 48f, 6f, 24f, 14, 20, 54, 16, 40, 108, 32),
                CreateProfile("Act III", 3, 512f, 60f, 8f, 32f, 16, 28, 72, 22, 56, 144, 44)
            };
        }

        private bool EnsureProfile(
            int index,
            string label,
            int actNumber,
            float radius,
            float maxHeight,
            float spacing,
            float boundaryMargin,
            int stampCount,
            int chestCount,
            int ticketDepositCount,
            int healingPropCount,
            int looseChestCount,
            int looseTicketDepositCount,
            int looseHealingPropCount)
        {
            actProfiles ??= new List<RunWorldGenerationProfile>();
            bool changed = false;
            while (actProfiles.Count <= index)
            {
                actProfiles.Add(CreateProfile(
                    label,
                    actNumber,
                    radius,
                    maxHeight,
                    spacing,
                    boundaryMargin,
                    stampCount,
                    chestCount,
                    ticketDepositCount,
                    healingPropCount,
                    looseChestCount,
                    looseTicketDepositCount,
                    looseHealingPropCount));
                changed = true;
            }

            RunWorldGenerationProfile profile = actProfiles[index];
            if (profile == null)
            {
                actProfiles[index] = CreateProfile(
                    label,
                    actNumber,
                    radius,
                    maxHeight,
                    spacing,
                    boundaryMargin,
                    stampCount,
                    chestCount,
                    ticketDepositCount,
                    healingPropCount,
                    looseChestCount,
                    looseTicketDepositCount,
                    looseHealingPropCount);
                return true;
            }

            if (string.IsNullOrWhiteSpace(profile.actLabel))
            {
                profile.actLabel = label;
                changed = true;
            }

            if (profile.actNumber <= 0)
            {
                profile.actNumber = actNumber;
                changed = true;
            }

            if (profile.playableRadius <= 0f)
            {
                profile.playableRadius = radius;
                changed = true;
            }

            if (profile.gridSpacing <= 0f)
            {
                profile.gridSpacing = spacing;
                changed = true;
            }

            if (profile.maxHeight < 0f)
            {
                profile.maxHeight = maxHeight;
                changed = true;
            }

            if (profile.boundaryMargin < 0f)
            {
                profile.boundaryMargin = boundaryMargin;
                changed = true;
            }

            if (profile.landformStampCount <= 0)
            {
                profile.landformStampCount = stampCount;
                changed = true;
            }

            changed |= EnsureStructuralDefaults(profile, actNumber);

            if (profile.generatedChestCount < 0)
            {
                profile.generatedChestCount = chestCount;
                changed = true;
            }

            if (profile.generatedTicketDepositCount < 0)
            {
                profile.generatedTicketDepositCount = ticketDepositCount;
                changed = true;
            }

            if (profile.generatedHealingPropCount < 0)
            {
                profile.generatedHealingPropCount = healingPropCount;
                changed = true;
            }

            if (profile.generatedLooseChestCount < 0)
            {
                profile.generatedLooseChestCount = looseChestCount;
                changed = true;
            }

            if (profile.generatedLooseTicketDepositCount < 0)
            {
                profile.generatedLooseTicketDepositCount = looseTicketDepositCount;
                changed = true;
            }

            if (profile.generatedLooseHealingPropCount < 0)
            {
                profile.generatedLooseHealingPropCount = looseHealingPropCount;
                changed = true;
            }

            return changed;
        }

        private static RunWorldGenerationProfile CreateProfile(
            string label,
            int actNumber,
            float radius,
            float maxHeight,
            float spacing,
            float boundaryMargin,
            int stampCount,
            int chestCount,
            int ticketDepositCount,
            int healingPropCount,
            int looseChestCount,
            int looseTicketDepositCount,
            int looseHealingPropCount)
        {
            return new RunWorldGenerationProfile
            {
                actLabel = label,
                actNumber = actNumber,
                playableRadius = radius,
                maxHeight = maxHeight,
                gridSpacing = spacing,
                boundaryMargin = boundaryMargin,
                landformStampCount = stampCount,
                angularity = 0.85f,
                terraceSteps = actNumber + 5,
                plateauCount = actNumber switch
                {
                    1 => 6,
                    2 => 8,
                    _ => 10
                },
                plateauRadiusMinPercent = 0.09f,
                plateauRadiusMaxPercent = 0.18f,
                rampWidthPercent = 0.085f,
                rampBlendPercent = 0.018f,
                rampCount = actNumber switch
                {
                    1 => 7,
                    2 => 10,
                    _ => 13
                },
                rampWidthMinPercent = 0.045f,
                rampWidthMaxPercent = 0.11f,
                rampLengthMinPercent = 0.07f,
                rampLengthMaxPercent = 0.20f,
                wallHeightThreshold = Mathf.Max(1.25f, spacing * 0.35f),
                allowRampOverlap = true,
                surfaceVariationStrength = 0f,
                generatedChestCount = chestCount,
                generatedTicketDepositCount = ticketDepositCount,
                generatedHealingPropCount = healingPropCount,
                generatedLooseChestCount = looseChestCount,
                generatedLooseTicketDepositCount = looseTicketDepositCount,
                generatedLooseHealingPropCount = looseHealingPropCount
            };
        }

        private static bool EnsureStructuralDefaults(RunWorldGenerationProfile profile, int actNumber)
        {
            bool changed = false;
            if (profile.plateauCount <= 0)
            {
                profile.plateauCount = Mathf.Max(3, actNumber + 1);
                changed = true;
            }

            if (profile.plateauRadiusMinPercent <= 0f)
            {
                profile.plateauRadiusMinPercent = 0.11f;
                changed = true;
            }

            if (profile.plateauRadiusMaxPercent <= 0f)
            {
                profile.plateauRadiusMaxPercent = 0.24f;
                changed = true;
            }

            if (profile.rampWidthPercent <= 0f)
            {
                profile.rampWidthPercent = 0.11f;
                changed = true;
            }

            if (profile.rampBlendPercent <= 0f)
            {
                profile.rampBlendPercent = 0.018f;
                changed = true;
            }

            if (profile.rampCount <= 0)
            {
                profile.rampCount = actNumber switch
                {
                    1 => 7,
                    2 => 10,
                    _ => 13
                };
                changed = true;
            }

            if (profile.rampWidthMinPercent <= 0f)
            {
                profile.rampWidthMinPercent = Mathf.Max(0.02f, profile.RampWidthPercent * 0.55f);
                changed = true;
            }

            if (profile.rampWidthMaxPercent <= 0f)
            {
                profile.rampWidthMaxPercent = Mathf.Max(profile.rampWidthMinPercent, profile.RampWidthPercent * 1.35f);
                changed = true;
            }

            if (profile.rampLengthMinPercent <= 0f)
            {
                profile.rampLengthMinPercent = 0.07f;
                changed = true;
            }

            if (profile.rampLengthMaxPercent <= 0f)
            {
                profile.rampLengthMaxPercent = 0.20f;
                changed = true;
            }

            if (profile.wallHeightThreshold <= 0f)
            {
                profile.wallHeightThreshold = Mathf.Max(1.25f, profile.GridSpacing * 0.35f);
                changed = true;
            }

            if (profile.surfaceVariationStrength < 0f)
            {
                profile.surfaceVariationStrength = 0.035f;
                changed = true;
            }

            return changed;
        }

        private static bool EnsureMinimum(ref float value, float defaultValue, float minimum)
        {
            if (value >= minimum)
            {
                return false;
            }

            value = Mathf.Max(minimum, defaultValue);
            return true;
        }

        private static bool EnsureMinimum(ref int value, int defaultValue, int minimum)
        {
            if (value >= minimum)
            {
                return false;
            }

            value = Mathf.Max(minimum, defaultValue);
            return true;
        }

        private static bool EnsureEaseDefault(ref EaseSettings settings, EaseSettings defaultValue)
        {
            if (settings.shape > 0f)
            {
                return false;
            }

            settings = defaultValue;
            return true;
        }

        private static bool EnsureDefaultColor(ref Color color, Color defaultValue)
        {
            if (color.a > 0f || color.maxColorComponent > 0f)
            {
                return false;
            }

            color = defaultValue;
            return true;
        }
    }
}
