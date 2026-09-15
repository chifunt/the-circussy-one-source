using System.Collections.Generic;
using UnityEngine;
using TheCircussyOne.Rules;
using TheCircussyOne.Runtime;
using TheCircussyOne.Stats;

namespace TheCircussyOne.Content
{
    public sealed partial class WeaponDefinition
    {
        public void ApplyDefaults(
            string id,
            string name,
            ContentTagSet tags,
            float baseFireIntervalSeconds,
            float minimumFireIntervalSeconds,
            int projectileDamage,
            float projectileSpeed,
            float projectileLifetimeSeconds,
            float projectileHitRadius,
            LayerMask projectileHitMask,
            float weaponRange,
            float projectileSpawnForwardOffset,
            float projectileSpawnHeight)
        {
            weaponId = id;
            displayName = name;
            this.tags = tags ?? ContentTagSet.With(ContentTag.Projectile, ContentTag.Physical);
            isActive = true;
            availabilityDefaultsVersion = AvailabilityDefaultsVersion;
            this.baseFireIntervalSeconds = baseFireIntervalSeconds;
            this.minimumFireIntervalSeconds = minimumFireIntervalSeconds;
            this.projectileDamage = projectileDamage;
            this.projectileSpeed = projectileSpeed;
            this.projectileLifetimeSeconds = projectileLifetimeSeconds;
            this.projectileHitRadius = projectileHitRadius;
            this.projectileHitMask = projectileHitMask;
            this.weaponRange = weaponRange;
            this.projectileSpawnForwardOffset = projectileSpawnForwardOffset;
            this.projectileSpawnHeight = projectileSpawnHeight;
            EnsureWorkflowDefaults();
        }

        public void ApplyDefaults(FirstPartyWeaponSpec spec)
        {
            ApplyDefaults(
                spec.Id,
                spec.DisplayName,
                spec.CreateTagSet(),
                spec.BaseFireIntervalSeconds,
                spec.MinimumFireIntervalSeconds,
                spec.ProjectileDamage,
                spec.ProjectileSpeed,
                spec.ProjectileLifetimeSeconds,
                spec.ProjectileHitRadius,
                LayerMask.GetMask(GameLayers.Enemy),
                spec.WeaponRange,
                spec.ProjectileSpawnForwardOffset,
                spec.ProjectileSpawnHeight);

            int environmentMask = GameLayers.EnvironmentMaskExcludingGameplay;
            requireLineOfSight = true;
            lineOfSightMask = environmentMask;
            projectileBlockMask = environmentMask;
            projectileBlockRadius = spec.ProjectileBlockRadius;
            projectileCollisionDefaultsVersion = ProjectileCollisionDefaultsVersion;
            canAppearAsLevelUpWeapon = spec.CanAppearAsLevelUpWeapon;
            addWeaponShortDescription = spec.AddWeaponShortDescription;
            acquisitionDefaultsVersion = AcquisitionDefaultsVersion;
            SetProjectileVisualDefaults(
                spec.Visuals.Shape,
                spec.Visuals.Scale,
                spec.Visuals.PrimaryColor,
                spec.Visuals.EmissionColor,
                spec.Visuals.EmissionStrength,
                spec.Visuals.TrailEnabled,
                spec.Visuals.TrailColor,
                spec.Visuals.TrailWidth,
                spec.Visuals.TrailLifetime);
            projectileVisualDefaultsVersion = ProjectileVisualDefaultsVersion;
            SetProjectileEmissionDefaults(
                spec.BaseProjectileCount,
                spec.MaxProjectileCount,
                spec.BaseSpreadAngleDegrees,
                spec.AccuracyAffectsSpread);
            projectileEmissionDefaultsVersion = ProjectileEmissionDefaultsVersion;
            SetProjectileAimDefaults(
                spec.BaseAimErrorDegrees,
                spec.AccuracyAffectsAimError);
            projectileAimDefaultsVersion = ProjectileAimDefaultsVersion;
            SetProjectileTrajectoryDefaults(spec.TrajectoryMode, spec.ArcHeight);
            projectileTrajectoryDefaultsVersion = ProjectileTrajectoryDefaultsVersion;
            SetBounceDefaults(
                spec.BounceEnabled,
                spec.BaseBounceCount,
                spec.MaxBounceCount,
                spec.EnemyBounceSearchRadius,
                spec.BounceOffWorldBlockers);
            bounceDefaultsVersion = BounceDefaultsVersion;
            SetExplosionDefaults(
                spec.ExplosiveEnabled,
                spec.BaseSplashRadius,
                spec.MaxSplashRadius,
                spec.SecondarySplashDamageMultiplier,
                spec.ExplodeOnWorldImpact,
                spec.ExplosionVfxEnabled);
            explosionDefaultsVersion = ExplosionDefaultsVersion;
            SetChainDefaults(
                spec.ChainEnabled,
                spec.BaseChainCount,
                spec.MaxChainCount,
                spec.ChainSearchRadius,
                spec.ChainDamageMultiplier,
                spec.ChainRequiresLineOfSight);
            chainDefaultsVersion = ChainDefaultsVersion;
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
            supportedUpgradeStats = new List<StatId>(spec.SupportedUpgradeStats);
            EnsureWorkflowDefaults();
        }

