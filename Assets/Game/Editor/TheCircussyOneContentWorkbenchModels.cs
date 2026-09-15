using System;
using System.Collections.Generic;
using System.IO;
using TheCircussyOne.Content;
using TheCircussyOne.Stats;
using UnityEditor;
using UnityEngine;

public enum ContentWorkbenchDomain
{
    Weapons,
    Items,
    Chests,
    Performers,
    Enemies,
    Headliners,
    Upgrades,
    Talents
}

public readonly struct ContentWorkbenchDomainGroup
{
    public ContentWorkbenchDomainGroup(string title, ContentWorkbenchDomain[] domains)
    {
        Title = title ?? string.Empty;
        Domains = domains ?? Array.Empty<ContentWorkbenchDomain>();
    }

    public string Title { get; }
    public IReadOnlyList<ContentWorkbenchDomain> Domains { get; }
}

public static class ContentWorkbenchDomainLayout
{
    private static readonly ContentWorkbenchDomainGroup[] Groups =
    {
        new("Playable Roster", new[] { ContentWorkbenchDomain.Performers, ContentWorkbenchDomain.Weapons }),
        new("Level-Up Rewards", new[] { ContentWorkbenchDomain.Upgrades, ContentWorkbenchDomain.Talents }),
        new("World Content", new[] { ContentWorkbenchDomain.Items, ContentWorkbenchDomain.Chests, ContentWorkbenchDomain.Enemies, ContentWorkbenchDomain.Headliners })
    };

    public static IReadOnlyList<ContentWorkbenchDomainGroup> DomainGroups => Groups;

    public static string DisplayName(ContentWorkbenchDomain domain)
    {
        return domain switch
        {
            ContentWorkbenchDomain.Upgrades => "Weapon Upgrades",
            _ => domain.ToString()
        };
    }

    public static int SortIndex(ContentWorkbenchDomain domain)
    {
        int index = 0;
        for (int groupIndex = 0; groupIndex < Groups.Length; groupIndex++)
        {
            IReadOnlyList<ContentWorkbenchDomain> domains = Groups[groupIndex].Domains;
            for (int domainIndex = 0; domainIndex < domains.Count; domainIndex++)
            {
                if (domains[domainIndex] == domain)
                {
                    return index;
                }

                index++;
            }
        }

        return int.MaxValue;
    }
}

public enum ContentWorkbenchCreateTemplate
{
    DirectProjectileWeapon,
    SpreadProjectileWeapon,
    ExplosiveArcWeapon,
    BounceWeapon,
    ChainWeapon,
    OrbitWeapon,
    PassiveItem,
    Chest,
    Performer,
    EnemyFromNormal,
    Headliner,
    WeaponStatUpgrade,
    SharedTalent,
    PerformerTalent
}

public enum ContentWorkbenchDetailMode
{
    SelectedContent,
    CreateContent
}

public enum ContentWorkbenchAvailabilityState
{
    Available,
    Disabled,
    Draft
}

public readonly struct ContentWorkbenchFilterOptions
{
    public ContentWorkbenchFilterOptions(
        bool showAvailable,
        bool showDisabled,
        bool showDraft,
        string search,
        bool showOk = true,
        bool showWarnings = true,
        bool showErrors = true)
    {
        ShowAvailable = showAvailable;
        ShowDisabled = showDisabled;
        ShowDraft = showDraft;
        Search = search ?? string.Empty;
        ShowOk = showOk;
        ShowWarnings = showWarnings;
        ShowErrors = showErrors;
    }

    public bool ShowAvailable { get; }
    public bool ShowDisabled { get; }
    public bool ShowDraft { get; }
    public string Search { get; }
    public bool ShowOk { get; }
    public bool ShowWarnings { get; }
    public bool ShowErrors { get; }

    public static ContentWorkbenchFilterOptions Default => new(true, true, true, string.Empty, true, true, true);

    public bool Passes(ContentWorkbenchEntry entry)
    {
        if (entry == null)
        {
            return false;
        }

        ContentWorkbenchAvailabilityState availability = entry.Availability;
        if (availability == ContentWorkbenchAvailabilityState.Available && !ShowAvailable)
        {
            return false;
        }

        if (availability == ContentWorkbenchAvailabilityState.Disabled && !ShowDisabled)
        {
            return false;
        }

        if (availability == ContentWorkbenchAvailabilityState.Draft && !ShowDraft)
        {
            return false;
        }

        if (entry.ErrorCount > 0 && !ShowErrors)
        {
            return false;
        }

        if (entry.ErrorCount == 0 && entry.WarningCount > 0 && !ShowWarnings)
        {
            return false;
        }

        if (entry.ErrorCount == 0 && entry.WarningCount == 0 && !ShowOk)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(Search))
        {
            return true;
        }

        return (entry.DisplayName != null && entry.DisplayName.IndexOf(Search, StringComparison.OrdinalIgnoreCase) >= 0)
            || (entry.Id != null && entry.Id.IndexOf(Search, StringComparison.OrdinalIgnoreCase) >= 0);
    }

    public int CountVisible(IReadOnlyList<ContentWorkbenchEntry> entries)
    {
        if (entries == null)
        {
            return 0;
        }

        int count = 0;
        for (int i = 0; i < entries.Count; i++)
        {
            if (Passes(entries[i]))
            {
                count++;
            }
        }

        return count;
    }
}

public static class ContentWorkbenchEntryLabels
{
    public const string OkIconName = "TestPassed";
    public const string WarningIconName = "console.warnicon";
    public const string ErrorIconName = "console.erroricon";

    private static readonly Color AvailableText = new(0.28f, 0.72f, 0.36f, 1f);
    private static readonly Color DisabledText = new(0.95f, 0.58f, 0.22f, 1f);
    private static readonly Color DraftText = new(0.52f, 0.62f, 0.75f, 1f);
    private static readonly Color OkText = new(0.34f, 0.78f, 0.38f, 1f);
    private static readonly Color WarningText = new(1f, 0.76f, 0.25f, 1f);
    private static readonly Color ErrorText = new(1f, 0.34f, 0.3f, 1f);

    public static string ValidationBadge(ContentWorkbenchEntry entry)
    {
        if (entry == null)
        {
            return "Missing";
        }

        if (entry.ErrorCount > 0)
        {
            return "Error";
        }

        return entry.WarningCount > 0 ? "Warning" : "OK";
    }

    public static MessageType ValidationMessageType(ContentWorkbenchEntry entry)
    {
        if (entry == null)
        {
            return MessageType.Warning;
        }

        if (entry.ErrorCount > 0)
        {
            return MessageType.Error;
        }

        return entry.WarningCount > 0 ? MessageType.Warning : MessageType.Info;
    }

    public static string ListLabel(ContentWorkbenchEntry entry)
    {
        if (entry == null)
        {
            return "Unknown";
        }

        return $"{entry.DisplayName} ({entry.Id})";
    }

    public static string ValidationIconName(ContentWorkbenchEntry entry)
    {
        if (entry == null || entry.ErrorCount > 0)
        {
            return ErrorIconName;
        }

        return entry.WarningCount > 0 ? WarningIconName : OkIconName;
    }

    public static string ValidationTooltip(ContentWorkbenchEntry entry)
    {
        if (entry == null)
        {
            return "Missing content entry.";
        }

        return entry.ErrorCount > 0
            ? $"{entry.ErrorCount} validation error(s)."
            : entry.WarningCount > 0
                ? $"{entry.WarningCount} validation warning(s)."
                : "Validation OK.";
    }

    public static string AvailabilityLabel(ContentWorkbenchEntry entry)
    {
        return entry == null ? "Missing" : AvailabilityLabel(entry.Availability);
    }

    public static string AvailabilityLabel(ContentWorkbenchAvailabilityState availability)
    {
        return availability switch
        {
            ContentWorkbenchAvailabilityState.Available => "Available",
            ContentWorkbenchAvailabilityState.Disabled => "Disabled",
            ContentWorkbenchAvailabilityState.Draft => "Draft",
            _ => "Unknown"
        };
    }

    public static string AvailabilityDescription(ContentWorkbenchEntry entry)
    {
        return entry == null ? "No content selected." : entry.Availability switch
        {
            ContentWorkbenchAvailabilityState.Available => "Available to the game.",
            ContentWorkbenchAvailabilityState.Disabled => "In the game catalog, but disabled.",
            ContentWorkbenchAvailabilityState.Draft => "Draft only. The game will not use this asset.",
            _ => "Unknown availability state."
        };
    }

    public static Color AvailabilityTextColor(ContentWorkbenchEntry entry)
    {
        return entry == null ? Color.white : AvailabilityTextColor(entry.Availability);
    }

    public static Color AvailabilityTextColor(ContentWorkbenchAvailabilityState availability)
    {
        return availability switch
        {
            ContentWorkbenchAvailabilityState.Available => AvailableText,
            ContentWorkbenchAvailabilityState.Disabled => DisabledText,
            ContentWorkbenchAvailabilityState.Draft => DraftText,
            _ => Color.white
        };
    }

    public static Color AvailabilityBackgroundColor(ContentWorkbenchEntry entry, bool selected, bool proSkin)
    {
        Color baseColor = proSkin ? new Color(0.22f, 0.22f, 0.22f, 1f) : Color.white;
        Color tint = entry == null ? baseColor : AvailabilityTextColor(entry);
        float amount = selected ? 0.42f : 0.2f;
        return Color.Lerp(baseColor, tint, amount);
    }

    public static Color AvailabilityFilterBackgroundColor(ContentWorkbenchAvailabilityState availability, bool enabled, bool proSkin)
    {
        Color baseColor = proSkin ? new Color(0.24f, 0.24f, 0.24f, 1f) : Color.white;
        float amount = enabled ? 0.38f : 0.12f;
        return Color.Lerp(baseColor, AvailabilityTextColor(availability), amount);
    }

    public static Color AvailabilityFilterTextColor(ContentWorkbenchAvailabilityState availability, bool enabled)
    {
        Color color = AvailabilityTextColor(availability);
        return enabled ? color : Color.Lerp(color, Color.gray, 0.55f);
    }

    public static Color ValidationTextColor(int errors, int warnings)
    {
        if (errors > 0)
        {
            return ErrorText;
        }

        return warnings > 0 ? WarningText : OkText;
    }

    public static Color ValidationFilterBackgroundColor(int errors, int warnings, bool enabled, bool proSkin)
    {
        Color baseColor = proSkin ? new Color(0.24f, 0.24f, 0.24f, 1f) : Color.white;
        float amount = enabled ? 0.38f : 0.12f;
        return Color.Lerp(baseColor, ValidationTextColor(errors, warnings), amount);
    }

    public static Color ValidationFilterTextColor(int errors, int warnings, bool enabled)
    {
        Color color = ValidationTextColor(errors, warnings);
        return enabled ? color : Color.Lerp(color, Color.gray, 0.55f);
    }
}

public sealed class ContentWorkbenchCreateRequest
{
    public ContentWorkbenchDomain Domain;
    public ContentWorkbenchCreateTemplate Template;
    // Optional advanced override. Normal designer UI leaves this blank.
    public string Id;
    // Designer-facing name. When blank, the template supplies a safe default.
    public string DisplayName;
    public bool AddToCatalog = true;
    public WeaponDefinition TargetWeapon;
    public PerformerDefinition TargetPerformer;
    public StatId StatId = StatId.WeaponFlatDamage;
    public StatModifierBucket Bucket = StatModifierBucket.Flat;
    public float Value = 1f;
    public string AssetFolderOverride;
    public string CatalogPathOverride;
}

public sealed class ContentWorkbenchIdentityPreview
{
    public ContentWorkbenchIdentityPreview(string id, string displayName, string assetPath)
    {
        Id = id ?? string.Empty;
        DisplayName = displayName ?? string.Empty;
        AssetPath = assetPath ?? string.Empty;
    }

    public string Id { get; }
    public string DisplayName { get; }
    public string AssetPath { get; }
}

public sealed class ContentWorkbenchCatalogPathOverrides
{
    public string WeaponCatalogPath;
    public string ItemCatalogPath;
    public string ChestCatalogPath;
    public string PerformerCatalogPath;
    public string EnemyCatalogPath;
    public string HeadlinerCatalogPath;
    public string UpgradeCatalogPath;
    public string TalentCatalogPath;

    public string For(ContentWorkbenchDomain domain)
    {
        return domain switch
        {
            ContentWorkbenchDomain.Weapons => WeaponCatalogPath,
            ContentWorkbenchDomain.Items => ItemCatalogPath,
            ContentWorkbenchDomain.Chests => ChestCatalogPath,
            ContentWorkbenchDomain.Performers => PerformerCatalogPath,
            ContentWorkbenchDomain.Enemies => EnemyCatalogPath,
            ContentWorkbenchDomain.Headliners => HeadlinerCatalogPath,
            ContentWorkbenchDomain.Upgrades => UpgradeCatalogPath,
            ContentWorkbenchDomain.Talents => TalentCatalogPath,
            _ => null
        };
    }
}

public sealed class ContentWorkbenchDeletePreview
{
    public ContentWorkbenchDeletePreview(string title)
    {
        Title = string.IsNullOrWhiteSpace(title) ? "Delete Content" : title;
    }

    public string Title { get; }
    public List<string> Blockers { get; } = new();
    public List<string> Warnings { get; } = new();
    public List<string> CleanupActions { get; } = new();
    public List<string> AssetPaths { get; } = new();

    public bool CanDelete => Blockers.Count == 0 && AssetPaths.Count > 0;

    public string BlockerMessage
    {
        get
        {
            if (Blockers.Count == 0)
            {
                return "No delete blockers.";
            }

            return "Cannot delete content:\n" + string.Join("\n", Blockers);
        }
    }

    public string ConfirmationMessage
    {
        get
        {
            var lines = new List<string>();
            lines.Add("This permanently deletes:");
            for (int i = 0; i < AssetPaths.Count; i++)
            {
                lines.Add($"- {AssetPaths[i]}");
            }

            if (CleanupActions.Count > 0)
            {
                lines.Add(string.Empty);
                lines.Add("It will also clean:");
                for (int i = 0; i < CleanupActions.Count; i++)
                {
                    lines.Add($"- {CleanupActions[i]}");
                }
            }

            if (Warnings.Count > 0)
            {
                lines.Add(string.Empty);
                lines.Add("Warnings:");
                for (int i = 0; i < Warnings.Count; i++)
                {
                    lines.Add($"- {Warnings[i]}");
                }
            }

            return string.Join("\n", lines);
        }
    }

    public void AddAsset(UnityEngine.Object asset)
    {
        string path = AssetDatabase.GetAssetPath(asset);
        AddAssetPath(path);
    }

    public void AddAssetPath(string path)
    {
        if (!string.IsNullOrWhiteSpace(path) && !AssetPaths.Contains(path))
        {
            AssetPaths.Add(path);
        }
    }
}

public readonly struct ContentWorkbenchActionResult
{
    private ContentWorkbenchActionResult(bool success, string message, UnityEngine.Object asset)
    {
        Success = success;
        Message = message ?? string.Empty;
        Asset = asset;
    }

    public bool Success { get; }
    public string Message { get; }
    public UnityEngine.Object Asset { get; }

    public static ContentWorkbenchActionResult Ok(string message, UnityEngine.Object asset = null)
        => new(true, message, asset);

    public static ContentWorkbenchActionResult Fail(string message)
        => new(false, message, null);
}

