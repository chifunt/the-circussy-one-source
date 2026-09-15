using NUnit.Framework;
using UnityEngine;
using TheCircussyOne.Rules;

public sealed class EnemyHitboxAuthoringRulesTests
{
    [Test]
    public void CurrentDefaultsProducePositiveFrontMargin()
    {
        Vector3 contactSize = new(0.75f, 0.9f, 0.65f);
        Vector3 contactOffset = new(0f, 0.55f, 0.35f);
        Vector3 movementOffset = new(0f, 0.75f, 0f);

        float movementFront = EnemyHitboxAuthoringRules.MovementBodyFrontReach(movementOffset, 0.52f);
        float contactFront = EnemyHitboxAuthoringRules.ContactDamageFrontReach(contactOffset, contactSize);
        float margin = EnemyHitboxAuthoringRules.ContactFrontProtrusionMargin(contactOffset, contactSize, movementOffset, 0.52f);

        Assert.That(movementFront, Is.EqualTo(0.52f).Within(0.0001f));
        Assert.That(contactFront, Is.EqualTo(0.675f).Within(0.0001f));
        Assert.That(margin, Is.GreaterThan(EnemyHitboxAuthoringRules.MinimumContactFrontMargin));
        Assert.That(EnemyHitboxAuthoringRules.HasRequiredContactFrontMargin(contactOffset, contactSize, movementOffset, 0.52f), Is.True);
    }

    [Test]
    public void FullyBuriedContactBoxIsInvalid()
    {
        Vector3 contactSize = new(0.2f, 0.2f, 0.2f);
        Vector3 contactOffset = Vector3.zero;
        Vector3 movementOffset = Vector3.zero;

        float margin = EnemyHitboxAuthoringRules.ContactFrontProtrusionMargin(contactOffset, contactSize, movementOffset, 1f);

        Assert.That(margin, Is.LessThan(0f));
        Assert.That(EnemyHitboxAuthoringRules.HasRequiredContactFrontMargin(contactOffset, contactSize, movementOffset, 1f), Is.False);
    }

    [Test]
    public void FitContactFrontAdjustsOnlyZOffsetAndReachesRequiredMargin()
    {
        Vector3 contactSize = new(0.4f, 0.7f, 0.4f);
        Vector3 contactOffset = new(0.12f, 0.55f, -0.1f);
        Vector3 movementOffset = new(0f, 0.75f, 0f);

        Vector3 fitted = EnemyHitboxAuthoringRules.FitContactFrontToMovementBody(contactOffset, contactSize, movementOffset, 0.8f);
        float margin = EnemyHitboxAuthoringRules.ContactFrontProtrusionMargin(fitted, contactSize, movementOffset, 0.8f);

        Assert.That(fitted.x, Is.EqualTo(contactOffset.x).Within(0.0001f));
        Assert.That(fitted.y, Is.EqualTo(contactOffset.y).Within(0.0001f));
        Assert.That(fitted.z, Is.Not.EqualTo(contactOffset.z));
        Assert.That(margin, Is.EqualTo(EnemyHitboxAuthoringRules.MinimumContactFrontMargin).Within(0.0001f));
    }
}
