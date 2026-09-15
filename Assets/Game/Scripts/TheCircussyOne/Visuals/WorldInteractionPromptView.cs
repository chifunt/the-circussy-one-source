using Shapes;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;

namespace TheCircussyOne.Visuals
{
    public sealed class WorldInteractionPromptView : MonoBehaviour
    {
        private static readonly int BaseColorProperty = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorProperty = Shader.PropertyToID("_Color");

        [SerializeField] private Transform visualRoot;
        [SerializeField] private TextMeshPro holdPrefixText;
        [SerializeField] private TextMeshPro glyphText;
        [SerializeField] private Disc glyphBackground;
        [SerializeField] private TextMeshPro labelText;
        [SerializeField] private Transform progressRoot;
        [SerializeField] private MeshRenderer progressTrackRenderer;
        [SerializeField] private MeshRenderer progressFillRenderer;

        private MaterialPropertyBlock propertyBlock;
        private Material runtimeBarMaterial;
        private Material sharedTextMaterial;

        public bool IsActive => gameObject.activeInHierarchy;
        public string HoldPrefix => holdPrefixText != null ? holdPrefixText.text : string.Empty;
        public string Label => labelText != null ? labelText.text : string.Empty;
        public string Glyph => glyphText != null ? glyphText.text : string.Empty;
        public float ProgressFillScaleX => progressFillRenderer != null ? progressFillRenderer.transform.localScale.x : 0f;
        public Vector3 ProgressRootLocalPosition => progressRoot != null ? progressRoot.localPosition : Vector3.zero;
        public Vector3 HoldPrefixLocalPosition => holdPrefixText != null ? holdPrefixText.transform.localPosition : Vector3.zero;
        public Vector3 LabelLocalPosition => labelText != null ? labelText.transform.localPosition : Vector3.zero;
        public Vector3 GlyphLocalPosition => glyphText != null ? glyphText.transform.localPosition : Vector3.zero;
        public Vector3 GlyphBackgroundLocalPosition => glyphBackground != null ? glyphBackground.transform.localPosition : Vector3.zero;
        public Vector2 HoldPrefixTextBoxSize => holdPrefixText != null ? holdPrefixText.rectTransform.sizeDelta : Vector2.zero;
        public Vector2 LabelTextBoxSize => labelText != null ? labelText.rectTransform.sizeDelta : Vector2.zero;
        public Vector2 GlyphTextBoxSize => glyphText != null ? glyphText.rectTransform.sizeDelta : Vector2.zero;

        private void Awake()
        {
            ResolveComponents();
        }

        public void ConfigureDefaults(WorldInteractionPromptVisualConfig config, Material textMaterial = null, Material barMaterial = null)
        {
            ResolveComponents();
            ApplyTextDefaults(holdPrefixText, config, textMaterial, TextAlignmentOptions.MidlineLeft, config != null ? config.labelTextBoxSize : new Vector2(2.8f, 0.58f));
            ApplyTextDefaults(glyphText, config, textMaterial, TextAlignmentOptions.Center, config != null ? config.glyphTextBoxSize : new Vector2(0.48f, 0.42f));
            ApplyTextDefaults(labelText, config, textMaterial, TextAlignmentOptions.MidlineLeft, config != null ? config.labelTextBoxSize : new Vector2(2.8f, 0.58f));
            sharedTextMaterial = textMaterial != null ? textMaterial : ResolveSharedTextMaterial();
            ApplySharedTextMaterial(sharedTextMaterial);
            AssignBarMaterial(barMaterial);
            ClearRuntimeContent();
            gameObject.SetActive(false);
        }

