using System.Linq;
using NUnit.Framework;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using TheCircussyOne.Runtime;
using TheCircussyOne.Stats;
using TheCircussyOne.Visuals;
using UnityEngine;

public sealed class RunPhaseStateTests
{
    [TearDown]
    public void TearDown()
    {
        SceneInteractableRegistry.Clear();
        TheCircussyOneTestObjects.DestroyAll();
    }

    [Test]
    public void PhaseRulesGateCoreRuntimeWork()
    {
        Assert.That(RunPhaseRules.ShouldSpawnEnemies(RunPhase.PerformerSelection), Is.False);
        Assert.That(RunPhaseRules.ShouldTickCombat(RunPhase.WorldActive), Is.True);
        Assert.That(RunPhaseRules.ShouldSpawnEnemies(RunPhase.BossActive), Is.True);
        Assert.That(RunPhaseRules.ShouldSpawnEnemies(RunPhase.BossDefeated), Is.True);
        Assert.That(RunPhaseRules.ShouldSpawnEnemies(RunPhase.Encore), Is.True);
        Assert.That(RunPhaseRules.ShouldAllowInteractions(RunPhase.Encore), Is.True);
        Assert.That(RunPhaseRules.ShouldTickCombat(RunPhase.Intermission), Is.False);
        Assert.That(RunPhaseRules.ShouldSpawnEnemies(RunPhase.Intermission), Is.False);
        Assert.That(RunPhaseRules.ShouldAllowInteractions(RunPhase.Intermission), Is.False);
        Assert.That(RunPhaseRules.ShouldAllowInteractions(RunPhase.Finished), Is.False);
        Assert.That(RunPhaseRules.IsTerminal(RunPhase.Failed), Is.True);
        Assert.That(RunPhaseRules.IsTerminal(RunPhase.Intermission), Is.False);
    }

    [Test]
    public void RunPhaseStateExposesOnlyParameterlessPublicConstructorForVContainer()
    {
        var constructors = typeof(RunPhaseState).GetConstructors();

        Assert.That(constructors, Has.Length.EqualTo(1));
        Assert.That(constructors.Single().GetParameters(), Is.Empty);
    }

    [Test]
    public void PhaseStateTransitionsThroughBossAndFinishSkeleton()
    {
        var phase = new RunPhaseState();
        int changed = 0;
        phase.PhaseChanged += (_, _) => changed++;

        Assert.That(phase.TrySetPhase(RunPhase.BossWarning), Is.False);
        Assert.That(phase.CurrentPhase, Is.EqualTo(RunPhase.PerformerSelection));

        phase.BeginWorld();
        Assert.That(phase.CurrentPhase, Is.EqualTo(RunPhase.WorldActive));
        phase.Tick(2.5f);
        Assert.That(phase.PhaseElapsedSeconds, Is.EqualTo(2.5f).Within(0.001f));

        phase.StartBossWarning();
        Assert.That(phase.CurrentPhase, Is.EqualTo(RunPhase.BossWarning));
        Assert.That(phase.PhaseElapsedSeconds, Is.Zero);

        phase.ActivateBoss();
        phase.MarkBossDefeated();
        Assert.That(phase.CurrentPhase, Is.EqualTo(RunPhase.BossDefeated));
        phase.BeginEncore();
        Assert.That(phase.CurrentPhase, Is.EqualTo(RunPhase.Encore));

        phase.EnterIntermission();
        Assert.That(phase.CurrentPhase, Is.EqualTo(RunPhase.Intermission));

        phase.BeginNextWorld();
        Assert.That(phase.CurrentPhase, Is.EqualTo(RunPhase.NextWorldTransition));
        Assert.That(phase.WorldIndex, Is.EqualTo(2));

        phase.BeginWorld();
        Assert.That(phase.CurrentPhase, Is.EqualTo(RunPhase.WorldActive));
        phase.StartBossWarning();
        phase.ActivateBoss();
        phase.MarkBossDefeated();
        phase.BeginEncore();
        phase.FinishRun();
        Assert.That(phase.CurrentPhase, Is.EqualTo(RunPhase.Finished));
        phase.BeginNextWorld();
        Assert.That(phase.CurrentPhase, Is.EqualTo(RunPhase.Finished));
        Assert.That(phase.WorldIndex, Is.EqualTo(2));
        Assert.That(changed, Is.EqualTo(13));
    }

    [Test]
    public void RunScheduleDefaultsUseSevenNineElevenActs()
    {
        RunScheduleConfig config = RunScheduleConfig.CreateRuntimeDefault();

        Assert.That(config.Acts, Has.Count.EqualTo(3));
        Assert.That(config.Acts[0].DisplayName, Is.EqualTo("Opening Act"));
        Assert.That(config.Acts[0].DurationSeconds, Is.EqualTo(7f * 60f).Within(0.001f));
        Assert.That(config.Acts[0].ShowtimeSeconds, Is.EqualTo(new[] { 120f, 300f }));
        Assert.That(config.Acts[1].DisplayName, Is.EqualTo("Center Ring"));
        Assert.That(config.Acts[1].DurationSeconds, Is.EqualTo(9f * 60f).Within(0.001f));
        Assert.That(config.Acts[1].ShowtimeSeconds, Is.EqualTo(new[] { 120f, 300f, 450f }));
        Assert.That(config.Acts[2].DisplayName, Is.EqualTo("Grand Finale"));
        Assert.That(config.Acts[2].DurationSeconds, Is.EqualTo(11f * 60f).Within(0.001f));
        Assert.That(config.Acts[2].ShowtimeSeconds, Is.EqualTo(new[] { 120f, 330f, 510f }));
        Assert.That(config.actFinaleWarningSeconds, Is.EqualTo(2f).Within(0.001f));
        Assert.That(config.actFinaleProxyAutoDefeat, Is.True);
        Assert.That(config.actFinaleProxyBossSeconds, Is.EqualTo(6f).Within(0.001f));
        Assert.That(config.headlinerEncoreDeadlineSeconds, Is.EqualTo(90f).Within(0.001f));
        Assert.That(config.intermissionSeconds, Is.EqualTo(1.5f).Within(0.001f));
        Assert.That(config.showtimeAnnouncementSubtitles, Has.Count.GreaterThanOrEqualTo(3));
        Assert.That(config.actFinaleAnnouncementSubtitles, Has.Count.GreaterThanOrEqualTo(3));
        Assert.That(config.intermissionAnnouncementSubtitleFormats, Has.Count.GreaterThanOrEqualTo(3));
        Assert.That(config.curtainCallAnnouncementSubtitles, Has.Count.GreaterThanOrEqualTo(3));
        Assert.That(config.encorePressureIntervalSeconds, Is.EqualTo(30f).Within(0.001f));
        Assert.That(config.encoreSpawnIntervalMultiplierPerStep, Is.EqualTo(0.9f).Within(0.001f));
        Assert.That(config.encoreMaxEnemiesMultiplierPerStep, Is.EqualTo(1.1f).Within(0.001f));
        Assert.That(config.stageDoorTravelSeconds, Has.Count.GreaterThanOrEqualTo(3));
        Assert.That(config.stageDoorTravelSeconds[0].MinTravelSeconds, Is.EqualTo(15f).Within(0.001f));
        Assert.That(config.stageDoorTravelSeconds[0].MaxTravelSeconds, Is.EqualTo(25f).Within(0.001f));
        Assert.That(config.stageDoorTravelSeconds[1].MinTravelSeconds, Is.EqualTo(25f).Within(0.001f));
        Assert.That(config.stageDoorTravelSeconds[1].MaxTravelSeconds, Is.EqualTo(40f).Within(0.001f));
        Assert.That(config.stageDoorTravelSeconds[2].MinTravelSeconds, Is.EqualTo(35f).Within(0.001f));
        Assert.That(config.stageDoorTravelSeconds[2].MaxTravelSeconds, Is.EqualTo(55f).Within(0.001f));

        Object.DestroyImmediate(config);
    }

