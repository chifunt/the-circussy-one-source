using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using TheCircussyOne.Content;
using TheCircussyOne.Stats;

internal static class TheCircussyOneWeaponStarterContent
{
    private readonly struct WeaponTrackUpgradeSpec
    {
        public WeaponTrackUpgradeSpec(
            string path,
            string weaponId,
            string idSuffix,
            string nameSuffix,
            string description,
            Color iconColor,
            UpgradeStatModifierDefinition modifier)
        {
            Path = path;
            WeaponId = weaponId;
            IdSuffix = idSuffix;
            NameSuffix = nameSuffix;
            Description = description;
            IconColor = iconColor;
            Modifier = modifier;
        }

        public string Path { get; }
        public string WeaponId { get; }
        public string IdSuffix { get; }
        public string NameSuffix { get; }
        public string Description { get; }
        public Color IconColor { get; }
        public UpgradeStatModifierDefinition Modifier { get; }
    }

    private static readonly WeaponTrackUpgradeSpec[] WeaponTrackUpgrades =
    {
        new(
            TheCircussyOneAssetPaths.CannonTuningUpgradePath,
            FirstPartyWeaponDefaults.CannonId,
            "blast_radius",
            "Blast Radius",
            "Pack the cannon for a finale that fills the ring.",
            new Color(1f, 0.46f, 0.24f, 1f),
            new UpgradeStatModifierDefinition(StatId.WeaponSplashRadiusMultiplier, StatModifierBucket.AdditivePercent, 0.1f)),
        new(
            TheCircussyOneAssetPaths.CannonPayloadUpgradePath,
            FirstPartyWeaponDefaults.CannonId,
            "payload",
            "Payload",
            "Load a heavier roar into the barrel.",
            new Color(1f, 0.36f, 0.16f, 1f),
            new UpgradeStatModifierDefinition(StatId.WeaponFlatDamage, StatModifierBucket.Flat, 10f)),
        new(
            TheCircussyOneAssetPaths.CannonBiggerShotUpgradePath,
            FirstPartyWeaponDefaults.CannonId,
            "bigger_shot",
            "Bigger Shot",
            "Make every cannonball read from the back row.",
            new Color(1f, 0.58f, 0.26f, 1f),
            new UpgradeStatModifierDefinition(StatId.WeaponProjectileSizeMultiplier, StatModifierBucket.AdditivePercent, 0.08f)),
        new(
            TheCircussyOneAssetPaths.CannonQuickFuseUpgradePath,
            FirstPartyWeaponDefaults.CannonId,
            "quick_fuse",
            "Quick Fuse",
            "Trim the fuse until the finale answers faster.",
            new Color(1f, 0.7f, 0.28f, 1f),
            new UpgradeStatModifierDefinition(StatId.WeaponAttackSpeed, StatModifierBucket.AdditivePercent, 0.05f)),
        new(
            TheCircussyOneAssetPaths.CannonFastShotUpgradePath,
            FirstPartyWeaponDefaults.CannonId,
            "fast_shot",
            "Fast Shot",
            "Send the cannonball screaming across the tent.",
            new Color(1f, 0.5f, 0.18f, 1f),
            new UpgradeStatModifierDefinition(StatId.WeaponProjectileSpeedMultiplier, StatModifierBucket.AdditivePercent, 0.08f)),
        new(
            TheCircussyOneAssetPaths.KnifeFanTuningUpgradePath,
            FirstPartyWeaponDefaults.KnifeFanId,
            "spread",
            "Spread",
            "Send a wider fan of steel glittering through the air.",
            new Color(0.95f, 0.95f, 0.86f, 1f),
            new UpgradeStatModifierDefinition(StatId.WeaponProjectileCount, StatModifierBucket.Flat, 1f)),
        new(
            TheCircussyOneAssetPaths.KnifeFanFocusUpgradePath,
            FirstPartyWeaponDefaults.KnifeFanId,
            "focus",
            "Focus",
            "Let every blade leave the hand with show-night precision.",
            new Color(0.86f, 0.96f, 1f, 1f),
            new UpgradeStatModifierDefinition(StatId.WeaponAccuracyMultiplier, StatModifierBucket.AdditivePercent, 0.08f)),
        new(
            TheCircussyOneAssetPaths.KnifeFanHonedBladesUpgradePath,
            FirstPartyWeaponDefaults.KnifeFanId,
            "honed_blades",
            "Honed Blades",
            "Put a sharper glint on every throw.",
            new Color(0.98f, 0.98f, 0.9f, 1f),
            new UpgradeStatModifierDefinition(StatId.WeaponFlatDamage, StatModifierBucket.Flat, 10f)),
        new(
            TheCircussyOneAssetPaths.KnifeFanQuickTossUpgradePath,
            FirstPartyWeaponDefaults.KnifeFanId,
            "quick_toss",
            "Quick Toss",
            "Flick the fan open before the crowd can blink.",
            new Color(0.85f, 0.95f, 1f, 1f),
            new UpgradeStatModifierDefinition(StatId.WeaponAttackSpeed, StatModifierBucket.AdditivePercent, 0.05f)),
        new(
            TheCircussyOneAssetPaths.KnifeFanFastBladesUpgradePath,
            FirstPartyWeaponDefaults.KnifeFanId,
            "fast_blades",
            "Fast Blades",
            "Let the knives whistle through their cue.",
            new Color(0.9f, 0.98f, 1f, 1f),
            new UpgradeStatModifierDefinition(StatId.WeaponProjectileSpeedMultiplier, StatModifierBucket.AdditivePercent, 0.08f)),
        new(
            TheCircussyOneAssetPaths.KnifeFanLingeringCutUpgradePath,
            FirstPartyWeaponDefaults.KnifeFanId,
            "lingering_cut",
            "Lingering Cut",
            "Keep the silver line hanging in the lights.",
            new Color(0.92f, 0.92f, 1f, 1f),
            new UpgradeStatModifierDefinition(StatId.WeaponProjectileLifetimeMultiplier, StatModifierBucket.AdditivePercent, 0.08f)),
        new(
            TheCircussyOneAssetPaths.SpotlightBoltTuningUpgradePath,
            FirstPartyWeaponDefaults.SpotlightBoltId,
            "relay",
            "Relay",
            "Teach the spotlight to find another mark before the applause fades.",
            new Color(0.52f, 0.88f, 1f, 1f),
            new UpgradeStatModifierDefinition(StatId.WeaponChain, StatModifierBucket.Flat, 1f)),
        new(
            TheCircussyOneAssetPaths.SpotlightBoltBrightChargeUpgradePath,
            FirstPartyWeaponDefaults.SpotlightBoltId,
            "bright_charge",
            "Bright Charge",
            "Turn the beam bright enough to sting.",
            new Color(0.38f, 0.9f, 1f, 1f),
            new UpgradeStatModifierDefinition(StatId.WeaponFlatDamage, StatModifierBucket.Flat, 10f)),
        new(
            TheCircussyOneAssetPaths.SpotlightBoltQuickCueUpgradePath,
            FirstPartyWeaponDefaults.SpotlightBoltId,
            "quick_cue",
            "Quick Cue",
            "Call the spotlight before the shadow settles.",
            new Color(0.46f, 0.82f, 1f, 1f),
            new UpgradeStatModifierDefinition(StatId.WeaponAttackSpeed, StatModifierBucket.AdditivePercent, 0.05f)),
        new(
            TheCircussyOneAssetPaths.SpotlightBoltFastBeamUpgradePath,
            FirstPartyWeaponDefaults.SpotlightBoltId,
            "fast_beam",
            "Fast Beam",
            "Snap the light from mark to mark.",
            new Color(0.42f, 0.95f, 1f, 1f),
            new UpgradeStatModifierDefinition(StatId.WeaponProjectileSpeedMultiplier, StatModifierBucket.AdditivePercent, 0.08f)),
        new(
            TheCircussyOneAssetPaths.SpotlightBoltWideBeamUpgradePath,
            FirstPartyWeaponDefaults.SpotlightBoltId,
            "wide_beam",
            "Wide Beam",
            "Open the beam until the whole ring catches it.",
            new Color(0.56f, 0.9f, 1f, 1f),
            new UpgradeStatModifierDefinition(StatId.WeaponProjectileSizeMultiplier, StatModifierBucket.AdditivePercent, 0.08f)),
        new(
            TheCircussyOneAssetPaths.SpotlightBoltLingeringGlowUpgradePath,
            FirstPartyWeaponDefaults.SpotlightBoltId,
            "lingering_glow",
            "Lingering Glow",
            "Leave the light hanging just a heartbeat longer.",
            new Color(0.62f, 0.82f, 1f, 1f),
            new UpgradeStatModifierDefinition(StatId.WeaponProjectileLifetimeMultiplier, StatModifierBucket.AdditivePercent, 0.08f)),
        new(
            TheCircussyOneAssetPaths.FireHoopTuningUpgradePath,
            FirstPartyWeaponDefaults.FireHoopId,
            "fuel",
            "Fuel",
            "Feed the hoop hotter flame and make the circle bite.",
            new Color(1f, 0.32f, 0.18f, 1f),
            new UpgradeStatModifierDefinition(StatId.WeaponFlatDamage, StatModifierBucket.Flat, 10f)),
        new(
            TheCircussyOneAssetPaths.FireHoopWiderUpgradePath,
            FirstPartyWeaponDefaults.FireHoopId,
            "wider",
            "Wider Hoop",
            "Give the hoop a broader sweep around the stage.",
            new Color(1f, 0.42f, 0.14f, 1f),
            new UpgradeStatModifierDefinition(StatId.WeaponRangeMultiplier, StatModifierBucket.AdditivePercent, 0.1f)),
        new(
            TheCircussyOneAssetPaths.FireHoopExtraUpgradePath,
            FirstPartyWeaponDefaults.FireHoopId,
            "extra",
            "Extra Hoop",
            "Spin another burning circle into the act.",
            new Color(1f, 0.58f, 0.18f, 1f),
            new UpgradeStatModifierDefinition(StatId.WeaponProjectileCount, StatModifierBucket.Flat, 1f)),
        new(
            TheCircussyOneAssetPaths.FireHoopQuickSpinUpgradePath,
            FirstPartyWeaponDefaults.FireHoopId,
            "quick_spin",
            "Quick Spin",
            "Whip the hoop around before the flame can rest.",
            new Color(1f, 0.48f, 0.12f, 1f),
            new UpgradeStatModifierDefinition(StatId.WeaponAttackSpeed, StatModifierBucket.AdditivePercent, 0.05f)),
        new(
            TheCircussyOneAssetPaths.FireHoopBiggerFlameUpgradePath,
            FirstPartyWeaponDefaults.FireHoopId,
            "bigger_flame",
            "Bigger Flame",
            "Let the fire lick wider around the ring.",
            new Color(1f, 0.28f, 0.06f, 1f),
            new UpgradeStatModifierDefinition(StatId.WeaponProjectileSizeMultiplier, StatModifierBucket.AdditivePercent, 0.08f)),
        new(
            TheCircussyOneAssetPaths.JugglingBallTuningUpgradePath,
            FirstPartyWeaponDefaults.JugglingBallId,
            "bounce",
            "Bounce",
            "Put more spring in the ball and keep the trick alive.",
            new Color(0.54f, 0.9f, 1f, 1f),
            new UpgradeStatModifierDefinition(StatId.WeaponBounce, StatModifierBucket.Flat, 1f)),
        new(
            TheCircussyOneAssetPaths.JugglingBallTimingUpgradePath,
            FirstPartyWeaponDefaults.JugglingBallId,
            "timing",
            "Timing",
            "Catch the rhythm cleanly and send each ball where it belongs.",
            new Color(0.7f, 0.94f, 1f, 1f),
            new UpgradeStatModifierDefinition(StatId.WeaponAccuracyMultiplier, StatModifierBucket.AdditivePercent, 0.08f)),
        new(
            TheCircussyOneAssetPaths.JugglingBallWeightedBallUpgradePath,
            FirstPartyWeaponDefaults.JugglingBallId,
            "weighted_ball",
            "Weighted Ball",
            "Put a little more thump in the catch.",
            new Color(1f, 0.58f, 0.42f, 1f),
            new UpgradeStatModifierDefinition(StatId.WeaponFlatDamage, StatModifierBucket.Flat, 10f)),
        new(
            TheCircussyOneAssetPaths.JugglingBallQuickTossUpgradePath,
            FirstPartyWeaponDefaults.JugglingBallId,
            "quick_toss",
            "Quick Toss",
            "Keep the juggling pattern moving at show tempo.",
            new Color(1f, 0.74f, 0.28f, 1f),
            new UpgradeStatModifierDefinition(StatId.WeaponAttackSpeed, StatModifierBucket.AdditivePercent, 0.05f)),
        new(
            TheCircussyOneAssetPaths.JugglingBallFastRollUpgradePath,
            FirstPartyWeaponDefaults.JugglingBallId,
            "fast_roll",
            "Fast Roll",
            "Let the ball skim the sawdust with extra pace.",
            new Color(1f, 0.86f, 0.32f, 1f),
            new UpgradeStatModifierDefinition(StatId.WeaponProjectileSpeedMultiplier, StatModifierBucket.AdditivePercent, 0.08f)),
        new(
            TheCircussyOneAssetPaths.JugglingBallBiggerBallUpgradePath,
            FirstPartyWeaponDefaults.JugglingBallId,
            "bigger_ball",
            "Bigger Ball",
            "Make the prop big enough to own the spotlight.",
            new Color(1f, 0.46f, 0.34f, 1f),
            new UpgradeStatModifierDefinition(StatId.WeaponProjectileSizeMultiplier, StatModifierBucket.AdditivePercent, 0.08f)),
        new(
            TheCircussyOneAssetPaths.JugglingBallLongJuggleUpgradePath,
            FirstPartyWeaponDefaults.JugglingBallId,
            "long_juggle",
            "Long Juggle",
            "Keep the ball in the act for one more beat.",
            new Color(1f, 0.9f, 0.42f, 1f),
            new UpgradeStatModifierDefinition(StatId.WeaponProjectileLifetimeMultiplier, StatModifierBucket.AdditivePercent, 0.08f))
    };

