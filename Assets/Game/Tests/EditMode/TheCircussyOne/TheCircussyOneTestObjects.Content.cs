using UnityEngine;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Runtime;
using TheCircussyOne.Stats;

internal static partial class TheCircussyOneTestObjects
{
    public static WeaponDefinition CreateWeaponDefinition(GameConfig config)
    {
        var weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
        weapon.ApplyDefaults(FirstPartyWeaponDefaults.DefaultWeapon);
        return weapon;
    }

    public static WeaponDefinition CreateWeaponDefinition(GameConfig config, string id, string displayName)
    {
        var weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
        weapon.ApplyDefaults(
            id,
            displayName,
            ContentTagSet.With(ContentTag.Projectile, ContentTag.Physical),
            0.36f,
            TheCircussyOne.Rules.WeaponCooldownRules.MinimumFireInterval,
            20,
            24f,
            3.25f,
            0.85f,
            LayerMask.GetMask(GameLayers.Enemy),
            30f,
            0.9f,
            0.8f);
        return weapon;
    }

    public static WeaponCatalog CreateWeaponCatalog(params WeaponDefinition[] weapons)
    {
        var catalog = ScriptableObject.CreateInstance<WeaponCatalog>();
        if (weapons != null)
        {
            catalog.startingWeapons.AddRange(weapons);
            catalog.availableWeapons.AddRange(weapons);
        }

        return catalog;
    }

    public static ItemDefinition CreateItemDefinition(
        string id,
        string displayName,
        int maxStacks = 5,
        ItemStackPolicy stackPolicy = ItemStackPolicy.StackLinear,
        params ItemStatModifierDefinition[] modifiers)
    {
        var item = ScriptableObject.CreateInstance<ItemDefinition>();
        item.ApplyDefaults(
            id,
            displayName,
            "+1 test item.",
            ContentRarity.Common,
            ContentTagSet.With(ContentTag.Utility),
            stackPolicy,
            maxStacks,
            Color.white,
            modifiers != null && modifiers.Length > 0
                ? modifiers
                : new[] { new ItemStatModifierDefinition(StatId.PlayerArmor, StatModifierBucket.Flat, 1f) });
        return item;
    }

    public static ItemCatalog CreateItemCatalog(params ItemDefinition[] items)
    {
        var catalog = ScriptableObject.CreateInstance<ItemCatalog>();
        if (items != null)
        {
            catalog.items.AddRange(items);
        }

        return catalog;
    }

    public static ChestDefinition CreateChestDefinition(
        string id,
        string displayName,
        ChestKind kind = ChestKind.Locked,
        int ticketCost = 35,
        float holdSeconds = 1f,
        ContentRarityWeightTable? rarityWeights = null)
    {
        var chest = ScriptableObject.CreateInstance<ChestDefinition>();
        chest.ApplyDefaults(
            id,
            displayName,
            "Test chest.",
            kind,
            holdSeconds,
            ticketCost,
            "Open Chest",
            rarityWeights ?? ChestDefinition.DefaultWeightsFor(kind),
            Color.yellow,
            1f);
        return chest;
    }

    public static ChestCatalog CreateChestCatalog(params ChestDefinition[] chests)
    {
        var catalog = ScriptableObject.CreateInstance<ChestCatalog>();
        if (chests != null)
        {
            catalog.chests.AddRange(chests);
        }

        return catalog;
    }

    public static TicketDepositDefinition CreateTicketDepositDefinition(
        string id,
        string displayName,
        float holdSeconds = 1f,
        int minTickets = 5,
        int maxTickets = 8,
        TicketDepositPayoutMode payoutMode = TicketDepositPayoutMode.OnComplete,
        int payoutChunks = 1)
    {
        var deposit = ScriptableObject.CreateInstance<TicketDepositDefinition>();
        deposit.ApplyDefaults(
            id,
            displayName,
            "Test ticket deposit.",
            TicketDepositTier.Small,
            holdSeconds,
            minTickets,
            maxTickets,
            payoutMode,
            payoutChunks,
            "Collect Tickets",
            Color.yellow,
            1f);
        return deposit;
    }

    public static TicketDepositCatalog CreateTicketDepositCatalog(params TicketDepositDefinition[] deposits)
    {
        var catalog = ScriptableObject.CreateInstance<TicketDepositCatalog>();
        if (deposits != null)
        {
            catalog.deposits.AddRange(deposits);
        }

        return catalog;
    }

