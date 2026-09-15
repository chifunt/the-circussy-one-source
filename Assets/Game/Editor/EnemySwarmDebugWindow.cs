using System.Collections.Generic;
using System.Text;
using TheCircussyOne.Content;
using TheCircussyOne.DI;
using TheCircussyOne.Rules;
using TheCircussyOne.Runtime;
using TheCircussyOne.Visuals;
using UnityEditor;
using UnityEngine;

public sealed class EnemySwarmDebugSnapshot
{
    public EnemySwarmDebugSnapshot(
        string sourceName,
        bool isLive,
        int enemyCount,
        int activeSupportClimbs,
        int maxStackLayer,
        float averageStackedHeight,
        float maxStackedHeight,
        float supportClimbSpeedMultiplier,
        string warning)
    {
        SourceName = string.IsNullOrWhiteSpace(sourceName) ? "Unknown" : sourceName;
        IsLive = isLive;
        EnemyCount = enemyCount;
        ActiveSupportClimbs = activeSupportClimbs;
        MaxStackLayer = maxStackLayer;
        AverageStackedHeight = averageStackedHeight;
        MaxStackedHeight = maxStackedHeight;
        SupportClimbSpeedMultiplier = supportClimbSpeedMultiplier;
        Warning = warning;
    }

    public string SourceName { get; }
    public bool IsLive { get; }
    public int EnemyCount { get; }
    public int ActiveSupportClimbs { get; }
    public int MaxStackLayer { get; }
    public float AverageStackedHeight { get; }
    public float MaxStackedHeight { get; }
    public float SupportClimbSpeedMultiplier { get; }
    public string Warning { get; }
}

public readonly struct EnemySupportClimbGizmoFrame
{
    public EnemySupportClimbGizmoFrame(
        bool hasRuntime,
        bool isSupportClimbing,
        bool isHoldingHorizontal,
        int stackLayer,
        int supportedBySpawnId,
        float currentHeight,
        float targetHeight,
        float holdUntilHeight,
        Vector3 enemyPosition,
        Vector3 supportPosition,
        bool hasSupportPosition)
    {
        HasRuntime = hasRuntime;
        IsSupportClimbing = isSupportClimbing;
        IsHoldingHorizontal = isHoldingHorizontal;
        StackLayer = stackLayer;
        SupportedBySpawnId = supportedBySpawnId;
        CurrentHeight = currentHeight;
        TargetHeight = targetHeight;
        HoldUntilHeight = holdUntilHeight;
        EnemyPosition = enemyPosition;
        SupportPosition = supportPosition;
        HasSupportPosition = hasSupportPosition;
    }

    public bool HasRuntime { get; }
    public bool IsSupportClimbing { get; }
    public bool IsHoldingHorizontal { get; }
    public int StackLayer { get; }
    public int SupportedBySpawnId { get; }
    public float CurrentHeight { get; }
    public float TargetHeight { get; }
    public float HoldUntilHeight { get; }
    public Vector3 EnemyPosition { get; }
    public Vector3 SupportPosition { get; }
    public bool HasSupportPosition { get; }
}

public static class EnemySupportClimbGizmoModel
{
    public static EnemySupportClimbGizmoFrame Build(EnemyRuntime enemy, EnemyRuntime support)
    {
        if (enemy == null || enemy.VerticalMotor == null)
        {
            return new EnemySupportClimbGizmoFrame(
                false,
                false,
                false,
                0,
                -1,
                0f,
                0f,
                0f,
                Vector3.zero,
                Vector3.zero,
                false);
        }

        EnemyVerticalMotorState motor = enemy.VerticalMotor;
        float currentHeight = enemy.Position.y;
        bool isHolding = EnemySupportClimbRules.ShouldHoldHorizontalForSupport(motor, currentHeight);
        Vector3 supportPosition = support != null ? support.Position : Vector3.zero;
        bool hasSupportPosition = support != null;
        return new EnemySupportClimbGizmoFrame(
            true,
            motor.IsSupportClimbing,
            isHolding,
            motor.StackLayer,
            motor.SupportedBySpawnId,
            currentHeight,
            motor.SupportClimbTargetHeight,
            motor.SupportHorizontalHoldUntilHeight,
            enemy.Position,
            supportPosition,
            hasSupportPosition);
    }
}

