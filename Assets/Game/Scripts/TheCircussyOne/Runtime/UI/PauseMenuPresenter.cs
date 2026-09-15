using System;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using TheCircussyOne.Visuals;
using VContainer.Unity;

namespace TheCircussyOne.Runtime
{
    public sealed class PauseMenuPresenter : IStartable, ITickable, ITickableWhenPaused, IDisposable
    {
        private readonly IInputService input;
        private readonly RunPauseState pauseState;
        private readonly GameState state;
        private readonly IRunRestarter restarter;
        private readonly IMainMenuReturner mainMenuReturner;
        private readonly PauseMenuView view;
        private readonly RunDebugCommandService debugCommands;
        private readonly UpgradeSelectionView upgradeSelection;
        private readonly PerformerSelectionView performerSelection;
        private readonly RewardRevealView rewardReveal;
        private readonly ItemInventory itemInventory;
        private readonly RunWorldLayoutState worldLayoutState;

        private string status;

        public PauseMenuPresenter(
            IInputService input,
            RunPauseState pauseState,
            GameState state,
            IRunRestarter restarter,
            PauseMenuView view = null,
            RunDebugCommandService debugCommands = null,
            UpgradeSelectionView upgradeSelection = null,
            PerformerSelectionView performerSelection = null,
            RewardRevealView rewardReveal = null,
            ItemInventory itemInventory = null,
            RunWorldLayoutState worldLayoutState = null,
            IMainMenuReturner mainMenuReturner = null)
        {
            this.input = input;
            this.pauseState = pauseState;
            this.state = state;
            this.restarter = restarter;
            this.mainMenuReturner = mainMenuReturner;
            this.view = view;
            this.debugCommands = debugCommands;
            this.upgradeSelection = upgradeSelection;
            this.performerSelection = performerSelection;
            this.rewardReveal = rewardReveal;
            this.itemInventory = itemInventory;
            this.worldLayoutState = worldLayoutState;
        }

        public void Start()
        {
            if (view == null)
            {
                return;
            }

            view.ResumeRequested += Close;
            view.RestartRequested += Restart;
            view.AddTicketsRequested += AddTickets;
            view.AddExperienceRequested += AddExperience;
            view.HealRequested += Heal;
            view.DamageRequested += Damage;
            view.AddWeaponRequested += AddWeapon;
            view.RemoveWeaponRequested += RemoveWeapon;
            view.AddItemRequested += AddItem;
            view.RemoveItemRequested += RemoveItem;
            view.ApplyTalentRequested += ApplyTalent;
            view.ApplyUpgradeRequested += ApplyUpgrade;
            view.TriggerShowtimeRequested += TriggerShowtime;
            view.JumpToActFinaleRequested += JumpToActFinale;
            view.ActivateProxyHeadlinerRequested += ActivateProxyHeadliner;
            view.DefeatProxyHeadlinerRequested += DefeatProxyHeadliner;
            view.SkipEncoreGraceRequested += SkipEncoreGrace;
            view.CompleteStageDoorRequested += CompleteStageDoor;
            view.EnterNextActRequested += EnterNextAct;
            view.SwitchWorldLayoutModeRequested += SwitchWorldLayoutMode;
            if (itemInventory != null)
            {
                itemInventory.Changed += OnItemInventoryChanged;
            }

            view.Hide();
        }

        public void Tick()
        {
            view?.TickPopupMotion(UnityEngine.Time.unscaledDeltaTime);
            if (pauseState != null && pauseState.HasReason(RunPauseReasons.RunTransition))
            {
                return;
            }

            if (input == null || !input.PausePressedThisFrame)
            {
                return;
            }

            if (view != null && view.IsVisible)
            {
                Close();
                return;
            }

            if (CanOpen())
            {
                Open();
            }
        }

        public void Dispose()
        {
            if (view != null)
            {
                view.ResumeRequested -= Close;
                view.RestartRequested -= Restart;
                view.AddTicketsRequested -= AddTickets;
                view.AddExperienceRequested -= AddExperience;
                view.HealRequested -= Heal;
                view.DamageRequested -= Damage;
                view.AddWeaponRequested -= AddWeapon;
                view.RemoveWeaponRequested -= RemoveWeapon;
                view.AddItemRequested -= AddItem;
                view.RemoveItemRequested -= RemoveItem;
                view.ApplyTalentRequested -= ApplyTalent;
                view.ApplyUpgradeRequested -= ApplyUpgrade;
                view.TriggerShowtimeRequested -= TriggerShowtime;
                view.JumpToActFinaleRequested -= JumpToActFinale;
                view.ActivateProxyHeadlinerRequested -= ActivateProxyHeadliner;
                view.DefeatProxyHeadlinerRequested -= DefeatProxyHeadliner;
                view.SkipEncoreGraceRequested -= SkipEncoreGrace;
                view.CompleteStageDoorRequested -= CompleteStageDoor;
                view.EnterNextActRequested -= EnterNextAct;
                view.SwitchWorldLayoutModeRequested -= SwitchWorldLayoutMode;
                if (itemInventory != null)
                {
                    itemInventory.Changed -= OnItemInventoryChanged;
                }

                view.Hide();
            }

            pauseState?.Resume(RunPauseReasons.PauseMenu);
        }

