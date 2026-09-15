using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.DI;
using TheCircussyOne.Runtime;
using TheCircussyOne.Visuals;

public sealed class ArchitectureGuardrailTests
{
    [TearDown]
    public void TearDown()
    {
        TheCircussyOneTestObjects.DestroyAll();
    }

    [Test]
    public void RuntimeUnityEditorReferencesAreExplicitlyGuarded()
    {
        foreach (string path in RuntimeSourceFiles())
        {
            string source = File.ReadAllText(path);
            if (!source.Contains("UnityEditor"))
            {
                continue;
            }

            Assert.That(source, Does.Contain("#if UNITY_EDITOR"), $"{path} references UnityEditor without an explicit editor-only guard.");
        }
    }

    [Test]
    public void RuntimeSystemsDoNotInstantiateOrDestroyDirectlyOutsideFactories()
    {
        string runtimeRoot = Path.Combine(ProjectRoot, "Assets/Game/Scripts/TheCircussyOne/Runtime");
        string[] forbiddenPatterns =
        {
            "Object.Instantiate(",
            "UnityEngine.Object.Instantiate(",
            "Object.Destroy(",
            "UnityEngine.Object.Destroy(",
            "DestroyImmediate("
        };

        foreach (string path in Directory.GetFiles(runtimeRoot, "*.cs", SearchOption.AllDirectories))
        {
            string fileName = Path.GetFileNameWithoutExtension(path);
            if (fileName.EndsWith("Factory") || fileName == "OrbitWeaponSystem" || fileName == "ProjectileExplosionSystem")
            {
                continue;
            }

            string source = File.ReadAllText(path);
            foreach (string pattern in forbiddenPatterns)
            {
                Assert.That(source, Does.Not.Contain(pattern), $"{path} should use a factory/editor builder instead of {pattern}.");
            }
        }
    }

    [Test]
    public void ActorFixturesAndGeneratedEnemyPrefabIncludeActorBodyView()
    {
        GameConfig config = TheCircussyOneTestObjects.CreateConfig();
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(config);
        EnemyView enemy = TheCircussyOneTestObjects.CreateEnemyPrefab();
        GameObject enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/Prefabs/Enemy.prefab");

        Assert.That(player.GetComponent<ActorBodyView>(), Is.Not.Null);
        Assert.That(enemy.GetComponent<ActorBodyView>(), Is.Not.Null);
        Assert.That(enemyPrefab, Is.Not.Null);
        Assert.That(enemyPrefab.GetComponent<ActorBodyView>(), Is.Not.Null);

        TheCircussyOneTestObjects.Destroy(config);
    }

    [Test]
    public void SceneBuilderDelegatesConfigCreationToRepository()
    {
        string source = File.ReadAllText(Path.Combine(ProjectRoot, "Assets/Game/Editor/TheCircussyOneSceneBuilder.cs"));
        string[] repositoryCalls =
        {
            "TheCircussyOneConfigRepository.GetOrCreateGameConfig()",
            "TheCircussyOneConfigRepository.GetOrCreateWeaponCatalog()",
            "TheCircussyOneConfigRepository.GetOrCreateXpGemCatalog()",
            "TheCircussyOneConfigRepository.GetOrCreateUpgradeCatalog()",
            "TheCircussyOneConfigRepository.GetOrCreateCameraConfig()",
            "TheCircussyOneConfigRepository.GetOrCreateGridVisualConfig()",
            "TheCircussyOneConfigRepository.GetOrCreateLightingVisualConfig()",
            "TheCircussyOneConfigRepository.GetOrCreateDamageFeedbackVisualConfig()",
            "TheCircussyOneConfigRepository.GetOrCreateActorMotionVisualConfig()",
            "TheCircussyOneConfigRepository.GetOrCreateEnemySpawnVisualConfig()",
            "TheCircussyOneConfigRepository.GetOrCreateVfxVisualConfig()",
            "TheCircussyOneConfigRepository.GetOrCreateHudVisualConfig()",
            "TheCircussyOneConfigRepository.GetOrCreateXpGainCounterVisualConfig()"
        };

        foreach (string call in repositoryCalls)
        {
            Assert.That(source, Does.Contain(call));
        }

        Assert.That(source, Does.Not.Contain("AssetDatabase.CreateAsset(config, GameConfigPath)"));
        Assert.That(source, Does.Not.Contain("config.playerMoveSpeed ="));
        Assert.That(source, Does.Not.Contain("config.enemyMoveSpeed ="));
    }

