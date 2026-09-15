using UnityEngine;
using VContainer.Unity;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;

namespace TheCircussyOne.Runtime
{
    public sealed class EnemySpawnIndicatorSystem : ITickable
    {
        private const float EnemySpawnClearanceRadius = 0.56f;
        private const float EnemySpawnClearanceHeight = 1.6f;
        private const float EnemySpawnGroundClearance = 0.04f;

        private readonly EnemySpawnVisualConfig config;
        private readonly EnemySpawnIndicatorFactory indicatorFactory;
        private readonly EnemyFactory enemyFactory;
        private readonly GameState state;
        private readonly IGameTime time;
        private readonly WorldSurfaceResolver surfaceResolver;
        private readonly RunPhaseState phaseState;
        private readonly RunWorldGenerationState generationState;
        private readonly RunWorldGenerationConfig worldGenerationConfig;
        private readonly WorldPhysicsQuery spawnClearanceQuery = new();

        public EnemySpawnIndicatorSystem(
            EnemySpawnVisualConfig config,
            EnemySpawnIndicatorFactory indicatorFactory,
            EnemyFactory enemyFactory,
            GameState state,
            IGameTime time,
            WorldSurfaceResolver surfaceResolver = null,
            RunPhaseState phaseState = null,
            RunWorldGenerationState generationState = null,
            RunWorldGenerationConfig worldGenerationConfig = null)
        {
            this.config = config;
            this.indicatorFactory = indicatorFactory;
            this.enemyFactory = enemyFactory;
            this.state = state;
            this.time = time;
            this.surfaceResolver = surfaceResolver ?? new WorldSurfaceResolver();
            this.phaseState = phaseState;
            this.generationState = generationState;
            this.worldGenerationConfig = worldGenerationConfig;
        }

        public int PendingCount => indicatorFactory?.Active.Count ?? 0;

        public EnemySpawnIndicatorRuntime RequestSpawn(Vector3 position, int level, float elapsedSeconds)
        {
            return RequestSpawn(position, level, elapsedSeconds, enemyDefinition: null);
        }

        public EnemySpawnIndicatorRuntime RequestSpawn(Vector3 position, int level, float elapsedSeconds, bool useEncorePool)
        {
            EnemyDefinition definition = enemyFactory != null
                ? enemyFactory.ResolveSpawnDefinition(useEncorePool)
                : null;
            return RequestSpawn(position, level, elapsedSeconds, definition);
        }

        public EnemySpawnIndicatorRuntime RequestSpawn(Vector3 position, int level, float elapsedSeconds, EnemyDefinition enemyDefinition)
        {
            WorldSurfaceSample surface = ResolveSpawnSurface(position);
            if (config == null || !config.enabled || indicatorFactory == null)
            {
                enemyFactory.Spawn(enemyDefinition, surface.Position, level, elapsedSeconds);
                return null;
            }

            return indicatorFactory.Spawn(surface.Position, surface.Normal, level, elapsedSeconds, time.Time, enemyDefinition);
        }

