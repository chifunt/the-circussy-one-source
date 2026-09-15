using System;
using System.Collections.Generic;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using TheCircussyOne.Visuals;
using VContainer.Unity;

namespace TheCircussyOne.Runtime
{
    public sealed class RewardRevealPresenter : IStartable, ITickable, ITickableWhenPaused, IDisposable, IRunResettable
    {
        private const string PauseReason = RunPauseReasons.RewardReveal;

        private readonly ItemInventory itemInventory;
        private readonly RunPauseState pauseState;
        private readonly RewardRevealView view;
        private readonly IInputService input;
        private readonly IGameAudio audio;
        private readonly IGameHaptics haptics;
        private readonly Queue<RewardRevealFrame> pending = new();

        public RewardRevealPresenter(
            ItemInventory itemInventory,
            RunPauseState pauseState,
            RewardRevealView view = null,
            IInputService input = null,
            IGameAudio audio = null,
            IGameHaptics haptics = null)
        {
            this.itemInventory = itemInventory;
            this.pauseState = pauseState;
            this.view = view;
            this.input = input;
            this.audio = audio ?? NullGameAudio.Instance;
            this.haptics = haptics ?? NullGameHaptics.Instance;
        }

        public void Start()
        {
            if (itemInventory != null)
            {
                itemInventory.ItemAdded += OnItemAdded;
            }

            if (view != null)
            {
                view.DismissRequested += DismissActive;
                view.Hide();
            }
        }

        public void Tick()
        {
            view?.TickPopupMotion(UnityEngine.Time.unscaledDeltaTime);
            if (view == null || !view.IsVisible)
            {
                return;
            }

            if (input != null && input.SubmitPressedThisFrame && view.CanDismiss)
            {
                DismissActive();
            }
        }

        public void Dispose()
        {
            if (itemInventory != null)
            {
                itemInventory.ItemAdded -= OnItemAdded;
            }

            if (view != null)
            {
                view.DismissRequested -= DismissActive;
                view.Hide();
            }

            audio.StopLoop(GameAudioCue.UiSheenLoop, this);
            pauseState?.Resume(PauseReason);
        }

        public void ShowChestReward(ItemDefinition item, int addedStacks, int currentStackCount)
        {
            Enqueue(RewardRevealFrame.ForChest(item, addedStacks, currentStackCount));
        }

        public void ResetRunState()
        {
            ResetRunState(new RunResetContext(RunResetKind.ReturnToPerformerSelection));
        }

        public void ResetRunState(RunResetContext context)
        {
            pending.Clear();
            audio.StopLoop(GameAudioCue.UiSheenLoop, this);
            view?.Hide();
            pauseState?.Resume(PauseReason);
        }

        private void OnItemAdded(ItemAddedEvent itemAdded)
        {
            if (!itemAdded.HasItem)
            {
                return;
            }

            RewardRevealFrame frame = itemAdded.Source == ItemGrantSource.Chest
                ? RewardRevealFrame.ForChest(itemAdded.Definition, itemAdded.AddedStacks, itemAdded.CurrentStackCount)
                : RewardRevealFrame.ForItem(itemAdded.Definition, itemAdded.AddedStacks, itemAdded.CurrentStackCount);
            Enqueue(frame);
        }

        private void Enqueue(RewardRevealFrame frame)
        {
            if (!frame.HasReward)
            {
                return;
            }

            pending.Enqueue(frame);
            if (view == null || view.IsVisible)
            {
                return;
            }

            ShowNext();
        }

        private void DismissActive()
        {
            if (view == null || !view.IsVisible)
            {
                return;
            }

            view.Hide();
            audio.StopLoop(GameAudioCue.UiSheenLoop, this);
            if (pending.Count > 0)
            {
                ShowNext();
                return;
            }

            pauseState?.Resume(PauseReason);
        }

        private void ShowNext()
        {
            if (view == null || pending.Count <= 0)
            {
                return;
            }

            pauseState?.Pause(PauseReason);
            audio.StartLoop(GameAudioCue.UiSheenLoop, this, UnityEngine.Vector3.zero);
            haptics.Play(GameHapticsCue.ItemObtained);
            view.Show(pending.Dequeue());
        }
    }
}
