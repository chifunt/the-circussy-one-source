using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using TheCircussyOne.Runtime;
using TheCircussyOne.Visuals;
using UnityEngine;
using UnityEngine.Rendering;

public sealed class TicketDepositTests
{
    private readonly List<Object> _objects = new();

    [TearDown]
    public void TearDown()
    {
        for (int i = 0; i < _objects.Count; i++)
        {
            TheCircussyOneTestObjects.Destroy(_objects[i]);
        }

        _objects.Clear();
        SceneInteractableRegistry.Clear();
        SceneWorldRewardAnimationRegistry.Clear();
        InteractableSelectionOutlineRegistry.Clear();
    }

    [Test]
    public void TicketDepositDefinitionDefaultsExposeCurrencyIdentity()
    {
        TicketDepositDefinition deposit = Track(TheCircussyOneTestObjects.CreateTicketDepositDefinition(
            "small_ticket_stack",
            "Small Ticket Stack"));

        Assert.That(deposit.Id, Is.EqualTo("small_ticket_stack"));
        Assert.That(deposit.DisplayName, Is.EqualTo("Small Ticket Stack"));
        Assert.That(deposit.Tags.HasTag(ContentTag.Currency), Is.True);
        Assert.That(deposit.Tags.HasTag(ContentTag.Economy), Is.True);
        Assert.That(deposit.ClampedMinTickets, Is.EqualTo(5));
        Assert.That(deposit.ClampedMaxTickets, Is.EqualTo(8));
        Assert.That(deposit.EffectivePayoutChunks, Is.EqualTo(1));
        Assert.That(deposit.collectDisappearSeconds, Is.GreaterThan(0f));
        Assert.That(deposit.collectShrinkFinalScaleMultiplier, Is.InRange(0f, 1f));
        Assert.That(deposit.collectDisappearEase.preset, Is.EqualTo(EasePreset.InBack));
        Assert.That(deposit.ticketBurstEnabled, Is.True);
        Assert.That(deposit.ticketBurstCount, Is.GreaterThan(0));
        Assert.That(deposit.ticketBurstLifetimeSeconds, Is.GreaterThan(0f));
        Assert.That(deposit.ticketBurstSpeed, Is.GreaterThan(0f));
        Assert.That(deposit.ticketBurstGravity, Is.GreaterThanOrEqualTo(0f));
        Assert.That(deposit.ticketBurstSwayStrength, Is.GreaterThanOrEqualTo(0f));
        Assert.That(deposit.ticketBurstRectangleSize.x, Is.EqualTo(deposit.ticketBurstRectangleSize.y * 2f).Within(0.001f));
    }

    [Test]
    public void TicketDepositCatalogValidatesInvalidDefinitions()
    {
        TicketDepositDefinition first = Track(TheCircussyOneTestObjects.CreateTicketDepositDefinition("ticket_roll", "Ticket Roll"));
        TicketDepositDefinition duplicate = Track(TheCircussyOneTestObjects.CreateTicketDepositDefinition("ticket_roll", "Duplicate Roll"));
        TicketDepositDefinition invalid = Track(TheCircussyOneTestObjects.CreateTicketDepositDefinition("", ""));
        invalid.depositId = "";
        invalid.displayName = "";
        invalid.holdSeconds = 0f;
        invalid.minTickets = -1;
        invalid.maxTickets = -2;
        invalid.payoutMode = TicketDepositPayoutMode.Chunked;
        invalid.payoutChunks = 0;
        invalid.emissionStrength = -1f;
        invalid.targetOutlineThickness = -1f;
        invalid.interactionSquashStretchAmplitude = -1f;
        invalid.interactionSquashStretchFrequency = -1f;
        invalid.collectDisappearSeconds = -1f;
        invalid.collectShrinkFinalScaleMultiplier = 1.5f;
        invalid.ticketBurstCount = -1;
        invalid.ticketBurstLifetimeSeconds = 0f;
        invalid.ticketBurstSpeed = -1f;
        invalid.ticketBurstGravity = -1f;
        invalid.ticketBurstSwayStrength = -1f;
        invalid.ticketBurstRectangleSize = Vector2.zero;
        invalid.placement.footprintRadius = 0f;
        invalid.placement.groundClearance = -1f;
        invalid.placement.maxPlacementSlope = 90f;
        invalid.placement.blocksPlayer = true;
        invalid.placement.bodyColliderSize = Vector3.zero;

        TicketDepositCatalog catalog = Track(TheCircussyOneTestObjects.CreateTicketDepositCatalog(first, duplicate, invalid, null));

        var issues = catalog.ValidateContent();

        Assert.That(issues.Any(issue => issue.Code == "catalog.null-entry"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "content.duplicate-id"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "content.invalid-id"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "content.missing-display-name"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "ticket-deposit.invalid-hold-seconds"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "ticket-deposit.invalid-min-tickets"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "ticket-deposit.zero-ticket-reward"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "ticket-deposit.invalid-payout-chunks"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "ticket-deposit.invalid-emission-strength"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "ticket-deposit.invalid-outline-thickness"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "ticket-deposit.invalid-interaction-squash-amplitude"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "ticket-deposit.invalid-interaction-squash-frequency"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "ticket-deposit.invalid-collect-disappear-seconds"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "ticket-deposit.invalid-collect-shrink-scale"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "ticket-deposit.invalid-ticket-burst-count"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "ticket-deposit.invalid-ticket-burst-lifetime"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "ticket-deposit.invalid-ticket-burst-speed"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "ticket-deposit.invalid-ticket-burst-gravity"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "ticket-deposit.invalid-ticket-burst-sway"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "ticket-deposit.invalid-ticket-burst-rectangle-size"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "ticket-deposit.invalid-footprint-radius"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "ticket-deposit.invalid-ground-clearance"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "ticket-deposit.invalid-placement-slope"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "ticket-deposit.invalid-body-collider-size"), Is.True);
    }

