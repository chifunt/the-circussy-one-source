using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Sirenix.OdinInspector;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using TheCircussyOne.Runtime;
using TheCircussyOne.Stats;
using UnityEditor;
using UnityEngine;

public sealed class ContentWorkbenchTests
{
    [Test]
    public void WorkbenchUsesPlainEditorWindowForImGuiButtonActions()
    {
        Assert.That(typeof(TheCircussyOneContentWorkbench).BaseType, Is.EqualTo(typeof(EditorWindow)));
    }

    [Test]
    public void WorkbenchFilterOptionsDefaultShowsEveryEntry()
    {
        var entries = new[]
        {
            new ContentWorkbenchEntry { Id = "available_weapon", DisplayName = "Available Weapon", IsActive = true, IsCataloged = true },
            new ContentWorkbenchEntry { Id = "disabled_weapon", DisplayName = "Disabled Weapon", IsActive = false, IsCataloged = true },
            new ContentWorkbenchEntry { Id = "draft_weapon", DisplayName = "Draft Weapon", IsActive = true, IsCataloged = false }
        };

        Assert.That(ContentWorkbenchFilterOptions.Default.CountVisible(entries), Is.EqualTo(3));
    }

    [Test]
    public void WorkbenchFilterOptionsApplyAvailabilityAndSearch()
    {
        var visible = new ContentWorkbenchEntry { Id = "knife_fan", DisplayName = "Knife Fan", IsActive = true, IsCataloged = true };
        var disabled = new ContentWorkbenchEntry { Id = "knife_trick", DisplayName = "Knife Trick", IsActive = false, IsCataloged = true };
        var draft = new ContentWorkbenchEntry { Id = "knife_shadow", DisplayName = "Knife Shadow", IsActive = true, IsCataloged = false };
        var wrongSearch = new ContentWorkbenchEntry { Id = "cannon", DisplayName = "Cannon", IsActive = true, IsCataloged = true };
        var filter = new ContentWorkbenchFilterOptions(
            showAvailable: true,
            showDisabled: false,
            showDraft: false,
            search: "knife");

        Assert.That(filter.Passes(visible), Is.True);
        Assert.That(filter.Passes(disabled), Is.False);
        Assert.That(filter.Passes(draft), Is.False);
        Assert.That(filter.Passes(wrongSearch), Is.False);
    }

    [Test]
    public void WorkbenchFilterOptionsApplyValidationFilters()
    {
        var ok = new ContentWorkbenchEntry { Id = "ok_weapon", DisplayName = "OK Weapon", IsActive = true, IsCataloged = true };
        var warning = new ContentWorkbenchEntry { Id = "warn_weapon", DisplayName = "Warn Weapon", IsActive = true, IsCataloged = true, WarningCount = 1 };
        var error = new ContentWorkbenchEntry { Id = "error_weapon", DisplayName = "Error Weapon", IsActive = true, IsCataloged = true, ErrorCount = 1 };
        var filter = new ContentWorkbenchFilterOptions(
            showAvailable: true,
            showDisabled: true,
            showDraft: true,
            search: string.Empty,
            showOk: false,
            showWarnings: true,
            showErrors: false);

        Assert.That(filter.Passes(ok), Is.False);
        Assert.That(filter.Passes(warning), Is.True);
        Assert.That(filter.Passes(error), Is.False);
    }

    [Test]
    public void WorkbenchDomainSummaryCountsAvailabilityAndValidation()
    {
        var entries = new[]
        {
            new ContentWorkbenchEntry { Id = "available", DisplayName = "Available", IsActive = true, IsCataloged = true },
            new ContentWorkbenchEntry { Id = "disabled", DisplayName = "Disabled", IsActive = false, IsCataloged = true, WarningCount = 1 },
            new ContentWorkbenchEntry { Id = "draft", DisplayName = "Draft", IsActive = true, IsCataloged = false, ErrorCount = 1 }
        };

        ContentWorkbenchDomainSummary summary = ContentWorkbenchDomainSummary.Build(entries, ContentWorkbenchFilterOptions.Default);

        Assert.That(summary.Total, Is.EqualTo(3));
        Assert.That(summary.Visible, Is.EqualTo(3));
        Assert.That(summary.Available, Is.EqualTo(1));
        Assert.That(summary.Disabled, Is.EqualTo(1));
        Assert.That(summary.Draft, Is.EqualTo(1));
        Assert.That(summary.Ok, Is.EqualTo(1));
        Assert.That(summary.Warnings, Is.EqualTo(1));
        Assert.That(summary.Errors, Is.EqualTo(1));
    }

    [Test]
    public void WorkbenchDomainLayoutGroupsDesignerContent()
    {
        IReadOnlyList<ContentWorkbenchDomainGroup> groups = ContentWorkbenchDomainLayout.DomainGroups;

        Assert.That(groups.Count, Is.EqualTo(3));
        Assert.That(groups[0].Title, Is.EqualTo("Playable Roster"));
        Assert.That(groups[0].Domains, Is.EqualTo(new[] { ContentWorkbenchDomain.Performers, ContentWorkbenchDomain.Weapons }));
        Assert.That(groups[1].Title, Is.EqualTo("Level-Up Rewards"));
        Assert.That(groups[1].Domains, Is.EqualTo(new[] { ContentWorkbenchDomain.Upgrades, ContentWorkbenchDomain.Talents }));
        Assert.That(groups[2].Title, Is.EqualTo("World Content"));
        Assert.That(groups[2].Domains, Is.EqualTo(new[] { ContentWorkbenchDomain.Items, ContentWorkbenchDomain.Chests, ContentWorkbenchDomain.Enemies, ContentWorkbenchDomain.Headliners }));
        Assert.That(ContentWorkbenchDomainLayout.DisplayName(ContentWorkbenchDomain.Upgrades), Is.EqualTo("Weapon Upgrades"));
        Assert.That(ContentWorkbenchDomainLayout.SortIndex(ContentWorkbenchDomain.Performers), Is.LessThan(ContentWorkbenchDomainLayout.SortIndex(ContentWorkbenchDomain.Talents)));
    }

    [Test]
    public void WorkbenchDomainRegistryCoversEveryDesignerDomain()
    {
        ContentWorkbenchDomain[] domains = System.Enum.GetValues(typeof(ContentWorkbenchDomain))
            .Cast<ContentWorkbenchDomain>()
            .ToArray();

        Assert.That(ContentWorkbenchDomainRegistry.All.Select(module => module.Domain).OrderBy(domain => domain), Is.EqualTo(domains.OrderBy(domain => domain)));
        foreach (ContentWorkbenchDomain domain in domains)
        {
            IContentWorkbenchDomainModule module = ContentWorkbenchDomainRegistry.For(domain);

            Assert.That(module.Domain, Is.EqualTo(domain));
            Assert.That(module.Folder, Is.EqualTo(ExpectedFolder(domain)), domain.ToString());
            Assert.That(ContentWorkbenchService.FolderFor(domain), Is.EqualTo(module.Folder), domain.ToString());
            Assert.That(module.CatalogPath, Is.EqualTo(ExpectedCatalogPath(domain)), domain.ToString());
            Assert.That(module.DefinitionType, Is.EqualTo(ExpectedDefinitionType(domain)), domain.ToString());
            Assert.That(module.CatalogType, Is.EqualTo(ExpectedCatalogType(domain)), domain.ToString());
            Assert.That(module.TypeFilter, Is.EqualTo("t:" + module.DefinitionType.Name), domain.ToString());
            Assert.That(module.IsTemplateValid(module.DefaultTemplate), Is.True, domain.ToString());
            Assert.That(ContentWorkbenchService.DefaultTemplateFor(domain), Is.EqualTo(module.DefaultTemplate), domain.ToString());
            Assert.That(ContentWorkbenchService.IsTemplateValidForDomain(domain, module.DefaultTemplate), Is.True, domain.ToString());
        }
    }

    [Test]
    public void WorkbenchEntryLayoutGroupsByAvailabilityAndSortsBySeverityThenName()
    {
        var entries = new[]
        {
            new ContentWorkbenchEntry { Id = "draft", DisplayName = "Draft Card", IsActive = true, IsCataloged = false },
            new ContentWorkbenchEntry { Id = "ok", DisplayName = "Alpha", IsActive = true, IsCataloged = true },
            new ContentWorkbenchEntry { Id = "disabled", DisplayName = "Disabled Card", IsActive = false, IsCataloged = true },
            new ContentWorkbenchEntry { Id = "error", DisplayName = "Zulu", IsActive = true, IsCataloged = true, ErrorCount = 1 },
            new ContentWorkbenchEntry { Id = "warning", DisplayName = "Bravo", IsActive = true, IsCataloged = true, WarningCount = 1 }
        };

        IReadOnlyList<ContentWorkbenchEntryGroup> groups = ContentWorkbenchEntryLayout.BuildGroups(entries, ContentWorkbenchFilterOptions.Default);

        Assert.That(groups.Count, Is.EqualTo(3));
        Assert.That(groups[0].Availability, Is.EqualTo(ContentWorkbenchAvailabilityState.Available));
        Assert.That(groups[0].Entries.Select(entry => entry.Id), Is.EqualTo(new[] { "error", "warning", "ok" }));
        Assert.That(groups[1].Availability, Is.EqualTo(ContentWorkbenchAvailabilityState.Disabled));
        Assert.That(groups[1].Entries.Select(entry => entry.Id), Is.EqualTo(new[] { "disabled" }));
        Assert.That(groups[2].Availability, Is.EqualTo(ContentWorkbenchAvailabilityState.Draft));
        Assert.That(groups[2].Entries.Select(entry => entry.Id), Is.EqualTo(new[] { "draft" }));
    }

    [Test]
    public void WorkbenchEntryLayoutNestsEntriesByWeaponOrPerformerSubgroup()
    {
        var entries = new[]
        {
            new ContentWorkbenchEntry { Id = "knife_speed", DisplayName = "Speed", IsActive = true, IsCataloged = true, SubgroupLabel = "Knife Fan", SubgroupSortIndex = 10 },
            new ContentWorkbenchEntry { Id = "cannon_damage", DisplayName = "Damage", IsActive = true, IsCataloged = true, SubgroupLabel = "Cannon", SubgroupSortIndex = 10 },
            new ContentWorkbenchEntry { Id = "shared_luck", DisplayName = "Luck", IsActive = true, IsCataloged = true, SubgroupLabel = "Shared Talents", SubgroupSortIndex = 0 },
            new ContentWorkbenchEntry { Id = "cannon_radius", DisplayName = "Radius", IsActive = true, IsCataloged = true, SubgroupLabel = "Cannon", SubgroupSortIndex = 10 }
        };

        IReadOnlyList<ContentWorkbenchEntryGroup> groups = ContentWorkbenchEntryLayout.BuildGroups(entries, ContentWorkbenchFilterOptions.Default);

        Assert.That(groups, Has.Count.EqualTo(1));
        Assert.That(groups[0].Subgroups.Select(group => group.Label), Is.EqualTo(new[] { "Shared Talents", "Cannon", "Knife Fan" }));
        Assert.That(groups[0].Subgroups[1].Entries.Select(entry => entry.Id), Is.EqualTo(new[] { "cannon_damage", "cannon_radius" }));
    }

    [Test]
    public void WorkbenchEntryGroupingResolvesWeaponUpgradeAndTalentGroups()
    {
        WeaponDefinition weapon = TheCircussyOneTestObjects.CreateWeaponDefinition(null, "cannon", "Cannon");
        UpgradeDefinition upgrade = TheCircussyOneTestObjects.CreateWeaponStatUpgrade(weapon, "damage", new UpgradeStatModifierDefinition(StatId.WeaponFlatDamage, StatModifierBucket.Flat, 1f));
        TalentDefinition sharedTalent = TheCircussyOneTestObjects.CreateTalentDefinition("footwork", "Footwork");
        PerformerDefinition character = TheCircussyOneTestObjects.CreatePerformerDefinition("mira", "Mira the Knife Acrobat", weapon);
        TalentDefinition characterTalent = TheCircussyOneTestObjects.CreateTalentDefinition("knife_flourish", "Knife Flourish");
        characterTalent.poolKind = TalentPoolKind.PerformerSpecific;
        characterTalent.performerDefinition = character;
        characterTalent.SyncPerformerIdFromDefinition();

        try
        {
            ContentWorkbenchEntrySubgroupInfo upgradeGroup = ContentWorkbenchEntryGrouping.Resolve(ContentWorkbenchDomain.Upgrades, upgrade);
            ContentWorkbenchEntrySubgroupInfo sharedGroup = ContentWorkbenchEntryGrouping.Resolve(ContentWorkbenchDomain.Talents, sharedTalent);
            ContentWorkbenchEntrySubgroupInfo characterGroup = ContentWorkbenchEntryGrouping.Resolve(ContentWorkbenchDomain.Talents, characterTalent);

            Assert.That(upgradeGroup.Label, Is.EqualTo("Cannon"));
            Assert.That(sharedGroup.Label, Is.EqualTo("Shared Talents"));
            Assert.That(sharedGroup.SortIndex, Is.LessThan(characterGroup.SortIndex));
            Assert.That(characterGroup.Label, Is.EqualTo("Mira the Knife Acrobat"));
        }
        finally
        {
            TheCircussyOneTestObjects.Destroy(weapon);
            TheCircussyOneTestObjects.Destroy(upgrade);
            TheCircussyOneTestObjects.Destroy(sharedTalent);
            TheCircussyOneTestObjects.Destroy(character);
            TheCircussyOneTestObjects.Destroy(characterTalent);
        }
    }

