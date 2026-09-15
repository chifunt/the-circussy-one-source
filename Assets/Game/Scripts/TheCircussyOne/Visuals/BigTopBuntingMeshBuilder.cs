using System.Collections.Generic;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;
using UnityEngine;
using UnityEngine.Rendering;

namespace TheCircussyOne.Visuals
{
    public static class BigTopBuntingMeshBuilder
    {
        private const float Tau = Mathf.PI * 2f;

        public static Mesh BuildBuntingStrings(BigTopEnvironmentSpec spec, RunWorldGenerationConfig config)
        {
            if (!BuntingEnabled(spec, config) || config.BigTopBuntingStringThickness <= 0f)
            {
                return null;
            }

            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            float stringRadius = config.BigTopBuntingStringThickness * 0.5f;
            for (int ring = 0; ring < config.BigTopBuntingRingCount; ring++)
            {
                float domeT = BuntingDomeT(config, ring);
                float y = spec.CurtainHeight + spec.DomeHeight * domeT;
                float radius = BigTopBulbMeshBuilder.CanopyStringRadius(spec, domeT);
                AddTorus(vertices, triangles, Vector3.up * y, radius, stringRadius, spec.SegmentCount, 4);
            }

            return BuildMesh("Big Top Bunting String Mesh", vertices, triangles);
        }

        public static Mesh BuildBuntingFlagsPrimary(BigTopEnvironmentSpec spec, RunWorldGenerationConfig config)
        {
            return BuildBuntingFlags(spec, config, primary: true);
        }

        public static Mesh BuildBuntingFlagsSecondary(BigTopEnvironmentSpec spec, RunWorldGenerationConfig config)
        {
            return BuildBuntingFlags(spec, config, primary: false);
        }

        public static Mesh BuildCurtainBannerPanels(BigTopEnvironmentSpec spec, RunWorldGenerationConfig config)
        {
            if (!BannersEnabled(spec, config))
            {
                return null;
            }

            var vertices = new List<Vector3>(config.BigTopCurtainBannerCount * 4);
            var uvs = new List<Vector2>(config.BigTopCurtainBannerCount * 4);
            var triangles = new List<int>(config.BigTopCurtainBannerCount * 12);
            AddCurtainBannerRects(spec, config, vertices, uvs, triangles, trim: false);
            return BuildMesh("Big Top Curtain Banner Panel Mesh", vertices, triangles, uvs);
        }

        public static Mesh BuildCurtainBannerTrim(BigTopEnvironmentSpec spec, RunWorldGenerationConfig config)
        {
            if (!BannersEnabled(spec, config))
            {
                return null;
            }

            var vertices = new List<Vector3>(config.BigTopCurtainBannerCount * 16);
            var uvs = new List<Vector2>(config.BigTopCurtainBannerCount * 16);
            var triangles = new List<int>(config.BigTopCurtainBannerCount * 48);
            AddCurtainBannerRects(spec, config, vertices, uvs, triangles, trim: true);
            return BuildMesh("Big Top Curtain Banner Trim Mesh", vertices, triangles, uvs);
        }

