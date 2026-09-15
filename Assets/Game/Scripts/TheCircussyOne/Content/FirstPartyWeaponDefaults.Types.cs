using System;
using System.Collections.Generic;
using UnityEngine;
using TheCircussyOne.Stats;

namespace TheCircussyOne.Content
{
    public readonly struct ProjectileVisualDefaults
    {
        public ProjectileVisualDefaults(
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
            Shape = shape;
            Scale = scale;
            PrimaryColor = primaryColor;
            EmissionColor = emissionColor;
            EmissionStrength = emissionStrength;
            TrailEnabled = trailEnabled;
            TrailColor = trailColor;
            TrailWidth = trailWidth;
            TrailLifetime = trailLifetime;
        }

        public ProjectileVisualShape Shape { get; }
        public float Scale { get; }
        public Color PrimaryColor { get; }
        public Color EmissionColor { get; }
        public float EmissionStrength { get; }
        public bool TrailEnabled { get; }
        public Color TrailColor { get; }
        public float TrailWidth { get; }
        public float TrailLifetime { get; }
    }

    public readonly struct WeaponTimingDefaults
    {
        public WeaponTimingDefaults(float baseFireIntervalSeconds, float minimumFireIntervalSeconds)
        {
            BaseFireIntervalSeconds = baseFireIntervalSeconds;
            MinimumFireIntervalSeconds = minimumFireIntervalSeconds;
        }

        public float BaseFireIntervalSeconds { get; }
        public float MinimumFireIntervalSeconds { get; }
    }

    public readonly struct ProjectileGameplayDefaults
    {
        public ProjectileGameplayDefaults(
            int damage,
            float speed,
            float lifetimeSeconds,
            float hitRadius,
            float weaponRange,
            float spawnForwardOffset,
            float spawnHeight,
            float blockRadius)
        {
            Damage = damage;
            Speed = speed;
            LifetimeSeconds = lifetimeSeconds;
            HitRadius = hitRadius;
            WeaponRange = weaponRange;
            SpawnForwardOffset = spawnForwardOffset;
            SpawnHeight = spawnHeight;
            BlockRadius = blockRadius;
        }

        public int Damage { get; }
        public float Speed { get; }
        public float LifetimeSeconds { get; }
        public float HitRadius { get; }
        public float WeaponRange { get; }
        public float SpawnForwardOffset { get; }
        public float SpawnHeight { get; }
        public float BlockRadius { get; }
    }

    public readonly struct ProjectileEmissionDefaults
    {
        public ProjectileEmissionDefaults(int baseCount, int maxCount, float spreadAngleDegrees, bool accuracyAffectsSpread)
        {
            BaseCount = baseCount;
            MaxCount = maxCount;
            SpreadAngleDegrees = spreadAngleDegrees;
            AccuracyAffectsSpread = accuracyAffectsSpread;
        }

        public int BaseCount { get; }
        public int MaxCount { get; }
        public float SpreadAngleDegrees { get; }
        public bool AccuracyAffectsSpread { get; }

        public static ProjectileEmissionDefaults Single => new(1, 1, 0f, true);
    }

    public readonly struct ProjectileTrajectoryDefaults
    {
        public ProjectileTrajectoryDefaults(ProjectileTrajectoryMode mode, float arcHeight)
        {
            Mode = mode;
            ArcHeight = arcHeight;
        }

        public ProjectileTrajectoryMode Mode { get; }
        public float ArcHeight { get; }

        public static ProjectileTrajectoryDefaults Direct => new(ProjectileTrajectoryMode.Direct, 0f);
    }

    public readonly struct ProjectileAimDefaults
    {
        public ProjectileAimDefaults(float aimErrorDegrees, bool accuracyAffectsAimError)
        {
            AimErrorDegrees = aimErrorDegrees;
            AccuracyAffectsAimError = accuracyAffectsAimError;
        }

        public float AimErrorDegrees { get; }
        public bool AccuracyAffectsAimError { get; }

        public static ProjectileAimDefaults None => new(0f, true);
    }

    public readonly struct ProjectileBounceDefaults
    {
        public ProjectileBounceDefaults(bool enabled, int baseCount, int maxCount, float enemySearchRadius, bool offWorldBlockers)
        {
            Enabled = enabled;
            BaseCount = baseCount;
            MaxCount = maxCount;
            EnemySearchRadius = enemySearchRadius;
            OffWorldBlockers = offWorldBlockers;
        }

        public bool Enabled { get; }
        public int BaseCount { get; }
        public int MaxCount { get; }
        public float EnemySearchRadius { get; }
        public bool OffWorldBlockers { get; }

        public static ProjectileBounceDefaults Disabled => new(false, 0, 0, 12f, false);
    }

