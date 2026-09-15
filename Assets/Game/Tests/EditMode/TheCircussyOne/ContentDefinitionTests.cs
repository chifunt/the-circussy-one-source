using System.Collections.Generic;
using System.Linq;
using System;
using System.Reflection;
using NUnit.Framework;
using Sirenix.OdinInspector;
using TheCircussyOne.Content;
using TheCircussyOne.Stats;
using UnityEditor;
using UnityEngine;

public sealed class ContentDefinitionTests
{
    [TestCase("juggling_ball")]
    [TestCase("blue_xp_gem_01")]
    [TestCase("a1")]
    public void ContentIdAcceptsLowercaseSnakeCaseIds(string value)
    {
        Assert.That(ContentId.IsValidValue(value), Is.True);
    }

    [TestCase("")]
    [TestCase("JugglingBall")]
    [TestCase("starter-projectile")]
    [TestCase("starter__projectile")]
    [TestCase("juggling_ball_")]
    [TestCase("1juggling_ball")]
    public void ContentIdRejectsInvalidIds(string value)
    {
        Assert.That(ContentId.IsValidValue(value), Is.False);
    }

    [Test]
    public void ContentIdNormalizesDesignerTextToSnakeCase()
    {
        var id = new ContentId(" Juggling Ball-01 ");

        Assert.That(id.Value, Is.EqualTo("juggling_ball_01"));
        Assert.That(id.IsValid, Is.True);
    }

    [Test]
    public void ContentIdSuggestionRulesGenerateUniqueStableIds()
    {
        var existing = new HashSet<string> { "juggling_ball", "juggling_ball_002" };

        Assert.That(ContentIdSuggestionRules.MakeUniqueId("Juggling Ball", existing), Is.EqualTo("juggling_ball_003"));
        Assert.That(ContentIdSuggestionRules.MakeUniqueId("", existing, "Passive Item"), Is.EqualTo("passive_item"));
        Assert.That(ContentIdSuggestionRules.BuildId("Juggling Ball", "Weapon Damage"), Is.EqualTo("juggling_ball_weapon_damage"));
    }

    [Test]
    public void ContentIdSuggestionRulesGenerateCopyNamesAndIds()
    {
        var existingIds = new HashSet<string> { "rubber_soles_copy", "rubber_soles_copy_002" };
        var existingNames = new HashSet<string> { "Rubber Soles Copy" };

        Assert.That(ContentIdSuggestionRules.MakeCopyId("rubber_soles", existingIds), Is.EqualTo("rubber_soles_copy_003"));
        Assert.That(ContentIdSuggestionRules.MakeCopyDisplayName("Rubber Soles", existingNames), Is.EqualTo("Rubber Soles Copy 002"));
    }

    [Test]
    public void ContentStatValidationAllowsOnlyCanonicalAuthoredBuckets()
    {
        Assert.That(ContentStatValidationRules.IsValid(TheCircussyOne.Stats.StatId.PlayerArmor, TheCircussyOne.Stats.StatModifierBucket.Flat), Is.True);
        Assert.That(ContentStatValidationRules.IsValid(TheCircussyOne.Stats.StatId.WeaponFlatDamage, TheCircussyOne.Stats.StatModifierBucket.Flat), Is.True);
        Assert.That(ContentStatValidationRules.IsValid(TheCircussyOne.Stats.StatId.GlobalWeaponHaste, TheCircussyOne.Stats.StatModifierBucket.AdditivePercent), Is.True);
        Assert.That(ContentStatValidationRules.IsValid(TheCircussyOne.Stats.StatId.PlayerArmor, TheCircussyOne.Stats.StatModifierBucket.AdditivePercent), Is.False);
        Assert.That(ContentStatValidationRules.IsValid(TheCircussyOne.Stats.StatId.GlobalWeaponHaste, TheCircussyOne.Stats.StatModifierBucket.Flat), Is.False);
        Assert.That(ContentStatValidationRules.IsValid(TheCircussyOne.Stats.StatId.GlobalWeaponHaste, TheCircussyOne.Stats.StatModifierBucket.Haste), Is.False);
        Assert.That(ContentStatValidationRules.IsValid(TheCircussyOne.Stats.StatId.GlobalWeaponHaste, TheCircussyOne.Stats.StatModifierBucket.FlatSeconds), Is.False);
        Assert.That(ContentStatValidationRules.IsValid(TheCircussyOne.Stats.StatId.GlobalDamageMultiplier, TheCircussyOne.Stats.StatModifierBucket.GlobalPercent), Is.False);
        Assert.That(ContentStatValidationRules.IsValid(TheCircussyOne.Stats.StatId.WeaponDamageMultiplier, TheCircussyOne.Stats.StatModifierBucket.AdditivePercent), Is.False);
        Assert.That(ContentStatValidationRules.IsValid((TheCircussyOne.Stats.StatId)999, TheCircussyOne.Stats.StatModifierBucket.Flat), Is.False);
        Assert.That(ContentStatValidationRules.DescribeInvalidModifier(StatId.GlobalWeaponHaste, StatModifierBucket.Haste), Does.Contain("unsupported legacy bucket"));
        Assert.That(ContentStatValidationRules.DescribeInvalidModifier(StatId.GlobalDamageMultiplier, StatModifierBucket.GlobalPercent), Does.Contain("unsupported internal bucket"));
        Assert.That(ContentStatValidationRules.DescribeInvalidModifier(StatId.WeaponDamageMultiplier, StatModifierBucket.AdditivePercent), Does.Contain("internal compatibility stat"));
        Assert.That(ContentStatValidationRules.DescribeInvalidModifier(StatId.PlayerArmor, StatModifierBucket.AdditivePercent), Does.Contain("requires 'Flat'"));
    }

