using NUnit.Framework;
using UnityEngine;
using TheCircussyOne.Rules;
using TheCircussyOne.Runtime;

public sealed class CameraOrbitRulesTests
{
    [Test]
    public void MouseDeltaChangesYawAndPitch()
    {
        var state = new CameraOrbitState { Yaw = 0f, Pitch = 36f };

        ApplyLook(state, new Vector2(10f, -10f), LookInputKind.PointerDelta, deltaTime: 0.016f);

        Assert.That(state.Yaw, Is.EqualTo(0.8f).Within(0.001f));
        Assert.That(state.Pitch, Is.EqualTo(36.6f).Within(0.001f));
    }

    [Test]
    public void GamepadLookScalesByDeltaTime()
    {
        var state = new CameraOrbitState { Yaw = 0f, Pitch = 36f };

        ApplyLook(state, Vector2.right, LookInputKind.Stick, deltaTime: 0.5f);

        Assert.That(state.Yaw, Is.EqualTo(65f).Within(0.001f));
    }

    [Test]
    public void LookSensitivityMultiplierScalesMouseAndGamepad()
    {
        var mouseState = new CameraOrbitState { Yaw = 0f, Pitch = 36f };
        var gamepadState = new CameraOrbitState { Yaw = 0f, Pitch = 36f };

        ApplyLook(mouseState, new Vector2(10f, 0f), LookInputKind.PointerDelta, deltaTime: 0.016f, sensitivityMultiplier: 2f);
        ApplyLook(gamepadState, Vector2.right, LookInputKind.Stick, deltaTime: 0.5f, sensitivityMultiplier: 0.5f);

        Assert.That(mouseState.Yaw, Is.EqualTo(1.6f).Within(0.001f));
        Assert.That(gamepadState.Yaw, Is.EqualTo(32.5f).Within(0.001f));
    }

    [Test]
    public void RightTriggerIncreasesYaw()
    {
        var state = new CameraOrbitState { Yaw = 0f, Pitch = 36f };

        ApplyLook(state, Vector2.zero, LookInputKind.None, deltaTime: 0.5f, triggerOrbitAxis: 1f);

        Assert.That(state.Yaw, Is.EqualTo(65f).Within(0.001f));
        Assert.That(state.Pitch, Is.EqualTo(36f).Within(0.001f));
    }

    [Test]
    public void LeftTriggerDecreasesYaw()
    {
        var state = new CameraOrbitState { Yaw = 0f, Pitch = 36f };

        ApplyLook(state, Vector2.zero, LookInputKind.None, deltaTime: 0.5f, triggerOrbitAxis: -1f);

        Assert.That(state.Yaw, Is.EqualTo(-65f).Within(0.001f));
        Assert.That(state.Pitch, Is.EqualTo(36f).Within(0.001f));
    }

    [Test]
    public void TriggerDeadzoneSuppressesSmallInput()
    {
        var state = new CameraOrbitState { Yaw = 0f, Pitch = 36f };

        ApplyLook(state, Vector2.zero, LookInputKind.None, deltaTime: 0.5f, triggerOrbitAxis: 0.05f);

        Assert.That(state.Yaw, Is.EqualTo(0f).Within(0.001f));
        Assert.That(state.Pitch, Is.EqualTo(36f).Within(0.001f));
    }

    [Test]
    public void TriggerOrbitRespectsSensitivityMultiplier()
    {
        var state = new CameraOrbitState { Yaw = 0f, Pitch = 36f };

        ApplyLook(state, Vector2.zero, LookInputKind.None, deltaTime: 0.5f, triggerOrbitAxis: 1f, sensitivityMultiplier: 0.5f);

        Assert.That(state.Yaw, Is.EqualTo(32.5f).Within(0.001f));
        Assert.That(state.Pitch, Is.EqualTo(36f).Within(0.001f));
    }

