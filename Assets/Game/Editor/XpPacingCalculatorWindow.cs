using System.Collections.Generic;
using System.Linq;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using UnityEditor;
using UnityEngine;

public static class XpPacingCalculatorModel
{
    public static XpPacingProjectionResult BuildFromAssets(float xpGainMultiplier = 1f)
    {
        return Build(
            AssetDatabase.LoadAssetAtPath<GameConfig>(TheCircussyOneAssetPaths.GameConfigPath),
            AssetDatabase.LoadAssetAtPath<EnemyCatalog>(TheCircussyOneAssetPaths.EnemyCatalogPath),
            AssetDatabase.LoadAssetAtPath<XpGemCatalog>(TheCircussyOneAssetPaths.XpGemCatalogPath),
            xpGainMultiplier);
    }

    public static XpPacingProjectionResult BuildForConfig(GameConfig config, float xpGainMultiplier = 1f)
    {
        return Build(
            config,
            AssetDatabase.LoadAssetAtPath<EnemyCatalog>(TheCircussyOneAssetPaths.EnemyCatalogPath),
            AssetDatabase.LoadAssetAtPath<XpGemCatalog>(TheCircussyOneAssetPaths.XpGemCatalogPath),
            xpGainMultiplier);
    }

    public static XpPacingProjectionResult Build(
        GameConfig config,
        EnemyCatalog enemyCatalog,
        XpGemCatalog xpGemCatalog,
        float xpGainMultiplier = 1f)
    {
        return XpPacingProjectionRules.Project(
            config,
            enemyCatalog,
            xpGemCatalog,
            new XpPacingProjectionSettings(
                600f,
                new[] { 60f, 120f, 180f, 300f, 600f },
                new[] { 0.5f, 0.7f, 1f },
                xpGainMultiplier));
    }

    public static string FormatCompactSummary(XpPacingProjectionResult result)
    {
        if (result == null || result.Rows == null || result.Rows.Count == 0)
        {
            return "XP pacing projection unavailable.";
        }

        return string.Join("\n", new[] { 0.5f, 0.7f, 1f }
            .Select(efficiency =>
            {
                XpPacingProjectionRow twoMinutes = FindRow(result, 120f, efficiency);
                XpPacingProjectionRow fiveMinutes = FindRow(result, 300f, efficiency);
                XpPacingProjectionRow tenMinutes = FindRow(result, 600f, efficiency);
                return $"{efficiency * 100f:0}% kills: 2m {FormatLevel(twoMinutes)}, 5m {FormatLevel(fiveMinutes)}, 10m {FormatLevel(tenMinutes)}";
            }));
    }

    public static XpPacingProjectionRow FindRow(XpPacingProjectionResult result, float checkpointSeconds, float killEfficiency)
    {
        if (result == null || result.Rows == null)
        {
            return default;
        }

        for (int i = 0; i < result.Rows.Count; i++)
        {
            XpPacingProjectionRow row = result.Rows[i];
            if (Mathf.Abs(row.CheckpointSeconds - checkpointSeconds) < 0.01f
                && Mathf.Abs(row.KillEfficiency - killEfficiency) < 0.001f)
            {
                return row;
            }
        }

        return default;
    }

    public static string FormatLevel(XpPacingProjectionRow row)
    {
        if (row.NextLevelTarget <= 0)
        {
            return "n/a";
        }

        return $"L{row.ProjectedLevel} ({row.NextLevelProgress01 * 100f:0}% next)";
    }

    public static string FormatTime(float seconds)
    {
        int minutes = Mathf.FloorToInt(Mathf.Max(0f, seconds) / 60f);
        int remainingSeconds = Mathf.RoundToInt(Mathf.Max(0f, seconds) - minutes * 60);
        return $"{minutes}:{remainingSeconds:00}";
    }
}

