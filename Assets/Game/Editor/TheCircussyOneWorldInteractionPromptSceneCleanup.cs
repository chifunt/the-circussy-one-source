using TheCircussyOne.Visuals;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class TheCircussyOneWorldInteractionPromptSceneCleanup
{
    static TheCircussyOneWorldInteractionPromptSceneCleanup()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    public static int CleanupScenePromptInstancesForTests()
    {
        return CleanupScenePromptInstances();
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            CleanupScenePromptInstances();
        }
    }

    private static int CleanupScenePromptInstances()
    {
        int destroyed = 0;
        WorldInteractionPromptView[] promptViews = Object.FindObjectsByType<WorldInteractionPromptView>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);
        for (int i = 0; i < promptViews.Length; i++)
        {
            WorldInteractionPromptView view = promptViews[i];
            if (view == null || EditorUtility.IsPersistent(view) || !view.gameObject.scene.IsValid())
            {
                continue;
            }

            Object.DestroyImmediate(view.gameObject);
            destroyed++;
        }

        return destroyed;
    }
}