    [Test]
    public void FirstPartyContentAssetsUseCanonicalAuthoredStatBuckets()
    {
        var failures = new List<string>();

        foreach (UpgradeDefinition upgrade in LoadAssets<UpgradeDefinition>())
        {
            AppendInvalidUpgradeModifiers(failures, AssetDatabase.GetAssetPath(upgrade), upgrade.statModifiers);
            if (upgrade.statModifiers == null)
            {
                continue;
            }

            for (int i = 0; i < upgrade.statModifiers.Count; i++)
            {
                UpgradeStatModifierDefinition modifier = upgrade.statModifiers[i];
                if (ContentStatValidationRules.IsValid(modifier.statId, modifier.bucket)
                    && !ContentStatValidationRules.IsWeaponUpgradeStat(modifier.statId))
                {
                    failures.Add($"{AssetDatabase.GetAssetPath(upgrade)} modifier {i} uses non-weapon stat {modifier.statId}.");
                }
            }
        }

        foreach (TalentDefinition talent in LoadAssets<TalentDefinition>())
        {
            AppendInvalidUpgradeModifiers(failures, AssetDatabase.GetAssetPath(talent), talent.statModifiers);
            AppendUnexpectedWeaponLocalStats(failures, AssetDatabase.GetAssetPath(talent), talent.statModifiers);
        }

        foreach (PerformerDefinition performer in LoadAssets<PerformerDefinition>())
        {
            AppendInvalidUpgradeModifiers(failures, $"{AssetDatabase.GetAssetPath(performer)} base", performer.baseStatModifiers);
            AppendInvalidUpgradeModifiers(failures, $"{AssetDatabase.GetAssetPath(performer)} passive", performer.passiveStatModifiers);
            AppendUnexpectedWeaponLocalStats(failures, $"{AssetDatabase.GetAssetPath(performer)} base", performer.baseStatModifiers);
            AppendUnexpectedWeaponLocalStats(failures, $"{AssetDatabase.GetAssetPath(performer)} passive", performer.passiveStatModifiers);
        }

        foreach (ItemDefinition item in LoadAssets<ItemDefinition>())
        {
            AppendInvalidItemModifiers(failures, AssetDatabase.GetAssetPath(item), item.statModifiers);
            AppendUnexpectedWeaponLocalItemStats(failures, AssetDatabase.GetAssetPath(item), item.statModifiers);
        }

        Assert.That(failures, Is.Empty, string.Join("\n", failures));
    }

