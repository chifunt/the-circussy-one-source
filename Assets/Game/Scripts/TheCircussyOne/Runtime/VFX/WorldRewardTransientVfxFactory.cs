using System.Collections.Generic;
using TheCircussyOne.Content;
using TheCircussyOne.Visuals;
using UnityEngine;
using UnityEngine.Rendering;

namespace TheCircussyOne.Runtime
{
    public sealed class WorldRewardTransientVfxFactory
    {
        private readonly Transform root;
        private readonly Stack<ParticleSystem> ticketBurstPool = new();
        private readonly Stack<ParticleSystem> snackBurstPool = new();

        private static readonly Dictionary<Texture, Material> RuntimeTicketBurstMaterials = new();
        private static Material runtimeSnackBurstMaterial;
        private static Texture2D runtimeTicketBurstTexture;
        private static Texture2D runtimeSnackBurstTexture;

        public WorldRewardTransientVfxFactory(Transform root = null)
        {
            this.root = root;
        }

        public int PooledTicketBurstCount => ticketBurstPool.Count;
        public int PooledSnackBurstCount => snackBurstPool.Count;

        public int PrewarmTicketBursts(int targetPoolCount, int maxCreate = int.MaxValue)
        {
            GetRuntimeTicketBurstMaterial(null);
            return Prewarm(ticketBurstPool, "Ticket Paper Burst", targetPoolCount, maxCreate);
        }

        public int PrewarmSnackBursts(int targetPoolCount, int maxCreate = int.MaxValue)
        {
            GetRuntimeSnackBurstMaterial();
            return Prewarm(snackBurstPool, "Snack Burst", targetPoolCount, maxCreate);
        }

        public ParticleSystem SpawnTicketPaperBurst(TicketDepositDefinition definition, Vector3 origin)
        {
            if (definition == null || !definition.ticketBurstEnabled || definition.ticketBurstCount <= 0)
            {
                return null;
            }

            ParticleSystem burst = GetBurst(ticketBurstPool, "Ticket Paper Burst", origin);
            ConfigureTicketBurst(burst, definition);
            EmitTicketBurst(burst, definition);
            return burst;
        }

        public ParticleSystem SpawnSnackBurst(HealingPropDefinition definition, Vector3 origin)
        {
            if (definition == null || !definition.snackBurstEnabled || definition.snackBurstCount <= 0)
            {
                return null;
            }

            ParticleSystem burst = GetBurst(snackBurstPool, "Snack Burst", origin);
            ConfigureSnackBurst(burst, definition);
            EmitSnackBurst(burst, definition);
            return burst;
        }

        public void ReleaseTicketPaperBurst(ParticleSystem burst)
        {
            ReleaseToPool(burst, ticketBurstPool);
        }

        public void ReleaseSnackBurst(ParticleSystem burst)
        {
            ReleaseToPool(burst, snackBurstPool);
        }

        public void Release(GameObject target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Object.Destroy(target);
                return;
            }

            Object.DestroyImmediate(target);
        }

        private int Prewarm(Stack<ParticleSystem> pool, string name, int targetPoolCount, int maxCreate)
        {
            int target = Mathf.Max(0, targetPoolCount);
            int limit = Mathf.Max(0, maxCreate);
            int created = 0;
            while (pool.Count < target && created < limit)
            {
                GameObject burstObject = CreateBurstObject(name, Vector3.zero);
                ParticleSystem burst = burstObject.AddComponent<ParticleSystem>();
                burstObject.SetActive(false);
                pool.Push(burst);
                created++;
            }

            return created;
        }

        private ParticleSystem GetBurst(Stack<ParticleSystem> pool, string name, Vector3 origin)
        {
            ParticleSystem burst = null;
            while (pool.Count > 0 && burst == null)
            {
                burst = pool.Pop();
            }

            if (burst == null)
            {
                GameObject createdBurstObject = CreateBurstObject(name, origin);
                burst = createdBurstObject.AddComponent<ParticleSystem>();
            }

            GameObject burstObject = burst.gameObject;
            burstObject.name = name;
            if (root != null)
            {
                burstObject.transform.SetParent(root, worldPositionStays: false);
            }

            burstObject.transform.position = origin;
            burstObject.SetActive(true);
            return burst;
        }

        private void ReleaseToPool(ParticleSystem burst, Stack<ParticleSystem> pool)
        {
            if (burst == null)
            {
                return;
            }

            burst.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);
            GameObject burstObject = burst.gameObject;
            if (root != null)
            {
                burstObject.transform.SetParent(root, worldPositionStays: false);
            }

