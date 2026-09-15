using System;
using Sirenix.OdinInspector;
using TheCircussyOne.Config;
using UnityEngine;

namespace TheCircussyOne.Content
{
    [Serializable]
    public sealed class ActorModelTransformProfile
    {
        [BoxGroup("Transform"), LabelWidth(180), SuffixLabel("u")]
        public Vector3 localPosition = Vector3.zero;

        [BoxGroup("Transform"), LabelWidth(180), SuffixLabel("deg")]
        public Vector3 localEulerAngles = Vector3.zero;

        [BoxGroup("Transform"), LabelWidth(180)]
        public Vector3 localScale = Vector3.one;

        [BoxGroup("Diagnostics"), ShowInInspector, ReadOnly, LabelText("Runtime Summary"), PropertyOrder(100)]
        private string RuntimeSummary => $"pos {Format(localPosition)}, rot {Format(localEulerAngles)}, scale {Format(localScale)}";

        public bool EnsureWorkflowDefaults()
        {
            bool changed = false;
            changed |= EnsureFinite(ref localPosition, Vector3.zero);
            changed |= EnsureFinite(ref localEulerAngles, Vector3.zero);
            changed |= EnsureValidScale();
            return changed;
        }

        private bool EnsureValidScale()
        {
            if (IsFinite(localScale) && localScale.x > 0f && localScale.y > 0f && localScale.z > 0f)
            {
                return false;
            }

            localScale = Vector3.one;
            return true;
        }

        private static bool EnsureFinite(ref Vector3 value, Vector3 fallback)
        {
            if (IsFinite(value))
            {
                return false;
            }

            value = fallback;
            return true;
        }

        private static bool IsFinite(Vector3 value)
        {
            return IsFinite(value.x) && IsFinite(value.y) && IsFinite(value.z);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static string Format(Vector3 value)
        {
            return $"({value.x:0.##}, {value.y:0.##}, {value.z:0.##})";
        }
    }

    [Serializable]
    public sealed class PerformerAnimationProfile
    {
        [BoxGroup("Clips"), LabelWidth(180), AssetSelector]
        public AnimationClip idle;

        [BoxGroup("Clips"), LabelWidth(180), AssetSelector]
        public AnimationClip run;

        [BoxGroup("Clips"), LabelWidth(180), AssetSelector]
        public AnimationClip jump;

        [BoxGroup("Clips"), LabelWidth(180), AssetSelector]
        public AnimationClip land;

        [BoxGroup("Speed"), LabelWidth(180), Min(0.01f)]
        public float idleSpeed = 1f;

        [BoxGroup("Speed"), LabelWidth(180), Min(0.01f)]
        public float runSpeed = 1f;

        [BoxGroup("Speed"), LabelWidth(180), Min(0.01f)]
        public float jumpSpeed = 1f;

        [BoxGroup("Speed"), LabelWidth(180), Min(0.01f)]
        public float landSpeed = 1f;

        [BoxGroup("Movement Speed Scaling"), LabelWidth(180)]
        public bool scaleRunSpeedWithMovement = true;

        [BoxGroup("Movement Speed Scaling"), LabelWidth(180), Range(0f, 1f)]
        public float minRunSpeedMultiplier = 0.45f;

        [BoxGroup("Movement Speed Scaling"), LabelWidth(180), Min(0f)]
        public float runSpeedSmoothingSharpness = 12f;

        [BoxGroup("Timing"), LabelWidth(180), SuffixLabel("sec")]
        [Min(0f)] public float fadeSeconds = 0.08f;

        [BoxGroup("Timing"), LabelWidth(180), SuffixLabel("sec")]
        [Min(0f)] public float landLockSeconds = 0.12f;

        [BoxGroup("Timing"), LabelWidth(180)]
        [Range(0f, 1f)] public float runThreshold01 = 0.2f;

        [BoxGroup("Timing"), LabelWidth(180)]
        [Range(0f, 1f)] public float runExitThreshold01 = 0.06f;

        [BoxGroup("Diagnostics"), ShowInInspector, ReadOnly, LabelText("Runtime Summary"), PropertyOrder(100)]
        private string RuntimeSummary => $"{ClipName(idle)} / {ClipName(run)} / {ClipName(jump)} / {ClipName(land)}";

