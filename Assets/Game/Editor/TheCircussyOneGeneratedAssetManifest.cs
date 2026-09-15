using System;
using System.Collections.Generic;
using System.Linq;
using TheCircussyOne.Config;
using TheCircussyOne.VisualTests.Editor;

public enum GeneratedAssetCategory
{
    Prefab,
    VfxPrefab,
    Material,
    Shader,
    Texture,
    GridTexture,
    Lighting,
    Ui,
    VisualTestScenario,
    VisualTestScene
}

public enum GeneratedAssetOwner
{
    SceneBuilder,
    GridVisuals,
    LightingVisuals,
    DamageFeedback,
    XpGainCounterVisuals,
    WorldInteractionPromptVisuals,
    EnemySpawnVisuals,
    VfxVisuals,
    HudVisuals,
    VisualTestLab
}

public enum GeneratedAssetSafeAction
{
    None,
    ApplyProjectedVisuals,
    GenerateVisualTestLab,
    GenerateAuthoringShowroom,
    StructuralReset
}

public readonly struct GeneratedAssetManifestEntry
{
    public GeneratedAssetManifestEntry(
        string path,
        GeneratedAssetCategory category,
        GeneratedAssetOwner owner,
        ConfigWorkflowKind workflowKind,
        GeneratedAssetSafeAction safeAction,
        string displayName)
    {
        Path = path;
        Category = category;
        Owner = owner;
        WorkflowKind = workflowKind;
        SafeAction = safeAction;
        DisplayName = displayName;
    }

    public string Path { get; }
    public GeneratedAssetCategory Category { get; }
    public GeneratedAssetOwner Owner { get; }
    public ConfigWorkflowKind WorkflowKind { get; }
    public GeneratedAssetSafeAction SafeAction { get; }
    public string DisplayName { get; }
}

public static class TheCircussyOneGeneratedAssetManifest
{
    private static readonly Lazy<IReadOnlyList<GeneratedAssetManifestEntry>> CoreGeneratedAssetsLazy = new(BuildCoreGeneratedAssets);
    private static readonly Lazy<IReadOnlyList<GeneratedAssetManifestEntry>> VisualTestLabAssetsLazy = new(BuildVisualTestLabAssets);
    private static readonly Lazy<IReadOnlyList<GeneratedAssetManifestEntry>> AllLazy = new(() =>
        CoreGeneratedAssets.Concat(VisualTestLabAssets).ToArray());

    public static IReadOnlyList<GeneratedAssetManifestEntry> All => AllLazy.Value;
    public static IReadOnlyList<GeneratedAssetManifestEntry> CoreGeneratedAssets => CoreGeneratedAssetsLazy.Value;
    public static IReadOnlyList<GeneratedAssetManifestEntry> VisualTestLabAssets => VisualTestLabAssetsLazy.Value;

    public static GeneratedAssetManifestEntry? FindByPath(string path)
    {
        for (int i = 0; i < All.Count; i++)
        {
            if (string.Equals(All[i].Path, path, StringComparison.Ordinal))
            {
                return All[i];
            }
        }

        return null;
    }

