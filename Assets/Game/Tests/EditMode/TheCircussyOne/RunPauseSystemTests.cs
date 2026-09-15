using NUnit.Framework;
using TheCircussyOne.Config;
using TheCircussyOne.Runtime;

public sealed class RunPauseSystemTests
{
    [TearDown]
    public void TearDown()
    {
        TheCircussyOneTestObjects.DestroyAll();
    }

    [Test]
    public void GameOverAddsPauseReason()
    {
        GameConfig config = TheCircussyOneTestObjects.CreateConfig();
        var state = new GameState(config);
        var pauseState = new RunPauseState();
        var system = new GameOverPauseSystem(state, pauseState);

        system.Start();
        state.DamagePlayer(config.playerMaxHealth);

        Assert.That(pauseState.IsPaused, Is.True);
        Assert.That(pauseState.HasReason(RunPauseReasons.GameOver), Is.True);

        system.Dispose();
        TheCircussyOneTestObjects.Destroy(config);
    }

    [Test]
    public void GameStateResetClearsGameOverPauseReason()
    {
        GameConfig config = TheCircussyOneTestObjects.CreateConfig();
        var state = new GameState(config);
        var pauseState = new RunPauseState();
        var system = new GameOverPauseSystem(state, pauseState);

        system.Start();
        state.DamagePlayer(config.playerMaxHealth);
        state.Reset();

        Assert.That(pauseState.HasReason(RunPauseReasons.GameOver), Is.False);
        Assert.That(pauseState.IsPaused, Is.False);

        system.Dispose();
        TheCircussyOneTestObjects.Destroy(config);
    }
}
