using UnityEngine;

namespace TheCircussyOne.Rules
{
    public static class WeaponSpawnRules
    {
        public static Vector3 AimDirection(Vector3 origin, Vector3 target, Vector3 fallbackDirection)
        {
            Vector3 direction = target - origin;
            if (direction.sqrMagnitude >= 0.001f)
            {
                return direction.normalized;
            }

            if (fallbackDirection.sqrMagnitude >= 0.001f)
            {
                return fallbackDirection.normalized;
            }

            return Vector3.forward;
        }

        public static Vector3 MuzzlePosition(Vector3 playerPosition, Vector3 fireDirection, float height, float forwardOffset)
        {
            Vector3 horizontalDirection = fireDirection;
            horizontalDirection.y = 0f;
            if (horizontalDirection.sqrMagnitude < 0.001f)
            {
                horizontalDirection = Vector3.forward;
            }

            return playerPosition
                + Vector3.up * Mathf.Max(0.01f, height)
                + horizontalDirection.normalized * Mathf.Max(0f, forwardOffset);
        }

        public static Vector3 SpreadDirection(Vector3 centerDirection, int projectileIndex, int projectileCount, float totalSpreadAngleDegrees)
        {
            Vector3 safeDirection = centerDirection.sqrMagnitude >= 0.001f ? centerDirection.normalized : Vector3.forward;
            int safeCount = Mathf.Max(1, projectileCount);
            if (safeCount <= 1 || totalSpreadAngleDegrees <= 0f)
            {
                return safeDirection;
            }

            int safeIndex = Mathf.Clamp(projectileIndex, 0, safeCount - 1);
            float t = safeCount == 1 ? 0f : (float)safeIndex / (safeCount - 1);
            float yawOffset = (t - 0.5f) * Mathf.Max(0f, totalSpreadAngleDegrees);
            return (Quaternion.AngleAxis(yawOffset, Vector3.up) * safeDirection).normalized;
        }

        public static Vector3 AimErrorDirection(Vector3 intendedDirection, float maxAimErrorDegrees, int seed)
        {
            Vector3 safeDirection = intendedDirection.sqrMagnitude >= 0.001f ? intendedDirection.normalized : Vector3.forward;
            float maxError = Mathf.Max(0f, maxAimErrorDegrees);
            if (maxError <= 0f)
            {
                return safeDirection;
            }

            float yawOffset = DeterministicSigned(seed) * maxError;
            return (Quaternion.AngleAxis(yawOffset, Vector3.up) * safeDirection).normalized;
        }

        public static int AimErrorSeed(string weaponId, int fireSequence, int projectileIndex)
        {
            return AimErrorSeed(weaponId, fireSequence, projectileIndex, runSeed: 0);
        }

        public static int AimErrorSeed(string weaponId, int fireSequence, int projectileIndex, int runSeed)
        {
            return DeterministicSeed.Combine(
                runSeed,
                DeterministicSeed.StringHash(weaponId),
                fireSequence,
                projectileIndex);
        }

        private static float DeterministicSigned(int seed)
        {
            return DeterministicSeed.ToSignedFloat(seed);
        }
    }
}
