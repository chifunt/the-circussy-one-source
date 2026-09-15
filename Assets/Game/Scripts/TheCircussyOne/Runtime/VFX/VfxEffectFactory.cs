using System.Collections.Generic;
using UnityEngine;
using TheCircussyOne.Visuals;

namespace TheCircussyOne.Runtime
{
    public sealed class VfxEffectFactory
    {
        private readonly Transform root;
        private readonly Dictionary<VfxEffectId, ParticleEffectView> prefabs = new();
        private readonly Dictionary<VfxEffectId, Stack<ParticleEffectView>> pools = new();
        private readonly List<VfxEffectRuntime> active = new();

        public VfxEffectFactory(
            Transform root,
            ParticleEffectView projectileMuzzlePuff,
            ParticleEffectView enemyHitSparks,
            ParticleEffectView enemyDeathBurst,
            ParticleEffectView xpPickupCollectPop,
            ParticleEffectView playerDamageBurst,
            ParticleEffectView levelUpBurst,
            ParticleEffectView playerJumpTakeoff = null,
            ParticleEffectView playerJumpLand = null,
            ParticleEffectView projectileExplosion = null,
            ParticleEffectView projectileBounceBurst = null,
            ParticleEffectView knifeHitSparks = null)
        {
            this.root = root;
            RegisterPrefab(VfxEffectId.ProjectileMuzzlePuff, projectileMuzzlePuff);
            RegisterPrefab(VfxEffectId.EnemyHitSparks, enemyHitSparks);
            RegisterPrefab(VfxEffectId.EnemyDeathBurst, enemyDeathBurst);
            RegisterPrefab(VfxEffectId.XpPickupCollectPop, xpPickupCollectPop);
            RegisterPrefab(VfxEffectId.PlayerDamageBurst, playerDamageBurst);
            RegisterPrefab(VfxEffectId.LevelUpBurst, levelUpBurst);
            RegisterPrefab(VfxEffectId.PlayerJumpTakeoff, playerJumpTakeoff);
            RegisterPrefab(VfxEffectId.PlayerJumpLand, playerJumpLand);
            RegisterPrefab(VfxEffectId.ProjectileExplosion, projectileExplosion);
            RegisterPrefab(VfxEffectId.ProjectileBounceBurst, projectileBounceBurst);
            RegisterPrefab(VfxEffectId.KnifeHitSparks, knifeHitSparks);
        }

        public IReadOnlyList<VfxEffectRuntime> Active => active;

        public int PooledCount(VfxEffectId effectId)
        {
            return pools.TryGetValue(effectId, out Stack<ParticleEffectView> pool) ? pool.Count : 0;
        }

        public int PrewarmPerRegisteredEffect(int targetPoolCount, int maxCreate = int.MaxValue)
        {
            int target = Mathf.Max(0, targetPoolCount);
            int limit = Mathf.Max(0, maxCreate);
            if (target == 0 || limit == 0)
            {
                return 0;
            }

            int created = 0;
            foreach (KeyValuePair<VfxEffectId, ParticleEffectView> pair in prefabs)
            {
                if (pair.Value == null)
                {
                    continue;
                }

                if (!pools.TryGetValue(pair.Key, out Stack<ParticleEffectView> pool))
                {
                    pool = new Stack<ParticleEffectView>();
                    pools[pair.Key] = pool;
                }

                while (pool.Count < target && created < limit)
                {
                    ParticleEffectView view = Object.Instantiate(pair.Value, root);
                    view.Configure(pair.Key);
                    view.Deactivate();
                    if (root != null)
                    {
                        view.transform.SetParent(root, false);
                    }

                    pool.Push(view);
                    created++;
                }

                if (created >= limit)
                {
                    break;
                }
            }

            return created;
        }

        public VfxEffectRuntime Spawn(VfxEffectId effectId, Vector3 position, Quaternion rotation)
        {
            if (!prefabs.TryGetValue(effectId, out ParticleEffectView prefab) || prefab == null)
            {
                return null;
            }

            ParticleEffectView view = GetView(effectId, prefab);
            view.PlayAt(position, rotation);
            var runtime = new VfxEffectRuntime(effectId, view);
            active.Add(runtime);
            return runtime;
        }

        public void Release(VfxEffectRuntime runtime)
        {
            if (runtime == null)
            {
                return;
            }

            active.Remove(runtime);
            if (runtime.View == null)
            {
                return;
            }

            runtime.View.Deactivate();
            runtime.View.transform.SetParent(root, false);
            if (!pools.TryGetValue(runtime.EffectId, out Stack<ParticleEffectView> pool))
            {
                pool = new Stack<ParticleEffectView>();
                pools[runtime.EffectId] = pool;
            }

            pool.Push(runtime.View);
        }

        private void RegisterPrefab(VfxEffectId effectId, ParticleEffectView prefab)
        {
            if (prefab == null)
            {
                return;
            }

            prefabs[effectId] = prefab;
            if (!pools.ContainsKey(effectId))
            {
                pools[effectId] = new Stack<ParticleEffectView>();
            }
        }

        private ParticleEffectView GetView(VfxEffectId effectId, ParticleEffectView prefab)
        {
            if (pools.TryGetValue(effectId, out Stack<ParticleEffectView> pool) && pool.Count > 0)
            {
                return pool.Pop();
            }

            ParticleEffectView view = Object.Instantiate(prefab, root);
            view.Configure(effectId);
            return view;
        }
    }
}
