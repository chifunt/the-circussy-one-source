using System;
using Sirenix.OdinInspector;
using TheCircussyOne.Runtime;
using UnityEngine;

namespace TheCircussyOne.Config
{
    [CreateAssetMenu(menuName = "The Circussy One/Game Haptics Config", fileName = "GameHapticsConfig")]
    [InfoBox("LIVE RUNTIME: Controller haptics are read by runtime systems. Per-cue toggles, strengths, timings, priorities, and cooldowns are live runtime tuning.")]
    public sealed class GameHapticsConfig : SerializedScriptableObject
    {
        private const string Tabs = "Game Haptics";

        [TabGroup(Tabs, "Runtime"), BoxGroup(Tabs + "/Runtime/Master"), LabelWidth(190)]
        public bool hapticsEnabled = true;
        [TabGroup(Tabs, "Runtime"), BoxGroup(Tabs + "/Runtime/Master"), LabelWidth(190)]
        [Range(0f, 1f)] public float masterScale = 0.85f;
        [TabGroup(Tabs, "Runtime"), BoxGroup(Tabs + "/Runtime/Master"), LabelWidth(190)]
        public bool allowHapticsWhilePaused = true;
        [TabGroup(Tabs, "Runtime"), BoxGroup(Tabs + "/Runtime/Master"), LabelWidth(190), SuffixLabel("sec")]
        [Min(0f)] public float maximumPulseSeconds = 0.4f;

        [TabGroup(Tabs, "UI"), BoxGroup(Tabs + "/UI/Menu"), LabelWidth(190)]
        public GameHapticsPattern uiHover = GameHapticsPattern.Single(0.025f, 0.045f, 0.025f, 5, 0.045f);
        [TabGroup(Tabs, "UI"), BoxGroup(Tabs + "/UI/Menu"), LabelWidth(190)]
        public GameHapticsPattern uiClick = GameHapticsPattern.Single(0.08f, 0.13f, 0.045f, 8, 0.035f);
        [TabGroup(Tabs, "UI"), BoxGroup(Tabs + "/UI/Rewards"), LabelWidth(190)]
        public GameHapticsPattern levelUp = GameHapticsPattern.Sequence(
            30,
            0.2f,
            new GameHapticsPulse(0.12f, 0.2f, 0.05f),
            new GameHapticsPulse(0.18f, 0.3f, 0.06f, 0.04f),
            new GameHapticsPulse(0.26f, 0.45f, 0.09f, 0.04f));
        [TabGroup(Tabs, "UI"), BoxGroup(Tabs + "/UI/Rewards"), LabelWidth(190)]
        public GameHapticsPattern itemObtained = GameHapticsPattern.Sequence(
            24,
            0.12f,
            new GameHapticsPulse(0.16f, 0.28f, 0.06f),
            new GameHapticsPulse(0.1f, 0.18f, 0.05f, 0.06f));

        [TabGroup(Tabs, "Gameplay"), BoxGroup(Tabs + "/Gameplay/Pickups"), LabelWidth(190)]
        public GameHapticsPattern xpCollect = GameHapticsPattern.Single(0.025f, 0.04f, 0.025f, 3, 0.04f);
        [TabGroup(Tabs, "Gameplay"), BoxGroup(Tabs + "/Gameplay/Pickups"), LabelWidth(190)]
        public GameHapticsPattern ticketCollect = GameHapticsPattern.Single(0.04f, 0.07f, 0.035f, 4, 0.035f);
        [TabGroup(Tabs, "Gameplay"), BoxGroup(Tabs + "/Gameplay/Pickups"), LabelWidth(190)]
        public GameHapticsPattern snackEat = GameHapticsPattern.Single(0.08f, 0.12f, 0.05f, 7, 0.05f);

        [TabGroup(Tabs, "Gameplay"), BoxGroup(Tabs + "/Gameplay/Interactions"), LabelWidth(190)]
        public GameHapticsPattern interactionCanceled = GameHapticsPattern.Single(0.08f, 0.05f, 0.06f, 9, 0.1f);
        [TabGroup(Tabs, "Gameplay"), BoxGroup(Tabs + "/Gameplay/Interactions"), LabelWidth(190)]
        public GameHapticsPattern chestOpen = GameHapticsPattern.Single(0.18f, 0.25f, 0.08f, 12, 0.08f);
        [TabGroup(Tabs, "Gameplay"), BoxGroup(Tabs + "/Gameplay/Interactions"), LabelWidth(190)]
        public GameHapticsPattern snackOpen = GameHapticsPattern.Single(0.12f, 0.16f, 0.06f, 8, 0.08f);
        [TabGroup(Tabs, "Gameplay"), BoxGroup(Tabs + "/Gameplay/Interactions"), LabelWidth(190)]
        public GameHapticsPattern stageDoorOpen = GameHapticsPattern.Sequence(
            20,
            0.2f,
            new GameHapticsPulse(0.12f, 0.2f, 0.09f),
            new GameHapticsPulse(0.25f, 0.35f, 0.1f, 0.05f));

