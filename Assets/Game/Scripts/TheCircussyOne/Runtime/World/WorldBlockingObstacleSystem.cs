using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;
using UnityEngine;

namespace TheCircussyOne.Runtime
{
    public enum WorldBlockingObstacleShape
    {
        Box = 0,
        TallBox = 1,
        Column = 2
    }

    public readonly struct WorldBlockingObstacleSpawnRequest
    {
        public WorldBlockingObstacleSpawnRequest(
            string id,
            WorldBlockingObstacleShape shape,
            Vector3 position,
            Vector3 normal,
            Quaternion rotation,
            Vector3 size,
            float footprintRadius,
            float avoidanceRadius,
            int variantSeed)
        {
            Id = string.IsNullOrWhiteSpace(id) ? "blocking_obstacle" : id;
            Shape = shape;
            Position = position;
            Normal = normal.sqrMagnitude > 0.0001f ? normal.normalized : Vector3.up;
            Rotation = rotation;
            Size = new Vector3(
                Mathf.Max(0.1f, size.x),
                Mathf.Max(0.1f, size.y),
                Mathf.Max(0.1f, size.z));
            FootprintRadius = Mathf.Max(0.05f, footprintRadius);
            AvoidanceRadius = Mathf.Max(FootprintRadius, avoidanceRadius);
            VariantSeed = variantSeed;
        }

        public string Id { get; }
        public WorldBlockingObstacleShape Shape { get; }
        public Vector3 Position { get; }
        public Vector3 Normal { get; }
        public Quaternion Rotation { get; }
        public Vector3 Size { get; }
        public float FootprintRadius { get; }
        public float AvoidanceRadius { get; }
        public int VariantSeed { get; }
    }

    public static class GeneratedWorldObstaclePlanner
    {
        public static IReadOnlyList<WorldBlockingObstacleSpawnRequest> BuildRequests(
            GeneratedWorldMap map,
            RunWorldGenerationConfig config)
        {
            if (map == null || config == null || !config.WorldObstacleEnabled)
            {
                return Array.Empty<WorldBlockingObstacleSpawnRequest>();
            }

            int targetCount = TargetCount(map, config);
            if (targetCount <= 0)
            {
                return Array.Empty<WorldBlockingObstacleSpawnRequest>();
            }

            var requests = new List<WorldBlockingObstacleSpawnRequest>(targetCount);
            var selected = new SelectedObstacleIndex(Mathf.Max(map.GridSpacing, config.WorldObstacleMinSpacing));
            int seed = DeterministicSeed.Combine(map.AttemptSeed, map.ActNumber, config.WorldObstacleSeedOffset);
            int sampleCount = map.Samples.Count;
            for (int attempt = 0; attempt < sampleCount * 8 && requests.Count < targetCount; attempt++)
            {
                int sampleIndex = PositiveModulo(
                    DeterministicSeed.Combine(seed, attempt, 9137),
                    sampleCount);
                GeneratedWorldSample sample = map.Samples[sampleIndex];
                Vector3 position = map.PositionForIndex(sampleIndex);
                int localSeed = DeterministicSeed.Combine(seed, sampleIndex, requests.Count, attempt);
                if (IsRejected(map, sample, position, requests, selected, config))
                {
                    continue;
                }

                Vector3 size = SizeFor(localSeed, config);
                float footprintRadius = new Vector2(size.x, size.z).magnitude * 0.5f;
                float avoidanceRadius = footprintRadius + config.WorldObstacleRewardExclusionRadius;
                if (!selected.HasSpacing(position, avoidanceRadius))
                {
                    continue;
                }

                Quaternion rotation = ResolveRotation(sample.Normal, localSeed, config.WorldObstacleMaxTiltDegrees);
                float groundSink = GroundSinkFor(size, rotation, sample.Normal);
                WorldBlockingObstacleShape shape = ShapeFor(localSeed, config);
                var request = new WorldBlockingObstacleSpawnRequest(
                    $"blocking_obstacle_{requests.Count + 1:00}",
                    shape,
                    position + sample.Normal * (config.WorldObstacleSurfaceOffset - groundSink),
                    sample.Normal,
                    rotation,
                    size,
                    footprintRadius,
                    avoidanceRadius,
                    localSeed);
                requests.Add(request);
                selected.Add(request.Position, request.AvoidanceRadius);
            }

            return requests;
        }

