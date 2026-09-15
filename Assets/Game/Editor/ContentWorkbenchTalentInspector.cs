using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using TheCircussyOne.Content;
using UnityEditor;
using UnityEngine;

public static class ContentWorkbenchTalentInspector
{
    private static readonly string[] TabLabels =
    {
        "Identity",
        "Pool",
        "Progression",
        "Effect",
        "Visual",
        "Diagnostics"
    };

    private static ContentWorkbenchTalentTab selectedTab = ContentWorkbenchTalentTab.Identity;

    public static bool CanDraw(Object asset)
    {
        return asset is TalentDefinition;
    }

    public static void Draw(TalentDefinition talent)
    {
        if (talent == null)
        {
            EditorGUILayout.HelpBox("No talent selected.", MessageType.Info);
            return;
        }

        var serialized = new SerializedObject(talent);
        serialized.Update();

        DrawHeader(talent);
        DrawTabs();
        switch (selectedTab)
        {
            case ContentWorkbenchTalentTab.Identity:
                DrawIdentity(serialized, talent);
                break;
            case ContentWorkbenchTalentTab.Pool:
                DrawPool(serialized, talent);
                break;
            case ContentWorkbenchTalentTab.Progression:
                DrawProgression(serialized, talent);
                break;
            case ContentWorkbenchTalentTab.Effect:
                DrawEffect(serialized);
                break;
            case ContentWorkbenchTalentTab.Visual:
                DrawVisual(serialized);
                break;
            case ContentWorkbenchTalentTab.Diagnostics:
                DrawDiagnostics(talent);
                break;
        }

        if (serialized.ApplyModifiedProperties())
        {
            EditorUtility.SetDirty(talent);
        }
    }

    public static int TabCount => TabLabels.Length;

    public static string TabLabel(int index)
    {
        return TabLabels[Mathf.Clamp(index, 0, TabLabels.Length - 1)];
    }

