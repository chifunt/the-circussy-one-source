using NUnit.Framework;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;
using TheCircussyOne.Runtime;
using TheCircussyOne.Visuals;
using TMPro;
using UnityEngine;
using System.Reflection;

public sealed class WorldInteractionPromptTests
{
    [TearDown]
    public void TearDown()
    {
        TheCircussyOneTestObjects.DestroyAll();
    }

    [Test]
    public void PromptCopyUsesInputGlyphAndCollectingState()
    {
        Assert.That(WorldInteractionPromptRules.Glyph(useGamepadGlyph: false), Is.EqualTo("E"));
        Assert.That(WorldInteractionPromptRules.Glyph(useGamepadGlyph: true), Is.EqualTo("X"));
        Assert.That(WorldInteractionPromptRules.Label("Collect Tickets", isHolding: false), Is.EqualTo("to Collect Tickets"));
        Assert.That(WorldInteractionPromptRules.Label("Collect Tickets", isHolding: true), Is.EqualTo("Collecting Tickets"));
        Assert.That(WorldInteractionPromptRules.Label("Grab a Snack", isHolding: false), Is.EqualTo("to Grab a Snack"));
        Assert.That(WorldInteractionPromptRules.Label("Grab a Snack", isHolding: true), Is.EqualTo("Grabbing a Snack"));
        Assert.That(WorldInteractionPromptRules.Label("Need <color=#ff4758>12</color> more Tickets", isHolding: false, showActionPrefix: false), Is.EqualTo("Need <color=#ff4758>12</color> more Tickets"));
        Assert.That(WorldInteractionPromptRules.Label("Spend 35 Tickets to Open Chest", isHolding: true), Is.EqualTo("Spending 35 Tickets to Open Chest"));
        Assert.That(WorldInteractionPromptRules.Label("", isHolding: false), Is.Empty);
        Assert.That(WorldInteractionPromptRules.Label("", isHolding: true), Is.Empty);
    }

    [Test]
    public void ProgressShakeIntensifiesNearCompletion()
    {
        Vector2 early = WorldInteractionPromptRules.ProgressShake(
            0.2f,
            0.125f,
            0f,
            0.1f,
            1f,
            EaseSettings.Linear);
        Vector2 late = WorldInteractionPromptRules.ProgressShake(
            0.9f,
            0.125f,
            0f,
            0.1f,
            1f,
            EaseSettings.Linear);

        Assert.That(late.magnitude, Is.GreaterThan(early.magnitude));
    }

    [Test]
    public void PopScaleReachesTargetAndHideReturnsToZero()
    {
        float hidden = WorldInteractionPromptRules.Scale(
            WorldInteractionPromptTransition.Hidden,
            0f,
            0.16f,
            0.12f,
            0.72f,
            1f,
            EaseSettings.Linear,
            EaseSettings.Linear);
        float shown = WorldInteractionPromptRules.Scale(
            WorldInteractionPromptTransition.Showing,
            0.16f,
            0.16f,
            0.12f,
            0.72f,
            1f,
            EaseSettings.Linear,
            EaseSettings.Linear);
        float hiddenAfterHide = WorldInteractionPromptRules.Scale(
            WorldInteractionPromptTransition.Hiding,
            0.12f,
            0.16f,
            0.12f,
            0.72f,
            1f,
            EaseSettings.Linear,
            EaseSettings.Linear);

        Assert.That(hidden, Is.Zero);
        Assert.That(shown, Is.EqualTo(0.72f).Within(0.001f));
        Assert.That(hiddenAfterHide, Is.Zero.Within(0.001f));
    }

