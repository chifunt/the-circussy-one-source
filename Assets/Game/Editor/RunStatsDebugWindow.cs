using System.Collections.Generic;
using System.Text;
using TheCircussyOne.Config;
using TheCircussyOne.DI;
using TheCircussyOne.Stats;
using UnityEditor;
using UnityEngine;

public sealed class RunStatsDebugSnapshot
{
    public RunStatsDebugSnapshot(IReadOnlyList<StatBreakdown> breakdowns, string sourceName, bool isLive, string warning)
    {
        Breakdowns = breakdowns;
        SourceName = sourceName;
        IsLive = isLive;
        Warning = warning;
    }

    public IReadOnlyList<StatBreakdown> Breakdowns { get; }
    public string SourceName { get; }
    public bool IsLive { get; }
    public string Warning { get; }
}

public static class RunStatsDebugModel
{
    public const string DebugSourceId = "debug.run_stats_window";

    public static RunStatsDebugSnapshot BuildSnapshot(RunStats stats, string sourceName, bool isLive, string warning = null)
    {
        return new RunStatsDebugSnapshot(
            stats != null ? stats.GetAllBreakdowns() : System.Array.Empty<StatBreakdown>(),
            string.IsNullOrWhiteSpace(sourceName) ? "Unknown" : sourceName,
            isLive,
            warning);
    }

    public static RunStatsDebugSnapshot BuildBaselineSnapshot()
    {
        GameConfig config = AssetDatabase.LoadAssetAtPath<GameConfig>(TheCircussyOneAssetPaths.GameConfigPath);
        bool createdTemporaryConfig = config == null;
        if (createdTemporaryConfig)
        {
            config = ScriptableObject.CreateInstance<GameConfig>();
        }

        var stats = new RunStats(config);
        RunStatsDebugSnapshot snapshot = BuildSnapshot(
            stats,
            createdTemporaryConfig ? "Temporary GameConfig defaults" : TheCircussyOneAssetPaths.GameConfigPath,
            false,
            createdTemporaryConfig ? "Canonical GameConfig asset was missing; showing temporary defaults." : null);

        if (createdTemporaryConfig)
        {
            Object.DestroyImmediate(config);
        }

        return snapshot;
    }

    public static RunStatsDebugSnapshot BuildCurrentSnapshot()
    {
        if (TryFindActiveRunStats(out RunStats stats, out string sourceName))
        {
            return BuildSnapshot(stats, sourceName, true);
        }

        RunStatsDebugSnapshot baseline = BuildBaselineSnapshot();
        string warning = EditorApplication.isPlaying
            ? "No active Play Mode RunStats found; showing baseline GameConfig stats."
            : "Editor is not in Play Mode; showing baseline GameConfig stats.";
        return new RunStatsDebugSnapshot(baseline.Breakdowns, baseline.SourceName, false, warning);
    }

    public static bool TryFindActiveRunStats(out RunStats stats, out string sourceName)
    {
        stats = null;
        sourceName = null;
        if (!EditorApplication.isPlaying)
        {
            return false;
        }

        GameLifetimeScope[] scopes = Resources.FindObjectsOfTypeAll<GameLifetimeScope>();
        for (int i = 0; i < scopes.Length; i++)
        {
            GameLifetimeScope scope = scopes[i];
            if (scope == null || EditorUtility.IsPersistent(scope) || scope.ActiveRunStats == null)
            {
                continue;
            }

            stats = scope.ActiveRunStats;
            sourceName = scope.gameObject.scene.IsValid()
                ? $"{scope.gameObject.scene.name}/{scope.name}"
                : scope.name;
            return true;
        }

        return false;
    }

    public static bool AddTemporaryDebugModifiers(RunStats stats)
    {
        if (!EditorApplication.isPlaying || stats == null)
        {
            return false;
        }

        stats.RemoveSource(DebugSourceId);
        stats.AddModifier(StatModifier.Flat(StatId.PlayerArmor, 25f, DebugSourceId));
        stats.AddModifier(StatModifier.AdditivePercent(StatId.PlayerMoveSpeedMultiplier, 0.15f, DebugSourceId));
        stats.AddModifier(StatModifier.AdditivePercent(StatId.XpGainMultiplier, 0.5f, DebugSourceId));
        stats.AddModifier(StatModifier.AdditivePercent(StatId.GlobalDamageMultiplier, 0.25f, DebugSourceId));
        stats.AddModifier(StatModifier.AttackSpeedBonus(StatId.GlobalWeaponHaste, 0.25f, DebugSourceId));
        stats.AddModifier(StatModifier.Flat(StatId.ProjectileCount, 1f, DebugSourceId));
        return true;
    }

    public static void ClearTemporaryDebugModifiers(RunStats stats)
    {
        stats?.RemoveSource(DebugSourceId);
    }

    public static string ExportPlainText(RunStatsDebugSnapshot snapshot)
    {
        var builder = new StringBuilder();
        builder.AppendLine("The Circussy One Run Stats");
        builder.AppendLine($"Source: {snapshot?.SourceName ?? "Unknown"}");
        builder.AppendLine($"Live: {(snapshot != null && snapshot.IsLive ? "Yes" : "No")}");
        if (!string.IsNullOrWhiteSpace(snapshot?.Warning))
        {
            builder.AppendLine($"Warning: {snapshot.Warning}");
        }

        if (snapshot?.Breakdowns == null)
        {
            return builder.ToString();
        }

        string currentCategory = null;
        for (int i = 0; i < snapshot.Breakdowns.Count; i++)
        {
            StatBreakdown breakdown = snapshot.Breakdowns[i];
            if (currentCategory != breakdown.Category)
            {
                currentCategory = breakdown.Category;
                builder.AppendLine();
                builder.AppendLine($"[{currentCategory}]");
            }

            builder.AppendLine($"{breakdown.DisplayName}: {FormatValue(breakdown)} (base {FormatValue(breakdown.Definition, breakdown.BaseValue)})");
            for (int j = 0; j < breakdown.Modifiers.Count; j++)
            {
                StatModifierSnapshot modifier = breakdown.Modifiers[j];
                builder.AppendLine($"  - {modifier.SourceId}: {modifier.Bucket} {StatDisplayRules.FormatModifier(breakdown.Definition, modifier.Bucket, modifier.Value)}");
            }
        }

        return builder.ToString();
    }

