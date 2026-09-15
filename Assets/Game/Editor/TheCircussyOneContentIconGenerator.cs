using System;
using System.Collections.Generic;
using System.IO;
using TheCircussyOne.Content;
using UnityEditor;
using UnityEngine;

public static class TheCircussyOneContentIconGenerator
{
    public const int PlaceholderSize = 256;
    public const int RecommendedMaxSize = 512;

    private const string MenuPath = "Tools/The Circussy One/Content/Generate Missing Placeholder Icons";

    private enum IconDomain
    {
        Performer,
        Weapon,
        Talent,
        Item
    }

    [MenuItem(MenuPath)]
    public static void GenerateMissingPlaceholderIcons()
    {
        int assigned = GenerateMissingForAllContent();
        Debug.Log($"Generated/assigned {assigned} missing content icon reference(s).");
    }

    public static int GenerateMissingForAllContent()
    {
        int assigned = 0;
        assigned += AssignWeaponIcons(TheCircussyOneConfigRepository.GetOrCreateWeaponCatalog());
        assigned += AssignPerformerIcons(TheCircussyOneConfigRepository.GetOrCreatePerformerCatalog());
        assigned += AssignTalentIcons(TheCircussyOneConfigRepository.GetOrCreateTalentCatalog());
        assigned += AssignItemIcons(TheCircussyOneConfigRepository.GetOrCreateItemCatalog());
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return assigned;
    }

    public static int GenerateMissingForSelected(UnityEngine.Object asset)
    {
        int assigned = asset switch
        {
            WeaponDefinition weapon => AssignWeaponIcon(weapon),
            PerformerDefinition performer => AssignPerformerIcon(performer),
            TalentDefinition talent => AssignTalentIcon(talent),
            ItemDefinition item => AssignItemIcon(item),
            UpgradeDefinition upgrade => AssignUpgradeFallback(upgrade),
            _ => 0
        };

        if (assigned > 0)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        return assigned;
    }

    public static Texture2D CreatePlaceholderTextureForTests(string id, Color primary, IconDomainForTests domain)
    {
        return CreateTexture(id, primary, (IconDomain)domain);
    }

    private static int AssignWeaponIcons(WeaponCatalog catalog)
    {
        int assigned = 0;
        IReadOnlyList<WeaponDefinition> weapons = catalog?.AvailableWeapons;
        if (weapons == null)
        {
            return assigned;
        }

        for (int i = 0; i < weapons.Count; i++)
        {
            assigned += AssignWeaponIcon(weapons[i]);
        }

        return assigned;
    }

    private static int AssignPerformerIcons(PerformerCatalog catalog)
    {
        int assigned = 0;
        IReadOnlyList<PerformerDefinition> performers = catalog?.Performers;
        if (performers == null)
        {
            return assigned;
        }

        for (int i = 0; i < performers.Count; i++)
        {
            assigned += AssignPerformerIcon(performers[i]);
        }

        return assigned;
    }

    private static int AssignTalentIcons(TalentCatalog catalog)
    {
        int assigned = 0;
        IReadOnlyList<TalentDefinition> talents = catalog?.Talents;
        if (talents == null)
        {
            return assigned;
        }

        for (int i = 0; i < talents.Count; i++)
        {
            assigned += AssignTalentIcon(talents[i]);
        }

        return assigned;
    }

    private static int AssignItemIcons(ItemCatalog catalog)
    {
        int assigned = 0;
        IReadOnlyList<ItemDefinition> items = catalog?.Items;
        if (items == null)
        {
            return assigned;
        }

        for (int i = 0; i < items.Count; i++)
        {
            assigned += AssignItemIcon(items[i]);
        }

        return assigned;
    }

    private static int AssignWeaponIcon(WeaponDefinition weapon)
    {
        if (weapon == null || weapon.iconSprite != null)
        {
            return 0;
        }

        weapon.iconSprite = LoadOrCreateSprite("Weapons", weapon.Id, weapon.projectilePrimaryColor, IconDomain.Weapon);
        EditorUtility.SetDirty(weapon);
        return weapon.iconSprite != null ? 1 : 0;
    }

    private static int AssignPerformerIcon(PerformerDefinition performer)
    {
        if (performer == null || performer.portraitSprite != null)
        {
            return 0;
        }

        performer.portraitSprite = LoadOrCreateSprite("Performers", performer.Id, performer.portraitColor, IconDomain.Performer);
        EditorUtility.SetDirty(performer);
        return performer.portraitSprite != null ? 1 : 0;
    }

