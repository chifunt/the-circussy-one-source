using NUnit.Framework;
using System.Linq;
using UnityEngine;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Runtime;
using TheCircussyOne.Stats;

public sealed class PickupSystemTests
{
    private GameConfig _config;
    private ActorRegistry _registry;

    [SetUp]
    public void SetUp()
    {
        TheCircussyOneTestObjects.DestroyAll();
        _config = TheCircussyOneTestObjects.CreateConfig();
        _registry = new ActorRegistry();
    }

    [TearDown]
    public void TearDown()
    {
        TheCircussyOneTestObjects.DestroyAll();
        TheCircussyOneTestObjects.Destroy(_config);
    }

    [Test]
    public void PickupInsideMagnetRadiusAttractsThenCollects()
    {
        var player = TheCircussyOneTestObjects.CreatePlayer(_config);
        var state = new GameState(_config);
        state.Reset();
        var pickupFactory = new PickupFactory(
            _config,
            _registry,
            TheCircussyOneTestObjects.CreatePickupPrefab(),
            TheCircussyOneTestObjects.CreateRoot("Pickups").transform);
        PickupRuntime pickup = pickupFactory.Spawn(new Vector3(_config.pickupCollectRadius + 1f, 0f, 0f));
        var vfx = new FakeVfxSpawner();
        var xpCounter = new FakeXpGainCounter();
        var system = new PickupSystem(_config, TheCircussyOneTestObjects.CreateDamageFeedbackConfig(), player, _registry, pickupFactory, state, vfx, xpCounter);

        system.Tick();
        Assert.That(pickup.View.IsAttracting, Is.True);
        Assert.That(_registry.Pickups, Has.Count.EqualTo(1));

        system.Tick();
        Assert.That(_registry.Pickups, Is.Empty);
        Assert.That(state.Experience, Is.EqualTo(XpGemDefinition.DefaultBlueXpAmount));
        Assert.That(xpCounter.Amounts, Is.EqualTo(new[] { XpGemDefinition.DefaultBlueXpAmount }));
        Assert.That(vfx.Calls, Has.Count.EqualTo(1));
        Assert.That(vfx.Calls[0].effectId, Is.EqualTo(VfxEffectId.XpPickupCollectPop));
    }

    [Test]
    public void PickupRadiiUseRunStats()
    {
        var player = TheCircussyOneTestObjects.CreatePlayer(_config);
        var stats = new RunStats(_config);
        stats.AddModifier(StatModifier.Flat(StatId.PickupMagnetRadius, 4f, "magnet"));
        stats.AddModifier(StatModifier.Flat(StatId.PickupCollectRadius, 2f, "collect"));
        var state = new GameState(_config, stats);
        state.Reset();
        var pickupFactory = new PickupFactory(
            _config,
            _registry,
            TheCircussyOneTestObjects.CreatePickupPrefab(),
            TheCircussyOneTestObjects.CreateRoot("Pickups").transform);
        PickupRuntime pickup = pickupFactory.Spawn(new Vector3(_config.pickupCollectRadius + 1.5f, 0f, 0f));
        var system = new PickupSystem(_config, TheCircussyOneTestObjects.CreateDamageFeedbackConfig(), player, _registry, pickupFactory, state, stats: stats);

        system.Tick();

        Assert.That(_registry.Pickups, Is.Empty);
        Assert.That(state.Experience, Is.EqualTo(pickup.Amount));
    }

