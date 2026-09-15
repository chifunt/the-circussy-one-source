using UnityEngine;
using TheCircussyOne.Content;

namespace TheCircussyOne.Rules
{
    public static class EnemySupportStackRules
    {
        public const float SupportedSeparationMultiplier = 0.2f;
        public const float SupportCandidateSeparationMultiplier = 0.45f;

        private const float Epsilon = 0.0001f;

        public static bool CanUseSupportStack(EnemyStackProfile stack)
        {
            return stack != null
                && stack.policy == EnemyStackPolicy.SupportBased
                && stack.canClimbEnemies;
        }

        public static bool CanUseSupportStack(EnemyStackProfile stack, EnemyLocomotionProfile locomotion)
        {
            return CanUseSupportStack(stack)
                && (locomotion == null || locomotion.mode != EnemyLocomotionMode.Floating);
        }

        public static bool CanBeSupport(EnemyStackProfile stack)
        {
            return stack != null && stack.canBeStackedOn;
        }

        public static bool CanBeSupport(EnemyStackProfile stack, EnemyLocomotionProfile locomotion)
        {
            return CanBeSupport(stack)
                && (locomotion == null || locomotion.mode != EnemyLocomotionMode.Floating);
        }

        public static bool HasStableSupportOrder(int climberSpawnId, int supportSpawnId)
        {
            return supportSpawnId < climberSpawnId;
        }

        public static bool IsWithinSupportReach(
            Vector3 climberPosition,
            float climberRadius,
            Vector3 supportPosition,
            float supportRadius)
        {
            Vector3 offset = climberPosition - supportPosition;
            offset.y = 0f;
            float reach = Mathf.Max(0f, climberRadius) + Mathf.Max(0f, supportRadius);
            return reach > Epsilon && offset.sqrMagnitude <= reach * reach;
        }

        public static float SeparationMultiplier(
            bool stackingEnabled,
            EnemyStackProfile firstStack,
            int firstSpawnId,
            int firstLayer,
            float firstRadius,
            Vector3 firstPosition,
            bool firstIsSupportClimbing,
            EnemyStackProfile secondStack,
            int secondSpawnId,
            int secondLayer,
            float secondRadius,
            Vector3 secondPosition,
            bool secondIsSupportClimbing)
        {
            return SeparationMultiplier(
                stackingEnabled,
                firstStack,
                null,
                firstSpawnId,
                firstLayer,
                firstRadius,
                firstPosition,
                firstIsSupportClimbing,
                secondStack,
                null,
                secondSpawnId,
                secondLayer,
                secondRadius,
                secondPosition,
                secondIsSupportClimbing);
        }

        public static float SeparationMultiplier(
            bool stackingEnabled,
            EnemyStackProfile firstStack,
            EnemyLocomotionProfile firstLocomotion,
            int firstSpawnId,
            int firstLayer,
            float firstRadius,
            Vector3 firstPosition,
            bool firstIsSupportClimbing,
            EnemyStackProfile secondStack,
            EnemyLocomotionProfile secondLocomotion,
            int secondSpawnId,
            int secondLayer,
            float secondRadius,
            Vector3 secondPosition,
            bool secondIsSupportClimbing)
        {
            if (!stackingEnabled || firstSpawnId == secondSpawnId)
            {
                return 1f;
            }

            bool firstCanClimbSecond = CanUseSupportStack(firstStack, firstLocomotion)
                && CanBeSupport(secondStack, secondLocomotion)
                && HasStableSupportOrder(firstSpawnId, secondSpawnId);
            bool secondCanClimbFirst = CanUseSupportStack(secondStack, secondLocomotion)
                && CanBeSupport(firstStack, firstLocomotion)
                && HasStableSupportOrder(secondSpawnId, firstSpawnId);
            if (!firstCanClimbSecond && !secondCanClimbFirst)
            {
                return 1f;
            }

            if (!IsWithinSupportReach(firstPosition, firstRadius, secondPosition, secondRadius))
            {
                return 1f;
            }

            float multiplier = firstLayer != secondLayer
                ? SupportedSeparationMultiplier
                : SupportCandidateSeparationMultiplier;
            if (firstIsSupportClimbing && firstCanClimbSecond)
            {
                multiplier = Mathf.Min(multiplier, SupportClimbSeparationMultiplier(firstStack));
            }

            if (secondIsSupportClimbing && secondCanClimbFirst)
            {
                multiplier = Mathf.Min(multiplier, SupportClimbSeparationMultiplier(secondStack));
            }

            return multiplier;
        }

        public static float SeparationMultiplier(
            bool stackingEnabled,
            EnemyStackProfile firstStack,
            int firstSpawnId,
            int firstLayer,
            float firstRadius,
            Vector3 firstPosition,
            EnemyStackProfile secondStack,
            int secondSpawnId,
            int secondLayer,
            float secondRadius,
            Vector3 secondPosition)
        {
            return SeparationMultiplier(
                stackingEnabled,
                firstStack,
                null,
                firstSpawnId,
                firstLayer,
                firstRadius,
                firstPosition,
                false,
                secondStack,
                null,
                secondSpawnId,
                secondLayer,
                secondRadius,
                secondPosition,
                false);
        }

        private static float SupportClimbSeparationMultiplier(EnemyStackProfile stack)
        {
            return Mathf.Clamp01(stack != null ? stack.supportClimbSeparationMultiplier : 0.15f);
        }
    }
}