    [Test]
    public void PrototypeSceneSerializesRunWorldGenerationConfig()
    {
        string scene = File.ReadAllText(Path.Combine(ProjectRoot, TheCircussyOneAssetPaths.ScenePath));
        string worldGenerationConfigGuid = AssetDatabase.AssetPathToGUID(TheCircussyOneAssetPaths.RunWorldGenerationConfigPath);

        Assert.That(worldGenerationConfigGuid, Is.Not.Empty);
        Assert.That(scene, Does.Contain("runWorldGenerationConfig:"));
        Assert.That(scene, Does.Contain($"runWorldGenerationConfig: {{fileID: 11400000, guid: {worldGenerationConfigGuid}, type: 2}}"));
    }

    [Test]
    public void ConfigRepositoryDelegatesWeaponStarterContentToFocusedModule()
    {
        string repository = File.ReadAllText(Path.Combine(ProjectRoot, "Assets/Game/Editor/TheCircussyOneConfigRepository.cs"));
        string weaponStarterContent = File.ReadAllText(Path.Combine(ProjectRoot, "Assets/Game/Editor/TheCircussyOneWeaponStarterContent.cs"));

        Assert.That(repository, Does.Contain("TheCircussyOneWeaponStarterContent.GetOrCreateJugglingBallWeapon()"));
        Assert.That(repository, Does.Contain("TheCircussyOneWeaponStarterContent.GetOrCreateWeaponCatalog()"));
        Assert.That(repository, Does.Contain("TheCircussyOneWeaponStarterContent.GetOrCreateUpgradeCatalog("));
        Assert.That(repository, Does.Not.Contain("private static WeaponDefinition GetOrCreateWeapon("));
        Assert.That(repository, Does.Not.Contain("private static UpgradeDefinition GetOrCreateWeaponTrackUpgrade("));
        Assert.That(weaponStarterContent, Does.Contain("WeaponTrackUpgradeSpec"));
        Assert.That(weaponStarterContent, Does.Contain("FirstPartyWeaponDefaults"));
    }

    [Test]
    public void GameLifetimeScopeDelegatesRuntimeFallbackDefaultsToFocusedModule()
    {
        string lifetimeScope = RuntimeLifetimeScopeSource();
        string defaults = File.ReadAllText(Path.Combine(ProjectRoot, "Assets/Game/Scripts/TheCircussyOne/DI/GameRuntimeDefaults.cs"));
        string catalogFactory = File.ReadAllText(Path.Combine(ProjectRoot, "Assets/Game/Scripts/TheCircussyOne/DI/GameFallbackCatalogFactory.cs"));

        Assert.That(lifetimeScope, Does.Contain("private GameRuntimeDefaults runtimeDefaults;"));
        Assert.That(lifetimeScope, Does.Contain("runtimeDefaults.ResolveWeaponCatalog"));
        Assert.That(lifetimeScope, Does.Contain("runtimeDefaults.ResolvePerformerCatalog"));
        Assert.That(lifetimeScope, Does.Contain("runtimeDefaults.ResolveWorldInteractionPromptPrefab"));
        Assert.That(lifetimeScope, Does.Contain("DestroyOwnedWorldInteractionPromptObjects"));
        Assert.That(lifetimeScope, Does.Not.Contain("public sealed class GameRuntimeDefaults"));
        Assert.That(lifetimeScope, Does.Not.Contain("CreateFallbackWeaponCatalog"));
        Assert.That(lifetimeScope, Does.Not.Contain("CreateRuntimeWorldInteractionPromptPrefab"));

        Assert.That(defaults, Does.Contain("public sealed class GameRuntimeDefaults"));
        Assert.That(defaults, Does.Contain("private readonly GameFallbackCatalogFactory fallbackCatalogs;"));
        Assert.That(defaults, Does.Contain("fallbackCatalogs.CreateWeaponCatalog"));
        Assert.That(defaults, Does.Contain("fallbackCatalogs.CreatePerformerCatalog"));
        Assert.That(defaults, Does.Not.Contain("private WeaponCatalog CreateWeaponCatalog"));
        Assert.That(defaults, Does.Not.Contain("private PerformerCatalog CreatePerformerCatalog"));
        Assert.That(defaults, Does.Contain("CreateRuntimeWorldInteractionPromptPrefab"));

        Assert.That(catalogFactory, Does.Contain("internal sealed class GameFallbackCatalogFactory"));
        Assert.That(catalogFactory, Does.Contain("CreateWeaponCatalog"));
        Assert.That(catalogFactory, Does.Contain("CreatePerformerCatalog"));
        Assert.That(catalogFactory, Does.Contain("CreateTicketDepositCatalog"));
        Assert.That(catalogFactory, Does.Contain("FirstPartyWeaponDefaults"));
    }

