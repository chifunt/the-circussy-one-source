using NUnit.Framework;
using System.Linq;
using UnityEditor;
using UnityEngine;
using TheCircussyOne.Content;
using TheCircussyOne.Runtime;
using TheCircussyOne.Stats;

public sealed class WeaponDefinitionTests
{
    [Test]
    public void DefaultWeaponAppliesFirstPartyDefaults()
    {
        FirstPartyWeaponSpec spec = FirstPartyWeaponDefaults.DefaultWeapon;
        var weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
        weapon.ApplyDefaults(spec);

        Assert.That(weapon.weaponId, Is.EqualTo(spec.Id));
        Assert.That(weapon.displayName, Is.EqualTo(spec.DisplayName));
        Assert.That(weapon.Id, Is.EqualTo(weapon.weaponId));
        Assert.That(weapon.DisplayName, Is.EqualTo(weapon.displayName));
        Assert.That(weapon.Tags.HasTag(ContentTag.Projectile), Is.True);
        Assert.That(weapon.Tags.HasTag(ContentTag.Ranged), Is.True);
        Assert.That(weapon.Tags.HasTag(ContentTag.Damage), Is.True);
        Assert.That(weapon.baseFireIntervalSeconds, Is.EqualTo(spec.BaseFireIntervalSeconds));
        Assert.That(weapon.projectileDamage, Is.EqualTo(spec.ProjectileDamage));
        Assert.That(weapon.projectileSpeed, Is.EqualTo(spec.ProjectileSpeed));
        Assert.That(weapon.projectileLifetimeSeconds, Is.EqualTo(spec.ProjectileLifetimeSeconds));
        Assert.That(weapon.projectileHitRadius, Is.EqualTo(spec.ProjectileHitRadius));
        Assert.That(weapon.projectileHitMask.value, Is.EqualTo(LayerMask.GetMask(GameLayers.Enemy)));
        Assert.That(weapon.weaponRange, Is.EqualTo(spec.WeaponRange));
        Assert.That(weapon.projectileSpawnForwardOffset, Is.EqualTo(spec.ProjectileSpawnForwardOffset));
        Assert.That(weapon.projectileSpawnHeight, Is.EqualTo(spec.ProjectileSpawnHeight));

        TheCircussyOneTestObjects.Destroy(weapon);
    }

    [Test]
    public void WorkflowDefaultsPreserveValidTunedWeaponValues()
    {
        var weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
        weapon.weaponId = "tuned_weapon";
        weapon.displayName = "Tuned Weapon";
        weapon.EnsureWorkflowDefaults();
        weapon.baseFireIntervalSeconds = 0.5f;
        weapon.minimumFireIntervalSeconds = 0.2f;
        weapon.projectileDamage = 11;
        weapon.projectileSpeed = 17f;
        weapon.projectileLifetimeSeconds = 6f;
        weapon.projectileHitRadius = 0.33f;
        weapon.projectileHitMask = 1 << 1;
        weapon.weaponRange = 26f;
        weapon.projectileSpawnForwardOffset = 1.4f;
        weapon.projectileSpawnHeight = 1.1f;
        weapon.tags = ContentTagSet.With(ContentTag.Fire, ContentTag.Magic);
        weapon.requireLineOfSight = false;
        weapon.lineOfSightMask = 1 << 3;
        weapon.projectileBlockMask = 1 << 4;
        weapon.projectileVisualShape = ProjectileVisualShape.Bolt;
        weapon.projectileVisualScale = 1.7f;
        weapon.projectilePrimaryColor = Color.magenta;
        weapon.projectileTrailWidth = 0.22f;

        bool changed = weapon.EnsureWorkflowDefaults();

        Assert.That(changed, Is.False);
        Assert.That(weapon.projectileDamage, Is.EqualTo(11));
        Assert.That(weapon.projectileSpeed, Is.EqualTo(17f));
        Assert.That(weapon.weaponRange, Is.EqualTo(26f));
        Assert.That(weapon.tags.Values, Is.EquivalentTo(new[] { ContentTag.Fire, ContentTag.Magic }));
        Assert.That(weapon.Tags.HasTag(ContentTag.Fire), Is.False);
        Assert.That(weapon.Tags.HasTag(ContentTag.Magic), Is.False);
        Assert.That(weapon.Tags.HasTag(ContentTag.Projectile), Is.True);
        Assert.That(weapon.requireLineOfSight, Is.False);
        Assert.That(weapon.lineOfSightMask.value, Is.EqualTo(1 << 3));
        Assert.That(weapon.projectileBlockMask.value, Is.EqualTo(1 << 4));
        Assert.That(weapon.projectileVisualShape, Is.EqualTo(ProjectileVisualShape.Bolt));
        Assert.That(weapon.projectileVisualScale, Is.EqualTo(1.7f));
        Assert.That(weapon.projectilePrimaryColor, Is.EqualTo(Color.magenta));
        Assert.That(weapon.projectileTrailWidth, Is.EqualTo(0.22f));

        Object.DestroyImmediate(weapon);
    }

