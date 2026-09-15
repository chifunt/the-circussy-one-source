using PrimeTween;
using UnityEngine;
using TheCircussyOne.Content;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;

namespace TheCircussyOne.Visuals
{
    public sealed class PickupView : MonoBehaviour
    {
        private const float AttractTargetHeight = 0.8f;

        [SerializeField] private Renderer bodyRenderer;
        [SerializeField] private MeshFilter bodyMeshFilter;
        [SerializeField] private ParticleSystem attractTrail;
        [SerializeField] private Transform visualRoot;
        private Transform attractTarget;
        private Vector3 attractStartPosition;
        private Vector3 logicalPosition;
        private Vector3 baseSpawnScale = Vector3.one;
        private Vector3 spawnScale = Vector3.one;
        private float visualRestLocalY;
        private DamageFeedbackVisualConfig feedbackVisualConfig;
        private VfxVisualConfig vfxConfig;
        private MaterialPropertyBlock propertyBlock;
        private bool hasSpawnScale;
        private Mesh defaultBodyMesh;
        private static Mesh heartMesh;

        public Vector3 Position => IsAttracting ? transform.position : logicalPosition;
        public bool IsActive => gameObject.activeInHierarchy;
        public bool IsAttracting { get; private set; }
        public bool HasReachedPlayer { get; private set; }
        public bool IsCollectable { get; private set; } = true;
        public bool IsAttractTrailPlaying => attractTrail != null && attractTrail.isPlaying;
        public float VisualRestLocalY => visualRestLocalY;

        private void Awake()
        {
            ResolveComponents();
            CaptureSpawnScale();
        }

        public void Prepare(GameConfig config, Vector3 position)
        {
            Prepare(config, null, position);
        }

        public void Prepare(GameConfig config, DamageFeedbackVisualConfig feedbackConfig, Vector3 position)
        {
            Prepare(config, feedbackConfig, null, position);
        }

        public void Prepare(GameConfig config, DamageFeedbackVisualConfig feedbackConfig, VfxVisualConfig vfxConfig, Vector3 position)
        {
            Prepare(config, feedbackConfig, vfxConfig, PickupDropFrame.Immediate(position), null, 1f);
        }

        public void Prepare(GameConfig config, DamageFeedbackVisualConfig feedbackConfig, VfxVisualConfig vfxConfig, Vector3 position, Color? tint, float visualScale)
        {
            Prepare(config, feedbackConfig, vfxConfig, PickupDropFrame.Immediate(position), tint, visualScale);
        }

        public void Prepare(GameConfig config, DamageFeedbackVisualConfig feedbackConfig, VfxVisualConfig vfxConfig, Vector3 position, Color? tint, float visualScale, Color? emissionTint, float emissionStrength)
        {
            Prepare(config, feedbackConfig, vfxConfig, PickupDropFrame.Immediate(position), tint, visualScale, emissionTint, emissionStrength);
        }

        public void Prepare(GameConfig config, DamageFeedbackVisualConfig feedbackConfig, VfxVisualConfig vfxConfig, PickupDropFrame dropFrame, Color? tint, float visualScale)
        {
            Prepare(config, feedbackConfig, vfxConfig, dropFrame, tint, visualScale, null, 0f);
        }

        public void Prepare(
            GameConfig config,
            DamageFeedbackVisualConfig feedbackConfig,
            VfxVisualConfig vfxConfig,
            PickupDropFrame dropFrame,
            Color? tint,
            float visualScale,
            Color? emissionTint,
            float emissionStrength)
        {
            Prepare(config, feedbackConfig, vfxConfig, dropFrame, tint, visualScale, emissionTint, emissionStrength, PickupVisualShape.Gem);
        }