        public bool EnsureWorkflowDefaults()
        {
            bool changed = false;
            changed |= EnsureMinimum(ref idleSpeed, 1f, 0.01f);
            changed |= EnsureMinimum(ref runSpeed, 1f, 0.01f);
            changed |= EnsureMinimum(ref jumpSpeed, 1f, 0.01f);
            changed |= EnsureMinimum(ref landSpeed, 1f, 0.01f);
            changed |= EnsureRange(ref minRunSpeedMultiplier, 0.45f, 0f, 1f);
            changed |= EnsureMinimum(ref runSpeedSmoothingSharpness, 12f, 0f);
            changed |= EnsureMinimum(ref fadeSeconds, 0.08f, 0f);
            changed |= EnsureMinimum(ref landLockSeconds, 0.12f, 0f);
            changed |= EnsureRange(ref runThreshold01, 0.2f, 0f, 1f);
            changed |= EnsureRange(ref runExitThreshold01, 0.06f, 0f, 1f);
            if (runExitThreshold01 > runThreshold01)
            {
                runExitThreshold01 = Mathf.Max(0f, runThreshold01 * 0.5f);
                changed = true;
            }

            return changed;
        }

        private static string ClipName(AnimationClip clip)
        {
            return clip != null ? clip.name : "Missing";
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

        private static bool EnsureRange(ref float value, float defaultValue, float minimum, float maximum)
        {
            if (value >= minimum && value <= maximum)
            {
                return false;
            }

            value = defaultValue;
            return true;
        }
    }

    [Serializable]
    public sealed class PerformerMotionVisualProfile
    {
        [BoxGroup("Procedural Bop"), LabelWidth(190)]
        [Tooltip("Use this performer's idle/move bop settings instead of the global Actor Motion Visual Config.")]
        public bool overrideGlobalBop;

        [BoxGroup("Procedural Bop"), LabelWidth(190)]
        [Tooltip("Disable to remove the idle/move squash/stretch bop for this performer while keeping animation clips, jump pulses, and terrain tilt.")]
        public bool proceduralBopEnabled = true;

        [BoxGroup("Idle Bop"), LabelWidth(190), NumericSlider(0f, 0.18f)]
        public float idleAmplitude = 0.018f;

        [BoxGroup("Idle Bop"), LabelWidth(190), SuffixLabel("loops / sec", true), Min(0.01f)]
        public float idleFrequency = 0.7407408f;

        [BoxGroup("Idle Bop"), LabelWidth(190), NumericSlider(0f, 1f)]
        public float idleWidthCompensation = 0.5f;

        [BoxGroup("Move Bop"), LabelWidth(190), NumericSlider(0f, 0.18f)]
        public float moveAmplitude = 0.048f;

        [BoxGroup("Move Bop"), LabelWidth(190), SuffixLabel("loops / sec", true), Min(0.01f)]
        public float moveFrequency = 2.3809524f;

        [BoxGroup("Move Bop"), LabelWidth(190), NumericSlider(0f, 1f)]
        public float moveWidthCompensation = 0.55f;

        [BoxGroup("Diagnostics"), ShowInInspector, ReadOnly, LabelText("Runtime Summary"), PropertyOrder(100)]
        private string RuntimeSummary => overrideGlobalBop
            ? proceduralBopEnabled
                ? $"idle {idleAmplitude:0.000} @ {idleFrequency:0.00}/s, move {moveAmplitude:0.000} @ {moveFrequency:0.00}/s"
                : "Performer bop disabled"
            : "Using global actor motion config";

        public bool EnsureWorkflowDefaults()
        {
            bool changed = false;
            changed |= EnsureRange(ref idleAmplitude, 0.018f, 0f, 0.18f);
            changed |= EnsureMinimum(ref idleFrequency, 0.7407408f, 0.01f);
            changed |= EnsureRange(ref idleWidthCompensation, 0.5f, 0f, 1f);
            changed |= EnsureRange(ref moveAmplitude, 0.048f, 0f, 0.18f);
            changed |= EnsureMinimum(ref moveFrequency, 2.3809524f, 0.01f);
            changed |= EnsureRange(ref moveWidthCompensation, 0.55f, 0f, 1f);
            return changed;
        }

        public ActorSquashStretchCycleSettings IdleSettings(ActorSquashStretchCycleSettings fallback)
        {
            if (!overrideGlobalBop)
            {
                return fallback;
            }

            if (!proceduralBopEnabled)
            {
                return new ActorSquashStretchCycleSettings(0f, fallback.secondsPerCycle, fallback.xzCompensation);
            }

            return new ActorSquashStretchCycleSettings(idleAmplitude, SecondsPerCycle(idleFrequency), idleWidthCompensation);
        }