    [Test]
    public void WorkflowDefaultsSeedProjectileCollisionAndDistinctVisualIdentity()
    {
        WeaponDefinition defaultWeapon = CreateTestWeapon("juggling_ball", "Juggling Ball");
        WeaponDefinition cannon = CreateTestWeapon("cannon", "Cannon");
        WeaponDefinition knife = CreateTestWeapon("knife_fan", "Knife Fan");
        WeaponDefinition spotlight = CreateTestWeapon("spotlight_bolt", "Spotlight Bolt");

        Assert.That(defaultWeapon.requireLineOfSight, Is.True);
        Assert.That(defaultWeapon.lineOfSightMask.value, Is.EqualTo(GameLayers.EnvironmentMaskExcludingGameplay));
        Assert.That(defaultWeapon.projectileBlockMask.value, Is.EqualTo(GameLayers.EnvironmentMaskExcludingGameplay));
        Assert.That(defaultWeapon.projectileBlockRadius, Is.EqualTo(0.16f).Within(0.0001f));
        Assert.That(defaultWeapon.projectileHitMask.value, Is.EqualTo(LayerMask.GetMask(GameLayers.Enemy)));
        Assert.That(defaultWeapon.projectileVisualShape, Is.EqualTo(ProjectileVisualShape.Sphere));
        Assert.That(cannon.projectileVisualShape, Is.EqualTo(ProjectileVisualShape.Sphere));
        Assert.That(cannon.projectileTrajectoryMode, Is.EqualTo(ProjectileTrajectoryMode.Arc));
        Assert.That(cannon.explosiveEnabled, Is.True);
        Assert.That(cannon.baseSplashRadius, Is.GreaterThan(0f));
        Assert.That(cannon.explodeOnWorldImpact, Is.True);
        Assert.That(knife.projectileVisualShape, Is.EqualTo(ProjectileVisualShape.Shard));
        Assert.That(spotlight.projectileVisualShape, Is.EqualTo(ProjectileVisualShape.Bolt));
        Assert.That(spotlight.chainEnabled, Is.True);
        Assert.That(spotlight.baseChainCount, Is.EqualTo(2));
        Assert.That(spotlight.maxChainCount, Is.EqualTo(6));
        Assert.That(spotlight.chainSearchRadius, Is.EqualTo(10f).Within(0.0001f));
        Assert.That(spotlight.chainDamageMultiplier, Is.EqualTo(0.65f).Within(0.0001f));
        Assert.That(new[] { defaultWeapon.projectileTrailColor, cannon.projectileTrailColor, knife.projectileTrailColor, spotlight.projectileTrailColor }.Distinct().Count(), Is.GreaterThan(2));

        TheCircussyOneTestObjects.Destroy(defaultWeapon);
        TheCircussyOneTestObjects.Destroy(cannon);
        TheCircussyOneTestObjects.Destroy(knife);
        TheCircussyOneTestObjects.Destroy(spotlight);
    }

