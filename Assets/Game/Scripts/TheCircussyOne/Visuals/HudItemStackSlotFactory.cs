using TheCircussyOne.Config;
using TheCircussyOne.Rules;
using TheCircussyOne.Runtime;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheCircussyOne.Visuals
{
    internal static class HudItemStackSlotFactory
    {
        public static VisualElement Create(ItemStackRuntime item, HudVisualConfig config, float scale)
        {
            if (item?.Definition == null)
            {
                return null;
            }

            var slot = new VisualElement();
            slot.AddToClassList("item-stack-slot");
            ContentIconVisuals.Apply(slot, ContentIconRules.ForItem(item.Definition), ContentIconRules.FallbackColor(item.Definition));

            var count = new Label
            {
                text = item.StackCount > 1 ? item.StackCount.ToString() : string.Empty
            };
            count.AddToClassList("item-stack-count");
            count.style.display = item.StackCount > 1 ? DisplayStyle.Flex : DisplayStyle.None;
            slot.Add(count);

            ApplyScale(slot, config, scale);
            return slot;
        }

        public static void ApplyScale(VisualElement slot, HudVisualConfig config, float scale)
        {
            if (slot == null)
            {
                return;
            }

            float slotSize = config != null ? config.itemStackSlotSize : 32f;
            float gap = config != null ? config.itemStackGap : 5f;
            float cornerRadius = config != null ? config.itemStackCornerRadius : 4f;
            slot.style.width = Px(slotSize, scale);
            slot.style.height = Px(slotSize, scale);
            slot.style.marginBottom = Px(gap, scale);
            SetRadius(slot, cornerRadius, scale);

            Label count = slot.Q<Label>(className: "item-stack-count");
            if (count != null)
            {
                Vector2 offset = config != null ? config.itemStackCountOffset : new Vector2(1f, 0f);
                float minWidth = config != null ? config.itemStackCountMinWidth : 13f;
                float height = config != null ? config.itemStackCountHeight : 12f;
                float fontSize = config != null ? config.itemStackCountFontSize : 8f;
                count.style.right = Px(offset.x, scale);
                count.style.bottom = Px(offset.y, scale);
                count.style.minWidth = Px(minWidth, scale);
                count.style.height = Px(height, scale);
                count.style.fontSize = Px(fontSize, scale);
            }
        }

        private static float Px(float value, float scale)
        {
            return value * scale;
        }

        private static void SetRadius(VisualElement element, float radius, float scale)
        {
            float scaled = Px(radius, scale);
            element.style.borderTopLeftRadius = scaled;
            element.style.borderTopRightRadius = scaled;
            element.style.borderBottomLeftRadius = scaled;
            element.style.borderBottomRightRadius = scaled;
        }
    }
}
