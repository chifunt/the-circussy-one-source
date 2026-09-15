using NUnit.Framework;
using TheCircussyOne.Config;
using TheCircussyOne.Runtime;
using UnityEngine;

public sealed class GameAudioPresenterTests
{
    [Test]
    public void HeadlinerDefeatDoesNotPlayEncoreCueUntilEncorePressureStarts()
    {
        RunScheduleConfig config = RunScheduleConfig.CreateRuntimeDefault();
        config.encoreGraceSeconds = 10f;
        var schedule = new RunActScheduleState(config);
        var phase = new RunPhaseState();
        var defeatState = new HeadlinerDefeatState();
        var audio = new FakeGameAudio();
        var presenter = new GameAudioPresenter(audio, phaseState: phase, scheduleState: schedule, headlinerDefeatState: defeatState);

        presenter.Start();
        try
        {
            schedule.BeginAct(1);
            phase.BeginWorld();
            phase.StartBossWarning();
            phase.ActivateBoss();
            defeatState.RecordDeathPosition(Vector3.zero);
            phase.MarkBossDefeated();
            schedule.BeginEncore();

            Assert.That(audio.PlayedCues, Is.EqualTo(new[] { GameAudioCue.AnnouncementHeadlinerDefeat }));

            schedule.TickEncore(9.9f);

            Assert.That(audio.PlayedCues, Is.EqualTo(new[] { GameAudioCue.AnnouncementHeadlinerDefeat }));

            schedule.TickEncore(0.2f);

            Assert.That(audio.PlayedCues, Is.EqualTo(new[]
            {
                GameAudioCue.AnnouncementHeadlinerDefeat,
                GameAudioCue.AnnouncementShowtimeHeadliner
            }));
        }
        finally
        {
            presenter.Dispose();
            Object.DestroyImmediate(config);
        }
    }

    [Test]
    public void HeadlinerDeadlineEncoreDoesNotPlayDefeatCue()
    {
        RunScheduleConfig config = RunScheduleConfig.CreateRuntimeDefault();
        var schedule = new RunActScheduleState(config);
        var phase = new RunPhaseState();
        var defeatState = new HeadlinerDefeatState();
        var audio = new FakeGameAudio();
        var presenter = new GameAudioPresenter(audio, phaseState: phase, scheduleState: schedule, headlinerDefeatState: defeatState);

        presenter.Start();
        try
        {
            phase.BeginWorld();
            phase.StartBossWarning();
            phase.ActivateBoss();
            phase.BeginEncore();

            Assert.That(audio.PlayedCues, Is.Empty);
        }
        finally
        {
            presenter.Dispose();
            Object.DestroyImmediate(config);
        }
    }

    [Test]
    public void LateHeadlinerDefeatDuringEncoreStillPlaysDefeatCue()
    {
        RunScheduleConfig config = RunScheduleConfig.CreateRuntimeDefault();
        var schedule = new RunActScheduleState(config);
        var phase = new RunPhaseState();
        var defeatState = new HeadlinerDefeatState();
        var audio = new FakeGameAudio();
        var presenter = new GameAudioPresenter(audio, phaseState: phase, scheduleState: schedule, headlinerDefeatState: defeatState);

        presenter.Start();
        try
        {
            phase.BeginWorld();
            phase.StartBossWarning();
            phase.ActivateBoss();
            phase.BeginEncore();
            defeatState.RecordDeathPosition(new Vector3(1f, 0f, 2f));

            Assert.That(audio.PlayedCues, Is.EqualTo(new[] { GameAudioCue.AnnouncementHeadlinerDefeat }));
        }
        finally
        {
            presenter.Dispose();
            Object.DestroyImmediate(config);
        }
    }
}
