using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TheCircussyOne.Config;
using UnityEngine;

namespace TheCircussyOne.Runtime
{
    public enum GeneratedWorldSurfaceMaterialRole
    {
        Top,
        Organic,
        Wall,
        Ramp,
        Rim
    }

    public readonly struct GeneratedWorldSurfaceTextureData
    {
        public GeneratedWorldSurfaceTextureData(
            string name,
            int width,
            int height,
            Color32[] pixels,
            TextureWrapMode wrapMode,
            FilterMode filterMode,
            bool linear = false)
        {
            Name = string.IsNullOrWhiteSpace(name) ? "Generated World Surface Texture" : name;
            Width = Mathf.Max(1, width);
            Height = Mathf.Max(1, height);
            Pixels = pixels ?? Array.Empty<Color32>();
            WrapMode = wrapMode;
            FilterMode = filterMode;
            Linear = linear;
        }

        public string Name { get; }
        public int Width { get; }
        public int Height { get; }
        public Color32[] Pixels { get; }
        public TextureWrapMode WrapMode { get; }
        public FilterMode FilterMode { get; }
        public bool Linear { get; }
    }

    public readonly struct GeneratedWorldSurfaceTextureSettings
    {
        public GeneratedWorldSurfaceTextureSettings(
            int seed,
            int actNumber,
            int resolution,
            GeneratedWorldSurfaceStyleMode styleMode,
            int selectedStyleProfileIndex,
            float styleVariationStrength,
            float surfaceColorfulness,
            float surfaceWearStrength,
            float organicPatternScale,
            int surfaceZoneCount,
            float surfaceZonePaletteStrength,
            float surfaceZoneBlendSharpness,
            float gritStrength,
            float dirtStrength,
            float gridStrength,
            float rampCueStrength,
            float organicPatternStrength,
            int organicPatternIterations,
            float surfaceReliefStrength,
            float topTileHeightVariation,
            float topTileReliefStrength,
            float topTileGroutDepthStrength,
            float topTileEdgeWearStrength,
            float topTileSmoothnessVariationStrength,
            float wallPanelHeightVariation,
            float rampPlankSeamStrength,
            float rampEdgeWearStrength,
            float rampWoodGrainStrength,
            float wallPanelSeamStrength,
            float wallEdgeWearStrength,
            float wallVerticalWearStrength,
            float topSmoothnessMin,
            float topSmoothnessMax,
            float wallSmoothnessMin,
            float wallSmoothnessMax,
            float surfaceSmoothnessVariationStrength,
            float organicTextureScale,
            Color topColor,
            Color wallColor,
            Color rampColor,
            Color rimColor)
        {
            Seed = seed;
            ActNumber = Mathf.Max(1, actNumber);
            Resolution = Mathf.Clamp(resolution, 128, 1024);
            StyleMode = styleMode;
            SelectedStyleProfileIndex = Mathf.Clamp(selectedStyleProfileIndex, 0, 2);
            StyleVariationStrength = Mathf.Clamp01(styleVariationStrength);
            SurfaceColorfulness = Mathf.Clamp01(surfaceColorfulness);
            SurfaceWearStrength = Mathf.Clamp01(surfaceWearStrength);
            OrganicPatternScale = Mathf.Clamp(organicPatternScale, 0.25f, 6f);
            SurfaceZoneCount = Mathf.Clamp(surfaceZoneCount, 1, 8);
            SurfaceZonePaletteStrength = Mathf.Clamp01(surfaceZonePaletteStrength);
            SurfaceZoneBlendSharpness = Mathf.Clamp(surfaceZoneBlendSharpness, 0.25f, 4f);
            GritStrength = Mathf.Clamp01(gritStrength);
            DirtStrength = Mathf.Clamp01(dirtStrength);
            GridStrength = Mathf.Clamp01(gridStrength);
            RampCueStrength = Mathf.Clamp01(rampCueStrength);
            OrganicPatternStrength = Mathf.Clamp01(organicPatternStrength);
            OrganicPatternIterations = Mathf.Clamp(organicPatternIterations, 1, 160);
            SurfaceReliefStrength = Mathf.Clamp(surfaceReliefStrength, 0f, 2f);
            TopTileHeightVariation = Mathf.Clamp(topTileHeightVariation, 0f, 0.25f);
            TopTileReliefStrength = Mathf.Clamp(topTileReliefStrength, 0f, 3f);
            TopTileGroutDepthStrength = Mathf.Clamp(topTileGroutDepthStrength, 0f, 3f);
            TopTileEdgeWearStrength = Mathf.Clamp(topTileEdgeWearStrength, 0f, 2f);
            TopTileSmoothnessVariationStrength = Mathf.Clamp(topTileSmoothnessVariationStrength, 0f, 3f);
            WallPanelHeightVariation = Mathf.Clamp(wallPanelHeightVariation, 0f, 0.25f);
            RampPlankSeamStrength = Mathf.Clamp(rampPlankSeamStrength, 0f, 2f);
            RampEdgeWearStrength = Mathf.Clamp(rampEdgeWearStrength, 0f, 2f);
            RampWoodGrainStrength = Mathf.Clamp(rampWoodGrainStrength, 0f, 2f);
            WallPanelSeamStrength = Mathf.Clamp(wallPanelSeamStrength, 0f, 2f);
            WallEdgeWearStrength = Mathf.Clamp(wallEdgeWearStrength, 0f, 2f);
            WallVerticalWearStrength = Mathf.Clamp(wallVerticalWearStrength, 0f, 2f);
            TopSmoothnessMin = Mathf.Clamp01(Mathf.Min(topSmoothnessMin, topSmoothnessMax));
            TopSmoothnessMax = Mathf.Clamp01(Mathf.Max(topSmoothnessMin, topSmoothnessMax));
            WallSmoothnessMin = Mathf.Clamp01(Mathf.Min(wallSmoothnessMin, wallSmoothnessMax));
            WallSmoothnessMax = Mathf.Clamp01(Mathf.Max(wallSmoothnessMin, wallSmoothnessMax));
            SurfaceSmoothnessVariationStrength = Mathf.Clamp(surfaceSmoothnessVariationStrength, 0f, 2f);
            OrganicTextureScale = Mathf.Clamp(organicTextureScale, 0.01f, 4f);
            TopColor = topColor;
            WallColor = wallColor;
            RampColor = rampColor;
            RimColor = rimColor;
        }

        public int Seed { get; }
        public int ActNumber { get; }
        public int Resolution { get; }
        public GeneratedWorldSurfaceStyleMode StyleMode { get; }
        public int SelectedStyleProfileIndex { get; }
        public float StyleVariationStrength { get; }
        public float SurfaceColorfulness { get; }
        public float SurfaceWearStrength { get; }
        public float OrganicPatternScale { get; }
        public int SurfaceZoneCount { get; }
        public float SurfaceZonePaletteStrength { get; }
        public float SurfaceZoneBlendSharpness { get; }
        public float GritStrength { get; }
        public float DirtStrength { get; }
        public float GridStrength { get; }
        public float RampCueStrength { get; }
        public float OrganicPatternStrength { get; }
        public int OrganicPatternIterations { get; }
        public float SurfaceReliefStrength { get; }
        public float TopTileHeightVariation { get; }
        public float TopTileReliefStrength { get; }
        public float TopTileGroutDepthStrength { get; }
        public float TopTileEdgeWearStrength { get; }
        public float TopTileSmoothnessVariationStrength { get; }
        public float WallPanelHeightVariation { get; }
        public float RampPlankSeamStrength { get; }
        public float RampEdgeWearStrength { get; }
        public float RampWoodGrainStrength { get; }
        public float WallPanelSeamStrength { get; }
        public float WallEdgeWearStrength { get; }
        public float WallVerticalWearStrength { get; }
        public float TopSmoothnessMin { get; }
        public float TopSmoothnessMax { get; }
        public float WallSmoothnessMin { get; }
        public float WallSmoothnessMax { get; }
        public float SurfaceSmoothnessVariationStrength { get; }
        public float OrganicTextureScale { get; }
        public Color TopColor { get; }
        public Color WallColor { get; }
        public Color RampColor { get; }
        public Color RimColor { get; }
    }

    public sealed class GeneratedWorldSurfaceMaterialSet : IDisposable
    {
        private UnityEngine.Object[] ownedObjects;

        public GeneratedWorldSurfaceMaterialSet(
            Material topMaterial,
            Material organicMaterial,
            Material wallMaterial,
            Material rampMaterial,
            Material rimMaterial,
            UnityEngine.Object[] ownedObjects)
            : this(
                new[] { topMaterial },
                new[] { organicMaterial },
                new[] { wallMaterial },
                new[] { rampMaterial },
                rimMaterial,
                0,
                1,
                ownedObjects)
        {
        }

        public GeneratedWorldSurfaceMaterialSet(
            Material[] topMaterials,
            Material[] organicMaterials,
            Material[] wallMaterials,
            Material[] rampMaterials,
            Material rimMaterial,
            int surfaceZoneSeed,
            int surfaceZoneActNumber,
            UnityEngine.Object[] ownedObjects)
            : this(
                topMaterials,
                organicMaterials,
                wallMaterials,
                rampMaterials,
                rimMaterial,
                surfaceZoneSeed,
                surfaceZoneActNumber,
                BuildZoneTints(GeneratedWorldSurfaceMaterialRole.Top, topMaterials != null ? topMaterials.Length : 1),
                BuildZoneTints(GeneratedWorldSurfaceMaterialRole.Organic, organicMaterials != null ? organicMaterials.Length : 1),
                BuildZoneTints(GeneratedWorldSurfaceMaterialRole.Wall, wallMaterials != null ? wallMaterials.Length : 1),
                BuildZoneTints(GeneratedWorldSurfaceMaterialRole.Ramp, rampMaterials != null ? rampMaterials.Length : 1),
                1.15f,
                ownedObjects)
        {
        }

        internal GeneratedWorldSurfaceMaterialSet(
            Material[] topMaterials,
            Material[] organicMaterials,
            Material[] wallMaterials,
            Material[] rampMaterials,
            Material rimMaterial,
            int surfaceZoneSeed,
            int surfaceZoneActNumber,
            Color[] topZoneTints,
            Color[] organicZoneTints,
            Color[] wallZoneTints,
            Color[] rampZoneTints,
            float surfaceZoneBlendSharpness,
            UnityEngine.Object[] ownedObjects)
        {
            TopMaterials = NormalizeMaterials(topMaterials);
            OrganicMaterials = NormalizeMaterials(organicMaterials);
            WallMaterials = NormalizeMaterials(wallMaterials);
            RampMaterials = NormalizeMaterials(rampMaterials);
            TopMaterial = TopMaterials[0];
            OrganicMaterial = OrganicMaterials[0];
            WallMaterial = WallMaterials[0];
            RampMaterial = RampMaterials[0];
            RimMaterial = rimMaterial;
            SurfaceZoneSeed = surfaceZoneSeed;
            SurfaceZoneActNumber = Mathf.Max(1, surfaceZoneActNumber);
            TopZoneTints = NormalizeTints(topZoneTints, TopMaterials.Length);
            OrganicZoneTints = NormalizeTints(organicZoneTints, OrganicMaterials.Length);
            WallZoneTints = NormalizeTints(wallZoneTints, WallMaterials.Length);
            RampZoneTints = NormalizeTints(rampZoneTints, RampMaterials.Length);
            SurfaceZoneBlendSharpness = Mathf.Clamp(surfaceZoneBlendSharpness, 0.25f, 4f);
            this.ownedObjects = ownedObjects ?? Array.Empty<UnityEngine.Object>();
        }

