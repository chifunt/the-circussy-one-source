using System;
using TheCircussyOne.Config;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheCircussyOne.Visuals
{
    internal readonly struct HudGameOverElements
    {
        public HudGameOverElements(VisualElement panel, Label titleLabel, Label subtitleLabel)
        {
            Panel = panel;
            TitleLabel = titleLabel;
            SubtitleLabel = subtitleLabel;
        }

        public VisualElement Panel { get; }
        public Label TitleLabel { get; }
        public Label SubtitleLabel { get; }
    }

    internal sealed class HudGameOverDriver
    {
        private Button registeredRestartButton;
        private Action restartRequested;

        public void BindRestartButton(Button button, Action callback)
        {
            if (registeredRestartButton == button && restartRequested == callback)
            {
                return;
            }

            UnbindRestartButton();
            registeredRestartButton = button;
            restartRequested = callback;
            if (registeredRestartButton != null)
            {
                registeredRestartButton.clicked += OnRestartClicked;
            }
        }

        public void UnbindRestartButton()
        {
            if (registeredRestartButton != null)
            {
                registeredRestartButton.clicked -= OnRestartClicked;
                registeredRestartButton = null;
            }

            restartRequested = null;
        }

        public void RequestRestart()
        {
            restartRequested?.Invoke();
        }

        public static void SetVisible(VisualElement panel, bool visible)
        {
            if (panel != null)
            {
                panel.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        public static void Apply(HudGameOverElements elements, HudVisualConfig config, float scale)
        {
            if (config == null)
            {
                return;
            }

            if (elements.Panel != null)
            {
                elements.Panel.style.left = 0f;
                elements.Panel.style.right = 0f;
                elements.Panel.style.top = 0f;
                elements.Panel.style.bottom = 0f;
                elements.Panel.style.width = StyleKeyword.Auto;
                elements.Panel.style.height = StyleKeyword.Auto;
                elements.Panel.style.marginLeft = 0f;
                elements.Panel.style.marginTop = 0f;
            }

        }

        private void OnRestartClicked()
        {
            RequestRestart();
        }
    }
}