    [Test]
    public void ViewAppliesTextGlyphBillboardAndProgressFill()
    {
        WorldInteractionPromptVisualConfig config = TheCircussyOneTestObjects.CreateWorldInteractionPromptConfig();
        WorldInteractionPromptView view = TheCircussyOneTestObjects.CreateWorldInteractionPromptPrefab(config);
        var cameraObject = new GameObject("Prompt Camera");
        var camera = cameraObject.AddComponent<Camera>();
        camera.transform.position = new Vector3(0f, 2f, -4f);

        var frame = new WorldInteractionPromptFrame(
            true,
            new Vector3(1f, 2f, 3f),
            0.75f,
            1f,
            "Collecting Tickets",
            "X",
            showGlyph: false,
            useGamepadGlyph: true,
            showProgress: true,
            progress: 0.5f,
            progressShake: new Vector2(0.02f, 0.01f));

        view.ApplyFrame(frame, config, camera);

        Assert.That(view.IsActive, Is.True);
        Assert.That(view.Label, Is.EqualTo("Collecting Tickets"));
        Assert.That(view.Glyph, Is.EqualTo("X"));
        Assert.That(view.ProgressFillScaleX, Is.EqualTo(config.progressBarWidth * 0.5f).Within(0.001f));
        Assert.That(view.ProgressRootLocalPosition.x, Is.EqualTo(0.02f).Within(0.001f));
        Assert.That(view.ProgressRootLocalPosition.y, Is.EqualTo(config.progressBarYOffset + 0.01f).Within(0.001f));

        TheCircussyOneTestObjects.Destroy(cameraObject);
    }

    [Test]
    public void ViewUsesBoundedTextBoxesWithReadableGlyphRow()
    {
        WorldInteractionPromptVisualConfig config = TheCircussyOneTestObjects.CreateWorldInteractionPromptConfig();
        WorldInteractionPromptView view = TheCircussyOneTestObjects.CreateWorldInteractionPromptPrefab(config);
        var frame = new WorldInteractionPromptFrame(
            true,
            Vector3.zero,
            1f,
            1f,
            "to Collect Tickets",
            "E",
            showGlyph: true,
            useGamepadGlyph: false,
            showProgress: false,
            progress: 0f,
            progressShake: Vector2.zero);

        view.ApplyFrame(frame, config, camera: null);

        TextMeshPro label = view.transform.Find("Visual Root/Prompt Text")?.GetComponent<TextMeshPro>();
        TextMeshPro glyph = view.transform.Find("Visual Root/Glyph Text")?.GetComponent<TextMeshPro>();
        TextMeshPro hold = view.transform.Find("Visual Root/Hold Text")?.GetComponent<TextMeshPro>();

        Assert.That(label, Is.Not.Null);
        Assert.That(glyph, Is.Not.Null);
        Assert.That(hold, Is.Not.Null);
        Assert.That(label.alignment, Is.EqualTo(TextAlignmentOptions.MidlineLeft));
        Assert.That(glyph.alignment, Is.EqualTo(TextAlignmentOptions.Center));
        Assert.That(hold.alignment, Is.EqualTo(TextAlignmentOptions.MidlineLeft));
        Assert.That(label.fontStyle, Is.EqualTo(FontStyles.Bold));
        Assert.That(glyph.fontStyle, Is.EqualTo(FontStyles.Bold));
        Assert.That(hold.fontStyle, Is.EqualTo(FontStyles.Bold));
        Assert.That(label.overflowMode, Is.EqualTo(TextOverflowModes.Overflow));
        Assert.That(label.richText, Is.True);
        Assert.That(label.rectTransform.pivot, Is.EqualTo(new Vector2(0f, 0.5f)));
        Assert.That(glyph.rectTransform.pivot, Is.EqualTo(new Vector2(0.5f, 0.5f)));
        Assert.That(hold.rectTransform.pivot, Is.EqualTo(new Vector2(0f, 0.5f)));
        Assert.That(label.rectTransform.sizeDelta.x, Is.GreaterThan(0f));
        Assert.That(label.rectTransform.sizeDelta.x, Is.LessThanOrEqualTo(config.labelMaxWidth));
        Assert.That(label.rectTransform.sizeDelta.y, Is.EqualTo(config.labelTextBoxSize.y).Within(0.001f));
        Assert.That(hold.rectTransform.sizeDelta.x, Is.GreaterThan(0f));
        Assert.That(hold.rectTransform.sizeDelta.x, Is.LessThanOrEqualTo(config.labelMaxWidth));
        Assert.That(hold.rectTransform.sizeDelta.y, Is.EqualTo(config.labelTextBoxSize.y).Within(0.001f));
        Assert.That(glyph.rectTransform.sizeDelta.x, Is.EqualTo(config.glyphTextBoxSize.x).Within(0.001f));
        Assert.That(glyph.rectTransform.sizeDelta.y, Is.EqualTo(config.glyphTextBoxSize.y).Within(0.001f));

        float holdRightEdge = view.HoldPrefixLocalPosition.x + view.HoldPrefixTextBoxSize.x;
        float glyphLeftEdge = view.GlyphBackgroundLocalPosition.x - config.glyphRadius;
        float glyphRightEdge = view.GlyphBackgroundLocalPosition.x + config.glyphRadius;
        float labelLeftEdge = view.LabelLocalPosition.x;
        float rowLeft = view.HoldPrefixLocalPosition.x;
        float rowRight = view.LabelLocalPosition.x + view.LabelTextBoxSize.x;
        Assert.That(glyphLeftEdge, Is.GreaterThan(holdRightEdge), "The Hold prefix should not overlap the input glyph.");
        Assert.That(labelLeftEdge, Is.GreaterThan(glyphRightEdge), "The input glyph should not overlap or hide the first letters of the prompt text.");
        Assert.That((rowLeft + rowRight) * 0.5f, Is.EqualTo(0f).Within(0.001f), "The Hold prefix, glyph, and prompt text should be centered together above the target.");
        Assert.That(hold.text, Is.EqualTo("Hold"));
        Assert.That(label.text, Is.EqualTo("to Collect Tickets"));
    }

