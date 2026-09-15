using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using TheCircussyOne.Content;
using UnityEditor;
using UnityEngine;

public static class ContentWorkbenchEnemyInspector
{
    private static readonly string[] TabLabels =
    {
        "Identity",
        "Spawning",
        "Stats",
        "Body",
        "Climb",
        "Stack",
        "Drops",
        "Visual",
        "Diagnostics"
    };

    private static ContentWorkbenchEnemyTab selectedTab = ContentWorkbenchEnemyTab.Identity;

    public static bool CanDraw(Object asset)
    {
        return asset is EnemyDefinition;
    }

    public static void Draw(EnemyDefinition enemy)
    {
        if (enemy == null)
        {
            EditorGUILayout.HelpBox("No enemy selected.", MessageType.Info);
            return;
        }

        var serialized = new SerializedObject(enemy);
        serialized.Update();

        DrawHeader(enemy);
        DrawTabs();
        switch (selectedTab)
        {
            case ContentWorkbenchEnemyTab.Identity:
                DrawIdentity(serialized, enemy);
                break;
            case ContentWorkbenchEnemyTab.Spawning:
                DrawSpawning(serialized);
                break;
            case ContentWorkbenchEnemyTab.Stats:
                DrawStats(serialized);
                break;
            case ContentWorkbenchEnemyTab.Body:
                DrawBody(serialized);
                break;
            case ContentWorkbenchEnemyTab.Climb:
                DrawClimb(serialized, enemy);
                break;
            case ContentWorkbenchEnemyTab.Stack:
                DrawStack(serialized, enemy);
                break;
            case ContentWorkbenchEnemyTab.Drops:
                DrawDrops(serialized);
                break;
            case ContentWorkbenchEnemyTab.Visual:
                DrawVisual(serialized, enemy);
                break;
            case ContentWorkbenchEnemyTab.Diagnostics:
                DrawDiagnostics(enemy);
                break;
        }

        if (serialized.ApplyModifiedProperties())
        {
            EditorUtility.SetDirty(enemy);
        }
    }

    public static int TabCount => TabLabels.Length;

    public static string TabLabel(int index)
    {
        return TabLabels[Mathf.Clamp(index, 0, TabLabels.Length - 1)];
    }

