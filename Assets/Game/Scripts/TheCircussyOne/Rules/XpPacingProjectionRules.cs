using System.Collections.Generic;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using UnityEngine;

namespace TheCircussyOne.Rules
{
    public readonly struct XpPacingProjectionSettings
    {
        public XpPacingProjectionSettings(
            float durationSeconds,
            IReadOnlyList<float> checkpointsSeconds,
            IReadOnlyList<float> killEfficiencies,
            float xpGainMultiplier = 1f)
        {
            DurationSeconds = durationSeconds;
            CheckpointsSeconds = checkpointsSeconds;
            KillEfficiencies = killEfficiencies;
            XpGainMultiplier = xpGainMultiplier;
        }

        public float DurationSeconds { get; }
        public IReadOnlyList<float> CheckpointsSeconds { get; }
        public IReadOnlyList<float> KillEfficiencies { get; }
        public float XpGainMultiplier { get; }

        public static XpPacingProjectionSettings Default => new(
            durationSeconds: 600f,
            checkpointsSeconds: new[] { 60f, 120f, 180f, 300f, 600f },
            killEfficiencies: new[] { 0.5f, 0.7f, 1f },
            xpGainMultiplier: 1f);
    }

    public readonly struct XpPacingEnemySource
    {
        public XpPacingEnemySource(string displayName, float spawnWeight, int xpBudget, XpDropStyle dropStyle, int resolvedXpPerKill)
        {
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? "Unnamed Enemy" : displayName;
            SpawnWeight = Mathf.Max(0f, spawnWeight);
            XpBudget = Mathf.Max(0, xpBudget);
            DropStyle = dropStyle;
            ResolvedXpPerKill = Mathf.Max(0, resolvedXpPerKill);
        }

        public string DisplayName { get; }
        public float SpawnWeight { get; }
        public int XpBudget { get; }
        public XpDropStyle DropStyle { get; }
        public int ResolvedXpPerKill { get; }
    }

    public readonly struct XpPacingProjectionRow
    {
        public XpPacingProjectionRow(
            float checkpointSeconds,
            float killEfficiency,
            int spawnUpperBound,
            int estimatedKills,
            int totalXp,
            int projectedLevel,
            int popupCount,
            int experienceRemainder,
            int nextLevelTarget,
            float nextLevelProgress01)
        {
            CheckpointSeconds = checkpointSeconds;
            KillEfficiency = killEfficiency;
            SpawnUpperBound = spawnUpperBound;
            EstimatedKills = estimatedKills;
            TotalXp = totalXp;
            ProjectedLevel = projectedLevel;
            PopupCount = popupCount;
            ExperienceRemainder = experienceRemainder;
            NextLevelTarget = nextLevelTarget;
            NextLevelProgress01 = Mathf.Clamp01(nextLevelProgress01);
        }

        public float CheckpointSeconds { get; }
        public float KillEfficiency { get; }
        public int SpawnUpperBound { get; }
        public int EstimatedKills { get; }
        public int TotalXp { get; }
        public int ProjectedLevel { get; }
        public int PopupCount { get; }
        public int ExperienceRemainder { get; }
        public int NextLevelTarget { get; }
        public float NextLevelProgress01 { get; }
    }

    public sealed class XpPacingProjectionResult
    {
        public XpPacingProjectionResult(
            IReadOnlyList<XpPacingEnemySource> enemySources,
            IReadOnlyList<XpPacingProjectionRow> rows,
            IReadOnlyList<string> warnings,
            float weightedXpPerKill)
        {
            EnemySources = enemySources;
            Rows = rows;
            Warnings = warnings;
            WeightedXpPerKill = Mathf.Max(0f, weightedXpPerKill);
        }

        public IReadOnlyList<XpPacingEnemySource> EnemySources { get; }
        public IReadOnlyList<XpPacingProjectionRow> Rows { get; }
        public IReadOnlyList<string> Warnings { get; }
        public float WeightedXpPerKill { get; }
    }

