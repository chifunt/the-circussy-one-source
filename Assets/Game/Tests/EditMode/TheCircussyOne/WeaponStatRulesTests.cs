using NUnit.Framework;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Runtime;
using TheCircussyOne.Stats;

public sealed class WeaponStatRulesTests
{
    private GameConfig _config;
    private WeaponDefinition _weapon;

    [SetUp]
    public void SetUp()
    {
        _config = TheCircussyOneTestObjects.CreateConfig();
        _weapon = TheCircussyOneTestObjects.CreateWeaponDefinition(_config);
    }

    [TearDown]
    public void TearDown()
    {
        TheCircussyOneTestObjects.Destroy(_weapon);
        TheCircussyOneTestObjects.Destroy(_config);
    }

    [Test]
    public void DefaultsMatchWeaponDefinitionAndWeaponLevelDoesNotScaleFireRate()
    {
        var stats = new RunStats(_config);

        Assert.That(WeaponStatRules.Damage(_weapon, stats), Is.EqualTo(_weapon.projectileDamage));
        Assert.That(WeaponStatRules.ProjectileSpeed(_weapon, stats), Is.EqualTo(_weapon.projectileSpeed));
        Assert.That(WeaponStatRules.FireInterval(_weapon, stats), Is.EqualTo(_weapon.baseFireIntervalSeconds));
        var leveledWeapon = new WeaponRuntime(_weapon, 5);

        Assert.That(leveledWeapon.Level, Is.EqualTo(5));
        Assert.That(
            WeaponStatRules.FireInterval(leveledWeapon.Definition, stats, leveledWeapon.Modifiers),
            Is.EqualTo(_weapon.baseFireIntervalSeconds).Within(0.0001f));
    }

    [Test]
    public void DamageSpeedAndAttackSpeedModifiersAffectNewWeaponEvaluations()
    {
        _weapon.projectileDamage = 10;
        _weapon.projectileSpeed = 20f;
        _weapon.baseFireIntervalSeconds = 0.5f;
        _weapon.minimumFireIntervalSeconds = 0.1f;
        var stats = new RunStats(_config);
        stats.AddModifier(StatModifier.AdditivePercent(StatId.GlobalDamageMultiplier, 0.5f, "damage"));
        stats.AddModifier(StatModifier.AdditivePercent(StatId.ProjectileSpeedMultiplier, 0.25f, "speed"));
        stats.AddModifier(StatModifier.AttackSpeedBonus(StatId.GlobalWeaponHaste, 1f, "attack_speed"));

        Assert.That(WeaponStatRules.Damage(_weapon, stats), Is.EqualTo(15));
        Assert.That(WeaponStatRules.ProjectileSpeed(_weapon, stats), Is.EqualTo(25f));
        Assert.That(WeaponStatRules.FireInterval(_weapon, stats), Is.EqualTo(0.25f).Within(0.0001f));
    }