        public void ApplyCustomTemplateDefaults(string id, string name, WeaponFamilyTemplate template)
        {
            string safeId = ContentId.Normalize(id);
            weaponId = string.IsNullOrWhiteSpace(safeId) ? "custom_weapon" : safeId;
            displayName = string.IsNullOrWhiteSpace(name) ? "Custom Weapon" : name;
            tags = TagsForTemplate(template);
            isActive = true;
            availabilityDefaultsVersion = AvailabilityDefaultsVersion;
            canAppearAsLevelUpWeapon = true;
            addWeaponShortDescription = $"Add {displayName} to the run loadout.";

            baseFireIntervalSeconds = 0.7f;
            minimumFireIntervalSeconds = WeaponCooldownRules.MinimumFireInterval;
            projectileDamage = 20;
            projectileSpeed = 20f;
            projectileLifetimeSeconds = 3f;
            projectileHitRadius = 0.55f;
            projectileHitMask = LayerMask.GetMask(GameLayers.Enemy);
            weaponRange = 24f;
            projectileSpawnForwardOffset = 0.9f;
            projectileSpawnHeight = 0.8f;

            int environmentMask = GameLayers.EnvironmentMaskExcludingGameplay;
            requireLineOfSight = true;
            lineOfSightMask = environmentMask;
            projectileBlockMask = environmentMask;
            projectileBlockRadius = 0.14f;

            SetProjectileVisualDefaults(
                ProjectileVisualShape.Sphere,
                1f,
                new Color(1f, 0.86f, 0.25f, 1f),
                new Color(1f, 0.8f, 0.25f, 1f),
                0.65f,
                true,
                new Color(1f, 0.86f, 0.25f, 0.75f),
                0.08f,
                0.18f);
            SetProjectileEmissionDefaults(1, 1, 0f, true);
            SetProjectileAimDefaults(0f, true);
            SetProjectileTrajectoryDefaults(ProjectileTrajectoryMode.Direct, 0f);
            SetBounceDefaults(false, 0, 0, 12f, false);
            SetExplosionDefaults(false, 0f, 0f, 0.5f, false, false);
            SetChainDefaults(false, 0, 0, 10f, 0.65f, true);
            SetOrbitDefaults(false, 1, 1, 2.2f, 3.6f, 0.55f, 1.1f, 150f, 0.9f);

            supportedUpgradeStats = SupportedStatsForTemplate(template);
            ApplyTemplateFamily(template);

            projectileCollisionDefaultsVersion = ProjectileCollisionDefaultsVersion;
            projectileVisualDefaultsVersion = ProjectileVisualDefaultsVersion;
            projectileEmissionDefaultsVersion = ProjectileEmissionDefaultsVersion;
            projectileAimDefaultsVersion = ProjectileAimDefaultsVersion;
            projectileTrajectoryDefaultsVersion = ProjectileTrajectoryDefaultsVersion;
            acquisitionDefaultsVersion = AcquisitionDefaultsVersion;
            bounceDefaultsVersion = BounceDefaultsVersion;
            explosionDefaultsVersion = ExplosionDefaultsVersion;
            chainDefaultsVersion = ChainDefaultsVersion;
            orbitDefaultsVersion = OrbitDefaultsVersion;
            EnsureWorkflowDefaults();
        }

