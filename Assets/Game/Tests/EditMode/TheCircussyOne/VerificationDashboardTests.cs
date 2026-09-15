using System.IO;
using System.Linq;
using NUnit.Framework;

public sealed class VerificationDashboardTests
{
    [Test]
    public void SummaryCountsLeafResultsByStatus()
    {
        VerificationTestRunSummary summary = VerificationTestRunSummary.FromCaseResults(
            VerificationRunKind.EditMode,
            VerificationTestRunner.EditModeAssemblyName,
            "job-1",
            "2026-01-01T00:00:00.0000000Z",
            new[]
            {
                new VerificationTestCaseResult("Passed.Test", VerificationTestCaseStatus.Passed, 0.1d),
                new VerificationTestCaseResult("Failed.Test", VerificationTestCaseStatus.Failed, 0.2d),
                new VerificationTestCaseResult("Skipped.Test", VerificationTestCaseStatus.Skipped, 0.0d),
                new VerificationTestCaseResult("Inconclusive.Test", VerificationTestCaseStatus.Inconclusive, 0.3d)
            });

        Assert.That(summary.passCount, Is.EqualTo(1));
        Assert.That(summary.failCount, Is.EqualTo(1));
        Assert.That(summary.skipCount, Is.EqualTo(1));
        Assert.That(summary.inconclusiveCount, Is.EqualTo(1));
        Assert.That(summary.status, Is.EqualTo(VerificationRunStatus.Failed));
    }

    [Test]
    public void TestCasesSortFailuresBeforeSkippedAndPassed()
    {
        VerificationTestCaseStatus[] statuses = VerificationTestRunSummary.SortForDisplay(new[]
            {
                new VerificationTestCaseResult("Z.Passed", VerificationTestCaseStatus.Passed, 0.1d),
                new VerificationTestCaseResult("A.Skipped", VerificationTestCaseStatus.Skipped, 0.1d),
                new VerificationTestCaseResult("B.Failed", VerificationTestCaseStatus.Failed, 0.1d),
                new VerificationTestCaseResult("C.Inconclusive", VerificationTestCaseStatus.Inconclusive, 0.1d)
            })
            .Select(result => result.status)
            .ToArray();

        Assert.That(statuses, Is.EqualTo(new[]
        {
            VerificationTestCaseStatus.Failed,
            VerificationTestCaseStatus.Inconclusive,
            VerificationTestCaseStatus.Skipped,
            VerificationTestCaseStatus.Passed
        }));
    }

    [Test]
    public void StorePathLivesUnderLibraryAndRoundTripsJson()
    {
        string defaultPath = VerificationDashboardStore.DefaultStorePath.Replace('\\', '/');
        Assert.That(defaultPath, Does.Contain("/Library/TheCircussyOne/Verification/last-run.json"));
        Assert.That(defaultPath, Does.Not.Contain("/Assets/"));

        string testPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "Library",
            "TheCircussyOne",
            "Verification",
            "test-last-run.json");
        if (File.Exists(testPath))
        {
            File.Delete(testPath);
        }

        var state = new VerificationDashboardState();
        state.SetRun(VerificationTestRunSummary.FromCaseResults(
            VerificationRunKind.EditMode,
            VerificationTestRunner.EditModeAssemblyName,
            "job-2",
            "2026-01-01T00:00:00.0000000Z",
            new[] { new VerificationTestCaseResult("Roundtrip.Test", VerificationTestCaseStatus.Passed, 0.05d) }));
        state.coverageStatus = VerificationCoverageStatus.FromReport(
            true,
            "1.2.7",
            false,
            Path.Combine(Directory.GetCurrentDirectory(), "Library", "TheCircussyOne", "Coverage", "Report", "Summary.xml"),
            "ReportGenerator Summary",
            66.6d,
            2,
            3,
            "Project Code",
            25d,
            10,
            40);

        VerificationDashboardStore.SaveToPath(testPath, state);
        VerificationDashboardState loaded = VerificationDashboardStore.LoadFromPath(testPath);

