using TheCircussyOne.Runtime;
using TheCircussyOne.Visuals;
using VContainer;

namespace TheCircussyOne.DI
{
    public sealed partial class GameLifetimeScope
    {
        private void RegisterRuntimeInstances(IContainerBuilder builder, RuntimeCompositionContext context)
        {
            RegisterConfigInstances(builder);
            RegisterContentInstances(builder, context);
            RegisterSceneViewInstances(builder);
            RegisterCoreRuntimeInstances(builder, context);
            RegisterWorldRuntimeInstances(builder, context);
            RegisterCombatRuntimeInstances(builder, context);
            RegisterFeedbackRuntimeInstances(builder, context);
        }

        private void RegisterConfigInstances(IContainerBuilder builder)
        {
            builder.RegisterInstance(gameConfig);
            builder.RegisterInstance(runScheduleConfig);
            builder.RegisterInstance(runWorldGenerationConfig);
            builder.RegisterInstance(cameraConfig);
            builder.RegisterInstance(damageFeedbackConfig);
            builder.RegisterInstance(actorMotionVisualConfig);
            builder.RegisterInstance(enemySpawnVisualConfig);
            builder.RegisterInstance(vfxConfig);
            builder.RegisterInstance(gameAudioConfig);
            builder.RegisterInstance(gameHapticsConfig);
            builder.RegisterInstance(xpGainCounterConfig);
            builder.RegisterInstance(worldInteractionPromptConfig);
        }

        private static void RegisterContentInstances(IContainerBuilder builder, RuntimeCompositionContext context)
        {
            builder.RegisterInstance(context.WeaponCatalog);
            builder.RegisterInstance(context.ItemCatalog);
            builder.RegisterInstance(context.ChestCatalog);
            builder.RegisterInstance(context.TicketDepositCatalog);
            builder.RegisterInstance(context.WorldRewardPlacementCatalog);
            builder.RegisterInstance(context.WorldDecorationCatalog);
            builder.RegisterInstance(context.XpGemCatalog);
            builder.RegisterInstance(context.HealthPickupCatalog);
            builder.RegisterInstance(context.HealingPropCatalog);
            builder.RegisterInstance(context.UpgradeCatalog);
            builder.RegisterInstance(context.TalentCatalog);
            builder.RegisterInstance(context.PerformerCatalog);
            builder.RegisterInstance(context.EnemyCatalog);
            builder.RegisterInstance(context.HeadlinerCatalog);
        }

        private void RegisterSceneViewInstances(IContainerBuilder builder)
        {
            builder.RegisterInstance(player);
            builder.RegisterInstance(gameCamera);
            builder.RegisterInstance(hud);
            builder.RegisterInstance(damageVignette);
            builder.RegisterInstance(upgradeSelection);
            builder.RegisterInstance(performerSelection);
            builder.RegisterInstance(rewardReveal);
            builder.RegisterInstance(pauseMenu);
            builder.RegisterInstance(fireHoopOrbitPrefab);
            builder.RegisterInstance(fireHoopAreaTelegraphPrefab);
            builder.RegisterInstance(worldAmbientDustPrefab);
            builder.RegisterInstance(orbitWeaponRoot);
        }

        private static void RegisterCoreRuntimeInstances(IContainerBuilder builder, RuntimeCompositionContext context)
        {
            builder.RegisterInstance(context.Registry);
            builder.RegisterInstance<IGameTime>(context.Time);
            builder.RegisterInstance<IInputService>(context.Input);
            builder.RegisterInstance(context.CameraOrbit);
            builder.RegisterInstance(context.RunStats);
            builder.RegisterInstance(context.PauseState);
            builder.RegisterInstance(context.WeaponLoadout);
            builder.RegisterInstance(context.ItemInventory);
            builder.RegisterInstance(context.RunCurrencyState);
            builder.RegisterInstance(context.RunSeedState);
            builder.RegisterInstance(context.RunActScheduleState);
            builder.RegisterInstance(context.UpgradeRunState);
            builder.RegisterInstance(context.TalentRunState);
            builder.RegisterInstance(context.PerformerRunState);
            builder.RegisterInstance(context.PerformerPassive);
            builder.RegisterInstance(context.RunTransitionService);
            builder.RegisterInstance<IRunLoadingStageSink>(context.RunTransitionService);
            builder.RegisterInstance<IRunRestarter>(context.RunRestarter);
        }

        private static void RegisterWorldRuntimeInstances(IContainerBuilder builder, RuntimeCompositionContext context)
        {
            builder.RegisterInstance(context.WorldPhysicsQuery);
            builder.RegisterInstance(context.EnemyEnvironmentSurfaceSampler);
            builder.RegisterInstance(context.EnemySupportSurfaceSampler);
            builder.RegisterInstance(context.WorldSurfaceResolver);
            builder.RegisterInstance(context.WorldPropPlacementService);
            builder.RegisterInstance(context.WorldRewardSpawnPlanner);
            builder.RegisterInstance(context.WorldRewardRuntimeRegistry);
            builder.RegisterInstance(context.RunWorldGenerationState);
            builder.RegisterInstance(context.RunWorldLayoutState);
            builder.RegisterInstance(context.RunWorldGenerator);
            builder.RegisterInstance(context.PickupSurfaceSampler);
            builder.RegisterInstance(context.WeaponTargetingQuery);
            builder.RegisterInstance(context.InteractableSource);
        }