        public bool EnsureWorkflowDefaults()
        {
            bool changed = false;
            FirstPartyWeaponSpec defaultSpec = FirstPartyWeaponDefaults.GetOrDefault(weaponId);
            changed |= EnsureString(ref weaponId, FirstPartyWeaponDefaults.DefaultWeapon.Id);
            changed |= EnsureString(ref displayName, defaultSpec.DisplayName);
            changed |= EnsureAvailabilityDefaults();
            changed |= EnsureAddWeaponCardDefaults(defaultSpec);
            changed |= EnsureMinimum(ref baseFireIntervalSeconds, 0.36f, 0.01f);
            changed |= EnsureMinimum(ref minimumFireIntervalSeconds, WeaponCooldownRules.MinimumFireInterval, 0.01f);
            if (minimumFireIntervalSeconds > baseFireIntervalSeconds)
            {
                minimumFireIntervalSeconds = Mathf.Min(WeaponCooldownRules.MinimumFireInterval, baseFireIntervalSeconds);
                changed = true;
            }

            changed |= EnsureMinimum(ref projectileDamage, 20, 1);
            changed |= EnsureMinimum(ref projectileSpeed, 24f, 0f);
            changed |= EnsureMinimum(ref projectileLifetimeSeconds, 3.25f, 0.01f);
            changed |= EnsureMinimum(ref projectileHitRadius, 0.85f, 0.01f);
            changed |= EnsureMinimum(ref weaponRange, 30f, 0.01f);
            changed |= EnsureMinimum(ref projectileSpawnForwardOffset, 0.9f, 0f);
            changed |= EnsureMinimum(ref projectileSpawnHeight, 0.8f, 0.01f);
            if (projectileHitMask.value == 0)
            {
                projectileHitMask = LayerMask.GetMask(GameLayers.Enemy);
                changed = true;
            }

            changed |= EnsureProjectileCollisionDefaults();
            changed |= EnsureProjectileVisualDefaults();
            changed |= EnsureProjectileEmissionDefaults();
            changed |= EnsureProjectileAimDefaults();
            changed |= EnsureProjectileTrajectoryDefaults();
            changed |= EnsureAcquisitionDefaults();
            changed |= EnsureBounceDefaults();
            changed |= EnsureExplosionDefaults();
            changed |= EnsureChainDefaults();
            changed |= EnsureOrbitDefaults();
            changed |= EnsureMinimum(ref projectileBlockRadius, DefaultProjectileBlockRadiusForId(), 0.001f);
            changed |= EnsureMinimum(ref baseProjectileCount, 1, 1);
            changed |= EnsureMinimum(ref maxProjectileCount, Mathf.Max(1, baseProjectileCount), 1);
            if (maxProjectileCount < baseProjectileCount)
            {
                maxProjectileCount = baseProjectileCount;
                changed = true;
            }

            changed |= EnsureMinimum(ref baseSpreadAngleDegrees, 0f, 0f);
            changed |= EnsureMinimum(ref baseAimErrorDegrees, 0f, 0f);
            changed |= EnsureMinimum(ref projectileArcHeight, 0f, 0f);
            changed |= EnsureMinimum(ref baseBounceCount, 0, 0);
            changed |= EnsureMinimum(ref maxBounceCount, Mathf.Max(0, baseBounceCount), 0);
            if (maxBounceCount < baseBounceCount)
            {
                maxBounceCount = baseBounceCount;
                changed = true;
            }

            changed |= EnsureMinimum(ref enemyBounceSearchRadius, 12f, 0.01f);
            changed |= EnsureMinimum(ref baseSplashRadius, 0f, 0f);
            changed |= EnsureMinimum(ref maxSplashRadius, Mathf.Max(0f, baseSplashRadius), 0f);
            if (maxSplashRadius < baseSplashRadius)
            {
                maxSplashRadius = baseSplashRadius;
                changed = true;
            }

            float clampedSecondaryMultiplier = Mathf.Clamp01(secondarySplashDamageMultiplier);
            if (!Mathf.Approximately(clampedSecondaryMultiplier, secondarySplashDamageMultiplier))
            {
                secondarySplashDamageMultiplier = clampedSecondaryMultiplier;
                changed = true;
            }

            changed |= EnsureMinimum(ref baseChainCount, 0, 0);
            changed |= EnsureMinimum(ref maxChainCount, Mathf.Max(0, baseChainCount), 0);
            if (maxChainCount < baseChainCount)
            {
                maxChainCount = baseChainCount;
                changed = true;
            }

            changed |= EnsureMinimum(ref chainSearchRadius, 10f, 0.01f);
            float clampedChainDamageMultiplier = Mathf.Clamp01(chainDamageMultiplier);
            if (!Mathf.Approximately(clampedChainDamageMultiplier, chainDamageMultiplier))
            {
                chainDamageMultiplier = clampedChainDamageMultiplier;
                changed = true;
            }

            changed |= EnsureMinimum(ref baseOrbitCount, 1, 1);
            changed |= EnsureMinimum(ref maxOrbitCount, Mathf.Max(1, baseOrbitCount), 1);
            if (maxOrbitCount < baseOrbitCount)
            {
                maxOrbitCount = baseOrbitCount;
                changed = true;
            }

            changed |= EnsureMinimum(ref orbitRadius, 2.2f, 0.01f);
            changed |= EnsureMinimum(ref maxOrbitRadius, Mathf.Max(orbitRadius, 3.6f), 0.01f);
            if (maxOrbitRadius < orbitRadius)
            {
                maxOrbitRadius = orbitRadius;
                changed = true;
            }

            changed |= EnsureMinimum(ref orbitHitRadius, 0.55f, 0.01f);
            changed |= EnsureMinimum(ref maxOrbitHitRadius, Mathf.Max(orbitHitRadius, 1.1f), 0.01f);
            if (maxOrbitHitRadius < orbitHitRadius)
            {
                maxOrbitHitRadius = orbitHitRadius;
                changed = true;
            }

            changed |= EnsureMinimum(ref orbitDegreesPerSecond, 150f, 0f);
            changed |= EnsureMinimum(ref orbitHeightOffset, 0.9f, 0f);
            float clampedOrbitAreaDamageMultiplier = Mathf.Clamp01(orbitAreaDamageMultiplier);
            if (!Mathf.Approximately(clampedOrbitAreaDamageMultiplier, orbitAreaDamageMultiplier))
            {
                orbitAreaDamageMultiplier = clampedOrbitAreaDamageMultiplier;
                changed = true;
            }

            changed |= EnsureMinimum(ref orbitAreaHitIntervalSeconds, 0.45f, 0.01f);

            changed |= EnsureMinimum(ref projectileVisualScale, 1f, 0.05f);
            changed |= EnsureMinimum(ref projectileEmissionStrength, 0.65f, 0f);
            changed |= EnsureMinimum(ref projectileTrailWidth, 0.08f, 0.001f);
            changed |= EnsureMinimum(ref projectileTrailLifetime, 0.18f, 0.01f);
            upgradeTrack ??= new List<UpgradeDefinition>();
            changed |= EnsureSupportedUpgradeStats();
            return changed;
        }