public sealed class ContentWorkbenchEntry
{
    public UnityEngine.Object Asset;
    public string Path;
    public string Id;
    public string DisplayName;
    public string SubgroupLabel;
    public int SubgroupSortIndex;
    public bool IsActive;
    public bool IsCataloged;
    public int ErrorCount;
    public int WarningCount;
    public ContentWorkbenchAvailabilityState Availability
    {
        get
        {
            if (!IsCataloged)
            {
                return ContentWorkbenchAvailabilityState.Draft;
            }

            return IsActive ? ContentWorkbenchAvailabilityState.Available : ContentWorkbenchAvailabilityState.Disabled;
        }
    }

    public string Status
    {
        get
        {
            string availability = ContentWorkbenchEntryLabels.AvailabilityLabel(Availability);
            string validation = ErrorCount > 0 ? $"{ErrorCount} error" : WarningCount > 0 ? $"{WarningCount} warning" : "Valid";
            return $"{availability} / {validation}";
        }
    }
}

public readonly struct ContentWorkbenchEntrySubgroupInfo
{
    public ContentWorkbenchEntrySubgroupInfo(string label, int sortIndex)
    {
        Label = label ?? string.Empty;
        SortIndex = sortIndex;
    }

    public string Label { get; }
    public int SortIndex { get; }
    public bool HasSubgroup => !string.IsNullOrWhiteSpace(Label);
}

public static class ContentWorkbenchEntryGrouping
{
    private const int DefaultSortIndex = 0;
    private const int TargetedContentSortIndex = 10;
    private const int MissingTargetSortIndex = 1000;

    public static ContentWorkbenchEntrySubgroupInfo Resolve(ContentWorkbenchDomain domain, UnityEngine.Object asset)
    {
        return domain switch
        {
            ContentWorkbenchDomain.Upgrades => UpgradeGroup(asset as UpgradeDefinition),
            ContentWorkbenchDomain.Talents => TalentGroup(asset as TalentDefinition),
            _ => new ContentWorkbenchEntrySubgroupInfo(string.Empty, DefaultSortIndex)
        };
    }

    private static ContentWorkbenchEntrySubgroupInfo UpgradeGroup(UpgradeDefinition upgrade)
    {
        if (upgrade == null)
        {
            return Missing("Unassigned Weapon");
        }

        if (upgrade.weaponDefinition != null)
        {
            return new ContentWorkbenchEntrySubgroupInfo(SafeDisplayName(upgrade.weaponDefinition.DisplayName, upgrade.weaponDefinition.Id, "Weapon"), TargetedContentSortIndex);
        }

        if (!string.IsNullOrWhiteSpace(upgrade.weaponId))
        {
            return new ContentWorkbenchEntrySubgroupInfo(DisplayNameFromId(upgrade.weaponId, "Weapon"), TargetedContentSortIndex);
        }

        return Missing("Unassigned Weapon");
    }

    private static ContentWorkbenchEntrySubgroupInfo TalentGroup(TalentDefinition talent)
    {
        if (talent == null)
        {
            return Missing("Unassigned Performer");
        }

        if (talent.poolKind == TalentPoolKind.Shared)
        {
            return new ContentWorkbenchEntrySubgroupInfo("Shared Talents", DefaultSortIndex);
        }

        if (talent.performerDefinition != null)
        {
            return new ContentWorkbenchEntrySubgroupInfo(SafeDisplayName(talent.performerDefinition.DisplayName, talent.performerDefinition.Id, "Performer"), TargetedContentSortIndex);
        }

        if (!string.IsNullOrWhiteSpace(talent.performerId))
        {
            return new ContentWorkbenchEntrySubgroupInfo(DisplayNameFromId(talent.performerId, "Performer"), TargetedContentSortIndex);
        }

        return Missing("Unassigned Performer");
    }

    private static ContentWorkbenchEntrySubgroupInfo Missing(string label)
    {
        return new ContentWorkbenchEntrySubgroupInfo(label, MissingTargetSortIndex);
    }

    private static string SafeDisplayName(string displayName, string id, string fallback)
    {
        if (!string.IsNullOrWhiteSpace(displayName))
        {
            return displayName.Trim();
        }

        return DisplayNameFromId(id, fallback);
    }

    private static string DisplayNameFromId(string id, string fallback)
    {
        string normalized = ContentId.Normalize(id);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return fallback;
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

        return string.Join(" ", parts);
    }
}

public sealed class ContentWorkbenchEntrySubgroup
{
    public ContentWorkbenchEntrySubgroup(string label, IReadOnlyList<ContentWorkbenchEntry> entries)
    {
        Label = label ?? string.Empty;
        Entries = entries ?? Array.Empty<ContentWorkbenchEntry>();
    }

    public string Label { get; }
    public IReadOnlyList<ContentWorkbenchEntry> Entries { get; }
}

public sealed class ContentWorkbenchEntryGroup
{
    public ContentWorkbenchEntryGroup(ContentWorkbenchAvailabilityState availability, IReadOnlyList<ContentWorkbenchEntry> entries)
        : this(availability, entries, Array.Empty<ContentWorkbenchEntrySubgroup>())
    {
    }

    public ContentWorkbenchEntryGroup(
        ContentWorkbenchAvailabilityState availability,
        IReadOnlyList<ContentWorkbenchEntry> entries,
        IReadOnlyList<ContentWorkbenchEntrySubgroup> subgroups)
    {
        Availability = availability;
        Entries = entries ?? Array.Empty<ContentWorkbenchEntry>();
        Subgroups = subgroups ?? Array.Empty<ContentWorkbenchEntrySubgroup>();
    }

    public ContentWorkbenchAvailabilityState Availability { get; }
    public IReadOnlyList<ContentWorkbenchEntry> Entries { get; }
    public IReadOnlyList<ContentWorkbenchEntrySubgroup> Subgroups { get; }
}

public static class ContentWorkbenchEntryLayout
{
    public static IReadOnlyList<ContentWorkbenchEntryGroup> BuildGroups(IReadOnlyList<ContentWorkbenchEntry> entries, ContentWorkbenchFilterOptions filters)
    {
        var available = new List<ContentWorkbenchEntry>();
        var disabled = new List<ContentWorkbenchEntry>();
        var draft = new List<ContentWorkbenchEntry>();
        if (entries != null)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                ContentWorkbenchEntry entry = entries[i];
                if (!filters.Passes(entry))
                {
                    continue;
                }

                switch (entry.Availability)
                {
                    case ContentWorkbenchAvailabilityState.Available:
                        available.Add(entry);
                        break;
                    case ContentWorkbenchAvailabilityState.Disabled:
                        disabled.Add(entry);
                        break;
                    case ContentWorkbenchAvailabilityState.Draft:
                        draft.Add(entry);
                        break;
                }
            }
        }

        available.Sort(CompareEntriesForLayout);
        disabled.Sort(CompareEntriesForLayout);
        draft.Sort(CompareEntriesForLayout);

        var groups = new List<ContentWorkbenchEntryGroup>(3);
        AddGroup(groups, ContentWorkbenchAvailabilityState.Available, available);
        AddGroup(groups, ContentWorkbenchAvailabilityState.Disabled, disabled);
        AddGroup(groups, ContentWorkbenchAvailabilityState.Draft, draft);
        return groups;
    }

    public static int CompareEntries(ContentWorkbenchEntry left, ContentWorkbenchEntry right)
    {
        if (ReferenceEquals(left, right))
        {
            return 0;
        }

        if (left == null)
        {
            return 1;
        }

        if (right == null)
        {
            return -1;
        }

        int validation = ValidationSortIndex(left).CompareTo(ValidationSortIndex(right));
        if (validation != 0)
        {
            return validation;
        }

        int name = string.Compare(left.DisplayName, right.DisplayName, StringComparison.OrdinalIgnoreCase);
        if (name != 0)
        {
            return name;
        }

        return string.Compare(left.Id, right.Id, StringComparison.OrdinalIgnoreCase);
    }

    private static int CompareEntriesForLayout(ContentWorkbenchEntry left, ContentWorkbenchEntry right)
    {
        int subgroup = CompareSubgroups(left, right);
        return subgroup != 0 ? subgroup : CompareEntries(left, right);
    }

    private static void AddGroup(List<ContentWorkbenchEntryGroup> groups, ContentWorkbenchAvailabilityState availability, IReadOnlyList<ContentWorkbenchEntry> entries)
    {
        if (entries.Count > 0)
        {
            groups.Add(new ContentWorkbenchEntryGroup(availability, entries, BuildSubgroups(entries)));
        }
    }

    private static IReadOnlyList<ContentWorkbenchEntrySubgroup> BuildSubgroups(IReadOnlyList<ContentWorkbenchEntry> entries)
    {
        if (!HasSubgroups(entries))
        {
            return Array.Empty<ContentWorkbenchEntrySubgroup>();
        }

        var groups = new List<ContentWorkbenchEntrySubgroup>();
        var currentEntries = new List<ContentWorkbenchEntry>();
        string currentLabel = null;
        for (int i = 0; i < entries.Count; i++)
        {
            ContentWorkbenchEntry entry = entries[i];
            string label = string.IsNullOrWhiteSpace(entry?.SubgroupLabel) ? "Other" : entry.SubgroupLabel.Trim();
            if (currentLabel != null && !string.Equals(currentLabel, label, StringComparison.Ordinal))
            {
                groups.Add(new ContentWorkbenchEntrySubgroup(currentLabel, currentEntries.ToArray()));
                currentEntries.Clear();
            }

            currentLabel = label;
            currentEntries.Add(entry);
        }

        if (currentLabel != null)
        {
            groups.Add(new ContentWorkbenchEntrySubgroup(currentLabel, currentEntries.ToArray()));
        }

        return groups;
    }

    private static bool HasSubgroups(IReadOnlyList<ContentWorkbenchEntry> entries)
    {
        if (entries == null)
        {
            return false;
        }

        for (int i = 0; i < entries.Count; i++)
        {
            if (!string.IsNullOrWhiteSpace(entries[i]?.SubgroupLabel))
            {
                return true;
            }
        }

        return false;
    }

    private static int CompareSubgroups(ContentWorkbenchEntry left, ContentWorkbenchEntry right)
    {
        if (ReferenceEquals(left, right))
        {
            return 0;
        }

        if (left == null)
        {
            return 1;
        }

        if (right == null)
        {
            return -1;
        }

        int sortIndex = left.SubgroupSortIndex.CompareTo(right.SubgroupSortIndex);
        if (sortIndex != 0)
        {
            return sortIndex;
        }

        return string.Compare(left.SubgroupLabel, right.SubgroupLabel, StringComparison.OrdinalIgnoreCase);
    }

    private static int ValidationSortIndex(ContentWorkbenchEntry entry)
    {
        if (entry.ErrorCount > 0)
        {
            return 0;
        }

        return entry.WarningCount > 0 ? 1 : 2;
    }
}

public readonly struct ContentWorkbenchDomainSummary
{
    private ContentWorkbenchDomainSummary(
        int total,
        int visible,
        int available,
        int disabled,
        int draft,
        int ok,
        int warnings,
        int errors)
    {
        Total = total;
        Visible = visible;
        Available = available;
        Disabled = disabled;
        Draft = draft;
        Ok = ok;
        Warnings = warnings;
        Errors = errors;
    }

    public int Total { get; }
    public int Visible { get; }
    public int Available { get; }
    public int Disabled { get; }
    public int Draft { get; }
    public int Ok { get; }
    public int Warnings { get; }
    public int Errors { get; }

    public static ContentWorkbenchDomainSummary Build(IReadOnlyList<ContentWorkbenchEntry> entries, ContentWorkbenchFilterOptions filters)
    {
        if (entries == null)
        {
            return new ContentWorkbenchDomainSummary(0, 0, 0, 0, 0, 0, 0, 0);
        }

        int visible = 0;
        int available = 0;
        int disabled = 0;
        int draft = 0;
        int ok = 0;
        int warnings = 0;
        int errors = 0;
        for (int i = 0; i < entries.Count; i++)
        {
            ContentWorkbenchEntry entry = entries[i];
            if (entry == null)
            {
                continue;
            }

            if (filters.Passes(entry))
            {
                visible++;
            }

            switch (entry.Availability)
            {
                case ContentWorkbenchAvailabilityState.Available:
                    available++;
                    break;
                case ContentWorkbenchAvailabilityState.Disabled:
                    disabled++;
                    break;
                case ContentWorkbenchAvailabilityState.Draft:
                    draft++;
                    break;
            }

            if (entry.ErrorCount > 0)
            {
                errors++;
            }
            else if (entry.WarningCount > 0)
            {
                warnings++;
            }
            else
            {
                ok++;
            }
        }

        return new ContentWorkbenchDomainSummary(entries.Count, visible, available, disabled, draft, ok, warnings, errors);
    }
}

public readonly struct ContentWorkbenchCreateTemplateInfo
{
    public ContentWorkbenchCreateTemplateInfo(ContentWorkbenchCreateTemplate template, string title, string description)
    {
        Template = template;
        Title = title ?? template.ToString();
        Description = description ?? string.Empty;
    }

    public ContentWorkbenchCreateTemplate Template { get; }
    public string Title { get; }
    public string Description { get; }

    public static IReadOnlyList<ContentWorkbenchCreateTemplateInfo> ForDomain(ContentWorkbenchDomain domain)
    {
        return domain switch
        {
            ContentWorkbenchDomain.Weapons => new[]
            {
                new ContentWorkbenchCreateTemplateInfo(ContentWorkbenchCreateTemplate.DirectProjectileWeapon, "Direct Projectile", "Single-shot projectile weapon with normal targeting and collision."),
                new ContentWorkbenchCreateTemplateInfo(ContentWorkbenchCreateTemplate.SpreadProjectileWeapon, "Spread Projectile", "Projectile weapon that can emit a fan or pattern."),
                new ContentWorkbenchCreateTemplateInfo(ContentWorkbenchCreateTemplate.ExplosiveArcWeapon, "Explosive Arc", "Arcing projectile weapon with splash authoring."),
                new ContentWorkbenchCreateTemplateInfo(ContentWorkbenchCreateTemplate.BounceWeapon, "Bounce", "Projectile weapon prepared for ricochet upgrades."),
                new ContentWorkbenchCreateTemplateInfo(ContentWorkbenchCreateTemplate.ChainWeapon, "Chain", "Projectile weapon prepared for secondary chained hits."),
                new ContentWorkbenchCreateTemplateInfo(ContentWorkbenchCreateTemplate.OrbitWeapon, "Orbit", "Persistent orbit weapon that follows the player.")
            },
            ContentWorkbenchDomain.Items => new[]
            {
                new ContentWorkbenchCreateTemplateInfo(ContentWorkbenchCreateTemplate.PassiveItem, "Passive Item", "World-loot item with stack rules and stat modifiers.")
            },
            ContentWorkbenchDomain.Chests => new[]
            {
                new ContentWorkbenchCreateTemplateInfo(ContentWorkbenchCreateTemplate.Chest, "Chest", "World interactable that spends Tickets if configured and grants one item.")
            },
            ContentWorkbenchDomain.Performers => new[]
            {
                new ContentWorkbenchCreateTemplateInfo(ContentWorkbenchCreateTemplate.Performer, "Performer", "Playable performer with starting weapon, stats, and talent pool.")
            },
            ContentWorkbenchDomain.Enemies => new[]
            {
                new ContentWorkbenchCreateTemplateInfo(ContentWorkbenchCreateTemplate.EnemyFromNormal, "Enemy From Normal", "Duplicate the normal enemy profile as a safe starting point.")
            },
            ContentWorkbenchDomain.Headliners => new[]
            {
                new ContentWorkbenchCreateTemplateInfo(ContentWorkbenchCreateTemplate.Headliner, "Headliner", "Act-ending boss entry that references an enemy actor profile.")
            },
            ContentWorkbenchDomain.Upgrades => new[]
            {
                new ContentWorkbenchCreateTemplateInfo(ContentWorkbenchCreateTemplate.WeaponStatUpgrade, "Owned Weapon Upgrade", "Rarity-scaled card that improves one owned weapon stat.")
            },
            ContentWorkbenchDomain.Talents => new[]
            {
                new ContentWorkbenchCreateTemplateInfo(ContentWorkbenchCreateTemplate.SharedTalent, "Shared Talent", "Rarity-scaled stat talent available to shared pools."),
                new ContentWorkbenchCreateTemplateInfo(ContentWorkbenchCreateTemplate.PerformerTalent, "Performer Talent", "Rarity-scaled stat talent locked to one performer.")
            },
            _ => Array.Empty<ContentWorkbenchCreateTemplateInfo>()
        };
    }
}

