using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Runtime;
using TheCircussyOne.Rules;
using TheCircussyOne.Stats;

public sealed class ProjectileSystemTests
{
    private GameConfig _config;
    private DamageFeedbackVisualConfig _damageFeedbackConfig;
    private ActorMotionVisualConfig _actorMotionConfig;
    private ActorRegistry _registry;
    private FakeGameTime _time;
    private EnemyFactory _enemyFactory;
    private ProjectileFactory _projectileFactory;
    private PickupFactory _pickupFactory;
    private XpDropService _xpDropService;
    private WeaponDefinition _weapon;
    private XpGemDefinition _blueGem;
    private XpGemDefinition _greenGem;
    private XpGemCatalog _xpGemCatalog;
    private FakeDamageNumberSpawner _damageNumbers;
    private FakeVfxSpawner _vfx;
    private FakeProjectileExplosionSpawner _projectileExplosions;
    private FakeProjectileChainVisualSpawner _chainVisuals;

    [SetUp]
    public void SetUp()
    {
        TheCircussyOneTestObjects.DestroyAll();
        TheCircussyOneTestObjects.RequireLayer(GameLayers.Enemy);
        _config = TheCircussyOneTestObjects.CreateConfig();
        _damageFeedbackConfig = TheCircussyOneTestObjects.CreateDamageFeedbackConfig();
        _actorMotionConfig = TheCircussyOneTestObjects.CreateActorMotionVisualConfig();
        _weapon = TheCircussyOneTestObjects.CreateWeaponDefinition(_config);
        ConfigurePlainProjectile();
        _registry = new ActorRegistry();
        _time = new FakeGameTime { DeltaTime = 0f };
        _enemyFactory = new EnemyFactory(_config, _registry, TheCircussyOneTestObjects.CreateEnemyPrefab(), TheCircussyOneTestObjects.CreateRoot("Enemies").transform);
        _projectileFactory = new ProjectileFactory(_damageFeedbackConfig, _registry, _time, TheCircussyOneTestObjects.CreateProjectilePrefab(), TheCircussyOneTestObjects.CreateRoot("Projectiles").transform);
        _pickupFactory = new PickupFactory(_config, _registry, TheCircussyOneTestObjects.CreatePickupPrefab(), TheCircussyOneTestObjects.CreateRoot("Pickups").transform);
        _blueGem = TheCircussyOneTestObjects.CreateXpGemDefinition("blue_xp_gem", "Blue XP Gem", 7, Color.cyan);
        _greenGem = TheCircussyOneTestObjects.CreateXpGemDefinition("green_xp_gem", "Green XP Gem", 23, Color.green);
        _xpGemCatalog = TheCircussyOneTestObjects.CreateXpGemCatalog(_blueGem, _greenGem);
        _xpGemCatalog.defaultEnemyXpBudget = 7;
        _xpDropService = new XpDropService(_xpGemCatalog, _pickupFactory);
        _damageNumbers = new FakeDamageNumberSpawner();
        _vfx = new FakeVfxSpawner();
        _projectileExplosions = new FakeProjectileExplosionSpawner();
        _chainVisuals = new FakeProjectileChainVisualSpawner();
    }

    [TearDown]
    public void TearDown()
    {
        TheCircussyOneTestObjects.DestroyAll();
        TheCircussyOneTestObjects.Destroy(_config);
        TheCircussyOneTestObjects.Destroy(_damageFeedbackConfig);
        TheCircussyOneTestObjects.Destroy(_actorMotionConfig);
        TheCircussyOneTestObjects.Destroy(_weapon);
        TheCircussyOneTestObjects.Destroy(_blueGem);
        TheCircussyOneTestObjects.Destroy(_greenGem);
        TheCircussyOneTestObjects.Destroy(_xpGemCatalog);
    }

    [Test]
    public void ProjectileHitDamagesEnemyAndRemovesProjectile()
    {
        var state = new GameState(_config);
        EnemyRuntime enemy = _enemyFactory.Spawn(Vector3.zero, level: 1, elapsedSeconds: 0f);
        _projectileFactory.Spawn(_weapon, new Vector3(0f, 0.8f, 0f), Vector3.forward);

        CreateSystem(state).Tick();

        Assert.That(enemy.CurrentHealth, Is.EqualTo(_config.enemyHealth - _weapon.projectileDamage));
        Assert.That(_registry.Projectiles, Is.Empty);
        Assert.That(_projectileFactory.PooledCount, Is.EqualTo(1));
        Assert.That(state.Kills, Is.Zero);
        Assert.That(_damageNumbers.Count, Is.EqualTo(1));
        Assert.That(_damageNumbers.LastAmount, Is.EqualTo(_weapon.projectileDamage));
        Assert.That(_damageNumbers.LastImpactPosition, Is.EqualTo(new Vector3(0f, 0.8f, 0f)));
        Assert.That(_damageNumbers.LastAnchorPosition, Is.EqualTo(enemy.AimPosition));
        Assert.That(_vfx.Calls, Has.Count.EqualTo(1));
        Assert.That(_vfx.Calls[0].effectId, Is.EqualTo(VfxEffectId.EnemyHitSparks));
    }