        private bool EnsureAvailabilityDefaults()
        {
            if (availabilityDefaultsVersion >= AvailabilityDefaultsVersion)
            {
                return false;
            }

            isActive = true;
            availabilityDefaultsVersion = AvailabilityDefaultsVersion;
            return true;
        }

        public void SetProjectileEmissionDefaults(int baseCount, int maxCount, float spreadAngleDegrees, bool useAccuracy)
        {
            baseProjectileCount = Mathf.Max(1, baseCount);
            maxProjectileCount = Mathf.Max(baseProjectileCount, maxCount);
            baseSpreadAngleDegrees = Mathf.Max(0f, spreadAngleDegrees);
            accuracyAffectsSpread = useAccuracy;
        }

        public void SetProjectileAimDefaults(float aimErrorDegrees, bool useAccuracy)
        {
            baseAimErrorDegrees = Mathf.Max(0f, aimErrorDegrees);
            accuracyAffectsAimError = useAccuracy;
        }

        public void SetProjectileTrajectoryDefaults(ProjectileTrajectoryMode mode, float arcHeight)
        {
            projectileTrajectoryMode = mode;
            projectileArcHeight = Mathf.Max(0f, arcHeight);
        }

        public void SetBounceDefaults(bool enabled, int baseCount, int maxCount, float enemySearchRadius, bool bounceWorldBlockers)
        {
            bounceEnabled = enabled;
            baseBounceCount = Mathf.Max(0, baseCount);
            maxBounceCount = Mathf.Max(baseBounceCount, maxCount);
            enemyBounceSearchRadius = Mathf.Max(0.01f, enemySearchRadius);
            bounceOffWorldBlockers = bounceWorldBlockers;
        }

