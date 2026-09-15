using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Runtime;
using TheCircussyOne.VisualTests.Editor;
using UnityEditor;
using UnityEngine;

public static partial class AuthoringDoctorRunner
{
    private static readonly string[] ConfigAssetPaths =
    {
        TheCircussyOneAssetPaths.GameConfigPath,
        TheCircussyOneAssetPaths.RunScheduleConfigPath,
        TheCircussyOneAssetPaths.CannonWeaponPath,
        TheCircussyOneAssetPaths.KnifeFanWeaponPath,
        TheCircussyOneAssetPaths.SpotlightBoltWeaponPath,
        TheCircussyOneAssetPaths.FireHoopWeaponPath,
        TheCircussyOneAssetPaths.JugglingBallWeaponPath,
        TheCircussyOneAssetPaths.WeaponCatalogPath,
        TheCircussyOneAssetPaths.RubberSolesItemPath,
        TheCircussyOneAssetPaths.SafetyPaddingItemPath,
        TheCircussyOneAssetPaths.MagnetCharmItemPath,
        TheCircussyOneAssetPaths.LuckyCoinItemPath,
        TheCircussyOneAssetPaths.StudyNotesItemPath,
        TheCircussyOneAssetPaths.TempoBraceletItemPath,
        TheCircussyOneAssetPaths.PowderFlaskItemPath,
        TheCircussyOneAssetPaths.SpringBootsItemPath,
        TheCircussyOneAssetPaths.WideLensItemPath,
        TheCircussyOneAssetPaths.LongFuseItemPath,
        TheCircussyOneAssetPaths.ItemCatalogPath,
        TheCircussyOneAssetPaths.OpenChestPath,
        TheCircussyOneAssetPaths.LockedChestPath,
        TheCircussyOneAssetPaths.PremiumChestPath,
        TheCircussyOneAssetPaths.SpecialEnemyChestPath,
        TheCircussyOneAssetPaths.ChestCatalogPath,
        TheCircussyOneAssetPaths.SmallTicketStackPath,
        TheCircussyOneAssetPaths.TicketRollPath,
        TheCircussyOneAssetPaths.JackpotCachePath,
        TheCircussyOneAssetPaths.TicketDepositCatalogPath,
        TheCircussyOneAssetPaths.CannonTuningUpgradePath,
        TheCircussyOneAssetPaths.CannonPayloadUpgradePath,
        TheCircussyOneAssetPaths.CannonBiggerShotUpgradePath,
        TheCircussyOneAssetPaths.CannonQuickFuseUpgradePath,
        TheCircussyOneAssetPaths.CannonFastShotUpgradePath,
        TheCircussyOneAssetPaths.KnifeFanTuningUpgradePath,
        TheCircussyOneAssetPaths.KnifeFanFocusUpgradePath,
        TheCircussyOneAssetPaths.KnifeFanHonedBladesUpgradePath,
        TheCircussyOneAssetPaths.KnifeFanQuickTossUpgradePath,
        TheCircussyOneAssetPaths.KnifeFanFastBladesUpgradePath,
        TheCircussyOneAssetPaths.KnifeFanLingeringCutUpgradePath,
        TheCircussyOneAssetPaths.SpotlightBoltTuningUpgradePath,
        TheCircussyOneAssetPaths.SpotlightBoltBrightChargeUpgradePath,
        TheCircussyOneAssetPaths.SpotlightBoltQuickCueUpgradePath,
        TheCircussyOneAssetPaths.SpotlightBoltFastBeamUpgradePath,
        TheCircussyOneAssetPaths.SpotlightBoltWideBeamUpgradePath,
        TheCircussyOneAssetPaths.SpotlightBoltLingeringGlowUpgradePath,
        TheCircussyOneAssetPaths.FireHoopTuningUpgradePath,
        TheCircussyOneAssetPaths.FireHoopWiderUpgradePath,
        TheCircussyOneAssetPaths.FireHoopExtraUpgradePath,
        TheCircussyOneAssetPaths.FireHoopQuickSpinUpgradePath,
        TheCircussyOneAssetPaths.FireHoopBiggerFlameUpgradePath,
        TheCircussyOneAssetPaths.JugglingBallTuningUpgradePath,
        TheCircussyOneAssetPaths.JugglingBallTimingUpgradePath,
        TheCircussyOneAssetPaths.JugglingBallWeightedBallUpgradePath,
        TheCircussyOneAssetPaths.JugglingBallQuickTossUpgradePath,
        TheCircussyOneAssetPaths.JugglingBallFastRollUpgradePath,
        TheCircussyOneAssetPaths.JugglingBallBiggerBallUpgradePath,
        TheCircussyOneAssetPaths.JugglingBallLongJuggleUpgradePath,
        TheCircussyOneAssetPaths.UpgradeCatalogPath,
        TheCircussyOneAssetPaths.FootworkTalentPath,
        TheCircussyOneAssetPaths.StageStaminaTalentPath,
        TheCircussyOneAssetPaths.ToughSkinTalentPath,
        TheCircussyOneAssetPaths.CrowdFavoriteTalentPath,
        TheCircussyOneAssetPaths.QuickHandsTalentPath,
        TheCircussyOneAssetPaths.SharpEyeTalentPath,
        TheCircussyOneAssetPaths.LongReachTalentPath,
        TheCircussyOneAssetPaths.LuckyBreakTalentPath,
        TheCircussyOneAssetPaths.BiggerPropsTalentPath,
        TheCircussyOneAssetPaths.LongerActTalentPath,
        TheCircussyOneAssetPaths.MagneticApplauseTalentPath,
        TheCircussyOneAssetPaths.SpringboardTalentPath,
        TheCircussyOneAssetPaths.JugglerTalentPath,
        TheCircussyOneAssetPaths.CleanCompileTalentPath,
        TheCircussyOneAssetPaths.SampleArcTalentPath,
        TheCircussyOneAssetPaths.DebugMarkerTalentPath,
        TheCircussyOneAssetPaths.SteadyBaselineTalentPath,
        TheCircussyOneAssetPaths.CappaTalentPath,
        TheCircussyOneAssetPaths.RubberGrinTalentPath,
        TheCircussyOneAssetPaths.FaceForwardTalentPath,
        TheCircussyOneAssetPaths.CapsulePopTalentPath,
        TheCircussyOneAssetPaths.SampleSmileTalentPath,
        TheCircussyOneAssetPaths.StrongmanTalentPath,
        TheCircussyOneAssetPaths.CannonBraceTalentPath,
        TheCircussyOneAssetPaths.HeavyLiftTalentPath,
        TheCircussyOneAssetPaths.SteadyHandsTalentPath,
        TheCircussyOneAssetPaths.BulwarkStepTalentPath,
        TheCircussyOneAssetPaths.AcrobatTalentPath,
        TheCircussyOneAssetPaths.TightropeFootworkTalentPath,
        TheCircussyOneAssetPaths.KnifeFlourishTalentPath,
        TheCircussyOneAssetPaths.FastRecoveryTalentPath,
        TheCircussyOneAssetPaths.LongStrideTalentPath,
        TheCircussyOneAssetPaths.RingmasterTalentPath,
        TheCircussyOneAssetPaths.SpotlightCueTalentPath,
        TheCircussyOneAssetPaths.GrandEntranceTalentPath,
        TheCircussyOneAssetPaths.EncoreOddsTalentPath,
        TheCircussyOneAssetPaths.CommandingTempoTalentPath,
        TheCircussyOneAssetPaths.HoopFlowTalentPath,
        TheCircussyOneAssetPaths.EmberStepTalentPath,
        TheCircussyOneAssetPaths.CloseCircleTalentPath,
        TheCircussyOneAssetPaths.WarmApplauseTalentPath,
        TheCircussyOneAssetPaths.SecondCircleTalentPath,
        TheCircussyOneAssetPaths.EncoreBounceTalentPath,
        TheCircussyOneAssetPaths.SoftCatchTalentPath,
        TheCircussyOneAssetPaths.RicochetRhythmTalentPath,
        TheCircussyOneAssetPaths.CaromLineTalentPath,
        TheCircussyOneAssetPaths.PackedBallsTalentPath,
        TheCircussyOneAssetPaths.TalentCatalogPath,
        TheCircussyOneAssetPaths.JugglerPerformerPath,
        TheCircussyOneAssetPaths.CappaPerformerPath,
        TheCircussyOneAssetPaths.StrongmanPerformerPath,
        TheCircussyOneAssetPaths.AcrobatPerformerPath,
        TheCircussyOneAssetPaths.RingmasterPerformerPath,
        TheCircussyOneAssetPaths.SolaHoopDancerPerformerPath,
        TheCircussyOneAssetPaths.BibiBallJugglerPerformerPath,
        TheCircussyOneAssetPaths.PerformerCatalogPath,
        TheCircussyOneAssetPaths.BlueXpGemPath,
        TheCircussyOneAssetPaths.GreenXpGemPath,
        TheCircussyOneAssetPaths.RedXpGemPath,
        TheCircussyOneAssetPaths.XpGemCatalogPath,
        TheCircussyOneAssetPaths.TreatPath,
        TheCircussyOneAssetPaths.HealthPickupCatalogPath,
        TheCircussyOneAssetPaths.SnackBoxPath,
        TheCircussyOneAssetPaths.SnackCartPath,
        TheCircussyOneAssetPaths.HealingPropCatalogPath,
        TheCircussyOneAssetPaths.WorldRewardPlacementCatalogPath,
        TheCircussyOneAssetPaths.NormalEnemyPath,
        TheCircussyOneAssetPaths.EnemyCatalogPath,
        TheCircussyOneAssetPaths.OpeningHeadlinerEnemyPath,
        TheCircussyOneAssetPaths.OpeningHeadlinerPath,
        TheCircussyOneAssetPaths.HeadlinerCatalogPath,
        TheCircussyOneAssetPaths.CameraConfigPath,
        TheCircussyOneAssetPaths.GridVisualConfigPath,
        TheCircussyOneAssetPaths.LightingVisualConfigPath,
        TheCircussyOneAssetPaths.DamageFeedbackVisualConfigPath,
        TheCircussyOneAssetPaths.ActorMotionVisualConfigPath,
        TheCircussyOneAssetPaths.EnemySpawnVisualConfigPath,
        TheCircussyOneAssetPaths.VfxVisualConfigPath,
        TheCircussyOneAssetPaths.HudVisualConfigPath,
        TheCircussyOneAssetPaths.XpGainCounterVisualConfigPath,
        TheCircussyOneAssetPaths.WorldInteractionPromptVisualConfigPath
    };