        public ActorSquashStretchCycleSettings MoveSettings(ActorSquashStretchCycleSettings fallback)
        {
            if (!overrideGlobalBop)
            {
                return fallback;
            }

            if (!proceduralBopEnabled)
            {
                return new ActorSquashStretchCycleSettings(0f, fallback.secondsPerCycle, fallback.xzCompensation);
            }

            return new ActorSquashStretchCycleSettings(moveAmplitude, SecondsPerCycle(moveFrequency), moveWidthCompensation);
        }

        private static float SecondsPerCycle(float frequency)
        {
            return 1f / Mathf.Max(0.01f, frequency);
        }

        private static bool EnsureMinimum(ref float value, float defaultValue, float minimum)
        {
            if (!float.IsNaN(value) && !float.IsInfinity(value) && value >= minimum)
            {
                return false;
            }

            value = defaultValue;
            return true;
        }

        private static bool EnsureRange(ref float value, float defaultValue, float minimum, float maximum)
        {
            if (!float.IsNaN(value) && !float.IsInfinity(value) && value >= minimum && value <= maximum)
            {
                return false;
            }

            value = Mathf.Clamp(defaultValue, minimum, maximum);
            return true;
        }
    }

    [Serializable]
    public sealed class EnemyAnimationProfile
    {
        [BoxGroup("Clips"), LabelWidth(180), AssetSelector]
        public AnimationClip idle;

        [BoxGroup("Clips"), LabelWidth(180), AssetSelector]
        public AnimationClip run;

        [BoxGroup("Clips"), LabelWidth(180), AssetSelector]
        public AnimationClip floatingIdle;

        [BoxGroup("Speed"), LabelWidth(180), Min(0.01f)]
        public float idleSpeed = 1f;

        [BoxGroup("Speed"), LabelWidth(180), Min(0.01f)]
        public float runSpeed = 1f;

        [BoxGroup("Speed"), LabelWidth(180), Min(0.01f)]
        public float floatingIdleSpeed = 1f;

        [BoxGroup("Movement Speed Scaling"), LabelWidth(180)]
        public bool scaleRunSpeedWithMovement = true;

        [BoxGroup("Movement Speed Scaling"), LabelWidth(180), Range(0f, 1f)]
        public float minRunSpeedMultiplier = 0.45f;

        [BoxGroup("Movement Speed Scaling"), LabelWidth(180), Min(0f)]
        public float runSpeedSmoothingSharpness = 12f;

        [BoxGroup("Timing"), LabelWidth(180), SuffixLabel("sec")]
        [Min(0f)] public float fadeSeconds = 0.08f;

        [BoxGroup("Timing"), LabelWidth(180)]
        [Range(0f, 1f)] public float runThreshold01 = 0.15f;

        [BoxGroup("Timing"), LabelWidth(180)]
        [Range(0f, 1f)] public float runExitThreshold01 = 0.05f;

        [BoxGroup("Diagnostics"), ShowInInspector, ReadOnly, LabelText("Runtime Summary"), PropertyOrder(100)]
        private string RuntimeSummary => $"{ClipName(idle)} / {ClipName(run)} / floating {ClipName(floatingIdle)}";

        public bool EnsureWorkflowDefaults()
        {
            bool changed = false;
            changed |= EnsureMinimum(ref idleSpeed, 1f, 0.01f);
            changed |= EnsureMinimum(ref runSpeed, 1f, 0.01f);
            changed |= EnsureMinimum(ref floatingIdleSpeed, 1f, 0.01f);
            changed |= EnsureRange(ref minRunSpeedMultiplier, 0.45f, 0f, 1f);
            changed |= EnsureMinimum(ref runSpeedSmoothingSharpness, 12f, 0f);
            changed |= EnsureMinimum(ref fadeSeconds, 0.08f, 0f);
            changed |= EnsureRange(ref runThreshold01, 0.15f, 0f, 1f);
            changed |= EnsureRange(ref runExitThreshold01, 0.05f, 0f, 1f);
            if (runExitThreshold01 > runThreshold01)
            {
                runExitThreshold01 = Mathf.Max(0f, runThreshold01 * 0.5f);
                changed = true;
            }

            return changed;
        }

        private static string ClipName(AnimationClip clip)
        {
            return clip != null ? clip.name : "Missing";
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

        private static bool EnsureRange(ref float value, float defaultValue, float minimum, float maximum)
        {
            if (value >= minimum && value <= maximum)
            {
                return false;
            }

            value = defaultValue;
            return true;
        }
    }
}
