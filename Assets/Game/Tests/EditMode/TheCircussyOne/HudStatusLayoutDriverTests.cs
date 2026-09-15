using NUnit.Framework;
using TheCircussyOne.Config;
using TheCircussyOne.Visuals;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheCircussyOne.Tests
{
    public sealed class HudStatusLayoutDriverTests
    {
        [Test]
        public void ApplyPositionsAndStylesStatusRowFromHudConfig()
        {
            var xpStrip = new VisualElement();
            var xpTrack = new VisualElement();
            var xpSegments = new VisualElement();
            var xpFill = new VisualElement();
            var xpPulse = new VisualElement();
            var level = new Label();
            var healthCluster = new VisualElement();
            var healthTrack = new VisualElement();
            var healthFill = new VisualElement();
            var healthSegments = new VisualElement();
            var healthLabel = new Label();
            var ticketsCluster = new VisualElement();
            var ticketsIcon = new VisualElement();
            var ticketsValue = new Label();
            var gradient = new HudXpGradientDriver();
            var config = ScriptableObject.CreateInstance<HudVisualConfig>();
            config.globalScale = 1.5f;
            config.xpStripHeight = 20f;
            config.xpSideMargin = 10f;
            config.xpTopPadding = 2f;
            config.xpTrackHeight = 12f;
            config.xpCornerRadius = 4f;
            config.xpFillColor = Color.cyan;
            config.xpFillGradientStartColor = Color.blue;
            config.xpFillGradientEndColor = Color.cyan;
            config.xpLevelUpPulseColor = Color.white;
            config.xpSegmentCount = 5;
            config.healthWidth = 180f;
            config.healthHeight = 18f;
            config.healthOffset = new Vector2(14f, 30f);
            config.healthTextVerticalOffset = -1f;
            config.healthFontSize = 10f;
            config.healthHpPerSegment = 25f;
            config.healthMinimumSegmentPixelSpacing = 0f;
            config.healthSegmentWidth = 1f;
            config.levelBadgeWidth = 24f;
            config.levelBadgeHeight = 24f;
            config.levelBadgeOffset = new Vector2(4f, 0f);
            config.levelBadgeFontSize = 12f;
            config.ticketsLabelText = "TICKETS";
            config.ticketsHeight = 24f;
            config.ticketsIconWidth = 30f;
            config.ticketsIconHeight = 18f;
            config.ticketsIconYOffset = -2f;
            config.ticketsOffsetFromHealth = new Vector2(14f, 0f);
            config.ticketsNumberMinWidth = 42f;
            config.ticketsNumberFontSize = 12f;

            try
            {
                HudStatusLayoutDriver.Apply(
                    new HudStatusLayoutElements(
                        xpStrip,
                        xpTrack,
                        xpSegments,
                        xpFill,
                        xpPulse,
                        level,
                        healthCluster,
                        healthTrack,
                        healthFill,
                        healthSegments,
                        healthLabel,
                        ticketsCluster,
                        ticketsIcon,
                        ticketsValue),
                    config,
                    gradient,
                    100,
                    1.5f);

                Assert.That(xpStrip.style.height.value.value, Is.EqualTo(30f).Within(0.01f));
                Assert.That(xpTrack.style.left.value.value, Is.EqualTo(15f).Within(0.01f));
                Assert.That(xpTrack.style.right.value.value, Is.EqualTo(15f).Within(0.01f));
                Assert.That(xpTrack.style.height.value.value, Is.EqualTo(18f).Within(0.01f));
                Assert.That(xpFill.style.backgroundImage.value.texture, Is.EqualTo(gradient.Texture));
                Assert.That(xpSegments.childCount, Is.EqualTo(4));
                Assert.That(level.style.left.value.value, Is.EqualTo(21f).Within(0.01f));
                Assert.That(level.style.top.value.value, Is.EqualTo(45f).Within(0.01f));
                Assert.That(level.style.width.value.value, Is.EqualTo(36f).Within(0.01f));
                Assert.That(level.style.height.value.value, Is.EqualTo(36f).Within(0.01f));
                Assert.That(level.style.unityTextAlign.value, Is.EqualTo(TextAnchor.MiddleCenter));
                Assert.That(healthCluster.style.left.value.value, Is.EqualTo(63f).Within(0.01f));
                Assert.That(healthCluster.style.top.value.value, Is.EqualTo(49.5f).Within(0.01f));
                Assert.That(healthCluster.style.width.value.value, Is.EqualTo(270f).Within(0.01f));
                Assert.That(healthLabel.style.top.value.value, Is.EqualTo(-1.5f).Within(0.01f));
                Assert.That(healthLabel.style.bottom.value.value, Is.EqualTo(1.5f).Within(0.01f));
                Assert.That(healthSegments.childCount, Is.EqualTo(3));
                Assert.That(ticketsCluster.style.left.value.value, Is.EqualTo(354f).Within(0.01f));
                Assert.That(ticketsCluster.style.top.value.value, Is.EqualTo(45f).Within(0.01f));
                Assert.That(ticketsIcon.style.width.value.value, Is.EqualTo(45f).Within(0.01f));
                Assert.That(ticketsIcon.style.height.value.value, Is.EqualTo(27f).Within(0.01f));
                Assert.That(ticketsIcon.style.top.value.value, Is.EqualTo(-3f).Within(0.01f));
                Assert.That(ticketsValue.style.minWidth.value.value, Is.EqualTo(63f).Within(0.01f));
            }
            finally
            {
                gradient.Dispose(false);
                Object.DestroyImmediate(config);
            }
        }
    }
}
