using System.Linq;
using NUnit.Framework;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using UnityEngine;

public sealed class XpPacingProjectionTests
{
    [Test]
    public void ProjectionUsesCurveMultiplierAndProjectsCurrentCadence()
    {
        GameConfig config = TheCircussyOneTestObjects.CreateConfig();
        XpGemDefinition blue = TheCircussyOneTestObjects.CreateXpGemDefinition("blue_xp_gem", "Blue XP Gem", 7, Color.cyan);
        XpGemCatalog gems = TheCircussyOneTestObjects.CreateXpGemCatalog(blue);
        EnemyDefinition enemy = TheCircussyOneTestObjects.CreateEnemyDefinition(config);
        enemy.displayName = "Normal Enemy";
        enemy.spawnWeight = 100f;
        enemy.xpBudget = 7;
        EnemyCatalog enemies = TheCircussyOneTestObjects.CreateEnemyCatalog(enemy);

        XpPacingProjectionResult result = XpPacingProjectionRules.Project(
            config,
            enemies,
            gems,
            XpPacingProjectionSettings.Default);

        XpPacingProjectionRow twoMinuteUpperBound = result.Rows.Single(row =>
            Mathf.Abs(row.CheckpointSeconds - 120f) < 0.01f
            && Mathf.Abs(row.KillEfficiency - 1f) < 0.001f);

        Assert.That(result.WeightedXpPerKill, Is.EqualTo(7f).Within(0.001f));
        Assert.That(twoMinuteUpperBound.ProjectedLevel, Is.InRange(6, 8));
        Assert.That(twoMinuteUpperBound.PopupCount, Is.EqualTo(twoMinuteUpperBound.ProjectedLevel - 1));

        Object.DestroyImmediate(config);
        Object.DestroyImmediate(blue);
        Object.DestroyImmediate(gems);
        Object.DestroyImmediate(enemy);
        Object.DestroyImmediate(enemies);
    }

    [Test]
    public void WeightedEnemyXpUsesSpawnWeightsBudgetsAndGemDrops()
    {
        GameConfig config = TheCircussyOneTestObjects.CreateConfig();
        XpGemDefinition blue = TheCircussyOneTestObjects.CreateXpGemDefinition("blue_xp_gem", "Blue XP Gem", 7, Color.cyan);
        XpGemDefinition green = TheCircussyOneTestObjects.CreateXpGemDefinition("green_xp_gem", "Green XP Gem", 23, Color.green);
        XpGemCatalog gems = TheCircussyOneTestObjects.CreateXpGemCatalog(blue, green);
        EnemyDefinition small = TheCircussyOneTestObjects.CreateEnemyDefinition(config);
        EnemyDefinition large = TheCircussyOneTestObjects.CreateEnemyDefinition(config);
        small.displayName = "Small";
        small.enemyId = "small";
        small.spawnWeight = 1f;
        small.xpBudget = 7;
        large.displayName = "Large";
        large.enemyId = "large";
        large.spawnWeight = 3f;
        large.xpBudget = 30;
        EnemyCatalog enemies = TheCircussyOneTestObjects.CreateEnemyCatalog(small, large);

        XpPacingProjectionResult result = XpPacingProjectionRules.Project(
            config,
            enemies,
            gems,
            XpPacingProjectionSettings.Default);

        Assert.That(result.WeightedXpPerKill, Is.EqualTo(24.25f).Within(0.001f));
        Assert.That(result.EnemySources.Select(source => source.ResolvedXpPerKill).ToArray(), Is.EqualTo(new[] { 7, 30 }));

        Object.DestroyImmediate(config);
        Object.DestroyImmediate(blue);
        Object.DestroyImmediate(green);
        Object.DestroyImmediate(gems);
        Object.DestroyImmediate(small);
        Object.DestroyImmediate(large);
        Object.DestroyImmediate(enemies);
    }

    [Test]
    public void KillEfficiencyChangesProjectionWithoutChangingSources()
    {
        GameConfig config = TheCircussyOneTestObjects.CreateConfig();
        XpGemDefinition blue = TheCircussyOneTestObjects.CreateXpGemDefinition("blue_xp_gem", "Blue XP Gem", 7, Color.cyan);
        XpGemCatalog gems = TheCircussyOneTestObjects.CreateXpGemCatalog(blue);
        EnemyDefinition enemy = TheCircussyOneTestObjects.CreateEnemyDefinition(config);
        enemy.xpBudget = 7;
        EnemyCatalog enemies = TheCircussyOneTestObjects.CreateEnemyCatalog(enemy);
        var settings = new XpPacingProjectionSettings(
            120f,
            new[] { 120f },
            new[] { 0.5f, 1f });

        XpPacingProjectionResult result = XpPacingProjectionRules.Project(config, enemies, gems, settings);
        XpPacingProjectionRow half = result.Rows.Single(row => Mathf.Abs(row.KillEfficiency - 0.5f) < 0.001f);
        XpPacingProjectionRow full = result.Rows.Single(row => Mathf.Abs(row.KillEfficiency - 1f) < 0.001f);

        Assert.That(half.SpawnUpperBound, Is.EqualTo(full.SpawnUpperBound));
        Assert.That(half.EstimatedKills, Is.LessThan(full.EstimatedKills));
        Assert.That(half.TotalXp, Is.LessThan(full.TotalXp));
        Assert.That(result.EnemySources.Single().XpBudget, Is.EqualTo(7));

        Object.DestroyImmediate(config);
        Object.DestroyImmediate(blue);
        Object.DestroyImmediate(gems);
        Object.DestroyImmediate(enemy);
        Object.DestroyImmediate(enemies);
    }

    [Test]
    public void MissingOrEmptyCatalogsProduceWarningsInsteadOfThrowing()
    {
        GameConfig config = TheCircussyOneTestObjects.CreateConfig();

        Assert.DoesNotThrow(() =>
        {
            XpPacingProjectionResult result = XpPacingProjectionRules.Project(
                config,
                enemyCatalog: null,
                xpGemCatalog: null,
                XpPacingProjectionSettings.Default);

            Assert.That(result.Warnings, Is.Not.Empty);
            Assert.That(result.WeightedXpPerKill, Is.Zero);
        });

        Object.DestroyImmediate(config);
    }

    [Test]
    public void AssetBackedCalculatorLoadsCurrentCanonicalContent()
    {
        XpPacingProjectionResult result = XpPacingCalculatorModel.BuildFromAssets();
        XpPacingProjectionRow twoMinuteUpperBound = XpPacingCalculatorModel.FindRow(result, 120f, 1f);

        Assert.That(result.EnemySources.Any(source => source.DisplayName == "Audience Member"), Is.True);
        Assert.That(result.WeightedXpPerKill, Is.EqualTo(7f).Within(0.001f));
        Assert.That(twoMinuteUpperBound.ProjectedLevel, Is.InRange(6, 8));
        Assert.That(typeof(XpPacingCalculatorWindow), Is.Not.Null);
        Assert.That(typeof(GameConfigEditor), Is.Not.Null);
        Assert.That(typeof(TheCircussyOneConfigHub), Is.Not.Null);
    }
}
