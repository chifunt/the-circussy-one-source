using Sirenix.OdinInspector.Editor;
using UnityEditor;
using TheCircussyOne.Config;

[CustomEditor(typeof(LightingVisualConfig))]
public sealed class LightingVisualConfigEditor : OdinEditor
{
    public override void OnInspectorGUI()
    {
        TheCircussyOneConfigEditorUtility.DrawHeader(
            "Lighting Visual Config",
            "Scene lighting, post-processing, fog, and stylized contact shadows.",
            ConfigInspectorStyle.LightingColor);
        TheCircussyOneConfigEditorUtility.DrawWorkflowNotice(ConfigWorkflowCatalog.LightingVisualConfig);

        EditorGUI.BeginChangeCheck();
        base.OnInspectorGUI();
        bool changed = EditorGUI.EndChangeCheck();
        var config = (LightingVisualConfig)target;
        if (changed && config.autoApplyOnChange)
        {
            LightingVisualConfigApplyScheduler.Schedule();
        }

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Generated Visual Assets", EditorStyles.boldLabel);
        if (TheCircussyOneConfigEditorUtility.LargeButton("Apply Lighting Visuals", ConfigInspectorStyle.LightingColor))
        {
            LightingVisualConfigApplyScheduler.ApplyNow();
        }

        TheCircussyOneConfigEditorUtility.DrawVisualTestActions();
    }
}

public sealed class LightingVisualConfigPostprocessor : AssetPostprocessor
{
    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (string importedAsset in importedAssets)
        {
            if (importedAsset == TheCircussyOneSceneBuilder.LightingVisualConfigPath)
            {
                LightingVisualConfig config = AssetDatabase.LoadAssetAtPath<LightingVisualConfig>(importedAsset);
                if (config == null || config.autoApplyOnChange)
                {
                    LightingVisualConfigApplyScheduler.Schedule();
                }

                return;
            }
        }
    }
}

internal static class LightingVisualConfigApplyScheduler
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

        TheCircussyOneLightingVisualApplier.ApplyNow();
    }

    private static void ApplyScheduled()
    {
        ApplyNow();
    }
}