    [Test]
    public void WeaponLocalModifiersAffectOnlyTheTargetWeapon()
    {
        _weapon.projectileDamage = 10;
        _weapon.projectileSpeed = 20f;
        _weapon.projectileHitRadius = 0.5f;
        _weapon.baseFireIntervalSeconds = 0.5f;
        _weapon.minimumFireIntervalSeconds = 0.1f;
        var stats = new RunStats(_config);
        var upgraded = new WeaponRuntime(_weapon, 1);
        var plain = new WeaponRuntime(_weapon, 1);

        upgraded.AddModifier(StatModifier.Flat(StatId.WeaponFlatDamage, 5f, "weapon:damage"));
        upgraded.AddModifier(StatModifier.AdditivePercent(StatId.WeaponProjectileSpeedMultiplier, 0.25f, "weapon:speed"));
        upgraded.AddModifier(StatModifier.AttackSpeedBonus(StatId.WeaponAttackSpeed, 1f, "weapon:attack_speed"));
        upgraded.AddModifier(StatModifier.AdditivePercent(StatId.WeaponProjectileSizeMultiplier, 0.5f, "weapon:size"));

        Assert.That(WeaponStatRules.Damage(upgraded.Definition, stats, upgraded.Modifiers), Is.EqualTo(15));
        Assert.That(WeaponStatRules.ProjectileSpeed(upgraded.Definition, stats, upgraded.Modifiers), Is.EqualTo(25f));
        Assert.That(WeaponStatRules.ProjectileHitRadius(upgraded.Definition, stats, upgraded.Modifiers), Is.EqualTo(0.75f).Within(0.0001f));
        Assert.That(WeaponStatRules.FireInterval(upgraded.Definition, stats, upgraded.Modifiers), Is.EqualTo(0.25f).Within(0.0001f));
        Assert.That(WeaponStatRules.Damage(plain.Definition, stats, plain.Modifiers), Is.EqualTo(10));
        Assert.That(WeaponStatRules.ProjectileSpeed(plain.Definition, stats, plain.Modifiers), Is.EqualTo(20f));
        Assert.That(WeaponStatRules.ProjectileHitRadius(plain.Definition, stats, plain.Modifiers), Is.EqualTo(0.5f).Within(0.0001f));
        Assert.That(WeaponStatRules.FireInterval(plain.Definition, stats, plain.Modifiers), Is.EqualTo(0.5f).Within(0.0001f));
    }

    [Test]
    public void FractionalWeaponDamageFloorsFinalAppliedDamage()
    {
        _weapon.projectileDamage = 10;
        var stats = new RunStats(_config);
        var fractionalFlat = new WeaponRuntime(_weapon, 1);
        fractionalFlat.AddModifier(StatModifier.Flat(StatId.WeaponFlatDamage, 0.99f, "weapon:damage"));

        Assert.That(WeaponStatRules.Damage(_weapon, stats, fractionalFlat.Modifiers), Is.EqualTo(10));

        fractionalFlat.AddModifier(StatModifier.Flat(StatId.WeaponFlatDamage, 1f, "weapon:damage_2"));
        Assert.That(WeaponStatRules.Damage(_weapon, stats, fractionalFlat.Modifiers), Is.EqualTo(11));

        stats.AddModifier(StatModifier.AdditivePercent(StatId.GlobalDamageMultiplier, 0.19f, "global:damage"));
        Assert.That(WeaponStatRules.Damage(_weapon, stats), Is.EqualTo(11));
    }

    [Test]
    public void FractionalWeaponCountsFloorBeforeClamping()
    {
        _weapon.baseProjectileCount = 1;
        _weapon.maxProjectileCount = 5;
        _weapon.bounceEnabled = true;
        _weapon.baseBounceCount = 1;
        _weapon.maxBounceCount = 5;
        _weapon.orbitEnabled = true;
        _weapon.baseOrbitCount = 2;
        _weapon.maxOrbitCount = 5;
        var stats = new RunStats(_config);
        stats.AddModifier(StatModifier.Flat(StatId.ProjectileCount, 0.99f, "global:count"));
        stats.AddModifier(StatModifier.Flat(StatId.Bounce, 0.99f, "global:bounce"));
        var fractional = new WeaponRuntime(_weapon, 1);
        fractional.AddModifier(StatModifier.Flat(StatId.WeaponProjectileCount, 1.99f, "weapon:count"));
        fractional.AddModifier(StatModifier.Flat(StatId.WeaponBounce, 1.99f, "weapon:bounce"));

        Assert.That(WeaponStatRules.ProjectileCount(_weapon, stats, fractional.Modifiers), Is.EqualTo(2));
        Assert.That(WeaponStatRules.BounceCount(_weapon, stats, fractional.Modifiers), Is.EqualTo(2));
        Assert.That(WeaponStatRules.OrbitCount(_weapon, stats, fractional.Modifiers), Is.EqualTo(3));
    }