        public Material TopMaterial { get; }
        public Material OrganicMaterial { get; }
        public Material WallMaterial { get; }
        public Material RampMaterial { get; }
        public Material RimMaterial { get; }
        public Material[] TopMaterials { get; }
        public Material[] OrganicMaterials { get; }
        public Material[] WallMaterials { get; }
        public Material[] RampMaterials { get; }
        public Color[] TopZoneTints { get; }
        public Color[] OrganicZoneTints { get; }
        public Color[] WallZoneTints { get; }
        public Color[] RampZoneTints { get; }
        public int SurfaceZoneSeed { get; }
        public int SurfaceZoneActNumber { get; }
        public int SurfaceZoneCount => TopMaterials.Length;
        public float SurfaceZoneBlendSharpness { get; }
        public bool OwnsRuntimeObjects => ownedObjects.Length > 0;

        public static GeneratedWorldSurfaceMaterialSet Fallback(Material material)
        {
            return new GeneratedWorldSurfaceMaterialSet(material, material, material, material, material, Array.Empty<UnityEngine.Object>());
        }

        public Material TopMaterialAt(int zoneIndex)
        {
            return MaterialAt(TopMaterials, zoneIndex);
        }

        public Material OrganicMaterialAt(int zoneIndex)
        {
            return MaterialAt(OrganicMaterials, zoneIndex);
        }

        public Material WallMaterialAt(int zoneIndex)
        {
            return MaterialAt(WallMaterials, zoneIndex);
        }

        public Material RampMaterialAt(int zoneIndex)
        {
            return MaterialAt(RampMaterials, zoneIndex);
        }

        public void AttachCleanup(GameObject owner)
        {
            if (owner == null || ownedObjects.Length == 0)
            {
                return;
            }

            GeneratedWorldSurfaceMaterialCleanup cleanup = owner.AddComponent<GeneratedWorldSurfaceMaterialCleanup>();
            cleanup.Initialize(ownedObjects);
            ownedObjects = Array.Empty<UnityEngine.Object>();
        }

        public void Dispose()
        {
            DestroyObjects(ownedObjects);
            ownedObjects = Array.Empty<UnityEngine.Object>();
        }

        internal static void DestroyObjects(UnityEngine.Object[] objects)
        {
            if (objects == null)
            {
                return;
            }

            for (int i = 0; i < objects.Length; i++)
            {
                UnityEngine.Object instance = objects[i];
                if (instance == null)
                {
                    continue;
                }

                RuntimeObjectFactory.Release(instance);
            }
        }

        private static Material[] NormalizeMaterials(Material[] materials)
        {
            return materials != null && materials.Length > 0
                ? materials
                : new Material[] { null };
        }

        private static Color[] NormalizeTints(Color[] tints, int length)
        {
            int count = Mathf.Max(1, length);
            var normalized = new Color[count];
            for (int i = 0; i < count; i++)
            {
                normalized[i] = tints != null && i < tints.Length ? tints[i] : Color.white;
                normalized[i].a = 1f;
            }

            return normalized;
        }

        private static Color[] BuildZoneTints(GeneratedWorldSurfaceMaterialRole role, int zoneCount)
        {
            int count = Mathf.Clamp(zoneCount, 1, 8);
            var tints = new Color[count];
            for (int i = 0; i < count; i++)
            {
                tints[i] = Color.white;
            }

            return tints;
        }

        private static Material MaterialAt(Material[] materials, int zoneIndex)
        {
            if (materials == null || materials.Length == 0)
            {
                return null;
            }

            return materials[Mathf.Clamp(zoneIndex, 0, materials.Length - 1)];
        }
    }

    public sealed class GeneratedWorldSurfaceMaterialCleanup : MonoBehaviour
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

    internal readonly struct GeneratedWorldSurfaceTextureBundleData
    {
        public GeneratedWorldSurfaceTextureBundleData(
            GeneratedWorldSurfaceTextureData albedo,
            GeneratedWorldSurfaceTextureData normal,
            GeneratedWorldSurfaceTextureData height,
            GeneratedWorldSurfaceTextureData smoothness)
        {
            Albedo = albedo;
            Normal = normal;
            Height = height;
            Smoothness = smoothness;
        }

        public GeneratedWorldSurfaceTextureData Albedo { get; }
        public GeneratedWorldSurfaceTextureData Normal { get; }
        public GeneratedWorldSurfaceTextureData Height { get; }
        public GeneratedWorldSurfaceTextureData Smoothness { get; }
    }

    internal readonly struct GeneratedWorldOrganicPatternData
    {
        public GeneratedWorldOrganicPatternData(int size, float[] values)
        {
            Size = Mathf.Max(1, size);
            Values = values ?? Array.Empty<float>();
        }

        public int Size { get; }
        public float[] Values { get; }
        public bool IsValid => Size > 1 && Values.Length == Size * Size;

        public float Sample(float u, float v)
        {
            if (!IsValid)
            {
                return 0.5f;
            }

            float x = Mathf.Repeat(u, 1f) * Size;
            float y = Mathf.Repeat(v, 1f) * Size;
            int x0 = Mod(Mathf.FloorToInt(x), Size);
            int y0 = Mod(Mathf.FloorToInt(y), Size);
            int x1 = (x0 + 1) % Size;
            int y1 = (y0 + 1) % Size;
            float tx = SmoothStep(0f, 1f, x - Mathf.Floor(x));
            float ty = SmoothStep(0f, 1f, y - Mathf.Floor(y));
            float a = Values[y0 * Size + x0];
            float b = Values[y0 * Size + x1];
            float c = Values[y1 * Size + x0];
            float d = Values[y1 * Size + x1];
            return Mathf.Lerp(Mathf.Lerp(a, b, tx), Mathf.Lerp(c, d, tx), ty);
        }

        private static int Mod(int value, int period)
        {
            int result = value % period;
            return result < 0 ? result + period : result;
        }

        private static float SmoothStep(float edge0, float edge1, float value)
        {
            float t = Mathf.Clamp01((value - edge0) / Mathf.Max(0.0001f, edge1 - edge0));
            return t * t * (3f - 2f * t);
        }
    }

    public static class GeneratedWorldSurfaceTextureGenerator
    {
        private static readonly Color DirtColor = new(0.075f, 0.055f, 0.038f, 1f);
        private static readonly Color DustColor = new(0.44f, 0.36f, 0.25f, 1f);
        private const int TopTileCells = 4;
        private const int WallPanelCount = 7;
        private const int RampPlankCount = 6;

        private readonly struct SurfaceStyleProfile
        {
            public SurfaceStyleProfile(
                Color topBase,
                Color checkerLight,
                Color checkerDark,
                Color organicBase,
                Color organicPattern,
                Color wallRed,
                Color wallAlternate,
                Color wallDark,
                Color rampBase,
                Color rampGrain,
                Color accentGold,
                Color accentRed,
                Color accentCool,
                Color rimBase)
            {
                TopBase = topBase;
                CheckerLight = checkerLight;
                CheckerDark = checkerDark;
                OrganicBase = organicBase;
                OrganicPattern = organicPattern;
                WallRed = wallRed;
                WallAlternate = wallAlternate;
                WallDark = wallDark;
                RampBase = rampBase;
                RampGrain = rampGrain;
                AccentGold = accentGold;
                AccentRed = accentRed;
                AccentCool = accentCool;
                RimBase = rimBase;
            }

            public Color TopBase { get; }
            public Color CheckerLight { get; }
            public Color CheckerDark { get; }
            public Color OrganicBase { get; }
            public Color OrganicPattern { get; }
            public Color WallRed { get; }
            public Color WallAlternate { get; }
            public Color WallDark { get; }
            public Color RampBase { get; }
            public Color RampGrain { get; }
            public Color AccentGold { get; }
            public Color AccentRed { get; }
            public Color AccentCool { get; }
            public Color RimBase { get; }
        }

        private static readonly SurfaceStyleProfile[] SurfaceStyleProfiles =
        {
            new(
                topBase: new Color(0.070f, 0.062f, 0.050f, 1f),
                checkerLight: new Color(0.39f, 0.34f, 0.240f, 1f),
                checkerDark: new Color(0.066f, 0.058f, 0.050f, 1f),
                organicBase: new Color(0.175f, 0.126f, 0.070f, 1f),
                organicPattern: new Color(0.255f, 0.225f, 0.155f, 1f),
                wallRed: new Color(0.325f, 0.026f, 0.030f, 1f),
                wallAlternate: new Color(0.034f, 0.120f, 0.128f, 1f),
                wallDark: new Color(0.028f, 0.025f, 0.032f, 1f),
                rampBase: new Color(0.205f, 0.100f, 0.043f, 1f),
                rampGrain: new Color(0.315f, 0.165f, 0.065f, 1f),
                accentGold: new Color(0.56f, 0.355f, 0.115f, 1f),
                accentRed: new Color(0.385f, 0.030f, 0.035f, 1f),
                accentCool: new Color(0.036f, 0.185f, 0.185f, 1f),
                rimBase: new Color(0.130f, 0.072f, 0.050f, 1f)),
            new(
                topBase: new Color(0.085f, 0.055f, 0.055f, 1f),
                checkerLight: new Color(0.43f, 0.355f, 0.255f, 1f),
                checkerDark: new Color(0.092f, 0.040f, 0.043f, 1f),
                organicBase: new Color(0.162f, 0.120f, 0.080f, 1f),
                organicPattern: new Color(0.255f, 0.195f, 0.125f, 1f),
                wallRed: new Color(0.365f, 0.030f, 0.042f, 1f),
                wallAlternate: new Color(0.054f, 0.148f, 0.140f, 1f),
                wallDark: new Color(0.036f, 0.031f, 0.036f, 1f),
                rampBase: new Color(0.228f, 0.115f, 0.050f, 1f),
                rampGrain: new Color(0.360f, 0.195f, 0.080f, 1f),
                accentGold: new Color(0.62f, 0.405f, 0.125f, 1f),
                accentRed: new Color(0.430f, 0.030f, 0.050f, 1f),
                accentCool: new Color(0.045f, 0.220f, 0.205f, 1f),
                rimBase: new Color(0.155f, 0.070f, 0.055f, 1f)),
            new(
                topBase: new Color(0.076f, 0.060f, 0.045f, 1f),
                checkerLight: new Color(0.355f, 0.292f, 0.205f, 1f),
                checkerDark: new Color(0.060f, 0.054f, 0.047f, 1f),
                organicBase: new Color(0.190f, 0.132f, 0.070f, 1f),
                organicPattern: new Color(0.245f, 0.182f, 0.100f, 1f),
                wallRed: new Color(0.282f, 0.040f, 0.034f, 1f),
                wallAlternate: new Color(0.036f, 0.095f, 0.125f, 1f),
                wallDark: new Color(0.032f, 0.025f, 0.025f, 1f),
                rampBase: new Color(0.245f, 0.135f, 0.060f, 1f),
                rampGrain: new Color(0.385f, 0.225f, 0.095f, 1f),
                accentGold: new Color(0.515f, 0.315f, 0.105f, 1f),
                accentRed: new Color(0.330f, 0.042f, 0.035f, 1f),
                accentCool: new Color(0.035f, 0.160f, 0.172f, 1f),
                rimBase: new Color(0.165f, 0.085f, 0.045f, 1f))
        };

