using UnityEngine;
using VContainer.Unity;
using TheCircussyOne.Config;
using TheCircussyOne.Visuals;

namespace TheCircussyOne.Runtime
{
    public sealed class GameBootstrapper : IStartable
    {
        private readonly GameConfig _config;
        private readonly CameraConfig _cameraConfig;
        private readonly DamageFeedbackVisualConfig _damageFeedbackConfig;
        private readonly PlayerView _player;
        private readonly CameraView _camera;

        public GameBootstrapper(GameConfig config, CameraConfig cameraConfig, DamageFeedbackVisualConfig damageFeedbackConfig, PlayerView player, CameraView camera)
        {
            _config = config;
            _cameraConfig = cameraConfig;
            _damageFeedbackConfig = damageFeedbackConfig;
            _player = player;
            _camera = camera;
        }

        public void Start()
        {
            Application.runInBackground = true;
            _player.ApplyConfig(_config, _damageFeedbackConfig);
            _camera.ApplyInitialPose(_player, _cameraConfig);
        }
    }
}
