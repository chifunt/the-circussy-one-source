using UnityEngine;

namespace TheCircussyOne.Rules
{
    public enum WorldInteractionPromptTransition
    {
        Hidden = 0,
        Showing = 1,
        Visible = 2,
        Hiding = 3
    }

    public readonly struct WorldInteractionPromptFrame
    {
        public WorldInteractionPromptFrame(
            bool visible,
            Vector3 position,
            float scale,
            float alpha,
            string label,
            string glyph,
            bool showGlyph,
            bool useGamepadGlyph,
            bool showProgress,
            float progress,
            Vector2 progressShake)
        {
            Visible = visible;
            Position = position;
            Scale = scale;
            Alpha = alpha;
            Label = label;
            Glyph = glyph;
            ShowGlyph = showGlyph;
            UseGamepadGlyph = useGamepadGlyph;
            ShowProgress = showProgress;
            Progress = progress;
            ProgressShake = progressShake;
        }

        public bool Visible { get; }
        public Vector3 Position { get; }
        public float Scale { get; }
        public float Alpha { get; }
        public string Label { get; }
        public string Glyph { get; }
        public bool ShowGlyph { get; }
        public bool UseGamepadGlyph { get; }
        public bool ShowProgress { get; }
        public float Progress { get; }
        public Vector2 ProgressShake { get; }
    }

    public static class WorldInteractionPromptRules
    {
        public static string Glyph(bool useGamepadGlyph)
        {
            return useGamepadGlyph ? "X" : "E";
        }

        public static string Label(string action, bool isHolding)
        {
            return Label(action, isHolding, showActionPrefix: true);
        }

        public static string Label(string action, bool isHolding, bool showActionPrefix)
        {
            if (string.IsNullOrWhiteSpace(action))
            {
                return string.Empty;
            }

            action = action.Trim();
            if (isHolding)
            {
                return HoldingLabel(action);
            }

            return showActionPrefix ? $"to {action}" : action;
        }

        public static string HoldingLabel(string action)
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
                "spend" => "Spending",
                "claim" => "Claiming",
                "raid" => "Raiding",
                "cash" => "Cashing",
                "crack" => "Cracking",
                "finish" => "Finishing",
                "grab" => "Grabbing",
                "interact" => "Interacting",
                _ => string.Empty
            };

            return string.IsNullOrEmpty(gerund) ? action : $"{gerund}{remainder}";
        }

        public static float Scale(
            WorldInteractionPromptTransition transition,
            float elapsedSeconds,
            float showSeconds,
            float hideSeconds,
            float baseWorldScale,
            float targetScale,
            EaseSettings showEase,
            EaseSettings hideEase)
        {
            float baseScale = Mathf.Max(0.001f, baseWorldScale);
            float target = Mathf.Max(0.001f, targetScale);
            return transition switch
            {
                WorldInteractionPromptTransition.Showing => baseScale * Mathf.LerpUnclamped(0f, target, GameEasing.Evaluate01(showEase, elapsedSeconds / Mathf.Max(0.01f, showSeconds))),
                WorldInteractionPromptTransition.Hiding => baseScale * Mathf.LerpUnclamped(target, 0f, GameEasing.Evaluate01(hideEase, elapsedSeconds / Mathf.Max(0.01f, hideSeconds))),
                WorldInteractionPromptTransition.Visible => baseScale * target,
                _ => 0f
            };
        }

        public static float Alpha(WorldInteractionPromptTransition transition, float elapsedSeconds, float showSeconds, float hideSeconds)
        {
            return transition switch
            {
                WorldInteractionPromptTransition.Showing => Mathf.Clamp01(elapsedSeconds / Mathf.Max(0.01f, showSeconds)),
                WorldInteractionPromptTransition.Hiding => 1f - Mathf.Clamp01(elapsedSeconds / Mathf.Max(0.01f, hideSeconds)),
                WorldInteractionPromptTransition.Visible => 1f,
                _ => 0f
            };
        }

        public static Vector2 ProgressShake(
            float progress,
            float elapsedSeconds,
            float minAmplitude,
            float maxAmplitude,
            float frequency,
            EaseSettings ease)
        {
            float normalizedProgress = Mathf.Clamp01(progress);
            if (normalizedProgress <= 0f || maxAmplitude <= 0f || frequency <= 0f)
            {
                return Vector2.zero;
            }

            float t = GameEasing.Evaluate01(ease, normalizedProgress);
            float amplitude = Mathf.Lerp(Mathf.Max(0f, minAmplitude), Mathf.Max(0f, maxAmplitude), t);
            float phase = elapsedSeconds * Mathf.Max(0f, frequency) * Mathf.PI * 2f;
            return new Vector2(Mathf.Sin(phase) * amplitude, Mathf.Sin(phase * 1.73f + 0.65f) * amplitude * 0.45f);
        }
    }
}
