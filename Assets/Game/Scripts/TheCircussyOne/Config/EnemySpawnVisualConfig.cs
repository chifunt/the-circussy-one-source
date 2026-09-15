using Sirenix.OdinInspector;
using UnityEngine;
using TheCircussyOne.Rules;

namespace TheCircussyOne.Config
{
    [CreateAssetMenu(menuName = "The Circussy One/Enemy Spawn Visual Config", fileName = "EnemySpawnVisualConfig")]
    [InfoBox("PROJECTED VISUAL: spawn telegraph tuning applies to the indicator prefab. Spawn rate and enemy balance still live in GameConfig.")]
    public sealed class EnemySpawnVisualConfig : SerializedScriptableObject
    {
        private const string Tabs = "Enemy Spawn Visuals";

        [TabGroup(Tabs, "Indicator"), BoxGroup(Tabs + "/Indicator/Workflow"), LabelWidth(190), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.WarningColor")]
        public bool autoApplyOnChange = true;

        [TabGroup(Tabs, "Indicator"), BoxGroup(Tabs + "/Indicator/Workflow"), LabelWidth(190)]
        public bool enabled = true;

        [TabGroup(Tabs, "Indicator"), BoxGroup(Tabs + "/Indicator/Timing"), LabelWidth(190), SuffixLabel("sec")]
        [ValidateInput(nameof(IsPositive), "Warning duration must be greater than zero.")]
        [Min(0.01f)] public float warningSeconds = 0.85f;

        [TabGroup(Tabs, "Indicator"), BoxGroup(Tabs + "/Indicator/Shape"), LabelWidth(190), SuffixLabel("u")]
        [ValidateInput(nameof(IsPositive), "Indicator radius must be greater than zero.")]
        [Min(0.01f)] public float radius = 1.35f;

        [TabGroup(Tabs, "Indicator"), BoxGroup(Tabs + "/Indicator/Shape"), LabelWidth(190), SuffixLabel("u")]
        [ValidateInput(nameof(IsInnerStartRadiusValid), "Inner start radius must be smaller than the final radius.")]
        [Min(0f)] public float innerStartRadius = 0.08f;

        [TabGroup(Tabs, "Indicator"), BoxGroup(Tabs + "/Indicator/Shape"), LabelWidth(190), SuffixLabel("u")]
        [Min(0f)] public float groundYOffset = 0.035f;

        [TabGroup(Tabs, "Indicator"), BoxGroup(Tabs + "/Indicator/Terrain Placement"), LabelWidth(190), SuffixLabel("u")]
        [Min(0.01f)] public float surfaceProbeHeight = 12f;

        [TabGroup(Tabs, "Indicator"), BoxGroup(Tabs + "/Indicator/Terrain Placement"), LabelWidth(190), SuffixLabel("u")]
        [Min(0.01f)] public float surfaceProbeDepth = 24f;

        [TabGroup(Tabs, "Indicator"), BoxGroup(Tabs + "/Indicator/Terrain Placement"), LabelWidth(190)]
        [Tooltip("Environment layers that can receive enemy spawn indicators. Gameplay layers are always excluded at runtime.")]
        public LayerMask surfaceMask = ~0;

        [TabGroup(Tabs, "Indicator"), BoxGroup(Tabs + "/Indicator/Shape"), LabelWidth(190), SuffixLabel("u")]
        [Min(0.001f)] public float outerThickness = 0.09f;

        [TabGroup(Tabs, "Indicator"), BoxGroup(Tabs + "/Indicator/Shape"), LabelWidth(190), SuffixLabel("u")]
        [Min(0.001f)] public float innerThickness = 0.06f;

        [TabGroup(Tabs, "Indicator"), BoxGroup(Tabs + "/Indicator/Shape"), LabelWidth(190), NumericSlider(0f, 1f)]
        public float centerRadiusMultiplier = 0.72f;

        [TabGroup(Tabs, "Indicator"), BoxGroup(Tabs + "/Indicator/Colors"), LabelWidth(190), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.CombatColor")]
        public Color outerColor = new(1f, 0.28f, 0.08f, 0.82f);

        [TabGroup(Tabs, "Indicator"), BoxGroup(Tabs + "/Indicator/Colors"), LabelWidth(190)]
        public Color innerColor = new(1f, 0.78f, 0.24f, 0.72f);

        [TabGroup(Tabs, "Indicator"), BoxGroup(Tabs + "/Indicator/Colors"), LabelWidth(190)]
        public Color centerColor = new(1f, 0.18f, 0.08f, 0.12f);

        [TabGroup(Tabs, "Indicator"), BoxGroup(Tabs + "/Indicator/Pulse"), LabelWidth(190), NumericSlider(0f, 1f)]
        public float outerPulseStrength = 0.18f;

        [TabGroup(Tabs, "Indicator"), BoxGroup(Tabs + "/Indicator/Pulse"), LabelWidth(190), SuffixLabel("cycles")]
        [Min(0f)] public float outerPulseCycles = 2f;

        [TabGroup(Tabs, "Enemy Emergence"), BoxGroup(Tabs + "/Enemy Emergence/Timing"), LabelWidth(190), SuffixLabel("sec")]
        [ValidateInput(nameof(IsPositive), "Emerge duration must be greater than zero.")]
        [Min(0.01f)] public float emergeSeconds = 0.28f;

        [TabGroup(Tabs, "Enemy Emergence"), BoxGroup(Tabs + "/Enemy Emergence/Body Motion"), LabelWidth(190), SuffixLabel("local y")]
        public float emergeStartBodyY = -0.65f;

        [TabGroup(Tabs, "Enemy Emergence"), BoxGroup(Tabs + "/Enemy Emergence/Body Motion"), LabelWidth(190), SuffixLabel("local y")]
        [ValidateInput(nameof(IsEmergeEndValid), "Final body height must be above the start height.")]
        public float emergeEndBodyY = 1f;

        [TabGroup(Tabs, "Pooling"), BoxGroup(Tabs + "/Pooling/Capacity"), LabelWidth(190)]
        [Min(0)] public int prewarmCount = 0;

        [TabGroup(Tabs, "Indicator"), BoxGroup(Tabs + "/Indicator/Easing"), LabelWidth(190), InlineProperty]
        public EaseSettings innerExpandEase = EaseSettings.OutCubic;

        [TabGroup(Tabs, "Indicator"), BoxGroup(Tabs + "/Indicator/Easing"), LabelWidth(190), InlineProperty]
        public EaseSettings outerPulseEase = EaseSettings.InOutSine;

        [TabGroup(Tabs, "Enemy Emergence"), BoxGroup(Tabs + "/Enemy Emergence/Easing"), LabelWidth(190), InlineProperty]
        public EaseSettings enemyEmergeEase = EaseSettings.OutBack;

        [TabGroup(Tabs, "Preview"), ShowInInspector, ReadOnly, LabelText("Telegraph"), PropertyOrder(100)]
        private string TelegraphSummary => enabled
            ? $"{warningSeconds:0.00}s warning, {emergeSeconds:0.00}s emerge, {radius:0.00}u radius"
            : "Disabled: enemies spawn immediately";

        [TabGroup(Tabs, "Diagnostics"), ShowInInspector, ReadOnly, LabelText("Pending Lifetime"), PropertyOrder(101)]
        private string PendingLifetimeSummary => $"{warningSeconds + emergeSeconds:0.00}s total before active enemy";

        public bool EnsureWorkflowDefaults()
        {
            bool changed = false;
            changed |= EnsureMinimum(ref warningSeconds, 0.85f, 0.01f);
            changed |= EnsureMinimum(ref emergeSeconds, 0.28f, 0.01f);
            changed |= EnsureMinimum(ref radius, 1.35f, 0.01f);
            changed |= EnsureMinimum(ref outerThickness, 0.09f, 0.001f);
            changed |= EnsureMinimum(ref innerThickness, 0.06f, 0.001f);
            changed |= EnsureMinimum(ref surfaceProbeHeight, 12f, 0.01f);
            changed |= EnsureMinimum(ref surfaceProbeDepth, 24f, 0.01f);
            if (surfaceMask.value == 0)
            {
                surfaceMask = ~0;
                changed = true;
            }

            if (innerStartRadius < 0f || innerStartRadius >= radius)
            {
                innerStartRadius = Mathf.Min(0.08f, Mathf.Max(0f, radius * 0.25f));
                changed = true;
            }

            if (emergeEndBodyY <= emergeStartBodyY)
            {
                emergeEndBodyY = 1f;
                emergeStartBodyY = Mathf.Min(emergeStartBodyY, -0.25f);
                changed = true;
            }

            changed |= EnsureEaseDefault(ref innerExpandEase, EaseSettings.OutCubic);
            changed |= EnsureEaseDefault(ref outerPulseEase, EaseSettings.InOutSine);
            changed |= EnsureEaseDefault(ref enemyEmergeEase, EaseSettings.OutBack);
            return changed;
        }

        private void OnValidate()
        {
            EnsureWorkflowDefaults();
        }

        private bool IsPositive(float value) => value > 0f;
        private bool IsInnerStartRadiusValid(float value) => value >= 0f && value < radius;
        private bool IsEmergeEndValid(float value) => value > emergeStartBodyY;

        private static bool EnsureEaseDefault(ref EaseSettings settings, EaseSettings defaultValue)
        {
            if (settings.shape > 0f)
            {
                return false;
            }

            settings = defaultValue;
            return true;
        }

        private static bool EnsureMinimum(ref float value, float defaultValue, float minimum)
        {
            if (value >= minimum)
            {
                return false;
            }

            value = defaultValue;
            return true;
        }
    }
}
