using UnityEngine;

namespace TheCircussyOne.Rules
{
    public static class EnemyHitboxAuthoringRules
    {
        public const float MinimumContactFrontMargin = 0.05f;

        public static float MovementBodyFrontReach(Vector3 movementBodyOffset, float movementBodyRadius)
        {
            return movementBodyOffset.z + Mathf.Max(0f, movementBodyRadius);
        }

        public static float ContactDamageFrontReach(Vector3 contactHitboxOffset, Vector3 contactHitboxSize)
        {
            return contactHitboxOffset.z + Mathf.Max(0f, contactHitboxSize.z) * 0.5f;
        }

        public static float ContactFrontProtrusionMargin(
            Vector3 contactHitboxOffset,
            Vector3 contactHitboxSize,
            Vector3 movementBodyOffset,
            float movementBodyRadius)
        {
            return ContactDamageFrontReach(contactHitboxOffset, contactHitboxSize)
                - MovementBodyFrontReach(movementBodyOffset, movementBodyRadius);
        }

        public static bool HasRequiredContactFrontMargin(
            Vector3 contactHitboxOffset,
            Vector3 contactHitboxSize,
            Vector3 movementBodyOffset,
            float movementBodyRadius,
            float requiredMargin = MinimumContactFrontMargin)
        {
            return ContactFrontProtrusionMargin(contactHitboxOffset, contactHitboxSize, movementBodyOffset, movementBodyRadius)
                >= Mathf.Max(0f, requiredMargin);
        }

        public static Vector3 FitContactFrontToMovementBody(
            Vector3 contactHitboxOffset,
            Vector3 contactHitboxSize,
            Vector3 movementBodyOffset,
            float movementBodyRadius,
            float requiredMargin = MinimumContactFrontMargin)
        {
            float requiredFront = MovementBodyFrontReach(movementBodyOffset, movementBodyRadius) + Mathf.Max(0f, requiredMargin);
            float halfContactDepth = Mathf.Max(0f, contactHitboxSize.z) * 0.5f;
            contactHitboxOffset.z = requiredFront - halfContactDepth;
            return contactHitboxOffset;
        }
    }
}
