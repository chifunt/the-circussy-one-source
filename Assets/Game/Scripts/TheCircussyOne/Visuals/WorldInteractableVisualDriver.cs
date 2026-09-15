using UnityEngine;

namespace TheCircussyOne.Visuals
{
    public struct WorldInteractableViewState
    {
        public bool Targeted { get; private set; }
        public bool Interacting { get; private set; }
        public float InteractionVisualSeconds { get; private set; }

        public void SetTargeted(bool targeted)
        {
            Targeted = targeted;
        }

        public void BeginInteraction()
        {
            Interacting = true;
            InteractionVisualSeconds = 0f;
        }

        public void AdvanceInteraction(float deltaTime)
        {
            InteractionVisualSeconds += Mathf.Max(0f, deltaTime);
        }

        public void CancelInteraction()
        {
            Interacting = false;
            InteractionVisualSeconds = 0f;
        }

        public void ClearInteraction()
        {
            Interacting = false;
            InteractionVisualSeconds = 0f;
            Targeted = false;
        }
    }

    public readonly struct WorldInteractableVisualStyle
    {
        public WorldInteractableVisualStyle(
            float visualScale,
            Color visualColor,
            Color emissionColor,
            float emissionStrength,
            Color outlineColor,
            float outlineThickness,
            float squashStretchAmplitude,
            float squashStretchFrequency,
            float fallbackPromptHeight,
            float targetedScaleMultiplier = 1.1f,
            float targetedColorLerp = 0.35f,
            float targetedEmissionMultiplier = 1.35f,
            Material visualMaterial = null)
        {
            VisualScale = Mathf.Max(0.1f, visualScale);
            VisualColor = visualColor;
            VisualMaterial = visualMaterial;
            EmissionColor = emissionColor;
            EmissionStrength = Mathf.Max(0f, emissionStrength);
            OutlineColor = outlineColor;
            OutlineThickness = Mathf.Max(0f, outlineThickness);
            SquashStretchAmplitude = Mathf.Max(0f, squashStretchAmplitude);
            SquashStretchFrequency = Mathf.Max(0f, squashStretchFrequency);
            FallbackPromptHeight = Mathf.Max(0f, fallbackPromptHeight);
            TargetedScaleMultiplier = Mathf.Max(0.1f, targetedScaleMultiplier);
            TargetedColorLerp = Mathf.Clamp01(targetedColorLerp);
            TargetedEmissionMultiplier = Mathf.Max(0f, targetedEmissionMultiplier);
        }

        public float VisualScale { get; }
        public Color VisualColor { get; }
        public Material VisualMaterial { get; }
        public Color EmissionColor { get; }
        public float EmissionStrength { get; }
        public Color OutlineColor { get; }
        public float OutlineThickness { get; }
        public float SquashStretchAmplitude { get; }
        public float SquashStretchFrequency { get; }
        public float FallbackPromptHeight { get; }
        public float TargetedScaleMultiplier { get; }
        public float TargetedColorLerp { get; }
        public float TargetedEmissionMultiplier { get; }
    }

    public sealed class WorldInteractableVisualDriver
    {
        private MaterialPropertyBlock propertyBlock;
        private Vector3 baseScale = Vector3.one;
        private Vector3 baseVisualLocalPosition;
        private float baseVisualRootToBottomLocalY;
        private bool baseScaleCaptured;

        public Transform VisualRoot { get; private set; }
        public Renderer[] Renderers { get; private set; }

        public void Resolve(Transform owner, ref Transform visualRoot, ref Renderer[] renderers)
        {
            if (owner == null)
            {
                return;
            }

            if (renderers == null || renderers.Length == 0)
            {
                renderers = owner.GetComponentsInChildren<Renderer>(includeInactive: true);
            }

            if (visualRoot == null)
            {
                visualRoot = VisualGroundAnchorUtility.ResolveVisualRoot(owner, renderers);
            }

            VisualRoot = visualRoot;
            Renderers = renderers;

            if (VisualRoot != null && !baseScaleCaptured)
            {
                baseScale = VisualRoot.localScale;
                baseVisualLocalPosition = VisualRoot.localPosition;
                baseVisualRootToBottomLocalY = VisualGroundAnchorUtility.ResolveRootToBottomLocalDistance(VisualRoot, Renderers);
                baseScaleCaptured = true;
            }
        }

