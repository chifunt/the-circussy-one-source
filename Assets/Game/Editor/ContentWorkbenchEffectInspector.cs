using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using TheCircussyOne.Stats;
using UnityEditor;
using UnityEngine;

public static class ContentWorkbenchEffectInspector
{
    private static readonly string[] UpgradeTabLabels =
    {
        "Identity",
        "Progression",
        "Effect",
        "Visual",
        "Diagnostics"
    };

    private static readonly string[] ItemTabLabels =
    {
        "Identity",
        "Stacking",
        "Effects",
        "Visual",
        "Diagnostics"
    };

    private static ContentWorkbenchUpgradeTab selectedUpgradeTab = ContentWorkbenchUpgradeTab.Identity;
    private static ContentWorkbenchItemTab selectedItemTab = ContentWorkbenchItemTab.Identity;

    public static bool CanDraw(Object asset)
    {
        return asset is UpgradeDefinition or ItemDefinition;
    }

    public static void Draw(Object asset)
    {
        switch (asset)
        {
            case UpgradeDefinition upgrade:
                DrawUpgrade(upgrade);
                break;
            case ItemDefinition item:
                DrawItem(item);
                break;
        }
    }

    private static void DrawUpgrade(UpgradeDefinition upgrade)
    {
        var serialized = new SerializedObject(upgrade);
        serialized.Update();

        DrawHeader("Weapon Upgrade Authoring", upgrade);
        DrawUpgradeTabs();
        switch (selectedUpgradeTab)
        {
            case ContentWorkbenchUpgradeTab.Identity:
                DrawUpgradeIdentity(serialized, upgrade);
                break;
            case ContentWorkbenchUpgradeTab.Progression:
                DrawUpgradeProgression(serialized);
                break;
            case ContentWorkbenchUpgradeTab.Effect:
                DrawUpgradeEffect(serialized);
                break;
            case ContentWorkbenchUpgradeTab.Visual:
                DrawUpgradeVisual(serialized);
                break;
            case ContentWorkbenchUpgradeTab.Diagnostics:
                DrawUpgradeDiagnostics(upgrade);
                break;
        }

        if (serialized.ApplyModifiedProperties())
        {
            EditorUtility.SetDirty(upgrade);
        }
    }

    private static void DrawItem(ItemDefinition item)
    {
        var serialized = new SerializedObject(item);
        serialized.Update();

        DrawHeader("Item Authoring", item);
        DrawItemTabs();
        switch (selectedItemTab)
        {
            case ContentWorkbenchItemTab.Identity:
                DrawItemIdentity(serialized, item);
                break;
            case ContentWorkbenchItemTab.Stacking:
                DrawItemStacking(serialized, item);
                break;
            case ContentWorkbenchItemTab.Effects:
                DrawItemEffects(serialized);
                break;
            case ContentWorkbenchItemTab.Visual:
                DrawItemVisual(serialized);
                break;
            case ContentWorkbenchItemTab.Diagnostics:
                DrawItemDiagnostics(item);
                break;
        }

        if (serialized.ApplyModifiedProperties())
        {
            EditorUtility.SetDirty(item);
        }
    }

    public static int UpgradeTabCount => UpgradeTabLabels.Length;
    public static int ItemTabCount => ItemTabLabels.Length;

    public static string UpgradeTabLabel(int index)
    {
        return UpgradeTabLabels[Mathf.Clamp(index, 0, UpgradeTabLabels.Length - 1)];
    }

    public static string ItemTabLabel(int index)
    {
        return ItemTabLabels[Mathf.Clamp(index, 0, ItemTabLabels.Length - 1)];
    }

