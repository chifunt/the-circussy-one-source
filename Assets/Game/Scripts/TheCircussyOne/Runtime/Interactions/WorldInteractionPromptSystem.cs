using System;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;
using TheCircussyOne.Visuals;
using UnityEngine;
using VContainer.Unity;

namespace TheCircussyOne.Runtime
{
    public sealed class WorldInteractionPromptSystem : IWorldInteractionPrompt, ITickable, ITickableWhenPaused, IDisposable, IRunResettable
    {
        private readonly WorldInteractionPromptVisualConfig config;
        private readonly WorldInteractionPromptFactory factory;
        private readonly CameraView cameraView;
        private readonly RunPauseState pauseState;

        private WorldInteractionPromptView view;
        private WorldInteractionPromptFrame lastFrame;
        private IWorldInteractionPromptTarget currentTarget;
        private IWorldInteractionPromptTarget lastTarget;
        private string currentPrompt;
        private float currentProgress;
        private bool currentHolding;
        private bool currentGamepadGlyph;
        private bool currentShowGlyph = true;
        private bool requestedVisible;
        private float transitionSeconds;
        private float visibleSeconds;
        private Vector3 lastPosition;
        private WorldInteractionPromptTransition transition = WorldInteractionPromptTransition.Hidden;

        public WorldInteractionPromptSystem(
            WorldInteractionPromptVisualConfig config,
            WorldInteractionPromptFactory factory,
            CameraView cameraView = null,
            RunPauseState pauseState = null)
        {
            this.config = config;
            this.factory = factory;
            this.cameraView = cameraView;
            this.pauseState = pauseState;
        }

        public bool IsShowing => transition != WorldInteractionPromptTransition.Hidden;

        public void Dispose()
        {
            requestedVisible = false;
            currentTarget = null;
            lastTarget = null;
            transition = WorldInteractionPromptTransition.Hidden;
            view = null;
            factory?.Clear();
        }

        public void Show(IWorldInteractionPromptTarget target, string prompt, float progress, bool isHolding, bool useGamepadGlyph, bool showGlyph = true)
        {
            if (target == null || config == null || !config.enabled)
            {
                Hide();
                return;
            }

            currentTarget = target;
            currentPrompt = prompt;
            currentProgress = Mathf.Clamp01(progress);
            currentHolding = isHolding;
            currentGamepadGlyph = useGamepadGlyph;
            currentShowGlyph = showGlyph;
            requestedVisible = true;

            if (transition == WorldInteractionPromptTransition.Hidden || lastTarget != target)
            {
                transition = WorldInteractionPromptTransition.Showing;
                transitionSeconds = 0f;
                visibleSeconds = 0f;
                lastTarget = target;
            }
        }

        public void Hide()
        {
            requestedVisible = false;
            currentTarget = null;
            currentPrompt = string.Empty;
            currentProgress = 0f;
            currentHolding = false;
            if (pauseState != null && pauseState.IsPaused)
            {
                HideImmediate();
                return;
            }

            if (transition == WorldInteractionPromptTransition.Hidden || transition == WorldInteractionPromptTransition.Hiding)
            {
                return;
            }

            transition = WorldInteractionPromptTransition.Hiding;
            transitionSeconds = 0f;
        }

        public void Clear()
        {
            HideImmediate();
            view = null;
            factory?.Clear();
        }

        public void ResetRunState(RunResetContext context)
        {
            Clear();
        }

        public void Tick()
        {
            if (pauseState != null && pauseState.IsPaused)
            {
                HideImmediate();
                return;
            }

            float deltaTime = Mathf.Max(0f, Time.unscaledDeltaTime);
            if (!requestedVisible && transition == WorldInteractionPromptTransition.Visible)
            {
                Hide();
            }

            if (transition == WorldInteractionPromptTransition.Hidden)
            {
                DeactivateViewIfAlive();
                return;
            }

            transitionSeconds += deltaTime;
            visibleSeconds += deltaTime;
            if (transition == WorldInteractionPromptTransition.Showing && transitionSeconds >= Mathf.Max(0.01f, config.showSeconds))
            {
                transition = WorldInteractionPromptTransition.Visible;
                transitionSeconds = 0f;
            }
            else if (transition == WorldInteractionPromptTransition.Hiding && transitionSeconds >= Mathf.Max(0.01f, config.hideSeconds))
            {
                transition = WorldInteractionPromptTransition.Hidden;
                transitionSeconds = 0f;
                lastTarget = null;
                DeactivateViewIfAlive();
                return;
            }

            WorldInteractionPromptFrame frame = BuildFrame();

            WorldInteractionPromptView promptView = GetOrCreateView();
            promptView?.ApplyFrame(frame, config, cameraView != null ? cameraView.Camera : Camera.main);
        }

        private WorldInteractionPromptFrame BuildFrame()
        {
            Vector3 anchor = currentTarget != null ? currentTarget.WorldInteractionPromptPosition : lastPosition;
            lastPosition = anchor;
            Vector3 position = anchor + config.worldOffset;
            Vector2 shake = currentHolding
                ? WorldInteractionPromptRules.ProgressShake(
                    currentProgress,
                    visibleSeconds,
                    config.progressShakeMinAmplitude,
                    config.progressShakeMaxAmplitude,
                    config.progressShakeFrequency,
                    config.progressShakeEase)
                : Vector2.zero;

            lastFrame = new WorldInteractionPromptFrame(
                true,
                position,
                WorldInteractionPromptRules.Scale(
                    transition,
                    transitionSeconds,
                    config.showSeconds,
                    config.hideSeconds,
                    config.baseWorldScale,
                    config.targetScale,
                    config.showEase,
                    config.hideEase),
                WorldInteractionPromptRules.Alpha(transition, transitionSeconds, config.showSeconds, config.hideSeconds),
                WorldInteractionPromptRules.Label(currentPrompt, currentHolding, currentShowGlyph),
                WorldInteractionPromptRules.Glyph(currentGamepadGlyph),
                currentShowGlyph && !currentHolding,
                currentGamepadGlyph,
                currentHolding,
                currentProgress,
                shake);
            return lastFrame;
        }

        private WorldInteractionPromptView GetOrCreateView()
        {
            if (view != null)
            {
                return view;
            }

            view = null;
            view = factory?.GetOrCreate();
            return view;
        }

        private void DeactivateViewIfAlive()
        {
            if (view == null)
            {
                view = null;
                return;
            }

            view.Deactivate();
        }

        private void HideImmediate()
        {
            requestedVisible = false;
            transition = WorldInteractionPromptTransition.Hidden;
            transitionSeconds = 0f;
            visibleSeconds = 0f;
            currentTarget = null;
            lastTarget = null;
            currentPrompt = string.Empty;
            currentProgress = 0f;
            currentHolding = false;
            lastFrame = default;
            DeactivateViewIfAlive();
        }
    }
}
