using NUnit.Framework;
using UnityEngine;
using TheCircussyOne.Rules;
using TheCircussyOne.Visuals;

public sealed class ActorBodyViewTests
{
    private static readonly int BaseColorProperty = Shader.PropertyToID("_BaseColor");
    private static readonly int GridColorProperty = Shader.PropertyToID("_GridColor");
    private static readonly int EmissionColorProperty = Shader.PropertyToID("_EmissionColor");
    private static readonly int EmissionStrengthProperty = Shader.PropertyToID("_EmissionStrength");

    private Material _material;

    [TearDown]
    public void TearDown()
    {
        TheCircussyOneTestObjects.DestroyAll();
        if (_material != null)
        {
            Object.DestroyImmediate(_material);
        }
    }

    [Test]
    public void FlashAppliesPropertyBlocksToBodyAndFaceRenderers()
    {
        ActorBodyView view = CreateActorBody(out Transform body, out Renderer bodyRenderer, out Renderer faceRenderer);
        Color materialColor = new(0.2f, 0.7f, 0.4f, 1f);
        _material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        _material.color = materialColor;
        bodyRenderer.sharedMaterial = _material;
        faceRenderer.sharedMaterial = _material;
        var propertyBlock = new MaterialPropertyBlock();

        view.CaptureBaseColor();
        view.PlayFlash(Color.white, 0.1f, EaseSettings.OutQuad, 4f);

        bodyRenderer.GetPropertyBlock(propertyBlock);
        Assert.That(propertyBlock.isEmpty, Is.False);
        AssertColor(propertyBlock.GetColor(BaseColorProperty), Color.white);
        AssertColor(propertyBlock.GetColor(GridColorProperty), Color.white);
        AssertColor(propertyBlock.GetColor(EmissionColorProperty), Color.white);
        Assert.That(propertyBlock.GetFloat(EmissionStrengthProperty), Is.EqualTo(4f).Within(0.0001f));
        faceRenderer.GetPropertyBlock(propertyBlock);
        Assert.That(propertyBlock.isEmpty, Is.False);
        AssertColor(propertyBlock.GetColor(BaseColorProperty), Color.white);
        AssertColor(view.CurrentBodyColor, Color.white);
        AssertColor(bodyRenderer.sharedMaterial.color, materialColor);
        Assert.That(body.localScale, Is.EqualTo(Vector3.one));
    }

    [Test]
    public void ClearingFlashRestoresEmptyPropertyBlocksAndMaterialOwnedColor()
    {
        ActorBodyView view = CreateActorBody(out _, out Renderer bodyRenderer, out Renderer faceRenderer);
        Color materialColor = new(0.23f, 0.35f, 0.83f, 1f);
        _material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        _material.color = materialColor;
        bodyRenderer.sharedMaterial = _material;
        faceRenderer.sharedMaterial = _material;
        var propertyBlock = new MaterialPropertyBlock();

        view.CaptureBaseColor();
        view.PlayFlash(Color.red, 0.1f, EaseSettings.OutQuad, 2f);
        view.ClearBodyPropertyBlock();

        bodyRenderer.GetPropertyBlock(propertyBlock);
        Assert.That(propertyBlock.isEmpty, Is.True);
        faceRenderer.GetPropertyBlock(propertyBlock);
        Assert.That(propertyBlock.isEmpty, Is.True);
        AssertColor(view.CurrentBodyColor, materialColor);
        AssertColor(bodyRenderer.sharedMaterial.color, materialColor);
    }

    [Test]
    public void MotionVisualsScaleOnlyBodyScaleRootAndPreserveBodyBase()
    {
        ActorBodyView view = CreateActorBody(out Transform body, out _, out _);
        Transform scaleRoot = view.BodyScaleRoot;
        float baseBefore = BodyBaseWorldY(body);

        view.SetMotionVisualSpeed(1f, immediate: true);
        view.TickMotionVisuals(
            0.105f,
            idleAmplitude: 0.018f,
            idleSecondsPerCycle: 1.35f,
            idleXzCompensation: 0.5f,
            moveAmplitude: 0.05f,
            moveSecondsPerCycle: 0.42f,
            moveXzCompensation: 0.55f,
            speedSmoothingSharpness: 16f,
            speedSmoothingEase: EaseSettings.Exponential);

        Assert.That(view.transform.localScale, Is.EqualTo(Vector3.one));
        Assert.That(scaleRoot.localScale, Is.Not.EqualTo(Vector3.one));
        Assert.That(body.localScale, Is.EqualTo(Vector3.one));
        Assert.That(BodyBaseWorldY(body), Is.EqualTo(baseBefore).Within(0.001f));
    }

