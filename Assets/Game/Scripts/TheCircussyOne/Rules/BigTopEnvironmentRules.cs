using TheCircussyOne.Config;
using UnityEngine;

namespace TheCircussyOne.Rules
{
    public readonly struct BigTopEnvironmentSpec
    {
        public BigTopEnvironmentSpec(
            float radius,
            float groundSkirtRadius,
            float curtainHeight,
            float domeHeight,
            int segmentCount,
            int canopyRingCount,
            int textureResolution,
            int stripeCount,
            int seed,
            int visualHash = 0)
        {
            Radius = Mathf.Max(0.1f, radius);
            GroundSkirtRadius = Mathf.Clamp(groundSkirtRadius, 0.1f, Radius);
            CurtainHeight = Mathf.Max(1f, curtainHeight);
            DomeHeight = Mathf.Max(1f, domeHeight);
            SegmentCount = Mathf.Max(16, segmentCount);
            CanopyRingCount = Mathf.Max(2, canopyRingCount);
            TextureResolution = Mathf.Clamp(textureResolution, 128, 2048);
            StripeCount = Mathf.Max(2, stripeCount);
            Seed = seed;
            VisualHash = visualHash;
        }

        public float Radius { get; }
        public float GroundSkirtRadius { get; }
        public float CurtainHeight { get; }
        public float DomeHeight { get; }
        public int SegmentCount { get; }
        public int CanopyRingCount { get; }
        public int TextureResolution { get; }
        public int StripeCount { get; }
        public int Seed { get; }
        public int VisualHash { get; }
        public float TotalHeight => CurtainHeight + DomeHeight;
        public bool IsValid => Radius > 0f && TotalHeight > 0f && SegmentCount >= 16 && CanopyRingCount >= 2;
    }

    public static class BigTopEnvironmentRules
    {
        public static BigTopEnvironmentSpec BuildSpec(
            float baseRadius,
            float generatedMaxHeight,
            int seed,
            RunWorldGenerationConfig config)
        {
            float radius = Mathf.Max(0.1f, baseRadius) + (config != null ? config.BigTopRadiusPadding : 24f);
            float terrainClearanceHeight = Mathf.Max(0f, generatedMaxHeight) + 16f;
            float curtainHeight = Mathf.Max(
                config != null ? config.BigTopCurtainHeight : 72f,
                terrainClearanceHeight);
            float domeHeight = Mathf.Max(
                config != null ? config.BigTopMinimumDomeHeight : 120f,
                radius * (config != null ? config.BigTopDomeHeightPercent : 0.34f));
            return new BigTopEnvironmentSpec(
                radius,
                Mathf.Max(0.1f, radius - (config != null ? config.BigTopGroundSkirtInset : 28f)),
                curtainHeight,
                domeHeight,
                config != null ? config.BigTopSegmentCount : 128,
                config != null ? config.BigTopCanopyRingCount : 8,
                config != null ? config.BigTopTextureResolution : 1024,
                config != null ? config.BigTopStripeCount : 24,
                seed,
                BuildVisualHash(config));
        }

        public static bool SpecMatches(BigTopEnvironmentSpec a, BigTopEnvironmentSpec b)
        {
            return Mathf.Approximately(a.Radius, b.Radius)
                && Mathf.Approximately(a.GroundSkirtRadius, b.GroundSkirtRadius)
                && Mathf.Approximately(a.CurtainHeight, b.CurtainHeight)
                && Mathf.Approximately(a.DomeHeight, b.DomeHeight)
                && a.SegmentCount == b.SegmentCount
                && a.CanopyRingCount == b.CanopyRingCount
                && a.TextureResolution == b.TextureResolution
                && a.StripeCount == b.StripeCount
                && a.Seed == b.Seed
                && a.VisualHash == b.VisualHash;
        }

