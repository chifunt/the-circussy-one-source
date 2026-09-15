using NUnit.Framework;
using UnityEngine;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using TheCircussyOne.Runtime;
using TheCircussyOne.Stats;
using TheCircussyOne.Visuals;

public sealed class OrbitWeaponSystemTests
{
    private GameConfig _config;
    private DamageFeedbackVisualConfig _damageFeedbackConfig;
    private ActorMotionVisualConfig _actorMotionConfig;
    private VfxVisualConfig _vfxConfig;
    private WeaponDefinition _weapon;
    private WeaponCatalog _weaponCatalog;
    private WeaponLoadout _loadout;
    private ActorRegistry _registry;
    private FakeGameTime _time;
    private RunStats _stats;
    private EnemyFactory _enemyFactory;
    private XpDropService _xpDropService;
    private FakeDamageNumberSpawner _damageNumbers;
    private FakeVfxSpawner _vfx;

    [SetUp]
    public void SetUp()
    {
        TheCircussyOneTestObjects.DestroyAll();
        TheCircussyOneTestObjects.RequireLayer(GameLayers.Enemy);
        _config = TheCircussyOneTestObjects.CreateConfig();
        _config.enemyHealth = 20;
        _damageFeedbackConfig = TheCircussyOneTestObjects.CreateDamageFeedbackConfig();
        _actorMotionConfig = TheCircussyOneTestObjects.CreateActorMotionVisualConfig();
        _vfxConfig = TheCircussyOneTestObjects.CreateVfxVisualConfig();
        _vfxConfig.fireHoopAreaTelegraphSampleCount = 16;
        _weapon = TheCircussyOneTestObjects.CreateWeaponDefinition(_config);
        _weapon.ApplyDefaults(FirstPartyWeaponDefaults.GetOrDefault(FirstPartyWeaponDefaults.FireHoopId));
        _weapon.projectileDamage = 2;
        _weapon.projectileHitMask = LayerMask.GetMask(GameLayers.Enemy);
        _weapon.lineOfSightMask = 0;
        _weapon.baseFireIntervalSeconds = 0.6f;
        _weapon.minimumFireIntervalSeconds = 0.1f;
        _weapon.orbitEnabled = true;
        _weapon.baseOrbitCount = 1;
        _weapon.maxOrbitCount = 4;
        _weapon.orbitRadius = 2.2f;
        _weapon.maxOrbitRadius = 3.6f;
        _weapon.orbitHitRadius = 0.6f;
        _weapon.maxOrbitHitRadius = 1.2f;
        _weapon.orbitDegreesPerSecond = 0f;
        _weapon.orbitHeightOffset = 0.9f;
        _weapon.orbitAreaDamageEnabled = false;
        _weapon.orbitAreaDamageMultiplier = 0.55f;
        _weapon.orbitAreaHitIntervalSeconds = 0.45f;
        _weaponCatalog = TheCircussyOneTestObjects.CreateWeaponCatalog(_weapon);
        _loadout = new WeaponLoadout(_weaponCatalog);
        _registry = new ActorRegistry();
        _time = new FakeGameTime { Time = 0f, DeltaTime = 0f };
        _stats = new RunStats(_config);
        _enemyFactory = new EnemyFactory(
            _config,
            _damageFeedbackConfig,
            _registry,
            TheCircussyOneTestObjects.CreateEnemyPrefab(),
            TheCircussyOneTestObjects.CreateRoot("Enemies").transform);
        var pickupFactory = new PickupFactory(
            _config,
            _registry,
            TheCircussyOneTestObjects.CreatePickupPrefab(),
            TheCircussyOneTestObjects.CreateRoot("Pickups").transform);
        var gem = TheCircussyOneTestObjects.CreateXpGemDefinition("blue_xp_gem", "Blue XP Gem", XpGemDefinition.DefaultBlueXpAmount, Color.cyan);
        var gems = TheCircussyOneTestObjects.CreateXpGemCatalog(gem);
        _xpDropService = new XpDropService(gems, pickupFactory);
        _damageNumbers = new FakeDamageNumberSpawner();
        _vfx = new FakeVfxSpawner();
    }

