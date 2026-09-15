using System.Collections.Generic;
using NUnit.Framework;
using TheCircussyOne.Config;
using TheCircussyOne.Runtime;
using UnityEngine;

public sealed class GameAudioServiceTests
{
    private GameAudioConfig config;
    private XpGainCounterVisualConfig xpCounterConfig;

    [TearDown]
    public void TearDown()
    {
        TheCircussyOneTestObjects.Destroy(config);
        TheCircussyOneTestObjects.Destroy(xpCounterConfig);
    }

    [Test]
    public void PlayDoesNotPostWhenBankLoadFails()
    {
        config = ScriptableObject.CreateInstance<GameAudioConfig>();
        config.EnsureWorkflowDefaults();
        var backend = new FakeWwiseRuntimeAudioBackend(loadResult: false);
        var service = new GameAudioService(config, null, null, backend);

        service.Play(GameAudioCue.AnnouncementAct);

        Assert.That(backend.LoadBankCount, Is.EqualTo(1));
        Assert.That(backend.PostEventCount, Is.Zero);
    }

    [Test]
    public void PlayPostsAfterBankLoadSucceeds()
    {
        config = ScriptableObject.CreateInstance<GameAudioConfig>();
        config.EnsureWorkflowDefaults();
        var backend = new FakeWwiseRuntimeAudioBackend(loadResult: true);
        var service = new GameAudioService(config, null, null, backend);

        service.Play(GameAudioCue.AnnouncementAct);

        Assert.That(backend.LoadBankCount, Is.EqualTo(1));
        Assert.That(backend.PostEventCount, Is.EqualTo(1));
        Assert.That(backend.LastPostedEvent, Is.EqualTo(config.announcementActEvent));
    }

    [Test]
    public void WeaponEventsAreConfiguredForEventBankPreload()
    {
        config = ScriptableObject.CreateInstance<GameAudioConfig>();
        config.EnsureWorkflowDefaults();

        Assert.That(config.EventName(GameAudioCue.JugglingBallThrow), Is.EqualTo(config.jugglingBallThrowEvent));
        Assert.That(config.EventName(GameAudioCue.JugglingBallHit), Is.EqualTo(config.jugglingBallHitEvent));
        Assert.That(config.EventName(GameAudioCue.CannonLaunch), Is.EqualTo(config.cannonLaunchEvent));
        Assert.That(config.EventName(GameAudioCue.CannonHit), Is.EqualTo(config.cannonHitEvent));
        Assert.That(config.EventName(GameAudioCue.KnifeFanThrow), Is.EqualTo(config.knifeFanThrowEvent));
        Assert.That(config.EventName(GameAudioCue.KnifeFanHit), Is.EqualTo(config.knifeFanHitEvent));
        Assert.That(config.EventName(GameAudioCue.SpotlightBoltLaunch), Is.EqualTo(config.spotlightBoltLaunchEvent));
        Assert.That(config.EventName(GameAudioCue.SpotlightBoltHit), Is.EqualTo(config.spotlightBoltHitEvent));
        Assert.That(config.EventName(GameAudioCue.FireHoopHit), Is.EqualTo(config.fireHoopHitEvent));

        IReadOnlyList<string> eventBanks = config.EventSoundBankNames();
        Assert.That(eventBanks, Does.Contain(config.jugglingBallThrowEvent));
        Assert.That(eventBanks, Does.Contain(config.jugglingBallHitEvent));
        Assert.That(eventBanks, Does.Contain(config.cannonLaunchEvent));
        Assert.That(eventBanks, Does.Contain(config.cannonHitEvent));
        Assert.That(eventBanks, Does.Contain(config.knifeFanThrowEvent));
        Assert.That(eventBanks, Does.Contain(config.knifeFanHitEvent));
        Assert.That(eventBanks, Does.Contain(config.spotlightBoltLaunchEvent));
        Assert.That(eventBanks, Does.Contain(config.spotlightBoltHitEvent));
        Assert.That(eventBanks, Does.Contain(config.fireHoopHitEvent));
    }

