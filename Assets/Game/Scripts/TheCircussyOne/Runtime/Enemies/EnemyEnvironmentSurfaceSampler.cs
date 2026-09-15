using UnityEngine;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;

namespace TheCircussyOne.Runtime
{
    public class EnemyEnvironmentSurfaceSampler
    {
        private readonly WorldPhysicsQuery worldQuery;

        public EnemyEnvironmentSurfaceSampler(int capacity = 8)
            : this(new WorldPhysicsQuery(capacity))
        {
        }

        public EnemyEnvironmentSurfaceSampler(WorldPhysicsQuery worldQuery)
        {
            this.worldQuery = worldQuery ?? new WorldPhysicsQuery(8);
        }

        public virtual bool TrySampleHeight(Vector3 position, float maxHeight, float probeDistance, int layerMask, out float height)
        {
            height = 0f;
            if (layerMask == 0)
            {
                return false;
            }

            float safeMaxHeight = Mathf.Max(0f, maxHeight);
            float safeProbeDistance = Mathf.Max(0.01f, probeDistance);
            Vector3 samplePosition = new(position.x, position.y, position.z);
            if (!worldQuery.TrySampleHighestSurfaceY(
                    samplePosition,
                    safeMaxHeight + safeProbeDistance,
                    safeProbeDistance,
                    layerMask,
                    out float sampledHeight,
                    QueryTriggerInteraction.Ignore))
            {
                return false;
            }

            height = Mathf.Max(0f, sampledHeight);
            return true;
        }

        public virtual bool TryFindHorizontalBlock(
            Vector3 position,
            Vector3 direction,
            float distance,
            EnemyBodyProfile bodyProfile,
            int layerMask,
            out RaycastHit hit)
        {
            hit = default;
            if (layerMask == 0 || distance <= 0f || !EnemyTerrainLocomotionRules.IsValidHorizontalDirection(direction))
            {
                return false;
            }

            Vector3 normalizedDirection = direction;
            normalizedDirection.y = 0f;
            normalizedDirection.Normalize();
            CapsulePoints(position, bodyProfile, out Vector3 pointA, out Vector3 pointB, out float radius);
            return worldQuery.TryHitCapsulePath(
                pointA,
                pointB,
                radius,
                normalizedDirection,
                Mathf.Max(0f, distance),
                layerMask,
                out hit,
                candidate => EnemyTerrainLocomotionRules.IsWalkableSurfaceNormal(candidate.normal),
                QueryTriggerInteraction.Ignore);
        }

        private static void CapsulePoints(Vector3 position, EnemyBodyProfile bodyProfile, out Vector3 pointA, out Vector3 pointB, out float radius)
        {
            float profileRadius = bodyProfile != null ? bodyProfile.movementBodyRadius : 0.52f;
            float profileHeight = bodyProfile != null ? bodyProfile.movementBodyHeight : 1.45f;
            Vector3 profileOffset = bodyProfile != null ? bodyProfile.movementBodyOffset : new Vector3(0f, 0.75f, 0f);
            radius = Mathf.Max(0.01f, profileRadius);
            float height = Mathf.Max(radius * 2f, profileHeight);
            Vector3 center = position + profileOffset;
            float halfSegment = Mathf.Max(0f, height * 0.5f - radius);
            pointA = center + Vector3.up * halfSegment;
            pointB = center - Vector3.up * halfSegment;
        }
    }
}
