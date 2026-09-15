using NUnit.Framework;
using UnityEngine;
using TheCircussyOne.Rules;

public sealed class PlayerJumpRulesTests
{
    [Test]
    public void GroundedJumpStartsWithHeightDerivedVelocityAndConsumesJump()
    {
        var state = new PlayerMotorState { IsGrounded = true };

        TickJump(state, jumpPressed: true, jumpHeld: true, isGrounded: true);

        Assert.That(state.JumpStartedThisFrame, Is.True);
        Assert.That(state.VerticalVelocity, Is.EqualTo(PlayerMovementRules.InitialJumpVelocity(3f, 30f)).Within(0.001f));
        Assert.That(state.JumpsUsed, Is.EqualTo(1));
        Assert.That(state.IsGrounded, Is.False);
    }

    [Test]
    public void CoyoteTimeAllowsLateJump()
    {
        var state = new PlayerMotorState { IsGrounded = false, CoyoteTimer = 0.05f };

        TickJump(state, jumpPressed: true, jumpHeld: true, isGrounded: false);

        Assert.That(state.JumpStartedThisFrame, Is.True);
    }

    [Test]
    public void BufferedJumpFiresAfterLanding()
    {
        var state = new PlayerMotorState
        {
            IsGrounded = false,
            VerticalVelocity = -5f,
            JumpBufferTimer = 0.08f
        };

        TickJump(state, jumpPressed: false, jumpHeld: false, isGrounded: false);
        PlayerMovementRules.ApplyMoveResult(state, isGroundedAfterMove: true);
        TickJump(state, jumpPressed: false, jumpHeld: true, isGrounded: true);

        Assert.That(state.JumpStartedThisFrame, Is.True);
    }

    [Test]
    public void ReleasingJumpEarlyAppliesStrongerLowJumpGravity()
    {
        var held = new PlayerMotorState { IsGrounded = true };
        var released = new PlayerMotorState { IsGrounded = true };
        TickJump(held, jumpPressed: true, jumpHeld: true, isGrounded: true);
        TickJump(released, jumpPressed: true, jumpHeld: true, isGrounded: true);

        TickJump(held, jumpPressed: false, jumpHeld: true, isGrounded: false);
        TickJump(released, jumpPressed: false, jumpHeld: false, isGrounded: false);

        Assert.That(released.VerticalVelocity, Is.LessThan(held.VerticalVelocity));
    }

    [Test]
    public void FallingUsesFallMultiplierAndClampsTerminalSpeed()
    {
        var state = new PlayerMotorState
        {
            IsGrounded = false,
            VerticalVelocity = -31f
        };

        TickJump(state, jumpPressed: false, jumpHeld: false, isGrounded: false, deltaTime: 0.5f);

        Assert.That(state.VerticalVelocity, Is.EqualTo(-32f).Within(0.001f));
    }

    [Test]
    public void LandingResetsJumpCountAndRaisesLandingEvent()
    {
        var state = new PlayerMotorState
        {
            IsGrounded = false,
            JumpsUsed = 1,
            VerticalVelocity = -8f
        };

        PlayerMovementRules.ApplyMoveResult(state, isGroundedAfterMove: true);

        Assert.That(state.LandedThisFrame, Is.True);
        Assert.That(state.JumpsUsed, Is.Zero);
        Assert.That(state.IsGrounded, Is.True);
    }

    [Test]
    public void ShortGroundingBlipDoesNotRaiseLandingEventWhenMinimumAirTimeIsRequired()
    {
        var state = new PlayerMotorState
        {
            IsGrounded = true,
            VerticalVelocity = -2f
        };

        TickJump(state, jumpPressed: false, jumpHeld: false, isGrounded: false, deltaTime: 0.016f);
        PlayerMovementRules.ApplyMoveResult(state, isGroundedAfterMove: true, minimumLandingAirTime: 0.08f);

        Assert.That(state.LandedThisFrame, Is.False);
        Assert.That(state.IsGrounded, Is.True);
        Assert.That(state.AirTime, Is.Zero);
    }

    [Test]
    public void RampDescentVisualGroundingDoesNotCountAsLandingPulse()
    {
        float visualAirTime = 0f;
        visualAirTime = PlayerMovementRules.NextVisualAirTime(visualAirTime, visuallyGrounded: true, deltaTime: 0.12f);

        Assert.That(PlayerMovementRules.ShouldPlayLandingPulse(
            landedThisFrame: true,
            visualAirTimeBeforeLanding: visualAirTime,
            minimumLandingAirTime: 0.08f), Is.False);
    }

