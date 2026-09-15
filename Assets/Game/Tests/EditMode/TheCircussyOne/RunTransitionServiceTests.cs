using System;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using TheCircussyOne.Config;
using TheCircussyOne.Runtime;
using UnityEngine;

public sealed class RunTransitionServiceTests
{
    private HudVisualConfig config;

    [TearDown]
    public void TearDown()
    {
        TheCircussyOneTestObjects.Destroy(config);
    }

    [Test]
    public void RunTransitionReleasesPauseBeforeOverlayFadesOut()
    {
        config = ScriptableObject.CreateInstance<HudVisualConfig>();
        config.runTransition = HudTransitionSettings.Default;
        var pause = new RunPauseState();
        bool workRan = false;
        bool pauseWasHeldDuringWork = false;
        bool pauseWasReleasedBeforeComplete = false;
        var overlay = new FakeRunTransitionOverlay(
            onBeforeWork: () => pauseWasHeldDuringWork = pause.HasReason(RunPauseReasons.RunTransition),
            onAfterReady: () => pauseWasReleasedBeforeComplete = !pause.HasReason(RunPauseReasons.RunTransition));
        var service = new RunTransitionService(config, pause, overlayProvider: () => overlay);

        Assert.That(service.Play("RESETTING THE STAGE", "Loading...", () => workRan = true), Is.True, "transition should start");

        Assert.That(workRan, Is.True);
        Assert.That(pauseWasHeldDuringWork, Is.True);
        Assert.That(pauseWasReleasedBeforeComplete, Is.True);
        Assert.That(pause.HasReason(RunPauseReasons.RunTransition), Is.False);
    }

    [Test]
    public void RunTransitionForwardsLoadingStageWhileOverlayIsActive()
    {
        config = ScriptableObject.CreateInstance<HudVisualConfig>();
        config.runTransition = HudTransitionSettings.Default;
        var pause = new RunPauseState();
        var overlay = new FakeRunTransitionOverlay(null, null);
        RunTransitionService service = null;
        service = new RunTransitionService(config, pause, overlayProvider: () => overlay);

        Assert.That(service.Play("SETTING THE STAGE", "Loading...", () => service.SetLoadingStage("Laying the Grounds")), Is.True);

        Assert.That(overlay.LastLoadingStage, Is.EqualTo("Laying the Grounds"));
        Assert.That(service.IsTransitionActive, Is.False);
    }

    [Test]
    public void RunTransitionIgnoresLoadingStageWhenNoOverlayIsActive()
    {
        config = ScriptableObject.CreateInstance<HudVisualConfig>();
        config.runTransition = HudTransitionSettings.Default;
        var pause = new RunPauseState();
        var overlay = new FakeRunTransitionOverlay(null, null);
        var service = new RunTransitionService(config, pause, overlayProvider: () => overlay);

        service.SetLoadingStage("Laying the Grounds");

        Assert.That(overlay.LastLoadingStage, Is.Null);
    }

    private sealed class FakeRunTransitionOverlay : IRunTransitionOverlay
    {
        private readonly Action onBeforeWork;
        private readonly Action onAfterReady;

        public FakeRunTransitionOverlay(Action onBeforeWork, Action onAfterReady)
        {
            this.onBeforeWork = onBeforeWork;
            this.onAfterReady = onAfterReady;
        }

        public bool IsActive { get; private set; }
        public string LastLoadingStage { get; private set; }

        public void SetLoadingStage(string stage)
        {
            LastLoadingStage = stage;
        }

        public bool Play(
            HudVisualConfig config,
            string title,
            string subtitle,
            Action work,
            Action onComplete = null,
            Action onReadyToReveal = null,
            float? minimumVisibleSecondsOverride = null)
        {
            IsActive = true;
            onBeforeWork?.Invoke();
            work?.Invoke();
            onReadyToReveal?.Invoke();
            onAfterReady?.Invoke();
            onComplete?.Invoke();
            IsActive = false;
            return true;
        }

        public bool Play(
            HudVisualConfig config,
            string title,
            string subtitle,
            Func<UniTask> work,
            Action onComplete = null,
            Action onReadyToReveal = null,
            float? minimumVisibleSecondsOverride = null)
        {
            IsActive = true;
            onBeforeWork?.Invoke();
            if (work != null)
            {
                work.Invoke().GetAwaiter().GetResult();
            }

            onReadyToReveal?.Invoke();
            onAfterReady?.Invoke();
            onComplete?.Invoke();
            IsActive = false;
            return true;
        }

        public bool Play(
            HudVisualConfig config,
            string title,
            string subtitle,
            Func<AsyncOperation> work,
            Action onComplete = null,
            Action onReadyToReveal = null,
            float? minimumVisibleSecondsOverride = null)
        {
            return Play(config, title, subtitle, () =>
            {
                work?.Invoke();
                return UniTask.CompletedTask;
            }, onComplete, onReadyToReveal, minimumVisibleSecondsOverride);
        }
    }
}
