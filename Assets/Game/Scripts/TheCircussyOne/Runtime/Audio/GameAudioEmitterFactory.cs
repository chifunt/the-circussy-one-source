using UnityEngine;

namespace TheCircussyOne.Runtime
{
    internal static class GameAudioEmitterFactory
    {
        public static GameObject Create(string name)
        {
            return new GameObject(name)
            {
                hideFlags = HideFlags.DontSave
            };
        }

        public static GameObject CreateChild(string name, Transform parent, Vector3 worldPosition)
        {
            GameObject emitter = Create(name);
            emitter.transform.SetParent(parent, worldPositionStays: true);
            emitter.transform.position = worldPosition;
            return emitter;
        }

        public static void Release(GameObject emitter, float delaySeconds = 0f)
        {
            RuntimeObjectFactory.Release(emitter, delaySeconds);
        }
    }
}
