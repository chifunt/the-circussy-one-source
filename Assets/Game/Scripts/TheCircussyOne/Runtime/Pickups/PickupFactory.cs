using System.Collections.Generic;
using UnityEngine;
using TheCircussyOne.Content;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;
using TheCircussyOne.Visuals;

namespace TheCircussyOne.Runtime
{
    public sealed class PickupFactory
    {
        private readonly GameConfig config;
        private readonly DamageFeedbackVisualConfig feedbackConfig;
        private readonly VfxVisualConfig vfxConfig;
        private readonly ActorRegistry registry;
        private readonly PickupView prefab;
        private readonly Transform root;
        private readonly PickupSurfaceSampler surfaceSampler;
        private readonly Stack<PickupView> pool = new();

        public PickupFactory(GameConfig config, ActorRegistry registry, PickupView prefab, Transform root)
            : this(config, null, registry, prefab, root)
        {
        }

        public PickupFactory(GameConfig config, DamageFeedbackVisualConfig feedbackConfig, ActorRegistry registry, PickupView prefab, Transform root)
            : this(config, feedbackConfig, null, registry, prefab, root)
        {
        }

        public PickupFactory(GameConfig config, DamageFeedbackVisualConfig feedbackConfig, VfxVisualConfig vfxConfig, ActorRegistry registry, PickupView prefab, Transform root)
            : this(config, feedbackConfig, vfxConfig, registry, prefab, root, null)
        {
        }

        public PickupFactory(GameConfig config, DamageFeedbackVisualConfig feedbackConfig, VfxVisualConfig vfxConfig, ActorRegistry registry, PickupView prefab, Transform root, PickupSurfaceSampler surfaceSampler)
        {
            this.config = config;
            this.feedbackConfig = feedbackConfig;
            this.vfxConfig = vfxConfig;
            this.registry = registry;
            this.prefab = prefab;
            this.root = root;
            this.surfaceSampler = surfaceSampler ?? new PickupSurfaceSampler();
        }

        public int PooledCount => pool.Count;

        public int Prewarm(int targetPoolCount, int maxCreate = int.MaxValue)
        {
            if (prefab == null)
            {
                return 0;
            }

            int target = Mathf.Max(0, targetPoolCount);
            int limit = Mathf.Max(0, maxCreate);
            int created = 0;
            while (pool.Count < target && created < limit)
            {
                PickupView view = Object.Instantiate(prefab, root);
                view.Deactivate();
                if (root != null)
                {
                    view.transform.SetParent(root, false);
                }

                pool.Push(view);
                created++;
            }

            return created;
        }

        public PickupRuntime Spawn(Vector3 position)
        {
            PickupView view = GetView();
            view.Prepare(config, feedbackConfig, vfxConfig, BuildImmediateFrame(position), null, 1f);
            var pickup = new PickupRuntime(view, XpGemDefinition.DefaultBlueXpAmount);
            registry.Register(pickup);
            return pickup;
        }

        public PickupRuntime SpawnDrop(XpGemDefinition gem, Vector3 origin, int index, int count, int seed)
        {
            PickupDropFrame dropFrame = BuildDropFrame(origin, index, count, seed, isExperienceDrop: true);
            return Spawn(gem, dropFrame);
        }

        public PickupRuntime SpawnDrop(HealthPickupDefinition pickup, Vector3 origin, int index, int count, int seed)
        {
            PickupDropFrame dropFrame = BuildDropFrame(origin, index, count, seed, isExperienceDrop: false);
            return Spawn(pickup, dropFrame);
        }

        public PickupRuntime Spawn(XpGemDefinition gem, Vector3 position)
        {
            if (gem == null)
            {
                return Spawn(position);
            }

            PickupView view = GetView();
            view.Prepare(config, feedbackConfig, vfxConfig, BuildImmediateFrame(position), gem.color, gem.visualScale, gem.emissionColor, gem.emissionStrength);
            var pickup = new PickupRuntime(view, gem, gem.xpAmount);
            registry.Register(pickup);
            return pickup;
        }

