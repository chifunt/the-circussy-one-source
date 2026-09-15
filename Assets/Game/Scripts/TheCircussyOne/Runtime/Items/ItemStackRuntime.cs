using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using UnityEngine;

namespace TheCircussyOne.Runtime
{
    public sealed class ItemStackRuntime
    {
        public ItemStackRuntime(ItemDefinition definition, int stackCount)
        {
            Definition = definition;
            StackCount = Mathf.Clamp(stackCount, 1, MaxStacks);
        }

        public ItemDefinition Definition { get; }
        public int StackCount { get; private set; }
        public int MaxStacks => ItemStackRules.MaxStacks(Definition);
        public bool IsAtMax => StackCount >= MaxStacks;

        public int AddStacks(int amount)
        {
            if (amount <= 0 || Definition == null || IsAtMax)
            {
                return 0;
            }

            int previous = StackCount;
            StackCount = Mathf.Clamp(StackCount + amount, 1, MaxStacks);
            return StackCount - previous;
        }

        public int RemoveStacks(int amount)
        {
            if (amount <= 0 || Definition == null || StackCount <= 0)
            {
                return 0;
            }

            int previous = StackCount;
            StackCount = Mathf.Max(0, StackCount - amount);
            return previous - StackCount;
        }
    }
}