        private static Mesh BuildBuntingFlags(BigTopEnvironmentSpec spec, RunWorldGenerationConfig config, bool primary)
        {
            if (!BuntingEnabled(spec, config)
                || config.BigTopBuntingFlagsPerRing <= 0
                || config.BigTopBuntingFlagWidth <= 0f
                || config.BigTopBuntingFlagDrop <= 0f)
            {
                return null;
            }

            int maxFlags = config.BigTopBuntingRingCount * config.BigTopBuntingFlagsPerRing;
            var vertices = new List<Vector3>(maxFlags * 3 / 2);
            var triangles = new List<int>(maxFlags * 6 / 2);
            for (int ring = 0; ring < config.BigTopBuntingRingCount; ring++)
            {
                float domeT = BuntingDomeT(config, ring);
                float y = spec.CurtainHeight + spec.DomeHeight * domeT;
                float radius = BigTopBulbMeshBuilder.CanopyStringRadius(spec, domeT);
                float maxHalfWidth = Mathf.Sqrt(Mathf.Max(0f, spec.Radius * spec.Radius - radius * radius)) - 0.05f;
                float halfWidth = Mathf.Min(config.BigTopBuntingFlagWidth * 0.5f, Mathf.Max(0f, maxHalfWidth));
                float drop = Mathf.Min(config.BigTopBuntingFlagDrop, Mathf.Max(0.05f, y - 0.05f));
                if (halfWidth <= 0f || drop <= 0f)
                {
                    continue;
                }

                float turnOffset = ring * 0.5f / Mathf.Max(1, config.BigTopBuntingFlagsPerRing);
                for (int flag = 0; flag < config.BigTopBuntingFlagsPerRing; flag++)
                {
                    bool isPrimary = ((ring * config.BigTopBuntingFlagsPerRing + flag) & 1) == 0;
                    if (isPrimary != primary)
                    {
                        continue;
                    }

                    float angle = ((flag / (float)config.BigTopBuntingFlagsPerRing) + turnOffset) * Tau;
                    Vector3 tangent = new(-Mathf.Sin(angle), 0f, Mathf.Cos(angle));
                    Vector3 topCenter = Polar(angle, radius, y);
                    AddPennant(vertices, triangles, topCenter, tangent, halfWidth, drop);
                }
            }

            return BuildMesh(primary ? "Big Top Primary Bunting Flag Mesh" : "Big Top Secondary Bunting Flag Mesh", vertices, triangles);
        }

        private static void AddCurtainBannerRects(
            BigTopEnvironmentSpec spec,
            RunWorldGenerationConfig config,
            List<Vector3> vertices,
            List<Vector2> uvs,
            List<int> triangles,
            bool trim)
        {
            float width = Mathf.Min(config.BigTopCurtainBannerWidth, spec.Radius * 0.45f);
            float valanceClearance = config.BigTopCurtainValanceEnabled
                ? config.BigTopCurtainValanceHeight + config.BigTopCurtainValanceScallopDrop + 1.25f
                : 1.25f;
            float topOffset = Mathf.Max(config.BigTopCurtainBannerTopOffset, valanceClearance);
            float topY = Mathf.Clamp(spec.CurtainHeight - topOffset, 0.1f, spec.CurtainHeight);
            float height = Mathf.Min(config.BigTopCurtainBannerHeight, Mathf.Max(0.05f, topY - 0.05f));
            float centerY = topY - height * 0.5f;
            float halfWidth = width * 0.5f;
            float radius = Mathf.Max(0.1f, spec.Radius - config.BigTopCurtainBannerInwardInset - halfWidth - 0.05f);
            float trimThickness = Mathf.Max(0.08f, Mathf.Min(width, height) * 0.055f);
            float innerWidth = Mathf.Max(0.01f, width - trimThickness * 2f);
            float innerHeight = Mathf.Max(0.01f, height - trimThickness * 2f);

            for (int banner = 0; banner < config.BigTopCurtainBannerCount; banner++)
            {
                float angle = CurtainBannerAngle(config, banner);
                Vector3 tangent = new(-Mathf.Sin(angle), 0f, Mathf.Cos(angle));
                Vector3 center = Polar(angle, radius, centerY);
                if (!trim)
                {
                    AddRectangle(vertices, uvs, triangles, center, tangent, Vector3.up, innerWidth, innerHeight);
                    continue;
                }

                AddRectangle(vertices, uvs, triangles, center + Vector3.up * (height * 0.5f - trimThickness * 0.5f), tangent, Vector3.up, width, trimThickness);
                AddRectangle(vertices, uvs, triangles, center - Vector3.up * (height * 0.5f - trimThickness * 0.5f), tangent, Vector3.up, width, trimThickness);
                AddRectangle(vertices, uvs, triangles, center - tangent * (width * 0.5f - trimThickness * 0.5f), tangent, Vector3.up, trimThickness, innerHeight);
                AddRectangle(vertices, uvs, triangles, center + tangent * (width * 0.5f - trimThickness * 0.5f), tangent, Vector3.up, trimThickness, innerHeight);
            }
        }

        private static float CurtainBannerAngle(RunWorldGenerationConfig config, int banner)
        {
            int bannerCount = Mathf.Max(1, config.BigTopCurtainBannerCount);
            int poleCount = Mathf.Max(0, config.BigTopPerimeterPoleCount);
            float polePhaseOffset = poleCount > 0 ? 0.5f / poleCount : 0f;
            return (banner / (float)bannerCount + polePhaseOffset) * Tau;
        }

