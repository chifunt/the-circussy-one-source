using Cysharp.Threading.Tasks;
using TheCircussyOne.Config;
using VContainer;

namespace TheCircussyOne.Runtime
{
    public sealed class RunWorldLifecycleSystem
    {
        private readonly IRunWorldGenerator worldGenerator;
        private readonly RunWorldGenerationState generationState;
        private readonly RunWorldGenerationConfig worldConfig;
        private readonly IRunLoadingStageSink loadingStageSink;
        private readonly RunWorldClearer clearer;
        private readonly RunWorldLifecyclePlan plan;

        private int transitionSequence;

        [Inject]
        public RunWorldLifecycleSystem(
            ActorRegistry registry,
            EnemyFactory enemyFactory,
            ProjectileFactory projectileFactory,
            PickupFactory pickupFactory,
            EnemySpawnIndicatorSystem spawnIndicatorSystem,
            WorldInteractionPromptSystem promptSystem,
            FinishPortalSystem finishPortalSystem,
            WorldPropPlacementService placementService,
            WorldRewardRuntimeRegistry rewardRegistry,
            IRunWorldGenerator worldGenerator,
            WorldRewardSpawnSystem worldRewardSpawnSystem,
            RunWorldGenerationState generationState = null,
            ArenaBarrierSystem arenaBarrierSystem = null,
            BigTopEnvironmentSystem bigTopEnvironmentSystem = null,
            RunWorldGenerationConfig worldConfig = null,
            RunStartupPrewarmSystem prewarmSystem = null,
            IRunLoadingStageSink loadingStageSink = null,
            WorldDressingSystem worldDressingSystem = null,
            WorldDecorationSystem worldDecorationSystem = null,
            RunWorldClearer clearer = null)
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
                worldGenerator,
                worldRewardSpawnSystem,
                generationState,
                arenaBarrierSystem,
                bigTopEnvironmentSystem,
                worldConfig,
                prewarmSystem,
                loadingStageSink,
                worldDressingSystem,
                worldDecorationSystem,
                clearer,
                null,
                null,
                null)
        {
        }

        internal static RunWorldLifecycleSystem CreateForTests(
            IRunWorldGenerator worldGenerator,
            IRunWorldRefreshAdapter[] refreshAdapters,
            IRunWorldSpawnAdapter[] spawnAdapters,
            IRunWorldPrewarmAdapter[] prewarmAdapters,
            IRunLoadingStageSink loadingStageSink = null,
            RunWorldGenerationState generationState = null,
            RunWorldGenerationConfig worldConfig = null)
        {
            return new RunWorldLifecycleSystem(
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                worldGenerator,
                null,
                generationState,
                null,
                null,
                worldConfig,
                null,
                loadingStageSink,
                null,
                null,
                null,
                refreshAdapters,
                spawnAdapters,
                prewarmAdapters);
        }

        private RunWorldLifecycleSystem(
            ActorRegistry registry,
            EnemyFactory enemyFactory,
            ProjectileFactory projectileFactory,
            PickupFactory pickupFactory,
            EnemySpawnIndicatorSystem spawnIndicatorSystem,
            WorldInteractionPromptSystem promptSystem,
            FinishPortalSystem finishPortalSystem,
            WorldPropPlacementService placementService,
            WorldRewardRuntimeRegistry rewardRegistry,
            IRunWorldGenerator worldGenerator,
            WorldRewardSpawnSystem worldRewardSpawnSystem,
            RunWorldGenerationState generationState = null,
            ArenaBarrierSystem arenaBarrierSystem = null,
            BigTopEnvironmentSystem bigTopEnvironmentSystem = null,
            RunWorldGenerationConfig worldConfig = null,
            RunStartupPrewarmSystem prewarmSystem = null,
            IRunLoadingStageSink loadingStageSink = null,
            WorldDressingSystem worldDressingSystem = null,
            WorldDecorationSystem worldDecorationSystem = null,
            RunWorldClearer clearer = null,
            IRunWorldRefreshAdapter[] refreshAdapters = null,
            IRunWorldSpawnAdapter[] spawnAdapters = null,
            IRunWorldPrewarmAdapter[] prewarmAdapters = null)
        {
            this.worldGenerator = worldGenerator ?? new PrototypeRunWorldGenerator();
            this.generationState = generationState;
            this.worldConfig = worldConfig;
            this.loadingStageSink = loadingStageSink;
            plan = refreshAdapters != null || spawnAdapters != null || prewarmAdapters != null
                ? RunWorldLifecyclePlan.CreateForAdapters(refreshAdapters, spawnAdapters, prewarmAdapters)
                : RunWorldLifecyclePlan.CreateDefault(
                    arenaBarrierSystem,
                    bigTopEnvironmentSystem,
                    worldDressingSystem,
                    worldDecorationSystem,
                    worldRewardSpawnSystem,
                    prewarmSystem);
            this.clearer = clearer ?? new RunWorldClearer(
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
                plan.ClearAdapters);
        }

        public WorldGenerationResult LastGenerationResult { get; private set; }

