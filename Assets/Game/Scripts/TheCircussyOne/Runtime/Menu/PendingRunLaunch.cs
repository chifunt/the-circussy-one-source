using System;
using Cysharp.Threading.Tasks;

namespace TheCircussyOne.Runtime
{
    public readonly struct PendingRunLaunchRequest
    {
        public PendingRunLaunchRequest(string performerId, bool waitForGameplayReady)
        {
            PerformerId = performerId ?? string.Empty;
            WaitForGameplayReady = waitForGameplayReady;
        }

        public string PerformerId { get; }
        public bool WaitForGameplayReady { get; }
        public bool IsValid => !string.IsNullOrWhiteSpace(PerformerId);
    }

    public static class PendingRunLaunch
    {
        private static PendingRunLaunchRequest pendingRequest;
        private static bool hasPendingRequest;
        private static UniTaskCompletionSource<bool> gameplayReadySource;
        private static Action gameplayReleaseAction;

        public static bool HasPendingRequest => hasPendingRequest;

        public static void SetPerformer(string performerId, bool waitForGameplayReady)
        {
            pendingRequest = new PendingRunLaunchRequest(performerId, waitForGameplayReady);
            hasPendingRequest = pendingRequest.IsValid;
            gameplayReadySource = waitForGameplayReady ? new UniTaskCompletionSource<bool>() : null;
            gameplayReleaseAction = null;
        }

        public static bool TryConsume(out PendingRunLaunchRequest request)
        {
            request = pendingRequest;
            if (!hasPendingRequest)
            {
                return false;
            }

            hasPendingRequest = false;
            pendingRequest = default;
            return request.IsValid;
        }

        public static async UniTask WaitForGameplayReadyAsync()
        {
            if (gameplayReadySource == null)
            {
                return;
            }

            await gameplayReadySource.Task;
        }

        public static void RegisterGameplayRelease(Action releaseAction)
        {
            gameplayReleaseAction = releaseAction;
        }

        public static void ReleaseGameplay()
        {
            Action releaseAction = gameplayReleaseAction;
            gameplayReleaseAction = null;
            releaseAction?.Invoke();
        }

        public static void SignalGameplayReady()
        {
            gameplayReadySource?.TrySetResult(true);
            gameplayReadySource = null;
        }

        public static void SignalGameplayFailed()
        {
            gameplayReadySource?.TrySetResult(false);
            gameplayReadySource = null;
            ReleaseGameplay();
        }

        public static void Clear()
        {
            pendingRequest = default;
            hasPendingRequest = false;
            SignalGameplayFailed();
        }
    }
}