    [Test]
    public void WorkbenchCreateTemplateInfoMatchesDomainValidity()
    {
        foreach (ContentWorkbenchDomain domain in System.Enum.GetValues(typeof(ContentWorkbenchDomain)))
        {
            IReadOnlyList<ContentWorkbenchCreateTemplateInfo> templates = ContentWorkbenchCreateTemplateInfo.ForDomain(domain);
            Assert.That(templates, Is.Not.Empty, domain.ToString());
            foreach (ContentWorkbenchCreateTemplateInfo info in templates)
            {
                Assert.That(ContentWorkbenchService.IsTemplateValidForDomain(domain, info.Template), Is.True, $"{domain} / {info.Template}");
                Assert.That(info.Title, Is.Not.Empty);
                Assert.That(info.Description, Is.Not.Empty);
            }
        }
    }

    [Test]
    public void WorkbenchStatOptionsSeparateWeaponUpgradesFromTalentStats()
    {
        IReadOnlyList<StatId> upgradeStats = ContentWorkbenchStatOptions.ForDomain(ContentWorkbenchDomain.Upgrades)
            .Select(option => option.StatId)
            .ToArray();
        IReadOnlyList<StatId> talentStats = ContentWorkbenchStatOptions.ForDomain(ContentWorkbenchDomain.Talents)
            .Select(option => option.StatId)
            .ToArray();
        IReadOnlyList<StatId> itemStats = ContentWorkbenchStatOptions.ForDomain(ContentWorkbenchDomain.Items)
            .Select(option => option.StatId)
            .ToArray();

        Assert.That(upgradeStats, Does.Contain(StatId.WeaponFlatDamage));
        Assert.That(upgradeStats, Has.No.Member(StatId.WeaponDamageMultiplier));
        Assert.That(upgradeStats, Does.Contain(StatId.WeaponAccuracyMultiplier));
        Assert.That(upgradeStats, Does.Contain(StatId.WeaponPierce));
        Assert.That(upgradeStats.All(WeaponDefinition.IsWeaponLocalUpgradeStat), Is.True);
        Assert.That(upgradeStats, Has.No.Member(StatId.GlobalDamageMultiplier));

        Assert.That(talentStats, Does.Contain(StatId.GlobalDamageMultiplier));
        Assert.That(talentStats, Does.Contain(StatId.PlayerMaxHealth));
        Assert.That(talentStats, Has.No.Member(StatId.WeaponFlatDamage));
        Assert.That(talentStats.Any(WeaponDefinition.IsWeaponLocalUpgradeStat), Is.False);

        Assert.That(itemStats, Does.Contain(StatId.PlayerMaxHealth));
        Assert.That(itemStats, Does.Contain(StatId.PickupMagnetRadius));
        Assert.That(itemStats, Has.No.Member(StatId.WeaponFlatDamage));
        Assert.That(itemStats.Any(WeaponDefinition.IsWeaponLocalUpgradeStat), Is.False);
    }

    [Test]
    public void WorkbenchStatOptionsLimitWeaponUpgradesToTargetWeaponSupportedStats()
    {
        WeaponDefinition weapon = TheCircussyOneTestObjects.CreateWeaponDefinition(null, "juggling_ball", "Juggling Ball");
        weapon.supportedUpgradeStats.Clear();
        weapon.supportedUpgradeStats.Add(StatId.WeaponBounce);

        try
        {
            IReadOnlyList<StatId> stats = ContentWorkbenchStatOptions.ForWeaponUpgrade(weapon)
                .Select(option => option.StatId)
                .ToArray();

            Assert.That(stats, Is.EqualTo(new[] { StatId.WeaponBounce }));
            Assert.That(ContentWorkbenchStatOptions.IsAllowedForWeaponUpgrade(weapon, StatId.WeaponBounce), Is.True);
            Assert.That(ContentWorkbenchStatOptions.IsAllowedForWeaponUpgrade(weapon, StatId.WeaponFlatDamage), Is.False);
            Assert.That(ContentWorkbenchStatOptions.Contains(ContentWorkbenchStatOptions.ForWeaponUpgrade(weapon), StatId.WeaponFlatDamage), Is.False);
            Assert.That(ContentWorkbenchStatOptions.EnsureAllowedForWeaponUpgrade(weapon, StatId.WeaponFlatDamage), Is.EqualTo(StatId.WeaponBounce));
            Assert.That(ContentWorkbenchStatOptions.FirstOrFallback(ContentWorkbenchStatOptions.ForWeaponUpgrade(weapon), StatId.WeaponFlatDamage), Is.EqualTo(StatId.WeaponBounce));
            Assert.That(ContentWorkbenchStatOptions.UnsupportedStatMessage(ContentWorkbenchDomain.Upgrades, StatId.WeaponFlatDamage, weapon), Does.Contain("Weapon Damage"));
            Assert.That(ContentWorkbenchStatOptions.UnsupportedStatMessage(ContentWorkbenchDomain.Upgrades, StatId.WeaponFlatDamage, weapon), Does.Contain("Juggling Ball"));
            Assert.That(ContentWorkbenchStatOptions.UnsupportedStatMessage(ContentWorkbenchDomain.Upgrades, StatId.GlobalDamageMultiplier, weapon), Does.Contain("weapon-local"));
            Assert.That(ContentWorkbenchStatOptions.UnsupportedStatMessage(ContentWorkbenchDomain.Talents, StatId.WeaponFlatDamage), Does.Contain("weapon-local"));
        }
        finally
        {
            TheCircussyOneTestObjects.Destroy(weapon);
        }
    }

    [Test]
    public void WorkbenchCreateRejectsInvalidStatFamilies()
    {
        var weaponUpgradeRequest = new ContentWorkbenchCreateRequest
        {
            Domain = ContentWorkbenchDomain.Upgrades,
            Template = ContentWorkbenchCreateTemplate.WeaponStatUpgrade,
            StatId = StatId.GlobalDamageMultiplier
        };
        var talentRequest = new ContentWorkbenchCreateRequest
        {
            Domain = ContentWorkbenchDomain.Talents,
            Template = ContentWorkbenchCreateTemplate.SharedTalent,
            StatId = StatId.WeaponFlatDamage
        };

        ContentWorkbenchActionResult upgradeResult = ContentWorkbenchService.CreateFromTemplate(weaponUpgradeRequest);
        ContentWorkbenchActionResult talentResult = ContentWorkbenchService.CreateFromTemplate(talentRequest);

        Assert.That(upgradeResult.Success, Is.False);
        Assert.That(upgradeResult.Message, Does.Contain("not valid for Weapon Upgrades"));
        Assert.That(talentResult.Success, Is.False);
        Assert.That(talentResult.Message, Does.Contain("not valid for Talents"));
    }