    [Test]
    public void FirstPartyWeaponDefaultsContainCurrentRoster()
    {
        string[] expectedIds =
        {
            "cannon",
            "knife_fan",
            "spotlight_bolt",
            "fire_hoop",
            "juggling_ball"
        };

        Assert.That(FirstPartyWeaponDefaults.All.Select(spec => spec.Id), Is.EquivalentTo(expectedIds));
        Assert.That(FirstPartyWeaponDefaults.All.Select(spec => spec.Id).Distinct().Count(), Is.EqualTo(expectedIds.Length));

        foreach (FirstPartyWeaponSpec spec in FirstPartyWeaponDefaults.All)
        {
            Assert.That(spec.DisplayName, Is.Not.Empty);
            Assert.That(spec.Tags, Is.Not.Empty);
            Assert.That(spec.ProjectileDamage, Is.GreaterThan(0));
            Assert.That(spec.ProjectileSpeed, Is.GreaterThan(0f));
            Assert.That(spec.ProjectileBlockRadius, Is.GreaterThan(0f));
            Assert.That(spec.SupportedUpgradeStats, Is.Not.Empty);
        }

        Assert.That(FirstPartyWeaponDefaults.DefaultWeapon.Id, Is.EqualTo("juggling_ball"));
        Assert.That(FirstPartyWeaponDefaults.All.All(spec => spec.CanAppearAsLevelUpWeapon), Is.True);
        Assert.That(FirstPartyWeaponDefaults.GetOrDefault("cannon").Visuals.Shape, Is.EqualTo(ProjectileVisualShape.Sphere));
        Assert.That(FirstPartyWeaponDefaults.GetOrDefault("knife_fan").BaseProjectileCount, Is.EqualTo(3));
        Assert.That(FirstPartyWeaponDefaults.GetOrDefault("knife_fan").MaxProjectileCount, Is.EqualTo(7));
        Assert.That(FirstPartyWeaponDefaults.GetOrDefault("knife_fan").BaseSpreadAngleDegrees, Is.EqualTo(16f));
        Assert.That(FirstPartyWeaponDefaults.GetOrDefault("knife_fan").BaseAimErrorDegrees, Is.EqualTo(4f));
        Assert.That(FirstPartyWeaponDefaults.GetOrDefault("knife_fan").SupportedUpgradeStats, Does.Contain(StatId.WeaponProjectileCount));
        Assert.That(FirstPartyWeaponDefaults.GetOrDefault("knife_fan").SupportedUpgradeStats, Does.Contain(StatId.WeaponAccuracyMultiplier));
        Assert.That(FirstPartyWeaponDefaults.GetOrDefault("fire_hoop").OrbitEnabled, Is.True);
        Assert.That(FirstPartyWeaponDefaults.GetOrDefault("fire_hoop").BaseOrbitCount, Is.EqualTo(2));
        Assert.That(FirstPartyWeaponDefaults.GetOrDefault("fire_hoop").MaxOrbitCount, Is.EqualTo(4));
        Assert.That(FirstPartyWeaponDefaults.GetOrDefault("fire_hoop").OrbitRadius, Is.EqualTo(2.2f).Within(0.0001f));
        Assert.That(FirstPartyWeaponDefaults.GetOrDefault("fire_hoop").MaxOrbitRadius, Is.EqualTo(3.6f).Within(0.0001f));
        Assert.That(FirstPartyWeaponDefaults.GetOrDefault("fire_hoop").OrbitHitRadius, Is.EqualTo(0.75f).Within(0.0001f));
        Assert.That(FirstPartyWeaponDefaults.GetOrDefault("fire_hoop").MaxOrbitHitRadius, Is.EqualTo(1.25f).Within(0.0001f));
        Assert.That(FirstPartyWeaponDefaults.GetOrDefault("fire_hoop").SupportedUpgradeStats, Does.Contain(StatId.WeaponFlatDamage));
        Assert.That(FirstPartyWeaponDefaults.GetOrDefault("fire_hoop").SupportedUpgradeStats, Does.Contain(StatId.WeaponAttackSpeed));
        Assert.That(FirstPartyWeaponDefaults.GetOrDefault("fire_hoop").SupportedUpgradeStats, Does.Contain(StatId.WeaponProjectileCount));
        Assert.That(FirstPartyWeaponDefaults.GetOrDefault("fire_hoop").SupportedUpgradeStats, Does.Contain(StatId.WeaponProjectileSizeMultiplier));
        Assert.That(FirstPartyWeaponDefaults.GetOrDefault("fire_hoop").SupportedUpgradeStats, Does.Contain(StatId.WeaponRangeMultiplier));
        Assert.That(FirstPartyWeaponDefaults.GetOrDefault("cannon").TrajectoryMode, Is.EqualTo(ProjectileTrajectoryMode.Arc));
        Assert.That(FirstPartyWeaponDefaults.GetOrDefault("cannon").ExplosiveEnabled, Is.True);
        Assert.That(FirstPartyWeaponDefaults.GetOrDefault("cannon").SupportedUpgradeStats, Does.Contain(StatId.WeaponSplashRadiusMultiplier));
        Assert.That(FirstPartyWeaponDefaults.All.Where(spec => spec.Id != "cannon").All(spec => !spec.SupportedUpgradeStats.Contains(StatId.WeaponSplashRadiusMultiplier)), Is.True);
        Assert.That(FirstPartyWeaponDefaults.GetOrDefault("spotlight_bolt").ChainEnabled, Is.True);
        Assert.That(FirstPartyWeaponDefaults.GetOrDefault("spotlight_bolt").BaseChainCount, Is.EqualTo(2));
        Assert.That(FirstPartyWeaponDefaults.GetOrDefault("spotlight_bolt").MaxChainCount, Is.EqualTo(6));
        Assert.That(FirstPartyWeaponDefaults.GetOrDefault("spotlight_bolt").SupportedUpgradeStats, Does.Contain(StatId.WeaponChain));
        Assert.That(FirstPartyWeaponDefaults.All.Where(spec => spec.Id != "spotlight_bolt").All(spec => !spec.SupportedUpgradeStats.Contains(StatId.WeaponChain)), Is.True);
        Assert.That(FirstPartyWeaponDefaults.GetOrDefault("juggling_ball").Visuals.Shape, Is.EqualTo(ProjectileVisualShape.Sphere));
        Assert.That(FirstPartyWeaponDefaults.GetOrDefault("juggling_ball").BaseAimErrorDegrees, Is.EqualTo(7f));
        Assert.That(FirstPartyWeaponDefaults.GetOrDefault("juggling_ball").BounceEnabled, Is.True);
        Assert.That(FirstPartyWeaponDefaults.GetOrDefault("juggling_ball").SupportedUpgradeStats, Does.Contain(StatId.WeaponBounce));
        Assert.That(FirstPartyWeaponDefaults.GetOrDefault("juggling_ball").SupportedUpgradeStats, Does.Contain(StatId.WeaponAccuracyMultiplier));
        Assert.That(FirstPartyWeaponDefaults.All.Where(spec => spec.Id != "knife_fan" && spec.Id != "juggling_ball").All(spec => spec.BaseAimErrorDegrees == 0f), Is.True);
        Assert.That(FirstPartyWeaponDefaults.All.Where(spec => spec.Id != "juggling_ball").All(spec => !spec.SupportedUpgradeStats.Contains(StatId.WeaponBounce)), Is.True);
    }