        public static GeneratedWorldSurfaceTextureSettings Snapshot(
            RunWorldGenerationConfig config,
            int seed,
            int actNumber)
        {
            GeneratedWorldSurfaceStyleMode styleMode = config != null
                ? config.GeneratedTerrainSurfaceStyleMode
                : GeneratedWorldSurfaceStyleMode.SeededCircus;
            int selectedStyleProfileIndex = ResolveStyleProfileIndex(styleMode, seed, actNumber);
            return new GeneratedWorldSurfaceTextureSettings(
                seed,
                actNumber,
                config != null ? config.GeneratedTerrainSurfaceTextureResolution : 512,
                styleMode,
                selectedStyleProfileIndex,
                config != null ? config.GeneratedTerrainSurfaceStyleVariationStrength : 0.55f,
                config != null ? config.GeneratedTerrainSurfaceColorfulness : 0.62f,
                config != null ? config.GeneratedTerrainSurfaceWearStrength : 0.68f,
                config != null ? config.GeneratedTerrainOrganicPatternScale : 1.65f,
                config != null ? config.GeneratedTerrainSurfaceZoneCount : 1,
                config != null ? config.GeneratedTerrainSurfaceZonePaletteStrength : 0.45f,
                config != null ? config.GeneratedTerrainSurfaceZoneBlendSharpness : 1.15f,
                config != null ? config.GeneratedTerrainSurfaceGritStrength : 0.30f,
                config != null ? config.GeneratedTerrainSurfaceDirtStrength : 0.36f,
                config != null ? config.GeneratedTerrainSurfaceGridStrength : 0.26f,
                config != null ? config.GeneratedTerrainRampCueStrength : 0.38f,
                config != null ? config.GeneratedTerrainOrganicPatternStrength : 0.18f,
                config != null ? config.GeneratedTerrainOrganicPatternIterations : 64,
                config != null ? config.GeneratedTerrainSurfaceReliefStrength : 1f,
                config != null ? config.GeneratedTerrainTopTileHeightVariation : 0.075f,
                config != null ? config.GeneratedTerrainTopTileReliefStrength : 1.45f,
                config != null ? config.GeneratedTerrainTopTileGroutDepthStrength : 1.35f,
                config != null ? config.GeneratedTerrainTopTileEdgeWearStrength : 0.55f,
                config != null ? config.GeneratedTerrainTopTileSmoothnessVariationStrength : 1.20f,
                config != null ? config.GeneratedTerrainWallPanelHeightVariation : 0.065f,
                config != null ? config.GeneratedTerrainRampPlankSeamStrength : 1.35f,
                config != null ? config.GeneratedTerrainRampEdgeWearStrength : 0.60f,
                config != null ? config.GeneratedTerrainRampWoodGrainStrength : 1.20f,
                config != null ? config.GeneratedTerrainWallPanelSeamStrength : 1.35f,
                config != null ? config.GeneratedTerrainWallEdgeWearStrength : 0.55f,
                config != null ? config.GeneratedTerrainWallVerticalWearStrength : 1.15f,
                config != null ? config.GeneratedTerrainTopSmoothnessMin : 0.08f,
                config != null ? config.GeneratedTerrainTopSmoothnessMax : 0.24f,
                config != null ? config.GeneratedTerrainWallSmoothnessMin : 0.05f,
                config != null ? config.GeneratedTerrainWallSmoothnessMax : 0.18f,
                config != null ? config.GeneratedTerrainSurfaceSmoothnessVariationStrength : 1f,
                config != null ? config.GeneratedTerrainOrganicTextureScale : 0.45f,
                config != null ? config.generatedTerrainTopColor : new Color(0.055f, 0.06f, 0.065f, 1f),
                config != null ? config.generatedTerrainWallColor : new Color(0.22f, 0.035f, 0.045f, 1f),
                config != null ? config.generatedTerrainRampColor : new Color(0.22f, 0.12f, 0.055f, 1f),
                config != null ? config.generatedTerrainRimColor : new Color(0.12f, 0.075f, 0.06f, 1f));
        }

        private static int ResolveStyleProfileIndex(GeneratedWorldSurfaceStyleMode mode, int seed, int actNumber)
        {
            return mode switch
            {
                GeneratedWorldSurfaceStyleMode.ClassicBigTop => 0,
                GeneratedWorldSurfaceStyleMode.CandyChecker => 1,
                GeneratedWorldSurfaceStyleMode.PaintedBoardwalk => 2,
                _ => SeededStyleProfileIndex(seed, actNumber)
            };
        }

        private static int SeededStyleProfileIndex(int seed, int actNumber)
        {
            float roll = Hash01(seed, actNumber, 3109);
            if (roll < 0.50f)
            {
                return 0;
            }

            return roll < 0.80f ? 1 : 2;
        }

        public static GeneratedWorldSurfaceMaterialSet GenerateMaterialSet(
            Material fallbackMaterial,
            RunWorldGenerationConfig config,
            int seed,
            int actNumber)
        {
            if (config == null || !config.generatedTerrainSurfaceTexturesEnabled)
            {
                return GeneratedWorldSurfaceMaterialSet.Fallback(fallbackMaterial);
            }

            Shader shader = ResolveShader();
            if (shader == null)
            {
                return GeneratedWorldSurfaceMaterialSet.Fallback(fallbackMaterial);
            }

            GeneratedWorldSurfaceTextureSettings settings = Snapshot(config, seed, actNumber);
            return CreateMaterialSet(
                shader,
                settings,
                BuildTextureBundle(settings, GeneratedWorldSurfaceMaterialRole.Top),
                BuildTextureBundle(settings, GeneratedWorldSurfaceMaterialRole.Organic),
                BuildTextureBundle(settings, GeneratedWorldSurfaceMaterialRole.Wall),
                BuildTextureBundle(settings, GeneratedWorldSurfaceMaterialRole.Ramp),
                BuildTextureBundle(settings, GeneratedWorldSurfaceMaterialRole.Rim));
        }

        public static async UniTask<GeneratedWorldSurfaceMaterialSet> GenerateMaterialSetAsync(
            Material fallbackMaterial,
            RunWorldGenerationConfig config,
            int seed,
            int actNumber,
            WorldLoadTimingDiagnostics timing = null)
        {
            if (config == null || !config.generatedTerrainSurfaceTexturesEnabled)
            {
                return GeneratedWorldSurfaceMaterialSet.Fallback(fallbackMaterial);
            }

            Shader shader = ResolveShader();
            if (shader == null)
            {
                return GeneratedWorldSurfaceMaterialSet.Fallback(fallbackMaterial);
            }

            timing ??= WorldLoadTimingDiagnostics.Disabled;
            GeneratedWorldSurfaceTextureSettings settings = Snapshot(config, seed, actNumber);
            GeneratedWorldSurfaceTextureBundleData[] data;
            using (timing.Stage("SurfaceTextures.PixelsThreadPool"))
            {
                data = await UniTask.RunOnThreadPool(() => new[]
                {
                    BuildTextureBundle(settings, GeneratedWorldSurfaceMaterialRole.Top),
                    BuildTextureBundle(settings, GeneratedWorldSurfaceMaterialRole.Organic),
                    BuildTextureBundle(settings, GeneratedWorldSurfaceMaterialRole.Wall),
                    BuildTextureBundle(settings, GeneratedWorldSurfaceMaterialRole.Ramp),
                    BuildTextureBundle(settings, GeneratedWorldSurfaceMaterialRole.Rim)
                });
                await UniTask.SwitchToMainThread();
            }

            using (timing.Stage("SurfaceTextures.TextureUpload"))
            {
                return CreateMaterialSet(shader, settings, data[0], data[1], data[2], data[3], data[4]);
            }
        }

        public static GeneratedWorldSurfaceTextureData BuildTextureData(
            GeneratedWorldSurfaceTextureSettings settings,
            GeneratedWorldSurfaceMaterialRole role)
        {
            GeneratedWorldOrganicPatternData organicPattern = role == GeneratedWorldSurfaceMaterialRole.Organic
                ? BuildOrganicPattern(settings, RoleSeed(settings.Seed, settings.ActNumber, role) + 24001)
                : default;
            return BuildTextureData(settings, role, organicPattern);
        }

        public static GeneratedWorldSurfaceTextureData BuildHeightTextureData(
            GeneratedWorldSurfaceTextureSettings settings,
            GeneratedWorldSurfaceMaterialRole role)
        {
            GeneratedWorldOrganicPatternData organicPattern = role == GeneratedWorldSurfaceMaterialRole.Organic
                ? BuildOrganicPattern(settings, RoleSeed(settings.Seed, settings.ActNumber, role) + 24001)
                : default;
            return BuildHeightTextureData(settings, role, BuildHeightValues(settings, role, organicPattern));
        }

        public static GeneratedWorldSurfaceTextureData BuildSmoothnessTextureData(
            GeneratedWorldSurfaceTextureSettings settings,
            GeneratedWorldSurfaceMaterialRole role)
        {
            int size = settings.Resolution;
            int seed = RoleSeed(settings.Seed, settings.ActNumber, role) + 7601;
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                float v = y / (float)(size - 1);
                for (int x = 0; x < size; x++)
                {
                    float u = x / (float)(size - 1);
                    byte smoothness = (byte)Mathf.RoundToInt(SurfaceSmoothness(settings, role, u, v, seed) * 255f);
                    pixels[y * size + x] = new Color32(0, 0, 0, smoothness);
                }
            }

            return new GeneratedWorldSurfaceTextureData(
                $"Generated World {role} Smoothness Mask",
                size,
                size,
                pixels,
                TextureWrapMode.Repeat,
                FilterMode.Bilinear,
                linear: true);
        }

        private static GeneratedWorldSurfaceTextureData BuildTextureData(
            GeneratedWorldSurfaceTextureSettings settings,
            GeneratedWorldSurfaceMaterialRole role,
            GeneratedWorldOrganicPatternData organicPattern)
        {
            int size = settings.Resolution;
            int seed = RoleSeed(settings.Seed, settings.ActNumber, role);
            var pixels = new Color32[size * size];
            Color baseColor = BaseColor(settings, role);

            for (int y = 0; y < size; y++)
            {
                float v = y / (float)(size - 1);
                for (int x = 0; x < size; x++)
                {
                    float u = x / (float)(size - 1);
                    float weave = Weave(u, v, role);
                    float broadGrit = TileableValueNoise(u * 7f, v * 7f, seed + 17, 7, 7);
                    float fineGrit = TileableValueNoise(u * 37f, v * 43f, seed + 131, 37, 43);
                    float scuff = TileableValueNoise(u * 5f, v * 6f, seed + 293, 5, 6);
                    float grid = GridLine(u, v, role);
                    float dirt = DirtAmount(u, v, role, broadGrit, fineGrit, seed, settings);
                    float rampCue = role == GeneratedWorldSurfaceMaterialRole.Ramp
                        ? RampCue(u, v) * settings.RampCueStrength
                        : 0f;

                    Color color = PatternColor(settings, role, u, v, seed, baseColor, organicPattern);
                    color = Color.Lerp(color, DirtColor, dirt * DirtBlend(role));
                    color = ApplySurfaceAccent(color, role, u, v, grid, rampCue, settings);

                    float shade = 1f
                        + weave * 0.06f
                        - (broadGrit * 0.65f + fineGrit * 0.35f) * settings.GritStrength * 0.12f
                        - dirt * settings.DirtStrength * 0.16f
                        - grid * settings.GridStrength * GridDarkening(role)
                        - rampCue * 0.08f
                        + (scuff - 0.5f) * settings.GritStrength * 0.035f;

                    color.r = Mathf.Clamp01(color.r * shade);
                    color.g = Mathf.Clamp01(color.g * shade);
                    color.b = Mathf.Clamp01(color.b * shade);
                    color.a = 1f;
                    pixels[y * size + x] = color;
                }
            }

            return new GeneratedWorldSurfaceTextureData(
                $"Generated World {role} Surface",
                size,
                size,
                pixels,
                TextureWrapMode.Repeat,
                FilterMode.Bilinear);
        }