public readonly struct ContentWorkbenchStatOption
{
    public ContentWorkbenchStatOption(StatId statId, string label)
    {
        StatId = statId;
        Label = label ?? statId.ToString();
    }

    public StatId StatId { get; }
    public string Label { get; }
}

public static class ContentWorkbenchStatOptions
{
    public static IReadOnlyList<ContentWorkbenchStatOption> ForDomain(ContentWorkbenchDomain domain)
    {
        var options = new List<ContentWorkbenchStatOption>();
        foreach (StatId statId in Enum.GetValues(typeof(StatId)))
        {
            if (!IsAllowed(domain, statId))
            {
                continue;
            }

            options.Add(new ContentWorkbenchStatOption(statId, ContentModifierDisplayRules.StatDisplayName(statId)));
        }

        return options;
    }

    public static IReadOnlyList<ContentWorkbenchStatOption> ForWeaponUpgrade(WeaponDefinition weapon)
    {
        if (weapon == null)
        {
            return ForDomain(ContentWorkbenchDomain.Upgrades);
        }

        var options = new List<ContentWorkbenchStatOption>();
        IReadOnlyList<StatId> supportedStats = weapon.SupportedUpgradeStats;
        if (supportedStats != null && supportedStats.Count > 0)
        {
            var seen = new HashSet<StatId>();
            for (int i = 0; i < supportedStats.Count; i++)
            {
                StatId statId = supportedStats[i];
                if (!seen.Add(statId) || !IsAllowedForWeaponUpgrade(weapon, statId))
                {
                    continue;
                }

                options.Add(new ContentWorkbenchStatOption(statId, ContentModifierDisplayRules.StatDisplayName(statId)));
            }
        }

        if (options.Count > 0)
        {
            return options;
        }

        IReadOnlyList<ContentWorkbenchStatOption> allWeaponStats = ForDomain(ContentWorkbenchDomain.Upgrades);
        for (int i = 0; i < allWeaponStats.Count; i++)
        {
            if (IsAllowedForWeaponUpgrade(weapon, allWeaponStats[i].StatId))
            {
                options.Add(allWeaponStats[i]);
            }
        }

        return options.Count > 0 ? options : allWeaponStats;
    }

    public static bool IsAllowed(ContentWorkbenchDomain domain, StatId statId)
    {
        return domain switch
        {
            ContentWorkbenchDomain.Upgrades => ContentStatValidationRules.IsWeaponUpgradeStat(statId),
            ContentWorkbenchDomain.Talents => ContentStatValidationRules.IsTalentStat(statId),
            ContentWorkbenchDomain.Items => ContentStatValidationRules.IsTalentStat(statId),
            _ => Enum.IsDefined(typeof(StatId), statId)
        };
    }

    public static bool IsAllowedForWeaponUpgrade(WeaponDefinition weapon, StatId statId)
    {
        return IsAllowed(ContentWorkbenchDomain.Upgrades, statId)
            && (weapon == null || weapon.SupportsUpgradeStat(statId));
    }

    public static bool Contains(IReadOnlyList<ContentWorkbenchStatOption> options, StatId statId)
    {
        if (options == null)
        {
            return false;
        }

        for (int i = 0; i < options.Count; i++)
        {
            if (options[i].StatId == statId)
            {
                return true;
            }
        }

        return false;
    }

    public static StatId FirstOrFallback(IReadOnlyList<ContentWorkbenchStatOption> options, StatId fallback)
    {
        return options != null && options.Count > 0 ? options[0].StatId : fallback;
    }

    public static string UnsupportedStatMessage(ContentWorkbenchDomain domain, StatId statId, WeaponDefinition weapon = null)
    {
        string statName = ContentModifierDisplayRules.StatDisplayName(statId);
        if (domain == ContentWorkbenchDomain.Upgrades)
        {
            if (!ContentStatValidationRules.IsWeaponUpgradeStat(statId))
            {
                if (WeaponDefinition.IsWeaponScopedStat(statId))
                {
                    return $"{statName} is internal or not designer-selectable and cannot be used by Weapon Upgrades.";
                }

                return $"{statName} is not a weapon-local stat and cannot be used by Weapon Upgrades.";
            }

            if (weapon != null && !weapon.SupportsUpgradeStat(statId))
            {
                string weaponName = string.IsNullOrWhiteSpace(weapon.DisplayName) ? weapon.Id : weapon.DisplayName;
                return $"{statName} is not supported by target weapon {weaponName}.";
            }
        }

        if ((domain == ContentWorkbenchDomain.Talents || domain == ContentWorkbenchDomain.Items || domain == ContentWorkbenchDomain.Performers)
            && WeaponDefinition.IsWeaponScopedStat(statId))
        {
            return $"{statName} is weapon-local and cannot be used by {ContentWorkbenchDomainLayout.DisplayName(domain)}.";
        }

        return $"{statName} is not supported by {ContentWorkbenchDomainLayout.DisplayName(domain)}.";
    }

    public static StatId EnsureAllowed(ContentWorkbenchDomain domain, StatId statId)
    {
        if (IsAllowed(domain, statId))
        {
            return statId;
        }

        IReadOnlyList<ContentWorkbenchStatOption> options = ForDomain(domain);
        return options.Count > 0 ? options[0].StatId : statId;
    }

    public static StatId EnsureAllowedForWeaponUpgrade(WeaponDefinition weapon, StatId statId)
    {
        if (IsAllowedForWeaponUpgrade(weapon, statId))
        {
            return statId;
        }

        IReadOnlyList<ContentWorkbenchStatOption> options = ForWeaponUpgrade(weapon);
        return options.Count > 0 ? options[0].StatId : statId;
    }
}

public sealed class ContentWorkbenchUsageContext
{
    public WeaponCatalog WeaponCatalog;
    public ItemCatalog ItemCatalog;
    public ChestCatalog ChestCatalog;
    public PerformerCatalog PerformerCatalog;
    public UpgradeCatalog UpgradeCatalog;
    public TalentCatalog TalentCatalog;
    public EnemyCatalog EnemyCatalog;
    public HeadlinerCatalog HeadlinerCatalog;

    public static ContentWorkbenchUsageContext LoadFromProject(ContentWorkbenchCatalogPathOverrides overrides = null)
    {
        return new ContentWorkbenchUsageContext
        {
            WeaponCatalog = ContentWorkbenchService.LoadCatalog(ContentWorkbenchDomain.Weapons, overrides?.For(ContentWorkbenchDomain.Weapons)) as WeaponCatalog,
            ItemCatalog = ContentWorkbenchService.LoadCatalog(ContentWorkbenchDomain.Items, overrides?.For(ContentWorkbenchDomain.Items)) as ItemCatalog,
            ChestCatalog = ContentWorkbenchService.LoadCatalog(ContentWorkbenchDomain.Chests, overrides?.For(ContentWorkbenchDomain.Chests)) as ChestCatalog,
            PerformerCatalog = ContentWorkbenchService.LoadCatalog(ContentWorkbenchDomain.Performers, overrides?.For(ContentWorkbenchDomain.Performers)) as PerformerCatalog,
            UpgradeCatalog = ContentWorkbenchService.LoadCatalog(ContentWorkbenchDomain.Upgrades, overrides?.For(ContentWorkbenchDomain.Upgrades)) as UpgradeCatalog,
            TalentCatalog = ContentWorkbenchService.LoadCatalog(ContentWorkbenchDomain.Talents, overrides?.For(ContentWorkbenchDomain.Talents)) as TalentCatalog,
            EnemyCatalog = ContentWorkbenchService.LoadCatalog(ContentWorkbenchDomain.Enemies, overrides?.For(ContentWorkbenchDomain.Enemies)) as EnemyCatalog,
            HeadlinerCatalog = ContentWorkbenchService.LoadCatalog(ContentWorkbenchDomain.Headliners, overrides?.For(ContentWorkbenchDomain.Headliners)) as HeadlinerCatalog
        };
    }
}

public sealed class ContentWorkbenchUsageSummary
{
    private ContentWorkbenchUsageSummary(string title, List<string> lines, MessageType messageType)
    {
        Title = title ?? "Usage";
        Lines = lines ?? new List<string>();
        MessageType = messageType;
    }

    public string Title { get; }
    public IReadOnlyList<string> Lines { get; }
    public MessageType MessageType { get; }

    public static ContentWorkbenchUsageSummary BuildFromProject(ContentWorkbenchDomain domain, UnityEngine.Object asset)
    {
        return Build(domain, asset, ContentWorkbenchUsageContext.LoadFromProject());
    }

    public static ContentWorkbenchUsageSummary Build(ContentWorkbenchDomain domain, UnityEngine.Object asset, ContentWorkbenchUsageContext context)
    {
        var lines = new List<string>();
        if (asset == null)
        {
            lines.Add("No content selected.");
            return new ContentWorkbenchUsageSummary("Usage", lines, MessageType.Info);
        }

        context ??= new ContentWorkbenchUsageContext();
        string title = asset is IContentDefinition definition ? $"Usage: {definition.DisplayName}" : "Usage";
        switch (domain)
        {
            case ContentWorkbenchDomain.Weapons:
                BuildWeaponUsage(asset as WeaponDefinition, context, lines);
                break;
            case ContentWorkbenchDomain.Items:
                BuildItemUsage(asset as ItemDefinition, context, lines);
                break;
            case ContentWorkbenchDomain.Chests:
                lines.Add("Chest reward source for world interactions.");
                break;
            case ContentWorkbenchDomain.Performers:
                BuildPerformerUsage(asset as PerformerDefinition, context, lines);
                break;
            case ContentWorkbenchDomain.Talents:
                BuildTalentUsage(asset as TalentDefinition, context, lines);
                break;
            case ContentWorkbenchDomain.Upgrades:
                BuildUpgradeUsage(asset as UpgradeDefinition, context, lines);
                break;
            case ContentWorkbenchDomain.Enemies:
                BuildEnemyUsage(asset as EnemyDefinition, context, lines);
                break;
            case ContentWorkbenchDomain.Headliners:
                BuildHeadlinerUsage(asset as HeadlinerDefinition, context, lines);
                break;
            default:
                lines.Add("No usage model for this domain.");
                break;
        }

        if (lines.Count == 0)
        {
            lines.Add("No direct references found.");
        }

        return new ContentWorkbenchUsageSummary(title, lines, MessageType.Info);
    }

    private static void BuildWeaponUsage(WeaponDefinition weapon, ContentWorkbenchUsageContext context, List<string> lines)
    {
        if (weapon == null)
        {
            lines.Add("Selected asset is not a weapon definition.");
            return;
        }

        AddYesNoLine(lines, "In available weapon catalog", Contains(context.WeaponCatalog?.AvailableWeapons, weapon));
        AddYesNoLine(lines, "Starting loadout", Contains(context.WeaponCatalog?.StartingWeapons, weapon));
        AddYesNoLine(lines, "Appears as level-up weapon", weapon.CanAppearAsLevelUpWeapon);
        AddLine(lines, "Level-up card text", weapon.AddWeaponShortDescription);
        AddLine(lines, "Starter for performers", Names(PerformersStartingWith(context.PerformerCatalog?.Performers, weapon)));
        AddLine(lines, "Owned upgrade cards", Names(UpgradesTargetingWeapon(context.UpgradeCatalog?.Upgrades, weapon)));
        AddLine(lines, "Weapon track cards", Names(weapon.UpgradeTrack));
        AddLine(lines, "Supported stats", StatNames(weapon.SupportedUpgradeStats));
    }

    private static void BuildItemUsage(ItemDefinition item, ContentWorkbenchUsageContext context, List<string> lines)
    {
        if (item == null)
        {
            lines.Add("Selected asset is not an item definition.");
            return;
        }

        AddYesNoLine(lines, "In item catalog", Contains(context.ItemCatalog?.Items, item));
        AddLine(lines, "Stacking", item.StackSummary);
        AddLine(lines, "Upside stats", ModifierNames(item.statModifiers));
        AddLine(lines, "Downside stats", item.HasDownside ? ModifierNames(item.downsideStatModifiers) : "None");
    }

    private static void BuildPerformerUsage(PerformerDefinition performer, ContentWorkbenchUsageContext context, List<string> lines)
    {
        if (performer == null)
        {
            lines.Add("Selected asset is not a performer definition.");
            return;
        }

        AddYesNoLine(lines, "In performer catalog", Contains(context.PerformerCatalog?.Performers, performer));
        AddYesNoLine(lines, "Fallback performer", ReferenceEquals(context.PerformerCatalog?.FallbackPerformer, performer));
        AddLine(lines, "Starting weapon", Name(performer.startingWeapon));
        AddLine(lines, "Eligible talents", Names(TalentsAvailableToPerformer(context.TalentCatalog?.Talents, performer)));
    }

    private static void BuildTalentUsage(TalentDefinition talent, ContentWorkbenchUsageContext context, List<string> lines)
    {
        if (talent == null)
        {
            lines.Add("Selected asset is not a talent definition.");
            return;
        }

        AddYesNoLine(lines, "In talent catalog", Contains(context.TalentCatalog?.Talents, talent));
        AddLine(lines, "Pool", talent.PoolSummary);
        AddLine(lines, "Eligible performers", Names(PerformersUsingTalent(context.PerformerCatalog?.Performers, talent)));
        AddLine(lines, "Affected stats", ModifierNames(talent.statModifiers));
    }

