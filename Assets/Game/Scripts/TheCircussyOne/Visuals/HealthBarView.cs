using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using UnityEngine.Rendering;
using TheCircussyOne.Rules;

namespace TheCircussyOne.Visuals
{
    public sealed class HealthBarView : MonoBehaviour
    {
        private const string BackgroundName = "Health Background";
        private const string FillName = "Health Fill";
        private const float BackgroundDepthOffset = 0.02f;
        private const float FillDepthOffset = -0.02f;
        private static readonly int BaseColorProperty = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorProperty = Shader.PropertyToID("_Color");
        private static readonly Dictionary<string, Material> MaterialsByKey = new();
        private static Mesh quadMesh;
        private static Camera billboardCamera;

        [SerializeField] private MeshRenderer backgroundRenderer;
        [SerializeField] private MeshRenderer fillRenderer;
        [SerializeField] private float width = 1.5f;
        [SerializeField] private Vector3 localOffset = new(0f, 2.35f, 0f);
        [SerializeField] private float height = 2.35f;
        [SerializeField] private float backgroundThickness = 0.12f;
        [SerializeField] private float fillThickness = 0.14f;
        [SerializeField] private Color backgroundColor = new(0.08f, 0.05f, 0.07f, 0.85f);
        [SerializeField] private Color fillColor = new(1f, 0.12f, 0.28f, 1f);
        [SerializeField] private CompareFunction zTest = CompareFunction.LessEqual;
        [SerializeField] private int sortingOrderBase = 30;
        [SerializeField] private int renderQueueBase = 3000;
        [SerializeField] private float currentAlpha = 1f;

        private MaterialPropertyBlock backgroundPropertyBlock;
        private MaterialPropertyBlock fillPropertyBlock;
        private float normalizedValue = 1f;
        private float fadeStartAlpha = 1f;
        private EaseSettings fadeEase = EaseSettings.OutQuad;

        public float CurrentAlpha => currentAlpha;
        public float NormalizedValue => normalizedValue;
        public float BackgroundLength => width;
        public float FillLength => width * normalizedValue;
        public Color BackgroundDisplayColor { get; private set; }
        public Color FillDisplayColor { get; private set; }
        public CompareFunction ZTest => zTest;
        public int BackgroundRenderQueue => renderQueueBase;
        public int FillRenderQueue => renderQueueBase + 1;
        public int BackgroundSortingOrder => sortingOrderBase;
        public int FillSortingOrder => sortingOrderBase + 1;
        public MeshRenderer BackgroundRenderer => backgroundRenderer;
        public MeshRenderer FillRenderer => fillRenderer;

        private void Awake()
        {
            EnsureRenderers();
            SetNormalized(1f);
        }

        private void LateUpdate()
        {
            Camera camera = ResolveBillboardCamera();
            if (camera != null)
            {
                transform.rotation = camera.transform.rotation;
            }
        }

        public static void RegisterBillboardCamera(Camera camera)
        {
            if (camera != null)
            {
                billboardCamera = camera;
            }
        }

        public static void UnregisterBillboardCamera(Camera camera)
        {
            if (billboardCamera == camera)
            {
                billboardCamera = null;
            }
        }

        public void Configure(float width, float height)
        {
            Configure(
                width,
                new Vector3(0f, height, 0f),
                backgroundColor,
                fillColor,
                backgroundThickness,
                fillThickness,
                zTest == CompareFunction.Always,
                sortingOrderBase,
                renderQueueBase);
        }

        public void Configure(
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
            this.width = Mathf.Max(0.01f, width);
            this.localOffset = localOffset;
            this.height = localOffset.y;
            this.backgroundColor = backgroundColor;
            this.fillColor = fillColor;
            this.backgroundThickness = Mathf.Max(0.001f, backgroundThickness);
            this.fillThickness = Mathf.Max(0.001f, fillThickness);
            this.zTest = visibleThroughGeometry ? CompareFunction.Always : CompareFunction.LessEqual;
            this.sortingOrderBase = sortingOrderBase;
            this.renderQueueBase = renderQueueBase;
            SetNormalized(1f);
        }

        public void SetNormalized(float value)
        {
            EnsureRenderers();
            normalizedValue = Mathf.Clamp01(value);

            ApplySegment(
                backgroundRenderer,
                isFill: false,
                length: width,
                thickness: backgroundThickness,
                color: backgroundColor);

            float fillWidth = width * normalizedValue;
            ApplySegment(
                fillRenderer,
                isFill: true,
                length: fillWidth,
                thickness: fillThickness,
                color: fillColor);
        }

        public void SetAlpha(float alpha)
        {
            currentAlpha = Mathf.Clamp01(alpha);
            SetNormalized(normalizedValue);
        }

        public void ResetVisibility()
        {
            Tween.StopAll(this);
            gameObject.SetActive(true);
            SetAlpha(1f);
        }

        public void FadeOut(float seconds, EaseSettings ease)
        {
            Tween.StopAll(this);
            gameObject.SetActive(true);
            fadeStartAlpha = currentAlpha;
            fadeEase = ease;

            if (!Application.isPlaying || seconds <= 0f)
            {
                SetAlpha(0f);
                gameObject.SetActive(false);
                return;
            }

            Tween.Custom(this, 0f, 1f, seconds, static (view, progress) =>
            {
                float eased = GameEasing.Evaluate01(view.fadeEase, progress);
                view.SetAlpha(Mathf.Lerp(view.fadeStartAlpha, 0f, eased));
            }, Ease.Linear)
                .OnComplete(this, static view => view.gameObject.SetActive(false), warnIfTargetDestroyed: false);
        }

