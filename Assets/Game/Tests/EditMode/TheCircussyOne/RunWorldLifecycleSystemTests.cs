using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using TheCircussyOne.Runtime;

public sealed class RunWorldLifecycleSystemTests
{
    [Test]
    public void TransitionToWorldReportsPlayerFacingLoadingStages()
    {
        var sink = new RecordingLoadingStageSink();
        var lifecycle = new RunWorldLifecycleSystem(
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            new CapturingRunWorldGenerator(),
            null,
            loadingStageSink: sink);

        lifecycle.TransitionToWorld(1, 123);

        CollectionAssert.Contains(sink.Stages, "Clearing the Ring");
        CollectionAssert.Contains(sink.Stages, "Laying the Grounds");
        CollectionAssert.Contains(sink.Stages, "Setting the Boundary");
        CollectionAssert.Contains(sink.Stages, "Raising the Big Top");
        CollectionAssert.Contains(sink.Stages, "Placing Prizes");
        CollectionAssert.Contains(sink.Stages, "Warming Up Effects");
        CollectionAssert.Contains(sink.Stages, "Opening the Curtain");
        CollectionAssert.DoesNotContain(sink.Stages, "Lifecycle.GenerateWorld");
    }

    [Test]
    public void TransitionToWorldRunsLifecycleAdaptersInOrder()
    {
        var events = new List<string>();
        var lifecycle = RunWorldLifecycleSystem.CreateForTests(
            new RecordingRunWorldGenerator(events),
            refreshAdapters: new IRunWorldRefreshAdapter[]
            {
                new RecordingRefreshAdapter("ArenaBarrier", "Setting the Boundary", events),
                new RecordingRefreshAdapter("BigTop", "Raising the Big Top", events)
            },
            spawnAdapters: new IRunWorldSpawnAdapter[]
            {
                new RecordingSpawnAdapter("SpawnRewards", "Placing Prizes", events)
            },
            prewarmAdapters: new IRunWorldPrewarmAdapter[]
            {
                new RecordingPrewarmAdapter("Warming Up Effects", events)
            });

        lifecycle.TransitionToWorld(1, 123);

        CollectionAssert.AreEqual(
            new[]
            {
                "clear:ArenaBarrier",
                "clear:BigTop",
                "generate",
                "refresh:ArenaBarrier",
                "refresh:BigTop",
                "spawn:SpawnRewards",
                "prewarm"
            },
            events);
    }

    [Test]
    public void DefaultLifecyclePlanKeepsOrderedStagesAndFocusedClearAdapters()
    {
        RunWorldLifecyclePlan plan = RunWorldLifecyclePlan.CreateDefault(
            null,
            null,
            null,
            null,
            null,
            null);

        CollectionAssert.AreEqual(
            new[] { "ArenaBarrier", "BigTop", "WorldDressing", "WorldDecoration" },
            plan.RefreshSteps.Select(step => step.DiagnosticsName));
        CollectionAssert.AreEqual(
            new[] { "Setting the Boundary", "Raising the Big Top", "Dressing the Ring", "Scattering Props" },
            plan.RefreshSteps.Select(step => step.LoadingStage));
        CollectionAssert.AreEqual(
            new[] { "SpawnRewards" },
            plan.SpawnSteps.Select(step => step.DiagnosticsName));
        CollectionAssert.AreEqual(
            new[] { "Placing Prizes" },
            plan.SpawnSteps.Select(step => step.LoadingStage));
        CollectionAssert.AreEqual(
            new[] { "Warming Up Effects" },
            plan.PrewarmSteps.Select(step => step.LoadingStage));
        Assert.That(plan.ClearAdapters, Has.Length.EqualTo(3));
    }

    [Test]
    public void RuntimeConstructorIsExplicitlySelectedForVContainer()
    {
        ConstructorInfo[] constructors = typeof(RunWorldLifecycleSystem).GetConstructors(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        ConstructorInfo[] injectedConstructors = constructors
            .Where(constructor => constructor.GetCustomAttributes()
                .Any(attribute => attribute.GetType().FullName == "VContainer.InjectAttribute"))
            .ToArray();

        Assert.That(injectedConstructors, Has.Length.EqualTo(1));
        Assert.That(injectedConstructors[0].IsPublic, Is.True);
        CollectionAssert.DoesNotContain(
            injectedConstructors[0].GetParameters().Select(parameter => parameter.ParameterType).ToArray(),
            typeof(IRunWorldRefreshAdapter[]));
    }

    private sealed class RecordingLoadingStageSink : IRunLoadingStageSink
    {
        public List<string> Stages { get; } = new();

        public void SetLoadingStage(string stage)
        {
            Stages.Add(stage);
        }
    }

    private sealed class CapturingRunWorldGenerator : IRunWorldGenerator
    {
        public WorldGenerationResult Generate(WorldGenerationRequest request)
        {
            return new WorldGenerationResult(request.WorldIndex, request.ActNumber, request.Seed, usedPrototypePlacement: false);
        }
    }

    private sealed class RecordingRunWorldGenerator : IRunWorldGenerator
    {
        private readonly List<string> events;

        public RecordingRunWorldGenerator(List<string> events)
        {
            this.events = events;
        }

        public WorldGenerationResult Generate(WorldGenerationRequest request)
        {
            events.Add("generate");
            return new WorldGenerationResult(request.WorldIndex, request.ActNumber, request.Seed, usedPrototypePlacement: false);
        }
    }

    private sealed class RecordingRefreshAdapter : IRunWorldRefreshAdapter
    {
        private readonly List<string> events;

        public RecordingRefreshAdapter(string diagnosticsName, string loadingStage, List<string> events)
        {
            DiagnosticsName = diagnosticsName;
            LoadingStage = loadingStage;
            this.events = events;
        }

        public string DiagnosticsName { get; }
        public string LoadingStage { get; }

        public void Clear()
        {
            events.Add($"clear:{DiagnosticsName}");
        }

        public void RefreshForCurrentWorld()
        {
            events.Add($"refresh:{DiagnosticsName}");
        }

        public UniTask RefreshForCurrentWorldAsync(WorldLoadTimingDiagnostics timing = null)
        {
            events.Add($"refreshAsync:{DiagnosticsName}");
            return UniTask.CompletedTask;
        }
    }

    private sealed class RecordingSpawnAdapter : IRunWorldSpawnAdapter
    {
        private readonly List<string> events;

        public RecordingSpawnAdapter(string diagnosticsName, string loadingStage, List<string> events)
        {
            DiagnosticsName = diagnosticsName;
            LoadingStage = loadingStage;
            this.events = events;
        }

        public string DiagnosticsName { get; }
        public string LoadingStage { get; }

        public void SpawnWorld(int worldIndex)
        {
            events.Add($"spawn:{DiagnosticsName}");
        }

        public UniTask SpawnWorldAsync(int worldIndex, WorldLoadTimingDiagnostics timing = null)
        {
            events.Add($"spawnAsync:{DiagnosticsName}");
            return UniTask.CompletedTask;
        }
    }

    private sealed class RecordingPrewarmAdapter : IRunWorldPrewarmAdapter
    {
        private readonly List<string> events;

        public RecordingPrewarmAdapter(string loadingStage, List<string> events)
        {
            LoadingStage = loadingStage;
            this.events = events;
        }

        public string LoadingStage { get; }

        public void PrewarmImmediate(WorldLoadTimingDiagnostics timing = null)
        {
            events.Add("prewarm");
        }

        public UniTask PrewarmAsync(WorldLoadTimingDiagnostics timing = null)
        {
            events.Add("prewarmAsync");
            return UniTask.CompletedTask;
        }
    }
}
