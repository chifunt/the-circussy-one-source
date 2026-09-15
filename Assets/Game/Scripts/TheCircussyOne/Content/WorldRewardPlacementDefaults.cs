using UnityEngine;

namespace TheCircussyOne.Content
{
    public static class WorldRewardPlacementDefaults
    {
        public static WorldRewardPlacementDefinition[] CreatePrototypePlacements()
        {
            return new[]
            {
                WorldRewardPlacementDefinition.Create(
                    "open_chest_northwest",
                    "Open Chest Northwest",
                    WorldRewardPlacementKind.Chest,
                    "open_chest",
                    new Vector3(-14f, 0.5f, 30f)),
                WorldRewardPlacementDefinition.Create(
                    "locked_chest_east",
                    "Locked Chest East",
                    WorldRewardPlacementKind.Chest,
                    "locked_chest",
                    new Vector3(30f, 0.5f, -4f)),
                WorldRewardPlacementDefinition.Create(
                    "premium_chest_southwest",
                    "Premium Chest Southwest",
                    WorldRewardPlacementKind.Chest,
                    "premium_chest",
                    new Vector3(-30f, 0.5f, -23f)),
                WorldRewardPlacementDefinition.Create(
                    "open_chest_northeast",
                    "Open Chest Northeast",
                    WorldRewardPlacementKind.Chest,
                    "open_chest",
                    new Vector3(16f, 0.5f, 22f)),
                WorldRewardPlacementDefinition.Create(
                    "small_ticket_stack_northwest",
                    "Small Ticket Stack Northwest",
                    WorldRewardPlacementKind.TicketDeposit,
                    "small_ticket_stack",
                    new Vector3(-18f, 0.35f, 14f)),
                WorldRewardPlacementDefinition.Create(
                    "ticket_roll_east",
                    "Ticket Roll East",
                    WorldRewardPlacementKind.TicketDeposit,
                    "ticket_roll",
                    new Vector3(24f, 0.35f, -4f)),
                WorldRewardPlacementDefinition.Create(
                    "jackpot_cache_southwest",
                    "Jackpot Cache Southwest",
                    WorldRewardPlacementKind.TicketDeposit,
                    "jackpot_cache",
                    new Vector3(-30f, 0.35f, -18f)),
                WorldRewardPlacementDefinition.Create(
                    "small_ticket_stack_southeast",
                    "Small Ticket Stack Southeast",
                    WorldRewardPlacementKind.TicketDeposit,
                    "small_ticket_stack",
                    new Vector3(18f, 0.35f, -28f)),
                WorldRewardPlacementDefinition.Create(
                    "ticket_roll_northeast",
                    "Ticket Roll Northeast",
                    WorldRewardPlacementKind.TicketDeposit,
                    "ticket_roll",
                    new Vector3(18f, 0.35f, 16f)),
                WorldRewardPlacementDefinition.Create(
                    "snack_box_west",
                    "Snack Box West",
                    WorldRewardPlacementKind.HealingProp,
                    "snack_box",
                    new Vector3(-18f, 0.3f, 6f)),
                WorldRewardPlacementDefinition.Create(
                    "snack_cart_northeast",
                    "Snack Cart Northeast",
                    WorldRewardPlacementKind.HealingProp,
                    "snack_cart",
                    new Vector3(21f, 0.3f, 16f)),
                WorldRewardPlacementDefinition.Create(
                    "snack_box_south",
                    "Snack Box South",
                    WorldRewardPlacementKind.HealingProp,
                    "snack_box",
                    new Vector3(9f, 0.3f, -24f))
            };
        }
    }
}
