using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using TheCircussyOne.Rules;

namespace TheCircussyOne.Config
{
    [CreateAssetMenu(menuName = "The Circussy One/XP Gain Counter Visual Config", fileName = "XpGainCounterVisualConfig")]
    [InfoBox("PROJECTED VISUAL: world-space XP gain counter tuning applies to the generated TMP prefab and runtime counter behavior. XP balance still lives in GameConfig.")]
    public sealed class XpGainCounterVisualConfig : SerializedScriptableObject
    {
        private const string Tabs = "XP Gain Counter";

        [TabGroup(Tabs, "Text"), BoxGroup(Tabs + "/Text/Workflow"), LabelWidth(190), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.VisualColor")]
        public bool autoApplyOnChange = true;
        [TabGroup(Tabs, "Text"), BoxGroup(Tabs + "/Text/Workflow"), LabelWidth(190)]
        public bool enabled = true;

        [TabGroup(Tabs, "Text"), BoxGroup(Tabs + "/Text/Typography"), LabelWidth(190), AssetSelector, PreviewField(60)]
        public TMP_FontAsset font;
        [TabGroup(Tabs, "Text"), BoxGroup(Tabs + "/Text/Typography"), LabelWidth(190), SuffixLabel("TMP size")]
        [Min(1f)] public float fontSize = 3.6f;
        [TabGroup(Tabs, "Text"), BoxGroup(Tabs + "/Text/Typography"), LabelWidth(190)]
        [Min(0f)] public float outlineWidth = 0.16f;
        [TabGroup(Tabs, "Text"), BoxGroup(Tabs + "/Text/Typography"), LabelWidth(190), NumericSlider(4f, 24f)]
        public float sdfGradientScale = 12f;
        [TabGroup(Tabs, "Text"), BoxGroup(Tabs + "/Text/Typography"), LabelWidth(190), NumericSlider(-1f, 1f)]
        public float sdfSharpness = 0.3f;
        [TabGroup(Tabs, "Text"), BoxGroup(Tabs + "/Text/Render"), LabelWidth(190)]
        public bool alwaysOnTop = true;
        [TabGroup(Tabs, "Text"), BoxGroup(Tabs + "/Text/Render"), LabelWidth(190)]
        [Min(0)] public int sortingOrder = 245;

        [TabGroup(Tabs, "Position"), BoxGroup(Tabs + "/Position/Camera Relative Offset"), LabelWidth(190), SuffixLabel("u")]
        public float rightOffset = 1.15f;
        [TabGroup(Tabs, "Position"), BoxGroup(Tabs + "/Position/Camera Relative Offset"), LabelWidth(190), SuffixLabel("u")]
        public float heightOffset = 1.55f;
        [TabGroup(Tabs, "Position"), BoxGroup(Tabs + "/Position/Camera Relative Offset"), LabelWidth(190), SuffixLabel("u")]
        public float forwardOffset = 0f;

        [TabGroup(Tabs, "Position"), BoxGroup(Tabs + "/Position/Ticket Counter"), LabelWidth(190), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.CurrencyColor")]
        public bool ticketCounterEnabled = true;
        [TabGroup(Tabs, "Position"), BoxGroup(Tabs + "/Position/Ticket Counter"), LabelWidth(190), SuffixLabel("u")]
        public float ticketRightOffset = -1.15f;
        [TabGroup(Tabs, "Position"), BoxGroup(Tabs + "/Position/Ticket Counter"), LabelWidth(190), SuffixLabel("u")]
        public float ticketHeightOffset = 1.55f;
        [TabGroup(Tabs, "Position"), BoxGroup(Tabs + "/Position/Ticket Counter"), LabelWidth(190), SuffixLabel("u")]
        public float ticketForwardOffset = 0f;

        [TabGroup(Tabs, "Timing"), BoxGroup(Tabs + "/Timing/Lifetime"), LabelWidth(190), SuffixLabel("sec")]
        [Min(0f)] public float holdSeconds = 1f;
        [TabGroup(Tabs, "Timing"), BoxGroup(Tabs + "/Timing/Lifetime"), LabelWidth(190), SuffixLabel("sec")]
        [Min(0.01f)] public float fadeSeconds = 0.5f;
        [TabGroup(Tabs, "Timing"), BoxGroup(Tabs + "/Timing/Lifetime"), LabelWidth(190), SuffixLabel("sec")]
        [Min(0.01f)] public float popReturnSeconds = 0.18f;

        [TabGroup(Tabs, "Scale"), BoxGroup(Tabs + "/Scale/Pop"), LabelWidth(190)]
        [Min(0.01f)] public float baseWorldScale = 0.78f;
        [TabGroup(Tabs, "Scale"), BoxGroup(Tabs + "/Scale/Pop"), LabelWidth(190)]
        [Min(0.01f)] public float normalScale = 1f;
        [TabGroup(Tabs, "Scale"), BoxGroup(Tabs + "/Scale/Pop"), LabelWidth(190)]
        [Min(0.01f)] public float popScale = 1.35f;
        [TabGroup(Tabs, "Scale"), BoxGroup(Tabs + "/Scale/Easing"), LabelWidth(190), InlineProperty]
        public EaseSettings popEase = EaseSettings.OutBack;
        [TabGroup(Tabs, "Scale"), BoxGroup(Tabs + "/Scale/Easing"), LabelWidth(190), InlineProperty]
        public EaseSettings fadeEase = EaseSettings.InCubic;

        [TabGroup(Tabs, "Colors"), BoxGroup(Tabs + "/Colors/Text"), LabelWidth(190), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.VisualColor")]
        public Color textColor = new(0.192f, 0.843f, 1f, 1f);
        [TabGroup(Tabs, "Colors"), BoxGroup(Tabs + "/Colors/Text"), LabelWidth(190)]
        public Color outlineColor = new(0.015f, 0.04f, 0.06f, 0.92f);
        [TabGroup(Tabs, "Colors"), BoxGroup(Tabs + "/Colors/Text"), LabelWidth(190), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.CurrencyColor")]
        public Color ticketTextColor = new(1f, 0.843f, 0.353f, 1f);

        [TabGroup(Tabs, "Preview"), BoxGroup(Tabs + "/Preview/Sample"), LabelWidth(190)]
        [Min(0)] public int previewAmount = 7;
        [TabGroup(Tabs, "Preview"), BoxGroup(Tabs + "/Preview/Sample"), LabelWidth(190), SuffixLabel("sec")]
        [Min(0f)] public float previewSecondsSinceGain = 0.15f;

        [TabGroup(Tabs, "Diagnostics"), ShowInInspector, ReadOnly, LabelText("Workflow")]
        private string WorkflowSummary => enabled
            ? $"XP right {rightOffset:0.##}u, tickets right {ticketRightOffset:0.##}u, hold {holdSeconds:0.##}s, fade {fadeSeconds:0.##}s"
            : "Disabled";

        [TabGroup(Tabs, "Diagnostics"), ShowInInspector, ReadOnly, LabelText("Preview Text")]
        private string PreviewText => $"+{Mathf.Max(0, previewAmount)}";

        public bool EnsureWorkflowDefaults()
        {
            bool changed = false;
            changed |= EnsureMinimum(ref fontSize, 3.6f, 1f);
            changed |= EnsureMinimum(ref outlineWidth, 0.16f, 0f);
            changed |= EnsureRange(ref sdfGradientScale, 12f, 4f, 24f);
            changed |= EnsureRange(ref sdfSharpness, 0.3f, -1f, 1f);
            changed |= EnsureMinimum(ref sortingOrder, 245, 0);
            changed |= EnsureMinimum(ref fadeSeconds, 0.5f, 0.01f);
            changed |= EnsureMinimum(ref popReturnSeconds, 0.18f, 0.01f);
            changed |= EnsureMinimum(ref baseWorldScale, 0.78f, 0.01f);
            changed |= EnsureMinimum(ref normalScale, 1f, 0.01f);
            changed |= EnsureMinimum(ref popScale, 1.35f, 0.01f);
            changed |= EnsureEaseDefault(ref popEase, EaseSettings.OutBack);
            changed |= EnsureEaseDefault(ref fadeEase, EaseSettings.InCubic);
            changed |= EnsureMinimum(ref previewAmount, 7, 0);
            changed |= EnsureMinimum(ref previewSecondsSinceGain, 0.15f, 0f);
            changed |= EnsureVisibleColor(ref textColor, new Color(0.192f, 0.843f, 1f, 1f));
            changed |= EnsureVisibleColor(ref outlineColor, new Color(0.015f, 0.04f, 0.06f, 0.92f));
            changed |= EnsureVisibleColor(ref ticketTextColor, new Color(1f, 0.843f, 0.353f, 1f));
            if (holdSeconds < 0f)
            {
                holdSeconds = 1f;
                changed = true;
            }

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

        private static bool EnsureRange(ref float value, float defaultValue, float minimum, float maximum)
        {
            if (!float.IsNaN(value) && value >= minimum && value <= maximum)
            {
                return false;
            }

            value = Mathf.Clamp(defaultValue, minimum, maximum);
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
