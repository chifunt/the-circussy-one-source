using System.Collections.Generic;
using TheCircussyOne.VisualTests.Editor;


public static partial class AuthoringDoctorRunner
{
    private sealed class ConfigAssetsCheck : IAuthoringCheck
    {
        public void Run(List<AuthoringCheckResult> results)
        {
            int missingCount = 0;
            for (int i = 0; i < ConfigAssetPaths.Length; i++)
            {
                AuthoringCheckResult result = RequiredAssetResult(
                    ConfigAssetPaths[i],
                    "Configs",
                    CreateMissingConfigs,
                    "Create Missing Configs");
                if (result != null)
                {
                    missingCount++;
                    results.Add(result);
                }
            }

            if (missingCount == 0)
            {
                results.Add(new AuthoringCheckResult(
                    "configs.ok",
                    "Configs",
                    AuthoringCheckSeverity.Info,
                    "Config assets found",
                    "All canonical config assets exist at their expected paths."));
            }
        }
    }

    private sealed class GeneratedAssetsCheck : IAuthoringCheck
    {
        public void Run(List<AuthoringCheckResult> results)
        {
            int missingCount = 0;
            IReadOnlyList<GeneratedAssetManifestEntry> entries = TheCircussyOneGeneratedAssetManifest.CoreGeneratedAssets;
            for (int i = 0; i < entries.Count; i++)
            {
                AuthoringCheckResult result = RequiredAssetResult(entries[i]);
                if (result != null)
                {
                    missingCount++;
                    results.Add(result);
                }
            }

            if (missingCount == 0)
            {
                results.Add(new AuthoringCheckResult(
                    "generated-assets.ok",
                    "Generated Assets",
                    AuthoringCheckSeverity.Info,
                    "Generated assets found",
                    "Core generated prefabs, materials, HUD assets, grid assets, lighting assets, and VFX prefabs exist."));
            }
        }
    }

    private sealed class VisualTestLabCheck : IAuthoringCheck
    {
        public void Run(List<AuthoringCheckResult> results)
        {
            int missingCount = 0;
            IReadOnlyList<GeneratedAssetManifestEntry> entries = TheCircussyOneGeneratedAssetManifest.VisualTestLabAssets;
            for (int i = 0; i < entries.Count; i++)
            {
                AuthoringCheckResult result = RequiredAssetResult(entries[i], AuthoringCheckSeverity.Warning);
                if (result != null)
                {
                    missingCount++;
                    results.Add(result);
                }
            }

            if (missingCount == 0)
            {
                results.Add(new AuthoringCheckResult(
                    "visual-test-lab.ok",
                    "Visual Test Lab",
                    AuthoringCheckSeverity.Info,
                    "Visual Test Lab assets found",
                    "All registered Visual Test Lab scenario assets and scenes exist."));
            }
        }
    }

    private sealed class ScenarioBrowserCheck : IAuthoringCheck
    {
        public void Run(List<AuthoringCheckResult> results)
        {
            if (!VisualTestCaptureUtility.IsCaptureFolderIgnoredForTests())
            {
                results.Add(new AuthoringCheckResult(
                    "scenario-browser.captures-not-ignored",
                    "Visual Test Lab",
                    AuthoringCheckSeverity.Warning,
                    "Generated captures are not ignored",
                    "Visual Test Lab generated captures should stay ignored unless intentionally promoted.",
                    ".gitignore"));
                return;
            }

            results.Add(new AuthoringCheckResult(
                "scenario-browser.ok",
                "Visual Test Lab",
                AuthoringCheckSeverity.Info,
                "Visual Almanac tooling available",
                "Visual Almanac inspection, editor playback, review beats, and optional evidence export are available."));
        }
    }

    private sealed class RunStatsDebugCheck : IAuthoringCheck
    {
        public void Run(List<AuthoringCheckResult> results)
        {
            RunStatsDebugSnapshot snapshot = RunStatsDebugModel.BuildBaselineSnapshot();
            if (snapshot.Breakdowns == null || snapshot.Breakdowns.Count == 0)
            {
                results.Add(new AuthoringCheckResult(
                    "run-stats-debug.empty",
                    "Debug Tools",
                    AuthoringCheckSeverity.Warning,
                    "Run Stats debugger has no stats",
                    "Run Stats debugger could not build a baseline stat snapshot."));
                return;
            }

            results.Add(new AuthoringCheckResult(
                "run-stats-debug.ok",
                "Debug Tools",
                AuthoringCheckSeverity.Info,
                "Run Stats debugger available",
                "Run Stats debugger can build baseline stat breakdowns for authoring checks."));
        }
    }
}