    [Test]
    public void WeaponCatalogCreatesStartingLoadout()
    {
        var config = TheCircussyOneTestObjects.CreateConfig();
        WeaponDefinition weapon = TheCircussyOneTestObjects.CreateWeaponDefinition(config);
        WeaponCatalog catalog = TheCircussyOneTestObjects.CreateWeaponCatalog(weapon);

        var loadout = new WeaponLoadout(catalog);

        Assert.That(loadout.Weapons, Has.Count.EqualTo(1));
        Assert.That(loadout.Weapons[0].Definition, Is.SameAs(weapon));

        TheCircussyOneTestObjects.Destroy(catalog);
        TheCircussyOneTestObjects.Destroy(weapon);
        TheCircussyOneTestObjects.Destroy(config);
    }

    [Test]
    public void WeaponLoadoutSeedsMissingProjectileCollisionDefaults()
    {
        var weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
        weapon.weaponId = "juggling_ball";
        weapon.displayName = "Juggling Ball";
        weapon.projectileHitMask = LayerMask.GetMask(GameLayers.Enemy);
        WeaponCatalog catalog = TheCircussyOneTestObjects.CreateWeaponCatalog(weapon);

        _ = new WeaponLoadout(catalog);

        Assert.That(weapon.requireLineOfSight, Is.True);
        Assert.That(weapon.lineOfSightMask.value, Is.EqualTo(GameLayers.EnvironmentMaskExcludingGameplay));
        Assert.That(weapon.projectileBlockMask.value, Is.EqualTo(GameLayers.EnvironmentMaskExcludingGameplay));
        Assert.That(weapon.projectileBlockRadius, Is.EqualTo(0.16f).Within(0.0001f));

        TheCircussyOneTestObjects.Destroy(catalog);
        TheCircussyOneTestObjects.Destroy(weapon);
    }

    [Test]
    public void WeaponCatalogSeparatesAvailableWeaponsFromStartingLoadout()
    {
        WeaponDefinition starter = CreateTestWeapon("juggling_ball", "Juggling Ball");
        WeaponDefinition cannon = CreateTestWeapon("cannon", "Cannon");
        WeaponDefinition knifeFan = CreateTestWeapon("knife_fan", "Knife Fan");
        var catalog = ScriptableObject.CreateInstance<WeaponCatalog>();

        bool changed = catalog.EnsureWorkflowDefaults(starter, starter, cannon, knifeFan);
        var loadout = new WeaponLoadout(catalog);

        Assert.That(changed, Is.True);
        Assert.That(catalog.StartingWeapons, Is.EqualTo(new[] { starter }));
        Assert.That(catalog.AvailableWeapons, Is.EqualTo(new[] { starter, cannon, knifeFan }));
        Assert.That(catalog.Definitions, Is.EqualTo(catalog.AvailableWeapons));
        Assert.That(loadout.Weapons, Has.Count.EqualTo(1));
        Assert.That(loadout.OwnsWeapon("juggling_ball"), Is.True);
        Assert.That(loadout.OwnsWeapon("cannon"), Is.False);

        TheCircussyOneTestObjects.Destroy(catalog);
        TheCircussyOneTestObjects.Destroy(starter);
        TheCircussyOneTestObjects.Destroy(cannon);
        TheCircussyOneTestObjects.Destroy(knifeFan);
    }

    [Test]
    public void WeaponDefinitionsOwnUpgradeTrackCards()
    {
        WeaponDefinition cannon = CreateTestWeapon("cannon", "Cannon");
        UpgradeDefinition damage = TheCircussyOneTestObjects.CreateWeaponStatUpgrade(
            cannon,
            "calibration",
            new UpgradeStatModifierDefinition(StatId.GlobalDamageMultiplier, StatModifierBucket.AdditivePercent, 0.2f));

        bool changed = cannon.EnsureUpgradeTrackDefaults(damage);
        bool repeated = cannon.EnsureUpgradeTrackDefaults(damage);

        Assert.That(changed, Is.True);
        Assert.That(repeated, Is.False);
        Assert.That(cannon.UpgradeTrack, Is.EqualTo(new[] { damage }));
        Assert.That(damage.weaponDefinition, Is.SameAs(cannon));
        Assert.That(damage.weaponId, Is.EqualTo("cannon"));

        TheCircussyOneTestObjects.Destroy(damage);
        TheCircussyOneTestObjects.Destroy(cannon);
    }

