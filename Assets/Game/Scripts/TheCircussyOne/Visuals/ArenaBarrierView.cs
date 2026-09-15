using System.Collections.Generic;
using Shapes;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;
using UnityEngine;
using UnityEngine.Rendering;

namespace TheCircussyOne.Visuals
{
    public sealed class ArenaBarrierView : MonoBehaviour
    {
        private const float Tau = Mathf.PI * 2f;

        [SerializeField] private Disc baseRing;
        [SerializeField] private List<Line> staticLines = new();
        [SerializeField] private List<Disc> dynamicRings = new();

        private float radius;

        public float Radius => radius;
        public int StaticLineCount => staticLines.Count;
        public int DynamicRingCount => dynamicRings.Count;
        public float BaseRingRadius => baseRing != null ? baseRing.Radius : 0f;

        public float DynamicRingHeight(int index)
        {
            return index >= 0 && index < dynamicRings.Count && dynamicRings[index] != null
                ? dynamicRings[index].transform.localPosition.y
                : 0f;
        }

        public void Configure(float barrierRadius, RunWorldGenerationConfig config)
        {
            radius = Mathf.Max(0.1f, barrierRadius);
            int segmentCount = config != null ? config.ArenaBarrierSegmentCount : 96;
            int ringCount = config != null ? config.ArenaBarrierDynamicRingCount : 5;
            EnsureStaticLines(segmentCount);
            EnsureDynamicRings(ringCount);
            EnsureBaseRing();
            ApplyStaticLayer(config);
            ConfigureRenderers();
            gameObject.SetActive(config == null || config.arenaBarrierEnabled);
        }

        public void ApplyFrame(float time, RunWorldGenerationConfig config)
        {
            if (config == null || !config.arenaBarrierEnabled)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);
            ApplyDynamicLayer(time, config);
        }

        public void Deactivate()
        {
            gameObject.SetActive(false);
        }

        private void ApplyStaticLayer(RunWorldGenerationConfig config)
        {
            float height = ArenaBarrierVisualRules.Height(config);
            float lineThickness = Mathf.Max(0.001f, config != null ? config.arenaBarrierLineThickness : 0.16f);
            Color bottom = config != null ? config.arenaBarrierBottomColor : new Color(0.22f, 0.86f, 1f, 0.34f);
            Color top = config != null ? config.arenaBarrierTopColor : new Color(0.74f, 0.32f, 1f, 0.04f);

            for (int i = 0; i < staticLines.Count; i++)
            {
                Line line = staticLines[i];
                if (line == null)
                {
                    continue;
                }

                float angle = i * Tau / Mathf.Max(1, staticLines.Count);
                Vector3 ground = new(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                Vector3 topPoint = ground + Vector3.up * height;
                line.Geometry = LineGeometry.Volumetric3D;
                line.ColorMode = Line.LineColorMode.Double;
                line.ColorStart = bottom;
                line.ColorEnd = top;
                line.Start = ground;
                line.End = topPoint;
                line.ThicknessSpace = ThicknessSpace.Meters;
                line.Thickness = lineThickness;
                line.EndCaps = LineEndCap.Round;
                line.BlendMode = ShapesBlendMode.Transparent;
                line.DetailLevel = DetailLevel.Low;
                line.SortingOrder = 38;
                line.gameObject.SetActive(true);
            }

            if (baseRing != null)
            {
                bool showBaseRing = config == null || config.arenaBarrierBaseRingEnabled;
                baseRing.gameObject.SetActive(showBaseRing);
                baseRing.Type = DiscType.Ring;
                baseRing.Geometry = DiscGeometry.Flat2D;
                baseRing.RadiusSpace = ThicknessSpace.Meters;
                baseRing.ThicknessSpace = ThicknessSpace.Meters;
                baseRing.Radius = radius;
                baseRing.Thickness = Mathf.Max(0.001f, config != null ? config.arenaBarrierBaseRingThickness : 0.45f);
                baseRing.Color = config != null ? config.arenaBarrierBaseRingColor : new Color(0.18f, 0.84f, 1f, 0.48f);
                baseRing.BlendMode = ShapesBlendMode.Transparent;
                baseRing.SortingOrder = 39;
                baseRing.transform.localPosition = Vector3.up * 0.025f;
                baseRing.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                baseRing.transform.localScale = Vector3.one;
            }
        }

        private void ApplyDynamicLayer(float time, RunWorldGenerationConfig config)
        {
            float ringThickness = Mathf.Max(0.001f, config.arenaBarrierDynamicRingThickness);
            for (int i = 0; i < dynamicRings.Count; i++)
            {
                Disc ring = dynamicRings[i];
                if (ring == null)
                {
                    continue;
                }

                ArenaBarrierDynamicRingFrame frame = ArenaBarrierVisualRules.DynamicRingFrame(
                    i,
                    dynamicRings.Count,
                    time,
                    config);
                bool show = ArenaBarrierVisualRules.ShouldShowDynamicRing(config, frame);
                ring.gameObject.SetActive(show);
                ring.Type = DiscType.Ring;
                ring.Geometry = DiscGeometry.Flat2D;
                ring.RadiusSpace = ThicknessSpace.Meters;
                ring.ThicknessSpace = ThicknessSpace.Meters;
                ring.Radius = radius;
                ring.Thickness = ringThickness;
                ring.Color = frame.Color;
                ring.BlendMode = ShapesBlendMode.Transparent;
                ring.SortingOrder = 40 + i;
                ring.transform.localPosition = Vector3.up * frame.Height;
                ring.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                ring.transform.localScale = Vector3.one;
            }
        }

        private void EnsureStaticLines(int count)
        {
            int safeCount = Mathf.Max(12, count);
            staticLines ??= new List<Line>();
            while (staticLines.Count < safeCount)
            {
                staticLines.Add(CreateShapeChild<Line>($"Static Barrier Line {staticLines.Count:000}"));
            }

            for (int i = staticLines.Count - 1; i >= safeCount; i--)
            {
                DestroyChild(staticLines[i]);
                staticLines.RemoveAt(i);
            }
        }

        private void EnsureDynamicRings(int count)
        {
            int safeCount = Mathf.Max(1, count);
            dynamicRings ??= new List<Disc>();
            while (dynamicRings.Count < safeCount)
            {
                dynamicRings.Add(CreateShapeChild<Disc>($"Dynamic Barrier Ring {dynamicRings.Count:000}"));
            }

            for (int i = dynamicRings.Count - 1; i >= safeCount; i--)
            {
                DestroyChild(dynamicRings[i]);
                dynamicRings.RemoveAt(i);
            }
        }

        private void EnsureBaseRing()
        {
            baseRing ??= CreateShapeChild<Disc>("Static Barrier Base Ring");
        }

        private T CreateShapeChild<T>(string childName) where T : ShapeRenderer
        {
            var child = new GameObject(childName);
            child.transform.SetParent(transform, false);
            child.AddComponent<MeshFilter>();
            child.AddComponent<MeshRenderer>();
            var shape = child.AddComponent<T>();
            shape.name = $"{childName} Shape";
            return shape;
        }

        private static void DestroyChild(Component component)
        {
            if (component == null)
            {
                return;
            }

            GameObject target = component.gameObject;
            if (Application.isPlaying)
            {
                Destroy(target);
                return;
            }

            DestroyImmediate(target);
        }

        private void ConfigureRenderers()
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>(includeInactive: true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                renderer.lightProbeUsage = LightProbeUsage.Off;
                renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            }
        }
    }
}
