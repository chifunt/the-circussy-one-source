using UnityEngine;
using TheCircussyOne.Content;
using TheCircussyOne.Runtime;

namespace TheCircussyOne.Rules
{
    public static class EnemySupportClimbRules
    {
        public const float SupportReleaseTolerance = 0.08f;
        public const float SupportReachPadding = 0.08f;

        private const float Epsilon = 0.0001f;

        public static int TargetLayer(int supportLayer, int maxLayers)
        {
            int layer = Mathf.Max(0, supportLayer) + 1;
            return layer <= Mathf.Max(0, maxLayers) ? layer : 0;
        }

        public static float MovementBodyTopHeight(Vector3 rootPosition, EnemyBodyProfile profile)
        {
            CapsuleMetrics(profile, out _, out float height, out Vector3 offset);
            return rootPosition.y + offset.y + height * 0.5f;
        }

        public static float MovementBodyBottomOffset(EnemyBodyProfile profile)
        {
            CapsuleMetrics(profile, out _, out float height, out Vector3 offset);
            return offset.y - height * 0.5f;
        }

        public static float TargetRootHeight(
            Vector3 supportRootPosition,
            EnemyBodyProfile supportProfile,
            EnemyBodyProfile climberProfile,
            float heightMultiplier)
        {
            float rawTarget = MovementBodyTopHeight(supportRootPosition, supportProfile) - MovementBodyBottomOffset(climberProfile);
            float supportBase = Mathf.Max(0f, supportRootPosition.y);
            return Mathf.Max(0f, supportBase + (rawTarget - supportBase) * Mathf.Max(0f, heightMultiplier));
        }

        public static bool ShouldHoldHorizontalForSupport(EnemyVerticalMotorState state, float currentHeight)
        {
            return state != null
                && state.IsSupportClimbing
                && currentHeight < state.SupportHorizontalHoldUntilHeight - Epsilon;
        }

        public static bool HasReachedSupportReleaseHeight(EnemyVerticalMotorState state, float currentHeight)
        {
            return state != null
                && state.IsSupportClimbing
                && currentHeight >= state.SupportHorizontalHoldUntilHeight - Epsilon;
        }

        public static bool IsMovementBlockedBySupport(
            Vector3 start,
            Vector3 direction,
            float distance,
            float climberRadius,
            Vector3 supportPosition,
            float supportRadius,
            out float pathDistance)
        {
            pathDistance = 0f;
            direction.y = 0f;
            if (distance <= 0f || direction.sqrMagnitude <= Epsilon)
            {
                return false;
            }

            direction.Normalize();
            Vector3 startFlat = Flatten(start);
            Vector3 endFlat = startFlat + direction * Mathf.Max(0f, distance);
            Vector3 supportFlat = Flatten(supportPosition);
            Vector3 segment = endFlat - startFlat;
            float segmentLengthSquared = segment.sqrMagnitude;
            float t = segmentLengthSquared <= Epsilon
                ? 0f
                : Mathf.Clamp01(Vector3.Dot(supportFlat - startFlat, segment) / segmentLengthSquared);
            Vector3 closest = startFlat + segment * t;
            float combinedRadius = Mathf.Max(0f, climberRadius) + Mathf.Max(0f, supportRadius) + SupportReachPadding;
            pathDistance = Mathf.Sqrt(Mathf.Max(0f, (closest - startFlat).sqrMagnitude));
            return combinedRadius > Epsilon && (supportFlat - closest).sqrMagnitude <= combinedRadius * combinedRadius;
        }

        public static bool IsWithinSupportReach(
            Vector3 climberPosition,
            float climberRadius,
            Vector3 supportPosition,
            float supportRadius)
        {
            Vector3 offset = Flatten(climberPosition) - Flatten(supportPosition);
            float combinedRadius = Mathf.Max(0f, climberRadius) + Mathf.Max(0f, supportRadius) + SupportReachPadding;
            return combinedRadius > Epsilon && offset.sqrMagnitude <= combinedRadius * combinedRadius;
        }

        private static void CapsuleMetrics(EnemyBodyProfile profile, out float radius, out float height, out Vector3 offset)
        {
            radius = Mathf.Max(0.01f, profile != null ? profile.movementBodyRadius : 0.52f);
            height = Mathf.Max(radius * 2f, profile != null ? profile.movementBodyHeight : 1.45f);
            offset = profile != null ? profile.movementBodyOffset : new Vector3(0f, 0.75f, 0f);
        }

        private static Vector3 Flatten(Vector3 value)
        {
            value.y = 0f;
            return value;
        }
    }
}