    [Test]
    public void WorkflowDefaultsSeedSupportedWeaponUpgradeStats()
    {
        WeaponDefinition cannon = CreateTestWeapon("cannon", "Cannon");
        WeaponDefinition fireHoop = CreateTestWeapon("fire_hoop", "Fire Hoop");
        WeaponDefinition jugglingBall = CreateTestWeapon("juggling_ball", "Juggling Ball");

        Assert.That(cannon.SupportedUpgradeStats, Does.Contain(StatId.WeaponFlatDamage));
        Assert.That(cannon.SupportedUpgradeStats, Does.Contain(StatId.WeaponSplashRadiusMultiplier));
        Assert.That(cannon.explosiveEnabled, Is.True);
        Assert.That(cannon.projectileTrajectoryMode, Is.EqualTo(ProjectileTrajectoryMode.Arc));
        Assert.That(cannon.SupportedUpgradeStats.Contains(StatId.WeaponBounce), Is.False);
        Assert.That(fireHoop.orbitEnabled, Is.True);
        Assert.That(fireHoop.baseOrbitCount, Is.EqualTo(2));
        Assert.That(fireHoop.maxOrbitCount, Is.EqualTo(4));
        Assert.That(fireHoop.orbitHitRadius, Is.EqualTo(0.75f).Within(0.0001f));
        Assert.That(fireHoop.orbitDegreesPerSecond, Is.EqualTo(240f).Within(0.0001f));
        Assert.That(fireHoop.orbitAreaDamageEnabled, Is.True);
        Assert.That(fireHoop.orbitAreaDamageMultiplier, Is.EqualTo(0.55f).Within(0.0001f));
        Assert.That(fireHoop.orbitAreaHitIntervalSeconds, Is.EqualTo(0.45f).Within(0.0001f));
        Assert.That(fireHoop.SupportedUpgradeStats, Does.Contain(StatId.WeaponProjectileCount));
        Assert.That(fireHoop.SupportedUpgradeStats, Does.Contain(StatId.WeaponRangeMultiplier));
        Assert.That(fireHoop.SupportedUpgradeStats.Contains(StatId.WeaponProjectileLifetimeMultiplier), Is.False);
        Assert.That(jugglingBall.bounceEnabled, Is.True);
        Assert.That(jugglingBall.baseAimErrorDegrees, Is.EqualTo(7f).Within(0.0001f));
        Assert.That(jugglingBall.baseBounceCount, Is.EqualTo(1));
        Assert.That(jugglingBall.maxBounceCount, Is.EqualTo(4));
        Assert.That(jugglingBall.SupportedUpgradeStats, Does.Contain(StatId.WeaponBounce));
        Assert.That(jugglingBall.SupportedUpgradeStats, Does.Contain(StatId.WeaponAccuracyMultiplier));
        WeaponDefinition spotlight = CreateTestWeapon("spotlight_bolt", "Spotlight Bolt");
        Assert.That(spotlight.chainEnabled, Is.True);
        Assert.That(spotlight.baseChainCount, Is.EqualTo(2));
        Assert.That(spotlight.maxChainCount, Is.EqualTo(6));
        Assert.That(spotlight.SupportedUpgradeStats, Does.Contain(StatId.WeaponChain));

        TheCircussyOneTestObjects.Destroy(cannon);
        TheCircussyOneTestObjects.Destroy(fireHoop);
        TheCircussyOneTestObjects.Destroy(jugglingBall);
        TheCircussyOneTestObjects.Destroy(spotlight);
    }

    [Test]
    public void WorkflowDefaultsRemoveInvalidSupportedWeaponUpgradeStats()
    {
        WeaponDefinition weapon = CreateTestWeapon("juggling_ball", "Juggling Ball");
        weapon.supportedUpgradeStats.Clear();
        weapon.supportedUpgradeStats.Add(StatId.WeaponDamageMultiplier);
        weapon.supportedUpgradeStats.Add(StatId.WeaponFlatDamage);
        weapon.supportedUpgradeStats.Add(StatId.PlayerArmor);

        bool changed = weapon.EnsureWorkflowDefaults();

        Assert.That(changed, Is.True);
        Assert.That(weapon.SupportedUpgradeStats, Is.EqualTo(new[] { StatId.WeaponFlatDamage }));

        TheCircussyOneTestObjects.Destroy(weapon);
    }

    [Test]
    public void WeaponAuthoringApplicabilityMatchesImplementedFamilies()
    {
        WeaponDefinition fireHoop = CreateTestWeapon("fire_hoop", "Fire Hoop");
        WeaponDefinition cannon = CreateTestWeapon("cannon", "Cannon");
        WeaponDefinition jugglingBall = CreateTestWeapon("juggling_ball", "Juggling Ball");
        WeaponDefinition spotlightBolt = CreateTestWeapon("spotlight_bolt", "Spotlight Bolt");

        Assert.That(WeaponAuthoringApplicability.IsActive(fireHoop, WeaponAuthoringArea.Orbit), Is.True);
        Assert.That(WeaponAuthoringApplicability.IsActive(fireHoop, WeaponAuthoringArea.ProjectileTravel), Is.False);
        Assert.That(WeaponAuthoringApplicability.IsSupportedUpgradeStatActive(fireHoop, StatId.WeaponProjectileCount), Is.True);
        Assert.That(WeaponAuthoringApplicability.IsSupportedUpgradeStatActive(fireHoop, StatId.WeaponRangeMultiplier), Is.True);
        Assert.That(WeaponAuthoringApplicability.IsSupportedUpgradeStatActive(fireHoop, StatId.WeaponProjectileLifetimeMultiplier), Is.False);

        Assert.That(WeaponAuthoringApplicability.IsActive(cannon, WeaponAuthoringArea.Explosion), Is.True);
        Assert.That(WeaponAuthoringApplicability.IsSupportedUpgradeStatActive(cannon, StatId.WeaponSplashRadiusMultiplier), Is.True);
        Assert.That(WeaponAuthoringApplicability.IsSupportedUpgradeStatActive(cannon, StatId.WeaponBounce), Is.False);

        Assert.That(WeaponAuthoringApplicability.IsActive(jugglingBall, WeaponAuthoringArea.Bounce), Is.True);
        Assert.That(WeaponAuthoringApplicability.IsSupportedUpgradeStatActive(jugglingBall, StatId.WeaponBounce), Is.True);
        Assert.That(WeaponAuthoringApplicability.IsSupportedUpgradeStatActive(jugglingBall, StatId.WeaponAccuracyMultiplier), Is.True);

        Assert.That(WeaponAuthoringApplicability.IsActive(spotlightBolt, WeaponAuthoringArea.Chain), Is.True);
        Assert.That(WeaponAuthoringApplicability.IsSupportedUpgradeStatActive(spotlightBolt, StatId.WeaponChain), Is.True);
        Assert.That(WeaponAuthoringApplicability.IsSupportedUpgradeStatActive(spotlightBolt, StatId.WeaponSplashRadiusMultiplier), Is.False);

        TheCircussyOneTestObjects.Destroy(fireHoop);
        TheCircussyOneTestObjects.Destroy(cannon);
        TheCircussyOneTestObjects.Destroy(jugglingBall);
        TheCircussyOneTestObjects.Destroy(spotlightBolt);
    }