    public static WeaponDefinition GetOrCreateCannonWeapon()
    {
        return GetOrCreateWeapon(FirstPartyWeaponDefaults.GetOrDefault(FirstPartyWeaponDefaults.CannonId));
    }

    public static WeaponDefinition GetOrCreateKnifeFanWeapon()
    {
        return GetOrCreateWeapon(FirstPartyWeaponDefaults.GetOrDefault(FirstPartyWeaponDefaults.KnifeFanId));
    }

    public static WeaponDefinition GetOrCreateSpotlightBoltWeapon()
    {
        return GetOrCreateWeapon(FirstPartyWeaponDefaults.GetOrDefault(FirstPartyWeaponDefaults.SpotlightBoltId));
    }

    public static WeaponDefinition GetOrCreateFireHoopWeapon()
    {
        return GetOrCreateWeapon(FirstPartyWeaponDefaults.GetOrDefault(FirstPartyWeaponDefaults.FireHoopId));
    }

    public static WeaponDefinition GetOrCreateJugglingBallWeapon()
    {
        return GetOrCreateWeapon(FirstPartyWeaponDefaults.GetOrDefault(FirstPartyWeaponDefaults.JugglingBallId));
    }

    public static WeaponCatalog GetOrCreateWeaponCatalog()
    {
        WeaponDefinition defaultWeapon = GetOrCreateJugglingBallWeapon();
        WeaponDefinition[] availableWeapons =
        {
            GetOrCreateCannonWeapon(),
            GetOrCreateKnifeFanWeapon(),
            GetOrCreateSpotlightBoltWeapon(),
            GetOrCreateFireHoopWeapon(),
            defaultWeapon
        };
        WeaponCatalog catalog = AssetDatabase.LoadAssetAtPath<WeaponCatalog>(TheCircussyOneAssetPaths.WeaponCatalogPath);
        if (catalog != null)
        {
            MarkDirtyIf(catalog, catalog.EnsureWorkflowDefaults(defaultWeapon, availableWeapons));
            return catalog;
        }

        catalog = CreateAsset<WeaponCatalog>(TheCircussyOneAssetPaths.WeaponCatalogPath);
        catalog.EnsureWorkflowDefaults(defaultWeapon, availableWeapons);
        EditorUtility.SetDirty(catalog);
        return catalog;
    }

