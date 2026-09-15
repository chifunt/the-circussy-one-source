using System;
using PrimeTween;
using UnityEngine;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using TheCircussyOne.Runtime;

namespace TheCircussyOne.Visuals
{
    public sealed class EnemyView : MonoBehaviour
    {
        [SerializeField] private Renderer bodyRenderer;
        [SerializeField] private Transform body;
        [SerializeField] private Transform bodyScaleRoot;
        [SerializeField] private HealthBarView healthBar;
        [SerializeField] private ActorBodyView actorBody;
        [SerializeField] private EnemyHitbox hurtbox;
        [SerializeField] private EnemyHitbox contactHitbox;
        [SerializeField] private EnemyHitbox movementBody;

        private bool motionVisualsSuspended;
        private readonly ActorRuntimeModelView runtimeModel = new();
        private readonly ActorAnimancerDriver animancerDriver = new();
        private EnemyAnimationProfile animationProfile;
        private EnemyLocomotionProfile locomotionProfile;

        public Vector3 Position => transform.position;
        public Vector3 AimPosition
        {
            get
            {
                ResolveComponents();
                return body != null ? body.position : transform.position + Vector3.up * Mathf.Max(0f, BodyLocalY);
            }
        }

        public bool IsActive => gameObject.activeInHierarchy;
        public float BodyLocalY => BodyView.BodyCenterLocalY();
        public Color CurrentBodyColor => BodyView.CurrentBodyColor;
        public EnemyHitbox Hurtbox
        {
            get
            {
                ResolveComponents();
                return hurtbox;
            }
        }

        public EnemyHitbox ContactHitbox
        {
            get
            {
                ResolveComponents();
                return contactHitbox;
            }
        }

        public Collider ContactHitboxCollider => ContactHitbox != null ? ContactHitbox.Collider : null;
        public EnemyHitbox MovementBody
        {
            get
            {
                ResolveComponents();
                return movementBody;
            }
        }

        public Collider MovementBodyCollider => MovementBody != null ? MovementBody.Collider : null;
        public Vector3 MovementBodyCenter => MovementBody != null ? MovementBody.WorldCenter : transform.position;
        public float MovementBodyRadius => MovementBody != null ? MovementBody.WorldRadius : 0f;
        public Vector3 HorizontalForward => EnemyMovementRules.HorizontalForward(transform.rotation, Vector3.forward);
        public float MotionVisualTargetSpeed01 => BodyView.MotionVisualTargetSpeed01;
        public bool IsIdleHoverActive { get; private set; }
        public GameObject RuntimeModelInstance => runtimeModel.Instance;
        public bool HasVisibleFallbackBody => runtimeModel.HasVisibleFallbackRenderer;
        public AnimationClip RuntimeAnimationClip => animancerDriver.CurrentClip;
        public float RuntimeAnimationSpeed => animancerDriver.CurrentSpeed;
        public bool RuntimeAnimationPaused => animancerDriver.IsPlaybackPaused;

        private void Awake()
        {
            ResolveComponents();
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            ResolveHitboxes();
            EnemyHitboxViewDriver.DrawReachMarkers(transform, movementBody, contactHitbox);
        }
#endif

        private ActorBodyView BodyView
        {
            get
            {
                ResolveComponents();
                return actorBody;
            }
        }

        public void Prepare(GameConfig config, Vector3 position)
        {
            var profile = new EnemyBodyProfile();
            profile.ApplyDefaultsFromGameConfig(config);
            Prepare(profile, position);
        }

        public void Prepare(EnemyBodyProfile bodyProfile, Vector3 position)
        {
            Prepare(bodyProfile, null, null, position);
        }

        public void Prepare(EnemyBodyProfile bodyProfile, DamageFeedbackVisualConfig feedbackConfig, Vector3 position)
        {
            Prepare(bodyProfile, null, feedbackConfig, position);
        }

        public void Prepare(EnemyBodyProfile bodyProfile, EnemyLocomotionProfile locomotionProfile, DamageFeedbackVisualConfig feedbackConfig, Vector3 position)
        {
            Prepare(bodyProfile, locomotionProfile, feedbackConfig, position, null);
        }

