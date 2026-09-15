using UnityEngine;
using UnityEngine.Rendering;
using TheCircussyOne.Config;
using TheCircussyOne.Runtime;

namespace TheCircussyOne.Visuals
{
    public sealed class WorldAmbientDustView : MonoBehaviour
    {
        [SerializeField] private ParticleSystem dustParticles;
        [SerializeField] private WorldAtmosphereLayerId layerId = WorldAtmosphereLayerId.AmbientDust;
        private bool configured;
        private Material runtimeParticleMaterial;
        private static Texture2D softParticleTexture;

        public ParticleSystem DustParticles => ResolveParticles();
        public bool IsActive => gameObject.activeSelf;
        public WorldAtmosphereLayerId LayerId => layerId;

        public void ConfigureDefaults(VfxVisualConfig config, Material material)
        {
            ConfigureDefaults(config, WorldAtmosphereLayerId.AmbientDust, material);
        }

        public void ConfigureDefaults(
            VfxVisualConfig config,
            WorldAtmosphereLayerId layer = WorldAtmosphereLayerId.AmbientDust,
            Material material = null)
        {
            layerId = layer;
            ParticleSystem particles = ResolveParticles();
            particles.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ApplyParticleSettings(config, layerId, material);
            particles.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);
            configured = true;
        }

        public void Show(
            VfxVisualConfig config,
            Vector3 worldPosition,
            WorldAtmosphereLayerId layer = WorldAtmosphereLayerId.AmbientDust)
        {
            if (!IsLayerEnabled(config, layer))
            {
                Deactivate();
                return;
            }

            gameObject.SetActive(true);
            ParticleLayerSettings settings = ResolveLayerSettings(config, layer);
            transform.position = worldPosition + settings.FollowOffset;
            ParticleSystem particles = ResolveParticles();
            if (!configured || layerId != layer)
            {
                layerId = layer;
                particles.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ApplyParticleSettings(config, layerId, null);
                configured = true;
            }

            if (!particles.isPlaying && !particles.isPaused)
            {
                particles.Play(withChildren: true);
            }
        }

        public void Deactivate()
        {
            if (dustParticles != null)
            {
                dustParticles.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            gameObject.SetActive(false);
        }

        public static bool IsLayerEnabled(VfxVisualConfig config, WorldAtmosphereLayerId layer)
        {
            if (config == null || !config.enabled)
            {
                return false;
            }

            return ResolveLayerSettings(config, layer).Enabled;
        }

        private ParticleSystem ResolveParticles()
        {
            if (dustParticles != null)
            {
                return dustParticles;
            }

            dustParticles = GetComponentInChildren<ParticleSystem>(includeInactive: true);
            if (dustParticles == null)
            {
                dustParticles = gameObject.AddComponent<ParticleSystem>();
            }

            return dustParticles;
        }

        private void ApplyParticleSettings(
            VfxVisualConfig config,
            WorldAtmosphereLayerId layer,
            Material material)
        {
            ParticleSystem particles = ResolveParticles();
            if (config == null)
            {
                return;
            }

            ParticleLayerSettings settings = ResolveLayerSettings(config, layer);
            Vector2 lifetime = SafeRange(settings.LifetimeRange, settings.FallbackLifetimeRange, 0.02f, 0.01f);
            Vector2 size = SafeRange(settings.SizeRange, settings.FallbackSizeRange, 0.001f, 0.001f);
            Vector2 height = SafeRange(settings.HeightRange, settings.FallbackHeightRange, 0f, 0.1f);
            float radius = Mathf.Max(1f, settings.FollowRadius);
            float driftSpeed = Mathf.Max(0f, settings.DriftSpeed);
            float emissionRate = Mathf.Max(0f, settings.EmissionRate);

            var main = particles.main;
            main.loop = true;
            main.duration = Mathf.Max(1f, lifetime.y);
            main.startLifetime = new ParticleSystem.MinMaxCurve(lifetime.x, lifetime.y);
            main.startSize = new ParticleSystem.MinMaxCurve(size.x, size.y);
            main.startSpeed = 0f;
            main.startColor = settings.Color;
            main.gravityModifier = 0f;
            main.maxParticles = Mathf.Max(1, settings.MaxParticles);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            main.playOnAwake = false;

            var emission = particles.emission;
            emission.enabled = config.enabled && settings.Enabled && emissionRate > 0f;
            emission.rateOverTime = emissionRate;
            emission.SetBursts(System.Array.Empty<ParticleSystem.Burst>());

            var shape = particles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(radius * 2f, Mathf.Max(0.1f, height.y - height.x), radius * 2f);
            shape.position = new Vector3(0f, (height.x + height.y) * 0.5f, 0f);

            var velocity = particles.velocityOverLifetime;
            velocity.enabled = driftSpeed > 0f;
            velocity.space = ParticleSystemSimulationSpace.World;
            velocity.x = new ParticleSystem.MinMaxCurve(-driftSpeed, driftSpeed);
            velocity.y = new ParticleSystem.MinMaxCurve(-driftSpeed * 0.18f, driftSpeed * 0.28f);
            velocity.z = new ParticleSystem.MinMaxCurve(-driftSpeed, driftSpeed);

            var noise = particles.noise;
            noise.enabled = settings.NoiseStrength > 0f;
            noise.strength = new ParticleSystem.MinMaxCurve(Mathf.Max(0f, settings.NoiseStrength));
            noise.frequency = 0.16f;

            var colorOverLifetime = particles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(1f, 0.18f),
                    new GradientAlphaKey(0.82f, 0.72f),
                    new GradientAlphaKey(0f, 1f)
                });
            colorOverLifetime.color = gradient;

