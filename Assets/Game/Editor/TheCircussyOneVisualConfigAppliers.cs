using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using TheCircussyOne.Config;
using TheCircussyOne.Visuals;

public static class TheCircussyOneGridVisualApplier
{
    public static TheCircussyOneSceneBuilder.GridVisualAssets ApplyAssets(GameConfig gameConfig, GridVisualConfig gridConfig)
    {
        return TheCircussyOneSceneBuilder.ApplyGridVisualAssets(gameConfig, gridConfig);
    }

    public static void ApplyNow()
    {
        TheCircussyOneSceneBuilder.EnsureGeneratedFolders();
        GameConfig gameConfig = TheCircussyOneSceneBuilder.GetOrCreateGameConfig();
        GridVisualConfig gridConfig = TheCircussyOneSceneBuilder.GetOrCreateGridVisualConfig();
        ApplyAssets(gameConfig, gridConfig);
        AssetDatabase.SaveAssets();
        SceneView.RepaintAll();
        Debug.Log("The Circussy One grid visual config applied.");
    }
}

public static class TheCircussyOneLightingVisualApplier
{
    public static TheCircussyOneSceneBuilder.LightingVisualAssets ApplyAssets(LightingVisualConfig config)
    {
        return TheCircussyOneSceneBuilder.ApplyLightingVisualAssets(config);
    }

    public static void ApplyToOpenScene(LightingVisualConfig config, VolumeProfile volumeProfile)
    {
        TheCircussyOneSceneBuilder.ApplyRenderSettings(config);
        TheCircussyOneSceneBuilder.UpdateSceneLight(config);
        TheCircussyOneSceneBuilder.UpdateSceneCameras(config);
        TheCircussyOneSceneBuilder.UpdateSceneLightingVolume(volumeProfile);
    }

    public static void ApplyToNewScene(LightingVisualConfig config, VolumeProfile volumeProfile)
    {
        TheCircussyOneSceneBuilder.ApplyRenderSettings(config);
        TheCircussyOneSceneBuilder.CreateLight(config);
        TheCircussyOneSceneBuilder.CreateLightingVolume(volumeProfile);
    }

    public static void ApplyNow()
    {
        TheCircussyOneSceneBuilder.EnsureGeneratedFolders();
        LightingVisualConfig config = TheCircussyOneSceneBuilder.GetOrCreateLightingVisualConfig();
        TheCircussyOneSceneBuilder.LightingVisualAssets assets = ApplyAssets(config);
        ApplyToOpenScene(config, assets.volumeProfile);
        AssetDatabase.SaveAssets();
        SceneView.RepaintAll();
        Debug.Log("The Circussy One lighting visual config applied.");
    }
}

public static class TheCircussyOneDamageFeedbackVisualApplier
{
    public static Material ApplyAssets(DamageFeedbackVisualConfig config)
    {
        return TheCircussyOneSceneBuilder.ApplyDamageFeedbackVisualAssets(config);
    }

    public static DamageNumberView ApplyPrefab(DamageFeedbackVisualConfig config)
    {
        Material material = ApplyAssets(config);
        return TheCircussyOneSceneBuilder.CreateDamageNumberPrefab(material, config);
    }

    public static void ApplyNow()
    {
        TheCircussyOneSceneBuilder.EnsureGeneratedFolders();
        DamageFeedbackVisualConfig config = TheCircussyOneSceneBuilder.GetOrCreateDamageFeedbackVisualConfig();
        ApplyPrefab(config);
        AssetDatabase.SaveAssets();
        SceneView.RepaintAll();
        Debug.Log("The Circussy One damage feedback visual config applied.");
    }
}

public static class TheCircussyOneXpGainCounterVisualApplier
{
    public static Material ApplyAssets(XpGainCounterVisualConfig config)
    {
        return TheCircussyOneSceneBuilder.ApplyXpGainCounterVisualAssets(config);
    }

    public static XpGainCounterView ApplyPrefab(XpGainCounterVisualConfig config)
    {
        Material material = ApplyAssets(config);
        return TheCircussyOneSceneBuilder.CreateXpGainCounterPrefab(material, config);
    }

