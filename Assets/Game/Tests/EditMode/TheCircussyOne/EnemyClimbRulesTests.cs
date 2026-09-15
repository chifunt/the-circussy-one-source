using NUnit.Framework;
using TheCircussyOne.Rules;
using TheCircussyOne.Runtime;

public sealed class EnemyClimbRulesTests
{
    [Test]
    public void ClimbSpeedUsesMoveSpeedWhenConfigured()
    {
        float speed = EnemyClimbRules.ClimbSpeed(
            enemyMoveSpeed: 3f,
            useMoveSpeedForClimb: true,
            climbSpeedMultiplier: 1.5f,
            fallbackClimbSpeed: 4f);

        Assert.That(speed, Is.EqualTo(4.5f).Within(0.0001f));
    }

    [Test]
    public void ClimbSpeedUsesFallbackWhenMoveSpeedIsDisabled()
    {
        float speed = EnemyClimbRules.ClimbSpeed(
            enemyMoveSpeed: 3f,
            useMoveSpeedForClimb: false,
            climbSpeedMultiplier: 1.5f,
            fallbackClimbSpeed: 4f);

        Assert.That(speed, Is.EqualTo(4f).Within(0.0001f));
    }

    [Test]
    public void SupportClimbSpeedAppliesSupportMultiplier()
    {
        float speed = EnemyClimbRules.SupportClimbSpeed(8f, 0.5f);

        Assert.That(speed, Is.EqualTo(4f).Within(0.0001f));
    }

    [Test]
    public void ManualGravityFallsTowardTargetAndClampsTerminalSpeed()
    {
        var state = new EnemyVerticalMotorState { VerticalVelocity = -20f };

        float height = EnemyClimbRules.TickVerticalMotor(
            state,
            currentHeight: 5f,
            targetHeight: 0f,
            climbSpeed: 4f,
            gravity: 35f,
            terminalFallSpeed: 12f,
            deltaTime: 0.1f);

        Assert.That(height, Is.EqualTo(3.8f).Within(0.0001f));
        Assert.That(state.VerticalVelocity, Is.EqualTo(-12f).Within(0.0001f));
    }

    [Test]
    public void LandingClearsDownwardVelocity()
    {
        var state = new EnemyVerticalMotorState { VerticalVelocity = -12f };

        float height = EnemyClimbRules.TickVerticalMotor(
            state,
            currentHeight: 0.2f,
            targetHeight: 0f,
            climbSpeed: 4f,
            gravity: 35f,
            terminalFallSpeed: 28f,
            deltaTime: 0.5f);

        Assert.That(height, Is.EqualTo(0f).Within(0.0001f));
        Assert.That(state.VerticalVelocity, Is.EqualTo(0f).Within(0.0001f));
    }

    [Test]
    public void ProbeStaggeringUsesSpawnIdAndFrame()
    {
        Assert.That(EnemyClimbRules.ShouldProbeEnvironment(0, 3, 4, hasCachedHeight: false), Is.True);
        Assert.That(EnemyClimbRules.ShouldProbeEnvironment(1, 0, 4, hasCachedHeight: true), Is.False);
        Assert.That(EnemyClimbRules.ShouldProbeEnvironment(1, 3, 4, hasCachedHeight: true), Is.True);
    }
}
