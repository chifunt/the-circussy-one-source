using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using TheCircussyOne.Config;
using TheCircussyOne.VisualTests.Editor;

internal static class TheCircussyOneConfigEditorUtility
{
    public static void DrawHeader(string title, string subtitle, Color color)
    {
        EditorGUILayout.Space(4f);
        Rect rect = EditorGUILayout.GetControlRect(false, 44f);
        EditorGUI.DrawRect(rect, new Color(color.r * 0.18f, color.g * 0.18f, color.b * 0.18f, 0.92f));
        var titleRect = new Rect(rect.x + 10f, rect.y + 5f, rect.width - 20f, 18f);
        var subtitleRect = new Rect(rect.x + 10f, rect.y + 23f, rect.width - 20f, 16f);
        EditorGUI.LabelField(titleRect, title, EditorStyles.boldLabel);
        EditorGUI.LabelField(subtitleRect, subtitle, EditorStyles.miniLabel);
        EditorGUILayout.Space(6f);
    }

    public static bool LargeButton(string label, Color color)
    {
        Color previous = GUI.backgroundColor;
        GUI.backgroundColor = color;
        bool clicked = GUILayout.Button(label, GUILayout.Height(30f));
        GUI.backgroundColor = previous;
        return clicked;
    }

    public static void DrawWorkflowNotice(ConfigWorkflowDescriptor descriptor)
    {
        MessageType messageType = descriptor.Kind == ConfigWorkflowKind.StructuralReset
            ? MessageType.Warning
            : MessageType.Info;
        EditorGUILayout.HelpBox($"[{descriptor.Label}] {descriptor.Summary}\n{descriptor.Detail}", messageType);
        EditorGUILayout.Space(4f);
    }

    public static void DrawDangerousResetActions()
    {
        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Generated Scene Reset", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "This rebuilds the canonical generated scene, prefab structure, runtime roots, HUD object, and DI wiring. Use it for structural recovery, not normal tuning.",
            MessageType.Warning);
        if (LargeButton("Reset Generated Prototype Scene", ConfigInspectorStyle.WarningColor))
        {
            TheCircussyOneSceneBuilder.RebuildPrototypeScene();
        }
    }

    public static void DrawVisualTestActions()
    {
        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Visual Test Lab", EditorStyles.boldLabel);
        using (new EditorGUILayout.HorizontalScope())
        {
            if (LargeButton("Generate Visual Test Lab", ConfigInspectorStyle.VisualColor))
            {
                VisualTestLabBuilder.GenerateAllScenes();
            }

            if (LargeButton("Capture Damage Numbers Test", ConfigInspectorStyle.WarningColor))
            {
                VisualTestLabBuilder.RunDamageNumbersTest();
            }
        }
    }
}

[CustomEditor(typeof(GameConfig))]
public sealed class GameConfigEditor : OdinEditor
{
    public override void OnInspectorGUI()
    {
        TheCircussyOneConfigEditorUtility.DrawHeader(
            "Game Config",
            "Runtime balance, arena, combat, and progression tuning.",
            ConfigInspectorStyle.BalanceColor);
        TheCircussyOneConfigEditorUtility.DrawWorkflowNotice(ConfigWorkflowCatalog.GameConfig);
        base.OnInspectorGUI();
        XpPacingCalculatorEditorGui.DrawCompactGameConfigProjection((GameConfig)target);
        TheCircussyOneConfigEditorUtility.DrawVisualTestActions();
    }
}

[CustomEditor(typeof(CameraConfig))]
public sealed class CameraConfigEditor : OdinEditor
{
    public override void OnInspectorGUI()
    {
        TheCircussyOneConfigEditorUtility.DrawHeader(
            "Camera Config",
            "Orbit, look-ahead, obstruction, and sensitivity tuning.",
            ConfigInspectorStyle.CameraColor);
        TheCircussyOneConfigEditorUtility.DrawWorkflowNotice(ConfigWorkflowCatalog.CameraConfig);
        base.OnInspectorGUI();
        TheCircussyOneConfigEditorUtility.DrawVisualTestActions();
    }
}

