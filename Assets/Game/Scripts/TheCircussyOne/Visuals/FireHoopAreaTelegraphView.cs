using UnityEngine;
using UnityEngine.Rendering;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;

namespace TheCircussyOne.Visuals
{
    public sealed class FireHoopAreaTelegraphView : MonoBehaviour
    {
        private static readonly int BaseColorProperty = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorProperty = Shader.PropertyToID("_Color");

        [SerializeField] private MeshFilter fillMeshFilter;
        [SerializeField] private MeshRenderer fillRenderer;
        [SerializeField] private MeshFilter ringMeshFilter;
        [SerializeField] private MeshRenderer ringRenderer;

        private Mesh fillMesh;
        private Mesh ringMesh;
        private Material runtimeMaterial;
        private MaterialPropertyBlock propertyBlock;
        private Vector3 lastCenter = new(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        private float lastRadius = -1f;
        private float lastRingWidth = -1f;
        private int lastSampleCount = -1;
        private float nextMeshRefreshTime;

        public bool IsActive => gameObject.activeInHierarchy;
        public float Radius { get; private set; }
        public int SampleCount { get; private set; }
        public int RingVertexCount => ringMeshFilter != null && ringMeshFilter.sharedMesh != null ? ringMeshFilter.sharedMesh.vertexCount : 0;
        public int FillVertexCount => fillMeshFilter != null && fillMeshFilter.sharedMesh != null ? fillMeshFilter.sharedMesh.vertexCount : 0;

        private void Awake()
        {
            ResolveComponents();
        }

        public void ConfigureDefaults(VfxVisualConfig config, Material material = null)
        {
            ResolveComponents();
            AssignMaterial(material);
            ApplyFrame(Vector3.zero, 0f, FireHoopAreaTelegraphRules.Evaluate(0f, 2.95f, 0.45f, config), ~0, forceMeshRefresh: true);
            gameObject.SetActive(false);
        }

        public void ApplyFrame(Vector3 center, float elapsedSeconds, FireHoopAreaTelegraphFrame frame, int groundMask, bool forceMeshRefresh = false)
        {
            ResolveComponents();
            if (!frame.Visible || frame.Radius <= 0f)
            {
                Deactivate();
                return;
            }

            gameObject.SetActive(true);
            transform.position = center;
            transform.rotation = Quaternion.identity;
            Radius = frame.Radius;
            SampleCount = frame.SampleCount;

            ApplyColor(fillRenderer, frame.FillColor);
            ApplyColor(ringRenderer, frame.RingColor);

            if (forceMeshRefresh
                || elapsedSeconds >= nextMeshRefreshTime
                || (center - lastCenter).sqrMagnitude > 0.0025f
                || !Mathf.Approximately(lastRadius, frame.Radius)
                || !Mathf.Approximately(lastRingWidth, frame.RingWidth)
                || lastSampleCount != frame.SampleCount)
            {
                RebuildMeshes(center, frame, groundMask);
                lastCenter = center;
                lastRadius = frame.Radius;
                lastRingWidth = frame.RingWidth;
                lastSampleCount = frame.SampleCount;
                nextMeshRefreshTime = elapsedSeconds + frame.RefreshSeconds;
            }
        }

        public void Deactivate()
        {
            if (this == null)
            {
                return;
            }

            gameObject.SetActive(false);
        }

        private void RebuildMeshes(Vector3 center, FireHoopAreaTelegraphFrame frame, int groundMask)
        {
            int samples = Mathf.Max(8, frame.SampleCount);
            Vector3[] outer = new Vector3[samples];
            Vector3[] inner = new Vector3[samples];
            float innerRadius = frame.InnerRingRadius;

            for (int i = 0; i < samples; i++)
            {
                float angle = i * Mathf.PI * 2f / samples;
                Vector3 direction = new(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                outer[i] = SampleLocalPoint(center, direction * frame.Radius, frame, groundMask);
                inner[i] = SampleLocalPoint(center, direction * innerRadius, frame, groundMask);
            }

            RebuildFillMesh(center, outer, frame, groundMask);
            RebuildRingMesh(inner, outer);
        }

        private void RebuildFillMesh(Vector3 center, Vector3[] outer, FireHoopAreaTelegraphFrame frame, int groundMask)
        {
            int samples = outer.Length;
            Vector3[] vertices = new Vector3[samples + 1];
            int[] triangles = new int[samples * 3];
            vertices[0] = SampleLocalPoint(center, Vector3.zero, frame, groundMask);
            for (int i = 0; i < samples; i++)
            {
                vertices[i + 1] = outer[i];
                int triangle = i * 3;
                triangles[triangle] = 0;
                triangles[triangle + 1] = i + 1;
                triangles[triangle + 2] = i == samples - 1 ? 1 : i + 2;
            }

            fillMesh.Clear();
            fillMesh.vertices = vertices;
            fillMesh.triangles = triangles;
            fillMesh.RecalculateBounds();
        }

        private void RebuildRingMesh(Vector3[] inner, Vector3[] outer)
        {
            int samples = outer.Length;
            Vector3[] vertices = new Vector3[samples * 2];
            int[] triangles = new int[samples * 6];
            for (int i = 0; i < samples; i++)
            {
                int next = i == samples - 1 ? 0 : i + 1;
                int innerIndex = i * 2;
                int outerIndex = innerIndex + 1;
                int nextInnerIndex = next * 2;
                int nextOuterIndex = nextInnerIndex + 1;
                vertices[innerIndex] = inner[i];
                vertices[outerIndex] = outer[i];
                int triangle = i * 6;
                triangles[triangle] = innerIndex;
                triangles[triangle + 1] = outerIndex;
                triangles[triangle + 2] = nextOuterIndex;
                triangles[triangle + 3] = innerIndex;
                triangles[triangle + 4] = nextOuterIndex;
                triangles[triangle + 5] = nextInnerIndex;
            }

            ringMesh.Clear();
            ringMesh.vertices = vertices;
            ringMesh.triangles = triangles;
            ringMesh.RecalculateBounds();
        }

        private Vector3 SampleLocalPoint(Vector3 center, Vector3 offset, FireHoopAreaTelegraphFrame frame, int groundMask)
        {
            Vector3 world = center + offset;
            Vector3 origin = world + Vector3.up * frame.ProbeHeight;
            float fallbackY = center.y + frame.GroundClearance;
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, frame.ProbeHeight + frame.ProbeDepth, groundMask, QueryTriggerInteraction.Ignore))
            {
                world.y = hit.point.y + frame.GroundClearance;
            }
            else
            {
                world.y = fallbackY;
            }

            return transform.InverseTransformPoint(world);
        }

