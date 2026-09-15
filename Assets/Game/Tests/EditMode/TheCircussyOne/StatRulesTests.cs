using NUnit.Framework;
using TheCircussyOne.Stats;

public sealed class StatRulesTests
{
    [Test]
    public void DamageBracketsEvaluateInOrder()
    {
        var modifiers = new[]
        {
            new StatModifier(StatId.GlobalDamageMultiplier, StatModifierBucket.Flat, 2f, "base_bonus"),
            new StatModifier(StatId.GlobalDamageMultiplier, StatModifierBucket.WeaponLocalPercent, 0.5f, "weapon"),
            new StatModifier(StatId.GlobalDamageMultiplier, StatModifierBucket.GlobalPercent, 0.25f, "global"),
            new StatModifier(StatId.GlobalDamageMultiplier, StatModifierBucket.TagSpecializationPercent, 0.2f, "tag"),
            new StatModifier(StatId.GlobalDamageMultiplier, StatModifierBucket.ConditionalPercent, 0.1f, "conditional"),
            new StatModifier(StatId.GlobalDamageMultiplier, StatModifierBucket.RareUniqueMultiplier, 2f, "rare")
        };

        float damage = StatRules.EvaluateDamage(10f, modifiers);

        Assert.That(damage, Is.EqualTo(((10f + 2f) * 1.5f * 1.25f * 1.2f * 1.1f * 2f)).Within(0.0001f));
    }

    [Test]
    public void CooldownAttackSpeedBonusPreservesBaseAndRespectsMinimum()
    {
        Assert.That(StatRules.EvaluateCooldown(0.5f, 0f, 0f, 0.12f), Is.EqualTo(0.5f));
        Assert.That(StatRules.EvaluateCooldown(0.5f, -0.1f, 1f, 0.12f), Is.EqualTo(0.2f).Within(0.0001f));
        Assert.That(StatRules.EvaluateCooldown(0.2f, -0.1f, 4f, 0.12f), Is.EqualTo(0.12f));
    }

    [Test]
    public void IntegerStatsAddFlatValuesAndClamp()
    {
        StatDefinition definition = StatMetadata.Get(StatId.Pierce);
        var modifiers = new[]
        {
            StatModifier.Flat(StatId.Pierce, 2f, "item"),
            StatModifier.Flat(StatId.Pierce, 1f, "upgrade")
        };

        Assert.That(StatRules.EvaluateInt(definition, 0f, modifiers), Is.EqualTo(3));
        Assert.That(StatRules.EvaluateInt(definition, 98f, modifiers), Is.EqualTo(99));
    }

    [Test]
    public void IntegerStatsFloorFractionalFinalValues()
    {
        StatDefinition jumps = StatMetadata.Get(StatId.PlayerExtraJumps);
        StatDefinition flatDamage = StatMetadata.Get(StatId.WeaponFlatDamage);

        Assert.That(StatRules.EvaluateInt(jumps, 0f, new[] { StatModifier.Flat(StatId.PlayerExtraJumps, 0.99f, "fraction") }), Is.Zero);
        Assert.That(StatRules.EvaluateInt(jumps, 0f, new[] { StatModifier.Flat(StatId.PlayerExtraJumps, 1f, "whole") }), Is.EqualTo(1));
        Assert.That(StatRules.EvaluateInt(jumps, 0f, new[] { StatModifier.Flat(StatId.PlayerExtraJumps, 1.99f, "fraction") }), Is.EqualTo(1));
        Assert.That(StatRules.EvaluateInt(flatDamage, 0f, new[] { StatModifier.Flat(StatId.WeaponFlatDamage, 2.75f, "fraction") }), Is.EqualTo(2));
    }

    [Test]
    public void AdditivePercentStatsPreserveBaseWithNoModifiers()
    {
        StatDefinition definition = StatMetadata.Get(StatId.ProjectileSpeedMultiplier);

        Assert.That(StatRules.EvaluateFloat(definition, 1f, System.Array.Empty<StatModifier>()), Is.EqualTo(1f));
    }

