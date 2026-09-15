using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

public sealed class RuntimeTimeSourceAuditTests
{
    private static readonly string[] RewardViewFiles =
    {
        "Game/Scripts/TheCircussyOne/Visuals/ChestView.cs",
        "Game/Scripts/TheCircussyOne/Visuals/TicketDepositView.cs",
        "Game/Scripts/TheCircussyOne/Visuals/HealingPropView.cs"
    };

    private static readonly Regex RawGameplayTimePattern = new(
        @"\bTime\.(deltaTime|time)\b|UnityEngine\.Time\.(deltaTime|time)\b",
        RegexOptions.Compiled);

    [Test]
    public void ConsumedRewardViewsDoNotTickWithRawUnityDeltaTime()
    {
        foreach (string relativePath in RewardViewFiles)
        {
            string text = File.ReadAllText(Path.Combine(Application.dataPath, relativePath));

            Assert.That(text, Does.Not.Contain("Time.deltaTime"), relativePath);
            Assert.That(text, Does.Not.Contain("UnityEngine.Time.deltaTime"), relativePath);
        }
    }

    [Test]
    public void RuntimeGameplayRawUnityTimeIsCentralizedInUnityGameTime()
    {
        string root = Path.Combine(Application.dataPath, "Game/Scripts/TheCircussyOne");
        string[] offenders = Directory
            .EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.EndsWith("Runtime/Core/UnityGameTime.cs"))
            .Where(path => RawGameplayTimePattern.IsMatch(File.ReadAllText(path)))
            .Select(path => path.Substring(Application.dataPath.Length + 1))
            .OrderBy(path => path)
            .ToArray();

        Assert.That(offenders, Is.Empty);
    }
}
