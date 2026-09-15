using TheCircussyOne.Content;
using UnityEngine;

namespace TheCircussyOne.Rules
{
    public static class WorldPropPlacementRules
    {
        public static bool IsSlopeAllowed(Vector3 normal, float maxSlopeDegrees)
        {
            Vector3 safeNormal = normal.sqrMagnitude > 0.000001f ? normal.normalized : Vector3.up;
            float angle = Vector3.Angle(safeNormal, Vector3.up);
            return angle <= Mathf.Clamp(maxSlopeDegrees, 0f, 89f) + 0.001f;
        }

        public static Vector3 GroundedRootPosition(Vector3 surfacePosition, Vector3 surfaceNormal, WorldPropPlacementProfile profile)
        {
            Vector3 safeNormal = surfaceNormal.sqrMagnitude > 0.000001f ? surfaceNormal.normalized : Vector3.up;
            return surfacePosition + safeNormal * Mathf.Max(0f, profile.RootHeightOffset);
        }

        public static Vector3 SafeFallbackPosition(Vector3 desiredPosition, WorldPropPlacementProfile profile)
        {
            float minimumY = Mathf.Max(0f, profile.RootHeightOffset);
            return new Vector3(desiredPosition.x, Mathf.Max(desiredPosition.y, minimumY), desiredPosition.z);
        }

        public static Quaternion SurfaceRotation(Vector3 normal)
        {
            Vector3 safeNormal = normal.sqrMagnitude > 0.000001f ? normal.normalized : Vector3.up;
            return Quaternion.FromToRotation(Vector3.up, safeNormal);
        }

        public static Vector3 CandidateOffset(int attempt, float footprintRadius)
        {
            if (attempt <= 0)
            {
                return Vector3.zero;
            }

            int ringIndex = attempt - 1;
            const int samplesPerRing = 8;
            int ring = ringIndex / samplesPerRing + 1;
            int sample = ringIndex % samplesPerRing;
            float angle = sample * Mathf.PI * 2f / samplesPerRing;
            float radius = Mathf.Max(0.35f, footprintRadius * 2.1f) * ring;
            return new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
        }

        public static bool FootprintsOverlap(Vector3 first, float firstRadius, Vector3 second, float secondRadius)
        {
            float combined = Mathf.Max(0f, firstRadius) + Mathf.Max(0f, secondRadius);
            Vector2 firstHorizontal = new(first.x, first.z);
            Vector2 secondHorizontal = new(second.x, second.z);
            return (firstHorizontal - secondHorizontal).sqrMagnitude < combined * combined;
        }

        public static bool HasValidBlockingBody(WorldPropPlacementProfile profile)
        {
            return profile.HasBlockingBody;
        }
    }
}