    [Test]
    public void PickupExperienceGainUsesRunStatsAndReportsGainedAmount()
    {
        _config.experienceCurveEarlyBase = 100;
        var player = TheCircussyOneTestObjects.CreatePlayer(_config);
        var stats = new RunStats(_config);
        stats.AddModifier(StatModifier.AdditivePercent(StatId.XpGainMultiplier, 0.1f, "xp_gain"));
        var state = new GameState(_config, stats);
        state.Reset();
        var pickupFactory = new PickupFactory(
            _config,
            _registry,
            TheCircussyOneTestObjects.CreatePickupPrefab(),
            TheCircussyOneTestObjects.CreateRoot("Pickups").transform);
        XpGemDefinition gem = TheCircussyOneTestObjects.CreateXpGemDefinition("small_xp_gem", "Small XP Gem", 1, Color.cyan);
        var xpCounter = new FakeXpGainCounter();
        var system = new PickupSystem(_config, TheCircussyOneTestObjects.CreateDamageFeedbackConfig(), player, _registry, pickupFactory, state, xpGainCounter: xpCounter, stats: stats);

        for (int i = 0; i < 10; i++)
        {
            pickupFactory.Spawn(gem, Vector3.zero);
            system.Tick();
        }

        Assert.That(_registry.Pickups, Is.Empty);
        Assert.That(state.Experience, Is.EqualTo(11));
        Assert.That(xpCounter.Amounts.Sum(), Is.EqualTo(11));
        Assert.That(xpCounter.Amounts, Does.Contain(2));

        TheCircussyOneTestObjects.Destroy(gem);
    }

    [Test]
    public void DefinitionSpawnedPickupGrantsGemAmountThroughExistingXpFlow()
    {
        _config.experienceCurveEarlyBase = 100;
        var player = TheCircussyOneTestObjects.CreatePlayer(_config);
        var state = new GameState(_config);
        state.Reset();
        var pickupFactory = new PickupFactory(
            _config,
            _registry,
            TheCircussyOneTestObjects.CreatePickupPrefab(),
            TheCircussyOneTestObjects.CreateRoot("Pickups").transform);
        XpGemDefinition gem = TheCircussyOneTestObjects.CreateXpGemDefinition("green_xp_gem", "Green XP Gem", 23, Color.green);
        PickupRuntime pickup = pickupFactory.Spawn(gem, Vector3.zero);
        var xpCounter = new FakeXpGainCounter();
        var system = new PickupSystem(_config, TheCircussyOneTestObjects.CreateDamageFeedbackConfig(), player, _registry, pickupFactory, state, xpGainCounter: xpCounter);

        system.Tick();

        Assert.That(pickup.Gem, Is.SameAs(gem));
        Assert.That(pickup.Amount, Is.EqualTo(23));
        Assert.That(state.Experience, Is.EqualTo(23));
        Assert.That(xpCounter.Amounts, Is.EqualTo(new[] { 23 }));

        TheCircussyOneTestObjects.Destroy(gem);
    }

    [Test]
    public void HealthPickupHealsDamagedPlayerWithoutGrantingExperience()
    {
        var player = TheCircussyOneTestObjects.CreatePlayer(_config);
        var state = new GameState(_config);
        state.Reset();
        state.DamagePlayer(35);
        var pickupFactory = new PickupFactory(
            _config,
            _registry,
            TheCircussyOneTestObjects.CreatePickupPrefab(),
            TheCircussyOneTestObjects.CreateRoot("Pickups").transform);
        HealthPickupDefinition pickupDefinition = TheCircussyOneTestObjects.CreateHealthPickupDefinition(
            "treat",
            "Treat",
            healAmount: 20);
        PickupRuntime pickup = pickupFactory.Spawn(pickupDefinition, Vector3.zero);
        var xpCounter = new FakeXpGainCounter();
        var system = new PickupSystem(_config, TheCircussyOneTestObjects.CreateDamageFeedbackConfig(), player, _registry, pickupFactory, state, xpGainCounter: xpCounter);

        system.Tick();

        Assert.That(pickup.Reward.Kind, Is.EqualTo(PickupRewardKind.Health));
        Assert.That(state.Health, Is.EqualTo(_config.playerMaxHealth - 15));
        Assert.That(state.Experience, Is.Zero);
        Assert.That(xpCounter.Amounts, Is.Empty);
        Assert.That(_registry.Pickups, Is.Empty);

        TheCircussyOneTestObjects.Destroy(pickupDefinition);
    }

