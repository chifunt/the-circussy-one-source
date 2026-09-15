using System;
using System.Collections.Generic;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using TheCircussyOne.Visuals;
using UnityEngine;
using VContainer.Unity;

namespace TheCircussyOne.Runtime
{
    public sealed class PerformerSelectionPresenter : IStartable, ITickable, ITickableWhenPaused, IDisposable
    {
        private const string PauseReason = RunPauseReasons.PerformerSelection;

        private readonly PerformerCatalog catalog;
        private readonly PerformerRunState runState;
        private readonly PerformerRunComposer composer;
        private readonly RunPauseState pauseState;
        private readonly PerformerSelectionView view;
        private readonly IInputService input;
        private readonly IGameAudio audio;
        private readonly IGameHaptics haptics;
        private UpgradeNavigationRepeatState navigationRepeat;
        private UpgradeNavigationRepeatState cycleRepeat;
        private IReadOnlyList<PerformerSelectionCardFrame> pendingFrames;
        private bool waitingForView;
        private bool subscribedToView;

        public PerformerSelectionPresenter(
            PerformerCatalog catalog,
            PerformerRunState runState,
            PerformerRunComposer composer,
            RunPauseState pauseState,
            PerformerSelectionView view = null,
            IInputService input = null,
            IGameAudio audio = null,
            IGameHaptics haptics = null)
        {
            this.catalog = catalog;
            this.runState = runState;
            this.composer = composer;
            this.pauseState = pauseState;
            this.view = view;
            this.input = input;
            this.audio = audio ?? NullGameAudio.Instance;
            this.haptics = haptics ?? NullGameHaptics.Instance;
        }

        public void Start()
        {
            if (runState != null && runState.IsSelectionComplete)
            {
                return;
            }

            if (TryStartPendingLaunch())
            {
                return;
            }

            ShowSelection();
        }

        public bool ShowSelection()
        {
            IReadOnlyList<PerformerDefinition> performers = catalog?.Performers;
            PerformerDefinition fallback = catalog?.FallbackPerformer;
            IReadOnlyList<PerformerSelectionCardFrame> frames = PerformerSelectionPreviewRules.BuildFrames(performers);
            if (view == null || frames.Count == 0)
            {
                return composer?.ComposeAndStart(fallback) == true;
            }

            if (!subscribedToView)
            {
                view.PerformerSelected += OnPerformerSelected;
                subscribedToView = true;
            }

            pauseState?.Pause(PauseReason);
            navigationRepeat = new UpgradeNavigationRepeatState { RequiresNeutralRelease = true };
            cycleRepeat = new UpgradeNavigationRepeatState { RequiresNeutralRelease = true };
            ShowWhenViewIsReady(frames);
            return true;
        }

        public void Tick()
        {
            view?.TickPopupMotion(Time.unscaledDeltaTime);
            if (waitingForView)
            {
                TryShowPendingSelection();
            }

            if (view == null || !view.IsVisible)
            {
                return;
            }

            int directChoice = input != null ? input.PerformerChoicePressedThisFrame : 0;
            if (directChoice >= 1 && directChoice <= view.VisibleCardCount)
            {
                audio.Play(GameAudioCue.UiClick);
                haptics.Play(GameHapticsCue.UiClick);
                view.Select(directChoice - 1);
                return;
            }

            UiCycleNavigationDirection cycle = input != null ? input.UiCycleNavigation : UiCycleNavigationDirection.None;
            UpgradeNavigationDirection cycleNavigation = CycleNavigationAsUpgradeDirection(cycle);
            if (UpgradeNavigationRules.ShouldTrigger(ref cycleRepeat, cycleNavigation, Time.unscaledTime))
            {
                int index = NextCycleIndex(view.HighlightedIndex, view.VisibleCardCount, cycle);
                if (index >= 0)
                {
                    HighlightWithAudio(index);
                }
            }

            UpgradeNavigationDirection navigation = input != null ? input.UpgradeNavigation : UpgradeNavigationDirection.None;
            if (UpgradeNavigationRules.ShouldTrigger(ref navigationRepeat, navigation, Time.unscaledTime))
            {
                int index = NextPerformerIndex(view.HighlightedIndex, view.VisibleCardCount, navigation);
                if (index >= 0)
                {
                    HighlightWithAudio(index);
                }
            }

            if (input != null && input.SubmitPressedThisFrame && view.HasHighlightedSelection)
            {
                audio.Play(GameAudioCue.UiClick);
                haptics.Play(GameHapticsCue.UiClick);
                view.SelectHighlighted();
            }
        }