            var renderer = particles.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingOrder = settings.SortingOrder;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            if (material != null)
            {
                ReleaseRuntimeParticleMaterial();
                ConfigureParticleMaterial(material);
                renderer.sharedMaterial = material;
            }
            else
            {
                renderer.sharedMaterial = ResolveRuntimeParticleMaterial();
            }
        }

        private void OnDestroy()
        {
            ReleaseRuntimeParticleMaterial();
        }

        private static Vector2 SafeRange(Vector2 value, Vector2 fallback, float minimum, float minimumSpan)
        {
            return value.x >= minimum && value.y >= value.x + minimumSpan ? value : fallback;
        }

        private static ParticleLayerSettings ResolveLayerSettings(VfxVisualConfig config, WorldAtmosphereLayerId layer)
        {
            return layer switch
            {
                WorldAtmosphereLayerId.FloorHaze => ParticleLayerSettings.From(config.worldFloorHaze, ParticleLayerSettings.FloorHazeFallback),
                WorldAtmosphereLayerId.GodRayDust => ParticleLayerSettings.From(config.worldGodRayDust, ParticleLayerSettings.GodRayDustFallback),
                _ => ParticleLayerSettings.AmbientDust(config)
            };
        }

        private Material ResolveRuntimeParticleMaterial()
        {
            if (VfxParticleMaterialFactory.IsUsable(runtimeParticleMaterial))
            {
                return runtimeParticleMaterial;
            }

            ReleaseRuntimeParticleMaterial();
            runtimeParticleMaterial = VfxParticleMaterialFactory.CreateTransparent(
                "World Ambient Dust Runtime Material",
                GetOrCreateSoftParticleTexture());
            if (runtimeParticleMaterial == null)
            {
                Debug.LogError("No supported shader is available for the runtime world-atmosphere particle fallback.", this);
            }

            return runtimeParticleMaterial;
        }

        private void ReleaseRuntimeParticleMaterial()
        {
            RuntimeObjectFactory.Release(runtimeParticleMaterial);
            runtimeParticleMaterial = null;
        }

        private static void ConfigureParticleMaterial(Material material)
        {
            if (material == null)
            {
                return;
            }

            material.renderQueue = (int)RenderQueue.Transparent;
            SetFloatIfPresent(material, "_Surface", 1f);
            SetFloatIfPresent(material, "_Blend", 0f);
            SetFloatIfPresent(material, "_SrcBlend", (float)BlendMode.SrcAlpha);
            SetFloatIfPresent(material, "_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            SetFloatIfPresent(material, "_ZWrite", 0f);
            SetFloatIfPresent(material, "_Cull", (float)CullMode.Off);
            SetColorIfPresent(material, "_BaseColor", Color.white);
            material.SetOverrideTag("RenderType", "Transparent");
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.DisableKeyword("_ALPHATEST_ON");

            if (TextureFor(material) == null)
            {
                Texture2D texture = GetOrCreateSoftParticleTexture();
                if (material.HasProperty("_BaseMap"))
                {
                    material.SetTexture("_BaseMap", texture);
                }

                if (material.HasProperty("_MainTex"))
                {
                    material.SetTexture("_MainTex", texture);
                }
            }
        }

        private static Texture TextureFor(Material material)
        {
            if (material == null)
            {
                return null;
            }

            if (material.HasProperty("_BaseMap"))
            {
                Texture texture = material.GetTexture("_BaseMap");
                if (texture != null)
                {
                    return texture;
                }
            }

            return material.HasProperty("_MainTex") ? material.GetTexture("_MainTex") : null;
        }

        private static void SetFloatIfPresent(Material material, string propertyName, float value)
        {
            if (material != null && material.HasProperty(propertyName))
            {
                material.SetFloat(propertyName, value);
            }
        }

        private static void SetColorIfPresent(Material material, string propertyName, Color value)
        {
            if (material != null && material.HasProperty(propertyName))
            {
                material.SetColor(propertyName, value);
            }
        }

        private static Texture2D GetOrCreateSoftParticleTexture()
        {
            if (softParticleTexture != null)
            {
                return softParticleTexture;
            }

            softParticleTexture = VfxParticleTextureFactory.Create(VfxParticleTextureKind.SoftDustPuff);
            softParticleTexture.name = "World Atmosphere Soft Particle Texture";
            softParticleTexture.hideFlags = HideFlags.DontSave;
            return softParticleTexture;
        }

        private readonly struct ParticleLayerSettings
        {
            public readonly bool Enabled;
            public readonly float EmissionRate;
            public readonly int MaxParticles;
            public readonly float FollowRadius;
            public readonly Vector2 HeightRange;
            public readonly Vector3 FollowOffset;
            public readonly Color Color;
            public readonly Vector2 SizeRange;
            public readonly Vector2 LifetimeRange;
            public readonly float DriftSpeed;
            public readonly float NoiseStrength;
            public readonly int SortingOrder;
            public readonly Vector2 FallbackHeightRange;
            public readonly Vector2 FallbackSizeRange;
            public readonly Vector2 FallbackLifetimeRange;

            private ParticleLayerSettings(
                bool enabled,
                float emissionRate,
                int maxParticles,
                float followRadius,
                Vector2 heightRange,
                Vector3 followOffset,
                Color color,
                Vector2 sizeRange,
                Vector2 lifetimeRange,
                float driftSpeed,
                float noiseStrength,
                int sortingOrder,
                Vector2 fallbackHeightRange,
                Vector2 fallbackSizeRange,
                Vector2 fallbackLifetimeRange)
            {
                Enabled = enabled;
                EmissionRate = emissionRate;
                MaxParticles = maxParticles;
                FollowRadius = followRadius;
                HeightRange = heightRange;
                FollowOffset = followOffset;
                Color = color;
                SizeRange = sizeRange;
                LifetimeRange = lifetimeRange;
                DriftSpeed = driftSpeed;
                NoiseStrength = noiseStrength;
                SortingOrder = sortingOrder;
                FallbackHeightRange = fallbackHeightRange;
                FallbackSizeRange = fallbackSizeRange;
                FallbackLifetimeRange = fallbackLifetimeRange;
            }

            public static ParticleLayerSettings AmbientDust(VfxVisualConfig config)
            {
                return new ParticleLayerSettings(
                    config.worldAmbientDustEnabled,
                    config.worldAmbientDustEmissionRate,
                    config.worldAmbientDustMaxParticles,
                    config.worldAmbientDustFollowRadius,
                    config.worldAmbientDustHeightRange,
                    config.worldAmbientDustFollowOffset,
                    config.worldAmbientDustColor,
                    config.worldAmbientDustSizeRange,
                    config.worldAmbientDustLifetimeRange,
                    config.worldAmbientDustDriftSpeed,
                    config.worldAmbientDustNoiseStrength,
                    2,
                    new Vector2(0.5f, 8f),
                    new Vector2(0.025f, 0.09f),
                    new Vector2(7f, 13f));
            }

            public static ParticleLayerSettings FloorHazeFallback => new(
                enabled: true,
                emissionRate: 8f,
                maxParticles: 140,
                followRadius: 42f,
                heightRange: new Vector2(0.05f, 1.25f),
                followOffset: Vector3.zero,
                color: new Color(0.48f, 0.43f, 0.34f, 0.09f),
                sizeRange: new Vector2(0.85f, 2.4f),
                lifetimeRange: new Vector2(6f, 12f),
                driftSpeed: 0.16f,
                noiseStrength: 0.16f,
                sortingOrder: 1,
                fallbackHeightRange: new Vector2(0.05f, 1.25f),
                fallbackSizeRange: new Vector2(0.85f, 2.4f),
                fallbackLifetimeRange: new Vector2(6f, 12f));

            public static ParticleLayerSettings GodRayDustFallback => new(
                enabled: true,
                emissionRate: 10f,
                maxParticles: 160,
                followRadius: 38f,
                heightRange: new Vector2(2.4f, 12f),
                followOffset: Vector3.zero,
                color: new Color(1f, 0.74f, 0.42f, 0.1f),
                sizeRange: new Vector2(0.025f, 0.1f),
                lifetimeRange: new Vector2(7f, 14f),
                driftSpeed: 0.06f,
                noiseStrength: 0.14f,
                sortingOrder: 3,
                fallbackHeightRange: new Vector2(2.4f, 12f),
                fallbackSizeRange: new Vector2(0.025f, 0.1f),
                fallbackLifetimeRange: new Vector2(7f, 14f));

            public static ParticleLayerSettings From(
                WorldAtmosphereLayerSettings settings,
                ParticleLayerSettings fallback)
            {
                if (settings == null)
                {
                    return fallback;
                }

                return new ParticleLayerSettings(
                    settings.enabled,
                    settings.emissionRate,
                    settings.maxParticles,
                    settings.followRadius,
                    settings.heightRange,
                    settings.followOffset,
                    settings.color,
                    settings.sizeRange,
                    settings.lifetimeRange,
                    settings.driftSpeed,
                    settings.noiseStrength,
                    settings.sortingOrder,
                    fallback.FallbackHeightRange,
                    fallback.FallbackSizeRange,
                    fallback.FallbackLifetimeRange);
            }
        }
    }
}