        private static int TargetCount(GeneratedWorldMap map, RunWorldGenerationConfig config)
        {
            float area = Mathf.PI * map.PlayableRadius * map.PlayableRadius;
            float count = area / 100000f * config.WorldObstacleCountPer100k;
            return Mathf.Clamp(Mathf.RoundToInt(count), 0, config.WorldObstacleMaxCount);
        }

        private static bool IsRejected(
            GeneratedWorldMap map,
            GeneratedWorldSample sample,
            Vector3 position,
            IReadOnlyList<WorldBlockingObstacleSpawnRequest> requests,
            SelectedObstacleIndex selected,
            RunWorldGenerationConfig config)
        {
            if (!sample.HasMask(GeneratedWorldMask.Reachable)
                || sample.SlopeDegrees > config.WorldObstacleMaxSlope
                || sample.IsRamp
                || OutsideBounds(map, position, config.WorldObstacleArenaEdgePadding)
                || TooClose2D(position, map.PlayerStartPosition, config.WorldObstaclePlayerStartExclusionRadius))
            {
                return true;
            }

            if (!selected.HasSpacing(position, config.WorldObstacleMinSpacing))
            {
                return true;
            }

            for (int i = 0; i < requests.Count; i++)
            {
                if (TooClose2D(position, requests[i].Position, requests[i].AvoidanceRadius))
                {
                    return true;
                }
            }

            return false;
        }

        private static Vector3 SizeFor(int seed, RunWorldGenerationConfig config)
        {
            float roll = DeterministicSeed.ToFloat01(DeterministicSeed.Combine(seed, 31));
            float width = Mathf.Lerp(config.WorldObstacleWidthMin, config.WorldObstacleWidthMax, DeterministicSeed.ToFloat01(seed + 101));
            float depth = Mathf.Lerp(config.WorldObstacleDepthMin, config.WorldObstacleDepthMax, DeterministicSeed.ToFloat01(seed + 211));
            float height;
            if (roll < config.WorldObstacleTallChance)
            {
                width *= 0.58f;
                depth *= 0.58f;
                height = Mathf.Lerp(config.WorldObstacleTallHeightMin, config.WorldObstacleTallHeightMax, DeterministicSeed.ToFloat01(seed + 307));
            }
            else if (roll < config.WorldObstacleTallChance + config.WorldObstacleJumpableChance)
            {
                width *= 0.72f;
                depth *= 0.72f;
                height = Mathf.Lerp(config.WorldObstacleJumpableHeightMin, config.WorldObstacleJumpableHeightMax, DeterministicSeed.ToFloat01(seed + 409));
            }
            else
            {
                height = Mathf.Lerp(config.WorldObstacleHeightMin, config.WorldObstacleHeightMax, DeterministicSeed.ToFloat01(seed + 503));
            }

            return new Vector3(width, height, depth);
        }

        private static WorldBlockingObstacleShape ShapeFor(int seed, RunWorldGenerationConfig config)
        {
            float roll = DeterministicSeed.ToFloat01(DeterministicSeed.Combine(seed, 727));
            if (roll < config.WorldObstacleColumnChance)
            {
                return WorldBlockingObstacleShape.Column;
            }

            return DeterministicSeed.ToFloat01(DeterministicSeed.Combine(seed, 733)) < config.WorldObstacleTallChance
                ? WorldBlockingObstacleShape.TallBox
                : WorldBlockingObstacleShape.Box;
        }

