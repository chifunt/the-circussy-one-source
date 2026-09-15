using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;
using TheCircussyOne.Visuals;

namespace TheCircussyOne.Runtime
{
    public sealed class ProjectileExplosionSystem : ITickable, IProjectileExplosionSpawner
    {
        private readonly VfxVisualConfig config;
        private readonly ProjectileExplosionView prefab;
        private readonly Transform root;
        private readonly IGameTime time;
        private readonly Stack<ProjectileExplosionView> pool = new();
        private readonly List<ExplosionRuntime> active = new();

        public ProjectileExplosionSystem(VfxVisualConfig config, ProjectileExplosionView prefab, Transform root, IGameTime time)
        {
            this.config = config;
            this.prefab = prefab;
            this.root = root;
            this.time = time;
        }

        public int ActiveCount => active.Count;
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
                ProjectileExplosionView view = UnityEngine.Object.Instantiate(prefab, root);
                view.ConfigureDefaults(config);
                Release(view);
                created++;
            }

            return created;
        }

        public void ForEachActiveView(Action<ProjectileExplosionView> action)
        {
            if (action == null)
            {
                return;
            }

            for (int i = active.Count - 1; i >= 0; i--)
            {
                ProjectileExplosionView view = active[i].View;
                if (view != null && view.IsActive)
                {
                    action(view);
                }
            }
        }

        public void Show(Vector3 position, float radius, Vector3 direction)
        {
            if (config == null || !config.enabled || prefab == null)
            {
                return;
            }

            if (!config.projectileExplosionOverlayEnabled && (config.projectileExplosion == null || !config.projectileExplosion.enabled))
            {
                return;
            }

            Quaternion rotation = direction.sqrMagnitude > 0.001f
                ? Quaternion.LookRotation(direction.normalized, Vector3.up)
                : Quaternion.identity;
            ProjectileExplosionView view = GetView();
            view.PlayAt(position, rotation, config);
            view.ApplyFrame(ProjectileExplosionVisualRules.Evaluate(0f, radius, config), config);
            active.Add(new ExplosionRuntime(view, radius, time != null ? time.Time : 0f));
        }

        public void Tick()
        {
            if (config == null)
            {
                return;
            }

            float now = time != null ? time.Time : 0f;
            float lifetime = ProjectileExplosionVisualRules.Lifetime(config);
            for (int i = active.Count - 1; i >= 0; i--)
            {
                ExplosionRuntime runtime = active[i];
                if (runtime.View == null || !runtime.View.IsActive)
                {
                    active.RemoveAt(i);
                    continue;
                }

                float elapsed = Mathf.Max(0f, now - runtime.StartedAt);
                if (lifetime <= 0f || elapsed >= lifetime)
                {
                    Release(runtime.View);
                    active.RemoveAt(i);
                    continue;
                }

                runtime.View.ApplyFrame(ProjectileExplosionVisualRules.Evaluate(elapsed, runtime.Radius, config), config);
            }
        }

        private ProjectileExplosionView GetView()
        {
            if (pool.Count > 0)
            {
                return pool.Pop();
            }

            return UnityEngine.Object.Instantiate(prefab, root);
        }

        private void Release(ProjectileExplosionView view)
        {
            view.Deactivate();
            if (root != null)
            {
                view.transform.SetParent(root, false);
            }

            pool.Push(view);
        }

        private readonly struct ExplosionRuntime
        {
            public ExplosionRuntime(ProjectileExplosionView view, float radius, float startedAt)
            {
                View = view;
                Radius = radius;
                StartedAt = startedAt;
            }

            public ProjectileExplosionView View { get; }
            public float Radius { get; }
            public float StartedAt { get; }
        }
    }
}
