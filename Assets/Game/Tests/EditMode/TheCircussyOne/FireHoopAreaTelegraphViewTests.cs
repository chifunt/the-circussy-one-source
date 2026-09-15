using NUnit.Framework;
using UnityEngine;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;
using TheCircussyOne.Runtime;
using TheCircussyOne.Visuals;

public sealed class FireHoopAreaTelegraphViewTests
{
    private VfxVisualConfig _config;

    [SetUp]
    public void SetUp()
    {
        TheCircussyOneTestObjects.DestroyAll();
        _config = TheCircussyOneTestObjects.CreateVfxVisualConfig();
        _config.fireHoopAreaTelegraphSampleCount = 16;
        _config.fireHoopAreaTelegraphGroundClearance = 0.05f;
        _config.fireHoopAreaTelegraphGroundMask = ~0;
    }

    [TearDown]
    public void TearDown()
    {
        TheCircussyOneTestObjects.DestroyAll();
        TheCircussyOneTestObjects.Destroy(_config);
    }

    [Test]
    public void RulesUseExactGameplayRadiusAndPulseWithoutOvershooting()
    {
        _config.fireHoopAreaTelegraphRingWidth = 0.2f;
        _config.fireHoopAreaTelegraphPulseAlphaBoost = 0.25f;
        _config.fireHoopAreaTelegraphPulseRingWidthBoost = 0.5f;

        FireHoopAreaTelegraphFrame frame = FireHoopAreaTelegraphRules.Evaluate(0f, 2.95f, 0.45f, _config);

        Assert.That(frame.Visible, Is.True);
        Assert.That(frame.Radius, Is.EqualTo(2.95f).Within(0.0001f));
        Assert.That(frame.RingWidth, Is.LessThanOrEqualTo(frame.Radius));
        Assert.That(frame.RingColor.a, Is.GreaterThan(_config.fireHoopAreaTelegraphRingColor.a));
    }

    [Test]
    public void DisabledTelegraphReturnsInvisibleFrame()
    {
        _config.fireHoopAreaTelegraphEnabled = false;

        FireHoopAreaTelegraphFrame frame = FireHoopAreaTelegraphRules.Evaluate(0f, 3f, 0.45f, _config);

        Assert.That(frame.Visible, Is.False);
        Assert.That(frame.Radius, Is.Zero);
        Assert.That(frame.RingColor.a, Is.Zero);
    }

    [Test]
    public void ViewBuildsTerrainConformingFillAndRingMeshes()
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.name = "TCO Test Raised Fire Hoop Ground";
        ground.transform.position = new Vector3(0f, 1f, 0f);
        ground.transform.localScale = new Vector3(8f, 0.2f, 8f);
        FireHoopAreaTelegraphView view = TheCircussyOneTestObjects.CreateFireHoopAreaTelegraphPrefab(_config);
        FireHoopAreaTelegraphFrame frame = FireHoopAreaTelegraphRules.Evaluate(0f, 2f, 0.45f, _config);
        Physics.SyncTransforms();

        view.ApplyFrame(Vector3.zero, 0f, frame, GameLayers.EnvironmentMaskExcludingGameplay, forceMeshRefresh: true);

        Assert.That(view.IsActive, Is.True);
        Assert.That(view.Radius, Is.EqualTo(2f).Within(0.0001f));
        Assert.That(view.SampleCount, Is.EqualTo(16));
        Assert.That(view.FillVertexCount, Is.EqualTo(17));
        Assert.That(view.RingVertexCount, Is.EqualTo(32));
    }
}
