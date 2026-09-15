using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using TheCircussyOne.Runtime;
using TheCircussyOne.Stats;
using TheCircussyOne.Visuals;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public sealed class UpgradeSelectionTests
{
    private readonly List<Object> _created = new();

    [TearDown]
    public void TearDown()
    {
        TheCircussyOneTestObjects.DestroyAll();
        for (int i = _created.Count - 1; i >= 0; i--)
        {
            TheCircussyOneTestObjects.Destroy(_created[i]);
        }

        _created.Clear();
    }

    [Test]
    public void UpgradeCatalogValidationCatchesInvalidDefinitionsAndEffects()
    {
        UpgradeDefinition first = Track(TheCircussyOneTestObjects.CreateUpgradeDefinition("move_speed", "Move Speed"));
        UpgradeDefinition duplicate = Track(TheCircussyOneTestObjects.CreateUpgradeDefinition("move_speed", "Duplicate Move Speed"));
        UpgradeDefinition missing = Track(TheCircussyOneTestObjects.CreateUpgradeDefinition("", ""));
        missing.upgradeId = "";
        missing.displayName = "";
        UpgradeDefinition invalidMax = Track(TheCircussyOneTestObjects.CreateUpgradeDefinition("invalid_max", "Invalid Max"));
        invalidMax.maxLevel = 0;
        UpgradeDefinition missingStats = Track(TheCircussyOneTestObjects.CreateUpgradeDefinition("missing_stats", "Missing Stats"));
        missingStats.statModifiers.Clear();
        UpgradeDefinition invalidStat = Track(TheCircussyOneTestObjects.CreateUpgradeDefinition(
            "invalid_stat",
            "Invalid Stat",
            ContentRarity.Common,
            5,
            new UpgradeStatModifierDefinition((StatId)999, StatModifierBucket.Flat, 1f)));
        UpgradeDefinition missingWeaponStat = Track(ScriptableObject.CreateInstance<UpgradeDefinition>());
        missingWeaponStat.ApplyWeaponStatUpgradeDefaults(
            null,
            "damage",
            "Damage",
            "+10% damage.",
            Color.white,
            new UpgradeStatModifierDefinition(StatId.GlobalDamageMultiplier, StatModifierBucket.AdditivePercent, 0.1f));
        missingWeaponStat.weaponId = "";
        missingWeaponStat.weaponDefinition = null;
        WeaponDefinition weapon = Track(CreateTestWeapon("cannon", "Cannon"));
        weapon.supportedUpgradeStats.Clear();
        weapon.supportedUpgradeStats.Add(StatId.WeaponFlatDamage);
        UpgradeDefinition unsupportedWeaponStat = Track(CreateWeaponStatUpgrade(
            weapon,
            "bounce",
            "Bounce",
            new UpgradeStatModifierDefinition(StatId.WeaponBounce, StatModifierBucket.Flat, 1f)));
        UpgradeCatalog catalog = Track(TheCircussyOneTestObjects.CreateUpgradeCatalog(first, duplicate, missing, invalidMax, missingStats, invalidStat, missingWeaponStat, unsupportedWeaponStat, null));

        List<ContentValidationIssue> issues = catalog.ValidateContent();

        Assert.That(issues.Any(issue => issue.Code == "catalog.null-entry"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "content.duplicate-id"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "content.invalid-id"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "content.missing-display-name"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "upgrade.invalid-max-level"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "upgrade.missing-stat-modifiers"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "upgrade.invalid-stat-modifier"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "upgrade.non-weapon-stat"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "upgrade.non-weapon-stat" && issue.Message.Contains("weapon-local stats")), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "upgrade.missing-weapon-id"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "upgrade.missing-weapon-definition"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "upgrade.unsupported-weapon-stat" && issue.Message.Contains("target weapon 'Cannon'")), Is.True);
    }

    [Test]
    public void TalentCatalogValidationCatchesInvalidDefinitionsAndEffects()
    {
        TalentDefinition first = Track(TheCircussyOneTestObjects.CreateTalentDefinition("footwork", "Footwork"));
        TalentDefinition duplicate = Track(TheCircussyOneTestObjects.CreateTalentDefinition("footwork", "Duplicate Footwork"));
        TalentDefinition missing = Track(TheCircussyOneTestObjects.CreateTalentDefinition("", ""));
        missing.talentId = "";
        missing.displayName = "";
        TalentDefinition missingStats = Track(TheCircussyOneTestObjects.CreateTalentDefinition("missing_stats", "Missing Stats"));
        missingStats.statModifiers.Clear();
        TalentDefinition invalidStat = Track(TheCircussyOneTestObjects.CreateTalentDefinition("invalid_stat", "Invalid Stat"));
        invalidStat.statModifiers.Clear();
        invalidStat.statModifiers.Add(new UpgradeStatModifierDefinition((StatId)999, StatModifierBucket.Flat, 1f));
        TalentDefinition weaponStat = Track(TheCircussyOneTestObjects.CreateTalentDefinition("weapon_stat", "Weapon Stat"));
        weaponStat.statModifiers.Clear();
        weaponStat.statModifiers.Add(new UpgradeStatModifierDefinition(StatId.WeaponFlatDamage, StatModifierBucket.Flat, 1f));
        TalentDefinition cappedInvalid = Track(TheCircussyOneTestObjects.CreateTalentDefinition("bad_cap", "Bad Cap", repeatPolicy: TalentRepeatPolicy.Capped, maxLevel: 0));
        TalentDefinition characterSpecific = Track(TheCircussyOneTestObjects.CreateTalentDefinition("performer_only", "Performer Only"));
        characterSpecific.poolKind = TalentPoolKind.PerformerSpecific;
        characterSpecific.performerId = "";
        TalentDefinition invalidAccess = Track(TheCircussyOneTestObjects.CreateTalentDefinition("invalid_access", "Invalid Access"));
        invalidAccess.sharedPerformerAccess.Add(new TalentPerformerAccess { enabled = true, performerId = "" });
        TalentDefinition duplicateAccess = Track(TheCircussyOneTestObjects.CreateTalentDefinition("duplicate_access", "Duplicate Access"));
        duplicateAccess.sharedPerformerAccess.Add(new TalentPerformerAccess { enabled = true, performerId = "acrobat" });
        duplicateAccess.sharedPerformerAccess.Add(new TalentPerformerAccess { enabled = false, performerId = "acrobat" });
        TalentCatalog catalog = Track(TheCircussyOneTestObjects.CreateTalentCatalog(first, duplicate, missing, missingStats, invalidStat, weaponStat, cappedInvalid, characterSpecific, invalidAccess, duplicateAccess, null));

        List<ContentValidationIssue> issues = catalog.ValidateContent();

        Assert.That(issues.Any(issue => issue.Code == "catalog.null-entry"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "content.duplicate-id"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "content.invalid-id"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "content.missing-display-name"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "talent.missing-stat-modifiers"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "talent.invalid-stat-modifier"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "talent.weapon-stat" && issue.Message.Contains("Weapon Damage")), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "talent.invalid-max-level"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "talent.missing-performer-id"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "talent.invalid-performer-access"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "talent.duplicate-performer-access"), Is.True);
    }

    [Test]
    public void LevelUpCatalogValidationWarnsAboutMechanicalShortDescriptions()
    {
        WeaponDefinition weapon = Track(CreateTestWeapon("cannon", "Cannon"));
        weapon.supportedUpgradeStats.Clear();
        weapon.supportedUpgradeStats.Add(StatId.WeaponFlatDamage);
        UpgradeDefinition upgrade = Track(CreateWeaponStatUpgrade(
            weapon,
            "damage",
            "Damage",
            new UpgradeStatModifierDefinition(StatId.WeaponFlatDamage, StatModifierBucket.Flat, 1f)));
        upgrade.shortDescription = "Rarity-scaled +1 to +2 damage.";
        UpgradeCatalog upgradeCatalog = Track(TheCircussyOneTestObjects.CreateUpgradeCatalog(upgrade));

        TalentDefinition talent = Track(TheCircussyOneTestObjects.CreateTalentDefinition("quick_hands", "Quick Hands"));
        talent.shortDescription = "Rarity-scaled +5% to +10% Attack Speed.";
        TalentCatalog talentCatalog = Track(TheCircussyOneTestObjects.CreateTalentCatalog(talent));

        List<ContentValidationIssue> upgradeIssues = upgradeCatalog.ValidateContent();
        List<ContentValidationIssue> talentIssues = talentCatalog.ValidateContent();

        Assert.That(upgradeIssues.Any(issue => issue.Code == "upgrade.mechanical-short-description" && issue.Severity == ContentValidationSeverity.Warning), Is.True);
        Assert.That(talentIssues.Any(issue => issue.Code == "talent.mechanical-short-description" && issue.Severity == ContentValidationSeverity.Warning), Is.True);
    }

    [Test]
    public void RarityRollAndFallbackFillThreeUniqueEligibleChoices()
    {
        TalentDefinition commonA = Track(TheCircussyOneTestObjects.CreateTalentDefinition("common_a", "Common A", ContentRarity.Common));
        TalentDefinition commonB = Track(TheCircussyOneTestObjects.CreateTalentDefinition("common_b", "Common B", ContentRarity.Common));
        TalentDefinition rare = Track(TheCircussyOneTestObjects.CreateTalentDefinition("rare_a", "Rare A", ContentRarity.Rare));
        TalentDefinition legendary = Track(TheCircussyOneTestObjects.CreateTalentDefinition("legendary_a", "Legendary A", ContentRarity.Legendary));
        commonA.possibleRarities = ContentRaritySet.Only(ContentRarity.Common);
        commonB.possibleRarities = ContentRaritySet.Only(ContentRarity.Common);
        rare.possibleRarities = ContentRaritySet.Only(ContentRarity.Rare);
        legendary.possibleRarities = ContentRaritySet.Only(ContentRarity.Legendary);
        UpgradeCatalog catalog = Track(TheCircussyOneTestObjects.CreateUpgradeCatalog());
        catalog.levelUpRarityWeights = new ContentRarityWeightTable { legendary = 1 };
        TalentCatalog talentCatalog = Track(TheCircussyOneTestObjects.CreateTalentCatalog(commonA, commonB, rare, legendary));

        List<UpgradeChoice> choices = UpgradeSelectionRules.BuildChoices(
            catalog,
            new UpgradeRunState(),
            seed: 1,
            choiceCount: 3,
            talentCatalog: talentCatalog,
            talentRunState: new TalentRunState());

        Assert.That(UpgradeSelectionRules.RollRarity(catalog.levelUpRarityWeights, 0f, 0f), Is.EqualTo(ContentRarity.Legendary));
        Assert.That(choices, Has.Count.EqualTo(3));
        Assert.That(choices.Select(choice => choice.Id), Does.Contain("legendary_a"));
        Assert.That(choices.Select(choice => choice.Id).Distinct().Count(), Is.EqualTo(3));
    }

    [Test]
    public void RarityScaledChoicesCarryRolledRarity()
    {
        TalentDefinition footwork = Track(TheCircussyOneTestObjects.CreateTalentDefinition(
            "footwork",
            "Footwork",
            modifiers: new UpgradeStatModifierDefinition(StatId.PlayerMoveSpeedMultiplier, StatModifierBucket.AdditivePercent, 0.05f)));
        footwork.possibleRarities = ContentRaritySet.All();
        UpgradeCatalog catalog = Track(TheCircussyOneTestObjects.CreateUpgradeCatalog());
        catalog.levelUpRarityWeights = new ContentRarityWeightTable { legendary = 1 };
        TalentCatalog talentCatalog = Track(TheCircussyOneTestObjects.CreateTalentCatalog(footwork));

        List<UpgradeChoice> choices = UpgradeSelectionRules.BuildChoices(
            catalog,
            new UpgradeRunState(),
            seed: 11,
            choiceCount: 1,
            talentCatalog: talentCatalog,
            talentRunState: new TalentRunState());

        Assert.That(choices, Has.Count.EqualTo(1));
        Assert.That(choices[0].Id, Is.EqualTo("footwork"));
        Assert.That(choices[0].Rarity, Is.EqualTo(ContentRarity.Legendary));
        Assert.That(choices[0].RolledRarity, Is.EqualTo(ContentRarity.Legendary));
    }

    [Test]
    public void CommonRollDoesNotSelectRestrictedRarePlusLineWhenCommonLineExists()
    {
        TalentDefinition common = Track(TheCircussyOneTestObjects.CreateTalentDefinition("common_line", "Common Line"));
        common.possibleRarities = ContentRaritySet.All();
        TalentDefinition rarePlus = Track(TheCircussyOneTestObjects.CreateTalentDefinition("rare_plus", "Rare Plus"));
        rarePlus.possibleRarities = ContentRaritySet.Range(ContentRarity.Rare, ContentRarity.Legendary);
        UpgradeCatalog catalog = Track(TheCircussyOneTestObjects.CreateUpgradeCatalog());
        catalog.levelUpRarityWeights = new ContentRarityWeightTable { common = 1 };
        TalentCatalog talentCatalog = Track(TheCircussyOneTestObjects.CreateTalentCatalog(common, rarePlus));

        List<UpgradeChoice> choices = UpgradeSelectionRules.BuildChoices(
            catalog,
            new UpgradeRunState(),
            seed: 12,
            choiceCount: 1,
            talentCatalog: talentCatalog,
            talentRunState: new TalentRunState());

        Assert.That(choices.Select(choice => choice.Id), Is.EqualTo(new[] { "common_line" }));
        Assert.That(choices[0].Rarity, Is.EqualTo(ContentRarity.Common));
    }

    [Test]
    public void DuplicateUpgradeLineCannotAppearUnderMultipleRaritiesInOnePopup()
    {
        TalentDefinition footwork = Track(TheCircussyOneTestObjects.CreateTalentDefinition("footwork", "Footwork"));
        footwork.possibleRarities = ContentRaritySet.All();
        UpgradeCatalog catalog = Track(TheCircussyOneTestObjects.CreateUpgradeCatalog());
        TalentCatalog talentCatalog = Track(TheCircussyOneTestObjects.CreateTalentCatalog(footwork));

        List<UpgradeChoice> choices = UpgradeSelectionRules.BuildChoices(
            catalog,
            new UpgradeRunState(),
            seed: 13,
            choiceCount: 3,
            talentCatalog: talentCatalog,
            talentRunState: new TalentRunState());

        Assert.That(choices, Has.Count.EqualTo(1));
        Assert.That(choices.Select(choice => choice.Id).Distinct().Count(), Is.EqualTo(choices.Count));
    }

    [Test]
    public void MaxLevelUpgradesRemainEligibleForNow()
    {
        WeaponDefinition weapon = Track(CreateTestWeapon("juggling_ball", "Juggling Ball"));
        UpgradeDefinition capped = Track(CreateWeaponStatUpgrade(
            weapon,
            "damage",
            "Damage",
            new UpgradeStatModifierDefinition(StatId.WeaponFlatDamage, StatModifierBucket.Flat, 1f)));
        capped.maxLevel = 1;
        UpgradeCatalog catalog = Track(TheCircussyOneTestObjects.CreateUpgradeCatalog(capped));
        var state = new UpgradeRunState();
        state.IncrementLevel(capped);
        var loadout = new WeaponLoadout(Track(TheCircussyOneTestObjects.CreateWeaponCatalog(weapon)));

        List<UpgradeChoice> choices = UpgradeSelectionRules.BuildChoices(catalog, state, seed: 2, choiceCount: 1, weaponLoadout: loadout);

        Assert.That(choices.Select(choice => choice.Id), Does.Contain("juggling_ball_damage"));
        Assert.That(choices[0].CurrentLevel, Is.EqualTo(1));
        Assert.That(choices[0].NextLevel, Is.EqualTo(2));
    }

    [Test]
    public void UpgradeChoiceProviderPreservesSelectionRuleSeedAndLuck()
    {
        GameConfig config = Track(TheCircussyOneTestObjects.CreateConfig());
        var stats = new RunStats(config);
        stats.AddModifier(StatModifier.Flat(StatId.Luck, 25f, "test_luck"));
        UpgradeDefinition commonA = Track(TheCircussyOneTestObjects.CreateUpgradeDefinition("common_a", "Common A", ContentRarity.Common));
        UpgradeDefinition commonB = Track(TheCircussyOneTestObjects.CreateUpgradeDefinition("common_b", "Common B", ContentRarity.Common));
        UpgradeDefinition rare = Track(TheCircussyOneTestObjects.CreateUpgradeDefinition("rare_a", "Rare A", ContentRarity.Rare));
        UpgradeDefinition legendary = Track(TheCircussyOneTestObjects.CreateUpgradeDefinition("legendary_a", "Legendary A", ContentRarity.Legendary));
        UpgradeCatalog catalog = Track(TheCircussyOneTestObjects.CreateUpgradeCatalog(commonA, commonB, rare, legendary));
        var runState = new UpgradeRunState();
        var provider = new UpgradeChoiceProvider(catalog, runState, stats, new WeaponLoadout(TheCircussyOneTestObjects.CreateWeaponCatalog()));

        List<UpgradeChoice> expected = UpgradeSelectionRules.BuildChoices(
            catalog,
            runState,
            seed: 3 * 7919 + 2,
            choiceCount: 3,
            luck: stats.GetFloat(StatId.Luck));
        List<UpgradeChoice> actual = provider.BuildChoices(level: 3, pendingCount: 2);

        Assert.That(actual.Select(choice => choice.Definition.Id), Is.EqualTo(expected.Select(choice => choice.Definition.Id)));
        Assert.That(stats.Modifiers, Has.Count.EqualTo(1));
        Assert.That(catalog.Definitions, Has.Count.EqualTo(4));
    }

    [Test]
    public void UpgradeChoiceProviderIncludesRunSeedWhenAvailable()
    {
        GameConfig config = Track(TheCircussyOneTestObjects.CreateConfig());
        var stats = new RunStats(config);
        UpgradeDefinition commonA = Track(TheCircussyOneTestObjects.CreateUpgradeDefinition("common_a", "Common A", ContentRarity.Common));
        UpgradeDefinition commonB = Track(TheCircussyOneTestObjects.CreateUpgradeDefinition("common_b", "Common B", ContentRarity.Common));
        UpgradeDefinition rare = Track(TheCircussyOneTestObjects.CreateUpgradeDefinition("rare_a", "Rare A", ContentRarity.Rare));
        UpgradeDefinition legendary = Track(TheCircussyOneTestObjects.CreateUpgradeDefinition("legendary_a", "Legendary A", ContentRarity.Legendary));
        UpgradeCatalog catalog = Track(TheCircussyOneTestObjects.CreateUpgradeCatalog(commonA, commonB, rare, legendary));
        var runState = new UpgradeRunState();
        var runSeedState = new RunSeedState();
        runSeedState.SetForTests(3003);
        var loadout = new WeaponLoadout(TheCircussyOneTestObjects.CreateWeaponCatalog());
        var provider = new UpgradeChoiceProvider(catalog, runState, stats, loadout, runSeedState: runSeedState);

        List<UpgradeChoice> expected = UpgradeSelectionRules.BuildChoices(
            catalog,
            runState,
            seed: DeterministicSeed.Combine(3003, 4, 1),
            choiceCount: 3,
            luck: stats.GetFloat(StatId.Luck),
            weaponLoadout: loadout);
        List<UpgradeChoice> actual = provider.BuildChoices(level: 4, pendingCount: 1);

        Assert.That(actual.Select(choice => choice.Id), Is.EqualTo(expected.Select(choice => choice.Id)));
        Assert.That(actual.Select(choice => choice.RolledRarity), Is.EqualTo(expected.Select(choice => choice.RolledRarity)));
    }

    [Test]
    public void RarityRollUsesOneHundredLuckAsNeutral()
    {
        ContentRarityWeightTable table = ContentRarityWeightTable.LevelUpDefault();

        Assert.That(UpgradeSelectionRules.RollRarity(table, 0.60f, 100f), Is.EqualTo(table.Pick(0.60f)));
        Assert.That(UpgradeSelectionRules.RollRarity(table, 0.94f, 300f), Is.EqualTo(ContentRarity.Legendary));
    }

    [Test]
    public void UpgradeChoiceProviderReturnsEmptyChoicesForMissingCatalog()
    {
        GameConfig config = Track(TheCircussyOneTestObjects.CreateConfig());
        var provider = new UpgradeChoiceProvider(null, new UpgradeRunState(), new RunStats(config), new WeaponLoadout(TheCircussyOneTestObjects.CreateWeaponCatalog()));

        List<UpgradeChoice> choices = provider.BuildChoices(level: 2, pendingCount: 0);

        Assert.That(choices, Is.Empty);
    }

    [Test]
    public void AddWeaponChoicesRequireOpenWeaponSlot()
    {
        WeaponDefinition starter = Track(CreateTestWeapon("juggling_ball", "Juggling Ball"));
        WeaponDefinition cannon = Track(CreateTestWeapon("cannon", "Cannon"));
        WeaponCatalog openCatalog = Track(CreateWeaponCatalog(starter, cannon));
        WeaponCatalog fullCatalog = Track(CreateWeaponCatalog(starter, cannon));
        var openLoadout = new WeaponLoadout(openCatalog, maxWeapons: 2);
        var fullLoadout = new WeaponLoadout(fullCatalog, maxWeapons: 1);
        UpgradeCatalog upgradeCatalog = Track(TheCircussyOneTestObjects.CreateUpgradeCatalog());
        var stats = new RunStats(Track(TheCircussyOneTestObjects.CreateConfig()));
        var openProvider = new UpgradeChoiceProvider(upgradeCatalog, new UpgradeRunState(), stats, openLoadout, weaponCatalog: openCatalog);
        var fullProvider = new UpgradeChoiceProvider(upgradeCatalog, new UpgradeRunState(), stats, fullLoadout, weaponCatalog: fullCatalog);

        Assert.That(UpgradeSelectionRules.IsAddWeaponEligible(cannon, openLoadout), Is.True);
        Assert.That(UpgradeSelectionRules.IsAddWeaponEligible(cannon, fullLoadout), Is.False);
        Assert.That(openProvider.BuildChoices(level: 2, pendingCount: 0, choiceCount: 1), Has.Count.EqualTo(1));
        Assert.That(fullProvider.BuildChoices(level: 2, pendingCount: 0, choiceCount: 1), Is.Empty);
    }

    [Test]
    public void AddWeaponChoicesAreFixedCommonCards()
    {
        WeaponDefinition cannon = Track(CreateTestWeapon("cannon", "Cannon"));
        UpgradeChoice choice = new(cannon);

        Assert.That(choice.Kind, Is.EqualTo(UpgradeChoiceKind.AddWeapon));
        Assert.That(choice.RolledRarity, Is.EqualTo(ContentRarity.Common));
        Assert.That(choice.ShortDescription, Is.EqualTo(cannon.AddWeaponShortDescription));
    }

    [Test]
    public void BuiltAddWeaponChoicesStayCommonEvenWhenRarityRollIsLegendary()
    {
        WeaponDefinition starter = Track(CreateTestWeapon("juggling_ball", "Juggling Ball"));
        WeaponDefinition cannon = Track(CreateTestWeapon("cannon", "Cannon"));
        WeaponCatalog weaponCatalog = Track(CreateWeaponCatalog(starter, cannon));
        var loadout = new WeaponLoadout(weaponCatalog, maxWeapons: 2);
        UpgradeCatalog upgradeCatalog = Track(TheCircussyOneTestObjects.CreateUpgradeCatalog());
        upgradeCatalog.levelUpRarityWeights = new ContentRarityWeightTable { legendary = 1 };

        List<UpgradeChoice> choices = UpgradeSelectionRules.BuildChoices(
            upgradeCatalog,
            new UpgradeRunState(),
            seed: 14,
            choiceCount: 1,
            weaponLoadout: loadout,
            weaponCatalog: weaponCatalog);

        Assert.That(choices, Has.Count.EqualTo(1));
        Assert.That(choices[0].Kind, Is.EqualTo(UpgradeChoiceKind.AddWeapon));
        Assert.That(choices[0].Rarity, Is.EqualTo(ContentRarity.Common));
    }

    [Test]
    public void AddWeaponChoicesRejectNonAcquirableWeapons()
    {
        WeaponDefinition starter = Track(CreateTestWeapon("juggling_ball", "Juggling Ball"));
        WeaponDefinition hidden = Track(CreateTestWeapon("hidden_weapon", "Hidden Weapon"));
        hidden.canAppearAsLevelUpWeapon = false;
        WeaponCatalog weaponCatalog = Track(CreateWeaponCatalog(starter, hidden));
        var loadout = new WeaponLoadout(weaponCatalog, maxWeapons: 2);
        UpgradeCatalog upgradeCatalog = Track(TheCircussyOneTestObjects.CreateUpgradeCatalog());
        var provider = new UpgradeChoiceProvider(
            upgradeCatalog,
            new UpgradeRunState(),
            new RunStats(Track(TheCircussyOneTestObjects.CreateConfig())),
            loadout,
            weaponCatalog: weaponCatalog);

        Assert.That(UpgradeSelectionRules.IsAddWeaponEligible(hidden, loadout), Is.False);
        Assert.That(provider.BuildChoices(level: 2, pendingCount: 0, choiceCount: 1), Is.Empty);
    }

    [Test]
    public void WeaponStatChoicesRequireOwnedWeapon()
    {
        WeaponDefinition starter = Track(CreateTestWeapon("juggling_ball", "Juggling Ball"));
        WeaponDefinition cannon = Track(CreateTestWeapon("cannon", "Cannon"));
        WeaponCatalog weaponCatalog = Track(TheCircussyOneTestObjects.CreateWeaponCatalog(starter));
        var loadout = new WeaponLoadout(weaponCatalog, maxWeapons: 2);
        UpgradeDefinition starterDamage = Track(CreateWeaponStatUpgrade(
            starter,
            "damage",
            "Damage",
            new UpgradeStatModifierDefinition(StatId.WeaponFlatDamage, StatModifierBucket.Flat, 5f)));
        UpgradeDefinition cannonDamage = Track(CreateWeaponStatUpgrade(
            cannon,
            "damage",
            "Damage",
            new UpgradeStatModifierDefinition(StatId.WeaponFlatDamage, StatModifierBucket.Flat, 5f)));
        UpgradeCatalog upgradeCatalog = Track(TheCircussyOneTestObjects.CreateUpgradeCatalog(starterDamage, cannonDamage));
        var provider = new UpgradeChoiceProvider(
            upgradeCatalog,
            new UpgradeRunState(),
            new RunStats(Track(TheCircussyOneTestObjects.CreateConfig())),
            loadout);

        Assert.That(UpgradeSelectionRules.IsEligible(starterDamage, new UpgradeRunState(), loadout), Is.True);
        Assert.That(UpgradeSelectionRules.IsEligible(cannonDamage, new UpgradeRunState(), loadout), Is.False);
        Assert.That(provider.BuildChoices(level: 2, pendingCount: 0, choiceCount: 2).Select(choice => choice.Definition.Id), Is.EquivalentTo(new[] { "juggling_ball_damage" }));
    }

    [Test]
    public void WeaponStatChoicesRequireSupportedWeaponStats()
    {
        WeaponDefinition weapon = Track(CreateTestWeapon("cannon", "Cannon"));
        weapon.supportedUpgradeStats.Clear();
        weapon.supportedUpgradeStats.Add(StatId.WeaponFlatDamage);
        WeaponCatalog weaponCatalog = Track(TheCircussyOneTestObjects.CreateWeaponCatalog(weapon));
        var loadout = new WeaponLoadout(weaponCatalog);
        UpgradeDefinition damage = Track(CreateWeaponStatUpgrade(
            weapon,
            "damage",
            "Damage",
            new UpgradeStatModifierDefinition(StatId.WeaponFlatDamage, StatModifierBucket.Flat, 5f)));
        UpgradeDefinition bounce = Track(CreateWeaponStatUpgrade(
            weapon,
            "bounce",
            "Bounce",
            new UpgradeStatModifierDefinition(StatId.WeaponBounce, StatModifierBucket.Flat, 1f)));
        UpgradeCatalog upgradeCatalog = Track(TheCircussyOneTestObjects.CreateUpgradeCatalog(damage, bounce));
        var provider = new UpgradeChoiceProvider(
            upgradeCatalog,
            new UpgradeRunState(),
            new RunStats(Track(TheCircussyOneTestObjects.CreateConfig())),
            loadout);

        Assert.That(UpgradeSelectionRules.IsEligible(damage, new UpgradeRunState(), loadout), Is.True);
        Assert.That(UpgradeSelectionRules.IsEligible(bounce, new UpgradeRunState(), loadout), Is.False);
        Assert.That(provider.BuildChoices(level: 2, pendingCount: 0, choiceCount: 2).Select(choice => choice.Definition.Id), Is.EquivalentTo(new[] { "cannon_damage" }));
    }

    [Test]
    public void TalentCatalogMakesSharedTalentsEligibleAndExcludesLegacyStatCards()
    {
        UpgradeDefinition legacyArmor = Track(TheCircussyOneTestObjects.CreateUpgradeDefinition(
            "armor",
            "Armor",
            ContentRarity.Common,
            5,
            new UpgradeStatModifierDefinition(StatId.PlayerArmor, StatModifierBucket.Flat, 5f)));
        GameConfig config = Track(TheCircussyOneTestObjects.CreateConfig());
        WeaponDefinition starterWeapon = Track(TheCircussyOneTestObjects.CreateWeaponDefinition(config));
        UpgradeDefinition weaponDamage = Track(CreateWeaponStatUpgrade(
            starterWeapon,
            "damage",
            "Damage",
            new UpgradeStatModifierDefinition(StatId.WeaponFlatDamage, StatModifierBucket.Flat, 1f)));
        UpgradeCatalog upgradeCatalog = Track(TheCircussyOneTestObjects.CreateUpgradeCatalog(legacyArmor, weaponDamage));
        TalentDefinition footwork = Track(TheCircussyOneTestObjects.CreateTalentDefinition(
            "footwork",
            "Footwork",
            ContentRarity.Common,
            modifiers: new UpgradeStatModifierDefinition(StatId.PlayerMoveSpeedMultiplier, StatModifierBucket.AdditivePercent, 0.1f)));
        TalentDefinition toughSkin = Track(TheCircussyOneTestObjects.CreateTalentDefinition(
            "tough_skin",
            "Tough Skin",
            ContentRarity.Common,
            modifiers: new UpgradeStatModifierDefinition(StatId.PlayerArmor, StatModifierBucket.Flat, 5f)));
        toughSkin.possibleRarities = ContentRaritySet.Only(ContentRarity.Common);
        TalentCatalog talentCatalog = Track(TheCircussyOneTestObjects.CreateTalentCatalog(footwork, toughSkin));
        var talentState = new TalentRunState();
        WeaponCatalog weaponCatalog = Track(TheCircussyOneTestObjects.CreateWeaponCatalog(starterWeapon));
        var provider = new UpgradeChoiceProvider(
            upgradeCatalog,
            new UpgradeRunState(),
            new RunStats(config),
            new WeaponLoadout(weaponCatalog),
            talentCatalog: talentCatalog,
            talentRunState: talentState);

        List<UpgradeChoice> choices = provider.BuildChoices(level: 2, pendingCount: 0, choiceCount: 3);

        string[] ids = choices.Select(choice => choice.Id).ToArray();
        Assert.That(ids, Does.Not.Contain("armor"));
        Assert.That(ids.Contains("footwork") || ids.Contains("tough_skin"), Is.True);
        Assert.That(choices.Any(choice => choice.Kind == UpgradeChoiceKind.Talent), Is.True);
    }

    [Test]
    public void TalentChoicesRejectWeaponLocalStatsAndItemsRemainExcluded()
    {
        GameConfig config = Track(TheCircussyOneTestObjects.CreateConfig());
        TalentDefinition invalidTalent = Track(TheCircussyOneTestObjects.CreateTalentDefinition("bad_weapon_talent", "Bad Weapon Talent"));
        invalidTalent.statModifiers.Clear();
        invalidTalent.statModifiers.Add(new UpgradeStatModifierDefinition(StatId.WeaponFlatDamage, StatModifierBucket.Flat, 1f));
        TalentCatalog talentCatalog = Track(TheCircussyOneTestObjects.CreateTalentCatalog(invalidTalent));
        ItemDefinition item = Track(TheCircussyOneTestObjects.CreateItemDefinition("safety_padding", "Safety Padding"));
        var itemInventory = new ItemInventory(new RunStats(config));
        itemInventory.AddItem(item);

        List<UpgradeChoice> choices = UpgradeSelectionRules.BuildChoices(
            Track(TheCircussyOneTestObjects.CreateUpgradeCatalog()),
            new UpgradeRunState(),
            seed: 15,
            choiceCount: 3,
            itemInventory: itemInventory,
            talentCatalog: talentCatalog,
            talentRunState: new TalentRunState());

        Assert.That(UpgradeSelectionRules.IsTalentEligible(invalidTalent, new TalentRunState()), Is.False);
        Assert.That(itemInventory.ItemCount, Is.EqualTo(1));
        Assert.That(choices, Is.Empty);
    }

    [Test]
    public void UpgradeEffectApplierAppliesStatModifiersWithStableSourceIdsAndRefreshesState()
    {
        GameConfig config = Track(TheCircussyOneTestObjects.CreateConfig());
        var stats = new RunStats(config);
        var gameState = new GameState(config, stats);
        var upgradeState = new UpgradeRunState();
        WeaponDefinition weapon = Track(TheCircussyOneTestObjects.CreateWeaponDefinition(config));
        weapon.projectileDamage = 10;
        weapon.baseFireIntervalSeconds = 0.5f;
        weapon.minimumFireIntervalSeconds = 0.1f;
        WeaponCatalog weaponCatalog = Track(TheCircussyOneTestObjects.CreateWeaponCatalog(weapon));
        var weaponLoadout = new WeaponLoadout(weaponCatalog);
        UpgradeDefinition damage = Track(CreateWeaponStatUpgrade(
            weapon,
            "damage",
            "Damage",
            new UpgradeStatModifierDefinition(StatId.WeaponFlatDamage, StatModifierBucket.Flat, 5f)));
        var applier = new UpgradeEffectApplier(
            upgradeState,
            stats,
            weaponLoadout,
            gameState);

        bool applied = applier.Apply(new UpgradeChoice(damage, 0));

        Assert.That(applied, Is.True);
        Assert.That(upgradeState.GetLevel(damage), Is.EqualTo(1));
        Assert.That(weaponLoadout.Weapons[0].Level, Is.EqualTo(2));
        Assert.That(weaponLoadout.Weapons[0].Modifiers.Select(modifier => modifier.SourceId), Does.Contain("upgrade:juggling_ball_damage:1"));
        Assert.That(WeaponStatRules.Damage(weapon, stats, weaponLoadout.Weapons[0].Modifiers), Is.EqualTo(15));
        Assert.That(WeaponStatRules.FireInterval(weapon, stats, weaponLoadout.Weapons[0].Modifiers), Is.EqualTo(0.5f).Within(0.0001f));
        Assert.That(stats.Modifiers, Is.Empty);
    }

    [Test]
    public void UpgradeEffectApplierRaisesWeaponLoadoutChangedWhenWeaponLevelChanges()
    {
        GameConfig config = Track(TheCircussyOneTestObjects.CreateConfig());
        var stats = new RunStats(config);
        var upgradeState = new UpgradeRunState();
        WeaponDefinition weapon = Track(TheCircussyOneTestObjects.CreateWeaponDefinition(config));
        WeaponCatalog weaponCatalog = Track(TheCircussyOneTestObjects.CreateWeaponCatalog(weapon));
        var weaponLoadout = new WeaponLoadout(weaponCatalog);
        int changedCount = 0;
        weaponLoadout.Changed += () => changedCount++;
        UpgradeDefinition damage = Track(CreateWeaponStatUpgrade(
            weapon,
            "damage",
            "Damage",
            new UpgradeStatModifierDefinition(StatId.WeaponFlatDamage, StatModifierBucket.Flat, 5f)));
        var applier = new UpgradeEffectApplier(
            upgradeState,
            stats,
            weaponLoadout,
            new GameState(config, stats));

        bool applied = applier.Apply(new UpgradeChoice(damage, 0));

        Assert.That(applied, Is.True);
        Assert.That(weaponLoadout.Weapons[0].Level, Is.EqualTo(2));
        Assert.That(changedCount, Is.EqualTo(1));
    }

    public void UpgradeEffectApplierAppliesWeaponLocalModifiersWithStableSourceIds()
    {
        GameConfig config = Track(TheCircussyOneTestObjects.CreateConfig());
        WeaponDefinition weapon = Track(TheCircussyOneTestObjects.CreateWeaponDefinition(config));
        weapon.projectileDamage = 10;
        WeaponCatalog weaponCatalog = Track(TheCircussyOneTestObjects.CreateWeaponCatalog(weapon));
        var weaponLoadout = new WeaponLoadout(weaponCatalog);
        var upgradeState = new UpgradeRunState();
        UpgradeDefinition damage = Track(CreateWeaponStatUpgrade(
            weapon,
            "damage",
            "Damage",
            new UpgradeStatModifierDefinition(StatId.WeaponFlatDamage, StatModifierBucket.Flat, 5f)));
        var stats = new RunStats(config);
        var applier = new UpgradeEffectApplier(
            upgradeState,
            stats,
            weaponLoadout,
            new GameState(config, stats));

        bool applied = applier.Apply(new UpgradeChoice(damage, 0));

        Assert.That(applied, Is.True);
        Assert.That(upgradeState.GetLevel(damage), Is.EqualTo(1));
        Assert.That(weaponLoadout.Weapons[0].Modifiers.Select(modifier => modifier.SourceId), Does.Contain("upgrade:juggling_ball_damage:1"));
        Assert.That(WeaponStatRules.Damage(weapon, stats, weaponLoadout.Weapons[0].Modifiers), Is.EqualTo(15));
        Assert.That(stats.GetFloat(StatId.GlobalDamageMultiplier), Is.EqualTo(1f).Within(0.001f));
    }

    [Test]
    public void UpgradeEffectApplierScalesWeaponLocalModifiersByRolledRarity()
    {
        GameConfig config = Track(TheCircussyOneTestObjects.CreateConfig());
        WeaponDefinition weapon = Track(TheCircussyOneTestObjects.CreateWeaponDefinition(config));
        weapon.projectileDamage = 20;
        WeaponCatalog weaponCatalog = Track(TheCircussyOneTestObjects.CreateWeaponCatalog(weapon));
        var weaponLoadout = new WeaponLoadout(weaponCatalog);
        var upgradeState = new UpgradeRunState();
        UpgradeDefinition damage = Track(CreateWeaponStatUpgrade(
            weapon,
            "damage",
            "Damage",
            new UpgradeStatModifierDefinition(StatId.WeaponFlatDamage, StatModifierBucket.Flat, 1f)));
        var stats = new RunStats(config);
        var applier = new UpgradeEffectApplier(
            upgradeState,
            stats,
            weaponLoadout,
            new GameState(config, stats));

        bool applied = applier.Apply(new UpgradeChoice(damage, 0, ContentRarity.Legendary));

        Assert.That(applied, Is.True);
        Assert.That(weaponLoadout.Weapons[0].Modifiers.Single().Value, Is.EqualTo(2f).Within(0.0001f));
        Assert.That(WeaponStatRules.Damage(weapon, stats, weaponLoadout.Weapons[0].Modifiers), Is.EqualTo(22));
        Assert.That(stats.GetFloat(StatId.GlobalDamageMultiplier), Is.EqualTo(1f).Within(0.001f));
    }

    [Test]
    public void UpgradeEffectApplierAddsWeaponDefinitionWhenSlotOpen()
    {
        GameConfig config = Track(TheCircussyOneTestObjects.CreateConfig());
        WeaponDefinition starter = Track(TheCircussyOneTestObjects.CreateWeaponDefinition(config));
        WeaponDefinition cannon = Track(CreateTestWeapon("cannon", "Cannon"));
        var weaponLoadout = new WeaponLoadout(Track(TheCircussyOneTestObjects.CreateWeaponCatalog(starter)), maxWeapons: 2);
        var upgradeState = new UpgradeRunState();
        var applier = new UpgradeEffectApplier(
            upgradeState,
            new RunStats(config),
            weaponLoadout,
            new GameState(config));

        bool applied = applier.Apply(new UpgradeChoice(cannon));

        Assert.That(applied, Is.True);
        Assert.That(weaponLoadout.OwnsWeapon("cannon"), Is.True);
        Assert.That(weaponLoadout.FindById("cannon").Level, Is.EqualTo(1));
    }

    [Test]
    public void UpgradeEffectApplierRejectsAddWeaponWhenSlotUnavailableWithoutMutatingState()
    {
        GameConfig config = Track(TheCircussyOneTestObjects.CreateConfig());
        WeaponDefinition starter = Track(TheCircussyOneTestObjects.CreateWeaponDefinition(config));
        WeaponDefinition cannon = Track(CreateTestWeapon("cannon", "Cannon"));
        var weaponLoadout = new WeaponLoadout(Track(TheCircussyOneTestObjects.CreateWeaponCatalog(starter)), maxWeapons: 1);
        var upgradeState = new UpgradeRunState();
        var applier = new UpgradeEffectApplier(
            upgradeState,
            new RunStats(config),
            weaponLoadout,
            new GameState(config));

        bool applied = applier.Apply(new UpgradeChoice(cannon));

        Assert.That(applied, Is.False);
        Assert.That(weaponLoadout.OwnsWeapon("cannon"), Is.False);
        Assert.That(weaponLoadout.WeaponCount, Is.EqualTo(1));
    }

    [Test]
    public void UpgradeEffectApplierRejectsInvalidChoiceWithoutMutatingState()
    {
        GameConfig config = Track(TheCircussyOneTestObjects.CreateConfig());
        var stats = new RunStats(config);
        var upgradeState = new UpgradeRunState();
        WeaponDefinition weapon = Track(TheCircussyOneTestObjects.CreateWeaponDefinition(config));
        var weaponLoadout = new WeaponLoadout(Track(TheCircussyOneTestObjects.CreateWeaponCatalog(weapon)));
        var applier = new UpgradeEffectApplier(upgradeState, stats, weaponLoadout, new GameState(config, stats));

        bool applied = applier.Apply(default);

        Assert.That(applied, Is.False);
        Assert.That(stats.Modifiers, Is.Empty);
        Assert.That(weaponLoadout.Weapons[0].Level, Is.EqualTo(1));
    }

    [Test]
    public void UpgradeEffectApplierRepeatedApplicationsUseSequentialSourceIds()
    {
        GameConfig config = Track(TheCircussyOneTestObjects.CreateConfig());
        var stats = new RunStats(config);
        var upgradeState = new UpgradeRunState();
        WeaponDefinition weapon = Track(TheCircussyOneTestObjects.CreateWeaponDefinition(config));
        weapon.projectileDamage = 10;
        WeaponCatalog weaponCatalog = Track(TheCircussyOneTestObjects.CreateWeaponCatalog(weapon));
        var weaponLoadout = new WeaponLoadout(weaponCatalog);
        UpgradeDefinition damage = Track(CreateWeaponStatUpgrade(
            weapon,
            "damage",
            "Damage",
            new UpgradeStatModifierDefinition(StatId.WeaponFlatDamage, StatModifierBucket.Flat, 2.5f)));
        var applier = new UpgradeEffectApplier(
            upgradeState,
            stats,
            weaponLoadout,
            new GameState(config, stats));

        Assert.That(applier.Apply(new UpgradeChoice(damage, 0)), Is.True);
        Assert.That(applier.Apply(new UpgradeChoice(damage, 1)), Is.True);

        Assert.That(upgradeState.GetLevel(damage), Is.EqualTo(2));
        Assert.That(weaponLoadout.Weapons[0].Level, Is.EqualTo(3));
        Assert.That(WeaponStatRules.Damage(weapon, stats, weaponLoadout.Weapons[0].Modifiers), Is.EqualTo(15));
        Assert.That(weaponLoadout.Weapons[0].Modifiers.Any(modifier => modifier.SourceId == "upgrade:juggling_ball_damage:1"), Is.True);
        Assert.That(weaponLoadout.Weapons[0].Modifiers.Any(modifier => modifier.SourceId == "upgrade:juggling_ball_damage:2"), Is.True);
    }

    [Test]
    public void TalentEffectApplierStacksRepeatableTalentsWithStableSourceIds()
    {
        GameConfig config = Track(TheCircussyOneTestObjects.CreateConfig());
        var stats = new RunStats(config);
        var gameState = new GameState(config, stats);
        var talentState = new TalentRunState();
        TalentDefinition footwork = Track(TheCircussyOneTestObjects.CreateTalentDefinition(
            "footwork",
            "Footwork",
            ContentRarity.Common,
            modifiers: new UpgradeStatModifierDefinition(StatId.PlayerMoveSpeedMultiplier, StatModifierBucket.AdditivePercent, 0.1f)));
        var applier = new TalentEffectApplier(talentState, stats, gameState);

        Assert.That(applier.Apply(footwork), Is.True);
        Assert.That(applier.Apply(footwork), Is.True);

        Assert.That(talentState.GetLevel(footwork), Is.EqualTo(2));
        Assert.That(stats.GetFloat(StatId.PlayerMoveSpeedMultiplier), Is.EqualTo(1.2f).Within(0.001f));
        Assert.That(stats.Modifiers.Any(modifier => modifier.SourceId == "talent:footwork:1"), Is.True);
        Assert.That(stats.Modifiers.Any(modifier => modifier.SourceId == "talent:footwork:2"), Is.True);
    }

    [Test]
    public void TalentEffectApplierScalesModifiersByRolledRarity()
    {
        GameConfig config = Track(TheCircussyOneTestObjects.CreateConfig());
        var stats = new RunStats(config);
        var gameState = new GameState(config, stats);
        var talentState = new TalentRunState();
        TalentDefinition footwork = Track(TheCircussyOneTestObjects.CreateTalentDefinition(
            "footwork",
            "Footwork",
            ContentRarity.Common,
            modifiers: new UpgradeStatModifierDefinition(StatId.PlayerMoveSpeedMultiplier, StatModifierBucket.AdditivePercent, 0.05f)));
        var applier = new TalentEffectApplier(talentState, stats, gameState);

        Assert.That(applier.Apply(footwork, ContentRarity.Common), Is.True);
        Assert.That(applier.Apply(footwork, ContentRarity.Legendary), Is.True);

        Assert.That(talentState.GetLevel(footwork), Is.EqualTo(2));
        Assert.That(stats.GetFloat(StatId.PlayerMoveSpeedMultiplier), Is.EqualTo(1.15f).Within(0.001f));
        Assert.That(stats.Modifiers[0].Value, Is.EqualTo(0.05f).Within(0.0001f));
        Assert.That(stats.Modifiers[1].Value, Is.EqualTo(0.10f).Within(0.0001f));
    }

    [Test]
    public void UpgradeEffectApplierDelegatesTalentChoices()
    {
        GameConfig config = Track(TheCircussyOneTestObjects.CreateConfig());
        var stats = new RunStats(config);
        var talentState = new TalentRunState();
        var gameState = new GameState(config, stats);
        TalentDefinition toughSkin = Track(TheCircussyOneTestObjects.CreateTalentDefinition(
            "tough_skin",
            "Tough Skin",
            ContentRarity.Common,
            modifiers: new UpgradeStatModifierDefinition(StatId.PlayerArmor, StatModifierBucket.Flat, 5f)));
        var talentApplier = new TalentEffectApplier(talentState, stats, gameState);
        var applier = new UpgradeEffectApplier(
            new UpgradeRunState(),
            stats,
            new WeaponLoadout(TheCircussyOneTestObjects.CreateWeaponCatalog()),
            gameState,
            talentEffectApplier: talentApplier);

        Assert.That(applier.Apply(new UpgradeChoice(toughSkin, 0, ContentRarity.Legendary)), Is.True);

        Assert.That(talentState.GetLevel(toughSkin), Is.EqualTo(1));
        Assert.That(stats.GetFloat(StatId.PlayerArmor), Is.EqualTo(10f).Within(0.001f));
    }

    [Test]
    public void LevelUpPausesRunAndStatChoiceAppliesModifierThenResumes()
    {
        GameConfig config = Track(TheCircussyOneTestObjects.CreateConfig());
        TalentDefinition toughSkin = Track(TheCircussyOneTestObjects.CreateTalentDefinition(
            "tough_skin",
            "Tough Skin",
            ContentRarity.Common,
            modifiers: new UpgradeStatModifierDefinition(StatId.PlayerArmor, StatModifierBucket.Flat, 5f)));
        toughSkin.possibleRarities = ContentRaritySet.Only(ContentRarity.Common);
        UpgradeCatalog catalog = Track(TheCircussyOneTestObjects.CreateUpgradeCatalog());
        TalentCatalog talentCatalog = Track(CreateThreeChoiceTalentCatalog(toughSkin));
        var talentState = new TalentRunState();
        var stats = new RunStats(config);
        var gameState = new GameState(config, stats);
        var upgradeState = new UpgradeRunState();
        var pauseState = new RunPauseState();
        var weaponLoadout = new WeaponLoadout(TheCircussyOneTestObjects.CreateWeaponCatalog());
        var system = CreateUpgradeSelectionSystem(gameState, catalog, upgradeState, stats, weaponLoadout, pauseState, talentCatalog, talentState);
        system.Start();

        gameState.AddExperience(gameState.ExperienceTarget);

        Assert.That(system.HasActiveSelection, Is.True);
        Assert.That(pauseState.IsPaused, Is.True);
        int toughSkinChoiceIndex = IndexOf(system.ActiveChoices, "tough_skin");
        Assert.That(toughSkinChoiceIndex, Is.GreaterThanOrEqualTo(0));

        Assert.That(system.SelectChoice(toughSkinChoiceIndex), Is.True);

        Assert.That(system.HasActiveSelection, Is.False);
        Assert.That(pauseState.IsPaused, Is.False);
        Assert.That(stats.GetFloat(StatId.PlayerArmor), Is.EqualTo(5f));
        Assert.That(stats.Modifiers.Any(modifier => modifier.SourceId == "talent:tough_skin:1"), Is.True);
        system.Dispose();
    }

    public void MultipleLevelUpsQueueSequentialSelections()
    {
        GameConfig config = Track(TheCircussyOneTestObjects.CreateConfig());
        UpgradeCatalog catalog = Track(TheCircussyOneTestObjects.CreateUpgradeCatalog());
        TalentCatalog talentCatalog = Track(CreateThreeChoiceTalentCatalog());
        var talentState = new TalentRunState();
        var stats = new RunStats(config);
        var gameState = new GameState(config, stats);
        var upgradeState = new UpgradeRunState();
        var pauseState = new RunPauseState();
        var system = CreateUpgradeSelectionSystem(
            gameState,
            catalog,
            upgradeState,
            stats,
            new WeaponLoadout(TheCircussyOneTestObjects.CreateWeaponCatalog()),
            pauseState,
            talentCatalog,
            talentState);
        system.Start();

        gameState.AddExperience(ExperienceRules.TotalExperienceRequiredForLevelUps(gameState.Level, 2, config.ExperienceCurve));

        Assert.That(system.HasActiveSelection, Is.True);
        Assert.That(system.PendingSelectionCount, Is.EqualTo(2));
        Assert.That(system.SelectChoice(0), Is.True);
        Assert.That(system.HasActiveSelection, Is.True);
        Assert.That(system.PendingSelectionCount, Is.EqualTo(1));
        Assert.That(pauseState.IsPaused, Is.True);
        Assert.That(system.SelectChoice(0), Is.True);
        Assert.That(system.HasActiveSelection, Is.False);
        Assert.That(system.PendingSelectionCount, Is.EqualTo(0));
        Assert.That(pauseState.IsPaused, Is.False);
        system.Dispose();
    }

    [Test]
    public void ResetRunStateClearsPendingUpgradeSelections()
    {
        GameConfig config = Track(TheCircussyOneTestObjects.CreateConfig());
        UpgradeCatalog catalog = Track(TheCircussyOneTestObjects.CreateUpgradeCatalog());
        TalentCatalog talentCatalog = Track(CreateThreeChoiceTalentCatalog());
        var talentState = new TalentRunState();
        var stats = new RunStats(config);
        var gameState = new GameState(config, stats);
        var upgradeState = new UpgradeRunState();
        var pauseState = new RunPauseState();
        var system = CreateUpgradeSelectionSystem(
            gameState,
            catalog,
            upgradeState,
            stats,
            new WeaponLoadout(TheCircussyOneTestObjects.CreateWeaponCatalog()),
            pauseState,
            talentCatalog,
            talentState);
        system.Start();

        gameState.AddExperience(ExperienceRules.TotalExperienceRequiredForLevelUps(gameState.Level, 2, config.ExperienceCurve));

        Assert.That(system.PendingSelectionCount, Is.EqualTo(2));
        Assert.That(pauseState.IsPaused, Is.True);

        system.ResetRunState();

        Assert.That(system.HasActiveSelection, Is.False);
        Assert.That(system.PendingSelectionCount, Is.Zero);
        Assert.That(pauseState.IsPaused, Is.False);
        system.Dispose();
    }

    [Test]
    public void GameOverDoesNotOpenUpgradeChoices()
    {
        GameConfig config = Track(TheCircussyOneTestObjects.CreateConfig());
        var stats = new RunStats(config);
        var gameState = new GameState(config, stats);
        gameState.DamagePlayer(config.playerMaxHealth);
        var system = CreateUpgradeSelectionSystem(
            gameState,
            Track(TheCircussyOneTestObjects.CreateUpgradeCatalog()),
            new UpgradeRunState(),
            stats,
            new WeaponLoadout(TheCircussyOneTestObjects.CreateWeaponCatalog()),
            new RunPauseState(),
            Track(CreateThreeChoiceTalentCatalog()),
            new TalentRunState());
        system.Start();

        gameState.AddExperience(gameState.ExperienceTarget);

        Assert.That(system.HasActiveSelection, Is.False);
        system.Dispose();
    }

    [Test]
    public void UpgradeNavigationRulesMapDirectionsAndRepeatDiscretely()
    {
        var repeat = new UpgradeNavigationRepeatState();

        Assert.That(UpgradeNavigationRules.IndexForDirection(UpgradeNavigationDirection.Left), Is.EqualTo(0));
        Assert.That(UpgradeNavigationRules.IndexForDirection(UpgradeNavigationDirection.Middle), Is.EqualTo(1));
        Assert.That(UpgradeNavigationRules.IndexForDirection(UpgradeNavigationDirection.Right), Is.EqualTo(2));
        Assert.That(UpgradeNavigationRules.IndexForDirection(UpgradeNavigationDirection.None), Is.EqualTo(-1));
        Assert.That(UpgradeNavigationRules.NextIndex(-1, 3, UpgradeNavigationDirection.Left), Is.EqualTo(0));
        Assert.That(UpgradeNavigationRules.NextIndex(-1, 3, UpgradeNavigationDirection.Middle), Is.EqualTo(1));
        Assert.That(UpgradeNavigationRules.NextIndex(-1, 3, UpgradeNavigationDirection.Right), Is.EqualTo(2));
        Assert.That(UpgradeNavigationRules.NextIndex(1, 3, UpgradeNavigationDirection.Middle), Is.EqualTo(1));
        Assert.That(UpgradeNavigationRules.NextIndex(1, 3, UpgradeNavigationDirection.Left), Is.EqualTo(0));
        Assert.That(UpgradeNavigationRules.NextIndex(1, 3, UpgradeNavigationDirection.Right), Is.EqualTo(2));
        Assert.That(UpgradeNavigationRules.NextIndex(0, 3, UpgradeNavigationDirection.Left), Is.EqualTo(2));
        Assert.That(UpgradeNavigationRules.NextIndex(2, 3, UpgradeNavigationDirection.Right), Is.EqualTo(0));

        repeat = new UpgradeNavigationRepeatState { RequiresNeutralRelease = true };
        Assert.That(UpgradeNavigationRules.ShouldTrigger(ref repeat, UpgradeNavigationDirection.Right, 0f), Is.False);
        Assert.That(repeat.RequiresNeutralRelease, Is.True);
        Assert.That(UpgradeNavigationRules.ShouldTrigger(ref repeat, UpgradeNavigationDirection.None, 0.1f), Is.False);
        Assert.That(repeat.RequiresNeutralRelease, Is.False);
        Assert.That(UpgradeNavigationRules.ShouldTrigger(ref repeat, UpgradeNavigationDirection.Right, 0.2f), Is.True);

        repeat = default;
        Assert.That(UpgradeNavigationRules.ShouldTrigger(ref repeat, UpgradeNavigationDirection.Left, 0f), Is.True);
        Assert.That(UpgradeNavigationRules.ShouldTrigger(ref repeat, UpgradeNavigationDirection.Left, 0.1f), Is.False);
        Assert.That(UpgradeNavigationRules.ShouldTrigger(ref repeat, UpgradeNavigationDirection.Left, 0.29f), Is.True);
        Assert.That(UpgradeNavigationRules.ShouldTrigger(ref repeat, UpgradeNavigationDirection.None, 0.3f), Is.False);
        Assert.That(UpgradeNavigationRules.ShouldTrigger(ref repeat, UpgradeNavigationDirection.Left, 0.31f), Is.True);
    }

    [Test]
    public void UpgradePreviewRulesFormatSmartStatAndWeaponChanges()
    {
        GameConfig config = Track(TheCircussyOneTestObjects.CreateConfig());
        var stats = new RunStats(config);
        TalentDefinition moveSpeed = Track(TheCircussyOneTestObjects.CreateTalentDefinition(
            "move_speed",
            "Move Speed",
            ContentRarity.Common,
            modifiers: new UpgradeStatModifierDefinition(StatId.PlayerMoveSpeedMultiplier, StatModifierBucket.AdditivePercent, 0.1f)));
        TalentDefinition armor = Track(TheCircussyOneTestObjects.CreateTalentDefinition(
            "armor",
            "Armor",
            ContentRarity.Common,
            modifiers: new UpgradeStatModifierDefinition(StatId.PlayerArmor, StatModifierBucket.Flat, 5f)));
        TalentDefinition attackSpeed = Track(TheCircussyOneTestObjects.CreateTalentDefinition(
            "attack_speed",
            "Attack Speed",
            ContentRarity.Uncommon,
            modifiers: new UpgradeStatModifierDefinition(StatId.GlobalWeaponHaste, StatModifierBucket.AdditivePercent, 0.12f)));
        TalentDefinition fractionalJump = Track(TheCircussyOneTestObjects.CreateTalentDefinition(
            "springboard_half",
            "Springboard Half",
            ContentRarity.Common,
            modifiers: new UpgradeStatModifierDefinition(StatId.PlayerExtraJumps, StatModifierBucket.Flat, 0.75f)));
        WeaponDefinition weapon = Track(TheCircussyOneTestObjects.CreateWeaponDefinition(config));
        WeaponCatalog weaponCatalog = Track(TheCircussyOneTestObjects.CreateWeaponCatalog(weapon));
        var loadout = new WeaponLoadout(weaponCatalog);
        UpgradeDefinition weaponDamage = Track(CreateWeaponStatUpgrade(
            weapon,
            "damage",
            "Damage",
            new UpgradeStatModifierDefinition(StatId.WeaponFlatDamage, StatModifierBucket.Flat, 5f)));
        UpgradeDefinition weaponAttackSpeed = Track(CreateWeaponStatUpgrade(
            weapon,
            "attack_speed",
            "Attack Speed",
            new UpgradeStatModifierDefinition(StatId.WeaponAttackSpeed, StatModifierBucket.AdditivePercent, 0.12f)));
        weapon.chainEnabled = true;
        weapon.baseChainCount = 2;
        weapon.maxChainCount = 6;
        UpgradeDefinition weaponChain = Track(CreateWeaponStatUpgrade(
            weapon,
            "chain",
            "Chain",
            new UpgradeStatModifierDefinition(StatId.WeaponChain, StatModifierBucket.Flat, 1f)));
        UpgradeDefinition weaponHalfChain = Track(CreateWeaponStatUpgrade(
            weapon,
            "half_chain",
            "Half Chain",
            new UpgradeStatModifierDefinition(StatId.WeaponChain, StatModifierBucket.Flat, 0.5f)));
        TalentDefinition footwork = Track(TheCircussyOneTestObjects.CreateTalentDefinition(
            "footwork",
            "Footwork",
            ContentRarity.Common,
            modifiers: new UpgradeStatModifierDefinition(StatId.PlayerMoveSpeedMultiplier, StatModifierBucket.AdditivePercent, 0.1f)));
        UpgradePreviewFrame moveSpeedFrame = UpgradePreviewRules.BuildPreview(new UpgradeChoice(moveSpeed, 0), stats, loadout);
        UpgradePreviewFrame armorFrame = UpgradePreviewRules.BuildPreview(new UpgradeChoice(armor, 0), stats, loadout);
        UpgradePreviewFrame attackSpeedFrame = UpgradePreviewRules.BuildPreview(new UpgradeChoice(attackSpeed, 0, ContentRarity.Uncommon), stats, loadout);
        UpgradePreviewFrame fractionalJumpFrame = UpgradePreviewRules.BuildPreview(new UpgradeChoice(fractionalJump, 0), stats, loadout);
        UpgradePreviewFrame weaponDamageFrame = UpgradePreviewRules.BuildPreview(new UpgradeChoice(weaponDamage, 0), stats, loadout);
        UpgradePreviewFrame weaponAttackSpeedFrame = UpgradePreviewRules.BuildPreview(new UpgradeChoice(weaponAttackSpeed, 0), stats, loadout);
        UpgradePreviewFrame weaponChainFrame = UpgradePreviewRules.BuildPreview(new UpgradeChoice(weaponChain, 0), stats, loadout);
        UpgradePreviewFrame weaponHalfChainFrame = UpgradePreviewRules.BuildPreview(new UpgradeChoice(weaponHalfChain, 0), stats, loadout);
        UpgradePreviewFrame talentFrame = UpgradePreviewRules.BuildPreview(new UpgradeChoice(footwork, 0), stats, loadout);
        UpgradePreviewFrame legendaryTalentFrame = UpgradePreviewRules.BuildPreview(new UpgradeChoice(footwork, 0, ContentRarity.Legendary), stats, loadout);

        Assert.That(moveSpeedFrame.Text, Does.Contain("Movement Speed 100% -> <color=#73F59A>110%</color>"));
        Assert.That(moveSpeedFrame.BeneficialChange, Is.True);
        Assert.That(armorFrame.Text, Does.Contain("Armor 0 -> <color=#73F59A>5</color>"));
        Assert.That(attackSpeedFrame.Text, Does.Contain("Attack Speed 100% -> <color=#73F59A>114%</color>"));
        Assert.That(fractionalJumpFrame.Text, Does.Contain("Extra Jumps 0 -> <color=#73F59A>0.75</color>"));
        Assert.That(fractionalJumpFrame.BeneficialChange, Is.True);
        Assert.That(weaponDamageFrame.Text, Does.Contain("Juggling Ball"));
        Assert.That(weaponDamageFrame.Text, Does.Contain("Damage 51 -> <color=#73F59A>56</color>"));
        Assert.That(weaponAttackSpeedFrame.Text, Does.Contain("Weapon Attack Speed 100% -> <color=#73F59A>112%</color>"));
        Assert.That(weaponAttackSpeedFrame.Text, Does.Not.Contain("Fire Interval"));
        Assert.That(weaponChainFrame.Text, Does.Contain("Chain 2 -> <color=#73F59A>3</color>"));
        Assert.That(weaponHalfChainFrame.Text, Does.Contain("Chain 2 -> <color=#73F59A>2.5</color>"));
        Assert.That(talentFrame.Text, Does.Contain("Movement Speed 100% -> <color=#73F59A>110%</color>"));
        Assert.That(legendaryTalentFrame.Text, Does.Contain("Movement Speed 100% -> <color=#73F59A>120%</color>"));
    }

    [Test]
    public void WeaponUpgradePreviewRulesCoverSupportedWeaponStatsInsteadOfRepeatingDescription()
    {
        GameConfig config = Track(TheCircussyOneTestObjects.CreateConfig());
        var stats = new RunStats(config);
        WeaponDefinition weapon = Track(CreateTestWeapon("preview_weapon", "Preview Weapon"));
        weapon.baseProjectileCount = 1;
        weapon.maxProjectileCount = 6;
        weapon.projectileLifetimeSeconds = 2f;
        weapon.weaponRange = 9f;
        weapon.explosiveEnabled = true;
        weapon.baseSplashRadius = 1.5f;
        weapon.maxSplashRadius = 3f;
        weapon.bounceEnabled = true;
        weapon.baseBounceCount = 1;
        weapon.maxBounceCount = 5;
        weapon.accuracyAffectsAimError = true;
        weapon.baseAimErrorDegrees = 12f;
        WeaponCatalog weaponCatalog = Track(CreateWeaponCatalog(weapon));
        var loadout = new WeaponLoadout(weaponCatalog);

        var cases = new (string suffix, string label, UpgradeStatModifierDefinition modifier)[]
        {
            ("count", "Projectile Count", new UpgradeStatModifierDefinition(StatId.WeaponProjectileCount, StatModifierBucket.Flat, 1f)),
            ("lifetime", "Projectile Lifetime", new UpgradeStatModifierDefinition(StatId.WeaponProjectileLifetimeMultiplier, StatModifierBucket.AdditivePercent, 0.25f)),
            ("range", "Range", new UpgradeStatModifierDefinition(StatId.WeaponRangeMultiplier, StatModifierBucket.AdditivePercent, 0.25f)),
            ("splash", "Splash Radius", new UpgradeStatModifierDefinition(StatId.WeaponSplashRadiusMultiplier, StatModifierBucket.AdditivePercent, 0.25f)),
            ("bounce", "Bounce", new UpgradeStatModifierDefinition(StatId.WeaponBounce, StatModifierBucket.Flat, 1f)),
            ("accuracy", "Accuracy", new UpgradeStatModifierDefinition(StatId.WeaponAccuracyMultiplier, StatModifierBucket.AdditivePercent, 0.5f))
        };

        for (int i = 0; i < cases.Length; i++)
        {
            UpgradeDefinition upgrade = Track(CreateWeaponStatUpgrade(
                weapon,
                cases[i].suffix,
                cases[i].label,
                cases[i].modifier));
            upgrade.shortDescription = "Flavor text only.";

            UpgradePreviewFrame frame = UpgradePreviewRules.BuildPreview(new UpgradeChoice(upgrade, 0), stats, loadout);

            Assert.That(frame.Text, Does.Contain("Preview Weapon"));
            Assert.That(frame.Text, Does.Contain(cases[i].label));
            Assert.That(frame.Text, Does.Not.Contain(upgrade.shortDescription));
            Assert.That(frame.Text, Does.Not.Contain(" deg"));
            Assert.That(frame.Text, Does.Not.Contain("s ->"));
        }
    }

    [Test]
    public void OrbitWeaponUpgradePreviewUsesOrbitTerms()
    {
        GameConfig config = Track(TheCircussyOneTestObjects.CreateConfig());
        var stats = new RunStats(config);
        WeaponDefinition weapon = Track(CreateTestWeapon("fire_hoop", "Fire Hoop"));
        weapon.orbitEnabled = true;
        weapon.baseOrbitCount = 2;
        weapon.maxOrbitCount = 4;
        weapon.orbitRadius = 2.2f;
        weapon.maxOrbitRadius = 3.6f;
        weapon.orbitHitRadius = 0.55f;
        weapon.maxOrbitHitRadius = 1.1f;
        WeaponCatalog weaponCatalog = Track(CreateWeaponCatalog(weapon));
        var loadout = new WeaponLoadout(weaponCatalog);

        UpgradeDefinition count = Track(CreateWeaponStatUpgrade(
            weapon,
            "count",
            "Count",
            new UpgradeStatModifierDefinition(StatId.WeaponProjectileCount, StatModifierBucket.Flat, 1f)));
        UpgradeDefinition radius = Track(CreateWeaponStatUpgrade(
            weapon,
            "radius",
            "Radius",
            new UpgradeStatModifierDefinition(StatId.WeaponRangeMultiplier, StatModifierBucket.AdditivePercent, 0.1f)));
        UpgradeDefinition hitRadius = Track(CreateWeaponStatUpgrade(
            weapon,
            "size",
            "Size",
            new UpgradeStatModifierDefinition(StatId.WeaponProjectileSizeMultiplier, StatModifierBucket.AdditivePercent, 0.1f)));

        UpgradePreviewFrame countFrame = UpgradePreviewRules.BuildPreview(new UpgradeChoice(count, 0), stats, loadout);
        UpgradePreviewFrame radiusFrame = UpgradePreviewRules.BuildPreview(new UpgradeChoice(radius, 0), stats, loadout);
        UpgradePreviewFrame hitRadiusFrame = UpgradePreviewRules.BuildPreview(new UpgradeChoice(hitRadius, 0), stats, loadout);

        Assert.That(countFrame.Text, Does.Contain("Orbit Count 2 -> <color=#73F59A>3</color>"));
        Assert.That(radiusFrame.Text, Does.Contain("Orbit Radius"));
        Assert.That(hitRadiusFrame.Text, Does.Contain("Hoop Hit Radius"));
    }

    [Test]
    public void FirstPartyWeaponUpgradeAssetPreviewsDoNotRepeatFlavorDescriptions()
    {
        GameConfig config = Track(TheCircussyOneTestObjects.CreateConfig());
        var stats = new RunStats(config);
        string[] guids = AssetDatabase.FindAssets("t:UpgradeDefinition", new[] { "Assets/Game/ScriptableObjects/Balance/Upgrades" });
        Assert.That(guids.Length, Is.GreaterThan(0));

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            UpgradeDefinition upgrade = AssetDatabase.LoadAssetAtPath<UpgradeDefinition>(path);
            Assert.That(upgrade, Is.Not.Null, path);
            Assert.That(upgrade.weaponDefinition, Is.Not.Null, path);
            WeaponDefinition weapon = Track(Object.Instantiate(upgrade.weaponDefinition));
            WeaponCatalog weaponCatalog = Track(CreateWeaponCatalog(weapon));
            var loadout = new WeaponLoadout(weaponCatalog);

            UpgradePreviewFrame frame = UpgradePreviewRules.BuildPreview(new UpgradeChoice(upgrade, 0), stats, loadout);

            Assert.That(frame.Text, Does.Not.Contain(upgrade.shortDescription), path);
        }
    }

    [Test]
    public void PresenterRequiresSelectionBeforeSubmitAndSupportsNavigation()
    {
        GameConfig config = Track(TheCircussyOneTestObjects.CreateConfig());
        UpgradeCatalog catalog = Track(TheCircussyOneTestObjects.CreateUpgradeCatalog());
        TalentCatalog talentCatalog = Track(CreateThreeChoiceTalentCatalog());
        var talentState = new TalentRunState();
        var stats = new RunStats(config);
        var gameState = new GameState(config, stats);
        var upgradeState = new UpgradeRunState();
        var pauseState = new RunPauseState();
        var input = new FakeInputService();
        var view = CreateConfiguredUpgradeView(out _, out _, out _, out _, out _, out _, out _, out _);
        var system = CreateUpgradeSelectionSystem(
            gameState,
            catalog,
            upgradeState,
            stats,
            new WeaponLoadout(TheCircussyOneTestObjects.CreateWeaponCatalog()),
            pauseState,
            talentCatalog,
            talentState);
        var presenter = new UpgradeSelectionPresenter(system, view, input, stats, new WeaponLoadout(TheCircussyOneTestObjects.CreateWeaponCatalog()));
        system.Start();
        presenter.Start();

        gameState.AddExperience(gameState.ExperienceTarget);
        input.SubmitPressedThisFrame = true;
        presenter.Tick();

        Assert.That(system.HasActiveSelection, Is.True);
        Assert.That(view.HighlightedIndex, Is.EqualTo(-1));

        input.SubmitPressedThisFrame = false;
        input.UpgradeNavigation = UpgradeNavigationDirection.Middle;
        presenter.Tick();

        Assert.That(view.HighlightedIndex, Is.EqualTo(1));

        input.UpgradeNavigation = UpgradeNavigationDirection.None;
        presenter.Tick();
        input.UpgradeNavigation = UpgradeNavigationDirection.Right;
        presenter.Tick();

        Assert.That(view.HighlightedIndex, Is.EqualTo(2));

        input.UpgradeNavigation = UpgradeNavigationDirection.None;
        presenter.Tick();
        input.UpgradeNavigation = UpgradeNavigationDirection.Middle;
        presenter.Tick();

        Assert.That(view.HighlightedIndex, Is.EqualTo(2));

        input.UpgradeNavigation = UpgradeNavigationDirection.None;
        input.SubmitPressedThisFrame = true;
        presenter.Tick();

        Assert.That(system.HasActiveSelection, Is.False);
        Assert.That(pauseState.IsPaused, Is.False);
        presenter.Dispose();
        system.Dispose();
    }

    [Test]
    public void PresenterIgnoresHeldNavigationUntilNeutralAfterPopupOpens()
    {
        GameConfig config = Track(TheCircussyOneTestObjects.CreateConfig());
        UpgradeCatalog catalog = Track(TheCircussyOneTestObjects.CreateUpgradeCatalog());
        TalentCatalog talentCatalog = Track(CreateThreeChoiceTalentCatalog());
        var talentState = new TalentRunState();
        var stats = new RunStats(config);
        var gameState = new GameState(config, stats);
        var upgradeState = new UpgradeRunState();
        var pauseState = new RunPauseState();
        var input = new FakeInputService { UpgradeNavigation = UpgradeNavigationDirection.Right };
        var view = CreateConfiguredUpgradeView(out _, out _, out _, out _, out _, out _, out _, out _);
        var system = CreateUpgradeSelectionSystem(
            gameState,
            catalog,
            upgradeState,
            stats,
            new WeaponLoadout(TheCircussyOneTestObjects.CreateWeaponCatalog()),
            pauseState,
            talentCatalog,
            talentState);
        var presenter = new UpgradeSelectionPresenter(system, view, input, stats, new WeaponLoadout(TheCircussyOneTestObjects.CreateWeaponCatalog()));
        system.Start();
        presenter.Start();

        gameState.AddExperience(gameState.ExperienceTarget);
        presenter.Tick();

        Assert.That(system.HasActiveSelection, Is.True);
        Assert.That(view.HighlightedIndex, Is.EqualTo(-1));

        input.UpgradeNavigation = UpgradeNavigationDirection.None;
        presenter.Tick();

        Assert.That(view.HighlightedIndex, Is.EqualTo(-1));

        input.UpgradeNavigation = UpgradeNavigationDirection.Right;
        presenter.Tick();

        Assert.That(view.HighlightedIndex, Is.EqualTo(2));
        presenter.Dispose();
        system.Dispose();
    }

    [Test]
    public void PresenterCyclesChoicesWithControllerTriggers()
    {
        GameConfig config = Track(TheCircussyOneTestObjects.CreateConfig());
        UpgradeCatalog catalog = Track(TheCircussyOneTestObjects.CreateUpgradeCatalog());
        TalentCatalog talentCatalog = Track(CreateThreeChoiceTalentCatalog());
        var talentState = new TalentRunState();
        var stats = new RunStats(config);
        var gameState = new GameState(config, stats);
        var upgradeState = new UpgradeRunState();
        var pauseState = new RunPauseState();
        var input = new FakeInputService();
        var view = CreateConfiguredUpgradeView(out _, out _, out _, out _, out _, out _, out _, out _);
        var system = CreateUpgradeSelectionSystem(
            gameState,
            catalog,
            upgradeState,
            stats,
            new WeaponLoadout(TheCircussyOneTestObjects.CreateWeaponCatalog()),
            pauseState,
            talentCatalog,
            talentState);
        var presenter = new UpgradeSelectionPresenter(system, view, input, stats, new WeaponLoadout(TheCircussyOneTestObjects.CreateWeaponCatalog()));
        system.Start();
        presenter.Start();

        gameState.AddExperience(gameState.ExperienceTarget);
        presenter.Tick();

        Assert.That(system.HasActiveSelection, Is.True);
        Assert.That(view.HighlightedIndex, Is.EqualTo(-1));

        input.UiCycleNavigation = UiCycleNavigationDirection.Next;
        presenter.Tick();

        Assert.That(view.HighlightedIndex, Is.EqualTo(0));

        input.UiCycleNavigation = UiCycleNavigationDirection.None;
        presenter.Tick();
        input.UiCycleNavigation = UiCycleNavigationDirection.Next;
        presenter.Tick();

        Assert.That(view.HighlightedIndex, Is.EqualTo(1));

        input.UiCycleNavigation = UiCycleNavigationDirection.None;
        presenter.Tick();
        input.UiCycleNavigation = UiCycleNavigationDirection.Previous;
        presenter.Tick();

        Assert.That(view.HighlightedIndex, Is.EqualTo(0));

        input.UiCycleNavigation = UiCycleNavigationDirection.None;
        presenter.Tick();
        input.UiCycleNavigation = UiCycleNavigationDirection.Previous;
        presenter.Tick();

        Assert.That(view.HighlightedIndex, Is.EqualTo(2));
        presenter.Dispose();
        system.Dispose();
    }

    [Test]
    public void PresenterPlaysControllerNavigationAndSubmitAudio()
    {
        GameConfig config = Track(TheCircussyOneTestObjects.CreateConfig());
        UpgradeCatalog catalog = Track(TheCircussyOneTestObjects.CreateUpgradeCatalog());
        TalentCatalog talentCatalog = Track(CreateThreeChoiceTalentCatalog());
        var talentState = new TalentRunState();
        var stats = new RunStats(config);
        var gameState = new GameState(config, stats);
        var upgradeState = new UpgradeRunState();
        var pauseState = new RunPauseState();
        var input = new FakeInputService();
        var audio = new FakeGameAudio();
        var view = CreateConfiguredUpgradeView(out _, out _, out _, out _, out _, out _, out _, out _);
        var system = CreateUpgradeSelectionSystem(
            gameState,
            catalog,
            upgradeState,
            stats,
            new WeaponLoadout(TheCircussyOneTestObjects.CreateWeaponCatalog()),
            pauseState,
            talentCatalog,
            talentState);
        var presenter = new UpgradeSelectionPresenter(
            system,
            view,
            input,
            stats,
            new WeaponLoadout(TheCircussyOneTestObjects.CreateWeaponCatalog()),
            audio: audio);
        system.Start();
        presenter.Start();

        gameState.AddExperience(gameState.ExperienceTarget);
        presenter.Tick();
        input.UiCycleNavigation = UiCycleNavigationDirection.Next;
        presenter.Tick();
        input.UiCycleNavigation = UiCycleNavigationDirection.None;
        presenter.Tick();
        input.SubmitPressedThisFrame = true;
        presenter.Tick();

        Assert.That(audio.PlayedCues, Is.EqualTo(new[] { GameAudioCue.UiHover, GameAudioCue.UiClick }));
        Assert.That(system.HasActiveSelection, Is.False);
        presenter.Dispose();
        system.Dispose();
    }

    [Test]
    public void PresenterIgnoresHeldTriggerCycleUntilNeutralAfterPopupOpens()
    {
        GameConfig config = Track(TheCircussyOneTestObjects.CreateConfig());
        UpgradeCatalog catalog = Track(TheCircussyOneTestObjects.CreateUpgradeCatalog());
        TalentCatalog talentCatalog = Track(CreateThreeChoiceTalentCatalog());
        var talentState = new TalentRunState();
        var stats = new RunStats(config);
        var gameState = new GameState(config, stats);
        var upgradeState = new UpgradeRunState();
        var pauseState = new RunPauseState();
        var input = new FakeInputService { UiCycleNavigation = UiCycleNavigationDirection.Next };
        var view = CreateConfiguredUpgradeView(out _, out _, out _, out _, out _, out _, out _, out _);
        var system = CreateUpgradeSelectionSystem(
            gameState,
            catalog,
            upgradeState,
            stats,
            new WeaponLoadout(TheCircussyOneTestObjects.CreateWeaponCatalog()),
            pauseState,
            talentCatalog,
            talentState);
        var presenter = new UpgradeSelectionPresenter(system, view, input, stats, new WeaponLoadout(TheCircussyOneTestObjects.CreateWeaponCatalog()));
        system.Start();
        presenter.Start();

        gameState.AddExperience(gameState.ExperienceTarget);
        presenter.Tick();

        Assert.That(system.HasActiveSelection, Is.True);
        Assert.That(view.HighlightedIndex, Is.EqualTo(-1));

        input.UiCycleNavigation = UiCycleNavigationDirection.None;
        presenter.Tick();

        Assert.That(view.HighlightedIndex, Is.EqualTo(-1));

        input.UiCycleNavigation = UiCycleNavigationDirection.Next;
        presenter.Tick();

        Assert.That(view.HighlightedIndex, Is.EqualTo(0));
        presenter.Dispose();
        system.Dispose();
    }

    [Test]
    public void NumberKeysStillSelectDirectChoices()
    {
        GameConfig config = Track(TheCircussyOneTestObjects.CreateConfig());
        UpgradeCatalog catalog = Track(TheCircussyOneTestObjects.CreateUpgradeCatalog());
        TalentCatalog talentCatalog = Track(CreateThreeChoiceTalentCatalog());
        var talentState = new TalentRunState();
        var stats = new RunStats(config);
        var gameState = new GameState(config, stats);
        var upgradeState = new UpgradeRunState();
        var pauseState = new RunPauseState();
        var input = new FakeInputService();
        var view = CreateConfiguredUpgradeView(out _, out _, out _, out _, out _, out _, out _, out _);
        var system = CreateUpgradeSelectionSystem(
            gameState,
            catalog,
            upgradeState,
            stats,
            new WeaponLoadout(TheCircussyOneTestObjects.CreateWeaponCatalog()),
            pauseState,
            talentCatalog,
            talentState);
        var presenter = new UpgradeSelectionPresenter(system, view, input, stats, new WeaponLoadout(TheCircussyOneTestObjects.CreateWeaponCatalog()));
        system.Start();
        presenter.Start();

        gameState.AddExperience(gameState.ExperienceTarget);
        input.UpgradeChoicePressedThisFrame = 2;
        presenter.Tick();

        Assert.That(system.HasActiveSelection, Is.False);
        presenter.Dispose();
        system.Dispose();
    }

    [Test]
    public void UpgradeSelectionViewBindsThreeCardsAndInvokesSelectedIndex()
    {
        TalentDefinition first = Track(TheCircussyOneTestObjects.CreateTalentDefinition("first", "First Upgrade", ContentRarity.Common));
        TalentDefinition second = Track(TheCircussyOneTestObjects.CreateTalentDefinition("second", "Second Upgrade", ContentRarity.Uncommon));
        TalentDefinition third = Track(TheCircussyOneTestObjects.CreateTalentDefinition("third", "Third Upgrade", ContentRarity.Rare));
        var view = CreateConfiguredUpgradeView(
            out VisualElement overlay,
            out VisualElement[] cards,
            out Label[] rarity,
            out Label[] title,
            out _,
            out Label[] level,
            out _,
            out Label[] previews);
        int selected = -1;
        view.ChoiceSelected += index => selected = index;
        var choices = new[]
        {
            new UpgradeChoice(first, 0),
            new UpgradeChoice(second, 1, ContentRarity.Uncommon),
            new UpgradeChoice(third, 2, ContentRarity.Rare)
        };
        var previewFrames = new[]
        {
            new UpgradePreviewFrame("First 0 -> <color=#73F59A>1</color>", true),
            new UpgradePreviewFrame("Second 1 -> <color=#73F59A>2</color>", true),
            new UpgradePreviewFrame("Third 2 -> <color=#73F59A>3</color>", true)
        };

        view.Show(choices, previewFrames);
        Assert.That(view.HighlightedIndex, Is.EqualTo(-1));
        Assert.That(view.HasHighlightedSelection, Is.False);
        view.SelectHighlighted();
        Assert.That(selected, Is.EqualTo(-1));
        view.Highlight(1);
        view.SelectHighlighted();

        Assert.That(overlay.style.display.value, Is.EqualTo(DisplayStyle.Flex));
        Assert.That(title[0].text, Is.EqualTo("First Upgrade"));
        Assert.That(rarity[1].text, Is.EqualTo("UNCOMMON"));
        Assert.That(level[2].text, Is.EqualTo("LV 2 -> 3"));
        Assert.That(previews[0].text, Does.Contain("First 0 ->"));
        Assert.That(cards[1].ClassListContains("upgrade-card-selected"), Is.True);
        Assert.That(selected, Is.EqualTo(1));
    }

    [Test]
    public void UpgradeSelectionViewMakesCardsPointerPickable()
    {
        _ = CreateConfiguredUpgradeView(
            out VisualElement overlay,
            out VisualElement[] cards,
            out _,
            out _,
            out _,
            out _,
            out _,
            out _);

        Assert.That(overlay.pickingMode, Is.EqualTo(PickingMode.Position));
        foreach (VisualElement card in cards)
        {
            Assert.That(card.pickingMode, Is.EqualTo(PickingMode.Position));
        }
    }

    [Test]
    public void UpgradeSelectionViewAppliesChoiceIconWithSelectedPerformerFallback()
    {
        Sprite portrait = CreateSprite(Color.cyan);
        var performer = Track(ScriptableObject.CreateInstance<PerformerDefinition>());
        performer.performerId = "sola_hoop_dancer";
        performer.displayName = "Sola";
        performer.portraitSprite = portrait;
        var talent = Track(ScriptableObject.CreateInstance<TalentDefinition>());
        talent.talentId = "hoop_flow";
        talent.displayName = "Hoop Flow";
        talent.poolKind = TalentPoolKind.PerformerSpecific;
        talent.performerId = performer.Id;
        var gameObject = new GameObject("TCO Test Upgrade Selection Icon View");
        _created.Add(gameObject);
        var view = gameObject.AddComponent<UpgradeSelectionView>();
        VisualElement overlay = new();
        VisualElement[] cards = CreateVisualElements(3);
        Label[] rarity = CreateLabels(3);
        Label[] title = CreateLabels(3);
        Label[] description = CreateLabels(3);
        Label[] level = CreateLabels(3);
        VisualElement[] strips = CreateVisualElements(3);
        Label[] previews = CreateLabels(3);
        VisualElement[] icons = CreateVisualElements(3);
        view.ConfigureForTests(overlay, cards, rarity, title, description, level, strips, previews, icons);

        view.Show(new[] { new UpgradeChoice(talent, 0, ContentRarity.Rare) }, selectedPerformer: performer);

        Assert.That(icons[0].style.backgroundImage.keyword, Is.EqualTo(StyleKeyword.Null));
        Assert.That(icons[0].style.borderTopColor.value, Is.EqualTo(ContentRarityMetadata.Get(ContentRarity.Rare).Color));
        Image image = icons[0].Q<Image>(ContentIconVisuals.ImageElementName);
        Assert.That(image, Is.Not.Null);
        Assert.That(image.sprite, Is.SameAs(portrait));
        Assert.That(image.scaleMode, Is.EqualTo(ScaleMode.ScaleToFit));

        DestroySprite(portrait);
    }

    [Test]
    public void UpgradeSelectionViewIgnoresInitialPointerEnterUntilPointerMoves()
    {
        UpgradeDefinition first = Track(TheCircussyOneTestObjects.CreateUpgradeDefinition("first", "First Upgrade", ContentRarity.Common));
        UpgradeDefinition second = Track(TheCircussyOneTestObjects.CreateUpgradeDefinition("second", "Second Upgrade", ContentRarity.Uncommon));
        UpgradeDefinition third = Track(TheCircussyOneTestObjects.CreateUpgradeDefinition("third", "Third Upgrade", ContentRarity.Rare));
        var view = CreateConfiguredUpgradeView(
            out _,
            out VisualElement[] cards,
            out _,
            out _,
            out _,
            out _,
            out _,
            out _);
        var choices = new[]
        {
            new UpgradeChoice(first, 0),
            new UpgradeChoice(second, 1),
            new UpgradeChoice(third, 2)
        };

        view.Show(choices);
        InvokePrivatePointerHandler(view, "HighlightFromPointerEnter", 1);

        Assert.That(view.HighlightedIndex, Is.EqualTo(-1));

        InvokePrivatePointerHandler(view, "HighlightFromPointerMove", 1);

        Assert.That(view.HighlightedIndex, Is.EqualTo(1));
    }

    [Test]
    public void UpgradeSelectionViewKeepsOnlyExplicitHighlightSelected()
    {
        UpgradeDefinition first = Track(TheCircussyOneTestObjects.CreateUpgradeDefinition("first", "First Upgrade", ContentRarity.Common));
        UpgradeDefinition second = Track(TheCircussyOneTestObjects.CreateUpgradeDefinition("second", "Second Upgrade", ContentRarity.Uncommon));
        UpgradeDefinition third = Track(TheCircussyOneTestObjects.CreateUpgradeDefinition("third", "Third Upgrade", ContentRarity.Rare));
        var view = CreateConfiguredUpgradeView(
            out _,
            out VisualElement[] cards,
            out _,
            out _,
            out _,
            out _,
            out _,
            out _);
        var choices = new[]
        {
            new UpgradeChoice(first, 0),
            new UpgradeChoice(second, 1),
            new UpgradeChoice(third, 2)
        };

        view.Show(choices);
        InvokePrivatePointerHandler(view, "HighlightFromPointerMove", 0);
        view.Highlight(2);

        Assert.That(cards[0].ClassListContains("upgrade-card-selected"), Is.False);
        Assert.That(cards[0].ClassListContains("is-selected"), Is.False);
        Assert.That(cards[2].ClassListContains("upgrade-card-selected"), Is.True);
        Assert.That(cards[2].ClassListContains("is-selected"), Is.True);
    }

    [Test]
    public void UpgradeSelectionStylesDoNotLetPseudoHoverCompeteWithExplicitSelection()
    {
        string styles = File.ReadAllText("Assets/Game/UI/Hud.uss");

        Assert.That(styles, Does.Not.Contain(".upgrade-card:hover"));
        Assert.That(styles, Does.Not.Contain(".upgrade-card:focus"));
    }

    private UpgradeCatalog CreateThreeChoiceCatalog(UpgradeDefinition first = null)
    {
        first ??= Track(TheCircussyOneTestObjects.CreateUpgradeDefinition("first", "First"));
        UpgradeDefinition second = Track(TheCircussyOneTestObjects.CreateUpgradeDefinition("second", "Second"));
        UpgradeDefinition third = Track(TheCircussyOneTestObjects.CreateUpgradeDefinition("third", "Third"));
        return TheCircussyOneTestObjects.CreateUpgradeCatalog(first, second, third);
    }

    private TalentCatalog CreateThreeChoiceTalentCatalog(TalentDefinition first = null)
    {
        first ??= Track(TheCircussyOneTestObjects.CreateTalentDefinition("first", "First"));
        TalentDefinition second = Track(TheCircussyOneTestObjects.CreateTalentDefinition("second", "Second"));
        TalentDefinition third = Track(TheCircussyOneTestObjects.CreateTalentDefinition("third", "Third"));
        return TheCircussyOneTestObjects.CreateTalentCatalog(first, second, third);
    }

    private T Track<T>(T target)
        where T : Object
    {
        _created.Add(target);
        return target;
    }

    private static int IndexOf(IReadOnlyList<UpgradeChoice> choices, string id)
    {
        for (int i = 0; i < choices.Count; i++)
        {
            if (choices[i].Id == id)
            {
                return i;
            }
        }

        return -1;
    }

    private static UpgradeSelectionSystem CreateUpgradeSelectionSystem(
        GameState gameState,
        UpgradeCatalog catalog,
        UpgradeRunState upgradeState,
        RunStats stats,
        WeaponLoadout weaponLoadout,
        RunPauseState pauseState,
        TalentCatalog talentCatalog = null,
        TalentRunState talentState = null)
    {
        talentState ??= new TalentRunState();
        var provider = new UpgradeChoiceProvider(catalog, upgradeState, stats, weaponLoadout, talentCatalog: talentCatalog, talentRunState: talentState);
        var talentApplier = new TalentEffectApplier(talentState, stats, gameState);
        var applier = new UpgradeEffectApplier(upgradeState, stats, weaponLoadout, gameState, talentEffectApplier: talentApplier);
        return new UpgradeSelectionSystem(gameState, upgradeState, pauseState, provider, applier);
    }

    private static WeaponDefinition CreateTestWeapon(string id, string displayName)
    {
        var config = TheCircussyOneTestObjects.CreateConfig();
        WeaponDefinition weapon = TheCircussyOneTestObjects.CreateWeaponDefinition(config, id, displayName);
        TheCircussyOneTestObjects.Destroy(config);
        return weapon;
    }

    private static UpgradeDefinition CreateWeaponStatUpgrade(
        WeaponDefinition weapon,
        string suffix,
        string displaySuffix,
        params UpgradeStatModifierDefinition[] modifiers)
    {
        var upgrade = ScriptableObject.CreateInstance<UpgradeDefinition>();
        upgrade.ApplyWeaponStatUpgradeDefaults(
            weapon,
            suffix,
            displaySuffix,
            "+1 test weapon upgrade.",
            Color.white,
            modifiers);
        return upgrade;
    }

    private static WeaponCatalog CreateWeaponCatalog(WeaponDefinition startingWeapon, params WeaponDefinition[] availableOnlyWeapons)
    {
        var catalog = ScriptableObject.CreateInstance<WeaponCatalog>();
        if (startingWeapon != null)
        {
            catalog.startingWeapons.Add(startingWeapon);
            catalog.availableWeapons.Add(startingWeapon);
        }

        if (availableOnlyWeapons != null)
        {
            for (int i = 0; i < availableOnlyWeapons.Length; i++)
            {
                WeaponDefinition weapon = availableOnlyWeapons[i];
                if (weapon != null && !catalog.availableWeapons.Contains(weapon))
                {
                    catalog.availableWeapons.Add(weapon);
                }
            }
        }

        return catalog;
    }

    private UpgradeSelectionView CreateConfiguredUpgradeView(
        out VisualElement overlay,
        out VisualElement[] cards,
        out Label[] rarity,
        out Label[] title,
        out Label[] description,
        out Label[] level,
        out VisualElement[] strips,
        out Label[] previews)
    {
        var gameObject = new GameObject("TCO Test Upgrade Selection View");
        _created.Add(gameObject);
        var view = gameObject.AddComponent<UpgradeSelectionView>();
        overlay = new VisualElement();
        cards = new[] { new VisualElement(), new VisualElement(), new VisualElement() };
        rarity = new[] { new Label(), new Label(), new Label() };
        title = new[] { new Label(), new Label(), new Label() };
        description = new[] { new Label(), new Label(), new Label() };
        level = new[] { new Label(), new Label(), new Label() };
        strips = new[] { new VisualElement(), new VisualElement(), new VisualElement() };
        previews = new[] { new Label(), new Label(), new Label() };
        view.ConfigureForTests(overlay, cards, rarity, title, description, level, strips, previews);
        return view;
    }

    private static void InvokePrivatePointerHandler(UpgradeSelectionView view, string methodName, int index)
    {
        MethodInfo method = typeof(UpgradeSelectionView).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(method, Is.Not.Null);
        method.Invoke(view, new object[] { index });
    }

    private static VisualElement[] CreateVisualElements(int count)
    {
        return Enumerable.Range(0, count).Select(_ => new VisualElement()).ToArray();
    }

    private static Label[] CreateLabels(int count)
    {
        return Enumerable.Range(0, count).Select(_ => new Label()).ToArray();
    }

    private static Sprite CreateSprite(Color color)
    {
        var texture = new Texture2D(4, 4, TextureFormat.RGBA32, false);
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply(false, false);
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), texture.width);
    }

    private static void DestroySprite(Sprite sprite)
    {
        if (sprite == null)
        {
            return;
        }

        Texture2D texture = sprite.texture;
        Object.DestroyImmediate(sprite);
        Object.DestroyImmediate(texture);
    }
}
