using UnityEngine;
using VContainer.Unity;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using TheCircussyOne.Visuals;

namespace TheCircussyOne.Runtime
{
    public sealed class EnemyDirectorSystem : ITickable
    {
        private readonly GameConfig _config;
        private readonly PlayerView _player;
        private readonly ActorRegistry _registry;
        private readonly GameState _state;
        private readonly IGameTime _time;
        private readonly EnemyEnvironmentSurfaceSampler _surfaceSampler;
        private readonly EnemySupportSurfaceSampler _supportSurfaceSampler;
        private readonly RunPhaseState _phaseState;
        private readonly EnemySpatialIndex _enemySpatialIndex = new();
        private readonly System.Collections.Generic.List<int> _nearbyEnemyIndices = new(32);

        public EnemyDirectorSystem(GameConfig config, PlayerView player, ActorRegistry registry, GameState state, IGameTime time)
            : this(config, player, registry, state, time, new EnemyEnvironmentSurfaceSampler(), new EnemySupportSurfaceSampler())
        {
        }

        public EnemyDirectorSystem(
            GameConfig config,
            PlayerView player,
            ActorRegistry registry,
            GameState state,
            IGameTime time,
            EnemyEnvironmentSurfaceSampler surfaceSampler)
            : this(config, player, registry, state, time, surfaceSampler, new EnemySupportSurfaceSampler())
        {
        }

        public EnemyDirectorSystem(
            GameConfig config,
            PlayerView player,
            ActorRegistry registry,
            GameState state,
            IGameTime time,
            EnemyEnvironmentSurfaceSampler surfaceSampler,
            EnemySupportSurfaceSampler supportSurfaceSampler,
            RunPhaseState phaseState = null)
        {
            _config = config;
            _player = player;
            _registry = registry;
            _state = state;
            _time = time;
            _surfaceSampler = surfaceSampler;
            _supportSurfaceSampler = supportSurfaceSampler;
            _phaseState = phaseState;
        }

        public void Tick()
        {
            if (_state.IsGameOver || (_phaseState != null && !RunPhaseRules.ShouldTickCombat(_phaseState.CurrentPhase)))
            {
                return;
            }

            Vector3 playerPosition = _player.Position;
            var enemies = _registry.Enemies;
            RemoveInvalidEnemies(enemies);
            _enemySpatialIndex.Rebuild(enemies, EnemySpatialCellSize(enemies));
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                EnemyRuntime enemy = enemies[i];
                if (enemy == null || enemy.IsDead || enemy.View == null || !enemy.View.IsActive)
                {
                    continue;
                }

                enemy.View.SetMotionVisualSpeed(0f);
                Vector3 toPlayer = EnemyMovementRules.HorizontalOffsetToTarget(enemy.Position, playerPosition);
                if (EnemyMovementRules.TryGetDirection(toPlayer, out Vector3 chaseDirection, out float distance))
                {
                    Vector3 desired = Vector3.zero;
                    if (EnemyMovementRules.ShouldChase(distance, _config.enemyPlayerStopDistance))
                    {
                        desired += chaseDirection;
                    }

                    desired += EnemyMovementRules.SeparationAwayFrom(
                        enemy.Position,
                        playerPosition,
                        _config.enemyPlayerStopDistance,
                        -chaseDirection) * _config.enemySeparationWeight;

                    desired += EnemySeparationFor(enemy, i, enemies) * _config.enemySeparationWeight;

                    if (EnemyMovementRules.TryGetDirection(desired, out Vector3 desiredDirection, out _))
                    {
                        enemy.View.TurnTowards(desiredDirection, enemy.TurnDegreesPerSecond, _time.DeltaTime);
                        Vector3 moveDirection = enemy.View.HorizontalForward;
                        float alignment = EnemyMovementRules.MovementAlignmentMultiplier(moveDirection, desiredDirection);
                        float desiredMoveDistance = enemy.MoveSpeed * _time.DeltaTime * alignment;
                        float towardPlayerDot = Vector3.Dot(moveDirection, chaseDirection);
                        float moveDistance = EnemyMovementRules.AllowedMoveDistance(
                            distance,
                            _config.enemyPlayerStopDistance,
                            desiredMoveDistance,
                            towardPlayerDot);

                        if (moveDistance > 0f)
                        {
                            float actualSpeed = moveDistance / Mathf.Max(0.0001f, _time.DeltaTime);
                            if (CanApplyHorizontalMovement(enemy, moveDirection, moveDistance))
                            {
                                enemy.View.Move(moveDirection, actualSpeed, _time.DeltaTime);
                                enemy.View.SetMotionVisualSpeed(ActorMotionVisualRules.NormalizeSpeed(actualSpeed, enemy.MoveSpeed));
                            }
                        }
                    }
                }

            }
        }

