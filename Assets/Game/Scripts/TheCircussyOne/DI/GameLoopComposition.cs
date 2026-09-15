using System;
using TheCircussyOne.Runtime;
using VContainer;
using VContainer.Unity;

namespace TheCircussyOne.DI
{
    public static class GameLoopComposition
    {
        public static void ConstructRunner(GameLoopRunner runner, IObjectResolver container, RunPauseState pauseState)
        {
            if (runner == null)
            {
                throw new InvalidOperationException("GameLoopRunner is required on the GameLifetimeScope GameObject.");
            }

            runner.Construct(
                new IStartable[]
                {
                    container.Resolve<GameBootstrapper>(),
                    container.Resolve<GameAudioService>(),
                    container.Resolve<GameAudioPresenter>(),
                    container.Resolve<GameUiAudioPresenter>(),
                    container.Resolve<GameHapticsService>(),
                    container.Resolve<GameHapticsPresenter>(),
                    container.Resolve<GameUiHapticsPresenter>(),
                    container.Resolve<RunActScheduleSystem>(),
                    container.Resolve<RunActProgressionSystem>(),
                    container.Resolve<HeadlinerSystem>(),
                    container.Resolve<PerformerSelectionPresenter>(),
                    container.Resolve<UpgradeSelectionSystem>(),
                    container.Resolve<UpgradeSelectionPresenter>(),
                    container.Resolve<HudPresenter>(),
                    container.Resolve<RewardRevealPresenter>(),
                    container.Resolve<PauseMenuPresenter>(),
                    container.Resolve<GameOverPauseSystem>(),
                    container.Resolve<WorldAtmosphereSystem>(),
                    container.Resolve<WorldAnimationPauseSystem>(),
                    container.Resolve<TicketGainCounterSystem>(),
                    container.Resolve<PlayerDamageVignettePresenter>(),
                    container.Resolve<RuntimeCursorSystem>(),
                    container.Resolve<WorldRewardSpawnSystem>(),
                    container.Resolve<FinishPortalSystem>(),
                    container.Resolve<PlayerHealthVisualPresenter>(),
                    container.Resolve<LevelUpVfxPresenter>()
                },
                new ITickable[]
                {
                    container.Resolve<ControlInputModeTracker>(),
                    container.Resolve<GameAudioService>(),
                    container.Resolve<GameUiAudioPresenter>(),
                    container.Resolve<GameHapticsService>(),
                    container.Resolve<GameUiHapticsPresenter>(),
                    container.Resolve<RunPhaseSystem>(),
                    container.Resolve<RunActScheduleSystem>(),
                    container.Resolve<RunActProgressionSystem>(),
                    container.Resolve<RewardRevealPresenter>(),
                    container.Resolve<PauseMenuPresenter>(),
                    container.Resolve<RuntimeCursorSystem>(),
                    container.Resolve<DebugRunRestartSystem>(),
                    container.Resolve<WorldAnimationPauseSystem>(),
                    container.Resolve<PerformerSelectionPresenter>(),
                    container.Resolve<PlayerMovementSystem>(),
                    container.Resolve<PlayerMovementVfxSystem>(),
                    container.Resolve<EnemySpawnSystem>(),
                    container.Resolve<EnemySpawnIndicatorSystem>(),
                    container.Resolve<EnemyDirectorSystem>(),
                    container.Resolve<EnemyClimbSystem>(),
                    container.Resolve<EnemyContactDamageSystem>(),
                    container.Resolve<EnemyBodyCollisionSystem>(),
                    container.Resolve<ArenaBarrierSystem>(),
                    container.Resolve<BigTopEnvironmentSystem>(),
                    container.Resolve<InteractionSystem>(),
                    container.Resolve<WorldRewardAnimationSystem>(),
                    container.Resolve<WorldInteractionPromptSystem>(),
                    container.Resolve<AutoWeaponSystem>(),
                    container.Resolve<OrbitWeaponSystem>(),
                    container.Resolve<ProjectileSystem>(),
                    container.Resolve<UpgradeSelectionPresenter>(),
                    container.Resolve<ActorMotionVisualSystem>(),
                    container.Resolve<PickupSystem>(),
                    container.Resolve<HudPresenter>(),
                    container.Resolve<PlayerDamageVignettePresenter>(),
                    container.Resolve<VfxSystem>(),
                    container.Resolve<WorldRewardTransientVfxTickSystem>(),
                    container.Resolve<WorldAtmosphereSystem>(),
                    container.Resolve<ProjectileExplosionSystem>(),
                    container.Resolve<ProjectileChainVisualSystem>()
                },
                Array.Empty<IFixedTickable>(),
                new ILateTickable[]
                {
                    container.Resolve<CameraFollowSystem>(),
                    container.Resolve<PlayerDamageVignettePresenter>(),
                    container.Resolve<XpGainCounterSystem>(),
                    container.Resolve<TicketGainCounterSystem>(),
                    container.Resolve<DamageNumberSystem>()
                },
                pauseState);
        }
    }
}
