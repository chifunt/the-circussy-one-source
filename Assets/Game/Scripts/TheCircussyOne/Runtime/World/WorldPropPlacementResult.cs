using UnityEngine;

namespace TheCircussyOne.Runtime
{
    public readonly struct WorldPropPlacementResult
    {
        public WorldPropPlacementResult(Vector3 position, Vector3 groundNormal, Quaternion rotation, bool foundSurface, int attempt)
        {
            Position = position;
            GroundNormal = groundNormal.sqrMagnitude > 0.000001f ? groundNormal.normalized : Vector3.up;
            Rotation = rotation;
            FoundSurface = foundSurface;
            Attempt = attempt;
        }

        public Vector3 Position { get; }
        public Vector3 GroundNormal { get; }
        public Quaternion Rotation { get; }
        public bool FoundSurface { get; }
        public int Attempt { get; }
    }
}
