using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;
using TheCircussyOne.Runtime;
using UnityEngine;
using UnityEngine.Rendering;

namespace TheCircussyOne.Visuals
{
    public sealed class BigTopEnvironmentView : MonoBehaviour
    {
        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private MeshFilter godRayMeshFilter;
        [SerializeField] private MeshRenderer godRayMeshRenderer;
        [SerializeField] private MeshFilter perimeterFogGroundMeshFilter;
        [SerializeField] private MeshRenderer perimeterFogGroundMeshRenderer;
        [SerializeField] private MeshFilter perimeterFogCurtainMeshFilter;
        [SerializeField] private MeshRenderer perimeterFogCurtainMeshRenderer;
        [SerializeField] private MeshFilter bulbMeshFilter;
        [SerializeField] private MeshRenderer bulbMeshRenderer;
        [SerializeField] private MeshFilter dressingRopeMeshFilter;
        [SerializeField] private MeshRenderer dressingRopeMeshRenderer;
        [SerializeField] private MeshFilter dressingPoleMeshFilter;
        [SerializeField] private MeshRenderer dressingPoleMeshRenderer;
        [SerializeField] private MeshFilter dressingValanceMeshFilter;
        [SerializeField] private MeshRenderer dressingValanceMeshRenderer;
        [SerializeField] private MeshFilter buntingStringMeshFilter;
        [SerializeField] private MeshRenderer buntingStringMeshRenderer;
        [SerializeField] private MeshFilter buntingPrimaryFlagMeshFilter;
        [SerializeField] private MeshRenderer buntingPrimaryFlagMeshRenderer;
        [SerializeField] private MeshFilter buntingSecondaryFlagMeshFilter;
        [SerializeField] private MeshRenderer buntingSecondaryFlagMeshRenderer;
        [SerializeField] private MeshFilter curtainBannerPanelMeshFilter;
        [SerializeField] private MeshRenderer curtainBannerPanelMeshRenderer;
        [SerializeField] private MeshFilter curtainBannerTrimMeshFilter;
        [SerializeField] private MeshRenderer curtainBannerTrimMeshRenderer;
        [SerializeField] private MeshFilter godRayFixtureMeshFilter;
        [SerializeField] private MeshRenderer godRayFixtureMeshRenderer;
        [SerializeField] private MeshFilter godRayFixtureLensMeshFilter;
        [SerializeField] private MeshRenderer godRayFixtureLensMeshRenderer;

        private Mesh runtimeMesh;
        private Material runtimeMaterial;
        private Texture2D runtimeTexture;
        private Mesh runtimeGodRayMesh;
        private Material runtimeGodRayMaterial;
        private Texture2D runtimeGodRayTexture;
        private Mesh runtimePerimeterFogGroundMesh;
        private Material runtimePerimeterFogGroundMaterial;
        private Texture2D runtimePerimeterFogGroundTexture;
        private Mesh runtimePerimeterFogCurtainMesh;
        private Material runtimePerimeterFogCurtainMaterial;
        private Texture2D runtimePerimeterFogCurtainTexture;
        private Mesh runtimeBulbMesh;
        private Material runtimeBulbMaterial;
        private Mesh runtimeDressingRopeMesh;
        private Material runtimeDressingRopeMaterial;
        private Mesh runtimeDressingPoleMesh;
        private Material runtimeDressingPoleMaterial;
        private Mesh runtimeDressingValanceMesh;
        private Material runtimeDressingValanceMaterial;
        private Texture2D runtimeDressingValanceTexture;
        private Mesh runtimeBuntingStringMesh;
        private Material runtimeBuntingStringMaterial;
        private Mesh runtimeBuntingPrimaryFlagMesh;
        private Material runtimeBuntingPrimaryFlagMaterial;
        private Mesh runtimeBuntingSecondaryFlagMesh;
        private Material runtimeBuntingSecondaryFlagMaterial;
        private Mesh runtimeCurtainBannerPanelMesh;
        private Material runtimeCurtainBannerPanelMaterial;
        private Texture2D runtimeCurtainBannerPanelTexture;
        private Mesh runtimeCurtainBannerTrimMesh;
        private Material runtimeCurtainBannerTrimMaterial;
        private Texture2D runtimeCurtainBannerTrimTexture;
        private Mesh runtimeGodRayFixtureMesh;
        private Material runtimeGodRayFixtureMaterial;
        private Mesh runtimeGodRayFixtureLensMesh;
        private Material runtimeGodRayFixtureLensMaterial;
        private BigTopEnvironmentSpec configuredSpec;
        private bool hasConfiguredSpec;
        private float lastShowLightMeshRefreshTime = float.NegativeInfinity;

        public float Radius { get; private set; }
        public float TotalHeight { get; private set; }
        public Mesh RuntimeMesh => runtimeMesh;
        public Texture2D RuntimeTexture => runtimeTexture;
        public Material RuntimeMaterial => runtimeMaterial;
        public Mesh RuntimeGodRayMesh => runtimeGodRayMesh;
        public Material RuntimeGodRayMaterial => runtimeGodRayMaterial;
        public Mesh RuntimePerimeterFogGroundMesh => runtimePerimeterFogGroundMesh;
        public Material RuntimePerimeterFogGroundMaterial => runtimePerimeterFogGroundMaterial;
        public Texture2D RuntimePerimeterFogGroundTexture => runtimePerimeterFogGroundTexture;
        public Mesh RuntimePerimeterFogCurtainMesh => runtimePerimeterFogCurtainMesh;
        public Material RuntimePerimeterFogCurtainMaterial => runtimePerimeterFogCurtainMaterial;
        public Texture2D RuntimePerimeterFogCurtainTexture => runtimePerimeterFogCurtainTexture;
        public Mesh RuntimeBulbMesh => runtimeBulbMesh;
        public Material RuntimeBulbMaterial => runtimeBulbMaterial;
        public Mesh RuntimeDressingRopeMesh => runtimeDressingRopeMesh;
        public Material RuntimeDressingRopeMaterial => runtimeDressingRopeMaterial;
        public Mesh RuntimeDressingPoleMesh => runtimeDressingPoleMesh;
        public Material RuntimeDressingPoleMaterial => runtimeDressingPoleMaterial;
        public Mesh RuntimeDressingValanceMesh => runtimeDressingValanceMesh;
        public Material RuntimeDressingValanceMaterial => runtimeDressingValanceMaterial;
        public Mesh RuntimeBuntingStringMesh => runtimeBuntingStringMesh;
        public Material RuntimeBuntingStringMaterial => runtimeBuntingStringMaterial;
        public Mesh RuntimeBuntingPrimaryFlagMesh => runtimeBuntingPrimaryFlagMesh;
        public Material RuntimeBuntingPrimaryFlagMaterial => runtimeBuntingPrimaryFlagMaterial;
        public Mesh RuntimeBuntingSecondaryFlagMesh => runtimeBuntingSecondaryFlagMesh;
        public Material RuntimeBuntingSecondaryFlagMaterial => runtimeBuntingSecondaryFlagMaterial;
        public Mesh RuntimeCurtainBannerPanelMesh => runtimeCurtainBannerPanelMesh;
        public Material RuntimeCurtainBannerPanelMaterial => runtimeCurtainBannerPanelMaterial;
        public Mesh RuntimeCurtainBannerTrimMesh => runtimeCurtainBannerTrimMesh;
        public Material RuntimeCurtainBannerTrimMaterial => runtimeCurtainBannerTrimMaterial;
        public Mesh RuntimeGodRayFixtureMesh => runtimeGodRayFixtureMesh;
        public Material RuntimeGodRayFixtureMaterial => runtimeGodRayFixtureMaterial;
        public Mesh RuntimeGodRayFixtureLensMesh => runtimeGodRayFixtureLensMesh;
        public Material RuntimeGodRayFixtureLensMaterial => runtimeGodRayFixtureLensMaterial;

        public void Configure(BigTopEnvironmentSpec spec, RunWorldGenerationConfig config)
        {
            if (!spec.IsValid)
            {
                Deactivate();
                return;
            }

            EnsureComponents();
            ReleaseRuntimeAssets();
            configuredSpec = spec;
            hasConfiguredSpec = true;
            Radius = spec.Radius;
            TotalHeight = spec.TotalHeight;
            runtimeMesh = BigTopEnvironmentMeshBuilder.Build(spec);
            runtimeTexture = BigTopClothTextureGenerator.Generate(spec, config);
            runtimeMaterial = CreateRuntimeMaterial(runtimeTexture);
            meshFilter.sharedMesh = runtimeMesh;
            meshRenderer.sharedMaterial = runtimeMaterial;
            ConfigureRenderer(meshRenderer);
            ConfigureGodRays(spec, config);
            ConfigurePerimeterFog(spec, config);
            ConfigureBulbs(spec, config);
            ConfigureDressing(spec, config);
            gameObject.SetActive(config == null || config.bigTopEnvironmentEnabled);
        }

        public async UniTask ConfigureAsync(
            BigTopEnvironmentSpec spec,
            RunWorldGenerationConfig config,
            WorldLoadTimingDiagnostics timing = null)
        {
            if (!spec.IsValid)
            {
                Deactivate();
                return;
            }

            timing ??= WorldLoadTimingDiagnostics.Disabled;
            using (timing.Stage("BigTop.Prepare"))
            {
                EnsureComponents();
                ReleaseRuntimeAssets();
                configuredSpec = spec;
                hasConfiguredSpec = true;
                Radius = spec.Radius;
                TotalHeight = spec.TotalHeight;
            }

            await UniTask.Yield(PlayerLoopTiming.Update);
            if (this == null)
            {
                return;
            }

            using (timing.Stage("BigTop.CanopyMesh"))
            {
                runtimeMesh = BigTopEnvironmentMeshBuilder.Build(spec);
            }

            await UniTask.Yield(PlayerLoopTiming.Update);
            if (this == null)
            {
                return;
            }

            using (timing.Stage("BigTop.ClothTexture"))
            {
                runtimeTexture = await BigTopClothTextureGenerator.GenerateAsync(spec, config, timing);
            }

            await UniTask.Yield(PlayerLoopTiming.Update);
            if (this == null)
            {
                return;
            }

            using (timing.Stage("BigTop.CanopyApply"))
            {
                runtimeMaterial = CreateRuntimeMaterial(runtimeTexture);
                meshFilter.sharedMesh = runtimeMesh;
                meshRenderer.sharedMaterial = runtimeMaterial;
                ConfigureRenderer(meshRenderer);
            }

            await UniTask.Yield(PlayerLoopTiming.Update);
            if (this == null)
            {
                return;
            }

            await ConfigureGodRaysAsync(spec, config, timing);
            if (this == null)
            {
                return;
            }

            using (timing.Stage("BigTop.PerimeterFog"))
            {
                ConfigurePerimeterFog(spec, config);
            }

            await UniTask.Yield(PlayerLoopTiming.Update);
            if (this == null)
            {
                return;
            }

            await ConfigureBulbsAsync(spec, config, timing);
            if (this == null)
            {
                return;
            }

            await ConfigureDressingAsync(spec, config, timing);
            if (this == null)
            {
                return;
            }

            using (timing.Stage("BigTop.Activate"))
            {
                gameObject.SetActive(config == null || config.bigTopEnvironmentEnabled);
            }
        }

        public void Tick(float timeSeconds, RunWorldGenerationConfig config)
        {
            if (runtimeGodRayMaterial != null)
            {
                Color color = BigTopVisualColorRules.GodRayColor(config, timeSeconds);
                SetMaterialColor(runtimeGodRayMaterial, color);
            }

            if (runtimeBulbMaterial != null)
            {
                Color color = BigTopVisualColorRules.BulbColor(config, timeSeconds);
                SetMaterialColor(runtimeBulbMaterial, color);
            }

            if (runtimeGodRayFixtureLensMaterial != null)
            {
                Color color = BigTopVisualColorRules.GodRayFixtureLensColor(config, timeSeconds);
                SetMaterialColor(runtimeGodRayFixtureLensMaterial, color);
            }

            UpdateAnimatedShowLights(timeSeconds, config);
        }

        public void Deactivate()
        {
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            ReleaseRuntimeAssets();
        }

        private void EnsureComponents()
        {
            meshFilter = meshFilter != null ? meshFilter : GetComponent<MeshFilter>();
            meshRenderer = meshRenderer != null ? meshRenderer : GetComponent<MeshRenderer>();
            meshFilter = meshFilter != null ? meshFilter : gameObject.AddComponent<MeshFilter>();
            meshRenderer = meshRenderer != null ? meshRenderer : gameObject.AddComponent<MeshRenderer>();
        }

