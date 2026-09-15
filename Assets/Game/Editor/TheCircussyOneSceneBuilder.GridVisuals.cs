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
    internal static GridVisualAssets ApplyGridVisualAssets(GameConfig config, GridVisualConfig gridConfig)
    {
        Texture2D floorGridTexture = GetOrCreateGridTexture(
            "Assets/Game/Visuals/GridTextures/FloorGrid.asset",
            gridConfig,
            FloorCellsPerTexture(gridConfig) * 2,
            majorCellSpan: FloorCellsPerTexture(gridConfig),
            minorLinePixels: 0.35f,
            majorLinePixels: 1.8f,
            minorLineAlpha: 0.015f,
            majorLineAlpha: 0.12f,
            lineToneBoost: 0.03f,
            lowTone: 0.38f,
            highTone: 0.64f,
            majorToneStrength: 0.045f,
            generateMipmaps: false);
        Texture2D actorGridTexture = GetOrCreateGridTexture(
            "Assets/Game/Visuals/GridTextures/ActorGrid.asset",
            gridConfig,
            gridConfig.cellsPerTexture,
            majorCellSpan: Mathf.Max(1, gridConfig.cellsPerTexture / 2),
            minorLinePixels: 0.65f,
            majorLinePixels: 1.6f,
            minorLineAlpha: 0.04f,
            majorLineAlpha: 0.08f,
            lineToneBoost: 0.02f,
            lowTone: 0.3f,
            highTone: 0.72f,
            majorToneStrength: 0.025f,
            generateMipmaps: false);
        Texture2D projectileGridTexture = GetOrCreateGridTexture(
            "Assets/Game/Visuals/GridTextures/ProjectileGrid.asset",
            gridConfig,
            gridConfig.cellsPerTexture,
            majorCellSpan: Mathf.Max(1, gridConfig.cellsPerTexture / 2),
            minorLinePixels: 0.9f,
            majorLinePixels: 1.8f,
            minorLineAlpha: 0.44f,
            majorLineAlpha: 0.72f,
            lineToneBoost: 0.26f,
            lowTone: 0.5f,
            highTone: 0.58f,
            majorToneStrength: 0.04f,
            generateMipmaps: false);
        Texture2D pickupGridTexture = GetOrCreateGridTexture(
            "Assets/Game/Visuals/GridTextures/PickupGrid.asset",
            gridConfig,
            gridConfig.cellsPerTexture,
            majorCellSpan: Mathf.Max(1, gridConfig.cellsPerTexture / 2),
            minorLinePixels: 0.9f,
            majorLinePixels: 1.9f,
            minorLineAlpha: 0.48f,
            majorLineAlpha: 0.76f,
            lineToneBoost: 0.28f,
            lowTone: 0.49f,
            highTone: 0.59f,
            majorToneStrength: 0.04f,
            generateMipmaps: false);

        gridConfig.EnsureLightingDefaults();

        Material playerMaterial = GetGridMaterial("Assets/Game/Materials/Player.mat", config.playerColor, ObjectGridColor(config.playerColor), actorGridTexture, LocalGridTextureWorldSize(gridConfig.actorMinorSpacing, gridConfig), ActorUvTiling(gridConfig.actorMinorSpacing, gridConfig), gridConfig.actorGridStrength, gridConfig.actorTileStrength, useWorldSpace: false, useUvSpace: true, gridConfig.actorLighting);
        Material enemyMaterial = GetGridMaterial("Assets/Game/Materials/Enemy.mat", config.enemyColor, ObjectGridColor(config.enemyColor), actorGridTexture, LocalGridTextureWorldSize(gridConfig.actorMinorSpacing, gridConfig), ActorUvTiling(gridConfig.actorMinorSpacing, gridConfig), gridConfig.actorGridStrength, gridConfig.actorTileStrength, useWorldSpace: false, useUvSpace: true, gridConfig.actorLighting);
        Material projectileMaterial = GetGridMaterial("Assets/Game/Materials/Projectile.mat", config.projectileColor, ObjectGridColor(config.projectileColor), projectileGridTexture, LocalGridTextureWorldSize(gridConfig.projectileMinorSpacing, gridConfig), SmallObjectUvTiling(gridConfig.projectileMinorSpacing, gridConfig, 0.22f), gridConfig.projectileGridStrength, gridConfig.projectileTileStrength, useWorldSpace: false, useUvSpace: true, gridConfig.projectileLighting);
        Material pickupMaterial = GetGridMaterial("Assets/Game/Materials/Pickup.mat", config.pickupColor, ObjectGridColor(config.pickupColor), pickupGridTexture, LocalGridTextureWorldSize(gridConfig.pickupMinorSpacing, gridConfig), SmallObjectUvTiling(gridConfig.pickupMinorSpacing, gridConfig, 0.56f), gridConfig.pickupGridStrength, gridConfig.pickupTileStrength, useWorldSpace: false, useUvSpace: true, gridConfig.pickupLighting, preferAllInOneShader: true);
        Material floorMaterial = GetGridMaterial(TheCircussyOneAssetPaths.FloorMaterialPath, gridConfig.floorBaseColor, gridConfig.majorLineColor, floorGridTexture, Mathf.Max(0.01f, gridConfig.majorSpacing * 2f), 1f, gridConfig.floorGridStrength, gridConfig.floorTileStrength, useWorldSpace: true, useUvSpace: false, gridConfig.floorLighting);

        return new GridVisualAssets(playerMaterial, enemyMaterial, projectileMaterial, pickupMaterial, floorMaterial);
    }

    private static Material GetGridMaterial(string path, Color baseColor, Color gridColor, Texture2D gridTexture, float textureWorldSize, float uvTiling, float gridStrength, float textureStrength, bool useWorldSpace, bool useUvSpace, GridMaterialLightingSettings lighting, bool preferAllInOneShader = false)
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        Shader shader = preferAllInOneShader
            ? ShaderVisualProperties.FindAllInOneShader()
            : null;
        shader ??= AssetDatabase.LoadAssetAtPath<Shader>(WorldGridShaderPath) ?? Shader.Find("TheCircussyOne/World Grid");
        if (shader == null)
        {
            throw new System.InvalidOperationException($"Missing world grid shader at {WorldGridShaderPath}.");
        }

        if (material == null)
        {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }

        material.shader = shader;
        material.color = baseColor;
        SetColorIfPresent(material, "_Color", baseColor);
        SetColorIfPresent(material, "_GridColor", gridColor);
        SetTextureIfPresent(material, "_GridTexture", gridTexture);
        SetFloatIfPresent(material, "_GridTextureWorldSize", textureWorldSize);
        SetFloatIfPresent(material, "_UseWorldSpace", useWorldSpace ? 1f : 0f);
        SetFloatIfPresent(material, "_UseUvSpace", useUvSpace ? 1f : 0f);
        SetFloatIfPresent(material, "_UvTiling", uvTiling);
        SetFloatIfPresent(material, "_GridStrength", gridStrength);
        SetFloatIfPresent(material, "_TextureStrength", textureStrength);
        ApplyGridLighting(material, lighting);
        if (preferAllInOneShader && shader.name == ShaderVisualProperties.AllInOneShaderName)
        {
            SetTextureIfPresent(material, "_MainTex", gridTexture);
            ShaderVisualProperties.ConfigureMaterial(
                material,
                baseColor,
                lighting != null ? lighting.emissionColor : Color.black,
                lighting != null ? lighting.emissionStrength : 0f,
                outlineEnabled: false);
        }

        EditorUtility.SetDirty(material);
        return material;
    }

    private static void ApplyGridLighting(Material material, GridMaterialLightingSettings lighting)
    {
        if (lighting == null)
        {
            return;
        }

        SetFloatIfPresent(material, "_Metallic", Mathf.Clamp01(lighting.metallic));
        SetFloatIfPresent(material, "_Smoothness", Mathf.Clamp01(lighting.smoothness));
        SetFloatIfPresent(material, "_SpecularStrength", Mathf.Clamp01(lighting.specularStrength));
        SetFloatIfPresent(material, "_Occlusion", Mathf.Clamp01(lighting.occlusion));
        SetColorIfPresent(material, "_EmissionColor", lighting.emissionColor);
        SetFloatIfPresent(material, "_EmissionStrength", Mathf.Max(0f, lighting.emissionStrength));
    }

    private static Texture2D GetOrCreateGridTexture(
        string path,
        GridVisualConfig config,
        int cellsPerTexture,
        int majorCellSpan,
        float minorLinePixels,
        float majorLinePixels,
        float minorLineAlpha,
        float majorLineAlpha,
        float lineToneBoost,
        float lowTone,
        float highTone,
        float majorToneStrength,
        bool generateMipmaps)
    {
        int resolution = Mathf.Clamp(config.textureResolution, 64, 512);
        cellsPerTexture = Mathf.Max(1, cellsPerTexture);
        majorCellSpan = Mathf.Clamp(majorCellSpan, 1, cellsPerTexture);

        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        bool hasMipmaps = texture != null && texture.mipmapCount > 1;
        if (texture != null && (texture.width != resolution || texture.height != resolution || hasMipmaps != generateMipmaps))
        {
            AssetDatabase.DeleteAsset(path);
            texture = null;
        }

        if (texture == null)
        {
            texture = new Texture2D(resolution, resolution, TextureFormat.RGBA32, generateMipmaps, false);
            texture.name = Path.GetFileNameWithoutExtension(path);
            AssetDatabase.CreateAsset(texture, path);
        }

        texture.wrapMode = TextureWrapMode.Repeat;
        texture.filterMode = FilterMode.Bilinear;
        texture.anisoLevel = 4;

        var pixels = new Color[resolution * resolution];
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float u = (x + 0.5f) / resolution;
                float v = (y + 0.5f) / resolution;
                int cellX = Mathf.FloorToInt(u * cellsPerTexture);
                int cellY = Mathf.FloorToInt(v * cellsPerTexture);
                int majorCellX = cellX / majorCellSpan;
                int majorCellY = cellY / majorCellSpan;
                float minorTone = ((cellX + cellY) & 1) == 0 ? lowTone : highTone;
                float majorTone = ((majorCellX + majorCellY) & 1) == 0 ? -majorToneStrength : majorToneStrength;
                float tone = Mathf.Clamp01(minorTone + majorTone);

                float minorU = Mathf.Repeat(u * cellsPerTexture, 1f);
                float minorV = Mathf.Repeat(v * cellsPerTexture, 1f);
                float minorDistancePixels = Mathf.Min(
                    Mathf.Min(minorU, 1f - minorU),
                    Mathf.Min(minorV, 1f - minorV)) * resolution / cellsPerTexture;
                float majorDistancePixels = Mathf.Min(
                    Mathf.Min(u, 1f - u),
                    Mathf.Min(v, 1f - v)) * resolution;

                float minorMask = LineMask(minorDistancePixels, minorLinePixels) * minorLineAlpha;
                float majorMask = LineMask(majorDistancePixels, majorLinePixels) * majorLineAlpha;
                float lineMask = Mathf.Clamp01(Mathf.Max(minorMask, majorMask));
                float noise = (Hash01(x, y) - 0.5f) * config.noiseStrength;
                float value = Mathf.Clamp01(tone + lineMask * lineToneBoost + noise);
                pixels[y * resolution + x] = new Color(value, value, value, lineMask);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(generateMipmaps, false);
        EditorUtility.SetDirty(texture);
        return texture;
    }

    private static int FloorCellsPerTexture(GridVisualConfig config)
    {
        return Mathf.Max(1, Mathf.RoundToInt(Mathf.Max(0.01f, config.majorSpacing) / Mathf.Max(0.01f, config.minorSpacing)));
    }

    private static float LocalGridTextureWorldSize(float minorSpacing, GridVisualConfig config)
    {
        return Mathf.Max(0.001f, minorSpacing) * Mathf.Max(1, config.cellsPerTexture);
    }

    private static float ActorUvTiling(float minorSpacing, GridVisualConfig config)
    {
        const float capsuleReferenceHeight = 2.2f;
        return Mathf.Max(0.25f, capsuleReferenceHeight / LocalGridTextureWorldSize(minorSpacing, config));
    }

    private static float SmallObjectUvTiling(float minorSpacing, GridVisualConfig config, float referenceSize)
    {
        return Mathf.Max(1f, referenceSize / LocalGridTextureWorldSize(minorSpacing, config));
    }

    private static float LineMask(float distancePixels, float widthPixels)
    {
        float t = Mathf.Clamp01(distancePixels - widthPixels);
        t = t * t * (3f - 2f * t);
        return 1f - t;
    }

    private static float Hash01(int x, int y)
    {
        unchecked
        {
            uint hash = (uint)(x * 374761393 + y * 668265263);
            hash = (hash ^ (hash >> 13)) * 1274126177u;
            return (hash ^ (hash >> 16)) / 4294967295f;
        }
    }

    private static Color ObjectGridColor(Color baseColor)
    {
        return Color.Lerp(baseColor, Color.white, 0.68f);
    }

    private static void SetColorIfPresent(Material material, string property, Color value)
    {
        if (material.HasProperty(property))
        {
            material.SetColor(property, value);
        }
    }

    private static void SetTextureIfPresent(Material material, string property, Texture value)
    {
        if (material.HasProperty(property))
        {
            material.SetTexture(property, value);
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
