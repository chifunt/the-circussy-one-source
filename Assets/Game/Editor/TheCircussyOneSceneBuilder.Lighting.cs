using System.IO;
using MoreMountains.Feedbacks;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.DI;
using TheCircussyOne.Runtime;
using TheCircussyOne.Visuals;

public static partial class TheCircussyOneSceneBuilder
{
    internal static void ApplyRenderSettings(LightingVisualConfig config)
    {
        RenderSettings.skybox = null;
        RenderSettings.ambientMode = config.ambientMode;
        RenderSettings.ambientSkyColor = config.ambientSkyColor;
        RenderSettings.ambientEquatorColor = config.ambientEquatorColor;
        RenderSettings.ambientGroundColor = config.ambientGroundColor;
        RenderSettings.ambientIntensity = config.ambientIntensity;
        RenderSettings.reflectionIntensity = config.reflectionIntensity;
        RenderSettings.fog = config.fogEnabled;
        RenderSettings.fogMode = config.fogMode;
        RenderSettings.fogColor = config.fogColor;
        RenderSettings.fogDensity = config.fogDensity;
    }

    internal static Light CreateLight(LightingVisualConfig config)
    {
        GameObject lightObject = new GameObject("Directional Light");
        var light = lightObject.AddComponent<Light>();
        ConfigureLight(light, config);
        RenderSettings.sun = light;
        return light;
    }

    private static void ConfigureLight(Light light, LightingVisualConfig config)
    {
        light.type = LightType.Directional;
        light.color = config.keyLightColor;
        light.intensity = config.keyLightIntensity;
        light.shadows = LightShadows.Soft;
        light.shadowStrength = config.keyLightShadowStrength;
        light.shadowBias = config.keyLightShadowBias;
        light.shadowNormalBias = config.keyLightShadowNormalBias;
        light.transform.rotation = Quaternion.Euler(config.keyLightEuler);
    }

    internal static void CreateLightingVolume(VolumeProfile profile)
    {
        GameObject volumeObject = new GameObject("Global Lighting Volume");
        var volume = volumeObject.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = 0f;
        volume.weight = 1f;
        volume.sharedProfile = profile;
    }

    internal static void UpdateSceneLight(LightingVisualConfig config)
    {
        Light light = null;
        foreach (Light existing in Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            light = existing;
            break;
        }

        if (light == null)
        {
            light = CreateLight(config);
        }
        else
        {
            light.name = "Directional Light";
            ConfigureLight(light, config);
            RenderSettings.sun = light;
            EditorUtility.SetDirty(light);
            EditorUtility.SetDirty(light.gameObject);
        }
    }

    internal static void UpdateSceneCameras(LightingVisualConfig config)
    {
        foreach (Camera camera in Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = config.cameraBackgroundColor;
            camera.allowHDR = true;

            var cameraData = camera.GetComponent<UniversalAdditionalCameraData>();
            if (cameraData == null)
            {
                cameraData = camera.gameObject.AddComponent<UniversalAdditionalCameraData>();
            }

            cameraData.renderPostProcessing = config.postProcessingEnabled;
            cameraData.renderShadows = true;
            EditorUtility.SetDirty(camera);
            EditorUtility.SetDirty(cameraData);
        }
    }

    internal static void UpdateSceneLightingVolume(VolumeProfile profile)
    {
        Volume volume = null;
        foreach (Volume existing in Object.FindObjectsByType<Volume>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (existing.name == "Global Lighting Volume" || volume == null)
            {
                volume = existing;
            }
        }

        if (volume == null)
        {
            CreateLightingVolume(profile);
            return;
        }

        volume.name = "Global Lighting Volume";
        volume.isGlobal = true;
        volume.priority = 0f;
        volume.weight = 1f;
        volume.sharedProfile = profile;
        EditorUtility.SetDirty(volume);
        EditorUtility.SetDirty(volume.gameObject);
    }

    internal static LightingVisualAssets ApplyLightingVisualAssets(LightingVisualConfig config)
    {
        Texture2D contactShadowTexture = GetOrCreateContactShadowTexture(config);
        Material contactShadowMaterial = GetContactShadowMaterial(config, contactShadowTexture);
        VolumeProfile volumeProfile = GetOrCreateLightingVolumeProfile(config);
        return new LightingVisualAssets(contactShadowMaterial, volumeProfile);
    }

