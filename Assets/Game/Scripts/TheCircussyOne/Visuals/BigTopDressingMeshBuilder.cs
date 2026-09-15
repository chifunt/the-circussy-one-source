using System.Collections.Generic;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;
using UnityEngine;
using UnityEngine.Rendering;

namespace TheCircussyOne.Visuals
{
    public static class BigTopDressingMeshBuilder
    {
        private const float Tau = Mathf.PI * 2f;

        public static Mesh BuildRopesAndHub(BigTopEnvironmentSpec spec, RunWorldGenerationConfig config)
        {
            if (!spec.IsValid || config == null || !config.BigTopDressingEnabled)
            {
                return null;
            }

            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            float ropeRadius = config.BigTopCanopyRopeThickness * 0.5f;
            float hubTubeRadius = config.BigTopCenterHubTubeRadius;
            float hubRadius = Mathf.Max(hubTubeRadius * 2f, spec.Radius * config.BigTopCenterHubRadiusPercent);
            float hubY = spec.TotalHeight - Mathf.Max(0.75f, hubTubeRadius * 2.5f);

            if (config.BigTopCenterHubEnabled && hubTubeRadius > 0f)
            {
                AddTorus(vertices, triangles, Vector3.up * hubY, hubRadius, hubTubeRadius, spec.SegmentCount, 8);
                AddHangingCords(vertices, triangles, spec, config, hubY, hubRadius, Mathf.Max(0.025f, Mathf.Min(ropeRadius, hubTubeRadius * 0.45f)));
            }

            if (config.BigTopCanopyRopesEnabled && ropeRadius > 0f)
            {
                AddRadialRopes(vertices, triangles, spec, config, hubY, hubRadius, ropeRadius);
                AddRingRopes(vertices, triangles, spec, config, ropeRadius);
            }

            return BuildMesh("Big Top Dressing Rope Mesh", vertices, triangles);
        }

        public static Mesh BuildPoles(BigTopEnvironmentSpec spec, RunWorldGenerationConfig config)
        {
            if (!spec.IsValid
                || config == null
                || !config.BigTopPerimeterPolesEnabled
                || config.BigTopPerimeterPoleCount <= 0
                || config.BigTopPerimeterPoleRadius <= 0f)
            {
                return null;
            }

            var vertices = new List<Vector3>(config.BigTopPerimeterPoleCount * 32);
            var triangles = new List<int>(config.BigTopPerimeterPoleCount * 48);
            float radius = Mathf.Min(spec.Radius - config.BigTopPerimeterPoleRadius, spec.Radius * config.BigTopPerimeterPoleRadiusPercent);
            float topY = Mathf.Max(spec.CurtainHeight + 1f, spec.TotalHeight - config.BigTopPerimeterPoleTopPadding);
            for (int i = 0; i < config.BigTopPerimeterPoleCount; i++)
            {
                float angle = i / (float)config.BigTopPerimeterPoleCount * Tau;
                Vector3 basePoint = Polar(angle, radius, 0f);
                Vector3 topPoint = new(basePoint.x, topY, basePoint.z);
                AddCylinder(vertices, triangles, basePoint, topPoint, config.BigTopPerimeterPoleRadius, 10);
            }

            return BuildMesh("Big Top Dressing Pole Mesh", vertices, triangles);
        }

