using TheCircussyOne.Config;
using TheCircussyOne.Rules;
using TheCircussyOne.Stats;
using TheCircussyOne.Visuals;
using UnityEngine;

namespace TheCircussyOne.Runtime
{
    public sealed class BlockyCircusTerrainSandboxPlaytestRunner : MonoBehaviour
    {
        [SerializeField] private GameConfig gameConfig;
        [SerializeField] private CameraConfig cameraConfig;
        [SerializeField] private DamageFeedbackVisualConfig damageFeedbackConfig;
        [SerializeField] private ActorMotionVisualConfig actorMotionVisualConfig;
        [SerializeField] private VfxVisualConfig vfxConfig;
        [SerializeField] private PlayerView player;
        [SerializeField] private CameraView cameraView;

        private UnityInputService input;
        private UnityGameTime time;
        private CameraOrbitState cameraOrbit;
        private RunStats runStats;
        private GameState gameState;
        private PlayerMovementSystem movement;
        private PlayerMovementVfxSystem movementVfx;
        private CameraFollowSystem cameraFollow;
        private bool initialized;

        public void Configure(
            GameConfig gameConfig,
            CameraConfig cameraConfig,
            DamageFeedbackVisualConfig damageFeedbackConfig,
            ActorMotionVisualConfig actorMotionVisualConfig,
            VfxVisualConfig vfxConfig,
            PlayerView player,
            CameraView cameraView)
        {
            this.gameConfig = gameConfig;
            this.cameraConfig = cameraConfig;
            this.damageFeedbackConfig = damageFeedbackConfig;
            this.actorMotionVisualConfig = actorMotionVisualConfig;
            this.vfxConfig = vfxConfig;
            this.player = player;
            this.cameraView = cameraView;
            initialized = false;
        }

        private void Start()
        {
            EnsureInitialized();
        }

        private void Update()
        {
            EnsureInitialized();
            if (!initialized)
            {
                return;
            }

            movement.Tick();
            movementVfx.Tick();
            player.TickMotionVisuals(actorMotionVisualConfig, time.DeltaTime);
        }

        private void LateUpdate()
        {
            EnsureInitialized();
            if (!initialized)
            {
                return;
            }

            cameraFollow.LateTick();
        }

        private void EnsureInitialized()
        {
            if (initialized || player == null || cameraView == null)
            {
                return;
            }

            gameConfig = ResolveConfig(gameConfig);
            cameraConfig = ResolveConfig(cameraConfig);
            actorMotionVisualConfig = ResolveConfig(actorMotionVisualConfig);
            vfxConfig = ResolveConfig(vfxConfig);

            gameConfig.EnsureWorkflowDefaults();
            cameraConfig.EnsureWorkflowDefaults();
            actorMotionVisualConfig.EnsureWorkflowDefaults();
            vfxConfig.EnsureWorkflowDefaults();

            input = new UnityInputService();
            time = new UnityGameTime();
            cameraOrbit = new CameraOrbitState
            {
                Yaw = cameraConfig.orbitYaw,
                Pitch = cameraConfig.orbitPitch
            };
            runStats = new RunStats(gameConfig);
            gameState = new GameState(gameConfig, runStats);

            player.ApplyConfig(gameConfig, damageFeedbackConfig);
            player.ApplyMoveDustVfxConfig(vfxConfig);
            player.ApplyJumpTrailVfxConfig(vfxConfig);
            cameraView.ApplyInitialPose(player, cameraConfig);

            movement = new PlayerMovementSystem(
                gameConfig,
                player,
                gameState,
                input,
                time,
                cameraOrbit,
                actorMotionVisualConfig,
                vfx: null,
                runStats);
            movementVfx = new PlayerMovementVfxSystem(vfxConfig, player, gameState);
            cameraFollow = new CameraFollowSystem(cameraConfig, player, cameraView, time, input, cameraOrbit);
            initialized = true;
        }

        private static T ResolveConfig<T>(T config) where T : ScriptableObject
        {
            return config != null ? config : ScriptableObject.CreateInstance<T>();
        }
    }
}
