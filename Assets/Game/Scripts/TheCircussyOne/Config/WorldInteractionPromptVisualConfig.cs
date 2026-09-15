using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using TheCircussyOne.Rules;

namespace TheCircussyOne.Config
{
    [CreateAssetMenu(menuName = "The Circussy One/World Interaction Prompt Visual Config", fileName = "WorldInteractionPromptVisualConfig")]
    [InfoBox("PROJECTED VISUAL: world-space interaction prompt tuning applies to the generated TMP prefab and runtime prompt behavior. Interaction rules still live in GameConfig and interactable content.")]
    public sealed class WorldInteractionPromptVisualConfig : SerializedScriptableObject
    {
        private const string Tabs = "World Interaction Prompt";

        [TabGroup(Tabs, "Workflow"), BoxGroup(Tabs + "/Workflow/Apply"), LabelWidth(190), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.VisualColor")]
        public bool autoApplyOnChange = true;
        [TabGroup(Tabs, "Workflow"), BoxGroup(Tabs + "/Workflow/Apply"), LabelWidth(190)]
        public bool enabled = true;

        [TabGroup(Tabs, "Text"), BoxGroup(Tabs + "/Text/Typography"), LabelWidth(190), AssetSelector, PreviewField(60)]
        public TMP_FontAsset font;
        [TabGroup(Tabs, "Text"), BoxGroup(Tabs + "/Text/Typography"), LabelWidth(190), SuffixLabel("TMP size")]
        [Min(0.1f)] public float fontSize = 2.85f;
        [TabGroup(Tabs, "Text"), BoxGroup(Tabs + "/Text/Typography"), LabelWidth(190), Min(0f)]
        public float outlineWidth = 0.28f;
        [TabGroup(Tabs, "Text"), BoxGroup(Tabs + "/Text/Typography"), LabelWidth(190)]
        public bool boldText = true;
        [TabGroup(Tabs, "Text"), BoxGroup(Tabs + "/Text/Typography"), LabelWidth(190), NumericSlider(0f, 1f)]
        public float sdfWeightNormal = 0.32f;
        [TabGroup(Tabs, "Text"), BoxGroup(Tabs + "/Text/Typography"), LabelWidth(190), NumericSlider(0f, 1f)]
        public float sdfWeightBold = 0.82f;
        [TabGroup(Tabs, "Text"), BoxGroup(Tabs + "/Text/Typography"), LabelWidth(190), NumericSlider(4f, 24f)]
        public float sdfGradientScale = 20f;
        [TabGroup(Tabs, "Text"), BoxGroup(Tabs + "/Text/Typography"), LabelWidth(190), NumericSlider(-1f, 1f)]
        public float sdfSharpness = 0.3f;
        [TabGroup(Tabs, "Text"), BoxGroup(Tabs + "/Text/Render"), LabelWidth(190)]
        public bool alwaysOnTop = true;
        [TabGroup(Tabs, "Text"), BoxGroup(Tabs + "/Text/Render"), LabelWidth(190), Min(0)]
        public int sortingOrder = 260;

        [TabGroup(Tabs, "Layout"), BoxGroup(Tabs + "/Layout/Anchor"), LabelWidth(190), SuffixLabel("u")]
        public Vector3 worldOffset = new(0f, 0.72f, 0f);
        [TabGroup(Tabs, "Layout"), BoxGroup(Tabs + "/Layout/Row"), LabelWidth(190), SuffixLabel("u")]
        public float rowYOffset = 0f;
        [TabGroup(Tabs, "Layout"), BoxGroup(Tabs + "/Layout/Row"), LabelWidth(190), SuffixLabel("u"), Min(0f)]
        public float glyphTextGap = 0.16f;
        [TabGroup(Tabs, "Layout"), BoxGroup(Tabs + "/Layout/Row"), LabelWidth(190), SuffixLabel("u"), Min(0.1f)]
        public float labelMaxWidth = 5.25f;
        [TabGroup(Tabs, "Layout"), BoxGroup(Tabs + "/Layout/Row"), LabelWidth(190), SuffixLabel("u"), Min(0.1f)]
        public float collectingLabelMaxWidth = 5.25f;
        [HideInInspector]
        public Vector3 glyphLocalOffset = new(-0.72f, 0.02f, 0f);
        [HideInInspector]
        public Vector3 labelLocalOffset = new(-0.42f, 0f, 0f);
        [TabGroup(Tabs, "Layout"), BoxGroup(Tabs + "/Layout/Glyph"), LabelWidth(190), SuffixLabel("u")]
        public Vector2 glyphTextBoxSize = new(0.58f, 0.52f);
        [TabGroup(Tabs, "Layout"), BoxGroup(Tabs + "/Layout/Text"), LabelWidth(190), SuffixLabel("u")]
        public Vector2 labelTextBoxSize = new(3.4f, 0.72f);
        [TabGroup(Tabs, "Layout"), BoxGroup(Tabs + "/Layout/Glyph"), LabelWidth(190), SuffixLabel("u"), Min(0.01f)]
        public float glyphRadius = 0.23f;
        [TabGroup(Tabs, "Layout"), BoxGroup(Tabs + "/Layout/Progress Bar"), LabelWidth(190), SuffixLabel("u"), Min(0.05f)]
        public float progressBarWidth = 1.55f;
        [TabGroup(Tabs, "Layout"), BoxGroup(Tabs + "/Layout/Progress Bar"), LabelWidth(190), SuffixLabel("u"), Min(0.01f)]
        public float progressBarHeight = 0.12f;
        [TabGroup(Tabs, "Layout"), BoxGroup(Tabs + "/Layout/Progress Bar"), LabelWidth(190), SuffixLabel("u")]
        public float progressBarYOffset = -0.36f;

        [TabGroup(Tabs, "Animation"), BoxGroup(Tabs + "/Animation/Pop"), LabelWidth(190), SuffixLabel("sec"), Min(0.01f)]
        public float showSeconds = 0.16f;
        [TabGroup(Tabs, "Animation"), BoxGroup(Tabs + "/Animation/Pop"), LabelWidth(190), SuffixLabel("sec"), Min(0.01f)]
        public float hideSeconds = 0.12f;
        [TabGroup(Tabs, "Animation"), BoxGroup(Tabs + "/Animation/Scale"), LabelWidth(190), Min(0.01f)]
        public float baseWorldScale = 0.82f;
        [TabGroup(Tabs, "Animation"), BoxGroup(Tabs + "/Animation/Scale"), LabelWidth(190), Min(0.01f)]
        public float targetScale = 1f;
        [TabGroup(Tabs, "Animation"), BoxGroup(Tabs + "/Animation/Easing"), LabelWidth(190), InlineProperty]
        public EaseSettings showEase = EaseSettings.OutBack;
        [TabGroup(Tabs, "Animation"), BoxGroup(Tabs + "/Animation/Easing"), LabelWidth(190), InlineProperty]
        public EaseSettings hideEase = EaseSettings.InBack;

        [TabGroup(Tabs, "Animation"), BoxGroup(Tabs + "/Animation/Progress Shake"), LabelWidth(190), SuffixLabel("u"), Min(0f)]
        public float progressShakeMinAmplitude = 0.004f;
        [TabGroup(Tabs, "Animation"), BoxGroup(Tabs + "/Animation/Progress Shake"), LabelWidth(190), SuffixLabel("u"), Min(0f)]
        public float progressShakeMaxAmplitude = 0.055f;
        [TabGroup(Tabs, "Animation"), BoxGroup(Tabs + "/Animation/Progress Shake"), LabelWidth(190), SuffixLabel("Hz"), Min(0f)]
        public float progressShakeFrequency = 18f;
        [TabGroup(Tabs, "Animation"), BoxGroup(Tabs + "/Animation/Progress Shake"), LabelWidth(190), InlineProperty]
        public EaseSettings progressShakeEase = EaseSettings.InQuad;

        [TabGroup(Tabs, "Colors"), BoxGroup(Tabs + "/Colors/Text"), LabelWidth(190)]
        public Color labelColor = new(1f, 0.97f, 0.78f, 1f);
        [TabGroup(Tabs, "Colors"), BoxGroup(Tabs + "/Colors/Text"), LabelWidth(190)]
        public Color collectingLabelColor = new(1f, 0.86f, 0.24f, 1f);
        [TabGroup(Tabs, "Colors"), BoxGroup(Tabs + "/Colors/Text"), LabelWidth(190)]
        public Color textOutlineColor = new(0.005f, 0.004f, 0.002f, 1f);
        [TabGroup(Tabs, "Colors"), BoxGroup(Tabs + "/Colors/Glyph"), LabelWidth(190)]
        public Color keyboardGlyphColor = Color.white;
        [TabGroup(Tabs, "Colors"), BoxGroup(Tabs + "/Colors/Glyph"), LabelWidth(190)]
        public Color keyboardGlyphBackgroundColor = new(0.035f, 0.03f, 0.02f, 0.92f);
        [TabGroup(Tabs, "Colors"), BoxGroup(Tabs + "/Colors/Glyph"), LabelWidth(190)]
        public Color gamepadGlyphColor = Color.white;
        [TabGroup(Tabs, "Colors"), BoxGroup(Tabs + "/Colors/Glyph"), LabelWidth(190)]
        public Color gamepadGlyphBackgroundColor = new(0.12f, 0.42f, 1f, 0.95f);
        [TabGroup(Tabs, "Colors"), BoxGroup(Tabs + "/Colors/Progress Bar"), LabelWidth(190)]
        public Color progressTrackColor = new(0.035f, 0.03f, 0.02f, 0.82f);
        [TabGroup(Tabs, "Colors"), BoxGroup(Tabs + "/Colors/Progress Bar"), LabelWidth(190)]
        public Color progressFillColor = new(1f, 0.78f, 0.18f, 0.98f);

        [TabGroup(Tabs, "Diagnostics"), ShowInInspector, ReadOnly, LabelText("Workflow")]
        private string WorkflowSummary => enabled
            ? $"Offset {worldOffset.y:0.##}u, show {showSeconds:0.##}s, hide {hideSeconds:0.##}s, shake {progressShakeMaxAmplitude:0.###}u"
            : "Disabled";

        public bool EnsureWorkflowDefaults()
        {
            bool changed = false;
            changed |= EnsureMinimum(ref fontSize, 2.85f, 0.1f);
            changed |= EnsureMinimum(ref outlineWidth, 0.28f, 0f);
            changed |= EnsureRange(ref sdfWeightNormal, 0.32f, 0f, 1f);
            changed |= EnsureRange(ref sdfWeightBold, 0.82f, 0f, 1f);
            changed |= EnsureRange(ref sdfGradientScale, 20f, 4f, 24f);
            changed |= EnsureRange(ref sdfSharpness, 0.3f, -1f, 1f);
            changed |= EnsureMinimum(ref sortingOrder, 260, 0);
            changed |= EnsureReadableRowLayoutDefaults();
            changed |= EnsureFinite(ref rowYOffset, 0f);
            changed |= EnsureMinimum(ref glyphTextGap, 0.16f, 0f);
            changed |= EnsureMinimum(ref labelMaxWidth, 5.25f, 0.1f);
            changed |= EnsureMinimum(ref collectingLabelMaxWidth, 5.25f, 0.1f);
            changed |= EnsureMinimum(ref glyphTextBoxSize, new Vector2(0.58f, 0.52f), 0.01f);
            changed |= EnsureMinimum(ref labelTextBoxSize, new Vector2(3.4f, 0.72f), 0.01f);
            changed |= EnsureMinimum(ref glyphRadius, 0.23f, 0.01f);
            changed |= EnsureMinimum(ref progressBarWidth, 1.55f, 0.05f);
            changed |= EnsureMinimum(ref progressBarHeight, 0.12f, 0.01f);
            changed |= EnsureMinimum(ref showSeconds, 0.16f, 0.01f);
            changed |= EnsureMinimum(ref hideSeconds, 0.12f, 0.01f);
            changed |= EnsureMinimum(ref baseWorldScale, 0.82f, 0.01f);
            changed |= EnsureMinimum(ref targetScale, 1f, 0.01f);
            changed |= EnsureEaseDefault(ref showEase, EaseSettings.OutBack);
            changed |= EnsureEaseDefault(ref hideEase, EaseSettings.InBack);
            changed |= EnsureMinimum(ref progressShakeMinAmplitude, 0.004f, 0f);
            changed |= EnsureMinimum(ref progressShakeMaxAmplitude, 0.055f, 0f);
            changed |= EnsureMinimum(ref progressShakeFrequency, 18f, 0f);
            changed |= EnsureEaseDefault(ref progressShakeEase, EaseSettings.InQuad);
            changed |= EnsureVisibleColor(ref labelColor, new Color(1f, 0.97f, 0.78f, 1f));
            changed |= EnsureVisibleColor(ref collectingLabelColor, new Color(1f, 0.86f, 0.24f, 1f));
            changed |= EnsureVisibleColor(ref textOutlineColor, new Color(0.005f, 0.004f, 0.002f, 1f));
            changed |= EnsureVisibleColor(ref keyboardGlyphColor, Color.white);
            changed |= EnsureVisibleColor(ref keyboardGlyphBackgroundColor, new Color(0.035f, 0.03f, 0.02f, 0.92f));
            changed |= EnsureVisibleColor(ref gamepadGlyphColor, Color.white);
            changed |= EnsureVisibleColor(ref gamepadGlyphBackgroundColor, new Color(0.12f, 0.42f, 1f, 0.95f));
            changed |= EnsureVisibleColor(ref progressTrackColor, new Color(0.035f, 0.03f, 0.02f, 0.82f));
            changed |= EnsureVisibleColor(ref progressFillColor, new Color(1f, 0.78f, 0.18f, 0.98f));
            return changed;
        }

        private void OnValidate()
        {
            EnsureWorkflowDefaults();
        }

        private static bool EnsureEaseDefault(ref EaseSettings settings, EaseSettings defaultValue)
        {
            if (settings.shape > 0f)
            {
                return false;
            }

            settings = defaultValue;
            return true;
        }

        private static bool EnsureMinimum(ref int value, int defaultValue, int minimum)
        {
            if (value >= minimum)
            {
                return false;
            }

            value = defaultValue;
            return true;
        }

        private static bool EnsureMinimum(ref float value, float defaultValue, float minimum)
        {
            if (value >= minimum && !float.IsNaN(value))
            {
                return false;
            }

            value = defaultValue;
            return true;
        }

        private static bool EnsureMinimum(ref Vector2 value, Vector2 defaultValue, float minimum)
        {
            bool invalid = float.IsNaN(value.x)
                || float.IsNaN(value.y)
                || value.x < minimum
                || value.y < minimum;
            if (!invalid)
            {
                return false;
            }

            value = defaultValue;
            return true;
        }

        private static bool EnsureRange(ref float value, float defaultValue, float minimum, float maximum)
        {
            if (!float.IsNaN(value) && value >= minimum && value <= maximum)
            {
                return false;
            }

            value = Mathf.Clamp(defaultValue, minimum, maximum);
            return true;
        }

        private bool EnsureReadableRowLayoutDefaults()
        {
            bool oldOverlappingLayout = Mathf.Abs(glyphLocalOffset.x - -0.52f) < 0.001f
                && Mathf.Abs(labelLocalOffset.x - -0.25f) < 0.001f;
            bool wideCenteredLayout = Mathf.Abs(glyphLocalOffset.x - -1f) < 0.001f
                && Mathf.Abs(labelLocalOffset.x - 0.7f) < 0.001f;
            if (!oldOverlappingLayout && !wideCenteredLayout)
            {
                return false;
            }

            glyphLocalOffset = new Vector3(-0.72f, glyphLocalOffset.y, glyphLocalOffset.z);
            labelLocalOffset = new Vector3(-0.42f, labelLocalOffset.y, labelLocalOffset.z);
            return true;
        }

        private static bool EnsureFinite(ref float value, float defaultValue)
        {
            if (!float.IsNaN(value) && !float.IsInfinity(value))
            {
                return false;
            }

            value = defaultValue;
            return true;
        }

        private static bool EnsureVisibleColor(ref Color color, Color defaultValue)
        {
            if (color.a > 0f)
            {
                return false;
            }

            color = defaultValue;
            return true;
        }
    }
}
