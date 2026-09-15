using System;
using System.Collections.Generic;
using TheCircussyOne.Rules;
using VContainer.Unity;

namespace TheCircussyOne.Runtime
{
    public sealed class UpgradeSelectionSystem : IStartable, IDisposable, IRunResettable
    {
        private const string PauseReason = RunPauseReasons.UpgradeSelection;

        private readonly GameState state;
        private readonly UpgradeRunState upgradeState;
        private readonly RunPauseState pauseState;
        private readonly UpgradeChoiceProvider choiceProvider;
        private readonly UpgradeEffectApplier effectApplier;
        private readonly Queue<int> pendingLevels = new();

        public UpgradeSelectionSystem(
            GameState state,
            UpgradeRunState upgradeState,
            RunPauseState pauseState,
            UpgradeChoiceProvider choiceProvider,
            UpgradeEffectApplier effectApplier)
        {
            this.state = state;
            this.upgradeState = upgradeState;
            this.pauseState = pauseState;
            this.choiceProvider = choiceProvider;
            this.effectApplier = effectApplier;
        }

        public event Action<IReadOnlyList<UpgradeChoice>> ChoicesOpened;
        public event Action ChoicesClosed;

        public bool HasActiveSelection => upgradeState.HasActiveChoices;
        public IReadOnlyList<UpgradeChoice> ActiveChoices => upgradeState.ActiveChoices;
        public int PendingSelectionCount => pendingLevels.Count + (HasActiveSelection ? 1 : 0);

        public void Start()
        {
            if (state != null)
            {
                state.LevelChanged += OnLevelChanged;
            }
        }

        public void Dispose()
        {
            if (state != null)
            {
                state.LevelChanged -= OnLevelChanged;
            }
        }

        public bool SelectChoice(int index)
        {
            if (!HasActiveSelection || index < 0 || index >= upgradeState.ActiveChoices.Count)
            {
                return false;
            }

            UpgradeChoice choice = upgradeState.ActiveChoices[index];
            if (!effectApplier.Apply(choice))
            {
                return false;
            }

            upgradeState.ClearActiveChoices();
            ChoicesClosed?.Invoke();
            TryOpenNextSelection();
            return true;
        }

        public void ResetRunState()
        {
            ResetRunState(new RunResetContext(RunResetKind.ReturnToPerformerSelection));
        }

        public void ResetRunState(RunResetContext context)
        {
            pendingLevels.Clear();
            bool hadActiveSelection = HasActiveSelection;
            upgradeState?.ClearActiveChoices();
            pauseState?.Resume(PauseReason);
            if (hadActiveSelection)
            {
                ChoicesClosed?.Invoke();
            }
        }

        private void OnLevelChanged(int level)
        {
            if (level <= 1 || state == null || state.IsGameOver)
            {
                return;
            }

            pendingLevels.Enqueue(level);
            TryOpenNextSelection();
        }

        private void TryOpenNextSelection()
        {
            if (HasActiveSelection)
            {
                return;
            }

            if (pendingLevels.Count == 0)
            {
                pauseState?.Resume(PauseReason);
                return;
            }

            int level = pendingLevels.Dequeue();
            List<UpgradeChoice> choices = choiceProvider.BuildChoices(level, pendingLevels.Count);

            if (choices.Count == 0)
            {
                TryOpenNextSelection();
                return;
            }

            upgradeState.SetActiveChoices(choices);
            pauseState?.Pause(PauseReason);
            ChoicesOpened?.Invoke(upgradeState.ActiveChoices);
        }
    }
}