    [Test]
    public void RunAnnouncementSubtitleSelectionUsesAuthoredVariants()
    {
        string[] variants = { " First ", string.Empty, "Second", "Third" };
        string pickedA = RunActScheduleRules.PickSubtitleVariant(variants, "Fallback", 12345);
        string pickedB = RunActScheduleRules.PickSubtitleVariant(variants, "Fallback", 12345);

        Assert.That(pickedA, Is.EqualTo(pickedB));
        Assert.That(new[] { "First", "Second", "Third" }, Does.Contain(pickedA));
        Assert.That(RunActScheduleRules.PickSubtitleVariant(System.Array.Empty<string>(), "Fallback", 99), Is.EqualTo("Fallback"));
    }

    [Test]
    public void RunScheduleTimerDisplaysCountdownOnly()
    {
        RunScheduleConfig config = RunScheduleConfig.CreateRuntimeDefault();
        var schedule = new RunActScheduleState(config);

        schedule.BeginAct(1);

        Assert.That(RunActScheduleRules.FormatActTimer(schedule), Is.EqualTo("07:00"));
        Assert.That(RunActScheduleRules.FormatActTimer(schedule), Does.Not.Contain("ACT"));

        Object.DestroyImmediate(config);
    }

    [Test]
    public void RunScheduleEncoreUsesGraceBeforeCountingPressure()
    {
        RunScheduleConfig config = RunScheduleConfig.CreateRuntimeDefault();
        config.encoreGraceSeconds = 10f;
        config.encorePressureIntervalSeconds = 30f;
        config.encoreSpawnIntervalMultiplierPerStep = 0.9f;
        config.encoreMaxEnemiesMultiplierPerStep = 1.1f;
        var schedule = new RunActScheduleState(config);
        int pressureStarts = 0;
        schedule.EncorePressureStarted += () => pressureStarts++;

        schedule.BeginAct(1);
        schedule.BeginEncore();

        Assert.That(schedule.IsEncoreActive, Is.True);
        Assert.That(schedule.IsEncoreGraceActive, Is.True);
        Assert.That(pressureStarts, Is.Zero);
        Assert.That(RunActScheduleRules.FormatEncoreTimer(schedule), Is.EqualTo("ENCORE IN 10"));

        schedule.TickEncore(9.9f);

        Assert.That(schedule.IsEncoreActive, Is.True);
        Assert.That(schedule.IsEncoreGraceActive, Is.True);
        Assert.That(pressureStarts, Is.Zero);
        Assert.That(schedule.EncorePressureStep, Is.Zero);
        Assert.That(RunActScheduleRules.FormatEncoreTimer(schedule), Is.EqualTo("ENCORE IN 1"));
        Assert.That(schedule.EncoreSpawnIntervalMultiplier, Is.EqualTo(1f).Within(0.001f));
        Assert.That(schedule.EncoreMaxEnemiesMultiplier, Is.EqualTo(1f).Within(0.001f));

        schedule.TickEncore(0.2f);

        Assert.That(schedule.IsEncoreGraceActive, Is.False);
        Assert.That(pressureStarts, Is.EqualTo(1));
        Assert.That(schedule.EncorePressureStep, Is.Zero);
        Assert.That(RunActScheduleRules.FormatEncoreTimer(schedule), Is.EqualTo("00:00"));

        schedule.TickEncore(29.9f);

        Assert.That(schedule.EncorePressureStep, Is.EqualTo(1));
        Assert.That(RunActScheduleRules.FormatEncoreTimer(schedule), Is.EqualTo("00:30"));
        Assert.That(schedule.EncoreSpawnIntervalMultiplier, Is.EqualTo(0.9f).Within(0.001f));
        Assert.That(schedule.EncoreMaxEnemiesMultiplier, Is.EqualTo(1.1f).Within(0.001f));

        schedule.TickEncore(30f);

        Assert.That(schedule.EncorePressureStep, Is.EqualTo(2));
        Assert.That(schedule.EncoreSpawnIntervalMultiplier, Is.EqualTo(0.81f).Within(0.001f));
        Assert.That(schedule.EncoreMaxEnemiesMultiplier, Is.EqualTo(1.21f).Within(0.001f));
        Assert.That(pressureStarts, Is.EqualTo(1));

        schedule.EndEncore();

        Assert.That(schedule.IsEncoreActive, Is.False);
        Assert.That(schedule.EncoreElapsedSeconds, Is.Zero);

        Object.DestroyImmediate(config);
    }

