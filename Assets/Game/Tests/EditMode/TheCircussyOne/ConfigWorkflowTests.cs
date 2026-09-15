using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Visuals;

public sealed class ConfigWorkflowTests
{
    [Test]
    public void ConfigWorkflowCatalogClassifiesTuningAndGeneratedWorkflows()
    {
        Assert.That(ConfigWorkflowCatalog.GameConfig.Kind, Is.EqualTo(ConfigWorkflowKind.LiveRuntime));
        Assert.That(ConfigWorkflowCatalog.RunScheduleConfig.Kind, Is.EqualTo(ConfigWorkflowKind.LiveRuntime));
        Assert.That(ConfigWorkflowCatalog.RunWorldGenerationConfig.Kind, Is.EqualTo(ConfigWorkflowKind.LiveRuntime));
        Assert.That(ConfigWorkflowCatalog.WeaponDefinition.Kind, Is.EqualTo(ConfigWorkflowKind.LiveRuntime));
        Assert.That(ConfigWorkflowCatalog.WeaponCatalog.Kind, Is.EqualTo(ConfigWorkflowKind.LiveRuntime));
        Assert.That(ConfigWorkflowCatalog.ItemDefinition.Kind, Is.EqualTo(ConfigWorkflowKind.LiveRuntime));
        Assert.That(ConfigWorkflowCatalog.ItemCatalog.Kind, Is.EqualTo(ConfigWorkflowKind.LiveRuntime));
        Assert.That(ConfigWorkflowCatalog.ChestDefinition.Kind, Is.EqualTo(ConfigWorkflowKind.LiveRuntime));
        Assert.That(ConfigWorkflowCatalog.ChestCatalog.Kind, Is.EqualTo(ConfigWorkflowKind.LiveRuntime));
        Assert.That(ConfigWorkflowCatalog.TicketDepositDefinition.Kind, Is.EqualTo(ConfigWorkflowKind.LiveRuntime));
        Assert.That(ConfigWorkflowCatalog.TicketDepositCatalog.Kind, Is.EqualTo(ConfigWorkflowKind.LiveRuntime));
        Assert.That(ConfigWorkflowCatalog.WorldRewardPlacementCatalog.Kind, Is.EqualTo(ConfigWorkflowKind.LiveRuntime));
        Assert.That(ConfigWorkflowCatalog.XpGemDefinition.Kind, Is.EqualTo(ConfigWorkflowKind.LiveRuntime));
        Assert.That(ConfigWorkflowCatalog.XpGemCatalog.Kind, Is.EqualTo(ConfigWorkflowKind.LiveRuntime));
        Assert.That(ConfigWorkflowCatalog.HealthPickupDefinition.Kind, Is.EqualTo(ConfigWorkflowKind.LiveRuntime));
        Assert.That(ConfigWorkflowCatalog.HealthPickupCatalog.Kind, Is.EqualTo(ConfigWorkflowKind.LiveRuntime));
        Assert.That(ConfigWorkflowCatalog.HealingPropDefinition.Kind, Is.EqualTo(ConfigWorkflowKind.LiveRuntime));
        Assert.That(ConfigWorkflowCatalog.HealingPropCatalog.Kind, Is.EqualTo(ConfigWorkflowKind.LiveRuntime));
        Assert.That(ConfigWorkflowCatalog.UpgradeDefinition.Kind, Is.EqualTo(ConfigWorkflowKind.LiveRuntime));
        Assert.That(ConfigWorkflowCatalog.UpgradeCatalog.Kind, Is.EqualTo(ConfigWorkflowKind.LiveRuntime));
        Assert.That(ConfigWorkflowCatalog.TalentDefinition.Kind, Is.EqualTo(ConfigWorkflowKind.LiveRuntime));
        Assert.That(ConfigWorkflowCatalog.TalentCatalog.Kind, Is.EqualTo(ConfigWorkflowKind.LiveRuntime));
        Assert.That(ConfigWorkflowCatalog.PerformerDefinition.Kind, Is.EqualTo(ConfigWorkflowKind.LiveRuntime));
        Assert.That(ConfigWorkflowCatalog.PerformerCatalog.Kind, Is.EqualTo(ConfigWorkflowKind.LiveRuntime));
        Assert.That(ConfigWorkflowCatalog.CameraConfig.Kind, Is.EqualTo(ConfigWorkflowKind.LiveRuntime));
        Assert.That(ConfigWorkflowCatalog.ActorMotionVisualConfig.Kind, Is.EqualTo(ConfigWorkflowKind.LiveRuntime));
        Assert.That(ConfigWorkflowCatalog.GridVisualConfig.Kind, Is.EqualTo(ConfigWorkflowKind.ProjectedVisual));
        Assert.That(ConfigWorkflowCatalog.LightingVisualConfig.Kind, Is.EqualTo(ConfigWorkflowKind.ProjectedVisual));
        Assert.That(ConfigWorkflowCatalog.DamageFeedbackVisualConfig.Kind, Is.EqualTo(ConfigWorkflowKind.ProjectedVisual));
        Assert.That(ConfigWorkflowCatalog.EnemySpawnVisualConfig.Kind, Is.EqualTo(ConfigWorkflowKind.ProjectedVisual));
        Assert.That(ConfigWorkflowCatalog.VfxVisualConfig.Kind, Is.EqualTo(ConfigWorkflowKind.ProjectedVisual));
        Assert.That(ConfigWorkflowCatalog.HudVisualConfig.Kind, Is.EqualTo(ConfigWorkflowKind.ProjectedVisual));
        Assert.That(ConfigWorkflowCatalog.XpGainCounterVisualConfig.Kind, Is.EqualTo(ConfigWorkflowKind.ProjectedVisual));
        Assert.That(ConfigWorkflowCatalog.StructuralScene.Kind, Is.EqualTo(ConfigWorkflowKind.StructuralReset));
    }