    [Test]
    public void EnemyXpBurstOriginUsesElevatedAimPoint()
    {
        EnemyRuntime enemy = _enemyFactory.Spawn(Vector3.zero, level: 1, elapsedSeconds: 0f);

        Vector3 origin = EnemyRewardDropRules.ResolveXpBurstOrigin(enemy);

        Assert.That(origin.y, Is.EqualTo(enemy.AimPosition.y + 0.15f).Within(0.001f));
        Assert.That(origin.y, Is.GreaterThan(enemy.Position.y + 0.5f));
    }

    [Test]
    public void ProjectileOverlapOnContactHitboxDoesNotDamageEnemy()
    {
        _config.enemyHurtboxRadius = 0.05f;
        _config.enemyHurtboxHeightOffset = 3f;
        _config.enemyContactHitboxSize = new Vector3(0.56f, 0.56f, 0.56f);
        _config.enemyContactHitboxOffset = new Vector3(0f, 0.8f, 0f);
        _weapon.projectileHitRadius = 0.04f;
        var state = new GameState(_config);
        EnemyRuntime enemy = _enemyFactory.Spawn(Vector3.zero, level: 1, elapsedSeconds: 0f);
        _projectileFactory.Spawn(_weapon, enemy.View.ContactHitboxCollider.bounds.center, Vector3.forward);

        CreateSystem(state).Tick();

        Assert.That(enemy.CurrentHealth, Is.EqualTo(_config.enemyHealth));
        Assert.That(_registry.Projectiles, Has.Count.EqualTo(1));
        Assert.That(_damageNumbers.Count, Is.Zero);
        Assert.That(_vfx.Calls, Is.Empty);
    }

    [Test]
    public void ProjectileOutsideVisualBodyButInsideHurtboxDamagesEnemy()
    {
        _config.enemyHurtboxRadius = 0.72f;
        _config.enemyHurtboxHeightOffset = 0.95f;
        _config.enemyContactHitboxSize = new Vector3(0.1f, 0.1f, 0.1f);
        _config.enemyContactHitboxOffset = new Vector3(0f, 3f, 0f);
        _weapon.projectileHitRadius = 0.04f;
        var state = new GameState(_config);
        EnemyRuntime enemy = _enemyFactory.Spawn(Vector3.zero, level: 1, elapsedSeconds: 0f);
        _projectileFactory.Spawn(_weapon, new Vector3(0.68f, _config.enemyHurtboxHeightOffset, 0f), Vector3.forward);

        CreateSystem(state).Tick();

        Assert.That(enemy.CurrentHealth, Is.EqualTo(_config.enemyHealth - _weapon.projectileDamage));
        Assert.That(_registry.Projectiles, Is.Empty);
        Assert.That(_damageNumbers.Count, Is.EqualTo(1));
        Assert.That(_vfx.Calls, Has.Count.EqualTo(1));
    }

    [Test]
    public void LethalProjectileHitAddsKillDespawnsEnemyAndSpawnsPickup()
    {
        _weapon.projectileDamage = _config.enemyHealth;
        var state = new GameState(_config);
        EnemyRuntime enemy = _enemyFactory.Spawn(Vector3.zero, level: 1, elapsedSeconds: 0f);
        _projectileFactory.Spawn(_weapon, new Vector3(0f, 0.8f, 0f), Vector3.forward);

        CreateSystem(state).Tick();

        Assert.That(enemy.IsDead, Is.True);
        Assert.That(_registry.Enemies, Is.Empty);
        Assert.That(_registry.Projectiles, Is.Empty);
        Assert.That(state.Kills, Is.EqualTo(1));
        Assert.That(_registry.Pickups, Has.Count.EqualTo(1));
        Assert.That(_registry.Pickups[0].Gem, Is.SameAs(_blueGem));
        Assert.That(_registry.Pickups[0].Amount, Is.EqualTo(7));
        Assert.That(_damageNumbers.Count, Is.EqualTo(1));
        Assert.That(_damageNumbers.LastAmount, Is.EqualTo(_weapon.projectileDamage));
        Assert.That(_vfx.Calls, Has.Count.EqualTo(2));
        Assert.That(_vfx.Calls[0].effectId, Is.EqualTo(VfxEffectId.EnemyHitSparks));
        Assert.That(_vfx.Calls[1].effectId, Is.EqualTo(VfxEffectId.EnemyDeathBurst));
    }

