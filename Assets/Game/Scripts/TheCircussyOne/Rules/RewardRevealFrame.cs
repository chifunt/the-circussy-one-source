using TheCircussyOne.Content;
using UnityEngine;

namespace TheCircussyOne.Rules
{
    public enum RewardRevealKind
    {
        ItemObtained,
        ChestOpened
    }

    public readonly struct RewardRevealFrame
    {
        private RewardRevealFrame(RewardRevealKind kind, ItemDefinition item, int addedStacks, int currentStackCount)
        {
            Kind = kind;
            Item = item;
            AddedStacks = Mathf.Max(0, addedStacks);
            CurrentStackCount = Mathf.Max(0, currentStackCount);
        }

        public RewardRevealKind Kind { get; }
        public ItemDefinition Item { get; }
        public int AddedStacks { get; }
        public int CurrentStackCount { get; }
        public bool HasReward => Item != null;
        public string Title => Kind == RewardRevealKind.ChestOpened ? "CHEST OPENED" : "ITEM OBTAINED";
        public string DisplayName => Item != null ? Item.DisplayName : string.Empty;
        public string ShortDescription => Item != null ? Item.shortDescription : string.Empty;
        public string EffectText => Item != null ? ItemEffectDisplayRules.BuildEffectText(Item, CurrentStackCount) : string.Empty;
        public ContentRarity Rarity => Item != null ? Item.rarity : ContentRarity.Common;
        public Sprite IconSprite => ContentIconRules.ForItem(Item);
        public Color IconColor => ContentIconRules.FallbackColor(Item);
        public string RarityLabel => ContentRarityMetadata.Get(Rarity).DisplayName.ToUpperInvariant();

        public string StackText
        {
            get
            {
                if (CurrentStackCount <= 1)
                {
                    return AddedStacks > 1 ? $"+{AddedStacks} STACKS" : "NEW ITEM";
                }

                return AddedStacks > 1
                    ? $"+{AddedStacks} STACKS  |  STACK {CurrentStackCount}"
                    : $"STACK {CurrentStackCount}";
            }
        }

        public static RewardRevealFrame ForItem(ItemDefinition item, int addedStacks, int currentStackCount)
        {
            return new RewardRevealFrame(RewardRevealKind.ItemObtained, item, addedStacks, currentStackCount);
        }

        public static RewardRevealFrame ForChest(ItemDefinition item, int addedStacks, int currentStackCount)
        {
            return new RewardRevealFrame(RewardRevealKind.ChestOpened, item, addedStacks, currentStackCount);
        }
    }
}
