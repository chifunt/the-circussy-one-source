using UnityEngine;

namespace TheCircussyOne.Runtime
{
    public static class GeneratedRampHitClassifier
    {
        private const float TopNormalY = 0.45f;
        private const float SideNormalDot = 0.65f;
        private const float EndNormalDot = 0.65f;

        public static GeneratedRampHitKind Classify(Collider collider, Vector3 hitPoint, Vector3 hitNormal, out GeneratedRampMarker marker)
        {
            marker = collider != null ? collider.GetComponentInParent<GeneratedRampMarker>() : null;
            return Classify(marker, hitPoint, hitNormal);
        }

        public static GeneratedRampHitKind Classify(GeneratedRampMarker marker, Vector3 hitPoint, Vector3 hitNormal)
        {
            if (marker == null)
            {
                return GeneratedRampHitKind.Unknown;
            }

            marker.Project(hitPoint, out float along, out float across);
            float lengthTolerance = Mathf.Max(0.08f, marker.HalfLength * 0.04f);
            float widthTolerance = Mathf.Max(0.08f, marker.HalfWidth * 0.12f);
            bool withinLength = along >= -marker.HalfLength - lengthTolerance
                && along <= marker.HalfLength + lengthTolerance;
            bool withinWidth = Mathf.Abs(across) <= marker.HalfWidth + widthTolerance;

            if (!withinLength || !withinWidth)
            {
                return GeneratedRampHitKind.Unknown;
            }

            Vector3 normal = hitNormal.sqrMagnitude > 0.0001f ? hitNormal.normalized : Vector3.zero;
            float sideNormal = Mathf.Abs(Vector3.Dot(normal, marker.Side));
            float endNormal = Vector3.Dot(normal, marker.Direction);
            bool nearSide = Mathf.Abs(Mathf.Abs(across) - marker.HalfWidth) <= widthTolerance;
            bool nearFoot = along <= -marker.HalfLength + lengthTolerance;
            bool nearHighEnd = along >= marker.HalfLength - lengthTolerance;

            if (nearSide && sideNormal >= SideNormalDot)
            {
                return GeneratedRampHitKind.RampSide;
            }

            if (nearFoot && endNormal <= -EndNormalDot)
            {
                return GeneratedRampHitKind.RampFoot;
            }

            if (nearHighEnd && endNormal >= EndNormalDot)
            {
                return GeneratedRampHitKind.RampHighEnd;
            }

            if (normal.y >= TopNormalY)
            {
                return GeneratedRampHitKind.RampTop;
            }

            if (nearSide)
            {
                return GeneratedRampHitKind.RampSide;
            }

            if (nearFoot)
            {
                return GeneratedRampHitKind.RampFoot;
            }

            if (nearHighEnd)
            {
                return GeneratedRampHitKind.RampHighEnd;
            }

            return GeneratedRampHitKind.Unknown;
        }
    }
}
