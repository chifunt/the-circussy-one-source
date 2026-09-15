using System.Linq;
using NUnit.Framework;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using TheCircussyOne.Runtime;
using TheCircussyOne.Visuals;
using UnityEngine;
using UnityEngine.Rendering;

public sealed class HealingPropTests
{
    [SetUp]
    public void SetUp()
    {
        TheCircussyOneTestObjects.DestroyAll();
        SceneInteractableRegistry.Clear();
        SceneWorldRewardAnimationRegistry.Clear();
        InteractableSelectionOutlineRegistry.Clear();
    }

    [TearDown]
    public void TearDown()
    {
        TheCircussyOneTestObjects.DestroyAll();
        SceneInteractableRegistry.Clear();
        SceneWorldRewardAnimationRegistry.Clear();
        InteractableSelectionOutlineRegistry.Clear();
    }

    [Test]
    public void HealingPropDefinitionDefaultsExposeHealingInteractionIdentity()
    {
        HealthPickupDefinition pickup = TheCircussyOneTestObjects.CreateHealthPickupDefinition("treat", "Treat");
        HealingPropDefinition prop = TheCircussyOneTestObjects.CreateHealingPropDefinition("snack_box", "Snack Box", pickup);

        Assert.That(prop.Id, Is.EqualTo("snack_box"));
        Assert.That(prop.DisplayName, Is.EqualTo("Snack Box"));
        Assert.That(prop.Tags.HasTag(ContentTag.Health), Is.True);
        Assert.That(prop.holdSeconds, Is.EqualTo(1f));
        Assert.That(prop.promptText, Is.EqualTo("Grab a Snack"));
        Assert.That(prop.healthPickup, Is.SameAs(pickup));
        Assert.That(prop.minPickupCount, Is.EqualTo(1));
        Assert.That(prop.maxPickupCount, Is.EqualTo(1));
        Assert.That(prop.openDisappearSeconds, Is.GreaterThan(0f));
        Assert.That(prop.openShrinkFinalScaleMultiplier, Is.InRange(0f, 1f));
        Assert.That(prop.openDisappearEase.preset, Is.EqualTo(EasePreset.InBack));
        Assert.That(prop.snackBurstEnabled, Is.True);
        Assert.That(prop.snackBurstCount, Is.GreaterThan(0));
        Assert.That(prop.snackBurstLifetimeSeconds, Is.GreaterThan(0f));
        Assert.That(prop.snackBurstSpeed, Is.GreaterThan(0f));
        Assert.That(prop.snackBurstGravity, Is.GreaterThanOrEqualTo(0f));
        Assert.That(prop.snackBurstSwayStrength, Is.GreaterThanOrEqualTo(0f));
        Assert.That(prop.snackBurstStartSize, Is.GreaterThan(0f));

        Object.DestroyImmediate(prop);
        Object.DestroyImmediate(pickup);
    }

