using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TheCircussyOne.Content
{
    [CreateAssetMenu(menuName = "The Circussy One/Content/Health Pickup Catalog", fileName = "HealthPickupCatalog")]
    public sealed class HealthPickupCatalog : SerializedScriptableObject, IContentCatalog<HealthPickupDefinition>
    {
        private const string Tabs = "Health Pickup Catalog";

        [TabGroup(Tabs, "Definitions"), BoxGroup(Tabs + "/Definitions/Pickups"), LabelWidth(170), AssetSelector]
        public List<HealthPickupDefinition> pickups = new();

        [TabGroup(Tabs, "Defaults"), BoxGroup(Tabs + "/Defaults/Healing"), LabelWidth(170), AssetSelector]
        public HealthPickupDefinition defaultHealthPickup;

        [TabGroup(Tabs, "Diagnostics"), ShowInInspector, ReadOnly, LabelText("Valid Pickups"), PropertyOrder(100)]
        private int ValidPickupCount => CountValidPickups();

        public IReadOnlyList<HealthPickupDefinition> Pickups => pickups;
        public IReadOnlyList<HealthPickupDefinition> Definitions => pickups;

        public bool EnsureWorkflowDefaults(params HealthPickupDefinition[] starterPickups)
        {
            bool changed = false;
            pickups ??= new List<HealthPickupDefinition>();
            if (starterPickups != null)
            {
                for (int i = 0; i < starterPickups.Length; i++)
                {
                    HealthPickupDefinition pickup = starterPickups[i];
                    if (pickup == null || pickups.Contains(pickup))
                    {
                        continue;
                    }

                    pickups.Add(pickup);
                    changed = true;
                }
            }

            if (defaultHealthPickup == null)
            {
                defaultHealthPickup = FirstValidPickup();
                changed |= defaultHealthPickup != null;
            }

            return changed;
        }

        public HealthPickupDefinition ResolveDefault()
        {
            if (IsValidPickup(defaultHealthPickup))
            {
                return defaultHealthPickup;
            }

            return FirstValidPickup();
        }

        public List<ContentValidationIssue> ValidateContent()
        {
            var issues = ContentCatalogRules.ValidateDefinitions(Definitions);
            int validCount = 0;
            if (Definitions != null)
            {
                for (int i = 0; i < Definitions.Count; i++)
                {
                    HealthPickupDefinition pickup = Definitions[i];
                    if (pickup == null || !pickup.isActive)
                    {
                        continue;
                    }

                    validCount++;
                    ValidatePickup(pickup, issues);
                }
            }

            if (validCount == 0)
            {
                issues.Add(new ContentValidationIssue(
                    "health-pickup.no-valid-pickups",
                    ContentValidationSeverity.Error,
                    "Health pickup catalog has no valid active pickups."));
            }

            if (defaultHealthPickup == null)
            {
                issues.Add(new ContentValidationIssue(
                    "health-pickup.missing-default",
                    ContentValidationSeverity.Warning,
                    "Health pickup catalog has no default pickup."));
            }

            return issues;
        }

        private static void ValidatePickup(HealthPickupDefinition pickup, List<ContentValidationIssue> issues)
        {
            if (pickup.healPercentOfMaxHealth <= 0f)
            {
                AddIssue(issues, "health-pickup.non-positive-heal", ContentValidationSeverity.Error, pickup, "must heal a positive max HP percentage.");
            }

            if (pickup.visualScale <= 0f)
            {
                AddIssue(issues, "health-pickup.non-positive-scale", ContentValidationSeverity.Warning, pickup, "has a non-positive visual scale.");
            }

            if (pickup.emissionStrength < 0f)
            {
                AddIssue(issues, "health-pickup.negative-emission", ContentValidationSeverity.Warning, pickup, "has negative emission strength.");
            }

            if (!System.Enum.IsDefined(typeof(PickupVisualShape), pickup.visualShape))
            {
                AddIssue(issues, "health-pickup.invalid-shape", ContentValidationSeverity.Error, pickup, "uses an invalid visual shape.");
            }
        }

        private int CountValidPickups()
        {
            int count = 0;
            if (pickups == null)
            {
                return count;
            }

            for (int i = 0; i < pickups.Count; i++)
            {
                HealthPickupDefinition pickup = pickups[i];
                if (IsValidPickup(pickup))
                {
                    count++;
                }
            }

            return count;
        }

        private HealthPickupDefinition FirstValidPickup()
        {
            if (pickups == null)
            {
                return null;
            }

            for (int i = 0; i < pickups.Count; i++)
            {
                HealthPickupDefinition pickup = pickups[i];
                if (IsValidPickup(pickup))
                {
                    return pickup;
                }
            }

            return null;
        }

        private static bool IsValidPickup(HealthPickupDefinition pickup)
        {
            return pickup != null && pickup.isActive && pickup.healPercentOfMaxHealth > 0f;
        }

        private static void AddIssue(
            List<ContentValidationIssue> issues,
            string code,
            ContentValidationSeverity severity,
            HealthPickupDefinition pickup,
            string message)
        {
            issues.Add(new ContentValidationIssue(
                code,
                severity,
                $"Health pickup '{pickup.Id}' {message}",
                pickup.Id));
        }
    }
}
