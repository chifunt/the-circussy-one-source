using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.TestTools.TestRunner.Api;

public enum VerificationRunKind
{
    EditMode,
    PlayModeVisualSmoke
}

public enum VerificationRunStatus
{
    NotRun,
    Running,
    Passed,
    Failed,
    Cancelled,
    Error
}

public enum VerificationTestCaseStatus
{
    Passed,
    Failed,
    Skipped,
    Inconclusive
}

[Serializable]
public sealed class VerificationTestCaseResult
{
    public string fullName;
    public VerificationTestCaseStatus status;
    public double durationSeconds;
    public string message;
    public string stackTrace;

    public VerificationTestCaseResult()
    {
    }

    public VerificationTestCaseResult(
        string fullName,
        VerificationTestCaseStatus status,
        double durationSeconds,
        string message = "",
        string stackTrace = "")
    {
        this.fullName = fullName;
        this.status = status;
        this.durationSeconds = durationSeconds;
        this.message = message ?? string.Empty;
        this.stackTrace = stackTrace ?? string.Empty;
    }
}

[Serializable]
public sealed class VerificationTestRunSummary
{
    public VerificationRunKind kind;
    public VerificationRunStatus status;
    public string assemblyName;
    public string jobId;
    public string startedAtIso;
    public string finishedAtIso;
    public double durationSeconds;
    public int passCount;
    public int failCount;
    public int skipCount;
    public int inconclusiveCount;
    public string errorMessage;
    public List<VerificationTestCaseResult> testCases = new();

    public int TotalCount => passCount + failCount + skipCount + inconclusiveCount;
    public bool HasFailures => failCount > 0 || inconclusiveCount > 0 || status == VerificationRunStatus.Error;
    public string CountsText => $"{passCount} passed, {failCount} failed, {skipCount} skipped, {inconclusiveCount} inconclusive";

    public static VerificationTestRunSummary NotRun(VerificationRunKind kind, string assemblyName)
    {
        return new VerificationTestRunSummary
        {
            kind = kind,
            status = VerificationRunStatus.NotRun,
            assemblyName = assemblyName,
            jobId = string.Empty,
            startedAtIso = string.Empty,
            finishedAtIso = string.Empty,
            errorMessage = string.Empty
        };
    }

    public static VerificationTestRunSummary Started(VerificationRunKind kind, string assemblyName, string jobId)
    {
        return new VerificationTestRunSummary
        {
            kind = kind,
            status = VerificationRunStatus.Running,
            assemblyName = assemblyName,
            jobId = jobId ?? string.Empty,
            startedAtIso = DateTime.UtcNow.ToString("O"),
            finishedAtIso = string.Empty,
            errorMessage = string.Empty
        };
    }

    public static VerificationTestRunSummary Error(VerificationRunKind kind, string assemblyName, string jobId, string message)
    {
        VerificationTestRunSummary summary = Started(kind, assemblyName, jobId);
        summary.status = VerificationRunStatus.Error;
        summary.finishedAtIso = DateTime.UtcNow.ToString("O");
        summary.errorMessage = message ?? "Test run failed.";
        return summary;
    }

