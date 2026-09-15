using System;
using System.Collections.Generic;
using UnityEngine;
using TheCircussyOne.Runtime;

namespace TheCircussyOne.Rules
{
    public static class EnemyTargeting
    {
        public static EnemyRuntime FindNearestAliveInRange(IReadOnlyList<EnemyRuntime> enemies, Vector3 origin, float range)
        {
            return FindNearestAliveInRange(enemies, origin, range, null);
        }

        public static EnemyRuntime FindNearestAliveInRange(IReadOnlyList<EnemyRuntime> enemies, Vector3 origin, float range, Predicate<EnemyRuntime> predicate)
        {
            if (enemies == null)
            {
                return null;
            }

            EnemyRuntime best = null;
            float bestDistance = range * range;

            for (int i = 0; i < enemies.Count; i++)
            {
                EnemyRuntime enemy = enemies[i];
                if (enemy == null || enemy.IsDead || enemy.View == null || !enemy.View.IsActive)
                {
                    continue;
                }

                float distance = (enemy.Position - origin).sqrMagnitude;
                if (distance <= bestDistance && (predicate == null || predicate(enemy)))
                {
                    bestDistance = distance;
                    best = enemy;
                }
            }

            return best;
        }
    }
}
