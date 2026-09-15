using NUnit.Framework;
using UnityEngine;
using TheCircussyOne.Config;

public sealed class ActorMaterialTests
{
    private GameConfig _config;
    private DamageFeedbackVisualConfig _feedbackConfig;

    [SetUp]
    public void SetUp()
    {
        TheCircussyOneTestObjects.DestroyAll();
        _config = TheCircussyOneTestObjects.CreateConfig();
        _feedbackConfig = TheCircussyOneTestObjects.CreateDamageFeedbackConfig();
    }

    [TearDown]
    public void TearDown()
    {
        TheCircussyOneTestObjects.DestroyAll();
        TheCircussyOneTestObjects.Destroy(_config);
        TheCircussyOneTestObjects.Destroy(_feedbackConfig);
    }

    [Test]
    public void ActorPrepareDoesNotSetPersistentColorPropertyBlocks()
    {
        var propertyBlock = new MaterialPropertyBlock();

        var player = TheCircussyOneTestObjects.CreatePlayer(_config);
        AssertEmptyPropertyBlock(player.GetComponentInChildren<Renderer>(), propertyBlock);

        var enemy = TheCircussyOneTestObjects.CreateEnemyPrefab();
        enemy.Prepare(_config, _feedbackConfig, Vector3.zero);
        AssertEmptyPropertyBlock(enemy.GetComponentInChildren<Renderer>(), propertyBlock);

        var pickup = TheCircussyOneTestObjects.CreatePickupPrefab();
        pickup.Prepare(_config, _feedbackConfig, Vector3.zero);
        AssertEmptyPropertyBlock(pickup.GetComponentInChildren<Renderer>(), propertyBlock);
    }

    [Test]
    public void EnemyHitFlashUsesPropertyBlockTemporarily()
    {
        var enemy = TheCircussyOneTestObjects.CreateEnemyPrefab();
        enemy.Prepare(_config, _feedbackConfig, Vector3.zero);
        Renderer renderer = enemy.GetComponentInChildren<Renderer>();
        var propertyBlock = new MaterialPropertyBlock();

        AssertEmptyPropertyBlock(renderer, propertyBlock);

        enemy.PlayHit(0.1f, _feedbackConfig);
        renderer.GetPropertyBlock(propertyBlock);

        Assert.That(propertyBlock.isEmpty, Is.False);

        enemy.Prepare(_config, _feedbackConfig, Vector3.one);
        AssertEmptyPropertyBlock(renderer, propertyBlock);
    }

    private static void AssertEmptyPropertyBlock(Renderer renderer, MaterialPropertyBlock propertyBlock)
    {
        renderer.GetPropertyBlock(propertyBlock);
        Assert.That(propertyBlock.isEmpty, Is.True);
    }
}
