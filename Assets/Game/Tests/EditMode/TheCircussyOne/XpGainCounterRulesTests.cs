using NUnit.Framework;
using UnityEngine;
using TheCircussyOne.Rules;

public sealed class XpGainCounterRulesTests
{
    [Test]
    public void FirstPickupShowsAmountAndRepeatedPickupAggregates()
    {
        var state = new XpGainCounterState();

        Assert.That(state.Add(3, 0.1f, 1f, 0.5f), Is.True);
        Assert.That(state.Amount, Is.EqualTo(3));

        Assert.That(state.Add(4, 0.35f, 1f, 0.5f), Is.True);
        Assert.That(state.Amount, Is.EqualTo(7));
        Assert.That(state.LastGainTime, Is.EqualTo(0.35f));
    }

    [Test]
    public void ExpiredCounterResetsBeforeNextPickup()
    {
        var state = new XpGainCounterState();
        state.Add(3, 0f, 1f, 0.5f);

        Assert.That(XpGainCounterRules.IsExpired(state, 1.51f, 1f, 0.5f), Is.True);
        state.Add(2, 1.6f, 1f, 0.5f);

        Assert.That(state.Amount, Is.EqualTo(2));
    }

    [Test]
    public void AlphaHoldsThenFadesAfterConfiguredDelay()
    {
        Assert.That(XpGainCounterRules.Alpha(0.99f, 1f, 0.5f, EaseSettings.Linear), Is.EqualTo(1f));
        Assert.That(XpGainCounterRules.Alpha(1.25f, 1f, 0.5f, EaseSettings.Linear), Is.EqualTo(0.5f).Within(0.001f));
        Assert.That(XpGainCounterRules.Alpha(1.5f, 1f, 0.5f, EaseSettings.Linear), Is.EqualTo(0f).Within(0.001f));
    }

    [Test]
    public void PopScaleStartsLargeAndEasesToNormal()
    {
        float start = XpGainCounterRules.Scale(0f, 0.78f, 1f, 1.35f, 0.18f, EaseSettings.Linear);
        float mid = XpGainCounterRules.Scale(0.09f, 0.78f, 1f, 1.35f, 0.18f, EaseSettings.Linear);
        float end = XpGainCounterRules.Scale(0.18f, 0.78f, 1f, 1.35f, 0.18f, EaseSettings.Linear);

        Assert.That(start, Is.EqualTo(0.78f * 1.35f).Within(0.001f));
        Assert.That(mid, Is.LessThan(start));
        Assert.That(end, Is.EqualTo(0.78f).Within(0.001f));
    }

    [Test]
    public void PositionUsesCameraRightAndFlattenedForward()
    {
        Vector3 position = XpGainCounterRules.CameraRelativePosition(
            Vector3.zero,
            new Vector3(0f, -0.5f, 1f),
            new Vector3(1f, 0.25f, 0f),
            1.25f,
            1.5f,
            0.5f);

        Assert.That(position.x, Is.EqualTo(1.25f).Within(0.001f));
        Assert.That(position.y, Is.EqualTo(1.5f).Within(0.001f));
        Assert.That(position.z, Is.EqualTo(0.5f).Within(0.001f));
    }

    [Test]
    public void ZeroAndNegativeAmountsAreIgnored()
    {
        var state = new XpGainCounterState();

        Assert.That(state.Add(0, 0f, 1f, 0.5f), Is.False);
        Assert.That(state.Add(-1, 0f, 1f, 0.5f), Is.False);
        Assert.That(state.Amount, Is.Zero);
        Assert.That(state.IsVisible, Is.False);
    }
}