    private static readonly string[] RequiredPrototypeRoots =
    {
        "Enemies",
        "Projectiles",
        "Pickups",
        "Damage Numbers",
        "XP Gain Counter",
        "Enemy Spawn Indicators",
        "VFX",
        "HUD",
        "Game Lifetime Scope"
    };

    private static readonly string[] TextScanRoots =
    {
        "Assets",
        "Packages",
        "ProjectSettings"
    };

    private static readonly string[] TextExtensions =
    {
        ".asmdef",
        ".asset",
        ".cs",
        ".inputactions",
        ".json",
        ".mat",
        ".meta",
        ".prefab",
        ".shader",
        ".txt",
        ".unity",
        ".uss",
        ".uxml"
    };

    private static readonly string[] OldCodenameNeedles =
    {
        "Vampire" + "Survivors" + "Lite",
        "Vampire" + " Survivors" + " Lite",
        "Mega" + "bonk" + "Lite",
        "Mega" + "bonk" + " Lite",
        "VS" + "L_"
    };

    private static readonly IAuthoringCheck[] Checks =
    {
        new ConfigAssetsCheck(),
        new RunScheduleContentCheck(),
        new WeaponContentCheck(),
        new ItemContentCheck(),
        new ChestContentCheck(),
        new TicketDepositContentCheck(),
        new HealthPickupContentCheck(),
        new HealingPropContentCheck(),
        new WorldRewardPlacementContentCheck(),
        new UpgradeContentCheck(),
        new TalentContentCheck(),
        new CharacterContentCheck(),
        new XpGemContentCheck(),
        new EnemyContentCheck(),
        new HeadlinerContentCheck(),
        new GeneratedAssetsCheck(),
        new LayerSetupCheck(),
        new VisualTestLabCheck(),
        new ScenarioBrowserCheck(),
        new RunStatsDebugCheck(),
        new OpenPrototypeSceneCheck(),
        new OldCodenameCheck(),
        new GeneratedCapturesCheck()
    };

