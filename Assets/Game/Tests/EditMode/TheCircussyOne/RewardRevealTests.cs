using System.Reflection;
using NUnit.Framework;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using TheCircussyOne.Runtime;
using TheCircussyOne.Stats;
using TheCircussyOne.Visuals;
using UnityEngine;
using UnityEngine.UIElements;

public sealed class RewardRevealTests
{
    private GameObject viewObject;

    [TearDown]
    public void TearDown()
    {
        if (viewObject != null)
        {
            Object.DestroyImmediate(viewObject);
        }
    }

    [Test]
    public void RewardRevealFrameFormatsItemAndChestCopy()
    {
        ItemDefinition item = TheCircussyOneTestObjects.CreateItemDefinition("rubber_soles", "Rubber Soles", maxStacks: 5);
        item.rarity = ContentRarity.Rare;

        RewardRevealFrame itemFrame = RewardRevealFrame.ForItem(item, 1, 1);
        RewardRevealFrame chestFrame = RewardRevealFrame.ForChest(item, 2, 3);

        Assert.That(itemFrame.Title, Is.EqualTo("ITEM OBTAINED"));
        Assert.That(itemFrame.DisplayName, Is.EqualTo("Rubber Soles"));
        Assert.That(itemFrame.RarityLabel, Is.EqualTo("RARE"));
        Assert.That(itemFrame.EffectText, Does.Contain("Armor <color=#73F59A>+1</color>"));
        Assert.That(itemFrame.EffectText, Does.Not.Contain("(Player, flat)"));
        Assert.That(itemFrame.StackText, Is.EqualTo("NEW ITEM"));
        Assert.That(chestFrame.Title, Is.EqualTo("CHEST OPENED"));
        Assert.That(chestFrame.EffectText, Does.Contain("Armor <color=#73F59A>+3</color>"));
        Assert.That(chestFrame.StackText, Is.EqualTo("+2 STACKS  |  STACK 3"));

        TheCircussyOneTestObjects.Destroy(item);
    }

    [Test]
    public void RewardRevealFrameShowsStackAwareUpsidesAndDownsides()
    {
        ItemDefinition item = TheCircussyOneTestObjects.CreateItemDefinition(
            "heavy_ticket",
            "Heavy Ticket",
            maxStacks: 3,
            modifiers: new ItemStatModifierDefinition(StatId.PlayerArmor, StatModifierBucket.Flat, 2f));
        item.downsideStatModifiers.Add(new ItemStatModifierDefinition(StatId.PlayerMoveSpeedMultiplier, StatModifierBucket.AdditivePercent, -0.05f));

        RewardRevealFrame frame = RewardRevealFrame.ForChest(item, 1, 2);

        Assert.That(frame.EffectText, Does.Contain("Armor <color=#73F59A>+4</color>"));
        Assert.That(frame.EffectText, Does.Contain("Movement Speed <color=#FF5A66>-10%</color>"));

        TheCircussyOneTestObjects.Destroy(item);
    }

    [Test]
    public void RewardRevealFrameShowsFractionalIntegerStatProgress()
    {
        ItemDefinition item = TheCircussyOneTestObjects.CreateItemDefinition(
            "springboard_stub",
            "Springboard Stub",
            maxStacks: 2,
            modifiers: new ItemStatModifierDefinition(StatId.PlayerExtraJumps, StatModifierBucket.Flat, 0.5f));

        RewardRevealFrame frame = RewardRevealFrame.ForItem(item, 1, 1);

        Assert.That(frame.EffectText, Does.Contain("Extra Jumps <color=#73F59A>+0.5</color>"));
        Assert.That(frame.EffectText, Does.Not.Contain("(Player, flat)"));

        TheCircussyOneTestObjects.Destroy(item);
    }