    [Test]
    public void HealthPickupHealsPercentOfCurrentMaxHealth()
    {
        var player = TheCircussyOneTestObjects.CreatePlayer(_config);
        var stats = new RunStats(_config);
        stats.AddModifier(StatModifier.Flat(StatId.PlayerMaxHealth, 100f, "test_max_hp"));
        var state = new GameState(_config, stats);
        state.Reset();
        state.DamagePlayer(100);
        var pickupFactory = new PickupFactory(
            _config,
            _registry,
            TheCircussyOneTestObjects.CreatePickupPrefab(),
            TheCircussyOneTestObjects.CreateRoot("Pickups").transform);
        HealthPickupDefinition pickupDefinition = TheCircussyOneTestObjects.CreateHealthPickupDefinition(
            "treat",
            "Treat",
            healPercentOfMaxHealth: 0.2f);
        pickupFactory.Spawn(pickupDefinition, Vector3.zero);
        var system = new PickupSystem(_config, TheCircussyOneTestObjects.CreateDamageFeedbackConfig(), player, _registry, pickupFactory, state, stats: stats);

        system.Tick();

        Assert.That(state.MaxHealth, Is.EqualTo(200));
        Assert.That(state.Health, Is.EqualTo(140));

        TheCircussyOneTestObjects.Destroy(pickupDefinition);
    }

    [Test]
    public void HealthPickupRoundsFractionalPercentHealingUp()
    {
        var player = TheCircussyOneTestObjects.CreatePlayer(_config);
        var stats = new RunStats(_config);
        stats.AddModifier(StatModifier.Flat(StatId.PlayerMaxHealth, 12f, "test_max_hp"));
        var state = new GameState(_config, stats);
        state.Reset();
        state.DamagePlayer(50);
        var pickupFactory = new PickupFactory(
            _config,
            _registry,
            TheCircussyOneTestObjects.CreatePickupPrefab(),
            TheCircussyOneTestObjects.CreateRoot("Pickups").transform);
        HealthPickupDefinition pickupDefinition = TheCircussyOneTestObjects.CreateHealthPickupDefinition(
            "treat",
            "Treat",
            healPercentOfMaxHealth: 0.2f);
        pickupFactory.Spawn(pickupDefinition, Vector3.zero);
        var system = new PickupSystem(_config, TheCircussyOneTestObjects.CreateDamageFeedbackConfig(), player, _registry, pickupFactory, state, stats: stats);

        system.Tick();

        Assert.That(state.MaxHealth, Is.EqualTo(112));
        Assert.That(state.Health, Is.EqualTo(85));

        TheCircussyOneTestObjects.Destroy(pickupDefinition);
    }

    [Test]
    public void HealthPickupDoesNotMagnetOrCollectAtFullHealth()
    {
        var player = TheCircussyOneTestObjects.CreatePlayer(_config);
        var state = new GameState(_config);
        state.Reset();
        var pickupFactory = new PickupFactory(
            _config,
            _registry,
            TheCircussyOneTestObjects.CreatePickupPrefab(),
            TheCircussyOneTestObjects.CreateRoot("Pickups").transform);
        HealthPickupDefinition pickupDefinition = TheCircussyOneTestObjects.CreateHealthPickupDefinition(
            "treat",
            "Treat",
            healAmount: 20);
        PickupRuntime pickup = pickupFactory.Spawn(pickupDefinition, Vector3.zero);
        var system = new PickupSystem(_config, TheCircussyOneTestObjects.CreateDamageFeedbackConfig(), player, _registry, pickupFactory, state);

        system.Tick();

        Assert.That(state.Health, Is.EqualTo(state.MaxHealth));
        Assert.That(_registry.Pickups, Has.Count.EqualTo(1));
        Assert.That(pickup.View.IsAttracting, Is.False);

        TheCircussyOneTestObjects.Destroy(pickupDefinition);
    }

