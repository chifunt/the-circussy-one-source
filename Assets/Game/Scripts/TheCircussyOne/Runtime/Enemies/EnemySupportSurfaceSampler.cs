using System.Collections.Generic;
using UnityEngine;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;

namespace TheCircussyOne.Runtime
{
    public readonly struct EnemySupportSurfaceHit
    {
        public EnemySupportSurfaceHit(EnemyRuntime support, float targetHeight, int targetLayer, float distance)
        {
            Support = support;
            TargetHeight = targetHeight;
            TargetLayer = targetLayer;
            Distance = distance;
        }

        public EnemyRuntime Support { get; }
        public float TargetHeight { get; }
        public int TargetLayer { get; }
        public float Distance { get; }
    }

    public class EnemySupportSurfaceSampler
    {
        private readonly List<int> nearbyEnemyIndices = new(32);

        public virtual bool TryFindSupportBlock(
            EnemyRuntime climber,
            Vector3 direction,
            float distance,
            IReadOnlyList<EnemyRuntime> enemies,
            EnemySpatialIndex spatialIndex,
            out EnemySupportSurfaceHit hit)
        {
            hit = default;
            if (climber == null
                || enemies == null
                || spatialIndex == null
                || !EnemyTerrainLocomotionRules.IsValidHorizontalDirection(direction)
                || !EnemySupportStackRules.CanUseSupportStack(climber.StackProfile, climber.LocomotionProfile))
            {
                return false;
            }

            spatialIndex.CollectNearbyIndices(climber.Position, nearbyEnemyIndices);
            bool found = false;
            float closestDistance = float.MaxValue;
            for (int i = 0; i < nearbyEnemyIndices.Count; i++)
            {
                EnemyRuntime support = enemies[nearbyEnemyIndices[i]];
                if (!IsValidSupportCandidate(climber, support))
                {
                    continue;
                }

                int supportLayer = support.VerticalMotor != null ? support.VerticalMotor.StackLayer : 0;
                int targetLayer = EnemySupportClimbRules.TargetLayer(supportLayer, climber.StackProfile.maxStackLayers);
                if (targetLayer <= 0)
                {
                    continue;
                }

                float targetHeight = EnemySupportClimbRules.TargetRootHeight(
                    support.Position,
                    support.BodyProfile,
                    climber.BodyProfile,
                    climber.StackProfile.layerHeightMultiplier);
                if (targetHeight <= climber.Position.y + EnemySupportClimbRules.SupportReleaseTolerance)
                {
                    continue;
                }

                float climberRadius = climber.BodyProfile != null ? climber.BodyProfile.movementBodyRadius : 0f;
                float supportRadius = support.BodyProfile != null ? support.BodyProfile.movementBodyRadius : 0f;
                if (!EnemySupportClimbRules.IsMovementBlockedBySupport(
                        climber.Position,
                        direction,
                        distance,
                        climberRadius,
                        support.Position,
                        supportRadius,
                        out float pathDistance))
                {
                    continue;
                }

                bool sameSupport = climber.VerticalMotor != null
                    && climber.VerticalMotor.SupportClimbTargetSpawnId == support.SpawnId;
                float score = sameSupport ? -1f : pathDistance;
                if (found && score >= closestDistance)
                {
                    continue;
                }

                found = true;
                closestDistance = score;
                hit = new EnemySupportSurfaceHit(support, targetHeight, targetLayer, pathDistance);
            }

            return found;
        }

        public static bool IsValidSupportCandidate(EnemyRuntime climber, EnemyRuntime support)
        {
            return climber != null
                && support != null
                && !support.IsDead
                && support.View != null
                && support.View.IsActive
                && support.VerticalMotor != null
                && support.SpawnId != climber.SpawnId
                && EnemySupportStackRules.CanBeSupport(support.StackProfile, support.LocomotionProfile)
                && EnemySupportStackRules.HasStableSupportOrder(climber.SpawnId, support.SpawnId);
        }
    }
}
