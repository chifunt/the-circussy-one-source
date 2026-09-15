using System.Collections.Generic;
using Sirenix.OdinInspector;
using TheCircussyOne.Runtime;
using UnityEngine;

namespace TheCircussyOne.Config
{
    [CreateAssetMenu(menuName = "The Circussy One/Game Audio Config", fileName = "GameAudioConfig")]
    [InfoBox("LIVE RUNTIME: Wwise event names, pitch ranges, loop RTPCs, and audio cooldowns are read by runtime audio systems. SoundBank generation still happens through Wwise tooling.")]
    public sealed class GameAudioConfig : SerializedScriptableObject
    {
        private const string Tabs = "Game Audio";

        [TabGroup(Tabs, "Runtime"), BoxGroup(Tabs + "/Runtime/Wwise"), LabelWidth(190)]
        public bool audioEnabled = true;
        [TabGroup(Tabs, "Runtime"), BoxGroup(Tabs + "/Runtime/Wwise"), LabelWidth(190)]
        public bool loadMainBankOnStart;
        [TabGroup(Tabs, "Runtime"), BoxGroup(Tabs + "/Runtime/Wwise"), LabelWidth(190)]
        public string mainSoundBankName = "Main";
        [TabGroup(Tabs, "Runtime"), BoxGroup(Tabs + "/Runtime/Wwise"), LabelWidth(190)]
        public bool preloadEventSoundBanksOnStart = true;
        [TabGroup(Tabs, "Runtime"), BoxGroup(Tabs + "/Runtime/Wwise"), LabelWidth(190), SuffixLabel("sec")]
        [Min(0.25f)] public float oneShotEmitterLifetimeSeconds = 4f;

        [TabGroup(Tabs, "Runtime"), BoxGroup(Tabs + "/Runtime/RTPC"), LabelWidth(190)]
        public string pitchRtpcName = "SfxPitchOffsetCents";
        [TabGroup(Tabs, "Runtime"), BoxGroup(Tabs + "/Runtime/RTPC"), LabelWidth(190)]
        public string interactionProgressRtpcName = "InteractionProgress01";
        [TabGroup(Tabs, "Runtime"), BoxGroup(Tabs + "/Runtime/RTPC"), LabelWidth(190)]
        public string playerMovementSpeedRtpcName = "PlayerMovementSpeed01";
        [TabGroup(Tabs, "Runtime"), BoxGroup(Tabs + "/Runtime/RTPC"), LabelWidth(190)]
        public string musicPresentationRtpcName = "MusicPresentation01";

        [TabGroup(Tabs, "Events"), BoxGroup(Tabs + "/Events/2D"), LabelWidth(220)]
        public string uiClickEvent = "Play_UI_Click";
        [TabGroup(Tabs, "Events"), BoxGroup(Tabs + "/Events/2D"), LabelWidth(220)]
        public string uiHoverEvent = "Play_UI_Hover";
        [TabGroup(Tabs, "Events"), BoxGroup(Tabs + "/Events/2D"), LabelWidth(220)]
        public string uiSheenLoopEvent = "Play_UI_Sheen_Loop";
        [TabGroup(Tabs, "Events"), BoxGroup(Tabs + "/Events/2D"), LabelWidth(220)]
        public string stopUiSheenLoopEvent = "Stop_UI_Sheen_Loop";
        [TabGroup(Tabs, "Events"), BoxGroup(Tabs + "/Events/2D"), LabelWidth(220)]
        public string interactionCanceledEvent = "Play_Interaction_Canceled";
        [TabGroup(Tabs, "Events"), BoxGroup(Tabs + "/Events/2D"), LabelWidth(220)]
        public string interactionHoldLoopEvent = "Play_Interaction_Loop";
        [TabGroup(Tabs, "Events"), BoxGroup(Tabs + "/Events/2D"), LabelWidth(220)]
        public string stopInteractionHoldLoopEvent = "Stop_Interaction_Loop";
        [TabGroup(Tabs, "Events"), BoxGroup(Tabs + "/Events/2D"), LabelWidth(220)]
        public string chestOpenEvent = "Play_Chest_Open";
        [TabGroup(Tabs, "Events"), BoxGroup(Tabs + "/Events/2D"), LabelWidth(220)]
        public string stageDoorOpenEvent = "Play_Stage_Door_Open";
        [TabGroup(Tabs, "Events"), BoxGroup(Tabs + "/Events/2D"), LabelWidth(220)]
        public string gameOverEvent = "Play_Game_Over";
        [TabGroup(Tabs, "Events"), BoxGroup(Tabs + "/Events/2D"), LabelWidth(220)]
        public string levelUpEvent = "Play_Level_Up";
        [TabGroup(Tabs, "Events"), BoxGroup(Tabs + "/Events/2D"), LabelWidth(220)]
        public string playerHurtEvent = "Play_Player_Hurt";
        [TabGroup(Tabs, "Events"), BoxGroup(Tabs + "/Events/2D"), LabelWidth(220)]
        public string rarityUpEvent = "Play_Rarity_Up";
        [TabGroup(Tabs, "Events"), BoxGroup(Tabs + "/Events/2D"), LabelWidth(220)]
        public string snackEatEvent = "Play_Snack_Eat";
        [TabGroup(Tabs, "Events"), BoxGroup(Tabs + "/Events/2D"), LabelWidth(220)]
        public string snackOpenEvent = "Play_Snack_Open";
        [TabGroup(Tabs, "Events"), BoxGroup(Tabs + "/Events/2D"), LabelWidth(220)]
        public string ticketCollectEvent = "Play_Ticket_Collect";
        [TabGroup(Tabs, "Events"), BoxGroup(Tabs + "/Events/2D"), LabelWidth(220)]
        public string xpCollectEvent = "Play_XP_Collect";