[CustomEditor(typeof(DamageFeedbackVisualConfig))]
public sealed class DamageFeedbackVisualConfigEditor : OdinEditor
{
    public override void OnInspectorGUI()
    {
        TheCircussyOneConfigEditorUtility.DrawHeader(
            "Damage Feedback Visual Config",
            "Damage numbers, hit flashes, pulses, and pickup/projectile motion.",
            ConfigInspectorStyle.VisualColor);
        TheCircussyOneConfigEditorUtility.DrawWorkflowNotice(ConfigWorkflowCatalog.DamageFeedbackVisualConfig);

        EditorGUI.BeginChangeCheck();
        base.OnInspectorGUI();
        bool changed = EditorGUI.EndChangeCheck();
        var config = (DamageFeedbackVisualConfig)target;
        if (changed && config.autoApplyOnChange)
        {
            DamageFeedbackVisualConfigApplyScheduler.Schedule();
        }

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Generated Visual Assets", EditorStyles.boldLabel);
        if (TheCircussyOneConfigEditorUtility.LargeButton("Apply Damage Feedback", ConfigInspectorStyle.VisualColor))
        {
            DamageFeedbackVisualConfigApplyScheduler.ApplyNow();
        }

        TheCircussyOneConfigEditorUtility.DrawVisualTestActions();
    }
}

[CustomEditor(typeof(ActorMotionVisualConfig))]
public sealed class ActorMotionVisualConfigEditor : OdinEditor
{
    public override void OnInspectorGUI()
    {
        TheCircussyOneConfigEditorUtility.DrawHeader(
            "Actor Motion Visual Config",
            "Body-only idle, movement, and hit squash/stretch tuning.",
            ConfigInspectorStyle.PlayerColor);
        TheCircussyOneConfigEditorUtility.DrawWorkflowNotice(ConfigWorkflowCatalog.ActorMotionVisualConfig);

        EditorGUI.BeginChangeCheck();
        base.OnInspectorGUI();
        bool changed = EditorGUI.EndChangeCheck();
        if (changed)
        {
            var config = (ActorMotionVisualConfig)target;
            config.EnsureWorkflowDefaults();
            EditorUtility.SetDirty(config);
        }

        TheCircussyOneConfigEditorUtility.DrawVisualTestActions();
    }
}

[CustomEditor(typeof(EnemySpawnVisualConfig))]
public sealed class EnemySpawnVisualConfigEditor : OdinEditor
{
    public override void OnInspectorGUI()
    {
        TheCircussyOneConfigEditorUtility.DrawHeader(
            "Enemy Spawn Visual Config",
            "Telegraph rings, pending spawn timing, and enemy emergence motion.",
            ConfigInspectorStyle.WarningColor);
        TheCircussyOneConfigEditorUtility.DrawWorkflowNotice(ConfigWorkflowCatalog.EnemySpawnVisualConfig);

        EditorGUI.BeginChangeCheck();
        base.OnInspectorGUI();
        bool changed = EditorGUI.EndChangeCheck();
        var config = (EnemySpawnVisualConfig)target;
        if (changed && config.autoApplyOnChange)
        {
            EnemySpawnVisualConfigApplyScheduler.Schedule();
        }

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Generated Visual Assets", EditorStyles.boldLabel);
        if (TheCircussyOneConfigEditorUtility.LargeButton("Apply Enemy Spawn Visuals", ConfigInspectorStyle.WarningColor))
        {
            EnemySpawnVisualConfigApplyScheduler.ApplyNow();
        }

        TheCircussyOneConfigEditorUtility.DrawVisualTestActions();
    }
}

[CustomEditor(typeof(VfxVisualConfig))]
public sealed class VfxVisualConfigEditor : OdinEditor
{
    public override void OnInspectorGUI()
    {
        TheCircussyOneConfigEditorUtility.DrawHeader(
            "VFX Visual Config",
            "Built-in ParticleSystem prefabs, shared materials, and visual-only effect tuning.",
            ConfigInspectorStyle.VisualColor);
        TheCircussyOneConfigEditorUtility.DrawWorkflowNotice(ConfigWorkflowCatalog.VfxVisualConfig);

        EditorGUI.BeginChangeCheck();
        base.OnInspectorGUI();
        bool changed = EditorGUI.EndChangeCheck();
        var config = (VfxVisualConfig)target;
        if (changed && config.autoApplyOnChange)
        {
            VfxVisualConfigApplyScheduler.Schedule();
        }

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Generated Visual Assets", EditorStyles.boldLabel);
        if (TheCircussyOneConfigEditorUtility.LargeButton("Apply VFX Visuals", ConfigInspectorStyle.VisualColor))
        {
            VfxVisualConfigApplyScheduler.ApplyNow();
        }

        TheCircussyOneConfigEditorUtility.DrawVisualTestActions();
    }
}

