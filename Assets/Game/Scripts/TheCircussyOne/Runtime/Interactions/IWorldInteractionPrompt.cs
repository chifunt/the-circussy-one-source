using UnityEngine;

namespace TheCircussyOne.Runtime
{
    public interface IWorldInteractionPromptTarget
    {
        Vector3 WorldInteractionPromptPosition { get; }
    }

    public interface IWorldInteractionPrompt
    {
        bool IsShowing { get; }
        void Show(IWorldInteractionPromptTarget target, string prompt, float progress, bool isHolding, bool useGamepadGlyph, bool showGlyph = true);
        void Hide();
    }

    public sealed class NullWorldInteractionPrompt : IWorldInteractionPrompt
    {
        public bool IsShowing => false;

        public void Show(IWorldInteractionPromptTarget target, string prompt, float progress, bool isHolding, bool useGamepadGlyph, bool showGlyph = true)
        {
        }

        public void Hide()
        {
        }
    }
}
