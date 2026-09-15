using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TheCircussyOne.Content
{
    [Serializable]
    public struct WorldPropPlacementProfile
    {
        [Min(0.05f), SuffixLabel("u", true)]
        public float footprintRadius;

        [Min(0f), SuffixLabel("u", true)]
        public float groundClearance;

        [Range(0f, 89f), SuffixLabel("deg", true)]
        public float maxPlacementSlope;

        public bool blocksPlayer;

        [ShowIf(nameof(blocksPlayer))]
        public Vector3 bodyColliderCenter;

        [ShowIf(nameof(blocksPlayer))]
        public Vector3 bodyColliderSize;

        public bool HasBlockingBody =>
            blocksPlayer
            && bodyColliderSize.x > 0f
            && bodyColliderSize.y > 0f
            && bodyColliderSize.z > 0f;

        public float RootHeightOffset => Mathf.Max(0f, groundClearance) + Mathf.Max(0f, bodyColliderSize.y) * 0.5f;

        public static WorldPropPlacementProfile Create(
            float footprintRadius,
            float groundClearance,
            float maxPlacementSlope,
            bool blocksPlayer,
            Vector3 bodyColliderSize)
        {
            return new WorldPropPlacementProfile
            {
                footprintRadius = Mathf.Max(0.05f, footprintRadius),
                groundClearance = Mathf.Max(0f, groundClearance),
                maxPlacementSlope = Mathf.Clamp(maxPlacementSlope, 0f, 89f),
                blocksPlayer = blocksPlayer,
                bodyColliderCenter = Vector3.zero,
                bodyColliderSize = new Vector3(
                    Mathf.Max(0.05f, bodyColliderSize.x),
                    Mathf.Max(0.05f, bodyColliderSize.y),
                    Mathf.Max(0.05f, bodyColliderSize.z))
            };
        }

        public static WorldPropPlacementProfile ChestDefault()
        {
            return Create(0.85f, 0.005f, 65f, blocksPlayer: true, new Vector3(1.2f, 0.75f, 0.9f));
        }

        public static WorldPropPlacementProfile TicketDepositDefault(TicketDepositTier tier)
        {
            return tier switch
            {
                TicketDepositTier.Large => Create(0.82f, 0.005f, 65f, blocksPlayer: true, new Vector3(1.25f, 1f, 1.25f)),
                TicketDepositTier.Medium => Create(0.58f, 0.005f, 65f, blocksPlayer: true, new Vector3(0.75f, 0.6f, 0.75f)),
                _ => Create(0.42f, 0.005f, 65f, blocksPlayer: true, new Vector3(0.45f, 0.36f, 0.45f))
            };
        }

        public static WorldPropPlacementProfile HealingPropDefault()
        {
            return Create(0.5f, 0.005f, 65f, blocksPlayer: false, new Vector3(0.85f, 0.72f, 0.85f));
        }

        public bool EnsureDefaults(WorldPropPlacementProfile defaults)
        {
            bool changed = false;

            if (footprintRadius <= 0f)
            {
                footprintRadius = defaults.footprintRadius;
                changed = true;
            }

            if (groundClearance < 0f)
            {
                groundClearance = defaults.groundClearance;
                changed = true;
            }

            if (maxPlacementSlope <= 0f || maxPlacementSlope >= 90f)
            {
                maxPlacementSlope = defaults.maxPlacementSlope;
                changed = true;
            }

            bool hasInvalidBodySize = bodyColliderSize.x <= 0f || bodyColliderSize.y <= 0f || bodyColliderSize.z <= 0f;
            if (hasInvalidBodySize)
            {
                bodyColliderSize = defaults.bodyColliderSize;
                blocksPlayer = defaults.blocksPlayer;
                changed = true;
            }

            if (bodyColliderCenter == default && defaults.bodyColliderCenter != default)
            {
                bodyColliderCenter = defaults.bodyColliderCenter;
                changed = true;
            }

            return changed;
        }
    }
}
