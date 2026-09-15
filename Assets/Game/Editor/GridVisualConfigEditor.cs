using Sirenix.OdinInspector.Editor;
using UnityEditor;
using TheCircussyOne.Config;

[CustomEditor(typeof(GridVisualConfig))]
public sealed class GridVisualConfigEditor : OdinEditor
{
    public override void OnInspectorGUI()
    {
        TheCircussyOneConfigEditorUtility.DrawHeader(
            "Grid Visual Config",
            "Procedural placeholder floor and actor grid material generation.",
            ConfigInspectorStyle.VisualColor);
        TheCircussyOneConfigEditorUtility.DrawWorkflowNotice(ConfigWorkflowCatalog.GridVisualConfig);

        EditorGUI.BeginChangeCheck();
        base.OnInspectorGUI();
        bool changed = EditorGUI.EndChangeCheck();
        var config = (GridVisualConfig)target;
        if (changed && config.autoApplyOnChange)
        {
            GridVisualConfigApplyScheduler.Schedule();
        }

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Generated Visual Assets", EditorStyles.boldLabel);
        if (TheCircussyOneConfigEditorUtility.LargeButton("Apply Grid Visuals", ConfigInspectorStyle.VisualColor))
        {
            GridVisualConfigApplyScheduler.ApplyNow();
        }

        TheCircussyOneConfigEditorUtility.DrawVisualTestActions();
    }
}

public sealed class GridVisualConfigPostprocessor : AssetPostprocessor
{
    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (string importedAsset in importedAssets)
        {
            if (importedAsset == TheCircussyOneSceneBuilder.GridVisualConfigPath)
            {
                GridVisualConfig config = AssetDatabase.LoadAssetAtPath<GridVisualConfig>(importedAsset);
                if (config == null || config.autoApplyOnChange)
                {
                    GridVisualConfigApplyScheduler.Schedule();
                }

                return;
            }
        }
    }
}

internal static class GridVisualConfigApplyScheduler
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

        TheCircussyOneGridVisualApplier.ApplyNow();
    }

    private static void ApplyScheduled()
    {
        ApplyNow();
    }
}