    [Test]
    public void CompatibilityWeaponMultipliersStillEvaluateAsWeaponLocalButLegacyHasteIsIgnored()
    {
        _weapon.projectileDamage = 10;
        _weapon.projectileSpeed = 20f;
        _weapon.baseFireIntervalSeconds = 0.5f;
        _weapon.minimumFireIntervalSeconds = 0.1f;
        var stats = new RunStats(_config);
        var upgraded = new WeaponRuntime(_weapon, 1);

        upgraded.AddModifier(StatModifier.AdditivePercent(StatId.WeaponDamageMultiplier, 0.5f, "legacy:weapon_damage_percent"));
        upgraded.AddModifier(StatModifier.AdditivePercent(StatId.ProjectileSpeedMultiplier, 0.25f, "legacy:speed"));
        upgraded.AddModifier(new StatModifier(StatId.GlobalWeaponHaste, StatModifierBucket.Haste, 1f, "legacy:haste"));

        Assert.That(WeaponStatRules.Damage(upgraded.Definition, stats, upgraded.Modifiers), Is.EqualTo(15));
        Assert.That(WeaponStatRules.ProjectileSpeed(upgraded.Definition, stats, upgraded.Modifiers), Is.EqualTo(25f));
        Assert.That(WeaponStatRules.FireInterval(upgraded.Definition, stats, upgraded.Modifiers), Is.EqualTo(0.5f).Within(0.0001f));
    }

    [Test]
    public void ProjectileSizeLifetimeAndRangeUseWeaponLocalModifiers()
    {
        _weapon.projectileHitRadius = 0.5f;
        _weapon.projectileLifetimeSeconds = 2f;
        _weapon.weaponRange = 8f;
        var stats = new RunStats(_config);
        var upgraded = new WeaponRuntime(_weapon, 1);

        upgraded.AddModifier(StatModifier.AdditivePercent(StatId.WeaponProjectileSizeMultiplier, 0.5f, "weapon:size"));
        upgraded.AddModifier(StatModifier.AdditivePercent(StatId.WeaponProjectileLifetimeMultiplier, 0.25f, "weapon:lifetime"));
        upgraded.AddModifier(StatModifier.AdditivePercent(StatId.WeaponRangeMultiplier, 0.5f, "weapon:range"));

        Assert.That(WeaponStatRules.ProjectileHitRadius(_weapon, stats, upgraded.Modifiers), Is.EqualTo(0.75f).Within(0.0001f));
        Assert.That(WeaponStatRules.ProjectileVisualScaleMultiplier(stats, upgraded.Modifiers), Is.EqualTo(1.5f).Within(0.0001f));
        Assert.That(WeaponStatRules.ProjectileLifetime(_weapon, stats, upgraded.Modifiers), Is.EqualTo(2.5f).Within(0.0001f));
        Assert.That(WeaponStatRules.Range(_weapon, upgraded.Modifiers), Is.EqualTo(12f).Within(0.0001f));
    }

    [Test]
    public void AccuracyReducesAimErrorButDoesNotChangePatternSpread()
    {
        _weapon.baseSpreadAngleDegrees = 18f;
        _weapon.baseAimErrorDegrees = 8f;
        _weapon.accuracyAffectsAimError = true;
        var stats = new RunStats(_config);
        var upgraded = new WeaponRuntime(_weapon, 1);
        upgraded.AddModifier(StatModifier.AdditivePercent(StatId.WeaponAccuracyMultiplier, 1f, "weapon:accuracy"));

        Assert.That(WeaponStatRules.SpreadAngle(_weapon, stats, upgraded.Modifiers), Is.EqualTo(18f).Within(0.0001f));
        Assert.That(WeaponStatRules.AimErrorAngle(_weapon, stats, upgraded.Modifiers), Is.EqualTo(4f).Within(0.0001f));
    }

    [Test]
    public void AimErrorIsZeroForWeaponsWithoutAimError()
    {
        _weapon.baseAimErrorDegrees = 0f;
        var stats = new RunStats(_config);
        var upgraded = new WeaponRuntime(_weapon, 1);
        upgraded.AddModifier(StatModifier.AdditivePercent(StatId.WeaponAccuracyMultiplier, 4f, "weapon:accuracy"));

        Assert.That(WeaponStatRules.AimErrorAngle(_weapon, stats, upgraded.Modifiers), Is.Zero);
    }