        public void Tick()
        {
            if (indicatorFactory == null || config == null)
            {
                return;
            }

            for (int i = indicatorFactory.Active.Count - 1; i >= 0; i--)
            {
                EnemySpawnIndicatorRuntime indicator = indicatorFactory.Active[i];
                if (indicator == null || indicator.View == null || !indicator.View.IsActive)
                {
                    indicatorFactory.Despawn(indicator);
                    continue;
                }

                if ((state != null && state.IsGameOver)
                    || (phaseState != null && !RunPhaseRules.ShouldSpawnEnemies(phaseState.CurrentPhase)))
                {
                    if (indicator.SpawnedEnemy != null)
                    {
                        enemyFactory.Despawn(indicator.SpawnedEnemy, playDeath: false);
                    }

                    indicatorFactory.Despawn(indicator);
                    continue;
                }

                float warningT = EnemySpawnIndicatorRules.NormalizedWarning(indicator.StartedAt, time.Time, indicator.WarningSeconds);
                float emergeT = indicator.HasSpawnedEnemy
                    ? EnemySpawnIndicatorRules.NormalizedEmerge(indicator.EnemySpawnedAt, time.Time, indicator.EmergeSeconds)
                    : 0f;

                indicator.View.ApplyFrame(EnemySpawnIndicatorRules.Evaluate(
                    warningT,
                    emergeT,
                    config.radius,
                    config.innerStartRadius,
                    config.centerRadiusMultiplier,
                    config.outerColor,
                    config.innerColor,
                    config.centerColor,
                    config.outerPulseStrength,
                    config.outerPulseCycles,
                    config.innerExpandEase,
                    config.outerPulseEase));

                if (!indicator.HasSpawnedEnemy && EnemySpawnIndicatorRules.WarningComplete(warningT))
                {
                    EnemyRuntime enemy = enemyFactory.SpawnPending(indicator.EnemyDefinition, indicator.Position, indicator.Level, indicator.EnemyElapsedSeconds);
                    enemy.View.BeginSpawnEmergence(config.emergeStartBodyY);
                    indicator.MarkEnemySpawned(enemy, indicator.StartedAt + indicator.WarningSeconds);
                    emergeT = EnemySpawnIndicatorRules.NormalizedEmerge(indicator.EnemySpawnedAt, time.Time, indicator.EmergeSeconds);
                }

                if (indicator.HasSpawnedEnemy)
                {
                    indicator.SpawnedEnemy.View.ApplySpawnEmergence(
                        emergeT,
                        config.emergeStartBodyY,
                        config.emergeEndBodyY,
                        config.enemyEmergeEase);

                    if (EnemySpawnIndicatorRules.EmergenceComplete(emergeT))
                    {
                        indicator.SpawnedEnemy.View.CompleteSpawnEmergence(config.emergeEndBodyY);
                        enemyFactory.ActivatePending(indicator.SpawnedEnemy);
                        indicatorFactory.Despawn(indicator);
                    }
                }
            }
        }

        public void Clear()
        {
            if (indicatorFactory == null)
            {
                return;
            }

            for (int i = indicatorFactory.Active.Count - 1; i >= 0; i--)
            {
                EnemySpawnIndicatorRuntime indicator = indicatorFactory.Active[i];
                if (indicator?.SpawnedEnemy != null)
                {
                    enemyFactory.Despawn(indicator.SpawnedEnemy, playDeath: false);
                }

                indicatorFactory.Despawn(indicator);
            }
        }

        private WorldSurfaceSample ResolveSpawnSurface(Vector3 position)
        {
            position = ResolveGeneratedSpawnPosition(position);
            if (surfaceResolver == null)
            {
                return new WorldSurfaceSample(position, Vector3.up, foundSurface: false);
            }

            float probeHeight = config != null ? config.surfaceProbeHeight : 12f;
            float probeDepth = config != null ? config.surfaceProbeDepth : 24f;
            int mask = SpawnSurfaceMask();
            return surfaceResolver.Resolve(position, probeHeight, probeDepth, mask);
        }

        private Vector3 ResolveGeneratedSpawnPosition(Vector3 position)
        {
            if (generationState == null || !generationState.HasGeneratedLayout || !generationState.HasGeneratedMap)
            {
                return position;
            }

            float searchRadius = worldGenerationConfig != null ? worldGenerationConfig.EnemySpawnSafeSearchRadius : 32f;
            float probeHeight = config != null ? config.surfaceProbeHeight : 12f;
            float probeDepth = config != null ? config.surfaceProbeDepth : 24f;
            var clearance = new GeneratedWorldSpawnClearance(
                EnemySpawnClearanceRadius,
                EnemySpawnClearanceHeight,
                EnemySpawnGroundClearance,
                probeHeight,
                probeDepth,
                SpawnSurfaceMask(),
                spawnClearanceQuery);
            GeneratedWorldMap map = generationState.GeneratedMap;
            if (GeneratedWorldSpawnPointResolver.TryResolve(
                    map,
                    position,
                    searchRadius,
                    clearance,
                    out Vector3 resolvedPosition))
            {
                return resolvedPosition;
            }

            return GeneratedWorldSpawnPointResolver.TryResolveNearest(
                map,
                position,
                clearance,
                out resolvedPosition)
                ? resolvedPosition
                : position;
        }

        private int SpawnSurfaceMask()
        {
            int configuredMask = config != null ? config.surfaceMask.value : ~0;
            int mask = configuredMask & GameLayers.EnvironmentMaskExcludingGameplay;
            return mask != 0 ? mask : GameLayers.EnvironmentMaskExcludingGameplay;
        }
    }
}
