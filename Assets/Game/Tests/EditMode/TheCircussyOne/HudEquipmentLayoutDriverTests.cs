using NUnit.Framework;
using TheCircussyOne.Config;
using TheCircussyOne.Visuals;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheCircussyOne.Tests
{
    public sealed class HudEquipmentLayoutDriverTests
    {
        [Test]
        public void ApplyPositionsWeaponSlotsAndItemStacksFromHudConfig()
        {
            var row = new VisualElement();
            var slots = new[] { new VisualElement(), new VisualElement() };
            var labels = new[] { new Label(), new Label() };
            var itemColumn = new VisualElement();
            var config = ScriptableObject.CreateInstance<HudVisualConfig>();
            config.healthOffset = new Vector2(10f, 20f);
            config.levelBadgeHeight = 14f;
            config.levelBadgeOffset = new Vector2(4f, 2f);
            config.healthHeight = 10f;
            config.weaponSlotsOffsetFromLevelBadge = new Vector2(3f, 11f);
            config.weaponSlotRowHeight = 40f;
            config.weaponSlotSize = 22f;
            config.weaponSlotGap = 6f;
            config.weaponSlotCornerRadius = 5f;
            config.weaponSlotLevelOffset = new Vector2(2f, -1f);
            config.weaponSlotLevelMinWidth = 17f;
            config.weaponSlotLevelHeight = 9f;
            config.weaponSlotLevelFontSize = 7f;
            config.killsOffset = new Vector2(7f, 9f);
            config.killsHeight = 15f;
            config.itemStackOffsetFromKills = new Vector2(2f, 4f);
            config.itemStackColumnWidth = 40f;

            HudEquipmentLayoutDriver.Apply(new HudEquipmentLayoutElements(row, slots, labels, itemColumn), config, 2f);

            Assert.That(row.style.left.value.value, Is.EqualTo(26f).Within(0.01f));
            Assert.That(row.style.top.value.value, Is.EqualTo(90f).Within(0.01f));
            Assert.That(row.style.height.value.value, Is.EqualTo(80f).Within(0.01f));
            Assert.That(row.style.overflow.value, Is.EqualTo(Overflow.Visible));
            Assert.That(slots[0].style.width.value.value, Is.EqualTo(44f).Within(0.01f));
            Assert.That(slots[0].style.height.value.value, Is.EqualTo(44f).Within(0.01f));
            Assert.That(slots[0].style.marginRight.value.value, Is.EqualTo(12f).Within(0.01f));
            Assert.That(slots[0].style.borderTopLeftRadius.value.value, Is.EqualTo(10f).Within(0.01f));
            Assert.That(labels[0].style.left.value.unit, Is.EqualTo(LengthUnit.Percent));
            Assert.That(labels[0].style.left.value.value, Is.EqualTo(50f).Within(0.01f));
            Assert.That(labels[0].style.top.value.value, Is.EqualTo(42f).Within(0.01f));
            Assert.That(labels[0].style.marginLeft.value.value, Is.EqualTo(-13f).Within(0.01f));
            Assert.That(labels[0].style.width.value.value, Is.EqualTo(34f).Within(0.01f));
            Assert.That(labels[0].style.minWidth.value.value, Is.EqualTo(34f).Within(0.01f));
            Assert.That(labels[0].style.height.value.value, Is.EqualTo(18f).Within(0.01f));
            Assert.That(labels[0].style.fontSize.value.value, Is.EqualTo(14f).Within(0.01f));
            Assert.That(itemColumn.style.right.value.value, Is.EqualTo(18f).Within(0.01f));
            Assert.That(itemColumn.style.top.value.value, Is.EqualTo(56f).Within(0.01f));
            Assert.That(itemColumn.style.width.value.value, Is.EqualTo(80f).Within(0.01f));

            Object.DestroyImmediate(config);
        }
    }
}
