using System.Collections.Generic;
using UnityEngine;
using TheCircussyOne.Content;

namespace TheCircussyOne.Stats
{
    public static class WeaponStatRules
    {
        public static int Damage(WeaponDefinition weapon, RunStats stats, IReadOnlyList<StatModifier> weaponModifiers = null)
        {
            if (weapon == null)
            {
                return 0;
            }

            var modifiers = new List<StatModifier>();
            float globalMultiplier = stats != null ? stats.GetFloat(StatId.GlobalDamageMultiplier) : 1f;
            if (!Mathf.Approximately(globalMultiplier, 1f))
            {
                modifiers.Add(new StatModifier(StatId.GlobalDamageMultiplier, StatModifierBucket.GlobalPercent, globalMultiplier - 1f, "run_stats"));
            }

            AddWeaponDamageModifiers(weaponModifiers, modifiers);
            float damage = StatRules.EvaluateDamage(weapon.projectileDamage, modifiers);
            return Mathf.Max(0, StatRules.FloorToAppliedInt(damage));
        }

        public static float ProjectileSpeed(WeaponDefinition weapon, RunStats stats, IReadOnlyList<StatModifier> weaponModifiers = null)
        {
            if (weapon == null)
            {
                return 0f;
            }

            float multiplier = stats != null ? stats.GetFloat(StatId.ProjectileSpeedMultiplier) : 1f;
            float baseSpeed = weapon.projectileSpeed * multiplier;
            return Mathf.Max(0f, StatRules.EvaluateAdditivePercent(baseSpeed, GatherWeaponMultiplierModifiers(weaponModifiers, StatId.WeaponProjectileSpeedMultiplier, StatId.ProjectileSpeedMultiplier)));
        }

        public static int ProjectileCount(WeaponDefinition weapon, RunStats stats, IReadOnlyList<StatModifier> weaponModifiers = null)
        {
            if (weapon == null)
            {
                return 0;
            }

            int baseCount = Mathf.Max(1, weapon.baseProjectileCount);
            int globalBonus = stats != null ? stats.GetInt(StatId.ProjectileCount) : 0;
            int localBonus = StatRules.FloorToAppliedInt(StatRules.EvaluateFlatAdditive(
                0f,
                GatherWeaponModifiers(weaponModifiers, StatId.WeaponProjectileCount)));
            return Mathf.Clamp(baseCount + globalBonus + localBonus, 1, Mathf.Max(baseCount, weapon.maxProjectileCount));
        }

        public static float SpreadAngle(WeaponDefinition weapon, RunStats stats, IReadOnlyList<StatModifier> weaponModifiers = null)
        {
            if (weapon == null)
            {
                return 0f;
            }

            return Mathf.Max(0f, weapon.baseSpreadAngleDegrees);
        }

        public static float AimErrorAngle(WeaponDefinition weapon, RunStats stats, IReadOnlyList<StatModifier> weaponModifiers = null)
        {
            if (weapon == null)
            {
                return 0f;
            }

            float accuracy = 1f;
            if (weapon.accuracyAffectsAimError)
            {
                accuracy = StatRules.EvaluateAdditivePercent(
                    1f,
                    GatherWeaponMultiplierModifiers(weaponModifiers, StatId.WeaponAccuracyMultiplier, null));
            }

            return StatRules.SpreadAngle(weapon.baseAimErrorDegrees, accuracy);
        }

        public static int BounceCount(WeaponDefinition weapon, RunStats stats, IReadOnlyList<StatModifier> weaponModifiers = null)
        {
            if (weapon == null || !weapon.bounceEnabled)
            {
                return 0;
            }

            int baseCount = Mathf.Max(0, weapon.baseBounceCount);
            int globalBonus = stats != null ? stats.GetInt(StatId.Bounce) : 0;
            int localBonus = StatRules.FloorToAppliedInt(StatRules.EvaluateFlatAdditive(
                0f,
                GatherWeaponModifiers(weaponModifiers, StatId.WeaponBounce)));
            return Mathf.Clamp(baseCount + globalBonus + localBonus, 0, Mathf.Max(baseCount, weapon.maxBounceCount));
        }

