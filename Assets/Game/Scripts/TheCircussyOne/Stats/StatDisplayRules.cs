using UnityEngine;

namespace TheCircussyOne.Stats
{
    public enum StatValueDisplayMode
    {
        Applied,
        Preview
    }

    public static class StatDisplayRules
    {
        public static string FormatValue(StatDefinition stat, float value, StatValueDisplayMode mode = StatValueDisplayMode.Applied)
        {
            if (stat.ValueKind == StatValueKind.Integer)
            {
                return mode == StatValueDisplayMode.Preview
                    ? FormatNumber(value)
                    : StatRules.FloorToAppliedInt(value).ToString();
            }

            if (IsAttackSpeedStat(stat.Id))
            {
                return $"{Mathf.RoundToInt((1f + value) * 100f)}%";
            }

            if (IsPercentStat(stat.Id))
            {
                return $"{Mathf.RoundToInt(value * 100f)}%";
            }

            if (stat.Id == StatId.PlayerHpRegenPerMinute)
            {
                return $"{value:0.#}/min";
            }

            return value.ToString("0.##");
        }

        public static string FormatModifier(StatDefinition stat, StatModifierBucket bucket, float value)
        {
            if (IsAttackSpeedStat(stat.Id))
            {
                return $"{Signed(Mathf.RoundToInt(value * 100f))}%";
            }

            if (bucket == StatModifierBucket.AdditivePercent || bucket == StatModifierBucket.MultiplicativePercent || IsPercentStat(stat.Id))
            {
                return $"{Signed(Mathf.RoundToInt(value * 100f))}%";
            }

            if (stat.ValueKind == StatValueKind.Integer)
            {
                return Signed(value);
            }

            return value >= 0f ? $"+{value:0.##}" : value.ToString("0.##");
        }

        public static bool IsPercentStat(StatId id)
        {
            return id is StatId.PlayerMoveSpeedMultiplier
                or StatId.XpGainMultiplier
                or StatId.GlobalDamageMultiplier
                or StatId.CritChance
                or StatId.CritDamageMultiplier
                or StatId.ProjectileSpeedMultiplier
                or StatId.ProjectileAreaMultiplier
                or StatId.ProjectileDurationMultiplier
                or StatId.PlayerLifestealChance
                or StatId.PlayerKnockbackMultiplier
                or StatId.WeaponDamageMultiplier
                or StatId.WeaponProjectileSpeedMultiplier
                or StatId.WeaponProjectileSizeMultiplier
                or StatId.WeaponProjectileLifetimeMultiplier
                or StatId.WeaponSplashRadiusMultiplier
                or StatId.WeaponRangeMultiplier
                or StatId.WeaponAccuracyMultiplier
                or StatId.WeaponKnockbackMultiplier;
        }

        public static bool IsAttackSpeedStat(StatId id)
        {
            return id is StatId.GlobalWeaponHaste or StatId.WeaponAttackSpeed;
        }

        private static string Signed(int value)
        {
            return value >= 0 ? $"+{value}" : value.ToString();
        }

        private static string Signed(float value)
        {
            string text = FormatNumber(value);
            return value >= 0f ? $"+{text}" : text;
        }

        private static string FormatNumber(float value)
        {
            return value.ToString("0.##");
        }
    }
}
