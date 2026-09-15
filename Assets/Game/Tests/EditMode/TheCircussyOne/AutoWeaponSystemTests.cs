using NUnit.Framework;
using UnityEngine;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using TheCircussyOne.Runtime;
using TheCircussyOne.Stats;
using TheCircussyOne.Visuals;

public sealed class AutoWeaponSystemTests
{
    private GameConfig _config;
    private DamageFeedbackVisualConfig _damageFeedbackConfig;
    private ActorMotionVisualConfig _actorMotionConfig;
    private WeaponDefinition _weapon;
    private WeaponCatalog _weaponCatalog;
    private WeaponLoadout _loadout;
    private ActorRegistry _registry;
    private FakeGameTime _time;

    [SetUp]
    public void SetUp()
    {
        TheCircussyOneTestObjects.DestroyAll();
        _config = TheCircussyOneTestObjects.CreateConfig();
        _damageFeedbackConfig = TheCircussyOneTestObjects.CreateDamageFeedbackConfig();
        _actorMotionConfig = TheCircussyOneTestObjects.CreateActorMotionVisualConfig();
        _weapon = TheCircussyOneTestObjects.CreateWeaponDefinition(_config);
        ConfigurePlainProjectile(_weapon);
        _weaponCatalog = TheCircussyOneTestObjects.CreateWeaponCatalog(_weapon);
        _loadout = new WeaponLoadout(_weaponCatalog);
        _registry = new ActorRegistry();
        _time = new FakeGameTime { DeltaTime = 1f };
    }

    [TearDown]
    public void TearDown()
    {
        TheCircussyOneTestObjects.DestroyAll();
        TheCircussyOneTestObjects.Destroy(_config);
        TheCircussyOneTestObjects.Destroy(_damageFeedbackConfig);
        TheCircussyOneTestObjects.Destroy(_actorMotionConfig);
        TheCircussyOneTestObjects.Destroy(_weaponCatalog);
        TheCircussyOneTestObjects.Destroy(_weapon);
    }

    [Test]
    public void FiresProjectileTowardEnemyBodyWhenPlayerIsAirborne()
    {
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(_config);
        player.transform.position = new Vector3(0f, 3f, 0f);

        EnemyRuntime enemy = TheCircussyOneTestObjects.CreateEnemy(_config, new Vector3(0f, 0f, 8f));
        _registry.Register(enemy);

        var projectileFactory = new ProjectileFactory(
            _registry,
            _time,
            TheCircussyOneTestObjects.CreateProjectilePrefab(),
            TheCircussyOneTestObjects.CreateRoot("Projectiles").transform);
        var state = new GameState(_config);
        var feedback = new FeedbackService(_config, _damageFeedbackConfig, _actorMotionConfig, player, null, null, null, null);
        var system = new AutoWeaponSystem(_loadout, player, _registry, projectileFactory, state, feedback, _time);

        system.Tick();

        Assert.That(_registry.Projectiles, Has.Count.EqualTo(1));
        ProjectileRuntime projectile = _registry.Projectiles[0];
        Vector3 expectedDirection = WeaponSpawnRules.AimDirection(projectile.Position, enemy.AimPosition, Vector3.forward);
        Assert.That(projectile.Direction.x, Is.EqualTo(expectedDirection.x).Within(0.0001f));
        Assert.That(projectile.Direction.y, Is.EqualTo(expectedDirection.y).Within(0.0001f));
        Assert.That(projectile.Direction.z, Is.EqualTo(expectedDirection.z).Within(0.0001f));
        Assert.That(Mathf.Abs(projectile.Direction.y), Is.GreaterThan(0.0001f));
    }

