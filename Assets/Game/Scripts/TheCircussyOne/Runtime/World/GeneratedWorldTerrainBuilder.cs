using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace TheCircussyOne.Runtime
{
    public readonly struct GeneratedWorldTerrainBuildSettings
    {
        public GeneratedWorldTerrainBuildSettings(Material material, int chunkQuadsPerSide, int layer = 0)
            : this(material, null, null, chunkQuadsPerSide, layer)
        {
        }

        public GeneratedWorldTerrainBuildSettings(Material material, Material wallMaterial, int chunkQuadsPerSide, int layer = 0)
            : this(material, wallMaterial, null, chunkQuadsPerSide, layer)
        {
        }

        public GeneratedWorldTerrainBuildSettings(
            Material material,
            Material wallMaterial,
            Material organicMaterial,
            int chunkQuadsPerSide,
            int layer = 0)
            : this(
                new[] { material },
                new[] { wallMaterial },
                new[] { organicMaterial },
                chunkQuadsPerSide,
                0,
                1,
                null,
                null,
                null,
                1.15f,
                layer)
        {
        }

        public GeneratedWorldTerrainBuildSettings(
            GeneratedWorldSurfaceMaterialSet surfaceMaterials,
            int chunkQuadsPerSide,
            int layer = 0)
            : this(
                surfaceMaterials != null ? surfaceMaterials.TopMaterials : null,
                surfaceMaterials != null ? surfaceMaterials.WallMaterials : null,
                surfaceMaterials != null ? surfaceMaterials.OrganicMaterials : null,
                chunkQuadsPerSide,
                surfaceMaterials != null ? surfaceMaterials.SurfaceZoneSeed : 0,
                surfaceMaterials != null ? surfaceMaterials.SurfaceZoneActNumber : 1,
                surfaceMaterials != null ? surfaceMaterials.TopZoneTints : null,
                surfaceMaterials != null ? surfaceMaterials.OrganicZoneTints : null,
                surfaceMaterials != null ? surfaceMaterials.WallZoneTints : null,
                surfaceMaterials != null ? surfaceMaterials.SurfaceZoneBlendSharpness : 1.15f,
                layer)
        {
        }

        private GeneratedWorldTerrainBuildSettings(
            Material[] materials,
            Material[] wallMaterials,
            Material[] organicMaterials,
            int chunkQuadsPerSide,
            int surfaceZoneSeed,
            int surfaceZoneActNumber,
            Color[] topZoneTints,
            Color[] organicZoneTints,
            Color[] wallZoneTints,
            float surfaceZoneBlendSharpness,
            int layer)
        {
            Materials = NormalizeMaterials(materials);
            WallMaterials = NormalizeMaterials(wallMaterials);
            OrganicMaterials = NormalizeMaterials(organicMaterials);
            Material = Materials[0];
            WallMaterial = WallMaterials[0];
            OrganicMaterial = OrganicMaterials[0];
            ChunkQuadsPerSide = Mathf.Max(1, chunkQuadsPerSide);
            SurfaceZoneSeed = surfaceZoneSeed;
            SurfaceZoneActNumber = Mathf.Max(1, surfaceZoneActNumber);
            TopZoneTints = NormalizeTints(topZoneTints, Materials.Length);
            OrganicZoneTints = NormalizeTints(organicZoneTints, OrganicMaterials.Length);
            WallZoneTints = NormalizeTints(wallZoneTints, WallMaterials.Length);
            SurfaceZoneBlendSharpness = Mathf.Clamp(surfaceZoneBlendSharpness, 0.25f, 4f);
            Layer = Mathf.Max(0, layer);
        }

        public Material Material { get; }
        public Material WallMaterial { get; }
        public Material OrganicMaterial { get; }
        public Material[] Materials { get; }
        public Material[] WallMaterials { get; }
        public Material[] OrganicMaterials { get; }
        public Color[] TopZoneTints { get; }
        public Color[] OrganicZoneTints { get; }
        public Color[] WallZoneTints { get; }
        public int ChunkQuadsPerSide { get; }
        public int SurfaceZoneSeed { get; }
        public int SurfaceZoneActNumber { get; }
        public int SurfaceZoneCount => Materials.Length;
        public float SurfaceZoneBlendSharpness { get; }
        public int Layer { get; }

        private static Material[] NormalizeMaterials(Material[] materials)
        {
            return materials != null && materials.Length > 0
                ? materials
                : new Material[] { null };
        }

        private static Color[] NormalizeTints(Color[] tints, int length)
        {
            int count = Mathf.Max(1, length);
            var normalized = new Color[count];
            for (int i = 0; i < count; i++)
            {
                normalized[i] = tints != null && i < tints.Length ? tints[i] : Color.white;
                normalized[i].a = 1f;
            }

            return normalized;
        }
    }

    public readonly struct GeneratedWorldTerrainBuildResult
    {
        public GeneratedWorldTerrainBuildResult(
            bool success,
            GameObject root,
            int chunkCount,
            Bounds bounds,
            string warning)
        {
            Success = success;
            Root = root;
            ChunkCount = Mathf.Max(0, chunkCount);
            Bounds = bounds;
            Warning = warning ?? string.Empty;
        }

        public bool Success { get; }
        public GameObject Root { get; }
        public int ChunkCount { get; }
        public Bounds Bounds { get; }
        public string Warning { get; }
    }

    public static class GeneratedWorldTerrainBuilder
    {
        private const string RootName = "Generated World Terrain";
        private const float RampVisualSkirtDepth = 2f;
        private const float RampVisualSkirtOverlap = 0.125f;
        private static Material fallbackMaterial;
        private static Material fallbackWallMaterial;

        public static GeneratedWorldTerrainBuildResult Build(
            GeneratedWorldMap map,
            GeneratedWorldTerrainBuildSettings settings)
        {
            if (map == null || map.Width < 2 || map.Depth < 2 || map.Samples.Count < map.Width * map.Depth)
            {
                return new GeneratedWorldTerrainBuildResult(false, null, 0, default, "Generated world map is missing or too small.");
            }

            GameObject root = new($"{RootName} Act {map.ActNumber}");
            root.layer = settings.Layer;
            Material[] materials = ResolveMaterialArray(settings.Materials, wall: false);
            Material[] wallMaterials = ResolveMaterialArray(settings.WallMaterials, wall: true);
            Material[] organicMaterials = ResolveMaterialArray(settings.OrganicMaterials, wall: false);

            int chunkCount = 0;
            int chunkQuads = Mathf.Max(1, settings.ChunkQuadsPerSide);
            for (int z = 0; z < map.Depth - 1; z += chunkQuads)
            {
                int endZ = Mathf.Min(map.Depth - 1, z + chunkQuads);
                for (int x = 0; x < map.Width - 1; x += chunkQuads)
                {
                    int endX = Mathf.Min(map.Width - 1, x + chunkQuads);
                    if (CreateChunk(map, settings, materials, wallMaterials, organicMaterials, root.transform, x, endX, z, endZ, chunkCount))
                    {
                        chunkCount++;
                    }
                }
            }

            return new GeneratedWorldTerrainBuildResult(true, root, chunkCount, map.Bounds, string.Empty);
        }

        public static async UniTask<GeneratedWorldTerrainBuildResult> BuildAsync(
            GeneratedWorldMap map,
            GeneratedWorldTerrainBuildSettings settings,
            int chunksPerFrame,
            WorldLoadTimingDiagnostics timing = null)
        {
            if (map == null || map.Width < 2 || map.Depth < 2 || map.Samples.Count < map.Width * map.Depth)
            {
                return new GeneratedWorldTerrainBuildResult(false, null, 0, default, "Generated world map is missing or too small.");
            }

            timing ??= WorldLoadTimingDiagnostics.Disabled;
            GameObject root = new($"{RootName} Act {map.ActNumber}");
            root.layer = settings.Layer;
            Material[] materials = ResolveMaterialArray(settings.Materials, wall: false);
            Material[] wallMaterials = ResolveMaterialArray(settings.WallMaterials, wall: true);
            Material[] organicMaterials = ResolveMaterialArray(settings.OrganicMaterials, wall: false);

            int chunkCount = 0;
            int chunksBuiltThisFrame = 0;
            int chunkQuads = Mathf.Max(1, settings.ChunkQuadsPerSide);
            int yieldEvery = Mathf.Max(1, chunksPerFrame);
            double maxChunkMs = 0.0;
            int slowChunks = 0;
            for (int z = 0; z < map.Depth - 1; z += chunkQuads)
            {
                int endZ = Mathf.Min(map.Depth - 1, z + chunkQuads);
                for (int x = 0; x < map.Width - 1; x += chunkQuads)
                {
                    int endX = Mathf.Min(map.Width - 1, x + chunkQuads);
                    long startedAt = timing.Enabled ? WorldLoadTimingDiagnostics.Timestamp() : 0L;
                    if (CreateChunk(map, settings, materials, wallMaterials, organicMaterials, root.transform, x, endX, z, endZ, chunkCount))
                    {
                        if (timing.Enabled)
                        {
                            double elapsedMs = WorldLoadTimingDiagnostics.ElapsedMilliseconds(startedAt);
                            if (elapsedMs > maxChunkMs)
                            {
                                maxChunkMs = elapsedMs;
                            }

                            if (timing.ShouldLogStage(elapsedMs, slowOnly: true))
                            {
                                slowChunks++;
                                timing.LogStage(
                                    "Terrain.Chunk",
                                    elapsedMs,
                                    $"chunk={chunkCount} x={x}-{endX} z={z}-{endZ}",
                                    slowOnly: true);
                            }
                        }

                        chunkCount++;
                        chunksBuiltThisFrame++;
                    }

                    if (chunksBuiltThisFrame >= yieldEvery)
                    {
                        chunksBuiltThisFrame = 0;
                        await UniTask.Yield(PlayerLoopTiming.Update);
                    }
                }
            }

            timing.LogStage(
                "Terrain.ChunksSummary",
                maxChunkMs,
                $"chunks={chunkCount} slowChunks={slowChunks} chunksPerFrame={yieldEvery}");

            return new GeneratedWorldTerrainBuildResult(true, root, chunkCount, map.Bounds, string.Empty);
        }

        public static int BuildConstructedRampMeshes(
            Transform parent,
            GeneratedWorldMap map,
            IReadOnlyList<BlockyCircusGroundsPiece> pieces,
            Material material,
            int layer = 0)
        {
            return BuildConstructedRampMeshes(parent, map, pieces, material, material, layer);
        }

        public static int BuildConstructedRampMeshes(
            Transform parent,
            GeneratedWorldMap map,
            IReadOnlyList<BlockyCircusGroundsPiece> pieces,
            Material rampMaterial,
            Material wallMaterial,
            int layer = 0)
        {
            if (parent == null || map == null || pieces == null || pieces.Count == 0)
            {
                return 0;
            }

            var root = new GameObject("Constructed Ramp Meshes");
            root.layer = Mathf.Max(0, layer);
            root.transform.SetParent(parent, false);

            Material resolvedRampMaterial = ResolveMaterial(rampMaterial);
            Material resolvedWallMaterial = ResolveWallMaterial(wallMaterial);
            int rampIndex = 0;
            for (int i = 0; i < pieces.Count; i++)
            {
                BlockyCircusGroundsPiece piece = pieces[i];
                if (piece.Kind != BlockyCircusGroundsPieceKind.RampAisle)
                {
                    continue;
                }

                BuildRampWedge(root.transform, map, piece, resolvedRampMaterial, resolvedWallMaterial, rampIndex, root.layer);
                rampIndex++;
            }

            return rampIndex;
        }

        public static int BuildConstructedRampMeshes(
            Transform parent,
            GeneratedWorldMap map,
            IReadOnlyList<BlockyCircusGroundsPiece> pieces,
            GeneratedWorldSurfaceMaterialSet surfaceMaterials,
            int layer = 0)
        {
            if (parent == null || map == null || pieces == null || pieces.Count == 0)
            {
                return 0;
            }

            var root = new GameObject("Constructed Ramp Meshes");
            root.layer = Mathf.Max(0, layer);
            root.transform.SetParent(parent, false);

            Material[] rampMaterials = ResolveMaterialArray(surfaceMaterials?.RampMaterials, wall: false);
            Material[] wallMaterials = ResolveMaterialArray(surfaceMaterials?.WallMaterials, wall: true);
            int zoneCount = Mathf.Max(rampMaterials.Length, wallMaterials.Length);
            int rampIndex = 0;
            for (int i = 0; i < pieces.Count; i++)
            {
                BlockyCircusGroundsPiece piece = pieces[i];
                if (piece.Kind != BlockyCircusGroundsPieceKind.RampAisle)
                {
                    continue;
                }

                int zoneIndex = SelectSurfaceZoneIndex(
                    map,
                    piece.Center,
                    surfaceMaterials?.SurfaceZoneSeed ?? 0,
                    surfaceMaterials?.SurfaceZoneActNumber ?? 1,
                    zoneCount);
                BuildRampWedge(
                    root.transform,
                    map,
                    piece,
                    MaterialAt(rampMaterials, zoneIndex),
                    MaterialAt(wallMaterials, zoneIndex),
                    rampIndex,
                    root.layer);
                rampIndex++;
            }

            return rampIndex;
        }

        public static async UniTask<int> BuildConstructedRampMeshesAsync(
            Transform parent,
            GeneratedWorldMap map,
            IReadOnlyList<BlockyCircusGroundsPiece> pieces,
            Material material,
            int layer = 0,
            int rampsPerFrame = 2,
            WorldLoadTimingDiagnostics timing = null)
        {
            return await BuildConstructedRampMeshesAsync(
                parent,
                map,
                pieces,
                material,
                material,
                layer,
                rampsPerFrame,
                timing);
        }

        public static async UniTask<int> BuildConstructedRampMeshesAsync(
            Transform parent,
            GeneratedWorldMap map,
            IReadOnlyList<BlockyCircusGroundsPiece> pieces,
            Material rampMaterial,
            Material wallMaterial,
            int layer = 0,
            int rampsPerFrame = 2,
            WorldLoadTimingDiagnostics timing = null)
        {
            if (parent == null || map == null || pieces == null || pieces.Count == 0)
            {
                return 0;
            }

            timing ??= WorldLoadTimingDiagnostics.Disabled;
            var root = new GameObject("Constructed Ramp Meshes");
            root.layer = Mathf.Max(0, layer);
            root.transform.SetParent(parent, false);

            Material resolvedRampMaterial = ResolveMaterial(rampMaterial);
            Material resolvedWallMaterial = ResolveWallMaterial(wallMaterial);
            int rampIndex = 0;
            int rampsBuiltThisFrame = 0;
            int yieldEvery = Mathf.Max(1, rampsPerFrame);
            double maxRampMs = 0.0;
            int slowRamps = 0;
            for (int i = 0; i < pieces.Count; i++)
            {
                BlockyCircusGroundsPiece piece = pieces[i];
                if (piece.Kind != BlockyCircusGroundsPieceKind.RampAisle)
                {
                    continue;
                }

                long startedAt = timing.Enabled ? WorldLoadTimingDiagnostics.Timestamp() : 0L;
                BuildRampWedge(root.transform, map, piece, resolvedRampMaterial, resolvedWallMaterial, rampIndex, root.layer);
                if (timing.Enabled)
                {
                    double elapsedMs = WorldLoadTimingDiagnostics.ElapsedMilliseconds(startedAt);
                    if (elapsedMs > maxRampMs)
                    {
                        maxRampMs = elapsedMs;
                    }

                    if (timing.ShouldLogStage(elapsedMs, slowOnly: true))
                    {
                        slowRamps++;
                        timing.LogStage(
                            "Terrain.RampWedge",
                            elapsedMs,
                            $"ramp={rampIndex} piece={i}",
                            slowOnly: true);
                    }
                }

                rampIndex++;
                rampsBuiltThisFrame++;
                if (rampsBuiltThisFrame >= yieldEvery)
                {
                    rampsBuiltThisFrame = 0;
                    await UniTask.Yield(PlayerLoopTiming.Update);
                }
            }

            timing.LogStage(
                "Terrain.RampSummary",
                maxRampMs,
                $"ramps={rampIndex} slowRamps={slowRamps} rampsPerFrame={yieldEvery}");
            return rampIndex;
        }

        public static async UniTask<int> BuildConstructedRampMeshesAsync(
            Transform parent,
            GeneratedWorldMap map,
            IReadOnlyList<BlockyCircusGroundsPiece> pieces,
            GeneratedWorldSurfaceMaterialSet surfaceMaterials,
            int layer = 0,
            int rampsPerFrame = 2,
            WorldLoadTimingDiagnostics timing = null)
        {
            if (parent == null || map == null || pieces == null || pieces.Count == 0)
            {
                return 0;
            }

            timing ??= WorldLoadTimingDiagnostics.Disabled;
            var root = new GameObject("Constructed Ramp Meshes");
            root.layer = Mathf.Max(0, layer);
            root.transform.SetParent(parent, false);

            Material[] rampMaterials = ResolveMaterialArray(surfaceMaterials?.RampMaterials, wall: false);
            Material[] wallMaterials = ResolveMaterialArray(surfaceMaterials?.WallMaterials, wall: true);
            int zoneCount = Mathf.Max(rampMaterials.Length, wallMaterials.Length);
            int rampIndex = 0;
            int rampsBuiltThisFrame = 0;
            int yieldEvery = Mathf.Max(1, rampsPerFrame);
            double maxRampMs = 0.0;
            int slowRamps = 0;
            for (int i = 0; i < pieces.Count; i++)
            {
                BlockyCircusGroundsPiece piece = pieces[i];
                if (piece.Kind != BlockyCircusGroundsPieceKind.RampAisle)
                {
                    continue;
                }

                long startedAt = timing.Enabled ? WorldLoadTimingDiagnostics.Timestamp() : 0L;
                int zoneIndex = SelectSurfaceZoneIndex(
                    map,
                    piece.Center,
                    surfaceMaterials?.SurfaceZoneSeed ?? 0,
                    surfaceMaterials?.SurfaceZoneActNumber ?? 1,
                    zoneCount);
                BuildRampWedge(
                    root.transform,
                    map,
                    piece,
                    MaterialAt(rampMaterials, zoneIndex),
                    MaterialAt(wallMaterials, zoneIndex),
                    rampIndex,
                    root.layer);
                if (timing.Enabled)
                {
                    double elapsedMs = WorldLoadTimingDiagnostics.ElapsedMilliseconds(startedAt);
                    if (elapsedMs > maxRampMs)
                    {
                        maxRampMs = elapsedMs;
                    }

                    if (timing.ShouldLogStage(elapsedMs, slowOnly: true))
                    {
                        slowRamps++;
                        timing.LogStage(
                            "Terrain.RampWedge",
                            elapsedMs,
                            $"ramp={rampIndex} piece={i} zone={zoneIndex}",
                            slowOnly: true);
                    }
                }

                rampIndex++;
                rampsBuiltThisFrame++;
                if (rampsBuiltThisFrame >= yieldEvery)
                {
                    rampsBuiltThisFrame = 0;
                    await UniTask.Yield(PlayerLoopTiming.Update);
                }
            }

            timing.LogStage(
                "Terrain.RampSummary",
                maxRampMs,
                $"ramps={rampIndex} slowRamps={slowRamps} rampsPerFrame={yieldEvery}");
            return rampIndex;
        }

        public static GameObject BuildSmoothBoundaryRim(
            Transform parent,
            GeneratedWorldMap map,
            Material material,
            float rimWidth,
            int segmentCount,
            int layer = 0)
        {
            if (parent == null || map == null)
            {
                return null;
            }

            int segments = Mathf.Clamp(segmentCount, 48, 256);
            float outerRadius = map.PlayableRadius + map.GridSpacing * 0.75f;
            float innerRadius = Mathf.Max(map.GridSpacing, outerRadius - Mathf.Max(1f, rimWidth));
            float topY = 0.12f;
            float bottomY = -Mathf.Max(3f, map.GridSpacing * 1.5f);

            var vertices = new List<Vector3>(segments * 12);
            var normals = new List<Vector3>(segments * 12);
            var uvs = new List<Vector2>(segments * 12);
            var colors = new List<Color>(segments * 12);
            var triangles = new List<int>(segments * 18);

            for (int i = 0; i < segments; i++)
            {
                float a = i * Mathf.PI * 2f / segments;
                float b = (i + 1) * Mathf.PI * 2f / segments;
                Vector3 innerA = CirclePoint(innerRadius, topY, a);
                Vector3 innerB = CirclePoint(innerRadius, topY, b);
                Vector3 outerA = CirclePoint(outerRadius, topY, a);
                Vector3 outerB = CirclePoint(outerRadius, topY, b);
                Vector3 outerBottomA = CirclePoint(outerRadius, bottomY, a);
                Vector3 outerBottomB = CirclePoint(outerRadius, bottomY, b);

                AddMeshQuad(innerA, innerB, outerB, outerA);
                AddMeshQuad(outerBottomA, outerA, outerB, outerBottomB);
                AddMeshQuad(outerBottomA, outerBottomB, outerB, outerA);
            }

            var mesh = new Mesh
            {
                name = "Smooth Boundary Rim Mesh",
                indexFormat = vertices.Count > ushort.MaxValue ? IndexFormat.UInt32 : IndexFormat.UInt16
            };
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetColors(colors);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();

            var rim = new GameObject("Smooth Boundary Rim");
            rim.layer = Mathf.Max(0, layer);
            rim.transform.SetParent(parent, worldPositionStays: false);
            var meshFilter = rim.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;
            var meshRenderer = rim.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = ResolveMaterial(material);
            var meshCollider = rim.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;
            return rim;

            static Vector3 CirclePoint(float radius, float y, float angle)
            {
                return new Vector3(Mathf.Cos(angle) * radius, y, Mathf.Sin(angle) * radius);
            }

            void AddVertex(Vector3 position, Vector3 normal)
            {
                vertices.Add(position);
                normals.Add(normal.sqrMagnitude > 0.0001f ? normal.normalized : Vector3.up);
                uvs.Add(new Vector2(
                    position.x / Mathf.Max(0.01f, map.GridSpacing),
                    position.z / Mathf.Max(0.01f, map.GridSpacing)));
                colors.Add(Color.white);
            }

            void AddMeshQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
            {
                Vector3 normal = Vector3.Cross(b - a, c - a);
                if (normal.sqrMagnitude < 0.0001f)
                {
                    normal = Vector3.up;
                }

                int baseIndex = vertices.Count;
                AddVertex(a, normal);
                AddVertex(b, normal);
                AddVertex(c, normal);
                AddVertex(d, normal);

                triangles.Add(baseIndex);
                triangles.Add(baseIndex + 1);
                triangles.Add(baseIndex + 2);
                triangles.Add(baseIndex);
                triangles.Add(baseIndex + 2);
                triangles.Add(baseIndex + 3);
            }
        }

        private static bool CreateChunk(
            GeneratedWorldMap map,
            GeneratedWorldTerrainBuildSettings settings,
            Material[] materials,
            Material[] wallMaterials,
            Material[] organicMaterials,
            Transform parent,
            int startX,
            int endX,
            int startZ,
            int endZ,
            int chunkIndex)
        {
            Mesh mesh = BuildChunkMesh(map, settings, startX, endX, startZ, endZ);
            if (mesh.vertexCount == 0)
            {
                return false;
            }

            var chunk = new GameObject($"Generated Terrain Chunk {chunkIndex:000}");
            chunk.layer = settings.Layer;
            chunk.transform.SetParent(parent, false);

            var filter = chunk.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            var renderer = chunk.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = new[]
            {
                MaterialAt(materials, 0),
                MaterialAt(wallMaterials, 0),
                MaterialAt(organicMaterials, 0)
            };

            var collider = chunk.AddComponent<MeshCollider>();
            collider.sharedMesh = mesh;
            return true;
        }

        private static void BuildRampWedge(
            Transform parent,
            GeneratedWorldMap map,
            BlockyCircusGroundsPiece piece,
            Material rampMaterial,
            Material wallMaterial,
            int index,
            int layer)
        {
            Vector3 direction = new(piece.Direction.x, 0f, piece.Direction.y);
            if (direction.sqrMagnitude < 0.0001f)
            {
                direction = Vector3.forward;
            }

            direction.Normalize();
            Vector3 side = new(-direction.z, 0f, direction.x);
            float halfWidth = Mathf.Max(0.1f, piece.Size.x * 0.5f);
            float halfLength = Mathf.Max(0.1f, piece.Size.z * 0.5f);
            float heightDelta = Mathf.Max(0.1f, piece.Size.y);
            float lowY = -heightDelta * 0.5f;
            float highY = heightDelta * 0.5f;

            Vector3 footCenter = -direction * halfLength;
            Vector3 topCenter = direction * halfLength;
            Vector3 lowLeft = footCenter - side * halfWidth + Vector3.up * lowY;
            Vector3 lowRight = footCenter + side * halfWidth + Vector3.up * lowY;
            Vector3 highLeft = topCenter - side * halfWidth + Vector3.up * highY;
            Vector3 highRight = topCenter + side * halfWidth + Vector3.up * highY;
            Vector3 highLeftBottom = topCenter - side * halfWidth + Vector3.up * lowY;
            Vector3 highRightBottom = topCenter + side * halfWidth + Vector3.up * lowY;

            Mesh visualMesh = BuildRampMesh(includeVisualSkirt: true, $"Constructed Ramp Wedge Visual Mesh {index:000}");
            Mesh colliderMesh = BuildRampMesh(includeVisualSkirt: false, $"Constructed Ramp Wedge Collider Mesh {index:000}");

            var ramp = new GameObject($"Constructed Ramp Wedge {index:000}");
            ramp.layer = layer;
            ramp.transform.SetParent(parent, worldPositionStays: false);
            ramp.transform.position = piece.Center;

            MeshFilter meshFilter = ramp.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = visualMesh;
            MeshRenderer meshRenderer = ramp.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterials = new[] { rampMaterial, wallMaterial };
            MeshCollider meshCollider = ramp.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = colliderMesh;
            GeneratedRampMarker marker = ramp.AddComponent<GeneratedRampMarker>();
            marker.Initialize(
                piece.Center,
                direction,
                side,
                halfWidth,
                halfLength,
                piece.Center.y + lowY,
                piece.Center.y + highY);

            Mesh BuildRampMesh(bool includeVisualSkirt, string meshName)
            {
                var vertices = new List<Vector3>(includeVisualSkirt ? 72 : 36);
                var normals = new List<Vector3>(includeVisualSkirt ? 72 : 36);
                var uvs = new List<Vector2>(includeVisualSkirt ? 72 : 36);
                var colors = new List<Color>(includeVisualSkirt ? 72 : 36);
                var rampTriangles = new List<int>(12);
                var wallTriangles = new List<int>(includeVisualSkirt ? 132 : 60);

                AddDoubleSidedQuad(vertices, normals, uvs, colors, rampTriangles, UvForRamp, lowLeft, highLeft, highRight, lowRight);
                AddDoubleSidedTriangle(vertices, normals, uvs, colors, wallTriangles, UvForRampWall, lowLeft, highLeftBottom, highLeft);
                AddDoubleSidedTriangle(vertices, normals, uvs, colors, wallTriangles, UvForRampWall, lowRight, highRight, highRightBottom);
                AddDoubleSidedQuad(vertices, normals, uvs, colors, wallTriangles, UvForRampWall, highLeftBottom, highRightBottom, highRight, highLeft);
                AddDoubleSidedQuad(vertices, normals, uvs, colors, wallTriangles, UvForRampWall, lowLeft, lowRight, highRightBottom, highLeftBottom);

                if (includeVisualSkirt)
                {
                    AddRampVisualSkirts(vertices, normals, uvs, colors, wallTriangles, UvForRampWall, map, piece, lowLeft, lowRight, highLeftBottom, highRightBottom);
                }

                var mesh = new Mesh
                {
                    name = meshName
                };
                mesh.SetVertices(vertices);
                mesh.SetNormals(normals);
                mesh.SetUVs(0, uvs);
                mesh.SetColors(colors);
                mesh.subMeshCount = 2;
                mesh.SetTriangles(rampTriangles, 0);
                mesh.SetTriangles(wallTriangles, 1);
                mesh.RecalculateBounds();
                mesh.RecalculateTangents();
                return mesh;
            }

            Vector2 UvForRamp(Vector3 position)
            {
                float scale = Mathf.Max(0.01f, map != null ? map.GridSpacing : 1f);
                float across = Vector3.Dot(position, side) + halfWidth;
                float along = Vector3.Dot(position, direction) + halfLength;
                return new Vector2(across / scale, along / scale);
            }

            Vector2 UvForRampWall(Vector3 position)
            {
                float scale = Mathf.Max(0.01f, map != null ? map.GridSpacing : 1f);
                float along = Vector3.Dot(position, direction) + halfLength;
                return new Vector2(along / scale, position.y / scale);
            }
        }

        private static void AddRampVisualSkirts(
            List<Vector3> vertices,
            List<Vector3> normals,
            List<Vector2> uvs,
            List<Color> colors,
            List<int> triangles,
            Func<Vector3, Vector2> uvFor,
            GeneratedWorldMap map,
            BlockyCircusGroundsPiece piece,
            Vector3 lowLeft,
            Vector3 lowRight,
            Vector3 highLeftBottom,
            Vector3 highRightBottom)
        {
            if (map == null || !piece.HasFootprint)
            {
                return;
            }

            Vector3 lowLeftBottom = SkirtBottomFor(map, piece, lowLeft);
            Vector3 lowRightBottom = SkirtBottomFor(map, piece, lowRight);
            Vector3 highLeftBottomSkirt = SkirtBottomFor(map, piece, highLeftBottom);
            Vector3 highRightBottomSkirt = SkirtBottomFor(map, piece, highRightBottom);

            AddDoubleSidedQuad(vertices, normals, uvs, colors, triangles, uvFor, lowLeftBottom, lowLeft, lowRight, lowRightBottom);
            AddDoubleSidedQuad(vertices, normals, uvs, colors, triangles, uvFor, lowLeftBottom, highLeftBottomSkirt, highLeftBottom, lowLeft);
            AddDoubleSidedQuad(vertices, normals, uvs, colors, triangles, uvFor, lowRight, highRightBottom, highRightBottomSkirt, lowRightBottom);
        }

        private static Vector3 SkirtBottomFor(GeneratedWorldMap map, BlockyCircusGroundsPiece piece, Vector3 localPoint)
        {
            Vector3 worldPoint = piece.Center + localPoint;
            int sampleX = Mathf.RoundToInt((worldPoint.x / map.GridSpacing) + (map.Width - 1) * 0.5f);
            int sampleZ = Mathf.RoundToInt((worldPoint.z / map.GridSpacing) + (map.Depth - 1) * 0.5f);
            float terrainY = worldPoint.y - RampVisualSkirtOverlap;
            if (map.TryGetSample(sampleX, sampleZ, out GeneratedWorldSample sample))
            {
                terrainY = Mathf.Min(worldPoint.y - RampVisualSkirtOverlap, sample.Height - RampVisualSkirtOverlap);
            }

            float bottomY = Mathf.Max(worldPoint.y - RampVisualSkirtDepth, terrainY);
            return new Vector3(localPoint.x, bottomY - piece.Center.y, localPoint.z);
        }

        private static void AddDoubleSidedQuad(
            List<Vector3> vertices,
            List<Vector3> normals,
            List<Vector2> uvs,
            List<Color> colors,
            List<int> triangles,
            Func<Vector3, Vector2> uvFor,
            Vector3 a,
            Vector3 b,
            Vector3 c,
            Vector3 d)
        {
            AddFace(vertices, normals, uvs, colors, triangles, uvFor, a, b, c, d);
            AddFace(vertices, normals, uvs, colors, triangles, uvFor, d, c, b, a);
        }

        private static void AddDoubleSidedTriangle(
            List<Vector3> vertices,
            List<Vector3> normals,
            List<Vector2> uvs,
            List<Color> colors,
            List<int> triangles,
            Func<Vector3, Vector2> uvFor,
            Vector3 a,
            Vector3 b,
            Vector3 c)
        {
            AddFace(vertices, normals, uvs, colors, triangles, uvFor, a, b, c);
            AddFace(vertices, normals, uvs, colors, triangles, uvFor, c, b, a);
        }

        private static void AddFace(
            List<Vector3> vertices,
            List<Vector3> normals,
            List<Vector2> uvs,
            List<Color> colors,
            List<int> triangles,
            Func<Vector3, Vector2> uvFor,
            params Vector3[] faceVertices)
        {
            if (faceVertices == null || faceVertices.Length < 3)
            {
                return;
            }

            int start = vertices.Count;
            Vector3 normal = Vector3.Cross(faceVertices[1] - faceVertices[0], faceVertices[2] - faceVertices[0]).normalized;
            if (normal.sqrMagnitude < 0.0001f)
            {
                normal = Vector3.up;
            }

            for (int i = 0; i < faceVertices.Length; i++)
            {
                vertices.Add(faceVertices[i]);
                normals.Add(normal);
                uvs?.Add(uvFor != null ? uvFor(faceVertices[i]) : Vector2.zero);
                colors?.Add(Color.white);
            }

            for (int i = 1; i < faceVertices.Length - 1; i++)
            {
                triangles.Add(start);
                triangles.Add(start + i);
                triangles.Add(start + i + 1);
            }
        }

        private static Material ResolveMaterial(Material material)
        {
            if (material != null && material.shader != null && material.shader.isSupported)
            {
                return material;
            }

            if (fallbackMaterial != null)
            {
                return fallbackMaterial;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Standard");
            fallbackMaterial = new Material(shader)
            {
                name = "Generated Terrain Fallback Material",
                color = new Color(0.16f, 0.22f, 0.24f, 1f),
                hideFlags = HideFlags.DontSave
            };
            return fallbackMaterial;
        }

        private static Material ResolveWallMaterial(Material material)
        {
            if (material != null && material.shader != null && material.shader.isSupported)
            {
                return material;
            }

            if (fallbackWallMaterial != null)
            {
                return fallbackWallMaterial;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Standard");
            fallbackWallMaterial = new Material(shader)
            {
                name = "Generated Terrain Wall Material",
                color = new Color(0.11f, 0.15f, 0.16f, 1f),
                hideFlags = HideFlags.DontSave
            };
            SetColorIfPresent(fallbackWallMaterial, "_BaseColor", new Color(0.11f, 0.15f, 0.16f, 1f));
            SetColorIfPresent(fallbackWallMaterial, "_Color", new Color(0.11f, 0.15f, 0.16f, 1f));
            SetFloatIfPresent(fallbackWallMaterial, "_Smoothness", 0.18f);
            SetFloatIfPresent(fallbackWallMaterial, "_SpecularHighlights", 0f);
            SetFloatIfPresent(fallbackWallMaterial, "_EmissionStrength", 0f);
            return fallbackWallMaterial;
        }

        private static Material[] ResolveMaterialArray(Material[] materials, bool wall)
        {
            if (materials == null || materials.Length == 0)
            {
                return new[] { wall ? ResolveWallMaterial(null) : ResolveMaterial(null) };
            }

            var resolved = new Material[materials.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                resolved[i] = wall ? ResolveWallMaterial(materials[i]) : ResolveMaterial(materials[i]);
            }

            return resolved;
        }

        private static Material MaterialAt(Material[] materials, int zoneIndex)
        {
            if (materials == null || materials.Length == 0)
            {
                return ResolveMaterial(null);
            }

            return materials[Mod(zoneIndex, materials.Length)];
        }

        public static int SelectSurfaceZoneIndex(
            GeneratedWorldMap map,
            Vector3 worldPosition,
            int seed,
            int actNumber,
            int zoneCount)
        {
            int zones = Mathf.Clamp(zoneCount, 1, 8);
            if (map == null || zones <= 1)
            {
                return 0;
            }

            float radius = Mathf.Max(map.GridSpacing, map.PlayableRadius * 0.92f);
            Vector2 position = new(worldPosition.x, worldPosition.z);
            float bestDistance = float.MaxValue;
            int bestZone = 0;
            for (int zone = 0; zone < zones; zone++)
            {
                float angle = Hash01(zone, actNumber, seed + 4011) * Mathf.PI * 2f;
                float radial = Mathf.Sqrt(Mathf.Lerp(0.04f, 0.95f, Hash01(zone, actNumber, seed + 4079))) * radius;
                Vector2 anchor = new(Mathf.Cos(angle) * radial, Mathf.Sin(angle) * radial);
                float weight = Mathf.Lerp(0.82f, 1.18f, Hash01(zone, actNumber, seed + 4129));
                float distance = (position - anchor).sqrMagnitude * weight;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestZone = zone;
                }
            }

            return bestZone;
        }

        public static Color SampleSurfaceZoneTint(
            GeneratedWorldMap map,
            GeneratedWorldTerrainBuildSettings settings,
            GeneratedWorldSurfaceMaterialRole role,
            Vector3 worldPosition)
        {
            Color[] tints = ZoneTintsForRole(settings, role);
            int zones = Mathf.Min(Mathf.Clamp(settings.SurfaceZoneCount, 1, 8), tints.Length);
            if (map == null || zones <= 1)
            {
                return Color.white;
            }

            float radius = Mathf.Max(map.GridSpacing, map.PlayableRadius * 0.92f);
            Vector2 position = new(worldPosition.x, worldPosition.z);
            float sharpness = Mathf.Clamp(settings.SurfaceZoneBlendSharpness, 0.25f, 4f);
            Color blended = Color.clear;
            float totalWeight = 0f;
            for (int zone = 0; zone < zones; zone++)
            {
                float angle = Hash01(zone, settings.SurfaceZoneActNumber, settings.SurfaceZoneSeed + 4011) * Mathf.PI * 2f;
                float radial = Mathf.Sqrt(Mathf.Lerp(0.04f, 0.95f, Hash01(zone, settings.SurfaceZoneActNumber, settings.SurfaceZoneSeed + 4079))) * radius;
                Vector2 anchor = new(Mathf.Cos(angle) * radial, Mathf.Sin(angle) * radial);
                float anchorWeight = Mathf.Lerp(0.82f, 1.18f, Hash01(zone, settings.SurfaceZoneActNumber, settings.SurfaceZoneSeed + 4129));
                float distance = Mathf.Sqrt(Mathf.Max(0.0001f, (position - anchor).sqrMagnitude * anchorWeight));
                float influence = 1f / Mathf.Pow(distance + map.GridSpacing * 0.35f, sharpness);
                blended += tints[zone] * influence;
                totalWeight += influence;
            }

            if (totalWeight <= 0.0001f)
            {
                return Color.white;
            }

            Color result = blended / totalWeight;
            result.a = 1f;
            return result;
        }

        private static Color[] ZoneTintsForRole(
            GeneratedWorldTerrainBuildSettings settings,
            GeneratedWorldSurfaceMaterialRole role)
        {
            return role switch
            {
                GeneratedWorldSurfaceMaterialRole.Organic => settings.OrganicZoneTints,
                GeneratedWorldSurfaceMaterialRole.Wall => settings.WallZoneTints,
                _ => settings.TopZoneTints
            };
        }

        private static Vector3 ChunkCenter(
            GeneratedWorldMap map,
            int startX,
            int endX,
            int startZ,
            int endZ)
        {
            float centerX = (((startX + endX) * 0.5f) - (map.Width - 1) * 0.5f) * map.GridSpacing;
            float centerZ = (((startZ + endZ) * 0.5f) - (map.Depth - 1) * 0.5f) * map.GridSpacing;
            return new Vector3(centerX, 0f, centerZ);
        }

        private static int Mod(int value, int modulo)
        {
            if (modulo <= 0)
            {
                return 0;
            }

            int result = value % modulo;
            return result < 0 ? result + modulo : result;
        }

        private static float Hash01(int x, int y, int seed)
        {
            unchecked
            {
                uint hash = (uint)seed;
                hash ^= (uint)(x * 374761393);
                hash = (hash << 13) | (hash >> 19);
                hash ^= (uint)(y * 668265263);
                hash *= 1274126177u;
                hash ^= hash >> 16;
                return (hash & 0x00FFFFFF) / 16777215f;
            }
        }

        private static Mesh BuildChunkMesh(
            GeneratedWorldMap map,
            GeneratedWorldTerrainBuildSettings settings,
            int startX,
            int endX,
            int startZ,
            int endZ)
        {
            int quadWidth = endX - startX;
            int quadDepth = endZ - startZ;
            int vertexCount = quadWidth * quadDepth * 4;
            var vertices = new List<Vector3>(vertexCount);
            var normals = new List<Vector3>(vertexCount);
            var uvs = new List<Vector2>(vertexCount);
            var colors = new List<Color>(vertexCount);
            var topTriangles = new List<int>(quadWidth * quadDepth * 6);
            var wallTriangles = new List<int>(quadWidth * quadDepth * 6);
            var organicTriangles = new List<int>(quadWidth * quadDepth * 6);
            float wallThreshold = Mathf.Max(0f, map.WallHeightThreshold);

            for (int z = startZ; z < endZ; z++)
            {
                for (int x = startX; x < endX; x++)
                {
                    if (!CellInsidePlayableCircle(map, x, z))
                    {
                        continue;
                    }

                    TerrainCell cell = CellFor(map, x, z);
                    if (IsSmoothGroundCell(map, x, z))
                    {
                        AddQuadWithNormals(
                            cell.BottomLeft,
                            cell.TopLeft,
                            cell.TopRight,
                            cell.BottomRight,
                            SampleNormal(x, z),
                            SampleNormal(x, z + 1),
                            SampleNormal(x + 1, z + 1),
                            SampleNormal(x + 1, z),
                            GeneratedWorldSurfaceMaterialRole.Organic,
                            organicTriangles);
                    }
                    else
                    {
                        AddQuad(cell.BottomLeft, cell.TopLeft, cell.TopRight, cell.BottomRight, GeneratedWorldSurfaceMaterialRole.Top, topTriangles);
                    }

                    if (x > 0 && !CellInsidePlayableCircle(map, x - 1, z))
                    {
                        AddBoundaryWall(cell.TopLeft, cell.BottomLeft);
                    }

                    if (z > 0 && !CellInsidePlayableCircle(map, x, z - 1))
                    {
                        AddBoundaryWall(cell.BottomLeft, cell.BottomRight);
                    }

                    if (x < map.Width - 2 && CellInsidePlayableCircle(map, x + 1, z))
                    {
                        TerrainCell right = CellFor(map, x + 1, z);
                        AddWallIfNeeded(
                            cell.BottomRight,
                            cell.TopRight,
                            right.BottomLeft,
                            right.TopLeft,
                            wallThreshold);
                    }
                    else
                    {
                        AddBoundaryWall(cell.BottomRight, cell.TopRight);
                    }

                    if (z < map.Depth - 2 && CellInsidePlayableCircle(map, x, z + 1))
                    {
                        TerrainCell top = CellFor(map, x, z + 1);
                        AddWallIfNeeded(
                            cell.TopLeft,
                            cell.TopRight,
                            top.BottomLeft,
                            top.BottomRight,
                            wallThreshold);
                    }
                    else
                    {
                        AddBoundaryWall(cell.TopRight, cell.TopLeft);
                    }
                }
            }

            var mesh = new Mesh
            {
                name = "Generated World Terrain Chunk Mesh",
                indexFormat = vertices.Count > ushort.MaxValue ? IndexFormat.UInt32 : IndexFormat.UInt16
            };
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetColors(colors);
            mesh.subMeshCount = 3;
            mesh.SetTriangles(topTriangles, 0);
            mesh.SetTriangles(wallTriangles, 1);
            mesh.SetTriangles(organicTriangles, 2);
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            return mesh;

            void AddTopVertex(Vector3 position, Vector3 normal, GeneratedWorldSurfaceMaterialRole role)
            {
                AddProjectedVertex(position, normal, WorldTopUv(position), SampleSurfaceZoneTint(map, settings, role, position));
            }

            void AddProjectedWhiteVertex(Vector3 position, Vector3 normal, Vector2 uv)
            {
                AddProjectedVertex(position, normal, uv, Color.white);
            }

            void AddProjectedVertex(Vector3 position, Vector3 normal, Vector2 uv, Color color)
            {
                vertices.Add(position);
                normals.Add(normal.sqrMagnitude > 0.0001f ? normal.normalized : Vector3.up);
                uvs.Add(uv);
                color.a = 1f;
                colors.Add(color);
            }

            Vector2 WorldTopUv(Vector3 position)
            {
                float scale = Mathf.Max(0.01f, map.GridSpacing);
                return new Vector2(position.x / scale, position.z / scale);
            }

            void AddQuad(
                Vector3 bottomLeft,
                Vector3 topLeft,
                Vector3 topRight,
                Vector3 bottomRight,
                GeneratedWorldSurfaceMaterialRole role,
                List<int> targetTriangles)
            {
                Vector3 normal = FaceNormal(bottomLeft, topLeft, topRight);
                AddQuadWithNormals(bottomLeft, topLeft, topRight, bottomRight, normal, normal, normal, normal, role, targetTriangles);
            }

            void AddQuadWithNormals(
                Vector3 bottomLeft,
                Vector3 topLeft,
                Vector3 topRight,
                Vector3 bottomRight,
                Vector3 bottomLeftNormal,
                Vector3 topLeftNormal,
                Vector3 topRightNormal,
                Vector3 bottomRightNormal,
                GeneratedWorldSurfaceMaterialRole role,
                List<int> targetTriangles)
            {
                int baseIndex = vertices.Count;
                AddTopVertex(bottomLeft, bottomLeftNormal, role);
                AddTopVertex(topLeft, topLeftNormal, role);
                AddTopVertex(topRight, topRightNormal, role);
                AddTopVertex(bottomRight, bottomRightNormal, role);

                targetTriangles.Add(baseIndex);
                targetTriangles.Add(baseIndex + 1);
                targetTriangles.Add(baseIndex + 2);
                targetTriangles.Add(baseIndex);
                targetTriangles.Add(baseIndex + 2);
                targetTriangles.Add(baseIndex + 3);
            }

            Vector3 SampleNormal(int sampleX, int sampleZ)
            {
                return map.TryGetSample(sampleX, sampleZ, out GeneratedWorldSample sample)
                    ? sample.Normal
                    : Vector3.up;
            }

            Vector3 FaceNormal(Vector3 a, Vector3 b, Vector3 c)
            {
                Vector3 normal = Vector3.Cross(b - a, c - a);
                return normal.sqrMagnitude > 0.0001f ? normal.normalized : Vector3.up;
            }

            void AddDoubleSidedWall(Vector3 bottomLeft, Vector3 topLeft, Vector3 topRight, Vector3 bottomRight)
            {
                AddWallQuad(bottomLeft, topLeft, topRight, bottomRight);
                AddWallQuad(bottomLeft, bottomRight, topRight, topLeft);
            }

            void AddWallQuad(Vector3 bottomLeft, Vector3 topLeft, Vector3 topRight, Vector3 bottomRight)
            {
                Vector3 normal = FaceNormal(bottomLeft, topLeft, topRight);
                float scale = Mathf.Max(0.01f, map.GridSpacing);
                float width = Vector3.Distance(bottomLeft, bottomRight) / scale;
                int baseIndex = vertices.Count;
                AddProjectedWhiteVertex(bottomLeft, normal, new Vector2(0f, bottomLeft.y / scale));
                AddProjectedWhiteVertex(topLeft, normal, new Vector2(0f, topLeft.y / scale));
                AddProjectedWhiteVertex(topRight, normal, new Vector2(width, topRight.y / scale));
                AddProjectedWhiteVertex(bottomRight, normal, new Vector2(width, bottomRight.y / scale));

                wallTriangles.Add(baseIndex);
                wallTriangles.Add(baseIndex + 1);
                wallTriangles.Add(baseIndex + 2);
                wallTriangles.Add(baseIndex);
                wallTriangles.Add(baseIndex + 2);
                wallTriangles.Add(baseIndex + 3);
            }

            void AddBoundaryWall(Vector3 firstTop, Vector3 secondTop)
            {
                float bottomY = Mathf.Min(0f, firstTop.y, secondTop.y) - map.GridSpacing * 0.25f;
                Vector3 firstBottom = new Vector3(firstTop.x, bottomY, firstTop.z);
                Vector3 secondBottom = new Vector3(secondTop.x, bottomY, secondTop.z);
                AddDoubleSidedWall(firstBottom, firstTop, secondTop, secondBottom);
            }

            void AddWallIfNeeded(Vector3 firstA, Vector3 firstB, Vector3 secondA, Vector3 secondB, float threshold)
            {
                float firstHeight = (firstA.y + firstB.y) * 0.5f;
                float secondHeight = (secondA.y + secondB.y) * 0.5f;
                float endpointDelta = Mathf.Max(
                    Mathf.Abs(firstA.y - secondA.y),
                    Mathf.Abs(firstB.y - secondB.y));
                if (Mathf.Abs(firstHeight - secondHeight) <= threshold && endpointDelta <= 0.01f)
                {
                    return;
                }

                if (firstHeight > secondHeight)
                {
                    AddDoubleSidedWall(secondA, firstA, firstB, secondB);
                    return;
                }

                AddDoubleSidedWall(firstA, secondA, secondB, firstB);
            }

        }

        private static bool CellInsidePlayableCircle(GeneratedWorldMap map, int x, int z)
        {
            Vector3 bottomLeft = map.PositionFor(x, z);
            Vector3 topRight = map.PositionFor(x + 1, z + 1);
            Vector2 center = new Vector2(
                (bottomLeft.x + topRight.x) * 0.5f,
                (bottomLeft.z + topRight.z) * 0.5f);
            return center.sqrMagnitude <= map.PlayableRadius * map.PlayableRadius;
        }

        private static bool IsSmoothGroundCell(GeneratedWorldMap map, int x, int z)
        {
            return IsContinuousSample(map, x, z)
                && IsContinuousSample(map, x + 1, z)
                && IsContinuousSample(map, x, z + 1)
                && IsContinuousSample(map, x + 1, z + 1);
        }

        private static void SetColorIfPresent(Material material, string property, Color value)
        {
            if (material != null && material.HasProperty(property))
            {
                material.SetColor(property, value);
            }
        }

        private static void SetTextureIfPresent(Material material, string property, Texture value)
        {
            if (material != null && value != null && material.HasProperty(property))
            {
                material.SetTexture(property, value);
            }
        }

        private static void SetFloatIfPresent(Material material, string property, float value)
        {
            if (material != null && material.HasProperty(property))
            {
                material.SetFloat(property, value);
            }
        }

        private readonly struct TerrainCell
        {
            public TerrainCell(Vector3 bottomLeft, Vector3 bottomRight, Vector3 topLeft, Vector3 topRight)
            {
                BottomLeft = bottomLeft;
                BottomRight = bottomRight;
                TopLeft = topLeft;
                TopRight = topRight;
            }

            public Vector3 BottomLeft { get; }
            public Vector3 BottomRight { get; }
            public Vector3 TopLeft { get; }
            public Vector3 TopRight { get; }
        }

        private static TerrainCell CellFor(GeneratedWorldMap map, int x, int z)
        {
            Vector3 bottomLeft = map.PositionFor(x, z);
            Vector3 bottomRight = map.PositionFor(x + 1, z);
            Vector3 topLeft = map.PositionFor(x, z + 1);
            Vector3 topRight = map.PositionFor(x + 1, z + 1);

            if (ShouldPreserveCellHeights(map, x, z))
            {
                return new TerrainCell(bottomLeft, bottomRight, topLeft, topRight);
            }

            float flatHeight = IsMixedConstructedContinuousCell(map, x, z)
                ? ConstructedHeight(map, x, z, bottomLeft.y, bottomRight.y, topLeft.y, topRight.y)
                : DominantHeight(bottomLeft.y, bottomRight.y, topLeft.y, topRight.y);
            bottomLeft.y = flatHeight;
            bottomRight.y = flatHeight;
            topLeft.y = flatHeight;
            topRight.y = flatHeight;
            return new TerrainCell(bottomLeft, bottomRight, topLeft, topRight);
        }

        private static bool IsRampSample(GeneratedWorldMap map, int x, int z)
        {
            return map.TryGetSample(x, z, out GeneratedWorldSample sample) && sample.IsRamp;
        }

        private static bool ShouldPreserveCellHeights(GeneratedWorldMap map, int x, int z)
        {
            if (HasRampSample(map, x, z))
            {
                return true;
            }

            return IsContinuousSample(map, x, z)
                && IsContinuousSample(map, x + 1, z)
                && IsContinuousSample(map, x, z + 1)
                && IsContinuousSample(map, x + 1, z + 1);
        }

        private static bool HasRampSample(GeneratedWorldMap map, int x, int z)
        {
            return IsRampSample(map, x, z)
                || IsRampSample(map, x + 1, z)
                || IsRampSample(map, x, z + 1)
                || IsRampSample(map, x + 1, z + 1);
        }

        private static bool IsContinuousSample(GeneratedWorldMap map, int x, int z)
        {
            return map.TryGetSample(x, z, out GeneratedWorldSample sample)
                && sample.SurfaceKind == GeneratedWorldSurfaceKind.Continuous;
        }

        private static bool IsMixedConstructedContinuousCell(GeneratedWorldMap map, int x, int z)
        {
            bool hasConstructed = false;
            bool hasContinuous = false;
            Consider(x, z);
            Consider(x + 1, z);
            Consider(x, z + 1);
            Consider(x + 1, z + 1);
            return hasConstructed && hasContinuous;

            void Consider(int sampleX, int sampleZ)
            {
                if (IsConstructedSample(map, sampleX, sampleZ))
                {
                    hasConstructed = true;
                }

                if (IsContinuousSample(map, sampleX, sampleZ))
                {
                    hasContinuous = true;
                }
            }
        }

        private static bool IsConstructedSample(GeneratedWorldMap map, int x, int z)
        {
            return map.TryGetSample(x, z, out GeneratedWorldSample sample)
                && (sample.SurfaceKind == GeneratedWorldSurfaceKind.Flat
                    || sample.SurfaceKind == GeneratedWorldSurfaceKind.Ramp);
        }

        private static float ConstructedHeight(
            GeneratedWorldMap map,
            int x,
            int z,
            float bottomLeft,
            float bottomRight,
            float topLeft,
            float topRight)
        {
            float height = float.MinValue;
            Consider(x, z, bottomLeft);
            Consider(x + 1, z, bottomRight);
            Consider(x, z + 1, topLeft);
            Consider(x + 1, z + 1, topRight);
            return height > float.MinValue ? height : DominantHeight(bottomLeft, bottomRight, topLeft, topRight);

            void Consider(int sampleX, int sampleZ, float sampleHeight)
            {
                if (IsConstructedSample(map, sampleX, sampleZ))
                {
                    height = Mathf.Max(height, sampleHeight);
                }
            }
        }

        private static float DominantHeight(float a, float b, float c, float d)
        {
            float average = (a + b + c + d) * 0.25f;
            float result = a;
            float bestDistance = Mathf.Abs(a - average);
            Consider(b);
            Consider(c);
            Consider(d);
            return result;

            void Consider(float value)
            {
                float distance = Mathf.Abs(value - average);
                if (distance < bestDistance || Mathf.Approximately(distance, bestDistance) && value > result)
                {
                    result = value;
                    bestDistance = distance;
                }
            }
        }

    }
}