[CustomEditor(typeof(HudVisualConfig))]
public sealed class HudVisualConfigEditor : OdinEditor
{
    public override void OnInspectorGUI()
    {
        TheCircussyOneConfigEditorUtility.DrawHeader(
            "HUD Visual Config",
            "UI Toolkit panel scaling and compact survivorslike HUD layout tuning.",
            ConfigInspectorStyle.VisualColor);
        TheCircussyOneConfigEditorUtility.DrawWorkflowNotice(ConfigWorkflowCatalog.HudVisualConfig);

        EditorGUI.BeginChangeCheck();
        base.OnInspectorGUI();
        bool changed = EditorGUI.EndChangeCheck();
        var config = (HudVisualConfig)target;
        if (changed)
        {
            config.EnsureReadableDefaults();
            EditorUtility.SetDirty(config);
            if (config.autoApplyOnChange)
            {
                HudVisualConfigApplyScheduler.Schedule();
            }
            else if (config.autoPreviewOnChange)
            {
                HudVisualConfigApplyScheduler.SchedulePreview(config);
            }
        }

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("HUD Visual Assets", EditorStyles.boldLabel);
        if (TheCircussyOneConfigEditorUtility.LargeButton("Apply HUD Visuals", ConfigInspectorStyle.VisualColor))
        {
            HudVisualConfigApplyScheduler.ApplyNow();
        }

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Edit Mode Preview", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Preview fills the open-scene HUD with authoring sample values without entering Play Mode. Runtime HUD values still come from HudPresenter.",
            MessageType.Info);
        if (TheCircussyOneConfigEditorUtility.LargeButton("Preview Normal", ConfigInspectorStyle.VisualColor))
        {
            SetPresetAndPreview(config, HudPreviewPreset.Normal);
        }

        if (TheCircussyOneConfigEditorUtility.LargeButton("Preview Damaged", ConfigInspectorStyle.BalanceColor))
        {
            SetPresetAndPreview(config, HudPreviewPreset.Damaged);
        }

        if (TheCircussyOneConfigEditorUtility.LargeButton("Preview Leveling", ConfigInspectorStyle.PlayerColor))
        {
            SetPresetAndPreview(config, HudPreviewPreset.Leveling);
        }

        if (TheCircussyOneConfigEditorUtility.LargeButton("Preview Game Over", ConfigInspectorStyle.WarningColor))
        {
            SetPresetAndPreview(config, HudPreviewPreset.GameOver);
        }

        if (TheCircussyOneConfigEditorUtility.LargeButton("Preview HUD In Open Scene", ConfigInspectorStyle.PlayerColor))
        {
            HudVisualConfigApplyScheduler.PreviewNow(config);
        }

        if (TheCircussyOneConfigEditorUtility.LargeButton("Clear HUD Preview", ConfigInspectorStyle.WarningColor))
        {
            HudVisualConfigApplyScheduler.ClearPreviewNow();
        }

        TheCircussyOneConfigEditorUtility.DrawVisualTestActions();
    }

    private static void SetPresetAndPreview(HudVisualConfig config, HudPreviewPreset preset)
    {
        config.SetPreviewPreset(preset);
        EditorUtility.SetDirty(config);
        HudVisualConfigApplyScheduler.PreviewNow(config);
    }
}

[CustomEditor(typeof(XpGainCounterVisualConfig))]
public sealed class XpGainCounterVisualConfigEditor : OdinEditor
{
    public override void OnInspectorGUI()
    {
        TheCircussyOneConfigEditorUtility.DrawHeader(
            "XP Gain Counter Visual Config",
            "World-space collected-XP aggregation text, timing, scale, and camera-relative offset.",
            ConfigInspectorStyle.VisualColor);
        TheCircussyOneConfigEditorUtility.DrawWorkflowNotice(ConfigWorkflowCatalog.XpGainCounterVisualConfig);

        EditorGUI.BeginChangeCheck();
        base.OnInspectorGUI();
        bool changed = EditorGUI.EndChangeCheck();
        var config = (XpGainCounterVisualConfig)target;
        if (changed && config.autoApplyOnChange)
        {
            XpGainCounterVisualConfigApplyScheduler.Schedule();
        }

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Generated Visual Assets", EditorStyles.boldLabel);
        if (TheCircussyOneConfigEditorUtility.LargeButton("Apply XP Gain Counter", ConfigInspectorStyle.VisualColor))
        {
            XpGainCounterVisualConfigApplyScheduler.ApplyNow();
        }

        TheCircussyOneConfigEditorUtility.DrawVisualTestActions();
    }
}

