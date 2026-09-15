namespace TheCircussyOne.Runtime
{
    public readonly struct ControlInputActivity
    {
        public ControlInputActivity(bool mouseKeyboardActive, bool gamepadActive)
        {
            MouseKeyboardActive = mouseKeyboardActive;
            GamepadActive = gamepadActive;
        }

        public bool MouseKeyboardActive { get; }
        public bool GamepadActive { get; }
    }

    public static class ControlInputModeRules
    {
        public static ControlInputMode Resolve(ControlInputMode currentMode, ControlInputActivity activity)
        {
            if (activity.MouseKeyboardActive)
            {
                return ControlInputMode.MouseKeyboard;
            }

            if (activity.GamepadActive)
            {
                return ControlInputMode.Gamepad;
            }

            return currentMode;
        }
    }
}
