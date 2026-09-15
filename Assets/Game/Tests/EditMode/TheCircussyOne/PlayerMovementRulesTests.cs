using NUnit.Framework;
using UnityEngine;
using TheCircussyOne.Rules;

public sealed class PlayerMovementRulesTests
{
    [Test]
    public void AcceleratesTowardTargetSpeedWithoutSnapping()
    {
        var state = new PlayerMotorState();

        Tick(state, Vector2.up, deltaTime: 0.1f);

        Assert.That(state.Velocity.magnitude, Is.GreaterThan(0f));
        Assert.That(state.Velocity.magnitude, Is.LessThan(11.5f));
    }

    [Test]
    public void DeceleratesSmoothlyWhenInputIsReleased()
    {
        var state = new PlayerMotorState { Velocity = Vector3.forward * 8f };

        Tick(state, Vector2.zero, deltaTime: 0.1f);

        Assert.That(state.Velocity.magnitude, Is.EqualTo(5f).Within(0.001f));
    }

    [Test]
    public void CameraYawConvertsInputIntoWorldDirection()
    {
        Vector3 direction = PlayerMovementRules.CameraRelativeDirection(Vector2.up, 90f);

        Assert.That(direction.x, Is.EqualTo(1f).Within(0.001f));
        Assert.That(direction.z, Is.EqualTo(0f).Within(0.001f));
    }

    [Test]
    public void ReversingAtSpeedStartsSkidBehavior()
    {
        var state = new PlayerMotorState { Velocity = Vector3.forward * 9f, Facing = Vector3.forward };

        Tick(state, Vector2.down, deltaTime: 0.1f);

        Assert.That(state.IsSkidding, Is.True);
        Assert.That(state.Velocity.z, Is.LessThan(9f));
    }

    [Test]
    public void DeadzoneClampsSmallInputToNoMovement()
    {
        Vector2 input = PlayerMovementRules.NormalizeInput(new Vector2(0.05f, 0f), 0.12f);

        Assert.That(input, Is.EqualTo(Vector2.zero));
    }

    [Test]
    public void KeyboardDiagonalIsNormalized()
    {
        Vector2 input = PlayerMovementRules.NormalizeInput(new Vector2(1f, 1f), 0f);

        Assert.That(input.magnitude, Is.EqualTo(1f).Within(0.001f));
    }

    [Test]
    public void FacingSmoothingUsesConfigurableEase()
    {
        var defaultState = new PlayerMotorState { Velocity = Vector3.forward * 5f, Facing = Vector3.forward };
        var fasterState = new PlayerMotorState { Velocity = Vector3.forward * 5f, Facing = Vector3.forward };

        Tick(defaultState, Vector2.right, 0.016f, EaseSettings.Exponential);
        Tick(fasterState, Vector2.right, 0.016f, new EaseSettings(EasePreset.Exponential, 2f));

        Assert.That(fasterState.Facing.x, Is.GreaterThan(defaultState.Facing.x));
    }

    private static void Tick(PlayerMotorState state, Vector2 input, float deltaTime)
    {
        Tick(state, input, deltaTime, EaseSettings.Exponential);
    }

    private static void Tick(PlayerMotorState state, Vector2 input, float deltaTime, EaseSettings facingEase)
    {
        PlayerMovementRules.Tick(
            state,
            input,
            cameraYawDegrees: 0f,
            speedMultiplier: 1f,
            deadzone: 0.12f,
            walkSpeed: 3.5f,
            runSpeed: 11.5f,
            acceleration: 38f,
            deceleration: 30f,
            turnAcceleration: 24f,
            reverseSkidAngle: 125f,
            skidFriction: 42f,
            rotationSharpness: 16f,
            deltaTime: deltaTime,
            facingEase: facingEase);
    }
}