    public static VerificationTestRunSummary FromResult(
        VerificationRunKind kind,
        string assemblyName,
        string jobId,
        string startedAtIso,
        ITestResultAdaptor result)
    {
        List<VerificationTestCaseResult> cases = new();
        CollectLeafResults(result, cases);
        VerificationTestRunSummary summary = FromCaseResults(kind, assemblyName, jobId, startedAtIso, cases);
        summary.durationSeconds = result != null ? result.Duration : summary.durationSeconds;
        if (result != null && result.ResultState != null && result.ResultState.IndexOf("cancel", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            summary.status = VerificationRunStatus.Cancelled;
        }

        return summary;
    }

    public static VerificationTestRunSummary FromCaseResults(
        VerificationRunKind kind,
        string assemblyName,
        string jobId,
        string startedAtIso,
        IEnumerable<VerificationTestCaseResult> cases)
    {
        var summary = new VerificationTestRunSummary
        {
            kind = kind,
            status = VerificationRunStatus.Passed,
            assemblyName = assemblyName ?? string.Empty,
            jobId = jobId ?? string.Empty,
            startedAtIso = startedAtIso ?? string.Empty,
            finishedAtIso = DateTime.UtcNow.ToString("O"),
            errorMessage = string.Empty,
            testCases = SortForDisplay(cases ?? Array.Empty<VerificationTestCaseResult>()).ToList()
        };

        summary.passCount = summary.testCases.Count(test => test.status == VerificationTestCaseStatus.Passed);
        summary.failCount = summary.testCases.Count(test => test.status == VerificationTestCaseStatus.Failed);
        summary.skipCount = summary.testCases.Count(test => test.status == VerificationTestCaseStatus.Skipped);
        summary.inconclusiveCount = summary.testCases.Count(test => test.status == VerificationTestCaseStatus.Inconclusive);
        summary.durationSeconds = summary.testCases.Sum(test => Math.Max(0d, test.durationSeconds));
        summary.status = summary.failCount > 0 || summary.inconclusiveCount > 0
            ? VerificationRunStatus.Failed
            : VerificationRunStatus.Passed;
        return summary;
    }

    public static IEnumerable<VerificationTestCaseResult> SortForDisplay(IEnumerable<VerificationTestCaseResult> cases)
    {
        return cases
            .OrderBy(test => StatusSortRank(test.status))
            .ThenBy(test => test.fullName, StringComparer.Ordinal);
    }

    private static void CollectLeafResults(ITestResultAdaptor result, List<VerificationTestCaseResult> cases)
    {
        if (result == null)
        {
            return;
        }

        if (result.HasChildren)
        {
            foreach (ITestResultAdaptor child in result.Children)
            {
                CollectLeafResults(child, cases);
            }

            return;
        }

        cases.Add(new VerificationTestCaseResult(
            result.FullName,
            ToCaseStatus(result.TestStatus),
            result.Duration,
            result.Message,
            result.StackTrace));
    }

    private static VerificationTestCaseStatus ToCaseStatus(TestStatus status)
    {
        return status switch
        {
            TestStatus.Passed => VerificationTestCaseStatus.Passed,
            TestStatus.Failed => VerificationTestCaseStatus.Failed,
            TestStatus.Skipped => VerificationTestCaseStatus.Skipped,
            _ => VerificationTestCaseStatus.Inconclusive
        };
    }

    private static int StatusSortRank(VerificationTestCaseStatus status)
    {
        return status switch
        {
            VerificationTestCaseStatus.Failed => 0,
            VerificationTestCaseStatus.Inconclusive => 1,
            VerificationTestCaseStatus.Skipped => 2,
            _ => 3
        };
    }
}

[Serializable]
public sealed class VerificationDoctorStatus
{
    public bool hasRun;
    public int errorCount;
    public int warningCount;
    public int infoCount;
    public string lastRunIso;

    public string Summary => hasRun
        ? $"Doctor: {errorCount} errors, {warningCount} warnings, {infoCount} info"
        : "Doctor has not been run from the dashboard.";
}

[Serializable]
public sealed class VerificationConsoleStatus
{
    public bool isKnown;
    public int errorCount;
    public int warningCount;
    public int logCount;
    public string message;

    public static VerificationConsoleStatus Unknown(string message)
    {
        return new VerificationConsoleStatus
        {
            isKnown = false,
            message = string.IsNullOrWhiteSpace(message) ? "Console counts unavailable." : message
        };
    }

    public static VerificationConsoleStatus Known(int errors, int warnings, int logs)
    {
        return new VerificationConsoleStatus
        {
            isKnown = true,
            errorCount = Math.Max(0, errors),
            warningCount = Math.Max(0, warnings),
            logCount = Math.Max(0, logs),
            message = $"{Math.Max(0, errors)} errors, {Math.Max(0, warnings)} warnings, {Math.Max(0, logs)} logs"
        };
    }
}

[Serializable]
public sealed class VerificationGitStatus
{
    public bool isKnown;
    public bool isClean;
    public int stagedCount;
    public int modifiedCount;
    public int untrackedCount;
    public int totalDirtyCount;
    public bool diffCheckKnown;
    public bool diffCheckPassed;
    public string summary;
    public string diffCheckMessage;
    public string rawStatus;
}

[Serializable]
public sealed class VerificationCoverageStatus
{
    public bool isKnown;
    public bool packageInstalled;
    public string packageVersion;
    public bool coverageEnabled;
    public bool reportFound;
    public string reportPath;
    public string reportKind;
    public string coverageScope;
    public double coveragePercent;
    public int coveredPoints;
    public int coverablePoints;
    public double fullReportCoveragePercent;
    public int fullReportCoveredPoints;
    public int fullReportCoverablePoints;
    public string generatedAtIso;
    public string message;

