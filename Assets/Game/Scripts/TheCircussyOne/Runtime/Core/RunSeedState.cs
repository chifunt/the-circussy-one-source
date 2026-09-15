using TheCircussyOne.Rules;

namespace TheCircussyOne.Runtime
{
    public sealed class RunSeedState
    {
        public int CurrentSeed { get; private set; }
        public int RunIndex { get; private set; }

        public void ResetForNewRun()
        {
            RunIndex++;
            CurrentSeed = DeterministicSeed.NewRuntimeSeed(RunIndex);
        }

        public void SetForTests(int seed, int runIndex = 1)
        {
            CurrentSeed = seed;
            RunIndex = runIndex < 0 ? 0 : runIndex;
        }

        public int Combine(params int[] localSeeds)
        {
            if (localSeeds == null || localSeeds.Length == 0)
            {
                return DeterministicSeed.Mix(CurrentSeed);
            }

            int[] values = new int[localSeeds.Length + 1];
            values[0] = CurrentSeed;
            for (int i = 0; i < localSeeds.Length; i++)
            {
                values[i + 1] = localSeeds[i];
            }

            return DeterministicSeed.Combine(values);
        }
    }
}
