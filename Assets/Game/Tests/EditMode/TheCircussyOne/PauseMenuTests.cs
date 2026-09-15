using NUnit.Framework;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using TheCircussyOne.Runtime;
using TheCircussyOne.Stats;
using TheCircussyOne.Visuals;
using UnityEngine;
using UnityEngine.UIElements;

public sealed class PauseMenuTests
{
    [TearDown]
    public void TearDown()
    {
        TheCircussyOneTestObjects.DestroyAll();
    }

    [Test]
    public void PauseMenuInputOpensAndClosesPauseReason()
    {
        GameConfig config = TheCircussyOneTestObjects.CreateConfig();
        var state = new GameState(config);
        var pauseState = new RunPauseState();
        var input = new FakeInputService();
        PauseMenuView view = CreateView();
        var restarter = new FakeRunRestarter();
        var presenter = new PauseMenuPresenter(input, pauseState, state, restarter, view);

        presenter.Start();
        input.PausePressedThisFrame = true;
        presenter.Tick();

        Assert.That(view.IsVisible, Is.True);
        Assert.That(pauseState.HasReason(RunPauseReasons.PauseMenu), Is.True);

        input.PausePressedThisFrame = false;
        presenter.Tick();
        input.PausePressedThisFrame = true;
        presenter.Tick();

        Assert.That(view.IsVisible, Is.False);
        Assert.That(pauseState.HasReason(RunPauseReasons.PauseMenu), Is.False);

        presenter.Dispose();
        TheCircussyOneTestObjects.Destroy(config);
    }

    [Test]
    public void PauseMenuDoesNotOpenDuringRunTransition()
    {
        GameConfig config = TheCircussyOneTestObjects.CreateConfig();
        var state = new GameState(config);
        var pauseState = new RunPauseState();
        pauseState.Pause(RunPauseReasons.RunTransition);
        var input = new FakeInputService { PausePressedThisFrame = true };
        PauseMenuView view = CreateView();
        var presenter = new PauseMenuPresenter(input, pauseState, state, new FakeRunRestarter(), view);

        presenter.Start();
        presenter.Tick();

        Assert.That(view.IsVisible, Is.False);
        Assert.That(pauseState.HasReason(RunPauseReasons.PauseMenu), Is.False);
        Assert.That(pauseState.HasReason(RunPauseReasons.RunTransition), Is.True);

        presenter.Dispose();
        TheCircussyOneTestObjects.Destroy(config);
    }

    [Test]
    public void PauseMenuDoesNotOpenOverRewardReveal()
    {
        GameConfig config = TheCircussyOneTestObjects.CreateConfig();
        var state = new GameState(config);
        var pauseState = new RunPauseState();
        var input = new FakeInputService { PausePressedThisFrame = true };
        PauseMenuView pauseView = CreateView();
        RewardRevealView rewardView = CreateRewardView();
        ItemDefinition item = TheCircussyOneTestObjects.CreateItemDefinition("rubber_soles", "Rubber Soles");
        rewardView.Show(RewardRevealFrame.ForItem(item, 1, 1));
        var presenter = new PauseMenuPresenter(
            input,
            pauseState,
            state,
            new FakeRunRestarter(),
            pauseView,
            rewardReveal: rewardView);

        presenter.Start();
        presenter.Tick();

        Assert.That(pauseView.IsVisible, Is.False);
        Assert.That(pauseState.HasReason(RunPauseReasons.PauseMenu), Is.False);

        presenter.Dispose();
        TheCircussyOneTestObjects.Destroy(item);
        TheCircussyOneTestObjects.Destroy(config);
    }

    [Test]
    public void PauseMenuPresenterPassesOwnedItemsToView()
    {
        GameConfig config = TheCircussyOneTestObjects.CreateConfig();
        var state = new GameState(config);
        var pauseState = new RunPauseState();
        var input = new FakeInputService { PausePressedThisFrame = true };
        var inventoryList = new VisualElement();
        var emptyLabel = new Label();
        PauseMenuView view = CreateView(inventoryList, emptyLabel);
        ItemDefinition item = TheCircussyOneTestObjects.CreateItemDefinition("rubber_soles", "Rubber Soles");
        var inventory = new ItemInventory();
        inventory.AddItem(item, 2);
        var presenter = new PauseMenuPresenter(
            input,
            pauseState,
            state,
            new FakeRunRestarter(),
            view,
            itemInventory: inventory);

        presenter.Start();
        presenter.Tick();

        Assert.That(view.IsVisible, Is.True);
        Assert.That(inventoryList.childCount, Is.EqualTo(1));
        Assert.That(inventoryList[0].Q<Label>("pause-menu-item-title").text, Is.EqualTo("Rubber Soles"));

        presenter.Dispose();
        TheCircussyOneTestObjects.Destroy(item);
        TheCircussyOneTestObjects.Destroy(config);
    }

