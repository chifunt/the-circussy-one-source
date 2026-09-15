using System.Linq;
using NUnit.Framework;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using TheCircussyOne.Runtime;
using TheCircussyOne.Stats;
using UnityEngine;

public sealed class ItemDefinitionTests
{
    [Test]
    public void ItemDefinitionDefaultsExposeContentIdentityAndStacking()
    {
        ItemDefinition item = TheCircussyOneTestObjects.CreateItemDefinition(
            "rubber_soles",
            "Rubber Soles",
            maxStacks: 5,
            modifiers: new ItemStatModifierDefinition(StatId.PlayerMoveSpeedMultiplier, StatModifierBucket.AdditivePercent, 0.08f));

        Assert.That(item.Id, Is.EqualTo("rubber_soles"));
        Assert.That(item.DisplayName, Is.EqualTo("Rubber Soles"));
        Assert.That(item.Tags.HasTag(ContentTag.Movement), Is.True);
        Assert.That(item.stackPolicy, Is.EqualTo(ItemStackPolicy.StackLinear));
        Assert.That(item.EffectiveMaxStacks, Is.EqualTo(5));
        Assert.That(item.statModifiers, Has.Count.EqualTo(1));
        Assert.That(item.statModifiers[0].ToRuntime("item:test:1").StatId, Is.EqualTo(StatId.PlayerMoveSpeedMultiplier));

        TheCircussyOneTestObjects.Destroy(item);
    }

    [Test]
    public void UniqueItemsRuntimeMaxStacksToOne()
    {
        ItemDefinition item = TheCircussyOneTestObjects.CreateItemDefinition(
            "unique_charm",
            "Unique Charm",
            maxStacks: 7,
            stackPolicy: ItemStackPolicy.Unique);

        Assert.That(item.EffectiveMaxStacks, Is.EqualTo(1));

        TheCircussyOneTestObjects.Destroy(item);
    }

    [Test]
    public void ItemStackRulesEvaluateLinearCappedDiminishingAndDuplicatePolicies()
    {
        ItemDefinition linear = TheCircussyOneTestObjects.CreateItemDefinition("linear_item", "Linear Item", maxStacks: 4);
        ItemDefinition capped = TheCircussyOneTestObjects.CreateItemDefinition("capped_item", "Capped Item", maxStacks: 5, stackPolicy: ItemStackPolicy.StackWithCap);
        capped.effectCapStacks = 2;
        ItemDefinition diminishing = TheCircussyOneTestObjects.CreateItemDefinition("diminishing_item", "Diminishing Item", maxStacks: 3, stackPolicy: ItemStackPolicy.StackDiminishing);
        diminishing.diminishingFalloff = 0.5f;
        ItemDefinition invalidDuplicate = TheCircussyOneTestObjects.CreateItemDefinition("invalid_duplicate", "Invalid Duplicate", maxStacks: 9, stackPolicy: ItemStackPolicy.InvalidDuplicate);

        Assert.That(ItemStackRules.MaxStacks(linear), Is.EqualTo(4));
        Assert.That(ItemStackRules.EffectiveModifierStackCount(linear), Is.EqualTo(4));
        Assert.That(ItemStackRules.ModifierMultiplier(linear, 4), Is.EqualTo(1f));
        Assert.That(ItemStackRules.StackSummary(linear), Is.EqualTo("Stack Linear x4"));

        Assert.That(ItemStackRules.MaxStacks(capped), Is.EqualTo(5));
        Assert.That(ItemStackRules.EffectiveModifierStackCount(capped), Is.EqualTo(2));
        Assert.That(ItemStackRules.ModifierMultiplier(capped, 2), Is.EqualTo(1f));
        Assert.That(ItemStackRules.ModifierMultiplier(capped, 3), Is.Zero);

        Assert.That(ItemStackRules.ModifierMultiplier(diminishing, 1), Is.EqualTo(1f).Within(0.001f));
        Assert.That(ItemStackRules.ModifierMultiplier(diminishing, 2), Is.EqualTo(2f / 3f).Within(0.001f));
        Assert.That(ItemStackRules.ModifierMultiplier(diminishing, 3), Is.EqualTo(0.5f).Within(0.001f));

        Assert.That(ItemStackRules.MaxStacks(invalidDuplicate), Is.EqualTo(1));
        Assert.That(ItemStackRules.CanAddItem(invalidDuplicate, 0), Is.True);
        Assert.That(ItemStackRules.CanAddItem(invalidDuplicate, 1), Is.False);

        TheCircussyOneTestObjects.Destroy(linear);
        TheCircussyOneTestObjects.Destroy(capped);
        TheCircussyOneTestObjects.Destroy(diminishing);
        TheCircussyOneTestObjects.Destroy(invalidDuplicate);
    }

