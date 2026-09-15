using System.Collections.Generic;
using System.IO;
using MoreMountains.Feedbacks;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.DI;
using TheCircussyOne.Runtime;
using TheCircussyOne.Visuals;

public static partial class TheCircussyOneSceneBuilder
{
    private enum ArenaRampHighEdge
    {
        North,
        South,
        East,
        West
    }

    private static EnemyView CreateEnemyPrefab(EnemyDefinition normalEnemy, Material material, LightingVisualConfig lightingConfig, Material contactShadowMaterial, Material faceMaterial)
    {
        EnemyBodyProfile bodyProfile = normalEnemy != null && normalEnemy.body != null
            ? normalEnemy.body
            : new EnemyBodyProfile();
        GameObject enemy = new GameObject("Enemy");
        enemy.name = "Enemy";
        enemy.transform.localScale = Vector3.one;

        GameObject bodyScaleRoot = new GameObject("Body Scale Root");
        bodyScaleRoot.transform.SetParent(enemy.transform, false);
        bodyScaleRoot.transform.localPosition = Vector3.zero;
        bodyScaleRoot.transform.localScale = Vector3.one;

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(bodyScaleRoot.transform, false);
        body.transform.localPosition = Vector3.up;
        body.transform.localScale = Vector3.one;
        RemoveCollider(body);
        SetRendererMaterial(body, material);
        AddPlaceholderFace(body, faceMaterial);
        AddContactShadow(enemy.transform, lightingConfig, contactShadowMaterial, lightingConfig.enemyContactShadowScale);
        EnemyHitbox hurtbox = CreateEnemySphereHitbox(enemy.transform, "Enemy Hurtbox", EnemyHitboxRole.Hurtbox, bodyProfile.hurtboxRadius, new Vector3(0f, bodyProfile.hurtboxHeightOffset, 0f));
        EnemyHitbox contactHitbox = CreateEnemyBoxHitbox(enemy.transform, "Enemy Contact Hitbox", EnemyHitboxRole.ContactDamage, bodyProfile.contactHitboxSize, bodyProfile.contactHitboxOffset);
        EnemyHitbox movementBody = CreateEnemyCapsuleHitbox(enemy.transform, "Enemy Movement Body", EnemyHitboxRole.MovementBody, bodyProfile.movementBodyRadius, bodyProfile.movementBodyHeight, bodyProfile.movementBodyOffset);

        var actorBody = enemy.AddComponent<ActorBodyView>();
        var enemyView = enemy.AddComponent<EnemyView>();
        SetSerialized(enemyView, "actorBody", actorBody);
        SetSerialized(enemyView, "hurtbox", hurtbox);
        SetSerialized(enemyView, "contactHitbox", contactHitbox);
        SetSerialized(enemyView, "movementBody", movementBody);

        var healthBar = new GameObject("Health Bar");
        healthBar.transform.SetParent(enemy.transform, false);
        healthBar.AddComponent<HealthBarView>();

        SetLayerRecursive(enemy, GameLayers.RequireLayer(GameLayers.Enemy));
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(enemy, "Assets/Game/Prefabs/Enemy.prefab");
        Object.DestroyImmediate(enemy);
        return prefab.GetComponent<EnemyView>();
    }

    private static T CreateActorPrefab<T>(string path, PrimitiveType primitive, string name, Material material, Vector3 scale, string layerName, bool addTriggerCollider)
        where T : Component
    {
        GameObject go = GameObject.CreatePrimitive(primitive);
        go.name = name;
        go.transform.localScale = scale;
        SetLayerRecursive(go, GameLayers.RequireLayer(layerName));
        if (addTriggerCollider)
        {
            ConfigureProjectileCollider(go);
        }
        else
        {
            RemoveCollider(go);
        }

        SetRendererMaterial(go, material);
        go.AddComponent<T>();
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab.GetComponent<T>();
    }

    private static ProjectileView CreateProjectilePrefab(Material material, WeaponDefinition starterWeapon)
    {
        GameObject projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        projectile.name = "Projectile";
        projectile.transform.localScale = Vector3.one * 0.22f;
        SetLayerRecursive(projectile, GameLayers.RequireLayer(GameLayers.Projectile));
        ConfigureProjectileCollider(projectile);
        SetRendererMaterial(projectile, material);

        var view = projectile.AddComponent<ProjectileView>();
        view.ApplyVisuals(starterWeapon);

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(projectile, "Assets/Game/Prefabs/Projectile.prefab");
        Object.DestroyImmediate(projectile);
        return prefab.GetComponent<ProjectileView>();
    }

