using UnityEngine;

namespace TheCircussyOne.Runtime
{
    public sealed class CursorLockBehaviour : MonoBehaviour
    {
        private void OnDisable()
        {
            RuntimeCursorSystem.ReleaseCursor();
        }

        private void OnApplicationQuit()
        {
            RuntimeCursorSystem.ReleaseCursor();
        }
    }
}
