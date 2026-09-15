using System;
using Cysharp.Threading.Tasks;
using TheCircussyOne.Config;
using TheCircussyOne.Visuals;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheCircussyOne.Runtime
{
    public interface IRunTransitionOverlay
    {
        bool IsActive { get; }
        void SetLoadingStage(string stage);
        bool Play(
            HudVisualConfig config,
            string title,
            string subtitle,
            Action work,
            Action onComplete = null,
            Action onReadyToReveal = null,
            float? minimumVisibleSecondsOverride = null);
        bool Play(
            HudVisualConfig config,
            string title,
            string subtitle,
            Func<UniTask> work,
            Action onComplete = null,
            Action onReadyToReveal = null,
            float? minimumVisibleSecondsOverride = null);
        bool Play(
            HudVisualConfig config,
            string title,
            string subtitle,
            Func<AsyncOperation> work,
            Action onComplete = null,
            Action onReadyToReveal = null,
            float? minimumVisibleSecondsOverride = null);
    }

    public interface IRunLoadingStageSink
    {
        void SetLoadingStage(string stage);
    }

    public interface IRunTransitionService : IRunLoadingStageSink
    {
        bool IsEnabled { get; }
        bool IsTransitionActive { get; }
        bool Play(string title, string subtitle, Action work);
        bool Play(string title, string subtitle, Action work, float minimumVisibleSeconds);
        bool PlayAsync(string title, string subtitle, Func<UniTask> work);
        bool PlayAsync(string title, string subtitle, Func<UniTask> work, float minimumVisibleSeconds);
        bool PlaySceneReload(string title, string subtitle, Func<AsyncOperation> loadScene);
    }

    public sealed class RunTransitionService : IRunTransitionService
    {
        private readonly HudVisualConfig config;
        private readonly RunPauseState pauseState;
        private readonly PanelSettings panelSettings;
        private readonly Func<IRunTransitionOverlay> overlayProvider;
        private IRunTransitionOverlay activeOverlay;

        public RunTransitionService(
            HudVisualConfig config = null,
            RunPauseState pauseState = null,
            PanelSettings panelSettings = null,
            Func<IRunTransitionOverlay> overlayProvider = null)
        {
            this.config = config;
            this.pauseState = pauseState;
            this.panelSettings = panelSettings;
            this.overlayProvider = overlayProvider ?? (() => RunTransitionOverlayHost.GetOrCreate(this.panelSettings));
        }

        public bool IsEnabled
        {
            get
            {
                HudTransitionSettings settings = config != null ? config.runTransition : HudTransitionSettings.Default;
                settings.EnsureDefaults();
                return settings.enabled;
            }
        }

        public bool IsTransitionActive => activeOverlay != null && activeOverlay.IsActive;

        public void SetLoadingStage(string stage)
        {
            if (!IsEnabled || activeOverlay == null || !activeOverlay.IsActive)
            {
                return;
            }

            activeOverlay.SetLoadingStage(stage);
        }

        public bool Play(string title, string subtitle, Action work)
        {
            return Play(title, subtitle, work, null);
        }

        public bool Play(string title, string subtitle, Action work, float minimumVisibleSeconds)
        {
            return Play(title, subtitle, work, (float?)minimumVisibleSeconds);
        }

        public bool PlayAsync(string title, string subtitle, Func<UniTask> work)
        {
            return PlayAsync(title, subtitle, work, null);
        }

        public bool PlayAsync(string title, string subtitle, Func<UniTask> work, float minimumVisibleSeconds)
        {
            return PlayAsync(title, subtitle, work, (float?)minimumVisibleSeconds);
        }

        public bool PlaySceneReload(string title, string subtitle, Func<AsyncOperation> loadScene)
        {
            if (!IsEnabled)
            {
                _ = loadScene?.Invoke();
                return true;
            }

            pauseState?.Pause(RunPauseReasons.RunTransition);
            activeOverlay = overlayProvider.Invoke();
            bool started = activeOverlay.Play(
                config,
                title,
                subtitle,
                loadScene,
                ClearActiveOverlay,
                () => pauseState?.Resume(RunPauseReasons.RunTransition));
            if (!started)
            {
                pauseState?.Resume(RunPauseReasons.RunTransition);
                ClearActiveOverlay();
            }

            return started;
        }

        private bool Play(string title, string subtitle, Action work, float? minimumVisibleSeconds)
        {
            if (!IsEnabled)
            {
                work?.Invoke();
                return true;
            }

            pauseState?.Pause(RunPauseReasons.RunTransition);
            activeOverlay = overlayProvider.Invoke();
            bool started = activeOverlay.Play(
                config,
                title,
                subtitle,
                work,
                ClearActiveOverlay,
                () => pauseState?.Resume(RunPauseReasons.RunTransition),
                minimumVisibleSeconds);
            if (!started)
            {
                pauseState?.Resume(RunPauseReasons.RunTransition);
                ClearActiveOverlay();
            }

            return started;
        }

        private bool PlayAsync(string title, string subtitle, Func<UniTask> work, float? minimumVisibleSeconds)
        {
            if (!IsEnabled)
            {
                if (work != null)
                {
                    work.Invoke().Forget();
                }

                return true;
            }

            pauseState?.Pause(RunPauseReasons.RunTransition);
            activeOverlay = overlayProvider.Invoke();
            bool started = activeOverlay.Play(
                config,
                title,
                subtitle,
                work,
                ClearActiveOverlay,
                () => pauseState?.Resume(RunPauseReasons.RunTransition),
                minimumVisibleSeconds);
            if (!started)
            {
                pauseState?.Resume(RunPauseReasons.RunTransition);
                ClearActiveOverlay();
            }

            return started;
        }

        private void ClearActiveOverlay()
        {
            activeOverlay = null;
        }
    }
}