    private static OrbitWeaponView CreateFireHoopOrbitPrefab(WeaponDefinition fireHoop)
    {
        GameObject orbit = new GameObject("Fire Hoop Orbit");
        var view = orbit.AddComponent<OrbitWeaponView>();
        view.ConfigureDefaults(fireHoop);
        orbit.SetActive(false);

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(orbit, FireHoopOrbitPrefabPath);
        Object.DestroyImmediate(orbit);
        return prefab.GetComponent<OrbitWeaponView>();
    }

    private static PickupView CreatePickupPrefab(Material material, ParticleSystem attractTrailPrefab)
    {
        Mesh diamondMesh = GetOrCreatePickupDiamondMesh();
        GameObject pickup = new GameObject("Experience Pickup");

        GameObject visualRoot = new GameObject("Visual Root");
        visualRoot.transform.SetParent(pickup.transform, false);
        visualRoot.transform.localScale = Vector3.one * 0.28f;
        visualRoot.transform.localRotation = Quaternion.Euler(0f, 45f, 0f);

        var filter = visualRoot.AddComponent<MeshFilter>();
        filter.sharedMesh = diamondMesh;
        var renderer = visualRoot.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        var view = pickup.AddComponent<PickupView>();
        ParticleSystem attractTrail = AddParticlePrefabChild(visualRoot, attractTrailPrefab, "XP Attract Trail");
        SetSerialized(view, "visualRoot", visualRoot.transform);
        SetSerialized(view, "bodyRenderer", renderer);
        SetSerialized(view, "attractTrail", attractTrail);
        SetLayerRecursive(pickup, GameLayers.RequireLayer(GameLayers.Pickup));

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(pickup, "Assets/Game/Prefabs/ExperiencePickup.prefab");
        Object.DestroyImmediate(pickup);
        return prefab.GetComponent<PickupView>();
    }

    internal static DamageNumberView CreateDamageNumberPrefab(Material material, DamageFeedbackVisualConfig config)
    {
        TMP_FontAsset font = GetDamageNumberFontAsset(config);
        GameObject damageNumber = new GameObject("Damage Number");
        var text = damageNumber.AddComponent<TextMeshPro>();
        text.font = font;
        text.fontSharedMaterial = material;
        text.text = "12";
        text.fontSize = config.fontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.richText = false;
        text.color = config.numberColor;

        var renderer = damageNumber.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = material;
            renderer.sortingOrder = config.sortingOrder;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        }

