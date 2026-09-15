using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Runtime;
using TheCircussyOne.Stats;
using TheCircussyOne.Visuals;

public sealed class RunRestartTests
{
    private GameConfig config;
    private readonly List<Object> trackedObjects = new();

    [TearDown]
    public void TearDown()
    {
        TheCircussyOneTestObjects.DestroyAll();
        for (int i = 0; i < trackedObjects.Count; i++)
        {
            TheCircussyOneTestObjects.Destroy(trackedObjects[i]);
        }

        trackedObjects.Clear();
        TheCircussyOneTestObjects.Destroy(config);
    }

    [Test]
    public void SceneRunRestarterLoadsCurrentSceneReferenceOnce()
    {
        int loadCount = 0;
        string loadedScene = null;
        bool prepared = false;
        bool preparedBeforeLoad = false;
        var restarter = new SceneRunRestarter(
            () => "Assets/Game/Scenes/TheCircussyOne.unity",
            scene =>
            {
                preparedBeforeLoad = prepared;
                loadCount++;
                loadedScene = scene;
            },
            () => prepared = true);

        restarter.RestartRun();
        restarter.RestartRun();

        Assert.That(loadCount, Is.EqualTo(1));
        Assert.That(loadedScene, Is.EqualTo("Assets/Game/Scenes/TheCircussyOne.unity"));
        Assert.That(preparedBeforeLoad, Is.True);
    }

    [Test]
    public void SceneRunRestarterRoutesSceneReloadThroughTransition()
    {
        int loadCount = 0;
        string loadedScene = null;
        bool prepared = false;
        bool preparedBeforeLoad = false;
        var transition = new FakeRunTransitionService();
        var restarter = new SceneRunRestarter(
            () => "Assets/Game/Scenes/TheCircussyOne.unity",
            scene =>
            {
                preparedBeforeLoad = prepared;
                loadCount++;
                loadedScene = scene;
                return null;
            },
            () => prepared = true,
            transition);

        restarter.RestartRun();

        Assert.That(transition.SceneReloadCount, Is.EqualTo(1));
        Assert.That(transition.LastTitle, Is.EqualTo("Setting the Stage"));
        Assert.That(loadCount, Is.EqualTo(1));
        Assert.That(loadedScene, Is.EqualTo("Assets/Game/Scenes/TheCircussyOne.unity"));
        Assert.That(preparedBeforeLoad, Is.True);
    }

    [Test]
    public void SceneRunRestarterUsesResettingStageForDeathRetry()
    {
        var transition = new FakeRunTransitionService();
        var restarter = new SceneRunRestarter(
            () => "Assets/Game/Scenes/TheCircussyOne.unity",
            _ => null,
            transitionService: transition);

        restarter.RestartRun(RunRestartReason.RetryAfterDeath);

        Assert.That(transition.SceneReloadCount, Is.EqualTo(1));
        Assert.That(transition.LastTitle, Is.EqualTo("Resetting the Stage"));
    }

    [Test]
    public void SceneRunRestarterRunsResetCoordinatorBeforeSceneLoad()
    {
        config = TheCircussyOneTestObjects.CreateConfig();
        bool loadSawUnpaused = false;
        var pauseState = new RunPauseState();
        var coordinator = CreateResetCoordinator(
            config,
            pauseState: pauseState,
            phaseState: new RunPhaseState(),
            stats: new RunStats(config),
            loadout: new WeaponLoadout(null),
            inventory: new ItemInventory(),
            currency: new RunCurrencyState(),
            upgradeRunState: new UpgradeRunState(),
            talentRunState: new TalentRunState(),
            gameState: new GameState(config));
        var restarter = new SceneRunRestarter(
            () => "Assets/Game/Scenes/TheCircussyOne.unity",
            _ =>
            {
                loadSawUnpaused = !pauseState.IsPaused;
                return null;
            });
        restarter.SetResetCoordinator(coordinator);

        pauseState.Pause(RunPauseReasons.PauseMenu);
        restarter.RestartRun();

        Assert.That(loadSawUnpaused, Is.True);
    }

    [Test]
    public void SceneRunRestarterIgnoresMissingSceneReference()
    {
        int loadCount = 0;
        var restarter = new SceneRunRestarter(() => string.Empty, _ => loadCount++);

        restarter.RestartRun();

        Assert.That(loadCount, Is.Zero);
    }

    [Test]
    public void SceneRunRestarterCanRetryAfterLoadThrows()
    {
        int attempts = 0;
        var restarter = new SceneRunRestarter(
            () => "Assets/Game/Scenes/TheCircussyOne.unity",
            _ =>
            {
                attempts++;
                if (attempts == 1)
                {
                    throw new System.InvalidOperationException("load failed");
                }
            });

        Assert.Throws<System.InvalidOperationException>(() => restarter.RestartRun());
        restarter.RestartRun();

        Assert.That(attempts, Is.EqualTo(2));
    }

