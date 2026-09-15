using PrimeTween;
using UnityEngine;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;

namespace TheCircussyOne.Visuals
{
    public sealed class ActorBodyView : MonoBehaviour
    {
        [SerializeField] private Renderer bodyRenderer;
        [SerializeField] private Transform body;
        [SerializeField] private Transform bodyScaleRoot;
        [SerializeField] private HealthBarView healthBar;
        [SerializeField] private Transform groundShadow;

        private readonly ActorBodyMaterialFlash materialFlash = new();
        private readonly ActorBodyMotionDriver motionDriver = new();
        private readonly ActorGroundShadowProjector groundShadowProjector = new();

        public Transform Body
        {
            get
            {
                ResolveComponents();
                return body != null ? body : transform;
            }
        }

        public Transform BodyScaleRoot
        {
            get
            {
                ResolveComponents();
                return bodyScaleRoot != null ? bodyScaleRoot : transform;
            }
        }

        public HealthBarView HealthBar
        {
            get
            {
                ResolveComponents();
                return healthBar;
            }
        }

        public Transform GroundShadow
        {
            get
            {
                ResolveComponents();
                return groundShadow;
            }
        }

        public float MotionVisualTargetSpeed01 => motionDriver.TargetSpeed01;
        public Color CurrentBodyColor => materialFlash.CurrentBodyColor;

        private void Awake()
        {
            ResolveComponents();
        }

        public void ResolveComponents()
        {
            if (body == null)
            {
                Transform bodyChild = transform.Find("Body Scale Root/Body") ?? transform.Find("Body");
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
                healthBar = GetComponentInChildren<HealthBarView>(includeInactive: true);
            }

            if (groundShadow == null)
            {
                Transform shadowChild = transform.Find("Ground Shadow");
                if (shadowChild != null)
                {
                    groundShadow = shadowChild;
                }
            }

            EnsureBodyScaleRoot();
            materialFlash.SetTargets(bodyRenderer, body, transform);
            groundShadowProjector.CaptureBase(groundShadow);
        }

        public void CaptureBaseColor()
        {
            ResolveComponents();
            materialFlash.CaptureBaseColor();
        }

        public void ClearBodyPropertyBlock()
        {
            ResolveComponents();
            materialFlash.ClearPropertyBlocks();
        }

        public void SetExternalBodyRenderers(Renderer[] renderers)
        {
            ResolveComponents();
            materialFlash.SetOverrideRenderers(renderers);
        }

        public void PlayFlash(Color flashColor, float seconds, EaseSettings ease, float emissionStrength)
        {
            ResolveComponents();
            Tween.StopAll(this);
            CaptureBaseColor();
            materialFlash.BeginFlash(flashColor, ease, emissionStrength);

            if (!Application.isPlaying)
            {
                return;
            }

            if (seconds <= 0f)
            {
                ClearBodyPropertyBlock();
                return;
            }

            Tween.Custom(this, 0f, 1f, seconds, static (view, progress) =>
            {
                view.materialFlash.ApplyFlashProgress(progress);
            }, Ease.Linear)
                .OnComplete(this, static view => view.ClearBodyPropertyBlock(), warnIfTargetDestroyed: false);
        }

        public void SetMotionVisualSpeed(float normalizedSpeed, bool immediate = false)
        {
            motionDriver.SetSpeed(normalizedSpeed, immediate);
        }

        public void StartPulse(float seconds, float scaleMultiplier, EaseSettings ease)
        {
            motionDriver.StartPulse(seconds, scaleMultiplier, ease);
        }

        public void StartPulse(ActorScalePulseSettings pulse)
        {
            motionDriver.StartPulse(pulse);
        }

        public void SetJumpHoldStretch(
            bool jumpHeld,
            bool rising,
            bool airborne,
            bool enabled,
            float maxSeconds,
            float releaseSeconds,
            float deltaTime)
        {
            motionDriver.SetJumpHoldStretch(
                jumpHeld,
                rising,
                airborne,
                enabled,
                maxSeconds,
                releaseSeconds,
                deltaTime);
        }

        public void TickMotionVisuals(
            float deltaTime,
            float idleAmplitude,
            float idleSecondsPerCycle,
            float idleXzCompensation,
            float moveAmplitude,
            float moveSecondsPerCycle,
            float moveXzCompensation,
            float speedSmoothingSharpness,
            EaseSettings speedSmoothingEase,
            float jumpHoldXzScale = 1f,
            float jumpHoldYScale = 1f,
            EaseSettings jumpHoldEase = default)
        {
            ResolveComponents();
            if (bodyScaleRoot == null)
            {
                return;
            }

            bodyScaleRoot.localScale = motionDriver.TickScale(
                deltaTime,
                idleAmplitude,
                idleSecondsPerCycle,
                idleXzCompensation,
                moveAmplitude,
                moveSecondsPerCycle,
                moveXzCompensation,
                speedSmoothingSharpness,
                speedSmoothingEase,
                jumpHoldXzScale,
                jumpHoldYScale,
                jumpHoldEase);
        }

