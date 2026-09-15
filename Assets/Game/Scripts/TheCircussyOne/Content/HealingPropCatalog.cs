using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TheCircussyOne.Content
{
    [CreateAssetMenu(menuName = "The Circussy One/Content/Healing Prop Catalog", fileName = "HealingPropCatalog")]
    public sealed class HealingPropCatalog : SerializedScriptableObject, IContentCatalog<HealingPropDefinition>
    {
        private const string Tabs = "Healing Prop Catalog";

        [TabGroup(Tabs, "Definitions"), BoxGroup(Tabs + "/Definitions/Props"), LabelWidth(170), AssetSelector]
        public List<HealingPropDefinition> props = new();

        [TabGroup(Tabs, "Diagnostics"), ShowInInspector, ReadOnly, LabelText("Valid Props"), PropertyOrder(100)]
        private int ValidPropCount => CountValidProps();

        public IReadOnlyList<HealingPropDefinition> Props => props;
        public IReadOnlyList<HealingPropDefinition> Definitions => props;

        public bool EnsureWorkflowDefaults(params HealingPropDefinition[] starterProps)
        {
            bool changed = false;
            props ??= new List<HealingPropDefinition>();
            if (starterProps == null)
            {
                return changed;
            }

            for (int i = 0; i < starterProps.Length; i++)
            {
                HealingPropDefinition prop = starterProps[i];
                if (prop == null || props.Contains(prop))
                {
                    continue;
                }

                props.Add(prop);
                changed = true;
            }

            return changed;
        }

        public List<ContentValidationIssue> ValidateContent()
        {
            var issues = ContentCatalogRules.ValidateDefinitions(Definitions);
            int validCount = 0;
            if (Definitions != null)
            {
                for (int i = 0; i < Definitions.Count; i++)
                {
                    HealingPropDefinition prop = Definitions[i];
                    if (prop == null || !prop.isActive)
                    {
                        continue;
                    }

                    validCount++;
                    ValidateProp(prop, issues);
                }
            }

            if (validCount == 0)
            {
                issues.Add(new ContentValidationIssue(
                    "healing-prop.no-valid-props",
                    ContentValidationSeverity.Error,
                    "Healing prop catalog has no valid active props."));
            }

            return issues;
        }

        private static void ValidateProp(HealingPropDefinition prop, List<ContentValidationIssue> issues)
        {
            if (prop.holdSeconds <= 0f)
            {
                AddIssue(issues, "healing-prop.invalid-hold-seconds", ContentValidationSeverity.Error, prop, "must have a positive hold duration.");
            }

            if (prop.healthPickup == null)
            {
                AddIssue(issues, "healing-prop.missing-pickup", ContentValidationSeverity.Error, prop, "must reference a health pickup reward.");
            }

            if (prop.minPickupCount <= 0 || prop.maxPickupCount <= 0)
            {
                AddIssue(issues, "healing-prop.invalid-pickup-count", ContentValidationSeverity.Error, prop, "must spawn at least one pickup.");
            }

            if (prop.maxPickupCount < prop.minPickupCount)
            {
                AddIssue(issues, "healing-prop.invalid-pickup-count-range", ContentValidationSeverity.Error, prop, "must not have max pickup count below min pickup count.");
            }

            if (string.IsNullOrWhiteSpace(prop.promptText))
            {
                AddIssue(issues, "healing-prop.missing-prompt", ContentValidationSeverity.Warning, prop, "has no prompt text.");
            }

            if (prop.visualScale <= 0f)
            {
                AddIssue(issues, "healing-prop.invalid-visual-scale", ContentValidationSeverity.Warning, prop, "has a non-positive visual scale.");
            }

            if (prop.emissionStrength < 0f)
            {
                AddIssue(issues, "healing-prop.invalid-emission-strength", ContentValidationSeverity.Warning, prop, "has negative emission strength.");
            }

            if (prop.targetOutlineThickness < 0f)
            {
                AddIssue(issues, "healing-prop.invalid-outline-thickness", ContentValidationSeverity.Warning, prop, "has negative target outline thickness.");
            }

            if (prop.interactionSquashStretchAmplitude < 0f)
            {
                AddIssue(issues, "healing-prop.invalid-interaction-squash-amplitude", ContentValidationSeverity.Warning, prop, "has negative interaction squash amplitude.");
            }

            if (prop.interactionSquashStretchFrequency < 0f)
            {
                AddIssue(issues, "healing-prop.invalid-interaction-squash-frequency", ContentValidationSeverity.Warning, prop, "has negative interaction squash frequency.");
            }

            if (prop.openDisappearSeconds <= 0f)
            {
                AddIssue(issues, "healing-prop.invalid-open-disappear-seconds", ContentValidationSeverity.Warning, prop, "must have a positive open disappear duration.");
            }

            if (prop.openShrinkFinalScaleMultiplier < 0f || prop.openShrinkFinalScaleMultiplier > 1f)
            {
                AddIssue(issues, "healing-prop.invalid-open-shrink-scale", ContentValidationSeverity.Warning, prop, "must shrink to a final scale between 0 and 1.");
            }

            if (prop.snackBurstCount < 0)
            {
                AddIssue(issues, "healing-prop.invalid-snack-burst-count", ContentValidationSeverity.Warning, prop, "must not use a negative snack burst count.");
            }

            if (prop.snackBurstLifetimeSeconds <= 0f)
            {
                AddIssue(issues, "healing-prop.invalid-snack-burst-lifetime", ContentValidationSeverity.Warning, prop, "must have a positive snack burst lifetime.");
            }

            if (prop.snackBurstSpeed < 0f)
            {
                AddIssue(issues, "healing-prop.invalid-snack-burst-speed", ContentValidationSeverity.Warning, prop, "must not use negative snack burst speed.");
            }

            if (prop.snackBurstGravity < 0f)
            {
                AddIssue(issues, "healing-prop.invalid-snack-burst-gravity", ContentValidationSeverity.Warning, prop, "must not use negative snack burst gravity.");
            }

            if (prop.snackBurstSwayStrength < 0f)
            {
                AddIssue(issues, "healing-prop.invalid-snack-burst-sway", ContentValidationSeverity.Warning, prop, "must not use negative snack burst sway.");
            }

            if (prop.snackBurstStartSize <= 0f)
            {
                AddIssue(issues, "healing-prop.invalid-snack-burst-size", ContentValidationSeverity.Warning, prop, "must use a positive snack burst particle size.");
            }

            ValidatePlacement(prop.placement, issues, prop);
        }

        private static void ValidatePlacement(
            WorldPropPlacementProfile placement,
            List<ContentValidationIssue> issues,
            HealingPropDefinition prop)
        {
            if (placement.footprintRadius <= 0f)
            {
                AddIssue(issues, "healing-prop.invalid-footprint-radius", ContentValidationSeverity.Error, prop, "must have a positive placement footprint radius.");
            }

            if (placement.groundClearance < 0f)
            {
                AddIssue(issues, "healing-prop.invalid-ground-clearance", ContentValidationSeverity.Warning, prop, "must not have negative ground clearance.");
            }

            if (placement.maxPlacementSlope <= 0f || placement.maxPlacementSlope >= 90f)
            {
                AddIssue(issues, "healing-prop.invalid-placement-slope", ContentValidationSeverity.Warning, prop, "must use a max placement slope between 0 and 90 degrees.");
            }

            if (placement.blocksPlayer
                && (placement.bodyColliderSize.x <= 0f || placement.bodyColliderSize.y <= 0f || placement.bodyColliderSize.z <= 0f))
            {
                AddIssue(issues, "healing-prop.invalid-body-collider-size", ContentValidationSeverity.Error, prop, "blocks the player but has an invalid body collider size.");
            }
        }

        private int CountValidProps()
        {
            if (props == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < props.Count; i++)
            {
                HealingPropDefinition prop = props[i];
                if (prop != null && prop.isActive && prop.holdSeconds > 0f && prop.healthPickup != null)
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
            HealingPropDefinition prop,
            string message)
        {
            issues.Add(new ContentValidationIssue(
                code,
                severity,
                $"Healing prop '{prop.Id}' {message}",
                prop.Id));
        }
    }
}
