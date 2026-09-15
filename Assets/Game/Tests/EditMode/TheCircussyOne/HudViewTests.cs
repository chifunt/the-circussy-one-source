using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using TheCircussyOne.Runtime;
using TheCircussyOne.Visuals;

public sealed class HudViewTests
{
    private GameObject _hudObject;

    [TearDown]
    public void TearDown()
    {
        if (_hudObject != null)
        {
            Object.DestroyImmediate(_hudObject);
        }
    }

    [Test]
    public void HudUxmlUsesCompactSurvivorslikeLayout()
    {
        var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Game/UI/Hud.uxml");
        Assert.That(asset, Is.Not.Null);

        VisualElement root = asset.CloneTree();

        Assert.That(root.Q<VisualElement>(className: "xp-strip"), Is.Not.Null);
        Assert.That(root.Q<VisualElement>(className: "health-cluster"), Is.Not.Null);
        Assert.That(root.Q<VisualElement>("health-segments"), Is.Not.Null);
        Assert.That(root.Q<Label>("level-value")?.text, Is.EqualTo("1"));
        Assert.That(root.Q<VisualElement>(className: "xp-track")?.Contains(root.Q<Label>("level-value")), Is.False);
        Assert.That(root.Q<VisualElement>(className: "kills-cluster"), Is.Not.Null);
        Assert.That(root.Q<VisualElement>(className: "hud-under-row")?.Contains(root.Q<VisualElement>(className: "tickets-cluster")), Is.True);
        Assert.That(root.Q<VisualElement>("tickets-icon"), Is.Not.Null);
        Assert.That(root.Q<VisualElement>("timer-icon"), Is.Not.Null);
        Assert.That(root.Q<VisualElement>("kills-icon"), Is.Not.Null);
        Assert.That(root.Q<Label>("timer-value")?.text, Is.EqualTo("00:00"));
        Assert.That(root.Q<Label>("xp-value")?.text, Is.EqualTo(string.Empty));
        Assert.That(root.Q<VisualElement>("xp-pulse"), Is.Not.Null);
        Assert.That(root.Q<VisualElement>("xp-segments"), Is.Not.Null);
        Assert.That(root.Q<VisualElement>("upgrade-selection"), Is.Not.Null);
        Assert.That(root.Q<VisualElement>("upgrade-ray-layer"), Is.Not.Null);
        Assert.That(root.Q<VisualElement>("upgrade-heading-stack"), Is.Not.Null);
        Assert.That(root.Q<Label>("upgrade-heading-glow"), Is.Not.Null);
        Assert.That(root.Q<Label>("upgrade-heading"), Is.Not.Null);
        Assert.That(root.Q<VisualElement>("upgrade-confetti-layer"), Is.Not.Null);
        Assert.That(root.Q<VisualElement>("upgrade-level-up-xp-strip"), Is.Null);
        Assert.That(root.Q<VisualElement>("damage-vignette"), Is.Not.Null);
        Assert.That(root.Q<Button>("restart-button"), Is.Not.Null);
        Assert.That(root.Q<VisualElement>("upgrade-choice-0"), Is.Not.Null);
        Assert.That(root.Q<VisualElement>("upgrade-choice-1"), Is.Not.Null);
        Assert.That(root.Q<VisualElement>("upgrade-choice-2"), Is.Not.Null);
        Assert.That(root.Q<VisualElement>("weapon-slot-row"), Is.Not.Null);
        Assert.That(root.Q<VisualElement>("weapon-slot-0"), Is.Not.Null);
        Assert.That(root.Q<VisualElement>("weapon-slot-3"), Is.Not.Null);
        Assert.That(root.Q<VisualElement>("item-stack-column"), Is.Not.Null);
        Assert.That(root.Q<VisualElement>("performer-weapon-icon-0"), Is.Not.Null);
        Assert.That(root.Q<VisualElement>("performer-weapon-icon-6"), Is.Not.Null);
        Assert.That(root.Q<VisualElement>("upgrade-icon-0"), Is.Not.Null);
        Assert.That(root.Q<VisualElement>("upgrade-icon-1"), Is.Not.Null);
        Assert.That(root.Q<VisualElement>("upgrade-icon-2"), Is.Not.Null);
        AssertUpgradeCardOrder(root.Q<VisualElement>("upgrade-choice-0"), 0);
        Assert.That(root.Q<VisualElement>("reward-reveal"), Is.Not.Null);
        Assert.That(root.Q<VisualElement>("reward-reveal-ray-layer"), Is.Not.Null);
        Assert.That(root.Q<VisualElement>("reward-reveal-icon"), Is.Not.Null);
        Assert.That(root.Q<Label>("reward-reveal-effect"), Is.Not.Null);
        Assert.That(root.Q<Button>("reward-reveal-dismiss"), Is.Not.Null);
        Assert.That(root.Q<Label>("interaction-button-glyph"), Is.Not.Null);
        Assert.That(root.Q<Label>("interaction-prompt-label")?.text, Is.EqualTo("Hold to Collect Tickets"));
    }

    [UnityTest]
    public IEnumerator HudOverlayTemplateSlotsFillScreenAndStartHidden()
    {
        VisualTreeAsset tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Game/UI/Hud.uxml");
        StyleSheet style = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Game/UI/Hud.uss");
        Assert.That(tree, Is.Not.Null);
        Assert.That(style, Is.Not.Null);

        EditorWindow window = EditorWindow.CreateInstance<EditorWindow>();
        try
        {
            window.titleContent = new GUIContent("TCO HUD Overlay Layout Test");
            window.position = new Rect(0f, 0f, 1400f, 900f);
            window.Show();

            window.rootVisualElement.style.width = 1400f;
            window.rootVisualElement.style.height = 900f;
            VisualElement root = tree.CloneTree();
            root.styleSheets.Add(style);
            root.style.width = 1400f;
            root.style.height = 900f;
            window.rootVisualElement.Add(root);

            yield return null;
            yield return null;

            AssertOverlaySlot(root, "pause-menu", shouldStartHidden: true);
            AssertOverlaySlot(root, "upgrade-selection", shouldStartHidden: true);
            AssertOverlaySlot(root, "reward-reveal", shouldStartHidden: true);

            VisualElement pauseMenu = root.Q<VisualElement>("pause-menu");
            VisualElement upgradeSelection = root.Q<VisualElement>("upgrade-selection");
            VisualElement rewardReveal = root.Q<VisualElement>("reward-reveal");
            pauseMenu.style.display = DisplayStyle.Flex;
            upgradeSelection.style.display = DisplayStyle.Flex;
            rewardReveal.style.display = DisplayStyle.Flex;

            yield return null;

            AssertOverlayFillsWindow(pauseMenu, 1400f, 900f);
            AssertOverlayFillsWindow(upgradeSelection, 1400f, 900f);
            AssertOverlayFillsWindow(rewardReveal, 1400f, 900f);
        }
        finally
        {
            if (window != null)
            {
                window.Close();
                Object.DestroyImmediate(window);
            }
        }
    }