    public static UpgradeDefinition GetOrCreateCannonTuningUpgrade()
    {
        return GetOrCreateWeaponTrackUpgrade(FirstPartyWeaponDefaults.CannonId);
    }

    public static UpgradeDefinition GetOrCreateKnifeFanTuningUpgrade()
    {
        return GetOrCreateWeaponTrackUpgrade(FirstPartyWeaponDefaults.KnifeFanId);
    }

    public static UpgradeDefinition GetOrCreateSpotlightBoltTuningUpgrade()
    {
        return GetOrCreateWeaponTrackUpgrade(FirstPartyWeaponDefaults.SpotlightBoltId);
    }

    public static UpgradeDefinition GetOrCreateFireHoopTuningUpgrade()
    {
        return GetOrCreateWeaponTrackUpgrade(FirstPartyWeaponDefaults.FireHoopId);
    }

    public static UpgradeDefinition GetOrCreateJugglingBallTuningUpgrade()
    {
        return GetOrCreateWeaponTrackUpgrade(FirstPartyWeaponDefaults.JugglingBallId);
    }

    public static UpgradeCatalog GetOrCreateUpgradeCatalog()
    {
        var starterUpgrades = new List<UpgradeDefinition>();
        for (int i = 0; i < WeaponTrackUpgrades.Length; i++)
        {
            starterUpgrades.Add(GetOrCreateWeaponTrackUpgrade(WeaponTrackUpgrades[i]));
        }

        UpgradeCatalog catalog = AssetDatabase.LoadAssetAtPath<UpgradeCatalog>(TheCircussyOneAssetPaths.UpgradeCatalogPath);
        if (catalog != null)
        {
            MarkDirtyIf(catalog, catalog.EnsureWorkflowDefaults(starterUpgrades.ToArray()));
            return catalog;
        }

        catalog = CreateAsset<UpgradeCatalog>(TheCircussyOneAssetPaths.UpgradeCatalogPath);
        catalog.levelUpRarityWeights = ContentRarityWeightTable.LevelUpDefault();
        catalog.EnsureWorkflowDefaults(starterUpgrades.ToArray());
        EditorUtility.SetDirty(catalog);
        return catalog;
    }