        public void Apply(WorldInteractableVisualStyle style, WorldInteractableViewState state, float extraScaleMultiplier = 1f)
        {
            if (VisualRoot != null)
            {
                Vector3 scale = Vector3.Scale(baseScale, InteractionScale(style, state))
                    * style.VisualScale
                    * (state.Targeted ? style.TargetedScaleMultiplier : 1f)
                    * Mathf.Max(0f, extraScaleMultiplier);
                VisualRoot.localScale = scale;
                ApplyBottomAnchoredVisualPosition(scale);
            }

            if (Renderers == null)
            {
                return;
            }

            Color color = state.Targeted
                ? Color.Lerp(style.VisualColor, Color.white, style.TargetedColorLerp)
                : style.VisualColor;
            float emissionStrength = style.EmissionStrength * (state.Targeted ? style.TargetedEmissionMultiplier : 1f);
            for (int i = 0; i < Renderers.Length; i++)
            {
                Renderer targetRenderer = Renderers[i];
                if (targetRenderer == null)
                {
                    continue;
                }

                if (style.VisualMaterial != null && targetRenderer.sharedMaterial != style.VisualMaterial)
                {
                    targetRenderer.sharedMaterial = style.VisualMaterial;
                }

                propertyBlock ??= new MaterialPropertyBlock();
                targetRenderer.GetPropertyBlock(propertyBlock);
                ShaderVisualProperties.ApplyBaseColor(propertyBlock, color);
                ShaderVisualProperties.ApplyEmission(propertyBlock, style.EmissionColor, emissionStrength);
                targetRenderer.SetPropertyBlock(propertyBlock);
            }
        }

        public void UpdateOutline(object owner, bool targeted, Color outlineColor, float outlineThickness)
        {
            if (targeted && Renderers != null)
            {
                InteractableSelectionOutlineRegistry.Register(owner, Renderers, outlineColor, outlineThickness);
                return;
            }

            InteractableSelectionOutlineRegistry.Unregister(owner);
        }

        public void ClearOutline(object owner)
        {
            InteractableSelectionOutlineRegistry.Unregister(owner);
        }

        public Vector3 ResolvePromptPosition(Transform owner, float fallbackHeight)
        {
            if (TryGetActiveRendererBounds(out Bounds bounds))
            {
                return new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
            }

            return owner != null ? owner.position + Vector3.up * Mathf.Max(0f, fallbackHeight) : Vector3.up * Mathf.Max(0f, fallbackHeight);
        }

        public Vector3 ResolveRewardBurstOrigin(Transform owner, float fallbackHeight, float liftMultiplier = 0.25f, float minimumLift = 0.15f)
        {
            if (TryGetActiveRendererBounds(out Bounds bounds))
            {
                float lift = Mathf.Max(minimumLift, bounds.extents.y * Mathf.Max(0f, liftMultiplier));
                return bounds.center + Vector3.up * lift;
            }

            return owner != null ? owner.position + Vector3.up * Mathf.Max(0f, fallbackHeight) : Vector3.up * Mathf.Max(0f, fallbackHeight);
        }

        public static Vector3 InteractionScale(WorldInteractableVisualStyle style, WorldInteractableViewState state)
        {
            if (!state.Interacting || style.SquashStretchAmplitude <= 0f || style.SquashStretchFrequency <= 0f)
            {
                return Vector3.one;
            }

            float pulse = Mathf.Sin(state.InteractionVisualSeconds * style.SquashStretchFrequency * Mathf.PI * 2f) * style.SquashStretchAmplitude;
            float horizontal = Mathf.Max(0.1f, 1f - pulse * 0.5f);
            float vertical = Mathf.Max(0.1f, 1f + pulse);
            return new Vector3(horizontal, vertical, horizontal);
        }

        private bool TryGetActiveRendererBounds(out Bounds bounds)
        {
            bounds = default;
            if (Renderers == null || Renderers.Length == 0)
            {
                return false;
            }

            bool hasBounds = false;
            for (int i = 0; i < Renderers.Length; i++)
            {
                Renderer renderer = Renderers[i];
                if (renderer == null || !renderer.gameObject.activeInHierarchy)
                {
                    continue;
                }

                if (!hasBounds)
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            return hasBounds;
        }

        private void ApplyBottomAnchoredVisualPosition(Vector3 appliedScale)
        {
            if (VisualRoot == null || !baseScaleCaptured)
            {
                return;
            }

            VisualRoot.localPosition = VisualGroundAnchorUtility.BottomAnchoredLocalPosition(
                VisualRoot,
                baseVisualLocalPosition,
                baseScale,
                appliedScale,
                baseVisualRootToBottomLocalY);
        }
    }
}
