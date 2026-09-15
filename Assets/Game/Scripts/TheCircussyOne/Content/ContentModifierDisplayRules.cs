using System;
using System.Text;
using TheCircussyOne.Stats;

namespace TheCircussyOne.Content
{
    public static class ContentModifierDisplayRules
    {
        public static string Summary(UpgradeStatModifierDefinition modifier)
        {
            return Summary(modifier.statId, modifier.bucket, modifier.value);
        }

        public static string Summary(ItemStatModifierDefinition modifier)
        {
            return Summary(modifier.statId, modifier.bucket, modifier.value);
        }

        public static string RarityPreview(UpgradeStatModifierDefinition modifier)
        {
            if (!TryGetStat(modifier.statId, out StatDefinition stat))
            {
                return InvalidStatSummary(modifier.statId);
            }

            var builder = new StringBuilder();
            AppendRarity(builder, stat, modifier, ContentRarity.Common);
            AppendRarity(builder, stat, modifier, ContentRarity.Uncommon);
            AppendRarity(builder, stat, modifier, ContentRarity.Rare);
            AppendRarity(builder, stat, modifier, ContentRarity.Epic);
            AppendRarity(builder, stat, modifier, ContentRarity.Legendary);
            return builder.ToString();
        }

        public static bool HasValidStatAndBucket(StatId statId, StatModifierBucket bucket)
        {
            return Enum.IsDefined(typeof(StatId), statId)
                && Enum.IsDefined(typeof(StatModifierBucket), bucket);
        }

        public static string StatDisplayName(StatId statId)
        {
            return TryGetStat(statId, out StatDefinition stat)
                ? stat.DisplayName
                : InvalidStatSummary(statId);
        }

        public static string Summary(StatId statId, StatModifierBucket bucket, float value)
        {
            if (!HasValidStatAndBucket(statId, bucket))
            {
                return InvalidStatSummary(statId);
            }

            StatDefinition stat = StatMetadata.Get(statId);
            return $"{stat.DisplayName} {StatDisplayRules.FormatModifier(stat, bucket, value)}";
        }

        private static void AppendRarity(StringBuilder builder, StatDefinition stat, UpgradeStatModifierDefinition modifier, ContentRarity rarity)
        {
            if (builder.Length > 0)
            {
                builder.Append(" | ");
            }

            builder.Append(rarity);
            builder.Append(' ');
            builder.Append(StatDisplayRules.FormatModifier(stat, modifier.bucket, modifier.ValueFor(rarity)));
        }

        private static bool TryGetStat(StatId statId, out StatDefinition stat)
        {
            if (Enum.IsDefined(typeof(StatId), statId))
            {
                stat = StatMetadata.Get(statId);
                return true;
            }

            stat = default;
            return false;
        }

        private static string InvalidStatSummary(StatId statId)
        {
            return $"Invalid stat ({(int)statId})";
        }
    }
}
