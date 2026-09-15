using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TheCircussyOne.Content
{
    [Serializable]
    public sealed class WorldRewardPlacementDefinition : IActivatableContentDefinition
    {
        [HorizontalGroup("Identity"), LabelWidth(130), ValidateInput(nameof(HasPlacementId), "Placement id is required.")]
        public string placementId = "locked_chest_west";

        [HorizontalGroup("Identity"), LabelWidth(130), ValidateInput(nameof(HasDisplayName), "Display name is required.")]
        public string displayName = "Locked Chest West";

        [LabelWidth(130)]
        public bool isActive = true;

        [LabelWidth(130), EnumToggleButtons]
        public WorldRewardPlacementKind kind = WorldRewardPlacementKind.Chest;

        [LabelWidth(130)]
        [InfoBox("Content ID of the chest, Ticket deposit, snack prop, or future shrine spawned at this placement.")]
        public string targetContentId = "locked_chest";

        [LabelWidth(130)]
        public Vector3 offset;

        [LabelWidth(130), Min(1)]
        public int count = 1;

        [LabelWidth(130)]
        [InfoBox("Required placements are reported by authoring validation if they cannot resolve content. Runtime still skips invalid rows safely.")]
        public bool required = true;

        [ShowInInspector, ReadOnly, LabelText("Computed Tags")]
        private string ComputedTags => ComputedContentTagRules.Format(Tags);

        public string Id => placementId;
        public string DisplayName => displayName;
        public ContentTagSet Tags => TagsFor(kind);
        public bool IsActive => isActive;
        public string TargetContentId => targetContentId;
        public int Count => Mathf.Max(1, count);
        public bool Required => required;

        public static WorldRewardPlacementDefinition Create(
            string id,
            string name,
            WorldRewardPlacementKind kind,
            string targetContentId,
            Vector3 offset,
            int count = 1,
            bool required = true)
        {
            var placement = new WorldRewardPlacementDefinition
            {
                placementId = id,
                displayName = name,
                kind = kind,
                targetContentId = targetContentId,
                offset = offset,
                count = Mathf.Max(1, count),
                required = required,
                isActive = true
            };
            placement.EnsureWorkflowDefaults();
            return placement;
        }

        public bool EnsureWorkflowDefaults()
        {
            bool changed = false;
            changed |= EnsureString(ref placementId, $"{kind.ToString().ToLowerInvariant()}_placement");
            changed |= EnsureString(ref displayName, "World Reward Placement");
            if (count < 1)
            {
                count = 1;
                changed = true;
            }

            return changed;
        }

        private bool HasPlacementId(string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        private bool HasDisplayName(string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        private static ContentTagSet TagsFor(WorldRewardPlacementKind kind)
        {
            return kind switch
            {
                WorldRewardPlacementKind.Chest => ContentTagSet.With(ContentTag.Chest, ContentTag.Economy),
                WorldRewardPlacementKind.TicketDeposit => ContentTagSet.With(ContentTag.Currency, ContentTag.Economy),
                WorldRewardPlacementKind.HealingProp => ContentTagSet.With(ContentTag.Health, ContentTag.Economy),
                WorldRewardPlacementKind.Shrine => ContentTagSet.With(ContentTag.Shrine, ContentTag.Risk, ContentTag.Economy),
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
}