    [Test]
    public void RunScheduleCanForceEncorePressureStart()
    {
        RunScheduleConfig config = RunScheduleConfig.CreateRuntimeDefault();
        config.encoreGraceSeconds = 10f;
        var schedule = new RunActScheduleState(config);
        int pressureStarts = 0;
        schedule.EncorePressureStarted += () => pressureStarts++;

        schedule.BeginAct(1);
        schedule.BeginEncore();

        Assert.That(schedule.ForceEndEncoreGrace(), Is.True);
        Assert.That(schedule.IsEncorePressureActive, Is.True);
        Assert.That(pressureStarts, Is.EqualTo(1));
        Assert.That(schedule.ForceEndEncoreGrace(), Is.False);
        Assert.That(pressureStarts, Is.EqualTo(1));

        Object.DestroyImmediate(config);
    }

    [Test]
    public void RunScheduleStateFiresShowtimesOnceAndMarksFinaleDue()
    {
        RunScheduleConfig config = RunScheduleConfig.CreateRuntimeDefault();
        var schedule = new RunActScheduleState(config);
        int showtimeStarts = 0;
        int showtimeEnds = 0;
        int finales = 0;
        schedule.ShowtimeStarted += _ => showtimeStarts++;
        schedule.ShowtimeEnded += _ => showtimeEnds++;
        schedule.ActFinaleBecameDue += _ => finales++;

        schedule.BeginAct(1);
        schedule.Tick(119f);
        Assert.That(showtimeStarts, Is.Zero);
        Assert.That(schedule.ActRemainingSeconds, Is.EqualTo(301f).Within(0.001f));

        schedule.Tick(1f);
        Assert.That(showtimeStarts, Is.EqualTo(1));
        Assert.That(schedule.IsShowtimeActive, Is.True);
        Assert.That(schedule.ShowtimeRemainingSeconds, Is.EqualTo(config.showtimePressureSeconds - 1f).Within(0.001f));

        schedule.Tick(config.showtimePressureSeconds - 1f);
        Assert.That(showtimeEnds, Is.EqualTo(1));
        Assert.That(schedule.IsShowtimeActive, Is.False);

        schedule.Tick(160f);
        Assert.That(showtimeStarts, Is.EqualTo(1));
        schedule.Tick(1f);
        Assert.That(showtimeStarts, Is.EqualTo(2));

        schedule.Tick(119f);
        Assert.That(finales, Is.Zero);
        schedule.Tick(1f);
        Assert.That(schedule.IsActFinaleDue, Is.True);
        Assert.That(finales, Is.EqualTo(1));

        schedule.Tick(10f);
        Assert.That(showtimeStarts, Is.EqualTo(2));
        Assert.That(finales, Is.EqualTo(1));

        Object.DestroyImmediate(config);
    }

    [Test]
    public void RunScheduleStateCanForceNextShowtime()
    {
        RunScheduleConfig config = RunScheduleConfig.CreateRuntimeDefault();
        var schedule = new RunActScheduleState(config);
        int starts = 0;
        RunShowtimeEvent started = default;
        schedule.ShowtimeStarted += showtime =>
        {
            starts++;
            started = showtime;
        };

        schedule.BeginAct(1);

        Assert.That(schedule.ForceNextShowtime(), Is.True);
        Assert.That(starts, Is.EqualTo(1));
        Assert.That(started.ShowtimeIndex, Is.EqualTo(0));
        Assert.That(started.ScheduledSeconds, Is.EqualTo(120f).Within(0.001f));
        Assert.That(schedule.IsShowtimeActive, Is.True);
        Assert.That(schedule.TryGetNextShowtimeRemainingSeconds(out float nextShowtime), Is.True);
        Assert.That(nextShowtime, Is.EqualTo(300f).Within(0.001f));
        Assert.That(schedule.ForceNextShowtime(), Is.False);

        schedule.Tick(config.showtimePressureSeconds);
        Assert.That(schedule.ForceNextShowtime(), Is.True);
        Assert.That(starts, Is.EqualTo(2));

        schedule.Tick(config.showtimePressureSeconds);
        Assert.That(schedule.ForceNextShowtime(), Is.False);

        Object.DestroyImmediate(config);
    }

    [Test]
    public void RunScheduleStateCanForceActFinaleOnce()
    {
        RunScheduleConfig config = RunScheduleConfig.CreateRuntimeDefault();
        var schedule = new RunActScheduleState(config);
        int finales = 0;
        RunActFinaleEvent finale = default;
        schedule.ActFinaleBecameDue += actFinale =>
        {
            finales++;
            finale = actFinale;
        };

        schedule.BeginAct(1);

        Assert.That(schedule.ForceActFinale(), Is.True);
        Assert.That(schedule.IsActFinaleDue, Is.True);
        Assert.That(schedule.ActRemainingSeconds, Is.Zero);
        Assert.That(finales, Is.EqualTo(1));
        Assert.That(finale.ActNumber, Is.EqualTo(1));
        Assert.That(finale.ScheduledSeconds, Is.EqualTo(7f * 60f).Within(0.001f));
        Assert.That(schedule.ForceActFinale(), Is.False);

        Object.DestroyImmediate(config);
    }

    [Test]
    public void RunScheduleStateAnnouncesActStart()
    {
        RunScheduleConfig config = RunScheduleConfig.CreateRuntimeDefault();
        var schedule = new RunActScheduleState(config);
        RunActStartedEvent started = default;
        int startedCount = 0;
        schedule.ActStarted += act =>
        {
            started = act;
            startedCount++;
        };

        schedule.BeginAct(2);

        Assert.That(startedCount, Is.EqualTo(1));
        Assert.That(started.ActNumber, Is.EqualTo(2));
        Assert.That(started.ActName, Is.EqualTo("Center Ring"));
        Assert.That(started.DurationSeconds, Is.EqualTo(9f * 60f).Within(0.001f));
        Assert.That(schedule.HasNextAct, Is.True);

        Object.DestroyImmediate(config);
    }

    [Test]
    public void RunScheduleStateResetsForRestart()
    {
        RunScheduleConfig config = RunScheduleConfig.CreateRuntimeDefault();
        var schedule = new RunActScheduleState(config);
        schedule.BeginAct(2);
        schedule.Tick(121f);

        schedule.Reset();

        Assert.That(schedule.HasActiveAct, Is.False);
        Assert.That(schedule.ActNumber, Is.Zero);
        Assert.That(schedule.IsShowtimeActive, Is.False);
        Assert.That(schedule.IsActFinaleDue, Is.False);
        Assert.That(schedule.ActElapsedSeconds, Is.Zero);

        Object.DestroyImmediate(config);
    }

