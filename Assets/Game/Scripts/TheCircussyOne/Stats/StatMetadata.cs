using System;
using System.Collections.Generic;

namespace TheCircussyOne.Stats
{
    public static class StatMetadata
    {
        private static readonly Dictionary<StatId, StatDefinition> Definitions = new()
        {
            [StatId.PlayerMaxHealth] = new StatDefinition(StatId.PlayerMaxHealth, "Max HP", "Player", StatValueKind.Integer, StatStackingPolicy.IntegerAdditive, 1f),
            [StatId.PlayerArmor] = new StatDefinition(StatId.PlayerArmor, "Armor", "Player", StatValueKind.Float, StatStackingPolicy.FlatAdditive, -1000f, 1000f),
            [StatId.PlayerMoveSpeedMultiplier] = new StatDefinition(StatId.PlayerMoveSpeedMultiplier, "Movement Speed", "Player", StatValueKind.Float, StatStackingPolicy.AdditivePercent, 0f),
            [StatId.PickupMagnetRadius] = new StatDefinition(StatId.PickupMagnetRadius, "Pickup Range", "Pickup", StatValueKind.Float, StatStackingPolicy.FlatAdditive, 0f),
            [StatId.PickupCollectRadius] = new StatDefinition(StatId.PickupCollectRadius, "Pickup Collect Radius", "Pickup", StatValueKind.Float, StatStackingPolicy.FlatAdditive, 0f),
            [StatId.XpGainMultiplier] = new StatDefinition(StatId.XpGainMultiplier, "XP Gain", "Progression", StatValueKind.Float, StatStackingPolicy.AdditivePercent, 0f, 10f),
            [StatId.Luck] = new StatDefinition(StatId.Luck, "Luck", "Rewards", StatValueKind.Float, StatStackingPolicy.FlatAdditive, 0f, 1000f),
            [StatId.GlobalDamageMultiplier] = new StatDefinition(StatId.GlobalDamageMultiplier, "Damage", "Combat", StatValueKind.Float, StatStackingPolicy.AdditivePercent, 0f),
            [StatId.GlobalWeaponHaste] = new StatDefinition(StatId.GlobalWeaponHaste, "Attack Speed", "Combat", StatValueKind.Float, StatStackingPolicy.PercentBonusAdditive, -0.9f),
            [StatId.CritChance] = new StatDefinition(StatId.CritChance, "Crit Chance", "Combat", StatValueKind.Float, StatStackingPolicy.FlatAdditive, 0f, 1f),
            [StatId.CritDamageMultiplier] = new StatDefinition(StatId.CritDamageMultiplier, "Crit Damage", "Combat", StatValueKind.Float, StatStackingPolicy.FlatAdditive, 1f, 10f),
            [StatId.ProjectileSpeedMultiplier] = new StatDefinition(StatId.ProjectileSpeedMultiplier, "Projectile Speed", "Combat", StatValueKind.Float, StatStackingPolicy.AdditivePercent, 0f),
            [StatId.ProjectileAreaMultiplier] = new StatDefinition(StatId.ProjectileAreaMultiplier, "Projectile Size", "Combat", StatValueKind.Float, StatStackingPolicy.AdditivePercent, 0.1f, 10f),
            [StatId.ProjectileDurationMultiplier] = new StatDefinition(StatId.ProjectileDurationMultiplier, "Projectile Lifetime", "Combat", StatValueKind.Float, StatStackingPolicy.AdditivePercent, 0.1f, 10f),
            [StatId.ProjectileCount] = new StatDefinition(StatId.ProjectileCount, "Projectile Count", "Combat", StatValueKind.Integer, StatStackingPolicy.IntegerAdditive, 0f, 8f),
            [StatId.Pierce] = new StatDefinition(StatId.Pierce, "Pierce", "Combat", StatValueKind.Integer, StatStackingPolicy.IntegerAdditive, 0f, 99f),
            [StatId.Bounce] = new StatDefinition(StatId.Bounce, "Bounce", "Combat", StatValueKind.Integer, StatStackingPolicy.IntegerAdditive, 0f, 20f),
            [StatId.Chain] = new StatDefinition(StatId.Chain, "Chain", "Combat", StatValueKind.Integer, StatStackingPolicy.IntegerAdditive, 0f, 20f),
            [StatId.PlayerHpRegenPerMinute] = new StatDefinition(StatId.PlayerHpRegenPerMinute, "HP Regen", "Player", StatValueKind.Float, StatStackingPolicy.FlatAdditive, 0f),
            [StatId.PlayerLifestealChance] = new StatDefinition(StatId.PlayerLifestealChance, "Lifesteal", "Player", StatValueKind.Float, StatStackingPolicy.FlatAdditive, 0f, 1f),
            [StatId.PlayerKnockbackMultiplier] = new StatDefinition(StatId.PlayerKnockbackMultiplier, "Knockback", "Combat", StatValueKind.Float, StatStackingPolicy.AdditivePercent, 0f),
            [StatId.PlayerExtraJumps] = new StatDefinition(StatId.PlayerExtraJumps, "Extra Jumps", "Player", StatValueKind.Integer, StatStackingPolicy.IntegerAdditive, 0f, 8f),
            [StatId.WeaponDamageMultiplier] = new StatDefinition(StatId.WeaponDamageMultiplier, "Weapon Damage % (Legacy)", "Weapon", StatValueKind.Float, StatStackingPolicy.AdditivePercent, 0f),
            [StatId.WeaponAttackSpeed] = new StatDefinition(StatId.WeaponAttackSpeed, "Weapon Attack Speed", "Weapon", StatValueKind.Float, StatStackingPolicy.PercentBonusAdditive, -0.9f),
            [StatId.WeaponProjectileCount] = new StatDefinition(StatId.WeaponProjectileCount, "Weapon Projectile Count", "Weapon", StatValueKind.Integer, StatStackingPolicy.IntegerAdditive, 0f, 16f),
            [StatId.WeaponProjectileSpeedMultiplier] = new StatDefinition(StatId.WeaponProjectileSpeedMultiplier, "Weapon Projectile Speed", "Weapon", StatValueKind.Float, StatStackingPolicy.AdditivePercent, 0f),
            [StatId.WeaponProjectileSizeMultiplier] = new StatDefinition(StatId.WeaponProjectileSizeMultiplier, "Weapon Projectile Size", "Weapon", StatValueKind.Float, StatStackingPolicy.AdditivePercent, 0.1f, 10f),
            [StatId.WeaponProjectileLifetimeMultiplier] = new StatDefinition(StatId.WeaponProjectileLifetimeMultiplier, "Weapon Projectile Lifetime", "Weapon", StatValueKind.Float, StatStackingPolicy.AdditivePercent, 0.1f, 10f),
            [StatId.WeaponSplashRadiusMultiplier] = new StatDefinition(StatId.WeaponSplashRadiusMultiplier, "Weapon Splash Radius", "Weapon", StatValueKind.Float, StatStackingPolicy.AdditivePercent, 0f),
            [StatId.WeaponRangeMultiplier] = new StatDefinition(StatId.WeaponRangeMultiplier, "Weapon Range", "Weapon", StatValueKind.Float, StatStackingPolicy.AdditivePercent, 0f),
            [StatId.WeaponPierce] = new StatDefinition(StatId.WeaponPierce, "Weapon Pierce", "Weapon", StatValueKind.Integer, StatStackingPolicy.IntegerAdditive, 0f, 99f),
            [StatId.WeaponBounce] = new StatDefinition(StatId.WeaponBounce, "Weapon Bounce", "Weapon", StatValueKind.Integer, StatStackingPolicy.IntegerAdditive, 0f, 20f),
            [StatId.WeaponAccuracyMultiplier] = new StatDefinition(StatId.WeaponAccuracyMultiplier, "Weapon Accuracy", "Weapon", StatValueKind.Float, StatStackingPolicy.AdditivePercent, 0.1f, 10f),
            [StatId.WeaponKnockbackMultiplier] = new StatDefinition(StatId.WeaponKnockbackMultiplier, "Weapon Knockback", "Weapon", StatValueKind.Float, StatStackingPolicy.AdditivePercent, 0f),
            [StatId.WeaponChain] = new StatDefinition(StatId.WeaponChain, "Weapon Chain", "Weapon", StatValueKind.Integer, StatStackingPolicy.IntegerAdditive, 0f, 20f),
            [StatId.WeaponFlatDamage] = new StatDefinition(StatId.WeaponFlatDamage, "Weapon Damage", "Weapon", StatValueKind.Integer, StatStackingPolicy.IntegerAdditive, 0f)
        };

        public static IReadOnlyDictionary<StatId, StatDefinition> All => Definitions;

        public static StatDefinition Get(StatId id)
        {
            if (!Definitions.TryGetValue(id, out StatDefinition definition))
            {
                throw new ArgumentOutOfRangeException(nameof(id), id, "Unknown stat id.");
            }

            return definition;
        }
    }
}
