using System.Collections.Generic;
using TheCircussyOne.Content;
using TheCircussyOne.Visuals;
using UnityEngine;

namespace TheCircussyOne.Runtime
{
    public sealed class HealingPropSystem : IWorldRewardKindSpawner
    {
        private readonly HealingPropCatalog catalog;
        private readonly HealthPickupCatalog healthPickupCatalog;
        private readonly PickupFactory pickupFactory;
        private readonly PlayerView player;
        private readonly WorldPropPlacementService placementService;
        private readonly RunSeedState runSeedState;
        private readonly WorldRewardSpawnPlanner spawnPlanner;
        private readonly WorldRewardRuntimeRegistry runtimeRegistry;
        private readonly RunPauseState pauseState;
        private readonly IWorldRewardTransientVfxSpawner transientVfx;
        private readonly IGameAudio audio;
        private readonly IGameHaptics haptics;
        private static Material runtimeHealingPropMaterial;

        public HealingPropSystem(
            HealingPropCatalog catalog,
            HealthPickupCatalog healthPickupCatalog,
            PickupFactory pickupFactory,
            PlayerView player = null,
            WorldPropPlacementService placementService = null,
            RunSeedState runSeedState = null,
            WorldRewardSpawnPlanner spawnPlanner = null,
            WorldRewardRuntimeRegistry runtimeRegistry = null,
            RunPauseState pauseState = null,
            IWorldRewardTransientVfxSpawner transientVfx = null,
            IGameAudio audio = null,
            IGameHaptics haptics = null)
        {
            this.catalog = catalog;
            this.healthPickupCatalog = healthPickupCatalog;
            this.pickupFactory = pickupFactory;
            this.player = player;
            this.placementService = placementService ?? new WorldPropPlacementService();
            this.runSeedState = runSeedState;
            this.spawnPlanner = spawnPlanner ?? new WorldRewardSpawnPlanner(player: player);
            this.runtimeRegistry = runtimeRegistry;
            this.pauseState = pauseState;
            this.transientVfx = transientVfx;
            this.audio = audio ?? NullGameAudio.Instance;
            this.haptics = haptics ?? NullGameHaptics.Instance;
        }

        public string DiagnosticsName => "HealingProps";

        public void SpawnWorld(int worldIndex)
        {
            if (SceneInteractableRegistry.CountByKind(SceneInteractableKind.HealingProp) > 0 || catalog?.Props == null)
            {
                return;
            }

            IReadOnlyList<WorldRewardSpawnRequest> requests = spawnPlanner.RequestsFor(WorldRewardPlacementKind.HealingProp);

            int sequentialIndex = 0;
            for (int i = 0; i < requests.Count; i++)
            {
                WorldRewardSpawnRequest request = requests[i];
                HealingPropDefinition definition = WorldRewardSpawnPlanner.ResolveContent(
                    catalog.Props,
                    request,
                    ref sequentialIndex);
                if (definition == null)
                {
                    continue;
                }

                CreatePrototypeProp(definition, request.Position, placementService);
            }
        }

        private void CreatePrototypeProp(
            HealingPropDefinition definition,
            Vector3 position,
            WorldPropPlacementService placementService)
        {
            WorldPropPlacementProfile placement = definition != null ? definition.placement : WorldPropPlacementProfile.HealingPropDefault();
            WorldRewardPropBuildResult prop = WorldRewardPropBuilder.Create(
                definition != null ? definition.DisplayName : "Healing Prop",
                position,
                PrimitiveType.Sphere,
                "Healing Prop Visual",
                placement,
                placementService,
                definition != null && definition.visualMaterial != null
                    ? definition.visualMaterial
                    : GetRuntimeHealingPropMaterial());

            var view = prop.Root.AddComponent<HealingPropView>();
            view.Prepare(definition, pickupFactory, healthPickupCatalog?.ResolveDefault(), runSeedState, pauseState, transientVfx, audio, haptics);
            runtimeRegistry?.Register(prop.Root);
        }

        private static Material GetRuntimeHealingPropMaterial()
        {
            if (runtimeHealingPropMaterial != null)
            {
                return runtimeHealingPropMaterial;
            }

            Shader shader = ShaderVisualProperties.FindAllInOneShader()
                ?? Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("Standard");
            if (shader == null)
            {
                return null;
            }

            runtimeHealingPropMaterial = new Material(shader)
            {
                name = "HealingPropRuntimeMaterial",
                hideFlags = HideFlags.DontSave
            };
            ShaderVisualProperties.ConfigureMaterial(
                runtimeHealingPropMaterial,
                Color.white,
                new Color(1f, 0.2f, 0.42f, 1f),
                0.9f,
                outlineEnabled: false);
            return runtimeHealingPropMaterial;
        }
    }
}