    private static Texture2D GetOrCreateContactShadowTexture(LightingVisualConfig config)
    {
        const int resolution = 128;
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(ContactShadowTexturePath);
        if (texture != null && (texture.width != resolution || texture.height != resolution))
        {
            AssetDatabase.DeleteAsset(ContactShadowTexturePath);
            texture = null;
        }

        if (texture == null)
        {
            texture = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false, false);
            texture.name = "ContactShadowTexture";
            AssetDatabase.CreateAsset(texture, ContactShadowTexturePath);
        }

        float featherStart = Mathf.Lerp(0.12f, 0.68f, Mathf.Clamp01(config.contactShadowSoftness));
        var pixels = new Color[resolution * resolution];
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float u = (x + 0.5f) / resolution * 2f - 1f;
                float v = (y + 0.5f) / resolution * 2f - 1f;
                float radius = Mathf.Sqrt(u * u + v * v);
                float alpha = 1f - Mathf.SmoothStep(featherStart, 1f, radius);
                pixels[y * resolution + x] = new Color(1f, 1f, 1f, Mathf.Clamp01(alpha));
            }
        }

        texture.SetPixels(pixels);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        texture.Apply(false, false);
        EditorUtility.SetDirty(texture);
        return texture;
    }

    private static Material GetContactShadowMaterial(LightingVisualConfig config, Texture2D texture)
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(ContactShadowMaterialPath);
        Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(ContactShadowShaderPath) ?? Shader.Find("TheCircussyOne/Contact Shadow");
        if (shader == null)
        {
            throw new System.InvalidOperationException($"Missing contact shadow shader at {ContactShadowShaderPath}.");
        }

        if (material == null)
        {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, ContactShadowMaterialPath);
        }

        Color color = config.contactShadowColor;
        color.a = config.contactShadowOpacity;
        material.shader = shader;
        material.renderQueue = (int)RenderQueue.Transparent;
        SetTextureIfPresent(material, "_BaseMap", texture);
        SetColorIfPresent(material, "_BaseColor", color);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static VolumeProfile GetOrCreateLightingVolumeProfile(LightingVisualConfig config)
    {
        var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(LightingVolumeProfilePath);
        if (profile == null)
        {
            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, LightingVolumeProfilePath);
        }

        ConfigureLightingVolumeProfile(profile, config);
        EditorUtility.SetDirty(profile);
        return profile;
    }

    private static void ConfigureLightingVolumeProfile(VolumeProfile profile, LightingVisualConfig config)
    {
        if (!profile.TryGet(out ColorAdjustments colorAdjustments))
        {
            colorAdjustments = profile.Add<ColorAdjustments>(true);
        }

        colorAdjustments.active = config.postProcessingEnabled;
        colorAdjustments.postExposure.overrideState = true;
        colorAdjustments.postExposure.value = config.postExposure;
        colorAdjustments.contrast.overrideState = true;
        colorAdjustments.contrast.value = config.contrast;
        colorAdjustments.saturation.overrideState = true;
        colorAdjustments.saturation.value = config.saturation;
        colorAdjustments.colorFilter.overrideState = true;
        colorAdjustments.colorFilter.value = config.colorFilter;

        if (!profile.TryGet(out Bloom bloom))
        {
            bloom = profile.Add<Bloom>(true);
        }

        bloom.active = config.postProcessingEnabled && config.bloomIntensity > 0f;
        bloom.intensity.overrideState = true;
        bloom.intensity.value = config.bloomIntensity;
        bloom.threshold.overrideState = true;
        bloom.threshold.value = config.bloomThreshold;
        bloom.scatter.overrideState = true;
        bloom.scatter.value = config.bloomScatter;

        if (!profile.TryGet(out Vignette vignette))
        {
            vignette = profile.Add<Vignette>(true);
        }

        vignette.active = config.postProcessingEnabled && config.vignetteIntensity > 0f;
        vignette.color.overrideState = true;
        vignette.color.value = Color.black;
        vignette.intensity.overrideState = true;
        vignette.intensity.value = config.vignetteIntensity;
        vignette.smoothness.overrideState = true;
        vignette.smoothness.value = config.vignetteSmoothness;
    }
}
