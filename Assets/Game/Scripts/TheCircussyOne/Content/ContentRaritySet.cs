using System;
using Sirenix.OdinInspector;

namespace TheCircussyOne.Content
{
    [Serializable]
    public struct ContentRaritySet
    {
        [VerticalGroup("Rarities"), ToggleLeft, LabelText("Common")]
        [GUIColor("@TheCircussyOne.Content.ContentRarityMetadata.CommonAuthoringColor")]
        public bool common;

        [VerticalGroup("Rarities"), ToggleLeft, LabelText("Uncommon")]
        [GUIColor("@TheCircussyOne.Content.ContentRarityMetadata.UncommonAuthoringColor")]
        public bool uncommon;

        [VerticalGroup("Rarities"), ToggleLeft, LabelText("Rare")]
        [GUIColor("@TheCircussyOne.Content.ContentRarityMetadata.RareAuthoringColor")]
        public bool rare;

        [VerticalGroup("Rarities"), ToggleLeft, LabelText("Epic")]
        [GUIColor("@TheCircussyOne.Content.ContentRarityMetadata.EpicAuthoringColor")]
        public bool epic;

        [VerticalGroup("Rarities"), ToggleLeft, LabelText("Legendary")]
        [GUIColor("@TheCircussyOne.Content.ContentRarityMetadata.LegendaryAuthoringColor")]
        public bool legendary;

        public bool IsEmpty => !common && !uncommon && !rare && !epic && !legendary;

        public static ContentRaritySet All()
        {
            return new ContentRaritySet
            {
                common = true,
                uncommon = true,
                rare = true,
                epic = true,
                legendary = true
            };
        }

        public static ContentRaritySet Only(ContentRarity rarity)
        {
            var set = new ContentRaritySet();
            set.Set(rarity, true);
            return set;
        }

        public static ContentRaritySet Range(ContentRarity minimum, ContentRarity maximum)
        {
            int min = Rank(minimum);
            int max = Rank(maximum);
            if (min > max)
            {
                (min, max) = (max, min);
            }

            var set = new ContentRaritySet();
            for (int rank = min; rank <= max; rank++)
            {
                set.Set(FromRank(rank), true);
            }

            return set;
        }

        public bool Contains(ContentRarity rarity)
        {
            return rarity switch
            {
                ContentRarity.Uncommon => uncommon,
                ContentRarity.Rare => rare,
                ContentRarity.Epic => epic,
                ContentRarity.Legendary => legendary,
                _ => common
            };
        }

        public void Set(ContentRarity rarity, bool enabled)
        {
            switch (rarity)
            {
                case ContentRarity.Uncommon:
                    uncommon = enabled;
                    break;
                case ContentRarity.Rare:
                    rare = enabled;
                    break;
                case ContentRarity.Epic:
                    epic = enabled;
                    break;
                case ContentRarity.Legendary:
                    legendary = enabled;
                    break;
                default:
                    common = enabled;
                    break;
            }
        }

        public bool EnsureAny(ContentRarity fallback, bool useAllWhenEmpty)
        {
            if (!IsEmpty)
            {
                return false;
            }

            this = useAllWhenEmpty ? All() : Only(fallback);
            return true;
        }

        public ContentRaritySet OrAll()
        {
            return IsEmpty ? All() : this;
        }

        private static int Rank(ContentRarity rarity)
        {
            return rarity switch
            {
                ContentRarity.Uncommon => 1,
                ContentRarity.Rare => 2,
                ContentRarity.Epic => 3,
                ContentRarity.Legendary => 4,
                _ => 0
            };
        }

        private static ContentRarity FromRank(int rank)
        {
            return rank switch
            {
                1 => ContentRarity.Uncommon,
                2 => ContentRarity.Rare,
                3 => ContentRarity.Epic,
                4 => ContentRarity.Legendary,
                _ => ContentRarity.Common
            };
        }
    }
}
