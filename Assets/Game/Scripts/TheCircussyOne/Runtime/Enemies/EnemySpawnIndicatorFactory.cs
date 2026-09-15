using System.Collections.Generic;
using UnityEngine;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Visuals;

namespace TheCircussyOne.Runtime
{
    public sealed class EnemySpawnIndicatorFactory
    {
        private readonly EnemySpawnVisualConfig config;
        private readonly EnemySpawnIndicatorView prefab;
        private readonly Transform root;
        private readonly Stack<EnemySpawnIndicatorView> pool = new();
        private readonly List<EnemySpawnIndicatorRuntime> active = new();

        public EnemySpawnIndicatorFactory(EnemySpawnVisualConfig config, EnemySpawnIndicatorView prefab, Transform root)
        {
            this.config = config;
            this.prefab = prefab;
            this.root = root;
            Prewarm();
        }

        public IReadOnlyList<EnemySpawnIndicatorRuntime> Active => active;
        public int PooledCount => pool.Count;

        public EnemySpawnIndicatorRuntime Spawn(Vector3 position, int level, float enemyElapsedSeconds, float startedAt)
        {
            return Spawn(position, Vector3.up, level, enemyElapsedSeconds, startedAt);
        }

        public EnemySpawnIndicatorRuntime Spawn(Vector3 position, Vector3 surfaceNormal, int level, float enemyElapsedSeconds, float startedAt, EnemyDefinition enemyDefinition = null)
        {
            EnemySpawnIndicatorView view = GetView();
            view.Prepare(config, position, surfaceNormal);
            var runtime = new EnemySpawnIndicatorRuntime(
                view,
                position,
                surfaceNormal,
                level,
                enemyElapsedSeconds,
                startedAt,
                config.warningSeconds,
                config.emergeSeconds,
                enemyDefinition);
            active.Add(runtime);
            return runtime;
        }

        public void Despawn(EnemySpawnIndicatorRuntime runtime)
        {
            if (runtime == null)
            {
                return;
            }

            active.Remove(runtime);
            EnemySpawnIndicatorView view = runtime.View;
            if (view == null)
            {
                return;
            }

            Release(view);
        }

        private void Prewarm()
        {
            if (config == null || prefab == null || root == null)
            {
                return;
            }

            for (int i = 0; i < config.prewarmCount; i++)
            {
                EnemySpawnIndicatorView view = Object.Instantiate(prefab, root);
                view.Deactivate();
                pool.Push(view);
            }
        }

        private EnemySpawnIndicatorView GetView()
        {
            if (pool.Count > 0)
            {
                return pool.Pop();
            }

            return Object.Instantiate(prefab, root);
        }

        private void Release(EnemySpawnIndicatorView view)
        {
            view.Deactivate();
            view.transform.SetParent(root, false);
            pool.Push(view);
        }
    }
}