    [Test]
    public void RealVisualAirborneTimeAllowsLandingPulse()
    {
        float visualAirTime = 0f;
        visualAirTime = PlayerMovementRules.NextVisualAirTime(visualAirTime, visuallyGrounded: false, deltaTime: 0.10f);

        Assert.That(PlayerMovementRules.ShouldPlayLandingPulse(
            landedThisFrame: true,
            visualAirTimeBeforeLanding: visualAirTime,
            minimumLandingAirTime: 0.08f), Is.True);
    }

    [Test]
    public void BriefDescendingVisualGroundLossStillSupportsGroundMotionVisuals()
    {
        Assert.That(PlayerMovementRules.IsGroundMotionVisualSupported(
            visuallyGrounded: false,
            visualAirTime: 0.03f,
            graceSeconds: 0.08f,
            verticalVelocity: -2f), Is.True);

        Assert.That(PlayerMovementRules.IsGroundMotionVisualSupported(
            visuallyGrounded: false,
            visualAirTime: 0.03f,
            graceSeconds: 0.08f,
            verticalVelocity: 4f), Is.False);

        Assert.That(PlayerMovementRules.IsGroundMotionVisualSupported(
            visuallyGrounded: false,
            visualAirTime: 0.12f,
            graceSeconds: 0.08f,
            verticalVelocity: -2f), Is.False);
    }

    [Test]
    public void BriefDescendingVisualGroundLossDoesNotPlayAirborneAnimation()
    {
        Assert.That(PlayerMovementRules.ShouldPlayAirborneAnimation(
            jumpStartedThisFrame: false,
            visuallyGrounded: false,
            visualAirTime: 0.03f,
            minimumAirTime: 0.14f,
            verticalVelocity: -2f), Is.False);
    }

    [Test]
    public void JumpOrRealFallPlaysAirborneAnimation()
    {
        Assert.That(PlayerMovementRules.ShouldPlayAirborneAnimation(
            jumpStartedThisFrame: true,
            visuallyGrounded: false,
            visualAirTime: 0f,
            minimumAirTime: 0.14f,
            verticalVelocity: 8f), Is.True);

        Assert.That(PlayerMovementRules.ShouldPlayAirborneAnimation(
            jumpStartedThisFrame: false,
            visuallyGrounded: false,
            visualAirTime: 0.16f,
            minimumAirTime: 0.14f,
            verticalVelocity: -6f), Is.True);
    }

    [Test]
    public void RealFallRaisesLandingEventAfterMinimumAirTime()
    {
        var state = new PlayerMotorState
        {
            IsGrounded = false,
            VerticalVelocity = -8f,
            AirTime = 0.12f
        };

        PlayerMovementRules.ApplyMoveResult(state, isGroundedAfterMove: true, minimumLandingAirTime: 0.08f);

        Assert.That(state.LandedThisFrame, Is.True);
        Assert.That(state.IsGrounded, Is.True);
    }

    [Test]
    public void MaxJumpCountBlocksOrAllowsAirJump()
    {
        var blocked = new PlayerMotorState { IsGrounded = false, JumpsUsed = 1 };
        var allowed = new PlayerMotorState { IsGrounded = false, JumpsUsed = 1 };

        TickJump(blocked, jumpPressed: true, jumpHeld: true, isGrounded: false, maxJumpCount: 1);
        TickJump(allowed, jumpPressed: true, jumpHeld: true, isGrounded: false, maxJumpCount: 2);

        Assert.That(blocked.JumpStartedThisFrame, Is.False);
        Assert.That(allowed.JumpStartedThisFrame, Is.True);
        Assert.That(allowed.JumpsUsed, Is.EqualTo(2));
    }

    private static void TickJump(
        PlayerMotorState state,
        bool jumpPressed,
        bool jumpHeld,
        bool isGrounded,
        int maxJumpCount = 1,
        float deltaTime = 0.016f)
    {
        PlayerMovementRules.Tick(
            state,
            rawInput: Vector2.zero,
            jumpPressedThisFrame: jumpPressed,
            jumpHeld: jumpHeld,
            isGrounded: isGrounded,
            cameraYawDegrees: 0f,
            speedMultiplier: 1f,
            deadzone: 0.12f,
            walkSpeed: 3.5f,
            runSpeed: 11.5f,
            acceleration: 38f,
            deceleration: 30f,
            turnAcceleration: 24f,
            airAcceleration: 18f,
            airDeceleration: 6f,
            airTurnAcceleration: 12f,
            reverseSkidAngle: 125f,
            skidFriction: 42f,
            rotationSharpness: 16f,
            maxJumpCount: maxJumpCount,
            jumpHeight: 3f,
            gravity: 30f,
            fallGravityMultiplier: 1.55f,
            lowJumpGravityMultiplier: 2.1f,
            terminalFallSpeed: 32f,
            coyoteSeconds: 0.08f,
            jumpBufferSeconds: 0.10f,
            deltaTime: deltaTime,
            facingEase: EaseSettings.Exponential);
    }
}