    private static void DrawHeader(TalentDefinition talent)
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField("Talent Authoring", EditorStyles.boldLabel);
            if (GUILayout.Button("Open Odin Inspector", GUILayout.Width(150f), GUILayout.Height(22f)))
            {
                OdinEditorWindow.InspectObject(talent);
            }
        }

        EditorGUILayout.LabelField("Workbench uses focused tabs here so performer access and stat effects use catalog-backed controls. Use Odin Inspector for the full advanced asset view.", EditorStyles.wordWrappedMiniLabel);
    }

    private static void DrawTabs()
    {
        EditorGUILayout.Space(4f);
        selectedTab = (ContentWorkbenchTalentTab)GUILayout.Toolbar((int)selectedTab, TabLabels, GUILayout.MinHeight(24f));
        EditorGUILayout.Space(4f);
    }

    private static void DrawIdentity(SerializedObject serialized, TalentDefinition talent)
    {
        ContentWorkbenchFieldDrawer.DrawSection("Identity");
        ContentWorkbenchFieldDrawer.DrawReadOnlyProperty(serialized, nameof(TalentDefinition.talentId), "Talent ID");
        ContentWorkbenchFieldDrawer.DrawProperty(serialized, nameof(TalentDefinition.displayName), "Display Name");
        ContentWorkbenchFieldDrawer.DrawIdNameSync(
            serialized.FindProperty(nameof(TalentDefinition.talentId)),
            serialized.FindProperty(nameof(TalentDefinition.displayName))?.stringValue,
            "talent",
            "Talent ID");
        ContentWorkbenchFieldDrawer.DrawProperty(serialized, nameof(TalentDefinition.shortDescription), "Short Description", includeChildren: true);

        ContentWorkbenchFieldDrawer.DrawRaritySet(serialized.FindProperty(nameof(TalentDefinition.possibleRarities)));
        EditorGUILayout.LabelField("Computed Tags", ComputedContentTagRules.Format(talent.Tags), EditorStyles.miniLabel);
    }

    private static void DrawPool(SerializedObject serialized, TalentDefinition talent)
    {
        ContentWorkbenchFieldDrawer.DrawSection("Pool");
        SerializedProperty poolKind = serialized.FindProperty(nameof(TalentDefinition.poolKind));
        EditorGUILayout.PropertyField(poolKind, new GUIContent("Pool Kind"));

        TalentPoolKind selectedKind = (TalentPoolKind)poolKind.enumValueIndex;
        if (selectedKind == TalentPoolKind.PerformerSpecific)
        {
            DrawPerformerSpecificPool(serialized, talent);
        }
        else
        {
            DrawSharedPool(serialized);
        }

        EditorGUILayout.LabelField("Summary", talent.PoolSummary, EditorStyles.wordWrappedMiniLabel);
    }

    private static void DrawPerformerSpecificPool(SerializedObject serialized, TalentDefinition talent)
    {
        ContentWorkbenchFieldDrawer.DrawSubsection("Performer-Specific");
        SerializedProperty performerId = serialized.FindProperty(nameof(TalentDefinition.performerId));
        SerializedProperty performerDefinition = serialized.FindProperty(nameof(TalentDefinition.performerDefinition));
        PerformerDefinition current = performerDefinition?.objectReferenceValue as PerformerDefinition;
        PerformerDefinition selected = DrawPerformerDropdown("Performer", current, performerId?.stringValue, out bool changed);
        if (changed && selected != null)
        {
            if (performerDefinition != null)
            {
                performerDefinition.objectReferenceValue = selected;
            }

            if (performerId != null)
            {
                performerId.stringValue = selected.Id;
            }
        }
        else if (current == null
            && selected != null
            && performerDefinition != null
            && performerId != null
            && !string.IsNullOrWhiteSpace(performerId.stringValue))
        {
            performerDefinition.objectReferenceValue = selected;
        }
    }

    private static void DrawSharedPool(SerializedObject serialized)
    {
        ContentWorkbenchFieldDrawer.DrawSubsection("Shared Performer Access");
        SerializedProperty accessList = serialized.FindProperty(nameof(TalentDefinition.sharedPerformerAccess));
        if (accessList == null || !accessList.isArray)
        {
            EditorGUILayout.HelpBox("Missing shared performer access list.", MessageType.Warning);
            return;
        }

        IReadOnlyList<PerformerDefinition> performers = LoadCatalogPerformers();
        if (performers == null || performers.Count == 0)
        {
            EditorGUILayout.HelpBox("No performers in the catalog. Shared talents without explicit access rows are available to all performers.", MessageType.Info);
            DrawOrphanedAccessRows(accessList, performers);
            return;
        }

        if (accessList.arraySize > 0)
        {
            EnsureExplicitSharedAccessRows(accessList, performers, defaultEnabled: true);
        }

        for (int performerIndex = 0; performerIndex < performers.Count; performerIndex++)
        {
            PerformerDefinition performer = performers[performerIndex];
            if (performer == null || string.IsNullOrWhiteSpace(performer.Id))
            {
                continue;
            }

            int accessIndex = FindAccessIndex(accessList, performer);
            if (accessIndex < 0)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    bool nextEnabled = EditorGUILayout.Toggle(true, GUILayout.Width(20f));
                    EditorGUILayout.LabelField(ContentOptionLabel(performer), EditorStyles.miniLabel);
                    if (!nextEnabled)
                    {
                        EnsureExplicitSharedAccessRows(accessList, performers, defaultEnabled: true);
                        int explicitIndex = FindAccessIndex(accessList, performer);
                        if (explicitIndex >= 0)
                        {
                            accessList.GetArrayElementAtIndex(explicitIndex)
                                .FindPropertyRelative(nameof(TalentPerformerAccess.enabled))
                                .boolValue = false;
                        }
                    }
                }
                continue;
            }

            SerializedProperty access = accessList.GetArrayElementAtIndex(accessIndex);
            SerializedProperty enabled = access.FindPropertyRelative(nameof(TalentPerformerAccess.enabled));
            using (new EditorGUILayout.HorizontalScope())
            {
                enabled.boolValue = EditorGUILayout.Toggle(enabled.boolValue, GUILayout.Width(20f));
                EditorGUILayout.LabelField(ContentOptionLabel(performer), EditorStyles.miniLabel);
            }
        }

        DrawOrphanedAccessRows(accessList, performers);
    }

    private static void EnsureExplicitSharedAccessRows(SerializedProperty accessList, IReadOnlyList<PerformerDefinition> performers, bool defaultEnabled)
    {
        if (accessList == null || performers == null)
        {
            return;
        }

        for (int i = 0; i < performers.Count; i++)
        {
            PerformerDefinition performer = performers[i];
            if (performer == null || string.IsNullOrWhiteSpace(performer.Id) || FindAccessIndex(accessList, performer) >= 0)
            {
                continue;
            }

            int index = accessList.arraySize;
            accessList.InsertArrayElementAtIndex(index);
            SerializedProperty access = accessList.GetArrayElementAtIndex(index);
            access.FindPropertyRelative(nameof(TalentPerformerAccess.enabled)).boolValue = defaultEnabled;
            access.FindPropertyRelative(nameof(TalentPerformerAccess.performer)).objectReferenceValue = performer;
            access.FindPropertyRelative(nameof(TalentPerformerAccess.performerId)).stringValue = performer.Id;
        }
    }

    private static void DrawProgression(SerializedObject serialized, TalentDefinition talent)
    {
        ContentWorkbenchFieldDrawer.DrawSection("Repeat Rules");
        ContentWorkbenchFieldDrawer.DrawProperty(serialized, nameof(TalentDefinition.repeatPolicy), "Repeat Policy");
        if (talent.repeatPolicy == TalentRepeatPolicy.Capped)
        {
            ContentWorkbenchFieldDrawer.DrawProperty(serialized, nameof(TalentDefinition.maxLevel), "Max Level");
        }
        else
        {
            EditorGUILayout.LabelField("Max level is hidden because this talent is repeatable without a cap.", EditorStyles.wordWrappedMiniLabel);
        }
    }

    private static void DrawEffect(SerializedObject serialized)
    {
        ContentWorkbenchFieldDrawer.DrawSection("Stat Modifiers");
        ContentWorkbenchEffectInspector.DrawUpgradeModifierList(
            serialized.FindProperty(nameof(TalentDefinition.statModifiers)),
            ContentWorkbenchDomain.Talents);
    }

    private static void DrawVisual(SerializedObject serialized)
    {
        ContentWorkbenchFieldDrawer.DrawSection("Card");
        ContentWorkbenchFieldDrawer.DrawIconSpriteProperty(
            serialized,
            nameof(TalentDefinition.iconSprite),
            "Talent Icon",
            "No icon assigned. Performer-specific talents can inherit their performer portrait; shared talents use fallback color until an icon is assigned.");
        ContentWorkbenchFieldDrawer.DrawProperty(serialized, nameof(TalentDefinition.iconColor), "Fallback Color");
    }

    private static void DrawDiagnostics(TalentDefinition talent)
    {
        ContentWorkbenchFieldDrawer.DrawSection("Diagnostics");
        EditorGUILayout.LabelField("Runtime Summary", $"{talent.DisplayName} [{talent.LevelUpRarities}] {talent.PoolSummary} {talent.repeatPolicy}: {talent.shortDescription}", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("Affected Stats", TalentStatSummary(talent), EditorStyles.wordWrappedMiniLabel);
    }

    private static PerformerDefinition DrawPerformerDropdown(string label, PerformerDefinition current, string currentId, out bool changed)
    {
        changed = false;
        IReadOnlyList<PerformerDefinition> performers = LoadCatalogPerformers();
        var options = new List<PerformerDefinition>();
        if (performers != null)
        {
            for (int i = 0; i < performers.Count; i++)
            {
                if (performers[i] != null && !ContainsReference(options, performers[i]))
                {
                    options.Add(performers[i]);
                }
            }
        }

        if (current != null && !ContainsReference(options, current))
        {
            options.Add(current);
        }

        if (options.Count == 0)
        {
            EditorGUILayout.LabelField(label, string.IsNullOrWhiteSpace(currentId) ? "No performers in catalog" : currentId);
            return current;
        }

        int selectedIndex = -1;
        for (int i = 0; i < options.Count; i++)
        {
            PerformerDefinition performer = options[i];
            if (ReferenceEquals(performer, current) || (!string.IsNullOrWhiteSpace(currentId) && performer.Id == currentId))
            {
                selectedIndex = i;
            }
        }

        bool hasUnsupportedCurrent = selectedIndex < 0 && !string.IsNullOrWhiteSpace(currentId);
        string[] labels = new string[options.Count + (hasUnsupportedCurrent ? 1 : 0)];
        for (int i = 0; i < options.Count; i++)
        {
            labels[i] = ContentOptionLabel(options[i]);
        }

        if (hasUnsupportedCurrent)
        {
            selectedIndex = labels.Length - 1;
            labels[selectedIndex] = $"Unsupported: {currentId}";
        }
        else if (selectedIndex < 0)
        {
            selectedIndex = 0;
        }

        int nextIndex = EditorGUILayout.Popup(label, selectedIndex, labels);
        changed = nextIndex != selectedIndex && nextIndex >= 0 && nextIndex < options.Count;
        return nextIndex >= 0 && nextIndex < options.Count ? options[nextIndex] : current;
    }

    private static void DrawOrphanedAccessRows(SerializedProperty accessList, IReadOnlyList<PerformerDefinition> performers)
    {
        bool wroteHeader = false;
        for (int i = 0; i < accessList.arraySize; i++)
        {
            SerializedProperty access = accessList.GetArrayElementAtIndex(i);
            SerializedProperty performer = access.FindPropertyRelative(nameof(TalentPerformerAccess.performer));
            SerializedProperty performerId = access.FindPropertyRelative(nameof(TalentPerformerAccess.performerId));
            if (IsKnownAccess(performers, performer?.objectReferenceValue as PerformerDefinition, performerId?.stringValue))
            {
                continue;
            }

            if (!wroteHeader)
            {
                EditorGUILayout.Space(4f);
                EditorGUILayout.LabelField("Uncataloged Access", EditorStyles.miniBoldLabel);
                wroteHeader = true;
            }

            SerializedProperty enabled = access.FindPropertyRelative(nameof(TalentPerformerAccess.enabled));
            using (new EditorGUILayout.HorizontalScope())
            {
                enabled.boolValue = EditorGUILayout.Toggle(enabled.boolValue, GUILayout.Width(20f));
                EditorGUILayout.LabelField($"Unsupported: {DisplayPerformerAccess(performer?.objectReferenceValue as PerformerDefinition, performerId?.stringValue)}", EditorStyles.miniLabel);
            }
        }
    }

    private static int FindAccessIndex(SerializedProperty accessList, PerformerDefinition performer)
    {
        if (accessList == null || performer == null)
        {
            return -1;
        }

        for (int i = 0; i < accessList.arraySize; i++)
        {
            SerializedProperty access = accessList.GetArrayElementAtIndex(i);
            SerializedProperty performerReference = access.FindPropertyRelative(nameof(TalentPerformerAccess.performer));
            SerializedProperty performerId = access.FindPropertyRelative(nameof(TalentPerformerAccess.performerId));
            if (ReferenceEquals(performerReference?.objectReferenceValue, performer)
                || (!string.IsNullOrWhiteSpace(performerId?.stringValue) && performerId.stringValue == performer.Id))
            {
                return i;
            }
        }

        return -1;
    }

    private static bool IsKnownAccess(IReadOnlyList<PerformerDefinition> performers, PerformerDefinition performer, string performerId)
    {
        if (performers == null)
        {
            return false;
        }

        for (int i = 0; i < performers.Count; i++)
        {
            PerformerDefinition candidate = performers[i];
            if (candidate == null)
            {
                continue;
            }

            if (ReferenceEquals(candidate, performer)
                || (!string.IsNullOrWhiteSpace(performerId) && performerId == candidate.Id))
            {
                return true;
            }
        }

        return false;
    }

    private static string DisplayPerformerAccess(PerformerDefinition performer, string performerId)
    {
        if (performer != null)
        {
            return ContentOptionLabel(performer);
        }

        return string.IsNullOrWhiteSpace(performerId) ? "Missing Performer" : performerId;
    }

    private static string TalentStatSummary(TalentDefinition talent)
    {
        if (talent.statModifiers == null || talent.statModifiers.Count == 0)
        {
            return "No stat modifiers.";
        }

        var names = new List<string>();
        for (int i = 0; i < talent.statModifiers.Count; i++)
        {
            names.Add(ContentModifierDisplayRules.StatDisplayName(talent.statModifiers[i].statId));
        }

        return string.Join(", ", names);
    }

    private static IReadOnlyList<PerformerDefinition> LoadCatalogPerformers()
    {
        PerformerCatalog catalog = AssetDatabase.LoadAssetAtPath<PerformerCatalog>(TheCircussyOneAssetPaths.PerformerCatalogPath);
        return catalog?.Performers;
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

public enum ContentWorkbenchTalentTab
{
    Identity = 0,
    Pool = 1,
    Progression = 2,
    Effect = 3,
    Visual = 4,
    Diagnostics = 5
}