    private static WeaponDefinition GetOrCreateWeapon(FirstPartyWeaponSpec spec)
    {
        string path = PathForWeaponId(spec.Id);
        WeaponDefinition weapon = AssetDatabase.LoadAssetAtPath<WeaponDefinition>(path);
        if (weapon != null)
        {
            bool changed = weapon.EnsureWorkflowDefaults();
            changed |= ApplyFirstPartySupportedStats(weapon, spec);
            MarkDirtyIf(weapon, changed);
            return weapon;
        }

        weapon = CreateAsset<WeaponDefinition>(path);
        weapon.ApplyDefaults(spec);
        EditorUtility.SetDirty(weapon);
        return weapon;
    }

    private static UpgradeDefinition GetOrCreateWeaponTrackUpgrade(string weaponId)
    {
        return GetOrCreateWeaponTrackUpgrade(TrackUpgradeForWeaponId(weaponId));
    }

    private static UpgradeDefinition GetOrCreateWeaponTrackUpgrade(WeaponTrackUpgradeSpec spec)
    {
        WeaponDefinition weapon = GetOrCreateWeapon(FirstPartyWeaponDefaults.GetOrDefault(spec.WeaponId));
        UpgradeDefinition upgrade = AssetDatabase.LoadAssetAtPath<UpgradeDefinition>(spec.Path);
        if (upgrade != null)
        {
            bool changed = upgrade.EnsureWorkflowDefaults();
            changed |= ApplyWeaponTrackUpgradeDefaults(upgrade, weapon, spec);
            MarkDirtyIf(upgrade, changed);
            return upgrade;
        }

        upgrade = CreateAsset<UpgradeDefinition>(spec.Path);
        upgrade.ApplyWeaponStatUpgradeDefaults(
            weapon,
            spec.IdSuffix,
            spec.NameSuffix,
            spec.Description,
            spec.IconColor,
            spec.Modifier);
        EditorUtility.SetDirty(upgrade);
        MarkDirtyIf(weapon, weapon.EnsureUpgradeTrackDefaults(upgrade));
        return upgrade;
    }

