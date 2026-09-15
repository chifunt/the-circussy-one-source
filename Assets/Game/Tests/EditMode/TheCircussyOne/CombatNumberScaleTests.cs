using NUnit.Framework;
using UnityEditor;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Stats;

public sealed class CombatNumberScaleTests
{
    [Test]
    public void FirstPartyWeaponDamageMatchesAuthoredPresentationScale()
    {
        AssertWeaponDamage(TheCircussyOneAssetPaths.KnifeFanWeaponPath, FirstPartyWeaponDefaults.KnifeFanId, 10);
        AssertWeaponDamage(TheCircussyOneAssetPaths.SpotlightBoltWeaponPath, FirstPartyWeaponDefaults.SpotlightBoltId, 42);
        AssertWeaponDamage(TheCircussyOneAssetPaths.JugglingBallWeaponPath, FirstPartyWeaponDefaults.JugglingBallId, 51);
        AssertWeaponDamage(TheCircussyOneAssetPaths.FireHoopWeaponPath, FirstPartyWeaponDefaults.FireHoopId, 55);
        AssertWeaponDamage(TheCircussyOneAssetPaths.CannonWeaponPath, FirstPartyWeaponDefaults.CannonId, 132);
    }

    [Test]
    public void FlatWeaponDamageUpgradesUseTenTimesCombatScale()
    {
        AssertFlatWeaponDamageUpgrade(TheCircussyOneAssetPaths.KnifeFanHonedBladesUpgradePath, 10f);
        AssertFlatWeaponDamageUpgrade(TheCircussyOneAssetPaths.SpotlightBoltBrightChargeUpgradePath, 10f);
        AssertFlatWeaponDamageUpgrade(TheCircussyOneAssetPaths.JugglingBallWeightedBallUpgradePath, 10f);
        AssertFlatWeaponDamageUpgrade(TheCircussyOneAssetPaths.FireHoopTuningUpgradePath, 10f);
        AssertFlatWeaponDamageUpgrade(TheCircussyOneAssetPaths.CannonPayloadUpgradePath, 10f);
    }

    [Test]
    public void AuthoredEnemyHealthUsesTenTimesCombatScaleWithoutScalingContactDamage()
    {
        GameConfig config = Load<GameConfig>(TheCircussyOneAssetPaths.GameConfigPath);
        EnemyDefinition normal = Load<EnemyDefinition>(TheCircussyOneAssetPaths.NormalEnemyPath);
        EnemyDefinition headliner = Load<EnemyDefinition>(TheCircussyOneAssetPaths.OpeningHeadlinerEnemyPath);

        Assert.That(config.enemyHealth, Is.EqualTo(170));
        Assert.That(config.enemyHealthGrowthPerMinute, Is.EqualTo(20f));
        Assert.That(normal.baseHealth, Is.EqualTo(170));
        Assert.That(normal.healthGrowthPerMinute, Is.EqualTo(30f));
        Assert.That(normal.contactDamage, Is.EqualTo(4));
        Assert.That(headliner.baseHealth, Is.EqualTo(3250));
        Assert.That(headliner.healthGrowthPerMinute, Is.EqualTo(160f));
        Assert.That(headliner.contactDamage, Is.EqualTo(21));
    }

    private static void AssertWeaponDamage(string path, string weaponId, int expectedDamage)
    {
        WeaponDefinition weapon = Load<WeaponDefinition>(path);
        Assert.That(weapon.projectileDamage, Is.EqualTo(expectedDamage), path);
        Assert.That(FirstPartyWeaponDefaults.GetOrDefault(weaponId).ProjectileDamage, Is.EqualTo(expectedDamage), weaponId);
    }

    private static void AssertFlatWeaponDamageUpgrade(string path, float expectedValue)
    {
        UpgradeDefinition upgrade = Load<UpgradeDefinition>(path);
        Assert.That(upgrade.statModifiers, Has.Count.EqualTo(1), path);
        Assert.That(upgrade.statModifiers[0].statId, Is.EqualTo(StatId.WeaponFlatDamage), path);
        Assert.That(upgrade.statModifiers[0].bucket, Is.EqualTo(StatModifierBucket.Flat), path);
        Assert.That(upgrade.statModifiers[0].ValueFor(ContentRarity.Common), Is.EqualTo(expectedValue), path);
    }

    private static T Load<T>(string path)
        where T : UnityEngine.Object
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        Assert.That(asset, Is.Not.Null, path);
        return asset;
    }
}
