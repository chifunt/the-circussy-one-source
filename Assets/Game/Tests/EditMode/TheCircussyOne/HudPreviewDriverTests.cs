using NUnit.Framework;
using TheCircussyOne.Config;
using TheCircussyOne.Visuals;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheCircussyOne.Tests
{
    public sealed class HudPreviewDriverTests
    {
        [Test]
        public void ApplyConfiguredPreviewUsesConfiguredSnapshotOutsidePlayMode()
        {
            HudVisualConfig config = ScriptableObject.CreateInstance<HudVisualConfig>();
            try
            {
                config.previewEnabled = true;
                config.previewPreset = HudPreviewPreset.Custom;
                config.previewHealth = 64;
                config.previewMaxHealth = 120;
                config.previewLevel = 7;
                HudPreviewSnapshot applied = HudPreviewSnapshot.Clear;

                HudPreviewDriver.ApplyConfiguredPreviewIfAllowed(config, isPlaying: false, snapshot => applied = snapshot);

                Assert.That(applied.CurrentHealth, Is.EqualTo(64));
                Assert.That(applied.MaxHealth, Is.EqualTo(120));
                Assert.That(applied.Level, Is.EqualTo(7));
            }
            finally
            {
                Object.DestroyImmediate(config);
            }
        }

        [Test]
        public void ApplyConfiguredPreviewUsesClearSnapshotWhenPreviewDisabled()
        {
            HudVisualConfig config = ScriptableObject.CreateInstance<HudVisualConfig>();
            try
            {
                config.previewEnabled = false;
                HudPreviewSnapshot applied = new(12, 34, 5, 6, 7, 8, 9f, true);

                HudPreviewDriver.ApplyConfiguredPreviewIfAllowed(config, isPlaying: false, snapshot => applied = snapshot);

                Assert.That(applied.CurrentHealth, Is.EqualTo(HudPreviewSnapshot.Clear.CurrentHealth));
                Assert.That(applied.Level, Is.EqualTo(HudPreviewSnapshot.Clear.Level));
                Assert.That(applied.GameOverVisible, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(config);
            }
        }

        [Test]
        public void ApplyConfiguredPreviewSkipsWhilePlaying()
        {
            HudVisualConfig config = ScriptableObject.CreateInstance<HudVisualConfig>();
            try
            {
                bool called = false;

                HudPreviewDriver.ApplyConfiguredPreviewIfAllowed(config, isPlaying: true, _ => called = true);

                Assert.That(called, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(config);
            }
        }

        [Test]
        public void ApplySurfaceSizeUsesReferenceResolutionForScaleWithScreenSize()
        {
            HudVisualConfig config = ScriptableObject.CreateInstance<HudVisualConfig>();
            try
            {
                config.scaleMode = PanelScaleMode.ScaleWithScreenSize;
                config.referenceResolution = new Vector2(1600f, 900f);
                var root = new VisualElement();

                HudPreviewDriver.ApplySurfaceSize(root, config, isPlaying: false);

                Assert.That(root.style.width.value.value, Is.EqualTo(1600f).Within(0.001f));
                Assert.That(root.style.height.value.value, Is.EqualTo(900f).Within(0.001f));
            }
            finally
            {
                Object.DestroyImmediate(config);
            }
        }

        [Test]
        public void ApplySurfaceSizeReleasesExplicitSizeWhilePlaying()
        {
            HudVisualConfig config = ScriptableObject.CreateInstance<HudVisualConfig>();
            try
            {
                var root = new VisualElement();
                root.style.width = 640f;
                root.style.height = 480f;

                HudPreviewDriver.ApplySurfaceSize(root, config, isPlaying: true);

                Assert.That(root.style.width.keyword, Is.EqualTo(StyleKeyword.Null));
                Assert.That(root.style.height.keyword, Is.EqualTo(StyleKeyword.Null));
            }
            finally
            {
                Object.DestroyImmediate(config);
            }
        }
    }
}