        public void SetExplosionDefaults(
            bool enabled,
            float baseRadius,
            float maxRadius,
            float secondaryDamageMultiplier,
            bool explodeWorldImpact,
            bool enableVfx)
        {
            explosiveEnabled = enabled;
            baseSplashRadius = Mathf.Max(0f, baseRadius);
            maxSplashRadius = Mathf.Max(baseSplashRadius, maxRadius);
            secondarySplashDamageMultiplier = Mathf.Clamp01(secondaryDamageMultiplier);
            explodeOnWorldImpact = explodeWorldImpact;
            explosionVfxEnabled = enableVfx;
        }

        public void SetChainDefaults(
            bool enabled,
            int baseCount,
            int maxCount,
            float searchRadius,
            float damageMultiplier,
            bool requiresLineOfSight)
        {
            chainEnabled = enabled;
            baseChainCount = Mathf.Max(0, baseCount);
            maxChainCount = Mathf.Max(baseChainCount, maxCount);
            chainSearchRadius = Mathf.Max(0.01f, searchRadius);
            chainDamageMultiplier = Mathf.Clamp01(damageMultiplier);
            chainRequiresLineOfSight = requiresLineOfSight;
        }

        public void SetOrbitDefaults(
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
            orbitEnabled = enabled;
            baseOrbitCount = Mathf.Max(1, baseCount);
            maxOrbitCount = Mathf.Max(baseOrbitCount, maxCount);
            orbitRadius = Mathf.Max(0.01f, radius);
            maxOrbitRadius = Mathf.Max(orbitRadius, maxRadius);
            orbitHitRadius = Mathf.Max(0.01f, hitRadius);
            maxOrbitHitRadius = Mathf.Max(orbitHitRadius, maxHitRadius);
            orbitDegreesPerSecond = Mathf.Max(0f, degreesPerSecond);
            orbitHeightOffset = Mathf.Max(0f, heightOffset);
            orbitAreaDamageEnabled = areaDamageEnabled;
            orbitAreaDamageMultiplier = Mathf.Clamp01(areaDamageMultiplier);
            orbitAreaHitIntervalSeconds = Mathf.Max(0.01f, areaHitIntervalSeconds);
        }

        public bool EnsureUpgradeTrackDefaults(params UpgradeDefinition[] upgrades)
        {
            bool changed = EnsureWorkflowDefaults();
            if (upgrades == null)
            {
                return changed;
            }

            for (int i = 0; i < upgrades.Length; i++)
            {
                UpgradeDefinition upgrade = upgrades[i];
                if (upgrade == null || upgradeTrack.Contains(upgrade))
                {
                    continue;
                }

                upgradeTrack.Add(upgrade);
                changed = true;
            }

            return changed;
        }