        private static float BuntingDomeT(RunWorldGenerationConfig config, int ring)
        {
            float t = (ring + 1f) / (Mathf.Max(1, config.BigTopBuntingRingCount) + 1f);
            return Mathf.Lerp(config.BigTopBuntingLowerDomePercent, config.BigTopBuntingUpperDomePercent, t);
        }

        private static bool BuntingEnabled(BigTopEnvironmentSpec spec, RunWorldGenerationConfig config)
        {
            return spec.IsValid && config != null && config.BigTopBuntingEnabled && config.BigTopBuntingRingCount > 0;
        }

        private static bool BannersEnabled(BigTopEnvironmentSpec spec, RunWorldGenerationConfig config)
        {
            return spec.IsValid
                && config != null
                && config.BigTopCurtainBannersEnabled
                && config.BigTopCurtainBannerCount > 0
                && config.BigTopCurtainBannerWidth > 0f
                && config.BigTopCurtainBannerHeight > 0f;
        }

        private static Mesh BuildMesh(string name, List<Vector3> vertices, List<int> triangles)
        {
            return BuildMesh(name, vertices, triangles, null);
        }

        private static Mesh BuildMesh(string name, List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
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
            if (uvs != null && uvs.Count == vertices.Count)
            {
                mesh.uv = uvs.ToArray();
            }

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void AddPennant(
            List<Vector3> vertices,
            List<int> triangles,
            Vector3 topCenter,
            Vector3 tangent,
            float halfWidth,
            float drop)
        {
            int index = vertices.Count;
            vertices.Add(topCenter - tangent * halfWidth);
            vertices.Add(topCenter + tangent * halfWidth);
            vertices.Add(topCenter - Vector3.up * drop);
            AddDoubleSidedTriangle(triangles, index, 0, 1, 2);
        }

        private static void AddRectangle(
            List<Vector3> vertices,
            List<int> triangles,
            Vector3 center,
            Vector3 right,
            Vector3 up,
            float width,
            float height)
        {
            int index = vertices.Count;
            Vector3 halfRight = right.normalized * (width * 0.5f);
            Vector3 halfUp = up.normalized * (height * 0.5f);
            vertices.Add(center - halfRight + halfUp);
            vertices.Add(center + halfRight + halfUp);
            vertices.Add(center + halfRight - halfUp);
            vertices.Add(center - halfRight - halfUp);
            AddDoubleSidedQuad(triangles, index, 0, 1, 2, 3);
        }

        private static void AddRectangle(
            List<Vector3> vertices,
            List<Vector2> uvs,
            List<int> triangles,
            Vector3 center,
            Vector3 right,
            Vector3 up,
            float width,
            float height)
        {
            AddRectangle(vertices, triangles, center, right, up, width, height);
            if (uvs == null)
            {
                return;
            }

            uvs.Add(new Vector2(0f, 1f));
            uvs.Add(new Vector2(1f, 1f));
            uvs.Add(new Vector2(1f, 0f));
            uvs.Add(new Vector2(0f, 0f));
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
                    AddQuad(triangles, a, b, c, d);
                }
            }
        }

        private static Vector3 Polar(float angle, float radius, float y)
        {
            return new Vector3(Mathf.Cos(angle) * radius, y, Mathf.Sin(angle) * radius);
        }

        private static void AddDoubleSidedTriangle(List<int> triangles, int index, int a, int b, int c)
        {
            triangles.Add(index + a);
            triangles.Add(index + b);
            triangles.Add(index + c);
            triangles.Add(index + a);
            triangles.Add(index + c);
            triangles.Add(index + b);
        }

        private static void AddDoubleSidedQuad(List<int> triangles, int index, int a, int b, int c, int d)
        {
            triangles.Add(index + a);
            triangles.Add(index + b);
            triangles.Add(index + c);
            triangles.Add(index + a);
            triangles.Add(index + c);
            triangles.Add(index + d);
            triangles.Add(index + a);
            triangles.Add(index + c);
            triangles.Add(index + b);
            triangles.Add(index + a);
            triangles.Add(index + d);
            triangles.Add(index + c);
        }

        private static void AddQuad(List<int> triangles, int a, int b, int c, int d)
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
