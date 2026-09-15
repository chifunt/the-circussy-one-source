using NUnit.Framework;
using TheCircussyOne.Config;
using TheCircussyOne.Runtime;
using TheCircussyOne.Stats;

public sealed class RunStatsTests
{
    private GameConfig _config;

    [SetUp]
    public void SetUp()
    {
        _config = TheCircussyOneTestObjects.CreateConfig();
    }

    [TearDown]
    public void TearDown()
    {
        TheCircussyOneTestObjects.Destroy(_config);
    }

    [Test]
    public void DefaultsMatchGameConfigAndDoNotGrantLevelSpeed()
    {
        var stats = new RunStats(_config);

        Assert.That(stats.GetInt(StatId.PlayerMaxHealth), Is.EqualTo(_config.playerMaxHealth));
        Assert.That(stats.GetFloat(StatId.PlayerArmor), Is.Zero);
        Assert.That(stats.GetFloat(StatId.PickupMagnetRadius), Is.EqualTo(_config.pickupMagnetRadius));
        Assert.That(stats.GetFloat(StatId.PickupCollectRadius), Is.EqualTo(_config.pickupCollectRadius));
        Assert.That(stats.GetFloat(StatId.XpGainMultiplier), Is.EqualTo(1f));
        Assert.That(stats.GetFloat(StatId.Luck), Is.EqualTo(100f));
        Assert.That(stats.GetFloat(StatId.CritChance), Is.Zero);
        Assert.That(stats.GetFloat(StatId.CritDamageMultiplier), Is.EqualTo(2f));
        Assert.That(stats.GetFloat(StatId.ProjectileAreaMultiplier), Is.EqualTo(1f));
        Assert.That(stats.GetFloat(StatId.ProjectileDurationMultiplier), Is.EqualTo(1f));
        Assert.That(stats.GetFloat(StatId.PlayerMoveSpeedMultiplier), Is.EqualTo(1f));
        Assert.That(stats.GetFloat(StatId.PlayerHpRegenPerMinute), Is.Zero);
        Assert.That(stats.GetFloat(StatId.PlayerLifestealChance), Is.Zero);
        Assert.That(stats.GetFloat(StatId.PlayerKnockbackMultiplier), Is.EqualTo(1f));
        Assert.That(stats.GetInt(StatId.PlayerExtraJumps), Is.Zero);
        Assert.That(stats.GetFloat(StatId.WeaponDamageMultiplier), Is.EqualTo(1f));
        Assert.That(stats.GetInt(StatId.WeaponFlatDamage), Is.Zero);
        Assert.That(stats.GetFloat(StatId.WeaponAttackSpeed), Is.Zero);
        Assert.That(stats.GetFloat(StatId.WeaponProjectileSizeMultiplier), Is.EqualTo(1f));

        stats.SetLevel(4);

        Assert.That(stats.GetFloat(StatId.PlayerMoveSpeedMultiplier), Is.EqualTo(1f).Within(0.0001f));
    }

    [Test]
    public void RemovingModifierSourceRevertsAffectedStats()
    {
        var stats = new RunStats(_config);
        stats.AddModifier(StatModifier.Flat(StatId.PickupMagnetRadius, 2f, "test_item"));

        Assert.That(stats.GetFloat(StatId.PickupMagnetRadius), Is.EqualTo(_config.pickupMagnetRadius + 2f));

        stats.RemoveSource("test_item");

        Assert.That(stats.GetFloat(StatId.PickupMagnetRadius), Is.EqualTo(_config.pickupMagnetRadius));
    }

    [Test]
    public void BreakdownSnapshotsIncludeEveryStatAndMatchEvaluatedValues()
    {
        var stats = new RunStats(_config);
        stats.AddModifier(StatModifier.Flat(StatId.PlayerArmor, 10f, "armor"));

        var all = stats.GetAllBreakdowns();

        Assert.That(all, Has.Count.EqualTo(StatMetadata.All.Count));
        foreach (StatBreakdown breakdown in all)
        {
            if (breakdown.ValueKind == StatValueKind.Integer)
            {
                Assert.That(StatRules.FloorToAppliedInt(breakdown.FinalValue), Is.EqualTo(stats.GetInt(breakdown.Id)), breakdown.DisplayName);
            }
            else
            {
                Assert.That(breakdown.FinalValue, Is.EqualTo(stats.GetFloat(breakdown.Id)).Within(0.0001f), breakdown.DisplayName);
            }
        }
    }

    [Test]
    public void BreakdownFiltersModifiersByTargetStat()
    {
        var stats = new RunStats(_config);
        stats.AddModifier(StatModifier.Flat(StatId.PlayerArmor, 10f, "armor"));
        stats.AddModifier(StatModifier.Flat(StatId.PickupMagnetRadius, 3f, "magnet"));

        StatBreakdown armor = stats.GetBreakdown(StatId.PlayerArmor);

        Assert.That(armor.Modifiers.Count, Is.EqualTo(1));
        Assert.That(armor.Modifiers[0].SourceId, Is.EqualTo("armor"));
        Assert.That(armor.FinalValue, Is.EqualTo(10f));
    }

    [Test]
    public void BreakdownReportsClampedFinalValues()
    {
        var stats = new RunStats(_config);
        stats.AddModifier(StatModifier.Flat(StatId.CritChance, 2f, "crit"));

        StatBreakdown crit = stats.GetBreakdown(StatId.CritChance);

        Assert.That(crit.FinalValue, Is.EqualTo(1f));
        Assert.That(crit.MinValue, Is.EqualTo(0f));
        Assert.That(crit.MaxValue, Is.EqualTo(1f));
    }

    [Test]
    public void GameStateUsesStatsForMaxHealthWithoutLevelSpeedBonus()
    {
        var stats = new RunStats(_config);
        stats.AddModifier(StatModifier.Flat(StatId.PlayerMaxHealth, 25f, "heart"));
        var state = new GameState(_config, stats);

        Assert.That(state.Health, Is.EqualTo(_config.playerMaxHealth + 25));
        Assert.That(state.MaxHealth, Is.EqualTo(_config.playerMaxHealth + 25));

        state.AddExperience(state.ExperienceTarget);

        Assert.That(state.MoveSpeedMultiplier, Is.EqualTo(1f).Within(0.0001f));
    }
}