        public void Prepare(
            GameConfig config,
            DamageFeedbackVisualConfig feedbackConfig,
            VfxVisualConfig vfxConfig,
            PickupDropFrame dropFrame,
            Color? tint,
            float visualScale,
            Color? emissionTint,
            float emissionStrength,
            PickupVisualShape visualShape)
        {
            ResolveComponents();
            CaptureSpawnScale();
            ApplyVisualShape(visualShape);
            feedbackVisualConfig = feedbackConfig;
            this.vfxConfig = vfxConfig;
            spawnScale = baseSpawnScale * Mathf.Max(0.05f, visualScale);
            visualRestLocalY = EstimateVisualRestLocalY();
            Tween.StopAll(this);
            Tween.StopAll(transform);
            Tween.StopAll(VisualTransform);
            attractTarget = null;
            IsAttracting = false;
            HasReachedPlayer = false;
            logicalPosition = dropFrame.LandingPosition;
            float collectDelaySeconds = Mathf.Max(
                feedbackConfig != null ? feedbackConfig.pickupSpawnDelaySeconds : 0f,
                dropFrame.CollectDelaySeconds);
            IsCollectable = collectDelaySeconds <= 0f;
            transform.position = dropFrame.StartPosition;
            SetVisualLocalPosition(VisualRestLocalPosition);
            SetVisualScale(Vector3.zero);
            gameObject.SetActive(true);
            ClearMaterialOverrides();
            ApplyMaterialVisuals(tint, emissionTint, emissionStrength);
            StopAttractTrail(clear: true);
            VisualTweenUtility.Scale(
                VisualTransform,
                spawnScale,
                Mathf.Max(0f, feedbackConfig != null ? feedbackConfig.pickupSpawnSeconds : 0.2f),
                feedbackConfig != null ? feedbackConfig.pickupSpawnEase : EaseSettings.OutBack);

            if (Application.isPlaying && dropFrame.HasToss)
            {
                if (dropFrame.TossSeconds > 0f)
                {
                    Tween.Custom(this, 0f, 1f, dropFrame.TossSeconds, (view, progress) => view.UpdateDropToss(dropFrame, progress), Ease.Linear)
                        .OnComplete(this, static view => view.SettleDrop(), warnIfTargetDestroyed: false);
                }
                else
                {
                    SettleDrop();
                }
            }
            else
            {
                SettleDrop();
            }

            if (!IsCollectable && Application.isPlaying)
            {
                Tween.Custom(this, 0f, 1f, collectDelaySeconds, (view, progress) => { }, Ease.Linear)
                    .OnComplete(this, static view => view.IsCollectable = true, warnIfTargetDestroyed: false);
            }
            else if (!Application.isPlaying)
            {
                IsCollectable = true;
            }
        }

        public void BeginAttractTo(Transform target, float duration)
        {
            BeginAttractTo(target, duration, EaseSettings.InBack);
        }

        public void BeginAttractTo(Transform target, float duration, EaseSettings attractEase)
        {
            if (IsAttracting)
            {
                return;
            }

            Tween.StopAll(this);
            Tween.StopAll(transform);
            Tween.StopAll(VisualTransform);
            attractTarget = target;
            attractStartPosition = transform.position;
            IsAttracting = true;
            HasReachedPlayer = false;
            SetVisualLocalPosition(VisualRestLocalPosition);
            SetVisualScale(spawnScale);
            StartAttractTrail();

            if (!Application.isPlaying)
            {
                HasReachedPlayer = true;
                return;
            }

            Tween.Custom(this, 0f, 1f, duration, (view, progress) => view.UpdateAttract(GameEasing.Evaluate01(attractEase, progress)), Ease.Linear)
                .OnComplete(this, static view => view.HasReachedPlayer = true, warnIfTargetDestroyed: false);
        }

        public void CancelAttractToRest()
        {
            if (!IsAttracting && !HasReachedPlayer)
            {
                return;
            }

            Tween.StopAll(this);
            Tween.StopAll(transform);
            Tween.StopAll(VisualTransform);
            attractTarget = null;
            IsAttracting = false;
            HasReachedPlayer = false;
            transform.position = logicalPosition;
            SetVisualLocalPosition(VisualRestLocalPosition);
            SetVisualScale(spawnScale);
            StopAttractTrail(clear: true);
            StartIdleBob();
        }

        public void Deactivate()
        {
            Tween.StopAll(this);
            Tween.StopAll(transform);
            Tween.StopAll(VisualTransform);
            attractTarget = null;
            IsAttracting = false;
            HasReachedPlayer = false;
            IsCollectable = false;
            StopAttractTrail(clear: true);
            gameObject.SetActive(false);
        }

        private void UpdateAttract(float progress)
        {
            if (attractTarget == null)
            {
                HasReachedPlayer = true;
                return;
            }

            Vector3 targetPosition = attractTarget.position + Vector3.up * AttractTargetHeight;
            transform.position = Vector3.LerpUnclamped(attractStartPosition, targetPosition, progress);
        }