    [Test]
    public void RunActProgressionStartsAndCompletesProxyBoss()
    {
        RunScheduleConfig config = RunScheduleConfig.CreateRuntimeDefault();
        config.actFinaleWarningSeconds = 0.5f;
        config.actFinaleProxyBossSeconds = 0.75f;
        config.headlinerEncoreDeadlineSeconds = 2f;
        var schedule = new RunActScheduleState(config);
        var phase = CreateWorldActivePhase();
        var defeatState = new HeadlinerDefeatState();
        GameConfig gameConfig = TheCircussyOneTestObjects.CreateConfig();
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(gameConfig);
        player.Teleport(new Vector3(7f, 0f, -4f));
        var progression = new RunActProgressionSystem(
            schedule,
            phase,
            config,
            headlinerDefeatState: defeatState,
            player: player);
        progression.Start();

        schedule.BeginAct(1);
        schedule.Tick(config.Acts[0].DurationSeconds);

        Assert.That(phase.CurrentPhase, Is.EqualTo(RunPhase.BossWarning));

        phase.Tick(0.5f);
        progression.Tick(0f);
        Assert.That(phase.CurrentPhase, Is.EqualTo(RunPhase.BossActive));

        phase.Tick(0.75f);
        progression.Tick(0.75f);
        Assert.That(phase.CurrentPhase, Is.EqualTo(RunPhase.BossDefeated));
        Assert.That(defeatState.TryGetDeathPosition(out Vector3 proxyDeathPosition), Is.True);
        Assert.That(Vector3.Distance(proxyDeathPosition, player.Position), Is.LessThan(0.001f));
        Assert.That(schedule.IsEncoreActive, Is.False);

        phase.Tick(1.25f);
        progression.Tick(1.25f);
        Assert.That(phase.CurrentPhase, Is.EqualTo(RunPhase.Encore));
        Assert.That(schedule.IsEncoreActive, Is.True);

        progression.Dispose();
        TheCircussyOneTestObjects.Destroy(gameConfig);
        Object.DestroyImmediate(config);
    }

    [Test]
    public void RunActProgressionForcesEncoreAtHeadlinerDeadline()
    {
        RunScheduleConfig config = RunScheduleConfig.CreateRuntimeDefault();
        config.actFinaleProxyAutoDefeat = false;
        config.headlinerEncoreDeadlineSeconds = 1.5f;
        var schedule = new RunActScheduleState(config);
        var phase = CreateWorldActivePhase();
        var progression = new RunActProgressionSystem(schedule, phase, config);
        progression.Start();

        schedule.BeginAct(1);
        phase.StartBossWarning();
        phase.ActivateBoss();

        phase.Tick(1f);
        progression.Tick(1f);
        Assert.That(phase.CurrentPhase, Is.EqualTo(RunPhase.BossActive));

        phase.Tick(0.5f);
        progression.Tick(0.5f);

        Assert.That(phase.CurrentPhase, Is.EqualTo(RunPhase.Encore));
        Assert.That(schedule.IsEncoreActive, Is.True);

        progression.Dispose();
        Object.DestroyImmediate(config);
    }

    [Test]
    public void RunActProgressionKeepsBossDefeatedWaitingUntilHeadlinerDeadline()
    {
        RunScheduleConfig config = RunScheduleConfig.CreateRuntimeDefault();
        config.actFinaleProxyAutoDefeat = false;
        config.headlinerEncoreDeadlineSeconds = 2f;
        var schedule = new RunActScheduleState(config);
        var phase = CreateWorldActivePhase();
        var progression = new RunActProgressionSystem(schedule, phase, config);
        progression.Start();

        schedule.BeginAct(1);
        phase.StartBossWarning();
        phase.ActivateBoss();
        progression.Tick(0.75f);
        phase.MarkBossDefeated();

        Assert.That(phase.CurrentPhase, Is.EqualTo(RunPhase.BossDefeated));
        Assert.That(schedule.IsEncoreActive, Is.False);

        progression.Tick(1.24f);
        Assert.That(phase.CurrentPhase, Is.EqualTo(RunPhase.BossDefeated));

        progression.Tick(0.01f);
        Assert.That(phase.CurrentPhase, Is.EqualTo(RunPhase.Encore));
        Assert.That(schedule.IsEncoreActive, Is.True);

        progression.Dispose();
        Object.DestroyImmediate(config);
    }

    [Test]
    public void RunActProgressionMovesToNextActAfterStageDoorOnNonFinalAct()
    {
        RunScheduleConfig config = RunScheduleConfig.CreateRuntimeDefault();
        config.intermissionSeconds = 0.5f;
        var schedule = new RunActScheduleState(config);
        var phase = CreateWorldActivePhase();
        var progression = new RunActProgressionSystem(schedule, phase, config);
        progression.Start();

        schedule.BeginAct(1);
        phase.StartBossWarning();
        phase.ActivateBoss();
        phase.MarkBossDefeated();
        phase.BeginEncore();
        phase.EnterIntermission();

        Assert.That(phase.CurrentPhase, Is.EqualTo(RunPhase.Intermission));

        progression.Tick(0.5f);

        Assert.That(phase.CurrentPhase, Is.EqualTo(RunPhase.WorldActive));
        Assert.That(phase.WorldIndex, Is.EqualTo(2));

        progression.Dispose();
        Object.DestroyImmediate(config);
    }

    [Test]
    public void RunActProgressionAppliesGeneratedPlayerStartAfterIntermission()
    {
        GameConfig gameConfig = TheCircussyOneTestObjects.CreateConfig();
        RunScheduleConfig config = RunScheduleConfig.CreateRuntimeDefault();
        config.intermissionSeconds = 0.5f;
        var schedule = new RunActScheduleState(config);
        var phase = CreateWorldActivePhase();
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(gameConfig);
        Vector3 nextActStart = new(13f, 4.5f, -9f);
        var lifecycle = new RunWorldLifecycleSystem(
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            new WorldPropPlacementService(),
            new WorldRewardRuntimeRegistry(),
            new PlayerStartRunWorldGenerator(nextActStart),
            null);
        var progression = new RunActProgressionSystem(
            schedule,
            phase,
            config,
            worldLifecycle: lifecycle,
            playerStartPlacement: new RunPlayerStartPlacementSystem(player));
        progression.Start();

        schedule.BeginAct(1);
        phase.StartBossWarning();
        phase.ActivateBoss();
        phase.MarkBossDefeated();
        phase.BeginEncore();
        phase.EnterIntermission();

        progression.Tick(0.5f);

        Assert.That(phase.CurrentPhase, Is.EqualTo(RunPhase.WorldActive));
        Assert.That(phase.WorldIndex, Is.EqualTo(2));
        Assert.That(Vector3.Distance(player.Position, nextActStart), Is.LessThan(0.001f));

        progression.Dispose();
        TheCircussyOneTestObjects.Destroy(config);
        TheCircussyOneTestObjects.Destroy(gameConfig);
    }

