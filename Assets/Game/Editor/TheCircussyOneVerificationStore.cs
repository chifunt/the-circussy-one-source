using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;

public static class VerificationDashboardStore
{
    public static string DefaultStorePath => Path.Combine(
        Directory.GetCurrentDirectory(),
        "Library",
        "TheCircussyOne",
        "Verification",
        "last-run.json");

    public static VerificationDashboardState Load()
    {
        return LoadFromPath(DefaultStorePath);
    }

    public static void Save(VerificationDashboardState state)
    {
        SaveToPath(DefaultStorePath, state);
    }

    public static VerificationDashboardState LoadFromPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return new VerificationDashboardState();
        }

        try
        {
            VerificationDashboardState state = JsonUtility.FromJson<VerificationDashboardState>(File.ReadAllText(path));
            return Normalize(state);
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Could not load verification dashboard state: {exception.Message}");
            return new VerificationDashboardState();
        }
    }

    public static void SaveToPath(string path, VerificationDashboardState state)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        string folder = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(folder))
        {
            Directory.CreateDirectory(folder);
        }

        File.WriteAllText(path, JsonUtility.ToJson(state ?? new VerificationDashboardState(), true));
    }

    private static VerificationDashboardState Normalize(VerificationDashboardState state)
    {
        state ??= new VerificationDashboardState();
        state.editModeRun ??= VerificationTestRunSummary.NotRun(VerificationRunKind.EditMode, VerificationTestRunner.EditModeAssemblyName);
        state.playModeRun ??= VerificationTestRunSummary.NotRun(VerificationRunKind.PlayModeVisualSmoke, VerificationTestRunner.PlayModeAssemblyName);
        state.doctorStatus ??= new VerificationDoctorStatus();
        state.consoleStatus ??= VerificationConsoleStatus.Unknown("Console has not been refreshed.");
        state.gitStatus ??= VerificationGitProbe.FromStatusOutput(string.Empty);
        state.coverageStatus ??= VerificationCoverageStatus.Unknown("Coverage has not been refreshed.");
        state.editModeRun.testCases ??= new List<VerificationTestCaseResult>();
        state.playModeRun.testCases ??= new List<VerificationTestCaseResult>();
        return state;
    }
}
