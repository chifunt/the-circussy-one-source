using System.Collections.Generic;
using System.Text;
using TheCircussyOne.Stats;

namespace TheCircussyOne.Content
{
    public enum WeaponAuthoringArea
    {
        SharedDamage,
        SharedTiming,
        SharedEnemyHitMask,
        SharedLineOfSight,
        SharedVisualColor,
        ProjectileTravel,
        ProjectileEmission,
        ProjectileAimError,
        ProjectileTrajectory,
        ProjectileCollision,
        ProjectileSpawnPose,
        ProjectileTrail,
        Bounce,
        Explosion,
        Chain,
        Orbit,
        OrbitAreaHeat,
    }

    public enum WeaponFamilyTemplate
    {
        DirectProjectile,
        SpreadProjectile,
        BounceProjectile,
        ExplosiveArcProjectile,
        ChainProjectile,
        Orbit,
    }

    public static class WeaponAuthoringApplicability
    {
        public static bool IsActive(WeaponDefinition weapon, WeaponAuthoringArea area)
        {
            if (weapon == null)
            {
                return false;
            }

            bool isOrbit = weapon.orbitEnabled;
            return area switch
            {
                WeaponAuthoringArea.SharedDamage => true,
                WeaponAuthoringArea.SharedTiming => true,
                WeaponAuthoringArea.SharedEnemyHitMask => true,
                WeaponAuthoringArea.SharedLineOfSight => true,
                WeaponAuthoringArea.SharedVisualColor => true,
                WeaponAuthoringArea.ProjectileTravel => !isOrbit,
                WeaponAuthoringArea.ProjectileEmission => !isOrbit,
                WeaponAuthoringArea.ProjectileAimError => !isOrbit,
                WeaponAuthoringArea.ProjectileTrajectory => !isOrbit,
                WeaponAuthoringArea.ProjectileCollision => !isOrbit,
                WeaponAuthoringArea.ProjectileSpawnPose => !isOrbit,
                WeaponAuthoringArea.ProjectileTrail => !isOrbit,
                WeaponAuthoringArea.Bounce => !isOrbit && weapon.bounceEnabled,
                WeaponAuthoringArea.Explosion => !isOrbit && weapon.explosiveEnabled,
                WeaponAuthoringArea.Chain => !isOrbit && weapon.chainEnabled,
                WeaponAuthoringArea.Orbit => isOrbit,
                WeaponAuthoringArea.OrbitAreaHeat => isOrbit && weapon.orbitAreaDamageEnabled,
                _ => true,
            };
        }

        public static string DisabledReason(WeaponDefinition weapon, WeaponAuthoringArea area)
        {
            if (weapon == null)
            {
                return "Disabled because no weapon definition is selected.";
            }

            if (IsActive(weapon, area))
            {
                return string.Empty;
            }

            bool isOrbit = weapon.orbitEnabled;
            return area switch
            {
                WeaponAuthoringArea.ProjectileTravel when isOrbit => "Disabled because orbit weapons do not spawn traveling projectiles.",
                WeaponAuthoringArea.ProjectileEmission when isOrbit => "Disabled because orbit weapons do not emit projectile volleys.",
                WeaponAuthoringArea.ProjectileAimError when isOrbit => "Disabled because orbit weapons do not aim traveling projectiles.",
                WeaponAuthoringArea.ProjectileTrajectory when isOrbit => "Disabled because orbit weapons do not evaluate projectile trajectories.",
                WeaponAuthoringArea.ProjectileCollision when isOrbit => "Disabled because orbit weapons do not use projectile travel blockers.",
                WeaponAuthoringArea.ProjectileSpawnPose when isOrbit => "Disabled because orbit weapons follow the player instead of spawning from a muzzle pose.",
                WeaponAuthoringArea.ProjectileTrail when isOrbit => "Disabled because orbit weapons use orbit visuals instead of projectile trails.",
                WeaponAuthoringArea.Bounce when isOrbit => "Disabled because orbit weapons do not ricochet as projectiles.",
                WeaponAuthoringArea.Explosion when isOrbit => "Disabled because orbit weapons do not explode as projectiles.",
                WeaponAuthoringArea.Chain when isOrbit => "Disabled because orbit weapons do not chain as projectiles.",
                WeaponAuthoringArea.Bounce => "Enable Bounce to edit ricochet settings.",
                WeaponAuthoringArea.Explosion => "Enable Explosion to edit splash settings.",
                WeaponAuthoringArea.Chain => "Enable Chain to edit chain settings.",
                WeaponAuthoringArea.Orbit => "Enable Orbit to edit persistent orbit settings.",
                WeaponAuthoringArea.OrbitAreaHeat => "Enable Orbit and Area Damage to edit aura pulse settings.",
                _ => "Disabled because this setting does not affect the selected weapon family.",
            };
        }

