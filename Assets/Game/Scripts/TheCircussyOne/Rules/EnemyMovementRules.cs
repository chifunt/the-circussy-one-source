using UnityEngine;

namespace TheCircussyOne.Rules
{
    public static class EnemyMovementRules
    {
        public const float MinimumMoveDistance = 0.05f;

        public static Vector3 HorizontalOffsetToTarget(Vector3 from, Vector3 target)
        {
            Vector3 offset = target - from;
            offset.y = 0f;
            return offset;
        }

        public static bool TryGetDirection(Vector3 offset, out Vector3 direction, out float distance)
        {
            distance = offset.magnitude;
            if (distance <= MinimumMoveDistance)
            {
                direction = Vector3.zero;
                return false;
            }

            direction = offset / distance;
            return true;
        }

        public static bool ShouldChase(float distance, float stopDistance)
        {
            return distance > Mathf.Max(MinimumMoveDistance, stopDistance);
        }

        public static Vector3 SeparationAwayFrom(Vector3 self, Vector3 other, float radius, Vector3 fallbackDirection)
        {
            if (radius <= MinimumMoveDistance)
            {
                return Vector3.zero;
            }

            Vector3 offset = HorizontalOffsetToTarget(other, self);
            float distance = offset.magnitude;
            if (distance >= radius)
            {
                return Vector3.zero;
            }

            Vector3 direction;
            if (distance <= MinimumMoveDistance)
            {
                fallbackDirection.y = 0f;
                direction = fallbackDirection.sqrMagnitude > MinimumMoveDistance * MinimumMoveDistance
                    ? fallbackDirection.normalized
                    : Vector3.right;
            }
            else
            {
                direction = offset / distance;
            }

            float strength = (radius - distance) / radius;
            return direction * strength;
        }

        public static float AllowedMoveDistance(float currentDistance, float stopDistance, float desiredMoveDistance, float towardTargetDot)
        {
            if (desiredMoveDistance <= 0f || towardTargetDot <= 0f)
            {
                return Mathf.Max(0f, desiredMoveDistance);
            }

            float remaining = Mathf.Max(0f, currentDistance - stopDistance);
            return Mathf.Min(desiredMoveDistance, remaining);
        }

        public static Quaternion RotateTowardsHorizontal(Quaternion currentRotation, Vector3 desiredDirection, float degreesPerSecond, float deltaTime)
        {
            if (!TryGetDirection(desiredDirection, out Vector3 direction, out _))
            {
                return currentRotation;
            }

            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            float maxDegrees = Mathf.Max(0f, degreesPerSecond) * Mathf.Max(0f, deltaTime);
            return Quaternion.RotateTowards(currentRotation, targetRotation, maxDegrees);
        }

        public static Vector3 HorizontalForward(Quaternion rotation, Vector3 fallbackDirection)
        {
            Vector3 forward = rotation * Vector3.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude > MinimumMoveDistance * MinimumMoveDistance)
            {
                return forward.normalized;
            }

            fallbackDirection.y = 0f;
            return fallbackDirection.sqrMagnitude > MinimumMoveDistance * MinimumMoveDistance
                ? fallbackDirection.normalized
                : Vector3.forward;
        }

        public static float MovementAlignmentMultiplier(Vector3 currentForward, Vector3 desiredDirection)
        {
            if (!TryGetDirection(currentForward, out Vector3 forward, out _) ||
                !TryGetDirection(desiredDirection, out Vector3 desired, out _))
            {
                return 0f;
            }

            return Mathf.Clamp01(Vector3.Dot(forward, desired));
        }
    }
}