    [Test]
    public void ViewKeepsHoldPrefixOnSameOutlinedMaterialAsPromptText()
    {
        WorldInteractionPromptVisualConfig config = TheCircussyOneTestObjects.CreateWorldInteractionPromptConfig();
        WorldInteractionPromptView view = TheCircussyOneTestObjects.CreateWorldInteractionPromptPrefab(config);
        TextMeshPro label = view.transform.Find("Visual Root/Prompt Text")?.GetComponent<TextMeshPro>();
        TextMeshPro glyph = view.transform.Find("Visual Root/Glyph Text")?.GetComponent<TextMeshPro>();
        TextMeshPro hold = view.transform.Find("Visual Root/Hold Text")?.GetComponent<TextMeshPro>();
        Assert.That(label, Is.Not.Null);
        Assert.That(glyph, Is.Not.Null);
        Assert.That(hold, Is.Not.Null);
        Material expectedMaterial = label.fontSharedMaterial;
        Assert.That(expectedMaterial, Is.Not.Null);
        var staleMaterial = new Material(expectedMaterial);
        hold.fontSharedMaterial = staleMaterial;
        hold.GetComponent<Renderer>().sharedMaterial = staleMaterial;

        var frame = new WorldInteractionPromptFrame(
            true,
            Vector3.zero,
            1f,
            1f,
            "to Collect Tickets",
            "E",
            showGlyph: true,
            useGamepadGlyph: false,
            showProgress: false,
            progress: 0f,
            progressShake: Vector2.zero);

        view.ApplyFrame(frame, config, camera: null);

        Assert.That(hold.fontSharedMaterial, Is.SameAs(expectedMaterial));
        Assert.That(hold.GetComponent<Renderer>().sharedMaterial, Is.SameAs(expectedMaterial));
        Assert.That(glyph.fontSharedMaterial, Is.SameAs(expectedMaterial));
        TheCircussyOneTestObjects.Destroy(staleMaterial);
    }

    [Test]
    public void ViewCentersCollectingTextWhenGlyphIsHidden()
    {
        WorldInteractionPromptVisualConfig config = TheCircussyOneTestObjects.CreateWorldInteractionPromptConfig();
        WorldInteractionPromptView view = TheCircussyOneTestObjects.CreateWorldInteractionPromptPrefab(config);
        var frame = new WorldInteractionPromptFrame(
            true,
            Vector3.zero,
            1f,
            1f,
            "Collecting Tickets",
            "E",
            showGlyph: false,
            useGamepadGlyph: false,
            showProgress: true,
            progress: 0.25f,
            progressShake: Vector2.zero);

        view.ApplyFrame(frame, config, camera: null);

        TextMeshPro label = view.transform.Find("Visual Root/Prompt Text")?.GetComponent<TextMeshPro>();
        TextMeshPro hold = view.transform.Find("Visual Root/Hold Text")?.GetComponent<TextMeshPro>();
        Assert.That(label, Is.Not.Null);
        Assert.That(hold, Is.Not.Null);
        Assert.That(hold.gameObject.activeSelf, Is.False);
        Assert.That(label.alignment, Is.EqualTo(TextAlignmentOptions.Center));
        Assert.That(label.rectTransform.pivot, Is.EqualTo(new Vector2(0.5f, 0.5f)));
        Assert.That(label.transform.localPosition, Is.EqualTo(new Vector3(0f, config.rowYOffset, 0f)));
    }