    [Test]
    public void FiresAtNearestVisibleEnemyWhenNearestEnemyIsOccluded()
    {
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(_config);
        EnemyRuntime occluded = TheCircussyOneTestObjects.CreateEnemy(_config, new Vector3(0f, 0f, 7f), "Occluded Enemy");
        EnemyRuntime visible = TheCircussyOneTestObjects.CreateEnemy(_config, new Vector3(3f, 0f, 8f), "Visible Enemy");
        _registry.Register(occluded);
        _registry.Register(visible);
        CreateWall("LoS Wall", new Vector3(0f, 1f, 3.8f), new Vector3(0.6f, 2f, 0.2f));
        Physics.SyncTransforms();

        var projectileFactory = new ProjectileFactory(
            _registry,
            _time,
            TheCircussyOneTestObjects.CreateProjectilePrefab(),
            TheCircussyOneTestObjects.CreateRoot("Projectiles").transform);
        var state = new GameState(_config);
        var feedback = new FeedbackService(_config, _damageFeedbackConfig, _actorMotionConfig, player, null, null, null, null);
        var system = new AutoWeaponSystem(_loadout, player, _registry, projectileFactory, state, feedback, _time);

        system.Tick();

        Assert.That(_registry.Projectiles, Has.Count.EqualTo(1));
        ProjectileRuntime projectile = _registry.Projectiles[0];
        Vector3 expectedDirection = WeaponSpawnRules.AimDirection(projectile.Position, visible.AimPosition, Vector3.forward);
        Assert.That(Vector3.Dot(projectile.Direction, expectedDirection), Is.GreaterThan(0.999f));
    }

    [Test]
    public void KnifeFanFiresConfiguredSymmetricSpread()
    {
        _weapon.weaponId = FirstPartyWeaponDefaults.KnifeFanId;
        _weapon.displayName = "Knife Fan";
        _weapon.baseProjectileCount = 3;
        _weapon.maxProjectileCount = 7;
        _weapon.baseSpreadAngleDegrees = 20f;
        _weapon.baseAimErrorDegrees = 0f;
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(_config);
        EnemyRuntime enemy = TheCircussyOneTestObjects.CreateEnemy(_config, new Vector3(0f, 0f, 8f));
        _registry.Register(enemy);

        var projectileFactory = new ProjectileFactory(
            _registry,
            _time,
            TheCircussyOneTestObjects.CreateProjectilePrefab(),
            TheCircussyOneTestObjects.CreateRoot("Projectiles").transform);
        var state = new GameState(_config);
        var feedback = new FeedbackService(_config, _damageFeedbackConfig, _actorMotionConfig, player, null, null, null, null);
        var system = new AutoWeaponSystem(_loadout, player, _registry, projectileFactory, state, feedback, _time);

        system.Tick();

        Assert.That(_registry.Projectiles, Has.Count.EqualTo(3));
        Vector3 center = WeaponSpawnRules.AimDirection(_registry.Projectiles[1].Position, enemy.AimPosition, Vector3.forward);
        Assert.That(Vector3.Dot(_registry.Projectiles[1].Direction, center), Is.GreaterThan(0.999f));
        Assert.That(Vector3.Dot(_registry.Projectiles[0].Direction, WeaponSpawnRules.SpreadDirection(center, 0, 3, 20f)), Is.GreaterThan(0.999f));
        Assert.That(Vector3.Dot(_registry.Projectiles[2].Direction, WeaponSpawnRules.SpreadDirection(center, 2, 3, 20f)), Is.GreaterThan(0.999f));
    }

    [Test]
    public void OrbitEnabledWeaponDoesNotFireNormalProjectiles()
    {
        _weapon.ApplyDefaults(FirstPartyWeaponDefaults.GetOrDefault(FirstPartyWeaponDefaults.FireHoopId));
        _weapon.orbitEnabled = true;
        _loadout.SetStartingWeapons(_weapon);
        float initialCooldown = _loadout.Weapons[0].RemainingCooldown;
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(_config);
        EnemyRuntime enemy = TheCircussyOneTestObjects.CreateEnemy(_config, new Vector3(0f, 0f, 8f));
        _registry.Register(enemy);

        var projectileFactory = new ProjectileFactory(
            _registry,
            _time,
            TheCircussyOneTestObjects.CreateProjectilePrefab(),
            TheCircussyOneTestObjects.CreateRoot("Projectiles").transform);
        var state = new GameState(_config);
        var feedback = new FeedbackService(_config, _damageFeedbackConfig, _actorMotionConfig, player, null, null, null, null);
        var system = new AutoWeaponSystem(_loadout, player, _registry, projectileFactory, state, feedback, _time);

        system.Tick();

        Assert.That(_registry.Projectiles, Is.Empty);
        Assert.That(_loadout.Weapons[0].RemainingCooldown, Is.EqualTo(initialCooldown).Within(0.0001f));
    }

