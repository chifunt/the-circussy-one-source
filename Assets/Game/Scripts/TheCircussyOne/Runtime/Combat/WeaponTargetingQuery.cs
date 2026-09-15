using UnityEngine;
using TheCircussyOne.Content;
using TheCircussyOne.Visuals;

namespace TheCircussyOne.Runtime
{
    public sealed class WeaponTargetingQuery
    {
        private const int HitBufferSize = 32;

        private readonly WorldPhysicsQuery _worldQuery;

        public WeaponTargetingQuery(int hitBufferSize = HitBufferSize)
            : this(new WorldPhysicsQuery(hitBufferSize))
        {
        }

        public WeaponTargetingQuery(WorldPhysicsQuery worldQuery)
        {
            _worldQuery = worldQuery ?? new WorldPhysicsQuery(HitBufferSize);
        }

        public bool HasLineOfSight(WeaponDefinition weapon, Vector3 origin, EnemyRuntime target)
        {
            if (weapon == null || !weapon.requireLineOfSight)
            {
                return true;
            }

            if (target == null || target.View == null || !target.View.IsActive)
            {
                return false;
            }

            int mask = weapon.lineOfSightMask.value;
            if (mask == 0)
            {
                return true;
            }

            Vector3 targetPosition = target.AimPosition;
            Vector3 direction = targetPosition - origin;
            float distance = direction.magnitude;
            if (distance <= 0.001f)
            {
                return true;
            }

            float radius = Mathf.Max(0.001f, weapon.projectileBlockRadius);
            return _worldQuery.HasClearSpherePath(
                origin,
                targetPosition,
                radius,
                mask,
                collider => BelongsToTarget(collider, target),
                QueryTriggerInteraction.Ignore);
        }

        private static bool BelongsToTarget(Collider collider, EnemyRuntime target)
        {
            EnemyView view = collider.GetComponentInParent<EnemyView>();
            return view != null && ReferenceEquals(view, target.View);
        }
    }
}
