using TheCircussyOne.Content;
using UnityEngine;

namespace TheCircussyOne.Rules
{
    public static class ItemStackRules
    {
        public const float DefaultDiminishingFalloff = 0.5f;

        public static bool UsesMaxStacks(ItemStackPolicy policy)
        {
            return policy is ItemStackPolicy.StackLinear
                or ItemStackPolicy.StackWithCap
                or ItemStackPolicy.StackDiminishing;
        }

        public static bool UsesEffectCap(ItemStackPolicy policy)
        {
            return policy == ItemStackPolicy.StackWithCap;
        }

        public static bool UsesDiminishingFalloff(ItemStackPolicy policy)
        {
            return policy == ItemStackPolicy.StackDiminishing;
        }

        public static int MaxStacks(ItemDefinition item)
        {
            return item == null ? 0 : MaxStacks(item.stackPolicy, item.maxStacks);
        }

        public static int MaxStacks(ItemStackPolicy policy, int authoredMaxStacks)
        {
            return UsesMaxStacks(policy) ? Mathf.Max(1, authoredMaxStacks) : 1;
        }

        public static int EffectiveModifierStackCount(ItemDefinition item)
        {
            if (item == null)
            {
                return 0;
            }

            int maxStacks = MaxStacks(item);
            return item.stackPolicy == ItemStackPolicy.StackWithCap
                ? Mathf.Clamp(item.effectCapStacks, 1, maxStacks)
                : maxStacks;
        }

        public static bool CanAddItem(ItemDefinition item, int currentStacks)
        {
            if (item == null || currentStacks < 0)
            {
                return false;
            }

            return currentStacks < MaxStacks(item);
        }

        public static float ModifierMultiplier(ItemDefinition item, int stackIndex)
        {
            if (item == null || stackIndex < 1 || stackIndex > EffectiveModifierStackCount(item))
            {
                return 0f;
            }

            if (item.stackPolicy != ItemStackPolicy.StackDiminishing)
            {
                return 1f;
            }

            float falloff = Mathf.Max(0f, item.diminishingFalloff);
            return 1f / (1f + falloff * (stackIndex - 1));
        }

        public static string StackSummary(ItemDefinition item)
        {
            if (item == null)
            {
                return "Missing";
            }

            return item.stackPolicy switch
            {
                ItemStackPolicy.Unique => "Unique",
                ItemStackPolicy.InvalidDuplicate => "Invalid Duplicate",
                ItemStackPolicy.StackWithCap => $"Stack With Cap x{MaxStacks(item)} (effects x{EffectiveModifierStackCount(item)})",
                ItemStackPolicy.StackDiminishing => $"Stack Diminishing x{MaxStacks(item)}",
                _ => $"Stack Linear x{MaxStacks(item)}"
            };
        }
    }
}