        [TabGroup(Tabs, "Events"), BoxGroup(Tabs + "/Events/3D"), LabelWidth(220)]
        public string enemyDeathEvent = "Play_Enemy_Death";
        [TabGroup(Tabs, "Events"), BoxGroup(Tabs + "/Events/3D"), LabelWidth(220)]
        public string enemyHitEvent = "Play_Enemy_Hit";
        [TabGroup(Tabs, "Events"), BoxGroup(Tabs + "/Events/3D"), LabelWidth(220)]
        public string playerJumpEvent = "Play_Player_Jump";
        [TabGroup(Tabs, "Events"), BoxGroup(Tabs + "/Events/3D"), LabelWidth(220)]
        public string playerLandEvent = "Play_Player_Land";
        [TabGroup(Tabs, "Events"), BoxGroup(Tabs + "/Events/3D"), LabelWidth(220)]
        public string playerMovementLoopEvent = "Play_Player_Movement_Loop";
        [TabGroup(Tabs, "Events"), BoxGroup(Tabs + "/Events/3D"), LabelWidth(220)]
        public string stopPlayerMovementLoopEvent = "Stop_Player_Movement_Loop";

        [TabGroup(Tabs, "Events"), BoxGroup(Tabs + "/Events/Weapons"), LabelWidth(240)]
        public string jugglingBallThrowEvent = "Play_Weapon_JugglingBall_Throw";
        [TabGroup(Tabs, "Events"), BoxGroup(Tabs + "/Events/Weapons"), LabelWidth(240)]
        public string jugglingBallHitEvent = "Play_Weapon_JugglingBall_Hit";
        [TabGroup(Tabs, "Events"), BoxGroup(Tabs + "/Events/Weapons"), LabelWidth(240)]
        public string cannonLaunchEvent = "Play_Weapon_Cannon_Launch";
        [TabGroup(Tabs, "Events"), BoxGroup(Tabs + "/Events/Weapons"), LabelWidth(240)]
        public string cannonHitEvent = "Play_Weapon_Cannon_Hit";
        [TabGroup(Tabs, "Events"), BoxGroup(Tabs + "/Events/Weapons"), LabelWidth(240)]
        public string knifeFanThrowEvent = "Play_Weapon_KnifeFan_Throw";
        [TabGroup(Tabs, "Events"), BoxGroup(Tabs + "/Events/Weapons"), LabelWidth(240)]
        public string knifeFanHitEvent = "Play_Weapon_KnifeFan_Hit";
        [TabGroup(Tabs, "Events"), BoxGroup(Tabs + "/Events/Weapons"), LabelWidth(240)]
        public string spotlightBoltLaunchEvent = "Play_Weapon_SpotlightBolt_Launch";
        [TabGroup(Tabs, "Events"), BoxGroup(Tabs + "/Events/Weapons"), LabelWidth(240)]
        public string spotlightBoltHitEvent = "Play_Weapon_SpotlightBolt_Hit";
        [TabGroup(Tabs, "Events"), BoxGroup(Tabs + "/Events/Weapons"), LabelWidth(240)]
        public string fireHoopHitEvent = "Play_Weapon_FireHoop_Hit";