    [Test]
    public void WeaponProjectileCountAndAccuracyModifyAimErrorAfterPatternSpread()
    {
        _weapon.baseProjectileCount = 3;
        _weapon.maxProjectileCount = 4;
        _weapon.baseSpreadAngleDegrees = 30f;
        _weapon.baseAimErrorDegrees = 10f;
        _weapon.accuracyAffectsAimError = true;
        WeaponRuntime runtime = _loadout.Weapons[0];
        runtime.AddModifier(StatModifier.Flat(StatId.WeaponProjectileCount, 5f, "count"));
        runtime.AddModifier(StatModifier.AdditivePercent(StatId.WeaponAccuracyMultiplier, 1f, "accuracy"));
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(_config);
        EnemyRuntime enemy = TheCircussyOneTestObjects.CreateEnemy(_config, new Vector3(0f, 0f, 8f));
        _registry.Register(enemy);

        var projectileFactory = new ProjectileFactory(
            _registry,
            _time,
            TheCircussyOneTestObjects.CreateProjectilePrefab(),
            TheCircussyOneTestObjects.CreateRoot("Projectiles").transform);
        var state = new GameState(_config);
        var feedback = new FeedbackService(_config, _damageFeedbackConfig, _actorMotionConfig, player, null, null, null, null);
        var system = new AutoWeaponSystem(_loadout, player, _registry, projectileFactory, state, feedback, _time);

        system.Tick();

        Assert.That(_registry.Projectiles, Has.Count.EqualTo(4));
        float patternSpread = WeaponStatRules.SpreadAngle(_weapon, null, runtime.Modifiers);
        float narrowedAimError = WeaponStatRules.AimErrorAngle(_weapon, null, runtime.Modifiers);
        Assert.That(patternSpread, Is.EqualTo(30f).Within(0.0001f));
        Assert.That(narrowedAimError, Is.EqualTo(5f).Within(0.0001f));
        Vector3 center = WeaponSpawnRules.AimDirection(_registry.Projectiles[1].Position, enemy.AimPosition, Vector3.forward);
        Vector3 firstPattern = WeaponSpawnRules.SpreadDirection(center, 0, 4, patternSpread);
        Vector3 lastPattern = WeaponSpawnRules.SpreadDirection(center, 3, 4, patternSpread);
        Vector3 expectedFirst = WeaponSpawnRules.AimErrorDirection(
            firstPattern,
            narrowedAimError,
            WeaponSpawnRules.AimErrorSeed(_weapon.Id, 0, 0));
        Vector3 expectedLast = WeaponSpawnRules.AimErrorDirection(
            lastPattern,
            narrowedAimError,
            WeaponSpawnRules.AimErrorSeed(_weapon.Id, 0, 3));
        Assert.That(Vector3.Dot(_registry.Projectiles[0].Direction, expectedFirst), Is.GreaterThan(0.999f));
        Assert.That(Vector3.Dot(_registry.Projectiles[3].Direction, expectedLast), Is.GreaterThan(0.999f));
        Assert.That(Vector3.Angle(_registry.Projectiles[0].Direction, firstPattern), Is.LessThanOrEqualTo(narrowedAimError + 0.0001f));
        Assert.That(Vector3.Angle(_registry.Projectiles[3].Direction, lastPattern), Is.LessThanOrEqualTo(narrowedAimError + 0.0001f));
    }

