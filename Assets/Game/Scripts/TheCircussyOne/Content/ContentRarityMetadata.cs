using UnityEngine;

namespace TheCircussyOne.Content
{
    public readonly struct ContentRarityInfo
    {
        public ContentRarityInfo(string displayName, Color color, int defaultWeight)
        {
            DisplayName = displayName;
            Color = color;
            DefaultWeight = defaultWeight;
        }

        public string DisplayName { get; }
        public Color Color { get; }
        public int DefaultWeight { get; }
    }

    public static class ContentRarityMetadata
    {
        public static Color CommonColor => new(0.92f, 0.92f, 0.88f, 1f);
        public static Color UncommonColor => new(0.2f, 0.9f, 0.35f, 1f);
        public static Color RareColor => new(0.25f, 0.55f, 1f, 1f);
        public static Color EpicColor => new(0.72f, 0.32f, 1f, 1f);
        public static Color LegendaryColor => new(1f, 0.58f, 0.15f, 1f);
        public static Color CommonAuthoringColor => new(0.98f, 0.98f, 0.94f, 1f);
        public static Color UncommonAuthoringColor => new(0.48f, 1f, 0.55f, 1f);
        public static Color RareAuthoringColor => new(0.58f, 0.82f, 1f, 1f);
        public static Color EpicAuthoringColor => new(0.9f, 0.68f, 1f, 1f);
        public static Color LegendaryAuthoringColor => new(1f, 0.78f, 0.38f, 1f);

        public static readonly ContentRarity[] All =
        {
            ContentRarity.Common,
            ContentRarity.Uncommon,
            ContentRarity.Rare,
            ContentRarity.Epic,
            ContentRarity.Legendary
        };

        public static ContentRarityInfo Get(ContentRarity rarity)
        {
            return rarity switch
            {
                ContentRarity.Uncommon => new ContentRarityInfo("Uncommon", UncommonColor, 25),
                ContentRarity.Rare => new ContentRarityInfo("Rare", RareColor, 10),
                ContentRarity.Epic => new ContentRarityInfo("Epic", EpicColor, 4),
                ContentRarity.Legendary => new ContentRarityInfo("Legendary", LegendaryColor, 1),
                _ => new ContentRarityInfo("Common", CommonColor, 60)
            };
        }

        public static Color AuthoringColor(ContentRarity rarity)
        {
            return rarity switch
            {
                ContentRarity.Uncommon => UncommonAuthoringColor,
                ContentRarity.Rare => RareAuthoringColor,
                ContentRarity.Epic => EpicAuthoringColor,
                ContentRarity.Legendary => LegendaryAuthoringColor,
                _ => CommonAuthoringColor
            };
        }
    }
}
