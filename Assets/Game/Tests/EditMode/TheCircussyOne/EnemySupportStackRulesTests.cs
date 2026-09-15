using NUnit.Framework;
using UnityEngine;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;

public sealed class EnemySupportStackRulesTests
{
    [Test]
    public void SupportStackRequiresSupportPolicyAndClimbFlag()
    {
        var stack = new EnemyStackProfile
        {
            policy = EnemyStackPolicy.SupportBased,
            canClimbEnemies = true
        };

        Assert.That(EnemySupportStackRules.CanUseSupportStack(stack), Is.True);

        stack.canClimbEnemies = false;
        Assert.That(EnemySupportStackRules.CanUseSupportStack(stack), Is.False);

        stack.policy = EnemyStackPolicy.GroundOnly;
        stack.canClimbEnemies = true;
        Assert.That(EnemySupportStackRules.CanUseSupportStack(stack), Is.False);
    }

    [Test]
    public void FloatingLocomotionCannotUseOrProvideSupportStacking()
    {
        var stack = new EnemyStackProfile
        {
            policy = EnemyStackPolicy.SupportBased,
            canClimbEnemies = true,
            canBeStackedOn = true
        };
        var floating = new EnemyLocomotionProfile
        {
            mode = EnemyLocomotionMode.Floating
        };
        var grounded = new EnemyLocomotionProfile
        {
            mode = EnemyLocomotionMode.Grounded
        };

        Assert.That(EnemySupportStackRules.CanUseSupportStack(stack, grounded), Is.True);
        Assert.That(EnemySupportStackRules.CanUseSupportStack(stack, floating), Is.False);
        Assert.That(EnemySupportStackRules.CanBeSupport(stack, grounded), Is.True);
        Assert.That(EnemySupportStackRules.CanBeSupport(stack, floating), Is.False);
    }

    [Test]
    public void SupportReachUsesMovementBodyRadii()
    {
        Assert.That(EnemySupportStackRules.IsWithinSupportReach(Vector3.zero, 0.5f, new Vector3(0.9f, 3f, 0f), 0.5f), Is.True);
        Assert.That(EnemySupportStackRules.IsWithinSupportReach(Vector3.zero, 0.5f, new Vector3(1.2f, 0f, 0f), 0.5f), Is.False);
    }

    [Test]
    public void SeparationReducesOnlyForValidSupportRelationships()
    {
        var climber = new EnemyStackProfile
        {
            policy = EnemyStackPolicy.SupportBased,
            canClimbEnemies = true,
            canBeStackedOn = true
        };
        var support = new EnemyStackProfile
        {
            policy = EnemyStackPolicy.SupportBased,
            canClimbEnemies = true,
            canBeStackedOn = true
        };

        float candidate = EnemySupportStackRules.SeparationMultiplier(
            true,
            climber,
            firstSpawnId: 2,
            firstLayer: 0,
            firstRadius: 0.5f,
            firstPosition: Vector3.zero,
            support,
            secondSpawnId: 1,
            secondLayer: 0,
            secondRadius: 0.5f,
            secondPosition: new Vector3(0.5f, 0f, 0f));
        float stacked = EnemySupportStackRules.SeparationMultiplier(
            true,
            climber,
            firstSpawnId: 2,
            firstLayer: 1,
            firstRadius: 0.5f,
            firstPosition: Vector3.zero,
            support,
            secondSpawnId: 1,
            secondLayer: 0,
            secondRadius: 0.5f,
            secondPosition: new Vector3(0.5f, 0f, 0f));
        float distant = EnemySupportStackRules.SeparationMultiplier(
            true,
            climber,
            firstSpawnId: 2,
            firstLayer: 1,
            firstRadius: 0.5f,
            firstPosition: Vector3.zero,
            support,
            secondSpawnId: 1,
            secondLayer: 0,
            secondRadius: 0.5f,
            secondPosition: new Vector3(3f, 0f, 0f));

        Assert.That(candidate, Is.EqualTo(EnemySupportStackRules.SupportCandidateSeparationMultiplier));
        Assert.That(stacked, Is.EqualTo(EnemySupportStackRules.SupportedSeparationMultiplier));
        Assert.That(distant, Is.EqualTo(1f));
    }

    [Test]
    public void SeparationDampensFurtherOnlyForActiveSupportClimbers()
    {
        var climber = new EnemyStackProfile
        {
            policy = EnemyStackPolicy.SupportBased,
            canClimbEnemies = true,
            canBeStackedOn = true,
            supportClimbSeparationMultiplier = 0.1f
        };
        var support = new EnemyStackProfile
        {
            policy = EnemyStackPolicy.SupportBased,
            canClimbEnemies = true,
            canBeStackedOn = true
        };

        float climbing = EnemySupportStackRules.SeparationMultiplier(
            true,
            climber,
            2,
            0,
            0.5f,
            Vector3.zero,
            true,
            support,
            1,
            0,
            0.5f,
            new Vector3(0.5f, 0f, 0f),
            false);
        float grounded = EnemySupportStackRules.SeparationMultiplier(
            true,
            climber,
            2,
            0,
            0.5f,
            Vector3.zero,
            false,
            support,
            1,
            0,
            0.5f,
            new Vector3(0.5f, 0f, 0f),
            false);

        Assert.That(climbing, Is.EqualTo(0.1f));
        Assert.That(grounded, Is.EqualTo(EnemySupportStackRules.SupportCandidateSeparationMultiplier));
    }
}
