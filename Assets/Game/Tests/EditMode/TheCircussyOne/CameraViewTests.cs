using NUnit.Framework;
using UnityEngine;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;
using TheCircussyOne.Visuals;

public sealed class CameraViewTests
{
    private GameObject _cameraObject;

    [TearDown]
    public void TearDown()
    {
        if (_cameraObject != null)
        {
            Object.DestroyImmediate(_cameraObject);
        }
    }

    [Test]
    public void RelativeShakeDoesNotBecomeFollowBaseline()
    {
        CameraView cameraView = CreateCameraView();
        DamageFeedbackVisualConfig config = TheCircussyOneTestObjects.CreateDamageFeedbackConfig();
        config.playerDamageCameraShakeSeconds = 0.25f;
        config.playerDamageCameraShakePositionAmplitude = 0.4f;
        config.playerDamageCameraShakeRotationDegrees = 1.2f;
        var targetPose = new CameraOrbitPose(new Vector3(10f, 3f, -4f), Quaternion.Euler(18f, 35f, 0f));

        cameraView.PlayRelativeShake(config);
        cameraView.FollowOrbit(targetPose, 1000f, 0.01f, EaseSettings.Exponential);

        Assert.That(cameraView.HasActiveRelativeShake, Is.True);
        Assert.That(cameraView.UnshakenPosition.x, Is.EqualTo(targetPose.Position.x).Within(0.01f));
        Assert.That(cameraView.UnshakenPosition.y, Is.EqualTo(targetPose.Position.y).Within(0.01f));
        Assert.That(cameraView.UnshakenPosition.z, Is.EqualTo(targetPose.Position.z).Within(0.01f));

        cameraView.FollowOrbit(targetPose, 1000f, 1f, EaseSettings.Exponential);

        Assert.That(cameraView.HasActiveRelativeShake, Is.False);
        Assert.That(cameraView.transform.position.x, Is.EqualTo(targetPose.Position.x).Within(0.01f));
        Assert.That(cameraView.transform.position.y, Is.EqualTo(targetPose.Position.y).Within(0.01f));
        Assert.That(cameraView.transform.position.z, Is.EqualTo(targetPose.Position.z).Within(0.01f));

        Object.DestroyImmediate(config);
    }

    [Test]
    public void DisabledRelativeShakeDoesNotStart()
    {
        CameraView cameraView = CreateCameraView();
        DamageFeedbackVisualConfig config = TheCircussyOneTestObjects.CreateDamageFeedbackConfig();
        config.playerDamageCameraShakeEnabled = false;

        cameraView.PlayRelativeShake(config);

        Assert.That(cameraView.HasActiveRelativeShake, Is.False);

        Object.DestroyImmediate(config);
    }

    [Test]
    public void ApplyLensUsesConfiguredFarClipPlane()
    {
        CameraView cameraView = CreateCameraView();
        CameraConfig config = ScriptableObject.CreateInstance<CameraConfig>();
        config.farClipPlane = 1800f;

        cameraView.ApplyLens(config);

        Assert.That(cameraView.Camera.farClipPlane, Is.EqualTo(1800f));

        Object.DestroyImmediate(config);
    }

    private CameraView CreateCameraView()
    {
        _cameraObject = new GameObject("CameraViewTests");
        _cameraObject.AddComponent<Camera>();
        return _cameraObject.AddComponent<CameraView>();
    }
}
