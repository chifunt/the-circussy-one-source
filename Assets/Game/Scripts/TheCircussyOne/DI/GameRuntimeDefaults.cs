using System.Collections.Generic;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Runtime;
using TheCircussyOne.Visuals;
using UnityEngine;

namespace TheCircussyOne.DI
{
    public sealed class GameRuntimeDefaults
    {
        private readonly VfxVisualConfig vfxConfig;
        private readonly GameFallbackCatalogFactory fallbackCatalogs;
        private bool ownsWorldInteractionPromptRoot;
        private bool ownsWorldInteractionPromptPrefab;
        private bool ownsWorldAmbientDustPrefab;
        private readonly List<ParticleEffectView> ownedParticleEffectPrefabs = new();
        private readonly List<Object> ownedParticleResources = new();

        public GameRuntimeDefaults(GameConfig gameConfig, VfxVisualConfig vfxConfig)
        {
            this.vfxConfig = vfxConfig;
            fallbackCatalogs = new GameFallbackCatalogFactory(gameConfig);
        }

        public ActorMotionVisualConfig ResolveActorMotionVisualConfig(ActorMotionVisualConfig config)
        {
            if (config != null)
            {
                return config;
            }

            var fallback = ScriptableObject.CreateInstance<ActorMotionVisualConfig>();
            fallback.EnsureWorkflowDefaults();
            return fallback;
        }

        public WorldInteractionPromptVisualConfig ResolveWorldInteractionPromptVisualConfig(WorldInteractionPromptVisualConfig config)
        {
            if (config != null)
            {
                return config;
            }

            var fallback = ScriptableObject.CreateInstance<WorldInteractionPromptVisualConfig>();
            fallback.EnsureWorkflowDefaults();
            return fallback;
        }

        public RunScheduleConfig ResolveRunScheduleConfig(RunScheduleConfig config)
        {
            if (config != null)
            {
                config.EnsureWorkflowDefaults();
                return config;
            }

            return RunScheduleConfig.CreateRuntimeDefault();
        }

        public RunWorldGenerationConfig ResolveRunWorldGenerationConfig(RunWorldGenerationConfig config)
        {
            if (config != null)
            {
                config.EnsureWorkflowDefaults();
                return config;
            }

            return RunWorldGenerationConfig.CreateRuntimeDefault();
        }

        public WeaponCatalog ResolveWeaponCatalog(WeaponCatalog catalog) => catalog != null ? catalog : fallbackCatalogs.CreateWeaponCatalog();
        public ItemCatalog ResolveItemCatalog(ItemCatalog catalog) => catalog != null ? catalog : fallbackCatalogs.CreateItemCatalog();
        public ChestCatalog ResolveChestCatalog(ChestCatalog catalog) => catalog != null ? catalog : fallbackCatalogs.CreateChestCatalog();
        public TicketDepositCatalog ResolveTicketDepositCatalog(TicketDepositCatalog catalog) => catalog != null ? catalog : fallbackCatalogs.CreateTicketDepositCatalog();
        public WorldRewardPlacementCatalog ResolveWorldRewardPlacementCatalog(WorldRewardPlacementCatalog catalog) => catalog != null ? catalog : fallbackCatalogs.CreateWorldRewardPlacementCatalog();
        public WorldDecorationCatalog ResolveWorldDecorationCatalog(WorldDecorationCatalog catalog) => catalog != null ? catalog : fallbackCatalogs.CreateWorldDecorationCatalog();
        public XpGemCatalog ResolveXpGemCatalog(XpGemCatalog catalog) => catalog != null ? catalog : fallbackCatalogs.CreateXpGemCatalog();
        public HealthPickupCatalog ResolveHealthPickupCatalog(HealthPickupCatalog catalog) => catalog != null ? catalog : fallbackCatalogs.CreateHealthPickupCatalog();
        public HealingPropCatalog ResolveHealingPropCatalog(HealingPropCatalog catalog, HealthPickupCatalog activeHealthPickupCatalog) => catalog != null ? catalog : fallbackCatalogs.CreateHealingPropCatalog(activeHealthPickupCatalog);
        public UpgradeCatalog ResolveUpgradeCatalog(UpgradeCatalog catalog) => catalog != null ? catalog : fallbackCatalogs.CreateUpgradeCatalog();
        public TalentCatalog ResolveTalentCatalog(TalentCatalog catalog) => catalog != null ? catalog : fallbackCatalogs.CreateTalentCatalog();
        public PerformerCatalog ResolvePerformerCatalog(PerformerCatalog catalog, WeaponCatalog activeWeaponCatalog) => catalog != null ? catalog : fallbackCatalogs.CreatePerformerCatalog(activeWeaponCatalog);
        public EnemyCatalog ResolveEnemyCatalog(EnemyCatalog catalog, XpGemCatalog activeXpGemCatalog) => catalog != null ? catalog : fallbackCatalogs.CreateEnemyCatalog(activeXpGemCatalog);
        public HeadlinerCatalog ResolveHeadlinerCatalog(HeadlinerCatalog catalog, EnemyCatalog activeEnemyCatalog) => catalog != null ? catalog : fallbackCatalogs.CreateHeadlinerCatalog(activeEnemyCatalog);