    [Test]
    public void WorkbenchCreateRejectsUnsupportedTargetWeaponStats()
    {
        WeaponDefinition weapon = TheCircussyOneTestObjects.CreateWeaponDefinition(null, "juggling_ball", "Juggling Ball");
        weapon.supportedUpgradeStats.Clear();
        weapon.supportedUpgradeStats.Add(StatId.WeaponBounce);
        var request = new ContentWorkbenchCreateRequest
        {
            Domain = ContentWorkbenchDomain.Upgrades,
            Template = ContentWorkbenchCreateTemplate.WeaponStatUpgrade,
            DisplayName = "Unsupported Damage",
            TargetWeapon = weapon,
            StatId = StatId.WeaponFlatDamage,
            Bucket = StatModifierBucket.Flat,
            Value = 1f
        };

        try
        {
            ContentWorkbenchActionResult result = ContentWorkbenchService.CreateFromTemplate(request);

            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Does.Contain("not supported by Juggling Ball"));
        }
        finally
        {
            TheCircussyOneTestObjects.Destroy(weapon);
        }
    }

    [Test]
    public void WorkbenchAvailabilityStateCombinesCatalogAndActiveState()
    {
        var available = new ContentWorkbenchEntry { IsActive = true, IsCataloged = true };
        var disabled = new ContentWorkbenchEntry { IsActive = false, IsCataloged = true };
        var draftActive = new ContentWorkbenchEntry { IsActive = true, IsCataloged = false };
        var draftInactive = new ContentWorkbenchEntry { IsActive = false, IsCataloged = false };

        Assert.That(available.Availability, Is.EqualTo(ContentWorkbenchAvailabilityState.Available));
        Assert.That(disabled.Availability, Is.EqualTo(ContentWorkbenchAvailabilityState.Disabled));
        Assert.That(draftActive.Availability, Is.EqualTo(ContentWorkbenchAvailabilityState.Draft));
        Assert.That(draftInactive.Availability, Is.EqualTo(ContentWorkbenchAvailabilityState.Draft));
    }

    [Test]
    public void WorkbenchEntryLabelsUseCleanNamesAndValidationIcons()
    {
        var error = new ContentWorkbenchEntry
        {
            Id = "parked_weapon",
            DisplayName = "Parked Weapon",
            IsActive = false,
            IsCataloged = true,
            ErrorCount = 1
        };
        var warning = new ContentWorkbenchEntry { Id = "warn_weapon", DisplayName = "Warn Weapon", WarningCount = 1, IsActive = true, IsCataloged = true };
        var ok = new ContentWorkbenchEntry { Id = "ok_weapon", DisplayName = "OK Weapon", IsActive = true, IsCataloged = true };

        Assert.That(ContentWorkbenchEntryLabels.ValidationBadge(error), Is.EqualTo("Error"));
        Assert.That(ContentWorkbenchEntryLabels.ValidationMessageType(error), Is.EqualTo(MessageType.Error));
        Assert.That(ContentWorkbenchEntryLabels.ListLabel(error), Is.EqualTo("Parked Weapon (parked_weapon)"));
        Assert.That(ContentWorkbenchEntryLabels.ListLabel(error), Does.Not.Contain("[Error]"));
        Assert.That(ContentWorkbenchEntryLabels.ListLabel(error), Does.Not.Contain("[Disabled]"));
        Assert.That(ContentWorkbenchEntryLabels.ValidationIconName(error), Is.EqualTo(ContentWorkbenchEntryLabels.ErrorIconName));
        Assert.That(ContentWorkbenchEntryLabels.ValidationIconName(warning), Is.EqualTo(ContentWorkbenchEntryLabels.WarningIconName));
        Assert.That(ContentWorkbenchEntryLabels.ValidationIconName(ok), Is.EqualTo(ContentWorkbenchEntryLabels.OkIconName));
        Assert.That(ContentWorkbenchEntryLabels.ValidationTooltip(warning), Does.Contain("warning"));
        Assert.That(ContentWorkbenchEntryLabels.AvailabilityDescription(error), Does.Contain("disabled"));
    }

    [Test]
    public void WorkbenchAvailabilityColorsDifferentiateListStates()
    {
        Color available = ContentWorkbenchEntryLabels.AvailabilityTextColor(ContentWorkbenchAvailabilityState.Available);
        Color disabled = ContentWorkbenchEntryLabels.AvailabilityTextColor(ContentWorkbenchAvailabilityState.Disabled);
        Color draft = ContentWorkbenchEntryLabels.AvailabilityTextColor(ContentWorkbenchAvailabilityState.Draft);
        var availableEntry = new ContentWorkbenchEntry { IsActive = true, IsCataloged = true };

        Assert.That(available, Is.Not.EqualTo(disabled));
        Assert.That(available, Is.Not.EqualTo(draft));
        Assert.That(disabled, Is.Not.EqualTo(draft));
        Assert.That(available.g, Is.GreaterThan(available.r));
        Assert.That(disabled.r, Is.GreaterThan(disabled.b));
        Assert.That(draft.b, Is.GreaterThan(draft.r));

        Color normalBackground = ContentWorkbenchEntryLabels.AvailabilityBackgroundColor(availableEntry, selected: false, proSkin: true);
        Color selectedBackground = ContentWorkbenchEntryLabels.AvailabilityBackgroundColor(availableEntry, selected: true, proSkin: true);
        Assert.That(selectedBackground.g, Is.GreaterThan(normalBackground.g));

        Color enabledFilter = ContentWorkbenchEntryLabels.AvailabilityFilterBackgroundColor(ContentWorkbenchAvailabilityState.Available, enabled: true, proSkin: true);
        Color disabledFilter = ContentWorkbenchEntryLabels.AvailabilityFilterBackgroundColor(ContentWorkbenchAvailabilityState.Available, enabled: false, proSkin: true);
        Assert.That(enabledFilter.g, Is.GreaterThan(disabledFilter.g));
        Assert.That(ContentWorkbenchEntryLabels.AvailabilityFilterTextColor(ContentWorkbenchAvailabilityState.Available, enabled: true).g,
            Is.GreaterThan(ContentWorkbenchEntryLabels.AvailabilityFilterTextColor(ContentWorkbenchAvailabilityState.Available, enabled: false).g));

        Color enabledError = ContentWorkbenchEntryLabels.ValidationFilterBackgroundColor(errors: 1, warnings: 0, enabled: true, proSkin: true);
        Color disabledError = ContentWorkbenchEntryLabels.ValidationFilterBackgroundColor(errors: 1, warnings: 0, enabled: false, proSkin: true);
        Assert.That(enabledError.r, Is.GreaterThan(disabledError.r));
    }

    [Test]
    public void WorkbenchDomainChangeClearsSelectionAndRepairsTemplate()
    {
        var window = ScriptableObject.CreateInstance<TheCircussyOneContentWorkbench>();
        WeaponDefinition selected = TheCircussyOneTestObjects.CreateWeaponDefinition(null, "selected_weapon", "Selected Weapon");

        try
        {
            SetPrivateField(window, "createTemplate", ContentWorkbenchCreateTemplate.OrbitWeapon);
            SetPrivateField(window, "selectedAsset", selected);

            InvokePrivate(window, "ChangeDomain", ContentWorkbenchDomain.Performers);

            Assert.That(GetPrivateField<ContentWorkbenchDomain>(window, "domain"), Is.EqualTo(ContentWorkbenchDomain.Performers));
            Assert.That(GetPrivateField<ContentWorkbenchCreateTemplate>(window, "createTemplate"), Is.EqualTo(ContentWorkbenchCreateTemplate.Performer));
            Assert.That(GetPrivateField<UnityEngine.Object>(window, "selectedAsset"), Is.Null);
        }
        finally
        {
            TheCircussyOneTestObjects.Destroy(selected);
            Object.DestroyImmediate(window);
        }
    }

    [Test]
    public void WorkbenchAuthoringSummaryReportsHiddenWeaponGroups()
    {
        WeaponDefinition orbitWeapon = TheCircussyOneTestObjects.CreateWeaponDefinition(null, "fire_hoop", "Fire Hoop");
        WeaponDefinition projectileWeapon = TheCircussyOneTestObjects.CreateWeaponDefinition(null, "juggling_ball", "Juggling Ball");

        try
        {
            orbitWeapon.orbitEnabled = true;
            orbitWeapon.bounceEnabled = true;
            orbitWeapon.explosiveEnabled = true;
            orbitWeapon.chainEnabled = true;

            ContentWorkbenchAuthoringSummary orbitSummary = ContentWorkbenchAuthoringSummary.Build(orbitWeapon);

            Assert.That(orbitSummary.HiddenSettings.Any(line => line.Contains("Projectile travel")), Is.True);
            Assert.That(orbitSummary.HiddenSettings.Any(line => line.Contains("Projectile spawn pose")), Is.True);
            Assert.That(orbitSummary.HiddenSettings.Any(line => line.Contains("Bounce settings")), Is.True);
            Assert.That(orbitSummary.HiddenSettings.Any(line => line.Contains("Orbit settings")), Is.False);

            projectileWeapon.orbitEnabled = false;
            projectileWeapon.bounceEnabled = false;
            projectileWeapon.explosiveEnabled = false;
            projectileWeapon.chainEnabled = false;

            ContentWorkbenchAuthoringSummary projectileSummary = ContentWorkbenchAuthoringSummary.Build(projectileWeapon);

            Assert.That(projectileSummary.HiddenSettings.Any(line => line.Contains("Projectile travel")), Is.False);
            Assert.That(projectileSummary.HiddenSettings.Any(line => line.Contains("Bounce settings")), Is.True);
            Assert.That(projectileSummary.HiddenSettings.Any(line => line.Contains("Explosion settings")), Is.True);
            Assert.That(projectileSummary.HiddenSettings.Any(line => line.Contains("Chain settings")), Is.True);
            Assert.That(projectileSummary.HiddenSettings.Any(line => line.Contains("Orbit settings")), Is.True);
        }
        finally
        {
            TheCircussyOneTestObjects.Destroy(orbitWeapon);
            TheCircussyOneTestObjects.Destroy(projectileWeapon);
        }
    }

    [Test]
    public void WorkbenchAuthoringSummaryReportsHiddenEnemyAndStackGroups()
    {
        GameConfig config = ScriptableObject.CreateInstance<GameConfig>();
        EnemyDefinition enemy = TheCircussyOneTestObjects.CreateEnemyDefinition(config);

        try
        {
            enemy.climb.canClimbEnvironment = false;
            enemy.stack.policy = EnemyStackPolicy.GroundOnly;

            ContentWorkbenchAuthoringSummary summary = ContentWorkbenchAuthoringSummary.Build(enemy);

            Assert.That(summary.HiddenSettings.Any(line => line.Contains("Environment climb controls")), Is.True);
            Assert.That(summary.HiddenSettings.Any(line => line.Contains("Support stacking controls")), Is.True);

            enemy.climb.canClimbEnvironment = true;
            enemy.climb.useMoveSpeedForClimb = true;
            enemy.stack.policy = EnemyStackPolicy.SupportBased;

            summary = ContentWorkbenchAuthoringSummary.Build(enemy);

            Assert.That(summary.HiddenSettings.Any(line => line.Contains("Fallback climb speed")), Is.True);
            Assert.That(summary.HiddenSettings.Any(line => line.Contains("Support stacking controls")), Is.False);
        }
        finally
        {
            TheCircussyOneTestObjects.Destroy(enemy);
            Object.DestroyImmediate(config);
        }
    }

    [Test]
    public void WorkbenchInspectorLabelWidthStaysReadableInNarrowAndWidePanes()
    {
        Assert.That(ContentWorkbenchAuthoringSummary.InspectorLabelWidth(320f), Is.EqualTo(118f));
        Assert.That(ContentWorkbenchAuthoringSummary.InspectorLabelWidth(1200f), Is.EqualTo(160f));
    }

    [Test]
    public void WorkbenchLayoutPreservesEmbeddedInspectorWidth()
    {
        Assert.That(TheCircussyOneContentWorkbench.LeftPaneWidthForWindow(700f), Is.LessThan(370f));
        Assert.That(TheCircussyOneContentWorkbench.InspectorContentWidthForWindow(700f), Is.GreaterThanOrEqualTo(680f));
        Assert.That(TheCircussyOneContentWorkbench.LeftPaneWidthForWindow(1280f), Is.EqualTo(370f));
        Assert.That(TheCircussyOneContentWorkbench.InspectorContentWidthForWindow(1280f), Is.GreaterThan(680f));
    }

    [Test]
    public void WorkbenchSummaryPanelsStartCollapsed()
    {
        var window = ScriptableObject.CreateInstance<TheCircussyOneContentWorkbench>();

        try
        {
            Assert.That(GetPrivateField<bool>(window, "showUsageSummary"), Is.False);
            Assert.That(GetPrivateField<bool>(window, "showAuthoringSummary"), Is.False);
        }
        finally
        {
            Object.DestroyImmediate(window);
        }
    }

    [Test]
    public void WorkbenchKeepsManualWeaponInspectorAvailableAsFallback()
    {
        WeaponDefinition weapon = TheCircussyOneTestObjects.CreateWeaponDefinition(null, "juggling_ball", "Juggling Ball");
        TalentDefinition talent = TheCircussyOneTestObjects.CreateTalentDefinition("footwork", "Footwork");

        try
        {
            Assert.That(ContentWorkbenchWeaponInspector.CanDraw(weapon), Is.True);
            Assert.That(ContentWorkbenchWeaponInspector.CanDraw(talent), Is.False);
        }
        finally
        {
            TheCircussyOneTestObjects.Destroy(weapon);
            TheCircussyOneTestObjects.Destroy(talent);
        }
    }

    [Test]
    public void WorkbenchRoutesUpgradeAndItemAssetsToFocusedEffectInspector()
    {
        WeaponDefinition weapon = TheCircussyOneTestObjects.CreateWeaponDefinition(null, "juggling_ball", "Juggling Ball");
        UpgradeDefinition upgrade = TheCircussyOneTestObjects.CreateWeaponStatUpgrade(
            weapon,
            "bounce",
            new UpgradeStatModifierDefinition(StatId.WeaponBounce, StatModifierBucket.Flat, 1f));
        ItemDefinition item = TheCircussyOneTestObjects.CreateItemDefinition("magnet_charm", "Magnet Charm");
        TalentDefinition talent = TheCircussyOneTestObjects.CreateTalentDefinition("footwork", "Footwork");

        try
        {
            Assert.That(ContentWorkbenchEffectInspector.CanDraw(upgrade), Is.True);
            Assert.That(ContentWorkbenchEffectInspector.CanDraw(item), Is.True);
            Assert.That(ContentWorkbenchEffectInspector.CanDraw(weapon), Is.False);
            Assert.That(ContentWorkbenchEffectInspector.CanDraw(talent), Is.False);
        }
        finally
        {
            TheCircussyOneTestObjects.Destroy(weapon);
            TheCircussyOneTestObjects.Destroy(upgrade);
            TheCircussyOneTestObjects.Destroy(item);
            TheCircussyOneTestObjects.Destroy(talent);
        }
    }

    [Test]
    public void WorkbenchRoutesEnemyAssetsToFocusedEnemyInspector()
    {
        GameConfig config = ScriptableObject.CreateInstance<GameConfig>();
        EnemyDefinition enemy = TheCircussyOneTestObjects.CreateEnemyDefinition(config);
        WeaponDefinition weapon = TheCircussyOneTestObjects.CreateWeaponDefinition(null, "juggling_ball", "Juggling Ball");

        try
        {
            Assert.That(ContentWorkbenchEnemyInspector.CanDraw(enemy), Is.True);
            Assert.That(ContentWorkbenchEnemyInspector.CanDraw(weapon), Is.False);
        }
        finally
        {
            TheCircussyOneTestObjects.Destroy(enemy);
            TheCircussyOneTestObjects.Destroy(weapon);
            Object.DestroyImmediate(config);
        }
    }

    [Test]
    public void WorkbenchRoutesPerformerAssetsToFocusedPerformerInspector()
    {
        WeaponDefinition weapon = TheCircussyOneTestObjects.CreateWeaponDefinition(null, "juggling_ball", "Juggling Ball");
        PerformerDefinition performer = TheCircussyOneTestObjects.CreatePerformerDefinition("bibi_ball_juggler", "Bibi", weapon);

        try
        {
            Assert.That(ContentWorkbenchPerformerInspector.CanDraw(performer), Is.True);
            Assert.That(ContentWorkbenchPerformerInspector.CanDraw(weapon), Is.False);
        }
        finally
        {
            TheCircussyOneTestObjects.Destroy(performer);
            TheCircussyOneTestObjects.Destroy(weapon);
        }
    }

    [Test]
    public void WorkbenchRoutesTalentAssetsToFocusedTalentInspector()
    {
        TalentDefinition talent = TheCircussyOneTestObjects.CreateTalentDefinition("footwork", "Footwork");
        WeaponDefinition weapon = TheCircussyOneTestObjects.CreateWeaponDefinition(null, "juggling_ball", "Juggling Ball");

        try
        {
            Assert.That(ContentWorkbenchTalentInspector.CanDraw(talent), Is.True);
            Assert.That(ContentWorkbenchTalentInspector.CanDraw(weapon), Is.False);
        }
        finally
        {
            TheCircussyOneTestObjects.Destroy(talent);
            TheCircussyOneTestObjects.Destroy(weapon);
        }
    }

    [Test]
    public void WorkbenchEffectInspectorKeepsUpgradeAndItemTabs()
    {
        Assert.That(ContentWorkbenchEffectInspector.UpgradeTabCount, Is.EqualTo(5));
        Assert.That(ContentWorkbenchEffectInspector.UpgradeTabLabel((int)ContentWorkbenchUpgradeTab.Identity), Is.EqualTo("Identity"));
        Assert.That(ContentWorkbenchEffectInspector.UpgradeTabLabel((int)ContentWorkbenchUpgradeTab.Progression), Is.EqualTo("Progression"));
        Assert.That(ContentWorkbenchEffectInspector.UpgradeTabLabel((int)ContentWorkbenchUpgradeTab.Effect), Is.EqualTo("Effect"));
        Assert.That(ContentWorkbenchEffectInspector.UpgradeTabLabel((int)ContentWorkbenchUpgradeTab.Visual), Is.EqualTo("Visual"));
        Assert.That(ContentWorkbenchEffectInspector.UpgradeTabLabel((int)ContentWorkbenchUpgradeTab.Diagnostics), Is.EqualTo("Diagnostics"));

        Assert.That(ContentWorkbenchEffectInspector.ItemTabCount, Is.EqualTo(5));
        Assert.That(ContentWorkbenchEffectInspector.ItemTabLabel((int)ContentWorkbenchItemTab.Identity), Is.EqualTo("Identity"));
        Assert.That(ContentWorkbenchEffectInspector.ItemTabLabel((int)ContentWorkbenchItemTab.Stacking), Is.EqualTo("Stacking"));
        Assert.That(ContentWorkbenchEffectInspector.ItemTabLabel((int)ContentWorkbenchItemTab.Effects), Is.EqualTo("Effects"));
        Assert.That(ContentWorkbenchEffectInspector.ItemTabLabel((int)ContentWorkbenchItemTab.Visual), Is.EqualTo("Visual"));
        Assert.That(ContentWorkbenchEffectInspector.ItemTabLabel((int)ContentWorkbenchItemTab.Diagnostics), Is.EqualTo("Diagnostics"));

        Assert.That(ContentWorkbenchChestInspector.TabCount, Is.EqualTo(5));
        Assert.That(ContentWorkbenchChestInspector.TabLabel((int)ContentWorkbenchChestTab.Identity), Is.EqualTo("Identity"));
        Assert.That(ContentWorkbenchChestInspector.TabLabel((int)ContentWorkbenchChestTab.Interaction), Is.EqualTo("Interaction"));
        Assert.That(ContentWorkbenchChestInspector.TabLabel((int)ContentWorkbenchChestTab.Reward), Is.EqualTo("Reward"));
        Assert.That(ContentWorkbenchChestInspector.TabLabel((int)ContentWorkbenchChestTab.Visual), Is.EqualTo("Visual"));
        Assert.That(ContentWorkbenchChestInspector.TabLabel((int)ContentWorkbenchChestTab.Diagnostics), Is.EqualTo("Diagnostics"));
    }

    [Test]
    public void WorkbenchEnemyInspectorKeepsEnemyDefinitionTabs()
    {
        Assert.That(ContentWorkbenchEnemyInspector.TabCount, Is.EqualTo(9));
        Assert.That(ContentWorkbenchEnemyInspector.TabLabel((int)ContentWorkbenchEnemyTab.Identity), Is.EqualTo("Identity"));
        Assert.That(ContentWorkbenchEnemyInspector.TabLabel((int)ContentWorkbenchEnemyTab.Spawning), Is.EqualTo("Spawning"));
        Assert.That(ContentWorkbenchEnemyInspector.TabLabel((int)ContentWorkbenchEnemyTab.Stats), Is.EqualTo("Stats"));
        Assert.That(ContentWorkbenchEnemyInspector.TabLabel((int)ContentWorkbenchEnemyTab.Body), Is.EqualTo("Body"));
        Assert.That(ContentWorkbenchEnemyInspector.TabLabel((int)ContentWorkbenchEnemyTab.Climb), Is.EqualTo("Climb"));
        Assert.That(ContentWorkbenchEnemyInspector.TabLabel((int)ContentWorkbenchEnemyTab.Stack), Is.EqualTo("Stack"));
        Assert.That(ContentWorkbenchEnemyInspector.TabLabel((int)ContentWorkbenchEnemyTab.Drops), Is.EqualTo("Drops"));
        Assert.That(ContentWorkbenchEnemyInspector.TabLabel((int)ContentWorkbenchEnemyTab.Visual), Is.EqualTo("Visual"));
        Assert.That(ContentWorkbenchEnemyInspector.TabLabel((int)ContentWorkbenchEnemyTab.Diagnostics), Is.EqualTo("Diagnostics"));
    }

    [Test]
    public void WorkbenchPerformerInspectorKeepsPerformerDefinitionTabs()
    {
        Assert.That(ContentWorkbenchPerformerInspector.TabCount, Is.EqualTo(7));
        Assert.That(ContentWorkbenchPerformerInspector.TabLabel((int)ContentWorkbenchPerformerTab.Identity), Is.EqualTo("Identity"));
        Assert.That(ContentWorkbenchPerformerInspector.TabLabel((int)ContentWorkbenchPerformerTab.Loadout), Is.EqualTo("Loadout"));
        Assert.That(ContentWorkbenchPerformerInspector.TabLabel((int)ContentWorkbenchPerformerTab.Stats), Is.EqualTo("Stats"));
        Assert.That(ContentWorkbenchPerformerInspector.TabLabel((int)ContentWorkbenchPerformerTab.Passive), Is.EqualTo("Passive"));
        Assert.That(ContentWorkbenchPerformerInspector.TabLabel((int)ContentWorkbenchPerformerTab.Visual), Is.EqualTo("Visual"));
        Assert.That(ContentWorkbenchPerformerInspector.TabLabel((int)ContentWorkbenchPerformerTab.Theme), Is.EqualTo("Theme"));
        Assert.That(ContentWorkbenchPerformerInspector.TabLabel((int)ContentWorkbenchPerformerTab.Diagnostics), Is.EqualTo("Diagnostics"));
    }

    [Test]
    public void ActorVisualPreviewWindowOpensForPerformerAndEnemyDefinitions()
    {
        WeaponDefinition weapon = TheCircussyOneTestObjects.CreateWeaponDefinition(null, "juggling_ball", "Juggling Ball");
        PerformerDefinition performer = TheCircussyOneTestObjects.CreatePerformerDefinition("preview_performer", "Preview Performer", weapon);
        GameConfig config = ScriptableObject.CreateInstance<GameConfig>();
        EnemyDefinition enemy = TheCircussyOneTestObjects.CreateEnemyDefinition(config);

        ActorVisualPreviewWindow performerWindow = null;
        ActorVisualPreviewWindow enemyWindow = null;
        try
        {
            performerWindow = ActorVisualPreviewWindow.Open(performer);
            enemyWindow = ActorVisualPreviewWindow.Open(enemy);

            Assert.That(performerWindow, Is.Not.Null);
            Assert.That(enemyWindow, Is.Not.Null);
        }
        finally
        {
            performerWindow?.Close();
            enemyWindow?.Close();
            TheCircussyOneTestObjects.Destroy(performer);
            TheCircussyOneTestObjects.Destroy(enemy);
            TheCircussyOneTestObjects.Destroy(weapon);
            Object.DestroyImmediate(config);
        }
    }

    [Test]
    public void WorkbenchTalentInspectorKeepsTalentDefinitionTabs()
    {
        Assert.That(ContentWorkbenchTalentInspector.TabCount, Is.EqualTo(6));
        Assert.That(ContentWorkbenchTalentInspector.TabLabel((int)ContentWorkbenchTalentTab.Identity), Is.EqualTo("Identity"));
        Assert.That(ContentWorkbenchTalentInspector.TabLabel((int)ContentWorkbenchTalentTab.Pool), Is.EqualTo("Pool"));
        Assert.That(ContentWorkbenchTalentInspector.TabLabel((int)ContentWorkbenchTalentTab.Progression), Is.EqualTo("Progression"));
        Assert.That(ContentWorkbenchTalentInspector.TabLabel((int)ContentWorkbenchTalentTab.Effect), Is.EqualTo("Effect"));
        Assert.That(ContentWorkbenchTalentInspector.TabLabel((int)ContentWorkbenchTalentTab.Visual), Is.EqualTo("Visual"));
        Assert.That(ContentWorkbenchTalentInspector.TabLabel((int)ContentWorkbenchTalentTab.Diagnostics), Is.EqualTo("Diagnostics"));
    }

    [Test]
    public void WorkbenchEffectInspectorDoesNotExposeRawTargetWeaponIdField()
    {
        string text = File.ReadAllText("Assets/Game/Editor/ContentWorkbenchEffectInspector.cs");

        Assert.That(text, Does.Not.Contain("Target Weapon ID"));
    }

    [Test]
    public void WorkbenchTalentInspectorDoesNotExposeRawPerformerIdField()
    {
        string text = File.ReadAllText("Assets/Game/Editor/ContentWorkbenchTalentInspector.cs");

        Assert.That(text, Does.Not.Contain("Performer ID"));
    }

    [Test]
    public void WorkbenchWeaponInspectorTreatsOwnedUpgradeTrackAsReadOnlyRelationship()
    {
        string text = File.ReadAllText("Assets/Game/Editor/ContentWorkbenchWeaponInspector.cs");

        Assert.That(text, Does.Contain("Owned weapon cards are assigned from the Weapon Upgrades domain"));
        Assert.That(text, Does.Contain("new EditorGUI.DisabledScope(true)"));
    }

    [Test]
    public void WorkbenchModifierUnitsUseDesignerFacingLabels()
    {
        Assert.That(ContentWorkbenchFieldDrawer.ModifierUnit(StatId.WeaponFlatDamage, StatModifierBucket.Flat), Is.EqualTo("damage"));
        Assert.That(ContentWorkbenchFieldDrawer.ModifierUnit(StatId.GlobalWeaponHaste, StatModifierBucket.AdditivePercent), Is.EqualTo("%"));
        Assert.That(ContentWorkbenchFieldDrawer.ModifierUnit(StatId.PickupMagnetRadius, StatModifierBucket.Flat), Is.EqualTo("u"));
        Assert.That(ContentWorkbenchFieldDrawer.ModifierUnit(StatId.PlayerHpRegenPerMinute, StatModifierBucket.Flat), Is.EqualTo("/min"));
        Assert.That(ContentWorkbenchFieldDrawer.ModifierUnit(StatId.WeaponBounce, StatModifierBucket.Flat), Is.EqualTo("bounces"));
    }

    [Test]
    public void WorkbenchExplainsDecimalIntegerStatFlooring()
    {
        Assert.That(ContentWorkbenchFieldDrawer.UsesFlooredIntegerApplication(StatId.WeaponFlatDamage, StatModifierBucket.Flat), Is.True);
        Assert.That(ContentWorkbenchFieldDrawer.UsesFlooredIntegerApplication(StatId.WeaponAttackSpeed, StatModifierBucket.AdditivePercent), Is.False);
        Assert.That(ContentWorkbenchFieldDrawer.IntegerModifierFlooringHelp, Does.Contain("Final runtime values are floored"));
        Assert.That(ContentWorkbenchFieldDrawer.IntegerModifierFlooringHelp, Does.Contain("+0.75 Weapon Damage"));
        Assert.That(ContentWorkbenchFieldDrawer.IntegerCommonValueWarning(StatId.WeaponFlatDamage, StatModifierBucket.Flat, 0.75f), Does.Contain("floors to +0"));
        Assert.That(ContentWorkbenchFieldDrawer.IntegerCommonValueWarning(StatId.WeaponFlatDamage, StatModifierBucket.Flat, 1f), Is.Empty);
        Assert.That(ContentWorkbenchFieldDrawer.IntegerCommonValueWarning(StatId.WeaponAttackSpeed, StatModifierBucket.AdditivePercent, 0.05f), Is.Empty);
    }

    [Test]
    public void WorkbenchBucketOptionsHideInternalModifierBuckets()
    {
        Assert.That(ContentWorkbenchFieldDrawer.DesignerBucketLabelsForStat(StatId.WeaponFlatDamage), Is.EqualTo(new[] { "Flat" }));
        Assert.That(ContentWorkbenchFieldDrawer.DesignerBucketLabelsForStat(StatId.WeaponAttackSpeed), Is.EqualTo(new[] { "Additive Percent" }));
        Assert.That(ContentWorkbenchFieldDrawer.DesignerBucketLabelsForStat(StatId.WeaponBounce), Is.EqualTo(new[] { "Flat" }));
        Assert.That(ContentWorkbenchFieldDrawer.DesignerBucketLabel(StatId.WeaponAttackSpeed, StatModifierBucket.AdditivePercent), Is.EqualTo("Additive Percent"));
        Assert.That(ContentWorkbenchFieldDrawer.DesignerBucketLabel(StatId.WeaponFlatDamage, StatModifierBucket.GlobalPercent), Is.EqualTo("Flat"));
    }

    [Test]
    public void WorkbenchBucketValidationExplainsLegacyAndInternalBuckets()
    {
        Assert.That(ContentWorkbenchFieldDrawer.IsValidDesignerBucket(StatId.GlobalWeaponHaste, StatModifierBucket.Haste), Is.False);
        Assert.That(ContentWorkbenchFieldDrawer.InvalidBucketMessage(StatId.GlobalWeaponHaste, StatModifierBucket.Haste), Does.Contain("unsupported legacy bucket"));
        Assert.That(ContentWorkbenchFieldDrawer.IsValidDesignerBucket(StatId.GlobalDamageMultiplier, StatModifierBucket.GlobalPercent), Is.False);
        Assert.That(ContentWorkbenchFieldDrawer.InvalidBucketMessage(StatId.GlobalDamageMultiplier, StatModifierBucket.GlobalPercent), Does.Contain("unsupported internal bucket"));
        Assert.That(ContentWorkbenchFieldDrawer.IsValidDesignerBucket(StatId.GlobalWeaponHaste, StatModifierBucket.AdditivePercent), Is.True);
    }

    [Test]
    public void WorkbenchBucketDefaultsUseCanonicalAttackSpeedBuckets()
    {
        Assert.That(ContentWorkbenchFieldDrawer.DefaultBucketForStat(StatId.GlobalWeaponHaste), Is.EqualTo(StatModifierBucket.AdditivePercent));
        Assert.That(ContentWorkbenchFieldDrawer.DefaultBucketForStat(StatId.WeaponAttackSpeed), Is.EqualTo(StatModifierBucket.AdditivePercent));
        Assert.That(ContentWorkbenchFieldDrawer.DefaultBucketForStat(StatId.WeaponBounce), Is.EqualTo(StatModifierBucket.Flat));
    }

    [Test]
    public void WorkbenchEffectEditorsDoNotExposeRawBucketEnumPopup()
    {
        string workbench = File.ReadAllText("Assets/Game/Editor/TheCircussyOneContentWorkbench.cs");
        string effectInspector = File.ReadAllText("Assets/Game/Editor/ContentWorkbenchEffectInspector.cs");

        Assert.That(workbench, Does.Not.Contain("EnumPopup(\"Bucket\""));
        Assert.That(effectInspector, Does.Not.Contain("PropertyField(bucket"));
        Assert.That(effectInspector, Does.Not.Contain("DrawBucketPopup"));
    }

    [Test]
    public void WorkbenchWeaponInspectorProvidesFocusedTabs()
    {
        Assert.That(ContentWorkbenchWeaponInspector.TabCount, Is.EqualTo(9));
        Assert.That(ContentWorkbenchWeaponInspector.TabLabel((int)ContentWorkbenchWeaponTab.Identity), Is.EqualTo("Identity"));
        Assert.That(ContentWorkbenchWeaponInspector.TabLabel((int)ContentWorkbenchWeaponTab.Timing), Is.EqualTo("Timing"));
        Assert.That(ContentWorkbenchWeaponInspector.TabLabel((int)ContentWorkbenchWeaponTab.Projectile), Is.EqualTo("Projectile"));
        Assert.That(ContentWorkbenchWeaponInspector.TabLabel((int)ContentWorkbenchWeaponTab.Orbit), Is.EqualTo("Orbit"));
        Assert.That(ContentWorkbenchWeaponInspector.TabLabel((int)ContentWorkbenchWeaponTab.Targeting), Is.EqualTo("Targeting"));
        Assert.That(ContentWorkbenchWeaponInspector.TabLabel((int)ContentWorkbenchWeaponTab.SpawnPose), Is.EqualTo("Spawn Pose"));
        Assert.That(ContentWorkbenchWeaponInspector.TabLabel((int)ContentWorkbenchWeaponTab.Visuals), Is.EqualTo("Visuals"));
        Assert.That(ContentWorkbenchWeaponInspector.TabLabel((int)ContentWorkbenchWeaponTab.UpgradeTrack), Is.EqualTo("Upgrade Track"));
        Assert.That(ContentWorkbenchWeaponInspector.TabLabel((int)ContentWorkbenchWeaponTab.Diagnostics), Is.EqualTo("Diagnostics"));
    }

    [Test]
    public void WorkbenchWeaponSupportedStatsEditorAddsOnlyUniqueWeaponLocalStats()
    {
        var currentStats = new[] { StatId.WeaponBounce, StatId.WeaponFlatDamage };

        IReadOnlyList<StatId> addOptions = ContentWorkbenchWeaponInspector.SupportedStatAddOptions(currentStats)
            .Select(option => option.StatId)
            .ToArray();
        IReadOnlyList<StatId> firstRowOptions = ContentWorkbenchWeaponInspector.SupportedStatRowOptions(currentStats, 0)
            .Select(option => option.StatId)
            .ToArray();

        Assert.That(addOptions, Has.No.Member(StatId.WeaponBounce));
        Assert.That(addOptions, Has.No.Member(StatId.WeaponFlatDamage));
        Assert.That(addOptions, Does.Contain(StatId.WeaponAttackSpeed));
        Assert.That(addOptions, Has.No.Member(StatId.GlobalDamageMultiplier));
        Assert.That(addOptions, Has.No.Member(StatId.PlayerMaxHealth));

        Assert.That(firstRowOptions, Does.Contain(StatId.WeaponBounce));
        Assert.That(firstRowOptions, Has.No.Member(StatId.WeaponFlatDamage));
        Assert.That(firstRowOptions, Does.Contain(StatId.WeaponAttackSpeed));
        Assert.That(firstRowOptions, Has.No.Member(StatId.GlobalDamageMultiplier));
    }

    [Test]
    public void WorkbenchWeaponInspectorSummarizesLevelUpUpgradePool()
    {
        WeaponDefinition weapon = TheCircussyOneTestObjects.CreateWeaponDefinition(null, "fire_hoop", "Fire Hoop");
        weapon.supportedUpgradeStats.Clear();
        weapon.supportedUpgradeStats.Add(StatId.WeaponFlatDamage);
        weapon.supportedUpgradeStats.Add(StatId.WeaponProjectileCount);

        try
        {
            string summary = ContentWorkbenchWeaponInspector.LevelUpUpgradePoolSummary(weapon);

            Assert.That(summary, Does.Contain("Weapon Damage"));
            Assert.That(summary, Does.Contain("Weapon Projectile Count"));
            Assert.That(summary, Does.Contain("Common-only"));

            weapon.canAppearAsLevelUpWeapon = false;
            Assert.That(ContentWorkbenchWeaponInspector.LevelUpUpgradePoolSummary(weapon), Does.Contain("cannot appear as an add-weapon card"));

            weapon.supportedUpgradeStats.Clear();
            Assert.That(ContentWorkbenchWeaponInspector.LevelUpUpgradePoolSummary(weapon), Does.Contain("No owned weapon upgrade stats"));
        }
        finally
        {
            TheCircussyOneTestObjects.Destroy(weapon);
        }
    }

    [Test]
    public void WeaponInactiveFamilyFieldsUseShowIfInsteadOfEnableIf()
    {
        AssertHasShowIfWithoutEnableIf(typeof(WeaponDefinition), nameof(WeaponDefinition.projectileSpeed));
        AssertHasShowIfWithoutEnableIf(typeof(WeaponDefinition), nameof(WeaponDefinition.weaponRange));
        AssertHasShowIfWithoutEnableIf(typeof(WeaponDefinition), nameof(WeaponDefinition.projectileSpawnForwardOffset));
        AssertHasShowIfWithoutEnableIf(typeof(WeaponDefinition), nameof(WeaponDefinition.projectileVisualShape));
        AssertHasShowIfWithoutEnableIf(typeof(WeaponDefinition), nameof(WeaponDefinition.bounceEnabled));
        AssertHasShowIfWithoutEnableIf(typeof(WeaponDefinition), nameof(WeaponDefinition.baseBounceCount));
        AssertHasShowIfWithoutEnableIf(typeof(WeaponDefinition), nameof(WeaponDefinition.baseSplashRadius));
        AssertHasShowIfWithoutEnableIf(typeof(WeaponDefinition), nameof(WeaponDefinition.baseOrbitCount));
    }

    [Test]
    public void EnemyInactivePolicyFieldsUseShowIfInsteadOfEnableIf()
    {
        AssertHasShowIfWithoutEnableIf(typeof(EnemyClimbProfile), nameof(EnemyClimbProfile.useMoveSpeedForClimb));
        AssertHasShowIfWithoutEnableIf(typeof(EnemyClimbProfile), nameof(EnemyClimbProfile.fallbackClimbSpeed));
        AssertHasShowIfWithoutEnableIf(typeof(EnemyClimbProfile), nameof(EnemyClimbProfile.maxEnvironmentClimbHeight));
        AssertHasShowIfWithoutEnableIf(typeof(EnemyStackProfile), nameof(EnemyStackProfile.canClimbEnemies));
        AssertHasShowIfWithoutEnableIf(typeof(EnemyStackProfile), nameof(EnemyStackProfile.pileRadius));
        AssertHasShowIfWithoutEnableIf(typeof(EnemyStackProfile), nameof(EnemyStackProfile.supportClimbSpeedMultiplier));
    }

    [Test]
    public void ActiveContentFiltersRuntimeCatalogsAndLoadouts()
    {
        WeaponDefinition activeWeapon = TheCircussyOneTestObjects.CreateWeaponDefinition(null, "active_weapon", "Active Weapon");
        WeaponDefinition inactiveWeapon = TheCircussyOneTestObjects.CreateWeaponDefinition(null, "inactive_weapon", "Inactive Weapon");
        inactiveWeapon.isActive = false;
        WeaponCatalog weaponCatalog = TheCircussyOneTestObjects.CreateWeaponCatalog(activeWeapon, inactiveWeapon);

        var loadout = new WeaponLoadout(weaponCatalog);

        Assert.That(ContentAvailabilityRules.IsActiveAndValid(activeWeapon), Is.True);
        Assert.That(ContentAvailabilityRules.IsActiveAndValid(inactiveWeapon), Is.False);
        Assert.That(loadout.OwnsWeapon(activeWeapon.Id), Is.True);
        Assert.That(loadout.OwnsWeapon(inactiveWeapon.Id), Is.False);

        PerformerDefinition activeCharacter = TheCircussyOneTestObjects.CreatePerformerDefinition("active_character", "Active Character", activeWeapon);
        PerformerDefinition inactiveCharacter = TheCircussyOneTestObjects.CreatePerformerDefinition("inactive_character", "Inactive Character", activeWeapon);
        inactiveCharacter.isActive = false;

        Assert.That(PerformerSelectionPreviewRules.BuildFrames(new[] { activeCharacter, inactiveCharacter }).Select(frame => frame.Performer.Id), Is.EqualTo(new[] { activeCharacter.Id }));

        GameConfig config = ScriptableObject.CreateInstance<GameConfig>();
        EnemyDefinition inactiveEnemy = TheCircussyOneTestObjects.CreateEnemyDefinition(config);
        inactiveEnemy.enemyId = "inactive_enemy";
        inactiveEnemy.isActive = false;
        EnemyDefinition activeEnemy = TheCircussyOneTestObjects.CreateEnemyDefinition(config);
        activeEnemy.enemyId = "active_enemy";
        EnemyCatalog enemyCatalog = TheCircussyOneTestObjects.CreateEnemyCatalog(inactiveEnemy, activeEnemy);

        try
        {
            Assert.That(enemyCatalog.DefaultEnemy, Is.SameAs(activeEnemy));
        }
        finally
        {
            TheCircussyOneTestObjects.Destroy(weaponCatalog);
            TheCircussyOneTestObjects.Destroy(activeWeapon);
            TheCircussyOneTestObjects.Destroy(inactiveWeapon);
            TheCircussyOneTestObjects.Destroy(activeCharacter);
            TheCircussyOneTestObjects.Destroy(inactiveCharacter);
            TheCircussyOneTestObjects.Destroy(enemyCatalog);
            TheCircussyOneTestObjects.Destroy(inactiveEnemy);
            TheCircussyOneTestObjects.Destroy(activeEnemy);
            TheCircussyOneTestObjects.Destroy(config);
        }
    }

    [Test]
    public void UpgradeSelectionSkipsInactiveUpgradesTalentsAndWeaponTargets()
    {
        WeaponDefinition activeWeapon = TheCircussyOneTestObjects.CreateWeaponDefinition(null, "active_weapon", "Active Weapon");
        WeaponDefinition inactiveWeapon = TheCircussyOneTestObjects.CreateWeaponDefinition(null, "inactive_weapon", "Inactive Weapon");
        inactiveWeapon.isActive = false;
        WeaponCatalog catalog = TheCircussyOneTestObjects.CreateWeaponCatalog(activeWeapon);
        WeaponLoadout loadout = new(catalog);

        UpgradeDefinition inactiveUpgrade = TheCircussyOneTestObjects.CreateWeaponStatUpgrade(activeWeapon, "inactive_damage", new UpgradeStatModifierDefinition(StatId.WeaponFlatDamage, StatModifierBucket.Flat, 1f));
        inactiveUpgrade.isActive = false;
        TalentDefinition inactiveTalent = TheCircussyOneTestObjects.CreateTalentDefinition("inactive_talent", "Inactive Talent");
        inactiveTalent.isActive = false;

        Assert.That(UpgradeSelectionRules.IsEligible(inactiveUpgrade, new UpgradeRunState(), loadout), Is.False);
        Assert.That(UpgradeSelectionRules.IsAddWeaponEligible(inactiveWeapon, loadout), Is.False);
        Assert.That(UpgradeSelectionRules.IsTalentEligible(inactiveTalent, new TalentRunState()), Is.False);

        TheCircussyOneTestObjects.Destroy(activeWeapon);
        TheCircussyOneTestObjects.Destroy(inactiveWeapon);
        TheCircussyOneTestObjects.Destroy(catalog);
        TheCircussyOneTestObjects.Destroy(inactiveUpgrade);
        TheCircussyOneTestObjects.Destroy(inactiveTalent);
    }

    [Test]
    public void CatalogValidationReportsInactiveAndNoActiveEntries()
    {
        GameConfig config = ScriptableObject.CreateInstance<GameConfig>();
        EnemyDefinition enemy = TheCircussyOneTestObjects.CreateEnemyDefinition(config);
        enemy.enemyId = "parked_enemy";
        enemy.isActive = false;
        EnemyCatalog catalog = TheCircussyOneTestObjects.CreateEnemyCatalog(enemy);

        var issues = catalog.ValidateContent();

        Assert.That(issues.Any(issue => issue.Code == "content.inactive"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "enemy.no-valid-enemies"), Is.True);

        TheCircussyOneTestObjects.Destroy(enemy);
        TheCircussyOneTestObjects.Destroy(catalog);
        TheCircussyOneTestObjects.Destroy(config);
    }

    [Test]
    public void ItemCatalogValidationReportsInactiveAndNoActiveEntries()
    {
        ItemDefinition item = TheCircussyOneTestObjects.CreateItemDefinition("parked_item", "Parked Item");
        item.isActive = false;
        ItemCatalog catalog = TheCircussyOneTestObjects.CreateItemCatalog(item);

        var issues = catalog.ValidateContent();

        Assert.That(issues.Any(issue => issue.Code == "content.inactive"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "item.no-valid-items"), Is.True);

        TheCircussyOneTestObjects.Destroy(item);
        TheCircussyOneTestObjects.Destroy(catalog);
    }

    [Test]
    public void CatalogValidationReportsInactiveStartingWeapons()
    {
        WeaponDefinition weapon = TheCircussyOneTestObjects.CreateWeaponDefinition(null, "parked_weapon", "Parked Weapon");
        weapon.isActive = false;
        WeaponCatalog weaponCatalog = TheCircussyOneTestObjects.CreateWeaponCatalog(weapon);
        PerformerDefinition character = TheCircussyOneTestObjects.CreatePerformerDefinition("parked_starter", "Parked Starter", weapon);
        PerformerCatalog performerCatalog = TheCircussyOneTestObjects.CreatePerformerCatalog(character, character);

        Assert.That(weaponCatalog.ValidateContent().Any(issue => issue.Code == "weapon.inactive-starting-weapon"), Is.True);
        Assert.That(performerCatalog.ValidateContent().Any(issue => issue.Code == "performer.inactive-starting-weapon"), Is.True);

        TheCircussyOneTestObjects.Destroy(weapon);
        TheCircussyOneTestObjects.Destroy(weaponCatalog);
        TheCircussyOneTestObjects.Destroy(character);
        TheCircussyOneTestObjects.Destroy(performerCatalog);
    }

    [Test]
    public void WorkbenchUsageSummaryReportsWeaponRelationships()
    {
        WeaponDefinition weapon = TheCircussyOneTestObjects.CreateWeaponDefinition(null, "test_weapon", "Test Weapon");
        weapon.supportedUpgradeStats.Add(StatId.WeaponFlatDamage);
        weapon.addWeaponShortDescription = "Add Test Weapon to the run.";
        PerformerDefinition character = TheCircussyOneTestObjects.CreatePerformerDefinition("weapon_user", "Weapon User", weapon);
        UpgradeDefinition ownedUpgrade = TheCircussyOneTestObjects.CreateWeaponStatUpgrade(weapon, "damage", new UpgradeStatModifierDefinition(StatId.WeaponFlatDamage, StatModifierBucket.Flat, 1f));
        weapon.upgradeTrack.Add(ownedUpgrade);
        WeaponCatalog weaponCatalog = TheCircussyOneTestObjects.CreateWeaponCatalog(weapon);
        PerformerCatalog performerCatalog = TheCircussyOneTestObjects.CreatePerformerCatalog(character, character);
        UpgradeCatalog upgradeCatalog = TheCircussyOneTestObjects.CreateUpgradeCatalog(ownedUpgrade);

        var context = new ContentWorkbenchUsageContext
        {
            WeaponCatalog = weaponCatalog,
            PerformerCatalog = performerCatalog,
            UpgradeCatalog = upgradeCatalog
        };

        ContentWorkbenchUsageSummary summary = ContentWorkbenchUsageSummary.Build(ContentWorkbenchDomain.Weapons, weapon, context);

        Assert.That(summary.Lines.Any(line => line.Contains("Starting loadout: Yes")), Is.True);
        Assert.That(summary.Lines.Any(line => line.Contains("Appears as level-up weapon: Yes")), Is.True);
        Assert.That(summary.Lines.Any(line => line.Contains("Level-up card text: Add Test Weapon to the run.")), Is.True);
        Assert.That(summary.Lines.Any(line => line.Contains("Starter for performers: Weapon User")), Is.True);
        Assert.That(summary.Lines.Any(line => line.Contains("Owned upgrade cards") && line.Contains("Damage")), Is.True);
        Assert.That(summary.Lines.Any(line => line.Contains("Supported stats") && line.Contains("Weapon Damage")), Is.True);

        TheCircussyOneTestObjects.Destroy(weapon);
        TheCircussyOneTestObjects.Destroy(character);
        TheCircussyOneTestObjects.Destroy(ownedUpgrade);
        TheCircussyOneTestObjects.Destroy(weaponCatalog);
        TheCircussyOneTestObjects.Destroy(performerCatalog);
        TheCircussyOneTestObjects.Destroy(upgradeCatalog);
    }

    [Test]
    public void WorkbenchUsageSummaryReportsItemRelationships()
    {
        ItemDefinition item = TheCircussyOneTestObjects.CreateItemDefinition(
            "rubber_soles",
            "Rubber Soles",
            maxStacks: 3,
            modifiers: new ItemStatModifierDefinition(StatId.PlayerArmor, StatModifierBucket.Flat, 2f));
        item.downsideStatModifiers.Add(new ItemStatModifierDefinition(StatId.PlayerMoveSpeedMultiplier, StatModifierBucket.AdditivePercent, -0.03f));
        ItemCatalog itemCatalog = TheCircussyOneTestObjects.CreateItemCatalog(item);
        var context = new ContentWorkbenchUsageContext
        {
            ItemCatalog = itemCatalog
        };

        ContentWorkbenchUsageSummary summary = ContentWorkbenchUsageSummary.Build(ContentWorkbenchDomain.Items, item, context);

        Assert.That(summary.Lines.Any(line => line.Contains("In item catalog: Yes")), Is.True);
        Assert.That(summary.Lines.Any(line => line.Contains("Stacking: Stack Linear x3")), Is.True);
        Assert.That(summary.Lines.Any(line => line.Contains("Upside stats") && line.Contains("Armor")), Is.True);
        Assert.That(summary.Lines.Any(line => line.Contains("Downside stats") && line.Contains("Movement Speed")), Is.True);

        TheCircussyOneTestObjects.Destroy(item);
        TheCircussyOneTestObjects.Destroy(itemCatalog);
    }

    [Test]
    public void WorkbenchUsageSummaryReportsPerformerAndTalentRelationships()
    {
        WeaponDefinition weapon = TheCircussyOneTestObjects.CreateWeaponDefinition(null, "juggling_ball", "Juggling Ball");
        TalentDefinition talent = TheCircussyOneTestObjects.CreateTalentDefinition("solo_tempo", "Solo Tempo");
        talent.poolKind = TalentPoolKind.PerformerSpecific;
        talent.performerId = "solo";
        talent.statModifiers.Add(new UpgradeStatModifierDefinition(StatId.PlayerMoveSpeedMultiplier, StatModifierBucket.AdditivePercent, 0.05f));
        PerformerDefinition character = TheCircussyOneTestObjects.CreatePerformerDefinition("solo", "Solo", weapon, talent);
        TalentCatalog talentCatalog = TheCircussyOneTestObjects.CreateTalentCatalog(talent);
        PerformerCatalog performerCatalog = TheCircussyOneTestObjects.CreatePerformerCatalog(character, character);
        var context = new ContentWorkbenchUsageContext
        {
            PerformerCatalog = performerCatalog,
            TalentCatalog = talentCatalog
        };

        ContentWorkbenchUsageSummary characterSummary = ContentWorkbenchUsageSummary.Build(ContentWorkbenchDomain.Performers, character, context);
        ContentWorkbenchUsageSummary talentSummary = ContentWorkbenchUsageSummary.Build(ContentWorkbenchDomain.Talents, talent, context);

        Assert.That(characterSummary.Lines.Any(line => line.Contains("Starting weapon: Juggling Ball")), Is.True);
        Assert.That(characterSummary.Lines.Any(line => line.Contains("Eligible talents: Solo Tempo")), Is.True);
        Assert.That(talentSummary.Lines.Any(line => line.Contains("Pool: Performer-specific: solo")), Is.True);
        Assert.That(talentSummary.Lines.Any(line => line.Contains("Eligible performers: Solo")), Is.True);
        Assert.That(talentSummary.Lines.Any(line => line.Contains("Affected stats") && line.Contains("Movement Speed")), Is.True);

        TheCircussyOneTestObjects.Destroy(weapon);
        TheCircussyOneTestObjects.Destroy(talent);
        TheCircussyOneTestObjects.Destroy(character);
        TheCircussyOneTestObjects.Destroy(talentCatalog);
        TheCircussyOneTestObjects.Destroy(performerCatalog);
    }

    [Test]
    public void WorkbenchUsageSummaryReportsUpgradeAndEnemyRelationships()
    {
        WeaponDefinition weapon = TheCircussyOneTestObjects.CreateWeaponDefinition(null, "cannon", "Cannon");
        UpgradeDefinition upgrade = TheCircussyOneTestObjects.CreateWeaponStatUpgrade(weapon, "blast_radius", new UpgradeStatModifierDefinition(StatId.WeaponSplashRadiusMultiplier, StatModifierBucket.AdditivePercent, 0.05f));
        weapon.upgradeTrack.Add(upgrade);
        UpgradeCatalog upgradeCatalog = TheCircussyOneTestObjects.CreateUpgradeCatalog(upgrade);
        GameConfig config = ScriptableObject.CreateInstance<GameConfig>();
        EnemyDefinition enemy = TheCircussyOneTestObjects.CreateEnemyDefinition(config);
        enemy.displayName = "Normal Enemy";
        enemy.spawnWeight = 75f;
        enemy.xpBudget = 7;
        EnemyCatalog enemyCatalog = TheCircussyOneTestObjects.CreateEnemyCatalog(enemy);
        var context = new ContentWorkbenchUsageContext
        {
            UpgradeCatalog = upgradeCatalog,
            EnemyCatalog = enemyCatalog
        };

        ContentWorkbenchUsageSummary upgradeSummary = ContentWorkbenchUsageSummary.Build(ContentWorkbenchDomain.Upgrades, upgrade, context);
        ContentWorkbenchUsageSummary enemySummary = ContentWorkbenchUsageSummary.Build(ContentWorkbenchDomain.Enemies, enemy, context);

        Assert.That(upgradeSummary.Lines.Any(line => line.Contains("Target weapon: Cannon")), Is.True);
        Assert.That(upgradeSummary.Lines.Any(line => line.Contains("Listed on weapon track: Yes")), Is.True);
        Assert.That(upgradeSummary.Lines.Any(line => line.Contains("Weapon Splash Radius")), Is.True);
        Assert.That(enemySummary.Lines.Any(line => line.Contains("Default spawn enemy: Yes")), Is.True);
        Assert.That(enemySummary.Lines.Any(line => line.Contains("Spawn weight: 75")), Is.True);
        Assert.That(enemySummary.Lines.Any(line => line.Contains("XP budget: 7")), Is.True);

        TheCircussyOneTestObjects.Destroy(weapon);
        TheCircussyOneTestObjects.Destroy(upgrade);
        TheCircussyOneTestObjects.Destroy(upgradeCatalog);
        TheCircussyOneTestObjects.Destroy(config);
        TheCircussyOneTestObjects.Destroy(enemy);
        TheCircussyOneTestObjects.Destroy(enemyCatalog);
    }

    [Test]
    public void WorkbenchPreviewGeneratesIdentityWithoutUserStrings()
    {
        const string previewFolder = "Assets/__ContentWorkbenchPreviewOnly";
        AssetDatabase.DeleteAsset(previewFolder);
        var request = new ContentWorkbenchCreateRequest
        {
            Domain = ContentWorkbenchDomain.Items,
            Template = ContentWorkbenchCreateTemplate.PassiveItem,
            StatId = StatId.PlayerArmor,
            Bucket = StatModifierBucket.Flat,
            Value = 2f,
            AssetFolderOverride = previewFolder
        };

        ContentWorkbenchIdentityPreview preview = ContentWorkbenchService.PreviewCreate(request);

        Assert.That(preview.DisplayName, Is.EqualTo("Armor Item"));
        Assert.That(preview.Id, Is.EqualTo("armor_item"));
        Assert.That(preview.AssetPath, Is.EqualTo("Assets/__ContentWorkbenchPreviewOnly/ArmorItem.asset"));
        Assert.That(AssetDatabase.IsValidFolder(previewFolder), Is.False);
    }

    [Test]
    public void WorkbenchPreviewGeneratesChestIdentityWithoutUserStrings()
    {
        const string previewFolder = "Assets/__ContentWorkbenchChestPreviewOnly";
        AssetDatabase.DeleteAsset(previewFolder);
        var request = new ContentWorkbenchCreateRequest
        {
            Domain = ContentWorkbenchDomain.Chests,
            Template = ContentWorkbenchCreateTemplate.Chest,
            CatalogPathOverride = "Assets/__ContentWorkbenchChestPreviewOnly/MissingChestCatalog.asset",
            AssetFolderOverride = previewFolder
        };

        ContentWorkbenchIdentityPreview preview = ContentWorkbenchService.PreviewCreate(request);

        Assert.That(preview.DisplayName, Is.EqualTo("Locked Chest"));
        Assert.That(preview.Id, Is.EqualTo("locked_chest"));
        Assert.That(preview.AssetPath, Is.EqualTo("Assets/__ContentWorkbenchChestPreviewOnly/LockedChest.asset"));
        Assert.That(AssetDatabase.IsValidFolder(previewFolder), Is.False);
    }

    [Test]
    public void WorkbenchPreviewGeneratesIdFromDesignerName()
    {
        const string previewFolder = "Assets/__ContentWorkbenchNamedPreviewOnly";
        AssetDatabase.DeleteAsset(previewFolder);
        var request = new ContentWorkbenchCreateRequest
        {
            Domain = ContentWorkbenchDomain.Items,
            Template = ContentWorkbenchCreateTemplate.PassiveItem,
            DisplayName = "Lucky Feather",
            StatId = StatId.Luck,
            Bucket = StatModifierBucket.Flat,
            Value = 3f,
            AssetFolderOverride = previewFolder
        };

        ContentWorkbenchIdentityPreview preview = ContentWorkbenchService.PreviewCreate(request);

        Assert.That(preview.DisplayName, Is.EqualTo("Lucky Feather"));
        Assert.That(preview.Id, Is.EqualTo("lucky_feather"));
        Assert.That(preview.AssetPath, Is.EqualTo("Assets/__ContentWorkbenchNamedPreviewOnly/LuckyFeather.asset"));
        Assert.That(AssetDatabase.IsValidFolder(previewFolder), Is.False);
    }

    [Test]
    public void WorkbenchCreatesWeaponTemplateAndCatalogsItWithoutOverwriting()
    {
        const string testRoot = "Assets/__ContentWorkbenchTests";
        string weaponFolder = $"{testRoot}/Weapons";
        string weaponCatalogPath = $"{testRoot}/WeaponCatalog.asset";
        AssetDatabase.DeleteAsset(testRoot);
        EnsureFolder(testRoot);

        var weaponCatalog = ScriptableObject.CreateInstance<WeaponCatalog>();
        AssetDatabase.CreateAsset(weaponCatalog, weaponCatalogPath);

        var request = new ContentWorkbenchCreateRequest
        {
            Domain = ContentWorkbenchDomain.Weapons,
            Template = ContentWorkbenchCreateTemplate.OrbitWeapon,
            Id = "custom_hoop",
            DisplayName = "Custom Hoop",
            AddToCatalog = true,
            AssetFolderOverride = weaponFolder,
            CatalogPathOverride = weaponCatalogPath
        };

        try
        {
            ContentWorkbenchActionResult result = ContentWorkbenchService.CreateFromTemplate(request);
            Assert.That(result.Success, Is.True, result.Message);
            Assert.That(result.Asset, Is.TypeOf<WeaponDefinition>());

            weaponCatalog = AssetDatabase.LoadAssetAtPath<WeaponCatalog>(weaponCatalogPath);
            Assert.That(weaponCatalog.availableWeapons, Has.Count.EqualTo(1));
            Assert.That(weaponCatalog.availableWeapons[0].isActive, Is.True);

            ContentWorkbenchActionResult duplicate = ContentWorkbenchService.CreateFromTemplate(request);
            Assert.That(duplicate.Success, Is.False);
            Assert.That(duplicate.Message, Does.Contain("never overwritten"));
        }
        finally
        {
            AssetDatabase.DeleteAsset(testRoot);
            AssetDatabase.Refresh();
        }
    }

    [Test]
    public void WorkbenchCreatesItemTemplateAndCatalogsItWithoutOverwriting()
    {
        const string testRoot = "Assets/__ContentWorkbenchItemTests";
        string itemFolder = $"{testRoot}/Items";
        string itemCatalogPath = $"{testRoot}/ItemCatalog.asset";
        AssetDatabase.DeleteAsset(testRoot);
        EnsureFolder(testRoot);

        var itemCatalog = ScriptableObject.CreateInstance<ItemCatalog>();
        AssetDatabase.CreateAsset(itemCatalog, itemCatalogPath);

        var request = new ContentWorkbenchCreateRequest
        {
            Domain = ContentWorkbenchDomain.Items,
            Template = ContentWorkbenchCreateTemplate.PassiveItem,
            AddToCatalog = true,
            AssetFolderOverride = itemFolder,
            CatalogPathOverride = itemCatalogPath,
            StatId = StatId.PlayerArmor,
            Bucket = StatModifierBucket.Flat,
            Value = 2f
        };

        try
        {
            ContentWorkbenchActionResult result = ContentWorkbenchService.CreateFromTemplate(request);
            Assert.That(result.Success, Is.True, result.Message);
            Assert.That(result.Asset, Is.TypeOf<ItemDefinition>());

            itemCatalog = AssetDatabase.LoadAssetAtPath<ItemCatalog>(itemCatalogPath);
            Assert.That(itemCatalog.items, Has.Count.EqualTo(1));
            Assert.That(itemCatalog.items[0].Id, Is.EqualTo("armor_item"));
            Assert.That(itemCatalog.items[0].DisplayName, Is.EqualTo("Armor Item"));
            Assert.That(itemCatalog.items[0].isActive, Is.True);
            Assert.That(itemCatalog.items[0].statModifiers[0].statId, Is.EqualTo(StatId.PlayerArmor));
            Assert.That(itemCatalog.items[0].statModifiers[0].value, Is.EqualTo(2f));

            ContentWorkbenchActionResult duplicate = ContentWorkbenchService.CreateFromTemplate(request);
            Assert.That(duplicate.Success, Is.True);
            itemCatalog = AssetDatabase.LoadAssetAtPath<ItemCatalog>(itemCatalogPath);
            Assert.That(itemCatalog.items, Has.Count.EqualTo(2));
            Assert.That(itemCatalog.items[1].Id, Is.EqualTo("armor_item_002"));
        }
        finally
        {
            AssetDatabase.DeleteAsset(testRoot);
            AssetDatabase.Refresh();
        }
    }

    [Test]
    public void WorkbenchRejectsDuplicateDesignerName()
    {
        const string testRoot = "Assets/__ContentWorkbenchDuplicateNameTests";
        string itemFolder = $"{testRoot}/Items";
        string itemCatalogPath = $"{testRoot}/ItemCatalog.asset";
        AssetDatabase.DeleteAsset(testRoot);
        EnsureFolder(testRoot);

        var itemCatalog = ScriptableObject.CreateInstance<ItemCatalog>();
        AssetDatabase.CreateAsset(itemCatalog, itemCatalogPath);

        var request = new ContentWorkbenchCreateRequest
        {
            Domain = ContentWorkbenchDomain.Items,
            Template = ContentWorkbenchCreateTemplate.PassiveItem,
            DisplayName = "Lucky Feather",
            AddToCatalog = true,
            AssetFolderOverride = itemFolder,
            CatalogPathOverride = itemCatalogPath,
            StatId = StatId.Luck,
            Bucket = StatModifierBucket.Flat,
            Value = 3f
        };

        try
        {
            ContentWorkbenchActionResult first = ContentWorkbenchService.CreateFromTemplate(request);
            Assert.That(first.Success, Is.True, first.Message);

            ContentWorkbenchActionResult duplicate = ContentWorkbenchService.CreateFromTemplate(request);
            Assert.That(duplicate.Success, Is.False);
            Assert.That(duplicate.Message, Does.Contain("Lucky Feather"));
            Assert.That(duplicate.Message, Does.Contain("never overwritten"));

            itemCatalog = AssetDatabase.LoadAssetAtPath<ItemCatalog>(itemCatalogPath);
            Assert.That(itemCatalog.items, Has.Count.EqualTo(1));
            Assert.That(itemCatalog.items[0].Id, Is.EqualTo("lucky_feather"));
        }
        finally
        {
            AssetDatabase.DeleteAsset(testRoot);
            AssetDatabase.Refresh();
        }
    }

    [Test]
    public void WorkbenchAvailabilityAndDuplicateActionsAreNonDestructive()
    {
        const string testRoot = "Assets/__ContentWorkbenchActions";
        string weaponCatalogPath = $"{testRoot}/WeaponCatalog.asset";
        string weaponPath = $"{testRoot}/OriginalWeapon.asset";
        AssetDatabase.DeleteAsset(testRoot);
        EnsureFolder(testRoot);

        WeaponDefinition weapon = TheCircussyOneTestObjects.CreateWeaponDefinition(null, "original_weapon", "Original Weapon");
        var catalog = ScriptableObject.CreateInstance<WeaponCatalog>();
        AssetDatabase.CreateAsset(weapon, weaponPath);
        AssetDatabase.CreateAsset(catalog, weaponCatalogPath);

        try
        {
            ContentWorkbenchActionResult makeAvailable = ContentWorkbenchService.MakeAvailable(ContentWorkbenchDomain.Weapons, weapon, weaponCatalogPath);
            Assert.That(makeAvailable.Success, Is.True);
            Assert.That(weapon.isActive, Is.True);
            Assert.That(catalog.availableWeapons, Has.Count.EqualTo(1));

            ContentWorkbenchActionResult disable = ContentWorkbenchService.Disable(weapon);
            Assert.That(disable.Success, Is.True);
            Assert.That(weapon.isActive, Is.False);
            Assert.That(catalog.availableWeapons, Has.Count.EqualTo(1));

            ContentWorkbenchActionResult draft = ContentWorkbenchService.MoveToDraft(ContentWorkbenchDomain.Weapons, weapon, weaponCatalogPath);
            Assert.That(draft.Success, Is.True);
            Assert.That(catalog.availableWeapons, Is.Empty);
            Assert.That(AssetDatabase.LoadAssetAtPath<WeaponDefinition>(weaponPath), Is.Not.Null);

            ContentWorkbenchActionResult duplicate = ContentWorkbenchService.Duplicate(
                ContentWorkbenchDomain.Weapons,
                weapon,
                null,
                null,
                addToCatalog: false);

            Assert.That(duplicate.Success, Is.True, duplicate.Message);
            Assert.That(duplicate.Asset, Is.TypeOf<WeaponDefinition>());
            var copy = (WeaponDefinition)duplicate.Asset;
            Assert.That(copy.Id, Is.EqualTo("original_weapon_copy"));
            Assert.That(copy.DisplayName, Is.EqualTo("Original Weapon Copy"));
            Assert.That(copy.isActive, Is.True);
            Assert.That(catalog.availableWeapons, Is.Empty);
        }
        finally
        {
            AssetDatabase.DeleteAsset(testRoot);
            AssetDatabase.Refresh();
        }
    }

    [Test]
    public void WorkbenchDeleteBlocksAvailableContent()
    {
        const string testRoot = "Assets/__ContentWorkbenchDeleteAvailable";
        string weaponPath = $"{testRoot}/DeleteMe.asset";
        string weaponCatalogPath = $"{testRoot}/WeaponCatalog.asset";
        AssetDatabase.DeleteAsset(testRoot);
        EnsureFolder(testRoot);

        WeaponDefinition weapon = TheCircussyOneTestObjects.CreateWeaponDefinition(null, "delete_me", "Delete Me");
        var catalog = ScriptableObject.CreateInstance<WeaponCatalog>();
        catalog.availableWeapons.Add(weapon);
        AssetDatabase.CreateAsset(weapon, weaponPath);
        AssetDatabase.CreateAsset(catalog, weaponCatalogPath);

        try
        {
            var overrides = new ContentWorkbenchCatalogPathOverrides { WeaponCatalogPath = weaponCatalogPath };
            ContentWorkbenchDeletePreview preview = ContentWorkbenchService.PreviewDelete(ContentWorkbenchDomain.Weapons, weapon, overrides);

            Assert.That(preview.CanDelete, Is.False);
            Assert.That(preview.Blockers.Any(blocker => blocker.Contains("Available")), Is.True);
            Assert.That(AssetDatabase.LoadAssetAtPath<WeaponDefinition>(weaponPath), Is.Not.Null);
        }
        finally
        {
            AssetDatabase.DeleteAsset(testRoot);
            AssetDatabase.Refresh();
        }
    }

    [Test]
    public void WorkbenchDeleteDraftWeaponDeletesWeaponAsset()
    {
        const string testRoot = "Assets/__ContentWorkbenchDeleteWeapon";
        string weaponPath = $"{testRoot}/Weapons/Skibidi.asset";
        string weaponCatalogPath = $"{testRoot}/WeaponCatalog.asset";
        AssetDatabase.DeleteAsset(testRoot);
        EnsureFolder($"{testRoot}/Weapons");

        WeaponDefinition weapon = TheCircussyOneTestObjects.CreateWeaponDefinition(null, "skibidi", "Skibidi");
        var weaponCatalog = ScriptableObject.CreateInstance<WeaponCatalog>();
        AssetDatabase.CreateAsset(weapon, weaponPath);
        AssetDatabase.CreateAsset(weaponCatalog, weaponCatalogPath);

        try
        {
            var overrides = new ContentWorkbenchCatalogPathOverrides
            {
                WeaponCatalogPath = weaponCatalogPath
            };

            ContentWorkbenchDeletePreview preview = ContentWorkbenchService.PreviewDelete(ContentWorkbenchDomain.Weapons, weapon, overrides);
            Assert.That(preview.CanDelete, Is.True, string.Join("\n", preview.Blockers));
            Assert.That(preview.AssetPaths, Does.Contain(weaponPath));

            ContentWorkbenchActionResult result = ContentWorkbenchService.DeleteContent(ContentWorkbenchDomain.Weapons, weapon, overrides);

            Assert.That(result.Success, Is.True, result.Message);
            Assert.That(AssetDatabase.LoadAssetAtPath<WeaponDefinition>(weaponPath), Is.Null);
        }
        finally
        {
            AssetDatabase.DeleteAsset(testRoot);
            AssetDatabase.Refresh();
        }
    }

    [Test]
    public void WorkbenchDeleteWeaponBlocksPerformerStarterReferences()
    {
        const string testRoot = "Assets/__ContentWorkbenchDeleteWeaponBlocked";
        string weaponPath = $"{testRoot}/Weapons/Starter.asset";
        string characterPath = $"{testRoot}/Performers/User.asset";
        string weaponCatalogPath = $"{testRoot}/WeaponCatalog.asset";
        string performerCatalogPath = $"{testRoot}/PerformerCatalog.asset";
        AssetDatabase.DeleteAsset(testRoot);
        EnsureFolder($"{testRoot}/Weapons");
        EnsureFolder($"{testRoot}/Performers");

        WeaponDefinition weapon = TheCircussyOneTestObjects.CreateWeaponDefinition(null, "starter", "Starter");
        PerformerDefinition character = TheCircussyOneTestObjects.CreatePerformerDefinition("user", "User", weapon);
        var weaponCatalog = ScriptableObject.CreateInstance<WeaponCatalog>();
        var performerCatalog = ScriptableObject.CreateInstance<PerformerCatalog>();
        performerCatalog.performers.Add(character);
        performerCatalog.defaultPerformer = character;
        AssetDatabase.CreateAsset(weapon, weaponPath);
        AssetDatabase.CreateAsset(character, characterPath);
        AssetDatabase.CreateAsset(weaponCatalog, weaponCatalogPath);
        AssetDatabase.CreateAsset(performerCatalog, performerCatalogPath);

        try
        {
            var overrides = new ContentWorkbenchCatalogPathOverrides
            {
                WeaponCatalogPath = weaponCatalogPath,
                PerformerCatalogPath = performerCatalogPath
            };

            ContentWorkbenchDeletePreview preview = ContentWorkbenchService.PreviewDelete(ContentWorkbenchDomain.Weapons, weapon, overrides);

            Assert.That(preview.CanDelete, Is.False);
            Assert.That(preview.Blockers.Any(blocker => blocker.Contains("starting weapon")), Is.True);
            Assert.That(AssetDatabase.LoadAssetAtPath<WeaponDefinition>(weaponPath), Is.Not.Null);
        }
        finally
        {
            AssetDatabase.DeleteAsset(testRoot);
            AssetDatabase.Refresh();
        }
    }

    [Test]
    public void WorkbenchDeleteUpgradeRemovesWeaponTrackReferences()
    {
        const string testRoot = "Assets/__ContentWorkbenchDeleteUpgrade";
        string weaponPath = $"{testRoot}/Weapons/Cannon.asset";
        string upgradePath = $"{testRoot}/Upgrades/Blast.asset";
        string weaponCatalogPath = $"{testRoot}/WeaponCatalog.asset";
        string upgradeCatalogPath = $"{testRoot}/UpgradeCatalog.asset";
        AssetDatabase.DeleteAsset(testRoot);
        EnsureFolder($"{testRoot}/Weapons");
        EnsureFolder($"{testRoot}/Upgrades");

        WeaponDefinition weapon = TheCircussyOneTestObjects.CreateWeaponDefinition(null, "cannon", "Cannon");
        UpgradeDefinition upgrade = TheCircussyOneTestObjects.CreateWeaponStatUpgrade(
            weapon,
            "blast",
            new UpgradeStatModifierDefinition(StatId.WeaponSplashRadiusMultiplier, StatModifierBucket.AdditivePercent, 0.05f));
        upgrade.isActive = false;
        weapon.upgradeTrack.Add(upgrade);
        var weaponCatalog = ScriptableObject.CreateInstance<WeaponCatalog>();
        weaponCatalog.availableWeapons.Add(weapon);
        var upgradeCatalog = ScriptableObject.CreateInstance<UpgradeCatalog>();
        upgradeCatalog.upgrades.Add(upgrade);
        AssetDatabase.CreateAsset(weapon, weaponPath);
        AssetDatabase.CreateAsset(upgrade, upgradePath);
        AssetDatabase.CreateAsset(weaponCatalog, weaponCatalogPath);
        AssetDatabase.CreateAsset(upgradeCatalog, upgradeCatalogPath);

        try
        {
            var overrides = new ContentWorkbenchCatalogPathOverrides
            {
                WeaponCatalogPath = weaponCatalogPath,
                UpgradeCatalogPath = upgradeCatalogPath
            };

            ContentWorkbenchActionResult result = ContentWorkbenchService.DeleteContent(ContentWorkbenchDomain.Upgrades, upgrade, overrides);

            Assert.That(result.Success, Is.True, result.Message);
            Assert.That(AssetDatabase.LoadAssetAtPath<UpgradeDefinition>(upgradePath), Is.Null);
            weapon = AssetDatabase.LoadAssetAtPath<WeaponDefinition>(weaponPath);
            upgradeCatalog = AssetDatabase.LoadAssetAtPath<UpgradeCatalog>(upgradeCatalogPath);
            Assert.That(weapon.upgradeTrack, Is.Empty);
            Assert.That(upgradeCatalog.upgrades, Is.Empty);
        }
        finally
        {
            AssetDatabase.DeleteAsset(testRoot);
            AssetDatabase.Refresh();
        }
    }

    [Test]
    public void WorkbenchDeleteTalentRemovesTalentCatalogReference()
    {
        const string testRoot = "Assets/__ContentWorkbenchDeleteTalent";
        string weaponPath = $"{testRoot}/Weapons/JugglingBall.asset";
        string characterPath = $"{testRoot}/Performers/Solo.asset";
        string talentPath = $"{testRoot}/Talents/SoloTempo.asset";
        string performerCatalogPath = $"{testRoot}/PerformerCatalog.asset";
        string talentCatalogPath = $"{testRoot}/TalentCatalog.asset";
        AssetDatabase.DeleteAsset(testRoot);
        EnsureFolder($"{testRoot}/Weapons");
        EnsureFolder($"{testRoot}/Performers");
        EnsureFolder($"{testRoot}/Talents");

        WeaponDefinition weapon = TheCircussyOneTestObjects.CreateWeaponDefinition(null, "juggling_ball", "Juggling Ball");
        TalentDefinition talent = TheCircussyOneTestObjects.CreateTalentDefinition("solo_tempo", "Solo Tempo");
        talent.isActive = false;
        PerformerDefinition character = TheCircussyOneTestObjects.CreatePerformerDefinition("solo", "Solo", weapon, talent);
        var performerCatalog = ScriptableObject.CreateInstance<PerformerCatalog>();
        performerCatalog.performers.Add(character);
        performerCatalog.defaultPerformer = character;
        var talentCatalog = ScriptableObject.CreateInstance<TalentCatalog>();
        talentCatalog.talents.Add(talent);
        AssetDatabase.CreateAsset(weapon, weaponPath);
        AssetDatabase.CreateAsset(character, characterPath);
        AssetDatabase.CreateAsset(talent, talentPath);
        AssetDatabase.CreateAsset(performerCatalog, performerCatalogPath);
        AssetDatabase.CreateAsset(talentCatalog, talentCatalogPath);

        try
        {
            var overrides = new ContentWorkbenchCatalogPathOverrides
            {
                PerformerCatalogPath = performerCatalogPath,
                TalentCatalogPath = talentCatalogPath
            };

            ContentWorkbenchActionResult result = ContentWorkbenchService.DeleteContent(ContentWorkbenchDomain.Talents, talent, overrides);

            Assert.That(result.Success, Is.True, result.Message);
            Assert.That(AssetDatabase.LoadAssetAtPath<TalentDefinition>(talentPath), Is.Null);
            talentCatalog = AssetDatabase.LoadAssetAtPath<TalentCatalog>(talentCatalogPath);
            Assert.That(talentCatalog.talents, Is.Empty);
        }
        finally
        {
            AssetDatabase.DeleteAsset(testRoot);
            AssetDatabase.Refresh();
        }
    }

    [Test]
    public void WorkbenchDeleteFallbackPerformerIsBlocked()
    {
        const string testRoot = "Assets/__ContentWorkbenchDeleteFallbackPerformer";
        string weaponPath = $"{testRoot}/Weapons/JugglingBall.asset";
        string characterPath = $"{testRoot}/Performers/Fallback.asset";
        string performerCatalogPath = $"{testRoot}/PerformerCatalog.asset";
        AssetDatabase.DeleteAsset(testRoot);
        EnsureFolder($"{testRoot}/Weapons");
        EnsureFolder($"{testRoot}/Performers");

        WeaponDefinition weapon = TheCircussyOneTestObjects.CreateWeaponDefinition(null, "juggling_ball", "Juggling Ball");
        PerformerDefinition character = TheCircussyOneTestObjects.CreatePerformerDefinition("fallback_character", "Fallback Character", weapon);
        character.isActive = false;
        var performerCatalog = ScriptableObject.CreateInstance<PerformerCatalog>();
        performerCatalog.performers.Add(character);
        performerCatalog.defaultPerformer = character;
        AssetDatabase.CreateAsset(weapon, weaponPath);
        AssetDatabase.CreateAsset(character, characterPath);
        AssetDatabase.CreateAsset(performerCatalog, performerCatalogPath);

        try
        {
            var overrides = new ContentWorkbenchCatalogPathOverrides { PerformerCatalogPath = performerCatalogPath };
            ContentWorkbenchDeletePreview preview = ContentWorkbenchService.PreviewDelete(ContentWorkbenchDomain.Performers, character, overrides);

            Assert.That(preview.CanDelete, Is.False);
            Assert.That(preview.Blockers.Any(blocker => blocker.Contains("fallback performer")), Is.True);
            Assert.That(AssetDatabase.LoadAssetAtPath<PerformerDefinition>(characterPath), Is.Not.Null);
        }
        finally
        {
            AssetDatabase.DeleteAsset(testRoot);
            AssetDatabase.Refresh();
        }
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

    private static void SetPrivateField<T>(object target, string fieldName, T value)
    {
        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null, fieldName);
        field.SetValue(target, value);
    }

    private static T GetPrivateField<T>(object target, string fieldName)
    {
        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null, fieldName);
        return (T)field.GetValue(target);
    }

    private static void InvokePrivate(object target, string methodName, params object[] arguments)
    {
        MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(method, Is.Not.Null, methodName);
        method.Invoke(target, arguments);
    }

    private static string ExpectedFolder(ContentWorkbenchDomain domain)
    {
        return domain switch
        {
            ContentWorkbenchDomain.Weapons => TheCircussyOneAssetPaths.WeaponBalanceFolder,
            ContentWorkbenchDomain.Items => TheCircussyOneAssetPaths.ItemBalanceFolder,
            ContentWorkbenchDomain.Chests => TheCircussyOneAssetPaths.ChestBalanceFolder,
            ContentWorkbenchDomain.Performers => TheCircussyOneAssetPaths.PerformerBalanceFolder,
            ContentWorkbenchDomain.Enemies => TheCircussyOneAssetPaths.EnemyBalanceFolder,
            ContentWorkbenchDomain.Headliners => TheCircussyOneAssetPaths.HeadlinerBalanceFolder,
            ContentWorkbenchDomain.Upgrades => TheCircussyOneAssetPaths.UpgradeBalanceFolder,
            ContentWorkbenchDomain.Talents => TheCircussyOneAssetPaths.TalentBalanceFolder,
            _ => TheCircussyOneAssetPaths.BalanceFolder
        };
    }

    private static string ExpectedCatalogPath(ContentWorkbenchDomain domain)
    {
        return domain switch
        {
            ContentWorkbenchDomain.Weapons => TheCircussyOneAssetPaths.WeaponCatalogPath,
            ContentWorkbenchDomain.Items => TheCircussyOneAssetPaths.ItemCatalogPath,
            ContentWorkbenchDomain.Chests => TheCircussyOneAssetPaths.ChestCatalogPath,
            ContentWorkbenchDomain.Performers => TheCircussyOneAssetPaths.PerformerCatalogPath,
            ContentWorkbenchDomain.Enemies => TheCircussyOneAssetPaths.EnemyCatalogPath,
            ContentWorkbenchDomain.Headliners => TheCircussyOneAssetPaths.HeadlinerCatalogPath,
            ContentWorkbenchDomain.Upgrades => TheCircussyOneAssetPaths.UpgradeCatalogPath,
            ContentWorkbenchDomain.Talents => TheCircussyOneAssetPaths.TalentCatalogPath,
            _ => string.Empty
        };
    }

    private static System.Type ExpectedDefinitionType(ContentWorkbenchDomain domain)
    {
        return domain switch
        {
            ContentWorkbenchDomain.Weapons => typeof(WeaponDefinition),
            ContentWorkbenchDomain.Items => typeof(ItemDefinition),
            ContentWorkbenchDomain.Chests => typeof(ChestDefinition),
            ContentWorkbenchDomain.Performers => typeof(PerformerDefinition),
            ContentWorkbenchDomain.Enemies => typeof(EnemyDefinition),
            ContentWorkbenchDomain.Headliners => typeof(HeadlinerDefinition),
            ContentWorkbenchDomain.Upgrades => typeof(UpgradeDefinition),
            ContentWorkbenchDomain.Talents => typeof(TalentDefinition),
            _ => typeof(UnityEngine.Object)
        };
    }

    private static System.Type ExpectedCatalogType(ContentWorkbenchDomain domain)
    {
        return domain switch
        {
            ContentWorkbenchDomain.Weapons => typeof(WeaponCatalog),
            ContentWorkbenchDomain.Items => typeof(ItemCatalog),
            ContentWorkbenchDomain.Chests => typeof(ChestCatalog),
            ContentWorkbenchDomain.Performers => typeof(PerformerCatalog),
            ContentWorkbenchDomain.Enemies => typeof(EnemyCatalog),
            ContentWorkbenchDomain.Headliners => typeof(HeadlinerCatalog),
            ContentWorkbenchDomain.Upgrades => typeof(UpgradeCatalog),
            ContentWorkbenchDomain.Talents => typeof(TalentCatalog),
            _ => typeof(UnityEngine.Object)
        };
    }

    private static void AssertHasShowIfWithoutEnableIf(System.Type type, string fieldName)
    {
        FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null, $"{type.Name}.{fieldName}");
        Assert.That(field.GetCustomAttributes(typeof(ShowIfAttribute), inherit: false), Is.Not.Empty, $"{type.Name}.{fieldName} should hide when inactive.");
        Assert.That(field.GetCustomAttributes(typeof(EnableIfAttribute), inherit: false), Is.Empty, $"{type.Name}.{fieldName} should not render as a greyed-out inactive field.");
    }
}