    internal static bool ApplyFirstPartySupportedStatsForTests(WeaponDefinition weapon, FirstPartyWeaponSpec spec)
    {
        return ApplyFirstPartySupportedStats(weapon, spec);
    }

    internal static bool ApplyWeaponTrackUpgradeDefaultsForTests(UpgradeDefinition upgrade, WeaponDefinition weapon, string idSuffix, string nameSuffix, string description, Color iconColor, UpgradeStatModifierDefinition modifier)
    {
        var spec = new WeaponTrackUpgradeSpec(string.Empty, weapon != null ? weapon.Id : string.Empty, idSuffix, nameSuffix, description, iconColor, modifier);
        return ApplyWeaponTrackUpgradeDefaults(upgrade, weapon, spec);
    }

    private static bool ApplyFirstPartySupportedStats(WeaponDefinition weapon, FirstPartyWeaponSpec spec)
    {
        if (weapon == null || weapon.Id != spec.Id)
        {
            return false;
        }

        weapon.supportedUpgradeStats ??= new List<StatId>();
        if (StatsEqual(weapon.supportedUpgradeStats, spec.SupportedUpgradeStats))
        {
            return false;
        }

        weapon.supportedUpgradeStats.Clear();
        for (int i = 0; i < spec.SupportedUpgradeStats.Count; i++)
        {
            weapon.supportedUpgradeStats.Add(spec.SupportedUpgradeStats[i]);
        }

        return true;
    }