        private bool CanApplyHorizontalMovement(EnemyRuntime enemy, Vector3 moveDirection, float moveDistance)
        {
            if (!CanUseKinematicClimb(enemy))
            {
                return true;
            }

            EnemyVerticalMotorState motor = enemy.VerticalMotor;
            float currentHeight = enemy.Position.y;
            if (EnemyTerrainLocomotionRules.HasReachedClimbReleaseHeight(motor, currentHeight))
            {
                motor.ClearEnvironmentClimb();
            }

            if (EnemyTerrainLocomotionRules.ShouldHoldHorizontalForClimb(motor, currentHeight))
            {
                return false;
            }

            if (EnemySupportClimbRules.ShouldHoldHorizontalForSupport(motor, currentHeight))
            {
                return false;
            }

            int environmentMask = EnvironmentMaskExcludingGameplay();
            if (CanUseEnvironmentClimb(enemy) && _surfaceSampler.TryFindHorizontalBlock(
                    enemy.Position,
                    moveDirection,
                    moveDistance + 0.05f,
                    enemy.BodyProfile,
                    environmentMask,
                    out RaycastHit blockHit))
            {
                if (IsArenaBarrierBlock(blockHit.collider))
                {
                    return false;
                }

                EnemyClimbProfile climb = enemy.ClimbProfile;
                bool hasTargetHeight = TrySampleBlockedClimbTarget(
                    enemy,
                    moveDirection,
                    moveDistance,
                    blockHit,
                    climb,
                    environmentMask,
                    out float targetHeight,
                    out BlockedClimbTargetSample targetSample);
                EnemyTerrainLocomotionDecision decision = EnemyTerrainLocomotionRules.ClassifyBlockedSurface(
                    hasTargetHeight,
                    currentHeight,
                    targetHeight,
                    climb.maxEnvironmentClimbHeight);
                LogEnvironmentBlockDecision(enemy, blockHit, hasTargetHeight, targetHeight, currentHeight, climb, decision, targetSample);

                if (decision == EnemyTerrainLocomotionDecision.Walkable)
                {
                    return true;
                }

                if (decision == EnemyTerrainLocomotionDecision.ClimbBlocked)
                {
                    motor.BeginEnvironmentClimb(targetHeight, EnemyTerrainLocomotionRules.ClimbReleaseTolerance);
                }

                return false;
            }

            if (CanUseSupportClimb(enemy)
                && _supportSurfaceSampler.TryFindSupportBlock(
                    enemy,
                    moveDirection,
                    moveDistance + 0.05f,
                    _registry.Enemies,
                    _enemySpatialIndex,
                    out EnemySupportSurfaceHit supportHit))
            {
                motor.BeginSupportClimb(
                    supportHit.TargetHeight,
                    EnemySupportClimbRules.SupportReleaseTolerance,
                    supportHit.TargetLayer,
                    supportHit.Support.SpawnId);
                return false;
            }

            return true;
        }