        Assert.That(loaded.editModeRun.passCount, Is.EqualTo(1));
        Assert.That(loaded.editModeRun.testCases.Single().fullName, Is.EqualTo("Roundtrip.Test"));
        Assert.That(loaded.coverageStatus.reportFound, Is.True);
        Assert.That(loaded.coverageStatus.coveragePercent, Is.EqualTo(66.6d).Within(0.001d));
        Assert.That(loaded.coverageStatus.fullReportCoveragePercent, Is.EqualTo(25d).Within(0.001d));
    }

    [Test]
    public void CoveragePackageDetectorFindsInstalledPackage()
    {
        Assert.That(VerificationCoverageProbe.IsPackageInstalled(out string version), Is.True);
        Assert.That(version, Is.Not.Empty);
    }

    [Test]
    public void CoveragePackageDetectorReadsManifestJson()
    {
        string manifest = "{ \"dependencies\": { \"com.unity.testtools.codecoverage\": \"1.2.7\" } }";

        bool found = VerificationCoverageProbe.TryFindPackageVersionForTests(manifest, out string version);

        Assert.That(found, Is.True);
        Assert.That(version, Is.EqualTo("1.2.7"));
    }

    [Test]
    public void CoverageParserReadsReportGeneratorSummary()
    {
        string path = WriteCoverageTestFile(
            "Summary.xml",
            "<CoverageSummary>" +
            "<Summary><Coveredlines>45</Coveredlines><Coverablelines>200</Coverablelines><Linecoverage>22.5</Linecoverage></Summary>" +
            "<Assemblies>" +
            "<Assembly name=\"TheCircussyOne.Runtime\" coveredlines=\"30\" coverablelines=\"60\" />" +
            "<Assembly name=\"TheCircussyOne.Editor\" coveredlines=\"10\" coverablelines=\"40\" />" +
            "<Assembly name=\"TheCircussyOne.Tests.EditMode\" coveredlines=\"99\" coverablelines=\"100\" />" +
            "<Assembly name=\"MoreMountains.Tools\" coveredlines=\"5\" coverablelines=\"100\" />" +
            "</Assemblies>" +
            "</CoverageSummary>");

        bool parsed = VerificationCoverageProbe.TryParseReportGeneratorSummaryForTests(path, out VerificationCoverageStatus status);

        Assert.That(parsed, Is.True);
        Assert.That(status.reportFound, Is.True);
        Assert.That(status.reportKind, Is.EqualTo("ReportGenerator Summary"));
        Assert.That(status.coverageScope, Is.EqualTo("Project Code"));
        Assert.That(status.coveredPoints, Is.EqualTo(40));
        Assert.That(status.coverablePoints, Is.EqualTo(100));
        Assert.That(status.coveragePercent, Is.EqualTo(40d).Within(0.001d));
        Assert.That(status.fullReportCoveredPoints, Is.EqualTo(45));
        Assert.That(status.fullReportCoverablePoints, Is.EqualTo(200));
        Assert.That(status.fullReportCoveragePercent, Is.EqualTo(22.5d).Within(0.001d));
    }

    [Test]
    public void CoverageParserReadsOpenCoverSummary()
    {
        string path = WriteCoverageTestFile(
            "TestCoverageResults_0000.xml",
            "<CoverageSession>" +
            "<Summary numSequencePoints=\"80\" visitedSequencePoints=\"50\" sequenceCoverage=\"62.5\" />" +
            "<Modules>" +
            "<Module><ModuleName>TheCircussyOne.Runtime</ModuleName><Summary numSequencePoints=\"30\" visitedSequencePoints=\"20\" /></Module>" +
            "<Module><ModuleName>TheCircussyOne.Tests.EditMode</ModuleName><Summary numSequencePoints=\"20\" visitedSequencePoints=\"20\" /></Module>" +
            "<Module><ModuleName>MoreMountains.Tools</ModuleName><Summary numSequencePoints=\"30\" visitedSequencePoints=\"10\" /></Module>" +
            "</Modules>" +
            "</CoverageSession>");

        bool parsed = VerificationCoverageProbe.TryParseOpenCoverReportForTests(path, out VerificationCoverageStatus status);

        Assert.That(parsed, Is.True);
        Assert.That(status.reportKind, Is.EqualTo("OpenCover"));
        Assert.That(status.coverageScope, Is.EqualTo("Project Code"));
        Assert.That(status.coveredPoints, Is.EqualTo(20));
        Assert.That(status.coverablePoints, Is.EqualTo(30));
        Assert.That(status.coveragePercent, Is.EqualTo(66.666d).Within(0.01d));
        Assert.That(status.fullReportCoveredPoints, Is.EqualTo(50));
        Assert.That(status.fullReportCoverablePoints, Is.EqualTo(80));
        Assert.That(status.fullReportCoveragePercent, Is.EqualTo(62.5d).Within(0.001d));
    }

    [Test]
    public void CoverageParserHandlesMissingAndMalformedReportsWithoutThrowing()
    {
        string missingPath = Path.Combine(Directory.GetCurrentDirectory(), "Library", "TheCircussyOne", "Verification", "__missing_coverage.xml");
        string malformedPath = WriteCoverageTestFile("MalformedCoverage.xml", "<CoverageSession><Summary");

        Assert.That(VerificationCoverageProbe.TryParseOpenCoverReportForTests(missingPath, out _), Is.False);
        Assert.That(VerificationCoverageProbe.TryParseReportGeneratorSummaryForTests(malformedPath, out _), Is.False);
        Assert.That(() => VerificationCoverageStatus.Unknown("No report."), Throws.Nothing);
    }

    [Test]
    public void CoverageOutputPathsStayOutOfAssets()
    {
        string projectRoot = Directory.GetCurrentDirectory();
        string defaultCoverage = Path.Combine(projectRoot, "CodeCoverage", "Report", "Summary.xml");
        string libraryCoverage = Path.Combine(projectRoot, "Library", "TheCircussyOne", "Coverage", "Report", "Summary.xml");
        string assetPath = Path.Combine(projectRoot, "Assets", "Game", "Coverage", "Summary.xml");

        Assert.That(VerificationCoverageProbe.IsCoverageOutputIgnoredPath(defaultCoverage), Is.True);
        Assert.That(VerificationCoverageProbe.IsCoverageOutputIgnoredPath(libraryCoverage), Is.True);
        Assert.That(VerificationCoverageProbe.IsCoverageOutputIgnoredPath(assetPath), Is.False);
    }

    [Test]
    public void GitStatusParserSummarizesCleanDirtyStagedAndUntrackedOutputs()
    {
        VerificationGitStatus clean = VerificationGitProbe.FromStatusOutput(string.Empty);
        Assert.That(clean.isKnown, Is.True);
        Assert.That(clean.isClean, Is.True);
        Assert.That(clean.totalDirtyCount, Is.Zero);

        VerificationGitStatus dirty = VerificationGitProbe.FromStatusOutput(
            " M Assets/Modified.cs\n" +
            "M  Assets/Staged.cs\n" +
            "A  Assets/Added.cs\n" +
            "?? Assets/New.cs\n");

        Assert.That(dirty.isClean, Is.False);
        Assert.That(dirty.totalDirtyCount, Is.EqualTo(4));
        Assert.That(dirty.stagedCount, Is.EqualTo(2));
        Assert.That(dirty.modifiedCount, Is.EqualTo(1));
        Assert.That(dirty.untrackedCount, Is.EqualTo(1));
    }

    [Test]
    public void GitDiffCheckFailureIsReportedExplicitly()
    {
        VerificationGitStatus status = VerificationGitProbe.FromOutputs(string.Empty, "Assets/File.cs: trailing whitespace", 1);

        Assert.That(status.diffCheckKnown, Is.True);
        Assert.That(status.diffCheckPassed, Is.False);
        Assert.That(status.diffCheckMessage, Does.Contain("trailing whitespace"));
    }

    [Test]
    public void ConsoleProbeUnknownPathDoesNotThrow()
    {
        VerificationConsoleStatus status = VerificationConsoleProbe.RefreshWithTypeForTests(null);

        Assert.That(status.isKnown, Is.False);
        Assert.That(status.message, Does.Contain("LogEntries"));
    }

    [Test]
    public void ConfigHubAndDashboardEditorTypesCompileTogether()
    {
        Assert.That(typeof(TheCircussyOneConfigHub), Is.Not.Null);
        Assert.That(typeof(TheCircussyOneVerificationDashboardWindow), Is.Not.Null);
        Assert.That(typeof(VerificationTestRunner), Is.Not.Null);
        Assert.That(typeof(VerificationCoverageProbe), Is.Not.Null);
    }

    private static string WriteCoverageTestFile(string fileName, string contents)
    {
        string folder = Path.Combine(Directory.GetCurrentDirectory(), "Library", "TheCircussyOne", "Verification", "CoverageParserTests");
        Directory.CreateDirectory(folder);
        string path = Path.Combine(folder, fileName);
        File.WriteAllText(path, contents);
        return path;
    }
}