    [Test]
    public void ItemCatalogValidatesInvalidDefinitions()
    {
        ItemDefinition first = TheCircussyOneTestObjects.CreateItemDefinition("rubber_soles", "Rubber Soles");
        ItemDefinition duplicate = TheCircussyOneTestObjects.CreateItemDefinition("rubber_soles", "Duplicate Soles");
        ItemDefinition invalid = TheCircussyOneTestObjects.CreateItemDefinition("", "");
        invalid.itemId = "";
        invalid.displayName = "";
        invalid.maxStacks = 0;
        invalid.statModifiers.Clear();
        ItemDefinition invalidStat = TheCircussyOneTestObjects.CreateItemDefinition("invalid_stat", "Invalid Stat");
        invalidStat.statModifiers.Clear();
        invalidStat.statModifiers.Add(new ItemStatModifierDefinition((StatId)999, StatModifierBucket.Flat, 1f));
        ItemDefinition weaponStat = TheCircussyOneTestObjects.CreateItemDefinition("weapon_stat", "Weapon Stat");
        weaponStat.statModifiers.Clear();
        weaponStat.statModifiers.Add(new ItemStatModifierDefinition(StatId.WeaponFlatDamage, StatModifierBucket.Flat, 1f));
        ItemDefinition badCap = TheCircussyOneTestObjects.CreateItemDefinition("bad_cap", "Bad Cap", maxStacks: 2, stackPolicy: ItemStackPolicy.StackWithCap);
        badCap.effectCapStacks = 3;
        ItemDefinition badFalloff = TheCircussyOneTestObjects.CreateItemDefinition("bad_falloff", "Bad Falloff", stackPolicy: ItemStackPolicy.StackDiminishing);
        badFalloff.diminishingFalloff = -0.1f;
        ItemDefinition downsideWeaponStat = TheCircussyOneTestObjects.CreateItemDefinition("downside_weapon_stat", "Downside Weapon Stat");
        downsideWeaponStat.downsideStatModifiers.Add(new ItemStatModifierDefinition(StatId.WeaponFlatDamage, StatModifierBucket.Flat, -1f));
        ItemCatalog catalog = TheCircussyOneTestObjects.CreateItemCatalog(first, duplicate, invalid, invalidStat, weaponStat, badCap, badFalloff, downsideWeaponStat, null);

        var issues = catalog.ValidateContent();

        Assert.That(issues.Any(issue => issue.Code == "catalog.null-entry"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "content.duplicate-id"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "content.invalid-id"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "content.missing-display-name"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "item.invalid-max-stacks"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "item.missing-stat-modifiers"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "item.invalid-stat-modifier"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "item.weapon-stat" && issue.Message.Contains("Weapon Damage")), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "item.effect-cap-exceeds-max"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "item.invalid-diminishing-falloff"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "item.downside-weapon-stat" && issue.Message.Contains("Weapon Damage")), Is.True);