    [Test]
    public void SystemCanShowStatusPromptWithoutInputGlyph()
    {
        WorldInteractionPromptVisualConfig config = TheCircussyOneTestObjects.CreateWorldInteractionPromptConfig();
        WorldInteractionPromptView prefab = TheCircussyOneTestObjects.CreateWorldInteractionPromptPrefab(config);
        var rootObject = new GameObject("Prompt Runtime Root");
        var factory = new WorldInteractionPromptFactory(config, prefab, rootObject.transform);
        var system = new WorldInteractionPromptSystem(config, factory);

        system.Show(
            new PromptTarget(Vector3.zero),
            "Need <color=#ff4758>12</color> more Tickets",
            0f,
            isHolding: false,
            useGamepadGlyph: true,
            showGlyph: false);
        system.Tick();

        WorldInteractionPromptView view = factory.View;
        Assert.That(view, Is.Not.Null);
        Assert.That(view.Label, Is.EqualTo("Need <color=#ff4758>12</color> more Tickets"));
        Assert.That(view.transform.Find("Visual Root/Hold Text")?.gameObject.activeSelf, Is.False);
        Assert.That(view.transform.Find("Visual Root/Glyph Text")?.gameObject.activeSelf, Is.False);
        Assert.That(view.transform.Find("Visual Root/Prompt Text")?.GetComponent<TextMeshPro>()?.richText, Is.True);
        TheCircussyOneTestObjects.Destroy(rootObject);
    }

    [Test]
    public void SystemHidesPromptImmediatelyWhilePaused()
    {
        WorldInteractionPromptVisualConfig config = TheCircussyOneTestObjects.CreateWorldInteractionPromptConfig();
        WorldInteractionPromptView prefab = TheCircussyOneTestObjects.CreateWorldInteractionPromptPrefab(config);
        var rootObject = new GameObject("Prompt Runtime Root");
        var pauseState = new RunPauseState();
        var factory = new WorldInteractionPromptFactory(config, prefab, rootObject.transform);
        var system = new WorldInteractionPromptSystem(config, factory, pauseState: pauseState);

        system.Show(new PromptTarget(Vector3.zero), "Collect Tickets", 0.5f, isHolding: true, useGamepadGlyph: false);
        system.Tick();
        Assert.That(factory.View, Is.Not.Null);
        Assert.That(factory.View.IsActive, Is.True);
        SetFloat(system, "transitionSeconds", 0.05f);
        SetFloat(system, "visibleSeconds", 1.2f);
        pauseState.Pause(RunPauseReasons.UpgradeSelection);

        system.Tick();
        system.Tick();

        Assert.That(GetFloat(system, "transitionSeconds"), Is.Zero);
        Assert.That(GetFloat(system, "visibleSeconds"), Is.Zero);
        Assert.That(system.IsShowing, Is.False);
        Assert.That(factory.View.IsActive, Is.False);
        TheCircussyOneTestObjects.Destroy(rootObject);
    }

    [Test]
    public void SystemHideWhilePausedDeactivatesPromptImmediately()
    {
        WorldInteractionPromptVisualConfig config = TheCircussyOneTestObjects.CreateWorldInteractionPromptConfig();
        WorldInteractionPromptView prefab = TheCircussyOneTestObjects.CreateWorldInteractionPromptPrefab(config);
        var rootObject = new GameObject("Prompt Runtime Root");
        var pauseState = new RunPauseState();
        var factory = new WorldInteractionPromptFactory(config, prefab, rootObject.transform);
        var system = new WorldInteractionPromptSystem(config, factory, pauseState: pauseState);

        system.Show(new PromptTarget(Vector3.zero), "Open Chest", 1f, isHolding: true, useGamepadGlyph: false);
        system.Tick();
        Assert.That(factory.View, Is.Not.Null);
        Assert.That(factory.View.IsActive, Is.True);

        pauseState.Pause(RunPauseReasons.RewardReveal);
        system.Hide();
        system.Tick();

        Assert.That(system.IsShowing, Is.False);
        Assert.That(factory.View.IsActive, Is.False);
        TheCircussyOneTestObjects.Destroy(rootObject);
    }

