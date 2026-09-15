using TheCircussyOne.Content;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;
using TheCircussyOne.Runtime;
using TheCircussyOne.Stats;
using TheCircussyOne.Visuals;

namespace TheCircussyOne.DI
{
    public sealed partial class GameLifetimeScope
    {
        private sealed class RuntimeCompositionContext
        {
            public ActorRegistry Registry { get; set; }
            public UnityInputService Input { get; set; }
            public RunPauseState PauseState { get; set; }
            public UnityGameTime Time { get; set; }
            public CameraOrbitState CameraOrbit { get; set; }
            public RunStats RunStats { get; set; }
            public RunScheduleConfig RunScheduleConfig { get; set; }
            public RunWorldGenerationConfig RunWorldGenerationConfig { get; set; }

            public WeaponCatalog WeaponCatalog { get; set; }
            public ItemCatalog ItemCatalog { get; set; }
            public ChestCatalog ChestCatalog { get; set; }
            public TicketDepositCatalog TicketDepositCatalog { get; set; }
            public WorldRewardPlacementCatalog WorldRewardPlacementCatalog { get; set; }
            public WorldDecorationCatalog WorldDecorationCatalog { get; set; }
            public XpGemCatalog XpGemCatalog { get; set; }
            public HealthPickupCatalog HealthPickupCatalog { get; set; }
            public HealingPropCatalog HealingPropCatalog { get; set; }
            public UpgradeCatalog UpgradeCatalog { get; set; }
            public TalentCatalog TalentCatalog { get; set; }
            public PerformerCatalog PerformerCatalog { get; set; }
            public EnemyCatalog EnemyCatalog { get; set; }
            public HeadlinerCatalog HeadlinerCatalog { get; set; }

            public WeaponLoadout WeaponLoadout { get; set; }
            public ItemInventory ItemInventory { get; set; }
            public RunCurrencyState RunCurrencyState { get; set; }
            public RunSeedState RunSeedState { get; set; }
            public RunActScheduleState RunActScheduleState { get; set; }
            public UpgradeRunState UpgradeRunState { get; set; }
            public TalentRunState TalentRunState { get; set; }
            public PerformerRunState PerformerRunState { get; set; }
            public IPerformerPassive PerformerPassive { get; set; }
            public IRunTransitionService RunTransitionService { get; set; }
            public IRunRestarter RunRestarter { get; set; }

            public EnemyFactory EnemyFactory { get; set; }
            public WorldPhysicsQuery WorldPhysicsQuery { get; set; }
            public EnemyEnvironmentSurfaceSampler EnemyEnvironmentSurfaceSampler { get; set; }
            public EnemySupportSurfaceSampler EnemySupportSurfaceSampler { get; set; }
            public WorldSurfaceResolver WorldSurfaceResolver { get; set; }
            public WorldPropPlacementService WorldPropPlacementService { get; set; }
            public WorldRewardSpawnPlanner WorldRewardSpawnPlanner { get; set; }
            public WorldRewardRuntimeRegistry WorldRewardRuntimeRegistry { get; set; }
            public RunWorldGenerationState RunWorldGenerationState { get; set; }
            public RunWorldLayoutState RunWorldLayoutState { get; set; }
            public IRunWorldGenerator RunWorldGenerator { get; set; }
            public PickupSurfaceSampler PickupSurfaceSampler { get; set; }
            public WeaponTargetingQuery WeaponTargetingQuery { get; set; }
            public IInteractableSource InteractableSource { get; set; }

            public ProjectileFactory ProjectileFactory { get; set; }
            public PickupFactory PickupFactory { get; set; }
            public XpDropService XpDropService { get; set; }
            public DamageNumberFactory DamageNumberFactory { get; set; }
            public XpGainCounterFactory XpGainCounterFactory { get; set; }
            public WorldInteractionPromptFactory WorldInteractionPromptFactory { get; set; }
            public EnemySpawnIndicatorFactory EnemySpawnIndicatorFactory { get; set; }
            public VfxEffectFactory VfxEffectFactory { get; set; }
            public WorldAtmosphereFactory WorldAtmosphereFactory { get; set; }

            public DamageNumberSystem DamageNumberSystem { get; set; }
            public XpGainCounterSystem XpGainCounterSystem { get; set; }
            public TicketGainCounterSystem TicketGainCounterSystem { get; set; }
            public WorldRewardTransientVfxSystem WorldRewardTransientVfxSystem { get; set; }
            public WorldInteractionPromptSystem WorldInteractionPromptSystem { get; set; }
            public VfxSystem VfxSystem { get; set; }
            public WorldAtmosphereSystem WorldAtmosphereSystem { get; set; }
            public ProjectileExplosionSystem ProjectileExplosionSystem { get; set; }
            public ProjectileChainVisualSystem ProjectileChainVisualSystem { get; set; }
            public FeedbackService Feedback { get; set; }
        }
    }
}