    [Test]
    public void HudCounterIconsUseMipmappedUncompressedSpriteImports()
    {
        string[] paths =
        {
            "Assets/Game/Art/UI/Icons/HUD/HudTicketsIcon.png",
            "Assets/Game/Art/UI/Icons/HUD/HudTimerIcon.png",
            "Assets/Game/Art/UI/Icons/HUD/HudKillsIcon.png"
        };

        foreach (string path in paths)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            Assert.That(importer, Is.Not.Null, path);
            Assert.That(importer.textureType, Is.EqualTo(TextureImporterType.Sprite), path);
            Assert.That(importer.spriteImportMode, Is.EqualTo(SpriteImportMode.Single), path);
            Assert.That(importer.mipmapEnabled, Is.True, path);
            Assert.That(importer.alphaIsTransparency, Is.True, path);
            Assert.That(importer.filterMode, Is.EqualTo(FilterMode.Trilinear), path);
            Assert.That(importer.textureCompression, Is.EqualTo(TextureImporterCompression.Uncompressed), path);
        }
    }

    [UnityTest]
    public IEnumerator ContentIconFramesDeclareNonShrinkableSquaresInHudLayout()
    {
        VisualTreeAsset tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Game/UI/Hud.uxml");
        StyleSheet style = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Game/UI/Hud.uss");
        Assert.That(tree, Is.Not.Null);
        Assert.That(style, Is.Not.Null);

        Sprite portrait = CreateSprite(Color.cyan);
        Sprite weapon = CreateSprite(Color.white);
        EditorWindow window = EditorWindow.CreateInstance<EditorWindow>();
        try
        {
            window.titleContent = new GUIContent("TCO HUD Layout Test");
            window.position = new Rect(0f, 0f, 1400f, 900f);
            window.Show();

            window.rootVisualElement.style.width = 1400f;
            window.rootVisualElement.style.height = 900f;
            VisualElement root = tree.CloneTree();
            root.styleSheets.Add(style);
            root.style.width = 1400f;
            root.style.height = 900f;
            window.rootVisualElement.Add(root);

            yield return null;

            root.Q<VisualElement>("performer-selection").style.display = DisplayStyle.Flex;
            root.Q<VisualElement>("upgrade-selection").style.display = DisplayStyle.Flex;
            ContentIconVisuals.Apply(root.Q<VisualElement>("performer-portrait-0"), portrait, Color.cyan);
            ContentIconVisuals.Apply(root.Q<VisualElement>("performer-weapon-icon-0"), weapon, Color.white);
            ContentIconVisuals.Apply(root.Q<VisualElement>("upgrade-icon-0"), weapon, Color.white);
            window.Repaint();

            yield return null;
            yield return null;

            AssertSquareFrame(root.Q<VisualElement>("performer-portrait-0"), 96f);
            AssertSquareFrame(root.Q<VisualElement>("performer-weapon-icon-0"), 48f);
            AssertSquareFrame(root.Q<VisualElement>("upgrade-icon-0"), 122f);
        }
        finally
        {
            if (window != null)
            {
                window.Close();
                Object.DestroyImmediate(window);
            }

            DestroySprite(portrait);
            DestroySprite(weapon);
        }
    }

    private static void AssertUpgradeCardOrder(VisualElement card, int index)
    {
        Assert.That(card, Is.Not.Null);
        var children = card.Children().ToList();
        Label rarity = card.Q<Label>("upgrade-rarity-" + index);
        Label title = card.Q<Label>("upgrade-title-" + index);
        VisualElement icon = card.Q<VisualElement>("upgrade-icon-" + index);
        Label description = card.Q<Label>("upgrade-description-" + index);
        Label preview = card.Q<Label>("upgrade-preview-" + index);
        Label level = card.Q<Label>("upgrade-level-" + index);

        Assert.That(rarity, Is.Not.Null);
        Assert.That(title, Is.Not.Null);
        Assert.That(icon, Is.Not.Null);
        Assert.That(description, Is.Not.Null);
        Assert.That(preview, Is.Not.Null);
        Assert.That(level, Is.Not.Null);
        Assert.That(children.IndexOf(rarity), Is.LessThan(children.IndexOf(title)));
        Assert.That(children.IndexOf(title), Is.LessThan(children.IndexOf(icon)));
        Assert.That(children.IndexOf(icon), Is.LessThan(children.IndexOf(description)));
        Assert.That(children.IndexOf(description), Is.LessThan(children.IndexOf(preview)));
        Assert.That(children.IndexOf(preview), Is.LessThan(children.IndexOf(level)));
    }

    private static void AssertOverlaySlot(VisualElement root, string overlayName, bool shouldStartHidden)
    {
        VisualElement overlay = root.Q<VisualElement>(overlayName);
        Assert.That(overlay, Is.Not.Null, overlayName);
        Assert.That(overlay.parent, Is.Not.Null, overlayName);
        Assert.That(overlay.parent.ClassListContains("hud-layer-slot"), Is.True, overlayName);
        Assert.That(overlay.parent.resolvedStyle.position, Is.EqualTo(Position.Absolute), overlayName);
        if (shouldStartHidden)
        {
            Assert.That(overlay.resolvedStyle.display, Is.EqualTo(DisplayStyle.None), overlayName);
        }
    }

    private static void AssertOverlayFillsWindow(VisualElement overlay, float expectedWidth, float expectedHeight)
    {
        Assert.That(overlay, Is.Not.Null);

        Assert.That(overlay.resolvedStyle.position, Is.EqualTo(Position.Absolute), overlay.name);
        Assert.That(overlay.resolvedStyle.width, Is.EqualTo(expectedWidth).Within(1.5f), overlay.name);
        Assert.That(overlay.resolvedStyle.height, Is.EqualTo(expectedHeight).Within(1.5f), overlay.name);
    }

    private static void AssertSquareFrame(VisualElement frame, float expectedSize)
    {
        Assert.That(frame, Is.Not.Null);
        if (float.IsNaN(frame.resolvedStyle.width) || float.IsNaN(frame.resolvedStyle.height))
        {
            Assert.That(frame.resolvedStyle.minWidth.value, Is.EqualTo(expectedSize).Within(0.01f));
            Assert.That(frame.resolvedStyle.maxWidth.value, Is.EqualTo(expectedSize).Within(0.01f));
            Assert.That(frame.resolvedStyle.minHeight.value, Is.EqualTo(expectedSize).Within(0.01f));
            Assert.That(frame.resolvedStyle.maxHeight.value, Is.EqualTo(expectedSize).Within(0.01f));
        }
        else
        {
            Assert.That(frame.resolvedStyle.width, Is.EqualTo(expectedSize).Within(1.5f));
            Assert.That(frame.resolvedStyle.height, Is.EqualTo(expectedSize).Within(1.5f));
        }

        Assert.That(frame.resolvedStyle.flexShrink, Is.EqualTo(0f).Within(0.001f));
        Image image = frame.Q<Image>(ContentIconVisuals.ImageElementName);
        Assert.That(image, Is.Not.Null);
        Assert.That(image.resolvedStyle.position, Is.EqualTo(Position.Absolute));
        Assert.That(image.resolvedStyle.flexShrink, Is.EqualTo(0f).Within(0.001f));
        if (!float.IsNaN(image.resolvedStyle.width) && !float.IsNaN(image.resolvedStyle.height))
        {
            Assert.That(image.resolvedStyle.width, Is.GreaterThanOrEqualTo(expectedSize - 8f));
            Assert.That(image.resolvedStyle.height, Is.GreaterThanOrEqualTo(expectedSize - 8f));
        }
    }

    private static Color IconFrameBlack()
    {
        return new Color(0.015f, 0.018f, 0.024f, 0.98f);
    }

    [Test]
    public void SetExperienceUpdatesFillWithoutWritingXpNumbers()
    {
        HudView hud = CreateBoundHud(out _, out VisualElement xpFill, out _, out Label xpLabel, out _, out _, out _);
        xpLabel.text = "hidden";

        hud.SetExperience(6, 12);

        Assert.That(xpFill.style.width.value.unit, Is.EqualTo(LengthUnit.Percent));
        Assert.That(xpFill.style.width.value.value, Is.EqualTo(50f).Within(0.01f));
        Assert.That(xpLabel.text, Is.EqualTo("hidden"));
    }

    [Test]
    public void SetLevelUpdatesEmbeddedLevelLabel()
    {
        HudView hud = CreateBoundHud(out _, out _, out _, out _, out Label levelLabel, out _, out _);

        hud.SetLevel(7);

        Assert.That(levelLabel.text, Is.EqualTo("7"));
    }

    [Test]
    public void SetHealthFormatsCompactCurrentAndMax()
    {
        HudView hud = CreateBoundHud(out VisualElement healthFill, out _, out Label healthLabel, out _, out _, out _, out _);

        hud.SetHealth(40, 100);

        Assert.That(healthFill.style.width.value.unit, Is.EqualTo(LengthUnit.Percent));
        Assert.That(healthFill.style.width.value.value, Is.EqualTo(40f).Within(0.01f));
        Assert.That(healthLabel.text, Is.EqualTo("40 / 100"));
    }

    [Test]
    public void SetKillsKeepsTextIconSeparateFromNumber()
    {
        HudView hud = CreateBoundHud(out _, out _, out _, out _, out _, out Label killsLabel, out _);

        hud.SetKills(23);

        Assert.That(killsLabel.text, Is.EqualTo("23"));
    }

    [Test]
    public void SetTicketsClampsToNonNegativeText()
    {
        HudView hud = CreateBoundHud(out _, out _, out _, out _, out _, out _, out _);
        var ticketsLabel = new Label();
        SetField(hud, "ticketsLabel", ticketsLabel);

        hud.SetTickets(-3);

        Assert.That(ticketsLabel.text, Is.EqualTo("0"));
    }

    [Test]
    public void SetWeaponSlotsShowsFilledWeaponsAndLevels()
    {
        HudView hud = CreateBoundHud(out _, out _, out _, out _, out _, out _, out _);
        var slots = new[] { new VisualElement(), new VisualElement(), new VisualElement(), new VisualElement() };
        var labels = new[] { new Label(), new Label(), new Label(), new Label() };
        SetField(hud, "weaponSlots", slots);
        SetField(hud, "weaponSlotLevelLabels", labels);
        WeaponDefinition weapon = TheCircussyOneTestObjects.CreateWeaponDefinition(null, "knife_fan", "Knife Fan");
        Sprite weaponIcon = CreateSprite(Color.white);
        weapon.iconSprite = weaponIcon;
        var runtime = new WeaponRuntime(weapon, 3);

        hud.SetWeaponSlots(new[] { runtime }, 4);

        Assert.That(slots[0].ClassListContains("weapon-slot-filled"), Is.True);
        Assert.That(slots[0].style.backgroundColor.value, Is.EqualTo(IconFrameBlack()));
        Assert.That(slots[0].style.borderTopColor.value, Is.EqualTo(weapon.projectilePrimaryColor));
        Image image = slots[0].Q<Image>(ContentIconVisuals.ImageElementName);
        Assert.That(image, Is.Not.Null);
        Assert.That(slots[0].IndexOf(image), Is.EqualTo(0));
        Assert.That(image.sprite, Is.SameAs(weaponIcon));
        Assert.That(labels[0].text, Is.EqualTo("3"));
        Assert.That(labels[0].style.display.value, Is.EqualTo(DisplayStyle.Flex));
        Assert.That(slots[1].ClassListContains("weapon-slot-filled"), Is.False);
        Assert.That(labels[1].text, Is.EqualTo(string.Empty));
        Assert.That(labels[1].style.display.value, Is.EqualTo(DisplayStyle.None));

        Object.DestroyImmediate(weapon);
        DestroySprite(weaponIcon);
    }

    [Test]
    public void SetItemStacksCreatesVerticalIconSlotsWithStackCounts()
    {
        HudView hud = CreateBoundHud(out _, out _, out _, out _, out _, out _, out _);
        var column = new VisualElement();
        SetField(hud, "itemStackColumn", column);
        ItemDefinition item = TheCircussyOneTestObjects.CreateItemDefinition("rubber_soles", "Rubber Soles", maxStacks: 5);
        Sprite itemIcon = CreateSprite(Color.white);
        item.iconSprite = itemIcon;

        hud.SetItemStacks(new[] { new ItemStackRuntime(item, 3) });

        Assert.That(column.childCount, Is.EqualTo(1));
        Assert.That(column[0].ClassListContains("item-stack-slot"), Is.True);
        Assert.That(column[0].style.backgroundColor.value, Is.EqualTo(IconFrameBlack()));
        Assert.That(column[0].style.borderTopColor.value, Is.EqualTo(item.iconColor));
        Image image = column[0].Q<Image>(ContentIconVisuals.ImageElementName);
        Assert.That(image, Is.Not.Null);
        Assert.That(column[0].IndexOf(image), Is.EqualTo(0));
        Assert.That(image.sprite, Is.SameAs(itemIcon));
        Assert.That(column[0].Q<Label>(className: "item-stack-count")?.text, Is.EqualTo("3"));

        Object.DestroyImmediate(item);
        DestroySprite(itemIcon);
    }

    [Test]
    public void TimerFormatsSecondsAsMinutesAndSeconds()
    {
        HudView hud = CreateBoundHud(out _, out _, out _, out _, out _, out _, out Label timerLabel);

        hud.SetRunTime(125.8f);

        Assert.That(timerLabel.text, Is.EqualTo("02:05"));
        Assert.That(HudView.FormatRunTime(-3f), Is.EqualTo("00:00"));
    }

    [Test]
    public void ApplyPreviewUpdatesHudValuesWithoutPlayMode()
    {
        HudView hud = CreateBoundHudWithGameOver(
            out VisualElement healthFill,
            out VisualElement xpFill,
            out Label healthLabel,
            out _,
            out Label levelLabel,
            out Label killsLabel,
            out Label timerLabel,
            out VisualElement gameOver);

        hud.ApplyPreview(new HudPreviewSnapshot(25, 100, 7, 10, 4, 33, 185f, true));

        Assert.That(healthFill.style.width.value.value, Is.EqualTo(25f).Within(0.01f));
        Assert.That(xpFill.style.width.value.value, Is.EqualTo(70f).Within(0.01f));
        Assert.That(healthLabel.text, Is.EqualTo("25 / 100"));
        Assert.That(levelLabel.text, Is.EqualTo("4"));
        Assert.That(killsLabel.text, Is.EqualTo("33"));
        Assert.That(timerLabel.text, Is.EqualTo("03:05"));
        Assert.That(gameOver.style.display.value, Is.EqualTo(DisplayStyle.Flex));
    }

    [Test]
    public void ClearPreviewRestoresNeutralHudValues()
    {
        HudView hud = CreateBoundHudWithGameOver(
            out VisualElement healthFill,
            out VisualElement xpFill,
            out Label healthLabel,
            out _,
            out Label levelLabel,
            out Label killsLabel,
            out Label timerLabel,
            out VisualElement gameOver);

        hud.ApplyPreview(new HudPreviewSnapshot(0, 100, 9, 10, 5, 91, 612f, true));
        hud.ClearPreview();

        Assert.That(healthFill.style.width.value.value, Is.EqualTo(100f).Within(0.01f));
        Assert.That(xpFill.style.width.value.value, Is.EqualTo(0f).Within(0.01f));
        Assert.That(healthLabel.text, Is.EqualTo("100 / 100"));
        Assert.That(levelLabel.text, Is.EqualTo("1"));
        Assert.That(killsLabel.text, Is.EqualTo("0"));
        Assert.That(timerLabel.text, Is.EqualTo("00:00"));
        Assert.That(gameOver.style.display.value, Is.EqualTo(DisplayStyle.None));
    }

    [Test]
    public void RestartRequestRaisesHudEvent()
    {
        HudView hud = CreateBoundHud(out _, out _, out _, out _, out _, out _, out _);
        int requested = 0;
        hud.RestartRequested += () => requested++;

        hud.RequestRestart();

        Assert.That(requested, Is.EqualTo(1));
    }

    [Test]
    public void InteractionPromptUsesKeyboardGlyphBeforeHold()
    {
        HudView hud = CreateInteractionPromptHud(out VisualElement prompt, out Label glyph, out Label label, out VisualElement fill);

        hud.SetInteractionPrompt("Collect Tickets", 0.25f, isHolding: false);

        Assert.That(prompt.style.display.value, Is.EqualTo(DisplayStyle.Flex));
        Assert.That(glyph.style.display.value, Is.EqualTo(DisplayStyle.Flex));
        Assert.That(glyph.text, Is.EqualTo("E"));
        Assert.That(glyph.ClassListContains("interaction-button-glyph--keyboard"), Is.True);
        Assert.That(glyph.ClassListContains("interaction-button-glyph--gamepad"), Is.False);
        Assert.That(label.text, Is.EqualTo("Hold to Collect Tickets"));
        Assert.That(fill.style.width.value.value, Is.EqualTo(25f).Within(0.01f));
    }

    [Test]
    public void InteractionPromptUsesBlueXGlyphForGamepadBeforeHold()
    {
        HudView hud = CreateInteractionPromptHud(out _, out Label glyph, out Label label, out _);

        hud.SetInteractionPrompt("Collect Tickets", 0f, isHolding: false, useGamepadGlyph: true);

        Assert.That(glyph.style.display.value, Is.EqualTo(DisplayStyle.Flex));
        Assert.That(glyph.text, Is.EqualTo("X"));
        Assert.That(glyph.ClassListContains("interaction-button-glyph--gamepad"), Is.True);
        Assert.That(glyph.ClassListContains("interaction-button-glyph--keyboard"), Is.False);
        Assert.That(label.text, Is.EqualTo("Hold to Collect Tickets"));
    }

    [Test]
    public void InteractionPromptShowsCollectingCopyWhileHeld()
    {
        HudView hud = CreateInteractionPromptHud(out _, out Label glyph, out Label label, out VisualElement fill);

        hud.SetInteractionPrompt("Collect Tickets", 0.5f, isHolding: true, useGamepadGlyph: true);

        Assert.That(glyph.style.display.value, Is.EqualTo(DisplayStyle.None));
        Assert.That(label.text, Is.EqualTo("Collecting Tickets..."));
        Assert.That(fill.style.width.value.value, Is.EqualTo(50f).Within(0.01f));
    }

    [Test]
    public void ApplyConfigScalesHudDimensionsWhileKeepingXpWidthRelative()
    {
        HudView hud = CreateConfigBoundHud(
            out VisualElement xpStrip,
            out VisualElement xpTrack,
            out VisualElement hudUnderRow,
            out VisualElement healthCluster,
            out VisualElement killsIcon,
            out Label timerLabel);
        var config = ScriptableObject.CreateInstance<HudVisualConfig>();
        config.globalScale = 1.5f;
        config.xpStripHeight = 20f;
        config.xpSideMargin = 10f;
        config.xpTopPadding = 2f;
        config.xpTrackHeight = 12f;
        config.healthWidth = 180f;
        config.healthHeight = 18f;
        config.healthOffset = new Vector2(14f, 30f);
        config.healthTextVerticalOffset = -1f;
        config.killsLabelText = "KILLS";
        config.killsIconWidth = 17f;
        config.killsIconHeight = 17f;
        config.killsIconYOffset = -2f;
        config.ticketsIconWidth = 30f;
        config.ticketsIconHeight = 18f;
        config.ticketsIconYOffset = -1f;
        config.timerIconWidth = 20f;
        config.timerIconHeight = 20f;
        config.timerIconSpacing = 5f;
        config.timerIconYOffset = -3f;
        config.timerTopOffset = 24f;
        config.timerFontSize = 13f;

        hud.ApplyConfig(config);

        VisualElement ticketsCluster = GetField<VisualElement>(hud, "ticketsCluster");
        VisualElement ticketsIcon = GetField<VisualElement>(hud, "ticketsIcon");
        VisualElement timerCluster = GetField<VisualElement>(hud, "timerCluster");
        VisualElement timerIcon = GetField<VisualElement>(hud, "timerIcon");
        Label ticketsValue = GetField<Label>(hud, "ticketsLabel");

        Assert.That(xpStrip.style.height.value.value, Is.EqualTo(30f).Within(0.01f));
        Assert.That(xpTrack.style.left.value.value, Is.EqualTo(15f).Within(0.01f));
        Assert.That(xpTrack.style.right.value.value, Is.EqualTo(15f).Within(0.01f));
        Assert.That(xpTrack.style.height.value.value, Is.EqualTo(18f).Within(0.01f));
        Assert.That(xpTrack.style.width.keyword, Is.EqualTo(StyleKeyword.Null));
        Assert.That(hudUnderRow.style.top.value.value, Is.EqualTo(0f).Within(0.01f));
        Assert.That(healthCluster.style.left.value.value, Is.EqualTo(66f).Within(0.01f));
        Assert.That(healthCluster.style.top.value.value, Is.EqualTo(49.5f).Within(0.01f));
        Assert.That(healthCluster.style.width.value.value, Is.EqualTo(270f).Within(0.01f));
        Assert.That(GetField<Label>(hud, "levelLabel").style.left.value.value, Is.EqualTo(21f).Within(0.01f));
        Assert.That(GetField<Label>(hud, "levelLabel").style.top.value.value, Is.EqualTo(45f).Within(0.01f));
        Assert.That(GetField<Label>(hud, "levelLabel").style.width.value.value, Is.EqualTo(36f).Within(0.01f));
        Assert.That(GetField<Label>(hud, "levelLabel").style.height.value.value, Is.EqualTo(36f).Within(0.01f));
        Assert.That(GetField<Label>(hud, "healthLabel").style.top.value.value, Is.EqualTo(-1.5f).Within(0.01f));
        Assert.That(GetField<Label>(hud, "healthLabel").style.bottom.value.value, Is.EqualTo(1.5f).Within(0.01f));
        Assert.That(GetField<Label>(hud, "healthLabel").style.unityTextAlign.value, Is.EqualTo(TextAnchor.MiddleCenter));
        Assert.That(ticketsCluster.style.left.value.value, Is.EqualTo(357f).Within(0.01f));
        Assert.That(ticketsCluster.style.top.value.value, Is.EqualTo(45f).Within(0.01f));
        Assert.That(GetField<Label>(hud, "levelLabel").style.top.value.value + GetField<Label>(hud, "levelLabel").style.height.value.value * 0.5f, Is.EqualTo(63f).Within(0.01f));
        Assert.That(healthCluster.style.top.value.value + healthCluster.style.height.value.value * 0.5f, Is.EqualTo(63f).Within(0.01f));
        Assert.That(ticketsCluster.style.top.value.value + ticketsCluster.style.height.value.value * 0.5f, Is.EqualTo(63f).Within(0.01f));
        Assert.That(ticketsIcon.style.width.value.value, Is.EqualTo(45f).Within(0.01f));
        Assert.That(ticketsIcon.style.top.value.value, Is.EqualTo(-1.5f).Within(0.01f));
        Assert.That(ticketsValue.style.minWidth.value.value, Is.EqualTo(63f).Within(0.01f));
        Assert.That(killsIcon.style.width.value.value, Is.EqualTo(25.5f).Within(0.01f));
        Assert.That(killsIcon.style.top.value.value, Is.EqualTo(-3f).Within(0.01f));
        Assert.That(timerCluster.style.top.value.value, Is.EqualTo(36f).Within(0.01f));
        Assert.That(timerIcon.style.width.value.value, Is.EqualTo(30f).Within(0.01f));
        Assert.That(timerIcon.style.marginRight.value.value, Is.EqualTo(7.5f).Within(0.01f));
        Assert.That(timerIcon.style.top.value.value, Is.EqualTo(-4.5f).Within(0.01f));
        Assert.That(timerLabel.style.fontSize.value.value, Is.EqualTo(19.5f).Within(0.01f));

        Object.DestroyImmediate(config);
    }

    [Test]
    public void ApplyConfigUsesEquipmentLayoutSettings()
    {
        HudView hud = CreateConfigBoundHud(
            out _,
            out _,
            out _,
            out _,
            out _,
            out _);
        var weaponSlotRow = new VisualElement();
        var weaponSlots = new[] { new VisualElement(), new VisualElement(), new VisualElement(), new VisualElement() };
        var weaponLevels = new[] { new Label(), new Label(), new Label(), new Label() };
        var itemColumn = new VisualElement();
        SetField(hud, "weaponSlotRow", weaponSlotRow);
        SetField(hud, "weaponSlots", weaponSlots);
        SetField(hud, "weaponSlotLevelLabels", weaponLevels);
        SetField(hud, "itemStackColumn", itemColumn);

        var config = ScriptableObject.CreateInstance<HudVisualConfig>();
        config.globalScale = 2f;
        config.healthOffset = new Vector2(10f, 20f);
        config.levelBadgeWidth = 20f;
        config.levelBadgeHeight = 14f;
        config.levelBadgeOffset = new Vector2(4f, 2f);
        config.healthHeight = 10f;
        config.ticketsHeight = 16f;
        config.weaponSlotsOffsetFromLevelBadge = new Vector2(3f, 11f);
        config.weaponSlotRowHeight = 40f;
        config.weaponSlotSize = 22f;
        config.weaponSlotGap = 6f;
        config.weaponSlotLevelOffset = new Vector2(2f, -1f);
        config.weaponSlotLevelMinWidth = 17f;
        config.weaponSlotLevelHeight = 9f;
        config.weaponSlotLevelFontSize = 7f;
        config.killsOffset = new Vector2(7f, 9f);
        config.killsHeight = 15f;
        config.itemStackOffsetFromKills = new Vector2(2f, 4f);
        config.itemStackColumnWidth = 40f;
        config.itemStackSlotSize = 24f;
        config.itemStackGap = 8f;
        config.itemStackCountOffset = new Vector2(3f, -2f);
        config.itemStackCountMinWidth = 18f;
        config.itemStackCountHeight = 10f;
        config.itemStackCountFontSize = 6f;
        hud.ApplyConfig(config);

        Assert.That(weaponSlotRow.style.left.value.value, Is.EqualTo(26f).Within(0.01f));
        Assert.That(weaponSlotRow.style.top.value.value, Is.EqualTo(90f).Within(0.01f));
        Assert.That(weaponSlotRow.style.height.value.value, Is.EqualTo(80f).Within(0.01f));
        Assert.That(weaponSlots[0].style.width.value.value, Is.EqualTo(44f).Within(0.01f));
        Assert.That(weaponSlots[0].style.height.value.value, Is.EqualTo(44f).Within(0.01f));
        Assert.That(weaponSlots[0].style.marginRight.value.value, Is.EqualTo(12f).Within(0.01f));
        Assert.That(weaponLevels[0].style.left.value.unit, Is.EqualTo(LengthUnit.Percent));
        Assert.That(weaponLevels[0].style.left.value.value, Is.EqualTo(50f).Within(0.01f));
        Assert.That(weaponLevels[0].style.top.value.value, Is.EqualTo(42f).Within(0.01f));
        Assert.That(weaponLevels[0].style.marginLeft.value.value, Is.EqualTo(-13f).Within(0.01f));
        Assert.That(weaponLevels[0].style.width.value.value, Is.EqualTo(34f).Within(0.01f));
        Assert.That(weaponLevels[0].style.minWidth.value.value, Is.EqualTo(34f).Within(0.01f));
        Assert.That(weaponLevels[0].style.height.value.value, Is.EqualTo(18f).Within(0.01f));
        Assert.That(weaponLevels[0].style.fontSize.value.value, Is.EqualTo(14f).Within(0.01f));
        Assert.That(itemColumn.style.right.value.value, Is.EqualTo(18f).Within(0.01f));
        Assert.That(itemColumn.style.top.value.value, Is.EqualTo(56f).Within(0.01f));
        Assert.That(itemColumn.style.width.value.value, Is.EqualTo(80f).Within(0.01f));

        ItemDefinition item = TheCircussyOneTestObjects.CreateItemDefinition("test_ticket_charm", "Test Ticket Charm", maxStacks: 9);
        hud.SetItemStacks(new[] { new ItemStackRuntime(item, 4) });

        VisualElement itemSlot = itemColumn[0];
        Label itemCount = itemSlot.Q<Label>(className: "item-stack-count");
        Assert.That(itemSlot.style.width.value.value, Is.EqualTo(48f).Within(0.01f));
        Assert.That(itemSlot.style.height.value.value, Is.EqualTo(48f).Within(0.01f));
        Assert.That(itemSlot.style.marginBottom.value.value, Is.EqualTo(16f).Within(0.01f));
        Assert.That(itemCount.style.right.value.value, Is.EqualTo(6f).Within(0.01f));
        Assert.That(itemCount.style.bottom.value.value, Is.EqualTo(-4f).Within(0.01f));
        Assert.That(itemCount.style.minWidth.value.value, Is.EqualTo(36f).Within(0.01f));
        Assert.That(itemCount.style.height.value.value, Is.EqualTo(20f).Within(0.01f));
        Assert.That(itemCount.style.fontSize.value.value, Is.EqualTo(12f).Within(0.01f));

        Object.DestroyImmediate(item);
        Object.DestroyImmediate(config);
    }

    [Test]
    public void ApplyConfigUsesReferenceSurfaceForScaleWithScreenSizeEditPreview()
    {
        HudView hud = CreateConfigBoundHud(
            out _,
            out _,
            out _,
            out _,
            out _,
            out _);
        var config = ScriptableObject.CreateInstance<HudVisualConfig>();
        config.scaleMode = PanelScaleMode.ScaleWithScreenSize;
        config.referenceResolution = new Vector2(1920f, 1080f);

        hud.ApplyConfig(config);

        VisualElement root = GetField<VisualElement>(hud, "root");
        Assert.That(root.style.width.value.value, Is.EqualTo(1920f).Within(0.01f));
        Assert.That(root.style.height.value.value, Is.EqualTo(1080f).Within(0.01f));

        Object.DestroyImmediate(config);
    }

    [Test]
    public void CounterPopScalesKillAndTicketClustersThenReturnsToNormal()
    {
        HudView hud = CreateConfigBoundHud(
            out _,
            out _,
            out _,
            out _,
            out _,
            out _);
        VisualElement killsCluster = GetField<VisualElement>(hud, "killsCluster");
        VisualElement ticketsCluster = GetField<VisualElement>(hud, "ticketsCluster");
        var config = ScriptableObject.CreateInstance<HudVisualConfig>();
        config.killsCounterPop = new HudCounterPopSettings(true, 0.2f, 1.25f, EaseSettings.Linear);
        config.ticketsCounterPop = new HudCounterPopSettings(true, 0.2f, 1.15f, EaseSettings.Linear);

        hud.ApplyConfig(config);
        hud.PlayKillsPop();
        hud.PlayTicketsPop();

        Assert.That(ScaleX(killsCluster), Is.EqualTo(1.25f).Within(0.001f));
        Assert.That(ScaleX(ticketsCluster), Is.EqualTo(1.15f).Within(0.001f));

        hud.TickCounterPops(0.1f);
        Assert.That(ScaleX(killsCluster), Is.EqualTo(1.125f).Within(0.001f));
        Assert.That(ScaleX(ticketsCluster), Is.EqualTo(1.075f).Within(0.001f));

        hud.TickCounterPops(0.1f);
        Assert.That(ScaleX(killsCluster), Is.EqualTo(1f).Within(0.001f));
        Assert.That(ScaleX(ticketsCluster), Is.EqualTo(1f).Within(0.001f));

        Object.DestroyImmediate(config);
    }

    [Test]
    public void ExperienceTweenAnimatesFillWhenConfigIsApplied()
    {
        HudView hud = CreateConfigBoundHud(
            out _,
            out _,
            out _,
            out _,
            out _,
            out _);
        VisualElement xpFill = GetField<VisualElement>(hud, "experienceFill");
        var config = ScriptableObject.CreateInstance<HudVisualConfig>();
        config.xpBarTween = new HudBarTweenSettings(true, 0.2f, EaseSettings.Linear);
        hud.ApplyConfig(config);

        hud.SetExperience(0, 10);
        hud.SetExperience(10, 10);
        Assert.That(xpFill.style.width.value.value, Is.EqualTo(0f).Within(0.01f));

        hud.TickHudFeedback(0.1f);
        Assert.That(xpFill.style.width.value.value, Is.EqualTo(50f).Within(0.01f));

        hud.TickHudFeedback(0.1f);
        Assert.That(xpFill.style.width.value.value, Is.EqualTo(100f).Within(0.01f));

        Object.DestroyImmediate(config);
    }

    [Test]
    public void LevelUpHoldKeepsXpFullPulsingUntilReleased()
    {
        HudView hud = CreateConfigBoundHud(
            out _,
            out _,
            out _,
            out _,
            out _,
            out _);
        VisualElement xpFill = GetField<VisualElement>(hud, "experienceFill");
        VisualElement xpPulse = GetField<VisualElement>(hud, "experiencePulse");
        VisualElement xpStrip = GetField<VisualElement>(hud, "xpStrip");
        var originalXpParent = new VisualElement();
        originalXpParent.Add(xpStrip);
        var config = ScriptableObject.CreateInstance<HudVisualConfig>();
        config.xpBarTween = new HudBarTweenSettings(true, 0.2f, EaseSettings.Linear);
        config.xpLevelUpPulseSpeed = 0f;
        config.xpLevelUpPulseMaxOpacity = 0.5f;
        hud.ApplyConfig(config);

        hud.SetExperience(8, 10);
        hud.BeginXpLevelUpHold();
        VisualElement hudRoot = GetField<VisualElement>(hud, "hudRoot");
        VisualElement root = GetField<VisualElement>(hud, "root");
        VisualElement promotedParent = hudRoot ?? root;
        Assert.That(xpStrip.parent, Is.EqualTo(promotedParent));
        Assert.That(xpStrip.ClassListContains("xp-strip-level-up-overlay"), Is.True);

        hud.SetExperience(2, 12);
        hud.TickHudFeedback(0.2f);

        Assert.That(xpFill.style.width.value.value, Is.EqualTo(100f).Within(0.01f));
        Assert.That(xpPulse.style.opacity.value, Is.EqualTo(0.5f).Within(0.01f));

        hud.EndXpLevelUpHold();
        Assert.That(xpStrip.parent, Is.EqualTo(originalXpParent));
        Assert.That(xpStrip.ClassListContains("xp-strip-level-up-overlay"), Is.False);
        Assert.That(xpFill.style.width.value.value, Is.EqualTo(0f).Within(0.01f));
        Assert.That(xpPulse.style.opacity.value, Is.EqualTo(0f).Within(0.01f));

        hud.TickHudFeedback(0.2f);
        Assert.That(xpFill.style.width.value.value, Is.EqualTo(16.666f).Within(0.05f));

        Object.DestroyImmediate(config);
    }

    [Test]
    public void HealthTweenAnimatesDamageAndHealingWhenConfigIsApplied()
    {
        HudView hud = CreateConfigBoundHud(
            out _,
            out _,
            out _,
            out _,
            out _,
            out _);
        VisualElement healthFill = GetField<VisualElement>(hud, "healthFill");
        var config = ScriptableObject.CreateInstance<HudVisualConfig>();
        config.healthBarTween = new HudBarTweenSettings(true, 0.2f, EaseSettings.Linear);
        hud.ApplyConfig(config);

        hud.SetHealth(100, 100);
        hud.SetHealth(40, 100);
        Assert.That(healthFill.style.width.value.value, Is.EqualTo(100f).Within(0.01f));

        hud.TickHudFeedback(0.1f);
        Assert.That(healthFill.style.width.value.value, Is.EqualTo(70f).Within(0.01f));

        hud.TickHudFeedback(0.1f);
        Assert.That(healthFill.style.width.value.value, Is.EqualTo(40f).Within(0.01f));

        hud.SetHealth(80, 100);
        hud.TickHudFeedback(0.2f);
        Assert.That(healthFill.style.width.value.value, Is.EqualTo(80f).Within(0.01f));

        Object.DestroyImmediate(config);
    }

    [Test]
    public void ApplyConfigBuildsXpSegmentDividers()
    {
        HudView hud = CreateConfigBoundHud(
            out _,
            out _,
            out _,
            out _,
            out _,
            out _);
        VisualElement xpSegments = GetField<VisualElement>(hud, "xpSegments");
        var config = ScriptableObject.CreateInstance<HudVisualConfig>();
        config.xpSegmentCount = 10;
        config.xpSegmentWidth = 2f;
        hud.ApplyConfig(config);

        Assert.That(xpSegments.childCount, Is.EqualTo(9));
        Assert.That(xpSegments[0].style.left.value.unit, Is.EqualTo(LengthUnit.Percent));
        Assert.That(xpSegments[0].style.left.value.value, Is.EqualTo(10f).Within(0.01f));
        Assert.That(xpSegments[0].style.width.value.value, Is.EqualTo(2f).Within(0.01f));

        Object.DestroyImmediate(config);
    }

    [Test]
    public void HealthSegmentsUseAbsoluteHpThresholds()
    {
        HudView hud = CreateConfigBoundHud(
            out _,
            out _,
            out _,
            out _,
            out _,
            out _);
        VisualElement healthSegments = GetField<VisualElement>(hud, "healthSegments");
        var config = ScriptableObject.CreateInstance<HudVisualConfig>();
        config.healthWidth = 200f;
        config.healthHpPerSegment = 25f;
        config.healthSegmentWidth = 2f;
        hud.ApplyConfig(config);

        hud.SetHealth(100, 100);
        Assert.That(healthSegments.childCount, Is.EqualTo(3));
        Assert.That(healthSegments[0].style.left.value.value, Is.EqualTo(25f).Within(0.01f));
        Assert.That(healthSegments[1].style.left.value.value, Is.EqualTo(50f).Within(0.01f));
        Assert.That(healthSegments[2].style.left.value.value, Is.EqualTo(75f).Within(0.01f));
        Assert.That(healthSegments[0].style.width.value.value, Is.EqualTo(2f).Within(0.01f));

        Object.DestroyImmediate(config);
    }

    [Test]
    public void HealthSegmentsLeavePartialFinalSegmentForNonMultipleMaxHp()
    {
        HudView hud = CreateConfigBoundHud(
            out _,
            out _,
            out _,
            out _,
            out _,
            out _);
        VisualElement healthSegments = GetField<VisualElement>(hud, "healthSegments");
        var config = ScriptableObject.CreateInstance<HudVisualConfig>();
        config.healthWidth = 224f;
        config.healthHpPerSegment = 25f;
        hud.ApplyConfig(config);

        hud.SetHealth(112, 112);

        Assert.That(healthSegments.childCount, Is.EqualTo(4));
        Assert.That(healthSegments[3].style.left.value.value, Is.EqualTo(100f / 112f * 100f).Within(0.01f));

        Object.DestroyImmediate(config);
    }

    [Test]
    public void HealthSegmentsHideWhenTooDense()
    {
        HudView hud = CreateConfigBoundHud(
            out _,
            out _,
            out _,
            out _,
            out _,
            out _);
        VisualElement healthSegments = GetField<VisualElement>(hud, "healthSegments");
        var config = ScriptableObject.CreateInstance<HudVisualConfig>();
        config.healthWidth = 100f;
        config.healthHpPerSegment = 25f;
        config.healthMinimumSegmentPixelSpacing = 10f;
        hud.ApplyConfig(config);

        hud.SetHealth(1000, 1000);

        Assert.That(healthSegments.childCount, Is.EqualTo(0));
        Assert.That(healthSegments.style.display.value, Is.EqualTo(DisplayStyle.None));

        Object.DestroyImmediate(config);
    }

    [Test]
    public void HudPresenterPopsCountersOnlyWhenKillsOrTicketsIncrease()
    {
        HudView hud = CreateConfigBoundHud(
            out _,
            out _,
            out _,
            out _,
            out _,
            out _);
        VisualElement killsCluster = GetField<VisualElement>(hud, "killsCluster");
        VisualElement ticketsCluster = GetField<VisualElement>(hud, "ticketsCluster");
        var hudConfig = ScriptableObject.CreateInstance<HudVisualConfig>();
        hudConfig.killsCounterPop = new HudCounterPopSettings(true, 0.2f, 1.25f, EaseSettings.Linear);
        hudConfig.ticketsCounterPop = new HudCounterPopSettings(true, 0.2f, 1.15f, EaseSettings.Linear);
        hud.ApplyConfig(hudConfig);

        GameConfig gameConfig = TheCircussyOneTestObjects.CreateConfig();
        var state = new GameState(gameConfig);
        var currency = new RunCurrencyState();
        var time = new FakeGameTime { Time = 0f, DeltaTime = 0.2f };
        var presenter = new HudPresenter(state, hud, time, new FakeRunRestarter(), currency);

        presenter.Start();
        Assert.That(ScaleX(killsCluster), Is.EqualTo(1f).Within(0.001f));
        Assert.That(ScaleX(ticketsCluster), Is.EqualTo(1f).Within(0.001f));

        state.AddKill();
        currency.AddTickets(5);
        Assert.That(ScaleX(killsCluster), Is.GreaterThan(1f));
        Assert.That(ScaleX(ticketsCluster), Is.GreaterThan(1f));

        presenter.Tick();
        Assert.That(ScaleX(killsCluster), Is.EqualTo(1f).Within(0.001f));
        Assert.That(ScaleX(ticketsCluster), Is.EqualTo(1f).Within(0.001f));

        state.Reset();
        currency.TrySpendTickets(1);
        Assert.That(ScaleX(killsCluster), Is.EqualTo(1f).Within(0.001f));
        Assert.That(ScaleX(ticketsCluster), Is.EqualTo(1f).Within(0.001f));

        presenter.Dispose();
        Object.DestroyImmediate(hudConfig);
        Object.DestroyImmediate(gameConfig);
    }

    [Test]
    public void HudPresenterKeepsFinalCurtainVisibleWhenDeathFailsRun()
    {
        HudView hud = CreateConfigBoundHud(
            out _,
            out _,
            out _,
            out _,
            out _,
            out _);
        VisualElement gameOver = GetField<VisualElement>(hud, "gameOver");
        Label gameOverTitle = GetField<Label>(hud, "gameOverTitleLabel");
        Label gameOverSubtitle = GetField<Label>(hud, "gameOverSubtitleLabel");
        GameConfig gameConfig = TheCircussyOneTestObjects.CreateConfig();
        var state = new GameState(gameConfig);
        var pauseState = new RunPauseState();
        var phase = new RunPhaseState();
        phase.ResetToPerformerSelection();
        phase.BeginWorld();
        var presenter = new HudPresenter(state, hud, new FakeGameTime(), new FakeRunRestarter(), runPhase: phase);
        var gameOverPause = new GameOverPauseSystem(state, pauseState, phase);

        presenter.Start();
        gameOverPause.Start();
        state.DamagePlayer(gameConfig.playerMaxHealth);

        Assert.That(phase.CurrentPhase, Is.EqualTo(RunPhase.Failed));
        Assert.That(pauseState.HasReason(RunPauseReasons.GameOver), Is.True);
        Assert.That(hud.IsTerminalOverlayVisible, Is.True);
        Assert.That(gameOver.style.display.value, Is.EqualTo(DisplayStyle.Flex));
        Assert.That(gameOverTitle.text, Is.EqualTo("FINAL CURTAIN"));
        Assert.That(gameOverSubtitle.text, Is.EqualTo("You were overwhelmed."));

        gameOverPause.Dispose();
        presenter.Dispose();
        Object.DestroyImmediate(gameConfig);
    }

    [Test]
    public void HudPresenterPlaysAnnouncementAudioWhenBannerAppearsAndFades()
    {
        HudView hud = CreateConfigBoundHud(
            out _,
            out _,
            out _,
            out _,
            out _,
            out _);
        var banner = new VisualElement();
        var kicker = new Label();
        var title = new Label();
        SetField(hud, "actBanner", banner);
        SetField(hud, "actBannerKickerLabel", kicker);
        SetField(hud, "actBannerTitleLabel", title);

        var hudConfig = ScriptableObject.CreateInstance<HudVisualConfig>();
        hudConfig.actBannerEnabled = true;
        hudConfig.actBannerDelaySeconds = 0.5f;
        hudConfig.actBannerPopSeconds = 0.1f;
        hudConfig.actBannerHoldSeconds = 0.2f;
        hudConfig.actBannerFadeSeconds = 0.1f;
        hud.ApplyConfig(hudConfig);

        GameConfig gameConfig = TheCircussyOneTestObjects.CreateConfig();
        var state = new GameState(gameConfig);
        var runScheduleConfig = RunScheduleConfig.CreateRuntimeDefault();
        var schedule = new RunActScheduleState(runScheduleConfig);
        var time = new FakeGameTime { DeltaTime = 0.49f };
        var audio = new FakeGameAudio();
        var presenter = new HudPresenter(
            state,
            hud,
            time,
            new FakeRunRestarter(),
            runSchedule: schedule,
            runScheduleConfig: runScheduleConfig,
            audio: audio);

        presenter.Start();
        schedule.BeginAct(1);
        Assert.That(audio.PlayedCues, Is.Empty);

        presenter.Tick();
        Assert.That(audio.PlayedCues, Is.Empty);

        time.DeltaTime = 0.02f;
        presenter.Tick();
        Assert.That(audio.PlayedCues, Is.EqualTo(new[] { GameAudioCue.AnnouncementAct }));

        time.DeltaTime = 0.1f;
        presenter.Tick();
        time.DeltaTime = 0.2f;
        presenter.Tick();
        Assert.That(audio.PlayedCues, Is.EqualTo(new[]
        {
            GameAudioCue.AnnouncementAct,
            GameAudioCue.AnnouncementFadeout
        }));

        presenter.Dispose();
        Object.DestroyImmediate(runScheduleConfig);
        Object.DestroyImmediate(hudConfig);
        Object.DestroyImmediate(gameConfig);
    }

    [Test]
    public void HudPresenterKeepsPausedGameDeltaForRunHudFeedback()
    {
        var pausedTime = new FakeGameTime { DeltaTime = 0f };

        Assert.That(HudFeedbackTimeRules.GameDeltaTime(pausedTime, 0.25f), Is.Zero);
        Assert.That(HudFeedbackTimeRules.GameDeltaTime(null, 0.25f), Is.EqualTo(0.25f).Within(0.001f));
    }

    [Test]
    public void RunAnnouncementDoesNotAdvanceWhenGameDeltaIsPaused()
    {
        HudView hud = CreateConfigBoundHud(
            out _,
            out _,
            out _,
            out _,
            out _,
            out _);
        var banner = new VisualElement();
        var kicker = new Label();
        var title = new Label();
        SetField(hud, "actBanner", banner);
        SetField(hud, "actBannerKickerLabel", kicker);
        SetField(hud, "actBannerTitleLabel", title);

        var hudConfig = ScriptableObject.CreateInstance<HudVisualConfig>();
        hudConfig.actBannerEnabled = true;
        hudConfig.actBannerDelaySeconds = 0.5f;
        hudConfig.actBannerPopSeconds = 0.1f;
        hudConfig.actBannerHoldSeconds = 0.2f;
        hudConfig.actBannerFadeSeconds = 0.1f;
        hud.ApplyConfig(hudConfig);

        int shownCount = 0;
        hud.RunAnnouncementShown += () => shownCount++;
        hud.ShowRunAnnouncement("ACT I", "Opening Act", hudConfig.actBannerDelaySeconds);

        hud.TickHudFeedback(gameDeltaTime: 0f, unscaledDeltaTime: 1f);
        Assert.That(shownCount, Is.Zero);
        Assert.That(banner.style.display.value, Is.EqualTo(DisplayStyle.None));

        hud.TickHudFeedback(gameDeltaTime: 0.5f, unscaledDeltaTime: 0f);
        Assert.That(shownCount, Is.EqualTo(1));
        Assert.That(banner.style.display.value, Is.EqualTo(DisplayStyle.Flex));

        Object.DestroyImmediate(hudConfig);
    }

    [Test]
    public void SceneBuilderAssignsHudVisualConfigToGeneratedHud()
    {
        var config = ScriptableObject.CreateInstance<HudVisualConfig>();
        MethodInfo method = typeof(TheCircussyOneSceneBuilder).GetMethod("CreateHud", BindingFlags.Static | BindingFlags.NonPublic);
        Assert.That(method, Is.Not.Null);

        var hud = (HudView)method.Invoke(null, new object[] { config });
        _hudObject = hud.gameObject;
        FieldInfo field = typeof(HudView).GetField("hudConfig", BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.That(field, Is.Not.Null);
        Assert.That(field.GetValue(hud), Is.SameAs(config));
        Assert.That(hud.GetComponent<DamageVignetteView>(), Is.Not.Null);

        Object.DestroyImmediate(config);
    }


    private HudView CreateBoundHud(
        out VisualElement healthFill,
        out VisualElement xpFill,
        out Label healthLabel,
        out Label xpLabel,
        out Label levelLabel,
        out Label killsLabel,
        out Label timerLabel)
    {
        _hudObject = new GameObject("HudViewTests");
        HudView hud = _hudObject.AddComponent<HudView>();

        healthFill = new VisualElement();
        xpFill = new VisualElement();
        healthLabel = new Label();
        xpLabel = new Label();
        levelLabel = new Label();
        killsLabel = new Label();
        timerLabel = new Label();

        SetField(hud, "healthFill", healthFill);
        SetField(hud, "experienceFill", xpFill);
        SetField(hud, "healthLabel", healthLabel);
        SetField(hud, "experienceLabel", xpLabel);
        SetField(hud, "levelLabel", levelLabel);
        SetField(hud, "killsLabel", killsLabel);
        SetField(hud, "timerLabel", timerLabel);

        return hud;
    }

    private HudView CreateInteractionPromptHud(
        out VisualElement interactionPrompt,
        out Label interactionButtonGlyph,
        out Label interactionPromptLabel,
        out VisualElement interactionProgressFill)
    {
        HudView hud = CreateBoundHud(out _, out _, out _, out _, out _, out _, out _);
        interactionPrompt = new VisualElement();
        interactionButtonGlyph = new Label();
        interactionPromptLabel = new Label();
        interactionProgressFill = new VisualElement();

        SetField(hud, "interactionPrompt", interactionPrompt);
        SetField(hud, "interactionButtonGlyph", interactionButtonGlyph);
        SetField(hud, "interactionPromptLabel", interactionPromptLabel);
        SetField(hud, "interactionProgressFill", interactionProgressFill);
        return hud;
    }

    private HudView CreateBoundHudWithGameOver(
        out VisualElement healthFill,
        out VisualElement xpFill,
        out Label healthLabel,
        out Label xpLabel,
        out Label levelLabel,
        out Label killsLabel,
        out Label timerLabel,
        out VisualElement gameOver)
    {
        HudView hud = CreateBoundHud(out healthFill, out xpFill, out healthLabel, out xpLabel, out levelLabel, out killsLabel, out timerLabel);
        gameOver = new VisualElement();
        SetField(hud, "gameOver", gameOver);
        return hud;
    }

    private HudView CreateConfigBoundHud(
        out VisualElement xpStrip,
        out VisualElement xpTrack,
        out VisualElement hudUnderRow,
        out VisualElement healthCluster,
        out VisualElement killsIcon,
        out Label timerLabel)
    {
        HudView hud = CreateBoundHud(
            out VisualElement healthFill,
            out VisualElement experienceFill,
            out Label healthLabel,
            out Label experienceLabel,
            out Label levelLabel,
            out Label killsLabel,
            out timerLabel);

        xpStrip = new VisualElement();
        xpTrack = new VisualElement();
        var xpSegments = new VisualElement();
        var xpPulse = new VisualElement();
        hudUnderRow = new VisualElement();
        healthCluster = new VisualElement();
        var healthTrack = new VisualElement();
        var healthSegments = new VisualElement();
        var killsCluster = new VisualElement();
        var ticketsCluster = new VisualElement();
        killsIcon = new VisualElement();
        var ticketsIcon = new VisualElement();
        var timerCluster = new VisualElement();
        var timerIcon = new VisualElement();
        var ticketsValue = new Label();
        var gameOver = new VisualElement();
        var gameOverTitle = new Label();
        var gameOverSubtitle = new Label();

        SetField(hud, "xpStrip", xpStrip);
        SetField(hud, "xpTrack", xpTrack);
        SetField(hud, "xpSegments", xpSegments);
        SetField(hud, "experiencePulse", xpPulse);
        SetField(hud, "hudUnderRow", hudUnderRow);
        SetField(hud, "healthCluster", healthCluster);
        SetField(hud, "healthTrack", healthTrack);
        SetField(hud, "healthSegments", healthSegments);
        SetField(hud, "killsCluster", killsCluster);
        SetField(hud, "killsIcon", killsIcon);
        SetField(hud, "ticketsCluster", ticketsCluster);
        SetField(hud, "ticketsIcon", ticketsIcon);
        SetField(hud, "ticketsLabel", ticketsValue);
        SetField(hud, "timerCluster", timerCluster);
        SetField(hud, "timerIcon", timerIcon);
        SetField(hud, "gameOver", gameOver);
        SetField(hud, "gameOverTitleLabel", gameOverTitle);
        SetField(hud, "gameOverSubtitleLabel", gameOverSubtitle);
        SetField(hud, "healthFill", healthFill);
        SetField(hud, "experienceFill", experienceFill);
        SetField(hud, "healthLabel", healthLabel);
        SetField(hud, "experienceLabel", experienceLabel);
        SetField(hud, "levelLabel", levelLabel);
        SetField(hud, "killsLabel", killsLabel);
        SetField(hud, "timerLabel", timerLabel);

        return hud;
    }

    private static void SetField(object target, string fieldName, object value)
    {
        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null, fieldName);
        field.SetValue(target, value);
    }

    private static float ScaleX(VisualElement element)
    {
        return element.style.scale.value.value.x;
    }

    private static T GetField<T>(HudView hud, string fieldName)
    {
        FieldInfo field = typeof(HudView).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null, fieldName);
        return (T)field.GetValue(hud);
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

    private sealed class FakeGameAudio : IGameAudio
    {
        public System.Collections.Generic.List<GameAudioCue> PlayedCues { get; } = new();

        public void Play(GameAudioCue cue)
        {
            PlayedCues.Add(cue);
        }

        public void PlayAt(GameAudioCue cue, Vector3 position)
        {
            PlayedCues.Add(cue);
        }

        public void PlayXpCollect(Vector3 position)
        {
            PlayedCues.Add(GameAudioCue.XpCollect);
        }

        public void StartLoop(GameAudioCue cue, object owner, Vector3 position, float initialProgress = 0f)
        {
        }

        public void UpdateLoop(GameAudioCue cue, object owner, Vector3 position, float progress01)
        {
        }

        public void StopLoop(GameAudioCue cue, object owner)
        {
        }

        public void StopAllLoops()
        {
        }
    }
}
