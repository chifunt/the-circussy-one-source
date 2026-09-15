using NUnit.Framework;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;
using TheCircussyOne.Runtime;
using TheCircussyOne.Stats;

public sealed class GameStateTests
{
    private GameConfig _config;

    [SetUp]
    public void SetUp()
    {
        _config = TheCircussyOneTestObjects.CreateConfig();
    }

    [TearDown]
    public void TearDown()
    {
        TheCircussyOneTestObjects.Destroy(_config);
    }

    [Test]
    public void ResetRestoresRunValues()
    {
        var state = new GameState(_config);
        state.DamagePlayer(25, "Test Hazard");
        state.AddKill();
        state.AddExperience(state.ExperienceTarget + 3);

        state.Reset();

        Assert.That(state.Health, Is.EqualTo(_config.playerMaxHealth));
        Assert.That(state.Kills, Is.Zero);
        Assert.That(state.Level, Is.EqualTo(1));
        Assert.That(state.Experience, Is.Zero);
        Assert.That(state.IsGameOver, Is.False);
        Assert.That(state.LastDamageCause, Is.EqualTo("Unknown"));
    }

    [Test]
    public void DamagePlayerClampsHealthAndRaisesGameOverOnce()
    {
        var state = new GameState(_config);
        int gameOverCount = 0;
        state.GameOver += () => gameOverCount++;

        state.DamagePlayer(_config.playerMaxHealth + 50);
        state.DamagePlayer(1);

        Assert.That(state.Health, Is.Zero);
        Assert.That(state.IsGameOver, Is.True);
        Assert.That(gameOverCount, Is.EqualTo(1));
    }

    [Test]
    public void DamagePlayerStoresLastDamageCause()
    {
        var state = new GameState(_config);

        state.DamagePlayer(7, "  Falling Prop  ");

        Assert.That(state.LastDamageCause, Is.EqualTo("Falling Prop"));
    }

    [Test]
    public void DefaultArmorPreservesCurrentPlayerDamage()
    {
        var state = new GameState(_config);

        state.DamagePlayer(_config.enemyContactDamage);

        Assert.That(state.Health, Is.EqualTo(_config.playerMaxHealth - _config.enemyContactDamage));
    }

    [Test]
    public void ArmorModifierReducesPlayerDamage()
    {
        var stats = new RunStats(_config);
        stats.AddModifier(StatModifier.Flat(StatId.PlayerArmor, 100f, "armor"));
        var state = new GameState(_config, stats);

        state.DamagePlayer(10);

        Assert.That(state.Health, Is.EqualTo(_config.playerMaxHealth - 5));
    }

    [Test]
    public void PlayerDamagedEventReportsPostArmorDamageAndHealthPercent()
    {
        var stats = new RunStats(_config);
        stats.AddModifier(StatModifier.Flat(StatId.PlayerArmor, 100f, "armor"));
        var state = new GameState(_config, stats);
        int reportedDamage = 0;
        int reportedMaxHealth = 0;
        float reportedPercent = 0f;
        state.PlayerDamaged += (damage, maxHealth, percent) =>
        {
            reportedDamage = damage;
            reportedMaxHealth = maxHealth;
            reportedPercent = percent;
        };

        state.DamagePlayer(10);

        Assert.That(reportedDamage, Is.EqualTo(5));
        Assert.That(reportedMaxHealth, Is.EqualTo(_config.playerMaxHealth));
        Assert.That(reportedPercent, Is.EqualTo(0.05f).Within(0.001f));
    }

    [Test]
    public void PlayerDamagedEventIgnoresZeroAndNegativeDamage()
    {
        var state = new GameState(_config);
        int events = 0;
        state.PlayerDamaged += (_, _, _) => events++;

        state.DamagePlayer(0);
        state.DamagePlayer(-4);

        Assert.That(events, Is.Zero);
    }

    [Test]
    public void PlayerHealedEventReportsActualHealAmountAndPercent()
    {
        var state = new GameState(_config);
        int reportedHealing = 0;
        int reportedMaxHealth = 0;
        float reportedPercent = 0f;
        state.PlayerHealed += (healed, maxHealth, percent) =>
        {
            reportedHealing = healed;
            reportedMaxHealth = maxHealth;
            reportedPercent = percent;
        };

        state.DamagePlayer(15);
        state.HealPlayer(50);

        Assert.That(reportedHealing, Is.EqualTo(15));
        Assert.That(reportedMaxHealth, Is.EqualTo(_config.playerMaxHealth));
        Assert.That(reportedPercent, Is.EqualTo(0.15f).Within(0.001f));
    }

