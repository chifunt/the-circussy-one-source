using Sirenix.OdinInspector.Editor;
using TheCircussyOne.Content;
using UnityEditor;
using UnityEngine;

public static class ContentWorkbenchChestInspector
{
    private static readonly string[] TabLabels =
    {
        "Identity",
        "Interaction",
        "Reward",
        "Visual",
        "Diagnostics"
    };

    private static ContentWorkbenchChestTab selectedTab = ContentWorkbenchChestTab.Identity;

    public static bool CanDraw(Object asset)
    {
        return asset is ChestDefinition;
    }

    public static void Draw(ChestDefinition chest)
    {
        var serialized = new SerializedObject(chest);
        serialized.Update();

        DrawHeader(chest);
        selectedTab = (ContentWorkbenchChestTab)GUILayout.Toolbar((int)selectedTab, TabLabels, GUILayout.MinHeight(24f));
        EditorGUILayout.Space(4f);

        switch (selectedTab)
        {
            case ContentWorkbenchChestTab.Identity:
                DrawIdentity(serialized, chest);
                break;
            case ContentWorkbenchChestTab.Interaction:
                DrawInteraction(serialized);
                break;
            case ContentWorkbenchChestTab.Reward:
                DrawReward(serialized);
                break;
            case ContentWorkbenchChestTab.Visual:
                DrawVisual(serialized);
                break;
            case ContentWorkbenchChestTab.Diagnostics:
                DrawDiagnostics(chest);
                break;
        }

        if (serialized.ApplyModifiedProperties())
        {
            EditorUtility.SetDirty(chest);
        }
    }

    public static int TabCount => TabLabels.Length;

    public static string TabLabel(int index)
    {
        return TabLabels[Mathf.Clamp(index, 0, TabLabels.Length - 1)];
    }

    private static void DrawHeader(ChestDefinition chest)
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField("Chest Authoring", EditorStyles.boldLabel);
            if (GUILayout.Button("Open Odin Inspector", GUILayout.Width(150f), GUILayout.Height(22f)))
            {
                OdinEditorWindow.InspectObject(chest);
            }
        }

        EditorGUILayout.LabelField("Chests are world interactables that can spend Tickets and reveal one item reward.", EditorStyles.wordWrappedMiniLabel);
    }

    private static void DrawIdentity(SerializedObject serialized, ChestDefinition chest)
    {
        ContentWorkbenchFieldDrawer.DrawSection("Identity");
        ContentWorkbenchFieldDrawer.DrawReadOnlyProperty(serialized, nameof(ChestDefinition.chestId), "Chest ID");
        ContentWorkbenchFieldDrawer.DrawProperty(serialized, nameof(ChestDefinition.displayName), "Display Name");
        ContentWorkbenchFieldDrawer.DrawIdNameSync(
            serialized.FindProperty(nameof(ChestDefinition.chestId)),
            serialized.FindProperty(nameof(ChestDefinition.displayName))?.stringValue,
            "chest",
            "Chest ID");
        ContentWorkbenchFieldDrawer.DrawProperty(serialized, nameof(ChestDefinition.shortDescription), "Short Description", includeChildren: true);
        ContentWorkbenchFieldDrawer.DrawProperty(serialized, nameof(ChestDefinition.kind), "Kind");
        EditorGUILayout.LabelField("Computed Tags", ComputedContentTagRules.Format(chest.Tags), EditorStyles.miniLabel);
    }

    private static void DrawInteraction(SerializedObject serialized)
    {
        ContentWorkbenchFieldDrawer.DrawSection("Interaction");
        ContentWorkbenchFieldDrawer.DrawUnitProperty(serialized.FindProperty(nameof(ChestDefinition.holdSeconds)), "Hold Time", "sec");
        ContentWorkbenchFieldDrawer.DrawUnitProperty(serialized.FindProperty(nameof(ChestDefinition.ticketCost)), "Ticket Cost", "Tickets");
        ContentWorkbenchFieldDrawer.DrawProperty(serialized, nameof(ChestDefinition.promptText), "Prompt Text");
    }

    private static void DrawReward(SerializedObject serialized)
    {
        ContentWorkbenchFieldDrawer.DrawSection("Item Reward");
        ContentWorkbenchFieldDrawer.DrawProperty(serialized, nameof(ChestDefinition.itemRarityWeights), "Item Rarity Weights", includeChildren: true);
        EditorGUILayout.LabelField("The chest rolls one active addable item. Rarity uses these weights plus current Luck.", EditorStyles.wordWrappedMiniLabel);
    }

    private static void DrawVisual(SerializedObject serialized)
    {
        ContentWorkbenchFieldDrawer.DrawSection("Placeholder Visual");
        ContentWorkbenchFieldDrawer.DrawProperty(serialized, nameof(ChestDefinition.visualColor), "Color");
        ContentWorkbenchFieldDrawer.DrawProperty(serialized, nameof(ChestDefinition.visualScale), "Scale");

        ContentWorkbenchFieldDrawer.DrawSection("Glow");
        ContentWorkbenchFieldDrawer.DrawProperty(serialized, nameof(ChestDefinition.emissionColor), "Emission Color");
        ContentWorkbenchFieldDrawer.DrawUnitProperty(serialized.FindProperty(nameof(ChestDefinition.emissionStrength)), "Emission Strength", "x");

        ContentWorkbenchFieldDrawer.DrawSection("Target Outline");
        ContentWorkbenchFieldDrawer.DrawProperty(serialized, nameof(ChestDefinition.targetOutlineColor), "Outline Color");
        ContentWorkbenchFieldDrawer.DrawProperty(serialized, nameof(ChestDefinition.insufficientTicketsOutlineColor), "Insufficient Tickets Color");
        ContentWorkbenchFieldDrawer.DrawUnitProperty(serialized.FindProperty(nameof(ChestDefinition.targetOutlineThickness)), "Outline Width", "px");

        ContentWorkbenchFieldDrawer.DrawSection("Interaction Squash");
        ContentWorkbenchFieldDrawer.DrawUnitProperty(serialized.FindProperty(nameof(ChestDefinition.interactionSquashStretchAmplitude)), "Amplitude", "x");
        ContentWorkbenchFieldDrawer.DrawUnitProperty(serialized.FindProperty(nameof(ChestDefinition.interactionSquashStretchFrequency)), "Frequency", "Hz");
    }

    private static void DrawDiagnostics(ChestDefinition chest)
    {
        ContentWorkbenchFieldDrawer.DrawSection("Diagnostics");
        EditorGUILayout.LabelField("Runtime Summary", $"{chest.DisplayName}: {chest.kind}, {chest.TicketCost} Tickets, {chest.holdSeconds:0.##} sec", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("Reward", $"Weights total {chest.itemRarityWeights.TotalWeight}", EditorStyles.miniLabel);
    }
}

public enum ContentWorkbenchChestTab
{
    Identity,
    Interaction,
    Reward,
    Visual,
    Diagnostics
}
