using System.Collections.Generic;
using UnityEngine;

namespace TheCircussyOne.Visuals
{
    public readonly struct InteractableSelectionOutlineSettings
    {
        public InteractableSelectionOutlineSettings(Color color, float widthPixels)
        {
            Color = color;
            WidthPixels = Mathf.Max(0f, widthPixels);
        }

        public Color Color { get; }
        public float WidthPixels { get; }
    }

    public static class InteractableSelectionOutlineRegistry
    {
        private sealed class Entry
        {
            public object Owner;
            public Renderer[] Renderers;
            public InteractableSelectionOutlineSettings Settings;
        }

        private static readonly List<Entry> Entries = new();

        public static int Count
        {
            get
            {
                PruneInactiveEntries();
                return Entries.Count;
            }
        }

        public static void Register(object owner, Renderer[] renderers, Color color, float widthPixels)
        {
            if (owner == null)
            {
                return;
            }

            if (renderers == null || renderers.Length == 0 || widthPixels <= 0f)
            {
                Unregister(owner);
                return;
            }

            int index = IndexOf(owner);
            Entry entry;
            if (index >= 0)
            {
                entry = Entries[index];
            }
            else
            {
                entry = new Entry { Owner = owner };
                Entries.Add(entry);
            }

            entry.Renderers = renderers;
            entry.Settings = new InteractableSelectionOutlineSettings(color, widthPixels);
        }

        public static void Unregister(object owner)
        {
            int index = IndexOf(owner);
            if (index >= 0)
            {
                Entries.RemoveAt(index);
            }
        }

        public static void Clear()
        {
            Entries.Clear();
        }

        public static void CopyRenderers(List<Renderer> results)
        {
            if (results == null)
            {
                return;
            }

            PruneInactiveEntries();
            for (int entryIndex = Entries.Count - 1; entryIndex >= 0; entryIndex--)
            {
                Entry entry = Entries[entryIndex];
                for (int rendererIndex = 0; rendererIndex < entry.Renderers.Length; rendererIndex++)
                {
                    Renderer renderer = entry.Renderers[rendererIndex];
                    if (renderer != null && renderer.enabled && renderer.gameObject.activeInHierarchy)
                    {
                        results.Add(renderer);
                    }
                }
            }
        }

        public static bool TryGetCompositeSettings(out InteractableSelectionOutlineSettings settings)
        {
            settings = default;
            PruneInactiveEntries();
            if (Entries.Count == 0)
            {
                return false;
            }

            Color color = Color.white;
            float widthPixels = 0f;
            bool hasColor = false;
            for (int i = Entries.Count - 1; i >= 0; i--)
            {
                Entry entry = Entries[i];
                if (entry.Owner == null || entry.Settings.WidthPixels <= 0f)
                {
                    Entries.RemoveAt(i);
                    continue;
                }

                if (!hasColor)
                {
                    color = entry.Settings.Color;
                    hasColor = true;
                }

                widthPixels = Mathf.Max(widthPixels, entry.Settings.WidthPixels);
            }

            settings = new InteractableSelectionOutlineSettings(color, widthPixels);
            return hasColor && widthPixels > 0f;
        }

        private static int IndexOf(object owner)
        {
            for (int i = 0; i < Entries.Count; i++)
            {
                if (ReferenceEquals(Entries[i].Owner, owner))
                {
                    return i;
                }
            }

            return -1;
        }

        private static void PruneInactiveEntries()
        {
            for (int entryIndex = Entries.Count - 1; entryIndex >= 0; entryIndex--)
            {
                Entry entry = Entries[entryIndex];
                if (entry.Owner == null || entry.Renderers == null || entry.Settings.WidthPixels <= 0f || !HasActiveRenderer(entry.Renderers))
                {
                    Entries.RemoveAt(entryIndex);
                }
            }
        }

        private static bool HasActiveRenderer(Renderer[] renderers)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer != null && renderer.enabled && renderer.gameObject.activeInHierarchy)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
