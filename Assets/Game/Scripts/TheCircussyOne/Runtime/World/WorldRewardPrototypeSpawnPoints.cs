using System.Collections.Generic;
using UnityEngine;

namespace TheCircussyOne.Runtime
{
    public static class WorldRewardPrototypeSpawnPoints
    {
        private static readonly Vector3[] ChestOffsets =
        {
            new(-14f, 0.5f, 30f),
            new(30f, 0.5f, -4f),
            new(-30f, 0.5f, -23f),
            new(15.5f, 0.5f, -12f)
        };

        private static readonly Vector3[] TicketDepositOffsets =
        {
            new(-18f, 0.35f, 14f),
            new(24f, 0.35f, -4f),
            new(-30f, 0.35f, -18f),
            new(18f, 0.35f, -28f),
            new(18f, 0.35f, 16f)
        };

        private static readonly Vector3[] HealingPropOffsets =
        {
            new(-18f, 0.3f, 6f),
            new(21f, 0.3f, 16f),
            new(9f, 0.3f, -24f)
        };

        public static IReadOnlyList<Vector3> Chests => ChestOffsets;
        public static IReadOnlyList<Vector3> TicketDeposits => TicketDepositOffsets;
        public static IReadOnlyList<Vector3> HealingProps => HealingPropOffsets;
    }
}