    [Test]
    public void GroundShadowRemainsProjectedToFloorWhileRootIsAirborne()
    {
        ActorBodyView view = CreateActorBody(out _, out _, out _);
        GameObject shadow = GameObject.CreatePrimitive(PrimitiveType.Quad);
        shadow.name = "Ground Shadow";
        shadow.transform.SetParent(view.transform, false);
        shadow.transform.localPosition = new Vector3(0f, 0.035f, 0f);
        shadow.transform.localScale = Vector3.one;
        view.transform.position = Vector3.up * 2f;

        view.UpdateGroundProjectedShadow(rootWorldY: 2f);

        Assert.That(shadow.transform.position.y, Is.EqualTo(0.035f).Within(0.001f));
        Assert.That(shadow.transform.localScale.x, Is.LessThan(1f));
        Assert.That(shadow.transform.localScale.y, Is.LessThan(1f));
        Assert.That(shadow.transform.localScale.z, Is.EqualTo(1f).Within(0.001f));
    }

    [Test]
    public void HealthBarIsHiddenAtFullHealthAndVisibleWhenDamaged()
    {
        ActorBodyView view = CreateActorBody(out _, out _, out _);
        HealthBarView healthBar = view.HealthBar;

        view.SetHealthNormalized(1f);
        Assert.That(healthBar.gameObject.activeSelf, Is.False);

        view.SetHealthNormalized(0.5f);
        Assert.That(healthBar.gameObject.activeSelf, Is.True);

        view.SetHealthNormalized(1f, enabled: false);
        Assert.That(healthBar.gameObject.activeSelf, Is.False);
    }

    private static ActorBodyView CreateActorBody(out Transform body, out Renderer bodyRenderer, out Renderer faceRenderer)
    {
        GameObject root = TheCircussyOneTestObjects.CreateRoot("Actor Body");
        GameObject bodyObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        bodyObject.name = "Body";
        bodyObject.transform.SetParent(root.transform, false);
        bodyObject.transform.localPosition = Vector3.up;
        Object.DestroyImmediate(bodyObject.GetComponent<Collider>());

        GameObject faceObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        faceObject.name = "Face Test Eye";
        faceObject.transform.SetParent(bodyObject.transform, false);
        Object.DestroyImmediate(faceObject.GetComponent<Collider>());

        GameObject healthBarObject = new("Health Bar");
        healthBarObject.transform.SetParent(root.transform, false);
        healthBarObject.AddComponent<HealthBarView>();

        var view = root.AddComponent<ActorBodyView>();
        body = view.Body;
        bodyRenderer = bodyObject.GetComponent<Renderer>();
        faceRenderer = faceObject.GetComponent<Renderer>();
        return view;
    }

    private static float BodyBaseWorldY(Transform body)
    {
        MeshFilter meshFilter = body.GetComponent<MeshFilter>();
        if (meshFilter == null || meshFilter.sharedMesh == null)
        {
            return body.position.y;
        }

        return body.TransformPoint(new Vector3(0f, meshFilter.sharedMesh.bounds.min.y, 0f)).y;
    }

    private static void AssertColor(Color actual, Color expected)
    {
        Assert.That(actual.r, Is.EqualTo(expected.r).Within(0.0001f));
        Assert.That(actual.g, Is.EqualTo(expected.g).Within(0.0001f));
        Assert.That(actual.b, Is.EqualTo(expected.b).Within(0.0001f));
        Assert.That(actual.a, Is.EqualTo(expected.a).Within(0.0001f));
    }
}