        [TabGroup(Tabs, "Events"), BoxGroup(Tabs + "/Events/Announcements"), LabelWidth(220)]
        public string announcementActEvent = "Play_Announcement_Act";
        [TabGroup(Tabs, "Events"), BoxGroup(Tabs + "/Events/Announcements"), LabelWidth(220)]
        public string announcementFadeoutEvent = "Play_Announcement_Fadeout";
        [TabGroup(Tabs, "Events"), BoxGroup(Tabs + "/Events/Announcements"), LabelWidth(220)]
        public string announcementHeadlinerDefeatEvent = "Play_Announcement_Headliner_Defeat";
        [TabGroup(Tabs, "Events"), BoxGroup(Tabs + "/Events/Announcements"), LabelWidth(220)]
        public string announcementShowtimeHeadlinerEvent = "Play_Announcement_Showtime_Headliner";

        [TabGroup(Tabs, "Events"), BoxGroup(Tabs + "/Events/Music"), LabelWidth(220)]
        public string menuMusicEvent = "Play_Music_Menu";
        [TabGroup(Tabs, "Events"), BoxGroup(Tabs + "/Events/Music"), LabelWidth(220)]
        public string stopMenuMusicEvent = "Stop_Music_Menu";
        [TabGroup(Tabs, "Events"), BoxGroup(Tabs + "/Events/Music"), LabelWidth(220)]
        public string gameplayMusicEvent = "Play_Music_Gameplay";
        [TabGroup(Tabs, "Events"), BoxGroup(Tabs + "/Events/Music"), LabelWidth(220)]
        public string stopGameplayMusicEvent = "Stop_Music_Gameplay";

        [TabGroup(Tabs, "Music"), BoxGroup(Tabs + "/Music/Presentation"), LabelWidth(220)]
        public bool gameplayMusicEnabled = true;
        [TabGroup(Tabs, "Music"), BoxGroup(Tabs + "/Music/Presentation"), LabelWidth(220)]
        public bool menuMusicEnabled = true;
        [TabGroup(Tabs, "Music"), BoxGroup(Tabs + "/Music/Presentation"), LabelWidth(220)]
        public float musicNormalPresentationValue;
        [TabGroup(Tabs, "Music"), BoxGroup(Tabs + "/Music/Presentation"), LabelWidth(220)]
        public float musicDistantPresentationValue = 1f;
        [TabGroup(Tabs, "Music"), BoxGroup(Tabs + "/Music/Presentation"), LabelWidth(220), Range(0f, 1f)]
        public float musicNormalVolume = 0.55f;
        [TabGroup(Tabs, "Music"), BoxGroup(Tabs + "/Music/Presentation"), LabelWidth(220), Range(0f, 1f)]
        public float musicDistantVolume = 0.4f;

        [TabGroup(Tabs, "Pitch"), BoxGroup(Tabs + "/Pitch/Interaction Loop"), LabelWidth(220)]
        [SuffixLabel("cents")] public float interactionHoldLoopStartPitchCents = -400f;
        [TabGroup(Tabs, "Pitch"), BoxGroup(Tabs + "/Pitch/Interaction Loop"), LabelWidth(220)]
        [SuffixLabel("cents")] public float interactionHoldLoopEndPitchCents = 600f;

        [TabGroup(Tabs, "Pitch"), BoxGroup(Tabs + "/Pitch/XP Ladder"), LabelWidth(220)]
        [SuffixLabel("cents")] public float xpCollectPitchLiftStartCents;
        [TabGroup(Tabs, "Pitch"), BoxGroup(Tabs + "/Pitch/XP Ladder"), LabelWidth(220)]
        [Min(0f), SuffixLabel("cents")] public float xpCollectPitchLiftIncrementCents = 30f;
        [TabGroup(Tabs, "Pitch"), BoxGroup(Tabs + "/Pitch/XP Ladder"), LabelWidth(220)]
        [SuffixLabel("cents")] public float xpCollectPitchLiftMaxCents = 600f;
        [HideInInspector]
        [Min(0f)] public float xpCollectPitchResetSeconds = 1.5f;

