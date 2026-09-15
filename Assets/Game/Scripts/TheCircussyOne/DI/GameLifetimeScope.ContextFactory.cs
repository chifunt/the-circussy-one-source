using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using TheCircussyOne.Runtime;
using TheCircussyOne.Stats;
using TheCircussyOne.Visuals;

namespace TheCircussyOne.DI
{
    public sealed partial class GameLifetimeScope
    {
        private RuntimeCompositionContext CreateRuntimeCompositionContext()
        {
            InitializeRuntimeDefaults();

            var context = new RuntimeCompositionContext();
            ComposeRuntimeContent(context);
            ComposeRuntimeSceneFallbacks(context);
            ComposeCoreRuntime(context);
            ComposeWorldRuntime(context);
            ComposeCombatRuntime(context);
            ComposeFeedbackRuntime(context);
            return context;
        }

        private void InitializeRuntimeDefaults()
        {
            runtimeDefaults = new GameRuntimeDefaults(gameConfig, vfxConfig);
            actorMotionVisualConfig = runtimeDefaults.ResolveActorMotionVisualConfig(actorMotionVisualConfig);
            worldInteractionPromptConfig = runtimeDefaults.ResolveWorldInteractionPromptVisualConfig(worldInteractionPromptConfig);
            if (gameAudioConfig == null)
            {
                gameAudioConfig = GameAudioConfig.CreateRuntimeDefault();
                ownsRuntimeAudioConfig = true;
            }
            else
            {
                gameAudioConfig.EnsureWorkflowDefaults();
                ownsRuntimeAudioConfig = false;
            }

            if (gameHapticsConfig == null)
            {
                gameHapticsConfig = GameHapticsConfig.CreateRuntimeDefault();
                ownsRuntimeHapticsConfig = true;
            }
            else
            {
                gameHapticsConfig.EnsureWorkflowDefaults();
                ownsRuntimeHapticsConfig = false;
            }

            runtimeDefaults.DestroyOrphanedWorldInteractionPromptViews();
        }

        private void ComposeRuntimeContent(RuntimeCompositionContext context)
        {
            runScheduleConfig = runtimeDefaults.ResolveRunScheduleConfig(runScheduleConfig);
            context.RunScheduleConfig = runScheduleConfig;
            runWorldGenerationConfig = runtimeDefaults.ResolveRunWorldGenerationConfig(runWorldGenerationConfig);
            context.RunWorldGenerationConfig = runWorldGenerationConfig;
            context.WeaponCatalog = runtimeDefaults.ResolveWeaponCatalog(weaponCatalog);
            context.ItemCatalog = runtimeDefaults.ResolveItemCatalog(itemCatalog);
            context.ChestCatalog = runtimeDefaults.ResolveChestCatalog(chestCatalog);
            context.TicketDepositCatalog = runtimeDefaults.ResolveTicketDepositCatalog(ticketDepositCatalog);
            context.WorldRewardPlacementCatalog = runtimeDefaults.ResolveWorldRewardPlacementCatalog(worldRewardPlacementCatalog);
            context.WorldDecorationCatalog = runtimeDefaults.ResolveWorldDecorationCatalog(worldDecorationCatalog);
            context.XpGemCatalog = runtimeDefaults.ResolveXpGemCatalog(xpGemCatalog);
            context.HealthPickupCatalog = runtimeDefaults.ResolveHealthPickupCatalog(healthPickupCatalog);
            context.HealingPropCatalog = runtimeDefaults.ResolveHealingPropCatalog(healingPropCatalog, context.HealthPickupCatalog);
            context.UpgradeCatalog = runtimeDefaults.ResolveUpgradeCatalog(upgradeCatalog);
            context.TalentCatalog = runtimeDefaults.ResolveTalentCatalog(talentCatalog);
            context.PerformerCatalog = runtimeDefaults.ResolvePerformerCatalog(performerCatalog, context.WeaponCatalog);
            context.EnemyCatalog = runtimeDefaults.ResolveEnemyCatalog(enemyCatalog, context.XpGemCatalog);
            context.HeadlinerCatalog = runtimeDefaults.ResolveHeadlinerCatalog(headlinerCatalog, context.EnemyCatalog);
        }