        private static GeneratedWorldSurfaceMaterialSet CreateMaterialSet(
            Shader shader,
            GeneratedWorldSurfaceTextureSettings settings,
            GeneratedWorldSurfaceTextureBundleData top,
            GeneratedWorldSurfaceTextureBundleData organic,
            GeneratedWorldSurfaceTextureBundleData wall,
            GeneratedWorldSurfaceTextureBundleData ramp,
            GeneratedWorldSurfaceTextureBundleData rim)
        {
            Texture2D topTexture = CreateTexture(top.Albedo);
            Texture2D topNormalTexture = CreateTextureOrNull(top.Normal);
            Texture2D topHeightTexture = CreateTextureOrNull(top.Height);
            Texture2D topSmoothnessTexture = CreateTextureOrNull(top.Smoothness);
            Texture2D organicTexture = CreateTexture(organic.Albedo);
            Texture2D organicNormalTexture = CreateTextureOrNull(organic.Normal);
            Texture2D organicHeightTexture = CreateTextureOrNull(organic.Height);
            Texture2D organicSmoothnessTexture = CreateTextureOrNull(organic.Smoothness);
            Texture2D wallTexture = CreateTexture(wall.Albedo);
            Texture2D wallNormalTexture = CreateTextureOrNull(wall.Normal);
            Texture2D wallHeightTexture = CreateTextureOrNull(wall.Height);
            Texture2D wallSmoothnessTexture = CreateTextureOrNull(wall.Smoothness);
            Texture2D rampTexture = CreateTexture(ramp.Albedo);
            Texture2D rampNormalTexture = CreateTextureOrNull(ramp.Normal);
            Texture2D rampHeightTexture = CreateTextureOrNull(ramp.Height);
            Texture2D rampSmoothnessTexture = CreateTextureOrNull(ramp.Smoothness);
            Texture2D rimTexture = CreateTexture(rim.Albedo);
            Texture2D rimNormalTexture = CreateTextureOrNull(rim.Normal);
            Texture2D rimHeightTexture = CreateTextureOrNull(rim.Height);
            Texture2D rimSmoothnessTexture = CreateTextureOrNull(rim.Smoothness);
            Material topMaterial = CreateMaterial(shader, settings, "Generated World Top Surface Material", topTexture, topNormalTexture, topHeightTexture, topSmoothnessTexture, GeneratedWorldSurfaceMaterialRole.Top);
            Material organicMaterial = CreateMaterial(shader, settings, "Generated World Organic Ground Surface Material", organicTexture, organicNormalTexture, organicHeightTexture, organicSmoothnessTexture, GeneratedWorldSurfaceMaterialRole.Organic);
            Material wallMaterial = CreateMaterial(shader, settings, "Generated World Wall Surface Material", wallTexture, wallNormalTexture, wallHeightTexture, wallSmoothnessTexture, GeneratedWorldSurfaceMaterialRole.Wall);
            Material rampMaterial = CreateMaterial(shader, settings, "Generated World Ramp Surface Material", rampTexture, rampNormalTexture, rampHeightTexture, rampSmoothnessTexture, GeneratedWorldSurfaceMaterialRole.Ramp);
            Material rimMaterial = CreateMaterial(shader, settings, "Generated World Rim Surface Material", rimTexture, rimNormalTexture, rimHeightTexture, rimSmoothnessTexture, GeneratedWorldSurfaceMaterialRole.Rim);
            var ownedObjects = new List<UnityEngine.Object>
            {
                topTexture, topNormalTexture, topHeightTexture,
                topSmoothnessTexture,
                organicTexture, organicNormalTexture, organicHeightTexture,
                organicSmoothnessTexture,
                wallTexture, wallNormalTexture, wallHeightTexture,
                wallSmoothnessTexture,
                rampTexture, rampNormalTexture, rampHeightTexture, rampSmoothnessTexture,
                rimTexture, rimNormalTexture, rimHeightTexture, rimSmoothnessTexture,
                topMaterial, organicMaterial, wallMaterial, rampMaterial, rimMaterial
            };
            Material[] topMaterials = CreateZoneMaterialVariants(topMaterial, settings, GeneratedWorldSurfaceMaterialRole.Top, ownedObjects);
            Material[] organicMaterials = CreateZoneMaterialVariants(organicMaterial, settings, GeneratedWorldSurfaceMaterialRole.Organic, ownedObjects);
            Material[] wallMaterials = CreateZoneMaterialVariants(wallMaterial, settings, GeneratedWorldSurfaceMaterialRole.Wall, ownedObjects);
            Material[] rampMaterials = CreateZoneMaterialVariants(rampMaterial, settings, GeneratedWorldSurfaceMaterialRole.Ramp, ownedObjects);
            return new GeneratedWorldSurfaceMaterialSet(
                topMaterials,
                organicMaterials,
                wallMaterials,
                rampMaterials,
                rimMaterial,
                settings.Seed,
                settings.ActNumber,
                BuildZoneTints(settings, GeneratedWorldSurfaceMaterialRole.Top),
                BuildZoneTints(settings, GeneratedWorldSurfaceMaterialRole.Organic),
                BuildZoneTints(settings, GeneratedWorldSurfaceMaterialRole.Wall),
                BuildZoneTints(settings, GeneratedWorldSurfaceMaterialRole.Ramp),
                settings.SurfaceZoneBlendSharpness,
                ownedObjects.ToArray());
        }

        private static Color[] BuildZoneTints(
            GeneratedWorldSurfaceTextureSettings settings,
            GeneratedWorldSurfaceMaterialRole role)
        {
            int zoneCount = Mathf.Clamp(settings.SurfaceZoneCount, 1, 8);
            var tints = new Color[zoneCount];
            for (int zone = 0; zone < zoneCount; zone++)
            {
                tints[zone] = ZoneTint(settings, role, zone);
            }

            return tints;
        }

        private static Material[] CreateZoneMaterialVariants(
            Material baseMaterial,
            GeneratedWorldSurfaceTextureSettings settings,
            GeneratedWorldSurfaceMaterialRole role,
            List<UnityEngine.Object> ownedObjects)
        {
            int zoneCount = Mathf.Clamp(settings.SurfaceZoneCount, 1, 8);
            var materials = new Material[zoneCount];
            materials[0] = baseMaterial;
            for (int zone = 1; zone < zoneCount; zone++)
            {
                var variant = new Material(baseMaterial)
                {
                    name = $"{baseMaterial.name} Zone {zone + 1}",
                    hideFlags = HideFlags.DontSave
                };
                ApplyZoneTint(variant, settings, role, zone);
                ownedObjects.Add(variant);
                materials[zone] = variant;
            }

            return materials;
        }

        private static void ApplyZoneTint(
            Material material,
            GeneratedWorldSurfaceTextureSettings settings,
            GeneratedWorldSurfaceMaterialRole role,
            int zoneIndex)
        {
            Color tint = ZoneTint(settings, role, zoneIndex);
            SetColorIfPresent(material, "_BaseColor", tint);
            SetColorIfPresent(material, "_Color", tint);
        }

        internal static Color ZoneTint(
            GeneratedWorldSurfaceTextureSettings settings,
            GeneratedWorldSurfaceMaterialRole role,
            int zoneIndex)
        {
            if (zoneIndex <= 0 || settings.SurfaceZonePaletteStrength <= 0f)
            {
                return Color.white;
            }

            Color[] tints =
            {
                new(0.96f, 0.88f, 0.80f, 1f),
                new(0.82f, 0.94f, 0.92f, 1f),
                new(0.98f, 0.90f, 0.70f, 1f),
                new(0.90f, 0.86f, 0.98f, 1f)
            };
            Color target = tints[Mod(zoneIndex - 1, tints.Length)];
            float roleStrength = role switch
            {
                GeneratedWorldSurfaceMaterialRole.Wall => 0.48f,
                GeneratedWorldSurfaceMaterialRole.Ramp => 0.34f,
                GeneratedWorldSurfaceMaterialRole.Organic => 0.20f,
                _ => 0.30f
            };
            float variation = Mathf.Lerp(0.86f, 1.08f, Hash01(zoneIndex, (int)role, settings.Seed + settings.ActNumber * 97));
            Color tinted = target * variation;
            tinted.a = 1f;
            Color result = Color.Lerp(Color.white, tinted, settings.SurfaceZonePaletteStrength * roleStrength);
            result.a = 1f;
            return result;
        }

        private static GeneratedWorldSurfaceTextureBundleData BuildTextureBundle(
            GeneratedWorldSurfaceTextureSettings settings,
            GeneratedWorldSurfaceMaterialRole role)
        {
            GeneratedWorldOrganicPatternData organicPattern = role == GeneratedWorldSurfaceMaterialRole.Organic
                ? BuildOrganicPattern(settings, RoleSeed(settings.Seed, settings.ActNumber, role) + 24001)
                : default;
            GeneratedWorldSurfaceTextureData albedo = BuildTextureData(settings, role, organicPattern);

            float[] heightValues = BuildHeightValues(settings, role, organicPattern);
            return new GeneratedWorldSurfaceTextureBundleData(
                albedo,
                BuildNormalTextureData(settings, role, heightValues),
                BuildHeightTextureData(settings, role, heightValues),
                BuildSmoothnessTextureData(settings, role));
        }

        private static Texture2D CreateTexture(GeneratedWorldSurfaceTextureData data)
        {
            var texture = new Texture2D(data.Width, data.Height, TextureFormat.RGBA32, mipChain: true, linear: data.Linear)
            {
                name = data.Name,
                hideFlags = HideFlags.DontSave,
                wrapMode = data.WrapMode,
                filterMode = data.FilterMode,
                anisoLevel = 4
            };
            texture.SetPixels32(data.Pixels);
            texture.Apply(updateMipmaps: true, makeNoLongerReadable: true);
            return texture;
        }

        private static Texture2D CreateTextureOrNull(GeneratedWorldSurfaceTextureData data)
        {
            return data.Pixels == null || data.Pixels.Length == 0 ? null : CreateTexture(data);
        }