        private void OnValidate()
        {
            EnsureWorkflowDefaults();
        }

        private bool EnsureAddWeaponCardDefaults(FirstPartyWeaponSpec defaultSpec)
        {
            if (!string.IsNullOrWhiteSpace(addWeaponShortDescription))
            {
                return false;
            }

            addWeaponShortDescription = string.IsNullOrWhiteSpace(defaultSpec.AddWeaponShortDescription)
                ? $"Add {DisplayName} to the run loadout."
                : defaultSpec.AddWeaponShortDescription;
            return true;
        }

        private void ApplyTemplateFamily(WeaponFamilyTemplate template)
        {
            switch (template)
            {
                case WeaponFamilyTemplate.SpreadProjectile:
                    SetProjectileVisualDefaults(
                        ProjectileVisualShape.Shard,
                        0.75f,
                        new Color(0.93f, 0.94f, 0.88f, 1f),
                        new Color(0.78f, 0.95f, 1f, 1f),
                        0.5f,
                        true,
                        new Color(0.82f, 0.92f, 1f, 0.65f),
                        0.045f,
                        0.12f);
                    SetProjectileEmissionDefaults(3, 7, 18f, true);
                    SetProjectileAimDefaults(4f, true);
                    projectileHitRadius = 0.35f;
                    break;
                case WeaponFamilyTemplate.BounceProjectile:
                    SetProjectileVisualDefaults(
                        ProjectileVisualShape.Sphere,
                        1.1f,
                        new Color(0.96f, 0.78f, 0.28f, 1f),
                        new Color(1f, 0.72f, 0.18f, 1f),
                        0.65f,
                        true,
                        new Color(1f, 0.78f, 0.24f, 0.7f),
                        0.09f,
                        0.2f);
                    SetProjectileAimDefaults(5f, true);
                    SetBounceDefaults(true, 1, 4, 12f, true);
                    projectileHitRadius = 0.6f;
                    break;
                case WeaponFamilyTemplate.ExplosiveArcProjectile:
                    SetProjectileVisualDefaults(
                        ProjectileVisualShape.Sphere,
                        1.25f,
                        new Color(1f, 0.48f, 0.22f, 1f),
                        new Color(1f, 0.32f, 0.12f, 1f),
                        0.7f,
                        true,
                        new Color(1f, 0.42f, 0.18f, 0.7f),
                        0.12f,
                        0.22f);
                    SetProjectileTrajectoryDefaults(ProjectileTrajectoryMode.Arc, 1.8f);
                    SetExplosionDefaults(true, 1.6f, 3.2f, 0.5f, true, true);
                    projectileDamage = 40;
                    projectileSpeed = 16f;
                    projectileHitRadius = 0.5f;
                    break;
                case WeaponFamilyTemplate.ChainProjectile:
                    SetProjectileVisualDefaults(
                        ProjectileVisualShape.Bolt,
                        0.9f,
                        new Color(0.5f, 0.93f, 1f, 1f),
                        new Color(0.28f, 0.85f, 1f, 1f),
                        0.85f,
                        true,
                        new Color(0.4f, 0.9f, 1f, 0.75f),
                        0.07f,
                        0.16f);
                    SetChainDefaults(true, 2, 6, 10f, 0.65f, true);
                    projectileDamage = 20;
                    projectileSpeed = 26f;
                    projectileHitRadius = 0.4f;
                    break;
                case WeaponFamilyTemplate.Orbit:
                    SetProjectileVisualDefaults(
                        ProjectileVisualShape.Sphere,
                        1f,
                        new Color(1f, 0.38f, 0.16f, 1f),
                        new Color(1f, 0.22f, 0.08f, 1f),
                        0.85f,
                        false,
                        new Color(1f, 0.4f, 0.12f, 0.6f),
                        0.08f,
                        0.18f);
                    SetOrbitDefaults(true, 1, 4, 2.2f, 3.6f, 0.55f, 1.1f, 150f, 0.9f, true, 0.55f, 0.45f);
                    projectileDamage = 20;
                    projectileHitRadius = 0.55f;
                    break;
                case WeaponFamilyTemplate.DirectProjectile:
                default:
                    break;
            }
        }

