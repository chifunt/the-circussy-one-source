using System;
using UnityEngine;

namespace TheCircussyOne.Content
{
    [Serializable]
    public struct ContentRarityWeightTable
    {
        public int common;
        public int uncommon;
        public int rare;
        public int epic;
        public int legendary;

        public int TotalWeight => Mathf.Max(0, common) + Mathf.Max(0, uncommon) + Mathf.Max(0, rare) + Mathf.Max(0, epic) + Mathf.Max(0, legendary);
        public bool IsValid => common >= 0 && uncommon >= 0 && rare >= 0 && epic >= 0 && legendary >= 0 && TotalWeight > 0;

        public static ContentRarityWeightTable LevelUpDefault()
        {
            return new ContentRarityWeightTable { common = 60, uncommon = 25, rare = 10, epic = 4, legendary = 1 };
        }

        public static ContentRarityWeightTable ChestDefault()
        {
            return NormalChestDefault();
        }

        public static ContentRarityWeightTable BossChestDefault()
        {
            return BossRewardDefault();
        }

        public static ContentRarityWeightTable NormalChestDefault()
        {
            return new ContentRarityWeightTable { common = 45, uncommon = 30, rare = 17, epic = 6, legendary = 2 };
        }

        public static ContentRarityWeightTable SpecialEnemyChestDefault()
        {
            return new ContentRarityWeightTable { common = 30, uncommon = 35, rare = 23, epic = 9, legendary = 3 };
        }

        public static ContentRarityWeightTable BossRewardDefault()
        {
            return new ContentRarityWeightTable { common = 0, uncommon = 20, rare = 40, epic = 30, legendary = 10 };
        }

        public static ContentRarityWeightTable ShrineDefault()
        {
            return new ContentRarityWeightTable { common = 10, uncommon = 25, rare = 35, epic = 22, legendary = 8 };
        }

        public ContentRarity Pick(float normalizedRoll)
        {
            if (!IsValid)
            {
                return ContentRarity.Common;
            }

            float roll = Mathf.Clamp01(normalizedRoll) * TotalWeight;
            if (roll < common)
            {
                return ContentRarity.Common;
            }

            roll -= common;
            if (roll < uncommon)
            {
                return ContentRarity.Uncommon;
            }

            roll -= uncommon;
            if (roll < rare)
            {
                return ContentRarity.Rare;
            }

            roll -= rare;
            if (roll < epic)
            {
                return ContentRarity.Epic;
            }

            return ContentRarity.Legendary;
        }
    }
}