        private static void RegisterCombatRuntimeInstances(IContainerBuilder builder, RuntimeCompositionContext context)
        {
            builder.RegisterInstance(context.EnemyFactory);
            builder.RegisterInstance(context.ProjectileFactory);
            builder.RegisterInstance(context.PickupFactory);
            builder.RegisterInstance(context.XpDropService);
        }

        private static void RegisterFeedbackRuntimeInstances(IContainerBuilder builder, RuntimeCompositionContext context)
        {
            builder.RegisterInstance(context.DamageNumberFactory);
            builder.RegisterInstance(context.XpGainCounterFactory);
            builder.RegisterInstance(context.WorldInteractionPromptFactory);
            builder.RegisterInstance(context.EnemySpawnIndicatorFactory);
            builder.RegisterInstance(context.VfxEffectFactory);
            builder.RegisterInstance(context.WorldAtmosphereFactory);
            builder.RegisterInstance<DamageNumberSystem, IDamageNumberSpawner>(context.DamageNumberSystem);
            builder.RegisterInstance<XpGainCounterSystem, IXpGainCounter>(context.XpGainCounterSystem);
            builder.RegisterInstance(context.TicketGainCounterSystem);
            builder.RegisterInstance<WorldRewardTransientVfxSystem, IWorldRewardTransientVfxSpawner>(context.WorldRewardTransientVfxSystem);
            builder.RegisterInstance<WorldInteractionPromptSystem, IWorldInteractionPrompt>(context.WorldInteractionPromptSystem);
            builder.RegisterInstance<VfxSystem, IVfxSpawner>(context.VfxSystem);
            builder.RegisterInstance(context.WorldAtmosphereSystem);
            builder.RegisterInstance<ProjectileExplosionSystem, IProjectileExplosionSpawner>(context.ProjectileExplosionSystem);
            builder.RegisterInstance<ProjectileChainVisualSystem, IProjectileChainVisualSpawner>(context.ProjectileChainVisualSystem);
            builder.RegisterInstance(context.Feedback);
        }

        private static void RegisterRuntimeSystems(IContainerBuilder builder)
        {
            RegisterCoreSystems(builder);
            RegisterWorldSystems(builder);
            RegisterCombatSystems(builder);
            RegisterUpgradeAndRewardSystems(builder);
            RegisterFeedbackAndUiSystems(builder);
        }

        private static void RegisterCoreSystems(IContainerBuilder builder)
        {
            builder.Register<GameState>(Lifetime.Singleton);
            builder.Register<RunPhaseState>(Lifetime.Singleton);
            builder.Register<RunPhaseSystem>(Lifetime.Singleton);
            builder.Register<RunActScheduleSystem>(Lifetime.Singleton);
            builder.Register<RunActProgressionSystem>(Lifetime.Singleton);
            builder.Register<RunPlayerStartPlacementSystem>(Lifetime.Singleton);
            builder.Register<RunStartupPrewarmSystem>(Lifetime.Singleton);
            builder.Register<RunWorldClearer>(Lifetime.Singleton);
            builder.Register<RunWorldLifecycleSystem>(Lifetime.Singleton);
            builder.Register<WwiseRuntimeAudioBackend>(Lifetime.Singleton).As<IWwiseRuntimeAudioBackend>();
            builder.Register<GameAudioService>(Lifetime.Singleton).AsSelf().As<IGameAudio>();
            builder.Register<GameAudioPresenter>(Lifetime.Singleton);
            builder.Register<GameUiAudioPresenter>(Lifetime.Singleton);
            builder.Register<UnityGamepadHapticsBackend>(Lifetime.Singleton).As<IGamepadHapticsBackend>();
            builder.Register<GameHapticsService>(Lifetime.Singleton).AsSelf().As<IGameHaptics>();
            builder.Register<GameHapticsPresenter>(Lifetime.Singleton);
            builder.Register<GameUiHapticsPresenter>(Lifetime.Singleton);
            builder.Register<HeadlinerDefeatState>(Lifetime.Singleton);
            builder.Register<HeadlinerSystem>(Lifetime.Singleton);
            builder.Register<FinishPortalFactory>(Lifetime.Singleton);
            builder.Register<ArenaBarrierFactory>(Lifetime.Singleton);
            builder.Register<BigTopEnvironmentFactory>(Lifetime.Singleton);
            builder.Register<WorldDressingFactory>(Lifetime.Singleton);
            builder.Register<WorldDecorationFactory>(Lifetime.Singleton);
            builder.Register<GameBootstrapper>(Lifetime.Singleton);
            builder.Register<ControlInputModeTracker>(Lifetime.Singleton);
            builder.Register<RuntimeCursorSystem>(Lifetime.Singleton);
            builder.Register<RunDebugCommandService>(Lifetime.Singleton);
            builder.Register<RunResetCoordinator>(Lifetime.Singleton);
            builder.Register<RunRetryService>(Lifetime.Singleton);
            builder.Register<SceneMainMenuReturner>(Lifetime.Singleton).As<IMainMenuReturner>();
            builder.Register<PauseMenuPresenter>(Lifetime.Singleton);
            builder.Register<GameOverPauseSystem>(Lifetime.Singleton);
            builder.Register<WorldAnimationPauseSystem>(Lifetime.Singleton);
            builder.Register<PlayerMovementSystem>(Lifetime.Singleton);
            builder.Register<PlayerMovementVfxSystem>(Lifetime.Singleton);
            builder.Register<CameraFollowSystem>(Lifetime.Singleton);
            builder.Register<DebugRunRestartSystem>(Lifetime.Singleton);
        }

