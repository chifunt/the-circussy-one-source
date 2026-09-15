using System.Collections.Generic;
using UnityEngine;
using TheCircussyOne.Content;
using TheCircussyOne.Config;
using TheCircussyOne.Stats;
using TheCircussyOne.Visuals;

namespace TheCircussyOne.Runtime
{
    public sealed class ProjectileFactory
    {
        private readonly DamageFeedbackVisualConfig feedbackConfig;
        private readonly ActorRegistry registry;
        private readonly IGameTime time;
        private readonly ProjectileView prefab;
        private readonly Transform root;
        private readonly RunStats stats;
        private readonly Stack<ProjectileView> pool = new();

        public ProjectileFactory(ActorRegistry registry, IGameTime time, ProjectileView prefab, Transform root)
            : this(null, registry, time, prefab, root)
        {
        }

        public ProjectileFactory(DamageFeedbackVisualConfig feedbackConfig, ActorRegistry registry, IGameTime time, ProjectileView prefab, Transform root, RunStats stats = null)
        {
            this.feedbackConfig = feedbackConfig;
            this.registry = registry;
            this.time = time;
            this.prefab = prefab;
            this.root = root;
            this.stats = stats;
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
                ProjectileView view = Object.Instantiate(prefab, root);
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

        public ProjectileRuntime Spawn(WeaponDefinition weapon, Vector3 position, Vector3 direction)
        {
            return Spawn(weapon, null, ProjectileSpawnFrame.Direct(position, direction));
        }

        public ProjectileRuntime Spawn(WeaponDefinition weapon, ProjectileSpawnFrame spawnFrame)
        {
            return Spawn(weapon, null, spawnFrame);
        }

        public ProjectileRuntime Spawn(WeaponRuntime weapon, Vector3 position, Vector3 direction)
        {
            return Spawn(weapon, ProjectileSpawnFrame.Direct(position, direction));
        }

        public ProjectileRuntime Spawn(WeaponRuntime weapon, ProjectileSpawnFrame spawnFrame)
        {
            if (weapon == null)
            {
                throw new System.ArgumentNullException(nameof(weapon));
            }

            return Spawn(weapon.Definition, weapon.Modifiers, spawnFrame);
        }

        private ProjectileRuntime Spawn(WeaponDefinition weapon, IReadOnlyList<StatModifier> weaponModifiers, ProjectileSpawnFrame spawnFrame)
        {
            if (weapon == null)
            {
                throw new System.ArgumentNullException(nameof(weapon));
            }

            weapon.EnsureWorkflowDefaults();
            ProjectileView view = GetView();
            float projectileSizeMultiplier = WeaponStatRules.ProjectileVisualScaleMultiplier(stats, weaponModifiers);
            view.Prepare(feedbackConfig, weapon, spawnFrame.Position, spawnFrame.Direction, projectileSizeMultiplier);
            var projectile = new ProjectileRuntime(
                view,
                weapon,
                spawnFrame,
                WeaponStatRules.Damage(weapon, stats, weaponModifiers),
                WeaponStatRules.ProjectileSpeed(weapon, stats, weaponModifiers),
                WeaponStatRules.ProjectileHitRadius(weapon, stats, weaponModifiers),
                weapon.projectileHitMask,
                weapon.projectileBlockMask,
                weapon.projectileBlockRadius,
                time.Time + WeaponStatRules.ProjectileLifetime(weapon, stats, weaponModifiers),
                WeaponStatRules.BounceCount(weapon, stats, weaponModifiers),
                weapon.explosiveEnabled,
                WeaponStatRules.SplashRadius(weapon, stats, weaponModifiers),
                weapon.secondarySplashDamageMultiplier,
                weapon.explodeOnWorldImpact,
                weapon.explosionVfxEnabled,
                WeaponStatRules.ChainCount(weapon, stats, weaponModifiers),
                weapon.chainSearchRadius,
                weapon.chainDamageMultiplier,
                weapon.chainRequiresLineOfSight);
            registry.Register(projectile);
            return projectile;
        }

        public void Despawn(ProjectileRuntime projectile)
        {
            if (projectile == null)
            {
                return;
            }

            registry.Unregister(projectile);
            ProjectileView view = projectile.View;
            if (view == null)
            {
                return;
            }

            view.Deactivate();
            view.transform.SetParent(root, false);
            pool.Push(view);
        }

        private ProjectileView GetView()
        {
            if (pool.Count > 0)
            {
                return pool.Pop();
            }

            return Object.Instantiate(prefab, root);
        }
    }
}