    private static int AssignTalentIcon(TalentDefinition talent)
    {
        if (talent == null || talent.iconSprite != null)
        {
            return 0;
        }

        if (talent.poolKind == TalentPoolKind.PerformerSpecific)
        {
            if (talent.performerDefinition != null && talent.performerDefinition.portraitSprite == null)
            {
                return AssignPerformerIcon(talent.performerDefinition);
            }

            return 0;
        }

        talent.iconSprite = LoadOrCreateSprite("Talents", talent.Id, talent.iconColor, IconDomain.Talent);
        EditorUtility.SetDirty(talent);
        return talent.iconSprite != null ? 1 : 0;
    }

    private static int AssignItemIcon(ItemDefinition item)
    {
        if (item == null || item.iconSprite != null)
        {
            return 0;
        }

        item.iconSprite = LoadOrCreateSprite("Items", item.Id, item.iconColor, IconDomain.Item);
        EditorUtility.SetDirty(item);
        return item.iconSprite != null ? 1 : 0;
    }

    private static int AssignUpgradeFallback(UpgradeDefinition upgrade)
    {
        if (upgrade == null || upgrade.iconSpriteOverride != null)
        {
            return 0;
        }

        if (upgrade.weaponDefinition != null && upgrade.weaponDefinition.iconSprite == null)
        {
            return AssignWeaponIcon(upgrade.weaponDefinition);
        }

        return 0;
    }

