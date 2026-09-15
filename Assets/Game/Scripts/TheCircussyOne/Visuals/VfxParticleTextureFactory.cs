using UnityEngine;

namespace TheCircussyOne.Visuals
{
    public enum VfxParticleTextureKind
    {
        SoftDustPuff,
        SparkDiamond,
        SlashSpark,
        ConfettiStrip,
        TicketPaper,
        SnackHeart
    }

    public static class VfxParticleTextureFactory
    {
        public const int DefaultSize = 64;

        public static Texture2D Create(VfxParticleTextureKind kind, bool makeNoLongerReadable = true, int size = DefaultSize)
        {
            int resolution = Mathf.Max(16, size);
            var texture = new Texture2D(resolution, resolution, TextureFormat.RGBA32, mipChain: true)
            {
                name = $"{kind} Particle Texture",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            Color[] pixels = new Color[resolution * resolution];
            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    pixels[y * resolution + x] = Sample(kind, x, y, resolution);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply(updateMipmaps: true, makeNoLongerReadable: makeNoLongerReadable);
            return texture;
        }

        private static Color Sample(VfxParticleTextureKind kind, int x, int y, int size)
        {
            float u = (x + 0.5f) / size;
            float v = (y + 0.5f) / size;
            float cx = u * 2f - 1f;
            float cy = v * 2f - 1f;
            float alpha = kind switch
            {
                VfxParticleTextureKind.SoftDustPuff => SoftDustAlpha(cx, cy, x, y),
                VfxParticleTextureKind.SparkDiamond => SparkDiamondAlpha(cx, cy),
                VfxParticleTextureKind.SlashSpark => SlashSparkAlpha(cx, cy),
                VfxParticleTextureKind.ConfettiStrip => ConfettiStripAlpha(cx, cy, x, y),
                VfxParticleTextureKind.TicketPaper => TicketPaperAlpha(cx, cy, x, y),
                VfxParticleTextureKind.SnackHeart => SnackHeartAlpha(cx, cy),
                _ => 0f
            };

            return new Color(1f, 1f, 1f, Mathf.Clamp01(alpha));
        }

        private static float SoftDustAlpha(float cx, float cy, int x, int y)
        {
            float distance = Mathf.Sqrt(cx * cx + cy * cy);
            float alpha = SmoothBelow(0.1f, 1f, distance);
            alpha *= alpha;
            float grain = Mathf.Lerp(0.72f, 1f, Hash01(x, y, 13u));
            return alpha * grain;
        }

        private static float SparkDiamondAlpha(float cx, float cy)
        {
            float diamondDistance = Mathf.Abs(cx) + Mathf.Abs(cy);
            float body = SmoothBelow(0.12f, 0.94f, diamondDistance);
            float core = SmoothBelow(0f, 0.28f, diamondDistance);
            return Mathf.Clamp01(body * 0.78f + core * 0.32f);
        }

        private static float SlashSparkAlpha(float cx, float cy)
        {
            Rotate(cx, cy, -34f, out float along, out float across);
            float taper = Mathf.Clamp01(1f - Mathf.Abs(along) * 0.62f);
            float length = SmoothBelow(0.08f, 0.98f, Mathf.Abs(along));
            float width = SmoothBelow(0.018f, 0.065f + taper * 0.055f, Mathf.Abs(across));
            return Mathf.Clamp01(length * width);
        }

        private static float ConfettiStripAlpha(float cx, float cy, int x, int y)
        {
            Rotate(cx, cy, 12f, out float localX, out float localY);
            float body = SmoothBelow(0.48f, 0.62f, Mathf.Abs(localX))
                * SmoothBelow(0.105f, 0.16f, Mathf.Abs(localY));
            float edge = 1f - Mathf.Min(
                SmoothBelow(0.48f, 0.62f, Mathf.Abs(localX)),
                SmoothBelow(0.105f, 0.16f, Mathf.Abs(localY)));
            float wear = edge > 0.25f ? Mathf.Lerp(0.74f, 1f, Hash01(x, y, 37u)) : 1f;
            return body * wear;
        }

        private static float TicketPaperAlpha(float cx, float cy, int x, int y)
        {
            float rect = SmoothBelow(0.74f, 0.86f, Mathf.Abs(cx))
                * SmoothBelow(0.31f, 0.42f, Mathf.Abs(cy));
            float leftNotch = SmoothAbove(0.085f, 0.14f, Distance(cx + 0.82f, cy));
            float rightNotch = SmoothAbove(0.085f, 0.14f, Distance(cx - 0.82f, cy));
            float serration = Mathf.Sin((cy + 1f) * 55f) * 0.012f;
            float sideWear = SmoothAbove(0.79f + serration, 0.86f + serration, Mathf.Abs(cx));
            float randomWear = sideWear < 0.98f ? Mathf.Lerp(0.72f, 1f, Hash01(x, y, 73u)) : 1f;
            return rect * leftNotch * rightNotch * randomWear;
        }

        private static float SnackHeartAlpha(float cx, float cy)
        {
            float x = cx * 1.22f;
            float y = cy * 1.18f - 0.1f;
            float value = Mathf.Pow(x * x + y * y - 0.55f, 3f) - x * x * y * y * y;
            return SmoothBelow(-0.045f, 0.045f, value);
        }

        private static float Distance(float x, float y)
        {
            return Mathf.Sqrt(x * x + y * y);
        }

        private static void Rotate(float x, float y, float degrees, out float rotatedX, out float rotatedY)
        {
            float radians = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);
            rotatedX = x * cos - y * sin;
            rotatedY = x * sin + y * cos;
        }

        private static float SmoothBelow(float inner, float outer, float value)
        {
            float t = Mathf.InverseLerp(inner, outer, value);
            t = Mathf.Clamp01(t);
            return 1f - t * t * (3f - 2f * t);
        }

        private static float SmoothAbove(float inner, float outer, float value)
        {
            float t = Mathf.InverseLerp(inner, outer, value);
            t = Mathf.Clamp01(t);
            return t * t * (3f - 2f * t);
        }

        private static float Hash01(int x, int y, uint seed)
        {
            unchecked
            {
                uint h = (uint)x * 374761393u + (uint)y * 668265263u + seed * 2246822519u;
                h = (h ^ (h >> 13)) * 1274126177u;
                h ^= h >> 16;
                return h / 4294967295f;
            }
        }
    }
}