    private static void BuildUpgradeUsage(UpgradeDefinition upgrade, ContentWorkbenchUsageContext context, List<string> lines)
    {
        if (upgrade == null)
        {
            lines.Add("Selected asset is not an upgrade definition.");
            return;
        }

        AddYesNoLine(lines, "In upgrade catalog", Contains(context.UpgradeCatalog?.Upgrades, upgrade));
        AddLine(lines, "Target weapon", Name(upgrade.weaponDefinition));
        AddYesNoLine(lines, "Listed on weapon track", upgrade.weaponDefinition != null && Contains(upgrade.weaponDefinition.UpgradeTrack, upgrade));
        AddLine(lines, "Affected stats", ModifierNames(upgrade.statModifiers));
    }

    private static void BuildEnemyUsage(EnemyDefinition enemy, ContentWorkbenchUsageContext context, List<string> lines)
    {
        if (enemy == null)
        {
            lines.Add("Selected asset is not an enemy definition.");
            return;
        }

        AddYesNoLine(lines, "In enemy catalog", Contains(context.EnemyCatalog?.Enemies, enemy));
        AddYesNoLine(lines, "Default spawn enemy", ReferenceEquals(context.EnemyCatalog?.DefaultEnemy, enemy));
        AddLine(lines, "Spawn weight", enemy.spawnWeight.ToString("0.##"));
        AddLine(lines, "XP budget", $"{enemy.xpBudget} ({enemy.dropStyle})");
    }

    private static void BuildHeadlinerUsage(HeadlinerDefinition headliner, ContentWorkbenchUsageContext context, List<string> lines)
    {
        if (headliner == null)
        {
            lines.Add("Selected asset is not a Headliner definition.");
            return;
        }

        AddYesNoLine(lines, "In Headliner catalog", Contains(context.HeadlinerCatalog?.Headliners, headliner));
        AddYesNoLine(lines, "Default Headliner", ReferenceEquals(context.HeadlinerCatalog?.DefaultHeadliner, headliner));
        AddLine(lines, "Act", headliner.actNumber.ToString());
        AddLine(lines, "Enemy actor", Name(headliner.enemyActor));
    }

    private static List<PerformerDefinition> PerformersStartingWith(IReadOnlyList<PerformerDefinition> performers, WeaponDefinition weapon)
    {
        var results = new List<PerformerDefinition>();
        if (performers == null || weapon == null)
        {
            return results;
        }

        for (int i = 0; i < performers.Count; i++)
        {
            PerformerDefinition performer = performers[i];
            if (performer != null && ReferenceEquals(performer.startingWeapon, weapon))
            {
                results.Add(performer);
            }
        }

        return results;
    }

    private static List<PerformerDefinition> PerformersUsingTalent(IReadOnlyList<PerformerDefinition> performers, TalentDefinition talent)
    {
        var results = new List<PerformerDefinition>();
        if (performers == null || talent == null)
        {
            return results;
        }

        for (int i = 0; i < performers.Count; i++)
        {
            PerformerDefinition performer = performers[i];
            if (performer == null)
            {
                continue;
            }

            if (talent.AllowsPerformer(performer))
            {
                results.Add(performer);
            }
        }

        return results;
    }

    private static List<TalentDefinition> TalentsAvailableToPerformer(IReadOnlyList<TalentDefinition> talents, PerformerDefinition performer)
    {
        var results = new List<TalentDefinition>();
        if (talents == null || performer == null)
        {
            return results;
        }

        for (int i = 0; i < talents.Count; i++)
        {
            TalentDefinition talent = talents[i];
            if (talent != null && talent.AllowsPerformer(performer))
            {
                results.Add(talent);
            }
        }

        return results;
    }

    private static List<UpgradeDefinition> UpgradesTargetingWeapon(IReadOnlyList<UpgradeDefinition> upgrades, WeaponDefinition weapon)
    {
        var results = new List<UpgradeDefinition>();
        if (upgrades == null || weapon == null)
        {
            return results;
        }

        for (int i = 0; i < upgrades.Count; i++)
        {
            UpgradeDefinition upgrade = upgrades[i];
            if (upgrade == null)
            {
                continue;
            }

            bool referencesDefinition = ReferenceEquals(upgrade.weaponDefinition, weapon);
            bool referencesId = !string.IsNullOrWhiteSpace(upgrade.weaponId) && string.Equals(upgrade.weaponId, weapon.Id, StringComparison.Ordinal);
            if (referencesDefinition || referencesId)
            {
                results.Add(upgrade);
            }
        }

        return results;
    }

    private static bool Contains<T>(IReadOnlyList<T> list, T value)
        where T : class
    {
        if (list == null || value == null)
        {
            return false;
        }

        for (int i = 0; i < list.Count; i++)
        {
            if (ReferenceEquals(list[i], value))
            {
                return true;
            }
        }

        return false;
    }

    private static string Names<T>(IReadOnlyList<T> definitions)
        where T : class, IContentDefinition
    {
        if (definitions == null || definitions.Count == 0)
        {
            return "None";
        }

        var names = new List<string>();
        for (int i = 0; i < definitions.Count; i++)
        {
            if (definitions[i] != null)
            {
                names.Add(definitions[i].DisplayName);
            }
        }

        return names.Count > 0 ? string.Join(", ", names) : "None";
    }

    private static string Name(IContentDefinition definition)
    {
        return definition != null ? definition.DisplayName : "Missing";
    }

    private static string StatNames(IReadOnlyList<StatId> stats)
    {
        if (stats == null || stats.Count == 0)
        {
            return "None";
        }

        var names = new List<string>();
        for (int i = 0; i < stats.Count; i++)
        {
            StatId stat = stats[i];
            if (Enum.IsDefined(typeof(StatId), stat))
            {
                names.Add(StatMetadata.Get(stat).DisplayName);
            }
        }

        return names.Count > 0 ? string.Join(", ", names) : "None";
    }

    private static string ModifierNames(IReadOnlyList<UpgradeStatModifierDefinition> modifiers)
    {
        if (modifiers == null || modifiers.Count == 0)
        {
            return "None";
        }

        var names = new List<string>();
        for (int i = 0; i < modifiers.Count; i++)
        {
            names.Add(ContentModifierDisplayRules.Summary(modifiers[i]));
        }

        return string.Join(", ", names);
    }

    private static string ModifierNames(IReadOnlyList<ItemStatModifierDefinition> modifiers)
    {
        if (modifiers == null || modifiers.Count == 0)
        {
            return "None";
        }

        var names = new List<string>();
        for (int i = 0; i < modifiers.Count; i++)
        {
            names.Add(ContentModifierDisplayRules.Summary(modifiers[i]));
        }

        return string.Join(", ", names);
    }

    private static void AddYesNoLine(List<string> lines, string label, bool value)
    {
        AddLine(lines, label, value ? "Yes" : "No");
    }

    private static void AddLine(List<string> lines, string label, string value)
    {
        lines.Add($"{label}: {value}");
    }
}

public interface IContentWorkbenchDomainModule
{
    ContentWorkbenchDomain Domain { get; }
    string Folder { get; }
    string CatalogPath { get; }
    Type DefinitionType { get; }
    Type CatalogType { get; }
    string TypeFilter { get; }
    ContentWorkbenchCreateTemplate DefaultTemplate { get; }

    bool IsTemplateValid(ContentWorkbenchCreateTemplate template);
    IReadOnlyList<IContentDefinition> CatalogDefinitions(UnityEngine.Object catalog);
    IReadOnlyList<ContentValidationIssue> ValidateCatalog(UnityEngine.Object catalog);
    bool IsCataloged(UnityEngine.Object catalog, UnityEngine.Object asset);
    bool AddToCatalog(UnityEngine.Object catalog, UnityEngine.Object asset);
    bool RemoveFromCatalog(UnityEngine.Object catalog, UnityEngine.Object asset);
    bool TrySetActive(UnityEngine.Object asset, bool active);
    void AssignIdentityAndActive(UnityEngine.Object asset, string id, string displayName);
}

public static class ContentWorkbenchDomainRegistry
{
    private static readonly IContentWorkbenchDomainModule[] Modules =
    {
        new WeaponContentWorkbenchDomainModule(),
        new ItemContentWorkbenchDomainModule(),
        new ChestContentWorkbenchDomainModule(),
        new PerformerContentWorkbenchDomainModule(),
        new EnemyContentWorkbenchDomainModule(),
        new HeadlinerContentWorkbenchDomainModule(),
        new UpgradeContentWorkbenchDomainModule(),
        new TalentContentWorkbenchDomainModule()
    };

    public static IReadOnlyList<IContentWorkbenchDomainModule> All => Modules;

    public static IContentWorkbenchDomainModule For(ContentWorkbenchDomain domain)
    {
        for (int i = 0; i < Modules.Length; i++)
        {
            if (Modules[i].Domain == domain)
            {
                return Modules[i];
            }
        }

        return UnknownContentWorkbenchDomainModule.Instance;
    }
}

internal abstract class ContentWorkbenchDomainModule<TDefinition, TCatalog> : IContentWorkbenchDomainModule
    where TDefinition : UnityEngine.Object, IContentDefinition
    where TCatalog : UnityEngine.Object
{
    protected ContentWorkbenchDomainModule(
        ContentWorkbenchDomain domain,
        string folder,
        string catalogPath,
        string typeFilter,
        ContentWorkbenchCreateTemplate defaultTemplate)
    {
        Domain = domain;
        Folder = folder;
        CatalogPath = catalogPath;
        TypeFilter = typeFilter;
        DefaultTemplate = defaultTemplate;
    }

    public ContentWorkbenchDomain Domain { get; }
    public string Folder { get; }
    public string CatalogPath { get; }
    public Type DefinitionType => typeof(TDefinition);
    public Type CatalogType => typeof(TCatalog);
    public string TypeFilter { get; }
    public ContentWorkbenchCreateTemplate DefaultTemplate { get; }

    public abstract bool IsTemplateValid(ContentWorkbenchCreateTemplate template);
    protected abstract List<TDefinition> MutableDefinitions(TCatalog catalog);
    protected abstract IReadOnlyList<ContentValidationIssue> Validate(TCatalog catalog);
    protected abstract void AssignIdentity(TDefinition definition, string id, string displayName);

    public IReadOnlyList<IContentDefinition> CatalogDefinitions(UnityEngine.Object catalog)
    {
        if (catalog is not TCatalog typedCatalog)
        {
            return null;
        }

        List<TDefinition> definitions = MutableDefinitions(typedCatalog);
        if (definitions == null)
        {
            return null;
        }

        var results = new List<IContentDefinition>(definitions.Count);
        for (int i = 0; i < definitions.Count; i++)
        {
            results.Add(definitions[i]);
        }

        return results;
    }

    public IReadOnlyList<ContentValidationIssue> ValidateCatalog(UnityEngine.Object catalog)
    {
        return catalog is TCatalog typedCatalog ? Validate(typedCatalog) : Array.Empty<ContentValidationIssue>();
    }

    public bool IsCataloged(UnityEngine.Object catalog, UnityEngine.Object asset)
    {
        return catalog is TCatalog typedCatalog
            && asset is TDefinition definition
            && (MutableDefinitions(typedCatalog)?.Contains(definition) ?? false);
    }

    public bool AddToCatalog(UnityEngine.Object catalog, UnityEngine.Object asset)
    {
        if (catalog is not TCatalog typedCatalog || asset is not TDefinition definition)
        {
            return false;
        }

        List<TDefinition> definitions = MutableDefinitions(typedCatalog);
        if (definitions == null || definitions.Contains(definition))
        {
            return false;
        }

        definitions.Add(definition);
        return true;
    }

    public bool RemoveFromCatalog(UnityEngine.Object catalog, UnityEngine.Object asset)
    {
        List<TDefinition> definitions = catalog is TCatalog typedCatalog ? MutableDefinitions(typedCatalog) : null;
        if (definitions == null)
        {
            return false;
        }

        string assetPath = asset != null ? AssetDatabase.GetAssetPath(asset) : string.Empty;
        bool changed = false;
        for (int i = definitions.Count - 1; i >= 0; i--)
        {
            TDefinition current = definitions[i];
            string currentPath = current != null ? AssetDatabase.GetAssetPath(current) : string.Empty;
            bool sameAsset = current == null
                || ReferenceEquals(current, asset)
                || (!string.IsNullOrWhiteSpace(assetPath) && string.Equals(currentPath, assetPath, StringComparison.Ordinal));
            if (!sameAsset)
            {
                continue;
            }

            definitions.RemoveAt(i);
            changed = true;
        }

        return changed;
    }

    public bool TrySetActive(UnityEngine.Object asset, bool active)
    {
        if (asset is not TDefinition definition)
        {
            return false;
        }

        switch (definition)
        {
            case WeaponDefinition weapon:
                weapon.isActive = active;
                return true;
            case ItemDefinition item:
                item.isActive = active;
                return true;
            case ChestDefinition chest:
                chest.isActive = active;
                return true;
            case PerformerDefinition performer:
                performer.isActive = active;
                return true;
            case EnemyDefinition enemy:
                enemy.isActive = active;
                return true;
            case HeadlinerDefinition headliner:
                headliner.isActive = active;
                return true;
            case UpgradeDefinition upgrade:
                upgrade.isActive = active;
                return true;
            case TalentDefinition talent:
                talent.isActive = active;
                return true;
            default:
                return false;
        }
    }

    public void AssignIdentityAndActive(UnityEngine.Object asset, string id, string displayName)
    {
        if (asset is not TDefinition definition)
        {
            return;
        }

        AssignIdentity(definition, id, displayName);
        TrySetActive(definition, true);
    }
}

internal sealed class WeaponContentWorkbenchDomainModule : ContentWorkbenchDomainModule<WeaponDefinition, WeaponCatalog>
{
    public WeaponContentWorkbenchDomainModule()
        : base(ContentWorkbenchDomain.Weapons, TheCircussyOneAssetPaths.WeaponBalanceFolder, TheCircussyOneAssetPaths.WeaponCatalogPath, "t:WeaponDefinition", ContentWorkbenchCreateTemplate.DirectProjectileWeapon)
    {
    }

    public override bool IsTemplateValid(ContentWorkbenchCreateTemplate template)
    {
        return template is ContentWorkbenchCreateTemplate.DirectProjectileWeapon
            or ContentWorkbenchCreateTemplate.SpreadProjectileWeapon
            or ContentWorkbenchCreateTemplate.ExplosiveArcWeapon
            or ContentWorkbenchCreateTemplate.BounceWeapon
            or ContentWorkbenchCreateTemplate.ChainWeapon
            or ContentWorkbenchCreateTemplate.OrbitWeapon;
    }

    protected override List<WeaponDefinition> MutableDefinitions(WeaponCatalog catalog) => catalog?.availableWeapons;
    protected override IReadOnlyList<ContentValidationIssue> Validate(WeaponCatalog catalog) => catalog.ValidateContent();

    protected override void AssignIdentity(WeaponDefinition definition, string id, string displayName)
    {
        definition.weaponId = id;
        definition.displayName = displayName;
        definition.EnsureWorkflowDefaults();
    }
}

internal sealed class ItemContentWorkbenchDomainModule : ContentWorkbenchDomainModule<ItemDefinition, ItemCatalog>
{
    public ItemContentWorkbenchDomainModule()
        : base(ContentWorkbenchDomain.Items, TheCircussyOneAssetPaths.ItemBalanceFolder, TheCircussyOneAssetPaths.ItemCatalogPath, "t:ItemDefinition", ContentWorkbenchCreateTemplate.PassiveItem)
    {
    }

