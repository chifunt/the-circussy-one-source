using TheCircussyOne.Config;
using UnityEngine.UIElements;

namespace TheCircussyOne.Visuals
{
    internal readonly struct HudEquipmentLayoutElements
    {
        public HudEquipmentLayoutElements(
            VisualElement weaponSlotRow,
            VisualElement[] weaponSlots,
            Label[] weaponSlotLevelLabels,
            VisualElement itemStackColumn)
        {
            WeaponSlotRow = weaponSlotRow;
            WeaponSlots = weaponSlots;
            WeaponSlotLevelLabels = weaponSlotLevelLabels;
            ItemStackColumn = itemStackColumn;
        }

        public VisualElement WeaponSlotRow { get; }
        public VisualElement[] WeaponSlots { get; }
        public Label[] WeaponSlotLevelLabels { get; }
        public VisualElement ItemStackColumn { get; }
    }

    internal static class HudEquipmentLayoutDriver
    {
        public static void Apply(HudEquipmentLayoutElements elements, HudVisualConfig config, float scale)
        {
            if (config == null)
            {
                return;
            }

            ApplyWeaponRow(elements.WeaponSlotRow, config, scale);
            ApplyWeaponSlots(elements.WeaponSlots, config, scale);
            ApplyWeaponLevelLabels(elements.WeaponSlotLevelLabels, config, scale);
            ApplyItemStackColumn(elements.ItemStackColumn, config, scale);
        }

        private static void ApplyWeaponRow(VisualElement row, HudVisualConfig config, float scale)
        {
            if (row == null)
            {
                return;
            }

            row.style.left = Px(config.healthOffset.x + config.weaponSlotsOffsetFromLevelBadge.x, scale);
            row.style.top = Px(LevelBadgeTop(config) + config.levelBadgeHeight + config.weaponSlotsOffsetFromLevelBadge.y, scale);
            row.style.height = Px(config.weaponSlotRowHeight, scale);
            row.style.overflow = Overflow.Visible;
        }

        private static void ApplyWeaponSlots(VisualElement[] slots, HudVisualConfig config, float scale)
        {
            if (slots == null)
            {
                return;
            }

            for (int i = 0; i < slots.Length; i++)
            {
                VisualElement slot = slots[i];
                if (slot == null)
                {
                    continue;
                }

                slot.style.width = Px(config.weaponSlotSize, scale);
                slot.style.height = Px(config.weaponSlotSize, scale);
                slot.style.marginRight = Px(config.weaponSlotGap, scale);
                slot.style.overflow = Overflow.Visible;
                SetRadius(slot, config.weaponSlotCornerRadius, scale);
            }
        }

        private static void ApplyWeaponLevelLabels(Label[] labels, HudVisualConfig config, float scale)
        {
            if (labels == null)
            {
                return;
            }

            for (int i = 0; i < labels.Length; i++)
            {
                Label label = labels[i];
                if (label == null)
                {
                    continue;
                }

                float labelWidth = Px(config.weaponSlotLevelMinWidth, scale);
                label.style.left = Length.Percent(50f);
                label.style.right = StyleKeyword.Null;
                label.style.top = Px(config.weaponSlotSize + config.weaponSlotLevelOffset.y, scale);
                label.style.bottom = StyleKeyword.Null;
                label.style.marginLeft = -labelWidth * 0.5f + Px(config.weaponSlotLevelOffset.x, scale);
                label.style.width = labelWidth;
                label.style.minWidth = Px(config.weaponSlotLevelMinWidth, scale);
                label.style.height = Px(config.weaponSlotLevelHeight, scale);
                label.style.fontSize = Px(config.weaponSlotLevelFontSize, scale);
            }
        }

        private static void ApplyItemStackColumn(VisualElement column, HudVisualConfig config, float scale)
        {
            if (column == null)
            {
                return;
            }

            column.style.right = Px(config.killsOffset.x + config.itemStackOffsetFromKills.x, scale);
            column.style.top = Px(config.killsOffset.y + config.killsHeight + config.itemStackOffsetFromKills.y, scale);
            column.style.width = Px(config.itemStackColumnWidth, scale);
        }

        private static float Px(float value, float scale)
        {
            return value * scale;
        }

        private static float CenteredRowTop(HudVisualConfig config, float elementHeight)
        {
            float healthCenter = config.healthOffset.y + config.healthHeight * 0.5f;
            return healthCenter - elementHeight * 0.5f;
        }

        private static float LevelBadgeTop(HudVisualConfig config)
        {
            return CenteredRowTop(config, config.levelBadgeHeight) + config.levelBadgeOffset.y;
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