    public readonly struct ProjectileExplosionDefaults
    {
        public ProjectileExplosionDefaults(
            bool enabled,
            float baseRadius,
            float maxRadius,
            float secondaryDamageMultiplier,
            bool explodeOnWorldImpact,
            bool vfxEnabled)
        {
            Enabled = enabled;
            BaseRadius = baseRadius;
            MaxRadius = maxRadius;
            SecondaryDamageMultiplier = secondaryDamageMultiplier;
            ExplodeOnWorldImpact = explodeOnWorldImpact;
            VfxEnabled = vfxEnabled;
        }

        public bool Enabled { get; }
        public float BaseRadius { get; }
        public float MaxRadius { get; }
        public float SecondaryDamageMultiplier { get; }
        public bool ExplodeOnWorldImpact { get; }
        public bool VfxEnabled { get; }

        public static ProjectileExplosionDefaults Disabled => new(false, 0f, 0f, 0.5f, false, false);
    }

    public readonly struct ProjectileChainDefaults
    {
        public ProjectileChainDefaults(
            bool enabled,
            int baseCount,
            int maxCount,
            float searchRadius,
            float damageMultiplier,
            bool requiresLineOfSight)
        {
            Enabled = enabled;
            BaseCount = baseCount;
            MaxCount = maxCount;
            SearchRadius = searchRadius;
            DamageMultiplier = damageMultiplier;
            RequiresLineOfSight = requiresLineOfSight;
        }

        public bool Enabled { get; }
        public int BaseCount { get; }
        public int MaxCount { get; }
        public float SearchRadius { get; }
        public float DamageMultiplier { get; }
        public bool RequiresLineOfSight { get; }

        public static ProjectileChainDefaults Disabled => new(false, 0, 0, 10f, 0.65f, true);
    }

    public readonly struct OrbitWeaponDefaults
    {
        public OrbitWeaponDefaults(
            bool enabled,
            int baseCount,
            int maxCount,
            float radius,
            float maxRadius,
            float hitRadius,
            float maxHitRadius,
            float degreesPerSecond,
            float heightOffset,
            bool areaDamageEnabled = false,
            float areaDamageMultiplier = 0.55f,
            float areaHitIntervalSeconds = 0.45f)
        {
            Enabled = enabled;
            BaseCount = baseCount;
            MaxCount = maxCount;
            Radius = radius;
            MaxRadius = maxRadius;
            HitRadius = hitRadius;
            MaxHitRadius = maxHitRadius;
            DegreesPerSecond = degreesPerSecond;
            HeightOffset = heightOffset;
            AreaDamageEnabled = areaDamageEnabled;
            AreaDamageMultiplier = areaDamageMultiplier;
            AreaHitIntervalSeconds = areaHitIntervalSeconds;
        }

        public bool Enabled { get; }
        public int BaseCount { get; }
        public int MaxCount { get; }
        public float Radius { get; }
        public float MaxRadius { get; }
        public float HitRadius { get; }
        public float MaxHitRadius { get; }
        public float DegreesPerSecond { get; }
        public float HeightOffset { get; }
        public bool AreaDamageEnabled { get; }
        public float AreaDamageMultiplier { get; }
        public float AreaHitIntervalSeconds { get; }

        public static OrbitWeaponDefaults Disabled => new(false, 1, 1, 2.2f, 3.6f, 0.55f, 1.1f, 150f, 0.9f);
    }

