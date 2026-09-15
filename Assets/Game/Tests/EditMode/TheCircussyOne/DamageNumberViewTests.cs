using NUnit.Framework;
using TMPro;
using UnityEngine;
using TheCircussyOne.Rules;
using TheCircussyOne.Visuals;

public sealed class DamageNumberViewTests
{
    [Test]
    public void ApplyFrameFacesTextFrontTowardCamera()
    {
        var cameraObject = new GameObject("Damage Number Test Camera");
        var camera = cameraObject.AddComponent<Camera>();
        camera.transform.position = new Vector3(0f, 3f, -8f);
        camera.transform.rotation = Quaternion.Euler(20f, 0f, 0f);

        var damageNumberObject = new GameObject("Damage Number");
        damageNumberObject.AddComponent<TextMeshPro>();
        var view = damageNumberObject.AddComponent<DamageNumberView>();
        var frame = new DamageNumberFrame(new Vector3(0f, 2f, 1f), 1f, Color.white, 0f);

        view.ApplyFrame(frame, camera);

        Vector3 expectedForward = (frame.Position - camera.transform.position).normalized;
        Assert.That(Vector3.Dot(damageNumberObject.transform.forward, expectedForward), Is.GreaterThan(0.999f));

        Object.DestroyImmediate(damageNumberObject);
        Object.DestroyImmediate(cameraObject);
    }
}
