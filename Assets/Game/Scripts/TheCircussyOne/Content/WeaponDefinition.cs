using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using TheCircussyOne.Rules;
using TheCircussyOne.Stats;

namespace TheCircussyOne.Content
{
    public enum ProjectileVisualShape
    {
        Sphere,
        Capsule,
        Cube,
        Shard,
        Bolt
    }

    public enum ProjectileTrajectoryMode
    {
        Direct,
        Arc
    }

    [CreateAssetMenu(menuName = "The Circussy One/Content/Weapon Definition", fileName = "WeaponDefinition")]
    public sealed partial class WeaponDefinition : SerializedScriptableObject, IActivatableContentDefinition
    {
        private const string Tabs = "Weapon";
        private const int AvailabilityDefaultsVersion = 1;
        private const int ProjectileCollisionDefaultsVersion = 2;
        private const int ProjectileVisualDefaultsVersion = 1;
        private const int ProjectileEmissionDefaultsVersion = 1;
        private const int ProjectileAimDefaultsVersion = 1;
        private const int ProjectileTrajectoryDefaultsVersion = 1;
        private const int AcquisitionDefaultsVersion = 1;
        private const int BounceDefaultsVersion = 1;
        private const int ExplosionDefaultsVersion = 1;
        private const int ChainDefaultsVersion = 1;
        private const int OrbitDefaultsVersion = 1;

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Core"), LabelWidth(140), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.BalanceColor"), PropertyOrder(0)]
        [ValidateInput(nameof(HasWeaponId), "Weapon id is required."), ReadOnly]
        [Tooltip("Stable runtime ID. Use Regenerate ID only when intentionally migrating references.")]
        public string weaponId = "juggling_ball";

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Core"), LabelWidth(140), PropertyOrder(1)]
        [ValidateInput(nameof(HasDisplayName), "Display name is required.")]
        public string displayName = "Juggling Ball";

        [HideInInspector]
        public ContentTagSet tags = ContentTagSet.With(ContentTag.Projectile, ContentTag.Physical);

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Classification"), InfoBox("Computed from mechanics and effects. Change source fields instead.", InfoMessageType.Info), ShowInInspector, ReadOnly, LabelText("Computed Tags"), PropertyOrder(10)]
        private string ComputedTags => ComputedContentTagRules.Format(Tags);

        [HideInInspector]
        public bool isActive = true;

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Level-Up Card"), LabelWidth(140)]
        [LabelText("Can Appear As Level-Up Weapon")]
        public bool canAppearAsLevelUpWeapon = true;

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Level-Up Card"), LabelWidth(140)]
        [MultiLineProperty(2), ShowIf(nameof(canAppearAsLevelUpWeapon))]
        [LabelText("Short Description")]
        public string addWeaponShortDescription = "Add this weapon to the run loadout.";

        [TabGroup(Tabs, "Timing"), BoxGroup(Tabs + "/Timing/Cooldown"), LabelWidth(140), LabelText("Base Interval"), SuffixLabel("sec"), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.CombatColor")]
        [Min(0.01f)] public float baseFireIntervalSeconds = 0.36f;

        [TabGroup(Tabs, "Timing"), BoxGroup(Tabs + "/Timing/Cooldown"), LabelWidth(140), LabelText("Minimum Interval"), SuffixLabel("sec")]
        [ValidateInput(nameof(IsMinimumFireIntervalValid), "Minimum fire interval cannot exceed base fire interval.")]
        [Min(0.01f)] public float minimumFireIntervalSeconds = WeaponCooldownRules.MinimumFireInterval;

        [TabGroup(Tabs, "Projectile"), BoxGroup(Tabs + "/Projectile/Stats"), LabelWidth(140), LabelText("Base Damage")]
        [Min(1)] public int projectileDamage = 20;

        [TabGroup(Tabs, "Projectile"), BoxGroup(Tabs + "/Projectile/Stats"), LabelWidth(140), SuffixLabel("u/s")]
        [ShowIf(nameof(IsProjectileTravelActive))]
        [Min(0f)] public float projectileSpeed = 24f;

        [TabGroup(Tabs, "Projectile"), BoxGroup(Tabs + "/Projectile/Stats"), LabelWidth(140), SuffixLabel("sec")]
        [ShowIf(nameof(IsProjectileTravelActive))]
        [Min(0.01f)] public float projectileLifetimeSeconds = 3.25f;

        [TabGroup(Tabs, "Projectile"), BoxGroup(Tabs + "/Projectile/Hit Query"), LabelWidth(140), SuffixLabel("u")]
        [ShowIf(nameof(IsProjectileTravelActive))]
        [Min(0.01f)] public float projectileHitRadius = 0.85f;

        [TabGroup(Tabs, "Projectile"), BoxGroup(Tabs + "/Projectile/Hit Query"), LabelWidth(140), LabelText("Enemy Hurtbox Mask")]
        public LayerMask projectileHitMask;

        [TabGroup(Tabs, "Projectile"), BoxGroup(Tabs + "/Projectile/Emission"), LabelWidth(140)]
        [ShowIf(nameof(IsProjectileEmissionActive))]
        [Min(1)] public int baseProjectileCount = 1;

        [TabGroup(Tabs, "Projectile"), BoxGroup(Tabs + "/Projectile/Emission"), LabelWidth(140)]
        [ShowIf(nameof(IsProjectileEmissionActive))]
        [Min(1)] public int maxProjectileCount = 1;

        [TabGroup(Tabs, "Projectile"), BoxGroup(Tabs + "/Projectile/Emission"), LabelWidth(140), SuffixLabel("deg"), LabelText("Pattern Spread")]
        [ShowIf(nameof(IsProjectileEmissionActive))]
        [Min(0f)] public float baseSpreadAngleDegrees;

        [HideInInspector]
        public bool accuracyAffectsSpread = true;

        [TabGroup(Tabs, "Projectile"), BoxGroup(Tabs + "/Projectile/Aim Error"), LabelWidth(140), SuffixLabel("deg")]
        [ShowIf(nameof(IsProjectileAimErrorActive))]
        [Min(0f)] public float baseAimErrorDegrees;

        [TabGroup(Tabs, "Projectile"), BoxGroup(Tabs + "/Projectile/Aim Error"), LabelWidth(140)]
        [ShowIf(nameof(IsProjectileAimErrorActive))]
        public bool accuracyAffectsAimError = true;

        [TabGroup(Tabs, "Projectile"), BoxGroup(Tabs + "/Projectile/Trajectory"), LabelWidth(140)]
        [ShowIf(nameof(IsProjectileTrajectoryActive))]
        public ProjectileTrajectoryMode projectileTrajectoryMode = ProjectileTrajectoryMode.Direct;

        [TabGroup(Tabs, "Projectile"), BoxGroup(Tabs + "/Projectile/Trajectory"), LabelWidth(140), SuffixLabel("u")]
        [ShowIf(nameof(IsArcTrajectorySettingsActive))]
        [Min(0f)] public float projectileArcHeight;

        [TabGroup(Tabs, "Projectile"), BoxGroup(Tabs + "/Projectile/Bounce"), LabelWidth(140)]
        [ShowIf(nameof(IsProjectileTravelActive))]
        public bool bounceEnabled;

        [TabGroup(Tabs, "Projectile"), BoxGroup(Tabs + "/Projectile/Bounce"), LabelWidth(140)]
        [ShowIf(nameof(IsBounceSettingsActive))]
        [Min(0)] public int baseBounceCount;

        [TabGroup(Tabs, "Projectile"), BoxGroup(Tabs + "/Projectile/Bounce"), LabelWidth(140)]
        [ShowIf(nameof(IsBounceSettingsActive))]
        [Min(0)] public int maxBounceCount;

        [TabGroup(Tabs, "Projectile"), BoxGroup(Tabs + "/Projectile/Bounce"), LabelWidth(140), SuffixLabel("u")]
        [ShowIf(nameof(IsBounceSettingsActive))]
        [Min(0.01f)] public float enemyBounceSearchRadius = 12f;

        [TabGroup(Tabs, "Projectile"), BoxGroup(Tabs + "/Projectile/Bounce"), LabelWidth(140)]
        [ShowIf(nameof(IsBounceSettingsActive))]
        public bool bounceOffWorldBlockers;

        [TabGroup(Tabs, "Projectile"), BoxGroup(Tabs + "/Projectile/Explosion"), LabelWidth(140)]
        [ShowIf(nameof(IsProjectileTravelActive))]
        public bool explosiveEnabled;

        [TabGroup(Tabs, "Projectile"), BoxGroup(Tabs + "/Projectile/Explosion"), LabelWidth(140), SuffixLabel("u")]
        [ShowIf(nameof(IsExplosionSettingsActive))]
        [Min(0f)] public float baseSplashRadius;

        [TabGroup(Tabs, "Projectile"), BoxGroup(Tabs + "/Projectile/Explosion"), LabelWidth(140), SuffixLabel("u")]
        [ShowIf(nameof(IsExplosionSettingsActive))]
        [Min(0f)] public float maxSplashRadius;

        [TabGroup(Tabs, "Projectile"), BoxGroup(Tabs + "/Projectile/Explosion"), LabelWidth(140)]
        [ShowIf(nameof(IsExplosionSettingsActive))]
        [Range(0f, 1f)] public float secondarySplashDamageMultiplier = 0.5f;

        [TabGroup(Tabs, "Projectile"), BoxGroup(Tabs + "/Projectile/Explosion"), LabelWidth(140)]
        [ShowIf(nameof(IsExplosionSettingsActive))]
        public bool explodeOnWorldImpact;

        [TabGroup(Tabs, "Projectile"), BoxGroup(Tabs + "/Projectile/Explosion"), LabelWidth(140)]
        [ShowIf(nameof(IsExplosionSettingsActive))]
        public bool explosionVfxEnabled;

        [TabGroup(Tabs, "Projectile"), BoxGroup(Tabs + "/Projectile/Chain"), LabelWidth(140)]
        [ShowIf(nameof(IsProjectileTravelActive))]
        public bool chainEnabled;

        [TabGroup(Tabs, "Projectile"), BoxGroup(Tabs + "/Projectile/Chain"), LabelWidth(140)]
        [ShowIf(nameof(IsChainSettingsActive))]
        [Min(0)] public int baseChainCount;

        [TabGroup(Tabs, "Projectile"), BoxGroup(Tabs + "/Projectile/Chain"), LabelWidth(140)]
        [ShowIf(nameof(IsChainSettingsActive))]
        [Min(0)] public int maxChainCount;

        [TabGroup(Tabs, "Projectile"), BoxGroup(Tabs + "/Projectile/Chain"), LabelWidth(140), SuffixLabel("u")]
        [ShowIf(nameof(IsChainSettingsActive))]
        [Min(0.01f)] public float chainSearchRadius = 10f;

        [TabGroup(Tabs, "Projectile"), BoxGroup(Tabs + "/Projectile/Chain"), LabelWidth(140)]
        [ShowIf(nameof(IsChainSettingsActive))]
        [Range(0f, 1f)] public float chainDamageMultiplier = 0.65f;

        [TabGroup(Tabs, "Projectile"), BoxGroup(Tabs + "/Projectile/Chain"), LabelWidth(140)]
        [ShowIf(nameof(IsChainSettingsActive))]
        public bool chainRequiresLineOfSight = true;

        [TabGroup(Tabs, "Orbit"), BoxGroup(Tabs + "/Orbit/Runtime"), LabelWidth(140), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.CombatColor")]
        public bool orbitEnabled;

        [TabGroup(Tabs, "Orbit"), BoxGroup(Tabs + "/Orbit/Count"), LabelWidth(140)]
        [ShowIf(nameof(IsOrbitSettingsActive))]
        [Min(1)] public int baseOrbitCount = 1;

        [TabGroup(Tabs, "Orbit"), BoxGroup(Tabs + "/Orbit/Count"), LabelWidth(140)]
        [ShowIf(nameof(IsOrbitSettingsActive))]
        [Min(1)] public int maxOrbitCount = 1;

        [TabGroup(Tabs, "Orbit"), BoxGroup(Tabs + "/Orbit/Shape"), LabelWidth(140), SuffixLabel("u")]
        [ShowIf(nameof(IsOrbitSettingsActive))]
        [Min(0.01f)] public float orbitRadius = 2.2f;

        [TabGroup(Tabs, "Orbit"), BoxGroup(Tabs + "/Orbit/Shape"), LabelWidth(140), SuffixLabel("u")]
        [ShowIf(nameof(IsOrbitSettingsActive))]
        [Min(0.01f)] public float maxOrbitRadius = 3.6f;

        [TabGroup(Tabs, "Orbit"), BoxGroup(Tabs + "/Orbit/Shape"), LabelWidth(140), SuffixLabel("u")]
        [ShowIf(nameof(IsOrbitSettingsActive))]
        [Min(0.01f)] public float orbitHitRadius = 0.55f;

        [TabGroup(Tabs, "Orbit"), BoxGroup(Tabs + "/Orbit/Shape"), LabelWidth(140), SuffixLabel("u")]
        [ShowIf(nameof(IsOrbitSettingsActive))]
        [Min(0.01f)] public float maxOrbitHitRadius = 1.1f;

        [TabGroup(Tabs, "Orbit"), BoxGroup(Tabs + "/Orbit/Motion"), LabelWidth(140), SuffixLabel("deg/sec")]
        [ShowIf(nameof(IsOrbitSettingsActive))]
        [Min(0f)] public float orbitDegreesPerSecond = 150f;

        [TabGroup(Tabs, "Orbit"), BoxGroup(Tabs + "/Orbit/Motion"), LabelWidth(140), SuffixLabel("u")]
        [ShowIf(nameof(IsOrbitSettingsActive))]
        [Min(0f)] public float orbitHeightOffset = 0.9f;

        [TabGroup(Tabs, "Orbit"), BoxGroup(Tabs + "/Orbit/Area Heat"), LabelWidth(140)]
        [ShowIf(nameof(IsOrbitSettingsActive))]
        public bool orbitAreaDamageEnabled;

        [TabGroup(Tabs, "Orbit"), BoxGroup(Tabs + "/Orbit/Area Heat"), LabelWidth(140)]
        [ShowIf(nameof(IsOrbitAreaHeatSettingsActive))]
        [Range(0f, 1f)] public float orbitAreaDamageMultiplier = 0.55f;

        [TabGroup(Tabs, "Orbit"), BoxGroup(Tabs + "/Orbit/Area Heat"), LabelWidth(140), SuffixLabel("sec")]
        [ShowIf(nameof(IsOrbitAreaHeatSettingsActive))]
        [Min(0.01f)] public float orbitAreaHitIntervalSeconds = 0.45f;

        [TabGroup(Tabs, "Targeting"), BoxGroup(Tabs + "/Targeting/Range"), LabelWidth(140), SuffixLabel("u"), LabelText("Projectile Target Range")]
        [ShowIf(nameof(IsProjectileTravelActive))]
        [Min(0.01f)] public float weaponRange = 30f;

        [TabGroup(Tabs, "Targeting"), BoxGroup(Tabs + "/Targeting/Line of Sight"), LabelWidth(140)]
        public bool requireLineOfSight = true;

        [TabGroup(Tabs, "Targeting"), BoxGroup(Tabs + "/Targeting/Line of Sight"), LabelWidth(140)]
        public LayerMask lineOfSightMask;

        [TabGroup(Tabs, "Targeting"), BoxGroup(Tabs + "/Targeting/Projectile Collision"), LabelWidth(140)]
        [ShowIf(nameof(IsProjectileCollisionActive))]
        public LayerMask projectileBlockMask;

        [TabGroup(Tabs, "Targeting"), BoxGroup(Tabs + "/Targeting/Projectile Collision"), LabelWidth(140), SuffixLabel("u")]
        [ShowIf(nameof(IsProjectileCollisionActive))]
        [Min(0.001f)] public float projectileBlockRadius = 0.16f;

        [TabGroup(Tabs, "Spawn Pose"), BoxGroup(Tabs + "/Spawn Pose/Muzzle"), LabelWidth(140), SuffixLabel("u")]
        [ShowIf(nameof(IsProjectileSpawnPoseActive))]
        [Min(0f)] public float projectileSpawnForwardOffset = 0.9f;

        [TabGroup(Tabs, "Spawn Pose"), BoxGroup(Tabs + "/Spawn Pose/Muzzle"), LabelWidth(140), SuffixLabel("u")]
        [ShowIf(nameof(IsProjectileSpawnPoseActive))]
        [Min(0.01f)] public float projectileSpawnHeight = 0.8f;

        [TabGroup(Tabs, "Visuals"), BoxGroup(Tabs + "/Visuals/UI Icon"), LabelWidth(140), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.VisualColor"), AssetSelector, PreviewField(72)]
        [Tooltip("Square transparent icon used by level-up cards and HUD weapon slots. Projectile visual settings below still control in-world shots.")]
        public Sprite iconSprite;

        [TabGroup(Tabs, "Visuals"), BoxGroup(Tabs + "/Visuals/Shape"), LabelWidth(140), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.VisualColor")]
        [ShowIf(nameof(IsProjectileTravelActive))]
        public ProjectileVisualShape projectileVisualShape = ProjectileVisualShape.Sphere;

        [TabGroup(Tabs, "Visuals"), BoxGroup(Tabs + "/Visuals/Shape"), LabelWidth(140)]
        [ShowIf(nameof(IsProjectileTravelActive))]
        [Min(0.05f)] public float projectileVisualScale = 1f;

        [TabGroup(Tabs, "Visuals"), BoxGroup(Tabs + "/Visuals/Color"), LabelWidth(140), LabelText("Primary Color")]
        public Color projectilePrimaryColor = new(1f, 0.88f, 0.18f, 1f);

        [TabGroup(Tabs, "Visuals"), BoxGroup(Tabs + "/Visuals/Color"), LabelWidth(140), LabelText("Emission Color")]
        public Color projectileEmissionColor = new(1f, 0.87f, 0.22f, 1f);

        [TabGroup(Tabs, "Visuals"), BoxGroup(Tabs + "/Visuals/Color"), LabelWidth(140), LabelText("Emission Strength")]
        [Min(0f)] public float projectileEmissionStrength = 0.65f;

        [TabGroup(Tabs, "Visuals"), BoxGroup(Tabs + "/Visuals/Trail"), LabelWidth(140)]
        [ShowIf(nameof(IsProjectileTrailActive))]
        public bool projectileTrailEnabled = true;

        [TabGroup(Tabs, "Visuals"), BoxGroup(Tabs + "/Visuals/Trail"), LabelWidth(140)]
        [ShowIf(nameof(IsProjectileTrailDetailsActive))]
        public Color projectileTrailColor = new(1f, 0.88f, 0.18f, 0.75f);

        [TabGroup(Tabs, "Visuals"), BoxGroup(Tabs + "/Visuals/Trail"), LabelWidth(140), SuffixLabel("u")]
        [ShowIf(nameof(IsProjectileTrailDetailsActive))]
        [Min(0.001f)] public float projectileTrailWidth = 0.08f;

        [TabGroup(Tabs, "Visuals"), BoxGroup(Tabs + "/Visuals/Trail"), LabelWidth(140), SuffixLabel("sec")]
        [ShowIf(nameof(IsProjectileTrailDetailsActive))]
        [Min(0.01f)] public float projectileTrailLifetime = 0.18f;

        [TabGroup(Tabs, "Upgrade Track"), BoxGroup(Tabs + "/Upgrade Track/Owned Weapon Cards"), LabelWidth(140), AssetSelector]
        public List<UpgradeDefinition> upgradeTrack = new();

        [TabGroup(Tabs, "Upgrade Track"), BoxGroup(Tabs + "/Upgrade Track/Supported Stats"), LabelWidth(140)]
        [ValueDropdown(nameof(WeaponLocalUpgradeStats))]
        public List<StatId> supportedUpgradeStats = new();

        [SerializeField, HideInInspector] private int projectileCollisionDefaultsVersion;
        [SerializeField, HideInInspector] private int availabilityDefaultsVersion;
        [SerializeField, HideInInspector] private int projectileVisualDefaultsVersion;
        [SerializeField, HideInInspector] private int projectileEmissionDefaultsVersion;
        [SerializeField, HideInInspector] private int projectileAimDefaultsVersion;
        [SerializeField, HideInInspector] private int projectileTrajectoryDefaultsVersion;
        [SerializeField, HideInInspector] private int acquisitionDefaultsVersion;
        [SerializeField, HideInInspector] private int bounceDefaultsVersion;
        [SerializeField, HideInInspector] private int explosionDefaultsVersion;
        [SerializeField, HideInInspector] private int chainDefaultsVersion;
        [SerializeField, HideInInspector] private int orbitDefaultsVersion;

        [TabGroup(Tabs, "Diagnostics"), ShowInInspector, ReadOnly, LabelText("Cooldown Interval"), PropertyOrder(100)]
        private string CooldownInterval => $"{WeaponCooldownRules.FireInterval(this):0.###} sec";

        public string Id => weaponId;
        public string DisplayName => displayName;
        public ContentTagSet Tags => ComputedContentTagRules.For(this);
        public bool IsActive => isActive;
        public bool CanAppearAsLevelUpWeapon => canAppearAsLevelUpWeapon;
        public string AddWeaponShortDescription => string.IsNullOrWhiteSpace(addWeaponShortDescription)
            ? $"Add {DisplayName} to the run loadout."
            : addWeaponShortDescription;
        public IReadOnlyList<UpgradeDefinition> UpgradeTrack => upgradeTrack;
        public IReadOnlyList<StatId> SupportedUpgradeStats => supportedUpgradeStats;

    }
}
