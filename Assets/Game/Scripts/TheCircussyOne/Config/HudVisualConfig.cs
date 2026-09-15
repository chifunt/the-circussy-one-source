using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using TheCircussyOne.Rules;

namespace TheCircussyOne.Config
{
    public enum HudPreviewPreset
    {
        Normal,
        Damaged,
        Leveling,
        GameOver,
        Custom
    }

    public struct HudPreviewSnapshot
    {
        public HudPreviewSnapshot(
            int currentHealth,
            int maxHealth,
            int currentExperience,
            int targetExperience,
            int level,
            int kills,
            float runSeconds,
            bool gameOverVisible)
        {
            CurrentHealth = currentHealth;
            MaxHealth = maxHealth;
            CurrentExperience = currentExperience;
            TargetExperience = targetExperience;
            Level = level;
            Kills = kills;
            RunSeconds = runSeconds;
            GameOverVisible = gameOverVisible;
        }

        public int CurrentHealth;
        public int MaxHealth;
        public int CurrentExperience;
        public int TargetExperience;
        public int Level;
        public int Kills;
        public float RunSeconds;
        public bool GameOverVisible;

        public static HudPreviewSnapshot Clear => new(100, 100, 0, 1, 1, 0, 0f, false);

        public HudPreviewSnapshot Clamped()
        {
            int max = Mathf.Max(1, MaxHealth);
            int target = Mathf.Max(1, TargetExperience);
            return new HudPreviewSnapshot(
                Mathf.Clamp(CurrentHealth, 0, max),
                max,
                Mathf.Clamp(CurrentExperience, 0, target),
                target,
                Mathf.Max(1, Level),
                Mathf.Max(0, Kills),
                Mathf.Max(0f, RunSeconds),
                GameOverVisible);
        }
    }

    [CreateAssetMenu(menuName = "The Circussy One/HUD Visual Config", fileName = "HudVisualConfig")]
    [InfoBox("PROJECTED VISUAL: HUD tuning applies to the UI Toolkit panel and the open-scene HUD preview. Scene rebuild is not needed for normal HUD layout tuning.")]
    public sealed class HudVisualConfig : SerializedScriptableObject
    {
        private const string Tabs = "HUD Visuals";