    public string PercentText => reportFound ? $"{coveragePercent:0.0}%" : "No report";
    public string FullReportPercentText => reportFound ? $"{fullReportCoveragePercent:0.0}%" : "No report";

    public static VerificationCoverageStatus Unknown(string message)
    {
        return new VerificationCoverageStatus
        {
            isKnown = false,
            packageInstalled = VerificationCoverageProbe.IsPackageInstalled(out string version),
            packageVersion = version,
            coverageEnabled = VerificationCoverageProbe.IsCoverageEnabled(),
            message = string.IsNullOrWhiteSpace(message) ? "Coverage status unknown." : message
        };
    }

    public static VerificationCoverageStatus NoReport(bool packageInstalled, string packageVersion, bool coverageEnabled, string message)
    {
        return new VerificationCoverageStatus
        {
            isKnown = true,
            packageInstalled = packageInstalled,
            packageVersion = packageVersion ?? string.Empty,
            coverageEnabled = coverageEnabled,
            reportFound = false,
            reportPath = string.Empty,
            reportKind = string.Empty,
            coverageScope = "Project Code",
            message = message ?? "No coverage report found."
        };
    }

    public static VerificationCoverageStatus FromReport(
        bool packageInstalled,
        string packageVersion,
        bool coverageEnabled,
        string reportPath,
        string reportKind,
        double coveragePercent,
        int coveredPoints,
        int coverablePoints,
        string coverageScope = "Project Code",
        double fullReportCoveragePercent = -1d,
        int fullReportCoveredPoints = -1,
        int fullReportCoverablePoints = -1)
    {
        double clampedCoverage = Math.Max(0d, Math.Min(100d, coveragePercent));
        double clampedFullCoverage = fullReportCoveragePercent >= 0d
            ? Math.Max(0d, Math.Min(100d, fullReportCoveragePercent))
            : clampedCoverage;
        int safeCoveredPoints = Math.Max(0, coveredPoints);
        int safeCoverablePoints = Math.Max(0, coverablePoints);

        return new VerificationCoverageStatus
        {
            isKnown = true,
            packageInstalled = packageInstalled,
            packageVersion = packageVersion ?? string.Empty,
            coverageEnabled = coverageEnabled,
            reportFound = true,
            reportPath = reportPath ?? string.Empty,
            reportKind = reportKind ?? string.Empty,
            coverageScope = string.IsNullOrWhiteSpace(coverageScope) ? "Project Code" : coverageScope,
            coveragePercent = clampedCoverage,
            coveredPoints = safeCoveredPoints,
            coverablePoints = safeCoverablePoints,
            fullReportCoveragePercent = clampedFullCoverage,
            fullReportCoveredPoints = Math.Max(0, fullReportCoveredPoints >= 0 ? fullReportCoveredPoints : safeCoveredPoints),
            fullReportCoverablePoints = Math.Max(0, fullReportCoverablePoints >= 0 ? fullReportCoverablePoints : safeCoverablePoints),
            generatedAtIso = File.Exists(reportPath) ? File.GetLastWriteTimeUtc(reportPath).ToString("O") : DateTime.UtcNow.ToString("O"),
            message = $"Project coverage report found: {clampedCoverage:0.0}%"
        };
    }
}

[Serializable]
public sealed class VerificationDashboardState
{
    public VerificationTestRunSummary editModeRun = VerificationTestRunSummary.NotRun(VerificationRunKind.EditMode, VerificationTestRunner.EditModeAssemblyName);
    public VerificationTestRunSummary playModeRun = VerificationTestRunSummary.NotRun(VerificationRunKind.PlayModeVisualSmoke, VerificationTestRunner.PlayModeAssemblyName);
    public VerificationDoctorStatus doctorStatus = new();
    public VerificationConsoleStatus consoleStatus = VerificationConsoleStatus.Unknown("Console has not been refreshed.");
    public VerificationGitStatus gitStatus = VerificationGitProbe.FromStatusOutput(string.Empty);
    public VerificationCoverageStatus coverageStatus = VerificationCoverageStatus.Unknown("Coverage has not been refreshed.");

    public VerificationTestRunSummary RunFor(VerificationRunKind kind)
    {
        return kind == VerificationRunKind.PlayModeVisualSmoke ? playModeRun : editModeRun;
    }

    public void SetRun(VerificationTestRunSummary summary)
    {
        if (summary == null)
        {
            return;
        }

        if (summary.kind == VerificationRunKind.PlayModeVisualSmoke)
        {
            playModeRun = summary;
        }
        else
        {
            editModeRun = summary;
        }
    }
}