        public void Prepare(EnemyDefinition definition, DamageFeedbackVisualConfig feedbackConfig, Vector3 position)
        {
            Prepare(
                definition != null ? definition.body : null,
                definition != null ? definition.locomotion : null,
                feedbackConfig,
                position,
                definition);
        }

        private void Prepare(EnemyBodyProfile bodyProfile, EnemyLocomotionProfile locomotionProfile, DamageFeedbackVisualConfig feedbackConfig, Vector3 position, EnemyDefinition definition)
        {
            ResolveComponents();
            transform.position = position;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            StopIdleHoverAndGround();
            if (bodyScaleRoot != null && bodyScaleRoot != transform)
            {
                bodyScaleRoot.localPosition = Vector3.zero;
                bodyScaleRoot.localScale = Vector3.one;
            }

            if (body != null)
            {
                body.localScale = Vector3.one;
            }

            ConfigureHitboxes(bodyProfile);
            ApplyDefinitionVisuals(definition);
            gameObject.SetActive(true);
            Tween.StopAll(this);
            motionVisualsSuspended = false;
            ResetMotionVisuals();
            BodyView.CaptureBaseColor();
            BodyView.ClearBodyPropertyBlock();
            BodyView.ResetHealthBarVisibility();
            SetHealthNormalized(1f);
            ApplyLocomotionIdle(locomotionProfile, feedbackConfig);
        }

        public void Prepare(GameConfig config, DamageFeedbackVisualConfig feedbackConfig, Vector3 position)
        {
            var profile = new EnemyBodyProfile();
            profile.ApplyDefaultsFromGameConfig(config);
            Prepare(profile, null, feedbackConfig, position);
        }

        public void BeginSpawnEmergence(float startBodyLocalY)
        {
            ResolveComponents();
            if (bodyScaleRoot != null && body != null)
            {
                Tween.StopAll(bodyScaleRoot);
                IsIdleHoverActive = false;
                Vector3 position = bodyScaleRoot.localPosition;
                position.y = startBodyLocalY - BodyView.BodyCenterOffsetFromScaleRoot();
                bodyScaleRoot.localPosition = position;
                bodyScaleRoot.localScale = Vector3.one;
            }

            motionVisualsSuspended = true;
            SetHealthBarVisible(false);
        }

        public void ApplySpawnEmergence(float normalizedEmerge, float startBodyLocalY, float endBodyLocalY, EaseSettings ease)
        {
            ResolveComponents();
            if (bodyScaleRoot == null || body == null)
            {
                return;
            }

            Vector3 position = bodyScaleRoot.localPosition;
            position.y = EnemySpawnIndicatorRules.BodyLocalY(normalizedEmerge, startBodyLocalY, endBodyLocalY, ease) - BodyView.BodyCenterOffsetFromScaleRoot();
            bodyScaleRoot.localPosition = position;
        }

        public void CompleteSpawnEmergence(float bodyLocalY)
        {
            ResolveComponents();
            if (bodyScaleRoot != null && body != null)
            {
                Vector3 position = bodyScaleRoot.localPosition;
                position.y = bodyLocalY - BodyView.BodyCenterOffsetFromScaleRoot();
                bodyScaleRoot.localPosition = position;
            }

            motionVisualsSuspended = false;
            ResetMotionVisuals();
            SetHealthNormalized(1f);
        }

        public void ApplyLocomotionIdle(EnemyLocomotionProfile locomotionProfile, DamageFeedbackVisualConfig feedbackConfig)
        {
            ResolveComponents();
            this.locomotionProfile = locomotionProfile;
            EnemyLocomotionMode mode = locomotionProfile != null
                ? locomotionProfile.mode
                : EnemyLocomotionMode.Grounded;
            if (mode == EnemyLocomotionMode.Floating)
            {
                StartIdleHover(locomotionProfile, feedbackConfig);
                return;
            }

            StopIdleHoverAndGround();
        }

        public void StartIdleHover(DamageFeedbackVisualConfig feedbackConfig)
        {
            StartIdleHover(null, feedbackConfig);
        }