        public PickupRuntime Spawn(HealthPickupDefinition healthPickup, Vector3 position)
        {
            if (healthPickup == null)
            {
                return Spawn(position);
            }

            PickupView view = GetView();
            view.Prepare(
                config,
                feedbackConfig,
                vfxConfig,
                BuildImmediateFrame(position),
                healthPickup.color,
                healthPickup.visualScale,
                healthPickup.emissionColor,
                healthPickup.emissionStrength,
                healthPickup.visualShape);
            var pickup = new PickupRuntime(view, healthPickup, healthPickup.healAmount);
            registry.Register(pickup);
            return pickup;
        }

        public PickupRuntime Spawn(XpGemDefinition gem, PickupDropFrame dropFrame)
        {
            if (gem == null)
            {
                PickupView fallbackView = GetView();
                fallbackView.Prepare(config, feedbackConfig, vfxConfig, dropFrame, null, 1f);
                var fallbackPickup = new PickupRuntime(fallbackView, XpGemDefinition.DefaultBlueXpAmount);
                registry.Register(fallbackPickup);
                return fallbackPickup;
            }

            PickupView view = GetView();
            view.Prepare(config, feedbackConfig, vfxConfig, dropFrame, gem.color, gem.visualScale, gem.emissionColor, gem.emissionStrength);
            var pickup = new PickupRuntime(view, gem, gem.xpAmount);
            registry.Register(pickup);
            return pickup;
        }

        public PickupRuntime Spawn(HealthPickupDefinition healthPickup, PickupDropFrame dropFrame)
        {
            if (healthPickup == null)
            {
                return Spawn(dropFrame.LandingPosition);
            }

            PickupView view = GetView();
            view.Prepare(
                config,
                feedbackConfig,
                vfxConfig,
                dropFrame,
                healthPickup.color,
                healthPickup.visualScale,
                healthPickup.emissionColor,
                healthPickup.emissionStrength,
                healthPickup.visualShape);
            var pickup = new PickupRuntime(view, healthPickup, healthPickup.healAmount);
            registry.Register(pickup);
            return pickup;
        }

        public void Despawn(PickupRuntime pickup)
        {
            if (pickup == null)
            {
                return;
            }

            registry.Unregister(pickup);
            PickupView view = pickup.View;
            if (view == null)
            {
                return;
            }

            view.Deactivate();
            view.transform.SetParent(root, false);
            pool.Push(view);
        }

        private PickupView GetView()
        {
            if (pool.Count > 0)
            {
                return pool.Pop();
            }

            return Object.Instantiate(prefab, root);
        }

        private PickupDropFrame BuildDropFrame(Vector3 origin, int index, int count, int seed, bool isExperienceDrop)
        {
            if (feedbackConfig == null)
            {
                return PickupDropFrame.Immediate(origin);
            }

            float scatterRadius = Mathf.Max(0f, feedbackConfig.pickupDropBurstDistance);
            int mask = PickupEnvironmentMask();
            Vector3 burstCenter = ResolveTossStart(origin, mask);
            int scatterCount = PickupDropPlacementRules.PresentationScatterCount(count, isExperienceDrop);
            Vector3 start = burstCenter + PickupDropPlacementRules.LaunchOffset(
                index,
                scatterCount,
                seed,
                feedbackConfig.pickupCollisionRadius);
            Vector3 landing = ResolveLanding(burstCenter, index, scatterCount, seed, scatterRadius, mask);
            return new PickupDropFrame(
                start,
                landing,
                feedbackConfig.pickupDropBurstSeconds,
                PickupDropPlacementRules.ArcHeight(
                    feedbackConfig.pickupDropArcHeight,
                    feedbackConfig.pickupXpDropArcHeightMultiplier,
                    isExperienceDrop),
                feedbackConfig.pickupDropBounceHeight,
                feedbackConfig.pickupDropBounceCount,
                feedbackConfig.pickupDropSettleDelaySeconds,
                feedbackConfig.pickupDropBurstEase);
        }

