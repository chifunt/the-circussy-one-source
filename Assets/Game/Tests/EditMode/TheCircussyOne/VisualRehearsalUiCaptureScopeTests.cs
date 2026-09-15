using NUnit.Framework;
using TheCircussyOne.VisualTests.Editor;
using TheCircussyOne.VisualTests;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheCircussyOne.Tests.EditMode
{
    public sealed class VisualRehearsalUiCaptureScopeTests
    {
        [Test]
        public void CompositePremultiplied_BlendsOverlayInLayerOrder()
        {
            var destination = new[] { new Color32(100, 50, 0, 255) };
            var overlay = new[] { new Color32(0, 0, 128, 128) };

            VisualTestCaptureUtility.CompositePremultiplied(destination, overlay);

            Assert.That(destination[0].r, Is.EqualTo(50));
            Assert.That(destination[0].g, Is.EqualTo(25));
            Assert.That(destination[0].b, Is.EqualTo(128));
            Assert.That(destination[0].a, Is.EqualTo(255));
        }

        [Test]
        public void ForceOpaque_PreservesColorAndNormalizesAlpha()
        {
            var pixels = new[]
            {
                new Color32(12, 34, 56, 0),
                new Color32(78, 90, 123, 128)
            };

            VisualTestCaptureUtility.ForceOpaque(pixels);

            Assert.That(pixels[0], Is.EqualTo(new Color32(12, 34, 56, 255)));
            Assert.That(pixels[1], Is.EqualTo(new Color32(78, 90, 123, 255)));
        }

        [Test]
        public void ErrorPinkPixelRatio_CountsOnlyUnityErrorPinkPixels()
        {
            var pixels = new[]
            {
                new Color32(255, 0, 255, 255),
                new Color32(250, 20, 245, 255),
                new Color32(220, 0, 255, 255),
                new Color32(255, 80, 255, 255)
            };

            Assert.That(VisualTestCaptureUtility.ErrorPinkPixelRatio(pixels), Is.EqualTo(0.5f));
        }

        [Test]
        public void Scope_RedirectsActiveScreenSpaceDocumentsAndRestoresThem()
        {
            var panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
            var gameObject = new GameObject("Visual Rehearsal UI Capture Test");
            UIDocument document = gameObject.AddComponent<UIDocument>();
            document.panelSettings = panelSettings;

            try
            {
                using (var scope = new VisualRehearsalUiCaptureScope(320, 180))
                {
                    Assert.That(scope.LayerCount, Is.GreaterThanOrEqualTo(1));
                    Assert.That(document.panelSettings, Is.Not.SameAs(panelSettings));
                    Assert.That(document.panelSettings.targetTexture, Is.Not.Null);
                    Assert.That(document.panelSettings.targetTexture.width, Is.EqualTo(320));
                    Assert.That(document.panelSettings.targetTexture.height, Is.EqualTo(180));
                }

                Assert.That(document.panelSettings, Is.SameAs(panelSettings));
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
                Object.DestroyImmediate(panelSettings);
            }
        }

        [Test]
        public void Scope_RejectsTransparentUiEvidence()
        {
            var panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
            var gameObject = new GameObject("Visual Rehearsal Empty UI Test");
            UIDocument document = gameObject.AddComponent<UIDocument>();
            document.panelSettings = panelSettings;

            try
            {
                using var scope = new VisualRehearsalUiCaptureScope(64, 64);

                System.InvalidOperationException exception = Assert.Throws<System.InvalidOperationException>(
                    () => scope.EnsureVisibleContent());
                StringAssert.Contains("visible pixels", exception.Message);
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
                Object.DestroyImmediate(panelSettings);
            }
        }

        [Test]
        public void Scope_RendersOffscreenUiToolkitPanelsOnDemand()
        {
            var panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
            var gameObject = new GameObject("Visual Rehearsal Render UI Test");
            UIDocument document = gameObject.AddComponent<UIDocument>();
            document.panelSettings = panelSettings;

            try
            {
                using var scope = new VisualRehearsalUiCaptureScope(64, 64);
                VisualElement root = document.rootVisualElement;
                root.style.backgroundColor = Color.white;
                root.style.width = 64f;
                root.style.height = 64f;

                scope.RenderNow();

                Assert.DoesNotThrow(() => scope.EnsureVisibleContent(0.5f));
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
                Object.DestroyImmediate(panelSettings);
            }
        }

        [Test]
        public void PresentationScope_RestoresPartialSetupWhenALaterPresetFails()
        {
            var gameObject = new GameObject("Visual Rehearsal Presentation Test");
            Camera camera = gameObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.backgroundColor = Color.blue;
            var background = new VisualRehearsalBackgroundPreset
            {
                mode = VisualRehearsalBackgroundMode.SolidColor,
                color = Color.magenta
            };
            var lighting = new VisualRehearsalLightingPreset
            {
                mode = VisualRehearsalLightingMode.PreserveScene
            };
            var quality = new VisualRehearsalQualityPreset
            {
                qualityLevelName = "__missing_visual_rehearsal_quality__"
            };

            try
            {
                Assert.Throws<System.InvalidOperationException>(() =>
                    new VisualRehearsalPresentationScope(
                        camera,
                        background,
                        lighting,
                        quality,
                        new VisualRehearsalScenarioBinding()));

                Assert.That(camera.clearFlags, Is.EqualTo(CameraClearFlags.Skybox));
                Assert.That(camera.backgroundColor, Is.EqualTo(Color.blue));
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void PresentationScope_TeardownToleratesDestroyedScenarioCamera()
        {
            var gameObject = new GameObject("Visual Almanac Destroyed Camera Test");
            Camera camera = gameObject.AddComponent<Camera>();
            var background = new VisualRehearsalBackgroundPreset
            {
                mode = VisualRehearsalBackgroundMode.PreserveScene
            };
            var lighting = new VisualRehearsalLightingPreset
            {
                mode = VisualRehearsalLightingMode.PreserveScene
            };
            var quality = new VisualRehearsalQualityPreset();
            quality.qualityLevelName = QualitySettings.names[QualitySettings.GetQualityLevel()];
            var scope = new VisualRehearsalPresentationScope(
                camera,
                background,
                lighting,
                quality,
                new VisualRehearsalScenarioBinding());

            Object.DestroyImmediate(gameObject);

            Assert.DoesNotThrow(scope.Dispose);
        }
    }
}
