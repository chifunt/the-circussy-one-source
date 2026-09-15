using System.Collections.Generic;
using UnityEngine;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using TheCircussyOne.Visuals;

namespace TheCircussyOne.Runtime
{
    public sealed class EnemyFactory
    {
        private readonly GameConfig config;
        private readonly DamageFeedbackVisualConfig feedbackConfig;
        private readonly EnemyCatalog enemyCatalog;
        private readonly ActorRegistry registry;
        private readonly EnemyView prefab;
        private readonly Transform root;
        private readonly Stack<EnemyView> pool = new();
        private EnemyDefinition fallbackDefinition;
        private int nextSpawnId;

        public EnemyFactory(GameConfig config, ActorRegistry registry, EnemyView prefab, Transform root)
            : this(config, null, registry, prefab, root)
        {
        }

        public EnemyFactory(GameConfig config, DamageFeedbackVisualConfig feedbackConfig, ActorRegistry registry, EnemyView prefab, Transform root)
            : this(config, feedbackConfig, null, registry, prefab, root)
        {
        }

        public EnemyFactory(GameConfig config, DamageFeedbackVisualConfig feedbackConfig, EnemyCatalog enemyCatalog, ActorRegistry registry, EnemyView prefab, Transform root)
        {
            this.config = config;
            this.feedbackConfig = feedbackConfig;
            this.enemyCatalog = enemyCatalog;
            this.registry = registry;
            this.prefab = prefab;
            this.root = root;
        }

        public int PooledCount => pool.Count;

        public int Prewarm(int targetPoolCount, int maxCreate = int.MaxValue)
        {
            if (prefab == null)
            {
                return 0;
            }

            int target = Mathf.Max(0, targetPoolCount);
            int limit = Mathf.Max(0, maxCreate);
            int created = 0;
            while (pool.Count < target && created < limit)
            {
                EnemyView view = Object.Instantiate(prefab, root);
                Release(view);
                created++;
            }

            return created;
        }

        public EnemyRuntime Spawn(Vector3 position, int level)
        {
            return Spawn(position, level, 0f);
        }

        public EnemyRuntime Spawn(Vector3 position, int level, float elapsedSeconds)
        {
            EnemyRuntime enemy = Create(null, position, level, elapsedSeconds);
            registry.Register(enemy);
            return enemy;
        }

        public EnemyRuntime Spawn(EnemyDefinition definition, Vector3 position, int level, float elapsedSeconds)
        {
            EnemyRuntime enemy = Create(definition, position, level, elapsedSeconds);
            registry.Register(enemy);
            return enemy;
        }

        public EnemyRuntime SpawnPending(Vector3 position, int level, float elapsedSeconds)
        {
            return Create(null, position, level, elapsedSeconds);
        }

        public EnemyRuntime SpawnPending(EnemyDefinition definition, Vector3 position, int level, float elapsedSeconds)
        {
            return Create(definition, position, level, elapsedSeconds);
        }

        public EnemyDefinition ResolveSpawnDefinition(bool useEncorePool)
        {
            return ResolveDefinition(useEncorePool);
        }

        public void ActivatePending(EnemyRuntime enemy)
        {
            if (enemy == null || registry.Contains(enemy))
            {
                return;
            }

            enemy.View?.ApplyLocomotionIdle(enemy.LocomotionProfile, feedbackConfig);
            registry.Register(enemy);
        }

        private EnemyRuntime Create(EnemyDefinition explicitDefinition, Vector3 position, int level, float elapsedSeconds)
        {
            EnemyDefinition definition = explicitDefinition != null ? explicitDefinition : ResolveDefinition();
            EnemyView view = GetView();
            view.Prepare(definition, feedbackConfig, position);
            var enemy = new EnemyRuntime(
                definition,
                view,
                DifficultyRules.EnemyHealth(
                    definition != null ? definition.baseHealth : config.enemyHealth,
                    level,
                    elapsedSeconds,
                    definition != null ? definition.healthGrowthPerMinute : config.enemyHealthGrowthPerMinute),
                DifficultyRules.EnemyMoveSpeed(
                    definition != null ? definition.baseMoveSpeed : config.enemyMoveSpeed,
                    level,
                    elapsedSeconds,
                    definition != null ? definition.moveSpeedGrowthPerMinute : config.enemyMoveSpeedGrowthPerMinute),
                definition != null ? definition.turnDegreesPerSecond : config.enemyTurnDegreesPerSecond,
                definition != null ? definition.contactDamage : config.enemyContactDamage,
                definition != null ? definition.behaviorType : EnemyBehaviorType.Chaser,
                definition != null ? definition.spawnWeight : 100f,
                definition != null ? definition.body : null,
                definition != null ? definition.climb : null,
                definition != null ? definition.stack : null,
                definition != null ? definition.locomotion : null,
                definition != null ? definition.xpBudget : XpGemDefinition.DefaultBlueXpAmount,
                definition != null ? definition.dropStyle : XpDropStyle.Compact,
                nextSpawnId++);
            return enemy;
        }

        public void Despawn(EnemyRuntime enemy, bool playDeath)
        {
            if (enemy == null)
            {
                return;
            }

            registry.Unregister(enemy);
            EnemyView view = enemy.View;
            if (view == null)
            {
                return;
            }

            if (playDeath)
            {
                view.PlayDeath(
                    config.deathShrinkSeconds,
                    feedbackConfig != null ? feedbackConfig.enemyDeathEase : TheCircussyOne.Rules.EaseSettings.InBack,
                    feedbackConfig,
                    () => Release(view));
            }
            else
            {
                Release(view);
            }
        }

        private EnemyView GetView()
        {
            if (pool.Count > 0)
            {
                return pool.Pop();
            }

            return Object.Instantiate(prefab, root);
        }

        private EnemyDefinition ResolveDefinition(bool useEncorePool = false)
        {
            if (enemyCatalog != null && enemyCatalog.TryPickSpawnEnemy(useEncorePool, Random.value, out EnemyDefinition picked))
            {
                return picked;
            }

            if (enemyCatalog != null && enemyCatalog.DefaultEnemy != null)
            {
                return enemyCatalog.DefaultEnemy;
            }

            fallbackDefinition ??= EnemyDefinition.CreateRuntimeFallback(config);
            fallbackDefinition.ApplyDefaultsFromGameConfig(config);
            return fallbackDefinition;
        }

        private void Release(EnemyView view)
        {
            view.Deactivate();
            view.transform.SetParent(root, false);
            pool.Push(view);
        }
    }
}