    [Test]
    public void HealingPropCatalogValidatesInvalidDefinitions()
    {
        HealthPickupDefinition pickup = TheCircussyOneTestObjects.CreateHealthPickupDefinition("heart", "Heart");
        HealingPropDefinition first = TheCircussyOneTestObjects.CreateHealingPropDefinition("snack_box", "Snack Box", pickup);
        HealingPropDefinition duplicate = TheCircussyOneTestObjects.CreateHealingPropDefinition("snack_box", "Duplicate Snack Box", pickup);
        HealingPropDefinition invalid = TheCircussyOneTestObjects.CreateHealingPropDefinition("", "", null);
        invalid.propId = "";
        invalid.displayName = "";
        invalid.holdSeconds = 0f;
        invalid.minPickupCount = 0;
        invalid.maxPickupCount = -1;
        invalid.promptText = "";
        invalid.visualScale = -1f;
        invalid.emissionStrength = -1f;
        invalid.targetOutlineThickness = -1f;
        invalid.interactionSquashStretchAmplitude = -1f;
        invalid.interactionSquashStretchFrequency = -1f;
        invalid.openDisappearSeconds = -1f;
        invalid.openShrinkFinalScaleMultiplier = 1.5f;
        invalid.snackBurstCount = -1;
        invalid.snackBurstLifetimeSeconds = 0f;
        invalid.snackBurstSpeed = -1f;
        invalid.snackBurstGravity = -1f;
        invalid.snackBurstSwayStrength = -1f;
        invalid.snackBurstStartSize = 0f;
        invalid.placement.footprintRadius = 0f;
        invalid.placement.groundClearance = -1f;
        invalid.placement.maxPlacementSlope = 90f;
        invalid.placement.blocksPlayer = true;
        invalid.placement.bodyColliderSize = Vector3.zero;
        HealingPropCatalog catalog = TheCircussyOneTestObjects.CreateHealingPropCatalog(first, duplicate, invalid, null);

        var issues = catalog.ValidateContent();

        Assert.That(issues.Any(issue => issue.Code == "catalog.null-entry"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "content.duplicate-id"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "content.invalid-id"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "content.missing-display-name"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "healing-prop.invalid-hold-seconds"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "healing-prop.missing-pickup"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "healing-prop.invalid-pickup-count"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "healing-prop.invalid-pickup-count-range"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "healing-prop.missing-prompt"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "healing-prop.invalid-visual-scale"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "healing-prop.invalid-emission-strength"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "healing-prop.invalid-outline-thickness"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "healing-prop.invalid-interaction-squash-amplitude"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "healing-prop.invalid-interaction-squash-frequency"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "healing-prop.invalid-open-disappear-seconds"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "healing-prop.invalid-open-shrink-scale"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "healing-prop.invalid-snack-burst-count"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "healing-prop.invalid-snack-burst-lifetime"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "healing-prop.invalid-snack-burst-speed"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "healing-prop.invalid-snack-burst-gravity"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "healing-prop.invalid-snack-burst-sway"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "healing-prop.invalid-snack-burst-size"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "healing-prop.invalid-footprint-radius"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "healing-prop.invalid-ground-clearance"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "healing-prop.invalid-placement-slope"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "healing-prop.invalid-body-collider-size"), Is.True);

        Object.DestroyImmediate(catalog);
        Object.DestroyImmediate(first);
        Object.DestroyImmediate(duplicate);
        Object.DestroyImmediate(invalid);
        Object.DestroyImmediate(pickup);
    }

    [Test]
    public void HealingPropRulesResolveVariablePickupCountDeterministically()
    {
        HealthPickupDefinition pickup = TheCircussyOneTestObjects.CreateHealthPickupDefinition("treat", "Treat");
        HealingPropDefinition prop = TheCircussyOneTestObjects.CreateHealingPropDefinition("snack_cart", "Snack Cart", pickup);
        prop.minPickupCount = 2;
        prop.maxPickupCount = 3;
        int seed = TicketDepositRules.StableSeed(prop.Id, new Vector3(2f, 0f, 7f));

        int first = HealingPropRules.ResolvePickupCount(prop, seed);
        int second = HealingPropRules.ResolvePickupCount(prop, seed);

        Assert.That(first, Is.InRange(2, 3));
        Assert.That(second, Is.EqualTo(first));

        Object.DestroyImmediate(prop);
        Object.DestroyImmediate(pickup);
    }

    [Test]
    public void HealingPropDropSeedCanVaryByRunSeed()
    {
        Vector3 position = new(2f, 0f, 7f);

        int firstRun = TicketDepositRules.StableSeed("snack_cart", position, runSeed: 21);
        int secondRun = TicketDepositRules.StableSeed("snack_cart", position, runSeed: 22);

        Assert.That(TicketDepositRules.StableSeed("snack_cart", position, runSeed: 21), Is.EqualTo(firstRun));
        Assert.That(secondRun, Is.Not.EqualTo(firstRun));
    }

    [Test]
    public void HealingPropViewCompletesHoldAndSpawnsHealthPickups()
    {
        GameConfig config = TheCircussyOneTestObjects.CreateConfig();
        var registry = new ActorRegistry();
        var pickupFactory = new PickupFactory(
            config,
            registry,
            TheCircussyOneTestObjects.CreatePickupPrefab(),
            TheCircussyOneTestObjects.CreateRoot("Pickups").transform);
        HealthPickupDefinition pickup = TheCircussyOneTestObjects.CreateHealthPickupDefinition("treat", "Treat", healAmount: 20);
        HealingPropDefinition prop = TheCircussyOneTestObjects.CreateHealingPropDefinition("snack_cart", "Snack Cart", pickup, holdSeconds: 0.5f, pickupCount: 2);
        prop.snackBurstEnabled = false;
        GameObject root = TheCircussyOneTestObjects.CreateRoot("Healing Prop");
        root.AddComponent<MeshRenderer>();
        root.AddComponent<MeshFilter>();
        var view = root.AddComponent<HealingPropView>();
        view.Prepare(prop, pickupFactory, pickup);

        view.BeginInteraction();
        view.TickInteraction(0.5f, new RunCurrencyState());

        Assert.That(root.activeSelf, Is.True);
        Assert.That(view.IsOpen, Is.True);
        Assert.That(view.IsOpenDespawnStarted, Is.True);
        Assert.That(view.IsInteractionAvailable, Is.False);
        Assert.That(registry.Pickups, Has.Count.EqualTo(2));
        Assert.That(registry.Pickups.All(runtime => runtime.Reward.Kind == PickupRewardKind.Health), Is.True);
        Assert.That(registry.Pickups.All(runtime => runtime.HealthPickup == pickup), Is.True);
        Assert.That(registry.Pickups.All(runtime => runtime.HealthPickup.ResolveHealAmount(config.playerMaxHealth) == 20), Is.True);
        view.TickOpenDespawn(prop.openDisappearSeconds);
        Assert.That(root.activeSelf, Is.False);

        Object.DestroyImmediate(prop);
        Object.DestroyImmediate(pickup);
        Object.DestroyImmediate(config);
    }