        TheCircussyOneTestObjects.Destroy(catalog);
        TheCircussyOneTestObjects.Destroy(first);
        TheCircussyOneTestObjects.Destroy(duplicate);
        TheCircussyOneTestObjects.Destroy(invalid);
        TheCircussyOneTestObjects.Destroy(invalidStat);
        TheCircussyOneTestObjects.Destroy(weaponStat);
        TheCircussyOneTestObjects.Destroy(badCap);
        TheCircussyOneTestObjects.Destroy(badFalloff);
        TheCircussyOneTestObjects.Destroy(downsideWeaponStat);
    }

    [Test]
    public void ItemInventoryAddsStacksAndCapsAtDefinitionMax()
    {
        ItemDefinition item = TheCircussyOneTestObjects.CreateItemDefinition("rubber_soles", "Rubber Soles", maxStacks: 3);
        var inventory = new ItemInventory();

        Assert.That(inventory.AddItem(item), Is.EqualTo(1));
        Assert.That(inventory.AddItem(item, 5), Is.EqualTo(2));
        Assert.That(inventory.AddItem(item), Is.Zero);
        Assert.That(inventory.ItemCount, Is.EqualTo(1));
        Assert.That(inventory.FindById("rubber_soles").StackCount, Is.EqualTo(3));
        Assert.That(inventory.FindById("rubber_soles").IsAtMax, Is.True);

        TheCircussyOneTestObjects.Destroy(item);
    }

    [Test]
    public void ItemInventoryRaisesItemAddedWithAddedAndCurrentStacks()
    {
        ItemDefinition item = TheCircussyOneTestObjects.CreateItemDefinition("rubber_soles", "Rubber Soles", maxStacks: 3);
        var inventory = new ItemInventory();
        var events = new System.Collections.Generic.List<ItemAddedEvent>();
        inventory.ItemAdded += events.Add;

        Assert.That(inventory.AddItem(item), Is.EqualTo(1));
        Assert.That(inventory.AddItem(item, 5), Is.EqualTo(2));
        Assert.That(inventory.AddItem(item), Is.Zero);

        Assert.That(events.Count, Is.EqualTo(2));
        Assert.That(events[0].Definition, Is.SameAs(item));
        Assert.That(events[0].AddedStacks, Is.EqualTo(1));
        Assert.That(events[0].CurrentStackCount, Is.EqualTo(1));
        Assert.That(events[0].Source, Is.EqualTo(ItemGrantSource.WorldItem));
        Assert.That(events[1].AddedStacks, Is.EqualTo(2));
        Assert.That(events[1].CurrentStackCount, Is.EqualTo(3));
        Assert.That(events[1].Source, Is.EqualTo(ItemGrantSource.WorldItem));

        TheCircussyOneTestObjects.Destroy(item);
    }

    [Test]
    public void ItemInventoryAppliesStatModifiersForEachAddedStack()
    {
        GameConfig config = TheCircussyOneTestObjects.CreateConfig();
        var stats = new RunStats(config);
        ItemDefinition item = TheCircussyOneTestObjects.CreateItemDefinition(
            "safety_padding",
            "Safety Padding",
            maxStacks: 3,
            modifiers: new ItemStatModifierDefinition(StatId.PlayerArmor, StatModifierBucket.Flat, 4f));
        var inventory = new ItemInventory(stats);

        Assert.That(inventory.AddItem(item), Is.EqualTo(1));
        Assert.That(inventory.AddItem(item, 5), Is.EqualTo(2));

        Assert.That(stats.GetFloat(StatId.PlayerArmor), Is.EqualTo(12f).Within(0.001f));
        Assert.That(stats.Modifiers.Select(modifier => modifier.SourceId), Does.Contain(ItemInventory.ModifierSourceId("safety_padding", 1)));
        Assert.That(stats.Modifiers.Select(modifier => modifier.SourceId), Does.Contain(ItemInventory.ModifierSourceId("safety_padding", 2)));
        Assert.That(stats.Modifiers.Select(modifier => modifier.SourceId), Does.Contain(ItemInventory.ModifierSourceId("safety_padding", 3)));

        TheCircussyOneTestObjects.Destroy(item);
        TheCircussyOneTestObjects.Destroy(config);
    }

    [Test]
    public void ItemInventoryStackWithCapAppliesModifiersOnlyThroughEffectCap()
    {
        GameConfig config = TheCircussyOneTestObjects.CreateConfig();
        var stats = new RunStats(config);
        ItemDefinition item = TheCircussyOneTestObjects.CreateItemDefinition(
            "capped_padding",
            "Capped Padding",
            maxStacks: 5,
            stackPolicy: ItemStackPolicy.StackWithCap,
            modifiers: new ItemStatModifierDefinition(StatId.PlayerArmor, StatModifierBucket.Flat, 4f));
        item.effectCapStacks = 2;
        var inventory = new ItemInventory(stats);

        Assert.That(inventory.AddItem(item, 5), Is.EqualTo(5));

        Assert.That(inventory.FindById("capped_padding").StackCount, Is.EqualTo(5));
        Assert.That(stats.GetFloat(StatId.PlayerArmor), Is.EqualTo(8f).Within(0.001f));
        Assert.That(stats.Modifiers.Select(modifier => modifier.SourceId), Does.Contain(ItemInventory.ModifierSourceId("capped_padding", 1)));
        Assert.That(stats.Modifiers.Select(modifier => modifier.SourceId), Does.Contain(ItemInventory.ModifierSourceId("capped_padding", 2)));
        Assert.That(stats.Modifiers.Select(modifier => modifier.SourceId), Does.Not.Contain(ItemInventory.ModifierSourceId("capped_padding", 3)));

        TheCircussyOneTestObjects.Destroy(item);
        TheCircussyOneTestObjects.Destroy(config);
    }

    [Test]
    public void ItemInventoryDiminishingStacksScaleModifierValues()
    {
        GameConfig config = TheCircussyOneTestObjects.CreateConfig();
        var stats = new RunStats(config);
        ItemDefinition item = TheCircussyOneTestObjects.CreateItemDefinition(
            "diminishing_padding",
            "Diminishing Padding",
            maxStacks: 3,
            stackPolicy: ItemStackPolicy.StackDiminishing,
            modifiers: new ItemStatModifierDefinition(StatId.PlayerArmor, StatModifierBucket.Flat, 6f));
        item.diminishingFalloff = 0.5f;
        var inventory = new ItemInventory(stats);

        Assert.That(inventory.AddItem(item, 3), Is.EqualTo(3));

        Assert.That(stats.GetFloat(StatId.PlayerArmor), Is.EqualTo(13f).Within(0.001f));

        TheCircussyOneTestObjects.Destroy(item);
        TheCircussyOneTestObjects.Destroy(config);
    }

    [Test]
    public void ItemInventoryRiskItemsApplyAndRemoveUpsideAndDownsideTogether()
    {
        GameConfig config = TheCircussyOneTestObjects.CreateConfig();
        var stats = new RunStats(config);
        ItemDefinition item = TheCircussyOneTestObjects.CreateItemDefinition(
            "glass_padding",
            "Glass Padding",
            maxStacks: 2,
            modifiers: new ItemStatModifierDefinition(StatId.PlayerArmor, StatModifierBucket.Flat, 5f));
        item.downsideStatModifiers.Add(new ItemStatModifierDefinition(StatId.PlayerMaxHealth, StatModifierBucket.Flat, -10f));
        var inventory = new ItemInventory(stats);

        Assert.That(inventory.AddItem(item, 2), Is.EqualTo(2));

        Assert.That(item.HasDownside, Is.True);
        Assert.That(stats.GetFloat(StatId.PlayerArmor), Is.EqualTo(10f).Within(0.001f));
        Assert.That(stats.GetFloat(StatId.PlayerMaxHealth), Is.EqualTo(config.playerMaxHealth - 20f).Within(0.001f));

        Assert.That(inventory.RemoveItem("glass_padding"), Is.EqualTo(1));

        Assert.That(stats.GetFloat(StatId.PlayerArmor), Is.EqualTo(5f).Within(0.001f));
        Assert.That(stats.GetFloat(StatId.PlayerMaxHealth), Is.EqualTo(config.playerMaxHealth - 10f).Within(0.001f));

        TheCircussyOneTestObjects.Destroy(item);
        TheCircussyOneTestObjects.Destroy(config);
    }

    [Test]
    public void ItemInventoryRemovesNewestStackModifiersWithoutTouchingOtherSources()
    {
        GameConfig config = TheCircussyOneTestObjects.CreateConfig();
        var stats = new RunStats(config);
        ItemDefinition item = TheCircussyOneTestObjects.CreateItemDefinition(
            "safety_padding",
            "Safety Padding",
            maxStacks: 3,
            modifiers: new ItemStatModifierDefinition(StatId.PlayerArmor, StatModifierBucket.Flat, 4f));
        var inventory = new ItemInventory(stats);
        stats.AddModifier(StatModifier.Flat(StatId.PlayerArmor, 10f, "debug:armor"));
        inventory.AddItem(item, 3);

        Assert.That(inventory.RemoveItem("safety_padding", 2), Is.EqualTo(2));

        Assert.That(inventory.FindById("safety_padding").StackCount, Is.EqualTo(1));
        Assert.That(stats.GetFloat(StatId.PlayerArmor), Is.EqualTo(14f).Within(0.001f));
        Assert.That(stats.Modifiers.Select(modifier => modifier.SourceId), Does.Contain(ItemInventory.ModifierSourceId("safety_padding", 1)));
        Assert.That(stats.Modifiers.Select(modifier => modifier.SourceId), Does.Not.Contain(ItemInventory.ModifierSourceId("safety_padding", 2)));
        Assert.That(stats.Modifiers.Select(modifier => modifier.SourceId), Does.Not.Contain(ItemInventory.ModifierSourceId("safety_padding", 3)));
        Assert.That(stats.Modifiers.Select(modifier => modifier.SourceId), Does.Contain("debug:armor"));

        TheCircussyOneTestObjects.Destroy(item);
        TheCircussyOneTestObjects.Destroy(config);
    }

    [Test]
    public void ItemInventoryRemovesStackWhenFinalModifierStackIsRemoved()
    {
        GameConfig config = TheCircussyOneTestObjects.CreateConfig();
        var stats = new RunStats(config);
        ItemDefinition item = TheCircussyOneTestObjects.CreateItemDefinition(
            "magnet_charm",
            "Magnet Charm",
            maxStacks: 2,
            modifiers: new ItemStatModifierDefinition(StatId.PickupMagnetRadius, StatModifierBucket.Flat, 1.5f));
        var inventory = new ItemInventory(stats);
        inventory.AddItem(item);

        Assert.That(inventory.RemoveItem("magnet_charm"), Is.EqualTo(1));

        Assert.That(inventory.ItemCount, Is.Zero);
        Assert.That(inventory.OwnsItem("magnet_charm"), Is.False);
        Assert.That(stats.GetFloat(StatId.PickupMagnetRadius), Is.EqualTo(config.pickupMagnetRadius).Within(0.001f));
        Assert.That(stats.Modifiers.Select(modifier => modifier.SourceId), Does.Not.Contain(ItemInventory.ModifierSourceId("magnet_charm", 1)));

        TheCircussyOneTestObjects.Destroy(item);
        TheCircussyOneTestObjects.Destroy(config);
    }

    [Test]
    public void ItemInventoryRejectsInvalidOrDuplicateUniqueItems()
    {
        ItemDefinition unique = TheCircussyOneTestObjects.CreateItemDefinition(
            "unique_charm",
            "Unique Charm",
            stackPolicy: ItemStackPolicy.Unique);
        var inventory = new ItemInventory();

        Assert.That(inventory.AddItem(null), Is.Zero);
        Assert.That(inventory.AddItem(unique), Is.EqualTo(1));
        Assert.That(inventory.AddItem(unique), Is.Zero);
        Assert.That(inventory.OwnsItem("unique_charm"), Is.True);
        Assert.That(inventory.FindById("unique_charm").StackCount, Is.EqualTo(1));

        TheCircussyOneTestObjects.Destroy(unique);
    }

    [Test]
    public void ItemInventoryRejectsInvalidDuplicateItemsAfterFirstCopy()
    {
        ItemDefinition invalidDuplicate = TheCircussyOneTestObjects.CreateItemDefinition(
            "invalid_duplicate",
            "Invalid Duplicate",
            stackPolicy: ItemStackPolicy.InvalidDuplicate);
        var inventory = new ItemInventory();

        Assert.That(inventory.AddItem(invalidDuplicate), Is.EqualTo(1));
        Assert.That(inventory.AddItem(invalidDuplicate), Is.Zero);
        Assert.That(inventory.FindById("invalid_duplicate").StackCount, Is.EqualTo(1));

        TheCircussyOneTestObjects.Destroy(invalidDuplicate);
    }
}
