using UnityEngine;

namespace TheCircussyOne.Rules
{
    public readonly struct OrbitWeaponFrame
    {
        public OrbitWeaponFrame(Vector3 position, float angleDegrees, float hitRadius)
            : this(position, angleDegrees, hitRadius, Quaternion.Euler(0f, Mathf.Repeat(angleDegrees, 360f), 0f))
        {
        }

        public OrbitWeaponFrame(Vector3 position, float angleDegrees, float hitRadius, Quaternion rotation)
        {
            Position = position;
            AngleDegrees = Mathf.Repeat(angleDegrees, 360f);
            HitRadius = Mathf.Max(0f, hitRadius);
            Rotation = rotation;
        }

        public Vector3 Position { get; }
        public float AngleDegrees { get; }
        public float HitRadius { get; }
        public Quaternion Rotation { get; }
    }

    public static class OrbitWeaponRules
    {
        public static float AdvanceAngle(float currentAngleDegrees, float degreesPerSecond, float deltaTime)
        {
            float next = currentAngleDegrees + Mathf.Max(0f, degreesPerSecond) * Mathf.Max(0f, deltaTime);
            return Mathf.Repeat(next, 360f);
        }

        public static OrbitWeaponFrame EvaluateFrame(
            Vector3 center,
            int index,
            int count,
            float baseAngleDegrees,
            float radius,
            float heightOffset,
            float hitRadius)
        {
            return EvaluateFrame(center, index, count, baseAngleDegrees, radius, heightOffset, hitRadius, Vector3.up);
        }

        public static OrbitWeaponFrame EvaluateFrame(
            Vector3 center,
            int index,
            int count,
            float baseAngleDegrees,
            float radius,
            float heightOffset,
            float hitRadius,
            Vector3 orbitUp)
        {
            int safeCount = Mathf.Max(1, count);
            int safeIndex = Mathf.Clamp(index, 0, safeCount - 1);
            float angle = baseAngleDegrees + safeIndex * (360f / safeCount);
            float radians = angle * Mathf.Deg2Rad;
            Vector3 safeUp = SafeUp(orbitUp);
            Quaternion tilt = Quaternion.FromToRotation(Vector3.up, safeUp);
            Vector3 flatOffset = new(Mathf.Cos(radians) * radius, Mathf.Max(0f, heightOffset), Mathf.Sin(radians) * radius);
            Quaternion rotation = tilt * Quaternion.Euler(0f, Mathf.Repeat(angle, 360f), 0f);
            return new OrbitWeaponFrame(center + tilt * flatOffset, angle, hitRadius, rotation);
        }

        private static Vector3 SafeUp(Vector3 orbitUp)
        {
            return orbitUp.sqrMagnitude > 0.000001f ? orbitUp.normalized : Vector3.up;
        }
    }
}