        public static bool IsSupportedUpgradeStatActive(WeaponDefinition weapon, StatId statId)
        {
            if (weapon == null || !WeaponDefinition.IsWeaponLocalUpgradeStat(statId))
            {
                return false;
            }

            bool isOrbit = weapon.orbitEnabled;
            return statId switch
            {
                StatId.WeaponFlatDamage => true,
                StatId.WeaponAttackSpeed => true,
                StatId.WeaponProjectileCount => true,
                StatId.WeaponProjectileSizeMultiplier => true,
                StatId.WeaponRangeMultiplier => true,
                StatId.WeaponProjectileSpeedMultiplier => !isOrbit,
                StatId.WeaponProjectileLifetimeMultiplier => !isOrbit,
                StatId.WeaponAccuracyMultiplier => !isOrbit && weapon.accuracyAffectsAimError,
                StatId.WeaponSplashRadiusMultiplier => !isOrbit && weapon.explosiveEnabled,
                StatId.WeaponBounce => !isOrbit && weapon.bounceEnabled,
                StatId.WeaponChain => !isOrbit && weapon.chainEnabled,
                StatId.WeaponPierce => false,
                StatId.WeaponKnockbackMultiplier => false,
                _ => false,
            };
        }

        public static string FamilySummary(WeaponDefinition weapon)
        {
            if (weapon == null)
            {
                return "No weapon selected.";
            }

            return FamilyName(weapon);
        }

        public static string ActiveGameplayHooksSummary(WeaponDefinition weapon)
        {
            if (weapon == null)
            {
                return "No active hooks.";
            }

            var hooks = new List<string>();
            if (weapon.orbitEnabled)
            {
                hooks.Add("Orbit damage");
                if (weapon.orbitAreaDamageEnabled)
                {
                    hooks.Add("Orbit area heat");
                }
            }
            else
            {
                hooks.Add("Projectile travel");
                hooks.Add(weapon.requireLineOfSight ? "Line of sight required" : "Line of sight ignored");
                hooks.Add("World blocker sweep");
                if (weapon.baseProjectileCount > 1 || weapon.baseSpreadAngleDegrees > 0f)
                {
                    hooks.Add("Pattern spread");
                }

                if (weapon.baseAimErrorDegrees > 0f)
                {
                    hooks.Add("Aim error");
                }

                if (weapon.projectileTrajectoryMode == ProjectileTrajectoryMode.Arc)
                {
                    hooks.Add("Arc trajectory");
                }

                if (weapon.bounceEnabled)
                {
                    hooks.Add("Bounce");
                }

                if (weapon.explosiveEnabled)
                {
                    hooks.Add("Explosion");
                }

                if (weapon.chainEnabled)
                {
                    hooks.Add("Chain");
                }
            }

            return string.Join(", ", hooks);
        }

        public static string AcquisitionSummary(WeaponDefinition weapon)
        {
            if (weapon == null)
            {
                return "No weapon selected.";
            }

            return weapon.CanAppearAsLevelUpWeapon
                ? "Can appear as a normal add-weapon level-up card."
                : "Performer-only weapon. It should not appear as a normal add-weapon reward.";
        }

        public static string SupportedUpgradeStatsSummary(WeaponDefinition weapon)
        {
            if (weapon == null || weapon.SupportedUpgradeStats == null || weapon.SupportedUpgradeStats.Count == 0)
            {
                return "No weapon-local upgrade stats configured.";
            }

            var builder = new StringBuilder();
            var seen = new HashSet<StatId>();
            for (int i = 0; i < weapon.SupportedUpgradeStats.Count; i++)
            {
                StatId statId = weapon.SupportedUpgradeStats[i];
                if (!seen.Add(statId))
                {
                    continue;
                }

                if (builder.Length > 0)
                {
                    builder.Append(", ");
                }

                builder.Append(ContentModifierDisplayRules.StatDisplayName(statId));
                if (!IsSupportedUpgradeStatActive(weapon, statId))
                {
                    builder.Append(" (inactive)");
                }
            }

            return builder.ToString();
        }