        public static int ChainCount(WeaponDefinition weapon, RunStats stats, IReadOnlyList<StatModifier> weaponModifiers = null)
        {
            if (weapon == null || !weapon.chainEnabled)
            {
                return 0;
            }

            int baseCount = Mathf.Max(0, weapon.baseChainCount);
            int globalBonus = stats != null ? stats.GetInt(StatId.Chain) : 0;
            int localBonus = StatRules.FloorToAppliedInt(StatRules.EvaluateFlatAdditive(
                0f,
                GatherWeaponModifiers(weaponModifiers, StatId.WeaponChain)));
            return Mathf.Clamp(baseCount + globalBonus + localBonus, 0, Mathf.Max(baseCount, weapon.maxChainCount));
        }

        public static float SplashRadius(WeaponDefinition weapon, RunStats stats, IReadOnlyList<StatModifier> weaponModifiers = null)
        {
            if (weapon == null || !weapon.explosiveEnabled)
            {
                return 0f;
            }

            float baseRadius = Mathf.Max(0f, weapon.baseSplashRadius);
            float multiplier = StatRules.EvaluateAdditivePercent(
                1f,
                GatherWeaponMultiplierModifiers(weaponModifiers, StatId.WeaponSplashRadiusMultiplier, null));
            float radius = StatRules.MultiplyPositive(baseRadius, multiplier);
            return Mathf.Clamp(radius, 0f, Mathf.Max(baseRadius, weapon.maxSplashRadius));
        }

        public static int OrbitCount(WeaponDefinition weapon, RunStats stats, IReadOnlyList<StatModifier> weaponModifiers = null)
        {
            if (weapon == null || !weapon.orbitEnabled)
            {
                return 0;
            }

            int baseCount = Mathf.Max(1, weapon.baseOrbitCount);
            int globalBonus = stats != null ? stats.GetInt(StatId.ProjectileCount) : 0;
            int localBonus = StatRules.FloorToAppliedInt(StatRules.EvaluateFlatAdditive(
                0f,
                GatherWeaponModifiers(weaponModifiers, StatId.WeaponProjectileCount)));
            return Mathf.Clamp(baseCount + globalBonus + localBonus, 1, Mathf.Max(baseCount, weapon.maxOrbitCount));
        }

        public static float OrbitRadius(WeaponDefinition weapon, IReadOnlyList<StatModifier> weaponModifiers = null)
        {
            if (weapon == null || !weapon.orbitEnabled)
            {
                return 0f;
            }

            float baseRadius = Mathf.Max(0f, weapon.orbitRadius);
            float multiplier = StatRules.EvaluateAdditivePercent(
                1f,
                GatherWeaponMultiplierModifiers(weaponModifiers, StatId.WeaponRangeMultiplier, null));
            float radius = StatRules.MultiplyPositive(baseRadius, multiplier);
            return Mathf.Clamp(radius, 0f, Mathf.Max(baseRadius, weapon.maxOrbitRadius));
        }

        public static float OrbitHitRadius(WeaponDefinition weapon, RunStats stats, IReadOnlyList<StatModifier> weaponModifiers = null)
        {
            if (weapon == null || !weapon.orbitEnabled)
            {
                return 0f;
            }

            float radius = StatRules.MultiplyPositive(weapon.orbitHitRadius, ProjectileSizeMultiplier(stats, weaponModifiers));
            return Mathf.Clamp(radius, 0f, Mathf.Max(weapon.orbitHitRadius, weapon.maxOrbitHitRadius));
        }