    private static Sprite LoadOrCreateSprite(string folder, string id, Color primary, IconDomain domain)
    {
        string safeId = string.IsNullOrWhiteSpace(id) ? "content_icon" : ContentId.Normalize(id);
        string path = $"{TheCircussyOneAssetPaths.GeneratedContentIconFolder}/{folder}/{safeId}.png";
        EnsureFolder(Path.GetDirectoryName(path)?.Replace("\\", "/"));
        if (!File.Exists(path))
        {
            Texture2D texture = CreateTexture(safeId, primary, domain);
            File.WriteAllBytes(path, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
        }

        AssetDatabase.ImportAsset(path);
        ConfigureImporter(path);
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static void ConfigureImporter(string path)
    {
        if (AssetImporter.GetAtPath(path) is not TextureImporter importer)
        {
            return;
        }

        bool changed = false;
        if (importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            changed = true;
        }

        if (importer.spriteImportMode != SpriteImportMode.Single)
        {
            importer.spriteImportMode = SpriteImportMode.Single;
            changed = true;
        }

        var textureSettings = new TextureImporterSettings();
        importer.ReadTextureSettings(textureSettings);
        if (textureSettings.spriteMeshType != SpriteMeshType.FullRect)
        {
            textureSettings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(textureSettings);
            changed = true;
        }

        if (importer.mipmapEnabled)
        {
            importer.mipmapEnabled = false;
            changed = true;
        }

        if (!importer.alphaIsTransparency)
        {
            importer.alphaIsTransparency = true;
            changed = true;
        }

        if (importer.wrapMode != TextureWrapMode.Clamp)
        {
            importer.wrapMode = TextureWrapMode.Clamp;
            changed = true;
        }

        if (importer.filterMode != FilterMode.Bilinear)
        {
            importer.filterMode = FilterMode.Bilinear;
            changed = true;
        }

        if (importer.maxTextureSize != RecommendedMaxSize)
        {
            importer.maxTextureSize = RecommendedMaxSize;
            changed = true;
        }

        if (!Mathf.Approximately(importer.spritePixelsPerUnit, PlaceholderSize))
        {
            importer.spritePixelsPerUnit = PlaceholderSize;
            changed = true;
        }

        if (changed)
        {
            importer.SaveAndReimport();
        }
    }

    private static Texture2D CreateTexture(string id, Color primary, IconDomain domain)
    {
        var texture = new Texture2D(PlaceholderSize, PlaceholderSize, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };

        Color clear = new(0f, 0f, 0f, 0f);
        for (int y = 0; y < PlaceholderSize; y++)
        {
            for (int x = 0; x < PlaceholderSize; x++)
            {
                texture.SetPixel(x, y, clear);
            }
        }

        Color accent = ReadableAccent(primary);
        DrawSoftDisc(texture, new Vector2(128f, 128f), 104f, primary.WithAlpha(0.18f));
        DrawRing(texture, new Vector2(128f, 128f), 102f, 8f, primary.WithAlpha(0.58f));
        switch (domain)
        {
            case IconDomain.Performer:
                DrawPerformer(texture, primary, accent);
                break;
            case IconDomain.Weapon:
                DrawWeapon(texture, id, primary, accent);
                break;
            case IconDomain.Item:
                DrawItem(texture, id, primary, accent);
                break;
            default:
                DrawTalent(texture, id, primary, accent);
                break;
        }

        texture.Apply(false, false);
        return texture;
    }

    private static void DrawPerformer(Texture2D texture, Color primary, Color accent)
    {
        DrawSoftDisc(texture, new Vector2(128f, 94f), 38f, accent.WithAlpha(0.95f));
        DrawCapsule(texture, new Vector2(128f, 166f), 72f, 56f, primary.WithAlpha(0.94f));
        DrawRing(texture, new Vector2(128f, 94f), 42f, 5f, Color.white.WithAlpha(0.5f));
    }

    private static void DrawWeapon(Texture2D texture, string id, Color primary, Color accent)
    {
        string value = id ?? string.Empty;
        if (value.Contains("cannon", StringComparison.OrdinalIgnoreCase))
        {
            DrawThickLine(texture, new Vector2(74f, 154f), new Vector2(178f, 112f), 34f, primary.WithAlpha(0.95f));
            DrawSoftDisc(texture, new Vector2(74f, 154f), 30f, accent.WithAlpha(0.94f));
            DrawSoftDisc(texture, new Vector2(180f, 111f), 22f, Color.white.WithAlpha(0.35f));
            return;
        }

        if (value.Contains("knife", StringComparison.OrdinalIgnoreCase))
        {
            DrawTriangle(texture, new Vector2(128f, 54f), new Vector2(110f, 174f), new Vector2(146f, 174f), accent.WithAlpha(0.95f));
            DrawTriangle(texture, new Vector2(68f, 82f), new Vector2(111f, 176f), new Vector2(132f, 158f), primary.WithAlpha(0.9f));
            DrawTriangle(texture, new Vector2(188f, 82f), new Vector2(124f, 158f), new Vector2(145f, 176f), primary.WithAlpha(0.9f));
            DrawSoftDisc(texture, new Vector2(128f, 180f), 18f, Color.white.WithAlpha(0.5f));
            return;
        }

        if (value.Contains("hoop", StringComparison.OrdinalIgnoreCase))
        {
            DrawRing(texture, new Vector2(128f, 128f), 66f, 16f, accent.WithAlpha(0.96f));
            DrawRing(texture, new Vector2(128f, 128f), 86f, 7f, primary.WithAlpha(0.7f));
            return;
        }

        if (value.Contains("spotlight", StringComparison.OrdinalIgnoreCase) || value.Contains("bolt", StringComparison.OrdinalIgnoreCase))
        {
            DrawTriangle(texture, new Vector2(122f, 42f), new Vector2(82f, 142f), new Vector2(127f, 132f), accent.WithAlpha(0.95f));
            DrawTriangle(texture, new Vector2(134f, 118f), new Vector2(174f, 218f), new Vector2(129f, 146f), primary.WithAlpha(0.95f));
            DrawRing(texture, new Vector2(128f, 128f), 82f, 6f, Color.white.WithAlpha(0.35f));
            return;
        }

        DrawSoftDisc(texture, new Vector2(100f, 116f), 34f, primary.WithAlpha(0.94f));
        DrawSoftDisc(texture, new Vector2(150f, 116f), 34f, accent.WithAlpha(0.94f));
        DrawSoftDisc(texture, new Vector2(126f, 164f), 34f, Color.white.WithAlpha(0.46f));
    }

    private static void DrawTalent(Texture2D texture, string id, Color primary, Color accent)
    {
        int hash = Mathf.Abs((id ?? string.Empty).GetHashCode());
        if (hash % 3 == 0)
        {
            DrawDiamond(texture, new Vector2(128f, 128f), 70f, accent.WithAlpha(0.92f));
            DrawRing(texture, new Vector2(128f, 128f), 48f, 7f, primary.WithAlpha(0.75f));
        }
        else if (hash % 3 == 1)
        {
            DrawStar(texture, new Vector2(128f, 128f), 34f, 78f, accent.WithAlpha(0.92f));
        }
        else
        {
            DrawRing(texture, new Vector2(128f, 128f), 76f, 12f, accent.WithAlpha(0.92f));
            DrawSoftDisc(texture, new Vector2(128f, 128f), 32f, primary.WithAlpha(0.9f));
        }
    }

    private static void DrawItem(Texture2D texture, string id, Color primary, Color accent)
    {
        int hash = Mathf.Abs((id ?? string.Empty).GetHashCode());
        if (hash % 2 == 0)
        {
            DrawDiamond(texture, new Vector2(128f, 128f), 74f, primary.WithAlpha(0.92f));
            DrawSoftDisc(texture, new Vector2(128f, 128f), 38f, accent.WithAlpha(0.85f));
        }
        else
        {
            DrawRoundedBox(texture, new Rect(70f, 76f, 116f, 104f), 22f, primary.WithAlpha(0.92f));
            DrawRing(texture, new Vector2(128f, 128f), 46f, 6f, accent.WithAlpha(0.8f));
        }
    }

    private static void DrawSoftDisc(Texture2D texture, Vector2 center, float radius, Color color)
    {
        int minX = Mathf.Max(0, Mathf.FloorToInt(center.x - radius - 2f));
        int maxX = Mathf.Min(PlaceholderSize - 1, Mathf.CeilToInt(center.x + radius + 2f));
        int minY = Mathf.Max(0, Mathf.FloorToInt(center.y - radius - 2f));
        int maxY = Mathf.Min(PlaceholderSize - 1, Mathf.CeilToInt(center.y + radius + 2f));
        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                float alpha = Mathf.Clamp01(radius + 1.5f - distance);
                if (alpha > 0f)
                {
                    Blend(texture, x, y, color.WithAlpha(color.a * alpha));
                }
            }
        }
    }