    [Test]
    public void RuntimeCursorTreatsPauseMenuAsMenu()
    {
        PauseMenuView view = CreateView();
        view.Show(default, null);
        var system = new RuntimeCursorSystem(new ControlInputModeTracker(), null, pauseMenu: view);
        System.Reflection.MethodInfo method = typeof(RuntimeCursorSystem).GetMethod("IsMenuOpen", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        Assert.That(method, Is.Not.Null);
        Assert.That((bool)method.Invoke(system, null), Is.True);
    }

    [Test]
    public void PauseMenuShowsEmptyItemInventoryState()
    {
        var inventoryList = new VisualElement();
        var emptyLabel = new Label();
        PauseMenuView view = CreateView(inventoryList, emptyLabel);

        view.Show(default, null, ItemInventoryPanelRules.BuildFrames(null));

        Assert.That(emptyLabel.resolvedStyle.display, Is.EqualTo(DisplayStyle.Flex));
        Assert.That(inventoryList.childCount, Is.Zero);
    }

    [Test]
    public void PauseMenuRendersOwnedItemCards()
    {
        var inventoryList = new VisualElement();
        var emptyLabel = new Label();
        PauseMenuView view = CreateView(inventoryList, emptyLabel);
        ItemDefinition item = TheCircussyOneTestObjects.CreateItemDefinition(
            "rubber_soles",
            "Rubber Soles",
            maxStacks: 5,
            modifiers: new ItemStatModifierDefinition(StatId.PlayerArmor, StatModifierBucket.Flat, 2f));
        item.shortDescription = "Keeps the sawdust quiet.";

        var frames = ItemInventoryPanelRules.BuildFrames(new[] { new ItemStackRuntime(item, 2) });

        view.Show(default, null, frames);

        Assert.That(emptyLabel.resolvedStyle.display, Is.EqualTo(DisplayStyle.None));
        Assert.That(inventoryList.childCount, Is.EqualTo(1));
        VisualElement card = inventoryList[0];
        Assert.That(card.Q<Label>("pause-menu-item-title").text, Is.EqualTo("Rubber Soles"));
        Assert.That(card.Q<Label>("pause-menu-item-description").text, Is.EqualTo("Keeps the sawdust quiet."));
        Label effect = card.Q<Label>("pause-menu-item-effect");
        Assert.That(effect.text, Does.Contain("Armor <color=#73F59A>+4</color>"));
        Assert.That(effect.enableRichText, Is.True);

        TheCircussyOneTestObjects.Destroy(item);
    }

    [Test]
    public void PauseMenuShowsWorldLayoutSwitch()
    {
        var debugRoot = new VisualElement();
        PauseMenuView view = CreateView(debugRoot: debugRoot);
        var snapshot = new RunDebugSnapshot(
            true,
            0,
            100,
            100,
            0,
            1,
            RunPhase.WorldActive,
            0f,
            1,
            null,
            null,
            null,
            null,
            null,
            worldLayoutMode: RunWorldLayoutMode.Prototype);

        view.Show(snapshot, null);

        Assert.That(debugRoot.Q<Label>("pause-menu-world-layout").text, Is.EqualTo("World: Prototype Arena"));
        Assert.That(debugRoot.Q<Button>().text, Is.EqualTo("Use Generated Terrain"));
    }

    private static PauseMenuView CreateView(
        VisualElement inventoryList = null,
        Label inventoryEmptyLabel = null,
        VisualElement debugRoot = null)
    {
        var gameObject = new GameObject("TCO Test Pause Menu View");
        var view = gameObject.AddComponent<PauseMenuView>();
        view.ConfigureForTests(new VisualElement(), new Label(), debugRoot ?? new VisualElement(), new Label(), inventoryList, inventoryEmptyLabel);
        return view;
    }

    private static RewardRevealView CreateRewardView()
    {
        var gameObject = new GameObject("TCO Test Reward Reveal View");
        var view = gameObject.AddComponent<RewardRevealView>();
        view.ConfigureForTests(new VisualElement(), new VisualElement(), new VisualElement(), new Label(), new Label(), new Label(), new Label(), new Label(), new Button());
        return view;
    }
}