    [Test]
    public void TriggerOrbitStacksWithStickYaw()
    {
        var state = new CameraOrbitState { Yaw = 0f, Pitch = 36f };

        ApplyLook(state, Vector2.right, LookInputKind.Stick, deltaTime: 0.5f, triggerOrbitAxis: 1f);

        Assert.That(state.Yaw, Is.EqualTo(130f).Within(0.001f));
    }

    [Test]
    public void PitchClampsBetweenMinAndMax()
    {
        var state = new CameraOrbitState { Yaw = 0f, Pitch = 36f };

        ApplyLook(state, new Vector2(0f, -1000f), LookInputKind.PointerDelta, deltaTime: 0.016f);
        Assert.That(state.Pitch, Is.EqualTo(58f).Within(0.001f));

        ApplyLook(state, new Vector2(0f, 1000f), LookInputKind.PointerDelta, deltaTime: 0.016f);
        Assert.That(state.Pitch, Is.EqualTo(20f).Within(0.001f));
    }

    [Test]
    public void PitchClampAllowsNegativeMinimum()
    {
        var state = new CameraOrbitState { Yaw = 0f, Pitch = 0f };

        ApplyLook(state, new Vector2(0f, 1000f), LookInputKind.PointerDelta, deltaTime: 0.016f, minPitch: -30f, maxPitch: 58f);

        Assert.That(state.Pitch, Is.EqualTo(-30f).Within(0.001f));
    }

    [Test]
    public void OrbitPoseMatchesYawPitchDistanceAroundTarget()
    {
        CameraOrbitPose pose = CameraOrbitRules.Pose(Vector3.zero, yaw: 0f, pitch: 36f, distance: 20f);

        Assert.That(pose.Position.x, Is.EqualTo(0f).Within(0.001f));
        Assert.That(pose.Position.y, Is.EqualTo(Mathf.Sin(36f * Mathf.Deg2Rad) * 20f).Within(0.001f));
        Assert.That(pose.Position.z, Is.EqualTo(-Mathf.Cos(36f * Mathf.Deg2Rad) * 20f).Within(0.001f));
    }

    [Test]
    public void ObstructionResolutionMovesCameraBeforeHit()
    {
        Vector3 resolved = CameraOrbitRules.ResolveObstructedCameraPosition(
            target: Vector3.zero,
            desiredPosition: new Vector3(0f, 0f, -10f),
            hitDistance: 4f,
            padding: 0.25f);

        Assert.That(resolved.x, Is.EqualTo(0f).Within(0.001f));
        Assert.That(resolved.y, Is.EqualTo(0f).Within(0.001f));
        Assert.That(resolved.z, Is.EqualTo(-3.75f).Within(0.001f));
    }

    [Test]
    public void DefaultLayerFloorObstructsNegativePitchCamera()
    {
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.layer = 0;
        try
        {
            Physics.SyncTransforms();
            Vector3 target = new Vector3(0f, 1.35f, 0f);
            CameraOrbitPose pose = CameraOrbitRules.Pose(target, yaw: 0f, pitch: -45f, distance: 10f);
            Vector3 toCamera = pose.Position - target;
            float distance = toCamera.magnitude;
            float radius = 0.28f;

            Assert.That(pose.Position.y, Is.LessThan(0f));
            Assert.That(Physics.SphereCast(
                    target,
                    radius,
                    toCamera.normalized,
                    out RaycastHit hit,
                    distance,
                    1,
                    QueryTriggerInteraction.Ignore),
                Is.True);

            Vector3 resolved = CameraOrbitRules.ResolveObstructedCameraPosition(target, pose.Position, hit.distance, padding: 0.08f);
            Assert.That(resolved.y, Is.GreaterThanOrEqualTo(radius - 0.081f));
        }
        finally
        {
            Object.DestroyImmediate(floor);
        }
    }

