using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using TheCircussyOne.Stats;

namespace TheCircussyOne.Content
{
    [CreateAssetMenu(menuName = "The Circussy One/Content/Weapon Catalog", fileName = "WeaponCatalog")]
    public sealed class WeaponCatalog : SerializedScriptableObject, IContentCatalog<WeaponDefinition>
    {
        private const string Tabs = "Weapon Catalog";

        [TabGroup(Tabs, "Starting Loadout"), BoxGroup(Tabs + "/Starting Loadout/Weapons"), LabelWidth(150), AssetSelector]
        public List<WeaponDefinition> startingWeapons = new();

        [TabGroup(Tabs, "Available Weapons"), BoxGroup(Tabs + "/Available Weapons/Definitions"), LabelWidth(150), AssetSelector]
        public List<WeaponDefinition> availableWeapons = new();

        public IReadOnlyList<WeaponDefinition> StartingWeapons => startingWeapons;
        public IReadOnlyList<WeaponDefinition> AvailableWeapons => availableWeapons;
        public IReadOnlyList<WeaponDefinition> Definitions => availableWeapons;

        public List<ContentValidationIssue> ValidateContent()
        {
            var issues = ContentCatalogRules.ValidateDefinitions(Definitions);
            if (availableWeapons == null)
            {
                return issues;
            }

            int activeAvailableCount = 0;
            for (int i = 0; i < availableWeapons.Count; i++)
            {
                WeaponDefinition weapon = availableWeapons[i];
                if (weapon == null)
                {
                    continue;
                }

                if (weapon.isActive)
                {
                    activeAvailableCount++;
                }

                if (!weapon.isActive)
                {
                    continue;
                }

                ValidateWeaponAuthoring(weapon, issues);
                if (weapon.UpgradeTrack == null)
                {
                    continue;
                }

                var seenUpgradeIds = new HashSet<string>();
                for (int upgradeIndex = 0; upgradeIndex < weapon.UpgradeTrack.Count; upgradeIndex++)
                {
                    UpgradeDefinition upgrade = weapon.UpgradeTrack[upgradeIndex];
                    if (upgrade == null)
                    {
                        issues.Add(new ContentValidationIssue("weapon.upgrade-track-null", ContentValidationSeverity.Error, $"Weapon '{weapon.Id}' has a null upgrade-track entry.", weapon.Id));
                        continue;
                    }

                    if (!seenUpgradeIds.Add(upgrade.Id))
                    {
                        issues.Add(new ContentValidationIssue("weapon.upgrade-track-duplicate", ContentValidationSeverity.Error, $"Weapon '{weapon.Id}' has duplicate upgrade-track entry '{upgrade.Id}'.", weapon.Id));
                    }

                    if (!string.IsNullOrWhiteSpace(upgrade.weaponId) && upgrade.weaponId != weapon.Id)
                    {
                        issues.Add(new ContentValidationIssue("weapon.upgrade-track-wrong-weapon-id", ContentValidationSeverity.Error, $"Weapon '{weapon.Id}' upgrade '{upgrade.Id}' targets weapon id '{upgrade.weaponId}'.", weapon.Id));
                    }

                    if (!ReferenceEquals(upgrade.weaponDefinition, weapon))
                    {
                        issues.Add(new ContentValidationIssue("weapon.upgrade-track-wrong-weapon-reference", ContentValidationSeverity.Error, $"Weapon '{weapon.Id}' upgrade '{upgrade.Id}' references a different weapon definition.", weapon.Id));
                    }

                    if (!weapon.SupportsUpgrade(upgrade))
                    {
                        issues.Add(new ContentValidationIssue("weapon.upgrade-track-unsupported-stat", ContentValidationSeverity.Error, $"Weapon '{weapon.Id}' upgrade '{upgrade.Id}' uses a stat not listed in the weapon's supported upgrade stats.", weapon.Id));
                    }
                }
            }

            if (activeAvailableCount == 0)
            {
                issues.Add(new ContentValidationIssue("weapon.no-active-weapons", ContentValidationSeverity.Error, "Weapon catalog has no active available weapons."));
            }

            if (startingWeapons != null)
            {
                for (int i = 0; i < startingWeapons.Count; i++)
                {
                    WeaponDefinition weapon = startingWeapons[i];
                    if (weapon != null && !weapon.isActive)
                    {
                        issues.Add(new ContentValidationIssue("weapon.inactive-starting-weapon", ContentValidationSeverity.Error, $"Starting loadout contains inactive weapon '{weapon.Id}'.", weapon.Id));
                    }
                }
            }

            return issues;
        }

        private static void ValidateWeaponAuthoring(WeaponDefinition weapon, List<ContentValidationIssue> issues)
        {
            if (weapon.orbitEnabled && (weapon.bounceEnabled || weapon.explosiveEnabled || weapon.chainEnabled))
            {
                issues.Add(new ContentValidationIssue(
                    "weapon.family-conflict",
                    ContentValidationSeverity.Warning,
                    $"Weapon '{weapon.Id}' is an orbit weapon but also has projectile-only family toggles enabled. Those projectile settings will not affect runtime behavior.",
                    weapon.Id));
            }

            IReadOnlyList<StatId> stats = weapon.SupportedUpgradeStats;
            if (stats == null)
            {
                return;
            }

            for (int i = 0; i < stats.Count; i++)
            {
                StatId statId = stats[i];
                if (!WeaponAuthoringApplicability.IsSupportedUpgradeStatActive(weapon, statId))
                {
                    issues.Add(new ContentValidationIssue(
                        "weapon.inactive-supported-stat",
                        ContentValidationSeverity.Warning,
                        $"Weapon '{weapon.Id}' lists supported stat '{statId}', but that stat does not affect its current weapon family/settings.",
                        weapon.Id));
                }
            }
        }

        public bool EnsureWorkflowDefaults(WeaponDefinition starterWeapon, params WeaponDefinition[] availableDefinitions)
        {
            bool changed = false;
            startingWeapons ??= new List<WeaponDefinition>();
            availableWeapons ??= new List<WeaponDefinition>();
            if (starterWeapon != null)
            {
                changed |= AddIfMissing(startingWeapons, starterWeapon);
                changed |= AddIfMissing(availableWeapons, starterWeapon);
            }

            if (availableDefinitions != null)
            {
                for (int i = 0; i < availableDefinitions.Length; i++)
                {
                    changed |= AddIfMissing(availableWeapons, availableDefinitions[i]);
                }
            }

            return changed;
        }

        private static bool AddIfMissing(List<WeaponDefinition> list, WeaponDefinition definition)
        {
            if (definition == null || list.Contains(definition))
            {
                return false;
            }

            list.Add(definition);
            return true;
        }
    }
}
