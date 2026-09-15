using System.Collections;
using System.Linq;
using NUnit.Framework;
using TheCircussyOne.Content;
using TheCircussyOne.VisualTests;
using TheCircussyOne.VisualTests.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;

public sealed class VisualRehearsalProfileTests
{
    [Test]
    public void ValidProfileResolvesHumanAuthoredPresentationCoordinates()
    {
        VisualRehearsalProfile profile = CreateValidProfile();

        Assert.That(profile.IsValid(out string message), Is.True, message);
        Assert.That(profile.TryGetSubject("performer-idle", out VisualRehearsalScenarioBinding subject), Is.True);
        Assert.That(subject.scenarioId, Is.EqualTo("PlayerMovement_Test"));
        Assert.That(profile.TryGetSubjectForScenario("PlayerMovement_Test", out VisualRehearsalScenarioBinding scenarioSubject), Is.True);
        Assert.That(scenarioSubject.subjectId, Is.EqualTo("performer-idle"));
        Assert.That(profile.TryGetBackground("neutral-diagnostic", out _), Is.True);
        Assert.That(profile.TryGetLighting("intended-game", out _), Is.True);
        Assert.That(profile.TryGetQuality("project-high", out _), Is.True);
        Assert.That(profile.TryGetContent("production-default", out _), Is.True);

        Object.DestroyImmediate(profile);
    }

    [Test]
    public void ValidationRejectsDuplicatePresentationIds()
    {
        VisualRehearsalProfile profile = CreateValidProfile();
        profile.lighting.Add(new VisualRehearsalLightingPreset { id = "intended-game" });

        Assert.That(profile.IsValid(out string message), Is.False);
        Assert.That(message, Does.Contain("Duplicate lighting id"));

        Object.DestroyImmediate(profile);
    }

    [Test]
    public void ValidationRejectsUnboundedMotionCapture()
    {
        VisualRehearsalProfile profile = CreateValidProfile();
        VisualRehearsalMotionSettings motion = profile.subjects[0].motion;
        motion.enabled = true;
        motion.durationSeconds = 10f;
        motion.frameRate = 60f;
        motion.maxFrames = 120;

        Assert.That(profile.IsValid(out string message), Is.False);
        Assert.That(message, Does.Contain("600 motion frames"));

        Object.DestroyImmediate(profile);
    }

    [Test]
    public void ValidationRejectsMissingMotionSettings()
    {
        VisualRehearsalProfile profile = CreateValidProfile();
        profile.subjects[0].motion = null;

        Assert.That(profile.IsValid(out string message), Is.False);
        Assert.That(message, Does.Contain("requires motion settings"));

        Object.DestroyImmediate(profile);
    }

    [Test]
    public void ValidationRejectsIncompleteNeutralLightingRig()
    {
        VisualRehearsalProfile profile = CreateValidProfile();
        profile.lighting[0].mode = VisualRehearsalLightingMode.NeutralRig;
        profile.lighting[0].fill = null;

        Assert.That(profile.IsValid(out string message), Is.False);
        Assert.That(message, Does.Contain("requires key and fill settings"));

        Object.DestroyImmediate(profile);
    }

    [Test]
    public void ValidationRejectsNonFiniteHumanAuthoredCoordinates()
    {
        VisualRehearsalProfile profile = CreateValidProfile();
        profile.subjects[0].focusPoint = new Vector3(0f, float.NaN, 0f);

        Assert.That(profile.IsValid(out string message), Is.False);
        Assert.That(message, Does.Contain("invalid focus, sample-time, or pre-roll"));

        Object.DestroyImmediate(profile);
    }

    [Test]
    public void DisabledMotionPreviewDoesNotRequireFfmpeg()
    {
        VisualRehearsalProfile profile = CreateValidProfile();
        profile.captureMotionPreviews = false;
        profile.ffmpegExecutable = string.Empty;
        profile.subjects[0].motion.enabled = true;

        Assert.That(profile.IsValid(out string message), Is.True, message);

        Object.DestroyImmediate(profile);
    }

