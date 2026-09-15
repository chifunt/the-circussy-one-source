using UnityEngine;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;

namespace TheCircussyOne.Visuals
{
    [RequireComponent(typeof(Camera))]
    public sealed class CameraView : MonoBehaviour
    {
        [SerializeField] private Camera cameraComponent;

        private Vector3 unshakenPosition;
        private Quaternion unshakenRotation;
        private bool hasUnshakenPose;
        private float shakeElapsed;
        private float shakeDuration;
        private float shakePositionAmplitude;
        private float shakeRotationDegrees;
        private float shakeFrequency;
        private float shakePhase;
        private EaseSettings shakeFalloffEase = EaseSettings.OutQuad;

        public Camera Camera
        {
            get
            {
                ResolveComponents();
                return cameraComponent;
            }
        }

        private void Awake()
        {
            ResolveComponents();
            ResetUnshakenPoseFromTransform();
        }

        private void OnEnable()
        {
            ResolveComponents();
            HealthBarView.RegisterBillboardCamera(cameraComponent);
        }

        private void OnDisable()
        {
            HealthBarView.UnregisterBillboardCamera(cameraComponent);
        }

        public void ApplyInitialPose(PlayerView player, CameraConfig config)
        {
            ResolveComponents();
            ApplyLens(config);
            cameraComponent.tag = "MainCamera";
            HealthBarView.RegisterBillboardCamera(cameraComponent);
            Vector3 target = CameraOrbitRules.DynamicLookAheadTarget(
                player.Position,
                config.orbitYaw,
                config.orbitPitch,
                config.minPitch,
                config.maxPitch,
                config.targetHeight,
                config.lookAheadDistance,
                config.topDownLookAheadDistance,
                config.lookAheadPitchBias,
                config.lookAheadPitchEase);
            CameraOrbitPose pose = CameraOrbitRules.Pose(
                target,
                config.orbitYaw,
                config.orbitPitch,
                config.orbitDistance);
            transform.SetPositionAndRotation(pose.Position, pose.Rotation);
            ResetUnshakenPoseFromTransform();
        }

        public void Follow(PlayerView player, CameraConfig config, float deltaTime)
        {
            ApplyLens(config);
            CameraOrbitPose pose = CameraOrbitRules.Pose(
                CameraOrbitRules.DynamicLookAheadTarget(
                    player.Position,
                    config.orbitYaw,
                    config.orbitPitch,
                    config.minPitch,
                    config.maxPitch,
                    config.targetHeight,
                    config.lookAheadDistance,
                    config.topDownLookAheadDistance,
                    config.lookAheadPitchBias,
                    config.lookAheadPitchEase),
                config.orbitYaw,
                config.orbitPitch,
                config.orbitDistance);
            FollowOrbit(pose, config.followSharpness, deltaTime, config.followSmoothingEase);
        }

        public void FollowOrbit(CameraOrbitPose pose, float followSharpness, float deltaTime)
        {
            FollowOrbit(pose, followSharpness, deltaTime, EaseSettings.Exponential);
        }

        public void FollowOrbit(CameraOrbitPose pose, float followSharpness, float deltaTime, EaseSettings followEase)
        {
            EnsureUnshakenPose();
            float t = GameEasing.SmoothingWeight(followSharpness, deltaTime, followEase);
            unshakenPosition = Vector3.Lerp(unshakenPosition, pose.Position, t);
            unshakenRotation = Quaternion.Slerp(unshakenRotation, pose.Rotation, t);
            ApplyPoseWithRelativeShake(deltaTime);
        }

        public void ApplyOrbitPoseImmediate(CameraOrbitPose pose, float deltaTime)
        {
            unshakenPosition = pose.Position;
            unshakenRotation = pose.Rotation;
            hasUnshakenPose = true;
            ApplyPoseWithRelativeShake(deltaTime);
        }

        public void ApplyLens(CameraConfig config)
        {
            ResolveComponents();
            if (cameraComponent == null || config == null)
            {
                return;
            }

            cameraComponent.farClipPlane = Mathf.Max(100f, config.farClipPlane);
        }

        public void PlayRelativeShake(DamageFeedbackVisualConfig config)
        {
            if (config == null || !config.playerDamageCameraShakeEnabled)
            {
                return;
            }

            if (config.playerDamageCameraShakeSeconds <= 0f)
            {
                return;
            }

            float positionAmplitude = Mathf.Max(0f, config.playerDamageCameraShakePositionAmplitude);
            float rotationDegrees = Mathf.Max(0f, config.playerDamageCameraShakeRotationDegrees);
            if (positionAmplitude <= 0f && rotationDegrees <= 0f)
            {
                return;
            }

            EnsureUnshakenPose();
            shakeElapsed = 0f;
            shakeDuration = config.playerDamageCameraShakeSeconds;
            shakePositionAmplitude = positionAmplitude;
            shakeRotationDegrees = rotationDegrees;
            shakeFrequency = Mathf.Max(0.01f, config.playerDamageCameraShakeFrequency);
            shakeFalloffEase = config.playerDamageCameraShakeFalloffEase.shape > 0f
                ? config.playerDamageCameraShakeFalloffEase
                : EaseSettings.OutQuad;
            shakePhase = Mathf.Repeat(shakePhase + 1.6180339f, Mathf.PI * 2f);
        }

        public bool HasActiveRelativeShake => shakeElapsed < shakeDuration && shakeDuration > 0f;

        public Vector3 UnshakenPosition => unshakenPosition;

        private void ApplyPoseWithRelativeShake(float deltaTime)
        {
            if (!HasActiveRelativeShake)
            {
                transform.SetPositionAndRotation(unshakenPosition, unshakenRotation);
                return;
            }

            shakeElapsed = Mathf.Min(shakeDuration, shakeElapsed + Mathf.Max(0f, deltaTime));
            float normalized = shakeDuration <= 0f ? 1f : Mathf.Clamp01(shakeElapsed / shakeDuration);
            float envelope = 1f - GameEasing.Evaluate01(shakeFalloffEase, normalized);
            float sample = shakeElapsed * shakeFrequency * Mathf.PI * 2f;

            Vector3 localOffset = new Vector3(
                Mathf.Sin(sample + shakePhase),
                Mathf.Sin(sample * 1.31f + shakePhase * 0.73f),
                0f) * (shakePositionAmplitude * envelope);
            Quaternion localRotation = Quaternion.Euler(
                Mathf.Sin(sample * 1.13f + shakePhase * 0.41f) * shakeRotationDegrees * 0.35f * envelope,
                Mathf.Sin(sample * 1.47f + shakePhase * 0.89f) * shakeRotationDegrees * 0.35f * envelope,
                Mathf.Sin(sample * 1.71f + shakePhase * 1.11f) * shakeRotationDegrees * envelope);

            transform.SetPositionAndRotation(
                unshakenPosition + unshakenRotation * localOffset,
                unshakenRotation * localRotation);
        }

        private void EnsureUnshakenPose()
        {
            if (!hasUnshakenPose)
            {
                ResetUnshakenPoseFromTransform();
            }
        }

        private void ResetUnshakenPoseFromTransform()
        {
            unshakenPosition = transform.position;
            unshakenRotation = transform.rotation;
            hasUnshakenPose = true;
        }

        private void ResolveComponents()
        {
            if (cameraComponent == null)
            {
                cameraComponent = GetComponent<Camera>();
            }
        }
    }
}
