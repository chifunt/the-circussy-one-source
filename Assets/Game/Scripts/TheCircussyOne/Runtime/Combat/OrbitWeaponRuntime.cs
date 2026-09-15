using System.Collections.Generic;
using TheCircussyOne.Visuals;

namespace TheCircussyOne.Runtime
{
    public sealed class OrbitWeaponRuntime
    {
        private readonly Dictionary<EnemyRuntime, float> _nextDirectHitTimes = new();
        private readonly Dictionary<EnemyRuntime, float> _nextAreaHitTimes = new();

        public OrbitWeaponRuntime(WeaponRuntime weapon)
        {
            Weapon = weapon;
        }

        public WeaponRuntime Weapon { get; }
        public float AngleDegrees { get; set; }
        public List<OrbitWeaponView> Views { get; } = new();
        public FireHoopAreaTelegraphView AreaTelegraph { get; set; }

        public bool CanHit(EnemyRuntime enemy, float time)
        {
            return enemy != null && (!_nextDirectHitTimes.TryGetValue(enemy, out float nextTime) || time >= nextTime);
        }

        public void MarkHit(EnemyRuntime enemy, float nextHitTime)
        {
            if (enemy != null)
            {
                _nextDirectHitTimes[enemy] = nextHitTime;
            }
        }

        public bool CanAreaHit(EnemyRuntime enemy, float time)
        {
            return enemy != null && (!_nextAreaHitTimes.TryGetValue(enemy, out float nextTime) || time >= nextTime);
        }

        public void MarkAreaHit(EnemyRuntime enemy, float nextHitTime)
        {
            if (enemy != null)
            {
                _nextAreaHitTimes[enemy] = nextHitTime;
            }
        }

        public void PruneHitCooldowns()
        {
            PruneHitCooldowns(_nextDirectHitTimes);
            PruneHitCooldowns(_nextAreaHitTimes);
        }

        private static void PruneHitCooldowns(Dictionary<EnemyRuntime, float> cooldowns)
        {
            s_pruneBuffer.Clear();
            foreach (KeyValuePair<EnemyRuntime, float> pair in cooldowns)
            {
                EnemyRuntime enemy = pair.Key;
                if (enemy == null || enemy.IsDead || enemy.View == null || !enemy.View.IsActive)
                {
                    s_pruneBuffer.Add(enemy);
                }
            }

            for (int i = 0; i < s_pruneBuffer.Count; i++)
            {
                cooldowns.Remove(s_pruneBuffer[i]);
            }
        }

        private static readonly List<EnemyRuntime> s_pruneBuffer = new();
    }
}
