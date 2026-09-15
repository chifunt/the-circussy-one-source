using System.Collections.Generic;
using UnityEngine;
using TheCircussyOne.Content;

namespace TheCircussyOne.Rules
{
    public enum XpDropStyle
    {
        Compact,
        Scatter,
        Bonus,
        LowRoll
    }

    public readonly struct XpGemDrop
    {
        public XpGemDrop(XpGemDefinition gem, int amount)
        {
            Gem = gem;
            Amount = amount;
        }

        public XpGemDefinition Gem { get; }
        public int Amount { get; }
    }

    public static class XpDropRules
    {
        public static List<XpGemDrop> BuildDrops(int baseBudget, IReadOnlyList<XpGemDefinition> gems, XpDropStyle style, int seed = 0)
        {
            var drops = new List<XpGemDrop>();
            if (baseBudget <= 0 || gems == null || gems.Count == 0)
            {
                return drops;
            }

            List<XpGemDefinition> validGems = SortedValidGems(gems, descending: style != XpDropStyle.Scatter);
            if (validGems.Count == 0)
            {
                return drops;
            }

            int budget = ApplyStyleBudget(baseBudget, validGems[validGems.Count - 1].xpAmount, style, seed);
            if (budget <= 0)
            {
                return drops;
            }

            for (int i = 0; i < validGems.Count; i++)
            {
                XpGemDefinition gem = validGems[i];
                while (budget >= gem.xpAmount)
                {
                    drops.Add(new XpGemDrop(gem, gem.xpAmount));
                    budget -= gem.xpAmount;
                }
            }

            return drops;
        }

        public static int ApplyStyleBudget(int baseBudget, int smallestGemValue, XpDropStyle style, int seed)
        {
            int safeBudget = Mathf.Max(0, baseBudget);
            int safeGemValue = Mathf.Max(1, smallestGemValue);
            return style switch
            {
                XpDropStyle.Bonus => safeBudget + safeGemValue,
                XpDropStyle.LowRoll => Mathf.Max(0, safeBudget - safeGemValue),
                _ => safeBudget
            };
        }

        public static int ValidGemCount(IReadOnlyList<XpGemDefinition> gems)
        {
            if (gems == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < gems.Count; i++)
            {
                if (IsValidGem(gems[i]))
                {
                    count++;
                }
            }

            return count;
        }

        public static XpGemDefinition SmallestGem(IReadOnlyList<XpGemDefinition> gems)
        {
            List<XpGemDefinition> validGems = SortedValidGems(gems, descending: false);
            return validGems.Count > 0 ? validGems[0] : null;
        }

        private static List<XpGemDefinition> SortedValidGems(IReadOnlyList<XpGemDefinition> gems, bool descending)
        {
            var validGems = new List<XpGemDefinition>();
            if (gems == null)
            {
                return validGems;
            }

            for (int i = 0; i < gems.Count; i++)
            {
                if (IsValidGem(gems[i]))
                {
                    validGems.Add(gems[i]);
                }
            }

            validGems.Sort((a, b) => descending
                ? b.xpAmount.CompareTo(a.xpAmount)
                : a.xpAmount.CompareTo(b.xpAmount));
            return validGems;
        }

        private static bool IsValidGem(XpGemDefinition gem)
        {
            return gem != null && gem.xpAmount > 0;
        }
    }
}
