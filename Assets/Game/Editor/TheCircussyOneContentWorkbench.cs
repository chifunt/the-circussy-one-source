using System;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using TheCircussyOne.Content;
using TheCircussyOne.Stats;
using UnityEditor;
using UnityEngine;

public sealed class TheCircussyOneContentWorkbench : EditorWindow
{
    private const float PreferredLeftPaneWidth = 370f;
    private const float MinimumLeftPaneWidth = 260f;
    private const float MinimumInspectorContentWidth = 680f;
    private const float PaneChromeWidth = 48f;
    private const float ActionButtonHeight = 24f;
    private static readonly string[] DetailModeLabels = { "Selected Content", "Create Content" };

    private ContentWorkbenchDomain domain = ContentWorkbenchDomain.Weapons;
    private ContentWorkbenchCreateTemplate createTemplate = ContentWorkbenchCreateTemplate.DirectProjectileWeapon;
    private ContentWorkbenchDetailMode detailMode = ContentWorkbenchDetailMode.SelectedContent;
    private string search = string.Empty;
    private bool showAvailable = true;
    private bool showDisabled = true;
    private bool showDraft = true;
    private bool showOk = true;
    private bool showWarnings = true;
    private bool showErrors = true;
    private bool showHelpLegend;
    private bool showUsageSummary;
    private bool showAuthoringSummary;
    private bool addToCatalog = true;
    private string createDisplayName = string.Empty;
    private WeaponDefinition targetWeapon;
    private PerformerDefinition targetPerformer;
    private StatId statId = StatId.WeaponFlatDamage;
    private StatModifierBucket bucket = StatModifierBucket.Flat;
    private float value = 1f;
    private UnityEngine.Object selectedAsset;
    private PropertyTree selectedPropertyTree;
    private Vector2 listScroll;
    private Vector2 detailScroll;
    private Vector2 createScroll;
    private string lastResult = "No action yet.";
    private MessageType lastResultType = MessageType.Info;

    [MenuItem("Tools/The Circussy One/Content Workbench")]
    public static void Open()
    {
        EnsureCoreContentAssets();
        var window = GetWindow<TheCircussyOneContentWorkbench>();
        window.titleContent = new GUIContent("Content Workbench");
        window.minSize = new Vector2(1080f, 640f);
        window.Show();
    }

    private void OnDisable()
    {
        DestroySelectedPropertyTree();
    }

    private void OnGUI()
    {
        DrawHeader();
        EditorGUILayout.Space(4f);
        DrawBody();
    }

    private void DrawHeader()
    {
        EditorGUILayout.LabelField("Designer Content Workbench", EditorStyles.boldLabel);
    }

    private void DrawBody()
    {
        IReadOnlyList<ContentWorkbenchEntry> entries = ContentWorkbenchService.LoadEntries(domain);
        using (new EditorGUILayout.HorizontalScope())
        {
            DrawLeftPane(entries);
            DrawRightPane(entries);
        }
    }

