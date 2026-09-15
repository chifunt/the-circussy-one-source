using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TheCircussyOne.Content
{
    [CreateAssetMenu(menuName = "The Circussy One/Content/Upgrade Catalog", fileName = "UpgradeCatalog")]
    public sealed class UpgradeCatalog : SerializedScriptableObject, IContentCatalog<UpgradeDefinition>
    {
        private const string Tabs = "Upgrade Catalog";

        [TabGroup(Tabs, "Definitions"), BoxGroup(Tabs + "/Definitions/Available Upgrades"), LabelWidth(160), AssetSelector]
        public List<UpgradeDefinition> upgrades = new();

        [TabGroup(Tabs, "Rarity"), BoxGroup(Tabs + "/Rarity/Level Up Weights"), LabelWidth(160), InlineProperty]
        public ContentRarityWeightTable levelUpRarityWeights = ContentRarityWeightTable.LevelUpDefault();

        public IReadOnlyList<UpgradeDefinition> Upgrades => upgrades;
        public IReadOnlyList<UpgradeDefinition> Definitions => upgrades;

        public List<ContentValidationIssue> ValidateContent()
        {
            var issues = ContentCatalogRules.ValidateDefinitions(Definitions);
            issues.AddRange(ContentCatalogRules.ValidateRarityWeights(levelUpRarityWeights, "Upgrade level-up"));
            if (upgrades == null)
            {
                return issues;
            }

            for (int i = 0; i < upgrades.Count; i++)
            {
                UpgradeDefinition upgrade = upgrades[i];
                if (upgrade == null)
                {
                    continue;
                }

                if (!upgrade.isActive)
                {
                    continue;
                }

                if (upgrade.maxLevel < 1)
                {
                    issues.Add(new ContentValidationIssue("upgrade.invalid-max-level", ContentValidationSeverity.Error, $"Upgrade '{upgrade.Id}' has an invalid max level.", upgrade.Id));
                }

                if (upgrade.possibleRarities.IsEmpty)
                {
                    issues.Add(new ContentValidationIssue("upgrade.empty-possible-rarities", ContentValidationSeverity.Error, $"Upgrade '{upgrade.Id}' has no possible level-up rarities.", upgrade.Id));
                }

                if (ContentCatalogRules.LooksLikeMechanicalLevelUpDescription(upgrade.shortDescription))
                {
                    issues.Add(new ContentValidationIssue("upgrade.mechanical-short-description", ContentValidationSeverity.Warning, $"Upgrade '{upgrade.Id}' short description should be flavor text; exact numbers belong in the preview line.", upgrade.Id));
                }

                if (upgrade.statModifiers == null || upgrade.statModifiers.Count == 0)
                {
                    issues.Add(new ContentValidationIssue("upgrade.missing-stat-modifiers", ContentValidationSeverity.Error, $"Upgrade '{upgrade.Id}' has no stat modifiers.", upgrade.Id));
                }
                else
                {
                    ContentStatValidationRules.ValidateUpgradeModifiers(upgrade.statModifiers, issues, "upgrade.invalid-stat-modifier", upgrade.Id, "Upgrade");
                }

                ValidateWeaponUpgradeStats(upgrade, issues);

                if (string.IsNullOrWhiteSpace(upgrade.weaponId))
                {
                    issues.Add(new ContentValidationIssue("upgrade.missing-weapon-id", ContentValidationSeverity.Error, $"Upgrade '{upgrade.Id}' has no weapon id.", upgrade.Id));
                }

                if (upgrade.weaponDefinition != null
                    && !string.Equals(upgrade.weaponId, upgrade.weaponDefinition.Id, System.StringComparison.Ordinal))
                {
                    issues.Add(new ContentValidationIssue("upgrade.weapon-id-reference-mismatch", ContentValidationSeverity.Warning, $"Upgrade '{upgrade.Id}' has mirrored weapon id '{upgrade.weaponId}' but references '{upgrade.weaponDefinition.Id}'. Use Sync ID From Reference.", upgrade.Id));
                }

                if (upgrade.weaponDefinition == null)
                {
                    issues.Add(new ContentValidationIssue("upgrade.missing-weapon-definition", ContentValidationSeverity.Error, $"Upgrade '{upgrade.Id}' has no weapon definition.", upgrade.Id));
                }
                else if (!upgrade.weaponDefinition.IsActive)
                {
                    issues.Add(new ContentValidationIssue("upgrade.inactive-weapon-definition", ContentValidationSeverity.Error, $"Upgrade '{upgrade.Id}' targets inactive weapon '{upgrade.weaponDefinition.Id}'.", upgrade.Id));
                }
            }

            return issues;
        }

        private static void ValidateWeaponUpgradeStats(UpgradeDefinition upgrade, List<ContentValidationIssue> issues)
        {
            if (upgrade.statModifiers == null)
            {
                return;
            }

            for (int modifierIndex = 0; modifierIndex < upgrade.statModifiers.Count; modifierIndex++)
            {
                UpgradeStatModifierDefinition modifier = upgrade.statModifiers[modifierIndex];
                if (!ContentStatValidationRules.IsValid(modifier.statId, modifier.bucket))
                {
                    continue;
                }

                if (!ContentStatValidationRules.IsWeaponUpgradeStat(modifier.statId))
                {
                    issues.Add(new ContentValidationIssue(
                        "upgrade.non-weapon-stat",
                        ContentValidationSeverity.Error,
                        $"Upgrade '{upgrade.Id}' uses '{ContentModifierDisplayRules.StatDisplayName(modifier.statId)}', but weapon upgrades can only use weapon-local stats.",
                        upgrade.Id));
                    continue;
                }

                if (upgrade.weaponDefinition != null && !upgrade.weaponDefinition.SupportsUpgradeStat(modifier.statId))
                {
                    string weaponName = string.IsNullOrWhiteSpace(upgrade.weaponDefinition.DisplayName)
                        ? upgrade.weaponDefinition.Id
                        : upgrade.weaponDefinition.DisplayName;
                    issues.Add(new ContentValidationIssue(
                        "upgrade.unsupported-weapon-stat",
                        ContentValidationSeverity.Error,
                        $"Upgrade '{upgrade.Id}' uses '{ContentModifierDisplayRules.StatDisplayName(modifier.statId)}', but target weapon '{weaponName}' does not support that stat.",
                        upgrade.Id));
                }
            }
        }

        public bool EnsureWorkflowDefaults(params UpgradeDefinition[] starterUpgrades)
        {
            bool changed = false;
            upgrades ??= new List<UpgradeDefinition>();
            if (!levelUpRarityWeights.IsValid)
            {
                levelUpRarityWeights = ContentRarityWeightTable.LevelUpDefault();
                changed = true;
            }

            if (starterUpgrades == null)
            {
                return changed;
            }

            for (int i = 0; i < starterUpgrades.Length; i++)
            {
                UpgradeDefinition upgrade = starterUpgrades[i];
                if (upgrade == null || upgrades.Contains(upgrade))
                {
                    continue;
                }

                upgrades.Add(upgrade);
                changed = true;
            }

            return changed;
        }
    }
}