    [Test]
    public void GameRuntimeDefaultsCreatesFallbackCatalogsWithCurrentDefaults()
    {
        GameConfig config = TheCircussyOneTestObjects.CreateConfig();
        var defaults = new GameRuntimeDefaults(config, null);
        WeaponCatalog weapons = null;
        ItemCatalog items = null;
        ChestCatalog chests = null;
        TicketDepositCatalog deposits = null;
        XpGemCatalog gems = null;
        UpgradeCatalog upgrades = null;
        TalentCatalog talents = null;
        PerformerCatalog performers = null;
        EnemyCatalog enemies = null;

        try
        {
            weapons = defaults.ResolveWeaponCatalog(null);
            items = defaults.ResolveItemCatalog(null);
            chests = defaults.ResolveChestCatalog(null);
            deposits = defaults.ResolveTicketDepositCatalog(null);
            gems = defaults.ResolveXpGemCatalog(null);
            upgrades = defaults.ResolveUpgradeCatalog(null);
            talents = defaults.ResolveTalentCatalog(null);
            performers = defaults.ResolvePerformerCatalog(null, weapons);
            enemies = defaults.ResolveEnemyCatalog(null, gems);

            Assert.That(weapons.StartingWeapons, Has.Count.EqualTo(1));
            Assert.That(weapons.AvailableWeapons, Has.Count.EqualTo(1));
            Assert.That(items.Items, Has.Count.EqualTo(1));
            Assert.That(chests.Chests, Has.Count.EqualTo(4));
            Assert.That(deposits.Deposits, Has.Count.EqualTo(3));
            Assert.That(gems.Gems, Has.Count.EqualTo(1));
            Assert.That(upgrades.Upgrades, Has.Count.EqualTo(2));
            Assert.That(talents.Talents, Is.Not.Empty);
            Assert.That(performers.Performers, Is.Not.Empty);
            Assert.That(enemies.Enemies, Has.Count.EqualTo(1));
        }
        finally
        {
            DestroyFallbackCatalogs(weapons, items, chests, deposits, gems, upgrades, talents, performers, enemies);
            TheCircussyOneTestObjects.Destroy(config);
        }
    }

    [Test]
    public void GameRuntimeDefaultsReleasesOwnedWorldPromptFallbackObjects()
    {
        WorldInteractionPromptVisualConfig config = TheCircussyOneTestObjects.CreateWorldInteractionPromptConfig();
        var defaults = new GameRuntimeDefaults(null, null);
        Transform root = defaults.ResolveWorldInteractionPromptRoot(null);
        WorldInteractionPromptView prefab = defaults.ResolveWorldInteractionPromptPrefab(null, config);

        Assert.That(root, Is.Not.Null);
        Assert.That(prefab, Is.Not.Null);

        defaults.DestroyOwnedWorldInteractionPromptObjects(ref root, ref prefab);

        Assert.That(root, Is.Null);
        Assert.That(prefab, Is.Null);
        TheCircussyOneTestObjects.Destroy(config);
    }

