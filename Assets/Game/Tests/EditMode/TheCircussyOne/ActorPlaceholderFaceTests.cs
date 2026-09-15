using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public sealed class ActorPlaceholderFaceTests
{
    private GameObject _body;
    private Material _material;

    [TearDown]
    public void TearDown()
    {
        TheCircussyOneTestObjects.DestroyAll();
        if (_body != null)
        {
            Object.DestroyImmediate(_body);
        }

        if (_material != null)
        {
            Object.DestroyImmediate(_material);
        }
    }

    [Test]
    public void PlaceholderFaceHasForwardEyesMouthAndNoColliders()
    {
        _body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        _body.name = "Body";
        _material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));

        InvokeAddPlaceholderFace(_body, _material);

        Transform face = _body.transform.Find("Face");
        Assert.That(face, Is.Not.Null);
        Assert.That(face.childCount, Is.EqualTo(3));

        Transform leftEye = face.Find("Left Eye");
        Transform rightEye = face.Find("Right Eye");
        Transform mouth = face.Find("Mouth");

        Assert.That(leftEye, Is.Not.Null);
        Assert.That(rightEye, Is.Not.Null);
        Assert.That(mouth, Is.Not.Null);
        Assert.That(leftEye.localPosition.x, Is.LessThan(0f));
        Assert.That(rightEye.localPosition.x, Is.GreaterThan(0f));
        Assert.That(leftEye.localPosition.z, Is.GreaterThan(0.4f));
        Assert.That(rightEye.localPosition.z, Is.GreaterThan(0.4f));
        Assert.That(mouth.localPosition.z, Is.GreaterThan(0.4f));
        Assert.That(mouth.localEulerAngles.z, Is.EqualTo(90f).Within(0.001f));
        Assert.That(face.GetComponentsInChildren<Collider>(includeInactive: true), Is.Empty);

        foreach (Renderer renderer in face.GetComponentsInChildren<Renderer>(includeInactive: true))
        {
            Assert.That(renderer.sharedMaterial, Is.EqualTo(_material));
            Assert.That(renderer.shadowCastingMode, Is.EqualTo(UnityEngine.Rendering.ShadowCastingMode.Off));
            Assert.That(renderer.receiveShadows, Is.False);
        }
    }

    private static void InvokeAddPlaceholderFace(GameObject body, Material material)
    {
        MethodInfo method = typeof(TheCircussyOneSceneBuilder).GetMethod(
            "AddPlaceholderFace",
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.That(method, Is.Not.Null);
        method.Invoke(null, new object[] { body, material });
    }
}
