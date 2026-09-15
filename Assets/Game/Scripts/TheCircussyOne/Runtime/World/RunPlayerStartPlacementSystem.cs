using TheCircussyOne.Config;
using TheCircussyOne.Visuals;

namespace TheCircussyOne.Runtime
{
    public sealed class RunPlayerStartPlacementSystem
    {
        private readonly PlayerView player;
        private readonly CameraView camera;
        private readonly CameraConfig cameraConfig;

        public RunPlayerStartPlacementSystem(
            PlayerView player = null,
            CameraView camera = null,
            CameraConfig cameraConfig = null)
        {
            this.player = player;
            this.camera = camera;
            this.cameraConfig = cameraConfig;
        }

        public bool Apply(WorldGenerationResult result, bool applyCameraPose = true)
        {
            if (player == null || !result.HasPlayerStart)
            {
                return false;
            }

            player.Teleport(result.PlayerStart);
            if (applyCameraPose && camera != null && cameraConfig != null)
            {
                camera.ApplyInitialPose(player, cameraConfig);
            }

            return true;
        }
    }
}
