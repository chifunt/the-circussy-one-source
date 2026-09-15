using UnityEngine;

namespace TheCircussyOne.Runtime
{
    public readonly struct WorldSurfaceSample
    {
        public WorldSurfaceSample(Vector3 position, Vector3 normal, bool foundSurface)
        {
            Position = position;
            Normal = normal.sqrMagnitude > 0.000001f ? normal.normalized : Vector3.up;
            FoundSurface = foundSurface;
        }

        public Vector3 Position { get; }
        public Vector3 Normal { get; }
        public bool FoundSurface { get; }
    }

    public sealed class WorldSurfaceResolver
    {
        private readonly WorldPhysicsQuery worldQuery;

        public WorldSurfaceResolver(WorldPhysicsQuery worldQuery = null)
        {
            this.worldQuery = worldQuery ?? new WorldPhysicsQuery();
        }

        public WorldSurfaceSample Resolve(Vector3 desiredPosition, float probeHeight, float probeDepth, int environmentMask)
        {
            if (environmentMask == 0)
            {
                return new WorldSurfaceSample(desiredPosition, Vector3.up, foundSurface: false);
            }

            if (!worldQuery.TrySampleHighestSurface(
                    desiredPosition,
                    probeHeight,
                    probeDepth,
                    environmentMask,
                    out RaycastHit hit))
            {
                return new WorldSurfaceSample(desiredPosition, Vector3.up, foundSurface: false);
            }

            return new WorldSurfaceSample(hit.point, hit.normal, foundSurface: true);
        }
    }
}