    [Test]
    public void LethalProjectileHitSpawnsConfiguredXpBudgetDrops()
    {
        _weapon.projectileDamage = _config.enemyHealth;
        EnemyDefinition definition = TheCircussyOneTestObjects.CreateEnemyDefinition(_config);
        definition.xpBudget = 30;
        definition.dropStyle = XpDropStyle.Compact;
        EnemyCatalog enemyCatalog = TheCircussyOneTestObjects.CreateEnemyCatalog(definition);
        _enemyFactory = new EnemyFactory(
            _config,
            null,
            enemyCatalog,
            _registry,
            TheCircussyOneTestObjects.CreateEnemyPrefab(),
            TheCircussyOneTestObjects.CreateRoot("Definition Enemies").transform);
        var state = new GameState(_config);
        _enemyFactory.Spawn(Vector3.zero, level: 1, elapsedSeconds: 0f);
        _projectileFactory.Spawn(_weapon, new Vector3(0f, 0.8f, 0f), Vector3.forward);

        CreateSystem(state).Tick();

        Assert.That(_registry.Pickups, Has.Count.EqualTo(2));
        Assert.That(_registry.Pickups.Select(pickup => pickup.Gem.Id).ToArray(), Is.EqualTo(new[] { "green_xp_gem", "blue_xp_gem" }));
        Assert.That(_registry.Pickups.Sum(pickup => pickup.Amount), Is.EqualTo(30));
        Object.DestroyImmediate(definition);
        Object.DestroyImmediate(enemyCatalog);
    }

    [Test]
    public void ProjectileMovementAndHitQueryUseProjectileOwnedStats()
    {
        _weapon.projectileSpeed = 4f;
        _weapon.projectileHitRadius = 0.15f;
        _time.DeltaTime = 0.25f;
        var state = new GameState(_config);
        EnemyRuntime enemy = _enemyFactory.Spawn(new Vector3(0f, 0f, 2.2f), level: 1, elapsedSeconds: 0f);
        ProjectileRuntime projectile = _projectileFactory.Spawn(_weapon, new Vector3(0f, 0.8f, 0f), Vector3.forward);

        CreateSystem(state).Tick();

        Assert.That(projectile.Position.z, Is.EqualTo(1f).Within(0.0001f));
        Assert.That(enemy.CurrentHealth, Is.EqualTo(_config.enemyHealth));
        Assert.That(_registry.Projectiles, Has.Count.EqualTo(1));
    }

    [Test]
    public void ProjectileDespawnsOnWorldBlockerBeforeEnemyHitProcessing()
    {
        _weapon.projectileSpeed = 12f;
        _weapon.projectileDamage = _config.enemyHealth;
        _weapon.projectileBlockMask = 1 << 0;
        _time.DeltaTime = 0.12f;
        var state = new GameState(_config);
        EnemyRuntime enemy = _enemyFactory.Spawn(new Vector3(0f, 0f, 1.4f), level: 1, elapsedSeconds: 0f);
        CreateWall("Projectile Blocker", new Vector3(0f, 0.8f, 0.7f), new Vector3(2f, 2f, 0.08f));
        _projectileFactory.Spawn(_weapon, new Vector3(0f, 0.8f, 0f), Vector3.forward);
        Physics.SyncTransforms();

        CreateSystem(state).Tick();

        Assert.That(_registry.Projectiles, Is.Empty);
        Assert.That(enemy.CurrentHealth, Is.EqualTo(_config.enemyHealth));
        Assert.That(state.Kills, Is.Zero);
        Assert.That(_damageNumbers.Count, Is.Zero);
        Assert.That(_vfx.Calls, Is.Empty);
    }

    [Test]
    public void FastProjectileSweepCatchesThinWorldBlockerBetweenFrames()
    {
        _weapon.projectileSpeed = 60f;
        _weapon.projectileBlockMask = 1 << 0;
        _time.DeltaTime = 0.1f;
        var state = new GameState(_config);
        CreateWall("Thin Projectile Blocker", new Vector3(0f, 0.8f, 2f), new Vector3(2f, 2f, 0.04f));
        _projectileFactory.Spawn(_weapon, new Vector3(0f, 0.8f, 0f), Vector3.forward);
        Physics.SyncTransforms();

        CreateSystem(state).Tick();

        Assert.That(_registry.Projectiles, Is.Empty);
    }

    [Test]
    public void ProjectileBlockSweepUsesProjectileBodyRadiusAroundObstacleEdges()
    {
        _weapon.projectileSpeed = 16f;
        _weapon.projectileBlockMask = 1 << 0;
        _weapon.projectileBlockRadius = 0.25f;
        _time.DeltaTime = 0.1f;
        var state = new GameState(_config);
        CreateWall("Offset Projectile Blocker", new Vector3(0.22f, 0.8f, 0.8f), new Vector3(0.08f, 2f, 0.25f));
        _projectileFactory.Spawn(_weapon, new Vector3(0f, 0.8f, 0f), Vector3.forward);
        Physics.SyncTransforms();

        CreateSystem(state).Tick();

        Assert.That(_registry.Projectiles, Is.Empty);
    }