        public Transform ResolveRuntimeRoot(Transform root, string rootName)
        {
            return root != null ? root : CreateRuntimeRoot(rootName);
        }

        public Transform ResolveWorldInteractionPromptRoot(Transform root)
        {
            if (root != null)
            {
                return root;
            }

            ownsWorldInteractionPromptRoot = true;
            return CreateRuntimeRoot("World Interaction Prompts");
        }

        public OrbitWeaponView ResolveFireHoopOrbitPrefab(OrbitWeaponView prefab, WeaponCatalog activeWeaponCatalog)
        {
            return prefab != null ? prefab : CreateRuntimeOrbitPrefab(activeWeaponCatalog);
        }

        public FireHoopAreaTelegraphView ResolveFireHoopAreaTelegraphPrefab(FireHoopAreaTelegraphView prefab)
        {
            return prefab != null ? prefab : CreateRuntimeFireHoopAreaTelegraphPrefab(vfxConfig);
        }

        public WorldAmbientDustView ResolveWorldAmbientDustPrefab(WorldAmbientDustView prefab)
        {
            if (prefab != null)
            {
                return prefab;
            }

            ownsWorldAmbientDustPrefab = true;
            return CreateRuntimeWorldAmbientDustPrefab(vfxConfig);
        }

        public ParticleEffectView ResolveParticleEffectPrefab(ParticleEffectView prefab, VfxEffectId effectId, string prefabName)
        {
            if (prefab != null)
            {
                return prefab;
            }

            ParticleEffectView runtimePrefab = CreateRuntimeParticleEffectPrefab(vfxConfig, effectId, prefabName);
            ownedParticleEffectPrefabs.Add(runtimePrefab);
            return runtimePrefab;
        }

        public WorldInteractionPromptView ResolveWorldInteractionPromptPrefab(WorldInteractionPromptView prefab, WorldInteractionPromptVisualConfig config)
        {
            if (prefab != null)
            {
                return prefab;
            }

            ownsWorldInteractionPromptPrefab = true;
            return CreateRuntimeWorldInteractionPromptPrefab(config);
        }

        public T ResolveHudComponent<T>(T component, HudView hud)
            where T : Component
        {
            if (component != null || hud == null)
            {
                return component;
            }

            T existing = hud.GetComponent<T>();
            return existing != null ? existing : hud.gameObject.AddComponent<T>();
        }