    [Test]
    public void BounceCountUsesGlobalAndWeaponLocalModifiersWhenEnabled()
    {
        _weapon.bounceEnabled = true;
        _weapon.baseBounceCount = 1;
        _weapon.maxBounceCount = 4;
        var stats = new RunStats(_config);
        stats.AddModifier(StatModifier.Flat(StatId.Bounce, 1f, "global:bounce"));
        var upgraded = new WeaponRuntime(_weapon, 1);
        upgraded.AddModifier(StatModifier.Flat(StatId.WeaponBounce, 3f, "weapon:bounce"));

        Assert.That(WeaponStatRules.BounceCount(_weapon, stats, upgraded.Modifiers), Is.EqualTo(4));
    }

    [Test]
    public void BounceCountIsZeroForNonBounceWeapons()
    {
        _weapon.bounceEnabled = false;
        _weapon.baseBounceCount = 1;
        _weapon.maxBounceCount = 4;
        var stats = new RunStats(_config);
        stats.AddModifier(StatModifier.Flat(StatId.Bounce, 3f, "global:bounce"));

        Assert.That(WeaponStatRules.BounceCount(_weapon, stats), Is.Zero);
    }

    [Test]
    public void ChainCountUsesGlobalAndWeaponLocalModifiersWhenEnabled()
    {
        _weapon.chainEnabled = true;
        _weapon.baseChainCount = 2;
        _weapon.maxChainCount = 5;
        var stats = new RunStats(_config);
        stats.AddModifier(StatModifier.Flat(StatId.Chain, 1f, "global:chain"));
        var upgraded = new WeaponRuntime(_weapon, 1);
        upgraded.AddModifier(StatModifier.Flat(StatId.WeaponChain, 4f, "weapon:chain"));

        Assert.That(WeaponStatRules.ChainCount(_weapon, stats, upgraded.Modifiers), Is.EqualTo(5));
    }

    [Test]
    public void ChainCountIsZeroForNonChainWeapons()
    {
        _weapon.chainEnabled = false;
        _weapon.baseChainCount = 2;
        _weapon.maxChainCount = 5;
        var stats = new RunStats(_config);
        stats.AddModifier(StatModifier.Flat(StatId.Chain, 3f, "global:chain"));

        Assert.That(WeaponStatRules.ChainCount(_weapon, stats), Is.Zero);
    }

    [Test]
    public void SplashRadiusUsesWeaponLocalModifiersAndClampsWhenExplosive()
    {
        _weapon.explosiveEnabled = true;
        _weapon.baseSplashRadius = 2f;
        _weapon.maxSplashRadius = 3f;
        var stats = new RunStats(_config);
        var upgraded = new WeaponRuntime(_weapon, 1);
        upgraded.AddModifier(StatModifier.AdditivePercent(StatId.WeaponSplashRadiusMultiplier, 1f, "weapon:splash"));

        Assert.That(WeaponStatRules.SplashRadius(_weapon, stats, upgraded.Modifiers), Is.EqualTo(3f).Within(0.0001f));
    }

    [Test]
    public void SplashRadiusIsZeroForNonExplosiveWeapons()
    {
        _weapon.explosiveEnabled = false;
        _weapon.baseSplashRadius = 2f;
        _weapon.maxSplashRadius = 4f;
        var stats = new RunStats(_config);
        stats.AddModifier(StatModifier.AdditivePercent(StatId.WeaponSplashRadiusMultiplier, 1f, "global:splash"));

        Assert.That(WeaponStatRules.SplashRadius(_weapon, stats), Is.Zero);
    }