    [Test]
    public void CustomWeaponTemplatesApplyImplementedFamilyDefaultsWithoutFirstPartyMigration()
    {
        var spread = ScriptableObject.CreateInstance<WeaponDefinition>();
        spread.ApplyCustomTemplateDefaults("custom_knife_fan", "Custom Knife Fan", WeaponFamilyTemplate.SpreadProjectile);

        Assert.That(spread.weaponId, Is.EqualTo("custom_knife_fan"));
        Assert.That(spread.displayName, Is.EqualTo("Custom Knife Fan"));
        Assert.That(spread.baseProjectileCount, Is.EqualTo(3));
        Assert.That(spread.maxProjectileCount, Is.EqualTo(7));
        Assert.That(spread.baseSpreadAngleDegrees, Is.EqualTo(18f).Within(0.0001f));
        Assert.That(spread.SupportedUpgradeStats, Does.Contain(StatId.WeaponProjectileCount));
        Assert.That(spread.SupportedUpgradeStats, Does.Contain(StatId.WeaponAccuracyMultiplier));

        bool changed = spread.EnsureWorkflowDefaults();

        Assert.That(changed, Is.False);
        Assert.That(spread.weaponId, Is.EqualTo("custom_knife_fan"));
        Assert.That(spread.baseProjectileCount, Is.EqualTo(3));
        Assert.That(spread.bounceEnabled, Is.False);
        Assert.That(spread.orbitEnabled, Is.False);

        var orbit = ScriptableObject.CreateInstance<WeaponDefinition>();
        orbit.ApplyCustomTemplateDefaults("custom_hoop", "Custom Hoop", WeaponFamilyTemplate.Orbit);

        Assert.That(orbit.orbitEnabled, Is.True);
        Assert.That(orbit.baseOrbitCount, Is.EqualTo(1));
        Assert.That(orbit.orbitAreaDamageEnabled, Is.True);
        Assert.That(orbit.SupportedUpgradeStats, Does.Contain(StatId.WeaponRangeMultiplier));
        Assert.That(orbit.SupportedUpgradeStats.Contains(StatId.WeaponProjectileLifetimeMultiplier), Is.False);

        TheCircussyOneTestObjects.Destroy(spread);
        TheCircussyOneTestObjects.Destroy(orbit);
    }

    [Test]
    public void WeaponCatalogReportsFamilyConflictsAndInactiveSupportedStats()
    {
        var weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
        weapon.ApplyCustomTemplateDefaults("confused_hoop", "Confused Hoop", WeaponFamilyTemplate.Orbit);
        weapon.bounceEnabled = true;
        weapon.supportedUpgradeStats.Add(StatId.WeaponProjectileLifetimeMultiplier);
        WeaponCatalog catalog = TheCircussyOneTestObjects.CreateWeaponCatalog(weapon);

        var issues = catalog.ValidateContent();

        Assert.That(issues.Any(issue => issue.Code == "weapon.family-conflict"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "weapon.inactive-supported-stat"), Is.True);

        TheCircussyOneTestObjects.Destroy(catalog);
        TheCircussyOneTestObjects.Destroy(weapon);
    }

