using NUnit.Framework;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;
using TheCircussyOne.Visuals;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheCircussyOne.Tests
{
    public sealed class HudExperienceProgressDriverTests
    {
        [Test]
        public void SetExperienceFirstWriteIsImmediate()
        {
            HudVisualConfig config = CreateConfig(tweenEnabled: true);
            try
            {
                var driver = new HudExperienceProgressDriver();
                var fill = new VisualElement();
                var elements = new HudExperienceProgressElements(fill, new VisualElement());

                driver.SetExperience(elements, 5, 20, config);

                Assert.That(fill.style.width.value.value, Is.EqualTo(25f).Within(0.001f));
            }
            finally
            {
                Object.DestroyImmediate(config);
            }
        }

        [Test]
        public void SetExperienceAnimatesAfterInitialValue()
        {
            HudVisualConfig config = CreateConfig(tweenEnabled: true);
            try
            {
                var driver = new HudExperienceProgressDriver();
                var fill = new VisualElement();
                var elements = new HudExperienceProgressElements(fill, new VisualElement());

                driver.SetImmediate(elements, 0, 10);
                driver.SetExperience(elements, 10, 10, config);
                driver.Tick(elements, 0.1f, config);

                Assert.That(fill.style.width.value.value, Is.EqualTo(50f).Within(0.001f));
            }
            finally
            {
                Object.DestroyImmediate(config);
            }
        }

        [Test]
        public void LevelUpHoldFillsPulsesAndReleasesToHeldRemainder()
        {
            HudVisualConfig config = CreateConfig(tweenEnabled: false);
            config.xpLevelUpPulseSpeed = 0f;
            config.xpLevelUpPulseMaxOpacity = 0.6f;
            try
            {
                var driver = new HudExperienceProgressDriver();
                var fill = new VisualElement();
                var pulse = new VisualElement();
                var elements = new HudExperienceProgressElements(fill, pulse);

                driver.SetImmediate(elements, 2, 10);
                driver.BeginLevelUpHold(elements, config);
                driver.SetExperience(elements, 3, 10, config);
                driver.Tick(elements, 0.1f, config);

                Assert.That(fill.style.width.value.value, Is.EqualTo(100f).Within(0.001f));
                Assert.That(pulse.style.opacity.value, Is.EqualTo(0.6f).Within(0.001f));

                driver.EndLevelUpHold(elements, config);

                Assert.That(fill.style.width.value.value, Is.EqualTo(30f).Within(0.001f));
                Assert.That(pulse.style.opacity.value, Is.Zero);
            }
            finally
            {
                Object.DestroyImmediate(config);
            }
        }

        private static HudVisualConfig CreateConfig(bool tweenEnabled)
        {
            HudVisualConfig config = ScriptableObject.CreateInstance<HudVisualConfig>();
            config.xpBarTween = new HudBarTweenSettings(tweenEnabled, 0.2f, EaseSettings.Linear);
            return config;
        }
    }
}
