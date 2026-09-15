using System;
using NUnit.Framework;
using TheCircussyOne.Visuals;
using UnityEngine;

public sealed class VfxParticleTextureFactoryTests
{
    [Test]
    public void CreateProducesReadableShapedAlphaMasksForEveryParticleKind()
    {
        foreach (VfxParticleTextureKind kind in Enum.GetValues(typeof(VfxParticleTextureKind)))
        {
            Texture2D texture = VfxParticleTextureFactory.Create(kind, makeNoLongerReadable: false);
            try
            {
                Assert.That(texture.width, Is.EqualTo(VfxParticleTextureFactory.DefaultSize), kind.ToString());
                Assert.That(texture.height, Is.EqualTo(VfxParticleTextureFactory.DefaultSize), kind.ToString());

                Color[] pixels = texture.GetPixels();
                float minAlpha = 1f;
                float maxAlpha = 0f;
                for (int i = 0; i < pixels.Length; i++)
                {
                    minAlpha = Mathf.Min(minAlpha, pixels[i].a);
                    maxAlpha = Mathf.Max(maxAlpha, pixels[i].a);
                }

                Assert.That(maxAlpha, Is.GreaterThan(0.45f), kind.ToString());
                Assert.That(minAlpha, Is.LessThan(0.05f), kind.ToString());
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }
    }
}
