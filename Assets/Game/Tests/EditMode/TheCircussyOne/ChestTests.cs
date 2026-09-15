using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using TheCircussyOne.Runtime;
using TheCircussyOne.Visuals;
using UnityEngine;

public sealed class ChestTests
{
    private readonly List<Object> _objects = new();

    [TearDown]
    public void TearDown()
    {
        DestroyRuntimeRewardViews<ChestView>();
        DestroyRuntimeRewardViews<TicketDepositView>();
        DestroyRuntimeRewardViews<HealingPropView>();

        for (int i = 0; i < _objects.Count; i++)
        {
            TheCircussyOneTestObjects.Destroy(_objects[i]);
        }

        _objects.Clear();
        SceneInteractableRegistry.Clear();
        InteractableSelectionOutlineRegistry.Clear();
    }

    private static void DestroyRuntimeRewardViews<TView>() where TView : Component
    {
        TView[] views = Object.FindObjectsByType<TView>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < views.Length; i++)
        {
            if (views[i] != null)
            {
                Object.DestroyImmediate(views[i].gameObject);
            }
        }
    }

    [Test]
    public void ChestDefinitionDefaultsExposeWorldRewardIdentity()
    {
        ChestDefinition chest = Track(TheCircussyOneTestObjects.CreateChestDefinition(
            "locked_chest",
            "Locked Chest"));

        Assert.That(chest.Id, Is.EqualTo("locked_chest"));
        Assert.That(chest.DisplayName, Is.EqualTo("Locked Chest"));
        Assert.That(chest.Tags.HasTag(ContentTag.Chest), Is.True);
        Assert.That(chest.Tags.HasTag(ContentTag.Economy), Is.True);
        Assert.That(chest.TicketCost, Is.EqualTo(35));
        Assert.That(chest.itemRarityWeights.IsValid, Is.True);
        Assert.That(chest.interactionSquashStretchAmplitude, Is.GreaterThan(0f));
        Assert.That(chest.interactionSquashStretchFrequency, Is.GreaterThan(0f));
        Assert.That(chest.openDisappearSeconds, Is.GreaterThan(0f));
        Assert.That(chest.openShrinkFinalScaleMultiplier, Is.InRange(0f, 1f));
        Assert.That(chest.openDisappearEase.preset, Is.EqualTo(EasePreset.InBack));
    }

    [Test]
    public void ChestCatalogValidatesInvalidDefinitions()
    {
        ChestDefinition first = Track(TheCircussyOneTestObjects.CreateChestDefinition("locked_chest", "Locked Chest"));
        ChestDefinition duplicate = Track(TheCircussyOneTestObjects.CreateChestDefinition("locked_chest", "Duplicate Chest"));
        ChestDefinition invalid = Track(TheCircussyOneTestObjects.CreateChestDefinition("", ""));
        invalid.chestId = "";
        invalid.displayName = "";
        invalid.holdSeconds = 0f;
        invalid.ticketCost = -1;
        invalid.itemRarityWeights = default;
        invalid.emissionStrength = -1f;
        invalid.targetOutlineThickness = -1f;
        invalid.interactionSquashStretchAmplitude = -1f;
        invalid.interactionSquashStretchFrequency = -1f;
        invalid.openDisappearSeconds = -1f;
        invalid.openShrinkFinalScaleMultiplier = 1.5f;
        invalid.placement.footprintRadius = 0f;
        invalid.placement.groundClearance = -1f;
        invalid.placement.maxPlacementSlope = 90f;
        invalid.placement.blocksPlayer = true;
        invalid.placement.bodyColliderSize = Vector3.zero;

        ChestCatalog catalog = Track(TheCircussyOneTestObjects.CreateChestCatalog(first, duplicate, invalid, null));

        var issues = catalog.ValidateContent();

        Assert.That(issues.Any(issue => issue.Code == "catalog.null-entry"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "content.duplicate-id"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "content.invalid-id"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "content.missing-display-name"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "chest.invalid-hold-seconds"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "chest.invalid-ticket-cost"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "chest.empty-rarity-table"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "chest.invalid-emission-strength"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "chest.invalid-outline-thickness"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "chest.invalid-interaction-squash-amplitude"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "chest.invalid-interaction-squash-frequency"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "chest.invalid-open-disappear-seconds"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "chest.invalid-open-shrink-scale"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "chest.invalid-footprint-radius"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "chest.invalid-ground-clearance"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "chest.invalid-placement-slope"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "chest.invalid-body-collider-size"), Is.True);
    }

    [Test]
    public void ChestRewardRulesRollActiveAddableItemsAndFallbackRarity()
    {
        ChestDefinition chest = Track(TheCircussyOneTestObjects.CreateChestDefinition(
            "locked_chest",
            "Locked Chest",
            rarityWeights: new ContentRarityWeightTable { legendary = 1 }));
        ItemDefinition common = Track(TheCircussyOneTestObjects.CreateItemDefinition("rubber_soles", "Rubber Soles"));
        common.rarity = ContentRarity.Common;
        ItemDefinition rare = Track(TheCircussyOneTestObjects.CreateItemDefinition("lucky_coin", "Lucky Coin"));
        rare.rarity = ContentRarity.Rare;
        ItemDefinition inactive = Track(TheCircussyOneTestObjects.CreateItemDefinition("inactive", "Inactive"));
        inactive.rarity = ContentRarity.Legendary;
        inactive.isActive = false;
        ItemCatalog catalog = Track(TheCircussyOneTestObjects.CreateItemCatalog(common, rare, inactive));
        var inventory = new ItemInventory();

        ChestRewardRoll roll = ChestRewardRules.Roll(chest, catalog, inventory, stats: null, seed: 1234);

        Assert.That(roll.HasReward, Is.True);
        Assert.That(roll.Item, Is.SameAs(rare));
        Assert.That(roll.RolledRarity, Is.EqualTo(ContentRarity.Rare));
    }

    [Test]
    public void ChestRewardStableSeedIncludesRunSeedWhenProvided()
    {
        Vector3 position = new(4f, 0f, -2f);

        int firstRun = ChestRewardRules.StableSeed("locked_chest", position, runSeed: 11);
        int secondRun = ChestRewardRules.StableSeed("locked_chest", position, runSeed: 12);

        Assert.That(ChestRewardRules.StableSeed("locked_chest", position, runSeed: 11), Is.EqualTo(firstRun));
        Assert.That(secondRun, Is.Not.EqualTo(firstRun));
    }

    [Test]
    public void ChestRuntimeSpendsTicketsAndGrantsChestSourceItem()
    {
        ChestDefinition chest = Track(TheCircussyOneTestObjects.CreateChestDefinition(
            "locked_chest",
            "Locked Chest",
            ticketCost: 35,
            holdSeconds: 1f,
            rarityWeights: new ContentRarityWeightTable { common = 1 }));
        ItemDefinition item = Track(TheCircussyOneTestObjects.CreateItemDefinition("rubber_soles", "Rubber Soles"));
        ItemCatalog catalog = Track(TheCircussyOneTestObjects.CreateItemCatalog(item));
        var inventory = new ItemInventory();
        var currency = new RunCurrencyState();
        currency.AddTickets(40);
        var events = new List<ItemAddedEvent>();
        inventory.ItemAdded += events.Add;
        var runtime = new ChestRuntime(chest, catalog, inventory, stats: null, Vector3.zero);

        bool opened = runtime.Tick(1f, currency);

        Assert.That(opened, Is.True);
        Assert.That(runtime.Completed, Is.True);
        Assert.That(currency.Tickets, Is.EqualTo(5));
        Assert.That(inventory.OwnsItem(item.Id), Is.True);
        Assert.That(events.Count, Is.EqualTo(1));
        Assert.That(events[0].Source, Is.EqualTo(ItemGrantSource.Chest));
    }

    [Test]
    public void ChestRuntimeDoesNotOpenOrSpendWhenNoRewardIsAvailable()
    {
        ChestDefinition chest = Track(TheCircussyOneTestObjects.CreateChestDefinition(
            "locked_chest",
            "Locked Chest",
            ticketCost: 35,
            holdSeconds: 1f));
        ItemCatalog emptyCatalog = Track(TheCircussyOneTestObjects.CreateItemCatalog());
        var inventory = new ItemInventory();
        var currency = new RunCurrencyState();
        currency.AddTickets(40);
        var runtime = new ChestRuntime(chest, emptyCatalog, inventory, stats: null, Vector3.zero);

        bool opened = runtime.Tick(1f, currency);

        Assert.That(opened, Is.False);
        Assert.That(runtime.Completed, Is.False);
        Assert.That(currency.Tickets, Is.EqualTo(40));
        Assert.That(inventory.ItemCount, Is.Zero);
    }

    [Test]
    public void ChestViewShowsAffordableAndUnaffordableTicketPrompts()
    {
        ChestDefinition chest = Track(TheCircussyOneTestObjects.CreateChestDefinition(
            "locked_chest",
            "Locked Chest",
            ticketCost: 35));
        ItemDefinition item = Track(TheCircussyOneTestObjects.CreateItemDefinition("rubber_soles", "Rubber Soles"));
        ItemCatalog catalog = Track(TheCircussyOneTestObjects.CreateItemCatalog(item));
        GameObject root = Track(GameObject.CreatePrimitive(PrimitiveType.Cube));
        var view = root.AddComponent<ChestView>();
        view.Prepare(chest, catalog, new ItemInventory(), null);
        var currency = new RunCurrencyState();

        view.SetInteractionTargeted(true);

        Assert.That(view.PromptTextFor(currency), Is.EqualTo("Need <color=#ff4758>35</color> more Tickets"));
        Assert.That(view.CanBeginInteraction(currency), Is.False);
        Assert.That(InteractableSelectionOutlineRegistry.TryGetCompositeSettings(out var shortageSettings), Is.True);
        AssertColor(shortageSettings.Color, chest.insufficientTicketsOutlineColor);

        currency.AddTickets(35);

        Assert.That(view.PromptTextFor(currency), Is.EqualTo("Spend 35 Tickets to Open Chest"));
        Assert.That(view.CanBeginInteraction(currency), Is.True);
        Assert.That(InteractableSelectionOutlineRegistry.TryGetCompositeSettings(out var affordableSettings), Is.True);
        AssertColor(affordableSettings.Color, chest.targetOutlineColor);
    }

    [Test]
    public void ChestViewSquashesAndStretchesWhileInteracting()
    {
        ChestDefinition chest = Track(TheCircussyOneTestObjects.CreateChestDefinition(
            "locked_chest",
            "Locked Chest",
            ticketCost: 0,
            holdSeconds: 10f));
        chest.visualScale = 1f;
        chest.interactionSquashStretchAmplitude = 0.1f;
        chest.interactionSquashStretchFrequency = 1f;
        ItemCatalog catalog = Track(TheCircussyOneTestObjects.CreateItemCatalog());
        GameObject root = Track(GameObject.CreatePrimitive(PrimitiveType.Cube));
        var view = root.AddComponent<ChestView>();
        view.Prepare(chest, catalog, new ItemInventory(), null);

        view.BeginInteraction();
        view.TickInteraction(0.25f, new RunCurrencyState());

        Assert.That(root.transform.localScale.x, Is.EqualTo(0.95f).Within(0.001f));
        Assert.That(root.transform.localScale.y, Is.EqualTo(1.1f).Within(0.001f));
        Assert.That(root.transform.localScale.z, Is.EqualTo(0.95f).Within(0.001f));

        view.CancelInteraction();

        Assert.That(root.transform.localScale, Is.EqualTo(Vector3.one));
    }

    [Test]
    public void ChestViewStaysVisibleAndOpenAfterSuccessfulInteraction()
    {
        ChestDefinition chest = Track(TheCircussyOneTestObjects.CreateChestDefinition(
            "locked_chest",
            "Locked Chest",
            ticketCost: 0,
            holdSeconds: 0.1f,
            rarityWeights: new ContentRarityWeightTable { common = 1 }));
        ItemDefinition item = Track(TheCircussyOneTestObjects.CreateItemDefinition("rubber_soles", "Rubber Soles"));
        ItemCatalog catalog = Track(TheCircussyOneTestObjects.CreateItemCatalog(item));
        GameObject root = Track(GameObject.CreatePrimitive(PrimitiveType.Cube));
        GameObject lid = Track(GameObject.CreatePrimitive(PrimitiveType.Cube));
        lid.name = "Lid";
        lid.transform.SetParent(root.transform, worldPositionStays: false);
        lid.transform.localPosition = new Vector3(0f, 0.58f, 0f);
        var view = root.AddComponent<ChestView>();
        view.Prepare(chest, catalog, new ItemInventory(), null);
        Quaternion closedRotation = view.LidRoot.localRotation;

        view.BeginInteraction();
        view.TickInteraction(0.1f, new RunCurrencyState());

        Assert.That(root.activeSelf, Is.True);
        Assert.That(view.IsOpen, Is.True);
        Assert.That(view.IsInteractionAvailable, Is.False);
        Assert.That(view.CanBeginInteraction(new RunCurrencyState()), Is.False);
        Assert.That(Quaternion.Angle(closedRotation, view.LidRoot.localRotation), Is.GreaterThan(10f));
    }

    [Test]
    public void ChestViewShrinksAwayWithBackEaseAfterOpening()
    {
        ChestDefinition chest = Track(TheCircussyOneTestObjects.CreateChestDefinition(
            "locked_chest",
            "Locked Chest",
            ticketCost: 0,
            holdSeconds: 0.1f,
            rarityWeights: new ContentRarityWeightTable { common = 1 }));
        chest.visualScale = 1f;
        chest.openDisappearSeconds = 1f;
        chest.openShrinkFinalScaleMultiplier = 0.02f;
        chest.openDisappearEase = EaseSettings.InBack;
        ItemDefinition item = Track(TheCircussyOneTestObjects.CreateItemDefinition("rubber_soles", "Rubber Soles"));
        ItemCatalog catalog = Track(TheCircussyOneTestObjects.CreateItemCatalog(item));
        GameObject root = Track(GameObject.CreatePrimitive(PrimitiveType.Cube));
        Renderer renderer = root.GetComponent<Renderer>();
        var view = root.AddComponent<ChestView>();
        view.Prepare(chest, catalog, new ItemInventory(), null);
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
    }

    [Test]
    public void ChestViewOpenDespawnUsesConfiguredDuration()
    {
        ChestDefinition chest = Track(TheCircussyOneTestObjects.CreateChestDefinition(
            "locked_chest",
            "Locked Chest",
            ticketCost: 0,
            holdSeconds: 0.1f,
            rarityWeights: new ContentRarityWeightTable { common = 1 }));
        chest.openDisappearSeconds = 2f;
        chest.openDisappearEase = EaseSettings.Linear;
        ItemDefinition item = Track(TheCircussyOneTestObjects.CreateItemDefinition("rubber_soles", "Rubber Soles"));
        ItemCatalog catalog = Track(TheCircussyOneTestObjects.CreateItemCatalog(item));
        GameObject root = Track(GameObject.CreatePrimitive(PrimitiveType.Cube));
        var view = root.AddComponent<ChestView>();
        view.Prepare(chest, catalog, new ItemInventory(), null);

        view.BeginInteraction();
        view.TickInteraction(0.1f, new RunCurrencyState());
        view.TickOpenDespawn(1.99f);

        Assert.That(root.activeSelf, Is.True);
        Assert.That(view.OpenDespawnElapsedSeconds, Is.EqualTo(1.99f).Within(0.001f));

        view.TickOpenDespawn(0.02f);

        Assert.That(root.activeSelf, Is.False);
        Assert.That(view.OpenDespawnElapsedSeconds, Is.EqualTo(2f).Within(0.001f));
    }

    [Test]
    public void ChestViewOpenDespawnDoesNotAdvanceWhileRunPaused()
    {
        ChestDefinition chest = Track(TheCircussyOneTestObjects.CreateChestDefinition(
            "locked_chest",
            "Locked Chest",
            ticketCost: 0,
            holdSeconds: 0.1f,
            rarityWeights: new ContentRarityWeightTable { common = 1 }));
        chest.visualScale = 1f;
        chest.openDisappearSeconds = 1f;
        chest.openDisappearEase = EaseSettings.InBack;
        ItemDefinition item = Track(TheCircussyOneTestObjects.CreateItemDefinition("rubber_soles", "Rubber Soles"));
        ItemCatalog catalog = Track(TheCircussyOneTestObjects.CreateItemCatalog(item));
        GameObject root = Track(GameObject.CreatePrimitive(PrimitiveType.Cube));
        var pauseState = new RunPauseState();
        var view = root.AddComponent<ChestView>();
        view.Prepare(chest, catalog, new ItemInventory(), null, pauseState);
        view.BeginInteraction();
        view.TickInteraction(0.1f, new RunCurrencyState());
        pauseState.Pause(RunPauseReasons.RewardReveal);

        view.TickOpenDespawn(0.5f);

        Assert.That(view.OpenDespawnElapsedSeconds, Is.Zero);
        Assert.That(root.activeSelf, Is.True);
        Assert.That(root.transform.localScale, Is.EqualTo(Vector3.one));

        pauseState.Resume(RunPauseReasons.RewardReveal);
        view.TickOpenDespawn(0.1f);

        Assert.That(view.OpenDespawnElapsedSeconds, Is.EqualTo(0.1f).Within(0.001f));
        Assert.That(root.transform.localScale.x, Is.GreaterThan(1f));
    }

    [Test]
    public void ChestViewPromptAnchorUsesRendererBoundsTop()
    {
        ChestDefinition chest = Track(TheCircussyOneTestObjects.CreateChestDefinition(
            "locked_chest",
            "Locked Chest",
            ticketCost: 0));
        ItemCatalog catalog = Track(TheCircussyOneTestObjects.CreateItemCatalog());
        GameObject root = Track(GameObject.CreatePrimitive(PrimitiveType.Cube));
        root.transform.position = new Vector3(2f, 3f, 4f);
        root.transform.localScale = new Vector3(1.2f, 0.75f, 0.9f);
        Renderer renderer = root.GetComponent<Renderer>();
        var view = root.AddComponent<ChestView>();
        view.Prepare(chest, catalog, new ItemInventory(), null);

        Vector3 promptPosition = view.WorldInteractionPromptPosition;

        Assert.That(promptPosition.x, Is.EqualTo(renderer.bounds.center.x).Within(0.001f));
        Assert.That(promptPosition.y, Is.EqualTo(renderer.bounds.max.y).Within(0.001f));
        Assert.That(promptPosition.z, Is.EqualTo(renderer.bounds.center.z).Within(0.001f));
    }

    [Test]
    public void ChestSystemSpawningDoesNotSuppressTicketDepositSpawning()
    {
        GameConfig config = Track(TheCircussyOneTestObjects.CreateConfig());
        var playerObject = Track(new GameObject("Player"));
        var player = playerObject.AddComponent<PlayerView>();
        ChestDefinition chest = Track(TheCircussyOneTestObjects.CreateChestDefinition("locked_chest", "Locked Chest", ticketCost: 0));
        ChestCatalog chestCatalog = Track(TheCircussyOneTestObjects.CreateChestCatalog(chest));
        ItemDefinition item = Track(TheCircussyOneTestObjects.CreateItemDefinition("rubber_soles", "Rubber Soles"));
        ItemCatalog itemCatalog = Track(TheCircussyOneTestObjects.CreateItemCatalog(item));
        TicketDepositDefinition deposit = Track(TheCircussyOneTestObjects.CreateTicketDepositDefinition("small_ticket_stack", "Small Ticket Stack"));
        TicketDepositCatalog depositCatalog = Track(TheCircussyOneTestObjects.CreateTicketDepositCatalog(deposit));

        new ChestSystem(chestCatalog, itemCatalog, new ItemInventory(), null, player).SpawnWorld(1);
        new TicketDepositSystem(depositCatalog, player).SpawnWorld(1);

        Assert.That(SceneInteractableRegistry.CountByKind(SceneInteractableKind.Chest), Is.GreaterThan(0));
        Assert.That(SceneInteractableRegistry.CountByKind(SceneInteractableKind.TicketDeposit), Is.GreaterThan(0));
    }

    private T Track<T>(T obj) where T : Object
    {
        _objects.Add(obj);
        return obj;
    }

    private static void AssertColor(Color actual, Color expected)
    {
        Assert.That(actual.r, Is.EqualTo(expected.r).Within(0.001f));
        Assert.That(actual.g, Is.EqualTo(expected.g).Within(0.001f));
        Assert.That(actual.b, Is.EqualTo(expected.b).Within(0.001f));
        Assert.That(actual.a, Is.EqualTo(expected.a).Within(0.001f));
    }
}