        private bool TrySampleBlockedClimbTarget(
            EnemyRuntime enemy,
            Vector3 moveDirection,
            float moveDistance,
            RaycastHit blockHit,
            EnemyClimbProfile climb,
            int environmentMask,
            out float targetHeight,
            out BlockedClimbTargetSample sample)
        {
            targetHeight = 0f;
            sample = default;
            if (climb == null || environmentMask == 0 || !EnemyTerrainLocomotionRules.IsValidHorizontalDirection(moveDirection))
            {
                return false;
            }

            Vector3 direction = moveDirection;
            direction.y = 0f;
            direction.Normalize();
            float bodyRadius = enemy.BodyProfile != null ? enemy.BodyProfile.movementBodyRadius : 0.52f;
            float sampleReach = Mathf.Max(bodyRadius + 0.05f, Mathf.Max(0f, climb.environmentProbeDistance));
            if (TryGetConstructedRampClimbTarget(enemy, blockHit, out targetHeight, out sample))
            {
                return true;
            }

            bool found = false;
            Vector3 sampleAnchor = BlockTargetSampleAnchor(enemy, blockHit, moveDistance, sampleReach);
            Vector3 firstSample = sampleAnchor + direction * sampleReach;
            if (_surfaceSampler.TrySampleHeight(firstSample, EnemyTerrainLocomotionRules.EnvironmentClimbProbeHeight, climb.environmentProbeDistance, environmentMask, out float firstHeight))
            {
                targetHeight = Mathf.Max(targetHeight, firstHeight);
                sample.FirstFound = true;
                sample.FirstPosition = firstSample;
                sample.FirstHeight = firstHeight;
                found = true;
            }

            Vector3 secondSample = enemy.Position + direction * Mathf.Max(moveDistance + sampleReach, sampleReach);
            if (_surfaceSampler.TrySampleHeight(secondSample, EnemyTerrainLocomotionRules.EnvironmentClimbProbeHeight, climb.environmentProbeDistance, environmentMask, out float secondHeight))
            {
                targetHeight = Mathf.Max(targetHeight, secondHeight);
                sample.SecondFound = true;
                sample.SecondPosition = secondSample;
                sample.SecondHeight = secondHeight;
                found = true;
            }

            return found;
        }

        private static bool TryGetConstructedRampClimbTarget(
            EnemyRuntime enemy,
            RaycastHit blockHit,
            out float targetHeight,
            out BlockedClimbTargetSample sample)
        {
            targetHeight = 0f;
            sample = default;
            if (blockHit.collider == null)
            {
                return false;
            }

            GeneratedRampMarker rampMarker = blockHit.collider.GetComponentInParent<GeneratedRampMarker>();
            if (rampMarker == null)
            {
                return false;
            }

            Vector3 referencePoint = RampTargetReferencePoint(rampMarker, blockHit.point, enemy != null ? enemy.Position : Vector3.zero);
            targetHeight = rampMarker.HeightAt(referencePoint);
            sample.FirstFound = true;
            sample.FirstPosition = referencePoint;
            sample.FirstHeight = targetHeight;
            return true;
        }

        private static Vector3 RampTargetReferencePoint(GeneratedRampMarker rampMarker, Vector3 hitPoint, Vector3 fallbackPoint)
        {
            if (rampMarker == null)
            {
                return fallbackPoint;
            }

            rampMarker.Project(hitPoint, out float along, out float across);
            float lengthTolerance = Mathf.Max(0.08f, rampMarker.HalfLength * 0.04f);
            float widthTolerance = Mathf.Max(0.08f, rampMarker.HalfWidth * 0.12f);
            bool hitPointLooksValid = along >= -rampMarker.HalfLength - lengthTolerance
                && along <= rampMarker.HalfLength + lengthTolerance
                && Mathf.Abs(across) <= rampMarker.HalfWidth + widthTolerance;

            return hitPointLooksValid ? hitPoint : fallbackPoint;
        }

