using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;
using TheCircussyOne.Runtime;

namespace TheCircussyOne.Visuals
{
    [ExecuteAlways]
    [RequireComponent(typeof(UIDocument))]
    public sealed class HudView : MonoBehaviour
    {
        [SerializeField] private UIDocument document;
        [SerializeField] private StyleSheet styleSheet;
        [SerializeField] private HudVisualConfig hudConfig;

        private VisualElement root;
        private VisualElement hudRoot;
        private VisualElement xpStrip;
        private VisualElement xpTrack;
        private VisualElement xpSegments;
        private VisualElement experiencePulse;
        private VisualElement hudUnderRow;
        private VisualElement healthFill;
        private VisualElement healthCluster;
        private VisualElement healthTrack;
        private VisualElement healthSegments;
        private VisualElement experienceFill;
        private VisualElement gameOver;
        private VisualElement curtainCallPanel;
        private VisualElement intermissionOverlay;
        private Label healthLabel;
        private Label experienceLabel;
        private Label levelLabel;
        private VisualElement killsIcon;
        private Label killsLabel;
        private VisualElement ticketsIcon;
        private Label ticketsLabel;
        private VisualElement timerCluster;
        private VisualElement timerIcon;
        private Label timerLabel;
        private VisualElement actBanner;
        private Label actBannerKickerLabel;
        private Label actBannerTitleLabel;
        private Label intermissionTitleLabel;
        private Label intermissionSubtitleLabel;
        private Label interactionButtonGlyph;
        private Label interactionPromptLabel;
        private Label gameOverTitleLabel;
        private Label gameOverSubtitleLabel;
        private Label curtainCallTimeLabel;
        private Label curtainCallActLabel;
        private Label curtainCallCauseLabel;
        private Label curtainCallKillsLabel;
        private Label curtainCallLevelLabel;
        private Label curtainCallTicketsLabel;
        private Label curtainCallWeaponsLabel;
        private Label curtainCallItemsLabel;
        private Label curtainCallDamageLabel;
        private VisualElement killsCluster;
        private VisualElement ticketsCluster;
        private VisualElement weaponSlotRow;
        private VisualElement[] weaponSlots;
        private Label[] weaponSlotLevelLabels;
        private VisualElement itemStackColumn;
        private VisualElement interactionPrompt;
        private VisualElement interactionProgressFill;
        private VisualElement damageVignette;
        private HudCounterPopState killsPop;
        private HudCounterPopState ticketsPop;
        private readonly HudXpGradientDriver xpGradient = new();
        private readonly HudGameOverDriver gameOverDriver = new();
        private readonly HudExperienceProgressDriver experienceProgress = new();
        private readonly HudHealthProgressDriver healthProgress = new();
        private readonly UiPopupMotionDriver gameOverMotion = new();
        private HudActBannerPhase actBannerPhase;
        private float actBannerElapsedSeconds;
        private float actBannerActiveDelaySeconds;
        private bool terminalOverlayVisible;
        private bool intermissionOverlayVisible;
        private bool styleApplied;
        private VisualElement xpStripOriginalParent;
        private int xpStripOriginalIndex = -1;
        private bool xpStripPromoted;

        public event Action RestartRequested;
        public event Action RunAnnouncementShown;
        public event Action RunAnnouncementFadeoutStarted;
        public HudVisualConfig Config => hudConfig;
        public PanelSettings PanelSettings
        {
            get
            {
                Resolve();
                return document != null ? document.panelSettings : null;
            }
        }

        public bool IsTerminalOverlayVisible => terminalOverlayVisible;
        public bool IsIntermissionOverlayVisible => intermissionOverlayVisible;
        public bool HasDamageVignetteElement
        {
            get
            {
                Resolve();
                return damageVignette != null;
            }
        }

        private void Awake()
        {
            Resolve();
            ApplyConfig(hudConfig);
            ApplyConfiguredPreviewIfAllowed();
        }

        private void OnEnable()
        {
            Resolve();
            ApplyConfig(hudConfig);
            ApplyConfiguredPreviewIfAllowed();
        }

        private void OnDisable()
        {
            gameOverDriver.UnbindRestartButton();
            HideActBannerImmediate();
            SetIntermissionVisible(false, string.Empty, string.Empty);
        }

