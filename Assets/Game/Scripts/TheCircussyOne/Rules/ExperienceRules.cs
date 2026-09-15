namespace TheCircussyOne.Rules
{
    public readonly struct ExperienceCurveSettings
    {
        public ExperienceCurveSettings(
            int earlyEndLevel,
            int midEndLevel,
            int earlyBase,
            int earlyIncrement,
            int midBase,
            int midIncrement,
            int lateBase,
            int lateIncrement,
            float requirementMultiplier = 1f)
        {
            EarlyEndLevel = earlyEndLevel;
            MidEndLevel = midEndLevel;
            EarlyBase = earlyBase;
            EarlyIncrement = earlyIncrement;
            MidBase = midBase;
            MidIncrement = midIncrement;
            LateBase = lateBase;
            LateIncrement = lateIncrement;
            RequirementMultiplier = requirementMultiplier;
        }

        public int EarlyEndLevel { get; }
        public int MidEndLevel { get; }
        public int EarlyBase { get; }
        public int EarlyIncrement { get; }
        public int MidBase { get; }
        public int MidIncrement { get; }
        public int LateBase { get; }
        public int LateIncrement { get; }
        public float RequirementMultiplier { get; }

        public static ExperienceCurveSettings Starter => new(
            earlyEndLevel: 10,
            midEndLevel: 25,
            earlyBase: 21,
            earlyIncrement: 14,
            midBase: 161,
            midIncrement: 23,
            lateBase: 506,
            lateIncrement: 37);
    }

    public static class ExperienceRules
    {
        public static void AddExperience(int currentExperience, int currentLevel, int amount, int target, out int nextExperience, out int nextLevel)
        {
            nextExperience = currentExperience;
            nextLevel = currentLevel;
            if (amount <= 0)
            {
                return;
            }

            nextExperience += amount;
            while (nextExperience >= target)
            {
                nextExperience -= target;
                nextLevel++;
            }
        }

        public static void AddExperience(
            int currentExperience,
            int currentLevel,
            int amount,
            ExperienceCurveSettings curve,
            out int nextExperience,
            out int nextLevel,
            out int nextTarget)
        {
            nextExperience = currentExperience;
            nextLevel = currentLevel;
            nextTarget = ExperienceRequiredForNextLevel(nextLevel, curve);
            if (amount <= 0)
            {
                return;
            }

            nextExperience += amount;
            while (nextExperience >= nextTarget)
            {
                nextExperience -= nextTarget;
                nextLevel++;
                nextTarget = ExperienceRequiredForNextLevel(nextLevel, curve);
            }
        }

        public static int ExperienceRequiredForNextLevel(int level, ExperienceCurveSettings curve)
        {
            int safeLevel = System.Math.Max(1, level);
            int earlyEnd = System.Math.Max(1, curve.EarlyEndLevel);
            int midEnd = System.Math.Max(earlyEnd + 1, curve.MidEndLevel);
            int earlyBase = System.Math.Max(1, curve.EarlyBase);
            int earlyIncrement = System.Math.Max(0, curve.EarlyIncrement);
            int midBase = System.Math.Max(1, curve.MidBase);
            int midIncrement = System.Math.Max(0, curve.MidIncrement);
            int lateBase = System.Math.Max(1, curve.LateBase);
            int lateIncrement = System.Math.Max(0, curve.LateIncrement);

            if (safeLevel <= earlyEnd)
            {
                return ApplyRequirementMultiplier(earlyBase + earlyIncrement * (safeLevel - 1), curve.RequirementMultiplier);
            }

            if (safeLevel <= midEnd)
            {
                return ApplyRequirementMultiplier(midBase + midIncrement * (safeLevel - earlyEnd), curve.RequirementMultiplier);
            }

            return ApplyRequirementMultiplier(lateBase + lateIncrement * (safeLevel - midEnd), curve.RequirementMultiplier);
        }

        public static int TotalExperienceRequiredForLevelUps(int startLevel, int levelUps, ExperienceCurveSettings curve)
        {
            int safeLevel = System.Math.Max(1, startLevel);
            int total = 0;
            for (int i = 0; i < levelUps; i++)
            {
                total += ExperienceRequiredForNextLevel(safeLevel + i, curve);
            }

            return total;
        }

        private static int ApplyRequirementMultiplier(int baseTarget, float multiplier)
        {
            float safeMultiplier = multiplier <= 0f ? 1f : multiplier;
            return System.Math.Max(1, (int)System.Math.Ceiling(System.Math.Max(1, baseTarget) * safeMultiplier));
        }
    }
}
