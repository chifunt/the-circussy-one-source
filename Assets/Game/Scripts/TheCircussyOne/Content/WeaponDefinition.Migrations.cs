using UnityEngine;
using TheCircussyOne.Runtime;

namespace TheCircussyOne.Content
{
    public sealed partial class WeaponDefinition
    {
        private bool EnsureProjectileCollisionDefaults()
        {
            bool changed = false;
            if (projectileCollisionDefaultsVersion < 1)
            {
                requireLineOfSight = true;
                int environmentMask = GameLayers.EnvironmentMaskExcludingGameplay;
                lineOfSightMask = environmentMask;
                projectileBlockMask = environmentMask;
                changed = true;
            }

            if (projectileCollisionDefaultsVersion < 2 && projectileBlockRadius < 0.001f)
            {
                projectileBlockRadius = DefaultProjectileBlockRadiusForId();
                changed = true;
            }

            if (projectileCollisionDefaultsVersion < ProjectileCollisionDefaultsVersion)
            {
                projectileCollisionDefaultsVersion = ProjectileCollisionDefaultsVersion;
                changed = true;
            }

            return changed;
        }

        private float DefaultProjectileBlockRadiusForId()
        {
            return FirstPartyWeaponDefaults.GetOrDefault(weaponId).ProjectileBlockRadius;
        }

        private bool EnsureProjectileVisualDefaults()
        {
            if (projectileVisualDefaultsVersion >= ProjectileVisualDefaultsVersion)
            {
                return false;
            }

            ApplyProjectileVisualDefaultsForId();
            projectileVisualDefaultsVersion = ProjectileVisualDefaultsVersion;
            return true;
        }

        private bool EnsureProjectileEmissionDefaults()
        {
            if (projectileEmissionDefaultsVersion >= ProjectileEmissionDefaultsVersion)
            {
                return false;
            }

            FirstPartyWeaponSpec spec = FirstPartyWeaponDefaults.GetOrDefault(weaponId);
            SetProjectileEmissionDefaults(
                spec.BaseProjectileCount,
                spec.MaxProjectileCount,
                spec.BaseSpreadAngleDegrees,
                spec.AccuracyAffectsSpread);
            projectileEmissionDefaultsVersion = ProjectileEmissionDefaultsVersion;
            return true;
        }

        private bool EnsureProjectileAimDefaults()
        {
            if (projectileAimDefaultsVersion >= ProjectileAimDefaultsVersion)
            {
                return false;
            }

            FirstPartyWeaponSpec spec = FirstPartyWeaponDefaults.GetOrDefault(weaponId);
            SetProjectileAimDefaults(
                spec.BaseAimErrorDegrees,
                spec.AccuracyAffectsAimError);
            projectileAimDefaultsVersion = ProjectileAimDefaultsVersion;
            return true;
        }

        private bool EnsureProjectileTrajectoryDefaults()
        {
            if (projectileTrajectoryDefaultsVersion >= ProjectileTrajectoryDefaultsVersion)
            {
                return false;
            }

            FirstPartyWeaponSpec spec = FirstPartyWeaponDefaults.GetOrDefault(weaponId);
            SetProjectileTrajectoryDefaults(spec.TrajectoryMode, spec.ArcHeight);
            projectileTrajectoryDefaultsVersion = ProjectileTrajectoryDefaultsVersion;
            return true;
        }

        private bool EnsureAcquisitionDefaults()
        {
            if (acquisitionDefaultsVersion >= AcquisitionDefaultsVersion)
            {
                return false;
            }

            canAppearAsLevelUpWeapon = !FirstPartyWeaponDefaults.TryGet(weaponId, out FirstPartyWeaponSpec spec)
                || spec.CanAppearAsLevelUpWeapon;
            acquisitionDefaultsVersion = AcquisitionDefaultsVersion;
            return true;
        }

