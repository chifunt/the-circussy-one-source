using System.Collections.Generic;
using UnityEngine;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;
using TheCircussyOne.Visuals;

namespace TheCircussyOne.Runtime
{
    public sealed class DamageNumberFactory
    {
        private readonly DamageFeedbackVisualConfig config;
        private readonly DamageNumberView prefab;
        private readonly Transform root;
        private readonly Stack<DamageNumberView> pool = new();
        private readonly List<DamageNumberRuntime> active = new();

        public DamageNumberFactory(DamageFeedbackVisualConfig config, DamageNumberView prefab, Transform root)
        {
            this.config = config;
            this.prefab = prefab;
            this.root = root;
        }

        public IReadOnlyList<DamageNumberRuntime> Active => active;
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
                DamageNumberView view = Object.Instantiate(prefab, root);
                view.Deactivate();
                if (root != null)
                {
                    view.transform.SetParent(root, false);
                }

                pool.Push(view);
                created++;
            }

            return created;
        }

        public DamageNumberRuntime Spawn(DamageNumberRequest request, float spawnedAt, int seed)
        {
            DamageNumberView view = GetView();
            float spawnRadius = Mathf.Max(config.spawnJitterRadius, config.sameEnemyNumberSpreadRadius);
            Vector3 startPosition = DamageNumberRules.SpawnPosition(
                request.AnchorPosition,
                config.spawnHeightBias,
                spawnRadius,
                seed);
            Vector3 driftDirection = DamageNumberRules.LateralDriftDirection(seed);
            view.Prepare(config, request.Amount, startPosition);
            var runtime = new DamageNumberRuntime(
                view,
                startPosition,
                driftDirection,
                request.Amount,
                spawnedAt,
                config.lifetime,
                request.MergeKey,
                spawnedAt + Mathf.Max(0f, config.damageNumberMergeWindowSeconds));
            active.Add(runtime);
            return runtime;
        }

        public bool TryMerge(DamageNumberRequest request, float time)
        {
            if (!request.HasMergeKey)
            {
                return false;
            }

            for (int i = active.Count - 1; i >= 0; i--)
            {
                DamageNumberRuntime runtime = active[i];
                if (runtime == null
                    || runtime.View == null
                    || !runtime.View.IsActive
                    || runtime.IsExpired(time)
                    || !runtime.CanMerge(request.MergeKey, time))
                {
                    continue;
                }

                runtime.Merge(request.Amount, time, config.lifetime, config.damageNumberMergeWindowSeconds);
                return true;
            }

            return false;
        }

        public void Despawn(DamageNumberRuntime runtime)
        {
            if (runtime == null)
            {
                return;
            }

            active.Remove(runtime);
            DamageNumberView view = runtime.View;
            if (view == null)
            {
                return;
            }

            view.Deactivate();
            view.transform.SetParent(root, false);
            pool.Push(view);
        }

        private DamageNumberView GetView()
        {
            if (pool.Count > 0)
            {
                return pool.Pop();
            }

            return Object.Instantiate(prefab, root);
        }
    }
}