        private void ComposeRuntimeSceneFallbacks(RuntimeCompositionContext context)
        {
            orbitWeaponRoot = runtimeDefaults.ResolveRuntimeRoot(orbitWeaponRoot, "Orbit Weapons");
            fireHoopOrbitPrefab = runtimeDefaults.ResolveFireHoopOrbitPrefab(fireHoopOrbitPrefab, context.WeaponCatalog);
            fireHoopAreaTelegraphPrefab = runtimeDefaults.ResolveFireHoopAreaTelegraphPrefab(fireHoopAreaTelegraphPrefab);
            worldAmbientDustPrefab = runtimeDefaults.ResolveWorldAmbientDustPrefab(worldAmbientDustPrefab);
            worldInteractionPromptRoot = runtimeDefaults.ResolveWorldInteractionPromptRoot(worldInteractionPromptRoot);
            worldInteractionPromptPrefab = runtimeDefaults.ResolveWorldInteractionPromptPrefab(worldInteractionPromptPrefab, worldInteractionPromptConfig);
            upgradeSelection = runtimeDefaults.ResolveHudComponent(upgradeSelection, hud);
            performerSelection = runtimeDefaults.ResolveHudComponent(performerSelection, hud);
            rewardReveal = runtimeDefaults.ResolveHudComponent(rewardReveal, hud);
            pauseMenu = runtimeDefaults.ResolveHudComponent(pauseMenu, hud);
            damageVignette = runtimeDefaults.ResolveHudComponent(damageVignette, hud);
            HudVisualConfig hudVisualConfig = hud != null ? hud.Config : null;
            upgradeSelection?.ApplyConfig(hudVisualConfig);
            performerSelection?.ApplyConfig(hudVisualConfig);
            rewardReveal?.ApplyConfig(hudVisualConfig);
            pauseMenu?.ApplyConfig(hudVisualConfig);
        }

        private void ComposeCoreRuntime(RuntimeCompositionContext context)
        {
            context.Registry = new ActorRegistry();
            ActiveActorRegistry = context.Registry;
            context.Input = new UnityInputService();
            context.PauseState = new RunPauseState();
            context.Time = new UnityGameTime(context.PauseState);
            context.CameraOrbit = new CameraOrbitState { Yaw = cameraConfig.orbitYaw, Pitch = cameraConfig.orbitPitch };
            context.RunStats = new RunStats(gameConfig);
            ActiveRunStats = context.RunStats;

            context.WeaponLoadout = new WeaponLoadout(context.WeaponCatalog);
            context.ItemInventory = new ItemInventory(context.RunStats);
            context.RunCurrencyState = new RunCurrencyState();
            context.RunSeedState = new RunSeedState();
            context.RunActScheduleState = new RunActScheduleState(context.RunScheduleConfig);
            context.UpgradeRunState = new UpgradeRunState();
            context.TalentRunState = new TalentRunState();
            context.PerformerRunState = new PerformerRunState();
            context.PerformerPassive = new StaticStatPerformerPassive();
            context.RunTransitionService = new RunTransitionService(hud != null ? hud.Config : null, context.PauseState, hud != null ? hud.PanelSettings : null);
            context.RunRestarter = new SceneRunRestarter(context.RunTransitionService);
        }

        private void ComposeWorldRuntime(RuntimeCompositionContext context)
        {
            context.WorldPhysicsQuery = new WorldPhysicsQuery();
            context.EnemyEnvironmentSurfaceSampler = new EnemyEnvironmentSurfaceSampler(context.WorldPhysicsQuery);
            context.EnemySupportSurfaceSampler = new EnemySupportSurfaceSampler();
            context.WorldSurfaceResolver = new WorldSurfaceResolver(context.WorldPhysicsQuery);
            context.WorldPropPlacementService = new WorldPropPlacementService(context.WorldPhysicsQuery);
            context.RunWorldGenerationState = new RunWorldGenerationState();
            context.RunWorldLayoutState = new RunWorldLayoutState(context.RunWorldGenerationConfig.defaultLayoutMode);
            context.WorldRewardSpawnPlanner = new WorldRewardSpawnPlanner(context.WorldRewardPlacementCatalog, player, context.RunWorldGenerationState);
            context.WorldRewardRuntimeRegistry = new WorldRewardRuntimeRegistry();
            context.RunWorldGenerator = new ProceduralRunWorldGenerator(
                context.RunWorldGenerationState,
                context.RunWorldGenerationConfig,
                gameConfig,
                context.RunWorldLayoutState,
                context.WorldPropPlacementService);
            context.PickupSurfaceSampler = new PickupSurfaceSampler(context.WorldPhysicsQuery);
            context.InteractableSource = new SceneInteractableSource();
        }