    private static IReadOnlyList<GeneratedAssetManifestEntry> BuildCoreGeneratedAssets()
    {
        var entries = new List<GeneratedAssetManifestEntry>
        {
            Structural(TheCircussyOneAssetPaths.ScenePath, GeneratedAssetCategory.VisualTestScene, "Prototype Scene"),
            Structural(TheCircussyOneAssetPaths.PrefabFolder + "/Enemy.prefab", GeneratedAssetCategory.Prefab, "Enemy Prefab"),
            Structural(TheCircussyOneAssetPaths.PrefabFolder + "/Projectile.prefab", GeneratedAssetCategory.Prefab, "Projectile Prefab"),
            Structural(TheCircussyOneAssetPaths.PrefabFolder + "/ExperiencePickup.prefab", GeneratedAssetCategory.Prefab, "Experience Pickup Prefab"),
            Structural(TheCircussyOneAssetPaths.PickupDiamondMeshPath, GeneratedAssetCategory.Prefab, "Pickup Diamond Mesh"),

            Projected(TheCircussyOneAssetPaths.DamageNumberPrefabPath, GeneratedAssetCategory.Prefab, GeneratedAssetOwner.DamageFeedback, "Damage Number Prefab"),
            Projected(TheCircussyOneAssetPaths.DamageNumberMaterialPath, GeneratedAssetCategory.Material, GeneratedAssetOwner.DamageFeedback, "Damage Number Overlay Material"),
            Projected(TheCircussyOneAssetPaths.PlayerDamageVignetteShaderPath, GeneratedAssetCategory.Shader, GeneratedAssetOwner.DamageFeedback, "Player Damage Vignette Shader"),
            Projected(TheCircussyOneAssetPaths.PlayerDamageVignetteMaterialPath, GeneratedAssetCategory.Material, GeneratedAssetOwner.DamageFeedback, "Player Damage Vignette Material"),
            Projected(TheCircussyOneAssetPaths.InteractableSelectionMaskShaderPath, GeneratedAssetCategory.Shader, GeneratedAssetOwner.VfxVisuals, "Interactable Selection Mask Shader"),
            Projected(TheCircussyOneAssetPaths.InteractableSelectionOutlineShaderPath, GeneratedAssetCategory.Shader, GeneratedAssetOwner.VfxVisuals, "Interactable Selection Outline Shader"),
            Projected(TheCircussyOneAssetPaths.XpGainCounterPrefabPath, GeneratedAssetCategory.Prefab, GeneratedAssetOwner.XpGainCounterVisuals, "XP Gain Counter Prefab"),
            Projected(TheCircussyOneAssetPaths.XpGainCounterMaterialPath, GeneratedAssetCategory.Material, GeneratedAssetOwner.XpGainCounterVisuals, "XP Gain Counter Overlay Material"),
            Projected(TheCircussyOneAssetPaths.WorldInteractionPromptPrefabPath, GeneratedAssetCategory.Prefab, GeneratedAssetOwner.WorldInteractionPromptVisuals, "World Interaction Prompt Prefab"),
            Projected(TheCircussyOneAssetPaths.WorldInteractionPromptMaterialPath, GeneratedAssetCategory.Material, GeneratedAssetOwner.WorldInteractionPromptVisuals, "World Interaction Prompt Overlay Material"),
            Projected(TheCircussyOneAssetPaths.WorldInteractionPromptBarMaterialPath, GeneratedAssetCategory.Material, GeneratedAssetOwner.WorldInteractionPromptVisuals, "World Interaction Prompt Bar Material"),
            Projected(TheCircussyOneAssetPaths.EnemySpawnIndicatorPrefabPath, GeneratedAssetCategory.Prefab, GeneratedAssetOwner.EnemySpawnVisuals, "Enemy Spawn Indicator Prefab"),
            Projected(TheCircussyOneAssetPaths.FireHoopOrbitPrefabPath, GeneratedAssetCategory.Prefab, GeneratedAssetOwner.VfxVisuals, "Fire Hoop Orbit Prefab"),

            Projected(TheCircussyOneAssetPaths.PlayerMoveDustPrefabPath, GeneratedAssetCategory.VfxPrefab, GeneratedAssetOwner.VfxVisuals, "Player Move Dust VFX"),
            Projected(TheCircussyOneAssetPaths.PlayerJumpTrailPrefabPath, GeneratedAssetCategory.VfxPrefab, GeneratedAssetOwner.VfxVisuals, "Player Jump Trail VFX"),
            Projected(TheCircussyOneAssetPaths.WorldAmbientDustPrefabPath, GeneratedAssetCategory.VfxPrefab, GeneratedAssetOwner.VfxVisuals, "World Ambient Dust VFX"),
            Projected(TheCircussyOneAssetPaths.FireHoopAreaTelegraphPrefabPath, GeneratedAssetCategory.VfxPrefab, GeneratedAssetOwner.VfxVisuals, "Fire Hoop Area Telegraph VFX"),
            Projected(TheCircussyOneAssetPaths.ProjectileMuzzlePuffPrefabPath, GeneratedAssetCategory.VfxPrefab, GeneratedAssetOwner.VfxVisuals, "Projectile Muzzle Puff VFX"),
            Projected(TheCircussyOneAssetPaths.ProjectileExplosionPrefabPath, GeneratedAssetCategory.VfxPrefab, GeneratedAssetOwner.VfxVisuals, "Projectile Explosion VFX"),
            Projected(TheCircussyOneAssetPaths.EnemyHitSparksPrefabPath, GeneratedAssetCategory.VfxPrefab, GeneratedAssetOwner.VfxVisuals, "Enemy Hit Sparks VFX"),
            Projected(TheCircussyOneAssetPaths.EnemyDeathBurstPrefabPath, GeneratedAssetCategory.VfxPrefab, GeneratedAssetOwner.VfxVisuals, "Enemy Death Burst VFX"),
            Projected(TheCircussyOneAssetPaths.XpPickupAttractTrailPrefabPath, GeneratedAssetCategory.VfxPrefab, GeneratedAssetOwner.VfxVisuals, "XP Pickup Attract Trail VFX"),
            Projected(TheCircussyOneAssetPaths.XpPickupCollectPopPrefabPath, GeneratedAssetCategory.VfxPrefab, GeneratedAssetOwner.VfxVisuals, "XP Pickup Collect Pop VFX"),
            Projected(TheCircussyOneAssetPaths.PlayerDamageBurstPrefabPath, GeneratedAssetCategory.VfxPrefab, GeneratedAssetOwner.VfxVisuals, "Player Damage Burst VFX"),
            Projected(TheCircussyOneAssetPaths.PlayerJumpTakeoffPrefabPath, GeneratedAssetCategory.VfxPrefab, GeneratedAssetOwner.VfxVisuals, "Player Jump Takeoff VFX"),
            Projected(TheCircussyOneAssetPaths.PlayerJumpLandPrefabPath, GeneratedAssetCategory.VfxPrefab, GeneratedAssetOwner.VfxVisuals, "Player Jump Land VFX"),
            Projected(TheCircussyOneAssetPaths.LevelUpBurstPrefabPath, GeneratedAssetCategory.VfxPrefab, GeneratedAssetOwner.VfxVisuals, "Level Up Burst VFX"),
            Projected(TheCircussyOneAssetPaths.VfxAlphaMaterialPath, GeneratedAssetCategory.Material, GeneratedAssetOwner.VfxVisuals, "VFX Alpha Material"),
            Projected(TheCircussyOneAssetPaths.VfxAdditiveMaterialPath, GeneratedAssetCategory.Material, GeneratedAssetOwner.VfxVisuals, "VFX Additive Material"),
            Projected(TheCircussyOneAssetPaths.VfxSoftDustMaterialPath, GeneratedAssetCategory.Material, GeneratedAssetOwner.VfxVisuals, "VFX Soft Dust Material"),
            Projected(TheCircussyOneAssetPaths.VfxSparkDiamondMaterialPath, GeneratedAssetCategory.Material, GeneratedAssetOwner.VfxVisuals, "VFX Spark Diamond Material"),
            Projected(TheCircussyOneAssetPaths.VfxSlashSparkMaterialPath, GeneratedAssetCategory.Material, GeneratedAssetOwner.VfxVisuals, "VFX Slash Spark Material"),
            Projected(TheCircussyOneAssetPaths.VfxConfettiStripMaterialPath, GeneratedAssetCategory.Material, GeneratedAssetOwner.VfxVisuals, "VFX Confetti Strip Material"),
            Projected(TheCircussyOneAssetPaths.VfxTicketPaperMaterialPath, GeneratedAssetCategory.Material, GeneratedAssetOwner.VfxVisuals, "VFX Ticket Paper Material"),
            Projected(TheCircussyOneAssetPaths.VfxSnackHeartMaterialPath, GeneratedAssetCategory.Material, GeneratedAssetOwner.VfxVisuals, "VFX Snack Heart Material"),
            Projected(TheCircussyOneAssetPaths.VfxSoftDustPuffTexturePath, GeneratedAssetCategory.Texture, GeneratedAssetOwner.VfxVisuals, "VFX Soft Dust Puff Texture"),
            Projected(TheCircussyOneAssetPaths.VfxSparkDiamondTexturePath, GeneratedAssetCategory.Texture, GeneratedAssetOwner.VfxVisuals, "VFX Spark Diamond Texture"),
            Projected(TheCircussyOneAssetPaths.VfxSlashSparkTexturePath, GeneratedAssetCategory.Texture, GeneratedAssetOwner.VfxVisuals, "VFX Slash Spark Texture"),
            Projected(TheCircussyOneAssetPaths.VfxConfettiStripTexturePath, GeneratedAssetCategory.Texture, GeneratedAssetOwner.VfxVisuals, "VFX Confetti Strip Texture"),
            Projected(TheCircussyOneAssetPaths.VfxTicketPaperTexturePath, GeneratedAssetCategory.Texture, GeneratedAssetOwner.VfxVisuals, "VFX Ticket Paper Texture"),
            Projected(TheCircussyOneAssetPaths.VfxSnackHeartTexturePath, GeneratedAssetCategory.Texture, GeneratedAssetOwner.VfxVisuals, "VFX Snack Heart Texture"),

            Projected(TheCircussyOneAssetPaths.HudUxmlPath, GeneratedAssetCategory.Ui, GeneratedAssetOwner.HudVisuals, "HUD UXML"),
            Projected(TheCircussyOneAssetPaths.HudUssPath, GeneratedAssetCategory.Ui, GeneratedAssetOwner.HudVisuals, "HUD USS"),
            Projected(TheCircussyOneAssetPaths.HudPanelSettingsPath, GeneratedAssetCategory.Ui, GeneratedAssetOwner.HudVisuals, "HUD Panel Settings"),

            Projected(TheCircussyOneAssetPaths.WorldGridShaderPath, GeneratedAssetCategory.Shader, GeneratedAssetOwner.GridVisuals, "World Grid Shader"),
            Projected(TheCircussyOneAssetPaths.GridTextureFolder + "/FloorGrid.asset", GeneratedAssetCategory.GridTexture, GeneratedAssetOwner.GridVisuals, "Floor Grid Texture"),
            Projected(TheCircussyOneAssetPaths.GridTextureFolder + "/ActorGrid.asset", GeneratedAssetCategory.GridTexture, GeneratedAssetOwner.GridVisuals, "Actor Grid Texture"),
            Projected(TheCircussyOneAssetPaths.GridTextureFolder + "/ProjectileGrid.asset", GeneratedAssetCategory.GridTexture, GeneratedAssetOwner.GridVisuals, "Projectile Grid Texture"),
            Projected(TheCircussyOneAssetPaths.GridTextureFolder + "/PickupGrid.asset", GeneratedAssetCategory.GridTexture, GeneratedAssetOwner.GridVisuals, "Pickup Grid Texture"),
            Projected(TheCircussyOneAssetPaths.FloorMaterialPath, GeneratedAssetCategory.Material, GeneratedAssetOwner.GridVisuals, "Floor Material"),
            Projected(TheCircussyOneAssetPaths.MaterialFolder + "/Player.mat", GeneratedAssetCategory.Material, GeneratedAssetOwner.GridVisuals, "Player Material"),
            Projected(TheCircussyOneAssetPaths.MaterialFolder + "/Enemy.mat", GeneratedAssetCategory.Material, GeneratedAssetOwner.GridVisuals, "Enemy Material"),
            Projected(TheCircussyOneAssetPaths.MaterialFolder + "/Projectile.mat", GeneratedAssetCategory.Material, GeneratedAssetOwner.GridVisuals, "Projectile Material"),
            Projected(TheCircussyOneAssetPaths.MaterialFolder + "/Pickup.mat", GeneratedAssetCategory.Material, GeneratedAssetOwner.GridVisuals, "Pickup Material"),

            Projected(TheCircussyOneAssetPaths.ContactShadowShaderPath, GeneratedAssetCategory.Shader, GeneratedAssetOwner.LightingVisuals, "Contact Shadow Shader"),
            Projected(TheCircussyOneAssetPaths.ContactShadowTexturePath, GeneratedAssetCategory.Texture, GeneratedAssetOwner.LightingVisuals, "Contact Shadow Texture"),
            Projected(TheCircussyOneAssetPaths.ContactShadowMaterialPath, GeneratedAssetCategory.Material, GeneratedAssetOwner.LightingVisuals, "Contact Shadow Material"),
            Projected(TheCircussyOneAssetPaths.LightingVolumeProfilePath, GeneratedAssetCategory.Lighting, GeneratedAssetOwner.LightingVisuals, "Lighting Volume Profile"),
            Structural(TheCircussyOneAssetPaths.FaceMaterialPath, GeneratedAssetCategory.Material, "Face Material")
        };

        return entries;
    }

