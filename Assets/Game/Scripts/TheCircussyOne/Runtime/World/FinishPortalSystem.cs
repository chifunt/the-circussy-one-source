using System;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;
using TheCircussyOne.Stats;
using TheCircussyOne.Visuals;
using UnityEngine;
using VContainer.Unity;

namespace TheCircussyOne.Runtime
{
    public sealed class FinishPortalSystem : IStartable, IDisposable
    {
        private readonly RunPhaseState phaseState;
        private readonly RunActScheduleState scheduleState;
        private readonly PlayerView player;
        private readonly RunScheduleConfig scheduleConfig;
        private readonly GameConfig gameConfig;
        private readonly RunStats runStats;
        private readonly WorldSurfaceResolver surfaceResolver;
        private readonly FinishPortalFactory portalFactory;
        private readonly RunWorldGenerationState generationState;
        private readonly HeadlinerDefeatState headlinerDefeatState;
        private readonly IGameAudio audio;
        private readonly IGameHaptics haptics;
        private FinishPortalView portal;

        public FinishPortalSystem(
            RunPhaseState phaseState,
            RunActScheduleState scheduleState = null,
            PlayerView player = null,
            RunScheduleConfig scheduleConfig = null,
            GameConfig gameConfig = null,
            RunStats runStats = null,
            WorldSurfaceResolver surfaceResolver = null,
            FinishPortalFactory portalFactory = null,
            RunWorldGenerationState generationState = null,
            HeadlinerDefeatState headlinerDefeatState = null,
            IGameAudio audio = null,
            IGameHaptics haptics = null)
        {
            this.phaseState = phaseState;
            this.scheduleState = scheduleState;
            this.player = player;
            this.scheduleConfig = scheduleConfig != null ? scheduleConfig : RunScheduleConfig.CreateRuntimeDefault();
            this.gameConfig = gameConfig;
            this.runStats = runStats;
            this.surfaceResolver = surfaceResolver ?? new WorldSurfaceResolver();
            this.portalFactory = portalFactory ?? new FinishPortalFactory();
            this.generationState = generationState;
            this.headlinerDefeatState = headlinerDefeatState;
            this.audio = audio ?? NullGameAudio.Instance;
            this.haptics = haptics ?? NullGameHaptics.Instance;
            this.scheduleConfig.EnsureWorkflowDefaults();
        }

        public FinishPortalView ActivePortal => portal != null && portal.gameObject.activeSelf ? portal : null;

        public void ClearWorld()
        {
            HidePortal();
        }

        public void Start()
        {
            if (phaseState == null)
            {
                return;
            }

            phaseState.PhaseChanged += OnPhaseChanged;
            if (headlinerDefeatState != null)
            {
                headlinerDefeatState.DeathPositionRecorded += OnHeadlinerDeathPositionRecorded;
            }

            if (phaseState.Is(RunPhase.Encore))
            {
                ShowPortalIfAllowed();
            }
        }

        public void Dispose()
        {
            if (phaseState != null)
            {
                phaseState.PhaseChanged -= OnPhaseChanged;
            }

            if (headlinerDefeatState != null)
            {
                headlinerDefeatState.DeathPositionRecorded -= OnHeadlinerDeathPositionRecorded;
            }

            if (portal != null)
            {
                portalFactory.Release(portal);
                portal = null;
            }
        }

        private void OnPhaseChanged(RunPhase previous, RunPhase next)
        {
            if (next == RunPhase.Encore)
            {
                if (previous == RunPhase.BossDefeated)
                {
                    // A defeated Headliner must never leave the run without an exit,
                    // even if an upstream proxy or debug path omitted its death anchor.
                    ShowPortal();
                }
                else
                {
                    ShowPortalIfAllowed();
                }

                return;
            }

            if (!RunPhaseRules.ShouldAllowInteractions(next))
            {
                HidePortal();
            }
        }

        private void OnHeadlinerDeathPositionRecorded(Vector3 _)
        {
            if (phaseState != null && phaseState.Is(RunPhase.Encore))
            {
                ShowPortal();
            }
        }

        private void ShowPortalIfAllowed()
        {
            if (headlinerDefeatState != null && !headlinerDefeatState.HasDeathPosition)
            {
                return;
            }

            ShowPortal();
        }

