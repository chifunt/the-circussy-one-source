using System.Collections.Generic;
using NUnit.Framework;
using TheCircussyOne.Runtime;
using TheCircussyOne.Visuals;
using UnityEngine;

public sealed class WorldInteractableVisualDriverTests
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
        InteractableSelectionOutlineRegistry.Clear();
    }

    [Test]
    public void InteractionScaleMatchesSharedSquashStretchFormula()
    {
        var state = new WorldInteractableViewState();
        state.BeginInteraction();
        state.AdvanceInteraction(0.25f);
        var style = new WorldInteractableVisualStyle(
            1f,
            Color.white,
            Color.white,
            0f,
            Color.white,
            0f,
            0.1f,
            1f,
            1f);

        Vector3 scale = WorldInteractableVisualDriver.InteractionScale(style, state);

        Assert.That(scale.x, Is.EqualTo(0.95f).Within(0.001f));
        Assert.That(scale.y, Is.EqualTo(1.1f).Within(0.001f));
        Assert.That(scale.z, Is.EqualTo(0.95f).Within(0.001f));
    }

    [Test]
    public void PromptAnchorUsesActiveRendererBoundsTop()
    {
        GameObject root = Track(new GameObject("Interactable"));
        root.transform.position = new Vector3(2f, 0.5f, 3f);
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.transform.SetParent(root.transform, worldPositionStays: false);
        body.transform.localScale = new Vector3(1f, 2f, 1f);
        Transform visualRoot = body.transform;
        Renderer[] renderers = null;
        var driver = new WorldInteractableVisualDriver();

        driver.Resolve(root.transform, ref visualRoot, ref renderers);
        Vector3 anchor = driver.ResolvePromptPosition(root.transform, fallbackHeight: 0.35f);

        Assert.That(anchor.x, Is.EqualTo(root.transform.position.x).Within(0.001f));
        Assert.That(anchor.z, Is.EqualTo(root.transform.position.z).Within(0.001f));
        Assert.That(anchor.y, Is.GreaterThan(root.transform.position.y + 0.9f));
    }

    [Test]
    public void ApplyKeepsVisualRootBottomAnchoredDuringScale()
    {
        GameObject root = Track(new GameObject("Interactable"));
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.transform.SetParent(root.transform, worldPositionStays: false);
        Transform visualRoot = body.transform;
        Renderer[] renderers = null;
        var driver = new WorldInteractableVisualDriver();
        var style = new WorldInteractableVisualStyle(
            2f,
            Color.white,
            Color.white,
            0f,
            Color.white,
            0f,
            0f,
            0f,
            1f);

        driver.Resolve(root.transform, ref visualRoot, ref renderers);
        float bottomBefore = visualRoot.position.y - body.GetComponent<Renderer>().bounds.extents.y;
        driver.Apply(style, new WorldInteractableViewState());
        float bottomAfter = visualRoot.position.y - body.GetComponent<Renderer>().bounds.extents.y;

        Assert.That(bottomAfter, Is.EqualTo(bottomBefore).Within(0.001f));
    }

    [Test]
    public void ApplySetsMaterialColorEmissionAndOutline()
    {
        GameObject root = Track(GameObject.CreatePrimitive(PrimitiveType.Cube));
        Transform visualRoot = root.transform;
        Renderer[] renderers = null;
        var driver = new WorldInteractableVisualDriver();
        var state = new WorldInteractableViewState();
        state.SetTargeted(true);
        var style = new WorldInteractableVisualStyle(
            1f,
            new Color(0.2f, 0.4f, 0.6f, 1f),
            new Color(1f, 0.8f, 0.2f, 1f),
            0.75f,
            new Color(1f, 0.9f, 0.3f, 1f),
            4f,
            0f,
            0f,
            1f);

        driver.Resolve(root.transform, ref visualRoot, ref renderers);
        driver.Apply(style, state);
        driver.UpdateOutline(root, state.Targeted, style.OutlineColor, style.OutlineThickness);

        var block = new MaterialPropertyBlock();
        root.GetComponent<Renderer>().GetPropertyBlock(block);
        AssertColor(block.GetColor("_BaseColor"), Color.Lerp(style.VisualColor, Color.white, style.TargetedColorLerp));
        AssertColor(block.GetColor("_EmissionColor"), style.EmissionColor);
        Assert.That(block.GetFloat("_EmissionSelfGlow"), Is.EqualTo(0.75f * 1.35f).Within(0.001f));
        Assert.That(InteractableSelectionOutlineRegistry.Count, Is.EqualTo(1));
        Assert.That(InteractableSelectionOutlineRegistry.TryGetCompositeSettings(out var settings), Is.True);
        AssertColor(settings.Color, style.OutlineColor);
        Assert.That(settings.WidthPixels, Is.EqualTo(4f).Within(0.001f));
    }

    [Test]
    public void RewardInteractableDriverOwnsRegistrationTargetStateAndCompletion()
    {
        GameObject root = Track(GameObject.CreatePrimitive(PrimitiveType.Cube));
        var owner = root.AddComponent<FakeInteractableOwner>();
        var behavior = new FakeRewardBehavior();
        var driver = new WorldRewardInteractableDriver(behavior);

        driver.Resolve(root.transform);
        driver.Register(owner, owner);
        driver.SetInteractionTargeted(owner, root.transform, targeted: true);
        driver.BeginInteraction(root.transform);
        behavior.NextResult = WorldRewardInteractionTickResult.CompletedResult();

        WorldRewardInteractionTickResult result = driver.TickInteraction(owner, root.transform, 0.25f, new RunCurrencyState());

        Assert.That(SceneInteractableRegistry.CountByKind(SceneInteractableKind.TicketDeposit), Is.EqualTo(1));
        Assert.That(InteractableSelectionOutlineRegistry.Count, Is.Zero);
        Assert.That(driver.IsTargeted, Is.False);
        Assert.That(driver.IsInteracting, Is.False);
        Assert.That(result.Completed, Is.True);
        Assert.That(behavior.BeginCount, Is.EqualTo(1));
        Assert.That(behavior.TickCount, Is.EqualTo(1));
    }

    [Test]
    public void RewardInteractableDriverDeactivateUnregistersAndClearsOutline()
    {
        GameObject root = Track(GameObject.CreatePrimitive(PrimitiveType.Cube));
        var owner = root.AddComponent<FakeInteractableOwner>();
        var behavior = new FakeRewardBehavior();
        var driver = new WorldRewardInteractableDriver(behavior);

        driver.Resolve(root.transform);
        driver.Register(owner, owner);
        driver.SetInteractionTargeted(owner, root.transform, targeted: true);

        driver.Deactivate(owner);

        Assert.That(SceneInteractableRegistry.Count, Is.Zero);
        Assert.That(InteractableSelectionOutlineRegistry.Count, Is.Zero);
        Assert.That(driver.IsTargeted, Is.False);
        Assert.That(driver.IsInteracting, Is.False);
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

    private sealed class FakeRewardBehavior : IWorldRewardInteractionBehavior
    {
        public bool IsRewardInteractionAvailable { get; set; } = true;
        public SceneInteractableKind RegistryKind => SceneInteractableKind.TicketDeposit;
        public WorldRewardInteractionTickResult NextResult { get; set; } = WorldRewardInteractionTickResult.InProgress();
        public int BeginCount { get; private set; }
        public int TickCount { get; private set; }
        public int CancelCount { get; private set; }

        public WorldInteractableVisualStyle VisualStyle => new(
            1f,
            Color.white,
            Color.yellow,
            0.5f,
            Color.cyan,
            3f,
            0.1f,
            1f,
            1f);

        public void BeginRewardInteraction()
        {
            BeginCount++;
        }

        public WorldRewardInteractionTickResult TickRewardInteraction(float deltaTime, RunCurrencyState currency)
        {
            TickCount++;
            return NextResult;
        }

        public void CancelRewardInteraction()
        {
            CancelCount++;
        }
    }

    private sealed class FakeInteractableOwner : MonoBehaviour, IInteractable
    {
        public bool IsInteractionAvailable => true;
        public InteractPriority Priority => InteractPriority.TicketDeposit;
        public Vector3 InteractionPosition => transform.position;
        public string PromptText => "Fake";
        public float HoldSeconds => 1f;
        public float Progress => 0f;

        public void SetInteractionTargeted(bool targeted)
        {
        }

        public void BeginInteraction()
        {
        }

        public void TickInteraction(float deltaTime, RunCurrencyState currency)
        {
        }

        public void CancelInteraction()
        {
        }
    }
}