    public override bool IsTemplateValid(ContentWorkbenchCreateTemplate template) => template == ContentWorkbenchCreateTemplate.PassiveItem;
    protected override List<ItemDefinition> MutableDefinitions(ItemCatalog catalog) => catalog?.items;
    protected override IReadOnlyList<ContentValidationIssue> Validate(ItemCatalog catalog) => catalog.ValidateContent();

    protected override void AssignIdentity(ItemDefinition definition, string id, string displayName)
    {
        definition.itemId = id;
        definition.displayName = displayName;
        definition.EnsureWorkflowDefaults();
    }
}

internal sealed class ChestContentWorkbenchDomainModule : ContentWorkbenchDomainModule<ChestDefinition, ChestCatalog>
{
    public ChestContentWorkbenchDomainModule()
        : base(ContentWorkbenchDomain.Chests, TheCircussyOneAssetPaths.ChestBalanceFolder, TheCircussyOneAssetPaths.ChestCatalogPath, "t:ChestDefinition", ContentWorkbenchCreateTemplate.Chest)
    {
    }

    public override bool IsTemplateValid(ContentWorkbenchCreateTemplate template) => template == ContentWorkbenchCreateTemplate.Chest;
    protected override List<ChestDefinition> MutableDefinitions(ChestCatalog catalog) => catalog?.chests;
    protected override IReadOnlyList<ContentValidationIssue> Validate(ChestCatalog catalog) => catalog.ValidateContent();

    protected override void AssignIdentity(ChestDefinition definition, string id, string displayName)
    {
        definition.chestId = id;
        definition.displayName = displayName;
        definition.EnsureWorkflowDefaults();
    }
}

internal sealed class PerformerContentWorkbenchDomainModule : ContentWorkbenchDomainModule<PerformerDefinition, PerformerCatalog>
{
    public PerformerContentWorkbenchDomainModule()
        : base(ContentWorkbenchDomain.Performers, TheCircussyOneAssetPaths.PerformerBalanceFolder, TheCircussyOneAssetPaths.PerformerCatalogPath, "t:PerformerDefinition", ContentWorkbenchCreateTemplate.Performer)
    {
    }

    public override bool IsTemplateValid(ContentWorkbenchCreateTemplate template) => template == ContentWorkbenchCreateTemplate.Performer;
    protected override List<PerformerDefinition> MutableDefinitions(PerformerCatalog catalog) => catalog?.performers;
    protected override IReadOnlyList<ContentValidationIssue> Validate(PerformerCatalog catalog) => catalog.ValidateContent();

    protected override void AssignIdentity(PerformerDefinition definition, string id, string displayName)
    {
        definition.performerId = id;
        definition.displayName = displayName;
        definition.EnsureWorkflowDefaults();
    }
}

internal sealed class EnemyContentWorkbenchDomainModule : ContentWorkbenchDomainModule<EnemyDefinition, EnemyCatalog>
{
    public EnemyContentWorkbenchDomainModule()
        : base(ContentWorkbenchDomain.Enemies, TheCircussyOneAssetPaths.EnemyBalanceFolder, TheCircussyOneAssetPaths.EnemyCatalogPath, "t:EnemyDefinition", ContentWorkbenchCreateTemplate.EnemyFromNormal)
    {
    }

    public override bool IsTemplateValid(ContentWorkbenchCreateTemplate template) => template == ContentWorkbenchCreateTemplate.EnemyFromNormal;
    protected override List<EnemyDefinition> MutableDefinitions(EnemyCatalog catalog) => catalog?.enemies;
    protected override IReadOnlyList<ContentValidationIssue> Validate(EnemyCatalog catalog) => catalog.ValidateContent();

    protected override void AssignIdentity(EnemyDefinition definition, string id, string displayName)
    {
        definition.enemyId = id;
        definition.displayName = displayName;
        definition.EnsureWorkflowDefaults();
    }
}

internal sealed class HeadlinerContentWorkbenchDomainModule : ContentWorkbenchDomainModule<HeadlinerDefinition, HeadlinerCatalog>
{
    public HeadlinerContentWorkbenchDomainModule()
        : base(ContentWorkbenchDomain.Headliners, TheCircussyOneAssetPaths.HeadlinerBalanceFolder, TheCircussyOneAssetPaths.HeadlinerCatalogPath, "t:HeadlinerDefinition", ContentWorkbenchCreateTemplate.Headliner)
    {
    }

    public override bool IsTemplateValid(ContentWorkbenchCreateTemplate template) => template == ContentWorkbenchCreateTemplate.Headliner;
    protected override List<HeadlinerDefinition> MutableDefinitions(HeadlinerCatalog catalog) => catalog?.headliners;
    protected override IReadOnlyList<ContentValidationIssue> Validate(HeadlinerCatalog catalog) => catalog.ValidateContent();

    protected override void AssignIdentity(HeadlinerDefinition definition, string id, string displayName)
    {
        definition.headlinerId = id;
        definition.displayName = displayName;
        definition.EnsureWorkflowDefaults();
    }
}

internal sealed class UpgradeContentWorkbenchDomainModule : ContentWorkbenchDomainModule<UpgradeDefinition, UpgradeCatalog>
{
    public UpgradeContentWorkbenchDomainModule()
        : base(ContentWorkbenchDomain.Upgrades, TheCircussyOneAssetPaths.UpgradeBalanceFolder, TheCircussyOneAssetPaths.UpgradeCatalogPath, "t:UpgradeDefinition", ContentWorkbenchCreateTemplate.WeaponStatUpgrade)
    {
    }

    public override bool IsTemplateValid(ContentWorkbenchCreateTemplate template) => template == ContentWorkbenchCreateTemplate.WeaponStatUpgrade;
    protected override List<UpgradeDefinition> MutableDefinitions(UpgradeCatalog catalog) => catalog?.upgrades;
    protected override IReadOnlyList<ContentValidationIssue> Validate(UpgradeCatalog catalog) => catalog.ValidateContent();

    protected override void AssignIdentity(UpgradeDefinition definition, string id, string displayName)
    {
        definition.upgradeId = id;
        definition.displayName = displayName;
        definition.EnsureWorkflowDefaults();
    }
}

internal sealed class TalentContentWorkbenchDomainModule : ContentWorkbenchDomainModule<TalentDefinition, TalentCatalog>
{
    public TalentContentWorkbenchDomainModule()
        : base(ContentWorkbenchDomain.Talents, TheCircussyOneAssetPaths.TalentBalanceFolder, TheCircussyOneAssetPaths.TalentCatalogPath, "t:TalentDefinition", ContentWorkbenchCreateTemplate.SharedTalent)
    {
    }

    public override bool IsTemplateValid(ContentWorkbenchCreateTemplate template)
    {
        return template is ContentWorkbenchCreateTemplate.SharedTalent
            or ContentWorkbenchCreateTemplate.PerformerTalent;
    }

    protected override List<TalentDefinition> MutableDefinitions(TalentCatalog catalog) => catalog?.talents;
    protected override IReadOnlyList<ContentValidationIssue> Validate(TalentCatalog catalog) => catalog.ValidateContent();

    protected override void AssignIdentity(TalentDefinition definition, string id, string displayName)
    {
        definition.talentId = id;
        definition.displayName = displayName;
        definition.EnsureWorkflowDefaults();
    }
}

internal sealed class UnknownContentWorkbenchDomainModule : IContentWorkbenchDomainModule
{
    public static readonly UnknownContentWorkbenchDomainModule Instance = new();

    private UnknownContentWorkbenchDomainModule()
    {
    }

    public ContentWorkbenchDomain Domain => default;
    public string Folder => TheCircussyOneAssetPaths.BalanceFolder;
    public string CatalogPath => string.Empty;
    public Type DefinitionType => typeof(UnityEngine.Object);
    public Type CatalogType => typeof(UnityEngine.Object);
    public string TypeFilter => "t:Object";
    public ContentWorkbenchCreateTemplate DefaultTemplate => ContentWorkbenchCreateTemplate.DirectProjectileWeapon;
    public bool IsTemplateValid(ContentWorkbenchCreateTemplate template) => false;
    public IReadOnlyList<IContentDefinition> CatalogDefinitions(UnityEngine.Object catalog) => null;
    public IReadOnlyList<ContentValidationIssue> ValidateCatalog(UnityEngine.Object catalog) => Array.Empty<ContentValidationIssue>();
    public bool IsCataloged(UnityEngine.Object catalog, UnityEngine.Object asset) => false;
    public bool AddToCatalog(UnityEngine.Object catalog, UnityEngine.Object asset) => false;
    public bool RemoveFromCatalog(UnityEngine.Object catalog, UnityEngine.Object asset) => false;
    public bool TrySetActive(UnityEngine.Object asset, bool active) => false;
    public void AssignIdentityAndActive(UnityEngine.Object asset, string id, string displayName) { }
}

public static class ContentWorkbenchService
{
    private readonly struct ResolvedContentIdentity
    {
        public ResolvedContentIdentity(string id, string displayName, string assetPath)
        {
            Id = id;
            DisplayName = displayName;
            AssetPath = assetPath;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public string AssetPath { get; }
    }

    public static IReadOnlyList<ContentWorkbenchEntry> LoadEntries(ContentWorkbenchDomain domain)
    {
        var entries = new List<ContentWorkbenchEntry>();
        string folder = FolderFor(domain);
        if (!AssetDatabase.IsValidFolder(folder))
        {
            return entries;
        }

        UnityEngine.Object catalog = LoadCatalog(domain);
        Dictionary<string, (int Errors, int Warnings)> issueCounts = IssueCountsById(domain, catalog);
        string[] guids = AssetDatabase.FindAssets(TypeFilterFor(domain), new[] { folder });
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath(path, TypeFor(domain));
            if (asset is not IContentDefinition definition)
            {
                continue;
            }

            issueCounts.TryGetValue(definition.Id ?? string.Empty, out (int Errors, int Warnings) counts);
            ContentWorkbenchEntrySubgroupInfo subgroup = ContentWorkbenchEntryGrouping.Resolve(domain, asset);
            entries.Add(new ContentWorkbenchEntry
            {
                Asset = asset,
                Path = path,
                Id = definition.Id,
                DisplayName = definition.DisplayName,
                SubgroupLabel = subgroup.Label,
                SubgroupSortIndex = subgroup.SortIndex,
                IsActive = ContentAvailabilityRules.IsActive(definition),
                IsCataloged = IsCataloged(domain, asset, catalog),
                ErrorCount = counts.Errors,
                WarningCount = counts.Warnings
            });
        }

        entries.Sort((left, right) => string.Compare(left.DisplayName, right.DisplayName, StringComparison.OrdinalIgnoreCase));
        return entries;
    }

    public static ContentWorkbenchIdentityPreview PreviewCreate(ContentWorkbenchCreateRequest request)
    {
        if (request == null)
        {
            return new ContentWorkbenchIdentityPreview(string.Empty, string.Empty, string.Empty);
        }

        ResolvedContentIdentity identity = ResolveCreateIdentity(request);
        return new ContentWorkbenchIdentityPreview(identity.Id, identity.DisplayName, identity.AssetPath);
    }

    public static ContentWorkbenchActionResult SetActive(UnityEngine.Object asset, bool active)
    {
        if (!TrySetActive(asset, active))
        {
            return ContentWorkbenchActionResult.Fail("Selected asset does not support activation.");
        }

        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();
        return ContentWorkbenchActionResult.Ok(active ? "Activated content." : "Deactivated content.", asset);
    }

    public static ContentWorkbenchActionResult MakeAvailable(ContentWorkbenchDomain domain, UnityEngine.Object asset, string catalogPathOverride = null)
    {
        if (!TrySetActive(asset, true))
        {
            return ContentWorkbenchActionResult.Fail("Selected asset does not support availability.");
        }

        ContentWorkbenchActionResult catalogResult = AddToCatalog(domain, asset, catalogPathOverride);
        if (!catalogResult.Success)
        {
            return catalogResult;
        }

        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();
        return ContentWorkbenchActionResult.Ok("Made content available to the game.", asset);
    }

    public static ContentWorkbenchActionResult Disable(UnityEngine.Object asset)
    {
        if (!TrySetActive(asset, false))
        {
            return ContentWorkbenchActionResult.Fail("Selected asset does not support availability.");
        }

        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();
        return ContentWorkbenchActionResult.Ok("Disabled content. It stays in the catalog.", asset);
    }

    public static ContentWorkbenchActionResult MoveToDraft(ContentWorkbenchDomain domain, UnityEngine.Object asset, string catalogPathOverride = null)
    {
        ContentWorkbenchActionResult removeResult = RemoveFromCatalog(domain, asset, catalogPathOverride);
        if (!removeResult.Success)
        {
            return removeResult;
        }

        return ContentWorkbenchActionResult.Ok("Moved content to Draft. The asset was not deleted.", asset);
    }

    public static ContentWorkbenchActionResult AddToCatalog(ContentWorkbenchDomain domain, UnityEngine.Object asset, string catalogPathOverride = null)
    {
        UnityEngine.Object catalog = LoadCatalog(domain, catalogPathOverride);
        if (catalog == null)
        {
            return ContentWorkbenchActionResult.Fail($"Missing {domain} catalog.");
        }

        if (!AddToCatalogList(domain, catalog, asset))
        {
            return ContentWorkbenchActionResult.Ok("Content is already in the catalog.", asset);
        }

        EditorUtility.SetDirty(catalog);
        AssetDatabase.SaveAssets();
        return ContentWorkbenchActionResult.Ok("Added content to catalog.", asset);
    }

    public static ContentWorkbenchActionResult RemoveFromCatalog(ContentWorkbenchDomain domain, UnityEngine.Object asset, string catalogPathOverride = null)
    {
        UnityEngine.Object catalog = LoadCatalog(domain, catalogPathOverride);
        if (catalog == null)
        {
            return ContentWorkbenchActionResult.Fail($"Missing {domain} catalog.");
        }

        if (!RemoveFromCatalogList(domain, catalog, asset))
        {
            return ContentWorkbenchActionResult.Ok("Content was not in the catalog.", asset);
        }

        EditorUtility.SetDirty(catalog);
        AssetDatabase.SaveAssets();
        return ContentWorkbenchActionResult.Ok("Removed content from catalog. The asset was not deleted.", asset);
    }

