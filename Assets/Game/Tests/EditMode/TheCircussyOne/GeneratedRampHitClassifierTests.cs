using NUnit.Framework;
using TheCircussyOne.Runtime;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TheCircussyOne.Tests
{
    public sealed class GeneratedRampHitClassifierTests
    {
        private GameObject root;
        private GeneratedRampMarker marker;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("Generated Ramp Marker Test");
            marker = root.AddComponent<GeneratedRampMarker>();
            marker.Initialize(
                Vector3.zero,
                Vector3.forward,
                Vector3.right,
                rampHalfWidth: 2f,
                rampHalfLength: 5f,
                rampLowHeight: 0f,
                rampHighHeight: 2f);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(root);
        }

        [Test]
        public void ClassifyReturnsRampTopForWalkableRampSurface()
        {
            GeneratedRampHitKind kind = GeneratedRampHitClassifier.Classify(
                marker,
                new Vector3(0f, 1f, 0f),
                Vector3.up);

            Assert.That(kind, Is.EqualTo(GeneratedRampHitKind.RampTop));
        }

        [Test]
        public void ClassifyReturnsRampSideForSideWall()
        {
            GeneratedRampHitKind kind = GeneratedRampHitClassifier.Classify(
                marker,
                new Vector3(2f, 1f, 0f),
                Vector3.right);

            Assert.That(kind, Is.EqualTo(GeneratedRampHitKind.RampSide));
        }

        [Test]
        public void ClassifyReturnsRampFootForLowEnd()
        {
            GeneratedRampHitKind kind = GeneratedRampHitClassifier.Classify(
                marker,
                new Vector3(0f, 0f, -5f),
                Vector3.back);

            Assert.That(kind, Is.EqualTo(GeneratedRampHitKind.RampFoot));
        }

        [Test]
        public void ClassifyReturnsRampHighEndForUpperEnd()
        {
            GeneratedRampHitKind kind = GeneratedRampHitClassifier.Classify(
                marker,
                new Vector3(0f, 2f, 5f),
                Vector3.forward);

            Assert.That(kind, Is.EqualTo(GeneratedRampHitKind.RampHighEnd));
        }

        [Test]
        public void ClassifyReturnsUnknownOutsideRampFootprint()
        {
            GeneratedRampHitKind kind = GeneratedRampHitClassifier.Classify(
                marker,
                new Vector3(8f, 1f, 0f),
                Vector3.right);

            Assert.That(kind, Is.EqualTo(GeneratedRampHitKind.Unknown));
        }

        [Test]
        public void MarkerInterpolatesHeightAtRampPosition()
        {
            Assert.That(marker.HeightAt(new Vector3(0f, 0f, -5f)), Is.EqualTo(0f).Within(0.001f));
            Assert.That(marker.HeightAt(Vector3.zero), Is.EqualTo(1f).Within(0.001f));
            Assert.That(marker.HeightAt(new Vector3(0f, 0f, 5f)), Is.EqualTo(2f).Within(0.001f));
        }
    }
}
