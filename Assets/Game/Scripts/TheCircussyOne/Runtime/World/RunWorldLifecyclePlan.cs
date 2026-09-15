using System;

namespace TheCircussyOne.Runtime
{
    internal sealed class RunWorldLifecyclePlan
    {
        private RunWorldLifecyclePlan(
            RunWorldRefreshStep[] refreshSteps,
            RunWorldSpawnStep[] spawnSteps,
            RunWorldPrewarmStep[] prewarmSteps,
            IRunWorldRefreshAdapter[] clearAdapters)
        {
            RefreshSteps = refreshSteps ?? Array.Empty<RunWorldRefreshStep>();
            SpawnSteps = spawnSteps ?? Array.Empty<RunWorldSpawnStep>();
            PrewarmSteps = prewarmSteps ?? Array.Empty<RunWorldPrewarmStep>();
            ClearAdapters = clearAdapters ?? ExtractRefreshAdapters(RefreshSteps);
        }

        public RunWorldRefreshStep[] RefreshSteps { get; }
        public RunWorldSpawnStep[] SpawnSteps { get; }
        public RunWorldPrewarmStep[] PrewarmSteps { get; }
        public IRunWorldRefreshAdapter[] ClearAdapters { get; }

        public static RunWorldLifecyclePlan CreateDefault(
            ArenaBarrierSystem arenaBarrierSystem,
            BigTopEnvironmentSystem bigTopEnvironmentSystem,
            WorldDressingSystem worldDressingSystem,
            WorldDecorationSystem worldDecorationSystem,
            WorldRewardSpawnSystem worldRewardSpawnSystem,
            RunStartupPrewarmSystem prewarmSystem)
        {
            return new RunWorldLifecyclePlan(
                new[]
                {
                    new RunWorldRefreshStep("ArenaBarrier", "Setting the Boundary", arenaBarrierSystem),
                    new RunWorldRefreshStep("BigTop", "Raising the Big Top", bigTopEnvironmentSystem),
                    new RunWorldRefreshStep("WorldDressing", "Dressing the Ring", worldDressingSystem),
                    new RunWorldRefreshStep("WorldDecoration", "Scattering Props", worldDecorationSystem)
                },
                new[]
                {
                    new RunWorldSpawnStep("SpawnRewards", "Placing Prizes", worldRewardSpawnSystem)
                },
                new[]
                {
                    new RunWorldPrewarmStep("Warming Up Effects", prewarmSystem)
                },
                new IRunWorldRefreshAdapter[]
                {
                    arenaBarrierSystem,
                    worldDressingSystem,
                    worldDecorationSystem
                });
        }

        public static RunWorldLifecyclePlan CreateForAdapters(
            IRunWorldRefreshAdapter[] refreshAdapters,
            IRunWorldSpawnAdapter[] spawnAdapters,
            IRunWorldPrewarmAdapter[] prewarmAdapters)
        {
            RunWorldRefreshStep[] refreshSteps = BuildRefreshSteps(refreshAdapters ?? Array.Empty<IRunWorldRefreshAdapter>());
            return new RunWorldLifecyclePlan(
                refreshSteps,
                BuildSpawnSteps(spawnAdapters ?? Array.Empty<IRunWorldSpawnAdapter>()),
                BuildPrewarmSteps(prewarmAdapters ?? Array.Empty<IRunWorldPrewarmAdapter>()),
                ExtractRefreshAdapters(refreshSteps));
        }

        private static RunWorldRefreshStep[] BuildRefreshSteps(IRunWorldRefreshAdapter[] adapters)
        {
            var steps = new RunWorldRefreshStep[adapters.Length];
            for (int i = 0; i < adapters.Length; i++)
            {
                IRunWorldRefreshAdapter adapter = adapters[i];
                steps[i] = new RunWorldRefreshStep(
                    adapter?.DiagnosticsName ?? "Refresh",
                    adapter?.LoadingStage ?? "Refreshing the World",
                    adapter);
            }

            return steps;
        }

        private static RunWorldSpawnStep[] BuildSpawnSteps(IRunWorldSpawnAdapter[] adapters)
        {
            var steps = new RunWorldSpawnStep[adapters.Length];
            for (int i = 0; i < adapters.Length; i++)
            {
                IRunWorldSpawnAdapter adapter = adapters[i];
                steps[i] = new RunWorldSpawnStep(
                    adapter?.DiagnosticsName ?? "SpawnWorld",
                    adapter?.LoadingStage ?? "Spawning the World",
                    adapter);
            }

            return steps;
        }

        private static RunWorldPrewarmStep[] BuildPrewarmSteps(IRunWorldPrewarmAdapter[] adapters)
        {
            var steps = new RunWorldPrewarmStep[adapters.Length];
            for (int i = 0; i < adapters.Length; i++)
            {
                IRunWorldPrewarmAdapter adapter = adapters[i];
                steps[i] = new RunWorldPrewarmStep(
                    adapter?.LoadingStage ?? "Warming Up Effects",
                    adapter);
            }

            return steps;
        }

        private static IRunWorldRefreshAdapter[] ExtractRefreshAdapters(RunWorldRefreshStep[] steps)
        {
            var adapters = new IRunWorldRefreshAdapter[steps.Length];
            for (int i = 0; i < steps.Length; i++)
            {
                adapters[i] = steps[i].Adapter;
            }

            return adapters;
        }
    }

    internal readonly struct RunWorldRefreshStep
    {
        public RunWorldRefreshStep(string diagnosticsName, string loadingStage, IRunWorldRefreshAdapter adapter)
        {
            DiagnosticsName = diagnosticsName;
            LoadingStage = loadingStage;
            Adapter = adapter;
        }

        public string DiagnosticsName { get; }
        public string LoadingStage { get; }
        public IRunWorldRefreshAdapter Adapter { get; }
    }

    internal readonly struct RunWorldSpawnStep
    {
        public RunWorldSpawnStep(string diagnosticsName, string loadingStage, IRunWorldSpawnAdapter adapter)
        {
            DiagnosticsName = diagnosticsName;
            LoadingStage = loadingStage;
            Adapter = adapter;
        }

        public string DiagnosticsName { get; }
        public string LoadingStage { get; }
        public IRunWorldSpawnAdapter Adapter { get; }
    }

    internal readonly struct RunWorldPrewarmStep
    {
        public RunWorldPrewarmStep(string loadingStage, IRunWorldPrewarmAdapter adapter)
        {
            LoadingStage = loadingStage;
            Adapter = adapter;
        }

        public string LoadingStage { get; }
        public IRunWorldPrewarmAdapter Adapter { get; }
    }
}