        private bool CanOpen()
        {
            if (state != null && state.IsGameOver)
            {
                return false;
            }

            return !(upgradeSelection != null && upgradeSelection.IsVisible)
                && !(performerSelection != null && performerSelection.IsVisible)
                && !(rewardReveal != null && rewardReveal.IsVisible);
        }

        private void Open()
        {
            pauseState?.Pause(RunPauseReasons.PauseMenu);
            RefreshView();
        }

        private void Close()
        {
            view?.Hide();
            pauseState?.Resume(RunPauseReasons.PauseMenu);
        }

        private void Restart()
        {
            view?.Hide();
            pauseState?.Resume(RunPauseReasons.PauseMenu);
            if (mainMenuReturner != null)
            {
                mainMenuReturner.ReturnToMainMenu();
                return;
            }

            restarter?.RestartRun();
        }

        private void SwitchWorldLayoutMode()
        {
            if (worldLayoutState == null)
            {
                status = "World layout mode unavailable.";
                RefreshView();
                return;
            }

            RunWorldLayoutMode next = worldLayoutState.Mode == RunWorldLayoutMode.Generated
                ? RunWorldLayoutMode.Prototype
                : RunWorldLayoutMode.Generated;
            worldLayoutState.SetMode(next);
            status = $"World layout set to {WorldLayoutLabel(next)}. Returning to performer selection.";
            view?.Hide();
            pauseState?.Resume(RunPauseReasons.PauseMenu);
            restarter?.RestartRun();
        }

        private void AddTickets(int amount) => RunCommand("Added tickets.", () => debugCommands != null && debugCommands.AddTickets(amount));
        private void AddExperience(int amount) => RunCommand("Added XP.", () => debugCommands != null && debugCommands.AddExperience(amount));
        private void Heal(int amount) => RunCommand("Healed player.", () => debugCommands != null && debugCommands.HealPlayer(amount));
        private void Damage(int amount) => RunCommand("Damaged player.", () => debugCommands != null && debugCommands.DamagePlayer(amount));
        private void AddWeapon(string id) => RunCommand("Added weapon.", () => debugCommands != null && debugCommands.AddWeapon(id));
        private void RemoveWeapon(string id) => RunCommand("Removed weapon.", () => debugCommands != null && debugCommands.RemoveWeapon(id));
        private void AddItem(string id, int amount) => RunCommand("Added item.", () => debugCommands != null && debugCommands.AddItem(id, amount));
        private void RemoveItem(string id, int amount) => RunCommand("Removed item.", () => debugCommands != null && debugCommands.RemoveItem(id, amount));
        private void ApplyTalent(string id, ContentRarity rarity) => RunCommand("Applied talent.", () => debugCommands != null && debugCommands.ApplyTalent(id, rarity));
        private void ApplyUpgrade(string id, ContentRarity rarity) => RunCommand("Applied upgrade.", () => debugCommands != null && debugCommands.ApplyUpgrade(id, rarity));
        private void TriggerShowtime() => RunCommand("Triggered Showtime.", () => debugCommands != null && debugCommands.TriggerNextShowtime());
        private void JumpToActFinale() => RunCommand("Jumped to Headliner.", () => debugCommands != null && debugCommands.JumpToActFinale());
        private void ActivateProxyHeadliner() => RunCommand("Activated proxy Headliner.", () => debugCommands != null && debugCommands.ActivateProxyHeadliner());
        private void DefeatProxyHeadliner() => RunCommand("Defeated proxy Headliner.", () => debugCommands != null && debugCommands.DefeatProxyHeadliner());
        private void SkipEncoreGrace() => RunCommand("Skipped Encore grace.", () => debugCommands != null && debugCommands.SkipEncoreGrace());
        private void CompleteStageDoor() => RunCommand("Completed Stage Door.", () => debugCommands != null && debugCommands.CompleteStageDoor());
        private void EnterNextAct() => RunCommand("Entered next Act.", () => debugCommands != null && debugCommands.EnterNextAct());

        private void RunCommand(string successMessage, Func<bool> command)
        {
            bool success = command != null && command.Invoke();
            status = success ? successMessage : "Command could not be applied.";
            RefreshView();
        }

        private void RefreshView()
        {
            view?.Show(
                debugCommands != null ? debugCommands.BuildSnapshot() : default,
                status,
                ItemInventoryPanelRules.BuildFrames(itemInventory?.Items));
        }

        private void OnItemInventoryChanged()
        {
            if (view != null && view.IsVisible)
            {
                RefreshView();
            }
        }

        private static string WorldLayoutLabel(RunWorldLayoutMode mode)
        {
            return mode == RunWorldLayoutMode.Prototype ? "Prototype Arena" : "Generated Terrain";
        }
    }
}
