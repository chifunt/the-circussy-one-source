using NUnit.Framework;
using TheCircussyOne.Visuals;
using UnityEngine.UIElements;

namespace TheCircussyOne.Tests
{
    public sealed class HudInteractionPromptDriverTests
    {
        [Test]
        public void ApplyShowsKeyboardPromptBeforeHold()
        {
            var root = new VisualElement();
            var glyph = new Label();
            var label = new Label();
            var fill = new VisualElement();

            HudInteractionPromptDriver.Apply(root, glyph, label, fill, "Collect Tickets", 0.25f, false, false, true);

            Assert.That(root.style.display.value, Is.EqualTo(DisplayStyle.Flex));
            Assert.That(glyph.style.display.value, Is.EqualTo(DisplayStyle.Flex));
            Assert.That(glyph.text, Is.EqualTo("E"));
            Assert.That(glyph.ClassListContains("interaction-button-glyph--keyboard"), Is.True);
            Assert.That(glyph.ClassListContains("interaction-button-glyph--gamepad"), Is.False);
            Assert.That(label.text, Is.EqualTo("Hold to Collect Tickets"));
            Assert.That(fill.style.width.value.value, Is.EqualTo(25f).Within(0.01f));
        }

        [Test]
        public void ApplyUsesHoldingCopyAndHidesGlyphWhileHeld()
        {
            var root = new VisualElement();
            var glyph = new Label();
            var label = new Label();
            var fill = new VisualElement();

            HudInteractionPromptDriver.Apply(root, glyph, label, fill, "Open Chest", 0.5f, true, true, true);

            Assert.That(root.style.display.value, Is.EqualTo(DisplayStyle.Flex));
            Assert.That(glyph.style.display.value, Is.EqualTo(DisplayStyle.None));
            Assert.That(glyph.text, Is.EqualTo("X"));
            Assert.That(glyph.ClassListContains("interaction-button-glyph--gamepad"), Is.True);
            Assert.That(label.text, Is.EqualTo("Opening Chest..."));
            Assert.That(fill.style.width.value.value, Is.EqualTo(50f).Within(0.01f));
        }

        [Test]
        public void SetVisibleHidesPromptAndClearsProgress()
        {
            var root = new VisualElement();
            var glyph = new Label();
            var fill = new VisualElement();
            root.style.display = DisplayStyle.Flex;
            glyph.style.display = DisplayStyle.Flex;
            fill.style.width = Length.Percent(65f);

            HudInteractionPromptDriver.SetVisible(root, glyph, fill, false);

            Assert.That(root.style.display.value, Is.EqualTo(DisplayStyle.None));
            Assert.That(glyph.style.display.value, Is.EqualTo(DisplayStyle.None));
            Assert.That(fill.style.width.value.value, Is.Zero);
        }

        [Test]
        public void ApplyHidesBlankPromptWithoutFallbackCopy()
        {
            var root = new VisualElement();
            var glyph = new Label();
            var label = new Label("Interact");
            var fill = new VisualElement();

            HudInteractionPromptDriver.Apply(root, glyph, label, fill, "", 0.25f, false, false, true);

            Assert.That(root.style.display.value, Is.EqualTo(DisplayStyle.None));
            Assert.That(glyph.style.display.value, Is.EqualTo(DisplayStyle.None));
            Assert.That(label.text, Is.Empty);
        }

        [Test]
        public void HoldingPromptTextKeepsUnknownActionsReadable()
        {
            Assert.That(HudInteractionPromptDriver.HoldingPromptText("Spend Tickets"), Is.EqualTo("Spend Tickets..."));
            Assert.That(HudInteractionPromptDriver.HoldingPromptText(""), Is.Empty);
        }
    }
}
