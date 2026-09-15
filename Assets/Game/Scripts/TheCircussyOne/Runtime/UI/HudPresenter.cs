using System;
using TheCircussyOne.Config;
using VContainer.Unity;
using TheCircussyOne.Rules;
using TheCircussyOne.Visuals;
using UnityEngine;

namespace TheCircussyOne.Runtime
{
    public sealed class HudPresenter : IStartable, ITickable, ITickableWhenPaused, IDisposable
    {
        private const int ShowtimeSubtitleSeed = 4101;
        private const int ActFinaleSubtitleSeed = 4102;
        private const int IntermissionSubtitleSeed = 4103;
        private const int CurtainCallSubtitleSeed = 4104;

        private readonly GameState _state;
        private readonly HudView _hud;
        private readonly IGameTime _time;
        private readonly IRunRestarter _restarter;
        private readonly RunCurrencyState _currency;
        private readonly UpgradeSelectionSystem _upgradeSelection;
        private readonly WeaponLoadout _weaponLoadout;
        private readonly ItemInventory _itemInventory;
        private readonly RunActScheduleState _runSchedule;
        private readonly RunScheduleConfig _runScheduleConfig;
        private readonly RunPhaseState _runPhase;
        private readonly RunSeedState _runSeedState;
        private readonly RunRetryService _retryService;
        private readonly IMainMenuReturner _mainMenuReturner;
        private readonly IGameAudio _audio;
        private int _lastKills;
        private int _lastTickets;
        private bool _initialValuesBound;
        private bool _hasPendingRunAnnouncementCue;
        private GameAudioCue _pendingRunAnnouncementCue;

        public HudPresenter(
            GameState state,
            HudView hud,
            IGameTime time,
            IRunRestarter restarter,
            RunCurrencyState currency = null,
            UpgradeSelectionSystem upgradeSelection = null,
            WeaponLoadout weaponLoadout = null,
            ItemInventory itemInventory = null,
            RunActScheduleState runSchedule = null,
            RunScheduleConfig runScheduleConfig = null,
            RunPhaseState runPhase = null,
            RunSeedState runSeedState = null,
            RunRetryService retryService = null,
            IMainMenuReturner mainMenuReturner = null,
            IGameAudio audio = null)
        {
            _state = state;
            _hud = hud;
            _time = time;
            _restarter = restarter;
            _currency = currency;
            _upgradeSelection = upgradeSelection;
            _weaponLoadout = weaponLoadout;
            _itemInventory = itemInventory;
            _runSchedule = runSchedule;
            _runScheduleConfig = runScheduleConfig;
            _runPhase = runPhase;
            _runSeedState = runSeedState;
            _retryService = retryService;
            _mainMenuReturner = mainMenuReturner;
            _audio = audio ?? NullGameAudio.Instance;
        }

        public void Start()
        {
            if (_hud == null)
            {
                return;
            }

            _state.HealthChanged += OnHealthChanged;
            _state.ExperienceChanged += OnExperienceChanged;
            _state.LevelChanged += OnLevelChanged;
            _state.KillsChanged += OnKillsChanged;
            _state.GameOver += OnGameOver;
            _hud.RestartRequested += OnRestartRequested;
            _hud.RunAnnouncementShown += OnRunAnnouncementShown;
            _hud.RunAnnouncementFadeoutStarted += OnRunAnnouncementFadeoutStarted;
            if (_currency != null)
            {
                _currency.TicketsChanged += OnTicketsChanged;
            }

            if (_upgradeSelection != null)
            {
                _upgradeSelection.ChoicesOpened += OnUpgradeChoicesOpened;
                _upgradeSelection.ChoicesClosed += OnUpgradeChoicesClosed;
            }

            if (_weaponLoadout != null)
            {
                _weaponLoadout.Changed += OnWeaponLoadoutChanged;
            }

            if (_itemInventory != null)
            {
                _itemInventory.Changed += OnItemInventoryChanged;
            }

            if (_runSchedule != null)
            {
                _runSchedule.ActStarted += OnActStarted;
                _runSchedule.ShowtimeStarted += OnShowtimeStarted;
                _runSchedule.ActFinaleBecameDue += OnActFinaleBecameDue;
            }

            if (_runPhase != null)
            {
                _runPhase.PhaseChanged += OnRunPhaseChanged;
            }

            OnHealthChanged(_state.Health, _state.MaxHealth);
            OnExperienceChanged(_state.Experience, _state.ExperienceTarget);
            OnLevelChanged(_state.Level);
            OnKillsChanged(_state.Kills);
            OnTicketsChanged(_currency?.Tickets ?? 0);
            RefreshWeaponSlots();
            RefreshItemStacks();
            RefreshRunTimer();
            if (_state.IsGameOver)
            {
                ShowFinalCurtain();
            }
            else
            {
                _hud.SetGameOverVisible(false);
            }

            _initialValuesBound = true;
        }