    public static ContentWorkbenchDeletePreview PreviewDelete(ContentWorkbenchDomain domain, UnityEngine.Object asset, ContentWorkbenchCatalogPathOverrides overrides = null)
    {
        string title = asset is IContentDefinition definition ? $"Delete {definition.DisplayName}" : "Delete Content";
        var preview = new ContentWorkbenchDeletePreview(title);
        if (asset is not IContentDefinition content)
        {
            preview.Blockers.Add("Select a content definition asset first.");
            return preview;
        }

        string path = AssetDatabase.GetAssetPath(asset);
        if (string.IsNullOrWhiteSpace(path))
        {
            preview.Blockers.Add($"'{content.DisplayName}' is not a saved asset.");
            return preview;
        }

        UnityEngine.Object catalog = LoadCatalog(domain, overrides?.For(domain));
        if (catalog == null)
        {
            preview.Blockers.Add($"Missing {domain} catalog, so delete safety cannot be verified.");
            return preview;
        }

        bool isCataloged = IsCataloged(domain, asset, catalog);
        bool isActive = asset is IActivatableContentDefinition active && active.IsActive;
        if (isCataloged && isActive)
        {
            preview.Blockers.Add($"'{content.DisplayName}' is Available. Disable it or move it to Draft before deleting.");
        }

        preview.AddAsset(asset);
        AddGeneratedIconDeleteAsset(asset, preview);
        ContentWorkbenchUsageContext context = ContentWorkbenchUsageContext.LoadFromProject(overrides);
        switch (domain)
        {
            case ContentWorkbenchDomain.Weapons:
                BuildWeaponDeletePreview(asset as WeaponDefinition, context, preview);
                break;
            case ContentWorkbenchDomain.Items:
                BuildItemDeletePreview(asset as ItemDefinition, context, preview);
                break;
            case ContentWorkbenchDomain.Chests:
                break;
            case ContentWorkbenchDomain.Performers:
                BuildPerformerDeletePreview(asset as PerformerDefinition, context, preview);
                break;
            case ContentWorkbenchDomain.Headliners:
                break;
            case ContentWorkbenchDomain.Upgrades:
                BuildUpgradeDeletePreview(asset as UpgradeDefinition, context, preview);
                break;
            case ContentWorkbenchDomain.Talents:
                BuildTalentDeletePreview(asset as TalentDefinition, context, preview);
                break;
        }

        return preview;
    }

    private static void AddGeneratedIconDeleteAsset(UnityEngine.Object asset, ContentWorkbenchDeletePreview preview)
    {
        Sprite sprite = asset switch
        {
            WeaponDefinition weapon => weapon.iconSprite,
            PerformerDefinition performer => performer.portraitSprite,
            UpgradeDefinition upgrade => upgrade.iconSpriteOverride,
            TalentDefinition talent => talent.iconSprite,
            ItemDefinition item => item.iconSprite,
            _ => null
        };

        string iconPath = AssetDatabase.GetAssetPath(sprite);
        string generatedFolder = TheCircussyOneAssetPaths.GeneratedContentIconFolder + "/";
        if (!string.IsNullOrWhiteSpace(iconPath) && iconPath.StartsWith(generatedFolder, StringComparison.Ordinal))
        {
            preview.AddAssetPath(iconPath);
        }
    }

