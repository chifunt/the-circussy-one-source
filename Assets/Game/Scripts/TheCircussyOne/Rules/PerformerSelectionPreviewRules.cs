using System.Collections.Generic;
using System.Text;
using TheCircussyOne.Content;
using TheCircussyOne.Stats;

namespace TheCircussyOne.Rules
{
    public readonly struct PerformerSelectionCardFrame
    {
        public PerformerSelectionCardFrame(
            PerformerDefinition performer,
            WeaponDefinition startingWeaponDefinition,
            string startingWeapon,
            string passive,
            string statPreview,
            string talentTheme)
        {
            Performer = performer;
            StartingWeaponDefinition = startingWeaponDefinition;
            StartingWeapon = startingWeapon ?? string.Empty;
            Passive = passive ?? string.Empty;
            StatPreview = statPreview ?? string.Empty;
            TalentTheme = talentTheme ?? string.Empty;
        }

        public PerformerDefinition Performer { get; }
        public WeaponDefinition StartingWeaponDefinition { get; }
        public string StartingWeapon { get; }
        public string Passive { get; }
        public string StatPreview { get; }
        public string TalentTheme { get; }
    }

    public static class PerformerSelectionPreviewRules
    {
        public static IReadOnlyList<PerformerSelectionCardFrame> BuildFrames(IReadOnlyList<PerformerDefinition> performers)
        {
            var frames = new List<PerformerSelectionCardFrame>(performers?.Count ?? 0);
            if (performers == null)
            {
                return frames;
            }

            for (int i = 0; i < performers.Count; i++)
            {
                PerformerDefinition performer = performers[i];
                if (!ContentAvailabilityRules.IsActiveAndValid(performer) || !performer.unlocked)
                {
                    continue;
                }

                frames.Add(BuildFrame(performer));
            }

            return frames;
        }

        public static PerformerSelectionCardFrame BuildFrame(PerformerDefinition performer)
        {
            if (performer == null)
            {
                return default;
            }

            string startingWeapon = performer.startingWeapon != null ? performer.startingWeapon.DisplayName : "Missing Weapon";
            return new PerformerSelectionCardFrame(
                performer,
                performer.startingWeapon,
                startingWeapon,
                performer.passiveDescription,
                BuildStatPreview(performer),
                performer.talentThemeSummary);
        }

        public static string BuildStatPreview(PerformerDefinition performer)
        {
            if (performer == null)
            {
                return string.Empty;
            }

            var builder = new StringBuilder();
            AppendModifiers(builder, performer.baseStatModifiers);
            AppendModifiers(builder, performer.passiveStatModifiers);
            return builder.Length > 0 ? builder.ToString() : "Baseline stats";
        }

        private static void AppendModifiers(StringBuilder builder, IReadOnlyList<UpgradeStatModifierDefinition> modifiers)
        {
            if (modifiers == null)
            {
                return;
            }

            for (int i = 0; i < modifiers.Count; i++)
            {
                UpgradeStatModifierDefinition modifier = modifiers[i];
                StatDefinition stat = StatMetadata.Get(modifier.statId);
                if (builder.Length > 0)
                {
                    builder.Append(", ");
                }

                builder.Append(FormatModifier(stat, modifier));
            }
        }

        private static string FormatModifier(StatDefinition stat, UpgradeStatModifierDefinition modifier)
        {
            return $"{stat.DisplayName} {StatDisplayRules.FormatModifier(stat, modifier.bucket, modifier.value)}";
        }
    }
}
