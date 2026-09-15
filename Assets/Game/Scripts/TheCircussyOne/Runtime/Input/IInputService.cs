using UnityEngine;

namespace TheCircussyOne.Runtime
{
    public enum LookInputKind
    {
        None,
        PointerDelta,
        Stick
    }

    public enum UpgradeNavigationDirection
    {
        None,
        Left,
        Middle,
        Right
    }

    public enum UiCycleNavigationDirection
    {
        None,
        Previous,
        Next
    }

    public interface IInputService
    {
        Vector2 Movement { get; }
        Vector2 Look { get; }
        LookInputKind LookKind { get; }
        float CameraOrbitTriggerAxis { get; }
        bool JumpPressedThisFrame { get; }
        bool JumpHeld { get; }
        UpgradeNavigationDirection UpgradeNavigation { get; }
        UiCycleNavigationDirection UiCycleNavigation { get; }
        int UpgradeChoicePressedThisFrame { get; }
        int PerformerChoicePressedThisFrame { get; }
        bool SubmitPressedThisFrame { get; }
        bool PausePressedThisFrame { get; }
        bool InteractPressedThisFrame { get; }
        bool InteractHeld { get; }
        bool InteractReleasedThisFrame { get; }
        bool DebugRestartPressedThisFrame { get; }
    }
}