    [Test]
    public void TicketDepositRulesResolveDeterministicRewardsAndChunks()
    {
        TicketDepositDefinition deposit = Track(TheCircussyOneTestObjects.CreateTicketDepositDefinition(
            "jackpot_cache",
            "Jackpot Cache",
            minTickets: 45,
            maxTickets: 60,
            payoutMode: TicketDepositPayoutMode.Chunked,
            payoutChunks: 4));

        int seed = TicketDepositRules.StableSeed(deposit.Id, new Vector3(1.25f, 0f, 3.5f));
        int first = TicketDepositRules.ResolveReward(deposit, seed);
        int second = TicketDepositRules.ResolveReward(deposit, seed);

        Assert.That(first, Is.EqualTo(second));
        Assert.That(first, Is.InRange(45, 60));
        Assert.That(TicketDepositRules.PaidChunkCount(0.49f, 4), Is.EqualTo(1));
        Assert.That(TicketDepositRules.PaidChunkCount(1f, 4), Is.EqualTo(4));
        Assert.That(Enumerable.Range(0, 4).Sum(index => TicketDepositRules.PayoutForChunk(first, 4, index)), Is.EqualTo(first));
    }

    [Test]
    public void TicketDepositStableSeedIncludesRunSeedWhenProvided()
    {
        Vector3 position = new(1.25f, 0f, 3.5f);

        int firstRun = TicketDepositRules.StableSeed("jackpot_cache", position, runSeed: 1001);
        int secondRun = TicketDepositRules.StableSeed("jackpot_cache", position, runSeed: 2002);

        Assert.That(TicketDepositRules.StableSeed("jackpot_cache", position, runSeed: 1001), Is.EqualTo(firstRun));
        Assert.That(secondRun, Is.Not.EqualTo(firstRun));
    }

    [Test]
    public void TicketDepositRuntimeRecomputesUnpaidRewardWhenRunSeedChanges()
    {
        TicketDepositDefinition deposit = Track(TheCircussyOneTestObjects.CreateTicketDepositDefinition(
            "jackpot_cache",
            "Jackpot Cache",
            minTickets: 45,
            maxTickets: 60));
        Vector3 position = new(1.25f, 0f, 3.5f);
        var runSeedState = new RunSeedState();
        runSeedState.SetForTests(1001);
        var runtime = new TicketDepositRuntime(deposit, position, runSeedState);

        _ = runtime.TotalReward;
        runSeedState.SetForTests(2002);

        int expected = TicketDepositRules.ResolveReward(
            deposit,
            TicketDepositRules.StableSeed(deposit.Id, position, runSeed: 2002));
        Assert.That(runtime.TotalReward, Is.EqualTo(expected));
    }

    [Test]
    public void RunCurrencyStateAddsSpendsAndResetsTickets()
    {
        var currency = new RunCurrencyState();
        var changes = new List<int>();
        currency.TicketsChanged += changes.Add;

        currency.AddTickets(25);
        bool spent = currency.TrySpendTickets(10);
        bool failed = currency.TrySpendTickets(99);
        currency.Reset();

        Assert.That(spent, Is.True);
        Assert.That(failed, Is.False);
        Assert.That(currency.Tickets, Is.Zero);
        Assert.That(changes, Is.EqualTo(new[] { 25, 15, 0 }));
    }

    [Test]
    public void TicketDepositViewAppliesGlowAndTargetOutlineProperties()
    {
        TicketDepositDefinition deposit = Track(TheCircussyOneTestObjects.CreateTicketDepositDefinition(
            "ticket_roll",
            "Ticket Roll"));
        deposit.visualColor = new Color(1f, 0.72f, 0.18f, 1f);
        deposit.emissionColor = new Color(1f, 0.85f, 0.35f, 1f);
        deposit.emissionStrength = 0.9f;
        deposit.targetOutlineColor = new Color(1f, 0.98f, 0.72f, 1f);
        deposit.targetOutlineThickness = 3.5f;

        GameObject root = Track(GameObject.CreatePrimitive(PrimitiveType.Cube));
        var view = root.AddComponent<TicketDepositView>();
        view.Prepare(deposit);
        Renderer renderer = root.GetComponent<Renderer>();
        var block = new MaterialPropertyBlock();

        renderer.GetPropertyBlock(block);
        AssertColor(block.GetColor("_BaseColor"), deposit.visualColor);
        AssertColor(block.GetColor("_EmissionColor"), deposit.emissionColor);
        Assert.That(block.GetFloat("_EmissionStrength"), Is.EqualTo(0.9f).Within(0.001f));
        Assert.That(block.GetFloat("_EmissionSelfGlow"), Is.EqualTo(0.9f).Within(0.001f));

        view.SetInteractionTargeted(true);
        renderer.GetPropertyBlock(block);

        AssertColor(block.GetColor("_BaseColor"), Color.Lerp(deposit.visualColor, Color.white, 0.35f));
        Assert.That(block.GetFloat("_EmissionSelfGlow"), Is.EqualTo(0.9f * 1.35f).Within(0.001f));
    }

    [Test]
    public void TicketDepositViewRegistersScreenSpaceOutlineTargetWhenTargeted()
    {
        TicketDepositDefinition deposit = Track(TheCircussyOneTestObjects.CreateTicketDepositDefinition(
            "ticket_roll",
            "Ticket Roll"));
        deposit.targetOutlineColor = new Color(1f, 0.98f, 0.72f, 1f);
        deposit.targetOutlineThickness = 3.5f;

        GameObject root = Track(GameObject.CreatePrimitive(PrimitiveType.Cube));
        var view = root.AddComponent<TicketDepositView>();
        view.Prepare(deposit);
        var results = new List<Renderer>();

        Assert.That(InteractableSelectionOutlineRegistry.Count, Is.Zero);

        view.SetInteractionTargeted(true);

        InteractableSelectionOutlineRegistry.CopyRenderers(results);
        Assert.That(InteractableSelectionOutlineRegistry.Count, Is.EqualTo(1));
        Assert.That(results, Does.Contain(root.GetComponent<Renderer>()));
        Assert.That(InteractableSelectionOutlineRegistry.TryGetCompositeSettings(out var settings), Is.True);
        AssertColor(settings.Color, deposit.targetOutlineColor);
        Assert.That(settings.WidthPixels, Is.EqualTo(3.5f).Within(0.001f));

        view.SetInteractionTargeted(false);

        results.Clear();
        InteractableSelectionOutlineRegistry.CopyRenderers(results);
        Assert.That(InteractableSelectionOutlineRegistry.Count, Is.Zero);
        Assert.That(results, Is.Empty);
    }