    private static bool ApplyWeaponTrackUpgradeDefaults(UpgradeDefinition upgrade, WeaponDefinition weapon, WeaponTrackUpgradeSpec spec)
    {
        if (upgrade == null || weapon == null)
        {
            return false;
        }

        bool changed = false;
        string expectedId = $"{weapon.Id}_{spec.IdSuffix}";
        string expectedDisplayName = $"{weapon.DisplayName} {spec.NameSuffix}";

        if (upgrade.weaponDefinition != weapon)
        {
            upgrade.weaponDefinition = weapon;
            changed = true;
        }

        if (upgrade.weaponId != weapon.Id)
        {
            upgrade.weaponId = weapon.Id;
            changed = true;
        }

        if (upgrade.upgradeId != expectedId)
        {
            upgrade.upgradeId = expectedId;
            changed = true;
        }

        if (upgrade.displayName != expectedDisplayName)
        {
            upgrade.displayName = expectedDisplayName;
            changed = true;
        }

        if (upgrade.shortDescription != spec.Description)
        {
            upgrade.shortDescription = spec.Description;
            changed = true;
        }

        if (upgrade.iconColor != spec.IconColor)
        {
            upgrade.iconColor = spec.IconColor;
            changed = true;
        }

        upgrade.statModifiers ??= new List<UpgradeStatModifierDefinition>();
        bool hasSingleModifier = upgrade.statModifiers.Count == 1;
        bool hasExpectedModifier = hasSingleModifier
            && System.Enum.IsDefined(typeof(StatId), upgrade.statModifiers[0].statId)
            && System.Enum.IsDefined(typeof(StatModifierBucket), upgrade.statModifiers[0].bucket)
            && upgrade.statModifiers[0].statId == spec.Modifier.statId
            && upgrade.statModifiers[0].bucket == spec.Modifier.bucket;

        if (!hasExpectedModifier)
        {
            UpgradeStatModifierDefinition repaired = hasSingleModifier
                ? WithModifierIdentity(upgrade.statModifiers[0], spec.Modifier)
                : spec.Modifier;
            upgrade.statModifiers.Clear();
            upgrade.statModifiers.Add(repaired);
            changed = true;
        }

        return changed;
    }

