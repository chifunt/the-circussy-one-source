using NUnit.Framework;
using UnityEngine;
using TheCircussyOne.Rules;

public sealed class ActorMotionVisualRulesTests
{
    [Test]
    public void IdleScaleIsSubtleAndPeriodic()
    {
        var state = new ActorMotionVisualState();

        Vector3 scale = ActorMotionVisualRules.TickScale(
            state,
            targetSpeed01: 0f,
            deltaTime: 1.35f * 0.25f,
            idleAmplitude: 0.018f,
            idleSecondsPerCycle: 1.35f,
            idleXzCompensation: 0.5f,
            moveAmplitude: 0.05f,
            moveSecondsPerCycle: 0.42f,
            moveXzCompensation: 0.55f,
            smoothingSharpness: 16f,
            smoothingEase: EaseSettings.Exponential);

        Assert.That(scale.y, Is.GreaterThan(1f));
        Assert.That(scale.y, Is.LessThan(1.03f));
        Assert.That(scale.x, Is.LessThan(1f));
        Assert.That(scale.z, Is.EqualTo(scale.x).Within(0.0001f));
    }

    [Test]
    public void MovingScaleIsFasterAndStrongerThanIdle()
    {
        var idle = new ActorMotionVisualState();
        var moving = new ActorMotionVisualState();

        Vector3 idleScale = ActorMotionVisualRules.TickScale(
            idle,
            0f,
            0.105f,
            0.018f,
            1.35f,
            0.5f,
            0.05f,
            0.42f,
            0.55f,
            999f,
            EaseSettings.Exponential);

        Vector3 moveScale = ActorMotionVisualRules.TickScale(
            moving,
            1f,
            0.105f,
            0.018f,
            1.35f,
            0.5f,
            0.05f,
            0.42f,
            0.55f,
            999f,
            EaseSettings.Exponential);

        Assert.That(Mathf.Abs(moveScale.y - 1f), Is.GreaterThan(Mathf.Abs(idleScale.y - 1f)));
        Assert.That(moving.Phase01, Is.GreaterThan(idle.Phase01));
    }

    [Test]
    public void BodyScaleCounterbalancesVerticalStretch()
    {
        Vector3 scale = ActorMotionVisualRules.BodyScale(0.25f, 0.06f, 0.5f, 1f);

        Assert.That(scale.y, Is.GreaterThan(1f));
        Assert.That(scale.x, Is.LessThan(1f));
        Assert.That(scale.z, Is.EqualTo(scale.x).Within(0.0001f));
    }

    [Test]
    public void SpeedInputClampsAndSmooths()
    {
        Assert.That(ActorMotionVisualRules.NormalizeSpeed(20f, 10f), Is.EqualTo(1f));
        Assert.That(ActorMotionVisualRules.NormalizeSpeed(2.5f, 10f), Is.EqualTo(0.25f).Within(0.0001f));

        var state = new ActorMotionVisualState();
        ActorMotionVisualRules.TickScale(
            state,
            1f,
            0.016f,
            0f,
            1f,
            0.5f,
            0.05f,
            0.5f,
            0.5f,
            1f,
            EaseSettings.Exponential);

        Assert.That(state.SmoothedSpeed01, Is.GreaterThan(0f));
        Assert.That(state.SmoothedSpeed01, Is.LessThan(1f));
    }

    [Test]
    public void MovingCycleRespondsImmediatelyToTargetBeforeAmplitudeCatchesUp()
    {
        var state = new ActorMotionVisualState();

        ActorMotionVisualRules.TickScale(
            state,
            1f,
            0.105f,
            idleAmplitude: 0.018f,
            idleSecondsPerCycle: 1.35f,
            idleXzCompensation: 0.5f,
            moveAmplitude: 0.05f,
            moveSecondsPerCycle: 0.42f,
            moveXzCompensation: 0.55f,
            smoothingSharpness: 0.1f,
            smoothingEase: EaseSettings.Exponential);

        Assert.That(state.SmoothedSpeed01, Is.LessThan(0.02f));
        Assert.That(state.Phase01, Is.GreaterThan(0.20f));
    }

    [Test]
    public void PulseComposesWithScaleAndExpires()
    {
        var state = new ActorMotionVisualState();
        ActorMotionVisualRules.StartPulse(state, 1f, 1.2f, EaseSettings.OutQuad);

        Vector3 pulsed = ActorMotionVisualRules.TickScale(
            state,
            0f,
            0.25f,
            0f,
            1f,
            0.5f,
            0f,
            1f,
            0.5f,
            999f,
            EaseSettings.Exponential);

        Assert.That(pulsed.x, Is.GreaterThan(1f));
        Assert.That(pulsed.y, Is.GreaterThan(1f));

        Vector3 expired = ActorMotionVisualRules.TickScale(
            state,
            0f,
            0.8f,
            0f,
            1f,
            0.5f,
            0f,
            1f,
            0.5f,
            999f,
            EaseSettings.Exponential);

        Assert.That(expired, Is.EqualTo(Vector3.one));
    }

    [Test]
    public void DirectionalTakeoffPulseNarrowsAndStretches()
    {
        var state = new ActorMotionVisualState();
        ActorMotionVisualRules.StartPulse(state, 1f, 0.84f, 1.18f, EaseSettings.Linear, reboundStrength: 0f);

        Vector3 scale = ActorMotionVisualRules.TickScale(
            state,
            0f,
            0.25f,
            0f,
            1f,
            0.5f,
            0f,
            1f,
            0.5f,
            999f,
            EaseSettings.Exponential);

        Assert.That(scale.x, Is.LessThan(1f));
        Assert.That(scale.z, Is.EqualTo(scale.x).Within(0.0001f));
        Assert.That(scale.y, Is.GreaterThan(1f));
    }