    [Test]
    public void InteractionLoopPitchRampsWithInteractionProgress()
    {
        config = ScriptableObject.CreateInstance<GameAudioConfig>();
        config.EnsureWorkflowDefaults();
        config.interactionHoldLoopStartPitchCents = -400f;
        config.interactionHoldLoopEndPitchCents = 600f;
        var backend = new FakeWwiseRuntimeAudioBackend(loadResult: true);
        var service = new GameAudioService(config, null, null, backend);
        object owner = new();

        service.StartLoop(GameAudioCue.InteractionHoldLoop, owner, Vector3.zero, initialProgress: 0f);
        service.UpdateLoop(GameAudioCue.InteractionHoldLoop, owner, Vector3.one, progress01: 0.5f);
        service.UpdateLoop(GameAudioCue.InteractionHoldLoop, owner, Vector3.one, progress01: 1f);

        float[] pitchValues = backend.RtpcValues(config.pitchRtpcName);
        Assert.That(pitchValues.Length, Is.EqualTo(3));
        Assert.That(pitchValues[0], Is.EqualTo(-400f).Within(0.001f));
        Assert.That(pitchValues[1], Is.EqualTo(100f).Within(0.001f));
        Assert.That(pitchValues[2], Is.EqualTo(600f).Within(0.001f));

        float[] progressValues = backend.RtpcValues(config.interactionProgressRtpcName);
        Assert.That(progressValues, Is.EqualTo(new[] { 0f, 0.5f, 1f }).Within(0.001f));
    }

    [Test]
    public void PlayerMovementLoopAppliesSpeedRtpcWithoutRuntimePitchRandomization()
    {
        config = ScriptableObject.CreateInstance<GameAudioConfig>();
        config.EnsureWorkflowDefaults();
        var backend = new FakeWwiseRuntimeAudioBackend(loadResult: true);
        var service = new GameAudioService(config, null, null, backend);

        service.StartLoop(GameAudioCue.PlayerMovementLoop, new object(), Vector3.zero, initialProgress: 0.4f);

        float[] pitchValues = backend.RtpcValues(config.pitchRtpcName);
        Assert.That(pitchValues, Is.Empty);

        float[] speedValues = backend.RtpcValues(config.playerMovementSpeedRtpcName);
        Assert.That(speedValues, Is.EqualTo(new[] { 0.4f }).Within(0.001f));
    }

    [Test]
    public void XpCollectPitchResetFollowsXpGainCounterLifetime()
    {
        config = ScriptableObject.CreateInstance<GameAudioConfig>();
        config.EnsureWorkflowDefaults();
        config.xpCollectPitchLiftStartCents = 0f;
        config.xpCollectPitchLiftIncrementCents = 30f;
        config.xpCollectPitchLiftMaxCents = 60f;
        config.xpCollectPitchResetSeconds = 99f;
        xpCounterConfig = ScriptableObject.CreateInstance<XpGainCounterVisualConfig>();
        xpCounterConfig.holdSeconds = 0.2f;
        xpCounterConfig.fadeSeconds = 0.3f;
        var time = new FakeGameTime { DeltaTime = 0.49f };
        var backend = new FakeWwiseRuntimeAudioBackend(loadResult: true);
        var service = new GameAudioService(config, time, null, backend, xpCounterConfig);

        service.PlayXpCollect(Vector3.zero);
        service.PlayXpCollect(Vector3.zero);
        service.Tick();
        service.PlayXpCollect(Vector3.zero);
        time.DeltaTime = 0.5f;
        service.Tick();
        service.PlayXpCollect(Vector3.zero);

        float[] pitchValues = backend.RtpcValues(config.pitchRtpcName);
        Assert.That(pitchValues.Length, Is.EqualTo(4));
        Assert.That(pitchValues[0], Is.EqualTo(0f).Within(0.001f));
        Assert.That(pitchValues[1], Is.EqualTo(30f).Within(0.001f));
        Assert.That(pitchValues[2], Is.EqualTo(60f).Within(0.001f));
        Assert.That(pitchValues[3], Is.EqualTo(0f).Within(0.001f));
    }

