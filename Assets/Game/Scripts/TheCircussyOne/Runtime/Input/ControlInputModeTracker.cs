using UnityEngine;
using UnityEngine.InputSystem;
using VContainer.Unity;

namespace TheCircussyOne.Runtime
{
    public sealed class ControlInputModeTracker : ITickable, ITickableWhenPaused
    {
        private const float PointerDeltaThreshold = 0.01f;
        private const float ScrollThreshold = 0.01f;
        private const float StickThreshold = 0.35f;
        private const float TriggerThreshold = 0.2f;

        public ControlInputMode CurrentMode { get; private set; } = ControlInputMode.MouseKeyboard;

        public void Tick()
        {
            CurrentMode = ControlInputModeRules.Resolve(CurrentMode, ReadActivity());
        }

        private static ControlInputActivity ReadActivity()
        {
            bool mouseKeyboard = ReadMouseActivity() || ReadKeyboardActivity();
            bool gamepad = ReadGamepadActivity();
            return new ControlInputActivity(mouseKeyboard, gamepad);
        }

        private static bool ReadMouseActivity()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null)
            {
                return false;
            }

            return mouse.delta.ReadValue().sqrMagnitude > PointerDeltaThreshold * PointerDeltaThreshold
                || Mathf.Abs(mouse.scroll.ReadValue().y) > ScrollThreshold
                || mouse.leftButton.wasPressedThisFrame
                || mouse.rightButton.wasPressedThisFrame
                || mouse.middleButton.wasPressedThisFrame;
        }

        private static bool ReadKeyboardActivity()
        {
            Keyboard keyboard = Keyboard.current;
            return keyboard != null && keyboard.anyKey.wasPressedThisFrame;
        }

        private static bool ReadGamepadActivity()
        {
            Gamepad gamepad = Gamepad.current;
            if (gamepad == null)
            {
                return false;
            }

            return gamepad.leftStick.ReadValue().sqrMagnitude > StickThreshold * StickThreshold
                || gamepad.rightStick.ReadValue().sqrMagnitude > StickThreshold * StickThreshold
                || gamepad.dpad.ReadValue().sqrMagnitude > 0f
                || gamepad.buttonSouth.wasPressedThisFrame
                || gamepad.buttonNorth.wasPressedThisFrame
                || gamepad.buttonEast.wasPressedThisFrame
                || gamepad.buttonWest.wasPressedThisFrame
                || gamepad.leftShoulder.wasPressedThisFrame
                || gamepad.rightShoulder.wasPressedThisFrame
                || gamepad.startButton.wasPressedThisFrame
                || gamepad.selectButton.wasPressedThisFrame
                || gamepad.leftTrigger.ReadValue() > TriggerThreshold
                || gamepad.rightTrigger.ReadValue() > TriggerThreshold;
        }
    }
}
