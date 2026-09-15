using UnityEngine;
using TheCircussyOne.Rules;

namespace TheCircussyOne.Visuals
{
    internal sealed class ActorBodyMaterialFlash
    {
        private static readonly int ColorProperty = Shader.PropertyToID("_Color");
        private static readonly int BaseColorProperty = Shader.PropertyToID("_BaseColor");
        private static readonly int GridColorProperty = Shader.PropertyToID("_GridColor");
        private static readonly int EmissionColorProperty = Shader.PropertyToID("_EmissionColor");
        private static readonly int EmissionStrengthProperty = Shader.PropertyToID("_EmissionStrength");

        private Renderer primaryRenderer;
        private Transform body;
        private Transform owner;
        private Renderer[] renderers;
        private Renderer[] overrideRenderers;
        private Transform renderersRoot;
        private MaterialPropertyBlock propertyBlock;
        private Color baseColor = Color.white;
        private Color baseEmissionColor = Color.black;
        private float baseEmissionStrength;
        private Color flashStartColor = Color.white;
        private EaseSettings flashEase = EaseSettings.OutQuad;
        private float flashEmissionStrength;

        public Color CurrentBodyColor { get; private set; } = Color.white;

        public void SetTargets(Renderer primaryRenderer, Transform body, Transform owner)
        {
            if (this.primaryRenderer == primaryRenderer && this.body == body && this.owner == owner)
            {
                return;
            }

            this.primaryRenderer = primaryRenderer;
            this.body = body;
            this.owner = owner;
            renderers = null;
            renderersRoot = null;
        }

        public void SetOverrideRenderers(Renderer[] overrideRenderers)
        {
            this.overrideRenderers = HasAnyRenderer(overrideRenderers) ? overrideRenderers : null;
            renderers = null;
            renderersRoot = null;
        }

        public void CaptureBaseColor()
        {
            RefreshRenderers(force: true);
            Renderer captureRenderer = FirstRendererWithMaterial();
            if (captureRenderer == null)
            {
                baseColor = Color.white;
                baseEmissionColor = Color.black;
                baseEmissionStrength = 0f;
                CurrentBodyColor = baseColor;
                return;
            }

            Material material = captureRenderer.sharedMaterial;
            if (material.HasProperty(BaseColorProperty))
            {
                baseColor = material.GetColor(BaseColorProperty);
            }
            else if (material.HasProperty(ColorProperty))
            {
                baseColor = material.GetColor(ColorProperty);
            }
            else
            {
                baseColor = Color.white;
            }

            baseEmissionColor = material.HasProperty(EmissionColorProperty)
                ? material.GetColor(EmissionColorProperty)
                : Color.black;
            baseEmissionStrength = material.HasProperty(EmissionStrengthProperty)
                ? material.GetFloat(EmissionStrengthProperty)
                : 0f;
            CurrentBodyColor = baseColor;
        }

        public void ClearPropertyBlocks()
        {
            CurrentBodyColor = baseColor;
            RefreshRenderers(force: true);
            if (renderers == null || renderers.Length == 0)
            {
                return;
            }

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    renderers[i].SetPropertyBlock(null);
                }
            }
        }

        public void BeginFlash(Color flashColor, EaseSettings ease, float emissionStrength)
        {
            flashEase = ease;
            flashEmissionStrength = Mathf.Max(0f, emissionStrength);
            flashStartColor = flashColor;
            ApplyTransientColor(flashColor, flashColor, flashEmissionStrength);
        }

        public void ApplyFlashProgress(float progress)
        {
            float easedProgress = GameEasing.Evaluate01(flashEase, progress);
            ApplyTransientColor(
                Color.LerpUnclamped(flashStartColor, baseColor, easedProgress),
                Color.LerpUnclamped(flashStartColor, baseEmissionColor, easedProgress),
                Mathf.Lerp(flashEmissionStrength, baseEmissionStrength, easedProgress));
        }

        private void ApplyTransientColor(Color color, Color emissionColor, float emissionStrength)
        {
            CurrentBodyColor = color;
            RefreshRenderers(force: true);
            if (renderers == null || renderers.Length == 0)
            {
                return;
            }

            propertyBlock ??= new MaterialPropertyBlock();
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                renderer.GetPropertyBlock(propertyBlock);
                ShaderVisualProperties.ApplyBaseColor(propertyBlock, color);
                propertyBlock.SetColor(GridColorProperty, color);
                ShaderVisualProperties.ApplyEmission(propertyBlock, emissionColor, emissionStrength);
                renderer.SetPropertyBlock(propertyBlock);
            }
        }

        private void RefreshRenderers(bool force = false)
        {
            if (overrideRenderers != null)
            {
                if (!force && renderers == overrideRenderers)
                {
                    return;
                }

                renderersRoot = null;
                renderers = overrideRenderers;
                return;
            }

            Transform root = body != null && body != owner ? body : null;
            if (!force && renderers != null && renderersRoot == root)
            {
                return;
            }

            renderersRoot = root;
            renderers = root != null
                ? root.GetComponentsInChildren<Renderer>(includeInactive: true)
                : primaryRenderer != null
                    ? new[] { primaryRenderer }
                    : null;
        }

        private Renderer FirstRendererWithMaterial()
        {
            if (renderers != null)
            {
                for (int i = 0; i < renderers.Length; i++)
                {
                    Renderer renderer = renderers[i];
                    if (renderer != null && renderer.sharedMaterial != null)
                    {
                        return renderer;
                    }
                }
            }

            return primaryRenderer != null && primaryRenderer.sharedMaterial != null
                ? primaryRenderer
                : null;
        }

        private static bool HasAnyRenderer(Renderer[] candidates)
        {
            if (candidates == null)
            {
                return false;
            }

            for (int i = 0; i < candidates.Length; i++)
            {
                if (candidates[i] != null)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
