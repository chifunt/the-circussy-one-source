using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using TheCircussyOne.Content;
using UnityEditor;
using UnityEngine;

public static class ContentWorkbenchPerformerInspector
{
    private static readonly string[] TabLabels =
    {
        "Identity",
        "Loadout",
        "Stats",
        "Passive",
        "Visual",
        "Theme",
        "Diagnostics"
    };

    private static ContentWorkbenchPerformerTab selectedTab = ContentWorkbenchPerformerTab.Identity;

    public static bool CanDraw(Object asset)
    {
        return asset is PerformerDefinition;
    }

    public static void Draw(PerformerDefinition performer)
    {
        if (performer == null)
        {
            EditorGUILayout.HelpBox("No performer selected.", MessageType.Info);
            return;
        }

        var serialized = new SerializedObject(performer);
        serialized.Update();

        DrawHeader(performer);
        DrawTabs();
        switch (selectedTab)
        {
            case ContentWorkbenchPerformerTab.Identity:
                DrawIdentity(serialized, performer);
                break;
            case ContentWorkbenchPerformerTab.Loadout:
                DrawLoadout(serialized, performer);
                break;
            case ContentWorkbenchPerformerTab.Stats:
                DrawStats(serialized);
                break;
            case ContentWorkbenchPerformerTab.Passive:
                DrawPassive(serialized, performer);
                break;
            case ContentWorkbenchPerformerTab.Visual:
                DrawVisual(serialized, performer);
                break;
            case ContentWorkbenchPerformerTab.Theme:
                DrawTheme(serialized);
                break;
            case ContentWorkbenchPerformerTab.Diagnostics:
                DrawDiagnostics(performer);
                break;
        }

        if (serialized.ApplyModifiedProperties())
        {
            EditorUtility.SetDirty(performer);
        }
    }

    public static int TabCount => TabLabels.Length;

    public static string TabLabel(int index)
    {
        return TabLabels[Mathf.Clamp(index, 0, TabLabels.Length - 1)];
    }

