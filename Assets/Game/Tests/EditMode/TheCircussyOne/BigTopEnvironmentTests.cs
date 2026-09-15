using System.Collections;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;
using TheCircussyOne.Runtime;
using TheCircussyOne.Visuals;
using UnityEngine;
using UnityEngine.Rendering;

public sealed class BigTopEnvironmentRulesTests
{
    [Test]
    public void SpecUsesRadiusPaddingAndTerrainClearance()
    {
        RunWorldGenerationConfig config = RunWorldGenerationConfig.CreateRuntimeDefault();
        config.bigTopRadiusPadding = 7f;
        config.bigTopGroundSkirtInset = 9f;
        config.bigTopCurtainHeight = 20f;
        config.bigTopDomeHeightPercent = 0.5f;
        config.bigTopMinimumDomeHeight = 15f;
        config.bigTopSegmentCount = 48;
        config.bigTopCanopyRingCount = 5;
        config.bigTopTextureResolution = 256;
        config.bigTopStripeCount = 10;

        BigTopEnvironmentSpec spec = BigTopEnvironmentRules.BuildSpec(40f, 32f, 123, config);

        Assert.That(spec.Radius, Is.EqualTo(47f).Within(0.0001f));
        Assert.That(spec.GroundSkirtRadius, Is.EqualTo(38f).Within(0.0001f));
        Assert.That(spec.CurtainHeight, Is.EqualTo(48f).Within(0.0001f));
        Assert.That(spec.DomeHeight, Is.EqualTo(23.5f).Within(0.0001f));
        Assert.That(spec.SegmentCount, Is.EqualTo(48));
        Assert.That(spec.CanopyRingCount, Is.EqualTo(5));
        Assert.That(spec.TextureResolution, Is.EqualTo(256));
        Assert.That(spec.StripeCount, Is.EqualTo(10));
        Assert.That(spec.Seed, Is.EqualTo(123));
        Assert.That(spec.VisualHash, Is.Not.EqualTo(0));

        Object.DestroyImmediate(config);
    }

    [Test]
    public void VisualSettingsContributeToSpecMatching()
    {
        RunWorldGenerationConfig config = RunWorldGenerationConfig.CreateRuntimeDefault();
        BigTopEnvironmentSpec original = BigTopEnvironmentRules.BuildSpec(40f, 32f, 123, config);

        config.bigTopGodRayCount += 1;
        BigTopEnvironmentSpec changed = BigTopEnvironmentRules.BuildSpec(40f, 32f, 123, config);

        Assert.That(BigTopEnvironmentRules.SpecMatches(original, changed), Is.False);

        Object.DestroyImmediate(config);
    }

    [Test]
    public void PerimeterFogSettingsContributeToSpecMatching()
    {
        RunWorldGenerationConfig config = RunWorldGenerationConfig.CreateRuntimeDefault();
        BigTopEnvironmentSpec original = BigTopEnvironmentRules.BuildSpec(40f, 32f, 123, config);

        config.bigTopPerimeterFogBottomOpacity += 0.01f;
        BigTopEnvironmentSpec changed = BigTopEnvironmentRules.BuildSpec(40f, 32f, 123, config);

        Assert.That(BigTopEnvironmentRules.SpecMatches(original, changed), Is.False);

        Object.DestroyImmediate(config);
    }

    [Test]
    public void GodRayPolishSettingsContributeToSpecMatching()
    {
        RunWorldGenerationConfig config = RunWorldGenerationConfig.CreateRuntimeDefault();
        BigTopEnvironmentSpec original = BigTopEnvironmentRules.BuildSpec(40f, 32f, 123, config);

        config.bigTopGodRayEdgeFadePower += 0.2f;
        config.bigTopGodRayTextureNoiseStrength += 0.05f;
        config.bigTopGodRayFixtureBeamCoverage += 0.2f;
        BigTopEnvironmentSpec changed = BigTopEnvironmentRules.BuildSpec(40f, 32f, 123, config);

        Assert.That(BigTopEnvironmentRules.SpecMatches(original, changed), Is.False);

        Object.DestroyImmediate(config);
    }

    [Test]
    public void ShowLightSettingsContributeToSpecMatching()
    {
        RunWorldGenerationConfig config = RunWorldGenerationConfig.CreateRuntimeDefault();
        BigTopEnvironmentSpec original = BigTopEnvironmentRules.BuildSpec(40f, 32f, 123, config);

        config.bigTopShowLightSweepDegrees += 1f;
        config.bigTopShowLightAccentStrength += 0.01f;
        BigTopEnvironmentSpec changed = BigTopEnvironmentRules.BuildSpec(40f, 32f, 123, config);

        Assert.That(BigTopEnvironmentRules.SpecMatches(original, changed), Is.False);

        Object.DestroyImmediate(config);
    }

    [Test]
    public void DressingSettingsContributeToSpecMatching()
    {
        RunWorldGenerationConfig config = RunWorldGenerationConfig.CreateRuntimeDefault();
        BigTopEnvironmentSpec original = BigTopEnvironmentRules.BuildSpec(40f, 32f, 123, config);

        config.bigTopCurtainValanceScallopCount += 1;
        BigTopEnvironmentSpec changed = BigTopEnvironmentRules.BuildSpec(40f, 32f, 123, config);

        Assert.That(BigTopEnvironmentRules.SpecMatches(original, changed), Is.False);

        Object.DestroyImmediate(config);
    }

    [Test]
    public void BuntingAndBannerSettingsContributeToSpecMatching()
    {
        RunWorldGenerationConfig config = RunWorldGenerationConfig.CreateRuntimeDefault();
        BigTopEnvironmentSpec original = BigTopEnvironmentRules.BuildSpec(40f, 32f, 123, config);

        config.bigTopBuntingFlagsPerRing += 1;
        config.bigTopCurtainBannerCount += 1;
        BigTopEnvironmentSpec changed = BigTopEnvironmentRules.BuildSpec(40f, 32f, 123, config);

        Assert.That(BigTopEnvironmentRules.SpecMatches(original, changed), Is.False);

        Object.DestroyImmediate(config);
    }
}

public sealed class BigTopEnvironmentViewTests
{
    private GameObject root;
    private RunWorldGenerationConfig config;

    [SetUp]
    public void SetUp()
    {
        TheCircussyOneTestObjects.DestroyAll();
        root = new GameObject("Big Top View Test");
        config = RunWorldGenerationConfig.CreateRuntimeDefault();
        config.bigTopTextureResolution = 128;
        config.bigTopSegmentCount = 32;
        config.bigTopCanopyRingCount = 4;
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(config);
        TheCircussyOneTestObjects.DestroyAll();
    }

