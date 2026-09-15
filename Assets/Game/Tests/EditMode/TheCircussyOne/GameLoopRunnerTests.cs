using NUnit.Framework;
using TheCircussyOne.Runtime;

public sealed class GameLoopRunnerTests
{
    [Test]
    public void GameplayTickIsSkippedWhenPausedModalResumesInSamePhase()
    {
        bool skip = GameLoopRunnerRules.ShouldSkipGameplayTick(
            hasPauseState: true,
            pausedAtPhaseStart: true,
            pausedNow: false,
            canTickWhenPaused: false);

        Assert.That(skip, Is.True);
    }

    [Test]
    public void PausedTickablesCanRunWhilePhaseStartedPaused()
    {
        bool skip = GameLoopRunnerRules.ShouldSkipGameplayTick(
            hasPauseState: true,
            pausedAtPhaseStart: true,
            pausedNow: true,
            canTickWhenPaused: true);

        Assert.That(skip, Is.False);
    }

    [Test]
    public void GameplayTickIsSkippedWhenPauseStartsDuringPhase()
    {
        bool skip = GameLoopRunnerRules.ShouldSkipGameplayTick(
            hasPauseState: true,
            pausedAtPhaseStart: false,
            pausedNow: true,
            canTickWhenPaused: false);

        Assert.That(skip, Is.True);
    }
}