    public static ContentWorkbenchActionResult DeleteContent(ContentWorkbenchDomain domain, UnityEngine.Object asset, ContentWorkbenchCatalogPathOverrides overrides = null)
    {
        ContentWorkbenchDeletePreview preview = PreviewDelete(domain, asset, overrides);
        if (!preview.CanDelete)
        {
            return ContentWorkbenchActionResult.Fail(preview.BlockerMessage);
        }

        var dirty = new HashSet<UnityEngine.Object>();
        ContentWorkbenchUsageContext context = ContentWorkbenchUsageContext.LoadFromProject(overrides);
        CleanupDeleteReferences(domain, asset, context, dirty);
        UnityEngine.Object domainCatalog = LoadCatalog(domain, overrides?.For(domain));
        if (domainCatalog != null && RemoveFromCatalogList(domain, domainCatalog, asset))
        {
            dirty.Add(domainCatalog);
        }

        foreach (UnityEngine.Object changed in dirty)
        {
            if (changed != null)
            {
                EditorUtility.SetDirty(changed);
            }
        }

        AssetDatabase.SaveAssets();

        for (int i = 0; i < preview.AssetPaths.Count; i++)
        {
            string path = preview.AssetPaths[i];
            if (!string.IsNullOrWhiteSpace(path) && AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path) != null && !AssetDatabase.DeleteAsset(path))
            {
                return ContentWorkbenchActionResult.Fail($"Unity could not delete asset at {path}.");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return ContentWorkbenchActionResult.Ok($"Deleted {preview.AssetPaths.Count} asset(s).");
    }

    private static void BuildWeaponDeletePreview(WeaponDefinition weapon, ContentWorkbenchUsageContext context, ContentWorkbenchDeletePreview preview)
    {
        if (weapon == null)
        {
            return;
        }

        if (ContainsReference(context.WeaponCatalog?.StartingWeapons, weapon))
        {
            preview.Blockers.Add($"'{weapon.DisplayName}' is in the starting weapon list.");
        }

        List<PerformerDefinition> performers = PerformersStartingWith(context.PerformerCatalog?.Performers, weapon);
        if (performers.Count > 0)
        {
            preview.Blockers.Add($"'{weapon.DisplayName}' is a starting weapon for: {Names(performers)}.");
        }

        List<UpgradeDefinition> ownedUpgrades = UpgradesTargetingWeapon(context.UpgradeCatalog?.Upgrades, weapon);
        if (ownedUpgrades.Count > 0)
        {
            preview.Blockers.Add($"'{weapon.DisplayName}' has owned-weapon upgrade cards: {Names(ownedUpgrades)}.");
        }

    }

    private static void BuildItemDeletePreview(ItemDefinition item, ContentWorkbenchUsageContext context, ContentWorkbenchDeletePreview preview)
    {
        if (item == null)
        {
            return;
        }

        // Items are world-loot content in v0.3 and do not own level-up reward cards.
    }

    private static void BuildPerformerDeletePreview(PerformerDefinition performer, ContentWorkbenchUsageContext context, ContentWorkbenchDeletePreview preview)
    {
        if (performer == null)
        {
            return;
        }

        if (ReferenceEquals(context.PerformerCatalog?.defaultPerformer, performer))
        {
            preview.Blockers.Add($"'{performer.DisplayName}' is the fallback performer.");
        }

        List<TalentDefinition> talents = TalentsTargetingPerformer(context.TalentCatalog?.Talents, performer);
        if (talents.Count > 0)
        {
            preview.Blockers.Add($"'{performer.DisplayName}' is referenced by talents: {Names(talents)}.");
        }
    }

    private static void BuildUpgradeDeletePreview(UpgradeDefinition upgrade, ContentWorkbenchUsageContext context, ContentWorkbenchDeletePreview preview)
    {
        if (upgrade == null)
        {
            return;
        }

        if (AnyWeaponTrackContains(context.WeaponCatalog?.AvailableWeapons, upgrade))
        {
            preview.CleanupActions.Add($"remove '{upgrade.DisplayName}' from weapon upgrade tracks");
        }
    }

    private static void BuildTalentDeletePreview(TalentDefinition talent, ContentWorkbenchUsageContext context, ContentWorkbenchDeletePreview preview)
    {
        if (talent == null)
        {
            return;
        }

        // Talent ownership lives on the Talent asset; deleting a talent only removes it from the talent catalog.
    }

    private static void CleanupDeleteReferences(ContentWorkbenchDomain domain, UnityEngine.Object asset, ContentWorkbenchUsageContext context, HashSet<UnityEngine.Object> dirty)
    {
        switch (domain)
        {
            case ContentWorkbenchDomain.Weapons:
                CleanupWeaponDelete(asset as WeaponDefinition, context, dirty);
                break;
            case ContentWorkbenchDomain.Items:
                break;
            case ContentWorkbenchDomain.Chests:
                break;
            case ContentWorkbenchDomain.Headliners:
                break;
            case ContentWorkbenchDomain.Upgrades:
                CleanupUpgradeDelete(asset as UpgradeDefinition, context, dirty);
                break;
            case ContentWorkbenchDomain.Talents:
                CleanupTalentDelete(asset as TalentDefinition, context, dirty);
                break;
        }
    }

    private static void CleanupWeaponDelete(WeaponDefinition weapon, ContentWorkbenchUsageContext context, HashSet<UnityEngine.Object> dirty)
    {
        if (weapon == null)
        {
            return;
        }

    }

    private static void CleanupUpgradeDelete(UpgradeDefinition upgrade, ContentWorkbenchUsageContext context, HashSet<UnityEngine.Object> dirty)
    {
        if (upgrade == null)
        {
            return;
        }

        if (RemoveFromCatalogList(ContentWorkbenchDomain.Upgrades, context.UpgradeCatalog, upgrade))
        {
            dirty.Add(context.UpgradeCatalog);
        }

        RemoveUpgradeFromWeaponTracks(context.WeaponCatalog?.AvailableWeapons, upgrade, dirty);
    }

    private static void CleanupTalentDelete(TalentDefinition talent, ContentWorkbenchUsageContext context, HashSet<UnityEngine.Object> dirty)
    {
        if (talent == null)
        {
            return;
        }

        if (RemoveFromCatalogList(ContentWorkbenchDomain.Talents, context.TalentCatalog, talent))
        {
            dirty.Add(context.TalentCatalog);
        }

        // Talent ownership lives on the Talent asset; no performer-owned talent lists are cleaned up.
    }

    private static List<PerformerDefinition> PerformersStartingWith(IReadOnlyList<PerformerDefinition> performers, WeaponDefinition weapon)
    {
        var results = new List<PerformerDefinition>();
        if (performers == null || weapon == null)
        {
            return results;
        }

        for (int i = 0; i < performers.Count; i++)
        {
            PerformerDefinition performer = performers[i];
            if (performer != null && ReferenceEquals(performer.startingWeapon, weapon))
            {
                results.Add(performer);
            }
        }

        return results;
    }

    private static List<UpgradeDefinition> UpgradesTargetingWeapon(IReadOnlyList<UpgradeDefinition> upgrades, WeaponDefinition weapon)
    {
        var results = new List<UpgradeDefinition>();
        if (upgrades == null || weapon == null)
        {
            return results;
        }

        for (int i = 0; i < upgrades.Count; i++)
        {
            UpgradeDefinition upgrade = upgrades[i];
            if (upgrade == null)
            {
                continue;
            }

            bool referencesDefinition = ReferenceEquals(upgrade.weaponDefinition, weapon);
            bool referencesId = !string.IsNullOrWhiteSpace(upgrade.weaponId) && string.Equals(upgrade.weaponId, weapon.Id, StringComparison.Ordinal);
            if (referencesDefinition || referencesId)
            {
                results.Add(upgrade);
            }
        }

        return results;
    }

    private static List<TalentDefinition> TalentsTargetingPerformer(IReadOnlyList<TalentDefinition> talents, PerformerDefinition performer)
    {
        var results = new List<TalentDefinition>();
        if (talents == null || performer == null)
        {
            return results;
        }

        for (int i = 0; i < talents.Count; i++)
        {
            TalentDefinition talent = talents[i];
            if (talent == null)
            {
                continue;
            }

            if (talent.HasDirectPerformerReference(performer))
            {
                results.Add(talent);
            }
        }

        return results;
    }

    private static bool ContainsReference<T>(IReadOnlyList<T> list, T value)
        where T : class
    {
        if (list == null || value == null)
        {
            return false;
        }

        for (int i = 0; i < list.Count; i++)
        {
            if (ReferenceEquals(list[i], value))
            {
                return true;
            }
        }

        return false;
    }

    private static bool AnyWeaponTrackContains(IReadOnlyList<WeaponDefinition> weapons, UpgradeDefinition upgrade)
    {
        if (weapons == null || upgrade == null)
        {
            return false;
        }

        for (int i = 0; i < weapons.Count; i++)
        {
            WeaponDefinition weapon = weapons[i];
            if (weapon?.upgradeTrack != null && weapon.upgradeTrack.Contains(upgrade))
            {
                return true;
            }
        }

        return false;
    }

    private static List<TalentDefinition> TalentsAvailableToPerformer(IReadOnlyList<TalentDefinition> talents, PerformerDefinition performer)
    {
        var results = new List<TalentDefinition>();
        if (talents == null || performer == null)
        {
            return results;
        }

        for (int i = 0; i < talents.Count; i++)
        {
            TalentDefinition talent = talents[i];
            if (talent != null && talent.AllowsPerformer(performer))
            {
                results.Add(talent);
            }
        }

        return results;
    }

    private static void RemoveUpgradeFromWeaponTracks(IReadOnlyList<WeaponDefinition> weapons, UpgradeDefinition upgrade, HashSet<UnityEngine.Object> dirty)
    {
        if (weapons == null || upgrade == null)
        {
            return;
        }

        for (int i = 0; i < weapons.Count; i++)
        {
            WeaponDefinition weapon = weapons[i];
            if (weapon?.upgradeTrack != null && RemoveIfPresent(weapon.upgradeTrack, upgrade))
            {
                dirty.Add(weapon);
            }
        }
    }

    private static bool RemoveIfPresent<T>(List<T> list, T asset)
        where T : UnityEngine.Object
    {
        if (list == null)
        {
            return false;
        }

        string assetPath = asset != null ? AssetDatabase.GetAssetPath(asset) : string.Empty;
        bool changed = false;
        for (int i = list.Count - 1; i >= 0; i--)
        {
            T current = list[i];
            string currentPath = current != null ? AssetDatabase.GetAssetPath(current) : string.Empty;
            bool sameAsset = current == null
                || ReferenceEquals(current, asset)
                || (!string.IsNullOrWhiteSpace(assetPath) && string.Equals(currentPath, assetPath, StringComparison.Ordinal));
            if (!sameAsset)
            {
                continue;
            }

            list.RemoveAt(i);
            changed = true;
        }

        return changed;
    }

    private static string Names<T>(IReadOnlyList<T> definitions)
        where T : class, IContentDefinition
    {
        if (definitions == null || definitions.Count == 0)
        {
            return "None";
        }

        var names = new List<string>();
        for (int i = 0; i < definitions.Count; i++)
        {
            if (definitions[i] != null)
            {
                names.Add(definitions[i].DisplayName);
            }
        }

        return names.Count > 0 ? string.Join(", ", names) : "None";
    }

    public static ContentWorkbenchActionResult Duplicate(ContentWorkbenchDomain domain, UnityEngine.Object source, string requestedId, string requestedDisplayName, bool addToCatalog = true)
    {
        if (source is not IContentDefinition sourceDefinition)
        {
            return ContentWorkbenchActionResult.Fail("Select content to duplicate first.");
        }

        string sourcePath = AssetDatabase.GetAssetPath(source);
        if (string.IsNullOrWhiteSpace(sourcePath))
        {
            return ContentWorkbenchActionResult.Fail("Selected content is not a saved asset.");
        }

        string folder = Path.GetDirectoryName(sourcePath)?.Replace("\\", "/") ?? FolderFor(domain);
        HashSet<string> existingIds = ExistingIds(domain, folder, null);
        HashSet<string> existingNames = ExistingDisplayNames(domain, folder, null);
        string normalizedId = string.IsNullOrWhiteSpace(requestedId)
            ? ContentIdSuggestionRules.MakeCopyId(sourceDefinition.Id, existingIds)
            : ContentId.Normalize(requestedId);
        if (!ContentId.IsValidValue(normalizedId))
        {
            return ContentWorkbenchActionResult.Fail($"Content id '{requestedId}' is invalid. Use lowercase snake_case.");
        }

        string destination = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{AssetNameFromId(normalizedId)}.asset");
        if (!AssetDatabase.CopyAsset(sourcePath, destination))
        {
            return ContentWorkbenchActionResult.Fail("Unity could not duplicate the asset.");
        }

        UnityEngine.Object duplicate = AssetDatabase.LoadAssetAtPath(destination, TypeFor(domain));
        string displayName = string.IsNullOrWhiteSpace(requestedDisplayName)
            ? ContentIdSuggestionRules.MakeCopyDisplayName(sourceDefinition.DisplayName, existingNames)
            : requestedDisplayName.Trim();
        AssignIdentityAndActive(duplicate, normalizedId, displayName);
        EditorUtility.SetDirty(duplicate);
        if (addToCatalog)
        {
            AddToCatalog(domain, duplicate);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return ContentWorkbenchActionResult.Ok($"Duplicated content at {destination}.", duplicate);
    }

    public static ContentWorkbenchActionResult CreateFromTemplate(ContentWorkbenchCreateRequest request)
    {
        if (request == null)
        {
            return ContentWorkbenchActionResult.Fail("Creation request is missing.");
        }

        if (!IsTemplateValidForDomain(request.Domain, request.Template))
        {
            return ContentWorkbenchActionResult.Fail("Unsupported content template for the selected domain.");
        }

        if (UsesStatModifier(request) && !ContentWorkbenchStatOptions.IsAllowed(request.Domain, request.StatId))
        {
            return ContentWorkbenchActionResult.Fail($"{ContentModifierDisplayRules.StatDisplayName(request.StatId)} is not valid for {ContentWorkbenchDomainLayout.DisplayName(request.Domain)}.");
        }

        if (request.Domain == ContentWorkbenchDomain.Upgrades
            && request.Template == ContentWorkbenchCreateTemplate.WeaponStatUpgrade
            && request.TargetWeapon != null
            && !ContentWorkbenchStatOptions.IsAllowedForWeaponUpgrade(request.TargetWeapon, request.StatId))
        {
            return ContentWorkbenchActionResult.Fail($"{ContentModifierDisplayRules.StatDisplayName(request.StatId)} is not supported by {request.TargetWeapon.DisplayName}.");
        }

        if (!string.IsNullOrWhiteSpace(request.Id) && !ContentId.IsValidValue(ContentId.Normalize(request.Id)))
        {
            return ContentWorkbenchActionResult.Fail($"Content id '{request.Id}' is invalid. Use lowercase snake_case.");
        }

        string duplicateName = ExistingRequestedDisplayName(request);
        if (!string.IsNullOrWhiteSpace(duplicateName))
        {
            return ContentWorkbenchActionResult.Fail($"Content named '{duplicateName}' already exists. Existing content is never overwritten.");
        }

        ResolvedContentIdentity identity = ResolveCreateIdentity(request);
        return request.Domain switch
        {
            ContentWorkbenchDomain.Weapons => CreateWeapon(request, identity),
            ContentWorkbenchDomain.Items => CreateItem(request, identity),
            ContentWorkbenchDomain.Chests => CreateChest(request, identity),
            ContentWorkbenchDomain.Performers => CreatePerformer(request, identity),
            ContentWorkbenchDomain.Enemies => CreateEnemy(request, identity),
            ContentWorkbenchDomain.Headliners => CreateHeadliner(request, identity),
            ContentWorkbenchDomain.Upgrades => CreateUpgrade(request, identity),
            ContentWorkbenchDomain.Talents => CreateTalent(request, identity),
            _ => ContentWorkbenchActionResult.Fail("Unsupported content domain.")
        };
    }

    private static bool UsesStatModifier(ContentWorkbenchCreateRequest request)
    {
        return request.Domain == ContentWorkbenchDomain.Items
            || request.Domain == ContentWorkbenchDomain.Talents
            || (request.Domain == ContentWorkbenchDomain.Upgrades && request.Template == ContentWorkbenchCreateTemplate.WeaponStatUpgrade);
    }

    public static UnityEngine.Object LoadCatalog(ContentWorkbenchDomain domain, string overridePath = null)
    {
        string path = string.IsNullOrWhiteSpace(overridePath) ? CatalogPathFor(domain) : overridePath;
        return AssetDatabase.LoadAssetAtPath(path, CatalogTypeFor(domain));
    }

    public static string FolderFor(ContentWorkbenchDomain domain)
    {
        return ContentWorkbenchDomainRegistry.For(domain).Folder;
    }

    public static ContentWorkbenchCreateTemplate DefaultTemplateFor(ContentWorkbenchDomain domain)
    {
        return ContentWorkbenchDomainRegistry.For(domain).DefaultTemplate;
    }

    public static bool IsTemplateValidForDomain(ContentWorkbenchDomain domain, ContentWorkbenchCreateTemplate template)
    {
        return ContentWorkbenchDomainRegistry.For(domain).IsTemplateValid(template);
    }

    private static ContentWorkbenchActionResult CreateWeapon(ContentWorkbenchCreateRequest request, ResolvedContentIdentity identity)
    {
        WeaponFamilyTemplate template = request.Template switch
        {
            ContentWorkbenchCreateTemplate.SpreadProjectileWeapon => WeaponFamilyTemplate.SpreadProjectile,
            ContentWorkbenchCreateTemplate.ExplosiveArcWeapon => WeaponFamilyTemplate.ExplosiveArcProjectile,
            ContentWorkbenchCreateTemplate.BounceWeapon => WeaponFamilyTemplate.BounceProjectile,
            ContentWorkbenchCreateTemplate.ChainWeapon => WeaponFamilyTemplate.ChainProjectile,
            ContentWorkbenchCreateTemplate.OrbitWeapon => WeaponFamilyTemplate.Orbit,
            _ => WeaponFamilyTemplate.DirectProjectile
        };

        CustomWeaponCreationResult result = TheCircussyOneCustomWeaponCreator.CreateCustomWeapon(
            identity.Id,
            identity.DisplayName,
            template,
            addToWeaponCatalog: false,
            weaponFolder: string.IsNullOrWhiteSpace(request.AssetFolderOverride) ? TheCircussyOneCustomWeaponCreator.CustomWeaponFolder : request.AssetFolderOverride);

        if (!result.Success)
        {
            return ContentWorkbenchActionResult.Fail(result.Message);
        }

        if (request.AddToCatalog)
        {
            AddToCatalog(ContentWorkbenchDomain.Weapons, result.Weapon, request.CatalogPathOverride);
        }

        GenerateMissingIconForCreatedContent(request, result.Weapon);
        return ContentWorkbenchActionResult.Ok(result.Message, result.Weapon);
    }

    private static ContentWorkbenchActionResult CreateItem(ContentWorkbenchCreateRequest request, ResolvedContentIdentity identity)
    {
        string path = identity.AssetPath;
        if (AssetDatabase.LoadAssetAtPath<ItemDefinition>(path) != null)
        {
            return ContentWorkbenchActionResult.Fail($"Asset already exists at {path}. Existing assets are never overwritten.");
        }

        var modifier = new ItemStatModifierDefinition(request.StatId, request.Bucket, request.Value);
        ItemDefinition item = ScriptableObject.CreateInstance<ItemDefinition>();
        item.ApplyDefaults(
            identity.Id,
            identity.DisplayName,
            ContentModifierDisplayRules.Summary(modifier),
            ContentRarity.Common,
            ContentTagSet.With(ContentTag.Utility),
            ItemStackPolicy.StackLinear,
            5,
            new Color(0.9f, 0.95f, 0.82f, 1f),
            modifier);
        item.isActive = true;
        item.EnsureWorkflowDefaults();
        EnsureAssetFolderForPath(path);
        AssetDatabase.CreateAsset(item, path);
        EditorUtility.SetDirty(item);
        if (request.AddToCatalog)
        {
            AddToCatalog(ContentWorkbenchDomain.Items, item, request.CatalogPathOverride);
        }

        GenerateMissingIconForCreatedContent(request, item);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return ContentWorkbenchActionResult.Ok($"Created item at {path}.", item);
    }

    private static ContentWorkbenchActionResult CreateChest(ContentWorkbenchCreateRequest request, ResolvedContentIdentity identity)
    {
        string path = identity.AssetPath;
        if (AssetDatabase.LoadAssetAtPath<ChestDefinition>(path) != null)
        {
            return ContentWorkbenchActionResult.Fail($"Asset already exists at {path}. Existing assets are never overwritten.");
        }

        ChestDefinition chest = ScriptableObject.CreateInstance<ChestDefinition>();
        chest.ApplyDefaults(
            identity.Id,
            identity.DisplayName,
            "Spend Tickets to claim one item.",
            ChestKind.Locked,
            1f,
            35,
            "Open Chest",
            ChestDefinition.DefaultWeightsFor(ChestKind.Locked),
            new Color(0.62f, 0.36f, 0.14f, 1f),
            1f);
        chest.isActive = true;
        chest.EnsureWorkflowDefaults();
        EnsureAssetFolderForPath(path);
        AssetDatabase.CreateAsset(chest, path);
        EditorUtility.SetDirty(chest);
        if (request.AddToCatalog)
        {
            AddToCatalog(ContentWorkbenchDomain.Chests, chest, request.CatalogPathOverride);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return ContentWorkbenchActionResult.Ok($"Created chest at {path}.", chest);
    }

    private static ContentWorkbenchActionResult CreatePerformer(ContentWorkbenchCreateRequest request, ResolvedContentIdentity identity)
    {
        WeaponDefinition startingWeapon = request.TargetWeapon != null ? request.TargetWeapon : FirstActiveWeapon();
        if (!ContentAvailabilityRules.IsActiveAndValid(startingWeapon))
        {
            return ContentWorkbenchActionResult.Fail("An active starting weapon is required.");
        }

        string path = identity.AssetPath;
        if (AssetDatabase.LoadAssetAtPath<PerformerDefinition>(path) != null)
        {
            return ContentWorkbenchActionResult.Fail($"Asset already exists at {path}. Existing assets are never overwritten.");
        }

        PerformerDefinition performer = ScriptableObject.CreateInstance<PerformerDefinition>();
        performer.ApplyDefaults(
            identity.Id,
            identity.DisplayName,
            "Designer-created performer.",
            ContentTagSet.With(ContentTag.Utility),
            startingWeapon,
            new Color(0.95f, 0.78f, 0.28f, 1f),
            "No special passive yet.",
            PerformerPassiveKind.None,
            "Shared talent pool.",
            Array.Empty<UpgradeStatModifierDefinition>(),
            Array.Empty<UpgradeStatModifierDefinition>());
        EnsureAssetFolderForPath(path);
        AssetDatabase.CreateAsset(performer, path);
        EditorUtility.SetDirty(performer);
        if (request.AddToCatalog)
        {
            AddToCatalog(ContentWorkbenchDomain.Performers, performer, request.CatalogPathOverride);
        }

        GenerateMissingIconForCreatedContent(request, performer);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return ContentWorkbenchActionResult.Ok($"Created performer at {path}.", performer);
    }

    private static ContentWorkbenchActionResult CreateEnemy(ContentWorkbenchCreateRequest request, ResolvedContentIdentity identity)
    {
        string path = identity.AssetPath;
        if (AssetDatabase.LoadAssetAtPath<EnemyDefinition>(path) != null)
        {
            return ContentWorkbenchActionResult.Fail($"Asset already exists at {path}. Existing assets are never overwritten.");
        }

        EnemyDefinition enemy = ScriptableObject.CreateInstance<EnemyDefinition>();
        EnemyDefinition normal = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(TheCircussyOneAssetPaths.NormalEnemyPath);
        if (normal != null)
        {
            EditorUtility.CopySerialized(normal, enemy);
        }
        else
        {
            enemy.ApplyDefaultsFromGameConfig(AssetDatabase.LoadAssetAtPath<TheCircussyOne.Config.GameConfig>(TheCircussyOneAssetPaths.GameConfigPath));
        }

        enemy.enemyId = identity.Id;
        enemy.displayName = identity.DisplayName;
        enemy.isActive = true;
        enemy.EnsureWorkflowDefaults();
        EnsureAssetFolderForPath(path);
        AssetDatabase.CreateAsset(enemy, path);
        EditorUtility.SetDirty(enemy);
        if (request.AddToCatalog)
        {
            AddToCatalog(ContentWorkbenchDomain.Enemies, enemy, request.CatalogPathOverride);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return ContentWorkbenchActionResult.Ok($"Created enemy at {path}.", enemy);
    }

    private static ContentWorkbenchActionResult CreateHeadliner(ContentWorkbenchCreateRequest request, ResolvedContentIdentity identity)
    {
        string path = identity.AssetPath;
        if (AssetDatabase.LoadAssetAtPath<HeadlinerDefinition>(path) != null)
        {
            return ContentWorkbenchActionResult.Fail($"Asset already exists at {path}. Existing assets are never overwritten.");
        }

        EnemyDefinition actor = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(TheCircussyOneAssetPaths.OpeningHeadlinerEnemyPath)
            ?? AssetDatabase.LoadAssetAtPath<EnemyDefinition>(TheCircussyOneAssetPaths.NormalEnemyPath);
        if (!ContentAvailabilityRules.IsActiveAndValid(actor))
        {
            return ContentWorkbenchActionResult.Fail("An active enemy actor is required before creating a Headliner.");
        }

        HeadlinerDefinition headliner = ScriptableObject.CreateInstance<HeadlinerDefinition>();
        headliner.ApplyDefaults(actor);
        headliner.headlinerId = identity.Id;
        headliner.displayName = identity.DisplayName;
        headliner.isActive = true;
        headliner.EnsureWorkflowDefaults();
        EnsureAssetFolderForPath(path);
        AssetDatabase.CreateAsset(headliner, path);
        EditorUtility.SetDirty(headliner);
        if (request.AddToCatalog)
        {
            AddToCatalog(ContentWorkbenchDomain.Headliners, headliner, request.CatalogPathOverride);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return ContentWorkbenchActionResult.Ok($"Created Headliner at {path}.", headliner);
    }

    private static ContentWorkbenchActionResult CreateUpgrade(ContentWorkbenchCreateRequest request, ResolvedContentIdentity identity)
    {
        WeaponDefinition targetWeapon = request.TargetWeapon != null ? request.TargetWeapon : FirstActiveWeapon();
        if (!ContentAvailabilityRules.IsActiveAndValid(targetWeapon))
        {
            return ContentWorkbenchActionResult.Fail("An active target weapon is required.");
        }

        if (!ContentWorkbenchStatOptions.IsAllowedForWeaponUpgrade(targetWeapon, request.StatId))
        {
            return ContentWorkbenchActionResult.Fail($"{ContentModifierDisplayRules.StatDisplayName(request.StatId)} is not supported by {targetWeapon.DisplayName}.");
        }

        string path = identity.AssetPath;
        if (AssetDatabase.LoadAssetAtPath<UpgradeDefinition>(path) != null)
        {
            return ContentWorkbenchActionResult.Fail($"Asset already exists at {path}. Existing assets are never overwritten.");
        }

        UpgradeDefinition upgrade = ScriptableObject.CreateInstance<UpgradeDefinition>();
        upgrade.ApplyWeaponStatUpgradeDefaults(
            targetWeapon,
            ContentIdSuggestionRules.StatSlug(request.StatId),
            identity.DisplayName,
            ContentModifierDisplayRules.RarityPreview(new UpgradeStatModifierDefinition(request.StatId, request.Bucket, request.Value)),
            targetWeapon.projectilePrimaryColor,
            new UpgradeStatModifierDefinition(request.StatId, request.Bucket, request.Value));

        upgrade.upgradeId = identity.Id;
        upgrade.displayName = identity.DisplayName;
        upgrade.isActive = true;
        upgrade.EnsureWorkflowDefaults();
        EnsureAssetFolderForPath(path);
        AssetDatabase.CreateAsset(upgrade, path);
        EditorUtility.SetDirty(upgrade);
        if (request.AddToCatalog)
        {
            AddToCatalog(ContentWorkbenchDomain.Upgrades, upgrade, request.CatalogPathOverride);
        }

        GenerateMissingIconForCreatedContent(request, upgrade);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return ContentWorkbenchActionResult.Ok($"Created upgrade at {path}.", upgrade);
    }

    private static ContentWorkbenchActionResult CreateTalent(ContentWorkbenchCreateRequest request, ResolvedContentIdentity identity)
    {
        string path = identity.AssetPath;
        if (AssetDatabase.LoadAssetAtPath<TalentDefinition>(path) != null)
        {
            return ContentWorkbenchActionResult.Fail($"Asset already exists at {path}. Existing assets are never overwritten.");
        }

        TalentDefinition talent = ScriptableObject.CreateInstance<TalentDefinition>();
        talent.ApplyDefaults(
            identity.Id,
            identity.DisplayName,
            ContentModifierDisplayRules.RarityPreview(new UpgradeStatModifierDefinition(request.StatId, request.Bucket, request.Value)),
            ContentTagSet.With(ContentTag.Utility),
            new Color(0.7f, 0.9f, 1f, 1f),
            new UpgradeStatModifierDefinition(request.StatId, request.Bucket, request.Value));
        if (request.Template == ContentWorkbenchCreateTemplate.PerformerTalent)
        {
            PerformerDefinition performer = request.TargetPerformer != null ? request.TargetPerformer : FirstActivePerformer();
            if (!ContentAvailabilityRules.IsActiveAndValid(performer))
            {
                UnityEngine.Object.DestroyImmediate(talent);
                return ContentWorkbenchActionResult.Fail("An active target performer is required for a performer talent.");
            }

            talent.poolKind = TalentPoolKind.PerformerSpecific;
            talent.performerDefinition = performer;
            talent.performerId = performer.Id;
        }
        else
        {
            talent.EnsureSharedPerformerAccess((LoadCatalog(ContentWorkbenchDomain.Performers) as PerformerCatalog)?.Performers, defaultEnabled: true);
        }

        talent.isActive = true;
        talent.EnsureWorkflowDefaults();
        EnsureAssetFolderForPath(path);
        AssetDatabase.CreateAsset(talent, path);
        EditorUtility.SetDirty(talent);
        if (request.AddToCatalog)
        {
            AddToCatalog(ContentWorkbenchDomain.Talents, talent, request.CatalogPathOverride);
        }

        GenerateMissingIconForCreatedContent(request, talent);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return ContentWorkbenchActionResult.Ok($"Created talent at {path}.", talent);
    }

    private static void GenerateMissingIconForCreatedContent(ContentWorkbenchCreateRequest request, UnityEngine.Object asset)
    {
        if (!string.IsNullOrWhiteSpace(request.AssetFolderOverride)
            || !string.IsNullOrWhiteSpace(request.CatalogPathOverride))
        {
            return;
        }

        TheCircussyOneContentIconGenerator.GenerateMissingForSelected(asset);
    }

    private static ResolvedContentIdentity ResolveCreateIdentity(ContentWorkbenchCreateRequest request)
    {
        string folder = string.IsNullOrWhiteSpace(request.AssetFolderOverride) ? FolderFor(request.Domain) : request.AssetFolderOverride;
        HashSet<string> existingIds = ExistingIds(request.Domain, folder, request.CatalogPathOverride);
        HashSet<string> existingNames = ExistingDisplayNames(request.Domain, folder, request.CatalogPathOverride);

        string displayBase = string.IsNullOrWhiteSpace(request.DisplayName)
            ? SuggestedDisplayName(request)
            : request.DisplayName.Trim();
        string displayName = string.IsNullOrWhiteSpace(request.DisplayName)
            ? ContentIdSuggestionRules.MakeUniqueDisplayName(displayBase, existingNames)
            : request.DisplayName.Trim();

        string idBase = string.IsNullOrWhiteSpace(request.Id)
            ? SuggestedIdBase(request, displayName)
            : request.Id;
        string id = string.IsNullOrWhiteSpace(request.Id)
            ? ContentIdSuggestionRules.MakeUniqueId(idBase, existingIds, ContentId.Normalize(displayName))
            : ContentId.Normalize(request.Id);
        string path = AssetPathFor(folder, id);

        if (string.IsNullOrWhiteSpace(request.Id))
        {
            while (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path) != null)
            {
                existingIds.Add(id);
                id = ContentIdSuggestionRules.MakeUniqueId(idBase, existingIds, ContentId.Normalize(displayName));
                path = AssetPathFor(folder, id);
            }
        }

        return new ResolvedContentIdentity(id, displayName, path);
    }

    private static string ExistingRequestedDisplayName(ContentWorkbenchCreateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.DisplayName))
        {
            return null;
        }

        string folder = string.IsNullOrWhiteSpace(request.AssetFolderOverride) ? FolderFor(request.Domain) : request.AssetFolderOverride;
        HashSet<string> existingNames = ExistingDisplayNames(request.Domain, folder, request.CatalogPathOverride);
        string displayName = request.DisplayName.Trim();
        foreach (string existingName in existingNames)
        {
            if (string.Equals(existingName, displayName, StringComparison.OrdinalIgnoreCase))
            {
                return existingName;
            }
        }

        return null;
    }

    private static string SuggestedDisplayName(ContentWorkbenchCreateRequest request)
    {
        string statName = ContentIdSuggestionRules.StatDisplayName(request.StatId);
        return request.Domain switch
        {
            ContentWorkbenchDomain.Weapons => request.Template switch
            {
                ContentWorkbenchCreateTemplate.SpreadProjectileWeapon => "Spread Projectile Weapon",
                ContentWorkbenchCreateTemplate.ExplosiveArcWeapon => "Explosive Arc Weapon",
                ContentWorkbenchCreateTemplate.BounceWeapon => "Bounce Weapon",
                ContentWorkbenchCreateTemplate.ChainWeapon => "Chain Weapon",
                ContentWorkbenchCreateTemplate.OrbitWeapon => "Orbit Weapon",
                _ => "Direct Projectile Weapon"
            },
            ContentWorkbenchDomain.Items => $"{statName} Item",
            ContentWorkbenchDomain.Chests => "Locked Chest",
            ContentWorkbenchDomain.Performers => "Performer",
            ContentWorkbenchDomain.Enemies => "Enemy",
            ContentWorkbenchDomain.Headliners => "Headliner",
            ContentWorkbenchDomain.Upgrades => $"{TargetWeaponName(request)} {statName}",
            ContentWorkbenchDomain.Talents => request.Template == ContentWorkbenchCreateTemplate.PerformerTalent
                ? $"{TargetPerformerName(request)} {statName} Talent"
                : $"{statName} Talent",
            _ => "Content"
        };
    }

    private static string SuggestedIdBase(ContentWorkbenchCreateRequest request, string displayName)
    {
        string statSlug = ContentIdSuggestionRules.StatSlug(request.StatId);
        return request.Domain switch
        {
            ContentWorkbenchDomain.Upgrades => ContentIdSuggestionRules.BuildId(TargetWeaponId(request), statSlug),
            ContentWorkbenchDomain.Talents => request.Template == ContentWorkbenchCreateTemplate.PerformerTalent
                ? ContentIdSuggestionRules.BuildId(TargetPerformerId(request), statSlug, "talent")
                : ContentIdSuggestionRules.BuildId(statSlug, "talent"),
            ContentWorkbenchDomain.Headliners => ContentIdSuggestionRules.NormalizeBase(displayName, "headliner"),
            _ => ContentIdSuggestionRules.NormalizeBase(displayName, request.Domain.ToString())
        };
    }

    private static string TargetWeaponName(ContentWorkbenchCreateRequest request)
    {
        WeaponDefinition target = request.TargetWeapon != null ? request.TargetWeapon : FirstActiveWeapon();
        return target != null && !string.IsNullOrWhiteSpace(target.DisplayName) ? target.DisplayName : "Weapon";
    }

    private static string TargetWeaponId(ContentWorkbenchCreateRequest request)
    {
        WeaponDefinition target = request.TargetWeapon != null ? request.TargetWeapon : FirstActiveWeapon();
        return target != null && !string.IsNullOrWhiteSpace(target.Id) ? target.Id : "weapon";
    }

    private static string TargetPerformerName(ContentWorkbenchCreateRequest request)
    {
        PerformerDefinition target = request.TargetPerformer != null ? request.TargetPerformer : FirstActivePerformer();
        return target != null && !string.IsNullOrWhiteSpace(target.DisplayName) ? target.DisplayName : "Performer";
    }

    private static string TargetPerformerId(ContentWorkbenchCreateRequest request)
    {
        PerformerDefinition target = request.TargetPerformer != null ? request.TargetPerformer : FirstActivePerformer();
        return target != null && !string.IsNullOrWhiteSpace(target.Id) ? target.Id : "performer";
    }

    private static string AssetPathFor(string folder, string normalizedId)
    {
        return $"{folder}/{AssetNameFromId(normalizedId)}.asset";
    }

    private static void EnsureAssetFolderForPath(string path)
    {
        string folder = Path.GetDirectoryName(path)?.Replace("\\", "/");
        if (!string.IsNullOrWhiteSpace(folder))
        {
            EnsureFolder(folder);
        }
    }

    private static HashSet<string> ExistingIds(ContentWorkbenchDomain domain, string folder, string catalogPathOverride)
    {
        var ids = new HashSet<string>(StringComparer.Ordinal);
        AddExistingDefinitions(domain, folder, catalogPathOverride, definition => ids.Add(definition.Id));
        return ids;
    }

    private static HashSet<string> ExistingDisplayNames(ContentWorkbenchDomain domain, string folder, string catalogPathOverride)
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        AddExistingDefinitions(domain, folder, catalogPathOverride, definition => names.Add(definition.DisplayName));
        return names;
    }

    private static void AddExistingDefinitions(ContentWorkbenchDomain domain, string folder, string catalogPathOverride, Action<IContentDefinition> add)
    {
        if (AssetDatabase.IsValidFolder(folder))
        {
            string[] guids = AssetDatabase.FindAssets(TypeFilterFor(domain), new[] { folder });
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (AssetDatabase.LoadAssetAtPath(path, TypeFor(domain)) is IContentDefinition definition)
                {
                    add(definition);
                }
            }
        }

        if (LoadCatalog(domain, catalogPathOverride) is not UnityEngine.Object catalog)
        {
            return;
        }

        IReadOnlyList<IContentDefinition> definitions = CatalogDefinitions(domain, catalog);
        if (definitions == null)
        {
            return;
        }

        for (int i = 0; i < definitions.Count; i++)
        {
            if (definitions[i] != null)
            {
                add(definitions[i]);
            }
        }
    }