    [Test]
    public void ConfigureCreatesShellTextureAndNoGameplayPhysics()
    {
        var spec = new BigTopEnvironmentSpec(24f, 19f, 12f, 18f, 32, 4, 128, 8, 77);
        BigTopEnvironmentView view = root.AddComponent<BigTopEnvironmentView>();

        view.Configure(spec, config);

        Assert.That(view.Radius, Is.EqualTo(24f).Within(0.0001f));
        Assert.That(view.TotalHeight, Is.EqualTo(30f).Within(0.0001f));
        Assert.That(view.RuntimeMesh, Is.Not.Null);
        Assert.That(view.RuntimeMesh.vertexCount, Is.EqualTo((4 + 3) * (32 + 1)));
        Assert.That(view.RuntimeMesh.triangles.Length, Is.EqualTo((4 + 2) * 32 * 6));
        Assert.That(view.RuntimeTexture, Is.Not.Null);
        Assert.That(view.RuntimeTexture.width, Is.EqualTo(128));
        Assert.That(view.RuntimeGodRayMesh, Is.Not.Null);
        Assert.That(view.RuntimeGodRayMaterial, Is.Not.Null);
        Assert.That(view.RuntimePerimeterFogGroundMesh, Is.Not.Null);
        Assert.That(view.RuntimePerimeterFogGroundMaterial, Is.Not.Null);
        Assert.That(view.RuntimePerimeterFogGroundTexture, Is.Not.Null);
        Assert.That(view.RuntimePerimeterFogCurtainMesh, Is.Not.Null);
        Assert.That(view.RuntimePerimeterFogCurtainMaterial, Is.Not.Null);
        Assert.That(view.RuntimePerimeterFogCurtainTexture, Is.Not.Null);
        Assert.That(view.RuntimeGodRayFixtureMesh, Is.Not.Null);
        Assert.That(view.RuntimeGodRayFixtureMaterial, Is.Not.Null);
        Assert.That(view.RuntimeGodRayFixtureLensMesh, Is.Not.Null);
        Assert.That(view.RuntimeGodRayFixtureLensMaterial, Is.Not.Null);
        Assert.That(view.RuntimeBulbMesh, Is.Not.Null);
        Assert.That(view.RuntimeBulbMaterial, Is.Not.Null);
        Assert.That(view.RuntimeDressingRopeMesh, Is.Not.Null);
        Assert.That(view.RuntimeDressingRopeMaterial, Is.Not.Null);
        Assert.That(view.RuntimeDressingPoleMesh, Is.Not.Null);
        Assert.That(view.RuntimeDressingPoleMaterial, Is.Not.Null);
        Assert.That(view.RuntimeDressingValanceMesh, Is.Not.Null);
        Assert.That(view.RuntimeDressingValanceMaterial, Is.Not.Null);
        Assert.That(view.RuntimeDressingValanceMaterial.mainTexture, Is.Not.Null);
        Assert.That(view.RuntimeBuntingStringMesh, Is.Not.Null);
        Assert.That(view.RuntimeBuntingStringMaterial, Is.Not.Null);
        Assert.That(view.RuntimeBuntingPrimaryFlagMesh, Is.Not.Null);
        Assert.That(view.RuntimeBuntingPrimaryFlagMaterial, Is.Not.Null);
        Assert.That(view.RuntimeBuntingSecondaryFlagMesh, Is.Not.Null);
        Assert.That(view.RuntimeBuntingSecondaryFlagMaterial, Is.Not.Null);
        Assert.That(view.RuntimeCurtainBannerPanelMesh, Is.Not.Null);
        Assert.That(view.RuntimeCurtainBannerPanelMaterial, Is.Not.Null);
        Assert.That(view.RuntimeCurtainBannerPanelMaterial.mainTexture, Is.Not.Null);
        Assert.That(view.RuntimeCurtainBannerTrimMesh, Is.Not.Null);
        Assert.That(view.RuntimeCurtainBannerTrimMaterial, Is.Not.Null);
        Assert.That(view.RuntimeCurtainBannerTrimMaterial.mainTexture, Is.Not.Null);
        Assert.That(view.GetComponentsInChildren<Collider>(includeInactive: true), Is.Empty);
        Assert.That(view.GetComponentsInChildren<Rigidbody>(includeInactive: true), Is.Empty);
        foreach (MeshRenderer renderer in view.GetComponentsInChildren<MeshRenderer>(includeInactive: true))
        {
            Assert.That(renderer.shadowCastingMode, Is.EqualTo(ShadowCastingMode.Off));
            Assert.That(renderer.receiveShadows, Is.False);
            Assert.That(renderer.lightProbeUsage, Is.EqualTo(LightProbeUsage.Off));
            Assert.That(renderer.reflectionProbeUsage, Is.EqualTo(ReflectionProbeUsage.Off));
        }
    }

    [UnityEngine.TestTools.UnityTest]
    public IEnumerator ConfigureAsyncCreatesRuntimeTexturesAndNoGameplayPhysics()
    {
        var spec = new BigTopEnvironmentSpec(24f, 19f, 12f, 18f, 32, 4, 128, 8, 77);
        BigTopEnvironmentView view = root.AddComponent<BigTopEnvironmentView>();

        yield return view.ConfigureAsync(spec, config).ToCoroutine();

        Assert.That(view.Radius, Is.EqualTo(24f).Within(0.0001f));
        Assert.That(view.TotalHeight, Is.EqualTo(30f).Within(0.0001f));
        Assert.That(view.RuntimeTexture, Is.Not.Null);
        Assert.That(view.RuntimeTexture.width, Is.EqualTo(128));
        Assert.That(view.RuntimePerimeterFogGroundMesh, Is.Not.Null);
        Assert.That(view.RuntimePerimeterFogGroundTexture, Is.Not.Null);
        Assert.That(view.RuntimePerimeterFogCurtainMesh, Is.Not.Null);
        Assert.That(view.RuntimePerimeterFogCurtainTexture, Is.Not.Null);
        Assert.That(view.RuntimeDressingValanceMaterial.mainTexture, Is.Not.Null);
        Assert.That(view.RuntimeCurtainBannerPanelMaterial.mainTexture, Is.Not.Null);
        Assert.That(view.RuntimeCurtainBannerTrimMaterial.mainTexture, Is.Not.Null);
        Assert.That(view.GetComponentsInChildren<Collider>(includeInactive: true), Is.Empty);
        Assert.That(view.GetComponentsInChildren<Rigidbody>(includeInactive: true), Is.Empty);
    }