    [Test]
    public void DeprecatedHasteBucketDoesNotAffectAttackSpeedStats()
    {
        StatDefinition definition = StatMetadata.Get(StatId.GlobalWeaponHaste);
        var modifiers = new[]
        {
            new StatModifier(StatId.GlobalWeaponHaste, StatModifierBucket.Haste, 1f, "legacy:haste"),
            StatModifier.AdditivePercent(StatId.GlobalWeaponHaste, 0.25f, "attack_speed")
        };

        Assert.That(StatRules.EvaluateFloat(definition, 0f, modifiers), Is.EqualTo(0.25f).Within(0.0001f));
    }

    [Test]
    public void EveryStatHasValidMetadata()
    {
        foreach (StatId id in System.Enum.GetValues(typeof(StatId)))
        {
            StatDefinition definition = StatMetadata.Get(id);

            Assert.That(definition.DisplayName, Is.Not.Empty, id.ToString());
            Assert.That(definition.Category, Is.Not.Empty, id.ToString());
            Assert.That(definition.MinValue, Is.LessThanOrEqualTo(definition.MaxValue), id.ToString());
        }
    }

    [Test]
    public void CanonicalPlayerAndWeaponStatsHaveExpectedDisplayNames()
    {
        Assert.That(StatMetadata.Get(StatId.PlayerMaxHealth).DisplayName, Is.EqualTo("Max HP"));
        Assert.That(StatMetadata.Get(StatId.PlayerHpRegenPerMinute).DisplayName, Is.EqualTo("HP Regen"));
        Assert.That(StatMetadata.Get(StatId.PlayerLifestealChance).DisplayName, Is.EqualTo("Lifesteal"));
        Assert.That(StatMetadata.Get(StatId.GlobalDamageMultiplier).DisplayName, Is.EqualTo("Damage"));
        Assert.That(StatMetadata.Get(StatId.GlobalWeaponHaste).DisplayName, Is.EqualTo("Attack Speed"));
        Assert.That(StatMetadata.Get(StatId.ProjectileAreaMultiplier).DisplayName, Is.EqualTo("Projectile Size"));
        Assert.That(StatMetadata.Get(StatId.PickupMagnetRadius).DisplayName, Is.EqualTo("Pickup Range"));
        Assert.That(StatMetadata.Get(StatId.PlayerExtraJumps).DisplayName, Is.EqualTo("Extra Jumps"));

        Assert.That(StatMetadata.Get(StatId.WeaponDamageMultiplier).DisplayName, Is.EqualTo("Weapon Damage % (Legacy)"));
        Assert.That(StatMetadata.Get(StatId.WeaponFlatDamage).DisplayName, Is.EqualTo("Weapon Damage"));
        Assert.That(StatMetadata.Get(StatId.WeaponAttackSpeed).DisplayName, Is.EqualTo("Weapon Attack Speed"));
        Assert.That(StatMetadata.Get(StatId.WeaponProjectileCount).DisplayName, Is.EqualTo("Weapon Projectile Count"));
        Assert.That(StatMetadata.Get(StatId.WeaponProjectileSpeedMultiplier).DisplayName, Is.EqualTo("Weapon Projectile Speed"));
        Assert.That(StatMetadata.Get(StatId.WeaponProjectileSizeMultiplier).DisplayName, Is.EqualTo("Weapon Projectile Size"));
        Assert.That(StatMetadata.Get(StatId.WeaponProjectileLifetimeMultiplier).DisplayName, Is.EqualTo("Weapon Projectile Lifetime"));
        Assert.That(StatMetadata.Get(StatId.WeaponSplashRadiusMultiplier).DisplayName, Is.EqualTo("Weapon Splash Radius"));
        Assert.That(StatMetadata.Get(StatId.WeaponRangeMultiplier).DisplayName, Is.EqualTo("Weapon Range"));
        Assert.That(StatMetadata.Get(StatId.WeaponPierce).DisplayName, Is.EqualTo("Weapon Pierce"));
        Assert.That(StatMetadata.Get(StatId.WeaponBounce).DisplayName, Is.EqualTo("Weapon Bounce"));
        Assert.That(StatMetadata.Get(StatId.WeaponAccuracyMultiplier).DisplayName, Is.EqualTo("Weapon Accuracy"));
        Assert.That(StatMetadata.Get(StatId.WeaponKnockbackMultiplier).DisplayName, Is.EqualTo("Weapon Knockback"));
        Assert.That(StatMetadata.Get(StatId.WeaponChain).DisplayName, Is.EqualTo("Weapon Chain"));
    }

