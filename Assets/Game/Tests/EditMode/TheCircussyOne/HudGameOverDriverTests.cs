using System.Reflection;
using NUnit.Framework;
using TheCircussyOne.Config;
using TheCircussyOne.Visuals;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheCircussyOne.Tests
{
    public sealed class HudGameOverDriverTests
    {
        [Test]
        public void BindRestartButtonRoutesClickToCallbackAndUnbindClearsIt()
        {
            var driver = new HudGameOverDriver();
            var button = new Button();
            int requested = 0;

            driver.BindRestartButton(button, () => requested++);
            InvokeRestartClick(driver);

            Assert.That(requested, Is.EqualTo(1));

            driver.UnbindRestartButton();
            InvokeRestartClick(driver);

            Assert.That(requested, Is.EqualTo(1));
        }

        [Test]
        public void ApplyStretchesTerminalOverlayAndLeavesCardStylingToUss()
        {
            var panel = new VisualElement();
            var title = new Label();
            var subtitle = new Label();
            var config = ScriptableObject.CreateInstance<HudVisualConfig>();
            config.gameOverWidth = 200f;
            config.gameOverHeight = 120f;
            config.gameOverCenterOffset = new Vector2(10f, -5f);
            config.gameOverCornerRadius = 8f;
            config.gameOverBackgroundColor = Color.black;
            config.gameOverBorderColor = Color.red;
            config.gameOverTitleFontSize = 20f;
            config.gameOverTitleColor = Color.white;
            config.gameOverSubtitleFontSize = 10f;
            config.gameOverSubtitleTopMargin = 6f;
            config.gameOverSubtitleColor = Color.gray;

            HudGameOverDriver.Apply(new HudGameOverElements(panel, title, subtitle), config, 1.5f);

            Assert.That(panel.style.left.value.value, Is.EqualTo(0f).Within(0.01f));
            Assert.That(panel.style.right.value.value, Is.EqualTo(0f).Within(0.01f));
            Assert.That(panel.style.top.value.value, Is.EqualTo(0f).Within(0.01f));
            Assert.That(panel.style.bottom.value.value, Is.EqualTo(0f).Within(0.01f));
            Assert.That(panel.style.width.keyword, Is.EqualTo(StyleKeyword.Auto));
            Assert.That(panel.style.height.keyword, Is.EqualTo(StyleKeyword.Auto));
            Assert.That(panel.style.marginLeft.value.value, Is.EqualTo(0f).Within(0.01f));
            Assert.That(panel.style.marginTop.value.value, Is.EqualTo(0f).Within(0.01f));

            Object.DestroyImmediate(config);
        }

        [Test]
        public void SetVisibleTogglesPanelDisplay()
        {
            var panel = new VisualElement();

            HudGameOverDriver.SetVisible(panel, true);
            Assert.That(panel.style.display.value, Is.EqualTo(DisplayStyle.Flex));

            HudGameOverDriver.SetVisible(panel, false);
            Assert.That(panel.style.display.value, Is.EqualTo(DisplayStyle.None));
        }

        private static void InvokeRestartClick(HudGameOverDriver driver)
        {
            MethodInfo method = typeof(HudGameOverDriver).GetMethod("OnRestartClicked", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            method.Invoke(driver, null);
        }
    }
}
