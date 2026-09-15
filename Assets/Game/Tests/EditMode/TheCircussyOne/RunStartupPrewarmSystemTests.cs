using System.Collections;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using TheCircussyOne.Config;
using TheCircussyOne.Runtime;
using TheCircussyOne.Visuals;
using UnityEngine;
using UnityEngine.TestTools;

public sealed class RunStartupPrewarmSystemTests
{
    [SetUp]
    public void SetUp()
    {
        TheCircussyOneTestObjects.DestroyAll();
    }

    [TearDown]
    public void TearDown()
    {
        TheCircussyOneTestObjects.DestroyAll();
    }

    [Test]
    public void ImmediatePrewarmFillsConfiguredPools()
    {
        GameConfig config = CreatePrewarmConfig();
        config.runtimePrewarmProjectileViews = 2;
        config.runtimePrewarmPickupViews = 3;
        config.runtimePrewarmWorldPrompt = true;
        config.runtimePrewarmGainCounters = true;
        var registry = new ActorRegistry();
        DamageFeedbackVisualConfig damageConfig = TheCircussyOneTestObjects.CreateDamageFeedbackConfig();
        var projectileFactory = new ProjectileFactory(
            damageConfig,
            registry,
            new FakeGameTime(),
            TheCircussyOneTestObjects.CreateProjectilePrefab(),
            TheCircussyOneTestObjects.CreateRoot("Projectiles").transform);
        var pickupFactory = new PickupFactory(
            config,
            registry,
            TheCircussyOneTestObjects.CreatePickupPrefab(),
            TheCircussyOneTestObjects.CreateRoot("Pickups").transform);
        WorldInteractionPromptVisualConfig promptConfig = TheCircussyOneTestObjects.CreateWorldInteractionPromptConfig();
        var promptFactory = new WorldInteractionPromptFactory(
            promptConfig,
            TheCircussyOneTestObjects.CreateWorldInteractionPromptPrefab(promptConfig),
            TheCircussyOneTestObjects.CreateRoot("Prompt").transform);
        XpGainCounterVisualConfig gainConfig = TheCircussyOneTestObjects.CreateXpGainCounterConfig();
        XpGainCounterView gainPrefab = TheCircussyOneTestObjects.CreateXpGainCounterPrefab();
        var xpGainFactory = new XpGainCounterFactory(
            gainConfig,
            gainPrefab,
            TheCircussyOneTestObjects.CreateRoot("XP Gain Counter").transform);
        var ticketGainFactory = new XpGainCounterFactory(
            gainConfig,
            gainPrefab,
            TheCircussyOneTestObjects.CreateRoot("Ticket Gain Counter").transform,
            "Ticket Gain Counter");
        var xpGainCounter = new XpGainCounterSystem(gainConfig, null, null, new FakeGameTime(), xpGainFactory);
        var ticketGainCounter = new TicketGainCounterSystem(gainConfig, new RunCurrencyState(), null, null, new FakeGameTime(), ticketGainFactory);
        var system = new RunStartupPrewarmSystem(
            config,
            projectileFactory: projectileFactory,
            pickupFactory: pickupFactory,
            worldInteractionPromptFactory: promptFactory,
            xpGainCounterSystem: xpGainCounter,
            ticketGainCounterSystem: ticketGainCounter);

        system.PrewarmImmediate();

        Assert.That(projectileFactory.PooledCount, Is.EqualTo(2));
        Assert.That(pickupFactory.PooledCount, Is.EqualTo(3));
        Assert.That(promptFactory.View, Is.Not.Null);
        Assert.That(promptFactory.View.IsActive, Is.False);
        Assert.That(xpGainFactory.View, Is.Not.Null);
        Assert.That(xpGainFactory.View.IsActive, Is.False);
        Assert.That(ticketGainFactory.View, Is.Not.Null);
        Assert.That(ticketGainFactory.View.IsActive, Is.False);

        Object.DestroyImmediate(config);
        Object.DestroyImmediate(damageConfig);
        Object.DestroyImmediate(promptConfig);
        Object.DestroyImmediate(gainConfig);
    }