        private void ReleaseRuntimeAssets()
        {
            DestroyRuntimeObject(runtimeMesh);
            DestroyRuntimeObject(runtimeMaterial);
            DestroyRuntimeObject(runtimeTexture);
            DestroyRuntimeObject(runtimeGodRayMesh);
            DestroyRuntimeObject(runtimeGodRayMaterial);
            DestroyRuntimeObject(runtimeGodRayTexture);
            DestroyRuntimeObject(runtimePerimeterFogGroundMesh);
            DestroyRuntimeObject(runtimePerimeterFogGroundMaterial);
            DestroyRuntimeObject(runtimePerimeterFogGroundTexture);
            DestroyRuntimeObject(runtimePerimeterFogCurtainMesh);
            DestroyRuntimeObject(runtimePerimeterFogCurtainMaterial);
            DestroyRuntimeObject(runtimePerimeterFogCurtainTexture);
            DestroyRuntimeObject(runtimeBulbMesh);
            DestroyRuntimeObject(runtimeBulbMaterial);
            DestroyRuntimeObject(runtimeDressingRopeMesh);
            DestroyRuntimeObject(runtimeDressingRopeMaterial);
            DestroyRuntimeObject(runtimeDressingPoleMesh);
            DestroyRuntimeObject(runtimeDressingPoleMaterial);
            DestroyRuntimeObject(runtimeDressingValanceMesh);
            DestroyRuntimeObject(runtimeDressingValanceMaterial);
            DestroyRuntimeObject(runtimeDressingValanceTexture);
            DestroyRuntimeObject(runtimeBuntingStringMesh);
            DestroyRuntimeObject(runtimeBuntingStringMaterial);
            DestroyRuntimeObject(runtimeBuntingPrimaryFlagMesh);
            DestroyRuntimeObject(runtimeBuntingPrimaryFlagMaterial);
            DestroyRuntimeObject(runtimeBuntingSecondaryFlagMesh);
            DestroyRuntimeObject(runtimeBuntingSecondaryFlagMaterial);
            DestroyRuntimeObject(runtimeCurtainBannerPanelMesh);
            DestroyRuntimeObject(runtimeCurtainBannerPanelMaterial);
            DestroyRuntimeObject(runtimeCurtainBannerPanelTexture);
            DestroyRuntimeObject(runtimeCurtainBannerTrimMesh);
            DestroyRuntimeObject(runtimeCurtainBannerTrimMaterial);
            DestroyRuntimeObject(runtimeCurtainBannerTrimTexture);
            DestroyRuntimeObject(runtimeGodRayFixtureMesh);
            DestroyRuntimeObject(runtimeGodRayFixtureMaterial);
            DestroyRuntimeObject(runtimeGodRayFixtureLensMesh);
            DestroyRuntimeObject(runtimeGodRayFixtureLensMaterial);
            runtimeMesh = null;
            runtimeMaterial = null;
            runtimeTexture = null;
            runtimeGodRayMesh = null;
            runtimeGodRayMaterial = null;
            runtimeGodRayTexture = null;
            runtimePerimeterFogGroundMesh = null;
            runtimePerimeterFogGroundMaterial = null;
            runtimePerimeterFogGroundTexture = null;
            runtimePerimeterFogCurtainMesh = null;
            runtimePerimeterFogCurtainMaterial = null;
            runtimePerimeterFogCurtainTexture = null;
            runtimeBulbMesh = null;
            runtimeBulbMaterial = null;
            runtimeDressingRopeMesh = null;
            runtimeDressingRopeMaterial = null;
            runtimeDressingPoleMesh = null;
            runtimeDressingPoleMaterial = null;
            runtimeDressingValanceMesh = null;
            runtimeDressingValanceMaterial = null;
            runtimeDressingValanceTexture = null;
            runtimeBuntingStringMesh = null;
            runtimeBuntingStringMaterial = null;
            runtimeBuntingPrimaryFlagMesh = null;
            runtimeBuntingPrimaryFlagMaterial = null;
            runtimeBuntingSecondaryFlagMesh = null;
            runtimeBuntingSecondaryFlagMaterial = null;
            runtimeCurtainBannerPanelMesh = null;
            runtimeCurtainBannerPanelMaterial = null;
            runtimeCurtainBannerPanelTexture = null;
            runtimeCurtainBannerTrimMesh = null;
            runtimeCurtainBannerTrimMaterial = null;
            runtimeCurtainBannerTrimTexture = null;
            runtimeGodRayFixtureMesh = null;
            runtimeGodRayFixtureMaterial = null;
            runtimeGodRayFixtureLensMesh = null;
            runtimeGodRayFixtureLensMaterial = null;
            configuredSpec = default;
            hasConfiguredSpec = false;
            lastShowLightMeshRefreshTime = float.NegativeInfinity;
            if (godRayMeshFilter != null)
            {
                godRayMeshFilter.sharedMesh = null;
            }

            if (godRayMeshRenderer != null)
            {
                godRayMeshRenderer.sharedMaterial = null;
            }

            if (perimeterFogGroundMeshFilter != null)
            {
                perimeterFogGroundMeshFilter.sharedMesh = null;
            }

            if (perimeterFogGroundMeshRenderer != null)
            {
                perimeterFogGroundMeshRenderer.sharedMaterial = null;
            }

            if (perimeterFogCurtainMeshFilter != null)
            {
                perimeterFogCurtainMeshFilter.sharedMesh = null;
            }

            if (perimeterFogCurtainMeshRenderer != null)
            {
                perimeterFogCurtainMeshRenderer.sharedMaterial = null;
            }

            if (bulbMeshFilter != null)
            {
                bulbMeshFilter.sharedMesh = null;
            }

            if (bulbMeshRenderer != null)
            {
                bulbMeshRenderer.sharedMaterial = null;
            }

            if (dressingRopeMeshFilter != null)
            {
                dressingRopeMeshFilter.sharedMesh = null;
            }

            if (dressingRopeMeshRenderer != null)
            {
                dressingRopeMeshRenderer.sharedMaterial = null;
            }

            if (dressingPoleMeshFilter != null)
            {
                dressingPoleMeshFilter.sharedMesh = null;
            }

            if (dressingPoleMeshRenderer != null)
            {
                dressingPoleMeshRenderer.sharedMaterial = null;
            }

            if (dressingValanceMeshFilter != null)
            {
                dressingValanceMeshFilter.sharedMesh = null;
            }

            if (dressingValanceMeshRenderer != null)
            {
                dressingValanceMeshRenderer.sharedMaterial = null;
            }

            if (buntingStringMeshFilter != null)
            {
                buntingStringMeshFilter.sharedMesh = null;
            }

            if (buntingStringMeshRenderer != null)
            {
                buntingStringMeshRenderer.sharedMaterial = null;
            }

            if (buntingPrimaryFlagMeshFilter != null)
            {
                buntingPrimaryFlagMeshFilter.sharedMesh = null;
            }

            if (buntingPrimaryFlagMeshRenderer != null)
            {
                buntingPrimaryFlagMeshRenderer.sharedMaterial = null;
            }

            if (buntingSecondaryFlagMeshFilter != null)
            {
                buntingSecondaryFlagMeshFilter.sharedMesh = null;
            }

            if (buntingSecondaryFlagMeshRenderer != null)
            {
                buntingSecondaryFlagMeshRenderer.sharedMaterial = null;
            }

            if (curtainBannerPanelMeshFilter != null)
            {
                curtainBannerPanelMeshFilter.sharedMesh = null;
            }

            if (curtainBannerPanelMeshRenderer != null)
            {
                curtainBannerPanelMeshRenderer.sharedMaterial = null;
            }

            if (curtainBannerTrimMeshFilter != null)
            {
                curtainBannerTrimMeshFilter.sharedMesh = null;
            }

            if (curtainBannerTrimMeshRenderer != null)
            {
                curtainBannerTrimMeshRenderer.sharedMaterial = null;
            }

            if (godRayFixtureMeshFilter != null)
            {
                godRayFixtureMeshFilter.sharedMesh = null;
            }

            if (godRayFixtureMeshRenderer != null)
            {
                godRayFixtureMeshRenderer.sharedMaterial = null;
            }

            if (godRayFixtureLensMeshFilter != null)
            {
                godRayFixtureLensMeshFilter.sharedMesh = null;
            }

            if (godRayFixtureLensMeshRenderer != null)
            {
                godRayFixtureLensMeshRenderer.sharedMaterial = null;
            }
        }

        private void ConfigureGodRays(BigTopEnvironmentSpec spec, RunWorldGenerationConfig config)
        {
            EnsureGodRayComponents();
            if (config == null || config.BigTopGodRayCount <= 0 || config.BigTopGodRayOpacity <= 0f)
            {
                godRayMeshFilter.sharedMesh = null;
                godRayMeshRenderer.sharedMaterial = null;
                godRayMeshRenderer.gameObject.SetActive(false);
                EnsureGodRayFixtureComponents();
                ClearDressingRenderer(godRayFixtureMeshFilter, godRayFixtureMeshRenderer);
                ClearDressingRenderer(godRayFixtureLensMeshFilter, godRayFixtureLensMeshRenderer);
                return;
            }

            runtimeGodRayMesh = BigTopGodRayMeshBuilder.Build(spec, config);
            runtimeGodRayTexture = BigTopGodRayTextureGenerator.Generate(config);
            runtimeGodRayMaterial = CreateVertexColorTransparentRuntimeMaterial(
                "Big Top God Ray Runtime Material",
                runtimeGodRayTexture,
                BigTopVisualColorRules.GodRayColor(config, 0f));
            godRayMeshFilter.sharedMesh = runtimeGodRayMesh;
            godRayMeshRenderer.sharedMaterial = runtimeGodRayMaterial;
            ConfigureRenderer(godRayMeshRenderer);
            godRayMeshRenderer.gameObject.SetActive(runtimeGodRayMesh != null);
            ConfigureGodRayFixtures(spec, config);
        }

        private void ConfigureGodRayFixtures(BigTopEnvironmentSpec spec, RunWorldGenerationConfig config)
        {
            EnsureGodRayFixtureComponents();
            if (config == null
                || !config.BigTopGodRayFixturesEnabled
                || config.BigTopGodRayFixtureLength <= 0f
                || config.BigTopGodRayFixtureRadius <= 0f)
            {
                ClearDressingRenderer(godRayFixtureMeshFilter, godRayFixtureMeshRenderer);
                ClearDressingRenderer(godRayFixtureLensMeshFilter, godRayFixtureLensMeshRenderer);
                return;
            }

            runtimeGodRayFixtureMesh = BigTopDressingMeshBuilder.BuildGodRayFixtureBodies(spec, config);
            runtimeGodRayFixtureMaterial = CreateOpaqueRuntimeMaterial(
                "Big Top God Ray Fixture Runtime Material",
                config.bigTopGodRayFixtureColor,
                CullMode.Off);
            ApplyDressingRenderer(
                godRayFixtureMeshFilter,
                godRayFixtureMeshRenderer,
                runtimeGodRayFixtureMesh,
                runtimeGodRayFixtureMaterial);

            runtimeGodRayFixtureLensMesh = BigTopDressingMeshBuilder.BuildGodRayFixtureLenses(spec, config);
            runtimeGodRayFixtureLensMaterial = CreateVertexColorTransparentRuntimeMaterial(
                "Big Top God Ray Fixture Lens Runtime Material",
                null,
                BigTopVisualColorRules.GodRayFixtureLensColor(config, 0f),
                CullMode.Off);
            ApplyDressingRenderer(
                godRayFixtureLensMeshFilter,
                godRayFixtureLensMeshRenderer,
                runtimeGodRayFixtureLensMesh,
                runtimeGodRayFixtureLensMaterial);
        }

        private void UpdateAnimatedShowLights(float timeSeconds, RunWorldGenerationConfig config)
        {
            if (!hasConfiguredSpec
                || config == null
                || !config.BigTopShowLightsEnabled
                || config.BigTopShowLightCount <= 0
                || runtimeGodRayMesh == null)
            {
                return;
            }

            if (Mathf.Abs(timeSeconds - lastShowLightMeshRefreshTime) < config.BigTopShowLightMeshRefreshInterval)
            {
                return;
            }

            lastShowLightMeshRefreshTime = timeSeconds;
            ReplaceRuntimeMesh(
                ref runtimeGodRayMesh,
                BigTopGodRayMeshBuilder.Build(configuredSpec, config, timeSeconds, animateShowLights: true),
                godRayMeshFilter,
                godRayMeshRenderer);

            if (runtimeGodRayFixtureMesh != null)
            {
                ReplaceRuntimeMesh(
                    ref runtimeGodRayFixtureMesh,
                    BigTopDressingMeshBuilder.BuildGodRayFixtureBodies(configuredSpec, config, timeSeconds, animateShowLights: true),
                    godRayFixtureMeshFilter,
                    godRayFixtureMeshRenderer);
            }

            if (runtimeGodRayFixtureLensMesh != null)
            {
                ReplaceRuntimeMesh(
                    ref runtimeGodRayFixtureLensMesh,
                    BigTopDressingMeshBuilder.BuildGodRayFixtureLenses(configuredSpec, config, timeSeconds, animateShowLights: true),
                    godRayFixtureLensMeshFilter,
                    godRayFixtureLensMeshRenderer);
            }
        }

        private void ReplaceRuntimeMesh(
            ref Mesh current,
            Mesh replacement,
            MeshFilter targetMeshFilter,
            MeshRenderer targetMeshRenderer)
        {
            if (targetMeshFilter == null)
            {
                DestroyRuntimeObject(replacement);
                return;
            }

            if (current != null && replacement != null)
            {
                current.Clear();
                current.indexFormat = replacement.indexFormat;
                current.vertices = replacement.vertices;
                current.uv = replacement.uv;
                current.colors = replacement.colors;
                current.triangles = replacement.triangles;
                current.normals = replacement.normals;
                current.bounds = replacement.bounds;
                DestroyRuntimeObject(replacement);
            }
            else
            {
                DestroyRuntimeObject(current);
                current = replacement;
            }

            targetMeshFilter.sharedMesh = current;
            if (targetMeshRenderer != null)
            {
                ConfigureRenderer(targetMeshRenderer);
                targetMeshRenderer.gameObject.SetActive(current != null);
            }
        }