        public void ApplyFrame(WorldInteractionPromptFrame frame, WorldInteractionPromptVisualConfig config, Camera camera)
        {
            ResolveComponents();
            if (!frame.Visible || config == null || !config.enabled)
            {
                Deactivate();
                return;
            }

            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            EnsureSharedTextMaterial();
            transform.position = frame.Position;
            transform.localScale = Vector3.one * Mathf.Max(0.001f, frame.Scale);
            if (camera != null)
            {
                Vector3 awayFromCamera = transform.position - camera.transform.position;
                if (awayFromCamera.sqrMagnitude > 0.0001f)
                {
                    transform.rotation = Quaternion.LookRotation(awayFromCamera.normalized, camera.transform.up);
                }
            }

            Color labelColor = frame.ShowProgress ? config.collectingLabelColor : config.labelColor;
            labelColor.a *= Mathf.Clamp01(frame.Alpha);
            Color glyphColor = frame.UseGamepadGlyph ? config.gamepadGlyphColor : config.keyboardGlyphColor;
            glyphColor.a *= Mathf.Clamp01(frame.Alpha);
            Color glyphBackgroundColor = frame.UseGamepadGlyph ? config.gamepadGlyphBackgroundColor : config.keyboardGlyphBackgroundColor;
            glyphBackgroundColor.a *= Mathf.Clamp01(frame.Alpha);
            Color trackColor = config.progressTrackColor;
            Color fillColor = config.progressFillColor;
            trackColor.a *= Mathf.Clamp01(frame.Alpha);
            fillColor.a *= Mathf.Clamp01(frame.Alpha);

            if (visualRoot != null)
            {
                visualRoot.localPosition = Vector3.zero;
                visualRoot.localRotation = Quaternion.identity;
                visualRoot.localScale = Vector3.one;
            }

            ApplyPromptRow(frame, config, labelColor, glyphColor, glyphBackgroundColor);

            ApplyProgress(config, frame, trackColor, fillColor);
        }

        public void Deactivate()
        {
            if (this == null)
            {
                return;
            }

            ClearRuntimeContent();
            gameObject.SetActive(false);
        }

        private void ClearRuntimeContent()
        {
            if (holdPrefixText != null)
            {
                holdPrefixText.text = string.Empty;
                holdPrefixText.gameObject.SetActive(false);
            }

            if (glyphText != null)
            {
                glyphText.text = string.Empty;
                glyphText.gameObject.SetActive(false);
            }

            if (glyphBackground != null)
            {
                glyphBackground.gameObject.SetActive(false);
            }

            if (labelText != null)
            {
                labelText.text = string.Empty;
            }

            if (progressRoot != null)
            {
                progressRoot.gameObject.SetActive(false);
            }
        }

