using System.Collections;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using TheCircussyOne.Config;
using TheCircussyOne.VisualTests;
using TheCircussyOne.Visuals;

public sealed class VisualTestDirectorPlayModeTests
{
    [UnityTest]
    public IEnumerator SeekSpawnsDamageNumberAtKnownScenarioTime()
    {
        var scenario = ScriptableObject.CreateInstance<VisualTestScenario>();
        scenario.scenarioId = "PlayMode_DamageNumber_Test";
        scenario.durationSeconds = 1f;
        scenario.events.Add(new VisualTestTimelineEvent
        {
            time = 0.25f,
            type = VisualTestEventType.TriggerDamageNumber,
            position = new Vector3(0f, 1.2f, 0f),
            amount = 42
        });

        var damageConfig = ScriptableObject.CreateInstance<DamageFeedbackVisualConfig>();
        damageConfig.lifetime = 0.75f;
        damageConfig.baseWorldScale = 0.85f;
        damageConfig.startScale = 1f;
        damageConfig.popScale = 1.35f;
        damageConfig.endScale = 0.55f;
        damageConfig.popDurationNormalized = 0.18f;
        damageConfig.fadeStartNormalized = 0.34f;
        damageConfig.numberColor = Color.yellow;

        GameObject root = new("PlayMode Visual Test Root");
        CameraView cameraView = CreateCamera();
        DamageNumberView prefab = CreateDamageNumberPrefab();
        GameConfig gameConfig = ScriptableObject.CreateInstance<GameConfig>();
        ActorMotionVisualConfig actorMotionConfig = ScriptableObject.CreateInstance<ActorMotionVisualConfig>();

        var director = root.AddComponent<VisualTestDirector>();
        director.ConfigureForTests(
            scenario,
            gameConfig,
            damageConfig,
            actorMotionConfig,
            cameraView,
            prefab,
            root.transform);

        yield return null;

        director.Seek(0.30f);
        yield return null;

        Assert.That(director.DamageNumberCount, Is.EqualTo(1));

        Object.Destroy(root);
        Object.Destroy(cameraView.gameObject);
        Object.Destroy(prefab.gameObject);
        Object.Destroy(scenario);
        Object.Destroy(damageConfig);
        Object.Destroy(actorMotionConfig);
        Object.Destroy(gameConfig);
    }

    [UnityTest]
    public IEnumerator ShowroomDirectorAnimatesConfiguredTargets()
    {
        var root = new GameObject("Showroom Director Test Root");
        var cameraView = CreateCamera();
        var labelObject = new GameObject("Station Label");
        labelObject.transform.position = Vector3.forward * 2f;
        labelObject.AddComponent<TextMeshPro>();
        ShowroomStationLabel label = labelObject.AddComponent<ShowroomStationLabel>();
        label.Configure("Station", "Subtitle", cameraView.transform);

        var squashTarget = GameObject.CreatePrimitive(PrimitiveType.Capsule).transform;
        squashTarget.name = "Squash Target";
        squashTarget.SetParent(root.transform, false);

        ShowroomDirector director = root.AddComponent<ShowroomDirector>();
        director.Configure(
            cameraView.transform,
            new[] { label },
            new[] { squashTarget },
            new Transform[0],
            new Transform[0],
            new ParticleSystem[0]);

        yield return null;
        yield return null;

        Assert.That(squashTarget.localScale.y, Is.Not.EqualTo(1f).Within(0.0001f));

        Object.Destroy(root);
        Object.Destroy(cameraView.gameObject);
        Object.Destroy(labelObject);
    }

    [UnityTest]
    public IEnumerator EditorSeekWrapperWorksInPlayModeSmoke()
    {
        var scenario = ScriptableObject.CreateInstance<VisualTestScenario>();
        scenario.scenarioId = "PlayMode_EditorSeekWrapper_Test";
        scenario.durationSeconds = 1f;
        scenario.events.Add(new VisualTestTimelineEvent
        {
            time = 0.2f,
            type = VisualTestEventType.TriggerDamageNumber,
            position = new Vector3(0f, 1.2f, 0f),
            amount = 7
        });

        var damageConfig = ScriptableObject.CreateInstance<DamageFeedbackVisualConfig>();
        damageConfig.lifetime = 0.75f;
        damageConfig.baseWorldScale = 0.85f;
        damageConfig.startScale = 1f;
        damageConfig.popScale = 1.35f;
        damageConfig.endScale = 0.55f;
        damageConfig.popDurationNormalized = 0.18f;
        damageConfig.fadeStartNormalized = 0.34f;
        damageConfig.numberColor = Color.yellow;

        GameObject root = new("PlayMode Editor Seek Root");
        CameraView cameraView = CreateCamera();
        DamageNumberView prefab = CreateDamageNumberPrefab();
        GameConfig gameConfig = ScriptableObject.CreateInstance<GameConfig>();
        ActorMotionVisualConfig actorMotionConfig = ScriptableObject.CreateInstance<ActorMotionVisualConfig>();

        var director = root.AddComponent<VisualTestDirector>();
        director.ConfigureForTests(
            scenario,
            gameConfig,
            damageConfig,
            actorMotionConfig,
            cameraView,
            prefab,
            root.transform);

        yield return null;

        director.EditorSeek(0.25f);

        Assert.That(director.ElapsedSeconds, Is.EqualTo(0.25f).Within(0.001f));
        Assert.That(director.DamageNumberCount, Is.EqualTo(1));

        Object.Destroy(root);
        Object.Destroy(cameraView.gameObject);
        Object.Destroy(prefab.gameObject);
        Object.Destroy(scenario);
        Object.Destroy(damageConfig);
        Object.Destroy(actorMotionConfig);
        Object.Destroy(gameConfig);
    }

    [UnityTest]
    public IEnumerator PlaybackControlsPauseSeekRestartAndLoopWithoutEditorOrchestration()
    {
        var scenario = ScriptableObject.CreateInstance<VisualTestScenario>();
        scenario.scenarioId = "Runtime_Almanac_Playback_Test";
        scenario.displayName = "Runtime Almanac Playback";
        scenario.durationSeconds = 1f;

        GameObject root = new("Runtime Almanac Playback Root");
        CameraView cameraView = CreateCamera();
        DamageNumberView prefab = CreateDamageNumberPrefab();
        GameConfig gameConfig = ScriptableObject.CreateInstance<GameConfig>();
        DamageFeedbackVisualConfig damageConfig = ScriptableObject.CreateInstance<DamageFeedbackVisualConfig>();
        ActorMotionVisualConfig actorMotionConfig = ScriptableObject.CreateInstance<ActorMotionVisualConfig>();
        var director = root.AddComponent<VisualTestDirector>();
        director.ConfigureForTests(
            scenario,
            gameConfig,
            damageConfig,
            actorMotionConfig,
            cameraView,
            prefab,
            root.transform);

        yield return null;

        director.Pause();
        director.Seek(0.75f);
        Assert.That(director.IsPlaying, Is.False);
        Assert.That(director.ElapsedSeconds, Is.EqualTo(0.75f).Within(0.001f));

        director.SetLoopPlayback(false);
        Assert.That(director.LoopPlayback, Is.False);
        director.Restart();
        Assert.That(director.IsPlaying, Is.True);
        Assert.That(director.ElapsedSeconds, Is.Zero.Within(0.001f));

        Object.Destroy(root);
        Object.Destroy(cameraView.gameObject);
        Object.Destroy(prefab.gameObject);
        Object.Destroy(scenario);
        Object.Destroy(damageConfig);
        Object.Destroy(actorMotionConfig);
        Object.Destroy(gameConfig);
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
