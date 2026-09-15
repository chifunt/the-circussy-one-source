using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using TheCircussyOne.Config;

public static class TheCircussyOneFontAssets
{
    private const string RequiredReadexCharacters =
        "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789" +
        " !\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~" +
        "\u00d7\u2013\u2014\u2022\u2026\u2190\u2191\u2192\u2193";

    [MenuItem("Tools/The Circussy One/Apply Readex Pro Font")]
    public static void ApplyReadexProFont()
    {
        TMP_FontAsset tmpFont = GetOrCreateReadexTmpFontAsset();
        EnsureStaticProjectGlyphSet(tmpFont);
        Font uiFont = GetReadexSourceFont();

        DamageFeedbackVisualConfig damageConfig = TheCircussyOneConfigRepository.GetOrCreateDamageFeedbackVisualConfig();
        if (damageConfig.damageNumberFont != tmpFont)
        {
            damageConfig.damageNumberFont = tmpFont;
            EditorUtility.SetDirty(damageConfig);
        }

        HudVisualConfig hudConfig = TheCircussyOneConfigRepository.GetOrCreateHudVisualConfig();
        if (hudConfig.uiFont != uiFont)
        {
            hudConfig.uiFont = uiFont;
            EditorUtility.SetDirty(hudConfig);
        }

        XpGainCounterVisualConfig xpGainCounterConfig = TheCircussyOneConfigRepository.GetOrCreateXpGainCounterVisualConfig();
        if (xpGainCounterConfig.font != tmpFont)
        {
            xpGainCounterConfig.font = tmpFont;
            EditorUtility.SetDirty(xpGainCounterConfig);
        }

        WorldInteractionPromptVisualConfig worldInteractionPromptConfig = TheCircussyOneConfigRepository.GetOrCreateWorldInteractionPromptVisualConfig();
        if (worldInteractionPromptConfig.font != tmpFont)
        {
            worldInteractionPromptConfig.font = tmpFont;
            EditorUtility.SetDirty(worldInteractionPromptConfig);
        }

        ApplyTmpDefaultFont(tmpFont);
        TheCircussyOneDamageFeedbackVisualApplier.ApplyPrefab(damageConfig);
        TheCircussyOneXpGainCounterVisualApplier.ApplyPrefab(xpGainCounterConfig);
        TheCircussyOneWorldInteractionPromptVisualApplier.ApplyPrefab(worldInteractionPromptConfig);
        TheCircussyOneHudVisualApplier.ApplyPanelSettings(hudConfig);
        TheCircussyOneHudVisualApplier.ApplyToOpenScene(hudConfig);
        TheCircussyOneHudVisualApplier.PreviewOpenScene(hudConfig);
        AssetDatabase.SaveAssets();
        SceneView.RepaintAll();
        Debug.Log("The Circussy One Readex Pro font applied to HUD, TMP defaults, damage numbers, XP gain counter, and world interaction prompt.");
    }

    public static Font GetReadexSourceFont()
    {
        var font = AssetDatabase.LoadAssetAtPath<Font>(TheCircussyOneAssetPaths.ReadexProSourceFontPath);
        if (font == null)
        {
            throw new System.InvalidOperationException($"Readex Pro source font is missing at {TheCircussyOneAssetPaths.ReadexProSourceFontPath}.");
        }

        return font;
    }

    public static TMP_FontAsset GetOrCreateReadexTmpFontAsset()
    {
        var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TheCircussyOneAssetPaths.ReadexProTmpFontAssetPath);
        if (fontAsset != null)
        {
            EnsureStaticProjectGlyphSet(fontAsset);
            EnsurePersistentMaterial(fontAsset);
            return fontAsset;
        }

