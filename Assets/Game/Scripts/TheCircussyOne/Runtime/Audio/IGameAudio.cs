using UnityEngine;
using TheCircussyOne.Content;

namespace TheCircussyOne.Runtime
{
    public enum GameAudioCue
    {
        UiClick,
        UiHover,
        UiSheenLoop,
        InteractionCanceled,
        InteractionHoldLoop,
        ChestOpen,
        StageDoorOpen,
        GameOver,
        LevelUp,
        PlayerHurt,
        RarityUp,
        SnackEat,
        SnackOpen,
        TicketCollect,
        XpCollect,
        EnemyDeath,
        EnemyHit,
        PlayerJump,
        PlayerLand,
        PlayerMovementLoop,
        AnnouncementAct,
        AnnouncementFadeout,
        AnnouncementHeadlinerDefeat,
        AnnouncementShowtimeHeadliner,
        JugglingBallThrow,
        JugglingBallHit,
        CannonLaunch,
        CannonHit,
        KnifeFanThrow,
        KnifeFanHit,
        SpotlightBoltLaunch,
        SpotlightBoltHit,
        FireHoopHit
    }

    public interface IGameAudio
    {
        void Play(GameAudioCue cue);
        void PlayAt(GameAudioCue cue, Vector3 position);
        void PlayXpCollect(Vector3 position);
        void StartLoop(GameAudioCue cue, object owner, Vector3 position, float initialProgress = 0f);
        void UpdateLoop(GameAudioCue cue, object owner, Vector3 position, float progress01);
        void StopLoop(GameAudioCue cue, object owner);
        void StopAllLoops();
    }

    public sealed class NullGameAudio : IGameAudio
    {
        public static readonly NullGameAudio Instance = new();

        private NullGameAudio()
        {
        }

        public void Play(GameAudioCue cue)
        {
        }

        public void PlayAt(GameAudioCue cue, Vector3 position)
        {
        }

        public void PlayXpCollect(Vector3 position)
        {
        }

        public void StartLoop(GameAudioCue cue, object owner, Vector3 position, float initialProgress = 0f)
        {
        }

        public void UpdateLoop(GameAudioCue cue, object owner, Vector3 position, float progress01)
        {
        }

        public void StopLoop(GameAudioCue cue, object owner)
        {
        }

        public void StopAllLoops()
        {
        }
    }

    public static class WeaponAudioCueRules
    {
        public static bool TryGetLaunchCue(WeaponDefinition weapon, out GameAudioCue cue)
        {
            cue = default;
            if (weapon == null)
            {
                return false;
            }

            switch (weapon.Id)
            {
                case FirstPartyWeaponDefaults.JugglingBallId:
                    cue = GameAudioCue.JugglingBallThrow;
                    return true;
                case FirstPartyWeaponDefaults.CannonId:
                    cue = GameAudioCue.CannonLaunch;
                    return true;
                case FirstPartyWeaponDefaults.KnifeFanId:
                    cue = GameAudioCue.KnifeFanThrow;
                    return true;
                case FirstPartyWeaponDefaults.SpotlightBoltId:
                    cue = GameAudioCue.SpotlightBoltLaunch;
                    return true;
                default:
                    return false;
            }
        }

        public static bool TryGetHitCue(WeaponDefinition weapon, out GameAudioCue cue)
        {
            cue = default;
            if (weapon == null)
            {
                return false;
            }

            switch (weapon.Id)
            {
                case FirstPartyWeaponDefaults.JugglingBallId:
                    cue = GameAudioCue.JugglingBallHit;
                    return true;
                case FirstPartyWeaponDefaults.CannonId:
                    cue = GameAudioCue.CannonHit;
                    return true;
                case FirstPartyWeaponDefaults.KnifeFanId:
                    cue = GameAudioCue.KnifeFanHit;
                    return true;
                case FirstPartyWeaponDefaults.SpotlightBoltId:
                    cue = GameAudioCue.SpotlightBoltHit;
                    return true;
                case FirstPartyWeaponDefaults.FireHoopId:
                    cue = GameAudioCue.FireHoopHit;
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsWeapon(WeaponDefinition weapon, string weaponId)
        {
            return weapon != null && string.Equals(weapon.Id, weaponId, System.StringComparison.Ordinal);
        }
    }
}
