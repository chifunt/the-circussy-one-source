using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using TheCircussyOne.Config;

public sealed class GameConfigHitboxDiagnosticsTests
{
    [Test]
    public void DefaultConfigHasValidEnemyContactFrontMargin()
    {
        GameConfig config = TheCircussyOneTestObjects.CreateConfig();

        Assert.That(config.HasValidEnemyContactFrontMargin, Is.True);

        TheCircussyOneTestObjects.Destroy(config);
    }

    [Test]
    public void TunedGameConfigAssetHasValidEnemyContactFrontMargin()
    {
        GameConfig config = AssetDatabase.LoadAssetAtPath<GameConfig>(TheCircussyOneAssetPaths.GameConfigPath);

        Assert.That(config, Is.Not.Null);
        Assert.That(config.HasValidEnemyContactFrontMargin, Is.True);
    }

    [Test]
    public void BuriedContactFrontIsReportedInvalid()
    {
        GameConfig config = TheCircussyOneTestObjects.CreateConfig();
        config.enemyMovementBodyRadius = 1f;
        config.enemyMovementBodyOffset = Vector3.zero;
        config.enemyContactHitboxSize = new Vector3(0.2f, 0.2f, 0.2f);
        config.enemyContactHitboxOffset = Vector3.zero;

        Assert.That(config.HasValidEnemyContactFrontMargin, Is.False);

        TheCircussyOneTestObjects.Destroy(config);
    }
}