        private void ApplyPromptRow(
            WorldInteractionPromptFrame frame,
            WorldInteractionPromptVisualConfig config,
            Color labelColor,
            Color glyphColor,
            Color glyphBackgroundColor)
        {
            string label = frame.Label ?? string.Empty;
            bool showGlyph = frame.ShowGlyph;
            string holdPrefix = showGlyph ? "Hold" : string.Empty;

            if (holdPrefixText != null)
            {
                holdPrefixText.gameObject.SetActive(showGlyph);
                holdPrefixText.text = holdPrefix;
                holdPrefixText.color = labelColor;
                holdPrefixText.fontSize = config.fontSize;
                holdPrefixText.fontStyle = config.boldText ? FontStyles.Bold : FontStyles.Normal;
                holdPrefixText.richText = true;
            }

            if (labelText != null)
            {
                labelText.text = label;
                labelText.color = labelColor;
                labelText.fontSize = config.fontSize;
                labelText.fontStyle = config.boldText ? FontStyles.Bold : FontStyles.Normal;
                labelText.richText = true;
            }

            if (glyphText != null)
            {
                glyphText.gameObject.SetActive(showGlyph);
                glyphText.text = frame.Glyph ?? string.Empty;
                glyphText.color = glyphColor;
                glyphText.fontSize = config.fontSize * 0.86f;
                glyphText.fontStyle = config.boldText ? FontStyles.Bold : FontStyles.Normal;
            }

            if (glyphBackground != null)
            {
                glyphBackground.gameObject.SetActive(showGlyph);
                glyphBackground.Type = DiscType.Disc;
                glyphBackground.Geometry = DiscGeometry.Flat2D;
                glyphBackground.RadiusSpace = ThicknessSpace.Meters;
                glyphBackground.Radius = Mathf.Max(0.01f, config.glyphRadius);
                glyphBackground.Color = glyphBackgroundColor;
                glyphBackground.SortingOrder = Mathf.Max(0, config.sortingOrder - 1);
                glyphBackground.transform.localRotation = Quaternion.identity;
                glyphBackground.transform.localScale = Vector3.one;
            }

            float labelWidth = ResolvePreferredWidth(
                labelText,
                label,
                showGlyph ? config.labelMaxWidth : config.collectingLabelMaxWidth,
                config.labelTextBoxSize.x);
            float holdPrefixWidth = showGlyph
                ? ResolvePreferredWidth(holdPrefixText, holdPrefix, config.labelMaxWidth, config.labelTextBoxSize.x)
                : 0f;
            var labelBoxSize = new Vector2(labelWidth, Mathf.Max(0.01f, config.labelTextBoxSize.y));
            var holdPrefixBoxSize = new Vector2(holdPrefixWidth, Mathf.Max(0.01f, config.labelTextBoxSize.y));
            if (!showGlyph)
            {
                if (holdPrefixText != null)
                {
                    holdPrefixText.gameObject.SetActive(false);
                }

                ConfigureTextBox(labelText, TextAlignmentOptions.Center, labelBoxSize);
                if (labelText != null)
                {
                    labelText.transform.localPosition = new Vector3(0f, config.rowYOffset, 0f);
                }

                return;
            }

            float glyphRadius = Mathf.Max(0.01f, config.glyphRadius);
            float glyphDiameter = glyphRadius * 2f;
            float gap = Mathf.Max(0f, config.glyphTextGap);
            float rowWidth = holdPrefixWidth + gap + glyphDiameter + gap + labelWidth;
            float rowLeft = -rowWidth * 0.5f;
            float holdPrefixLeftX = rowLeft;
            float glyphCenterX = rowLeft + holdPrefixWidth + gap + glyphRadius;
            float labelLeftX = rowLeft + holdPrefixWidth + gap + glyphDiameter + gap;
            Vector2 glyphBoxSize = new(
                Mathf.Max(config.glyphTextBoxSize.x, glyphDiameter),
                Mathf.Max(0.01f, config.glyphTextBoxSize.y));

            // Layout from measured text widths each frame so localized prompts and
            // shortage messages stay centered around the glyph instead of drifting.
            ConfigureTextBox(holdPrefixText, TextAlignmentOptions.MidlineLeft, holdPrefixBoxSize);
            ConfigureTextBox(glyphText, TextAlignmentOptions.Center, glyphBoxSize);
            ConfigureTextBox(labelText, TextAlignmentOptions.MidlineLeft, labelBoxSize);

            if (holdPrefixText != null)
            {
                holdPrefixText.transform.localPosition = new Vector3(holdPrefixLeftX, config.rowYOffset, 0f);
            }

            if (glyphText != null)
            {
                glyphText.transform.localPosition = new Vector3(glyphCenterX, config.rowYOffset, -0.002f);
            }

            if (glyphBackground != null)
            {
                glyphBackground.transform.localPosition = new Vector3(glyphCenterX, config.rowYOffset, 0f);
            }

            if (labelText != null)
            {
                labelText.transform.localPosition = new Vector3(labelLeftX, config.rowYOffset, 0f);
            }
        }

        private void EnsureSharedTextMaterial()
        {
            if (sharedTextMaterial == null)
            {
                sharedTextMaterial = ResolveSharedTextMaterial();
            }

            ApplySharedTextMaterial(sharedTextMaterial);
        }

        private Material ResolveSharedTextMaterial()
        {
            if (labelText != null && labelText.fontSharedMaterial != null)
            {
                return labelText.fontSharedMaterial;
            }

            if (holdPrefixText != null && holdPrefixText.fontSharedMaterial != null)
            {
                return holdPrefixText.fontSharedMaterial;
            }

            return glyphText != null ? glyphText.fontSharedMaterial : null;
        }

        private void ApplySharedTextMaterial(Material material)
        {
            if (material == null)
            {
                return;
            }

            ApplySharedTextMaterial(holdPrefixText, material);
            ApplySharedTextMaterial(glyphText, material);
            ApplySharedTextMaterial(labelText, material);
        }

        private static void ApplySharedTextMaterial(TextMeshPro text, Material material)
        {
            if (text == null || material == null)
            {
                return;
            }

            text.fontSharedMaterial = material;
            Renderer renderer = text.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }
        }

