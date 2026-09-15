using System;
using UnityEngine;

namespace TheCircussyOne.Runtime
{
    public static class GameLayers
    {
        public const string Player = "Player";
        public const string Enemy = "Enemy";
        public const string Projectile = "Projectile";
        public const string Pickup = "Pickup";

        public static int PlayerIndex => LayerMask.NameToLayer(Player);
        public static int EnemyIndex => LayerMask.NameToLayer(Enemy);
        public static int ProjectileIndex => LayerMask.NameToLayer(Projectile);
        public static int PickupIndex => LayerMask.NameToLayer(Pickup);

        public static int EnemyMask => LayerMask.GetMask(Enemy);
        public static int EnvironmentMaskExcludingGameplay => ~LayerMask.GetMask(Player, Enemy, Projectile, Pickup);

        public static int RequireLayer(string layerName)
        {
            int layer = LayerMask.NameToLayer(layerName);
            if (layer < 0)
            {
                throw new InvalidOperationException($"Layer '{layerName}' is not configured.");
            }

            return layer;
        }
    }
}