    [Test]
    public void FirstPartyWeaponUpgradeAssetsMatchTuningTableAndSupportedStats()
    {
        var failures = new List<string>();
        var upgradeStatsByWeapon = new Dictionary<string, HashSet<StatId>>(StringComparer.Ordinal);

        foreach (UpgradeDefinition upgrade in LoadAssets<UpgradeDefinition>())
        {
            string path = AssetDatabase.GetAssetPath(upgrade);
            if (upgrade == null || !path.Contains("/Balance/Upgrades/"))
            {
                continue;
            }

            if (upgrade.statModifiers == null || upgrade.statModifiers.Count != 1)
            {
                failures.Add($"{path} should have exactly one weapon stat modifier.");
                continue;
            }

            UpgradeStatModifierDefinition modifier = upgrade.statModifiers[0];
            if (!ContentStatValidationRules.IsWeaponUpgradeStat(modifier.statId))
            {
                failures.Add($"{path} uses non-weapon stat {modifier.statId}.");
                continue;
            }

            if (!TryExpectedFirstPartyWeaponUpgradeValue(modifier.statId, out float expected))
            {
                failures.Add($"{path} uses stat {modifier.statId} without a first-party tuning expectation.");
                continue;
            }

            if (!Mathf.Approximately(modifier.value, expected))
            {
                failures.Add($"{path} {modifier.statId} Common value should be {expected:0.###}, found {modifier.value:0.###}.");
            }

            if (StatMetadata.Get(modifier.statId).ValueKind == StatValueKind.Integer
                && modifier.value > 0f
                && StatRules.FloorToAppliedInt(modifier.ValueFor(ContentRarity.Common)) <= 0)
            {
                failures.Add($"{path} {modifier.statId} floors to +0 at Common rarity.");
            }

            string weaponId = !string.IsNullOrWhiteSpace(upgrade.weaponId)
                ? upgrade.weaponId
                : upgrade.weaponDefinition != null ? upgrade.weaponDefinition.Id : string.Empty;
            if (!string.IsNullOrWhiteSpace(weaponId))
            {
                if (!upgradeStatsByWeapon.TryGetValue(weaponId, out HashSet<StatId> stats))
                {
                    stats = new HashSet<StatId>();
                    upgradeStatsByWeapon.Add(weaponId, stats);
                }

                stats.Add(modifier.statId);
            }
        }

        foreach (WeaponDefinition weapon in LoadAssets<WeaponDefinition>())
        {
            string path = AssetDatabase.GetAssetPath(weapon);
            if (weapon == null || !path.Contains("/Balance/Weapons/"))
            {
                continue;
            }

            if (!upgradeStatsByWeapon.TryGetValue(weapon.Id, out HashSet<StatId> upgradeStats))
            {
                failures.Add($"{path} has no first-party weapon upgrade cards.");
                continue;
            }

            for (int i = 0; i < weapon.SupportedUpgradeStats.Count; i++)
            {
                StatId stat = weapon.SupportedUpgradeStats[i];
                if (!upgradeStats.Contains(stat))
                {
                    failures.Add($"{path} supports {stat} but no first-party upgrade card uses it.");
                }
            }
        }

        Assert.That(failures, Is.Empty, string.Join("\n", failures));
    }

    [Test]
    public void ContentTagSetRemovesDuplicatesAndSupportsQueries()
    {
        var tags = new ContentTagSet();
        tags.values.Add(ContentTag.Projectile);
        tags.values.Add(ContentTag.Projectile);
        tags.values.Add(ContentTag.Physical);

        bool changed = tags.RemoveDuplicates();

        Assert.That(changed, Is.True);
        Assert.That(tags.Values, Is.EqualTo(new[] { ContentTag.Projectile, ContentTag.Physical }));
        Assert.That(tags.HasTag(ContentTag.Projectile), Is.True);
        Assert.That(tags.HasAny(ContentTag.Fire, ContentTag.Physical), Is.True);
        Assert.That(tags.HasAny(ContentTag.Fire, ContentTag.Ice), Is.False);
    }

    [Test]
    public void ContentTagsCoverV03SpecCategories()
    {
        string[] required =
        {
            "Projectile", "Melee", "Aura", "Summon", "Fire", "Ice", "Lightning", "Poison", "Explosive", "Physical", "Magic",
            "Bounce", "Pierce", "Chain", "Heavy", "Rapid", "Orbit", "Trap",
            "Swarm", "Tank", "Ranged", "Elite", "Boss", "Flying", "Armored", "Fast", "Exploder", "Summoner", "Shielded", "Special",
            "Damage", "Defense", "Economy", "Utility", "Crit", "Status", "Movement", "Pickup", "Luck", "XP", "Chest", "Shrine", "Currency", "WeaponSynergy", "Risk"
        };

        string[] available = Enum.GetNames(typeof(ContentTag));

        Assert.That(required.Except(available), Is.Empty);
    }

    [Test]
    public void ComputedContentTagsDeriveFromStructuredWeaponFields()
    {
        var weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
        weapon.weaponId = "juggling_ball";
        weapon.displayName = "Juggling Ball";
        weapon.bounceEnabled = true;
        weapon.baseBounceCount = 1;
        weapon.maxBounceCount = 4;
        weapon.projectileDamage = 2;
        weapon.tags = ContentTagSet.With(ContentTag.Fire, ContentTag.Magic);

        ContentTagSet tags = weapon.Tags;

        Assert.That(tags.HasTag(ContentTag.Projectile), Is.True);
        Assert.That(tags.HasTag(ContentTag.Ranged), Is.True);
        Assert.That(tags.HasTag(ContentTag.Damage), Is.True);
        Assert.That(tags.HasTag(ContentTag.Bounce), Is.True);
        Assert.That(tags.HasTag(ContentTag.Fire), Is.False);
        Assert.That(tags.HasTag(ContentTag.Magic), Is.False);

        UnityEngine.Object.DestroyImmediate(weapon);
    }