        Font sourceFont = GetReadexSourceFont();
        fontAsset = TMP_FontAsset.CreateFontAsset(
            sourceFont,
            90,
            9,
            GlyphRenderMode.SDFAA,
            1024,
            1024,
            AtlasPopulationMode.Dynamic,
            true);
        fontAsset.name = "ReadexPro SDF";
        EnsureStaticProjectGlyphSet(fontAsset);
        AssetDatabase.CreateAsset(fontAsset, TheCircussyOneAssetPaths.ReadexProTmpFontAssetPath);
        EnsurePersistentMaterial(fontAsset);
        EditorUtility.SetDirty(fontAsset);
        return fontAsset;
    }

    public static bool EnsureStaticAtlasMode(TMP_FontAsset fontAsset)
    {
        if (fontAsset == null || fontAsset.atlasPopulationMode == AtlasPopulationMode.Static)
        {
            return false;
        }

        fontAsset.atlasPopulationMode = AtlasPopulationMode.Static;
        EditorUtility.SetDirty(fontAsset);
        return true;
    }

    public static bool EnsureStaticProjectGlyphSet(TMP_FontAsset fontAsset)
    {
        if (fontAsset == null)
        {
            return false;
        }

        bool changed = false;

        if (!fontAsset.HasCharacters(RequiredReadexCharacters, out var missingCharacters) && missingCharacters.Count > 0)
        {
            if (fontAsset.atlasPopulationMode != AtlasPopulationMode.Dynamic)
            {
                fontAsset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
                changed = true;
            }

            string missingText = new string(missingCharacters.ToArray());
            if (!fontAsset.TryAddCharacters(missingText, out string stillMissing) && !string.IsNullOrEmpty(stillMissing))
            {
                Debug.LogWarning($"Readex Pro TMP font could not add project glyphs: {stillMissing}");
            }

            changed = true;
        }

        if (fontAsset.atlasPopulationMode != AtlasPopulationMode.Static)
        {
            fontAsset.atlasPopulationMode = AtlasPopulationMode.Static;
            changed = true;
        }

        if (changed)
        {
            EditorUtility.SetDirty(fontAsset);
        }

        return changed;
    }

    public static Texture2D EnsurePersistentAtlas(TMP_FontAsset fontAsset)
    {
        if (fontAsset == null || fontAsset.atlasTexture == null)
        {
            return null;
        }

        Texture2D atlas = fontAsset.atlasTexture;
        if (!AssetDatabase.Contains(atlas) && AssetDatabase.Contains(fontAsset))
        {
            atlas.name = $"{fontAsset.name} Atlas";
            AssetDatabase.AddObjectToAsset(atlas, fontAsset);
            EditorUtility.SetDirty(fontAsset);
            EditorUtility.SetDirty(atlas);
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(fontAsset));
            atlas = fontAsset.atlasTexture;
        }

        return atlas;
    }

    public static Material EnsurePersistentMaterial(TMP_FontAsset fontAsset)
    {
        if (fontAsset == null)
        {
            return null;
        }

        Texture2D atlas = EnsurePersistentAtlas(fontAsset);
        Material material = fontAsset.material;
        bool mustCreate = material == null || !AssetDatabase.Contains(material);
        bool materialChanged = false;
        if (mustCreate)
        {
            Shader shader = Shader.Find("TextMeshPro/Distance Field");
            if (shader == null)
            {
                throw new System.InvalidOperationException("Could not find TextMeshPro/Distance Field shader for Readex Pro font material.");
            }

            material = new Material(shader)
            {
                name = $"{fontAsset.name} Material"
            };
            AssetDatabase.AddObjectToAsset(material, fontAsset);
            fontAsset.material = material;
            materialChanged = true;
        }

        if (atlas != null && material.mainTexture != atlas)
        {
            material.mainTexture = atlas;
            materialChanged = true;
        }

        if (atlas != null && material.GetTexture("_MainTex") != atlas)
        {
            material.SetTexture("_MainTex", atlas);
            materialChanged = true;
        }

        if (materialChanged)
        {
            EditorUtility.SetDirty(material);
        }

        if (mustCreate)
        {
            EditorUtility.SetDirty(fontAsset);
        }

        return material;
    }

    public static void ApplyTmpDefaultFont(TMP_FontAsset fontAsset)
    {
        if (fontAsset == null)
        {
            return;
        }

        if (TMP_Settings.defaultFontAsset != fontAsset)
        {
            TMP_Settings.defaultFontAsset = fontAsset;
        }

        Object settings = AssetDatabase.LoadAssetAtPath<Object>("Assets/_ThirdParty/TextMesh Pro/Resources/TMP Settings.asset");
        if (settings != null)
        {
            EditorUtility.SetDirty(settings);
        }
    }
}