    public static XpGemDefinition CreateXpGemDefinition(
        string id,
        string displayName,
        int amount,
        Color color,
        ContentRarity rarity = ContentRarity.Common,
        float visualScale = 1f,
        Color? emissionColor = null,
        float emissionStrength = XpGemDefinition.DefaultEmissionStrength)
    {
        var gem = ScriptableObject.CreateInstance<XpGemDefinition>();
        gem.ApplyDefaults(id, displayName, amount, color, rarity, visualScale);
        gem.emissionColor = emissionColor ?? color;
        gem.emissionStrength = Mathf.Max(0f, emissionStrength);
        return gem;
    }

    public static XpGemCatalog CreateXpGemCatalog(params XpGemDefinition[] gems)
    {
        var catalog = ScriptableObject.CreateInstance<XpGemCatalog>();
        if (gems != null)
        {
            catalog.gems.AddRange(gems);
        }

        catalog.defaultEnemyXpBudget = gems != null && gems.Length > 0 && gems[0] != null
            ? gems[0].xpAmount
            : 7;
        return catalog;
    }

    public static HealthPickupDefinition CreateHealthPickupDefinition(
        string id,
        string displayName,
        int healAmount = HealthPickupDefinition.DefaultHealAmount,
        Color? color = null,
        float visualScale = 1.05f,
        float healPercentOfMaxHealth = HealthPickupDefinition.DefaultHealPercentOfMaxHealth)
    {
        var pickup = ScriptableObject.CreateInstance<HealthPickupDefinition>();
        Color tint = color ?? new Color(1f, 0.2f, 0.42f, 1f);
        pickup.ApplyDefaults(
            id,
            displayName,
            "Test health pickup.",
            healAmount,
            tint,
            visualScale,
            healPercentOfMaxHealth);
        return pickup;
    }

    public static HealthPickupCatalog CreateHealthPickupCatalog(params HealthPickupDefinition[] pickups)
    {
        var catalog = ScriptableObject.CreateInstance<HealthPickupCatalog>();
        if (pickups != null)
        {
            catalog.pickups.AddRange(pickups);
            catalog.defaultHealthPickup = pickups.Length > 0 ? pickups[0] : null;
        }

        return catalog;
    }

    public static HealingPropDefinition CreateHealingPropDefinition(
        string id,
        string displayName,
        HealthPickupDefinition pickup = null,
        float holdSeconds = 1f,
        int pickupCount = 1)
    {
        var prop = ScriptableObject.CreateInstance<HealingPropDefinition>();
        prop.ApplyDefaults(
            id,
            displayName,
            "Test healing prop.",
            holdSeconds,
            "Grab a Snack",
            pickup,
            pickupCount,
            pickupCount,
            new Color(1f, 0.24f, 0.42f, 1f),
            0.85f);
        return prop;
    }

    public static HealingPropCatalog CreateHealingPropCatalog(params HealingPropDefinition[] props)
    {
        var catalog = ScriptableObject.CreateInstance<HealingPropCatalog>();
        if (props != null)
        {
            catalog.props.AddRange(props);
        }

        return catalog;
    }

    public static WorldRewardPlacementDefinition CreateWorldRewardPlacement(
        string id,
        string displayName,
        WorldRewardPlacementKind kind,
        string targetContentId,
        Vector3 offset)
    {
        return WorldRewardPlacementDefinition.Create(id, displayName, kind, targetContentId, offset);
    }

    public static WorldRewardPlacementCatalog CreateWorldRewardPlacementCatalog(params WorldRewardPlacementDefinition[] placements)
    {
        var catalog = ScriptableObject.CreateInstance<WorldRewardPlacementCatalog>();
        if (placements != null)
        {
            catalog.placements.AddRange(placements);
        }

        return catalog;
    }

    public static EnemyDefinition CreateEnemyDefinition(GameConfig config)
    {
        var enemy = ScriptableObject.CreateInstance<EnemyDefinition>();
        enemy.ApplyDefaultsFromGameConfig(config);
        return enemy;
    }

    public static EnemyCatalog CreateEnemyCatalog(params EnemyDefinition[] enemies)
    {
        var catalog = ScriptableObject.CreateInstance<EnemyCatalog>();
        if (enemies != null)
        {
            catalog.enemies.AddRange(enemies);
        }

        return catalog;
    }