    [Test]
    public void ComputedContentTagsDeriveFromModifierStats()
    {
        TalentDefinition talent = ScriptableObject.CreateInstance<TalentDefinition>();
        talent.talentId = "quick_hands";
        talent.displayName = "Quick Hands";
        talent.statModifiers.Add(new UpgradeStatModifierDefinition(TheCircussyOne.Stats.StatId.GlobalWeaponHaste, TheCircussyOne.Stats.StatModifierBucket.Flat, 1f));

        ItemDefinition item = ScriptableObject.CreateInstance<ItemDefinition>();
        item.itemId = "lucky_charm";
        item.displayName = "Lucky Charm";
        item.statModifiers.Add(new ItemStatModifierDefinition(TheCircussyOne.Stats.StatId.Luck, TheCircussyOne.Stats.StatModifierBucket.Flat, 3f));

        Assert.That(talent.Tags.HasTag(ContentTag.Rapid), Is.True);
        Assert.That(talent.Tags.HasTag(ContentTag.Utility), Is.True);
        Assert.That(item.Tags.HasTag(ContentTag.Luck), Is.True);
        Assert.That(item.Tags.HasTag(ContentTag.Economy), Is.True);

        UnityEngine.Object.DestroyImmediate(talent);
        UnityEngine.Object.DestroyImmediate(item);
    }

    [Test]
    public void ComputedContentTagsDeriveFromPerformerEnemyAndXpRoles()
    {
        WeaponDefinition weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
        weapon.weaponId = "fire_hoop";
        weapon.displayName = "Fire Hoop";
        weapon.orbitEnabled = true;

        PerformerDefinition performer = ScriptableObject.CreateInstance<PerformerDefinition>();
        performer.performerId = "sola_hoop_dancer";
        performer.displayName = "Sola Hoop Dancer";
        performer.startingWeapon = weapon;
        performer.baseStatModifiers.Add(new UpgradeStatModifierDefinition(TheCircussyOne.Stats.StatId.PlayerMoveSpeedMultiplier, TheCircussyOne.Stats.StatModifierBucket.Flat, 1f));

        EnemyDefinition enemy = ScriptableObject.CreateInstance<EnemyDefinition>();
        enemy.enemyId = "normal_enemy";
        enemy.displayName = "Normal Enemy";
        enemy.behaviorType = EnemyBehaviorType.Chaser;

        XpGemDefinition gem = ScriptableObject.CreateInstance<XpGemDefinition>();

        Assert.That(performer.Tags.HasTag(ContentTag.Orbit), Is.True);
        Assert.That(performer.Tags.HasTag(ContentTag.Movement), Is.True);
        Assert.That(enemy.Tags.HasTag(ContentTag.Swarm), Is.True);
        Assert.That(enemy.Tags.HasTag(ContentTag.Damage), Is.True);
        Assert.That(gem.Tags.HasTag(ContentTag.XP), Is.True);
        Assert.That(gem.Tags.HasTag(ContentTag.Pickup), Is.True);

        UnityEngine.Object.DestroyImmediate(weapon);
        UnityEngine.Object.DestroyImmediate(performer);
        UnityEngine.Object.DestroyImmediate(enemy);
        UnityEngine.Object.DestroyImmediate(gem);
    }

    [Test]
    public void ContentRarityMetadataReturnsReadableNamesColorsAndWeights()
    {
        foreach (ContentRarity rarity in ContentRarityMetadata.All)
        {
            ContentRarityInfo info = ContentRarityMetadata.Get(rarity);

            Assert.That(info.DisplayName, Is.Not.Empty);
            Assert.That(info.DefaultWeight, Is.Positive);
            Assert.That(info.Color.a, Is.EqualTo(1f));
        }

        Assert.That(ContentRarityMetadata.Get(ContentRarity.Legendary).DisplayName, Is.EqualTo("Legendary"));
        Assert.That(ContentRarityMetadata.Get(ContentRarity.Common).Color, Is.EqualTo(ContentRarityMetadata.CommonColor));
        Assert.That(ContentRarityMetadata.Get(ContentRarity.Uncommon).Color, Is.EqualTo(ContentRarityMetadata.UncommonColor));
        Assert.That(ContentRarityMetadata.Get(ContentRarity.Rare).Color, Is.EqualTo(ContentRarityMetadata.RareColor));
        Assert.That(ContentRarityMetadata.Get(ContentRarity.Epic).Color, Is.EqualTo(ContentRarityMetadata.EpicColor));
        Assert.That(ContentRarityMetadata.Get(ContentRarity.Legendary).Color, Is.EqualTo(ContentRarityMetadata.LegendaryColor));
        Assert.That(RelativeLuminance(ContentRarityMetadata.RareAuthoringColor), Is.GreaterThan(RelativeLuminance(ContentRarityMetadata.RareColor)));
        Assert.That(RelativeLuminance(ContentRarityMetadata.EpicAuthoringColor), Is.GreaterThan(RelativeLuminance(ContentRarityMetadata.EpicColor)));
        Assert.That(ContentRarityMetadata.AuthoringColor(ContentRarity.Rare), Is.EqualTo(ContentRarityMetadata.RareAuthoringColor));
        Assert.That(ContentRarityMetadata.AuthoringColor(ContentRarity.Epic), Is.EqualTo(ContentRarityMetadata.EpicAuthoringColor));
    }