    [Test]
    public void LineOfSightUsesProjectileBodyRadiusAroundObstacleEdges()
    {
        _weapon.projectileBlockRadius = 0.25f;
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(_config);
        EnemyRuntime occluded = TheCircussyOneTestObjects.CreateEnemy(_config, new Vector3(0f, 0f, 7f), "Occluded Enemy");
        EnemyRuntime visible = TheCircussyOneTestObjects.CreateEnemy(_config, new Vector3(3f, 0f, 8f), "Visible Enemy");
        _registry.Register(occluded);
        _registry.Register(visible);
        CreateWall("LoS Edge Wall", new Vector3(0.22f, 1f, 3.8f), new Vector3(0.08f, 2f, 0.2f));
        Physics.SyncTransforms();

        var projectileFactory = new ProjectileFactory(
            _registry,
            _time,
            TheCircussyOneTestObjects.CreateProjectilePrefab(),
            TheCircussyOneTestObjects.CreateRoot("Projectiles").transform);
        var state = new GameState(_config);
        var feedback = new FeedbackService(_config, _damageFeedbackConfig, _actorMotionConfig, player, null, null, null, null);
        var system = new AutoWeaponSystem(_loadout, player, _registry, projectileFactory, state, feedback, _time);

        system.Tick();

        Assert.That(_registry.Projectiles, Has.Count.EqualTo(1));
        ProjectileRuntime projectile = _registry.Projectiles[0];
        Vector3 expectedDirection = WeaponSpawnRules.AimDirection(projectile.Position, visible.AimPosition, Vector3.forward);
        Assert.That(Vector3.Dot(projectile.Direction, expectedDirection), Is.GreaterThan(0.999f));
    }

    [Test]
    public void DoesNotFireOrConsumeCooldownWhenAllEnemiesAreOccluded()
    {
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(_config);
        EnemyRuntime enemy = TheCircussyOneTestObjects.CreateEnemy(_config, new Vector3(0f, 0f, 7f));
        _registry.Register(enemy);
        CreateWall("LoS Wall", new Vector3(0f, 1f, 3.8f), new Vector3(6f, 2f, 0.2f));
        Physics.SyncTransforms();

        var projectileFactory = new ProjectileFactory(
            _registry,
            _time,
            TheCircussyOneTestObjects.CreateProjectilePrefab(),
            TheCircussyOneTestObjects.CreateRoot("Projectiles").transform);
        var state = new GameState(_config);
        var feedback = new FeedbackService(_config, _damageFeedbackConfig, _actorMotionConfig, player, null, null, null, null);
        var system = new AutoWeaponSystem(_loadout, player, _registry, projectileFactory, state, feedback, _time);

        system.Tick();

        Assert.That(_registry.Projectiles, Is.Empty);
        Assert.That(_loadout.Weapons[0].RemainingCooldown, Is.Zero);
    }

    [Test]
    public void DisablingLineOfSightPreservesNearestTargeting()
    {
        _weapon.requireLineOfSight = false;
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(_config);
        EnemyRuntime occludedNearest = TheCircussyOneTestObjects.CreateEnemy(_config, new Vector3(0f, 0f, 7f), "Occluded Enemy");
        EnemyRuntime visibleFarther = TheCircussyOneTestObjects.CreateEnemy(_config, new Vector3(3f, 0f, 8f), "Visible Enemy");
        _registry.Register(occludedNearest);
        _registry.Register(visibleFarther);
        CreateWall("LoS Wall", new Vector3(0f, 1f, 3.8f), new Vector3(0.6f, 2f, 0.2f));
        Physics.SyncTransforms();

        var projectileFactory = new ProjectileFactory(
            _registry,
            _time,
            TheCircussyOneTestObjects.CreateProjectilePrefab(),
            TheCircussyOneTestObjects.CreateRoot("Projectiles").transform);
        var state = new GameState(_config);
        var feedback = new FeedbackService(_config, _damageFeedbackConfig, _actorMotionConfig, player, null, null, null, null);
        var system = new AutoWeaponSystem(_loadout, player, _registry, projectileFactory, state, feedback, _time);

        system.Tick();

        ProjectileRuntime projectile = _registry.Projectiles[0];
        Vector3 expectedDirection = WeaponSpawnRules.AimDirection(projectile.Position, occludedNearest.AimPosition, Vector3.forward);
        Assert.That(Vector3.Dot(projectile.Direction, expectedDirection), Is.GreaterThan(0.999f));
    }

