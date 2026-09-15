using System.Collections.Generic;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using TheCircussyOne.Visuals;
using UnityEngine;

namespace TheCircussyOne.Runtime
{
    public sealed class TicketDepositSystem : IWorldRewardKindSpawner
    {
        private readonly TicketDepositCatalog catalog;
        private readonly PlayerView player;
        private readonly WorldPropPlacementService placementService;
        private readonly RunSeedState runSeedState;
        private readonly WorldRewardSpawnPlanner spawnPlanner;
        private readonly WorldRewardRuntimeRegistry runtimeRegistry;
        private readonly RunPauseState pauseState;
        private readonly IWorldRewardTransientVfxSpawner transientVfx;
        private readonly IGameAudio audio;
        private readonly IGameHaptics haptics;
        private static Material runtimeDepositMaterial;

        public TicketDepositSystem(
            TicketDepositCatalog catalog,
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

        public string DiagnosticsName => "TicketDeposits";

        public void SpawnWorld(int worldIndex)
        {
            if (SceneInteractableRegistry.CountByKind(SceneInteractableKind.TicketDeposit) > 0 || catalog?.Deposits == null)
            {
                return;
            }

            IReadOnlyList<WorldRewardSpawnRequest> requests = spawnPlanner.RequestsFor(WorldRewardPlacementKind.TicketDeposit);

            int sequentialIndex = 0;
            for (int i = 0; i < requests.Count; i++)
            {
                WorldRewardSpawnRequest request = requests[i];
                TicketDepositDefinition definition = WorldRewardSpawnPlanner.ResolveContent(
                    catalog.Deposits,
                    request,
                    ref sequentialIndex);
                if (definition == null)
                {
                    continue;
                }

                CreatePrototypeDeposit(definition, request.Position, request.PlacementId, placementService, runSeedState, runtimeRegistry, pauseState, transientVfx, audio, haptics);
            }
        }

        private static void CreatePrototypeDeposit(
            TicketDepositDefinition definition,
            Vector3 position,
            string placementId,
            WorldPropPlacementService placementService,
            RunSeedState runSeedState,
            WorldRewardRuntimeRegistry runtimeRegistry,
            RunPauseState pauseState,
            IWorldRewardTransientVfxSpawner transientVfx,
            IGameAudio audio,
            IGameHaptics haptics)
        {
            WorldPropPlacementProfile placement = definition != null
                ? definition.placement
                : WorldPropPlacementProfile.TicketDepositDefault(TicketDepositTier.Small);
            WorldRewardPropBuildResult prop = WorldRewardPropBuilder.Create(
                definition != null ? definition.DisplayName : "Ticket Deposit",
                position,
                definition != null ? definition.visualPrefab : null,
                definition != null ? definition.visualScale : 1f,
                placement,
                placementService,
                GetRuntimeDepositMaterial(definition),
                PrimitiveType.Cube,
                "Ticket Visual",
                WorldRewardPropColliderMode.Authored,
                ResolveYaw(definition, position, placementId, runSeedState));

            var view = prop.Root.AddComponent<TicketDepositView>();
            view.Prepare(definition, runSeedState, pauseState, transientVfx, audio, haptics);
            runtimeRegistry?.Register(prop.Root);
        }

        private static Material GetRuntimeDepositMaterial(TicketDepositDefinition definition)
        {
            if (runtimeDepositMaterial != null)
            {
                return runtimeDepositMaterial;
            }

            Shader shader = ShaderVisualProperties.FindAllInOneShader()
                ?? Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("Standard");
            if (shader == null)
            {
                return null;
            }

            Color color = definition != null ? definition.visualColor : new Color(1f, 0.86f, 0.22f, 1f);
            Color emission = definition != null ? definition.emissionColor : color;
            float strength = definition != null ? definition.emissionStrength : 0.7f;
            runtimeDepositMaterial = new Material(shader)
            {
                name = "TicketDepositRuntimeMaterial",
                hideFlags = HideFlags.DontSave
            };
            ShaderVisualProperties.ConfigureMaterial(
                runtimeDepositMaterial,
                Color.white,
                emission,
                strength,
                outlineEnabled: false);
            return runtimeDepositMaterial;
        }

        private static float ResolveYaw(TicketDepositDefinition definition, Vector3 position, string placementId, RunSeedState runSeedState)
        {
            string contentId = definition != null ? definition.Id : "ticket_deposit";
            string yawId = string.IsNullOrWhiteSpace(placementId) ? contentId : $"{contentId}:{placementId}";
            int seed = DeterministicSeed.ContentPosition(runSeedState != null ? runSeedState.CurrentSeed : 0, yawId, position);
            return DeterministicSeed.ToFloat01(seed) * 360f;
        }
    }
}