    [Test]
    public void TicketDepositViewProvidesWorldPromptAnchorAboveVisibleBounds()
    {
        TicketDepositDefinition deposit = Track(TheCircussyOneTestObjects.CreateTicketDepositDefinition(
            "ticket_roll",
            "Ticket Roll"));
        GameObject root = Track(GameObject.CreatePrimitive(PrimitiveType.Cube));
        root.transform.position = new Vector3(2f, 0.5f, 3f);
        var view = root.AddComponent<TicketDepositView>();
        view.Prepare(deposit);

        Vector3 anchor = view.WorldInteractionPromptPosition;

        Assert.That(anchor.x, Is.EqualTo(root.transform.position.x).Within(0.001f));
        Assert.That(anchor.z, Is.EqualTo(root.transform.position.z).Within(0.001f));
        Assert.That(anchor.y, Is.GreaterThan(root.transform.position.y));
    }

    [Test]
    public void TicketDepositViewSquashesAndStretchesWhileInteracting()
    {
        TicketDepositDefinition deposit = Track(TheCircussyOneTestObjects.CreateTicketDepositDefinition(
            "ticket_roll",
            "Ticket Roll",
            holdSeconds: 10f));
        deposit.visualScale = 1f;
        deposit.interactionSquashStretchAmplitude = 0.1f;
        deposit.interactionSquashStretchFrequency = 1f;

        GameObject root = Track(GameObject.CreatePrimitive(PrimitiveType.Cube));
        var view = root.AddComponent<TicketDepositView>();
        view.Prepare(deposit);

        view.BeginInteraction();
        view.TickInteraction(0.25f, new RunCurrencyState());

        Assert.That(root.transform.localScale.x, Is.EqualTo(0.95f).Within(0.001f));
        Assert.That(root.transform.localScale.y, Is.EqualTo(1.1f).Within(0.001f));
        Assert.That(root.transform.localScale.z, Is.EqualTo(0.95f).Within(0.001f));

        view.CancelInteraction();

        Assert.That(root.transform.localScale, Is.EqualTo(Vector3.one));
    }

    [Test]
    public void TicketDepositViewShrinksAwayWithBackEaseAfterCollecting()
    {
        TicketDepositDefinition deposit = Track(TheCircussyOneTestObjects.CreateTicketDepositDefinition(
            "small_ticket_stack",
            "Small Ticket Stack",
            holdSeconds: 0.1f));
        deposit.visualScale = 1f;
        deposit.collectDisappearSeconds = 1f;
        deposit.collectShrinkFinalScaleMultiplier = 0.02f;
        deposit.collectDisappearEase = EaseSettings.InBack;
        deposit.ticketBurstEnabled = false;
        GameObject root = Track(GameObject.CreatePrimitive(PrimitiveType.Cube));
        Renderer renderer = root.GetComponent<Renderer>();
        var view = root.AddComponent<TicketDepositView>();
        view.Prepare(deposit);
        float baseBottomY = renderer.bounds.min.y;

        view.BeginInteraction();
        view.TickInteraction(0.1f, new RunCurrencyState());

        Assert.That(root.activeSelf, Is.True);
        Assert.That(view.IsCollected, Is.True);
        Assert.That(view.IsCollectDespawnStarted, Is.True);
        Assert.That(view.IsInteractionAvailable, Is.False);

        view.TickCollectDespawn(0.1f);

        Assert.That(root.transform.localScale.x, Is.GreaterThan(1f));
        Assert.That(renderer.bounds.min.y, Is.EqualTo(baseBottomY).Within(0.001f));
        float overshootScale = root.transform.localScale.x;

        view.TickCollectDespawn(0.55f);

        Assert.That(root.activeSelf, Is.True);
        Assert.That(root.transform.localScale.x, Is.LessThan(overshootScale));
        Assert.That(root.transform.localScale.x, Is.GreaterThan(0.02f));
        Assert.That(renderer.bounds.min.y, Is.EqualTo(baseBottomY).Within(0.001f));

        view.TickCollectDespawn(0.35f);

        Assert.That(root.activeSelf, Is.False);
    }

    [Test]
    public void TicketDepositViewCollectDespawnUsesConfiguredDuration()
    {
        TicketDepositDefinition deposit = Track(TheCircussyOneTestObjects.CreateTicketDepositDefinition(
            "small_ticket_stack",
            "Small Ticket Stack",
            holdSeconds: 0.1f));
        deposit.collectDisappearSeconds = 2f;
        deposit.collectDisappearEase = EaseSettings.Linear;
        deposit.ticketBurstEnabled = false;
        GameObject root = Track(GameObject.CreatePrimitive(PrimitiveType.Cube));
        var view = root.AddComponent<TicketDepositView>();
        view.Prepare(deposit);

        view.BeginInteraction();
        view.TickInteraction(0.1f, new RunCurrencyState());
        view.TickCollectDespawn(1.99f);

        Assert.That(root.activeSelf, Is.True);
        Assert.That(view.CollectDespawnElapsedSeconds, Is.EqualTo(1.99f).Within(0.001f));

        view.TickCollectDespawn(0.02f);

        Assert.That(root.activeSelf, Is.False);
        Assert.That(view.CollectDespawnElapsedSeconds, Is.EqualTo(2f).Within(0.001f));
    }

    [Test]
    public void TicketDepositViewCollectDespawnDoesNotAdvanceWhileRunPaused()
    {
        TicketDepositDefinition deposit = Track(TheCircussyOneTestObjects.CreateTicketDepositDefinition(
            "small_ticket_stack",
            "Small Ticket Stack",
            holdSeconds: 0.1f));
        deposit.collectDisappearSeconds = 1f;
        deposit.collectDisappearEase = EaseSettings.InBack;
        deposit.ticketBurstEnabled = false;
        GameObject root = Track(GameObject.CreatePrimitive(PrimitiveType.Cube));
        var pauseState = new RunPauseState();
        var view = root.AddComponent<TicketDepositView>();
        view.Prepare(deposit, activePauseState: pauseState);
        view.BeginInteraction();
        view.TickInteraction(0.1f, new RunCurrencyState());
        pauseState.Pause(RunPauseReasons.RewardReveal);

        view.TickCollectDespawn(0.5f);

        Assert.That(view.CollectDespawnElapsedSeconds, Is.Zero);
        Assert.That(root.activeSelf, Is.True);
        Assert.That(root.transform.localScale, Is.EqualTo(Vector3.one));

        pauseState.Resume(RunPauseReasons.RewardReveal);
        view.TickCollectDespawn(0.1f);

        Assert.That(view.CollectDespawnElapsedSeconds, Is.EqualTo(0.1f).Within(0.001f));
        Assert.That(root.transform.localScale.x, Is.GreaterThan(1f));
    }