    public static void ApplyNow()
    {
        TheCircussyOneSceneBuilder.EnsureGeneratedFolders();
        XpGainCounterVisualConfig config = TheCircussyOneSceneBuilder.GetOrCreateXpGainCounterVisualConfig();
        ApplyPrefab(config);
        AssetDatabase.SaveAssets();
        SceneView.RepaintAll();
        Debug.Log("The Circussy One XP gain counter visual config applied.");
    }
}

public static class TheCircussyOneWorldInteractionPromptVisualApplier
{
    public static Material ApplyTextMaterial(WorldInteractionPromptVisualConfig config)
    {
        return TheCircussyOneSceneBuilder.ApplyWorldInteractionPromptVisualTextAssets(config);
    }

    public static Material ApplyBarMaterial(WorldInteractionPromptVisualConfig config)
    {
        return TheCircussyOneSceneBuilder.ApplyWorldInteractionPromptVisualBarAssets(config);
    }

    public static WorldInteractionPromptView ApplyPrefab(WorldInteractionPromptVisualConfig config)
    {
        Material textMaterial = ApplyTextMaterial(config);
        Material barMaterial = ApplyBarMaterial(config);
        return TheCircussyOneSceneBuilder.CreateWorldInteractionPromptPrefab(textMaterial, barMaterial, config);
    }

    public static void ApplyNow()
    {
        TheCircussyOneSceneBuilder.EnsureGeneratedFolders();
        WorldInteractionPromptVisualConfig config = TheCircussyOneSceneBuilder.GetOrCreateWorldInteractionPromptVisualConfig();
        ApplyPrefab(config);
        AssetDatabase.SaveAssets();
        SceneView.RepaintAll();
        Debug.Log("The Circussy One world interaction prompt visual config applied.");
    }
}

public static class TheCircussyOneActorMotionVisualApplier
{
    public static void ApplyNow()
    {
        ActorMotionVisualConfig config = TheCircussyOneSceneBuilder.GetOrCreateActorMotionVisualConfig();
        config.EnsureWorkflowDefaults();
        EditorUtility.SetDirty(config);
        AssetDatabase.SaveAssets();
        SceneView.RepaintAll();
        Debug.Log("The Circussy One actor motion visual config applied.");
    }
}

public static class TheCircussyOneEnemySpawnVisualApplier
{
    public static EnemySpawnIndicatorView ApplyPrefab(EnemySpawnVisualConfig config)
    {
        return TheCircussyOneSceneBuilder.CreateEnemySpawnIndicatorPrefab(config);
    }

    public static void ApplyNow()
    {
        TheCircussyOneSceneBuilder.EnsureGeneratedFolders();
        EnemySpawnVisualConfig config = TheCircussyOneSceneBuilder.GetOrCreateEnemySpawnVisualConfig();
        ApplyPrefab(config);
        AssetDatabase.SaveAssets();
        SceneView.RepaintAll();
        Debug.Log("The Circussy One enemy spawn visual config applied.");
    }
}

public static class TheCircussyOneVfxVisualApplier
{
    public static TheCircussyOneSceneBuilder.VfxVisualAssets ApplyAssets(VfxVisualConfig config)
    {
        return TheCircussyOneSceneBuilder.ApplyVfxVisualAssets(config);
    }

    public static void ApplyNow()
    {
        TheCircussyOneSceneBuilder.EnsureGeneratedFolders();
        VfxVisualConfig config = TheCircussyOneSceneBuilder.GetOrCreateVfxVisualConfig();
        ApplyAssets(config);
        ApplyToOpenScene(config);
        AssetDatabase.SaveAssets();
        SceneView.RepaintAll();
        Debug.Log("The Circussy One VFX visual config applied.");
    }

    public static void ApplyToOpenScene(VfxVisualConfig config)
    {
        if (config == null)
        {
            return;
        }

        PlayerView[] players = Object.FindObjectsByType<PlayerView>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < players.Length; i++)
        {
            PlayerView player = players[i];
            if (player == null)
            {
                continue;
            }

            player.ApplyMoveDustVfxConfig(config);
            player.ApplyJumpTrailVfxConfig(config);
            EditorUtility.SetDirty(player);
            if (player.gameObject.scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(player.gameObject.scene);
            }
        }
    }
}