    [Test]
    public void SystemDisposeDestroysRuntimePromptView()
    {
        WorldInteractionPromptVisualConfig config = TheCircussyOneTestObjects.CreateWorldInteractionPromptConfig();
        WorldInteractionPromptView prefab = TheCircussyOneTestObjects.CreateWorldInteractionPromptPrefab(config);
        var rootObject = new GameObject("Prompt Runtime Root");
        var factory = new WorldInteractionPromptFactory(config, prefab, rootObject.transform);
        var system = new WorldInteractionPromptSystem(config, factory);

        system.Show(new PromptTarget(Vector3.zero), "Collect Tickets", 0f, isHolding: false, useGamepadGlyph: false);
        system.Tick();
        WorldInteractionPromptView view = factory.View;

        Assert.That(view, Is.Not.Null);
        system.Dispose();

        Assert.That(factory.View, Is.Null);
        Assert.That(view == null, Is.True);
        TheCircussyOneTestObjects.Destroy(rootObject);
    }

    [Test]
    public void SystemIgnoresDestroyedCachedViewWhileHidden()
    {
        WorldInteractionPromptVisualConfig config = TheCircussyOneTestObjects.CreateWorldInteractionPromptConfig();
        WorldInteractionPromptView prefab = TheCircussyOneTestObjects.CreateWorldInteractionPromptPrefab(config);
        var rootObject = new GameObject("Prompt Runtime Root");
        var factory = new WorldInteractionPromptFactory(config, prefab, rootObject.transform);
        var system = new WorldInteractionPromptSystem(config, factory);

        system.Show(new PromptTarget(Vector3.zero), "Collect Tickets", 0f, isHolding: false, useGamepadGlyph: false);
        system.Tick();
        WorldInteractionPromptView view = factory.View;
        Assert.That(view, Is.Not.Null);
        Object.DestroyImmediate(view.gameObject);
        SetTransition(system, WorldInteractionPromptTransition.Hidden);

        Assert.DoesNotThrow(() => system.Tick());
        Assert.That(factory.View, Is.Null);
        TheCircussyOneTestObjects.Destroy(rootObject);
    }

    [Test]
    public void SystemRecreatesPromptAfterCachedViewWasDestroyed()
    {
        WorldInteractionPromptVisualConfig config = TheCircussyOneTestObjects.CreateWorldInteractionPromptConfig();
        WorldInteractionPromptView prefab = TheCircussyOneTestObjects.CreateWorldInteractionPromptPrefab(config);
        var rootObject = new GameObject("Prompt Runtime Root");
        var factory = new WorldInteractionPromptFactory(config, prefab, rootObject.transform);
        var system = new WorldInteractionPromptSystem(config, factory);
        var target = new PromptTarget(Vector3.zero);

        system.Show(target, "Collect Tickets", 0f, isHolding: false, useGamepadGlyph: false);
        system.Tick();
        WorldInteractionPromptView first = factory.View;
        Assert.That(first, Is.Not.Null);
        Object.DestroyImmediate(first.gameObject);
        SetTransition(system, WorldInteractionPromptTransition.Visible);

        system.Show(target, "Collect Tickets", 0f, isHolding: false, useGamepadGlyph: false);
        Assert.DoesNotThrow(() => system.Tick());
        WorldInteractionPromptView second = factory.View;
        Assert.That(second, Is.Not.Null);
        Assert.That(second == first, Is.False);
        TheCircussyOneTestObjects.Destroy(rootObject);
    }

    private static void SetTransition(WorldInteractionPromptSystem system, WorldInteractionPromptTransition transition)
    {
        FieldInfo field = typeof(WorldInteractionPromptSystem).GetField("transition", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null);
        field.SetValue(system, transition);
    }

    private static void SetFloat(WorldInteractionPromptSystem system, string fieldName, float value)
    {
        FieldInfo field = typeof(WorldInteractionPromptSystem).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null);
        field.SetValue(system, value);
    }

    private static float GetFloat(WorldInteractionPromptSystem system, string fieldName)
    {
        FieldInfo field = typeof(WorldInteractionPromptSystem).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null);
        return (float)field.GetValue(system);
    }

    private sealed class PromptTarget : IWorldInteractionPromptTarget
    {
        public PromptTarget(Vector3 position)
        {
            WorldInteractionPromptPosition = position;
        }

        public Vector3 WorldInteractionPromptPosition { get; }
    }
}
