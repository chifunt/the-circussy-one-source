using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using TheCircussyOne.Config;
using TheCircussyOne.Runtime;

public sealed class EnemySpatialIndexTests
{
    private GameConfig _config;

    [SetUp]
    public void SetUp()
    {
        TheCircussyOneTestObjects.DestroyAll();
        _config = TheCircussyOneTestObjects.CreateConfig();
    }

    [TearDown]
    public void TearDown()
    {
        TheCircussyOneTestObjects.DestroyAll();
        TheCircussyOneTestObjects.Destroy(_config);
    }

    [Test]
    public void CollectNearbyIndicesReturnsOnlyNeighboringCells()
    {
        var enemies = new List<EnemyRuntime>
        {
            TheCircussyOneTestObjects.CreateEnemy(_config, Vector3.zero, "Near A"),
            TheCircussyOneTestObjects.CreateEnemy(_config, new Vector3(0.9f, 0f, 0f), "Near B"),
            TheCircussyOneTestObjects.CreateEnemy(_config, new Vector3(5f, 0f, 0f), "Far")
        };
        var index = new EnemySpatialIndex();
        var nearby = new List<int>();

        index.Rebuild(enemies, requestedCellSize: 1f);
        index.CollectNearbyIndices(Vector3.zero, nearby);

        Assert.That(nearby, Has.Member(0));
        Assert.That(nearby, Has.Member(1));
        Assert.That(nearby, Has.No.Member(2));
    }

    [Test]
    public void RebuildIgnoresDeadInactiveAndMissingViews()
    {
        EnemyRuntime alive = TheCircussyOneTestObjects.CreateEnemy(_config, Vector3.zero, "Alive");
        EnemyRuntime dead = TheCircussyOneTestObjects.CreateEnemy(_config, Vector3.right, "Dead");
        EnemyRuntime inactive = TheCircussyOneTestObjects.CreateEnemy(_config, Vector3.left, "Inactive");
        dead.SetHealth(0);
        inactive.View.gameObject.SetActive(false);
        var enemies = new List<EnemyRuntime> { alive, dead, inactive, null };
        var index = new EnemySpatialIndex();
        var nearby = new List<int>();

        index.Rebuild(enemies, requestedCellSize: 2f);
        index.CollectNearbyIndices(Vector3.zero, nearby);

        Assert.That(nearby, Is.EqualTo(new[] { 0 }));
    }
}
