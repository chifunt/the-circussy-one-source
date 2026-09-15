using UnityEngine;
using VContainer.Unity;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;
using TheCircussyOne.Visuals;

namespace TheCircussyOne.Runtime
{
    public sealed class CameraFollowSystem : ILateTickable
    {
        private readonly CameraConfig _config;
        private readonly PlayerView _player;
        private readonly CameraView _camera;
        private readonly IGameTime _time;
        private readonly IInputService _input;
        private readonly CameraOrbitState _orbit;

        public CameraFollowSystem(CameraConfig config, PlayerView player, CameraView camera, IGameTime time, IInputService input, CameraOrbitState orbit)
        {
            _config = config;
            _player = player;
            _camera = camera;
            _time = time;
            _input = input;
            _orbit = orbit;
            _orbit.Yaw = config.orbitYaw;
            _orbit.Pitch = config.orbitPitch;
        }

        public void LateTick()
        {
            if (_camera == null || _player == null)
            {
                return;
            }

            CameraOrbitRules.ApplyLook(
                _orbit,
                _input.Look,
                _input.LookKind,
                _input.CameraOrbitTriggerAxis,
                _config.lookSensitivityMultiplier,
                _config.mouseYawSensitivity,
                _config.mousePitchSensitivity,
                _config.gamepadYawSpeed,
                _config.gamepadPitchSpeed,
                _config.gamepadTriggerYawSpeed,
                _config.gamepadTriggerDeadzone,
                _config.minPitch,
                _config.maxPitch,
                _time.DeltaTime);

            Vector3 target = CameraOrbitRules.DynamicLookAheadTarget(
                _player.Position,
                _orbit.Yaw,
                _orbit.Pitch,
                _config.minPitch,
                _config.maxPitch,
                _config.targetHeight,
                _config.lookAheadDistance,
                _config.topDownLookAheadDistance,
                _config.lookAheadPitchBias,
                _config.lookAheadPitchEase);
            CameraOrbitPose pose = CameraOrbitRules.Pose(
                target,
                _orbit.Yaw,
                _orbit.Pitch,
                _config.orbitDistance);
            Vector3 obstructionTarget = _player.Position + Vector3.up * _config.targetHeight;
            CameraObstructionResult obstruction = ResolveCameraObstructions(obstructionTarget, target, pose);
            if (obstruction.IsObstructed)
            {
                _camera.ApplyOrbitPoseImmediate(obstruction.Pose, _time.DeltaTime);
                return;
            }

            if (IsCurrentCameraUnsafe(obstructionTarget))
            {
                _camera.ApplyOrbitPoseImmediate(obstruction.Pose, _time.DeltaTime);
                return;
            }

            _camera.FollowOrbit(obstruction.Pose, _config.followSharpness, _time.DeltaTime, _config.followSmoothingEase);
        }

        private CameraObstructionResult ResolveCameraObstructions(Vector3 obstructionTarget, Vector3 lookTarget, CameraOrbitPose pose)
        {
            int obstructionMask = CameraObstructionMask();
            if (!_config.preventCameraClipping || obstructionMask == 0)
            {
                return CameraObstructionResult.Clear(pose);
            }

            Vector3 toCamera = pose.Position - obstructionTarget;
            float distance = toCamera.magnitude;
            if (distance <= 0.001f)
            {
                return CameraObstructionResult.Clear(pose);
            }

            Vector3 direction = toCamera / distance;
            if (!Physics.SphereCast(
                    obstructionTarget,
                    Mathf.Max(0.01f, _config.cameraObstructionRadius),
                    direction,
                    out RaycastHit hit,
                    distance,
                    obstructionMask,
                    QueryTriggerInteraction.Ignore))
            {
                return CameraObstructionResult.Clear(pose);
            }

            Vector3 position = CameraOrbitRules.ResolveObstructedCameraPosition(
                obstructionTarget,
                pose.Position,
                hit.distance,
                _config.cameraObstructionPadding);
            Quaternion rotation = Quaternion.LookRotation(lookTarget - position, Vector3.up);
            return CameraObstructionResult.Obstructed(new CameraOrbitPose(position, rotation));
        }

        private bool IsCurrentCameraUnsafe(Vector3 obstructionTarget)
        {
            int obstructionMask = CameraObstructionMask();
            if (!_config.preventCameraClipping || obstructionMask == 0 || _camera == null)
            {
                return false;
            }

            float radius = Mathf.Max(0.01f, _config.cameraObstructionRadius);
            Vector3 cameraPosition = _camera.transform.position;
            if (Physics.CheckSphere(cameraPosition, radius, obstructionMask, QueryTriggerInteraction.Ignore))
            {
                return true;
            }

            Vector3 toCamera = cameraPosition - obstructionTarget;
            float distance = toCamera.magnitude;
            if (distance <= 0.001f)
            {
                return false;
            }

            return Physics.SphereCast(
                obstructionTarget,
                radius,
                toCamera / distance,
                out _,
                distance,
                obstructionMask,
                QueryTriggerInteraction.Ignore);
        }

        private int CameraObstructionMask()
        {
            int gameplayMask = LayerMask.GetMask(
                GameLayers.Player,
                GameLayers.Enemy,
                GameLayers.Projectile,
                GameLayers.Pickup);
            return _config.cameraObstructionMask.value & ~gameplayMask;
        }

        private readonly struct CameraObstructionResult
        {
            private CameraObstructionResult(CameraOrbitPose pose, bool isObstructed)
            {
                Pose = pose;
                IsObstructed = isObstructed;
            }

            public CameraOrbitPose Pose { get; }
            public bool IsObstructed { get; }

            public static CameraObstructionResult Clear(CameraOrbitPose pose)
            {
                return new CameraObstructionResult(pose, false);
            }

            public static CameraObstructionResult Obstructed(CameraOrbitPose pose)
            {
                return new CameraObstructionResult(pose, true);
            }
        }
    }
}