        public void ApplyGroundTilt(Vector3 worldGroundNormal, float maxDegrees, float smoothingSharpness, float deltaTime)
        {
            ResolveComponents();
            if (bodyScaleRoot == null || bodyScaleRoot == transform)
            {
                return;
            }

            Quaternion target = Quaternion.identity;
            if (maxDegrees > 0f && worldGroundNormal.sqrMagnitude > 0.000001f)
            {
                Vector3 localNormal = transform.InverseTransformDirection(worldGroundNormal.normalized);
                if (localNormal.sqrMagnitude > 0.000001f)
                {
                    target = Quaternion.FromToRotation(Vector3.up, localNormal.normalized);
                    target = Quaternion.RotateTowards(Quaternion.identity, target, Mathf.Max(0f, maxDegrees));
                }
            }

            float weight = smoothingSharpness <= 0f || deltaTime <= 0f
                ? 1f
                : GameEasing.SmoothingWeight(smoothingSharpness, deltaTime, EaseSettings.Exponential);
            bodyScaleRoot.localRotation = Quaternion.Slerp(bodyScaleRoot.localRotation, target, Mathf.Clamp01(weight));
        }

        public void ResetMotionVisuals(bool resetScaleRootPosition)
        {
            ResolveComponents();
            motionDriver.Reset();
            if (bodyScaleRoot != null && bodyScaleRoot != transform)
            {
                if (resetScaleRootPosition)
                {
                    bodyScaleRoot.localPosition = Vector3.zero;
                }

                bodyScaleRoot.localScale = Vector3.one;
                bodyScaleRoot.localRotation = Quaternion.identity;
            }
        }

        public void ConfigureHealthBar(
            float width,
            Vector3 localOffset,
            Color backgroundColor,
            Color fillColor,
            float backgroundThickness,
            float fillThickness,
            bool visibleThroughGeometry,
            int sortingOrderBase,
            int renderQueueBase)
        {
            ResolveComponents();
            healthBar?.Configure(
                width,
                localOffset,
                backgroundColor,
                fillColor,
                backgroundThickness,
                fillThickness,
                visibleThroughGeometry,
                sortingOrderBase,
                renderQueueBase);
        }

        public void SetHealthNormalized(float value, bool enabled = true)
        {
            ResolveComponents();
            if (healthBar == null)
            {
                return;
            }

            if (!enabled)
            {
                healthBar.gameObject.SetActive(false);
                return;
            }

            float normalized = Mathf.Clamp01(value);
            healthBar.SetNormalized(normalized);
            healthBar.gameObject.SetActive(normalized < 0.999f);
        }

        public void SetHealthBarVisible(bool visible)
        {
            ResolveComponents();
            if (healthBar != null)
            {
                healthBar.gameObject.SetActive(visible);
            }
        }

        public void ResetHealthBarVisibility()
        {
            ResolveComponents();
            healthBar?.ResetVisibility();
        }

        public void FadeHealthBar(float seconds, EaseSettings ease)
        {
            ResolveComponents();
            healthBar?.FadeOut(seconds, ease);
        }

        public void UpdateGroundProjectedShadow(float rootWorldY, float referenceJumpHeight = 3f)
        {
            ResolveComponents();
            groundShadowProjector.Project(groundShadow, rootWorldY, referenceJumpHeight);
        }

        public float BodyCenterLocalY()
        {
            ResolveComponents();
            if (bodyScaleRoot == null || body == null)
            {
                return body != null ? body.localPosition.y : 0f;
            }

            return bodyScaleRoot.localPosition.y + BodyCenterOffsetFromScaleRoot();
        }

        public float BodyCenterOffsetFromScaleRoot()
        {
            ResolveComponents();
            return body != null ? body.localPosition.y : 1f;
        }

        public void SetBodyScaleRootLocalY(float localY)
        {
            ResolveComponents();
            if (bodyScaleRoot == null)
            {
                return;
            }

            Vector3 position = bodyScaleRoot.localPosition;
            position.y = localY;
            bodyScaleRoot.localPosition = position;
        }

        public Transform DeathScaleTarget()
        {
            ResolveComponents();
            return bodyScaleRoot != null ? bodyScaleRoot : body != null ? body : transform;
        }

        private void EnsureBodyScaleRoot()
        {
            if (body == null || body == transform)
            {
                bodyScaleRoot = transform;
                return;
            }

            if (bodyScaleRoot != null)
            {
                return;
            }

            if (body.parent != null && body.parent.name == "Body Scale Root")
            {
                bodyScaleRoot = body.parent;
                return;
            }

            Transform found = transform.Find("Body Scale Root");
            if (found != null)
            {
                bodyScaleRoot = found;
                return;
            }

            float baseOffset = EstimateBodyBaseOffset();
            Vector3 bodyLocalPosition = body.localPosition;
            Quaternion bodyLocalRotation = body.localRotation;
            Vector3 bodyLocalScale = body.localScale;

            var pivotObject = new GameObject("Body Scale Root");
            Transform pivot = pivotObject.transform;
            pivot.SetParent(transform, false);
            pivot.localPosition = new Vector3(bodyLocalPosition.x, bodyLocalPosition.y - baseOffset, bodyLocalPosition.z);
            pivot.localRotation = Quaternion.identity;
            pivot.localScale = Vector3.one;
            pivot.gameObject.layer = gameObject.layer;

            body.SetParent(pivot, false);
            body.localPosition = new Vector3(0f, baseOffset, 0f);
            body.localRotation = bodyLocalRotation;
            body.localScale = bodyLocalScale;
            bodyScaleRoot = pivot;
        }

        private float EstimateBodyBaseOffset()
        {
            MeshFilter meshFilter = body != null ? body.GetComponent<MeshFilter>() : null;
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                return Mathf.Max(0.01f, -meshFilter.sharedMesh.bounds.min.y * Mathf.Abs(body.localScale.y));
            }

            return Mathf.Max(0.01f, body != null ? body.localPosition.y : 1f);
        }
    }
}