    [Test]
    public void WorldRewardAnimationSystemTicksTicketDepositDespawnWithGameTime()
    {
        TicketDepositDefinition deposit = Track(TheCircussyOneTestObjects.CreateTicketDepositDefinition(
            "small_ticket_stack",
            "Small Ticket Stack",
            holdSeconds: 0.1f));
        deposit.collectDisappearSeconds = 1f;
        deposit.collectDisappearEase = EaseSettings.Linear;
        deposit.ticketBurstEnabled = false;
        GameObject root = Track(GameObject.CreatePrimitive(PrimitiveType.Cube));
        var view = root.AddComponent<TicketDepositView>();
        view.Prepare(deposit);
        view.BeginInteraction();
        view.TickInteraction(0.1f, new RunCurrencyState());
        var time = new FakeGameTime { DeltaTime = 0.25f };
        var system = new WorldRewardAnimationSystem(time);

        system.Tick();

        Assert.That(view.CollectDespawnElapsedSeconds, Is.EqualTo(0.25f).Within(0.001f));
    }

    [Test]
    public void TicketDepositViewSpawnsTicketPaperBurstWhenTicketsAreGranted()
    {
        TicketDepositDefinition deposit = Track(TheCircussyOneTestObjects.CreateTicketDepositDefinition(
            "small_ticket_stack",
            "Small Ticket Stack",
            holdSeconds: 0.1f,
            minTickets: 5,
            maxTickets: 5));
        deposit.ticketBurstEnabled = true;
        deposit.ticketBurstCount = 6;
        deposit.ticketBurstLifetimeSeconds = 1f;
        deposit.ticketBurstRectangleSize = new Vector2(0.2f, 0.1f);
        Texture2D ticketTexture = Track(new Texture2D(8, 4, TextureFormat.RGBA32, mipChain: false));
        ticketTexture.name = "Test Ticket Burst Texture";
        deposit.ticketBurstTexture = ticketTexture;
        GameObject root = Track(GameObject.CreatePrimitive(PrimitiveType.Cube));
        root.transform.position = new Vector3(12f, 0.5f, -7f);
        var vfx = new WorldRewardTransientVfxSystem(new FakeGameTime());
        var view = root.AddComponent<TicketDepositView>();
        view.Prepare(deposit, activeTransientVfx: vfx);

        view.BeginInteraction();
        view.TickInteraction(0.1f, new RunCurrencyState());

        Assert.That(view.LastTicketBurst, Is.Not.Null);
        Track(view.LastTicketBurst.gameObject);
        ParticleSystem burst = view.LastTicketBurst;
        ParticleSystem.MainModule main = burst.main;
        Assert.That(main.simulationSpace, Is.EqualTo(ParticleSystemSimulationSpace.Local));
        Assert.That(main.startSize3D, Is.True);
        Assert.That(main.startSizeX.constant, Is.EqualTo(0.2f).Within(0.001f));
        Assert.That(main.startSizeY.constant, Is.EqualTo(0.1f).Within(0.001f));
        Assert.That(burst.particleCount, Is.EqualTo(6));
        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = burst.colorOverLifetime;
        Assert.That(colorOverLifetime.enabled, Is.True);
        GradientAlphaKey[] alphaKeys = colorOverLifetime.color.gradient.alphaKeys;
        Assert.That(alphaKeys.Length, Is.GreaterThanOrEqualTo(4));
        Assert.That(alphaKeys[alphaKeys.Length - 2].time, Is.LessThan(0.9f));
        Assert.That(alphaKeys[alphaKeys.Length - 2].alpha, Is.LessThan(alphaKeys[0].alpha * 0.5f));
        Assert.That(alphaKeys[alphaKeys.Length - 1].alpha, Is.Zero);
        ParticleSystemRenderer burstRenderer = burst.GetComponent<ParticleSystemRenderer>();
        Assert.That(burstRenderer.sharedMaterial, Is.Not.Null);
        Assert.That(burstRenderer.sharedMaterial.renderQueue, Is.EqualTo((int)RenderQueue.Transparent));
        Assert.That(TextureFor(burstRenderer.sharedMaterial), Is.SameAs(ticketTexture));
        var particles = new ParticleSystem.Particle[6];
        int particleCount = burst.GetParticles(particles);
        Assert.That(particleCount, Is.GreaterThan(0));
        Vector3 firstWorldPosition = burst.transform.TransformPoint(particles[0].position);
        Assert.That(firstWorldPosition.x, Is.EqualTo(root.transform.position.x).Within(0.5f));
        Assert.That(firstWorldPosition.z, Is.EqualTo(root.transform.position.z).Within(0.5f));
    }

    [Test]
    public void InteractableSelectionOutlineRendererFeatureOccludesHiddenPixelsByDefault()
    {
        var feature = Track(ScriptableObject.CreateInstance<InteractableSelectionOutlineRendererFeature>());

        Assert.That(feature.OccludeHiddenPixels, Is.True);
    }

    [Test]
    public void TicketDepositViewUnregistersScreenSpaceOutlineWhenDisabled()
    {
        TicketDepositDefinition deposit = Track(TheCircussyOneTestObjects.CreateTicketDepositDefinition(
            "ticket_roll",
            "Ticket Roll"));
        GameObject root = Track(GameObject.CreatePrimitive(PrimitiveType.Cube));
        var view = root.AddComponent<TicketDepositView>();
        view.Prepare(deposit);

        view.SetInteractionTargeted(true);
        root.SetActive(false);

        Assert.That(InteractableSelectionOutlineRegistry.Count, Is.Zero);
    }