    private static void DrawHeader(EnemyDefinition enemy)
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField("Enemy Authoring", EditorStyles.boldLabel);
            if (GUILayout.Button("Open Odin Inspector", GUILayout.Width(150f), GUILayout.Height(22f)))
            {
                OdinEditorWindow.InspectObject(enemy);
            }
        }

        EditorGUILayout.LabelField("Workbench uses focused tabs here so compact unit fields stay readable. Use Odin Inspector for the full advanced asset view.", EditorStyles.wordWrappedMiniLabel);
    }

    private static void DrawTabs()
    {
        EditorGUILayout.Space(4f);
        selectedTab = (ContentWorkbenchEnemyTab)GUILayout.Toolbar((int)selectedTab, TabLabels, GUILayout.MinHeight(24f));
        EditorGUILayout.Space(4f);
    }

    private static void DrawIdentity(SerializedObject serialized, EnemyDefinition enemy)
    {
        ContentWorkbenchFieldDrawer.DrawSection("Identity");
        ContentWorkbenchFieldDrawer.DrawReadOnlyProperty(serialized, nameof(EnemyDefinition.enemyId), "Enemy ID");
        ContentWorkbenchFieldDrawer.DrawProperty(serialized, nameof(EnemyDefinition.displayName), "Display Name");
        ContentWorkbenchFieldDrawer.DrawIdNameSync(
            serialized.FindProperty(nameof(EnemyDefinition.enemyId)),
            serialized.FindProperty(nameof(EnemyDefinition.displayName))?.stringValue,
            "enemy",
            "Enemy ID");
        ContentWorkbenchFieldDrawer.DrawProperty(serialized, nameof(EnemyDefinition.rarity), "Rarity");
        EditorGUILayout.LabelField("Computed Tags", ComputedContentTagRules.Format(enemy.Tags), EditorStyles.miniLabel);
    }

    private static void DrawSpawning(SerializedObject serialized)
    {
        ContentWorkbenchFieldDrawer.DrawSection("Spawning");
        ContentWorkbenchFieldDrawer.DrawProperty(serialized, nameof(EnemyDefinition.behaviorType), "Behavior");
        ContentWorkbenchFieldDrawer.DrawProperty(serialized, nameof(EnemyDefinition.spawnWeight), "Spawn Weight");
    }

    private static void DrawStats(SerializedObject serialized)
    {
        ContentWorkbenchFieldDrawer.DrawSection("Combat");
        ContentWorkbenchFieldDrawer.DrawProperty(serialized, nameof(EnemyDefinition.baseHealth), "Base Health");
        ContentWorkbenchFieldDrawer.DrawUnitProperty(serialized, nameof(EnemyDefinition.healthGrowthPerMinute), "Health Growth", "/min");
        ContentWorkbenchFieldDrawer.DrawProperty(serialized, nameof(EnemyDefinition.contactDamage), "Contact Damage");

        ContentWorkbenchFieldDrawer.DrawSection("Movement");
        ContentWorkbenchFieldDrawer.DrawUnitProperty(serialized, nameof(EnemyDefinition.baseMoveSpeed), "Base Move Speed", "u/s");
        ContentWorkbenchFieldDrawer.DrawUnitProperty(serialized, nameof(EnemyDefinition.moveSpeedGrowthPerMinute), "Move Speed Growth", "/min");
        ContentWorkbenchFieldDrawer.DrawUnitProperty(serialized, nameof(EnemyDefinition.turnDegreesPerSecond), "Turn Rate", "deg/sec");
    }

    private static void DrawBody(SerializedObject serialized)
    {
        SerializedProperty body = serialized.FindProperty(nameof(EnemyDefinition.body));
        if (body == null)
        {
            EditorGUILayout.HelpBox("Missing body profile.", MessageType.Warning);
            return;
        }

        ContentWorkbenchFieldDrawer.DrawSection("Hurtbox");
        DrawNestedUnitProperty(body, nameof(EnemyBodyProfile.hurtboxRadius), "Radius", "u");
        DrawNestedUnitProperty(body, nameof(EnemyBodyProfile.hurtboxHeightOffset), "Height Offset", "u");

        ContentWorkbenchFieldDrawer.DrawSection("Contact Damage");
        DrawNestedUnitProperty(body, nameof(EnemyBodyProfile.contactHitboxSize), "Box Size", "u");
        DrawNestedUnitProperty(body, nameof(EnemyBodyProfile.contactHitboxOffset), "Box Offset", "u");

        ContentWorkbenchFieldDrawer.DrawSection("Movement Body");
        DrawNestedUnitProperty(body, nameof(EnemyBodyProfile.movementBodyRadius), "Radius", "u");
        DrawNestedUnitProperty(body, nameof(EnemyBodyProfile.movementBodyHeight), "Height", "u");
        DrawNestedUnitProperty(body, nameof(EnemyBodyProfile.movementBodyOffset), "Offset", "u");
    }

    private static void DrawClimb(SerializedObject serialized, EnemyDefinition enemy)
    {
        SerializedProperty climb = serialized.FindProperty(nameof(EnemyDefinition.climb));
        if (climb == null)
        {
            EditorGUILayout.HelpBox("Missing climb profile.", MessageType.Warning);
            return;
        }

        ContentWorkbenchFieldDrawer.DrawSection("Environment");
        DrawNestedProperty(climb, nameof(EnemyClimbProfile.canClimbEnvironment), "Can Climb Environment");
        bool environmentActive = NestedBool(climb, nameof(EnemyClimbProfile.canClimbEnvironment));
        if (environmentActive)
        {
            DrawNestedUnitProperty(climb, nameof(EnemyClimbProfile.maxEnvironmentClimbHeight), "Max Climb Height", "u");
            DrawNestedUnitProperty(climb, nameof(EnemyClimbProfile.environmentProbeDistance), "Probe Distance", "u");
            DrawNestedUnitProperty(climb, nameof(EnemyClimbProfile.environmentProbeIntervalFrames), "Probe Interval", "frames");
        }
        else
        {
            EditorGUILayout.HelpBox(EnemyAuthoringApplicability.EnvironmentClimbingDisabledReason(enemy.climb), MessageType.Info);
        }

        ContentWorkbenchFieldDrawer.DrawSection("Motion");
        DrawNestedUnitProperty(climb, nameof(EnemyClimbProfile.gravity), "Gravity", "u/s^2");
        DrawNestedUnitProperty(climb, nameof(EnemyClimbProfile.terminalFallSpeed), "Terminal Fall Speed", "u/s");
        if (!environmentActive)
        {
            return;
        }

        DrawNestedProperty(climb, nameof(EnemyClimbProfile.useMoveSpeedForClimb), "Use Move Speed For Climb");
        bool usesMoveSpeed = NestedBool(climb, nameof(EnemyClimbProfile.useMoveSpeedForClimb));
        DrawNestedUnitProperty(climb, nameof(EnemyClimbProfile.climbSpeedMultiplier), "Climb Speed Multiplier", "x");
        if (!usesMoveSpeed)
        {
            DrawNestedUnitProperty(climb, nameof(EnemyClimbProfile.fallbackClimbSpeed), "Fallback Climb Speed", "u/s");
        }
        else
        {
            EditorGUILayout.LabelField(EnemyAuthoringApplicability.FallbackClimbSpeedDisabledReason(enemy.climb), EditorStyles.wordWrappedMiniLabel);
        }
    }

    private static void DrawStack(SerializedObject serialized, EnemyDefinition enemy)
    {
        SerializedProperty stack = serialized.FindProperty(nameof(EnemyDefinition.stack));
        if (stack == null)
        {
            EditorGUILayout.HelpBox("Missing stack profile.", MessageType.Warning);
            return;
        }

        ContentWorkbenchFieldDrawer.DrawSection("Policy");
        DrawNestedProperty(stack, nameof(EnemyStackProfile.policy), "Stack Policy");
        bool supportActive = NestedEnum<EnemyStackPolicy>(stack, nameof(EnemyStackProfile.policy)) == EnemyStackPolicy.SupportBased;
        if (!supportActive)
        {
            EditorGUILayout.HelpBox(EnemyAuthoringApplicability.SupportStackingDisabledReason(enemy.stack), MessageType.Info);
            return;
        }

        DrawNestedProperty(stack, nameof(EnemyStackProfile.canClimbEnemies), "Can Climb Enemies");
        DrawNestedProperty(stack, nameof(EnemyStackProfile.canBeStackedOn), "Can Be Stacked On");

        ContentWorkbenchFieldDrawer.DrawSection("Support Stacking");
        DrawNestedUnitProperty(stack, nameof(EnemyStackProfile.pileRadius), "Pile Radius", "u");
        DrawNestedProperty(stack, nameof(EnemyStackProfile.pileStartCount), "Pile Start Count");
        DrawNestedProperty(stack, nameof(EnemyStackProfile.pileEnemiesPerLayer), "Enemies Per Layer");
        DrawNestedProperty(stack, nameof(EnemyStackProfile.maxStackLayers), "Max Stack Layers");
        DrawNestedUnitProperty(stack, nameof(EnemyStackProfile.layerHeightMultiplier), "Layer Height", "x body height");
        DrawNestedUnitProperty(stack, nameof(EnemyStackProfile.supportClimbSpeedMultiplier), "Support Climb Speed", "x climb speed");
        DrawNestedUnitProperty(stack, nameof(EnemyStackProfile.supportClimbSeparationMultiplier), "Support Separation", "x separation");
    }

    private static void DrawDrops(SerializedObject serialized)
    {
        ContentWorkbenchFieldDrawer.DrawSection("XP Drops");
        ContentWorkbenchFieldDrawer.DrawUnitProperty(serialized, nameof(EnemyDefinition.xpBudget), "XP Budget", "XP");
        ContentWorkbenchFieldDrawer.DrawProperty(serialized, nameof(EnemyDefinition.dropStyle), "Drop Style");
    }

    private static void DrawVisual(SerializedObject serialized, EnemyDefinition enemy)
    {
        ContentWorkbenchFieldDrawer.DrawSection("Runtime Model");
        ContentWorkbenchFieldDrawer.DrawProperty(serialized, nameof(EnemyDefinition.worldPrefab), "World Prefab");
        DrawPreviewButtons(enemy);
        DrawModelTransform(serialized.FindProperty(nameof(EnemyDefinition.modelTransform)));

        ContentWorkbenchFieldDrawer.DrawSection("Locomotion Visuals");
        DrawEnemyLocomotion(serialized.FindProperty(nameof(EnemyDefinition.locomotion)));

        ContentWorkbenchFieldDrawer.DrawSection("Animations");
        DrawEnemyAnimation(serialized.FindProperty(nameof(EnemyDefinition.animation)));

        ContentWorkbenchFieldDrawer.DrawSection("Visual Diagnostics");
        DrawDiagnosticsList(ActorAnimationAuthoringRules.EnemyDiagnostics(enemy));
    }

    private static void DrawDiagnostics(EnemyDefinition enemy)
    {
        ContentWorkbenchFieldDrawer.DrawSection("Diagnostics");
        EditorGUILayout.LabelField("Runtime Summary", $"{enemy.DisplayName} / {enemy.behaviorType} / weight {enemy.spawnWeight:0.##} / HP {enemy.baseHealth} +{enemy.healthGrowthPerMinute:0.##}/min / {enemy.baseMoveSpeed:0.##} u/s / {enemy.stack.policy}", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("Climb", EnemyAuthoringApplicability.ClimbSummary(enemy.climb), EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("Stack", EnemyAuthoringApplicability.StackSummary(enemy.stack), EditorStyles.wordWrappedLabel);
        ContentWorkbenchFieldDrawer.DrawSubsection("Actor Visuals");
        DrawDiagnosticsList(ActorAnimationAuthoringRules.EnemyDiagnostics(enemy));
    }

    private static void DrawPreviewButtons(EnemyDefinition enemy)
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Preview Model / Clips", GUILayout.Width(170f), GUILayout.Height(22f)))
            {
                ActorVisualPreviewWindow.Open(enemy);
            }

            using (new EditorGUI.DisabledScope(enemy == null || enemy.worldPrefab == null))
            {
                if (GUILayout.Button("Ping Model", GUILayout.Width(100f), GUILayout.Height(22f)))
                {
                    EditorGUIUtility.PingObject(enemy.worldPrefab);
                }
            }
        }
    }

    private static void DrawModelTransform(SerializedProperty transform)
    {
        if (transform == null)
        {
            EditorGUILayout.HelpBox("Missing model transform profile.", MessageType.Warning);
            return;
        }

        ContentWorkbenchFieldDrawer.DrawSubsection("Local Transform");
        DrawNestedUnitProperty(transform, nameof(ActorModelTransformProfile.localPosition), "Position", "u");
        DrawNestedUnitProperty(transform, nameof(ActorModelTransformProfile.localEulerAngles), "Rotation", "deg");
        DrawNestedProperty(transform, nameof(ActorModelTransformProfile.localScale), "Scale");
    }

    private static void DrawEnemyLocomotion(SerializedProperty locomotion)
    {
        if (locomotion == null)
        {
            EditorGUILayout.HelpBox("Missing enemy locomotion profile.", MessageType.Warning);
            return;
        }

        DrawNestedProperty(locomotion, nameof(EnemyLocomotionProfile.mode), "Mode");
        SerializedProperty mode = locomotion.FindPropertyRelative(nameof(EnemyLocomotionProfile.mode));
        bool floating = mode != null && mode.enumValueIndex == (int)EnemyLocomotionMode.Floating;
        if (!floating)
        {
            EditorGUILayout.LabelField("Grounded enemies stay on terrain and use idle/run clips when assigned.", EditorStyles.wordWrappedMiniLabel);
            return;
        }

        DrawNestedUnitProperty(locomotion, nameof(EnemyLocomotionProfile.hoverHeight), "Hover Height", "u");
        DrawNestedUnitProperty(locomotion, nameof(EnemyLocomotionProfile.hoverSeconds), "Hover Cycle", "sec");
        DrawNestedProperty(locomotion, nameof(EnemyLocomotionProfile.hoverEase), "Hover Ease");
    }

    private static void DrawEnemyAnimation(SerializedProperty animation)
    {
        if (animation == null)
        {
            EditorGUILayout.HelpBox("Missing enemy animation profile.", MessageType.Warning);
            return;
        }

        ContentWorkbenchFieldDrawer.DrawSubsection("Clip Slots");
        DrawNestedProperty(animation, nameof(EnemyAnimationProfile.idle), "Idle");
        DrawNestedProperty(animation, nameof(EnemyAnimationProfile.run), "Run");
        DrawNestedProperty(animation, nameof(EnemyAnimationProfile.floatingIdle), "Floating Idle");

        ContentWorkbenchFieldDrawer.DrawSubsection("Clip Speeds");
        DrawNestedUnitProperty(animation, nameof(EnemyAnimationProfile.idleSpeed), "Idle Speed", "x");
        DrawNestedUnitProperty(animation, nameof(EnemyAnimationProfile.runSpeed), "Run Speed", "x");
        DrawNestedUnitProperty(animation, nameof(EnemyAnimationProfile.floatingIdleSpeed), "Floating Idle Speed", "x");

        ContentWorkbenchFieldDrawer.DrawSubsection("Run Blend");
        DrawNestedProperty(animation, nameof(EnemyAnimationProfile.scaleRunSpeedWithMovement), "Scale Run Speed");
        DrawNestedProperty(animation, nameof(EnemyAnimationProfile.minRunSpeedMultiplier), "Min Run Speed");
        DrawNestedProperty(animation, nameof(EnemyAnimationProfile.runSpeedSmoothingSharpness), "Run Smoothing");
        DrawNestedProperty(animation, nameof(EnemyAnimationProfile.runThreshold01), "Run Enter");
        DrawNestedProperty(animation, nameof(EnemyAnimationProfile.runExitThreshold01), "Run Exit");

        ContentWorkbenchFieldDrawer.DrawSubsection("Timing");
        DrawNestedUnitProperty(animation, nameof(EnemyAnimationProfile.fadeSeconds), "Fade", "sec");
    }

    private static void DrawDiagnosticsList(IReadOnlyList<ActorAnimationAuthoringDiagnostic> diagnostics)
    {
        if (diagnostics == null || diagnostics.Count == 0)
        {
            EditorGUILayout.LabelField("No actor visual diagnostics.", EditorStyles.miniLabel);
            return;
        }

        for (int i = 0; i < diagnostics.Count; i++)
        {
            ActorAnimationAuthoringDiagnostic diagnostic = diagnostics[i];
            MessageType messageType = diagnostic.Severity switch
            {
                ActorAnimationDiagnosticSeverity.Error => MessageType.Error,
                ActorAnimationDiagnosticSeverity.Warning => MessageType.Warning,
                _ => MessageType.Info
            };
            EditorGUILayout.HelpBox(diagnostic.Message, messageType);
        }
    }

    private static void DrawNestedProperty(SerializedProperty owner, string propertyName, string label, bool includeChildren = false)
    {
        SerializedProperty property = owner.FindPropertyRelative(propertyName);
        if (property == null)
        {
            EditorGUILayout.HelpBox($"Missing serialized field: {propertyName}", MessageType.Warning);
            return;
        }

        EditorGUILayout.PropertyField(property, new GUIContent(label), includeChildren);
    }

    private static void DrawNestedUnitProperty(SerializedProperty owner, string propertyName, string label, string unit)
    {
        SerializedProperty property = owner.FindPropertyRelative(propertyName);
        if (property == null)
        {
            EditorGUILayout.HelpBox($"Missing serialized field: {propertyName}", MessageType.Warning);
            return;
        }

        ContentWorkbenchFieldDrawer.DrawUnitProperty(property, label, unit);
    }

    private static bool NestedBool(SerializedProperty owner, string propertyName)
    {
        SerializedProperty property = owner.FindPropertyRelative(propertyName);
        return property != null && property.boolValue;
    }

    private static T NestedEnum<T>(SerializedProperty owner, string propertyName)
        where T : struct
    {
        SerializedProperty property = owner.FindPropertyRelative(propertyName);
        return property != null && System.Enum.IsDefined(typeof(T), property.enumValueIndex)
            ? (T)System.Enum.ToObject(typeof(T), property.enumValueIndex)
            : default;
    }
}

public enum ContentWorkbenchEnemyTab
{
    Identity = 0,
    Spawning = 1,
    Stats = 2,
    Body = 3,
    Climb = 4,
    Stack = 5,
    Drops = 6,
    Visual = 7,
    Diagnostics = 8
}
