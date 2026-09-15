using System.Collections.Generic;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using UnityEngine;

namespace TheCircussyOne.Runtime
{
    public sealed class WorldPropPlacementService
    {
        private const int DefaultAttempts = 9;

        private readonly WorldPhysicsQuery worldQuery;
        private readonly int defaultEnvironmentMask;
        private readonly List<PlacedFootprint> placedFootprints = new();

        public WorldPropPlacementService(WorldPhysicsQuery worldQuery = null, int environmentMask = 0)
        {
            this.worldQuery = worldQuery ?? new WorldPhysicsQuery();
            defaultEnvironmentMask = environmentMask != 0 ? environmentMask : GameLayers.EnvironmentMaskExcludingGameplay;
        }

        public void Clear()
        {
            placedFootprints.Clear();
        }

        public bool TryPlace(
            Vector3 desiredPosition,
            WorldPropPlacementProfile profile,
            out WorldPropPlacementResult result,
            int attemptCount = DefaultAttempts)
        {
            return TryPlace(
                desiredPosition,
                profile,
                defaultEnvironmentMask,
                out result,
                attemptCount);
        }

        public bool TryPlace(
            Vector3 desiredPosition,
            WorldPropPlacementProfile profile,
            int environmentMask,
            out WorldPropPlacementResult result,
            int attemptCount = DefaultAttempts)
        {
            int attempts = Mathf.Max(1, attemptCount);
            for (int i = 0; i < attempts; i++)
            {
                Vector3 candidate = desiredPosition + WorldPropPlacementRules.CandidateOffset(i, profile.footprintRadius);
                WorldSurfaceSample sample = ResolveSurface(candidate, profile, environmentMask);
                if (sample.FoundSurface && !WorldPropPlacementRules.IsSlopeAllowed(sample.Normal, profile.maxPlacementSlope))
                {
                    continue;
                }

                Vector3 rootPosition = sample.FoundSurface
                    ? WorldPropPlacementRules.GroundedRootPosition(sample.Position, sample.Normal, profile)
                    : WorldPropPlacementRules.SafeFallbackPosition(candidate, profile);
                Quaternion rootRotation = sample.FoundSurface
                    ? WorldPropPlacementRules.SurfaceRotation(sample.Normal)
                    : Quaternion.identity;

                if (!IsFootprintClear(rootPosition, profile))
                {
                    continue;
                }

                if (!IsEnvironmentClear(rootPosition, rootRotation, profile, environmentMask, sample.SurfaceCollider))
                {
                    continue;
                }

                RegisterFootprint(rootPosition, profile.footprintRadius);
                result = new WorldPropPlacementResult(rootPosition, sample.Normal, rootRotation, sample.FoundSurface, i);
                return true;
            }

            Vector3 fallback = WorldPropPlacementRules.SafeFallbackPosition(desiredPosition, profile);
            RegisterFootprint(fallback, profile.footprintRadius);
            result = new WorldPropPlacementResult(fallback, Vector3.up, Quaternion.identity, foundSurface: false, attempts);
            return false;
        }

        public void RegisterFootprint(Vector3 position, float radius)
        {
            placedFootprints.Add(new PlacedFootprint(position, Mathf.Max(0f, radius)));
        }

        private WorldSurfaceSample ResolveSurface(Vector3 candidate, WorldPropPlacementProfile profile, int environmentMask)
        {
            float probeHeight = Mathf.Max(4f, profile.bodyColliderSize.y + 4f);
            float probeDepth = Mathf.Max(10f, profile.bodyColliderSize.y + 10f);
            if (!worldQuery.TrySampleHighestSurface(
                    candidate,
                    probeHeight,
                    probeDepth,
                    environmentMask,
                    out RaycastHit hit,
                    QueryTriggerInteraction.Ignore))
            {
                return new WorldSurfaceSample(candidate, Vector3.up, foundSurface: false, surfaceCollider: null);
            }

            return new WorldSurfaceSample(hit.point, hit.normal, foundSurface: true, hit.collider);
        }

        private bool IsFootprintClear(Vector3 rootPosition, WorldPropPlacementProfile profile)
        {
            for (int i = 0; i < placedFootprints.Count; i++)
            {
                PlacedFootprint placed = placedFootprints[i];
                if (WorldPropPlacementRules.FootprintsOverlap(
                        rootPosition,
                        profile.footprintRadius,
                        placed.Position,
                        placed.Radius))
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsEnvironmentClear(
            Vector3 rootPosition,
            Quaternion rootRotation,
            WorldPropPlacementProfile profile,
            int environmentMask,
            Collider surfaceCollider)
        {
            if ((profile.bodyColliderSize.x <= 0f || profile.bodyColliderSize.y <= 0f || profile.bodyColliderSize.z <= 0f)
                && profile.footprintRadius <= 0f)
            {
                return true;
            }

            float footprintDiameter = Mathf.Max(0f, profile.footprintRadius * 2f);
            Vector3 size = profile.bodyColliderSize;
            size.x = Mathf.Max(size.x, footprintDiameter);
            size.z = Mathf.Max(size.z, footprintDiameter);
            size.y = Mathf.Max(size.y, 0.35f);
            Vector3 halfExtents = new(
                Mathf.Max(0.01f, size.x * 0.5f),
                Mathf.Max(0.01f, size.y * 0.5f),
                Mathf.Max(0.01f, size.z * 0.5f));
            Vector3 center = rootPosition + rootRotation * profile.bodyColliderCenter;
            return worldQuery.IsBoxClear(
                center,
                halfExtents,
                rootRotation,
                environmentMask,
                collider => collider == surfaceCollider,
                QueryTriggerInteraction.Ignore);
        }

        private readonly struct PlacedFootprint
        {
            public PlacedFootprint(Vector3 position, float radius)
            {
                Position = position;
                Radius = radius;
            }

            public Vector3 Position { get; }
            public float Radius { get; }
        }

        private readonly struct WorldSurfaceSample
        {
            public WorldSurfaceSample(Vector3 position, Vector3 normal, bool foundSurface, Collider surfaceCollider)
            {
                Position = position;
                Normal = normal.sqrMagnitude > 0.000001f ? normal.normalized : Vector3.up;
                FoundSurface = foundSurface;
                SurfaceCollider = surfaceCollider;
            }

            public Vector3 Position { get; }
            public Vector3 Normal { get; }
            public bool FoundSurface { get; }
            public Collider SurfaceCollider { get; }
        }
    }
}