    [Test]
    public void FiredProjectileUsesWeaponDefinitionStatsInsteadOfGameConfig()
    {
        _weapon.projectileDamage = 9;
        _weapon.projectileSpeed = 13f;
        _weapon.projectileHitRadius = 0.42f;
        _weapon.projectileLifetimeSeconds = 2.25f;
        _weapon.projectileHitMask = LayerMask.GetMask(GameLayers.Enemy);

        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(_config);
        EnemyRuntime enemy = TheCircussyOneTestObjects.CreateEnemy(_config, new Vector3(0f, 0f, 8f));
        _registry.Register(enemy);

        var projectileFactory = new ProjectileFactory(
            _registry,
            _time,
            TheCircussyOneTestObjects.CreateProjectilePrefab(),
            TheCircussyOneTestObjects.CreateRoot("Projectiles").transform);
        var state = new GameState(_config);
        var feedback = new FeedbackService(_config, _damageFeedbackConfig, _actorMotionConfig, player, null, null, null, null);
        var system = new AutoWeaponSystem(_loadout, player, _registry, projectileFactory, state, feedback, _time);

        system.Tick();

        ProjectileRuntime projectile = _registry.Projectiles[0];
        Assert.That(projectile.SourceWeapon, Is.SameAs(_weapon));
        Assert.That(projectile.Damage, Is.EqualTo(9));
        Assert.That(projectile.Speed, Is.EqualTo(13f));
        Assert.That(projectile.HitRadius, Is.EqualTo(0.42f));
        Assert.That(projectile.HitMask.value, Is.EqualTo(_weapon.projectileHitMask.value));
        Assert.That(projectile.ExpiresAt, Is.EqualTo(_time.Time + 2.25f));
    }

    [Test]
    public void FiredProjectileCopiesEvaluatedSpawnTimeStats()
    {
        _weapon.projectileDamage = 10;
        _weapon.projectileSpeed = 20f;
        _weapon.projectileHitMask = LayerMask.GetMask(GameLayers.Enemy);
        var stats = new RunStats(_config);
        stats.AddModifier(StatModifier.AdditivePercent(StatId.GlobalDamageMultiplier, 0.5f, "damage"));
        stats.AddModifier(StatModifier.AdditivePercent(StatId.ProjectileSpeedMultiplier, 0.25f, "speed"));

        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(_config);
        EnemyRuntime enemy = TheCircussyOneTestObjects.CreateEnemy(_config, new Vector3(0f, 0f, 8f));
        _registry.Register(enemy);

        var projectileFactory = new ProjectileFactory(
            _damageFeedbackConfig,
            _registry,
            _time,
            TheCircussyOneTestObjects.CreateProjectilePrefab(),
            TheCircussyOneTestObjects.CreateRoot("Projectiles").transform,
            stats);
        var state = new GameState(_config, stats);
        var feedback = new FeedbackService(_config, _damageFeedbackConfig, _actorMotionConfig, player, null, null, null, null);
        var system = new AutoWeaponSystem(_loadout, player, _registry, projectileFactory, state, feedback, _time, stats);

        system.Tick();
        ProjectileRuntime projectile = _registry.Projectiles[0];
        stats.AddModifier(StatModifier.AdditivePercent(StatId.GlobalDamageMultiplier, 1f, "late_damage"));
        stats.AddModifier(StatModifier.AdditivePercent(StatId.ProjectileSpeedMultiplier, 1f, "late_speed"));

        Assert.That(projectile.Damage, Is.EqualTo(15));
        Assert.That(projectile.Speed, Is.EqualTo(25f));
    }