    private static void DrawHeader(string title, Object asset)
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            if (GUILayout.Button("Open Odin Inspector", GUILayout.Width(150f), GUILayout.Height(22f)))
            {
                OdinEditorWindow.InspectObject(asset);
            }
        }

        EditorGUILayout.LabelField("Workbench uses focused effect rows here so unit labels stay readable and values stay editable. Use Odin Inspector for the full advanced asset view.", EditorStyles.wordWrappedMiniLabel);
    }

    private static void DrawUpgradeTabs()
    {
        EditorGUILayout.Space(4f);
        selectedUpgradeTab = (ContentWorkbenchUpgradeTab)GUILayout.Toolbar((int)selectedUpgradeTab, UpgradeTabLabels, GUILayout.MinHeight(24f));
        EditorGUILayout.Space(4f);
    }

    private static void DrawItemTabs()
    {
        EditorGUILayout.Space(4f);
        selectedItemTab = (ContentWorkbenchItemTab)GUILayout.Toolbar((int)selectedItemTab, ItemTabLabels, GUILayout.MinHeight(24f));
        EditorGUILayout.Space(4f);
    }

    private static void DrawUpgradeIdentity(SerializedObject serialized, UpgradeDefinition upgrade)
    {
        ContentWorkbenchFieldDrawer.DrawSection("Identity");
        ContentWorkbenchFieldDrawer.DrawReadOnlyProperty(serialized, nameof(UpgradeDefinition.upgradeId), "Upgrade ID");
        ContentWorkbenchFieldDrawer.DrawProperty(serialized, nameof(UpgradeDefinition.displayName), "Display Name");
        ContentWorkbenchFieldDrawer.DrawIdNameSync(
            serialized.FindProperty(nameof(UpgradeDefinition.upgradeId)),
            serialized.FindProperty(nameof(UpgradeDefinition.displayName))?.stringValue,
            "upgrade",
            "Upgrade ID");
        ContentWorkbenchFieldDrawer.DrawProperty(serialized, nameof(UpgradeDefinition.shortDescription), "Short Description", includeChildren: true);
        ContentWorkbenchFieldDrawer.DrawRaritySet(serialized.FindProperty(nameof(UpgradeDefinition.possibleRarities)));
        EditorGUILayout.LabelField("Computed Tags", ComputedContentTagRules.Format(upgrade.Tags), EditorStyles.miniLabel);
    }

    private static void DrawUpgradeProgression(SerializedObject serialized)
    {
        ContentWorkbenchFieldDrawer.DrawSection("Progression");
        ContentWorkbenchFieldDrawer.DrawProperty(serialized, nameof(UpgradeDefinition.maxLevel), "Future Max Level");
        EditorGUILayout.LabelField("Max level is retained for future capped upgrade flows. Current level-up selection does not cap these lines.", EditorStyles.wordWrappedMiniLabel);
    }

    private static void DrawUpgradeEffect(SerializedObject serialized)
    {
        WeaponDefinition targetWeapon = DrawUpgradeWeaponTarget(serialized);
        DrawUpgradeEffects(serialized, targetWeapon);
    }

    private static WeaponDefinition DrawUpgradeWeaponTarget(SerializedObject serialized)
    {
        ContentWorkbenchFieldDrawer.DrawSection("Target Weapon");
        SerializedProperty weaponId = serialized.FindProperty(nameof(UpgradeDefinition.weaponId));
        SerializedProperty weaponDefinition = serialized.FindProperty(nameof(UpgradeDefinition.weaponDefinition));
        WeaponDefinition current = weaponDefinition?.objectReferenceValue as WeaponDefinition;
        WeaponDefinition selected = DrawWeaponTargetDropdown("Target Weapon", current, weaponId?.stringValue, out bool changed);
        if (changed && selected != null)
        {
            if (weaponDefinition != null)
            {
                weaponDefinition.objectReferenceValue = selected;
            }

            if (weaponId != null)
            {
                weaponId.stringValue = selected.Id;
            }
        }

        return selected;
    }

    private static void DrawUpgradeEffects(SerializedObject serialized, WeaponDefinition targetWeapon)
    {
        ContentWorkbenchFieldDrawer.DrawSection("Effect");
        EditorGUILayout.HelpBox(
            "Weapon damage upgrades add flat damage to this weapon. Performer and Talent Damage is a separate global percent multiplier. Damage order: weapon base + flat weapon damage, then global damage percent.",
            MessageType.Info);
        DrawUpgradeModifierList(
            serialized.FindProperty(nameof(UpgradeDefinition.statModifiers)),
            ContentWorkbenchDomain.Upgrades,
            ContentWorkbenchStatOptions.ForWeaponUpgrade(targetWeapon),
            targetWeapon);
    }

    private static void DrawUpgradeVisual(SerializedObject serialized)
    {
        ContentWorkbenchFieldDrawer.DrawSection("Visual");
        ContentWorkbenchFieldDrawer.DrawIconSpriteProperty(
            serialized,
            nameof(UpgradeDefinition.iconSpriteOverride),
            "Override Icon",
            "No override assigned. Weapon upgrades use the target weapon icon by default.");
        ContentWorkbenchFieldDrawer.DrawProperty(serialized, nameof(UpgradeDefinition.iconColor), "Fallback Color");
    }

    private static void DrawUpgradeDiagnostics(UpgradeDefinition upgrade)
    {
        ContentWorkbenchFieldDrawer.DrawSection("Diagnostics");
        EditorGUILayout.LabelField("Runtime Summary", $"{upgrade.DisplayName} [{upgrade.LevelUpRarities}] {upgrade.shortDescription}", EditorStyles.wordWrappedLabel);
    }

    private static void DrawItemIdentity(SerializedObject serialized, ItemDefinition item)
    {
        ContentWorkbenchFieldDrawer.DrawSection("Identity");
        ContentWorkbenchFieldDrawer.DrawReadOnlyProperty(serialized, nameof(ItemDefinition.itemId), "Item ID");
        ContentWorkbenchFieldDrawer.DrawProperty(serialized, nameof(ItemDefinition.displayName), "Display Name");
        ContentWorkbenchFieldDrawer.DrawIdNameSync(
            serialized.FindProperty(nameof(ItemDefinition.itemId)),
            serialized.FindProperty(nameof(ItemDefinition.displayName))?.stringValue,
            "item",
            "Item ID");
        ContentWorkbenchFieldDrawer.DrawProperty(serialized, nameof(ItemDefinition.shortDescription), "Short Description", includeChildren: true);
        ContentWorkbenchFieldDrawer.DrawProperty(serialized, nameof(ItemDefinition.rarity), "Rarity");
        EditorGUILayout.LabelField("Computed Tags", ComputedContentTagRules.Format(item.Tags), EditorStyles.miniLabel);
    }

    private static void DrawItemStacking(SerializedObject serialized, ItemDefinition item)
    {
        ContentWorkbenchFieldDrawer.DrawSection("Stacking");
        ContentWorkbenchFieldDrawer.DrawProperty(serialized, nameof(ItemDefinition.stackPolicy), "Stack Policy");
        EditorGUILayout.LabelField("Policy", ItemStackRules.StackSummary(item), EditorStyles.wordWrappedMiniLabel);
        if (ItemStackRules.UsesMaxStacks(item.stackPolicy))
        {
            ContentWorkbenchFieldDrawer.DrawProperty(serialized, nameof(ItemDefinition.maxStacks), "Max Stacks");
        }

        if (ItemStackRules.UsesEffectCap(item.stackPolicy))
        {
            ContentWorkbenchFieldDrawer.DrawProperty(serialized, nameof(ItemDefinition.effectCapStacks), "Effect Cap Stacks");
        }

        if (ItemStackRules.UsesDiminishingFalloff(item.stackPolicy))
        {
            ContentWorkbenchFieldDrawer.DrawProperty(serialized, nameof(ItemDefinition.diminishingFalloff), "Diminishing Falloff");
        }
    }

    private static void DrawItemEffects(SerializedObject serialized)
    {
        ContentWorkbenchFieldDrawer.DrawSection("Effects");
        DrawItemModifierList(serialized.FindProperty(nameof(ItemDefinition.statModifiers)), "Upside Modifiers");
        DrawItemModifierList(serialized.FindProperty(nameof(ItemDefinition.downsideStatModifiers)), "Downside Modifiers");
    }

    private static void DrawItemVisual(SerializedObject serialized)
    {
        ContentWorkbenchFieldDrawer.DrawSection("Visual");
        ContentWorkbenchFieldDrawer.DrawIconSpriteProperty(
            serialized,
            nameof(ItemDefinition.iconSprite),
            "Item Icon",
            "No icon assigned. Item cards and HUD stack slots use fallback color until a placeholder or final sprite is assigned.");
        ContentWorkbenchFieldDrawer.DrawProperty(serialized, nameof(ItemDefinition.iconColor), "Fallback Color");
    }

    private static void DrawItemDiagnostics(ItemDefinition item)
    {
        ContentWorkbenchFieldDrawer.DrawSection("Diagnostics");
        EditorGUILayout.LabelField("Runtime Summary", $"{item.DisplayName} [{item.rarity}] {item.StackSummary}: {item.shortDescription}", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("Risk Item", item.HasDownside ? "Yes" : "No", EditorStyles.miniLabel);
    }

    public static void DrawUpgradeModifierList(SerializedProperty list, ContentWorkbenchDomain statDomain)
    {
        DrawUpgradeModifierList(list, statDomain, null, null);
    }

    public static void DrawUpgradeModifierList(
        SerializedProperty list,
        ContentWorkbenchDomain statDomain,
        IReadOnlyList<ContentWorkbenchStatOption> statOptions)
    {
        DrawUpgradeModifierList(list, statDomain, statOptions, null);
    }

    public static void DrawUpgradeModifierList(
        SerializedProperty list,
        ContentWorkbenchDomain statDomain,
        IReadOnlyList<ContentWorkbenchStatOption> statOptions,
        WeaponDefinition targetWeapon)
    {
        if (list == null || !list.isArray)
        {
            EditorGUILayout.HelpBox("Missing stat modifier list.", MessageType.Warning);
            return;
        }

        DrawModifierListToolbar(list, "Stat Modifiers", statDomain, statOptions);
        for (int i = 0; i < list.arraySize; i++)
        {
            SerializedProperty element = list.GetArrayElementAtIndex(i);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField($"Modifier {i + 1}", EditorStyles.miniBoldLabel);
                    if (GUILayout.Button("Remove", GUILayout.Width(82f), GUILayout.Height(20f)))
                    {
                        list.DeleteArrayElementAtIndex(i);
                        return;
                    }
                }

                DrawUpgradeModifier(element, statDomain, statOptions, targetWeapon);
            }
        }
    }

    private static void DrawItemModifierList(SerializedProperty list, string title)
    {
        if (list == null || !list.isArray)
        {
            EditorGUILayout.HelpBox("Missing stat modifier list.", MessageType.Warning);
            return;
        }

        DrawModifierListToolbar(list, title, ContentWorkbenchDomain.Items, null);
        for (int i = 0; i < list.arraySize; i++)
        {
            SerializedProperty element = list.GetArrayElementAtIndex(i);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField($"Modifier {i + 1}", EditorStyles.miniBoldLabel);
                    if (GUILayout.Button("Remove", GUILayout.Width(82f), GUILayout.Height(20f)))
                    {
                        list.DeleteArrayElementAtIndex(i);
                        return;
                    }
                }

                DrawItemModifier(element);
            }
        }
    }

    private static void DrawModifierListToolbar(
        SerializedProperty list,
        string title,
        ContentWorkbenchDomain statDomain,
        IReadOnlyList<ContentWorkbenchStatOption> statOptions)
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField($"{title} ({list.arraySize})", EditorStyles.miniBoldLabel);
            if (GUILayout.Button("Add", GUILayout.Width(82f), GUILayout.Height(22f)))
            {
                int index = list.arraySize;
                list.InsertArrayElementAtIndex(index);
                InitializeModifier(list.GetArrayElementAtIndex(index), statDomain, statOptions);
            }
        }
    }

    private static void DrawUpgradeModifier(
        SerializedProperty element,
        ContentWorkbenchDomain statDomain,
        IReadOnlyList<ContentWorkbenchStatOption> statOptions,
        WeaponDefinition targetWeapon)
    {
        SerializedProperty statId = element.FindPropertyRelative(nameof(UpgradeStatModifierDefinition.statId));
        SerializedProperty bucket = element.FindPropertyRelative(nameof(UpgradeStatModifierDefinition.bucket));
        SerializedProperty value = element.FindPropertyRelative(nameof(UpgradeStatModifierDefinition.value));
        SerializedProperty useExplicit = element.FindPropertyRelative(nameof(UpgradeStatModifierDefinition.useExplicitRarityValues));

        StatId selectedStat = statOptions != null
            ? ContentWorkbenchFieldDrawer.DrawStatPopup(statOptions, statId, "Stat")
            : ContentWorkbenchFieldDrawer.DrawStatPopup(statDomain, statId, "Stat");
        selectedStat = DrawUnsupportedStatFix(statId, bucket, selectedStat, statDomain, statOptions, targetWeapon);
        StatModifierBucket selectedBucket = ContentWorkbenchFieldDrawer.DrawDesignerBucketPopup(bucket, selectedStat, "Bucket");
        selectedBucket = DrawUnsupportedBucketFix(bucket, selectedStat, selectedBucket);
        ContentWorkbenchFieldDrawer.DrawModifierValueProperty(value, selectedStat, selectedBucket, "Common Value");
        EditorGUILayout.LabelField("Common Preview", FormatModifierPreview(selectedStat, selectedBucket, value.floatValue), EditorStyles.miniLabel);
        DrawIntegerModifierGuidance(selectedStat, selectedBucket, value.floatValue);

        EditorGUILayout.PropertyField(useExplicit, new GUIContent("Use Explicit Rarity Values"));
        if (useExplicit.boolValue)
        {
            DrawRarityValue(element, nameof(UpgradeStatModifierDefinition.uncommonValue), "Uncommon Value", selectedStat, selectedBucket);
            DrawRarityValue(element, nameof(UpgradeStatModifierDefinition.rareValue), "Rare Value", selectedStat, selectedBucket);
            DrawRarityValue(element, nameof(UpgradeStatModifierDefinition.epicValue), "Epic Value", selectedStat, selectedBucket);
            DrawRarityValue(element, nameof(UpgradeStatModifierDefinition.legendaryValue), "Legendary Value", selectedStat, selectedBucket);
        }

        EditorGUILayout.LabelField("Rarity Preview", UpgradeRarityPreview(element), EditorStyles.wordWrappedMiniLabel);
    }

    private static void DrawItemModifier(SerializedProperty element)
    {
        SerializedProperty statId = element.FindPropertyRelative(nameof(ItemStatModifierDefinition.statId));
        SerializedProperty bucket = element.FindPropertyRelative(nameof(ItemStatModifierDefinition.bucket));
        SerializedProperty value = element.FindPropertyRelative(nameof(ItemStatModifierDefinition.value));

        StatId selectedStat = ContentWorkbenchFieldDrawer.DrawStatPopup(ContentWorkbenchDomain.Items, statId, "Stat");
        selectedStat = DrawUnsupportedStatFix(statId, bucket, selectedStat, ContentWorkbenchDomain.Items, null, null);
        StatModifierBucket selectedBucket = ContentWorkbenchFieldDrawer.DrawDesignerBucketPopup(bucket, selectedStat, "Bucket");
        selectedBucket = DrawUnsupportedBucketFix(bucket, selectedStat, selectedBucket);
        ContentWorkbenchFieldDrawer.DrawModifierValueProperty(value, selectedStat, selectedBucket, "Value");
        EditorGUILayout.LabelField("Preview", FormatModifierPreview(selectedStat, selectedBucket, value.floatValue), EditorStyles.miniLabel);
        DrawIntegerModifierGuidance(selectedStat, selectedBucket, value.floatValue);
    }

    private static StatId DrawUnsupportedStatFix(
        SerializedProperty statId,
        SerializedProperty bucket,
        StatId selectedStat,
        ContentWorkbenchDomain statDomain,
        IReadOnlyList<ContentWorkbenchStatOption> statOptions,
        WeaponDefinition targetWeapon)
    {
        bool allowed = statOptions != null
            ? ContentWorkbenchStatOptions.Contains(statOptions, selectedStat)
            : ContentWorkbenchStatOptions.IsAllowed(statDomain, selectedStat);
        if (allowed)
        {
            return selectedStat;
        }

        EditorGUILayout.HelpBox(ContentWorkbenchStatOptions.UnsupportedStatMessage(statDomain, selectedStat, targetWeapon), MessageType.Warning);
        string buttonLabel = statOptions != null ? "Use First Supported Stat" : "Use Valid Stat";
        if (!GUILayout.Button(buttonLabel, GUILayout.Height(22f)))
        {
            return selectedStat;
        }

        StatId replacement = statOptions != null
            ? ContentWorkbenchStatOptions.FirstOrFallback(statOptions, selectedStat)
            : ContentWorkbenchStatOptions.EnsureAllowed(statDomain, selectedStat);
        if (statId != null)
        {
            statId.intValue = (int)replacement;
        }

        if (bucket != null)
        {
            bucket.intValue = (int)ContentWorkbenchFieldDrawer.DefaultBucketForStat(replacement);
        }

        return replacement;
    }

    private static StatModifierBucket DrawUnsupportedBucketFix(
        SerializedProperty bucket,
        StatId selectedStat,
        StatModifierBucket selectedBucket)
    {
        if (ContentWorkbenchFieldDrawer.IsValidDesignerBucket(selectedStat, selectedBucket))
        {
            return selectedBucket;
        }

        EditorGUILayout.HelpBox(ContentWorkbenchFieldDrawer.InvalidBucketMessage(selectedStat, selectedBucket), MessageType.Warning);
        if (!GUILayout.Button("Use Valid Bucket", GUILayout.Height(22f)))
        {
            return selectedBucket;
        }

        StatModifierBucket replacement = ContentWorkbenchFieldDrawer.DefaultBucketForStat(selectedStat);
        if (bucket != null)
        {
            bucket.intValue = (int)replacement;
        }

        return replacement;
    }

    private static void DrawRarityValue(SerializedProperty element, string propertyName, string label, StatId statId, StatModifierBucket bucket)
    {
        SerializedProperty value = element.FindPropertyRelative(propertyName);
        if (value != null)
        {
            ContentWorkbenchFieldDrawer.DrawModifierValueProperty(value, statId, bucket, label);
        }
    }

    private static string UpgradeRarityPreview(SerializedProperty element)
    {
        UpgradeStatModifierDefinition modifier = ReadUpgradeModifier(element);
        return ContentModifierDisplayRules.RarityPreview(modifier);
    }

    private static string FormatModifierPreview(StatId statId, StatModifierBucket bucket, float value)
    {
        if (!System.Enum.IsDefined(typeof(StatId), statId) || !System.Enum.IsDefined(typeof(StatModifierBucket), bucket))
        {
            return "Invalid stat modifier.";
        }

        StatDefinition stat = StatMetadata.Get(statId);
        return $"{stat.DisplayName} {StatDisplayRules.FormatModifier(stat, bucket, value)}";
    }

    private static void DrawIntegerModifierGuidance(StatId statId, StatModifierBucket bucket, float commonValue)
    {
        if (!ContentWorkbenchFieldDrawer.UsesFlooredIntegerApplication(statId, bucket))
        {
            return;
        }

        EditorGUILayout.HelpBox(ContentWorkbenchFieldDrawer.IntegerModifierFlooringHelp, MessageType.Info);
        string warning = ContentWorkbenchFieldDrawer.IntegerCommonValueWarning(statId, bucket, commonValue);
        if (!string.IsNullOrWhiteSpace(warning))
        {
            EditorGUILayout.HelpBox(warning, MessageType.Warning);
        }
    }

    private static UpgradeStatModifierDefinition ReadUpgradeModifier(SerializedProperty element)
    {
        return new UpgradeStatModifierDefinition(
            (StatId)element.FindPropertyRelative(nameof(UpgradeStatModifierDefinition.statId)).intValue,
            (StatModifierBucket)element.FindPropertyRelative(nameof(UpgradeStatModifierDefinition.bucket)).intValue,
            element.FindPropertyRelative(nameof(UpgradeStatModifierDefinition.value)).floatValue,
            element.FindPropertyRelative(nameof(UpgradeStatModifierDefinition.uncommonValue)).floatValue,
            element.FindPropertyRelative(nameof(UpgradeStatModifierDefinition.rareValue)).floatValue,
            element.FindPropertyRelative(nameof(UpgradeStatModifierDefinition.epicValue)).floatValue,
            element.FindPropertyRelative(nameof(UpgradeStatModifierDefinition.legendaryValue)).floatValue)
        {
            useExplicitRarityValues = element.FindPropertyRelative(nameof(UpgradeStatModifierDefinition.useExplicitRarityValues)).boolValue
        };
    }

    private static void InitializeModifier(
        SerializedProperty element,
        ContentWorkbenchDomain statDomain,
        IReadOnlyList<ContentWorkbenchStatOption> statOptions)
    {
        SerializedProperty statId = element.FindPropertyRelative(nameof(UpgradeStatModifierDefinition.statId))
            ?? element.FindPropertyRelative(nameof(ItemStatModifierDefinition.statId));
        SerializedProperty bucket = element.FindPropertyRelative(nameof(UpgradeStatModifierDefinition.bucket))
            ?? element.FindPropertyRelative(nameof(ItemStatModifierDefinition.bucket));
        SerializedProperty value = element.FindPropertyRelative(nameof(UpgradeStatModifierDefinition.value))
            ?? element.FindPropertyRelative(nameof(ItemStatModifierDefinition.value));
        SerializedProperty useExplicit = element.FindPropertyRelative(nameof(UpgradeStatModifierDefinition.useExplicitRarityValues));

        if (statId != null)
        {
            StatId fallback = statOptions != null && statOptions.Count > 0
                ? statOptions[0].StatId
                : ContentWorkbenchStatOptions.EnsureAllowed(statDomain, StatId.PlayerMaxHealth);
            statId.intValue = (int)fallback;
        }

        if (bucket != null)
        {
            StatId selectedStat = statId != null ? (StatId)statId.intValue : StatId.PlayerMaxHealth;
            bucket.intValue = (int)ContentWorkbenchFieldDrawer.DefaultBucketForStat(selectedStat);
        }

        if (value != null)
        {
            value.floatValue = 1f;
        }

        if (useExplicit != null)
        {
            useExplicit.boolValue = false;
            element.FindPropertyRelative(nameof(UpgradeStatModifierDefinition.uncommonValue)).floatValue = 0f;
            element.FindPropertyRelative(nameof(UpgradeStatModifierDefinition.rareValue)).floatValue = 0f;
            element.FindPropertyRelative(nameof(UpgradeStatModifierDefinition.epicValue)).floatValue = 0f;
            element.FindPropertyRelative(nameof(UpgradeStatModifierDefinition.legendaryValue)).floatValue = 0f;
        }
    }

    private static WeaponDefinition DrawWeaponTargetDropdown(string label, WeaponDefinition current, string currentId, out bool changed)
    {
        changed = false;
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
            EditorGUILayout.LabelField(label, string.IsNullOrWhiteSpace(currentId) ? "No weapons in catalog" : currentId);
            return current;
        }

        int selectedIndex = -1;
        for (int i = 0; i < options.Count; i++)
        {
            WeaponDefinition weapon = options[i];
            if (ReferenceEquals(weapon, current) || (!string.IsNullOrWhiteSpace(currentId) && weapon.Id == currentId))
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

public enum ContentWorkbenchUpgradeTab
{
    Identity = 0,
    Progression = 1,
    Effect = 2,
    Visual = 3,
    Diagnostics = 4
}

public enum ContentWorkbenchItemTab
{
    Identity = 0,
    Stacking = 1,
    Effects = 2,
    Visual = 3,
    Diagnostics = 4
}