    private static IReadOnlyList<GeneratedAssetManifestEntry> BuildVisualTestLabAssets()
    {
        var entries = new List<GeneratedAssetManifestEntry>();
        foreach (string scenarioId in VisualTestLabBuilder.BuiltInScenarioIds)
        {
            entries.Add(new GeneratedAssetManifestEntry(
                $"{VisualTestLabBuilder.ScenarioFolder}/{scenarioId}.asset",
                GeneratedAssetCategory.VisualTestScenario,
                GeneratedAssetOwner.VisualTestLab,
                ConfigWorkflowKind.ProjectedVisual,
                GeneratedAssetSafeAction.GenerateVisualTestLab,
                $"{scenarioId} Scenario"));

            entries.Add(new GeneratedAssetManifestEntry(
                $"{VisualTestLabBuilder.SceneFolder}/{scenarioId}.unity",
                GeneratedAssetCategory.VisualTestScene,
                GeneratedAssetOwner.VisualTestLab,
                ConfigWorkflowKind.ProjectedVisual,
                GeneratedAssetSafeAction.GenerateVisualTestLab,
                $"{scenarioId} Scene"));
        }

        entries.Add(new GeneratedAssetManifestEntry(
            AuthoringShowroomBuilder.ScenePath,
            GeneratedAssetCategory.VisualTestScene,
            GeneratedAssetOwner.VisualTestLab,
            ConfigWorkflowKind.ProjectedVisual,
            GeneratedAssetSafeAction.GenerateAuthoringShowroom,
            "Authoring Showroom Scene"));

        return entries;
    }

    private static GeneratedAssetManifestEntry Structural(string path, GeneratedAssetCategory category, string displayName)
    {
        return new GeneratedAssetManifestEntry(
            path,
            category,
            GeneratedAssetOwner.SceneBuilder,
            ConfigWorkflowKind.StructuralReset,
            GeneratedAssetSafeAction.StructuralReset,
            displayName);
    }

    private static GeneratedAssetManifestEntry Projected(string path, GeneratedAssetCategory category, GeneratedAssetOwner owner, string displayName)
    {
        return new GeneratedAssetManifestEntry(
            path,
            category,
            owner,
            ConfigWorkflowKind.ProjectedVisual,
            GeneratedAssetSafeAction.ApplyProjectedVisuals,
            displayName);
    }
}