    [Test]
    public void OrdinaryOneShotDoesNotSendRuntimePitchRtpc()
    {
        config = ScriptableObject.CreateInstance<GameAudioConfig>();
        config.EnsureWorkflowDefaults();
        var backend = new FakeWwiseRuntimeAudioBackend(loadResult: true);
        var service = new GameAudioService(config, null, null, backend);

        service.PlayAt(GameAudioCue.EnemyHit, Vector3.zero);

        Assert.That(backend.PostEventCount, Is.EqualTo(1));
        Assert.That(backend.RtpcValues(config.pitchRtpcName), Is.Empty);
    }

    [Test]
    public void PauseTransitionStopsGameplayLoopsButKeepsUiSheenLoop()
    {
        config = ScriptableObject.CreateInstance<GameAudioConfig>();
        config.EnsureWorkflowDefaults();
        var pauseState = new RunPauseState();
        var backend = new FakeWwiseRuntimeAudioBackend(loadResult: true);
        var service = new GameAudioService(config, null, pauseState, backend);
        object uiOwner = new();
        object gameplayOwner = new();

        service.StartLoop(GameAudioCue.UiSheenLoop, uiOwner, Vector3.zero);
        service.StartLoop(GameAudioCue.InteractionHoldLoop, gameplayOwner, Vector3.zero);
        pauseState.Pause(RunPauseReasons.UpgradeSelection);
        service.Tick();

        Assert.That(backend.PostedEvents, Is.EqualTo(new[]
        {
            config.uiSheenLoopEvent,
            config.interactionHoldLoopEvent,
            config.stopInteractionHoldLoopEvent
        }));

        service.StopLoop(GameAudioCue.UiSheenLoop, uiOwner);

        Assert.That(backend.LastPostedEvent, Is.EqualTo(config.stopUiSheenLoopEvent));
    }

    [Test]
    public void GameplayMusicStartsAfterRunTransitionClears()
    {
        config = ScriptableObject.CreateInstance<GameAudioConfig>();
        config.EnsureWorkflowDefaults();
        config.preloadEventSoundBanksOnStart = false;
        var pauseState = new RunPauseState();
        pauseState.Pause(RunPauseReasons.RunTransition);
        var backend = new FakeWwiseRuntimeAudioBackend(loadResult: true);
        var service = new GameAudioService(config, null, pauseState, backend);

        service.Start();
        service.Tick();

        Assert.That(backend.PostedEvents, Does.Not.Contain(config.gameplayMusicEvent));

        pauseState.Resume(RunPauseReasons.RunTransition);
        service.Tick();

        Assert.That(backend.PostedEvents, Contains.Item(config.gameplayMusicEvent));
        Assert.That(backend.RtpcValues(config.musicPresentationRtpcName), Is.EqualTo(new[] { 0f }).Within(0.001f));
    }

    [Test]
    public void GameplayMusicPresentationDucksDuringRewardScreens()
    {
        config = ScriptableObject.CreateInstance<GameAudioConfig>();
        config.EnsureWorkflowDefaults();
        config.preloadEventSoundBanksOnStart = false;
        config.musicNormalPresentationValue = 0f;
        config.musicDistantPresentationValue = 1f;
        var pauseState = new RunPauseState();
        var backend = new FakeWwiseRuntimeAudioBackend(loadResult: true);
        var service = new GameAudioService(config, null, pauseState, backend);

        service.Start();
        service.Tick();
        pauseState.Pause(RunPauseReasons.UpgradeSelection);
        service.Tick();
        pauseState.Resume(RunPauseReasons.UpgradeSelection);
        service.Tick();

        Assert.That(backend.PostedEvents, Contains.Item(config.gameplayMusicEvent));
        Assert.That(backend.RtpcValues(config.musicPresentationRtpcName), Is.EqualTo(new[] { 0f, 1f, 0f }).Within(0.001f));
        Assert.That(backend.OutputBusVolumes, Is.EqualTo(new[] { 0.55f, 0.4f, 0.55f }).Within(0.001f));
    }