    public readonly struct FirstPartyWeaponSpec
    {
        public FirstPartyWeaponSpec(
            string id,
            string displayName,
            ContentTag[] tags,
            WeaponTimingDefaults timing,
            ProjectileGameplayDefaults projectile,
            ProjectileVisualDefaults visuals,
            StatId[] supportedUpgradeStats,
            bool canAppearAsLevelUpWeapon = true,
            string addWeaponShortDescription = null,
            ProjectileEmissionDefaults emission = default,
            ProjectileAimDefaults aim = default,
            ProjectileTrajectoryDefaults trajectory = default,
            ProjectileBounceDefaults bounce = default,
            ProjectileExplosionDefaults explosion = default,
            ProjectileChainDefaults chain = default,
            OrbitWeaponDefaults orbit = default)
        {
            ProjectileEmissionDefaults normalizedEmission = emission.BaseCount > 0 ? emission : ProjectileEmissionDefaults.Single;
            ProjectileAimDefaults normalizedAim = aim.AimErrorDegrees > 0f || aim.AccuracyAffectsAimError
                ? aim
                : ProjectileAimDefaults.None;
            ProjectileBounceDefaults normalizedBounce = bounce.Enabled || bounce.EnemySearchRadius > 0f ? bounce : ProjectileBounceDefaults.Disabled;
            ProjectileExplosionDefaults normalizedExplosion = explosion.Enabled
                || explosion.BaseRadius > 0f
                || explosion.MaxRadius > 0f
                || explosion.SecondaryDamageMultiplier > 0f
                || explosion.ExplodeOnWorldImpact
                || explosion.VfxEnabled
                    ? explosion
                    : ProjectileExplosionDefaults.Disabled;
            ProjectileChainDefaults normalizedChain = chain.Enabled || chain.SearchRadius > 0f
                ? chain
                : ProjectileChainDefaults.Disabled;
            OrbitWeaponDefaults normalizedOrbit = orbit.Enabled || orbit.Radius > 0f ? orbit : OrbitWeaponDefaults.Disabled;

            Id = id;
            DisplayName = displayName;
            Tags = tags ?? Array.Empty<ContentTag>();
            BaseFireIntervalSeconds = timing.BaseFireIntervalSeconds;
            MinimumFireIntervalSeconds = timing.MinimumFireIntervalSeconds;
            ProjectileDamage = projectile.Damage;
            ProjectileSpeed = projectile.Speed;
            ProjectileLifetimeSeconds = projectile.LifetimeSeconds;
            ProjectileHitRadius = projectile.HitRadius;
            WeaponRange = projectile.WeaponRange;
            ProjectileSpawnForwardOffset = projectile.SpawnForwardOffset;
            ProjectileSpawnHeight = projectile.SpawnHeight;
            ProjectileBlockRadius = projectile.BlockRadius;
            CanAppearAsLevelUpWeapon = canAppearAsLevelUpWeapon;
            AddWeaponShortDescription = string.IsNullOrWhiteSpace(addWeaponShortDescription)
                ? $"Add {displayName} to the run loadout."
                : addWeaponShortDescription;
            BaseProjectileCount = Mathf.Max(1, normalizedEmission.BaseCount);
            MaxProjectileCount = Mathf.Max(BaseProjectileCount, normalizedEmission.MaxCount);
            BaseSpreadAngleDegrees = Mathf.Max(0f, normalizedEmission.SpreadAngleDegrees);
            AccuracyAffectsSpread = normalizedEmission.AccuracyAffectsSpread;
            BaseAimErrorDegrees = Mathf.Max(0f, normalizedAim.AimErrorDegrees);
            AccuracyAffectsAimError = normalizedAim.AccuracyAffectsAimError;
            TrajectoryMode = trajectory.Mode;
            ArcHeight = Mathf.Max(0f, trajectory.ArcHeight);
            BounceEnabled = normalizedBounce.Enabled;
            BaseBounceCount = Mathf.Max(0, normalizedBounce.BaseCount);
            MaxBounceCount = Mathf.Max(BaseBounceCount, normalizedBounce.MaxCount);
            EnemyBounceSearchRadius = Mathf.Max(0.01f, normalizedBounce.EnemySearchRadius);
            BounceOffWorldBlockers = normalizedBounce.OffWorldBlockers;
            ExplosiveEnabled = normalizedExplosion.Enabled;
            BaseSplashRadius = Mathf.Max(0f, normalizedExplosion.BaseRadius);
            MaxSplashRadius = Mathf.Max(BaseSplashRadius, normalizedExplosion.MaxRadius);
            SecondarySplashDamageMultiplier = Mathf.Clamp01(normalizedExplosion.SecondaryDamageMultiplier);
            ExplodeOnWorldImpact = normalizedExplosion.ExplodeOnWorldImpact;
            ExplosionVfxEnabled = normalizedExplosion.VfxEnabled;
            ChainEnabled = normalizedChain.Enabled;
            BaseChainCount = Mathf.Max(0, normalizedChain.BaseCount);
            MaxChainCount = Mathf.Max(BaseChainCount, normalizedChain.MaxCount);
            ChainSearchRadius = Mathf.Max(0.01f, normalizedChain.SearchRadius);
            ChainDamageMultiplier = Mathf.Clamp01(normalizedChain.DamageMultiplier);
            ChainRequiresLineOfSight = normalizedChain.RequiresLineOfSight;
            Visuals = visuals;
            SupportedUpgradeStats = supportedUpgradeStats ?? Array.Empty<StatId>();
            OrbitEnabled = normalizedOrbit.Enabled;
            BaseOrbitCount = Mathf.Max(1, normalizedOrbit.BaseCount);
            MaxOrbitCount = Mathf.Max(BaseOrbitCount, normalizedOrbit.MaxCount);
            OrbitRadius = Mathf.Max(0.01f, normalizedOrbit.Radius);
            MaxOrbitRadius = Mathf.Max(OrbitRadius, normalizedOrbit.MaxRadius);
            OrbitHitRadius = Mathf.Max(0.01f, normalizedOrbit.HitRadius);
            MaxOrbitHitRadius = Mathf.Max(OrbitHitRadius, normalizedOrbit.MaxHitRadius);
            OrbitDegreesPerSecond = Mathf.Max(0f, normalizedOrbit.DegreesPerSecond);
            OrbitHeightOffset = Mathf.Max(0f, normalizedOrbit.HeightOffset);
            OrbitAreaDamageEnabled = normalizedOrbit.AreaDamageEnabled;
            OrbitAreaDamageMultiplier = Mathf.Clamp01(normalizedOrbit.AreaDamageMultiplier);
            OrbitAreaHitIntervalSeconds = Mathf.Max(0.01f, normalizedOrbit.AreaHitIntervalSeconds);
        }

        public string Id { get; }
        public string DisplayName { get; }
        public IReadOnlyList<ContentTag> Tags { get; }
        public float BaseFireIntervalSeconds { get; }
        public float MinimumFireIntervalSeconds { get; }
        public int ProjectileDamage { get; }
        public float ProjectileSpeed { get; }
        public float ProjectileLifetimeSeconds { get; }
        public float ProjectileHitRadius { get; }
        public float WeaponRange { get; }
        public float ProjectileSpawnForwardOffset { get; }
        public float ProjectileSpawnHeight { get; }
        public float ProjectileBlockRadius { get; }
        public bool CanAppearAsLevelUpWeapon { get; }
        public string AddWeaponShortDescription { get; }
        public int BaseProjectileCount { get; }
        public int MaxProjectileCount { get; }
        public float BaseSpreadAngleDegrees { get; }
        public bool AccuracyAffectsSpread { get; }
        public float BaseAimErrorDegrees { get; }
        public bool AccuracyAffectsAimError { get; }
        public ProjectileTrajectoryMode TrajectoryMode { get; }
        public float ArcHeight { get; }
        public bool BounceEnabled { get; }
        public int BaseBounceCount { get; }
        public int MaxBounceCount { get; }
        public float EnemyBounceSearchRadius { get; }
        public bool BounceOffWorldBlockers { get; }
        public bool ExplosiveEnabled { get; }
        public float BaseSplashRadius { get; }
        public float MaxSplashRadius { get; }
        public float SecondarySplashDamageMultiplier { get; }
        public bool ExplodeOnWorldImpact { get; }
        public bool ExplosionVfxEnabled { get; }
        public bool ChainEnabled { get; }
        public int BaseChainCount { get; }
        public int MaxChainCount { get; }
        public float ChainSearchRadius { get; }
        public float ChainDamageMultiplier { get; }
        public bool ChainRequiresLineOfSight { get; }
        public ProjectileVisualDefaults Visuals { get; }
        public IReadOnlyList<StatId> SupportedUpgradeStats { get; }
        public bool OrbitEnabled { get; }
        public int BaseOrbitCount { get; }
        public int MaxOrbitCount { get; }
        public float OrbitRadius { get; }
        public float MaxOrbitRadius { get; }
        public float OrbitHitRadius { get; }
        public float MaxOrbitHitRadius { get; }
        public float OrbitDegreesPerSecond { get; }
        public float OrbitHeightOffset { get; }
        public bool OrbitAreaDamageEnabled { get; }
        public float OrbitAreaDamageMultiplier { get; }
        public float OrbitAreaHitIntervalSeconds { get; }

        public ContentTagSet CreateTagSet()
        {
            var tags = new ContentTag[Tags.Count];
            for (int i = 0; i < Tags.Count; i++)
            {
                tags[i] = Tags[i];
            }

            return ContentTagSet.With(tags);
        }
    }
}
