using UnityEngine;

namespace TheCircussyOne.Runtime
{
    public static class RuntimeObjectFactory
    {
        public static GameObject Instantiate(GameObject prefab, Transform parent)
        {
            return prefab != null ? Object.Instantiate(prefab, parent) : null;
        }

        public static void Release(Object target, float delaySeconds = 0f)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Object.Destroy(target, Mathf.Max(0f, delaySeconds));
            }
            else
            {
                Object.DestroyImmediate(target);
            }
        }

        public static void ReleaseImmediately(Object target)
        {
            if (target != null)
            {
                Object.DestroyImmediate(target);
            }
        }
    }
}
