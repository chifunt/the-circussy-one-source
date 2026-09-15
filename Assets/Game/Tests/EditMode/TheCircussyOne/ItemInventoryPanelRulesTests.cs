using NUnit.Framework;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using TheCircussyOne.Runtime;
using TheCircussyOne.Stats;

public sealed class ItemInventoryPanelRulesTests
{
    [TearDown]
    public void TearDown()
    {
        TheCircussyOneTestObjects.DestroyAll();
    }

    [Test]
    public void BuildFramesReturnsEmptyForMissingInventory()
    {
        Assert.That(ItemInventoryPanelRules.BuildFrames(null), Is.Empty);
    }

    [Test]
    public void BuildFrameUsesItemIdentityRarityAndStackText()
    {
        ItemDefinition item = TheCircussyOneTestObjects.CreateItemDefinition(
            "rubber_soles",
            "Rubber Soles",
            maxStacks: 5,
            modifiers: new ItemStatModifierDefinition(StatId.PlayerArmor, StatModifierBucket.Flat, 2f));
        item.shortDescription = "Soft steps for rough tents.";
        item.rarity = ContentRarity.Rare;

        ItemInventoryCardFrame frame = ItemInventoryPanelRules.BuildFrame(new ItemStackRuntime(item, 3));

        Assert.That(frame.DisplayName, Is.EqualTo("Rubber Soles"));
        Assert.That(frame.RarityLabel, Is.EqualTo("Rare"));
        Assert.That(frame.ShortDescription, Is.EqualTo("Soft steps for rough tents."));
        Assert.That(frame.StackText, Is.EqualTo("x3 / 5"));
        Assert.That(frame.StackCount, Is.EqualTo(3));
        Assert.That(frame.MaxStacks, Is.EqualTo(5));
    }

    [Test]
    public void BuildFrameAggregatesLinearStackEffects()
    {
        ItemDefinition item = TheCircussyOneTestObjects.CreateItemDefinition(
            "iron_ticket",
            "Iron Ticket",
            maxStacks: 4,
            modifiers: new ItemStatModifierDefinition(StatId.PlayerArmor, StatModifierBucket.Flat, 2f));

        ItemInventoryCardFrame frame = ItemInventoryPanelRules.BuildFrame(new ItemStackRuntime(item, 3));

        Assert.That(frame.EffectText, Does.Contain("Armor <color=#73F59A>+6</color>"));
        Assert.That(frame.EffectText, Does.Not.Contain("(Player, flat)"));
    }

    [Test]
    public void BuildFrameShowsFractionalIntegerStatProgress()
    {
        ItemDefinition item = TheCircussyOneTestObjects.CreateItemDefinition(
            "springboard_stub",
            "Springboard Stub",
            maxStacks: 4,
            modifiers: new ItemStatModifierDefinition(StatId.PlayerExtraJumps, StatModifierBucket.Flat, 0.5f));

        ItemInventoryCardFrame frame = ItemInventoryPanelRules.BuildFrame(new ItemStackRuntime(item, 1));

        Assert.That(frame.EffectText, Does.Contain("Extra Jumps <color=#73F59A>+0.5</color>"));
        Assert.That(frame.EffectText, Does.Not.Contain("(Player, flat)"));
    }

    [Test]
    public void BuildFrameHonorsEffectCapStacks()
    {
        ItemDefinition item = TheCircussyOneTestObjects.CreateItemDefinition(
            "capped_ticket",
            "Capped Ticket",
            maxStacks: 5,
            stackPolicy: ItemStackPolicy.StackWithCap,
            modifiers: new ItemStatModifierDefinition(StatId.PlayerArmor, StatModifierBucket.Flat, 2f));
        item.effectCapStacks = 2;

        ItemInventoryCardFrame frame = ItemInventoryPanelRules.BuildFrame(new ItemStackRuntime(item, 5));

        Assert.That(frame.EffectText, Does.Contain("Armor <color=#73F59A>+4</color>"));
    }

    [Test]
    public void BuildFrameAggregatesDownsides()
    {
        ItemDefinition item = TheCircussyOneTestObjects.CreateItemDefinition(
            "heavy_ticket",
            "Heavy Ticket",
            maxStacks: 2,
            modifiers: new ItemStatModifierDefinition(StatId.PlayerArmor, StatModifierBucket.Flat, 3f));
        item.downsideStatModifiers.Add(new ItemStatModifierDefinition(StatId.PlayerMoveSpeedMultiplier, StatModifierBucket.AdditivePercent, -0.05f));

        ItemInventoryCardFrame frame = ItemInventoryPanelRules.BuildFrame(new ItemStackRuntime(item, 2));

        Assert.That(frame.EffectText, Does.Contain("Armor <color=#73F59A>+6</color>"));
        Assert.That(frame.EffectText, Does.Contain("Movement Speed <color=#FF5A66>-10%</color>"));
    }
}
