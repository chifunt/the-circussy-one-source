using NUnit.Framework;
using UnityEngine;
using TheCircussyOne.Rules;

public sealed class DamageNumberRulesTests
{
    [Test]
    public void NormalizedAgeClampsToLifetime()
    {
        Assert.That(DamageNumberRules.NormalizedAge(10f, 10.25f, 1f), Is.EqualTo(0.25f).Within(0.0001f));
        Assert.That(DamageNumberRules.NormalizedAge(10f, 12f, 1f), Is.EqualTo(1f));
        Assert.That(DamageNumberRules.NormalizedAge(10f, 9f, 1f), Is.EqualTo(0f));
    }

    [Test]
    public void SpawnPositionIsDeterministicAndBiasedUpward()
    {
        Vector3 hitPosition = new(2f, 0.8f, -1f);
        Vector3 first = DamageNumberRules.SpawnPosition(hitPosition, 0.55f, 0.18f, seed: 7);
        Vector3 second = DamageNumberRules.SpawnPosition(hitPosition, 0.55f, 0.18f, seed: 7);

        Assert.That(first, Is.EqualTo(second));
        Assert.That(first.y, Is.EqualTo(1.35f).Within(0.0001f));
        Assert.That(Vector2.Distance(new Vector2(hitPosition.x, hitPosition.z), new Vector2(first.x, first.z)), Is.LessThanOrEqualTo(0.18f));
    }

    [Test]
    public void LateralDriftDirectionIsDeterministicAndHorizontal()
    {
        Vector3 first = DamageNumberRules.LateralDriftDirection(12);
        Vector3 second = DamageNumberRules.LateralDriftDirection(12);

        Assert.That(first, Is.EqualTo(second));
        Assert.That(first.y, Is.EqualTo(0f).Within(0.0001f));
        Assert.That(first.magnitude, Is.EqualTo(1f).Within(0.0001f));
    }

    [Test]
    public void FrameFloatsUpShrinksAfterPopAndFades()
    {
        Vector3 start = new(0f, 1f, 0f);
        Color color = new(1f, 0.9f, 0.2f, 1f);

        DamageNumberFrame early = DamageNumberRules.Evaluate(start, 0.18f, 1.1f, 0.12f, 1f, 1.35f, 0.55f, 0.18f, 0.34f, color);
        DamageNumberFrame late = DamageNumberRules.Evaluate(start, 0.8f, 1.1f, 0.12f, 1f, 1.35f, 0.55f, 0.18f, 0.34f, color);
        DamageNumberFrame end = DamageNumberRules.Evaluate(start, 1f, 1.1f, 0.12f, 1f, 1.35f, 0.55f, 0.18f, 0.34f, color);

        Assert.That(late.Position.y, Is.GreaterThan(early.Position.y));
        Assert.That(late.Scale, Is.LessThan(early.Scale));
        Assert.That(late.Color.a, Is.LessThan(early.Color.a));
        Assert.That(end.Color.a, Is.EqualTo(0f).Within(0.0001f));
    }

    [Test]
    public void DeadzoneFreeAlphaStaysOpaqueBeforeFadeStart()
    {
        Assert.That(DamageNumberRules.Alpha(0.2f, 0.34f), Is.EqualTo(1f));
        Assert.That(DamageNumberRules.Alpha(0.9f, 0.34f), Is.LessThan(1f));
    }

    [Test]
    public void FrameUsesConfigurableEasing()
    {
        Vector3 start = new(0f, 1f, 0f);
        Color color = Color.white;

        DamageNumberFrame linear = DamageNumberRules.Evaluate(
            start,
            0.5f,
            1f,
            1f,
            1f,
            1.3f,
            0.5f,
            0.2f,
            0f,
            color,
            EaseSettings.Linear,
            EaseSettings.Linear,
            EaseSettings.Linear,
            EaseSettings.Linear);
        DamageNumberFrame shaped = DamageNumberRules.Evaluate(
            start,
            0.5f,
            1f,
            1f,
            1f,
            1.3f,
            0.5f,
            0.2f,
            0f,
            color,
            new EaseSettings(EasePreset.OutCubic, 2f),
            new EaseSettings(EasePreset.InCubic, 2f),
            new EaseSettings(EasePreset.InCubic, 2f),
            EaseSettings.OutBack);

        Assert.That(shaped.Position.y, Is.GreaterThan(linear.Position.y));
        Assert.That(shaped.Color.a, Is.GreaterThan(linear.Color.a));
    }

    [Test]
    public void FrameCanDriftLaterally()
    {
        DamageNumberFrame frame = DamageNumberRules.Evaluate(
            Vector3.zero,
            0.5f,
            1f,
            Vector3.right,
            0.4f,
            1f,
            1f,
            1.3f,
            0.5f,
            0.2f,
            0f,
            Color.white,
            EaseSettings.Linear,
            EaseSettings.Linear,
            EaseSettings.Linear,
            EaseSettings.Linear);

        Assert.That(frame.Position.x, Is.EqualTo(0.2f).Within(0.0001f));
        Assert.That(frame.Position.y, Is.EqualTo(0.5f).Within(0.0001f));
    }
}
