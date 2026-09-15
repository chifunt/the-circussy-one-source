using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public sealed class TheCircussyOneAuthoringDoctorWindow : EditorWindow
{
    private Vector2 scroll;
    private List<AuthoringCheckResult> results = new();

    [MenuItem("Tools/The Circussy One/Authoring Doctor")]
    public static void Open()
    {
        var window = GetWindow<TheCircussyOneAuthoringDoctorWindow>();
        window.titleContent = new GUIContent("Authoring Doctor");
        window.minSize = new Vector2(520f, 460f);
        window.Show();
        window.RunChecks();
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("The Circussy One Authoring Doctor", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(
            "Reports project health issues first. Fixes only run when you press an explicit button.",
            EditorStyles.wordWrappedLabel);
        EditorGUILayout.Space(8f);

        if (GUILayout.Button("Run Checks", GUILayout.Height(30f)))
        {
            RunChecks();
        }

        EditorGUILayout.Space(4f);
        DrawSafeFixButtons();
        EditorGUILayout.Space(8f);
        DrawSummary();

        scroll = EditorGUILayout.BeginScrollView(scroll);
        for (int i = 0; i < results.Count; i++)
        {
            DrawResult(results[i]);
        }

        EditorGUILayout.EndScrollView();
    }

    private void RunChecks()
    {
        results = AuthoringDoctorRunner.RunAllChecks();
        Repaint();
    }

    private void DrawSafeFixButtons()
    {
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            EditorGUILayout.LabelField("Safe Explicit Fixes", EditorStyles.boldLabel);
            if (GUILayout.Button("Create Missing Configs"))
            {
                AuthoringDoctorRunner.CreateMissingConfigs();
                RunChecks();
            }

            if (GUILayout.Button("Apply Projected Visuals"))
            {
                AuthoringDoctorRunner.ApplyProjectedVisuals();
                RunChecks();
            }

            if (GUILayout.Button("Generate Visual Test Lab"))
            {
                AuthoringDoctorRunner.GenerateVisualTestLab();
                RunChecks();
            }

            if (GUILayout.Button("Open Visual Almanac"))
            {
                AuthoringDoctorRunner.OpenScenarioBrowser();
                RunChecks();
            }

            if (GUILayout.Button("Generate Authoring Showroom"))
            {
                AuthoringDoctorRunner.GenerateAuthoringShowroom();
                RunChecks();
            }

            if (GUILayout.Button("Open Authoring Showroom"))
            {
                AuthoringDoctorRunner.OpenAuthoringShowroom();
                RunChecks();
            }
        }
    }

    private void DrawSummary()
    {
        int errors = results.Count(result => result.Severity == AuthoringCheckSeverity.Error);
        int warnings = results.Count(result => result.Severity == AuthoringCheckSeverity.Warning);
        int info = results.Count(result => result.Severity == AuthoringCheckSeverity.Info);
        EditorGUILayout.LabelField($"Results: {errors} errors, {warnings} warnings, {info} info", EditorStyles.boldLabel);
    }

    private void DrawResult(AuthoringCheckResult result)
    {
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            EditorGUILayout.LabelField($"{SeverityLabel(result.Severity)}  {result.Category} / {result.Title}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(result.Message, EditorStyles.wordWrappedLabel);

            if (!string.IsNullOrWhiteSpace(result.TargetPath))
            {
                EditorGUILayout.SelectableLabel(result.TargetPath, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (!string.IsNullOrWhiteSpace(result.TargetPath) && GUILayout.Button("Ping/Open Target"))
                {
                    PingOrOpenTarget(result.TargetPath);
                }

                if (result.HasFix && GUILayout.Button(result.FixLabel))
                {
                    result.ExecuteFix();
                    RunChecks();
                }
            }
        }
    }

    private static void PingOrOpenTarget(string path)
    {
        UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
        if (asset != null)
        {
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
            AssetDatabase.OpenAsset(asset);
            return;
        }

        if (File.Exists(path) || Directory.Exists(path))
        {
            EditorUtility.RevealInFinder(path);
        }
    }

    private static string SeverityLabel(AuthoringCheckSeverity severity)
    {
        return severity switch
        {
            AuthoringCheckSeverity.Error => "ERROR",
            AuthoringCheckSeverity.Warning => "WARN",
            _ => "INFO"
        };
    }
}
