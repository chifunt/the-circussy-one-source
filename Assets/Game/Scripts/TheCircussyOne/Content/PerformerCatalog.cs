using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using TheCircussyOne.Stats;

namespace TheCircussyOne.Content
{
    [CreateAssetMenu(menuName = "The Circussy One/Content/Performer Catalog", fileName = "PerformerCatalog")]
    public sealed class PerformerCatalog : SerializedScriptableObject, IContentCatalog<PerformerDefinition>
    {
        private const string Tabs = "Performer Catalog";

        [TabGroup(Tabs, "Fallback"), BoxGroup(Tabs + "/Fallback/Run Start"), LabelWidth(180), AssetSelector, LabelText("Fallback Performer")]
        public PerformerDefinition defaultPerformer;

        [TabGroup(Tabs, "Definitions"), BoxGroup(Tabs + "/Definitions/Performers"), LabelWidth(180), AssetSelector]
        public List<PerformerDefinition> performers = new();

        public IReadOnlyList<PerformerDefinition> Performers => performers;
        public IReadOnlyList<PerformerDefinition> Definitions => performers;
        public PerformerDefinition FallbackPerformer => defaultPerformer != null && ContentAvailabilityRules.IsActiveAndValid(defaultPerformer) && defaultPerformer.unlocked
            ? defaultPerformer
            : FirstUnlockedOrAny();

        public List<ContentValidationIssue> ValidateContent()
        {
            var issues = ContentCatalogRules.ValidateDefinitions(Definitions);
            if (performers == null)
            {
                return issues;
            }

            if (defaultPerformer == null)
            {
                issues.Add(new ContentValidationIssue("performer.missing-fallback", ContentValidationSeverity.Error, "Performer catalog has no fallback performer."));
            }
            else
            {
                if (!performers.Contains(defaultPerformer))
                {
                    issues.Add(new ContentValidationIssue("performer.fallback-not-in-catalog", ContentValidationSeverity.Error, $"Fallback performer '{defaultPerformer.Id}' is not listed in the catalog.", defaultPerformer.Id));
                }

                if (!defaultPerformer.isActive)
                {
                    issues.Add(new ContentValidationIssue("performer.fallback-inactive", ContentValidationSeverity.Error, $"Fallback performer '{defaultPerformer.Id}' is inactive.", defaultPerformer.Id));
                }

                if (!defaultPerformer.unlocked)
                {
                    issues.Add(new ContentValidationIssue("performer.fallback-locked", ContentValidationSeverity.Error, $"Fallback performer '{defaultPerformer.Id}' is locked.", defaultPerformer.Id));
                }
            }

            int activeUnlockedCount = 0;
            for (int i = 0; i < performers.Count; i++)
            {
                PerformerDefinition performer = performers[i];
                if (performer == null)
                {
                    continue;
                }

                if (performer.isActive && performer.unlocked)
                {
                    activeUnlockedCount++;
                }

                if (!performer.isActive)
                {
                    continue;
                }

                ValidatePerformer(performer, issues);
            }

            if (activeUnlockedCount == 0)
            {
                issues.Add(new ContentValidationIssue("performer.no-active-unlocked", ContentValidationSeverity.Error, "Performer catalog has no active unlocked performers."));
            }

            return issues;
        }

        public bool EnsureWorkflowDefaults(params PerformerDefinition[] starterPerformers)
        {
            bool changed = false;
            performers ??= new List<PerformerDefinition>();
            if (starterPerformers != null)
            {
                for (int i = 0; i < starterPerformers.Length; i++)
                {
                    PerformerDefinition performer = starterPerformers[i];
                    if (performer == null || performers.Contains(performer))
                    {
                        continue;
                    }

                    performers.Add(performer);
                    changed = true;
                }
            }

            if (defaultPerformer == null && performers.Count > 0)
            {
                defaultPerformer = performers[0];
                changed = true;
            }

            return changed;
        }

        public PerformerDefinition FindById(string performerId)
        {
            if (string.IsNullOrWhiteSpace(performerId) || performers == null)
            {
                return null;
            }

            for (int i = 0; i < performers.Count; i++)
            {
                PerformerDefinition performer = performers[i];
                if (performer != null && string.Equals(performer.Id, performerId, System.StringComparison.Ordinal))
                {
                    return performer;
                }
            }

            return null;
        }

        private PerformerDefinition FirstUnlockedOrAny()
        {
            if (performers == null || performers.Count == 0)
            {
                return null;
            }

            for (int i = 0; i < performers.Count; i++)
            {
                PerformerDefinition performer = performers[i];
                if (ContentAvailabilityRules.IsActiveAndValid(performer) && performer.unlocked)
                {
                    return performer;
                }
            }

            return ContentAvailabilityRules.FirstActive(performers);
        }

        private static void ValidatePerformer(PerformerDefinition performer, List<ContentValidationIssue> issues)
        {
            if (performer.startingWeapon == null)
            {
                issues.Add(new ContentValidationIssue("performer.missing-starting-weapon", ContentValidationSeverity.Error, $"Performer '{performer.Id}' has no starting weapon.", performer.Id));
            }
            else if (!performer.startingWeapon.IsActive)
            {
                issues.Add(new ContentValidationIssue("performer.inactive-starting-weapon", ContentValidationSeverity.Error, $"Performer '{performer.Id}' starts with inactive weapon '{performer.startingWeapon.Id}'.", performer.Id));
            }

            if (!System.Enum.IsDefined(typeof(PerformerPassiveKind), performer.passiveKind))
            {
                issues.Add(new ContentValidationIssue("performer.invalid-passive-kind", ContentValidationSeverity.Error, $"Performer '{performer.Id}' uses an invalid passive kind.", performer.Id));
            }

            ActorAnimationAuthoringRules.AddPerformerValidationIssues(performer, issues);

            ValidateModifiers(performer.baseStatModifiers, "performer.invalid-base-stat", performer.Id, issues);
            ValidateModifiers(performer.passiveStatModifiers, "performer.invalid-passive-stat", performer.Id, issues);
        }

        private static void ValidateModifiers(IReadOnlyList<UpgradeStatModifierDefinition> modifiers, string code, string performerId, List<ContentValidationIssue> issues)
        {
            if (modifiers == null)
            {
                return;
            }

            for (int i = 0; i < modifiers.Count; i++)
            {
                UpgradeStatModifierDefinition modifier = modifiers[i];
                if (!ContentStatValidationRules.IsValid(modifier.statId, modifier.bucket))
                {
                    issues.Add(new ContentValidationIssue(
                        code,
                        ContentValidationSeverity.Error,
                        $"Performer '{performerId}' has an invalid stat modifier at index {i}: {ContentStatValidationRules.DescribeInvalidModifier(modifier.statId, modifier.bucket)}.",
                        performerId));
                    continue;
                }

                if (!ContentStatValidationRules.IsTalentStat(modifier.statId))
                {
                    issues.Add(new ContentValidationIssue(
                        code,
                        ContentValidationSeverity.Error,
                        $"Performer '{performerId}' uses weapon-local stat '{ContentModifierDisplayRules.StatDisplayName(modifier.statId)}' at index {i}; performer modifiers can only use player/global stats.",
                        performerId));
                }
            }
        }
    }
}
