using UnityEngine;
using TheCircussyOne.Rules;

namespace TheCircussyOne.Visuals
{
    [ExecuteAlways]
    public sealed class DamageVignetteView : MonoBehaviour
    {
        private static readonly int TintId = Shader.PropertyToID("_Tint");
        private static readonly int DarkTintId = Shader.PropertyToID("_DarkTint");
        private static readonly int IntensityId = Shader.PropertyToID("_Intensity");
        private static readonly int MaxOpacityId = Shader.PropertyToID("_MaxOpacity");
        private static readonly int DarkMaxOpacityId = Shader.PropertyToID("_DarkMaxOpacity");
        private static readonly int GrainStrengthId = Shader.PropertyToID("_GrainStrength");
        private static readonly int SplotchStrengthId = Shader.PropertyToID("_SplotchStrength");
        private static readonly int RadiusId = Shader.PropertyToID("_Radius");
        private static readonly int SoftnessId = Shader.PropertyToID("_Softness");
        private static readonly int TimeValueId = Shader.PropertyToID("_TimeValue");

        [SerializeField] private Camera targetCamera;
        [SerializeField] private Material material;
        [SerializeField] private MeshRenderer overlayRenderer;
        [SerializeField] private MeshFilter overlayMeshFilter;

        private MaterialPropertyBlock propertyBlock;
        private Mesh runtimeMesh;
        private Material runtimeMaterial;

        public bool IsVisible => overlayRenderer != null && overlayRenderer.enabled;
        public Material Material => material;

        private void Awake()
        {
            if (overlayRenderer != null)
            {
                ApplyFrame(PlayerDamageVignetteFrame.Hidden);
            }
        }

        private void OnDisable()
        {
            ApplyFrame(PlayerDamageVignetteFrame.Hidden);
        }

        private void OnDestroy()
        {
            if (runtimeMesh != null)
            {
                DestroySafe(runtimeMesh);
                runtimeMesh = null;
            }

            if (runtimeMaterial != null)
            {
                DestroySafe(runtimeMaterial);
                runtimeMaterial = null;
            }
        }

        private void LateUpdate()
        {
            AlignToCamera();
        }

        public void Configure(Material vignetteMaterial, Camera camera, MeshRenderer renderer = null, MeshFilter filter = null)
        {
            material = vignetteMaterial;
            targetCamera = camera;
            if (renderer != null)
            {
                overlayRenderer = renderer;
            }

            if (filter != null)
            {
                overlayMeshFilter = filter;
            }

            if (overlayRenderer != null || overlayMeshFilter != null)
            {
                EnsureRenderer();
            }
        }

        public void ApplyFrame(PlayerDamageVignetteFrame frame)
        {
            bool visible = frame.Visible && frame.Intensity > 0f && material != null;
            if (frame.Visible && frame.Intensity > 0f && material == null)
            {
                EnsureRuntimeMaterial();
                visible = material != null;
            }

            if (!visible)
            {
                if (overlayRenderer != null)
                {
                    overlayRenderer.enabled = false;
                }

                return;
            }

            EnsureRenderer();
            if (overlayRenderer == null)
            {
                return;
            }

            overlayRenderer.enabled = true;
            overlayRenderer.sharedMaterial = material;
            propertyBlock ??= new MaterialPropertyBlock();
            overlayRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(TintId, frame.Tint);
            propertyBlock.SetColor(DarkTintId, frame.DarkTint);
            propertyBlock.SetFloat(IntensityId, frame.Intensity);
            propertyBlock.SetFloat(MaxOpacityId, frame.MaxOpacity);
            propertyBlock.SetFloat(DarkMaxOpacityId, frame.DarkMaxOpacity);
            propertyBlock.SetFloat(GrainStrengthId, frame.GrainStrength);
            propertyBlock.SetFloat(SplotchStrengthId, frame.SplotchStrength);
            propertyBlock.SetFloat(RadiusId, frame.Radius);
            propertyBlock.SetFloat(SoftnessId, frame.Softness);
            propertyBlock.SetFloat(TimeValueId, frame.Time);
            overlayRenderer.SetPropertyBlock(propertyBlock);
            AlignToCamera();
        }

        private void EnsureRenderer()
        {
            if (overlayRenderer != null && overlayMeshFilter != null)
            {
                EnsureMesh();
                overlayRenderer.sharedMaterial = material;
                return;
            }

            Transform child = transform.Find("Damage Vignette Overlay");
            GameObject overlayObject = child != null ? child.gameObject : new GameObject("Damage Vignette Overlay");
            overlayObject.transform.SetParent(transform, false);
            overlayObject.hideFlags = Application.isPlaying ? HideFlags.DontSave : HideFlags.None;

            overlayMeshFilter = overlayObject.GetComponent<MeshFilter>();
            if (overlayMeshFilter == null)
            {
                overlayMeshFilter = overlayObject.AddComponent<MeshFilter>();
            }

            overlayRenderer = overlayObject.GetComponent<MeshRenderer>();
            if (overlayRenderer == null)
            {
                overlayRenderer = overlayObject.AddComponent<MeshRenderer>();
            }

            overlayRenderer.sharedMaterial = material;
            overlayRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            overlayRenderer.receiveShadows = false;
            overlayRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            overlayRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            overlayRenderer.sortingOrder = 5000;
            EnsureMesh();
        }

        private void EnsureRuntimeMaterial()
        {
            if (material != null)
            {
                return;
            }

            Shader shader = Shader.Find("TheCircussyOne/Player Damage Vignette");
            if (shader == null)
            {
                return;
            }

            runtimeMaterial = new Material(shader)
            {
                name = "Runtime Player Damage Vignette",
                hideFlags = HideFlags.DontSave
            };
            material = runtimeMaterial;
        }

        private void EnsureMesh()
        {
            if (overlayMeshFilter == null || overlayMeshFilter.sharedMesh != null)
            {
                return;
            }

            runtimeMesh = new Mesh
            {
                name = "Damage Vignette Quad",
                hideFlags = HideFlags.DontSave
            };
            runtimeMesh.vertices = new[]
            {
                new Vector3(-0.5f, -0.5f, 0f),
                new Vector3(0.5f, -0.5f, 0f),
                new Vector3(0.5f, 0.5f, 0f),
                new Vector3(-0.5f, 0.5f, 0f)
            };
            runtimeMesh.uv = new[]
            {
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(1f, 1f),
                new Vector2(0f, 1f)
            };
            runtimeMesh.triangles = new[] { 0, 2, 1, 0, 3, 2 };
            runtimeMesh.RecalculateBounds();
            overlayMeshFilter.sharedMesh = runtimeMesh;
        }

        public void AlignToCamera()
        {
            if (overlayRenderer == null || !overlayRenderer.enabled)
            {
                return;
            }

            Camera camera = targetCamera != null ? targetCamera : Camera.main;
            if (camera == null)
            {
                return;
            }

            float distance = Mathf.Max(camera.nearClipPlane + 0.03f, 0.12f);
            float height = 2f * distance * Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float width = height * Mathf.Max(0.01f, camera.aspect);
            Transform overlayTransform = overlayRenderer.transform;
            overlayTransform.position = camera.transform.position + camera.transform.forward * distance;
            overlayTransform.rotation = camera.transform.rotation;
            overlayTransform.localScale = new Vector3(width, height, 1f);
        }

        private static void DestroySafe(Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }
    }
}