    [Test]
    public void PlayerHealedEventIgnoresZeroNegativeAndFullHealthHealing()
    {
        var state = new GameState(_config);
        int events = 0;
        state.PlayerHealed += (_, _, _) => events++;

        state.HealPlayer(10);
        state.HealPlayer(0);
        state.HealPlayer(-4);

        Assert.That(events, Is.Zero);
    }

    [Test]
    public void RefreshDerivedStatsAddsMaxHealthIncreaseToCurrentHealth()
    {
        var stats = new RunStats(_config);
        var state = new GameState(_config, stats);
        state.DamagePlayer(40);

        stats.AddModifier(StatModifier.Flat(StatId.PlayerMaxHealth, 25f, "max_hp"));
        state.RefreshDerivedStats();

        Assert.That(state.MaxHealth, Is.EqualTo(_config.playerMaxHealth + 25));
        Assert.That(state.Health, Is.EqualTo(_config.playerMaxHealth - 40 + 25));
    }

    [Test]
    public void AddExperienceLevelsUpAndCarriesRemainder()
    {
        _config.experienceCurveEarlyBase = 5;
        _config.experienceCurveEarlyIncrement = 0;
        _config.experienceCurveRequirementMultiplier = 1f;
        var state = new GameState(_config);

        state.AddExperience(7);

        Assert.That(state.Level, Is.EqualTo(2));
        Assert.That(state.Experience, Is.EqualTo(2));
        Assert.That(state.ExperienceTarget, Is.EqualTo(5));
    }

    [Test]
    public void ExperienceCurveMatchesStarterTargets()
    {
        var curve = ExperienceCurveSettings.Starter;

        Assert.That(ExperienceRules.ExperienceRequiredForNextLevel(1, curve), Is.EqualTo(21));
        Assert.That(ExperienceRules.ExperienceRequiredForNextLevel(2, curve), Is.EqualTo(35));
        Assert.That(ExperienceRules.ExperienceRequiredForNextLevel(10, curve), Is.EqualTo(147));
        Assert.That(ExperienceRules.ExperienceRequiredForNextLevel(20, curve), Is.EqualTo(391));
        Assert.That(ExperienceRules.ExperienceRequiredForNextLevel(30, curve), Is.EqualTo(691));
        Assert.That(ExperienceRules.ExperienceRequiredForNextLevel(40, curve), Is.EqualTo(1061));
    }

    [Test]
    public void ExperienceCurveRequirementMultiplierScalesTargets()
    {
        var curve = new ExperienceCurveSettings(
            earlyEndLevel: 10,
            midEndLevel: 25,
            earlyBase: 21,
            earlyIncrement: 14,
            midBase: 161,
            midIncrement: 23,
            lateBase: 506,
            lateIncrement: 37,
            requirementMultiplier: 3f);

        Assert.That(ExperienceRules.ExperienceRequiredForNextLevel(1, curve), Is.EqualTo(63));
        Assert.That(ExperienceRules.ExperienceRequiredForNextLevel(2, curve), Is.EqualTo(105));
        Assert.That(ExperienceRules.ExperienceRequiredForNextLevel(10, curve), Is.EqualTo(441));
        Assert.That(ExperienceRules.ExperienceRequiredForNextLevel(20, curve), Is.EqualTo(1173));
        Assert.That(ExperienceRules.ExperienceRequiredForNextLevel(30, curve), Is.EqualTo(2073));
        Assert.That(ExperienceRules.ExperienceRequiredForNextLevel(40, curve), Is.EqualTo(3183));
    }

    [Test]
    public void AddExperienceUsesIncreasingTargetsForMultipleLevelUps()
    {
        var state = new GameState(_config);

        int firstTarget = ExperienceRules.ExperienceRequiredForNextLevel(1, _config.ExperienceCurve);
        int secondTarget = ExperienceRules.ExperienceRequiredForNextLevel(2, _config.ExperienceCurve);
        state.AddExperience(firstTarget + secondTarget + 4);

        Assert.That(state.Level, Is.EqualTo(3));
        Assert.That(state.Experience, Is.EqualTo(4));
        Assert.That(state.ExperienceTarget, Is.EqualTo(147));
    }

}
