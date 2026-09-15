using UnityEngine;

namespace TheCircussyOne.Runtime
{
    public sealed class WorldRewardRuntimeObjectFactory
    {
        public static void ReleaseRoot(GameObject root)
        {
            if (root == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Object.Destroy(root);
            }
            else
            {
                Object.DestroyImmediate(root);
            }
        }

        public void Release(GameObject root)
        {
            ReleaseRoot(root);
        }
    }
}