    [Test]
    public void ChunkedTicketDepositKeepsPaidChunksWhenCancelled()
    {
        TicketDepositDefinition deposit = Track(TheCircussyOneTestObjects.CreateTicketDepositDefinition(
            "ticket_roll",
            "Ticket Roll",
            holdSeconds: 4f,
            minTickets: 20,
            maxTickets: 20,
            payoutMode: TicketDepositPayoutMode.Chunked,
            payoutChunks: 4));
        var currency = new RunCurrencyState();
        var runtime = new TicketDepositRuntime(deposit, Vector3.zero);

        runtime.Tick(1.05f, currency);
        runtime.Cancel();
        runtime.Tick(0.1f, currency);

        Assert.That(currency.Tickets, Is.EqualTo(5));
        Assert.That(runtime.PaidTickets, Is.EqualTo(5));
        Assert.That(runtime.Progress, Is.GreaterThanOrEqualTo(0.25f));
    }

    [Test]
    public void InteractionRulesSelectClosestCandidateAndLockHeldTarget()
    {
        var closeDeposit = new FakeInteractable(new Vector3(0.5f, 0f, 0f), InteractPriority.TicketDeposit);
        var fartherChest = new FakeInteractable(new Vector3(1.5f, 0f, 0f), InteractPriority.ChestOrShrine);
        var closestOther = new FakeInteractable(new Vector3(0.1f, 0f, 0f), InteractPriority.Other);
        var candidates = new IInteractable[] { closeDeposit, fartherChest, closestOther };

        IInteractable selected = InteractionRules.SelectBest(candidates, Vector3.zero, 2f);
        IInteractable locked = InteractionRules.SelectBest(candidates, Vector3.zero, 2f, closeDeposit);

        Assert.That(selected, Is.SameAs(closestOther));
        Assert.That(locked, Is.SameAs(closeDeposit));
    }

    [Test]
    public void InteractionSystemCompletesHoldAndRetargetsNextDepositWhenHeld()
    {
        GameConfig config = Track(TheCircussyOneTestObjects.CreateConfig());
        config.playerInteractRadius = 5f;
        var playerObject = Track(new GameObject("Player"));
        var player = playerObject.AddComponent<PlayerView>();
        var input = new FakeInputService { InteractPressedThisFrame = true, InteractHeld = true };
        var time = new FakeGameTime { DeltaTime = 1f };
        var currency = new RunCurrencyState();
        var first = new FakeInteractable(new Vector3(1f, 0f, 0f), InteractPriority.TicketDeposit, holdSeconds: 1f, ticketReward: 5);
        var second = new FakeInteractable(new Vector3(2f, 0f, 0f), InteractPriority.TicketDeposit, holdSeconds: 1f, ticketReward: 7);
        var source = new FakeInteractableSource(first, second);
        var system = new InteractionSystem(config, input, player, time, currency, hud: null, interactableSource: source);

        system.Tick();
        input.InteractPressedThisFrame = false;
        system.Tick();

        Assert.That(currency.Tickets, Is.EqualTo(12));
        Assert.That(first.Completed, Is.True);
        Assert.That(second.Completed, Is.True);
    }

    [Test]
    public void InteractionSystemResetCancelsHeldTarget()
    {
        GameConfig config = Track(TheCircussyOneTestObjects.CreateConfig());
        config.playerInteractRadius = 5f;
        var playerObject = Track(new GameObject("Player"));
        var player = playerObject.AddComponent<PlayerView>();
        var input = new FakeInputService { InteractPressedThisFrame = true, InteractHeld = true };
        var time = new FakeGameTime { DeltaTime = 0.25f };
        var currency = new RunCurrencyState();
        var deposit = new FakeInteractable(new Vector3(1f, 0f, 0f), InteractPriority.TicketDeposit, holdSeconds: 10f, ticketReward: 5);
        var source = new FakeInteractableSource(deposit);
        var system = new InteractionSystem(config, input, player, time, currency, hud: null, interactableSource: source);

        system.Tick();
        system.ResetRunState(new RunResetContext(RunResetKind.WorldTransition, worldIndex: 2));

        Assert.That(system.IsHolding, Is.False);
        Assert.That(system.CurrentTarget, Is.Null);
        Assert.That(deposit.Targeted, Is.False);
        Assert.That(deposit.CancelCount, Is.EqualTo(1));
    }

    [Test]
    public void InteractionSystemRoutesTicketTargetsToWorldPrompt()
    {
        GameConfig config = Track(TheCircussyOneTestObjects.CreateConfig());
        config.playerInteractRadius = 5f;
        var playerObject = Track(new GameObject("Player"));
        var player = playerObject.AddComponent<PlayerView>();
        var input = new FakeInputService();
        var time = new FakeGameTime { DeltaTime = 0.25f };
        var currency = new RunCurrencyState();
        var deposit = new FakeInteractable(new Vector3(1f, 0f, 0f), InteractPriority.TicketDeposit, holdSeconds: 1f, ticketReward: 5);
        var source = new FakeInteractableSource(deposit);
        var worldPrompt = new FakeWorldInteractionPrompt();
        var system = new InteractionSystem(config, input, player, time, currency, hud: null, interactableSource: source, worldPrompt: worldPrompt);

        system.Tick();

        Assert.That(worldPrompt.ShowCount, Is.EqualTo(1));
        Assert.That(worldPrompt.HideCount, Is.Zero);
        Assert.That(worldPrompt.LastPrompt, Is.EqualTo("Collect Tickets"));
        Assert.That(worldPrompt.LastProgress, Is.Zero);
        Assert.That(worldPrompt.LastHolding, Is.False);
        Assert.That(worldPrompt.LastTarget, Is.SameAs(deposit));
    }

