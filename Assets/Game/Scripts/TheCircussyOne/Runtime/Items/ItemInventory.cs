using System;
using System.Collections.Generic;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using TheCircussyOne.Stats;

namespace TheCircussyOne.Runtime
{
    public enum ItemGrantSource
    {
        WorldItem = 0,
        Chest = 1
    }

    public readonly struct ItemAddedEvent
    {
        public ItemAddedEvent(ItemDefinition definition, int addedStacks, int currentStackCount, ItemGrantSource source = ItemGrantSource.WorldItem)
        {
            Definition = definition;
            AddedStacks = Math.Max(0, addedStacks);
            CurrentStackCount = Math.Max(0, currentStackCount);
            Source = source;
        }

        public ItemDefinition Definition { get; }
        public int AddedStacks { get; }
        public int CurrentStackCount { get; }
        public ItemGrantSource Source { get; }
        public bool HasItem => Definition != null && AddedStacks > 0;
    }

    public sealed class ItemInventory
    {
        private readonly List<ItemStackRuntime> items = new();
        private readonly RunStats stats;

        public ItemInventory(RunStats stats = null)
        {
            this.stats = stats;
        }

        public IReadOnlyList<ItemStackRuntime> Items => items;
        public int ItemCount => items.Count;
        public event Action Changed;
        public event Action<ItemAddedEvent> ItemAdded;

        public bool OwnsItem(string itemId)
        {
            return FindById(itemId) != null;
        }

        public ItemStackRuntime FindById(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return null;
            }

            for (int i = 0; i < items.Count; i++)
            {
                ItemStackRuntime item = items[i];
                if (item?.Definition != null && string.Equals(item.Definition.Id, itemId, StringComparison.Ordinal))
                {
                    return item;
                }
            }

            return null;
        }

        public bool CanAddItem(ItemDefinition definition)
        {
            if (definition == null || !ContentId.IsValidValue(definition.Id))
            {
                return false;
            }

            ItemStackRuntime existing = FindById(definition.Id);
            return ItemStackRules.CanAddItem(definition, existing?.StackCount ?? 0);
        }

        public int AddItem(ItemDefinition definition, int amount = 1)
        {
            return AddItem(definition, amount, ItemGrantSource.WorldItem);
        }

        public int AddItem(ItemDefinition definition, int amount, ItemGrantSource source)
        {
            if (amount <= 0 || !CanAddItem(definition))
            {
                return 0;
            }

            ItemStackRuntime existing = FindById(definition.Id);
            if (existing != null)
            {
                int previousStacks = existing.StackCount;
                int added = existing.AddStacks(amount);
                ApplyStackModifiers(definition, previousStacks + 1, added);
                if (added > 0)
                {
                    Changed?.Invoke();
                    ItemAdded?.Invoke(new ItemAddedEvent(definition, added, existing.StackCount, source));
                }

                return added;
            }

            var stack = new ItemStackRuntime(definition, amount);
            items.Add(stack);
            ApplyStackModifiers(definition, 1, stack.StackCount);
            Changed?.Invoke();
            ItemAdded?.Invoke(new ItemAddedEvent(definition, stack.StackCount, stack.StackCount, source));
            return stack.StackCount;
        }

        public int RemoveItem(string itemId, int amount = 1)
        {
            if (amount <= 0)
            {
                return 0;
            }

            ItemStackRuntime existing = FindById(itemId);
            if (existing == null)
            {
                return 0;
            }

            int previousStacks = existing.StackCount;
            int removed = existing.RemoveStacks(amount);
            RemoveStackModifiers(existing.Definition, previousStacks, removed);
            if (existing.StackCount <= 0)
            {
                items.Remove(existing);
            }

            if (removed > 0)
            {
                Changed?.Invoke();
            }

            return removed;
        }

        public void Clear()
        {
            if (items.Count == 0)
            {
                return;
            }

            if (stats != null)
            {
                for (int itemIndex = 0; itemIndex < items.Count; itemIndex++)
                {
                    ItemStackRuntime item = items[itemIndex];
                    if (item?.Definition == null)
                    {
                        continue;
                    }

                    for (int stackIndex = 1; stackIndex <= item.StackCount; stackIndex++)
                    {
                        stats.RemoveSource(ModifierSourceId(item.Definition.Id, stackIndex));
                    }
                }
            }

            items.Clear();
            Changed?.Invoke();
        }

        public static string ModifierSourceId(string itemId, int stackIndex)
        {
            return $"item:{itemId}:{Math.Max(1, stackIndex)}";
        }

        private void ApplyStackModifiers(ItemDefinition definition, int firstStackIndex, int count)
        {
            if (stats == null || definition == null || count <= 0)
            {
                return;
            }

            for (int stackIndex = firstStackIndex; stackIndex < firstStackIndex + count; stackIndex++)
            {
                float multiplier = ItemStackRules.ModifierMultiplier(definition, stackIndex);
                if (multiplier <= 0f)
                {
                    continue;
                }

                string sourceId = ModifierSourceId(definition.Id, stackIndex);
                ApplyModifiers(definition.statModifiers, sourceId, multiplier);
                ApplyModifiers(definition.downsideStatModifiers, sourceId, multiplier);
            }
        }

        private void RemoveStackModifiers(ItemDefinition definition, int previousStackCount, int count)
        {
            if (stats == null || definition == null || count <= 0)
            {
                return;
            }

            for (int stackIndex = previousStackCount; stackIndex > previousStackCount - count; stackIndex--)
            {
                stats.RemoveSource(ModifierSourceId(definition.Id, stackIndex));
            }
        }

        private void ApplyModifiers(IReadOnlyList<ItemStatModifierDefinition> modifiers, string sourceId, float multiplier)
        {
            if (modifiers == null)
            {
                return;
            }

            for (int modifierIndex = 0; modifierIndex < modifiers.Count; modifierIndex++)
            {
                stats.AddModifier(modifiers[modifierIndex].ToRuntime(sourceId, multiplier));
            }
        }
    }
}