    public static UpgradeDefinition CreateUpgradeDefinition(
        string id,
        string displayName,
        ContentRarity rarity = ContentRarity.Common,
        int maxLevel = 5,
        params UpgradeStatModifierDefinition[] modifiers)
    {
        var upgrade = ScriptableObject.CreateInstance<UpgradeDefinition>();
        WeaponDefinition weapon = CreateWeaponDefinition(null);
        upgrade.ApplyWeaponStatUpgradeDefaults(
            weapon,
            id,
            displayName,
            "+1 test weapon upgrade.",
            Color.white,
            modifiers != null && modifiers.Length > 0
                ? modifiers
                : new[] { new UpgradeStatModifierDefinition(StatId.WeaponFlatDamage, StatModifierBucket.Flat, 1f) });
        upgrade.maxLevel = maxLevel;
        return upgrade;
    }

    public static UpgradeDefinition CreateWeaponStatUpgrade(
        WeaponDefinition weapon,
        string idSuffix = "damage",
        params UpgradeStatModifierDefinition[] modifiers)
    {
        var upgrade = ScriptableObject.CreateInstance<UpgradeDefinition>();
        upgrade.ApplyWeaponStatUpgradeDefaults(
            weapon,
            idSuffix,
            "Damage",
            "+1 test weapon upgrade.",
            Color.white,
            modifiers != null && modifiers.Length > 0
                ? modifiers
                : new[] { new UpgradeStatModifierDefinition(StatId.GlobalDamageMultiplier, StatModifierBucket.AdditivePercent, 0.1f) });
        return upgrade;
    }

    public static UpgradeCatalog CreateUpgradeCatalog(params UpgradeDefinition[] upgrades)
    {
        var catalog = ScriptableObject.CreateInstance<UpgradeCatalog>();
        if (upgrades != null)
        {
            catalog.upgrades.AddRange(upgrades);
        }

        catalog.levelUpRarityWeights = ContentRarityWeightTable.LevelUpDefault();
        return catalog;
    }

    public static TalentDefinition CreateTalentDefinition(
        string id,
        string displayName,
        ContentRarity rarity = ContentRarity.Common,
        TalentRepeatPolicy repeatPolicy = TalentRepeatPolicy.Infinite,
        int maxLevel = 5,
        params UpgradeStatModifierDefinition[] modifiers)
    {
        var talent = ScriptableObject.CreateInstance<TalentDefinition>();
        talent.ApplyDefaults(
            id,
            displayName,
            "+1 test talent.",
            ContentTagSet.With(ContentTag.Utility),
            Color.white,
            modifiers != null && modifiers.Length > 0
                ? modifiers
                : new[] { new UpgradeStatModifierDefinition(StatId.PlayerArmor, StatModifierBucket.Flat, 1f) });
        talent.possibleRarities = ContentRaritySet.Only(rarity);
        talent.repeatPolicy = repeatPolicy;
        talent.maxLevel = maxLevel;
        return talent;
    }

    public static TalentCatalog CreateTalentCatalog(params TalentDefinition[] talents)
    {
        var catalog = ScriptableObject.CreateInstance<TalentCatalog>();
        if (talents != null)
        {
            catalog.talents.AddRange(talents);
        }

        return catalog;
    }

    public static PerformerDefinition CreatePerformerDefinition(
        string id,
        string displayName,
        WeaponDefinition startingWeapon,
        TalentDefinition uniqueTalent = null,
        PerformerPassiveKind passiveKind = PerformerPassiveKind.None,
        UpgradeStatModifierDefinition[] baseModifiers = null,
        UpgradeStatModifierDefinition[] passiveModifiers = null)
    {
        var performer = ScriptableObject.CreateInstance<PerformerDefinition>();
        performer.ApplyDefaults(
            id,
            displayName,
            "Test performer.",
            ContentTagSet.With(ContentTag.Utility),
            startingWeapon,
            Color.white,
            passiveKind == PerformerPassiveKind.None ? "No special passive." : "Test passive.",
            passiveKind,
            "Test talent theme.",
            baseModifiers,
            passiveModifiers,
            uniqueTalent);
        if (uniqueTalent != null)
        {
            uniqueTalent.poolKind = TalentPoolKind.PerformerSpecific;
            uniqueTalent.performerDefinition = performer;
            uniqueTalent.performerId = performer.Id;
        }

        return performer;
    }

    public static PerformerCatalog CreatePerformerCatalog(PerformerDefinition defaultPerformer, params PerformerDefinition[] performers)
    {
        var catalog = ScriptableObject.CreateInstance<PerformerCatalog>();
        catalog.defaultPerformer = defaultPerformer;
        if (performers != null)
        {
            catalog.performers.AddRange(performers);
        }

        return catalog;
    }
}
