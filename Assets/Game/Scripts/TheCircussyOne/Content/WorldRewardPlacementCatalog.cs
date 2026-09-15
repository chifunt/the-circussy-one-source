using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TheCircussyOne.Content
{
    [CreateAssetMenu(menuName = "The Circussy One/Content/World Reward Placement Catalog", fileName = "WorldRewardPlacementCatalog")]
    public sealed class WorldRewardPlacementCatalog : SerializedScriptableObject, IContentCatalog<WorldRewardPlacementDefinition>
    {
        private const string Tabs = "World Reward Placement Catalog";

        [TabGroup(Tabs, "Definitions"), BoxGroup(Tabs + "/Definitions/Placements"), LabelWidth(170), ListDrawerSettings(DefaultExpandedState = true)]
        public List<WorldRewardPlacementDefinition> placements = new();

        [TabGroup(Tabs, "Diagnostics"), ShowInInspector, ReadOnly, LabelText("Active Placements"), PropertyOrder(100)]
        private int ActivePlacementCount => CountActivePlacements();

        public IReadOnlyList<WorldRewardPlacementDefinition> Placements => placements;
        public IReadOnlyList<WorldRewardPlacementDefinition> Definitions => placements;

        public bool EnsureWorkflowDefaults(params WorldRewardPlacementDefinition[] starterPlacements)
        {
            bool changed = false;
            placements ??= new List<WorldRewardPlacementDefinition>();
            if (starterPlacements == null)
            {
                return changed;
            }

            for (int i = 0; i < placements.Count; i++)
            {
                changed |= placements[i]?.EnsureWorkflowDefaults() ?? false;
            }

            for (int i = 0; i < starterPlacements.Length; i++)
            {
                WorldRewardPlacementDefinition placement = starterPlacements[i];
                if (placement == null || ContainsPlacementId(placement.Id))
                {
                    continue;
                }

                placement.EnsureWorkflowDefaults();
                placements.Add(placement);
                changed = true;
            }

            return changed;
        }

        public List<ContentValidationIssue> ValidateContent()
        {
            var issues = ContentCatalogRules.ValidateDefinitions(Definitions);
            int activeCount = 0;
            if (Definitions != null)
            {
                for (int i = 0; i < Definitions.Count; i++)
                {
                    WorldRewardPlacementDefinition placement = Definitions[i];
                    if (placement == null || !placement.isActive)
                    {
                        continue;
                    }

                    activeCount++;
                    ValidatePlacement(placement, issues);
                }
            }

            if (activeCount == 0)
            {
                issues.Add(new ContentValidationIssue(
                    "world-reward-placement.no-active-placements",
                    ContentValidationSeverity.Error,
                    "World reward placement catalog has no active placements."));
            }

            return issues;
        }

        private static void ValidatePlacement(WorldRewardPlacementDefinition placement, List<ContentValidationIssue> issues)
        {
            if (!System.Enum.IsDefined(typeof(WorldRewardPlacementKind), placement.kind))
            {
                AddIssue(issues, "world-reward-placement.invalid-kind", ContentValidationSeverity.Error, placement, "uses an invalid placement kind.");
            }

            if (string.IsNullOrWhiteSpace(placement.targetContentId))
            {
                AddIssue(issues, "world-reward-placement.missing-target-content", ContentValidationSeverity.Error, placement, "must name the target content id.");
            }
            else if (!ContentId.IsValidValue(placement.targetContentId))
            {
                AddIssue(issues, "world-reward-placement.invalid-target-content-id", ContentValidationSeverity.Error, placement, "uses an invalid target content id.");
            }

            if (placement.count < 1)
            {
                AddIssue(issues, "world-reward-placement.invalid-count", ContentValidationSeverity.Error, placement, "must have a positive count.");
            }

            if (placement.kind == WorldRewardPlacementKind.Shrine)
            {
                AddIssue(issues, "world-reward-placement.shrine-readiness", ContentValidationSeverity.Warning, placement, "is a shrine-ready placement. It will not spawn until shrine gameplay exists.");
            }
        }

        private int CountActivePlacements()
        {
            if (placements == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < placements.Count; i++)
            {
                WorldRewardPlacementDefinition placement = placements[i];
                if (placement != null && placement.isActive)
                {
                    count += Mathf.Max(1, placement.count);
                }
            }

            return count;
        }

        private bool ContainsPlacementId(string placementId)
        {
            if (string.IsNullOrWhiteSpace(placementId))
            {
                return false;
            }

            for (int i = 0; i < placements.Count; i++)
            {
                if (placements[i] != null && placements[i].Id == placementId)
                {
                    return true;
                }
            }

            return false;
        }

        private static void AddIssue(
            List<ContentValidationIssue> issues,
            string code,
            ContentValidationSeverity severity,
            WorldRewardPlacementDefinition placement,
            string message)
        {
            issues.Add(new ContentValidationIssue(
                code,
                severity,
                $"World reward placement '{placement.Id}' {message}",
                placement.Id));
        }
    }
}