    public static IReadOnlyList<string> RequiredConfigPaths => ConfigAssetPaths;
    public static IReadOnlyList<GeneratedAssetManifestEntry> RequiredGeneratedAssetEntries => TheCircussyOneGeneratedAssetManifest.CoreGeneratedAssets;

    public static List<AuthoringCheckResult> RunAllChecks()
    {
        var results = new List<AuthoringCheckResult>();
        for (int i = 0; i < Checks.Length; i++)
        {
            try
            {
                Checks[i].Run(results);
            }
            catch (Exception exception)
            {
                results.Add(new AuthoringCheckResult(
                    "doctor.check-failed",
                    "Doctor",
                    AuthoringCheckSeverity.Error,
                    "Authoring check failed",
                    $"{Checks[i].GetType().Name} threw {exception.GetType().Name}: {exception.Message}"));
            }
        }

        return SortResults(results).ToList();
    }

    public static IEnumerable<AuthoringCheckResult> SortResults(IEnumerable<AuthoringCheckResult> results)
    {
        return results
            .OrderBy(result => SeverityRank(result.Severity))
            .ThenBy(result => result.Category, StringComparer.Ordinal)
            .ThenBy(result => result.Title, StringComparer.Ordinal)
            .ThenBy(result => result.Id, StringComparer.Ordinal);
    }

    public static AuthoringCheckResult EvaluateRequiredAssetForTests(string path, string category, bool withFix)
    {
        return RequiredAssetResult(path, category, withFix ? ApplyProjectedVisuals : null, withFix ? "Apply Projected Visuals" : null);
    }

    public static AuthoringCheckResult EvaluateManifestEntryForTests(GeneratedAssetManifestEntry entry)
    {
        return RequiredAssetResult(entry);
    }

    public static List<AuthoringCheckResult> EvaluateWeaponCatalogForTests(
        IReadOnlyList<WeaponDefinition> definitions,
        string targetPath = "Assets/Test/WeaponCatalog.asset")
    {
        var catalog = ScriptableObject.CreateInstance<WeaponCatalog>();
        if (definitions != null)
        {
            catalog.availableWeapons.AddRange(definitions);
        }

        List<AuthoringCheckResult> results = SortResults(WeaponContentResults(catalog.ValidateContent(), targetPath)).ToList();
        UnityEngine.Object.DestroyImmediate(catalog);
        return results;
    }

    public static List<AuthoringCheckResult> EvaluateRunScheduleForTests(
        RunScheduleConfig config,
        string targetPath = "Assets/Test/RunScheduleConfig.asset")
    {
        List<AuthoringCheckResult> results = SortResults(RunScheduleContentResults(config?.ValidateContent(), targetPath)).ToList();
        return results;
    }

    public static List<AuthoringCheckResult> EvaluateUpgradeCatalogForTests(
        IReadOnlyList<UpgradeDefinition> definitions,
        string targetPath = "Assets/Test/UpgradeCatalog.asset")
    {
        var catalog = ScriptableObject.CreateInstance<UpgradeCatalog>();
        if (definitions != null)
        {
            catalog.upgrades.AddRange(definitions);
        }

        List<AuthoringCheckResult> results = SortResults(UpgradeContentResults(catalog.ValidateContent(), targetPath)).ToList();
        UnityEngine.Object.DestroyImmediate(catalog);
        return results;
    }

    public static List<AuthoringCheckResult> EvaluateItemCatalogForTests(
        IReadOnlyList<ItemDefinition> definitions,
        string targetPath = "Assets/Test/ItemCatalog.asset")
    {
        var catalog = ScriptableObject.CreateInstance<ItemCatalog>();
        if (definitions != null)
        {
            catalog.items.AddRange(definitions);
        }

        List<AuthoringCheckResult> results = SortResults(ItemContentResults(catalog.ValidateContent(), targetPath)).ToList();
        UnityEngine.Object.DestroyImmediate(catalog);
        return results;
    }

