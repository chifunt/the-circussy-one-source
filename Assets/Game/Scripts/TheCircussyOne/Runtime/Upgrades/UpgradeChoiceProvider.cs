using System.Collections.Generic;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using TheCircussyOne.Stats;

namespace TheCircussyOne.Runtime
{
    public sealed class UpgradeChoiceProvider
    {
        private readonly UpgradeCatalog catalog;
        private readonly WeaponCatalog weaponCatalog;
        private readonly TalentCatalog talentCatalog;
        private readonly UpgradeRunState runState;
        private readonly TalentRunState talentRunState;
        private readonly PerformerRunState performerRunState;
        private readonly RunStats stats;
        private readonly WeaponLoadout weaponLoadout;
        private readonly ItemInventory itemInventory;
        private readonly RunSeedState runSeedState;

        public UpgradeChoiceProvider(
            UpgradeCatalog catalog,
            UpgradeRunState runState,
            RunStats stats,
            WeaponLoadout weaponLoadout,
            ItemInventory itemInventory = null,
            WeaponCatalog weaponCatalog = null,
            TalentCatalog talentCatalog = null,
            TalentRunState talentRunState = null,
            PerformerRunState performerRunState = null,
            RunSeedState runSeedState = null)
        {
            this.catalog = catalog;
            this.weaponCatalog = weaponCatalog;
            this.talentCatalog = talentCatalog;
            this.runState = runState;
            this.talentRunState = talentRunState;
            this.performerRunState = performerRunState;
            this.stats = stats;
            this.weaponLoadout = weaponLoadout;
            this.itemInventory = itemInventory;
            this.runSeedState = runSeedState;
        }

        public List<UpgradeChoice> BuildChoices(int level, int pendingCount, int choiceCount = 3)
        {
            float luck = stats != null ? stats.GetFloat(StatId.Luck) : 100f;
            return UpgradeSelectionRules.BuildChoices(
                catalog,
                runState,
                seed: ResolveSeed(level, pendingCount),
                choiceCount: choiceCount,
                luck: luck,
                weaponLoadout: weaponLoadout,
                itemInventory: itemInventory,
                weaponCatalog: weaponCatalog,
                talentCatalog: talentCatalog,
                talentRunState: talentRunState,
                selectedPerformer: performerRunState?.SelectedPerformer);
        }

        private int ResolveSeed(int level, int pendingCount)
        {
            if (runSeedState == null)
            {
                return level * 7919 + pendingCount;
            }

            return DeterministicSeed.Combine(runSeedState.CurrentSeed, level, pendingCount);
        }
    }
}