    private static void DrawHeader(PerformerDefinition performer)
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField("Performer Authoring", EditorStyles.boldLabel);
            if (GUILayout.Button("Open Odin Inspector", GUILayout.Width(150f), GUILayout.Height(22f)))
            {
                OdinEditorWindow.InspectObject(performer);
            }
        }

        EditorGUILayout.LabelField("Workbench uses focused tabs here so references use catalog dropdowns. Use Odin Inspector for the full advanced asset view.", EditorStyles.wordWrappedMiniLabel);
    }

    private static void DrawTabs()
    {
        EditorGUILayout.Space(4f);
        selectedTab = (ContentWorkbenchPerformerTab)GUILayout.Toolbar((int)selectedTab, TabLabels, GUILayout.MinHeight(24f));
        EditorGUILayout.Space(4f);
    }

    private static void DrawIdentity(SerializedObject serialized, PerformerDefinition performer)
    {
        ContentWorkbenchFieldDrawer.DrawSection("Identity");
        ContentWorkbenchFieldDrawer.DrawReadOnlyProperty(serialized, nameof(PerformerDefinition.performerId), "Performer ID");
        ContentWorkbenchFieldDrawer.DrawProperty(serialized, nameof(PerformerDefinition.displayName), "Name");
        ContentWorkbenchFieldDrawer.DrawProperty(serialized, nameof(PerformerDefinition.performerTitle), "Title");
        string performerIdentityName = $"{serialized.FindProperty(nameof(PerformerDefinition.displayName))?.stringValue} {serialized.FindProperty(nameof(PerformerDefinition.performerTitle))?.stringValue}";
        ContentWorkbenchFieldDrawer.DrawIdNameSync(
            serialized.FindProperty(nameof(PerformerDefinition.performerId)),
            performerIdentityName,
            "performer",
            "Performer ID");
        ContentWorkbenchFieldDrawer.DrawProperty(serialized, nameof(PerformerDefinition.shortDescription), "Short Description", includeChildren: true);
        ContentWorkbenchFieldDrawer.DrawProperty(serialized, nameof(PerformerDefinition.unlocked), "Unlocked");
        EditorGUILayout.LabelField("Computed Tags", ComputedContentTagRules.Format(performer.Tags), EditorStyles.miniLabel);
    }

    private static void DrawLoadout(SerializedObject serialized, PerformerDefinition performer)
    {
        ContentWorkbenchFieldDrawer.DrawSection("Starting Loadout");
        DrawStartingWeaponDropdown(serialized.FindProperty(nameof(PerformerDefinition.startingWeapon)), performer.startingWeapon);
    }

    private static void DrawStats(SerializedObject serialized)
    {
        ContentWorkbenchFieldDrawer.DrawSection("Base Modifiers");
        ContentWorkbenchEffectInspector.DrawUpgradeModifierList(
            serialized.FindProperty(nameof(PerformerDefinition.baseStatModifiers)),
            ContentWorkbenchDomain.Talents);
    }

    private static void DrawPassive(SerializedObject serialized, PerformerDefinition performer)
    {
        ContentWorkbenchFieldDrawer.DrawSection("Passive");
        ContentWorkbenchFieldDrawer.DrawProperty(serialized, nameof(PerformerDefinition.passiveDescription), "Description", includeChildren: true);
        ContentWorkbenchFieldDrawer.DrawProperty(serialized, nameof(PerformerDefinition.passiveKind), "Kind");

        if (performer.UsesStaticPassive)
        {
            ContentWorkbenchFieldDrawer.DrawSubsection("Passive Modifiers");
            ContentWorkbenchEffectInspector.DrawUpgradeModifierList(
                serialized.FindProperty(nameof(PerformerDefinition.passiveStatModifiers)),
                ContentWorkbenchDomain.Talents);
        }
        else
        {
            EditorGUILayout.LabelField("Passive stat modifiers are hidden because this performer has no static stat passive.", EditorStyles.wordWrappedMiniLabel);
        }
    }

    private static void DrawVisual(SerializedObject serialized, PerformerDefinition performer)
    {
        ContentWorkbenchFieldDrawer.DrawSection("Selection Card");
        ContentWorkbenchFieldDrawer.DrawIconSpriteProperty(
            serialized,
            nameof(PerformerDefinition.portraitSprite),
            "Portrait Icon",
            "No portrait sprite assigned. Performer cards use fallback color until a placeholder or final portrait is assigned.");
        ContentWorkbenchFieldDrawer.DrawProperty(serialized, nameof(PerformerDefinition.portraitColor), "Fallback Color");

        ContentWorkbenchFieldDrawer.DrawSection("Runtime Model");
        ContentWorkbenchFieldDrawer.DrawProperty(serialized, nameof(PerformerDefinition.worldPrefab), "World Prefab");
        DrawPreviewButtons(performer);
        DrawModelTransform(serialized.FindProperty(nameof(PerformerDefinition.modelTransform)));

        ContentWorkbenchFieldDrawer.DrawSection("Animations");
        DrawPerformerAnimation(serialized.FindProperty(nameof(PerformerDefinition.animation)));

        ContentWorkbenchFieldDrawer.DrawSection("Procedural Bop");
        DrawPerformerMotionVisuals(serialized.FindProperty(nameof(PerformerDefinition.motionVisuals)));

        ContentWorkbenchFieldDrawer.DrawSection("Visual Diagnostics");
        DrawDiagnosticsList(ActorAnimationAuthoringRules.PerformerDiagnostics(performer));
    }

    private static void DrawTheme(SerializedObject serialized)
    {
        ContentWorkbenchFieldDrawer.DrawSection("Talent Theme");
        ContentWorkbenchFieldDrawer.DrawProperty(serialized, nameof(PerformerDefinition.talentThemeSummary), "Summary", includeChildren: true);
    }

    private static void DrawDiagnostics(PerformerDefinition performer)
    {
        ContentWorkbenchFieldDrawer.DrawSection("Diagnostics");
        EditorGUILayout.LabelField("Runtime Summary", $"{performer.DisplayName}: {performer.StartingWeaponName}, talents are assigned on Talent assets", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("Passive", performer.passiveDescription, EditorStyles.wordWrappedLabel);
        ContentWorkbenchFieldDrawer.DrawSubsection("Actor Visuals");
        DrawDiagnosticsList(ActorAnimationAuthoringRules.PerformerDiagnostics(performer));
    }

    private static void DrawPreviewButtons(PerformerDefinition performer)
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Preview Model / Clips", GUILayout.Width(170f), GUILayout.Height(22f)))
            {
                ActorVisualPreviewWindow.Open(performer);
            }

            using (new EditorGUI.DisabledScope(performer == null || performer.worldPrefab == null))
            {
                if (GUILayout.Button("Ping Model", GUILayout.Width(100f), GUILayout.Height(22f)))
                {
                    EditorGUIUtility.PingObject(performer.worldPrefab);
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

    private static void DrawPerformerAnimation(SerializedProperty animation)
    {
        if (animation == null)
        {
            EditorGUILayout.HelpBox("Missing performer animation profile.", MessageType.Warning);
            return;
        }

        EditorGUILayout.LabelField("Jump and land clips are optional. If empty, procedural jump/landing visuals remain active.", EditorStyles.wordWrappedMiniLabel);

        ContentWorkbenchFieldDrawer.DrawSubsection("Clip Slots");
        DrawNestedProperty(animation, nameof(PerformerAnimationProfile.idle), "Idle");
        DrawNestedProperty(animation, nameof(PerformerAnimationProfile.run), "Run");
        DrawNestedProperty(animation, nameof(PerformerAnimationProfile.jump), "Jump");
        DrawNestedProperty(animation, nameof(PerformerAnimationProfile.land), "Land");

        ContentWorkbenchFieldDrawer.DrawSubsection("Clip Speeds");
        DrawNestedUnitProperty(animation, nameof(PerformerAnimationProfile.idleSpeed), "Idle Speed", "x");
        DrawNestedUnitProperty(animation, nameof(PerformerAnimationProfile.runSpeed), "Run Speed", "x");
        DrawNestedUnitProperty(animation, nameof(PerformerAnimationProfile.jumpSpeed), "Jump Speed", "x");
        DrawNestedUnitProperty(animation, nameof(PerformerAnimationProfile.landSpeed), "Land Speed", "x");

        ContentWorkbenchFieldDrawer.DrawSubsection("Run Blend");
        DrawNestedProperty(animation, nameof(PerformerAnimationProfile.scaleRunSpeedWithMovement), "Scale Run Speed");
        DrawNestedProperty(animation, nameof(PerformerAnimationProfile.minRunSpeedMultiplier), "Min Run Speed");
        DrawNestedProperty(animation, nameof(PerformerAnimationProfile.runSpeedSmoothingSharpness), "Run Smoothing");
        DrawNestedProperty(animation, nameof(PerformerAnimationProfile.runThreshold01), "Run Enter");
        DrawNestedProperty(animation, nameof(PerformerAnimationProfile.runExitThreshold01), "Run Exit");

        ContentWorkbenchFieldDrawer.DrawSubsection("Timing");
        DrawNestedUnitProperty(animation, nameof(PerformerAnimationProfile.fadeSeconds), "Fade", "sec");
        DrawNestedUnitProperty(animation, nameof(PerformerAnimationProfile.landLockSeconds), "Land Lock", "sec");
    }

    private static void DrawPerformerMotionVisuals(SerializedProperty motion)
    {
        if (motion == null)
        {
            EditorGUILayout.HelpBox("Missing procedural motion profile.", MessageType.Warning);
            return;
        }

        DrawNestedProperty(motion, nameof(PerformerMotionVisualProfile.overrideGlobalBop), "Override Global Bop");
        SerializedProperty overrideBop = motion.FindPropertyRelative(nameof(PerformerMotionVisualProfile.overrideGlobalBop));
        bool isOverride = overrideBop != null && overrideBop.boolValue;
        if (!isOverride)
        {
            EditorGUILayout.LabelField("Using global Actor Motion Visual Config. Enable override to tune this performer here.", EditorStyles.wordWrappedMiniLabel);
        }

        using (new EditorGUI.DisabledScope(!isOverride))
        {
            DrawNestedProperty(motion, nameof(PerformerMotionVisualProfile.proceduralBopEnabled), "Procedural Bop");
            SerializedProperty enabled = motion.FindPropertyRelative(nameof(PerformerMotionVisualProfile.proceduralBopEnabled));
            bool isEnabled = enabled == null || enabled.boolValue;
            using (new EditorGUI.DisabledScope(!isEnabled))
            {
                ContentWorkbenchFieldDrawer.DrawSubsection("Idle Bop");
                DrawNestedUnitProperty(motion, nameof(PerformerMotionVisualProfile.idleAmplitude), "Amplitude", "scale");
                DrawNestedUnitProperty(motion, nameof(PerformerMotionVisualProfile.idleFrequency), "Frequency", "loops/s");
                DrawNestedProperty(motion, nameof(PerformerMotionVisualProfile.idleWidthCompensation), "Width Compensation");

                ContentWorkbenchFieldDrawer.DrawSubsection("Move Bop");
                DrawNestedUnitProperty(motion, nameof(PerformerMotionVisualProfile.moveAmplitude), "Amplitude", "scale");
                DrawNestedUnitProperty(motion, nameof(PerformerMotionVisualProfile.moveFrequency), "Frequency", "loops/s");
                DrawNestedProperty(motion, nameof(PerformerMotionVisualProfile.moveWidthCompensation), "Width Compensation");
            }
        }
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

    private static void DrawStartingWeaponDropdown(SerializedProperty property, WeaponDefinition current)
    {
        if (property == null)
        {
            EditorGUILayout.HelpBox("Missing starting weapon field.", MessageType.Warning);
            return;
        }

        IReadOnlyList<WeaponDefinition> weapons = LoadCatalogWeapons();
        var options = new List<WeaponDefinition> { null };
        if (weapons != null)
        {
            for (int i = 0; i < weapons.Count; i++)
            {
                if (weapons[i] != null && !ContainsReference(options, weapons[i]))
                {
                    options.Add(weapons[i]);
                }
            }
        }

        if (current != null && !ContainsReference(options, current))
        {
            options.Add(current);
        }

        if (options.Count == 1)
        {
            EditorGUILayout.LabelField("Starting Weapon", "No weapons in catalog");
            return;
        }

        string[] labels = new string[options.Count];
        int selectedIndex = 0;
        for (int i = 0; i < options.Count; i++)
        {
            WeaponDefinition weapon = options[i];
            labels[i] = weapon != null ? ContentOptionLabel(weapon) : "Missing Starting Weapon";
            if (ReferenceEquals(weapon, current))
            {
                selectedIndex = i;
            }
        }

        int nextIndex = EditorGUILayout.Popup("Starting Weapon", selectedIndex, labels);
        if (nextIndex != selectedIndex)
        {
            property.objectReferenceValue = options[Mathf.Clamp(nextIndex, 0, options.Count - 1)];
        }
    }

    private static IReadOnlyList<WeaponDefinition> LoadCatalogWeapons()
    {
        WeaponCatalog catalog = AssetDatabase.LoadAssetAtPath<WeaponCatalog>(TheCircussyOneAssetPaths.WeaponCatalogPath);
        return catalog?.AvailableWeapons;
    }

    private static string ContentOptionLabel(IContentDefinition definition)
    {
        if (definition == null)
        {
            return "Missing";
        }

        string name = string.IsNullOrWhiteSpace(definition.DisplayName) ? definition.Id : definition.DisplayName;
        return $"{name} ({definition.Id})";
    }

    private static bool ContainsReference<T>(IReadOnlyList<T> values, T candidate)
        where T : class
    {
        if (values == null || candidate == null)
        {
            return false;
        }

        for (int i = 0; i < values.Count; i++)
        {
            if (ReferenceEquals(values[i], candidate))
            {
                return true;
            }
        }

        return false;
    }
}

public enum ContentWorkbenchPerformerTab
{
    Identity = 0,
    Loadout = 1,
    Stats = 2,
    Passive = 3,
    Visual = 4,
    Theme = 5,
    Diagnostics = 6
}