    [TearDown]
    public void TearDown()
    {
        TheCircussyOneTestObjects.DestroyAll();
        TheCircussyOneTestObjects.Destroy(_config);
        TheCircussyOneTestObjects.Destroy(_damageFeedbackConfig);
        TheCircussyOneTestObjects.Destroy(_actorMotionConfig);
        TheCircussyOneTestObjects.Destroy(_vfxConfig);
        TheCircussyOneTestObjects.Destroy(_weapon);
        TheCircussyOneTestObjects.Destroy(_weaponCatalog);
    }

    [Test]
    public void ReconcilesOrbitViewsForEvaluatedHoopCount()
    {
        _loadout.Weapons[0].AddModifier(StatModifier.Flat(StatId.WeaponProjectileCount, 2f, "extra_hoops"));
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(_config);
        Transform root = TheCircussyOneTestObjects.CreateRoot("Orbit Weapons").transform;
        OrbitWeaponSystem system = CreateSystem(player, root: root);

        system.Tick();

        Assert.That(system.ActiveRuntimeCount, Is.EqualTo(1));
        Assert.That(root.childCount, Is.EqualTo(3));
    }

    [Test]
    public void OrbitViewsFollowPlayerVisualTilt()
    {
        _weapon.orbitRadius = 2f;
        _weapon.orbitHeightOffset = 0.9f;
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(_config);
        Vector3 groundNormal = Quaternion.AngleAxis(20f, Vector3.right) * Vector3.up;
        player.GetComponent<ActorBodyView>().ApplyGroundTilt(groundNormal, 20f, 0f, 0.016f);
        Transform root = TheCircussyOneTestObjects.CreateRoot("Tilted Orbit Weapons").transform;
        OrbitWeaponSystem system = CreateSystem(player, root: root);

        system.Tick();

        OrbitWeaponView view = root.GetComponentInChildren<OrbitWeaponView>();
        OrbitWeaponFrame expected = OrbitWeaponRules.EvaluateFrame(
            player.Position,
            0,
            1,
            0f,
            _weapon.orbitRadius,
            _weapon.orbitHeightOffset,
            _weapon.orbitHitRadius,
            player.VisualOrbitUp);
        Assert.That(view, Is.Not.Null);
        AssertPosition(view.transform.position, expected.Position);
        AssertDirection(view.transform.up, player.VisualOrbitUp);
    }

    [Test]
    public void HoopDamageUsesTiltedOrbitPosition()
    {
        _weapon.orbitRadius = 2f;
        _weapon.orbitHeightOffset = 0.9f;
        _weapon.orbitHitRadius = 0.6f;
        _weapon.maxOrbitHitRadius = 0.6f;
        _weapon.projectileDamage = 6;
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(_config);
        Vector3 groundNormal = Quaternion.AngleAxis(20f, Vector3.right) * Vector3.up;
        player.GetComponent<ActorBodyView>().ApplyGroundTilt(groundNormal, 20f, 0f, 0.016f);
        OrbitWeaponFrame frame = OrbitWeaponRules.EvaluateFrame(
            player.Position,
            0,
            1,
            0f,
            _weapon.orbitRadius,
            _weapon.orbitHeightOffset,
            _weapon.orbitHitRadius,
            player.VisualOrbitUp);
        EnemyRuntime enemy = _enemyFactory.Spawn(frame.Position - Vector3.up * _config.enemyHurtboxHeightOffset, level: 1, elapsedSeconds: 0f);
        OrbitWeaponSystem system = CreateSystem(player);
        Physics.SyncTransforms();

        system.Tick();

        Assert.That(enemy.CurrentHealth, Is.EqualTo(enemy.MaxHealth - _weapon.projectileDamage));
    }

