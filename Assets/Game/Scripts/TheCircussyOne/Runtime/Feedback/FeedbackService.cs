using MoreMountains.Feedbacks;
using UnityEngine;
using TheCircussyOne.Config;
using TheCircussyOne.Visuals;

namespace TheCircussyOne.Runtime
{
    public sealed class FeedbackService
    {
        private readonly GameConfig _config;
        private readonly DamageFeedbackVisualConfig _damageFeedbackConfig;
        private readonly ActorMotionVisualConfig _actorMotionConfig;
        private readonly PlayerView _player;
        private readonly CameraView _camera;
        private readonly MMF_Player _shootFeedback;
        private readonly MMF_Player _hitFeedback;
        private readonly MMF_Player _playerDamageFeedback;
        private readonly IVfxSpawner _vfx;

        public FeedbackService(
            GameConfig config,
            DamageFeedbackVisualConfig damageFeedbackConfig,
            ActorMotionVisualConfig actorMotionConfig,
            PlayerView player,
            CameraView camera,
            MMF_Player shootFeedback,
            MMF_Player hitFeedback,
            MMF_Player playerDamageFeedback,
            IVfxSpawner vfx = null)
        {
            _config = config;
            _damageFeedbackConfig = damageFeedbackConfig;
            _actorMotionConfig = actorMotionConfig;
            _player = player;
            _camera = camera;
            _shootFeedback = shootFeedback;
            _hitFeedback = hitFeedback;
            _playerDamageFeedback = playerDamageFeedback;
            _vfx = vfx;
        }

        public void PlayShoot(Vector3 position)
        {
            PlayShoot(position, Vector3.forward);
        }

        public void PlayShoot(Vector3 position, Vector3 direction)
        {
            _vfx?.Show(VfxEffectId.ProjectileMuzzlePuff, position, direction);
            Play(_shootFeedback, position, 0.8f);
        }

        public void PlayEnemyHit(Vector3 position)
        {
            Play(_hitFeedback, position, 1f);
        }

        public void PlayPlayerDamage()
        {
            _player.PlayDamageFlash(_damageFeedbackConfig);
            _vfx?.Show(VfxEffectId.PlayerDamageBurst, _player.Position + Vector3.up * 0.85f);

            if (_actorMotionConfig != null)
            {
                _player.PlayDamagePulse(_actorMotionConfig);
            }
            else
            {
                _player.PlayDamagePulse(
                    _config.hitPulseSeconds * 2f,
                    _damageFeedbackConfig != null ? _damageFeedbackConfig.playerDamagePulseEase : TheCircussyOne.Rules.EaseSettings.OutQuad,
                    _damageFeedbackConfig != null ? _damageFeedbackConfig.playerDamagePulseScale : 1.18f);
            }

            Play(_playerDamageFeedback, _player.Position, 1f);

            if (_camera != null)
            {
                _camera.PlayRelativeShake(_damageFeedbackConfig);
            }
        }

        private static void Play(MMF_Player player, Vector3 position, float intensity)
        {
            if (player == null)
            {
                return;
            }

            player.PlayFeedbacks(position, intensity);
        }
    }
}
