using UnityEngine;

namespace TheCircussyOne.Runtime
{
    public sealed class PickupSurfaceSampler
    {
        private const int MaxHits = 16;

        private readonly WorldPhysicsQuery worldQuery;

        public PickupSurfaceSampler(WorldPhysicsQuery worldQuery = null)
        {
            this.worldQuery = worldQuery ?? new WorldPhysicsQuery(MaxHits, MaxHits);
        }

        public bool TrySampleGround(Vector3 position, float probeHeight, float probeDepth, int environmentMask, out float surfaceY)
        {
            surfaceY = 0f;

            if (environmentMask == 0)
            {
                surfaceY = Mathf.Max(0f, position.y);
                return true;
            }

            return worldQuery.TrySampleHighestSurfaceY(position, probeHeight, probeDepth, environmentMask, out surfaceY);
        }

        public bool IsLandingClear(Vector3 landingPosition, float collisionRadius, int environmentMask)
        {
            if (environmentMask == 0 || collisionRadius <= 0f)
            {
                return true;
            }

            float safeRadius = Mathf.Max(0.01f, collisionRadius);
            Vector3 center = landingPosition + Vector3.up * (safeRadius + 0.01f);
            return worldQuery.IsSphereClear(center, safeRadius, environmentMask);
        }

        public bool HasClearPath(Vector3 from, Vector3 to, float collisionRadius, int environmentMask)
        {
            if (environmentMask == 0)
            {
                return true;
            }

            Vector3 delta = to - from;
            float distance = delta.magnitude;
            if (distance <= 0.001f)
            {
                return true;
            }

            float safeRadius = Mathf.Max(0.01f, collisionRadius);
            Vector3 castOffset = Vector3.up * Mathf.Max(safeRadius * 2f, 0.05f);
            Vector3 origin = from + castOffset;
            Vector3 destination = to + castOffset;
            Vector3 direction = (destination - origin).normalized;
            float adjustedDistance = Vector3.Distance(origin, destination);
            return worldQuery.HasClearSpherePath(origin, origin + direction * adjustedDistance, safeRadius, environmentMask);
        }
    }
}
