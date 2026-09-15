using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public sealed class TheCircussyOneVerificationDashboardWindow : EditorWindow
{
    private Vector2 scroll;
    private bool showEditModePassed;
    private bool showPlayModePassed;

    private enum DashboardVisualState
    {
        Neutral,
        Passed,
        Warning,
        Failed,
        Running
    }

    [MenuItem("Tools/The Circussy One/Verification Dashboard")]
    public static void Open()
    {
        var window = GetWindow<TheCircussyOneVerificationDashboardWindow>();
        window.titleContent = new GUIContent("Verification Dashboard");
        window.minSize = new Vector2(560f, 620f);
        window.Show();
    }

    private void OnEnable()
    {
        VerificationTestRunner.StateChanged += Repaint;
    }

    private void OnDisable()
    {
        VerificationTestRunner.StateChanged -= Repaint;
    }

    private void OnGUI()
    {
        VerificationDashboardState state = VerificationTestRunner.State;
        scroll = EditorGUILayout.BeginScrollView(scroll);
        DrawHeader(state);
        DrawTestCard("EditMode Tests", state.editModeRun, VerificationTestRunner.RunEditModeTests, ref showEditModePassed);
        DrawTestCard("PlayMode Visual Smoke", state.playModeRun, VerificationTestRunner.RunPlayModeVisualSmokeTests, ref showPlayModePassed);
        DrawCoverageCard(state);
        DrawDoctorCard(state);
        DrawConsoleCard(state);
        DrawGitCard(state);
        EditorGUILayout.EndScrollView();
    }

    private static void DrawHeader(VerificationDashboardState state)
    {
        DashboardVisualState visualState = OverallVisualState(state);
        using (new TintedCardScope(TintFor(visualState)))
        {
            DrawTitleRow("The Circussy One Verification Dashboard", visualState, OverallStatusText(state));
            EditorGUILayout.LabelField("Project-specific green gate. Use Unity Test Runner for detailed native browsing.", EditorStyles.wordWrappedLabel);
        }
    }

    private static DashboardVisualState OverallVisualState(VerificationDashboardState state)
    {
        if (state.editModeRun.status == VerificationRunStatus.Running || state.playModeRun.status == VerificationRunStatus.Running)
        {
            return DashboardVisualState.Running;
        }

        if (state.editModeRun.HasFailures || state.playModeRun.HasFailures || state.doctorStatus.errorCount > 0)
        {
            return DashboardVisualState.Failed;
        }

        if (state.consoleStatus.isKnown && (state.consoleStatus.errorCount > 0 || state.consoleStatus.warningCount > 0))
        {
            return DashboardVisualState.Warning;
        }

        if (state.editModeRun.status == VerificationRunStatus.Passed && state.playModeRun.status == VerificationRunStatus.Passed)
        {
            return DashboardVisualState.Passed;
        }

        return DashboardVisualState.Neutral;
    }

    private static string OverallStatusText(VerificationDashboardState state)
    {
        if (state.editModeRun.status == VerificationRunStatus.Running || state.playModeRun.status == VerificationRunStatus.Running)
        {
            return "Running";
        }

        if (state.editModeRun.HasFailures || state.playModeRun.HasFailures || state.doctorStatus.errorCount > 0)
        {
            return "Needs attention";
        }

        if (state.consoleStatus.isKnown && (state.consoleStatus.errorCount > 0 || state.consoleStatus.warningCount > 0))
        {
            return "Console not clean";
        }

        if (state.editModeRun.status == VerificationRunStatus.Passed && state.playModeRun.status == VerificationRunStatus.Passed)
        {
            return "Green";
        }

        return "Partially checked";
    }

    private static void DrawTestCard(string title, VerificationTestRunSummary summary, Func<string> runAction, ref bool showPassed)
    {
        DashboardVisualState visualState = VisualStateFor(summary.status);
        using (new TintedCardScope(TintFor(visualState)))
        {
            DrawTitleRow(title, visualState, StatusLabel(summary.status));
            EditorGUILayout.LabelField("Assembly", summary.assemblyName);
            DrawCountChips(summary.passCount, summary.failCount, summary.skipCount, summary.inconclusiveCount);
            if (!string.IsNullOrWhiteSpace(summary.finishedAtIso))
            {
                EditorGUILayout.LabelField("Finished", summary.finishedAtIso);
            }

            if (!string.IsNullOrWhiteSpace(summary.errorMessage))
            {
                EditorGUILayout.HelpBox(summary.errorMessage, MessageType.Error);
            }

            if (GUILayout.Button($"Run {title}", GUILayout.Height(28f)))
            {
                runAction.Invoke();
            }

            DrawInterestingTests(summary);
            int collapsedCount = summary.testCases.Count(test => test.status != VerificationTestCaseStatus.Failed && test.status != VerificationTestCaseStatus.Inconclusive);
            showPassed = EditorGUILayout.Foldout(showPassed, $"Passed / skipped tests ({collapsedCount})");
            if (showPassed)
            {
                DrawTestList(summary.testCases.Where(test => test.status != VerificationTestCaseStatus.Failed && test.status != VerificationTestCaseStatus.Inconclusive));
            }
        }
    }

    private static void DrawInterestingTests(VerificationTestRunSummary summary)
    {
        List<VerificationTestCaseResult> interesting = summary.testCases
            .Where(test => test.status == VerificationTestCaseStatus.Failed || test.status == VerificationTestCaseStatus.Inconclusive)
            .ToList();

        if (interesting.Count == 0)
        {
            if (summary.status == VerificationRunStatus.Passed && summary.TotalCount > 0)
            {
                DrawStatusLine("All tests passed.", DashboardVisualState.Passed);
            }

            return;
        }

        EditorGUILayout.Space(4f);
        DrawStatusLine("Failures", DashboardVisualState.Failed);
        DrawTestList(interesting);
    }

    private static void DrawTestList(IEnumerable<VerificationTestCaseResult> tests)
    {
        foreach (VerificationTestCaseResult test in tests)
        {
            DashboardVisualState visualState = VisualStateFor(test.status);
            using (new TintedCardScope(TintFor(visualState)))
            {
                DrawStatusLine($"{StatusLabel(test.status)}  {test.fullName}", visualState);
                EditorGUILayout.LabelField("Duration", $"{test.durationSeconds:0.000}s");
                if (!string.IsNullOrWhiteSpace(test.message))
                {
                    EditorGUILayout.LabelField(test.message, EditorStyles.wordWrappedLabel);
                }

                if (!string.IsNullOrWhiteSpace(test.stackTrace))
                {
                    EditorGUILayout.TextArea(test.stackTrace, GUILayout.MinHeight(44f));
                }
            }
        }
    }

    private static void DrawDoctorCard(VerificationDashboardState state)
    {
        DashboardVisualState visualState = state.doctorStatus.errorCount > 0
            ? DashboardVisualState.Failed
            : state.doctorStatus.warningCount > 0
                ? DashboardVisualState.Warning
                : state.doctorStatus.hasRun ? DashboardVisualState.Passed : DashboardVisualState.Neutral;
        using (new TintedCardScope(TintFor(visualState)))
        {
            DrawTitleRow("Authoring Doctor", visualState, state.doctorStatus.Summary);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Run Doctor"))
                {
                    VerificationTestRunner.RefreshDoctorSummary();
                }

                if (GUILayout.Button("Open Doctor"))
                {
                    TheCircussyOneAuthoringDoctorWindow.Open();
                }
            }
        }
    }

    private static void DrawCoverageCard(VerificationDashboardState state)
    {
        VerificationCoverageStatus coverage = state.coverageStatus ?? VerificationCoverageStatus.Unknown("Coverage has not been refreshed.");
        DashboardVisualState visualState = coverage.reportFound
            ? DashboardVisualState.Passed
            : coverage.packageInstalled ? DashboardVisualState.Warning : DashboardVisualState.Neutral;

        using (new TintedCardScope(TintFor(visualState)))
        {
            DrawTitleRow("Code Coverage", visualState, coverage.reportFound ? coverage.PercentText : "Informational");
            EditorGUILayout.LabelField("Package", coverage.packageInstalled
                ? $"{VerificationCoverageProbe.PackageName} {coverage.packageVersion}"
                : "Code Coverage package not installed.");
            EditorGUILayout.LabelField("Coverage Enabled", coverage.coverageEnabled ? "Yes" : "No");

            if (coverage.reportFound)
            {
                EditorGUILayout.LabelField("Scope", string.IsNullOrWhiteSpace(coverage.coverageScope) ? "Project Code" : coverage.coverageScope);
                EditorGUILayout.LabelField("Report Kind", coverage.reportKind ?? string.Empty);
                EditorGUILayout.LabelField("Project Covered Points", $"{coverage.coveredPoints} / {coverage.coverablePoints}");
                EditorGUILayout.LabelField("Full Report Coverage", $"{coverage.FullReportPercentText} ({coverage.fullReportCoveredPoints} / {coverage.fullReportCoverablePoints})");
                EditorGUILayout.LabelField("Report Path", coverage.reportPath ?? string.Empty, EditorStyles.wordWrappedLabel);
                if (!string.IsNullOrWhiteSpace(coverage.generatedAtIso))
                {
                    EditorGUILayout.LabelField("Generated", coverage.generatedAtIso);
                }
            }
            else
            {
                MessageType messageType = coverage.packageInstalled ? MessageType.Info : MessageType.Warning;
                EditorGUILayout.HelpBox(coverage.message ?? "No coverage report found.", messageType);
            }

            if (coverage.packageInstalled && !coverage.coverageEnabled)
            {
                EditorGUILayout.HelpBox("Unity only fully applies Code Coverage after it is enabled in Window/Analysis/Code Coverage and the editor is restarted.", MessageType.Info);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Refresh Coverage"))
                {
                    VerificationTestRunner.RefreshCoverageStatus();
                }

                if (GUILayout.Button("Open Code Coverage Window"))
                {
                    VerificationCoverageProbe.OpenCoverageWindow();
                }

                if (GUILayout.Button("Run EditMode Coverage"))
                {
                    VerificationTestRunner.RunEditModeCoverage();
                }
            }

            if (coverage.reportFound && GUILayout.Button("Reveal Latest Report"))
            {
                EditorUtility.RevealInFinder(coverage.reportPath);
            }
        }
    }

    private static void DrawConsoleCard(VerificationDashboardState state)
    {
        DashboardVisualState visualState = !state.consoleStatus.isKnown
            ? DashboardVisualState.Neutral
            : state.consoleStatus.errorCount > 0 ? DashboardVisualState.Failed
            : state.consoleStatus.warningCount > 0 ? DashboardVisualState.Warning
            : DashboardVisualState.Passed;
        using (new TintedCardScope(TintFor(visualState)))
        {
            DrawTitleRow("Console", visualState, state.consoleStatus.message ?? "Console status unknown.");
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Refresh Console"))
                {
                    VerificationTestRunner.RefreshConsoleStatus();
                }

                if (GUILayout.Button("Clear Console"))
                {
                    VerificationTestRunner.ClearConsole();
                }
            }
        }
    }

    private static void DrawGitCard(VerificationDashboardState state)
    {
        DashboardVisualState visualState = !state.gitStatus.isKnown
            ? DashboardVisualState.Neutral
            : state.gitStatus.isClean && state.gitStatus.diffCheckPassed ? DashboardVisualState.Passed
            : state.gitStatus.diffCheckKnown && !state.gitStatus.diffCheckPassed ? DashboardVisualState.Failed
            : DashboardVisualState.Warning;
        using (new TintedCardScope(TintFor(visualState)))
        {
            DrawTitleRow("Git Gate", visualState, state.gitStatus.summary ?? "Git status not refreshed.");
            EditorGUILayout.LabelField(state.gitStatus.summary ?? "Git status not refreshed.", EditorStyles.wordWrappedLabel);
            EditorGUILayout.LabelField(state.gitStatus.diffCheckMessage ?? "git diff --check not refreshed.", EditorStyles.wordWrappedLabel);
            if (!string.IsNullOrWhiteSpace(state.gitStatus.rawStatus))
            {
                EditorGUILayout.TextArea(state.gitStatus.rawStatus, GUILayout.MinHeight(44f));
            }

            if (GUILayout.Button("Refresh Git + Run diff --check"))
            {
                VerificationTestRunner.RefreshGitStatus();
            }
        }
    }

    private static void DrawTitleRow(string title, DashboardVisualState visualState, string statusText)
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Label(IconFor(visualState), GUILayout.Width(22f), GUILayout.Height(20f));
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel, GUILayout.MinWidth(170f));
            GUILayout.FlexibleSpace();
            GUIStyle style = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleRight,
                normal = { textColor = TextColorFor(visualState) }
            };
            EditorGUILayout.LabelField(statusText, style, GUILayout.MinWidth(130f));
        }
    }

    private static void DrawCountChips(int passed, int failed, int skipped, int inconclusive)
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            DrawChip($"{passed} PASSED", DashboardVisualState.Passed);
            DrawChip($"{failed} FAILED", failed > 0 ? DashboardVisualState.Failed : DashboardVisualState.Neutral);
            DrawChip($"{skipped} SKIPPED", skipped > 0 ? DashboardVisualState.Warning : DashboardVisualState.Neutral);
            DrawChip($"{inconclusive} INCONCLUSIVE", inconclusive > 0 ? DashboardVisualState.Failed : DashboardVisualState.Neutral);
        }
    }

    private static void DrawChip(string text, DashboardVisualState visualState)
    {
        Color previous = GUI.backgroundColor;
        GUI.backgroundColor = BackgroundColorFor(visualState);
        GUILayout.Label(new GUIContent(text, IconFor(visualState)), EditorStyles.miniButton, GUILayout.MinWidth(82f));
        GUI.backgroundColor = previous;
    }

    private static void DrawStatusLine(string text, DashboardVisualState visualState)
    {
        GUIStyle style = new GUIStyle(EditorStyles.label)
        {
            wordWrap = true,
            normal = { textColor = TextColorFor(visualState) }
        };
        EditorGUILayout.LabelField(new GUIContent(text, IconFor(visualState)), style);
    }

    private static DashboardVisualState VisualStateFor(VerificationRunStatus status)
    {
        return status switch
        {
            VerificationRunStatus.Passed => DashboardVisualState.Passed,
            VerificationRunStatus.Failed => DashboardVisualState.Failed,
            VerificationRunStatus.Cancelled => DashboardVisualState.Warning,
            VerificationRunStatus.Error => DashboardVisualState.Failed,
            VerificationRunStatus.Running => DashboardVisualState.Running,
            _ => DashboardVisualState.Neutral
        };
    }

    private static DashboardVisualState VisualStateFor(VerificationTestCaseStatus status)
    {
        return status switch
        {
            VerificationTestCaseStatus.Passed => DashboardVisualState.Passed,
            VerificationTestCaseStatus.Failed => DashboardVisualState.Failed,
            VerificationTestCaseStatus.Skipped => DashboardVisualState.Warning,
            VerificationTestCaseStatus.Inconclusive => DashboardVisualState.Failed,
            _ => DashboardVisualState.Neutral
        };
    }

    private static string StatusLabel(VerificationRunStatus status)
    {
        return status switch
        {
            VerificationRunStatus.Passed => "PASSED",
            VerificationRunStatus.Failed => "FAILED",
            VerificationRunStatus.Cancelled => "CANCELLED",
            VerificationRunStatus.Error => "ERROR",
            VerificationRunStatus.Running => "RUNNING",
            _ => "NOT RUN"
        };
    }

    private static string StatusLabel(VerificationTestCaseStatus status)
    {
        return status switch
        {
            VerificationTestCaseStatus.Passed => "PASSED",
            VerificationTestCaseStatus.Failed => "FAILED",
            VerificationTestCaseStatus.Skipped => "SKIPPED",
            VerificationTestCaseStatus.Inconclusive => "INCONCLUSIVE",
            _ => status.ToString().ToUpperInvariant()
        };
    }

    private static Texture IconFor(DashboardVisualState visualState)
    {
        string iconName = visualState switch
        {
            DashboardVisualState.Passed => "TestPassed",
            DashboardVisualState.Failed => "TestFailed",
            DashboardVisualState.Warning => "console.warnicon",
            DashboardVisualState.Running => "PlayButton",
            _ => "d_UnityEditor.ConsoleWindow"
        };

        return EditorGUIUtility.IconContent(iconName).image;
    }

    private static Color TextColorFor(DashboardVisualState visualState)
    {
        return visualState switch
        {
            DashboardVisualState.Passed => new Color(0.35f, 0.95f, 0.48f),
            DashboardVisualState.Failed => new Color(1f, 0.42f, 0.38f),
            DashboardVisualState.Warning => new Color(1f, 0.78f, 0.28f),
            DashboardVisualState.Running => new Color(0.42f, 0.72f, 1f),
            _ => EditorGUIUtility.isProSkin ? new Color(0.78f, 0.78f, 0.78f) : new Color(0.24f, 0.24f, 0.24f)
        };
    }

    private static Color BackgroundColorFor(DashboardVisualState visualState)
    {
        return visualState switch
        {
            DashboardVisualState.Passed => new Color(0.28f, 0.58f, 0.32f),
            DashboardVisualState.Failed => new Color(0.68f, 0.22f, 0.2f),
            DashboardVisualState.Warning => new Color(0.7f, 0.5f, 0.18f),
            DashboardVisualState.Running => new Color(0.26f, 0.42f, 0.68f),
            _ => Color.gray
        };
    }

    private static Color TintFor(DashboardVisualState visualState)
    {
        return visualState switch
        {
            DashboardVisualState.Passed => new Color(0.08f, 0.25f, 0.12f, 0.24f),
            DashboardVisualState.Failed => new Color(0.34f, 0.08f, 0.07f, 0.28f),
            DashboardVisualState.Warning => new Color(0.34f, 0.24f, 0.07f, 0.24f),
            DashboardVisualState.Running => new Color(0.08f, 0.16f, 0.3f, 0.24f),
            _ => new Color(0.16f, 0.16f, 0.16f, 0.14f)
        };
    }

    private readonly struct TintedCardScope : IDisposable
    {
        public TintedCardScope(Color tint)
        {
            Rect rect = EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            if (Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(rect, tint);
            }
        }

        public void Dispose()
        {
            EditorGUILayout.EndVertical();
        }
    }
}