    [TestCase(VfxEffectId.ProjectileBounceBurst, VfxParticleTextureKind.SparkDiamond)]
    [TestCase(VfxEffectId.KnifeHitSparks, VfxParticleTextureKind.SlashSpark)]
    public void RuntimeParticleFallbacksUseSupportedShapedMaterials(
        VfxEffectId effectId,
        VfxParticleTextureKind expectedTextureKind)
    {
        var defaults = new GameRuntimeDefaults(null, null);
        try
        {
            ParticleEffectView prefab = defaults.ResolveParticleEffectPrefab(null, effectId, $"{effectId} Test Prefab");
            var renderer = prefab.GetComponent<ParticleSystemRenderer>();

            Assert.That(renderer, Is.Not.Null);
            Assert.That(VfxParticleMaterialFactory.IsUsable(renderer.sharedMaterial), Is.True);
            Assert.That(renderer.sharedMaterial.mainTexture, Is.Not.Null);
            Assert.That(renderer.sharedMaterial.mainTexture.name, Does.Contain(expectedTextureKind.ToString()));
        }
        finally
        {
            defaults.DestroyOwnedParticleEffectPrefabs();
        }
    }

    [Test]
    public void GameLifetimeScopeDelegatesGameLoopOrderingToFocusedModule()
    {
        string lifetimeScope = File.ReadAllText(Path.Combine(ProjectRoot, "Assets/Game/Scripts/TheCircussyOne/DI/GameLifetimeScope.cs"));
        string loopComposition = File.ReadAllText(Path.Combine(ProjectRoot, "Assets/Game/Scripts/TheCircussyOne/DI/GameLoopComposition.cs"));

        Assert.That(lifetimeScope, Does.Contain("GameLoopComposition.ConstructRunner"));
        Assert.That(lifetimeScope, Does.Not.Contain("new IStartable[]"));
        Assert.That(lifetimeScope, Does.Not.Contain("new ITickable[]"));
        Assert.That(lifetimeScope, Does.Not.Contain("new ILateTickable[]"));

        Assert.That(loopComposition, Does.Contain("container.Resolve<GameBootstrapper>()"));
        Assert.That(loopComposition, Does.Contain("container.Resolve<PlayerMovementSystem>()"));
        Assert.That(loopComposition, Does.Contain("container.Resolve<ProjectileSystem>()"));
        Assert.That(loopComposition, Does.Contain("Array.Empty<IFixedTickable>()"));
        Assert.That(loopComposition, Does.Contain("container.Resolve<CameraFollowSystem>()"));
    }

    [Test]
    public void EditorConfigProtectsUnitySerializedYamlFromAutoTrim()
    {
        string source = File.ReadAllText(Path.Combine(ProjectRoot, ".editorconfig"));

        Assert.That(source, Does.Contain("[*.{unity,prefab,asset,meta,inputactions}]"));
        Assert.That(source, Does.Contain("trim_trailing_whitespace = false"));
    }

    [Test]
    public void PrototypeSceneDoesNotSerializeLegacyShapesLineHealthBarMaterials()
    {
        string scenePath = Path.Combine(ProjectRoot, "Assets/Game/Scenes/TheCircussyOne.unity");
        string scene = File.ReadAllText(scenePath);

        Assert.That(scene, Does.Not.Contain("Line 2D Transparent (CAP_ROUND) (instance)"));
        Assert.That(scene, Does.Not.Contain("ShapesRuntime::Shapes.Line"));
    }