    [Test]
    public void NewStatMetadataClampsExpectedRanges()
    {
        Assert.That(StatRules.EvaluateFloat(StatMetadata.Get(StatId.CritChance), 0f, new[] { StatModifier.Flat(StatId.CritChance, 1.5f, "test") }), Is.EqualTo(1f));
        Assert.That(StatRules.EvaluateFloat(StatMetadata.Get(StatId.ProjectileAreaMultiplier), 1f, new[] { StatModifier.AdditivePercent(StatId.ProjectileAreaMultiplier, -2f, "test") }), Is.EqualTo(0.1f));
        Assert.That(StatRules.EvaluateInt(StatMetadata.Get(StatId.ProjectileCount), 0f, new[] { StatModifier.Flat(StatId.ProjectileCount, 99f, "test") }), Is.EqualTo(8));
        Assert.That(StatRules.EvaluateInt(StatMetadata.Get(StatId.Bounce), 0f, new[] { StatModifier.Flat(StatId.Bounce, 99f, "test") }), Is.EqualTo(20));
        Assert.That(StatRules.EvaluateInt(StatMetadata.Get(StatId.Chain), 0f, new[] { StatModifier.Flat(StatId.Chain, 99f, "test") }), Is.EqualTo(20));
        Assert.That(StatRules.EvaluateInt(StatMetadata.Get(StatId.WeaponChain), 0f, new[] { StatModifier.Flat(StatId.WeaponChain, 99f, "test") }), Is.EqualTo(20));
        Assert.That(StatRules.EvaluateInt(StatMetadata.Get(StatId.PlayerExtraJumps), 0f, new[] { StatModifier.Flat(StatId.PlayerExtraJumps, 99f, "test") }), Is.EqualTo(8));
        Assert.That(StatRules.EvaluateFloat(StatMetadata.Get(StatId.WeaponProjectileSizeMultiplier), 1f, new[] { StatModifier.AdditivePercent(StatId.WeaponProjectileSizeMultiplier, -2f, "test") }), Is.EqualTo(0.1f));
    }

    [Test]
    public void ArmorDamageReductionPreservesZeroArmorAndHasMinimumDamage()
    {
        Assert.That(StatRules.ApplyArmorDamageReduction(10, 0f), Is.EqualTo(10));
        Assert.That(StatRules.ApplyArmorDamageReduction(10, 100f), Is.EqualTo(5));
        Assert.That(StatRules.ApplyArmorDamageReduction(10, -50f), Is.EqualTo(15));
        Assert.That(StatRules.ApplyArmorDamageReduction(10, 900f), Is.EqualTo(1));
        Assert.That(StatRules.ApplyArmorDamageReduction(0, 900f), Is.Zero);
        Assert.That(StatRules.ApplyArmorDamageReduction(-5, 900f), Is.Zero);
    }

    [Test]
    public void LuckUsesOneHundredAsNeutralWithDiminishingReturns()
    {
        Assert.That(StatRules.EffectiveLuckMultiplier(0f), Is.Zero);
        Assert.That(StatRules.EffectiveLuckMultiplier(100f), Is.EqualTo(1f).Within(0.0001f));
        Assert.That(StatRules.EffectiveLuckMultiplier(200f), Is.EqualTo(4f / 3f).Within(0.0001f));
        Assert.That(StatRules.EffectiveLuckMultiplier(500f), Is.EqualTo(10f / 6f).Within(0.0001f));
    }