        public static int OrbitAreaDamage(WeaponDefinition weapon, RunStats stats, IReadOnlyList<StatModifier> weaponModifiers = null)
        {
            if (weapon == null || !weapon.orbitEnabled || !weapon.orbitAreaDamageEnabled)
            {
                return 0;
            }

            int baseDamage = Damage(weapon, stats, weaponModifiers);
            if (baseDamage <= 0 || weapon.orbitAreaDamageMultiplier <= 0f)
            {
                return 0;
            }

            return Mathf.Max(1, Mathf.RoundToInt(baseDamage * Mathf.Clamp01(weapon.orbitAreaDamageMultiplier)));
        }

        public static float OrbitAreaHitInterval(WeaponDefinition weapon, RunStats stats, IReadOnlyList<StatModifier> weaponModifiers = null)
        {
            if (weapon == null || !weapon.orbitEnabled || !weapon.orbitAreaDamageEnabled)
            {
                return float.PositiveInfinity;
            }

            float attackSpeed = AttackSpeedMultiplier(stats, weaponModifiers);
            return StatRules.EvaluateCooldown(
                Mathf.Max(0.01f, weapon.orbitAreaHitIntervalSeconds),
                0f,
                Mathf.Max(0f, attackSpeed - 1f),
                Mathf.Max(0.01f, weapon.minimumFireIntervalSeconds));
        }

        public static float ProjectileSizeMultiplier(RunStats stats, IReadOnlyList<StatModifier> weaponModifiers = null)
        {
            float playerMultiplier = stats != null ? stats.GetFloat(StatId.ProjectileAreaMultiplier) : 1f;
            return Mathf.Max(0f, StatRules.EvaluateAdditivePercent(
                playerMultiplier,
                GatherWeaponMultiplierModifiers(weaponModifiers, StatId.WeaponProjectileSizeMultiplier, StatId.ProjectileAreaMultiplier)));
        }

        public static float ProjectileHitRadius(WeaponDefinition weapon, RunStats stats, IReadOnlyList<StatModifier> weaponModifiers = null)
        {
            if (weapon == null)
            {
                return 0f;
            }

            return StatRules.MultiplyPositive(weapon.projectileHitRadius, ProjectileSizeMultiplier(stats, weaponModifiers));
        }

        public static float ProjectileVisualScaleMultiplier(RunStats stats, IReadOnlyList<StatModifier> weaponModifiers = null)
        {
            return ProjectileSizeMultiplier(stats, weaponModifiers);
        }

        public static float ProjectileLifetime(WeaponDefinition weapon, RunStats stats, IReadOnlyList<StatModifier> weaponModifiers = null)
        {
            if (weapon == null)
            {
                return 0f;
            }

            float playerMultiplier = stats != null ? stats.GetFloat(StatId.ProjectileDurationMultiplier) : 1f;
            float multiplier = StatRules.EvaluateAdditivePercent(
                playerMultiplier,
                GatherWeaponMultiplierModifiers(weaponModifiers, StatId.WeaponProjectileLifetimeMultiplier, StatId.ProjectileDurationMultiplier));
            return StatRules.MultiplyPositive(weapon.projectileLifetimeSeconds, multiplier);
        }

        public static float Range(WeaponDefinition weapon, IReadOnlyList<StatModifier> weaponModifiers = null)
        {
            if (weapon == null)
            {
                return 0f;
            }

            float multiplier = StatRules.EvaluateAdditivePercent(
                1f,
                GatherWeaponMultiplierModifiers(weaponModifiers, StatId.WeaponRangeMultiplier, null));
            return StatRules.MultiplyPositive(weapon.weaponRange, multiplier);
        }

        public static float AttackSpeedMultiplier(RunStats stats, IReadOnlyList<StatModifier> weaponModifiers = null)
        {
            float global = stats != null ? StatRules.AttackSpeedMultiplierFromBonus(stats.GetFloat(StatId.GlobalWeaponHaste)) : 1f;
            return Mathf.Max(0f, global * WeaponLocalAttackSpeedMultiplier(weaponModifiers));
        }

