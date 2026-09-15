using UnityEngine;

namespace TheCircussyOne.Visuals
{
    public static class ShaderVisualProperties
    {
        public const string AllInOneShaderName = "AllIn13DShader/AllIn13DShader";

        private static readonly int BaseColorProperty = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorProperty = Shader.PropertyToID("_Color");
        private static readonly int EmissionColorProperty = Shader.PropertyToID("_EmissionColor");
        private static readonly int EmissionStrengthProperty = Shader.PropertyToID("_EmissionStrength");
        private static readonly int AllInOneEmissionSelfGlowProperty = Shader.PropertyToID("_EmissionSelfGlow");
        private static readonly int AllInOneOutlineColorProperty = Shader.PropertyToID("_OutlineColor");
        private static readonly int AllInOneOutlineThicknessProperty = Shader.PropertyToID("_OutlineThickness");
        private static readonly int AllInOneHitColorProperty = Shader.PropertyToID("_HitColor");
        private static readonly int AllInOneHitGlowProperty = Shader.PropertyToID("_HitGlow");
        private static readonly int AllInOneHitBlendProperty = Shader.PropertyToID("_HitBlend");

        public static Shader FindAllInOneShader()
        {
            return Shader.Find(AllInOneShaderName);
        }

        public static void ApplyBaseColor(MaterialPropertyBlock propertyBlock, Color color)
        {
            if (propertyBlock == null)
            {
                return;
            }

            propertyBlock.SetColor(BaseColorProperty, color);
            propertyBlock.SetColor(ColorProperty, color);
        }

        public static void ApplyEmission(MaterialPropertyBlock propertyBlock, Color color, float strength)
        {
            if (propertyBlock == null)
            {
                return;
            }

            float clampedStrength = Mathf.Max(0f, strength);
            propertyBlock.SetColor(EmissionColorProperty, color);
            propertyBlock.SetFloat(EmissionStrengthProperty, clampedStrength);
            propertyBlock.SetFloat(AllInOneEmissionSelfGlowProperty, clampedStrength);
        }

        public static void ApplyOutline(MaterialPropertyBlock propertyBlock, Color color, float thickness)
        {
            if (propertyBlock == null)
            {
                return;
            }

            propertyBlock.SetColor(AllInOneOutlineColorProperty, color);
            propertyBlock.SetFloat(AllInOneOutlineThicknessProperty, Mathf.Max(0f, thickness));
        }

        public static void ApplyHit(MaterialPropertyBlock propertyBlock, Color color, float glow, float blend)
        {
            if (propertyBlock == null)
            {
                return;
            }

            propertyBlock.SetColor(AllInOneHitColorProperty, color);
            propertyBlock.SetFloat(AllInOneHitGlowProperty, Mathf.Max(0f, glow));
            propertyBlock.SetFloat(AllInOneHitBlendProperty, Mathf.Clamp01(blend));
        }

        public static void ConfigureMaterial(Material material, Color color, Color emissionColor, float emissionStrength, bool outlineEnabled)
        {
            if (material == null)
            {
                return;
            }

            material.color = color;
            SetColorIfPresent(material, "_BaseColor", color);
            SetColorIfPresent(material, "_Color", color);
            SetColorIfPresent(material, "_EmissionColor", emissionColor);
            SetFloatIfPresent(material, "_EmissionStrength", Mathf.Max(0f, emissionStrength));
            SetFloatIfPresent(material, "_EmissionSelfGlow", Mathf.Max(0f, emissionStrength));
            SetFloatIfPresent(material, "_EmissionEnabled", emissionStrength > 0f ? 1f : 0f);

            if (emissionStrength > 0f)
            {
                material.EnableKeyword("_EMISSION_ON");
            }
            else
            {
                material.DisableKeyword("_EMISSION_ON");
            }

            ConfigureOutlineMaterial(material, outlineEnabled);
        }

        public static void ConfigureOutlineMaterial(Material material, bool enabled)
        {
            if (material == null)
            {
                return;
            }

            SetFloatIfPresent(material, "_OutlineType", enabled ? 1f : 0f);
            SetFloatIfPresent(material, "_OutlineThickness", 0f);
            SetColorIfPresent(material, "_OutlineColor", Color.white);

            if (enabled)
            {
                material.DisableKeyword("_OUTLINETYPE_NONE");
                material.EnableKeyword("_OUTLINETYPE_SIMPLE");
                material.DisableKeyword("_OUTLINETYPE_CONSTANT");
                material.DisableKeyword("_OUTLINETYPE_FADEWITHDISTANCE");
            }
            else
            {
                material.EnableKeyword("_OUTLINETYPE_NONE");
                material.DisableKeyword("_OUTLINETYPE_SIMPLE");
                material.DisableKeyword("_OUTLINETYPE_CONSTANT");
                material.DisableKeyword("_OUTLINETYPE_FADEWITHDISTANCE");
            }
        }

        private static void SetColorIfPresent(Material material, string property, Color value)
        {
            if (material.HasProperty(property))
            {
                material.SetColor(property, value);
            }
        }

        private static void SetFloatIfPresent(Material material, string property, float value)
        {
            if (material.HasProperty(property))
            {
                material.SetFloat(property, value);
            }
        }
    }
}
