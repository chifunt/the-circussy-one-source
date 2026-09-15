using UnityEngine;
using TheCircussyOne.Runtime;

namespace TheCircussyOne.Rules
{
    public static class DamageRules
    {
        public static bool ApplyDamage(EnemyRuntime enemy, int amount)
        {
            if (enemy == null || enemy.IsDead || amount <= 0)
            {
                return false;
            }

            enemy.SetHealth(Mathf.Max(0, enemy.CurrentHealth - amount));
            return enemy.IsDead;
        }
    }
}