        private static int BuildVisualHash(RunWorldGenerationConfig config)
        {
            if (config == null)
            {
                return 0;
            }

            unchecked
            {
                int hash = 17;
                Add(ref hash, config.bigTopEnvironmentEnabled);
                Add(ref hash, config.bigTopRedStripeColor);
                Add(ref hash, config.bigTopCanvasStripeColor);
                Add(ref hash, config.BigTopBrightness);
                Add(ref hash, config.BigTopSeamStrength);
                Add(ref hash, config.BigTopWeaveStrength);
                Add(ref hash, config.BigTopGritStrength);
                Add(ref hash, config.BigTopLowerDirtStrength);
                Add(ref hash, config.bigTopPerimeterFogEnabled);
                Add(ref hash, config.BigTopPerimeterFogInnerRadiusPercent);
                Add(ref hash, config.BigTopPerimeterFogOuterInset);
                Add(ref hash, config.BigTopPerimeterFogHeight);
                Add(ref hash, config.BigTopPerimeterFogSegmentCount);
                Add(ref hash, config.bigTopPerimeterFogColor);
                Add(ref hash, config.BigTopPerimeterFogBottomOpacity);
                Add(ref hash, config.BigTopPerimeterFogTopOpacity);
                Add(ref hash, config.BigTopPerimeterFogNoiseStrength);
                Add(ref hash, config.bigTopGodRaysEnabled);
                Add(ref hash, config.BigTopGodRayCount);
                Add(ref hash, config.bigTopGodRayColor);
                Add(ref hash, config.BigTopGodRayOpacity);
                Add(ref hash, config.BigTopGodRayStartDomePercent);
                Add(ref hash, config.BigTopGodRayStartRadiusMinPercent);
                Add(ref hash, config.BigTopGodRayStartRadiusMaxPercent);
                Add(ref hash, config.BigTopGodRayEndRadiusPercent);
                Add(ref hash, config.BigTopGodRayEndHeight);
                Add(ref hash, config.BigTopGodRayTopWidthPercent);
                Add(ref hash, config.BigTopGodRayBottomWidthPercent);
                Add(ref hash, config.BigTopGodRayAngleJitter);
                Add(ref hash, config.BigTopGodRayFlickerAmount);
                Add(ref hash, config.BigTopGodRayFlickerSpeed);
                Add(ref hash, config.BigTopGodRayBottomFadePower);
                Add(ref hash, config.BigTopGodRayEdgeFadePower);
                Add(ref hash, config.BigTopGodRayTextureNoiseStrength);
                Add(ref hash, config.bigTopGodRayFixturesEnabled);
                Add(ref hash, config.BigTopGodRayFixtureLength);
                Add(ref hash, config.BigTopGodRayFixtureRadius);
                Add(ref hash, config.BigTopGodRayFixtureLensRadius);
                Add(ref hash, config.BigTopGodRayFixtureBeamCoverage);
                Add(ref hash, config.bigTopGodRayFixtureColor);
                Add(ref hash, config.bigTopGodRayFixtureLensColor);
                Add(ref hash, config.BigTopGodRayFixtureLensEmissionStrength);
                Add(ref hash, config.bigTopShowLightsEnabled);
                Add(ref hash, config.BigTopShowLightCount);
                Add(ref hash, config.BigTopShowLightSweepDegrees);
                Add(ref hash, config.BigTopShowLightRadiusDriftPercent);
                Add(ref hash, config.BigTopShowLightSpeedMinHz);
                Add(ref hash, config.BigTopShowLightSpeedMaxHz);
                Add(ref hash, config.BigTopShowLightMeshRefreshInterval);
                Add(ref hash, config.bigTopShowLightColorVariationEnabled);
                Add(ref hash, config.bigTopShowLightAccentPinkColor);
                Add(ref hash, config.bigTopShowLightAccentTealColor);
                Add(ref hash, config.BigTopShowLightAccentStrength);
                Add(ref hash, config.BigTopShowLightColorDriftStrength);
                Add(ref hash, config.BigTopShowLightColorDriftSpeedMinHz);
                Add(ref hash, config.BigTopShowLightColorDriftSpeedMaxHz);
                Add(ref hash, config.bigTopBulbStringsEnabled);
                Add(ref hash, config.BigTopBulbRingCount);
                Add(ref hash, config.BigTopBulbsPerRing);
                Add(ref hash, config.BigTopBulbRadialStringCount);
                Add(ref hash, config.BigTopBulbsPerRadialString);
                Add(ref hash, config.BigTopBulbRadius);
                Add(ref hash, config.bigTopBulbColor);
                Add(ref hash, config.BigTopBulbEmissionStrength);
                Add(ref hash, config.BigTopBulbFlickerAmount);
                Add(ref hash, config.BigTopBulbFlickerSpeed);
                Add(ref hash, config.bigTopDressingEnabled);
                Add(ref hash, config.bigTopCenterHubEnabled);
                Add(ref hash, config.BigTopCenterHubRadiusPercent);
                Add(ref hash, config.BigTopCenterHubTubeRadius);
                Add(ref hash, config.BigTopCenterHubCordCount);
                Add(ref hash, config.BigTopCenterHubCordDrop);
                Add(ref hash, config.bigTopCanopyRopesEnabled);
                Add(ref hash, config.BigTopCanopyRopeThickness);
                Add(ref hash, config.BigTopCanopyRadialRopeCount);
                Add(ref hash, config.BigTopCanopyRingRopeCount);
                Add(ref hash, config.bigTopPerimeterPolesEnabled);
                Add(ref hash, config.BigTopPerimeterPoleCount);
                Add(ref hash, config.BigTopPerimeterPoleRadiusPercent);
                Add(ref hash, config.BigTopPerimeterPoleRadius);
                Add(ref hash, config.BigTopPerimeterPoleTopPadding);
                Add(ref hash, config.bigTopCurtainValanceEnabled);
                Add(ref hash, config.BigTopCurtainValanceHeight);
                Add(ref hash, config.BigTopCurtainValanceScallopDrop);
                Add(ref hash, config.BigTopCurtainValanceScallopCount);
                Add(ref hash, config.bigTopBuntingEnabled);
                Add(ref hash, config.BigTopBuntingRingCount);
                Add(ref hash, config.BigTopBuntingFlagsPerRing);
                Add(ref hash, config.BigTopBuntingLowerDomePercent);
                Add(ref hash, config.BigTopBuntingUpperDomePercent);
                Add(ref hash, config.BigTopBuntingFlagWidth);
                Add(ref hash, config.BigTopBuntingFlagDrop);
                Add(ref hash, config.BigTopBuntingStringThickness);
                Add(ref hash, config.bigTopCurtainBannersEnabled);
                Add(ref hash, config.BigTopCurtainBannerCount);
                Add(ref hash, config.BigTopCurtainBannerWidth);
                Add(ref hash, config.BigTopCurtainBannerHeight);
                Add(ref hash, config.BigTopCurtainBannerTopOffset);
                Add(ref hash, config.BigTopCurtainBannerInwardInset);
                Add(ref hash, config.bigTopDressingRopeColor);
                Add(ref hash, config.bigTopDressingPoleColor);
                Add(ref hash, config.bigTopCurtainValanceColor);
                Add(ref hash, config.bigTopBuntingStringColor);
                Add(ref hash, config.bigTopBuntingPrimaryColor);
                Add(ref hash, config.bigTopBuntingSecondaryColor);
                Add(ref hash, config.bigTopCurtainBannerColor);
                Add(ref hash, config.bigTopCurtainBannerTrimColor);
                return hash;
            }
        }

        private static void Add(ref int hash, bool value)
        {
            hash = hash * 31 + (value ? 1 : 0);
        }

        private static void Add(ref int hash, int value)
        {
            hash = hash * 31 + value;
        }

        private static void Add(ref int hash, float value)
        {
            hash = hash * 31 + Mathf.RoundToInt(value * 10000f);
        }

        private static void Add(ref int hash, Color value)
        {
            Add(ref hash, value.r);
            Add(ref hash, value.g);
            Add(ref hash, value.b);
            Add(ref hash, value.a);
        }
    }
}
