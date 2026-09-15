using TheCircussyOne.Stats;

namespace TheCircussyOne.Runtime
{
    public sealed class RunResetCoordinator
    {
        private readonly RunPauseState pauseState;
        private readonly RunPhaseState phaseState;
        private readonly PerformerRunState performerRunState;
        private readonly RunStats stats;
        private readonly WeaponLoadout weaponLoadout;
        private readonly ItemInventory itemInventory;
        private readonly RunCurrencyState currencyState;
        private readonly UpgradeRunState upgradeRunState;
        private readonly TalentRunState talentRunState;
        private readonly GameState gameState;
        private readonly RunActScheduleState scheduleState;
        private readonly RunWorldLifecycleSystem worldLifecycle;
        private readonly EnemySpawnSystem enemySpawnSystem;
        private readonly UpgradeSelectionSystem upgradeSelectionSystem;
        private readonly RewardRevealPresenter rewardRevealPresenter;
        private readonly InteractionSystem interactionSystem;
        private readonly WorldInteractionPromptSystem promptSystem;
        private readonly WorldRewardTransientVfxSystem rewardTransientVfxSystem;
        private readonly HeadlinerSystem headlinerSystem;
        private readonly GameAudioService audioService;

        public RunResetCoordinator(
            RunPauseState pauseState,
            RunPhaseState phaseState,
            PerformerRunState performerRunState,
            RunStats stats,
            WeaponLoadout weaponLoadout,
            ItemInventory itemInventory,
            RunCurrencyState currencyState,
            UpgradeRunState upgradeRunState,
            TalentRunState talentRunState,
            GameState gameState,
            RunActScheduleState scheduleState = null,
            RunWorldLifecycleSystem worldLifecycle = null,
            EnemySpawnSystem enemySpawnSystem = null,
            UpgradeSelectionSystem upgradeSelectionSystem = null,
            RewardRevealPresenter rewardRevealPresenter = null,
            InteractionSystem interactionSystem = null,
            WorldInteractionPromptSystem promptSystem = null,
            WorldRewardTransientVfxSystem rewardTransientVfxSystem = null,
            HeadlinerSystem headlinerSystem = null,
            GameAudioService audioService = null)
        {
            this.pauseState = pauseState;
            this.phaseState = phaseState;
            this.performerRunState = performerRunState;
            this.stats = stats;
            this.weaponLoadout = weaponLoadout;
            this.itemInventory = itemInventory;
            this.currencyState = currencyState;
            this.upgradeRunState = upgradeRunState;
            this.talentRunState = talentRunState;
            this.gameState = gameState;
            this.scheduleState = scheduleState;
            this.worldLifecycle = worldLifecycle;
            this.enemySpawnSystem = enemySpawnSystem;
            this.upgradeSelectionSystem = upgradeSelectionSystem;
            this.rewardRevealPresenter = rewardRevealPresenter;
            this.interactionSystem = interactionSystem;
            this.promptSystem = promptSystem;
            this.rewardTransientVfxSystem = rewardTransientVfxSystem;
            this.headlinerSystem = headlinerSystem;
            this.audioService = audioService;
        }

        public void ResetBeforeSceneReload(RunRestartReason reason)
        {
            var context = new RunResetContext(RunResetKind.BeforeSceneReload, reason);
            ResetTransientSystems(context, clearWorld: false);
            upgradeSelectionSystem?.ResetRunState(context);
            rewardRevealPresenter?.ResetRunState(context);
            pauseState?.Clear();
        }

        public void ResetToPerformerSelection(RunRestartReason reason)
        {
            var context = new RunResetContext(RunResetKind.ReturnToPerformerSelection, reason);
            ResetTransientSystems(context, clearWorld: true);
            ResetFullRunState(context);
            phaseState?.ResetToPerformerSelection();
            pauseState?.Clear();
        }

        public void ResetForNewRunStart(bool clearWorld = true)
        {
            var context = new RunResetContext(RunResetKind.StartNewRun);
            ResetTransientSystems(context, clearWorld);
            ResetFullRunState(context);
            phaseState?.ResetToPerformerSelection();
            pauseState?.Clear();
        }

        public void ResetForWorldTransition(int worldIndex, bool clearWorld = true)
        {
            var context = new RunResetContext(RunResetKind.WorldTransition, worldIndex: worldIndex);
            ResetTransientSystems(context, clearWorld);
        }

        private void ResetTransientSystems(RunResetContext context, bool clearWorld)
        {
            interactionSystem?.ResetRunState(context);
            promptSystem?.ResetRunState(context);
            headlinerSystem?.ResetRunState(context);
            rewardTransientVfxSystem?.ResetRunState(context);
            audioService?.ResetRunState(context);
            if (clearWorld)
            {
                worldLifecycle?.ClearCurrentWorld();
            }
        }

        private void ResetFullRunState(RunResetContext context)
        {
            upgradeSelectionSystem?.ResetRunState(context);
            rewardRevealPresenter?.ResetRunState(context);
            enemySpawnSystem?.ResetRunState(context);
            weaponLoadout?.Clear();
            itemInventory?.Clear();
            upgradeRunState?.Clear();
            talentRunState?.Clear();
            stats?.ClearModifiers();
            currencyState?.Reset();
            performerRunState?.Clear();
            scheduleState?.Reset();
            gameState?.Reset();
        }
    }
}