        public static float FireInterval(WeaponDefinition weapon, RunStats stats, IReadOnlyList<StatModifier> weaponModifiers = null)
        {
            if (weapon == null)
            {
                return 0.12f;
            }

            float attackSpeed = AttackSpeedMultiplier(stats, weaponModifiers);
            return StatRules.EvaluateCooldown(
                Mathf.Max(0f, weapon.baseFireIntervalSeconds),
                0f,
                Mathf.Max(0f, attackSpeed - 1f),
                weapon.minimumFireIntervalSeconds);
        }

        private static void AddWeaponDamageModifiers(IReadOnlyList<StatModifier> source, List<StatModifier> target)
        {
            if (source == null)
            {
                return;
            }

            for (int i = 0; i < source.Count; i++)
            {
                StatModifier modifier = source[i];
                if (modifier.StatId != StatId.GlobalDamageMultiplier
                    && modifier.StatId != StatId.WeaponDamageMultiplier
                    && modifier.StatId != StatId.WeaponFlatDamage)
                {
                    continue;
                }

                StatModifierBucket bucket = modifier.StatId == StatId.WeaponFlatDamage
                    ? StatModifierBucket.Flat
                    : modifier.Bucket switch
                {
                    StatModifierBucket.AdditivePercent => StatModifierBucket.WeaponLocalPercent,
                    StatModifierBucket.GlobalPercent => StatModifierBucket.WeaponLocalPercent,
                    _ => modifier.Bucket
                };
                target.Add(new StatModifier(modifier.StatId, bucket, modifier.Value, modifier.SourceId));
            }
        }

        private static List<StatModifier> GatherWeaponModifiers(IReadOnlyList<StatModifier> modifiers, StatId statId)
        {
            var matches = new List<StatModifier>();
            if (modifiers == null)
            {
                return matches;
            }

            for (int i = 0; i < modifiers.Count; i++)
            {
                if (modifiers[i].StatId == statId)
                {
                    matches.Add(modifiers[i]);
                }
            }

            return matches;
        }

        private static List<StatModifier> GatherWeaponMultiplierModifiers(IReadOnlyList<StatModifier> modifiers, StatId statId, StatId? compatibilityStatId)
        {
            var matches = new List<StatModifier>();
            if (modifiers == null)
            {
                return matches;
            }

            for (int i = 0; i < modifiers.Count; i++)
            {
                StatModifier modifier = modifiers[i];
                if (modifier.StatId == statId || (compatibilityStatId.HasValue && modifier.StatId == compatibilityStatId.Value))
                {
                    matches.Add(modifier);
                }
            }

            return matches;
        }

        private static float WeaponLocalAttackSpeedMultiplier(IReadOnlyList<StatModifier> modifiers)
        {
            if (modifiers == null)
            {
                return 1f;
            }

            float attackSpeedBonus = 0f;
            float additivePercent = 0f;
            float multiplicativePercent = 1f;
            for (int i = 0; i < modifiers.Count; i++)
            {
                StatModifier modifier = modifiers[i];
                if (modifier.StatId != StatId.GlobalWeaponHaste && modifier.StatId != StatId.WeaponAttackSpeed)
                {
                    continue;
                }

                switch (modifier.Bucket)
                {
                    case StatModifierBucket.Flat:
                        attackSpeedBonus += modifier.Value;
                        break;
                    case StatModifierBucket.AdditivePercent:
                        additivePercent += modifier.Value;
                        break;
                    case StatModifierBucket.MultiplicativePercent:
                        multiplicativePercent *= modifier.Value;
                        break;
                }
            }

            return Mathf.Max(0f, (1f + Mathf.Max(0f, attackSpeedBonus)) * Mathf.Max(0f, 1f + additivePercent) * Mathf.Max(0f, multiplicativePercent));
        }
    }
}
