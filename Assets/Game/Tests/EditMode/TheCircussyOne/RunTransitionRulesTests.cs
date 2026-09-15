using NUnit.Framework;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;
using UnityEngine;

public sealed class RunTransitionRulesTests
{
    [Test]
    public void DefaultTransitionFadeIsReadable()
    {
        HudTransitionSettings settings = HudTransitionSettings.Default;

        Assert.That(settings.fadeOutSeconds, Is.GreaterThanOrEqualTo(0.35f));
        Assert.That(settings.fadeInSeconds, Is.GreaterThanOrEqualTo(0.35f));
        Assert.That(settings.fadeEase.preset, Is.EqualTo(EasePreset.InOutSine));
        Assert.That(RunTransitionRules.OverlayOpacity(RunTransitionPhase.FadingOut, settings.fadeOutSeconds * 0.5f, settings), Is.EqualTo(0.5f).Within(0.08f));
        Assert.That(RunTransitionRules.OverlayOpacity(RunTransitionPhase.FadingIn, settings.fadeInSeconds * 0.5f, settings), Is.EqualTo(0.5f).Within(0.08f));
    }

    [Test]
    public void OverlayOpacityFadesInAndOutWithoutOvershoot()
    {
        HudTransitionSettings settings = HudTransitionSettings.Default;
        settings.fadeOutSeconds = 1f;
        settings.fadeInSeconds = 1f;
        settings.fadeEase = EaseSettings.Linear;

        Assert.That(RunTransitionRules.OverlayOpacity(RunTransitionPhase.Hidden, 0.5f, settings), Is.EqualTo(0f).Within(0.0001f));
        Assert.That(RunTransitionRules.OverlayOpacity(RunTransitionPhase.FadingOut, 0.5f, settings), Is.EqualTo(0.5f).Within(0.0001f));
        Assert.That(RunTransitionRules.OverlayOpacity(RunTransitionPhase.FadingOut, 2f, settings), Is.EqualTo(1f).Within(0.0001f));
        Assert.That(RunTransitionRules.OverlayOpacity(RunTransitionPhase.Holding, 0f, settings), Is.EqualTo(1f).Within(0.0001f));
        Assert.That(RunTransitionRules.OverlayOpacity(RunTransitionPhase.FadingIn, 0.5f, settings), Is.EqualTo(0.5f).Within(0.0001f));
        Assert.That(RunTransitionRules.OverlayOpacity(RunTransitionPhase.FadingIn, 2f, settings), Is.EqualTo(0f).Within(0.0001f));
    }

    [Test]
    public void MinimumVisibleUsesConfiguredHold()
    {
        HudTransitionSettings settings = HudTransitionSettings.Default;
        settings.minimumVisibleSeconds = 0.75f;

        Assert.That(RunTransitionRules.MinimumVisibleSatisfied(0.74f, settings), Is.False);
        Assert.That(RunTransitionRules.MinimumVisibleSatisfied(0.75f, settings), Is.True);
    }

    [Test]
    public void BackdropIsOpaqueBlack()
    {
        HudTransitionSettings settings = HudTransitionSettings.Default;
        settings.backgroundColor = new Color(0.5f, 0.1f, 0.1f, 0.25f);

        Assert.That(RunTransitionRules.BackdropColor(settings), Is.EqualTo(Color.black));
    }

    [Test]
    public void BulbBrightnessChasesAroundRing()
    {
        HudTransitionSettings settings = HudTransitionSettings.Default;
        settings.bulbCount = 8;
        settings.bulbChaseSpeed = 1f;

        Assert.That(RunTransitionRules.BulbBrightness(0, 0f, settings), Is.EqualTo(1f).Within(0.0001f));
        Assert.That(RunTransitionRules.BulbBrightness(1, 0f, settings), Is.EqualTo(0.62f).Within(0.0001f));
        Assert.That(RunTransitionRules.BulbBrightness(4, 0f, settings), Is.EqualTo(0f).Within(0.0001f));
        Assert.That(RunTransitionRules.BulbBrightness(1, 0.125f, settings), Is.EqualTo(1f).Within(0.0001f));
    }
}
