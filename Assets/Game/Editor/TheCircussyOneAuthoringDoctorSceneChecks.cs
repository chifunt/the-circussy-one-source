using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TheCircussyOne.Runtime;
using TheCircussyOne.VisualTests.Editor;
using UnityEngine;
using UnityEngine.SceneManagement;


public static partial class AuthoringDoctorRunner
{
    private sealed class LayerSetupCheck : IAuthoringCheck
    {
        public void Run(List<AuthoringCheckResult> results)
        {
            EvaluateLayerSetup(results, LayerMask.NameToLayer, Physics.GetIgnoreLayerCollision);
            if (!results.Any(result => result.Category == "Layers"))
            {
                results.Add(new AuthoringCheckResult(
                    "layers.ok",
                    "Layers",
                    AuthoringCheckSeverity.Info,
                    "Gameplay layers configured",
                    "Player, Enemy, Projectile, and Pickup layers exist, and Player/Projectile collision is ignored."));
            }
        }
    }

    private sealed class OpenPrototypeSceneCheck : IAuthoringCheck
    {
        public void Run(List<AuthoringCheckResult> results)
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (!string.Equals(activeScene.path, TheCircussyOneAssetPaths.ScenePath, StringComparison.Ordinal))
            {
                results.Add(new AuthoringCheckResult(
                    "prototype-scene.not-open",
                    "Open Scene",
                    AuthoringCheckSeverity.Info,
                    "Prototype scene is not active",
                    "Generated root checks are skipped unless the canonical prototype scene is the active open scene.",
                    TheCircussyOneAssetPaths.ScenePath));
                return;
            }

            int missingCount = 0;
            for (int i = 0; i < RequiredPrototypeRoots.Length; i++)
            {
                if (GameObject.Find(RequiredPrototypeRoots[i]) != null)
                {
                    continue;
                }

                missingCount++;
                results.Add(new AuthoringCheckResult(
                    $"prototype-scene.missing-root.{RequiredPrototypeRoots[i]}",
                    "Open Scene",
                    AuthoringCheckSeverity.Warning,
                    "Missing generated scene root",
                    $"The open prototype scene is missing expected root '{RequiredPrototypeRoots[i]}'. Use structural rebuild only if this root should be restored.",
                    TheCircussyOneAssetPaths.ScenePath));
            }

            if (missingCount == 0)
            {
                results.Add(new AuthoringCheckResult(
                    "prototype-scene.roots-ok",
                    "Open Scene",
                    AuthoringCheckSeverity.Info,
                    "Generated scene roots found",
                    "The active prototype scene contains the expected generated gameplay roots."));
            }
        }
    }

    private sealed class OldCodenameCheck : IAuthoringCheck
    {
        public void Run(List<AuthoringCheckResult> results)
        {
            int matchCount = 0;
            foreach (string root in TextScanRoots)
            {
                if (!Directory.Exists(root))
                {
                    continue;
                }

                foreach (string file in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
                {
                    if (!ShouldScanTextFile(file))
                    {
                        continue;
                    }

                    string text;
                    try
                    {
                        text = File.ReadAllText(file);
                    }
                    catch (IOException)
                    {
                        continue;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        continue;
                    }

                    if (!ContainsOldCodename(text))
                    {
                        continue;
                    }

                    matchCount++;
                    results.Add(new AuthoringCheckResult(
                        $"codename.old-reference.{SanitizeId(file)}",
                        "Codenames",
                        AuthoringCheckSeverity.Error,
                        "Old codename string found",
                        $"Old codename reference remains in project text: {file}",
                        file));
                }
            }

            if (matchCount == 0)
            {
                results.Add(new AuthoringCheckResult(
                    "codename.ok",
                    "Codenames",
                    AuthoringCheckSeverity.Info,
                    "No old codename strings found",
                    "Text files under Assets, Packages, and ProjectSettings do not contain old codename references."));
            }
        }

        private static bool ShouldScanTextFile(string path)
        {
            if (path.IndexOf("/Library/", StringComparison.Ordinal) >= 0)
            {
                return false;
            }

            string extension = Path.GetExtension(path);
            return TextExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
        }
    }

    private sealed class GeneratedCapturesCheck : IAuthoringCheck
    {
        public void Run(List<AuthoringCheckResult> results)
        {
            if (!Directory.Exists(VisualTestLabBuilder.CaptureFolder))
            {
                return;
            }

            string[] pngs = Directory.GetFiles(VisualTestLabBuilder.CaptureFolder, "*.png", SearchOption.TopDirectoryOnly);
            if (pngs.Length == 0)
            {
                results.Add(new AuthoringCheckResult(
                    "generated-captures.ok",
                    "Captures",
                    AuthoringCheckSeverity.Info,
                    "No generated capture PNGs found",
                    "No generated capture PNGs were found under the Visual Test Lab capture folder."));
                return;
            }

            for (int i = 0; i < pngs.Length; i++)
            {
                results.Add(new AuthoringCheckResult(
                    $"generated-captures.png.{SanitizeId(pngs[i])}",
                    "Captures",
                    AuthoringCheckSeverity.Warning,
                    "Generated capture PNG found",
                    "Generated captures should stay ignored unless intentionally promoted as curated baselines.",
                    pngs[i]));
            }
        }
    }
}