        [TabGroup(Tabs, "Cooldowns"), BoxGroup(Tabs + "/Cooldowns/Combat"), LabelWidth(220), SuffixLabel("sec")]
        [Min(0f)] public float enemyHitCooldownSeconds = 0.025f;
        [TabGroup(Tabs, "Cooldowns"), BoxGroup(Tabs + "/Cooldowns/Player"), LabelWidth(220), SuffixLabel("sec")]
        [Min(0f)] public float playerHurtCooldownSeconds = 0.08f;
        [TabGroup(Tabs, "Cooldowns"), BoxGroup(Tabs + "/Cooldowns/Collections"), LabelWidth(220), SuffixLabel("sec")]
        [Min(0f)] public float ticketCollectCooldownSeconds = 0.03f;
        [TabGroup(Tabs, "Cooldowns"), BoxGroup(Tabs + "/Cooldowns/Combat"), LabelWidth(220), SuffixLabel("sec")]
        [Min(0f)] public float weaponHitCooldownSeconds = 0.025f;
        [TabGroup(Tabs, "Cooldowns"), BoxGroup(Tabs + "/Cooldowns/Combat"), LabelWidth(220), SuffixLabel("sec")]
        [Min(0f)] public float fireHoopHitCooldownSeconds = 0.06f;

        public static GameAudioConfig CreateRuntimeDefault()
        {
            var config = CreateInstance<GameAudioConfig>();
            config.hideFlags = HideFlags.DontSave;
            config.EnsureWorkflowDefaults();
            return config;
        }

        public bool EnsureWorkflowDefaults()
        {
            bool changed = false;
            changed |= EnsureText(ref mainSoundBankName, "Main");
            changed |= EnsureMinimum(ref oneShotEmitterLifetimeSeconds, 4f, 0.25f);
            changed |= EnsureText(ref pitchRtpcName, "SfxPitchOffsetCents");
            changed |= EnsureText(ref interactionProgressRtpcName, "InteractionProgress01");
            changed |= EnsureText(ref playerMovementSpeedRtpcName, "PlayerMovementSpeed01");
            changed |= EnsureText(ref musicPresentationRtpcName, "MusicPresentation01");
            changed |= EnsureText(ref jugglingBallThrowEvent, "Play_Weapon_JugglingBall_Throw");
            changed |= EnsureText(ref jugglingBallHitEvent, "Play_Weapon_JugglingBall_Hit");
            changed |= EnsureText(ref cannonLaunchEvent, "Play_Weapon_Cannon_Launch");
            changed |= EnsureText(ref cannonHitEvent, "Play_Weapon_Cannon_Hit");
            changed |= EnsureText(ref knifeFanThrowEvent, "Play_Weapon_KnifeFan_Throw");
            changed |= EnsureText(ref knifeFanHitEvent, "Play_Weapon_KnifeFan_Hit");
            changed |= EnsureText(ref spotlightBoltLaunchEvent, "Play_Weapon_SpotlightBolt_Launch");
            changed |= EnsureText(ref spotlightBoltHitEvent, "Play_Weapon_SpotlightBolt_Hit");
            changed |= EnsureText(ref fireHoopHitEvent, "Play_Weapon_FireHoop_Hit");
            changed |= EnsureText(ref menuMusicEvent, "Play_Music_Menu");
            changed |= EnsureText(ref stopMenuMusicEvent, "Stop_Music_Menu");
            changed |= EnsureText(ref gameplayMusicEvent, "Play_Music_Gameplay");
            changed |= EnsureText(ref stopGameplayMusicEvent, "Stop_Music_Gameplay");
            changed |= EnsureFinite(ref musicNormalPresentationValue, 0f);
            changed |= EnsureFinite(ref musicDistantPresentationValue, 1f);
            changed |= EnsureClamped01(ref musicNormalVolume, 0.55f);
            changed |= EnsureClamped01(ref musicDistantVolume, 0.4f);
            changed |= EnsureFinite(ref interactionHoldLoopStartPitchCents, -400f);
            changed |= EnsureFinite(ref interactionHoldLoopEndPitchCents, 600f);
            changed |= EnsureFinite(ref xpCollectPitchLiftStartCents, 0f);
            changed |= EnsureMinimum(ref xpCollectPitchLiftIncrementCents, 30f, 0f);
            changed |= EnsureFinite(ref xpCollectPitchLiftMaxCents, 600f);
            if (xpCollectPitchLiftMaxCents < xpCollectPitchLiftStartCents)
            {
                xpCollectPitchLiftMaxCents = xpCollectPitchLiftStartCents;
                changed = true;
            }

            changed |= EnsureMinimum(ref xpCollectPitchResetSeconds, 1.5f, 0f);
            changed |= EnsureMinimum(ref enemyHitCooldownSeconds, 0.025f, 0f);
            changed |= EnsureMinimum(ref playerHurtCooldownSeconds, 0.08f, 0f);
            changed |= EnsureMinimum(ref ticketCollectCooldownSeconds, 0.03f, 0f);
            changed |= EnsureMinimum(ref weaponHitCooldownSeconds, 0.025f, 0f);
            changed |= EnsureMinimum(ref fireHoopHitCooldownSeconds, 0.06f, 0f);
            return changed;
        }

