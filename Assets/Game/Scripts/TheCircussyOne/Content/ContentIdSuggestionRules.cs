using System;
using System.Collections.Generic;
using TheCircussyOne.Stats;

namespace TheCircussyOne.Content
{
    public static class ContentIdSuggestionRules
    {
        private const string DefaultFallback = "content";

        public static string BuildId(params string[] parts)
        {
            if (parts == null || parts.Length == 0)
            {
                return DefaultFallback;
            }

            var joined = new List<string>();
            for (int i = 0; i < parts.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(parts[i]))
                {
                    joined.Add(parts[i]);
                }
            }

            return NormalizeBase(string.Join(" ", joined), DefaultFallback);
        }

        public static string NormalizeBase(string preferred, string fallback = DefaultFallback)
        {
            string normalized = ContentId.Normalize(preferred);
            if (ContentId.IsValidValue(normalized))
            {
                return normalized;
            }

            normalized = ContentId.Normalize(fallback);
            return ContentId.IsValidValue(normalized) ? normalized : DefaultFallback;
        }

        public static string MakeUniqueId(string preferred, IReadOnlyCollection<string> existingIds, string fallback = DefaultFallback)
        {
            string baseId = NormalizeBase(preferred, fallback);
            if (!Contains(existingIds, baseId))
            {
                return baseId;
            }

            for (int suffix = 2; suffix < 10000; suffix++)
            {
                string candidate = $"{baseId}_{suffix:000}";
                if (!Contains(existingIds, candidate))
                {
                    return candidate;
                }
            }

            throw new InvalidOperationException($"Could not generate a unique content id for '{baseId}'.");
        }

        public static string MakeCopyId(string sourceId, IReadOnlyCollection<string> existingIds)
        {
            return MakeUniqueId($"{NormalizeBase(sourceId)}_copy", existingIds);
        }

        public static string MakeCopyDisplayName(string sourceDisplayName, IReadOnlyCollection<string> existingDisplayNames)
        {
            return MakeUniqueDisplayName($"{SafeDisplayName(sourceDisplayName, "Content")} Copy", existingDisplayNames);
        }

        public static string MakeUniqueDisplayName(string preferred, IReadOnlyCollection<string> existingDisplayNames)
        {
            string baseName = SafeDisplayName(preferred, "Content");
            if (!Contains(existingDisplayNames, baseName))
            {
                return baseName;
            }

            for (int suffix = 2; suffix < 10000; suffix++)
            {
                string candidate = $"{baseName} {suffix:000}";
                if (!Contains(existingDisplayNames, candidate))
                {
                    return candidate;
                }
            }

            throw new InvalidOperationException($"Could not generate a unique display name for '{baseName}'.");
        }

        public static string SafeDisplayName(string preferred, string fallback)
        {
            return string.IsNullOrWhiteSpace(preferred) ? fallback : preferred.Trim();
        }

        public static string StatSlug(StatId statId)
        {
            return ContentId.Normalize(StatDisplayName(statId));
        }

        public static string StatDisplayName(StatId statId)
        {
            return Enum.IsDefined(typeof(StatId), statId)
                ? StatMetadata.Get(statId).DisplayName
                : "Stat";
        }

        private static bool Contains(IReadOnlyCollection<string> values, string value)
        {
            if (values == null || string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            foreach (string existing in values)
            {
                if (string.Equals(existing, value, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
