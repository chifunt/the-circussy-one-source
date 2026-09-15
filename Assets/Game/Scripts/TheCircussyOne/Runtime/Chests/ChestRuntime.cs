using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using TheCircussyOne.Stats;
using UnityEngine;

namespace TheCircussyOne.Runtime
{
    public sealed class ChestRuntime
    {
        private readonly ChestDefinition definition;
        private readonly ItemCatalog itemCatalog;
        private readonly ItemInventory inventory;
        private readonly RunStats stats;
        private readonly Vector3 position;
        private readonly RunSeedState runSeedState;
        private float elapsedSeconds;

        public ChestRuntime(
            ChestDefinition definition,
            ItemCatalog itemCatalog,
            ItemInventory inventory,
            RunStats stats,
            Vector3 position,
            RunSeedState runSeedState = null)
        {
            this.definition = definition;
            this.itemCatalog = itemCatalog;
            this.inventory = inventory;
            this.stats = stats;
            this.position = position;
            this.runSeedState = runSeedState;
        }

        public bool Completed { get; private set; }
        public float Progress => definition == null ? 0f : Mathf.Clamp01(elapsedSeconds / Mathf.Max(0.001f, definition.holdSeconds));

        public bool CanPay(RunCurrencyState currency)
        {
            return definition != null && (currency?.CanSpendTickets(definition.TicketCost) ?? definition.TicketCost <= 0);
        }

        public bool Tick(float deltaTime, RunCurrencyState currency)
        {
            if (definition == null || Completed || !CanPay(currency))
            {
                return false;
            }

            elapsedSeconds = Mathf.Min(definition.holdSeconds, elapsedSeconds + Mathf.Max(0f, deltaTime));
            if (elapsedSeconds < definition.holdSeconds)
            {
                return false;
            }

            return TryOpen(currency);
        }

        public void Cancel()
        {
            if (!Completed)
            {
                elapsedSeconds = 0f;
            }
        }

        private bool TryOpen(RunCurrencyState currency)
        {
            ChestRewardRoll reward = ChestRewardRules.Roll(definition, itemCatalog, inventory, stats, ResolveSeed());
            if (!reward.HasReward)
            {
                elapsedSeconds = 0f;
                return false;
            }

            int cost = definition.TicketCost;
            if (currency != null && !currency.TrySpendTickets(cost))
            {
                elapsedSeconds = 0f;
                return false;
            }

            int added = inventory?.AddItem(reward.Item, 1, ItemGrantSource.Chest) ?? 0;
            if (added <= 0)
            {
                currency?.AddTickets(cost);
                elapsedSeconds = 0f;
                return false;
            }

            Completed = true;
            return true;
        }

        private int ResolveSeed()
        {
            return ChestRewardRules.StableSeed(
                definition != null ? definition.Id : string.Empty,
                position,
                runSeedState != null ? runSeedState.CurrentSeed : 0);
        }
    }
}
