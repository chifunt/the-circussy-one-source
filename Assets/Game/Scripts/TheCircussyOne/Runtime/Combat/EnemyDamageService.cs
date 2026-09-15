using System;
using UnityEngine;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;
using TheCircussyOne.Visuals;

namespace TheCircussyOne.Runtime
{
    public readonly struct EnemyKilledEvent
    {
        public EnemyKilledEvent(EnemyRuntime enemy, Vector3 hitPosition, Vector3 hitDirection)
        {
            Enemy = enemy;
            HitPosition = hitPosition;
            HitDirection = hitDirection;
        }

        public EnemyRuntime Enemy { get; }
        public Vector3 HitPosition { get; }
        public Vector3 HitDirection { get; }
    }

    public sealed class EnemyDamageService
    {
        private readonly GameConfig _config;
        private readonly DamageFeedbackVisualConfig _damageFeedbackConfig;
        private readonly ActorMotionVisualConfig _actorMotionConfig;
        private readonly EnemyFactory _enemyFactory;
        private readonly XpDropService _xpDrops;
        private readonly GameState _state;
        private readonly FeedbackService _feedback;
        private readonly IDamageNumberSpawner _damageNumbers;
        private readonly IVfxSpawner _vfx;
        private readonly IGameAudio _audio;

        public EnemyDamageService(
            GameConfig config,
            DamageFeedbackVisualConfig damageFeedbackConfig,
            ActorMotionVisualConfig actorMotionConfig,
            EnemyFactory enemyFactory,
            XpDropService xpDrops,
            GameState state,
            FeedbackService feedback,
            IDamageNumberSpawner damageNumbers,
            IVfxSpawner vfx = null,
            IGameAudio audio = null)
        {
            _config = config;
            _damageFeedbackConfig = damageFeedbackConfig;
            _actorMotionConfig = actorMotionConfig;
            _enemyFactory = enemyFactory;
            _xpDrops = xpDrops;
            _state = state;
            _feedback = feedback;
            _damageNumbers = damageNumbers;
            _vfx = vfx;
            _audio = audio ?? NullGameAudio.Instance;
        }

        public event Action<EnemyKilledEvent> EnemyKilled;

        public bool Apply(EnemyRuntime enemy, int damage, Vector3 hitPosition, Vector3 hitDirection)
        {
            if (enemy == null || enemy.IsDead || enemy.View == null || !enemy.View.IsActive || damage <= 0)
            {
                return false;
            }

            int previousHealth = enemy.CurrentHealth;
            bool killed = DamageRules.ApplyDamage(enemy, damage);
            _state?.AddDamageDealt(Mathf.Min(previousHealth, damage));
            enemy.View.SetHealthNormalized((float)enemy.CurrentHealth / enemy.MaxHealth);
            enemy.View.PlayHit(_config != null ? _config.hitPulseSeconds : 0.08f, _damageFeedbackConfig, _actorMotionConfig);
            _feedback?.PlayEnemyHit(enemy.Position);
            _audio.PlayAt(GameAudioCue.EnemyHit, hitPosition);
            _damageNumbers?.Show(new DamageNumberRequest(
                damage,
                enemy.AimPosition,
                hitPosition,
                enemy.SpawnId + 1));
            _vfx?.Show(VfxEffectId.EnemyHitSparks, hitPosition, hitDirection);

            if (!killed)
            {
                return false;
            }

            _state?.AddKill();
            EnemyKilled?.Invoke(new EnemyKilledEvent(enemy, hitPosition, hitDirection));
            _audio.PlayAt(GameAudioCue.EnemyDeath, enemy.Position);
            _vfx?.Show(VfxEffectId.EnemyDeathBurst, enemy.Position + Vector3.up * 0.85f);
            _xpDrops?.SpawnEnemyDrops(enemy, EnemyRewardDropRules.ResolveXpBurstOrigin(enemy), _state != null ? _state.Kills : 0);
            _enemyFactory?.Despawn(enemy, playDeath: true);
            return true;
        }
    }

    public static class EnemyRewardDropRules
    {
        public static Vector3 ResolveXpBurstOrigin(EnemyRuntime enemy)
        {
            if (enemy == null)
            {
                return Vector3.up * 0.65f;
            }

            return enemy.AimPosition + Vector3.up * 0.15f;
        }
    }
}
