using System;
using System.Collections.Generic;
using TheCircussyOne.Stats;

namespace TheCircussyOne.Content
{
    public static class ContentStatValidationRules
    {
        public static void ValidateUpgradeModifiers(
            IReadOnlyList<UpgradeStatModifierDefinition> modifiers,
            List<ContentValidationIssue> issues,
            string code,
            string ownerId,
            string ownerLabel)
        {
            if (modifiers == null)
            {
                return;
            }

            for (int i = 0; i < modifiers.Count; i++)
            {
                UpgradeStatModifierDefinition modifier = modifiers[i];
                if (!IsValid(modifier.statId, modifier.bucket))
                {
                    issues.Add(new ContentValidationIssue(
                        code,
                        ContentValidationSeverity.Error,
                        $"{ownerLabel} '{ownerId}' has an invalid stat modifier at index {i}: {DescribeInvalidModifier(modifier.statId, modifier.bucket)}.",
                        ownerId));
                }
            }
        }

        public static void ValidateItemModifiers(
            IReadOnlyList<ItemStatModifierDefinition> modifiers,
            List<ContentValidationIssue> issues,
            string code,
            string ownerId,
            string ownerLabel)
        {
            if (modifiers == null)
            {
                return;
            }

            for (int i = 0; i < modifiers.Count; i++)
            {
                ItemStatModifierDefinition modifier = modifiers[i];
                if (!IsValid(modifier.statId, modifier.bucket))
                {
                    issues.Add(new ContentValidationIssue(
                        code,
                        ContentValidationSeverity.Error,
                        $"{ownerLabel} '{ownerId}' has an invalid stat modifier at index {i}: {DescribeInvalidModifier(modifier.statId, modifier.bucket)}.",
                        ownerId));
                }
            }
        }

        public static string DescribeInvalidModifier(StatId statId, StatModifierBucket bucket)
        {
            if (!Enum.IsDefined(typeof(StatId), statId))
            {
                return $"invalid stat id '{(int)statId}'";
            }

            if (!Enum.IsDefined(typeof(StatModifierBucket), bucket))
            {
                return $"unsupported modifier bucket '{(int)bucket}'";
            }

            if (bucket == StatModifierBucket.Haste || bucket == StatModifierBucket.FlatSeconds)
            {
                return $"unsupported legacy bucket '{bucket}'";
            }

            if (statId == StatId.WeaponDamageMultiplier)
            {
                return "'Weapon Damage % (Legacy)' is an internal compatibility stat. Use flat 'Weapon Damage' for weapon upgrades or global 'Damage' for talents and performers.";
            }

            if (!IsAuthoredBucket(bucket))
            {
                return $"unsupported internal bucket '{bucket}'";
            }

            StatModifierBucket expected = DefaultAuthoredBucket(statId);
            if (bucket != expected)
            {
                return $"'{DisplayStatName(statId)}' requires '{expected}' modifiers, not '{bucket}'";
            }

            return "unsupported stat modifier";
        }

        public static bool IsValid(StatId statId, StatModifierBucket bucket)
        {
            return Enum.IsDefined(typeof(StatId), statId)
                && statId != StatId.WeaponDamageMultiplier
                && IsAuthoredBucket(bucket)
                && bucket == DefaultAuthoredBucket(statId);
        }

        public static bool IsAuthoredBucket(StatModifierBucket bucket)
        {
            return bucket is StatModifierBucket.Flat or StatModifierBucket.AdditivePercent;
        }

        public static StatModifierBucket DefaultAuthoredBucket(StatId statId)
        {
            return StatDisplayRules.IsPercentStat(statId) || StatDisplayRules.IsAttackSpeedStat(statId)
                ? StatModifierBucket.AdditivePercent
                : StatModifierBucket.Flat;
        }

        public static bool IsWeaponUpgradeStat(StatId statId)
        {
            return Enum.IsDefined(typeof(StatId), statId)
                && WeaponDefinition.IsWeaponLocalUpgradeStat(statId);
        }

        public static bool IsTalentStat(StatId statId)
        {
            return Enum.IsDefined(typeof(StatId), statId)
                && !WeaponDefinition.IsWeaponScopedStat(statId);
        }

        private static string DisplayStatName(StatId statId)
        {
            return Enum.IsDefined(typeof(StatId), statId)
                ? StatMetadata.Get(statId).DisplayName
                : $"Invalid stat ({(int)statId})";
        }
    }
}