    [Test]
    public void InteractionSystemHidesWorldPromptInsteadOfShowingBlankRetargetAction()
    {
        GameConfig config = Track(TheCircussyOneTestObjects.CreateConfig());
        config.playerInteractRadius = 5f;
        var playerObject = Track(new GameObject("Player"));
        var player = playerObject.AddComponent<PlayerView>();
        var input = new FakeInputService { InteractPressedThisFrame = true, InteractHeld = true };
        var time = new FakeGameTime { DeltaTime = 1f };
        var currency = new RunCurrencyState();
        var snack = new FakeInteractable(
            new Vector3(1f, 0f, 0f),
            InteractPriority.TicketDeposit,
            holdSeconds: 1f,
            ticketReward: 5,
            promptText: "Grab a Snack");
        var staleTarget = new FakeInteractable(
            new Vector3(2f, 0f, 0f),
            InteractPriority.TicketDeposit,
            promptText: "");
        var source = new FakeInteractableSource(snack, staleTarget);
        var worldPrompt = new FakeWorldInteractionPrompt();
        var system = new InteractionSystem(config, input, player, time, currency, hud: null, interactableSource: source, worldPrompt: worldPrompt);

        system.Tick();

        Assert.That(snack.Completed, Is.True);
        Assert.That(worldPrompt.ShowCount, Is.EqualTo(1));
        Assert.That(worldPrompt.LastPrompt, Is.EqualTo("Grab a Snack"));
        Assert.That(worldPrompt.HideCount, Is.EqualTo(1));
        Assert.That(worldPrompt.IsShowing, Is.False);
    }

    [Test]
    public void InteractionSystemDoesNotRestartPromptForCurrencyBlockedTarget()
    {
        GameConfig config = Track(TheCircussyOneTestObjects.CreateConfig());
        config.playerInteractRadius = 5f;
        var playerObject = Track(new GameObject("Player"));
        var player = playerObject.AddComponent<PlayerView>();
        var input = new FakeInputService { InteractPressedThisFrame = true, InteractHeld = true };
        var time = new FakeGameTime { DeltaTime = 0.25f };
        var currency = new RunCurrencyState();
        var blocked = new FakeInteractable(
            new Vector3(1f, 0f, 0f),
            InteractPriority.ChestOrShrine,
            promptText: "Need <color=#ff4758>35</color> more Tickets",
            canBeginInteraction: false);
        var source = new FakeInteractableSource(blocked);
        var worldPrompt = new FakeWorldInteractionPrompt();
        var system = new InteractionSystem(config, input, player, time, currency, hud: null, interactableSource: source, worldPrompt: worldPrompt);

        system.Tick();

        Assert.That(system.IsHolding, Is.False);
        Assert.That(blocked.BeginCount, Is.Zero);
        Assert.That(worldPrompt.ShowCount, Is.EqualTo(1));
        Assert.That(worldPrompt.LastTarget, Is.SameAs(blocked));
        Assert.That(worldPrompt.LastPrompt, Is.EqualTo("Need <color=#ff4758>35</color> more Tickets"));
        Assert.That(worldPrompt.LastHolding, Is.False);
    }

    [Test]
    public void InteractionSystemUpdatesWorldPromptProgressWhileHolding()
    {
        GameConfig config = Track(TheCircussyOneTestObjects.CreateConfig());
        config.playerInteractRadius = 5f;
        var playerObject = Track(new GameObject("Player"));
        var player = playerObject.AddComponent<PlayerView>();
        var input = new FakeInputService { InteractPressedThisFrame = true, InteractHeld = true };
        var time = new FakeGameTime { DeltaTime = 0.5f };
        var currency = new RunCurrencyState();
        var deposit = new FakeInteractable(new Vector3(1f, 0f, 0f), InteractPriority.TicketDeposit, holdSeconds: 1f, ticketReward: 5);
        var source = new FakeInteractableSource(deposit);
        var worldPrompt = new FakeWorldInteractionPrompt();
        var system = new InteractionSystem(config, input, player, time, currency, hud: null, interactableSource: source, worldPrompt: worldPrompt);

        system.Tick();

        Assert.That(worldPrompt.LastHolding, Is.True);
        Assert.That(worldPrompt.LastProgress, Is.EqualTo(0.5f).Within(0.001f));
    }

    [Test]
    public void InteractionSystemUsesBasePromptTextWhileHoldingCurrencyAwareTarget()
    {
        GameConfig config = Track(TheCircussyOneTestObjects.CreateConfig());
        config.playerInteractRadius = 5f;
        var playerObject = Track(new GameObject("Player"));
        var player = playerObject.AddComponent<PlayerView>();
        var input = new FakeInputService { InteractPressedThisFrame = true, InteractHeld = true };
        var time = new FakeGameTime { DeltaTime = 0.5f };
        var currency = new RunCurrencyState();
        var chest = new FakeInteractable(
            new Vector3(1f, 0f, 0f),
            InteractPriority.ChestOrShrine,
            holdSeconds: 1f,
            promptText: "Open Chest",
            currencyPromptText: "Spend 35 Tickets to Open Chest");
        var source = new FakeInteractableSource(chest);
        var worldPrompt = new FakeWorldInteractionPrompt();
        var system = new InteractionSystem(config, input, player, time, currency, hud: null, interactableSource: source, worldPrompt: worldPrompt);

        system.Tick();

        Assert.That(worldPrompt.LastHolding, Is.True);
        Assert.That(worldPrompt.LastPrompt, Is.EqualTo("Open Chest"));
    }

    [Test]
    public void InteractionSystemHidesWorldPromptWhenTargetLeavesRange()
    {
        GameConfig config = Track(TheCircussyOneTestObjects.CreateConfig());
        config.playerInteractRadius = 0.25f;
        var playerObject = Track(new GameObject("Player"));
        var player = playerObject.AddComponent<PlayerView>();
        var input = new FakeInputService();
        var time = new FakeGameTime { DeltaTime = 0.25f };
        var currency = new RunCurrencyState();
        var deposit = new FakeInteractable(new Vector3(1f, 0f, 0f), InteractPriority.TicketDeposit);
        var source = new FakeInteractableSource(deposit);
        var worldPrompt = new FakeWorldInteractionPrompt();
        var system = new InteractionSystem(config, input, player, time, currency, hud: null, interactableSource: source, worldPrompt: worldPrompt);

        system.Tick();

        Assert.That(worldPrompt.ShowCount, Is.Zero);
        Assert.That(worldPrompt.HideCount, Is.EqualTo(1));
    }

    [Test]
    public void InteractionSystemSelectsVisibleTargetInRange()
    {
        GameConfig config = Track(TheCircussyOneTestObjects.CreateConfig());
        config.playerInteractRadius = 5f;
        PlayerView player = CreateInteractionPlayer();
        var deposit = new FakeInteractable(new Vector3(2f, 0f, 0f), InteractPriority.TicketDeposit);
        var worldPrompt = new FakeWorldInteractionPrompt();
        var system = new InteractionSystem(
            config,
            new FakeInputService(),
            player,
            new FakeGameTime { DeltaTime = 0.1f },
            new RunCurrencyState(),
            hud: null,
            interactableSource: new FakeInteractableSource(deposit),
            worldPrompt: worldPrompt);

        system.Tick();

        Assert.That(system.CurrentTarget, Is.SameAs(deposit));
        Assert.That(worldPrompt.LastTarget, Is.SameAs(deposit));
    }