        private static void RegisterWorldSystems(IContainerBuilder builder)
        {
            builder.Register<WorldRewardSpawnSystem>(Lifetime.Singleton);
            builder.Register<ChestSystem>(Lifetime.Singleton);
            builder.Register<TicketDepositSystem>(Lifetime.Singleton);
            builder.Register<HealingPropSystem>(Lifetime.Singleton);
            builder.Register<FinishPortalSystem>(Lifetime.Singleton);
            builder.Register<ArenaBarrierSystem>(Lifetime.Singleton);
            builder.Register<BigTopEnvironmentSystem>(Lifetime.Singleton);
            builder.Register<WorldDressingSystem>(Lifetime.Singleton);
            builder.Register<WorldDecorationSystem>(Lifetime.Singleton);
            builder.Register<InteractionSystem>(Lifetime.Singleton);
            builder.Register<WorldRewardAnimationSystem>(Lifetime.Singleton);
            builder.Register<WorldRewardTransientVfxTickSystem>(Lifetime.Singleton);
            builder.Register<PickupSystem>(Lifetime.Singleton);
        }

        private static void RegisterCombatSystems(IContainerBuilder builder)
        {
            builder.Register<EnemyDamageService>(Lifetime.Singleton);
            builder.Register<EnemySpawnIndicatorSystem>(Lifetime.Singleton);
            builder.Register<EnemySpawnSystem>(Lifetime.Singleton);
            builder.Register<EnemyDirectorSystem>(Lifetime.Singleton);
            builder.Register<EnemyClimbSystem>(Lifetime.Singleton);
            builder.Register<EnemyContactDamageSystem>(Lifetime.Singleton);
            builder.Register<EnemyBodyCollisionSystem>(Lifetime.Singleton);
            builder.Register<AutoWeaponSystem>(Lifetime.Singleton);
            builder.Register<OrbitWeaponSystem>(Lifetime.Singleton);
            builder.Register<ProjectileSystem>(Lifetime.Singleton);
        }

        private static void RegisterUpgradeAndRewardSystems(IContainerBuilder builder)
        {
            builder.Register<PerformerRunComposer>(Lifetime.Singleton);
            builder.Register<PerformerSelectionPresenter>(Lifetime.Singleton);
            builder.Register<UpgradeChoiceProvider>(Lifetime.Singleton);
            builder.Register<TalentEffectApplier>(Lifetime.Singleton);
            builder.Register<UpgradeEffectApplier>(Lifetime.Singleton);
            builder.Register<UpgradeSelectionSystem>(Lifetime.Singleton);
            builder.Register<UpgradeSelectionPresenter>(Lifetime.Singleton);
            builder.Register<RewardRevealPresenter>(Lifetime.Singleton);
        }

        private static void RegisterFeedbackAndUiSystems(IContainerBuilder builder)
        {
            builder.Register<ActorMotionVisualSystem>(Lifetime.Singleton);
            builder.Register<HudPresenter>(Lifetime.Singleton);
            builder.Register<PlayerDamageVignettePresenter>(Lifetime.Singleton);
            builder.Register<PlayerHealthVisualPresenter>(Lifetime.Singleton);
            builder.Register<LevelUpVfxPresenter>(Lifetime.Singleton);
        }

        private void RegisterGameLoop(IContainerBuilder builder, RunPauseState pauseState)
        {
            builder.RegisterBuildCallback(container =>
            {
                var runner = GetComponent<GameLoopRunner>();
                if (container.Resolve<IRunRestarter>() is SceneRunRestarter sceneRunRestarter)
                {
                    sceneRunRestarter.SetResetCoordinator(container.Resolve<RunResetCoordinator>());
                }

                GameLoopComposition.ConstructRunner(runner, container, pauseState);
            });
        }
    }
}