        [TabGroup(Tabs, "Gameplay"), BoxGroup(Tabs + "/Gameplay/Player"), LabelWidth(190)]
        public GameHapticsPattern playerDamaged = GameHapticsPattern.Single(0.7f, 0.85f, 0.16f, 40, 0.1f);
        [TabGroup(Tabs, "Gameplay"), BoxGroup(Tabs + "/Gameplay/Player"), LabelWidth(190)]
        public GameHapticsPattern playerJump = GameHapticsPattern.Single(0.05f, 0.08f, 0.035f, 6, 0.08f);
        [TabGroup(Tabs, "Gameplay"), BoxGroup(Tabs + "/Gameplay/Player"), LabelWidth(190)]
        public GameHapticsPattern playerLand = GameHapticsPattern.Single(0.08f, 0.16f, 0.05f, 7, 0.08f);
        [TabGroup(Tabs, "Gameplay"), BoxGroup(Tabs + "/Gameplay/Player"), LabelWidth(190)]
        public GameHapticsPattern gameOver = GameHapticsPattern.Sequence(
            50,
            0.5f,
            new GameHapticsPulse(0.45f, 0.7f, 0.24f),
            new GameHapticsPulse(0.25f, 0.35f, 0.14f, 0.05f));

        [TabGroup(Tabs, "Run Events"), BoxGroup(Tabs + "/Run Events/Headliner"), LabelWidth(190)]
        public GameHapticsPattern headlinerDefeated = GameHapticsPattern.Sequence(
            35,
            0.3f,
            new GameHapticsPulse(0.25f, 0.45f, 0.12f),
            new GameHapticsPulse(0.3f, 0.55f, 0.16f, 0.08f));
        [TabGroup(Tabs, "Run Events"), BoxGroup(Tabs + "/Run Events/Headliner"), LabelWidth(190)]
        public GameHapticsPattern encoreStarted = GameHapticsPattern.Single(0.28f, 0.5f, 0.18f, 35, 0.3f);

        public static GameHapticsConfig CreateRuntimeDefault()
        {
            var config = CreateInstance<GameHapticsConfig>();
            config.hideFlags = HideFlags.DontSave;
            config.EnsureWorkflowDefaults();
            return config;
        }