        private void LogEnvironmentBlockDecision(
            EnemyRuntime enemy,
            RaycastHit blockHit,
            bool hasTargetHeight,
            float targetHeight,
            float currentHeight,
            EnemyClimbProfile climb,
            EnemyTerrainLocomotionDecision decision,
            BlockedClimbTargetSample sample)
        {
            if (_config == null || !_config.enemyEnvironmentBlockDiagnosticsEnabled)
            {
                return;
            }

            GeneratedRampHitKind rampHit = GeneratedRampHitClassifier.Classify(
                blockHit.collider,
                blockHit.point,
                blockHit.normal,
                out GeneratedRampMarker rampMarker);
            string enemyName = enemy?.View != null ? enemy.View.name : $"spawn-{enemy?.SpawnId ?? -1}";
            string colliderName = blockHit.collider != null ? blockHit.collider.name : "<none>";
            string rampName = rampMarker != null ? rampMarker.name : "<none>";
            float maxClimbHeight = climb != null ? climb.maxEnvironmentClimbHeight : 0f;
            float climbDelta = hasTargetHeight
                ? EnemyTerrainLocomotionRules.ClimbDelta(currentHeight, targetHeight)
                : 0f;
            Debug.Log(
                "[TCO-EnemyBlockDiag] "
                + $"enemy={enemyName} "
                + $"enemyPos={FormatVector(enemy != null ? enemy.Position : Vector3.zero)} "
                + $"collider={colliderName} "
                + $"hitPoint={FormatVector(blockHit.point)} "
                + $"hitNormal={FormatVector(blockHit.normal)} "
                + $"hitDistance={blockHit.distance:0.###} "
                + $"rampHit={rampHit} "
                + $"ramp={rampName} "
                + $"currentHeight={currentHeight:0.###} "
                + $"hasTarget={hasTargetHeight} "
                + $"targetHeight={targetHeight:0.###} "
                + $"climbDelta={climbDelta:0.###} "
                + $"configuredMaxClimb={maxClimbHeight:0.###} "
                + $"probeHeight={EnemyTerrainLocomotionRules.EnvironmentClimbProbeHeight:0.###} "
                + $"decision={decision} "
                + $"first={SampleText(sample.FirstFound, sample.FirstPosition, sample.FirstHeight)} "
                + $"second={SampleText(sample.SecondFound, sample.SecondPosition, sample.SecondHeight)}");
        }

        private static string SampleText(bool found, Vector3 position, float height)
        {
            return found ? $"{FormatVector(position)}=>{height:0.###}" : "<none>";
        }

        private static string FormatVector(Vector3 value)
        {
            return value.ToString("F2");
        }

        private static Vector3 BlockTargetSampleAnchor(
            EnemyRuntime enemy,
            RaycastHit blockHit,
            float moveDistance,
            float sampleReach)
        {
            Vector3 fallback = enemy != null ? enemy.Position : Vector3.zero;
            if (blockHit.collider == null)
            {
                return fallback;
            }

            Vector3 hitPoint = blockHit.point;
            Vector2 enemyHorizontal = new(fallback.x, fallback.z);
            Vector2 hitHorizontal = new(hitPoint.x, hitPoint.z);
            float maxExpectedDistance = Mathf.Max(0f, moveDistance)
                + Mathf.Max(0f, sampleReach)
                + 0.25f;
            if ((hitHorizontal - enemyHorizontal).sqrMagnitude > maxExpectedDistance * maxExpectedDistance)
            {
                return fallback;
            }

            return hitPoint;
        }

        private bool CanUseKinematicClimb(EnemyRuntime enemy)
        {
            return _config != null
                && _config.enemyClimbingEnabled
                && enemy != null
                && enemy.LocomotionProfile.mode != EnemyLocomotionMode.Floating
                && enemy.VerticalMotor != null
                && enemy.ClimbProfile != null;
        }

        private bool CanUseEnvironmentClimb(EnemyRuntime enemy)
        {
            return enemy != null
                && enemy.ClimbProfile != null
                && enemy.ClimbProfile.canClimbEnvironment
                && _surfaceSampler != null;
        }

        private bool CanUseSupportClimb(EnemyRuntime enemy)
        {
            return _config != null
                && _config.enemyPileClimbingEnabled
                && _supportSurfaceSampler != null
                && enemy != null
                && enemy.LocomotionProfile.mode != EnemyLocomotionMode.Floating
                && EnemySupportStackRules.CanUseSupportStack(enemy.StackProfile, enemy.LocomotionProfile);
        }