        [TabGroup(Tabs, "Scaling"), BoxGroup(Tabs + "/Scaling/Workflow"), LabelWidth(190), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.VisualColor")]
        public bool autoApplyOnChange = true;

        [TabGroup(Tabs, "Scaling"), BoxGroup(Tabs + "/Scaling/Panel"), LabelWidth(190), EnumToggleButtons]
        public PanelScaleMode scaleMode = PanelScaleMode.ConstantPhysicalSize;
        [TabGroup(Tabs, "Scaling"), BoxGroup(Tabs + "/Scaling/Panel"), LabelWidth(190), SuffixLabel("DPI")]
        [InfoBox("Physical-size scaling depends on monitor-reported DPI. If a display lies, use Global Scale as the practical correction knob.")]
        [Min(1f)] public float referenceDpi = 96f;
        [TabGroup(Tabs, "Scaling"), BoxGroup(Tabs + "/Scaling/Panel"), LabelWidth(190), SuffixLabel("DPI")]
        [Min(1f)] public float fallbackDpi = 96f;
        [TabGroup(Tabs, "Scaling"), BoxGroup(Tabs + "/Scaling/Panel"), LabelWidth(190)]
        public Vector2 referenceResolution = new(1200f, 800f);
        [TabGroup(Tabs, "Scaling"), BoxGroup(Tabs + "/Scaling/Panel"), LabelWidth(190), NumericSlider(0.25f, 3f)]
        public float globalScale = 1f;
        [TabGroup(Tabs, "Scaling"), BoxGroup(Tabs + "/Scaling/Typography"), LabelWidth(190), AssetSelector, PreviewField(60)]
        [InfoBox("UI Toolkit uses this source font directly. Damage numbers use the matching generated TMP SDF asset in DamageFeedbackVisualConfig.")]
        public Font uiFont;

        [TabGroup(Tabs, "XP Bar"), BoxGroup(Tabs + "/XP Bar/Layout"), LabelWidth(190), SuffixLabel("px")]
        [Min(1f)] public float xpStripHeight = 22f;
        [TabGroup(Tabs, "XP Bar"), BoxGroup(Tabs + "/XP Bar/Layout"), LabelWidth(190), SuffixLabel("px")]
        [Min(0f)] public float xpTopPadding = 4f;
        [TabGroup(Tabs, "XP Bar"), BoxGroup(Tabs + "/XP Bar/Layout"), LabelWidth(190), SuffixLabel("px")]
        [Min(0f)] public float xpSideMargin = 8f;
        [TabGroup(Tabs, "XP Bar"), BoxGroup(Tabs + "/XP Bar/Layout"), LabelWidth(190), SuffixLabel("px")]
        [Min(1f)] public float xpTrackHeight = 16f;
        [TabGroup(Tabs, "XP Bar"), BoxGroup(Tabs + "/XP Bar/Layout"), LabelWidth(190), SuffixLabel("px")]
        [Min(0f)] public float xpCornerRadius = 4f;
        [TabGroup(Tabs, "XP Bar"), BoxGroup(Tabs + "/XP Bar/Level Badge"), LabelWidth(190), SuffixLabel("px")]
        [Min(1f)] public float levelBadgeWidth = 24f;
        [TabGroup(Tabs, "XP Bar"), BoxGroup(Tabs + "/XP Bar/Level Badge"), LabelWidth(190), SuffixLabel("px")]
        [Min(1f)] public float levelBadgeHeight = 24f;
        [TabGroup(Tabs, "XP Bar"), BoxGroup(Tabs + "/XP Bar/Level Badge"), LabelWidth(190), LabelText("Gap / Y Offset"), SuffixLabel("px")]
        public Vector2 levelBadgeOffset = new(6f, 0f);
        [TabGroup(Tabs, "XP Bar"), BoxGroup(Tabs + "/XP Bar/Level Badge"), LabelWidth(190), SuffixLabel("px")]
        [Min(0f)] public float levelBadgeHorizontalPadding = 0f;
        [TabGroup(Tabs, "XP Bar"), BoxGroup(Tabs + "/XP Bar/Level Badge"), LabelWidth(190), SuffixLabel("px")]
        [Min(1f)] public float levelBadgeFontSize = 13f;
        [TabGroup(Tabs, "XP Bar"), BoxGroup(Tabs + "/XP Bar/Animation"), LabelWidth(190), HideLabel, InlineProperty]
        public HudBarTweenSettings xpBarTween = HudBarTweenSettings.DefaultXp;
        [TabGroup(Tabs, "XP Bar"), BoxGroup(Tabs + "/XP Bar/Segments"), LabelWidth(190), Min(1)]
        public int xpSegmentCount = 10;
        [TabGroup(Tabs, "XP Bar"), BoxGroup(Tabs + "/XP Bar/Segments"), LabelWidth(190), SuffixLabel("px", true)]
        [Min(0f)] public float xpSegmentWidth = 1f;
        [TabGroup(Tabs, "XP Bar"), BoxGroup(Tabs + "/XP Bar/Segments"), LabelWidth(190), NumericSlider(0f, 1f)]
        public float xpSegmentOpacity = 0.34f;
        [TabGroup(Tabs, "XP Bar"), BoxGroup(Tabs + "/XP Bar/Level-Up Pulse"), LabelWidth(190), SuffixLabel("cycles/sec", true)]
        [Min(0f)] public float xpLevelUpPulseSpeed = 2.8f;
        [TabGroup(Tabs, "XP Bar"), BoxGroup(Tabs + "/XP Bar/Level-Up Pulse"), LabelWidth(190), NumericSlider(0f, 1f)]
        public float xpLevelUpPulseMaxOpacity = 0.58f;

        [TabGroup(Tabs, "Health"), BoxGroup(Tabs + "/Health/Layout"), LabelWidth(190), SuffixLabel("px")]
        [Min(1f)] public float healthWidth = 210f;
        [TabGroup(Tabs, "Health"), BoxGroup(Tabs + "/Health/Layout"), LabelWidth(190), SuffixLabel("px")]
        [Min(1f)] public float healthHeight = 22f;
        [TabGroup(Tabs, "Health"), BoxGroup(Tabs + "/Health/Layout"), LabelWidth(190), SuffixLabel("px")]
        public Vector2 healthOffset = new(16f, 28f);
        [TabGroup(Tabs, "Health"), BoxGroup(Tabs + "/Health/Layout"), LabelWidth(190), SuffixLabel("px")]
        [Min(0f)] public float healthCornerRadius = 5f;
        [TabGroup(Tabs, "Health"), BoxGroup(Tabs + "/Health/Layout"), LabelWidth(190), SuffixLabel("px")]
        [Min(1f)] public float healthFontSize = 12f;
        [TabGroup(Tabs, "Health"), BoxGroup(Tabs + "/Health/Layout"), LabelWidth(190), SuffixLabel("px")]
        public float healthTextVerticalOffset = -1f;
        [TabGroup(Tabs, "Health"), BoxGroup(Tabs + "/Health/Animation"), LabelWidth(190), HideLabel, InlineProperty]
        public HudBarTweenSettings healthBarTween = HudBarTweenSettings.DefaultHealth;
        [TabGroup(Tabs, "Health"), BoxGroup(Tabs + "/Health/Segments"), LabelWidth(190), SuffixLabel("HP", true)]
        [Min(1f)] public float healthHpPerSegment = 25f;
        [TabGroup(Tabs, "Health"), BoxGroup(Tabs + "/Health/Segments"), LabelWidth(190), SuffixLabel("px", true)]
        [Min(0f)] public float healthSegmentWidth = 1f;
        [TabGroup(Tabs, "Health"), BoxGroup(Tabs + "/Health/Segments"), LabelWidth(190), NumericSlider(0f, 1f)]
        public float healthSegmentOpacity = 0.38f;
        [TabGroup(Tabs, "Health"), BoxGroup(Tabs + "/Health/Segments"), LabelWidth(190), SuffixLabel("px", true)]
        [Min(0f)] public float healthMinimumSegmentPixelSpacing = 10f;

        [TabGroup(Tabs, "Timer"), BoxGroup(Tabs + "/Timer/Layout"), LabelWidth(190), SuffixLabel("px")]
        [Min(0f)] public float timerTopOffset = 28f;
        [TabGroup(Tabs, "Timer"), BoxGroup(Tabs + "/Timer/Layout"), LabelWidth(190), SuffixLabel("px")]
        [Min(1f)] public float timerHeight = 24f;
        [TabGroup(Tabs, "Timer"), BoxGroup(Tabs + "/Timer/Layout"), LabelWidth(190), SuffixLabel("px")]
        [FormerlySerializedAs("timerIconSize")]
        [Min(1f)] public float timerIconWidth = 20f;
        [TabGroup(Tabs, "Timer"), BoxGroup(Tabs + "/Timer/Layout"), LabelWidth(190), SuffixLabel("px")]
        [Min(1f)] public float timerIconHeight = 20f;
        [TabGroup(Tabs, "Timer"), BoxGroup(Tabs + "/Timer/Layout"), LabelWidth(190), SuffixLabel("px")]
        [Min(0f)] public float timerIconSpacing = 6f;
        [TabGroup(Tabs, "Timer"), BoxGroup(Tabs + "/Timer/Layout"), LabelWidth(190), LabelText("Icon Y Offset"), SuffixLabel("px")]
        public float timerIconYOffset = 0f;
        [TabGroup(Tabs, "Timer"), BoxGroup(Tabs + "/Timer/Layout"), LabelWidth(190), SuffixLabel("px")]
        [Min(1f)] public float timerFontSize = 14f;
        [TabGroup(Tabs, "Timer"), BoxGroup(Tabs + "/Timer/Layout"), LabelWidth(190)]
        public TextAnchor timerTextAlign = TextAnchor.UpperCenter;

        [TabGroup(Tabs, "Timer"), BoxGroup(Tabs + "/Timer/Act Banner"), LabelWidth(190)]
        public bool actBannerEnabled = true;
        [TabGroup(Tabs, "Timer"), BoxGroup(Tabs + "/Timer/Act Banner"), LabelWidth(190), SuffixLabel("px")]
        [Min(1f)] public float actBannerWidth = 420f;
        [TabGroup(Tabs, "Timer"), BoxGroup(Tabs + "/Timer/Act Banner"), LabelWidth(190), SuffixLabel("px")]
        [Min(1f)] public float actBannerHeight = 96f;
        [TabGroup(Tabs, "Timer"), BoxGroup(Tabs + "/Timer/Act Banner"), LabelWidth(190), SuffixLabel("px")]
        [InfoBox("Distance from the top edge of the HUD panel. Around 240-300 px keeps it near the upper middle of a 16:9 screen.")]
        [Min(0f)] public float actBannerTopOffset = 260f;
        [TabGroup(Tabs, "Timer"), BoxGroup(Tabs + "/Timer/Act Banner"), LabelWidth(190), SuffixLabel("px")]
        [Min(1f)] public float actBannerKickerFontSize = 16f;
        [TabGroup(Tabs, "Timer"), BoxGroup(Tabs + "/Timer/Act Banner"), LabelWidth(190), SuffixLabel("px")]
        [Min(1f)] public float actBannerTitleFontSize = 34f;
        [TabGroup(Tabs, "Timer"), BoxGroup(Tabs + "/Timer/Act Banner"), LabelWidth(190), SuffixLabel("px")]
        [Min(0f)] public float actBannerTitleTopMargin = 3f;
        [TabGroup(Tabs, "Timer"), BoxGroup(Tabs + "/Timer/Act Banner"), LabelWidth(190), SuffixLabel("sec", true)]
        [Min(0f)] public float actBannerDelaySeconds = 2f;
        [TabGroup(Tabs, "Timer"), BoxGroup(Tabs + "/Timer/Act Banner"), LabelWidth(190), SuffixLabel("sec", true)]
        [Min(0.01f)] public float actBannerPopSeconds = 0.2f;
        [TabGroup(Tabs, "Timer"), BoxGroup(Tabs + "/Timer/Act Banner"), LabelWidth(190), SuffixLabel("sec", true)]
        [Min(0.01f)] public float actBannerHoldSeconds = 4f;
        [TabGroup(Tabs, "Timer"), BoxGroup(Tabs + "/Timer/Act Banner"), LabelWidth(190), SuffixLabel("sec", true)]
        [Min(0.01f)] public float actBannerFadeSeconds = 0.22f;
        [TabGroup(Tabs, "Timer"), BoxGroup(Tabs + "/Timer/Act Banner"), LabelWidth(190), Min(0.01f)]
        public float actBannerStartScale = 0.08f;
        [TabGroup(Tabs, "Timer"), BoxGroup(Tabs + "/Timer/Act Banner"), LabelWidth(190), LabelText("Kicker Outline"), SuffixLabel("px")]
        [Min(0f)] public float actBannerKickerOutlineWidth = 2f;
        [TabGroup(Tabs, "Timer"), BoxGroup(Tabs + "/Timer/Act Banner"), LabelWidth(190), LabelText("Title Outline"), SuffixLabel("px")]
        [Min(0f)] public float actBannerTitleOutlineWidth = 3f;
        [TabGroup(Tabs, "Timer"), BoxGroup(Tabs + "/Timer/Act Banner"), LabelWidth(190), InlineProperty]
        public EaseSettings actBannerPopEase = EaseSettings.OutBounce;
        [TabGroup(Tabs, "Timer"), BoxGroup(Tabs + "/Timer/Act Banner"), LabelWidth(190), InlineProperty]
        public EaseSettings actBannerFadeEase = EaseSettings.InBack;

        [TabGroup(Tabs, "Kills"), BoxGroup(Tabs + "/Kills/Layout"), LabelWidth(190), SuffixLabel("px")]
        public Vector2 killsOffset = new(16f, 28f);
        [TabGroup(Tabs, "Kills"), BoxGroup(Tabs + "/Kills/Layout"), LabelWidth(190), SuffixLabel("px")]
        [Min(1f)] public float killsHeight = 24f;
        [TabGroup(Tabs, "Kills"), BoxGroup(Tabs + "/Kills/Layout"), LabelWidth(190), SuffixLabel("px")]
        [FormerlySerializedAs("killsIconSize")]
        [Min(1f)] public float killsIconWidth = 18f;
        [TabGroup(Tabs, "Kills"), BoxGroup(Tabs + "/Kills/Layout"), LabelWidth(190), SuffixLabel("px")]
        [Min(1f)] public float killsIconHeight = 18f;
        [TabGroup(Tabs, "Kills"), BoxGroup(Tabs + "/Kills/Layout"), LabelWidth(190), LabelText("Icon Y Offset"), SuffixLabel("px")]
        public float killsIconYOffset = 0f;
        [TabGroup(Tabs, "Kills"), BoxGroup(Tabs + "/Kills/Layout"), LabelWidth(190)]
        public string killsLabelText = "KILLS";
        [TabGroup(Tabs, "Kills"), BoxGroup(Tabs + "/Kills/Layout"), LabelWidth(190), SuffixLabel("px")]
        [Min(1f)] public float killsLabelFontSize = 12f;
        [TabGroup(Tabs, "Kills"), BoxGroup(Tabs + "/Kills/Layout"), LabelWidth(190), SuffixLabel("px")]
        [Min(1f)] public float killsNumberFontSize = 18f;
        [TabGroup(Tabs, "Kills"), BoxGroup(Tabs + "/Kills/Layout"), LabelWidth(190), SuffixLabel("px")]
        [Min(0f)] public float killsSpacing = 7f;
        [TabGroup(Tabs, "Kills"), BoxGroup(Tabs + "/Kills/Layout"), LabelWidth(190), SuffixLabel("px")]
        [Min(1f)] public float killsNumberMinWidth = 32f;

        [TabGroup(Tabs, "Tickets"), BoxGroup(Tabs + "/Tickets/Layout"), LabelWidth(190), SuffixLabel("px")]
        public Vector2 ticketsOffsetFromHealth = new(14f, 0f);
        [TabGroup(Tabs, "Tickets"), BoxGroup(Tabs + "/Tickets/Layout"), LabelWidth(190), SuffixLabel("px")]
        [Min(1f)] public float ticketsHeight = 24f;
        [TabGroup(Tabs, "Tickets"), BoxGroup(Tabs + "/Tickets/Layout"), LabelWidth(190), SuffixLabel("px")]
        [FormerlySerializedAs("ticketsIconSize")]
        [Min(1f)] public float ticketsIconWidth = 30f;
        [TabGroup(Tabs, "Tickets"), BoxGroup(Tabs + "/Tickets/Layout"), LabelWidth(190), SuffixLabel("px")]
        [Min(1f)] public float ticketsIconHeight = 18f;
        [TabGroup(Tabs, "Tickets"), BoxGroup(Tabs + "/Tickets/Layout"), LabelWidth(190), LabelText("Icon Y Offset"), SuffixLabel("px")]
        public float ticketsIconYOffset = 0f;
        [TabGroup(Tabs, "Tickets"), BoxGroup(Tabs + "/Tickets/Layout"), LabelWidth(190)]
        public string ticketsLabelText = "TICKETS";
        [TabGroup(Tabs, "Tickets"), BoxGroup(Tabs + "/Tickets/Layout"), LabelWidth(190), SuffixLabel("px")]
        [Min(1f)] public float ticketsLabelFontSize = 12f;
        [TabGroup(Tabs, "Tickets"), BoxGroup(Tabs + "/Tickets/Layout"), LabelWidth(190), SuffixLabel("px")]
        [Min(1f)] public float ticketsNumberFontSize = 18f;
        [TabGroup(Tabs, "Tickets"), BoxGroup(Tabs + "/Tickets/Layout"), LabelWidth(190), SuffixLabel("px")]
        [Min(0f)] public float ticketsSpacing = 7f;
        [TabGroup(Tabs, "Tickets"), BoxGroup(Tabs + "/Tickets/Layout"), LabelWidth(190), SuffixLabel("px")]
        [Min(1f)] public float ticketsNumberMinWidth = 42f;

        [TabGroup(Tabs, "Equipment"), BoxGroup(Tabs + "/Equipment/Weapon Slots"), LabelWidth(190), LabelText("Offset From Level Badge"), SuffixLabel("px")]
        [InfoBox("Weapon slot offset is relative to the top-left of the level badge. X moves the row sideways, Y moves it below/above the level badge.")]
        [FormerlySerializedAs("weaponSlotsOffsetFromHealth")]
        public Vector2 weaponSlotsOffsetFromLevelBadge = new(0f, 8f);
        [TabGroup(Tabs, "Equipment"), BoxGroup(Tabs + "/Equipment/Weapon Slots"), LabelWidth(190), SuffixLabel("px")]
        [Min(1f)] public float weaponSlotRowHeight = 46f;
        [TabGroup(Tabs, "Equipment"), BoxGroup(Tabs + "/Equipment/Weapon Slots"), LabelWidth(190), SuffixLabel("px")]
        [Min(1f)] public float weaponSlotSize = 30f;
        [TabGroup(Tabs, "Equipment"), BoxGroup(Tabs + "/Equipment/Weapon Slots"), LabelWidth(190), SuffixLabel("px")]
        [Min(0f)] public float weaponSlotGap = 5f;
        [TabGroup(Tabs, "Equipment"), BoxGroup(Tabs + "/Equipment/Weapon Slots"), LabelWidth(190), SuffixLabel("px")]
        [Min(0f)] public float weaponSlotCornerRadius = 4f;
        [TabGroup(Tabs, "Equipment"), BoxGroup(Tabs + "/Equipment/Weapon Level Badge"), LabelWidth(190), LabelText("Offset Below Slot"), SuffixLabel("px")]
        [InfoBox("Level badge sits below each weapon slot. X moves it from the slot center, Y controls distance below the slot frame.")]
        public Vector2 weaponSlotLevelOffset = new(0f, 2f);
        [TabGroup(Tabs, "Equipment"), BoxGroup(Tabs + "/Equipment/Weapon Level Badge"), LabelWidth(190), SuffixLabel("px")]
        [Min(1f)] public float weaponSlotLevelMinWidth = 13f;
        [TabGroup(Tabs, "Equipment"), BoxGroup(Tabs + "/Equipment/Weapon Level Badge"), LabelWidth(190), SuffixLabel("px")]
        [Min(1f)] public float weaponSlotLevelHeight = 12f;
        [TabGroup(Tabs, "Equipment"), BoxGroup(Tabs + "/Equipment/Weapon Level Badge"), LabelWidth(190), SuffixLabel("px")]
        [Min(1f)] public float weaponSlotLevelFontSize = 8f;

        [TabGroup(Tabs, "Equipment"), BoxGroup(Tabs + "/Equipment/Item Stack"), LabelWidth(190), LabelText("Offset From Kills"), SuffixLabel("px")]
        [InfoBox("Item stack offset is relative to the kills counter. X moves inward from the right edge, Y moves below/above the kills counter.")]
        public Vector2 itemStackOffsetFromKills = new(0f, 8f);
        [TabGroup(Tabs, "Equipment"), BoxGroup(Tabs + "/Equipment/Item Stack"), LabelWidth(190), SuffixLabel("px")]
        [Min(1f)] public float itemStackColumnWidth = 36f;
        [TabGroup(Tabs, "Equipment"), BoxGroup(Tabs + "/Equipment/Item Stack"), LabelWidth(190), SuffixLabel("px")]
        [Min(1f)] public float itemStackSlotSize = 32f;
        [TabGroup(Tabs, "Equipment"), BoxGroup(Tabs + "/Equipment/Item Stack"), LabelWidth(190), SuffixLabel("px")]
        [Min(0f)] public float itemStackGap = 5f;
        [TabGroup(Tabs, "Equipment"), BoxGroup(Tabs + "/Equipment/Item Stack"), LabelWidth(190), SuffixLabel("px")]
        [Min(0f)] public float itemStackCornerRadius = 4f;
        [TabGroup(Tabs, "Equipment"), BoxGroup(Tabs + "/Equipment/Item Count Badge"), LabelWidth(190), LabelText("Offset From Slot Edge"), SuffixLabel("px")]
        public Vector2 itemStackCountOffset = new(1f, 0f);
        [TabGroup(Tabs, "Equipment"), BoxGroup(Tabs + "/Equipment/Item Count Badge"), LabelWidth(190), SuffixLabel("px")]
        [Min(1f)] public float itemStackCountMinWidth = 13f;
        [TabGroup(Tabs, "Equipment"), BoxGroup(Tabs + "/Equipment/Item Count Badge"), LabelWidth(190), SuffixLabel("px")]
        [Min(1f)] public float itemStackCountHeight = 12f;
        [TabGroup(Tabs, "Equipment"), BoxGroup(Tabs + "/Equipment/Item Count Badge"), LabelWidth(190), SuffixLabel("px")]
        [Min(1f)] public float itemStackCountFontSize = 8f;

        [TabGroup(Tabs, "Feedback"), BoxGroup(Tabs + "/Feedback/Kills Counter Pop"), LabelWidth(190), HideLabel, InlineProperty]
        public HudCounterPopSettings killsCounterPop = HudCounterPopSettings.Default;
        [TabGroup(Tabs, "Feedback"), BoxGroup(Tabs + "/Feedback/Tickets Counter Pop"), LabelWidth(190), HideLabel, InlineProperty]
        public HudCounterPopSettings ticketsCounterPop = HudCounterPopSettings.Default;
        [TabGroup(Tabs, "Feedback"), BoxGroup(Tabs + "/Feedback/Popup Motion"), LabelWidth(190), HideLabel, InlineProperty]
        public HudPopupMotionSettings popupMotion = HudPopupMotionSettings.Default;
        [TabGroup(Tabs, "Feedback"), BoxGroup(Tabs + "/Feedback/Reward Reveal Accept"), LabelWidth(190), SuffixLabel("sec", true)]
        [InfoBox("Delay before the item obtained accept button becomes visible and selectable. This prevents the same confirm input that opened the reward from instantly closing it.")]
        [Min(0f)] public float rewardRevealAcceptDelaySeconds = 0.5f;
        [TabGroup(Tabs, "Feedback"), BoxGroup(Tabs + "/Feedback/Reward Reveal Accept"), LabelWidth(190), HideLabel, InlineProperty]
        public HudPopupMotionSettings rewardRevealAcceptMotion = new(true, 0.2f, 0.08f, 1f, true, EaseSettings.OutBounce);
        [TabGroup(Tabs, "Feedback"), BoxGroup(Tabs + "/Feedback/Run Transition"), LabelWidth(190), HideLabel, InlineProperty]
        public HudTransitionSettings runTransition = HudTransitionSettings.Default;

        [TabGroup(Tabs, "Game Over"), BoxGroup(Tabs + "/Game Over/Layout"), LabelWidth(190), SuffixLabel("px")]
        [Min(1f)] public float gameOverWidth = 310f;
        [TabGroup(Tabs, "Game Over"), BoxGroup(Tabs + "/Game Over/Layout"), LabelWidth(190), SuffixLabel("px")]
        [Min(1f)] public float gameOverHeight = 128f;
        [TabGroup(Tabs, "Game Over"), BoxGroup(Tabs + "/Game Over/Layout"), LabelWidth(190), SuffixLabel("px")]
        public Vector2 gameOverCenterOffset = Vector2.zero;
        [TabGroup(Tabs, "Game Over"), BoxGroup(Tabs + "/Game Over/Layout"), LabelWidth(190), SuffixLabel("px")]
        [Min(0f)] public float gameOverCornerRadius = 8f;
        [TabGroup(Tabs, "Game Over"), BoxGroup(Tabs + "/Game Over/Text"), LabelWidth(190), SuffixLabel("px")]
        [Min(1f)] public float gameOverTitleFontSize = 34f;
        [TabGroup(Tabs, "Game Over"), BoxGroup(Tabs + "/Game Over/Text"), LabelWidth(190), SuffixLabel("px")]
        [Min(1f)] public float gameOverSubtitleFontSize = 13f;
        [TabGroup(Tabs, "Game Over"), BoxGroup(Tabs + "/Game Over/Text"), LabelWidth(190), SuffixLabel("px")]
        [Min(0f)] public float gameOverSubtitleTopMargin = 8f;

        [TabGroup(Tabs, "Preview"), BoxGroup(Tabs + "/Preview/Workflow"), LabelWidth(190), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.VisualColor")]
        public bool previewEnabled = true;
        [TabGroup(Tabs, "Preview"), BoxGroup(Tabs + "/Preview/Workflow"), LabelWidth(190), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.VisualColor")]
        public bool autoPreviewOnChange = true;
        [TabGroup(Tabs, "Preview"), BoxGroup(Tabs + "/Preview/Workflow"), LabelWidth(190), EnumToggleButtons]
        public HudPreviewPreset previewPreset = HudPreviewPreset.Normal;

        [TabGroup(Tabs, "Preview"), BoxGroup(Tabs + "/Preview/Presets"), ButtonGroup(Tabs + "/Preview/Presets/Buttons"), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.VisualColor")]
        public void PreviewNormal() => SetPreviewPreset(HudPreviewPreset.Normal);
        [TabGroup(Tabs, "Preview"), BoxGroup(Tabs + "/Preview/Presets"), ButtonGroup(Tabs + "/Preview/Presets/Buttons"), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.BalanceColor")]
        public void PreviewDamaged() => SetPreviewPreset(HudPreviewPreset.Damaged);
        [TabGroup(Tabs, "Preview"), BoxGroup(Tabs + "/Preview/Presets"), ButtonGroup(Tabs + "/Preview/Presets/Buttons"), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.PlayerColor")]
        public void PreviewLeveling() => SetPreviewPreset(HudPreviewPreset.Leveling);
        [TabGroup(Tabs, "Preview"), BoxGroup(Tabs + "/Preview/Presets"), ButtonGroup(Tabs + "/Preview/Presets/Buttons"), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.WarningColor")]
        public void PreviewGameOver() => SetPreviewPreset(HudPreviewPreset.GameOver);
        [TabGroup(Tabs, "Preview"), BoxGroup(Tabs + "/Preview/Presets"), ButtonGroup(Tabs + "/Preview/Presets/Clear"), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.WarningColor")]
        public void ClearPreview() => previewEnabled = false;

        [TabGroup(Tabs, "Preview"), BoxGroup(Tabs + "/Preview/Custom Values"), LabelWidth(190), ShowIf(nameof(IsCustomPreview))]
        [Min(0)] public int previewHealth = 78;
        [TabGroup(Tabs, "Preview"), BoxGroup(Tabs + "/Preview/Custom Values"), LabelWidth(190), ShowIf(nameof(IsCustomPreview))]
        [Min(1)] public int previewMaxHealth = 100;
        [TabGroup(Tabs, "Preview"), BoxGroup(Tabs + "/Preview/Custom Values"), LabelWidth(190), ShowIf(nameof(IsCustomPreview))]
        [Min(0)] public int previewExperience = 6;
        [TabGroup(Tabs, "Preview"), BoxGroup(Tabs + "/Preview/Custom Values"), LabelWidth(190), ShowIf(nameof(IsCustomPreview))]
        [Min(1)] public int previewExperienceTarget = 10;
        [TabGroup(Tabs, "Preview"), BoxGroup(Tabs + "/Preview/Custom Values"), LabelWidth(190), ShowIf(nameof(IsCustomPreview))]
        [Min(1)] public int previewLevel = 3;
        [TabGroup(Tabs, "Preview"), BoxGroup(Tabs + "/Preview/Custom Values"), LabelWidth(190), ShowIf(nameof(IsCustomPreview))]
        [Min(0)] public int previewKills = 42;
        [TabGroup(Tabs, "Preview"), BoxGroup(Tabs + "/Preview/Custom Values"), LabelWidth(190), SuffixLabel("sec"), ShowIf(nameof(IsCustomPreview))]
        [Min(0f)] public float previewRunSeconds = 154f;
        [TabGroup(Tabs, "Preview"), BoxGroup(Tabs + "/Preview/Custom Values"), LabelWidth(190), ShowIf(nameof(IsCustomPreview))]
        public bool previewGameOverVisible;

        [TabGroup(Tabs, "Colors"), BoxGroup(Tabs + "/Colors/XP"), LabelWidth(190)] public Color xpTrackColor = new(0.016f, 0.063f, 0.094f, 0.86f);
        [TabGroup(Tabs, "Colors"), BoxGroup(Tabs + "/Colors/XP"), LabelWidth(190)] public Color xpFillColor = new(0.192f, 0.843f, 1f, 1f);
        [TabGroup(Tabs, "Colors"), BoxGroup(Tabs + "/Colors/XP"), LabelWidth(190)] public Color xpFillGradientStartColor = new(0.09f, 0.49f, 1f, 1f);
        [TabGroup(Tabs, "Colors"), BoxGroup(Tabs + "/Colors/XP"), LabelWidth(190)] public Color xpFillGradientEndColor = new(0.56f, 1f, 0.95f, 1f);
        [TabGroup(Tabs, "Colors"), BoxGroup(Tabs + "/Colors/XP"), LabelWidth(190)] public Color xpBorderColor = new(0.353f, 0.902f, 1f, 0.46f);
        [TabGroup(Tabs, "Colors"), BoxGroup(Tabs + "/Colors/XP"), LabelWidth(190)] public Color xpSegmentColor = new(0.82f, 1f, 1f, 1f);
        [TabGroup(Tabs, "Colors"), BoxGroup(Tabs + "/Colors/XP"), LabelWidth(190)] public Color xpLevelUpPulseColor = Color.white;
        [TabGroup(Tabs, "Colors"), BoxGroup(Tabs + "/Colors/XP"), LabelWidth(190)] public Color levelBadgeColor = new(0.006f, 0.012f, 0.018f, 0.94f);
        [TabGroup(Tabs, "Colors"), BoxGroup(Tabs + "/Colors/XP"), LabelWidth(190)] public Color levelBadgeTextColor = new(0.192f, 0.843f, 1f, 1f);

        [TabGroup(Tabs, "Colors"), BoxGroup(Tabs + "/Colors/Health"), LabelWidth(190)] public Color healthTrackColor = new(0.137f, 0.027f, 0.039f, 0.82f);
        [TabGroup(Tabs, "Colors"), BoxGroup(Tabs + "/Colors/Health"), LabelWidth(190)] public Color healthFillColor = new(0.91f, 0.161f, 0.243f, 1f);
        [TabGroup(Tabs, "Colors"), BoxGroup(Tabs + "/Colors/Health"), LabelWidth(190)] public Color healthSegmentColor = new(1f, 0.76f, 0.81f, 1f);
        [TabGroup(Tabs, "Colors"), BoxGroup(Tabs + "/Colors/Health"), LabelWidth(190)] public Color healthTextColor = new(1f, 0.925f, 0.937f, 1f);

        [TabGroup(Tabs, "Colors"), BoxGroup(Tabs + "/Colors/Text"), LabelWidth(190)] public Color timerTextColor = new(0.882f, 0.949f, 0.965f, 1f);
        [TabGroup(Tabs, "Colors"), BoxGroup(Tabs + "/Colors/Text"), LabelWidth(190)] public Color encoreTimerTextColor = new(1f, 0.18f, 0.24f, 1f);
        [TabGroup(Tabs, "Colors"), BoxGroup(Tabs + "/Colors/Text"), LabelWidth(190)] public Color killsLabelColor = new(1f, 0.922f, 0.655f, 1f);
        [TabGroup(Tabs, "Colors"), BoxGroup(Tabs + "/Colors/Text"), LabelWidth(190)] public Color killsNumberColor = new(1f, 0.965f, 0.839f, 1f);
        [TabGroup(Tabs, "Colors"), BoxGroup(Tabs + "/Colors/Text"), LabelWidth(190)] public Color ticketsLabelColor = new(1f, 0.871f, 0.357f, 1f);
        [TabGroup(Tabs, "Colors"), BoxGroup(Tabs + "/Colors/Text"), LabelWidth(190)] public Color ticketsNumberColor = new(1f, 0.961f, 0.729f, 1f);

        [TabGroup(Tabs, "Colors"), BoxGroup(Tabs + "/Colors/Act Banner"), LabelWidth(190)] public Color actBannerKickerColor = new(0.47f, 0.93f, 1f, 1f);
        [TabGroup(Tabs, "Colors"), BoxGroup(Tabs + "/Colors/Act Banner"), LabelWidth(190)] public Color actBannerTitleColor = new(1f, 0.95f, 0.72f, 1f);
        [TabGroup(Tabs, "Colors"), BoxGroup(Tabs + "/Colors/Act Banner"), LabelWidth(190)] public Color actBannerOutlineColor = new(0f, 0f, 0f, 0.9f);

        [TabGroup(Tabs, "Colors"), BoxGroup(Tabs + "/Colors/Game Over"), LabelWidth(190)] public Color gameOverBackgroundColor = new(0.027f, 0.039f, 0.059f, 0.78f);
        [TabGroup(Tabs, "Colors"), BoxGroup(Tabs + "/Colors/Game Over"), LabelWidth(190)] public Color gameOverBorderColor = new(1f, 0.314f, 0.51f, 0.65f);
        [TabGroup(Tabs, "Colors"), BoxGroup(Tabs + "/Colors/Game Over"), LabelWidth(190)] public Color gameOverTitleColor = new(1f, 0.322f, 0.502f, 1f);
        [TabGroup(Tabs, "Colors"), BoxGroup(Tabs + "/Colors/Game Over"), LabelWidth(190)] public Color gameOverSubtitleColor = new(0.816f, 0.902f, 0.933f, 1f);

        [TabGroup(Tabs, "Diagnostics"), ShowInInspector, ReadOnly, LabelText("Workflow")]
        private string WorkflowSummary => $"{scaleMode}, {referenceDpi:0.#}/{fallbackDpi:0.#} DPI, {globalScale:0.00}x manual scale";
        [TabGroup(Tabs, "Diagnostics"), ShowInInspector, ReadOnly, LabelText("Preview")]
        private string PreviewSummary
        {
            get
            {
                HudPreviewSnapshot snapshot = GetPreviewSnapshot();
                return previewEnabled
                    ? $"{previewPreset}: {snapshot.CurrentHealth}/{snapshot.MaxHealth} HP, XP {snapshot.CurrentExperience}/{snapshot.TargetExperience}, LV {snapshot.Level}, {snapshot.Kills} kills, {snapshot.RunSeconds:0}s"
                    : "Preview disabled";
            }
        }

        private bool IsCustomPreview => previewPreset == HudPreviewPreset.Custom;

        public HudPreviewSnapshot GetPreviewSnapshot()
        {
            return GetPreviewSnapshot(previewPreset);
        }

        public HudPreviewSnapshot GetPreviewSnapshot(HudPreviewPreset preset)
        {
            HudPreviewSnapshot snapshot = preset switch
            {
                HudPreviewPreset.Normal => new HudPreviewSnapshot(100, 100, 2, 10, 1, 0, 23f, false),
                HudPreviewPreset.Damaged => new HudPreviewSnapshot(43, 100, 5, 10, 2, 18, 154f, false),
                HudPreviewPreset.Leveling => new HudPreviewSnapshot(86, 100, 9, 10, 4, 72, 468f, false),
                HudPreviewPreset.GameOver => new HudPreviewSnapshot(0, 100, 6, 10, 5, 103, 632f, true),
                HudPreviewPreset.Custom => new HudPreviewSnapshot(
                    previewHealth,
                    previewMaxHealth,
                    previewExperience,
                    previewExperienceTarget,
                    previewLevel,
                    previewKills,
                    previewRunSeconds,
                    previewGameOverVisible),
                _ => HudPreviewSnapshot.Clear
            };

            return snapshot.Clamped();
        }

        public void SetPreviewPreset(HudPreviewPreset preset)
        {
            previewEnabled = true;
            previewPreset = preset;
            EnsureReadableDefaults();
        }

        public bool EnsureReadableDefaults()
        {
            bool changed = false;
            changed |= ClampMinimum(ref referenceDpi, 96f, 1f);
            changed |= ClampMinimum(ref fallbackDpi, 96f, 1f);
            changed |= ClampRange(ref globalScale, 1f, 0.25f, 3f);
            changed |= ClampMinimum(ref referenceResolution.x, 1200f, 100f);
            changed |= ClampMinimum(ref referenceResolution.y, 800f, 100f);

            changed |= ClampMinimum(ref xpStripHeight, 22f, 1f);
            changed |= ClampMinimum(ref xpTopPadding, 4f, 0f);
            changed |= ClampMinimum(ref xpSideMargin, 8f, 0f);
            changed |= ClampMinimum(ref xpTrackHeight, 16f, 1f);
            changed |= ClampMinimum(ref xpCornerRadius, 4f, 0f);
            changed |= ClampMinimum(ref levelBadgeWidth, 24f, 1f);
            changed |= ClampMinimum(ref levelBadgeHeight, 24f, 1f);
            changed |= ClampMinimum(ref levelBadgeHorizontalPadding, 0f, 0f);
            changed |= ClampMinimum(ref levelBadgeFontSize, 13f, 1f);
            changed |= xpBarTween.EnsureDefaults(true, 0.24f, EaseSettings.OutCubic);
            changed |= ClampRange(ref xpSegmentCount, 10, 1, 40);
            changed |= ClampMinimum(ref xpSegmentWidth, 1f, 0f);
            changed |= ClampRange(ref xpSegmentOpacity, 0.34f, 0f, 1f);
            changed |= ClampMinimum(ref xpLevelUpPulseSpeed, 2.8f, 0f);
            changed |= ClampRange(ref xpLevelUpPulseMaxOpacity, 0.58f, 0f, 1f);

            changed |= ClampMinimum(ref healthWidth, 210f, 1f);
            changed |= ClampMinimum(ref healthHeight, 22f, 1f);
            changed |= ClampMinimum(ref healthCornerRadius, 5f, 0f);
            changed |= ClampMinimum(ref healthFontSize, 12f, 1f);
            changed |= ClampRange(ref healthTextVerticalOffset, -1f, -20f, 20f);
            changed |= healthBarTween.EnsureDefaults(true, 0.18f, EaseSettings.OutQuad);
            changed |= ClampMinimum(ref healthHpPerSegment, 25f, 1f);
            changed |= ClampMinimum(ref healthSegmentWidth, 1f, 0f);
            changed |= ClampRange(ref healthSegmentOpacity, 0.38f, 0f, 1f);
            changed |= ClampMinimum(ref healthMinimumSegmentPixelSpacing, 10f, 0f);

            changed |= ClampMinimum(ref timerTopOffset, 28f, 0f);
            changed |= ClampMinimum(ref timerHeight, 24f, 1f);
            changed |= ClampMinimum(ref timerFontSize, 14f, 1f);
            changed |= ClampMinimum(ref actBannerWidth, 420f, 1f);
            changed |= ClampMinimum(ref actBannerHeight, 96f, 1f);
            changed |= ClampMinimum(ref actBannerTopOffset, 260f, 0f);
            changed |= ClampMinimum(ref actBannerKickerFontSize, 16f, 1f);
            changed |= ClampMinimum(ref actBannerTitleFontSize, 34f, 1f);
            changed |= ClampMinimum(ref actBannerTitleTopMargin, 3f, 0f);
            changed |= ClampMinimum(ref actBannerDelaySeconds, 2f, 0f);
            changed |= ClampMinimum(ref actBannerPopSeconds, 0.2f, 0.01f);
            changed |= ClampMinimum(ref actBannerHoldSeconds, 4f, 0.01f);
            changed |= ClampMinimum(ref actBannerFadeSeconds, 0.22f, 0.01f);
            changed |= ClampMinimum(ref actBannerStartScale, 0.08f, 0.01f);
            changed |= ClampMinimum(ref actBannerKickerOutlineWidth, 2f, 0f);
            changed |= ClampMinimum(ref actBannerTitleOutlineWidth, 3f, 0f);
            changed |= EnsureEaseDefault(ref actBannerPopEase, EaseSettings.OutBounce);
            changed |= EnsureEaseDefault(ref actBannerFadeEase, EaseSettings.InBack);

            changed |= ClampMinimum(ref killsHeight, 24f, 1f);
            changed |= ClampMinimum(ref killsLabelFontSize, 12f, 1f);
            changed |= ClampMinimum(ref killsNumberFontSize, 18f, 1f);
            changed |= ClampMinimum(ref killsSpacing, 7f, 0f);
            changed |= ClampMinimum(ref killsNumberMinWidth, 32f, 1f);
            if (string.IsNullOrWhiteSpace(killsLabelText))
            {
                killsLabelText = "KILLS";
                changed = true;
            }

            changed |= ClampMinimum(ref ticketsHeight, 24f, 1f);
            changed |= ClampMinimum(ref ticketsLabelFontSize, 12f, 1f);
            changed |= ClampMinimum(ref ticketsNumberFontSize, 18f, 1f);
            changed |= ClampMinimum(ref ticketsSpacing, 7f, 0f);
            changed |= ClampMinimum(ref ticketsNumberMinWidth, 42f, 1f);
            if (string.IsNullOrWhiteSpace(ticketsLabelText))
            {
                ticketsLabelText = "TICKETS";
                changed = true;
            }

            changed |= ClampMinimum(ref weaponSlotRowHeight, 46f, 1f);
            changed |= ClampMinimum(ref weaponSlotSize, 30f, 1f);
            changed |= ClampMinimum(ref weaponSlotGap, 5f, 0f);
            changed |= ClampMinimum(ref weaponSlotCornerRadius, 4f, 0f);
            changed |= ClampMinimum(ref weaponSlotLevelMinWidth, 13f, 1f);
            changed |= ClampMinimum(ref weaponSlotLevelHeight, 12f, 1f);
            changed |= ClampMinimum(ref weaponSlotLevelFontSize, 8f, 1f);

            changed |= ClampMinimum(ref itemStackColumnWidth, 36f, 1f);
            changed |= ClampMinimum(ref itemStackSlotSize, 32f, 1f);
            changed |= ClampMinimum(ref itemStackGap, 5f, 0f);
            changed |= ClampMinimum(ref itemStackCornerRadius, 4f, 0f);
            changed |= ClampMinimum(ref itemStackCountMinWidth, 13f, 1f);
            changed |= ClampMinimum(ref itemStackCountHeight, 12f, 1f);
            changed |= ClampMinimum(ref itemStackCountFontSize, 8f, 1f);

            changed |= killsCounterPop.EnsureDefaults(true, 0.18f, 1.22f, EaseSettings.OutBack);
            changed |= ticketsCounterPop.EnsureDefaults(true, 0.18f, 1.22f, EaseSettings.OutBack);
            changed |= popupMotion.EnsureDefaults(true, 0.2f, 0.08f, 1f, true, EaseSettings.OutBack);
            changed |= ClampMinimum(ref rewardRevealAcceptDelaySeconds, 0.5f, 0f);
            changed |= rewardRevealAcceptMotion.EnsureDefaults(true, 0.2f, 0.08f, 1f, true, EaseSettings.OutBounce);
            changed |= runTransition.EnsureDefaults();

            changed |= ClampMinimum(ref gameOverWidth, 310f, 1f);
            changed |= ClampMinimum(ref gameOverHeight, 128f, 1f);
            changed |= ClampMinimum(ref gameOverCornerRadius, 8f, 0f);
            changed |= ClampMinimum(ref gameOverTitleFontSize, 34f, 1f);
            changed |= ClampMinimum(ref gameOverSubtitleFontSize, 13f, 1f);
            changed |= ClampMinimum(ref gameOverSubtitleTopMargin, 8f, 0f);

            changed |= ClampMinimum(ref previewMaxHealth, 100, 1);
            changed |= ClampRange(ref previewHealth, 78, 0, previewMaxHealth);
            changed |= ClampMinimum(ref previewExperienceTarget, 10, 1);
            changed |= ClampRange(ref previewExperience, 6, 0, previewExperienceTarget);
            changed |= ClampMinimum(ref previewLevel, 3, 1);
            changed |= ClampMinimum(ref previewKills, 42, 0);
            changed |= ClampMinimum(ref previewRunSeconds, 154f, 0f);
            return changed;
        }

        private void OnValidate()
        {
            EnsureReadableDefaults();
        }

        internal static bool ClampMinimum(ref float value, float fallback, float minimum)
        {
            if (value >= minimum)
            {
                return false;
            }

            value = Mathf.Max(fallback, minimum);
            return true;
        }

        internal static bool ClampRange(ref float value, float fallback, float minimum, float maximum)
        {
            if (value >= minimum && value <= maximum)
            {
                return false;
            }

            value = Mathf.Clamp(fallback, minimum, maximum);
            return true;
        }

        internal static bool ClampMinimum(ref int value, int fallback, int minimum)
        {
            if (value >= minimum)
            {
                return false;
            }

            value = Mathf.Max(fallback, minimum);
            return true;
        }

        internal static bool ClampRange(ref int value, int fallback, int minimum, int maximum)
        {
            if (value >= minimum && value <= maximum)
            {
                return false;
            }

            value = Mathf.Clamp(fallback, minimum, maximum);
            return true;
        }

        internal static bool EnsureEaseDefault(ref EaseSettings settings, EaseSettings defaultValue)
        {
            if (settings.shape > 0f)
            {
                return false;
            }

            settings = defaultValue;
            return true;
        }
    }

    [Serializable]
    public struct HudPopupMotionSettings
    {
        [VerticalGroup("Motion"), LabelText("Enabled")]
        public bool enabled;
        [VerticalGroup("Motion"), LabelText("Seconds"), SuffixLabel("sec", true), Min(0.01f)]
        public float seconds;
        [VerticalGroup("Motion"), LabelText("Start Scale"), Min(0.01f)]
        public float startScale;
        [VerticalGroup("Motion"), LabelText("End Scale"), Min(0.01f)]
        public float endScale;
        [VerticalGroup("Motion"), LabelText("Fade In")]
        public bool fadeIn;
        [VerticalGroup("Motion"), LabelText("Ease"), InlineProperty]
        public EaseSettings ease;

        public HudPopupMotionSettings(
            bool enabled,
            float seconds,
            float startScale,
            float endScale,
            bool fadeIn,
            EaseSettings ease)
        {
            this.enabled = enabled;
            this.seconds = seconds;
            this.startScale = startScale;
            this.endScale = endScale;
            this.fadeIn = fadeIn;
            this.ease = ease;
        }

        public static HudPopupMotionSettings Default => new(true, 0.2f, 0.08f, 1f, true, EaseSettings.OutBack);

        public bool EnsureDefaults(
            bool defaultEnabled,
            float defaultSeconds,
            float defaultStartScale,
            float defaultEndScale,
            bool defaultFadeIn,
            EaseSettings defaultEase)
        {
            bool changed = false;
            bool wasUninitialized = seconds <= 0f && startScale <= 0f && endScale <= 0f && ease.shape <= 0f;
            if (float.IsNaN(seconds) || seconds < 0.01f)
            {
                seconds = defaultSeconds;
                changed = true;
            }

            if (float.IsNaN(startScale) || startScale < 0.01f)
            {
                startScale = defaultStartScale;
                changed = true;
            }

            if (float.IsNaN(endScale) || endScale < 0.01f)
            {
                endScale = defaultEndScale;
                changed = true;
            }

            changed |= HudVisualConfig.EnsureEaseDefault(ref ease, defaultEase);
            if (wasUninitialized)
            {
                if (enabled != defaultEnabled)
                {
                    enabled = defaultEnabled;
                    changed = true;
                }

                if (fadeIn != defaultFadeIn)
                {
                    fadeIn = defaultFadeIn;
                    changed = true;
                }
            }

            return changed;
        }
    }

    [Serializable]
    public struct HudTransitionSettings
    {
        [VerticalGroup("Transition"), LabelText("Enabled")]
        public bool enabled;
        [VerticalGroup("Transition"), LabelText("Fade Out"), SuffixLabel("sec", true), Min(0.01f)]
        public float fadeOutSeconds;
        [VerticalGroup("Transition"), LabelText("Fade In"), SuffixLabel("sec", true), Min(0.01f)]
        public float fadeInSeconds;
        [VerticalGroup("Transition"), LabelText("Minimum Visible"), SuffixLabel("sec", true), Min(0f)]
        public float minimumVisibleSeconds;
        [VerticalGroup("Transition"), LabelText("Background")]
        public Color backgroundColor;
        [VerticalGroup("Transition"), LabelText("Title Color")]
        public Color titleColor;
        [VerticalGroup("Transition"), LabelText("Subtitle Color")]
        public Color subtitleColor;
        [VerticalGroup("Transition"), LabelText("Outline Color")]
        public Color outlineColor;
        [VerticalGroup("Transition"), LabelText("Title Size"), SuffixLabel("px", true), Min(1f)]
        public float titleFontSize;
        [VerticalGroup("Transition"), LabelText("Subtitle Size"), SuffixLabel("px", true), Min(1f)]
        public float subtitleFontSize;
        [VerticalGroup("Transition"), LabelText("Title Outline"), SuffixLabel("px", true), Min(0f)]
        public float titleOutlineWidth;
        [VerticalGroup("Transition"), LabelText("Subtitle Outline"), SuffixLabel("px", true), Min(0f)]
        public float subtitleOutlineWidth;
        [VerticalGroup("Transition"), LabelText("Stage Text Enabled")]
        public bool loadingStageTextEnabled;
        [VerticalGroup("Transition"), LabelText("Stage Text Color")]
        public Color loadingStageColor;
        [VerticalGroup("Transition"), LabelText("Stage Text Size"), SuffixLabel("px", true), Min(1f)]
        public float loadingStageFontSize;
        [VerticalGroup("Transition"), LabelText("Stage Text Outline"), SuffixLabel("px", true), Min(0f)]
        public float loadingStageOutlineWidth;
        [VerticalGroup("Transition"), LabelText("Bulb Count"), Min(3)]
        public int bulbCount;
        [VerticalGroup("Transition"), LabelText("Bulb Size"), SuffixLabel("px", true), Min(1f)]
        public float bulbSize;
        [VerticalGroup("Transition"), LabelText("Ring Radius"), SuffixLabel("px", true), Min(1f)]
        public float bulbRingRadius;
        [VerticalGroup("Transition"), LabelText("Bulb Bright")]
        public Color bulbBrightColor;
        [VerticalGroup("Transition"), LabelText("Bulb Dim")]
        public Color bulbDimColor;
        [VerticalGroup("Transition"), LabelText("Bulb Chase"), SuffixLabel("cycles/sec", true), Min(0f)]
        public float bulbChaseSpeed;
        [VerticalGroup("Transition"), LabelText("Fade Ease"), InlineProperty]
        public EaseSettings fadeEase;

        public HudTransitionSettings(
            bool enabled,
            float fadeOutSeconds,
            float fadeInSeconds,
            float minimumVisibleSeconds,
            Color backgroundColor,
            Color titleColor,
            Color subtitleColor,
            Color outlineColor,
            float titleFontSize,
            float subtitleFontSize,
            float titleOutlineWidth,
            float subtitleOutlineWidth,
            bool loadingStageTextEnabled,
            Color loadingStageColor,
            float loadingStageFontSize,
            float loadingStageOutlineWidth,
            int bulbCount,
            float bulbSize,
            float bulbRingRadius,
            Color bulbBrightColor,
            Color bulbDimColor,
            float bulbChaseSpeed,
            EaseSettings fadeEase)
        {
            this.enabled = enabled;
            this.fadeOutSeconds = fadeOutSeconds;
            this.fadeInSeconds = fadeInSeconds;
            this.minimumVisibleSeconds = minimumVisibleSeconds;
            this.backgroundColor = backgroundColor;
            this.titleColor = titleColor;
            this.subtitleColor = subtitleColor;
            this.outlineColor = outlineColor;
            this.titleFontSize = titleFontSize;
            this.subtitleFontSize = subtitleFontSize;
            this.titleOutlineWidth = titleOutlineWidth;
            this.subtitleOutlineWidth = subtitleOutlineWidth;
            this.loadingStageTextEnabled = loadingStageTextEnabled;
            this.loadingStageColor = loadingStageColor;
            this.loadingStageFontSize = loadingStageFontSize;
            this.loadingStageOutlineWidth = loadingStageOutlineWidth;
            this.bulbCount = bulbCount;
            this.bulbSize = bulbSize;
            this.bulbRingRadius = bulbRingRadius;
            this.bulbBrightColor = bulbBrightColor;
            this.bulbDimColor = bulbDimColor;
            this.bulbChaseSpeed = bulbChaseSpeed;
            this.fadeEase = fadeEase;
        }

        public static HudTransitionSettings Default => new(
            true,
            0.4f,
            0.45f,
            0.65f,
            new Color(0.004f, 0.006f, 0.012f, 1f),
            new Color(0.47f, 0.93f, 1f, 1f),
            new Color(1f, 0.94f, 0.7f, 1f),
            new Color(0f, 0f, 0f, 0.95f),
            42f,
            18f,
            4f,
            2f,
            true,
            new Color(1f, 0.78f, 0.28f, 1f),
            14f,
            2f,
            12,
            9f,
            34f,
            new Color(1f, 0.9f, 0.46f, 1f),
            new Color(1f, 0.55f, 0.12f, 0.3f),
            1.8f,
            EaseSettings.InOutSine);

        public bool EnsureDefaults()
        {
            bool changed = false;
            bool wasUninitialized = fadeOutSeconds <= 0f
                && fadeInSeconds <= 0f
                && minimumVisibleSeconds <= 0f
                && titleFontSize <= 0f
                && loadingStageFontSize <= 0f
                && bulbCount <= 0
                && fadeEase.shape <= 0f;
            bool stageTextWasUninitialized = loadingStageFontSize <= 0f && loadingStageColor.a <= 0f;
            HudTransitionSettings defaults = Default;
            if (float.IsNaN(fadeOutSeconds) || fadeOutSeconds < 0.01f)
            {
                fadeOutSeconds = defaults.fadeOutSeconds;
                changed = true;
            }

            if (float.IsNaN(fadeInSeconds) || fadeInSeconds < 0.01f)
            {
                fadeInSeconds = defaults.fadeInSeconds;
                changed = true;
            }

            if (float.IsNaN(minimumVisibleSeconds) || minimumVisibleSeconds < 0f)
            {
                minimumVisibleSeconds = defaults.minimumVisibleSeconds;
                changed = true;
            }

            if (backgroundColor.a <= 0f)
            {
                backgroundColor = defaults.backgroundColor;
                changed = true;
            }

            if (titleColor.a <= 0f)
            {
                titleColor = defaults.titleColor;
                changed = true;
            }

            if (subtitleColor.a <= 0f)
            {
                subtitleColor = defaults.subtitleColor;
                changed = true;
            }

            if (outlineColor.a <= 0f)
            {
                outlineColor = defaults.outlineColor;
                changed = true;
            }

            changed |= HudVisualConfig.ClampMinimum(ref titleFontSize, defaults.titleFontSize, 1f);
            changed |= HudVisualConfig.ClampMinimum(ref subtitleFontSize, defaults.subtitleFontSize, 1f);
            changed |= HudVisualConfig.ClampMinimum(ref titleOutlineWidth, defaults.titleOutlineWidth, 0f);
            changed |= HudVisualConfig.ClampMinimum(ref subtitleOutlineWidth, defaults.subtitleOutlineWidth, 0f);
            if (loadingStageColor.a <= 0f)
            {
                loadingStageColor = defaults.loadingStageColor;
                changed = true;
            }

            changed |= HudVisualConfig.ClampMinimum(ref loadingStageFontSize, defaults.loadingStageFontSize, 1f);
            changed |= HudVisualConfig.ClampMinimum(ref loadingStageOutlineWidth, defaults.loadingStageOutlineWidth, 0f);
            changed |= HudVisualConfig.ClampRange(ref bulbCount, defaults.bulbCount, 3, 48);
            changed |= HudVisualConfig.ClampMinimum(ref bulbSize, defaults.bulbSize, 1f);
            changed |= HudVisualConfig.ClampMinimum(ref bulbRingRadius, defaults.bulbRingRadius, 1f);
            if (bulbBrightColor.a <= 0f)
            {
                bulbBrightColor = defaults.bulbBrightColor;
                changed = true;
            }

            if (bulbDimColor.a <= 0f)
            {
                bulbDimColor = defaults.bulbDimColor;
                changed = true;
            }

            changed |= HudVisualConfig.ClampMinimum(ref bulbChaseSpeed, defaults.bulbChaseSpeed, 0f);
            changed |= HudVisualConfig.EnsureEaseDefault(ref fadeEase, defaults.fadeEase);
            if (wasUninitialized && enabled != defaults.enabled)
            {
                enabled = defaults.enabled;
                changed = true;
            }

            if (wasUninitialized && loadingStageTextEnabled != defaults.loadingStageTextEnabled)
            {
                loadingStageTextEnabled = defaults.loadingStageTextEnabled;
                changed = true;
            }
            else if (stageTextWasUninitialized && loadingStageTextEnabled != defaults.loadingStageTextEnabled)
            {
                loadingStageTextEnabled = defaults.loadingStageTextEnabled;
                changed = true;
            }

            return changed;
        }
    }

    [Serializable]
    public struct HudCounterPopSettings
    {
        [VerticalGroup("Pop"), LabelText("Enabled")]
        public bool enabled;
        [VerticalGroup("Pop"), LabelText("Seconds"), SuffixLabel("sec", true), Min(0.01f)]
        public float seconds;
        [VerticalGroup("Pop"), LabelText("Scale"), Min(1f)]
        public float scale;
        [VerticalGroup("Pop"), LabelText("Ease"), InlineProperty]
        public EaseSettings ease;

        public HudCounterPopSettings(bool enabled, float seconds, float scale, EaseSettings ease)
        {
            this.enabled = enabled;
            this.seconds = seconds;
            this.scale = scale;
            this.ease = ease;
        }

        public static HudCounterPopSettings Default => new(true, 0.18f, 1.22f, EaseSettings.OutBack);

        public bool EnsureDefaults(bool defaultEnabled, float defaultSeconds, float defaultScale, EaseSettings defaultEase)
        {
            bool changed = false;
            bool wasUninitialized = seconds <= 0f && scale <= 0f && ease.shape <= 0f;
            if (float.IsNaN(seconds) || seconds < 0.01f)
            {
                seconds = defaultSeconds;
                changed = true;
            }

            if (float.IsNaN(scale) || scale < 1f)
            {
                scale = defaultScale;
                changed = true;
            }

            changed |= HudVisualConfig.EnsureEaseDefault(ref ease, defaultEase);
            if (wasUninitialized && enabled != defaultEnabled)
            {
                enabled = defaultEnabled;
                changed = true;
            }

            return changed;
        }
    }

    [Serializable]
    public struct HudBarTweenSettings
    {
        [VerticalGroup("Tween"), LabelText("Enabled")]
        public bool enabled;
        [VerticalGroup("Tween"), LabelText("Seconds"), SuffixLabel("sec", true), Min(0.01f)]
        public float seconds;
        [VerticalGroup("Tween"), LabelText("Ease"), InlineProperty]
        public EaseSettings ease;

        public HudBarTweenSettings(bool enabled, float seconds, EaseSettings ease)
        {
            this.enabled = enabled;
            this.seconds = seconds;
            this.ease = ease;
        }

        public static HudBarTweenSettings DefaultXp => new(true, 0.24f, EaseSettings.OutCubic);
        public static HudBarTweenSettings DefaultHealth => new(true, 0.18f, EaseSettings.OutQuad);

        public bool EnsureDefaults(bool defaultEnabled, float defaultSeconds, EaseSettings defaultEase)
        {
            bool changed = false;
            bool wasUninitialized = seconds <= 0f && ease.shape <= 0f;
            if (float.IsNaN(seconds) || seconds < 0.01f)
            {
                seconds = defaultSeconds;
                changed = true;
            }

            changed |= HudVisualConfig.EnsureEaseDefault(ref ease, defaultEase);
            if (wasUninitialized && enabled != defaultEnabled)
            {
                enabled = defaultEnabled;
                changed = true;
            }

            return changed;
        }
    }
}