        private async UniTask ConfigureGodRaysAsync(
            BigTopEnvironmentSpec spec,
            RunWorldGenerationConfig config,
            WorldLoadTimingDiagnostics timing)
        {
            EnsureGodRayComponents();
            if (config == null || config.BigTopGodRayCount <= 0 || config.BigTopGodRayOpacity <= 0f)
            {
                godRayMeshFilter.sharedMesh = null;
                godRayMeshRenderer.sharedMaterial = null;
                godRayMeshRenderer.gameObject.SetActive(false);
                EnsureGodRayFixtureComponents();
                ClearDressingRenderer(godRayFixtureMeshFilter, godRayFixtureMeshRenderer);
                ClearDressingRenderer(godRayFixtureLensMeshFilter, godRayFixtureLensMeshRenderer);
                return;
            }

            using (timing.Stage("BigTop.GodRayMesh"))
            {
                runtimeGodRayMesh = BigTopGodRayMeshBuilder.Build(spec, config);
            }

            await UniTask.Yield(PlayerLoopTiming.Update);
            if (this == null)
            {
                return;
            }

            using (timing.Stage("BigTop.GodRayTexture"))
            {
                runtimeGodRayTexture = BigTopGodRayTextureGenerator.Generate(config);
            }

            await UniTask.Yield(PlayerLoopTiming.Update);
            if (this == null)
            {
                return;
            }

            using (timing.Stage("BigTop.GodRayApply"))
            {
                runtimeGodRayMaterial = CreateVertexColorTransparentRuntimeMaterial(
                    "Big Top God Ray Runtime Material",
                    runtimeGodRayTexture,
                    BigTopVisualColorRules.GodRayColor(config, 0f));
                godRayMeshFilter.sharedMesh = runtimeGodRayMesh;
                godRayMeshRenderer.sharedMaterial = runtimeGodRayMaterial;
                ConfigureRenderer(godRayMeshRenderer);
                godRayMeshRenderer.gameObject.SetActive(runtimeGodRayMesh != null);
            }

            await UniTask.Yield(PlayerLoopTiming.Update);
            if (this == null)
            {
                return;
            }

            using (timing.Stage("BigTop.GodRayFixtures"))
            {
                ConfigureGodRayFixtures(spec, config);
            }

            await UniTask.Yield(PlayerLoopTiming.Update);
        }

        private void ConfigureBulbs(BigTopEnvironmentSpec spec, RunWorldGenerationConfig config)
        {
            EnsureBulbComponents();
            if (config == null
                || config.BigTopBulbRadius <= 0f
                || (config.BigTopBulbRingCount <= 0 && config.BigTopBulbRadialStringCount <= 0))
            {
                bulbMeshFilter.sharedMesh = null;
                bulbMeshRenderer.sharedMaterial = null;
                bulbMeshRenderer.gameObject.SetActive(false);
                return;
            }

            runtimeBulbMesh = BigTopBulbMeshBuilder.Build(spec, config);
            runtimeBulbMaterial = CreateOpaqueRuntimeMaterial(
                "Big Top Bulb Runtime Material",
                BigTopVisualColorRules.BulbColor(config, 0f));
            bulbMeshFilter.sharedMesh = runtimeBulbMesh;
            bulbMeshRenderer.sharedMaterial = runtimeBulbMaterial;
            ConfigureRenderer(bulbMeshRenderer);
            bulbMeshRenderer.gameObject.SetActive(runtimeBulbMesh != null);
        }

        private async UniTask ConfigureBulbsAsync(
            BigTopEnvironmentSpec spec,
            RunWorldGenerationConfig config,
            WorldLoadTimingDiagnostics timing)
        {
            EnsureBulbComponents();
            if (config == null
                || config.BigTopBulbRadius <= 0f
                || (config.BigTopBulbRingCount <= 0 && config.BigTopBulbRadialStringCount <= 0))
            {
                bulbMeshFilter.sharedMesh = null;
                bulbMeshRenderer.sharedMaterial = null;
                bulbMeshRenderer.gameObject.SetActive(false);
                return;
            }

            using (timing.Stage("BigTop.BulbMesh"))
            {
                runtimeBulbMesh = BigTopBulbMeshBuilder.Build(spec, config);
            }

            await UniTask.Yield(PlayerLoopTiming.Update);
            if (this == null)
            {
                return;
            }

            using (timing.Stage("BigTop.BulbApply"))
            {
                runtimeBulbMaterial = CreateOpaqueRuntimeMaterial(
                    "Big Top Bulb Runtime Material",
                    BigTopVisualColorRules.BulbColor(config, 0f));
                bulbMeshFilter.sharedMesh = runtimeBulbMesh;
                bulbMeshRenderer.sharedMaterial = runtimeBulbMaterial;
                ConfigureRenderer(bulbMeshRenderer);
                bulbMeshRenderer.gameObject.SetActive(runtimeBulbMesh != null);
            }

            await UniTask.Yield(PlayerLoopTiming.Update);
        }

        private void ConfigureDressing(BigTopEnvironmentSpec spec, RunWorldGenerationConfig config)
        {
            EnsureDressingComponents();
            if (config == null || !config.BigTopDressingEnabled)
            {
                ClearDressingRenderer(dressingRopeMeshFilter, dressingRopeMeshRenderer);
                ClearDressingRenderer(dressingPoleMeshFilter, dressingPoleMeshRenderer);
                ClearDressingRenderer(dressingValanceMeshFilter, dressingValanceMeshRenderer);
                ClearDressingRenderer(buntingStringMeshFilter, buntingStringMeshRenderer);
                ClearDressingRenderer(buntingPrimaryFlagMeshFilter, buntingPrimaryFlagMeshRenderer);
                ClearDressingRenderer(buntingSecondaryFlagMeshFilter, buntingSecondaryFlagMeshRenderer);
                ClearDressingRenderer(curtainBannerPanelMeshFilter, curtainBannerPanelMeshRenderer);
                ClearDressingRenderer(curtainBannerTrimMeshFilter, curtainBannerTrimMeshRenderer);
                return;
            }

            runtimeDressingRopeMesh = BigTopDressingMeshBuilder.BuildRopesAndHub(spec, config);
            runtimeDressingRopeMaterial = CreateOpaqueRuntimeMaterial(
                "Big Top Dressing Rope Runtime Material",
                config.bigTopDressingRopeColor,
                CullMode.Off);
            ApplyDressingRenderer(
                dressingRopeMeshFilter,
                dressingRopeMeshRenderer,
                runtimeDressingRopeMesh,
                runtimeDressingRopeMaterial);

            runtimeDressingPoleMesh = BigTopDressingMeshBuilder.BuildPoles(spec, config);
            runtimeDressingPoleMaterial = CreateOpaqueRuntimeMaterial(
                "Big Top Dressing Pole Runtime Material",
                config.bigTopDressingPoleColor,
                CullMode.Off);
            ApplyDressingRenderer(
                dressingPoleMeshFilter,
                dressingPoleMeshRenderer,
                runtimeDressingPoleMesh,
                runtimeDressingPoleMaterial);

            runtimeDressingValanceMesh = BigTopDressingMeshBuilder.BuildValance(spec, config);
            runtimeDressingValanceTexture = runtimeDressingValanceMesh != null
                ? BigTopDressingTextureGenerator.Generate(spec, config, "Big Top Valance Procedural Cloth", config.bigTopCurtainValanceColor, 503, trim: false)
                : null;
            runtimeDressingValanceMaterial = runtimeDressingValanceMesh != null
                ? CreateOpaqueRuntimeMaterial(
                    "Big Top Dressing Valance Runtime Material",
                    runtimeDressingValanceTexture,
                    CullMode.Off)
                : null;
            ApplyDressingRenderer(
                dressingValanceMeshFilter,
                dressingValanceMeshRenderer,
                runtimeDressingValanceMesh,
                runtimeDressingValanceMaterial);

            runtimeBuntingStringMesh = BigTopBuntingMeshBuilder.BuildBuntingStrings(spec, config);
            runtimeBuntingStringMaterial = runtimeBuntingStringMesh != null
                ? CreateOpaqueRuntimeMaterial("Big Top Bunting String Runtime Material", config.bigTopBuntingStringColor, CullMode.Off)
                : null;
            ApplyDressingRenderer(
                buntingStringMeshFilter,
                buntingStringMeshRenderer,
                runtimeBuntingStringMesh,
                runtimeBuntingStringMaterial);

            runtimeBuntingPrimaryFlagMesh = BigTopBuntingMeshBuilder.BuildBuntingFlagsPrimary(spec, config);
            runtimeBuntingPrimaryFlagMaterial = runtimeBuntingPrimaryFlagMesh != null
                ? CreateOpaqueRuntimeMaterial("Big Top Primary Bunting Flag Runtime Material", config.bigTopBuntingPrimaryColor, CullMode.Off)
                : null;
            ApplyDressingRenderer(
                buntingPrimaryFlagMeshFilter,
                buntingPrimaryFlagMeshRenderer,
                runtimeBuntingPrimaryFlagMesh,
                runtimeBuntingPrimaryFlagMaterial);

            runtimeBuntingSecondaryFlagMesh = BigTopBuntingMeshBuilder.BuildBuntingFlagsSecondary(spec, config);
            runtimeBuntingSecondaryFlagMaterial = runtimeBuntingSecondaryFlagMesh != null
                ? CreateOpaqueRuntimeMaterial("Big Top Secondary Bunting Flag Runtime Material", config.bigTopBuntingSecondaryColor, CullMode.Off)
                : null;
            ApplyDressingRenderer(
                buntingSecondaryFlagMeshFilter,
                buntingSecondaryFlagMeshRenderer,
                runtimeBuntingSecondaryFlagMesh,
                runtimeBuntingSecondaryFlagMaterial);

            runtimeCurtainBannerPanelMesh = BigTopBuntingMeshBuilder.BuildCurtainBannerPanels(spec, config);
            runtimeCurtainBannerPanelTexture = runtimeCurtainBannerPanelMesh != null
                ? BigTopDressingTextureGenerator.Generate(spec, config, "Big Top Banner Panel Procedural Cloth", config.bigTopCurtainBannerColor, 761, trim: false)
                : null;
            runtimeCurtainBannerPanelMaterial = runtimeCurtainBannerPanelMesh != null
                ? CreateOpaqueRuntimeMaterial("Big Top Curtain Banner Panel Runtime Material", runtimeCurtainBannerPanelTexture, CullMode.Off)
                : null;
            ApplyDressingRenderer(
                curtainBannerPanelMeshFilter,
                curtainBannerPanelMeshRenderer,
                runtimeCurtainBannerPanelMesh,
                runtimeCurtainBannerPanelMaterial);

            runtimeCurtainBannerTrimMesh = BigTopBuntingMeshBuilder.BuildCurtainBannerTrim(spec, config);
            runtimeCurtainBannerTrimTexture = runtimeCurtainBannerTrimMesh != null
                ? BigTopDressingTextureGenerator.Generate(spec, config, "Big Top Banner Trim Procedural Cloth", config.bigTopCurtainBannerTrimColor, 997, trim: true)
                : null;
            runtimeCurtainBannerTrimMaterial = runtimeCurtainBannerTrimMesh != null
                ? CreateOpaqueRuntimeMaterial("Big Top Curtain Banner Trim Runtime Material", runtimeCurtainBannerTrimTexture, CullMode.Off)
                : null;
            ApplyDressingRenderer(
                curtainBannerTrimMeshFilter,
                curtainBannerTrimMeshRenderer,
                runtimeCurtainBannerTrimMesh,
                runtimeCurtainBannerTrimMaterial);
        }

