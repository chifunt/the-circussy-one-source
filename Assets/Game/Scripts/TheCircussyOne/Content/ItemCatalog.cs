using System.Collections.Generic;
using Sirenix.OdinInspector;
using TheCircussyOne.Rules;
using UnityEngine;

namespace TheCircussyOne.Content
{
    [CreateAssetMenu(menuName = "The Circussy One/Content/Item Catalog", fileName = "ItemCatalog")]
    public sealed class ItemCatalog : SerializedScriptableObject, IContentCatalog<ItemDefinition>
    {
        private const string Tabs = "Item Catalog";

        [TabGroup(Tabs, "Definitions"), BoxGroup(Tabs + "/Definitions/Items"), LabelWidth(160), AssetSelector]
        public List<ItemDefinition> items = new();

        [TabGroup(Tabs, "Diagnostics"), ShowInInspector, ReadOnly, LabelText("Valid Items"), PropertyOrder(100)]
        private int ValidItemCount => CountValidItems();

        public IReadOnlyList<ItemDefinition> Items => items;
        public IReadOnlyList<ItemDefinition> Definitions => items;

        public bool EnsureWorkflowDefaults(params ItemDefinition[] starterItems)
        {
            bool changed = false;
            items ??= new List<ItemDefinition>();
            if (starterItems == null)
            {
                return changed;
            }

            for (int i = 0; i < starterItems.Length; i++)
            {
                ItemDefinition item = starterItems[i];
                if (item == null || items.Contains(item))
                {
                    continue;
                }

                items.Add(item);
                changed = true;
            }

            return changed;
        }

        public List<ContentValidationIssue> ValidateContent()
        {
            var issues = ContentCatalogRules.ValidateDefinitions(Definitions);
            int validItemCount = 0;
            if (Definitions != null)
            {
                for (int i = 0; i < Definitions.Count; i++)
                {
                    ItemDefinition item = Definitions[i];
                    if (item == null)
                    {
                        continue;
                    }

                    if (!item.isActive)
                    {
                        continue;
                    }

                    ValidateStackPolicy(item, issues);
                    validItemCount++;

                    if (item.statModifiers == null || item.statModifiers.Count == 0)
                    {
                        AddIssue(issues, "item.missing-stat-modifiers", ContentValidationSeverity.Warning, item, "has no stat modifiers yet.");
                    }
                    else
                    {
                        ContentStatValidationRules.ValidateItemModifiers(item.statModifiers, issues, "item.invalid-stat-modifier", item.Id, "Item");
                        ValidateItemStats(item.statModifiers, item, issues, "item.weapon-stat", "upside");
                    }

                    if (item.downsideStatModifiers != null && item.downsideStatModifiers.Count > 0)
                    {
                        ContentStatValidationRules.ValidateItemModifiers(item.downsideStatModifiers, issues, "item.invalid-downside-stat-modifier", item.Id, "Item downside");
                        ValidateItemStats(item.downsideStatModifiers, item, issues, "item.downside-weapon-stat", "downside");
                    }
                }
            }

            if (validItemCount == 0)
            {
                issues.Add(new ContentValidationIssue(
                    "item.no-valid-items",
                    ContentValidationSeverity.Error,
                    "Item catalog has no valid items."));
            }

            return issues;
        }

        private static void ValidateStackPolicy(ItemDefinition item, List<ContentValidationIssue> issues)
        {
            if (!System.Enum.IsDefined(typeof(ItemStackPolicy), item.stackPolicy))
            {
                AddIssue(issues, "item.invalid-stack-policy", ContentValidationSeverity.Error, item, "uses an invalid stack policy.");
                return;
            }

            if (item.maxStacks < 1)
            {
                AddIssue(issues, "item.invalid-max-stacks", ContentValidationSeverity.Error, item, "must have at least one max stack.");
            }

            if (!ItemStackRules.UsesMaxStacks(item.stackPolicy) && item.maxStacks > 1)
            {
                AddIssue(issues, "item.unused-max-stacks", ContentValidationSeverity.Warning, item, $"{item.stackPolicy} always uses one runtime stack.");
            }

            if (item.stackPolicy == ItemStackPolicy.StackWithCap)
            {
                if (item.effectCapStacks < 1)
                {
                    AddIssue(issues, "item.invalid-effect-cap-stacks", ContentValidationSeverity.Error, item, "must have at least one effect cap stack.");
                }

                if (item.effectCapStacks > item.maxStacks)
                {
                    AddIssue(issues, "item.effect-cap-exceeds-max", ContentValidationSeverity.Error, item, "has effect cap stacks greater than max stacks.");
                }
            }

            if (item.stackPolicy == ItemStackPolicy.StackDiminishing && item.diminishingFalloff < 0f)
            {
                AddIssue(issues, "item.invalid-diminishing-falloff", ContentValidationSeverity.Error, item, "must have non-negative diminishing falloff.");
            }
        }

        private static void ValidateItemStats(
            IReadOnlyList<ItemStatModifierDefinition> modifiers,
            ItemDefinition item,
            List<ContentValidationIssue> issues,
            string code,
            string label)
        {
            for (int modifierIndex = 0; modifierIndex < modifiers.Count; modifierIndex++)
            {
                ItemStatModifierDefinition modifier = modifiers[modifierIndex];
                if (!ContentStatValidationRules.IsValid(modifier.statId, modifier.bucket))
                {
                    continue;
                }

                if (!ContentStatValidationRules.IsTalentStat(modifier.statId))
                {
                    AddIssue(
                        issues,
                        code,
                        ContentValidationSeverity.Error,
                        item,
                        $"uses weapon-local {label} stat '{ContentModifierDisplayRules.StatDisplayName(modifier.statId)}'; items can only use player/global stats.");
                }
            }
        }

        private int CountValidItems()
        {
            if (items == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] != null && items[i].isActive && items[i].maxStacks >= 1)
                {
                    count++;
                }
            }

            return count;
        }

        private static void AddIssue(
            List<ContentValidationIssue> issues,
            string code,
            ContentValidationSeverity severity,
            ItemDefinition item,
            string message)
        {
            issues.Add(new ContentValidationIssue(
                code,
                severity,
                $"Item '{item.Id}' {message}",
                item.Id));
        }
    }
}
