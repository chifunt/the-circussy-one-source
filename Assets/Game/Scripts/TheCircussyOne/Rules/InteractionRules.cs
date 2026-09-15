using System.Collections.Generic;
using TheCircussyOne.Runtime;
using UnityEngine;

namespace TheCircussyOne.Rules
{
    public static class InteractionRules
    {
        public static IInteractable SelectBest(
            IReadOnlyList<IInteractable> candidates,
            Vector3 playerPosition,
            float radius,
            IInteractable lockedTarget = null)
        {
            if (lockedTarget != null && IsValidCandidate(lockedTarget, playerPosition, radius))
            {
                return lockedTarget;
            }

            IInteractable best = null;
            float bestDistanceSquared = float.PositiveInfinity;
            if (candidates == null)
            {
                return null;
            }

            float radiusSquared = Mathf.Max(0f, radius) * Mathf.Max(0f, radius);
            for (int i = 0; i < candidates.Count; i++)
            {
                IInteractable candidate = candidates[i];
                if (candidate == null || !candidate.IsInteractionAvailable)
                {
                    continue;
                }

                float distanceSquared = (candidate.InteractionPosition - playerPosition).sqrMagnitude;
                if (distanceSquared > radiusSquared)
                {
                    continue;
                }

                if (best == null || distanceSquared < bestDistanceSquared)
                {
                    best = candidate;
                    bestDistanceSquared = distanceSquared;
                }
            }

            return best;
        }

        public static bool IsValidCandidate(IInteractable target, Vector3 playerPosition, float radius)
        {
            if (target == null || !target.IsInteractionAvailable)
            {
                return false;
            }

            float safeRadius = Mathf.Max(0f, radius);
            return (target.InteractionPosition - playerPosition).sqrMagnitude <= safeRadius * safeRadius;
        }
    }
}