    [Test]
    public void DefaultMotionCaptureDeclaresDeterministicStateSeekSemantics()
    {
        VisualRehearsalProfile profile = ScriptableObject.CreateInstance<VisualRehearsalProfile>();
        try
        {
            VisualRehearsalProfileFactory.ConfigureNewProfile(profile);

            Assert.That(profile.subjects, Is.Not.Empty);
            Assert.That(
                profile.subjects[0].motion.samplingMode,
                Is.EqualTo(VisualRehearsalMotionSamplingMode.DeterministicStateSeek));
            Assert.That(VisualRehearsalProfile.CurrentAdapterVersion, Is.EqualTo("0.4.0"));
        }
        finally
        {
            Object.DestroyImmediate(profile);
        }
    }

    [Test]
    public void DefaultProfileSeparatesDiagnosticAndIntendedPresentation()
    {
        VisualRehearsalProfile profile = ScriptableObject.CreateInstance<VisualRehearsalProfile>();

        VisualRehearsalProfileFactory.ConfigureNewProfile(profile);

        Assert.That(profile.IsValid(out string message), Is.True, message);
        Assert.That(profile.TryGetBackground("neutral-diagnostic", out VisualRehearsalBackgroundPreset diagnosticBackground), Is.True);
        Assert.That(diagnosticBackground.mode, Is.EqualTo(VisualRehearsalBackgroundMode.SolidColor));
        Assert.That(profile.TryGetLighting("neutral-diagnostic", out VisualRehearsalLightingPreset diagnosticLighting), Is.True);
        Assert.That(diagnosticLighting.mode, Is.EqualTo(VisualRehearsalLightingMode.NeutralRig));
        Assert.That(profile.TryGetBackground("intended-game", out VisualRehearsalBackgroundPreset intendedBackground), Is.True);
        Assert.That(intendedBackground.mode, Is.EqualTo(VisualRehearsalBackgroundMode.PreserveScene));
        Assert.That(profile.subjects.Exists(binding => binding.subjectId == "performer-movement" && binding.motion.enabled), Is.True);
        Assert.That(profile.subjects.Exists(binding => binding.subjectId == "hud-progression" && binding.includeHud), Is.True);
        Assert.That(profile.TryGetContent("production-default", out VisualRehearsalContentPreset content), Is.True);
        Assert.That(content.performer, Is.Not.Null);
        Assert.That(content.enemy, Is.Not.Null);
        Assert.That(content.projectileVisualWeapons, Has.Length.EqualTo(5));
        Assert.That(content.requireSupportedMaterials, Is.True);
        Assert.That(
            profile.subjects.Exists(binding => binding.subjectId == "performer-jump"
                && binding.cameraMode == VisualRehearsalCameraMode.PlanOrbit
                && binding.focusPoint == new Vector3(0f, 2.1f, 0.9f)
                && binding.focusRadius == 2.8f),
            Is.True);
        Assert.That(
            profile.subjects.Exists(binding => binding.subjectId == "camera-obstruction"
                && binding.cameraMode == VisualRehearsalCameraMode.AuthoredScenario),
            Is.True);
        Assert.That(
            profile.subjects.Find(binding => binding.subjectId == "enemy-arrival").reviewBeats
                .Select(beat => beat.id),
            Is.EqualTo(new[] { "warning", "arrival-cue", "enemy-revealed" }));
        Assert.That(
            profile.subjects.Find(binding => binding.subjectId == "performer-jump").reviewBeats
                .Select(beat => beat.id),
            Is.EqualTo(new[] { "takeoff", "apex", "landing" }));

        Object.DestroyImmediate(profile);
    }

    [Test]
    public void ValidationRejectsDuplicateReviewBeatIds()
    {
        VisualRehearsalProfile profile = CreateValidProfile();
        profile.subjects[0].reviewBeats.Add(new VisualAlmanacReviewBeat
        {
            id = "pose",
            label = "Pose",
            purpose = "Inspect the pose.",
            seconds = 0.1f
        });
        profile.subjects[0].reviewBeats.Add(new VisualAlmanacReviewBeat
        {
            id = "pose",
            label = "Second Pose",
            purpose = "Inspect the second pose.",
            seconds = 0.2f
        });

        Assert.That(profile.IsValid(out string message), Is.False);
        Assert.That(message, Does.Contain("duplicate review beat id 'pose'"));

        Object.DestroyImmediate(profile);
    }