    private static void DrawRing(Texture2D texture, Vector2 center, float radius, float thickness, Color color)
    {
        int minX = Mathf.Max(0, Mathf.FloorToInt(center.x - radius - thickness));
        int maxX = Mathf.Min(PlaceholderSize - 1, Mathf.CeilToInt(center.x + radius + thickness));
        int minY = Mathf.Max(0, Mathf.FloorToInt(center.y - radius - thickness));
        int maxY = Mathf.Min(PlaceholderSize - 1, Mathf.CeilToInt(center.y + radius + thickness));
        float half = Mathf.Max(0.5f, thickness * 0.5f);
        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                float alpha = Mathf.Clamp01(half + 1.5f - Mathf.Abs(distance - radius));
                if (alpha > 0f)
                {
                    Blend(texture, x, y, color.WithAlpha(color.a * alpha));
                }
            }
        }
    }

    private static void DrawCapsule(Texture2D texture, Vector2 center, float width, float height, Color color)
    {
        DrawRoundedBox(texture, new Rect(center.x - width * 0.5f, center.y - height * 0.5f, width, height), width * 0.35f, color);
    }

    private static void DrawRoundedBox(Texture2D texture, Rect rect, float radius, Color color)
    {
        int minX = Mathf.Max(0, Mathf.FloorToInt(rect.xMin));
        int maxX = Mathf.Min(PlaceholderSize - 1, Mathf.CeilToInt(rect.xMax));
        int minY = Mathf.Max(0, Mathf.FloorToInt(rect.yMin));
        int maxY = Mathf.Min(PlaceholderSize - 1, Mathf.CeilToInt(rect.yMax));
        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                float dx = Mathf.Max(rect.xMin + radius - x, 0f, x - (rect.xMax - radius));
                float dy = Mathf.Max(rect.yMin + radius - y, 0f, y - (rect.yMax - radius));
                if (dx * dx + dy * dy <= radius * radius)
                {
                    Blend(texture, x, y, color);
                }
            }
        }
    }

    private static void DrawDiamond(Texture2D texture, Vector2 center, float radius, Color color)
    {
        int minX = Mathf.Max(0, Mathf.FloorToInt(center.x - radius));
        int maxX = Mathf.Min(PlaceholderSize - 1, Mathf.CeilToInt(center.x + radius));
        int minY = Mathf.Max(0, Mathf.FloorToInt(center.y - radius));
        int maxY = Mathf.Min(PlaceholderSize - 1, Mathf.CeilToInt(center.y + radius));
        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                if (Mathf.Abs(x + 0.5f - center.x) + Mathf.Abs(y + 0.5f - center.y) <= radius)
                {
                    Blend(texture, x, y, color);
                }
            }
        }
    }

    private static void DrawTriangle(Texture2D texture, Vector2 a, Vector2 b, Vector2 c, Color color)
    {
        int minX = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(a.x, Mathf.Min(b.x, c.x))));
        int maxX = Mathf.Min(PlaceholderSize - 1, Mathf.CeilToInt(Mathf.Max(a.x, Mathf.Max(b.x, c.x))));
        int minY = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(a.y, Mathf.Min(b.y, c.y))));
        int maxY = Mathf.Min(PlaceholderSize - 1, Mathf.CeilToInt(Mathf.Max(a.y, Mathf.Max(b.y, c.y))));
        float area = Edge(a, b, c);
        if (Mathf.Approximately(area, 0f))
        {
            return;
        }

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                Vector2 p = new(x + 0.5f, y + 0.5f);
                float w0 = Edge(b, c, p);
                float w1 = Edge(c, a, p);
                float w2 = Edge(a, b, p);
                if ((w0 >= 0f && w1 >= 0f && w2 >= 0f) || (w0 <= 0f && w1 <= 0f && w2 <= 0f))
                {
                    Blend(texture, x, y, color);
                }
            }
        }
    }

    private static void DrawStar(Texture2D texture, Vector2 center, float innerRadius, float outerRadius, Color color)
    {
        Vector2[] points = new Vector2[10];
        for (int i = 0; i < points.Length; i++)
        {
            float radius = i % 2 == 0 ? outerRadius : innerRadius;
            float angle = Mathf.PI * 0.5f + i * Mathf.PI * 2f / points.Length;
            points[i] = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        }

        for (int i = 0; i < points.Length; i++)
        {
            DrawTriangle(texture, center, points[i], points[(i + 1) % points.Length], color);
        }
    }

    private static void DrawThickLine(Texture2D texture, Vector2 start, Vector2 end, float thickness, Color color)
    {
        float radius = thickness * 0.5f;
        int minX = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(start.x, end.x) - radius));
        int maxX = Mathf.Min(PlaceholderSize - 1, Mathf.CeilToInt(Mathf.Max(start.x, end.x) + radius));
        int minY = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(start.y, end.y) - radius));
        int maxY = Mathf.Min(PlaceholderSize - 1, Mathf.CeilToInt(Mathf.Max(start.y, end.y) + radius));
        Vector2 line = end - start;
        float lengthSq = Mathf.Max(0.001f, line.sqrMagnitude);
        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                Vector2 p = new(x + 0.5f, y + 0.5f);
                float t = Mathf.Clamp01(Vector2.Dot(p - start, line) / lengthSq);
                float distance = Vector2.Distance(p, start + line * t);
                float alpha = Mathf.Clamp01(radius + 1.5f - distance);
                if (alpha > 0f)
                {
                    Blend(texture, x, y, color.WithAlpha(color.a * alpha));
                }
            }
        }
    }

    private static float Edge(Vector2 a, Vector2 b, Vector2 c)
    {
        return (c.x - a.x) * (b.y - a.y) - (c.y - a.y) * (b.x - a.x);
    }

    private static void Blend(Texture2D texture, int x, int y, Color source)
    {
        Color destination = texture.GetPixel(x, y);
        float sourceAlpha = Mathf.Clamp01(source.a);
        float outAlpha = sourceAlpha + destination.a * (1f - sourceAlpha);
        if (outAlpha <= 0f)
        {
            texture.SetPixel(x, y, new Color(0f, 0f, 0f, 0f));
            return;
        }

        Color result = (source * sourceAlpha + destination * destination.a * (1f - sourceAlpha)) / outAlpha;
        result.a = outAlpha;
        texture.SetPixel(x, y, result);
    }

    private static Color ReadableAccent(Color primary)
    {
        Color.RGBToHSV(primary, out float h, out float s, out float v);
        return Color.HSVToRGB((h + 0.09f) % 1f, Mathf.Clamp01(s * 0.75f + 0.22f), Mathf.Clamp01(v * 0.75f + 0.25f));
    }

    private static Color WithAlpha(this Color color, float alpha)
    {
        color.a = Mathf.Clamp01(alpha);
        return color;
    }

    private static void EnsureFolder(string folder)
    {
        if (string.IsNullOrWhiteSpace(folder) || AssetDatabase.IsValidFolder(folder))
        {
            return;
        }

        string parent = Path.GetDirectoryName(folder)?.Replace("\\", "/");
        EnsureFolder(parent);
        string name = Path.GetFileName(folder);
        if (!string.IsNullOrWhiteSpace(parent) && !string.IsNullOrWhiteSpace(name) && !AssetDatabase.IsValidFolder(folder))
        {
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}

public enum IconDomainForTests
{
    Performer = 0,
    Weapon = 1,
    Talent = 2,
    Item = 3
}