        public string EventName(GameAudioCue cue)
        {
            return cue switch
            {
                GameAudioCue.UiClick => uiClickEvent,
                GameAudioCue.UiHover => uiHoverEvent,
                GameAudioCue.UiSheenLoop => uiSheenLoopEvent,
                GameAudioCue.InteractionCanceled => interactionCanceledEvent,
                GameAudioCue.InteractionHoldLoop => interactionHoldLoopEvent,
                GameAudioCue.ChestOpen => chestOpenEvent,
                GameAudioCue.StageDoorOpen => stageDoorOpenEvent,
                GameAudioCue.GameOver => gameOverEvent,
                GameAudioCue.LevelUp => levelUpEvent,
                GameAudioCue.PlayerHurt => playerHurtEvent,
                GameAudioCue.RarityUp => rarityUpEvent,
                GameAudioCue.SnackEat => snackEatEvent,
                GameAudioCue.SnackOpen => snackOpenEvent,
                GameAudioCue.TicketCollect => ticketCollectEvent,
                GameAudioCue.XpCollect => xpCollectEvent,
                GameAudioCue.EnemyDeath => enemyDeathEvent,
                GameAudioCue.EnemyHit => enemyHitEvent,
                GameAudioCue.PlayerJump => playerJumpEvent,
                GameAudioCue.PlayerLand => playerLandEvent,
                GameAudioCue.PlayerMovementLoop => playerMovementLoopEvent,
                GameAudioCue.AnnouncementAct => announcementActEvent,
                GameAudioCue.AnnouncementFadeout => announcementFadeoutEvent,
                GameAudioCue.AnnouncementHeadlinerDefeat => announcementHeadlinerDefeatEvent,
                GameAudioCue.AnnouncementShowtimeHeadliner => announcementShowtimeHeadlinerEvent,
                GameAudioCue.JugglingBallThrow => jugglingBallThrowEvent,
                GameAudioCue.JugglingBallHit => jugglingBallHitEvent,
                GameAudioCue.CannonLaunch => cannonLaunchEvent,
                GameAudioCue.CannonHit => cannonHitEvent,
                GameAudioCue.KnifeFanThrow => knifeFanThrowEvent,
                GameAudioCue.KnifeFanHit => knifeFanHitEvent,
                GameAudioCue.SpotlightBoltLaunch => spotlightBoltLaunchEvent,
                GameAudioCue.SpotlightBoltHit => spotlightBoltHitEvent,
                GameAudioCue.FireHoopHit => fireHoopHitEvent,
                _ => string.Empty
            };
        }

        public string StopEventName(GameAudioCue cue)
        {
            return cue switch
            {
                GameAudioCue.UiSheenLoop => stopUiSheenLoopEvent,
                GameAudioCue.InteractionHoldLoop => stopInteractionHoldLoopEvent,
                GameAudioCue.PlayerMovementLoop => stopPlayerMovementLoopEvent,
                _ => string.Empty
            };
        }

        public float InteractionHoldLoopPitchOffsetCents(float progress01)
        {
            return Mathf.Lerp(interactionHoldLoopStartPitchCents, interactionHoldLoopEndPitchCents, Mathf.Clamp01(progress01));
        }

