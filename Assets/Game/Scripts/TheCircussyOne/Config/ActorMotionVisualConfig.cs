using System;
using Sirenix.OdinInspector;
using UnityEngine;
using TheCircussyOne.Rules;

namespace TheCircussyOne.Config
{
    [CreateAssetMenu(menuName = "The Circussy One/Actor Motion Visual Config", fileName = "ActorMotionVisualConfig")]
    [InfoBox("LIVE RUNTIME: visual-only actor squash/stretch tuning. No scene rebuild or projected asset apply is needed.")]
    public sealed class ActorMotionVisualConfig : SerializedScriptableObject
    {
        private const string Tabs = "Actor Motion Visuals";

        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Workflow"), LabelWidth(190), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.PlayerColor")]
        public bool enabled = true;

        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Idle Breathing"), LabelWidth(190), InlineProperty, HideLabel]
        public ActorSquashStretchCycleSettings playerIdle = new(0.018f, 1.35f, 0.5f);

        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Moving Squash"), LabelWidth(190), InlineProperty, HideLabel]
        public ActorSquashStretchCycleSettings playerMove = new(0.048f, 0.42f, 0.55f);

        [TabGroup(Tabs, "Enemy"), BoxGroup(Tabs + "/Enemy/Idle"), LabelWidth(190), InlineProperty, HideLabel]
        public ActorSquashStretchCycleSettings enemyIdle = new(0f, 1.2f, 0.5f);

        [TabGroup(Tabs, "Enemy"), BoxGroup(Tabs + "/Enemy/Moving Squash"), LabelWidth(190), InlineProperty, HideLabel]
        public ActorSquashStretchCycleSettings enemyMove = new(0.06f, 0.38f, 0.55f);

        [TabGroup(Tabs, "Hit Pulses"), BoxGroup(Tabs + "/Hit Pulses/Player Damage"), LabelWidth(190), InlineProperty, HideLabel]
        public ActorScalePulseSettings playerDamagePulse = new(true, 0.20f, 1.18f, EaseSettings.OutQuad);

        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Jump Takeoff Stretch"), LabelWidth(190), InlineProperty, HideLabel]
        [InfoBox("Takeoff should read as stretch: narrower on X/Z and taller on Y.")]
        public ActorScalePulseSettings playerJumpTakeoffPulse = new(true, 0.16f, 1f, EaseSettings.OutBack, 0.86f, 1.16f, 0.12f);

        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Jump Hold Stretch"), LabelWidth(190)]
        [Tooltip("Visual-only stretch that grows while the jump button is held and the player is still rising.")]
        public bool jumpHoldStretchEnabled = true;

        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Jump Hold Stretch"), LabelWidth(190), SuffixLabel("sec", true)]
        [NumericSlider(0.01f, 0.6f)] public float jumpHoldMaxSeconds = 0.22f;

        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Jump Hold Stretch"), LabelWidth(190)]
        [NumericSlider(0.5f, 1.5f)] public float jumpHoldXZScale = 0.92f;

        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Jump Hold Stretch"), LabelWidth(190)]
        [NumericSlider(0.5f, 1.5f)] public float jumpHoldYScale = 1.10f;

        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Jump Hold Stretch"), LabelWidth(190), SuffixLabel("sec", true)]
        [NumericSlider(0f, 0.4f)] public float jumpHoldReleaseSeconds = 0.10f;

        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Jump Hold Stretch"), LabelWidth(190), InlineProperty]
        public EaseSettings jumpHoldEase = EaseSettings.OutCubic;

        [TabGroup(Tabs, "Player"), BoxGroup(Tabs + "/Player/Jump Landing Squash"), LabelWidth(190), InlineProperty, HideLabel]
        [InfoBox("Landing should read as squash: wider on X/Z and shorter on Y, with optional rebound.")]
        public ActorScalePulseSettings playerJumpLandPulse = new(true, 0.20f, 1f, EaseSettings.OutBack, 1.18f, 0.82f, 0.22f);

        [TabGroup(Tabs, "Hit Pulses"), BoxGroup(Tabs + "/Hit Pulses/Enemy Hit"), LabelWidth(190), InlineProperty, HideLabel]
        public ActorScalePulseSettings enemyHitPulse = new(true, 0.16f, 1.25f, EaseSettings.OutQuad);

        [TabGroup(Tabs, "Smoothing"), BoxGroup(Tabs + "/Smoothing/Speed Response"), LabelWidth(190)]
        [Min(0f)] public float speedSmoothingSharpness = 16f;

        [TabGroup(Tabs, "Smoothing"), BoxGroup(Tabs + "/Smoothing/Speed Response"), LabelWidth(190), InlineProperty]
        public EaseSettings speedSmoothingEase = EaseSettings.Exponential;

        [TabGroup(Tabs, "Terrain Tilt"), BoxGroup(Tabs + "/Terrain Tilt/Incline Alignment"), LabelWidth(190)]
        [Tooltip("Visual-only body tilt toward the walkable surface normal. Roots and gameplay colliders stay upright.")]
        public bool inclineTiltEnabled = true;

        [TabGroup(Tabs, "Terrain Tilt"), BoxGroup(Tabs + "/Terrain Tilt/Incline Alignment"), LabelWidth(190), SuffixLabel("deg", true)]
        [NumericSlider(0f, 35f)] public float playerInclineTiltMaxDegrees = 18f;

        [TabGroup(Tabs, "Terrain Tilt"), BoxGroup(Tabs + "/Terrain Tilt/Incline Alignment"), LabelWidth(190), SuffixLabel("deg", true)]
        [NumericSlider(0f, 35f)] public float enemyInclineTiltMaxDegrees = 14f;

        [TabGroup(Tabs, "Terrain Tilt"), BoxGroup(Tabs + "/Terrain Tilt/Incline Alignment"), LabelWidth(190)]
        [Min(0f)] public float inclineTiltSmoothingSharpness = 18f;

        [TabGroup(Tabs, "Terrain Tilt"), BoxGroup(Tabs + "/Terrain Tilt/Enemy Probe"), LabelWidth(190), SuffixLabel("u", true)]
        [Tooltip("How far below each enemy root to probe for the incline normal used by visual tilt.")]
        [NumericSlider(0f, 2f)] public float enemyInclineGroundProbeDistance = 0.8f;

        [TabGroup(Tabs, "Preview"), ShowInInspector, ReadOnly, LabelText("Player Summary"), PropertyOrder(100)]
        private string PlayerSummary => enabled
            ? $"idle {playerIdle.amplitude:0.000} / move {playerMove.amplitude:0.000}, {playerMove.secondsPerCycle:0.00}s moving cycle"
            : "Disabled";

        [TabGroup(Tabs, "Preview"), ShowInInspector, ReadOnly, LabelText("Enemy Summary"), PropertyOrder(101)]
        private string EnemySummary => enabled
            ? $"move {enemyMove.amplitude:0.000}, {enemyMove.secondsPerCycle:0.00}s cycle"
            : "Disabled";

        [TabGroup(Tabs, "Diagnostics"), ShowInInspector, ReadOnly, LabelText("Scale Owner"), PropertyOrder(102)]
        private string ScaleOwner => "PlayerView and EnemyView body scale only. Roots stay at Vector3.one.";

        public bool EnsureWorkflowDefaults()
        {
            bool changed = false;
            changed |= playerIdle.EnsureDefaults(0.018f, 1.35f, 0.5f);
            changed |= playerMove.EnsureDefaults(0.048f, 0.42f, 0.55f);
            changed |= enemyIdle.EnsureDefaults(0f, 1.2f, 0.5f);
            changed |= enemyMove.EnsureDefaults(0.06f, 0.38f, 0.55f);
            changed |= playerDamagePulse.EnsureDefaults(true, 0.20f, 1.18f, EaseSettings.OutQuad);
            changed |= playerJumpTakeoffPulse.EnsureDefaults(true, 0.16f, 1f, EaseSettings.OutBack, 0.86f, 1.16f, 0.12f);
            changed |= EnsureRange(ref jumpHoldMaxSeconds, 0.22f, 0.01f, 0.6f);
            changed |= EnsureRange(ref jumpHoldXZScale, 0.92f, 0.5f, 1.5f);
            changed |= EnsureRange(ref jumpHoldYScale, 1.10f, 0.5f, 1.5f);
            changed |= EnsureRange(ref jumpHoldReleaseSeconds, 0.10f, 0f, 0.4f);
            changed |= EnsureEaseDefault(ref jumpHoldEase, EaseSettings.OutCubic);
            changed |= playerJumpLandPulse.EnsureDefaults(true, 0.20f, 1f, EaseSettings.OutBack, 1.18f, 0.82f, 0.22f);
            changed |= enemyHitPulse.EnsureDefaults(true, 0.16f, 1.25f, EaseSettings.OutQuad);
            changed |= EnsureMinimum(ref speedSmoothingSharpness, 16f, 0f);
            changed |= EnsureEaseDefault(ref speedSmoothingEase, EaseSettings.Exponential);
            changed |= EnsureRange(ref playerInclineTiltMaxDegrees, 18f, 0f, 35f);
            changed |= EnsureRange(ref enemyInclineTiltMaxDegrees, 14f, 0f, 35f);
            changed |= EnsureMinimum(ref inclineTiltSmoothingSharpness, 18f, 0f);
            changed |= EnsureMinimum(ref enemyInclineGroundProbeDistance, 0.8f, 0f);
            return changed;
        }

        private void OnValidate()
        {
            EnsureWorkflowDefaults();
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

        private static bool EnsureEaseDefault(ref EaseSettings settings, EaseSettings defaultValue)
        {
            if (settings.shape > 0f)
            {
                return false;
            }

            settings = defaultValue;
            return true;
        }
    }

    [Serializable]
    [InlineProperty]
    public struct ActorSquashStretchCycleSettings
    {
        [VerticalGroup("Cycle"), LabelText("Amplitude"), NumericSlider(0f, 0.18f)]
        [Tooltip("How far the visual body scales away from 1.0 during the squash/stretch cycle.")]
        public float amplitude;

        [VerticalGroup("Cycle"), LabelText("Cycle Time"), SuffixLabel("sec / loop", true), Min(0.05f)]
        [Tooltip("How long one full squash/stretch loop takes. Lower values are faster; higher values are slower.")]
        public float secondsPerCycle;

        [VerticalGroup("Cycle"), ShowInInspector, ReadOnly, LabelText("Frequency"), SuffixLabel("loops / sec", true)]
        [Tooltip("Read-only frequency derived from Cycle Time.")]
        private float CyclesPerSecond => secondsPerCycle > 0f ? 1f / secondsPerCycle : 0f;

        [VerticalGroup("Cycle"), LabelText("Width Compensation"), NumericSlider(0f, 1f)]
        [Tooltip("How much the body narrows on X/Z when it stretches upward. 0 keeps width unchanged; 1 fully counterbalances the stretch.")]
        public float xzCompensation;

        public ActorSquashStretchCycleSettings(float amplitude, float secondsPerCycle, float xzCompensation)
        {
            this.amplitude = amplitude;
            this.secondsPerCycle = secondsPerCycle;
            this.xzCompensation = xzCompensation;
        }

        public bool EnsureDefaults(float defaultAmplitude, float defaultSeconds, float defaultXzCompensation)
        {
            bool changed = false;
            if (float.IsNaN(amplitude) || amplitude < 0f)
            {
                amplitude = defaultAmplitude;
                changed = true;
            }

            if (float.IsNaN(secondsPerCycle) || secondsPerCycle < 0.05f)
            {
                secondsPerCycle = defaultSeconds;
                changed = true;
            }

            if (float.IsNaN(xzCompensation) || xzCompensation < 0f || xzCompensation > 1f)
            {
                xzCompensation = defaultXzCompensation;
                changed = true;
            }

            return changed;
        }
    }

    [Serializable]
    [InlineProperty]
    public struct ActorScalePulseSettings
    {
        [VerticalGroup("Pulse"), LabelText("Enabled")]
        public bool enabled;

        [VerticalGroup("Pulse"), LabelText("Pulse Time"), SuffixLabel("sec", true), Min(0.01f)]
        [Tooltip("Total duration of the out-and-back scale pulse.")]
        public float seconds;

        [VerticalGroup("Pulse"), LabelText("Scale Multiplier"), Min(1f)]
        [Tooltip("Legacy uniform peak scale used when XZ/Y peak scale values are unset. Damage and enemy hit pulses still use this as their simple overall pulse.")]
        public float scaleMultiplier;

        [VerticalGroup("Pulse"), LabelText("XZ Peak Scale"), NumericSlider(0.5f, 1.5f)]
        [Tooltip("Peak width/depth scale for directional pulses. Values below 1 narrow the actor; values above 1 widen it.")]
        public float xzScaleMultiplier;

        [VerticalGroup("Pulse"), LabelText("Y Peak Scale"), NumericSlider(0.5f, 1.5f)]
        [Tooltip("Peak vertical scale for directional pulses. Values above 1 stretch upward; values below 1 squash downward.")]
        public float yScaleMultiplier;

        [VerticalGroup("Pulse"), LabelText("Rebound"), NumericSlider(0f, 0.45f)]
        [Tooltip("How much the pulse bounces past rest on the way back. Useful for landing squash.")]
        public float reboundStrength;

        [VerticalGroup("Pulse"), LabelText("Ease"), InlineProperty]
        public EaseSettings ease;

        public float EffectiveXzScaleMultiplier => xzScaleMultiplier > 0.01f ? xzScaleMultiplier : Mathf.Max(0.01f, scaleMultiplier);
        public float EffectiveYScaleMultiplier => yScaleMultiplier > 0.01f ? yScaleMultiplier : Mathf.Max(0.01f, scaleMultiplier);

        public ActorScalePulseSettings(
            bool enabled,
            float seconds,
            float scaleMultiplier,
            EaseSettings ease,
            float xzScaleMultiplier = 0f,
            float yScaleMultiplier = 0f,
            float reboundStrength = 0f)
        {
            this.enabled = enabled;
            this.seconds = seconds;
            this.scaleMultiplier = scaleMultiplier;
            this.ease = ease;
            this.xzScaleMultiplier = xzScaleMultiplier > 0f ? xzScaleMultiplier : scaleMultiplier;
            this.yScaleMultiplier = yScaleMultiplier > 0f ? yScaleMultiplier : scaleMultiplier;
            this.reboundStrength = reboundStrength;
        }

        public bool EnsureDefaults(
            bool defaultEnabled,
            float defaultSeconds,
            float defaultScaleMultiplier,
            EaseSettings defaultEase,
            float defaultXzScaleMultiplier = 0f,
            float defaultYScaleMultiplier = 0f,
            float defaultReboundStrength = 0f)
        {
            bool changed = false;
            if (float.IsNaN(seconds) || seconds < 0.01f)
            {
                seconds = defaultSeconds;
                changed = true;
            }

            if (float.IsNaN(scaleMultiplier) || scaleMultiplier < 1f)
            {
                scaleMultiplier = defaultScaleMultiplier;
                changed = true;
            }

            if (float.IsNaN(xzScaleMultiplier) || xzScaleMultiplier < 0.01f)
            {
                xzScaleMultiplier = defaultXzScaleMultiplier > 0f ? defaultXzScaleMultiplier : scaleMultiplier;
                changed = true;
            }

            if (float.IsNaN(yScaleMultiplier) || yScaleMultiplier < 0.01f)
            {
                yScaleMultiplier = defaultYScaleMultiplier > 0f ? defaultYScaleMultiplier : scaleMultiplier;
                changed = true;
            }

            if (float.IsNaN(reboundStrength) || reboundStrength < 0f || reboundStrength > 0.45f)
            {
                reboundStrength = Mathf.Clamp(defaultReboundStrength, 0f, 0.45f);
                changed = true;
            }

            if (ease.shape <= 0f)
            {
                ease = defaultEase;
                changed = true;
            }

            return changed;
        }
    }
}
