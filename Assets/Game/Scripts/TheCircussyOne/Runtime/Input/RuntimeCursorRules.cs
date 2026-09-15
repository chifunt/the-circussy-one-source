using UnityEngine;

namespace TheCircussyOne.Runtime
{
    public readonly struct RuntimeCursorPolicy
    {
        public RuntimeCursorPolicy(CursorLockMode lockMode, bool visible, bool transparentCursor)
        {
            LockMode = lockMode;
            Visible = visible;
            TransparentCursor = transparentCursor;
        }

        public CursorLockMode LockMode { get; }
        public bool Visible { get; }
        public bool TransparentCursor { get; }
    }

    public static class RuntimeCursorRules
    {
        public static RuntimeCursorPolicy Evaluate(bool menuOpen, ControlInputMode inputMode)
        {
            if (!menuOpen)
            {
                return new RuntimeCursorPolicy(CursorLockMode.Locked, false, true);
            }

            if (inputMode == ControlInputMode.Gamepad)
            {
                return new RuntimeCursorPolicy(CursorLockMode.None, false, true);
            }

            return new RuntimeCursorPolicy(CursorLockMode.None, true, false);
        }
    }
}
