using UnityEngine;
using UnityEngine.InputSystem;

namespace TheCircussyOne.Runtime
{
    public sealed class UnityInputService : IInputService
    {
        private const float UpgradeNavigationDeadzone = 0.55f;
        private const float TriggerCyclePressThreshold = 0.55f;
        private const float TriggerCycleReleaseThreshold = 0.25f;

        private int uiCycleFrame = -1;
        private bool leftTriggerCycleHeld;
        private bool rightTriggerCycleHeld;
        private UiCycleNavigationDirection cachedUiCycleNavigation;

        public Vector2 Movement
        {
            get
            {
                Vector2 input = Vector2.zero;
                if (Gamepad.current != null)
                {
                    input += Gamepad.current.leftStick.ReadValue();
                }

                if (Keyboard.current != null)
                {
                    if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) input.y += 1f;
                    if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) input.y -= 1f;
                    if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) input.x += 1f;
                    if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) input.x -= 1f;
                }

                if (input.sqrMagnitude > 1f)
                {
                    input.Normalize();
                }

                return input;
            }
        }

        public Vector2 Look
        {
            get
            {
                Vector2 pointerDelta = Mouse.current != null ? Mouse.current.delta.ReadValue() : Vector2.zero;
                if (pointerDelta.sqrMagnitude > 0f)
                {
                    return pointerDelta;
                }

                return Gamepad.current != null ? Gamepad.current.rightStick.ReadValue() : Vector2.zero;
            }
        }

        public LookInputKind LookKind
        {
            get
            {
                if (Mouse.current != null && Mouse.current.delta.ReadValue().sqrMagnitude > 0f)
                {
                    return LookInputKind.PointerDelta;
                }

                if (Gamepad.current != null && Gamepad.current.rightStick.ReadValue().sqrMagnitude > 0f)
                {
                    return LookInputKind.Stick;
                }

                return LookInputKind.None;
            }
        }

        public float CameraOrbitTriggerAxis
        {
            get
            {
                if (Gamepad.current == null)
                {
                    return 0f;
                }

                return Mathf.Clamp(
                    Gamepad.current.rightTrigger.ReadValue() - Gamepad.current.leftTrigger.ReadValue(),
                    -1f,
                    1f);
            }
        }

        public bool JumpPressedThisFrame
        {
            get
            {
                bool keyboard = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
                bool gamepad = Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame;
                return keyboard || gamepad;
            }
        }

        public bool JumpHeld
        {
            get
            {
                bool keyboard = Keyboard.current != null && Keyboard.current.spaceKey.isPressed;
                bool gamepad = Gamepad.current != null && Gamepad.current.buttonSouth.isPressed;
                return keyboard || gamepad;
            }
        }

        public UpgradeNavigationDirection UpgradeNavigation
        {
            get
            {
                if (Gamepad.current != null)
                {
                    UpgradeNavigationDirection dpad = DirectionFromVector(Gamepad.current.dpad.ReadValue());
                    if (dpad != UpgradeNavigationDirection.None)
                    {
                        return dpad;
                    }

                    UpgradeNavigationDirection leftStick = DirectionFromVector(Gamepad.current.leftStick.ReadValue());
                    if (leftStick != UpgradeNavigationDirection.None)
                    {
                        return leftStick;
                    }

                    UpgradeNavigationDirection rightStick = DirectionFromVector(Gamepad.current.rightStick.ReadValue());
                    if (rightStick != UpgradeNavigationDirection.None)
                    {
                        return rightStick;
                    }
                }

                if (Keyboard.current != null)
                {
                    Vector2 keyboard = Vector2.zero;
                    if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) keyboard.x -= 1f;
                    if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) keyboard.x += 1f;
                    if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) keyboard.y += 1f;
                    if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) keyboard.y -= 1f;
                    return DirectionFromVector(keyboard);
                }

                return UpgradeNavigationDirection.None;
            }
        }

        public UiCycleNavigationDirection UiCycleNavigation
        {
            get
            {
                int frame = Time.frameCount;
                if (uiCycleFrame == frame)
                {
                    return cachedUiCycleNavigation;
                }

                uiCycleFrame = frame;
                cachedUiCycleNavigation = ReadUiCycleNavigation();
                return cachedUiCycleNavigation;
            }
        }

        public int UpgradeChoicePressedThisFrame
        {
            get
            {
                if (Keyboard.current == null)
                {
                    return 0;
                }

                if (Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame) return 1;
                if (Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame) return 2;
                if (Keyboard.current.digit3Key.wasPressedThisFrame || Keyboard.current.numpad3Key.wasPressedThisFrame) return 3;
                return 0;
            }
        }

        public int PerformerChoicePressedThisFrame
        {
            get
            {
                int upgradeChoice = UpgradeChoicePressedThisFrame;
                if (upgradeChoice > 0)
                {
                    return upgradeChoice;
                }

                if (Keyboard.current != null && (Keyboard.current.digit4Key.wasPressedThisFrame || Keyboard.current.numpad4Key.wasPressedThisFrame))
                {
                    return 4;
                }

                if (Keyboard.current != null && (Keyboard.current.digit5Key.wasPressedThisFrame || Keyboard.current.numpad5Key.wasPressedThisFrame))
                {
                    return 5;
                }

                if (Keyboard.current != null && (Keyboard.current.digit6Key.wasPressedThisFrame || Keyboard.current.numpad6Key.wasPressedThisFrame))
                {
                    return 6;
                }

                if (Keyboard.current != null && (Keyboard.current.digit7Key.wasPressedThisFrame || Keyboard.current.numpad7Key.wasPressedThisFrame))
                {
                    return 7;
                }

                return 0;
            }
        }

        public bool SubmitPressedThisFrame
        {
            get
            {
                bool keyboard = Keyboard.current != null && (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame);
                bool gamepad = Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame;
                return keyboard || gamepad;
            }
        }

        public bool PausePressedThisFrame
        {
            get
            {
                bool keyboard = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
                bool gamepad = Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame;
                return keyboard || gamepad;
            }
        }

        public bool InteractPressedThisFrame
        {
            get
            {
                bool keyboard = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
                bool gamepad = Gamepad.current != null &&
                    Gamepad.current.buttonWest.wasPressedThisFrame;
                return keyboard || gamepad;
            }
        }

        public bool InteractHeld
        {
            get
            {
                bool keyboard = Keyboard.current != null && Keyboard.current.eKey.isPressed;
                bool gamepad = Gamepad.current != null &&
                    Gamepad.current.buttonWest.isPressed;
                return keyboard || gamepad;
            }
        }

        public bool InteractReleasedThisFrame
        {
            get
            {
                bool keyboard = Keyboard.current != null && Keyboard.current.eKey.wasReleasedThisFrame;
                bool gamepad = Gamepad.current != null &&
                    Gamepad.current.buttonWest.wasReleasedThisFrame;
                return keyboard || gamepad;
            }
        }

        public bool DebugRestartPressedThisFrame
        {
            get
            {
#if UNITY_EDITOR
                return Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame;
#else
                return false;
#endif
            }
        }

        private static UpgradeNavigationDirection DirectionFromVector(Vector2 value)
        {
            if (value.sqrMagnitude < UpgradeNavigationDeadzone * UpgradeNavigationDeadzone)
            {
                return UpgradeNavigationDirection.None;
            }

            if (Mathf.Abs(value.y) > Mathf.Abs(value.x))
            {
                return UpgradeNavigationDirection.Middle;
            }

            return value.x < 0f ? UpgradeNavigationDirection.Left : UpgradeNavigationDirection.Right;
        }

        private UiCycleNavigationDirection ReadUiCycleNavigation()
        {
            if (Gamepad.current == null)
            {
                leftTriggerCycleHeld = false;
                rightTriggerCycleHeld = false;
                return UiCycleNavigationDirection.None;
            }

            float left = Gamepad.current.leftTrigger.ReadValue();
            float right = Gamepad.current.rightTrigger.ReadValue();
            leftTriggerCycleHeld = UpdateTriggerHeld(leftTriggerCycleHeld, left);
            rightTriggerCycleHeld = UpdateTriggerHeld(rightTriggerCycleHeld, right);
            if (!leftTriggerCycleHeld && !rightTriggerCycleHeld)
            {
                return UiCycleNavigationDirection.None;
            }

            if (leftTriggerCycleHeld && rightTriggerCycleHeld)
            {
                if (Mathf.Abs(right - left) < 0.05f)
                {
                    return UiCycleNavigationDirection.None;
                }

                return right > left ? UiCycleNavigationDirection.Next : UiCycleNavigationDirection.Previous;
            }

            return rightTriggerCycleHeld ? UiCycleNavigationDirection.Next : UiCycleNavigationDirection.Previous;
        }

        private static bool UpdateTriggerHeld(bool wasHeld, float value)
        {
            return wasHeld
                ? value > TriggerCycleReleaseThreshold
                : value >= TriggerCyclePressThreshold;
        }
    }
}
