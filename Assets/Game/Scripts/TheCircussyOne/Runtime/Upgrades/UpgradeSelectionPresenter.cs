using System;
using System.Collections.Generic;
using TheCircussyOne.Rules;
using TheCircussyOne.Stats;
using TheCircussyOne.Visuals;
using VContainer.Unity;

namespace TheCircussyOne.Runtime
{
    public sealed class UpgradeSelectionPresenter : IStartable, ITickable, ITickableWhenPaused, IDisposable
    {
        private readonly UpgradeSelectionSystem system;
        private readonly UpgradeSelectionView view;
        private readonly IInputService input;
        private readonly RunStats stats;
        private readonly WeaponLoadout weaponLoadout;
        private readonly ItemInventory itemInventory;
        private readonly PerformerRunState performerRunState;
        private readonly IGameAudio audio;
        private readonly IGameHaptics haptics;
        private UpgradeNavigationRepeatState navigationRepeat;
        private UpgradeNavigationRepeatState cycleRepeat;

        public UpgradeSelectionPresenter(
            UpgradeSelectionSystem system,
            UpgradeSelectionView view,
            IInputService input,
            RunStats stats,
            WeaponLoadout weaponLoadout,
            ItemInventory itemInventory = null,
            PerformerRunState performerRunState = null,
            IGameAudio audio = null,
            IGameHaptics haptics = null)
        {
            this.system = system;
            this.view = view;
            this.input = input;
            this.stats = stats;
            this.weaponLoadout = weaponLoadout;
            this.itemInventory = itemInventory;
            this.performerRunState = performerRunState;
            this.audio = audio ?? NullGameAudio.Instance;
            this.haptics = haptics ?? NullGameHaptics.Instance;
        }

        public void Start()
        {
            if (system != null)
            {
                system.ChoicesOpened += OnChoicesOpened;
                system.ChoicesClosed += OnChoicesClosed;
            }

            if (view != null)
            {
                view.ChoiceSelected += OnChoiceSelected;
                view.Hide();
            }
        }

        public void Tick()
        {
            view?.TickPopupMotion(UnityEngine.Time.unscaledDeltaTime);
            if (system == null || view == null || !system.HasActiveSelection)
            {
                return;
            }

            int choice = input != null ? input.UpgradeChoicePressedThisFrame : 0;
            if (choice >= 1 && choice <= 3)
            {
                audio.Play(GameAudioCue.UiClick);
                haptics.Play(GameHapticsCue.UiClick);
                system.SelectChoice(choice - 1);
                return;
            }

            UiCycleNavigationDirection cycle = input != null ? input.UiCycleNavigation : UiCycleNavigationDirection.None;
            UpgradeNavigationDirection cycleNavigation = CycleNavigationAsUpgradeDirection(cycle);
            if (UpgradeNavigationRules.ShouldTrigger(ref cycleRepeat, cycleNavigation, UnityEngine.Time.unscaledTime))
            {
                int index = NextCycleIndex(view.HighlightedIndex, system.ActiveChoices.Count, cycle);
                if (index >= 0)
                {
                    HighlightWithAudio(index);
                }
            }

            UpgradeNavigationDirection navigation = input != null ? input.UpgradeNavigation : UpgradeNavigationDirection.None;
            if (UpgradeNavigationRules.ShouldTrigger(ref navigationRepeat, navigation, UnityEngine.Time.unscaledTime))
            {
                int index = UpgradeNavigationRules.NextIndex(view.HighlightedIndex, system.ActiveChoices.Count, navigation);
                if (index >= 0)
                {
                    HighlightWithAudio(index);
                }
            }

            float pulse = (UnityEngine.Mathf.Sin(UnityEngine.Time.unscaledTime * 7.5f) + 1f) * 0.5f;
            view.SetSelectionPulse(pulse);

            if (input != null && input.SubmitPressedThisFrame && view.HasHighlightedSelection)
            {
                audio.Play(GameAudioCue.UiClick);
                haptics.Play(GameHapticsCue.UiClick);
                system.SelectChoice(view.HighlightedIndex);
            }
        }

        public void Dispose()
        {
            if (system != null)
            {
                system.ChoicesOpened -= OnChoicesOpened;
                system.ChoicesClosed -= OnChoicesClosed;
            }

            if (view != null)
            {
                view.ChoiceSelected -= OnChoiceSelected;
            }

            audio.StopLoop(GameAudioCue.UiSheenLoop, this);
        }

        private void OnChoicesOpened(IReadOnlyList<UpgradeChoice> choices)
        {
            navigationRepeat = new UpgradeNavigationRepeatState { RequiresNeutralRelease = true };
            cycleRepeat = new UpgradeNavigationRepeatState { RequiresNeutralRelease = true };
            audio.StartLoop(GameAudioCue.UiSheenLoop, this, UnityEngine.Vector3.zero);
            view?.Show(
                choices,
                UpgradePreviewRules.BuildPreviews(choices, stats, weaponLoadout, itemInventory),
                performerRunState?.SelectedPerformer);
        }

        private void OnChoicesClosed()
        {
            audio.StopLoop(GameAudioCue.UiSheenLoop, this);
            view?.Hide();
        }

        private void OnChoiceSelected(int index)
        {
            system?.SelectChoice(index);
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