    [Test]
    public void ContentRarityWeightTablePicksDeterministicallyFromNormalizedRoll()
    {
        var weights = new ContentRarityWeightTable
        {
            common = 1,
            uncommon = 1,
            rare = 1,
            epic = 1,
            legendary = 1
        };

        Assert.That(weights.IsValid, Is.True);
        Assert.That(weights.Pick(0f), Is.EqualTo(ContentRarity.Common));
        Assert.That(weights.Pick(0.25f), Is.EqualTo(ContentRarity.Uncommon));
        Assert.That(weights.Pick(0.45f), Is.EqualTo(ContentRarity.Rare));
        Assert.That(weights.Pick(0.65f), Is.EqualTo(ContentRarity.Epic));
        Assert.That(weights.Pick(0.85f), Is.EqualTo(ContentRarity.Legendary));
    }

    [Test]
    public void ContentRarityWeightPresetsMatchV03RewardContexts()
    {
        AssertWeights(ContentRarityWeightTable.LevelUpDefault(), 60, 25, 10, 4, 1);
        AssertWeights(ContentRarityWeightTable.NormalChestDefault(), 45, 30, 17, 6, 2);
        AssertWeights(ContentRarityWeightTable.SpecialEnemyChestDefault(), 30, 35, 23, 9, 3);
        AssertWeights(ContentRarityWeightTable.BossRewardDefault(), 0, 20, 40, 30, 10);
        AssertWeights(ContentRarityWeightTable.ShrineDefault(), 10, 25, 35, 22, 8);
        AssertWeights(ContentRarityWeightTable.ChestDefault(), 45, 30, 17, 6, 2);
        AssertWeights(ContentRarityWeightTable.BossChestDefault(), 0, 20, 40, 30, 10);
    }

    [Test]
    public void ContentRaritySetDefaultsAndRangesSupportDesignerRestrictions()
    {
        ContentRaritySet all = ContentRaritySet.All();
        ContentRaritySet rarePlus = ContentRaritySet.Range(ContentRarity.Rare, ContentRarity.Legendary);
        ContentRaritySet uncommonOnly = ContentRaritySet.Only(ContentRarity.Uncommon);

        Assert.That(all.Contains(ContentRarity.Common), Is.True);
        Assert.That(all.Contains(ContentRarity.Legendary), Is.True);
        Assert.That(rarePlus.Contains(ContentRarity.Common), Is.False);
        Assert.That(rarePlus.Contains(ContentRarity.Rare), Is.True);
        Assert.That(rarePlus.Contains(ContentRarity.Legendary), Is.True);
        Assert.That(uncommonOnly.Contains(ContentRarity.Uncommon), Is.True);
        Assert.That(uncommonOnly.Contains(ContentRarity.Rare), Is.False);
    }

    [Test]
    public void ContentRaritySetUsesNarrowFriendlyVerticalAuthoringLayout()
    {
        string[] fields = { "common", "uncommon", "rare", "epic", "legendary" };
        foreach (string fieldName in fields)
        {
            FieldInfo field = typeof(ContentRaritySet).GetField(fieldName);

            Assert.That(field, Is.Not.Null, fieldName);
            Assert.That(field.GetCustomAttribute<ToggleLeftAttribute>(), Is.Not.Null, fieldName);
            Assert.That(field.GetCustomAttribute<VerticalGroupAttribute>(), Is.Not.Null, fieldName);
            Assert.That(field.GetCustomAttribute<GUIColorAttribute>(), Is.Not.Null, fieldName);
            Assert.That(field.GetCustomAttribute<HorizontalGroupAttribute>(), Is.Null, fieldName);
        }
    }