    [Test]
    public void HealingPropViewShrinksAwayWithBackEaseAfterOpening()
    {
        HealthPickupDefinition pickup = TheCircussyOneTestObjects.CreateHealthPickupDefinition("treat", "Treat");
        HealingPropDefinition prop = TheCircussyOneTestObjects.CreateHealingPropDefinition("snack_box", "Snack Box", pickup, holdSeconds: 0.1f);
        prop.visualScale = 1f;
        prop.openDisappearSeconds = 1f;
        prop.openShrinkFinalScaleMultiplier = 0.02f;
        prop.openDisappearEase = EaseSettings.InBack;
        prop.snackBurstEnabled = false;
        GameObject root = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Renderer renderer = root.GetComponent<Renderer>();
        var view = root.AddComponent<HealingPropView>();
        view.Prepare(prop, activePickupFactory: null, defaultHealthPickup: pickup);
        float baseBottomY = renderer.bounds.min.y;

        view.BeginInteraction();
        view.TickInteraction(0.1f, new RunCurrencyState());

        Assert.That(root.activeSelf, Is.True);
        Assert.That(view.IsOpenDespawnStarted, Is.True);

        view.TickOpenDespawn(0.1f);

        Assert.That(root.transform.localScale.x, Is.GreaterThan(1f));
        Assert.That(renderer.bounds.min.y, Is.EqualTo(baseBottomY).Within(0.001f));
        float overshootScale = root.transform.localScale.x;

        view.TickOpenDespawn(0.55f);

        Assert.That(root.activeSelf, Is.True);
        Assert.That(root.transform.localScale.x, Is.LessThan(overshootScale));
        Assert.That(root.transform.localScale.x, Is.GreaterThan(0.02f));
        Assert.That(renderer.bounds.min.y, Is.EqualTo(baseBottomY).Within(0.001f));

        view.TickOpenDespawn(0.35f);

        Assert.That(root.activeSelf, Is.False);

        Object.DestroyImmediate(root);
        Object.DestroyImmediate(prop);
        Object.DestroyImmediate(pickup);
    }

    [Test]
    public void HealingPropViewOpenDespawnUsesConfiguredDuration()
    {
        HealthPickupDefinition pickup = TheCircussyOneTestObjects.CreateHealthPickupDefinition("treat", "Treat");
        HealingPropDefinition prop = TheCircussyOneTestObjects.CreateHealingPropDefinition("snack_box", "Snack Box", pickup, holdSeconds: 0.1f);
        prop.openDisappearSeconds = 2f;
        prop.openDisappearEase = EaseSettings.Linear;
        prop.snackBurstEnabled = false;
        GameObject root = GameObject.CreatePrimitive(PrimitiveType.Cube);
        var view = root.AddComponent<HealingPropView>();
        view.Prepare(prop, activePickupFactory: null, defaultHealthPickup: pickup);

        view.BeginInteraction();
        view.TickInteraction(0.1f, new RunCurrencyState());
        view.TickOpenDespawn(1.99f);

        Assert.That(root.activeSelf, Is.True);
        Assert.That(view.OpenDespawnElapsedSeconds, Is.EqualTo(1.99f).Within(0.001f));

        view.TickOpenDespawn(0.02f);

        Assert.That(root.activeSelf, Is.False);
        Assert.That(view.OpenDespawnElapsedSeconds, Is.EqualTo(2f).Within(0.001f));

        Object.DestroyImmediate(root);
        Object.DestroyImmediate(prop);
        Object.DestroyImmediate(pickup);
    }

