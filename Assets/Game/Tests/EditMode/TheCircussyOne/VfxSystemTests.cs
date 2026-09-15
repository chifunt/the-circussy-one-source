using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using TheCircussyOne.Config;
using TheCircussyOne.Runtime;
using TheCircussyOne.Visuals;

public sealed class VfxSystemTests
{
    private VfxVisualConfig _config;
    private Transform _root;

    [SetUp]
    public void SetUp()
    {
        TheCircussyOneTestObjects.DestroyAll();
        _config = TheCircussyOneTestObjects.CreateVfxVisualConfig();
        _root = TheCircussyOneTestObjects.CreateRoot("VFX").transform;
    }

    [TearDown]
    public void TearDown()
    {
        TheCircussyOneTestObjects.DestroyAll();
        TheCircussyOneTestObjects.Destroy(_config);
    }

    [Test]
    public void FactoryReusesPooledOneShotView()
    {
        VfxEffectFactory factory = CreateFactory();
        VfxEffectRuntime first = factory.Spawn(VfxEffectId.EnemyHitSparks, Vector3.zero, Quaternion.identity);

        ParticleEffectView firstView = first.View;
        factory.Release(first);
        VfxEffectRuntime second = factory.Spawn(VfxEffectId.EnemyHitSparks, Vector3.right, Quaternion.identity);

        Assert.That(second.View, Is.SameAs(firstView));
        Assert.That(factory.Active, Has.Count.EqualTo(1));
        Assert.That(factory.PooledCount(VfxEffectId.EnemyHitSparks), Is.Zero);
    }

    [Test]
    public void FactoryPrewarmsRegisteredOneShotViews()
    {
        VfxEffectFactory factory = CreateFactory();

        int created = factory.PrewarmPerRegisteredEffect(2);
        VfxEffectRuntime runtime = factory.Spawn(VfxEffectId.EnemyHitSparks, Vector3.zero, Quaternion.identity);

        Assert.That(created, Is.GreaterThanOrEqualTo(2));
        Assert.That(factory.PooledCount(VfxEffectId.EnemyHitSparks), Is.EqualTo(1));
        Assert.That(runtime.View.IsActive, Is.True);
    }

    [Test]
    public void VfxSystemShowsEnabledEffectAtPosition()
    {
        var system = new VfxSystem(_config, CreateFactory());

        system.Show(VfxEffectId.EnemyDeathBurst, new Vector3(1f, 2f, 3f));

        Assert.That(system.ActiveCount, Is.EqualTo(1));
        Assert.That(_root.GetChild(0).position, Is.EqualTo(new Vector3(1f, 2f, 3f)));
    }

    [Test]
    public void VfxSystemIgnoresDisabledConfig()
    {
        _config.enabled = false;
        var system = new VfxSystem(_config, CreateFactory());

        system.Show(VfxEffectId.EnemyDeathBurst, Vector3.zero);

        Assert.That(system.ActiveCount, Is.Zero);
    }

    [Test]
    public void VfxSystemShowsJumpTakeoffAndLandingEffects()
    {
        var system = new VfxSystem(_config, CreateFactory());

        system.Show(VfxEffectId.PlayerJumpTakeoff, Vector3.zero);
        system.Show(VfxEffectId.PlayerJumpLand, Vector3.forward);

        Assert.That(system.ActiveCount, Is.EqualTo(2));
    }

    [Test]
    public void VfxSystemShowsProjectileExplosionEffect()
    {
        var system = new VfxSystem(_config, CreateFactory());

        system.Show(VfxEffectId.ProjectileExplosion, Vector3.one);

        Assert.That(system.ActiveCount, Is.EqualTo(1));
    }

    [Test]
    public void FeedbackServiceEmitsShootAndPlayerDamageVfx()
    {
        GameConfig gameConfig = TheCircussyOneTestObjects.CreateConfig();
        DamageFeedbackVisualConfig damageConfig = TheCircussyOneTestObjects.CreateDamageFeedbackConfig();
        ActorMotionVisualConfig motionConfig = TheCircussyOneTestObjects.CreateActorMotionVisualConfig();
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(gameConfig);
        var vfx = new FakeVfxSpawner();
        var feedback = new FeedbackService(gameConfig, damageConfig, motionConfig, player, null, null, null, null, vfx);

        feedback.PlayShoot(new Vector3(2f, 0f, 0f));
        feedback.PlayPlayerDamage();

        Assert.That(vfx.Calls, Has.Count.EqualTo(2));
        Assert.That(vfx.Calls[0].effectId, Is.EqualTo(VfxEffectId.ProjectileMuzzlePuff));
        Assert.That(vfx.Calls[0].position, Is.EqualTo(new Vector3(2f, 0f, 0f)));
        Assert.That(vfx.Calls[1].effectId, Is.EqualTo(VfxEffectId.PlayerDamageBurst));
    }

    [Test]
    public void LevelUpPresenterIgnoresInitialResetAndEmitsForRealLevelUp()
    {
        GameConfig gameConfig = TheCircussyOneTestObjects.CreateConfig();
        gameConfig.experienceCurveEarlyBase = 2;
        var state = new GameState(gameConfig);
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(gameConfig);
        var vfx = new FakeVfxSpawner();
        var presenter = new LevelUpVfxPresenter(state, player, vfx);

        presenter.Start();
        state.Reset();
        state.AddExperience(state.ExperienceTarget);
        presenter.Dispose();

        Assert.That(vfx.Calls, Has.Count.EqualTo(1));
        Assert.That(vfx.Calls[0].effectId, Is.EqualTo(VfxEffectId.LevelUpBurst));
    }

    private VfxEffectFactory CreateFactory()
    {
        return new VfxEffectFactory(
            _root,
            TheCircussyOneTestObjects.CreateParticleEffectPrefab(VfxEffectId.ProjectileMuzzlePuff),
            TheCircussyOneTestObjects.CreateParticleEffectPrefab(VfxEffectId.EnemyHitSparks),
            TheCircussyOneTestObjects.CreateParticleEffectPrefab(VfxEffectId.EnemyDeathBurst),
            TheCircussyOneTestObjects.CreateParticleEffectPrefab(VfxEffectId.XpPickupCollectPop),
            TheCircussyOneTestObjects.CreateParticleEffectPrefab(VfxEffectId.PlayerDamageBurst),
            TheCircussyOneTestObjects.CreateParticleEffectPrefab(VfxEffectId.LevelUpBurst),
            TheCircussyOneTestObjects.CreateParticleEffectPrefab(VfxEffectId.PlayerJumpTakeoff),
            TheCircussyOneTestObjects.CreateParticleEffectPrefab(VfxEffectId.PlayerJumpLand),
            TheCircussyOneTestObjects.CreateParticleEffectPrefab(VfxEffectId.ProjectileExplosion));
    }
}

internal sealed class FakeVfxSpawner : IVfxSpawner
{
    public readonly List<(VfxEffectId effectId, Vector3 position, Vector3 direction)> Calls = new();

    public void Show(VfxEffectId effectId, Vector3 position)
    {
        Show(effectId, position, Vector3.forward);
    }

    public void Show(VfxEffectId effectId, Vector3 position, Vector3 direction)
    {
        Calls.Add((effectId, position, direction));
    }
}