        public bool EnsureWorkflowDefaults()
        {
            bool changed = false;
            changed |= EnsureRange(ref masterScale, 0.85f, 0f, 1f);
            changed |= EnsureMinimum(ref maximumPulseSeconds, 0.4f, 0.01f);
            changed |= EnsurePattern(ref uiHover, GameHapticsPattern.Single(0.025f, 0.045f, 0.025f, 5, 0.045f));
            changed |= EnsurePattern(ref uiClick, GameHapticsPattern.Single(0.08f, 0.13f, 0.045f, 8, 0.035f));
            changed |= EnsurePattern(ref levelUp, GameHapticsPattern.Sequence(
                30,
                0.2f,
                new GameHapticsPulse(0.12f, 0.2f, 0.05f),
                new GameHapticsPulse(0.18f, 0.3f, 0.06f, 0.04f),
                new GameHapticsPulse(0.26f, 0.45f, 0.09f, 0.04f)));
            changed |= EnsurePattern(ref itemObtained, GameHapticsPattern.Sequence(
                24,
                0.12f,
                new GameHapticsPulse(0.16f, 0.28f, 0.06f),
                new GameHapticsPulse(0.1f, 0.18f, 0.05f, 0.06f)));
            changed |= EnsurePattern(ref xpCollect, GameHapticsPattern.Single(0.025f, 0.04f, 0.025f, 3, 0.04f));
            changed |= EnsurePattern(ref ticketCollect, GameHapticsPattern.Single(0.04f, 0.07f, 0.035f, 4, 0.035f));
            changed |= EnsurePattern(ref snackEat, GameHapticsPattern.Single(0.08f, 0.12f, 0.05f, 7, 0.05f));
            changed |= EnsurePattern(ref interactionCanceled, GameHapticsPattern.Single(0.08f, 0.05f, 0.06f, 9, 0.1f));
            changed |= EnsurePattern(ref chestOpen, GameHapticsPattern.Single(0.18f, 0.25f, 0.08f, 12, 0.08f));
            changed |= EnsurePattern(ref snackOpen, GameHapticsPattern.Single(0.12f, 0.16f, 0.06f, 8, 0.08f));
            changed |= EnsurePattern(ref stageDoorOpen, GameHapticsPattern.Sequence(
                20,
                0.2f,
                new GameHapticsPulse(0.12f, 0.2f, 0.09f),
                new GameHapticsPulse(0.25f, 0.35f, 0.1f, 0.05f)));
            changed |= EnsurePattern(ref playerDamaged, GameHapticsPattern.Single(0.7f, 0.85f, 0.16f, 40, 0.1f));
            changed |= EnsurePattern(ref playerJump, GameHapticsPattern.Single(0.05f, 0.08f, 0.035f, 6, 0.08f));
            changed |= EnsurePattern(ref playerLand, GameHapticsPattern.Single(0.08f, 0.16f, 0.05f, 7, 0.08f));
            changed |= EnsurePattern(ref gameOver, GameHapticsPattern.Sequence(
                50,
                0.5f,
                new GameHapticsPulse(0.45f, 0.7f, 0.24f),
                new GameHapticsPulse(0.25f, 0.35f, 0.14f, 0.05f)));
            changed |= EnsurePattern(ref headlinerDefeated, GameHapticsPattern.Sequence(
                35,
                0.3f,
                new GameHapticsPulse(0.25f, 0.45f, 0.12f),
                new GameHapticsPulse(0.3f, 0.55f, 0.16f, 0.08f)));
            changed |= EnsurePattern(ref encoreStarted, GameHapticsPattern.Single(0.28f, 0.5f, 0.18f, 35, 0.3f));
            return changed;
        }

        public GameHapticsPattern PatternFor(GameHapticsCue cue)
        {
            return cue switch
            {
                GameHapticsCue.UiHover => uiHover,
                GameHapticsCue.UiClick => uiClick,
                GameHapticsCue.LevelUp => levelUp,
                GameHapticsCue.ItemObtained => itemObtained,
                GameHapticsCue.XpCollect => xpCollect,
                GameHapticsCue.TicketCollect => ticketCollect,
                GameHapticsCue.SnackEat => snackEat,
                GameHapticsCue.InteractionCanceled => interactionCanceled,
                GameHapticsCue.ChestOpen => chestOpen,
                GameHapticsCue.SnackOpen => snackOpen,
                GameHapticsCue.StageDoorOpen => stageDoorOpen,
                GameHapticsCue.PlayerDamaged => playerDamaged,
                GameHapticsCue.PlayerJump => playerJump,
                GameHapticsCue.PlayerLand => playerLand,
                GameHapticsCue.GameOver => gameOver,
                GameHapticsCue.HeadlinerDefeated => headlinerDefeated,
                GameHapticsCue.EncoreStarted => encoreStarted,
                _ => null
            };
        }

        private static bool EnsurePattern(ref GameHapticsPattern pattern, GameHapticsPattern fallback)
        {
            if (pattern == null)
            {
                pattern = fallback.Clone();
                return true;
            }

            return pattern.EnsureWorkflowDefaults(fallback);
        }

        private static bool EnsureMinimum(ref float value, float fallback, float minimum)
        {
            if (!IsFinite(value) || value < minimum)
            {
                value = Mathf.Max(minimum, fallback);
                return true;
            }

            return false;
        }

        private static bool EnsureRange(ref float value, float fallback, float min, float max)
        {
            if (!IsFinite(value))
            {
                value = Mathf.Clamp(fallback, min, max);
                return true;
            }

            float clamped = Mathf.Clamp(value, min, max);
            if (!Mathf.Approximately(clamped, value))
            {
                value = clamped;
                return true;
            }

            return false;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private void OnValidate()
        {
            EnsureWorkflowDefaults();
        }
    }

    [Serializable]
    public sealed class GameHapticsPattern
    {
        public bool enabled = true;
        [Range(0f, 1f)] public float scale = 1f;
        [Min(0)] public int priority = 10;
        [Min(0f)] public float cooldownSeconds;
        public GameHapticsPulse[] pulses = Array.Empty<GameHapticsPulse>();

