using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TheCircussyOne.Content
{
    public enum WorldDecorationRole
    {
        GroundDecal,
        WallDecal,
        SmallProp,
        Landmark,
        PerimeterDressing,
        HangingDressing
    }

    [Flags]
    public enum WorldDecorationSurfaceMask
    {
        None = 0,
        OrganicGround = 1 << 0,
        TerraceTop = 1 << 1,
        Ramp = 1 << 2,
        Wall = 1 << 3,
        Perimeter = 1 << 4,
        HighGround = 1 << 5
    }

    public enum WorldDecorationShape
    {
        Crate,
        Barrel,
        RopeCoil,
        BrokenPlank,
        Hoop,
        SmallLamp,
        Sign
    }

    public readonly struct WorldDecorationSpec
    {
        public WorldDecorationSpec(
            string id,
            string displayName,
            WorldDecorationRole role,
            WorldDecorationSurfaceMask allowedSurfaces,
            WorldDecorationShape shape,
            float weight,
            int minCount,
            int maxCount,
            float scaleMin,
            float scaleMax,
            float rotationVariationDegrees,
            bool alignToSlope,
            float maxSlopeDegrees,
            float minDistanceBetweenDecorations,
            float minDistanceFromPlayerStart,
            float minDistanceFromRewards,
            float minDistanceFromStageDoor,
            Color primaryColor,
            Color secondaryColor,
            Color accentColor)
        {
            Id = string.IsNullOrWhiteSpace(id) ? "world_decoration" : id;
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? "World Decoration" : displayName;
            Role = role;
            AllowedSurfaces = allowedSurfaces == WorldDecorationSurfaceMask.None
                ? WorldDecorationSurfaceMask.OrganicGround | WorldDecorationSurfaceMask.TerraceTop
                : allowedSurfaces;
            Shape = shape;
            Weight = Mathf.Max(0f, weight);
            MinCount = Mathf.Max(0, minCount);
            MaxCount = Mathf.Max(MinCount, maxCount);
            ScaleMin = Mathf.Max(0.01f, Mathf.Min(scaleMin, scaleMax));
            ScaleMax = Mathf.Max(ScaleMin, Mathf.Max(scaleMin, scaleMax));
            RotationVariationDegrees = Mathf.Clamp(rotationVariationDegrees, 0f, 360f);
            AlignToSlope = alignToSlope;
            MaxSlopeDegrees = Mathf.Clamp(maxSlopeDegrees, 0f, 89f);
            MinDistanceBetweenDecorations = Mathf.Max(0f, minDistanceBetweenDecorations);
            MinDistanceFromPlayerStart = Mathf.Max(0f, minDistanceFromPlayerStart);
            MinDistanceFromRewards = Mathf.Max(0f, minDistanceFromRewards);
            MinDistanceFromStageDoor = Mathf.Max(0f, minDistanceFromStageDoor);
            PrimaryColor = primaryColor;
            SecondaryColor = secondaryColor;
            AccentColor = accentColor;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public WorldDecorationRole Role { get; }
        public WorldDecorationSurfaceMask AllowedSurfaces { get; }
        public WorldDecorationShape Shape { get; }
        public float Weight { get; }
        public int MinCount { get; }
        public int MaxCount { get; }
        public float ScaleMin { get; }
        public float ScaleMax { get; }
        public float RotationVariationDegrees { get; }
        public bool AlignToSlope { get; }
        public float MaxSlopeDegrees { get; }
        public float MinDistanceBetweenDecorations { get; }
        public float MinDistanceFromPlayerStart { get; }
        public float MinDistanceFromRewards { get; }
        public float MinDistanceFromStageDoor { get; }
        public Color PrimaryColor { get; }
        public Color SecondaryColor { get; }
        public Color AccentColor { get; }
    }

    [CreateAssetMenu(menuName = "The Circussy One/Content/World Decoration Definition", fileName = "WorldDecorationDefinition")]
    public sealed class WorldDecorationDefinition : SerializedScriptableObject, IActivatableContentDefinition
    {
        private const string Tabs = "World Decoration";

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Content"), LabelWidth(150)]
        public string decorationId = "crate";

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Content"), LabelWidth(150)]
        public string displayName = "Crate";

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Content"), LabelWidth(150)]
        public bool isActive = true;

        [TabGroup(Tabs, "Placement"), BoxGroup(Tabs + "/Placement/Role"), LabelWidth(150), EnumToggleButtons]
        public WorldDecorationRole role = WorldDecorationRole.SmallProp;

        [TabGroup(Tabs, "Placement"), BoxGroup(Tabs + "/Placement/Role"), LabelWidth(150)]
        public WorldDecorationSurfaceMask allowedSurfaces = WorldDecorationSurfaceMask.OrganicGround | WorldDecorationSurfaceMask.TerraceTop;

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Shape"), LabelWidth(150), EnumToggleButtons]
        public WorldDecorationShape shape = WorldDecorationShape.Crate;

        [TabGroup(Tabs, "Placement"), BoxGroup(Tabs + "/Placement/Density"), LabelWidth(150), Min(0f)]
        public float weight = 1f;

        [TabGroup(Tabs, "Placement"), BoxGroup(Tabs + "/Placement/Density"), LabelWidth(150), Min(0)]
        public int minCount = 8;

        [TabGroup(Tabs, "Placement"), BoxGroup(Tabs + "/Placement/Density"), LabelWidth(150), Min(0)]
        public int maxCount = 16;

        [TabGroup(Tabs, "Placement"), BoxGroup(Tabs + "/Placement/Transform"), LabelWidth(150), Min(0.01f)]
        public float scaleMin = 0.85f;

        [TabGroup(Tabs, "Placement"), BoxGroup(Tabs + "/Placement/Transform"), LabelWidth(150), Min(0.01f)]
        public float scaleMax = 1.25f;

        [TabGroup(Tabs, "Placement"), BoxGroup(Tabs + "/Placement/Transform"), LabelWidth(150), Range(0f, 360f)]
        public float rotationVariationDegrees = 360f;

        [TabGroup(Tabs, "Placement"), BoxGroup(Tabs + "/Placement/Transform"), LabelWidth(150)]
        public bool alignToSlope = true;

        [TabGroup(Tabs, "Placement"), BoxGroup(Tabs + "/Placement/Rules"), LabelWidth(150), Range(0f, 89f)]
        public float maxSlopeDegrees = 32f;

        [TabGroup(Tabs, "Placement"), BoxGroup(Tabs + "/Placement/Rules"), LabelWidth(150), Min(0f), SuffixLabel("u")]
        public float minDistanceBetweenDecorations = 8f;

        [TabGroup(Tabs, "Placement"), BoxGroup(Tabs + "/Placement/Rules"), LabelWidth(150), Min(0f), SuffixLabel("u")]
        public float minDistanceFromPlayerStart = 22f;

        [TabGroup(Tabs, "Placement"), BoxGroup(Tabs + "/Placement/Rules"), LabelWidth(150), Min(0f), SuffixLabel("u")]
        public float minDistanceFromRewards = 9f;

        [TabGroup(Tabs, "Placement"), BoxGroup(Tabs + "/Placement/Rules"), LabelWidth(150), Min(0f), SuffixLabel("u")]
        public float minDistanceFromStageDoor = 18f;

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Colors"), LabelWidth(150)]
        public Color primaryColor = new(0.42f, 0.22f, 0.10f, 1f);

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Colors"), LabelWidth(150)]
        public Color secondaryColor = new(0.18f, 0.045f, 0.035f, 1f);

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Colors"), LabelWidth(150)]
        public Color accentColor = new(0.78f, 0.52f, 0.14f, 1f);

        [ShowInInspector, ReadOnly, LabelText("Computed Tags")]
        private string ComputedTags => ComputedContentTagRules.Format(Tags);

        public string Id => decorationId;
        public string DisplayName => displayName;
        public ContentTagSet Tags => TagsFor(role);
        public bool IsActive => isActive;

        public WorldDecorationSpec ToSpec()
        {
            return new WorldDecorationSpec(
                decorationId,
                displayName,
                role,
                allowedSurfaces,
                shape,
                weight,
                minCount,
                maxCount,
                scaleMin,
                scaleMax,
                rotationVariationDegrees,
                alignToSlope,
                maxSlopeDegrees,
                minDistanceBetweenDecorations,
                minDistanceFromPlayerStart,
                minDistanceFromRewards,
                minDistanceFromStageDoor,
                primaryColor,
                secondaryColor,
                accentColor);
        }

        public bool EnsureWorkflowDefaults()
        {
            bool changed = false;
            changed |= EnsureString(ref decorationId, $"{shape.ToString().ToLowerInvariant()}_decoration");
            changed |= EnsureString(ref displayName, "World Decoration");
            if (maxCount < minCount)
            {
                maxCount = minCount;
                changed = true;
            }

            if (scaleMin <= 0f)
            {
                scaleMin = 0.85f;
                changed = true;
            }

            if (scaleMax < scaleMin)
            {
                scaleMax = scaleMin;
                changed = true;
            }

            return changed;
        }

        private static ContentTagSet TagsFor(WorldDecorationRole role)
        {
            return role switch
            {
                WorldDecorationRole.GroundDecal => ContentTagSet.With(ContentTag.Utility),
                WorldDecorationRole.WallDecal => ContentTagSet.With(ContentTag.Utility),
                WorldDecorationRole.SmallProp => ContentTagSet.With(ContentTag.Utility),
                WorldDecorationRole.Landmark => ContentTagSet.With(ContentTag.Utility),
                WorldDecorationRole.PerimeterDressing => ContentTagSet.With(ContentTag.Utility),
                WorldDecorationRole.HangingDressing => ContentTagSet.With(ContentTag.Utility),
                _ => ContentTagSet.With(ContentTag.Utility)
            };
        }

        private static bool EnsureString(ref string value, string fallback)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            value = fallback;
            return true;
        }
    }

    [CreateAssetMenu(menuName = "The Circussy One/Content/World Decoration Catalog", fileName = "WorldDecorationCatalog")]
    public sealed class WorldDecorationCatalog : SerializedScriptableObject, IContentCatalog<WorldDecorationDefinition>
    {
        private const string Tabs = "World Decoration Catalog";

        [TabGroup(Tabs, "Definitions"), BoxGroup(Tabs + "/Definitions/Decorations"), LabelWidth(160), AssetSelector]
        public List<WorldDecorationDefinition> decorations = new();

        public IReadOnlyList<WorldDecorationDefinition> Decorations => decorations;
        public IReadOnlyList<WorldDecorationDefinition> Definitions => decorations;

        public WorldDecorationSpec[] BuildRuntimeSpecs()
        {
            if (decorations == null || decorations.Count == 0)
            {
                return WorldDecorationDefaults.CreateSpecs();
            }

            var specs = new List<WorldDecorationSpec>(decorations.Count);
            for (int i = 0; i < decorations.Count; i++)
            {
                WorldDecorationDefinition definition = decorations[i];
                if (definition == null || !definition.IsActive)
                {
                    continue;
                }

                specs.Add(definition.ToSpec());
            }

            return specs.Count > 0 ? specs.ToArray() : WorldDecorationDefaults.CreateSpecs();
        }

        public bool EnsureWorkflowDefaults(params WorldDecorationDefinition[] starterDecorations)
        {
            bool changed = false;
            decorations ??= new List<WorldDecorationDefinition>();
            if (starterDecorations == null)
            {
                return changed;
            }

            for (int i = 0; i < starterDecorations.Length; i++)
            {
                WorldDecorationDefinition definition = starterDecorations[i];
                if (definition == null || decorations.Contains(definition))
                {
                    continue;
                }

                decorations.Add(definition);
                changed = true;
            }

            return changed;
        }

        public List<ContentValidationIssue> ValidateContent()
        {
            var issues = ContentCatalogRules.ValidateDefinitions(Definitions);
            if (Definitions == null)
            {
                return issues;
            }

            for (int i = 0; i < Definitions.Count; i++)
            {
                WorldDecorationDefinition definition = Definitions[i];
                if (definition == null || !definition.IsActive)
                {
                    continue;
                }

                if (definition.weight <= 0f)
                {
                    AddIssue(issues, "world-decoration.invalid-weight", ContentValidationSeverity.Warning, definition, "has no positive spawn weight.");
                }

                if (definition.maxCount < definition.minCount)
                {
                    AddIssue(issues, "world-decoration.invalid-count-range", ContentValidationSeverity.Error, definition, "has max count below min count.");
                }

                if (definition.scaleMin <= 0f || definition.scaleMax <= 0f)
                {
                    AddIssue(issues, "world-decoration.invalid-scale", ContentValidationSeverity.Error, definition, "has non-positive scale.");
                }
            }

            return issues;
        }

        private static void AddIssue(
            List<ContentValidationIssue> issues,
            string code,
            ContentValidationSeverity severity,
            WorldDecorationDefinition definition,
            string message)
        {
            issues.Add(new ContentValidationIssue(
                code,
                severity,
                $"World decoration '{definition.Id}' {message}",
                definition.Id));
        }
    }

    public static class WorldDecorationDefaults
    {
        public static WorldDecorationSpec[] CreateSpecs()
        {
            return new[]
            {
                new WorldDecorationSpec(
                    "crate_scatter",
                    "Crate Scatter",
                    WorldDecorationRole.SmallProp,
                    WorldDecorationSurfaceMask.OrganicGround | WorldDecorationSurfaceMask.TerraceTop | WorldDecorationSurfaceMask.HighGround,
                    WorldDecorationShape.Crate,
                    1f,
                    36,
                    58,
                    0.75f,
                    1.35f,
                    360f,
                    true,
                    28f,
                    9f,
                    24f,
                    9f,
                    18f,
                    new Color(0.45f, 0.24f, 0.10f, 1f),
                    new Color(0.18f, 0.07f, 0.035f, 1f),
                    new Color(0.75f, 0.48f, 0.16f, 1f)),
                new WorldDecorationSpec(
                    "barrel_scatter",
                    "Barrel Scatter",
                    WorldDecorationRole.SmallProp,
                    WorldDecorationSurfaceMask.OrganicGround | WorldDecorationSurfaceMask.TerraceTop,
                    WorldDecorationShape.Barrel,
                    0.8f,
                    28,
                    44,
                    0.75f,
                    1.25f,
                    360f,
                    true,
                    24f,
                    9f,
                    24f,
                    9f,
                    18f,
                    new Color(0.42f, 0.17f, 0.08f, 1f),
                    new Color(0.09f, 0.06f, 0.045f, 1f),
                    new Color(0.65f, 0.42f, 0.12f, 1f)),
                new WorldDecorationSpec(
                    "rope_coils",
                    "Rope Coils",
                    WorldDecorationRole.SmallProp,
                    WorldDecorationSurfaceMask.OrganicGround | WorldDecorationSurfaceMask.TerraceTop | WorldDecorationSurfaceMask.Ramp,
                    WorldDecorationShape.RopeCoil,
                    0.7f,
                    24,
                    36,
                    0.75f,
                    1.20f,
                    360f,
                    true,
                    30f,
                    8f,
                    20f,
                    8f,
                    16f,
                    new Color(0.62f, 0.44f, 0.25f, 1f),
                    new Color(0.26f, 0.16f, 0.08f, 1f),
                    new Color(0.78f, 0.56f, 0.30f, 1f)),
                new WorldDecorationSpec(
                    "broken_planks",
                    "Broken Planks",
                    WorldDecorationRole.SmallProp,
                    WorldDecorationSurfaceMask.OrganicGround | WorldDecorationSurfaceMask.TerraceTop | WorldDecorationSurfaceMask.Ramp,
                    WorldDecorationShape.BrokenPlank,
                    0.85f,
                    30,
                    46,
                    0.8f,
                    1.45f,
                    360f,
                    true,
                    32f,
                    8f,
                    20f,
                    8f,
                    16f,
                    new Color(0.34f, 0.17f, 0.07f, 1f),
                    new Color(0.12f, 0.06f, 0.035f, 1f),
                    new Color(0.60f, 0.36f, 0.14f, 1f)),
                new WorldDecorationSpec(
                    "fallen_hoops",
                    "Fallen Hoops",
                    WorldDecorationRole.SmallProp,
                    WorldDecorationSurfaceMask.OrganicGround | WorldDecorationSurfaceMask.TerraceTop,
                    WorldDecorationShape.Hoop,
                    0.45f,
                    12,
                    22,
                    0.8f,
                    1.25f,
                    360f,
                    true,
                    24f,
                    10f,
                    24f,
                    10f,
                    18f,
                    new Color(0.62f, 0.04f, 0.035f, 1f),
                    new Color(0.08f, 0.11f, 0.12f, 1f),
                    new Color(0.74f, 0.48f, 0.12f, 1f)),
                new WorldDecorationSpec(
                    "small_lamps",
                    "Small Lamps",
                    WorldDecorationRole.SmallProp,
                    WorldDecorationSurfaceMask.TerraceTop | WorldDecorationSurfaceMask.HighGround,
                    WorldDecorationShape.SmallLamp,
                    0.4f,
                    10,
                    18,
                    0.8f,
                    1.25f,
                    360f,
                    true,
                    20f,
                    12f,
                    28f,
                    11f,
                    18f,
                    new Color(0.10f, 0.07f, 0.045f, 1f),
                    new Color(0.54f, 0.34f, 0.12f, 1f),
                    new Color(1f, 0.74f, 0.28f, 1f)),
                new WorldDecorationSpec(
                    "small_signs",
                    "Small Signs",
                    WorldDecorationRole.SmallProp,
                    WorldDecorationSurfaceMask.OrganicGround | WorldDecorationSurfaceMask.TerraceTop | WorldDecorationSurfaceMask.Perimeter,
                    WorldDecorationShape.Sign,
                    0.45f,
                    12,
                    20,
                    0.8f,
                    1.3f,
                    360f,
                    true,
                    22f,
                    12f,
                    26f,
                    12f,
                    18f,
                    new Color(0.44f, 0.035f, 0.035f, 1f),
                    new Color(0.14f, 0.09f, 0.04f, 1f),
                    new Color(0.78f, 0.50f, 0.12f, 1f))
            };
        }
    }
}