public static class EnemySwarmDebugModel
{
    public static EnemySwarmDebugSnapshot BuildCurrentSnapshot()
    {
        if (TryFindActiveRegistry(out ActorRegistry registry, out string sourceName))
        {
            return BuildSnapshot(registry.Enemies, sourceName, true);
        }

        EnemyDefinition normalEnemy = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(TheCircussyOneAssetPaths.NormalEnemyPath);
        float multiplier = normalEnemy != null && normalEnemy.stack != null
            ? normalEnemy.stack.supportClimbSpeedMultiplier
            : 0f;
        string warning = EditorApplication.isPlaying
            ? "No active enemy registry found; showing baseline enemy definition settings."
            : "Editor is not in Play Mode; showing baseline enemy definition settings.";
        return new EnemySwarmDebugSnapshot(
            normalEnemy != null ? TheCircussyOneAssetPaths.NormalEnemyPath : "Missing NormalEnemy asset",
            false,
            0,
            0,
            0,
            0f,
            0f,
            multiplier,
            warning);
    }

    public static EnemySwarmDebugSnapshot BuildSnapshot(IReadOnlyList<EnemyRuntime> enemies, string sourceName, bool isLive, string warning = null)
    {
        int enemyCount = 0;
        int activeSupportClimbs = 0;
        int maxStackLayer = 0;
        float stackedHeightTotal = 0f;
        int stackedHeightCount = 0;
        float maxStackedHeight = 0f;
        float supportSpeedTotal = 0f;
        int supportSpeedCount = 0;

        if (enemies != null)
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                EnemyRuntime enemy = enemies[i];
                if (enemy == null || enemy.IsDead || enemy.View == null || !enemy.View.IsActive)
                {
                    continue;
                }

                enemyCount++;
                EnemyVerticalMotorState motor = enemy.VerticalMotor;
                if (motor != null)
                {
                    if (motor.IsSupportClimbing)
                    {
                        activeSupportClimbs++;
                    }

                    maxStackLayer = Mathf.Max(maxStackLayer, motor.StackLayer);
                }

                float height = Mathf.Max(0f, enemy.Position.y);
                if (height > 0.001f)
                {
                    stackedHeightTotal += height;
                    stackedHeightCount++;
                    maxStackedHeight = Mathf.Max(maxStackedHeight, height);
                }

                if (enemy.StackProfile != null)
                {
                    supportSpeedTotal += enemy.StackProfile.supportClimbSpeedMultiplier;
                    supportSpeedCount++;
                }
            }
        }

        float averageStackedHeight = stackedHeightCount > 0 ? stackedHeightTotal / stackedHeightCount : 0f;
        float supportClimbSpeedMultiplier = supportSpeedCount > 0 ? supportSpeedTotal / supportSpeedCount : 0f;
        return new EnemySwarmDebugSnapshot(
            sourceName,
            isLive,
            enemyCount,
            activeSupportClimbs,
            maxStackLayer,
            averageStackedHeight,
            maxStackedHeight,
            supportClimbSpeedMultiplier,
            warning);
    }

    public static bool TryFindActiveRegistry(out ActorRegistry registry, out string sourceName)
    {
        registry = null;
        sourceName = null;
        if (!EditorApplication.isPlaying)
        {
            return false;
        }

        GameLifetimeScope[] scopes = Resources.FindObjectsOfTypeAll<GameLifetimeScope>();
        for (int i = 0; i < scopes.Length; i++)
        {
            GameLifetimeScope scope = scopes[i];
            if (scope == null || EditorUtility.IsPersistent(scope) || scope.ActiveActorRegistry == null)
            {
                continue;
            }

            registry = scope.ActiveActorRegistry;
            sourceName = scope.gameObject.scene.IsValid()
                ? $"{scope.gameObject.scene.name}/{scope.name}"
                : scope.name;
            return true;
        }

        return false;
    }

    public static string ExportPlainText(EnemySwarmDebugSnapshot snapshot)
    {
        var builder = new StringBuilder();
        builder.AppendLine("The Circussy One Enemy Swarm");
        builder.AppendLine($"Source: {snapshot?.SourceName ?? "Unknown"}");
        builder.AppendLine($"Live: {(snapshot != null && snapshot.IsLive ? "Yes" : "No")}");
        if (!string.IsNullOrWhiteSpace(snapshot?.Warning))
        {
            builder.AppendLine($"Warning: {snapshot.Warning}");
        }

        builder.AppendLine($"Enemies: {snapshot?.EnemyCount ?? 0}");
        builder.AppendLine($"Active support climbs: {snapshot?.ActiveSupportClimbs ?? 0}");
        builder.AppendLine($"Max stack layer: {snapshot?.MaxStackLayer ?? 0}");
        builder.AppendLine($"Average stacked height: {FormatHeight(snapshot?.AverageStackedHeight ?? 0f)}");
        builder.AppendLine($"Max stacked height: {FormatHeight(snapshot?.MaxStackedHeight ?? 0f)}");
        builder.AppendLine($"Support climb speed multiplier: {(snapshot?.SupportClimbSpeedMultiplier ?? 0f):0.###}x");
        return builder.ToString();
    }

    public static string FormatHeight(float height)
    {
        return $"{Mathf.Max(0f, height):0.###}u";
    }
}