        private void ApplyProgress(WorldInteractionPromptVisualConfig config, WorldInteractionPromptFrame frame, Color trackColor, Color fillColor)
        {
            bool show = frame.ShowProgress;
            if (progressRoot != null)
            {
                progressRoot.gameObject.SetActive(show);
                progressRoot.localPosition = new Vector3(
                    frame.ProgressShake.x,
                    config.rowYOffset + config.progressBarYOffset + frame.ProgressShake.y,
                    0f);
                progressRoot.localRotation = Quaternion.identity;
                progressRoot.localScale = Vector3.one;
            }

            if (!show)
            {
                return;
            }

            float width = Mathf.Max(0.05f, config.progressBarWidth);
            float height = Mathf.Max(0.01f, config.progressBarHeight);
            float progress = Mathf.Clamp01(frame.Progress);

            if (progressTrackRenderer != null)
            {
                progressTrackRenderer.transform.localPosition = Vector3.zero;
                progressTrackRenderer.transform.localRotation = Quaternion.identity;
                progressTrackRenderer.transform.localScale = new Vector3(width, height, 1f);
                ApplyColor(progressTrackRenderer, trackColor);
            }

            if (progressFillRenderer != null)
            {
                float fillWidth = Mathf.Max(0.0001f, width * progress);
                progressFillRenderer.transform.localPosition = new Vector3(-width * 0.5f + fillWidth * 0.5f, 0f, -0.004f);
                progressFillRenderer.transform.localRotation = Quaternion.identity;
                progressFillRenderer.transform.localScale = new Vector3(fillWidth, height, 1f);
                ApplyColor(progressFillRenderer, fillColor);
            }
        }

        private void ResolveComponents()
        {
            if (visualRoot == null)
            {
                Transform existing = transform.Find("Visual Root");
                if (existing == null)
                {
                    existing = new GameObject("Visual Root").transform;
                    existing.SetParent(transform, false);
                }

                visualRoot = existing;
            }

            if (glyphText == null)
            {
                Transform child = visualRoot.Find("Glyph Text");
                glyphText = child != null ? child.GetComponent<TextMeshPro>() : CreateTextChild("Glyph Text");
            }

            if (holdPrefixText == null)
            {
                Transform child = visualRoot.Find("Hold Text");
                holdPrefixText = child != null ? child.GetComponent<TextMeshPro>() : CreateTextChild("Hold Text");
            }

            if (glyphBackground == null)
            {
                Transform child = visualRoot.Find("Glyph Background");
                glyphBackground = child != null ? child.GetComponent<Disc>() : CreateDiscChild("Glyph Background");
            }

            if (labelText == null)
            {
                Transform child = visualRoot.Find("Prompt Text");
                labelText = child != null ? child.GetComponent<TextMeshPro>() : CreateTextChild("Prompt Text");
            }

            if (progressRoot == null)
            {
                Transform child = visualRoot.Find("Progress Root");
                if (child == null)
                {
                    child = new GameObject("Progress Root").transform;
                    child.SetParent(visualRoot, false);
                }

                progressRoot = child;
            }

            if (progressTrackRenderer == null)
            {
                Transform child = progressRoot.Find("Progress Track");
                progressTrackRenderer = child != null ? child.GetComponent<MeshRenderer>() : CreateQuadChild(progressRoot, "Progress Track");
            }

            if (progressFillRenderer == null)
            {
                Transform child = progressRoot.Find("Progress Fill");
                progressFillRenderer = child != null ? child.GetComponent<MeshRenderer>() : CreateQuadChild(progressRoot, "Progress Fill");
            }

            ConfigureRenderer(progressTrackRenderer, sortingOrder: 258);
            ConfigureRenderer(progressFillRenderer, sortingOrder: 259);
        }

        private TextMeshPro CreateTextChild(string childName)
        {
            var child = new GameObject(childName);
            child.transform.SetParent(visualRoot, false);
            return child.AddComponent<TextMeshPro>();
        }

        private Disc CreateDiscChild(string childName)
        {
            var child = new GameObject(childName);
            child.transform.SetParent(visualRoot, false);
            child.AddComponent<MeshFilter>();
            child.AddComponent<MeshRenderer>();
            return child.AddComponent<Disc>();
        }

        private MeshRenderer CreateQuadChild(Transform parent, string childName)
        {
            GameObject child = GameObject.CreatePrimitive(PrimitiveType.Quad);
            child.name = childName;
            child.transform.SetParent(parent, false);
            Collider collider = child.GetComponent<Collider>();
            if (collider != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(collider);
                }
                else
                {
                    DestroyImmediate(collider);
                }
            }