        private static Material CreateMaterial(
            Shader shader,
            GeneratedWorldSurfaceTextureSettings settings,
            string name,
            Texture albedoTexture,
            Texture normalTexture,
            Texture heightTexture,
            Texture smoothnessTexture,
            GeneratedWorldSurfaceMaterialRole role)
        {
            var material = new Material(shader)
            {
                name = name,
                hideFlags = HideFlags.DontSave
            };
            SetTextureIfPresent(material, "_BaseMap", albedoTexture);
            SetTextureIfPresent(material, "_MainTex", albedoTexture);
            SetTextureIfPresent(material, "_BumpMap", normalTexture);
            SetTextureIfPresent(material, "_ParallaxMap", heightTexture);
            SetTextureIfPresent(material, "_MetallicGlossMap", smoothnessTexture);
            SetColorIfPresent(material, "_BaseColor", Color.white);
            SetColorIfPresent(material, "_Color", Color.white);
            SetFloatIfPresent(material, "_Metallic", 0f);
            SetFloatIfPresent(material, "_BumpScale", MaterialBumpScale(role) * settings.SurfaceReliefStrength);
            SetFloatIfPresent(material, "_Parallax", MaterialParallax(role) * settings.SurfaceReliefStrength);
            SetFloatIfPresent(material, "_Smoothness", role == GeneratedWorldSurfaceMaterialRole.Ramp ? 0.18f : 0.12f);
            SetFloatIfPresent(material, "_GlossMapScale", 1f);
            SetFloatIfPresent(material, "_SmoothnessTextureChannel", 0f);
            SetFloatIfPresent(material, "_SpecularHighlights", 0f);
            ApplyTextureScale(material, settings, role);
            if (normalTexture != null)
            {
                material.EnableKeyword("_NORMALMAP");
            }

            if (heightTexture != null)
            {
                material.EnableKeyword("_PARALLAXMAP");
            }

            if (smoothnessTexture != null)
            {
                material.EnableKeyword("_METALLICSPECGLOSSMAP");
            }

            return material;
        }

        private static float MaterialBumpScale(GeneratedWorldSurfaceMaterialRole role)
        {
            return role switch
            {
                GeneratedWorldSurfaceMaterialRole.Organic => 0.35f,
                GeneratedWorldSurfaceMaterialRole.Wall => 0.24f,
                GeneratedWorldSurfaceMaterialRole.Ramp => 0.72f,
                GeneratedWorldSurfaceMaterialRole.Rim => 0.18f,
                _ => 0.42f
            };
        }

        private static float MaterialParallax(GeneratedWorldSurfaceMaterialRole role)
        {
            return role switch
            {
                GeneratedWorldSurfaceMaterialRole.Organic => 0.012f,
                GeneratedWorldSurfaceMaterialRole.Wall => 0.006f,
                GeneratedWorldSurfaceMaterialRole.Ramp => 0.030f,
                GeneratedWorldSurfaceMaterialRole.Rim => 0.004f,
                _ => 0.014f
            };
        }

        private static Shader ResolveShader()
        {
            return Shader.Find("TheCircussyOne/Generated World Surface Lit")
                ?? Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Standard");
        }

        private static Color BaseColor(GeneratedWorldSurfaceTextureSettings settings, GeneratedWorldSurfaceMaterialRole role)
        {
            SurfaceStyleProfile profile = StyleProfile(settings);
            return role switch
            {
                GeneratedWorldSurfaceMaterialRole.Organic => StyledColor(profile.OrganicBase, settings),
                GeneratedWorldSurfaceMaterialRole.Wall => StyledColor(Color.Lerp(settings.WallColor, profile.WallRed, settings.StyleVariationStrength), settings),
                GeneratedWorldSurfaceMaterialRole.Ramp => StyledColor(Color.Lerp(settings.RampColor, profile.RampBase, settings.StyleVariationStrength), settings),
                GeneratedWorldSurfaceMaterialRole.Rim => StyledColor(Color.Lerp(settings.RimColor, profile.RimBase, settings.StyleVariationStrength), settings),
                _ => StyledColor(Color.Lerp(settings.TopColor, profile.TopBase, settings.StyleVariationStrength), settings)
            };
        }

        private static SurfaceStyleProfile StyleProfile(GeneratedWorldSurfaceTextureSettings settings)
        {
            int index = Mathf.Clamp(settings.SelectedStyleProfileIndex, 0, SurfaceStyleProfiles.Length - 1);
            return SurfaceStyleProfiles[index];
        }

        private static Color StyledColor(Color color, GeneratedWorldSurfaceTextureSettings settings)
        {
            float luma = color.r * 0.2126f + color.g * 0.7152f + color.b * 0.0722f;
            var gray = new Color(luma, luma, luma, color.a);
            Color styled = Color.Lerp(gray, color, Mathf.Lerp(0.62f, 1f, settings.SurfaceColorfulness));
            styled.r = Mathf.Clamp01(styled.r);
            styled.g = Mathf.Clamp01(styled.g);
            styled.b = Mathf.Clamp01(styled.b);
            styled.a = color.a;
            return styled;
        }

        private static Color PatternColor(
            GeneratedWorldSurfaceTextureSettings settings,
            GeneratedWorldSurfaceMaterialRole role,
            float u,
            float v,
            int seed,
            Color baseColor,
            GeneratedWorldOrganicPatternData organicPattern)
        {
            return role switch
            {
                GeneratedWorldSurfaceMaterialRole.Organic => OrganicGroundSurfaceColor(settings, u, v, seed, baseColor, organicPattern),
                GeneratedWorldSurfaceMaterialRole.Wall => WallPanelColor(settings, u, v, seed, baseColor),
                GeneratedWorldSurfaceMaterialRole.Ramp => RampPlankColor(settings, u, v, seed, baseColor),
                GeneratedWorldSurfaceMaterialRole.Rim => Color.Lerp(baseColor, StyleProfile(settings).AccentRed, 0.22f),
                _ => CheckerTileColor(settings, u, v, seed, baseColor)
            };
        }

        private static Color CheckerTileColor(
            GeneratedWorldSurfaceTextureSettings settings,
            float u,
            float v,
            int seed,
            Color baseColor)
        {
            SurfaceStyleProfile profile = StyleProfile(settings);
            int cellX = TopTileCoordinate(u);
            int cellY = TopTileCoordinate(v);
            bool light = ((cellX + cellY) & 1) == 0;
            Color tile = light ? profile.CheckerLight : profile.CheckerDark;
            float tileVariation = Hash01(cellX, cellY, seed + 109);
            Color variationTarget = tileVariation > 0.5f ? profile.TopBase : profile.RimBase;
            tile = Color.Lerp(tile, variationTarget, Mathf.Abs(tileVariation - 0.5f) * 0.12f * settings.StyleVariationStrength);
            float accentRoll = Hash01(cellX, cellY, seed + 211);
            if (accentRoll > 0.83f)
            {
                Color accent = accentRoll > 0.93f ? profile.AccentGold : profile.AccentRed;
                tile = Color.Lerp(tile, accent, (light ? 0.22f : 0.15f) * settings.StyleVariationStrength);
            }

            return StyledColor(Color.Lerp(tile, baseColor, 0.18f), settings);
        }

        private static Color OrganicGroundSurfaceColor(
            GeneratedWorldSurfaceTextureSettings settings,
            float u,
            float v,
            int seed,
            Color baseColor,
            GeneratedWorldOrganicPatternData organicPattern)
        {
            SurfaceStyleProfile profile = StyleProfile(settings);
            float patternScale = OrganicPatternFrequency(settings);
            float patternU = Mathf.Repeat(u * patternScale, 1f);
            float patternV = Mathf.Repeat(v * patternScale, 1f);
            float reaction = organicPattern.Sample(patternU, patternV);
            float cellular = SmoothStep(0.39f, 0.77f, reaction);
            float vein = Mathf.Abs(reaction - 0.52f);
            vein = 1f - SmoothStep(0.055f, 0.18f, vein);
            float microGrit = TileableValueNoise(u * 58f, v * 61f, seed + 1879, 58, 61);
            float dust = TileableValueNoise(u * 21f, v * 19f, seed + 1921, 21, 19);
            float warmDust = TileableValueNoise(u * 9f, v * 11f, seed + 2131, 9, 11);
            Color darkSoil = Color.Lerp(DirtColor, profile.OrganicBase, 0.40f);
            Color warmDirt = Color.Lerp(profile.OrganicBase, DustColor, 0.22f + warmDust * 0.10f);
            Color pattern = Color.Lerp(profile.OrganicPattern, profile.AccentCool, 0.10f * settings.StyleVariationStrength);
            Color soil = Color.Lerp(darkSoil, warmDirt, dust * 0.22f + microGrit * 0.06f);
            soil = Color.Lerp(soil, pattern, cellular * settings.OrganicPatternStrength * 1.55f);
            soil = Color.Lerp(soil, DirtColor, vein * settings.OrganicPatternStrength * 0.42f * settings.SurfaceWearStrength);
            soil = Color.Lerp(soil, profile.AccentGold, microGrit * settings.GritStrength * 0.025f);
            soil = Color.Lerp(soil, baseColor, 0.06f);
            return StyledColor(soil, settings);
        }

        private static Color WallPanelColor(
            GeneratedWorldSurfaceTextureSettings settings,
            float u,
            float v,
            int seed,
            Color baseColor)
        {
            SurfaceStyleProfile profile = StyleProfile(settings);
            int panel = WallPanelCoordinate(u);
            float roll = Hash01(panel, 0, seed + 503);
            float seam = GridLine(u, v, GeneratedWorldSurfaceMaterialRole.Wall) * settings.WallPanelSeamStrength;
            float edgeWear = WallEdgeWear(u, v) * settings.WallEdgeWearStrength;
            float verticalWear = Mathf.Pow(1f - v, 1.45f) * settings.WallVerticalWearStrength;
            float streaks = VerticalStreaks(u, v) * settings.WallVerticalWearStrength;
            Color panelColor = roll < 0.42f
                ? profile.WallRed
                : roll < 0.74f
                    ? profile.WallAlternate
                    : profile.WallDark;
            panelColor = Color.Lerp(panelColor, profile.WallDark, 0.12f);
            panelColor = Color.Lerp(panelColor, baseColor, 0.14f);
            panelColor = Color.Lerp(panelColor, DirtColor, verticalWear * 0.18f * settings.SurfaceWearStrength);
            panelColor = Color.Lerp(panelColor, profile.AccentGold, edgeWear * 0.055f * settings.SurfaceWearStrength);
            panelColor = Color.Lerp(panelColor, DirtColor, streaks * 0.050f * settings.SurfaceWearStrength);
            return StyledColor(Color.Lerp(panelColor, DirtColor, seam * 0.22f), settings);
        }

