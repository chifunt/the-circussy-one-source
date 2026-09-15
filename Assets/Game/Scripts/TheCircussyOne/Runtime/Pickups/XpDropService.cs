using System.Collections.Generic;
using UnityEngine;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;

namespace TheCircussyOne.Runtime
{
    public sealed class XpDropService
    {
        private readonly XpGemCatalog catalog;
        private readonly PickupFactory pickupFactory;
        private readonly RunSeedState runSeedState;

        public XpDropService(XpGemCatalog catalog, PickupFactory pickupFactory, RunSeedState runSeedState = null)
        {
            this.catalog = catalog;
            this.pickupFactory = pickupFactory;
            this.runSeedState = runSeedState;
        }

        public int SpawnEnemyDrops(Vector3 position, int seed = 0)
        {
            return SpawnDrops(
                catalog != null ? catalog.defaultEnemyXpBudget : XpGemDefinition.DefaultBlueXpAmount,
                catalog != null ? catalog.defaultDropStyle : XpDropStyle.Compact,
                position,
                ResolveDropSeed(null, position, seed));
        }

        public int SpawnEnemyDrops(EnemyRuntime enemy, Vector3 position, int seed = 0)
        {
            int budget = enemy != null ? enemy.XpBudget : catalog != null ? catalog.defaultEnemyXpBudget : XpGemDefinition.DefaultBlueXpAmount;
            XpDropStyle style = enemy != null ? enemy.DropStyle : catalog != null ? catalog.defaultDropStyle : XpDropStyle.Compact;
            return SpawnDrops(budget, style, position, ResolveDropSeed(enemy, position, seed));
        }

        private int SpawnDrops(int budget, XpDropStyle style, Vector3 position, int seed)
        {
            if (pickupFactory == null)
            {
                return 0;
            }

            if (catalog == null)
            {
                int count = budget > 0 ? 1 : 0;
                for (int i = 0; i < count; i++)
                {
                    pickupFactory.SpawnDrop((XpGemDefinition)null, position, i, count, seed);
                }

                return count;
            }

            List<XpGemDrop> drops = XpDropRules.BuildDrops(
                budget,
                catalog.Definitions,
                style,
                seed);

            if (drops.Count == 0)
            {
                return 0;
            }

            for (int i = 0; i < drops.Count; i++)
            {
                pickupFactory.SpawnDrop(drops[i].Gem, position, i, drops.Count, seed);
            }

            return drops.Count;
        }

        private int ResolveDropSeed(EnemyRuntime enemy, Vector3 position, int eventSeed)
        {
            return DeterministicSeed.Combine(
                runSeedState != null ? runSeedState.CurrentSeed : 0,
                eventSeed,
                enemy != null ? enemy.SpawnId : 0,
                DeterministicSeed.ContentPosition(enemy?.Definition != null ? enemy.Definition.Id : string.Empty, position));
        }
    }
}
