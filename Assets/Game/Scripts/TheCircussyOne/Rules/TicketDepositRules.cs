using TheCircussyOne.Content;
using UnityEngine;

namespace TheCircussyOne.Rules
{
    public static class TicketDepositRules
    {
        public static int ResolveReward(TicketDepositDefinition definition, int seed)
        {
            if (definition == null)
            {
                return 0;
            }

            int min = Mathf.Max(0, definition.minTickets);
            int max = Mathf.Max(min, definition.maxTickets);
            if (max <= 0)
            {
                return 0;
            }

            uint state = (uint)DeterministicSeed.Mix(seed);
            int range = max - min + 1;
            return min + (int)(state % (uint)range);
        }

        public static int StableSeed(string contentId, Vector3 position, int runSeed = 0)
        {
            return DeterministicSeed.ContentPosition(runSeed, contentId, position);
        }

        public static int PaidChunkCount(float normalizedProgress, int chunkCount)
        {
            if (chunkCount <= 0 || normalizedProgress <= 0f)
            {
                return 0;
            }

            if (normalizedProgress >= 1f)
            {
                return chunkCount;
            }

            return Mathf.Clamp(Mathf.FloorToInt(normalizedProgress * chunkCount), 0, chunkCount);
        }

        public static int PayoutForChunk(int totalReward, int chunkCount, int chunkIndex)
        {
            if (totalReward <= 0 || chunkCount <= 0 || chunkIndex < 0 || chunkIndex >= chunkCount)
            {
                return 0;
            }

            int baseAmount = totalReward / chunkCount;
            int remainder = totalReward % chunkCount;
            return baseAmount + (chunkIndex < remainder ? 1 : 0);
        }

        public static float NormalizedProgress(float elapsedSeconds, float holdSeconds)
        {
            if (holdSeconds <= 0f)
            {
                return 1f;
            }

            return Mathf.Clamp01(elapsedSeconds / holdSeconds);
        }
    }
}
