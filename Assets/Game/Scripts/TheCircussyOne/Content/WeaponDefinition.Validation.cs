using System;
using System.Collections.Generic;
using TheCircussyOne.Stats;

namespace TheCircussyOne.Content
{
    public sealed partial class WeaponDefinition
    {
        public bool SupportsUpgradeStat(StatId statId)
        {
            IReadOnlyList<StatId> stats = supportedUpgradeStats != null && supportedUpgradeStats.Count > 0
                ? supportedUpgradeStats
                : DefaultSupportedUpgradeStatsForId();

            for (int i = 0; i < stats.Count; i++)
            {
                if (stats[i] == statId)
                {
                    return true;
                }
            }

            return false;
        }

        public bool SupportsUpgrade(UpgradeDefinition upgrade)
        {
            if (upgrade == null)
            {
                return false;
            }

            if (!string.Equals(upgrade.weaponId, Id, System.StringComparison.Ordinal))
            {
                return false;
            }

            if (upgrade.statModifiers == null || upgrade.statModifiers.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < upgrade.statModifiers.Count; i++)
            {
                if (!SupportsUpgradeStat(upgrade.statModifiers[i].statId))
                {
                    return false;
                }
            }

            return true;
        }

        private bool HasWeaponId(string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        private bool HasDisplayName(string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        private bool IsMinimumFireIntervalValid(float value)
        {
            return value <= baseFireIntervalSeconds;
        }

        private static bool EnsureString(ref string value, string defaultValue)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            value = defaultValue;
            return true;
        }

        private static bool EnsureMinimum(ref float value, float defaultValue, float minimum)
        {
            if (value >= minimum)
            {
                return false;
            }

            value = defaultValue;
            return true;
        }

        private static bool EnsureMinimum(ref int value, int defaultValue, int minimum)
        {
            if (value >= minimum)
            {
                return false;
            }

            value = defaultValue;
            return true;
        }

        private bool EnsureSupportedUpgradeStats()
        {
            bool changed = false;
            supportedUpgradeStats ??= new List<StatId>();
            for (int i = supportedUpgradeStats.Count - 1; i >= 0; i--)
            {
                if (supportedUpgradeStats[i] == StatId.WeaponDamageMultiplier)
                {
                    supportedUpgradeStats[i] = StatId.WeaponFlatDamage;
                    changed = true;
                    continue;
                }

                if (!IsWeaponLocalUpgradeStat(supportedUpgradeStats[i]))
                {
                    supportedUpgradeStats.RemoveAt(i);
                    changed = true;
                }
            }

            var seen = new HashSet<StatId>();
            for (int i = supportedUpgradeStats.Count - 1; i >= 0; i--)
            {
                if (!seen.Add(supportedUpgradeStats[i]))
                {
                    supportedUpgradeStats.RemoveAt(i);
                    changed = true;
                }
            }

            if (supportedUpgradeStats.Count == 0)
            {
                supportedUpgradeStats.AddRange(DefaultSupportedUpgradeStatsForId());
                changed = true;
            }

            return changed;
        }

        private IReadOnlyList<StatId> DefaultSupportedUpgradeStatsForId()
        {
            return FirstPartyWeaponDefaults.SupportedStatsFor(weaponId);
        }

        public static bool IsWeaponLocalUpgradeStat(StatId statId)
        {
            return IsWeaponScopedStat(statId)
                && statId != StatId.WeaponDamageMultiplier;
        }

        public static bool IsWeaponScopedStat(StatId statId)
        {
            return Enum.IsDefined(typeof(StatId), statId)
                && string.Equals(StatMetadata.Get(statId).Category, "Weapon", StringComparison.Ordinal);
        }

        private static IEnumerable<StatId> WeaponLocalUpgradeStats()
        {
            return FirstPartyWeaponDefaults.WeaponLocalUpgradeStats();
        }
    }
}