        private static Color RampPlankColor(
            GeneratedWorldSurfaceTextureSettings settings,
            float u,
            float v,
            int seed,
            Color baseColor)
        {
            SurfaceStyleProfile profile = StyleProfile(settings);
            int plank = RampPlankCoordinate(u);
            float roll = Hash01(plank, 0, seed + 761);
            float seam = RampPlankSeam(u, RampPlankCount) * settings.RampPlankSeamStrength;
            float edgeWear = RampEdgeWear(u, v) * settings.RampEdgeWearStrength;
            float grain = WoodFiber(u, v, seed + 887, 34f, 0.14f) * settings.RampWoodGrainStrength;
            float knot = RampKnot(u, v, seed + 929);
            Color plankColor = Color.Lerp(profile.RampBase, baseColor, 0.07f + roll * 0.08f);
            plankColor = Color.Lerp(plankColor, profile.RampGrain, Mathf.Clamp01(grain) * 0.32f);
            plankColor = Color.Lerp(plankColor, profile.AccentRed, knot * 0.08f);
            plankColor = Color.Lerp(plankColor, profile.AccentGold, edgeWear * 0.10f * settings.SurfaceWearStrength);
            return StyledColor(Color.Lerp(plankColor, DirtColor, seam * 0.62f + knot * 0.16f), settings);
        }

        private static Color ApplySurfaceAccent(
            Color color,
            GeneratedWorldSurfaceMaterialRole role,
            float u,
            float v,
            float grid,
            float rampCue,
            GeneratedWorldSurfaceTextureSettings settings)
        {
            SurfaceStyleProfile profile = StyleProfile(settings);
            if (role == GeneratedWorldSurfaceMaterialRole.Top)
            {
                float grout = TopGroutLine(u, v);
                if (grout <= 0f)
                {
                    return color;
                }

                Color accent = Mathf.Repeat((u + v) * 2f, 1f) > 0.5f ? profile.AccentGold : profile.AccentRed;
                Color lineColor = Color.Lerp(accent, DirtColor, 0.54f);
                float edgeWear = TopTileEdgeWear(u, v) * settings.TopTileEdgeWearStrength;
                color = Color.Lerp(color, DirtColor, edgeWear * settings.DirtStrength * settings.SurfaceWearStrength * 0.07f);
                color = Color.Lerp(color, profile.AccentGold, edgeWear * settings.DirtStrength * settings.SurfaceWearStrength * 0.035f);
                return Color.Lerp(color, lineColor, grout * settings.GridStrength * 0.16f);
            }

            if (role == GeneratedWorldSurfaceMaterialRole.Organic && grid > 0f)
            {
                return Color.Lerp(color, DirtColor, grid * settings.GridStrength * 0.08f);
            }

            if (role == GeneratedWorldSurfaceMaterialRole.Wall && grid > 0f)
            {
                return color;
            }

            if (role == GeneratedWorldSurfaceMaterialRole.Ramp && rampCue > 0f)
            {
                Color cue = Mathf.Abs(u - 0.5f) > 0.38f ? profile.AccentGold : DirtColor;
                return Color.Lerp(color, cue, rampCue * 0.10f);
            }

            if (role == GeneratedWorldSurfaceMaterialRole.Rim)
            {
                return Color.Lerp(color, DustColor, Mathf.Pow(v, 1.7f) * settings.DirtStrength * 0.10f);
            }

            return color;
        }

        private static float DirtBlend(GeneratedWorldSurfaceMaterialRole role)
        {
            return role switch
            {
                GeneratedWorldSurfaceMaterialRole.Organic => 0.52f,
                GeneratedWorldSurfaceMaterialRole.Wall => 0.38f,
                GeneratedWorldSurfaceMaterialRole.Ramp => 0.48f,
                GeneratedWorldSurfaceMaterialRole.Rim => 0.46f,
                _ => 0.42f
            };
        }

        private static float DirtAmount(
            float u,
            float v,
            GeneratedWorldSurfaceMaterialRole role,
            float broadGrit,
            float fineGrit,
            int seed,
            GeneratedWorldSurfaceTextureSettings settings)
        {
            float baseDirt = broadGrit * 0.65f + fineGrit * 0.35f;
            float amount = role switch
            {
                GeneratedWorldSurfaceMaterialRole.Organic => Mathf.Clamp01(
                    baseDirt * settings.GritStrength * 0.76f
                    + TileableValueNoise(u * 6f, v * 7f, seed + 2113, 6, 7) * settings.DirtStrength * 0.30f
                    + EdgeWear(u) * settings.DirtStrength * 0.08f),
                GeneratedWorldSurfaceMaterialRole.Wall => Mathf.Clamp01(
                    baseDirt * settings.GritStrength * 0.70f
                    + VerticalStreaks(u, v) * settings.DirtStrength * 0.58f
                    + Mathf.Pow(1f - v, 1.8f) * settings.DirtStrength * 0.38f),
                GeneratedWorldSurfaceMaterialRole.Ramp => Mathf.Clamp01(
                    baseDirt * settings.GritStrength * 0.62f
                    + EdgeWear(u) * settings.DirtStrength * 0.38f),
                GeneratedWorldSurfaceMaterialRole.Rim => Mathf.Clamp01(
                    baseDirt * settings.GritStrength * 0.48f
                    + Mathf.Pow(v, 1.5f) * settings.DirtStrength * 0.34f),
                _ => Mathf.Clamp01(
                    baseDirt * settings.GritStrength * 0.52f
                    + EdgeWear(u) * settings.DirtStrength * 0.14f)
            };
            return Mathf.Clamp01(amount * Mathf.Lerp(0.30f, 1.18f, settings.SurfaceWearStrength));
        }

        private static float GridLine(float u, float v, GeneratedWorldSurfaceMaterialRole role)
        {
            float cells = role switch
            {
                GeneratedWorldSurfaceMaterialRole.Wall => 7f,
                GeneratedWorldSurfaceMaterialRole.Ramp => RampPlankCount,
                _ => 4f
            };
            if (role == GeneratedWorldSurfaceMaterialRole.Organic)
            {
                return 0f;
            }

            if (role == GeneratedWorldSurfaceMaterialRole.Wall)
            {
                return WallPanelSeam(u);
            }

            float x = Mathf.Abs(Mathf.Repeat(u * cells, 1f) - 0.5f);
            float y = Mathf.Abs(Mathf.Repeat(v * cells, 1f) - 0.5f);
            float threshold = role == GeneratedWorldSurfaceMaterialRole.Top ? 0.44f : 0.455f;
            float line = 1f - SmoothStep(threshold, 0.50f, Mathf.Max(x, y));
            return Mathf.Clamp01(line);
        }

        private static float GridDarkening(GeneratedWorldSurfaceMaterialRole role)
        {
            return role switch
            {
                GeneratedWorldSurfaceMaterialRole.Organic => 0.045f,
                GeneratedWorldSurfaceMaterialRole.Wall => 0.10f,
                GeneratedWorldSurfaceMaterialRole.Ramp => 0.10f,
                GeneratedWorldSurfaceMaterialRole.Rim => 0.06f,
                _ => 0.09f
            };
        }

        private static float RampCue(float u, float v)
        {
            float crossBand = 1f - SmoothStep(0.08f, 0.22f, Mathf.Abs(Mathf.Repeat(v * 7f, 1f) - 0.5f));
            float sideWear = EdgeWear(u);
            return Mathf.Clamp01(crossBand * 0.72f + sideWear * 0.35f);
        }

        private static float[] BuildHeightValues(
            GeneratedWorldSurfaceTextureSettings settings,
            GeneratedWorldSurfaceMaterialRole role,
            GeneratedWorldOrganicPatternData organicPattern)
        {
            int size = settings.Resolution;
            int seed = RoleSeed(settings.Seed, settings.ActNumber, role) + 4001;
            var values = new float[size * size];
            for (int y = 0; y < size; y++)
            {
                float v = y / (float)(size - 1);
                for (int x = 0; x < size; x++)
                {
                    float u = x / (float)(size - 1);
                    values[y * size + x] = SurfaceHeight(settings, role, u, v, seed, organicPattern);
                }
            }

            return values;
        }

        private static GeneratedWorldSurfaceTextureData BuildHeightTextureData(
            GeneratedWorldSurfaceTextureSettings settings,
            GeneratedWorldSurfaceMaterialRole role,
            float[] heightValues)
        {
            int size = settings.Resolution;
            var pixels = new Color32[size * size];
            for (int i = 0; i < heightValues.Length; i++)
            {
                byte value = (byte)Mathf.RoundToInt(Mathf.Clamp01(heightValues[i]) * 255f);
                pixels[i] = new Color32(value, value, value, 255);
            }

            return new GeneratedWorldSurfaceTextureData(
                $"Generated World {role} Height",
                size,
                size,
                pixels,
                TextureWrapMode.Repeat,
                FilterMode.Bilinear,
                linear: true);
        }

        private static GeneratedWorldSurfaceTextureData BuildNormalTextureData(
            GeneratedWorldSurfaceTextureSettings settings,
            GeneratedWorldSurfaceMaterialRole role,
            float[] heightValues)
        {
            int size = settings.Resolution;
            var pixels = new Color32[size * size];
            float strength = NormalStrength(role) * settings.SurfaceReliefStrength;

            for (int y = 0; y < size; y++)
            {
                int ym = (y - 1 + size) % size;
                int yp = (y + 1) % size;
                for (int x = 0; x < size; x++)
                {
                    int xm = (x - 1 + size) % size;
                    int xp = (x + 1) % size;
                    float dx = heightValues[y * size + xm] - heightValues[y * size + xp];
                    float dy = heightValues[ym * size + x] - heightValues[yp * size + x];
                    Vector3 normal = new Vector3(dx * strength, dy * strength, 1f).normalized;
                    pixels[y * size + x] = new Color32(
                        (byte)Mathf.RoundToInt((normal.x * 0.5f + 0.5f) * 255f),
                        (byte)Mathf.RoundToInt((normal.y * 0.5f + 0.5f) * 255f),
                        (byte)Mathf.RoundToInt(Mathf.Clamp01(normal.z) * 255f),
                        255);
                }
            }

            return new GeneratedWorldSurfaceTextureData(
                $"Generated World {role} Normal",
                size,
                size,
                pixels,
                TextureWrapMode.Repeat,
                FilterMode.Bilinear,
                linear: true);
        }

        private static float SurfaceHeight(
            GeneratedWorldSurfaceTextureSettings settings,
            GeneratedWorldSurfaceMaterialRole role,
            float u,
            float v,
            int seed,
            GeneratedWorldOrganicPatternData organicPattern)
        {
            return role switch
            {
                GeneratedWorldSurfaceMaterialRole.Organic => OrganicGroundHeight(settings, u, v, seed, organicPattern),
                GeneratedWorldSurfaceMaterialRole.Wall => WallHeight(settings, u, v, seed),
                GeneratedWorldSurfaceMaterialRole.Ramp => RampHeight(settings, u, v, seed),
                GeneratedWorldSurfaceMaterialRole.Rim => RimHeight(u, v, seed),
                _ => TopHeight(settings, u, v, seed)
            };
        }