    [Test]
    public void ArcProjectileFollowsRaisedPathAndDespawnsAtEndpoint()
    {
        _weapon.projectileSpeed = 10f;
        _weapon.projectileBlockMask = 0;
        _time.DeltaTime = 0.5f;
        var state = new GameState(_config);
        ProjectileRuntime projectile = _projectileFactory.Spawn(
            _weapon,
            new ProjectileSpawnFrame(
                Vector3.zero,
                Vector3.forward,
                new Vector3(0f, 0f, 10f),
                ProjectileTrajectoryMode.Arc,
                4f));

        CreateSystem(state).Tick();

        Assert.That(projectile.Position.z, Is.EqualTo(5f).Within(0.0001f));
        Assert.That(projectile.Position.y, Is.EqualTo(4f).Within(0.0001f));
        Assert.That(_registry.Projectiles, Has.Count.EqualTo(1));

        CreateSystem(state).Tick();

        Assert.That(_registry.Projectiles, Is.Empty);
    }

    [Test]
    public void JugglingBallBouncesFromFirstEnemyToSecondEnemy()
    {
        ConfigureJugglingBall(bounces: 1);
        _time.DeltaTime = 0.001f;
        var state = new GameState(_config);
        EnemyRuntime first = _enemyFactory.Spawn(Vector3.zero, level: 1, elapsedSeconds: 0f);
        EnemyRuntime second = _enemyFactory.Spawn(new Vector3(2f, 0f, 0f), level: 1, elapsedSeconds: 0f);
        ProjectileRuntime projectile = _projectileFactory.Spawn(_weapon, first.AimPosition, Vector3.forward);

        CreateSystem(state).Tick();

        Assert.That(first.CurrentHealth, Is.EqualTo(first.MaxHealth - _weapon.projectileDamage));
        Assert.That(second.CurrentHealth, Is.EqualTo(second.MaxHealth));
        Assert.That(_registry.Projectiles, Has.Count.EqualTo(1));
        Assert.That(projectile.RemainingBounces, Is.Zero);
        Assert.That(projectile.Direction.x, Is.GreaterThan(0.9f));

        _time.DeltaTime = 0.2f;
        CreateSystem(state).Tick();

        Assert.That(second.CurrentHealth, Is.EqualTo(second.MaxHealth - _weapon.projectileDamage));
        Assert.That(_registry.Projectiles, Is.Empty);
    }

    [Test]
    public void JugglingBallDoesNotRetargetAlreadyHitEnemies()
    {
        ConfigureJugglingBall(bounces: 2);
        _time.DeltaTime = 0.001f;
        var state = new GameState(_config);
        EnemyRuntime first = _enemyFactory.Spawn(Vector3.zero, level: 1, elapsedSeconds: 0f);
        EnemyRuntime second = _enemyFactory.Spawn(new Vector3(2f, 0f, 0f), level: 1, elapsedSeconds: 0f);
        _projectileFactory.Spawn(_weapon, first.AimPosition, Vector3.forward);

        CreateSystem(state).Tick();

        _time.DeltaTime = 0.2f;
        CreateSystem(state).Tick();

        Assert.That(first.CurrentHealth, Is.EqualTo(first.MaxHealth - _weapon.projectileDamage));
        Assert.That(second.CurrentHealth, Is.EqualTo(second.MaxHealth - _weapon.projectileDamage));
        Assert.That(_registry.Projectiles, Is.Empty);
    }

    [Test]
    public void JugglingBallIgnoresBlockedBounceTargets()
    {
        ConfigureJugglingBall(bounces: 1);
        _weapon.lineOfSightMask = 1 << 0;
        _time.DeltaTime = 0.001f;
        var state = new GameState(_config);
        EnemyRuntime first = _enemyFactory.Spawn(Vector3.zero, level: 1, elapsedSeconds: 0f);
        _enemyFactory.Spawn(new Vector3(2f, 0f, 0f), level: 1, elapsedSeconds: 0f);
        _enemyFactory.Spawn(new Vector3(0f, 0f, 2f), level: 1, elapsedSeconds: 0f);
        CreateWall("Bounce Line Of Sight Blocker", new Vector3(1f, 1f, 0f), new Vector3(0.12f, 3f, 2f));
        ProjectileRuntime projectile = _projectileFactory.Spawn(_weapon, first.AimPosition, Vector3.forward);
        Physics.SyncTransforms();

        CreateSystem(state).Tick();

        Assert.That(_registry.Projectiles, Has.Count.EqualTo(1));
        Assert.That(projectile.Direction.z, Is.GreaterThan(0.8f));
        Assert.That(Mathf.Abs(projectile.Direction.x), Is.LessThan(0.4f));
    }

