using Cysharp.Threading.Tasks;
using TheCircussyOne.Config;
using UnityEngine;

namespace TheCircussyOne.Runtime
{
    public sealed class RunStartupPrewarmSystem : IRunWorldPrewarmAdapter
    {
        private delegate int PrewarmChunk(int targetPoolCount, int maxCreate);

        private readonly GameConfig gameConfig;
        private readonly VfxVisualConfig vfxConfig;
        private readonly EnemyFactory enemyFactory;
        private readonly ProjectileFactory projectileFactory;
        private readonly PickupFactory pickupFactory;
        private readonly DamageNumberFactory damageNumberFactory;
        private readonly VfxEffectFactory vfxEffectFactory;
        private readonly ProjectileExplosionSystem projectileExplosionSystem;
        private readonly ProjectileChainVisualSystem projectileChainVisualSystem;
        private readonly OrbitWeaponSystem orbitWeaponSystem;
        private readonly WorldInteractionPromptFactory worldInteractionPromptFactory;
        private readonly WorldRewardTransientVfxSystem rewardTransientVfxSystem;
        private readonly XpGainCounterSystem xpGainCounterSystem;
        private readonly TicketGainCounterSystem ticketGainCounterSystem;

        public RunStartupPrewarmSystem(
            GameConfig gameConfig,
            VfxVisualConfig vfxConfig = null,
            EnemyFactory enemyFactory = null,
            ProjectileFactory projectileFactory = null,
            PickupFactory pickupFactory = null,
            DamageNumberFactory damageNumberFactory = null,
            VfxEffectFactory vfxEffectFactory = null,
            ProjectileExplosionSystem projectileExplosionSystem = null,
            ProjectileChainVisualSystem projectileChainVisualSystem = null,
            OrbitWeaponSystem orbitWeaponSystem = null,
            WorldInteractionPromptFactory worldInteractionPromptFactory = null,
            WorldRewardTransientVfxSystem rewardTransientVfxSystem = null,
            XpGainCounterSystem xpGainCounterSystem = null,
            TicketGainCounterSystem ticketGainCounterSystem = null)
        {
            this.gameConfig = gameConfig;
            this.vfxConfig = vfxConfig;
            this.enemyFactory = enemyFactory;
            this.projectileFactory = projectileFactory;
            this.pickupFactory = pickupFactory;
            this.damageNumberFactory = damageNumberFactory;
            this.vfxEffectFactory = vfxEffectFactory;
            this.projectileExplosionSystem = projectileExplosionSystem;
            this.projectileChainVisualSystem = projectileChainVisualSystem;
            this.orbitWeaponSystem = orbitWeaponSystem;
            this.worldInteractionPromptFactory = worldInteractionPromptFactory;
            this.rewardTransientVfxSystem = rewardTransientVfxSystem;
            this.xpGainCounterSystem = xpGainCounterSystem;
            this.ticketGainCounterSystem = ticketGainCounterSystem;
        }

        private bool IsEnabled => gameConfig != null && gameConfig.runtimePrewarmEnabled;
        private int ObjectsPerFrame => Mathf.Max(1, gameConfig != null ? gameConfig.runtimePrewarmObjectsPerFrame : 8);
        private int OneShotVfxTarget => vfxConfig != null ? Mathf.Max(0, vfxConfig.oneShotPrewarmCount) : 0;
        public string LoadingStage => "Warming Up Effects";