    [Test]
    public void DebugRunRestartSystemRestartsOnlyWhenDebugInputIsPressed()
    {
        var input = new FakeInputService();
        var restarter = new FakeRunRestarter();
        var system = new DebugRunRestartSystem(input, restarter);

        system.Tick();
        input.DebugRestartPressedThisFrame = true;
        system.Tick();

        Assert.That(restarter.RestartCount, Is.EqualTo(1));
        Assert.That(system, Is.InstanceOf<ITickableWhenPaused>());
    }

    [Test]
    public void DebugRunRestartSystemIgnoresInputDuringRunTransition()
    {
        var input = new FakeInputService { DebugRestartPressedThisFrame = true };
        var restarter = new FakeRunRestarter();
        var pauseState = new RunPauseState();
        pauseState.Pause(RunPauseReasons.RunTransition);
        var system = new DebugRunRestartSystem(input, restarter, pauseState);

        system.Tick();

        Assert.That(restarter.RestartCount, Is.Zero);
    }

    [Test]
    public void HudPresenterRestartsRunWhenHudRequestsRestart()
    {
        config = TheCircussyOneTestObjects.CreateConfig();
        var state = new GameState(config);
        var hudObject = TheCircussyOneTestObjects.CreateRoot("Restart HUD");
        var hud = hudObject.AddComponent<HudView>();
        var restarter = new FakeRunRestarter();
        var presenter = new HudPresenter(state, hud, new FakeGameTime(), restarter);

        presenter.Start();
        hud.RequestRestart();
        presenter.Dispose();

        Assert.That(restarter.RestartCount, Is.EqualTo(1));
    }

    [Test]
    public void HudPresenterReturnsToMainMenuAfterGameOver()
    {
        config = TheCircussyOneTestObjects.CreateConfig();
        var state = new GameState(config);
        var hudObject = TheCircussyOneTestObjects.CreateRoot("Death Restart HUD");
        var hud = hudObject.AddComponent<HudView>();
        var restarter = new FakeRunRestarter();
        var mainMenuReturner = new FakeMainMenuReturner();
        var presenter = new HudPresenter(state, hud, new FakeGameTime(), restarter, mainMenuReturner: mainMenuReturner);

        presenter.Start();
        state.DamagePlayer(state.MaxHealth);
        hud.RequestRestart();
        presenter.Dispose();

        Assert.That(restarter.RestartCount, Is.Zero);
        Assert.That(mainMenuReturner.ReturnCount, Is.EqualTo(1));
    }

    [Test]
    public void RunRetryServiceClearsRunStateForPerformerSelection()
    {
        config = TheCircussyOneTestObjects.CreateConfig();
        WeaponDefinition weapon = Track(TheCircussyOneTestObjects.CreateWeaponDefinition(config, "test_weapon", "Test Weapon"));
        PerformerDefinition performer = Track(TheCircussyOneTestObjects.CreatePerformerDefinition("test_performer", "Test Performer", weapon));
        var stats = new RunStats(config);
        WeaponCatalog catalog = Track(TheCircussyOneTestObjects.CreateWeaponCatalog(weapon));
        var loadout = new WeaponLoadout(catalog);
        var inventory = new ItemInventory(stats);
        var currency = new RunCurrencyState();
        var upgradeRunState = new UpgradeRunState();
        var talentRunState = new TalentRunState();
        var gameState = new GameState(config, stats);
        var phaseState = new RunPhaseState();
        var pauseState = new RunPauseState();
        var performerRunState = new PerformerRunState();
        var retry = new RunRetryService(CreateResetCoordinator(
            config,
            pauseState,
            phaseState,
            performerRunState,
            stats,
            loadout,
            inventory,
            currency,
            upgradeRunState,
            talentRunState,
            gameState));

        performerRunState.Select(performer);
        stats.AddModifier(new StatModifier(StatId.PlayerArmor, StatModifierBucket.Flat, 5f, "test"));
        currency.AddTickets(20);
        gameState.DamagePlayer(9999);
        phaseState.FailRun();
        pauseState.Pause(RunPauseReasons.GameOver);

        retry.ResetRunStateForRetry();

        Assert.That(performerRunState.IsSelectionComplete, Is.False);
        Assert.That(stats.Modifiers, Is.Empty);
        Assert.That(loadout.WeaponCount, Is.Zero);
        Assert.That(currency.Tickets, Is.Zero);
        Assert.That(gameState.IsGameOver, Is.False);
        Assert.That(phaseState.CurrentPhase, Is.EqualTo(RunPhase.PerformerSelection));
        Assert.That(pauseState.HasReason(RunPauseReasons.GameOver), Is.False);
    }