        damageNumber.AddComponent<DamageNumberView>();
        damageNumber.SetActive(false);

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(damageNumber, DamageNumberPrefabPath);
        Object.DestroyImmediate(damageNumber);
        return prefab.GetComponent<DamageNumberView>();
    }

    internal static XpGainCounterView CreateXpGainCounterPrefab(Material material, XpGainCounterVisualConfig config)
    {
        TMP_FontAsset font = GetXpGainCounterFontAsset(config);
        GameObject counter = new GameObject("XP Gain Counter");
        var text = counter.AddComponent<TextMeshPro>();
        text.font = font;
        text.fontSharedMaterial = material;
        text.text = "+7";
        text.fontSize = config.fontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.richText = false;
        text.color = config.textColor;

        var renderer = counter.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = material;
            renderer.sortingOrder = config.sortingOrder;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        }

        counter.AddComponent<XpGainCounterView>();
        counter.SetActive(false);

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(counter, XpGainCounterPrefabPath);
        Object.DestroyImmediate(counter);
        return prefab.GetComponent<XpGainCounterView>();
    }

    internal static WorldInteractionPromptView CreateWorldInteractionPromptPrefab(Material textMaterial, Material barMaterial, WorldInteractionPromptVisualConfig config)
    {
        TMP_FontAsset font = GetWorldInteractionPromptFontAsset(config);
        GameObject prompt = new GameObject("World Interaction Prompt");
        var view = prompt.AddComponent<WorldInteractionPromptView>();
        view.ConfigureDefaults(config, textMaterial, barMaterial);

        TextMeshPro[] texts = prompt.GetComponentsInChildren<TextMeshPro>(includeInactive: true);
        for (int i = 0; i < texts.Length; i++)
        {
            TextMeshPro text = texts[i];
            text.font = font;
            text.fontSharedMaterial = textMaterial;
            text.fontSize = config.fontSize;
            text.fontStyle = config.boldText ? FontStyles.Bold : FontStyles.Normal;
            text.color = config.labelColor;
            Renderer renderer = text.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = textMaterial;
                renderer.sortingOrder = config.sortingOrder;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                renderer.lightProbeUsage = LightProbeUsage.Off;
                renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            }
        }

        MeshRenderer[] renderers = prompt.GetComponentsInChildren<MeshRenderer>(includeInactive: true);
        for (int i = 0; i < renderers.Length; i++)
        {
            MeshRenderer renderer = renderers[i];
            if (renderer.GetComponent<TextMeshPro>() != null || renderer.gameObject.name == "Glyph Background")
            {
                continue;
            }

            renderer.sharedMaterial = barMaterial;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        }

        prompt.SetActive(false);

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(prompt, WorldInteractionPromptPrefabPath);
        Object.DestroyImmediate(prompt);
        return prefab.GetComponent<WorldInteractionPromptView>();
    }

    internal static EnemySpawnIndicatorView CreateEnemySpawnIndicatorPrefab(EnemySpawnVisualConfig config)
    {
        GameObject indicator = new GameObject("Enemy Spawn Indicator");
        indicator.transform.localScale = Vector3.one;
        var view = indicator.AddComponent<EnemySpawnIndicatorView>();
        view.ConfigureShapeDefaults(config);
        indicator.SetActive(false);

        ConfigureShapeRenderers(indicator);
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(indicator, EnemySpawnIndicatorPrefabPath);
        Object.DestroyImmediate(indicator);
        return prefab.GetComponent<EnemySpawnIndicatorView>();
    }

    private static void ConfigureShapeRenderers(GameObject root)
    {
        foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            renderer.sortingOrder = 10;
        }
    }

    private static Mesh GetOrCreatePickupDiamondMesh()
    {
        var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(PickupDiamondMeshPath);
        if (mesh == null)
        {
            mesh = new Mesh();
            AssetDatabase.CreateAsset(mesh, PickupDiamondMeshPath);
        }

        mesh.name = "PickupDiamondMesh";
        mesh.Clear();
        mesh.vertices = new[]
        {
            new Vector3(0f, 1f, 0f),
            new Vector3(1f, 0f, 0f),
            new Vector3(0f, 0f, 1f),
            new Vector3(-1f, 0f, 0f),
            new Vector3(0f, 0f, -1f),
            new Vector3(0f, -1f, 0f)
        };
        mesh.triangles = new[]
        {
            0, 1, 2,
            0, 2, 3,
            0, 3, 4,
            0, 4, 1,
            5, 2, 1,
            5, 3, 2,
            5, 4, 3,
            5, 1, 4
        };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        EditorUtility.SetDirty(mesh);
        return mesh;
    }

    private static PlayerView CreatePlayer(
        GameConfig config,
        DamageFeedbackVisualConfig damageFeedbackConfig,
        Material material,
        LightingVisualConfig lightingConfig,
        Material contactShadowMaterial,
        ParticleSystem moveDustPrefab,
        ParticleSystem jumpTrailPrefab,
        Material faceMaterial)
    {
        GameObject player = new GameObject("Player");
        player.name = "Player";
        player.transform.position = Vector3.zero;

        GameObject bodyScaleRoot = new GameObject("Body Scale Root");
        bodyScaleRoot.transform.SetParent(player.transform, false);
        bodyScaleRoot.transform.localPosition = Vector3.zero;
        bodyScaleRoot.transform.localScale = Vector3.one;

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(bodyScaleRoot.transform, false);
        body.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
        body.transform.localPosition = Vector3.up * body.transform.localScale.y;
        RemoveCollider(body);
        SetRendererMaterial(body, material);
        AddPlaceholderFace(body, faceMaterial);
        AddContactShadow(player.transform, lightingConfig, contactShadowMaterial, lightingConfig.playerContactShadowScale);

        GameObject healthBar = new GameObject("Health Bar");
        healthBar.transform.SetParent(player.transform, false);
        var healthBarView = healthBar.AddComponent<HealthBarView>();
        if (damageFeedbackConfig != null)
        {
            healthBarView.Configure(
                damageFeedbackConfig.playerWorldHealthBarWidth,
                damageFeedbackConfig.playerWorldHealthBarLocalOffset,
                damageFeedbackConfig.playerWorldHealthBarBackgroundColor,
                damageFeedbackConfig.playerWorldHealthBarFillColor,
                damageFeedbackConfig.playerWorldHealthBarBackgroundThickness,
                damageFeedbackConfig.playerWorldHealthBarFillThickness,
                damageFeedbackConfig.playerWorldHealthBarVisibleThroughPlayer,
                damageFeedbackConfig.playerWorldHealthBarSortingOrderBase,
                damageFeedbackConfig.playerWorldHealthBarRenderQueueBase);
            healthBar.SetActive(damageFeedbackConfig.playerWorldHealthBarEnabled);
        }
        else
        {
            healthBarView.Configure(1.45f, 0.22f);
        }

        var controller = player.AddComponent<CharacterController>();
        controller.height = 2f;
        controller.radius = 0.55f;
        controller.center = Vector3.up;

        var actorBody = player.AddComponent<ActorBodyView>();
        var actor = player.AddComponent<PlayerView>();
        SetSerialized(actor, "actorBody", actorBody);
        ParticleSystem moveDust = AddParticlePrefabChild(player, moveDustPrefab, "Player Move Dust");
        ParticleSystem jumpTrail = AddParticlePrefabChild(player, jumpTrailPrefab, "Player Jump Trail");
        SetSerialized(actor, "moveDust", moveDust);
        SetSerialized(actor, "jumpTrail", jumpTrail);
        SetLayerRecursive(player, GameLayers.RequireLayer(GameLayers.Player));
        actor.ApplyConfig(config, damageFeedbackConfig);
        return actor;
    }

    private static ParticleSystem AddParticlePrefabChild(GameObject parent, ParticleSystem prefab, string childName)
    {
        if (parent == null || prefab == null)
        {
            return null;
        }

        GameObject child = PrefabUtility.InstantiatePrefab(prefab.gameObject) as GameObject;
        if (child == null)
        {
            child = Object.Instantiate(prefab.gameObject);
        }

        child.name = childName;
        child.transform.SetParent(parent.transform, false);
        child.transform.localPosition = prefab.transform.localPosition;
        child.transform.localRotation = prefab.transform.localRotation;
        child.transform.localScale = prefab.transform.localScale;
        child.SetActive(true);
        return child.GetComponent<ParticleSystem>();
    }

    private static CameraView CreateCamera(PlayerView player, CameraConfig config, LightingVisualConfig lightingConfig)
    {
        GameObject cameraObject = new GameObject("Third Person Camera");
        var camera = cameraObject.AddComponent<Camera>();
        cameraObject.AddComponent<AudioListener>();
        camera.fieldOfView = 58f;
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 240f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = lightingConfig.cameraBackgroundColor;
        camera.allowHDR = true;
        camera.tag = "MainCamera";
        var cameraData = cameraObject.AddComponent<UniversalAdditionalCameraData>();
        cameraData.renderPostProcessing = lightingConfig.postProcessingEnabled;
        cameraData.renderShadows = true;
        var cameraView = cameraObject.AddComponent<CameraView>();
        cameraView.ApplyInitialPose(player, config);
        return cameraView;
    }

    private static HudView CreateHud(HudVisualConfig hudConfig)
    {
        GameObject hudObject = new GameObject("HUD");
        var document = hudObject.AddComponent<UIDocument>();
        document.panelSettings = GetOrCreateHudPanelSettings();
        document.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(HudUxmlPath);

        var hud = hudObject.AddComponent<HudView>();
        var upgradeSelection = hudObject.AddComponent<UpgradeSelectionView>();
        var performerSelection = hudObject.AddComponent<PerformerSelectionView>();
        var damageVignette = hudObject.AddComponent<DamageVignetteView>();
        SetSerialized(hud, "document", document);
        SetSerialized(hud, "styleSheet", AssetDatabase.LoadAssetAtPath<StyleSheet>(HudUssPath));
        SetSerialized(hud, "hudConfig", hudConfig);
        SetSerialized(upgradeSelection, "document", document);
        SetSerialized(upgradeSelection, "styleSheet", AssetDatabase.LoadAssetAtPath<StyleSheet>(HudUssPath));
        SetSerialized(performerSelection, "document", document);
        SetSerialized(performerSelection, "styleSheet", AssetDatabase.LoadAssetAtPath<StyleSheet>(HudUssPath));
        damageVignette.Configure(AssetDatabase.LoadAssetAtPath<Material>(PlayerDamageVignetteMaterialPath), Camera.main);
        hud.ApplyConfig(hudConfig);
        return hud;
    }

    private static void CreateFloor(Material material, GameConfig config)
    {
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Arena Floor";
        float planeScale = Mathf.Max(1f, config.arenaRadius / 5f);
        floor.transform.localScale = new Vector3(planeScale, 1f, planeScale);
        SetRendererMaterial(floor, material);

        Transform terrainRoot = new GameObject("Arena Terrain").transform;
        CreateArenaTerrain(terrainRoot, material);
    }

    private static void CreateArenaTerrain(Transform terrainRoot, Material material)
    {
        CreateBox(terrainRoot, "North Terrace", material, center: new Vector3(-14f, 0f, 30f), size: new Vector3(34f, 3.1f, 22f));
        CreateRamp(terrainRoot, "North Terrace South Ramp", material, minX: -25f, maxX: -3f, minZ: 10f, maxZ: 19f, height: 3.1f, ArenaRampHighEdge.North);
        CreateRamp(terrainRoot, "North Terrace East Ramp", material, minX: 3f, maxX: 13f, minZ: 22f, maxZ: 34f, height: 3.1f, ArenaRampHighEdge.West);

        CreateBox(terrainRoot, "East Overlook", material, center: new Vector3(30f, 0f, -4f), size: new Vector3(25f, 2.1f, 28f));
        CreateRamp(terrainRoot, "East Overlook West Ramp", material, minX: 10f, maxX: 19f, minZ: -16f, maxZ: 6f, height: 2.1f, ArenaRampHighEdge.East);

        CreateBox(terrainRoot, "Southwest Low Terrace", material, center: new Vector3(-30f, 0f, -23f), size: new Vector3(34f, 1.45f, 24f));
        CreateRamp(terrainRoot, "Southwest Terrace North Ramp", material, minX: -39f, maxX: -21f, minZ: -8f, maxZ: 0f, height: 1.45f, ArenaRampHighEdge.South);
        CreateRamp(terrainRoot, "Southwest Terrace East Ramp", material, minX: -11f, maxX: -3f, minZ: -31f, maxZ: -15f, height: 1.45f, ArenaRampHighEdge.West);

        CreateRamp(terrainRoot, "Long Southeast Ridge Ramp", material, minX: 6f, maxX: 25f, minZ: -36f, maxZ: -20f, height: 1.65f, ArenaRampHighEdge.North);
        CreateBox(terrainRoot, "Long Southeast Ridge", material, center: new Vector3(15.5f, 0f, -12f), size: new Vector3(19f, 1.65f, 16f));

        CreateRamp(terrainRoot, "Low Center West Berm", material, minX: -25f, maxX: -11f, minZ: 0f, maxZ: 12f, height: 0.85f, ArenaRampHighEdge.East);
        CreateRamp(terrainRoot, "Low Center East Berm", material, minX: 11f, maxX: 26f, minZ: 10f, maxZ: 23f, height: 0.9f, ArenaRampHighEdge.West);
    }

    private static void CreateBox(Transform parent, string name, Material material, Vector3 center, Vector3 size)
    {
        float halfX = size.x * 0.5f;
        float halfZ = size.z * 0.5f;
        float height = Mathf.Max(0.05f, size.y);
        Vector3[] positions =
        {
            new(center.x - halfX, 0f, center.z - halfZ),
            new(center.x + halfX, 0f, center.z - halfZ),
            new(center.x + halfX, 0f, center.z + halfZ),
            new(center.x - halfX, 0f, center.z + halfZ),
            new(center.x - halfX, height, center.z - halfZ),
            new(center.x + halfX, height, center.z - halfZ),
            new(center.x + halfX, height, center.z + halfZ),
            new(center.x - halfX, height, center.z + halfZ)
        };

        CreateTerrainSolid(parent, name, material, positions);
    }

    private static void CreateRamp(Transform parent, string name, Material material, float minX, float maxX, float minZ, float maxZ, float height, ArenaRampHighEdge highEdge)
    {
        height = Mathf.Max(0.05f, height);
        Vector3[] positions =
        {
            new(minX, -0.05f, minZ),
            new(maxX, -0.05f, minZ),
            new(maxX, -0.05f, maxZ),
            new(minX, -0.05f, maxZ),
            new(minX, RampHeight(minX, minZ, minX, maxX, minZ, maxZ, height, highEdge), minZ),
            new(maxX, RampHeight(maxX, minZ, minX, maxX, minZ, maxZ, height, highEdge), minZ),
            new(maxX, RampHeight(maxX, maxZ, minX, maxX, minZ, maxZ, height, highEdge), maxZ),
            new(minX, RampHeight(minX, maxZ, minX, maxX, minZ, maxZ, height, highEdge), maxZ)
        };

        CreateTerrainSolid(parent, name, material, positions);
    }

    private static float RampHeight(float x, float z, float minX, float maxX, float minZ, float maxZ, float height, ArenaRampHighEdge highEdge)
    {
        return highEdge switch
        {
            ArenaRampHighEdge.North => Mathf.Approximately(z, maxZ) ? height : 0f,
            ArenaRampHighEdge.South => Mathf.Approximately(z, minZ) ? height : 0f,
            ArenaRampHighEdge.East => Mathf.Approximately(x, maxX) ? height : 0f,
            ArenaRampHighEdge.West => Mathf.Approximately(x, minX) ? height : 0f,
            _ => 0f
        };
    }

    private static void CreateTerrainSolid(Transform parent, string name, Material material, Vector3[] positions)
    {
        GameObject terrain = new GameObject(name);
        terrain.transform.SetParent(parent, false);

        Mesh mesh = BuildSolidMesh(positions);
        var filter = terrain.AddComponent<MeshFilter>();
        filter.sharedMesh = mesh;
        var renderer = terrain.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        var collider = terrain.AddComponent<MeshCollider>();
        collider.sharedMesh = mesh;
    }

    private static Mesh BuildSolidMesh(Vector3[] positions)
    {
        var vertices = new List<Vector3>(24);
        var triangles = new List<int>(36);
        AddQuad(vertices, triangles, positions[0], positions[1], positions[2], positions[3]);
        AddQuad(vertices, triangles, positions[4], positions[7], positions[6], positions[5]);
        AddQuad(vertices, triangles, positions[0], positions[4], positions[5], positions[1]);
        AddQuad(vertices, triangles, positions[1], positions[5], positions[6], positions[2]);
        AddQuad(vertices, triangles, positions[2], positions[6], positions[7], positions[3]);
        AddQuad(vertices, triangles, positions[3], positions[7], positions[4], positions[0]);

        var mesh = new Mesh { name = "Arena Terrain Piece Mesh" };
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private static void AddQuad(List<Vector3> vertices, List<int> triangles, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        int index = vertices.Count;
        vertices.Add(a);
        vertices.Add(b);
        vertices.Add(c);
        vertices.Add(d);
        triangles.Add(index);
        triangles.Add(index + 1);
        triangles.Add(index + 2);
        triangles.Add(index);
        triangles.Add(index + 2);
        triangles.Add(index + 3);
    }

    private static Transform CreateRoot(string rootName)
    {
        GameObject parent = GameObject.Find("Runtime Roots");
        if (parent == null)
        {
            parent = new GameObject("Runtime Roots");
        }

        var root = new GameObject(rootName).transform;
        root.SetParent(parent.transform);
        return root;
    }

    private static void CreateFeelFeedbacks(out MMF_Player shootFeedback, out MMF_Player hitFeedback, out MMF_Player playerDamageFeedback)
    {
        var root = new GameObject("Feel Feedbacks").transform;
        shootFeedback = CreateFeedback(root, "Shoot Feedback");
        hitFeedback = CreateFeedback(root, "Hit Feedback");
        playerDamageFeedback = CreateFeedback(root, "Player Damage Feedback");
    }

    private static MMF_Player CreateFeedback(Transform root, string feedbackName)
    {
        var feedback = new GameObject(feedbackName).AddComponent<MMF_Player>();
        feedback.transform.SetParent(root);
        feedback.AutoPlayOnStart = false;
        feedback.AutoPlayOnEnable = false;
        feedback.InitializationMode = MMFeedbacks.InitializationModes.Start;
        feedback.FeedbacksList = new System.Collections.Generic.List<MMF_Feedback>();
        return feedback;
    }

    private static Material GetOrCreateFaceMaterial()
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(FaceMaterialPath);
        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        if (material == null)
        {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, FaceMaterialPath);
        }
        else if (material.shader == null && shader != null)
        {
            material.shader = shader;
        }

        Color faceColor = new(0.025f, 0.022f, 0.03f, 1f);
        material.color = faceColor;
        SetColorIfPresent(material, "_BaseColor", faceColor);
        SetColorIfPresent(material, "_EmissionColor", Color.black);
        SetFloatIfPresent(material, "_Metallic", 0f);
        SetFloatIfPresent(material, "_Smoothness", 0.38f);
        SetFloatIfPresent(material, "_EmissionStrength", 0f);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static void SetRendererMaterial(GameObject gameObject, Material material)
    {
        foreach (Renderer renderer in gameObject.GetComponentsInChildren<Renderer>(true))
        {
            renderer.sharedMaterial = material;
        }
    }

    private static void AddPlaceholderFace(GameObject body, Material faceMaterial)
    {
        if (body == null)
        {
            return;
        }

        Transform existingFace = body.transform.Find("Face");
        if (existingFace != null)
        {
            Object.DestroyImmediate(existingFace.gameObject);
        }

        var face = new GameObject("Face");
        face.transform.SetParent(body.transform, false);
        face.transform.localPosition = Vector3.zero;
        face.transform.localRotation = Quaternion.identity;
        face.transform.localScale = Vector3.one;
        face.layer = body.layer;

        CreateFacePrimitive(
            face.transform,
            PrimitiveType.Sphere,
            "Left Eye",
            new Vector3(-0.18f, 0.32f, 0.48f),
            Quaternion.identity,
            Vector3.one * 0.13f,
            faceMaterial,
            body.layer);

        CreateFacePrimitive(
            face.transform,
            PrimitiveType.Sphere,
            "Right Eye",
            new Vector3(0.18f, 0.32f, 0.48f),
            Quaternion.identity,
            Vector3.one * 0.13f,
            faceMaterial,
            body.layer);

        CreateFacePrimitive(
            face.transform,
            PrimitiveType.Capsule,
            "Mouth",
            new Vector3(0f, 0.08f, 0.51f),
            Quaternion.Euler(0f, 0f, 90f),
            new Vector3(0.045f, 0.16f, 0.045f),
            faceMaterial,
            body.layer);
    }

    private static void CreateFacePrimitive(
        Transform parent,
        PrimitiveType primitive,
        string name,
        Vector3 localPosition,
        Quaternion localRotation,
        Vector3 localScale,
        Material material,
        int layer)
    {
        GameObject part = GameObject.CreatePrimitive(primitive);
        part.name = name;
        part.layer = layer;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPosition;
        part.transform.localRotation = localRotation;
        part.transform.localScale = localScale;
        RemoveCollider(part);

        Renderer renderer = part.GetComponent<Renderer>();
        if (renderer == null)
        {
            return;
        }

        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.lightProbeUsage = LightProbeUsage.Off;
        renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
    }

    private static void AddContactShadow(Transform parent, LightingVisualConfig config, Material material, float scaleMultiplier)
    {
        if (!config.contactShadowsEnabled || material == null || scaleMultiplier <= 0f)
        {
            return;
        }

        GameObject shadow = GameObject.CreatePrimitive(PrimitiveType.Quad);
        shadow.name = "Ground Shadow";
        shadow.transform.SetParent(parent, false);
        shadow.transform.localPosition = new Vector3(0f, config.contactShadowYOffset, 0f);
        shadow.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        shadow.transform.localScale = new Vector3(
            Mathf.Max(0.01f, config.contactShadowBaseScale.x * scaleMultiplier),
            Mathf.Max(0.01f, config.contactShadowBaseScale.y * scaleMultiplier),
            1f);

        RemoveCollider(shadow);
        var renderer = shadow.GetComponent<Renderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.lightProbeUsage = LightProbeUsage.Off;
        renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
    }

    private static void RemoveCollider(GameObject gameObject)
    {
        foreach (Collider collider in gameObject.GetComponentsInChildren<Collider>(true))
        {
            Object.DestroyImmediate(collider);
        }
    }

    private static EnemyHitbox CreateEnemySphereHitbox(Transform parent, string name, EnemyHitboxRole role, float radius, Vector3 localPosition)
    {
        var hitboxObject = new GameObject(name);
        hitboxObject.transform.SetParent(parent, false);
        var hitbox = hitboxObject.AddComponent<EnemyHitbox>();
        hitbox.ConfigureSphere(role, radius, localPosition);
        return hitbox;
    }

    private static EnemyHitbox CreateEnemyBoxHitbox(Transform parent, string name, EnemyHitboxRole role, Vector3 size, Vector3 localPosition)
    {
        var hitboxObject = new GameObject(name);
        hitboxObject.transform.SetParent(parent, false);
        var hitbox = hitboxObject.AddComponent<EnemyHitbox>();
        hitbox.ConfigureBox(role, size, localPosition);
        return hitbox;
    }

    private static EnemyHitbox CreateEnemyCapsuleHitbox(Transform parent, string name, EnemyHitboxRole role, float radius, float height, Vector3 localPosition)
    {
        var hitboxObject = new GameObject(name);
        hitboxObject.transform.SetParent(parent, false);
        var hitbox = hitboxObject.AddComponent<EnemyHitbox>();
        hitbox.ConfigureCapsule(role, radius, height, localPosition);
        return hitbox;
    }

    private static void ConfigureProjectileCollider(GameObject projectile)
    {
        var collider = projectile.GetComponent<SphereCollider>();
        if (collider == null)
        {
            collider = projectile.AddComponent<SphereCollider>();
        }

        collider.isTrigger = true;
        collider.center = Vector3.zero;
        collider.radius = 0.5f;
    }

    private static void SetLayerRecursive(GameObject gameObject, int layer)
    {
        gameObject.layer = layer;
        foreach (Transform child in gameObject.transform)
        {
            SetLayerRecursive(child.gameObject, layer);
        }
    }

    private static void EnsureGameplayLayers()
    {
        EnsureLayer(GameLayers.Player);
        EnsureLayer(GameLayers.Enemy);
        EnsureLayer(GameLayers.Projectile);
        EnsureLayer(GameLayers.Pickup);
        Physics.IgnoreLayerCollision(
            GameLayers.RequireLayer(GameLayers.Player),
            GameLayers.RequireLayer(GameLayers.Projectile),
            true);
    }

    private static void EnsureLayer(string layerName)
    {
        Object tagManagerAsset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0];
        var tagManager = new SerializedObject(tagManagerAsset);
        SerializedProperty layers = tagManager.FindProperty("layers");

        for (int i = 0; i < layers.arraySize; i++)
        {
            SerializedProperty layer = layers.GetArrayElementAtIndex(i);
            if (layer.stringValue == layerName)
            {
                return;
            }
        }

        for (int i = 8; i < layers.arraySize; i++)
        {
            SerializedProperty layer = layers.GetArrayElementAtIndex(i);
            if (!string.IsNullOrEmpty(layer.stringValue))
            {
                continue;
            }

            layer.stringValue = layerName;
            tagManager.ApplyModifiedProperties();
            return;
        }

        throw new System.InvalidOperationException($"No free Unity layer slot is available for '{layerName}'.");
    }

    private static void SetSerialized(Object target, string propertyName, Object value)
    {
        System.Type type = target.GetType();
        System.Reflection.FieldInfo field = null;

        while (type != null && field == null)
        {
            field = type.GetField(propertyName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            type = type.BaseType;
        }

        if (field == null)
        {
            throw new System.InvalidOperationException($"Missing field '{propertyName}' on {target.name}.");
        }

        field.SetValue(target, value);
        EditorUtility.SetDirty(target);
    }
}