    public static string FormatValue(float value, StatValueKind valueKind)
    {
        return valueKind == StatValueKind.Integer
            ? StatRules.FloorToAppliedInt(value).ToString()
            : value.ToString("0.###");
    }

    public static string FormatValue(StatBreakdown breakdown)
    {
        return FormatValue(breakdown.Definition, breakdown.FinalValue);
    }

    public static string FormatValue(StatDefinition definition, float value)
    {
        return StatDisplayRules.FormatValue(definition, value);
    }
}

public sealed class RunStatsDebugWindow : EditorWindow
{
    private Vector2 scroll;
    private RunStatsDebugSnapshot snapshot;

    [MenuItem("Tools/The Circussy One/Debug/Run Stats")]
    public static void Open()
    {
        var window = GetWindow<RunStatsDebugWindow>();
        window.titleContent = new GUIContent("Run Stats");
        window.minSize = new Vector2(380f, 520f);
        window.Refresh();
        window.Show();
    }

    private void OnEnable()
    {
        Refresh();
    }

    private void OnGUI()
    {
        if (snapshot == null)
        {
            Refresh();
        }

        DrawToolbar();
        DrawSnapshotHeader();
        scroll = EditorGUILayout.BeginScrollView(scroll);
        DrawBreakdowns();
        EditorGUILayout.EndScrollView();
    }

    private void Refresh()
    {
        snapshot = RunStatsDebugModel.BuildCurrentSnapshot();
        Repaint();
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        if (GUILayout.Button("Refresh Snapshot", GUILayout.Height(28)))
        {
            Refresh();
        }

        if (GUILayout.Button("Copy Breakdown As Text", GUILayout.Height(28)))
        {
            EditorGUIUtility.systemCopyBuffer = RunStatsDebugModel.ExportPlainText(snapshot);
        }

        EditorGUI.BeginDisabledGroup(!EditorApplication.isPlaying);
        if (GUILayout.Button("Add Temporary Play Mode Debug Modifiers", GUILayout.Height(28)))
        {
            if (RunStatsDebugModel.TryFindActiveRunStats(out RunStats stats, out _))
            {
                RunStatsDebugModel.AddTemporaryDebugModifiers(stats);
                Refresh();
            }
        }

        if (GUILayout.Button("Clear Temporary Debug Modifiers", GUILayout.Height(28)))
        {
            if (RunStatsDebugModel.TryFindActiveRunStats(out RunStats stats, out _))
            {
                RunStatsDebugModel.ClearTemporaryDebugModifiers(stats);
                Refresh();
            }
        }
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndVertical();
    }

    private void DrawSnapshotHeader()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Run Stats", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Source", snapshot?.SourceName ?? "Unknown");
        EditorGUILayout.LabelField("Mode", snapshot != null && snapshot.IsLive ? "Play Mode Runtime" : "Baseline");
        if (!string.IsNullOrWhiteSpace(snapshot?.Warning))
        {
            EditorGUILayout.HelpBox(snapshot.Warning, MessageType.Info);
        }
        EditorGUILayout.EndVertical();
    }

    private void DrawBreakdowns()
    {
        if (snapshot?.Breakdowns == null)
        {
            return;
        }

        string currentCategory = null;
        for (int i = 0; i < snapshot.Breakdowns.Count; i++)
        {
            StatBreakdown breakdown = snapshot.Breakdowns[i];
            if (currentCategory != breakdown.Category)
            {
                currentCategory = breakdown.Category;
                EditorGUILayout.Space(8f);
                EditorGUILayout.LabelField(currentCategory, EditorStyles.boldLabel);
            }

            DrawStatCard(breakdown);
        }
    }

    private static void DrawStatCard(StatBreakdown breakdown)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField(breakdown.DisplayName, EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Final", RunStatsDebugModel.FormatValue(breakdown));
        EditorGUILayout.LabelField("Base", RunStatsDebugModel.FormatValue(breakdown.Definition, breakdown.BaseValue));
        EditorGUILayout.LabelField("Policy", breakdown.StackingPolicy.ToString());
        EditorGUILayout.LabelField("Clamp", $"{FormatClamp(breakdown.MinValue)} to {FormatClamp(breakdown.MaxValue)}");

        if (breakdown.Modifiers.Count == 0)
        {
            EditorGUILayout.LabelField("Modifiers", "None");
        }
        else
        {
            EditorGUILayout.LabelField("Modifiers", EditorStyles.boldLabel);
            for (int i = 0; i < breakdown.Modifiers.Count; i++)
            {
                StatModifierSnapshot modifier = breakdown.Modifiers[i];
                EditorGUILayout.LabelField(modifier.SourceId, $"{modifier.Bucket} {StatDisplayRules.FormatModifier(breakdown.Definition, modifier.Bucket, modifier.Value)}");
            }
        }

        EditorGUILayout.EndVertical();
    }

    private static string FormatClamp(float value)
    {
        if (float.IsNegativeInfinity(value))
        {
            return "-inf";
        }

        if (float.IsPositiveInfinity(value))
        {
            return "+inf";
        }

        return value.ToString("0.###");
    }
}