    [Test]
    public void CustomWeaponCreatorCreatesWeaponWithoutOverwritingAssets()
    {
        const string testRoot = "Assets/__WeaponCreatorTests";
        string weaponFolder = $"{testRoot}/Weapons";
        AssetDatabase.DeleteAsset(testRoot);

        CustomWeaponCreationResult result = TheCircussyOneCustomWeaponCreator.CreateCustomWeapon(
            "custom_cannon",
            "Custom Cannon",
            WeaponFamilyTemplate.ExplosiveArcProjectile,
            addToWeaponCatalog: false,
            weaponFolder: weaponFolder);

        try
        {
            Assert.That(result.Success, Is.True, result.Message);
            Assert.That(result.Weapon, Is.Not.Null);
            Assert.That(result.Weapon.weaponId, Is.EqualTo("custom_cannon"));
            Assert.That(result.Weapon.explosiveEnabled, Is.True);
            Assert.That(result.Weapon.projectileTrajectoryMode, Is.EqualTo(ProjectileTrajectoryMode.Arc));
            Assert.That(result.Weapon.SupportedUpgradeStats, Does.Contain(StatId.WeaponSplashRadiusMultiplier));
            Assert.That(result.Weapon.CanAppearAsLevelUpWeapon, Is.True);
            Assert.That(result.Weapon.AddWeaponShortDescription, Does.Contain("Custom Cannon"));

            CustomWeaponCreationResult duplicate = TheCircussyOneCustomWeaponCreator.CreateCustomWeapon(
                "custom_cannon",
                "Custom Cannon",
                WeaponFamilyTemplate.ExplosiveArcProjectile,
                addToWeaponCatalog: false,
                weaponFolder: weaponFolder);

            Assert.That(duplicate.Success, Is.False);
            Assert.That(duplicate.Message, Does.Contain("never overwritten"));
        }
        finally
        {
            AssetDatabase.DeleteAsset(testRoot);
            AssetDatabase.Refresh();
        }
    }

    [Test]
    public void WeaponCatalogReportsUnsupportedWeaponUpgradeStats()
    {
        WeaponDefinition cannon = CreateTestWeapon("cannon", "Cannon");
        cannon.supportedUpgradeStats.Clear();
        cannon.supportedUpgradeStats.Add(StatId.WeaponFlatDamage);
        UpgradeDefinition unsupported = TheCircussyOneTestObjects.CreateWeaponStatUpgrade(
            cannon,
            "bounce",
            new UpgradeStatModifierDefinition(StatId.WeaponBounce, StatModifierBucket.Flat, 1f));
        cannon.upgradeTrack.Add(unsupported);
        WeaponCatalog catalog = TheCircussyOneTestObjects.CreateWeaponCatalog(cannon);

        var issues = catalog.ValidateContent();

        Assert.That(issues.Any(issue => issue.Code == "weapon.upgrade-track-unsupported-stat"), Is.True);

        TheCircussyOneTestObjects.Destroy(catalog);
        TheCircussyOneTestObjects.Destroy(unsupported);
        TheCircussyOneTestObjects.Destroy(cannon);
    }

    [Test]
    public void WeaponLoadoutUsesFourSlotsAndStartingWeaponConsumesOne()
    {
        WeaponDefinition weapon = CreateTestWeapon("juggling_ball", "Juggling Ball");
        WeaponCatalog catalog = TheCircussyOneTestObjects.CreateWeaponCatalog(weapon);

        var loadout = new WeaponLoadout(catalog);

        Assert.That(loadout.MaxWeapons, Is.EqualTo(WeaponLoadout.DefaultMaxWeapons));
        Assert.That(loadout.WeaponCount, Is.EqualTo(1));
        Assert.That(loadout.AvailableSlots, Is.EqualTo(3));
        Assert.That(loadout.HasOpenSlot, Is.True);
        Assert.That(loadout.OwnsWeapon("juggling_ball"), Is.True);

        TheCircussyOneTestObjects.Destroy(catalog);
        TheCircussyOneTestObjects.Destroy(weapon);
    }

    [Test]
    public void WeaponLoadoutAddsNewWeaponsUntilSlotsAreFull()
    {
        WeaponDefinition starter = CreateTestWeapon("juggling_ball", "Juggling Ball");
        WeaponDefinition cannon = CreateTestWeapon("cannon", "Cannon");
        WeaponDefinition knifeFan = CreateTestWeapon("knife_fan", "Knife Fan");
        WeaponCatalog catalog = TheCircussyOneTestObjects.CreateWeaponCatalog(starter);

        var loadout = new WeaponLoadout(catalog, maxWeapons: 2);

        Assert.That(loadout.AddWeapon(starter), Is.False);
        Assert.That(loadout.AddWeapon(cannon), Is.True);
        Assert.That(loadout.WeaponCount, Is.EqualTo(2));
        Assert.That(loadout.AvailableSlots, Is.Zero);
        Assert.That(loadout.HasOpenSlot, Is.False);
        Assert.That(loadout.AddWeapon(knifeFan), Is.False);
        Assert.That(loadout.OwnsWeapon("cannon"), Is.True);
        Assert.That(loadout.OwnsWeapon("knife_fan"), Is.False);

        TheCircussyOneTestObjects.Destroy(catalog);
        TheCircussyOneTestObjects.Destroy(starter);
        TheCircussyOneTestObjects.Destroy(cannon);
        TheCircussyOneTestObjects.Destroy(knifeFan);
    }