    [Test]
    public void HealthPickupsOnlyMagnetEnoughTreatsToFillMissingHealth()
    {
        _config.pickupCollectRadius = 0.2f;
        _config.pickupMagnetRadius = 4f;
        var player = TheCircussyOneTestObjects.CreatePlayer(_config);
        var state = new GameState(_config);
        state.Reset();
        state.DamagePlayer(5);
        var pickupFactory = new PickupFactory(
            _config,
            _registry,
            TheCircussyOneTestObjects.CreatePickupPrefab(),
            TheCircussyOneTestObjects.CreateRoot("Pickups").transform);
        HealthPickupDefinition pickupDefinition = TheCircussyOneTestObjects.CreateHealthPickupDefinition(
            "treat",
            "Treat",
            healAmount: 20);
        PickupRuntime first = pickupFactory.Spawn(pickupDefinition, new Vector3(1f, 0f, 0f));
        PickupRuntime second = pickupFactory.Spawn(pickupDefinition, new Vector3(1.1f, 0f, 0f));
        PickupRuntime third = pickupFactory.Spawn(pickupDefinition, new Vector3(1.2f, 0f, 0f));
        var system = new PickupSystem(_config, TheCircussyOneTestObjects.CreateDamageFeedbackConfig(), player, _registry, pickupFactory, state);

        system.Tick();
        Assert.That(_registry.Pickups.Count(pickup => pickup.View.IsAttracting), Is.EqualTo(1));

        system.Tick();

        Assert.That(state.Health, Is.EqualTo(state.MaxHealth));
        Assert.That(_registry.Pickups, Has.Count.EqualTo(2));
        Assert.That(new[] { first, second, third }.Count(pickup => _registry.Pickups.Contains(pickup)), Is.EqualTo(2));
        Assert.That(_registry.Pickups.All(pickup => pickup.View.IsAttracting), Is.False);

        TheCircussyOneTestObjects.Destroy(pickupDefinition);
    }

    [Test]
    public void HealthPickupsCanReserveMultipleTreatsWhenMissingHealthNeedsThem()
    {
        _config.pickupCollectRadius = 0.2f;
        _config.pickupMagnetRadius = 4f;
        var player = TheCircussyOneTestObjects.CreatePlayer(_config);
        var state = new GameState(_config);
        state.Reset();
        state.DamagePlayer(35);
        var pickupFactory = new PickupFactory(
            _config,
            _registry,
            TheCircussyOneTestObjects.CreatePickupPrefab(),
            TheCircussyOneTestObjects.CreateRoot("Pickups").transform);
        HealthPickupDefinition pickupDefinition = TheCircussyOneTestObjects.CreateHealthPickupDefinition(
            "treat",
            "Treat",
            healAmount: 20);
        pickupFactory.Spawn(pickupDefinition, new Vector3(1f, 0f, 0f));
        pickupFactory.Spawn(pickupDefinition, new Vector3(1.1f, 0f, 0f));
        pickupFactory.Spawn(pickupDefinition, new Vector3(1.2f, 0f, 0f));
        var system = new PickupSystem(_config, TheCircussyOneTestObjects.CreateDamageFeedbackConfig(), player, _registry, pickupFactory, state);

        system.Tick();
        Assert.That(_registry.Pickups.Count(pickup => pickup.View.IsAttracting), Is.EqualTo(2));

        system.Tick();

        Assert.That(state.Health, Is.EqualTo(state.MaxHealth));
        Assert.That(_registry.Pickups, Has.Count.EqualTo(1));
        Assert.That(_registry.Pickups.All(pickup => pickup.View.IsAttracting), Is.False);

        TheCircussyOneTestObjects.Destroy(pickupDefinition);
    }
}

internal sealed class FakeXpGainCounter : IXpGainCounter
{
    public readonly System.Collections.Generic.List<int> Amounts = new();

    public void Add(int amount)
    {
        Amounts.Add(amount);
    }
}
