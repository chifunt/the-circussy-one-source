using NUnit.Framework;
using UnityEngine;
using TheCircussyOne.Config;
using TheCircussyOne.Runtime;
using TheCircussyOne.Visuals;

public sealed class FeedbackServiceTests
{
    [TearDown]
    public void TearDown()
    {
        TheCircussyOneTestObjects.DestroyAll();
    }

    [Test]
    public void PlayerDamageStartsCameraRelativeShake()
    {
        GameConfig gameConfig = TheCircussyOneTestObjects.CreateConfig();
        DamageFeedbackVisualConfig damageConfig = TheCircussyOneTestObjects.CreateDamageFeedbackConfig();
        ActorMotionVisualConfig motionConfig = TheCircussyOneTestObjects.CreateActorMotionVisualConfig();
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(gameConfig);
        var cameraObject = TheCircussyOneTestObjects.CreateRoot("Feedback Camera");
        cameraObject.AddComponent<Camera>();
        CameraView camera = cameraObject.AddComponent<CameraView>();
        var feedback = new FeedbackService(gameConfig, damageConfig, motionConfig, player, camera, null, null, null);

        feedback.PlayPlayerDamage();

        Assert.That(camera.HasActiveRelativeShake, Is.True);

        Object.DestroyImmediate(gameConfig);
        Object.DestroyImmediate(damageConfig);
        Object.DestroyImmediate(motionConfig);
    }
}