    private void DrawLeftPane(IReadOnlyList<ContentWorkbenchEntry> entries)
    {
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.Width(LeftPaneWidthForWindow(position.width)), GUILayout.ExpandHeight(true)))
        {
            DrawDomainMenu();
            EditorGUILayout.Space(5f);
            DrawFilters(entries);
            EditorGUILayout.Space(4f);
            DrawEntryList(entries);
        }
    }

    private void DrawDomainMenu()
    {
        EditorGUILayout.LabelField("Content", EditorStyles.boldLabel);
        IReadOnlyList<ContentWorkbenchDomainGroup> groups = ContentWorkbenchDomainLayout.DomainGroups;
        for (int groupIndex = 0; groupIndex < groups.Count; groupIndex++)
        {
            ContentWorkbenchDomainGroup group = groups[groupIndex];
            if (groupIndex > 0)
            {
                EditorGUILayout.Space(4f);
            }

            EditorGUILayout.LabelField(group.Title, EditorStyles.miniBoldLabel);
            for (int domainIndex = 0; domainIndex < group.Domains.Count; domainIndex++)
            {
                ContentWorkbenchDomain candidate = group.Domains[domainIndex];
                GUIStyle style = candidate == domain ? EditorStyles.toolbarButton : EditorStyles.miniButton;
                if (GUILayout.Button(ContentWorkbenchDomainLayout.DisplayName(candidate), style, GUILayout.Height(26f)))
                {
                    ChangeDomain(candidate);
                }
            }
        }
    }

    private void DrawFilters(IReadOnlyList<ContentWorkbenchEntry> entries)
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField("Domain Summary", EditorStyles.boldLabel);
            if (GUILayout.Button("?", EditorStyles.miniButton, GUILayout.Width(24f), GUILayout.Height(20f)))
            {
                showHelpLegend = !showHelpLegend;
            }
        }

        if (showHelpLegend)
        {
            DrawHelpLegend();
        }

        ContentWorkbenchDomainSummary summary = ContentWorkbenchDomainSummary.Build(entries, CurrentFilters());
        EditorGUILayout.LabelField($"{summary.Visible}/{summary.Total} shown", EditorStyles.miniLabel);

        using (new EditorGUILayout.HorizontalScope())
        {
            showAvailable = DrawAvailabilityFilterToggle(ContentWorkbenchAvailabilityState.Available, showAvailable, summary.Available);
            showDisabled = DrawAvailabilityFilterToggle(ContentWorkbenchAvailabilityState.Disabled, showDisabled, summary.Disabled);
            showDraft = DrawAvailabilityFilterToggle(ContentWorkbenchAvailabilityState.Draft, showDraft, summary.Draft);
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            showOk = DrawValidationFilterToggle("OK", showOk, summary.Ok, 0, 0);
            showWarnings = DrawValidationFilterToggle("Warnings", showWarnings, summary.Warnings, 0, 1);
            showErrors = DrawValidationFilterToggle("Errors", showErrors, summary.Errors, 1, 0);
        }

        GUIStyle searchStyle = GUI.skin.FindStyle("ToolbarSearchTextField") ?? EditorStyles.textField;
        EditorGUILayout.LabelField("Search", EditorStyles.miniBoldLabel);
        search = EditorGUILayout.TextField(search, searchStyle);
    }

    private static bool DrawAvailabilityFilterToggle(ContentWorkbenchAvailabilityState state, bool enabled, int count)
    {
        GUIStyle style = AvailabilityFilterStyle(EditorStyles.toolbarButton, state, enabled);
        Color previousBackground = GUI.backgroundColor;
        GUI.backgroundColor = ContentWorkbenchEntryLabels.AvailabilityFilterBackgroundColor(state, enabled, EditorGUIUtility.isProSkin);
        bool next = GUILayout.Toggle(enabled, $"{ContentWorkbenchEntryLabels.AvailabilityLabel(state)} {count}", style);
        GUI.backgroundColor = previousBackground;
        return next;
    }

    private static GUIStyle AvailabilityFilterStyle(GUIStyle source, ContentWorkbenchAvailabilityState state, bool enabled)
    {
        var style = new GUIStyle(source);
        Color textColor = ContentWorkbenchEntryLabels.AvailabilityFilterTextColor(state, enabled);
        style.normal.textColor = textColor;
        style.hover.textColor = textColor;
        style.active.textColor = textColor;
        style.focused.textColor = textColor;
        style.onNormal.textColor = textColor;
        style.onHover.textColor = textColor;
        style.onActive.textColor = textColor;
        style.onFocused.textColor = textColor;
        return style;
    }

    private static bool DrawValidationFilterToggle(string label, bool enabled, int count, int errors, int warnings)
    {
        GUIStyle style = ValidationFilterStyle(EditorStyles.toolbarButton, errors, warnings, enabled);
        Color previousBackground = GUI.backgroundColor;
        GUI.backgroundColor = ContentWorkbenchEntryLabels.ValidationFilterBackgroundColor(errors, warnings, enabled, EditorGUIUtility.isProSkin);
        bool next = GUILayout.Toggle(enabled, $"{label} {count}", style);
        GUI.backgroundColor = previousBackground;
        return next;
    }

    private static GUIStyle ValidationFilterStyle(GUIStyle source, int errors, int warnings, bool enabled)
    {
        var style = new GUIStyle(source);
        Color textColor = ContentWorkbenchEntryLabels.ValidationFilterTextColor(errors, warnings, enabled);
        style.normal.textColor = textColor;
        style.hover.textColor = textColor;
        style.active.textColor = textColor;
        style.focused.textColor = textColor;
        style.onNormal.textColor = textColor;
        style.onHover.textColor = textColor;
        style.onActive.textColor = textColor;
        style.onFocused.textColor = textColor;
        return style;
    }

    private void DrawHelpLegend()
    {
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            EditorGUILayout.LabelField("Workbench Guide", EditorStyles.boldLabel);
            DrawLegendLine(ContentWorkbenchAvailabilityState.Available, "Available: game can use it.");
            DrawLegendLine(ContentWorkbenchAvailabilityState.Disabled, "Disabled: kept in the catalog, ignored by game.");
            DrawLegendLine(ContentWorkbenchAvailabilityState.Draft, "Draft: asset only, not in runtime catalog.");
            EditorGUILayout.Space(2f);
            DrawValidationLegend(ContentWorkbenchEntryLabels.OkIconName, "Validation OK.");
            DrawValidationLegend(ContentWorkbenchEntryLabels.WarningIconName, "Warnings: usable, but should be reviewed.");
            DrawValidationLegend(ContentWorkbenchEntryLabels.ErrorIconName, "Errors: fix before relying on this content.");
            EditorGUILayout.Space(2f);
            EditorGUILayout.LabelField("Left: choose domain/content. Right: inspect selected content or create new content. Config Hub owns scene rebuild and infrastructure tools.", EditorStyles.wordWrappedMiniLabel);
        }
    }

    private static void DrawLegendLine(ContentWorkbenchAvailabilityState state, string text)
    {
        Color previous = GUI.color;
        GUI.color = ContentWorkbenchEntryLabels.AvailabilityTextColor(state);
        EditorGUILayout.LabelField(text, EditorStyles.miniLabel);
        GUI.color = previous;
    }

    private static void DrawValidationLegend(string iconName, string text)
    {
        GUIContent icon = EditorGUIUtility.IconContent(iconName);
        icon.tooltip = text;
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Label(icon, GUILayout.Width(20f), GUILayout.Height(18f));
            EditorGUILayout.LabelField(text, EditorStyles.miniLabel);
        }
    }

    private void DrawEntryList(IReadOnlyList<ContentWorkbenchEntry> entries)
    {
        EditorGUILayout.LabelField($"{ContentWorkbenchDomainLayout.DisplayName(domain)} ({FilteredCount(entries)} shown)", EditorStyles.boldLabel);
        listScroll = EditorGUILayout.BeginScrollView(listScroll);
        IReadOnlyList<ContentWorkbenchEntryGroup> groups = ContentWorkbenchEntryLayout.BuildGroups(entries, CurrentFilters());
        for (int groupIndex = 0; groupIndex < groups.Count; groupIndex++)
        {
            ContentWorkbenchEntryGroup group = groups[groupIndex];
            DrawEntrySectionHeader(group);
            if (group.Subgroups.Count > 0)
            {
                for (int subgroupIndex = 0; subgroupIndex < group.Subgroups.Count; subgroupIndex++)
                {
                    ContentWorkbenchEntrySubgroup subgroup = group.Subgroups[subgroupIndex];
                    DrawEntrySubsectionHeader(subgroup);
                    for (int entryIndex = 0; entryIndex < subgroup.Entries.Count; entryIndex++)
                    {
                        DrawEntry(subgroup.Entries[entryIndex]);
                    }
                }
            }
            else
            {
                for (int entryIndex = 0; entryIndex < group.Entries.Count; entryIndex++)
                {
                    DrawEntry(group.Entries[entryIndex]);
                }
            }

            EditorGUILayout.Space(3f);
        }

        EditorGUILayout.EndScrollView();
    }

    private static void DrawEntrySectionHeader(ContentWorkbenchEntryGroup group)
    {
        Color previous = GUI.color;
        GUI.color = ContentWorkbenchEntryLabels.AvailabilityTextColor(group.Availability);
        EditorGUILayout.LabelField($"{ContentWorkbenchEntryLabels.AvailabilityLabel(group.Availability)} ({group.Entries.Count})", EditorStyles.miniBoldLabel);
        GUI.color = previous;
    }

    private static void DrawEntrySubsectionHeader(ContentWorkbenchEntrySubgroup subgroup)
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Space(8f);
            EditorGUILayout.LabelField($"{subgroup.Label} ({subgroup.Entries.Count})", EditorStyles.miniBoldLabel);
        }
    }

    private void DrawRightPane(IReadOnlyList<ContentWorkbenchEntry> entries)
    {
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.ExpandHeight(true)))
        {
            DrawDetailModeToolbar();
            EditorGUILayout.Space(4f);
            if (detailMode == ContentWorkbenchDetailMode.CreateContent)
            {
                DrawCreatePanel();
            }
            else
            {
                DrawSelectedDetails(entries);
            }
        }
    }

    private void DrawDetailModeToolbar()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            detailMode = (ContentWorkbenchDetailMode)GUILayout.Toolbar((int)detailMode, DetailModeLabels, GUILayout.Height(26f));
            if (GUILayout.Button("Generate Missing Icons", GUILayout.Width(170f), GUILayout.Height(26f)))
            {
                RunServiceAction(GenerateMissingIcons);
            }

            if (GUILayout.Button("Open Authoring Doctor", GUILayout.Width(170f), GUILayout.Height(26f)))
            {
                TheCircussyOneAuthoringDoctorWindow.Open();
            }
        }
    }

    private void DrawCreatePanel()
    {
        EditorGUILayout.LabelField("Create From Template", EditorStyles.boldLabel);
        createScroll = EditorGUILayout.BeginScrollView(createScroll);
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            DrawCreateTemplateCards();

            createDisplayName = EditorGUILayout.TextField("Name", createDisplayName);
            DrawCreateIdentityPreview();
            addToCatalog = EditorGUILayout.Toggle("Make Available", addToCatalog);

            if (domain == ContentWorkbenchDomain.Performers
                || domain == ContentWorkbenchDomain.Upgrades)
            {
                targetWeapon = DrawWeaponTargetDropdown("Target Weapon", targetWeapon);
            }

            if (domain == ContentWorkbenchDomain.Talents && createTemplate == ContentWorkbenchCreateTemplate.PerformerTalent)
            {
                targetPerformer = DrawPerformerTargetDropdown("Target Performer", targetPerformer);
            }

            if ((domain == ContentWorkbenchDomain.Upgrades && createTemplate == ContentWorkbenchCreateTemplate.WeaponStatUpgrade)
                || domain == ContentWorkbenchDomain.Talents
                || domain == ContentWorkbenchDomain.Items)
            {
                statId = DrawStatPopup(domain, statId, targetWeapon);
                bucket = ContentWorkbenchFieldDrawer.DrawDesignerBucketPopup(statId, bucket, "Bucket");
                value = EditorGUILayout.FloatField("Base Value", value);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Create", GUILayout.Height(28f)))
                {
                    RunServiceAction(RunCreate);
                }
            }

            EditorGUILayout.HelpBox(lastResult, lastResultType);
        }
        EditorGUILayout.EndScrollView();
    }

    private static StatId DrawStatPopup(ContentWorkbenchDomain domain, StatId current, WeaponDefinition targetWeapon = null)
    {
        IReadOnlyList<ContentWorkbenchStatOption> options = domain == ContentWorkbenchDomain.Upgrades && targetWeapon != null
            ? ContentWorkbenchStatOptions.ForWeaponUpgrade(targetWeapon)
            : ContentWorkbenchStatOptions.ForDomain(domain);
        if (options.Count == 0)
        {
            EditorGUILayout.LabelField("Stat", "No valid stats");
            return current;
        }

        current = domain == ContentWorkbenchDomain.Upgrades && targetWeapon != null
            ? ContentWorkbenchStatOptions.EnsureAllowedForWeaponUpgrade(targetWeapon, current)
            : ContentWorkbenchStatOptions.EnsureAllowed(domain, current);
        var labels = new string[options.Count];
        int selectedIndex = 0;
        for (int i = 0; i < options.Count; i++)
        {
            labels[i] = options[i].Label;
            if (options[i].StatId == current)
            {
                selectedIndex = i;
            }
        }

        int nextIndex = EditorGUILayout.Popup("Stat", selectedIndex, labels);
        return options[Mathf.Clamp(nextIndex, 0, options.Count - 1)].StatId;
    }

    private static WeaponDefinition DrawWeaponTargetDropdown(string label, WeaponDefinition current)
    {
        IReadOnlyList<WeaponDefinition> weapons = LoadCatalogWeapons();
        var options = new List<WeaponDefinition>();
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

        if (options.Count == 0)
        {
            EditorGUILayout.LabelField(label, "No weapons in catalog");
            return null;
        }

        string[] labels = new string[options.Count];
        int selectedIndex = 0;
        for (int i = 0; i < options.Count; i++)
        {
            WeaponDefinition weapon = options[i];
            labels[i] = ContentOptionLabel(weapon);
            if (ReferenceEquals(weapon, current))
            {
                selectedIndex = i;
            }
        }

        int nextIndex = EditorGUILayout.Popup(label, selectedIndex, labels);
        return options[Mathf.Clamp(nextIndex, 0, options.Count - 1)];
    }

    private static PerformerDefinition DrawPerformerTargetDropdown(string label, PerformerDefinition current)
    {
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
            EditorGUILayout.LabelField(label, "No performers in catalog");
            return null;
        }

        string[] labels = new string[options.Count];
        int selectedIndex = 0;
        for (int i = 0; i < options.Count; i++)
        {
            PerformerDefinition performer = options[i];
            labels[i] = ContentOptionLabel(performer);
            if (ReferenceEquals(performer, current))
            {
                selectedIndex = i;
            }
        }

        int nextIndex = EditorGUILayout.Popup(label, selectedIndex, labels);
        return options[Mathf.Clamp(nextIndex, 0, options.Count - 1)];
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

    private static IReadOnlyList<WeaponDefinition> LoadCatalogWeapons()
    {
        WeaponCatalog catalog = AssetDatabase.LoadAssetAtPath<WeaponCatalog>(TheCircussyOneAssetPaths.WeaponCatalogPath);
        return catalog?.AvailableWeapons;
    }

    private static IReadOnlyList<PerformerDefinition> LoadCatalogPerformers()
    {
        PerformerCatalog catalog = AssetDatabase.LoadAssetAtPath<PerformerCatalog>(TheCircussyOneAssetPaths.PerformerCatalogPath);
        return catalog?.Performers;
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

    private void DrawCreateTemplateCards()
    {
        IReadOnlyList<ContentWorkbenchCreateTemplateInfo> templates = ContentWorkbenchCreateTemplateInfo.ForDomain(domain);
        if (!ContentWorkbenchService.IsTemplateValidForDomain(domain, createTemplate))
        {
            createTemplate = ContentWorkbenchService.DefaultTemplateFor(domain);
        }

        EditorGUILayout.LabelField("Template", EditorStyles.miniBoldLabel);
        for (int i = 0; i < templates.Count; i++)
        {
            ContentWorkbenchCreateTemplateInfo info = templates[i];
            bool selected = createTemplate == info.Template;
            GUIStyle style = selected ? EditorStyles.toolbarButton : EditorStyles.miniButton;
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (GUILayout.Button(info.Title, style, GUILayout.Height(24f)))
                {
                    createTemplate = info.Template;
                }

                EditorGUILayout.LabelField(info.Description, EditorStyles.wordWrappedMiniLabel);
            }
        }
    }

    private void DrawCreateIdentityPreview()
    {
        ContentWorkbenchIdentityPreview preview = ContentWorkbenchService.PreviewCreate(BuildCreateRequest());
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            EditorGUILayout.LabelField("Generated Identity", EditorStyles.miniBoldLabel);
            EditorGUILayout.LabelField("Name", preview.DisplayName);
            EditorGUILayout.LabelField("ID", preview.Id);
            EditorGUILayout.LabelField("Asset Path", preview.AssetPath, EditorStyles.wordWrappedMiniLabel);
        }
    }

    private void DrawEntry(ContentWorkbenchEntry entry)
    {
        bool selected = entry.Asset == selectedAsset;
        GUIStyle style = AvailabilityRowStyle(selected ? EditorStyles.toolbarButton : EditorStyles.miniButton, entry);
        Color previousBackground = GUI.backgroundColor;
        GUI.backgroundColor = ContentWorkbenchEntryLabels.AvailabilityBackgroundColor(entry, selected, EditorGUIUtility.isProSkin);
        bool clicked;
        using (new EditorGUILayout.HorizontalScope())
        {
            GUIContent validationIcon = EditorGUIUtility.IconContent(ContentWorkbenchEntryLabels.ValidationIconName(entry));
            validationIcon.tooltip = ContentWorkbenchEntryLabels.ValidationTooltip(entry);
            GUILayout.Label(validationIcon, GUILayout.Width(22f), GUILayout.Height(22f));
            clicked = GUILayout.Button(ContentWorkbenchEntryLabels.ListLabel(entry), style, GUILayout.MinHeight(24f));
        }

        GUI.backgroundColor = previousBackground;
        if (clicked)
        {
            SelectAsset(entry.Asset);
            detailMode = ContentWorkbenchDetailMode.SelectedContent;
        }
    }

    private static GUIStyle AvailabilityRowStyle(GUIStyle source, ContentWorkbenchEntry entry)
    {
        var style = new GUIStyle(source);
        Color textColor = ContentWorkbenchEntryLabels.AvailabilityTextColor(entry);
        style.normal.textColor = textColor;
        style.hover.textColor = textColor;
        style.active.textColor = textColor;
        style.focused.textColor = textColor;
        style.onNormal.textColor = textColor;
        style.onHover.textColor = textColor;
        style.onActive.textColor = textColor;
        style.onFocused.textColor = textColor;
        return style;
    }

    private void DrawSelectedDetails(IReadOnlyList<ContentWorkbenchEntry> entries)
    {
        if (selectedAsset == null)
        {
            EditorGUILayout.HelpBox("Select content on the left to inspect and manage it.", MessageType.Info);
            return;
        }

        ContentWorkbenchEntry selectedEntry = FindSelectedEntry(entries);
        DrawSelectedSummary(selectedEntry);
        DrawUsageSummary(ContentWorkbenchUsageSummary.BuildFromProject(domain, selectedAsset));
        DrawAuthoringSummary(ContentWorkbenchAuthoringSummary.Build(selectedAsset));
        DrawSelectedActions(selectedEntry);
        EditorGUILayout.Space(4f);
        EditorGUILayout.HelpBox(lastResult, lastResultType);
        EditorGUILayout.Space(4f);

        detailScroll = EditorGUILayout.BeginScrollView(
            detailScroll,
            false,
            false,
            GUI.skin.horizontalScrollbar,
            GUI.skin.verticalScrollbar,
            EditorStyles.helpBox);
        try
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(InspectorContentWidthForWindow(position.width))))
            {
                DrawSelectedAssetInspector();
            }
        }
        finally
        {
            EditorGUILayout.EndScrollView();
        }
    }

    public static float LeftPaneWidthForWindow(float windowWidth)
    {
        float widthAvailableAfterInspector = windowWidth - MinimumInspectorContentWidth - PaneChromeWidth;
        return Mathf.Clamp(widthAvailableAfterInspector, MinimumLeftPaneWidth, PreferredLeftPaneWidth);
    }

    public static float InspectorContentWidthForWindow(float windowWidth)
    {
        float leftPaneWidth = LeftPaneWidthForWindow(windowWidth);
        return Mathf.Max(MinimumInspectorContentWidth, windowWidth - leftPaneWidth - PaneChromeWidth);
    }

    private void DrawSelectedAssetInspector()
    {
        if (ContentWorkbenchWeaponInspector.CanDraw(selectedAsset) && selectedAsset is WeaponDefinition weapon)
        {
            ContentWorkbenchWeaponInspector.Draw(weapon);
            return;
        }

        if (ContentWorkbenchEnemyInspector.CanDraw(selectedAsset) && selectedAsset is EnemyDefinition enemy)
        {
            ContentWorkbenchEnemyInspector.Draw(enemy);
            return;
        }

        if (ContentWorkbenchPerformerInspector.CanDraw(selectedAsset) && selectedAsset is PerformerDefinition performer)
        {
            ContentWorkbenchPerformerInspector.Draw(performer);
            return;
        }

        if (ContentWorkbenchTalentInspector.CanDraw(selectedAsset) && selectedAsset is TalentDefinition talent)
        {
            ContentWorkbenchTalentInspector.Draw(talent);
            return;
        }

        if (ContentWorkbenchEffectInspector.CanDraw(selectedAsset))
        {
            ContentWorkbenchEffectInspector.Draw(selectedAsset);
            return;
        }

        if (ContentWorkbenchChestInspector.CanDraw(selectedAsset) && selectedAsset is ChestDefinition chest)
        {
            ContentWorkbenchChestInspector.Draw(chest);
            return;
        }

        bool previousWideMode = EditorGUIUtility.wideMode;
        bool previousHierarchyMode = EditorGUIUtility.hierarchyMode;
        float previousLabelWidth = EditorGUIUtility.labelWidth;
        float previousFieldWidth = EditorGUIUtility.fieldWidth;
        try
        {
            EditorGUIUtility.wideMode = true;
            EditorGUIUtility.hierarchyMode = true;
            EditorGUIUtility.labelWidth = 140f;
            EditorGUIUtility.fieldWidth = 90f;
            selectedPropertyTree ??= PropertyTree.Create(selectedAsset);
            selectedPropertyTree.Draw(false);
        }
        finally
        {
            EditorGUIUtility.wideMode = previousWideMode;
            EditorGUIUtility.hierarchyMode = previousHierarchyMode;
            EditorGUIUtility.labelWidth = previousLabelWidth;
            EditorGUIUtility.fieldWidth = previousFieldWidth;
        }
    }

    private void DrawAuthoringSummary(ContentWorkbenchAuthoringSummary summary)
    {
        if (summary == null || !summary.HasHiddenSettings)
        {
            return;
        }

        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            showAuthoringSummary = EditorGUILayout.Foldout(
                showAuthoringSummary,
                $"Active Settings ({summary.HiddenSettings.Count})",
                true);
            if (!showAuthoringSummary)
            {
                return;
            }

            EditorGUILayout.LabelField("Inactive controls are hidden so this panel only shows settings that affect the selected content.", EditorStyles.wordWrappedMiniLabel);
            for (int i = 0; i < summary.HiddenSettings.Count; i++)
            {
                EditorGUILayout.LabelField("- " + summary.HiddenSettings[i], EditorStyles.wordWrappedMiniLabel);
            }
        }
    }

    private void DrawSelectedSummary(ContentWorkbenchEntry selectedEntry)
    {
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            string title = selectedAsset is IContentDefinition definition ? $"{definition.DisplayName} ({definition.Id})" : selectedAsset.name;
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            if (selectedAsset is IContentDefinition contentDefinition)
            {
                EditorGUILayout.LabelField("Computed Tags", ComputedContentTagRules.Format(contentDefinition.Tags), EditorStyles.miniLabel);
            }

            string path = AssetDatabase.GetAssetPath(selectedAsset);
            if (!string.IsNullOrWhiteSpace(path))
            {
                EditorGUILayout.SelectableLabel(path, EditorStyles.miniLabel, GUILayout.Height(18f));
            }

            if (selectedEntry != null)
            {
                EditorGUILayout.HelpBox(selectedEntry.Status, ContentWorkbenchEntryLabels.ValidationMessageType(selectedEntry));
                EditorGUILayout.LabelField(ContentWorkbenchEntryLabels.AvailabilityDescription(selectedEntry), EditorStyles.wordWrappedMiniLabel);
            }
            else
            {
                EditorGUILayout.HelpBox("Selected asset is not in the current filtered domain list. Actions still use the active domain.", MessageType.Warning);
            }
        }
    }

    private void DrawUsageSummary(ContentWorkbenchUsageSummary summary)
    {
        if (summary == null)
        {
            return;
        }

        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            showUsageSummary = EditorGUILayout.Foldout(
                showUsageSummary,
                $"{summary.Title} ({summary.Lines.Count})",
                true);
            if (!showUsageSummary)
            {
                return;
            }

            for (int i = 0; i < summary.Lines.Count; i++)
            {
                EditorGUILayout.LabelField(summary.Lines[i], EditorStyles.wordWrappedMiniLabel);
            }
        }
    }

    private void DrawSelectedActions(ContentWorkbenchEntry selectedEntry)
    {
        bool supportsAvailability = selectedAsset is IActivatableContentDefinition;
        ContentWorkbenchAvailabilityState availability = selectedEntry?.Availability ?? ContentWorkbenchAvailabilityState.Draft;
        bool isAvailable = availability == ContentWorkbenchAvailabilityState.Available;
        bool isDisabled = availability == ContentWorkbenchAvailabilityState.Disabled;
        bool isDraft = availability == ContentWorkbenchAvailabilityState.Draft;
        bool hasSavedAsset = !string.IsNullOrWhiteSpace(AssetDatabase.GetAssetPath(selectedAsset));

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Open/Ping", GUILayout.Width(95f), GUILayout.Height(ActionButtonHeight)))
            {
                EditorGUIUtility.PingObject(selectedAsset);
                Selection.activeObject = selectedAsset;
                AssetDatabase.OpenAsset(selectedAsset);
            }

            using (new EditorGUI.DisabledScope(!supportsAvailability || isAvailable))
            {
                if (GUILayout.Button("Make Available", GUILayout.Width(120f), GUILayout.Height(ActionButtonHeight)))
                {
                    RunServiceAction(() => ContentWorkbenchService.MakeAvailable(domain, selectedAsset));
                }
            }

            using (new EditorGUI.DisabledScope(!supportsAvailability || isDisabled || isDraft))
            {
                if (GUILayout.Button("Disable", GUILayout.Width(90f), GUILayout.Height(ActionButtonHeight)))
                {
                    RunServiceAction(() => ContentWorkbenchService.Disable(selectedAsset));
                }
            }

            using (new EditorGUI.DisabledScope(isDraft))
            {
                if (GUILayout.Button("Move To Draft", GUILayout.Width(120f), GUILayout.Height(ActionButtonHeight)))
                {
                    RunServiceAction(() => ContentWorkbenchService.MoveToDraft(domain, selectedAsset));
                }
            }
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            using (new EditorGUI.DisabledScope(selectedAsset is not IContentDefinition))
            {
                if (GUILayout.Button("Generate Icon", GUILayout.Width(110f), GUILayout.Height(ActionButtonHeight)))
                {
                    RunServiceAction(GenerateSelectedIcon);
                }
            }

            using (new EditorGUI.DisabledScope(!hasSavedAsset || selectedAsset is not IContentDefinition))
            {
                if (GUILayout.Button("Duplicate Active Copy", GUILayout.Height(ActionButtonHeight)))
                {
                    RunServiceAction(() => ContentWorkbenchService.Duplicate(domain, selectedAsset, null, null));
                }
            }

            using (new EditorGUI.DisabledScope(!hasSavedAsset || selectedAsset is not IContentDefinition || isAvailable))
            {
                if (GUILayout.Button("Delete...", GUILayout.Width(100f), GUILayout.Height(ActionButtonHeight)))
                {
                    RunServiceAction(RunDeleteSelected);
                }
            }
        }
    }

    private static ContentWorkbenchActionResult GenerateMissingIcons()
    {
        int assigned = TheCircussyOneContentIconGenerator.GenerateMissingForAllContent();
        return ContentWorkbenchActionResult.Ok(assigned == 1
            ? "Generated or assigned 1 missing icon."
            : $"Generated or assigned {assigned} missing icons.");
    }

    private ContentWorkbenchActionResult GenerateSelectedIcon()
    {
        if (selectedAsset is not IContentDefinition)
        {
            return ContentWorkbenchActionResult.Fail("Select content first.");
        }

        int assigned = TheCircussyOneContentIconGenerator.GenerateMissingForSelected(selectedAsset);
        return ContentWorkbenchActionResult.Ok(assigned == 1
            ? "Generated or assigned 1 icon."
            : "Selected content already has an icon or uses a fallback icon.", selectedAsset);
    }

    private ContentWorkbenchActionResult RunCreate()
    {
        return ContentWorkbenchService.CreateFromTemplate(BuildCreateRequest());
    }

    private ContentWorkbenchActionResult RunDeleteSelected()
    {
        ContentWorkbenchDeletePreview preview = ContentWorkbenchService.PreviewDelete(domain, selectedAsset);
        if (!preview.CanDelete)
        {
            return ContentWorkbenchActionResult.Fail(preview.BlockerMessage);
        }

        if (!EditorUtility.DisplayDialog(preview.Title, preview.ConfirmationMessage, "Delete", "Cancel"))
        {
            return ContentWorkbenchActionResult.Ok("Delete cancelled.", selectedAsset);
        }

        ContentWorkbenchActionResult result = ContentWorkbenchService.DeleteContent(domain, selectedAsset);
        if (result.Success)
        {
            SelectAsset(null);
        }

        return result;
    }

    private ContentWorkbenchCreateRequest BuildCreateRequest()
    {
        var request = new ContentWorkbenchCreateRequest
        {
            Domain = domain,
            Template = createTemplate,
            DisplayName = createDisplayName,
            AddToCatalog = addToCatalog,
            TargetWeapon = targetWeapon,
            TargetPerformer = targetPerformer,
            StatId = statId,
            Bucket = bucket,
            Value = value
        };

        return request;
    }

    private void RunServiceAction(Func<ContentWorkbenchActionResult> action)
    {
        GUI.FocusControl(null);
        try
        {
            RunAction(action());
        }
        catch (Exception exception)
        {
            lastResult = exception.Message;
            lastResultType = MessageType.Error;
            Debug.LogException(exception);
            Repaint();
        }
    }

    private void RunAction(ContentWorkbenchActionResult result)
    {
        lastResult = result.Message;
        lastResultType = result.Success ? MessageType.Info : MessageType.Warning;
        if (result.Asset != null)
        {
            SelectAsset(result.Asset);
            EditorGUIUtility.PingObject(result.Asset);
        }

        Repaint();
    }

    private void SelectAsset(UnityEngine.Object asset)
    {
        if (selectedAsset == asset)
        {
            return;
        }

        selectedAsset = asset;
        DestroySelectedPropertyTree();
    }

    private void ChangeDomain(ContentWorkbenchDomain nextDomain)
    {
        if (domain == nextDomain)
        {
            return;
        }

        domain = nextDomain;
        createDisplayName = string.Empty;
        if (!ContentWorkbenchService.IsTemplateValidForDomain(domain, createTemplate))
        {
            createTemplate = ContentWorkbenchService.DefaultTemplateFor(domain);
        }

        statId = ContentWorkbenchStatOptions.EnsureAllowed(domain, statId);

        selectedAsset = null;
        listScroll = Vector2.zero;
        detailScroll = Vector2.zero;
        DestroySelectedPropertyTree();
    }

    private void DestroySelectedPropertyTree()
    {
        if (selectedPropertyTree != null)
        {
            selectedPropertyTree.Dispose();
            selectedPropertyTree = null;
        }
    }

    private int FilteredCount(IReadOnlyList<ContentWorkbenchEntry> entries)
    {
        return CurrentFilters().CountVisible(entries);
    }

    private bool PassesFilters(ContentWorkbenchEntry entry)
    {
        return CurrentFilters().Passes(entry);
    }

    private ContentWorkbenchEntry FindSelectedEntry(IReadOnlyList<ContentWorkbenchEntry> entries)
    {
        if (selectedAsset == null || entries == null)
        {
            return null;
        }

        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i]?.Asset == selectedAsset)
            {
                return entries[i];
            }
        }

        return null;
    }

    private ContentWorkbenchFilterOptions CurrentFilters()
    {
        return new ContentWorkbenchFilterOptions(showAvailable, showDisabled, showDraft, search, showOk, showWarnings, showErrors);
    }

    private static void EnsureCoreContentAssets()
    {
        TheCircussyOneConfigRepository.GetOrCreateWeaponCatalog();
        TheCircussyOneConfigRepository.GetOrCreateItemCatalog();
        TheCircussyOneConfigRepository.GetOrCreatePerformerCatalog();
        TheCircussyOneConfigRepository.GetOrCreateEnemyCatalog();
        TheCircussyOneConfigRepository.GetOrCreateHeadlinerCatalog();
        TheCircussyOneConfigRepository.GetOrCreateUpgradeCatalog();
        TheCircussyOneConfigRepository.GetOrCreateTalentCatalog();
    }
}