    [Test]
    public void DisabledLightingOverlaysCreateNoRayOrBulbMesh()
    {
        config.bigTopGodRaysEnabled = false;
        config.bigTopBulbStringsEnabled = false;
        config.bigTopPerimeterFogEnabled = false;
        var spec = new BigTopEnvironmentSpec(24f, 19f, 12f, 18f, 32, 4, 128, 8, 77);
        BigTopEnvironmentView view = root.AddComponent<BigTopEnvironmentView>();

        view.Configure(spec, config);

        Assert.That(view.RuntimeGodRayMesh, Is.Null);
        Assert.That(view.RuntimeGodRayMaterial, Is.Null);
        Assert.That(view.RuntimePerimeterFogGroundMesh, Is.Null);
        Assert.That(view.RuntimePerimeterFogGroundMaterial, Is.Null);
        Assert.That(view.RuntimePerimeterFogCurtainMesh, Is.Null);
        Assert.That(view.RuntimePerimeterFogCurtainMaterial, Is.Null);
        Assert.That(view.RuntimeGodRayFixtureMesh, Is.Null);
        Assert.That(view.RuntimeGodRayFixtureMaterial, Is.Null);
        Assert.That(view.RuntimeGodRayFixtureLensMesh, Is.Null);
        Assert.That(view.RuntimeGodRayFixtureLensMaterial, Is.Null);
        Assert.That(view.RuntimeBulbMesh, Is.Null);
        Assert.That(view.RuntimeBulbMaterial, Is.Null);
    }

    [Test]
    public void DisabledDressingCreatesNoDressingMeshes()
    {
        config.bigTopDressingEnabled = false;
        var spec = new BigTopEnvironmentSpec(24f, 19f, 12f, 18f, 32, 4, 128, 8, 77);
        BigTopEnvironmentView view = root.AddComponent<BigTopEnvironmentView>();

        view.Configure(spec, config);

        Assert.That(view.RuntimeDressingRopeMesh, Is.Null);
        Assert.That(view.RuntimeDressingRopeMaterial, Is.Null);
        Assert.That(view.RuntimeDressingPoleMesh, Is.Null);
        Assert.That(view.RuntimeDressingPoleMaterial, Is.Null);
        Assert.That(view.RuntimeDressingValanceMesh, Is.Null);
        Assert.That(view.RuntimeDressingValanceMaterial, Is.Null);
        Assert.That(view.RuntimeBuntingStringMesh, Is.Null);
        Assert.That(view.RuntimeBuntingStringMaterial, Is.Null);
        Assert.That(view.RuntimeBuntingPrimaryFlagMesh, Is.Null);
        Assert.That(view.RuntimeBuntingPrimaryFlagMaterial, Is.Null);
        Assert.That(view.RuntimeBuntingSecondaryFlagMesh, Is.Null);
        Assert.That(view.RuntimeBuntingSecondaryFlagMaterial, Is.Null);
        Assert.That(view.RuntimeCurtainBannerPanelMesh, Is.Null);
        Assert.That(view.RuntimeCurtainBannerPanelMaterial, Is.Null);
        Assert.That(view.RuntimeCurtainBannerTrimMesh, Is.Null);
        Assert.That(view.RuntimeCurtainBannerTrimMaterial, Is.Null);
    }

    [Test]
    public void DisabledBuntingAndBannersCreateNoBuntingOrBannerMeshes()
    {
        config.bigTopBuntingEnabled = false;
        config.bigTopCurtainBannersEnabled = false;
        var spec = new BigTopEnvironmentSpec(24f, 19f, 12f, 18f, 32, 4, 128, 8, 77);
        BigTopEnvironmentView view = root.AddComponent<BigTopEnvironmentView>();

        view.Configure(spec, config);

        Assert.That(view.RuntimeBuntingStringMesh, Is.Null);
        Assert.That(view.RuntimeBuntingStringMaterial, Is.Null);
        Assert.That(view.RuntimeBuntingPrimaryFlagMesh, Is.Null);
        Assert.That(view.RuntimeBuntingPrimaryFlagMaterial, Is.Null);
        Assert.That(view.RuntimeBuntingSecondaryFlagMesh, Is.Null);
        Assert.That(view.RuntimeBuntingSecondaryFlagMaterial, Is.Null);
        Assert.That(view.RuntimeCurtainBannerPanelMesh, Is.Null);
        Assert.That(view.RuntimeCurtainBannerPanelMaterial, Is.Null);
        Assert.That(view.RuntimeCurtainBannerTrimMesh, Is.Null);
        Assert.That(view.RuntimeCurtainBannerTrimMaterial, Is.Null);
    }

    [Test]
    public void TickAnimatesShowLightMeshFromGameTimeWithoutReplacingMeshObject()
    {
        config.bigTopGodRayCount = 6;
        config.bigTopShowLightsEnabled = true;
        config.bigTopShowLightCount = 6;
        config.bigTopShowLightSweepDegrees = 24f;
        config.bigTopShowLightSpeedMinHz = 0.25f;
        config.bigTopShowLightSpeedMaxHz = 0.25f;
        config.bigTopShowLightMeshRefreshInterval = 0.02f;
        var spec = new BigTopEnvironmentSpec(24f, 19f, 12f, 18f, 32, 4, 128, 8, 77);
        BigTopEnvironmentView view = root.AddComponent<BigTopEnvironmentView>();

        view.Configure(spec, config);
        Mesh originalMesh = view.RuntimeGodRayMesh;
        Vector3[] before = originalMesh.vertices;
        view.Tick(1f, config);

        Assert.That(view.RuntimeGodRayMesh, Is.SameAs(originalMesh));
        Assert.That(VerticesChanged(before, view.RuntimeGodRayMesh.vertices), Is.True);
    }

    [Test]
    public void DressingMeshesStayInsideBigTopBounds()
    {
        var spec = new BigTopEnvironmentSpec(80f, 64f, 42f, 70f, 64, 5, 128, 12, 913);

        Mesh ropes = BigTopDressingMeshBuilder.BuildRopesAndHub(spec, config);
        Mesh poles = BigTopDressingMeshBuilder.BuildPoles(spec, config);
        Mesh valance = BigTopDressingMeshBuilder.BuildValance(spec, config);
        Mesh fixtures = BigTopDressingMeshBuilder.BuildGodRayFixtureBodies(spec, config);
        Mesh fixtureLenses = BigTopDressingMeshBuilder.BuildGodRayFixtureLenses(spec, config);
        Mesh buntingStrings = BigTopBuntingMeshBuilder.BuildBuntingStrings(spec, config);
        Mesh primaryFlags = BigTopBuntingMeshBuilder.BuildBuntingFlagsPrimary(spec, config);
        Mesh secondaryFlags = BigTopBuntingMeshBuilder.BuildBuntingFlagsSecondary(spec, config);
        Mesh bannerPanels = BigTopBuntingMeshBuilder.BuildCurtainBannerPanels(spec, config);
        Mesh bannerTrim = BigTopBuntingMeshBuilder.BuildCurtainBannerTrim(spec, config);

        AssertMeshBounds(ropes, spec);
        AssertMeshBounds(poles, spec);
        AssertMeshBounds(valance, spec);
        AssertMeshBounds(fixtures, spec);
        AssertMeshBounds(fixtureLenses, spec);
        AssertMeshBounds(buntingStrings, spec);
        AssertMeshBounds(primaryFlags, spec);
        AssertMeshBounds(secondaryFlags, spec);
        AssertMeshBounds(bannerPanels, spec);
        AssertMeshBounds(bannerTrim, spec);

        Object.DestroyImmediate(ropes);
        Object.DestroyImmediate(poles);
        Object.DestroyImmediate(valance);
        Object.DestroyImmediate(fixtures);
        Object.DestroyImmediate(fixtureLenses);
        Object.DestroyImmediate(buntingStrings);
        Object.DestroyImmediate(primaryFlags);
        Object.DestroyImmediate(secondaryFlags);
        Object.DestroyImmediate(bannerPanels);
        Object.DestroyImmediate(bannerTrim);
    }

