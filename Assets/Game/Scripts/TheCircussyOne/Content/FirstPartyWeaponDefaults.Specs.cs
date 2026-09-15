using UnityEngine;
using TheCircussyOne.Rules;

namespace TheCircussyOne.Content
{
    public static partial class FirstPartyWeaponDefaults
    {
        private static readonly FirstPartyWeaponSpec[] Specs =
        {
            CannonSpec(),
            KnifeFanSpec(),
            SpotlightBoltSpec(),
            FireHoopSpec(),
            JugglingBallSpec()
        };

        private static FirstPartyWeaponSpec CannonSpec()
        {
            return new FirstPartyWeaponSpec(
                CannonId,
                "Cannon",
                new[] { ContentTag.Projectile, ContentTag.Physical, ContentTag.Explosive },
                new WeaponTimingDefaults(1.05f, 0.25f),
                new ProjectileGameplayDefaults(132, 18f, 3.5f, 1.05f, 32f, 1.1f, 0.9f, 0.24f),
                new ProjectileVisualDefaults(
                    ProjectileVisualShape.Sphere,
                    1.55f,
                    new Color(1f, 0.43f, 0.18f, 1f),
                    new Color(1f, 0.62f, 0.18f, 1f),
                    0.9f,
                    true,
                    new Color(1f, 0.45f, 0.12f, 0.7f),
                    0.13f,
                    0.22f),
                CannonSupportedStats,
                addWeaponShortDescription: "Add a heavy arcing cannon shot that explodes on impact.",
                emission: ProjectileEmissionDefaults.Single,
                trajectory: new ProjectileTrajectoryDefaults(ProjectileTrajectoryMode.Arc, 2.8f),
                bounce: ProjectileBounceDefaults.Disabled,
                explosion: new ProjectileExplosionDefaults(true, 2.2f, 5f, 0.5f, true, true),
                chain: ProjectileChainDefaults.Disabled,
                orbit: OrbitWeaponDefaults.Disabled);
        }

        private static FirstPartyWeaponSpec KnifeFanSpec()
        {
            return new FirstPartyWeaponSpec(
                KnifeFanId,
                "Knife Fan",
                new[] { ContentTag.Projectile, ContentTag.Physical, ContentTag.Ranged, ContentTag.Rapid },
                new WeaponTimingDefaults(0.18f, 0.08f),
                new ProjectileGameplayDefaults(10, 28f, 2.3f, 0.45f, 24f, 0.85f, 0.85f, 0.07f),
                new ProjectileVisualDefaults(
                    ProjectileVisualShape.Shard,
                    0.72f,
                    new Color(0.96f, 0.96f, 0.86f, 1f),
                    new Color(1f, 0.95f, 0.65f, 1f),
                    0.45f,
                    true,
                    new Color(1f, 0.96f, 0.78f, 0.55f),
                    0.035f,
                    0.12f),
                KnifeFanSupportedStats,
                addWeaponShortDescription: "Add a fast fan of knives with spread and accuracy scaling.",
                emission: new ProjectileEmissionDefaults(3, 7, 16f, true),
                aim: new ProjectileAimDefaults(4f, true),
                trajectory: ProjectileTrajectoryDefaults.Direct,
                bounce: ProjectileBounceDefaults.Disabled,
                explosion: ProjectileExplosionDefaults.Disabled,
                chain: ProjectileChainDefaults.Disabled,
                orbit: OrbitWeaponDefaults.Disabled);
        }

