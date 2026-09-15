using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TheCircussyOne.Content
{
    [CreateAssetMenu(menuName = "The Circussy One/Content/Talent Catalog", fileName = "TalentCatalog")]
    public sealed class TalentCatalog : SerializedScriptableObject, IContentCatalog<TalentDefinition>
    {
        private const string Tabs = "Talent Catalog";

        [TabGroup(Tabs, "Definitions"), BoxGroup(Tabs + "/Definitions/Shared Talents"), LabelWidth(160), AssetSelector]
        public List<TalentDefinition> talents = new();

        public IReadOnlyList<TalentDefinition> Talents => talents;
        public IReadOnlyList<TalentDefinition> Definitions => talents;

        public List<ContentValidationIssue> ValidateContent()
        {
            var issues = ContentCatalogRules.ValidateDefinitions(Definitions);
            if (talents == null)
            {
                return issues;
            }

            for (int i = 0; i < talents.Count; i++)
            {
                TalentDefinition talent = talents[i];
                if (talent == null)
                {
                    continue;
                }

                if (!talent.isActive)
                {
                    continue;
                }

                if (!System.Enum.IsDefined(typeof(TalentPoolKind), talent.poolKind))
                {
                    issues.Add(new ContentValidationIssue("talent.invalid-pool-kind", ContentValidationSeverity.Error, $"Talent '{talent.Id}' uses an invalid pool kind.", talent.Id));
                }

                if (!System.Enum.IsDefined(typeof(TalentRepeatPolicy), talent.repeatPolicy))
                {
                    issues.Add(new ContentValidationIssue("talent.invalid-repeat-policy", ContentValidationSeverity.Error, $"Talent '{talent.Id}' uses an invalid repeat policy.", talent.Id));
                }

                if (talent.possibleRarities.IsEmpty)
                {
                    issues.Add(new ContentValidationIssue("talent.empty-possible-rarities", ContentValidationSeverity.Error, $"Talent '{talent.Id}' has no possible level-up rarities.", talent.Id));
                }

                if (ContentCatalogRules.LooksLikeMechanicalLevelUpDescription(talent.shortDescription))
                {
                    issues.Add(new ContentValidationIssue("talent.mechanical-short-description", ContentValidationSeverity.Warning, $"Talent '{talent.Id}' short description should be flavor text; exact numbers belong in the preview line.", talent.Id));
                }

                if (talent.poolKind == TalentPoolKind.PerformerSpecific && !ContentId.IsValidValue(talent.performerId))
                {
                    issues.Add(new ContentValidationIssue("talent.missing-performer-id", ContentValidationSeverity.Error, $"Talent '{talent.Id}' is performer-specific but has no performer id.", talent.Id));
                }

                if (talent.poolKind == TalentPoolKind.PerformerSpecific
                    && talent.performerDefinition != null
                    && !string.Equals(talent.performerId, talent.performerDefinition.Id, System.StringComparison.Ordinal))
                {
                    issues.Add(new ContentValidationIssue("talent.performer-id-reference-mismatch", ContentValidationSeverity.Warning, $"Talent '{talent.Id}' has mirrored performer id '{talent.performerId}' but references '{talent.performerDefinition.Id}'. Use Sync ID From Reference.", talent.Id));
                }

                ValidateSharedPerformerAccess(talent, issues);

                if (talent.repeatPolicy == TalentRepeatPolicy.Capped && talent.maxLevel < 1)
                {
                    issues.Add(new ContentValidationIssue("talent.invalid-max-level", ContentValidationSeverity.Error, $"Talent '{talent.Id}' has an invalid max level.", talent.Id));
                }

                if (talent.statModifiers == null || talent.statModifiers.Count == 0)
                {
                    issues.Add(new ContentValidationIssue("talent.missing-stat-modifiers", ContentValidationSeverity.Error, $"Talent '{talent.Id}' has no stat modifiers.", talent.Id));
                }
                else
                {
                    ContentStatValidationRules.ValidateUpgradeModifiers(talent.statModifiers, issues, "talent.invalid-stat-modifier", talent.Id, "Talent");
                    ValidateTalentStats(talent, issues);
                }
            }

            return issues;
        }

        private static void ValidateSharedPerformerAccess(TalentDefinition talent, List<ContentValidationIssue> issues)
        {
            if (talent.poolKind != TalentPoolKind.Shared || talent.sharedPerformerAccess == null || talent.sharedPerformerAccess.Count == 0)
            {
                return;
            }

            var seen = new HashSet<string>(System.StringComparer.Ordinal);
            for (int accessIndex = 0; accessIndex < talent.sharedPerformerAccess.Count; accessIndex++)
            {
                TalentPerformerAccess access = talent.sharedPerformerAccess[accessIndex];
                if (access == null)
                {
                    issues.Add(new ContentValidationIssue("talent.null-performer-access", ContentValidationSeverity.Error, $"Talent '{talent.Id}' has a null shared performer access entry.", talent.Id));
                    continue;
                }

                if (!ContentId.IsValidValue(access.PerformerId))
                {
                    issues.Add(new ContentValidationIssue("talent.invalid-performer-access", ContentValidationSeverity.Error, $"Talent '{talent.Id}' has an invalid shared performer access entry.", talent.Id));
                    continue;
                }

                if (!seen.Add(access.PerformerId))
                {
                    issues.Add(new ContentValidationIssue("talent.duplicate-performer-access", ContentValidationSeverity.Error, $"Talent '{talent.Id}' has duplicate shared performer access for '{access.PerformerId}'.", talent.Id));
                }
            }
        }

        private static void ValidateTalentStats(TalentDefinition talent, List<ContentValidationIssue> issues)
        {
            for (int modifierIndex = 0; modifierIndex < talent.statModifiers.Count; modifierIndex++)
            {
                UpgradeStatModifierDefinition modifier = talent.statModifiers[modifierIndex];
                if (!ContentStatValidationRules.IsValid(modifier.statId, modifier.bucket))
                {
                    continue;
                }

                if (!ContentStatValidationRules.IsTalentStat(modifier.statId))
                {
                    issues.Add(new ContentValidationIssue(
                        "talent.weapon-stat",
                        ContentValidationSeverity.Error,
                        $"Talent '{talent.Id}' uses weapon-local stat '{ContentModifierDisplayRules.StatDisplayName(modifier.statId)}'; talents can only use player/global stats.",
                        talent.Id));
                }
            }
        }

        public bool EnsureWorkflowDefaults(params TalentDefinition[] starterTalents)
        {
            bool changed = false;
            talents ??= new List<TalentDefinition>();
            if (starterTalents == null)
            {
                return changed;
            }

            for (int i = 0; i < starterTalents.Length; i++)
            {
                TalentDefinition talent = starterTalents[i];
                if (talent == null || talents.Contains(talent))
                {
                    continue;
                }

                talents.Add(talent);
                changed = true;
            }

            return changed;
        }
    }
}
