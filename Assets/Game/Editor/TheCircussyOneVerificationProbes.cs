using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

public static class VerificationConsoleProbe
{
    public static VerificationConsoleStatus Refresh()
    {
        return RefreshWithType(FindLogEntriesType());
    }

    public static VerificationConsoleStatus RefreshWithTypeForTests(Type logEntriesType)
    {
        return RefreshWithType(logEntriesType);
    }

    public static void Clear()
    {
        Type logEntriesType = FindLogEntriesType();
        MethodInfo clearMethod = logEntriesType?.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        clearMethod?.Invoke(null, null);
    }

    private static VerificationConsoleStatus RefreshWithType(Type logEntriesType)
    {
        try
        {
            if (logEntriesType == null)
            {
                return VerificationConsoleStatus.Unknown("Unity LogEntries type was not found.");
            }

            MethodInfo method = logEntriesType.GetMethod("GetCountsByType", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (method == null)
            {
                return VerificationConsoleStatus.Unknown("Unity LogEntries.GetCountsByType was not found.");
            }

            object[] args = { 0, 0, 0 };
            method.Invoke(null, args);
            return VerificationConsoleStatus.Known(Convert.ToInt32(args[0]), Convert.ToInt32(args[1]), Convert.ToInt32(args[2]));
        }
        catch (Exception exception)
        {
            return VerificationConsoleStatus.Unknown($"Console counts unavailable: {exception.GetBaseException().Message}");
        }
    }

    private static Type FindLogEntriesType()
    {
        return typeof(EditorWindow).Assembly.GetType("UnityEditor.LogEntries")
            ?? Type.GetType("UnityEditor.LogEntries,UnityEditor.CoreModule")
            ?? Type.GetType("UnityEditor.LogEntries,UnityEditor");
    }
}

public static class VerificationGitProbe
{
    public static VerificationGitStatus Refresh()
    {
        GitCommandResult status = RunGit("status --short");
        GitCommandResult diffCheck = RunGit("diff --check");
        VerificationGitStatus gitStatus = FromOutputs(status.output, diffCheck.output, diffCheck.exitCode);
        if (status.exitCode != 0)
        {
            gitStatus.isKnown = false;
            gitStatus.summary = status.error;
        }

        return gitStatus;
    }

    public static VerificationGitStatus FromStatusOutput(string output)
    {
        return FromOutputs(output, string.Empty, 0);
    }

    public static VerificationGitStatus FromOutputs(string statusOutput, string diffCheckOutput, int diffCheckExitCode)
    {
        string raw = statusOutput ?? string.Empty;
        string[] lines = raw.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        int staged = 0;
        int modified = 0;
        int untracked = 0;

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            if (line.StartsWith("??", StringComparison.Ordinal))
            {
                untracked++;
                continue;
            }

            char indexStatus = line.Length > 0 ? line[0] : ' ';
            char workTreeStatus = line.Length > 1 ? line[1] : ' ';
            if (indexStatus != ' ' && indexStatus != '?')
            {
                staged++;
            }

            if (workTreeStatus != ' ' && workTreeStatus != '?')
            {
                modified++;
            }
        }

        int totalDirty = lines.Length;
        string summary = totalDirty == 0
            ? "Git working tree clean."
            : $"{totalDirty} dirty paths ({staged} staged, {modified} modified, {untracked} untracked).";

        return new VerificationGitStatus
        {
            isKnown = true,
            isClean = totalDirty == 0,
            stagedCount = staged,
            modifiedCount = modified,
            untrackedCount = untracked,
            totalDirtyCount = totalDirty,
            diffCheckKnown = true,
            diffCheckPassed = diffCheckExitCode == 0,
            diffCheckMessage = diffCheckExitCode == 0
                ? "git diff --check passed."
                : string.IsNullOrWhiteSpace(diffCheckOutput) ? "git diff --check failed." : diffCheckOutput.Trim(),
            summary = summary,
            rawStatus = raw
        };
    }