    [Test]
    public void InteractionSystemDoesNotSelectTargetBehindEnvironmentBlocker()
    {
        GameConfig config = Track(TheCircussyOneTestObjects.CreateConfig());
        config.playerInteractRadius = 5f;
        PlayerView player = CreateInteractionPlayer();
        CreateInteractionBlocker(new Vector3(1f, 0.75f, 0f), new Vector3(0.25f, 1.5f, 1.5f));
        var blocked = new FakeInteractable(new Vector3(2f, 0f, 0f), InteractPriority.TicketDeposit);
        var worldPrompt = new FakeWorldInteractionPrompt();
        var system = new InteractionSystem(
            config,
            new FakeInputService(),
            player,
            new FakeGameTime { DeltaTime = 0.1f },
            new RunCurrencyState(),
            hud: null,
            interactableSource: new FakeInteractableSource(blocked),
            worldPrompt: worldPrompt);

        system.Tick();

        Assert.That(system.CurrentTarget, Is.Null);
        Assert.That(worldPrompt.ShowCount, Is.Zero);
        Assert.That(worldPrompt.HideCount, Is.EqualTo(1));
        Assert.That(blocked.Targeted, Is.False);
    }

    [Test]
    public void InteractionSystemSkipsBlockedClosestTargetForVisibleTarget()
    {
        GameConfig config = Track(TheCircussyOneTestObjects.CreateConfig());
        config.playerInteractRadius = 5f;
        PlayerView player = CreateInteractionPlayer();
        CreateInteractionBlocker(new Vector3(0.75f, 0.75f, 0f), new Vector3(0.25f, 1.5f, 1f));
        var blockedClose = new FakeInteractable(new Vector3(1.5f, 0f, 0f), InteractPriority.TicketDeposit);
        var visibleFar = new FakeInteractable(new Vector3(0f, 0f, 2f), InteractPriority.HealingProp, promptText: "Grab a Snack");
        var worldPrompt = new FakeWorldInteractionPrompt();
        var system = new InteractionSystem(
            config,
            new FakeInputService(),
            player,
            new FakeGameTime { DeltaTime = 0.1f },
            new RunCurrencyState(),
            hud: null,
            interactableSource: new FakeInteractableSource(blockedClose, visibleFar),
            worldPrompt: worldPrompt);

        system.Tick();

        Assert.That(system.CurrentTarget, Is.SameAs(visibleFar));
        Assert.That(blockedClose.Targeted, Is.False);
        Assert.That(visibleFar.Targeted, Is.True);
        Assert.That(worldPrompt.LastTarget, Is.SameAs(visibleFar));
    }

    [Test]
    public void InteractionSystemCanDisableLineOfSightGate()
    {
        GameConfig config = Track(TheCircussyOneTestObjects.CreateConfig());
        config.playerInteractRadius = 5f;
        config.playerInteractRequiresLineOfSight = false;
        PlayerView player = CreateInteractionPlayer();
        CreateInteractionBlocker(new Vector3(1f, 0.75f, 0f), new Vector3(0.25f, 1.5f, 1.5f));
        var blocked = new FakeInteractable(new Vector3(2f, 0f, 0f), InteractPriority.TicketDeposit);
        var worldPrompt = new FakeWorldInteractionPrompt();
        var system = new InteractionSystem(
            config,
            new FakeInputService(),
            player,
            new FakeGameTime { DeltaTime = 0.1f },
            new RunCurrencyState(),
            hud: null,
            interactableSource: new FakeInteractableSource(blocked),
            worldPrompt: worldPrompt);

        system.Tick();

        Assert.That(system.CurrentTarget, Is.SameAs(blocked));
        Assert.That(worldPrompt.LastTarget, Is.SameAs(blocked));
    }

    [Test]
    public void InteractionSystemIgnoresTriggerBlockersForLineOfSight()
    {
        GameConfig config = Track(TheCircussyOneTestObjects.CreateConfig());
        config.playerInteractRadius = 5f;
        PlayerView player = CreateInteractionPlayer();
        CreateInteractionBlocker(new Vector3(1f, 0.75f, 0f), new Vector3(0.25f, 1.5f, 1.5f), isTrigger: true);
        var deposit = new FakeInteractable(new Vector3(2f, 0f, 0f), InteractPriority.TicketDeposit);
        var worldPrompt = new FakeWorldInteractionPrompt();
        var system = new InteractionSystem(
            config,
            new FakeInputService(),
            player,
            new FakeGameTime { DeltaTime = 0.1f },
            new RunCurrencyState(),
            hud: null,
            interactableSource: new FakeInteractableSource(deposit),
            worldPrompt: worldPrompt);

        system.Tick();

        Assert.That(system.CurrentTarget, Is.SameAs(deposit));
        Assert.That(worldPrompt.LastTarget, Is.SameAs(deposit));
    }

    [Test]
    public void InteractionSystemIgnoresGameplayLayerBlockersForLineOfSight()
    {
        GameConfig config = Track(TheCircussyOneTestObjects.CreateConfig());
        config.playerInteractRadius = 5f;
        PlayerView player = CreateInteractionPlayer();
        CreateInteractionBlocker(
            new Vector3(1f, 0.75f, 0f),
            new Vector3(0.25f, 1.5f, 1.5f),
            layer: GameLayers.PickupIndex);
        var deposit = new FakeInteractable(new Vector3(2f, 0f, 0f), InteractPriority.TicketDeposit);
        var worldPrompt = new FakeWorldInteractionPrompt();
        var system = new InteractionSystem(
            config,
            new FakeInputService(),
            player,
            new FakeGameTime { DeltaTime = 0.1f },
            new RunCurrencyState(),
            hud: null,
            interactableSource: new FakeInteractableSource(deposit),
            worldPrompt: worldPrompt);

        system.Tick();

        Assert.That(system.CurrentTarget, Is.SameAs(deposit));
        Assert.That(worldPrompt.LastTarget, Is.SameAs(deposit));
    }