        public void DestroyOwnedWorldInteractionPromptObjects(ref Transform root, ref WorldInteractionPromptView prefab)
        {
            if (ownsWorldInteractionPromptPrefab && prefab != null)
            {
                DestroyPromptObject(prefab.gameObject);
                prefab = null;
                ownsWorldInteractionPromptPrefab = false;
            }

            if (ownsWorldInteractionPromptRoot && root != null)
            {
                DestroyPromptObject(root.gameObject);
                root = null;
                ownsWorldInteractionPromptRoot = false;
            }
        }

        public void DestroyOwnedWorldAmbientDustPrefab(ref WorldAmbientDustView prefab)
        {
            if (!ownsWorldAmbientDustPrefab || prefab == null)
            {
                return;
            }

            DestroyPromptObject(prefab.gameObject);
            prefab = null;
            ownsWorldAmbientDustPrefab = false;
        }

        public void DestroyOwnedParticleEffectPrefabs()
        {
            for (int i = 0; i < ownedParticleEffectPrefabs.Count; i++)
            {
                ParticleEffectView prefab = ownedParticleEffectPrefabs[i];
                if (prefab != null)
                {
                    DestroyPromptObject(prefab.gameObject);
                }
            }

            ownedParticleEffectPrefabs.Clear();

            for (int i = 0; i < ownedParticleResources.Count; i++)
            {
                RuntimeObjectFactory.Release(ownedParticleResources[i]);
            }

            ownedParticleResources.Clear();
        }

        private static Transform CreateRuntimeRoot(string rootName)
        {
            var root = new GameObject(rootName);
            root.hideFlags = HideFlags.DontSave;
            return root.transform;
        }

        public void DestroyOrphanedWorldInteractionPromptViews()
        {
            WorldInteractionPromptView[] promptViews = Object.FindObjectsByType<WorldInteractionPromptView>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < promptViews.Length; i++)
            {
                WorldInteractionPromptView view = promptViews[i];
                if (view == null || !view.gameObject.scene.IsValid())
                {
                    continue;
                }

                DestroyPromptObject(view.gameObject);
            }
        }