        public WorldGenerationResult TransitionToWorld(int worldIndex, int seed)
        {
            var request = new WorldGenerationRequest(worldIndex, worldIndex, seed);
            WorldLoadTimingDiagnostics timing = WorldLoadTimingDiagnostics.ForWorld(
                worldConfig,
                ++transitionSequence,
                request);

            // Runtime registries must be empty before generation so stale interactables cannot survive between acts.
            SetLoadingStage("Clearing the Ring");
            using (timing.Stage("Lifecycle.ClearCurrentWorld"))
            {
                ClearCurrentWorld();
            }

            SetLoadingStage("Laying the Grounds");
            using (timing.Stage("Lifecycle.GenerateWorld"))
            {
                LastGenerationResult = worldGenerator.Generate(request);
            }

            PublishGenerationState(timing);
            RefreshWorld(timing);
            SpawnWorld(worldIndex, timing);
            PrewarmImmediate(timing);
            SetLoadingStage("Opening the Curtain");
            return LastGenerationResult;
        }

        public async UniTask<WorldGenerationResult> TransitionToWorldAsync(int worldIndex, int seed)
        {
            var request = new WorldGenerationRequest(worldIndex, worldIndex, seed);
            WorldLoadTimingDiagnostics timing = WorldLoadTimingDiagnostics.ForWorld(
                worldConfig,
                ++transitionSequence,
                request);

            // Runtime registries must be empty before generation so stale interactables cannot survive between acts.
            SetLoadingStage("Clearing the Ring");
            await ClearCurrentWorldAsync(timing);

            await UniTask.Yield(PlayerLoopTiming.Update);

            SetLoadingStage("Laying the Grounds");
            using (timing.Stage("Lifecycle.GenerateWorld"))
            {
                LastGenerationResult = worldGenerator is IAsyncRunWorldGenerator asyncGenerator
                    ? await asyncGenerator.GenerateAsync(request, timing)
                    : worldGenerator.Generate(request);
            }

            await UniTask.Yield(PlayerLoopTiming.Update);

            PublishGenerationState(timing);

            await UniTask.Yield(PlayerLoopTiming.Update);

            await RefreshWorldAsync(timing);
            await SpawnWorldAsync(worldIndex, timing);
            await PrewarmAsync(timing);
            SetLoadingStage("Opening the Curtain");
            return LastGenerationResult;
        }

        public void ClearCurrentWorld()
        {
            clearer.ClearCurrentWorld();
        }

        public UniTask ClearCurrentWorldAsync(WorldLoadTimingDiagnostics timing = null)
        {
            return clearer.ClearCurrentWorldAsync(timing);
        }

        private void PublishGenerationState(WorldLoadTimingDiagnostics timing)
        {
            using (timing.Stage("Lifecycle.PublishGenerationState"))
            {
                generationState?.Set(LastGenerationResult);
            }
        }

        private void RefreshWorld(WorldLoadTimingDiagnostics timing)
        {
            for (int i = 0; i < plan.RefreshSteps.Length; i++)
            {
                RunWorldRefreshStep step = plan.RefreshSteps[i];
                SetLoadingStage(step.LoadingStage);
                using (timing.Stage($"Lifecycle.{step.DiagnosticsName}"))
                {
                    step.Adapter?.RefreshForCurrentWorld();
                }
            }
        }

        private async UniTask RefreshWorldAsync(WorldLoadTimingDiagnostics timing)
        {
            for (int i = 0; i < plan.RefreshSteps.Length; i++)
            {
                RunWorldRefreshStep step = plan.RefreshSteps[i];
                SetLoadingStage(step.LoadingStage);
                using (timing.Stage($"Lifecycle.{step.DiagnosticsName}"))
                {
                    if (step.Adapter != null)
                    {
                        await step.Adapter.RefreshForCurrentWorldAsync(timing);
                    }
                }

                await UniTask.Yield(PlayerLoopTiming.Update);
            }
        }

        private void SpawnWorld(int worldIndex, WorldLoadTimingDiagnostics timing)
        {
            for (int i = 0; i < plan.SpawnSteps.Length; i++)
            {
                RunWorldSpawnStep step = plan.SpawnSteps[i];
                SetLoadingStage(step.LoadingStage);
                using (timing.Stage($"Lifecycle.{step.DiagnosticsName}"))
                {
                    step.Adapter?.SpawnWorld(worldIndex);
                }
            }
        }

        private async UniTask SpawnWorldAsync(int worldIndex, WorldLoadTimingDiagnostics timing)
        {
            for (int i = 0; i < plan.SpawnSteps.Length; i++)
            {
                RunWorldSpawnStep step = plan.SpawnSteps[i];
                SetLoadingStage(step.LoadingStage);
                using (timing.Stage($"Lifecycle.{step.DiagnosticsName}"))
                {
                    if (step.Adapter != null)
                    {
                        await step.Adapter.SpawnWorldAsync(worldIndex, timing);
                    }
                }
            }
        }

        private void PrewarmImmediate(WorldLoadTimingDiagnostics timing)
        {
            for (int i = 0; i < plan.PrewarmSteps.Length; i++)
            {
                RunWorldPrewarmStep step = plan.PrewarmSteps[i];
                SetLoadingStage(step.LoadingStage);
                step.Adapter?.PrewarmImmediate(timing);
            }
        }

        private async UniTask PrewarmAsync(WorldLoadTimingDiagnostics timing)
        {
            for (int i = 0; i < plan.PrewarmSteps.Length; i++)
            {
                RunWorldPrewarmStep step = plan.PrewarmSteps[i];
                SetLoadingStage(step.LoadingStage);
                if (step.Adapter != null)
                {
                    await step.Adapter.PrewarmAsync(timing);
                }
            }
        }

        private void SetLoadingStage(string stage)
        {
            loadingStageSink?.SetLoadingStage(stage);
        }
    }
}