    [Test]
    public void FireHoopDefaultsCreateTwoOrbitViews()
    {
        WeaponDefinition fireHoop = TheCircussyOneTestObjects.CreateWeaponDefinition(_config);
        fireHoop.ApplyDefaults(FirstPartyWeaponDefaults.GetOrDefault(FirstPartyWeaponDefaults.FireHoopId));
        var catalog = TheCircussyOneTestObjects.CreateWeaponCatalog(fireHoop);
        var loadout = new WeaponLoadout(catalog);
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(_config);
        Transform root = TheCircussyOneTestObjects.CreateRoot("Default Orbit Weapons").transform;
        var feedback = new FeedbackService(_config, _damageFeedbackConfig, _actorMotionConfig, player, null, null, null, null);
        var damageService = new EnemyDamageService(
            _config,
            _damageFeedbackConfig,
            _actorMotionConfig,
            _enemyFactory,
            _xpDropService,
            new GameState(_config, _stats),
            feedback,
            _damageNumbers,
            _vfx);
        var system = new OrbitWeaponSystem(
            loadout,
            player,
            _registry,
            _stats,
            _time,
            new WeaponTargetingQuery(),
            damageService,
            TheCircussyOneTestObjects.CreateOrbitWeaponPrefab(fireHoop),
            root);

        system.Tick();

        Assert.That(fireHoop.baseOrbitCount, Is.EqualTo(2));
        Assert.That(fireHoop.orbitAreaDamageEnabled, Is.True);
        Assert.That(fireHoop.orbitAreaDamageMultiplier, Is.EqualTo(0.55f).Within(0.0001f));
        Assert.That(root.childCount, Is.EqualTo(2));
        system.Dispose();
        TheCircussyOneTestObjects.Destroy(catalog);
        TheCircussyOneTestObjects.Destroy(fireHoop);
    }

    [Test]
    public void ClearingLoadoutImmediatelyReleasesOrbitViews()
    {
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(_config);
        Transform root = TheCircussyOneTestObjects.CreateRoot("Orbit Cleanup Root").transform;
        OrbitWeaponSystem system = CreateSystem(player, root: root);
        system.Tick();

        _loadout.Clear();

        Assert.That(system.ActiveRuntimeCount, Is.Zero);
        Assert.That(root.childCount, Is.GreaterThanOrEqualTo(1));
        for (int i = 0; i < root.childCount; i++)
        {
            Assert.That(root.GetChild(i).gameObject.activeSelf, Is.False);
        }
    }

    [Test]
    public void DisposeIgnoresDestroyedPooledOrbitViews()
    {
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(_config);
        Transform root = TheCircussyOneTestObjects.CreateRoot("Destroyed Orbit Cleanup Root").transform;
        OrbitWeaponSystem system = CreateSystem(player, root: root);
        system.Tick();

        _loadout.Clear();
        OrbitWeaponView view = root.GetComponentInChildren<OrbitWeaponView>(includeInactive: true);
        Assert.That(view, Is.Not.Null);
        Object.DestroyImmediate(view.gameObject);

        Assert.DoesNotThrow(() => system.Dispose());
    }

    [Test]
    public void PrewarmViewsCreatesRequestedInactivePool()
    {
        Transform root = TheCircussyOneTestObjects.CreateRoot("Orbit Prewarm Root").transform;
        var system = new OrbitWeaponSystem(
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            TheCircussyOneTestObjects.CreateOrbitWeaponPrefab(_weapon),
            root);

        int created = system.PrewarmViews(3, 10);
        int createdAgain = system.PrewarmViews(3, 10);

        Assert.That(created, Is.EqualTo(3));
        Assert.That(createdAgain, Is.Zero);
        Assert.That(system.PooledCount, Is.EqualTo(3));
        Assert.That(root.childCount, Is.EqualTo(3));
        for (int i = 0; i < root.childCount; i++)
        {
            Assert.That(root.GetChild(i).gameObject.activeSelf, Is.False);
        }

        system.Dispose();
    }

    [Test]
    public void PrewarmAreaTelegraphsCreatesRequestedInactivePool()
    {
        Transform root = TheCircussyOneTestObjects.CreateRoot("Area Telegraph Prewarm Root").transform;
        var system = new OrbitWeaponSystem(
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            root,
            vfxConfig: _vfxConfig,
            areaTelegraphPrefab: TheCircussyOneTestObjects.CreateFireHoopAreaTelegraphPrefab(_vfxConfig));

        int created = system.PrewarmAreaTelegraphs(2, 10);
        int createdAgain = system.PrewarmAreaTelegraphs(2, 10);

        Assert.That(created, Is.EqualTo(2));
        Assert.That(createdAgain, Is.Zero);
        Assert.That(system.PooledAreaTelegraphCount, Is.EqualTo(2));
        Assert.That(root.childCount, Is.EqualTo(2));
        for (int i = 0; i < root.childCount; i++)
        {
            Assert.That(root.GetChild(i).gameObject.activeSelf, Is.False);
        }

        system.Dispose();
    }