        private async UniTask ConfigureDressingAsync(
            BigTopEnvironmentSpec spec,
            RunWorldGenerationConfig config,
            WorldLoadTimingDiagnostics timing)
        {
            EnsureDressingComponents();
            if (config == null || !config.BigTopDressingEnabled)
            {
                ClearDressingRenderer(dressingRopeMeshFilter, dressingRopeMeshRenderer);
                ClearDressingRenderer(dressingPoleMeshFilter, dressingPoleMeshRenderer);
                ClearDressingRenderer(dressingValanceMeshFilter, dressingValanceMeshRenderer);
                ClearDressingRenderer(buntingStringMeshFilter, buntingStringMeshRenderer);
                ClearDressingRenderer(buntingPrimaryFlagMeshFilter, buntingPrimaryFlagMeshRenderer);
                ClearDressingRenderer(buntingSecondaryFlagMeshFilter, buntingSecondaryFlagMeshRenderer);
                ClearDressingRenderer(curtainBannerPanelMeshFilter, curtainBannerPanelMeshRenderer);
                ClearDressingRenderer(curtainBannerTrimMeshFilter, curtainBannerTrimMeshRenderer);
                return;
            }

            using (timing.Stage("BigTop.DressingRopes"))
            {
                runtimeDressingRopeMesh = BigTopDressingMeshBuilder.BuildRopesAndHub(spec, config);
                runtimeDressingRopeMaterial = CreateOpaqueRuntimeMaterial(
                    "Big Top Dressing Rope Runtime Material",
                    config.bigTopDressingRopeColor,
                    CullMode.Off);
                ApplyDressingRenderer(
                    dressingRopeMeshFilter,
                    dressingRopeMeshRenderer,
                    runtimeDressingRopeMesh,
                    runtimeDressingRopeMaterial);
            }

            await UniTask.Yield(PlayerLoopTiming.Update);
            if (this == null)
            {
                return;
            }

            using (timing.Stage("BigTop.DressingPoles"))
            {
                runtimeDressingPoleMesh = BigTopDressingMeshBuilder.BuildPoles(spec, config);
                runtimeDressingPoleMaterial = CreateOpaqueRuntimeMaterial(
                    "Big Top Dressing Pole Runtime Material",
                    config.bigTopDressingPoleColor,
                    CullMode.Off);
                ApplyDressingRenderer(
                    dressingPoleMeshFilter,
                    dressingPoleMeshRenderer,
                    runtimeDressingPoleMesh,
                    runtimeDressingPoleMaterial);
            }

            await UniTask.Yield(PlayerLoopTiming.Update);
            if (this == null)
            {
                return;
            }

            using (timing.Stage("BigTop.ValanceMesh"))
            {
                runtimeDressingValanceMesh = BigTopDressingMeshBuilder.BuildValance(spec, config);
            }

            await UniTask.Yield(PlayerLoopTiming.Update);
            if (this == null)
            {
                return;
            }

            using (timing.Stage("BigTop.ValanceTextureApply"))
            {
                runtimeDressingValanceTexture = runtimeDressingValanceMesh != null
                    ? await BigTopDressingTextureGenerator.GenerateAsync(
                        spec,
                        config,
                        "Big Top Valance Procedural Cloth",
                        config.bigTopCurtainValanceColor,
                        503,
                        trim: false,
                        "BigTop.Valance",
                        timing)
                    : null;
                runtimeDressingValanceMaterial = runtimeDressingValanceMesh != null
                    ? CreateOpaqueRuntimeMaterial(
                        "Big Top Dressing Valance Runtime Material",
                        runtimeDressingValanceTexture,
                        CullMode.Off)
                    : null;
                ApplyDressingRenderer(
                    dressingValanceMeshFilter,
                    dressingValanceMeshRenderer,
                    runtimeDressingValanceMesh,
                    runtimeDressingValanceMaterial);
            }

            await UniTask.Yield(PlayerLoopTiming.Update);
            if (this == null)
            {
                return;
            }

            using (timing.Stage("BigTop.BuntingStrings"))
            {
                runtimeBuntingStringMesh = BigTopBuntingMeshBuilder.BuildBuntingStrings(spec, config);
                runtimeBuntingStringMaterial = runtimeBuntingStringMesh != null
                    ? CreateOpaqueRuntimeMaterial("Big Top Bunting String Runtime Material", config.bigTopBuntingStringColor, CullMode.Off)
                    : null;
                ApplyDressingRenderer(
                    buntingStringMeshFilter,
                    buntingStringMeshRenderer,
                    runtimeBuntingStringMesh,
                    runtimeBuntingStringMaterial);
            }

            await UniTask.Yield(PlayerLoopTiming.Update);
            if (this == null)
            {
                return;
            }

            using (timing.Stage("BigTop.BuntingPrimaryFlags"))
            {
                runtimeBuntingPrimaryFlagMesh = BigTopBuntingMeshBuilder.BuildBuntingFlagsPrimary(spec, config);
                runtimeBuntingPrimaryFlagMaterial = runtimeBuntingPrimaryFlagMesh != null
                    ? CreateOpaqueRuntimeMaterial("Big Top Primary Bunting Flag Runtime Material", config.bigTopBuntingPrimaryColor, CullMode.Off)
                    : null;
                ApplyDressingRenderer(
                    buntingPrimaryFlagMeshFilter,
                    buntingPrimaryFlagMeshRenderer,
                    runtimeBuntingPrimaryFlagMesh,
                    runtimeBuntingPrimaryFlagMaterial);
            }

            await UniTask.Yield(PlayerLoopTiming.Update);
            if (this == null)
            {
                return;
            }

            using (timing.Stage("BigTop.BuntingSecondaryFlags"))
            {
                runtimeBuntingSecondaryFlagMesh = BigTopBuntingMeshBuilder.BuildBuntingFlagsSecondary(spec, config);
                runtimeBuntingSecondaryFlagMaterial = runtimeBuntingSecondaryFlagMesh != null
                    ? CreateOpaqueRuntimeMaterial("Big Top Secondary Bunting Flag Runtime Material", config.bigTopBuntingSecondaryColor, CullMode.Off)
                    : null;
                ApplyDressingRenderer(
                    buntingSecondaryFlagMeshFilter,
                    buntingSecondaryFlagMeshRenderer,
                    runtimeBuntingSecondaryFlagMesh,
                    runtimeBuntingSecondaryFlagMaterial);
            }

            await UniTask.Yield(PlayerLoopTiming.Update);
            if (this == null)
            {
                return;
            }

            using (timing.Stage("BigTop.CurtainBannerPanelMesh"))
            {
                runtimeCurtainBannerPanelMesh = BigTopBuntingMeshBuilder.BuildCurtainBannerPanels(spec, config);
            }

            await UniTask.Yield(PlayerLoopTiming.Update);
            if (this == null)
            {
                return;
            }

            using (timing.Stage("BigTop.CurtainBannerPanelTextureApply"))
            {
                runtimeCurtainBannerPanelTexture = runtimeCurtainBannerPanelMesh != null
                    ? await BigTopDressingTextureGenerator.GenerateAsync(
                        spec,
                        config,
                        "Big Top Banner Panel Procedural Cloth",
                        config.bigTopCurtainBannerColor,
                        761,
                        trim: false,
                        "BigTop.CurtainBannerPanel",
                        timing)
                    : null;
                runtimeCurtainBannerPanelMaterial = runtimeCurtainBannerPanelMesh != null
                    ? CreateOpaqueRuntimeMaterial("Big Top Curtain Banner Panel Runtime Material", runtimeCurtainBannerPanelTexture, CullMode.Off)
                    : null;
                ApplyDressingRenderer(
                    curtainBannerPanelMeshFilter,
                    curtainBannerPanelMeshRenderer,
                    runtimeCurtainBannerPanelMesh,
                    runtimeCurtainBannerPanelMaterial);
            }

            await UniTask.Yield(PlayerLoopTiming.Update);
            if (this == null)
            {
                return;
            }

            using (timing.Stage("BigTop.CurtainBannerTrimMesh"))
            {
                runtimeCurtainBannerTrimMesh = BigTopBuntingMeshBuilder.BuildCurtainBannerTrim(spec, config);
            }

            await UniTask.Yield(PlayerLoopTiming.Update);
            if (this == null)
            {
                return;
            }

            using (timing.Stage("BigTop.CurtainBannerTrimTextureApply"))
            {
                runtimeCurtainBannerTrimTexture = runtimeCurtainBannerTrimMesh != null
                    ? await BigTopDressingTextureGenerator.GenerateAsync(
                        spec,
                        config,
                        "Big Top Banner Trim Procedural Cloth",
                        config.bigTopCurtainBannerTrimColor,
                        997,
                        trim: true,
                        "BigTop.CurtainBannerTrim",
                        timing)
                    : null;
                runtimeCurtainBannerTrimMaterial = runtimeCurtainBannerTrimMesh != null
                    ? CreateOpaqueRuntimeMaterial("Big Top Curtain Banner Trim Runtime Material", runtimeCurtainBannerTrimTexture, CullMode.Off)
                    : null;
                ApplyDressingRenderer(
                    curtainBannerTrimMeshFilter,
                    curtainBannerTrimMeshRenderer,
                    runtimeCurtainBannerTrimMesh,
                    runtimeCurtainBannerTrimMaterial);
            }

            await UniTask.Yield(PlayerLoopTiming.Update);
        }

        private void ConfigurePerimeterFog(BigTopEnvironmentSpec spec, RunWorldGenerationConfig config)
        {
            EnsurePerimeterFogComponents();
            if (config == null
                || !config.BigTopPerimeterFogEnabled
                || (config.BigTopPerimeterFogBottomOpacity <= 0f && config.BigTopPerimeterFogTopOpacity <= 0f))
            {
                ClearDressingRenderer(perimeterFogGroundMeshFilter, perimeterFogGroundMeshRenderer);
                ClearDressingRenderer(perimeterFogCurtainMeshFilter, perimeterFogCurtainMeshRenderer);
                return;
            }

            runtimePerimeterFogGroundMesh = BigTopPerimeterFogMeshBuilder.BuildGround(spec, config);
            runtimePerimeterFogGroundTexture = runtimePerimeterFogGroundMesh != null
                ? BigTopPerimeterFogTextureGenerator.Generate(config, BigTopPerimeterFogTextureMode.Ground)
                : null;
            runtimePerimeterFogGroundMaterial = runtimePerimeterFogGroundMesh != null
                ? CreateTransparentRuntimeMaterial(
                    "Big Top Perimeter Ground Fog Runtime Material",
                    runtimePerimeterFogGroundTexture,
                    FogMaterialColor(config))
                : null;
            ApplyDressingRenderer(
                perimeterFogGroundMeshFilter,
                perimeterFogGroundMeshRenderer,
                runtimePerimeterFogGroundMesh,
                runtimePerimeterFogGroundMaterial);

            runtimePerimeterFogCurtainMesh = BigTopPerimeterFogMeshBuilder.BuildCurtain(spec, config);
            runtimePerimeterFogCurtainTexture = runtimePerimeterFogCurtainMesh != null
                ? BigTopPerimeterFogTextureGenerator.Generate(config, BigTopPerimeterFogTextureMode.Curtain)
                : null;
            runtimePerimeterFogCurtainMaterial = runtimePerimeterFogCurtainMesh != null
                ? CreateTransparentRuntimeMaterial(
                    "Big Top Perimeter Curtain Fog Runtime Material",
                    runtimePerimeterFogCurtainTexture,
                    FogMaterialColor(config))
                : null;
            ApplyDressingRenderer(
                perimeterFogCurtainMeshFilter,
                perimeterFogCurtainMeshRenderer,
                runtimePerimeterFogCurtainMesh,
                runtimePerimeterFogCurtainMaterial);
        }

        private void EnsureGodRayComponents()
        {
            EnsureChildRenderer("Big Top God Rays", ref godRayMeshFilter, ref godRayMeshRenderer);
        }

        private void EnsurePerimeterFogComponents()
        {
            EnsureChildRenderer("Big Top Perimeter Ground Fog", ref perimeterFogGroundMeshFilter, ref perimeterFogGroundMeshRenderer);
            EnsureChildRenderer("Big Top Perimeter Curtain Fog", ref perimeterFogCurtainMeshFilter, ref perimeterFogCurtainMeshRenderer);
        }

        private void EnsureBulbComponents()
        {
            EnsureChildRenderer("Big Top Bulb Strings", ref bulbMeshFilter, ref bulbMeshRenderer);
        }

        private void EnsureDressingComponents()
        {
            EnsureChildRenderer("Big Top Dressing Ropes", ref dressingRopeMeshFilter, ref dressingRopeMeshRenderer);
            EnsureChildRenderer("Big Top Dressing Poles", ref dressingPoleMeshFilter, ref dressingPoleMeshRenderer);
            EnsureChildRenderer("Big Top Dressing Valance", ref dressingValanceMeshFilter, ref dressingValanceMeshRenderer);
            EnsureChildRenderer("Big Top Bunting Strings", ref buntingStringMeshFilter, ref buntingStringMeshRenderer);
            EnsureChildRenderer("Big Top Bunting Primary Flags", ref buntingPrimaryFlagMeshFilter, ref buntingPrimaryFlagMeshRenderer);
            EnsureChildRenderer("Big Top Bunting Secondary Flags", ref buntingSecondaryFlagMeshFilter, ref buntingSecondaryFlagMeshRenderer);
            EnsureChildRenderer("Big Top Curtain Banner Panels", ref curtainBannerPanelMeshFilter, ref curtainBannerPanelMeshRenderer);
            EnsureChildRenderer("Big Top Curtain Banner Trim", ref curtainBannerTrimMeshFilter, ref curtainBannerTrimMeshRenderer);
        }

        private void EnsureGodRayFixtureComponents()
        {
            EnsureChildRenderer("Big Top God Ray Fixtures", ref godRayFixtureMeshFilter, ref godRayFixtureMeshRenderer);
            EnsureChildRenderer("Big Top God Ray Fixture Lenses", ref godRayFixtureLensMeshFilter, ref godRayFixtureLensMeshRenderer);
        }

        private void EnsureChildRenderer(
            string childName,
            ref MeshFilter targetMeshFilter,
            ref MeshRenderer targetMeshRenderer)
        {
            if (targetMeshFilter != null && targetMeshRenderer != null)
            {
                return;
            }

            Transform child = transform.Find(childName);
            if (child == null)
            {
                var childObject = new GameObject(childName);
                child = childObject.transform;
                child.SetParent(transform, worldPositionStays: false);
                child.localPosition = Vector3.zero;
                child.localRotation = Quaternion.identity;
                child.localScale = Vector3.one;
            }

            targetMeshFilter = child.GetComponent<MeshFilter>();
            targetMeshRenderer = child.GetComponent<MeshRenderer>();
            targetMeshFilter = targetMeshFilter != null ? targetMeshFilter : child.gameObject.AddComponent<MeshFilter>();
            targetMeshRenderer = targetMeshRenderer != null ? targetMeshRenderer : child.gameObject.AddComponent<MeshRenderer>();
        }

        private static Material CreateRuntimeMaterial(Texture texture)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Unlit/Texture")
                ?? Shader.Find("Unlit/Color")
                ?? Shader.Find("Standard");
            if (shader == null)
            {
                return null;
            }

            var material = new Material(shader)
            {
                name = "Big Top Cloth Runtime Material",
                hideFlags = HideFlags.DontSave
            };
            if (texture != null)
            {
                material.mainTexture = texture;
                if (material.HasProperty("_BaseMap"))
                {
                    material.SetTexture("_BaseMap", texture);
                }
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", Color.white);
            }

            return material;
        }

