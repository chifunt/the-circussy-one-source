using NUnit.Framework;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Runtime;
using TheCircussyOne.Visuals;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheCircussyOne.Tests
{
    public sealed class HudItemStackSlotFactoryTests
    {
        [Test]
        public void CreateBuildsIconSlotWithVisibleStackCount()
        {
            ItemDefinition item = CreateItem();

            VisualElement slot = HudItemStackSlotFactory.Create(new ItemStackRuntime(item, 3), null, 1f);

            Assert.That(slot, Is.Not.Null);
            Assert.That(slot.ClassListContains("item-stack-slot"), Is.True);
            Assert.That(slot.style.backgroundColor.value, Is.EqualTo(IconFrameBlack()));
            Assert.That(slot.style.borderTopColor.value, Is.EqualTo(item.iconColor));
            Assert.That(slot.Q<Image>(ContentIconVisuals.ImageElementName), Is.Not.Null);
            Label count = slot.Q<Label>(className: "item-stack-count");
            Assert.That(count, Is.Not.Null);
            Assert.That(count.text, Is.EqualTo("3"));
            Assert.That(count.style.display.value, Is.EqualTo(DisplayStyle.Flex));

            Object.DestroyImmediate(item);
        }

        [Test]
        public void CreateHidesCountForSingleStack()
        {
            ItemDefinition item = CreateItem();

            VisualElement slot = HudItemStackSlotFactory.Create(new ItemStackRuntime(item, 1), null, 1f);
            Label count = slot.Q<Label>(className: "item-stack-count");

            Assert.That(count.text, Is.Empty);
            Assert.That(count.style.display.value, Is.EqualTo(DisplayStyle.None));

            Object.DestroyImmediate(item);
        }

        [Test]
        public void ApplyScaleUsesHudConfigValues()
        {
            ItemDefinition item = CreateItem();
            var config = ScriptableObject.CreateInstance<HudVisualConfig>();
            config.itemStackSlotSize = 24f;
            config.itemStackGap = 8f;
            config.itemStackCornerRadius = 3f;
            config.itemStackCountOffset = new Vector2(3f, -2f);
            config.itemStackCountMinWidth = 18f;
            config.itemStackCountHeight = 10f;
            config.itemStackCountFontSize = 6f;

            VisualElement slot = HudItemStackSlotFactory.Create(new ItemStackRuntime(item, 4), config, 2f);
            Label count = slot.Q<Label>(className: "item-stack-count");

            Assert.That(slot.style.width.value.value, Is.EqualTo(48f).Within(0.01f));
            Assert.That(slot.style.height.value.value, Is.EqualTo(48f).Within(0.01f));
            Assert.That(slot.style.marginBottom.value.value, Is.EqualTo(16f).Within(0.01f));
            Assert.That(slot.style.borderTopLeftRadius.value.value, Is.EqualTo(6f).Within(0.01f));
            Assert.That(count.style.right.value.value, Is.EqualTo(6f).Within(0.01f));
            Assert.That(count.style.bottom.value.value, Is.EqualTo(-4f).Within(0.01f));
            Assert.That(count.style.minWidth.value.value, Is.EqualTo(36f).Within(0.01f));
            Assert.That(count.style.height.value.value, Is.EqualTo(20f).Within(0.01f));
            Assert.That(count.style.fontSize.value.value, Is.EqualTo(12f).Within(0.01f));

            Object.DestroyImmediate(config);
            Object.DestroyImmediate(item);
        }

        private static ItemDefinition CreateItem()
        {
            var item = ScriptableObject.CreateInstance<ItemDefinition>();
            item.itemId = "test_item";
            item.displayName = "Test Item";
            item.iconColor = new Color(0.7f, 0.5f, 0.2f, 1f);
            item.maxStacks = 9;
            return item;
        }

        private static Color IconFrameBlack()
        {
            return new Color(0.015f, 0.018f, 0.024f, 0.98f);
        }
    }
}