    [Test]
    public void GeneratedTextureContainsReadableStripesAndLowerDirt()
    {
        var spec = new BigTopEnvironmentSpec(24f, 19f, 12f, 18f, 32, 4, 128, 8, 77);

        Texture2D texture = BigTopClothTextureGenerator.Generate(spec, config);
        Color redStripe = texture.GetPixel(8, 96);
        Color canvasStripe = texture.GetPixel(24, 96);
        Color lower = texture.GetPixel(8, 4);
        Color upper = texture.GetPixel(8, 120);

        Assert.That(ColorDistance(redStripe, canvasStripe), Is.GreaterThan(0.12f));
        Assert.That(Grayscale(lower), Is.LessThan(Grayscale(upper)));

        Object.DestroyImmediate(texture);
    }

    [UnityEngine.TestTools.UnityTest]
    public IEnumerator AsyncGeneratedTextureMatchesSyncPixels()
    {
        var spec = new BigTopEnvironmentSpec(24f, 19f, 12f, 18f, 32, 4, 128, 8, 77);
        Texture2D sync = BigTopClothTextureGenerator.Generate(spec, config);
        Texture2D asyncTexture = null;

        yield return BigTopClothTextureGenerator.GenerateAsync(spec, config, makeNoLongerReadable: false)
            .ToCoroutine(texture => asyncTexture = texture);

        Assert.That(asyncTexture, Is.Not.Null);
        Assert.That(asyncTexture.GetPixel(8, 96), Is.EqualTo(sync.GetPixel(8, 96)));
        Assert.That(asyncTexture.GetPixel(24, 96), Is.EqualTo(sync.GetPixel(24, 96)));
        Assert.That(asyncTexture.GetPixel(8, 4), Is.EqualTo(sync.GetPixel(8, 4)));
        Assert.That(asyncTexture.GetPixel(8, 120), Is.EqualTo(sync.GetPixel(8, 120)));

        Object.DestroyImmediate(sync);
        Object.DestroyImmediate(asyncTexture);
    }

    [Test]
    public void GeneratedDressingTextureAddsLowerDirtAndTrimVariation()
    {
        var spec = new BigTopEnvironmentSpec(24f, 19f, 12f, 18f, 32, 4, 128, 8, 77);

        Texture2D panel = BigTopDressingTextureGenerator.Generate(spec, config, "Panel Test Texture", config.bigTopCurtainBannerColor, 761, trim: false);
        Texture2D trim = BigTopDressingTextureGenerator.Generate(spec, config, "Trim Test Texture", config.bigTopCurtainBannerTrimColor, 997, trim: true);
        Color panelLower = panel.GetPixel(64, 4);
        Color panelUpper = panel.GetPixel(64, 120);
        Color trimCenter = trim.GetPixel(64, 64);
        Color trimEdge = trim.GetPixel(1, 64);

        Assert.That(Grayscale(panelLower), Is.LessThan(Grayscale(panelUpper)));
        Assert.That(ColorDistance(trimCenter, trimEdge), Is.GreaterThan(0.01f));

        Object.DestroyImmediate(panel);
        Object.DestroyImmediate(trim);
    }

    [UnityEngine.TestTools.UnityTest]
    public IEnumerator AsyncDressingTextureMatchesSyncPixels()
    {
        var spec = new BigTopEnvironmentSpec(24f, 19f, 12f, 18f, 32, 4, 128, 8, 77);
        Texture2D sync = BigTopDressingTextureGenerator.Generate(
            spec,
            config,
            "Panel Test Texture",
            config.bigTopCurtainBannerColor,
            761,
            trim: false);
        Texture2D asyncTexture = null;

        yield return BigTopDressingTextureGenerator.GenerateAsync(
                spec,
                config,
                "Panel Test Texture",
                config.bigTopCurtainBannerColor,
                761,
                trim: false,
                timingPrefix: "Test.Panel",
                makeNoLongerReadable: false)
            .ToCoroutine(texture => asyncTexture = texture);

        Assert.That(asyncTexture, Is.Not.Null);
        Assert.That(asyncTexture.GetPixel(64, 4), Is.EqualTo(sync.GetPixel(64, 4)));
        Assert.That(asyncTexture.GetPixel(64, 120), Is.EqualTo(sync.GetPixel(64, 120)));
        Assert.That(asyncTexture.GetPixel(1, 64), Is.EqualTo(sync.GetPixel(1, 64)));

        Object.DestroyImmediate(sync);
        Object.DestroyImmediate(asyncTexture);
    }

    private static float ColorDistance(Color a, Color b)
    {
        return Mathf.Abs(a.r - b.r) + Mathf.Abs(a.g - b.g) + Mathf.Abs(a.b - b.b);
    }

    private static float Grayscale(Color color)
    {
        return color.r * 0.299f + color.g * 0.587f + color.b * 0.114f;
    }

    private static bool VerticesChanged(Vector3[] before, Vector3[] after)
    {
        if (before.Length != after.Length)
        {
            return true;
        }

        for (int i = 0; i < before.Length; i++)
        {
            if (Vector3.Distance(before[i], after[i]) > 0.001f)
            {
                return true;
            }
        }

        return false;
    }

    private static void AssertMeshBounds(Mesh mesh, BigTopEnvironmentSpec spec)
    {
        Assert.That(mesh, Is.Not.Null);
        foreach (Vector3 vertex in mesh.vertices)
        {
            Assert.That(vertex.y, Is.InRange(-0.001f, spec.TotalHeight + 0.001f));
            Assert.That(new Vector2(vertex.x, vertex.z).magnitude, Is.LessThanOrEqualTo(spec.Radius + 0.001f));
        }
    }
}

public sealed class BigTopEnvironmentLightingMeshTests
{
    private RunWorldGenerationConfig config;

    [SetUp]
    public void SetUp()
    {
        config = RunWorldGenerationConfig.CreateRuntimeDefault();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(config);
    }

    [Test]
    public void GodRayMeshIsDeterministicAndStaysInsideBigTopBounds()
    {
        config.bigTopGodRayCount = 6;
        var spec = new BigTopEnvironmentSpec(80f, 64f, 42f, 70f, 64, 5, 128, 12, 913);

        Mesh first = BigTopGodRayMeshBuilder.Build(spec, config);
        Mesh second = BigTopGodRayMeshBuilder.Build(spec, config);

        Assert.That(first.vertexCount, Is.EqualTo(second.vertexCount));
        Assert.That(first.triangles.Length, Is.EqualTo(second.triangles.Length));
        for (int i = 0; i < first.vertexCount; i++)
        {
            Assert.That(first.vertices[i], Is.EqualTo(second.vertices[i]));
            Assert.That(first.vertices[i].y, Is.InRange(config.BigTopGodRayEndHeight - 0.001f, spec.TotalHeight + 0.001f));
            Assert.That(new Vector2(first.vertices[i].x, first.vertices[i].z).magnitude, Is.LessThanOrEqualTo(spec.Radius + 0.001f));
        }

        Object.DestroyImmediate(first);
        Object.DestroyImmediate(second);
    }

