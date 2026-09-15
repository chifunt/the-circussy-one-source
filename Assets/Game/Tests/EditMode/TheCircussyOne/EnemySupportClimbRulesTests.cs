using NUnit.Framework;
using UnityEngine;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using TheCircussyOne.Runtime;

public sealed class EnemySupportClimbRulesTests
{
    [Test]
    public void TargetRootHeightUsesSupportTopAndClimberBottom()
    {
        var support = new EnemyBodyProfile
        {
            movementBodyHeight = 1.45f,
            movementBodyRadius = 0.52f,
            movementBodyOffset = new Vector3(0f, 0.75f, 0f)
        };
        var climber = support.Copy();

        float target = EnemySupportClimbRules.TargetRootHeight(Vector3.zero, support, climber, 1f);

        Assert.That(target, Is.EqualTo(1.45f).Within(0.001f));
    }

    [Test]
    public void HeightMultiplierScalesNaturalSupportHeight()
    {
        var support = new EnemyBodyProfile
        {
            movementBodyHeight = 2f,
            movementBodyRadius = 0.5f,
            movementBodyOffset = new Vector3(0f, 1f, 0f)
        };
        var climber = support.Copy();

        float target = EnemySupportClimbRules.TargetRootHeight(Vector3.zero, support, climber, 0.5f);

        Assert.That(target, Is.EqualTo(1f).Within(0.001f));
    }

    [Test]
    public void TargetLayerClampsToMaxStackLayers()
    {
        Assert.That(EnemySupportClimbRules.TargetLayer(0, 4), Is.EqualTo(1));
        Assert.That(EnemySupportClimbRules.TargetLayer(3, 4), Is.EqualTo(4));
        Assert.That(EnemySupportClimbRules.TargetLayer(4, 4), Is.EqualTo(0));
    }

    [Test]
    public void MovementBlockRequiresSupportOnIntendedPath()
    {
        bool ahead = EnemySupportClimbRules.IsMovementBlockedBySupport(
            Vector3.zero,
            Vector3.forward,
            1f,
            0.5f,
            new Vector3(0f, 0f, 0.9f),
            0.5f,
            out float pathDistance);
        bool side = EnemySupportClimbRules.IsMovementBlockedBySupport(
            Vector3.zero,
            Vector3.forward,
            1f,
            0.5f,
            new Vector3(2f, 0f, 0.9f),
            0.5f,
            out _);

        Assert.That(ahead, Is.True);
        Assert.That(pathDistance, Is.GreaterThan(0f));
        Assert.That(side, Is.False);
    }

    [Test]
    public void SupportHoldReleasesAtTargetHeight()
    {
        var motor = new EnemyVerticalMotorState();
        motor.BeginSupportClimb(1.45f, EnemySupportClimbRules.SupportReleaseTolerance, 1, 12);

        Assert.That(EnemySupportClimbRules.ShouldHoldHorizontalForSupport(motor, 1f), Is.True);
        Assert.That(EnemySupportClimbRules.HasReachedSupportReleaseHeight(motor, 1.38f), Is.True);
    }
}
