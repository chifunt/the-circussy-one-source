using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace TheCircussyOne.Visuals
{
    public sealed class InteractableSelectionOutlineRendererFeature : ScriptableRendererFeature
    {
        [SerializeField] private Shader maskShader;
        [SerializeField] private Shader compositeShader;
        [SerializeField] private Color fallbackColor = new(1f, 0.98f, 0.72f, 1f);
        [SerializeField, Min(0f)] private float fallbackWidthPixels = 3f;
        [Tooltip("When enabled, object parts hidden by scene depth do not render into the outline mask.")]
        [SerializeField] private bool occludeHiddenPixels = true;
        [SerializeField] private RenderPassEvent passEvent = RenderPassEvent.AfterRenderingPostProcessing;

        private InteractableSelectionOutlinePass pass;
        private Material depthTestMaskMaterial;
        private Material alwaysVisibleMaskMaterial;
        private Material compositeMaterial;

        public bool OccludeHiddenPixels => occludeHiddenPixels;

        public override void Create()
        {
            maskShader ??= Shader.Find("TheCircussyOne/Interactable Selection Mask");
            compositeShader ??= Shader.Find("TheCircussyOne/Interactable Selection Outline");
            CoreUtils.Destroy(depthTestMaskMaterial);
            CoreUtils.Destroy(alwaysVisibleMaskMaterial);
            CoreUtils.Destroy(compositeMaterial);
            depthTestMaskMaterial = CreateMaskMaterial(maskShader, CompareFunction.LessEqual);
            alwaysVisibleMaskMaterial = CreateMaskMaterial(maskShader, CompareFunction.Always);
            compositeMaterial = compositeShader != null ? CoreUtils.CreateEngineMaterial(compositeShader) : null;
            pass = new InteractableSelectionOutlinePass(passEvent);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.cameraType != CameraType.Game && renderingData.cameraData.cameraType != CameraType.SceneView)
            {
                return;
            }

            if (InteractableSelectionOutlineRegistry.Count == 0
                || depthTestMaskMaterial == null
                || alwaysVisibleMaskMaterial == null
                || compositeMaterial == null)
            {
                return;
            }

            pass.Setup(
                depthTestMaskMaterial,
                alwaysVisibleMaskMaterial,
                compositeMaterial,
                fallbackColor,
                fallbackWidthPixels,
                occludeHiddenPixels);
            renderer.EnqueuePass(pass);
        }

        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(depthTestMaskMaterial);
            CoreUtils.Destroy(alwaysVisibleMaskMaterial);
            CoreUtils.Destroy(compositeMaterial);
            depthTestMaskMaterial = null;
            alwaysVisibleMaskMaterial = null;
            compositeMaterial = null;
            pass = null;
        }

        private static Material CreateMaskMaterial(Shader shader, CompareFunction zTest)
        {
            if (shader == null)
            {
                return null;
            }

            Material material = CoreUtils.CreateEngineMaterial(shader);
            material.SetFloat("_ZTest", (float)zTest);
            return material;
        }

        private sealed class InteractableSelectionOutlinePass : ScriptableRenderPass
        {
            private static readonly int OutlineColorId = Shader.PropertyToID("_OutlineColor");
            private static readonly int OutlineWidthPixelsId = Shader.PropertyToID("_OutlineWidthPixels");
            private static readonly int SelectionMaskTexId = Shader.PropertyToID("_InteractableSelectionMaskTex");
            private static readonly int SelectionMaskTexelSizeId = Shader.PropertyToID("_InteractableSelectionMaskTex_TexelSize");
            private static readonly List<Renderer> SelectedRenderers = new();
            private static readonly MaterialPropertyBlock CompositeProperties = new();

            private Material depthTestMaskMaterial;
            private Material alwaysVisibleMaskMaterial;
            private Material compositeMaterial;
            private Color fallbackColor;
            private float fallbackWidthPixels;
            private bool occludeHiddenPixels;

            public InteractableSelectionOutlinePass(RenderPassEvent passEvent)
            {
                renderPassEvent = passEvent;
                profilingSampler = new ProfilingSampler("Interactable Selection Outline");
            }

            public void Setup(
                Material depthTestMask,
                Material alwaysVisibleMask,
                Material composite,
                Color color,
                float widthPixels,
                bool occludeHidden)
            {
                depthTestMaskMaterial = depthTestMask;
                alwaysVisibleMaskMaterial = alwaysVisibleMask;
                compositeMaterial = composite;
                fallbackColor = color;
                fallbackWidthPixels = widthPixels;
                occludeHiddenPixels = occludeHidden;
            }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                if (depthTestMaskMaterial == null || alwaysVisibleMaskMaterial == null || compositeMaterial == null)
                {
                    return;
                }

                SelectedRenderers.Clear();
                InteractableSelectionOutlineRegistry.CopyRenderers(SelectedRenderers);
                if (SelectedRenderers.Count == 0)
                {
                    return;
                }

                UniversalResourceData resources = frameData.Get<UniversalResourceData>();
                if (!resources.activeColorTexture.IsValid())
                {
                    return;
                }

                TextureDesc maskDesc = renderGraph.GetTextureDesc(resources.activeColorTexture);
                maskDesc.name = "_InteractableSelectionMaskTex";
                maskDesc.colorFormat = GraphicsFormat.R8_UNorm;
                maskDesc.depthBufferBits = DepthBits.None;
                maskDesc.msaaSamples = MSAASamples.None;
                maskDesc.clearBuffer = true;
                maskDesc.clearColor = Color.clear;
                maskDesc.filterMode = FilterMode.Point;
                TextureHandle maskTexture = renderGraph.CreateTexture(maskDesc);
                bool useDepthOcclusion = occludeHiddenPixels && resources.activeDepthTexture.IsValid();

                using (var builder = renderGraph.AddRasterRenderPass<MaskPassData>("Interactable Selection Mask", out var passData, profilingSampler))
                {
                    passData.maskMaterial = useDepthOcclusion ? depthTestMaskMaterial : alwaysVisibleMaskMaterial;
                    passData.renderers = SelectedRenderers.ToArray();
                    builder.SetRenderAttachment(maskTexture, 0, AccessFlags.Write);
                    if (useDepthOcclusion)
                    {
                        builder.SetRenderAttachmentDepth(resources.activeDepthTexture, AccessFlags.Read);
                    }

                    builder.AllowPassCulling(false);
                    builder.SetRenderFunc(static (MaskPassData data, RasterGraphContext context) =>
                    {
                        for (int i = 0; i < data.renderers.Length; i++)
                        {
                            Renderer renderer = data.renderers[i];
                            if (renderer == null)
                            {
                                continue;
                            }

                            int submeshCount = Mathf.Max(1, renderer.sharedMaterials != null ? renderer.sharedMaterials.Length : 1);
                            for (int submeshIndex = 0; submeshIndex < submeshCount; submeshIndex++)
                            {
                                context.cmd.DrawRenderer(renderer, data.maskMaterial, submeshIndex, 0);
                            }
                        }
                    });
                }

                InteractableSelectionOutlineSettings settings = InteractableSelectionOutlineRegistry.TryGetCompositeSettings(out var registrySettings)
                    ? registrySettings
                    : new InteractableSelectionOutlineSettings(fallbackColor, fallbackWidthPixels);
                if (settings.WidthPixels <= 0f)
                {
                    return;
                }

                TextureDesc colorDesc = renderGraph.GetTextureDesc(resources.activeColorTexture);
                using (var builder = renderGraph.AddRasterRenderPass<CompositePassData>("Interactable Selection Outline Composite", out var passData, profilingSampler))
                {
                    passData.compositeMaterial = compositeMaterial;
                    passData.maskTexture = maskTexture;
                    passData.color = settings.Color;
                    passData.widthPixels = settings.WidthPixels;
                    passData.texelSize = new Vector4(
                        1f / Mathf.Max(1, colorDesc.width),
                        1f / Mathf.Max(1, colorDesc.height),
                        colorDesc.width,
                        colorDesc.height);

                    builder.UseTexture(maskTexture, AccessFlags.Read);
                    builder.SetRenderAttachment(resources.activeColorTexture, 0, AccessFlags.ReadWrite);
                    builder.AllowGlobalStateModification(true);
                    builder.AllowPassCulling(false);
                    builder.SetRenderFunc(static (CompositePassData data, RasterGraphContext context) =>
                    {
                        CompositeProperties.Clear();
                        CompositeProperties.SetColor(OutlineColorId, data.color);
                        CompositeProperties.SetFloat(OutlineWidthPixelsId, data.widthPixels);
                        CompositeProperties.SetVector(SelectionMaskTexelSizeId, data.texelSize);
                        context.cmd.SetGlobalTexture(SelectionMaskTexId, data.maskTexture);
                        context.cmd.DrawProcedural(
                            Matrix4x4.identity,
                            data.compositeMaterial,
                            0,
                            MeshTopology.Triangles,
                            3,
                            1,
                            CompositeProperties);
                    });
                }
            }

            private sealed class MaskPassData
            {
                public Material maskMaterial;
                public Renderer[] renderers;
            }

            private sealed class CompositePassData
            {
                public Material compositeMaterial;
                public TextureHandle maskTexture;
                public Color color;
                public float widthPixels;
                public Vector4 texelSize;
            }
        }
    }
}
