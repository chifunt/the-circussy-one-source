using System;
using System.Collections.Generic;
using TheCircussyOne.Content;
using TheCircussyOne.Runtime;
using TheCircussyOne.Stats;
using UnityEngine;

namespace TheCircussyOne.Rules
{
    public readonly struct ChestRewardRoll
    {
        public ChestRewardRoll(ItemDefinition item, ContentRarity rolledRarity)
        {
            Item = item;
            RolledRarity = rolledRarity;
        }

        public ItemDefinition Item { get; }
        public ContentRarity RolledRarity { get; }
        public bool HasReward => Item != null;
    }

    public static class ChestRewardRules
    {
        private static readonly ContentRarity[] RarityFallbackOrder =
        {
            ContentRarity.Common,
            ContentRarity.Uncommon,
            ContentRarity.Rare,
            ContentRarity.Epic,
            ContentRarity.Legendary
        };

        public static int StableSeed(string chestId, Vector3 position)
        {
            return StableSeed(chestId, position, runSeed: 0);
        }

        public static int StableSeed(string chestId, Vector3 position, int runSeed)
        {
            return DeterministicSeed.ContentPosition(runSeed, chestId, position);
        }

        public static ChestRewardRoll Roll(
            ChestDefinition chest,
            ItemCatalog itemCatalog,
            ItemInventory inventory,
            RunStats stats,
            int seed)
        {
            if (chest == null || itemCatalog?.Items == null)
            {
                return default;
            }

            var rng = new System.Random(seed);
            float luck = stats != null ? stats.GetFloat(StatId.Luck) : 100f;
            ContentRarity targetRarity = RollRarity(chest.itemRarityWeights, (float)rng.NextDouble(), luck);
            List<ItemDefinition> candidates = EligibleItems(itemCatalog.Items, inventory);
            if (candidates.Count <= 0)
            {
                return default;
            }

            ContentRarity selectedRarity = ResolveAvailableRarity(candidates, targetRarity);
            List<ItemDefinition> rarityCandidates = CandidatesForRarity(candidates, selectedRarity);
            if (rarityCandidates.Count <= 0)
            {
                return default;
            }

            ItemDefinition selected = rarityCandidates[rng.Next(0, rarityCandidates.Count)];
            return new ChestRewardRoll(selected, selectedRarity);
        }

        public static ContentRarity RollRarity(ContentRarityWeightTable table, float normalizedRoll, float luck)
        {
            float effectiveLuck = StatRules.EffectiveLuckMultiplier(luck);
            float adjustedRoll = Mathf.Clamp01(normalizedRoll + Mathf.Max(0f, effectiveLuck - 1f) * 0.15f);
            return table.Pick(adjustedRoll);
        }

        private static List<ItemDefinition> EligibleItems(IReadOnlyList<ItemDefinition> items, ItemInventory inventory)
        {
            var candidates = new List<ItemDefinition>();
            if (items == null)
            {
                return candidates;
            }

            for (int i = 0; i < items.Count; i++)
            {
                ItemDefinition item = items[i];
                if (!ContentAvailabilityRules.IsActiveAndValid(item))
                {
                    continue;
                }

                if (inventory != null && !inventory.CanAddItem(item))
                {
                    continue;
                }

                candidates.Add(item);
            }

            return candidates;
        }

        private static ContentRarity ResolveAvailableRarity(IReadOnlyList<ItemDefinition> candidates, ContentRarity target)
        {
            if (HasRarity(candidates, target))
            {
                return target;
            }

            int targetIndex = Array.IndexOf(RarityFallbackOrder, target);
            if (targetIndex < 0)
            {
                targetIndex = 0;
            }

            for (int i = targetIndex - 1; i >= 0; i--)
            {
                if (HasRarity(candidates, RarityFallbackOrder[i]))
                {
                    return RarityFallbackOrder[i];
                }
            }

            for (int i = targetIndex + 1; i < RarityFallbackOrder.Length; i++)
            {
                if (HasRarity(candidates, RarityFallbackOrder[i]))
                {
                    return RarityFallbackOrder[i];
                }
            }

            return target;
        }

        private static bool HasRarity(IReadOnlyList<ItemDefinition> candidates, ContentRarity rarity)
        {
            for (int i = 0; i < candidates.Count; i++)
            {
                if (candidates[i] != null && candidates[i].rarity == rarity)
                {
                    return true;
                }
            }

            return false;
        }

        private static List<ItemDefinition> CandidatesForRarity(IReadOnlyList<ItemDefinition> candidates, ContentRarity rarity)
        {
            var results = new List<ItemDefinition>();
            for (int i = 0; i < candidates.Count; i++)
            {
                ItemDefinition item = candidates[i];
                if (item != null && item.rarity == rarity)
                {
                    results.Add(item);
                }
            }

            return results;
        }
    }
}