    [UnityTest]
    public IEnumerator AsyncPrewarmFillsSlicedPools()
    {
        GameConfig config = CreatePrewarmConfig();
        config.runtimePrewarmObjectsPerFrame = 1;
        config.runtimePrewarmDamageNumbers = 2;
        config.runtimePrewarmProjectileChainSegments = 2;
        config.runtimePrewarmTicketBurstVfx = 2;
        config.runtimePrewarmSnackBurstVfx = 1;
        DamageFeedbackVisualConfig damageConfig = TheCircussyOneTestObjects.CreateDamageFeedbackConfig();
        var damageNumbers = new DamageNumberFactory(
            damageConfig,
            TheCircussyOneTestObjects.CreateDamageNumberPrefab(),
            TheCircussyOneTestObjects.CreateRoot("Damage Numbers").transform);
        var chains = new ProjectileChainVisualSystem(TheCircussyOneTestObjects.CreateRoot("Chains").transform, new FakeGameTime());
        var rewardBursts = new WorldRewardTransientVfxSystem(
            new FakeGameTime(),
            new WorldRewardTransientVfxFactory(TheCircussyOneTestObjects.CreateRoot("Reward Bursts").transform));
        var system = new RunStartupPrewarmSystem(
            config,
            damageNumberFactory: damageNumbers,
            projectileChainVisualSystem: chains,
            rewardTransientVfxSystem: rewardBursts);

        yield return system.PrewarmAsync().ToCoroutine();

        Assert.That(damageNumbers.PooledCount, Is.EqualTo(2));
        Assert.That(chains.PooledCount, Is.EqualTo(2));
        Assert.That(rewardBursts.PooledTicketBurstCount, Is.EqualTo(2));
        Assert.That(rewardBursts.PooledSnackBurstCount, Is.EqualTo(1));

        Object.DestroyImmediate(config);
        Object.DestroyImmediate(damageConfig);
    }

    [Test]
    public void LifecyclePrewarmsBeforeSyncTransitionReturns()
    {
        GameConfig config = CreatePrewarmConfig();
        config.runtimePrewarmProjectileViews = 2;
        var registry = new ActorRegistry();
        DamageFeedbackVisualConfig damageConfig = TheCircussyOneTestObjects.CreateDamageFeedbackConfig();
        var projectileFactory = new ProjectileFactory(
            damageConfig,
            registry,
            new FakeGameTime(),
            TheCircussyOneTestObjects.CreateProjectilePrefab(),
            TheCircussyOneTestObjects.CreateRoot("Lifecycle Projectiles").transform);
        var prewarm = new RunStartupPrewarmSystem(config, projectileFactory: projectileFactory);
        var generator = new CapturingRunWorldGenerator();
        var lifecycle = new RunWorldLifecycleSystem(
            registry,
            null,
            projectileFactory,
            null,
            null,
            null,
            null,
            new WorldPropPlacementService(),
            new WorldRewardRuntimeRegistry(),
            generator,
            null,
            prewarmSystem: prewarm);

        lifecycle.TransitionToWorld(1, 123);

        Assert.That(generator.Request.Seed, Is.EqualTo(123));
        Assert.That(projectileFactory.PooledCount, Is.EqualTo(2));

        Object.DestroyImmediate(config);
        Object.DestroyImmediate(damageConfig);
    }

    private static GameConfig CreatePrewarmConfig()
    {
        GameConfig config = TheCircussyOneTestObjects.CreateConfig();
        config.runtimePrewarmEnabled = true;
        config.runtimePrewarmObjectsPerFrame = 8;
        config.runtimePrewarmEnemyViews = 0;
        config.runtimePrewarmProjectileViews = 0;
        config.runtimePrewarmPickupViews = 0;
        config.runtimePrewarmDamageNumbers = 0;
        config.runtimePrewarmProjectileExplosions = 0;
        config.runtimePrewarmProjectileChainSegments = 0;
        config.runtimePrewarmOrbitWeaponViews = 0;
        config.runtimePrewarmFireHoopAreaTelegraphs = 0;
        config.runtimePrewarmTicketBurstVfx = 0;
        config.runtimePrewarmSnackBurstVfx = 0;
        config.runtimePrewarmWorldPrompt = false;
        config.runtimePrewarmGainCounters = false;
        return config;
    }

    private sealed class CapturingRunWorldGenerator : IRunWorldGenerator
    {
        public WorldGenerationRequest Request { get; private set; }

        public WorldGenerationResult Generate(WorldGenerationRequest request)
        {
            Request = request;
            return new WorldGenerationResult(request.WorldIndex, request.ActNumber, request.Seed, usedPrototypePlacement: false);
        }
    }
}