    [Test]
    public void RunActProgressionRoutesIntermissionThroughTransition()
    {
        RunScheduleConfig config = RunScheduleConfig.CreateRuntimeDefault();
        config.intermissionSeconds = 0.5f;
        var schedule = new RunActScheduleState(config);
        var phase = CreateWorldActivePhase();
        var transition = new FakeRunTransitionService();
        var progression = new RunActProgressionSystem(
            schedule,
            phase,
            config,
            transitionService: transition);
        progression.Start();

        schedule.BeginAct(1);
        phase.StartBossWarning();
        phase.ActivateBoss();
        phase.MarkBossDefeated();
        phase.BeginEncore();
        phase.EnterIntermission();

        Assert.That(transition.PlayAsyncCount, Is.EqualTo(1));
        Assert.That(transition.LastTitle, Is.EqualTo("INTERMISSION"));
        Assert.That(transition.LastMinimumVisibleSeconds, Is.EqualTo(0.5f).Within(0.001f));
        Assert.That(phase.CurrentPhase, Is.EqualTo(RunPhase.WorldActive));
        Assert.That(phase.WorldIndex, Is.EqualTo(2));

        progression.Dispose();
        Object.DestroyImmediate(config);
    }

    [Test]
    public void RunActProgressionLeavesFinalActAtCurtainCall()
    {
        RunScheduleConfig config = RunScheduleConfig.CreateRuntimeDefault();
        var schedule = new RunActScheduleState(config);
        var phase = CreateWorldActivePhase();
        AdvanceToNextWorld(phase);
        AdvanceToNextWorld(phase);
        var progression = new RunActProgressionSystem(schedule, phase, config);
        progression.Start();

        schedule.BeginAct(3);
        phase.StartBossWarning();
        phase.ActivateBoss();
        phase.MarkBossDefeated();
        phase.BeginEncore();
        phase.FinishRun();

        progression.Tick(5f);

        Assert.That(phase.CurrentPhase, Is.EqualTo(RunPhase.Finished));
        Assert.That(phase.WorldIndex, Is.EqualTo(3));

        progression.Dispose();
        Object.DestroyImmediate(config);
    }

    [Test]
    public void PerformerCompositionStartsWorldPhase()
    {
        GameConfig config = TheCircussyOneTestObjects.CreateConfig();
        var stats = new RunStats(config);
        WeaponDefinition weapon = TheCircussyOneTestObjects.CreateWeaponDefinition(config, "juggling_ball", "Juggling Ball");
        var loadout = new WeaponLoadout(TheCircussyOneTestObjects.CreateWeaponCatalog(weapon));
        PerformerDefinition performer = TheCircussyOneTestObjects.CreatePerformerDefinition("bibi", "Bibi", weapon);
        var phase = new RunPhaseState();
        var composer = new PerformerRunComposer(
            new PerformerRunState(),
            stats,
            loadout,
            new GameState(config, stats),
            runPhaseState: phase);

        Assert.That(composer.ComposeAndStart(performer), Is.True);

        Assert.That(phase.CurrentPhase, Is.EqualTo(RunPhase.WorldActive));
    }

    [Test]
    public void PerformerCompositionGeneratesFirstWorld()
    {
        GameConfig config = TheCircussyOneTestObjects.CreateConfig();
        var stats = new RunStats(config);
        WeaponDefinition weapon = TheCircussyOneTestObjects.CreateWeaponDefinition(config, "juggling_ball", "Juggling Ball");
        var loadout = new WeaponLoadout(TheCircussyOneTestObjects.CreateWeaponCatalog(weapon));
        PerformerDefinition performer = TheCircussyOneTestObjects.CreatePerformerDefinition("bibi", "Bibi", weapon);
        var phase = new RunPhaseState();
        var runSeedState = new RunSeedState();
        var generator = new CapturingRunWorldGenerator();
        var generationState = new RunWorldGenerationState();
        var lifecycle = new RunWorldLifecycleSystem(
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            new WorldPropPlacementService(),
            new WorldRewardRuntimeRegistry(),
            generator,
            null,
            generationState);
        var composer = new PerformerRunComposer(
            new PerformerRunState(),
            stats,
            loadout,
            new GameState(config, stats),
            runSeedState: runSeedState,
            runPhaseState: phase,
            worldLifecycle: lifecycle);

        Assert.That(composer.ComposeAndStart(performer), Is.True);

        Assert.That(generator.Request.WorldIndex, Is.EqualTo(1));
        Assert.That(generator.Request.ActNumber, Is.EqualTo(1));
        Assert.That(generator.Request.Seed, Is.Not.EqualTo(0));
        Assert.That(phase.CurrentPhase, Is.EqualTo(RunPhase.WorldActive));

        Object.DestroyImmediate(weapon);
        Object.DestroyImmediate(performer);
        TheCircussyOneTestObjects.Destroy(config);
    }

    [Test]
    public void GameOverMarksRunFailedAndKeepsPauseReason()
    {
        GameConfig config = TheCircussyOneTestObjects.CreateConfig();
        var state = new GameState(config);
        var pauseState = new RunPauseState();
        var phase = CreateWorldActivePhase();
        var system = new GameOverPauseSystem(state, pauseState, phase);

        system.Start();
        state.DamagePlayer(config.playerMaxHealth);

        Assert.That(phase.CurrentPhase, Is.EqualTo(RunPhase.Failed));
        Assert.That(pauseState.HasReason(RunPauseReasons.GameOver), Is.True);

        system.Dispose();
    }