    [Test]
    public void HealingPropViewOpenDespawnDoesNotAdvanceWhileRunPaused()
    {
        HealthPickupDefinition pickup = TheCircussyOneTestObjects.CreateHealthPickupDefinition("treat", "Treat");
        HealingPropDefinition prop = TheCircussyOneTestObjects.CreateHealingPropDefinition("snack_box", "Snack Box", pickup, holdSeconds: 0.1f);
        prop.openDisappearSeconds = 1f;
        prop.openDisappearEase = EaseSettings.InBack;
        prop.snackBurstEnabled = false;
        GameObject root = GameObject.CreatePrimitive(PrimitiveType.Cube);
        var pauseState = new RunPauseState();
        var view = root.AddComponent<HealingPropView>();
        view.Prepare(prop, activePickupFactory: null, defaultHealthPickup: pickup, activePauseState: pauseState);
        view.BeginInteraction();
        view.TickInteraction(0.1f, new RunCurrencyState());
        Vector3 pausedScale = root.transform.localScale;
        pauseState.Pause(RunPauseReasons.RewardReveal);

        view.TickOpenDespawn(0.5f);

        Assert.That(view.OpenDespawnElapsedSeconds, Is.Zero);
        Assert.That(root.activeSelf, Is.True);
        Assert.That(root.transform.localScale, Is.EqualTo(pausedScale));

        pauseState.Resume(RunPauseReasons.RewardReveal);
        view.TickOpenDespawn(0.1f);

        Assert.That(view.OpenDespawnElapsedSeconds, Is.EqualTo(0.1f).Within(0.001f));
        Assert.That(root.transform.localScale.x, Is.Not.EqualTo(pausedScale.x).Within(0.0001f));

        Object.DestroyImmediate(root);
        Object.DestroyImmediate(prop);
        Object.DestroyImmediate(pickup);
    }

    [Test]
    public void HealingPropViewSpawnsSnackBurstWhenOpened()
    {
        HealthPickupDefinition pickup = TheCircussyOneTestObjects.CreateHealthPickupDefinition("treat", "Treat");
        HealingPropDefinition prop = TheCircussyOneTestObjects.CreateHealingPropDefinition("snack_box", "Snack Box", pickup, holdSeconds: 0.1f);
        prop.snackBurstEnabled = true;
        prop.snackBurstCount = 5;
        prop.snackBurstLifetimeSeconds = 1f;
        prop.snackBurstStartSize = 0.12f;
        GameObject root = GameObject.CreatePrimitive(PrimitiveType.Cube);
        root.transform.position = new Vector3(8f, 0.5f, -3f);
        var vfx = new WorldRewardTransientVfxSystem(new FakeGameTime());
        var view = root.AddComponent<HealingPropView>();
        view.Prepare(prop, activePickupFactory: null, defaultHealthPickup: pickup, activeTransientVfx: vfx);

        view.BeginInteraction();
        view.TickInteraction(0.1f, new RunCurrencyState());

        Assert.That(view.LastSnackBurst, Is.Not.Null);
        ParticleSystem burst = view.LastSnackBurst;
        ParticleSystem.MainModule main = burst.main;
        Assert.That(main.simulationSpace, Is.EqualTo(ParticleSystemSimulationSpace.Local));
        Assert.That(main.startSize.constant, Is.EqualTo(0.12f).Within(0.001f));
        Assert.That(burst.particleCount, Is.EqualTo(5));
        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = burst.colorOverLifetime;
        Assert.That(colorOverLifetime.enabled, Is.True);
        GradientAlphaKey[] alphaKeys = colorOverLifetime.color.gradient.alphaKeys;
        Assert.That(alphaKeys[alphaKeys.Length - 1].alpha, Is.Zero);
        ParticleSystemRenderer burstRenderer = burst.GetComponent<ParticleSystemRenderer>();
        Assert.That(burstRenderer.sharedMaterial, Is.Not.Null);
        Assert.That(burstRenderer.sharedMaterial.renderQueue, Is.EqualTo((int)RenderQueue.Transparent));
        Assert.That(TextureFor(burstRenderer.sharedMaterial), Is.Not.Null);
        var particles = new ParticleSystem.Particle[5];
        int particleCount = burst.GetParticles(particles);
        Assert.That(particleCount, Is.GreaterThan(0));
        Vector3 firstWorldPosition = burst.transform.TransformPoint(particles[0].position);
        Assert.That(firstWorldPosition.x, Is.EqualTo(root.transform.position.x).Within(0.45f));
        Assert.That(firstWorldPosition.z, Is.EqualTo(root.transform.position.z).Within(0.45f));

        Object.DestroyImmediate(burst.gameObject);
        Object.DestroyImmediate(root);
        Object.DestroyImmediate(prop);
        Object.DestroyImmediate(pickup);
    }