[InitializeOnLoad]
public static class EnemySupportClimbGizmoDrawer
{
    private static readonly Color SupportLineColor = new(1f, 0.78f, 0.18f, 0.95f);
    private static readonly Color TargetHeightColor = new(0.35f, 0.8f, 1f, 0.95f);
    private static readonly Color HoldColor = new(1f, 0.25f, 0.18f, 0.95f);

    static EnemySupportClimbGizmoDrawer()
    {
        SceneView.duringSceneGui -= DrawSelectedEnemySupportClimbGizmos;
        SceneView.duringSceneGui += DrawSelectedEnemySupportClimbGizmos;
    }

    private static void DrawSelectedEnemySupportClimbGizmos(SceneView sceneView)
    {
        if (!EditorApplication.isPlaying || !EnemySwarmDebugModel.TryFindActiveRegistry(out ActorRegistry registry, out _))
        {
            return;
        }

        GameObject[] selectedObjects = Selection.gameObjects;
        for (int i = 0; i < selectedObjects.Length; i++)
        {
            EnemyView view = selectedObjects[i] != null ? selectedObjects[i].GetComponentInParent<EnemyView>() : null;
            if (view == null || !registry.TryGetEnemy(view, out EnemyRuntime enemy))
            {
                continue;
            }

            EnemyRuntime support = FindSupport(registry.Enemies, enemy.VerticalMotor != null ? enemy.VerticalMotor.SupportedBySpawnId : -1);
            DrawFrame(EnemySupportClimbGizmoModel.Build(enemy, support));
        }
    }