    [Test]
    public void DebugCommandsStepBossAndFinishPhases()
    {
        var phase = CreateWorldActivePhase();
        RunScheduleConfig config = RunScheduleConfig.CreateRuntimeDefault();
        var schedule = new RunActScheduleState(config);
        schedule.BeginAct(1);
        var service = new RunDebugCommandService(
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            phase,
            schedule);

        Assert.That(service.TriggerNextShowtime(), Is.True);
        Assert.That(schedule.IsShowtimeActive, Is.True);
        Assert.That(service.JumpToActFinale(), Is.True);
        Assert.That(schedule.IsActFinaleDue, Is.True);
        Assert.That(phase.CurrentPhase, Is.EqualTo(RunPhase.BossWarning));
        Assert.That(service.ActivateProxyHeadliner(), Is.True);
        Assert.That(phase.CurrentPhase, Is.EqualTo(RunPhase.BossActive));
        Assert.That(service.DefeatProxyHeadliner(), Is.True);
        Assert.That(phase.CurrentPhase, Is.EqualTo(RunPhase.BossDefeated));
        phase.BeginEncore();
        Assert.That(phase.CurrentPhase, Is.EqualTo(RunPhase.Encore));
        Assert.That(service.CompleteStageDoor(), Is.True);
        Assert.That(phase.CurrentPhase, Is.EqualTo(RunPhase.Intermission));
        Assert.That(service.EnterNextAct(), Is.True);
        Assert.That(phase.CurrentPhase, Is.EqualTo(RunPhase.WorldActive));
        Assert.That(phase.WorldIndex, Is.EqualTo(2));
        Assert.That(schedule.ActNumber, Is.EqualTo(2));

        phase.StartBossWarning();
        Assert.That(service.StartBossWarning(), Is.True);
        Assert.That(phase.CurrentPhase, Is.EqualTo(RunPhase.BossWarning));
        Assert.That(service.ActivateBoss(), Is.True);
        Assert.That(phase.CurrentPhase, Is.EqualTo(RunPhase.BossActive));
        Assert.That(service.DefeatBoss(), Is.True);
        Assert.That(phase.CurrentPhase, Is.EqualTo(RunPhase.BossDefeated));
        phase.BeginEncore();
        Assert.That(phase.CurrentPhase, Is.EqualTo(RunPhase.Encore));
        Assert.That(service.CompleteStageDoor(), Is.True);
        Assert.That(phase.CurrentPhase, Is.EqualTo(RunPhase.Intermission));
        Assert.That(service.EnterNextAct(), Is.True);
        Assert.That(phase.CurrentPhase, Is.EqualTo(RunPhase.WorldActive));
        Assert.That(phase.WorldIndex, Is.EqualTo(3));

        phase.StartBossWarning();
        Assert.That(service.ActivateBoss(), Is.True);
        Assert.That(service.DefeatBoss(), Is.True);
        phase.BeginEncore();
        Assert.That(service.CompleteStageDoor(), Is.True);
        Assert.That(phase.CurrentPhase, Is.EqualTo(RunPhase.Finished));
        Assert.That(service.BeginNextWorld(), Is.False);

        Object.DestroyImmediate(config);
    }

    [Test]
    public void DebugDefeatHeadlinerFromWarningCreatesStageDoor()
    {
        GameConfig gameConfig = TheCircussyOneTestObjects.CreateConfig();
        RunScheduleConfig scheduleConfig = RunScheduleConfig.CreateRuntimeDefault();
        var schedule = new RunActScheduleState(scheduleConfig);
        var phase = CreateWorldActivePhase();
        var defeatState = new HeadlinerDefeatState();
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(gameConfig);
        player.Teleport(new Vector3(4f, 0f, -3f));
        GameObject ground = CreateFlatGround();
        var portalSystem = new FinishPortalSystem(
            phase,
            schedule,
            player,
            scheduleConfig,
            gameConfig,
            headlinerDefeatState: defeatState);
        var service = new RunDebugCommandService(
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            phase,
            schedule,
            headlinerDefeatState: defeatState,
            player: player);
        portalSystem.Start();
        schedule.BeginAct(1);

        Assert.That(service.JumpToActFinale(), Is.True);
        Assert.That(phase.CurrentPhase, Is.EqualTo(RunPhase.BossWarning));
        Assert.That(service.DefeatProxyHeadliner(), Is.True);

        Assert.That(phase.CurrentPhase, Is.EqualTo(RunPhase.BossDefeated));
        Assert.That(defeatState.TryGetDeathPosition(out Vector3 proxyDeathPosition), Is.True);
        Assert.That(Vector3.Distance(proxyDeathPosition, player.Position), Is.LessThan(0.001f));
        Assert.That(portalSystem.ActivePortal, Is.Null);

        phase.BeginEncore();

        Assert.That(portalSystem.ActivePortal, Is.Not.Null);

        portalSystem.Dispose();
        Object.DestroyImmediate(ground);
        TheCircussyOneTestObjects.Destroy(scheduleConfig);
        TheCircussyOneTestObjects.Destroy(gameConfig);
    }

    [Test]
    public void StageDoorCompletesOnlyDuringEncore()
    {
        var phase = CreateWorldActivePhase();
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        var view = go.AddComponent<FinishPortalView>();
        view.Prepare(phase);

        view.BeginInteraction();
        view.TickInteraction(1f, new RunCurrencyState());

        Assert.That(phase.CurrentPhase, Is.EqualTo(RunPhase.WorldActive));
        Assert.That(go.activeSelf, Is.True);

        phase.StartBossWarning();
        phase.ActivateBoss();
        phase.MarkBossDefeated();
        phase.BeginEncore();
        view.Prepare(phase);
        view.BeginInteraction();
        view.TickInteraction(1f, new RunCurrencyState());

        Assert.That(phase.CurrentPhase, Is.EqualTo(RunPhase.Intermission));
        Assert.That(go.activeSelf, Is.False);

        Object.DestroyImmediate(go);
    }

    [Test]
    public void FinalStageDoorCompletesRunAtCurtainCall()
    {
        var phase = CreateWorldActivePhase();
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        var view = go.AddComponent<FinishPortalView>();

        phase.StartBossWarning();
        phase.ActivateBoss();
        phase.MarkBossDefeated();
        phase.BeginEncore();
        view.Prepare(phase, isFinalAct: true);
        view.BeginInteraction();
        view.TickInteraction(1f, new RunCurrencyState());

        Assert.That(phase.CurrentPhase, Is.EqualTo(RunPhase.Finished));
        Assert.That(go.activeSelf, Is.False);

        Object.DestroyImmediate(go);
    }

