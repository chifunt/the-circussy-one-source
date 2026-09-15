using UnityEngine;
using UnityEngine.Rendering;

namespace TheCircussyOne.Visuals
{
    public static class VfxParticleMaterialFactory
    {
        private static readonly string[] SupportedShaderNames =
        {
            "Universal Render Pipeline/Particles/Unlit",
            "Particles/Standard Unlit",
            "Sprites/Default",
            "Universal Render Pipeline/Unlit",
            "Unlit/Color"
        };

        public static Material CreateTransparent(string materialName, Texture texture, bool additive = false)
        {
            Shader shader = FindSupportedShader();
            if (shader == null)
            {
                return null;
            }

            var material = new Material(shader)
            {
                name = string.IsNullOrWhiteSpace(materialName) ? "Runtime Particle Material" : materialName,
                hideFlags = HideFlags.DontSave,
                renderQueue = (int)RenderQueue.Transparent
            };

            ConfigureTransparent(material, additive);
            SetTextureIfPresent(material, "_BaseMap", texture);
            SetTextureIfPresent(material, "_MainTex", texture);
            return material;
        }

        public static bool IsUsable(Material material)
        {
            return material != null
                && material.shader != null
                && material.shader.name != "Hidden/InternalErrorShader"
                && material.shader.isSupported;
        }

        private static Shader FindSupportedShader()
        {
            for (int i = 0; i < SupportedShaderNames.Length; i++)
            {
                Shader shader = Shader.Find(SupportedShaderNames[i]);
                if (shader != null && shader.name != "Hidden/InternalErrorShader" && shader.isSupported)
                {
                    return shader;
                }
            }

            return null;
        }

        private static void ConfigureTransparent(Material material, bool additive)
        {
            SetFloatIfPresent(material, "_Surface", 1f);
            SetFloatIfPresent(material, "_Blend", additive ? 2f : 0f);
            SetFloatIfPresent(material, "_SrcBlend", (float)BlendMode.SrcAlpha);
            SetFloatIfPresent(material, "_DstBlend", additive ? (float)BlendMode.One : (float)BlendMode.OneMinusSrcAlpha);
            SetFloatIfPresent(material, "_ZWrite", 0f);
            SetFloatIfPresent(material, "_Cull", (float)CullMode.Off);
            SetColorIfPresent(material, "_BaseColor", Color.white);
            SetColorIfPresent(material, "_Color", Color.white);
            material.SetOverrideTag("RenderType", "Transparent");
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.DisableKeyword("_ALPHATEST_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        }

        private static void SetFloatIfPresent(Material material, string propertyName, float value)
        {
            if (material.HasProperty(propertyName))
            {
                material.SetFloat(propertyName, value);
            }
        }

        private static void SetColorIfPresent(Material material, string propertyName, Color value)
        {
            if (material.HasProperty(propertyName))
            {
                material.SetColor(propertyName, value);
            }
        }

        private static void SetTextureIfPresent(Material material, string propertyName, Texture texture)
        {
            if (texture != null && material.HasProperty(propertyName))
            {
                material.SetTexture(propertyName, texture);
            }
        }
    }
}