        private bool EnsureBounceDefaults()
        {
            if (bounceDefaultsVersion >= BounceDefaultsVersion)
            {
                return false;
            }

            FirstPartyWeaponSpec spec = FirstPartyWeaponDefaults.GetOrDefault(weaponId);
            SetBounceDefaults(
                spec.BounceEnabled,
                spec.BaseBounceCount,
                spec.MaxBounceCount,
                spec.EnemyBounceSearchRadius,
                spec.BounceOffWorldBlockers);
            bounceDefaultsVersion = BounceDefaultsVersion;
            return true;
        }

        private bool EnsureExplosionDefaults()
        {
            if (explosionDefaultsVersion >= ExplosionDefaultsVersion)
            {
                return false;
            }

            FirstPartyWeaponSpec spec = FirstPartyWeaponDefaults.GetOrDefault(weaponId);
            SetExplosionDefaults(
                spec.ExplosiveEnabled,
                spec.BaseSplashRadius,
                spec.MaxSplashRadius,
                spec.SecondarySplashDamageMultiplier,
                spec.ExplodeOnWorldImpact,
                spec.ExplosionVfxEnabled);
            explosionDefaultsVersion = ExplosionDefaultsVersion;
            return true;
        }

        private bool EnsureOrbitDefaults()
        {
            if (orbitDefaultsVersion >= OrbitDefaultsVersion)
            {
                return false;
            }

            FirstPartyWeaponSpec spec = FirstPartyWeaponDefaults.GetOrDefault(weaponId);
            SetOrbitDefaults(
                spec.OrbitEnabled,
                spec.BaseOrbitCount,
                spec.MaxOrbitCount,
                spec.OrbitRadius,
                spec.MaxOrbitRadius,
                spec.OrbitHitRadius,
                spec.MaxOrbitHitRadius,
                spec.OrbitDegreesPerSecond,
                spec.OrbitHeightOffset,
                spec.OrbitAreaDamageEnabled,
                spec.OrbitAreaDamageMultiplier,
                spec.OrbitAreaHitIntervalSeconds);
            orbitDefaultsVersion = OrbitDefaultsVersion;
            return true;
        }

        private bool EnsureChainDefaults()
        {
            if (chainDefaultsVersion >= ChainDefaultsVersion)
            {
                return false;
            }

            FirstPartyWeaponSpec spec = FirstPartyWeaponDefaults.GetOrDefault(weaponId);
            SetChainDefaults(
                spec.ChainEnabled,
                spec.BaseChainCount,
                spec.MaxChainCount,
                spec.ChainSearchRadius,
                spec.ChainDamageMultiplier,
                spec.ChainRequiresLineOfSight);
            chainDefaultsVersion = ChainDefaultsVersion;
            return true;
        }

        private void ApplyProjectileVisualDefaultsForId()
        {
            ProjectileVisualDefaults visuals = FirstPartyWeaponDefaults.GetOrDefault(weaponId).Visuals;
            SetProjectileVisualDefaults(
                visuals.Shape,
                visuals.Scale,
                visuals.PrimaryColor,
                visuals.EmissionColor,
                visuals.EmissionStrength,
                visuals.TrailEnabled,
                visuals.TrailColor,
                visuals.TrailWidth,
                visuals.TrailLifetime);
        }

        private void SetProjectileVisualDefaults(
            ProjectileVisualShape shape,
            float scale,
            Color primaryColor,
            Color emissionColor,
            float emissionStrength,
            bool trailEnabled,
            Color trailColor,
            float trailWidth,
            float trailLifetime)
        {
            projectileVisualShape = shape;
            projectileVisualScale = scale;
            projectilePrimaryColor = primaryColor;
            projectileEmissionColor = emissionColor;
            projectileEmissionStrength = emissionStrength;
            projectileTrailEnabled = trailEnabled;
            projectileTrailColor = trailColor;
            projectileTrailWidth = trailWidth;
            projectileTrailLifetime = trailLifetime;
        }
    }
}