        private static Quaternion ResolveRotation(Vector3 normal, int seed, float maxTiltDegrees)
        {
            Vector3 safeNormal = normal.sqrMagnitude > 0.0001f ? normal.normalized : Vector3.up;
            float yaw = Mathf.Round(DeterministicSeed.ToFloat01(seed + 809) * 4f) * 90f;
            Quaternion baseRotation = Quaternion.AngleAxis(yaw, safeNormal) * WorldPropPlacementRules.SurfaceRotation(safeNormal);
            float maxTilt = Mathf.Max(0f, maxTiltDegrees);
            if (maxTilt <= 0.001f)
            {
                return baseRotation;
            }

            float tilt = Mathf.Lerp(0f, maxTilt, DeterministicSeed.ToFloat01(DeterministicSeed.Combine(seed, 811)));
            float direction = DeterministicSeed.ToFloat01(DeterministicSeed.Combine(seed, 813)) * Mathf.PI * 2f;
            Vector3 localAxis = new(Mathf.Cos(direction), 0f, Mathf.Sin(direction));
            return baseRotation * Quaternion.AngleAxis(tilt, localAxis);
        }

        private static float GroundSinkFor(Vector3 size, Quaternion rotation, Vector3 normal)
        {
            Vector3 safeNormal = normal.sqrMagnitude > 0.0001f ? normal.normalized : Vector3.up;
            float tiltAngle = Vector3.Angle(rotation * Vector3.up, safeNormal);
            if (tiltAngle <= 0.001f)
            {
                return 0f;
            }

            float horizontalRadius = new Vector2(size.x, size.z).magnitude * 0.5f;
            return Mathf.Sin(tiltAngle * Mathf.Deg2Rad) * horizontalRadius + 0.02f;
        }

        private static bool OutsideBounds(GeneratedWorldMap map, Vector3 position, float padding)
        {
            float radius = new Vector2(position.x, position.z).magnitude;
            return radius > map.PlayableRadius - Mathf.Max(0f, padding);
        }

        private static bool TooClose2D(Vector3 first, Vector3 second, float distance)
        {
            if (distance <= 0f)
            {
                return false;
            }

            Vector2 first2 = new(first.x, first.z);
            Vector2 second2 = new(second.x, second.z);
            return (first2 - second2).sqrMagnitude < distance * distance;
        }

        private static int PositiveModulo(int value, int divisor)
        {
            if (divisor <= 0)
            {
                return 0;
            }

            int result = value % divisor;
            return result < 0 ? result + divisor : result;
        }

        private sealed class SelectedObstacleIndex
        {
            private readonly Dictionary<Vector2Int, List<Entry>> cells = new();
            private readonly float cellSize;

            public SelectedObstacleIndex(float cellSize)
            {
                this.cellSize = Mathf.Max(1f, cellSize);
            }

            public void Add(Vector3 position, float radius)
            {
                Vector2Int cell = CellFor(position);
                if (!cells.TryGetValue(cell, out List<Entry> entries))
                {
                    entries = new List<Entry>();
                    cells[cell] = entries;
                }

                entries.Add(new Entry(position, Mathf.Max(0f, radius)));
            }

            public bool HasSpacing(Vector3 position, float radius)
            {
                if (cells.Count == 0)
                {
                    return true;
                }

                float safeRadius = Mathf.Max(0f, radius);
                Vector2Int center = CellFor(position);
                int range = Mathf.CeilToInt((safeRadius + cellSize) / cellSize);
                for (int z = -range; z <= range; z++)
                {
                    for (int x = -range; x <= range; x++)
                    {
                        var cell = new Vector2Int(center.x + x, center.y + z);
                        if (!cells.TryGetValue(cell, out List<Entry> entries))
                        {
                            continue;
                        }

                        for (int i = 0; i < entries.Count; i++)
                        {
                            Entry entry = entries[i];
                            if (WorldPropPlacementRules.FootprintsOverlap(position, safeRadius, entry.Position, entry.Radius))
                            {
                                return false;
                            }
                        }
                    }
                }

                return true;
            }

