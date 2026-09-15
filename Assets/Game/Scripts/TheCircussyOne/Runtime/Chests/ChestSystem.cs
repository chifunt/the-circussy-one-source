using System.Collections.Generic;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using TheCircussyOne.Stats;
using TheCircussyOne.Visuals;
using UnityEngine;

namespace TheCircussyOne.Runtime
{
    public sealed class ChestSystem : IWorldRewardKindSpawner
    {
        private readonly ChestCatalog chestCatalog;
        private readonly ItemCatalog itemCatalog;
        private readonly ItemInventory itemInventory;
        private readonly RunStats stats;
        private readonly PlayerView player;
        private readonly RunPauseState pauseState;
        private readonly WorldPropPlacementService placementService;
        private readonly RunSeedState runSeedState;
        private readonly WorldRewardSpawnPlanner spawnPlanner;
        private readonly WorldRewardRuntimeRegistry runtimeRegistry;
        private readonly IGameAudio audio;
        private readonly IGameHaptics haptics;
        private static Material runtimeChestMaterial;

        public ChestSystem(
            ChestCatalog chestCatalog,
            ItemCatalog itemCatalog,
            ItemInventory itemInventory,
            RunStats stats,
            PlayerView player = null,
            RunPauseState pauseState = null,
            WorldPropPlacementService placementService = null,
            RunSeedState runSeedState = null,
            WorldRewardSpawnPlanner spawnPlanner = null,
            WorldRewardRuntimeRegistry runtimeRegistry = null,
            IGameAudio audio = null,
            IGameHaptics haptics = null)
        {
            this.chestCatalog = chestCatalog;
            this.itemCatalog = itemCatalog;
            this.itemInventory = itemInventory;
            this.stats = stats;
            this.player = player;
            this.pauseState = pauseState;
            this.placementService = placementService ?? new WorldPropPlacementService();
            this.runSeedState = runSeedState;
            this.spawnPlanner = spawnPlanner ?? new WorldRewardSpawnPlanner(player: player);
            this.runtimeRegistry = runtimeRegistry;
            this.audio = audio ?? NullGameAudio.Instance;
            this.haptics = haptics ?? NullGameHaptics.Instance;
        }

        public string DiagnosticsName => "Chests";

        public void SpawnWorld(int worldIndex)
        {
            if (SceneInteractableRegistry.CountByKind(SceneInteractableKind.Chest) > 0 || chestCatalog?.Chests == null)
            {
                return;
            }

            IReadOnlyList<WorldRewardSpawnRequest> requests = spawnPlanner.RequestsFor(WorldRewardPlacementKind.Chest);

            int sequentialIndex = 0;
            for (int i = 0; i < requests.Count; i++)
            {
                WorldRewardSpawnRequest request = requests[i];
                ChestDefinition definition = WorldRewardSpawnPlanner.ResolveContent(
                    chestCatalog.Chests,
                    request,
                    ref sequentialIndex,
                    ChestWorldSpawnRules.CanSpawnInWorld);
                if (definition == null)
                {
                    continue;
                }

                CreatePrototypeChest(definition, itemCatalog, itemInventory, stats, request.Position, request.PlacementId, pauseState, placementService, runSeedState, runtimeRegistry, audio, haptics);
            }
        }

        private static void CreatePrototypeChest(
            ChestDefinition definition,
            ItemCatalog itemCatalog,
            ItemInventory itemInventory,
            RunStats stats,
            Vector3 position,
            string placementId,
            RunPauseState pauseState,
            WorldPropPlacementService placementService,
            RunSeedState runSeedState,
            WorldRewardRuntimeRegistry runtimeRegistry,
            IGameAudio audio,
            IGameHaptics haptics)
        {
            WorldPropPlacementProfile placement = definition != null ? definition.placement : WorldPropPlacementProfile.ChestDefault();
            WorldRewardPropBuildResult prop = WorldRewardPropBuilder.Create(
                definition != null ? definition.DisplayName : "Chest",
                position,
                definition != null ? definition.visualPrefab : null,
                definition != null ? definition.visualScale : 1f,
                placement,
                placementService,
                GetRuntimeChestMaterial(),
                PrimitiveType.Cube,
                "Chest Body",
                WorldRewardPropColliderMode.Authored,
                ResolveYaw(definition, position, placementId, runSeedState));

            CreateFallbackLidIfNeeded(definition, placement, prop.VisualRoot);

            var view = prop.Root.AddComponent<ChestView>();
            view.Prepare(definition, itemCatalog, itemInventory, stats, pauseState, runSeedState, audio, haptics);
            runtimeRegistry?.Register(prop.Root);
        }

        private static void CreateFallbackLidIfNeeded(
            ChestDefinition definition,
            WorldPropPlacementProfile placement,
            GameObject visualRoot)
        {
            if (definition != null && definition.visualPrefab != null)
            {
                return;
            }

            var lid = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lid.name = "Lid";
            lid.transform.SetParent(visualRoot.transform, worldPositionStays: false);
            lid.transform.localPosition = Vector3.Scale(new Vector3(0f, 0.58f, 0f), placement.bodyColliderSize);
            lid.transform.localScale = Vector3.Scale(new Vector3(1.08f, 0.22f, 1.08f), placement.bodyColliderSize);
            Collider lidCollider = lid.GetComponent<Collider>();
            if (lidCollider != null)
            {
                lidCollider.enabled = false;
            }

            Renderer lidRenderer = lid.GetComponent<Renderer>();
            if (lidRenderer != null)
            {
                lidRenderer.sharedMaterial = GetRuntimeChestMaterial();
            }
        }

        private static Material GetRuntimeChestMaterial()
        {
            if (runtimeChestMaterial != null)
            {
                return runtimeChestMaterial;
            }

            Shader shader = ShaderVisualProperties.FindAllInOneShader()
                ?? Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("Standard");
            if (shader == null)
            {
                return null;
            }

            runtimeChestMaterial = new Material(shader)
            {
                name = "ChestRuntimeMaterial",
                hideFlags = HideFlags.DontSave
            };
            ShaderVisualProperties.ConfigureMaterial(
                runtimeChestMaterial,
                Color.white,
                new Color(1f, 0.78f, 0.26f, 1f),
                0.35f,
                outlineEnabled: false);
            return runtimeChestMaterial;
        }

        private static float ResolveYaw(ChestDefinition definition, Vector3 position, string placementId, RunSeedState runSeedState)
        {
            string contentId = definition != null ? definition.Id : "chest";
            string yawId = string.IsNullOrWhiteSpace(placementId) ? contentId : $"{contentId}:{placementId}";
            int seed = DeterministicSeed.ContentPosition(runSeedState != null ? runSeedState.CurrentSeed : 0, yawId, position);
            return DeterministicSeed.ToFloat01(seed) * 360f;
        }
    }
}
