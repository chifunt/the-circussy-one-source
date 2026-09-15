using System;
using TheCircussyOne.Visuals;
using UnityEngine;
using VContainer.Unity;

namespace TheCircussyOne.Runtime
{
    public sealed class RuntimeCursorSystem : IStartable, ITickable, ITickableWhenPaused, IDisposable
    {
        private static Texture2D transparentCursor;

        private readonly ControlInputModeTracker inputModeTracker;
        private readonly GameState state;
        private readonly UpgradeSelectionView upgradeSelection;
        private readonly PerformerSelectionView performerSelection;
        private readonly RewardRevealView rewardReveal;
        private readonly PauseMenuView pauseMenu;
        private readonly HudView hud;

        public RuntimeCursorSystem(
            ControlInputModeTracker inputModeTracker,
            GameState state,
            UpgradeSelectionView upgradeSelection = null,
            PerformerSelectionView performerSelection = null,
            RewardRevealView rewardReveal = null,
            PauseMenuView pauseMenu = null,
            HudView hud = null)
        {
            this.inputModeTracker = inputModeTracker;
            this.state = state;
            this.upgradeSelection = upgradeSelection;
            this.performerSelection = performerSelection;
            this.rewardReveal = rewardReveal;
            this.pauseMenu = pauseMenu;
            this.hud = hud;
        }

        public void Start()
        {
            ApplyCurrentPolicy();
        }

        public void Tick()
        {
            ApplyCurrentPolicy();
        }

        public void Dispose()
        {
            ReleaseCursor();
        }

        public static void ReleaseCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }

        private void ApplyCurrentPolicy()
        {
            RuntimeCursorPolicy policy = RuntimeCursorRules.Evaluate(IsMenuOpen(), inputModeTracker?.CurrentMode ?? ControlInputMode.MouseKeyboard);
            ApplyPolicy(policy);
        }

        private bool IsMenuOpen()
        {
            return state != null && state.IsGameOver
                || upgradeSelection != null && upgradeSelection.IsVisible
                || performerSelection != null && performerSelection.IsVisible
                || rewardReveal != null && rewardReveal.IsVisible
                || pauseMenu != null && pauseMenu.IsVisible
                || hud != null && hud.IsTerminalOverlayVisible;
        }

        private static void ApplyPolicy(RuntimeCursorPolicy policy)
        {
            Cursor.lockState = policy.LockMode;
            Cursor.visible = policy.Visible;
            Cursor.SetCursor(policy.TransparentCursor ? GetTransparentCursor() : null, Vector2.zero, policy.TransparentCursor ? CursorMode.ForceSoftware : CursorMode.Auto);
        }

        private static Texture2D GetTransparentCursor()
        {
            if (transparentCursor != null)
            {
                return transparentCursor;
            }

            transparentCursor = new Texture2D(1, 1, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            transparentCursor.SetPixel(0, 0, new Color(0f, 0f, 0f, 0f));
            transparentCursor.Apply(updateMipmaps: false, makeNoLongerReadable: false);
            return transparentCursor;
        }
    }
}