    public static List<AuthoringCheckResult> EvaluateTicketDepositCatalogForTests(
        IReadOnlyList<TicketDepositDefinition> definitions,
        string targetPath = "Assets/Test/TicketDepositCatalog.asset")
    {
        var catalog = ScriptableObject.CreateInstance<TicketDepositCatalog>();
        if (definitions != null)
        {
            catalog.deposits.AddRange(definitions);
        }

        List<AuthoringCheckResult> results = SortResults(TicketDepositContentResults(catalog.ValidateContent(), targetPath)).ToList();
        UnityEngine.Object.DestroyImmediate(catalog);
        return results;
    }

    public static List<AuthoringCheckResult> EvaluateChestCatalogForTests(
        IReadOnlyList<ChestDefinition> definitions,
        string targetPath = "Assets/Test/ChestCatalog.asset")
    {
        var catalog = ScriptableObject.CreateInstance<ChestCatalog>();
        if (definitions != null)
        {
            catalog.chests.AddRange(definitions);
        }

        List<AuthoringCheckResult> results = SortResults(ChestContentResults(catalog.ValidateContent(), targetPath)).ToList();
        UnityEngine.Object.DestroyImmediate(catalog);
        return results;
    }

    public static List<AuthoringCheckResult> EvaluateTalentCatalogForTests(
        IReadOnlyList<TalentDefinition> definitions,
        string targetPath = "Assets/Test/TalentCatalog.asset")
    {
        var catalog = ScriptableObject.CreateInstance<TalentCatalog>();
        if (definitions != null)
        {
            catalog.talents.AddRange(definitions);
        }

        List<AuthoringCheckResult> results = SortResults(TalentContentResults(catalog.ValidateContent(), targetPath)).ToList();
        UnityEngine.Object.DestroyImmediate(catalog);
        return results;
    }

    public static List<AuthoringCheckResult> EvaluatePerformerCatalogForTests(
        IReadOnlyList<PerformerDefinition> definitions,
        PerformerDefinition defaultPerformer = null,
        string targetPath = "Assets/Test/PerformerCatalog.asset")
    {
        var catalog = ScriptableObject.CreateInstance<PerformerCatalog>();
        catalog.defaultPerformer = defaultPerformer;
        if (definitions != null)
        {
            catalog.performers.AddRange(definitions);
        }

        List<AuthoringCheckResult> results = SortResults(CharacterContentResults(catalog.ValidateContent(), targetPath)).ToList();
        UnityEngine.Object.DestroyImmediate(catalog);
        return results;
    }

    public static List<AuthoringCheckResult> EvaluateXpGemCatalogForTests(
        IReadOnlyList<XpGemDefinition> definitions,
        string targetPath = "Assets/Test/XpGemCatalog.asset")
    {
        var catalog = ScriptableObject.CreateInstance<XpGemCatalog>();
        if (definitions != null)
        {
            catalog.gems.AddRange(definitions);
        }

        List<AuthoringCheckResult> results = SortResults(XpGemContentResults(catalog.ValidateContent(), targetPath)).ToList();
        UnityEngine.Object.DestroyImmediate(catalog);
        return results;
    }

    public static List<AuthoringCheckResult> EvaluateHealthPickupCatalogForTests(
        IReadOnlyList<HealthPickupDefinition> definitions,
        string targetPath = "Assets/Test/HealthPickupCatalog.asset")
    {
        var catalog = ScriptableObject.CreateInstance<HealthPickupCatalog>();
        if (definitions != null)
        {
            catalog.pickups.AddRange(definitions);
        }

        List<AuthoringCheckResult> results = SortResults(HealthPickupContentResults(catalog.ValidateContent(), targetPath)).ToList();
        UnityEngine.Object.DestroyImmediate(catalog);
        return results;
    }

    public static List<AuthoringCheckResult> EvaluateHealingPropCatalogForTests(
        IReadOnlyList<HealingPropDefinition> definitions,
        string targetPath = "Assets/Test/HealingPropCatalog.asset")
    {
        var catalog = ScriptableObject.CreateInstance<HealingPropCatalog>();
        if (definitions != null)
        {
            catalog.props.AddRange(definitions);
        }

        List<AuthoringCheckResult> results = SortResults(HealingPropContentResults(catalog.ValidateContent(), targetPath)).ToList();
        UnityEngine.Object.DestroyImmediate(catalog);
        return results;
    }

    public static List<AuthoringCheckResult> EvaluateWorldRewardPlacementCatalogForTests(
        IReadOnlyList<WorldRewardPlacementDefinition> definitions,
        string targetPath = "Assets/Test/WorldRewardPlacementCatalog.asset")
    {
        var catalog = ScriptableObject.CreateInstance<WorldRewardPlacementCatalog>();
        if (definitions != null)
        {
            catalog.placements.AddRange(definitions);
        }

        List<AuthoringCheckResult> results = SortResults(WorldRewardPlacementContentResults(catalog.ValidateContent(), targetPath)).ToList();
        UnityEngine.Object.DestroyImmediate(catalog);
        return results;
    }

    public static List<AuthoringCheckResult> EvaluateEnemyCatalogForTests(
        IReadOnlyList<EnemyDefinition> definitions,
        string targetPath = "Assets/Test/EnemyCatalog.asset")
    {
        var catalog = ScriptableObject.CreateInstance<EnemyCatalog>();
        if (definitions != null)
        {
            catalog.enemies.AddRange(definitions);
        }

        List<AuthoringCheckResult> results = SortResults(EnemyContentResults(catalog.ValidateContent(), targetPath)).ToList();
        UnityEngine.Object.DestroyImmediate(catalog);
        return results;
    }

