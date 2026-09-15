using Cysharp.Threading.Tasks;

namespace TheCircussyOne.Runtime
{
    public interface IRunWorldRefreshAdapter
    {
        string DiagnosticsName { get; }
        string LoadingStage { get; }

        void Clear();
        void RefreshForCurrentWorld();
        UniTask RefreshForCurrentWorldAsync(WorldLoadTimingDiagnostics timing = null);
    }

    public interface IRunWorldSpawnAdapter
    {
        string DiagnosticsName { get; }
        string LoadingStage { get; }

        void SpawnWorld(int worldIndex);
        UniTask SpawnWorldAsync(int worldIndex, WorldLoadTimingDiagnostics timing = null);
    }

    public interface IRunWorldPrewarmAdapter
    {
        string LoadingStage { get; }

        void PrewarmImmediate(WorldLoadTimingDiagnostics timing = null);
        UniTask PrewarmAsync(WorldLoadTimingDiagnostics timing = null);
    }
}