    [Test]
    public void RewardRevealViewShowsIconTextAndDismissEvent()
    {
        RewardRevealView view = CreateConfiguredView(
            out VisualElement overlay,
            out VisualElement card,
            out VisualElement icon,
            out Label title,
            out Label rarity,
            out Label name,
            out Label description,
            out Label effect,
            out Label stack,
            out _);
        ItemDefinition item = TheCircussyOneTestObjects.CreateItemDefinition("rubber_soles", "Rubber Soles", maxStacks: 5);
        Sprite itemIcon = CreateSprite(Color.white);
        item.rarity = ContentRarity.Epic;
        item.iconColor = Color.yellow;
        item.iconSprite = itemIcon;
        int dismisses = 0;
        view.DismissRequested += () => dismisses++;

        view.Show(RewardRevealFrame.ForItem(item, 1, 1));

        Assert.That(view.IsVisible, Is.True);
        Assert.That(overlay.style.display.value, Is.EqualTo(DisplayStyle.Flex));
        Assert.That(title.text, Is.EqualTo("ITEM OBTAINED"));
        Assert.That(rarity.text, Is.EqualTo("EPIC"));
        Assert.That(name.text, Is.EqualTo("Rubber Soles"));
        Assert.That(description.text, Is.EqualTo(item.shortDescription));
        Assert.That(effect.text, Does.Contain("Armor <color=#73F59A>+1</color>"));
        Assert.That(effect.enableRichText, Is.True);
        Assert.That(stack.text, Is.EqualTo("NEW ITEM"));
        Assert.That(view.CanDismiss, Is.False);
        Assert.That(icon.style.backgroundColor.value, Is.EqualTo(new Color(0.015f, 0.018f, 0.024f, 0.98f)));
        Assert.That(icon.style.borderTopColor.value, Is.EqualTo(ContentRarityMetadata.Get(ContentRarity.Epic).Color));
        Image image = icon.Q<Image>(ContentIconVisuals.ImageElementName);
        Assert.That(image, Is.Not.Null);
        Assert.That(image.sprite, Is.SameAs(itemIcon));
        Assert.That(image.scaleMode, Is.EqualTo(ScaleMode.ScaleToFit));
        Assert.That(card.style.borderTopColor.value, Is.EqualTo(ContentRarityMetadata.Get(ContentRarity.Epic).Color));

        view.TickPopupMotion(0.5f);
        Assert.That(view.CanDismiss, Is.True);
        InvokeDismiss(view);

        Assert.That(dismisses, Is.EqualTo(1));

        TheCircussyOneTestObjects.Destroy(item);
        DestroySprite(itemIcon);
    }

    [Test]
    public void RewardRevealPresenterPausesOnItemPickupAndDismissesOnSubmit()
    {
        RewardRevealView view = CreateConfiguredView(out VisualElement overlay, out _, out _, out Label title, out _, out Label name, out _, out _, out _, out _);
        ItemDefinition item = TheCircussyOneTestObjects.CreateItemDefinition("magnet_charm", "Magnet Charm", maxStacks: 2);
        var inventory = new ItemInventory();
        var pauseState = new RunPauseState();
        var input = new FakeInputService();
        var presenter = new RewardRevealPresenter(inventory, pauseState, view, input);

        presenter.Start();
        inventory.AddItem(item);

        Assert.That(view.IsVisible, Is.True);
        Assert.That(pauseState.IsPaused, Is.True);
        Assert.That(overlay.style.display.value, Is.EqualTo(DisplayStyle.Flex));
        Assert.That(title.text, Is.EqualTo("ITEM OBTAINED"));
        Assert.That(name.text, Is.EqualTo("Magnet Charm"));

        input.SubmitPressedThisFrame = true;
        presenter.Tick();

        Assert.That(view.IsVisible, Is.True);
        Assert.That(pauseState.IsPaused, Is.True);

        input.SubmitPressedThisFrame = false;
        view.TickPopupMotion(0.5f);
        Assert.That(view.CanDismiss, Is.True);

        input.SubmitPressedThisFrame = true;
        presenter.Tick();

        Assert.That(view.IsVisible, Is.False);
        Assert.That(overlay.style.display.value, Is.EqualTo(DisplayStyle.None));
        Assert.That(pauseState.IsPaused, Is.False);

        presenter.Dispose();
        TheCircussyOneTestObjects.Destroy(item);
    }

