using NUnit.Framework;
using UnityEngine;
using TheCircussyOne.Config;
using TheCircussyOne.Visuals;

public sealed class PickupViewVfxTests
{
    [SetUp]
    public void SetUp()
    {
        TheCircussyOneTestObjects.DestroyAll();
    }

    [TearDown]
    public void TearDown()
    {
        TheCircussyOneTestObjects.DestroyAll();
    }

    [Test]
    public void AttractTrailStartsOnMagnetAndStopsOnDeactivate()
    {
        GameConfig gameConfig = TheCircussyOneTestObjects.CreateConfig();
        DamageFeedbackVisualConfig feedbackConfig = TheCircussyOneTestObjects.CreateDamageFeedbackConfig();
        VfxVisualConfig vfxConfig = TheCircussyOneTestObjects.CreateVfxVisualConfig();
        PickupView pickup = CreatePickupWithAttractTrail();
        Transform player = TheCircussyOneTestObjects.CreatePlayer(gameConfig).transform;

        pickup.Prepare(gameConfig, feedbackConfig, vfxConfig, Vector3.zero);
        pickup.BeginAttractTo(player, 0.1f);

        Assert.That(pickup.IsAttractTrailPlaying, Is.True);

        pickup.Deactivate();

        Assert.That(pickup.IsAttractTrailPlaying, Is.False);
    }

    [Test]
    public void PrepareAppliesGemEmissionToRendererPropertyBlock()
    {
        GameConfig gameConfig = TheCircussyOneTestObjects.CreateConfig();
        DamageFeedbackVisualConfig feedbackConfig = TheCircussyOneTestObjects.CreateDamageFeedbackConfig();
        PickupView pickup = TheCircussyOneTestObjects.CreatePickupPrefab();
        Color tint = new(0.27f, 0.96f, 1f, 1f);
        Color emission = new(0.45f, 1f, 1f, 1f);

        pickup.Prepare(gameConfig, feedbackConfig, null, Vector3.zero, tint, 1f, emission, 1.75f);

        Renderer renderer = pickup.GetComponentInChildren<Renderer>();
        var block = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(block);

        AssertColor(block.GetColor("_BaseColor"), tint);
        AssertColor(block.GetColor("_Color"), tint);
        AssertColor(block.GetColor("_EmissionColor"), emission);
        Assert.That(block.GetFloat("_EmissionStrength"), Is.EqualTo(1.75f).Within(0.001f));
        Assert.That(block.GetFloat("_EmissionSelfGlow"), Is.EqualTo(1.75f).Within(0.001f));
    }

    private static PickupView CreatePickupWithAttractTrail()
    {
        GameObject pickupObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pickupObject.name = "TCO Test Pickup With Trail";
        Object.DestroyImmediate(pickupObject.GetComponent<Collider>());

        var trailChild = new GameObject("XP Attract Trail");
        trailChild.transform.SetParent(pickupObject.transform, false);
        var trail = trailChild.AddComponent<ParticleSystem>();
        var main = trail.main;
        main.loop = true;
        main.playOnAwake = false;
        var emission = trail.emission;
        emission.enabled = true;
        emission.rateOverTime = 1f;

        pickupObject.SetActive(false);
        return pickupObject.AddComponent<PickupView>();
    }

    private static void AssertColor(Color actual, Color expected)
    {
        Assert.That(actual.r, Is.EqualTo(expected.r).Within(0.001f));
        Assert.That(actual.g, Is.EqualTo(expected.g).Within(0.001f));
        Assert.That(actual.b, Is.EqualTo(expected.b).Within(0.001f));
        Assert.That(actual.a, Is.EqualTo(expected.a).Within(0.001f));
    }
}
