using UnityEngine;
using UnityEngine.UIElements;

namespace TheCircussyOne.Visuals
{
    public static class ContentIconVisuals
    {
        private static readonly Color FrameBackground = new(0.015f, 0.018f, 0.024f, 0.98f);
        public const string ImageElementName = "content-icon-image";
        public const string ImageClassName = "content-icon-image";

        public static void Apply(VisualElement element, Sprite sprite, Color fallbackColor)
        {
            Apply(element, sprite, fallbackColor, null);
        }

        public static void Apply(VisualElement element, Sprite sprite, Color fallbackColor, Color? borderColor)
        {
            if (element == null)
            {
                return;
            }

            element.style.backgroundColor = FrameBackground;
            element.style.backgroundImage = StyleKeyword.Null;
            SetBorderColor(element, borderColor ?? fallbackColor);
            Image image = ResolveImage(element);
            if (sprite != null && sprite.texture != null)
            {
                image.image = null;
                image.sprite = sprite;
                image.style.display = DisplayStyle.Flex;
            }
            else
            {
                image.sprite = null;
                image.image = null;
                image.style.display = DisplayStyle.None;
            }
        }

        public static void Clear(VisualElement element, Color fallbackColor)
        {
            if (element == null)
            {
                return;
            }

            element.style.backgroundColor = FrameBackground;
            element.style.backgroundImage = StyleKeyword.Null;
            SetBorderColor(element, fallbackColor);
            Image image = element.Q<Image>(ImageElementName);
            if (image != null)
            {
                image.sprite = null;
                image.image = null;
                image.style.display = DisplayStyle.None;
            }
        }

        private static void SetBorderColor(VisualElement element, Color color)
        {
            element.style.borderTopColor = color;
            element.style.borderBottomColor = color;
            element.style.borderLeftColor = color;
            element.style.borderRightColor = color;
        }

        private static Image ResolveImage(VisualElement element)
        {
            Image image = element.Q<Image>(ImageElementName);
            if (image == null)
            {
                image = new Image { name = ImageElementName, pickingMode = PickingMode.Ignore };
                image.AddToClassList(ImageClassName);
                element.Insert(0, image);
            }

            image.scaleMode = ScaleMode.ScaleToFit;
            image.pickingMode = PickingMode.Ignore;
            image.style.position = Position.Absolute;
            image.style.left = 0f;
            image.style.right = 0f;
            image.style.top = 0f;
            image.style.bottom = 0f;
            return image;
        }
    }
}
