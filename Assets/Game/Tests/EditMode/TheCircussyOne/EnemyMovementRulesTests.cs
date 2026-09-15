using NUnit.Framework;
using UnityEngine;
using TheCircussyOne.Rules;

public sealed class EnemyMovementRulesTests
{
    [Test]
    public void HorizontalOffsetIgnoresHeight()
    {
        Vector3 offset = EnemyMovementRules.HorizontalOffsetToTarget(new Vector3(1f, 5f, 2f), new Vector3(4f, -2f, 6f));

        Assert.That(offset, Is.EqualTo(new Vector3(3f, 0f, 4f)));
    }

    [Test]
    public void DirectionRequiresMinimumDistance()
    {
        bool canMove = EnemyMovementRules.TryGetDirection(new Vector3(0.01f, 0f, 0f), out Vector3 direction, out float distance);

        Assert.That(canMove, Is.False);
        Assert.That(direction, Is.EqualTo(Vector3.zero));
        Assert.That(distance, Is.LessThanOrEqualTo(EnemyMovementRules.MinimumMoveDistance));
    }

    [Test]
    public void SeparationPushesAwayInsideRadius()
    {
        Vector3 separation = EnemyMovementRules.SeparationAwayFrom(
            new Vector3(0.5f, 0f, 0f),
            Vector3.zero,
            radius: 1f,
            fallbackDirection: Vector3.right);

        Assert.That(separation.x, Is.GreaterThan(0f));
        Assert.That(separation.magnitude, Is.EqualTo(0.5f).Within(0.001f));
    }

    [Test]
    public void SeparationUsesFallbackForExactOverlap()
    {
        Vector3 separation = EnemyMovementRules.SeparationAwayFrom(
            Vector3.zero,
            Vector3.zero,
            radius: 1f,
            fallbackDirection: Vector3.forward);

        Assert.That(separation, Is.EqualTo(Vector3.forward));
    }

    [Test]
    public void ChaseStopsAtPlayerBodyDistance()
    {
        Assert.That(EnemyMovementRules.ShouldChase(1.5f, 1.25f), Is.True);
        Assert.That(EnemyMovementRules.ShouldChase(1.0f, 1.25f), Is.False);
    }

    [Test]
    public void MoveDistanceIsClampedBeforeEnteringPlayer()
    {
        float moveDistance = EnemyMovementRules.AllowedMoveDistance(
            currentDistance: 1.4f,
            stopDistance: 1.25f,
            desiredMoveDistance: 0.5f,
            towardTargetDot: 1f);

        Assert.That(moveDistance, Is.EqualTo(0.15f).Within(0.001f));
    }

    [Test]
    public void RotateTowardsHorizontalClampsTurnAngle()
    {
        Quaternion rotation = EnemyMovementRules.RotateTowardsHorizontal(
            Quaternion.identity,
            Vector3.right,
            degreesPerSecond: 90f,
            deltaTime: 0.5f);

        Assert.That(Quaternion.Angle(Quaternion.identity, rotation), Is.EqualTo(45f).Within(0.001f));
        Assert.That(Vector3.Dot(rotation * Vector3.forward, Vector3.right), Is.EqualTo(Mathf.Sqrt(0.5f)).Within(0.001f));
    }

    [Test]
    public void RotateTowardsHorizontalIgnoresInvalidDirection()
    {
        Quaternion current = Quaternion.Euler(0f, 35f, 0f);
        Quaternion rotation = EnemyMovementRules.RotateTowardsHorizontal(current, Vector3.zero, 540f, 1f);

        Assert.That(Quaternion.Angle(current, rotation), Is.EqualTo(0f).Within(0.001f));
    }

    [Test]
    public void HorizontalForwardUsesSafeFallback()
    {
        Vector3 forward = EnemyMovementRules.HorizontalForward(Quaternion.identity, Vector3.left);

        Assert.That(forward, Is.EqualTo(Vector3.forward));
    }

    [Test]
    public void MovementAlignmentMultiplierClampsDotToForwardOnly()
    {
        Assert.That(EnemyMovementRules.MovementAlignmentMultiplier(Vector3.forward, Vector3.forward), Is.EqualTo(1f).Within(0.001f));
        Assert.That(EnemyMovementRules.MovementAlignmentMultiplier(Vector3.forward, new Vector3(1f, 0f, 1f)), Is.EqualTo(Mathf.Sqrt(0.5f)).Within(0.001f));
        Assert.That(EnemyMovementRules.MovementAlignmentMultiplier(Vector3.forward, Vector3.back), Is.EqualTo(0f).Within(0.001f));
        Assert.That(EnemyMovementRules.MovementAlignmentMultiplier(Vector3.zero, Vector3.forward), Is.EqualTo(0f).Within(0.001f));
    }
}