        public float XpCollectPitchLiftCents(int pitchStep)
        {
            float raw = xpCollectPitchLiftStartCents + Mathf.Max(0, pitchStep) * xpCollectPitchLiftIncrementCents;
            return Mathf.Clamp(raw, xpCollectPitchLiftStartCents, xpCollectPitchLiftMaxCents);
        }

        public float XpCollectPitchResetDurationSeconds(XpGainCounterVisualConfig counterConfig)
        {
            if (counterConfig != null)
            {
                return Mathf.Max(0f, counterConfig.holdSeconds) + Mathf.Max(0.01f, counterConfig.fadeSeconds);
            }

            return Mathf.Max(0f, xpCollectPitchResetSeconds);
        }

        public IReadOnlyList<string> EventSoundBankNames()
        {
            var names = new List<string>();
            AddEventBankName(names, uiClickEvent);
            AddEventBankName(names, uiHoverEvent);
            AddEventBankName(names, uiSheenLoopEvent);
            AddEventBankName(names, stopUiSheenLoopEvent);
            AddEventBankName(names, interactionCanceledEvent);
            AddEventBankName(names, interactionHoldLoopEvent);
            AddEventBankName(names, stopInteractionHoldLoopEvent);
            AddEventBankName(names, chestOpenEvent);
            AddEventBankName(names, stageDoorOpenEvent);
            AddEventBankName(names, gameOverEvent);
            AddEventBankName(names, levelUpEvent);
            AddEventBankName(names, playerHurtEvent);
            AddEventBankName(names, rarityUpEvent);
            AddEventBankName(names, snackEatEvent);
            AddEventBankName(names, snackOpenEvent);
            AddEventBankName(names, ticketCollectEvent);
            AddEventBankName(names, xpCollectEvent);
            AddEventBankName(names, enemyDeathEvent);
            AddEventBankName(names, enemyHitEvent);
            AddEventBankName(names, playerJumpEvent);
            AddEventBankName(names, playerLandEvent);
            AddEventBankName(names, playerMovementLoopEvent);
            AddEventBankName(names, stopPlayerMovementLoopEvent);
            AddEventBankName(names, jugglingBallThrowEvent);
            AddEventBankName(names, jugglingBallHitEvent);
            AddEventBankName(names, cannonLaunchEvent);
            AddEventBankName(names, cannonHitEvent);
            AddEventBankName(names, knifeFanThrowEvent);
            AddEventBankName(names, knifeFanHitEvent);
            AddEventBankName(names, spotlightBoltLaunchEvent);
            AddEventBankName(names, spotlightBoltHitEvent);
            AddEventBankName(names, fireHoopHitEvent);
            AddEventBankName(names, announcementActEvent);
            AddEventBankName(names, announcementFadeoutEvent);
            AddEventBankName(names, announcementHeadlinerDefeatEvent);
            AddEventBankName(names, announcementShowtimeHeadlinerEvent);
            AddEventBankName(names, menuMusicEvent);
            AddEventBankName(names, stopMenuMusicEvent);
            AddEventBankName(names, gameplayMusicEvent);
            AddEventBankName(names, stopGameplayMusicEvent);
            return names;
        }

        private void OnValidate()
        {
            EnsureWorkflowDefaults();
        }

        private static void AddEventBankName(List<string> names, string eventName)
        {
            if (string.IsNullOrWhiteSpace(eventName) || names.Contains(eventName))
            {
                return;
            }

            names.Add(eventName);
        }

        private static bool EnsureText(ref string value, string defaultValue)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            value = defaultValue;
            return true;
        }

        private static bool EnsureMinimum(ref float value, float defaultValue, float minimum)
        {
            if (!float.IsNaN(value) && value >= minimum)
            {
                return false;
            }

            value = defaultValue;
            return true;
        }

        private static bool EnsureFinite(ref float value, float defaultValue)
        {
            if (!float.IsNaN(value) && !float.IsInfinity(value))
            {
                return false;
            }

            value = defaultValue;
            return true;
        }

        private static bool EnsureClamped01(ref float value, float defaultValue)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                value = defaultValue;
                return true;
            }

            float clamped = Mathf.Clamp01(value);
            if (Mathf.Approximately(value, clamped))
            {
                return false;
            }

            value = clamped;
            return true;
        }
    }
}