        public static Mesh BuildValance(BigTopEnvironmentSpec spec, RunWorldGenerationConfig config)
        {
            if (!spec.IsValid
                || config == null
                || !config.BigTopCurtainValanceEnabled
                || config.BigTopCurtainValanceHeight <= 0f)
            {
                return null;
            }

            int segments = Mathf.Max(16, spec.SegmentCount);
            int scallops = Mathf.Max(1, config.BigTopCurtainValanceScallopCount);
            float radius = Mathf.Max(0.1f, spec.Radius - 0.1f);
            float topY = spec.CurtainHeight + 0.06f;
            var vertices = new Vector3[(segments + 1) * 2];
            var uvs = new Vector2[vertices.Length];
            var triangles = new List<int>(segments * 6);

            for (int segment = 0; segment <= segments; segment++)
            {
                float u = segment / (float)segments;
                float angle = u * Tau;
                float scallopT = Mathf.Repeat(u * scallops, 1f);
                float scallop = Mathf.Sin(scallopT * Mathf.PI);
                float bottomY = topY - config.BigTopCurtainValanceHeight - scallop * config.BigTopCurtainValanceScallopDrop;
                int topIndex = segment * 2;
                int bottomIndex = topIndex + 1;
                vertices[topIndex] = Polar(angle, radius, topY);
                vertices[bottomIndex] = Polar(angle, radius, bottomY);
                uvs[topIndex] = new Vector2(u, 1f);
                uvs[bottomIndex] = new Vector2(u, 0f);
            }

            for (int segment = 0; segment < segments; segment++)
            {
                int a = segment * 2;
                int b = (segment + 1) * 2;
                int c = (segment + 1) * 2 + 1;
                int d = segment * 2 + 1;
                AddInwardQuad(triangles, a, b, c, d);
            }

            var mesh = new Mesh
            {
                name = "Big Top Dressing Valance Mesh",
                vertices = vertices,
                uv = uvs,
                triangles = triangles.ToArray()
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        public static Mesh BuildGodRayFixtureBodies(BigTopEnvironmentSpec spec, RunWorldGenerationConfig config)
        {
            return BuildGodRayFixtureBodies(spec, config, 0f, animateShowLights: false);
        }

        public static Mesh BuildGodRayFixtureBodies(
            BigTopEnvironmentSpec spec,
            RunWorldGenerationConfig config,
            float timeSeconds,
            bool animateShowLights)
        {
            if (!spec.IsValid
                || config == null
                || !config.BigTopGodRayFixturesEnabled
                || config.BigTopGodRayFixtureLength <= 0f
                || config.BigTopGodRayFixtureRadius <= 0f)
            {
                return null;
            }

            int count = config.BigTopGodRayCount;
            var vertices = new List<Vector3>(count * 112);
            var triangles = new List<int>(count * 168);
            float bodyRadius = config.BigTopGodRayFixtureRadius;
            float mountRadius = bodyRadius * 0.55f;
            float supportRadius = Mathf.Max(0.04f, bodyRadius * 0.06f);
            for (int i = 0; i < count; i++)
            {
                BigTopGodRaySource source = BigTopGodRayMeshBuilder.SourceAt(spec, config, i, count, timeSeconds, animateShowLights);
                AddCylinder(vertices, triangles, FixtureSupportAnchor(spec, source), source.Mount, supportRadius, 6);
                AddConeFrustum(
                    vertices,
                    triangles,
                    source.Mount,
                    source.Start,
                    mountRadius,
                    bodyRadius,
                    14);
                AddFixtureYoke(vertices, triangles, source, bodyRadius);
            }

            return BuildMesh("Big Top God Ray Fixture Body Mesh", vertices, triangles);
        }

        public static Mesh BuildGodRayFixtureLenses(BigTopEnvironmentSpec spec, RunWorldGenerationConfig config)
        {
            return BuildGodRayFixtureLenses(spec, config, 0f, animateShowLights: false);
        }

        public static Mesh BuildGodRayFixtureLenses(
            BigTopEnvironmentSpec spec,
            RunWorldGenerationConfig config,
            float timeSeconds,
            bool animateShowLights)
        {
            if (!spec.IsValid
                || config == null
                || !config.BigTopGodRayFixturesEnabled
                || config.BigTopGodRayFixtureLensRadius <= 0f)
            {
                return null;
            }

            int count = config.BigTopGodRayCount;
            const int segments = 24;
            var vertices = new List<Vector3>(count * (segments * 2 + 2));
            var colors = new List<Color>(count * (segments * 2 + 2));
            var triangles = new List<int>(count * segments * 12);
            float lensRadius = config.BigTopGodRayFixtureLensRadius;
            for (int i = 0; i < count; i++)
            {
                BigTopGodRaySource source = BigTopGodRayMeshBuilder.SourceAt(spec, config, i, count, timeSeconds, animateShowLights);
                Color tint = BigTopVisualColorRules.ShowLightTint(config, spec.Seed, i, timeSeconds);
                AddLensCap(vertices, colors, triangles, source, lensRadius, segments, tint);
            }

            return BuildMesh("Big Top God Ray Fixture Lens Mesh", vertices, triangles, colors);
        }

        private static void AddHangingCords(
            List<Vector3> vertices,
            List<int> triangles,
            BigTopEnvironmentSpec spec,
            RunWorldGenerationConfig config,
            float hubY,
            float hubRadius,
            float cordRadius)
        {
            int cordCount = config.BigTopCenterHubCordCount;
            if (cordCount <= 0 || config.BigTopCenterHubCordDrop <= 0f || cordRadius <= 0f)
            {
                return;
            }

            float bottomY = Mathf.Max(spec.CurtainHeight + spec.DomeHeight * 0.08f, hubY - config.BigTopCenterHubCordDrop);
            for (int i = 0; i < cordCount; i++)
            {
                float angle = i / (float)cordCount * Tau;
                Vector3 top = Polar(angle, hubRadius * 0.72f, hubY - config.BigTopCenterHubTubeRadius * 0.35f);
                Vector3 bottom = new(top.x, bottomY, top.z);
                AddCylinder(vertices, triangles, top, bottom, cordRadius, 6);
            }
        }

        private static void AddRadialRopes(
            List<Vector3> vertices,
            List<int> triangles,
            BigTopEnvironmentSpec spec,
            RunWorldGenerationConfig config,
            float hubY,
            float hubRadius,
            float ropeRadius)
        {
            int ropeCount = config.BigTopCanopyRadialRopeCount;
            if (ropeCount <= 0)
            {
                return;
            }

            int segments = Mathf.Max(2, config.BigTopBulbsPerRadialString + 1);
            for (int i = 0; i < ropeCount; i++)
            {
                float angle = i / (float)ropeCount * Tau;
                Vector3 previous = Polar(angle, hubRadius, hubY);
                for (int segment = 1; segment <= segments; segment++)
                {
                    float domeT = BigTopBulbMeshBuilder.RadialDomeT(segment - 1, segments - 1);
                    float y = spec.CurtainHeight + spec.DomeHeight * domeT;
                    float radius = BigTopBulbMeshBuilder.CanopyStringRadius(spec, domeT);
                    Vector3 next = Polar(angle, radius, y);
                    AddCylinder(vertices, triangles, previous, next, ropeRadius, 6);
                    previous = next;
                }
            }
        }

        private static void AddRingRopes(
            List<Vector3> vertices,
            List<int> triangles,
            BigTopEnvironmentSpec spec,
            RunWorldGenerationConfig config,
            float ropeRadius)
        {
            int ringCount = config.BigTopCanopyRingRopeCount;
            if (ringCount <= 0)
            {
                return;
            }

            for (int ring = 0; ring < ringCount; ring++)
            {
                float domeT = BigTopBulbMeshBuilder.RingDomeT(ring, ringCount);
                float y = spec.CurtainHeight + spec.DomeHeight * domeT;
                float radius = BigTopBulbMeshBuilder.CanopyStringRadius(spec, domeT);
                AddTorus(vertices, triangles, Vector3.up * y, radius, ropeRadius, spec.SegmentCount, 6);
            }
        }

        private static Mesh BuildMesh(string name, List<Vector3> vertices, List<int> triangles, List<Color> colors = null)
        {
            if (vertices.Count == 0 || triangles.Count == 0)
            {
                return null;
            }

            var mesh = new Mesh
            {
                name = name,
                indexFormat = vertices.Count > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16,
                vertices = vertices.ToArray(),
                triangles = triangles.ToArray()
            };
            if (colors != null && colors.Count == vertices.Count)
            {
                mesh.colors = colors.ToArray();
            }

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void AddTorus(
            List<Vector3> vertices,
            List<int> triangles,
            Vector3 center,
            float majorRadius,
            float minorRadius,
            int ringSegments,
            int tubeSegments)
        {
            if (majorRadius <= 0f || minorRadius <= 0f)
            {
                return;
            }

            ringSegments = Mathf.Max(16, ringSegments);
            tubeSegments = Mathf.Max(4, tubeSegments);
            int start = vertices.Count;
            for (int ring = 0; ring <= ringSegments; ring++)
            {
                float ringAngle = ring / (float)ringSegments * Tau;
                Vector3 outward = new(Mathf.Cos(ringAngle), 0f, Mathf.Sin(ringAngle));
                for (int tube = 0; tube <= tubeSegments; tube++)
                {
                    float tubeAngle = tube / (float)tubeSegments * Tau;
                    Vector3 offset = outward * (majorRadius + Mathf.Cos(tubeAngle) * minorRadius)
                        + Vector3.up * (Mathf.Sin(tubeAngle) * minorRadius);
                    vertices.Add(center + offset);
                }
            }

            int row = tubeSegments + 1;
            for (int ring = 0; ring < ringSegments; ring++)
            {
                for (int tube = 0; tube < tubeSegments; tube++)
                {
                    int a = start + ring * row + tube;
                    int b = start + (ring + 1) * row + tube;
                    int c = start + (ring + 1) * row + tube + 1;
                    int d = start + ring * row + tube + 1;
                    AddOutwardQuad(triangles, a, b, c, d);
                }
            }
        }

        private static void AddCylinder(
            List<Vector3> vertices,
            List<int> triangles,
            Vector3 startPoint,
            Vector3 endPoint,
            float radius,
            int sides)
        {
            Vector3 axis = endPoint - startPoint;
            if (axis.sqrMagnitude <= 0.0001f || radius <= 0f)
            {
                return;
            }

            sides = Mathf.Max(4, sides);
            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, axis.normalized);
            int start = vertices.Count;
            for (int side = 0; side <= sides; side++)
            {
                float angle = side / (float)sides * Tau;
                Vector3 local = new(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                Vector3 ringOffset = rotation * local;
                vertices.Add(startPoint + ringOffset);
                vertices.Add(endPoint + ringOffset);
            }

            for (int side = 0; side < sides; side++)
            {
                int a = start + side * 2;
                int b = start + (side + 1) * 2;
                int c = start + (side + 1) * 2 + 1;
                int d = start + side * 2 + 1;
                AddOutwardQuad(triangles, a, b, c, d);
            }
        }

        private static void AddConeFrustum(
            List<Vector3> vertices,
            List<int> triangles,
            Vector3 startPoint,
            Vector3 endPoint,
            float startRadius,
            float endRadius,
            int sides)
        {
            Vector3 axis = endPoint - startPoint;
            if (axis.sqrMagnitude <= 0.0001f || startRadius <= 0f || endRadius <= 0f)
            {
                return;
            }

            sides = Mathf.Max(4, sides);
            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, axis.normalized);
            int start = vertices.Count;
            for (int side = 0; side <= sides; side++)
            {
                float angle = side / (float)sides * Tau;
                Vector3 localStart = new(Mathf.Cos(angle) * startRadius, 0f, Mathf.Sin(angle) * startRadius);
                Vector3 localEnd = new(Mathf.Cos(angle) * endRadius, 0f, Mathf.Sin(angle) * endRadius);
                vertices.Add(startPoint + rotation * localStart);
                vertices.Add(endPoint + rotation * localEnd);
            }

            for (int side = 0; side < sides; side++)
            {
                int a = start + side * 2;
                int b = start + (side + 1) * 2;
                int c = start + (side + 1) * 2 + 1;
                int d = start + side * 2 + 1;
                AddOutwardQuad(triangles, a, b, c, d);
            }
        }

        private static void AddFixtureYoke(
            List<Vector3> vertices,
            List<int> triangles,
            BigTopGodRaySource source,
            float bodyRadius)
        {
            Vector3 right = Vector3.Cross(Vector3.up, source.Direction);
            if (right.sqrMagnitude <= 0.0001f)
            {
                right = Vector3.Cross(Vector3.forward, source.Direction);
            }

            right.Normalize();
            Vector3 mountCenter = Vector3.Lerp(source.Mount, source.Start, 0.18f);
            Vector3 bodyCenter = Vector3.Lerp(source.Mount, source.Start, 0.72f);
            float armOffset = bodyRadius * 0.72f;
            float armRadius = Mathf.Max(0.035f, bodyRadius * 0.085f);
            AddCylinder(vertices, triangles, mountCenter + right * armOffset, bodyCenter + right * armOffset, armRadius, 6);
            AddCylinder(vertices, triangles, mountCenter - right * armOffset, bodyCenter - right * armOffset, armRadius, 6);
            AddTorus(vertices, triangles, source.Mount, bodyRadius * 0.58f, armRadius, 14, 4);
        }

        private static Vector3 FixtureSupportAnchor(BigTopEnvironmentSpec spec, BigTopGodRaySource source)
        {
            float anchorDomeT = 0.68f;
            float anchorY = spec.CurtainHeight + spec.DomeHeight * anchorDomeT;
            return new Vector3(source.Mount.x, Mathf.Max(source.Mount.y + 0.5f, anchorY), source.Mount.z);
        }

        private static void AddLensCap(
            List<Vector3> vertices,
            List<Color> colors,
            List<int> triangles,
            BigTopGodRaySource source,
            float radius,
            int segments,
            Color color)
        {
            if (radius <= 0f)
            {
                return;
            }

            segments = Mathf.Max(8, segments);
            Vector3 forward = source.Direction;
            Vector3 right = Vector3.Cross(Vector3.up, forward);
            if (right.sqrMagnitude <= 0.0001f)
            {
                right = Vector3.Cross(Vector3.forward, forward);
            }

            right.Normalize();
            Vector3 up = Vector3.Cross(forward, right).normalized;
            float halfDepth = radius * 0.09f;
            float rimRadius = Mathf.Sqrt(Mathf.Max(0f, radius * radius - halfDepth * halfDepth));
            Vector3 frontCenter = source.Start + forward * halfDepth;
            Vector3 backCenter = source.Start - forward * halfDepth;
            int start = vertices.Count;
            vertices.Add(frontCenter);
            vertices.Add(backCenter);
            colors.Add(color);
            colors.Add(color);
            for (int side = 0; side <= segments; side++)
            {
                float angle = side / (float)segments * Tau;
                Vector3 offset = (right * Mathf.Cos(angle) + up * Mathf.Sin(angle)) * rimRadius;
                vertices.Add(frontCenter + offset);
                vertices.Add(backCenter + offset);
                colors.Add(color);
                colors.Add(color);
            }

            for (int side = 0; side < segments; side++)
            {
                int frontA = start + 2 + side * 2;
                int backA = frontA + 1;
                int frontB = start + 2 + (side + 1) * 2;
                int backB = frontB + 1;
                triangles.Add(start);
                triangles.Add(frontA);
                triangles.Add(frontB);
                triangles.Add(start + 1);
                triangles.Add(backB);
                triangles.Add(backA);
                AddOutwardQuad(triangles, frontA, backA, backB, frontB);
            }
        }

        private static Vector3 Polar(float angle, float radius, float y)
        {
            return new Vector3(Mathf.Cos(angle) * radius, y, Mathf.Sin(angle) * radius);
        }

        private static void AddInwardQuad(List<int> triangles, int a, int b, int c, int d)
        {
            triangles.Add(a);
            triangles.Add(b);
            triangles.Add(c);
            triangles.Add(a);
            triangles.Add(c);
            triangles.Add(d);
        }

        private static void AddOutwardQuad(List<int> triangles, int a, int b, int c, int d)
        {
            triangles.Add(a);
            triangles.Add(c);
            triangles.Add(b);
            triangles.Add(a);
            triangles.Add(d);
            triangles.Add(c);
        }
    }
}
