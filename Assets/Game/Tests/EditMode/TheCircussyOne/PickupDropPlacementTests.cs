using NUnit.Framework;
using UnityEngine;
using TheCircussyOne.Content;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;
using TheCircussyOne.Runtime;

public sealed class PickupDropPlacementTests
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
    public void ScatterOffsetsAreDeterministicAndDistinct()
    {
        Vector3 first = PickupDropPlacementRules.ScatterOffset(0, 3, 42, 1f);
        Vector3 repeated = PickupDropPlacementRules.ScatterOffset(0, 3, 42, 1f);
        Vector3 second = PickupDropPlacementRules.ScatterOffset(1, 3, 42, 1f);

        Assert.That(repeated, Is.EqualTo(first));
        Assert.That(Vector3.Distance(first, second), Is.GreaterThan(0.01f));
    }

    [Test]
    public void ScatterOffsetsSpreadSmallMultiDropsAroundSource()
    {
        Vector3 twoFirstOffset = PickupDropPlacementRules.ScatterOffset(0, 2, 17, 1f);
        Vector3 twoSecondOffset = PickupDropPlacementRules.ScatterOffset(1, 2, 17, 1f);
        Vector3 twoFirst = twoFirstOffset.normalized;
        Vector3 twoSecond = twoSecondOffset.normalized;

        Assert.That(Vector3.Dot(twoFirst, twoSecond), Is.GreaterThan(-0.6f));
        Assert.That(PlanarArea(twoFirstOffset, twoSecondOffset), Is.GreaterThan(0.4f));
        Assert.That(twoFirstOffset.magnitude, Is.Not.EqualTo(twoSecondOffset.magnitude).Within(0.001f));

        Vector3 threeFirstOffset = PickupDropPlacementRules.ScatterOffset(0, 3, 17, 1f);
        Vector3 threeSecondOffset = PickupDropPlacementRules.ScatterOffset(1, 3, 17, 1f);
        Vector3 threeThirdOffset = PickupDropPlacementRules.ScatterOffset(2, 3, 17, 1f);
        Vector3 threeFirst = threeFirstOffset.normalized;
        Vector3 threeSecond = threeSecondOffset.normalized;
        Vector3 threeThird = threeThirdOffset.normalized;

        Assert.That(Vector3.Dot(threeFirst, threeSecond), Is.EqualTo(-0.5f).Within(0.001f));
        Assert.That(Vector3.Dot(threeSecond, threeThird), Is.EqualTo(-0.5f).Within(0.001f));
        Assert.That(Vector3.Dot(threeThird, threeFirst), Is.EqualTo(-0.5f).Within(0.001f));
        Assert.That(TriangleArea(threeFirstOffset, threeSecondOffset, threeThirdOffset), Is.GreaterThan(1f));
        Assert.That(threeFirstOffset.magnitude, Is.LessThan(threeSecondOffset.magnitude));
        Assert.That(threeSecondOffset.magnitude, Is.LessThan(threeThirdOffset.magnitude));
    }

    [Test]
    public void LaunchOffsetsSeparateMultiDropsImmediately()
    {
        Vector3 first = PickupDropPlacementRules.LaunchOffset(0, 3, 17, 0.16f);
        Vector3 second = PickupDropPlacementRules.LaunchOffset(1, 3, 17, 0.16f);
        Vector3 single = PickupDropPlacementRules.LaunchOffset(0, 1, 17, 0.16f);

        Assert.That(first.sqrMagnitude, Is.GreaterThan(0f));
        Assert.That(Vector3.Distance(first, second), Is.GreaterThan(0.1f));
        Assert.That(single, Is.EqualTo(Vector3.zero));
    }

    [Test]
    public void SingleExperienceDropsUseScatterLaneButSingleHealthDropsStayCentered()
    {
        int xpCount = PickupDropPlacementRules.PresentationScatterCount(1, isExperienceDrop: true);
        int healthCount = PickupDropPlacementRules.PresentationScatterCount(1, isExperienceDrop: false);

        Assert.That(xpCount, Is.EqualTo(2));
        Assert.That(healthCount, Is.EqualTo(1));
        Assert.That(PickupDropPlacementRules.ScatterOffset(0, xpCount, 17, 1f).sqrMagnitude, Is.GreaterThan(0f));
        Assert.That(PickupDropPlacementRules.ScatterOffset(0, healthCount, 17, 1f), Is.EqualTo(Vector3.zero));
    }

    [Test]
    public void ExperienceDropArcHeightCanBeRaisedWithoutChangingHealthDrops()
    {
        Assert.That(PickupDropPlacementRules.ArcHeight(1.15f, 1.65f, isExperienceDrop: true), Is.EqualTo(1.8975f).Within(0.0001f));
        Assert.That(PickupDropPlacementRules.ArcHeight(1.15f, 1.65f, isExperienceDrop: false), Is.EqualTo(1.15f).Within(0.0001f));
    }

    [Test]
    public void ScatterOffsetsDoNotCollapseWithStableHashSizedSeeds()
    {
        int sceneSeed = TicketDepositRules.StableSeed("snack_cart", new Vector3(21f, 0.3f, 16f));
        Vector3 first = PickupDropPlacementRules.ScatterOffset(0, 3, sceneSeed, 1f);
        Vector3 second = PickupDropPlacementRules.ScatterOffset(1, 3, sceneSeed, 1f);
        Vector3 third = PickupDropPlacementRules.ScatterOffset(2, 3, sceneSeed, 1f);

        Assert.That(Vector3.Distance(first, second), Is.GreaterThan(0.25f));
        Assert.That(Vector3.Distance(second, third), Is.GreaterThan(0.25f));
        Assert.That(TriangleArea(first, second, third), Is.GreaterThan(1f));

        Vector3 maxSeedFirst = PickupDropPlacementRules.ScatterOffset(0, 2, int.MaxValue, 1f);
        Vector3 maxSeedSecond = PickupDropPlacementRules.ScatterOffset(1, 2, int.MaxValue, 1f);

        Assert.That(Vector3.Distance(maxSeedFirst, maxSeedSecond), Is.GreaterThan(0.25f));
        Assert.That(PlanarArea(maxSeedFirst, maxSeedSecond), Is.GreaterThan(0.4f));
    }

    [Test]
    public void FactoryDropLandingUsesRaisedSurfaceAndDistinctSlots()
    {
        CreateSurface("TCO Test Raised Ground", new Vector3(0f, 0.5f, 0f), new Vector3(8f, 1f, 8f));
        Physics.SyncTransforms();

        GameConfig config = TheCircussyOneTestObjects.CreateConfig();
        DamageFeedbackVisualConfig feedback = CreateDropFeedback();
        var registry = new ActorRegistry();
        var factory = new PickupFactory(
            config,
            feedback,
            null,
            registry,
            TheCircussyOneTestObjects.CreatePickupPrefab(),
            TheCircussyOneTestObjects.CreateRoot("Pickups").transform,
            new PickupSurfaceSampler());
        XpGemDefinition gem = TheCircussyOneTestObjects.CreateXpGemDefinition("blue_xp_gem", "Blue XP Gem", 7, Color.cyan);

        int sceneSeed = TicketDepositRules.StableSeed("snack_cart", new Vector3(21f, 0.3f, 16f));
        PickupRuntime first = factory.SpawnDrop(gem, new Vector3(0f, 1.1f, 0f), 0, 2, sceneSeed);
        PickupRuntime second = factory.SpawnDrop(gem, new Vector3(0f, 1.1f, 0f), 1, 2, sceneSeed);

        Assert.That(first.Position.y, Is.EqualTo(1f + feedback.pickupGroundClearance).Within(0.001f));
        Assert.That(second.Position.y, Is.EqualTo(1f + feedback.pickupGroundClearance).Within(0.001f));
        Assert.That(Vector3.Distance(Flatten(first.Position), Flatten(second.Position)), Is.GreaterThan(0.01f));
        Assert.That(PlanarArea(Flatten(first.Position), Flatten(second.Position)), Is.GreaterThan(0.4f));

        TheCircussyOneTestObjects.Destroy(config);
        TheCircussyOneTestObjects.Destroy(feedback);
        TheCircussyOneTestObjects.Destroy(gem);
    }

    [Test]
    public void FactorySingleXpDropLandsAwayFromSourceWhileSingleHealthDropStaysCentered()
    {
        CreateSurface("TCO Test Ground", new Vector3(0f, -0.5f, 0f), new Vector3(8f, 1f, 8f));
        Physics.SyncTransforms();

        GameConfig config = TheCircussyOneTestObjects.CreateConfig();
        DamageFeedbackVisualConfig feedback = CreateDropFeedback();
        feedback.pickupDropBurstDistance = 1f;
        var registry = new ActorRegistry();
        var factory = new PickupFactory(
            config,
            feedback,
            null,
            registry,
            TheCircussyOneTestObjects.CreatePickupPrefab(),
            TheCircussyOneTestObjects.CreateRoot("Pickups").transform,
            new PickupSurfaceSampler());
        XpGemDefinition gem = TheCircussyOneTestObjects.CreateXpGemDefinition("blue_xp_gem", "Blue XP Gem", 7, Color.cyan);
        HealthPickupDefinition treat = TheCircussyOneTestObjects.CreateHealthPickupDefinition("treat", "Treat", 20);
        Vector3 origin = new(0f, 1.1f, 0f);

        PickupRuntime xp = factory.SpawnDrop(gem, origin, 0, 1, seed: 17);
        PickupRuntime health = factory.SpawnDrop(treat, origin, 0, 1, seed: 17);

        Assert.That(Flatten(xp.Position).magnitude, Is.GreaterThan(0.25f));
        Assert.That(Flatten(health.Position).magnitude, Is.EqualTo(0f).Within(0.001f));

        TheCircussyOneTestObjects.Destroy(config);
        TheCircussyOneTestObjects.Destroy(feedback);
        TheCircussyOneTestObjects.Destroy(gem);
        TheCircussyOneTestObjects.Destroy(treat);
    }

    [Test]
    public void DirectDefinitionSpawnGroundsOnRaisedSurfaceWhenFeedbackIsAvailable()
    {
        CreateSurface("TCO Test Raised Ground", new Vector3(0f, 0.5f, 0f), new Vector3(8f, 1f, 8f));
        Physics.SyncTransforms();

        GameConfig config = TheCircussyOneTestObjects.CreateConfig();
        DamageFeedbackVisualConfig feedback = CreateDropFeedback();
        var registry = new ActorRegistry();
        var factory = new PickupFactory(
            config,
            feedback,
            null,
            registry,
            TheCircussyOneTestObjects.CreatePickupPrefab(),
            TheCircussyOneTestObjects.CreateRoot("Pickups").transform,
            new PickupSurfaceSampler());
        XpGemDefinition gem = TheCircussyOneTestObjects.CreateXpGemDefinition("blue_xp_gem", "Blue XP Gem", 7, Color.cyan);

        PickupRuntime pickup = factory.Spawn(gem, new Vector3(0f, 0f, 0f));

        Assert.That(pickup.Position.y, Is.EqualTo(1f + feedback.pickupGroundClearance).Within(0.001f));
        Assert.That(pickup.View.transform.position.y, Is.EqualTo(1f + feedback.pickupGroundClearance).Within(0.001f));

        TheCircussyOneTestObjects.Destroy(config);
        TheCircussyOneTestObjects.Destroy(feedback);
        TheCircussyOneTestObjects.Destroy(gem);
    }

    [Test]
    public void PickupVisualRestOffsetKeepsMeshAboveGroundedRoot()
    {
        GameConfig config = TheCircussyOneTestObjects.CreateConfig();
        DamageFeedbackVisualConfig feedback = CreateDropFeedback();
        feedback.pickupVisualGroundPadding = 0.18f;
        var registry = new ActorRegistry();
        var factory = new PickupFactory(
            config,
            feedback,
            null,
            registry,
            TheCircussyOneTestObjects.CreatePickupPrefab(),
            TheCircussyOneTestObjects.CreateRoot("Pickups").transform,
            new PickupSurfaceSampler());
        XpGemDefinition gem = TheCircussyOneTestObjects.CreateXpGemDefinition("red_xp_gem", "Red XP Gem", 73, Color.red);
        gem.visualScale = 1.28f;

        PickupRuntime pickup = factory.Spawn(gem, Vector3.zero);
        Transform visualRoot = pickup.View.transform.Find("Visual Root");

        Assert.That(visualRoot, Is.Not.Null);
        Assert.That(pickup.View.VisualRestLocalY, Is.GreaterThan(feedback.pickupVisualGroundPadding));
        Assert.That(visualRoot.localPosition.y, Is.EqualTo(pickup.View.VisualRestLocalY).Within(0.001f));

        TheCircussyOneTestObjects.Destroy(config);
        TheCircussyOneTestObjects.Destroy(feedback);
        TheCircussyOneTestObjects.Destroy(gem);
    }

    [Test]
    public void BlockedPickupDoesNotMagnetOrCollectThroughWall()
    {
        CreateSurface("TCO Test Wall", new Vector3(1f, 0.75f, 0f), new Vector3(0.25f, 1.5f, 2f));
        Physics.SyncTransforms();

        GameConfig config = TheCircussyOneTestObjects.CreateConfig();
        config.pickupCollectRadius = 3f;
        config.pickupMagnetRadius = 3f;
        DamageFeedbackVisualConfig feedback = CreateDropFeedback();
        var registry = new ActorRegistry();
        var player = TheCircussyOneTestObjects.CreatePlayer(config);
        var factory = new PickupFactory(
            config,
            feedback,
            null,
            registry,
            TheCircussyOneTestObjects.CreatePickupPrefab(),
            TheCircussyOneTestObjects.CreateRoot("Pickups").transform,
            new PickupSurfaceSampler());
        PickupRuntime pickup = factory.Spawn(new Vector3(2f, 0f, 0f));
        var state = new GameState(config);
        state.Reset();
        var system = new PickupSystem(config, feedback, player, registry, factory, state, surfaceSampler: new PickupSurfaceSampler());

        system.Tick();

        Assert.That(registry.Pickups, Has.Count.EqualTo(1));
        Assert.That(pickup.View.IsAttracting, Is.False);
        Assert.That(state.Experience, Is.Zero);

        TheCircussyOneTestObjects.Destroy(config);
        TheCircussyOneTestObjects.Destroy(feedback);
    }

    [Test]
    public void XpDropServiceFallbackSpawnsDefaultXpPickupWithoutCatalog()
    {
        GameConfig config = TheCircussyOneTestObjects.CreateConfig();
        var registry = new ActorRegistry();
        var factory = new PickupFactory(
            config,
            registry,
            TheCircussyOneTestObjects.CreatePickupPrefab(),
            TheCircussyOneTestObjects.CreateRoot("Pickups").transform);
        var service = new XpDropService(catalog: null, factory);

        int spawned = service.SpawnEnemyDrops(Vector3.zero, seed: 17);

        Assert.That(spawned, Is.EqualTo(1));
        Assert.That(registry.Pickups, Has.Count.EqualTo(1));
        Assert.That(registry.Pickups[0].Reward.Kind, Is.EqualTo(PickupRewardKind.Experience));
        Assert.That(registry.Pickups[0].Amount, Is.EqualTo(XpGemDefinition.DefaultBlueXpAmount));

        TheCircussyOneTestObjects.Destroy(config);
    }

    [Test]
    public void XpDropServiceRunSeedChangesFallbackDropLanding()
    {
        CreateSurface("TCO Test Ground", new Vector3(0f, -0.5f, 0f), new Vector3(10f, 1f, 10f));
        Physics.SyncTransforms();

        Vector3 firstLanding = SpawnFallbackXpDropLanding(runSeed: 101);
        TheCircussyOneTestObjects.DestroyAll();
        CreateSurface("TCO Test Ground", new Vector3(0f, -0.5f, 0f), new Vector3(10f, 1f, 10f));
        Physics.SyncTransforms();
        Vector3 secondLanding = SpawnFallbackXpDropLanding(runSeed: 202);

        Assert.That(Vector3.Distance(firstLanding, secondLanding), Is.GreaterThan(0.01f));
    }

    private static DamageFeedbackVisualConfig CreateDropFeedback()
    {
        DamageFeedbackVisualConfig feedback = TheCircussyOneTestObjects.CreateDamageFeedbackConfig();
        feedback.pickupDropBurstDistance = 0.8f;
        feedback.pickupDropBurstSeconds = 0f;
        feedback.pickupDropArcHeight = 0f;
        feedback.pickupDropBounceHeight = 0f;
        feedback.pickupDropBounceCount = 0;
        feedback.pickupGroundClearance = 0.14f;
        feedback.pickupGroundProbeHeight = 4f;
        feedback.pickupGroundProbeDepth = 10f;
        feedback.pickupCollisionRadius = 0.12f;
        feedback.pickupVisualGroundPadding = 0.04f;
        feedback.pickupEnvironmentMask = ~0;
        return feedback;
    }

    private static GameObject CreateSurface(string name, Vector3 position, Vector3 scale)
    {
        GameObject surface = GameObject.CreatePrimitive(PrimitiveType.Cube);
        surface.name = name;
        surface.transform.position = position;
        surface.transform.localScale = scale;
        return surface;
    }

    private static Vector3 SpawnFallbackXpDropLanding(int runSeed)
    {
        GameConfig config = TheCircussyOneTestObjects.CreateConfig();
        DamageFeedbackVisualConfig feedback = CreateDropFeedback();
        var registry = new ActorRegistry();
        var factory = new PickupFactory(
            config,
            feedback,
            null,
            registry,
            TheCircussyOneTestObjects.CreatePickupPrefab(),
            TheCircussyOneTestObjects.CreateRoot("Pickups").transform,
            new PickupSurfaceSampler());
        var runSeedState = new RunSeedState();
        runSeedState.SetForTests(runSeed);
        var service = new XpDropService(catalog: null, factory, runSeedState);

        service.SpawnEnemyDrops(new Vector3(0f, 1.1f, 0f), seed: 17);

        return registry.Pickups[0].Position;
    }

    private static Vector3 Flatten(Vector3 value)
    {
        return new Vector3(value.x, 0f, value.z);
    }

    private static float PlanarArea(Vector3 first, Vector3 second)
    {
        return Mathf.Abs(Vector3.Cross(Flatten(first), Flatten(second)).y);
    }

    private static float TriangleArea(Vector3 first, Vector3 second, Vector3 third)
    {
        return Mathf.Abs(Vector3.Cross(Flatten(second - first), Flatten(third - first)).y) * 0.5f;
    }
}
