using UnityEngine;
using PrimeTween;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using TheCircussyOne.Runtime;

namespace TheCircussyOne.Visuals
{
    public readonly struct PlayerMoveResult
    {
        public PlayerMoveResult(CollisionFlags collisionFlags, bool isGrounded)
            : this(collisionFlags, isGrounded, isGrounded, isGrounded, Vector3.up)
        {
        }

        public PlayerMoveResult(
            CollisionFlags collisionFlags,
            bool isGrounded,
            bool isVisuallyGrounded,
            bool isWalkableGrounded,
            Vector3 groundNormal)
        {
            CollisionFlags = collisionFlags;
            IsGrounded = isGrounded;
            IsVisuallyGrounded = isVisuallyGrounded;
            IsWalkableGrounded = isWalkableGrounded;
            GroundNormal = groundNormal.sqrMagnitude > 0.000001f ? groundNormal.normalized : Vector3.up;
        }

        public CollisionFlags CollisionFlags { get; }
        public bool IsGrounded { get; }
        public bool IsVisuallyGrounded { get; }
        public bool IsWalkableGrounded { get; }
        public Vector3 GroundNormal { get; }
    }

    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerView : MonoBehaviour
    {
        [SerializeField] private CharacterController controller;
        [SerializeField] private Renderer bodyRenderer;
        [SerializeField] private Transform body;
        [SerializeField] private Transform bodyScaleRoot;
        [SerializeField] private HealthBarView healthBar;
        [SerializeField] private ActorBodyView actorBody;
        [SerializeField] private ParticleSystem moveDust;
        [SerializeField] private ParticleSystem jumpTrail;
        [SerializeField] private Transform groundShadow;

        private bool isAirborneForVfx;
        private bool worldHealthBarEnabled = true;
        private float groundProbeDistance = 0.35f;
        private float visualGroundProbeMaxSlope = 75f;
        private int groundProbeMask = ~0;
        private Vector3 visualGroundNormal = Vector3.up;
        private bool jumpHoldVisualHeld;
        private bool jumpHoldVisualRising;
        private bool jumpHoldVisualAirborne;
        private bool jumpAnimationVisualActive;
        private bool visuallyGroundedForAnimation = true;
        private readonly ActorRuntimeModelView runtimeModel = new();
        private readonly ActorAnimancerDriver animancerDriver = new();
        private PerformerDefinition performerVisuals;

        public Transform Body => BodyView.Body;

        public Vector3 Position => transform.position;
        public float MotionVisualTargetSpeed01 => BodyView.MotionVisualTargetSpeed01;
        public bool IsAirborneForVfx => isAirborneForVfx;
        public Vector3 VisualOrbitUp
        {
            get
            {
                ResolveComponents();
                Transform scaleRoot = actorBody != null ? actorBody.BodyScaleRoot : null;
                Vector3 up = scaleRoot != null ? scaleRoot.up : Vector3.up;
                return up.sqrMagnitude > 0.000001f ? up.normalized : Vector3.up;
            }
        }
        public bool IsGrounded
        {
            get
            {
                ResolveComponents();
                return controller != null && controller.isGrounded;
            }
        }

        public Vector3 GroundProjectedPosition => new(transform.position.x, 0f, transform.position.z);
        public Color CurrentBodyColor => BodyView.CurrentBodyColor;
        public GameObject RuntimeModelInstance => runtimeModel.Instance;
        public bool HasVisibleFallbackBody => runtimeModel.HasVisibleFallbackRenderer;
        public AnimationClip RuntimeAnimationClip => animancerDriver.CurrentClip;
        public float RuntimeAnimationSpeed => animancerDriver.CurrentSpeed;
        public float RuntimeAnimationBlend01 => animancerDriver.CurrentBlend01;
        public bool RuntimeAnimationPaused => animancerDriver.IsPlaybackPaused;
        public Collider ContactCollider
        {
            get
            {
                ResolveComponents();
                return controller != null && controller.enabled && gameObject.activeInHierarchy ? controller : null;
            }
        }
        public ParticleSystem MoveDust
        {
            get
            {
                ResolveComponents();
                return moveDust;
            }
        }
        public ParticleSystem JumpTrail
        {
            get
            {
                ResolveComponents();
                return jumpTrail;
            }
        }

        public void Teleport(Vector3 position)
        {
            ResolveComponents();
            bool controllerWasEnabled = controller != null && controller.enabled;
            if (controller != null)
            {
                controller.enabled = false;
            }

            transform.position = position;

            if (controller != null)
            {
                controller.enabled = controllerWasEnabled;
            }

            visualGroundNormal = Vector3.up;
            visuallyGroundedForAnimation = true;
            UpdateGroundProjectedShadow();
        }

        public bool CanMove
        {
            get
            {
                ResolveComponents();
                return isActiveAndEnabled
                    && controller != null
                    && controller.enabled
                    && controller.gameObject.activeInHierarchy;
            }
        }

        private void Awake()
        {
            ResolveComponents();
        }

        private ActorBodyView BodyView
        {
            get
            {
                ResolveComponents();
                return actorBody;
            }
        }

        public void ApplyConfig(GameConfig config, DamageFeedbackVisualConfig feedbackConfig = null)
        {
            ResolveComponents();
            ApplyControllerConfig(config);
            ResetMotionVisuals();
            ApplyWorldHealthBarConfig(feedbackConfig);

            Tween.StopAll(this);
            actorBody.CaptureBaseColor();
            actorBody.ClearBodyPropertyBlock();
            SetAirborneVisual(false);
            actorBody.UpdateGroundProjectedShadow(transform.position.y);
            SetHealthNormalized(1f);
        }

        public void ApplyControllerConfig(GameConfig config)
        {
            ResolveComponents();
            if (controller == null || config == null)
            {
                return;
            }

            controller.stepOffset = Mathf.Clamp(config.playerControllerStepOffset, 0f, Mathf.Max(0f, controller.height));
            controller.slopeLimit = Mathf.Clamp(config.playerControllerSlopeLimit, 0f, 89f);
            groundProbeDistance = Mathf.Max(0f, config.playerGroundProbeDistance);
            visualGroundProbeMaxSlope = Mathf.Clamp(config.playerVisualGroundProbeMaxSlope, 0f, 89f);
            groundProbeMask = GameLayers.EnvironmentMaskExcludingGameplay;
        }

        public PlayerMoveResult Move(Vector3 worldDirection, float speed, float deltaTime)
        {
            ResolveComponents();
            if (worldDirection.sqrMagnitude > 1f)
            {
                worldDirection.Normalize();
            }

            Vector3 velocity = worldDirection * speed;
            return Move(velocity, worldDirection, deltaTime);
        }

        public PlayerMoveResult Move(Vector3 velocity, Vector3 facingDirection, float deltaTime)
        {
            ResolveComponents();
            if (!CanMove)
            {
                return new PlayerMoveResult(CollisionFlags.None, false);
            }

            CollisionFlags collisionFlags = controller.Move(velocity * deltaTime);
            bool grounded = controller.isGrounded || (collisionFlags & CollisionFlags.Below) != 0;
            bool hasGroundProbe = TryProbeGround(out Vector3 groundNormal, out float slopeAngle);
            bool hasVisualGroundProbe = hasGroundProbe && slopeAngle <= visualGroundProbeMaxSlope + 0.01f;
            bool hasWalkableGroundProbe = hasGroundProbe && slopeAngle <= controller.slopeLimit + 0.01f;
            bool visuallyGrounded = grounded || hasVisualGroundProbe;
            bool walkableGrounded = grounded || hasWalkableGroundProbe;
            visualGroundNormal = visuallyGrounded ? groundNormal : Vector3.up;
            visuallyGroundedForAnimation = visuallyGrounded;
            UpdateGroundProjectedShadow();

            facingDirection.y = 0f;
            if (facingDirection.sqrMagnitude > 0.001f)
            {
                Quaternion target = Quaternion.LookRotation(facingDirection.normalized, Vector3.up);
                transform.rotation = target;
            }

            return new PlayerMoveResult(collisionFlags, grounded, visuallyGrounded, walkableGrounded, visualGroundNormal);
        }

        public PlayerMoveResult MoveDisplacement(Vector3 displacement)
        {
            ResolveComponents();
            if (!CanMove)
            {
                return new PlayerMoveResult(CollisionFlags.None, false);
            }

            CollisionFlags collisionFlags = controller.Move(displacement);
            bool grounded = controller.isGrounded || (collisionFlags & CollisionFlags.Below) != 0;
            bool hasGroundProbe = TryProbeGround(out Vector3 groundNormal, out float slopeAngle);
            bool hasVisualGroundProbe = hasGroundProbe && slopeAngle <= visualGroundProbeMaxSlope + 0.01f;
            bool hasWalkableGroundProbe = hasGroundProbe && slopeAngle <= controller.slopeLimit + 0.01f;
            bool visuallyGrounded = grounded || hasVisualGroundProbe;
            bool walkableGrounded = grounded || hasWalkableGroundProbe;
            visualGroundNormal = visuallyGrounded ? groundNormal : Vector3.up;
            visuallyGroundedForAnimation = visuallyGrounded;
            UpdateGroundProjectedShadow();
            return new PlayerMoveResult(collisionFlags, grounded, visuallyGrounded, walkableGrounded, visualGroundNormal);
        }

        public void ApplyPerformerVisuals(PerformerDefinition performer)
        {
            ResolveComponents();
            performerVisuals = performer;
            runtimeModel.Apply(bodyScaleRoot, performer != null ? performer.worldPrefab : null, performer?.modelTransform, body);
            BodyView.SetExternalBodyRenderers(runtimeModel.ModelRenderers);
            animancerDriver.Configure(runtimeModel.Instance);
            BodyView.CaptureBaseColor();
            BodyView.ClearBodyPropertyBlock();
        }

        public void PlayDamagePulse(float seconds)
        {
            PlayDamagePulse(seconds, EaseSettings.OutQuad, 1.18f);
        }

        public void PlayDamagePulse(float seconds, EaseSettings ease, float scaleMultiplier)
        {
            BodyView.StartPulse(Mathf.Max(0f, seconds), Mathf.Max(1f, scaleMultiplier), ease);
        }

        public void PlayDamagePulse(ActorMotionVisualConfig motionConfig)
        {
            if (motionConfig == null || !motionConfig.enabled || !motionConfig.playerDamagePulse.enabled)
            {
                return;
            }

            PlayDamagePulse(
                motionConfig.playerDamagePulse.seconds,
                motionConfig.playerDamagePulse.ease,
                motionConfig.playerDamagePulse.scaleMultiplier);
        }

        public void PlayJumpTakeoffPulse(ActorMotionVisualConfig motionConfig)
        {
            PlayScalePulse(motionConfig != null ? motionConfig.playerJumpTakeoffPulse : new ActorScalePulseSettings(true, 0.16f, 1f, EaseSettings.OutBack, 0.86f, 1.16f, 0.12f));
        }

        public void PlayJumpLandPulse(ActorMotionVisualConfig motionConfig)
        {
            PlayScalePulse(motionConfig != null ? motionConfig.playerJumpLandPulse : new ActorScalePulseSettings(true, 0.20f, 1f, EaseSettings.OutBack, 1.18f, 0.82f, 0.22f));
        }

        public void PlayDamageFlash(DamageFeedbackVisualConfig feedbackConfig)
        {
            PlayDamageFlash(
                feedbackConfig != null ? feedbackConfig.playerDamageFlashColor : new Color(1f, 0.08f, 0.08f, 1f),
                feedbackConfig != null ? feedbackConfig.playerDamageFlashSeconds : 0.12f,
                feedbackConfig != null ? feedbackConfig.playerDamageFlashEase : EaseSettings.OutQuad,
                feedbackConfig != null ? feedbackConfig.playerDamageFlashEmissionStrength : 2f);
        }

        public void PlayDamageFlash(Color flashColor, float seconds, EaseSettings ease, float emissionStrength)
        {
            ResolveComponents();
            PlayFlash(flashColor, seconds, ease, emissionStrength);
        }

        public void SetHealthNormalized(float value)
        {
            ResolveComponents();
            BodyView.SetHealthNormalized(value, worldHealthBarEnabled);
        }

        public void ApplyWorldHealthBarConfig(DamageFeedbackVisualConfig feedbackConfig)
        {
            ResolveComponents();
            if (feedbackConfig == null)
            {
                worldHealthBarEnabled = true;
                SetHealthNormalized(1f);
                return;
            }

            worldHealthBarEnabled = feedbackConfig.playerWorldHealthBarEnabled;
            if (!worldHealthBarEnabled)
            {
                BodyView.SetHealthNormalized(1f, enabled: false);
                return;
            }

            BodyView.ConfigureHealthBar(
                feedbackConfig.playerWorldHealthBarWidth,
                feedbackConfig.playerWorldHealthBarLocalOffset,
                feedbackConfig.playerWorldHealthBarBackgroundColor,
                feedbackConfig.playerWorldHealthBarFillColor,
                feedbackConfig.playerWorldHealthBarBackgroundThickness,
                feedbackConfig.playerWorldHealthBarFillThickness,
                feedbackConfig.playerWorldHealthBarVisibleThroughPlayer,
                feedbackConfig.playerWorldHealthBarSortingOrderBase,
                feedbackConfig.playerWorldHealthBarRenderQueueBase);
            SetHealthNormalized(1f);
        }

        public void ApplyMoveDustVfxConfig(VfxVisualConfig vfxConfig)
        {
            ResolveComponents();
            if (moveDust == null)
            {
                return;
            }

            moveDust.transform.localPosition = vfxConfig != null ? vfxConfig.playerMoveDustLocalOffset : new Vector3(0f, 0.08f, -0.32f);
            var main = moveDust.main;
            main.loop = true;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startColor = vfxConfig != null ? vfxConfig.playerMoveDustColor : new Color(0.72f, 0.92f, 1f, 0.42f);
            main.startLifetime = vfxConfig != null ? vfxConfig.playerMoveDustLifetime : 0.46f;
            main.startSize = vfxConfig != null ? vfxConfig.playerMoveDustStartSize : 0.22f;

            var emission = moveDust.emission;
            emission.enabled = vfxConfig == null || (vfxConfig.enabled && vfxConfig.playerMoveDustEnabled);
            emission.rateOverTime = 0f;

            if (moveDust.isPlaying)
            {
                moveDust.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        public void ApplyJumpTrailVfxConfig(VfxVisualConfig vfxConfig)
        {
            ResolveComponents();
            if (jumpTrail == null)
            {
                return;
            }

            jumpTrail.transform.localPosition = vfxConfig != null ? vfxConfig.playerJumpTrailLocalOffset : new Vector3(0f, 0.12f, -0.08f);
            var main = jumpTrail.main;
            main.loop = true;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startColor = vfxConfig != null ? vfxConfig.playerJumpTrailColor : new Color(0.62f, 0.88f, 1f, 0.62f);
            main.startLifetime = vfxConfig != null ? vfxConfig.playerJumpTrailLifetime : 0.32f;
            main.startSize = vfxConfig != null ? vfxConfig.playerJumpTrailStartSize : 0.09f;

            var emission = jumpTrail.emission;
            emission.enabled = vfxConfig == null || (vfxConfig.enabled && vfxConfig.playerJumpTrailEnabled);
            emission.rateOverTime = vfxConfig != null ? vfxConfig.playerJumpTrailEmissionRate : 26f;

            if (jumpTrail.isPlaying)
            {
                jumpTrail.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        public void SetAirborneVisual(bool isAirborne)
        {
            isAirborneForVfx = isAirborne;
        }

        public void SetJumpHoldVisual(bool jumpHeld, bool rising, bool airborne)
        {
            jumpHoldVisualHeld = jumpHeld;
            jumpHoldVisualRising = rising;
            jumpHoldVisualAirborne = airborne;
        }

        public void SetJumpAnimationVisual(bool active)
        {
            jumpAnimationVisualActive = active;
        }

        public void SetMotionVisualSpeed(float normalizedSpeed, bool immediate = false)
        {
            float clampedSpeed = Mathf.Clamp01(normalizedSpeed);
            BodyView.SetMotionVisualSpeed(clampedSpeed, immediate);
        }

        public void SetWorldAnimationPaused(bool paused)
        {
            animancerDriver.SetPlaybackPaused(paused);
        }

        public void EvaluateCurrentAnimationPose(float deltaTime)
        {
            animancerDriver.EvaluateCurrentPose(deltaTime);
        }

        public void TickMotionVisuals(ActorMotionVisualConfig motionConfig, float deltaTime)
        {
            ResolveComponents();
            if (body == null)
            {
                return;
            }

            if (motionConfig == null || !motionConfig.enabled)
            {
                BodyView.SetJumpHoldStretch(false, false, false, false, 0f, 0f, deltaTime);
                BodyView.BodyScaleRoot.localScale = Vector3.one;
                BodyView.ApplyGroundTilt(Vector3.up, 0f, 0f, deltaTime);
                animancerDriver.TickPerformer(performerVisuals?.animation, visuallyGroundedForAnimation, jumpAnimationVisualActive, 0f, deltaTime);
                return;
            }

            PerformerMotionVisualProfile performerMotion = performerVisuals?.motionVisuals;
            performerMotion?.EnsureWorkflowDefaults();
            ActorSquashStretchCycleSettings idleMotion = performerMotion != null
                ? performerMotion.IdleSettings(motionConfig.playerIdle)
                : motionConfig.playerIdle;
            ActorSquashStretchCycleSettings moveMotion = performerMotion != null
                ? performerMotion.MoveSettings(motionConfig.playerMove)
                : motionConfig.playerMove;

            BodyView.SetJumpHoldStretch(
                jumpHoldVisualHeld,
                jumpHoldVisualRising,
                jumpHoldVisualAirborne,
                motionConfig.jumpHoldStretchEnabled,
                motionConfig.jumpHoldMaxSeconds,
                motionConfig.jumpHoldReleaseSeconds,
                deltaTime);
            BodyView.TickMotionVisuals(
                deltaTime,
                idleMotion.amplitude,
                idleMotion.secondsPerCycle,
                idleMotion.xzCompensation,
                moveMotion.amplitude,
                moveMotion.secondsPerCycle,
                moveMotion.xzCompensation,
                motionConfig.speedSmoothingSharpness,
                motionConfig.speedSmoothingEase,
                motionConfig.jumpHoldXZScale,
                motionConfig.jumpHoldYScale,
                motionConfig.jumpHoldEase);
            BodyView.ApplyGroundTilt(
                visualGroundNormal,
                motionConfig.inclineTiltEnabled ? motionConfig.playerInclineTiltMaxDegrees : 0f,
                motionConfig.inclineTiltSmoothingSharpness,
                deltaTime);
            animancerDriver.TickPerformer(
                performerVisuals?.animation,
                visuallyGroundedForAnimation,
                jumpAnimationVisualActive,
                MotionVisualTargetSpeed01,
                deltaTime);
        }

        public void ResetMotionVisuals()
        {
            ResolveComponents();
            SetJumpHoldVisual(false, false, false);
            SetJumpAnimationVisual(false);
            visuallyGroundedForAnimation = true;
            BodyView.ResetMotionVisuals(resetScaleRootPosition: true);
            BodyView.UpdateGroundProjectedShadow(transform.position.y);
        }

        private void ResolveComponents()
        {
            if (actorBody == null)
            {
                actorBody = GetComponent<ActorBodyView>();
                if (actorBody == null)
                {
                    actorBody = gameObject.AddComponent<ActorBodyView>();
                }
            }

            if (controller == null)
            {
                controller = GetComponent<CharacterController>();
            }

            actorBody.ResolveComponents();
            body = actorBody.Body;
            bodyScaleRoot = actorBody.BodyScaleRoot;
            healthBar = actorBody.HealthBar;
            groundShadow = actorBody.GroundShadow;
            bodyRenderer = body != null ? body.GetComponent<Renderer>() : null;

            if (moveDust == null)
            {
                Transform dustChild = transform.Find("Player Move Dust");
                if (dustChild != null)
                {
                    moveDust = dustChild.GetComponentInChildren<ParticleSystem>(includeInactive: true);
                }
            }

            if (jumpTrail == null)
            {
                Transform trailChild = transform.Find("Player Jump Trail");
                if (trailChild != null)
                {
                    jumpTrail = trailChild.GetComponentInChildren<ParticleSystem>(includeInactive: true);
                }
            }

            actorBody.ResolveComponents();
        }

        private void PlayScalePulse(ActorScalePulseSettings pulse)
        {
            BodyView.StartPulse(pulse);
        }

        private void UpdateGroundProjectedShadow()
        {
            if (groundShadow == null)
            {
                return;
            }

            BodyView.UpdateGroundProjectedShadow(transform.position.y);
        }

        private void PlayFlash(Color flashColor, float seconds, EaseSettings ease, float emissionStrength)
        {
            BodyView.PlayFlash(flashColor, seconds, ease, emissionStrength);
        }

        private bool TryProbeGround(out Vector3 groundNormal, out float slopeAngle)
        {
            groundNormal = Vector3.up;
            slopeAngle = 0f;
            if (controller == null || groundProbeDistance <= 0f)
            {
                return false;
            }

            float radius = Mathf.Max(0.01f, controller.radius * 0.9f);
            float halfHeight = Mathf.Max(radius, controller.height * 0.5f);
            Vector3 center = transform.TransformPoint(controller.center);
            Vector3 origin = center + Vector3.up * Mathf.Max(0.02f, radius * 0.25f);
            float castDistance = Mathf.Max(0.01f, halfHeight - radius + groundProbeDistance + radius * 0.25f);
            if (!Physics.SphereCast(
                    origin,
                    radius,
                    Vector3.down,
                    out RaycastHit hit,
                    castDistance,
                    groundProbeMask,
                    QueryTriggerInteraction.Ignore))
            {
                return false;
            }

            groundNormal = hit.normal.sqrMagnitude > 0.000001f ? hit.normal.normalized : Vector3.up;
            slopeAngle = Vector3.Angle(groundNormal, Vector3.up);
            return true;
        }
    }
}