        public void Tick()
        {
            RefreshRunTimer();
            ReleaseXpHoldIfSelectionFinished();
            _hud?.TickHudFeedback(
                HudFeedbackTimeRules.GameDeltaTime(_time, UnityEngine.Time.unscaledDeltaTime),
                UnityEngine.Time.unscaledDeltaTime);
        }

        public void Dispose()
        {
            _state.HealthChanged -= OnHealthChanged;
            _state.ExperienceChanged -= OnExperienceChanged;
            _state.LevelChanged -= OnLevelChanged;
            _state.KillsChanged -= OnKillsChanged;
            _state.GameOver -= OnGameOver;
            if (_hud != null)
            {
                _hud.RestartRequested -= OnRestartRequested;
                _hud.RunAnnouncementShown -= OnRunAnnouncementShown;
                _hud.RunAnnouncementFadeoutStarted -= OnRunAnnouncementFadeoutStarted;
            }

            if (_currency != null)
            {
                _currency.TicketsChanged -= OnTicketsChanged;
            }

            if (_upgradeSelection != null)
            {
                _upgradeSelection.ChoicesOpened -= OnUpgradeChoicesOpened;
                _upgradeSelection.ChoicesClosed -= OnUpgradeChoicesClosed;
            }

            if (_weaponLoadout != null)
            {
                _weaponLoadout.Changed -= OnWeaponLoadoutChanged;
            }

            if (_itemInventory != null)
            {
                _itemInventory.Changed -= OnItemInventoryChanged;
            }

            if (_runSchedule != null)
            {
                _runSchedule.ActStarted -= OnActStarted;
                _runSchedule.ShowtimeStarted -= OnShowtimeStarted;
                _runSchedule.ActFinaleBecameDue -= OnActFinaleBecameDue;
            }

            if (_runPhase != null)
            {
                _runPhase.PhaseChanged -= OnRunPhaseChanged;
            }
        }

        private void OnHealthChanged(int current, int max)
        {
            _hud?.SetHealth(current, max);
        }

        private void OnExperienceChanged(int current, int target)
        {
            _hud?.SetExperience(current, target);
        }

        private void OnLevelChanged(int level)
        {
            if (level <= 1)
            {
                _hud?.EndXpLevelUpHold();
            }

            _hud?.SetLevel(level);
        }

        private void OnKillsChanged(int kills)
        {
            _hud?.SetKills(kills);
            if (_initialValuesBound && kills > _lastKills)
            {
                _hud?.PlayKillsPop();
            }

            _lastKills = kills;
        }

        private void OnTicketsChanged(int tickets)
        {
            _hud?.SetTickets(tickets);
            if (_initialValuesBound && tickets > _lastTickets)
            {
                _hud?.PlayTicketsPop();
            }

            _lastTickets = tickets;
        }

        private void OnGameOver()
        {
            ShowFinalCurtain();
        }

        private void OnActStarted(RunActStartedEvent act)
        {
            float delay = _hud != null && _hud.Config != null ? _hud.Config.actBannerDelaySeconds : 2f;
            ShowRunAnnouncement(
                RunActScheduleRules.ActRomanLabel(act.ActNumber),
                act.ActName,
                delay,
                GameAudioCue.AnnouncementAct);
        }

        private void OnShowtimeStarted(RunShowtimeEvent showtime)
        {
            ShowRunAnnouncement(
                AnnouncementText(_runScheduleConfig?.showtimeAnnouncementTitle, "SHOWTIME!"),
                PickSubtitle(
                    _runScheduleConfig?.showtimeAnnouncementSubtitles,
                    _runScheduleConfig?.showtimeAnnouncementSubtitle,
                    "A swarm enters the ring!",
                    ShowtimeSubtitleSeed,
                    showtime.ActNumber,
                    showtime.ShowtimeIndex,
                    SecondsSeed(showtime.ScheduledSeconds)),
                0f,
                GameAudioCue.AnnouncementShowtimeHeadliner);
        }