    public static List<AuthoringCheckResult> EvaluateHeadlinerCatalogForTests(
        IReadOnlyList<HeadlinerDefinition> definitions,
        string targetPath = "Assets/Test/HeadlinerCatalog.asset")
    {
        var catalog = ScriptableObject.CreateInstance<HeadlinerCatalog>();
        if (definitions != null)
        {
            catalog.headliners.AddRange(definitions);
        }

        List<AuthoringCheckResult> results = SortResults(HeadlinerContentResults(catalog.ValidateContent(), targetPath)).ToList();
        UnityEngine.Object.DestroyImmediate(catalog);
        return results;
    }

    public static List<AuthoringCheckResult> EvaluateLayerSetupForTests(
        Func<string, int> layerLookup,
        Func<int, int, bool> ignoreCollisionLookup)
    {
        var results = new List<AuthoringCheckResult>();
        EvaluateLayerSetup(results, layerLookup, ignoreCollisionLookup);
        return SortResults(results).ToList();
    }

    public static bool ContainsOldCodenameForTests(string text)
    {
        return ContainsOldCodename(text);
    }

    public static void CreateMissingConfigs()
    {
        TheCircussyOneConfigRepository.GetOrCreateGameConfig();
        TheCircussyOneConfigRepository.GetOrCreateRunScheduleConfig();
        TheCircussyOneConfigRepository.GetOrCreateWeaponCatalog();
        TheCircussyOneConfigRepository.GetOrCreateItemCatalog();
        TheCircussyOneConfigRepository.GetOrCreateChestCatalog();
        TheCircussyOneConfigRepository.GetOrCreateTicketDepositCatalog();
        TheCircussyOneConfigRepository.GetOrCreateHealthPickupCatalog();
        TheCircussyOneConfigRepository.GetOrCreateHealingPropCatalog();
        TheCircussyOneConfigRepository.GetOrCreateWorldRewardPlacementCatalog();
        TheCircussyOneConfigRepository.GetOrCreateUpgradeCatalog();
        TheCircussyOneConfigRepository.GetOrCreateTalentCatalog();
        TheCircussyOneConfigRepository.GetOrCreatePerformerCatalog();
        TheCircussyOneConfigRepository.GetOrCreateXpGemCatalog();
        TheCircussyOneConfigRepository.GetOrCreateEnemyCatalog();
        TheCircussyOneConfigRepository.GetOrCreateHeadlinerCatalog();
        TheCircussyOneConfigRepository.GetOrCreateCameraConfig();
        TheCircussyOneConfigRepository.GetOrCreateGridVisualConfig();
        TheCircussyOneConfigRepository.GetOrCreateLightingVisualConfig();
        TheCircussyOneConfigRepository.GetOrCreateDamageFeedbackVisualConfig();
        TheCircussyOneConfigRepository.GetOrCreateActorMotionVisualConfig();
        TheCircussyOneConfigRepository.GetOrCreateEnemySpawnVisualConfig();
        TheCircussyOneConfigRepository.GetOrCreateVfxVisualConfig();
        TheCircussyOneConfigRepository.GetOrCreateHudVisualConfig();
        TheCircussyOneConfigRepository.GetOrCreateXpGainCounterVisualConfig();
        TheCircussyOneConfigRepository.GetOrCreateWorldInteractionPromptVisualConfig();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    public static void ApplyProjectedVisuals()
    {
        TheCircussyOneGridVisualApplier.ApplyNow();
        TheCircussyOneLightingVisualApplier.ApplyNow();
        TheCircussyOneDamageFeedbackVisualApplier.ApplyNow();
        TheCircussyOneXpGainCounterVisualApplier.ApplyNow();
        TheCircussyOneWorldInteractionPromptVisualApplier.ApplyNow();
        TheCircussyOneEnemySpawnVisualApplier.ApplyNow();
        TheCircussyOneVfxVisualApplier.ApplyNow();
        TheCircussyOneHudVisualApplier.ApplyNow();
    }

    public static void GenerateVisualTestLab()
    {
        VisualTestLabBuilder.GenerateAllScenes();
    }

    public static void GenerateAuthoringShowroom()
    {
        AuthoringShowroomBuilder.GenerateAuthoringShowroom();
    }

    public static void OpenAuthoringShowroom()
    {
        AuthoringShowroomBuilder.OpenAuthoringShowroom();
    }

    public static void OpenScenarioBrowser()
    {
        VisualTestLabScenarioBrowserWindow.Open();
    }

    private static AuthoringCheckResult RequiredAssetResult(
        string path,
        string category,
        Action fixAction = null,
        string fixLabel = null,
        AuthoringCheckSeverity severity = AuthoringCheckSeverity.Error)
    {
        if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path) != null || File.Exists(path) || Directory.Exists(path))
        {
            return null;
        }