    [Test]
    public void MigratedRuntimeValuesRouteThroughRunStats()
    {
        string lifetimeScope = RuntimeLifetimeScopeSource();
        string gameState = File.ReadAllText(Path.Combine(ProjectRoot, "Assets/Game/Scripts/TheCircussyOne/Runtime/Core/GameState.cs"));
        string movement = File.ReadAllText(Path.Combine(ProjectRoot, "Assets/Game/Scripts/TheCircussyOne/Runtime/Player/PlayerMovementSystem.cs"));
        string pickups = File.ReadAllText(Path.Combine(ProjectRoot, "Assets/Game/Scripts/TheCircussyOne/Runtime/Pickups/PickupSystem.cs"));
        string weapons = File.ReadAllText(Path.Combine(ProjectRoot, "Assets/Game/Scripts/TheCircussyOne/Runtime/Combat/AutoWeaponSystem.cs"));
        string projectiles = File.ReadAllText(Path.Combine(ProjectRoot, "Assets/Game/Scripts/TheCircussyOne/Runtime/Combat/ProjectileFactory.cs"));

        Assert.That(lifetimeScope, Does.Contain("new RunStats(gameConfig)"));
        Assert.That(lifetimeScope, Does.Contain("RegisterInstance(context.RunStats)"));
        Assert.That(gameState, Does.Contain("StatId.PlayerMaxHealth"));
        Assert.That(movement, Does.Contain("StatId.PlayerMoveSpeedMultiplier"));
        Assert.That(pickups, Does.Contain("StatId.PickupCollectRadius"));
        Assert.That(pickups, Does.Contain("StatId.PickupMagnetRadius"));
        Assert.That(weapons, Does.Contain("WeaponStatRules.FireInterval"));
        Assert.That(projectiles, Does.Contain("WeaponStatRules.Damage"));
        Assert.That(projectiles, Does.Contain("WeaponStatRules.ProjectileSpeed"));
    }

    [Test]
    public void UpgradeSelectionSystemDelegatesChoiceAndEffectDetails()
    {
        string source = File.ReadAllText(Path.Combine(ProjectRoot, "Assets/Game/Scripts/TheCircussyOne/Runtime/Upgrades/UpgradeSelectionSystem.cs"));

        Assert.That(source, Does.Contain("UpgradeChoiceProvider"));
        Assert.That(source, Does.Contain("UpgradeEffectApplier"));
        Assert.That(source, Does.Not.Contain("UpgradeCatalog"));
        Assert.That(source, Does.Not.Contain("RunStats"));
        Assert.That(source, Does.Not.Contain("WeaponLoadout"));
    }

    [Test]
    public void EnemyClimbSystemDependenciesAreRegisteredInLifetimeScope()
    {
        string source = RuntimeLifetimeScopeSource();

        Assert.That(source, Does.Contain("new WorldPhysicsQuery()"));
        Assert.That(source, Does.Contain("RegisterInstance(context.WorldPhysicsQuery)"));
        Assert.That(source, Does.Contain("new EnemyEnvironmentSurfaceSampler(context.WorldPhysicsQuery)"));
        Assert.That(source, Does.Contain("RegisterInstance(context.EnemyEnvironmentSurfaceSampler)"));
        Assert.That(source, Does.Contain("Register<EnemyClimbSystem>"));
    }

    [Test]
    public void EnemyDirectorSystemDependenciesAreRegisteredInLifetimeScope()
    {
        string source = RuntimeLifetimeScopeSource();

        Assert.That(source, Does.Contain("new WorldPhysicsQuery()"));
        Assert.That(source, Does.Contain("RegisterInstance(context.WorldPhysicsQuery)"));
        Assert.That(source, Does.Contain("new EnemyEnvironmentSurfaceSampler(context.WorldPhysicsQuery)"));
        Assert.That(source, Does.Contain("RegisterInstance(context.EnemyEnvironmentSurfaceSampler)"));
        Assert.That(source, Does.Contain("new EnemySupportSurfaceSampler()"));
        Assert.That(source, Does.Contain("RegisterInstance(context.EnemySupportSurfaceSampler)"));
        Assert.That(source, Does.Contain("Register<EnemyDirectorSystem>"));
    }

