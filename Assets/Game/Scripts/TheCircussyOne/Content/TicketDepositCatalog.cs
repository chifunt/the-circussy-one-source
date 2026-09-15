using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TheCircussyOne.Content
{
    [CreateAssetMenu(menuName = "The Circussy One/Content/Ticket Deposit Catalog", fileName = "TicketDepositCatalog")]
    public sealed class TicketDepositCatalog : SerializedScriptableObject, IContentCatalog<TicketDepositDefinition>
    {
        private const string Tabs = "Ticket Deposit Catalog";

        [TabGroup(Tabs, "Definitions"), BoxGroup(Tabs + "/Definitions/Deposits"), LabelWidth(160), AssetSelector]
        public List<TicketDepositDefinition> deposits = new();

        [TabGroup(Tabs, "Diagnostics"), ShowInInspector, ReadOnly, LabelText("Valid Deposits"), PropertyOrder(100)]
        private int ValidDepositCount => CountValidDeposits();

        public IReadOnlyList<TicketDepositDefinition> Deposits => deposits;
        public IReadOnlyList<TicketDepositDefinition> Definitions => deposits;

        public bool EnsureWorkflowDefaults(params TicketDepositDefinition[] starterDeposits)
        {
            bool changed = false;
            deposits ??= new List<TicketDepositDefinition>();
            if (starterDeposits == null)
            {
                return changed;
            }

            for (int i = 0; i < starterDeposits.Length; i++)
            {
                TicketDepositDefinition deposit = starterDeposits[i];
                if (deposit == null || deposits.Contains(deposit))
                {
                    continue;
                }

                deposits.Add(deposit);
                changed = true;
            }

            return changed;
        }

        public List<ContentValidationIssue> ValidateContent()
        {
            var issues = ContentCatalogRules.ValidateDefinitions(Definitions);
            int validDepositCount = 0;
            if (Definitions != null)
            {
                for (int i = 0; i < Definitions.Count; i++)
                {
                    TicketDepositDefinition deposit = Definitions[i];
                    if (deposit == null || !deposit.isActive)
                    {
                        continue;
                    }

                    validDepositCount++;
                    ValidateDeposit(deposit, issues);
                }
            }

            if (validDepositCount == 0)
            {
                issues.Add(new ContentValidationIssue(
                    "ticket-deposit.no-valid-deposits",
                    ContentValidationSeverity.Error,
                    "Ticket deposit catalog has no valid deposits."));
            }

            return issues;
        }

        private static void ValidateDeposit(TicketDepositDefinition deposit, List<ContentValidationIssue> issues)
        {
            if (!System.Enum.IsDefined(typeof(TicketDepositTier), deposit.tier))
            {
                AddIssue(issues, "ticket-deposit.invalid-tier", ContentValidationSeverity.Error, deposit, "uses an invalid tier.");
            }

            if (!System.Enum.IsDefined(typeof(TicketDepositPayoutMode), deposit.payoutMode))
            {
                AddIssue(issues, "ticket-deposit.invalid-payout-mode", ContentValidationSeverity.Error, deposit, "uses an invalid payout mode.");
            }

            if (deposit.holdSeconds <= 0f)
            {
                AddIssue(issues, "ticket-deposit.invalid-hold-seconds", ContentValidationSeverity.Error, deposit, "must have a positive hold duration.");
            }

            if (deposit.minTickets < 0)
            {
                AddIssue(issues, "ticket-deposit.invalid-min-tickets", ContentValidationSeverity.Error, deposit, "must not have a negative minimum reward.");
            }

            if (deposit.maxTickets < deposit.minTickets)
            {
                AddIssue(issues, "ticket-deposit.invalid-ticket-range", ContentValidationSeverity.Error, deposit, "must have max tickets greater than or equal to min tickets.");
            }

            if (deposit.maxTickets <= 0)
            {
                AddIssue(issues, "ticket-deposit.zero-ticket-reward", ContentValidationSeverity.Error, deposit, "must reward at least one Ticket.");
            }

            if (deposit.payoutMode == TicketDepositPayoutMode.Chunked && deposit.payoutChunks < 1)
            {
                AddIssue(issues, "ticket-deposit.invalid-payout-chunks", ContentValidationSeverity.Error, deposit, "must have at least one payout chunk.");
            }

            if (string.IsNullOrWhiteSpace(deposit.promptText))
            {
                AddIssue(issues, "ticket-deposit.missing-prompt", ContentValidationSeverity.Warning, deposit, "has no prompt text.");
            }

            if (deposit.visualScale <= 0f)
            {
                AddIssue(issues, "ticket-deposit.invalid-visual-scale", ContentValidationSeverity.Warning, deposit, "has a non-positive visual scale.");
            }

            if (deposit.emissionStrength < 0f)
            {
                AddIssue(issues, "ticket-deposit.invalid-emission-strength", ContentValidationSeverity.Warning, deposit, "has negative emission strength.");
            }

            if (deposit.targetOutlineThickness < 0f)
            {
                AddIssue(issues, "ticket-deposit.invalid-outline-thickness", ContentValidationSeverity.Warning, deposit, "has negative target outline thickness.");
            }

            if (deposit.interactionSquashStretchAmplitude < 0f)
            {
                AddIssue(issues, "ticket-deposit.invalid-interaction-squash-amplitude", ContentValidationSeverity.Warning, deposit, "has negative interaction squash amplitude.");
            }

            if (deposit.interactionSquashStretchFrequency < 0f)
            {
                AddIssue(issues, "ticket-deposit.invalid-interaction-squash-frequency", ContentValidationSeverity.Warning, deposit, "has negative interaction squash frequency.");
            }

            if (deposit.collectDisappearSeconds <= 0f)
            {
                AddIssue(issues, "ticket-deposit.invalid-collect-disappear-seconds", ContentValidationSeverity.Warning, deposit, "must have a positive collect disappear duration.");
            }

            if (deposit.collectShrinkFinalScaleMultiplier < 0f || deposit.collectShrinkFinalScaleMultiplier > 1f)
            {
                AddIssue(issues, "ticket-deposit.invalid-collect-shrink-scale", ContentValidationSeverity.Warning, deposit, "must shrink to a final scale between 0 and 1.");
            }

            if (deposit.ticketBurstCount < 0)
            {
                AddIssue(issues, "ticket-deposit.invalid-ticket-burst-count", ContentValidationSeverity.Warning, deposit, "must not use a negative ticket burst count.");
            }

            if (deposit.ticketBurstLifetimeSeconds <= 0f)
            {
                AddIssue(issues, "ticket-deposit.invalid-ticket-burst-lifetime", ContentValidationSeverity.Warning, deposit, "must have a positive ticket burst lifetime.");
            }

            if (deposit.ticketBurstSpeed < 0f)
            {
                AddIssue(issues, "ticket-deposit.invalid-ticket-burst-speed", ContentValidationSeverity.Warning, deposit, "must not use negative ticket burst speed.");
            }

            if (deposit.ticketBurstGravity < 0f)
            {
                AddIssue(issues, "ticket-deposit.invalid-ticket-burst-gravity", ContentValidationSeverity.Warning, deposit, "must not use negative ticket burst gravity.");
            }

            if (deposit.ticketBurstSwayStrength < 0f)
            {
                AddIssue(issues, "ticket-deposit.invalid-ticket-burst-sway", ContentValidationSeverity.Warning, deposit, "must not use negative ticket burst sway.");
            }

            if (deposit.ticketBurstRectangleSize.x <= 0f || deposit.ticketBurstRectangleSize.y <= 0f)
            {
                AddIssue(issues, "ticket-deposit.invalid-ticket-burst-rectangle-size", ContentValidationSeverity.Warning, deposit, "must use positive ticket burst rectangle dimensions.");
            }

            ValidatePlacement(deposit.placement, issues, deposit);
        }

        private static void ValidatePlacement(
            WorldPropPlacementProfile placement,
            List<ContentValidationIssue> issues,
            TicketDepositDefinition deposit)
        {
            if (placement.footprintRadius <= 0f)
            {
                AddIssue(issues, "ticket-deposit.invalid-footprint-radius", ContentValidationSeverity.Error, deposit, "must have a positive placement footprint radius.");
            }

            if (placement.groundClearance < 0f)
            {
                AddIssue(issues, "ticket-deposit.invalid-ground-clearance", ContentValidationSeverity.Warning, deposit, "must not have negative ground clearance.");
            }

            if (placement.maxPlacementSlope <= 0f || placement.maxPlacementSlope >= 90f)
            {
                AddIssue(issues, "ticket-deposit.invalid-placement-slope", ContentValidationSeverity.Warning, deposit, "must use a max placement slope between 0 and 90 degrees.");
            }

            if (placement.blocksPlayer
                && (placement.bodyColliderSize.x <= 0f || placement.bodyColliderSize.y <= 0f || placement.bodyColliderSize.z <= 0f))
            {
                AddIssue(issues, "ticket-deposit.invalid-body-collider-size", ContentValidationSeverity.Error, deposit, "blocks the player but has an invalid body collider size.");
            }
        }

        private int CountValidDeposits()
        {
            if (deposits == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < deposits.Count; i++)
            {
                TicketDepositDefinition deposit = deposits[i];
                if (deposit != null && deposit.isActive && deposit.holdSeconds > 0f && deposit.maxTickets > 0)
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
            TicketDepositDefinition deposit,
            string message)
        {
            issues.Add(new ContentValidationIssue(
                code,
                severity,
                $"Ticket deposit '{deposit.Id}' {message}",
                deposit.Id));
        }
    }
}