        public void PrewarmImmediate(WorldLoadTimingDiagnostics timing = null)
        {
            if (!IsEnabled)
            {
                return;
            }

            timing ??= WorldLoadTimingDiagnostics.Disabled;
            PrewarmImmediateStage("Lifecycle.Prewarm.Enemies", gameConfig.runtimePrewarmEnemyViews, EnemyPrewarm, timing);
            PrewarmImmediateStage("Lifecycle.Prewarm.Projectiles", gameConfig.runtimePrewarmProjectileViews, ProjectilePrewarm, timing);
            PrewarmImmediateStage("Lifecycle.Prewarm.Pickups", gameConfig.runtimePrewarmPickupViews, PickupPrewarm, timing);
            PrewarmImmediateStage("Lifecycle.Prewarm.DamageNumbers", gameConfig.runtimePrewarmDamageNumbers, DamageNumberPrewarm, timing);
            PrewarmImmediateStage("Lifecycle.Prewarm.OneShotVfx", OneShotVfxTarget, OneShotVfxPrewarm, timing);
            PrewarmImmediateStage("Lifecycle.Prewarm.ProjectileExplosions", gameConfig.runtimePrewarmProjectileExplosions, ProjectileExplosionPrewarm, timing);
            PrewarmImmediateStage("Lifecycle.Prewarm.ProjectileChains", gameConfig.runtimePrewarmProjectileChainSegments, ProjectileChainPrewarm, timing);
            PrewarmImmediateStage("Lifecycle.Prewarm.OrbitWeapons", gameConfig.runtimePrewarmOrbitWeaponViews, OrbitWeaponPrewarm, timing);
            PrewarmImmediateStage("Lifecycle.Prewarm.FireHoopTelegraphs", gameConfig.runtimePrewarmFireHoopAreaTelegraphs, FireHoopTelegraphPrewarm, timing);
            PrewarmImmediateStage("Lifecycle.Prewarm.TicketBursts", gameConfig.runtimePrewarmTicketBurstVfx, TicketBurstPrewarm, timing);
            PrewarmImmediateStage("Lifecycle.Prewarm.SnackBursts", gameConfig.runtimePrewarmSnackBurstVfx, SnackBurstPrewarm, timing);
            PrewarmGainCounters(timing);
            PrewarmPrompt(timing);
        }

        public async UniTask PrewarmAsync(WorldLoadTimingDiagnostics timing = null)
        {
            if (!IsEnabled)
            {
                return;
            }

            timing ??= WorldLoadTimingDiagnostics.Disabled;
            await PrewarmStageAsync("Lifecycle.Prewarm.Enemies", gameConfig.runtimePrewarmEnemyViews, EnemyPrewarm, timing);
            await PrewarmStageAsync("Lifecycle.Prewarm.Projectiles", gameConfig.runtimePrewarmProjectileViews, ProjectilePrewarm, timing);
            await PrewarmStageAsync("Lifecycle.Prewarm.Pickups", gameConfig.runtimePrewarmPickupViews, PickupPrewarm, timing);
            await PrewarmStageAsync("Lifecycle.Prewarm.DamageNumbers", gameConfig.runtimePrewarmDamageNumbers, DamageNumberPrewarm, timing);
            await PrewarmStageAsync("Lifecycle.Prewarm.OneShotVfx", OneShotVfxTarget, OneShotVfxPrewarm, timing);
            await PrewarmStageAsync("Lifecycle.Prewarm.ProjectileExplosions", gameConfig.runtimePrewarmProjectileExplosions, ProjectileExplosionPrewarm, timing);
            await PrewarmStageAsync("Lifecycle.Prewarm.ProjectileChains", gameConfig.runtimePrewarmProjectileChainSegments, ProjectileChainPrewarm, timing);
            await PrewarmStageAsync("Lifecycle.Prewarm.OrbitWeapons", gameConfig.runtimePrewarmOrbitWeaponViews, OrbitWeaponPrewarm, timing);
            await PrewarmStageAsync("Lifecycle.Prewarm.FireHoopTelegraphs", gameConfig.runtimePrewarmFireHoopAreaTelegraphs, FireHoopTelegraphPrewarm, timing);
            await PrewarmStageAsync("Lifecycle.Prewarm.TicketBursts", gameConfig.runtimePrewarmTicketBurstVfx, TicketBurstPrewarm, timing);
            await PrewarmStageAsync("Lifecycle.Prewarm.SnackBursts", gameConfig.runtimePrewarmSnackBurstVfx, SnackBurstPrewarm, timing);
            PrewarmGainCounters(timing);
            PrewarmPrompt(timing);
        }