    [Test]
    public void DirectionalTakeoffPulseSuppressesOpposingCycleScale()
    {
        var state = new ActorMotionVisualState
        {
            Phase01 = 0.75f,
            SmoothedSpeed01 = 1f
        };
        ActorMotionVisualRules.StartPulse(state, 1f, 0.84f, 1.18f, EaseSettings.Linear, reboundStrength: 0f);

        Vector3 scale = ActorMotionVisualRules.TickScale(
            state,
            1f,
            0.25f,
            0.12f,
            999f,
            0.5f,
            0.12f,
            999f,
            0.5f,
            999f,
            EaseSettings.Exponential);

        Assert.That(scale.x, Is.LessThan(0.96f));
        Assert.That(scale.z, Is.EqualTo(scale.x).Within(0.0001f));
        Assert.That(scale.y, Is.GreaterThan(1.05f));
    }

    [Test]
    public void DirectionalLandingPulseSquashesThenRebounds()
    {
        var state = new ActorMotionVisualState();
        ActorMotionVisualRules.StartPulse(state, 1f, 1.18f, 0.82f, EaseSettings.Linear, reboundStrength: 0.25f);

        Vector3 squash = ActorMotionVisualRules.TickScale(
            state,
            0f,
            0.25f,
            0f,
            1f,
            0.5f,
            0f,
            1f,
            0.5f,
            999f,
            EaseSettings.Exponential);

        Assert.That(squash.x, Is.GreaterThan(1f));
        Assert.That(squash.y, Is.LessThan(1f));

        Vector3 rebound = ActorMotionVisualRules.TickScale(
            state,
            0f,
            0.5f,
            0f,
            1f,
            0.5f,
            0f,
            1f,
            0.5f,
            999f,
            EaseSettings.Exponential);

        Assert.That(rebound.x, Is.LessThan(1f));
        Assert.That(rebound.y, Is.GreaterThan(1f));
    }

    [Test]
    public void DirectionalLandingPulseSuppressesOpposingCycleScale()
    {
        var state = new ActorMotionVisualState
        {
            Phase01 = 0.25f,
            SmoothedSpeed01 = 1f
        };
        ActorMotionVisualRules.StartPulse(state, 1f, 1.18f, 0.82f, EaseSettings.Linear, reboundStrength: 0f);

        Vector3 scale = ActorMotionVisualRules.TickScale(
            state,
            1f,
            0.25f,
            0.12f,
            999f,
            0.5f,
            0.12f,
            999f,
            0.5f,
            999f,
            EaseSettings.Exponential);

        Assert.That(scale.x, Is.GreaterThan(1.05f));
        Assert.That(scale.z, Is.EqualTo(scale.x).Within(0.0001f));
        Assert.That(scale.y, Is.LessThan(0.95f));
    }

    [Test]
    public void JumpHoldStretchReachesConfiguredScale()
    {
        var state = new ActorMotionVisualState();

        ActorMotionVisualRules.TickJumpHoldStretch(
            state,
            jumpHeld: true,
            rising: true,
            airborne: true,
            enabled: true,
            maxSeconds: 0.2f,
            releaseSeconds: 0.1f,
            deltaTime: 0.2f);
        Vector3 scale = ActorMotionVisualRules.JumpHoldStretchScale(state, 0.92f, 1.1f, EaseSettings.Linear);

        Assert.That(scale.x, Is.EqualTo(0.92f).Within(0.0001f));
        Assert.That(scale.z, Is.EqualTo(scale.x).Within(0.0001f));
        Assert.That(scale.y, Is.EqualTo(1.1f).Within(0.0001f));
    }

    [Test]
    public void JumpHoldStretchFadesOutOnRelease()
    {
        var state = new ActorMotionVisualState();
        ActorMotionVisualRules.TickJumpHoldStretch(state, true, true, true, true, 0.2f, 0.1f, 0.2f);

        ActorMotionVisualRules.TickJumpHoldStretch(
            state,
            jumpHeld: false,
            rising: true,
            airborne: true,
            enabled: true,
            maxSeconds: 0.2f,
            releaseSeconds: 0.1f,
            deltaTime: 0.05f);
        Vector3 fading = ActorMotionVisualRules.JumpHoldStretchScale(state, 0.92f, 1.1f, EaseSettings.Linear);
        ActorMotionVisualRules.TickJumpHoldStretch(state, false, true, true, true, 0.2f, 0.1f, 0.05f);
        Vector3 released = ActorMotionVisualRules.JumpHoldStretchScale(state, 0.92f, 1.1f, EaseSettings.Linear);

        Assert.That(fading.y, Is.GreaterThan(1f));
        Assert.That(fading.y, Is.LessThan(1.1f));
        Assert.That(released, Is.EqualTo(Vector3.one));
    }

    [Test]
    public void JumpHoldStretchDoesNotApplyWhenDisabledOrFalling()
    {
        var disabled = new ActorMotionVisualState();
        var falling = new ActorMotionVisualState();

        ActorMotionVisualRules.TickJumpHoldStretch(disabled, true, true, true, false, 0.2f, 0.1f, 0.2f);
        ActorMotionVisualRules.TickJumpHoldStretch(falling, true, false, true, true, 0.2f, 0.1f, 0.2f);

        Assert.That(ActorMotionVisualRules.JumpHoldStretchScale(disabled, 0.92f, 1.1f, EaseSettings.Linear), Is.EqualTo(Vector3.one));
        Assert.That(ActorMotionVisualRules.JumpHoldStretchScale(falling, 0.92f, 1.1f, EaseSettings.Linear), Is.EqualTo(Vector3.one));
    }
}
