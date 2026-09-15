using NUnit.Framework;
using UnityEngine;
using TheCircussyOne.Config;
using TheCircussyOne.Runtime;

public sealed class DamageNumberSystemTests
{
    private DamageFeedbackVisualConfig _config;
    private FakeGameTime _time;
    private DamageNumberFactory _factory;
    private DamageNumberSystem _system;

    [SetUp]
    public void SetUp()
    {
        TheCircussyOneTestObjects.DestroyAll();
        _config = TheCircussyOneTestObjects.CreateDamageFeedbackConfig();
        _config.sameEnemyNumberSpreadRadius = 0.4f;
        _config.damageNumberLateralDriftDistance = 0.2f;
        _config.damageNumberMergeWindowSeconds = 0.12f;
        _time = new FakeGameTime();
        _factory = new DamageNumberFactory(
            _config,
            TheCircussyOneTestObjects.CreateDamageNumberPrefab(),
            TheCircussyOneTestObjects.CreateRoot("Damage Numbers").transform);
        _system = new DamageNumberSystem(_config, _factory, null, _time);
    }

    [TearDown]
    public void TearDown()
    {
        TheCircussyOneTestObjects.DestroyAll();
        TheCircussyOneTestObjects.Destroy(_config);
    }

    [Test]
    public void ScatterModeKeepsOneNumberPerHit()
    {
        _config.damageNumberReadabilityMode = DamageNumberReadabilityMode.ScatterPerHit;

        _system.Show(new DamageNumberRequest(3, Vector3.up, Vector3.zero, 7));
        _system.Show(new DamageNumberRequest(4, Vector3.up, Vector3.zero, 7));

        Assert.That(_factory.Active, Has.Count.EqualTo(2));
        Assert.That(_factory.Active[0].Amount, Is.EqualTo(3));
        Assert.That(_factory.Active[1].Amount, Is.EqualTo(4));
        Assert.That(_factory.Active[0].StartPosition, Is.Not.EqualTo(_factory.Active[1].StartPosition));
    }

    [Test]
    public void HybridModeMergesRapidSameEnemyHits()
    {
        _config.damageNumberReadabilityMode = DamageNumberReadabilityMode.HybridMerge;

        _system.Show(new DamageNumberRequest(3, Vector3.up, Vector3.zero, 7));
        _time.Time = 0.04f;
        _system.Show(new DamageNumberRequest(4, Vector3.up, Vector3.zero, 7));

        Assert.That(_factory.Active, Has.Count.EqualTo(1));
        Assert.That(_factory.Active[0].Amount, Is.EqualTo(7));
    }

    [Test]
    public void HybridModeCreatesNewNumberAfterMergeWindow()
    {
        _config.damageNumberReadabilityMode = DamageNumberReadabilityMode.HybridMerge;

        _system.Show(new DamageNumberRequest(3, Vector3.up, Vector3.zero, 7));
        _time.Time = 0.2f;
        _system.Show(new DamageNumberRequest(4, Vector3.up, Vector3.zero, 7));

        Assert.That(_factory.Active, Has.Count.EqualTo(2));
    }
}
