using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using TheCircussyOne.Content;

public sealed class TheCircussyOneCustomWeaponCreator
{
    public const string CustomWeaponFolder = TheCircussyOneAssetPaths.WeaponBalanceFolder + "/Custom";
    public const string CustomUpgradeFolder = TheCircussyOneAssetPaths.UpgradeBalanceFolder + "/Custom";

    [TitleGroup("Create Custom Weapon")]
    [InfoBox("Creates a new weapon definition from an implemented family template. The weapon is added to Available Weapons only, never to Starting Loadout.")]
    [LabelWidth(170)]
    public string weaponId = "custom_weapon";

    [LabelWidth(170)]
    public string displayName = "Custom Weapon";

    [LabelWidth(170), EnumToggleButtons]
    public WeaponFamilyTemplate template = WeaponFamilyTemplate.DirectProjectile;

    [ShowInInspector, ReadOnly, MultiLineProperty(4), LabelText("Template Preview")]
    public string TemplatePreview => WeaponAuthoringApplicability.TemplatePreview(template);

    [LabelWidth(170)]
    public bool addToWeaponCatalog = true;

    [ShowInInspector, ReadOnly, MultiLineProperty(2), LabelText("Last Result")]
    public string LastResult { get; private set; } = "No custom weapon created in this session.";

    [Button(ButtonSizes.Large), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.BalanceColor")]
    public void CreateWeapon()
    {
        CustomWeaponCreationResult result = CreateCustomWeapon(
            weaponId,
            displayName,
            template,
            addToWeaponCatalog);

        LastResult = result.Message;
        if (result.Weapon != null)
        {
            Selection.activeObject = result.Weapon;
            EditorGUIUtility.PingObject(result.Weapon);
        }
    }

    public static CustomWeaponCreationResult CreateCustomWeapon(
        string requestedId,
        string requestedDisplayName,
        WeaponFamilyTemplate template,
        bool addToWeaponCatalog = true,
        string weaponFolder = CustomWeaponFolder)
    {
        string normalizedId = ContentId.Normalize(requestedId);
        if (!ContentId.IsValidValue(normalizedId))
        {
            return CustomWeaponCreationResult.Fail($"Weapon id '{requestedId}' is invalid. Use lowercase snake_case, for example 'rubber_chicken'.");
        }

        if (string.IsNullOrWhiteSpace(requestedDisplayName))
        {
            return CustomWeaponCreationResult.Fail("Display name is required.");
        }

        EnsureFolder(weaponFolder);

        string assetName = AssetNameFromId(normalizedId);
        string weaponPath = $"{weaponFolder}/{assetName}.asset";
        if (AssetDatabase.LoadAssetAtPath<WeaponDefinition>(weaponPath) != null)
        {
            return CustomWeaponCreationResult.Fail($"Weapon already exists at {weaponPath}. Existing assets are never overwritten.");
        }

        WeaponDefinition weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
        weapon.ApplyCustomTemplateDefaults(normalizedId, requestedDisplayName.Trim(), template);
        AssetDatabase.CreateAsset(weapon, weaponPath);
        EditorUtility.SetDirty(weapon);

        if (addToWeaponCatalog)
        {
            AddWeaponToCatalog(weapon);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return CustomWeaponCreationResult.Created(weapon, $"Created {weapon.DisplayName}.");
    }

    private static void AddWeaponToCatalog(WeaponDefinition weapon)
    {
        WeaponCatalog catalog = AssetDatabase.LoadAssetAtPath<WeaponCatalog>(TheCircussyOneAssetPaths.WeaponCatalogPath);
        if (catalog == null || weapon == null)
        {
            return;
        }

        catalog.availableWeapons ??= new System.Collections.Generic.List<WeaponDefinition>();
        if (!catalog.availableWeapons.Contains(weapon))
        {
            catalog.availableWeapons.Add(weapon);
            EditorUtility.SetDirty(catalog);
        }
    }

    private static string AssetNameFromId(string id)
    {
        string normalized = ContentId.Normalize(id);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return "CustomWeapon";
        }

        string[] parts = normalized.Split('_');
        for (int i = 0; i < parts.Length; i++)
        {
            if (parts[i].Length == 0)
            {
                continue;
            }

            parts[i] = char.ToUpperInvariant(parts[i][0]) + parts[i][1..];
        }

        return string.Concat(parts);
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        string[] parts = path.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = $"{current}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }

            current = next;
        }
    }
}

public readonly struct CustomWeaponCreationResult
{
    private CustomWeaponCreationResult(bool success, string message, WeaponDefinition weapon)
    {
        Success = success;
        Message = message;
        Weapon = weapon;
    }

    public bool Success { get; }
    public string Message { get; }
    public WeaponDefinition Weapon { get; }

    public static CustomWeaponCreationResult Created(WeaponDefinition weapon, string message)
    {
        return new CustomWeaponCreationResult(true, message, weapon);
    }

    public static CustomWeaponCreationResult Fail(string message)
    {
        return new CustomWeaponCreationResult(false, message, null);
    }
}
