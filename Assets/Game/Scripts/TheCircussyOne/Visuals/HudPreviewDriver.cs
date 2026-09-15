using System;
using TheCircussyOne.Config;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TheCircussyOne.Visuals
{
    internal static class HudPreviewDriver
    {
        public static void ApplyConfiguredPreviewIfAllowed(
            HudVisualConfig config,
            bool isPlaying,
            Action<HudPreviewSnapshot> applyPreview)
        {
            if (isPlaying || config == null || applyPreview == null)
            {
                return;
            }

            applyPreview(config.previewEnabled ? config.GetPreviewSnapshot() : HudPreviewSnapshot.Clear);
        }

        public static void ApplySurfaceSize(VisualElement root, HudVisualConfig config, bool isPlaying)
        {
            if (root == null || config == null)
            {
                return;
            }

            if (isPlaying)
            {
                root.style.width = StyleKeyword.Null;
                root.style.height = StyleKeyword.Null;
                return;
            }

            Vector2 previewSize = GetSurfaceSize(config);
            root.style.width = Mathf.Max(100f, previewSize.x);
            root.style.height = Mathf.Max(100f, previewSize.y);
        }

        public static Vector2 GetSurfaceSize(HudVisualConfig config)
        {
            if (config == null)
            {
                return new Vector2(100f, 100f);
            }

            if (config.scaleMode == PanelScaleMode.ScaleWithScreenSize)
            {
                return ReferenceResolution(config);
            }

#if UNITY_EDITOR
            Vector2 gameViewSize = Handles.GetMainGameViewSize();
            if (gameViewSize.x > 0f && gameViewSize.y > 0f)
            {
                return gameViewSize;
            }
#endif

            return ReferenceResolution(config);
        }

        private static Vector2 ReferenceResolution(HudVisualConfig config)
        {
            return new Vector2(
                Mathf.Max(100f, config.referenceResolution.x),
                Mathf.Max(100f, config.referenceResolution.y));
        }
    }
}