    [Test]
    public void JugglingBallReflectsFromWorldBlockerWhenConfigured()
    {
        ConfigureJugglingBall(bounces: 1);
        _weapon.projectileBlockMask = 1 << 0;
        _weapon.bounceOffWorldBlockers = true;
        _time.DeltaTime = 0.2f;
        var state = new GameState(_config);
        CreateWall("Juggling Ball Reflector", new Vector3(0f, 0.8f, 1f), new Vector3(2f, 2f, 0.08f));
        ProjectileRuntime projectile = _projectileFactory.Spawn(_weapon, new Vector3(0f, 0.8f, 0f), Vector3.forward);
        Physics.SyncTransforms();

        CreateSystem(state).Tick();

        Assert.That(_registry.Projectiles, Has.Count.EqualTo(1));
        Assert.That(projectile.Direction.z, Is.LessThan(-0.5f));
        Assert.That(projectile.RemainingBounces, Is.Zero);
    }

    [Test]
    public void JugglingBallDespawnsOnWorldBlockerWhenBouncesAreExhausted()
    {
        ConfigureJugglingBall(bounces: 0);
        _weapon.projectileBlockMask = 1 << 0;
        _weapon.bounceOffWorldBlockers = true;
        _time.DeltaTime = 0.2f;
        var state = new GameState(_config);
        CreateWall("Juggling Ball Stopper", new Vector3(0f, 0.8f, 1f), new Vector3(2f, 2f, 0.08f));
        _projectileFactory.Spawn(_weapon, new Vector3(0f, 0.8f, 0f), Vector3.forward);
        Physics.SyncTransforms();

        CreateSystem(state).Tick();

        Assert.That(_registry.Projectiles, Is.Empty);
    }

    [Test]
    public void CannonDirectHitDealsFullDamageAndSplashDealsHalfDamage()
    {
        ConfigureCannon(splashRadius: 2.5f);
        _weapon.projectileDamage = _config.enemyHealth * 2;
        _time.DeltaTime = 0.001f;
        var state = new GameState(_config);
        EnemyRuntime primary = _enemyFactory.Spawn(Vector3.zero, level: 1, elapsedSeconds: 0f);
        EnemyRuntime secondary = _enemyFactory.Spawn(new Vector3(1.6f, 0f, 0f), level: 1, elapsedSeconds: 0f);
        _projectileFactory.Spawn(_weapon, primary.AimPosition, Vector3.forward);

        CreateSystem(state).Tick();

        Assert.That(primary.IsDead, Is.True);
        Assert.That(secondary.IsDead, Is.True);
        Assert.That(state.Kills, Is.EqualTo(2));
        Assert.That(_registry.Projectiles, Is.Empty);
        Assert.That(_registry.Pickups, Has.Count.EqualTo(2));
        Assert.That(_damageNumbers.Count, Is.EqualTo(2));
        Assert.That(_damageNumbers.Calls.Select(call => call.AnchorPosition), Does.Contain(primary.AimPosition));
        Assert.That(_damageNumbers.Calls.Select(call => call.AnchorPosition), Does.Contain(secondary.AimPosition));
        Assert.That(_damageNumbers.Calls.Select(call => call.ImpactPosition).Distinct().Count(), Is.EqualTo(1));
        Assert.That(_projectileExplosions.Calls, Has.Count.EqualTo(1));
        Assert.That(_projectileExplosions.Calls[0].radius, Is.EqualTo(2.5f).Within(0.0001f));
    }

    [Test]
    public void CannonSplashIgnoresContactAndMovementHitboxes()
    {
        _config.enemyHurtboxRadius = 0.05f;
        _config.enemyHurtboxHeightOffset = 5f;
        _config.enemyContactHitboxSize = new Vector3(0.8f, 0.8f, 0.8f);
        _config.enemyContactHitboxOffset = new Vector3(0f, 0.8f, 0f);
        _config.enemyMovementBodyRadius = 0.8f;
        _config.enemyMovementBodyHeight = 1.2f;
        _config.enemyMovementBodyOffset = new Vector3(0f, 0.6f, 0f);
        ConfigureCannon(splashRadius: 1.2f);
        var state = new GameState(_config);
        EnemyRuntime enemy = _enemyFactory.Spawn(Vector3.zero, level: 1, elapsedSeconds: 0f);
        Vector3 explosionPosition = enemy.View.ContactHitboxCollider.bounds.center;
        _projectileFactory.Spawn(
            _weapon,
            new ProjectileSpawnFrame(
                explosionPosition,
                Vector3.forward,
                explosionPosition,
                ProjectileTrajectoryMode.Arc,
                0f));

        CreateSystem(state).Tick();

        Assert.That(enemy.CurrentHealth, Is.EqualTo(enemy.MaxHealth));
        Assert.That(_damageNumbers.Count, Is.Zero);
        Assert.That(_projectileExplosions.Calls, Has.Count.EqualTo(1));
        Assert.That(_projectileExplosions.Calls[0].radius, Is.EqualTo(1.2f).Within(0.0001f));
    }

