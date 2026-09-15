using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;

public sealed class XpGemDefinitionTests
{
    [Test]
    public void XpGemDefaultsExposeContentIdentityAndReward()
    {
        XpGemDefinition gem = TheCircussyOneTestObjects.CreateXpGemDefinition(
            "blue_xp_gem",
            "Blue XP Gem",
            7,
            new Color(0.27f, 0.96f, 1f, 1f));

        Assert.That(gem.Id, Is.EqualTo("blue_xp_gem"));
        Assert.That(gem.DisplayName, Is.EqualTo("Blue XP Gem"));
        Assert.That(gem.xpAmount, Is.EqualTo(7));
        Assert.That(gem.Tags.HasTag(ContentTag.Economy), Is.True);
        Assert.That(gem.Tags.HasTag(ContentTag.Pickup), Is.True);
        Assert.That(gem.Tags.HasTag(ContentTag.XP), Is.True);
        Assert.That(gem.rarity, Is.EqualTo(ContentRarity.Common));
        Assert.That(gem.emissionColor, Is.EqualTo(new Color(0.27f, 0.96f, 1f, 1f)));
        Assert.That(gem.emissionStrength, Is.EqualTo(XpGemDefinition.DefaultEmissionStrength));

        Object.DestroyImmediate(gem);
    }

    [Test]
    public void XpGemCatalogValidatesInvalidDefinitions()
    {
        XpGemDefinition duplicate = TheCircussyOneTestObjects.CreateXpGemDefinition("blue_xp_gem", "Duplicate", 7, Color.cyan);
        XpGemDefinition missingName = TheCircussyOneTestObjects.CreateXpGemDefinition("missing_name", "Missing", 7, Color.cyan);
        missingName.displayName = "";
        XpGemDefinition invalidAmount = TheCircussyOneTestObjects.CreateXpGemDefinition("invalid_amount", "Invalid Amount", 7, Color.cyan);
        invalidAmount.xpAmount = 0;
        XpGemCatalog catalog = TheCircussyOneTestObjects.CreateXpGemCatalog(
            TheCircussyOneTestObjects.CreateXpGemDefinition("blue_xp_gem", "Blue XP Gem", 7, Color.cyan),
            duplicate,
            missingName,
            invalidAmount,
            null);

        var issues = catalog.ValidateContent();

        Assert.That(issues.Any(issue => issue.Code == "content.duplicate-id"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "content.missing-display-name"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "xp-gem.non-positive-xp"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "catalog.null-entry"), Is.True);

        foreach (XpGemDefinition gem in catalog.gems.Where(gem => gem != null))
        {
            Object.DestroyImmediate(gem);
        }

        Object.DestroyImmediate(catalog);
    }

    [Test]
    public void CompactDropRulesConvertBudgetToFewestGems()
    {
        XpGemDefinition blue = TheCircussyOneTestObjects.CreateXpGemDefinition("blue_xp_gem", "Blue XP Gem", 7, Color.cyan);
        XpGemDefinition green = TheCircussyOneTestObjects.CreateXpGemDefinition("green_xp_gem", "Green XP Gem", 23, Color.green);
        XpGemDefinition red = TheCircussyOneTestObjects.CreateXpGemDefinition("red_xp_gem", "Red XP Gem", 73, Color.red);
        XpGemDefinition[] gems = { blue, green, red };

        AssertDrops(XpDropRules.BuildDrops(7, gems, XpDropStyle.Compact), "blue_xp_gem");
        AssertDrops(XpDropRules.BuildDrops(30, gems, XpDropStyle.Compact), "green_xp_gem", "blue_xp_gem");
        AssertDrops(XpDropRules.BuildDrops(103, gems, XpDropStyle.Compact), "red_xp_gem", "green_xp_gem", "blue_xp_gem");
        Assert.That(XpDropRules.BuildDrops(1, gems, XpDropStyle.Compact), Is.Empty);
        Assert.That(XpDropRules.BuildDrops(0, gems, XpDropStyle.Compact), Is.Empty);
        Assert.That(XpDropRules.BuildDrops(-1, gems, XpDropStyle.Compact), Is.Empty);

        Object.DestroyImmediate(blue);
        Object.DestroyImmediate(green);
        Object.DestroyImmediate(red);
    }

    [Test]
    public void DropStylesAreDeterministicAndBounded()
    {
        XpGemDefinition blue = TheCircussyOneTestObjects.CreateXpGemDefinition("blue_xp_gem", "Blue XP Gem", 7, Color.cyan);
        XpGemDefinition green = TheCircussyOneTestObjects.CreateXpGemDefinition("green_xp_gem", "Green XP Gem", 23, Color.green);
        XpGemDefinition[] gems = { blue, green };

        AssertDrops(XpDropRules.BuildDrops(30, gems, XpDropStyle.Scatter, seed: 123), "blue_xp_gem", "blue_xp_gem", "blue_xp_gem", "blue_xp_gem");
        Assert.That(XpDropRules.BuildDrops(7, gems, XpDropStyle.Bonus, seed: 123).Sum(drop => drop.Amount), Is.EqualTo(14));
        Assert.That(XpDropRules.BuildDrops(7, gems, XpDropStyle.LowRoll, seed: 123), Is.Empty);

        Object.DestroyImmediate(blue);
        Object.DestroyImmediate(green);
    }