    public static class XpPacingProjectionRules
    {
        public static XpPacingProjectionResult Project(
            GameConfig config,
            EnemyCatalog enemyCatalog,
            XpGemCatalog xpGemCatalog,
            XpPacingProjectionSettings settings)
        {
            var warnings = new List<string>();
            var sources = BuildEnemySources(enemyCatalog, xpGemCatalog, warnings);
            float weightedXpPerKill = WeightedXpPerKill(sources, warnings);
            var rows = new List<XpPacingProjectionRow>();

            if (config == null)
            {
                warnings.Add("GameConfig is missing; XP pacing cannot be projected.");
                return new XpPacingProjectionResult(sources, rows, warnings, weightedXpPerKill);
            }

            IReadOnlyList<float> checkpoints = settings.CheckpointsSeconds ?? XpPacingProjectionSettings.Default.CheckpointsSeconds;
            IReadOnlyList<float> efficiencies = settings.KillEfficiencies ?? XpPacingProjectionSettings.Default.KillEfficiencies;
            float safeXpGainMultiplier = settings.XpGainMultiplier <= 0f ? 1f : settings.XpGainMultiplier;

            for (int i = 0; i < checkpoints.Count; i++)
            {
                float checkpoint = Mathf.Max(0f, checkpoints[i]);
                int spawnUpperBound = SpawnUpperBound(config, checkpoint);
                for (int j = 0; j < efficiencies.Count; j++)
                {
                    float efficiency = Mathf.Clamp01(efficiencies[j]);
                    int estimatedKills = Mathf.FloorToInt(spawnUpperBound * efficiency);
                    int totalXp = Mathf.FloorToInt(estimatedKills * weightedXpPerKill * safeXpGainMultiplier);
                    ProjectLevel(totalXp, config.ExperienceCurve, out int level, out int remainder, out int nextTarget);
                    rows.Add(new XpPacingProjectionRow(
                        checkpoint,
                        efficiency,
                        spawnUpperBound,
                        estimatedKills,
                        totalXp,
                        level,
                        Mathf.Max(0, level - 1),
                        remainder,
                        nextTarget,
                        nextTarget > 0 ? (float)remainder / nextTarget : 0f));
                }
            }

            return new XpPacingProjectionResult(sources, rows, warnings, weightedXpPerKill);
        }

        public static List<XpPacingEnemySource> BuildEnemySources(
            EnemyCatalog enemyCatalog,
            XpGemCatalog xpGemCatalog,
            List<string> warnings = null)
        {
            var sources = new List<XpPacingEnemySource>();
            if (enemyCatalog == null || enemyCatalog.Enemies == null || enemyCatalog.Enemies.Count == 0)
            {
                warnings?.Add("EnemyCatalog is missing or empty; projected XP per kill is 0.");
                return sources;
            }

            if (xpGemCatalog == null || xpGemCatalog.Gems == null || XpDropRules.ValidGemCount(xpGemCatalog.Gems) == 0)
            {
                warnings?.Add("XpGemCatalog is missing or has no positive-value gems; enemy XP budgets cannot resolve into actual drops.");
            }

            for (int i = 0; i < enemyCatalog.Enemies.Count; i++)
            {
                EnemyDefinition enemy = enemyCatalog.Enemies[i];
                if (enemy == null || enemy.spawnWeight <= 0f)
                {
                    continue;
                }

                int resolvedXp = 0;
                if (xpGemCatalog != null && xpGemCatalog.Gems != null)
                {
                    List<XpGemDrop> drops = XpDropRules.BuildDrops(enemy.xpBudget, xpGemCatalog.Gems, enemy.dropStyle, seed: 0);
                    for (int dropIndex = 0; dropIndex < drops.Count; dropIndex++)
                    {
                        resolvedXp += drops[dropIndex].Amount;
                    }
                }

                sources.Add(new XpPacingEnemySource(
                    enemy.DisplayName,
                    enemy.spawnWeight,
                    enemy.xpBudget,
                    enemy.dropStyle,
                    resolvedXp));
            }

            if (sources.Count == 0)
            {
                warnings?.Add("EnemyCatalog has no positive-weight enemies; projected XP per kill is 0.");
            }

            return sources;
        }

        public static int SpawnUpperBound(GameConfig config, float seconds)
        {
            if (config == null || seconds <= 0f)
            {
                return 0;
            }

            float elapsed = 0f;
            float nextSpawn = Mathf.Max(0.01f, config.spawnIntervalSeconds);
            int count = 0;
            int guard = 0;
            while (nextSpawn <= seconds && guard++ < 1_000_000)
            {
                elapsed = nextSpawn;
                count++;
                float interval = DifficultyRules.SpawnInterval(
                    config.spawnIntervalSeconds,
                    config.minSpawnIntervalSeconds,
                    elapsed,
                    config.spawnRampDurationSeconds,
                    config.difficultyRampEase);
                nextSpawn += Mathf.Max(0.01f, interval);
            }

            return count;
        }

        public static void ProjectLevel(int totalXp, ExperienceCurveSettings curve, out int level, out int remainder, out int nextTarget)
        {
            level = 1;
            remainder = Mathf.Max(0, totalXp);
            nextTarget = ExperienceRules.ExperienceRequiredForNextLevel(level, curve);
            while (remainder >= nextTarget)
            {
                remainder -= nextTarget;
                level++;
                nextTarget = ExperienceRules.ExperienceRequiredForNextLevel(level, curve);
            }
        }

        private static float WeightedXpPerKill(IReadOnlyList<XpPacingEnemySource> sources, List<string> warnings)
        {
            if (sources == null || sources.Count == 0)
            {
                return 0f;
            }

            float totalWeight = 0f;
            float weightedXp = 0f;
            for (int i = 0; i < sources.Count; i++)
            {
                XpPacingEnemySource source = sources[i];
                totalWeight += source.SpawnWeight;
                weightedXp += source.SpawnWeight * source.ResolvedXpPerKill;
            }

            if (totalWeight <= 0f)
            {
                warnings?.Add("Enemy spawn weights sum to 0; projected XP per kill is 0.");
                return 0f;
            }

            return weightedXp / totalWeight;
        }
    }
}
