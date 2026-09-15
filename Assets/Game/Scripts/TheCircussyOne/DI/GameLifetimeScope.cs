using MoreMountains.Feedbacks;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Runtime;
using TheCircussyOne.Stats;
using TheCircussyOne.Visuals;

namespace TheCircussyOne.DI
{
    [RequireComponent(typeof(GameLoopRunner))]
    public sealed partial class GameLifetimeScope : LifetimeScope
    {
        [Header("Config")]
        [SerializeField] private GameConfig gameConfig;
        [SerializeField] private RunScheduleConfig runScheduleConfig;
        [SerializeField] private RunWorldGenerationConfig runWorldGenerationConfig;
        [SerializeField] private CameraConfig cameraConfig;
        [SerializeField] private DamageFeedbackVisualConfig damageFeedbackConfig;
        [SerializeField] private ActorMotionVisualConfig actorMotionVisualConfig;
        [SerializeField] private EnemySpawnVisualConfig enemySpawnVisualConfig;
        [SerializeField] private VfxVisualConfig vfxConfig;
        [SerializeField] private GameAudioConfig gameAudioConfig;
        [SerializeField] private GameHapticsConfig gameHapticsConfig;
        [SerializeField] private XpGainCounterVisualConfig xpGainCounterConfig;
        [SerializeField] private WorldInteractionPromptVisualConfig worldInteractionPromptConfig;
        [SerializeField] private WeaponCatalog weaponCatalog;
        [SerializeField] private ItemCatalog itemCatalog;
        [SerializeField] private ChestCatalog chestCatalog;
        [SerializeField] private TicketDepositCatalog ticketDepositCatalog;
        [SerializeField] private WorldRewardPlacementCatalog worldRewardPlacementCatalog;
        [SerializeField] private WorldDecorationCatalog worldDecorationCatalog;
        [SerializeField] private XpGemCatalog xpGemCatalog;
        [SerializeField] private HealthPickupCatalog healthPickupCatalog;
        [SerializeField] private HealingPropCatalog healingPropCatalog;
        [SerializeField] private UpgradeCatalog upgradeCatalog;
        [SerializeField] private TalentCatalog talentCatalog;
        [SerializeField] private PerformerCatalog performerCatalog;
        [SerializeField] private EnemyCatalog enemyCatalog;
        [SerializeField] private HeadlinerCatalog headlinerCatalog;

        [Header("Scene Views")]
        [SerializeField] private PlayerView player;
        [SerializeField] private CameraView gameCamera;
        [SerializeField] private HudView hud;
        [SerializeField] private DamageVignetteView damageVignette;
        [SerializeField] private UpgradeSelectionView upgradeSelection;
        [SerializeField] private PerformerSelectionView performerSelection;
        [SerializeField] private RewardRevealView rewardReveal;
        [SerializeField] private PauseMenuView pauseMenu;

        [Header("Runtime Roots")]
        [SerializeField] private Transform enemyRoot;
        [SerializeField] private Transform projectileRoot;
        [SerializeField] private Transform orbitWeaponRoot;
        [SerializeField] private Transform pickupRoot;
        [SerializeField] private Transform damageNumberRoot;
        [SerializeField] private Transform xpGainCounterRoot;
        [SerializeField] private Transform worldInteractionPromptRoot;
        [SerializeField] private Transform enemySpawnIndicatorRoot;
        [SerializeField] private Transform vfxRoot;

        [Header("Prefabs")]
        [SerializeField] private EnemyView enemyPrefab;
        [SerializeField] private ProjectileView projectilePrefab;
        [SerializeField] private OrbitWeaponView fireHoopOrbitPrefab;
        [SerializeField] private FireHoopAreaTelegraphView fireHoopAreaTelegraphPrefab;
        [SerializeField] private PickupView pickupPrefab;
        [SerializeField] private DamageNumberView damageNumberPrefab;
        [SerializeField] private XpGainCounterView xpGainCounterPrefab;
        [SerializeField] private WorldInteractionPromptView worldInteractionPromptPrefab;
        [SerializeField] private EnemySpawnIndicatorView enemySpawnIndicatorPrefab;
        [SerializeField] private ParticleEffectView projectileMuzzlePuffPrefab;
        [SerializeField] private ParticleEffectView enemyHitSparksPrefab;
        [SerializeField] private ParticleEffectView knifeHitSparksPrefab;
        [SerializeField] private ParticleEffectView enemyDeathBurstPrefab;
        [SerializeField] private ParticleEffectView xpPickupCollectPopPrefab;
        [SerializeField] private ParticleEffectView playerDamageBurstPrefab;
        [SerializeField] private ParticleEffectView levelUpBurstPrefab;
        [SerializeField] private ParticleEffectView playerJumpTakeoffPrefab;
        [SerializeField] private ParticleEffectView playerJumpLandPrefab;
        [SerializeField] private ParticleEffectView projectileBounceBurstPrefab;
        [SerializeField] private ProjectileExplosionView projectileExplosionPrefab;
        [SerializeField] private WorldAmbientDustView worldAmbientDustPrefab;

        [Header("Feel Feedbacks")]
        [SerializeField] private MMF_Player shootFeedback;
        [SerializeField] private MMF_Player hitFeedback;
        [SerializeField] private MMF_Player playerDamageFeedback;

        public RunStats ActiveRunStats { get; private set; }
        public ActorRegistry ActiveActorRegistry { get; private set; }
        private WorldInteractionPromptSystem runtimeWorldInteractionPromptSystem;
        private GameRuntimeDefaults runtimeDefaults;
        private bool ownsRuntimeAudioConfig;
        private bool ownsRuntimeHapticsConfig;

        protected override void Configure(IContainerBuilder builder)
        {
            RuntimeCompositionContext context = CreateRuntimeCompositionContext();
            RegisterRuntimeInstances(builder, context);
            RegisterRuntimeSystems(builder);
            // Game loop ordering remains delegated to GameLoopComposition.ConstructRunner.
            RegisterGameLoop(builder, context.PauseState);
        }

        private void OnDisable()
        {
            DisposeRuntimeWorldInteractionPrompt();
        }

        protected override void OnDestroy()
        {
            DisposeRuntimeWorldInteractionPrompt();
            runtimeDefaults?.DestroyOwnedWorldInteractionPromptObjects(ref worldInteractionPromptRoot, ref worldInteractionPromptPrefab);
            runtimeDefaults?.DestroyOwnedWorldAmbientDustPrefab(ref worldAmbientDustPrefab);
            runtimeDefaults?.DestroyOwnedParticleEffectPrefabs();
            if (ownsRuntimeAudioConfig && gameAudioConfig != null)
            {
                Destroy(gameAudioConfig);
                gameAudioConfig = null;
                ownsRuntimeAudioConfig = false;
            }

            if (ownsRuntimeHapticsConfig && gameHapticsConfig != null)
            {
                Destroy(gameHapticsConfig);
                gameHapticsConfig = null;
                ownsRuntimeHapticsConfig = false;
            }

            runtimeDefaults = null;
            base.OnDestroy();
        }

        private void DisposeRuntimeWorldInteractionPrompt()
        {
            runtimeWorldInteractionPromptSystem?.Dispose();
            runtimeWorldInteractionPromptSystem = null;
        }
    }

}