    [Test]
    public void InteractionSystemCancelsHeldTargetWhenLineOfSightStaysBlockedBeyondGrace()
    {
        GameConfig config = Track(TheCircussyOneTestObjects.CreateConfig());
        config.playerInteractRadius = 5f;
        config.playerInteractOutOfRangeGraceSeconds = 0.2f;
        PlayerView player = CreateInteractionPlayer();
        var input = new FakeInputService { InteractPressedThisFrame = true, InteractHeld = true };
        var time = new FakeGameTime { DeltaTime = 0.05f };
        var deposit = new FakeInteractable(new Vector3(2f, 0f, 0f), InteractPriority.TicketDeposit, holdSeconds: 2f);
        var system = new InteractionSystem(
            config,
            input,
            player,
            time,
            new RunCurrencyState(),
            hud: null,
            interactableSource: new FakeInteractableSource(deposit),
            worldPrompt: new FakeWorldInteractionPrompt());

        system.Tick();
        Assert.That(system.IsHolding, Is.True);

        input.InteractPressedThisFrame = false;
        time.DeltaTime = 0.25f;
        CreateInteractionBlocker(new Vector3(1f, 0.75f, 0f), new Vector3(0.25f, 1.5f, 1.5f));

        system.Tick();

        Assert.That(system.IsHolding, Is.False);
        Assert.That(system.CurrentTarget, Is.Null);
        Assert.That(deposit.Targeted, Is.False);
        Assert.That(deposit.Progress, Is.Zero);
    }

    private T Track<T>(T obj) where T : Object
    {
        _objects.Add(obj);
        return obj;
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

    private PlayerView CreateInteractionPlayer()
    {
        var playerObject = Track(new GameObject("Player"));
        playerObject.transform.position = Vector3.zero;
        return playerObject.AddComponent<PlayerView>();
    }

    private GameObject CreateInteractionBlocker(Vector3 position, Vector3 scale, bool isTrigger = false, int layer = 0)
    {
        var blocker = Track(GameObject.CreatePrimitive(PrimitiveType.Cube));
        blocker.name = "Interaction LOS Blocker";
        blocker.transform.position = position;
        blocker.transform.localScale = scale;
        blocker.layer = layer;
        blocker.GetComponent<Collider>().isTrigger = isTrigger;
        Physics.SyncTransforms();
        return blocker;
    }

    private static void AssertColor(Color actual, Color expected)
    {
        Assert.That(actual.r, Is.EqualTo(expected.r).Within(0.001f));
        Assert.That(actual.g, Is.EqualTo(expected.g).Within(0.001f));
        Assert.That(actual.b, Is.EqualTo(expected.b).Within(0.001f));
        Assert.That(actual.a, Is.EqualTo(expected.a).Within(0.001f));
    }

    private sealed class FakeInteractableSource : IInteractableSource
    {
        private readonly List<IInteractable> interactables;

        public FakeInteractableSource(params IInteractable[] interactables)
        {
            this.interactables = interactables.ToList();
        }

        public void GetInteractables(List<IInteractable> results)
        {
            results.AddRange(interactables);
        }
    }

    private sealed class FakeWorldInteractionPrompt : IWorldInteractionPrompt
    {
        public bool IsShowing { get; private set; }
        public int ShowCount { get; private set; }
        public int HideCount { get; private set; }
        public IWorldInteractionPromptTarget LastTarget { get; private set; }
        public string LastPrompt { get; private set; }
        public float LastProgress { get; private set; }
        public bool LastHolding { get; private set; }

        public void Show(IWorldInteractionPromptTarget target, string prompt, float progress, bool isHolding, bool useGamepadGlyph, bool showGlyph = true)
        {
            IsShowing = true;
            ShowCount++;
            LastTarget = target;
            LastPrompt = prompt;
            LastProgress = progress;
            LastHolding = isHolding;
        }

        public void Hide()
        {
            IsShowing = false;
            HideCount++;
        }
    }

    private sealed class FakeInteractable : IInteractable, ICurrencyAwareInteractable, IWorldInteractionPromptTarget
    {
        private readonly float holdSeconds;
        private readonly int ticketReward;
        private readonly string promptText;
        private readonly string currencyPromptText;
        private readonly bool canBeginInteraction;
        private float elapsedSeconds;

        public FakeInteractable(
            Vector3 position,
            InteractPriority priority,
            float holdSeconds = 1f,
            int ticketReward = 0,
            string promptText = "Collect Tickets",
            string currencyPromptText = null,
            bool canBeginInteraction = true)
        {
            InteractionPosition = position;
            Priority = priority;
            this.holdSeconds = holdSeconds;
            this.ticketReward = ticketReward;
            this.promptText = promptText;
            this.currencyPromptText = currencyPromptText ?? promptText;
            this.canBeginInteraction = canBeginInteraction;
        }

        public bool Completed { get; private set; }
        public bool Targeted { get; private set; }
        public int BeginCount { get; private set; }
        public int CancelCount { get; private set; }
        public bool IsInteractionAvailable => !Completed;
        public InteractPriority Priority { get; }
        public Vector3 InteractionPosition { get; }
        public Vector3 WorldInteractionPromptPosition => InteractionPosition + Vector3.up;
        public string PromptText => promptText;
        public float HoldSeconds => holdSeconds;
        public float Progress => Mathf.Clamp01(elapsedSeconds / holdSeconds);

        public void SetInteractionTargeted(bool targeted)
        {
            Targeted = targeted;
        }

        public void BeginInteraction()
        {
            BeginCount++;
        }

        public void TickInteraction(float deltaTime, RunCurrencyState currency)
        {
            elapsedSeconds += deltaTime;
            if (elapsedSeconds < holdSeconds || Completed)
            {
                return;
            }

            Completed = true;
            currency?.AddTickets(ticketReward);
        }

        public void CancelInteraction()
        {
            CancelCount++;
            elapsedSeconds = 0f;
        }

        public string PromptTextFor(RunCurrencyState currency)
        {
            return currencyPromptText;
        }

        public bool CanBeginInteraction(RunCurrencyState currency)
        {
            return canBeginInteraction;
        }
    }
}