    [Test]
    public void LookAheadTargetMovesInCameraYawDirection()
    {
        Vector3 target = CameraOrbitRules.LookAheadTarget(Vector3.zero, yaw: 90f, targetHeight: 1.35f, lookAheadDistance: 9f);

        Assert.That(target.x, Is.EqualTo(9f).Within(0.001f));
        Assert.That(target.y, Is.EqualTo(1.35f).Within(0.001f));
        Assert.That(target.z, Is.EqualTo(0f).Within(0.001f));
    }

    [Test]
    public void DynamicLookAheadKeepsFullFramingAtLowPitch()
    {
        Vector3 target = CameraOrbitRules.DynamicLookAheadTarget(
            Vector3.zero,
            yaw: 90f,
            pitch: 20f,
            minPitch: 20f,
            maxPitch: 58f,
            targetHeight: 1.35f,
            lookAheadDistance: 9f,
            topDownLookAheadDistance: 0f,
            lookAheadPitchBias: 1f);

        Assert.That(target.x, Is.EqualTo(9f).Within(0.001f));
        Assert.That(target.y, Is.EqualTo(1.35f).Within(0.001f));
        Assert.That(target.z, Is.EqualTo(0f).Within(0.001f));
    }

    [Test]
    public void DynamicLookAheadCentersPlayerAtTopDownPitch()
    {
        Vector3 target = CameraOrbitRules.DynamicLookAheadTarget(
            Vector3.zero,
            yaw: 90f,
            pitch: 58f,
            minPitch: 20f,
            maxPitch: 58f,
            targetHeight: 1.35f,
            lookAheadDistance: 9f,
            topDownLookAheadDistance: 0f,
            lookAheadPitchBias: 1f);

        Assert.That(target.x, Is.EqualTo(0f).Within(0.001f));
        Assert.That(target.y, Is.EqualTo(1.35f).Within(0.001f));
        Assert.That(target.z, Is.EqualTo(0f).Within(0.001f));
    }

    [Test]
    public void DynamicLookAheadBlendsBetweenLowAndTopDownPitch()
    {
        float lookAhead = CameraOrbitRules.PitchLookAheadDistance(
            pitch: 39f,
            minPitch: 20f,
            maxPitch: 58f,
            lowPitchLookAheadDistance: 9f,
            topDownLookAheadDistance: 0f,
            lookAheadPitchBias: 1f);

        Assert.That(lookAhead, Is.EqualTo(4.5f).Within(0.001f));
    }

    [Test]
    public void DynamicLookAheadUsesConfigurablePitchEase()
    {
        float linear = CameraOrbitRules.PitchLookAheadDistance(
            pitch: 39f,
            minPitch: 20f,
            maxPitch: 58f,
            lowPitchLookAheadDistance: 9f,
            topDownLookAheadDistance: 0f,
            lookAheadPitchBias: 1f,
            pitchEase: EaseSettings.Linear);
        float eased = CameraOrbitRules.PitchLookAheadDistance(
            pitch: 39f,
            minPitch: 20f,
            maxPitch: 58f,
            lowPitchLookAheadDistance: 9f,
            topDownLookAheadDistance: 0f,
            lookAheadPitchBias: 1f,
            pitchEase: EaseSettings.OutCubic);

        Assert.That(eased, Is.LessThan(linear));
    }

    private static void ApplyLook(
        CameraOrbitState state,
        Vector2 look,
        LookInputKind kind,
        float deltaTime,
        float sensitivityMultiplier = 1f,
        float triggerOrbitAxis = 0f,
        float minPitch = 20f,
        float maxPitch = 58f)
    {
        CameraOrbitRules.ApplyLook(
            state,
            look,
            kind,
            triggerOrbitAxis: triggerOrbitAxis,
            sensitivityMultiplier: sensitivityMultiplier,
            mouseYawSensitivity: 0.08f,
            mousePitchSensitivity: 0.06f,
            gamepadYawSpeed: 130f,
            gamepadPitchSpeed: 90f,
            gamepadTriggerYawSpeed: 130f,
            gamepadTriggerDeadzone: 0.08f,
            minPitch: minPitch,
            maxPitch: maxPitch,
            deltaTime: deltaTime);
    }
}