        private static float TopHeight(GeneratedWorldSurfaceTextureSettings settings, float u, float v, int seed)
        {
            int tileX = TopTileCoordinate(u);
            int tileY = TopTileCoordinate(v);
            float tileOffset = Mathf.Lerp(
                -settings.TopTileHeightVariation,
                settings.TopTileHeightVariation,
                Hash01(tileX, tileY, seed + 1703)) * settings.TopTileReliefStrength;
            float checker = ((tileX + tileY) & 1) == 0 ? 0.028f : -0.010f;
            float grout = TopGroutLine(u, v);
            float edgeWear = TopTileEdgeWear(u, v) * settings.TopTileEdgeWearStrength;
            float edgeChip = TileableValueNoise(u * 31f, v * 29f, seed + 929, 31, 29);
            float grit = TileableValueNoise(u * 42f, v * 42f, seed + 19, 42, 42) - 0.5f;
            float weave = Weave(u, v, GeneratedWorldSurfaceMaterialRole.Top);
            float tileHeight =
                0.55f
                + checker
                + tileOffset
                + grit * 0.033f
                + weave * 0.020f
                - edgeWear * settings.SurfaceWearStrength * (0.026f + edgeChip * 0.020f);
            float trenchHeight = 0.20f + grit * 0.008f + weave * 0.004f - edgeWear * settings.SurfaceWearStrength * 0.018f;
            float groutCut = Mathf.Clamp01(grout * settings.TopTileGroutDepthStrength * 0.86f);
            return Mathf.Clamp01(Mathf.Lerp(tileHeight, trenchHeight, groutCut));
        }

        private static float OrganicGroundHeight(
            GeneratedWorldSurfaceTextureSettings settings,
            float u,
            float v,
            int seed,
            GeneratedWorldOrganicPatternData organicPattern)
        {
            float patternScale = OrganicPatternFrequency(settings);
            float patternU = Mathf.Repeat(u * patternScale, 1f);
            float patternV = Mathf.Repeat(v * patternScale, 1f);
            float reaction = organicPattern.Sample(patternU, patternV);
            float cellular = SmoothStep(0.38f, 0.78f, reaction) - 0.5f;
            float vein = Mathf.Abs(reaction - 0.52f);
            vein = 1f - SmoothStep(0.055f, 0.18f, vein);
            float fine = TileableValueNoise(u * 62f, v * 58f, seed + 41, 62, 58) - 0.5f;
            float grit = TileableValueNoise(u * 27f, v * 29f, seed + 83, 27, 29) - 0.5f;
            float patternStrength = Mathf.Lerp(0.45f, 1f, settings.OrganicPatternStrength);
            return Mathf.Clamp01(0.50f + cellular * 0.125f * patternStrength - vein * 0.030f + fine * 0.033f + grit * 0.022f);
        }

        private static float WallHeight(GeneratedWorldSurfaceTextureSettings settings, float u, float v, int seed)
        {
            int panel = WallPanelCoordinate(u);
            float panelOffset = Mathf.Lerp(
                -settings.WallPanelHeightVariation,
                settings.WallPanelHeightVariation,
                Hash01(panel, 0, seed + 991));
            float grid = GridLine(u, v, GeneratedWorldSurfaceMaterialRole.Wall);
            float streaks = VerticalStreaks(u, v);
            float fabric = Weave(u, v, GeneratedWorldSurfaceMaterialRole.Wall);
            float lowerWear = Mathf.Pow(1f - v, 1.8f);
            float edgeWear = WallEdgeWear(u, v) * settings.WallEdgeWearStrength;
            return Mathf.Clamp01(
                0.52f
                + panelOffset
                + fabric * 0.026f
                + streaks * 0.046f * settings.WallVerticalWearStrength
                - grid * 0.20f * settings.WallPanelSeamStrength
                - lowerWear * 0.035f * settings.WallVerticalWearStrength * settings.SurfaceWearStrength
                - edgeWear * 0.045f * settings.SurfaceWearStrength);
        }

        private static float RampHeight(GeneratedWorldSurfaceTextureSettings settings, float u, float v, int seed)
        {
            int plank = RampPlankCoordinate(u);
            float raised = (Hash01(plank, 0, seed + 101) - 0.5f) * 0.10f;
            float seam = RampPlankSeam(u, RampPlankCount) * settings.RampPlankSeamStrength;
            float edgeWear = RampEdgeWear(u, v) * settings.RampEdgeWearStrength;
            float grain = WoodFiber(u, v, seed + 211, 48f, 0.16f) * settings.RampWoodGrainStrength;
            float knot = RampKnot(u, v, seed + 307);
            return Mathf.Clamp01(
                0.54f
                + raised
                + grain * 0.08f
                + knot * 0.06f
                - seam * 0.34f
                - edgeWear * 0.040f * settings.SurfaceWearStrength);
        }

        private static float RimHeight(float u, float v, int seed)
        {
            float grain = TileableValueNoise(u * 20f, v * 16f, seed + 139, 20, 16) - 0.5f;
            float dirtBand = Mathf.Pow(v, 1.4f);
            float weave = Weave(u, v, GeneratedWorldSurfaceMaterialRole.Rim);
            return Mathf.Clamp01(0.50f + grain * 0.040f + weave * 0.018f - dirtBand * 0.060f);
        }

        private static bool CheckerParity(float u, float v)
        {
            int cellX = TopTileCoordinate(u);
            int cellY = TopTileCoordinate(v);
            return ((cellX + cellY) & 1) == 0;
        }

        private static float SurfaceSmoothness(
            GeneratedWorldSurfaceTextureSettings settings,
            GeneratedWorldSurfaceMaterialRole role,
            float u,
            float v,
            int seed)
        {
            float variationStrength = settings.SurfaceSmoothnessVariationStrength;
            switch (role)
            {
                case GeneratedWorldSurfaceMaterialRole.Top:
                {
                    int tileX = TopTileCoordinate(u);
                    int tileY = TopTileCoordinate(v);
                    float tileRoll = Hash01(tileX, tileY, seed + 31);
                    tileRoll = Mathf.Clamp01((tileRoll - 0.5f) * settings.TopTileSmoothnessVariationStrength + 0.5f);
                    float tileSmoothness = Mathf.Lerp(
                        settings.TopSmoothnessMin,
                        settings.TopSmoothnessMax,
                        tileRoll);
                    float micro = TileableValueNoise(u * 40f, v * 44f, seed + 73, 40, 44) - 0.5f;
                    float grout = TopGroutLine(u, v);
                    float edgeWear = TopTileEdgeWear(u, v) * settings.TopTileEdgeWearStrength;
                    return Mathf.Clamp01(
                        tileSmoothness
                        + micro * 0.040f * variationStrength * settings.TopTileSmoothnessVariationStrength
                        - grout * 0.055f * settings.TopTileGroutDepthStrength
                        - edgeWear * 0.040f * settings.SurfaceWearStrength);
                }

                case GeneratedWorldSurfaceMaterialRole.Wall:
                {
                    int panel = WallPanelCoordinate(u);
                    float panelSmoothness = Mathf.Lerp(
                        settings.WallSmoothnessMin,
                        settings.WallSmoothnessMax,
                        Hash01(panel, 0, seed + 107));
                    float streaks = VerticalStreaks(u, v) - 0.5f;
                    float grid = GridLine(u, v, role) * settings.WallPanelSeamStrength;
                    float edgeWear = WallEdgeWear(u, v) * settings.WallEdgeWearStrength;
                    float lowerDust = Mathf.Pow(1f - v, 1.5f);
                    return Mathf.Clamp01(
                        panelSmoothness
                        + streaks * 0.035f * variationStrength * settings.WallVerticalWearStrength
                        - grid * 0.105f
                        - lowerDust * 0.035f * settings.WallVerticalWearStrength * settings.SurfaceWearStrength
                        - edgeWear * 0.020f * settings.SurfaceWearStrength);
                }

                case GeneratedWorldSurfaceMaterialRole.Ramp:
                {
                    float plank = Hash01(RampPlankCoordinate(u), 0, seed + 151);
                    float grain = WoodFiber(u, v, seed + 173, 36f, 0.12f) - 0.5f;
                    float seam = RampPlankSeam(u, RampPlankCount) * settings.RampPlankSeamStrength;
                    float edgeWear = RampEdgeWear(u, v) * settings.RampEdgeWearStrength;
                    return Mathf.Clamp01(
                        Mathf.Lerp(0.075f, 0.19f, plank)
                        + grain * 0.040f * variationStrength * settings.RampWoodGrainStrength
                        - seam * 0.052f
                        - edgeWear * 0.135f * Mathf.Lerp(0.65f, 1.15f, settings.SurfaceWearStrength));
                }

                case GeneratedWorldSurfaceMaterialRole.Organic:
                {
                    float dirt = TileableValueNoise(u * 16f, v * 16f, seed + 191, 16, 16);
                    return Mathf.Clamp01(Mathf.Lerp(0.055f, 0.145f, dirt) - settings.DirtStrength * settings.SurfaceWearStrength * 0.035f);
                }

                case GeneratedWorldSurfaceMaterialRole.Rim:
                {
                    float grit = TileableValueNoise(u * 18f, v * 18f, seed + 211, 18, 18);
                    return Mathf.Clamp01(Mathf.Lerp(0.060f, 0.135f, grit));
                }

                default:
                    return 0.12f;
            }
        }

        private static int TopTileCoordinate(float value)
        {
            return Mod(Mathf.FloorToInt(value * TopTileCells), TopTileCells);
        }

        private static float TopTileEdgeWear(float u, float v)
        {
            float x = Mathf.Abs(Mathf.Repeat(u * TopTileCells, 1f) - 0.5f);
            float y = Mathf.Abs(Mathf.Repeat(v * TopTileCells, 1f) - 0.5f);
            float nearEdge = SmoothStep(0.24f, 0.49f, Mathf.Max(x, y));
            float grout = TopGroutLine(u, v);
            return Mathf.Clamp01(nearEdge - grout * 0.35f);
        }

        private static float OrganicPatternFrequency(GeneratedWorldSurfaceTextureSettings settings)
        {
            return Mathf.Max(1f, Mathf.Round(settings.OrganicPatternScale));
        }

        private static float TopGroutLine(float u, float v)
        {
            float x = Mathf.Abs(Mathf.Repeat(u * TopTileCells, 1f) - 0.5f);
            float y = Mathf.Abs(Mathf.Repeat(v * TopTileCells, 1f) - 0.5f);
            float fromCenter = Mathf.Max(x, y);
            return SmoothStep(0.44f, 0.50f, fromCenter);
        }

        private static int WallPanelCoordinate(float value)
        {
            return Mod(Mathf.FloorToInt(value * WallPanelCount), WallPanelCount);
        }

        private static int RampPlankCoordinate(float value)
        {
            return Mod(Mathf.FloorToInt(value * RampPlankCount), RampPlankCount);
        }

        private static float RampPlankSeam(float u, float plankCount)
        {
            float within = Mathf.Repeat(u * plankCount, 1f);
            float distanceToSeam = Mathf.Min(within, 1f - within);
            return 1f - SmoothStep(0.018f, 0.065f, distanceToSeam);
        }

        private static float WallPanelSeam(float u)
        {
            float within = Mathf.Repeat(u * WallPanelCount, 1f);
            float distanceToSeam = Mathf.Min(within, 1f - within);
            return 1f - SmoothStep(0.016f, 0.060f, distanceToSeam);
        }

        private static float RampKnot(float u, float v, int seed)
        {
            float knot = 0f;
            for (int i = 0; i < 3; i++)
            {
                float centerU = Hash01(i, 0, seed + 31);
                float centerV = Hash01(i, 1, seed + 67);
                float du = Mathf.Abs(u - centerU);
                du = Mathf.Min(du, 1f - du);
                float dv = Mathf.Abs(v - centerV);
                dv = Mathf.Min(dv, 1f - dv);
                float oval = Mathf.Sqrt(du * du * 42f + dv * dv * 9f);
                knot = Mathf.Max(knot, 1f - SmoothStep(0.035f, 0.12f, oval));
            }

            return Mathf.Clamp01(knot);
        }