    [Test]
    public void ReplacingLoadoutWhilePausedClearsStaleHoops()
    {
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(_config);
        Transform root = TheCircussyOneTestObjects.CreateRoot("Paused Orbit Cleanup Root").transform;
        var pauseState = new RunPauseState();
        OrbitWeaponSystem system = CreateSystem(player, root: root, pauseState: pauseState);
        system.Tick();
        pauseState.Pause("test");
        WeaponDefinition cannon = TheCircussyOneTestObjects.CreateWeaponDefinition(_config);
        cannon.ApplyDefaults(FirstPartyWeaponDefaults.GetOrDefault(FirstPartyWeaponDefaults.CannonId));

        _loadout.SetStartingWeapons(cannon);
        system.Tick();

        Assert.That(system.ActiveRuntimeCount, Is.Zero);
        for (int i = 0; i < root.childCount; i++)
        {
            Assert.That(root.GetChild(i).gameObject.activeSelf, Is.False);
        }

        TheCircussyOneTestObjects.Destroy(cannon);
    }

    [Test]
    public void AreaHeatDamagesEnemiesInsideOrbitZoneOnSeparateInterval()
    {
        _weapon.projectileDamage = 10;
        _weapon.orbitAreaDamageEnabled = true;
        _weapon.orbitAreaDamageMultiplier = 0.5f;
        _weapon.orbitAreaHitIntervalSeconds = 0.45f;
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(_config);
        EnemyRuntime enemy = _enemyFactory.Spawn(new Vector3(-1.8f, 0f, 0f), level: 1, elapsedSeconds: 0f);
        OrbitWeaponSystem system = CreateSystem(player);
        Physics.SyncTransforms();

        system.Tick();
        int healthAfterFirstHeat = enemy.CurrentHealth;
        _time.Time = 0.2f;
        system.Tick();
        int healthDuringCooldown = enemy.CurrentHealth;
        _time.Time = 0.5f;
        system.Tick();

        Assert.That(healthAfterFirstHeat, Is.EqualTo(enemy.MaxHealth - 5));
        Assert.That(healthDuringCooldown, Is.EqualTo(healthAfterFirstHeat));
        Assert.That(enemy.CurrentHealth, Is.EqualTo(enemy.MaxHealth - 10));
        Assert.That(_damageNumbers.Count, Is.EqualTo(2));
    }

    [Test]
    public void AreaHeatCreatesOneGroundTelegraphUsingEvaluatedRadius()
    {
        _weapon.projectileDamage = 10;
        _weapon.orbitAreaDamageEnabled = true;
        _weapon.orbitAreaDamageMultiplier = 0.5f;
        _weapon.orbitAreaHitIntervalSeconds = 0.45f;
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(_config);
        Transform root = TheCircussyOneTestObjects.CreateRoot("Orbit Telegraph Root").transform;
        OrbitWeaponSystem system = CreateSystem(player, root: root, includeTelegraph: true);

        system.Tick();

        FireHoopAreaTelegraphView telegraph = root.GetComponentInChildren<FireHoopAreaTelegraphView>();
        Assert.That(system.ActiveAreaTelegraphCount, Is.EqualTo(1));
        Assert.That(telegraph, Is.Not.Null);
        Assert.That(telegraph.Radius, Is.EqualTo(_weapon.orbitRadius + _weapon.orbitHitRadius).Within(0.0001f));
    }

    [Test]
    public void ClearingLoadoutReleasesAreaTelegraph()
    {
        _weapon.projectileDamage = 10;
        _weapon.orbitAreaDamageEnabled = true;
        _weapon.orbitAreaDamageMultiplier = 0.5f;
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(_config);
        Transform root = TheCircussyOneTestObjects.CreateRoot("Orbit Telegraph Cleanup Root").transform;
        OrbitWeaponSystem system = CreateSystem(player, root: root, includeTelegraph: true);
        system.Tick();

        _loadout.Clear();

        Assert.That(system.ActiveAreaTelegraphCount, Is.Zero);
        FireHoopAreaTelegraphView telegraph = root.GetComponentInChildren<FireHoopAreaTelegraphView>(includeInactive: true);
        Assert.That(telegraph, Is.Not.Null);
        Assert.That(telegraph.IsActive, Is.False);
    }

