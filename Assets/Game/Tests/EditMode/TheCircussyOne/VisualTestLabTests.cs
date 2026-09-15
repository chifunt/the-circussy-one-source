using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using TheCircussyOne.VisualTests;
using TheCircussyOne.VisualTests.Editor;

public sealed class VisualTestLabTests
{
    [Test]
    public void ScenarioValidationRejectsCaptureOutsideDuration()
    {
        var scenario = ScriptableObject.CreateInstance<VisualTestScenario>();
        scenario.scenarioId = "Invalid_Test";
        scenario.durationSeconds = 1f;
        scenario.captureTimes.Add(1.5f);

        Assert.That(scenario.IsValid(out string message), Is.False);
        Assert.That(message, Does.Contain("outside"));

        Object.DestroyImmediate(scenario);
    }

    [Test]
    public void GeneratedRootResetPreservesManualOverrides()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var manualRoot = new GameObject(VisualTestLabBuilder.ManualRootName);
        var manualChild = new GameObject("Hand Tuned Marker");
        manualChild.transform.SetParent(manualRoot.transform, false);
        var oldGenerated = new GameObject(VisualTestLabBuilder.GeneratedRootName);
        new GameObject("Old Generated Marker").transform.SetParent(oldGenerated.transform, false);

        VisualTestLabBuilder.EnsureGeneratedAndManualRootsForTests();

        Assert.That(GameObject.Find(VisualTestLabBuilder.ManualRootName), Is.SameAs(manualRoot));
        Assert.That(GameObject.Find("Hand Tuned Marker"), Is.SameAs(manualChild));
        Assert.That(GameObject.Find(VisualTestLabBuilder.GeneratedRootName), Is.Not.Null);
        Assert.That(GameObject.Find("Old Generated Marker"), Is.Null);
    }

    [Test]
    public void BuiltInScenarioDefaultsAreValid()
    {
        for (int i = 0; i < VisualTestLabBuilder.BuiltInScenarioIds.Count; i++)
        {
            VisualTestScenario scenario = VisualTestLabBuilder.EnsureScenarioAsset(VisualTestLabBuilder.BuiltInScenarioIds[i]);
            Assert.That(scenario.IsValid(out string message), Is.True, message);
            Assert.That(scenario.captureTimes, Is.Not.Empty);
            Assert.That(scenario.events, Is.Not.Empty);
        }
    }
}