        private void OnActFinaleBecameDue(RunActFinaleEvent actFinale)
        {
            ShowRunAnnouncement(
                AnnouncementText(_runScheduleConfig?.actFinaleAnnouncementTitle, "HEADLINER!"),
                PickSubtitle(
                    _runScheduleConfig?.actFinaleAnnouncementSubtitles,
                    _runScheduleConfig?.actFinaleAnnouncementSubtitle,
                    "The Headliner enters!",
                    ActFinaleSubtitleSeed,
                    actFinale.ActNumber,
                    SecondsSeed(actFinale.ScheduledSeconds)),
                0f,
                GameAudioCue.AnnouncementShowtimeHeadliner);
        }

        private void ShowRunAnnouncement(string kicker, string title, float delaySeconds, GameAudioCue startCue)
        {
            if (_hud == null)
            {
                return;
            }

            _pendingRunAnnouncementCue = startCue;
            _hasPendingRunAnnouncementCue = true;
            _hud.ShowRunAnnouncement(kicker, title, delaySeconds);
        }

        private void OnRunAnnouncementShown()
        {
            if (!_hasPendingRunAnnouncementCue)
            {
                return;
            }

            _audio.Play(_pendingRunAnnouncementCue);
            _hasPendingRunAnnouncementCue = false;
        }

        private void OnRunAnnouncementFadeoutStarted()
        {
            _audio.Play(GameAudioCue.AnnouncementFadeout);
        }

        private void OnRunPhaseChanged(RunPhase previous, RunPhase next)
        {
            if (next == RunPhase.Intermission)
            {
                _hud?.SetTerminalOverlayVisible(false, string.Empty, string.Empty);
                _hud?.SetIntermissionVisible(
                    true,
                    AnnouncementText(_runScheduleConfig?.intermissionAnnouncementTitle, "INTERMISSION"),
                    FormatIntermissionSubtitle(_runPhase != null ? _runPhase.WorldIndex + 1 : (_runSchedule?.ActNumber ?? 0) + 1));
                return;
            }

            if (next == RunPhase.Finished)
            {
                _hud?.SetIntermissionVisible(false, string.Empty, string.Empty);
                _hud?.SetTerminalOverlayVisible(
                    true,
                    AnnouncementText(_runScheduleConfig?.curtainCallAnnouncementTitle, "CURTAIN CALL"),
                    PickSubtitle(
                        _runScheduleConfig?.curtainCallAnnouncementSubtitles,
                        _runScheduleConfig?.curtainCallAnnouncementSubtitle,
                        "The show is complete.",
                        CurtainCallSubtitleSeed,
                        _runPhase != null ? _runPhase.WorldIndex : 0),
                    "MAIN MENU",
                    BuildCurtainCallSummary("The show is complete."));
                return;
            }

            if (next == RunPhase.Failed)
            {
                ShowFinalCurtain();
                return;
            }

            if (next is RunPhase.PerformerSelection or RunPhase.WorldActive or RunPhase.NextWorldTransition)
            {
                _hud?.SetIntermissionVisible(false, string.Empty, string.Empty);
                _hud?.SetTerminalOverlayVisible(false, string.Empty, string.Empty);
            }
        }

        private void ShowFinalCurtain()
        {
            _hud?.SetIntermissionVisible(false, string.Empty, string.Empty);
            _hud?.SetTerminalOverlayVisible(
                true,
                "FINAL CURTAIN",
                "You were overwhelmed.",
                "MAIN MENU",
                BuildCurtainCallSummary(_state?.LastDamageCause));
        }

        private void OnWeaponLoadoutChanged()
        {
            RefreshWeaponSlots();
        }

        private void OnItemInventoryChanged()
        {
            RefreshItemStacks();
        }

        private void OnUpgradeChoicesOpened(System.Collections.Generic.IReadOnlyList<UpgradeChoice> _)
        {
            _hud?.BeginXpLevelUpHold();
        }

        private void OnUpgradeChoicesClosed()
        {
            if (_upgradeSelection == null || _upgradeSelection.PendingSelectionCount <= 0)
            {
                _hud?.EndXpLevelUpHold();
            }
        }

        private void OnRestartRequested()
        {
            bool isTerminalRun = (_state != null && _state.IsGameOver)
                || (_runPhase != null && (_runPhase.CurrentPhase == RunPhase.Failed || _runPhase.CurrentPhase == RunPhase.Finished));
            if (isTerminalRun && _mainMenuReturner != null)
            {
                _mainMenuReturner.ReturnToMainMenu();
                return;
            }

            _restarter?.RestartRun(RunRestartReason.StageSetup);
        }