    [Test]
    public void FiredProjectileCopiesEvaluatedWeaponLocalStats()
    {
        _weapon.projectileDamage = 10;
        _weapon.projectileSpeed = 20f;
        _weapon.projectileHitMask = LayerMask.GetMask(GameLayers.Enemy);
        var stats = new RunStats(_config);
        WeaponRuntime runtime = _loadout.Weapons[0];
        runtime.AddModifier(StatModifier.Flat(StatId.WeaponFlatDamage, 5f, "weapon_damage"));
        runtime.AddModifier(StatModifier.AdditivePercent(StatId.WeaponProjectileSpeedMultiplier, 0.25f, "weapon_speed"));

        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(_config);
        EnemyRuntime enemy = TheCircussyOneTestObjects.CreateEnemy(_config, new Vector3(0f, 0f, 8f));
        _registry.Register(enemy);

        var projectileFactory = new ProjectileFactory(
            _damageFeedbackConfig,
            _registry,
            _time,
            TheCircussyOneTestObjects.CreateProjectilePrefab(),
            TheCircussyOneTestObjects.CreateRoot("Projectiles").transform,
            stats);
        var state = new GameState(_config, stats);
        var feedback = new FeedbackService(_config, _damageFeedbackConfig, _actorMotionConfig, player, null, null, null, null);
        var system = new AutoWeaponSystem(_loadout, player, _registry, projectileFactory, state, feedback, _time, stats);

        system.Tick();
        ProjectileRuntime projectile = _registry.Projectiles[0];
        runtime.AddModifier(StatModifier.Flat(StatId.WeaponFlatDamage, 10f, "late_weapon_damage"));

        Assert.That(projectile.Damage, Is.EqualTo(15));
        Assert.That(projectile.Speed, Is.EqualTo(25f));
    }

    [Test]
    public void CooldownUsesAttackSpeedButIgnoresWeaponLevel()
    {
        _weapon.baseFireIntervalSeconds = 0.5f;
        _weapon.minimumFireIntervalSeconds = 0.1f;
        var stats = new RunStats(_config);
        stats.AddModifier(StatModifier.AttackSpeedBonus(StatId.GlobalWeaponHaste, 1f, "attack_speed"));
        _loadout.IncreaseWeaponLevel(_weapon.Id, 2);
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(_config);
        EnemyRuntime enemy = TheCircussyOneTestObjects.CreateEnemy(_config, new Vector3(0f, 0f, 8f));
        _registry.Register(enemy);
        var projectileFactory = new ProjectileFactory(
            _registry,
            _time,
            TheCircussyOneTestObjects.CreateProjectilePrefab(),
            TheCircussyOneTestObjects.CreateRoot("Projectiles").transform);
        var state = new GameState(_config, stats);
        var feedback = new FeedbackService(_config, _damageFeedbackConfig, _actorMotionConfig, player, null, null, null, null);
        var system = new AutoWeaponSystem(_loadout, player, _registry, projectileFactory, state, feedback, _time, stats);

        system.Tick();

        Assert.That(_loadout.Weapons[0].RemainingCooldown, Is.EqualTo(0.25f).Within(0.0001f));
    }

    private static GameObject CreateWall(string name, Vector3 position, Vector3 scale)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = "TCO Test " + name;
        wall.transform.position = position;
        wall.transform.localScale = scale;
        return wall;
    }

    private static void ConfigurePlainProjectile(WeaponDefinition weapon)
    {
        weapon.orbitEnabled = false;
        weapon.bounceEnabled = false;
        weapon.baseBounceCount = 0;
        weapon.maxBounceCount = 0;
        weapon.bounceOffWorldBlockers = false;
        weapon.explosiveEnabled = false;
        weapon.baseSplashRadius = 0f;
        weapon.maxSplashRadius = 0f;
        weapon.explodeOnWorldImpact = false;
        weapon.projectileTrajectoryMode = ProjectileTrajectoryMode.Direct;
        weapon.projectileArcHeight = 0f;
        weapon.baseAimErrorDegrees = 0f;
        weapon.accuracyAffectsAimError = true;
    }
}
