using System.Linq;
using NUnit.Framework;
using TheCircussyOne.Stats;
using UnityEditor.SceneManagement;

public sealed class RunStatsDebugTests
{
    [Test]
    public void BaselineSnapshotBuildsWithoutPlayMode()
    {
        RunStatsDebugSnapshot snapshot = RunStatsDebugModel.BuildBaselineSnapshot();

        Assert.That(snapshot, Is.Not.Null);
        Assert.That(snapshot.IsLive, Is.False);
        Assert.That(snapshot.Breakdowns, Has.Count.EqualTo(StatMetadata.All.Count));
        Assert.That(snapshot.Breakdowns.Any(breakdown => breakdown.Id == StatId.PlayerMaxHealth), Is.True);
    }

    [Test]
    public void ExportPlainTextContainsStatsValuesAndSources()
    {
        var config = TheCircussyOneTestObjects.CreateConfig();
        var stats = new RunStats(config);
        try
        {
            stats.AddModifier(StatModifier.Flat(StatId.PlayerArmor, 12f, "test_armor"));
            RunStatsDebugSnapshot snapshot = RunStatsDebugModel.BuildSnapshot(stats, "Test Stats", true);

            string text = RunStatsDebugModel.ExportPlainText(snapshot);

            Assert.That(text, Does.Contain("The Circussy One Run Stats"));
            Assert.That(text, Does.Contain("Armor"));
            Assert.That(text, Does.Contain("test_armor"));
            Assert.That(text, Does.Contain("Final").Or.Contain("12"));
        }
        finally
        {
            TheCircussyOneTestObjects.Destroy(config);
        }
    }

    [Test]
    public void ClearTemporaryDebugModifiersLeavesOtherSources()
    {
        var config = TheCircussyOneTestObjects.CreateConfig();
        var stats = new RunStats(config);
        try
        {
            stats.AddModifier(StatModifier.Flat(StatId.PlayerArmor, 12f, RunStatsDebugModel.DebugSourceId));
            stats.AddModifier(StatModifier.Flat(StatId.PlayerArmor, 7f, "real_item"));

            RunStatsDebugModel.ClearTemporaryDebugModifiers(stats);

            StatBreakdown breakdown = stats.GetBreakdown(StatId.PlayerArmor);
            Assert.That(breakdown.Modifiers.Count, Is.EqualTo(1));
            Assert.That(breakdown.Modifiers[0].SourceId, Is.EqualTo("real_item"));
            Assert.That(breakdown.FinalValue, Is.EqualTo(7f));
        }
        finally
        {
            TheCircussyOneTestObjects.Destroy(config);
        }
    }

    [Test]
    public void SnapshotGenerationDoesNotDirtyActiveScene()
    {
        bool wasDirty = EditorSceneManager.GetActiveScene().isDirty;

        RunStatsDebugModel.BuildBaselineSnapshot();

        Assert.That(EditorSceneManager.GetActiveScene().isDirty, Is.EqualTo(wasDirty));
    }

    [Test]
    public void DebuggerAndConfigHubEditorTypesCompile()
    {
        Assert.That(typeof(RunStatsDebugWindow), Is.Not.Null);
        Assert.That(typeof(TheCircussyOneConfigHub), Is.Not.Null);
    }
}