        public static string InactiveSupportedUpgradeStatsWarning(WeaponDefinition weapon)
        {
            if (weapon == null || weapon.SupportedUpgradeStats == null || weapon.SupportedUpgradeStats.Count == 0)
            {
                return string.Empty;
            }

            var builder = new StringBuilder();
            var seen = new HashSet<StatId>();
            for (int i = 0; i < weapon.SupportedUpgradeStats.Count; i++)
            {
                StatId statId = weapon.SupportedUpgradeStats[i];
                if (!seen.Add(statId) || IsSupportedUpgradeStatActive(weapon, statId))
                {
                    continue;
                }

                if (builder.Length > 0)
                {
                    builder.AppendLine();
                }

                builder.Append(ContentModifierDisplayRules.StatDisplayName(statId));
                builder.Append(": ");
                builder.Append(InactiveSupportedUpgradeStatReason(weapon, statId));
            }

            return builder.ToString();
        }

        public static string TemplateDescription(WeaponFamilyTemplate template)
        {
            return template switch
            {
                WeaponFamilyTemplate.DirectProjectile => "Single direct projectile with line of sight, world blockers, aim error, and projectile visuals.",
                WeaponFamilyTemplate.SpreadProjectile => "Projectile volley family for weapons like Knife Fan. Projectile Count adds blades; Accuracy narrows aim error.",
                WeaponFamilyTemplate.BounceProjectile => "Ricochet projectile family for Juggling Ball. Bounce upgrades add enemy/world ricochets.",
                WeaponFamilyTemplate.ExplosiveArcProjectile => "Arcing explosive family for Cannon. Splash Radius upgrades scale the exact explosion radius.",
                WeaponFamilyTemplate.ChainProjectile => "Spotlight-style projectile that chains secondary hits to visible nearby enemies.",
                WeaponFamilyTemplate.Orbit => "Persistent orbit family. It follows the player, skips normal projectile travel, and can use orbit area heat.",
                _ => "Unknown template."
            };
        }

        public static string TemplatePreview(WeaponFamilyTemplate template)
        {
            return $"{TemplateDescription(template)}\nCreates: weapon definition"
                + (template == WeaponFamilyTemplate.Orbit ? " with orbit defaults" : " with projectile defaults")
                + ". Enable the weapon's Level-Up Card section if it should appear as an add-weapon reward.";
        }

        private static string FamilyName(WeaponDefinition weapon)
        {
            if (weapon.orbitEnabled)
            {
                return weapon.orbitAreaDamageEnabled ? "Orbit + Area Heat" : "Orbit";
            }

            if (weapon.explosiveEnabled && weapon.projectileTrajectoryMode == ProjectileTrajectoryMode.Arc)
            {
                return "Explosive Arc Projectile";
            }

            if (weapon.explosiveEnabled)
            {
                return "Explosive Projectile";
            }

            if (weapon.chainEnabled)
            {
                return "Chain Projectile";
            }

            if (weapon.bounceEnabled)
            {
                return "Bounce Projectile";
            }

            if (weapon.baseProjectileCount > 1 || weapon.baseSpreadAngleDegrees > 0f)
            {
                return "Spread Projectile";
            }

            return "Direct Projectile";
        }

        private static string InactiveSupportedUpgradeStatReason(WeaponDefinition weapon, StatId statId)
        {
            if (!WeaponDefinition.IsWeaponLocalUpgradeStat(statId))
            {
                return "not a weapon-local stat.";
            }

            if (weapon.orbitEnabled)
            {
                return statId switch
                {
                    StatId.WeaponProjectileSpeedMultiplier => "orbit weapons do not use projectile speed.",
                    StatId.WeaponProjectileLifetimeMultiplier => "orbit weapons do not use projectile lifetime.",
                    StatId.WeaponAccuracyMultiplier => "orbit weapons do not aim traveling projectiles.",
                    StatId.WeaponSplashRadiusMultiplier => "orbit weapons do not use projectile splash.",
                    StatId.WeaponBounce => "orbit weapons do not ricochet.",
                    StatId.WeaponChain => "orbit weapons do not chain.",
                    _ => "this stat has no active runtime hook for the current weapon family."
                };
            }

            return statId switch
            {
                StatId.WeaponAccuracyMultiplier when !weapon.accuracyAffectsAimError => "Accuracy is disabled because Aim Error is not affected by accuracy.",
                StatId.WeaponSplashRadiusMultiplier when !weapon.explosiveEnabled => "Enable Explosion before splash radius upgrades can do anything.",
                StatId.WeaponBounce when !weapon.bounceEnabled => "Enable Bounce before bounce upgrades can do anything.",
                StatId.WeaponChain when !weapon.chainEnabled => "Enable Chain before chain upgrades can do anything.",
                StatId.WeaponPierce => "Pierce is not implemented yet.",
                StatId.WeaponKnockbackMultiplier => "Weapon knockback is not implemented yet.",
                _ => "this stat has no active runtime hook for the current weapon family."
            };
        }
    }
}