    [Test]
    public void StageDoorSpawnsAtConfiguredTravelDistance()
    {
        GameConfig gameConfig = TheCircussyOneTestObjects.CreateConfig();
        gameConfig.playerRunSpeed = 10f;
        RunScheduleConfig scheduleConfig = RunScheduleConfig.CreateRuntimeDefault();
        scheduleConfig.stageDoorTravelSeconds[0].minTravelSeconds = 1f;
        scheduleConfig.stageDoorTravelSeconds[0].maxTravelSeconds = 1f;
        var schedule = new RunActScheduleState(scheduleConfig);
        var phase = CreateWorldActivePhase();
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(gameConfig);
        player.transform.position = Vector3.zero;
        schedule.BeginAct(1);
        var system = new FinishPortalSystem(
            phase,
            schedule,
            player,
            scheduleConfig,
            gameConfig,
            new RunStats(gameConfig),
            new WorldSurfaceResolver());

        system.Start();
        phase.StartBossWarning();
        phase.ActivateBoss();
        phase.MarkBossDefeated();
        phase.BeginEncore();

        FinishPortalView portal = system.ActivePortal;
        Assert.That(portal, Is.Not.Null);
        Assert.That(portal.transform.position.z, Is.EqualTo(10f).Within(0.001f));

        system.Dispose();
        Object.DestroyImmediate(scheduleConfig);
        TheCircussyOneTestObjects.Destroy(gameConfig);
    }

    [Test]
    public void StageDoorWaitsForHeadlinerDeathWhenEncoreStartsByDeadline()
    {
        GameConfig gameConfig = TheCircussyOneTestObjects.CreateConfig();
        RunScheduleConfig scheduleConfig = RunScheduleConfig.CreateRuntimeDefault();
        var schedule = new RunActScheduleState(scheduleConfig);
        var phase = CreateWorldActivePhase();
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(gameConfig);
        var defeatState = new HeadlinerDefeatState();
        schedule.BeginAct(1);
        var system = new FinishPortalSystem(
            phase,
            schedule,
            player,
            scheduleConfig,
            gameConfig,
            new RunStats(gameConfig),
            new WorldSurfaceResolver(),
            null,
            null,
            defeatState);

        system.Start();
        phase.StartBossWarning();
        phase.ActivateBoss();
        phase.BeginEncore();

        Assert.That(system.ActivePortal, Is.Null);

        defeatState.RecordDeathPosition(new Vector3(2f, 0f, 3f));

        Assert.That(system.ActivePortal, Is.Not.Null);

        system.Dispose();
        Object.DestroyImmediate(scheduleConfig);
        TheCircussyOneTestObjects.Destroy(gameConfig);
    }

    [Test]
    public void StageDoorUsesFallbackWhenDefeatedHeadlinerHasNoRecordedAnchor()
    {
        GameConfig gameConfig = TheCircussyOneTestObjects.CreateConfig();
        RunScheduleConfig scheduleConfig = RunScheduleConfig.CreateRuntimeDefault();
        var schedule = new RunActScheduleState(scheduleConfig);
        var phase = CreateWorldActivePhase();
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(gameConfig);
        var defeatState = new HeadlinerDefeatState();
        schedule.BeginAct(1);
        var system = new FinishPortalSystem(
            phase,
            schedule,
            player,
            scheduleConfig,
            gameConfig,
            headlinerDefeatState: defeatState);

        system.Start();
        phase.StartBossWarning();
        phase.ActivateBoss();
        phase.MarkBossDefeated();
        phase.BeginEncore();

        Assert.That(defeatState.HasDeathPosition, Is.False);
        Assert.That(system.ActivePortal, Is.Not.Null);

        system.Dispose();
        TheCircussyOneTestObjects.Destroy(scheduleConfig);
        TheCircussyOneTestObjects.Destroy(gameConfig);
    }

    [Test]
    public void StageDoorPrefersHeadlinerDeathPositionWhenAvailable()
    {
        GameObject ground = CreateFlatGround();
        GameConfig gameConfig = TheCircussyOneTestObjects.CreateConfig();
        RunScheduleConfig scheduleConfig = RunScheduleConfig.CreateRuntimeDefault();
        scheduleConfig.stageDoorGroundClearance = 0.05f;
        var schedule = new RunActScheduleState(scheduleConfig);
        var phase = CreateWorldActivePhase();
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(gameConfig);
        var defeatState = new HeadlinerDefeatState();
        defeatState.RecordDeathPosition(new Vector3(4f, 3f, -6f));
        schedule.BeginAct(1);
        var system = new FinishPortalSystem(
            phase,
            schedule,
            player,
            scheduleConfig,
            gameConfig,
            new RunStats(gameConfig),
            new WorldSurfaceResolver(),
            null,
            null,
            defeatState);

        system.Start();
        phase.StartBossWarning();
        phase.ActivateBoss();
        phase.MarkBossDefeated();
        phase.BeginEncore();

        FinishPortalView portal = system.ActivePortal;
        Assert.That(portal, Is.Not.Null);
        Assert.That(portal.transform.position.x, Is.EqualTo(4f).Within(0.001f));
        Assert.That(portal.transform.position.z, Is.EqualTo(-6f).Within(0.001f));
        Assert.That(portal.transform.position.y, Is.EqualTo(0.05f).Within(0.001f));

        system.Dispose();
        Object.DestroyImmediate(ground);
        Object.DestroyImmediate(scheduleConfig);
        TheCircussyOneTestObjects.Destroy(gameConfig);
    }

    [Test]
    public void RunWorldLifecycleInvokesGeneratorForNextAct()
    {
        var generator = new CapturingRunWorldGenerator();
        var lifecycle = new RunWorldLifecycleSystem(
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            new WorldPropPlacementService(),
            new WorldRewardRuntimeRegistry(),
            generator,
            null);

        WorldGenerationResult result = lifecycle.TransitionToWorld(2, 12345);

        Assert.That(generator.Request.WorldIndex, Is.EqualTo(2));
        Assert.That(generator.Request.ActNumber, Is.EqualTo(2));
        Assert.That(generator.Request.Seed, Is.EqualTo(12345));
        Assert.That(result.WorldIndex, Is.EqualTo(2));
        Assert.That(result.UsedPrototypePlacement, Is.False);
    }

