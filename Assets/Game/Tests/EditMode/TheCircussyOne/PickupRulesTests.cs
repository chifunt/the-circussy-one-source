using NUnit.Framework;
using UnityEngine;
using TheCircussyOne.Rules;

public sealed class PickupRulesTests
{
    [Test]
    public void CollectRadiusIncludesThreshold()
    {
        Assert.That(PickupRules.ShouldCollect(1.1f, 1.1f), Is.True);
        Assert.That(PickupRules.ShouldCollect(1.11f, 1.1f), Is.False);
    }

    [Test]
    public void MagnetRadiusIncludesOuterThresholdAndExcludesTinyDistances()
    {
        Assert.That(PickupRules.ShouldMagnet(5.5f, 5.5f), Is.True);
        Assert.That(PickupRules.ShouldMagnet(5.51f, 5.5f), Is.False);
        Assert.That(PickupRules.ShouldMagnet(PickupRules.MinimumMagnetDistance, 5.5f), Is.False);
    }

    [Test]
    public void HorizontalOffsetIgnoresHeight()
    {
        Vector3 offset = PickupRules.HorizontalOffsetToPlayer(new Vector3(1f, 10f, 2f), new Vector3(4f, -2f, 6f));

        Assert.That(offset, Is.EqualTo(new Vector3(3f, 0f, 4f)));
    }
}