        private void ResolveComponents()
        {
            if (fillMeshFilter == null)
            {
                Transform fill = transform.Find("Area Fill");
                if (fill != null)
                {
                    fillMeshFilter = fill.GetComponent<MeshFilter>();
                    fillRenderer = fill.GetComponent<MeshRenderer>();
                }
            }

            if (ringMeshFilter == null)
            {
                Transform ring = transform.Find("Area Ring");
                if (ring != null)
                {
                    ringMeshFilter = ring.GetComponent<MeshFilter>();
                    ringRenderer = ring.GetComponent<MeshRenderer>();
                }
            }

            if (fillMeshFilter == null)
            {
                CreateMeshChild("Area Fill", out fillMeshFilter, out fillRenderer);
            }

            if (ringMeshFilter == null)
            {
                CreateMeshChild("Area Ring", out ringMeshFilter, out ringRenderer);
            }

            EnsureMeshes();
            ConfigureRenderer(fillRenderer, sortingOrder: 63);
            ConfigureRenderer(ringRenderer, sortingOrder: 64);
        }

        private void EnsureMeshes()
        {
            if (fillMesh == null)
            {
                fillMesh = new Mesh { name = "FireHoopAreaFillMesh" };
                fillMesh.MarkDynamic();
            }

            if (ringMesh == null)
            {
                ringMesh = new Mesh { name = "FireHoopAreaRingMesh" };
                ringMesh.MarkDynamic();
            }

            if (fillMeshFilter != null)
            {
                fillMeshFilter.sharedMesh = fillMesh;
            }

            if (ringMeshFilter != null)
            {
                ringMeshFilter.sharedMesh = ringMesh;
            }
        }

        private void AssignMaterial(Material material)
        {
            Material resolved = material != null ? material : runtimeMaterial;
            if (resolved == null)
            {
                resolved = CreateRuntimeMaterial();
                runtimeMaterial = resolved;
            }

            if (fillRenderer != null)
            {
                fillRenderer.sharedMaterial = resolved;
            }

            if (ringRenderer != null)
            {
                ringRenderer.sharedMaterial = resolved;
            }
        }

        private void ApplyColor(Renderer renderer, Color color)
        {
            if (renderer == null)
            {
                return;
            }

            if (renderer.sharedMaterial == null)
            {
                AssignMaterial(null);
            }

            propertyBlock ??= new MaterialPropertyBlock();
            renderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(BaseColorProperty, color);
            propertyBlock.SetColor(ColorProperty, color);
            renderer.SetPropertyBlock(propertyBlock);
        }

        private void CreateMeshChild(string childName, out MeshFilter filter, out MeshRenderer renderer)
        {
            var child = new GameObject(childName);
            filter = child.AddComponent<MeshFilter>();
            renderer = child.AddComponent<MeshRenderer>();
            child.transform.SetParent(transform, false);
        }

        private void ConfigureRenderer(Renderer renderer, int sortingOrder)
        {
            if (renderer == null)
            {
                return;
            }

            renderer.transform.SetParent(transform, false);
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            renderer.sortingOrder = sortingOrder;
        }

        private static Material CreateRuntimeMaterial()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Unlit/Color")
                ?? Shader.Find("Sprites/Default");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            var material = new Material(shader);
            material.name = "FireHoopAreaTelegraphRuntimeMaterial";
            material.hideFlags = HideFlags.DontSave;
            if (shader != null)
            {
                material.renderQueue = (int)RenderQueue.Transparent;
                SetFloatIfPresent(material, "_Surface", 1f);
                SetFloatIfPresent(material, "_Blend", 0f);
                SetFloatIfPresent(material, "_SrcBlend", (float)BlendMode.SrcAlpha);
                SetFloatIfPresent(material, "_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                SetFloatIfPresent(material, "_ZWrite", 0f);
                SetFloatIfPresent(material, "_Cull", (float)CullMode.Off);
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            }

            return material;
        }

        private static void SetFloatIfPresent(Material material, string property, float value)
        {
            if (material != null && material.HasProperty(property))
            {
                material.SetFloat(property, value);
            }
        }
    }
}