public static class XpPacingCalculatorEditorGui
{
    public static void DrawCompactGameConfigProjection(GameConfig config)
    {
        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("XP Pacing Calculator", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Projection reads GameConfig, EnemyCatalog, EnemyDefinitions, XpGemCatalog, and XpGemDefinitions. It is an estimate, not a combat simulator.",
            MessageType.Info);

        XpPacingProjectionResult result = XpPacingCalculatorModel.BuildForConfig(config);
        EditorGUILayout.LabelField("Weighted XP / Kill", result.WeightedXpPerKill.ToString("0.##"));
        EditorGUILayout.HelpBox(XpPacingCalculatorModel.FormatCompactSummary(result), MessageType.None);
        DrawWarnings(result.Warnings);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Open XP Pacing Calculator", GUILayout.Height(28f)))
            {
                XpPacingCalculatorWindow.Open();
            }

            if (GUILayout.Button("Ping XP Gem Catalog", GUILayout.Height(28f)))
            {
                Ping(TheCircussyOneAssetPaths.XpGemCatalogPath);
            }
        }
    }

    public static void DrawWarnings(IReadOnlyList<string> warnings)
    {
        if (warnings == null)
        {
            return;
        }

        for (int i = 0; i < warnings.Count; i++)
        {
            EditorGUILayout.HelpBox(warnings[i], MessageType.Warning);
        }
    }

    private static void Ping(string path)
    {
        Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);
        if (asset == null)
        {
            Debug.LogWarning($"Could not ping missing asset: {path}");
            return;
        }

        Selection.activeObject = asset;
        EditorGUIUtility.PingObject(asset);
    }
}

public sealed class XpPacingCalculatorWindow : EditorWindow
{
    private Vector2 scroll;
    private float xpGainMultiplier = 1f;
    private XpPacingProjectionResult result;

    [MenuItem("Tools/The Circussy One/Debug/XP Pacing Calculator")]
    public static void Open()
    {
        var window = GetWindow<XpPacingCalculatorWindow>();
        window.titleContent = new GUIContent("XP Pacing Calculator");
        window.minSize = new Vector2(620f, 520f);
        window.Refresh();
        window.Show();
    }

    private void OnEnable()
    {
        Refresh();
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(6f);
        EditorGUILayout.LabelField("XP Pacing Calculator", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Estimates level cadence from current spawn settings, enemy XP budgets, XP gem definitions, and the level XP curve.",
            MessageType.Info);

        EditorGUI.BeginChangeCheck();
        xpGainMultiplier = EditorGUILayout.Slider("XP Gain Multiplier", xpGainMultiplier, 0.1f, 5f);
        if (EditorGUI.EndChangeCheck())
        {
            Refresh();
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Refresh", GUILayout.Height(28f)))
            {
                Refresh();
            }

            if (GUILayout.Button("Open Game Config", GUILayout.Height(28f)))
            {
                AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<Object>(TheCircussyOneAssetPaths.GameConfigPath));
            }
        }

        if (result == null)
        {
            Refresh();
        }

        scroll = EditorGUILayout.BeginScrollView(scroll);
        DrawResult(result);
        EditorGUILayout.EndScrollView();
    }

    private void Refresh()
    {
        result = XpPacingCalculatorModel.BuildFromAssets(xpGainMultiplier);
        Repaint();
    }

    private static void DrawResult(XpPacingProjectionResult projection)
    {
        if (projection == null)
        {
            EditorGUILayout.HelpBox("No projection available.", MessageType.Warning);
            return;
        }

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("XP Sources", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Weighted XP / Kill", projection.WeightedXpPerKill.ToString("0.##"));
        if (projection.EnemySources.Count == 0)
        {
            EditorGUILayout.HelpBox("No enemy sources found.", MessageType.Warning);
        }

        for (int i = 0; i < projection.EnemySources.Count; i++)
        {
            XpPacingEnemySource source = projection.EnemySources[i];
            EditorGUILayout.LabelField(
                source.DisplayName,
                $"weight {source.SpawnWeight:0.##}, budget {source.XpBudget}, drops {source.ResolvedXpPerKill} XP, {source.DropStyle}");
        }

        XpPacingCalculatorEditorGui.DrawWarnings(projection.Warnings);
        DrawEfficiencyTable(projection, 0.5f);
        DrawEfficiencyTable(projection, 0.7f);
        DrawEfficiencyTable(projection, 1f);
    }

    private static void DrawEfficiencyTable(XpPacingProjectionResult projection, float efficiency)
    {
        EditorGUILayout.Space(10f);
        EditorGUILayout.LabelField($"{efficiency * 100f:0}% Kill / Collect Efficiency", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Time", "Spawns | Kills | XP | Level | Popups");

        List<XpPacingProjectionRow> rows = projection.Rows
            .Where(row => Mathf.Abs(row.KillEfficiency - efficiency) < 0.001f)
            .OrderBy(row => row.CheckpointSeconds)
            .ToList();

        for (int i = 0; i < rows.Count; i++)
        {
            XpPacingProjectionRow row = rows[i];
            EditorGUILayout.LabelField(
                XpPacingCalculatorModel.FormatTime(row.CheckpointSeconds),
                $"{row.SpawnUpperBound} | {row.EstimatedKills} | {row.TotalXp} | {XpPacingCalculatorModel.FormatLevel(row)} | {row.PopupCount}");
        }
    }
}