        return new AuthoringCheckResult(
            $"missing.{SanitizeId(path)}",
            category,
            severity,
            "Missing required asset",
            $"Expected asset was not found: {path}",
            path,
            fixLabel,
            fixAction);
    }

    private static AuthoringCheckResult RequiredAssetResult(
        GeneratedAssetManifestEntry entry,
        AuthoringCheckSeverity severity = AuthoringCheckSeverity.Error)
    {
        return RequiredAssetResult(
            entry.Path,
            CategoryForEntry(entry),
            FixActionFor(entry.SafeAction),
            FixLabelFor(entry.SafeAction),
            severity);
    }

    private static string CategoryForEntry(GeneratedAssetManifestEntry entry)
    {
        return entry.Owner == GeneratedAssetOwner.VisualTestLab ? "Visual Test Lab" : "Generated Assets";
    }

    private static Action FixActionFor(GeneratedAssetSafeAction safeAction)
    {
        return safeAction switch
        {
            GeneratedAssetSafeAction.ApplyProjectedVisuals => ApplyProjectedVisuals,
            GeneratedAssetSafeAction.GenerateVisualTestLab => GenerateVisualTestLab,
            GeneratedAssetSafeAction.GenerateAuthoringShowroom => GenerateAuthoringShowroom,
            _ => null
        };
    }

    private static string FixLabelFor(GeneratedAssetSafeAction safeAction)
    {
        return safeAction switch
        {
            GeneratedAssetSafeAction.ApplyProjectedVisuals => "Apply Projected Visuals",
            GeneratedAssetSafeAction.GenerateVisualTestLab => "Generate Visual Test Lab",
            GeneratedAssetSafeAction.GenerateAuthoringShowroom => "Generate Authoring Showroom",
            _ => null
        };
    }

    private static IEnumerable<AuthoringCheckResult> WeaponContentResults(
        IReadOnlyList<ContentValidationIssue> issues,
        string targetPath)
    {
        if (issues == null || issues.Count == 0)
        {
            yield return new AuthoringCheckResult(
                "content.weapons.ok",
                "Content",
                AuthoringCheckSeverity.Info,
                "Weapon catalog is valid",
                "Weapon definitions have valid IDs, display names, and tag metadata.",
                targetPath);
            yield break;
        }

        for (int i = 0; i < issues.Count; i++)
        {
            ContentValidationIssue issue = issues[i];
            yield return new AuthoringCheckResult(
                $"content.weapon.{issue.Code}.{i}",
                "Content",
                issue.Severity == ContentValidationSeverity.Error ? AuthoringCheckSeverity.Error : AuthoringCheckSeverity.Warning,
                "Weapon catalog validation issue",
                issue.Message,
                targetPath);
        }
    }

    private static IEnumerable<AuthoringCheckResult> RunScheduleContentResults(
        IReadOnlyList<ContentValidationIssue> issues,
        string targetPath)
    {
        if (issues == null || issues.Count == 0)
        {
            yield return new AuthoringCheckResult(
                "content.run-schedule.ok",
                "Content",
                AuthoringCheckSeverity.Info,
                "Run schedule is valid",
                "Act durations, Showtime timestamps, and Showtime pressure settings are valid.",
                targetPath);
            yield break;
        }

        for (int i = 0; i < issues.Count; i++)
        {
            ContentValidationIssue issue = issues[i];
            yield return new AuthoringCheckResult(
                $"content.run-schedule.{issue.Code}.{i}",
                "Content",
                issue.Severity == ContentValidationSeverity.Error ? AuthoringCheckSeverity.Error : AuthoringCheckSeverity.Warning,
                "Run schedule validation issue",
                issue.Message,
                targetPath);
        }
    }

    private static IEnumerable<AuthoringCheckResult> XpGemContentResults(
        IReadOnlyList<ContentValidationIssue> issues,
        string targetPath)
    {
        if (issues == null || issues.Count == 0)
        {
            yield return new AuthoringCheckResult(
                "content.xp-gems.ok",
                "Content",
                AuthoringCheckSeverity.Info,
                "XP gem catalog is valid",
                "XP gem definitions have valid IDs, display names, values, and tag metadata.",
                targetPath);
            yield break;
        }

        for (int i = 0; i < issues.Count; i++)
        {
            ContentValidationIssue issue = issues[i];
            yield return new AuthoringCheckResult(
                $"content.xp-gem.{issue.Code}.{i}",
                "Content",
                issue.Severity == ContentValidationSeverity.Error ? AuthoringCheckSeverity.Error : AuthoringCheckSeverity.Warning,
                "XP gem catalog validation issue",
                issue.Message,
                targetPath);
        }
    }

    private static IEnumerable<AuthoringCheckResult> ItemContentResults(
        IReadOnlyList<ContentValidationIssue> issues,
        string targetPath)
    {
        if (issues == null || issues.Count == 0)
        {
            yield return new AuthoringCheckResult(
                "content.items.ok",
                "Content",
                AuthoringCheckSeverity.Info,
                "Item catalog is valid",
                "Item definitions have valid IDs, display names, stacking rules, and stat modifier metadata.",
                targetPath);
            yield break;
        }

        for (int i = 0; i < issues.Count; i++)
        {
            ContentValidationIssue issue = issues[i];
            yield return new AuthoringCheckResult(
                $"content.item.{issue.Code}.{i}",
                "Content",
                issue.Severity == ContentValidationSeverity.Error ? AuthoringCheckSeverity.Error : AuthoringCheckSeverity.Warning,
                "Item catalog validation issue",
                issue.Message,
                targetPath);
        }
    }

    private static IEnumerable<AuthoringCheckResult> TicketDepositContentResults(
        IReadOnlyList<ContentValidationIssue> issues,
        string targetPath)
    {
        if (issues == null || issues.Count == 0)
        {
            yield return new AuthoringCheckResult(
                "content.ticket-deposits.ok",
                "Content",
                AuthoringCheckSeverity.Info,
                "Ticket deposit catalog is valid",
                "Ticket deposits have valid IDs, display names, hold times, payout ranges, and prompt metadata.",
                targetPath);
            yield break;
        }

        for (int i = 0; i < issues.Count; i++)
        {
            ContentValidationIssue issue = issues[i];
            yield return new AuthoringCheckResult(
                $"content.ticket-deposit.{issue.Code}.{i}",
                "Content",
                issue.Severity == ContentValidationSeverity.Error ? AuthoringCheckSeverity.Error : AuthoringCheckSeverity.Warning,
                "Ticket deposit catalog validation issue",
                issue.Message,
                targetPath);
        }
    }

    private static IEnumerable<AuthoringCheckResult> HealthPickupContentResults(
        IReadOnlyList<ContentValidationIssue> issues,
        string targetPath)
    {
        if (issues == null || issues.Count == 0)
        {
            yield return new AuthoringCheckResult(
                "content.health-pickups.ok",
                "Content",
                AuthoringCheckSeverity.Info,
                "Health pickup catalog is valid",
                "Health pickups have valid IDs, display names, healing amounts, and visual metadata.",
                targetPath);
            yield break;
        }

        for (int i = 0; i < issues.Count; i++)
        {
            ContentValidationIssue issue = issues[i];
            yield return new AuthoringCheckResult(
                $"content.health-pickup.{issue.Code}.{i}",
                "Content",
                issue.Severity == ContentValidationSeverity.Error ? AuthoringCheckSeverity.Error : AuthoringCheckSeverity.Warning,
                "Health pickup catalog validation issue",
                issue.Message,
                targetPath);
        }
    }

    private static IEnumerable<AuthoringCheckResult> HealingPropContentResults(
        IReadOnlyList<ContentValidationIssue> issues,
        string targetPath)
    {
        if (issues == null || issues.Count == 0)
        {
            yield return new AuthoringCheckResult(
                "content.healing-props.ok",
                "Content",
                AuthoringCheckSeverity.Info,
                "Healing prop catalog is valid",
                "Healing props have valid IDs, display names, hold times, pickup rewards, and prompt metadata.",
                targetPath);
            yield break;
        }

        for (int i = 0; i < issues.Count; i++)
        {
            ContentValidationIssue issue = issues[i];
            yield return new AuthoringCheckResult(
                $"content.healing-prop.{issue.Code}.{i}",
                "Content",
                issue.Severity == ContentValidationSeverity.Error ? AuthoringCheckSeverity.Error : AuthoringCheckSeverity.Warning,
                "Healing prop catalog validation issue",
                issue.Message,
                targetPath);
        }
    }

    private static IEnumerable<AuthoringCheckResult> WorldRewardPlacementContentResults(
        IReadOnlyList<ContentValidationIssue> issues,
        string targetPath)
    {
        if (issues == null || issues.Count == 0)
        {
            yield return new AuthoringCheckResult(
                "content.world-reward-placements.ok",
                "Content",
                AuthoringCheckSeverity.Info,
                "World reward placement catalog is valid",
                "Reward placements have valid IDs, target content IDs, kinds, and counts.",
                targetPath);
            yield break;
        }

        for (int i = 0; i < issues.Count; i++)
        {
            ContentValidationIssue issue = issues[i];
            yield return new AuthoringCheckResult(
                $"content.world-reward-placement.{issue.Code}.{i}",
                "Content",
                issue.Severity == ContentValidationSeverity.Error ? AuthoringCheckSeverity.Error : AuthoringCheckSeverity.Warning,
                "World reward placement catalog validation issue",
                issue.Message,
                targetPath);
        }
    }

    private static IEnumerable<AuthoringCheckResult> ChestContentResults(
        IReadOnlyList<ContentValidationIssue> issues,
        string targetPath)
    {
        if (issues == null || issues.Count == 0)
        {
            yield return new AuthoringCheckResult(
                "content.chests.ok",
                "Content",
                AuthoringCheckSeverity.Info,
                "Chest catalog is valid",
                "Chests have valid IDs, display names, hold times, Ticket costs, reward rarity tables, and prompt metadata.",
                targetPath);
            yield break;
        }

        for (int i = 0; i < issues.Count; i++)
        {
            ContentValidationIssue issue = issues[i];
            yield return new AuthoringCheckResult(
                $"content.chest.{issue.Code}.{i}",
                "Content",
                issue.Severity == ContentValidationSeverity.Error ? AuthoringCheckSeverity.Error : AuthoringCheckSeverity.Warning,
                "Chest catalog validation issue",
                issue.Message,
                targetPath);
        }
    }

    private static IEnumerable<AuthoringCheckResult> TalentContentResults(
        IReadOnlyList<ContentValidationIssue> issues,
        string targetPath)
    {
        if (issues == null || issues.Count == 0)
        {
            yield return new AuthoringCheckResult(
                "content.talents.ok",
                "Content",
                AuthoringCheckSeverity.Info,
                "Talent catalog is valid",
                "Talent definitions have valid IDs, display names, repeat policy, and stat modifier payloads.",
                targetPath);
            yield break;
        }

        for (int i = 0; i < issues.Count; i++)
        {
            ContentValidationIssue issue = issues[i];
            yield return new AuthoringCheckResult(
                $"content.talent.{issue.Code}.{i}",
                "Content",
                issue.Severity == ContentValidationSeverity.Error ? AuthoringCheckSeverity.Error : AuthoringCheckSeverity.Warning,
                "Talent catalog validation issue",
                issue.Message,
                targetPath);
        }
    }

    private static IEnumerable<AuthoringCheckResult> CharacterContentResults(
        IReadOnlyList<ContentValidationIssue> issues,
        string targetPath)
    {
        if (issues == null || issues.Count == 0)
        {
            yield return new AuthoringCheckResult(
                "content.performers.ok",
                "Content",
                AuthoringCheckSeverity.Info,
                "Performer catalog is valid",
                "Performer definitions have valid IDs, display names, starting weapons, passives, and talent pools.",
                targetPath);
            yield break;
        }

        for (int i = 0; i < issues.Count; i++)
        {
            ContentValidationIssue issue = issues[i];
            yield return new AuthoringCheckResult(
                $"content.performer.{issue.Code}.{i}",
                "Content",
                issue.Severity == ContentValidationSeverity.Error ? AuthoringCheckSeverity.Error : AuthoringCheckSeverity.Warning,
                "Performer catalog validation issue",
                issue.Message,
                targetPath);
        }
    }

    private static IEnumerable<AuthoringCheckResult> EnemyContentResults(
        IReadOnlyList<ContentValidationIssue> issues,
        string targetPath)
    {
        if (issues == null || issues.Count == 0)
        {
            yield return new AuthoringCheckResult(
                "content.enemies.ok",
                "Content",
                AuthoringCheckSeverity.Info,
                "Enemy catalog is valid",
                "Enemy definitions have valid IDs, display names, stats, hitboxes, climbing, stack policy, and drop metadata.",
                targetPath);
            yield break;
        }

        for (int i = 0; i < issues.Count; i++)
        {
            ContentValidationIssue issue = issues[i];
            yield return new AuthoringCheckResult(
                $"content.enemy.{issue.Code}.{i}",
                "Content",
                issue.Severity == ContentValidationSeverity.Error ? AuthoringCheckSeverity.Error : AuthoringCheckSeverity.Warning,
                "Enemy catalog validation issue",
                issue.Message,
                targetPath);
        }
    }

    private static IEnumerable<AuthoringCheckResult> HeadlinerContentResults(
        IReadOnlyList<ContentValidationIssue> issues,
        string targetPath)
    {
        if (issues == null || issues.Count == 0)
        {
            yield return new AuthoringCheckResult(
                "content.headliners.ok",
                "Content",
                AuthoringCheckSeverity.Info,
                "Headliner catalog is valid",
                "Headliner definitions have valid IDs, Act targets, enemy actors, and spawn placement.",
                targetPath);
            yield break;
        }

        for (int i = 0; i < issues.Count; i++)
        {
            ContentValidationIssue issue = issues[i];
            yield return new AuthoringCheckResult(
                $"content.headliner.{issue.Code}.{i}",
                "Content",
                issue.Severity == ContentValidationSeverity.Error ? AuthoringCheckSeverity.Error : AuthoringCheckSeverity.Warning,
                "Headliner catalog validation issue",
                issue.Message,
                targetPath);
        }
    }

    private static IEnumerable<AuthoringCheckResult> UpgradeContentResults(
        IReadOnlyList<ContentValidationIssue> issues,
        string targetPath)
    {
        if (issues == null || issues.Count == 0)
        {
            yield return new AuthoringCheckResult(
                "content.upgrades.ok",
                "Content",
                AuthoringCheckSeverity.Info,
                "Upgrade catalog is valid",
                "Upgrade definitions have valid IDs, display names, levels, rarity weights, and effect payloads.",
                targetPath);
            yield break;
        }

        for (int i = 0; i < issues.Count; i++)
        {
            ContentValidationIssue issue = issues[i];
            yield return new AuthoringCheckResult(
                $"content.upgrade.{issue.Code}.{i}",
                "Content",
                issue.Severity == ContentValidationSeverity.Error ? AuthoringCheckSeverity.Error : AuthoringCheckSeverity.Warning,
                "Upgrade catalog validation issue",
                issue.Message,
                targetPath);
        }
    }

    private static void EvaluateLayerSetup(
        List<AuthoringCheckResult> results,
        Func<string, int> layerLookup,
        Func<int, int, bool> ignoreCollisionLookup)
    {
        int playerLayer = layerLookup(GameLayers.Player);
        int enemyLayer = layerLookup(GameLayers.Enemy);
        int projectileLayer = layerLookup(GameLayers.Projectile);
        int pickupLayer = layerLookup(GameLayers.Pickup);

        AddLayerMissingIfNeeded(results, GameLayers.Player, playerLayer);
        AddLayerMissingIfNeeded(results, GameLayers.Enemy, enemyLayer);
        AddLayerMissingIfNeeded(results, GameLayers.Projectile, projectileLayer);
        AddLayerMissingIfNeeded(results, GameLayers.Pickup, pickupLayer);

        if (playerLayer >= 0 && projectileLayer >= 0 && !ignoreCollisionLookup(playerLayer, projectileLayer))
        {
            results.Add(new AuthoringCheckResult(
                "layers.player-projectile-collision",
                "Layers",
                AuthoringCheckSeverity.Error,
                "Player and Projectile collide",
                "Player and Projectile layers should ignore collision so player-owned shots never block the player.",
                "ProjectSettings/TagManager.asset"));
        }
    }

    private static void AddLayerMissingIfNeeded(List<AuthoringCheckResult> results, string layerName, int layerIndex)
    {
        if (layerIndex >= 0)
        {
            return;
        }

        results.Add(new AuthoringCheckResult(
            $"layers.missing.{layerName}",
            "Layers",
            AuthoringCheckSeverity.Error,
            "Missing gameplay layer",
            $"Required gameplay layer '{layerName}' is not configured.",
            "ProjectSettings/TagManager.asset"));
    }

    private static bool ContainsOldCodename(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        for (int i = 0; i < OldCodenameNeedles.Length; i++)
        {
            if (text.IndexOf(OldCodenameNeedles[i], StringComparison.Ordinal) >= 0)
            {
                return true;
            }
        }

        return false;
    }

    private static int SeverityRank(AuthoringCheckSeverity severity)
    {
        return severity switch
        {
            AuthoringCheckSeverity.Error => 0,
            AuthoringCheckSeverity.Warning => 1,
            _ => 2
        };
    }

    private static string SanitizeId(string value)
    {
        char[] chars = value.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            if (!char.IsLetterOrDigit(chars[i]))
            {
                chars[i] = '.';
            }
        }

        return new string(chars).Trim('.');
    }

}