        public static GameHapticsPattern Single(float lowFrequency, float highFrequency, float seconds, int priority, float cooldownSeconds)
        {
            return Sequence(priority, cooldownSeconds, new GameHapticsPulse(lowFrequency, highFrequency, seconds));
        }

        public static GameHapticsPattern Sequence(int priority, float cooldownSeconds, params GameHapticsPulse[] pulses)
        {
            return new GameHapticsPattern
            {
                priority = Mathf.Max(0, priority),
                cooldownSeconds = Mathf.Max(0f, cooldownSeconds),
                pulses = ClonePulses(pulses)
            };
        }

        public GameHapticsPattern Clone()
        {
            return new GameHapticsPattern
            {
                enabled = enabled,
                scale = scale,
                priority = priority,
                cooldownSeconds = cooldownSeconds,
                pulses = ClonePulses(pulses)
            };
        }

        public bool EnsureWorkflowDefaults(GameHapticsPattern fallback)
        {
            bool changed = false;
            if (!IsFinite(scale))
            {
                scale = fallback != null ? fallback.scale : 1f;
                changed = true;
            }

            float clampedScale = Mathf.Clamp01(scale);
            if (!Mathf.Approximately(clampedScale, scale))
            {
                scale = clampedScale;
                changed = true;
            }

            if (priority < 0)
            {
                priority = fallback != null ? fallback.priority : 10;
                changed = true;
            }

            if (!IsFinite(cooldownSeconds) || cooldownSeconds < 0f)
            {
                cooldownSeconds = fallback != null ? fallback.cooldownSeconds : 0f;
                changed = true;
            }

            if (pulses == null || pulses.Length == 0)
            {
                pulses = fallback != null ? ClonePulses(fallback.pulses) : new[] { new GameHapticsPulse(0.05f, 0.05f, 0.04f) };
                changed = true;
            }

            for (int i = 0; i < pulses.Length; i++)
            {
                if (pulses[i] == null)
                {
                    pulses[i] = new GameHapticsPulse(0.05f, 0.05f, 0.04f);
                    changed = true;
                }

                changed |= pulses[i].EnsureWorkflowDefaults();
            }

            return changed;
        }

        private static GameHapticsPulse[] ClonePulses(GameHapticsPulse[] source)
        {
            if (source == null || source.Length == 0)
            {
                return Array.Empty<GameHapticsPulse>();
            }

            var result = new GameHapticsPulse[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                result[i] = source[i] != null ? source[i].Clone() : null;
            }

            return result;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }

    [Serializable]
    public sealed class GameHapticsPulse
    {
        [Range(0f, 1f)] public float lowFrequency;
        [Range(0f, 1f)] public float highFrequency;
        [Min(0f)] public float durationSeconds = 0.04f;
        [Min(0f)] public float delayBeforeSeconds;

        public GameHapticsPulse()
        {
        }

        public GameHapticsPulse(float lowFrequency, float highFrequency, float durationSeconds, float delayBeforeSeconds = 0f)
        {
            this.lowFrequency = lowFrequency;
            this.highFrequency = highFrequency;
            this.durationSeconds = durationSeconds;
            this.delayBeforeSeconds = delayBeforeSeconds;
        }

        public GameHapticsPulse Clone()
        {
            return new GameHapticsPulse(lowFrequency, highFrequency, durationSeconds, delayBeforeSeconds);
        }

        public bool EnsureWorkflowDefaults()
        {
            bool changed = false;
            changed |= EnsureRange(ref lowFrequency, 0.05f, 0f, 1f);
            changed |= EnsureRange(ref highFrequency, 0.05f, 0f, 1f);
            changed |= EnsureMinimum(ref durationSeconds, 0.04f, 0.001f);
            changed |= EnsureMinimum(ref delayBeforeSeconds, 0f, 0f);
            return changed;
        }

        private static bool EnsureMinimum(ref float value, float fallback, float minimum)
        {
            if (!IsFinite(value) || value < minimum)
            {
                value = Mathf.Max(minimum, fallback);
                return true;
            }

            return false;
        }

        private static bool EnsureRange(ref float value, float fallback, float min, float max)
        {
            if (!IsFinite(value))
            {
                value = Mathf.Clamp(fallback, min, max);
                return true;
            }

            float clamped = Mathf.Clamp(value, min, max);
            if (!Mathf.Approximately(clamped, value))
            {
                value = clamped;
                return true;
            }

            return false;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
