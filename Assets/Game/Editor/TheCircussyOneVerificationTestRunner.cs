using System;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
using Debug = UnityEngine.Debug;

[InitializeOnLoad]
public static class VerificationTestRunner
{
    public const string EditModeAssemblyName = "TheCircussyOne.Tests.EditMode";
    public const string PlayModeAssemblyName = "TheCircussyOne.Tests.PlayMode";

    private static readonly DashboardTestCallbacks Callbacks = new();
    private static VerificationDashboardState state;
    private static VerificationRunKind activeKind;
    private static string activeAssemblyName;
    private static string activeJobId;
    private static string activeStartedAtIso;
    private static bool hasActiveRun;

    public static event Action StateChanged;

    static VerificationTestRunner()
    {
        state = VerificationDashboardStore.Load();
        TestRunnerApi.RegisterTestCallback(Callbacks);
    }

    public static VerificationDashboardState State => state ??= VerificationDashboardStore.Load();

    [MenuItem("Tools/The Circussy One/Verification/Run EditMode Tests")]
    public static string RunEditModeTests()
    {
        return RunTests(VerificationRunKind.EditMode, TestMode.EditMode, EditModeAssemblyName);
    }

    [MenuItem("Tools/The Circussy One/Verification/Run PlayMode Visual Smoke")]
    public static string RunPlayModeVisualSmokeTests()
    {
        return RunTests(VerificationRunKind.PlayModeVisualSmoke, TestMode.PlayMode, PlayModeAssemblyName);
    }

    public static string RunEditModeCoverage()
    {
        RefreshCoverageStatus();
        VerificationCoverageStatus coverageStatus = State.coverageStatus;
        if (!coverageStatus.packageInstalled)
        {
            Debug.LogWarning("Cannot run EditMode coverage because the Code Coverage package is not installed.");
            return string.Empty;
        }

        if (!coverageStatus.coverageEnabled)
        {
            Debug.LogWarning("Cannot run EditMode coverage because Code Coverage is disabled. Enable it in Window/Analysis/Code Coverage and restart Unity.");
            return string.Empty;
        }

        return RunEditModeTests();
    }

    public static void RefreshDoctorSummary()
    {
        ConfigHubDoctorSummary summary = ConfigHubStatusModel.GetDoctorSummary();
        State.doctorStatus = new VerificationDoctorStatus
        {
            hasRun = true,
            errorCount = summary.ErrorCount,
            warningCount = summary.WarningCount,
            infoCount = summary.InfoCount,
            lastRunIso = DateTime.UtcNow.ToString("O")
        };
        SaveAndNotify();
    }

    public static void RefreshCoverageStatus()
    {
        State.coverageStatus = VerificationCoverageProbe.Refresh();
        SaveAndNotify();
    }

    public static void RefreshConsoleStatus()
    {
        State.consoleStatus = VerificationConsoleProbe.Refresh();
        SaveAndNotify();
    }

    public static void ClearConsole()
    {
        VerificationConsoleProbe.Clear();
        RefreshConsoleStatus();
    }

    public static void RefreshGitStatus()
    {
        State.gitStatus = VerificationGitProbe.Refresh();
        SaveAndNotify();
    }

    private static string RunTests(VerificationRunKind kind, TestMode mode, string assemblyName)
    {
        var api = ScriptableObject.CreateInstance<TestRunnerApi>();
        var filter = new Filter
        {
            testMode = mode,
            assemblyNames = new[] { assemblyName }
        };

        string jobId = api.Execute(new ExecutionSettings(filter));
        MarkStarted(kind, assemblyName, jobId);
        Debug.Log($"The Circussy One verification dashboard started {mode} tests for {assemblyName}. Job: {jobId}");
        return jobId;
    }

    private static void MarkStarted(VerificationRunKind kind, string assemblyName, string jobId)
    {
        VerificationTestRunSummary summary = VerificationTestRunSummary.Started(kind, assemblyName, jobId);
        activeKind = kind;
        activeAssemblyName = assemblyName;
        activeJobId = jobId;
        activeStartedAtIso = summary.startedAtIso;
        hasActiveRun = true;
        State.SetRun(summary);
        SaveAndNotify();
    }

    private static void MarkFinished(ITestResultAdaptor result)
    {
        VerificationRunKind kind = hasActiveRun ? activeKind : InferKind(result?.Test);
        string assemblyName = hasActiveRun ? activeAssemblyName : InferAssemblyName(result?.Test, kind);
        string jobId = hasActiveRun ? activeJobId : string.Empty;
        string startedAtIso = hasActiveRun ? activeStartedAtIso : DateTime.UtcNow.ToString("O");

        VerificationTestRunSummary summary = VerificationTestRunSummary.FromResult(kind, assemblyName, jobId, startedAtIso, result);
        State.SetRun(summary);
        hasActiveRun = false;
        SaveAndNotify();
        EditorApplication.delayCall += RefreshCoverageStatus;
    }

    private static void MarkError(string message)
    {
        VerificationRunKind kind = hasActiveRun ? activeKind : VerificationRunKind.EditMode;
        string assemblyName = hasActiveRun ? activeAssemblyName : EditModeAssemblyName;
        string jobId = hasActiveRun ? activeJobId : string.Empty;
        State.SetRun(VerificationTestRunSummary.Error(kind, assemblyName, jobId, message));
        hasActiveRun = false;
        SaveAndNotify();
    }

    private static VerificationRunKind InferKind(ITestAdaptor test)
    {
        return test != null && test.TestMode == TestMode.PlayMode
            ? VerificationRunKind.PlayModeVisualSmoke
            : VerificationRunKind.EditMode;
    }

    private static string InferAssemblyName(ITestAdaptor test, VerificationRunKind kind)
    {
        if (test == null)
        {
            return kind == VerificationRunKind.PlayModeVisualSmoke ? PlayModeAssemblyName : EditModeAssemblyName;
        }

        if (test.IsTestAssembly)
        {
            return test.Name;
        }

        ITestAdaptor cursor = test;
        while (cursor.Parent != null)
        {
            cursor = cursor.Parent;
            if (cursor.IsTestAssembly)
            {
                return cursor.Name;
            }
        }

        return kind == VerificationRunKind.PlayModeVisualSmoke ? PlayModeAssemblyName : EditModeAssemblyName;
    }

    private static void SaveAndNotify()
    {
        VerificationDashboardStore.Save(State);
        StateChanged?.Invoke();
    }

    private sealed class DashboardTestCallbacks : IErrorCallbacks
    {
        public void RunStarted(ITestAdaptor testsToRun)
        {
            if (hasActiveRun)
            {
                return;
            }

            VerificationRunKind kind = InferKind(testsToRun);
            MarkStarted(kind, InferAssemblyName(testsToRun, kind), string.Empty);
        }

        public void RunFinished(ITestResultAdaptor result)
        {
            MarkFinished(result);
        }

        public void TestStarted(ITestAdaptor test)
        {
        }

        public void TestFinished(ITestResultAdaptor result)
        {
        }

        public void OnError(string message)
        {
            MarkError(message);
        }
    }
}
