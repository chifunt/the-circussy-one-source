using System.Linq;
using NUnit.Framework;
using TheCircussyOne.VisualTests;
using TheCircussyOne.VisualTests.Editor;
using UnityEditor.SceneManagement;
using UnityEngine;

public sealed class AuthoringShowroomTests
{
    [Test]
    public void GeneratedRootResetPreservesManualOverrides()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var manualRoot = new GameObject(VisualTestLabBuilder.ManualRootName);
        var manualChild = new GameObject("Hand Tuned Showroom Marker");
        manualChild.transform.SetParent(manualRoot.transform, false);
        var oldGenerated = new GameObject(VisualTestLabBuilder.GeneratedRootName);
        new GameObject("Old Showroom Generated Marker").transform.SetParent(oldGenerated.transform, false);

        AuthoringShowroomBuilder.GenerateContentsForTests();

        Assert.That(GameObject.Find(VisualTestLabBuilder.ManualRootName), Is.SameAs(manualRoot));
        Assert.That(GameObject.Find("Hand Tuned Showroom Marker"), Is.SameAs(manualChild));
        Assert.That(GameObject.Find(VisualTestLabBuilder.GeneratedRootName), Is.Not.Null);
        Assert.That(GameObject.Find("Old Showroom Generated Marker"), Is.Null);
    }

    [Test]
    public void GeneratedContentContainsRequiredStationsAndLabels()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        AuthoringShowroomBuilder.GenerateContentsForTests();

        Assert.That(GameObject.Find(AuthoringShowroomBuilder.StationsRootName), Is.Not.Null);
        Assert.That(Object.FindFirstObjectByType<ShowroomDirector>(), Is.Not.Null);

        foreach (string stationName in AuthoringShowroomBuilder.RequiredStationNames)
        {
            Assert.That(GameObject.Find(stationName), Is.Not.Null, stationName);
            ShowroomStationLabel label = Object.FindObjectsByType<ShowroomStationLabel>(FindObjectsSortMode.None)
                .FirstOrDefault(candidate => candidate.name == stationName + " Label");
            Assert.That(label, Is.Not.Null, stationName);
            Assert.That(label.Text, Does.Contain(stationName), stationName);
        }
    }

    [Test]
    public void GeneratedContentKeepsAuthoringRoots()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        AuthoringShowroomBuilder.GenerateContentsForTests();

        Assert.That(GameObject.Find(VisualTestLabBuilder.GeneratedRootName), Is.Not.Null);
        Assert.That(GameObject.Find(VisualTestLabBuilder.ManualRootName), Is.Not.Null);
        Assert.That(GameObject.Find(AuthoringShowroomBuilder.LightingRootName), Is.Not.Null);
        Assert.That(GameObject.Find(AuthoringShowroomBuilder.CameraRootName), Is.Not.Null);
        Assert.That(GameObject.Find(AuthoringShowroomBuilder.LabelsRootName), Is.Not.Null);
    }

    [Test]
    public void ManifestIncludesAuthoringShowroomScene()
    {
        GeneratedAssetManifestEntry? entry = TheCircussyOneGeneratedAssetManifest.FindByPath(AuthoringShowroomBuilder.ScenePath);

        Assert.That(entry.HasValue, Is.True);
        Assert.That(entry.Value.Category, Is.EqualTo(GeneratedAssetCategory.VisualTestScene));
        Assert.That(entry.Value.Owner, Is.EqualTo(GeneratedAssetOwner.VisualTestLab));
        Assert.That(entry.Value.SafeAction, Is.EqualTo(GeneratedAssetSafeAction.GenerateAuthoringShowroom));
    }

    [Test]
    public void DoctorManifestEvaluationUsesExplicitShowroomFix()
    {
        var entry = new GeneratedAssetManifestEntry(
            "Assets/Game/VisualTests/Scenes/__MissingAuthoringShowroom_Test.unity",
            GeneratedAssetCategory.VisualTestScene,
            GeneratedAssetOwner.VisualTestLab,
            TheCircussyOne.Config.ConfigWorkflowKind.ProjectedVisual,
            GeneratedAssetSafeAction.GenerateAuthoringShowroom,
            "Missing Authoring Showroom Test");

        AuthoringCheckResult result = AuthoringDoctorRunner.EvaluateManifestEntryForTests(entry);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.HasFix, Is.True);
        Assert.That(result.FixLabel, Is.EqualTo("Generate Authoring Showroom"));
    }
}
