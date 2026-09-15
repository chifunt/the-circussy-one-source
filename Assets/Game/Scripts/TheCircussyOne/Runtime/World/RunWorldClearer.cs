using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TheCircussyOne.Visuals;

namespace TheCircussyOne.Runtime
{
    public sealed class RunWorldClearer
    {
        private const int AsyncClearObjectsPerFrame = 48;

        private readonly ActorRegistry registry;
        private readonly EnemyFactory enemyFactory;
        private readonly ProjectileFactory projectileFactory;
        private readonly PickupFactory pickupFactory;
        private readonly EnemySpawnIndicatorSystem spawnIndicatorSystem;
        private readonly WorldInteractionPromptSystem promptSystem;
        private readonly FinishPortalSystem finishPortalSystem;
        private readonly WorldPropPlacementService placementService;
        private readonly WorldRewardRuntimeRegistry rewardRegistry;
        private readonly RunWorldGenerationState generationState;
        private readonly IRunWorldRefreshAdapter[] refreshAdapters;

        private readonly List<EnemyRuntime> enemyBuffer = new();
        private readonly List<ProjectileRuntime> projectileBuffer = new();
        private readonly List<PickupRuntime> pickupBuffer = new();

        public RunWorldClearer(
            ActorRegistry registry,
            EnemyFactory enemyFactory,
            ProjectileFactory projectileFactory,
            PickupFactory pickupFactory,
            EnemySpawnIndicatorSystem spawnIndicatorSystem,
            WorldInteractionPromptSystem promptSystem,
            FinishPortalSystem finishPortalSystem,
            WorldPropPlacementService placementService,
            WorldRewardRuntimeRegistry rewardRegistry,
            RunWorldGenerationState generationState,
            ArenaBarrierSystem arenaBarrierSystem = null,
            WorldDressingSystem worldDressingSystem = null,
            WorldDecorationSystem worldDecorationSystem = null)
            : this(
                registry,
                enemyFactory,
                projectileFactory,
                pickupFactory,
                spawnIndicatorSystem,
                promptSystem,
                finishPortalSystem,
                placementService,
                rewardRegistry,
                generationState,
                new IRunWorldRefreshAdapter[]
                {
                    arenaBarrierSystem,
                    worldDressingSystem,
                    worldDecorationSystem
                })
        {
        }

        internal RunWorldClearer(
            ActorRegistry registry,
            EnemyFactory enemyFactory,
            ProjectileFactory projectileFactory,
            PickupFactory pickupFactory,
            EnemySpawnIndicatorSystem spawnIndicatorSystem,
            WorldInteractionPromptSystem promptSystem,
            FinishPortalSystem finishPortalSystem,
            WorldPropPlacementService placementService,
            WorldRewardRuntimeRegistry rewardRegistry,
            RunWorldGenerationState generationState,
            IRunWorldRefreshAdapter[] refreshAdapters)
        {
            this.registry = registry;
            this.enemyFactory = enemyFactory;
            this.projectileFactory = projectileFactory;
            this.pickupFactory = pickupFactory;
            this.spawnIndicatorSystem = spawnIndicatorSystem;
            this.promptSystem = promptSystem;
            this.finishPortalSystem = finishPortalSystem;
            this.placementService = placementService;
            this.rewardRegistry = rewardRegistry;
            this.generationState = generationState;
            this.refreshAdapters = refreshAdapters ?? new IRunWorldRefreshAdapter[0];
        }

        public void ClearCurrentWorld()
        {
            spawnIndicatorSystem?.Clear();
            ClearProjectiles();
            ClearPickups();
            ClearEnemies();
            ClearWorldObjects();
        }

        public async UniTask ClearCurrentWorldAsync(WorldLoadTimingDiagnostics timing = null)
        {
            timing ??= WorldLoadTimingDiagnostics.Disabled;
            using (timing.Stage("Lifecycle.ClearCurrentWorld.SpawnIndicators"))
            {
                spawnIndicatorSystem?.Clear();
            }

            await UniTask.Yield(PlayerLoopTiming.Update);

            using (timing.Stage("Lifecycle.ClearCurrentWorld.Projectiles"))
            {
                await ClearProjectilesAsync();
            }

            using (timing.Stage("Lifecycle.ClearCurrentWorld.Pickups"))
            {
                await ClearPickupsAsync();
            }

            using (timing.Stage("Lifecycle.ClearCurrentWorld.Enemies"))
            {
                await ClearEnemiesAsync();
            }

            using (timing.Stage("Lifecycle.ClearCurrentWorld.WorldObjects"))
            {
                await ClearWorldObjectsAsync();
            }

            await UniTask.Yield(PlayerLoopTiming.Update);
        }