        private static ContentTagSet TagsForTemplate(WeaponFamilyTemplate template)
        {
            return template switch
            {
                WeaponFamilyTemplate.ExplosiveArcProjectile => ContentTagSet.With(ContentTag.Projectile, ContentTag.Explosive, ContentTag.Physical),
                WeaponFamilyTemplate.ChainProjectile => ContentTagSet.With(ContentTag.Projectile, ContentTag.Lightning, ContentTag.Magic),
                WeaponFamilyTemplate.Orbit => ContentTagSet.With(ContentTag.Aura, ContentTag.Fire, ContentTag.Magic),
                WeaponFamilyTemplate.BounceProjectile => ContentTagSet.With(ContentTag.Projectile, ContentTag.Physical),
                WeaponFamilyTemplate.SpreadProjectile => ContentTagSet.With(ContentTag.Projectile, ContentTag.Physical),
                _ => ContentTagSet.With(ContentTag.Projectile, ContentTag.Physical),
            };
        }

        private static List<StatId> SupportedStatsForTemplate(WeaponFamilyTemplate template)
        {
            return template switch
            {
                WeaponFamilyTemplate.SpreadProjectile => new List<StatId>
                {
                    StatId.WeaponProjectileCount,
                    StatId.WeaponAccuracyMultiplier,
                    StatId.WeaponFlatDamage,
                    StatId.WeaponAttackSpeed,
                    StatId.WeaponProjectileSpeedMultiplier,
                    StatId.WeaponProjectileSizeMultiplier,
                    StatId.WeaponProjectileLifetimeMultiplier,
                },
                WeaponFamilyTemplate.BounceProjectile => new List<StatId>
                {
                    StatId.WeaponBounce,
                    StatId.WeaponFlatDamage,
                    StatId.WeaponAttackSpeed,
                    StatId.WeaponAccuracyMultiplier,
                    StatId.WeaponProjectileSpeedMultiplier,
                    StatId.WeaponProjectileSizeMultiplier,
                    StatId.WeaponProjectileLifetimeMultiplier,
                },
                WeaponFamilyTemplate.ExplosiveArcProjectile => new List<StatId>
                {
                    StatId.WeaponSplashRadiusMultiplier,
                    StatId.WeaponFlatDamage,
                    StatId.WeaponAttackSpeed,
                    StatId.WeaponProjectileSpeedMultiplier,
                    StatId.WeaponProjectileSizeMultiplier,
                    StatId.WeaponProjectileLifetimeMultiplier,
                },
                WeaponFamilyTemplate.ChainProjectile => new List<StatId>
                {
                    StatId.WeaponChain,
                    StatId.WeaponFlatDamage,
                    StatId.WeaponAttackSpeed,
                    StatId.WeaponProjectileSpeedMultiplier,
                    StatId.WeaponProjectileSizeMultiplier,
                    StatId.WeaponProjectileLifetimeMultiplier,
                },
                WeaponFamilyTemplate.Orbit => new List<StatId>
                {
                    StatId.WeaponFlatDamage,
                    StatId.WeaponAttackSpeed,
                    StatId.WeaponProjectileCount,
                    StatId.WeaponProjectileSizeMultiplier,
                    StatId.WeaponRangeMultiplier,
                },
                _ => new List<StatId>
                {
                    StatId.WeaponFlatDamage,
                    StatId.WeaponAttackSpeed,
                    StatId.WeaponProjectileSpeedMultiplier,
                    StatId.WeaponProjectileSizeMultiplier,
                    StatId.WeaponProjectileLifetimeMultiplier,
                    StatId.WeaponRangeMultiplier,
                },
            };
        }
    }
}