        private int EnemyPrewarm(int targetPoolCount, int maxCreate) => enemyFactory != null ? enemyFactory.Prewarm(targetPoolCount, maxCreate) : 0;
        private int ProjectilePrewarm(int targetPoolCount, int maxCreate) => projectileFactory != null ? projectileFactory.Prewarm(targetPoolCount, maxCreate) : 0;
        private int PickupPrewarm(int targetPoolCount, int maxCreate) => pickupFactory != null ? pickupFactory.Prewarm(targetPoolCount, maxCreate) : 0;
        private int DamageNumberPrewarm(int targetPoolCount, int maxCreate) => damageNumberFactory != null ? damageNumberFactory.Prewarm(targetPoolCount, maxCreate) : 0;
        private int OneShotVfxPrewarm(int targetPoolCount, int maxCreate) => vfxEffectFactory != null ? vfxEffectFactory.PrewarmPerRegisteredEffect(targetPoolCount, maxCreate) : 0;
        private int ProjectileExplosionPrewarm(int targetPoolCount, int maxCreate) => projectileExplosionSystem != null ? projectileExplosionSystem.Prewarm(targetPoolCount, maxCreate) : 0;
        private int ProjectileChainPrewarm(int targetPoolCount, int maxCreate) => projectileChainVisualSystem != null ? projectileChainVisualSystem.Prewarm(targetPoolCount, maxCreate) : 0;
        private int OrbitWeaponPrewarm(int targetPoolCount, int maxCreate) => orbitWeaponSystem != null ? orbitWeaponSystem.PrewarmViews(targetPoolCount, maxCreate) : 0;
        private int FireHoopTelegraphPrewarm(int targetPoolCount, int maxCreate) => orbitWeaponSystem != null ? orbitWeaponSystem.PrewarmAreaTelegraphs(targetPoolCount, maxCreate) : 0;
        private int TicketBurstPrewarm(int targetPoolCount, int maxCreate) => rewardTransientVfxSystem != null ? rewardTransientVfxSystem.PrewarmTicketBursts(targetPoolCount, maxCreate) : 0;
        private int SnackBurstPrewarm(int targetPoolCount, int maxCreate) => rewardTransientVfxSystem != null ? rewardTransientVfxSystem.PrewarmSnackBursts(targetPoolCount, maxCreate) : 0;

        private void PrewarmImmediateStage(string stage, int targetPoolCount, PrewarmChunk prewarm, WorldLoadTimingDiagnostics timing)
        {
            if (targetPoolCount <= 0 || prewarm == null)
            {
                return;
            }

            using (timing.Stage(stage))
            {
                prewarm(targetPoolCount, int.MaxValue);
            }
        }

        private async UniTask PrewarmStageAsync(string stage, int targetPoolCount, PrewarmChunk prewarm, WorldLoadTimingDiagnostics timing)
        {
            if (targetPoolCount <= 0 || prewarm == null)
            {
                return;
            }

            using (timing.Stage(stage))
            {
                const int GuardLimit = 4096;
                int guard = 0;
                while (guard++ < GuardLimit)
                {
                    int created = prewarm(targetPoolCount, ObjectsPerFrame);
                    if (created <= 0)
                    {
                        break;
                    }

                    await UniTask.Yield(PlayerLoopTiming.Update);
                }
            }
        }

        private void PrewarmPrompt(WorldLoadTimingDiagnostics timing)
        {
            if (!gameConfig.runtimePrewarmWorldPrompt || worldInteractionPromptFactory == null)
            {
                return;
            }

            using (timing.Stage("Lifecycle.Prewarm.WorldPrompt"))
            {
                worldInteractionPromptFactory.Prewarm();
            }
        }

        private void PrewarmGainCounters(WorldLoadTimingDiagnostics timing)
        {
            if (!gameConfig.runtimePrewarmGainCounters)
            {
                return;
            }

            using (timing.Stage("Lifecycle.Prewarm.GainCounters"))
            {
                xpGainCounterSystem?.Prewarm();
                ticketGainCounterSystem?.Prewarm();
            }
        }
    }
}