        private void ClearWorldObjects()
        {
            finishPortalSystem?.ClearWorld();
            promptSystem?.Clear();
            ClearRefreshAdapters();
            rewardRegistry?.ClearAll();
            placementService?.Clear();
            generationState?.Clear();
            SceneInteractableRegistry.Clear();
            InteractableSelectionOutlineRegistry.Clear();
        }

        private async UniTask ClearWorldObjectsAsync()
        {
            finishPortalSystem?.ClearWorld();
            promptSystem?.Clear();
            ClearRefreshAdapters();
            if (rewardRegistry != null)
            {
                await rewardRegistry.ClearAllAsync(AsyncClearObjectsPerFrame);
            }

            placementService?.Clear();
            generationState?.Clear();
            SceneInteractableRegistry.Clear();
            InteractableSelectionOutlineRegistry.Clear();
        }

        private void ClearRefreshAdapters()
        {
            for (int i = 0; i < refreshAdapters.Length; i++)
            {
                refreshAdapters[i]?.Clear();
            }
        }

        private void ClearEnemies()
        {
            if (registry == null || enemyFactory == null)
            {
                return;
            }

            enemyBuffer.Clear();
            enemyBuffer.AddRange(registry.Enemies);
            for (int i = enemyBuffer.Count - 1; i >= 0; i--)
            {
                enemyFactory.Despawn(enemyBuffer[i], playDeath: false);
            }

            enemyBuffer.Clear();
        }

        private async UniTask ClearEnemiesAsync()
        {
            if (registry == null || enemyFactory == null)
            {
                return;
            }

            enemyBuffer.Clear();
            enemyBuffer.AddRange(registry.Enemies);
            int cleared = 0;
            for (int i = enemyBuffer.Count - 1; i >= 0; i--)
            {
                enemyFactory.Despawn(enemyBuffer[i], playDeath: false);
                cleared++;
                if (cleared % AsyncClearObjectsPerFrame == 0)
                {
                    await UniTask.Yield(PlayerLoopTiming.Update);
                }
            }

            enemyBuffer.Clear();
        }

        private void ClearProjectiles()
        {
            if (registry == null || projectileFactory == null)
            {
                return;
            }

            projectileBuffer.Clear();
            projectileBuffer.AddRange(registry.Projectiles);
            for (int i = projectileBuffer.Count - 1; i >= 0; i--)
            {
                projectileFactory.Despawn(projectileBuffer[i]);
            }

            projectileBuffer.Clear();
        }

        private async UniTask ClearProjectilesAsync()
        {
            if (registry == null || projectileFactory == null)
            {
                return;
            }

            projectileBuffer.Clear();
            projectileBuffer.AddRange(registry.Projectiles);
            int cleared = 0;
            for (int i = projectileBuffer.Count - 1; i >= 0; i--)
            {
                projectileFactory.Despawn(projectileBuffer[i]);
                cleared++;
                if (cleared % AsyncClearObjectsPerFrame == 0)
                {
                    await UniTask.Yield(PlayerLoopTiming.Update);
                }
            }

            projectileBuffer.Clear();
        }

        private void ClearPickups()
        {
            if (registry == null || pickupFactory == null)
            {
                return;
            }

            pickupBuffer.Clear();
            pickupBuffer.AddRange(registry.Pickups);
            for (int i = pickupBuffer.Count - 1; i >= 0; i--)
            {
                pickupFactory.Despawn(pickupBuffer[i]);
            }

            pickupBuffer.Clear();
        }

        private async UniTask ClearPickupsAsync()
        {
            if (registry == null || pickupFactory == null)
            {
                return;
            }

            pickupBuffer.Clear();
            pickupBuffer.AddRange(registry.Pickups);
            int cleared = 0;
            for (int i = pickupBuffer.Count - 1; i >= 0; i--)
            {
                pickupFactory.Despawn(pickupBuffer[i]);
                cleared++;
                if (cleared % AsyncClearObjectsPerFrame == 0)
                {
                    await UniTask.Yield(PlayerLoopTiming.Update);
                }
            }

            pickupBuffer.Clear();
        }
    }
}