        private CurtainCallSummaryFrame BuildCurtainCallSummary(string cause)
        {
            int actNumber = _runSchedule != null && _runSchedule.ActNumber > 0
                ? _runSchedule.ActNumber
                : _runPhase != null
                    ? Mathf.Max(1, _runPhase.WorldIndex)
                    : 1;
            string actName = _runSchedule != null && !string.IsNullOrWhiteSpace(_runSchedule.ActName)
                ? _runSchedule.ActName
                : "Unknown Act";
            string act = $"{RunActScheduleRules.ActRomanLabel(actNumber)}: {actName}";
            int weaponCount = _weaponLoadout?.Weapons?.Count ?? 0;
            int itemCount = _itemInventory?.Items?.Count ?? 0;
            return new CurtainCallSummaryFrame(
                FormatRunSummaryTime(_time != null ? _time.Time : 0f),
                act,
                string.IsNullOrWhiteSpace(cause) ? "Overwhelmed" : cause,
                (_state?.Kills ?? 0).ToString(),
                _state != null ? $"Level {_state.Level} (+{Mathf.Max(0, _state.Level - 1)})" : "Level 1 (+0)",
                (_currency?.Tickets ?? 0).ToString(),
                weaponCount.ToString(),
                itemCount.ToString(),
                (_state?.TotalDamageDealt ?? 0).ToString());
        }

        private static string FormatRunSummaryTime(float seconds)
        {
            int wholeSeconds = Mathf.Max(0, Mathf.FloorToInt(seconds));
            int minutes = wholeSeconds / 60;
            int remainder = wholeSeconds % 60;
            return $"{minutes:00}:{remainder:00}";
        }

        private void ReleaseXpHoldIfSelectionFinished()
        {
            if (_upgradeSelection != null && !_upgradeSelection.HasActiveSelection && _upgradeSelection.PendingSelectionCount <= 0)
            {
                _hud?.EndXpLevelUpHold();
            }
        }

        private void RefreshWeaponSlots()
        {
            _hud?.SetWeaponSlots(_weaponLoadout?.Weapons, _weaponLoadout?.MaxWeapons ?? WeaponLoadout.DefaultMaxWeapons);
        }

        private void RefreshItemStacks()
        {
            _hud?.SetItemStacks(_itemInventory?.Items);
        }

        private void RefreshRunTimer()
        {
            if (_hud == null)
            {
                return;
            }

            if (_runSchedule != null && _runSchedule.IsEncoreActive)
            {
                Color encoreColor = _hud.Config != null
                    ? _hud.Config.encoreTimerTextColor
                    : Color.red;
                _hud.SetTimerText(RunActScheduleRules.FormatEncoreTimer(_runSchedule), encoreColor);
                return;
            }

            if (_runSchedule != null && _runSchedule.HasActiveAct)
            {
                _hud.SetTimerText(RunActScheduleRules.FormatActTimer(_runSchedule));
                return;
            }

            _hud.SetRunTime(_time != null ? _time.Time : 0f);
        }

        private string FormatIntermissionSubtitle(int nextActNumber)
        {
            string actLabel = RunActScheduleRules.ActRomanLabel(nextActNumber);
            string format = PickSubtitle(
                _runScheduleConfig?.intermissionAnnouncementSubtitleFormats,
                _runScheduleConfig?.intermissionAnnouncementSubtitleFormat,
                "Preparing {0}...",
                IntermissionSubtitleSeed,
                nextActNumber);
            try
            {
                return string.Format(format, actLabel);
            }
            catch (FormatException)
            {
                return $"Preparing {actLabel}...";
            }
        }

        private static string AnnouncementText(string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }

        private string PickSubtitle(
            System.Collections.Generic.IReadOnlyList<string> variants,
            string legacyFallback,
            string defaultFallback,
            int seedKind,
            params int[] localSeeds)
        {
            string fallback = AnnouncementText(legacyFallback, defaultFallback);
            return RunActScheduleRules.PickSubtitleVariant(variants, fallback, AnnouncementSeed(seedKind, localSeeds));
        }

        private int AnnouncementSeed(int seedKind, params int[] localSeeds)
        {
            int[] seeds = new int[(localSeeds?.Length ?? 0) + 1];
            seeds[0] = seedKind;
            if (localSeeds != null)
            {
                for (int i = 0; i < localSeeds.Length; i++)
                {
                    seeds[i + 1] = localSeeds[i];
                }
            }

            return _runSeedState != null ? _runSeedState.Combine(seeds) : DeterministicSeed.Combine(seeds);
        }

        private static int SecondsSeed(float seconds)
        {
            return (int)Math.Round(seconds * 10f);
        }
    }

    internal static class HudFeedbackTimeRules
    {
        public static float GameDeltaTime(IGameTime time, float fallbackDeltaTime)
        {
            return Mathf.Max(0f, time != null ? time.DeltaTime : fallbackDeltaTime);
        }
    }
}
