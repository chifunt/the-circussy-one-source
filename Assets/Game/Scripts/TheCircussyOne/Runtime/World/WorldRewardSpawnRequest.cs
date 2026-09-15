using TheCircussyOne.Content;
using UnityEngine;

namespace TheCircussyOne.Runtime
{
    public readonly struct WorldRewardSpawnRequest
    {
        public WorldRewardSpawnRequest(
            string placementId,
            WorldRewardPlacementKind kind,
            string targetContentId,
            Vector3 position,
            bool required,
            bool repeatSequentialContent = false,
            bool generated = false)
        {
            PlacementId = placementId;
            Kind = kind;
            TargetContentId = targetContentId;
            Position = position;
            Required = required;
            RepeatSequentialContent = repeatSequentialContent;
            Generated = generated;
        }

        public string PlacementId { get; }
        public WorldRewardPlacementKind Kind { get; }
        public string TargetContentId { get; }
        public Vector3 Position { get; }
        public bool Required { get; }
        public bool RepeatSequentialContent { get; }
        public bool Generated { get; }
        public bool HasTargetContent => !string.IsNullOrWhiteSpace(TargetContentId);
    }
}