    [Test]
    public void GameConfigEnemySpeedIsRuntimeTuningNotSceneRebuild()
    {
        ConfigWorkflowDescriptor workflow = ConfigWorkflowCatalog.GameConfig;

        Assert.That(workflow.Kind, Is.EqualTo(ConfigWorkflowKind.LiveRuntime));
        Assert.That(workflow.Detail, Does.Contain("Enemy speed"));
        Assert.That(workflow.Summary, Does.Contain("No scene rebuild"));
    }

    [Test]
    public void WeaponContentIsRuntimeTuningNotSceneRebuild()
    {
        var weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
        var runSchedule = ScriptableObject.CreateInstance<RunScheduleConfig>();
        var worldGeneration = ScriptableObject.CreateInstance<RunWorldGenerationConfig>();
        var catalog = ScriptableObject.CreateInstance<WeaponCatalog>();
        var item = ScriptableObject.CreateInstance<ItemDefinition>();
        var itemCatalog = ScriptableObject.CreateInstance<ItemCatalog>();
        var chest = ScriptableObject.CreateInstance<ChestDefinition>();
        var chestCatalog = ScriptableObject.CreateInstance<ChestCatalog>();
        var ticketDeposit = ScriptableObject.CreateInstance<TicketDepositDefinition>();
        var ticketDepositCatalog = ScriptableObject.CreateInstance<TicketDepositCatalog>();
        var worldRewardPlacementCatalog = ScriptableObject.CreateInstance<WorldRewardPlacementCatalog>();
        var gem = ScriptableObject.CreateInstance<XpGemDefinition>();
        var gemCatalog = ScriptableObject.CreateInstance<XpGemCatalog>();
        var healthPickup = ScriptableObject.CreateInstance<HealthPickupDefinition>();
        var healthPickupCatalog = ScriptableObject.CreateInstance<HealthPickupCatalog>();
        var healingProp = ScriptableObject.CreateInstance<HealingPropDefinition>();
        var healingPropCatalog = ScriptableObject.CreateInstance<HealingPropCatalog>();
        var upgrade = ScriptableObject.CreateInstance<UpgradeDefinition>();
        var upgradeCatalog = ScriptableObject.CreateInstance<UpgradeCatalog>();
        var talent = ScriptableObject.CreateInstance<TalentDefinition>();
        var talentCatalog = ScriptableObject.CreateInstance<TalentCatalog>();
        var character = ScriptableObject.CreateInstance<PerformerDefinition>();
        var performerCatalog = ScriptableObject.CreateInstance<PerformerCatalog>();

        Assert.That(ConfigWorkflowCatalog.ForObject(weapon).Kind, Is.EqualTo(ConfigWorkflowKind.LiveRuntime));
        Assert.That(ConfigWorkflowCatalog.ForObject(runSchedule).Kind, Is.EqualTo(ConfigWorkflowKind.LiveRuntime));
        Assert.That(ConfigWorkflowCatalog.ForObject(worldGeneration).Kind, Is.EqualTo(ConfigWorkflowKind.LiveRuntime));
        Assert.That(ConfigWorkflowCatalog.ForObject(catalog).Kind, Is.EqualTo(ConfigWorkflowKind.LiveRuntime));
        Assert.That(ConfigWorkflowCatalog.ForObject(item).Kind, Is.EqualTo(ConfigWorkflowKind.LiveRuntime));
        Assert.That(ConfigWorkflowCatalog.ForObject(itemCatalog).Kind, Is.EqualTo(ConfigWorkflowKind.LiveRuntime));
        Assert.That(ConfigWorkflowCatalog.ForObject(chest).Kind, Is.EqualTo(ConfigWorkflowKind.LiveRuntime));
        Assert.That(ConfigWorkflowCatalog.ForObject(chestCatalog).Kind, Is.EqualTo(ConfigWorkflowKind.LiveRuntime));
        Assert.That(ConfigWorkflowCatalog.ForObject(ticketDeposit).Kind, Is.EqualTo(ConfigWorkflowKind.LiveRuntime));
        Assert.That(ConfigWorkflowCatalog.ForObject(ticketDepositCatalog).Kind, Is.EqualTo(ConfigWorkflowKind.LiveRuntime));
        Assert.That(ConfigWorkflowCatalog.ForObject(worldRewardPlacementCatalog).Kind, Is.EqualTo(ConfigWorkflowKind.LiveRuntime));
        Assert.That(ConfigWorkflowCatalog.ForObject(gem).Kind, Is.EqualTo(ConfigWorkflowKind.LiveRuntime));
        Assert.That(ConfigWorkflowCatalog.ForObject(gemCatalog).Kind, Is.EqualTo(ConfigWorkflowKind.LiveRuntime));
        Assert.That(ConfigWorkflowCatalog.ForObject(healthPickup).Kind, Is.EqualTo(ConfigWorkflowKind.LiveRuntime));
        Assert.That(ConfigWorkflowCatalog.ForObject(healthPickupCatalog).Kind, Is.EqualTo(ConfigWorkflowKind.LiveRuntime));
        Assert.That(ConfigWorkflowCatalog.ForObject(healingProp).Kind, Is.EqualTo(ConfigWorkflowKind.LiveRuntime));
        Assert.That(ConfigWorkflowCatalog.ForObject(healingPropCatalog).Kind, Is.EqualTo(ConfigWorkflowKind.LiveRuntime));
        Assert.That(ConfigWorkflowCatalog.ForObject(upgrade).Kind, Is.EqualTo(ConfigWorkflowKind.LiveRuntime));
        Assert.That(ConfigWorkflowCatalog.ForObject(upgradeCatalog).Kind, Is.EqualTo(ConfigWorkflowKind.LiveRuntime));
        Assert.That(ConfigWorkflowCatalog.ForObject(talent).Kind, Is.EqualTo(ConfigWorkflowKind.LiveRuntime));
        Assert.That(ConfigWorkflowCatalog.ForObject(talentCatalog).Kind, Is.EqualTo(ConfigWorkflowKind.LiveRuntime));
        Assert.That(ConfigWorkflowCatalog.ForObject(character).Kind, Is.EqualTo(ConfigWorkflowKind.LiveRuntime));
        Assert.That(ConfigWorkflowCatalog.ForObject(performerCatalog).Kind, Is.EqualTo(ConfigWorkflowKind.LiveRuntime));
        Assert.That(ConfigWorkflowCatalog.WeaponDefinition.Summary, Does.Contain("No scene rebuild"));
        Assert.That(ConfigWorkflowCatalog.RunScheduleConfig.Detail, Does.Contain("Showtime"));
        Assert.That(ConfigWorkflowCatalog.RunWorldGenerationConfig.Detail, Does.Contain("heightfield"));
        Assert.That(ConfigWorkflowCatalog.WeaponCatalog.Detail, Does.Contain("run starts"));
        Assert.That(ConfigWorkflowCatalog.ItemDefinition.Summary, Does.Contain("No scene rebuild"));
        Assert.That(ConfigWorkflowCatalog.ItemCatalog.Detail, Does.Contain("world reward"));
        Assert.That(ConfigWorkflowCatalog.ChestDefinition.Summary, Does.Contain("No scene rebuild"));
        Assert.That(ConfigWorkflowCatalog.ChestCatalog.Detail, Does.Contain("world reward"));
        Assert.That(ConfigWorkflowCatalog.TicketDepositDefinition.Summary, Does.Contain("No scene rebuild"));
        Assert.That(ConfigWorkflowCatalog.TicketDepositCatalog.Detail, Does.Contain("interaction"));
        Assert.That(ConfigWorkflowCatalog.WorldRewardPlacementCatalog.Detail, Does.Contain("placement"));
        Assert.That(ConfigWorkflowCatalog.XpGemDefinition.Summary, Does.Contain("No scene rebuild"));
        Assert.That(ConfigWorkflowCatalog.XpGemCatalog.Detail, Does.Contain("drops spawn"));
        Assert.That(ConfigWorkflowCatalog.HealthPickupDefinition.Summary, Does.Contain("No scene rebuild"));
        Assert.That(ConfigWorkflowCatalog.HealthPickupCatalog.Detail, Does.Contain("healing"));
        Assert.That(ConfigWorkflowCatalog.HealingPropDefinition.Summary, Does.Contain("No scene rebuild"));
        Assert.That(ConfigWorkflowCatalog.HealingPropCatalog.Detail, Does.Contain("interaction"));
        Assert.That(ConfigWorkflowCatalog.UpgradeDefinition.Summary, Does.Contain("No scene rebuild"));
        Assert.That(ConfigWorkflowCatalog.UpgradeCatalog.Detail, Does.Contain("choices roll"));
        Assert.That(ConfigWorkflowCatalog.TalentDefinition.Summary, Does.Contain("No scene rebuild"));
        Assert.That(ConfigWorkflowCatalog.TalentCatalog.Detail, Does.Contain("choices roll"));
        Assert.That(ConfigWorkflowCatalog.PerformerDefinition.Summary, Does.Contain("No scene rebuild"));
        Assert.That(ConfigWorkflowCatalog.PerformerCatalog.Detail, Does.Contain("run start"));

        UnityEngine.Object.DestroyImmediate(weapon);
        UnityEngine.Object.DestroyImmediate(runSchedule);
        UnityEngine.Object.DestroyImmediate(worldGeneration);
        UnityEngine.Object.DestroyImmediate(catalog);
        UnityEngine.Object.DestroyImmediate(item);
        UnityEngine.Object.DestroyImmediate(itemCatalog);
        UnityEngine.Object.DestroyImmediate(chest);
        UnityEngine.Object.DestroyImmediate(chestCatalog);
        UnityEngine.Object.DestroyImmediate(ticketDeposit);
        UnityEngine.Object.DestroyImmediate(ticketDepositCatalog);
        UnityEngine.Object.DestroyImmediate(worldRewardPlacementCatalog);
        UnityEngine.Object.DestroyImmediate(gem);
        UnityEngine.Object.DestroyImmediate(gemCatalog);
        UnityEngine.Object.DestroyImmediate(healthPickup);
        UnityEngine.Object.DestroyImmediate(healthPickupCatalog);
        UnityEngine.Object.DestroyImmediate(healingProp);
        UnityEngine.Object.DestroyImmediate(healingPropCatalog);
        UnityEngine.Object.DestroyImmediate(upgrade);
        UnityEngine.Object.DestroyImmediate(upgradeCatalog);
        UnityEngine.Object.DestroyImmediate(talent);
        UnityEngine.Object.DestroyImmediate(talentCatalog);
        UnityEngine.Object.DestroyImmediate(character);
        UnityEngine.Object.DestroyImmediate(performerCatalog);
    }

