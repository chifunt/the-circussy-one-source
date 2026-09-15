using NUnit.Framework;
using UnityEngine;
using TheCircussyOne.Rules;

public sealed class EnemySpawnIndicatorRulesTests
{
    [Test]
    public void NormalizedTimesClampToZeroOne()
    {
        Assert.That(EnemySpawnIndicatorRules.NormalizedWarning(10f, 9f, 0.85f), Is.EqualTo(0f));
        Assert.That(EnemySpawnIndicatorRules.NormalizedWarning(10f, 10.425f, 0.85f), Is.EqualTo(0.5f).Within(0.0001f));
        Assert.That(EnemySpawnIndicatorRules.NormalizedWarning(10f, 12f, 0.85f), Is.EqualTo(1f));

        Assert.That(EnemySpawnIndicatorRules.NormalizedEmerge(4f, 4.14f, 0.28f), Is.EqualTo(0.5f).Within(0.0001f));
        Assert.That(EnemySpawnIndicatorRules.NormalizedEmerge(4f, 5f, 0.28f), Is.EqualTo(1f));
    }

    [Test]
    public void InnerRadiusExpandsTowardOuterRadius()
    {
        float start = EnemySpawnIndicatorRules.InnerRadius(0f, 0.08f, 1.35f, EaseSettings.Linear);
        float middle = EnemySpawnIndicatorRules.InnerRadius(0.5f, 0.08f, 1.35f, EaseSettings.Linear);
        float end = EnemySpawnIndicatorRules.InnerRadius(1f, 0.08f, 1.35f, EaseSettings.Linear);

        Assert.That(start, Is.EqualTo(0.08f).Within(0.0001f));
        Assert.That(middle, Is.EqualTo(0.715f).Within(0.0001f));
        Assert.That(end, Is.EqualTo(1.35f).Within(0.0001f));
    }

    [Test]
    public void FrameFadesDuringEmergence()
    {
        Color outer = new(1f, 0.2f, 0.1f, 0.8f);
        EnemySpawnIndicatorFrame warning = EnemySpawnIndicatorRules.Evaluate(
            1f,
            0f,
            1.35f,
            0.08f,
            0.72f,
            outer,
            Color.white,
            Color.white,
            0f,
            1f,
            EaseSettings.Linear,
            EaseSettings.Linear);
        EnemySpawnIndicatorFrame faded = EnemySpawnIndicatorRules.Evaluate(
            1f,
            1f,
            1.35f,
            0.08f,
            0.72f,
            outer,
            Color.white,
            Color.white,
            0f,
            1f,
            EaseSettings.Linear,
            EaseSettings.Linear);

        Assert.That(warning.OuterColor.a, Is.EqualTo(0.8f).Within(0.0001f));
        Assert.That(faded.OuterColor.a, Is.EqualTo(0f).Within(0.0001f));
    }

    [Test]
    public void BodyLocalYUsesConfiguredEasing()
    {
        float linear = EnemySpawnIndicatorRules.BodyLocalY(0.5f, -0.65f, 1f, EaseSettings.Linear);
        float outBack = EnemySpawnIndicatorRules.BodyLocalY(0.5f, -0.65f, 1f, EaseSettings.OutBack);

        Assert.That(linear, Is.EqualTo(0.175f).Within(0.0001f));
        Assert.That(outBack, Is.GreaterThan(linear));
    }
}