    [Test]
    public void AreaHeatDoesNotSuppressDirectHoopContactDamage()
    {
        _weapon.projectileDamage = 10;
        _weapon.orbitAreaDamageEnabled = true;
        _weapon.orbitAreaDamageMultiplier = 0.5f;
        _weapon.orbitAreaHitIntervalSeconds = 1f;
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(_config);
        EnemyRuntime enemy = _enemyFactory.Spawn(new Vector3(2.2f, 0f, 0f), level: 1, elapsedSeconds: 0f);
        OrbitWeaponSystem system = CreateSystem(player);
        Physics.SyncTransforms();

        system.Tick();

        Assert.That(enemy.CurrentHealth, Is.EqualTo(enemy.MaxHealth - 15));
        Assert.That(_damageNumbers.Count, Is.EqualTo(2));
    }

    [Test]
    public void AreaHeatRespectsLineOfSightBlockers()
    {
        _weapon.projectileDamage = 10;
        _weapon.orbitAreaDamageEnabled = true;
        _weapon.orbitAreaDamageMultiplier = 0.5f;
        _weapon.lineOfSightMask = 1 << 0;
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(_config);
        EnemyRuntime enemy = _enemyFactory.Spawn(new Vector3(-1.8f, 0f, 0f), level: 1, elapsedSeconds: 0f);
        CreateWall("Orbit Area Heat Blocker", new Vector3(-0.9f, 0.9f, 0f), new Vector3(0.12f, 2f, 2f));
        OrbitWeaponSystem system = CreateSystem(player);
        Physics.SyncTransforms();

        system.Tick();

        Assert.That(enemy.CurrentHealth, Is.EqualTo(enemy.MaxHealth));
        Assert.That(_damageNumbers.Count, Is.Zero);
    }

    [Test]
    public void HoopDamagesHurtboxAndRespectsSharedHitInterval()
    {
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(_config);
        EnemyRuntime enemy = _enemyFactory.Spawn(new Vector3(2.2f, 0f, 0f), level: 1, elapsedSeconds: 0f);
        OrbitWeaponSystem system = CreateSystem(player);
        Physics.SyncTransforms();

        system.Tick();
        int healthAfterFirstHit = enemy.CurrentHealth;
        _time.Time = 0.2f;
        system.Tick();
        _time.Time = 0.7f;
        system.Tick();

        Assert.That(healthAfterFirstHit, Is.EqualTo(enemy.MaxHealth - _weapon.projectileDamage));
        Assert.That(enemy.CurrentHealth, Is.EqualTo(enemy.MaxHealth - _weapon.projectileDamage * 2));
        Assert.That(_damageNumbers.Count, Is.EqualTo(2));
    }

    [Test]
    public void MultipleHoopsDoNotStackDamageOnSameEnemyBeforeCooldownExpires()
    {
        _weapon.baseOrbitCount = 2;
        _weapon.maxOrbitCount = 2;
        _weapon.orbitHitRadius = 3f;
        _weapon.maxOrbitHitRadius = 3f;
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(_config);
        EnemyRuntime enemy = _enemyFactory.Spawn(new Vector3(0f, 0f, 0f), level: 1, elapsedSeconds: 0f);
        OrbitWeaponSystem system = CreateSystem(player);
        Physics.SyncTransforms();

        system.Tick();

        Assert.That(enemy.CurrentHealth, Is.EqualTo(enemy.MaxHealth - _weapon.projectileDamage));
        Assert.That(_damageNumbers.Count, Is.EqualTo(1));
    }

    [Test]
    public void HoopIgnoresContactAndMovementHitboxes()
    {
        _config.enemyHurtboxRadius = 0.05f;
        _config.enemyHurtboxHeightOffset = 4f;
        _config.enemyContactHitboxSize = new Vector3(1f, 1f, 1f);
        _config.enemyContactHitboxOffset = new Vector3(0f, 0.9f, 0f);
        _config.enemyMovementBodyRadius = 1f;
        _config.enemyMovementBodyHeight = 1.6f;
        _config.enemyMovementBodyOffset = new Vector3(0f, 0.8f, 0f);
        _weapon.orbitRadius = 0f;
        _weapon.orbitHitRadius = 0.75f;
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(_config);
        EnemyRuntime enemy = _enemyFactory.Spawn(Vector3.zero, level: 1, elapsedSeconds: 0f);
        OrbitWeaponSystem system = CreateSystem(player);
        Physics.SyncTransforms();

        system.Tick();

        Assert.That(enemy.CurrentHealth, Is.EqualTo(enemy.MaxHealth));
        Assert.That(_damageNumbers.Count, Is.Zero);
    }