        public void StartIdleHover(EnemyLocomotionProfile locomotionProfile, DamageFeedbackVisualConfig feedbackConfig)
        {
            ResolveComponents();
            if (bodyScaleRoot == null || body == null)
            {
                return;
            }

            Tween.StopAll(bodyScaleRoot);
            VisualTweenUtility.LocalPositionY(
                bodyScaleRoot,
                HoverHeight(locomotionProfile, feedbackConfig),
                Mathf.Max(0.01f, HoverSeconds(locomotionProfile, feedbackConfig)),
                HoverEase(locomotionProfile, feedbackConfig),
                cycles: -1,
                cycleMode: CycleMode.Yoyo);
            IsIdleHoverActive = true;
        }

        public void Move(Vector3 direction, float speed, float deltaTime)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.0000001f)
            {
                return;
            }

            Vector3 position = transform.position + direction.normalized * speed * deltaTime;
            transform.position = position;
        }

        public void TurnTowards(Vector3 direction, float degreesPerSecond, float deltaTime)
        {
            transform.rotation = EnemyMovementRules.RotateTowardsHorizontal(transform.rotation, direction, degreesPerSecond, deltaTime);
        }

        public void MoveDisplacement(Vector3 displacement)
        {
            displacement.y = 0f;
            if (displacement.sqrMagnitude <= 0.0000001f)
            {
                return;
            }

            Vector3 position = transform.position + displacement;
            transform.position = position;
        }

        public void SetHeight(float y)
        {
            Vector3 position = transform.position;
            position.y = Mathf.Max(0f, y);
            transform.position = position;
        }

        public void SetMotionVisualSpeed(float normalizedSpeed)
        {
            BodyView.SetMotionVisualSpeed(normalizedSpeed);
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
            if (body == null || motionVisualsSuspended)
            {
                return;
            }

            if (motionConfig == null || !motionConfig.enabled)
            {
                BodyView.BodyScaleRoot.localScale = Vector3.one;
                BodyView.ApplyGroundTilt(Vector3.up, 0f, 0f, deltaTime);
                animancerDriver.TickEnemy(animationProfile, LocomotionMode(), MotionVisualTargetSpeed01, deltaTime);
                return;
            }

            BodyView.TickMotionVisuals(
                deltaTime,
                motionConfig.enemyIdle.amplitude,
                motionConfig.enemyIdle.secondsPerCycle,
                motionConfig.enemyIdle.xzCompensation,
                motionConfig.enemyMove.amplitude,
                motionConfig.enemyMove.secondsPerCycle,
                motionConfig.enemyMove.xzCompensation,
                motionConfig.speedSmoothingSharpness,
                motionConfig.speedSmoothingEase);
            Vector3 groundNormal = TryProbeGroundNormal(motionConfig, out Vector3 probedNormal)
                ? probedNormal
                : Vector3.up;
            BodyView.ApplyGroundTilt(
                groundNormal,
                motionConfig.inclineTiltEnabled ? motionConfig.enemyInclineTiltMaxDegrees : 0f,
                motionConfig.inclineTiltSmoothingSharpness,
                deltaTime);
            animancerDriver.TickEnemy(animationProfile, LocomotionMode(), MotionVisualTargetSpeed01, deltaTime);
        }

        public void ResetMotionVisuals()
        {
            ResolveComponents();
            BodyView.ResetMotionVisuals(resetScaleRootPosition: false);
        }

        public void SetHealthNormalized(float value)
        {
            BodyView.SetHealthNormalized(value);
        }

        public void PlayHit(float seconds)
        {
            PlayHit(seconds, Color.white, Mathf.Min(0.08f, seconds));
        }

        public void PlayHit(float seconds, Color flashColor, float flashSeconds)
        {
            PlayHit(seconds, flashColor, flashSeconds, 4f, EaseSettings.OutQuad, 1.25f, EaseSettings.OutQuad);
        }

        public void PlayHit(float seconds, DamageFeedbackVisualConfig feedbackConfig)
        {
            PlayHit(seconds, feedbackConfig, null);
        }

        public void PlayHit(float seconds, DamageFeedbackVisualConfig feedbackConfig, ActorMotionVisualConfig motionConfig)
        {
            ActorScalePulseSettings pulse = motionConfig != null
                ? motionConfig.enemyHitPulse
                : new ActorScalePulseSettings(
                    true,
                    seconds,
                    feedbackConfig != null ? feedbackConfig.enemyHitPulseScale : 1.25f,
                    feedbackConfig != null ? feedbackConfig.enemyHitPulseEase : EaseSettings.OutQuad);

            PlayHit(
                seconds,
                feedbackConfig != null ? feedbackConfig.enemyFlashColor : Color.white,
                feedbackConfig != null ? feedbackConfig.enemyFlashSeconds : Mathf.Min(0.08f, seconds),
                feedbackConfig != null ? feedbackConfig.enemyFlashEmissionStrength : 4f,
                feedbackConfig != null ? feedbackConfig.enemyFlashEase : EaseSettings.OutQuad,
                pulse.enabled ? pulse.scaleMultiplier : 1f,
                pulse.enabled ? pulse.ease : EaseSettings.OutQuad,
                pulse.enabled ? pulse.seconds : 0f);
        }

        public void PlayHit(
            float seconds,
            Color flashColor,
            float flashSeconds,
            float flashEmissionStrength,
            EaseSettings flashEase,
            float pulseScale,
            EaseSettings pulseEase)
        {
            PlayHit(seconds, flashColor, flashSeconds, flashEmissionStrength, flashEase, pulseScale, pulseEase, seconds);
        }

        public void PlayHit(
            float seconds,
            Color flashColor,
            float flashSeconds,
            float flashEmissionStrength,
            EaseSettings flashEase,
            float pulseScale,
            EaseSettings pulseEase,
            float pulseSeconds)
        {
            ResolveComponents();
            if (!motionVisualsSuspended)
            {
                BodyView.StartPulse(
                    Mathf.Max(0f, pulseSeconds),
                    Mathf.Max(1f, pulseScale),
                    pulseEase);
            }

            PlayFlash(flashColor, flashSeconds, flashEase, flashEmissionStrength);
        }

        public void PlayDeath(float seconds, Action completed)
        {
            PlayDeath(seconds, EaseSettings.InBack, completed);
        }

        public void PlayDeath(float seconds, EaseSettings ease, Action completed)
        {
            PlayDeath(seconds, ease, null, completed);
        }

        public void PlayDeath(float seconds, EaseSettings ease, DamageFeedbackVisualConfig feedbackConfig, Action completed)
        {
            if (body != null)
            {
                Tween.StopAll(BodyView.DeathScaleTarget());
            }

            motionVisualsSuspended = true;
            FadeHealthBarForDeath(feedbackConfig);
            if (!Application.isPlaying)
            {
                gameObject.SetActive(false);
                completed?.Invoke();
                return;
            }

            Transform deathTarget = BodyView.DeathScaleTarget();
            VisualTweenUtility.Scale(deathTarget, Vector3.zero, seconds, ease)
                .OnComplete(deathTarget, _ => completed?.Invoke(), warnIfTargetDestroyed: false);
        }

        public void Deactivate()
        {
            Tween.StopAll(this);
            StopIdleHoverAndGround();
            if (body != null)
            {
                Tween.StopAll(BodyView.DeathScaleTarget());
            }

            Tween.StopAll(transform);
            BodyView.ClearBodyPropertyBlock();
            motionVisualsSuspended = false;
            ResetMotionVisuals();
            BodyView.ResetHealthBarVisibility();
            gameObject.SetActive(false);
        }

        private void StopIdleHoverAndGround()
        {
            ResolveComponents();
            if (bodyScaleRoot != null)
            {
                Tween.StopAll(bodyScaleRoot);
            }

            IsIdleHoverActive = false;
            if (bodyScaleRoot != null && bodyScaleRoot != transform)
            {
                BodyView.SetBodyScaleRootLocalY(0f);
            }
        }

        private static float HoverHeight(EnemyLocomotionProfile locomotionProfile, DamageFeedbackVisualConfig feedbackConfig)
        {
            if (locomotionProfile != null)
            {
                return Mathf.Max(0f, locomotionProfile.hoverHeight);
            }

            return feedbackConfig != null ? feedbackConfig.enemyHoverHeight : 0.15f;
        }

        private static float HoverSeconds(EnemyLocomotionProfile locomotionProfile, DamageFeedbackVisualConfig feedbackConfig)
        {
            if (locomotionProfile != null)
            {
                return Mathf.Max(0.01f, locomotionProfile.hoverSeconds);
            }

            return feedbackConfig != null ? feedbackConfig.enemyHoverSeconds : 0.85f;
        }

        private static EaseSettings HoverEase(EnemyLocomotionProfile locomotionProfile, DamageFeedbackVisualConfig feedbackConfig)
        {
            if (locomotionProfile != null)
            {
                return locomotionProfile.hoverEase;
            }

            return feedbackConfig != null ? feedbackConfig.enemyHoverEase : EaseSettings.InOutSine;
        }

        private void PlayFlash(Color flashColor, float seconds, EaseSettings ease, float emissionStrength)
        {
            BodyView.PlayFlash(flashColor, seconds, ease, emissionStrength);
        }

        private void ApplyDefinitionVisuals(EnemyDefinition definition)
        {
            animationProfile = definition != null ? definition.animation : null;
            locomotionProfile = definition != null ? definition.locomotion : null;
            runtimeModel.Apply(bodyScaleRoot, definition != null ? definition.worldPrefab : null, definition?.modelTransform, body);
            BodyView.SetExternalBodyRenderers(runtimeModel.ModelRenderers);
            animancerDriver.Configure(runtimeModel.Instance);
        }

        private EnemyLocomotionMode LocomotionMode()
        {
            return locomotionProfile != null ? locomotionProfile.mode : EnemyLocomotionMode.Grounded;
        }

        private bool TryProbeGroundNormal(ActorMotionVisualConfig motionConfig, out Vector3 groundNormal)
        {
            groundNormal = Vector3.up;
            float probeDistance = motionConfig != null ? motionConfig.enemyInclineGroundProbeDistance : 0.8f;
            if (probeDistance <= 0f)
            {
                return false;
            }

            Vector3 origin = transform.position + Vector3.up * 0.25f;
            float distance = 0.25f + probeDistance;
            if (!Physics.Raycast(
                    origin,
                    Vector3.down,
                    out RaycastHit hit,
                    distance,
                    GameLayers.EnvironmentMaskExcludingGameplay,
                    QueryTriggerInteraction.Ignore))
            {
                return false;
            }

            groundNormal = hit.normal.sqrMagnitude > 0.000001f ? hit.normal.normalized : Vector3.up;
            return true;
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

            actorBody.ResolveComponents();
            body = actorBody.Body;
            bodyScaleRoot = actorBody.BodyScaleRoot;
            healthBar = actorBody.HealthBar;
            bodyRenderer = body != null ? body.GetComponent<Renderer>() : null;
            ResolveHitboxes();
            DisableVisualBodyColliders();

            if (body == null)
            {
                Transform bodyChild = transform.Find("Body");
                if (bodyChild != null)
                {
                    body = bodyChild;
                }
            }

            if (bodyRenderer == null)
            {
                bodyRenderer = body != null ? body.GetComponent<Renderer>() : GetComponentInChildren<Renderer>();
            }

            if (body == null && bodyRenderer != null)
            {
                body = bodyRenderer.transform;
            }

            if (healthBar == null)
            {
                healthBar = GetComponentInChildren<HealthBarView>();
            }

            actorBody.ResolveComponents();
            ResolveHitboxes();
        }

        private void ConfigureHitboxes(EnemyBodyProfile profile)
        {
            EnemyHitboxViewDriver.Configure(transform, profile, ref hurtbox, ref contactHitbox, ref movementBody);
        }

        private void ResolveHitboxes()
        {
            EnemyHitboxViewDriver.Resolve(transform, ref hurtbox, ref contactHitbox, ref movementBody);
        }

        private void DisableVisualBodyColliders()
        {
            EnemyHitboxViewDriver.DisableVisualBodyColliders(body);
        }

        private void SetHealthBarVisible(bool visible)
        {
            BodyView.SetHealthBarVisible(visible);
        }

        private void FadeHealthBarForDeath(DamageFeedbackVisualConfig feedbackConfig)
        {
            BodyView.FadeHealthBar(
                feedbackConfig != null ? feedbackConfig.enemyHealthBarDeathFadeSeconds : 0.07f,
                feedbackConfig != null ? feedbackConfig.enemyHealthBarDeathFadeEase : EaseSettings.OutQuad);
        }
    }
}
