using UnityEngine;

namespace TheCircussyOne.Runtime
{
    public enum GeneratedRampHitKind
    {
        Unknown,
        RampTop,
        RampSide,
        RampFoot,
        RampHighEnd
    }

    public sealed class GeneratedRampMarker : MonoBehaviour
    {
        [SerializeField] private Vector3 center;
        [SerializeField] private Vector3 direction = Vector3.forward;
        [SerializeField] private Vector3 side = Vector3.right;
        [SerializeField] private float halfWidth = 0.5f;
        [SerializeField] private float halfLength = 0.5f;
        [SerializeField] private float lowHeight;
        [SerializeField] private float highHeight;

        public Vector3 Center => center;
        public Vector3 Direction => direction;
        public Vector3 Side => side;
        public float HalfWidth => halfWidth;
        public float HalfLength => halfLength;
        public float LowHeight => lowHeight;
        public float HighHeight => highHeight;

        public void Initialize(
            Vector3 worldCenter,
            Vector3 rampDirection,
            Vector3 rampSide,
            float rampHalfWidth,
            float rampHalfLength,
            float rampLowHeight,
            float rampHighHeight)
        {
            center = worldCenter;
            direction = NormalizeHorizontal(rampDirection, Vector3.forward);
            side = NormalizeHorizontal(rampSide, new Vector3(-direction.z, 0f, direction.x));
            halfWidth = Mathf.Max(0.01f, rampHalfWidth);
            halfLength = Mathf.Max(0.01f, rampHalfLength);
            lowHeight = Mathf.Min(rampLowHeight, rampHighHeight);
            highHeight = Mathf.Max(rampLowHeight, rampHighHeight);
        }

        public void Project(Vector3 worldPoint, out float along, out float across)
        {
            Vector3 delta = worldPoint - center;
            along = Vector3.Dot(delta, direction);
            across = Vector3.Dot(delta, side);
        }

        public float HeightAt(Vector3 worldPoint)
        {
            Project(worldPoint, out float along, out _);
            float t = Mathf.InverseLerp(-halfLength, halfLength, Mathf.Clamp(along, -halfLength, halfLength));
            return Mathf.Lerp(lowHeight, highHeight, t);
        }

        private static Vector3 NormalizeHorizontal(Vector3 value, Vector3 fallback)
        {
            value.y = 0f;
            if (value.sqrMagnitude < 0.0001f)
            {
                value = fallback;
                value.y = 0f;
            }

            return value.sqrMagnitude > 0.0001f ? value.normalized : Vector3.forward;
        }
    }
}
