using System.Collections.Generic;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using TheCircussyOne.Runtime;
using TheCircussyOne.Visuals;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public sealed class BlockyCircusTerrainSandboxWindow : EditorWindow
{
    private const string RootName = "[Generated] Blocky Circus Terrain Sandbox";
    private const string PreviewCameraName = "[Preview] Blocky Terrain Camera";
    private const string PreviewLightName = "[Preview] Blocky Terrain Light";
    private const string EnemyPrefabPath = "Assets/Game/Prefabs/Enemy.prefab";
    private const float SurfaceProbeHeight = 320f;
    private const float SurfaceProbeDepth = 960f;
    private const float RampVisualSkirtDepth = 2f;
    private const float RampVisualSkirtOverlap = 0.125f;
    private static readonly int[] ReviewSeeds = { 1, 2, 3, 4, 5 };

    private int seed = 1;
    private bool terrainOnly;
    private bool showSmoothBoundaryRim = true;
    private float boundaryRimWidth = 9f;
    private int boundaryRimSegments = 160;
    private bool showDebugOverlay = true;
    private bool showDecoration;
    private bool showSampleRewards = true;
    private bool showSampleEnemies;
    private int sampleRewardCount = 48;
    private int sampleEnemyCount = 48;
    private bool selectAfterGenerate = true;
    private BlockyCircusGroundsSandboxResult lastSandbox;
    private GameObject lastRoot;

    [MenuItem("Tools/The Circussy One/Debug/Blocky Circus Terrain Sandbox")]
    public static void Open()
    {
        var window = GetWindow<BlockyCircusTerrainSandboxWindow>();
        window.titleContent = new GUIContent("Blocky Terrain Sandbox");
        window.minSize = new Vector2(420f, 320f);
        window.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Blocky Circus Terrain Sandbox", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Dev-only terrain grammar preview at the Act I target footprint: 256u radius, 4u grid spacing, 32u max height, 30 degree readable ramp cap. Use the isolated preview scene for review so the prototype gameplay scene stays untouched.",
            MessageType.Info);

        DrawSceneSection();
        DrawSeedReviewSection();
        DrawTerrainLayersSection();
        DrawReviewOverlaysSection();
        DrawPreviewContentSection();
        DrawReviewGuidance();
    }

    private void DrawSceneSection()
    {
        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Scene", EditorStyles.boldLabel);
        if (GUILayout.Button("Open Isolated Preview Scene", GUILayout.Height(32f)))
        {
            OpenIsolatedPreviewScene();
        }

        if (GUILayout.Button("Generate Movement Playtest Scene", GUILayout.Height(32f)))
        {
            GenerateMovementPlaytestScene();
        }

        selectAfterGenerate = EditorGUILayout.Toggle("Select Generated Root", selectAfterGenerate);
        EditorGUILayout.HelpBox(
            "This creates an unsaved empty scene with preview camera and lighting, then generates the sandbox there. It does not create a scene asset unless you explicitly save it.",
            MessageType.None);
    }

    private void DrawSeedReviewSection()
    {
        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Seed Review", EditorStyles.boldLabel);
        seed = EditorGUILayout.IntField("Seed", seed);

        using (new EditorGUILayout.HorizontalScope())
        {
            for (int i = 0; i < ReviewSeeds.Length; i++)
            {
                int reviewSeed = ReviewSeeds[i];
                if (GUILayout.Button($"Seed {reviewSeed}", GUILayout.Height(26f)))
                {
                    seed = reviewSeed;
                    Generate();
                }
            }
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Generate Sandbox", GUILayout.Height(32f)))
            {
                Generate();
            }

            if (GUILayout.Button("Clear Sandbox", GUILayout.Height(32f)))
            {
                ClearExisting();
            }
        }
    }

    private void DrawTerrainLayersSection()
    {
        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Terrain Layers", EditorStyles.boldLabel);
        terrainOnly = EditorGUILayout.Toggle("Terrain Only", terrainOnly);
        showSmoothBoundaryRim = EditorGUILayout.Toggle("Smooth Boundary Rim", showSmoothBoundaryRim);
        using (new EditorGUI.DisabledScope(!showSmoothBoundaryRim))
        {
            boundaryRimWidth = EditorGUILayout.Slider("Boundary Rim Width", boundaryRimWidth, 2f, 24f);
            boundaryRimSegments = EditorGUILayout.IntSlider("Boundary Rim Segments", boundaryRimSegments, 48, 256);
        }
    }

    private void DrawReviewOverlaysSection()
    {
        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Review Overlays", EditorStyles.boldLabel);
        using (new EditorGUI.DisabledScope(terrainOnly))
        {
            showDebugOverlay = EditorGUILayout.Toggle("Debug Overlay", showDebugOverlay);
        }
    }

    private void DrawPreviewContentSection()
    {
        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Preview Content", EditorStyles.boldLabel);
        using (new EditorGUI.DisabledScope(terrainOnly))
        {
            showDecoration = EditorGUILayout.Toggle("Decoration Markers", showDecoration);
            showSampleRewards = EditorGUILayout.Toggle("Sample World Rewards", showSampleRewards);
            using (new EditorGUI.DisabledScope(!showSampleRewards))
            {
                sampleRewardCount = EditorGUILayout.IntSlider("Reward Preview Count", sampleRewardCount, 8, 120);
            }

            showSampleEnemies = EditorGUILayout.Toggle("Sample Enemies", showSampleEnemies);
            using (new EditorGUI.DisabledScope(!showSampleEnemies))
            {
                sampleEnemyCount = EditorGUILayout.IntSlider("Enemy Preview Count", sampleEnemyCount, 8, 160);
            }
        }
    }

    private static void DrawReviewGuidance()
    {
        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Recommended Review Passes", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Terrain Only + Smooth Boundary Rim: judge the arena silhouette and blocky terrain language.\nDebug Overlay: inspect ramp aisles, ledges, and reward pockets.\nRewards + Enemies: inspect grounding, slope alignment, and gameplay readability.",
            MessageType.None);
    }

    private void OpenIsolatedPreviewScene()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreatePreviewSceneHelpers();
        Generate();
    }

    private void Generate()
    {
        ClearExisting();

        BlockyCircusGroundsSandboxSettings settings = BlockyCircusGroundsSandboxSettings.Default(seed);
        BlockyCircusGroundsSandboxResult sandbox = BlockyCircusGroundsGrammar.Generate(settings);
        GameObject root = new GameObject(RootName);
        lastSandbox = sandbox;
        lastRoot = root;
        root.hideFlags = HideFlags.DontSaveInBuild;
        Undo.RegisterCreatedObjectUndo(root, "Generate Blocky Circus Terrain Sandbox");

        Material topMaterial = ResolveTopMaterial();
        GeneratedWorldTerrainBuildResult terrain = GeneratedWorldTerrainBuilder.Build(
            sandbox.Map,
            new GeneratedWorldTerrainBuildSettings(topMaterial, topMaterial, chunkQuadsPerSide: 16));
        if (terrain.Success && terrain.Root != null)
        {
            terrain.Root.name = "Terrain Mesh";
            terrain.Root.transform.SetParent(root.transform, worldPositionStays: true);
        }

        BuildConstructedRampMeshes(root.transform, sandbox.Map, sandbox.Pieces, topMaterial);
        if (showSmoothBoundaryRim)
        {
            BuildSmoothBoundaryRim(root.transform, sandbox.Map, topMaterial, boundaryRimWidth, boundaryRimSegments);
        }

        List<Collider> surfaceColliders = TerrainSurfaceColliders(root.transform);
        Physics.SyncTransforms();

        if (!terrainOnly && showDebugOverlay)
        {
            BuildDebugOverlay(root.transform, sandbox.Pieces);
        }

        if (!terrainOnly && showDecoration)
        {
            BuildDecoration(root.transform, sandbox.Pieces, seed, surfaceColliders);
        }

        if (!terrainOnly && showSampleRewards)
        {
            BuildSampleWorldRewards(root.transform, sandbox, seed, sampleRewardCount, surfaceColliders);
        }

        if (!terrainOnly && showSampleEnemies)
        {
            BuildSampleEnemies(root.transform, sandbox.Map, seed, sampleEnemyCount, surfaceColliders);
        }

        if (selectAfterGenerate)
        {
            Selection.activeGameObject = root;
            SceneView.lastActiveSceneView?.FrameSelected();
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    private void GenerateMovementPlaytestScene()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreatePreviewSceneHelpers();

        bool previousTerrainOnly = terrainOnly;
        bool previousShowDebugOverlay = showDebugOverlay;
        bool previousShowDecoration = showDecoration;
        bool previousShowSampleRewards = showSampleRewards;
        bool previousShowSampleEnemies = showSampleEnemies;
        try
        {
            terrainOnly = true;
            showDebugOverlay = false;
            showDecoration = false;
            showSampleRewards = false;
            showSampleEnemies = false;
            Generate();
        }
        finally
        {
            terrainOnly = previousTerrainOnly;
            showDebugOverlay = previousShowDebugOverlay;
            showDecoration = previousShowDecoration;
            showSampleRewards = previousShowSampleRewards;
            showSampleEnemies = previousShowSampleEnemies;
        }

        if (lastSandbox == null || lastRoot == null)
        {
            return;
        }

        BuildMovementPlaytest(lastRoot.transform, lastSandbox);
        Selection.activeGameObject = lastRoot;
        SceneView.lastActiveSceneView?.FrameSelected();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    private static void BuildMovementPlaytest(Transform parent, BlockyCircusGroundsSandboxResult sandbox)
    {
        GameConfig gameConfig = LoadPlaytestConfig<GameConfig>(TheCircussyOneAssetPaths.GameConfigPath);
        CameraConfig cameraConfig = LoadPlaytestConfig<CameraConfig>(TheCircussyOneAssetPaths.CameraConfigPath);
        DamageFeedbackVisualConfig damageFeedbackConfig = LoadPlaytestConfig<DamageFeedbackVisualConfig>(TheCircussyOneAssetPaths.DamageFeedbackVisualConfigPath);
        ActorMotionVisualConfig actorMotionConfig = LoadPlaytestConfig<ActorMotionVisualConfig>(TheCircussyOneAssetPaths.ActorMotionVisualConfigPath);
        VfxVisualConfig vfxConfig = LoadPlaytestConfig<VfxVisualConfig>(TheCircussyOneAssetPaths.VfxVisualConfigPath);

        GameObject root = new GameObject("Movement Playtest");
        root.hideFlags = HideFlags.DontSaveInBuild;
        root.transform.SetParent(parent, false);
        Undo.RegisterCreatedObjectUndo(root, "Generate Movement Playtest");

        PlayerView player = CreatePlaytestPlayer(
            root.transform,
            sandbox.Map.PlayerStartPosition + Vector3.up * 0.05f,
            gameConfig,
            damageFeedbackConfig,
            vfxConfig);
        CameraView cameraView = FindOrCreatePlaytestCamera(player, cameraConfig);

        GameObject runnerObject = new GameObject("Movement Playtest Runner");
        runnerObject.hideFlags = HideFlags.DontSaveInBuild;
        runnerObject.transform.SetParent(root.transform, false);
        BlockyCircusTerrainSandboxPlaytestRunner runner = runnerObject.AddComponent<BlockyCircusTerrainSandboxPlaytestRunner>();
        runner.Configure(gameConfig, cameraConfig, damageFeedbackConfig, actorMotionConfig, vfxConfig, player, cameraView);
    }

    private static PlayerView CreatePlaytestPlayer(
        Transform parent,
        Vector3 position,
        GameConfig gameConfig,
        DamageFeedbackVisualConfig damageFeedbackConfig,
        VfxVisualConfig vfxConfig)
    {
        GameObject playerObject = new GameObject("Sandbox Playtest Player");
        playerObject.hideFlags = HideFlags.DontSaveInBuild;
        playerObject.transform.SetParent(parent, false);
        playerObject.transform.position = position;

        var bodyScaleRoot = new GameObject("Body Scale Root");
        bodyScaleRoot.hideFlags = HideFlags.DontSaveInBuild;
        bodyScaleRoot.transform.SetParent(playerObject.transform, false);

        GameObject bodyObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        bodyObject.name = "Body";
        bodyObject.hideFlags = HideFlags.DontSaveInBuild;
        bodyObject.transform.SetParent(bodyScaleRoot.transform, false);
        bodyObject.transform.localPosition = Vector3.up;
        Collider bodyCollider = bodyObject.GetComponent<Collider>();
        if (bodyCollider != null)
        {
            UnityEngine.Object.DestroyImmediate(bodyCollider);
        }

        Renderer bodyRenderer = bodyObject.GetComponent<Renderer>();
        Material playerMaterial = AssetDatabase.LoadAssetAtPath<Material>(TheCircussyOneAssetPaths.PlayerMaterialPath);
        if (bodyRenderer != null && playerMaterial != null)
        {
            bodyRenderer.sharedMaterial = playerMaterial;
        }
        else if (bodyRenderer != null)
        {
            bodyRenderer.sharedMaterial = CreateMaterial(
                "Sandbox Playtest Player Fallback Material",
                new Color(0.36f, 0.76f, 1f, 1f),
                0.2f);
        }

        CharacterController controller = playerObject.AddComponent<CharacterController>();
        controller.height = 2f;
        controller.radius = 0.55f;
        controller.center = Vector3.up;

        GameObject healthBarObject = new GameObject("Health Bar");
        healthBarObject.hideFlags = HideFlags.DontSaveInBuild;
        healthBarObject.transform.SetParent(playerObject.transform, false);
        healthBarObject.AddComponent<HealthBarView>();

        playerObject.AddComponent<ActorBodyView>();
        PlayerView player = playerObject.AddComponent<PlayerView>();
        player.ApplyConfig(gameConfig, damageFeedbackConfig);
        player.ApplyMoveDustVfxConfig(vfxConfig);
        player.ApplyJumpTrailVfxConfig(vfxConfig);

        int playerLayer = LayerMask.NameToLayer(GameLayers.Player);
        if (playerLayer >= 0)
        {
            SetLayerRecursively(playerObject, playerLayer);
        }

        return player;
    }

    private static CameraView FindOrCreatePlaytestCamera(PlayerView player, CameraConfig cameraConfig)
    {
        GameObject cameraObject = GameObject.Find(PreviewCameraName);
        if (cameraObject == null)
        {
            cameraObject = new GameObject(PreviewCameraName);
            cameraObject.hideFlags = HideFlags.DontSaveInBuild;
        }

        Camera camera = cameraObject.GetComponent<Camera>();
        if (camera == null)
        {
            camera = cameraObject.AddComponent<Camera>();
        }

        camera.clearFlags = CameraClearFlags.Skybox;
        camera.nearClipPlane = 0.3f;
        camera.farClipPlane = Mathf.Max(2400f, cameraConfig != null ? cameraConfig.farClipPlane : 2400f);
        cameraObject.tag = "MainCamera";

        CameraView cameraView = cameraObject.GetComponent<CameraView>();
        if (cameraView == null)
        {
            cameraView = cameraObject.AddComponent<CameraView>();
        }

        cameraView.ApplyInitialPose(player, cameraConfig);
        return cameraView;
    }

    private static T LoadPlaytestConfig<T>(string path) where T : ScriptableObject
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset != null)
        {
            return asset;
        }

        T fallback = ScriptableObject.CreateInstance<T>();
        fallback.hideFlags = HideFlags.DontSave;
        return fallback;
    }

    private static void SetLayerRecursively(GameObject root, int layer)
    {
        root.layer = layer;
        for (int i = 0; i < root.transform.childCount; i++)
        {
            SetLayerRecursively(root.transform.GetChild(i).gameObject, layer);
        }
    }

    private static void BuildConstructedRampMeshes(
        Transform parent,
        GeneratedWorldMap map,
        IReadOnlyList<BlockyCircusGroundsPiece> pieces,
        Material material)
    {
        GameObject root = new GameObject("Constructed Ramp Meshes");
        root.hideFlags = HideFlags.DontSaveInBuild;
        root.transform.SetParent(parent, false);

        int rampIndex = 0;
        for (int i = 0; i < pieces.Count; i++)
        {
            BlockyCircusGroundsPiece piece = pieces[i];
            if (piece.Kind != BlockyCircusGroundsPieceKind.RampAisle)
            {
                continue;
            }

            BuildRampWedge(root.transform, map, piece, material, rampIndex);
            rampIndex++;
        }
    }

    private static void BuildRampWedge(
        Transform parent,
        GeneratedWorldMap map,
        BlockyCircusGroundsPiece piece,
        Material material,
        int index)
    {
        Vector3 direction = new Vector3(piece.Direction.x, 0f, piece.Direction.y);
        if (direction.sqrMagnitude < 0.0001f)
        {
            direction = Vector3.forward;
        }

        direction.Normalize();
        Vector3 side = new Vector3(-direction.z, 0f, direction.x);
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

        GameObject ramp = new GameObject($"Constructed Ramp Wedge {index:000}");
        ramp.hideFlags = HideFlags.DontSaveInBuild;
        ramp.transform.SetParent(parent, worldPositionStays: false);
        ramp.transform.position = piece.Center;

        MeshFilter meshFilter = ramp.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = visualMesh;
        MeshRenderer meshRenderer = ramp.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = material;
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
            var triangles = new List<int>(includeVisualSkirt ? 144 : 72);

            AddDoubleSidedQuad(vertices, normals, triangles, lowLeft, highLeft, highRight, lowRight);
            AddDoubleSidedTriangle(vertices, normals, triangles, lowLeft, highLeftBottom, highLeft);
            AddDoubleSidedTriangle(vertices, normals, triangles, lowRight, highRight, highRightBottom);
            AddDoubleSidedQuad(vertices, normals, triangles, highLeftBottom, highRightBottom, highRight, highLeft);
            AddDoubleSidedQuad(vertices, normals, triangles, lowLeft, lowRight, highRightBottom, highLeftBottom);

            if (includeVisualSkirt)
            {
                AddRampVisualSkirts(vertices, normals, triangles, map, piece, lowLeft, lowRight, highLeftBottom, highRightBottom);
            }

            var uvs = new List<Vector2>(vertices.Count);
            float uvScale = 1f / Mathf.Max(0.01f, map != null ? map.GridSpacing : 1f);
            for (int vertexIndex = 0; vertexIndex < vertices.Count; vertexIndex++)
            {
                Vector3 vertex = vertices[vertexIndex];
                uvs.Add(new Vector2(
                    Vector3.Dot(vertex, side) * uvScale,
                    Vector3.Dot(vertex, direction) * uvScale));
            }

            var mesh = new Mesh
            {
                name = meshName
            };
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            return mesh;
        }
    }

    private static void AddRampVisualSkirts(
        List<Vector3> vertices,
        List<Vector3> normals,
        List<int> triangles,
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

        AddDoubleSidedQuad(vertices, normals, triangles, lowLeftBottom, lowLeft, lowRight, lowRightBottom);
        AddDoubleSidedQuad(vertices, normals, triangles, lowLeftBottom, highLeftBottomSkirt, highLeftBottom, lowLeft);
        AddDoubleSidedQuad(vertices, normals, triangles, lowRight, highRightBottom, highRightBottomSkirt, lowRightBottom);
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

    private static void BuildSmoothBoundaryRim(
        Transform parent,
        GeneratedWorldMap map,
        Material material,
        float rimWidth,
        int segmentCount)
    {
        if (map == null)
        {
            return;
        }

        int segments = Mathf.Clamp(segmentCount, 48, 256);
        float outerRadius = map.PlayableRadius + map.GridSpacing * 0.75f;
        float innerRadius = Mathf.Max(map.GridSpacing, outerRadius - Mathf.Max(1f, rimWidth));
        float topY = 0.12f;
        float bottomY = -Mathf.Max(3f, map.GridSpacing * 1.5f);

        var vertices = new List<Vector3>(segments * 12);
        var normals = new List<Vector3>(segments * 12);
        var uvs = new List<Vector2>(segments * 12);
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
            indexFormat = vertices.Count > ushort.MaxValue
                ? UnityEngine.Rendering.IndexFormat.UInt32
                : UnityEngine.Rendering.IndexFormat.UInt16
        };
        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateBounds();

        GameObject rim = new GameObject("Smooth Boundary Rim");
        rim.hideFlags = HideFlags.DontSaveInBuild;
        rim.transform.SetParent(parent, worldPositionStays: false);
        MeshFilter meshFilter = rim.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = mesh;
        MeshRenderer meshRenderer = rim.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = material;

        Vector3 CirclePoint(float radius, float y, float angle)
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

    private static List<Collider> TerrainSurfaceColliders(Transform root)
    {
        var colliders = new List<Collider>();
        if (root == null)
        {
            return colliders;
        }

        Collider[] allColliders = root.GetComponentsInChildren<Collider>(includeInactive: false);
        for (int i = 0; i < allColliders.Length; i++)
        {
            Collider collider = allColliders[i];
            if (collider != null && collider.enabled)
            {
                colliders.Add(collider);
            }
        }

        return colliders;
    }

    private static void AddDoubleSidedQuad(
        List<Vector3> vertices,
        List<Vector3> normals,
        List<int> triangles,
        Vector3 a,
        Vector3 b,
        Vector3 c,
        Vector3 d)
    {
        AddFace(vertices, normals, triangles, a, b, c, d);
        AddFace(vertices, normals, triangles, d, c, b, a);
    }

    private static void AddDoubleSidedTriangle(
        List<Vector3> vertices,
        List<Vector3> normals,
        List<int> triangles,
        Vector3 a,
        Vector3 b,
        Vector3 c)
    {
        AddFace(vertices, normals, triangles, a, b, c);
        AddFace(vertices, normals, triangles, c, b, a);
    }

    private static void AddFace(
        List<Vector3> vertices,
        List<Vector3> normals,
        List<int> triangles,
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
        }

        for (int i = 1; i < faceVertices.Length - 1; i++)
        {
            triangles.Add(start);
            triangles.Add(start + i);
            triangles.Add(start + i + 1);
        }
    }

    private static void ClearExisting()
    {
        GameObject existing = GameObject.Find(RootName);
        if (existing == null)
        {
            return;
        }

        Undo.DestroyObjectImmediate(existing);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    private static void CreatePreviewSceneHelpers()
    {
        GameObject lightObject = new GameObject(PreviewLightName);
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.15f;
        light.color = new Color(1f, 0.94f, 0.82f, 1f);
        lightObject.transform.rotation = Quaternion.Euler(52f, -35f, 0f);

        GameObject cameraObject = new GameObject(PreviewCameraName);
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.Skybox;
        camera.fieldOfView = 50f;
        camera.nearClipPlane = 0.3f;
        camera.farClipPlane = 2400f;
        cameraObject.transform.position = new Vector3(0f, 380f, -620f);
        cameraObject.transform.rotation = Quaternion.LookRotation(
            new Vector3(0f, 0f, 0f) - cameraObject.transform.position,
            Vector3.up);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    private static void BuildDebugOverlay(Transform parent, IReadOnlyList<BlockyCircusGroundsPiece> pieces)
    {
        GameObject root = new GameObject("Debug Overlay");
        root.transform.SetParent(parent, false);

        Material ramp = CreateTransparentMaterial("Sandbox Overlay Ramp", new Color(0.12f, 0.45f, 1f, 0.34f), 0.08f);
        Material ledge = CreateTransparentMaterial("Sandbox Overlay Ledge", new Color(0.9f, 0.12f, 0.1f, 0.3f), 0.08f);
        Material reward = CreateTransparentMaterial("Sandbox Overlay Reward", new Color(1f, 0.86f, 0.08f, 0.42f), 0.08f);

        for (int i = 0; i < pieces.Count; i++)
        {
            BlockyCircusGroundsPiece piece = pieces[i];
            Material material = piece.Kind switch
            {
                BlockyCircusGroundsPieceKind.RampAisle => ramp,
                BlockyCircusGroundsPieceKind.VerticalLedge => ledge,
                BlockyCircusGroundsPieceKind.RewardPocket => reward,
                _ => null
            };
            if (material == null)
            {
                continue;
            }

            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.name = $"{piece.Kind} Overlay";
            marker.transform.SetParent(root.transform, worldPositionStays: true);
            marker.transform.SetPositionAndRotation(
                piece.Center + Vector3.up * 0.08f,
                OverlayRotationFor(piece));
            marker.transform.localScale = new Vector3(
                Mathf.Max(0.2f, piece.Size.x),
                0.08f,
                Mathf.Max(0.2f, piece.Size.z));
            Object.DestroyImmediate(marker.GetComponent<Collider>());
            marker.GetComponent<MeshRenderer>().sharedMaterial = material;
        }
    }

    private static void BuildDecoration(
        Transform parent,
        IReadOnlyList<BlockyCircusGroundsPiece> pieces,
        int seed,
        IReadOnlyList<Collider> surfaceColliders)
    {
        GameObject root = new GameObject("Decoration Markers");
        root.transform.SetParent(parent, false);
        Material treeTrunk = CreateMaterial("Sandbox Tree Trunk", new Color(0.32f, 0.18f, 0.08f, 1f), 0.25f);
        Material treeTop = CreateMaterial("Sandbox Tree Top", new Color(0.05f, 0.34f, 0.16f, 1f), 0.18f);

        for (int i = 0; i < pieces.Count; i++)
        {
            BlockyCircusGroundsPiece piece = pieces[i];
            if (piece.Kind == BlockyCircusGroundsPieceKind.TreeGrove)
            {
                BuildTreeGrove(root.transform, piece, seed, surfaceColliders, treeTrunk, treeTop);
            }
        }
    }

    private static void BuildSampleWorldRewards(
        Transform parent,
        BlockyCircusGroundsSandboxResult sandbox,
        int seed,
        int count,
        IReadOnlyList<Collider> surfaceColliders)
    {
        if (sandbox == null || count <= 0)
        {
            return;
        }

        GameObject root = new GameObject("Sample World Rewards");
        root.transform.SetParent(parent, false);

        List<SampleRewardDefinition> definitions = LoadSampleRewardDefinitions();

        List<BlockyCircusGroundsPiece> pockets = RewardPockets(sandbox.Pieces);
        if (pockets.Count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                BlockyCircusGroundsPiece pocket = pockets[i % pockets.Count];
                Vector3 position = RewardPositionForPocket(pocket, seed, i);
                BuildRewardMarker(root.transform, position, i, surfaceColliders, definitions);
            }

            return;
        }

        List<int> rewardSamples = SamplesWithMask(sandbox.Map, GeneratedWorldMask.RewardSafe);
        SelectSamplePositions(sandbox.Map, rewardSamples, seed, count, Mathf.Max(18f, sandbox.Map.GridSpacing * 3f), (index, order) =>
        {
            BuildRewardMarker(root.transform, sandbox.Map.PositionForIndex(index), order, surfaceColliders, definitions);
        });
    }

    private static void BuildSampleEnemies(
        Transform parent,
        GeneratedWorldMap map,
        int seed,
        int count,
        IReadOnlyList<Collider> surfaceColliders)
    {
        if (map == null || count <= 0)
        {
            return;
        }

        GameObject root = new GameObject("Sample Enemies");
        root.transform.SetParent(parent, false);

        GameObject enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(EnemyPrefabPath);
        Material enemy = enemyPrefab == null
            ? AssetDatabase.LoadAssetAtPath<Material>("Assets/Game/Materials/Enemy.mat")
                ?? CreateMaterial("Sandbox Enemy", new Color(0.86f, 0.16f, 0.22f, 1f), 0.18f)
            : null;
        Material face = enemyPrefab == null
            ? AssetDatabase.LoadAssetAtPath<Material>("Assets/Game/Materials/Face.mat")
                ?? CreateMaterial("Sandbox Enemy Face", Color.white, 0.08f)
            : null;

        List<int> spawnSamples = SamplesWithMask(map, GeneratedWorldMask.SpawnSafe);
        SelectSamplePositions(map, spawnSamples, seed + 1931, count, Mathf.Max(24f, map.GridSpacing * 4f), (index, order) =>
        {
            GeneratedWorldSample sample = map.Samples[index];
            Vector3 position = map.PositionForIndex(index);
            BuildEnemyMarker(root.transform, position, sample.Normal, order, surfaceColliders, enemyPrefab, enemy, face);
        });
    }

    private static List<SampleRewardDefinition> LoadSampleRewardDefinitions()
    {
        var definitions = new List<SampleRewardDefinition>(9);
        AddChest(definitions, TheCircussyOneAssetPaths.LockedChestPath);
        AddChest(definitions, TheCircussyOneAssetPaths.PremiumChestPath);
        AddChest(definitions, TheCircussyOneAssetPaths.OpenChestPath);
        AddTicketDeposit(definitions, TheCircussyOneAssetPaths.SmallTicketStackPath);
        AddTicketDeposit(definitions, TheCircussyOneAssetPaths.TicketRollPath);
        AddTicketDeposit(definitions, TheCircussyOneAssetPaths.JackpotCachePath);
        AddHealingProp(definitions, TheCircussyOneAssetPaths.SnackBoxPath);
        AddHealingProp(definitions, TheCircussyOneAssetPaths.SnackCartPath);

        if (definitions.Count == 0)
        {
            definitions.Add(new SampleRewardDefinition(
                "Chest Preview",
                PrimitiveType.Cube,
                "Chest Body",
                WorldPropPlacementProfile.ChestDefault(),
                CreateConfiguredMaterial("Sandbox Chest Fallback", Color.white, new Color(1f, 0.78f, 0.26f, 1f), 0.35f),
                AddChestLid));
            definitions.Add(new SampleRewardDefinition(
                "Ticket Deposit Preview",
                PrimitiveType.Cube,
                "Ticket Visual",
                WorldPropPlacementProfile.TicketDepositDefault(TicketDepositTier.Small),
                CreateConfiguredMaterial("Sandbox Ticket Fallback", Color.white, new Color(1f, 0.86f, 0.22f, 1f), 0.7f),
                null));
            definitions.Add(new SampleRewardDefinition(
                "Healing Prop Preview",
                PrimitiveType.Sphere,
                "Healing Prop Visual",
                WorldPropPlacementProfile.HealingPropDefault(),
                CreateConfiguredMaterial("Sandbox Healing Fallback", Color.white, new Color(1f, 0.2f, 0.42f, 1f), 0.9f),
                null));
        }

        return definitions;
    }

    private static void AddChest(List<SampleRewardDefinition> definitions, string path)
    {
        ChestDefinition definition = AssetDatabase.LoadAssetAtPath<ChestDefinition>(path);
        if (definition == null)
        {
            return;
        }

        definitions.Add(new SampleRewardDefinition(
            definition.DisplayName,
            PrimitiveType.Cube,
            "Chest Body",
            definition.placement,
            CreateConfiguredMaterial($"{definition.DisplayName} Sandbox Preview", definition.visualColor, definition.emissionColor, definition.emissionStrength),
            AddChestLid));
    }

    private static void AddTicketDeposit(List<SampleRewardDefinition> definitions, string path)
    {
        TicketDepositDefinition definition = AssetDatabase.LoadAssetAtPath<TicketDepositDefinition>(path);
        if (definition == null)
        {
            return;
        }

        definitions.Add(new SampleRewardDefinition(
            definition.DisplayName,
            PrimitiveType.Cube,
            "Ticket Visual",
            definition.placement,
            CreateConfiguredMaterial($"{definition.DisplayName} Sandbox Preview", definition.visualColor, definition.emissionColor, definition.emissionStrength),
            null));
    }

    private static void AddHealingProp(List<SampleRewardDefinition> definitions, string path)
    {
        HealingPropDefinition definition = AssetDatabase.LoadAssetAtPath<HealingPropDefinition>(path);
        if (definition == null)
        {
            return;
        }

        definitions.Add(new SampleRewardDefinition(
            definition.DisplayName,
            PrimitiveType.Sphere,
            "Healing Prop Visual",
            definition.placement,
            CreateConfiguredMaterial($"{definition.DisplayName} Sandbox Preview", definition.visualColor, definition.emissionColor, definition.emissionStrength),
            null));
    }

    private static List<BlockyCircusGroundsPiece> RewardPockets(IReadOnlyList<BlockyCircusGroundsPiece> pieces)
    {
        var pockets = new List<BlockyCircusGroundsPiece>();
        if (pieces == null)
        {
            return pockets;
        }

        for (int i = 0; i < pieces.Count; i++)
        {
            if (pieces[i].Kind == BlockyCircusGroundsPieceKind.RewardPocket)
            {
                pockets.Add(pieces[i]);
            }
        }

        return pockets;
    }

    private static Vector3 RewardPositionForPocket(BlockyCircusGroundsPiece pocket, int seed, int index)
    {
        int localSeed = TheCircussyOne.Rules.DeterministicSeed.Combine(seed, index, 9241);
        float x = TheCircussyOne.Rules.DeterministicSeed.ToSignedFloat(localSeed) * Mathf.Max(0.5f, pocket.Size.x * 0.36f);
        float z = TheCircussyOne.Rules.DeterministicSeed.ToSignedFloat(localSeed + 1) * Mathf.Max(0.5f, pocket.Size.z * 0.36f);
        return pocket.Center + new Vector3(x, 0.08f, z);
    }

    private static List<int> SamplesWithMask(GeneratedWorldMap map, GeneratedWorldMask mask)
    {
        var samples = new List<int>();
        if (map == null)
        {
            return samples;
        }

        for (int i = 0; i < map.Samples.Count; i++)
        {
            if (map.Samples[i].HasMask(mask) && !map.Samples[i].HasMask(GeneratedWorldMask.PlayerStartSafe))
            {
                samples.Add(i);
            }
        }

        return samples;
    }

    private static void SelectSamplePositions(
        GeneratedWorldMap map,
        List<int> candidates,
        int seed,
        int count,
        float spacing,
        System.Action<int, int> build)
    {
        if (map == null || candidates == null || build == null || candidates.Count == 0)
        {
            return;
        }

        candidates.Sort((left, right) => SampleSortKey(seed, left).CompareTo(SampleSortKey(seed, right)));
        var selected = new List<Vector3>(count);
        int emitted = 0;
        float currentSpacing = Mathf.Max(0f, spacing);
        while (emitted < count && currentSpacing >= 0f)
        {
            for (int i = 0; i < candidates.Count && emitted < count; i++)
            {
                int candidate = candidates[i];
                Vector3 position = map.PositionForIndex(candidate);
                if (!HasSpacing(position, selected, currentSpacing))
                {
                    continue;
                }

                selected.Add(position);
                build(candidate, emitted);
                emitted++;
            }

            if (emitted >= count || currentSpacing <= 0f)
            {
                break;
            }

            currentSpacing -= Mathf.Max(map.GridSpacing, spacing * 0.25f);
        }
    }

    private static int SampleSortKey(int seed, int sampleIndex)
    {
        return TheCircussyOne.Rules.DeterministicSeed.Combine(seed, sampleIndex, 6211);
    }

    private static bool HasSpacing(Vector3 position, IReadOnlyList<Vector3> selected, float spacing)
    {
        if (spacing <= 0f)
        {
            return true;
        }

        float spacingSqr = spacing * spacing;
        for (int i = 0; i < selected.Count; i++)
        {
            Vector3 delta = position - selected[i];
            delta.y = 0f;
            if (delta.sqrMagnitude < spacingSqr)
            {
                return false;
            }
        }

        return true;
    }

    private static void BuildRewardMarker(
        Transform parent,
        Vector3 position,
        int index,
        IReadOnlyList<Collider> surfaceColliders,
        IReadOnlyList<SampleRewardDefinition> definitions)
    {
        if (definitions == null || definitions.Count == 0)
        {
            return;
        }

        SampleRewardDefinition definition = definitions[index % definitions.Count];
        if (!TrySampleSurface(position, surfaceColliders, out RaycastHit hit))
        {
            return;
        }

        Vector3 rootPosition = WorldPropPlacementRules.GroundedRootPosition(hit.point, hit.normal, definition.Placement);
        Quaternion rootRotation = WorldPropPlacementRules.SurfaceRotation(hit.normal);
        WorldRewardPropBuildResult prop = WorldRewardPropBuilder.Create(
            definition.DisplayName,
            rootPosition,
            definition.Primitive,
            definition.BodyName,
            definition.Placement,
            placementService: null,
            definition.Material);
        prop.Root.name = $"Sample {definition.DisplayName} {index:000}";
        prop.Root.hideFlags = HideFlags.DontSaveInBuild;
        prop.Root.transform.SetPositionAndRotation(rootPosition, rootRotation);
        prop.Root.transform.SetParent(parent, worldPositionStays: true);
        definition.Decorate?.Invoke(prop);
    }

    private static void AddChestLid(WorldRewardPropBuildResult prop)
    {
        if (prop.VisualRoot == null || prop.Body == null)
        {
            return;
        }

        Vector3 bodyScale = prop.Body.transform.localScale;
        GameObject lid = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lid.name = "Lid";
        lid.transform.SetParent(prop.VisualRoot.transform, worldPositionStays: false);
        lid.transform.localPosition = Vector3.Scale(new Vector3(0f, 0.58f, 0f), bodyScale);
        lid.transform.localScale = Vector3.Scale(new Vector3(1.08f, 0.22f, 1.08f), bodyScale);
        Collider lidCollider = lid.GetComponent<Collider>();
        if (lidCollider != null)
        {
            Object.DestroyImmediate(lidCollider);
        }

        Renderer lidRenderer = lid.GetComponent<Renderer>();
        if (lidRenderer != null && prop.BodyRenderer != null)
        {
            lidRenderer.sharedMaterial = prop.BodyRenderer.sharedMaterial;
        }
    }

    private static void BuildEnemyMarker(
        Transform parent,
        Vector3 position,
        Vector3 normal,
        int index,
        IReadOnlyList<Collider> surfaceColliders,
        GameObject enemyPrefab,
        Material body,
        Material face)
    {
        if (TrySampleSurface(position, surfaceColliders, out RaycastHit hit))
        {
            position = hit.point;
            normal = hit.normal;
        }

        GameObject root = enemyPrefab != null
            ? (GameObject)PrefabUtility.InstantiatePrefab(enemyPrefab)
            : new GameObject($"Sample Enemy {index:000}");
        if (root == null)
        {
            return;
        }

        root.name = $"Sample Enemy {index:000}";
        root.hideFlags = HideFlags.DontSaveInBuild;
        root.transform.SetParent(parent, worldPositionStays: true);
        root.transform.SetPositionAndRotation(
            position,
            Quaternion.FromToRotation(Vector3.up, normal.sqrMagnitude > 0.0001f ? normal.normalized : Vector3.up));
        if (enemyPrefab == null)
        {
            AddPrimitive(root.transform, PrimitiveType.Capsule, "Body", new Vector3(0f, 1.0f, 0f), new Vector3(1.25f, 1.0f, 1.25f), body);
            AddPrimitive(root.transform, PrimitiveType.Sphere, "Left Eye", new Vector3(-0.24f, 1.35f, -0.56f), Vector3.one * 0.18f, face);
            AddPrimitive(root.transform, PrimitiveType.Sphere, "Right Eye", new Vector3(0.24f, 1.35f, -0.56f), Vector3.one * 0.18f, face);
        }
    }

    private static GameObject AddPrimitive(
        Transform parent,
        PrimitiveType primitive,
        string name,
        Vector3 localPosition,
        Vector3 localScale,
        Material material)
    {
        GameObject gameObject = GameObject.CreatePrimitive(primitive);
        gameObject.name = name;
        gameObject.transform.SetParent(parent, worldPositionStays: false);
        gameObject.transform.localPosition = localPosition;
        gameObject.transform.localRotation = Quaternion.identity;
        gameObject.transform.localScale = localScale;
        Collider collider = gameObject.GetComponent<Collider>();
        if (collider != null)
        {
            Object.DestroyImmediate(collider);
        }

        MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
        if (renderer != null && material != null)
        {
            renderer.sharedMaterial = material;
        }

        return gameObject;
    }

    private static Material CreateConfiguredMaterial(string name, Color color, Color emissionColor, float emissionStrength)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit")
            ?? Shader.Find("Standard");
        if (shader == null)
        {
            return null;
        }

        var material = new Material(shader)
        {
            name = name,
            hideFlags = HideFlags.DontSave
        };
        float clampedEmission = Mathf.Max(0f, emissionStrength);
        material.color = color;
        SetColorIfPresent(material, "_BaseColor", color);
        SetColorIfPresent(material, "_Color", color);
        SetColorIfPresent(material, "_EmissionColor", emissionColor);
        SetFloatIfPresent(material, "_EmissionStrength", clampedEmission);
        SetFloatIfPresent(material, "_EmissionSelfGlow", clampedEmission);
        SetFloatIfPresent(material, "_EmissionEnabled", clampedEmission > 0f ? 1f : 0f);
        if (clampedEmission > 0f)
        {
            material.EnableKeyword("_EMISSION");
            material.EnableKeyword("_EMISSION_ON");
        }

        return material;
    }

    private readonly struct SampleRewardDefinition
    {
        public SampleRewardDefinition(
            string displayName,
            PrimitiveType primitive,
            string bodyName,
            WorldPropPlacementProfile placement,
            Material material,
            System.Action<WorldRewardPropBuildResult> decorate)
        {
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? "World Reward" : displayName;
            Primitive = primitive;
            BodyName = string.IsNullOrWhiteSpace(bodyName) ? "Visual" : bodyName;
            Placement = placement;
            Material = material;
            Decorate = decorate;
        }

        public string DisplayName { get; }
        public PrimitiveType Primitive { get; }
        public string BodyName { get; }
        public WorldPropPlacementProfile Placement { get; }
        public Material Material { get; }
        public System.Action<WorldRewardPropBuildResult> Decorate { get; }
    }

    private static void BuildTreeGrove(
        Transform parent,
        BlockyCircusGroundsPiece grove,
        int seed,
        IReadOnlyList<Collider> surfaceColliders,
        Material trunkMaterial,
        Material topMaterial)
    {
        for (int i = 0; i < 5; i++)
        {
            int localSeed = TheCircussyOne.Rules.DeterministicSeed.Combine(seed, i, 7103);
            Vector3 offset = new Vector3(
                TheCircussyOne.Rules.DeterministicSeed.ToSignedFloat(localSeed) * grove.Size.x * 0.45f,
                0f,
                TheCircussyOne.Rules.DeterministicSeed.ToSignedFloat(localSeed + 1) * grove.Size.z * 0.45f);
            Vector3 basePosition = grove.Center + offset;
            if (!TrySampleSurface(basePosition, surfaceColliders, out RaycastHit hit))
            {
                continue;
            }

            Quaternion surfaceRotation = WorldPropPlacementRules.SurfaceRotation(hit.normal);

            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Sparse Tree Trunk";
            trunk.transform.SetParent(parent, worldPositionStays: true);
            trunk.transform.SetPositionAndRotation(hit.point + hit.normal * 1.8f, surfaceRotation);
            trunk.transform.localScale = new Vector3(0.7f, 1.8f, 0.7f);
            trunk.GetComponent<MeshRenderer>().sharedMaterial = trunkMaterial;

            GameObject canopy = GameObject.CreatePrimitive(PrimitiveType.Cube);
            canopy.name = "Sparse Tree Canopy";
            canopy.transform.SetParent(parent, worldPositionStays: true);
            canopy.transform.SetPositionAndRotation(hit.point + hit.normal * 4.4f, surfaceRotation);
            canopy.transform.localScale = Vector3.one * 3.2f;
            canopy.GetComponent<MeshRenderer>().sharedMaterial = topMaterial;
        }
    }

    private static bool TrySampleSurface(Vector3 desiredPosition, IReadOnlyList<Collider> surfaceColliders, out RaycastHit bestHit)
    {
        bestHit = default;
        if (surfaceColliders == null || surfaceColliders.Count == 0)
        {
            return false;
        }

        Vector3 origin = new Vector3(desiredPosition.x, desiredPosition.y + SurfaceProbeHeight, desiredPosition.z);
        var ray = new Ray(origin, Vector3.down);
        bool found = false;
        float bestY = float.NegativeInfinity;
        for (int i = 0; i < surfaceColliders.Count; i++)
        {
            Collider collider = surfaceColliders[i];
            if (collider == null || !collider.enabled)
            {
                continue;
            }

            if (!collider.Raycast(ray, out RaycastHit hit, SurfaceProbeDepth))
            {
                continue;
            }

            if (found && hit.point.y <= bestY)
            {
                continue;
            }

            bestHit = hit;
            bestY = hit.point.y;
            found = true;
        }

        return found;
    }

    private static Quaternion OverlayRotationFor(BlockyCircusGroundsPiece piece)
    {
        Quaternion yaw = RotationFor(piece.Direction);
        if (piece.Kind != BlockyCircusGroundsPieceKind.RampAisle)
        {
            return yaw;
        }

        float horizontalLength = Mathf.Max(0.001f, piece.Size.z);
        float pitch = Mathf.Atan2(Mathf.Max(0f, piece.Size.y), horizontalLength) * Mathf.Rad2Deg;
        return yaw * Quaternion.Euler(-pitch, 0f, 0f);
    }

    private static Quaternion RotationFor(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.0001f)
        {
            return Quaternion.identity;
        }

        return Quaternion.LookRotation(new Vector3(direction.x, 0f, direction.y), Vector3.up);
    }

    private static Material CreateMaterial(string name, Color color, float smoothness)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit")
            ?? Shader.Find("Universal Render Pipeline/Unlit")
            ?? Shader.Find("Standard");
        Material material = new Material(shader)
        {
            name = name,
            color = color,
            hideFlags = HideFlags.DontSave
        };
        SetColorIfPresent(material, "_BaseColor", color);
        SetColorIfPresent(material, "_Color", color);
        SetFloatIfPresent(material, "_Smoothness", smoothness);
        SetFloatIfPresent(material, "_SpecularHighlights", 0f);
        return material;
    }

    private static Material ResolveTopMaterial()
    {
        Material floorMaterial = AssetDatabase.LoadAssetAtPath<Material>(TheCircussyOneAssetPaths.FloorMaterialPath);
        if (floorMaterial != null && floorMaterial.shader != null && floorMaterial.shader.isSupported)
        {
            return floorMaterial;
        }

        return CreateMaterial("Sandbox Arena Grid Fallback Top", new Color(0.18f, 0.36f, 0.29f, 1f), 0.22f);
    }

    private static Material CreateTransparentMaterial(string name, Color color, float smoothness)
    {
        Material material = CreateMaterial(name, color, smoothness);
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        SetFloatIfPresent(material, "_Surface", 1f);
        SetFloatIfPresent(material, "_Blend", 0f);
        SetFloatIfPresent(material, "_ZWrite", 0f);
        SetFloatIfPresent(material, "_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        SetFloatIfPresent(material, "_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.EnableKeyword("_ALPHABLEND_ON");
        return material;
    }

    private static void SetColorIfPresent(Material material, string property, Color value)
    {
        if (material != null && material.HasProperty(property))
        {
            material.SetColor(property, value);
        }
    }

    private static void SetFloatIfPresent(Material material, string property, float value)
    {
        if (material != null && material.HasProperty(property))
        {
            material.SetFloat(property, value);
        }
    }
}
