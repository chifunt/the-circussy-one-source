using System;
using System.Collections.Generic;
using TheCircussyOne.Content;
using TheCircussyOne.Visuals;
using UnityEngine;

namespace TheCircussyOne.Runtime
{
    public sealed class WorldRewardSpawnPlanner
    {
        private readonly WorldRewardPlacementCatalog catalog;
        private readonly PlayerView player;
        private readonly RunWorldGenerationState generationState;

        public WorldRewardSpawnPlanner(
            WorldRewardPlacementCatalog catalog = null,
            PlayerView player = null,
            RunWorldGenerationState generationState = null)
        {
            this.catalog = catalog;
            this.player = player;
            this.generationState = generationState;
        }

        public IReadOnlyList<WorldRewardSpawnRequest> RequestsFor(WorldRewardPlacementKind kind)
        {
            Vector3 origin = player != null ? player.Position : Vector3.zero;
            var requests = new List<WorldRewardSpawnRequest>();
            AddGeneratedRequests(kind, requests);
            if (requests.Count > 0)
            {
                return requests;
            }

            AddCatalogRequests(kind, origin, requests);
            if (requests.Count == 0)
            {
                AddPrototypeFallbackRequests(kind, origin, requests);
            }

            return requests;
        }

        public static TDefinition ResolveContent<TDefinition>(
            IReadOnlyList<TDefinition> definitions,
            WorldRewardSpawnRequest request,
            ref int sequentialIndex)
            where TDefinition : class, IContentDefinition
        {
            return ResolveContent(definitions, request, ref sequentialIndex, isEligible: null);
        }

        public static TDefinition ResolveContent<TDefinition>(
            IReadOnlyList<TDefinition> definitions,
            WorldRewardSpawnRequest request,
            ref int sequentialIndex,
            Func<TDefinition, bool> isEligible)
            where TDefinition : class, IContentDefinition
        {
            if (definitions == null)
            {
                return null;
            }

            if (request.HasTargetContent)
            {
                return FindActiveById(definitions, request.TargetContentId, isEligible);
            }

            if (request.RepeatSequentialContent)
            {
                return ResolveRepeatingSequentialContent(definitions, ref sequentialIndex, isEligible);
            }

            while (sequentialIndex < definitions.Count)
            {
                TDefinition definition = definitions[sequentialIndex];
                sequentialIndex++;
                if (CanUse(definition, isEligible))
                {
                    return definition;
                }
            }

            return null;
        }

        private void AddGeneratedRequests(WorldRewardPlacementKind kind, List<WorldRewardSpawnRequest> requests)
        {
            if (generationState == null || !generationState.HasGeneratedLayout)
            {
                return;
            }

            IReadOnlyList<WorldRewardSpawnRequest> generatedRequests = generationState.RewardSpawnRequests;
            for (int i = 0; i < generatedRequests.Count; i++)
            {
                WorldRewardSpawnRequest request = generatedRequests[i];
                if (request.Kind == kind)
                {
                    requests.Add(request);
                }
            }
        }

        private void AddCatalogRequests(
            WorldRewardPlacementKind kind,
            Vector3 origin,
            List<WorldRewardSpawnRequest> requests)
        {
            IReadOnlyList<WorldRewardPlacementDefinition> placements = catalog != null ? catalog.Placements : null;
            if (placements == null)
            {
                return;
            }

            for (int i = 0; i < placements.Count; i++)
            {
                WorldRewardPlacementDefinition placement = placements[i];
                if (!ContentAvailabilityRules.IsActiveAndValid(placement) || placement.kind != kind)
                {
                    continue;
                }

                if (!ContentId.IsValidValue(placement.TargetContentId))
                {
                    continue;
                }

                int count = Mathf.Max(1, placement.count);
                for (int instanceIndex = 0; instanceIndex < count; instanceIndex++)
                {
                    requests.Add(new WorldRewardSpawnRequest(
                        placement.Id,
                        placement.kind,
                        placement.TargetContentId,
                        origin + placement.offset,
                        placement.Required));
                }
            }
        }

        private static void AddPrototypeFallbackRequests(
            WorldRewardPlacementKind kind,
            Vector3 origin,
            List<WorldRewardSpawnRequest> requests)
        {
            IReadOnlyList<Vector3> offsets = kind switch
            {
                WorldRewardPlacementKind.Chest => WorldRewardPrototypeSpawnPoints.Chests,
                WorldRewardPlacementKind.TicketDeposit => WorldRewardPrototypeSpawnPoints.TicketDeposits,
                WorldRewardPlacementKind.HealingProp => WorldRewardPrototypeSpawnPoints.HealingProps,
                _ => null
            };

            if (offsets == null)
            {
                return;
            }

            for (int i = 0; i < offsets.Count; i++)
            {
                requests.Add(new WorldRewardSpawnRequest(
                    $"{kind.ToString().ToLowerInvariant()}_prototype_{i + 1}",
                    kind,
                    targetContentId: null,
                    origin + offsets[i],
                    required: false));
            }
        }

        private static TDefinition FindActiveById<TDefinition>(
            IReadOnlyList<TDefinition> definitions,
            string contentId,
            Func<TDefinition, bool> isEligible)
            where TDefinition : class, IContentDefinition
        {
            if (string.IsNullOrWhiteSpace(contentId))
            {
                return null;
            }

            for (int i = 0; i < definitions.Count; i++)
            {
                TDefinition definition = definitions[i];
                if (CanUse(definition, isEligible) && definition.Id == contentId)
                {
                    return definition;
                }
            }

            return null;
        }

        private static TDefinition ResolveRepeatingSequentialContent<TDefinition>(
            IReadOnlyList<TDefinition> definitions,
            ref int sequentialIndex,
            Func<TDefinition, bool> isEligible)
            where TDefinition : class, IContentDefinition
        {
            if (definitions == null || definitions.Count == 0)
            {
                return null;
            }

            int attempts = 0;
            while (attempts < definitions.Count)
            {
                int index = sequentialIndex % definitions.Count;
                sequentialIndex++;
                attempts++;
                TDefinition definition = definitions[index];
                if (CanUse(definition, isEligible))
                {
                    return definition;
                }
            }

            return null;
        }

        private static bool CanUse<TDefinition>(
            TDefinition definition,
            Func<TDefinition, bool> isEligible)
            where TDefinition : class, IContentDefinition
        {
            return ContentAvailabilityRules.IsActiveAndValid(definition)
                && (isEligible == null || isEligible(definition));
        }
    }
}