        private void EnsureRenderers()
        {
            backgroundRenderer = EnsureSegmentRenderer(backgroundRenderer, BackgroundName, isFill: false);
            fillRenderer = EnsureSegmentRenderer(fillRenderer, FillName, isFill: true);

            transform.localPosition = localOffset == Vector3.zero && !Mathf.Approximately(height, 0f)
                ? new Vector3(0f, height, 0f)
                : localOffset;
        }

        private static Camera ResolveBillboardCamera()
        {
            if (billboardCamera != null && billboardCamera.isActiveAndEnabled)
            {
                return billboardCamera;
            }

            return Camera.main;
        }

        private MeshRenderer EnsureSegmentRenderer(MeshRenderer renderer, string segmentName, bool isFill)
        {
            if (renderer != null)
            {
                ConfigureRenderer(renderer, isFill);
                return renderer;
            }

            Transform child = transform.Find(segmentName);
            if (child == null)
            {
                var childObject = new GameObject(segmentName);
                child = childObject.transform;
                child.SetParent(transform, false);
            }

            if (!child.TryGetComponent(out MeshFilter meshFilter))
            {
                meshFilter = child.gameObject.AddComponent<MeshFilter>();
            }

            meshFilter.sharedMesh = GetQuadMesh();

            if (!child.TryGetComponent(out MeshRenderer meshRenderer))
            {
                meshRenderer = child.gameObject.AddComponent<MeshRenderer>();
            }

            ConfigureRenderer(meshRenderer, isFill);
            return meshRenderer;
        }

        private void ConfigureRenderer(MeshRenderer renderer, bool isFill)
        {
            renderer.sharedMaterial = GetMaterial(isFill ? FillRenderQueue : BackgroundRenderQueue, zTest);
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            renderer.sortingOrder = isFill ? FillSortingOrder : BackgroundSortingOrder;
        }

        private void ApplySegment(
            MeshRenderer renderer,
            bool isFill,
            float length,
            float thickness,
            Color color)
        {
            if (renderer == null)
            {
                return;
            }

            float clampedLength = Mathf.Max(0.0001f, length);
            Transform segment = renderer.transform;
            segment.localScale = new Vector3(clampedLength, Mathf.Max(0.0001f, thickness), 1f);
            segment.localPosition = isFill
                ? new Vector3(-width * 0.5f + clampedLength * 0.5f, 0.01f, FillDepthOffset)
                : new Vector3(0f, 0f, BackgroundDepthOffset);

            Color displayColor = color;
            displayColor.a *= currentAlpha;
            if (isFill)
            {
                FillDisplayColor = displayColor;
            }
            else
            {
                BackgroundDisplayColor = displayColor;
            }

            ConfigureRenderer(renderer, isFill);
            MaterialPropertyBlock propertyBlock = GetPropertyBlock(isFill);
            propertyBlock.Clear();
            propertyBlock.SetColor(BaseColorProperty, displayColor);
            propertyBlock.SetColor(ColorProperty, displayColor);
            renderer.SetPropertyBlock(propertyBlock);
        }

        private MaterialPropertyBlock GetPropertyBlock(bool isFill)
        {
            if (isFill)
            {
                fillPropertyBlock ??= new MaterialPropertyBlock();
                return fillPropertyBlock;
            }

            backgroundPropertyBlock ??= new MaterialPropertyBlock();
            return backgroundPropertyBlock;
        }

        private static Mesh GetQuadMesh()
        {
            if (quadMesh != null)
            {
                return quadMesh;
            }

            quadMesh = new Mesh
            {
                name = "TheCircussyOne Health Bar Quad",
                hideFlags = HideFlags.HideAndDontSave
            };
            quadMesh.SetVertices(new[]
            {
                new Vector3(-0.5f, -0.5f, 0f),
                new Vector3(0.5f, -0.5f, 0f),
                new Vector3(-0.5f, 0.5f, 0f),
                new Vector3(0.5f, 0.5f, 0f)
            });
            quadMesh.SetTriangles(new[] { 0, 2, 1, 2, 3, 1 }, 0);
            quadMesh.SetUVs(0, new[]
            {
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(0f, 1f),
                new Vector2(1f, 1f)
            });
            quadMesh.RecalculateNormals();
            quadMesh.RecalculateBounds();
            return quadMesh;
        }

        private static Material GetMaterial(int renderQueue, CompareFunction compareFunction)
        {
            string key = $"{renderQueue}:{compareFunction}";
            if (MaterialsByKey.TryGetValue(key, out Material material) && material != null)
            {
                return material;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Sprites/Default")
                ?? Shader.Find("Unlit/Color");
            material = new Material(shader)
            {
                name = $"TheCircussyOne Health Bar {compareFunction} {renderQueue}",
                hideFlags = HideFlags.HideAndDontSave,
                renderQueue = renderQueue
            };
            SetFloatIfPresent(material, "_Surface", 1f);
            SetFloatIfPresent(material, "_Blend", 0f);
            SetFloatIfPresent(material, "_SrcBlend", (float)BlendMode.SrcAlpha);
            SetFloatIfPresent(material, "_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            SetFloatIfPresent(material, "_ZWrite", 0f);
            SetFloatIfPresent(material, "_ZTest", (float)compareFunction);
            SetFloatIfPresent(material, "_Cull", (float)CullMode.Off);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.DisableKeyword("_ALPHATEST_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            MaterialsByKey[key] = material;
            return material;
        }

        private static void SetFloatIfPresent(Material material, string propertyName, float value)
        {
            if (material != null && material.HasProperty(propertyName))
            {
                material.SetFloat(propertyName, value);
            }
        }
    }
}
