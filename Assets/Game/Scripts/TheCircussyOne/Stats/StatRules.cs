using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheCircussyOne.Stats
{
    public static class StatRules
    {
        private const float IntegerEpsilon = 0.0001f;

        public static float EvaluateFloat(StatDefinition definition, float baseValue, IReadOnlyList<StatModifier> modifiers)
        {
            float value = definition.StackingPolicy switch
            {
                StatStackingPolicy.FlatAdditive => EvaluateFlatAdditive(baseValue, modifiers),
                StatStackingPolicy.IntegerAdditive => EvaluateFlatAdditive(baseValue, modifiers),
                StatStackingPolicy.AdditivePercent => EvaluateAdditivePercent(baseValue, modifiers),
                StatStackingPolicy.PercentBonusAdditive => EvaluatePercentBonusAdditive(baseValue, modifiers),
                StatStackingPolicy.DamageBracket => EvaluateDamage(baseValue, modifiers),
                _ => baseValue
            };

            return Clamp(value, definition.MinValue, definition.MaxValue);
        }

        public static int EvaluateInt(StatDefinition definition, float baseValue, IReadOnlyList<StatModifier> modifiers)
        {
            return FloorToAppliedInt(EvaluateFloat(definition, baseValue, modifiers));
        }

        public static int FloorToAppliedInt(float value)
        {
            return Mathf.FloorToInt(value + IntegerEpsilon);
        }

        public static float EvaluateDamage(float baseDamage, IReadOnlyList<StatModifier> modifiers)
        {
            float value = baseDamage + Sum(modifiers, StatModifierBucket.Flat);
            value *= 1f + Sum(modifiers, StatModifierBucket.WeaponLocalPercent);
            value *= 1f + Sum(modifiers, StatModifierBucket.GlobalPercent);
            value *= 1f + Sum(modifiers, StatModifierBucket.TagSpecializationPercent);
            value *= 1f + Sum(modifiers, StatModifierBucket.ConditionalPercent);
            value *= Product(modifiers, StatModifierBucket.CritMultiplier);
            value *= Product(modifiers, StatModifierBucket.RareUniqueMultiplier);
            return Mathf.Max(0f, value);
        }

        public static float EvaluateCooldown(float baseCooldown, float flatSeconds, float attackSpeedBonus, float minimumCooldown)
        {
            float adjustedBase = Mathf.Max(0f, baseCooldown + flatSeconds);
            float divisor = 1f + Mathf.Max(0f, attackSpeedBonus);
            return Mathf.Max(minimumCooldown, adjustedBase / divisor);
        }

        public static int ApplyArmorDamageReduction(int incomingDamage, float armor)
        {
            if (incomingDamage <= 0)
            {
                return 0;
            }

            float reducedDamage = armor >= 0f
                ? incomingDamage * 100f / (100f + armor)
                : incomingDamage * (1f + Mathf.Abs(armor) / 100f);
            return Mathf.Max(1, Mathf.RoundToInt(reducedDamage));
        }

        public static float EffectiveLuckMultiplier(float luck)
        {
            float safeLuck = Mathf.Max(0f, luck);
            return safeLuck <= 0f ? 0f : (2f * safeLuck) / (safeLuck + 100f);
        }

        public static int ApplyXpGain(int baseAmount, float xpGainMultiplier, ref float fractionalRemainder)
        {
            if (baseAmount <= 0)
            {
                return 0;
            }

            float safeMultiplier = Mathf.Max(0f, xpGainMultiplier);
            float total = baseAmount * safeMultiplier + Mathf.Max(0f, fractionalRemainder);
            int gained = Mathf.FloorToInt(total + 0.0001f);
            fractionalRemainder = Mathf.Max(0f, total - gained);
            return Mathf.Max(0, gained);
        }

        public static float HpRegenPerSecond(float hpRegenPerMinute)
        {
            return Mathf.Max(0f, hpRegenPerMinute) / 60f;
        }

        public static float AttackSpeedMultiplierFromBonus(float attackSpeedBonus)
        {
            return Mathf.Max(0.1f, 1f + attackSpeedBonus);
        }

        public static float MultiplyPositive(float baseValue, params float[] multipliers)
        {
            float value = Mathf.Max(0f, baseValue);
            if (multipliers == null)
            {
                return value;
            }

            for (int i = 0; i < multipliers.Length; i++)
            {
                value *= Mathf.Max(0f, multipliers[i]);
            }

            return value;
        }

        public static float SpreadAngle(float baseSpreadAngle, float accuracyMultiplier)
        {
            float safeAccuracy = Mathf.Max(0.01f, accuracyMultiplier);
            return Mathf.Max(0f, baseSpreadAngle) / safeAccuracy;
        }

        public static float EvaluateAdditivePercent(float baseValue, IReadOnlyList<StatModifier> modifiers)
        {
            float value = baseValue + Sum(modifiers, StatModifierBucket.Flat);
            value *= 1f + Sum(modifiers, StatModifierBucket.AdditivePercent);
            value *= Product(modifiers, StatModifierBucket.MultiplicativePercent);
            return value;
        }

        public static float EvaluateFlatAdditive(float baseValue, IReadOnlyList<StatModifier> modifiers)
        {
            return baseValue + Sum(modifiers, StatModifierBucket.Flat);
        }

        public static float EvaluatePercentBonusAdditive(float baseValue, IReadOnlyList<StatModifier> modifiers)
        {
            return baseValue
                + Sum(modifiers, StatModifierBucket.Flat)
                + Sum(modifiers, StatModifierBucket.AdditivePercent);
        }

        public static float Sum(IReadOnlyList<StatModifier> modifiers, StatModifierBucket bucket)
        {
            if (modifiers == null)
            {
                return 0f;
            }

            float total = 0f;
            for (int i = 0; i < modifiers.Count; i++)
            {
                if (modifiers[i].Bucket == bucket)
                {
                    total += modifiers[i].Value;
                }
            }

            return total;
        }

        private static float Product(IReadOnlyList<StatModifier> modifiers, StatModifierBucket bucket)
        {
            if (modifiers == null)
            {
                return 1f;
            }

            float value = 1f;
            bool hasModifier = false;
            for (int i = 0; i < modifiers.Count; i++)
            {
                if (modifiers[i].Bucket != bucket)
                {
                    continue;
                }

                hasModifier = true;
                value *= modifiers[i].Value;
            }

            return hasModifier ? value : 1f;
        }

        private static float Clamp(float value, float minValue, float maxValue)
        {
            if (!float.IsNegativeInfinity(minValue))
            {
                value = Math.Max(minValue, value);
            }

            if (!float.IsPositiveInfinity(maxValue))
            {
                value = Math.Min(maxValue, value);
            }

            return value;
        }
    }
}