    [Test]
    public void AutoWeaponSystemDependenciesAreRegisteredInLifetimeScope()
    {
        string source = RuntimeLifetimeScopeSource();

        Assert.That(source, Does.Contain("new WorldPhysicsQuery()"));
        Assert.That(source, Does.Contain("RegisterInstance(context.WorldPhysicsQuery)"));
        Assert.That(source, Does.Contain("new WeaponTargetingQuery(context.WorldPhysicsQuery)"));
        Assert.That(source, Does.Contain("RegisterInstance(context.WeaponTargetingQuery)"));
        Assert.That(source, Does.Contain("Register<AutoWeaponSystem>"));
    }

    [Test]
    public void GameplayPhysicsQueriesRouteThroughWorldPhysicsQuery()
    {
        string runtimeRoot = Path.Combine(ProjectRoot, "Assets/Game/Scripts/TheCircussyOne/Runtime");
        string worldQueryPath = Path.Combine(runtimeRoot, "Core/WorldPhysicsQuery.cs");
        string cameraPath = Path.Combine(runtimeRoot, "Camera/CameraFollowSystem.cs");
        string[] forbiddenPatterns =
        {
            "Physics.RaycastNonAlloc(",
            "Physics.SphereCastNonAlloc(",
            "Physics.CapsuleCastNonAlloc(",
            "Physics.OverlapSphereNonAlloc(",
            "Physics.ComputePenetration("
        };

        foreach (string path in Directory.GetFiles(runtimeRoot, "*.cs", SearchOption.AllDirectories))
        {
            if (Path.GetFullPath(path) == Path.GetFullPath(worldQueryPath)
                || Path.GetFullPath(path) == Path.GetFullPath(cameraPath))
            {
                continue;
            }

            string source = File.ReadAllText(path);
            foreach (string pattern in forbiddenPatterns)
            {
                Assert.That(source, Does.Not.Contain(pattern), $"{path} should route gameplay physics queries through WorldPhysicsQuery.");
            }
        }
    }

    [Test]
    public void InteractionSystemDependenciesAreRegisteredInLifetimeScope()
    {
        string source = RuntimeLifetimeScopeSource();

        Assert.That(source, Does.Contain("new SceneInteractableSource()"));
        Assert.That(source, Does.Contain("RegisterInstance(context.InteractableSource)"));
        Assert.That(source, Does.Contain("Register<InteractionSystem>"));
    }

    [Test]
    public void InteractInputUsesControllerWestButton()
    {
        string inputService = File.ReadAllText(Path.Combine(ProjectRoot, "Assets/Game/Scripts/TheCircussyOne/Runtime/Input/UnityInputService.cs"));
        string inputActions = File.ReadAllText(Path.Combine(ProjectRoot, "Assets/Game/Scripts/InputSystem/InputSystem_Actions.inputactions"));

        Assert.That(inputService, Does.Contain("Gamepad.current.buttonWest.wasPressedThisFrame"));
        Assert.That(inputService, Does.Contain("Gamepad.current.buttonWest.isPressed"));
        Assert.That(inputService, Does.Contain("Gamepad.current.buttonWest.wasReleasedThisFrame"));
        Assert.That(inputActions, Does.Contain("\"path\": \"<Gamepad>/buttonWest\""));
        Assert.That(inputActions, Does.Contain("\"action\": \"Interact\""));
    }

    [Test]
    public void PrototypeArenaUsesGroundedMeshTerrain()
    {
        string source = File.ReadAllText(Path.Combine(ProjectRoot, "Assets/Game/Editor/TheCircussyOneSceneBuilder.Prefabs.cs"));

        Assert.That(source, Does.Contain("Arena Terrain"));
        Assert.That(source, Does.Contain("BuildSolidMesh"));
        Assert.That(source, Does.Contain("MeshCollider"));
        Assert.That(source, Does.Contain("North Terrace South Ramp"));
        Assert.That(source, Does.Contain("East Overlook West Ramp"));
    }

