using System.IO;
using System.Linq;
using NUnit.Framework;
using TMPro;
using TheCircussyOne.Config;
using TheCircussyOne.VisualTests;
using TheCircussyOne.VisualTests.Editor;
using TheCircussyOne.Visuals;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class VisualTestScenarioBrowserTests
{
    [Test]
    public void BrowserModelListsBuiltInScenarioIds()
    {
        string[] ids = VisualTestScenarioBrowserModel.BuildEntries()
            .Select(entry => entry.ScenarioId)
            .ToArray();

        Assert.That(ids, Is.EqualTo(VisualTestLabBuilder.BuiltInScenarioIds));
    }

    [Test]
    public void BrowserModelLoadsScenarioMetadata()
    {
        foreach (VisualTestScenarioBrowserEntry entry in VisualTestScenarioBrowserModel.BuildEntries())
        {
            Assert.That(entry.Scenario, Is.Not.Null, entry.ScenarioId);
            Assert.That(entry.Description, Is.Not.Null.And.Not.Empty, entry.ScenarioId);
            Assert.That(entry.DurationSeconds, Is.GreaterThan(0f), entry.ScenarioId);
            Assert.That(entry.ScenePath, Is.EqualTo($"{VisualTestLabBuilder.SceneFolder}/{entry.ScenarioId}.unity"));
            Assert.That(entry.ScenarioPath, Is.EqualTo($"{VisualTestLabBuilder.ScenarioFolder}/{entry.ScenarioId}.asset"));
        }
    }

    [Test]
    public void AlmanacModelUsesProfileOrderAndNamedReviewBeats()
    {
        VisualRehearsalProfile profile = VisualRehearsalProfileFactory.EnsureDefaultProfile();

        VisualAlmanacEntry first = VisualAlmanacModel.BuildEntries(profile)[0];
        VisualAlmanacEntry arrival = VisualAlmanacModel.BuildEntries(profile)
            .Single(entry => entry.SubjectId == "enemy-arrival");

        Assert.That(first.SubjectId, Is.EqualTo(profile.subjects[0].subjectId));
        Assert.That(arrival.IsProfileBound, Is.True);
        Assert.That(arrival.ReviewBeats.Select(beat => beat.Label),
            Is.EqualTo(new[] { "Warning", "Arrival Cue", "Enemy Revealed" }));
        Assert.That(arrival.ReviewBeats.All(beat => beat.IsAuthored), Is.True);
    }

    [Test]
    public void AlmanacModelFallsBackToScenarioCaptureTimesForUncataloguedSubject()
    {
        VisualTestScenario scenario = ScriptableObject.CreateInstance<VisualTestScenario>();
        scenario.scenarioId = "Fallback_Test";
        scenario.captureTimes.Add(0.4f);
        scenario.captureTimes.Add(0.1f);
        var entry = new VisualAlmanacEntry("fallback", scenario, null);

        Assert.That(entry.ReviewBeats.Select(beat => beat.Seconds), Is.EqualTo(new[] { 0.1f, 0.4f }));
        Assert.That(entry.ReviewBeats.All(beat => !beat.IsAuthored), Is.True);

        Object.DestroyImmediate(scenario);
    }

    [Test]
    public void AlmanacPlaybackCanLoopOrStopAtScenarioEnd()
    {
        float looped = VisualAlmanacPlayback.Advance(0.9f, 0.3f, 1f, true, out bool loopCompleted);
        float stopped = VisualAlmanacPlayback.Advance(0.9f, 0.3f, 1f, false, out bool stopCompleted);

        Assert.That(looped, Is.EqualTo(0.2f).Within(0.0001f));
        Assert.That(loopCompleted, Is.False);
        Assert.That(stopped, Is.EqualTo(1f));
        Assert.That(stopCompleted, Is.True);
    }

    [Test]
    public void EditorSeekMovesDirectorToKnownTimestamp()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        VisualTestScenario scenario = ScriptableObject.CreateInstance<VisualTestScenario>();
        scenario.scenarioId = "EditorSeek_Test";
        scenario.durationSeconds = 1f;
        scenario.events.Add(new VisualTestTimelineEvent
        {
            time = 0f,
            type = VisualTestEventType.SpawnPlayer,
            position = Vector3.zero
        });
        scenario.events.Add(new VisualTestTimelineEvent
        {
            time = 0.2f,
            type = VisualTestEventType.TriggerDamageNumber,
            position = new Vector3(0f, 1f, 0f),
            amount = 12
        });

        DamageFeedbackVisualConfig damageConfig = ScriptableObject.CreateInstance<DamageFeedbackVisualConfig>();
        damageConfig.lifetime = 0.75f;
        damageConfig.baseWorldScale = 0.85f;
        damageConfig.startScale = 1f;
        damageConfig.popScale = 1.35f;
        damageConfig.endScale = 0.55f;
        damageConfig.popDurationNormalized = 0.18f;
        damageConfig.fadeStartNormalized = 0.34f;
        damageConfig.numberColor = Color.yellow;

        GameObject root = new("Editor Seek Root");
        CameraView cameraView = CreateCamera();
        DamageNumberView prefab = CreateDamageNumberPrefab();
        GameConfig gameConfig = ScriptableObject.CreateInstance<GameConfig>();
        ActorMotionVisualConfig actorMotionConfig = ScriptableObject.CreateInstance<ActorMotionVisualConfig>();

        VisualTestDirector director = root.AddComponent<VisualTestDirector>();
        director.ConfigureForTests(
            scenario,
            gameConfig,
            damageConfig,
            actorMotionConfig,
            cameraView,
            prefab,
            root.transform);

        director.EditorSeek(0.25f);

        Assert.That(director.ElapsedSeconds, Is.EqualTo(0.25f).Within(0.001f));
        Assert.That(director.DamageNumberCount, Is.EqualTo(1));

        Object.DestroyImmediate(root);
        Object.DestroyImmediate(cameraView.gameObject);
        Object.DestroyImmediate(prefab.gameObject);
        Object.DestroyImmediate(scenario);
        Object.DestroyImmediate(damageConfig);
        Object.DestroyImmediate(actorMotionConfig);
        Object.DestroyImmediate(gameConfig);
    }

    [Test]
    public void CaptureUtilityBuildsDeterministicIgnoredPathWithoutWriting()
    {
        string path = VisualTestCaptureUtility.BuildCapturePath("BrowserPath_Test", 0.25f, VisualTestLabBuilder.CaptureFolder);

        Assert.That(path, Is.EqualTo("Assets/Game/VisualTests/GeneratedCaptures/BrowserPath_Test_0.25.png"));
        Assert.That(File.Exists(path), Is.False);
        Assert.That(VisualTestCaptureUtility.IsCaptureFolderIgnoredForTests(), Is.True);
    }

    [Test]
    public void EditorSeekUsesNonSavedPreviewWithoutDirtyingScenarioScene()
    {
        Scene scene = EditorSceneManager.OpenScene(
            VisualTestScenarioBrowserModel.ScenePath("DamageNumbers_Test"),
            OpenSceneMode.Single);
        Assert.That(scene.isDirty, Is.False, "Scenario scene should open clean before preview scrubbing.");

        VisualTestDirector director = Object.FindFirstObjectByType<VisualTestDirector>();
        Assert.That(director, Is.Not.Null);

        director.EditorSeek(0.25f);

        Assert.That(director.HasEditorPreview, Is.True);
        Assert.That(director.DamageNumberCount, Is.GreaterThanOrEqualTo(1));
        Assert.That(scene.isDirty, Is.False, "Editor preview scrubbing should not dirty the saved scenario scene.");

        director.ClearEditorPreview();

        Assert.That(director.HasEditorPreview, Is.False);
        Assert.That(scene.isDirty, Is.False, "Clearing preview should not dirty the saved scenario scene.");
    }

    [Test]
    public void ConfigHubAndDoctorCompileWithScenarioBrowserHooks()
    {
        Assert.That(typeof(VisualTestLabScenarioBrowserWindow), Is.Not.Null);
        Assert.That(typeof(VisualTestScenarioBrowserModel), Is.Not.Null);
        Assert.That(typeof(VisualTestCaptureUtility), Is.Not.Null);
        Assert.That(typeof(TheCircussyOneConfigHub), Is.Not.Null);
        Assert.That(typeof(TheCircussyOneAuthoringDoctorWindow), Is.Not.Null);
        Assert.That(AuthoringDoctorRunner.RunAllChecks().Any(result => result.Id == "scenario-browser.ok"), Is.True);
    }

    private static CameraView CreateCamera()
    {
        var cameraObject = new GameObject("Visual Test Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.AddComponent<Camera>();
        return cameraObject.AddComponent<CameraView>();
    }

    private static DamageNumberView CreateDamageNumberPrefab()
    {
        var damageNumberObject = new GameObject("Damage Number Prefab");
        var text = damageNumberObject.AddComponent<TextMeshPro>();
        text.font = TMP_Settings.defaultFontAsset;
        text.text = "0";
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        damageNumberObject.SetActive(false);
        return damageNumberObject.AddComponent<DamageNumberView>();
    }
}