            return child.GetComponent<MeshRenderer>();
        }

        private static void ApplyTextDefaults(TextMeshPro text, WorldInteractionPromptVisualConfig config, Material material, TextAlignmentOptions alignment, Vector2 textBoxSize)
        {
            if (text == null)
            {
                return;
            }

            if (config != null && config.font != null)
            {
                text.font = config.font;
            }

            if (material != null)
            {
                text.fontSharedMaterial = material;
            }

            text.fontSize = config != null ? config.fontSize : 2.35f;
            text.fontStyle = config != null && config.boldText ? FontStyles.Bold : FontStyles.Normal;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Overflow;
            text.richText = true;
            ConfigureTextBox(text, alignment, textBoxSize);
            Renderer renderer = text.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sortingOrder = config != null ? config.sortingOrder : 260;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                renderer.lightProbeUsage = LightProbeUsage.Off;
                renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            }
        }

        private static void ConfigureTextBox(TextMeshPro text, TextAlignmentOptions alignment, Vector2 textBoxSize)
        {
            if (text == null)
            {
                return;
            }

            text.alignment = alignment;
            text.overflowMode = TextOverflowModes.Overflow;
            RectTransform rectTransform = text.rectTransform;
            if (rectTransform == null)
            {
                return;
            }

            bool leftAnchored = alignment == TextAlignmentOptions.MidlineLeft
                || alignment == TextAlignmentOptions.Left
                || alignment == TextAlignmentOptions.TopLeft
                || alignment == TextAlignmentOptions.BottomLeft;
            rectTransform.pivot = new Vector2(leftAnchored ? 0f : 0.5f, 0.5f);
            rectTransform.sizeDelta = new Vector2(
                Mathf.Max(0.01f, textBoxSize.x),
                Mathf.Max(0.01f, textBoxSize.y));
            rectTransform.localRotation = Quaternion.identity;
            rectTransform.localScale = Vector3.one;
        }

        private static float ResolvePreferredWidth(TextMeshPro text, string value, float maxWidth, float fallbackWidth)
        {
            float fallback = Mathf.Max(0.01f, fallbackWidth);
            float cap = Mathf.Max(0.01f, maxWidth);
            if (text == null)
            {
                return Mathf.Min(fallback, cap);
            }

            Vector2 preferred = text.GetPreferredValues(value ?? string.Empty, 1000f, 0f);
            if (float.IsNaN(preferred.x) || float.IsInfinity(preferred.x) || preferred.x <= 0f)
            {
                return Mathf.Min(fallback, cap);
            }

            return Mathf.Clamp(preferred.x, 0.01f, cap);
        }

        private void AssignBarMaterial(Material material)
        {
            Material resolved = material != null ? material : runtimeBarMaterial;
            if (resolved == null)
            {
                resolved = CreateRuntimeBarMaterial();
                runtimeBarMaterial = resolved;
            }

            if (progressTrackRenderer != null)
            {
                progressTrackRenderer.sharedMaterial = resolved;
            }

            if (progressFillRenderer != null)
            {
                progressFillRenderer.sharedMaterial = resolved;
            }
        }

        private static void ConfigureRenderer(Renderer renderer, int sortingOrder)
        {
            if (renderer == null)
            {
                return;
            }

            renderer.sortingOrder = sortingOrder;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        }

        private void ApplyColor(Renderer renderer, Color color)
        {
            if (renderer == null)
            {
                return;
            }

            propertyBlock ??= new MaterialPropertyBlock();
            renderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(BaseColorProperty, color);
            propertyBlock.SetColor(ColorProperty, color);
            renderer.SetPropertyBlock(propertyBlock);
        }

        private static Material CreateRuntimeBarMaterial()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Unlit/Transparent")
                ?? Shader.Find("Unlit/Color")
                ?? Shader.Find("Sprites/Default");
            if (shader == null)
            {
                return null;
            }

            var material = new Material(shader)
            {
                name = "WorldInteractionPromptRuntimeBar",
                hideFlags = HideFlags.DontSave,
                renderQueue = (int)RenderQueue.Transparent
            };
            return material;
        }
    }
}