    [Test]
    public void OrbitStatsUseWeaponLocalModifiersAndClampWhenEnabled()
    {
        _weapon.orbitEnabled = true;
        _weapon.baseOrbitCount = 1;
        _weapon.maxOrbitCount = 4;
        _weapon.orbitRadius = 2f;
        _weapon.maxOrbitRadius = 3f;
        _weapon.orbitHitRadius = 0.5f;
        _weapon.maxOrbitHitRadius = 1f;
        var stats = new RunStats(_config);
        var upgraded = new WeaponRuntime(_weapon, 1);
        upgraded.AddModifier(StatModifier.Flat(StatId.WeaponProjectileCount, 5f, "weapon:count"));
        upgraded.AddModifier(StatModifier.AdditivePercent(StatId.WeaponRangeMultiplier, 1f, "weapon:range"));
        upgraded.AddModifier(StatModifier.AdditivePercent(StatId.WeaponProjectileSizeMultiplier, 2f, "weapon:size"));

        Assert.That(WeaponStatRules.OrbitCount(_weapon, stats, upgraded.Modifiers), Is.EqualTo(4));
        Assert.That(WeaponStatRules.OrbitRadius(_weapon, upgraded.Modifiers), Is.EqualTo(3f).Within(0.0001f));
        Assert.That(WeaponStatRules.OrbitHitRadius(_weapon, stats, upgraded.Modifiers), Is.EqualTo(1f).Within(0.0001f));
    }

    [Test]
    public void OrbitAreaDamageUsesFlatWeaponDamageAndMinimumOne()
    {
        _weapon.orbitEnabled = true;
        _weapon.orbitAreaDamageEnabled = true;
        _weapon.projectileDamage = 7;
        _weapon.orbitAreaDamageMultiplier = 0.55f;
        var stats = new RunStats(_config);
        var upgraded = new WeaponRuntime(_weapon, 1);
        upgraded.AddModifier(StatModifier.Flat(StatId.WeaponFlatDamage, 7f, "weapon:damage"));

        Assert.That(WeaponStatRules.OrbitAreaDamage(_weapon, stats), Is.EqualTo(4));
        Assert.That(WeaponStatRules.OrbitAreaDamage(_weapon, stats, upgraded.Modifiers), Is.EqualTo(8));

        _weapon.projectileDamage = 1;
        _weapon.orbitAreaDamageMultiplier = 0.2f;
        Assert.That(WeaponStatRules.OrbitAreaDamage(_weapon, stats), Is.EqualTo(1));
    }

    [Test]
    public void OrbitAreaHitIntervalUsesAttackSpeedAndMinimum()
    {
        _weapon.orbitEnabled = true;
        _weapon.orbitAreaDamageEnabled = true;
        _weapon.orbitAreaHitIntervalSeconds = 0.45f;
        _weapon.minimumFireIntervalSeconds = 0.12f;
        var stats = new RunStats(_config);
        var upgraded = new WeaponRuntime(_weapon, 1);
        upgraded.AddModifier(StatModifier.AttackSpeedBonus(StatId.WeaponAttackSpeed, 1f, "weapon:attack_speed"));

        Assert.That(WeaponStatRules.OrbitAreaHitInterval(_weapon, stats), Is.EqualTo(0.45f).Within(0.0001f));
        Assert.That(WeaponStatRules.OrbitAreaHitInterval(_weapon, stats, upgraded.Modifiers), Is.EqualTo(0.225f).Within(0.0001f));

        _weapon.orbitAreaHitIntervalSeconds = 0.01f;
        Assert.That(WeaponStatRules.OrbitAreaHitInterval(_weapon, stats), Is.EqualTo(0.12f).Within(0.0001f));
    }

    [Test]
    public void OrbitStatsAreZeroForNonOrbitWeapons()
    {
        _weapon.orbitEnabled = false;
        _weapon.baseOrbitCount = 1;
        _weapon.orbitRadius = 2f;
        _weapon.orbitHitRadius = 0.5f;
        var stats = new RunStats(_config);
        stats.AddModifier(StatModifier.Flat(StatId.ProjectileCount, 3f, "global:count"));

        Assert.That(WeaponStatRules.OrbitCount(_weapon, stats), Is.Zero);
        Assert.That(WeaponStatRules.OrbitRadius(_weapon), Is.Zero);
        Assert.That(WeaponStatRules.OrbitHitRadius(_weapon, stats), Is.Zero);
        Assert.That(WeaponStatRules.OrbitAreaDamage(_weapon, stats), Is.Zero);
    }
}