    [Test]
    public void GodRayMeshUsesSoftEightSidedFrustums()
    {
        config.bigTopGodRayCount = 3;
        var spec = new BigTopEnvironmentSpec(80f, 64f, 42f, 70f, 64, 5, 128, 12, 913);

        Mesh mesh = BigTopGodRayMeshBuilder.Build(spec, config);

        Assert.That(mesh.vertexCount, Is.EqualTo(config.BigTopGodRayCount * 8 * 4));
        Assert.That(mesh.triangles.Length, Is.EqualTo(config.BigTopGodRayCount * 8 * 6));

        Object.DestroyImmediate(mesh);
    }

    [Test]
    public void AnimatedShowLightSourcesSweepFromGameTimeWithoutMovingMount()
    {
        config.bigTopGodRayCount = 6;
        config.bigTopShowLightsEnabled = true;
        config.bigTopShowLightCount = 6;
        config.bigTopShowLightSweepDegrees = 24f;
        config.bigTopShowLightSpeedMinHz = 0.25f;
        config.bigTopShowLightSpeedMaxHz = 0.25f;
        var spec = new BigTopEnvironmentSpec(80f, 64f, 42f, 70f, 64, 5, 128, 12, 913);

        BigTopGodRaySource first = BigTopGodRayMeshBuilder.SourceAt(spec, config, 0, 6, 0f, animateShowLights: true);
        BigTopGodRaySource second = BigTopGodRayMeshBuilder.SourceAt(spec, config, 0, 6, 1f, animateShowLights: true);

        Assert.That(first.Mount, Is.EqualTo(second.Mount));
        Assert.That(Vector3.Distance(first.End, second.End), Is.GreaterThan(0.01f));
    }

    [Test]
    public void AnimatedGodRayMeshChangesWithGameTime()
    {
        config.bigTopGodRayCount = 6;
        config.bigTopShowLightsEnabled = true;
        config.bigTopShowLightCount = 6;
        config.bigTopShowLightSweepDegrees = 24f;
        config.bigTopShowLightSpeedMinHz = 0.25f;
        config.bigTopShowLightSpeedMaxHz = 0.25f;
        var spec = new BigTopEnvironmentSpec(80f, 64f, 42f, 70f, 64, 5, 128, 12, 913);

        Mesh first = BigTopGodRayMeshBuilder.Build(spec, config, 0f, animateShowLights: true);
        Mesh second = BigTopGodRayMeshBuilder.Build(spec, config, 1f, animateShowLights: true);

        Assert.That(MeshHasDifferentVertex(first, second), Is.True);

        Object.DestroyImmediate(first);
        Object.DestroyImmediate(second);
    }

    [Test]
    public void GodRayMeshCarriesPerShowLightTint()
    {
        config.bigTopGodRayCount = 6;
        config.bigTopShowLightsEnabled = true;
        config.bigTopShowLightCount = 6;
        config.bigTopShowLightColorVariationEnabled = true;
        config.bigTopShowLightAccentStrength = 0.2f;
        config.bigTopShowLightColorDriftStrength = 0f;
        var spec = new BigTopEnvironmentSpec(80f, 64f, 42f, 70f, 64, 5, 128, 12, 913);

        Mesh mesh = BigTopGodRayMeshBuilder.Build(spec, config, 0f, animateShowLights: true);

        Assert.That(mesh.colors, Has.Length.EqualTo(mesh.vertexCount));
        Assert.That(ColorDistance(mesh.colors[0], Color.white), Is.GreaterThan(0.001f));
        Assert.That(ColorDistance(mesh.colors[0], mesh.colors[8 * 4]), Is.GreaterThan(0.001f));

        Object.DestroyImmediate(mesh);
    }

    [Test]
    public void ShowLightTintDriftsDeterministicallyWithGameTime()
    {
        config.bigTopGodRayCount = 6;
        config.bigTopShowLightsEnabled = true;
        config.bigTopShowLightCount = 6;
        config.bigTopShowLightColorVariationEnabled = true;
        config.bigTopShowLightColorDriftStrength = 0.2f;
        config.bigTopShowLightColorDriftSpeedMinHz = 0.25f;
        config.bigTopShowLightColorDriftSpeedMaxHz = 0.25f;

        Color first = BigTopVisualColorRules.ShowLightTint(config, 913, 1, 0f);
        Color repeat = BigTopVisualColorRules.ShowLightTint(config, 913, 1, 0f);
        Color later = BigTopVisualColorRules.ShowLightTint(config, 913, 1, 1f);

        Assert.That(ColorDistance(first, repeat), Is.EqualTo(0f).Within(0.0001f));
        Assert.That(ColorDistance(first, later), Is.GreaterThan(0.001f));
    }

    [Test]
    public void DisabledShowLightColorVariationUsesWhiteMeshTint()
    {
        config.bigTopGodRayCount = 6;
        config.bigTopShowLightsEnabled = true;
        config.bigTopShowLightCount = 6;
        config.bigTopShowLightColorVariationEnabled = false;
        var spec = new BigTopEnvironmentSpec(80f, 64f, 42f, 70f, 64, 5, 128, 12, 913);

        Mesh mesh = BigTopGodRayMeshBuilder.Build(spec, config, 0f, animateShowLights: true);

        Assert.That(mesh.colors, Has.Length.EqualTo(mesh.vertexCount));
        Assert.That(ColorDistance(mesh.colors[0], Color.white), Is.EqualTo(0f).Within(0.0001f));

        Object.DestroyImmediate(mesh);
    }

    [Test]
    public void BulbMeshCreatesConfiguredRingAndRadialBulbs()
    {
        config.bigTopBulbRingCount = 2;
        config.bigTopBulbsPerRing = 5;
        config.bigTopBulbRadialStringCount = 3;
        config.bigTopBulbsPerRadialString = 4;
        var spec = new BigTopEnvironmentSpec(80f, 64f, 42f, 70f, 64, 5, 128, 12, 913);

        Mesh mesh = BigTopBulbMeshBuilder.Build(spec, config);

        int expectedBulbs = BigTopBulbMeshBuilder.BulbCount(config);
        Assert.That(expectedBulbs, Is.EqualTo(22));
        Assert.That(mesh.vertexCount, Is.EqualTo(expectedBulbs * 6));
        Assert.That(mesh.triangles.Length, Is.EqualTo(expectedBulbs * 8 * 3));

        Object.DestroyImmediate(mesh);
    }