    [Test]
    public void LightingApplierUpdatesOpenSceneStateFromConfig()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var config = ScriptableObject.CreateInstance<LightingVisualConfig>();
        var profile = ScriptableObject.CreateInstance<VolumeProfile>();
        var camera = new GameObject("Test Camera").AddComponent<Camera>();

        config.cameraBackgroundColor = new Color(0.12f, 0.18f, 0.24f, 1f);
        config.keyLightColor = new Color(0.9f, 0.74f, 0.42f, 1f);
        config.keyLightIntensity = 2.5f;
        config.keyLightShadowStrength = 0.62f;
        config.keyLightShadowBias = 0.07f;
        config.keyLightShadowNormalBias = 0.41f;
        config.keyLightEuler = new Vector3(48f, -27f, 3f);
        config.ambientMode = AmbientMode.Trilight;
        config.ambientSkyColor = new Color(0.2f, 0.22f, 0.3f, 1f);
        config.ambientEquatorColor = new Color(0.08f, 0.09f, 0.12f, 1f);
        config.ambientGroundColor = new Color(0.02f, 0.025f, 0.035f, 1f);
        config.ambientIntensity = 0.37f;
        config.reflectionIntensity = 0.19f;
        config.fogEnabled = true;
        config.fogMode = FogMode.ExponentialSquared;
        config.fogColor = new Color(0.03f, 0.04f, 0.05f, 1f);
        config.fogDensity = 0.006f;
        config.postProcessingEnabled = true;