        private static void DestroyPromptObject(GameObject target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Object.Destroy(target);
            }
            else
            {
                Object.DestroyImmediate(target);
            }
        }

        private static OrbitWeaponView CreateRuntimeOrbitPrefab(WeaponCatalog catalog)
        {
            var prefab = new GameObject("Fire Hoop Orbit Runtime Prefab");
            prefab.hideFlags = HideFlags.DontSave;
            var view = prefab.AddComponent<OrbitWeaponView>();
            view.ConfigureDefaults(GameFallbackCatalogFactory.FindWeapon(catalog, FirstPartyWeaponDefaults.FireHoopId));
            prefab.SetActive(false);
            return view;
        }

        private static FireHoopAreaTelegraphView CreateRuntimeFireHoopAreaTelegraphPrefab(VfxVisualConfig config)
        {
            var prefab = new GameObject("Fire Hoop Area Telegraph Runtime Prefab");
            prefab.hideFlags = HideFlags.DontSave;
            var view = prefab.AddComponent<FireHoopAreaTelegraphView>();
            view.ConfigureDefaults(config);
            prefab.SetActive(false);
            return view;
        }

        private static WorldAmbientDustView CreateRuntimeWorldAmbientDustPrefab(VfxVisualConfig config)
        {
            var prefab = new GameObject("World Ambient Dust Runtime Prefab");
            prefab.hideFlags = HideFlags.DontSave;
            var view = prefab.AddComponent<WorldAmbientDustView>();
            view.ConfigureDefaults(config);
            prefab.SetActive(false);
            return view;
        }

        private static WorldInteractionPromptView CreateRuntimeWorldInteractionPromptPrefab(WorldInteractionPromptVisualConfig config)
        {
            var prefab = new GameObject("World Interaction Prompt Runtime Prefab");
            prefab.hideFlags = HideFlags.DontSave;
            var view = prefab.AddComponent<WorldInteractionPromptView>();
            view.ConfigureDefaults(config);
            prefab.SetActive(false);
            return view;
        }

        private ParticleEffectView CreateRuntimeParticleEffectPrefab(VfxVisualConfig config, VfxEffectId effectId, string prefabName)
        {
            var prefab = new GameObject(string.IsNullOrWhiteSpace(prefabName) ? $"{effectId} Runtime Prefab" : prefabName);
            prefab.hideFlags = HideFlags.DontSave;
            var particles = prefab.AddComponent<ParticleSystem>();
            VfxOneShotSettings settings = config != null ? config.SettingsFor(effectId) : null;
            ConfigureRuntimeParticleSystem(particles, settings, effectId);
            var view = prefab.AddComponent<ParticleEffectView>();
            view.Configure(effectId);
            prefab.SetActive(false);
            return view;
        }

        private void ConfigureRuntimeParticleSystem(ParticleSystem particles, VfxOneShotSettings settings, VfxEffectId effectId)
        {
            settings ??= VfxOneShotSettings.Default(Color.white);
            settings.EnsureDefaults();

            particles.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particles.Clear(withChildren: true);

            var main = particles.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = Mathf.Max(0.03f, settings.duration);
            main.startLifetime = Mathf.Max(0.02f, settings.lifetime);
            main.startSize = Mathf.Max(0.005f, settings.startSize);
            main.startSpeed = Mathf.Max(0f, settings.speed);
            main.startColor = settings.color;
            main.gravityModifier = settings.gravityModifier;
            main.maxParticles = Mathf.Max(1, settings.maxParticles);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;

            if (effectId == VfxEffectId.ProjectileBounceBurst)
            {
                main.startColor = new ParticleSystem.MinMaxGradient(
                    new Color(0.18f, 0.88f, 1f, 0.92f),
                    new Color(1f, 0.32f, 0.86f, 0.92f));
            }
            else if (effectId == VfxEffectId.KnifeHitSparks)
            {
                main.startRotation = new ParticleSystem.MinMaxCurve(-0.75f, 0.75f);
            }

            var emission = particles.emission;
            emission.enabled = true;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)Mathf.Max(1, settings.burstCount)) });

            var shape = particles.shape;
            shape.enabled = true;
            shape.shapeType = effectId == VfxEffectId.KnifeHitSparks ? ParticleSystemShapeType.Cone : ParticleSystemShapeType.Sphere;
            shape.radius = Mathf.Max(0.01f, settings.shapeRadius);
            shape.radiusThickness = 0.15f;
            if (effectId == VfxEffectId.KnifeHitSparks)
            {
                shape.angle = 18f;
                shape.radius = 0.025f;
                shape.radiusThickness = 1f;
            }

            var colorOverLifetime = particles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
            colorOverLifetime.color = gradient;

            var sizeOverLifetime = particles.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(0.18f, 1.15f),
                new Keyframe(1f, 0.10f)));

            var renderer = particles.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.sortingOrder = settings.sortingOrder;

            VfxParticleTextureKind textureKind = effectId switch
            {
                VfxEffectId.KnifeHitSparks => VfxParticleTextureKind.SlashSpark,
                VfxEffectId.ProjectileBounceBurst => VfxParticleTextureKind.SparkDiamond,
                _ => VfxParticleTextureKind.SoftDustPuff
            };
            Texture2D texture = VfxParticleTextureFactory.Create(textureKind);
            texture.name = $"{textureKind} ({effectId}) Runtime Particle Texture";
            texture.hideFlags = HideFlags.DontSave;
            Material material = VfxParticleMaterialFactory.CreateTransparent(
                $"{effectId} Runtime Particle Material",
                texture,
                additive: effectId != VfxEffectId.KnifeHitSparks);
            ownedParticleResources.Add(texture);
            if (material != null)
            {
                renderer.sharedMaterial = material;
                ownedParticleResources.Add(material);
            }
            else
            {
                Debug.LogError($"No supported shader is available for the runtime {effectId} particle fallback.");
            }
        }

    }
}