        private void ComposeCombatRuntime(RuntimeCompositionContext context)
        {
            context.EnemyFactory = new EnemyFactory(gameConfig, damageFeedbackConfig, context.EnemyCatalog, context.Registry, enemyPrefab, enemyRoot);
            context.WeaponTargetingQuery = new WeaponTargetingQuery(context.WorldPhysicsQuery);
            context.ProjectileFactory = new ProjectileFactory(damageFeedbackConfig, context.Registry, context.Time, projectilePrefab, projectileRoot, context.RunStats);
            context.PickupFactory = new PickupFactory(gameConfig, damageFeedbackConfig, vfxConfig, context.Registry, pickupPrefab, pickupRoot, context.PickupSurfaceSampler);
            context.XpDropService = new XpDropService(context.XpGemCatalog, context.PickupFactory, context.RunSeedState);
        }

        private void ComposeFeedbackRuntime(RuntimeCompositionContext context)
        {
            context.DamageNumberFactory = new DamageNumberFactory(damageFeedbackConfig, damageNumberPrefab, damageNumberRoot);
            context.XpGainCounterFactory = new XpGainCounterFactory(xpGainCounterConfig, xpGainCounterPrefab, xpGainCounterRoot);
            context.WorldInteractionPromptFactory = new WorldInteractionPromptFactory(worldInteractionPromptConfig, worldInteractionPromptPrefab, worldInteractionPromptRoot);
            context.EnemySpawnIndicatorFactory = new EnemySpawnIndicatorFactory(enemySpawnVisualConfig, enemySpawnIndicatorPrefab, enemySpawnIndicatorRoot);
            ParticleEffectView activeProjectileBounceBurstPrefab = runtimeDefaults.ResolveParticleEffectPrefab(
                projectileBounceBurstPrefab,
                VfxEffectId.ProjectileBounceBurst,
                "Projectile Bounce Burst Runtime Prefab");
            ParticleEffectView activeKnifeHitSparksPrefab = runtimeDefaults.ResolveParticleEffectPrefab(
                knifeHitSparksPrefab,
                VfxEffectId.KnifeHitSparks,
                "Knife Hit Sparks Runtime Prefab");
            context.VfxEffectFactory = new VfxEffectFactory(
                vfxRoot,
                projectileMuzzlePuffPrefab,
                enemyHitSparksPrefab,
                enemyDeathBurstPrefab,
                xpPickupCollectPopPrefab,
                playerDamageBurstPrefab,
                levelUpBurstPrefab,
                playerJumpTakeoffPrefab,
                playerJumpLandPrefab,
                projectileBounceBurst: activeProjectileBounceBurstPrefab,
                knifeHitSparks: activeKnifeHitSparksPrefab);
            context.WorldAtmosphereFactory = new WorldAtmosphereFactory(worldAmbientDustPrefab, vfxRoot);

            var ticketGainCounterFactory = new XpGainCounterFactory(xpGainCounterConfig, xpGainCounterPrefab, xpGainCounterRoot, "Ticket Gain Counter");
            context.DamageNumberSystem = new DamageNumberSystem(damageFeedbackConfig, context.DamageNumberFactory, gameCamera, context.Time);
            context.XpGainCounterSystem = new XpGainCounterSystem(xpGainCounterConfig, player, gameCamera, context.Time, context.XpGainCounterFactory);
            context.TicketGainCounterSystem = new TicketGainCounterSystem(xpGainCounterConfig, context.RunCurrencyState, player, gameCamera, context.Time, ticketGainCounterFactory);
            context.WorldRewardTransientVfxSystem = new WorldRewardTransientVfxSystem(context.Time, new WorldRewardTransientVfxFactory(vfxRoot));
            context.WorldInteractionPromptSystem = new WorldInteractionPromptSystem(worldInteractionPromptConfig, context.WorldInteractionPromptFactory, gameCamera, context.PauseState);
            runtimeWorldInteractionPromptSystem = context.WorldInteractionPromptSystem;
            context.VfxSystem = new VfxSystem(vfxConfig, context.VfxEffectFactory);
            context.WorldAtmosphereSystem = new WorldAtmosphereSystem(vfxConfig, context.WorldAtmosphereFactory, player);
            context.ProjectileExplosionSystem = new ProjectileExplosionSystem(vfxConfig, projectileExplosionPrefab, vfxRoot, context.Time);
            context.ProjectileChainVisualSystem = new ProjectileChainVisualSystem(vfxRoot, context.Time);
            context.Feedback = new FeedbackService(gameConfig, damageFeedbackConfig, actorMotionVisualConfig, player, gameCamera, shootFeedback, hitFeedback, playerDamageFeedback, context.VfxSystem);
        }
    }
}