    [UnityTest]
    public IEnumerator CaptureSessionRetainsAuthoredContentAcrossSceneAssetCollection()
    {
        SceneSetup[] initialSceneSetup = EditorSceneManager.GetSceneManagerSetup();
        VisualRehearsalCaptureSession session = null;
        try
        {
            VisualRehearsalProfile profile = AssetDatabase.LoadAssetAtPath<VisualRehearsalProfile>(
                VisualRehearsalProfileFactory.DefaultProfilePath);
            Assert.That(profile, Is.Not.Null);
            AssertAuthoredContent(profile, "before opening a capture scene");
            session = new VisualRehearsalCaptureSession(profile);
            Assert.That(session.Profile, Is.Not.SameAs(profile));
            Assert.That(AssetDatabase.GetAssetPath(session.Profile), Is.Empty);

            EditorSceneManager.OpenScene(
                VisualTestScenarioBrowserModel.ScenePath("DamageNumbers_Test"),
                OpenSceneMode.Single);
            yield return null;
            yield return null;
            yield return Resources.UnloadUnusedAssets();

            AssertAuthoredContent(session.Profile, "after opening a capture scene and collecting unused assets");
        }
        finally
        {
            session?.Dispose();
            if (initialSceneSetup.Length > 0)
            {
                EditorSceneManager.RestoreSceneManagerSetup(initialSceneSetup);
            }
            else
            {
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            }
        }
    }

    [Test]
    public void ValidationRejectsUnknownSubjectContentPreset()
    {
        VisualRehearsalProfile profile = CreateValidProfile();
        profile.subjects[0].contentPresetId = "missing";

        Assert.That(profile.IsValid(out string message), Is.False);
        Assert.That(message, Does.Contain("valid content preset id"));

        Object.DestroyImmediate(profile);
    }

    [Test]
    public void ValidationRejectsRequiredAuthoredContentWithoutDefinitions()
    {
        VisualRehearsalProfile profile = CreateValidProfile();
        profile.content[0].requireAuthoredActorModels = true;

        Assert.That(profile.IsValid(out string message), Is.False);
        Assert.That(message, Does.Contain("authored Performer definition"));

        Object.DestroyImmediate(profile);
    }

    [Test]
    public void ValidationIdentifiesDefinitionMissingItsAuthoredWorldPrefab()
    {
        VisualRehearsalProfile profile = CreateValidProfile();
        PerformerDefinition performer = ScriptableObject.CreateInstance<PerformerDefinition>();
        try
        {
            profile.content[0].requireAuthoredActorModels = true;
            profile.content[0].performer = performer;

            Assert.That(profile.IsValid(out string message), Is.False);
            Assert.That(message, Does.Contain("Performer"));
            Assert.That(message, Does.Contain("authored world prefab"));
        }
        finally
        {
            Object.DestroyImmediate(performer);
            Object.DestroyImmediate(profile);
        }
    }

    private static VisualRehearsalProfile CreateValidProfile()
    {
        VisualRehearsalProfile profile = ScriptableObject.CreateInstance<VisualRehearsalProfile>();
        profile.backgrounds.Add(new VisualRehearsalBackgroundPreset { id = "neutral-diagnostic" });
        profile.lighting.Add(new VisualRehearsalLightingPreset { id = "intended-game" });
        profile.quality.Add(new VisualRehearsalQualityPreset { id = "project-high" });
        profile.content.Add(new VisualRehearsalContentPreset
        {
            id = "production-default",
            requireAuthoredActorModels = false,
            requireProjectileDefinitions = false
        });
        profile.subjects.Add(new VisualRehearsalScenarioBinding
        {
            subjectId = "performer-idle",
            scenarioId = "PlayerMovement_Test"
        });
        return profile;
    }

    private static void AssertAuthoredContent(VisualRehearsalProfile profile, string phase)
    {
        Assert.That(
            profile.TryGetContent("production-default", out VisualRehearsalContentPreset content),
            Is.True,
            phase);
        Assert.That(content.performer, Is.Not.Null, $"Performer was missing {phase}.");
        Assert.That(content.performer.worldPrefab, Is.Not.Null, $"Performer prefab was missing {phase}.");
        Assert.That(content.enemy, Is.Not.Null, $"Enemy was missing {phase}.");
        Assert.That(content.enemy.worldPrefab, Is.Not.Null, $"Enemy prefab was missing {phase}.");
        Assert.That(content.IsValid(out string message), Is.True, $"{phase}: {message}");
    }
}