[CustomEditor(typeof(WorldInteractionPromptVisualConfig))]
public sealed class WorldInteractionPromptVisualConfigEditor : OdinEditor
{
    public override void OnInspectorGUI()
    {
        TheCircussyOneConfigEditorUtility.DrawHeader(
            "World Interaction Prompt Visual Config",
            "World-space interact prompt text, glyph, progress bar, pop timing, and progress shake.",
            ConfigInspectorStyle.VisualColor);
        TheCircussyOneConfigEditorUtility.DrawWorkflowNotice(ConfigWorkflowCatalog.WorldInteractionPromptVisualConfig);

        EditorGUI.BeginChangeCheck();
        base.OnInspectorGUI();
        bool changed = EditorGUI.EndChangeCheck();
        var config = (WorldInteractionPromptVisualConfig)target;
        if (changed && config.autoApplyOnChange)
        {
            WorldInteractionPromptVisualConfigApplyScheduler.Schedule();
        }

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Generated Visual Assets", EditorStyles.boldLabel);
        if (TheCircussyOneConfigEditorUtility.LargeButton("Apply World Interaction Prompt", ConfigInspectorStyle.VisualColor))
        {
            WorldInteractionPromptVisualConfigApplyScheduler.ApplyNow();
        }

        TheCircussyOneConfigEditorUtility.DrawVisualTestActions();
    }
}

public sealed class DamageFeedbackVisualConfigPostprocessor : AssetPostprocessor
{
    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (string importedAsset in importedAssets)
        {
            if (importedAsset == TheCircussyOneSceneBuilder.DamageFeedbackVisualConfigPath)
            {
                DamageFeedbackVisualConfig config = AssetDatabase.LoadAssetAtPath<DamageFeedbackVisualConfig>(importedAsset);
                if (config == null || config.autoApplyOnChange)
                {
                    DamageFeedbackVisualConfigApplyScheduler.Schedule();
                }

                return;
            }
        }
    }
}

public sealed class XpGainCounterVisualConfigPostprocessor : AssetPostprocessor
{
    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (string importedAsset in importedAssets)
        {
            if (importedAsset == TheCircussyOneSceneBuilder.XpGainCounterVisualConfigPath)
            {
                XpGainCounterVisualConfig config = AssetDatabase.LoadAssetAtPath<XpGainCounterVisualConfig>(importedAsset);
                if (config == null || config.autoApplyOnChange)
                {
                    XpGainCounterVisualConfigApplyScheduler.Schedule();
                }

                return;
            }
        }
    }
}

public sealed class WorldInteractionPromptVisualConfigPostprocessor : AssetPostprocessor
{
    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (string importedAsset in importedAssets)
        {
            if (importedAsset == TheCircussyOneSceneBuilder.WorldInteractionPromptVisualConfigPath)
            {
                WorldInteractionPromptVisualConfig config = AssetDatabase.LoadAssetAtPath<WorldInteractionPromptVisualConfig>(importedAsset);
                if (config == null || config.autoApplyOnChange)
                {
                    WorldInteractionPromptVisualConfigApplyScheduler.Schedule();
                }

                return;
            }
        }
    }
}

public sealed class EnemySpawnVisualConfigPostprocessor : AssetPostprocessor
{
    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (string importedAsset in importedAssets)
        {
            if (importedAsset == TheCircussyOneSceneBuilder.EnemySpawnVisualConfigPath)
            {
                EnemySpawnVisualConfig config = AssetDatabase.LoadAssetAtPath<EnemySpawnVisualConfig>(importedAsset);
                if (config == null || config.autoApplyOnChange)
                {
                    EnemySpawnVisualConfigApplyScheduler.Schedule();
                }

                return;
            }
        }
    }
}

public sealed class VfxVisualConfigPostprocessor : AssetPostprocessor
{
    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (string importedAsset in importedAssets)
        {
            if (importedAsset == TheCircussyOneSceneBuilder.VfxVisualConfigPath)
            {
                VfxVisualConfig config = AssetDatabase.LoadAssetAtPath<VfxVisualConfig>(importedAsset);
                if (config == null || config.autoApplyOnChange)
                {
                    VfxVisualConfigApplyScheduler.Schedule();
                }

                return;
            }
        }
    }
}

public sealed class HudVisualConfigPostprocessor : AssetPostprocessor
{
    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (string importedAsset in importedAssets)
        {
            if (importedAsset == TheCircussyOneSceneBuilder.HudVisualConfigPath)
            {
                HudVisualConfig config = AssetDatabase.LoadAssetAtPath<HudVisualConfig>(importedAsset);
                if (config == null || config.autoApplyOnChange)
                {
                    HudVisualConfigApplyScheduler.Schedule();
                }

                return;
            }
        }
    }
}

