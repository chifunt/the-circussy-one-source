using NUnit.Framework;
using UnityEngine;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;

public sealed class ProjectileExplosionVisualRulesTests
{
    private VfxVisualConfig _config;

    [SetUp]
    public void SetUp()
    {
        _config = TheCircussyOneTestObjects.CreateVfxVisualConfig();
        _config.projectileExplosionOverlayDuration = 0.4f;
        _config.projectileExplosionOverlayStartRadiusFraction = 0.1f;
        _config.projectileExplosionOverlayRingColor = new Color(1f, 0.5f, 0f, 0.6f);
        _config.projectileExplosionOverlaySphereColor = new Color(1f, 0.25f, 0f, 0.12f);
    }

    [TearDown]
    public void TearDown()
    {
        TheCircussyOneTestObjects.Destroy(_config);
    }

    [Test]
    public void OverlayRadiusReachesSuppliedSplashRadiusWithoutOvershoot()
    {
        float splashRadius = 2.75f;

        ProjectileExplosionVisualFrame start = ProjectileExplosionVisualRules.Evaluate(0f, splashRadius, _config);
        ProjectileExplosionVisualFrame middle = ProjectileExplosionVisualRules.Evaluate(0.2f, splashRadius, _config);
        ProjectileExplosionVisualFrame nearEnd = ProjectileExplosionVisualRules.Evaluate(0.399f, splashRadius, _config);

        Assert.That(start.Radius, Is.EqualTo(0.275f).Within(0.0001f));
        Assert.That(middle.Radius, Is.LessThanOrEqualTo(splashRadius));
        Assert.That(nearEnd.Radius, Is.LessThanOrEqualTo(splashRadius));
        Assert.That(nearEnd.Radius, Is.EqualTo(splashRadius).Within(0.0001f));
    }

    [Test]
    public void OverlayAlphaFadesToZeroByDuration()
    {
        ProjectileExplosionVisualFrame active = ProjectileExplosionVisualRules.Evaluate(0.1f, 2f, _config);
        ProjectileExplosionVisualFrame done = ProjectileExplosionVisualRules.Evaluate(0.4f, 2f, _config);

        Assert.That(active.RingColor.a, Is.GreaterThan(0f));
        Assert.That(active.SphereColor.a, Is.GreaterThan(0f));
        Assert.That(done.Visible, Is.False);
        Assert.That(done.RingColor.a, Is.Zero);
        Assert.That(done.SphereColor.a, Is.Zero);
    }

    [Test]
    public void ZeroOrNegativeRadiusProducesNoVisibleOverlay()
    {
        Assert.That(ProjectileExplosionVisualRules.Evaluate(0f, 0f, _config).Visible, Is.False);
        Assert.That(ProjectileExplosionVisualRules.Evaluate(0f, -1f, _config).Visible, Is.False);
    }

    [Test]
    public void DisabledOverlayProducesNoVisibleOverlay()
    {
        _config.projectileExplosionOverlayEnabled = false;

        ProjectileExplosionVisualFrame frame = ProjectileExplosionVisualRules.Evaluate(0f, 2f, _config);

        Assert.That(frame.Visible, Is.False);
        Assert.That(frame.Radius, Is.Zero);
    }
}
