using UnityEngine;

namespace TheCircussyOne.Rules
{
    public readonly struct PickupDropFrame
    {
        public PickupDropFrame(
            Vector3 startPosition,
            Vector3 landingPosition,
            float tossSeconds,
            float arcHeight,
            float bounceHeight,
            int bounceCount,
            float settleDelaySeconds,
            EaseSettings horizontalEase)
        {
            StartPosition = startPosition;
            LandingPosition = landingPosition;
            TossSeconds = Mathf.Max(0f, tossSeconds);
            ArcHeight = Mathf.Max(0f, arcHeight);
            BounceHeight = Mathf.Max(0f, bounceHeight);
            BounceCount = Mathf.Max(0, bounceCount);
            SettleDelaySeconds = Mathf.Max(0f, settleDelaySeconds);
            HorizontalEase = horizontalEase;
        }

        public Vector3 StartPosition { get; }
        public Vector3 LandingPosition { get; }
        public float TossSeconds { get; }
        public float ArcHeight { get; }
        public float BounceHeight { get; }
        public int BounceCount { get; }
        public float SettleDelaySeconds { get; }
        public EaseSettings HorizontalEase { get; }
        public float CollectDelaySeconds => TossSeconds + SettleDelaySeconds;
        public bool HasToss => TossSeconds > 0.0001f || SettleDelaySeconds > 0.0001f;

        public static PickupDropFrame Immediate(Vector3 position)
        {
            return new PickupDropFrame(position, position, 0f, 0f, 0f, 0, 0f, EaseSettings.Linear);
        }
    }

    public static class PickupDropPlacementRules
    {
        private const float FullTurnRadians = 6.28318531f;
        private const float GoldenAngle = 2.39996323f;
        private const float TwoDropHalfAngleRadians = 0.95993109f;
        private const float MinimumMultiDropRadiusScale = 0.85f;
        private const float MaximumMultiDropRadiusScale = 1.15f;

        public static Vector3 ScatterOffset(int index, int count, int seed, float radius)
        {
            if (radius <= 0f || count <= 1)
            {
                return Vector3.zero;
            }

            int safeIndex = Mathf.Max(0, index);
            int safeCount = Mathf.Max(1, count);
            int laneIndex = safeIndex % safeCount;
            float angle = AngleFor(safeIndex, safeCount, seed);
            float radiusScale = RadiusScale(laneIndex, safeCount);
            return new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius * radiusScale;
        }

        public static Vector3 LaunchOffset(int index, int count, int seed, float collisionRadius)
        {
            if (collisionRadius <= 0f || count <= 1)
            {
                return Vector3.zero;
            }

            return ScatterOffset(index, count, seed, Mathf.Clamp(collisionRadius * 1.35f, 0.08f, 0.26f));
        }

        public static int PresentationScatterCount(int dropCount, bool isExperienceDrop)
        {
            int count = Mathf.Max(1, dropCount);
            return isExperienceDrop ? Mathf.Max(2, count) : count;
        }

        public static float ArcHeight(float baseArcHeight, float experienceMultiplier, bool isExperienceDrop)
        {
            float arcHeight = Mathf.Max(0f, baseArcHeight);
            if (!isExperienceDrop)
            {
                return arcHeight;
            }

            return arcHeight * Mathf.Max(0f, experienceMultiplier);
        }

        private static float AngleFor(int index, int count, int seed)
        {
            float baseAngle = DeterministicSeed.ToAngleRadians(seed);
            if (count == 2)
            {
                return baseAngle + (index % 2 == 0 ? -TwoDropHalfAngleRadians : TwoDropHalfAngleRadians);
            }

            if (count == 3)
            {
                return baseAngle + (index % 3) * (FullTurnRadians / 3f);
            }

            return baseAngle + index * GoldenAngle;
        }
        private static float RadiusScale(int laneIndex, int count)
        {
            if (count <= 1)
            {
                return 1f;
            }

            if (count == 2)
            {
                return laneIndex == 0 ? MinimumMultiDropRadiusScale : MaximumMultiDropRadiusScale;
            }

            float t = Mathf.Clamp01(laneIndex / (count - 1f));
            return Mathf.Lerp(MinimumMultiDropRadiusScale, MaximumMultiDropRadiusScale, t);
        }

        public static Vector3 AboveSurface(Vector3 position, float surfaceY, float clearance)
        {
            return new Vector3(position.x, surfaceY + Mathf.Max(0f, clearance), position.z);
        }

        public static Vector3 SafeFallback(Vector3 origin, float clearance)
        {
            return new Vector3(origin.x, Mathf.Max(origin.y, 0f) + Mathf.Max(0f, clearance), origin.z);
        }
    }
}