internal static class DamageFeedbackVisualConfigApplyScheduler
{
    private static bool scheduled;

    public static void Schedule()
    {
        if (scheduled)
        {
            return;
        }

        scheduled = true;
        EditorApplication.delayCall += ApplyScheduled;
    }

    public static void ApplyNow()
    {
        scheduled = false;
        if (EditorApplication.isCompiling)
        {
            Schedule();
            return;
        }

        TheCircussyOneDamageFeedbackVisualApplier.ApplyNow();
    }

    private static void ApplyScheduled()
    {
        ApplyNow();
    }
}

internal static class XpGainCounterVisualConfigApplyScheduler
{
    private static bool scheduled;

    public static void Schedule()
    {
        if (scheduled)
        {
            return;
        }

        scheduled = true;
        EditorApplication.delayCall += ApplyScheduled;
    }

    public static void ApplyNow()
    {
        scheduled = false;
        if (EditorApplication.isCompiling)
        {
            Schedule();
            return;
        }

        TheCircussyOneXpGainCounterVisualApplier.ApplyNow();
    }

    private static void ApplyScheduled()
    {
        ApplyNow();
    }
}

internal static class WorldInteractionPromptVisualConfigApplyScheduler
{
    private static bool scheduled;

    public static void Schedule()
    {
        if (scheduled)
        {
            return;
        }

        scheduled = true;
        EditorApplication.delayCall += ApplyScheduled;
    }

    public static void ApplyNow()
    {
        scheduled = false;
        if (EditorApplication.isCompiling)
        {
            Schedule();
            return;
        }

        TheCircussyOneWorldInteractionPromptVisualApplier.ApplyNow();
    }

    private static void ApplyScheduled()
    {
        ApplyNow();
    }
}

internal static class HudVisualConfigApplyScheduler
{
    private static bool scheduled;
    private static bool previewScheduled;
    private static HudVisualConfig previewConfig;

    public static void Schedule()
    {
        if (scheduled)
        {
            return;
        }

        scheduled = true;
        EditorApplication.delayCall += ApplyScheduled;
    }

    public static void SchedulePreview(HudVisualConfig config)
    {
        previewConfig = config;
        if (previewScheduled)
        {
            return;
        }

        previewScheduled = true;
        EditorApplication.delayCall += PreviewScheduled;
    }

    public static void ApplyNow()
    {
        scheduled = false;
        if (EditorApplication.isCompiling)
        {
            Schedule();
            return;
        }

        TheCircussyOneHudVisualApplier.ApplyNow();
    }

    public static void PreviewNow(HudVisualConfig config)
    {
        previewScheduled = false;
        previewConfig = config;
        if (EditorApplication.isCompiling)
        {
            SchedulePreview(config);
            return;
        }

        if (config == null)
        {
            config = TheCircussyOneSceneBuilder.GetOrCreateHudVisualConfig();
        }

        TheCircussyOneHudVisualApplier.PreviewOpenScene(config);
        SceneView.RepaintAll();
        Debug.Log("The Circussy One HUD preview applied.");
    }

    public static void ClearPreviewNow()
    {
        previewScheduled = false;
        TheCircussyOneHudVisualApplier.ClearPreviewOpenScene();
        SceneView.RepaintAll();
        Debug.Log("The Circussy One HUD preview cleared.");
    }

    private static void ApplyScheduled()
    {
        ApplyNow();
    }

    private static void PreviewScheduled()
    {
        PreviewNow(previewConfig);
    }
}

internal static class EnemySpawnVisualConfigApplyScheduler
{
    private static bool scheduled;

    public static void Schedule()
    {
        if (scheduled)
        {
            return;
        }

        scheduled = true;
        EditorApplication.delayCall += ApplyScheduled;
    }

    public static void ApplyNow()
    {
        scheduled = false;
        if (EditorApplication.isCompiling)
        {
            Schedule();
            return;
        }

        TheCircussyOneEnemySpawnVisualApplier.ApplyNow();
    }

    private static void ApplyScheduled()
    {
        ApplyNow();
    }
}

internal static class VfxVisualConfigApplyScheduler
{
    private static bool scheduled;

    public static void Schedule()
    {
        if (scheduled)
        {
            return;
        }

        scheduled = true;
        EditorApplication.delayCall += ApplyScheduled;
    }

    public static void ApplyNow()
    {
        scheduled = false;
        if (EditorApplication.isCompiling)
        {
            Schedule();
            return;
        }

        TheCircussyOneVfxVisualApplier.ApplyNow();
    }

    private static void ApplyScheduled()
    {
        ApplyNow();
    }
}