    [Test]
    public void CannonWorldImpactExplodesAndDoesNotPassThroughBlocker()
    {
        ConfigureCannon(splashRadius: 0.5f);
        _weapon.projectileSpeed = 12f;
        _weapon.projectileBlockMask = 1 << 0;
        _time.DeltaTime = 0.12f;
        var state = new GameState(_config);
        EnemyRuntime enemy = _enemyFactory.Spawn(new Vector3(0f, 0f, 2.2f), level: 1, elapsedSeconds: 0f);
        CreateWall("Cannon Impact Wall", new Vector3(0f, 0.8f, 0.7f), new Vector3(2f, 2f, 0.08f));
        _projectileFactory.Spawn(_weapon, new Vector3(0f, 0.8f, 0f), Vector3.forward);
        Physics.SyncTransforms();

        CreateSystem(state).Tick();

        Assert.That(_registry.Projectiles, Is.Empty);
        Assert.That(enemy.CurrentHealth, Is.EqualTo(enemy.MaxHealth));
        Assert.That(_damageNumbers.Count, Is.Zero);
        Assert.That(_projectileExplosions.Calls, Has.Count.EqualTo(1));
        Assert.That(_projectileExplosions.Calls[0].radius, Is.EqualTo(0.5f).Within(0.0001f));
    }

    [Test]
    public void CannonArcEndpointExplodesWhenNoEnemyIsHitFirst()
    {
        ConfigureCannon(splashRadius: 2f);
        _weapon.projectileDamage = 6;
        _weapon.projectileHitRadius = 0.05f;
        _weapon.projectileBlockMask = 0;
        _time.DeltaTime = 0.5f;
        var state = new GameState(_config);
        EnemyRuntime enemy = _enemyFactory.Spawn(new Vector3(1.4f, 0f, 1f), level: 1, elapsedSeconds: 0f);
        _projectileFactory.Spawn(
            _weapon,
            new ProjectileSpawnFrame(
                new Vector3(0f, 0.8f, 0f),
                Vector3.forward,
                new Vector3(0f, 0.8f, 1f),
                ProjectileTrajectoryMode.Arc,
                1.5f));

        CreateSystem(state).Tick();
        CreateSystem(state).Tick();

        Assert.That(_registry.Projectiles, Is.Empty);
        Assert.That(enemy.CurrentHealth, Is.EqualTo(enemy.MaxHealth - 3));
        Assert.That(_damageNumbers.Count, Is.EqualTo(1));
        Assert.That(_projectileExplosions.Calls, Has.Count.EqualTo(1));
        Assert.That(_projectileExplosions.Calls[0].radius, Is.EqualTo(2f).Within(0.0001f));
    }

    [Test]
    public void SpotlightBoltChainsFromPrimaryToNearbyUnhitEnemies()
    {
        ConfigureSpotlightBolt(chains: 2, damage: 4, chainDamageMultiplier: 0.5f);
        _time.DeltaTime = 0.001f;
        var state = new GameState(_config);
        EnemyRuntime first = _enemyFactory.Spawn(Vector3.zero, level: 1, elapsedSeconds: 0f);
        EnemyRuntime second = _enemyFactory.Spawn(new Vector3(2f, 0f, 0f), level: 1, elapsedSeconds: 0f);
        EnemyRuntime third = _enemyFactory.Spawn(new Vector3(4f, 0f, 0f), level: 1, elapsedSeconds: 0f);
        _projectileFactory.Spawn(_weapon, first.AimPosition, Vector3.forward);

        CreateSystem(state).Tick();

        Assert.That(first.CurrentHealth, Is.EqualTo(first.MaxHealth - 4));
        Assert.That(second.CurrentHealth, Is.EqualTo(second.MaxHealth - 2));
        Assert.That(third.CurrentHealth, Is.EqualTo(third.MaxHealth - 2));
        Assert.That(_registry.Projectiles, Is.Empty);
        Assert.That(_chainVisuals.Calls, Has.Count.EqualTo(2));
    }

    [Test]
    public void SpotlightBoltDoesNotChainToAlreadyHitEnemy()
    {
        ConfigureSpotlightBolt(chains: 4, damage: 4, chainDamageMultiplier: 0.5f);
        _time.DeltaTime = 0.001f;
        var state = new GameState(_config);
        EnemyRuntime first = _enemyFactory.Spawn(Vector3.zero, level: 1, elapsedSeconds: 0f);
        EnemyRuntime second = _enemyFactory.Spawn(new Vector3(2f, 0f, 0f), level: 1, elapsedSeconds: 0f);
        _projectileFactory.Spawn(_weapon, first.AimPosition, Vector3.forward);

        CreateSystem(state).Tick();

        Assert.That(first.CurrentHealth, Is.EqualTo(first.MaxHealth - 4));
        Assert.That(second.CurrentHealth, Is.EqualTo(second.MaxHealth - 2));
        Assert.That(_chainVisuals.Calls, Has.Count.EqualTo(1));
        Assert.That(_registry.Projectiles, Is.Empty);
    }