    [Test]
    public void CatalogWorkflowDefaultsPreserveTunedBudgetAndAddMissingBuiltIns()
    {
        XpGemDefinition blue = TheCircussyOneTestObjects.CreateXpGemDefinition("blue_xp_gem", "Blue XP Gem", 7, Color.cyan);
        XpGemDefinition green = TheCircussyOneTestObjects.CreateXpGemDefinition("green_xp_gem", "Green XP Gem", 23, Color.green);
        XpGemDefinition red = TheCircussyOneTestObjects.CreateXpGemDefinition("red_xp_gem", "Red XP Gem", 73, Color.red);
        XpGemCatalog catalog = TheCircussyOneTestObjects.CreateXpGemCatalog(blue);
        catalog.defaultEnemyXpBudget = 7;

        bool changed = catalog.EnsureWorkflowDefaults(blue, green, red, defaultBudget: 1);

        Assert.That(changed, Is.True);
        Assert.That(catalog.defaultEnemyXpBudget, Is.EqualTo(7));
        Assert.That(catalog.gems, Is.EquivalentTo(new[] { blue, green, red }));

        Object.DestroyImmediate(catalog);
        Object.DestroyImmediate(blue);
        Object.DestroyImmediate(green);
        Object.DestroyImmediate(red);
    }

    [Test]
    public void BuiltInXpGemAssetsUseLockedValuesAndPreserveTunedVisuals()
    {
        XpGemDefinition blue = AssetDatabase.LoadAssetAtPath<XpGemDefinition>(TheCircussyOneAssetPaths.BlueXpGemPath);
        XpGemDefinition green = AssetDatabase.LoadAssetAtPath<XpGemDefinition>(TheCircussyOneAssetPaths.GreenXpGemPath);
        XpGemDefinition red = AssetDatabase.LoadAssetAtPath<XpGemDefinition>(TheCircussyOneAssetPaths.RedXpGemPath);
        XpGemCatalog catalog = AssetDatabase.LoadAssetAtPath<XpGemCatalog>(TheCircussyOneAssetPaths.XpGemCatalogPath);
        EnemyDefinition normalEnemy = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(TheCircussyOneAssetPaths.NormalEnemyPath);

        Assert.That(blue, Is.Not.Null);
        Assert.That(green, Is.Not.Null);
        Assert.That(red, Is.Not.Null);
        Assert.That(catalog, Is.Not.Null);
        Assert.That(normalEnemy, Is.Not.Null);
        Assert.That(blue.xpAmount, Is.EqualTo(7));
        Assert.That(green.xpAmount, Is.EqualTo(23));
        Assert.That(red.xpAmount, Is.EqualTo(73));
        Assert.That(catalog.defaultEnemyXpBudget, Is.EqualTo(7));
        Assert.That(normalEnemy.xpBudget, Is.EqualTo(7));
        Assert.That(blue.color, Is.EqualTo(new Color(0.27f, 0.96f, 1f, 1f)));
        Assert.That(green.color, Is.EqualTo(new Color(0.38f, 1f, 0.42f, 1f)));
        Assert.That(red.color, Is.EqualTo(new Color(1f, 0.22f, 0.34f, 1f)));
        Assert.That(blue.visualScale, Is.EqualTo(1f));
        Assert.That(green.visualScale, Is.EqualTo(1.12f));
        Assert.That(red.visualScale, Is.EqualTo(1.28f));
        Assert.That(blue.emissionColor, Is.EqualTo(blue.color));
        Assert.That(green.emissionColor, Is.EqualTo(green.color));
        Assert.That(red.emissionColor, Is.EqualTo(red.color));
        Assert.That(blue.emissionStrength, Is.GreaterThan(1f));
        Assert.That(green.emissionStrength, Is.GreaterThan(1f));
        Assert.That(red.emissionStrength, Is.GreaterThan(1f));
        Assert.That(blue.Tags.HasTag(ContentTag.Economy), Is.True);
        Assert.That(blue.Tags.HasTag(ContentTag.Pickup), Is.True);
        Assert.That(blue.Tags.HasTag(ContentTag.XP), Is.True);
    }

    [Test]
    public void WorkflowDefaultsUseCurrentBuiltInXpAmountWithoutMigration()
    {
        XpGemDefinition gem = ScriptableObject.CreateInstance<XpGemDefinition>();
        XpGemCatalog catalog = ScriptableObject.CreateInstance<XpGemCatalog>();
        EnemyDefinition enemy = ScriptableObject.CreateInstance<EnemyDefinition>();

        gem.xpAmount = 0;
        catalog.defaultEnemyXpBudget = -1;
        enemy.xpBudget = -1;

        Assert.That(gem.EnsureWorkflowDefaults(), Is.True);
        Assert.That(catalog.EnsureWorkflowDefaults(gem, null, null, XpGemDefinition.DefaultBlueXpAmount), Is.True);
        Assert.That(enemy.EnsureWorkflowDefaults(), Is.True);
        Assert.That(gem.xpAmount, Is.EqualTo(XpGemDefinition.DefaultBlueXpAmount));
        Assert.That(catalog.defaultEnemyXpBudget, Is.EqualTo(XpGemDefinition.DefaultBlueXpAmount));
        Assert.That(enemy.xpBudget, Is.EqualTo(XpGemDefinition.DefaultBlueXpAmount));

        Object.DestroyImmediate(gem);
        Object.DestroyImmediate(catalog);
        Object.DestroyImmediate(enemy);
    }

    private static void AssertDrops(System.Collections.Generic.IReadOnlyList<XpGemDrop> drops, params string[] expectedIds)
    {
        Assert.That(drops.Select(drop => drop.Gem.Id).ToArray(), Is.EqualTo(expectedIds));
    }
}