        public void Dispose()
        {
            if (view != null)
            {
                view.PerformerSelected -= OnPerformerSelected;
                subscribedToView = false;
            }
        }

        private void OnPerformerSelected(PerformerDefinition performer)
        {
            if (!composer.ComposeAndStart(performer))
            {
                return;
            }

            view?.Hide();
            pendingFrames = null;
            waitingForView = false;
            pauseState?.Resume(PauseReason);
        }

        private bool TryStartPendingLaunch()
        {
            if (!PendingRunLaunch.TryConsume(out PendingRunLaunchRequest request))
            {
                return false;
            }

            PerformerDefinition performer = catalog?.FindById(request.PerformerId);
            if (!ContentAvailabilityRules.IsActiveAndValid(performer)
                || !performer.unlocked
                || !ContentAvailabilityRules.IsActiveAndValid(performer.startingWeapon))
            {
                performer = catalog?.FallbackPerformer;
            }

            if (performer == null)
            {
                PendingRunLaunch.SignalGameplayFailed();
                return false;
            }

            if (request.WaitForGameplayReady && pauseState != null)
            {
                pauseState.Pause(RunPauseReasons.RunTransition);
                PendingRunLaunch.RegisterGameplayRelease(() => pauseState.Resume(RunPauseReasons.RunTransition));
            }

            bool started = request.WaitForGameplayReady
                ? composer?.ComposeAndStartSilentlyAsync(performer, PendingRunLaunch.SignalGameplayReady, PendingRunLaunch.SignalGameplayFailed) == true
                : composer?.ComposeAndStart(performer) == true;
            if (!started)
            {
                PendingRunLaunch.SignalGameplayFailed();
            }

            return started;
        }

        private void ShowWhenViewIsReady(IReadOnlyList<PerformerSelectionCardFrame> frames)
        {
            pendingFrames = frames;
            waitingForView = true;
            TryShowPendingSelection();
        }

        private void TryShowPendingSelection()
        {
            if (view == null || pendingFrames == null || !view.CanShowSelection)
            {
                return;
            }

            waitingForView = false;
            view.Show(pendingFrames);
        }

        private void HighlightWithAudio(int index)
        {
            int previousIndex = view.HighlightedIndex;
            view.Highlight(index);
            if (view.HighlightedIndex != previousIndex && view.HasHighlightedSelection)
            {
                audio.Play(GameAudioCue.UiHover);
                haptics.Play(GameHapticsCue.UiHover);
            }
        }

        private static int NextPerformerIndex(int currentIndex, int choiceCount, UpgradeNavigationDirection direction)
        {
            if (choiceCount <= 0 || direction == UpgradeNavigationDirection.None)
            {
                return -1;
            }

            if (currentIndex < 0)
            {
                return direction switch
                {
                    UpgradeNavigationDirection.Right => choiceCount - 1,
                    UpgradeNavigationDirection.Middle => choiceCount / 2,
                    _ => 0
                };
            }

            if (direction == UpgradeNavigationDirection.Middle)
            {
                return currentIndex;
            }

            int offset = direction == UpgradeNavigationDirection.Left ? -1 : 1;
            return (currentIndex + offset + choiceCount) % choiceCount;
        }

        private static UpgradeNavigationDirection CycleNavigationAsUpgradeDirection(UiCycleNavigationDirection direction)
        {
            return direction switch
            {
                UiCycleNavigationDirection.Previous => UpgradeNavigationDirection.Left,
                UiCycleNavigationDirection.Next => UpgradeNavigationDirection.Right,
                _ => UpgradeNavigationDirection.None
            };
        }

        private static int NextCycleIndex(int currentIndex, int choiceCount, UiCycleNavigationDirection direction)
        {
            if (choiceCount <= 0 || direction == UiCycleNavigationDirection.None)
            {
                return -1;
            }

            if (currentIndex < 0)
            {
                return direction == UiCycleNavigationDirection.Previous ? choiceCount - 1 : 0;
            }

            int offset = direction == UiCycleNavigationDirection.Previous ? -1 : 1;
            return (currentIndex + offset + choiceCount) % choiceCount;
        }
    }
}
