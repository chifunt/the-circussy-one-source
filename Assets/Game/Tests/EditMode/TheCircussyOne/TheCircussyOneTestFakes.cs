using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TheCircussyOne.Runtime;
using UnityEngine;

internal sealed class FakeGameTime : IGameTime
{
    public float Time { get; set; }
    public float DeltaTime { get; set; } = 0.016f;
}

internal sealed class FakeInputService : IInputService
{
    public Vector2 Movement { get; set; }
    public Vector2 Look { get; set; }
    public LookInputKind LookKind { get; set; }
    public float CameraOrbitTriggerAxis { get; set; }
    public bool JumpPressedThisFrame { get; set; }
    public bool JumpHeld { get; set; }
    public UpgradeNavigationDirection UpgradeNavigation { get; set; }
    public UiCycleNavigationDirection UiCycleNavigation { get; set; }
    public int UpgradeChoicePressedThisFrame { get; set; }
    public int PerformerChoicePressedThisFrame { get; set; }
    public bool SubmitPressedThisFrame { get; set; }
    public bool PausePressedThisFrame { get; set; }
    public bool InteractPressedThisFrame { get; set; }
    public bool InteractHeld { get; set; }
    public bool InteractReleasedThisFrame { get; set; }
    public bool DebugRestartPressedThisFrame { get; set; }
}

internal sealed class FakeGameAudio : IGameAudio
{
    public List<GameAudioCue> PlayedCues { get; } = new();

    public List<(GameAudioCue cue, Vector3 position)> PlayedAtCues { get; } = new();

    public List<(GameAudioCue cue, object owner)> StartedLoops { get; } = new();

    public List<(GameAudioCue cue, object owner)> StoppedLoops { get; } = new();

    public void Play(GameAudioCue cue)
    {
        PlayedCues.Add(cue);
    }

    public void PlayAt(GameAudioCue cue, Vector3 position)
    {
        PlayedAtCues.Add((cue, position));
    }

    public void PlayXpCollect(Vector3 position)
    {
        PlayedAtCues.Add((GameAudioCue.XpCollect, position));
    }

    public void StartLoop(GameAudioCue cue, object owner, Vector3 position, float initialProgress = 0f)
    {
        StartedLoops.Add((cue, owner));
    }

    public void UpdateLoop(GameAudioCue cue, object owner, Vector3 position, float progress01)
    {
    }

    public void StopLoop(GameAudioCue cue, object owner)
    {
        StoppedLoops.Add((cue, owner));
    }

    public void StopAllLoops()
    {
    }
}

internal sealed class FakeRunRestarter : IRunRestarter
{
    public int RestartCount { get; private set; }
    public RunRestartReason LastReason { get; private set; }

    public void RestartRun()
    {
        RestartRun(RunRestartReason.StageSetup);
    }

    public void RestartRun(RunRestartReason reason)
    {
        LastReason = reason;
        RestartCount++;
    }
}

internal sealed class FakeMainMenuReturner : IMainMenuReturner
{
    public int ReturnCount { get; private set; }

    public void ReturnToMainMenu()
    {
        ReturnCount++;
    }
}

internal sealed class FakeRunTransitionService : IRunTransitionService
{
    public bool Enabled { get; set; } = true;
    public bool Accept { get; set; } = true;
    public bool RunImmediately { get; set; } = true;
    public bool IsTransitionActive { get; set; }
    public int PlayCount { get; private set; }
    public int PlayAsyncCount { get; private set; }
    public int SceneReloadCount { get; private set; }
    public string LastTitle { get; private set; }
    public string LastSubtitle { get; private set; }
    public float? LastMinimumVisibleSeconds { get; private set; }
    public Action LastWork { get; private set; }
    public Func<AsyncOperation> LastLoadScene { get; private set; }
    public Func<UniTask> LastAsyncWork { get; private set; }
    public string LastLoadingStage { get; private set; }

    public bool IsEnabled => Enabled;

    public void SetLoadingStage(string stage)
    {
        LastLoadingStage = stage;
    }

    public bool Play(string title, string subtitle, Action work)
    {
        return Play(title, subtitle, work, null);
    }

    public bool Play(string title, string subtitle, Action work, float minimumVisibleSeconds)
    {
        return Play(title, subtitle, work, (float?)minimumVisibleSeconds);
    }

    public bool PlaySceneReload(string title, string subtitle, Func<AsyncOperation> loadScene)
    {
        SceneReloadCount++;
        LastTitle = title;
        LastSubtitle = subtitle;
        LastLoadScene = loadScene;
        if (!Accept)
        {
            return false;
        }

        if (RunImmediately)
        {
            _ = loadScene?.Invoke();
        }

        return true;
    }

    public bool PlayAsync(string title, string subtitle, Func<UniTask> work)
    {
        return PlayAsync(title, subtitle, work, null);
    }

    public bool PlayAsync(string title, string subtitle, Func<UniTask> work, float minimumVisibleSeconds)
    {
        return PlayAsync(title, subtitle, work, (float?)minimumVisibleSeconds);
    }

    private bool Play(string title, string subtitle, Action work, float? minimumVisibleSeconds)
    {
        PlayCount++;
        LastTitle = title;
        LastSubtitle = subtitle;
        LastMinimumVisibleSeconds = minimumVisibleSeconds;
        LastWork = work;
        if (!Accept)
        {
            return false;
        }

        if (RunImmediately)
        {
            work?.Invoke();
        }

        return true;
    }

    private bool PlayAsync(string title, string subtitle, Func<UniTask> work, float? minimumVisibleSeconds)
    {
        PlayAsyncCount++;
        LastTitle = title;
        LastSubtitle = subtitle;
        LastMinimumVisibleSeconds = minimumVisibleSeconds;
        LastAsyncWork = work;
        if (!Accept)
        {
            return false;
        }

        if (RunImmediately)
        {
            if (work != null)
            {
                work.Invoke().GetAwaiter().GetResult();
            }
        }

        return true;
    }
}
