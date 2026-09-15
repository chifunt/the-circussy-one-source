using Sirenix.OdinInspector;
using UnityEngine;

namespace TheCircussyOne.Config
{
    [System.Serializable]
    [InlineProperty]
    public sealed class GridMaterialLightingSettings
    {
        [VerticalGroup("PBR"), LabelWidth(160), NumericSlider(0f, 1f)]
        public float metallic;
        [VerticalGroup("PBR"), LabelWidth(160), NumericSlider(0f, 1f)]
        public float smoothness = 0.5f;
        [VerticalGroup("PBR"), LabelWidth(160), NumericSlider(0f, 1f)]
        public float specularStrength = 0.5f;
        [VerticalGroup("PBR"), LabelWidth(160), NumericSlider(0f, 1f)]
        public float occlusion = 1f;
        [VerticalGroup("Emission"), LabelWidth(160)]
        public Color emissionColor = Color.black;
        [VerticalGroup("Emission"), LabelWidth(160), NumericSlider(0f, 4f)]
        public float emissionStrength;

        public static GridMaterialLightingSettings FloorDefault()
        {
            return new GridMaterialLightingSettings
            {
                metallic = 0f,
                smoothness = 0.42f,
                specularStrength = 0.35f,
                occlusion = 1f,
                emissionColor = Color.black,
                emissionStrength = 0f
            };
        }

        public static GridMaterialLightingSettings ActorDefault()
        {
            return new GridMaterialLightingSettings
            {
                metallic = 0f,
                smoothness = 0.58f,
                specularStrength = 0.5f,
                occlusion = 1f,
                emissionColor = Color.black,
                emissionStrength = 0f
            };
        }

        public static GridMaterialLightingSettings ProjectileDefault()
        {
            return new GridMaterialLightingSettings
            {
                metallic = 0f,
                smoothness = 0.75f,
                specularStrength = 0.7f,
                occlusion = 1f,
                emissionColor = new Color(1f, 0.87f, 0.22f, 1f),
                emissionStrength = 0.35f
            };
        }

        public static GridMaterialLightingSettings PickupDefault()
        {
            return new GridMaterialLightingSettings
            {
                metallic = 0f,
                smoothness = 0.72f,
                specularStrength = 0.65f,
                occlusion = 1f,
                emissionColor = new Color(0.36f, 0.84f, 1f, 1f),
                emissionStrength = 0.35f
            };
        }
    }

    [CreateAssetMenu(menuName = "The Circussy One/Grid Visual Config", fileName = "GridVisualConfig")]
    [InfoBox("PROJECTED VISUAL: Apply regenerates generated textures/materials. Rebuild is only for generated scene structure resets.")]
    public sealed class GridVisualConfig : SerializedScriptableObject
    {
        private const string Tabs = "Grid Visuals";
        private const int LightingDefaultsVersion = 1;