        private static float WoodFiber(float u, float v, int seed, float cycles, float warpAmount)
        {
            float warp = TileableValueNoise(u * 6f, v * 6f, seed + 19, 6, 6) * warpAmount;
            float line = Mathf.Sin((v * cycles + warp) * Mathf.PI * 2f) * 0.5f + 0.5f;
            float broken = TileableValueNoise(u * 16f, v * 32f, seed + 47, 16, 32);
            return Mathf.Clamp01(line * 0.55f + broken * 0.45f);
        }

        private static float EdgeWear(float u)
        {
            return Mathf.Clamp01(1f - Mathf.Min(u, 1f - u) * 7f);
        }

        private static float RampEdgeWear(float u, float v)
        {
            float sideWear = EdgeWear(u);
            float endWear = Mathf.Clamp01(1f - Mathf.Min(v, 1f - v) * 9f);
            return Mathf.Clamp01(sideWear * 0.80f + endWear * 0.28f);
        }

        private static float WallEdgeWear(float u, float v)
        {
            float sideWear = EdgeWear(u);
            float bottomWear = Mathf.Pow(1f - v, 1.45f);
            float topWear = Mathf.Pow(v, 2.2f);
            return Mathf.Clamp01(sideWear * 0.45f + bottomWear * 0.60f + topWear * 0.16f);
        }

        private static float VerticalStreaks(float u, float v)
        {
            float narrow = Mathf.Pow(TileableValueNoise(u * 36f, v * 3f, 1009, 36, 3), 2.2f);
            float broad = TileableValueNoise(u * 12f, v * 2f, 1447, 12, 2);
            return Mathf.Clamp01(narrow * 0.65f + broad * 0.35f);
        }

        private static float Weave(float u, float v, GeneratedWorldSurfaceMaterialRole role)
        {
            float multiplier = role == GeneratedWorldSurfaceMaterialRole.Wall ? 0.55f : 1f;
            if (role == GeneratedWorldSurfaceMaterialRole.Organic)
            {
                multiplier = 0.72f;
            }
            float xWave = Mathf.Sin(u * Mathf.PI * 2f * 18f) * 0.5f + 0.5f;
            float yWave = Mathf.Sin(v * Mathf.PI * 2f * 21f) * 0.5f + 0.5f;
            float broad = Mathf.Sin((u + v) * Mathf.PI * 2f * 4f) * 0.5f + 0.5f;
            return ((xWave * 0.34f + yWave * 0.40f + broad * 0.26f) - 0.5f) * multiplier;
        }

        private static GeneratedWorldOrganicPatternData BuildOrganicPattern(
            GeneratedWorldSurfaceTextureSettings settings,
            int seed)
        {
            int size = Mathf.Clamp(settings.Resolution / 2, 64, 256);
            int length = size * size;
            float[] a = new float[length];
            float[] b = new float[length];
            float[] nextA = new float[length];
            float[] nextB = new float[length];

            for (int i = 0; i < length; i++)
            {
                a[i] = 1f;
            }

            int spotCount = Mathf.Clamp(size / 6, 14, 42);
            for (int spot = 0; spot < spotCount; spot++)
            {
                float centerU = Hash01(spot, 0, seed + 31);
                float centerV = Hash01(spot, 1, seed + 71);
                float radius = Mathf.Lerp(0.025f, 0.070f, Hash01(spot, 2, seed + 127));
                for (int y = 0; y < size; y++)
                {
                    float v = y / (float)size;
                    for (int x = 0; x < size; x++)
                    {
                        float u = x / (float)size;
                        float distance = WrappedDistance01(u, v, centerU, centerV);
                        if (distance >= radius)
                        {
                            continue;
                        }

                        int index = y * size + x;
                        float amount = 1f - SmoothStep(radius * 0.35f, radius, distance);
                        b[index] = Mathf.Max(b[index], amount);
                        a[index] = Mathf.Min(a[index], 1f - amount * 0.42f);
                    }
                }
            }

            const float DiffuseA = 1.0f;
            const float DiffuseB = 0.48f;
            const float Feed = 0.037f;
            const float Kill = 0.061f;
            int iterations = Mathf.Clamp(settings.OrganicPatternIterations, 1, 160);
            for (int iteration = 0; iteration < iterations; iteration++)
            {
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        int index = y * size + x;
                        float currentA = a[index];
                        float currentB = b[index];
                        float reaction = currentA * currentB * currentB;
                        float nextValueA = currentA + DiffuseA * Laplacian(a, size, x, y) - reaction + Feed * (1f - currentA);
                        float nextValueB = currentB + DiffuseB * Laplacian(b, size, x, y) + reaction - (Kill + Feed) * currentB;
                        nextA[index] = Mathf.Clamp01(nextValueA);
                        nextB[index] = Mathf.Clamp01(nextValueB);
                    }
                }

                (a, nextA) = (nextA, a);
                (b, nextB) = (nextB, b);
            }

            float min = float.MaxValue;
            float max = float.MinValue;
            for (int i = 0; i < length; i++)
            {
                min = Mathf.Min(min, b[i]);
                max = Mathf.Max(max, b[i]);
            }

            float span = max - min;
            float[] values = new float[length];
            if (span <= 0.0001f)
            {
                for (int y = 0; y < size; y++)
                {
                    float v = y / (float)size;
                    for (int x = 0; x < size; x++)
                    {
                        float u = x / (float)size;
                        values[y * size + x] = TileableValueNoise(u * 8f, v * 8f, seed + 491, 8, 8);
                    }
                }
            }
            else
            {
                for (int i = 0; i < length; i++)
                {
                    values[i] = SmoothStep(0.08f, 0.92f, (b[i] - min) / span);
                }
            }

            return new GeneratedWorldOrganicPatternData(size, values);
        }

        private static float Laplacian(float[] values, int size, int x, int y)
        {
            int left = Mod(x - 1, size);
            int right = (x + 1) % size;
            int down = Mod(y - 1, size);
            int up = (y + 1) % size;
            float center = values[y * size + x] * -1f;
            float cardinal =
                values[y * size + left]
                + values[y * size + right]
                + values[down * size + x]
                + values[up * size + x];
            float diagonal =
                values[down * size + left]
                + values[down * size + right]
                + values[up * size + left]
                + values[up * size + right];
            return center + cardinal * 0.20f + diagonal * 0.05f;
        }

        private static float WrappedDistance01(float u, float v, float centerU, float centerV)
        {
            float du = Mathf.Abs(u - centerU);
            float dv = Mathf.Abs(v - centerV);
            du = Mathf.Min(du, 1f - du);
            dv = Mathf.Min(dv, 1f - dv);
            return Mathf.Sqrt(du * du + dv * dv);
        }

        private static float NormalStrength(GeneratedWorldSurfaceMaterialRole role)
        {
            return role switch
            {
                GeneratedWorldSurfaceMaterialRole.Organic => 2.2f,
                GeneratedWorldSurfaceMaterialRole.Wall => 1.35f,
                GeneratedWorldSurfaceMaterialRole.Ramp => 5.5f,
                GeneratedWorldSurfaceMaterialRole.Rim => 1.0f,
                _ => 2.25f
            };
        }

        private static int RoleSeed(int seed, int actNumber, GeneratedWorldSurfaceMaterialRole role)
        {
            unchecked
            {
                int value = seed;
                value = value * 397 ^ actNumber;
                value = value * 397 ^ (int)role * 1009;
                return value;
            }
        }

        private static float ValueNoise(float x, float y, int seed)
        {
            int x0 = Mathf.FloorToInt(x);
            int y0 = Mathf.FloorToInt(y);
            int x1 = x0 + 1;
            int y1 = y0 + 1;
            float tx = SmoothStep(0f, 1f, x - x0);
            float ty = SmoothStep(0f, 1f, y - y0);
            float a = Hash01(x0, y0, seed);
            float b = Hash01(x1, y0, seed);
            float c = Hash01(x0, y1, seed);
            float d = Hash01(x1, y1, seed);
            return Mathf.Lerp(Mathf.Lerp(a, b, tx), Mathf.Lerp(c, d, tx), ty);
        }

        private static float TileableValueNoise(float x, float y, int seed, int periodX, int periodY)
        {
            int x0 = Mathf.FloorToInt(x);
            int y0 = Mathf.FloorToInt(y);
            int x1 = x0 + 1;
            int y1 = y0 + 1;
            float tx = SmoothStep(0f, 1f, x - x0);
            float ty = SmoothStep(0f, 1f, y - y0);
            int px = Mathf.Max(1, periodX);
            int py = Mathf.Max(1, periodY);
            float a = Hash01(Mod(x0, px), Mod(y0, py), seed);
            float b = Hash01(Mod(x1, px), Mod(y0, py), seed);
            float c = Hash01(Mod(x0, px), Mod(y1, py), seed);
            float d = Hash01(Mod(x1, px), Mod(y1, py), seed);
            return Mathf.Lerp(Mathf.Lerp(a, b, tx), Mathf.Lerp(c, d, tx), ty);
        }

        private static int Mod(int value, int period)
        {
            int result = value % period;
            return result < 0 ? result + period : result;
        }

        private static float Hash01(int x, int y, int seed)
        {
            unchecked
            {
                uint h = (uint)seed;
                h ^= (uint)x * 374761393u;
                h = (h << 13) | (h >> 19);
                h ^= (uint)y * 668265263u;
                h *= 1274126177u;
                h ^= h >> 16;
                return (h & 0x00FFFFFF) / 16777215f;
            }
        }

        private static float SmoothStep(float edge0, float edge1, float value)
        {
            float t = Mathf.Clamp01((value - edge0) / Mathf.Max(0.0001f, edge1 - edge0));
            return t * t * (3f - 2f * t);
        }

        private static void SetTextureIfPresent(Material material, string property, Texture texture)
        {
            if (material != null && texture != null && material.HasProperty(property))
            {
                material.SetTexture(property, texture);
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

        private static void ApplyTextureScale(
            Material material,
            GeneratedWorldSurfaceTextureSettings settings,
            GeneratedWorldSurfaceMaterialRole role)
        {
            Vector2 scale = role switch
            {
                GeneratedWorldSurfaceMaterialRole.Organic => new Vector2(settings.OrganicTextureScale, settings.OrganicTextureScale),
                GeneratedWorldSurfaceMaterialRole.Ramp => new Vector2(0.68f, 1.00f),
                GeneratedWorldSurfaceMaterialRole.Wall => new Vector2(0.75f, 0.75f),
                _ => Vector2.one
            };
            SetTextureScaleIfPresent(material, "_BaseMap", scale);
            SetTextureScaleIfPresent(material, "_MainTex", scale);
            SetTextureScaleIfPresent(material, "_BumpMap", scale);
            SetTextureScaleIfPresent(material, "_ParallaxMap", scale);
            SetTextureScaleIfPresent(material, "_MetallicGlossMap", scale);
        }

        private static void SetTextureScaleIfPresent(Material material, string property, Vector2 scale)
        {
            if (material != null && material.HasProperty(property))
            {
                material.SetTextureScale(property, scale);
            }
        }
    }
}
