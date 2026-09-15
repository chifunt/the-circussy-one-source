using System.Collections.Generic;
using System.Text;
using TheCircussyOne.Content;
using TheCircussyOne.Runtime;
using TheCircussyOne.Stats;
using UnityEngine;

namespace TheCircussyOne.Rules
{
    public readonly struct ItemInventoryCardFrame
    {
        public ItemInventoryCardFrame(
            ItemDefinition definition,
            string displayName,
            string rarityLabel,
            string shortDescription,
            string stackText,
            string effectText,
            Sprite iconSprite,
            Color iconColor,
            Color rarityColor,
            int stackCount,
            int maxStacks)
        {
            Definition = definition;
            DisplayName = displayName ?? string.Empty;
            RarityLabel = rarityLabel ?? string.Empty;
            ShortDescription = shortDescription ?? string.Empty;
            StackText = stackText ?? string.Empty;
            EffectText = effectText ?? string.Empty;
            IconSprite = iconSprite;
            IconColor = iconColor;
            RarityColor = rarityColor;
            StackCount = Mathf.Max(0, stackCount);
            MaxStacks = Mathf.Max(0, maxStacks);
        }

        public ItemDefinition Definition { get; }
        public string DisplayName { get; }
        public string RarityLabel { get; }
        public string ShortDescription { get; }
        public string StackText { get; }
        public string EffectText { get; }
        public Sprite IconSprite { get; }
        public Color IconColor { get; }
        public Color RarityColor { get; }
        public int StackCount { get; }
        public int MaxStacks { get; }
        public bool HasItem => Definition != null && StackCount > 0;
    }

    public static class ItemInventoryPanelRules
    {
        public static IReadOnlyList<ItemInventoryCardFrame> BuildFrames(IReadOnlyList<ItemStackRuntime> items)
        {
            var frames = new List<ItemInventoryCardFrame>();
            if (items == null)
            {
                return frames;
            }

            for (int i = 0; i < items.Count; i++)
            {
                ItemStackRuntime stack = items[i];
                if (stack?.Definition == null || stack.StackCount <= 0)
                {
                    continue;
                }

                frames.Add(BuildFrame(stack));
            }

            return frames;
        }

        public static ItemInventoryCardFrame BuildFrame(ItemStackRuntime stack)
        {
            if (stack?.Definition == null)
            {
                return default;
            }

            ItemDefinition item = stack.Definition;
            ContentRarityInfo rarity = ContentRarityMetadata.Get(item.rarity);
            return new ItemInventoryCardFrame(
                item,
                item.DisplayName,
                rarity.DisplayName,
                item.shortDescription,
                StackText(item, stack.StackCount),
                ItemEffectDisplayRules.BuildEffectText(item, stack.StackCount),
                ContentIconRules.ForItem(item),
                ContentIconRules.FallbackColor(item),
                rarity.Color,
                stack.StackCount,
                stack.MaxStacks);
        }

        private static string StackText(ItemDefinition item, int stackCount)
        {
            if (item == null)
            {
                return string.Empty;
            }

            if (!ItemStackRules.UsesMaxStacks(item.stackPolicy))
            {
                return item.stackPolicy == ItemStackPolicy.Unique ? "Unique" : item.StackSummary;
            }

            return $"x{Mathf.Max(1, stackCount)} / {ItemStackRules.MaxStacks(item)}";
        }
    }

    public static class ItemEffectDisplayRules
    {
        private const string PositiveColorHex = "#73F59A";
        private const string NegativeColorHex = "#FF5A66";

        public static string BuildEffectText(ItemDefinition item, int stackCount)
        {
            var builder = new StringBuilder();
            AppendModifierSummaries(builder, item, stackCount, item.statModifiers, isDownside: false);
            AppendModifierSummaries(builder, item, stackCount, item.downsideStatModifiers, isDownside: true);
            return builder.Length > 0 ? builder.ToString() : "No stat effects.";
        }

        private static void AppendModifierSummaries(
            StringBuilder builder,
            ItemDefinition item,
            int stackCount,
            IReadOnlyList<ItemStatModifierDefinition> modifiers,
            bool isDownside)
        {
            if (item == null || modifiers == null)
            {
                return;
            }

            float stackMultiplier = EffectiveStackMultiplier(item, stackCount);
            if (stackMultiplier <= 0f)
            {
                return;
            }

            for (int i = 0; i < modifiers.Count; i++)
            {
                ItemStatModifierDefinition modifier = modifiers[i];
                if (!ContentModifierDisplayRules.HasValidStatAndBucket(modifier.statId, modifier.bucket))
                {
                    AppendLine(builder, ContentModifierDisplayRules.Summary(modifier.statId, modifier.bucket, modifier.value));
                    continue;
                }

                float value = modifier.value * stackMultiplier;
                StatDefinition stat = StatMetadata.Get(modifier.statId);
                string amount = StatDisplayRules.FormatModifier(stat, modifier.bucket, value);
                bool beneficial = !isDownside && value >= 0f;
                AppendLine(builder, $"{stat.DisplayName} {Colorize(amount, beneficial)}");
            }
        }

        private static string Colorize(string text, bool beneficial)
        {
            return $"<color={(beneficial ? PositiveColorHex : NegativeColorHex)}>{text}</color>";
        }

        private static float EffectiveStackMultiplier(ItemDefinition item, int stackCount)
        {
            float multiplier = 0f;
            int count = Mathf.Max(0, stackCount);
            for (int stackIndex = 1; stackIndex <= count; stackIndex++)
            {
                multiplier += ItemStackRules.ModifierMultiplier(item, stackIndex);
            }

            return multiplier;
        }

        private static void AppendLine(StringBuilder builder, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            if (builder.Length > 0)
            {
                builder.AppendLine();
            }

            builder.Append(text);
        }
    }
}
