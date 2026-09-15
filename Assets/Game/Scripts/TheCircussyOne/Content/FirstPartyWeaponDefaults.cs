using System;
using System.Collections.Generic;
using TheCircussyOne.Stats;

namespace TheCircussyOne.Content
{
    public static partial class FirstPartyWeaponDefaults
    {
        public const string CannonId = "cannon";
        public const string KnifeFanId = "knife_fan";
        public const string SpotlightBoltId = "spotlight_bolt";
        public const string FireHoopId = "fire_hoop";
        public const string JugglingBallId = "juggling_ball";

        private static readonly StatId[] DefaultSupportedStats =
        {
            StatId.WeaponFlatDamage,
            StatId.WeaponAttackSpeed,
            StatId.WeaponProjectileSpeedMultiplier,
            StatId.WeaponProjectileSizeMultiplier,
            StatId.WeaponProjectileLifetimeMultiplier
        };

        private static readonly StatId[] KnifeFanSupportedStats =
        {
            StatId.WeaponProjectileCount,
            StatId.WeaponAccuracyMultiplier,
            StatId.WeaponAttackSpeed,
            StatId.WeaponProjectileSpeedMultiplier,
            StatId.WeaponFlatDamage,
            StatId.WeaponProjectileLifetimeMultiplier
        };

        private static readonly StatId[] FireHoopSupportedStats =
        {
            StatId.WeaponFlatDamage,
            StatId.WeaponAttackSpeed,
            StatId.WeaponProjectileCount,
            StatId.WeaponProjectileSizeMultiplier,
            StatId.WeaponRangeMultiplier
        };

        private static readonly StatId[] CannonSupportedStats =
        {
            StatId.WeaponSplashRadiusMultiplier,
            StatId.WeaponFlatDamage,
            StatId.WeaponProjectileSizeMultiplier,
            StatId.WeaponAttackSpeed,
            StatId.WeaponProjectileSpeedMultiplier
        };

        private static readonly StatId[] SpotlightBoltSupportedStats =
        {
            StatId.WeaponChain,
            StatId.WeaponFlatDamage,
            StatId.WeaponAttackSpeed,
            StatId.WeaponProjectileSpeedMultiplier,
            StatId.WeaponProjectileSizeMultiplier,
            StatId.WeaponProjectileLifetimeMultiplier
        };

        private static readonly StatId[] JugglingBallSupportedStats =
        {
            StatId.WeaponBounce,
            StatId.WeaponFlatDamage,
            StatId.WeaponAttackSpeed,
            StatId.WeaponAccuracyMultiplier,
            StatId.WeaponProjectileSpeedMultiplier,
            StatId.WeaponProjectileSizeMultiplier,
            StatId.WeaponProjectileLifetimeMultiplier
        };

        public static IReadOnlyList<FirstPartyWeaponSpec> All => Specs;
        public static FirstPartyWeaponSpec DefaultWeapon => GetRequired(JugglingBallId);

        public static bool TryGet(string weaponId, out FirstPartyWeaponSpec spec)
        {
            for (int i = 0; i < Specs.Length; i++)
            {
                if (string.Equals(Specs[i].Id, weaponId, StringComparison.Ordinal))
                {
                    spec = Specs[i];
                    return true;
                }
            }

            spec = DefaultWeapon;
            return false;
        }

        public static FirstPartyWeaponSpec GetOrDefault(string weaponId)
        {
            return TryGet(weaponId, out FirstPartyWeaponSpec spec) ? spec : DefaultWeapon;
        }

        public static IReadOnlyList<StatId> SupportedStatsFor(string weaponId)
        {
            return GetOrDefault(weaponId).SupportedUpgradeStats;
        }

        private static FirstPartyWeaponSpec GetRequired(string weaponId)
        {
            for (int i = 0; i < Specs.Length; i++)
            {
                if (string.Equals(Specs[i].Id, weaponId, StringComparison.Ordinal))
                {
                    return Specs[i];
                }
            }

            return Specs.Length > 0 ? Specs[0] : default;
        }

        public static IEnumerable<StatId> WeaponLocalUpgradeStats()
        {
            yield return StatId.WeaponFlatDamage;
            yield return StatId.WeaponAttackSpeed;
            yield return StatId.WeaponProjectileCount;
            yield return StatId.WeaponProjectileSpeedMultiplier;
            yield return StatId.WeaponProjectileSizeMultiplier;
            yield return StatId.WeaponProjectileLifetimeMultiplier;
            yield return StatId.WeaponSplashRadiusMultiplier;
            yield return StatId.WeaponRangeMultiplier;
            yield return StatId.WeaponBounce;
            yield return StatId.WeaponAccuracyMultiplier;
            yield return StatId.WeaponChain;
        }
    }
}
