using TheCircussyOne.Config;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheCircussyOne.Visuals
{
    internal sealed class HudXpGradientDriver
    {
        private const int TextureWidth = 64;

        private Texture2D texture;
        private Color appliedStart;
        private Color appliedEnd;

        public Texture2D Texture => texture;

        public void Apply(VisualElement fill, HudVisualConfig config)
        {
            if (fill == null || config == null)
            {
                return;
            }

            if (texture == null || appliedStart != config.xpFillGradientStartColor || appliedEnd != config.xpFillGradientEndColor)
            {
                CreateOrUpdate(config.xpFillGradientStartColor, config.xpFillGradientEndColor);
            }

            if (texture != null)
            {
                fill.style.backgroundImage = new StyleBackground(texture);
            }
        }

        public void Dispose(bool isPlaying)
        {
            if (texture == null)
            {
                return;
            }

            if (isPlaying)
            {
                Object.Destroy(texture);
            }
            else
            {
                Object.DestroyImmediate(texture);
            }

            texture = null;
        }

        private void CreateOrUpdate(Color start, Color end)
        {
            if (texture == null)
            {
                texture = new Texture2D(TextureWidth, 1, TextureFormat.RGBA32, false)
                {
                    hideFlags = HideFlags.DontSave,
                    wrapMode = TextureWrapMode.Clamp,
                    filterMode = FilterMode.Bilinear
                };
            }

            for (int x = 0; x < TextureWidth; x++)
            {
                float t = TextureWidth <= 1 ? 0f : (float)x / (TextureWidth - 1);
                texture.SetPixel(x, 0, Color.Lerp(start, end, t));
            }

            texture.Apply(false, false);
            appliedStart = start;
            appliedEnd = end;
        }
    }
}
