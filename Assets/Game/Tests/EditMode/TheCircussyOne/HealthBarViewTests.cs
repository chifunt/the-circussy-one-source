using System.Reflection;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using TheCircussyOne.Visuals;

public sealed class HealthBarViewTests
{
    [TearDown]
    public void TearDown()
    {
        TheCircussyOneTestObjects.DestroyAll();
    }

    [Test]
    public void FillAlwaysRendersInFrontOfBackground()
    {
        GameObject root = TheCircussyOneTestObjects.CreateRoot("Health Bar");
        var healthBar = root.AddComponent<HealthBarView>();

        healthBar.SetNormalized(0.5f);

        MeshRenderer background = FindRenderer(root, "Health Background");
        MeshRenderer fill = FindRenderer(root, "Health Fill");

        Assert.That(background, Is.Not.Null);
        Assert.That(fill, Is.Not.Null);
        Assert.That(fill.sortingOrder, Is.GreaterThan(background.sortingOrder));
        Assert.That(healthBar.FillRenderQueue, Is.GreaterThan(healthBar.BackgroundRenderQueue));
        Assert.That(fill.transform.localPosition.z, Is.LessThan(background.transform.localPosition.z));
    }

    [Test]
    public void FadeOutInEditModeHidesBarAndSetsAlphaToZero()
    {
        GameObject root = TheCircussyOneTestObjects.CreateRoot("Health Bar");
        var healthBar = root.AddComponent<HealthBarView>();

        healthBar.SetNormalized(0f);
        healthBar.FadeOut(0.07f, TheCircussyOne.Rules.EaseSettings.OutQuad);

        Assert.That(healthBar.CurrentAlpha, Is.EqualTo(0f));
        Assert.That(healthBar.BackgroundDisplayColor.a, Is.EqualTo(0f));
        Assert.That(root.activeSelf, Is.False);
    }

    [Test]
    public void ResetVisibilityRestoresAlphaAndActivatesBar()
    {
        GameObject root = TheCircussyOneTestObjects.CreateRoot("Health Bar");
        var healthBar = root.AddComponent<HealthBarView>();

        healthBar.FadeOut(0.07f, TheCircussyOne.Rules.EaseSettings.OutQuad);
        healthBar.ResetVisibility();

        Assert.That(healthBar.CurrentAlpha, Is.EqualTo(1f));
        Assert.That(root.activeSelf, Is.True);
    }

    [Test]
    public void BillboardPrefersRegisteredCameraWhenSceneHasMultipleMainCameras()
    {
        GameObject wrongCameraObject = TheCircussyOneTestObjects.CreateRoot("Wrong Main Camera");
        wrongCameraObject.tag = "MainCamera";
        wrongCameraObject.transform.rotation = Quaternion.Euler(8f, 20f, 3f);
        wrongCameraObject.AddComponent<Camera>();

        GameObject gameplayCameraObject = TheCircussyOneTestObjects.CreateRoot("Gameplay Camera");
        gameplayCameraObject.tag = "MainCamera";
        gameplayCameraObject.transform.rotation = Quaternion.Euler(35f, 120f, 4f);
        gameplayCameraObject.AddComponent<Camera>();
        var cameraView = gameplayCameraObject.AddComponent<CameraView>();
        InvokeOnEnable(cameraView);

        GameObject root = TheCircussyOneTestObjects.CreateRoot("Health Bar");
        root.transform.rotation = Quaternion.identity;
        var healthBar = root.AddComponent<HealthBarView>();

        InvokeLateUpdate(healthBar);

        Assert.That(
            Quaternion.Angle(root.transform.rotation, gameplayCameraObject.transform.rotation),
            Is.LessThan(0.001f));
    }

    private static MeshRenderer FindRenderer(GameObject root, string rendererName)
    {
        return root.GetComponentsInChildren<MeshRenderer>(true)
            .FirstOrDefault(renderer => renderer.gameObject.name == rendererName);
    }

    private static void InvokeLateUpdate(HealthBarView healthBar)
    {
        typeof(HealthBarView)
            .GetMethod("LateUpdate", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.Invoke(healthBar, null);
    }

    private static void InvokeOnEnable(CameraView cameraView)
    {
        typeof(CameraView)
            .GetMethod("OnEnable", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.Invoke(cameraView, null);
    }
}
