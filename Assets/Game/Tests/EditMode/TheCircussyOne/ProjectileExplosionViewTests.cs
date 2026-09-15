using NUnit.Framework;
using UnityEngine;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;
using TheCircussyOne.Runtime;
using TheCircussyOne.Visuals;

public sealed class ProjectileExplosionViewTests
{
    private VfxVisualConfig _config;

    [SetUp]
    public void SetUp()
    {
        TheCircussyOneTestObjects.DestroyAll();
        _config = TheCircussyOneTestObjects.CreateVfxVisualConfig();
        _config.projectileExplosionOverlayDuration = 0.2f;
        _config.projectileExplosion.duration = 0.02f;
        _config.projectileExplosion.lifetime = 0.02f;
    }

    [TearDown]
    public void TearDown()
    {
        TheCircussyOneTestObjects.DestroyAll();
        TheCircussyOneTestObjects.Destroy(_config);
    }

    [Test]
    public void ViewCreatesShapesAndAppliesSuppliedRadius()
    {
        ProjectileExplosionView view = TheCircussyOneTestObjects.CreateProjectileExplosionPrefab(_config);
        ProjectileExplosionVisualFrame frame = ProjectileExplosionVisualRules.Evaluate(0.19f, 3f, _config);

        view.PlayAt(Vector3.one, Quaternion.identity, _config);
        view.ApplyFrame(frame, _config);

        Assert.That(view.RingRadius, Is.EqualTo(frame.Radius).Within(0.0001f));
        Assert.That(view.SphereRadius, Is.EqualTo(frame.Radius).Within(0.0001f));
        Assert.That(view.ParticleEffect, Is.Not.Null);
        Assert.That(view.IsActive, Is.True);
    }

    [Test]
    public void ExplosionSystemPoolsAndReleasesViewsAfterDuration()
    {
        ProjectileExplosionView prefab = TheCircussyOneTestObjects.CreateProjectileExplosionPrefab(_config);
        Transform root = TheCircussyOneTestObjects.CreateRoot("Explosion Root").transform;
        var time = new FakeGameTime { Time = 10f };
        var system = new ProjectileExplosionSystem(_config, prefab, root, time);

        system.Show(Vector3.zero, 2f, Vector3.forward);

        Assert.That(system.ActiveCount, Is.EqualTo(1));
        Assert.That(root.childCount, Is.EqualTo(1));
        Assert.That(root.GetChild(0).GetComponent<ProjectileExplosionView>().RingRadius, Is.GreaterThan(0f));

        time.Time = 11f;
        system.Tick();

        Assert.That(system.ActiveCount, Is.Zero);
        Assert.That(system.PooledCount, Is.EqualTo(1));
    }

    [Test]
    public void ExplosionSystemPrewarmsAndUsesPool()
    {
        ProjectileExplosionView prefab = TheCircussyOneTestObjects.CreateProjectileExplosionPrefab(_config);
        Transform root = TheCircussyOneTestObjects.CreateRoot("Prewarm Explosion Root").transform;
        var system = new ProjectileExplosionSystem(_config, prefab, root, new FakeGameTime());

        int created = system.Prewarm(2);
        system.Show(Vector3.zero, 2f, Vector3.forward);

        Assert.That(created, Is.EqualTo(2));
        Assert.That(system.ActiveCount, Is.EqualTo(1));
        Assert.That(system.PooledCount, Is.EqualTo(1));
    }

    [Test]
    public void DisabledOverlayStillAllowsParticleOnlyExplosion()
    {
        _config.projectileExplosionOverlayEnabled = false;
        ProjectileExplosionView prefab = TheCircussyOneTestObjects.CreateProjectileExplosionPrefab(_config);
        Transform root = TheCircussyOneTestObjects.CreateRoot("Particle Explosion Root").transform;
        var system = new ProjectileExplosionSystem(_config, prefab, root, new FakeGameTime());

        system.Show(Vector3.zero, 2f, Vector3.forward);

        ProjectileExplosionView view = root.GetChild(0).GetComponent<ProjectileExplosionView>();
        Assert.That(system.ActiveCount, Is.EqualTo(1));
        Assert.That(view.RingRadius, Is.Zero);
        Assert.That(view.SphereRadius, Is.Zero);
        Assert.That(view.ParticleEffect, Is.Not.Null);
    }
}
