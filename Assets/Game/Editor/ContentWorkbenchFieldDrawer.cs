using System.Collections.Generic;
using TheCircussyOne.Content;
using TheCircussyOne.Stats;
using UnityEditor;
using UnityEngine;

public static class ContentWorkbenchFieldDrawer
{
    public const string IntegerModifierFlooringHelp = "Decimals can build toward the next whole point. Final runtime values are floored: +0.75 Weapon Damage displays as +0.75, but applies as +0 until the total reaches +1.";

    private const float LabelWidth = 180f;
    private const float UnitWidth = 72f;
    private const float RowGap = 6f;

    private enum DesignerBucketChoice
    {
        Flat,
        AdditivePercent
    }

    public static void DrawSection(string title)
    {
        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        Rect rect = EditorGUILayout.GetControlRect(false, 1f);
        EditorGUI.DrawRect(rect, new Color(1f, 1f, 1f, 0.12f));
        EditorGUILayout.Space(2f);
    }

    public static void DrawSubsection(string title)
    {
        EditorGUILayout.Space(4f);
        EditorGUILayout.LabelField(title, EditorStyles.miniBoldLabel);
    }

    public static void DrawProperty(SerializedObject serialized, string propertyName, string label, bool includeChildren = false)
    {
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property == null)
        {
            EditorGUILayout.HelpBox($"Missing serialized field: {propertyName}", MessageType.Warning);
            return;
        }

