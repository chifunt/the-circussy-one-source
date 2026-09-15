using UnityEngine;
using VContainer.Unity;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;
using TheCircussyOne.Visuals;

namespace TheCircussyOne.Runtime
{
    public sealed class EnemyBodyCollisionSystem : ITickable
    {
        private const float Epsilon = 0.0001f;

        private readonly GameConfig _config;
        private readonly PlayerView _player;
        private readonly ActorRegistry _registry;
        private readonly WorldPhysicsQuery _worldQuery;
        private readonly RunPhaseState _phaseState;
        private readonly EnemySpatialIndex _enemySpatialIndex = new();
        private readonly System.Collections.Generic.List<int> _nearbyEnemyIndices = new(32);

        public EnemyBodyCollisionSystem(
            GameConfig config,
            PlayerView player,
            ActorRegistry registry,
            WorldPhysicsQuery worldQuery = null,
            RunPhaseState phaseState = null)
        {
            _config = config;
            _player = player;
            _registry = registry;
            _worldQuery = worldQuery ?? new WorldPhysicsQuery();
            _phaseState = phaseState;
        }

        public void Tick()
        {
            if (_phaseState != null && !RunPhaseRules.ShouldTickCombat(_phaseState.CurrentPhase))
            {
                return;
            }

            ResolvePlayerEnemyOverlaps();
        }

        private void ResolvePlayerEnemyOverlaps()
        {
            Collider playerCollider = _player != null ? _player.ContactCollider : null;
            float remainingPush = Mathf.Max(0f, _config.playerEnemyBlockMaxPushPerFrame);
            if (playerCollider == null || remainingPush <= 0f)
            {
                return;
            }

            var enemies = _registry.Enemies;
            _enemySpatialIndex.Rebuild(enemies, PlayerBlockCandidateCellSize(playerCollider, enemies));
            _enemySpatialIndex.CollectNearbyIndices(_player.Position, _nearbyEnemyIndices);

            Vector3 accumulatedPush = Vector3.zero;
            Vector3 deepestPush = Vector3.zero;
            float deepestDistance = 0f;
            for (int nearbyIndex = 0; nearbyIndex < _nearbyEnemyIndices.Count; nearbyIndex++)
            {
                int i = _nearbyEnemyIndices[nearbyIndex];
                EnemyRuntime enemy = enemies[i];
                if (!IsBlockable(enemy))
                {
                    continue;
                }

                Collider enemyCollider = enemy.View.MovementBodyCollider;
                if (enemyCollider == null || !enemyCollider.enabled)
                {
                    continue;
                }

                if (!_worldQuery.TryComputeHorizontalPenetration(playerCollider, enemyCollider, out Vector3 horizontalDirection, out float distance))
                {
                    continue;
                }

                if (horizontalDirection.sqrMagnitude <= Epsilon)
                {
                    horizontalDirection = HorizontalOffset(_player.Position, enemy.View.MovementBodyCenter);
                }

                if (horizontalDirection.sqrMagnitude <= Epsilon)
                {
                    horizontalDirection = FallbackSeparationDirection(i, -1);
                }

                horizontalDirection.Normalize();
                Vector3 push = horizontalDirection * distance;
                accumulatedPush += push;
                if (distance > deepestDistance)
                {
                    deepestDistance = distance;
                    deepestPush = push;
                }
            }

            Vector3 finalPush = accumulatedPush;
            finalPush.y = 0f;
            if (finalPush.sqrMagnitude <= Epsilon && deepestDistance > Epsilon)
            {
                finalPush = deepestPush;
            }

            float magnitude = finalPush.magnitude;
            if (magnitude <= Epsilon)
            {
                return;
            }

            if (magnitude > remainingPush)
            {
                finalPush = finalPush / magnitude * remainingPush;
            }

            _player.MoveDisplacement(finalPush);
        }

        private static bool IsBlockable(EnemyRuntime enemy)
        {
            return enemy != null
                && !enemy.IsDead
                && enemy.View != null
                && enemy.View.IsActive
                && enemy.View.MovementBodyCollider != null;
        }

        private static Vector3 HorizontalOffset(Vector3 from, Vector3 to)
        {
            Vector3 offset = from - to;
            offset.y = 0f;
            return offset;
        }

        private static Vector3 FallbackSeparationDirection(int firstIndex, int secondIndex)
        {
            int hash = firstIndex * 73856093 ^ secondIndex * 19349663;
            float angle = (hash & 1023) / 1023f * Mathf.PI * 2f;
            return new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
        }

        private static float PlayerBlockCandidateCellSize(Collider playerCollider, System.Collections.Generic.IReadOnlyList<EnemyRuntime> enemies)
        {
            float playerRadius = playerCollider is CharacterController controller
                ? controller.radius
                : HorizontalMagnitude(playerCollider.bounds.extents);
            float largestEnemyReach = 0f;
            for (int i = 0; i < enemies.Count; i++)
            {
                EnemyRuntime enemy = enemies[i];
                if (!IsBlockable(enemy))
                {
                    continue;
                }

                Vector3 enemyOffset = enemy.BodyProfile != null ? enemy.BodyProfile.movementBodyOffset : Vector3.zero;
                float enemyOffsetRadius = new Vector2(enemyOffset.x, enemyOffset.z).magnitude;
                float enemyRadius = enemy.BodyProfile != null ? enemy.BodyProfile.movementBodyRadius : enemy.View.MovementBodyRadius;
                largestEnemyReach = Mathf.Max(largestEnemyReach, enemyRadius + enemyOffsetRadius);
            }

            return Mathf.Max(
                EnemyMovementRules.MinimumMoveDistance,
                playerRadius + largestEnemyReach);
        }

        private static float HorizontalMagnitude(Vector3 vector)
        {
            vector.y = 0f;
            return vector.magnitude;
        }
    }
}
