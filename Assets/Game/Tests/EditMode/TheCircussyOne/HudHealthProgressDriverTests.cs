using NUnit.Framework;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;
using TheCircussyOne.Visuals;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheCircussyOne.Tests
{
    public sealed class HudHealthProgressDriverTests
    {
        [Test]
        public void SetHealthFirstWriteIsImmediateAndRefreshesLabelAndSegments()
        {
            HudVisualConfig config = CreateConfig(tweenEnabled: true);
            try
            {
                var driver = new HudHealthProgressDriver();
                var fill = new VisualElement();
                var label = new Label();
                var segments = new VisualElement();
                var elements = new HudHealthProgressElements(fill, label, segments);

                driver.SetHealth(elements, 75, 100, config, 1f);

                Assert.That(fill.style.width.value.value, Is.EqualTo(75f).Within(0.001f));
                Assert.That(label.text, Is.EqualTo("75 / 100"));
                Assert.That(driver.MaxHealthForSegments, Is.EqualTo(100));
                Assert.That(segments.childCount, Is.EqualTo(3));
            }
            finally
            {
                Object.DestroyImmediate(config);
            }
        }

        [Test]
        public void SetHealthAnimatesAfterInitialValue()
        {
            HudVisualConfig config = CreateConfig(tweenEnabled: true);
            try
            {
                var driver = new HudHealthProgressDriver();
                var fill = new VisualElement();
                var label = new Label();
                var elements = new HudHealthProgressElements(fill, label, new VisualElement());

                driver.SetImmediate(elements, 100, 100, config, 1f);
                driver.SetHealth(elements, 50, 100, config, 1f);
                driver.Tick(elements, 0.1f);

                Assert.That(fill.style.width.value.value, Is.EqualTo(75f).Within(0.001f));
                Assert.That(label.text, Is.EqualTo("50 / 100"));
            }
            finally
            {
                Object.DestroyImmediate(config);
            }
        }

        [Test]
        public void SetImmediateCancelsActiveTween()
        {
            HudVisualConfig config = CreateConfig(tweenEnabled: true);
            try
            {
                var driver = new HudHealthProgressDriver();
                var fill = new VisualElement();
                var elements = new HudHealthProgressElements(fill, new Label(), new VisualElement());

                driver.SetImmediate(elements, 100, 100, config, 1f);
                driver.SetHealth(elements, 0, 100, config, 1f);
                driver.SetImmediate(elements, 25, 100, config, 1f);
                driver.Tick(elements, 1f);

                Assert.That(fill.style.width.value.value, Is.EqualTo(25f).Within(0.001f));
            }
            finally
            {
                Object.DestroyImmediate(config);
            }
        }

        private static HudVisualConfig CreateConfig(bool tweenEnabled)
        {
            HudVisualConfig config = ScriptableObject.CreateInstance<HudVisualConfig>();
            config.healthBarTween = new HudBarTweenSettings(tweenEnabled, 0.2f, EaseSettings.Linear);
            config.healthHpPerSegment = 25f;
            config.healthWidth = 200f;
            config.healthMinimumSegmentPixelSpacing = 0f;
            config.healthSegmentWidth = 1f;
            config.healthSegmentOpacity = 1f;
            return config;
        }
    }
}