        EditorGUILayout.PropertyField(property, new GUIContent(label), includeChildren);
    }

    public static void DrawIconSpriteProperty(SerializedObject serialized, string propertyName, string label, string fallbackDescription)
    {
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property == null)
        {
            EditorGUILayout.HelpBox($"Missing serialized field: {propertyName}", MessageType.Warning);
            return;
        }

        EditorGUILayout.PropertyField(property, new GUIContent(label));
        Sprite sprite = property.objectReferenceValue as Sprite;
        if (sprite == null)
        {
            EditorGUILayout.LabelField(fallbackDescription, EditorStyles.wordWrappedMiniLabel);
            return;
        }

        if (!Mathf.Approximately(sprite.rect.width, sprite.rect.height))
        {
            EditorGUILayout.HelpBox("Icon art should be square so cards and HUD slots do not crop awkwardly.", MessageType.Warning);
        }

        if (sprite.texture != null && (sprite.texture.width > 512 || sprite.texture.height > 512))
        {
            EditorGUILayout.HelpBox("Recommended maximum icon texture size is 512x512. Use 256x256 for generated placeholders.", MessageType.Warning);
        }
    }

    public static void DrawReadOnlyProperty(SerializedObject serialized, string propertyName, string label, bool includeChildren = false)
    {
        using (new EditorGUI.DisabledScope(true))
        {
            DrawProperty(serialized, propertyName, label, includeChildren);
        }
    }

    public static void DrawIdNameSync(SerializedProperty idProperty, string sourceName, string fallback, string idLabel = "ID")
    {
        if (idProperty == null)
        {
            EditorGUILayout.HelpBox("Missing ID field.", MessageType.Warning);
            return;
        }

        string expectedId = ContentIdSuggestionRules.NormalizeBase(sourceName, fallback);
        bool mismatched = idProperty.stringValue != expectedId;
        if (mismatched)
        {
            EditorGUILayout.HelpBox($"{idLabel} does not match the current name. Expected '{expectedId}'. Runtime references use IDs, so only update this when you intend to migrate references.", MessageType.Warning);
        }
        else
        {
            EditorGUILayout.LabelField($"{idLabel} matches current name.", EditorStyles.miniLabel);
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField("Expected ID", expectedId, EditorStyles.miniLabel);
            if (GUILayout.Button("Match ID To Name", GUILayout.Width(150f), GUILayout.Height(22f)))
            {
                idProperty.stringValue = expectedId;
            }
        }
    }

    public static void DrawRaritySet(SerializedProperty raritySet, string title = "Possible Rarities")
    {
        if (raritySet == null)
        {
            EditorGUILayout.HelpBox($"Missing {title.ToLowerInvariant()}.", MessageType.Warning);
            return;
        }

        DrawSubsection(title);
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            DrawRarityToggle(raritySet, nameof(ContentRaritySet.common), ContentRarity.Common);
            DrawRarityToggle(raritySet, nameof(ContentRaritySet.uncommon), ContentRarity.Uncommon);
            DrawRarityToggle(raritySet, nameof(ContentRaritySet.rare), ContentRarity.Rare);
            DrawRarityToggle(raritySet, nameof(ContentRaritySet.epic), ContentRarity.Epic);
            DrawRarityToggle(raritySet, nameof(ContentRaritySet.legendary), ContentRarity.Legendary);
        }
    }

    public static void DrawUnitProperty(SerializedObject serialized, string propertyName, string label, string unit)
    {
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property == null)
        {
            EditorGUILayout.HelpBox($"Missing serialized field: {propertyName}", MessageType.Warning);
            return;
        }

        DrawUnitProperty(property, label, unit);
    }

    public static void DrawUnitProperty(SerializedProperty property, string label, string unit)
    {
        Rect rect = EditorGUILayout.GetControlRect();
        Rect labelRect = new(rect.x, rect.y, LabelWidth, rect.height);
        Rect unitRect = new(rect.xMax - UnitWidth, rect.y, UnitWidth, rect.height);
        Rect fieldRect = new(labelRect.xMax + RowGap, rect.y, Mathf.Max(80f, rect.width - LabelWidth - UnitWidth - RowGap * 2f), rect.height);

        EditorGUI.LabelField(labelRect, label);
        EditorGUI.PropertyField(fieldRect, property, GUIContent.none);
        EditorGUI.LabelField(unitRect, unit, EditorStyles.miniLabel);
    }

    private static void DrawRarityToggle(SerializedProperty raritySet, string propertyName, ContentRarity rarity)
    {
        SerializedProperty property = raritySet.FindPropertyRelative(propertyName);
        if (property == null)
        {
            return;
        }

        Color previous = GUI.color;
        GUI.color = ContentRarityMetadata.AuthoringColor(rarity);
        property.boolValue = EditorGUILayout.ToggleLeft(ContentRarityMetadata.Get(rarity).DisplayName, property.boolValue);
        GUI.color = previous;
    }

    public static void DrawModifierValueProperty(SerializedProperty property, StatId statId, StatModifierBucket bucket, string label)
    {
        string unit = ModifierUnit(statId, bucket);
        Rect rect = EditorGUILayout.GetControlRect();
        Rect labelRect = new(rect.x, rect.y, LabelWidth, rect.height);
        Rect unitRect = string.IsNullOrWhiteSpace(unit)
            ? new Rect(rect.xMax, rect.y, 0f, rect.height)
            : new Rect(rect.xMax - UnitWidth, rect.y, UnitWidth, rect.height);
        float unitAndGap = string.IsNullOrWhiteSpace(unit) ? RowGap : UnitWidth + RowGap * 2f;
        Rect fieldRect = new(labelRect.xMax + RowGap, rect.y, Mathf.Max(80f, rect.width - LabelWidth - unitAndGap), rect.height);

        EditorGUI.LabelField(labelRect, label);
        if (UsesPercentAuthoring(statId, bucket))
        {
            float percentValue = property.floatValue * 100f;
            EditorGUI.BeginChangeCheck();
            percentValue = EditorGUI.FloatField(fieldRect, GUIContent.none, percentValue);
            if (EditorGUI.EndChangeCheck())
            {
                property.floatValue = percentValue / 100f;
            }
        }
        else
        {
            EditorGUI.PropertyField(fieldRect, property, GUIContent.none);
        }

        if (!string.IsNullOrWhiteSpace(unit))
        {
            EditorGUI.LabelField(unitRect, unit, EditorStyles.miniLabel);
        }
    }

    public static StatId DrawStatPopup(ContentWorkbenchDomain domain, SerializedProperty property, string label)
    {
        return DrawStatPopup(ContentWorkbenchStatOptions.ForDomain(domain), property, label);
    }

    public static StatId DrawStatPopup(IReadOnlyList<ContentWorkbenchStatOption> options, SerializedProperty property, string label)
    {
        StatId current = property != null ? (StatId)property.intValue : StatId.PlayerMaxHealth;
        StatId next = DrawStatPopup(options, current, label);
        if (property != null && next != current)
        {
            property.intValue = (int)next;
        }

        return next;
    }

    public static StatId DrawStatPopup(IReadOnlyList<ContentWorkbenchStatOption> options, StatId current, string label)
    {
        if (options == null || options.Count == 0)
        {
            EditorGUILayout.LabelField(label, "No valid stats");
            return current;
        }

        bool currentIsAllowed = ContentWorkbenchStatOptions.Contains(options, current);
        int extraUnsupported = currentIsAllowed ? 0 : 1;
        var labels = new string[options.Count + extraUnsupported];
        var statIds = new StatId[options.Count + extraUnsupported];
        int selectedIndex = currentIsAllowed ? 0 : options.Count;
        for (int i = 0; i < options.Count; i++)
        {
            labels[i] = options[i].Label;
            statIds[i] = options[i].StatId;
            if (options[i].StatId == current)
            {
                selectedIndex = i;
            }
        }

        if (!currentIsAllowed)
        {
            int unsupportedIndex = labels.Length - 1;
            labels[unsupportedIndex] = $"Unsupported: {current}";
            statIds[unsupportedIndex] = current;
        }

        int nextIndex = EditorGUILayout.Popup(label, selectedIndex, labels);
        return statIds[Mathf.Clamp(nextIndex, 0, statIds.Length - 1)];
    }

    public static StatModifierBucket DrawDesignerBucketPopup(SerializedProperty property, StatId statId, string label)
    {
        if (property == null)
        {
            EditorGUILayout.LabelField(label, "Missing bucket");
            return DefaultBucketForStat(statId);
        }

        StatModifierBucket current = (StatModifierBucket)property.intValue;
        DesignerBucketChoice currentChoice = DesignerChoiceFor(statId, current);
        DesignerBucketChoice nextChoice = DrawDesignerBucketChoice(statId, currentChoice, label);
        if (nextChoice != currentChoice)
        {
            current = StoredBucketFor(statId, nextChoice);
            property.intValue = (int)current;
        }

        return current;
    }

    public static StatModifierBucket DrawDesignerBucketPopup(StatId statId, StatModifierBucket current, string label)
    {
        DesignerBucketChoice nextChoice = DrawDesignerBucketChoice(statId, DesignerChoiceFor(statId, current), label);
        return StoredBucketFor(statId, nextChoice);
    }

    public static StatModifierBucket DefaultBucketForStat(StatId statId)
    {
        return ContentStatValidationRules.DefaultAuthoredBucket(statId);
    }

    public static bool IsValidDesignerBucket(StatId statId, StatModifierBucket bucket)
    {
        return ContentStatValidationRules.IsValid(statId, bucket);
    }

    public static string InvalidBucketMessage(StatId statId, StatModifierBucket bucket)
    {
        return ContentStatValidationRules.DescribeInvalidModifier(statId, bucket);
    }

    public static bool UsesFlooredIntegerApplication(StatId statId, StatModifierBucket bucket)
    {
        return System.Enum.IsDefined(typeof(StatId), statId)
            && !UsesPercentAuthoring(statId, bucket)
            && StatMetadata.Get(statId).ValueKind == StatValueKind.Integer;
    }

    public static string IntegerCommonValueWarning(StatId statId, StatModifierBucket bucket, float commonValue)
    {
        if (!UsesFlooredIntegerApplication(statId, bucket) || commonValue <= 0f)
        {
            return string.Empty;
        }

        int applied = StatRules.FloorToAppliedInt(ContentRarityScaling.Scale(commonValue, ContentRarity.Common));
        return applied > 0
            ? string.Empty
            : $"{StatMetadata.Get(statId).DisplayName} Common value {commonValue:0.##} floors to +0 at runtime. Raise it to at least +1 or make the line Rare+ if this is intentional.";
    }

    public static string[] DesignerBucketLabelsForStat(StatId statId)
    {
        DesignerBucketChoice[] choices = DesignerChoicesFor(statId);
        string[] labels = new string[choices.Length];
        for (int i = 0; i < choices.Length; i++)
        {
            labels[i] = DesignerLabel(choices[i]);
        }

        return labels;
    }

    public static string DesignerBucketLabel(StatId statId, StatModifierBucket bucket)
    {
        return DesignerLabel(DesignerChoiceFor(statId, bucket));
    }

    private static DesignerBucketChoice DrawDesignerBucketChoice(StatId statId, DesignerBucketChoice current, string label)
    {
        DesignerBucketChoice[] choices = DesignerChoicesFor(statId);
        string[] labels = new string[choices.Length];
        int selectedIndex = 0;
        for (int i = 0; i < choices.Length; i++)
        {
            labels[i] = DesignerLabel(choices[i]);
            if (choices[i] == current)
            {
                selectedIndex = i;
            }
        }

        int nextIndex = EditorGUILayout.Popup(label, selectedIndex, labels);
        return choices[Mathf.Clamp(nextIndex, 0, choices.Length - 1)];
    }

    private static DesignerBucketChoice[] DesignerChoicesFor(StatId statId)
    {
        return DefaultDesignerChoiceFor(statId) == DesignerBucketChoice.AdditivePercent
            ? new[] { DesignerBucketChoice.AdditivePercent }
            : new[] { DesignerBucketChoice.Flat };
    }

    private static DesignerBucketChoice DesignerChoiceFor(StatId statId, StatModifierBucket bucket)
    {
        if (DefaultDesignerChoiceFor(statId) == DesignerBucketChoice.AdditivePercent)
        {
            return DesignerBucketChoice.AdditivePercent;
        }

        return DesignerBucketChoice.Flat;
    }

    private static DesignerBucketChoice DefaultDesignerChoiceFor(StatId statId)
    {
        return StatDisplayRules.IsPercentStat(statId) || StatDisplayRules.IsAttackSpeedStat(statId)
            ? DesignerBucketChoice.AdditivePercent
            : DesignerBucketChoice.Flat;
    }

    private static StatModifierBucket StoredBucketFor(StatId statId, DesignerBucketChoice choice)
    {
        if (choice == DesignerBucketChoice.Flat)
        {
            return StatModifierBucket.Flat;
        }

        return StatModifierBucket.AdditivePercent;
    }

    private static string DesignerLabel(DesignerBucketChoice choice)
    {
        return choice == DesignerBucketChoice.AdditivePercent ? "Additive Percent" : "Flat";
    }

    public static string ModifierUnit(StatId statId, StatModifierBucket bucket)
    {
        if (UsesPercentAuthoring(statId, bucket))
        {
            return "%";
        }

        return statId switch
        {
            StatId.PlayerMaxHealth => "HP",
            StatId.PlayerHpRegenPerMinute => "/min",
            StatId.PlayerArmor => "armor",
            StatId.WeaponFlatDamage => "damage",
            StatId.PickupMagnetRadius or StatId.PickupCollectRadius => "u",
            StatId.Luck => "luck",
            StatId.PlayerExtraJumps => "jumps",
            StatId.ProjectileCount or StatId.WeaponProjectileCount => "shots",
            StatId.Pierce or StatId.WeaponPierce => "pierce",
            StatId.Bounce or StatId.WeaponBounce => "bounces",
            StatId.Chain or StatId.WeaponChain => "chains",
            _ => string.Empty
        };
    }

    public static bool UsesPercentAuthoring(StatId statId, StatModifierBucket bucket)
    {
        return bucket is StatModifierBucket.AdditivePercent
            or StatModifierBucket.MultiplicativePercent
            or StatModifierBucket.WeaponLocalPercent
            or StatModifierBucket.GlobalPercent
            or StatModifierBucket.TagSpecializationPercent
            or StatModifierBucket.ConditionalPercent
            || StatDisplayRules.IsPercentStat(statId)
            || StatDisplayRules.IsAttackSpeedStat(statId);
    }
}
