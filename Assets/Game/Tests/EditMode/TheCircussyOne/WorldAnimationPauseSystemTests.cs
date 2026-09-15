using NUnit.Framework;
using UnityEngine;
using TheCircussyOne.Content;
using TheCircussyOne.Runtime;
using TheCircussyOne.Visuals;

public sealed class WorldAnimationPauseSystemTests
{
    [TearDown]
    public void TearDown()
    {
        TheCircussyOneTestObjects.DestroyAll();
    }

    [Test]
    public void PauseStatePausesAndResumesPlayerParticleSystems()
    {
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(TheCircussyOneTestObjects.CreateConfig());
        ParticleSystem trail = AddPlayingParticleSystem(player.transform, "Player Jump Trail");
        var pauseState = new RunPauseState();
        var system = new WorldAnimationPauseSystem(pauseState, player, new ActorRegistry());

        system.Start();
        pauseState.Pause("test");

        Assert.That(trail.isPaused, Is.True);

        pauseState.Resume("test");

        Assert.That(trail.isPaused, Is.False);
        Assert.That(trail.isPlaying, Is.True);

        system.Dispose();
    }

    [Test]
    public void PauseStatePausesRegisteredPickupParticleSystems()
    {
        var registry = new ActorRegistry();
        PickupView pickup = TheCircussyOneTestObjects.CreateRoot("Pickup").AddComponent<PickupView>();
        ParticleSystem attractTrail = AddPlayingParticleSystem(pickup.transform, "XP Attract Trail");
        registry.Register(new PickupRuntime(pickup, 1));
        var pauseState = new RunPauseState();
        var system = new WorldAnimationPauseSystem(pauseState, null, registry);

        system.Start();
        pauseState.Pause("test");

        Assert.That(attractTrail.isPaused, Is.True);

        pauseState.Resume("test");

        Assert.That(attractTrail.isPaused, Is.False);
        Assert.That(attractTrail.isPlaying, Is.True);

        system.Dispose();
    }

    [Test]
    public void PauseStatePausesActiveOneShotVfxParticleSystems()
    {
        Transform root = TheCircussyOneTestObjects.CreateRoot("VFX").transform;
        var factory = new VfxEffectFactory(
            root,
            TheCircussyOneTestObjects.CreateParticleEffectPrefab(VfxEffectId.ProjectileMuzzlePuff),
            null,
            null,
            null,
            null,
            null);
        VfxEffectRuntime runtime = factory.Spawn(VfxEffectId.ProjectileMuzzlePuff, Vector3.zero, Quaternion.identity);
        ParticleSystem particles = runtime.View.GetComponentInChildren<ParticleSystem>(includeInactive: true);
        var pauseState = new RunPauseState();
        var system = new WorldAnimationPauseSystem(pauseState, null, new ActorRegistry(), factory);

        system.Start();
        pauseState.Pause("test");

        Assert.That(particles.isPaused, Is.True);

        pauseState.Resume("test");

        Assert.That(particles.isPaused, Is.False);
        Assert.That(particles.isPlaying, Is.True);

        system.Dispose();
    }

    [Test]
    public void PauseStatePausesActiveWorldRewardTransientParticles()
    {
        var time = new FakeGameTime();
        var transientVfx = new WorldRewardTransientVfxSystem(time);
        TicketDepositDefinition deposit = TheCircussyOneTestObjects.CreateTicketDepositDefinition(
            "small_ticket_stack",
            "Small Ticket Stack",
            holdSeconds: 0.1f);
        deposit.ticketBurstCount = 4;
        ParticleSystem particles = transientVfx.ShowTicketPaperBurst(deposit, Vector3.zero);
        var pauseState = new RunPauseState();
        var system = new WorldAnimationPauseSystem(
            pauseState,
            null,
            new ActorRegistry(),
            worldRewardTransientVfxSystem: transientVfx);

        system.Start();
        pauseState.Pause("test");

        Assert.That(particles.isPaused, Is.True);

        pauseState.Resume("test");

        Assert.That(particles.isPaused, Is.False);
        Assert.That(particles.isPlaying, Is.True);

        system.Dispose();
        Object.DestroyImmediate(deposit);
        Object.DestroyImmediate(particles.gameObject);
    }

    [Test]
    public void WorldRewardTransientVfxLifetimeUsesGameTime()
    {
        var time = new FakeGameTime { Time = 10f };
        var transientVfx = new WorldRewardTransientVfxSystem(time);
        TicketDepositDefinition deposit = TheCircussyOneTestObjects.CreateTicketDepositDefinition(
            "small_ticket_stack",
            "Small Ticket Stack",
            holdSeconds: 0.1f);
        deposit.ticketBurstLifetimeSeconds = 1f;
        ParticleSystem particles = transientVfx.ShowTicketPaperBurst(deposit, Vector3.zero);

        transientVfx.Tick();

        Assert.That(transientVfx.ActiveCount, Is.EqualTo(1));

        time.Time = 11.61f;
        transientVfx.Tick();

        Assert.That(transientVfx.ActiveCount, Is.EqualTo(0));
        Assert.That(particles == null, Is.False);
        Assert.That(particles.gameObject.activeSelf, Is.False);
        Assert.That(transientVfx.PooledTicketBurstCount, Is.EqualTo(1));

        Object.DestroyImmediate(particles.gameObject);
        Object.DestroyImmediate(deposit);
    }

    private static ParticleSystem AddPlayingParticleSystem(Transform parent, string name)
    {
        var child = new GameObject(name);
        child.transform.SetParent(parent, false);
        ParticleSystem particles = child.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.loop = true;
        main.duration = 1f;
        main.startLifetime = 1f;
        main.playOnAwake = false;
        particles.Play(withChildren: true);
        return particles;
    }
}