    [Test]
    public void HoopRespectsLineOfSightBlockers()
    {
        _weapon.lineOfSightMask = 1 << 0;
        _weapon.orbitHitRadius = 1.2f;
        _weapon.maxOrbitHitRadius = 1.2f;
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(_config);
        EnemyRuntime enemy = _enemyFactory.Spawn(new Vector3(3f, 0f, 0f), level: 1, elapsedSeconds: 0f);
        CreateWall("Orbit Line Of Sight Blocker", new Vector3(2.6f, 0.9f, 0f), new Vector3(0.12f, 2f, 2f));
        OrbitWeaponSystem system = CreateSystem(player);
        Physics.SyncTransforms();

        system.Tick();

        Assert.That(enemy.CurrentHealth, Is.EqualTo(enemy.MaxHealth));
        Assert.That(_damageNumbers.Count, Is.Zero);
    }

    [Test]
    public void LethalHoopHitSpawnsKillFeedbackAndXpDrops()
    {
        _weapon.projectileDamage = _config.enemyHealth;
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(_config);
        var state = new GameState(_config, _stats);
        _enemyFactory.Spawn(new Vector3(2.2f, 0f, 0f), level: 1, elapsedSeconds: 0f);
        OrbitWeaponSystem system = CreateSystem(player, state: state);
        Physics.SyncTransforms();

        system.Tick();

        Assert.That(state.Kills, Is.EqualTo(1));
        Assert.That(_registry.Enemies, Is.Empty);
        Assert.That(_registry.Pickups, Has.Count.EqualTo(1));
        Assert.That(_damageNumbers.Count, Is.EqualTo(1));
        Assert.That(_vfx.Calls, Has.Count.EqualTo(2));
        Assert.That(_vfx.Calls[0].effectId, Is.EqualTo(VfxEffectId.EnemyHitSparks));
        Assert.That(_vfx.Calls[1].effectId, Is.EqualTo(VfxEffectId.EnemyDeathBurst));
    }

    private OrbitWeaponSystem CreateSystem(PlayerView player, Transform root = null, GameState state = null, RunPauseState pauseState = null, bool includeTelegraph = false)
    {
        state ??= new GameState(_config, _stats);
        var feedback = new FeedbackService(_config, _damageFeedbackConfig, _actorMotionConfig, player, null, null, null, null);
        var damageService = new EnemyDamageService(
            _config,
            _damageFeedbackConfig,
            _actorMotionConfig,
            _enemyFactory,
            _xpDropService,
            state,
            feedback,
            _damageNumbers,
            _vfx);
        return new OrbitWeaponSystem(
            _loadout,
            player,
            _registry,
            _stats,
            _time,
            new WeaponTargetingQuery(),
            damageService,
            TheCircussyOneTestObjects.CreateOrbitWeaponPrefab(_weapon),
            root != null ? root : TheCircussyOneTestObjects.CreateRoot("Orbit Root").transform,
            pauseState,
            _vfxConfig,
            includeTelegraph ? TheCircussyOneTestObjects.CreateFireHoopAreaTelegraphPrefab(_vfxConfig) : null);
    }

    private static GameObject CreateWall(string name, Vector3 position, Vector3 scale)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = "TCO Test " + name;
        wall.transform.position = position;
        wall.transform.localScale = scale;
        return wall;
    }

    private static void AssertPosition(Vector3 actual, Vector3 expected)
    {
        Assert.That(actual.x, Is.EqualTo(expected.x).Within(0.0001f));
        Assert.That(actual.y, Is.EqualTo(expected.y).Within(0.0001f));
        Assert.That(actual.z, Is.EqualTo(expected.z).Within(0.0001f));
    }

    private static void AssertDirection(Vector3 actual, Vector3 expected)
    {
        Assert.That(Vector3.Angle(actual.normalized, expected.normalized), Is.LessThan(0.001f));
    }
}
