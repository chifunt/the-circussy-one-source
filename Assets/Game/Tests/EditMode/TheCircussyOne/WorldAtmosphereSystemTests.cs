using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using TheCircussyOne.Config;
using TheCircussyOne.Runtime;
using TheCircussyOne.Visuals;

public sealed class WorldAtmosphereSystemTests
{
    private VfxVisualConfig config;
    private PlayerView player;
    private Transform root;

    [SetUp]
    public void SetUp()
    {
        TheCircussyOneTestObjects.DestroyAll();
        config = TheCircussyOneTestObjects.CreateVfxVisualConfig();
        player = TheCircussyOneTestObjects.CreatePlayer(TheCircussyOneTestObjects.CreateConfig());
        root = TheCircussyOneTestObjects.CreateRoot("VFX").transform;
    }

    [TearDown]
    public void TearDown()
    {
        TheCircussyOneTestObjects.Destroy(config);
        TheCircussyOneTestObjects.DestroyAll();
    }

    [Test]
    public void AmbientDustDefaultsClampInvalidRanges()
    {
        config.worldAmbientDustEmissionRate = -1f;
        config.worldAmbientDustMaxParticles = 0;
        config.worldAmbientDustFollowRadius = 0f;
        config.worldAmbientDustHeightRange = new Vector2(4f, 2f);
        config.worldAmbientDustSizeRange = new Vector2(-1f, -2f);
        config.worldAmbientDustLifetimeRange = new Vector2(0f, 0f);
        config.worldAmbientDustDriftSpeed = -1f;
        config.worldAmbientDustNoiseStrength = -1f;
        config.worldFloorHaze.emissionRate = -1f;
        config.worldFloorHaze.maxParticles = 0;
        config.worldFloorHaze.followRadius = 0f;
        config.worldFloorHaze.heightRange = new Vector2(4f, 1f);
        config.worldFloorHaze.sizeRange = new Vector2(0f, 0f);
        config.worldFloorHaze.lifetimeRange = new Vector2(0f, 0f);
        config.worldFloorHaze.driftSpeed = -1f;
        config.worldFloorHaze.noiseStrength = -1f;
        config.worldGodRayDust = null;

        bool changed = config.EnsureWorkflowDefaults();

        Assert.That(changed, Is.True);
        Assert.That(config.worldAmbientDustEmissionRate, Is.EqualTo(16f));
        Assert.That(config.worldAmbientDustMaxParticles, Is.EqualTo(220));
        Assert.That(config.worldAmbientDustFollowRadius, Is.EqualTo(34f));
        Assert.That(config.worldAmbientDustHeightRange, Is.EqualTo(new Vector2(0.5f, 8f)));
        Assert.That(config.worldAmbientDustSizeRange, Is.EqualTo(new Vector2(0.025f, 0.09f)));
        Assert.That(config.worldAmbientDustLifetimeRange, Is.EqualTo(new Vector2(7f, 13f)));
        Assert.That(config.worldAmbientDustDriftSpeed, Is.EqualTo(0.08f));
        Assert.That(config.worldAmbientDustNoiseStrength, Is.EqualTo(0.12f));
        Assert.That(config.worldFloorHaze.emissionRate, Is.EqualTo(8f));
        Assert.That(config.worldFloorHaze.maxParticles, Is.EqualTo(140));
        Assert.That(config.worldFloorHaze.followRadius, Is.EqualTo(42f));
        Assert.That(config.worldFloorHaze.heightRange, Is.EqualTo(new Vector2(0.05f, 1.25f)));
        Assert.That(config.worldFloorHaze.sizeRange, Is.EqualTo(new Vector2(0.85f, 2.4f)));
        Assert.That(config.worldFloorHaze.lifetimeRange, Is.EqualTo(new Vector2(6f, 12f)));
        Assert.That(config.worldFloorHaze.driftSpeed, Is.EqualTo(0.16f));
        Assert.That(config.worldFloorHaze.noiseStrength, Is.EqualTo(0.16f));
        Assert.That(config.worldGodRayDust, Is.Not.Null);
        Assert.That(config.worldGodRayDust.emissionRate, Is.EqualTo(10f));
    }

    [Test]
    public void AmbientDustViewConfiguresWorldSpaceBoxWithoutGameplayPhysics()
    {
        WorldAmbientDustView prefab = TheCircussyOneTestObjects.CreateWorldAmbientDustPrefab(config);
        ParticleSystem particles = prefab.DustParticles;

        Assert.That(particles.main.simulationSpace, Is.EqualTo(ParticleSystemSimulationSpace.World));
        Assert.That(particles.shape.shapeType, Is.EqualTo(ParticleSystemShapeType.Box));
        Assert.That(particles.GetComponentsInChildren<Collider>(includeInactive: true), Is.Empty);
        Assert.That(particles.GetComponentsInChildren<Rigidbody>(includeInactive: true), Is.Empty);
    }