    [Test]
    public void RewardRevealPresenterUsesChestTitleForChestSourceItems()
    {
        RewardRevealView view = CreateConfiguredView(out _, out _, out _, out Label title, out _, out Label name, out _, out _, out _, out _);
        ItemDefinition item = TheCircussyOneTestObjects.CreateItemDefinition("rubber_soles", "Rubber Soles", maxStacks: 2);
        var inventory = new ItemInventory();
        var pauseState = new RunPauseState();
        var presenter = new RewardRevealPresenter(inventory, pauseState, view);

        presenter.Start();
        inventory.AddItem(item, 1, ItemGrantSource.Chest);

        Assert.That(view.IsVisible, Is.True);
        Assert.That(title.text, Is.EqualTo("CHEST OPENED"));
        Assert.That(name.text, Is.EqualTo("Rubber Soles"));

        presenter.Dispose();
        TheCircussyOneTestObjects.Destroy(item);
    }

    [Test]
    public void RewardRevealPresenterResetRunStateClearsVisibleAndQueuedRewards()
    {
        RewardRevealView view = CreateConfiguredView(out _, out _, out _, out _, out _, out Label name, out _, out _, out _, out _);
        ItemDefinition first = TheCircussyOneTestObjects.CreateItemDefinition("first_item", "First Item", maxStacks: 2);
        ItemDefinition queued = TheCircussyOneTestObjects.CreateItemDefinition("queued_item", "Queued Item", maxStacks: 2);
        ItemDefinition fresh = TheCircussyOneTestObjects.CreateItemDefinition("fresh_item", "Fresh Item", maxStacks: 2);
        var inventory = new ItemInventory();
        var pauseState = new RunPauseState();
        var presenter = new RewardRevealPresenter(inventory, pauseState, view);

        presenter.Start();
        inventory.AddItem(first);
        inventory.AddItem(queued);

        Assert.That(view.IsVisible, Is.True);
        Assert.That(pauseState.IsPaused, Is.True);

        presenter.ResetRunState();
        inventory.AddItem(fresh);

        Assert.That(view.IsVisible, Is.True);
        Assert.That(name.text, Is.EqualTo("Fresh Item"));

        presenter.Dispose();
        TheCircussyOneTestObjects.Destroy(first);
        TheCircussyOneTestObjects.Destroy(queued);
        TheCircussyOneTestObjects.Destroy(fresh);
    }

    private RewardRevealView CreateConfiguredView(
        out VisualElement overlay,
        out VisualElement card,
        out VisualElement icon,
        out Label title,
        out Label rarity,
        out Label name,
        out Label description,
        out Label effect,
        out Label stack,
        out Button dismiss)
    {
        viewObject = new GameObject("TCO Test Reward Reveal View");
        var view = viewObject.AddComponent<RewardRevealView>();
        overlay = new VisualElement();
        card = new VisualElement();
        icon = new VisualElement();
        title = new Label();
        rarity = new Label();
        name = new Label();
        description = new Label();
        effect = new Label();
        stack = new Label();
        dismiss = new Button();
        view.ConfigureForTests(overlay, card, icon, title, rarity, name, description, effect, stack, dismiss);
        return view;
    }

    private static void InvokeDismiss(RewardRevealView view)
    {
        MethodInfo method = typeof(RewardRevealView).GetMethod("OnDismissClicked", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(method, Is.Not.Null);
        method.Invoke(view, null);
    }

    private static Sprite CreateSprite(Color color)
    {
        var texture = new Texture2D(4, 4, TextureFormat.RGBA32, false);
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, 4f, 4f), new Vector2(0.5f, 0.5f));
    }

    private static void DestroySprite(Sprite sprite)
    {
        if (sprite == null)
        {
            return;
        }

        Texture2D texture = sprite.texture;
        Object.DestroyImmediate(sprite);
        Object.DestroyImmediate(texture);
    }
}