    [Test]
    public void GameplayMusicPresentationDucksDuringPauseMenu()
    {
        config = ScriptableObject.CreateInstance<GameAudioConfig>();
        config.EnsureWorkflowDefaults();
        config.preloadEventSoundBanksOnStart = false;
        config.musicNormalPresentationValue = 0f;
        config.musicDistantPresentationValue = 1f;
        var pauseState = new RunPauseState();
        var backend = new FakeWwiseRuntimeAudioBackend(loadResult: true);
        var service = new GameAudioService(config, null, pauseState, backend);

        service.Start();
        service.Tick();
        pauseState.Pause(RunPauseReasons.PauseMenu);
        service.Tick();
        pauseState.Resume(RunPauseReasons.PauseMenu);
        service.Tick();

        Assert.That(backend.PostedEvents, Contains.Item(config.gameplayMusicEvent));
        Assert.That(backend.RtpcValues(config.musicPresentationRtpcName), Is.EqualTo(new[] { 0f, 1f, 0f }).Within(0.001f));
        Assert.That(backend.OutputBusVolumes, Is.EqualTo(new[] { 0.55f, 0.4f, 0.55f }).Within(0.001f));
    }

    [Test]
    public void ResetRunStateStopsGameplayMusic()
    {
        config = ScriptableObject.CreateInstance<GameAudioConfig>();
        config.EnsureWorkflowDefaults();
        config.preloadEventSoundBanksOnStart = false;
        var backend = new FakeWwiseRuntimeAudioBackend(loadResult: true);
        var service = new GameAudioService(config, null, null, backend);

        service.Start();
        service.Tick();
        service.ResetRunState(new RunResetContext(RunResetKind.StartNewRun));

        Assert.That(backend.PostedEvents, Is.EqualTo(new[] { config.gameplayMusicEvent, config.stopGameplayMusicEvent }));
    }

    private sealed class FakeWwiseRuntimeAudioBackend : IWwiseRuntimeAudioBackend
    {
        private readonly bool loadResult;
        private readonly List<RtpcCall> rtpcs = new();
        private readonly List<string> postedEvents = new();
        private readonly List<float> outputBusVolumes = new();

        public FakeWwiseRuntimeAudioBackend(bool loadResult)
        {
            this.loadResult = loadResult;
        }

        public int LoadBankCount { get; private set; }
        public int PostEventCount { get; private set; }
        public string LastPostedEvent { get; private set; }
        public IReadOnlyList<string> PostedEvents => postedEvents;
        public IReadOnlyList<float> OutputBusVolumes => outputBusVolumes;

        public float[] RtpcValues(string rtpcName)
        {
            var values = new List<float>();
            for (int i = 0; i < rtpcs.Count; i++)
            {
                if (rtpcs[i].Name == rtpcName)
                {
                    values.Add(rtpcs[i].Value);
                }
            }

            return values.ToArray();
        }

        public bool LoadBank(string bankName, bool eventBank)
        {
            LoadBankCount++;
            return loadResult;
        }

        public void PostEvent(string eventName, GameObject emitter)
        {
            PostEventCount++;
            LastPostedEvent = eventName;
            postedEvents.Add(eventName);
        }

        public void SetRtpc(string rtpcName, float value, GameObject emitter)
        {
            rtpcs.Add(new RtpcCall(rtpcName, value));
        }

        public void SetOutputBusVolume(GameObject emitter, float volume)
        {
            outputBusVolumes.Add(volume);
        }

        public void StopAllOnEmitter(GameObject emitter)
        {
        }

        private readonly struct RtpcCall
        {
            public RtpcCall(string name, float value)
            {
                Name = name;
                Value = value;
            }

            public string Name { get; }
            public float Value { get; }
        }
    }
}
