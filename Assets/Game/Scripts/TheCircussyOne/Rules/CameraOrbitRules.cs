using UnityEngine;
using TheCircussyOne.Runtime;

namespace TheCircussyOne.Rules
{
    public readonly struct CameraOrbitPose
    {
        public CameraOrbitPose(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }

        public Vector3 Position { get; }
        public Quaternion Rotation { get; }
    }

    public static class CameraOrbitRules
    {
        public static void ApplyLook(
            CameraOrbitState state,
            Vector2 look,
            LookInputKind lookKind,
            float triggerOrbitAxis,
            float sensitivityMultiplier,
            float mouseYawSensitivity,
            float mousePitchSensitivity,
            float gamepadYawSpeed,
            float gamepadPitchSpeed,
            float gamepadTriggerYawSpeed,
            float gamepadTriggerDeadzone,
            float minPitch,
            float maxPitch,
            float deltaTime)
        {
            float sensitivity = Mathf.Max(0f, sensitivityMultiplier);
            if (lookKind == LookInputKind.PointerDelta)
            {
                state.Yaw += look.x * mouseYawSensitivity * sensitivity;
                state.Pitch -= look.y * mousePitchSensitivity * sensitivity;
            }
            else if (lookKind == LookInputKind.Stick)
            {
                state.Yaw += look.x * gamepadYawSpeed * sensitivity * deltaTime;
                state.Pitch -= look.y * gamepadPitchSpeed * sensitivity * deltaTime;
            }

            float triggerAxis = TriggerOrbitAxis(triggerOrbitAxis, gamepadTriggerDeadzone);
            state.Yaw += triggerAxis * Mathf.Max(0f, gamepadTriggerYawSpeed) * sensitivity * deltaTime;

            state.Pitch = Mathf.Clamp(state.Pitch, Mathf.Min(minPitch, maxPitch), Mathf.Max(minPitch, maxPitch));
        }

        public static float TriggerOrbitAxis(float axis, float deadzone)
        {
            float safeAxis = Mathf.Clamp(axis, -1f, 1f);
            return Mathf.Abs(safeAxis) > Mathf.Max(0f, deadzone) ? safeAxis : 0f;
        }

        public static CameraOrbitPose Pose(Vector3 target, float yaw, float pitch, float distance)
        {
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 offset = rotation * (Vector3.back * distance);
            Vector3 position = target + offset;
            Quaternion lookRotation = Quaternion.LookRotation(target - position, Vector3.up);
            return new CameraOrbitPose(position, lookRotation);
        }

        public static Vector3 ResolveObstructedCameraPosition(
            Vector3 target,
            Vector3 desiredPosition,
            float hitDistance,
            float padding)
        {
            Vector3 toDesired = desiredPosition - target;
            float desiredDistance = toDesired.magnitude;
            if (desiredDistance <= 0.001f)
            {
                return desiredPosition;
            }

            float resolvedDistance = Mathf.Clamp(hitDistance - Mathf.Max(0f, padding), 0.001f, desiredDistance);
            return target + toDesired / desiredDistance * resolvedDistance;
        }

        public static Vector3 LookAheadTarget(Vector3 playerPosition, float yaw, float targetHeight, float lookAheadDistance)
        {
            Quaternion yawRotation = Quaternion.Euler(0f, yaw, 0f);
            Vector3 forward = yawRotation * Vector3.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude > 0f)
            {
                forward.Normalize();
            }

            return playerPosition + Vector3.up * targetHeight + forward * lookAheadDistance;
        }

        public static Vector3 DynamicLookAheadTarget(
            Vector3 playerPosition,
            float yaw,
            float pitch,
            float minPitch,
            float maxPitch,
            float targetHeight,
            float lookAheadDistance,
            float topDownLookAheadDistance,
            float lookAheadPitchBias)
        {
            return DynamicLookAheadTarget(
                playerPosition,
                yaw,
                pitch,
                minPitch,
                maxPitch,
                targetHeight,
                lookAheadDistance,
                topDownLookAheadDistance,
                lookAheadPitchBias,
                EaseSettings.Linear);
        }

        public static Vector3 DynamicLookAheadTarget(
            Vector3 playerPosition,
            float yaw,
            float pitch,
            float minPitch,
            float maxPitch,
            float targetHeight,
            float lookAheadDistance,
            float topDownLookAheadDistance,
            float lookAheadPitchBias,
            EaseSettings pitchEase)
        {
            float dynamicLookAhead = PitchLookAheadDistance(
                pitch,
                minPitch,
                maxPitch,
                lookAheadDistance,
                topDownLookAheadDistance,
                lookAheadPitchBias,
                pitchEase);
            return LookAheadTarget(playerPosition, yaw, targetHeight, dynamicLookAhead);
        }

        public static float PitchLookAheadDistance(
            float pitch,
            float minPitch,
            float maxPitch,
            float lowPitchLookAheadDistance,
            float topDownLookAheadDistance,
            float lookAheadPitchBias)
        {
            return PitchLookAheadDistance(
                pitch,
                minPitch,
                maxPitch,
                lowPitchLookAheadDistance,
                topDownLookAheadDistance,
                lookAheadPitchBias,
                EaseSettings.Linear);
        }

        public static float PitchLookAheadDistance(
            float pitch,
            float minPitch,
            float maxPitch,
            float lowPitchLookAheadDistance,
            float topDownLookAheadDistance,
            float lookAheadPitchBias,
            EaseSettings pitchEase)
        {
            float lowPitch = Mathf.Min(minPitch, maxPitch);
            float highPitch = Mathf.Max(minPitch, maxPitch);
            float topDown01 = Mathf.InverseLerp(lowPitch, highPitch, pitch);
            float shapedTopDown01 = GameEasing.Evaluate01(pitchEase, topDown01);
            float lowPitchWeight = Mathf.Pow(1f - shapedTopDown01, Mathf.Max(0.01f, lookAheadPitchBias));
            return Mathf.Lerp(topDownLookAheadDistance, lowPitchLookAheadDistance, lowPitchWeight);
        }
    }
}