    private static GitCommandResult RunGit(string arguments)
    {
        try
        {
            var startInfo = new ProcessStartInfo("git", arguments)
            {
                WorkingDirectory = Directory.GetCurrentDirectory(),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using Process process = Process.Start(startInfo);
            string output = process?.StandardOutput.ReadToEnd() ?? string.Empty;
            string error = process?.StandardError.ReadToEnd() ?? string.Empty;
            process?.WaitForExit(10000);
            return new GitCommandResult(process?.ExitCode ?? 1, output, error);
        }
        catch (Exception exception)
        {
            return new GitCommandResult(1, string.Empty, exception.Message);
        }
    }

    private readonly struct GitCommandResult
    {
        public GitCommandResult(int exitCode, string output, string error)
        {
            this.exitCode = exitCode;
            this.output = output ?? string.Empty;
            this.error = error ?? string.Empty;
        }

        public readonly int exitCode;
        public readonly string output;
        public readonly string error;
    }
}

public static class VerificationCoverageProbe
{
    public const string PackageName = "com.unity.testtools.codecoverage";
    public const string DefaultCoverageFolderName = "CodeCoverage";

    private static readonly string[] ReportRootCandidates =
    {
        Path.Combine(Directory.GetCurrentDirectory(), "Library", "TheCircussyOne", "Coverage"),
        Path.Combine(Directory.GetCurrentDirectory(), DefaultCoverageFolderName)
    };

    public static VerificationCoverageStatus Refresh()
    {
        bool packageInstalled = IsPackageInstalled(out string packageVersion);
        bool coverageEnabled = IsCoverageEnabled();
        if (!packageInstalled)
        {
            return VerificationCoverageStatus.NoReport(false, string.Empty, coverageEnabled, "Code Coverage package is not installed.");
        }

        if (TryFindLatestReportGeneratorSummary(out string summaryPath) &&
            TryParseReportGeneratorSummary(summaryPath, packageInstalled, packageVersion, coverageEnabled, out VerificationCoverageStatus reportGeneratorStatus))
        {
            return reportGeneratorStatus;
        }

        if (TryFindLatestOpenCoverReport(out string openCoverPath) &&
            TryParseOpenCoverReport(openCoverPath, packageInstalled, packageVersion, coverageEnabled, out VerificationCoverageStatus openCoverStatus))
        {
            return openCoverStatus;
        }

        return VerificationCoverageStatus.NoReport(
            packageInstalled,
            packageVersion,
            coverageEnabled,
            coverageEnabled
                ? "No coverage report found. Run EditMode Coverage or generate a report from Window/Analysis/Code Coverage."
                : "Coverage package is installed, but Code Coverage is disabled. Enable it in Window/Analysis/Code Coverage and restart Unity.");
    }

    public static bool IsPackageInstalled(out string version)
    {
        string manifestPath = Path.Combine(Directory.GetCurrentDirectory(), "Packages", "manifest.json");
        if (!File.Exists(manifestPath))
        {
            version = string.Empty;
            return false;
        }

        return TryFindPackageVersion(File.ReadAllText(manifestPath), out version);
    }

    public static bool TryFindPackageVersionForTests(string manifestJson, out string version)
    {
        return TryFindPackageVersion(manifestJson, out version);
    }

    public static bool IsCoverageEnabled()
    {
        try
        {
            Type coverageType = AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType("UnityEngine.TestTools.Coverage"))
                .FirstOrDefault(type => type != null);
            if (coverageType == null)
            {
                return false;
            }

            BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            PropertyInfo property = coverageType.GetProperty("enabled", flags) ?? coverageType.GetProperty("Enabled", flags);
            if (property != null && property.PropertyType == typeof(bool))
            {
                return (bool)property.GetValue(null);
            }

            FieldInfo field = coverageType.GetField("enabled", flags) ?? coverageType.GetField("Enabled", flags);
            return field != null && field.FieldType == typeof(bool) && (bool)field.GetValue(null);
        }
        catch
        {
            return false;
        }
    }

    public static void OpenCoverageWindow()
    {
        if (!EditorApplication.ExecuteMenuItem("Window/Analysis/Code Coverage"))
        {
            Debug.LogWarning("Could not open Window/Analysis/Code Coverage. Is the Code Coverage package installed?");
        }
    }

