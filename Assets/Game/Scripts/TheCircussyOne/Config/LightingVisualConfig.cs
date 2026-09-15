using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;

namespace TheCircussyOne.Config
{
    [CreateAssetMenu(menuName = "The Circussy One/Lighting Visual Config", fileName = "LightingVisualConfig")]
    [InfoBox("PROJECTED VISUAL: Apply updates the open scene and generated lighting assets. Rebuild already applies lighting and should not require a second Apply step.")]
    public sealed class LightingVisualConfig : SerializedScriptableObject
    {
        private const string Tabs = "Lighting Visuals";

        [TabGroup(Tabs, "Camera"), BoxGroup(Tabs + "/Camera/Workflow"), LabelWidth(190), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.LightingColor")]
        public bool autoApplyOnChange = true;

        [TabGroup(Tabs, "Camera"), BoxGroup(Tabs + "/Camera/Background"), LabelWidth(190)]
        public Color cameraBackgroundColor = new(0.085f, 0.065f, 0.055f, 1f);

        [TabGroup(Tabs, "Key Light"), BoxGroup(Tabs + "/Key Light/Directional Light"), LabelWidth(190), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.LightingColor")]
        public Color keyLightColor = new(1f, 0.88f, 0.68f, 1f);
        [TabGroup(Tabs, "Key Light"), BoxGroup(Tabs + "/Key Light/Directional Light"), LabelWidth(190), NumericSlider(0f, 8f)]
        [Min(0f)] public float keyLightIntensity = 2.25f;
        [TabGroup(Tabs, "Key Light"), BoxGroup(Tabs + "/Key Light/Directional Light"), LabelWidth(190), SuffixLabel("Euler")]
        public Vector3 keyLightEuler = new(60f, -32f, 0f);
        [TabGroup(Tabs, "Key Light"), BoxGroup(Tabs + "/Key Light/Shadows"), LabelWidth(190), NumericSlider(0f, 1f)]
        public float keyLightShadowStrength = 0.38f;
        [TabGroup(Tabs, "Key Light"), BoxGroup(Tabs + "/Key Light/Shadows"), LabelWidth(190), NumericSlider(0f, 2f)]
        public float keyLightShadowBias = 0.035f;
        [TabGroup(Tabs, "Key Light"), BoxGroup(Tabs + "/Key Light/Shadows"), LabelWidth(190), NumericSlider(0f, 3f)]
        public float keyLightShadowNormalBias = 0.32f;

        [TabGroup(Tabs, "Ambient"), BoxGroup(Tabs + "/Ambient/Mode"), LabelWidth(190), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.LightingColor")]
        [EnumToggleButtons]
        public AmbientMode ambientMode = AmbientMode.Trilight;
        [TabGroup(Tabs, "Ambient"), BoxGroup(Tabs + "/Ambient/Gradient"), LabelWidth(190)]
        public Color ambientSkyColor = new(0.36f, 0.44f, 0.50f, 1f);
        [TabGroup(Tabs, "Ambient"), BoxGroup(Tabs + "/Ambient/Gradient"), LabelWidth(190)]
        public Color ambientEquatorColor = new(0.45f, 0.40f, 0.35f, 1f);
        [TabGroup(Tabs, "Ambient"), BoxGroup(Tabs + "/Ambient/Gradient"), LabelWidth(190)]
        public Color ambientGroundColor = new(0.32f, 0.29f, 0.26f, 1f);
        [TabGroup(Tabs, "Ambient"), BoxGroup(Tabs + "/Ambient/Intensity"), LabelWidth(190), NumericSlider(0f, 2f)]
        public float ambientIntensity = 2f;
        [TabGroup(Tabs, "Ambient"), BoxGroup(Tabs + "/Ambient/Intensity"), LabelWidth(190), NumericSlider(0f, 1f)]
        public float reflectionIntensity = 0.22f;

        [TabGroup(Tabs, "Fog"), BoxGroup(Tabs + "/Fog/Scene Fog"), LabelWidth(190), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.CameraColor")]
        public bool fogEnabled = true;
        [TabGroup(Tabs, "Fog"), BoxGroup(Tabs + "/Fog/Scene Fog"), LabelWidth(190), EnableIf(nameof(fogEnabled))]
        [EnumToggleButtons]
        public FogMode fogMode = FogMode.ExponentialSquared;
        [TabGroup(Tabs, "Fog"), BoxGroup(Tabs + "/Fog/Scene Fog"), LabelWidth(190), EnableIf(nameof(fogEnabled))]
        public Color fogColor = new(0.16f, 0.125f, 0.105f, 1f);
        [TabGroup(Tabs, "Fog"), BoxGroup(Tabs + "/Fog/Scene Fog"), LabelWidth(190), EnableIf(nameof(fogEnabled)), NumericSlider(0f, 0.05f)]
        public float fogDensity = 0.0028f;

        [TabGroup(Tabs, "Post"), BoxGroup(Tabs + "/Post/Volume"), LabelWidth(190), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.VisualColor")]
        public bool postProcessingEnabled = true;
        [TabGroup(Tabs, "Post"), BoxGroup(Tabs + "/Post/Color Adjustments"), LabelWidth(190), EnableIf(nameof(postProcessingEnabled)), NumericSlider(-5f, 5f)]
        public float postExposure = 0.12f;
        [TabGroup(Tabs, "Post"), BoxGroup(Tabs + "/Post/Color Adjustments"), LabelWidth(190), EnableIf(nameof(postProcessingEnabled)), NumericSlider(-100f, 100f)]
        public float contrast = 9f;
        [TabGroup(Tabs, "Post"), BoxGroup(Tabs + "/Post/Color Adjustments"), LabelWidth(190), EnableIf(nameof(postProcessingEnabled)), NumericSlider(-100f, 100f)]
        public float saturation = 4f;
        [TabGroup(Tabs, "Post"), BoxGroup(Tabs + "/Post/Color Adjustments"), LabelWidth(190), EnableIf(nameof(postProcessingEnabled))]
        public Color colorFilter = new(1.02f, 0.99f, 0.92f, 1f);
        [TabGroup(Tabs, "Post"), BoxGroup(Tabs + "/Post/Bloom"), LabelWidth(190), EnableIf(nameof(postProcessingEnabled)), NumericSlider(0f, 5f)]
        [InfoBox("High bloom can wash out damage numbers and HUD accents.", InfoMessageType.Warning, nameof(HasHighBloom))]
        public float bloomIntensity = 0.46f;
        [TabGroup(Tabs, "Post"), BoxGroup(Tabs + "/Post/Bloom"), LabelWidth(190), EnableIf(nameof(postProcessingEnabled)), NumericSlider(0f, 10f)]
        public float bloomThreshold = 0.95f;
        [TabGroup(Tabs, "Post"), BoxGroup(Tabs + "/Post/Bloom"), LabelWidth(190), EnableIf(nameof(postProcessingEnabled)), NumericSlider(0f, 1f)]
        public float bloomScatter = 0.56f;
        [TabGroup(Tabs, "Post"), BoxGroup(Tabs + "/Post/Vignette"), LabelWidth(190), EnableIf(nameof(postProcessingEnabled)), NumericSlider(0f, 1f)]
        public float vignetteIntensity = 0.14f;
        [TabGroup(Tabs, "Post"), BoxGroup(Tabs + "/Post/Vignette"), LabelWidth(190), EnableIf(nameof(postProcessingEnabled)), NumericSlider(0.01f, 1f)]
        public float vignetteSmoothness = 0.55f;

        [TabGroup(Tabs, "Contact Shadows"), BoxGroup(Tabs + "/Contact Shadows/Stylized Grounding"), LabelWidth(190), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.WarningColor")]
        public bool contactShadowsEnabled = true;
        [TabGroup(Tabs, "Contact Shadows"), BoxGroup(Tabs + "/Contact Shadows/Stylized Grounding"), LabelWidth(190), EnableIf(nameof(contactShadowsEnabled))]
        public Color contactShadowColor = new(0.08f, 0.07f, 0.06f, 1f);
        [TabGroup(Tabs, "Contact Shadows"), BoxGroup(Tabs + "/Contact Shadows/Stylized Grounding"), LabelWidth(190), EnableIf(nameof(contactShadowsEnabled)), NumericSlider(0f, 1f)]
        public float contactShadowOpacity = 0.18f;
        [TabGroup(Tabs, "Contact Shadows"), BoxGroup(Tabs + "/Contact Shadows/Stylized Grounding"), LabelWidth(190), EnableIf(nameof(contactShadowsEnabled)), NumericSlider(0f, 1f)]
        public float contactShadowSoftness = 0.82f;
        [TabGroup(Tabs, "Contact Shadows"), BoxGroup(Tabs + "/Contact Shadows/Stylized Grounding"), LabelWidth(190), EnableIf(nameof(contactShadowsEnabled)), SuffixLabel("u")]
        [Min(0f)] public float contactShadowYOffset = 0.018f;
        [TabGroup(Tabs, "Contact Shadows"), BoxGroup(Tabs + "/Contact Shadows/Scale"), LabelWidth(190), EnableIf(nameof(contactShadowsEnabled))]
        public Vector2 contactShadowBaseScale = new(1.65f, 1.08f);
        [TabGroup(Tabs, "Contact Shadows"), BoxGroup(Tabs + "/Contact Shadows/Scale"), LabelWidth(190), EnableIf(nameof(contactShadowsEnabled))]
        [Min(0f)] public float playerContactShadowScale = 1.08f;
        [TabGroup(Tabs, "Contact Shadows"), BoxGroup(Tabs + "/Contact Shadows/Scale"), LabelWidth(190), EnableIf(nameof(contactShadowsEnabled))]
        [Min(0f)] public float enemyContactShadowScale = 0.96f;

        [TabGroup(Tabs, "Diagnostics"), ShowInInspector, ReadOnly, LabelText("Post Stack"), PropertyOrder(100)]
        private string PostSummary => postProcessingEnabled
            ? $"Contrast {contrast:0.#}, saturation {saturation:0.#}, bloom {bloomIntensity:0.##}"
            : "Disabled";

        [TabGroup(Tabs, "Diagnostics"), ShowInInspector, ReadOnly, LabelText("Contact Shadows"), PropertyOrder(101)]
        private string ContactShadowSummary => contactShadowsEnabled
            ? $"Opacity {contactShadowOpacity:0.##}, softness {contactShadowSoftness:0.##}"
            : "Disabled";

        private bool HasHighBloom => ConfigValidationRules.WarningIfGreater(bloomIntensity, 1f, "Bloom intensity").Level == ConfigValidationLevel.Warning;
    }
}
