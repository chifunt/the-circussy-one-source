using NUnit.Framework;
using UnityEngine;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Runtime;

public sealed class EnemySwarmDebugTests
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
    public void SnapshotCountsSupportClimbsAndStackHeights()
    {
        EnemyDefinition definition = TheCircussyOneTestObjects.CreateEnemyDefinition(_config);
        definition.stack.policy = EnemyStackPolicy.SupportBased;
        definition.stack.canClimbEnemies = true;
        definition.stack.canBeStackedOn = true;
        definition.stack.supportClimbSpeedMultiplier = 0.3f;
        EnemyRuntime support = TheCircussyOneTestObjects.CreateEnemy(definition, Vector3.zero, "Support", 0);
        EnemyRuntime climber = TheCircussyOneTestObjects.CreateEnemy(definition, new Vector3(0.2f, 0.4f, 0f), "Climber", 1);
        climber.VerticalMotor.BeginSupportClimb(1.45f, 0.08f, 1, support.SpawnId);

        EnemySwarmDebugSnapshot snapshot = EnemySwarmDebugModel.BuildSnapshot(
            new[] { support, climber },
            "Test Swarm",
            true);

        Assert.That(snapshot.SourceName, Is.EqualTo("Test Swarm"));
        Assert.That(snapshot.IsLive, Is.True);
        Assert.That(snapshot.EnemyCount, Is.EqualTo(2));
        Assert.That(snapshot.ActiveSupportClimbs, Is.EqualTo(1));
        Assert.That(snapshot.MaxStackLayer, Is.EqualTo(1));
        Assert.That(snapshot.AverageStackedHeight, Is.EqualTo(0.4f).Within(0.001f));
        Assert.That(snapshot.MaxStackedHeight, Is.EqualTo(0.4f).Within(0.001f));
        Assert.That(snapshot.SupportClimbSpeedMultiplier, Is.EqualTo(0.3f).Within(0.001f));
        Object.DestroyImmediate(definition);
    }

    [Test]
    public void SnapshotExportContainsUsefulSummary()
    {
        var snapshot = new EnemySwarmDebugSnapshot(
            "Unit Test",
            true,
            enemyCount: 12,
            activeSupportClimbs: 3,
            maxStackLayer: 2,
            averageStackedHeight: 0.8f,
            maxStackedHeight: 1.6f,
            supportClimbSpeedMultiplier: 0.3f,
            warning: null);

        string text = EnemySwarmDebugModel.ExportPlainText(snapshot);

        Assert.That(text, Does.Contain("Enemies: 12"));
        Assert.That(text, Does.Contain("Active support climbs: 3"));
        Assert.That(text, Does.Contain("Max stack layer: 2"));
        Assert.That(text, Does.Contain("Support climb speed multiplier: 0.3x"));
    }

    [Test]
    public void GizmoFrameReportsSupportClimbHoldState()
    {
        EnemyDefinition definition = TheCircussyOneTestObjects.CreateEnemyDefinition(_config);
        EnemyRuntime support = TheCircussyOneTestObjects.CreateEnemy(definition, Vector3.zero, "Support", 0);
        EnemyRuntime climber = TheCircussyOneTestObjects.CreateEnemy(definition, new Vector3(0.2f, 0.4f, 0f), "Climber", 1);
        climber.VerticalMotor.BeginSupportClimb(1.45f, 0.08f, 1, support.SpawnId);

        EnemySupportClimbGizmoFrame frame = EnemySupportClimbGizmoModel.Build(climber, support);

        Assert.That(frame.HasRuntime, Is.True);
        Assert.That(frame.IsSupportClimbing, Is.True);
        Assert.That(frame.IsHoldingHorizontal, Is.True);
        Assert.That(frame.StackLayer, Is.EqualTo(1));
        Assert.That(frame.SupportedBySpawnId, Is.EqualTo(support.SpawnId));
        Assert.That(frame.TargetHeight, Is.EqualTo(1.45f).Within(0.001f));
        Assert.That(frame.HasSupportPosition, Is.True);
        Assert.That(frame.SupportPosition, Is.EqualTo(support.Position));
        Object.DestroyImmediate(definition);
    }
}