    [Test]
    public void RunRetryServiceResetsEnemySpawnDifficultyState()
    {
        config = TheCircussyOneTestObjects.CreateConfig();
        config.spawnIntervalSeconds = 1f;
        config.minSpawnIntervalSeconds = 0.1f;
        config.spawnRampDurationSeconds = 1f;
        var registry = new ActorRegistry();
        var time = new FakeGameTime { DeltaTime = 10f };
        DamageFeedbackVisualConfig damageFeedbackConfig = Track(TheCircussyOneTestObjects.CreateDamageFeedbackConfig());
        EnemySpawnVisualConfig spawnVisualConfig = Track(TheCircussyOneTestObjects.CreateEnemySpawnVisualConfig());
        EnemyFactory enemyFactory = new EnemyFactory(
            config,
            damageFeedbackConfig,
            registry,
            TheCircussyOneTestObjects.CreateEnemyPrefab(),
            TheCircussyOneTestObjects.CreateRoot("Retry Enemies").transform);
        var indicatorFactory = new EnemySpawnIndicatorFactory(
            spawnVisualConfig,
            TheCircussyOneTestObjects.CreateEnemySpawnIndicatorPrefab(),
            TheCircussyOneTestObjects.CreateRoot("Retry Spawn Indicators").transform);
        var gameState = new GameState(config);
        var indicatorSystem = new EnemySpawnIndicatorSystem(spawnVisualConfig, indicatorFactory, enemyFactory, gameState, time);
        var pauseState = new RunPauseState();
        var phaseState = new RunPhaseState();
        var performerRunState = new PerformerRunState();
        var stats = new RunStats(config);
        var loadout = new WeaponLoadout(null);
        var inventory = new ItemInventory(stats);
        var currency = new RunCurrencyState();
        var upgradeRunState = new UpgradeRunState();
        var talentRunState = new TalentRunState();
        EnemySpawnSystem spawnSystem = new EnemySpawnSystem(
            config,
            TheCircussyOneTestObjects.CreatePlayer(config),
            registry,
            indicatorSystem,
            gameState,
            time);
        var retry = new RunRetryService(CreateResetCoordinator(
            config,
            pauseState,
            phaseState,
            performerRunState,
            stats,
            loadout,
            inventory,
            currency,
            upgradeRunState,
            talentRunState,
            gameState,
            enemySpawnSystem: spawnSystem));

        spawnSystem.Tick();
        Assert.That(indicatorFactory.Active, Has.Count.EqualTo(1));
        indicatorSystem.Clear();

        retry.ResetRunStateForRetry();
        time.DeltaTime = 0.25f;
        spawnSystem.Tick();

        Assert.That(indicatorFactory.Active, Is.Empty);
    }

    [Test]
    public void RunResetCoordinatorWorldTransitionPreservesRunProgressionAndBuild()
    {
        config = TheCircussyOneTestObjects.CreateConfig();
        WeaponDefinition weapon = Track(TheCircussyOneTestObjects.CreateWeaponDefinition(config, "juggling_ball", "Juggling Ball"));
        var stats = new RunStats(config);
        var loadout = new WeaponLoadout(null);
        var inventory = new ItemInventory(stats);
        var currency = new RunCurrencyState();
        var gameState = new GameState(config, stats);
        var phaseState = new RunPhaseState();
        var upgradeRunState = new UpgradeRunState();
        var talentRunState = new TalentRunState();
        var coordinator = CreateResetCoordinator(
            config,
            new RunPauseState(),
            phaseState,
            new PerformerRunState(),
            stats,
            loadout,
            inventory,
            currency,
            upgradeRunState,
            talentRunState,
            gameState);

        Assert.That(loadout.AddWeapon(weapon), Is.True);
        currency.AddTickets(37);
        gameState.AddExperience(gameState.ExperienceTarget);
        phaseState.BeginWorld();

        coordinator.ResetForWorldTransition(2);

        Assert.That(loadout.WeaponCount, Is.EqualTo(1));
        Assert.That(currency.Tickets, Is.EqualTo(37));
        Assert.That(gameState.Level, Is.GreaterThan(1));
        Assert.That(phaseState.CurrentPhase, Is.EqualTo(RunPhase.WorldActive));
    }

    private T Track<T>(T obj) where T : Object
    {
        if (obj != null)
        {
            trackedObjects.Add(obj);
        }

        return obj;
    }

    private static RunResetCoordinator CreateResetCoordinator(
        GameConfig config,
        RunPauseState pauseState,
        RunPhaseState phaseState,
        PerformerRunState performerRunState = null,
        RunStats stats = null,
        WeaponLoadout loadout = null,
        ItemInventory inventory = null,
        RunCurrencyState currency = null,
        UpgradeRunState upgradeRunState = null,
        TalentRunState talentRunState = null,
        GameState gameState = null,
        EnemySpawnSystem enemySpawnSystem = null)
    {
        stats ??= new RunStats(config);
        loadout ??= new WeaponLoadout(null);
        inventory ??= new ItemInventory(stats);
        currency ??= new RunCurrencyState();
        upgradeRunState ??= new UpgradeRunState();
        talentRunState ??= new TalentRunState();
        gameState ??= new GameState(config, stats);
        return new RunResetCoordinator(
            pauseState,
            phaseState,
            performerRunState ?? new PerformerRunState(),
            stats,
            loadout,
            inventory,
            currency,
            upgradeRunState,
            talentRunState,
            gameState,
            enemySpawnSystem: enemySpawnSystem);
    }
}