            burstObject.SetActive(false);
            pool.Push(burst);
        }

        private GameObject CreateBurstObject(string name, Vector3 origin)
        {
            GameObject burstObject = new(name);
            if (root != null)
            {
                burstObject.transform.SetParent(root, worldPositionStays: false);
            }

            burstObject.transform.position = origin;
            return burstObject;
        }

        private static void EmitTicketBurst(ParticleSystem burst, TicketDepositDefinition definition)
        {
            burst.Play(false);
            float lifetime = Mathf.Max(0.01f, definition.ticketBurstLifetimeSeconds);
            for (int i = 0; i < definition.ticketBurstCount; i++)
            {
                float angle = Random.value * Mathf.PI * 2f;
                float radius = Random.Range(0.02f, 0.16f) * Mathf.Max(0.75f, definition.visualScale);
                Vector3 radial = new(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                float speed = Mathf.Max(0f, definition.ticketBurstSpeed) * Random.Range(0.65f, 1.25f);
                float upward = speed * Random.Range(0.42f, 0.82f);
                var emit = new ParticleSystem.EmitParams
                {
                    position = radial * radius + Vector3.up * Random.Range(0f, 0.08f),
                    velocity = radial * speed + Vector3.up * upward,
                    startLifetime = lifetime * Random.Range(0.82f, 1.18f),
                    startColor = definition.ticketBurstColor,
                    randomSeed = (uint)Random.Range(1, int.MaxValue)
                };
                burst.Emit(emit, 1);
            }
        }

        private static void EmitSnackBurst(ParticleSystem burst, HealingPropDefinition definition)
        {
            burst.Play(false);
            float lifetime = Mathf.Max(0.01f, definition.snackBurstLifetimeSeconds);
            for (int i = 0; i < definition.snackBurstCount; i++)
            {
                float angle = Random.value * Mathf.PI * 2f;
                float radius = Random.Range(0.01f, 0.08f) * Mathf.Max(0.75f, definition.visualScale);
                Vector3 radial = new(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                float speed = Mathf.Max(0f, definition.snackBurstSpeed) * Random.Range(0.55f, 1.2f);
                float upward = speed * Random.Range(0.35f, 0.75f);
                var emit = new ParticleSystem.EmitParams
                {
                    position = radial * radius + Vector3.up * Random.Range(0f, 0.06f),
                    velocity = radial * speed + Vector3.up * upward,
                    startLifetime = lifetime * Random.Range(0.82f, 1.15f),
                    startColor = definition.snackBurstColor,
                    startSize = Mathf.Max(0.01f, definition.snackBurstStartSize) * Random.Range(0.72f, 1.18f),
                    randomSeed = (uint)Random.Range(1, int.MaxValue)
                };
                burst.Emit(emit, 1);
            }
        }

        private static void ConfigureTicketBurst(ParticleSystem burst, TicketDepositDefinition definition)
        {
            burst.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ParticleSystem.MainModule main = burst.main;
            main.loop = false;
            main.playOnAwake = false;
            main.duration = 0.08f;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.maxParticles = Mathf.Max(1, definition.ticketBurstCount);
            main.startLifetime = Mathf.Max(0.01f, definition.ticketBurstLifetimeSeconds);
            main.startSpeed = 0f;
            main.startSize3D = true;
            Vector2 size = definition.ticketBurstRectangleSize;
            main.startSizeX = Mathf.Max(0.01f, size.x);
            main.startSizeY = Mathf.Max(0.01f, size.y);
            main.startSizeZ = 1f;
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startColor = definition.ticketBurstColor;
            main.gravityModifier = Mathf.Max(0f, definition.ticketBurstGravity);

            ParticleSystem.EmissionModule emission = burst.emission;
            emission.enabled = false;

            ParticleSystem.NoiseModule noise = burst.noise;
            noise.enabled = definition.ticketBurstSwayStrength > 0f;
            noise.strength = Mathf.Max(0f, definition.ticketBurstSwayStrength);
            noise.frequency = 0.85f;
            noise.scrollSpeed = 0.25f;

            ParticleSystem.ColorOverLifetimeModule colorOverLifetime = burst.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new();
            Color start = definition.ticketBurstColor;
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(start, 0f),
                    new GradientColorKey(Color.Lerp(start, Color.white, 0.12f), 0.25f),
                    new GradientColorKey(start, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(start.a, 0f),
                    new GradientAlphaKey(start.a * 0.82f, 0.42f),
                    new GradientAlphaKey(start.a * 0.28f, 0.78f),
                    new GradientAlphaKey(0f, 1f)
                });
            colorOverLifetime.color = gradient;

            ParticleSystem.RotationOverLifetimeModule rotationOverLifetime = burst.rotationOverLifetime;
            rotationOverLifetime.enabled = true;
            rotationOverLifetime.z = new ParticleSystem.MinMaxCurve(-Mathf.PI * 1.25f, Mathf.PI * 1.25f);

            ParticleSystemRenderer renderer = burst.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sharedMaterial = GetRuntimeTicketBurstMaterial(definition.ticketBurstTexture);
            renderer.sortingOrder = 320;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        }

        private static void ConfigureSnackBurst(ParticleSystem burst, HealingPropDefinition definition)
        {
            burst.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ParticleSystem.MainModule main = burst.main;
            main.loop = false;
            main.playOnAwake = false;
            main.duration = 0.08f;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.maxParticles = Mathf.Max(1, definition.snackBurstCount);
            main.startLifetime = Mathf.Max(0.01f, definition.snackBurstLifetimeSeconds);
            main.startSpeed = 0f;
            main.startSize = Mathf.Max(0.01f, definition.snackBurstStartSize);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startColor = definition.snackBurstColor;
            main.gravityModifier = Mathf.Max(0f, definition.snackBurstGravity);

            ParticleSystem.EmissionModule emission = burst.emission;
            emission.enabled = false;

            ParticleSystem.NoiseModule noise = burst.noise;
            noise.enabled = definition.snackBurstSwayStrength > 0f;
            noise.strength = Mathf.Max(0f, definition.snackBurstSwayStrength);
            noise.frequency = 0.9f;
            noise.scrollSpeed = 0.2f;

            ParticleSystem.ColorOverLifetimeModule colorOverLifetime = burst.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new();
            Color start = definition.snackBurstColor;
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(start, 0f),
                    new GradientColorKey(Color.Lerp(start, Color.white, 0.18f), 0.22f),
                    new GradientColorKey(start, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(start.a, 0f),
                    new GradientAlphaKey(start.a * 0.7f, 0.38f),
                    new GradientAlphaKey(start.a * 0.18f, 0.78f),
                    new GradientAlphaKey(0f, 1f)
                });
            colorOverLifetime.color = gradient;

            ParticleSystem.RotationOverLifetimeModule rotationOverLifetime = burst.rotationOverLifetime;
            rotationOverLifetime.enabled = true;
            rotationOverLifetime.z = new ParticleSystem.MinMaxCurve(-Mathf.PI, Mathf.PI);

            ParticleSystemRenderer renderer = burst.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sharedMaterial = GetRuntimeSnackBurstMaterial();
            renderer.sortingOrder = 318;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        }

        private static Material GetRuntimeTicketBurstMaterial(Texture ticketBurstTexture)
        {
            Texture texture = ticketBurstTexture != null ? ticketBurstTexture : GetRuntimeTicketBurstTexture();
            if (RuntimeTicketBurstMaterials.TryGetValue(texture, out Material material) && material != null)
            {
                return material;
            }

            material = CreateTransparentParticleMaterial(
                "Runtime Ticket Paper Burst",
                texture);
            RuntimeTicketBurstMaterials[texture] = material;
            return material;
        }

        private static Material GetRuntimeSnackBurstMaterial()
        {
            if (runtimeSnackBurstMaterial != null)
            {
                return runtimeSnackBurstMaterial;
            }

            runtimeSnackBurstMaterial = CreateTransparentParticleMaterial(
                "Runtime Snack Burst",
                GetRuntimeSnackBurstTexture());
            return runtimeSnackBurstMaterial;
        }

        private static Texture2D GetRuntimeTicketBurstTexture()
        {
            if (runtimeTicketBurstTexture != null)
            {
                return runtimeTicketBurstTexture;
            }

            runtimeTicketBurstTexture = VfxParticleTextureFactory.Create(VfxParticleTextureKind.TicketPaper);
            runtimeTicketBurstTexture.name = "Runtime Ticket Paper Texture";
            runtimeTicketBurstTexture.hideFlags = HideFlags.DontSave;
            return runtimeTicketBurstTexture;
        }

        private static Texture2D GetRuntimeSnackBurstTexture()
        {
            if (runtimeSnackBurstTexture != null)
            {
                return runtimeSnackBurstTexture;
            }

            runtimeSnackBurstTexture = VfxParticleTextureFactory.Create(VfxParticleTextureKind.SnackHeart);
            runtimeSnackBurstTexture.name = "Runtime Snack Heart Texture";
            runtimeSnackBurstTexture.hideFlags = HideFlags.DontSave;
            return runtimeSnackBurstTexture;
        }

        private static Material CreateTransparentParticleMaterial(string materialName, Texture texture)
        {
            return VfxParticleMaterialFactory.CreateTransparent(materialName, texture);
        }
    }
}