        private static FirstPartyWeaponSpec SpotlightBoltSpec()
        {
            return new FirstPartyWeaponSpec(
                SpotlightBoltId,
                "Spotlight Bolt",
                new[] { ContentTag.Projectile, ContentTag.Lightning, ContentTag.Magic, ContentTag.Chain },
                new WeaponTimingDefaults(0.48f, 0.14f),
                new ProjectileGameplayDefaults(42, 34f, 2.6f, 0.7f, 34f, 0.95f, 0.9f, 0.1f),
                new ProjectileVisualDefaults(
                    ProjectileVisualShape.Bolt,
                    0.9f,
                    new Color(0.36f, 0.9f, 1f, 1f),
                    new Color(0.22f, 0.78f, 1f, 1f),
                    1.15f,
                    true,
                    new Color(0.24f, 0.86f, 1f, 0.72f),
                    0.07f,
                    0.2f),
                SpotlightBoltSupportedStats,
                addWeaponShortDescription: "Add a bright bolt that chains between visible enemies.",
                emission: ProjectileEmissionDefaults.Single,
                trajectory: ProjectileTrajectoryDefaults.Direct,
                bounce: ProjectileBounceDefaults.Disabled,
                explosion: ProjectileExplosionDefaults.Disabled,
                chain: new ProjectileChainDefaults(true, 2, 6, 10f, 0.65f, true),
                orbit: OrbitWeaponDefaults.Disabled);
        }

        private static FirstPartyWeaponSpec FireHoopSpec()
        {
            return new FirstPartyWeaponSpec(
                FireHoopId,
                "Fire Hoop",
                new[] { ContentTag.Projectile, ContentTag.Fire, ContentTag.Magic, ContentTag.Orbit },
                new WeaponTimingDefaults(0.5f, 0.12f),
                new ProjectileGameplayDefaults(55, 14f, 5f, 1.15f, 28f, 1f, 0.95f, 0.18f),
                new ProjectileVisualDefaults(
                    ProjectileVisualShape.Sphere,
                    1.25f,
                    new Color(1f, 0.22f, 0.08f, 1f),
                    new Color(1f, 0.52f, 0.09f, 1f),
                    1.2f,
                    true,
                    new Color(1f, 0.18f, 0.04f, 0.7f),
                    0.11f,
                    0.28f),
                FireHoopSupportedStats,
                addWeaponShortDescription: "Add orbiting fire hoops that burn nearby visible enemies.",
                emission: ProjectileEmissionDefaults.Single,
                trajectory: ProjectileTrajectoryDefaults.Direct,
                bounce: ProjectileBounceDefaults.Disabled,
                explosion: ProjectileExplosionDefaults.Disabled,
                chain: ProjectileChainDefaults.Disabled,
                orbit: new OrbitWeaponDefaults(true, 2, 4, 2.2f, 3.6f, 0.75f, 1.25f, 240f, 0.9f, true, 0.55f, 0.45f));
        }

        private static FirstPartyWeaponSpec JugglingBallSpec()
        {
            return new FirstPartyWeaponSpec(
                JugglingBallId,
                "Juggling Ball",
                new[] { ContentTag.Projectile, ContentTag.Physical, ContentTag.Bounce },
                new WeaponTimingDefaults(0.42f, 0.12f),
                new ProjectileGameplayDefaults(51, 25f, 3.2f, 0.75f, 30f, 0.95f, 0.9f, 0.11f),
                new ProjectileVisualDefaults(
                    ProjectileVisualShape.Sphere,
                    1f,
                    new Color(0.95f, 0.22f, 0.36f, 1f),
                    new Color(1f, 0.82f, 0.18f, 1f),
                    0.75f,
                    true,
                    new Color(1f, 0.78f, 0.18f, 0.62f),
                    0.075f,
                    0.18f),
                JugglingBallSupportedStats,
                addWeaponShortDescription: "Add a bouncing juggling ball that ricochets between enemies.",
                emission: ProjectileEmissionDefaults.Single,
                aim: new ProjectileAimDefaults(7f, true),
                trajectory: ProjectileTrajectoryDefaults.Direct,
                bounce: new ProjectileBounceDefaults(true, 1, 4, 14f, true),
                explosion: ProjectileExplosionDefaults.Disabled,
                chain: ProjectileChainDefaults.Disabled,
                orbit: OrbitWeaponDefaults.Disabled);
        }
    }
}
