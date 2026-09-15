using System.Collections.Generic;
using NUnit.Framework;
using TheCircussyOne.Config;
using TheCircussyOne.Runtime;
using UnityEngine;

public sealed class GameHapticsServiceTests
{
    private GameHapticsConfig config;

    [TearDown]
    public void TearDown()
    {
        TheCircussyOneTestObjects.Destroy(config);
    }

    [Test]
    public void PlayDoesNotSetMotorsWhenDisabled()
    {
        config = ScriptableObject.CreateInstance<GameHapticsConfig>();
        config.EnsureWorkflowDefaults();
        config.hapticsEnabled = false;
        var backend = new FakeGamepadHapticsBackend();
        var service = new GameHapticsService(config, backend, () => 0.016f);

        service.Start();
        backend.Clear();
        service.Play(GameHapticsCue.UiClick);

        Assert.That(backend.SetCalls, Is.Empty);
    }

    [Test]
    public void PulseStopsAfterConfiguredDuration()
    {
        config = ScriptableObject.CreateInstance<GameHapticsConfig>();
        config.EnsureWorkflowDefaults();
        config.uiClick = GameHapticsPattern.Single(0.2f, 0.4f, 0.02f, 5, 0f);
        var backend = new FakeGamepadHapticsBackend();
        var service = new GameHapticsService(config, backend, () => 0.03f);

        service.Start();
        backend.Clear();
        service.Play(GameHapticsCue.UiClick);
        service.Tick();

        AssertHapticsCall(backend.SetCalls[0], 0.17f, 0.34f);
        Assert.That(backend.StopCount, Is.EqualTo(1));
    }

    [Test]
    public void HigherPriorityCueOverridesLowerPriorityCue()
    {
        config = ScriptableObject.CreateInstance<GameHapticsConfig>();
        config.EnsureWorkflowDefaults();
        config.xpCollect = GameHapticsPattern.Single(0.02f, 0.03f, 0.2f, 1, 0f);
        config.playerDamaged = GameHapticsPattern.Single(0.7f, 0.8f, 0.2f, 40, 0f);
        var backend = new FakeGamepadHapticsBackend();
        var service = new GameHapticsService(config, backend, () => 0.016f);

        service.Start();
        backend.Clear();
        service.Play(GameHapticsCue.XpCollect);
        service.Play(GameHapticsCue.PlayerDamaged);

        Assert.That(backend.SetCalls.Count, Is.EqualTo(2));
        AssertHapticsCall(backend.SetCalls[1], 0.595f, 0.68f);
    }

    [Test]
    public void LowerPriorityCueDoesNotInterruptActiveHigherPriorityCue()
    {
        config = ScriptableObject.CreateInstance<GameHapticsConfig>();
        config.EnsureWorkflowDefaults();
        config.xpCollect = GameHapticsPattern.Single(0.02f, 0.03f, 0.2f, 1, 0f);
        config.playerDamaged = GameHapticsPattern.Single(0.7f, 0.8f, 0.2f, 40, 0f);
        var backend = new FakeGamepadHapticsBackend();
        var service = new GameHapticsService(config, backend, () => 0.016f);

        service.Start();
        backend.Clear();
        service.Play(GameHapticsCue.PlayerDamaged);
        service.Play(GameHapticsCue.XpCollect);

        Assert.That(backend.SetCalls.Count, Is.EqualTo(1));
        AssertHapticsCall(backend.SetCalls[0], 0.595f, 0.68f);
    }

    [Test]
    public void CooldownPreventsImmediateRepeat()
    {
        config = ScriptableObject.CreateInstance<GameHapticsConfig>();
        config.EnsureWorkflowDefaults();
        config.uiHover = GameHapticsPattern.Single(0.1f, 0.1f, 0.01f, 5, 0.5f);
        var backend = new FakeGamepadHapticsBackend();
        var service = new GameHapticsService(config, backend, () => 0.6f);

        service.Start();
        backend.Clear();
        service.Play(GameHapticsCue.UiHover);
        service.Play(GameHapticsCue.UiHover);
        service.Tick();
        service.Play(GameHapticsCue.UiHover);

        Assert.That(backend.SetCalls.Count, Is.EqualTo(2));
    }

    [Test]
    public void PauseToggleCanBlockHapticsWhilePaused()
    {
        config = ScriptableObject.CreateInstance<GameHapticsConfig>();
        config.EnsureWorkflowDefaults();
        config.allowHapticsWhilePaused = false;
        var backend = new FakeGamepadHapticsBackend();
        var pauseState = new RunPauseState();
        var service = new GameHapticsService(config, backend, pauseState);

        service.Start();
        backend.Clear();
        pauseState.Pause(RunPauseReasons.PauseMenu);
        service.Play(GameHapticsCue.UiClick);

        Assert.That(backend.SetCalls, Is.Empty);
    }

    private sealed class FakeGamepadHapticsBackend : IGamepadHapticsBackend
    {
        public readonly List<(float LowFrequency, float HighFrequency)> SetCalls = new();

        public bool IsAvailable { get; set; } = true;
        public int StopCount { get; private set; }

        public void SetMotorSpeeds(float lowFrequency, float highFrequency)
        {
            SetCalls.Add((lowFrequency, highFrequency));
        }

        public void StopMotorSpeeds()
        {
            StopCount++;
        }

        public void Clear()
        {
            SetCalls.Clear();
            StopCount = 0;
        }
    }

    private static void AssertHapticsCall((float LowFrequency, float HighFrequency) call, float expectedLow, float expectedHigh)
    {
        Assert.That(call.LowFrequency, Is.EqualTo(expectedLow).Within(0.001f));
        Assert.That(call.HighFrequency, Is.EqualTo(expectedHigh).Within(0.001f));
    }
}