    [Test]
    public void SnackCartTreatDropsUseNonLinearBurstLayout()
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.name = "TCO Test Ground";
        ground.transform.position = new Vector3(0f, -0.5f, 0f);
        ground.transform.localScale = new Vector3(8f, 1f, 8f);
        Physics.SyncTransforms();

        GameConfig config = TheCircussyOneTestObjects.CreateConfig();
        DamageFeedbackVisualConfig feedback = TheCircussyOneTestObjects.CreateDamageFeedbackConfig();
        feedback.pickupDropBurstDistance = 1.15f;
        feedback.pickupGroundClearance = 0.14f;
        feedback.pickupGroundProbeHeight = 4f;
        feedback.pickupGroundProbeDepth = 10f;
        feedback.pickupCollisionRadius = 0.12f;
        feedback.pickupEnvironmentMask = ~0;
        var registry = new ActorRegistry();
        var pickupFactory = new PickupFactory(
            config,
            feedback,
            null,
            registry,
            TheCircussyOneTestObjects.CreatePickupPrefab(),
            TheCircussyOneTestObjects.CreateRoot("Pickups").transform,
            new PickupSurfaceSampler());
        HealthPickupDefinition pickup = TheCircussyOneTestObjects.CreateHealthPickupDefinition("treat", "Treat", healAmount: 20);
        HealingPropDefinition prop = TheCircussyOneTestObjects.CreateHealingPropDefinition("snack_cart", "Snack Cart", pickup, holdSeconds: 0.5f, pickupCount: 2);
        GameObject root = GameObject.CreatePrimitive(PrimitiveType.Cube);
        root.name = "TCO Test Snack Cart";
        Collider propCollider = root.GetComponent<Collider>();
        if (propCollider != null)
        {
            Object.DestroyImmediate(propCollider);
        }

        var view = root.AddComponent<HealingPropView>();
        view.Prepare(prop, pickupFactory, pickup);

        view.BeginInteraction();
        view.TickInteraction(0.5f, new RunCurrencyState());

        Assert.That(registry.Pickups, Has.Count.EqualTo(2));
        Vector3 first = registry.Pickups[0].Position - root.transform.position;
        Vector3 second = registry.Pickups[1].Position - root.transform.position;
        Assert.That(PlanarArea(first, second), Is.GreaterThan(0.4f));

        Object.DestroyImmediate(prop);
        Object.DestroyImmediate(pickup);
        Object.DestroyImmediate(feedback);
        Object.DestroyImmediate(config);
    }

    [Test]
    public void HealingPropViewRegistersOutlineAndPromptAnchorWhenTargeted()
    {
        HealthPickupDefinition pickup = TheCircussyOneTestObjects.CreateHealthPickupDefinition("treat", "Treat");
        HealingPropDefinition prop = TheCircussyOneTestObjects.CreateHealingPropDefinition("snack_box", "Snack Box", pickup);
        prop.targetOutlineColor = Color.magenta;
        prop.targetOutlineThickness = 4f;
        GameObject root = GameObject.CreatePrimitive(PrimitiveType.Cube);
        root.name = "TCO Test Healing Prop";
        var view = root.AddComponent<HealingPropView>();
        view.Prepare(prop, activePickupFactory: null, defaultHealthPickup: pickup);

        view.SetInteractionTargeted(true);

        Assert.That(SceneInteractableRegistry.Count, Is.EqualTo(1));
        Assert.That(InteractableSelectionOutlineRegistry.Count, Is.EqualTo(1));
        Assert.That(InteractableSelectionOutlineRegistry.TryGetCompositeSettings(out var settings), Is.True);
        Assert.That(settings.Color, Is.EqualTo(Color.magenta));
        Assert.That(settings.WidthPixels, Is.EqualTo(4f).Within(0.001f));
        Assert.That(view.WorldInteractionPromptPosition.y, Is.GreaterThan(root.transform.position.y));

        Object.DestroyImmediate(prop);
        Object.DestroyImmediate(pickup);
    }

    private static float PlanarArea(Vector3 first, Vector3 second)
    {
        Vector3 flatFirst = new(first.x, 0f, first.z);
        Vector3 flatSecond = new(second.x, 0f, second.z);
        return Mathf.Abs(Vector3.Cross(flatFirst, flatSecond).y);
    }

    private static Texture TextureFor(Material material)
    {
        if (material == null)
        {
            return null;
        }

        if (material.HasProperty("_BaseMap"))
        {
            return material.GetTexture("_BaseMap");
        }

        return material.HasProperty("_MainTex") ? material.GetTexture("_MainTex") : null;
    }
}