        TheCircussyOneLightingVisualApplier.ApplyToOpenScene(config, profile);

        Light light = UnityEngine.Object.FindFirstObjectByType<Light>();
        Volume volume = UnityEngine.Object.FindFirstObjectByType<Volume>();
        var cameraData = camera.GetComponent<UniversalAdditionalCameraData>();

        Assert.That(RenderSettings.ambientMode, Is.EqualTo(config.ambientMode));
        Assert.That(RenderSettings.ambientSkyColor, Is.EqualTo(config.ambientSkyColor));
        Assert.That(RenderSettings.ambientEquatorColor, Is.EqualTo(config.ambientEquatorColor));
        Assert.That(RenderSettings.ambientGroundColor, Is.EqualTo(config.ambientGroundColor));
        Assert.That(RenderSettings.ambientIntensity, Is.EqualTo(config.ambientIntensity));
        Assert.That(RenderSettings.reflectionIntensity, Is.EqualTo(config.reflectionIntensity));
        Assert.That(RenderSettings.fog, Is.EqualTo(config.fogEnabled));
        Assert.That(RenderSettings.fogMode, Is.EqualTo(config.fogMode));
        Assert.That(RenderSettings.fogColor, Is.EqualTo(config.fogColor));
        Assert.That(RenderSettings.fogDensity, Is.EqualTo(config.fogDensity));
        Assert.That(light, Is.Not.Null);
        Assert.That(light.color, Is.EqualTo(config.keyLightColor));
        Assert.That(light.intensity, Is.EqualTo(config.keyLightIntensity));
        Assert.That(light.shadowStrength, Is.EqualTo(config.keyLightShadowStrength));
        Assert.That(light.shadowBias, Is.EqualTo(config.keyLightShadowBias));
        Assert.That(light.shadowNormalBias, Is.EqualTo(config.keyLightShadowNormalBias));
        Assert.That(camera.backgroundColor, Is.EqualTo(config.cameraBackgroundColor));
        Assert.That(cameraData, Is.Not.Null);
        Assert.That(cameraData.renderPostProcessing, Is.True);
        Assert.That(cameraData.renderShadows, Is.True);
        Assert.That(volume, Is.Not.Null);
        Assert.That(volume.sharedProfile, Is.SameAs(profile));