        private static bool IsArenaBarrierBlock(Collider collider)
        {
            return collider != null && collider.GetComponentInParent<ArenaBarrierCollisionMarker>() != null;
        }

        private struct BlockedClimbTargetSample
        {
            public bool FirstFound;
            public Vector3 FirstPosition;
            public float FirstHeight;
            public bool SecondFound;
            public Vector3 SecondPosition;
            public float SecondHeight;
        }

        private Vector3 EnemySeparationFor(EnemyRuntime enemy, int index, System.Collections.Generic.IReadOnlyList<EnemyRuntime> enemies)
        {
            Vector3 separation = Vector3.zero;
            _enemySpatialIndex.CollectNearbyIndices(enemy.Position, _nearbyEnemyIndices);
            for (int nearbyIndex = 0; nearbyIndex < _nearbyEnemyIndices.Count; nearbyIndex++)
            {
                int otherIndex = _nearbyEnemyIndices[nearbyIndex];
                if (otherIndex == index)
                {
                    continue;
                }

                EnemyRuntime other = enemies[otherIndex];
                if (other == null || other.IsDead || other.View == null || !other.View.IsActive)
                {
                    continue;
                }

                Vector3 separationAway = EnemyMovementRules.SeparationAwayFrom(
                    enemy.Position,
                    other.Position,
                    _config.enemySeparationRadius,
                    FallbackSeparationDirection(index, otherIndex));
                separation += separationAway * EnemySupportStackRules.SeparationMultiplier(
                    _config != null && _config.enemyPileClimbingEnabled,
                    enemy.StackProfile,
                    enemy.LocomotionProfile,
                    enemy.SpawnId,
                    enemy.VerticalMotor != null ? enemy.VerticalMotor.StackLayer : 0,
                    enemy.BodyProfile != null ? enemy.BodyProfile.movementBodyRadius : enemy.View.MovementBodyRadius,
                    enemy.Position,
                    enemy.VerticalMotor != null && enemy.VerticalMotor.IsSupportClimbing,
                    other.StackProfile,
                    other.LocomotionProfile,
                    other.SpawnId,
                    other.VerticalMotor != null ? other.VerticalMotor.StackLayer : 0,
                    other.BodyProfile != null ? other.BodyProfile.movementBodyRadius : other.View.MovementBodyRadius,
                    other.Position,
                    other.VerticalMotor != null && other.VerticalMotor.IsSupportClimbing);
            }

            return separation;
        }

        private void RemoveInvalidEnemies(System.Collections.Generic.IReadOnlyList<EnemyRuntime> enemies)
        {
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                EnemyRuntime enemy = enemies[i];
                if (enemy == null || enemy.IsDead || enemy.View == null || !enemy.View.IsActive)
                {
                    _registry.Unregister(enemy);
                }
            }
        }

        private static Vector3 FallbackSeparationDirection(int index, int otherIndex)
        {
            int hash = index * 73856093 ^ otherIndex * 19349663;
            float angle = (hash & 1023) / 1023f * Mathf.PI * 2f;
            return new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
        }

        private float EnemySpatialCellSize(System.Collections.Generic.IReadOnlyList<EnemyRuntime> enemies)
        {
            float cellSize = Mathf.Max(EnemyMovementRules.MinimumMoveDistance, _config.enemySeparationRadius);
            for (int i = 0; i < enemies.Count; i++)
            {
                EnemyRuntime enemy = enemies[i];
                if (enemy == null || enemy.BodyProfile == null)
                {
                    continue;
                }

                cellSize = Mathf.Max(
                    cellSize,
                    enemy.BodyProfile.movementBodyRadius * 2f + EnemySupportClimbRules.SupportReachPadding);
            }

            return cellSize;
        }

        private int EnvironmentMaskExcludingGameplay()
        {
            int configuredMask = _config.enemyClimbEnvironmentMask.value;
            int gameplayMask = LayerMask.GetMask(
                GameLayers.Player,
                GameLayers.Enemy,
                GameLayers.Projectile,
                GameLayers.Pickup);
            return configuredMask & ~gameplayMask;
        }
    }
}