            private Vector2Int CellFor(Vector3 position)
            {
                return new Vector2Int(
                    Mathf.FloorToInt(position.x / cellSize),
                    Mathf.FloorToInt(position.z / cellSize));
            }

            private readonly struct Entry
            {
                public Entry(Vector3 position, float radius)
                {
                    Position = position;
                    Radius = radius;
                }

                public Vector3 Position { get; }
                public float Radius { get; }
            }
        }
    }

    public static class WorldBlockingObstacleFactory
    {
        private const string RootName = "Blocking Circus Obstacles";

        public static GameObject Build(
            Transform parent,
            IReadOnlyList<WorldBlockingObstacleSpawnRequest> requests,
            RunWorldGenerationConfig config,
            WorldPropPlacementService placementService = null)
        {
            if (parent == null || requests == null || requests.Count == 0)
            {
                return null;
            }

            GameObject root = CreateRoot(parent);
            var cache = new MaterialCache(config, requests.Count > 0 ? requests[0].VariantSeed : 0);
            var cleanup = root.AddComponent<GeneratedWorldObstacleCleanup>();
            for (int i = 0; i < requests.Count; i++)
            {
                CreateObstacle(root.transform, requests[i], cache);
                placementService?.RegisterFootprint(requests[i].Position, requests[i].AvoidanceRadius);
            }

            cleanup.Capture(cache.OwnedObjects);
            return root;
        }

        public static async UniTask<GameObject> BuildAsync(
            Transform parent,
            IReadOnlyList<WorldBlockingObstacleSpawnRequest> requests,
            RunWorldGenerationConfig config,
            WorldPropPlacementService placementService,
            WorldLoadTimingDiagnostics timing)
        {
            if (parent == null || requests == null || requests.Count == 0)
            {
                return null;
            }

            GameObject root = CreateRoot(parent);
            var cache = new MaterialCache(config, requests.Count > 0 ? requests[0].VariantSeed : 0);
            var cleanup = root.AddComponent<GeneratedWorldObstacleCleanup>();
            int yieldEvery = config != null ? config.WorldObstacleBuildItemsPerFrame : 16;
            for (int i = 0; i < requests.Count; i++)
            {
                CreateObstacle(root.transform, requests[i], cache);
                placementService?.RegisterFootprint(requests[i].Position, requests[i].AvoidanceRadius);

                if ((i + 1) % yieldEvery == 0)
                {
                    await UniTask.Yield(PlayerLoopTiming.Update);
                }
            }

            cleanup.Capture(cache.OwnedObjects);
            return root;
        }

        private static GameObject CreateRoot(Transform parent)
        {
            var root = new GameObject(RootName);
            root.layer = parent.gameObject.layer;
            root.transform.SetParent(parent, worldPositionStays: false);
            return root;
        }

        private static void CreateObstacle(
            Transform parent,
            WorldBlockingObstacleSpawnRequest request,
            MaterialCache cache)
        {
            var root = new GameObject(request.Id);
            int layer = parent.gameObject.layer;
            root.layer = layer;
            root.transform.SetParent(parent, worldPositionStays: false);
            root.transform.SetPositionAndRotation(request.Position, request.Rotation);

            switch (request.Shape)
            {
                case WorldBlockingObstacleShape.Column:
                    CreateColumn(root.transform, request, cache);
                    break;
                case WorldBlockingObstacleShape.TallBox:
                case WorldBlockingObstacleShape.Box:
                default:
                    CreateBox(root.transform, request, cache);
                    break;
            }

            SetLayerRecursively(root, layer);
        }

