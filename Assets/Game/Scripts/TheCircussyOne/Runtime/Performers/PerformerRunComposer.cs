using System;
using Cysharp.Threading.Tasks;
using TheCircussyOne.Content;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;
using TheCircussyOne.Stats;
using TheCircussyOne.Visuals;
using UnityEngine;

namespace TheCircussyOne.Runtime
{
    public sealed class PerformerRunComposer
    {
        private readonly PerformerRunState runState;
        private readonly RunStats stats;
        private readonly WeaponLoadout weaponLoadout;
        private readonly GameState gameState;
        private readonly IPerformerPassive staticStatPassive;
        private readonly RunCurrencyState currencyState;
        private readonly PlayerView player;
        private readonly RunSeedState runSeedState;
        private readonly RunPhaseState runPhaseState;
        private readonly RunWorldLifecycleSystem worldLifecycle;
        private readonly RunResetCoordinator resetCoordinator;
        private readonly EnemySpawnSystem enemySpawnSystem;
        private readonly CameraView camera;
        private readonly CameraConfig cameraConfig;
        private readonly IRunTransitionService transitionService;
        private readonly RunPlayerStartPlacementSystem playerStartPlacement;

        public PerformerRunComposer(
            PerformerRunState runState,
            RunStats stats,
            WeaponLoadout weaponLoadout,
            GameState gameState,
            IPerformerPassive staticStatPassive = null,
            RunCurrencyState currencyState = null,
            PlayerView player = null,
            RunSeedState runSeedState = null,
            RunPhaseState runPhaseState = null,
            RunWorldLifecycleSystem worldLifecycle = null,
            RunResetCoordinator resetCoordinator = null,
            EnemySpawnSystem enemySpawnSystem = null,
            CameraView camera = null,
            CameraConfig cameraConfig = null,
            IRunTransitionService transitionService = null,
            RunPlayerStartPlacementSystem playerStartPlacement = null)
        {
            this.runState = runState;
            this.stats = stats;
            this.weaponLoadout = weaponLoadout;
            this.gameState = gameState;
            this.staticStatPassive = staticStatPassive ?? new StaticStatPerformerPassive();
            this.currencyState = currencyState;
            this.player = player;
            this.runSeedState = runSeedState;
            this.runPhaseState = runPhaseState;
            this.worldLifecycle = worldLifecycle;
            this.resetCoordinator = resetCoordinator;
            this.enemySpawnSystem = enemySpawnSystem;
            this.camera = camera;
            this.cameraConfig = cameraConfig;
            this.transitionService = transitionService;
            this.playerStartPlacement = playerStartPlacement ?? new RunPlayerStartPlacementSystem(player, camera, cameraConfig);
        }

        public bool ComposeAndStart(PerformerDefinition performer)
        {
            if (!ContentAvailabilityRules.IsActiveAndValid(performer)
                || !performer.unlocked
                || !ContentAvailabilityRules.IsActiveAndValid(performer.startingWeapon))
            {
                return false;
            }

            if (transitionService != null && transitionService.IsEnabled)
            {
                return transitionService.PlayAsync(
                    "Setting the Stage",
                    "Loading the opening act...",
                    () => ComposeAndStartAsync(performer));
            }

            ComposeAndStartImmediate(performer);
            return true;
        }

        public bool ComposeAndStartSilentlyAsync(PerformerDefinition performer, Action onComplete = null, Action onFailure = null)
        {
            if (!ContentAvailabilityRules.IsActiveAndValid(performer)
                || !performer.unlocked
                || !ContentAvailabilityRules.IsActiveAndValid(performer.startingWeapon))
            {
                return false;
            }

            ComposeAndStartSilentlyAsyncInternal(performer, onComplete, onFailure).Forget();
            return true;
        }

        private void ComposeAndStartImmediate(PerformerDefinition performer)
        {
            ResetBeforeApplyingPerformer();
            runSeedState?.ResetForNewRun();
            weaponLoadout?.SetStartingWeapons(performer.startingWeapon);
            ApplyBaseStats(performer);
            ApplyPassive(performer);
            gameState?.RefreshDerivedStats();
            runState?.Select(performer);
            player?.ApplyPerformerVisuals(performer);
            StartFirstWorld();
            runPhaseState?.BeginWorld();
        }

        private async UniTask ComposeAndStartAsync(PerformerDefinition performer)
        {
            ResetBeforeApplyingPerformer();
            runSeedState?.ResetForNewRun();
            weaponLoadout?.SetStartingWeapons(performer.startingWeapon);
            ApplyBaseStats(performer);
            ApplyPassive(performer);
            gameState?.RefreshDerivedStats();
            runState?.Select(performer);
            player?.ApplyPerformerVisuals(performer);

            WorldGenerationResult result = default;
            if (worldLifecycle != null)
            {
                result = await worldLifecycle.TransitionToWorldAsync(1, WorldSeed(1));
            }

            ApplyPlayerStart(result);
            runPhaseState?.BeginWorld();
        }

        private async UniTaskVoid ComposeAndStartSilentlyAsyncInternal(PerformerDefinition performer, Action onComplete, Action onFailure)
        {
            try
            {
                await ComposeAndStartAsync(performer);
                onComplete?.Invoke();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                onFailure?.Invoke();
            }
        }

        private void ResetBeforeApplyingPerformer()
        {
            if (resetCoordinator != null)
            {
                resetCoordinator.ResetForNewRunStart(clearWorld: worldLifecycle == null);
                return;
            }

            RemoveCurrentPerformerModifiers();
            currencyState?.Reset();
            gameState?.Reset();
            enemySpawnSystem?.ResetRunState();
            runPhaseState?.ResetToPerformerSelection();
        }

        private void RemoveCurrentPerformerModifiers()
        {
            PerformerDefinition current = runState?.SelectedPerformer;
            if (current == null || stats == null)
            {
                return;
            }

            stats.RemoveSource(BaseSourceId(current));
            stats.RemoveSource(PassiveSourceId(current));
        }

        private void ApplyBaseStats(PerformerDefinition performer)
        {
            if (stats == null || performer.baseStatModifiers == null)
            {
                return;
            }

            string sourceId = BaseSourceId(performer);
            stats.RemoveSource(sourceId);
            for (int i = 0; i < performer.baseStatModifiers.Count; i++)
            {
                stats.AddModifier(performer.baseStatModifiers[i].ToRuntime(sourceId));
            }
        }

        private void ApplyPassive(PerformerDefinition performer)
        {
            if (stats == null)
            {
                return;
            }

            string sourceId = PassiveSourceId(performer);
            stats.RemoveSource(sourceId);
            if (performer.passiveKind == PerformerPassiveKind.StaticStatModifiers)
            {
                staticStatPassive.Apply(performer, stats, sourceId);
            }
        }

        public static string BaseSourceId(PerformerDefinition performer)
        {
            return $"performer:{performer.Id}:base";
        }

        public static string PassiveSourceId(PerformerDefinition performer)
        {
            return $"performer:{performer.Id}:passive";
        }

        private void StartFirstWorld()
        {
            if (worldLifecycle == null)
            {
                return;
            }

            WorldGenerationResult result = worldLifecycle.TransitionToWorld(1, WorldSeed(1));
            ApplyPlayerStart(result);
        }

        private void ApplyPlayerStart(WorldGenerationResult result)
        {
            playerStartPlacement?.Apply(result);
        }

        private int WorldSeed(int worldIndex)
        {
            return runSeedState != null
                ? runSeedState.Combine(8301, worldIndex)
                : DeterministicSeed.Combine(8301, worldIndex);
        }
    }
}