    [Test]
    public void BuntingAndBannerMeshesUseConfiguredCounts()
    {
        config.bigTopBuntingRingCount = 2;
        config.bigTopBuntingFlagsPerRing = 6;
        config.bigTopCurtainBannerCount = 5;
        var spec = new BigTopEnvironmentSpec(80f, 64f, 42f, 70f, 64, 5, 128, 12, 913);

        Mesh primaryFlags = BigTopBuntingMeshBuilder.BuildBuntingFlagsPrimary(spec, config);
        Mesh secondaryFlags = BigTopBuntingMeshBuilder.BuildBuntingFlagsSecondary(spec, config);
        Mesh bannerPanels = BigTopBuntingMeshBuilder.BuildCurtainBannerPanels(spec, config);
        Mesh bannerTrim = BigTopBuntingMeshBuilder.BuildCurtainBannerTrim(spec, config);

        Assert.That(primaryFlags.vertexCount, Is.EqualTo(6 * 3));
        Assert.That(secondaryFlags.vertexCount, Is.EqualTo(6 * 3));
        Assert.That(bannerPanels.vertexCount, Is.EqualTo(5 * 4));
        Assert.That(bannerTrim.vertexCount, Is.EqualTo(5 * 4 * 4));
        Assert.That(bannerPanels.uv.Length, Is.EqualTo(bannerPanels.vertexCount));
        Assert.That(bannerTrim.uv.Length, Is.EqualTo(bannerTrim.vertexCount));

        Object.DestroyImmediate(primaryFlags);
        Object.DestroyImmediate(secondaryFlags);
        Object.DestroyImmediate(bannerPanels);
        Object.DestroyImmediate(bannerTrim);
    }

    [Test]
    public void PerimeterFogMeshesStayInsideBigTopAndFadeAwayFromPerimeter()
    {
        config.bigTopPerimeterFogSegmentCount = 24;
        config.bigTopPerimeterFogInnerRadiusPercent = 0.65f;
        config.bigTopPerimeterFogOuterInset = 6f;
        config.bigTopPerimeterFogHeight = 12f;
        var spec = new BigTopEnvironmentSpec(80f, 64f, 42f, 70f, 64, 5, 128, 12, 913);

        Mesh ground = BigTopPerimeterFogMeshBuilder.BuildGround(spec, config);
        Mesh curtain = BigTopPerimeterFogMeshBuilder.BuildCurtain(spec, config);

        Assert.That(ground, Is.Not.Null);
        Assert.That(curtain, Is.Not.Null);
        Assert.That(ground.vertexCount, Is.EqualTo((24 + 1) * 2));
        Assert.That(curtain.vertexCount, Is.EqualTo((24 + 1) * 2));
        Assert.That(ground.triangles.Length, Is.EqualTo(24 * 6));
        Assert.That(curtain.triangles.Length, Is.EqualTo(24 * 6));
        Assert.That(ground.colors[0].a, Is.LessThan(ground.colors[1].a));
        Assert.That(curtain.colors[1].a, Is.LessThan(curtain.colors[0].a));
        AssertMeshBounds(ground, spec);
        AssertMeshBounds(curtain, spec);
        Assert.That(MaxY(curtain), Is.EqualTo(12f).Within(0.001f));

        Object.DestroyImmediate(ground);
        Object.DestroyImmediate(curtain);
    }

    [Test]
    public void PerimeterFogTextureIsTransparentInwardAndStrongestAtPerimeter()
    {
        config.bigTopPerimeterFogBottomOpacity = 0.2f;
        config.bigTopPerimeterFogTopOpacity = 0.02f;
        config.bigTopPerimeterFogNoiseStrength = 0f;

        Texture2D ground = BigTopPerimeterFogTextureGenerator.Generate(config, BigTopPerimeterFogTextureMode.Ground);
        Texture2D curtain = BigTopPerimeterFogTextureGenerator.Generate(config, BigTopPerimeterFogTextureMode.Curtain);

        Assert.That(ground.GetPixel(ground.width / 2, 0).a, Is.LessThan(ground.GetPixel(ground.width / 2, ground.height - 1).a));
        Assert.That(curtain.GetPixel(curtain.width / 2, 0).a, Is.LessThan(curtain.GetPixel(curtain.width / 2, curtain.height - 1).a));

        Object.DestroyImmediate(ground);
        Object.DestroyImmediate(curtain);
    }

    [Test]
    public void CurtainBannersStayBelowValanceEvenWithSmallTopOffset()
    {
        config.bigTopCurtainBannerTopOffset = 0f;
        config.bigTopCurtainValanceHeight = 3.5f;
        config.bigTopCurtainValanceScallopDrop = 1.8f;
        var spec = new BigTopEnvironmentSpec(80f, 64f, 42f, 70f, 64, 5, 128, 12, 913);

        Mesh bannerPanels = BigTopBuntingMeshBuilder.BuildCurtainBannerPanels(spec, config);
        Mesh bannerTrim = BigTopBuntingMeshBuilder.BuildCurtainBannerTrim(spec, config);
        float maximumAllowedY = spec.CurtainHeight - config.BigTopCurtainValanceHeight - config.BigTopCurtainValanceScallopDrop - 1.25f + 0.001f;

        Assert.That(MaxY(bannerPanels), Is.LessThanOrEqualTo(maximumAllowedY));
        Assert.That(MaxY(bannerTrim), Is.LessThanOrEqualTo(maximumAllowedY));

        Object.DestroyImmediate(bannerPanels);
        Object.DestroyImmediate(bannerTrim);
    }

    [Test]
    public void CurtainBannerPanelIsInsetFromTrimFrame()
    {
        config.bigTopCurtainBannerCount = 1;
        config.bigTopCurtainBannerWidth = 8f;
        config.bigTopCurtainBannerHeight = 12f;
        var spec = new BigTopEnvironmentSpec(80f, 64f, 42f, 70f, 64, 5, 128, 12, 913);

        Mesh bannerPanels = BigTopBuntingMeshBuilder.BuildCurtainBannerPanels(spec, config);
        Mesh bannerTrim = BigTopBuntingMeshBuilder.BuildCurtainBannerTrim(spec, config);

        Assert.That(bannerPanels.bounds.size.z, Is.LessThan(bannerTrim.bounds.size.z));
        Assert.That(bannerPanels.bounds.size.y, Is.LessThan(bannerTrim.bounds.size.y));

        Object.DestroyImmediate(bannerPanels);
        Object.DestroyImmediate(bannerTrim);
    }

    [Test]
    public void CurtainBannersArePhasedBetweenPerimeterPoles()
    {
        config.bigTopCurtainBannerCount = 1;
        config.bigTopPerimeterPoleCount = 4;
        var spec = new BigTopEnvironmentSpec(80f, 64f, 42f, 70f, 64, 5, 128, 12, 913);

        Mesh bannerPanels = BigTopBuntingMeshBuilder.BuildCurtainBannerPanels(spec, config);

        Vector3 center = AverageVertexPosition(bannerPanels);
        float angle = Mathf.Atan2(center.z, center.x);

        Assert.That(angle, Is.EqualTo(Mathf.PI * 0.25f).Within(0.001f));

        Object.DestroyImmediate(bannerPanels);
    }