        private static void CreateBox(
            Transform parent,
            WorldBlockingObstacleSpawnRequest request,
            MaterialCache cache)
        {
            Vector3 bodySize = request.Size;
            float topHeight = Mathf.Clamp(bodySize.y * 0.12f, 0.12f, 0.32f);
            float bodyHeight = Mathf.Max(0.15f, bodySize.y - topHeight);
            GameObject body = Primitive("Painted Body", PrimitiveType.Cube, cache.Body(request.VariantSeed));
            body.transform.SetParent(parent, false);
            body.transform.localPosition = Vector3.up * (bodyHeight * 0.5f);
            body.transform.localScale = new Vector3(bodySize.x, bodyHeight, bodySize.z);

            GameObject top = Primitive("Painted Top", PrimitiveType.Cube, cache.Top(request.VariantSeed));
            top.transform.SetParent(parent, false);
            top.transform.localPosition = Vector3.up * (bodyHeight + topHeight * 0.5f);
            top.transform.localScale = new Vector3(bodySize.x * 1.08f, topHeight, bodySize.z * 1.08f);
        }

        private static void CreateColumn(
            Transform parent,
            WorldBlockingObstacleSpawnRequest request,
            MaterialCache cache)
        {
            Vector3 size = request.Size;
            GameObject body = Primitive("Painted Column", PrimitiveType.Cylinder, cache.Body(request.VariantSeed));
            body.transform.SetParent(parent, false);
            body.transform.localPosition = Vector3.up * (size.y * 0.5f);
            body.transform.localScale = new Vector3(size.x, size.y * 0.5f, size.z);

            float capHeight = Mathf.Clamp(size.y * 0.08f, 0.1f, 0.28f);
            GameObject cap = Primitive("Column Cap", PrimitiveType.Cylinder, cache.Top(request.VariantSeed));
            cap.transform.SetParent(parent, false);
            cap.transform.localPosition = Vector3.up * (size.y + capHeight * 0.5f);
            cap.transform.localScale = new Vector3(size.x * 1.18f, capHeight * 0.5f, size.z * 1.18f);
        }

        private static GameObject Primitive(string name, PrimitiveType primitiveType, Material material)
        {
            GameObject gameObject = GameObject.CreatePrimitive(primitiveType);
            gameObject.name = name;
            if (primitiveType == PrimitiveType.Cylinder)
            {
                ReplacePrimitiveColliderWithMeshCollider(gameObject);
            }

            if (gameObject.TryGetComponent(out Renderer renderer))
            {
                renderer.sharedMaterial = material;
            }

            return gameObject;
        }

        private static void ReplacePrimitiveColliderWithMeshCollider(GameObject gameObject)
        {
            Collider primitiveCollider = gameObject.GetComponent<Collider>();
            if (primitiveCollider != null)
            {
                RuntimeObjectFactory.ReleaseImmediately(primitiveCollider);
            }

            MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.sharedMesh == null)
            {
                return;
            }

            MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = meshFilter.sharedMesh;
        }