    private static UpgradeStatModifierDefinition WithModifierIdentity(UpgradeStatModifierDefinition source, UpgradeStatModifierDefinition identity)
    {
        source.statId = identity.statId;
        source.bucket = identity.bucket;
        if (float.IsNaN(source.value) || float.IsInfinity(source.value))
        {
            source.value = identity.value;
        }

        return source;
    }

    private static bool StatsEqual(IReadOnlyList<StatId> left, IReadOnlyList<StatId> right)
    {
        int leftCount = left != null ? left.Count : 0;
        int rightCount = right != null ? right.Count : 0;
        if (leftCount != rightCount)
        {
            return false;
        }

        for (int i = 0; i < leftCount; i++)
        {
            if (left[i] != right[i])
            {
                return false;
            }
        }

        return true;
    }

    private static WeaponTrackUpgradeSpec TrackUpgradeForWeaponId(string weaponId)
    {
        for (int i = 0; i < WeaponTrackUpgrades.Length; i++)
        {
            if (WeaponTrackUpgrades[i].WeaponId == weaponId)
            {
                return WeaponTrackUpgrades[i];
            }
        }

        return WeaponTrackUpgrades[0];
    }

    private static string PathForWeaponId(string weaponId)
    {
        return weaponId switch
        {
            FirstPartyWeaponDefaults.CannonId => TheCircussyOneAssetPaths.CannonWeaponPath,
            FirstPartyWeaponDefaults.KnifeFanId => TheCircussyOneAssetPaths.KnifeFanWeaponPath,
            FirstPartyWeaponDefaults.SpotlightBoltId => TheCircussyOneAssetPaths.SpotlightBoltWeaponPath,
            FirstPartyWeaponDefaults.FireHoopId => TheCircussyOneAssetPaths.FireHoopWeaponPath,
            FirstPartyWeaponDefaults.JugglingBallId => TheCircussyOneAssetPaths.JugglingBallWeaponPath,
            _ => TheCircussyOneAssetPaths.JugglingBallWeaponPath
        };
    }

    private static T CreateAsset<T>(string path)
        where T : ScriptableObject
    {
        EnsureParentFolder(path);
        T asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }

    private static void EnsureParentFolder(string path)
    {
        string folder = System.IO.Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(folder) && !AssetDatabase.IsValidFolder(folder))
        {
            EnsureFolder(folder);
        }
    }

    private static void EnsureFolder(string path)
    {
        if (string.IsNullOrEmpty(path) || AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        string parent = System.IO.Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
        {
            EnsureFolder(parent);
        }

        string folderName = System.IO.Path.GetFileName(path);
        AssetDatabase.CreateFolder(string.IsNullOrEmpty(parent) ? "Assets" : parent, folderName);
    }

    private static void MarkDirtyIf(Object asset, bool dirty)
    {
        if (dirty)
        {
            EditorUtility.SetDirty(asset);
        }
    }
}