    [Test]
    public void FormulaHelpersEvaluateCanonicalStats()
    {
        Assert.That(StatRules.HpRegenPerSecond(30f), Is.EqualTo(0.5f).Within(0.0001f));
        Assert.That(StatRules.AttackSpeedMultiplierFromBonus(0.12f), Is.EqualTo(1.12f).Within(0.0001f));
        Assert.That(StatRules.MultiplyPositive(2f, 1.5f, 0.5f), Is.EqualTo(1.5f).Within(0.0001f));
        Assert.That(StatRules.SpreadAngle(12f, 2f), Is.EqualTo(6f).Within(0.0001f));
    }

    [Test]
    public void CanonicalStatsHaveExpectedDisplayFormatting()
    {
        Assert.That(StatDisplayRules.FormatValue(StatMetadata.Get(StatId.GlobalWeaponHaste), 0f), Is.EqualTo("100%"));
        Assert.That(StatDisplayRules.FormatValue(StatMetadata.Get(StatId.GlobalWeaponHaste), 0.12f), Is.EqualTo("112%"));
        Assert.That(StatDisplayRules.FormatValue(StatMetadata.Get(StatId.PlayerMoveSpeedMultiplier), 1.1f), Is.EqualTo("110%"));
        Assert.That(StatDisplayRules.FormatValue(StatMetadata.Get(StatId.PlayerHpRegenPerMinute), 30f), Is.EqualTo("30/min"));
        Assert.That(StatDisplayRules.FormatValue(StatMetadata.Get(StatId.WeaponFlatDamage), 1.99f), Is.EqualTo("1"));
        Assert.That(StatDisplayRules.FormatValue(StatMetadata.Get(StatId.WeaponFlatDamage), 1.99f, StatValueDisplayMode.Preview), Is.EqualTo("1.99"));
        Assert.That(StatDisplayRules.FormatModifier(StatMetadata.Get(StatId.WeaponFlatDamage), StatModifierBucket.Flat, 0.75f), Is.EqualTo("+0.75"));
    }

    [Test]
    public void AppendedStatIdsPreserveExistingSerializedValues()
    {
        Assert.That((int)StatId.PlayerMaxHealth, Is.EqualTo(0));
        Assert.That((int)StatId.PlayerArmor, Is.EqualTo(1));
        Assert.That((int)StatId.PlayerMoveSpeedMultiplier, Is.EqualTo(2));
        Assert.That((int)StatId.PickupMagnetRadius, Is.EqualTo(3));
        Assert.That((int)StatId.Luck, Is.EqualTo(6));
        Assert.That((int)StatId.GlobalDamageMultiplier, Is.EqualTo(7));
        Assert.That((int)StatId.GlobalWeaponHaste, Is.EqualTo(8));
        Assert.That((int)StatId.ProjectileSpeedMultiplier, Is.EqualTo(11));
        Assert.That((int)StatId.Chain, Is.EqualTo(17));
        Assert.That((int)StatId.PlayerHpRegenPerMinute, Is.EqualTo(18));
        Assert.That((int)StatId.WeaponDamageMultiplier, Is.EqualTo(22));
        Assert.That((int)StatId.WeaponChain, Is.EqualTo(34));
        Assert.That((int)StatId.WeaponFlatDamage, Is.EqualTo(35));
    }

    [Test]
    public void XpGainUsesMultiplierAndAccumulatesFractionalRemainder()
    {
        float remainder = 0f;

        Assert.That(StatRules.ApplyXpGain(1, 1f, ref remainder), Is.EqualTo(1));
        Assert.That(remainder, Is.EqualTo(0f));

        int total = 0;
        for (int i = 0; i < 10; i++)
        {
            total += StatRules.ApplyXpGain(1, 1.1f, ref remainder);
        }

        Assert.That(total, Is.EqualTo(11));
        Assert.That(remainder, Is.EqualTo(0f).Within(0.0001f));
    }

    [Test]
    public void XpGainIgnoresZeroAndNegativeAmounts()
    {
        float remainder = 0.75f;

        Assert.That(StatRules.ApplyXpGain(0, 2f, ref remainder), Is.Zero);
        Assert.That(StatRules.ApplyXpGain(-5, 2f, ref remainder), Is.Zero);
        Assert.That(remainder, Is.EqualTo(0.75f));
    }
}