    [Test]
    public void GodRayFixtureBodiesIncludeCanopySupportRods()
    {
        config.bigTopGodRayCount = 1;
        var spec = new BigTopEnvironmentSpec(80f, 64f, 42f, 70f, 64, 5, 128, 12, 913);
        BigTopGodRaySource source = BigTopGodRayMeshBuilder.SourceAt(spec, config, 0, 1);

        Mesh fixtures = BigTopDressingMeshBuilder.BuildGodRayFixtureBodies(spec, config);
        Vector3 highest = HighestVertex(fixtures);

        Assert.That(MaxY(fixtures), Is.GreaterThan(spec.CurtainHeight + spec.DomeHeight * 0.5f));
        Assert.That(highest.x, Is.EqualTo(source.Mount.x).Within(config.BigTopGodRayFixtureRadius));
        Assert.That(highest.z, Is.EqualTo(source.Mount.z).Within(config.BigTopGodRayFixtureRadius));

        Object.DestroyImmediate(fixtures);
    }

    [Test]
    public void GodRayFixtureLensesUseRoundSegmentedMeshes()
    {
        config.bigTopGodRayCount = 1;
        var spec = new BigTopEnvironmentSpec(80f, 64f, 42f, 70f, 64, 5, 128, 12, 913);

        Mesh fixtureLenses = BigTopDressingMeshBuilder.BuildGodRayFixtureLenses(spec, config);

        Assert.That(fixtureLenses.vertexCount, Is.GreaterThan(48));
        Assert.That(fixtureLenses.triangles.Length, Is.GreaterThan(48));

        Object.DestroyImmediate(fixtureLenses);
    }

    [Test]
    public void GodRayFixtureLensesCarryMatchingShowLightTint()
    {
        config.bigTopGodRayCount = 2;
        config.bigTopShowLightsEnabled = true;
        config.bigTopShowLightCount = 2;
        config.bigTopShowLightColorVariationEnabled = true;
        config.bigTopShowLightAccentStrength = 0.2f;
        config.bigTopShowLightColorDriftStrength = 0f;
        var spec = new BigTopEnvironmentSpec(80f, 64f, 42f, 70f, 64, 5, 128, 12, 913);

        Mesh fixtureLenses = BigTopDressingMeshBuilder.BuildGodRayFixtureLenses(spec, config, 0f, animateShowLights: true);

        Assert.That(fixtureLenses.colors, Has.Length.EqualTo(fixtureLenses.vertexCount));
        Assert.That(ColorDistance(fixtureLenses.colors[0], BigTopVisualColorRules.ShowLightTint(config, spec.Seed, 0, 0f)), Is.EqualTo(0f).Within(0.0001f));

        Object.DestroyImmediate(fixtureLenses);
    }

    [Test]
    public void GodRayFixtureLensesKeepAuthoredSizeWhenRayIsWide()
    {
        config.bigTopGodRayCount = 1;
        config.bigTopGodRayTopWidthPercent = 0.08f;
        config.bigTopGodRayFixtureLensRadius = 1.15f;
        config.bigTopGodRayFixtureBeamCoverage = 1.5f;
        var spec = new BigTopEnvironmentSpec(80f, 64f, 42f, 70f, 64, 5, 128, 12, 913);
        BigTopGodRaySource source = BigTopGodRayMeshBuilder.SourceAt(spec, config, 0, 1);

        Mesh fixtureLenses = BigTopDressingMeshBuilder.BuildGodRayFixtureLenses(spec, config);

        Assert.That(MaxDistanceFrom(fixtureLenses, source.Start), Is.EqualTo(config.BigTopGodRayFixtureLensRadius).Within(0.001f));

        Object.DestroyImmediate(fixtureLenses);
    }

    [Test]
    public void GodRayFixtureLensesAreShallowCapsInsteadOfFullSpheres()
    {
        config.bigTopGodRayCount = 1;
        config.bigTopGodRayFixtureLensRadius = 1.15f;
        var spec = new BigTopEnvironmentSpec(80f, 64f, 42f, 70f, 64, 5, 128, 12, 913);
        BigTopGodRaySource source = BigTopGodRayMeshBuilder.SourceAt(spec, config, 0, 1);

        Mesh fixtureLenses = BigTopDressingMeshBuilder.BuildGodRayFixtureLenses(spec, config);

        Assert.That(MaxAbsProjectionFrom(fixtureLenses, source.Start, source.Direction), Is.LessThan(config.BigTopGodRayFixtureLensRadius * 0.2f));

        Object.DestroyImmediate(fixtureLenses);
    }

    [Test]
    public void GodRayTopWidthShrinksToFitAuthoredFixtureMouth()
    {
        config.bigTopGodRayCount = 1;
        config.bigTopGodRayTopWidthPercent = 0.08f;
        config.bigTopGodRayFixtureLensRadius = 1.15f;
        config.bigTopGodRayFixtureBeamCoverage = 1.5f;
        var spec = new BigTopEnvironmentSpec(80f, 64f, 42f, 70f, 64, 5, 128, 12, 913);

        float topWidth = BigTopGodRayMeshBuilder.ResolveTopWidth(spec, config);

        Assert.That(topWidth, Is.EqualTo(config.BigTopGodRayFixtureLensRadius * 2f * config.BigTopGodRayFixtureBeamCoverage).Within(0.001f));
        Assert.That(topWidth, Is.LessThan(spec.Radius * config.BigTopGodRayTopWidthPercent));
    }

    [Test]
    public void VisualColorRulesRespectOpacityAndFadeToZero()
    {
        config.bigTopGodRayOpacity = 0f;
        config.bigTopBulbEmissionStrength = 0f;
        config.bigTopGodRayFixtureLensEmissionStrength = 0f;

        Assert.That(BigTopVisualColorRules.GodRayColor(config, 12f).a, Is.EqualTo(0f));
        Assert.That(BigTopVisualColorRules.BulbColor(config, 12f).maxColorComponent, Is.EqualTo(0f));
        Assert.That(BigTopVisualColorRules.GodRayFixtureLensColor(config, 12f).maxColorComponent, Is.EqualTo(0f));
    }

    [Test]
    public void GodRayTextureFadesTowardLowerEndAndEdges()
    {
        Texture2D texture = BigTopGodRayTextureGenerator.Generate(config);

        float lowerAlpha = texture.GetPixel(texture.width / 2, 0).a;
        float upperAlpha = texture.GetPixel(texture.width / 2, texture.height - 1).a;
        float upperEdgeAlpha = texture.GetPixel(0, texture.height - 1).a;

        Assert.That(lowerAlpha, Is.LessThan(upperAlpha));
        Assert.That(upperEdgeAlpha, Is.LessThan(upperAlpha));

        Object.DestroyImmediate(texture);
    }

