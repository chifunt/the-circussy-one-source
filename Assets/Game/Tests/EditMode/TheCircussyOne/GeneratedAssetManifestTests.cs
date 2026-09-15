using System;
using System.Linq;
using NUnit.Framework;
using TheCircussyOne.VisualTests.Editor;
using UnityEditor;

public sealed class GeneratedAssetManifestTests
{
    [Test]
    public void ManifestPathsAreUnique()
    {
        string[] paths = TheCircussyOneGeneratedAssetManifest.All.Select(entry => entry.Path).ToArray();

        Assert.That(paths.Length, Is.EqualTo(paths.Distinct().Count()));
    }

    [Test]
    public void ManifestEntriesHaveRequiredMetadata()
    {
        foreach (GeneratedAssetManifestEntry entry in TheCircussyOneGeneratedAssetManifest.All)
        {
            Assert.That(entry.Path, Is.Not.Null.And.Not.Empty, entry.DisplayName);
            Assert.That(entry.DisplayName, Is.Not.Null.And.Not.Empty, entry.Path);
            Assert.That(Enum.IsDefined(typeof(GeneratedAssetCategory), entry.Category), Is.True, entry.Path);
            Assert.That(Enum.IsDefined(typeof(GeneratedAssetOwner), entry.Owner), Is.True, entry.Path);
            Assert.That(Enum.IsDefined(typeof(GeneratedAssetSafeAction), entry.SafeAction), Is.True, entry.Path);
            Assert.That(Enum.IsDefined(typeof(TheCircussyOne.Config.ConfigWorkflowKind), entry.WorkflowKind), Is.True, entry.Path);
        }
    }

    [Test]
    public void ExistingManifestBaselineResolvesThroughAssetDatabase()
    {
        foreach (GeneratedAssetManifestEntry entry in TheCircussyOneGeneratedAssetManifest.All)
        {
            Assert.That(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(entry.Path), Is.Not.Null, entry.Path);
        }
    }

    [Test]
    public void VisualTestLabManifestEntriesMatchBuiltInScenarioIds()
    {
        string[] scenarioIds = VisualTestLabBuilder.BuiltInScenarioIds.ToArray();
        string[] scenarioPaths = TheCircussyOneGeneratedAssetManifest.VisualTestLabAssets
            .Where(entry => entry.Category == GeneratedAssetCategory.VisualTestScenario)
            .Select(entry => entry.Path)
            .ToArray();
        string[] scenePaths = TheCircussyOneGeneratedAssetManifest.VisualTestLabAssets
            .Where(entry => entry.Category == GeneratedAssetCategory.VisualTestScene)
            .Select(entry => entry.Path)
            .ToArray();

        Assert.That(scenarioPaths, Is.EquivalentTo(scenarioIds.Select(id => $"{VisualTestLabBuilder.ScenarioFolder}/{id}.asset")));
        Assert.That(scenePaths, Is.SupersetOf(scenarioIds.Select(id => $"{VisualTestLabBuilder.SceneFolder}/{id}.unity")));
        Assert.That(scenePaths, Does.Contain(AuthoringShowroomBuilder.ScenePath));
    }

    [Test]
    public void AuthoringDoctorGeneratedAssetEntriesComeFromManifest()
    {
        Assert.That(AuthoringDoctorRunner.RequiredGeneratedAssetEntries.Select(entry => entry.Path),
            Is.EqualTo(TheCircussyOneGeneratedAssetManifest.CoreGeneratedAssets.Select(entry => entry.Path)));
    }

    [Test]
    public void AuthoringDoctorManifestEvaluationDoesNotAutoRunSafeAction()
    {
        var entry = new GeneratedAssetManifestEntry(
            "Assets/Game/__MissingManifestSafeActionTest.asset",
            GeneratedAssetCategory.Material,
            GeneratedAssetOwner.GridVisuals,
            TheCircussyOne.Config.ConfigWorkflowKind.ProjectedVisual,
            GeneratedAssetSafeAction.ApplyProjectedVisuals,
            "Missing Manifest Safe Action Test");

        AuthoringCheckResult result = AuthoringDoctorRunner.EvaluateManifestEntryForTests(entry);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.HasFix, Is.True);
        Assert.That(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(entry.Path), Is.Null);
    }

    [Test]
    public void ConfigHubCanSurfaceManifestBackedGeneratedAssetRows()
    {
        ConfigHubStatusRow[] rows = ConfigHubStatusModel.BuildGeneratedAssetRows().ToArray();

        Assert.That(rows.Select(row => row.AssetPath), Does.Contain(TheCircussyOneAssetPaths.DamageNumberPrefabPath));
        Assert.That(rows.Select(row => row.AssetPath), Does.Contain(TheCircussyOneAssetPaths.PlayerDamageVignetteMaterialPath));
        Assert.That(rows.Single(row => row.AssetPath == TheCircussyOneAssetPaths.DamageNumberPrefabPath).HasApplyAction, Is.True);
        Assert.That(rows.Single(row => row.AssetPath == TheCircussyOneAssetPaths.XpGainCounterPrefabPath).HasApplyAction, Is.True);
        Assert.That(rows.Single(row => row.AssetPath == TheCircussyOneAssetPaths.ScenePath).HasApplyAction, Is.False);
        Assert.That(rows.Single(row => row.AssetPath == AuthoringShowroomBuilder.ScenePath).HasApplyAction, Is.True);
    }
}