    [Test]
    public void AmbientDustViewUsesSoftTransparentParticleMaterial()
    {
        WorldAmbientDustView prefab = TheCircussyOneTestObjects.CreateWorldAmbientDustPrefab(config);
        ParticleSystem particles = prefab.DustParticles;
        Material material = particles.GetComponent<ParticleSystemRenderer>().sharedMaterial;

        Assert.That(material, Is.Not.Null);
        Assert.That(VfxParticleMaterialFactory.IsUsable(material), Is.True);
        Assert.That(material.renderQueue, Is.EqualTo((int)RenderQueue.Transparent));
        Assert.That(TextureFor(material), Is.Not.Null);
        if (material.HasProperty("_ZWrite"))
        {
            Assert.That(material.GetFloat("_ZWrite"), Is.Zero);
        }
    }

    [Test]
    public void SystemCreatesAtmosphereViewsAndFollowsPlayer()
    {
        config.worldAmbientDustFollowOffset = new Vector3(1f, 2f, 3f);
        config.worldFloorHaze.followOffset = new Vector3(0f, 0.25f, 0f);
        config.worldGodRayDust.followOffset = new Vector3(-1f, 3f, 2f);
        WorldAmbientDustView prefab = TheCircussyOneTestObjects.CreateWorldAmbientDustPrefab(config);
        var system = new WorldAtmosphereSystem(config, new WorldAtmosphereFactory(prefab, root), player);
        player.transform.position = new Vector3(4f, 0f, 6f);

        system.Start();
        system.Tick();

        Assert.That(system.ActiveViewCount, Is.EqualTo(3));
        Assert.That(system.ActiveDust.transform.parent, Is.EqualTo(root));
        Assert.That(system.ActiveFloorHaze.transform.parent, Is.EqualTo(root));
        Assert.That(system.ActiveGodRayDust.transform.parent, Is.EqualTo(root));
        Assert.That(system.ActiveDust.LayerId, Is.EqualTo(WorldAtmosphereLayerId.AmbientDust));
        Assert.That(system.ActiveFloorHaze.LayerId, Is.EqualTo(WorldAtmosphereLayerId.FloorHaze));
        Assert.That(system.ActiveGodRayDust.LayerId, Is.EqualTo(WorldAtmosphereLayerId.GodRayDust));
        Assert.That(system.ActiveDust.transform.position, Is.EqualTo(new Vector3(5f, 2f, 9f)));
        Assert.That(system.ActiveFloorHaze.transform.position, Is.EqualTo(new Vector3(4f, 0.25f, 6f)));
        Assert.That(system.ActiveGodRayDust.transform.position, Is.EqualTo(new Vector3(3f, 3f, 8f)));

        player.transform.position = new Vector3(-2f, 0f, 8f);
        system.Tick();

        Assert.That(system.ActiveViewCount, Is.EqualTo(3));
        Assert.That(system.ActiveDust.transform.position, Is.EqualTo(new Vector3(-1f, 2f, 11f)));
        Assert.That(system.ActiveFloorHaze.transform.position, Is.EqualTo(new Vector3(-2f, 0.25f, 8f)));
        Assert.That(system.ActiveGodRayDust.transform.position, Is.EqualTo(new Vector3(-3f, 3f, 10f)));

        system.Dispose();
    }

    [Test]
    public void SystemDoesNotCreateAtmosphereWhenAllLayersDisabled()
    {
        config.worldAmbientDustEnabled = false;
        config.worldFloorHaze.enabled = false;
        config.worldGodRayDust.enabled = false;
        WorldAmbientDustView prefab = TheCircussyOneTestObjects.CreateWorldAmbientDustPrefab(config);
        var system = new WorldAtmosphereSystem(config, new WorldAtmosphereFactory(prefab, root), player);

        system.Start();

        Assert.That(system.ActiveViewCount, Is.Zero);

        system.Dispose();
    }

    [Test]
    public void PauseSystemPausesAndResumesAmbientDustParticles()
    {
        WorldAmbientDustView prefab = TheCircussyOneTestObjects.CreateWorldAmbientDustPrefab(config);
        var atmosphere = new WorldAtmosphereSystem(config, new WorldAtmosphereFactory(prefab, root), player);
        atmosphere.Start();
        ParticleSystem dust = atmosphere.ActiveDust.DustParticles;
        ParticleSystem floorHaze = atmosphere.ActiveFloorHaze.DustParticles;
        ParticleSystem godRayDust = atmosphere.ActiveGodRayDust.DustParticles;
        var pauseState = new RunPauseState();
        var pauseSystem = new WorldAnimationPauseSystem(pauseState, player, new ActorRegistry(), worldAtmosphereSystem: atmosphere);

        pauseSystem.Start();
        pauseState.Pause("test");

        Assert.That(dust.isPaused, Is.True);
        Assert.That(floorHaze.isPaused, Is.True);
        Assert.That(godRayDust.isPaused, Is.True);

        pauseState.Resume("test");

        Assert.That(dust.isPaused, Is.False);
        Assert.That(dust.isPlaying, Is.True);
        Assert.That(floorHaze.isPaused, Is.False);
        Assert.That(floorHaze.isPlaying, Is.True);
        Assert.That(godRayDust.isPaused, Is.False);
        Assert.That(godRayDust.isPlaying, Is.True);

        pauseSystem.Dispose();
        atmosphere.Dispose();
    }

    private static Texture TextureFor(Material material)
    {
        if (material == null)
        {
            return null;
        }

        if (material.HasProperty("_BaseMap"))
        {
            return material.GetTexture("_BaseMap");
        }

        return material.HasProperty("_MainTex") ? material.GetTexture("_MainTex") : null;
    }
}