        private void UpdateDropToss(PickupDropFrame dropFrame, float progress)
        {
            float eased = GameEasing.Evaluate01(dropFrame.HorizontalEase, progress);
            Vector3 position = Vector3.LerpUnclamped(dropFrame.StartPosition, dropFrame.LandingPosition, eased);
            float arc = Mathf.Sin(Mathf.Clamp01(progress) * Mathf.PI) * dropFrame.ArcHeight;
            float bounce = 0f;
            if (dropFrame.BounceCount > 0 && dropFrame.BounceHeight > 0f)
            {
                float bouncePhase = Mathf.Clamp01(progress) * Mathf.PI * 2f * dropFrame.BounceCount;
                bounce = Mathf.Abs(Mathf.Sin(bouncePhase)) * dropFrame.BounceHeight * (1f - Mathf.Clamp01(progress));
            }

            position.y += arc + bounce;
            transform.position = position;
        }

        private void SettleDrop()
        {
            transform.position = logicalPosition;
            StartIdleBob();
        }

        private void StartIdleBob()
        {
            VisualTweenUtility.LocalPositionY(
                VisualTransform,
                visualRestLocalY + PickupIdleHeight,
                PickupIdleSeconds,
                PickupIdleEase,
                cycles: -1,
                cycleMode: CycleMode.Yoyo);
        }

        private void ClearMaterialOverrides()
        {
            if (bodyRenderer == null)
            {
                return;
            }

            bodyRenderer.SetPropertyBlock(null);
        }

        private void ApplyMaterialVisuals(Color? tint, Color? emissionTint, float emissionStrength)
        {
            if (bodyRenderer == null || (!tint.HasValue && !emissionTint.HasValue && emissionStrength <= 0f))
            {
                return;
            }

            propertyBlock ??= new MaterialPropertyBlock();
            bodyRenderer.GetPropertyBlock(propertyBlock);
            if (tint.HasValue)
            {
                ShaderVisualProperties.ApplyBaseColor(propertyBlock, tint.Value);
            }

            if (emissionTint.HasValue || emissionStrength > 0f)
            {
                ShaderVisualProperties.ApplyEmission(propertyBlock, emissionTint ?? tint ?? Color.black, emissionStrength);
            }

            bodyRenderer.SetPropertyBlock(propertyBlock);
        }

        private void StartAttractTrail()
        {
            ConfigureAttractTrail();
            if (attractTrail == null || vfxConfig == null || !vfxConfig.enabled || !vfxConfig.pickupAttractTrailEnabled)
            {
                return;
            }

            attractTrail.Play(withChildren: true);
        }

        private void StopAttractTrail(bool clear)
        {
            if (attractTrail == null)
            {
                return;
            }

            attractTrail.Stop(
                withChildren: true,
                clear ? ParticleSystemStopBehavior.StopEmittingAndClear : ParticleSystemStopBehavior.StopEmitting);
        }

        private void ConfigureAttractTrail()
        {
            if (attractTrail == null)
            {
                return;
            }

            var main = attractTrail.main;
            main.loop = true;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startColor = vfxConfig != null ? vfxConfig.pickupAttractTrailColor : new Color(0.42f, 0.9f, 1f, 0.72f);
            main.startLifetime = vfxConfig != null ? vfxConfig.pickupAttractTrailLifetime : 0.24f;
            main.startSize = vfxConfig != null ? vfxConfig.pickupAttractTrailStartSize : 0.075f;
            var emission = attractTrail.emission;
            emission.enabled = true;
            emission.rateOverTime = vfxConfig != null ? vfxConfig.pickupAttractTrailEmissionRate : 24f;
        }

        private void ResolveComponents()
        {
            if (visualRoot == null)
            {
                Transform visualChild = transform.Find("Visual Root");
                if (visualChild != null)
                {
                    visualRoot = visualChild;
                }
            }

            if (bodyRenderer == null)
            {
                bodyRenderer = GetComponentInChildren<Renderer>();
            }

            if (bodyMeshFilter == null && bodyRenderer != null)
            {
                bodyMeshFilter = bodyRenderer.GetComponent<MeshFilter>();
            }

            if (defaultBodyMesh == null && bodyMeshFilter != null)
            {
                defaultBodyMesh = bodyMeshFilter.sharedMesh;
            }

            if (attractTrail == null)
            {
                Transform trailChild = transform.Find("XP Attract Trail");
                if (trailChild != null)
                {
                    attractTrail = trailChild.GetComponentInChildren<ParticleSystem>(includeInactive: true);
                }
            }
        }