        UnityEngine.Object.DestroyImmediate(config);
        UnityEngine.Object.DestroyImmediate(profile);
    }

    [Test]
    public void HudApplierSetsPhysicalPanelScalingFromConfig()
    {
        var panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
        var config = ScriptableObject.CreateInstance<HudVisualConfig>();
        config.scaleMode = PanelScaleMode.ConstantPhysicalSize;
        config.referenceDpi = 110f;
        config.fallbackDpi = 92f;
        config.referenceResolution = new Vector2(1440f, 900f);
        config.globalScale = 1.2f;
        panelSettings.targetTexture = new RenderTexture(16, 16, 0);
        RenderTexture staleTarget = panelSettings.targetTexture;

        TheCircussyOneHudVisualApplier.ApplyPanelSettings(panelSettings, config);

        Assert.That(panelSettings.scaleMode, Is.EqualTo(PanelScaleMode.ConstantPhysicalSize));
        Assert.That(panelSettings.referenceDpi, Is.EqualTo(110f));
        Assert.That(panelSettings.fallbackDpi, Is.EqualTo(92f));
        Assert.That(panelSettings.referenceResolution, Is.EqualTo(new Vector2Int(1440, 900)));
        Assert.That(panelSettings.targetTexture, Is.Null);
        Assert.That(panelSettings.scale, Is.EqualTo(1f));

        UnityEngine.Object.DestroyImmediate(staleTarget);
        UnityEngine.Object.DestroyImmediate(panelSettings);
        UnityEngine.Object.DestroyImmediate(config);
    }

    [Test]
    public void HudPreviewPreservesVisualTestOverrideConfig()
    {
        const string overridePath = "Assets/Game/VisualTests/ConfigOverrides/__HudPreviewOverrideTest.asset";
        AssetDatabase.DeleteAsset(overridePath);
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var canonical = ScriptableObject.CreateInstance<HudVisualConfig>();
        canonical.previewPreset = HudPreviewPreset.Normal;
        var overrideConfig = ScriptableObject.CreateInstance<HudVisualConfig>();
        overrideConfig.previewPreset = HudPreviewPreset.GameOver;
        AssetDatabase.CreateAsset(overrideConfig, overridePath);

        var hudObject = new GameObject("HUD Preview Override Test");
        HudView hud = hudObject.AddComponent<HudView>();
        var healthFill = new VisualElement();
        var xpFill = new VisualElement();
        var healthLabel = new Label();
        var levelLabel = new Label();
        var killsLabel = new Label();
        var timerLabel = new Label();
        var gameOver = new VisualElement();
        SetField(hud, "hudConfig", overrideConfig);
        SetField(hud, "healthFill", healthFill);
        SetField(hud, "experienceFill", xpFill);
        SetField(hud, "healthLabel", healthLabel);
        SetField(hud, "levelLabel", levelLabel);
        SetField(hud, "killsLabel", killsLabel);
        SetField(hud, "timerLabel", timerLabel);
        SetField(hud, "gameOver", gameOver);

        try
        {
            TheCircussyOneHudVisualApplier.PreviewOpenScene(canonical);

            Assert.That(GetField<HudVisualConfig>(hud, "hudConfig"), Is.SameAs(overrideConfig));
            Assert.That(healthLabel.text, Is.EqualTo("0 / 100"));
            Assert.That(levelLabel.text, Is.EqualTo("5"));
            Assert.That(killsLabel.text, Is.EqualTo("103"));
            Assert.That(timerLabel.text, Is.EqualTo("10:32"));
            Assert.That(gameOver.style.display.value, Is.EqualTo(DisplayStyle.Flex));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(canonical);
            UnityEngine.Object.DestroyImmediate(hudObject);
            AssetDatabase.DeleteAsset(overridePath);
        }
    }

    [Test]
    public void VfxApplierUpdatesOpenScenePlayerContinuousVfx()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameConfig gameConfig = TheCircussyOneTestObjects.CreateConfig();
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(gameConfig);
        AddParticleChild(player.transform, "Player Move Dust");
        AddParticleChild(player.transform, "Player Jump Trail");
        var config = ScriptableObject.CreateInstance<VfxVisualConfig>();
        config.playerMoveDustLocalOffset = new Vector3(0f, 0.08f, -0.32f);
        config.playerJumpTrailLocalOffset = new Vector3(0f, 0.12f, -0.08f);

        TheCircussyOneVfxVisualApplier.ApplyToOpenScene(config);

        Assert.That(player.MoveDust.transform.localPosition, Is.EqualTo(config.playerMoveDustLocalOffset));
        Assert.That(player.JumpTrail.transform.localPosition, Is.EqualTo(config.playerJumpTrailLocalOffset));

        UnityEngine.Object.DestroyImmediate(config);
        UnityEngine.Object.DestroyImmediate(gameConfig);
    }

    [Test]
    public void RebuildLightingPathCreatesConfiguredLightAndVolumeWithoutSecondApply()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var config = ScriptableObject.CreateInstance<LightingVisualConfig>();
        var profile = ScriptableObject.CreateInstance<VolumeProfile>();
        config.keyLightIntensity = 1.7f;
        config.ambientIntensity = 0.31f;

        TheCircussyOneLightingVisualApplier.ApplyToNewScene(config, profile);

        Light light = UnityEngine.Object.FindFirstObjectByType<Light>();
        Volume volume = UnityEngine.Object.FindFirstObjectByType<Volume>();

        Assert.That(light, Is.Not.Null);
        Assert.That(light.intensity, Is.EqualTo(config.keyLightIntensity));
        Assert.That(RenderSettings.ambientIntensity, Is.EqualTo(config.ambientIntensity));
        Assert.That(volume, Is.Not.Null);
        Assert.That(volume.sharedProfile, Is.SameAs(profile));

        UnityEngine.Object.DestroyImmediate(config);
        UnityEngine.Object.DestroyImmediate(profile);
    }

    private static void AddParticleChild(Transform parent, string name)
    {
        var child = new GameObject(name);
        child.transform.SetParent(parent, false);
        child.AddComponent<ParticleSystem>();
    }

    private static void SetField(object target, string fieldName, object value)
    {
        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null, fieldName);
        field.SetValue(target, value);
    }

    private static T GetField<T>(object target, string fieldName)
    {
        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null, fieldName);
        return (T)field.GetValue(target);
    }
}
