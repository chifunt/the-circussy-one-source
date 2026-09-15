using Cysharp.Threading.Tasks;
using VContainer.Unity;

namespace TheCircussyOne.Runtime
{
    public interface IWorldRewardKindSpawner : IRunWorldContentSystem
    {
        string DiagnosticsName { get; }
    }

    public sealed class WorldRewardSpawnSystem : IStartable, IRunWorldContentSystem, IRunWorldSpawnAdapter
    {
        private readonly IWorldRewardKindSpawner[] spawners;
        private readonly IRunLoadingStageSink loadingStageSink;

        public WorldRewardSpawnSystem(
            ChestSystem chestSystem = null,
            TicketDepositSystem ticketDepositSystem = null,
            HealingPropSystem healingPropSystem = null,
            IRunLoadingStageSink loadingStageSink = null)
        {
            this.loadingStageSink = loadingStageSink;
            spawners = new IWorldRewardKindSpawner[]
            {
                chestSystem,
                ticketDepositSystem,
                healingPropSystem
            };
        }

        public string DiagnosticsName => "SpawnRewards";
        public string LoadingStage => "Placing Prizes";

        public void Start()
        {
            SpawnWorld(1);
        }

        public void SpawnWorld(int worldIndex)
        {
            for (int i = 0; i < spawners.Length; i++)
            {
                IWorldRewardKindSpawner spawner = spawners[i];
                if (spawner == null)
                {
                    continue;
                }

                SetLoadingStage(spawner);
                spawner.SpawnWorld(worldIndex);
            }
        }

        public async UniTask SpawnWorldAsync(int worldIndex, WorldLoadTimingDiagnostics timing)
        {
            timing ??= WorldLoadTimingDiagnostics.Disabled;
            for (int i = 0; i < spawners.Length; i++)
            {
                IWorldRewardKindSpawner spawner = spawners[i];
                if (spawner == null)
                {
                    continue;
                }

                SetLoadingStage(spawner);
                using (timing.Stage($"WorldRewards.{spawner.DiagnosticsName}"))
                {
                    spawner.SpawnWorld(worldIndex);
                }

                await UniTask.Yield(PlayerLoopTiming.Update);
            }
        }

        private void SetLoadingStage(IWorldRewardKindSpawner spawner)
        {
            loadingStageSink?.SetLoadingStage(StageTextFor(spawner.DiagnosticsName));
        }

        private static string StageTextFor(string diagnosticsName)
        {
            return diagnosticsName switch
            {
                "Chests" => "Placing Prize Trunks",
                "TicketDeposits" => "Scattering Tickets",
                "HealingProps" => "Stocking Snack Carts",
                _ => "Placing Prizes"
            };
        }
    }
}