    [Test]
    public void VisualTweenCompletionCallbacksDoNotErrorWhenSceneRestartDestroysTargets()
    {
        string visualsRoot = Path.Combine(ProjectRoot, "Assets/Game/Scripts/TheCircussyOne/Visuals");
        foreach (string path in Directory.GetFiles(visualsRoot, "*.cs", SearchOption.AllDirectories))
        {
            foreach (string line in File.ReadLines(path))
            {
                if (!line.Contains(".OnComplete("))
                {
                    continue;
                }

                Assert.That(
                    line,
                    Does.Contain("warnIfTargetDestroyed: false"),
                    $"{path} has a visual PrimeTween completion callback that can log errors if restart destroys the scene mid-tween.");
            }
        }
    }

    private static string[] RuntimeSourceFiles()
    {
        string runtimeRoot = Path.Combine(ProjectRoot, "Assets/Game/Scripts/TheCircussyOne");
        return Directory.GetFiles(runtimeRoot, "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}Editor{Path.DirectorySeparatorChar}"))
            .ToArray();
    }

    private static void DestroyFallbackCatalogs(params UnityEngine.Object[] catalogs)
    {
        foreach (UnityEngine.Object catalog in catalogs)
        {
            DestroyCatalogContents(catalog);
        }

        foreach (UnityEngine.Object catalog in catalogs)
        {
            TheCircussyOneTestObjects.Destroy(catalog);
        }
    }

    private static void DestroyCatalogContents(UnityEngine.Object catalog)
    {
        switch (catalog)
        {
            case WeaponCatalog weapons:
                foreach (WeaponDefinition weapon in weapons.StartingWeapons.Concat(weapons.AvailableWeapons).Where(weapon => weapon != null).Distinct())
                {
                    TheCircussyOneTestObjects.Destroy(weapon);
                }

                break;
            case ItemCatalog items:
                foreach (ItemDefinition item in items.Items)
                {
                    TheCircussyOneTestObjects.Destroy(item);
                }

                break;
            case ChestCatalog chests:
                foreach (ChestDefinition chest in chests.Chests)
                {
                    TheCircussyOneTestObjects.Destroy(chest);
                }

                break;
            case TicketDepositCatalog deposits:
                foreach (TicketDepositDefinition deposit in deposits.Deposits)
                {
                    TheCircussyOneTestObjects.Destroy(deposit);
                }

                break;
            case XpGemCatalog gems:
                foreach (XpGemDefinition gem in gems.Gems)
                {
                    TheCircussyOneTestObjects.Destroy(gem);
                }

                break;
            case UpgradeCatalog upgrades:
                foreach (UpgradeDefinition upgrade in upgrades.Upgrades)
                {
                    TheCircussyOneTestObjects.Destroy(upgrade?.weaponDefinition);
                    TheCircussyOneTestObjects.Destroy(upgrade);
                }

                break;
            case TalentCatalog talents:
                foreach (TalentDefinition talent in talents.Talents)
                {
                    TheCircussyOneTestObjects.Destroy(talent);
                }

                break;
            case PerformerCatalog performers:
                foreach (PerformerDefinition performer in performers.Performers)
                {
                    TheCircussyOneTestObjects.Destroy(performer);
                }

                break;
            case EnemyCatalog enemies:
                foreach (EnemyDefinition enemy in enemies.Enemies)
                {
                    TheCircussyOneTestObjects.Destroy(enemy);
                }

                break;
        }
    }

    private static string RuntimeLifetimeScopeSource()
    {
        string diRoot = Path.Combine(ProjectRoot, "Assets/Game/Scripts/TheCircussyOne/DI");
        return string.Join(
            "\n",
            Directory.GetFiles(diRoot, "GameLifetimeScope*.cs")
                .OrderBy(Path.GetFileName)
                .Select(File.ReadAllText));
    }

    private static string ProjectRoot => Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
}