        private void OnDestroy()
        {
            xpGradient.Dispose(Application.isPlaying);
        }

        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                ApplyConfig(hudConfig);
                ApplyConfiguredPreviewIfAllowed();
            }
        }

        public void SetHealth(int current, int max)
        {
            Resolve();
            healthProgress.SetHealth(HealthProgressElements(), current, max, hudConfig, CurrentHudScale());
        }

        public void SetExperience(int current, int target)
        {
            Resolve();
            experienceProgress.SetExperience(ExperienceProgressElements(), current, target, hudConfig);

            // XP numbers are intentionally hidden in the compact HUD. The optional label is
            // kept in the UXML so a future config toggle can opt back into numeric XP.
        }

        public void SetLevel(int level)
        {
            Resolve();
            if (levelLabel != null)
            {
                levelLabel.text = Mathf.Max(1, level).ToString();
            }
        }

        public void SetKills(int kills)
        {
            Resolve();
            if (killsLabel != null)
            {
                killsLabel.text = kills.ToString();
            }
        }

        public void PlayKillsPop()
        {
            Resolve();
            HudCounterPopDriver.Start(ref killsPop, killsCluster, hudConfig != null ? hudConfig.killsCounterPop : HudCounterPopSettings.Default);
        }

        public void SetTickets(int tickets)
        {
            Resolve();
            if (ticketsLabel != null)
            {
                ticketsLabel.text = Mathf.Max(0, tickets).ToString();
            }
        }

        public void PlayTicketsPop()
        {
            Resolve();
            HudCounterPopDriver.Start(ref ticketsPop, ticketsCluster, hudConfig != null ? hudConfig.ticketsCounterPop : HudCounterPopSettings.Default);
        }

        public void SetWeaponSlots(IReadOnlyList<WeaponRuntime> weapons, int maxSlots)
        {
            Resolve();
            int resolvedSlotCount = Mathf.Clamp(maxSlots <= 0 ? WeaponLoadout.DefaultMaxWeapons : maxSlots, 0, weaponSlots?.Length ?? 0);
            for (int i = 0; i < (weaponSlots?.Length ?? 0); i++)
            {
                WeaponRuntime weapon = weapons != null && i < weapons.Count ? weapons[i] : null;
                bool visibleSlot = i < resolvedSlotCount;
                VisualElement slot = weaponSlots[i];
                Label levelLabel = weaponSlotLevelLabels != null && i < weaponSlotLevelLabels.Length ? weaponSlotLevelLabels[i] : null;
                HudWeaponSlotDriver.Apply(slot, levelLabel, weapon, visibleSlot);
            }
        }

        public void SetItemStacks(IReadOnlyList<ItemStackRuntime> items)
        {
            Resolve();
            if (itemStackColumn == null)
            {
                return;
            }

            itemStackColumn.Clear();
            if (items == null)
            {
                return;
            }

            for (int i = 0; i < items.Count; i++)
            {
                ItemStackRuntime item = items[i];
                if (item?.Definition == null)
                {
                    continue;
                }

                VisualElement slot = HudItemStackSlotFactory.Create(item, hudConfig, CurrentHudScale());
                itemStackColumn.Add(slot);
            }
        }

        public void TickCounterPops(float deltaTime)
        {
            Resolve();
            HudVisualConfig config = hudConfig;
            if (config == null)
            {
                HudCounterPopDriver.Reset(ref killsPop, killsCluster);
                HudCounterPopDriver.Reset(ref ticketsPop, ticketsCluster);
                return;
            }

            config.EnsureReadableDefaults();
            HudCounterPopDriver.Tick(ref killsPop, killsCluster, deltaTime, config.killsCounterPop);
            HudCounterPopDriver.Tick(ref ticketsPop, ticketsCluster, deltaTime, config.ticketsCounterPop);
        }

        public void TickHudFeedback(float deltaTime)
        {
            TickHudFeedback(deltaTime, deltaTime);
        }

        public void TickHudFeedback(float gameDeltaTime, float unscaledDeltaTime)
        {
            TickCounterPops(gameDeltaTime);
            TickBarTweens(gameDeltaTime);
            TickActBanner(gameDeltaTime);
            gameOverMotion.Tick(TerminalOverlayMotionTarget(), unscaledDeltaTime, PopupMotionSettings());
        }

        public void BeginXpLevelUpHold()
        {
            Resolve();
            PromoteXpStripToTopLayer();
            experienceProgress.BeginLevelUpHold(ExperienceProgressElements(), hudConfig);
        }

        public void EndXpLevelUpHold()
        {
            Resolve();
            experienceProgress.EndLevelUpHold(ExperienceProgressElements(), hudConfig);
            RestoreXpStripLayer();
        }

        public void SetRunTime(float seconds)
        {
            SetTimerText(FormatRunTime(seconds));
        }

        public void SetTimerText(string text, Color? colorOverride = null)
        {
            Resolve();
            if (timerLabel != null)
            {
                timerLabel.text = text ?? string.Empty;
                timerLabel.style.color = colorOverride ?? (hudConfig != null ? hudConfig.timerTextColor : Color.white);
            }
        }

        public void ShowActBanner(string actKicker, string actTitle)
        {
            ShowRunAnnouncement(actKicker, actTitle, hudConfig != null ? hudConfig.actBannerDelaySeconds : 0f);
        }

        public void ShowRunAnnouncement(string kicker, string title, float delaySeconds = 0f)
        {
            Resolve();
            HudVisualConfig config = hudConfig;
            if (actBanner == null || config == null || !config.actBannerEnabled)
            {
                HideActBannerImmediate();
                return;
            }

            config.EnsureReadableDefaults();
            if (actBannerKickerLabel != null)
            {
                actBannerKickerLabel.text = kicker ?? string.Empty;
            }

            if (actBannerTitleLabel != null)
            {
                actBannerTitleLabel.text = string.IsNullOrWhiteSpace(title) ? kicker ?? string.Empty : title;
            }

            actBannerElapsedSeconds = 0f;
            actBannerActiveDelaySeconds = Mathf.Max(0f, delaySeconds);
            actBannerPhase = actBannerActiveDelaySeconds > 0f ? HudActBannerPhase.Delay : HudActBannerPhase.Pop;
            if (actBannerPhase == HudActBannerPhase.Delay)
            {
                actBanner.style.display = DisplayStyle.None;
            }
            else
            {
                BeginActBannerPop(config);
            }
        }

        public void SetGameOverVisible(bool visible)
        {
            SetTerminalOverlayVisible(visible, "GAME OVER", "The run is over", "RESTART", default);
        }

        public void SetTerminalOverlayVisible(bool visible, string title, string subtitle, string restartText = "RESTART")
        {
            SetTerminalOverlayVisible(visible, title, subtitle, restartText, default);
        }

        public void SetTerminalOverlayVisible(
            bool visible,
            string title,
            string subtitle,
            string restartText,
            CurtainCallSummaryFrame summary)
        {
            Resolve();
            terminalOverlayVisible = visible;
            if (gameOverTitleLabel != null)
            {
                gameOverTitleLabel.text = string.IsNullOrWhiteSpace(title) ? "GAME OVER" : title;
            }

            if (gameOverSubtitleLabel != null)
            {
                gameOverSubtitleLabel.text = string.IsNullOrWhiteSpace(subtitle) ? string.Empty : subtitle;
            }

            Button restartButton = root?.Q<Button>("restart-button");
            if (restartButton != null)
            {
                restartButton.text = string.IsNullOrWhiteSpace(restartText) ? "RESTART" : restartText;
            }

            SetCurtainCallSummary(summary);

            if (Application.isPlaying && visible)
            {
                gameOverMotion.Show(gameOver, TerminalOverlayMotionTarget(), PopupMotionSettings());
                return;
            }

            gameOverMotion.HideImmediate(gameOver, TerminalOverlayMotionTarget());
            if (visible)
            {
                HudGameOverDriver.SetVisible(gameOver, true);
            }
        }

        private void SetCurtainCallSummary(CurtainCallSummaryFrame summary)
        {
            SetLabel(curtainCallTimeLabel, summary.Time);
            SetLabel(curtainCallActLabel, summary.Act);
            SetLabel(curtainCallCauseLabel, summary.Cause);
            SetLabel(curtainCallKillsLabel, summary.Kills);
            SetLabel(curtainCallLevelLabel, summary.Level);
            SetLabel(curtainCallTicketsLabel, summary.Tickets);
            SetLabel(curtainCallWeaponsLabel, summary.Weapons);
            SetLabel(curtainCallItemsLabel, summary.Items);
            SetLabel(curtainCallDamageLabel, summary.DamageDealt);
        }

        private static void SetLabel(Label label, string text)
        {
            if (label != null)
            {
                label.text = text ?? string.Empty;
            }
        }

        public void SetIntermissionVisible(bool visible, string title, string subtitle)
        {
            Resolve();
            intermissionOverlayVisible = visible;
            if (intermissionTitleLabel != null)
            {
                intermissionTitleLabel.text = string.IsNullOrWhiteSpace(title) ? "INTERMISSION" : title;
            }

            if (intermissionSubtitleLabel != null)
            {
                intermissionSubtitleLabel.text = string.IsNullOrWhiteSpace(subtitle) ? "Preparing next act..." : subtitle;
            }

            if (intermissionOverlay != null)
            {
                intermissionOverlay.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
                intermissionOverlay.style.opacity = visible ? 1f : 0f;
            }
        }

        public void SetInteractionPrompt(string prompt, float normalizedProgress, bool isHolding)
        {
            SetInteractionPrompt(prompt, normalizedProgress, isHolding, useGamepadGlyph: false);
        }

        public void SetInteractionPrompt(string prompt, float normalizedProgress, bool isHolding, bool useGamepadGlyph)
        {
            SetInteractionPrompt(prompt, normalizedProgress, isHolding, useGamepadGlyph, showGlyph: true);
        }

        public void SetInteractionPrompt(string prompt, float normalizedProgress, bool isHolding, bool useGamepadGlyph, bool showGlyph)
        {
            Resolve();
            HudInteractionPromptDriver.Apply(
                interactionPrompt,
                interactionButtonGlyph,
                interactionPromptLabel,
                interactionProgressFill,
                prompt,
                normalizedProgress,
                isHolding,
                useGamepadGlyph,
                showGlyph);
        }

        public void SetInteractionPromptVisible(bool visible)
        {
            Resolve();
            HudInteractionPromptDriver.SetVisible(interactionPrompt, interactionButtonGlyph, interactionProgressFill, visible);
        }

        public void ApplyConfig(HudVisualConfig config)
        {
            ApplyConfig(config, true);
        }

        public void ApplyConfig(HudVisualConfig config, bool persistReference)
        {
            if (persistReference)
            {
                hudConfig = config;
            }

            Resolve();
            if (config == null)
            {
                return;
            }

            config.EnsureReadableDefaults();
            float scale = Mathf.Clamp(config.globalScale, 0.25f, 3f);

            HudPreviewDriver.ApplySurfaceSize(root, config, Application.isPlaying);
            ApplyTypographyConfig(config);
            ApplyStatusConfig(config, scale);
            ApplyUnderRowConfig(scale);
            ApplyTimerConfig(config, scale);
            ApplyActBannerConfig(config, scale);
            ApplyKillsConfig(config, scale);
            ApplyEquipmentConfig(config, scale);
            ApplyGameOverConfig(config, scale);
        }

        public void ApplyPreview(HudPreviewSnapshot snapshot)
        {
            HudPreviewSnapshot clamped = snapshot.Clamped();
            SetHealthImmediate(clamped.CurrentHealth, clamped.MaxHealth);
            SetExperienceImmediate(clamped.CurrentExperience, clamped.TargetExperience);
            SetLevel(clamped.Level);
            SetKills(clamped.Kills);
            SetRunTime(clamped.RunSeconds);
            SetGameOverVisible(clamped.GameOverVisible);
        }

        public void ClearPreview()
        {
            ApplyPreview(HudPreviewSnapshot.Clear);
        }

        public void RequestRestart()
        {
            RestartRequested?.Invoke();
        }

        public static string FormatRunTime(float seconds)
        {
            int wholeSeconds = Mathf.Max(0, Mathf.FloorToInt(seconds));
            int minutes = wholeSeconds / 60;
            int remainder = wholeSeconds % 60;
            return $"{minutes:00}:{remainder:00}";
        }

        private void Resolve()
        {
            if (document == null)
            {
                document = GetComponent<UIDocument>();
            }

            if (document == null || document.rootVisualElement == null)
            {
                return;
            }

            root = document.rootVisualElement;
            HudLayerPickingRules.Apply(root);
            if (!styleApplied && styleSheet != null)
            {
                root.styleSheets.Add(styleSheet);
                styleApplied = true;
            }

            hudRoot ??= root.Q<VisualElement>("hud-root");
            xpStrip ??= root.Q<VisualElement>(className: "xp-strip");
            xpTrack ??= root.Q<VisualElement>(className: "xp-track");
            xpSegments ??= root.Q<VisualElement>("xp-segments");
            hudUnderRow ??= root.Q<VisualElement>(className: "hud-under-row");
            healthFill ??= root.Q<VisualElement>("health-fill");
            healthCluster ??= root.Q<VisualElement>(className: "health-cluster");
            healthTrack ??= root.Q<VisualElement>(className: "health-track");
            healthSegments ??= root.Q<VisualElement>("health-segments");
            experienceFill ??= root.Q<VisualElement>("xp-fill");
            experiencePulse ??= root.Q<VisualElement>("xp-pulse");
            gameOver ??= root.Q<VisualElement>("game-over");
            curtainCallPanel ??= root.Q<VisualElement>(className: "curtain-call-panel");
            intermissionOverlay ??= root.Q<VisualElement>("intermission-overlay");
            healthLabel ??= root.Q<Label>("health-value");
            experienceLabel ??= root.Q<Label>("xp-value");
            levelLabel ??= root.Q<Label>("level-value");
            killsIcon ??= root.Q<VisualElement>("kills-icon");
            killsLabel ??= root.Q<Label>("kills-value");
            ticketsIcon ??= root.Q<VisualElement>("tickets-icon");
            ticketsLabel ??= root.Q<Label>("tickets-value");
            timerCluster ??= root.Q<VisualElement>("timer-cluster");
            timerIcon ??= root.Q<VisualElement>("timer-icon");
            timerLabel ??= root.Q<Label>("timer-value");
            actBanner ??= root.Q<VisualElement>("act-banner");
            actBannerKickerLabel ??= root.Q<Label>("act-banner-kicker");
            actBannerTitleLabel ??= root.Q<Label>("act-banner-title");
            intermissionTitleLabel ??= root.Q<Label>("intermission-title");
            intermissionSubtitleLabel ??= root.Q<Label>("intermission-subtitle");
            killsCluster ??= root.Q<VisualElement>(className: "kills-cluster");
            ticketsCluster ??= root.Q<VisualElement>(className: "tickets-cluster");
            weaponSlotRow ??= root.Q<VisualElement>("weapon-slot-row");
            weaponSlots ??= QueryElements("weapon-slot-", 4);
            weaponSlotLevelLabels ??= QueryLabels("weapon-slot-level-", 4);
            itemStackColumn ??= root.Q<VisualElement>("item-stack-column");
            interactionPrompt ??= root.Q<VisualElement>("interaction-prompt");
            interactionButtonGlyph ??= root.Q<Label>("interaction-button-glyph");
            interactionPromptLabel ??= root.Q<Label>("interaction-prompt-label");
            interactionProgressFill ??= root.Q<VisualElement>("interaction-progress-fill");
            damageVignette ??= root.Q<VisualElement>("damage-vignette");
            if (damageVignette != null)
            {
                damageVignette.pickingMode = PickingMode.Ignore;
            }

            gameOverTitleLabel ??= root.Q<Label>(className: "game-over-title");
            gameOverSubtitleLabel ??= root.Q<Label>(className: "game-over-subtitle");
            curtainCallTimeLabel ??= root.Q<Label>("curtain-call-time");
            curtainCallActLabel ??= root.Q<Label>("curtain-call-act");
            curtainCallCauseLabel ??= root.Q<Label>("curtain-call-cause");
            curtainCallKillsLabel ??= root.Q<Label>("curtain-call-kills");
            curtainCallLevelLabel ??= root.Q<Label>("curtain-call-level");
            curtainCallTicketsLabel ??= root.Q<Label>("curtain-call-tickets");
            curtainCallWeaponsLabel ??= root.Q<Label>("curtain-call-weapons");
            curtainCallItemsLabel ??= root.Q<Label>("curtain-call-items");
            curtainCallDamageLabel ??= root.Q<Label>("curtain-call-damage");
            gameOverDriver.BindRestartButton(root.Q<Button>("restart-button"), RequestRestart);
        }

        private void ApplyConfiguredPreviewIfAllowed()
        {
            HudPreviewDriver.ApplyConfiguredPreviewIfAllowed(hudConfig, Application.isPlaying, ApplyPreview);
        }

        private void ApplyTypographyConfig(HudVisualConfig config)
        {
            if (root == null)
            {
                return;
            }

            if (config.uiFont != null)
            {
                root.style.unityFontDefinition = FontDefinition.FromFont(config.uiFont);
            }
            else
            {
                root.style.unityFontDefinition = StyleKeyword.Null;
            }
        }

        private void ApplyStatusConfig(HudVisualConfig config, float scale)
        {
            HudStatusLayoutDriver.Apply(
                new HudStatusLayoutElements(
                    xpStrip,
                    xpTrack,
                    xpSegments,
                    experienceFill,
                    experiencePulse,
                    levelLabel,
                    healthCluster,
                    healthTrack,
                    healthFill,
                    healthSegments,
                    healthLabel,
                    ticketsCluster,
                    ticketsIcon,
                    ticketsLabel),
                config,
                xpGradient,
                healthProgress.MaxHealthForSegments,
                scale);
        }

        private void ApplyUnderRowConfig(float scale)
        {
            if (hudUnderRow == null)
            {
                return;
            }

            hudUnderRow.style.left = 0f;
            hudUnderRow.style.right = 0f;
            hudUnderRow.style.top = 0f;
            hudUnderRow.style.height = Px(96f, scale);
        }

        private void SetHealthImmediate(int current, int max)
        {
            Resolve();
            healthProgress.SetImmediate(HealthProgressElements(), current, max, hudConfig, CurrentHudScale());
        }

        private void SetExperienceImmediate(int current, int target)
        {
            Resolve();
            experienceProgress.SetImmediate(ExperienceProgressElements(), current, target);
        }

        private void ApplyTimerConfig(HudVisualConfig config, float scale)
        {
            if (timerCluster == null || timerLabel == null)
            {
                return;
            }

            timerCluster.style.top = Px(config.timerTopOffset, scale);
            timerCluster.style.height = Px(config.timerHeight, scale);
            if (timerIcon != null)
            {
                timerIcon.style.width = Px(config.timerIconWidth, scale);
                timerIcon.style.minWidth = Px(config.timerIconWidth, scale);
                timerIcon.style.height = Px(config.timerIconHeight, scale);
                timerIcon.style.minHeight = Px(config.timerIconHeight, scale);
                timerIcon.style.marginRight = Px(config.timerIconSpacing, scale);
                timerIcon.style.position = Position.Relative;
                timerIcon.style.top = Px(config.timerIconYOffset, scale);
            }

            timerLabel.style.height = Px(config.timerHeight, scale);
            timerLabel.style.fontSize = Px(config.timerFontSize, scale);
            timerLabel.style.color = config.timerTextColor;
            timerLabel.style.unityTextAlign = config.timerTextAlign;
        }

        private void ApplyActBannerConfig(HudVisualConfig config, float scale)
        {
            if (actBanner == null)
            {
                return;
            }

            float width = Px(config.actBannerWidth, scale);
            actBanner.style.left = Length.Percent(50f);
            actBanner.style.marginLeft = -width * 0.5f;
            actBanner.style.top = Px(config.actBannerTopOffset, scale);
            actBanner.style.width = width;
            actBanner.style.height = Px(config.actBannerHeight, scale);
            actBanner.style.backgroundColor = Color.clear;
            actBanner.style.borderLeftWidth = 0f;
            actBanner.style.borderRightWidth = 0f;
            actBanner.style.borderTopWidth = 0f;
            actBanner.style.borderBottomWidth = 0f;

            if (actBannerPhase == HudActBannerPhase.Hidden)
            {
                actBanner.style.display = DisplayStyle.None;
            }

            if (actBannerKickerLabel != null)
            {
                actBannerKickerLabel.style.fontSize = Px(config.actBannerKickerFontSize, scale);
                actBannerKickerLabel.style.color = config.actBannerKickerColor;
                actBannerKickerLabel.style.unityTextOutlineColor = config.actBannerOutlineColor;
                actBannerKickerLabel.style.unityTextOutlineWidth = Px(config.actBannerKickerOutlineWidth, scale);
            }

            if (actBannerTitleLabel != null)
            {
                actBannerTitleLabel.style.marginTop = Px(config.actBannerTitleTopMargin, scale);
                actBannerTitleLabel.style.fontSize = Px(config.actBannerTitleFontSize, scale);
                actBannerTitleLabel.style.color = config.actBannerTitleColor;
                actBannerTitleLabel.style.unityTextOutlineColor = config.actBannerOutlineColor;
                actBannerTitleLabel.style.unityTextOutlineWidth = Px(config.actBannerTitleOutlineWidth, scale);
            }
        }

        private void ApplyKillsConfig(HudVisualConfig config, float scale)
        {
            if (killsCluster != null)
            {
                killsCluster.style.right = Px(config.killsOffset.x, scale);
                killsCluster.style.top = Px(config.killsOffset.y, scale);
                killsCluster.style.height = Px(config.killsHeight, scale);
            }

            if (killsIcon != null)
            {
                killsIcon.style.width = Px(config.killsIconWidth, scale);
                killsIcon.style.minWidth = Px(config.killsIconWidth, scale);
                killsIcon.style.height = Px(config.killsIconHeight, scale);
                killsIcon.style.minHeight = Px(config.killsIconHeight, scale);
                killsIcon.style.marginRight = Px(config.killsSpacing, scale);
                killsIcon.style.position = Position.Relative;
                killsIcon.style.top = Px(config.killsIconYOffset, scale);
            }

            if (killsLabel != null)
            {
                killsLabel.style.minWidth = Px(config.killsNumberMinWidth, scale);
                killsLabel.style.fontSize = Px(config.killsNumberFontSize, scale);
                killsLabel.style.color = config.killsNumberColor;
            }
        }

        private void ApplyEquipmentConfig(HudVisualConfig config, float scale)
        {
            HudEquipmentLayoutDriver.Apply(
                new HudEquipmentLayoutElements(weaponSlotRow, weaponSlots, weaponSlotLevelLabels, itemStackColumn),
                config,
                scale);
        }

        private void ApplyGameOverConfig(HudVisualConfig config, float scale)
        {
            HudGameOverDriver.Apply(
                new HudGameOverElements(gameOver, gameOverTitleLabel, gameOverSubtitleLabel),
                config,
                scale);
        }

        private VisualElement TerminalOverlayMotionTarget()
        {
            return curtainCallPanel ?? gameOver;
        }

        private static float Px(float value, float scale)
        {
            return value * scale;
        }

        private void TickBarTweens(float deltaTime)
        {
            healthProgress.Tick(HealthProgressElements(), deltaTime);
            experienceProgress.Tick(ExperienceProgressElements(), deltaTime, hudConfig);
        }

        private void TickActBanner(float deltaTime)
        {
            if (actBannerPhase == HudActBannerPhase.Hidden)
            {
                return;
            }

            HudVisualConfig config = hudConfig;
            if (actBanner == null || config == null || !config.actBannerEnabled)
            {
                HideActBannerImmediate();
                return;
            }

            config.EnsureReadableDefaults();
            actBannerElapsedSeconds += Mathf.Max(0f, deltaTime);
            switch (actBannerPhase)
            {
                case HudActBannerPhase.Delay:
                    if (actBannerElapsedSeconds < actBannerActiveDelaySeconds)
                    {
                        return;
                    }

                    actBannerElapsedSeconds = 0f;
                    BeginActBannerPop(config);
                    break;

                case HudActBannerPhase.Pop:
                    TickActBannerPop(config);
                    break;

                case HudActBannerPhase.Hold:
                    if (actBannerElapsedSeconds >= config.actBannerHoldSeconds)
                    {
                        actBannerElapsedSeconds = 0f;
                        BeginActBannerFadeout();
                    }
                    else
                    {
                        ApplyActBannerFrame(1f, 1f);
                    }

                    break;

                case HudActBannerPhase.Fade:
                    TickActBannerFade(config);
                    break;
            }
        }

        private void TickActBannerPop(HudVisualConfig config)
        {
            float duration = Mathf.Max(0.01f, config.actBannerPopSeconds);
            float normalized = Mathf.Clamp01(actBannerElapsedSeconds / duration);
            float eased = GameEasing.Evaluate01(config.actBannerPopEase, normalized);
            float scale = Mathf.LerpUnclamped(config.actBannerStartScale, 1f, eased);
            ApplyActBannerFrame(Mathf.Max(0.01f, scale), 1f);
            if (actBannerElapsedSeconds < duration)
            {
                return;
            }

            actBannerElapsedSeconds = 0f;
            actBannerPhase = HudActBannerPhase.Hold;
            ApplyActBannerFrame(1f, 1f);
        }

        private void BeginActBannerPop(HudVisualConfig config)
        {
            actBannerPhase = HudActBannerPhase.Pop;
            actBanner.style.display = DisplayStyle.Flex;
            ApplyActBannerFrame(config.actBannerStartScale, 1f);
            RunAnnouncementShown?.Invoke();
        }

        private void BeginActBannerFadeout()
        {
            actBannerPhase = HudActBannerPhase.Fade;
            RunAnnouncementFadeoutStarted?.Invoke();
        }

        private void TickActBannerFade(HudVisualConfig config)
        {
            float duration = Mathf.Max(0.01f, config.actBannerFadeSeconds);
            float normalized = Mathf.Clamp01(actBannerElapsedSeconds / duration);
            float eased = GameEasing.Evaluate01(config.actBannerFadeEase, normalized);
            float scale = Mathf.LerpUnclamped(1f, config.actBannerStartScale, eased);
            ApplyActBannerFrame(Mathf.Max(0.01f, scale), Mathf.Clamp01(1f - Mathf.Clamp01(eased)));
            if (actBannerElapsedSeconds < duration)
            {
                return;
            }

            HideActBannerImmediate();
        }

        private void ApplyActBannerFrame(float scale, float opacity)
        {
            if (actBanner == null)
            {
                return;
            }

            actBanner.style.scale = new Scale(new Vector3(scale, scale, 1f));
            actBanner.style.opacity = opacity;
        }

        private void HideActBannerImmediate()
        {
            actBannerPhase = HudActBannerPhase.Hidden;
            actBannerElapsedSeconds = 0f;
            if (actBanner == null)
            {
                return;
            }

            ApplyActBannerFrame(1f, 1f);
            actBanner.style.display = DisplayStyle.None;
        }

        private HudPopupMotionSettings PopupMotionSettings()
        {
            return hudConfig != null ? hudConfig.popupMotion : HudPopupMotionSettings.Default;
        }

        private float CurrentHudScale()
        {
            return hudConfig == null ? 1f : Mathf.Clamp(hudConfig.globalScale, 0.25f, 3f);
        }

        private void PromoteXpStripToTopLayer()
        {
            if (xpStrip == null || xpStripPromoted)
            {
                return;
            }

            VisualElement targetParent = hudRoot ?? root;
            if (targetParent == null)
            {
                return;
            }

            xpStripOriginalParent = xpStrip.parent;
            xpStripOriginalIndex = ChildIndexOf(xpStripOriginalParent, xpStrip);
            xpStrip.RemoveFromHierarchy();
            targetParent.Add(xpStrip);
            xpStrip.EnableInClassList("xp-strip-level-up-overlay", true);
            xpStripPromoted = true;
        }

        private void RestoreXpStripLayer()
        {
            if (xpStrip == null || !xpStripPromoted)
            {
                return;
            }

            xpStrip.EnableInClassList("xp-strip-level-up-overlay", false);
            xpStrip.RemoveFromHierarchy();
            if (xpStripOriginalParent != null)
            {
                int insertIndex = Mathf.Clamp(xpStripOriginalIndex, 0, xpStripOriginalParent.childCount);
                xpStripOriginalParent.Insert(insertIndex, xpStrip);
            }
            else
            {
                (hudRoot ?? root)?.Add(xpStrip);
            }

            xpStripOriginalParent = null;
            xpStripOriginalIndex = -1;
            xpStripPromoted = false;
        }

        private static int ChildIndexOf(VisualElement parent, VisualElement child)
        {
            if (parent == null || child == null)
            {
                return -1;
            }

            int index = 0;
            foreach (VisualElement candidate in parent.Children())
            {
                if (candidate == child)
                {
                    return index;
                }

                index++;
            }

            return -1;
        }

        private HudHealthProgressElements HealthProgressElements()
        {
            return new HudHealthProgressElements(healthFill, healthLabel, healthSegments);
        }

        private HudExperienceProgressElements ExperienceProgressElements()
        {
            return new HudExperienceProgressElements(experienceFill, experiencePulse);
        }

        private VisualElement[] QueryElements(string prefix, int count)
        {
            var elements = new VisualElement[count];
            if (root == null)
            {
                return elements;
            }

            for (int i = 0; i < count; i++)
            {
                elements[i] = root.Q<VisualElement>(prefix + i);
            }

            return elements;
        }

        private Label[] QueryLabels(string prefix, int count)
        {
            var labels = new Label[count];
            if (root == null)
            {
                return labels;
            }

            for (int i = 0; i < count; i++)
            {
                labels[i] = root.Q<Label>(prefix + i);
            }

            return labels;
        }

        private enum HudActBannerPhase
        {
            Hidden,
            Delay,
            Pop,
            Hold,
            Fade
        }
    }
}
