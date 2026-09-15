using Cysharp.Threading.Tasks;

namespace TheCircussyOne.Runtime
{
    public sealed class RunRetryService
    {
        private const string RetryTitle = "Resetting the Stage";
        private const string RetrySubtitle = "Loading the next show...";

        private readonly RunResetCoordinator resetCoordinator;
        private readonly PerformerSelectionPresenter performerSelectionPresenter;
        private readonly IRunTransitionService transitionService;
        private bool isRetrying;

        public RunRetryService(
            RunResetCoordinator resetCoordinator,
            PerformerSelectionPresenter performerSelectionPresenter = null,
            IRunTransitionService transitionService = null)
        {
            this.resetCoordinator = resetCoordinator;
            this.performerSelectionPresenter = performerSelectionPresenter;
            this.transitionService = transitionService;
        }

        public bool TryRetryAfterDeath()
        {
            if (isRetrying)
            {
                return false;
            }

            isRetrying = true;
            if (transitionService != null && transitionService.IsEnabled)
            {
                bool started = transitionService.PlayAsync(RetryTitle, RetrySubtitle, RetryAfterDeathAsync);
                if (!started)
                {
                    isRetrying = false;
                }

                return started;
            }

            RetryAfterDeathAsync().Forget();
            return true;
        }

        public void ResetRunStateForRetry()
        {
            resetCoordinator?.ResetToPerformerSelection(RunRestartReason.RetryAfterDeath);
        }

        private async UniTask RetryAfterDeathAsync()
        {
            try
            {
                await UniTask.Yield(PlayerLoopTiming.Update);
                ResetRunStateForRetry();
                performerSelectionPresenter?.ShowSelection();
                await UniTask.Yield(PlayerLoopTiming.Update);
            }
            finally
            {
                isRetrying = false;
            }
        }
    }
}
