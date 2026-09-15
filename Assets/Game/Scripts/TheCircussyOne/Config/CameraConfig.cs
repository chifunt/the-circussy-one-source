using Sirenix.OdinInspector;
using UnityEngine;
using TheCircussyOne.Rules;

namespace TheCircussyOne.Config
{
    [CreateAssetMenu(menuName = "The Circussy One/Camera Config", fileName = "CameraConfig")]
    [InfoBox("LIVE RUNTIME: camera feel tuning does not need scene rebuild. Restart Play Mode to refresh starting pose values.")]
    public sealed class CameraConfig : SerializedScriptableObject
    {
        private const string Tabs = "Camera Config";
        private const int ObstructionDefaultsVersion = 2;
        private const int TriggerOrbitDefaultsVersion = 1;

        [TabGroup(Tabs, "Orbit"), BoxGroup(Tabs + "/Orbit/Follow"), LabelWidth(190), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.CameraColor")]
        [NumericSlider(1f, 30f)] public float followSharpness = 9f;
        [TabGroup(Tabs, "Orbit"), BoxGroup(Tabs + "/Orbit/Follow"), LabelWidth(190), SuffixLabel("u")]
        [Min(1f)] public float orbitDistance = 20f;
        [TabGroup(Tabs, "Orbit"), BoxGroup(Tabs + "/Orbit/Starting Pose"), LabelWidth(190), SuffixLabel("deg")]
        public float orbitYaw = 0f;
        [TabGroup(Tabs, "Orbit"), BoxGroup(Tabs + "/Orbit/Starting Pose"), LabelWidth(190), SuffixLabel("deg")]
        [ValidateInput(nameof(IsOrbitPitchValid), "Starting pitch must be between min and max pitch.")]
        [NumericSlider(-45f, 85f)] public float orbitPitch = 36f;
        [TabGroup(Tabs, "Orbit"), BoxGroup(Tabs + "/Orbit/Pitch Limits"), LabelWidth(190), SuffixLabel("deg")]
        [ValidateInput(nameof(IsMinPitchValid), "Min pitch cannot exceed max pitch.")]
        [NumericSlider(-45f, 85f)] public float minPitch = 20f;
        [TabGroup(Tabs, "Orbit"), BoxGroup(Tabs + "/Orbit/Pitch Limits"), LabelWidth(190), SuffixLabel("deg")]
        [ValidateInput(nameof(IsMaxPitchValid), "Max pitch cannot be below min pitch.")]
        [NumericSlider(-45f, 85f)] public float maxPitch = 58f;
        [TabGroup(Tabs, "Orbit"), BoxGroup(Tabs + "/Orbit/Follow"), LabelWidth(190), SuffixLabel("u")]
        [Min(0f)] public float targetHeight = 1.35f;

        [TabGroup(Tabs, "Lens"), BoxGroup(Tabs + "/Lens/Clipping"), LabelWidth(190), SuffixLabel("u")]
        [Tooltip("How far the game camera can render. Generated Acts are large, so keep this comfortably above the largest Act radius.")]
        [Min(100f)] public float farClipPlane = 2500f;

        [TabGroup(Tabs, "Look Ahead"), BoxGroup(Tabs + "/Look Ahead/Dynamic Framing"), LabelWidth(190), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.PlayerColor")]
        [Tooltip("Maximum forward framing when the camera is at its lowest/behind-the-player pitch.")]
        [SuffixLabel("u")]
        [Min(0f)] public float lookAheadDistance = 9f;
        [TabGroup(Tabs, "Look Ahead"), BoxGroup(Tabs + "/Look Ahead/Dynamic Framing"), LabelWidth(190), SuffixLabel("u")]
        [Tooltip("Forward framing when the camera is at its highest/top-down pitch. Use 0 to center the player.")]
        [Min(0f)] public float topDownLookAheadDistance = 0f;
        [TabGroup(Tabs, "Look Ahead"), BoxGroup(Tabs + "/Look Ahead/Dynamic Framing"), LabelWidth(190), NumericSlider(0.25f, 3f)]
        [Tooltip("Controls how quickly look-ahead fades as pitch moves toward top-down. 1 is linear; higher values keep more centering near top-down.")]
        public float lookAheadPitchBias = 1.15f;

        [TabGroup(Tabs, "Obstruction"), BoxGroup(Tabs + "/Obstruction/Collision"), LabelWidth(190), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.WarningColor")]
        [Tooltip("Prevents the third-person camera from clipping through floor and environment colliders.")]
        public bool preventCameraClipping = true;
        [TabGroup(Tabs, "Obstruction"), BoxGroup(Tabs + "/Obstruction/Collision"), LabelWidth(190), EnableIf(nameof(preventCameraClipping))]
        [Tooltip("Layers that can block the camera. Gameplay layers are always ignored at runtime.")]
        public LayerMask cameraObstructionMask = 1;
        [TabGroup(Tabs, "Obstruction"), BoxGroup(Tabs + "/Obstruction/Collision"), LabelWidth(190), EnableIf(nameof(preventCameraClipping)), SuffixLabel("u")]
        [Tooltip("Sphere radius used for camera obstruction checks.")]
        [Min(0.01f)] public float cameraObstructionRadius = 0.28f;
        [TabGroup(Tabs, "Obstruction"), BoxGroup(Tabs + "/Obstruction/Collision"), LabelWidth(190), EnableIf(nameof(preventCameraClipping)), SuffixLabel("u")]
        [Tooltip("Extra distance kept between the camera and obstruction hit surfaces.")]
        [Min(0f)] public float cameraObstructionPadding = 0.08f;

        [TabGroup(Tabs, "Sensitivity"), BoxGroup(Tabs + "/Sensitivity/Master"), LabelWidth(190), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.CameraColor"), NumericSlider(0f, 3f)]
        [Tooltip("One-stop camera look sensitivity multiplier. 1 keeps the tuned defaults, 0.5 halves sensitivity, 2 doubles it.")]
        public float lookSensitivityMultiplier = 1f;
        [TabGroup(Tabs, "Sensitivity"), BoxGroup(Tabs + "/Sensitivity/Mouse"), LabelWidth(190), SuffixLabel("deg/px")]
        [Tooltip("Base mouse horizontal camera orbit sensitivity in degrees per screen pixel.")]
        [Min(0f)] public float mouseYawSensitivity = 0.08f;
        [TabGroup(Tabs, "Sensitivity"), BoxGroup(Tabs + "/Sensitivity/Mouse"), LabelWidth(190), SuffixLabel("deg/px")]
        [Tooltip("Base mouse vertical camera orbit sensitivity in degrees per screen pixel.")]
        [Min(0f)] public float mousePitchSensitivity = 0.06f;
        [TabGroup(Tabs, "Sensitivity"), BoxGroup(Tabs + "/Sensitivity/Gamepad"), LabelWidth(190), SuffixLabel("deg/sec")]
        [Tooltip("Base gamepad right-stick horizontal camera orbit speed in degrees per second.")]
        [Min(0f)] public float gamepadYawSpeed = 130f;
        [TabGroup(Tabs, "Sensitivity"), BoxGroup(Tabs + "/Sensitivity/Gamepad"), LabelWidth(190), SuffixLabel("deg/sec")]
        [Tooltip("Base gamepad right-stick vertical camera orbit speed in degrees per second.")]
        [Min(0f)] public float gamepadPitchSpeed = 90f;
        [TabGroup(Tabs, "Sensitivity"), BoxGroup(Tabs + "/Sensitivity/Gamepad"), LabelWidth(190), SuffixLabel("deg/sec")]
        [Tooltip("Yaw-only camera orbit speed when using left/right controller triggers.")]
        [Min(0f)] public float gamepadTriggerYawSpeed = 130f;
        [TabGroup(Tabs, "Sensitivity"), BoxGroup(Tabs + "/Sensitivity/Gamepad"), LabelWidth(190), NumericSlider(0f, 0.5f)]
        [Tooltip("Analog trigger deadzone for yaw-only camera orbit. Prevents resting trigger noise from drifting the camera.")]
        [Min(0f)] public float gamepadTriggerDeadzone = 0.08f;

        [TabGroup(Tabs, "Smoothing"), BoxGroup(Tabs + "/Smoothing/Easing"), LabelWidth(190), InlineProperty]
        public EaseSettings followSmoothingEase = EaseSettings.Exponential;
        [TabGroup(Tabs, "Smoothing"), BoxGroup(Tabs + "/Smoothing/Easing"), LabelWidth(190), InlineProperty]
        public EaseSettings lookAheadPitchEase = EaseSettings.Linear;

        [SerializeField] private int obstructionDefaultsVersion;
        [SerializeField] private int triggerOrbitDefaultsVersion;

        [TabGroup(Tabs, "Diagnostics"), ShowInInspector, ReadOnly, LabelText("Effective Mouse Sensitivity"), PropertyOrder(100)]
        private string MouseSensitivitySummary => $"{mouseYawSensitivity * lookSensitivityMultiplier:0.###} yaw / {mousePitchSensitivity * lookSensitivityMultiplier:0.###} pitch";

        [TabGroup(Tabs, "Diagnostics"), ShowInInspector, ReadOnly, LabelText("Pitch Span"), PropertyOrder(101)]
        private string PitchSummary => $"{minPitch:0.#}deg to {maxPitch:0.#}deg";

        public bool EnsureObstructionDefaults()
        {
            bool changed = EnsureWorkflowDefaults();
            if (obstructionDefaultsVersion >= ObstructionDefaultsVersion)
            {
                return changed;
            }

            preventCameraClipping = true;
            cameraObstructionMask = 1;
            cameraObstructionRadius = 0.28f;
            cameraObstructionPadding = 0.08f;
            if (farClipPlane < 1000f)
            {
                farClipPlane = 2500f;
            }

            obstructionDefaultsVersion = ObstructionDefaultsVersion;
            return true;
        }

        public bool EnsureWorkflowDefaults()
        {
            bool changed = false;
            changed |= EnsureTriggerOrbitDefaults();
            changed |= EnsureEaseDefault(ref followSmoothingEase, EaseSettings.Exponential);
            changed |= EnsureEaseDefault(ref lookAheadPitchEase, EaseSettings.Linear);
            changed |= EnsureMinimum(ref farClipPlane, 2500f, 1000f);
            return changed;
        }

        private bool EnsureTriggerOrbitDefaults()
        {
            if (triggerOrbitDefaultsVersion >= TriggerOrbitDefaultsVersion)
            {
                return false;
            }

            if (gamepadTriggerYawSpeed <= 0f)
            {
                gamepadTriggerYawSpeed = gamepadYawSpeed > 0f ? gamepadYawSpeed : 130f;
            }

            if (gamepadTriggerDeadzone <= 0f)
            {
                gamepadTriggerDeadzone = 0.08f;
            }

            triggerOrbitDefaultsVersion = TriggerOrbitDefaultsVersion;
            return true;
        }

        private void OnValidate()
        {
            EnsureObstructionDefaults();
            EnsureWorkflowDefaults();
        }

        private bool IsMinPitchValid(float value)
        {
            return ConfigValidationRules.MinLessOrEqual(value, maxPitch, "Min pitch", "Max pitch").IsValid;
        }

        private bool IsMaxPitchValid(float value)
        {
            return ConfigValidationRules.MinLessOrEqual(minPitch, value, "Min pitch", "Max pitch").IsValid;
        }

        private bool IsOrbitPitchValid(float value)
        {
            return value >= minPitch && value <= maxPitch;
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
