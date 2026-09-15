using UnityEngine;

namespace TheCircussyOne.Rules
{
    public static class PickupRules
    {
        public const float MinimumMagnetDistance = 0.05f;

        public static Vector3 HorizontalOffsetToPlayer(Vector3 pickupPosition, Vector3 playerPosition)
        {
            Vector3 toPlayer = playerPosition - pickupPosition;
            toPlayer.y = 0f;
            return toPlayer;
        }

        public static Vector3 OffsetToPlayer(Vector3 pickupPosition, Vector3 playerPosition)
        {
            return playerPosition - pickupPosition;
        }

        public static bool ShouldCollect(float distance, float collectRadius)
        {
            return distance <= collectRadius;
        }

        public static bool ShouldMagnet(float distance, float magnetRadius)
        {
            return distance <= magnetRadius && distance > MinimumMagnetDistance;
        }
    }
}
