using System;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using TheCircussyOne.Visuals;
using UnityEngine;
using VContainer.Unity;

namespace TheCircussyOne.Runtime
{
    public sealed class HeadlinerDefeatState
    {
        private bool hasDeathPosition;
        private Vector3 deathPosition;

        public bool HasDeathPosition => hasDeathPosition;
        public Vector3 DeathPosition => deathPosition;

        public event Action<Vector3> DeathPositionRecorded;

        public void RecordDeathPosition(Vector3 position)
        {
            deathPosition = position;
            hasDeathPosition = true;
            DeathPositionRecorded?.Invoke(position);
        }

        public bool RecordDeathPositionIfMissing(Vector3 position)
        {
            if (hasDeathPosition)
            {
                return false;
            }

            RecordDeathPosition(position);
            return true;
        }

        public bool TryGetDeathPosition(out Vector3 position)
        {
            position = deathPosition;
            return hasDeathPosition;
        }

        public void Clear()
        {
            hasDeathPosition = false;
            deathPosition = default;
        }
    }

    public sealed class HeadlinerSystem : IStartable, IDisposable, IRunResettable
    {
        private readonly HeadlinerCatalog catalog;
        private readonly EnemyFactory enemyFactory;
        private readonly PlayerView player;
        private readonly RunPhaseState phaseState;
        private readonly RunActScheduleState scheduleState;
        private readonly EnemyDamageService damageService;
        private readonly WorldSurfaceResolver surfaceResolver;
        private readonly RunWorldGenerationState generationState;
        private readonly HeadlinerDefeatState defeatState;

        private HeadlinerRuntime activeHeadliner;

        public HeadlinerSystem(
            HeadlinerCatalog catalog,
            EnemyFactory enemyFactory,
            PlayerView player,
            RunPhaseState phaseState,
            RunActScheduleState scheduleState,
            EnemyDamageService damageService,
            WorldSurfaceResolver surfaceResolver = null,
            RunWorldGenerationState generationState = null,
            HeadlinerDefeatState defeatState = null)
        {
            this.catalog = catalog;
            this.enemyFactory = enemyFactory;
            this.player = player;
            this.phaseState = phaseState;
            this.scheduleState = scheduleState;
            this.damageService = damageService;
            this.surfaceResolver = surfaceResolver ?? new WorldSurfaceResolver();
            this.generationState = generationState;
            this.defeatState = defeatState;
        }

        public HeadlinerRuntime ActiveHeadliner => activeHeadliner != null && activeHeadliner.IsActive ? activeHeadliner : null;

        public void ResetRunState(RunResetContext context)
        {
            ClearActiveHeadliner(despawn: true);
            defeatState?.Clear();
        }

        public void Start()
        {
            if (phaseState != null)
            {
                phaseState.PhaseChanged += OnPhaseChanged;
            }

            if (damageService != null)
            {
                damageService.EnemyKilled += OnEnemyKilled;
            }

            if (phaseState != null && phaseState.Is(RunPhase.BossActive))
            {
                SpawnForCurrentAct();
            }
        }

        public void Dispose()
        {
            if (phaseState != null)
            {
                phaseState.PhaseChanged -= OnPhaseChanged;
            }

            if (damageService != null)
            {
                damageService.EnemyKilled -= OnEnemyKilled;
            }

            ClearActiveHeadliner(despawn: true);
        }

        private void OnPhaseChanged(RunPhase previous, RunPhase next)
        {
            if (next == RunPhase.BossActive)
            {
                SpawnForCurrentAct();
                return;
            }

            if (next is RunPhase.PerformerSelection or RunPhase.WorldActive or RunPhase.Intermission or RunPhase.NextWorldTransition or RunPhase.Failed or RunPhase.Finished)
            {
                ClearActiveHeadliner(despawn: true);
            }
        }

        private void OnEnemyKilled(EnemyKilledEvent killed)
        {
            if (activeHeadliner == null || killed.Enemy != activeHeadliner.Enemy)
            {
                return;
            }

            defeatState?.RecordDeathPositionIfMissing(killed.Enemy.Position);
            activeHeadliner = null;
            phaseState?.MarkBossDefeated();
        }

        private void SpawnForCurrentAct()
        {
            if (ActiveHeadliner != null || catalog == null || enemyFactory == null)
            {
                return;
            }

            int actNumber = scheduleState != null && scheduleState.HasActiveAct ? scheduleState.ActNumber : phaseState?.WorldIndex ?? 1;
            HeadlinerDefinition definition = catalog.ForAct(actNumber);
            if (!ContentAvailabilityRules.IsActiveAndValid(definition) || !ContentAvailabilityRules.IsActiveAndValid(definition.enemyActor))
            {
                return;
            }

            Vector3 position = ResolveSpawnPosition(definition);
            EnemyRuntime enemy = enemyFactory.Spawn(definition.enemyActor, position, actNumber, 0f);
            activeHeadliner = new HeadlinerRuntime(definition, enemy, actNumber);
        }

        private Vector3 ResolveSpawnPosition(HeadlinerDefinition definition)
        {
            Vector3 desired;
            if (generationState != null && generationState.TryGetHeadlinerAnchor(out Vector3 anchor))
            {
                desired = anchor;
            }
            else
            {
                Vector3 playerPosition = player != null ? player.Position : Vector3.zero;
                Vector3 forward = player != null && player.Body != null ? player.Body.forward : Vector3.forward;
                forward.y = 0f;
                if (forward.sqrMagnitude < 0.0001f)
                {
                    forward = Vector3.forward;
                }

                forward.Normalize();
                desired = playerPosition + forward * Mathf.Max(0f, definition.spawnDistanceFromPlayer);
            }

            WorldSurfaceSample surface = surfaceResolver.Resolve(
                desired,
                definition.spawnProbeHeight,
                definition.spawnProbeDepth,
                GameLayers.EnvironmentMaskExcludingGameplay);

            Vector3 position = surface.FoundSurface ? surface.Position : desired;
            position += Vector3.up * Mathf.Max(0f, definition.spawnGroundClearance);
            return position;
        }

        private void ClearActiveHeadliner(bool despawn)
        {
            HeadlinerRuntime headliner = activeHeadliner;
            activeHeadliner = null;
            if (!despawn || headliner?.Enemy == null || headliner.Enemy.IsDead)
            {
                return;
            }

            enemyFactory?.Despawn(headliner.Enemy, playDeath: false);
        }
    }
}