    [Test]
    public void ItemRarityDropdownUsesRarityAuthoringColor()
    {
        FieldInfo field = typeof(ItemDefinition).GetField(nameof(ItemDefinition.rarity));

        Assert.That(field, Is.Not.Null);
        Assert.That(field.GetCustomAttribute<GUIColorAttribute>(), Is.Not.Null);
    }

    [Test]
    public void WorkbenchManagedContentHidesRawIsActiveCheckboxAndRawTags()
    {
        Type[] types =
        {
            typeof(WeaponDefinition),
            typeof(UpgradeDefinition),
            typeof(TalentDefinition),
            typeof(ItemDefinition),
            typeof(PerformerDefinition),
            typeof(EnemyDefinition),
            typeof(XpGemDefinition)
        };

        foreach (Type type in types)
        {
            FieldInfo activeField = type.GetField("isActive");

            if (activeField != null)
            {
                Assert.That(activeField.GetCustomAttribute<HideInInspector>(), Is.Not.Null, type.Name);
            }

            FieldInfo tagsField = type.GetField("tags");
            Assert.That(tagsField, Is.Not.Null, type.Name);
            Assert.That(tagsField.GetCustomAttribute<HideInInspector>(), Is.Not.Null, type.Name);

            PropertyInfo computedTags = type.GetProperty("ComputedTags", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(computedTags, Is.Not.Null, type.Name);
            Assert.That(computedTags.GetCustomAttribute<ShowInInspectorAttribute>(), Is.Not.Null, type.Name);
            Assert.That(computedTags.GetCustomAttribute<ReadOnlyAttribute>(), Is.Not.Null, type.Name);
        }
    }

    [Test]
    public void ContentRarityScalingMatchesV03UpgradeFactors()
    {
        Assert.That(ContentRarityScaling.Factor(ContentRarity.Common), Is.EqualTo(1f));
        Assert.That(ContentRarityScaling.Factor(ContentRarity.Uncommon), Is.EqualTo(1.2f));
        Assert.That(ContentRarityScaling.Factor(ContentRarity.Rare), Is.EqualTo(1.4f));
        Assert.That(ContentRarityScaling.Factor(ContentRarity.Epic), Is.EqualTo(1.6f));
        Assert.That(ContentRarityScaling.Factor(ContentRarity.Legendary), Is.EqualTo(2f));
    }

    [Test]
    public void UpgradeStatModifierDefinitionScalesByRarityAndAllowsExplicitLuckValues()
    {
        var damage = new UpgradeStatModifierDefinition(TheCircussyOne.Stats.StatId.GlobalDamageMultiplier, TheCircussyOne.Stats.StatModifierBucket.AdditivePercent, 0.05f);
        var luck = new UpgradeStatModifierDefinition(TheCircussyOne.Stats.StatId.Luck, TheCircussyOne.Stats.StatModifierBucket.Flat, 3f);
        var explicitOverride = new UpgradeStatModifierDefinition(TheCircussyOne.Stats.StatId.Luck, TheCircussyOne.Stats.StatModifierBucket.Flat, 3f, 5f, 8f, 13f, 21f);

        Assert.That(damage.ValueFor(ContentRarity.Common), Is.EqualTo(0.05f).Within(0.0001f));
        Assert.That(damage.ValueFor(ContentRarity.Legendary), Is.EqualTo(0.10f).Within(0.0001f));
        Assert.That(luck.ValueFor(ContentRarity.Common), Is.EqualTo(3f));
        Assert.That(luck.ValueFor(ContentRarity.Legendary), Is.EqualTo(6f));
        Assert.That(explicitOverride.ValueFor(ContentRarity.Common), Is.EqualTo(3f));
        Assert.That(explicitOverride.ValueFor(ContentRarity.Uncommon), Is.EqualTo(5f));
        Assert.That(explicitOverride.ValueFor(ContentRarity.Rare), Is.EqualTo(8f));
        Assert.That(explicitOverride.ValueFor(ContentRarity.Epic), Is.EqualTo(13f));
        Assert.That(explicitOverride.ValueFor(ContentRarity.Legendary), Is.EqualTo(21f));
    }

    [Test]
    public void MirroredReferenceIdsSyncFromSelectedAssets()
    {
        WeaponDefinition weapon = TheCircussyOneTestObjects.CreateWeaponDefinition(null, "juggling_ball", "Juggling Ball");
        UpgradeDefinition upgrade = TheCircussyOneTestObjects.CreateWeaponStatUpgrade(
            weapon,
            "damage",
            new UpgradeStatModifierDefinition(TheCircussyOne.Stats.StatId.WeaponFlatDamage, TheCircussyOne.Stats.StatModifierBucket.Flat, 1f));
        upgrade.weaponId = "stale_weapon";

        Assert.That(upgrade.SyncWeaponIdFromDefinition(), Is.True);
        Assert.That(upgrade.weaponId, Is.EqualTo("juggling_ball"));

        PerformerDefinition character = TheCircussyOneTestObjects.CreatePerformerDefinition("acrobat", "Acrobat", weapon);
        TalentDefinition talent = TheCircussyOneTestObjects.CreateTalentDefinition("aerial_tempo", "Aerial Tempo");
        talent.poolKind = TalentPoolKind.PerformerSpecific;
        talent.performerDefinition = character;
        talent.performerId = "stale_character";

        Assert.That(talent.SyncPerformerIdFromDefinition(), Is.True);
        Assert.That(talent.performerId, Is.EqualTo("acrobat"));

        TheCircussyOneTestObjects.Destroy(weapon);
        TheCircussyOneTestObjects.Destroy(upgrade);
        TheCircussyOneTestObjects.Destroy(character);
        TheCircussyOneTestObjects.Destroy(talent);
    }

    [Test]
    public void CatalogValidationWarnsWhenMirroredReferenceIdsDrift()
    {
        WeaponDefinition weapon = TheCircussyOneTestObjects.CreateWeaponDefinition(null, "juggling_ball", "Juggling Ball");
        UpgradeDefinition upgrade = TheCircussyOneTestObjects.CreateWeaponStatUpgrade(
            weapon,
            "damage",
            new UpgradeStatModifierDefinition(TheCircussyOne.Stats.StatId.WeaponFlatDamage, TheCircussyOne.Stats.StatModifierBucket.Flat, 1f));
        upgrade.weaponId = "stale_weapon";
        UpgradeCatalog upgradeCatalog = TheCircussyOneTestObjects.CreateUpgradeCatalog(upgrade);

        PerformerDefinition character = TheCircussyOneTestObjects.CreatePerformerDefinition("acrobat", "Acrobat", weapon);
        TalentDefinition talent = TheCircussyOneTestObjects.CreateTalentDefinition("aerial_tempo", "Aerial Tempo");
        talent.poolKind = TalentPoolKind.PerformerSpecific;
        talent.performerDefinition = character;
        talent.performerId = "stale_character";
        TalentCatalog talentCatalog = TheCircussyOneTestObjects.CreateTalentCatalog(talent);

        Assert.That(upgradeCatalog.ValidateContent().Any(issue => issue.Code == "upgrade.weapon-id-reference-mismatch"), Is.True);
        Assert.That(talentCatalog.ValidateContent().Any(issue => issue.Code == "talent.performer-id-reference-mismatch"), Is.True);

        TheCircussyOneTestObjects.Destroy(weapon);
        TheCircussyOneTestObjects.Destroy(upgrade);
        TheCircussyOneTestObjects.Destroy(upgradeCatalog);
        TheCircussyOneTestObjects.Destroy(character);
        TheCircussyOneTestObjects.Destroy(talent);
        TheCircussyOneTestObjects.Destroy(talentCatalog);
    }

    [Test]
    public void ContentCatalogRulesCatchInvalidDefinitions()
    {
        var definitions = new List<TestDefinition>
        {
            null,
            new("juggling_ball", "Juggling Ball", ContentTagSet.With(ContentTag.Projectile)),
            new("juggling_ball", "Duplicate Starter", ContentTagSet.With(ContentTag.Physical)),
            new("", "Missing Id", ContentTagSet.With(ContentTag.Utility)),
            new("missing_name", "", ContentTagSet.With(ContentTag.Utility)),
            new("duplicate_tags", "Duplicate Tags", new ContentTagSet
            {
                values = { ContentTag.XP, ContentTag.XP }
            })
        };

        List<ContentValidationIssue> issues = ContentCatalogRules.ValidateDefinitions(definitions);

        Assert.That(issues.Any(issue => issue.Code == "catalog.null-entry"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "content.duplicate-id"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "content.invalid-id"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "content.missing-display-name"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "content.duplicate-tags"), Is.True);
    }

    [Test]
    public void ContentCatalogRulesCatchInvalidRarityWeights()
    {
        ContentRarityWeightTable negative = ContentRarityWeightTable.LevelUpDefault();
        negative.rare = -1;
        var empty = default(ContentRarityWeightTable);

        Assert.That(ContentCatalogRules.ValidateRarityWeights(negative, "test").Any(issue => issue.Code == "rarity.negative-weight"), Is.True);
        Assert.That(ContentCatalogRules.ValidateRarityWeights(empty, "test").Any(issue => issue.Code == "rarity.empty-table"), Is.True);
    }

    private static void AssertWeights(ContentRarityWeightTable table, int common, int uncommon, int rare, int epic, int legendary)
    {
        Assert.That(table.common, Is.EqualTo(common));
        Assert.That(table.uncommon, Is.EqualTo(uncommon));
        Assert.That(table.rare, Is.EqualTo(rare));
        Assert.That(table.epic, Is.EqualTo(epic));
        Assert.That(table.legendary, Is.EqualTo(legendary));
        Assert.That(table.IsValid, Is.True);
    }

    private static List<T> LoadAssets<T>() where T : UnityEngine.Object
    {
        var assets = new List<T>();
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { "Assets/Game/ScriptableObjects/Balance" });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                assets.Add(asset);
            }
        }

        return assets;
    }

    private static bool TryExpectedFirstPartyWeaponUpgradeValue(StatId statId, out float value)
    {
        switch (statId)
        {
            case StatId.WeaponFlatDamage:
                value = 10f;
                return true;
            case StatId.WeaponProjectileCount:
            case StatId.WeaponPierce:
            case StatId.WeaponBounce:
            case StatId.WeaponChain:
                value = 1f;
                return true;
            case StatId.WeaponAttackSpeed:
                value = 0.05f;
                return true;
            case StatId.WeaponProjectileSpeedMultiplier:
            case StatId.WeaponProjectileSizeMultiplier:
            case StatId.WeaponProjectileLifetimeMultiplier:
            case StatId.WeaponAccuracyMultiplier:
                value = 0.08f;
                return true;
            case StatId.WeaponSplashRadiusMultiplier:
            case StatId.WeaponRangeMultiplier:
                value = 0.1f;
                return true;
            default:
                value = 0f;
                return false;
        }
    }

    private static void AppendInvalidUpgradeModifiers(List<string> failures, string owner, IReadOnlyList<UpgradeStatModifierDefinition> modifiers)
    {
        if (modifiers == null)
        {
            return;
        }

        for (int i = 0; i < modifiers.Count; i++)
        {
            UpgradeStatModifierDefinition modifier = modifiers[i];
            if (!ContentStatValidationRules.IsValid(modifier.statId, modifier.bucket))
            {
                failures.Add($"{owner} modifier {i} uses unsupported {modifier.statId}/{modifier.bucket}.");
            }
        }
    }

    private static void AppendInvalidItemModifiers(List<string> failures, string owner, IReadOnlyList<ItemStatModifierDefinition> modifiers)
    {
        if (modifiers == null)
        {
            return;
        }

        for (int i = 0; i < modifiers.Count; i++)
        {
            ItemStatModifierDefinition modifier = modifiers[i];
            if (!ContentStatValidationRules.IsValid(modifier.statId, modifier.bucket))
            {
                failures.Add($"{owner} modifier {i} uses unsupported {modifier.statId}/{modifier.bucket}.");
            }
        }
    }

    private static void AppendUnexpectedWeaponLocalStats(List<string> failures, string owner, IReadOnlyList<UpgradeStatModifierDefinition> modifiers)
    {
        if (modifiers == null)
        {
            return;
        }

        for (int i = 0; i < modifiers.Count; i++)
        {
            UpgradeStatModifierDefinition modifier = modifiers[i];
            if (ContentStatValidationRules.IsValid(modifier.statId, modifier.bucket)
                && !ContentStatValidationRules.IsTalentStat(modifier.statId))
            {
                failures.Add($"{owner} modifier {i} uses weapon-local stat {modifier.statId}.");
            }
        }
    }

    private static void AppendUnexpectedWeaponLocalItemStats(List<string> failures, string owner, IReadOnlyList<ItemStatModifierDefinition> modifiers)
    {
        if (modifiers == null)
        {
            return;
        }

        for (int i = 0; i < modifiers.Count; i++)
        {
            ItemStatModifierDefinition modifier = modifiers[i];
            if (ContentStatValidationRules.IsValid(modifier.statId, modifier.bucket)
                && !ContentStatValidationRules.IsTalentStat(modifier.statId))
            {
                failures.Add($"{owner} modifier {i} uses weapon-local stat {modifier.statId}.");
            }
        }
    }

    private static float RelativeLuminance(Color color)
    {
        return (0.2126f * color.r) + (0.7152f * color.g) + (0.0722f * color.b);
    }

    private sealed class TestDefinition : IContentDefinition
    {
        public TestDefinition(string id, string displayName, ContentTagSet tags)
        {
            Id = id;
            DisplayName = displayName;
            Tags = tags;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public ContentTagSet Tags { get; }
    }
}