    private static float MaxY(Mesh mesh)
    {
        Assert.That(mesh, Is.Not.Null);
        float maxY = float.NegativeInfinity;
        foreach (Vector3 vertex in mesh.vertices)
        {
            maxY = Mathf.Max(maxY, vertex.y);
        }

        return maxY;
    }

    private static float MaxDistanceFrom(Mesh mesh, Vector3 center)
    {
        Assert.That(mesh, Is.Not.Null);
        float maxDistance = 0f;
        foreach (Vector3 vertex in mesh.vertices)
        {
            maxDistance = Mathf.Max(maxDistance, Vector3.Distance(vertex, center));
        }

        return maxDistance;
    }

    private static float MaxAbsProjectionFrom(Mesh mesh, Vector3 center, Vector3 axis)
    {
        Assert.That(mesh, Is.Not.Null);
        axis.Normalize();
        float maxDistance = 0f;
        foreach (Vector3 vertex in mesh.vertices)
        {
            maxDistance = Mathf.Max(maxDistance, Mathf.Abs(Vector3.Dot(vertex - center, axis)));
        }

        return maxDistance;
    }

    private static bool MeshHasDifferentVertex(Mesh first, Mesh second)
    {
        Assert.That(first, Is.Not.Null);
        Assert.That(second, Is.Not.Null);
        if (first.vertexCount != second.vertexCount)
        {
            return true;
        }

        Vector3[] firstVertices = first.vertices;
        Vector3[] secondVertices = second.vertices;
        for (int i = 0; i < firstVertices.Length; i++)
        {
            if (Vector3.Distance(firstVertices[i], secondVertices[i]) > 0.001f)
            {
                return true;
            }
        }

        return false;
    }

    private static float ColorDistance(Color first, Color second)
    {
        return Mathf.Abs(first.r - second.r)
            + Mathf.Abs(first.g - second.g)
            + Mathf.Abs(first.b - second.b)
            + Mathf.Abs(first.a - second.a);
    }

    private static void AssertMeshBounds(Mesh mesh, BigTopEnvironmentSpec spec)
    {
        Assert.That(mesh, Is.Not.Null);
        foreach (Vector3 vertex in mesh.vertices)
        {
            Assert.That(vertex.y, Is.InRange(-0.001f, spec.TotalHeight + 0.001f));
            Assert.That(new Vector2(vertex.x, vertex.z).magnitude, Is.LessThanOrEqualTo(spec.Radius + 0.001f));
        }
    }

    private static Vector3 AverageVertexPosition(Mesh mesh)
    {
        Assert.That(mesh, Is.Not.Null);
        Vector3 sum = Vector3.zero;
        Vector3[] vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            sum += vertices[i];
        }

        return vertices.Length > 0 ? sum / vertices.Length : Vector3.zero;
    }

    private static Vector3 HighestVertex(Mesh mesh)
    {
        Assert.That(mesh, Is.Not.Null);
        Vector3 highest = Vector3.zero;
        float maxY = float.NegativeInfinity;
        foreach (Vector3 vertex in mesh.vertices)
        {
            if (vertex.y > maxY)
            {
                maxY = vertex.y;
                highest = vertex;
            }
        }

        return highest;
    }
}

public sealed class BigTopEnvironmentSystemTests
{
    private RunWorldGenerationConfig config;
    private GameConfig gameConfig;
    private RunWorldGenerationState generationState;
    private BigTopEnvironmentSystem system;

    [SetUp]
    public void SetUp()
    {
        TheCircussyOneTestObjects.DestroyAll();
        config = RunWorldGenerationConfig.CreateRuntimeDefault();
        config.bigTopTextureResolution = 128;
        config.bigTopSegmentCount = 32;
        config.bigTopCanopyRingCount = 4;
        gameConfig = ScriptableObject.CreateInstance<GameConfig>();
        generationState = new RunWorldGenerationState();
        system = new BigTopEnvironmentSystem(config, gameConfig, generationState);
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
    public void GeneratedWorldCreatesViewUnderGeneratedRoot()
    {
        GameObject generatedRoot = new GameObject("TCO Test Generated Root");
        generationState.Set(GeneratedResult(generatedRoot, playableRadius: 40f, maxHeight: 20f));

        system.RefreshForCurrentWorld();

        Assert.That(system.HasView, Is.True);
        Assert.That(system.CurrentRadius, Is.EqualTo(64f).Within(0.0001f));
        Assert.That(system.View.transform.parent, Is.EqualTo(generatedRoot.transform));
    }

    [Test]
    public void TickDoesNotCreateViewBeforeLifecycleRefresh()
    {
        GameObject generatedRoot = new GameObject("TCO Test Generated Root");
        generationState.Set(GeneratedResult(generatedRoot, playableRadius: 40f, maxHeight: 20f));

        system.Tick();

        Assert.That(system.HasView, Is.False);
    }

    [Test]
    public void TickAnimatesBigTopLightingFromGameTime()
    {
        system.Clear();
        var time = new FakeGameTime { Time = 3.25f };
        system = new BigTopEnvironmentSystem(config, gameConfig, generationState, time: time);
        GameObject generatedRoot = new GameObject("TCO Test Generated Root");
        generationState.Set(GeneratedResult(generatedRoot, playableRadius: 40f, maxHeight: 20f));

        system.RefreshForCurrentWorld();
        system.Tick();

        Color expected = BigTopVisualColorRules.GodRayColor(config, time.Time);
        Assert.That(system.View.RuntimeGodRayMaterial.color.r, Is.EqualTo(expected.r).Within(0.001f));
        Assert.That(system.View.RuntimeGodRayMaterial.color.g, Is.EqualTo(expected.g).Within(0.001f));
        Assert.That(system.View.RuntimeGodRayMaterial.color.b, Is.EqualTo(expected.b).Within(0.001f));
        Assert.That(system.View.RuntimeGodRayMaterial.color.a, Is.EqualTo(expected.a).Within(0.001f));
    }

    [Test]
    public void ClearRemovesView()
    {
        GameObject generatedRoot = new GameObject("TCO Test Generated Root");
        generationState.Set(GeneratedResult(generatedRoot, playableRadius: 40f, maxHeight: 20f));
        system.RefreshForCurrentWorld();

        system.Clear();

        Assert.That(system.HasView, Is.False);
        Assert.That(system.View, Is.Null);
    }

    [Test]
    public void DisabledConfigCreatesNoView()
    {
        config.bigTopEnvironmentEnabled = false;
        GameObject generatedRoot = new GameObject("TCO Test Generated Root");
        generationState.Set(GeneratedResult(generatedRoot, playableRadius: 40f, maxHeight: 20f));

        system.RefreshForCurrentWorld();

        Assert.That(system.HasView, Is.False);
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
