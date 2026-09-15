using UnityEngine;

namespace TheCircussyOne.Rules
{
    public static class EnemyPileClimbRules
    {
        public static int LayerCount(int localEnemyCount, int startCount, int enemiesPerLayer, int maxLayers)
        {
            if (localEnemyCount < Mathf.Max(1, startCount))
            {
                return 0;
            }

            int safePerLayer = Mathf.Max(1, enemiesPerLayer);
            int layers = 1 + (localEnemyCount - Mathf.Max(1, startCount)) / safePerLayer;
            return Mathf.Clamp(layers, 0, Mathf.Max(0, maxLayers));
        }

        public static float MoundHeight(
            int localEnemyCount,
            float distanceFromPileCenter,
            float pileRadius,
            float movementBodyHeight,
            float layerHeightMultiplier,
            int startCount,
            int enemiesPerLayer,
            int maxLayers)
        {
            if (pileRadius <= EnemyMovementRules.MinimumMoveDistance)
            {
                return 0f;
            }

            int layers = LayerCount(localEnemyCount, startCount, enemiesPerLayer, maxLayers);
            if (layers <= 0)
            {
                return 0f;
            }

            float centerWeight = 1f - Mathf.Clamp01(distanceFromPileCenter / pileRadius);
            return layers * Mathf.Max(0f, movementBodyHeight) * Mathf.Max(0f, layerHeightMultiplier) * centerWeight;
        }
    }
}