    [Test]
    public void SpotlightBoltIgnoresBlockedChainTargets()
    {
        ConfigureSpotlightBolt(chains: 1, damage: 4, chainDamageMultiplier: 0.5f);
        _weapon.lineOfSightMask = 1 << 0;
        _time.DeltaTime = 0.001f;
        var state = new GameState(_config);
        EnemyRuntime first = _enemyFactory.Spawn(Vector3.zero, level: 1, elapsedSeconds: 0f);
        EnemyRuntime blocked = _enemyFactory.Spawn(new Vector3(2f, 0f, 0f), level: 1, elapsedSeconds: 0f);
        EnemyRuntime visible = _enemyFactory.Spawn(new Vector3(0f, 0f, 2f), level: 1, elapsedSeconds: 0f);
        CreateWall("Chain Line Of Sight Blocker", new Vector3(1f, 1f, 0f), new Vector3(0.12f, 3f, 2f));
        _projectileFactory.Spawn(_weapon, first.AimPosition, Vector3.forward);
        Physics.SyncTransforms();

        CreateSystem(state).Tick();

        Assert.That(first.CurrentHealth, Is.EqualTo(first.MaxHealth - 4));
        Assert.That(blocked.CurrentHealth, Is.EqualTo(blocked.MaxHealth));
        Assert.That(visible.CurrentHealth, Is.EqualTo(visible.MaxHealth - 2));
        Assert.That(_chainVisuals.Calls, Has.Count.EqualTo(1));
    }

    [Test]
    public void SpotlightBoltChainDamageIsAtLeastOneForPositiveProjectileDamage()
    {
        ConfigureSpotlightBolt(chains: 1, damage: 1, chainDamageMultiplier: 0.1f);
        _time.DeltaTime = 0.001f;
        var state = new GameState(_config);
        EnemyRuntime first = _enemyFactory.Spawn(Vector3.zero, level: 1, elapsedSeconds: 0f);
        EnemyRuntime second = _enemyFactory.Spawn(new Vector3(2f, 0f, 0f), level: 1, elapsedSeconds: 0f);
        _projectileFactory.Spawn(_weapon, first.AimPosition, Vector3.forward);

        CreateSystem(state).Tick();

        Assert.That(first.CurrentHealth, Is.EqualTo(first.MaxHealth - 1));
        Assert.That(second.CurrentHealth, Is.EqualTo(second.MaxHealth - 1));
    }

    [Test]
    public void ProjectileFactoryEvaluatesStatsOnlyAtSpawnTime()
    {
        _weapon.projectileDamage = 10;
        _weapon.projectileSpeed = 20f;
        var stats = new RunStats(_config);
        stats.AddModifier(StatModifier.AdditivePercent(StatId.GlobalDamageMultiplier, 0.5f, "damage"));
        stats.AddModifier(StatModifier.AdditivePercent(StatId.ProjectileSpeedMultiplier, 0.25f, "speed"));
        var factory = new ProjectileFactory(
            _damageFeedbackConfig,
            _registry,
            _time,
            TheCircussyOneTestObjects.CreateProjectilePrefab(),
            TheCircussyOneTestObjects.CreateRoot("Stat Projectiles").transform,
            stats);

        ProjectileRuntime projectile = factory.Spawn(_weapon, Vector3.zero, Vector3.forward);
        stats.AddModifier(StatModifier.AdditivePercent(StatId.GlobalDamageMultiplier, 1f, "late_damage"));
        stats.AddModifier(StatModifier.AdditivePercent(StatId.ProjectileSpeedMultiplier, 1f, "late_speed"));

        Assert.That(projectile.Damage, Is.EqualTo(15));
        Assert.That(projectile.Speed, Is.EqualTo(25f));
    }

    private ProjectileSystem CreateSystem(GameState state)
    {
        var feedback = new FeedbackService(_config, _damageFeedbackConfig, _actorMotionConfig, TheCircussyOneTestObjects.CreatePlayer(_config), null, null, null, null);
        return new ProjectileSystem(_config, _damageFeedbackConfig, _actorMotionConfig, _registry, _projectileFactory, _enemyFactory, _xpDropService, state, feedback, _damageNumbers, _time, _vfx, _projectileExplosions, _chainVisuals);
    }