        private PickupDropFrame BuildImmediateFrame(Vector3 position)
        {
            if (feedbackConfig == null)
            {
                return PickupDropFrame.Immediate(position);
            }

            return PickupDropFrame.Immediate(ResolveGroundedPosition(position, PickupEnvironmentMask()));
        }

        private Vector3 ResolveLanding(Vector3 origin, int index, int count, int seed, float scatterRadius, int mask)
        {
            const int Attempts = 8;
            for (int attempt = 0; attempt < Attempts; attempt++)
            {
                int attemptIndex = index + attempt * Mathf.Max(1, count);
                float attemptRadius = scatterRadius * Mathf.Max(0.25f, 1f - attempt * 0.12f);
                Vector3 offset = PickupDropPlacementRules.ScatterOffset(attemptIndex, Mathf.Max(1, count), seed + attempt * 17, attemptRadius);
                Vector3 candidate = origin + offset;
                if (!surfaceSampler.TrySampleGround(
                        candidate,
                        feedbackConfig.pickupGroundProbeHeight,
                        feedbackConfig.pickupGroundProbeDepth,
                        mask,
                        out float surfaceY))
                {
                    continue;
                }

                Vector3 landing = PickupDropPlacementRules.AboveSurface(candidate, surfaceY, feedbackConfig.pickupGroundClearance);
                if (!surfaceSampler.IsLandingClear(landing, feedbackConfig.pickupCollisionRadius, mask))
                {
                    continue;
                }

                if (!surfaceSampler.HasClearPath(origin, landing, feedbackConfig.pickupCollisionRadius, mask))
                {
                    continue;
                }

                return landing;
            }

            if (surfaceSampler.TrySampleGround(
                    origin,
                    feedbackConfig.pickupGroundProbeHeight,
                    feedbackConfig.pickupGroundProbeDepth,
                    mask,
                    out float fallbackSurfaceY))
            {
                return PickupDropPlacementRules.AboveSurface(origin, fallbackSurfaceY, feedbackConfig.pickupGroundClearance);
            }

            return PickupDropPlacementRules.SafeFallback(origin, feedbackConfig.pickupGroundClearance);
        }

        private Vector3 ResolveGroundedPosition(Vector3 position, int mask)
        {
            if (surfaceSampler.TrySampleGround(
                    position,
                    feedbackConfig.pickupGroundProbeHeight,
                    feedbackConfig.pickupGroundProbeDepth,
                    mask,
                    out float surfaceY))
            {
                return PickupDropPlacementRules.AboveSurface(position, surfaceY, feedbackConfig.pickupGroundClearance);
            }

            return PickupDropPlacementRules.SafeFallback(position, feedbackConfig.pickupGroundClearance);
        }

        private Vector3 ResolveTossStart(Vector3 origin, int mask)
        {
            if (!surfaceSampler.TrySampleGround(
                    origin,
                    feedbackConfig.pickupGroundProbeHeight,
                    feedbackConfig.pickupGroundProbeDepth,
                    mask,
                    out float surfaceY))
            {
                return PickupDropPlacementRules.SafeFallback(origin, feedbackConfig.pickupGroundClearance);
            }

            float minimumY = surfaceY + Mathf.Max(0f, feedbackConfig.pickupGroundClearance);
            return new Vector3(origin.x, Mathf.Max(origin.y, minimumY), origin.z);
        }

        private int PickupEnvironmentMask()
        {
            int configuredMask = feedbackConfig != null ? feedbackConfig.pickupEnvironmentMask.value : ~0;
            return configuredMask & GameLayers.EnvironmentMaskExcludingGameplay;
        }
    }
}
