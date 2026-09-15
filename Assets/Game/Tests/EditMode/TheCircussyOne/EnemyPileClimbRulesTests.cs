using NUnit.Framework;
using TheCircussyOne.Rules;

public sealed class EnemyPileClimbRulesTests
{
    [Test]
    public void PileHeightIsZeroBelowStartCount()
    {
        float height = EnemyPileClimbRules.MoundHeight(
            localEnemyCount: 5,
            distanceFromPileCenter: 0f,
            pileRadius: 2.4f,
            movementBodyHeight: 1.45f,
            layerHeightMultiplier: 0.45f,
            startCount: 6,
            enemiesPerLayer: 6,
            maxLayers: 4);

        Assert.That(height, Is.EqualTo(0f));
    }

    [Test]
    public void PileHeightIncreasesWithLocalDensity()
    {
        float sparse = EnemyPileClimbRules.MoundHeight(6, 0f, 2.4f, 1.45f, 0.45f, 6, 6, 4);
        float dense = EnemyPileClimbRules.MoundHeight(18, 0f, 2.4f, 1.45f, 0.45f, 6, 6, 4);

        Assert.That(sparse, Is.GreaterThan(0f));
        Assert.That(dense, Is.GreaterThan(sparse));
    }

    [Test]
    public void PileHeightIsTallestNearCenter()
    {
        float center = EnemyPileClimbRules.MoundHeight(18, 0f, 2.4f, 1.45f, 0.45f, 6, 6, 4);
        float edge = EnemyPileClimbRules.MoundHeight(18, 2.4f, 2.4f, 1.45f, 0.45f, 6, 6, 4);

        Assert.That(center, Is.GreaterThan(0f));
        Assert.That(edge, Is.EqualTo(0f));
    }

    [Test]
    public void LayerCountClampsToConfiguredMax()
    {
        int layers = EnemyPileClimbRules.LayerCount(100, 6, 6, 4);

        Assert.That(layers, Is.EqualTo(4));
    }
}