public static class TheCircussyOneHudVisualApplier
{
    public static PanelSettings ApplyPanelSettings(HudVisualConfig config)
    {
        PanelSettings panelSettings = TheCircussyOneSceneBuilder.GetOrCreateHudPanelSettings();
        ApplyPanelSettings(panelSettings, config);
        return panelSettings;
    }

    public static void ApplyPanelSettings(PanelSettings panelSettings, HudVisualConfig config)
    {
        if (panelSettings == null || config == null)
        {
            return;
        }

        config.EnsureReadableDefaults();
        panelSettings.scaleMode = config.scaleMode;
        panelSettings.referenceDpi = Mathf.Max(1f, config.referenceDpi);
        panelSettings.fallbackDpi = Mathf.Max(1f, config.fallbackDpi);
        panelSettings.referenceResolution = new Vector2Int(
            Mathf.RoundToInt(Mathf.Max(100f, config.referenceResolution.x)),
            Mathf.RoundToInt(Mathf.Max(100f, config.referenceResolution.y)));
        panelSettings.targetTexture = null;
        panelSettings.scale = 1f;
        EditorUtility.SetDirty(panelSettings);
    }

    public static void ApplyToOpenScene(HudVisualConfig config)
    {
        HudView[] huds = Object.FindObjectsByType<HudView>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < huds.Length; i++)
        {
            HudView hud = huds[i];
            if (hud == null)
            {
                continue;
            }

            HudVisualConfig configForHud = ResolveConfigForHud(hud, config, true, out bool serializedChanged);
            hud.ApplyConfig(configForHud, false);
            if (serializedChanged)
            {
                EditorUtility.SetDirty(hud);
                if (hud.gameObject.scene.IsValid())
                {
                    EditorSceneManager.MarkSceneDirty(hud.gameObject.scene);
                }
            }
        }
    }

    public static void PreviewOpenScene(HudVisualConfig config)
    {
        HudView[] huds = Object.FindObjectsByType<HudView>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < huds.Length; i++)
        {
            HudView hud = huds[i];
            if (hud == null)
            {
                continue;
            }

            HudVisualConfig configForHud = ResolveConfigForHud(hud, config, false, out _);
            if (configForHud == null)
            {
                continue;
            }

            hud.ApplyConfig(configForHud, false);
            if (configForHud.previewEnabled)
            {
                hud.ApplyPreview(configForHud.GetPreviewSnapshot());
            }
            else
            {
                hud.ClearPreview();
            }
        }
    }

    public static void ClearPreviewOpenScene()
    {
        HudView[] huds = Object.FindObjectsByType<HudView>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < huds.Length; i++)
        {
            HudView hud = huds[i];
            if (hud != null)
            {
                hud.ClearPreview();
            }
        }
    }

    public static void ApplyNow()
    {
        TheCircussyOneSceneBuilder.EnsureGeneratedFolders();
        TheCircussyOneSceneBuilder.GetOrCreateHudPanelSettings();
        HudVisualConfig config = TheCircussyOneSceneBuilder.GetOrCreateHudVisualConfig();
        ApplyPanelSettings(config);
        ApplyToOpenScene(config);
        PreviewOpenScene(config);
        AssetDatabase.SaveAssets();
        SceneView.RepaintAll();
        Debug.Log("The Circussy One HUD visual config applied.");
    }

    private static HudVisualConfig ResolveConfigForHud(HudView hud, HudVisualConfig fallback, bool assignFallback, out bool serializedChanged)
    {
        serializedChanged = false;
        if (hud == null)
        {
            return fallback;
        }

        var serialized = new SerializedObject(hud);
        SerializedProperty property = serialized.FindProperty("hudConfig");
        HudVisualConfig existingConfig = property?.objectReferenceValue as HudVisualConfig;
        if (IsVisualTestOverride(existingConfig))
        {
            return existingConfig;
        }

        if (!assignFallback)
        {
            return existingConfig != null ? existingConfig : fallback;
        }

        if (property != null && property.objectReferenceValue != fallback)
        {
            property.objectReferenceValue = fallback;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            serializedChanged = true;
        }

        return fallback;
    }

    private static bool IsVisualTestOverride(HudVisualConfig config)
    {
        if (config == null)
        {
            return false;
        }

        string path = AssetDatabase.GetAssetPath(config);
        return path.StartsWith("Assets/Game/VisualTests/ConfigOverrides/", System.StringComparison.Ordinal);
    }
}
