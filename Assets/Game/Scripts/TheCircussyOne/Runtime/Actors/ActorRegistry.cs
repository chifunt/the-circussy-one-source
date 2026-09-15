using System.Collections.Generic;
using TheCircussyOne.Visuals;

namespace TheCircussyOne.Runtime
{
    public sealed class ActorRegistry
    {
        private readonly List<EnemyRuntime> enemies = new();
        private readonly List<ProjectileRuntime> projectiles = new();
        private readonly List<PickupRuntime> pickups = new();
        private readonly Dictionary<EnemyView, EnemyRuntime> enemiesByView = new();

        public IReadOnlyList<EnemyRuntime> Enemies => enemies;
        public IReadOnlyList<ProjectileRuntime> Projectiles => projectiles;
        public IReadOnlyList<PickupRuntime> Pickups => pickups;

        public void Register(EnemyRuntime enemy)
        {
            enemies.Add(enemy);
            if (enemy.View != null)
            {
                enemiesByView[enemy.View] = enemy;
            }
        }

        public void Register(ProjectileRuntime projectile)
        {
            projectiles.Add(projectile);
        }

        public void Register(PickupRuntime pickup)
        {
            pickups.Add(pickup);
        }

        public void Unregister(EnemyRuntime enemy)
        {
            enemies.Remove(enemy);
            if (enemy != null && enemy.View != null)
            {
                enemiesByView.Remove(enemy.View);
            }
        }

        public void Unregister(ProjectileRuntime projectile)
        {
            projectiles.Remove(projectile);
        }

        public void Unregister(PickupRuntime pickup)
        {
            pickups.Remove(pickup);
        }

        public bool TryGetEnemy(EnemyView view, out EnemyRuntime enemy)
        {
            if (view == null)
            {
                enemy = null;
                return false;
            }

            return enemiesByView.TryGetValue(view, out enemy);
        }

        public bool Contains(EnemyRuntime enemy)
        {
            return enemy != null && enemies.Contains(enemy);
        }

        public void RemoveInactive()
        {
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                EnemyRuntime enemy = enemies[i];
                if (enemy?.View == null || !enemy.View.IsActive)
                {
                    Unregister(enemy);
                }
            }

            projectiles.RemoveAll(static projectile => projectile?.View == null || !projectile.View.IsActive);
            pickups.RemoveAll(static pickup => pickup?.View == null || !pickup.View.IsActive);
        }
    }
}
