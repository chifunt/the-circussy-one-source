using NUnit.Framework;
using TheCircussyOne.Config;
using TheCircussyOne.Visuals;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheCircussyOne.Tests
{
    public sealed class HudXpGradientDriverTests
    {
        [Test]
        public void ApplyCreatesGradientTextureAndAssignsItToFill()
        {
            var driver = new HudXpGradientDriver();
            var fill = new VisualElement();
            var config = ScriptableObject.CreateInstance<HudVisualConfig>();
            config.xpFillGradientStartColor = Color.red;
            config.xpFillGradientEndColor = Color.blue;

            driver.Apply(fill, config);

            Assert.That(driver.Texture, Is.Not.Null);
            Assert.That(driver.Texture.width, Is.EqualTo(64));
            Assert.That(driver.Texture.height, Is.EqualTo(1));
            AssertColor(driver.Texture.GetPixel(0, 0), Color.red);
            AssertColor(driver.Texture.GetPixel(63, 0), Color.blue);
            Assert.That(fill.style.backgroundImage.value.texture, Is.SameAs(driver.Texture));

            driver.Dispose(false);
            Object.DestroyImmediate(config);
        }

        [Test]
        public void ApplyReusesTextureWhenColorsDoNotChange()
        {
            var driver = new HudXpGradientDriver();
            var fill = new VisualElement();
            var config = ScriptableObject.CreateInstance<HudVisualConfig>();
            config.xpFillGradientStartColor = Color.green;
            config.xpFillGradientEndColor = Color.yellow;

            driver.Apply(fill, config);
            Texture2D first = driver.Texture;
            driver.Apply(fill, config);

            Assert.That(driver.Texture, Is.SameAs(first));

            driver.Dispose(false);
            Object.DestroyImmediate(config);
        }

        [Test]
        public void DisposeClearsOwnedTexture()
        {
            var driver = new HudXpGradientDriver();
            var fill = new VisualElement();
            var config = ScriptableObject.CreateInstance<HudVisualConfig>();

            driver.Apply(fill, config);
            Assert.That(driver.Texture, Is.Not.Null);

            driver.Dispose(false);

            Assert.That(driver.Texture, Is.Null);
            Object.DestroyImmediate(config);
        }

        private static void AssertColor(Color actual, Color expected)
        {
            Assert.That(actual.r, Is.EqualTo(expected.r).Within(0.01f));
            Assert.That(actual.g, Is.EqualTo(expected.g).Within(0.01f));
            Assert.That(actual.b, Is.EqualTo(expected.b).Within(0.01f));
            Assert.That(actual.a, Is.EqualTo(expected.a).Within(0.01f));
        }
    }
}
