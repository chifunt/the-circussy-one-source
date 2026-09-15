using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TheCircussyOne.Content
{
    [CreateAssetMenu(menuName = "The Circussy One/Content/Chest Catalog", fileName = "ChestCatalog")]
    public sealed class ChestCatalog : SerializedScriptableObject, IContentCatalog<ChestDefinition>
    {
        private const string Tabs = "Chest Catalog";

        [TabGroup(Tabs, "Definitions"), BoxGroup(Tabs + "/Definitions/Chests"), LabelWidth(160), AssetSelector]
        public List<ChestDefinition> chests = new();

        [TabGroup(Tabs, "Diagnostics"), ShowInInspector, ReadOnly, LabelText("Valid Chests"), PropertyOrder(100)]
        private int ValidChestCount => CountValidChests();

        public IReadOnlyList<ChestDefinition> Chests => chests;
        public IReadOnlyList<ChestDefinition> Definitions => chests;

        public bool EnsureWorkflowDefaults(params ChestDefinition[] starterChests)
        {
            bool changed = false;
            chests ??= new List<ChestDefinition>();
            if (starterChests == null)
            {
                return changed;
            }

            for (int i = 0; i < starterChests.Length; i++)
            {
                ChestDefinition chest = starterChests[i];
                if (chest == null || chests.Contains(chest))
                {
                    continue;
                }

                chests.Add(chest);
                changed = true;
            }

            return changed;
        }

        public List<ContentValidationIssue> ValidateContent()
        {
            var issues = ContentCatalogRules.ValidateDefinitions(Definitions);
            int validChestCount = 0;
            if (Definitions != null)
            {
                for (int i = 0; i < Definitions.Count; i++)
                {
                    ChestDefinition chest = Definitions[i];
                    if (chest == null || !chest.isActive)
                    {
                        continue;
                    }

                    validChestCount++;
                    ValidateChest(chest, issues);
                }
            }

            if (validChestCount == 0)
            {
                issues.Add(new ContentValidationIssue(
                    "chest.no-valid-chests",
                    ContentValidationSeverity.Error,
                    "Chest catalog has no valid chests."));
            }

            return issues;
        }

        private static void ValidateChest(ChestDefinition chest, List<ContentValidationIssue> issues)
        {
            if (!System.Enum.IsDefined(typeof(ChestKind), chest.kind))
            {
                AddIssue(issues, "chest.invalid-kind", ContentValidationSeverity.Error, chest, "uses an invalid kind.");
            }

            if (chest.holdSeconds <= 0f)
            {
                AddIssue(issues, "chest.invalid-hold-seconds", ContentValidationSeverity.Error, chest, "must have a positive hold duration.");
            }

            if (chest.ticketCost < 0)
            {
                AddIssue(issues, "chest.invalid-ticket-cost", ContentValidationSeverity.Error, chest, "must not have a negative Ticket cost.");
            }

            if (chest.itemRarityWeights.common < 0
                || chest.itemRarityWeights.uncommon < 0
                || chest.itemRarityWeights.rare < 0
                || chest.itemRarityWeights.epic < 0
                || chest.itemRarityWeights.legendary < 0)
            {
                AddIssue(issues, "chest.negative-rarity-weight", ContentValidationSeverity.Error, chest, "has a negative item rarity weight.");
            }

            if (chest.itemRarityWeights.TotalWeight <= 0)
            {
                AddIssue(issues, "chest.empty-rarity-table", ContentValidationSeverity.Error, chest, "has no positive item rarity weights.");
            }

            if (string.IsNullOrWhiteSpace(chest.promptText))
            {
                AddIssue(issues, "chest.missing-prompt", ContentValidationSeverity.Warning, chest, "has no prompt text.");
            }

            if (chest.visualScale <= 0f)
            {
                AddIssue(issues, "chest.invalid-visual-scale", ContentValidationSeverity.Warning, chest, "has a non-positive visual scale.");
            }

            if (chest.emissionStrength < 0f)
            {
                AddIssue(issues, "chest.invalid-emission-strength", ContentValidationSeverity.Warning, chest, "has negative emission strength.");
            }

            if (chest.targetOutlineThickness < 0f)
            {
                AddIssue(issues, "chest.invalid-outline-thickness", ContentValidationSeverity.Warning, chest, "has negative target outline thickness.");
            }

            if (chest.interactionSquashStretchAmplitude < 0f)
            {
                AddIssue(issues, "chest.invalid-interaction-squash-amplitude", ContentValidationSeverity.Warning, chest, "has negative interaction squash amplitude.");
            }

            if (chest.interactionSquashStretchFrequency < 0f)
            {
                AddIssue(issues, "chest.invalid-interaction-squash-frequency", ContentValidationSeverity.Warning, chest, "has negative interaction squash frequency.");
            }

            if (chest.openDisappearSeconds <= 0f)
            {
                AddIssue(issues, "chest.invalid-open-disappear-seconds", ContentValidationSeverity.Warning, chest, "must have a positive open disappear duration.");
            }

            if (chest.openShrinkFinalScaleMultiplier < 0f || chest.openShrinkFinalScaleMultiplier > 1f)
            {
                AddIssue(issues, "chest.invalid-open-shrink-scale", ContentValidationSeverity.Warning, chest, "has an open shrink final scale outside 0x-1x.");
            }

            ValidatePlacement(chest.placement, issues, chest);
        }

        private static void ValidatePlacement(
            WorldPropPlacementProfile placement,
            List<ContentValidationIssue> issues,
            ChestDefinition chest)
        {
            if (placement.footprintRadius <= 0f)
            {
                AddIssue(issues, "chest.invalid-footprint-radius", ContentValidationSeverity.Error, chest, "must have a positive placement footprint radius.");
            }

            if (placement.groundClearance < 0f)
            {
                AddIssue(issues, "chest.invalid-ground-clearance", ContentValidationSeverity.Warning, chest, "must not have negative ground clearance.");
            }

            if (placement.maxPlacementSlope <= 0f || placement.maxPlacementSlope >= 90f)
            {
                AddIssue(issues, "chest.invalid-placement-slope", ContentValidationSeverity.Warning, chest, "must use a max placement slope between 0 and 90 degrees.");
            }

            if (placement.blocksPlayer
                && (placement.bodyColliderSize.x <= 0f || placement.bodyColliderSize.y <= 0f || placement.bodyColliderSize.z <= 0f))
            {
                AddIssue(issues, "chest.invalid-body-collider-size", ContentValidationSeverity.Error, chest, "blocks the player but has an invalid body collider size.");
            }
        }

        private int CountValidChests()
        {
            if (chests == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < chests.Count; i++)
            {
                ChestDefinition chest = chests[i];
                if (chest != null && chest.isActive && chest.holdSeconds > 0f && chest.itemRarityWeights.TotalWeight > 0)
                {
                    count++;
                }
            }

            return count;
        }

        private static void AddIssue(
            List<ContentValidationIssue> issues,
            string code,
            ContentValidationSeverity severity,
            ChestDefinition chest,
            string message)
        {
            issues.Add(new ContentValidationIssue(
                code,
                severity,
                $"Chest '{chest.Id}' {message}",
                chest.Id));
        }
    }
}