    private static void DrawFrame(EnemySupportClimbGizmoFrame frame)
    {
        if (!frame.HasRuntime)
        {
            return;
        }

        Vector3 labelPosition = frame.EnemyPosition + Vector3.up * 2.2f;
        string label = frame.IsSupportClimbing
            ? $"Stack L{frame.StackLayer} -> {frame.TargetHeight:0.##}u"
            : $"Stack L{frame.StackLayer}";
        if (frame.IsHoldingHorizontal)
        {
            label += " HOLD";
        }

        Handles.Label(labelPosition, label);
        if (!frame.IsSupportClimbing)
        {
            return;
        }

        if (frame.HasSupportPosition)
        {
            Handles.color = SupportLineColor;
            Handles.DrawAAPolyLine(4f, frame.EnemyPosition + Vector3.up * 0.25f, frame.SupportPosition + Vector3.up * 0.25f);
        }

        Vector3 targetCenter = new(frame.EnemyPosition.x, frame.TargetHeight, frame.EnemyPosition.z);
        Handles.color = TargetHeightColor;
        Handles.DrawAAPolyLine(4f, targetCenter + Vector3.left * 0.35f, targetCenter + Vector3.right * 0.35f);
        Handles.DrawAAPolyLine(4f, targetCenter + Vector3.back * 0.35f, targetCenter + Vector3.forward * 0.35f);

        if (frame.IsHoldingHorizontal)
        {
            Vector3 holdCenter = new(frame.EnemyPosition.x, frame.HoldUntilHeight, frame.EnemyPosition.z);
            Handles.color = HoldColor;
            Handles.DrawAAPolyLine(4f, frame.EnemyPosition, holdCenter);
        }
    }

    private static EnemyRuntime FindSupport(IReadOnlyList<EnemyRuntime> enemies, int spawnId)
    {
        if (spawnId < 0 || enemies == null)
        {
            return null;
        }

        for (int i = 0; i < enemies.Count; i++)
        {
            EnemyRuntime enemy = enemies[i];
            if (enemy != null && enemy.SpawnId == spawnId)
            {
                return enemy;
            }
        }

        return null;
    }
}

public sealed class EnemySwarmDebugWindow : EditorWindow
{
    private EnemySwarmDebugSnapshot snapshot;

    [MenuItem("Tools/The Circussy One/Debug/Enemy Swarm")]
    public static void Open()
    {
        var window = GetWindow<EnemySwarmDebugWindow>();
        window.titleContent = new GUIContent("Enemy Swarm");
        window.minSize = new Vector2(360f, 320f);
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

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        if (GUILayout.Button("Refresh", GUILayout.Height(28)))
        {
            Refresh();
        }

        if (GUILayout.Button("Copy Summary", GUILayout.Height(28)))
        {
            EditorGUIUtility.systemCopyBuffer = EnemySwarmDebugModel.ExportPlainText(snapshot);
        }
        EditorGUILayout.EndVertical();

        DrawSnapshot();
    }

    private void Refresh()
    {
        snapshot = EnemySwarmDebugModel.BuildCurrentSnapshot();
        Repaint();
    }

    private void DrawSnapshot()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Enemy Swarm", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Source", snapshot?.SourceName ?? "Unknown");
        EditorGUILayout.LabelField("Mode", snapshot != null && snapshot.IsLive ? "Play Mode Runtime" : "Baseline");
        if (!string.IsNullOrWhiteSpace(snapshot?.Warning))
        {
            EditorGUILayout.HelpBox(snapshot.Warning, MessageType.Info);
        }

        EditorGUILayout.Space(6f);
        EditorGUILayout.LabelField("Enemies", (snapshot?.EnemyCount ?? 0).ToString());
        EditorGUILayout.LabelField("Active Support Climbs", (snapshot?.ActiveSupportClimbs ?? 0).ToString());
        EditorGUILayout.LabelField("Max Stack Layer", (snapshot?.MaxStackLayer ?? 0).ToString());
        EditorGUILayout.LabelField("Average Stacked Height", EnemySwarmDebugModel.FormatHeight(snapshot?.AverageStackedHeight ?? 0f));
        EditorGUILayout.LabelField("Max Stacked Height", EnemySwarmDebugModel.FormatHeight(snapshot?.MaxStackedHeight ?? 0f));
        EditorGUILayout.LabelField("Support Climb Speed", $"{snapshot?.SupportClimbSpeedMultiplier ?? 0f:0.###}x");
        EditorGUILayout.EndVertical();
    }
}
