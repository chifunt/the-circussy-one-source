using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using UnityEngine;

namespace TheCircussyOne.Runtime
{
    public sealed class TicketDepositRuntime
    {
        private readonly TicketDepositDefinition definition;
        private readonly Vector3 position;
        private readonly RunSeedState runSeedState;
        private int totalReward;
        private int resolvedSeed;
        private bool rewardResolved;
        private float elapsedSeconds;
        private int paidChunks;
        private bool completed;

        public TicketDepositRuntime(TicketDepositDefinition definition, Vector3 position, RunSeedState runSeedState = null)
        {
            this.definition = definition;
            this.position = position;
            this.runSeedState = runSeedState;
        }

        public int TotalReward => ResolveTotalReward();
        public int PaidTickets { get; private set; }
        public bool Completed => completed;
        public float Progress => definition == null ? 0f : TicketDepositRules.NormalizedProgress(elapsedSeconds, definition.holdSeconds);

        public int Tick(float deltaTime, RunCurrencyState currency)
        {
            int reward = ResolveTotalReward();
            if (definition == null || completed || reward <= 0)
            {
                return 0;
            }

            elapsedSeconds = Mathf.Min(definition.holdSeconds, elapsedSeconds + Mathf.Max(0f, deltaTime));
            int granted = definition.payoutMode == TicketDepositPayoutMode.Chunked
                ? GrantAvailableChunks(currency)
                : GrantOnCompletion(currency);

            if (elapsedSeconds >= definition.holdSeconds)
            {
                completed = true;
            }

            return granted;
        }

        public void Cancel()
        {
            if (definition == null || completed)
            {
                return;
            }

            if (definition.payoutMode == TicketDepositPayoutMode.Chunked)
            {
                int chunks = definition.EffectivePayoutChunks;
                elapsedSeconds = chunks <= 0 ? 0f : definition.holdSeconds * ((float)paidChunks / chunks);
                return;
            }

            elapsedSeconds = 0f;
        }

        private int GrantOnCompletion(RunCurrencyState currency)
        {
            if (elapsedSeconds < definition.holdSeconds || PaidTickets > 0)
            {
                return 0;
            }

            int reward = ResolveTotalReward();
            PaidTickets = reward;
            currency?.AddTickets(reward);
            return reward;
        }

        private int GrantAvailableChunks(RunCurrencyState currency)
        {
            int chunks = definition.EffectivePayoutChunks;
            int chunkCount = TicketDepositRules.PaidChunkCount(Progress, chunks);
            int granted = 0;
            while (paidChunks < chunkCount)
            {
                int payout = TicketDepositRules.PayoutForChunk(ResolveTotalReward(), chunks, paidChunks);
                paidChunks++;
                PaidTickets += payout;
                granted += payout;
                currency?.AddTickets(payout);
            }

            return granted;
        }

        private int ResolveTotalReward()
        {
            int seed = TicketDepositRules.StableSeed(
                definition != null ? definition.Id : string.Empty,
                position,
                runSeedState != null ? runSeedState.CurrentSeed : 0);
            if (rewardResolved && (resolvedSeed == seed || PaidTickets > 0 || paidChunks > 0))
            {
                return totalReward;
            }

            resolvedSeed = seed;
            totalReward = TicketDepositRules.ResolveReward(definition, seed);
            rewardResolved = true;
            return totalReward;
        }
    }
}