        private static void SetLayerRecursively(GameObject gameObject, int layer)
        {
            gameObject.layer = layer;
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                SetLayerRecursively(gameObject.transform.GetChild(i).gameObject, layer);
            }
        }

        private sealed class MaterialCache
        {
            private readonly Dictionary<int, Material> bodyMaterials = new();
            private readonly Dictionary<int, Material> topMaterials = new();
            private readonly List<UnityEngine.Object> ownedObjects = new();
            private readonly int resolution;

            public MaterialCache(RunWorldGenerationConfig config, int seed)
            {
                resolution = config != null ? config.WorldObstacleTextureResolution : 128;
                Seed = seed;
            }

            public int Seed { get; }
            public IReadOnlyList<UnityEngine.Object> OwnedObjects => ownedObjects;

            public Material Body(int seed)
            {
                int key = Mathf.Abs(seed % 5);
                if (bodyMaterials.TryGetValue(key, out Material material))
                {
                    return material;
                }

                material = CreateMaterial($"Circus Blocking Obstacle Body {key}", CreateBodyMaps(key));
                bodyMaterials[key] = material;
                return material;
            }

            public Material Top(int seed)
            {
                int key = Mathf.Abs(seed % 4);
                if (topMaterials.TryGetValue(key, out Material material))
                {
                    return material;
                }

                material = CreateMaterial($"Circus Blocking Obstacle Top {key}", CreateTopMaps(key));
                topMaterials[key] = material;
                return material;
            }

            private Material CreateMaterial(string name, TextureMaps maps)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                var material = new Material(shader)
                {
                    name = name,
                    mainTexture = maps.Albedo
                };
                if (material.HasProperty("_BaseMap"))
                {
                    material.SetTexture("_BaseMap", maps.Albedo);
                }

                if (material.HasProperty("_BumpMap") && maps.Normal != null)
                {
                    material.SetTexture("_BumpMap", maps.Normal);
                    material.EnableKeyword("_NORMALMAP");
                    if (material.HasProperty("_BumpScale"))
                    {
                        material.SetFloat("_BumpScale", 0.72f);
                    }
                }

                if (material.HasProperty("_Smoothness"))
                {
                    material.SetFloat("_Smoothness", maps.Smoothness);
                }

                if (material.HasProperty("_Glossiness"))
                {
                    material.SetFloat("_Glossiness", maps.Smoothness);
                }

                ownedObjects.Add(maps.Albedo);
                if (maps.Normal != null)
                {
                    ownedObjects.Add(maps.Normal);
                }

                ownedObjects.Add(material);
                return material;
            }

            private TextureMaps CreateBodyMaps(int variant)
            {
                int size = Mathf.Clamp(resolution, 64, 256);
                var albedo = new Texture2D(size, size, TextureFormat.RGBA32, mipChain: true)
                {
                    name = $"Circus Blocking Obstacle Body Texture {variant}",
                    wrapMode = TextureWrapMode.Repeat,
                    filterMode = FilterMode.Trilinear,
                    anisoLevel = 4
                };
                var height = new float[size * size];
                Color[] palette = BodyPalette(variant);
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float u = (x + 0.5f) / size;
                        float v = (y + 0.5f) / size;
                        int stripe = Mathf.FloorToInt(u * 8f) % palette.Length;
                        Color color = palette[stripe];
                        float withinStripe = Mathf.Repeat(u * 8f, 1f);
                        float seamDistance = Mathf.Min(withinStripe, 1f - withinStripe);
                        float seam = 1f - Mathf.SmoothStep(0f, 0.105f, seamDistance);
                        float panelEdgeWear = 1f - Mathf.SmoothStep(0f, 0.075f, seamDistance);
                        float verticalWear = Mathf.Pow(1f - v, 2.2f);
                        float dirt = ValueNoise(x + variant * 31, y + 97, 19) * 0.25f
                            + ValueNoise(x, y + variant * 53, 7) * 0.12f;
                        float brush = Mathf.Sin((v * 42f + ValueNoise(x, y, 11) * 4f) * Mathf.PI) * 0.035f;
                        float streak = Mathf.Pow(ValueNoise(x / 3 + variant * 13, y / 9, 151), 2.4f) * verticalWear;
                        color = Color.Lerp(color, Color.black, dirt + 0.11f + streak * 0.32f);
                        color = Color.Lerp(color, new Color(0.95f, 0.76f, 0.35f, 1f), panelEdgeWear * 0.18f);
                        color = Color.Lerp(color, new Color(0.18f, 0.13f, 0.08f, 1f), seam * 0.22f);
                        color += new Color(brush, brush, brush, 0f);
                        albedo.SetPixel(x, y, ClampColor(color));
                        float plankRaised = (Hash01(stripe, variant, 223) - 0.5f) * 0.08f;
                        height[y * size + x] = Mathf.Clamp01(0.50f + plankRaised - seam * 0.20f + panelEdgeWear * 0.035f - dirt * 0.06f + brush * 0.55f);
                    }
                }

                albedo.Apply(updateMipmaps: true, makeNoLongerReadable: true);
                Texture2D normal = CreateNormalTexture($"Circus Blocking Obstacle Body Normal {variant}", size, height, 4.8f);
                return new TextureMaps(albedo, normal, 0.14f);
            }

            private TextureMaps CreateTopMaps(int variant)
            {
                int size = Mathf.Clamp(resolution, 64, 256);
                var albedo = new Texture2D(size, size, TextureFormat.RGBA32, mipChain: true)
                {
                    name = $"Circus Blocking Obstacle Top Texture {variant}",
                    wrapMode = TextureWrapMode.Repeat,
                    filterMode = FilterMode.Trilinear,
                    anisoLevel = 4
                };
                var height = new float[size * size];
                Color baseColor = variant switch
                {
                    0 => new Color(0.58f, 0.06f, 0.045f, 1f),
                    1 => new Color(0.02f, 0.25f, 0.42f, 1f),
                    2 => new Color(0.52f, 0.40f, 0.12f, 1f),
                    _ => new Color(0.11f, 0.11f, 0.14f, 1f)
                };
                Color accent = new(0.86f, 0.79f, 0.58f, 1f);
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float u = (x + 0.5f) / size * 2f - 1f;
                        float v = (y + 0.5f) / size * 2f - 1f;
                        Color color = baseColor;
                        if (InsideStar(u, v, 0.72f, 0.32f))
                        {
                            color = accent;
                        }

                        float border = Mathf.Max(Mathf.Abs(u), Mathf.Abs(v));
                        float borderWear = 0f;
                        if (border > 0.82f)
                        {
                            borderWear = Mathf.InverseLerp(0.82f, 1f, border);
                            color = Color.Lerp(color, new Color(0.78f, 0.57f, 0.18f, 1f), 0.75f);
                        }

                        float dirt = ValueNoise(x + variant * 17, y + 41, 23) * 0.28f;
                        float grain = WoodFiber((x + variant * 37) / (float)size, y / (float)size, variant + 401);
                        color = Color.Lerp(color, Color.black, dirt + 0.07f);
                        color += new Color(grain, grain, grain, 0f) * 0.045f;
                        albedo.SetPixel(x, y, ClampColor(color));
                        height[y * size + x] = Mathf.Clamp01(0.46f + borderWear * 0.08f + grain * 0.09f - dirt * 0.05f);
                    }
                }

                albedo.Apply(updateMipmaps: true, makeNoLongerReadable: true);
                Texture2D normal = CreateNormalTexture($"Circus Blocking Obstacle Top Normal {variant}", size, height, 3.2f);
                return new TextureMaps(albedo, normal, 0.20f);
            }

            private static Color[] BodyPalette(int variant)
            {
                Color red = new(0.48f, 0.04f, 0.035f, 1f);
                Color blue = new(0.02f, 0.20f, 0.34f, 1f);
                Color cream = new(0.70f, 0.62f, 0.43f, 1f);
                Color black = new(0.035f, 0.030f, 0.035f, 1f);
                Color gold = new(0.62f, 0.42f, 0.12f, 1f);
                return variant switch
                {
                    1 => new[] { blue, cream, red, cream },
                    2 => new[] { red, gold, black, cream },
                    3 => new[] { black, red, black, blue },
                    4 => new[] { cream, red, cream, blue },
                    _ => new[] { red, cream, blue, cream }
                };
            }

            private static bool InsideStar(float x, float y, float outerRadius, float innerRadius)
            {
                float angle = Mathf.Atan2(y, x) + Mathf.PI * 0.5f;
                float radius = Mathf.Sqrt(x * x + y * y);
                float spoke = Mathf.Abs(Mathf.Repeat(angle / (Mathf.PI * 2f) * 5f, 1f) - 0.5f) * 2f;
                float limit = Mathf.Lerp(outerRadius, innerRadius, spoke);
                return radius < limit;
            }

            private static float ValueNoise(int x, int y, int salt)
            {
                int hash = DeterministicSeed.Combine(x, y, salt);
                return DeterministicSeed.ToFloat01(hash);
            }

            private static float Hash01(int x, int y, int salt)
            {
                return DeterministicSeed.ToFloat01(DeterministicSeed.Combine(x, y, salt));
            }

            private static float WoodFiber(float u, float v, int seed)
            {
                float wave = Mathf.Sin((u * 42f + ValueNoise(Mathf.FloorToInt(v * 48f), seed, 907) * 3.5f) * Mathf.PI);
                float fine = ValueNoise(Mathf.FloorToInt(u * 180f), Mathf.FloorToInt(v * 72f), seed + 37) - 0.5f;
                return wave * 0.18f + fine * 0.45f;
            }

            private static Texture2D CreateNormalTexture(string name, int size, float[] height, float strength)
            {
                var texture = new Texture2D(size, size, TextureFormat.RGBA32, mipChain: true, linear: true)
                {
                    name = name,
                    wrapMode = TextureWrapMode.Repeat,
                    filterMode = FilterMode.Trilinear,
                    anisoLevel = 4
                };

                Color32[] pixels = new Color32[size * size];
                for (int y = 0; y < size; y++)
                {
                    int up = (y + 1) % size;
                    int down = (y - 1 + size) % size;
                    for (int x = 0; x < size; x++)
                    {
                        int right = (x + 1) % size;
                        int left = (x - 1 + size) % size;
                        float dx = height[y * size + right] - height[y * size + left];
                        float dy = height[up * size + x] - height[down * size + x];
                        Vector3 normal = new Vector3(-dx * strength, -dy * strength, 1f).normalized;
                        pixels[y * size + x] = new Color32(
                            (byte)Mathf.RoundToInt((normal.x * 0.5f + 0.5f) * 255f),
                            (byte)Mathf.RoundToInt((normal.y * 0.5f + 0.5f) * 255f),
                            (byte)Mathf.RoundToInt(normal.z * 255f),
                            255);
                    }
                }

                texture.SetPixels32(pixels);
                texture.Apply(updateMipmaps: true, makeNoLongerReadable: true);
                return texture;
            }

            private static Color ClampColor(Color color)
            {
                return new Color(
                    Mathf.Clamp01(color.r),
                    Mathf.Clamp01(color.g),
                    Mathf.Clamp01(color.b),
                    Mathf.Clamp01(color.a));
            }

            private readonly struct TextureMaps
            {
                public TextureMaps(Texture2D albedo, Texture2D normal, float smoothness)
                {
                    Albedo = albedo;
                    Normal = normal;
                    Smoothness = Mathf.Clamp01(smoothness);
                }

                public Texture2D Albedo { get; }
                public Texture2D Normal { get; }
                public float Smoothness { get; }
            }
        }
    }

    public sealed class GeneratedWorldObstacleCleanup : MonoBehaviour
    {
        private readonly List<UnityEngine.Object> ownedObjects = new();

        public void Capture(IReadOnlyList<UnityEngine.Object> objects)
        {
            ownedObjects.Clear();
            if (objects == null)
            {
                return;
            }

            for (int i = 0; i < objects.Count; i++)
            {
                if (objects[i] != null)
                {
                    ownedObjects.Add(objects[i]);
                }
            }
        }

        private void OnDestroy()
        {
            for (int i = 0; i < ownedObjects.Count; i++)
            {
                UnityEngine.Object owned = ownedObjects[i];
                if (owned == null)
                {
                    continue;
                }

                RuntimeObjectFactory.Release(owned);
            }

            ownedObjects.Clear();
        }
    }
}
