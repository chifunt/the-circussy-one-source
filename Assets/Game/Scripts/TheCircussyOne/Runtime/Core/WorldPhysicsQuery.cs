using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheCircussyOne.Runtime
{
    public sealed class WorldPhysicsQuery
    {
        private const int DefaultHitCapacity = 32;
        private const int DefaultOverlapCapacity = 64;
        private const float MinimumCastDistance = 0.0001f;
        private const float MinimumRadius = 0.001f;

        private readonly RaycastHit[] _hits;
        private readonly Collider[] _overlaps;

        public WorldPhysicsQuery(int hitCapacity = DefaultHitCapacity, int overlapCapacity = DefaultOverlapCapacity)
        {
            _hits = new RaycastHit[Mathf.Max(1, hitCapacity)];
            _overlaps = new Collider[Mathf.Max(1, overlapCapacity)];
        }

        public bool TryComputePenetration(
            Collider first,
            Collider second,
            out Vector3 direction,
            out float distance)
        {
            direction = Vector3.zero;
            distance = 0f;
            if (!IsUsableCollider(first) || !IsUsableCollider(second))
            {
                return false;
            }

            return Physics.ComputePenetration(
                first,
                first.transform.position,
                first.transform.rotation,
                second,
                second.transform.position,
                second.transform.rotation,
                out direction,
                out distance);
        }

        public bool TryComputeHorizontalPenetration(
            Collider first,
            Collider second,
            out Vector3 horizontalDirection,
            out float distance)
        {
            if (!TryComputePenetration(first, second, out Vector3 direction, out distance))
            {
                horizontalDirection = Vector3.zero;
                return false;
            }

            horizontalDirection = new Vector3(direction.x, 0f, direction.z);
            return true;
        }

        public bool HasClearSpherePath(
            Vector3 from,
            Vector3 to,
            float radius,
            int mask,
            Predicate<Collider> ignore = null,
            QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore)
        {
            return !TryHitSpherePath(from, to, radius, mask, out _, ignore, triggerInteraction);
        }

        public bool TryHitSpherePath(
            Vector3 from,
            Vector3 to,
            float radius,
            int mask,
            out RaycastHit hit,
            Predicate<Collider> ignore = null,
            QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore)
        {
            hit = default;
            if (mask == 0)
            {
                return false;
            }

            Vector3 delta = to - from;
            float distance = delta.magnitude;
            if (distance <= MinimumCastDistance)
            {
                return false;
            }

            int count = Physics.SphereCastNonAlloc(
                from,
                Mathf.Max(MinimumRadius, radius),
                delta / distance,
                _hits,
                distance,
                mask,
                triggerInteraction);

            float closestDistance = float.PositiveInfinity;
            bool found = false;
            for (int i = 0; i < count; i++)
            {
                RaycastHit candidate = _hits[i];
                Collider collider = candidate.collider;
                if (collider == null || (ignore != null && ignore(collider)))
                {
                    continue;
                }

                if (candidate.distance >= closestDistance)
                {
                    continue;
                }

                closestDistance = candidate.distance;
                hit = candidate;
                found = true;
            }

            return found;
        }

        public bool TryHitCapsulePath(
            Vector3 pointA,
            Vector3 pointB,
            float radius,
            Vector3 direction,
            float distance,
            int mask,
            out RaycastHit hit,
            Predicate<RaycastHit> ignore = null,
            QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore)
        {
            hit = default;
            if (mask == 0 || distance <= MinimumCastDistance)
            {
                return false;
            }

            Vector3 normalizedDirection = direction;
            if (normalizedDirection.sqrMagnitude <= MinimumCastDistance * MinimumCastDistance)
            {
                return false;
            }

            normalizedDirection.Normalize();
            int count = Physics.CapsuleCastNonAlloc(
                pointA,
                pointB,
                Mathf.Max(MinimumRadius, radius),
                normalizedDirection,
                _hits,
                distance,
                mask,
                triggerInteraction);

            float closestDistance = float.PositiveInfinity;
            bool found = false;
            for (int i = 0; i < count; i++)
            {
                RaycastHit candidate = _hits[i];
                if (candidate.collider == null || (ignore != null && ignore(candidate)))
                {
                    continue;
                }

                if (candidate.distance >= closestDistance)
                {
                    continue;
                }

                closestDistance = candidate.distance;
                hit = candidate;
                found = true;
            }

            return found;
        }

        public bool TrySampleHighestSurfaceY(
            Vector3 position,
            float probeHeight,
            float probeDepth,
            int mask,
            out float surfaceY,
            QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore)
        {
            surfaceY = 0f;
            if (mask == 0)
            {
                return false;
            }

            float safeProbeHeight = Mathf.Max(0.01f, probeHeight);
            float safeProbeDepth = Mathf.Max(0.01f, probeDepth);
            Vector3 origin = position + Vector3.up * safeProbeHeight;
            float distance = safeProbeHeight + safeProbeDepth;
            int count = Physics.RaycastNonAlloc(
                origin,
                Vector3.down,
                _hits,
                distance,
                mask,
                triggerInteraction);

            if (count <= 0)
            {
                return false;
            }

            float highest = float.NegativeInfinity;
            for (int i = 0; i < count; i++)
            {
                highest = Mathf.Max(highest, _hits[i].point.y);
            }

            surfaceY = highest;
            return true;
        }

        public bool TrySampleHighestSurface(
            Vector3 position,
            float probeHeight,
            float probeDepth,
            int mask,
            out RaycastHit surfaceHit,
            QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore)
        {
            surfaceHit = default;
            if (mask == 0)
            {
                return false;
            }

            float safeProbeHeight = Mathf.Max(0.01f, probeHeight);
            float safeProbeDepth = Mathf.Max(0.01f, probeDepth);
            Vector3 origin = position + Vector3.up * safeProbeHeight;
            float distance = safeProbeHeight + safeProbeDepth;
            int count = Physics.RaycastNonAlloc(
                origin,
                Vector3.down,
                _hits,
                distance,
                mask,
                triggerInteraction);

            if (count <= 0)
            {
                return false;
            }

            bool found = false;
            float highest = float.NegativeInfinity;
            for (int i = 0; i < count; i++)
            {
                RaycastHit candidate = _hits[i];
                if (candidate.collider == null || candidate.point.y < highest)
                {
                    continue;
                }

                highest = candidate.point.y;
                surfaceHit = candidate;
                found = true;
            }

            return found;
        }

        public bool IsSphereClear(
            Vector3 center,
            float radius,
            int mask,
            QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore)
        {
            if (mask == 0 || radius <= 0f)
            {
                return true;
            }

            int count = Physics.OverlapSphereNonAlloc(
                center,
                Mathf.Max(MinimumRadius, radius),
                _overlaps,
                mask,
                triggerInteraction);

            return count == 0;
        }

        public bool IsCapsuleClear(
            Vector3 pointA,
            Vector3 pointB,
            float radius,
            int mask,
            Predicate<Collider> ignore = null,
            QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore)
        {
            if (mask == 0 || radius <= 0f)
            {
                return true;
            }

            int count = Physics.OverlapCapsuleNonAlloc(
                pointA,
                pointB,
                Mathf.Max(MinimumRadius, radius),
                _overlaps,
                mask,
                triggerInteraction);

            for (int i = 0; i < count; i++)
            {
                Collider collider = _overlaps[i];
                if (collider == null || (ignore != null && ignore(collider)))
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        public bool IsBoxClear(
            Vector3 center,
            Vector3 halfExtents,
            Quaternion orientation,
            int mask,
            Predicate<Collider> ignore = null,
            QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore)
        {
            if (mask == 0)
            {
                return true;
            }

            Vector3 safeHalfExtents = new(
                Mathf.Max(MinimumRadius, halfExtents.x),
                Mathf.Max(MinimumRadius, halfExtents.y),
                Mathf.Max(MinimumRadius, halfExtents.z));
            int count = Physics.OverlapBoxNonAlloc(
                center,
                safeHalfExtents,
                _overlaps,
                orientation,
                mask,
                triggerInteraction);

            for (int i = 0; i < count; i++)
            {
                Collider collider = _overlaps[i];
                if (collider == null || (ignore != null && ignore(collider)))
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        public int CollectSphereOverlaps(
            Vector3 center,
            float radius,
            int mask,
            List<Collider> results,
            QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore)
        {
            results?.Clear();
            if (results == null || mask == 0 || radius <= 0f)
            {
                return 0;
            }

            int count = Physics.OverlapSphereNonAlloc(
                center,
                Mathf.Max(MinimumRadius, radius),
                _overlaps,
                mask,
                triggerInteraction);

            for (int i = 0; i < count; i++)
            {
                Collider collider = _overlaps[i];
                if (collider != null)
                {
                    results.Add(collider);
                }
            }

            return results.Count;
        }

        private static bool IsUsableCollider(Collider collider)
        {
            return collider != null && collider.enabled && collider.gameObject.activeInHierarchy;
        }
    }
}
