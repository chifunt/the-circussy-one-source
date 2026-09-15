using System;
using System.Collections.Generic;

namespace TheCircussyOne.Content
{
    [Serializable]
    public sealed class ContentTagSet
    {
        public List<ContentTag> values = new();

        public ContentTagSet()
        {
        }

        public ContentTagSet(params ContentTag[] tags)
        {
            if (tags != null)
            {
                values.AddRange(tags);
            }

            RemoveDuplicates();
        }

        public IReadOnlyList<ContentTag> Values => values;

        public static ContentTagSet With(params ContentTag[] tags)
        {
            return new ContentTagSet(tags);
        }

        public bool HasTag(ContentTag tag)
        {
            return values != null && values.Contains(tag);
        }

        public bool HasAny(params ContentTag[] tags)
        {
            if (tags == null || values == null)
            {
                return false;
            }

            for (int i = 0; i < tags.Length; i++)
            {
                if (values.Contains(tags[i]))
                {
                    return true;
                }
            }

            return false;
        }

        public bool Ensure(params ContentTag[] defaultTags)
        {
            bool changed = false;
            values ??= new List<ContentTag>();
            changed |= RemoveDuplicates();
            if (values.Count == 0 && defaultTags != null)
            {
                values.AddRange(defaultTags);
                changed = true;
            }

            changed |= RemoveDuplicates();
            return changed;
        }

        public bool RemoveDuplicates()
        {
            if (values == null)
            {
                values = new List<ContentTag>();
                return true;
            }

            bool changed = false;
            var seen = new HashSet<ContentTag>();
            for (int i = values.Count - 1; i >= 0; i--)
            {
                if (seen.Add(values[i]))
                {
                    continue;
                }

                values.RemoveAt(i);
                changed = true;
            }

            values.Sort();
            return changed;
        }
    }
}