    [Test]
    public void RunWorldLifecycleClearsPreviousGeneratedRootOnTransition()
    {
        var generator = new RootCreatingRunWorldGenerator();
        var generationState = new RunWorldGenerationState();
        var lifecycle = new RunWorldLifecycleSystem(
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            new WorldPropPlacementService(),
            new WorldRewardRuntimeRegistry(),
            generator,
            null,
            generationState);

        lifecycle.TransitionToWorld(1, 101);
        GameObject firstRoot = generator.LastRoot;
        Assert.That(firstRoot, Is.Not.Null);

        lifecycle.TransitionToWorld(2, 202);

        Assert.That(firstRoot == null, Is.True);
        Assert.That(generator.LastRoot, Is.Not.Null);

        lifecycle.ClearCurrentWorld();
    }

    [Test]
    public void RunWorldLifecycleClearsTransientInteractableAndGeneratedState()
    {
        var rewardRegistry = new WorldRewardRuntimeRegistry();
        var generationState = new RunWorldGenerationState();
        var rewardRoot = new GameObject("Runtime Reward Root");
        var generatedRoot = new GameObject("Generated Terrain Root");
        var outlineObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        var interactable = new DummyInteractable();
        rewardRegistry.Register(rewardRoot);
        generationState.Set(new WorldGenerationResult(
            1,
            1,
            123,
            usedPrototypePlacement: false,
            generatedRoot: generatedRoot,
            rewardSpawnRequests: null,
            enemySpawnBands: null,
            playerStart: Vector3.zero,
            hasPlayerStart: false,
            headlinerAnchor: Vector3.zero,
            hasHeadlinerAnchor: false,
            stageDoorAnchor: Vector3.zero,
            hasStageDoorAnchor: false,
            bounds: default));
        SceneInteractableRegistry.Register(interactable, SceneInteractableKind.Chest);
        InteractableSelectionOutlineRegistry.Register(
            interactable,
            new[] { outlineObject.GetComponent<Renderer>() },
            Color.white,
            2f);
        var lifecycle = new RunWorldLifecycleSystem(
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            new WorldPropPlacementService(),
            rewardRegistry,
            new CapturingRunWorldGenerator(),
            null,
            generationState);

        Assert.That(rewardRegistry.Count, Is.EqualTo(1));
        Assert.That(SceneInteractableRegistry.Count, Is.EqualTo(1));
        Assert.That(InteractableSelectionOutlineRegistry.Count, Is.EqualTo(1));

        lifecycle.ClearCurrentWorld();

        Assert.That(rewardRegistry.Count, Is.Zero);
        Assert.That(rewardRoot == null, Is.True);
        Assert.That(generatedRoot == null, Is.True);
        Assert.That(generationState.Current.GeneratedRoot == null, Is.True);
        Assert.That(SceneInteractableRegistry.Count, Is.Zero);
        Assert.That(InteractableSelectionOutlineRegistry.Count, Is.Zero);

        Object.DestroyImmediate(outlineObject);
    }

    private static RunPhaseState CreateWorldActivePhase()
    {
        var phase = new RunPhaseState();
        phase.BeginWorld();
        return phase;
    }

    private static GameObject CreateFlatGround()
    {
        GameObject ground = TheCircussyOneTestObjects.CreateRoot("Stage Door Test Ground");
        var collider = ground.AddComponent<BoxCollider>();
        collider.center = Vector3.zero;
        collider.size = new Vector3(100f, 0.2f, 100f);
        ground.transform.position = new Vector3(0f, -0.1f, 0f);
        Physics.SyncTransforms();
        return ground;
    }

    private static void AdvanceToNextWorld(RunPhaseState phase)
    {
        phase.StartBossWarning();
        phase.ActivateBoss();
        phase.MarkBossDefeated();
        phase.BeginEncore();
        phase.EnterIntermission();
        phase.BeginNextWorld();
        phase.BeginWorld();
    }

    private sealed class CapturingRunWorldGenerator : IRunWorldGenerator
    {
        public WorldGenerationRequest Request { get; private set; }

        public WorldGenerationResult Generate(WorldGenerationRequest request)
        {
            Request = request;
            return new WorldGenerationResult(request.WorldIndex, request.ActNumber, request.Seed, usedPrototypePlacement: false);
        }
    }

    private sealed class PlayerStartRunWorldGenerator : IRunWorldGenerator
    {
        private readonly Vector3 playerStart;

        public PlayerStartRunWorldGenerator(Vector3 playerStart)
        {
            this.playerStart = playerStart;
        }

        public WorldGenerationResult Generate(WorldGenerationRequest request)
        {
            return new WorldGenerationResult(
                request.WorldIndex,
                request.ActNumber,
                request.Seed,
                usedPrototypePlacement: false,
                generatedRoot: null,
                rewardSpawnRequests: null,
                enemySpawnBands: null,
                playerStart: playerStart,
                hasPlayerStart: true,
                headlinerAnchor: Vector3.zero,
                hasHeadlinerAnchor: false,
                stageDoorAnchor: Vector3.zero,
                hasStageDoorAnchor: false,
                bounds: default);
        }
    }

    private sealed class RootCreatingRunWorldGenerator : IRunWorldGenerator
    {
        public GameObject LastRoot { get; private set; }

        public WorldGenerationResult Generate(WorldGenerationRequest request)
        {
            LastRoot = new GameObject($"TCO Test Generated Root {request.WorldIndex}");
            return new WorldGenerationResult(
                request.WorldIndex,
                request.ActNumber,
                request.Seed,
                usedPrototypePlacement: false,
                generatedRoot: LastRoot,
                rewardSpawnRequests: null,
                enemySpawnBands: null,
                playerStart: Vector3.zero,
                hasPlayerStart: false,
                headlinerAnchor: Vector3.zero,
                hasHeadlinerAnchor: false,
                stageDoorAnchor: Vector3.zero,
                hasStageDoorAnchor: false,
                bounds: default);
        }
    }

    private sealed class DummyInteractable : IInteractable
    {
        public bool IsInteractionAvailable => true;
        public InteractPriority Priority => InteractPriority.Other;
        public Vector3 InteractionPosition => Vector3.zero;
        public string PromptText => "Test";
        public float HoldSeconds => 1f;
        public float Progress => 0f;

        public void SetInteractionTargeted(bool targeted)
        {
        }

        public void BeginInteraction()
        {
        }

        public void TickInteraction(float deltaTime, RunCurrencyState currency)
        {
        }

        public void CancelInteraction()
        {
        }
    }
}
