using UnityEngine;
using TheCircussyOne.Runtime;

namespace TheCircussyOne.Rules
{
    public enum EnemyTerrainLocomotionDecision
    {
        Clear,
        Walkable,
        ClimbBlocked,
        Unreachable
    }

    public static class EnemyTerrainLocomotionRules
    {
        public const float WalkableStepHeight = 0.35f;
        public const float ClimbReleaseTolerance = 0.08f;
        public const float WalkableSurfaceNormalY = 0.45f;
        public const float EnvironmentClimbProbeHeight = 128f;

        private const float Epsilon = 0.0001f;

        public static bool IsValidHorizontalDirection(Vector3 direction)
        {
            direction.y = 0f;
            return direction.sqrMagnitude > Epsilon;
        }

        public static bool IsWalkableSurfaceNormal(Vector3 normal)
        {
            return normal.y >= WalkableSurfaceNormalY;
        }

        public static EnemyTerrainLocomotionDecision ClassifyBlockedSurface(
            bool hasTargetHeight,
            float currentHeight,
            float targetHeight,
            float maxClimbHeight,
            float walkableStepHeight = WalkableStepHeight)
        {
            if (!hasTargetHeight)
            {
                return EnemyTerrainLocomotionDecision.Unreachable;
            }

            float safeCurrent = Mathf.Max(0f, currentHeight);
            float safeTarget = Mathf.Max(0f, targetHeight);
            float safeStep = Mathf.Max(0f, walkableStepHeight);
            if (safeTarget <= safeCurrent + safeStep)
            {
                return EnemyTerrainLocomotionDecision.Walkable;
            }

            return EnemyTerrainLocomotionDecision.ClimbBlocked;
        }

        public static float ClimbDelta(float currentHeight, float targetHeight)
        {
            float safeCurrent = Mathf.Max(0f, currentHeight);
            float safeTarget = Mathf.Max(0f, targetHeight);
            return Mathf.Max(0f, safeTarget - safeCurrent);
        }

        public static bool IsWalkableHeight(float currentHeight, float targetHeight, float walkableStepHeight = WalkableStepHeight)
        {
            float safeCurrent = Mathf.Max(0f, currentHeight);
            float safeTarget = Mathf.Max(0f, targetHeight);
            float safeStep = Mathf.Max(0f, walkableStepHeight);
            return safeTarget <= safeCurrent + safeStep;
        }

        public static bool ShouldHoldHorizontalForClimb(EnemyVerticalMotorState state, float currentHeight)
        {
            return state != null
                && state.IsEnvironmentClimbing
                && currentHeight < state.HorizontalHoldUntilHeight - Epsilon;
        }

        public static bool HasReachedClimbReleaseHeight(EnemyVerticalMotorState state, float currentHeight)
        {
            return state != null
                && state.IsEnvironmentClimbing
                && currentHeight >= state.HorizontalHoldUntilHeight - Epsilon;
        }
    }
}
