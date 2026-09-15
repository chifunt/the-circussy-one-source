namespace TheCircussyOne.Content
{
    public static class ContentRarityScaling
    {
        public static float Factor(ContentRarity rarity)
        {
            return rarity switch
            {
                ContentRarity.Uncommon => 1.2f,
                ContentRarity.Rare => 1.4f,
                ContentRarity.Epic => 1.6f,
                ContentRarity.Legendary => 2f,
                _ => 1f
            };
        }

        public static float Scale(float baseValue, ContentRarity rarity)
        {
            return baseValue * Factor(rarity);
        }
    }
}