    private static IReadOnlyList<IContentDefinition> CatalogDefinitions(ContentWorkbenchDomain domain, UnityEngine.Object catalog)
    {
        return ContentWorkbenchDomainRegistry.For(domain).CatalogDefinitions(catalog);
    }

    private static bool IsCataloged(ContentWorkbenchDomain domain, UnityEngine.Object asset, UnityEngine.Object catalog)
    {
        return ContentWorkbenchDomainRegistry.For(domain).IsCataloged(catalog, asset);
    }

    private static bool AddToCatalogList(ContentWorkbenchDomain domain, UnityEngine.Object catalog, UnityEngine.Object asset)
    {
        return ContentWorkbenchDomainRegistry.For(domain).AddToCatalog(catalog, asset);
    }

    private static bool RemoveFromCatalogList(ContentWorkbenchDomain domain, UnityEngine.Object catalog, UnityEngine.Object asset)
    {
        return ContentWorkbenchDomainRegistry.For(domain).RemoveFromCatalog(catalog, asset);
    }

    private static bool TrySetActive(UnityEngine.Object asset, bool active)
    {
        for (int i = 0; i < ContentWorkbenchDomainRegistry.All.Count; i++)
        {
            if (ContentWorkbenchDomainRegistry.All[i].TrySetActive(asset, active))
            {
                return true;
            }
        }

        return false;
    }

    private static void AssignIdentityAndActive(UnityEngine.Object asset, string id, string displayName)
    {
        for (int i = 0; i < ContentWorkbenchDomainRegistry.All.Count; i++)
        {
            IContentWorkbenchDomainModule module = ContentWorkbenchDomainRegistry.All[i];
            if (module.DefinitionType.IsInstanceOfType(asset))
            {
                module.AssignIdentityAndActive(asset, id, displayName);
                return;
            }
        }
    }

    private static Dictionary<string, (int Errors, int Warnings)> IssueCountsById(ContentWorkbenchDomain domain, UnityEngine.Object catalog)
    {
        var counts = new Dictionary<string, (int Errors, int Warnings)>(StringComparer.Ordinal);
        IReadOnlyList<ContentValidationIssue> issues = ValidateCatalog(domain, catalog);
        for (int i = 0; i < issues.Count; i++)
        {
            ContentValidationIssue issue = issues[i];
            if (string.IsNullOrWhiteSpace(issue.ContentId))
            {
                continue;
            }

            counts.TryGetValue(issue.ContentId, out (int Errors, int Warnings) current);
            if (issue.Severity == ContentValidationSeverity.Error)
            {
                current.Errors++;
            }
            else
            {
                current.Warnings++;
            }

            counts[issue.ContentId] = current;
        }

        return counts;
    }

    private static IReadOnlyList<ContentValidationIssue> ValidateCatalog(ContentWorkbenchDomain domain, UnityEngine.Object catalog)
    {
        return ContentWorkbenchDomainRegistry.For(domain).ValidateCatalog(catalog);
    }

    private static WeaponDefinition FirstActiveWeapon()
    {
        var catalog = AssetDatabase.LoadAssetAtPath<WeaponCatalog>(TheCircussyOneAssetPaths.WeaponCatalogPath);
        return ContentAvailabilityRules.FirstActive(catalog?.AvailableWeapons);
    }

    private static PerformerDefinition FirstActivePerformer()
    {
        var catalog = AssetDatabase.LoadAssetAtPath<PerformerCatalog>(TheCircussyOneAssetPaths.PerformerCatalogPath);
        return ContentAvailabilityRules.FirstActive(catalog?.Performers);
    }

    private static string CatalogPathFor(ContentWorkbenchDomain domain)
    {
        return ContentWorkbenchDomainRegistry.For(domain).CatalogPath;
    }

    private static Type TypeFor(ContentWorkbenchDomain domain)
    {
        return ContentWorkbenchDomainRegistry.For(domain).DefinitionType;
    }

    private static Type CatalogTypeFor(ContentWorkbenchDomain domain)
    {
        return ContentWorkbenchDomainRegistry.For(domain).CatalogType;
    }

    private static string TypeFilterFor(ContentWorkbenchDomain domain)
    {
        return ContentWorkbenchDomainRegistry.For(domain).TypeFilter;
    }

    private static string AssetNameFromId(string id)
    {
        string normalized = ContentId.Normalize(id);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return "Content";
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
