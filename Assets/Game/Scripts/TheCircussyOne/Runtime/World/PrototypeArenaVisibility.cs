using System.Collections.Generic;
using UnityEngine;

namespace TheCircussyOne.Runtime
{
    public static class PrototypeArenaVisibility
    {
        private const string ArenaFloorName = "Arena Floor";
        private const string ArenaTerrainName = "Arena Terrain";

        private static readonly List<GameObject> hiddenObjects = new();

        public static void SetVisible(bool visible)
        {
            if (visible)
            {
                RestoreHiddenObjects();
                return;
            }

            HideIfFound(ArenaFloorName);
            HideIfFound(ArenaTerrainName);
        }

        private static void HideIfFound(string name)
        {
            GameObject gameObject = GameObject.Find(name);
            if (gameObject == null || !gameObject.activeSelf)
            {
                return;
            }

            if (!hiddenObjects.Contains(gameObject))
            {
                hiddenObjects.Add(gameObject);
            }

            gameObject.SetActive(false);
        }

        private static void RestoreHiddenObjects()
        {
            for (int i = hiddenObjects.Count - 1; i >= 0; i--)
            {
                GameObject gameObject = hiddenObjects[i];
                if (gameObject != null)
                {
                    gameObject.SetActive(true);
                }
            }

            hiddenObjects.Clear();
        }
    }
}