        private void CaptureSpawnScale()
        {
            if (hasSpawnScale || VisualTransform.localScale == Vector3.zero)
            {
                return;
            }

            baseSpawnScale = VisualTransform.localScale;
            spawnScale = baseSpawnScale;
            hasSpawnScale = true;
        }

        private Transform VisualTransform => visualRoot != null ? visualRoot : transform;
        private Vector3 VisualRestLocalPosition => Vector3.up * visualRestLocalY;

        private float PickupIdleHeight => feedbackVisualConfig != null ? feedbackVisualConfig.pickupIdleHeight : 0.35f;
        private float PickupIdleSeconds => Mathf.Max(0.01f, feedbackVisualConfig != null ? feedbackVisualConfig.pickupIdleSeconds : 0.65f);
        private EaseSettings PickupIdleEase => feedbackVisualConfig != null ? feedbackVisualConfig.pickupIdleEase : EaseSettings.InOutSine;

        private float EstimateVisualRestLocalY()
        {
            if (bodyRenderer != null && bodyRenderer.TryGetComponent(out MeshFilter meshFilter) && meshFilter.sharedMesh != null)
            {
                float bottom = meshFilter.sharedMesh.bounds.min.y * Mathf.Abs(spawnScale.y);
                return Mathf.Max(0f, -bottom) + PickupVisualGroundPadding;
            }

            return Mathf.Max(0f, feedbackVisualConfig != null ? feedbackVisualConfig.pickupCollisionRadius : 0.16f)
                + PickupVisualGroundPadding;
        }

        private float PickupVisualGroundPadding => Mathf.Max(0f, feedbackVisualConfig != null ? feedbackVisualConfig.pickupVisualGroundPadding : 0f);

        private void SetVisualScale(Vector3 value)
        {
            VisualTransform.localScale = value;
        }

        private void SetVisualLocalPosition(Vector3 value)
        {
            VisualTransform.localPosition = value;
        }

        private void ApplyVisualShape(PickupVisualShape visualShape)
        {
            if (bodyMeshFilter == null)
            {
                return;
            }

            bodyMeshFilter.sharedMesh = visualShape == PickupVisualShape.Heart
                ? GetHeartMesh()
                : defaultBodyMesh;
        }

        private static Mesh GetHeartMesh()
        {
            if (heartMesh != null)
            {
                return heartMesh;
            }

            const int Segments = 32;
            const float Depth = 0.18f;
            var vertices = new Vector3[(Segments + 1) * 2];
            vertices[0] = new Vector3(0f, 0.12f, -Depth);
            vertices[Segments + 1] = new Vector3(0f, 0.12f, Depth);
            for (int i = 0; i < Segments; i++)
            {
                float t = (Mathf.PI * 2f * i) / Segments;
                float sin = Mathf.Sin(t);
                float x = 16f * sin * sin * sin / 18f;
                float y = (13f * Mathf.Cos(t)
                    - 5f * Mathf.Cos(2f * t)
                    - 2f * Mathf.Cos(3f * t)
                    - Mathf.Cos(4f * t)) / 18f;
                Vector3 point = new(x, y - 0.05f, -Depth);
                vertices[i + 1] = point;
                vertices[Segments + 2 + i] = new Vector3(point.x, point.y, Depth);
            }

            var triangles = new int[Segments * 12];
            int cursor = 0;
            for (int i = 0; i < Segments; i++)
            {
                int next = (i + 1) % Segments;
                int frontA = i + 1;
                int frontB = next + 1;
                int backA = Segments + 2 + i;
                int backB = Segments + 2 + next;

                triangles[cursor++] = 0;
                triangles[cursor++] = frontA;
                triangles[cursor++] = frontB;

                triangles[cursor++] = Segments + 1;
                triangles[cursor++] = backB;
                triangles[cursor++] = backA;

                triangles[cursor++] = frontA;
                triangles[cursor++] = backA;
                triangles[cursor++] = backB;

                triangles[cursor++] = frontA;
                triangles[cursor++] = backB;
                triangles[cursor++] = frontB;
            }

            heartMesh = new Mesh
            {
                name = "RuntimeHeartPickupMesh",
                hideFlags = HideFlags.DontSave
            };
            heartMesh.vertices = vertices;
            heartMesh.triangles = triangles;
            heartMesh.RecalculateNormals();
            heartMesh.RecalculateBounds();
            return heartMesh;
        }
    }
}