    public static bool IsCoverageOutputIgnoredPath(string path)
    {
        string normalized = (path ?? string.Empty).Replace('\\', '/');
        return normalized.IndexOf("/Library/TheCircussyOne/Coverage", StringComparison.OrdinalIgnoreCase) >= 0
            || normalized.EndsWith("/CodeCoverage", StringComparison.OrdinalIgnoreCase)
            || normalized.IndexOf("/CodeCoverage/", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    public static bool TryParseReportGeneratorSummaryForTests(string path, out VerificationCoverageStatus status)
    {
        return TryParseReportGeneratorSummary(path, true, "test", false, out status);
    }

    public static bool TryParseOpenCoverReportForTests(string path, out VerificationCoverageStatus status)
    {
        return TryParseOpenCoverReport(path, true, "test", false, out status);
    }

    private static bool TryFindPackageVersion(string manifestJson, out string version)
    {
        version = string.Empty;
        if (string.IsNullOrWhiteSpace(manifestJson))
        {
            return false;
        }

        Match match = Regex.Match(
            manifestJson,
            "\"" + Regex.Escape(PackageName) + "\"\\s*:\\s*\"(?<version>[^\"]+)\"",
            RegexOptions.CultureInvariant);
        if (!match.Success)
        {
            return false;
        }

        version = match.Groups["version"].Value;
        return true;
    }

    private static bool TryFindLatestReportGeneratorSummary(out string path)
    {
        path = FindLatestFile("Summary.xml", file => file.Replace('\\', '/').IndexOf("/Report/", StringComparison.OrdinalIgnoreCase) >= 0);
        return !string.IsNullOrWhiteSpace(path);
    }

    private static bool TryFindLatestOpenCoverReport(out string path)
    {
        path = FindLatestFile("TestCoverageResults_*.xml", file => true);
        return !string.IsNullOrWhiteSpace(path);
    }

    private static string FindLatestFile(string searchPattern, Func<string, bool> filter)
    {
        try
        {
            return ReportRootCandidates
                .Where(Directory.Exists)
                .SelectMany(root => Directory.EnumerateFiles(root, searchPattern, SearchOption.AllDirectories))
                .Where(file => filter?.Invoke(file) ?? true)
                .OrderByDescending(File.GetLastWriteTimeUtc)
                .FirstOrDefault();
        }
        catch
        {
            return string.Empty;
        }
    }

    private static bool TryParseReportGeneratorSummary(
        string path,
        bool packageInstalled,
        string packageVersion,
        bool coverageEnabled,
        out VerificationCoverageStatus status)
    {
        status = null;
        try
        {
            var document = new XmlDocument();
            document.Load(path);

            bool hasFullReport = TryReadReportGeneratorFullSummary(document, out int fullCoveredLines, out int fullCoverableLines, out double fullCoveragePercent);
            if (TryAggregateReportGeneratorAssemblies(document, out int coveredLines, out int coverableLines))
            {
                double percent = coverableLines > 0 ? coveredLines * 100d / coverableLines : 0d;
                status = VerificationCoverageStatus.FromReport(
                    packageInstalled,
                    packageVersion,
                    coverageEnabled,
                    path,
                    "ReportGenerator Summary",
                    percent,
                    coveredLines,
                    coverableLines,
                    "Project Code",
                    hasFullReport ? fullCoveragePercent : percent,
                    hasFullReport ? fullCoveredLines : coveredLines,
                    hasFullReport ? fullCoverableLines : coverableLines);
                return true;
            }

            XmlNode node = document.DocumentElement;
            if (TryReadDoubleAttributeRecursive(node, new[] { "linecoverage", "lineCoverage", "sequenceCoverage", "coverage", "line-rate" }, out double percentFromAttribute))
            {
                int covered = TryReadIntAttributeRecursive(node, new[] { "coveredlines", "coveredLines", "visitedSequencePoints" }, out int coveredValue) ? coveredValue : 0;
                int coverable = TryReadIntAttributeRecursive(node, new[] { "coverablelines", "coverableLines", "numSequencePoints" }, out int coverableValue) ? coverableValue : 0;
                status = VerificationCoverageStatus.FromReport(packageInstalled, packageVersion, coverageEnabled, path, "ReportGenerator Summary", NormalizePercent(percentFromAttribute), covered, coverable);
                return true;
            }
        }
        catch
        {
            return false;
        }

        return false;
    }

    private static bool TryParseOpenCoverReport(
        string path,
        bool packageInstalled,
        string packageVersion,
        bool coverageEnabled,
        out VerificationCoverageStatus status)
    {
        status = null;
        try
        {
            var document = new XmlDocument();
            document.Load(path);
            bool hasProjectModules = TryAggregateOpenCoverModules(document, out int projectCoveredPoints, out int projectCoverablePoints, out double projectPercent);
            XmlNode summary = document.SelectSingleNode("/CoverageSession/Summary") ?? document.GetElementsByTagName("Summary").Cast<XmlNode>().FirstOrDefault();
            if (summary == null)
            {
                return false;
            }

            bool hasVisited = TryReadIntAttribute(summary, new[] { "visitedSequencePoints" }, out int coveredPoints);
            bool hasTotal = TryReadIntAttribute(summary, new[] { "numSequencePoints" }, out int coverablePoints);
            bool hasPercent = TryReadDoubleAttribute(summary, new[] { "sequenceCoverage" }, out double percent);

            if (!hasPercent && hasVisited && hasTotal && coverablePoints > 0)
            {
                percent = coveredPoints * 100d / coverablePoints;
                hasPercent = true;
            }

            if (!hasPercent)
            {
                return false;
            }

            double fullPercent = NormalizePercent(percent);
            status = hasProjectModules
                ? VerificationCoverageStatus.FromReport(
                    packageInstalled,
                    packageVersion,
                    coverageEnabled,
                    path,
                    "OpenCover",
                    projectPercent,
                    projectCoveredPoints,
                    projectCoverablePoints,
                    "Project Code",
                    fullPercent,
                    coveredPoints,
                    coverablePoints)
                : VerificationCoverageStatus.FromReport(packageInstalled, packageVersion, coverageEnabled, path, "OpenCover", fullPercent, coveredPoints, coverablePoints, "Full Report");
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool TryReadReportGeneratorFullSummary(XmlDocument document, out int coveredLines, out int coverableLines, out double coveragePercent)
    {
        coveredLines = 0;
        coverableLines = 0;
        coveragePercent = 0d;
        XmlNode summary = document.GetElementsByTagName("Summary").Cast<XmlNode>().FirstOrDefault();
        if (summary == null)
        {
            return false;
        }

        bool hasCoveredLines = TryReadIntElement(summary, "Coveredlines", out coveredLines) || TryReadIntElement(summary, "CoveredLines", out coveredLines);
        bool hasCoverableLines = TryReadIntElement(summary, "Coverablelines", out coverableLines) || TryReadIntElement(summary, "CoverableLines", out coverableLines);
        bool hasCoveragePercent = TryReadDoubleElement(summary, "Linecoverage", out coveragePercent) || TryReadDoubleElement(summary, "LineCoverage", out coveragePercent);
        if (!hasCoveragePercent && hasCoveredLines && hasCoverableLines && coverableLines > 0)
        {
            coveragePercent = coveredLines * 100d / coverableLines;
            hasCoveragePercent = true;
        }

        coveragePercent = NormalizePercent(coveragePercent);
        return hasCoveragePercent || (hasCoveredLines && hasCoverableLines);
    }

    private static bool TryAggregateReportGeneratorAssemblies(XmlDocument document, out int coveredLines, out int coverableLines)
    {
        coveredLines = 0;
        coverableLines = 0;
        XmlNodeList assemblies = document.GetElementsByTagName("Assembly");
        foreach (XmlNode assembly in assemblies)
        {
            string assemblyName = assembly.Attributes?["name"]?.Value ?? string.Empty;
            if (!IsProjectCoverageAssembly(assemblyName))
            {
                continue;
            }

            if (TryReadIntAttribute(assembly, new[] { "coveredlines", "coveredLines" }, out int covered) &&
                TryReadIntAttribute(assembly, new[] { "coverablelines", "coverableLines" }, out int coverable))
            {
                coveredLines += Math.Max(0, covered);
                coverableLines += Math.Max(0, coverable);
            }
        }

        return coverableLines > 0;
    }

    private static bool TryAggregateOpenCoverModules(XmlDocument document, out int coveredPoints, out int coverablePoints, out double coveragePercent)
    {
        coveredPoints = 0;
        coverablePoints = 0;
        coveragePercent = 0d;

        foreach (XmlNode module in document.GetElementsByTagName("Module"))
        {
            string moduleName = module.SelectSingleNode("ModuleName")?.InnerText ?? string.Empty;
            if (!IsProjectCoverageAssembly(moduleName))
            {
                continue;
            }

            XmlNode summary = module.SelectSingleNode("Summary");
            if (summary == null)
            {
                continue;
            }

            if (TryReadIntAttribute(summary, new[] { "visitedSequencePoints" }, out int covered) &&
                TryReadIntAttribute(summary, new[] { "numSequencePoints" }, out int coverable))
            {
                coveredPoints += Math.Max(0, covered);
                coverablePoints += Math.Max(0, coverable);
            }
        }

        if (coverablePoints <= 0)
        {
            return false;
        }

        coveragePercent = coveredPoints * 100d / coverablePoints;
        return true;
    }

    private static bool IsProjectCoverageAssembly(string assemblyName)
    {
        if (string.IsNullOrWhiteSpace(assemblyName))
        {
            return false;
        }

        bool isTheCircussyOne = string.Equals(assemblyName, "TheCircussyOne", StringComparison.Ordinal)
            || assemblyName.StartsWith("TheCircussyOne.", StringComparison.Ordinal);
        bool isTestAssembly = assemblyName.StartsWith("TheCircussyOne.Tests", StringComparison.Ordinal);
        return isTheCircussyOne && !isTestAssembly;
    }

    private static bool TryReadIntAttributeRecursive(XmlNode node, string[] names, out int value)
    {
        value = 0;
        if (node == null)
        {
            return false;
        }

        if (TryReadIntAttribute(node, names, out value))
        {
            return true;
        }

        foreach (XmlNode child in node.ChildNodes)
        {
            if (TryReadIntAttributeRecursive(child, names, out value))
            {
                return true;
            }
        }

        return false;
    }

    private static bool TryReadDoubleAttributeRecursive(XmlNode node, string[] names, out double value)
    {
        value = 0d;
        if (node == null)
        {
            return false;
        }

        if (TryReadDoubleAttribute(node, names, out value))
        {
            return true;
        }

        foreach (XmlNode child in node.ChildNodes)
        {
            if (TryReadDoubleAttributeRecursive(child, names, out value))
            {
                return true;
            }
        }

        return false;
    }

    private static bool TryReadIntElement(XmlNode node, string elementName, out int value)
    {
        value = 0;
        XmlNode child = node?.SelectSingleNode(elementName);
        return child != null && int.TryParse(child.InnerText, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }

    private static bool TryReadDoubleElement(XmlNode node, string elementName, out double value)
    {
        value = 0d;
        XmlNode child = node?.SelectSingleNode(elementName);
        return child != null && double.TryParse(child.InnerText, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }

    private static bool TryReadIntAttribute(XmlNode node, string[] names, out int value)
    {
        value = 0;
        if (node?.Attributes == null)
        {
            return false;
        }

        foreach (string name in names)
        {
            XmlAttribute attribute = node.Attributes[name];
            if (attribute != null && int.TryParse(attribute.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
            {
                return true;
            }
        }

        return false;
    }

    private static bool TryReadDoubleAttribute(XmlNode node, string[] names, out double value)
    {
        value = 0d;
        if (node?.Attributes == null)
        {
            return false;
        }

        foreach (string name in names)
        {
            XmlAttribute attribute = node.Attributes[name];
            if (attribute != null && double.TryParse(attribute.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
            {
                return true;
            }
        }

        return false;
    }

    private static double NormalizePercent(double value)
    {
        return value <= 1d ? value * 100d : value;
    }
}