    [Test]
    public void WeaponLoadoutCapsStartingCatalogBySlotLimit()
    {
        WeaponDefinition starter = CreateTestWeapon("juggling_ball", "Juggling Ball");
        WeaponDefinition cannon = CreateTestWeapon("cannon", "Cannon");
        WeaponDefinition knifeFan = CreateTestWeapon("knife_fan", "Knife Fan");
        WeaponCatalog catalog = TheCircussyOneTestObjects.CreateWeaponCatalog(starter, cannon, knifeFan);

        var loadout = new WeaponLoadout(catalog, maxWeapons: 2);

        Assert.That(loadout.Weapons, Has.Count.EqualTo(2));
        Assert.That(loadout.OwnsWeapon("juggling_ball"), Is.True);
        Assert.That(loadout.OwnsWeapon("cannon"), Is.True);
        Assert.That(loadout.OwnsWeapon("knife_fan"), Is.False);

        TheCircussyOneTestObjects.Destroy(catalog);
        TheCircussyOneTestObjects.Destroy(starter);
        TheCircussyOneTestObjects.Destroy(cannon);
        TheCircussyOneTestObjects.Destroy(knifeFan);
    }

    [Test]
    public void WeaponCatalogReportsDuplicateAndMissingIdsWithoutMutatingAssets()
    {
        var first = ScriptableObject.CreateInstance<WeaponDefinition>();
        first.weaponId = "juggling_ball";
        first.displayName = "Juggling Ball";
        first.tags = ContentTagSet.With(ContentTag.Projectile);

        var duplicate = ScriptableObject.CreateInstance<WeaponDefinition>();
        duplicate.weaponId = "juggling_ball";
        duplicate.displayName = "Duplicate Projectile";
        duplicate.tags = ContentTagSet.With(ContentTag.Projectile);

        var missing = ScriptableObject.CreateInstance<WeaponDefinition>();
        missing.weaponId = "";
        missing.displayName = "";
        missing.tags = ContentTagSet.With(ContentTag.Utility);

        WeaponCatalog catalog = TheCircussyOneTestObjects.CreateWeaponCatalog(first, duplicate, missing, null);

        var issues = catalog.ValidateContent();

        Assert.That(issues.Any(issue => issue.Code == "content.duplicate-id"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "content.invalid-id"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "content.missing-display-name"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "catalog.null-entry"), Is.True);
        Assert.That(catalog.availableWeapons, Has.Count.EqualTo(4));

        TheCircussyOneTestObjects.Destroy(catalog);
        Object.DestroyImmediate(first);
        Object.DestroyImmediate(duplicate);
        Object.DestroyImmediate(missing);
    }

    [Test]
    public void WeaponCatalogValidatesUpgradeTrackOwnership()
    {
        WeaponDefinition cannon = CreateTestWeapon("cannon", "Cannon");
        WeaponDefinition spotlightBolt = CreateTestWeapon("spotlight_bolt", "Spotlight Bolt");
        UpgradeDefinition valid = TheCircussyOneTestObjects.CreateWeaponStatUpgrade(cannon, "calibration");
        UpgradeDefinition duplicate = valid;
        UpgradeDefinition wrongReference = TheCircussyOneTestObjects.CreateWeaponStatUpgrade(spotlightBolt, "charge");
        UpgradeDefinition invalidEffect = TheCircussyOneTestObjects.CreateWeaponStatUpgrade(cannon, "invalid");
        invalidEffect.statModifiers.Clear();
        cannon.upgradeTrack.Add(valid);
        cannon.upgradeTrack.Add(duplicate);
        cannon.upgradeTrack.Add(wrongReference);
        cannon.upgradeTrack.Add(invalidEffect);
        cannon.upgradeTrack.Add(null);
        WeaponCatalog catalog = TheCircussyOneTestObjects.CreateWeaponCatalog(cannon, spotlightBolt);

        var issues = catalog.ValidateContent();

        Assert.That(issues.Any(issue => issue.Code == "weapon.upgrade-track-duplicate"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "weapon.upgrade-track-wrong-weapon-id"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "weapon.upgrade-track-wrong-weapon-reference"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "weapon.upgrade-track-unsupported-stat"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "weapon.upgrade-track-null"), Is.True);

        TheCircussyOneTestObjects.Destroy(catalog);
        TheCircussyOneTestObjects.Destroy(invalidEffect);
        TheCircussyOneTestObjects.Destroy(wrongReference);
        TheCircussyOneTestObjects.Destroy(valid);
        TheCircussyOneTestObjects.Destroy(cannon);
        TheCircussyOneTestObjects.Destroy(spotlightBolt);
    }

    [Test]
    public void WeaponLoadoutIgnoresNullDefinitions()
    {
        WeaponCatalog catalog = TheCircussyOneTestObjects.CreateWeaponCatalog(new WeaponDefinition[] { null });

        var loadout = new WeaponLoadout(catalog);

        Assert.That(loadout.Weapons, Is.Empty);

        TheCircussyOneTestObjects.Destroy(catalog);
    }

    private static WeaponDefinition CreateTestWeapon(string id, string displayName)
    {
        var config = TheCircussyOneTestObjects.CreateConfig();
        WeaponDefinition weapon = TheCircussyOneTestObjects.CreateWeaponDefinition(config, id, displayName);
        TheCircussyOneTestObjects.Destroy(config);
        return weapon;
    }
}
