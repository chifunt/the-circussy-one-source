using UnityEngine;
using UnityEngine.UIElements;

namespace TheCircussyOne.Visuals
{
    internal static class HudInteractionPromptDriver
    {
        public static void Apply(
            VisualElement promptRoot,
            Label buttonGlyph,
            Label promptLabel,
            VisualElement progressFill,
            string prompt,
            float normalizedProgress,
            bool isHolding,
            bool useGamepadGlyph,
            bool showGlyph)
        {
            bool hasPrompt = !string.IsNullOrWhiteSpace(prompt);
            if (promptRoot != null)
            {
                promptRoot.style.display = hasPrompt ? DisplayStyle.Flex : DisplayStyle.None;
            }

            if (buttonGlyph != null)
            {
                buttonGlyph.style.display = isHolding || !showGlyph || !hasPrompt ? DisplayStyle.None : DisplayStyle.Flex;
                buttonGlyph.text = useGamepadGlyph ? "X" : "E";
                buttonGlyph.EnableInClassList("interaction-button-glyph--gamepad", useGamepadGlyph);
                buttonGlyph.EnableInClassList("interaction-button-glyph--keyboard", !useGamepadGlyph);
            }

            if (promptLabel != null)
            {
                promptLabel.text = hasPrompt ? FormatPromptText(prompt, isHolding, showGlyph) : string.Empty;
            }

            if (progressFill != null)
            {
                progressFill.style.width = Length.Percent(Mathf.Clamp01(normalizedProgress) * 100f);
            }
        }

        public static void SetVisible(
            VisualElement promptRoot,
            Label buttonGlyph,
            VisualElement progressFill,
            bool visible)
        {
            if (promptRoot != null)
            {
                promptRoot.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            }

            if (!visible && progressFill != null)
            {
                progressFill.style.width = Length.Percent(0f);
            }

            if (!visible && buttonGlyph != null)
            {
                buttonGlyph.style.display = DisplayStyle.None;
            }
        }

        public static string FormatPromptText(string action, bool isHolding, bool showGlyph)
        {
            if (string.IsNullOrWhiteSpace(action))
            {
                return string.Empty;
            }

            if (isHolding)
            {
                return HoldingPromptText(action);
            }

            return showGlyph ? $"Hold to {action}" : action;
        }

        public static string HoldingPromptText(string action)
        {
            if (string.IsNullOrWhiteSpace(action))
            {
                return string.Empty;
            }

            int firstSpace = action.IndexOf(' ');
            string verb = firstSpace < 0 ? action : action[..firstSpace];
            string remainder = firstSpace < 0 ? string.Empty : action[firstSpace..];
            string gerund = verb.ToLowerInvariant() switch
            {
                "collect" => "Collecting",
                "open" => "Opening",
                "claim" => "Claiming",
                "raid" => "Raiding",
                "cash" => "Cashing",
                "crack" => "Cracking",
                "finish" => "Finishing",
                "interact" => "Interacting",
                _ => string.Empty
            };

            return string.IsNullOrEmpty(gerund) ? $"{action}..." : $"{gerund}{remainder}...";
        }
    }
}