        private void ShowPortal()
        {
            FinishPortalView view = ResolvePortal();
            if (view == null)
            {
                return;
            }

            StageDoorPlacement placement = ResolveSpawnPlacement();
            view.transform.SetPositionAndRotation(placement.Position, placement.Rotation);
            view.gameObject.SetActive(true);
            view.Prepare(phaseState, scheduleState == null || !scheduleState.HasNextAct, audio, haptics);
        }

        private void HidePortal()
        {
            if (portal != null)
            {
                portal.gameObject.SetActive(false);
            }
        }

        private FinishPortalView ResolvePortal()
        {
            if (portal != null)
            {
                return portal;
            }

            portal = portalFactory.CreatePortal();
            return portal;
        }

        private StageDoorPlacement ResolveSpawnPlacement()
        {
            if (headlinerDefeatState != null
                && headlinerDefeatState.TryGetDeathPosition(out Vector3 deathPosition)
                && TryResolveGroundedPlacement(deathPosition, out StageDoorPlacement deathPlacement))
            {
                return deathPlacement;
            }

            Vector3 desired;
            if (generationState != null && generationState.TryGetStageDoorAnchor(out Vector3 stageDoorAnchor))
            {
                desired = stageDoorAnchor;
            }
            else if (player == null)
            {
                return new StageDoorPlacement(new Vector3(0f, 0.05f, 4f), Quaternion.identity);
            }
            else
            {
                Vector3 forward = player.Body != null ? player.Body.forward : Vector3.forward;
                forward.y = 0f;
                if (forward.sqrMagnitude < 0.0001f)
                {
                    forward = Vector3.forward;
                }

                forward.Normalize();
                float distance = ResolveStageDoorDistance();
                desired = player.Position + forward * distance;
            }

            if (TryResolveGroundedPlacement(desired, out StageDoorPlacement groundedPlacement))
            {
                return groundedPlacement;
            }

            float clearance = scheduleConfig != null ? Mathf.Max(0f, scheduleConfig.stageDoorGroundClearance) : 0.04f;
            float fallbackY = player != null ? player.Position.y + clearance : clearance;
            desired.y = Mathf.Max(clearance, fallbackY);
            return new StageDoorPlacement(desired, Quaternion.identity);
        }

        private bool TryResolveGroundedPlacement(Vector3 desired, out StageDoorPlacement placement)
        {
            WorldSurfaceSample sample = surfaceResolver.Resolve(
                desired,
                scheduleConfig != null ? scheduleConfig.stageDoorProbeHeight : 16f,
                scheduleConfig != null ? scheduleConfig.stageDoorProbeDepth : 32f,
                GameLayers.EnvironmentMaskExcludingGameplay);

            float clearance = scheduleConfig != null ? Mathf.Max(0f, scheduleConfig.stageDoorGroundClearance) : 0.04f;
            if (sample.FoundSurface)
            {
                placement = new StageDoorPlacement(
                    sample.Position + sample.Normal * clearance,
                    Quaternion.FromToRotation(Vector3.up, sample.Normal));
                return true;
            }

            placement = default;
            return false;
        }

        private float ResolveStageDoorDistance()
        {
            int actNumber = scheduleState != null && scheduleState.HasActiveAct
                ? scheduleState.ActNumber
                : phaseState?.WorldIndex ?? 1;
            RunStageDoorTravelDefinition travel = scheduleConfig != null
                ? scheduleConfig.GetStageDoorTravelForAct(actNumber)
                : null;
            float travelSeconds = travel != null ? travel.MidpointSeconds : 20f;
            // Portal distance is expressed as expected travel time so balancing stays consistent across speed upgrades.
            return Mathf.Max(2f, travelSeconds * ResolvePlayerTravelSpeed());
        }

        private float ResolvePlayerTravelSpeed()
        {
            float baseSpeed = gameConfig != null ? gameConfig.playerRunSpeed : 8f;
            float multiplier = runStats != null ? runStats.GetFloat(StatId.PlayerMoveSpeedMultiplier) : 1f;
            return Mathf.Max(1f, baseSpeed * Mathf.Max(0.1f, multiplier));
        }

        private readonly struct StageDoorPlacement
        {
            public StageDoorPlacement(Vector3 position, Quaternion rotation)
            {
                Position = position;
                Rotation = rotation;
            }

            public Vector3 Position { get; }
            public Quaternion Rotation { get; }
        }
    }
}
