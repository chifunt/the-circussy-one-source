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
    internal static Material ApplyDamageFeedbackVisualAssets(DamageFeedbackVisualConfig config)
    {
        if (config == null)
        {
            config = ScriptableObject.CreateInstance<DamageFeedbackVisualConfig>();
        }

        config.EnsureReadableDefaults();
        TMP_FontAsset font = GetDamageNumberFontAsset(config);
        GetPlayerDamageVignetteMaterial(config);
        return GetDamageNumberMaterial(config, font);
    }

    internal static Material ApplyXpGainCounterVisualAssets(XpGainCounterVisualConfig config)
    {
        if (config == null)
        {
            config = ScriptableObject.CreateInstance<XpGainCounterVisualConfig>();
        }

        config.EnsureWorkflowDefaults();
        TMP_FontAsset font = GetXpGainCounterFontAsset(config);
        return GetXpGainCounterMaterial(config, font);
    }

    internal static Material ApplyWorldInteractionPromptVisualTextAssets(WorldInteractionPromptVisualConfig config)
    {
        if (config == null)
        {
            config = ScriptableObject.CreateInstance<WorldInteractionPromptVisualConfig>();
        }

        config.EnsureWorkflowDefaults();
        TMP_FontAsset font = GetWorldInteractionPromptFontAsset(config);
        return GetWorldInteractionPromptMaterial(config, font);
    }

    internal static Material ApplyWorldInteractionPromptVisualBarAssets(WorldInteractionPromptVisualConfig config)
    {
        if (config == null)
        {
            config = ScriptableObject.CreateInstance<WorldInteractionPromptVisualConfig>();
        }

        config.EnsureWorkflowDefaults();
        return GetWorldInteractionPromptBarMaterial(config);
    }

    internal static VfxVisualAssets ApplyVfxVisualAssets(VfxVisualConfig config)
    {
        if (config == null)
        {
            config = ScriptableObject.CreateInstance<VfxVisualConfig>();
        }

        config.EnsureWorkflowDefaults();
        EnsureInteractableSelectionOutlineRendererFeature();
        Material alphaMaterial = GetVfxParticleMaterial(VfxAlphaMaterialPath, additive: false);
        Material additiveMaterial = GetVfxParticleMaterial(VfxAdditiveMaterialPath, additive: true);
        VfxParticleMaterialSet particleMaterials = GetVfxParticleMaterials(alphaMaterial, additiveMaterial);
        ParticleSystem playerMoveDust = CreateContinuousParticlePrefab(
            PlayerMoveDustPrefabPath,
            "Player Move Dust",
            config.playerMoveDustColor,
            config.playerMoveDustLifetime,
            config.playerMoveDustStartSize,
            config.playerMoveDustMaxEmissionRate,
            particleMaterials.softDustAlpha,
            config.playerMoveDustLocalOffset,
            ParticleSystemSimulationSpace.World);
        ParticleSystem playerJumpTrail = CreateContinuousParticlePrefab(
            PlayerJumpTrailPrefabPath,
            "Player Jump Trail",
            config.playerJumpTrailColor,
            config.playerJumpTrailLifetime,
            config.playerJumpTrailStartSize,
            config.playerJumpTrailEmissionRate,
            particleMaterials.sparkDiamondAdditive,
            config.playerJumpTrailLocalOffset,
            ParticleSystemSimulationSpace.World);
        WorldAmbientDustView worldAmbientDust = CreateWorldAmbientDustPrefab(
            WorldAmbientDustPrefabPath,
            "World Ambient Dust",
            config,
            particleMaterials.softDustAlpha);
        FireHoopAreaTelegraphView fireHoopAreaTelegraph = CreateFireHoopAreaTelegraphPrefab(
            FireHoopAreaTelegraphPrefabPath,
            "Fire Hoop Area Telegraph",
            config,
            alphaMaterial);
        ParticleEffectView projectileMuzzlePuff = CreateOneShotParticlePrefab(
            ProjectileMuzzlePuffPrefabPath,
            "Projectile Muzzle Puff",
            VfxEffectId.ProjectileMuzzlePuff,
            config.projectileMuzzlePuff,
            particleMaterials);
        ParticleEffectView projectileBounceBurst = CreateOneShotParticlePrefab(
            ProjectileBounceBurstPrefabPath,
            "Projectile Bounce Burst",
            VfxEffectId.ProjectileBounceBurst,
            config.projectileBounceBurst,
            particleMaterials);
        ProjectileExplosionView projectileExplosion = CreateProjectileExplosionPrefab(
            ProjectileExplosionPrefabPath,
            "Projectile Explosion",
            VfxEffectId.ProjectileExplosion,
            config.projectileExplosion,
            particleMaterials,
            config);
        ParticleEffectView enemyHitSparks = CreateOneShotParticlePrefab(
            EnemyHitSparksPrefabPath,
            "Enemy Hit Sparks",
            VfxEffectId.EnemyHitSparks,
            config.enemyHitSparks,
            particleMaterials);
        ParticleEffectView knifeHitSparks = CreateOneShotParticlePrefab(
            KnifeHitSparksPrefabPath,
            "Knife Hit Sparks",
            VfxEffectId.KnifeHitSparks,
            config.knifeHitSparks,
            particleMaterials);
        ParticleEffectView enemyDeathBurst = CreateOneShotParticlePrefab(
            EnemyDeathBurstPrefabPath,
            "Enemy Death Burst",
            VfxEffectId.EnemyDeathBurst,
            config.enemyDeathBurst,
            particleMaterials);
        ParticleSystem xpAttractTrail = CreateContinuousParticlePrefab(
            XpPickupAttractTrailPrefabPath,
            "XP Pickup Attract Trail",
            config.pickupAttractTrailColor,
            config.pickupAttractTrailLifetime,
            config.pickupAttractTrailStartSize,
            config.pickupAttractTrailEmissionRate,
            particleMaterials.sparkDiamondAdditive,
            Vector3.zero,
            ParticleSystemSimulationSpace.World);
        ParticleEffectView xpCollectPop = CreateOneShotParticlePrefab(
            XpPickupCollectPopPrefabPath,
            "XP Pickup Collect Pop",
            VfxEffectId.XpPickupCollectPop,
            config.xpPickupCollectPop,
            particleMaterials);
        ParticleEffectView playerDamageBurst = CreateOneShotParticlePrefab(
            PlayerDamageBurstPrefabPath,
            "Player Damage Burst",
            VfxEffectId.PlayerDamageBurst,
            config.playerDamageBurst,
            particleMaterials);
        ParticleEffectView playerJumpTakeoff = CreateOneShotParticlePrefab(
            PlayerJumpTakeoffPrefabPath,
            "Player Jump Takeoff",
            VfxEffectId.PlayerJumpTakeoff,
            config.playerJumpTakeoff,
            particleMaterials);
        ParticleEffectView playerJumpLand = CreateOneShotParticlePrefab(
            PlayerJumpLandPrefabPath,
            "Player Jump Land",
            VfxEffectId.PlayerJumpLand,
            config.playerJumpLand,
            particleMaterials);
        ParticleEffectView levelUpBurst = CreateOneShotParticlePrefab(
            LevelUpBurstPrefabPath,
            "Level Up Burst",
            VfxEffectId.LevelUpBurst,
            config.levelUpBurst,
            particleMaterials);

        return new VfxVisualAssets(
            alphaMaterial,
            additiveMaterial,
            playerMoveDust,
            playerJumpTrail,
            worldAmbientDust,
            fireHoopAreaTelegraph,
            projectileMuzzlePuff,
            projectileBounceBurst,
            projectileExplosion,
            enemyHitSparks,
            knifeHitSparks,
            enemyDeathBurst,
            xpAttractTrail,
            xpCollectPop,
            playerDamageBurst,
            levelUpBurst,
            playerJumpTakeoff,
            playerJumpLand);
    }

    private static TMP_FontAsset GetDamageNumberFontAsset(DamageFeedbackVisualConfig config)
    {
        if (config != null && config.damageNumberFont != null)
        {
            return config.damageNumberFont;
        }

        return GetFallbackTmpFontAsset("generated damage numbers");
    }

    private static TMP_FontAsset GetXpGainCounterFontAsset(XpGainCounterVisualConfig config)
    {
        if (config != null && config.font != null)
        {
            return config.font;
        }

        var readexFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TheCircussyOneAssetPaths.ReadexProTmpFontAssetPath);
        if (readexFont != null)
        {
            return readexFont;
        }

        var damageConfig = AssetDatabase.LoadAssetAtPath<DamageFeedbackVisualConfig>(DamageFeedbackVisualConfigPath);
        if (damageConfig != null && damageConfig.damageNumberFont != null)
        {
            return damageConfig.damageNumberFont;
        }

        return GetFallbackTmpFontAsset("generated XP gain counter");
    }

    private static TMP_FontAsset GetWorldInteractionPromptFontAsset(WorldInteractionPromptVisualConfig config)
    {
        if (config != null && config.font != null)
        {
            return config.font;
        }

        XpGainCounterVisualConfig xpConfig = AssetDatabase.LoadAssetAtPath<XpGainCounterVisualConfig>(XpGainCounterVisualConfigPath);
        if (xpConfig != null && xpConfig.font != null)
        {
            return xpConfig.font;
        }

        var readexFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TheCircussyOneAssetPaths.ReadexProTmpFontAssetPath);
        if (readexFont != null)
        {
            return readexFont;
        }

        return GetFallbackTmpFontAsset("generated world interaction prompt");
    }

    private static Material GetDamageNumberMaterial(DamageFeedbackVisualConfig config, TMP_FontAsset font)
    {
        Material sourceMaterial = TheCircussyOneFontAssets.EnsurePersistentMaterial(font);
        Shader shader = Shader.Find("TextMeshPro/Distance Field Overlay")
            ?? Shader.Find("TextMeshPro/Mobile/Distance Field Overlay")
            ?? (sourceMaterial != null ? sourceMaterial.shader : Shader.Find("TextMeshPro/Distance Field"));
        if (shader == null)
        {
            throw new System.InvalidOperationException("Could not find a TextMeshPro distance field shader for damage numbers.");
        }

        var material = AssetDatabase.LoadAssetAtPath<Material>(DamageNumberMaterialPath);
        if (material == null)
        {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, DamageNumberMaterialPath);
        }

        if (sourceMaterial != null)
        {
            material.CopyPropertiesFromMaterial(sourceMaterial);
        }

        material.shader = shader;
        material.name = System.IO.Path.GetFileNameWithoutExtension(DamageNumberMaterialPath);
        material.renderQueue = (int)RenderQueue.Overlay;
        Texture2D atlasTexture = TheCircussyOneFontAssets.EnsurePersistentAtlas(font);
        if (atlasTexture != null)
        {
            material.mainTexture = atlasTexture;
            material.SetTexture("_MainTex", atlasTexture);
            SetFloatIfPresent(material, "_TextureWidth", atlasTexture.width);
            SetFloatIfPresent(material, "_TextureHeight", atlasTexture.height);
        }

        SetFloatIfPresent(material, "_GradientScale", Mathf.Max(4f, config.sdfGradientScale));
        SetFloatIfPresent(material, "_Sharpness", Mathf.Clamp(config.sdfSharpness, -1f, 1f));
        SetColorIfPresent(material, "_FaceColor", config.numberColor);
        SetColorIfPresent(material, "_OutlineColor", config.outlineColor);
        SetFloatIfPresent(material, "_OutlineWidth", Mathf.Max(0f, config.outlineWidth));
        SetFloatIfPresent(material, "_ZTest", (float)CompareFunction.Always);
        SetFloatIfPresent(material, "_ZWrite", 0f);
        SetFloatIfPresent(material, "_CullMode", (float)CullMode.Off);
        ShaderUtilities.UpdateShaderRatios(material);
        EditorUtility.SetDirty(material);
        return material;
    }

    internal static Material GetPlayerDamageVignetteMaterial(DamageFeedbackVisualConfig config)
    {
        AssetDatabase.ImportAsset(PlayerDamageVignetteShaderPath);
        Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(PlayerDamageVignetteShaderPath)
            ?? Shader.Find("TheCircussyOne/Player Damage Vignette");
        if (shader == null)
        {
            throw new System.InvalidOperationException("Could not find the player damage vignette shader.");
        }

        var material = AssetDatabase.LoadAssetAtPath<Material>(PlayerDamageVignetteMaterialPath);
        if (material == null)
        {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, PlayerDamageVignetteMaterialPath);
        }

        material.shader = shader;
        material.name = System.IO.Path.GetFileNameWithoutExtension(PlayerDamageVignetteMaterialPath);
        material.renderQueue = (int)RenderQueue.Overlay;
        material.SetColor("_Tint", config.playerDamageVignetteColor);
        material.SetColor("_DarkTint", config.playerDamageVignetteDarkColor);
        material.SetFloat("_Intensity", 0f);
        material.SetFloat("_MaxOpacity", Mathf.Clamp01(config.playerDamageVignetteMaxOpacity));
        material.SetFloat("_DarkMaxOpacity", Mathf.Clamp01(config.playerDamageVignetteDarkMaxOpacity));
        material.SetFloat("_GrainStrength", Mathf.Clamp01(config.playerDamageVignetteGrainStrength));
        material.SetFloat("_SplotchStrength", Mathf.Clamp01(config.playerDamageVignetteSplotchStrength));
        material.SetFloat("_Radius", Mathf.Clamp01(config.playerDamageVignetteRadius));
        material.SetFloat("_Softness", Mathf.Clamp01(config.playerDamageVignetteSoftness));
        material.SetFloat("_TimeValue", 0f);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static Material GetXpGainCounterMaterial(XpGainCounterVisualConfig config, TMP_FontAsset font)
    {
        Material sourceMaterial = TheCircussyOneFontAssets.EnsurePersistentMaterial(font);
        Shader shader = Shader.Find("TextMeshPro/Distance Field Overlay")
            ?? Shader.Find("TextMeshPro/Mobile/Distance Field Overlay")
            ?? (sourceMaterial != null ? sourceMaterial.shader : Shader.Find("TextMeshPro/Distance Field"));
        if (shader == null)
        {
            throw new System.InvalidOperationException("Could not find a TextMeshPro distance field shader for XP gain counter.");
        }

        var material = AssetDatabase.LoadAssetAtPath<Material>(XpGainCounterMaterialPath);
        if (material == null)
        {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, XpGainCounterMaterialPath);
        }

        if (sourceMaterial != null)
        {
            material.CopyPropertiesFromMaterial(sourceMaterial);
        }

        material.shader = shader;
        material.name = System.IO.Path.GetFileNameWithoutExtension(XpGainCounterMaterialPath);
        material.renderQueue = config.alwaysOnTop ? (int)RenderQueue.Overlay : (int)RenderQueue.Transparent;
        Texture2D atlasTexture = TheCircussyOneFontAssets.EnsurePersistentAtlas(font);
        if (atlasTexture != null)
        {
            material.mainTexture = atlasTexture;
            material.SetTexture("_MainTex", atlasTexture);
            SetFloatIfPresent(material, "_TextureWidth", atlasTexture.width);
            SetFloatIfPresent(material, "_TextureHeight", atlasTexture.height);
        }

        SetFloatIfPresent(material, "_GradientScale", Mathf.Max(4f, config.sdfGradientScale));
        SetFloatIfPresent(material, "_Sharpness", Mathf.Clamp(config.sdfSharpness, -1f, 1f));
        // Runtime counters pass their own vertex color per frame. Keep material color neutral
        // so XP cyan and ticket gold do not get multiplied into the wrong hue.
        SetColorIfPresent(material, "_FaceColor", Color.white);
        SetColorIfPresent(material, "_GlowColor", Color.clear);
        SetColorIfPresent(material, "_OutlineColor", config.outlineColor);
        SetFloatIfPresent(material, "_OutlineWidth", Mathf.Max(0f, config.outlineWidth));
        SetFloatIfPresent(material, "_ZTest", config.alwaysOnTop ? (float)CompareFunction.Always : (float)CompareFunction.LessEqual);
        SetFloatIfPresent(material, "_ZWrite", 0f);
        SetFloatIfPresent(material, "_CullMode", (float)CullMode.Off);
        ShaderUtilities.UpdateShaderRatios(material);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static Material GetWorldInteractionPromptMaterial(WorldInteractionPromptVisualConfig config, TMP_FontAsset font)
    {
        Material sourceMaterial = TheCircussyOneFontAssets.EnsurePersistentMaterial(font);
        Shader shader = Shader.Find("TextMeshPro/Distance Field Overlay")
            ?? Shader.Find("TextMeshPro/Mobile/Distance Field Overlay")
            ?? (sourceMaterial != null ? sourceMaterial.shader : Shader.Find("TextMeshPro/Distance Field"));
        if (shader == null)
        {
            throw new System.InvalidOperationException("Could not find a TextMeshPro distance field shader for world interaction prompts.");
        }

        var material = AssetDatabase.LoadAssetAtPath<Material>(WorldInteractionPromptMaterialPath);
        if (material == null)
        {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, WorldInteractionPromptMaterialPath);
        }

        if (sourceMaterial != null)
        {
            material.CopyPropertiesFromMaterial(sourceMaterial);
        }

        material.shader = shader;
        material.name = System.IO.Path.GetFileNameWithoutExtension(WorldInteractionPromptMaterialPath);
        material.renderQueue = config.alwaysOnTop ? (int)RenderQueue.Overlay : (int)RenderQueue.Transparent;
        Texture2D atlasTexture = TheCircussyOneFontAssets.EnsurePersistentAtlas(font);
        if (atlasTexture != null)
        {
            material.mainTexture = atlasTexture;
            material.SetTexture("_MainTex", atlasTexture);
            SetFloatIfPresent(material, "_TextureWidth", atlasTexture.width);
            SetFloatIfPresent(material, "_TextureHeight", atlasTexture.height);
        }

        SetFloatIfPresent(material, "_GradientScale", Mathf.Max(4f, config.sdfGradientScale));
        SetFloatIfPresent(material, "_Sharpness", Mathf.Clamp(config.sdfSharpness, -1f, 1f));
        SetColorIfPresent(material, "_FaceColor", Color.white);
        SetColorIfPresent(material, "_GlowColor", Color.clear);
        SetColorIfPresent(material, "_OutlineColor", config.textOutlineColor);
        SetFloatIfPresent(material, "_OutlineWidth", Mathf.Max(0f, config.outlineWidth));
        SetFloatIfPresent(material, "_WeightNormal", Mathf.Clamp01(config.sdfWeightNormal));
        SetFloatIfPresent(material, "_WeightBold", Mathf.Clamp01(config.sdfWeightBold));
        SetFloatIfPresent(material, "_ZTest", config.alwaysOnTop ? (float)CompareFunction.Always : (float)CompareFunction.LessEqual);
        SetFloatIfPresent(material, "_ZWrite", 0f);
        SetFloatIfPresent(material, "_CullMode", (float)CullMode.Off);
        ShaderUtilities.UpdateShaderRatios(material);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static Material GetWorldInteractionPromptBarMaterial(WorldInteractionPromptVisualConfig config)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit")
            ?? Shader.Find("Unlit/Transparent")
            ?? Shader.Find("Unlit/Color")
            ?? Shader.Find("Sprites/Default");
        if (shader == null)
        {
            throw new System.InvalidOperationException("Could not find an unlit shader for world interaction prompt bars.");
        }

        var material = AssetDatabase.LoadAssetAtPath<Material>(WorldInteractionPromptBarMaterialPath);
        if (material == null)
        {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, WorldInteractionPromptBarMaterialPath);
        }

        material.shader = shader;
        material.name = System.IO.Path.GetFileNameWithoutExtension(WorldInteractionPromptBarMaterialPath);
        material.renderQueue = config.alwaysOnTop ? (int)RenderQueue.Overlay : (int)RenderQueue.Transparent;
        SetColorIfPresent(material, "_BaseColor", Color.white);
        SetColorIfPresent(material, "_Color", Color.white);
        SetFloatIfPresent(material, "_ZTest", config.alwaysOnTop ? (float)CompareFunction.Always : (float)CompareFunction.LessEqual);
        SetFloatIfPresent(material, "_ZWrite", 0f);
        SetFloatIfPresent(material, "_CullMode", (float)CullMode.Off);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static TMP_FontAsset GetFallbackTmpFontAsset(string context)
    {
        TMP_FontAsset defaultFont = null;
        try
        {
            defaultFont = TMP_Settings.defaultFontAsset;
        }
        catch (System.NullReferenceException)
        {
            defaultFont = null;
        }

        if (defaultFont != null)
        {
            return defaultFont;
        }

        string[] fontGuids = AssetDatabase.FindAssets("t:TMP_FontAsset");
        for (int i = 0; i < fontGuids.Length; i++)
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(fontGuids[i]));
            if (font != null)
            {
                return font;
            }
        }

        throw new System.InvalidOperationException($"No TMP_FontAsset was found for {context}.");
    }

    private static VfxParticleMaterialSet GetVfxParticleMaterials(Material alphaMaterial, Material additiveMaterial)
    {
        Texture2D softDust = GetOrCreateVfxParticleTexture(
            TheCircussyOneAssetPaths.VfxSoftDustPuffTexturePath,
            VfxParticleTextureKind.SoftDustPuff);
        Texture2D sparkDiamond = GetOrCreateVfxParticleTexture(
            TheCircussyOneAssetPaths.VfxSparkDiamondTexturePath,
            VfxParticleTextureKind.SparkDiamond);
        Texture2D slashSpark = GetOrCreateVfxParticleTexture(
            TheCircussyOneAssetPaths.VfxSlashSparkTexturePath,
            VfxParticleTextureKind.SlashSpark);
        Texture2D confettiStrip = GetOrCreateVfxParticleTexture(
            TheCircussyOneAssetPaths.VfxConfettiStripTexturePath,
            VfxParticleTextureKind.ConfettiStrip);
        Texture2D ticketPaper = GetOrCreateVfxParticleTexture(
            TheCircussyOneAssetPaths.VfxTicketPaperTexturePath,
            VfxParticleTextureKind.TicketPaper);
        Texture2D snackHeart = GetOrCreateVfxParticleTexture(
            TheCircussyOneAssetPaths.VfxSnackHeartTexturePath,
            VfxParticleTextureKind.SnackHeart);

        return new VfxParticleMaterialSet(
            alphaMaterial,
            additiveMaterial,
            GetVfxParticleMaterial(TheCircussyOneAssetPaths.VfxSoftDustMaterialPath, additive: false, softDust),
            GetVfxParticleMaterial(TheCircussyOneAssetPaths.VfxSparkDiamondMaterialPath, additive: true, sparkDiamond),
            GetVfxParticleMaterial(TheCircussyOneAssetPaths.VfxSlashSparkMaterialPath, additive: true, slashSpark),
            GetVfxParticleMaterial(TheCircussyOneAssetPaths.VfxConfettiStripMaterialPath, additive: true, confettiStrip),
            GetVfxParticleMaterial(TheCircussyOneAssetPaths.VfxTicketPaperMaterialPath, additive: false, ticketPaper),
            GetVfxParticleMaterial(TheCircussyOneAssetPaths.VfxSnackHeartMaterialPath, additive: false, snackHeart));
    }

    private static Texture2D GetOrCreateVfxParticleTexture(string path, VfxParticleTextureKind kind)
    {
        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        Texture2D generated = VfxParticleTextureFactory.Create(kind, makeNoLongerReadable: false);
        generated.name = Path.GetFileNameWithoutExtension(path);
        if (texture == null)
        {
            AssetDatabase.CreateAsset(generated, path);
            texture = generated;
        }
        else
        {
            EditorUtility.CopySerialized(generated, texture);
            texture.name = generated.name;
            Object.DestroyImmediate(generated);
        }

        EditorUtility.SetDirty(texture);
        return texture;
    }

    private static Material GetVfxParticleMaterial(string path, bool additive, Texture texture = null)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
            ?? Shader.Find("Particles/Standard Unlit")
            ?? Shader.Find(additive ? "Mobile/Particles/Additive" : "Mobile/Particles/Alpha Blended");
        if (shader == null)
        {
            throw new System.InvalidOperationException("Could not find a particle shader for generated VFX materials.");
        }

        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }

        material.shader = shader;
        material.name = Path.GetFileNameWithoutExtension(path);
        material.renderQueue = (int)RenderQueue.Transparent;
        SetFloatIfPresent(material, "_Surface", 1f);
        SetFloatIfPresent(material, "_Blend", additive ? 2f : 0f);
        SetFloatIfPresent(material, "_SrcBlend", additive ? (float)BlendMode.SrcAlpha : (float)BlendMode.SrcAlpha);
        SetFloatIfPresent(material, "_DstBlend", additive ? (float)BlendMode.One : (float)BlendMode.OneMinusSrcAlpha);
        SetFloatIfPresent(material, "_ZWrite", 0f);
        SetFloatIfPresent(material, "_Cull", (float)CullMode.Off);
        SetColorIfPresent(material, "_BaseColor", Color.white);
        SetTextureIfPresent(material, "_BaseMap", texture);
        SetTextureIfPresent(material, "_MainTex", texture);
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.DisableKeyword("_ALPHATEST_ON");
        EditorUtility.SetDirty(material);
        return material;
    }

    private static Material ResolveOneShotParticleMaterial(
        VfxEffectId effectId,
        bool additive,
        VfxParticleMaterialSet materials)
    {
        return effectId switch
        {
            VfxEffectId.ProjectileMuzzlePuff => materials.softDustAlpha,
            VfxEffectId.ProjectileBounceBurst => materials.sparkDiamondAdditive,
            VfxEffectId.ProjectileExplosion => materials.sparkDiamondAdditive,
            VfxEffectId.EnemyHitSparks => materials.slashSparkAdditive,
            VfxEffectId.KnifeHitSparks => materials.slashSparkAdditive,
            VfxEffectId.EnemyDeathBurst => materials.confettiStripAdditive,
            VfxEffectId.XpPickupCollectPop => materials.sparkDiamondAdditive,
            VfxEffectId.PlayerDamageBurst => materials.sparkDiamondAdditive,
            VfxEffectId.PlayerJumpTakeoff => materials.softDustAlpha,
            VfxEffectId.PlayerJumpLand => materials.softDustAlpha,
            VfxEffectId.LevelUpBurst => materials.confettiStripAdditive,
            _ => additive ? materials.genericAdditive : materials.genericAlpha
        };
    }

    private readonly struct VfxParticleMaterialSet
    {
        public readonly Material genericAlpha;
        public readonly Material genericAdditive;
        public readonly Material softDustAlpha;
        public readonly Material sparkDiamondAdditive;
        public readonly Material slashSparkAdditive;
        public readonly Material confettiStripAdditive;
        public readonly Material ticketPaperAlpha;
        public readonly Material snackHeartAlpha;

        public VfxParticleMaterialSet(
            Material genericAlpha,
            Material genericAdditive,
            Material softDustAlpha,
            Material sparkDiamondAdditive,
            Material slashSparkAdditive,
            Material confettiStripAdditive,
            Material ticketPaperAlpha,
            Material snackHeartAlpha)
        {
            this.genericAlpha = genericAlpha;
            this.genericAdditive = genericAdditive;
            this.softDustAlpha = softDustAlpha;
            this.sparkDiamondAdditive = sparkDiamondAdditive;
            this.slashSparkAdditive = slashSparkAdditive;
            this.confettiStripAdditive = confettiStripAdditive;
            this.ticketPaperAlpha = ticketPaperAlpha;
            this.snackHeartAlpha = snackHeartAlpha;
        }
    }

    private static ParticleSystem CreateContinuousParticlePrefab(
        string path,
        string name,
        Color color,
        float lifetime,
        float startSize,
        float emissionRate,
        Material material,
        Vector3 localPosition,
        ParticleSystemSimulationSpace simulationSpace)
    {
        GameObject root = new GameObject(name);
        var particleSystem = root.AddComponent<ParticleSystem>();
        ConfigureParticleSystem(
            particleSystem,
            color,
            Mathf.Max(0.02f, lifetime),
            Mathf.Max(0.01f, startSize),
            speed: 0.12f,
            radius: 0.18f,
            emissionRate: Mathf.Max(0f, emissionRate),
            burstCount: 0,
            duration: Mathf.Max(0.2f, lifetime),
            maxParticles: Mathf.Max(32, Mathf.CeilToInt(emissionRate * Mathf.Max(0.1f, lifetime)) + 16),
            gravityModifier: 0f,
            material: material,
            sortingOrder: 4,
            loop: true,
            simulationSpace: simulationSpace);
        root.transform.localPosition = localPosition;
        root.SetActive(false);

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab.GetComponent<ParticleSystem>();
    }

    private static WorldAmbientDustView CreateWorldAmbientDustPrefab(
        string path,
        string name,
        VfxVisualConfig config,
        Material material)
    {
        GameObject root = new GameObject(name);
        var view = root.AddComponent<WorldAmbientDustView>();
        view.ConfigureDefaults(config, material);
        root.SetActive(false);

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab.GetComponent<WorldAmbientDustView>();
    }

    private static ParticleEffectView CreateOneShotParticlePrefab(
        string path,
        string name,
        VfxEffectId effectId,
        VfxOneShotSettings settings,
        VfxParticleMaterialSet particleMaterials)
    {
        if (settings == null)
        {
            settings = VfxOneShotSettings.Default(Color.white);
        }

        settings.EnsureDefaults();
        Material material = ResolveOneShotParticleMaterial(effectId, settings.additive, particleMaterials);
        GameObject root = new GameObject(name);
        var particleSystem = root.AddComponent<ParticleSystem>();
        ConfigureParticleSystem(
            particleSystem,
            settings.color,
            settings.lifetime,
            settings.startSize,
            settings.speed,
            settings.shapeRadius,
            emissionRate: 0f,
            burstCount: settings.burstCount,
            duration: settings.duration,
            maxParticles: settings.maxParticles,
            gravityModifier: settings.gravityModifier,
            material: material,
            sortingOrder: settings.sortingOrder,
            loop: false,
            simulationSpace: ParticleSystemSimulationSpace.World);
        ApplyOneShotParticleOverrides(effectId, particleSystem);

        var view = root.AddComponent<ParticleEffectView>();
        view.Configure(effectId);
        root.SetActive(false);

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab.GetComponent<ParticleEffectView>();
    }

    private static void ApplyOneShotParticleOverrides(VfxEffectId effectId, ParticleSystem particleSystem)
    {
        if (particleSystem == null)
        {
            return;
        }

        if (effectId == VfxEffectId.ProjectileBounceBurst)
        {
            var main = particleSystem.main;
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(0.18f, 0.88f, 1f, 0.92f),
                new Color(1f, 0.32f, 0.86f, 0.92f));

            var shape = particleSystem.shape;
            shape.shapeType = ParticleSystemShapeType.Hemisphere;
            shape.arc = 360f;
        }
        else if (effectId == VfxEffectId.KnifeHitSparks)
        {
            var main = particleSystem.main;
            main.startRotation = new ParticleSystem.MinMaxCurve(-0.75f, 0.75f);

            var shape = particleSystem.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 18f;
            shape.radius = 0.025f;
            shape.radiusThickness = 1f;
        }
    }

    private static FireHoopAreaTelegraphView CreateFireHoopAreaTelegraphPrefab(
        string path,
        string name,
        VfxVisualConfig config,
        Material material)
    {
        GameObject root = new GameObject(name);
        var view = root.AddComponent<FireHoopAreaTelegraphView>();
        view.ConfigureDefaults(config, material);
        root.SetActive(false);

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab.GetComponent<FireHoopAreaTelegraphView>();
    }

    private static ProjectileExplosionView CreateProjectileExplosionPrefab(
        string path,
        string name,
        VfxEffectId effectId,
        VfxOneShotSettings settings,
        VfxParticleMaterialSet particleMaterials,
        VfxVisualConfig config)
    {
        if (settings == null)
        {
            settings = VfxOneShotSettings.Default(Color.white);
        }

        settings.EnsureDefaults();
        Material material = ResolveOneShotParticleMaterial(effectId, settings.additive, particleMaterials);
        GameObject root = new GameObject(name);
        var particleSystem = root.AddComponent<ParticleSystem>();
        ConfigureParticleSystem(
            particleSystem,
            settings.color,
            settings.lifetime,
            settings.startSize,
            settings.speed,
            settings.shapeRadius,
            emissionRate: 0f,
            burstCount: settings.burstCount,
            duration: settings.duration,
            maxParticles: settings.maxParticles,
            gravityModifier: settings.gravityModifier,
            material: material,
            sortingOrder: settings.sortingOrder,
            loop: false,
            simulationSpace: ParticleSystemSimulationSpace.World);

        var particleView = root.AddComponent<ParticleEffectView>();
        particleView.Configure(effectId);
        var explosionView = root.AddComponent<ProjectileExplosionView>();
        SetSerialized(explosionView, "particleEffect", particleView);
        explosionView.ConfigureDefaults(config);
        root.SetActive(false);

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab.GetComponent<ProjectileExplosionView>();
    }

    private static void ConfigureParticleSystem(
        ParticleSystem particleSystem,
        Color color,
        float lifetime,
        float startSize,
        float speed,
        float radius,
        float emissionRate,
        int burstCount,
        float duration,
        int maxParticles,
        float gravityModifier,
        Material material,
        int sortingOrder,
        bool loop,
        ParticleSystemSimulationSpace simulationSpace)
    {
        var main = particleSystem.main;
        main.loop = loop;
        main.duration = Mathf.Max(0.03f, duration);
        main.startLifetime = Mathf.Max(0.02f, lifetime);
        main.startSize = Mathf.Max(0.005f, startSize);
        main.startSpeed = Mathf.Max(0f, speed);
        main.startColor = color;
        main.gravityModifier = gravityModifier;
        main.maxParticles = Mathf.Max(1, maxParticles);
        main.simulationSpace = simulationSpace;
        main.playOnAwake = false;
        main.scalingMode = ParticleSystemScalingMode.Hierarchy;

        var emission = particleSystem.emission;
        emission.enabled = true;
        emission.rateOverTime = loop ? Mathf.Max(0f, emissionRate) : 0f;
        if (loop)
        {
            emission.SetBursts(System.Array.Empty<ParticleSystem.Burst>());
        }
        else
        {
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)Mathf.Max(1, burstCount)) });
        }

        var shape = particleSystem.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = Mathf.Max(0.01f, radius);
        shape.radiusThickness = 0.15f;

        var colorOverLifetime = particleSystem.colorOverLifetime;
        colorOverLifetime.enabled = true;
        var gradient = new Gradient();
        gradient.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
        colorOverLifetime.color = gradient;

        var sizeOverLifetime = particleSystem.sizeOverLifetime;
        sizeOverLifetime.enabled = !loop;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 1f),
            new Keyframe(0.18f, 1.15f),
            new Keyframe(1f, 0.10f)));

        var renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sharedMaterial = material;
        renderer.sortingOrder = sortingOrder;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.lightProbeUsage = LightProbeUsage.Off;
        renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
    }
}
