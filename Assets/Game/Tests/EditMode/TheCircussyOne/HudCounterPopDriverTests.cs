using NUnit.Framework;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;
using TheCircussyOne.Visuals;
using UnityEngine.UIElements;

namespace TheCircussyOne.Tests
{
    public sealed class HudCounterPopDriverTests
    {
        [Test]
        public void StartAppliesConfiguredScaleAndActivatesState()
        {
            var element = new VisualElement();
            var state = new HudCounterPopState();
            var settings = new HudCounterPopSettings(true, 0.2f, 1.4f, EaseSettings.Linear);

            HudCounterPopDriver.Start(ref state, element, settings);

            Assert.That(state.Active, Is.True);
            Assert.That(state.Elapsed, Is.Zero);
            Assert.That(ScaleX(element), Is.EqualTo(1.4f).Within(0.001f));
        }

        [Test]
        public void TickInterpolatesTowardNormalScaleAndResetsAtDuration()
        {
            var element = new VisualElement();
            var state = new HudCounterPopState();
            var settings = new HudCounterPopSettings(true, 0.2f, 1.4f, EaseSettings.Linear);

            HudCounterPopDriver.Start(ref state, element, settings);
            HudCounterPopDriver.Tick(ref state, element, 0.1f, settings);

            Assert.That(state.Active, Is.True);
            Assert.That(ScaleX(element), Is.EqualTo(1.2f).Within(0.001f));

            HudCounterPopDriver.Tick(ref state, element, 0.1f, settings);

            Assert.That(state.Active, Is.False);
            Assert.That(state.Elapsed, Is.Zero);
            Assert.That(ScaleX(element), Is.EqualTo(1f).Within(0.001f));
        }

        [Test]
        public void DisabledSettingsResetActivePopToNormalScale()
        {
            var element = new VisualElement();
            var state = new HudCounterPopState { Active = true, Elapsed = 0.1f };
            element.style.scale = new Scale(new UnityEngine.Vector3(1.3f, 1.3f, 1f));

            HudCounterPopDriver.Tick(ref state, element, 0.1f, new HudCounterPopSettings(false, 0.2f, 1.4f, EaseSettings.Linear));

            Assert.That(state.Active, Is.False);
            Assert.That(state.Elapsed, Is.Zero);
            Assert.That(ScaleX(element), Is.EqualTo(1f).Within(0.001f));
        }

        private static float ScaleX(VisualElement element)
        {
            return element.style.scale.value.value.x;
        }
    }

    public sealed class UiPopupMotionDriverTests
    {
        [Test]
        public void ShowStartsAtConfiguredScaleAndOpacity()
        {
            var root = new VisualElement();
            var panel = new VisualElement();
            var driver = new UiPopupMotionDriver();
            var settings = new HudPopupMotionSettings(true, 0.2f, 0.2f, 1f, true, EaseSettings.Linear);

            driver.Show(root, panel, settings);

            Assert.That(driver.IsActive, Is.True);
            Assert.That(root.style.display.value, Is.EqualTo(DisplayStyle.Flex));
            Assert.That(ScaleX(panel), Is.EqualTo(0.2f).Within(0.001f));
            Assert.That(Opacity(panel), Is.EqualTo(0f).Within(0.001f));
        }

        [Test]
        public void TickInterpolatesToFullScaleAndOpacity()
        {
            var root = new VisualElement();
            var panel = new VisualElement();
            var driver = new UiPopupMotionDriver();
            var settings = new HudPopupMotionSettings(true, 0.2f, 0.2f, 1f, true, EaseSettings.Linear);

            driver.Show(root, panel, settings);
            driver.Tick(panel, 0.1f, settings);

            Assert.That(driver.IsActive, Is.True);
            Assert.That(ScaleX(panel), Is.EqualTo(0.6f).Within(0.001f));
            Assert.That(Opacity(panel), Is.EqualTo(0.5f).Within(0.001f));

            driver.Tick(panel, 0.1f, settings);

            Assert.That(driver.IsActive, Is.False);
            Assert.That(ScaleX(panel), Is.EqualTo(1f).Within(0.001f));
            Assert.That(Opacity(panel), Is.EqualTo(1f).Within(0.001f));
        }

        [Test]
        public void DisabledMotionShowsAtFullScaleImmediately()
        {
            var root = new VisualElement();
            var panel = new VisualElement();
            var driver = new UiPopupMotionDriver();
            var settings = new HudPopupMotionSettings(false, 0.2f, 0.2f, 1f, true, EaseSettings.Linear);

            driver.Show(root, panel, settings);

            Assert.That(driver.IsActive, Is.False);
            Assert.That(root.style.display.value, Is.EqualTo(DisplayStyle.Flex));
            Assert.That(ScaleX(panel), Is.EqualTo(1f).Within(0.001f));
            Assert.That(Opacity(panel), Is.EqualTo(1f).Within(0.001f));
        }

        [Test]
        public void HideImmediateResetsScaleOpacityAndDisplay()
        {
            var root = new VisualElement();
            var panel = new VisualElement();
            var driver = new UiPopupMotionDriver();
            var settings = new HudPopupMotionSettings(true, 0.2f, 0.2f, 1f, true, EaseSettings.Linear);

            driver.Show(root, panel, settings);
            driver.HideImmediate(root, panel);

            Assert.That(driver.IsActive, Is.False);
            Assert.That(root.style.display.value, Is.EqualTo(DisplayStyle.None));
            Assert.That(ScaleX(panel), Is.EqualTo(1f).Within(0.001f));
            Assert.That(Opacity(panel), Is.EqualTo(1f).Within(0.001f));
        }

        private static float ScaleX(VisualElement element)
        {
            return element.style.scale.value.value.x;
        }

        private static float Opacity(VisualElement element)
        {
            return element.style.opacity.value;
        }
    }
}
