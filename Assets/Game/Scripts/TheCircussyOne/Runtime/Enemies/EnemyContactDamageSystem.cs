using UnityEngine;
using VContainer.Unity;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;
using TheCircussyOne.Visuals;

namespace TheCircussyOne.Runtime
{
    public sealed class EnemyContactDamageSystem : ITickable
    {
        private readonly GameConfig _config;
        private readonly PlayerView _player;
        private readonly ActorRegistry _registry;
        private readonly GameState _state;
        private readonly FeedbackService _feedback;
        private readonly IGameTime _time;
        private readonly WorldPhysicsQuery _worldQuery;
        private readonly RunPhaseState _phaseState;

        private float _nextContactDamageTime;

        public EnemyContactDamageSystem(
            GameConfig config,
            PlayerView player,
            ActorRegistry registry,
            GameState state,
            FeedbackService feedback,
            IGameTime time,
            WorldPhysicsQuery worldQuery = null,
            RunPhaseState phaseState = null)
        {
            _config = config;
            _player = player;
            _registry = registry;
            _state = state;
            _feedback = feedback;
            _time = time;
            _worldQuery = worldQuery ?? new WorldPhysicsQuery();
            _phaseState = phaseState;
        }

        public void Tick()
        {
            if (_state.IsGameOver || (_phaseState != null && !RunPhaseRules.ShouldTickCombat(_phaseState.CurrentPhase)))
            {
                return;
            }

            var enemies = _registry.Enemies;
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                EnemyRuntime enemy = enemies[i];
                if (!IsDamageCandidate(enemy))
                {
                    continue;
                }

                if (!ContactDamageRules.CanDamage(IsPlayerOverlappingContactHitbox(enemy.View), _time.Time, _nextContactDamageTime))
                {
                    continue;
                }

                _nextContactDamageTime = ContactDamageRules.NextAllowedTime(_time.Time, _config.contactDamageCooldown);
                _state.DamagePlayer(enemy.ContactDamage, DamageCause(enemy));
                _feedback.PlayPlayerDamage();
                return;
            }
        }

        private bool IsPlayerOverlappingContactHitbox(EnemyView enemyView)
        {
            Collider enemyCollider = enemyView != null ? enemyView.ContactHitboxCollider : null;
            Collider playerCollider = _player != null ? _player.ContactCollider : null;
            if (enemyCollider == null || playerCollider == null || !enemyCollider.enabled || !playerCollider.enabled)
            {
                return false;
            }

            return _worldQuery.TryComputePenetration(enemyCollider, playerCollider, out _, out _);
        }

        private static bool IsDamageCandidate(EnemyRuntime enemy)
        {
            return enemy != null
                && !enemy.IsDead
                && enemy.View != null
                && enemy.View.IsActive;
        }

        private static string DamageCause(EnemyRuntime enemy)
        {
            string displayName = enemy?.Definition != null ? enemy.Definition.DisplayName : null;
            return string.IsNullOrWhiteSpace(displayName) ? "Audience Member" : displayName.Trim();
        }
    }
}
