using System.Text.RegularExpressions;
using NUnit.Framework;
using TheCircussyOne.Config;
using TheCircussyOne.Runtime;
using UnityEngine;
using UnityEngine.TestTools;

public sealed class WorldLoadTimingDiagnosticsTests
{
    [TearDown]
    public void TearDown()
    {
        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public void DisabledDiagnosticsDoNotLog()
    {
        WorldLoadTimingDiagnostics.Disabled.LogStage("Test.Disabled", 20f);
    }

    [Test]
    public void EnabledDiagnosticsLogWorldContext()
    {
        RunWorldGenerationConfig config = ScriptableObject.CreateInstance<RunWorldGenerationConfig>();
        config.worldLoadTimingDiagnosticsEnabled = true;
        config.worldLoadTimingLogAllStages = true;
        config.worldLoadTimingSlowThresholdMs = 999f;
        var request = new WorldGenerationRequest(2, 3, 12345);
        WorldLoadTimingDiagnostics timing = WorldLoadTimingDiagnostics.ForWorld(config, 7, request);

        LogAssert.Expect(
            LogType.Log,
            new Regex(@"\[LOAD-TIMING\].*transition=7.*world=2.*act=3.*seed=12345.*stage=Test\.Enabled.*ms=12\.3.*chunks=4"));

        timing.LogStage("Test.Enabled", 12.3, "chunks=4");
        Object.DestroyImmediate(config);
    }

    [Test]
    public void SlowThresholdCanFilterFastStages()
    {
        RunWorldGenerationConfig config = ScriptableObject.CreateInstance<RunWorldGenerationConfig>();
        config.worldLoadTimingDiagnosticsEnabled = true;
        config.worldLoadTimingLogAllStages = false;
        config.worldLoadTimingSlowThresholdMs = 10f;
        var request = new WorldGenerationRequest(1, 1, 99);
        WorldLoadTimingDiagnostics timing = WorldLoadTimingDiagnostics.ForWorld(config, 1, request);

        timing.LogStage("Test.Fast", 4f);
        LogAssert.Expect(LogType.Log, new Regex(@"\[LOAD-TIMING\].*stage=Test\.Slow.*ms=12"));
        timing.LogStage("Test.Slow", 12f);
        Object.DestroyImmediate(config);
    }
}