        [TabGroup(Tabs, "Floor"), BoxGroup(Tabs + "/Floor/Workflow"), LabelWidth(190), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.VisualColor")]
        public bool autoApplyOnChange = true;

        [TabGroup(Tabs, "Floor"), BoxGroup(Tabs + "/Floor/World Grid"), LabelWidth(190), SuffixLabel("u")]
        [ValidateInput(nameof(IsMinorSpacingValid), "Minor spacing must be greater than zero.")]
        [Min(0.01f)] public float minorSpacing = 1f;
        [TabGroup(Tabs, "Floor"), BoxGroup(Tabs + "/Floor/World Grid"), LabelWidth(190), SuffixLabel("u")]
        [ValidateInput(nameof(IsMajorSpacingValid), "Major spacing must be greater than or equal to minor spacing.")]
        [Min(0.01f)] public float majorSpacing = 5f;
        [TabGroup(Tabs, "Floor"), BoxGroup(Tabs + "/Floor/World Grid"), LabelWidth(190), SuffixLabel("u")]
        [Min(0.001f)] public float minorLineWidth = 0.008f;
        [TabGroup(Tabs, "Floor"), BoxGroup(Tabs + "/Floor/World Grid"), LabelWidth(190), SuffixLabel("u")]
        [Min(0.001f)] public float majorLineWidth = 0.028f;

        [TabGroup(Tabs, "Texture Generation"), BoxGroup(Tabs + "/Texture Generation/Generated Texture"), LabelWidth(190), NumericSlider(4, 20)]
        public int cellsPerTexture = 10;
        [TabGroup(Tabs, "Texture Generation"), BoxGroup(Tabs + "/Texture Generation/Generated Texture"), LabelWidth(190), NumericSlider(64, 512), SuffixLabel("px")]
        public int textureResolution = 256;
        [TabGroup(Tabs, "Actor"), BoxGroup(Tabs + "/Actor/Local Grid"), LabelWidth(190), SuffixLabel("u")]
        [Min(0.001f)] public float actorMinorSpacing = 0.18f;
        [TabGroup(Tabs, "Projectile"), BoxGroup(Tabs + "/Projectile/Local Grid"), LabelWidth(190), SuffixLabel("u")]
        [Min(0.001f)] public float projectileMinorSpacing = 0.06f;
        [TabGroup(Tabs, "Pickup"), BoxGroup(Tabs + "/Pickup/Local Grid"), LabelWidth(190), SuffixLabel("u")]
        [Min(0.001f)] public float pickupMinorSpacing = 0.08f;

        [TabGroup(Tabs, "Floor"), BoxGroup(Tabs + "/Floor/Colors"), LabelWidth(190), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.VisualColor")]
        public Color floorBaseColor = new(0.2f, 0.31f, 0.34f, 1f);
        [TabGroup(Tabs, "Floor"), BoxGroup(Tabs + "/Floor/Colors"), LabelWidth(190)]
        public Color floorAlternateColor = new(0.27f, 0.39f, 0.42f, 1f);
        [TabGroup(Tabs, "Floor"), BoxGroup(Tabs + "/Floor/Colors"), LabelWidth(190)]
        public Color minorLineColor = new(0.62f, 0.78f, 0.82f, 0.1f);
        [TabGroup(Tabs, "Floor"), BoxGroup(Tabs + "/Floor/Colors"), LabelWidth(190)]
        public Color majorLineColor = new(0.86f, 0.93f, 0.94f, 0.36f);

        [TabGroup(Tabs, "Floor"), BoxGroup(Tabs + "/Floor/Strength"), LabelWidth(190), NumericSlider(0f, 1f)]
        public float floorGridStrength = 0.18f;
        [TabGroup(Tabs, "Actor"), BoxGroup(Tabs + "/Actor/Strength"), LabelWidth(190), NumericSlider(0f, 1f)]
        public float actorGridStrength = 0.04f;
        [TabGroup(Tabs, "Projectile"), BoxGroup(Tabs + "/Projectile/Strength"), LabelWidth(190), NumericSlider(0f, 1f)]
        public float projectileGridStrength = 0.18f;
        [TabGroup(Tabs, "Pickup"), BoxGroup(Tabs + "/Pickup/Strength"), LabelWidth(190), NumericSlider(0f, 1f)]
        public float pickupGridStrength = 0.2f;
        [TabGroup(Tabs, "Floor"), BoxGroup(Tabs + "/Floor/Strength"), LabelWidth(190), NumericSlider(0f, 1f)]
        public float floorTileStrength = 0.58f;
        [TabGroup(Tabs, "Actor"), BoxGroup(Tabs + "/Actor/Strength"), LabelWidth(190), NumericSlider(0f, 1f)]
        public float actorTileStrength = 0.72f;
        [TabGroup(Tabs, "Projectile"), BoxGroup(Tabs + "/Projectile/Strength"), LabelWidth(190), NumericSlider(0f, 1f)]
        public float projectileTileStrength = 0.08f;
        [TabGroup(Tabs, "Pickup"), BoxGroup(Tabs + "/Pickup/Strength"), LabelWidth(190), NumericSlider(0f, 1f)]
        public float pickupTileStrength = 0.1f;
        [TabGroup(Tabs, "Texture Generation"), BoxGroup(Tabs + "/Texture Generation/Generated Texture"), LabelWidth(190), NumericSlider(0f, 0.25f)]
        public float noiseStrength = 0.025f;

        [TabGroup(Tabs, "PBR Lighting"), BoxGroup(Tabs + "/PBR Lighting/Floor"), LabelWidth(190), InlineProperty]
        public GridMaterialLightingSettings floorLighting = GridMaterialLightingSettings.FloorDefault();
        [TabGroup(Tabs, "PBR Lighting"), BoxGroup(Tabs + "/PBR Lighting/Actor"), LabelWidth(190), InlineProperty]
        public GridMaterialLightingSettings actorLighting = GridMaterialLightingSettings.ActorDefault();
        [TabGroup(Tabs, "PBR Lighting"), BoxGroup(Tabs + "/PBR Lighting/Projectile"), LabelWidth(190), InlineProperty]
        public GridMaterialLightingSettings projectileLighting = GridMaterialLightingSettings.ProjectileDefault();
        [TabGroup(Tabs, "PBR Lighting"), BoxGroup(Tabs + "/PBR Lighting/Pickup"), LabelWidth(190), InlineProperty]
        public GridMaterialLightingSettings pickupLighting = GridMaterialLightingSettings.PickupDefault();

        [SerializeField] private int lightingDefaultsVersion;

        [TabGroup(Tabs, "Preview"), ShowInInspector, ReadOnly, LabelText("Floor Grid"), PropertyOrder(100)]
        private string FloorGridSummary => $"{minorSpacing:0.###}u minor / {majorSpacing:0.###}u major";

        [TabGroup(Tabs, "Preview"), ShowInInspector, ReadOnly, LabelText("Generated Texture"), PropertyOrder(101)]
        private string TextureSummary => $"{textureResolution}px, {cellsPerTexture} cells";

        public bool EnsureLightingDefaults()
        {
            if (lightingDefaultsVersion >= LightingDefaultsVersion &&
                floorLighting != null &&
                actorLighting != null &&
                projectileLighting != null &&
                pickupLighting != null)
            {
                return false;
            }

            floorLighting ??= GridMaterialLightingSettings.FloorDefault();
            actorLighting ??= GridMaterialLightingSettings.ActorDefault();
            projectileLighting ??= GridMaterialLightingSettings.ProjectileDefault();
            pickupLighting ??= GridMaterialLightingSettings.PickupDefault();

            if (lightingDefaultsVersion < LightingDefaultsVersion)
            {
                floorLighting = GridMaterialLightingSettings.FloorDefault();
                actorLighting = GridMaterialLightingSettings.ActorDefault();
                projectileLighting = GridMaterialLightingSettings.ProjectileDefault();
                pickupLighting = GridMaterialLightingSettings.PickupDefault();
                lightingDefaultsVersion = LightingDefaultsVersion;
                return true;
            }

            return true;
        }

        private void OnValidate()
        {
            EnsureLightingDefaults();
        }

        private bool IsMinorSpacingValid(float value)
        {
            return value > 0f;
        }

        private bool IsMajorSpacingValid(float value)
        {
            return ConfigValidationRules.MajorSpacing(minorSpacing, value).IsValid;
        }
    }
}
