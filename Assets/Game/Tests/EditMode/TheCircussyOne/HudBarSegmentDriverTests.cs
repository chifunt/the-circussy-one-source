using NUnit.Framework;
using TheCircussyOne.Config;
using TheCircussyOne.Visuals;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheCircussyOne.Tests
{
    public sealed class HudBarSegmentDriverTests
    {
        [Test]
        public void ConfigureXpSegmentsBuildsPercentDividers()
        {
            var root = new VisualElement();
            var config = ScriptableObject.CreateInstance<HudVisualConfig>();
            config.xpSegmentCount = 5;
            config.xpSegmentWidth = 2f;
            config.xpSegmentColor = Color.white;
            config.xpSegmentOpacity = 0.5f;

            HudBarSegmentDriver.ConfigureXpSegments(root, config, 2f);

            Assert.That(root.style.display.value, Is.EqualTo(DisplayStyle.Flex));
            Assert.That(root.childCount, Is.EqualTo(4));
            Assert.That(root[0].ClassListContains("xp-segment-divider"), Is.True);
            Assert.That(root[0].style.left.value.value, Is.EqualTo(20f).Within(0.01f));
            Assert.That(root[0].style.width.value.value, Is.EqualTo(4f).Within(0.01f));
            Assert.That(root[0].style.backgroundColor.value.a, Is.EqualTo(0.5f).Within(0.01f));

            Object.DestroyImmediate(config);
        }

        [Test]
        public void ConfigureXpSegmentsHidesWhenSingleSegment()
        {
            var root = new VisualElement();
            root.Add(new VisualElement());
            var config = ScriptableObject.CreateInstance<HudVisualConfig>();
            config.xpSegmentCount = 1;

            HudBarSegmentDriver.ConfigureXpSegments(root, config, 1f);

            Assert.That(root.childCount, Is.Zero);
            Assert.That(root.style.display.value, Is.EqualTo(DisplayStyle.None));

            Object.DestroyImmediate(config);
        }

        [Test]
        public void ConfigureHealthSegmentsUsesAbsoluteHpThresholds()
        {
            var root = new VisualElement();
            var config = ScriptableObject.CreateInstance<HudVisualConfig>();
            config.healthWidth = 200f;
            config.healthHpPerSegment = 25f;
            config.healthSegmentWidth = 2f;
            config.healthSegmentColor = Color.white;
            config.healthSegmentOpacity = 0.4f;

            HudBarSegmentDriver.ConfigureHealthSegments(root, config, 100, 1f);

            Assert.That(root.style.display.value, Is.EqualTo(DisplayStyle.Flex));
            Assert.That(root.childCount, Is.EqualTo(3));
            Assert.That(root[0].ClassListContains("health-segment-divider"), Is.True);
            Assert.That(root[0].style.left.value.value, Is.EqualTo(25f).Within(0.01f));
            Assert.That(root[1].style.left.value.value, Is.EqualTo(50f).Within(0.01f));
            Assert.That(root[2].style.left.value.value, Is.EqualTo(75f).Within(0.01f));
            Assert.That(root[0].style.width.value.value, Is.EqualTo(2f).Within(0.01f));
            Assert.That(root[0].style.backgroundColor.value.a, Is.EqualTo(0.4f).Within(0.01f));

            Object.DestroyImmediate(config);
        }

        [Test]
        public void ConfigureHealthSegmentsHidesWhenTooDense()
        {
            var root = new VisualElement();
            root.Add(new VisualElement());
            var config = ScriptableObject.CreateInstance<HudVisualConfig>();
            config.healthWidth = 100f;
            config.healthHpPerSegment = 25f;
            config.healthMinimumSegmentPixelSpacing = 10f;

            HudBarSegmentDriver.ConfigureHealthSegments(root, config, 1000, 1f);

            Assert.That(root.childCount, Is.Zero);
            Assert.That(root.style.display.value, Is.EqualTo(DisplayStyle.None));

            Object.DestroyImmediate(config);
        }
    }
}
