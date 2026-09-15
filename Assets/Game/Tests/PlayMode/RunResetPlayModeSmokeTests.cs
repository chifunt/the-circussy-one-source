using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Runtime;
using TheCircussyOne.Stats;

public sealed class RunResetPlayModeSmokeTests
{
    [UnityTest]
    public IEnumerator RetryAfterDeathResetsRunStateAcrossFrames()
    {
        GameConfig config = ScriptableObject.CreateInstance<GameConfig>();
        WeaponDefinition weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
        weapon.ApplyDefaults(
            "smoke_weapon",
            "Smoke Weapon",
            ContentTagSet.With(ContentTag.Projectile),
            1f,
            0.1f,
            1,
            1f,
            1f,
            1f,
            default,
            12f,
            1f,
            1f);
        var stats = new RunStats(config);
        var loadout = new WeaponLoadout(null);
        var inventory = new ItemInventory();
        var currency = new RunCurrencyState();
        var upgradeRunState = new UpgradeRunState();
        var talentRunState = new TalentRunState();
        var gameState = new GameState(config, stats);
        var pauseState = new RunPauseState();
        var phaseState = new RunPhaseState();
        var performerRunState = new PerformerRunState();
        var resetCoordinator = new RunResetCoordinator(
            pauseState,
            phaseState,
            performerRunState,
            stats,
            loadout,
            inventory,
            currency,
            upgradeRunState,
            talentRunState,
            gameState);
        var retryService = new RunRetryService(resetCoordinator);

        loadout.SetStartingWeapons(weapon);
        currency.AddTickets(25);
        gameState.AddKill();
        gameState.AddExperience(25);
        gameState.DamagePlayer(gameState.MaxHealth);
        pauseState.Pause(RunPauseReasons.GameOver);
        phaseState.BeginWorld();
        phaseState.FailRun();

        Assert.That(retryService.TryRetryAfterDeath(), Is.True);

        yield return null;
        yield return null;

        Assert.That(pauseState.IsPaused, Is.False);
        Assert.That(phaseState.CurrentPhase, Is.EqualTo(RunPhase.PerformerSelection));
        Assert.That(gameState.IsGameOver, Is.False);
        Assert.That(gameState.Health, Is.EqualTo(gameState.MaxHealth));
        Assert.That(gameState.Level, Is.EqualTo(1));
        Assert.That(gameState.Kills, Is.Zero);
        Assert.That(currency.Tickets, Is.Zero);
        Assert.That(loadout.WeaponCount, Is.Zero);
        Assert.That(performerRunState.IsSelectionComplete, Is.False);

        Object.Destroy(config);
        Object.Destroy(weapon);
    }
}
