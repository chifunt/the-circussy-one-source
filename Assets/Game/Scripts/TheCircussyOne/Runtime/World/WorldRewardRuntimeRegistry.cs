using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TheCircussyOne.Runtime
{
    public sealed class WorldRewardRuntimeRegistry
    {
        private readonly List<GameObject> roots = new();
        private readonly WorldRewardRuntimeObjectFactory objectFactory;

        public WorldRewardRuntimeRegistry(WorldRewardRuntimeObjectFactory objectFactory = null)
        {
            this.objectFactory = objectFactory ?? new WorldRewardRuntimeObjectFactory();
        }

        public int Count
        {
            get
            {
                PruneDestroyed();
                return roots.Count;
            }
        }

        public void Register(GameObject root)
        {
            if (root == null || roots.Contains(root))
            {
                return;
            }

            roots.Add(root);
        }

        public void ClearAll()
        {
            for (int i = roots.Count - 1; i >= 0; i--)
            {
                GameObject root = roots[i];
                if (root == null)
                {
                    continue;
                }

                objectFactory.Release(root);
            }

            roots.Clear();
        }

        public async UniTask ClearAllAsync(int objectsPerFrame)
        {
            int batchSize = Mathf.Max(1, objectsPerFrame);
            int released = 0;
            for (int i = roots.Count - 1; i >= 0; i--)
            {
                GameObject root = roots[i];
                if (root != null)
                {
                    objectFactory.Release(root);
                    released++;
                }

                if (released > 0 && released % batchSize == 0)
                {
                    await UniTask.Yield(PlayerLoopTiming.Update);
                }
            }

            roots.Clear();
        }

        private void PruneDestroyed()
        {
            for (int i = roots.Count - 1; i >= 0; i--)
            {
                if (roots[i] == null)
                {
                    roots.RemoveAt(i);
                }
            }
        }
    }
}