        private static Material CreateTransparentRuntimeMaterial(string name, Texture texture, Color color)
        {
            Material material = CreateRuntimeMaterial(texture);
            if (material == null)
            {
                return null;
            }

            material.name = name;
            material.renderQueue = (int)RenderQueue.Transparent;
            SetFloatIfPresent(material, "_Surface", 1f);
            SetFloatIfPresent(material, "_Blend", 0f);
            SetFloatIfPresent(material, "_SrcBlend", (float)BlendMode.SrcAlpha);
            SetFloatIfPresent(material, "_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            SetFloatIfPresent(material, "_ZWrite", 0f);
            SetFloatIfPresent(material, "_Cull", (float)CullMode.Off);
            SetMaterialColor(material, color);
            return material;
        }

        private static Material CreateVertexColorTransparentRuntimeMaterial(
            string name,
            Texture texture,
            Color color,
            CullMode cullMode = CullMode.Off)
        {
            Shader shader = Shader.Find("TheCircussyOne/Big Top Vertex Color Unlit")
                ?? Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Unlit/Texture")
                ?? Shader.Find("Unlit/Color")
                ?? Shader.Find("Standard");
            if (shader == null)
            {
                return null;
            }

            var material = new Material(shader)
            {
                name = name,
                hideFlags = HideFlags.DontSave,
                renderQueue = (int)RenderQueue.Transparent
            };
            if (texture != null)
            {
                material.mainTexture = texture;
                if (material.HasProperty("_BaseMap"))
                {
                    material.SetTexture("_BaseMap", texture);
                }
            }

            SetFloatIfPresent(material, "_Surface", 1f);
            SetFloatIfPresent(material, "_Blend", 0f);
            SetFloatIfPresent(material, "_SrcBlend", (float)BlendMode.SrcAlpha);
            SetFloatIfPresent(material, "_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            SetFloatIfPresent(material, "_ZWrite", 0f);
            SetFloatIfPresent(material, "_Cull", (float)cullMode);
            SetMaterialColor(material, color);
            return material;
        }

        private static Material CreateOpaqueRuntimeMaterial(string name, Color color, CullMode cullMode = CullMode.Back)
        {
            Material material = CreateRuntimeMaterial(null);
            if (material == null)
            {
                return null;
            }

            material.name = name;
            SetFloatIfPresent(material, "_Surface", 0f);
            SetFloatIfPresent(material, "_Cull", (float)cullMode);
            SetMaterialColor(material, color);
            return material;
        }

        private static Material CreateOpaqueRuntimeMaterial(string name, Texture texture, CullMode cullMode = CullMode.Back)
        {
            Material material = CreateRuntimeMaterial(texture);
            if (material == null)
            {
                return null;
            }

            material.name = name;
            SetFloatIfPresent(material, "_Surface", 0f);
            SetFloatIfPresent(material, "_Cull", (float)cullMode);
            SetMaterialColor(material, Color.white);
            return material;
        }

        private static void ApplyDressingRenderer(
            MeshFilter targetMeshFilter,
            MeshRenderer targetMeshRenderer,
            Mesh mesh,
            Material material)
        {
            targetMeshFilter.sharedMesh = mesh;
            targetMeshRenderer.sharedMaterial = material;
            ConfigureRenderer(targetMeshRenderer);
            targetMeshRenderer.gameObject.SetActive(mesh != null);
        }

        private static void ClearDressingRenderer(MeshFilter targetMeshFilter, MeshRenderer targetMeshRenderer)
        {
            targetMeshFilter.sharedMesh = null;
            targetMeshRenderer.sharedMaterial = null;
            targetMeshRenderer.gameObject.SetActive(false);
        }

        private static void ConfigureRenderer(Renderer renderer)
        {
            if (renderer == null)
            {
                return;
            }

            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            renderer.allowOcclusionWhenDynamic = false;
        }

        private static Color FogMaterialColor(RunWorldGenerationConfig config)
        {
            Color color = config != null ? config.bigTopPerimeterFogColor : new Color(0.42f, 0.34f, 0.24f, 1f);
            color.a = 1f;
            return color;
        }

        private static void SetMaterialColor(Material material, Color color)
        {
            if (material == null)
            {
                return;
            }

            material.color = color;
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }

            if (material.HasProperty("_EmissionColor"))
            {
                material.SetColor("_EmissionColor", color);
            }
        }

        private static void SetFloatIfPresent(Material material, string property, float value)
        {
            if (material != null && material.HasProperty(property))
            {
                material.SetFloat(property, value);
            }
        }

        private static void DestroyRuntimeObject(Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(target);
                return;
            }

            DestroyImmediate(target);
        }
    }

    public static class BigTopEnvironmentMeshBuilder
    {
        private const float Tau = Mathf.PI * 2f;

        public static Mesh Build(BigTopEnvironmentSpec spec)
        {
            int segments = Mathf.Max(16, spec.SegmentCount);
            int canopyRings = Mathf.Max(2, spec.CanopyRingCount);
            int ringVertexCount = segments + 1;
            int ringCount = canopyRings + 3;
            var vertices = new Vector3[ringCount * ringVertexCount];
            var uvs = new Vector2[vertices.Length];
            var triangles = new List<int>((ringCount - 1) * segments * 6);

            for (int ring = 0; ring < ringCount; ring++)
            {
                float ringRadius;
                float y;
                if (ring == 0)
                {
                    ringRadius = spec.GroundSkirtRadius;
                    y = 0f;
                }
                else if (ring == 1)
                {
                    ringRadius = spec.Radius;
                    y = 0f;
                }
                else if (ring == 2)
                {
                    ringRadius = spec.Radius;
                    y = spec.CurtainHeight;
                }
                else
                {
                    float domeT = (ring - 2) / (float)canopyRings;
                    float eased = 1f - Mathf.Pow(1f - domeT, 1.35f);
                    ringRadius = Mathf.Lerp(spec.Radius, 0.01f, eased);
                    y = spec.CurtainHeight + spec.DomeHeight * domeT;
                }

                float v = spec.TotalHeight <= 0f ? 0f : Mathf.Clamp01(y / spec.TotalHeight);
                for (int segment = 0; segment <= segments; segment++)
                {
                    float u = segment / (float)segments;
                    float angle = u * Tau;
                    int index = ring * ringVertexCount + segment;
                    vertices[index] = new Vector3(
                        Mathf.Cos(angle) * ringRadius,
                        y,
                        Mathf.Sin(angle) * ringRadius);
                    uvs[index] = new Vector2(u, v);
                }
            }

            for (int ring = 0; ring < ringCount - 1; ring++)
            {
                for (int segment = 0; segment < segments; segment++)
                {
                    int a = ring * ringVertexCount + segment;
                    int b = ring * ringVertexCount + segment + 1;
                    int c = (ring + 1) * ringVertexCount + segment + 1;
                    int d = (ring + 1) * ringVertexCount + segment;
                    AddInwardQuad(triangles, a, b, c, d);
                }
            }

            var mesh = new Mesh
            {
                name = "Big Top Environment Mesh",
                vertices = vertices,
                uv = uvs,
                triangles = triangles.ToArray()
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void AddInwardQuad(List<int> triangles, int a, int b, int c, int d)
        {
            triangles.Add(a);
            triangles.Add(b);
            triangles.Add(c);
            triangles.Add(a);
            triangles.Add(c);
            triangles.Add(d);
        }
    }

    public static class BigTopPerimeterFogMeshBuilder
    {
        private const float Tau = Mathf.PI * 2f;
        private const float GroundY = 0.08f;

        public static Mesh BuildGround(BigTopEnvironmentSpec spec, RunWorldGenerationConfig config)
        {
            if (!TryResolveRadii(spec, config, out float innerRadius, out float outerRadius))
            {
                return null;
            }

            int segments = config != null ? config.BigTopPerimeterFogSegmentCount : 96;
            int ringVertexCount = segments + 1;
            var vertices = new Vector3[ringVertexCount * 2];
            var uvs = new Vector2[vertices.Length];
            var colors = new Color[vertices.Length];
            var triangles = new List<int>(segments * 6);

            for (int segment = 0; segment <= segments; segment++)
            {
                float u = segment / (float)segments;
                float angle = u * Tau;
                Vector3 inner = Polar(angle, innerRadius, GroundY);
                Vector3 outer = Polar(angle, outerRadius, GroundY);
                int index = segment * 2;
                vertices[index] = inner;
                vertices[index + 1] = outer;
                uvs[index] = new Vector2(u * 4f, 0f);
                uvs[index + 1] = new Vector2(u * 4f, 1f);
                colors[index] = new Color(1f, 1f, 1f, 0f);
                colors[index + 1] = new Color(1f, 1f, 1f, 1f);
            }

            for (int segment = 0; segment < segments; segment++)
            {
                int a = segment * 2;
                int b = a + 1;
                int d = a + 2;
                int c = a + 3;
                AddUpwardQuad(triangles, a, b, c, d);
            }

            return CreateMesh("Big Top Perimeter Ground Fog Mesh", vertices, uvs, colors, triangles);
        }

        public static Mesh BuildCurtain(BigTopEnvironmentSpec spec, RunWorldGenerationConfig config)
        {
            if (!TryResolveRadii(spec, config, out _, out float radius))
            {
                return null;
            }

            int segments = config != null ? config.BigTopPerimeterFogSegmentCount : 96;
            float height = Mathf.Min(
                config != null ? config.BigTopPerimeterFogHeight : 18f,
                Mathf.Max(0.05f, spec.CurtainHeight));
            int ringVertexCount = segments + 1;
            var vertices = new Vector3[ringVertexCount * 2];
            var uvs = new Vector2[vertices.Length];
            var colors = new Color[vertices.Length];
            var triangles = new List<int>(segments * 6);

            for (int segment = 0; segment <= segments; segment++)
            {
                float u = segment / (float)segments;
                float angle = u * Tau;
                int index = segment * 2;
                vertices[index] = Polar(angle, radius, 0f);
                vertices[index + 1] = Polar(angle, radius, height);
                uvs[index] = new Vector2(u * 4f, 1f);
                uvs[index + 1] = new Vector2(u * 4f, 0f);
                colors[index] = new Color(1f, 1f, 1f, 1f);
                colors[index + 1] = new Color(1f, 1f, 1f, 0f);
            }

            for (int segment = 0; segment < segments; segment++)
            {
                int a = segment * 2;
                int b = a + 1;
                int d = a + 2;
                int c = a + 3;
                AddInwardQuad(triangles, a, b, c, d);
            }

            return CreateMesh("Big Top Perimeter Curtain Fog Mesh", vertices, uvs, colors, triangles);
        }

        private static bool TryResolveRadii(
            BigTopEnvironmentSpec spec,
            RunWorldGenerationConfig config,
            out float innerRadius,
            out float outerRadius)
        {
            innerRadius = 0f;
            outerRadius = 0f;
            if (!spec.IsValid || config == null || !config.BigTopPerimeterFogEnabled)
            {
                return false;
            }

            outerRadius = Mathf.Max(0.1f, spec.Radius - config.BigTopPerimeterFogOuterInset);
            innerRadius = Mathf.Min(spec.Radius * config.BigTopPerimeterFogInnerRadiusPercent, outerRadius - 0.5f);
            innerRadius = Mathf.Max(0.1f, innerRadius);
            return outerRadius - innerRadius > 0.1f;
        }

        private static Mesh CreateMesh(
            string name,
            Vector3[] vertices,
            Vector2[] uvs,
            Color[] colors,
            List<int> triangles)
        {
            var mesh = new Mesh
            {
                name = name,
                vertices = vertices,
                uv = uvs,
                colors = colors,
                triangles = triangles.ToArray()
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void AddUpwardQuad(List<int> triangles, int a, int b, int c, int d)
        {
            triangles.Add(a);
            triangles.Add(c);
            triangles.Add(b);
            triangles.Add(a);
            triangles.Add(d);
            triangles.Add(c);
        }

        private static void AddInwardQuad(List<int> triangles, int a, int b, int c, int d)
        {
            triangles.Add(a);
            triangles.Add(b);
            triangles.Add(c);
            triangles.Add(a);
            triangles.Add(c);
            triangles.Add(d);
        }

        private static Vector3 Polar(float angle, float radius, float y)
        {
            return new Vector3(Mathf.Cos(angle) * radius, y, Mathf.Sin(angle) * radius);
        }
    }

    public enum BigTopPerimeterFogTextureMode
    {
        Ground,
        Curtain
    }

    public static class BigTopPerimeterFogTextureGenerator
    {
        public static Texture2D Generate(
            RunWorldGenerationConfig config,
            BigTopPerimeterFogTextureMode mode)
        {
            const int width = 64;
            const int height = 64;
            float bottomOpacity = config != null ? config.BigTopPerimeterFogBottomOpacity : 0.18f;
            float topOpacity = config != null ? config.BigTopPerimeterFogTopOpacity : 0.02f;
            float noiseStrength = config != null ? config.BigTopPerimeterFogNoiseStrength : 0.32f;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, mipChain: true, linear: false)
            {
                name = mode == BigTopPerimeterFogTextureMode.Ground
                    ? "Big Top Perimeter Ground Fog Gradient"
                    : "Big Top Perimeter Curtain Fog Gradient",
                hideFlags = HideFlags.DontSave,
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear
            };

            var pixels = new Color32[width * height];
            int seed = mode == BigTopPerimeterFogTextureMode.Ground ? 1739 : 2711;
            for (int y = 0; y < height; y++)
            {
                float v = y / (float)(height - 1);
                float fade = Mathf.Pow(SmoothStep(0f, 1f, v), mode == BigTopPerimeterFogTextureMode.Ground ? 1.55f : 1.2f);
                float baseAlpha = Mathf.Lerp(topOpacity, bottomOpacity, fade);
                for (int x = 0; x < width; x++)
                {
                    float u = x / (float)(width - 1);
                    float broad = ValueNoise(u * 5.5f, v * 3.5f, seed);
                    float fine = ValueNoise(u * 22f, v * 14f, seed + 97);
                    float noise = broad * 0.72f + fine * 0.28f;
                    float alpha = Mathf.Clamp01(baseAlpha * Mathf.Lerp(1f, Mathf.Lerp(0.55f, 1.25f, noise), noiseStrength));
                    byte alphaByte = (byte)Mathf.RoundToInt(alpha * 255f);
                    pixels[y * width + x] = new Color32(255, 255, 255, alphaByte);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(updateMipmaps: true, makeNoLongerReadable: false);
            return texture;
        }

        private static float ValueNoise(float x, float y, int seed)
        {
            int x0 = Mathf.FloorToInt(x);
            int y0 = Mathf.FloorToInt(y);
            int x1 = x0 + 1;
            int y1 = y0 + 1;
            float tx = SmoothStep(0f, 1f, x - x0);
            float ty = SmoothStep(0f, 1f, y - y0);
            float a = Hash01(x0, y0, seed);
            float b = Hash01(x1, y0, seed);
            float c = Hash01(x0, y1, seed);
            float d = Hash01(x1, y1, seed);
            return Mathf.Lerp(Mathf.Lerp(a, b, tx), Mathf.Lerp(c, d, tx), ty);
        }

        private static float Hash01(int x, int y, int seed)
        {
            unchecked
            {
                uint h = (uint)seed;
                h ^= (uint)x * 374761393u;
                h = (h << 13) | (h >> 19);
                h ^= (uint)y * 668265263u;
                h *= 1274126177u;
                h ^= h >> 16;
                return (h & 0x00FFFFFF) / 16777215f;
            }
        }

        private static float SmoothStep(float edge0, float edge1, float value)
        {
            float t = Mathf.Clamp01((value - edge0) / Mathf.Max(0.0001f, edge1 - edge0));
            return t * t * (3f - 2f * t);
        }
    }

    public readonly struct BigTopGodRaySource
    {
        public BigTopGodRaySource(Vector3 mount, Vector3 start, Vector3 end)
        {
            Mount = mount;
            Start = start;
            End = end;
        }

        public Vector3 Mount { get; }
        public Vector3 Start { get; }
        public Vector3 End { get; }
        public Vector3 Direction => (End - Mount).sqrMagnitude <= 0.0001f ? Vector3.down : (End - Mount).normalized;
    }

    public static class BigTopGodRayMeshBuilder
    {
        private const float Tau = Mathf.PI * 2f;
        private const int FrustumSideCount = 8;

        public static Mesh Build(BigTopEnvironmentSpec spec, RunWorldGenerationConfig config)
        {
            return Build(spec, config, 0f, animateShowLights: false);
        }

        public static Mesh Build(
            BigTopEnvironmentSpec spec,
            RunWorldGenerationConfig config,
            float timeSeconds,
            bool animateShowLights)
        {
            int count = config != null ? config.BigTopGodRayCount : 0;
            if (!spec.IsValid || count <= 0)
            {
                return null;
            }

            var vertices = new List<Vector3>(count * FrustumSideCount * 4);
            var uvs = new List<Vector2>(count * FrustumSideCount * 4);
            var colors = new List<Color>(count * FrustumSideCount * 4);
            var triangles = new List<int>(count * FrustumSideCount * 6);
            float topWidth = ResolveTopWidth(spec, config);
            float bottomWidth = Mathf.Max(topWidth, spec.Radius * (config != null ? config.BigTopGodRayBottomWidthPercent : 0.065f));

            for (int i = 0; i < count; i++)
            {
                BigTopGodRaySource source = SourceAt(spec, config, i, count, timeSeconds, animateShowLights);
                Color tint = BigTopVisualColorRules.ShowLightTint(config, spec.Seed, i, timeSeconds);
                AddFrustum(vertices, uvs, colors, triangles, source.Start, source.End, topWidth, bottomWidth, tint);
            }

            var mesh = new Mesh
            {
                name = "Big Top God Ray Mesh",
                vertices = vertices.ToArray(),
                uv = uvs.ToArray(),
                colors = colors.ToArray(),
                triangles = triangles.ToArray()
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        internal static float ResolveTopWidth(BigTopEnvironmentSpec spec, RunWorldGenerationConfig config)
        {
            float topWidth = Mathf.Max(0.05f, spec.Radius * (config != null ? config.BigTopGodRayTopWidthPercent : 0.018f));
            if (config != null && config.BigTopGodRayFixturesEnabled && config.BigTopGodRayFixtureLensRadius > 0f)
            {
                float fixtureMouthWidth = config.BigTopGodRayFixtureLensRadius * 2f * config.BigTopGodRayFixtureBeamCoverage;
                topWidth = Mathf.Min(topWidth, Mathf.Max(0.05f, fixtureMouthWidth));
            }

            return topWidth;
        }

        private static void AddFrustum(
            List<Vector3> vertices,
            List<Vector2> uvs,
            List<Color> colors,
            List<int> triangles,
            Vector3 start,
            Vector3 end,
            float topWidth,
            float bottomWidth,
            Color color)
        {
            Vector3 forward = new Vector3(end.x - start.x, 0f, end.z - start.z);
            if (forward.sqrMagnitude <= 0.0001f)
            {
                forward = Vector3.forward;
            }

            forward.Normalize();
            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
            float topRadius = topWidth * 0.5f;
            float bottomRadius = bottomWidth * 0.5f;
            for (int side = 0; side < FrustumSideCount; side++)
            {
                float a0 = side / (float)FrustumSideCount * Tau;
                float a1 = (side + 1) / (float)FrustumSideCount * Tau;
                Vector3 topA = start + RadialOffset(right, forward, a0, topRadius);
                Vector3 topB = start + RadialOffset(right, forward, a1, topRadius);
                Vector3 bottomA = end + RadialOffset(right, forward, a0, bottomRadius);
                Vector3 bottomB = end + RadialOffset(right, forward, a1, bottomRadius);
                AddQuad(vertices, uvs, colors, triangles, topA, bottomA, bottomB, topB, color);
            }
        }

        private static Vector3 RadialOffset(Vector3 right, Vector3 forward, float angle, float radius)
        {
            return (right * Mathf.Cos(angle) + forward * Mathf.Sin(angle)) * radius;
        }

        private static void AddQuad(
            List<Vector3> vertices,
            List<Vector2> uvs,
            List<Color> colors,
            List<int> triangles,
            Vector3 topLeft,
            Vector3 bottomLeft,
            Vector3 bottomRight,
            Vector3 topRight,
            Color color)
        {
            int index = vertices.Count;
            vertices.Add(topLeft);
            vertices.Add(bottomLeft);
            vertices.Add(bottomRight);
            vertices.Add(topRight);
            uvs.Add(new Vector2(0f, 1f));
            uvs.Add(new Vector2(0f, 0f));
            uvs.Add(new Vector2(1f, 0f));
            uvs.Add(new Vector2(1f, 1f));
            colors.Add(color);
            colors.Add(color);
            colors.Add(color);
            colors.Add(color);
            triangles.Add(index);
            triangles.Add(index + 1);
            triangles.Add(index + 2);
            triangles.Add(index);
            triangles.Add(index + 2);
            triangles.Add(index + 3);
        }

        private static Vector3 Polar(float angle, float radius, float y)
        {
            return new Vector3(Mathf.Cos(angle) * radius, y, Mathf.Sin(angle) * radius);
        }

        internal static float DomeRadius(BigTopEnvironmentSpec spec, float domeT)
        {
            float eased = 1f - Mathf.Pow(1f - Mathf.Clamp01(domeT), 1.35f);
            return Mathf.Lerp(spec.Radius, 0.01f, eased);
        }

        internal static BigTopGodRaySource SourceAt(
            BigTopEnvironmentSpec spec,
            RunWorldGenerationConfig config,
            int index,
            int count)
        {
            return SourceAt(spec, config, index, count, 0f, animateShowLights: false);
        }

        internal static BigTopGodRaySource SourceAt(
            BigTopEnvironmentSpec spec,
            RunWorldGenerationConfig config,
            int index,
            int count,
            float timeSeconds,
            bool animateShowLights)
        {
            count = Mathf.Max(1, count);
            float startDomeT = config != null ? config.BigTopGodRayStartDomePercent : 0.18f;
            float startY = spec.CurtainHeight + spec.DomeHeight * startDomeT;
            float canopyRadius = DomeRadius(spec, startDomeT);
            float startRadiusMin = canopyRadius * (config != null ? config.BigTopGodRayStartRadiusMinPercent : 0.55f);
            float startRadiusMax = canopyRadius * (config != null ? config.BigTopGodRayStartRadiusMaxPercent : 0.95f);
            float endRadius = spec.Radius * (config != null ? config.BigTopGodRayEndRadiusPercent : 0.18f);
            float endY = Mathf.Clamp(config != null ? config.BigTopGodRayEndHeight : 8f, 0f, startY - 1f);
            float jitterTurns = config != null ? config.BigTopGodRayAngleJitter : 0.14f;
            float baseTurn = index / (float)count;
            float angleJitter = (Hash01(index, 13, spec.Seed) - 0.5f) * jitterTurns;
            float startAngle = (baseTurn + angleJitter) * Tau;
            float endAngle = (baseTurn + (Hash01(index, 97, spec.Seed) - 0.5f) * jitterTurns * 0.55f) * Tau;
            float startRadius = Mathf.Lerp(startRadiusMin, startRadiusMax, Hash01(index, 37, spec.Seed));
            float endRadiusLocal = endRadius * Mathf.Lerp(0.55f, 1.2f, Hash01(index, 71, spec.Seed));
            if (config != null && animateShowLights && index < config.BigTopShowLightCount)
            {
                float speed = Mathf.Lerp(config.BigTopShowLightSpeedMinHz, config.BigTopShowLightSpeedMaxHz, Hash01(index, 211, spec.Seed));
                float phase = Hash01(index, 223, spec.Seed) * Tau;
                float cycle = timeSeconds * speed * Tau + phase;
                float sweepRadians = config.BigTopShowLightSweepDegrees * Mathf.Deg2Rad;
                endAngle += Mathf.Sin(cycle) * sweepRadians;
                float radiusDrift = spec.Radius * config.BigTopShowLightRadiusDriftPercent * Mathf.Sin(cycle * 0.67f + phase * 1.31f);
                endRadiusLocal = Mathf.Clamp(endRadiusLocal + radiusDrift, spec.Radius * 0.05f, spec.Radius * 0.65f);
            }

            Vector3 mount = Polar(startAngle, startRadius, startY);
            Vector3 end = Polar(endAngle, endRadiusLocal, endY);
            Vector3 direction = (end - mount).sqrMagnitude <= 0.0001f ? Vector3.down : (end - mount).normalized;
            float sourceOffset = 0f;
            if (config != null && config.BigTopGodRayFixturesEnabled)
            {
                sourceOffset = Mathf.Min(config.BigTopGodRayFixtureLength, Vector3.Distance(mount, end) * 0.35f);
            }

            return new BigTopGodRaySource(mount, mount + direction * sourceOffset, end);
        }

        private static float Hash01(int index, int salt, int seed)
        {
            unchecked
            {
                uint h = (uint)(seed + salt * 374761393);
                h ^= (uint)index * 668265263u;
                h = (h << 13) | (h >> 19);
                h *= 1274126177u;
                h ^= h >> 16;
                return (h & 0x00FFFFFF) / 16777215f;
            }
        }
    }

    public static class BigTopBulbMeshBuilder
    {
        private const float Tau = Mathf.PI * 2f;

        public static int BulbCount(RunWorldGenerationConfig config)
        {
            if (config == null)
            {
                return 0;
            }

            return config.BigTopBulbRingCount * config.BigTopBulbsPerRing
                + config.BigTopBulbRadialStringCount * config.BigTopBulbsPerRadialString;
        }

        public static Mesh Build(BigTopEnvironmentSpec spec, RunWorldGenerationConfig config)
        {
            int bulbCount = BulbCount(config);
            float radius = config != null ? config.BigTopBulbRadius : 0f;
            if (!spec.IsValid || bulbCount <= 0 || radius <= 0f)
            {
                return null;
            }

            var vertices = new List<Vector3>(bulbCount * 6);
            var triangles = new List<int>(bulbCount * 24);
            AddRingBulbs(spec, config, vertices, triangles, radius);
            AddRadialBulbs(spec, config, vertices, triangles, radius);

            var mesh = new Mesh
            {
                name = "Big Top Bulb String Mesh",
                vertices = vertices.ToArray(),
                triangles = triangles.ToArray()
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void AddRingBulbs(
            BigTopEnvironmentSpec spec,
            RunWorldGenerationConfig config,
            List<Vector3> vertices,
            List<int> triangles,
            float bulbRadius)
        {
            int ringCount = config.BigTopBulbRingCount;
            int bulbsPerRing = config.BigTopBulbsPerRing;
            if (ringCount <= 0 || bulbsPerRing <= 0)
            {
                return;
            }

            for (int ring = 0; ring < ringCount; ring++)
            {
                float domeT = RingDomeT(ring, ringCount);
                float y = spec.CurtainHeight + spec.DomeHeight * domeT;
                float radius = CanopyStringRadius(spec, domeT);
                float turnOffset = ring * 0.5f / Mathf.Max(1, bulbsPerRing);
                for (int bulb = 0; bulb < bulbsPerRing; bulb++)
                {
                    float angle = ((bulb / (float)bulbsPerRing) + turnOffset) * Tau;
                    AddOctahedron(vertices, triangles, Polar(angle, radius, y), bulbRadius);
                }
            }
        }

        private static void AddRadialBulbs(
            BigTopEnvironmentSpec spec,
            RunWorldGenerationConfig config,
            List<Vector3> vertices,
            List<int> triangles,
            float bulbRadius)
        {
            int stringCount = config.BigTopBulbRadialStringCount;
            int bulbsPerString = config.BigTopBulbsPerRadialString;
            if (stringCount <= 0 || bulbsPerString <= 0)
            {
                return;
            }

            for (int stringIndex = 0; stringIndex < stringCount; stringIndex++)
            {
                float angle = stringIndex / (float)stringCount * Tau;
                for (int bulb = 0; bulb < bulbsPerString; bulb++)
                {
                    float domeT = RadialDomeT(bulb, bulbsPerString);
                    float y = spec.CurtainHeight + spec.DomeHeight * domeT;
                    float radius = CanopyStringRadius(spec, domeT);
                    AddOctahedron(vertices, triangles, Polar(angle, radius, y), bulbRadius * Mathf.Lerp(1f, 0.72f, domeT));
                }
            }
        }

        internal static float RingDomeT(int ring, int ringCount)
        {
            return Mathf.Lerp(0.04f, 0.42f, (ring + 1f) / (Mathf.Max(1, ringCount) + 1f));
        }

        internal static float RadialDomeT(int bulb, int bulbsPerString)
        {
            return Mathf.Lerp(0.06f, 0.72f, (bulb + 1f) / (Mathf.Max(1, bulbsPerString) + 1f));
        }

        internal static float CanopyStringRadius(BigTopEnvironmentSpec spec, float domeT)
        {
            return BigTopGodRayMeshBuilder.DomeRadius(spec, domeT) * 0.96f;
        }

        private static void AddOctahedron(List<Vector3> vertices, List<int> triangles, Vector3 center, float radius)
        {
            int index = vertices.Count;
            vertices.Add(center + Vector3.up * radius);
            vertices.Add(center + Vector3.down * radius);
            vertices.Add(center + Vector3.right * radius);
            vertices.Add(center + Vector3.left * radius);
            vertices.Add(center + Vector3.forward * radius);
            vertices.Add(center + Vector3.back * radius);

            AddTriangle(triangles, index, 0, 2, 4);
            AddTriangle(triangles, index, 0, 4, 3);
            AddTriangle(triangles, index, 0, 3, 5);
            AddTriangle(triangles, index, 0, 5, 2);
            AddTriangle(triangles, index, 1, 4, 2);
            AddTriangle(triangles, index, 1, 3, 4);
            AddTriangle(triangles, index, 1, 5, 3);
            AddTriangle(triangles, index, 1, 2, 5);
        }

        private static void AddTriangle(List<int> triangles, int index, int a, int b, int c)
        {
            triangles.Add(index + a);
            triangles.Add(index + b);
            triangles.Add(index + c);
        }

        private static Vector3 Polar(float angle, float radius, float y)
        {
            return new Vector3(Mathf.Cos(angle) * radius, y, Mathf.Sin(angle) * radius);
        }
    }

    public static class BigTopGodRayTextureGenerator
    {
        public static Texture2D Generate(RunWorldGenerationConfig config = null)
        {
            const int width = 64;
            const int height = 128;
            float bottomFadePower = config != null ? config.BigTopGodRayBottomFadePower : 1.8f;
            float edgeFadePower = config != null ? config.BigTopGodRayEdgeFadePower : 1.75f;
            float noiseStrength = config != null ? config.BigTopGodRayTextureNoiseStrength : 0.12f;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, mipChain: true, linear: false)
            {
                name = "Big Top God Ray Gradient",
                hideFlags = HideFlags.DontSave,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            var pixels = new Color32[width * height];
            for (int y = 0; y < height; y++)
            {
                float v = y / (float)(height - 1);
                float vertical = Mathf.Pow(SmoothStep(0f, 1f, v), bottomFadePower);
                for (int x = 0; x < width; x++)
                {
                    float u = x / (float)(width - 1);
                    float centered = SmoothStep(0f, 1f, 1f - Mathf.Abs(u * 2f - 1f));
                    float horizontal = Mathf.Pow(centered, edgeFadePower);
                    float broadStreak = ValueNoise(u * 10f, v * 2.5f, 917);
                    float fineStreak = ValueNoise(u * 38f, v * 8f, 1493);
                    float streak = broadStreak * 0.75f + fineStreak * 0.25f;
                    float streakFactor = Mathf.Lerp(1f, Mathf.Lerp(0.78f, 1.18f, streak), noiseStrength);
                    byte alpha = (byte)Mathf.RoundToInt(Mathf.Clamp01(horizontal * vertical * streakFactor) * 255f);
                    pixels[y * width + x] = new Color32(255, 255, 255, alpha);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(updateMipmaps: true, makeNoLongerReadable: false);
            return texture;
        }

        private static float SmoothStep(float edge0, float edge1, float value)
        {
            float t = Mathf.Clamp01((value - edge0) / Mathf.Max(0.0001f, edge1 - edge0));
            return t * t * (3f - 2f * t);
        }

        private static float ValueNoise(float x, float y, int seed)
        {
            int x0 = Mathf.FloorToInt(x);
            int y0 = Mathf.FloorToInt(y);
            int x1 = x0 + 1;
            int y1 = y0 + 1;
            float tx = SmoothStep(0f, 1f, x - x0);
            float ty = SmoothStep(0f, 1f, y - y0);
            float a = Hash01(x0, y0, seed);
            float b = Hash01(x1, y0, seed);
            float c = Hash01(x0, y1, seed);
            float d = Hash01(x1, y1, seed);
            return Mathf.Lerp(Mathf.Lerp(a, b, tx), Mathf.Lerp(c, d, tx), ty);
        }

        private static float Hash01(int x, int y, int seed)
        {
            unchecked
            {
                uint h = (uint)seed;
                h ^= (uint)x * 374761393u;
                h = (h << 13) | (h >> 19);
                h ^= (uint)y * 668265263u;
                h *= 1274126177u;
                h ^= h >> 16;
                return (h & 0x00FFFFFF) / 16777215f;
            }
        }
    }

    public readonly struct BigTopGeneratedTextureData
    {
        public BigTopGeneratedTextureData(
            string name,
            int width,
            int height,
            Color32[] pixels,
            TextureWrapMode wrapMode,
            FilterMode filterMode)
        {
            Name = string.IsNullOrWhiteSpace(name) ? "Big Top Procedural Texture" : name;
            Width = Mathf.Max(1, width);
            Height = Mathf.Max(1, height);
            Pixels = pixels ?? System.Array.Empty<Color32>();
            WrapMode = wrapMode;
            FilterMode = filterMode;
        }

        public string Name { get; }
        public int Width { get; }
        public int Height { get; }
        public Color32[] Pixels { get; }
        public TextureWrapMode WrapMode { get; }
        public FilterMode FilterMode { get; }
    }

    public static class BigTopTextureUpload
    {
        public static Texture2D CreateTexture(
            BigTopGeneratedTextureData data,
            bool mipChain = true,
            bool makeNoLongerReadable = false)
        {
            var texture = new Texture2D(data.Width, data.Height, TextureFormat.RGBA32, mipChain, linear: false)
            {
                name = data.Name,
                hideFlags = HideFlags.DontSave,
                wrapMode = data.WrapMode,
                filterMode = data.FilterMode
            };
            texture.SetPixels32(data.Pixels);
            texture.Apply(updateMipmaps: mipChain, makeNoLongerReadable: makeNoLongerReadable);
            return texture;
        }
    }

    public static class BigTopDressingTextureGenerator
    {
        private static readonly Color DirtColor = new(0.16f, 0.105f, 0.065f, 1f);
        private static readonly Color TarnishColor = new(0.29f, 0.20f, 0.09f, 1f);
        private static readonly Color WornGoldColor = new(1f, 0.78f, 0.35f, 1f);

        private readonly struct Settings
        {
            public Settings(
                int size,
                float brightness,
                float weaveStrength,
                float gritStrength,
                float lowerDirtStrength,
                string name,
                Color baseColor,
                int seed,
                bool trim)
            {
                Size = Mathf.Clamp(size, 128, 1024);
                Brightness = Mathf.Clamp01(brightness);
                WeaveStrength = Mathf.Clamp01(weaveStrength);
                GritStrength = Mathf.Clamp01(gritStrength);
                LowerDirtStrength = Mathf.Clamp01(lowerDirtStrength);
                Name = name;
                BaseColor = baseColor;
                Seed = seed;
                Trim = trim;
            }

            public int Size { get; }
            public float Brightness { get; }
            public float WeaveStrength { get; }
            public float GritStrength { get; }
            public float LowerDirtStrength { get; }
            public string Name { get; }
            public Color BaseColor { get; }
            public int Seed { get; }
            public bool Trim { get; }
        }

        public static Texture2D Generate(
            BigTopEnvironmentSpec spec,
            RunWorldGenerationConfig config,
            string name,
            Color baseColor,
            int seedSalt,
            bool trim)
        {
            return BigTopTextureUpload.CreateTexture(BuildTextureData(Snapshot(spec, config, name, baseColor, seedSalt, trim)));
        }

        public static async UniTask<Texture2D> GenerateAsync(
            BigTopEnvironmentSpec spec,
            RunWorldGenerationConfig config,
            string name,
            Color baseColor,
            int seedSalt,
            bool trim,
            string timingPrefix = "BigTop.Dressing",
            WorldLoadTimingDiagnostics timing = null,
            bool makeNoLongerReadable = true)
        {
            timing ??= WorldLoadTimingDiagnostics.Disabled;
            Settings settings = Snapshot(spec, config, name, baseColor, seedSalt, trim);
            BigTopGeneratedTextureData data;
            using (timing.Stage($"{timingPrefix}PixelsThreadPool"))
            {
                data = await UniTask.RunOnThreadPool(() => BuildTextureData(settings));
                await UniTask.SwitchToMainThread();
            }

            using (timing.Stage($"{timingPrefix}TextureUpload"))
            {
                return BigTopTextureUpload.CreateTexture(data, makeNoLongerReadable: makeNoLongerReadable);
            }
        }

        public static BigTopGeneratedTextureData BuildTextureData(
            BigTopEnvironmentSpec spec,
            RunWorldGenerationConfig config,
            string name,
            Color baseColor,
            int seedSalt,
            bool trim)
        {
            return BuildTextureData(Snapshot(spec, config, name, baseColor, seedSalt, trim));
        }

        private static Settings Snapshot(
            BigTopEnvironmentSpec spec,
            RunWorldGenerationConfig config,
            string name,
            Color baseColor,
            int seedSalt,
            bool trim)
        {
            return new Settings(
                spec.TextureResolution,
                config != null ? config.BigTopBrightness : 0.92f,
                config != null ? config.BigTopWeaveStrength : 0.18f,
                config != null ? config.BigTopGritStrength : 0.28f,
                config != null ? config.BigTopLowerDirtStrength : 0.36f,
                name,
                baseColor,
                spec.Seed + seedSalt,
                trim);
        }

        private static BigTopGeneratedTextureData BuildTextureData(Settings settings)
        {
            int size = settings.Size;
            var pixels = new Color32[size * size];

            for (int y = 0; y < size; y++)
            {
                float v = y / (float)(size - 1);
                float lowerDirt = Mathf.Pow(1f - v, settings.Trim ? 2.9f : 2.15f);
                for (int x = 0; x < size; x++)
                {
                    float u = x / (float)(size - 1);
                    float weave = Weave(x, y, size);
                    float broadGrit = ValueNoise(u * 16f, v * 16f, settings.Seed + 31);
                    float fineGrit = ValueNoise(u * 72f, v * 92f, settings.Seed + 191);
                    float streak = ValueNoise(u * 38f, v * 4.5f, settings.Seed + 383);
                    float edgeWear = settings.Trim
                        ? Mathf.Clamp01(1f - Mathf.Min(u, 1f - u) * 9f) * 0.55f
                            + Mathf.Clamp01(1f - Mathf.Min(v, 1f - v) * 9f) * 0.45f
                        : 0f;
                    float grit = broadGrit * 0.55f + fineGrit * 0.35f + streak * lowerDirt * 0.1f;
                    float dirtAmount = Mathf.Clamp01(grit * settings.GritStrength * (settings.Trim ? 0.55f : 0.85f)
                        + lowerDirt * settings.LowerDirtStrength * (settings.Trim ? 0.42f : 0.9f));
                    Color dirtTarget = settings.Trim ? TarnishColor : DirtColor;
                    Color color = Color.Lerp(settings.BaseColor, dirtTarget, dirtAmount * (settings.Trim ? 0.42f : 0.68f));
                    if (settings.Trim && edgeWear > 0f)
                    {
                        color = Color.Lerp(color, WornGoldColor, edgeWear * 0.18f);
                    }

                    float shade = settings.Brightness
                        + weave * settings.WeaveStrength * 0.14f
                        - grit * settings.GritStrength * (settings.Trim ? 0.08f : 0.16f)
                        - lowerDirt * settings.LowerDirtStrength * (settings.Trim ? 0.08f : 0.18f)
                        + edgeWear * 0.06f;
                    color.r = Mathf.Clamp01(color.r * shade);
                    color.g = Mathf.Clamp01(color.g * shade);
                    color.b = Mathf.Clamp01(color.b * shade);
                    color.a = 1f;
                    pixels[y * size + x] = color;
                }
            }

            return new BigTopGeneratedTextureData(
                settings.Name,
                size,
                size,
                pixels,
                TextureWrapMode.Repeat,
                FilterMode.Bilinear);
        }

        private static float Weave(int x, int y, int size)
        {
            float xWave = Mathf.Sin(x * 0.74f) * 0.5f + 0.5f;
            float yWave = Mathf.Sin(y * 1.16f) * 0.5f + 0.5f;
            float broad = Mathf.Sin((x - y) * Mathf.PI / Mathf.Max(8f, size * 0.04f)) * 0.5f + 0.5f;
            return (xWave * 0.34f + yWave * 0.42f + broad * 0.24f) - 0.5f;
        }

        private static float ValueNoise(float x, float y, int seed)
        {
            int x0 = Mathf.FloorToInt(x);
            int y0 = Mathf.FloorToInt(y);
            int x1 = x0 + 1;
            int y1 = y0 + 1;
            float tx = SmoothStep(0f, 1f, x - x0);
            float ty = SmoothStep(0f, 1f, y - y0);
            float a = Hash01(x0, y0, seed);
            float b = Hash01(x1, y0, seed);
            float c = Hash01(x0, y1, seed);
            float d = Hash01(x1, y1, seed);
            return Mathf.Lerp(Mathf.Lerp(a, b, tx), Mathf.Lerp(c, d, tx), ty);
        }

        private static float Hash01(int x, int y, int seed)
        {
            unchecked
            {
                uint h = (uint)seed;
                h ^= (uint)x * 374761393u;
                h = (h << 13) | (h >> 19);
                h ^= (uint)y * 668265263u;
                h *= 1274126177u;
                h ^= h >> 16;
                return (h & 0x00FFFFFF) / 16777215f;
            }
        }

        private static float SmoothStep(float edge0, float edge1, float value)
        {
            float t = Mathf.Clamp01((value - edge0) / Mathf.Max(0.0001f, edge1 - edge0));
            return t * t * (3f - 2f * t);
        }
    }

    public static class BigTopVisualColorRules
    {
        public static Color GodRayColor(RunWorldGenerationConfig config, float timeSeconds)
        {
            Color color = config != null ? config.bigTopGodRayColor : new Color(1f, 0.74f, 0.34f, 1f);
            float opacity = config != null ? config.BigTopGodRayOpacity : 0.16f;
            float flicker = Flicker(
                timeSeconds,
                config != null ? config.BigTopGodRayFlickerAmount : 0.08f,
                config != null ? config.BigTopGodRayFlickerSpeed : 0.22f,
                phase: 1.7f);
            color.a = Mathf.Clamp01(opacity * flicker);
            return color;
        }

        public static Color BulbColor(RunWorldGenerationConfig config, float timeSeconds)
        {
            Color color = config != null ? config.bigTopBulbColor : new Color(1f, 0.83f, 0.48f, 1f);
            float emission = config != null ? config.BigTopBulbEmissionStrength : 2.2f;
            float flicker = Flicker(
                timeSeconds,
                config != null ? config.BigTopBulbFlickerAmount : 0.10f,
                config != null ? config.BigTopBulbFlickerSpeed : 0.55f,
                phase: 5.1f);
            float multiplier = Mathf.Max(0f, emission * flicker);
            return new Color(color.r * multiplier, color.g * multiplier, color.b * multiplier, 1f);
        }

        public static Color GodRayFixtureLensColor(RunWorldGenerationConfig config, float timeSeconds)
        {
            Color color = config != null ? config.bigTopGodRayFixtureLensColor : new Color(1f, 0.78f, 0.38f, 1f);
            float emission = config != null ? config.BigTopGodRayFixtureLensEmissionStrength : 2.7f;
            float flicker = Flicker(
                timeSeconds,
                config != null ? config.BigTopGodRayFlickerAmount : 0.08f,
                config != null ? config.BigTopGodRayFlickerSpeed : 0.22f,
                phase: 2.9f);
            float multiplier = Mathf.Max(0f, emission * flicker);
            return new Color(color.r * multiplier, color.g * multiplier, color.b * multiplier, 1f);
        }

        public static Color ShowLightTint(
            RunWorldGenerationConfig config,
            int seed,
            int index,
            float timeSeconds)
        {
            if (config == null
                || !config.BigTopShowLightColorVariationEnabled
                || index < 0
                || index >= config.BigTopShowLightCount)
            {
                return Color.white;
            }

            Color pink = NormalizeTint(config.bigTopShowLightAccentPinkColor);
            Color teal = NormalizeTint(config.bigTopShowLightAccentTealColor);
            float seededAccent = config.BigTopShowLightAccentStrength * Mathf.Lerp(0.35f, 1f, Hash01(index, 431, seed));
            Color seededTarget = Hash01(index, 439, seed) < 0.5f ? pink : teal;
            Color tint = Color.Lerp(Color.white, seededTarget, seededAccent);

            float driftSpeed = Mathf.Lerp(
                config.BigTopShowLightColorDriftSpeedMinHz,
                config.BigTopShowLightColorDriftSpeedMaxHz,
                Hash01(index, 449, seed));
            float driftStrength = config.BigTopShowLightColorDriftStrength;
            if (driftSpeed > 0f && driftStrength > 0f)
            {
                float phase = Hash01(index, 457, seed) * Mathf.PI * 2f;
                float wave = Mathf.Sin(timeSeconds * driftSpeed * Mathf.PI * 2f + phase);
                Color driftTarget = wave >= 0f ? pink : teal;
                tint = Color.Lerp(tint, driftTarget, Mathf.Abs(wave) * driftStrength);
            }

            tint.a = 1f;
            return tint;
        }

        private static float Flicker(float timeSeconds, float amount, float speed, float phase)
        {
            if (amount <= 0f || speed <= 0f)
            {
                return 1f;
            }

            float wave = Mathf.Sin(timeSeconds * speed * Mathf.PI * 2f + phase) * 0.65f
                + Mathf.Sin(timeSeconds * speed * Mathf.PI * 5.13f + phase * 0.37f) * 0.35f;
            return Mathf.Max(0f, 1f + wave * Mathf.Clamp01(amount));
        }

        private static Color NormalizeTint(Color color)
        {
            float max = Mathf.Max(0.0001f, color.maxColorComponent);
            return new Color(
                Mathf.Clamp01(color.r / max),
                Mathf.Clamp01(color.g / max),
                Mathf.Clamp01(color.b / max),
                1f);
        }

        private static float Hash01(int x, int y, int seed)
        {
            unchecked
            {
                uint h = (uint)seed;
                h ^= (uint)x * 374761393u;
                h = (h << 13) | (h >> 19);
                h ^= (uint)y * 668265263u;
                h *= 1274126177u;
                h ^= h >> 16;
                return (h & 0x00FFFFFF) / 16777215f;
            }
        }
    }

    public static class BigTopClothTextureGenerator
    {
        private static readonly Color DirtColor = new(0.16f, 0.105f, 0.065f, 1f);

        private readonly struct Settings
        {
            public Settings(
                int size,
                int stripeCount,
                int seed,
                Color red,
                Color canvas,
                float brightness,
                float seamStrength,
                float weaveStrength,
                float gritStrength,
                float lowerDirtStrength)
            {
                Size = Mathf.Clamp(size, 128, 2048);
                StripeCount = Mathf.Max(2, stripeCount);
                Seed = seed;
                Red = red;
                Canvas = canvas;
                Brightness = Mathf.Clamp01(brightness);
                SeamStrength = Mathf.Clamp01(seamStrength);
                WeaveStrength = Mathf.Clamp01(weaveStrength);
                GritStrength = Mathf.Clamp01(gritStrength);
                LowerDirtStrength = Mathf.Clamp01(lowerDirtStrength);
            }

            public int Size { get; }
            public int StripeCount { get; }
            public int Seed { get; }
            public Color Red { get; }
            public Color Canvas { get; }
            public float Brightness { get; }
            public float SeamStrength { get; }
            public float WeaveStrength { get; }
            public float GritStrength { get; }
            public float LowerDirtStrength { get; }
        }

        public static Texture2D Generate(BigTopEnvironmentSpec spec, RunWorldGenerationConfig config)
        {
            return BigTopTextureUpload.CreateTexture(BuildTextureData(Snapshot(spec, config)));
        }

        public static async UniTask<Texture2D> GenerateAsync(
            BigTopEnvironmentSpec spec,
            RunWorldGenerationConfig config,
            WorldLoadTimingDiagnostics timing = null,
            bool makeNoLongerReadable = true)
        {
            timing ??= WorldLoadTimingDiagnostics.Disabled;
            Settings settings = Snapshot(spec, config);
            BigTopGeneratedTextureData data;
            using (timing.Stage("BigTop.ClothPixelsThreadPool"))
            {
                data = await UniTask.RunOnThreadPool(() => BuildTextureData(settings));
                await UniTask.SwitchToMainThread();
            }

            using (timing.Stage("BigTop.ClothTextureUpload"))
            {
                return BigTopTextureUpload.CreateTexture(data, makeNoLongerReadable: makeNoLongerReadable);
            }
        }

        public static BigTopGeneratedTextureData BuildTextureData(BigTopEnvironmentSpec spec, RunWorldGenerationConfig config)
        {
            return BuildTextureData(Snapshot(spec, config));
        }

        private static Settings Snapshot(BigTopEnvironmentSpec spec, RunWorldGenerationConfig config)
        {
            return new Settings(
                spec.TextureResolution,
                spec.StripeCount,
                spec.Seed,
                config != null ? config.bigTopRedStripeColor : new Color(0.66f, 0.055f, 0.04f, 1f),
                config != null ? config.bigTopCanvasStripeColor : new Color(0.82f, 0.69f, 0.48f, 1f),
                config != null ? config.BigTopBrightness : 0.92f,
                config != null ? config.BigTopSeamStrength : 0.42f,
                config != null ? config.BigTopWeaveStrength : 0.18f,
                config != null ? config.BigTopGritStrength : 0.28f,
                config != null ? config.BigTopLowerDirtStrength : 0.36f);
        }

        private static BigTopGeneratedTextureData BuildTextureData(Settings settings)
        {
            int size = settings.Size;
            int stripeCount = settings.StripeCount;
            var pixels = new Color32[size * size];

            for (int y = 0; y < size; y++)
            {
                float v = y / (float)(size - 1);
                for (int x = 0; x < size; x++)
                {
                    float u = x / (float)(size - 1);
                    float stripePosition = u * stripeCount;
                    int stripeIndex = Mathf.FloorToInt(stripePosition);
                    float stripeLocal = Mathf.Repeat(stripePosition, 1f);
                    Color baseColor = (stripeIndex & 1) == 0 ? settings.Red : settings.Canvas;
                    float seamDistance = Mathf.Min(stripeLocal, 1f - stripeLocal);
                    float seam = 1f - SmoothStep(0.012f, 0.055f, seamDistance);
                    float weave = Weave(x, y, size);
                    float grit = ValueNoise(u * 18f, v * 18f, settings.Seed + 19)
                        * 0.55f
                        + ValueNoise(u * 84f, v * 84f, settings.Seed + 173) * 0.45f;
                    float lowerDirt = Mathf.Pow(1f - v, 2.4f);
                    float dirtAmount = Mathf.Clamp01(grit * settings.GritStrength + lowerDirt * settings.LowerDirtStrength);
                    Color color = Color.Lerp(baseColor, DirtColor, dirtAmount * 0.68f);
                    float shade = settings.Brightness
                        - seam * settings.SeamStrength * 0.22f
                        + weave * settings.WeaveStrength * 0.16f
                        - grit * settings.GritStrength * 0.18f
                        - lowerDirt * settings.LowerDirtStrength * 0.18f;
                    color.r = Mathf.Clamp01(color.r * shade);
                    color.g = Mathf.Clamp01(color.g * shade);
                    color.b = Mathf.Clamp01(color.b * shade);
                    color.a = 1f;
                    pixels[y * size + x] = color;
                }
            }

            return new BigTopGeneratedTextureData(
                "Big Top Procedural Cloth",
                size,
                size,
                pixels,
                TextureWrapMode.Repeat,
                FilterMode.Bilinear);
        }

        private static float Weave(int x, int y, int size)
        {
            float xWave = Mathf.Sin(x * 0.78f) * 0.5f + 0.5f;
            float yWave = Mathf.Sin(y * 1.04f) * 0.5f + 0.5f;
            float broad = Mathf.Sin((x + y) * Mathf.PI / Mathf.Max(8f, size * 0.035f)) * 0.5f + 0.5f;
            return (xWave * 0.38f + yWave * 0.38f + broad * 0.24f) - 0.5f;
        }

        private static float ValueNoise(float x, float y, int seed)
        {
            int x0 = Mathf.FloorToInt(x);
            int y0 = Mathf.FloorToInt(y);
            int x1 = x0 + 1;
            int y1 = y0 + 1;
            float tx = SmoothStep(0f, 1f, x - x0);
            float ty = SmoothStep(0f, 1f, y - y0);
            float a = Hash01(x0, y0, seed);
            float b = Hash01(x1, y0, seed);
            float c = Hash01(x0, y1, seed);
            float d = Hash01(x1, y1, seed);
            return Mathf.Lerp(Mathf.Lerp(a, b, tx), Mathf.Lerp(c, d, tx), ty);
        }

        private static float Hash01(int x, int y, int seed)
        {
            unchecked
            {
                uint h = (uint)seed;
                h ^= (uint)x * 374761393u;
                h = (h << 13) | (h >> 19);
                h ^= (uint)y * 668265263u;
                h *= 1274126177u;
                h ^= h >> 16;
                return (h & 0x00FFFFFF) / 16777215f;
            }
        }

        private static float SmoothStep(float edge0, float edge1, float value)
        {
            float t = Mathf.Clamp01((value - edge0) / Mathf.Max(0.0001f, edge1 - edge0));
            return t * t * (3f - 2f * t);
        }
    }
}
