using NUnit.Framework;
using UnityEngine;
using TheCircussyOne.Rules;
using TheCircussyOne.Runtime;

public sealed class EnemyTerrainLocomotionRulesTests
{
    [Test]
    public void ShallowBlockedHeightIsWalkable()
    {
        EnemyTerrainLocomotionDecision decision = EnemyTerrainLocomotionRules.ClassifyBlockedSurface(
            hasTargetHeight: true,
            currentHeight: 0f,
            targetHeight: 0.2f,
            maxClimbHeight: 8f);

        Assert.That(decision, Is.EqualTo(EnemyTerrainLocomotionDecision.Walkable));
    }

    [Test]
    public void ReachableHighBlockedHeightHoldsForClimb()
    {
        EnemyTerrainLocomotionDecision decision = EnemyTerrainLocomotionRules.ClassifyBlockedSurface(
            hasTargetHeight: true,
            currentHeight: 0f,
            targetHeight: 2f,
            maxClimbHeight: 8f);

        Assert.That(decision, Is.EqualTo(EnemyTerrainLocomotionDecision.ClimbBlocked));
    }

    [Test]
    public void ReachableRelativeHeightHoldsForClimbEvenAtHighWorldY()
    {
        EnemyTerrainLocomotionDecision decision = EnemyTerrainLocomotionRules.ClassifyBlockedSurface(
            hasTargetHeight: true,
            currentHeight: 27f,
            targetHeight: 32f,
            maxClimbHeight: 8f);

        Assert.That(decision, Is.EqualTo(EnemyTerrainLocomotionDecision.ClimbBlocked));
        Assert.That(EnemyTerrainLocomotionRules.ClimbDelta(27f, 32f), Is.EqualTo(5f).Within(0.001f));
    }

    [Test]
    public void MissingTargetIsUnreachable()
    {
        Assert.That(
            EnemyTerrainLocomotionRules.ClassifyBlockedSurface(false, 0f, 0f, 8f),
            Is.EqualTo(EnemyTerrainLocomotionDecision.Unreachable));
    }

    [Test]
    public void TallGeneratedTerrainRemainsClimbable()
    {
        Assert.That(
            EnemyTerrainLocomotionRules.ClassifyBlockedSurface(true, 0f, 9f, 8f),
            Is.EqualTo(EnemyTerrainLocomotionDecision.ClimbBlocked));
        Assert.That(
            EnemyTerrainLocomotionRules.ClassifyBlockedSurface(true, 27f, 36f, 8f),
            Is.EqualTo(EnemyTerrainLocomotionDecision.ClimbBlocked));
    }

    [Test]
    public void ClimbHoldReleasesAtConfiguredHeight()
    {
        var state = new EnemyVerticalMotorState();
        state.BeginEnvironmentClimb(2f, EnemyTerrainLocomotionRules.ClimbReleaseTolerance);

        Assert.That(EnemyTerrainLocomotionRules.ShouldHoldHorizontalForClimb(state, 1f), Is.True);
        Assert.That(EnemyTerrainLocomotionRules.HasReachedClimbReleaseHeight(state, 1.93f), Is.True);
    }

    [Test]
    public void ZeroOrVerticalDirectionsAreInvalid()
    {
        Assert.That(EnemyTerrainLocomotionRules.IsValidHorizontalDirection(Vector3.zero), Is.False);
        Assert.That(EnemyTerrainLocomotionRules.IsValidHorizontalDirection(Vector3.up), Is.False);
        Assert.That(EnemyTerrainLocomotionRules.IsValidHorizontalDirection(Vector3.forward), Is.True);
    }
}