    private void ConfigureJugglingBall(int bounces)
    {
        _weapon.ApplyDefaults(FirstPartyWeaponDefaults.GetOrDefault(FirstPartyWeaponDefaults.JugglingBallId));
        _weapon.projectileDamage = 1;
        _weapon.projectileSpeed = 10f;
        _weapon.projectileHitRadius = 0.75f;
        _weapon.projectileBlockMask = 0;
        _weapon.lineOfSightMask = 0;
        _weapon.bounceEnabled = true;
        _weapon.baseBounceCount = Mathf.Max(0, bounces);
        _weapon.maxBounceCount = Mathf.Max(0, bounces);
        _weapon.enemyBounceSearchRadius = 6f;
        _weapon.bounceOffWorldBlockers = false;
    }

    private void ConfigurePlainProjectile()
    {
        _weapon.orbitEnabled = false;
        _weapon.bounceEnabled = false;
        _weapon.baseBounceCount = 0;
        _weapon.maxBounceCount = 0;
        _weapon.bounceOffWorldBlockers = false;
        _weapon.explosiveEnabled = false;
        _weapon.baseSplashRadius = 0f;
        _weapon.maxSplashRadius = 0f;
        _weapon.explodeOnWorldImpact = false;
        _weapon.chainEnabled = false;
        _weapon.baseChainCount = 0;
        _weapon.maxChainCount = 0;
        _weapon.projectileTrajectoryMode = ProjectileTrajectoryMode.Direct;
        _weapon.projectileArcHeight = 0f;
        _weapon.baseProjectileCount = 1;
        _weapon.maxProjectileCount = 1;
        _weapon.baseSpreadAngleDegrees = 0f;
        _weapon.accuracyAffectsSpread = false;
        _weapon.lineOfSightMask = 0;
        _weapon.projectileBlockMask = 0;
    }

    private void ConfigureCannon(float splashRadius)
    {
        _weapon.ApplyDefaults(FirstPartyWeaponDefaults.GetOrDefault(FirstPartyWeaponDefaults.CannonId));
        _weapon.projectileBlockMask = 0;
        _weapon.lineOfSightMask = 0;
        _weapon.explosiveEnabled = true;
        _weapon.baseSplashRadius = splashRadius;
        _weapon.maxSplashRadius = splashRadius;
        _weapon.secondarySplashDamageMultiplier = 0.5f;
        _weapon.explodeOnWorldImpact = true;
        _weapon.explosionVfxEnabled = true;
    }

    private void ConfigureSpotlightBolt(int chains, int damage, float chainDamageMultiplier)
    {
        _weapon.ApplyDefaults(FirstPartyWeaponDefaults.GetOrDefault(FirstPartyWeaponDefaults.SpotlightBoltId));
        _weapon.projectileDamage = damage;
        _weapon.projectileSpeed = 10f;
        _weapon.projectileHitRadius = 0.75f;
        _weapon.projectileBlockMask = 0;
        _weapon.lineOfSightMask = 0;
        _weapon.chainEnabled = true;
        _weapon.baseChainCount = Mathf.Max(0, chains);
        _weapon.maxChainCount = Mathf.Max(0, chains);
        _weapon.chainSearchRadius = 6f;
        _weapon.chainDamageMultiplier = chainDamageMultiplier;
        _weapon.chainRequiresLineOfSight = true;
        _weapon.bounceEnabled = false;
        _weapon.explosiveEnabled = false;
        _weapon.orbitEnabled = false;
    }

    private static GameObject CreateWall(string name, Vector3 position, Vector3 scale)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = "TCO Test " + name;
        wall.transform.position = position;
        wall.transform.localScale = scale;
        return wall;
    }
}

internal sealed class FakeDamageNumberSpawner : IDamageNumberSpawner
{
    public readonly List<DamageNumberRequest> Calls = new();
    public int Count => Calls.Count;
    public int LastAmount => Calls.Count > 0 ? Calls[^1].Amount : 0;
    public Vector3 LastAnchorPosition => Calls.Count > 0 ? Calls[^1].AnchorPosition : Vector3.zero;
    public Vector3 LastImpactPosition => Calls.Count > 0 ? Calls[^1].ImpactPosition : Vector3.zero;

    public void Show(DamageNumberRequest request)
    {
        Calls.Add(request);
    }
}

internal sealed class FakeProjectileExplosionSpawner : IProjectileExplosionSpawner
{
    public readonly System.Collections.Generic.List<(Vector3 position, float radius, Vector3 direction)> Calls = new();

    public void Show(Vector3 position, float radius, Vector3 direction)
    {
        Calls.Add((position, radius, direction));
    }
}

internal sealed class FakeProjectileChainVisualSpawner : IProjectileChainVisualSpawner
{
    public readonly System.Collections.Generic.List<(Vector3 start, Vector3 end, Color color)> Calls = new();

    public void Show(Vector3 start, Vector3 end, Color color)
    {
        Calls.Add((start, end, color));
    }
}
