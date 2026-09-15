using System;
using System.Collections.Generic;

namespace TheCircussyOne.Content
{
    public enum ContentValidationSeverity
    {
        Warning,
        Error
    }

    public readonly struct ContentValidationIssue
    {
        public ContentValidationIssue(string code, ContentValidationSeverity severity, string message, string contentId = null)
        {
            Code = code;
            Severity = severity;
            Message = message;
            ContentId = contentId;
        }

        public string Code { get; }
        public ContentValidationSeverity Severity { get; }
        public string Message { get; }
        public string ContentId { get; }
    }

    public static class ContentCatalogRules
    {
        public static List<ContentValidationIssue> ValidateDefinitions<TDefinition>(IReadOnlyList<TDefinition> definitions)
            where TDefinition : class, IContentDefinition
        {
            var issues = new List<ContentValidationIssue>();
            var ids = new HashSet<string>(StringComparer.Ordinal);
            if (definitions == null)
            {
                issues.Add(new ContentValidationIssue("catalog.null", ContentValidationSeverity.Error, "Catalog definition list is null."));
                return issues;
            }

            for (int i = 0; i < definitions.Count; i++)
            {
                TDefinition definition = definitions[i];
                if (definition == null)
                {
                    issues.Add(new ContentValidationIssue("catalog.null-entry", ContentValidationSeverity.Error, $"Catalog contains a null entry at index {i}."));
                    continue;
                }

                string id = definition.Id;
                if (!ContentAvailabilityRules.IsActive(definition))
                {
                    issues.Add(new ContentValidationIssue("content.inactive", ContentValidationSeverity.Warning, $"Content '{id}' is inactive and will be ignored by normal runtime flows.", id));
                }

                bool hasValidId = ContentId.IsValidValue(id);
                if (!hasValidId)
                {
                    issues.Add(new ContentValidationIssue("content.invalid-id", ContentValidationSeverity.Error, $"Content id '{id}' is not a valid id. Use lowercase snake_case.", id));
                }

                if (hasValidId && !ids.Add(id))
                {
                    issues.Add(new ContentValidationIssue("content.duplicate-id", ContentValidationSeverity.Error, $"Duplicate content id '{id}'.", id));
                }

                if (string.IsNullOrWhiteSpace(definition.DisplayName))
                {
                    issues.Add(new ContentValidationIssue("content.missing-display-name", ContentValidationSeverity.Error, $"Content '{id}' is missing a display name.", id));
                }

                if (definition.Tags == null)
                {
                    issues.Add(new ContentValidationIssue("content.missing-tags", ContentValidationSeverity.Warning, $"Content '{id}' has no tag set.", id));
                    continue;
                }

                if (HasDuplicateTags(definition.Tags.Values))
                {
                    issues.Add(new ContentValidationIssue("content.duplicate-tags", ContentValidationSeverity.Warning, $"Content '{id}' contains duplicate tags.", id));
                }
            }

            return issues;
        }

        public static List<ContentValidationIssue> ValidateRarityWeights(ContentRarityWeightTable table, string contextName)
        {
            var issues = new List<ContentValidationIssue>();
            if (table.common < 0 || table.uncommon < 0 || table.rare < 0 || table.epic < 0 || table.legendary < 0)
            {
                issues.Add(new ContentValidationIssue("rarity.negative-weight", ContentValidationSeverity.Error, $"{contextName} rarity table contains a negative weight."));
            }

            if (table.TotalWeight <= 0)
            {
                issues.Add(new ContentValidationIssue("rarity.empty-table", ContentValidationSeverity.Error, $"{contextName} rarity table has no positive weights."));
            }

            return issues;
        }

        public static bool LooksLikeMechanicalLevelUpDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                return false;
            }

            return description.IndexOf("Rarity-scaled", StringComparison.OrdinalIgnoreCase) >= 0
                || description.IndexOf("Rare-Legendary", StringComparison.OrdinalIgnoreCase) >= 0
                || description.Contains('+')
                || description.Contains('%')
                || description.Contains("->")
                || ContainsDigit(description);
        }

        private static bool ContainsDigit(string value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                if (char.IsDigit(value[i]))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasDuplicateTags(IReadOnlyList<ContentTag> tags)
        {
            if (tags == null)
            {
                return false;
            }

            var seen = new HashSet<ContentTag>();
            for (int i = 0; i < tags.Count; i++)
            {
                if (!seen.Add(tags[i]))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
