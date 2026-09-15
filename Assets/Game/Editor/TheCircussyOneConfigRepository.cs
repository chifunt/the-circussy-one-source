using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using TheCircussyOne.Runtime;

public static class TheCircussyOneConfigRepository
{
    public static GameConfig GetOrCreateGameConfig()
    {
        GameConfig config = AssetDatabase.LoadAssetAtPath<GameConfig>(TheCircussyOneAssetPaths.GameConfigPath);
        if (config != null)
        {
            MarkDirtyIf(config, config.EnsureWorkflowDefaults());
            return config;
        }

        config = CreateAsset<GameConfig>(TheCircussyOneAssetPaths.GameConfigPath);
        ApplyGameConfigDefaults(config);
        config.EnsureWorkflowDefaults();
        EditorUtility.SetDirty(config);
        return config;
    }

    public static RunScheduleConfig GetOrCreateRunScheduleConfig()
    {
        RunScheduleConfig config = AssetDatabase.LoadAssetAtPath<RunScheduleConfig>(TheCircussyOneAssetPaths.RunScheduleConfigPath);
        if (config != null)
        {
            MarkDirtyIf(config, config.EnsureWorkflowDefaults());
            return config;
        }

        config = CreateAsset<RunScheduleConfig>(TheCircussyOneAssetPaths.RunScheduleConfigPath);
        config.ApplyDefaultSchedule();
        EditorUtility.SetDirty(config);
        return config;
    }

    public static RunWorldGenerationConfig GetOrCreateRunWorldGenerationConfig()
    {
        RunWorldGenerationConfig config = AssetDatabase.LoadAssetAtPath<RunWorldGenerationConfig>(TheCircussyOneAssetPaths.RunWorldGenerationConfigPath);
        if (config != null)
        {
            bool changed = config.EnsureWorkflowDefaults();
            if (config.terrainMaterial == null)
            {
                config.terrainMaterial = AssetDatabase.LoadAssetAtPath<Material>(TheCircussyOneAssetPaths.FloorMaterialPath);
                changed = true;
            }

            MarkDirtyIf(config, changed);
            return config;
        }

        config = CreateAsset<RunWorldGenerationConfig>(TheCircussyOneAssetPaths.RunWorldGenerationConfigPath);
        config.ApplyDefaultProfiles();
        config.terrainMaterial = AssetDatabase.LoadAssetAtPath<Material>(TheCircussyOneAssetPaths.FloorMaterialPath);
        EditorUtility.SetDirty(config);
        return config;
    }

    public static CameraConfig GetOrCreateCameraConfig()
    {
        CameraConfig config = AssetDatabase.LoadAssetAtPath<CameraConfig>(TheCircussyOneAssetPaths.CameraConfigPath);
        if (config != null)
        {
            MarkDirtyIf(config, config.EnsureObstructionDefaults());
            return config;
        }

        config = CreateAsset<CameraConfig>(TheCircussyOneAssetPaths.CameraConfigPath);
        config.followSharpness = 9f;
        config.orbitDistance = 20f;
        config.orbitYaw = 0f;
        config.orbitPitch = 36f;
        config.minPitch = 20f;
        config.maxPitch = 58f;
        config.targetHeight = 1.35f;
        config.lookAheadDistance = 9f;
        config.topDownLookAheadDistance = 0f;
        config.lookAheadPitchBias = 1.15f;
        config.lookSensitivityMultiplier = 1f;
        config.mouseYawSensitivity = 0.08f;
        config.mousePitchSensitivity = 0.06f;
        config.gamepadYawSpeed = 130f;
        config.gamepadPitchSpeed = 90f;
        config.EnsureObstructionDefaults();
        config.EnsureWorkflowDefaults();
        EditorUtility.SetDirty(config);
        return config;
    }

    public static WeaponDefinition GetOrCreateCannonWeapon()
        => TheCircussyOneWeaponStarterContent.GetOrCreateCannonWeapon();

    public static WeaponDefinition GetOrCreateKnifeFanWeapon()
        => TheCircussyOneWeaponStarterContent.GetOrCreateKnifeFanWeapon();

    public static WeaponDefinition GetOrCreateSpotlightBoltWeapon()
        => TheCircussyOneWeaponStarterContent.GetOrCreateSpotlightBoltWeapon();

    public static WeaponDefinition GetOrCreateFireHoopWeapon()
        => TheCircussyOneWeaponStarterContent.GetOrCreateFireHoopWeapon();

    public static WeaponDefinition GetOrCreateJugglingBallWeapon()
        => TheCircussyOneWeaponStarterContent.GetOrCreateJugglingBallWeapon();

    public static WeaponCatalog GetOrCreateWeaponCatalog()
        => TheCircussyOneWeaponStarterContent.GetOrCreateWeaponCatalog();

    public static ItemDefinition GetOrCreateRubberSolesItem()
    {
        return GetOrCreateItem(
            TheCircussyOneAssetPaths.RubberSolesItemPath,
            "rubber_soles",
            "Rubber Soles",
            "Move faster.",
            ContentRarity.Common,
            ContentTagSet.With(ContentTag.Movement, ContentTag.Utility),
            ItemStackPolicy.StackLinear,
            5,
            new Color(0.92f, 0.95f, 0.82f, 1f),
            new ItemStatModifierDefinition(TheCircussyOne.Stats.StatId.PlayerMoveSpeedMultiplier, TheCircussyOne.Stats.StatModifierBucket.AdditivePercent, 0.08f));
    }

    public static ItemDefinition GetOrCreateSafetyPaddingItem()
    {
        return GetOrCreateItem(
            TheCircussyOneAssetPaths.SafetyPaddingItemPath,
            "safety_padding",
            "Safety Padding",
            "Reduce contact damage pressure with extra armor.",
            ContentRarity.Common,
            ContentTagSet.With(ContentTag.Defense, ContentTag.Utility),
            ItemStackPolicy.StackLinear,
            5,
            new Color(0.78f, 0.9f, 1f, 1f),
            new ItemStatModifierDefinition(TheCircussyOne.Stats.StatId.PlayerArmor, TheCircussyOne.Stats.StatModifierBucket.Flat, 4f));
    }

    public static ItemDefinition GetOrCreateMagnetCharmItem()
    {
        return GetOrCreateItem(
            TheCircussyOneAssetPaths.MagnetCharmItemPath,
            "magnet_charm",
            "Magnet Charm",
            "Pull XP gems from farther away.",
            ContentRarity.Uncommon,
            ContentTagSet.With(ContentTag.Economy, ContentTag.Utility),
            ItemStackPolicy.StackLinear,
            5,
            new Color(0.44f, 0.92f, 1f, 1f),
            new ItemStatModifierDefinition(TheCircussyOne.Stats.StatId.PickupMagnetRadius, TheCircussyOne.Stats.StatModifierBucket.AdditivePercent, 0.18f));
    }

    public static ItemDefinition GetOrCreateLuckyCoinItem()
    {
        return GetOrCreateItem(
            TheCircussyOneAssetPaths.LuckyCoinItemPath,
            "lucky_coin",
            "Lucky Coin",
            "Improve reward quality rolls.",
            ContentRarity.Uncommon,
            ContentTagSet.With(ContentTag.Economy, ContentTag.Utility),
            ItemStackPolicy.StackLinear,
            5,
            new Color(1f, 0.86f, 0.34f, 1f),
            new ItemStatModifierDefinition(TheCircussyOne.Stats.StatId.Luck, TheCircussyOne.Stats.StatModifierBucket.Flat, 10f));
    }

    public static ItemDefinition GetOrCreateStudyNotesItem()
    {
        return GetOrCreateItem(
            TheCircussyOneAssetPaths.StudyNotesItemPath,
            "study_notes",
            "Study Notes",
            "Gain more XP from collected gems.",
            ContentRarity.Common,
            ContentTagSet.With(ContentTag.Economy, ContentTag.Utility),
            ItemStackPolicy.StackLinear,
            5,
            new Color(0.55f, 0.86f, 1f, 1f),
            new ItemStatModifierDefinition(TheCircussyOne.Stats.StatId.XpGainMultiplier, TheCircussyOne.Stats.StatModifierBucket.AdditivePercent, 0.10f));
    }

    public static ItemDefinition GetOrCreateTempoBraceletItem()
    {
        return GetOrCreateItem(
            TheCircussyOneAssetPaths.TempoBraceletItemPath,
            "tempo_bracelet",
            "Tempo Bracelet",
            "Fire weapons more often.",
            ContentRarity.Uncommon,
            ContentTagSet.With(ContentTag.Utility, ContentTag.Projectile),
            ItemStackPolicy.StackLinear,
            5,
            new Color(0.95f, 0.74f, 1f, 1f),
            new ItemStatModifierDefinition(TheCircussyOne.Stats.StatId.GlobalWeaponHaste, TheCircussyOne.Stats.StatModifierBucket.AdditivePercent, 0.08f));
    }

    public static ItemDefinition GetOrCreatePowderFlaskItem()
    {
        return GetOrCreateItem(
            TheCircussyOneAssetPaths.PowderFlaskItemPath,
            "powder_flask",
            "Powder Flask",
            "Increase global weapon damage.",
            ContentRarity.Common,
            ContentTagSet.With(ContentTag.Damage, ContentTag.Physical),
            ItemStackPolicy.StackLinear,
            5,
            new Color(1f, 0.48f, 0.3f, 1f),
            new ItemStatModifierDefinition(TheCircussyOne.Stats.StatId.GlobalDamageMultiplier, TheCircussyOne.Stats.StatModifierBucket.AdditivePercent, 0.12f));
    }

    public static ItemDefinition GetOrCreateSpringBootsItem()
    {
        return GetOrCreateItem(
            TheCircussyOneAssetPaths.SpringBootsItemPath,
            "spring_boots",
            "Spring Boots",
            "Launch projectiles faster.",
            ContentRarity.Common,
            ContentTagSet.With(ContentTag.Projectile, ContentTag.Movement),
            ItemStackPolicy.StackLinear,
            5,
            new Color(0.78f, 1f, 0.54f, 1f),
            new ItemStatModifierDefinition(TheCircussyOne.Stats.StatId.ProjectileSpeedMultiplier, TheCircussyOne.Stats.StatModifierBucket.AdditivePercent, 0.10f));
    }

    public static ItemDefinition GetOrCreateWideLensItem()
    {
        return GetOrCreateItem(
            TheCircussyOneAssetPaths.WideLensItemPath,
            "wide_lens",
            "Wide Lens",
            "Increase projectile area where supported.",
            ContentRarity.Uncommon,
            ContentTagSet.With(ContentTag.Projectile, ContentTag.Utility),
            ItemStackPolicy.StackLinear,
            5,
            new Color(0.52f, 1f, 0.9f, 1f),
            new ItemStatModifierDefinition(TheCircussyOne.Stats.StatId.ProjectileAreaMultiplier, TheCircussyOne.Stats.StatModifierBucket.AdditivePercent, 0.12f));
    }

    public static ItemDefinition GetOrCreateLongFuseItem()
    {
        return GetOrCreateItem(
            TheCircussyOneAssetPaths.LongFuseItemPath,
            "long_fuse",
            "Long Fuse",
            "Increase projectile duration where supported.",
            ContentRarity.Common,
            ContentTagSet.With(ContentTag.Projectile, ContentTag.Utility),
            ItemStackPolicy.StackLinear,
            5,
            new Color(1f, 0.74f, 0.42f, 1f),
            new ItemStatModifierDefinition(TheCircussyOne.Stats.StatId.ProjectileDurationMultiplier, TheCircussyOne.Stats.StatModifierBucket.AdditivePercent, 0.15f));
    }

    public static ItemCatalog GetOrCreateItemCatalog()
    {
        ItemDefinition[] starterItems =
        {
            GetOrCreateRubberSolesItem(),
            GetOrCreateSafetyPaddingItem(),
            GetOrCreateMagnetCharmItem(),
            GetOrCreateLuckyCoinItem(),
            GetOrCreateStudyNotesItem(),
            GetOrCreateTempoBraceletItem(),
            GetOrCreatePowderFlaskItem(),
            GetOrCreateSpringBootsItem(),
            GetOrCreateWideLensItem(),
            GetOrCreateLongFuseItem()
        };

        ItemCatalog catalog = AssetDatabase.LoadAssetAtPath<ItemCatalog>(TheCircussyOneAssetPaths.ItemCatalogPath);
        if (catalog != null)
        {
            MarkDirtyIf(catalog, catalog.EnsureWorkflowDefaults(starterItems));
            return catalog;
        }

        catalog = CreateAsset<ItemCatalog>(TheCircussyOneAssetPaths.ItemCatalogPath);
        catalog.EnsureWorkflowDefaults(starterItems);
        EditorUtility.SetDirty(catalog);
        return catalog;
    }

    public static ChestDefinition GetOrCreateOpenChest()
    {
        return GetOrCreateChest(
            TheCircussyOneAssetPaths.OpenChestPath,
            "open_chest",
            "Open Chest",
            "A free prize box with one item inside.",
            ChestKind.Open,
            0.5f,
            0,
            "Open Chest",
            ChestDefinition.DefaultWeightsFor(ChestKind.Open),
            new Color(0.52f, 0.35f, 0.16f, 1f),
            0.9f);
    }

    public static ChestDefinition GetOrCreateLockedChest()
    {
        return GetOrCreateChest(
            TheCircussyOneAssetPaths.LockedChestPath,
            "locked_chest",
            "Locked Chest",
            "Spend Tickets to claim one item.",
            ChestKind.Locked,
            1f,
            35,
            "Open Chest",
            ChestDefinition.DefaultWeightsFor(ChestKind.Locked),
            new Color(0.62f, 0.36f, 0.14f, 1f),
            1f);
    }

    public static ChestDefinition GetOrCreatePremiumChest()
    {
        return GetOrCreateChest(
            TheCircussyOneAssetPaths.PremiumChestPath,
            "premium_chest",
            "Premium Chest",
            "A costly prize chest with better item odds.",
            ChestKind.Premium,
            1.25f,
            60,
            "Open Chest",
            ChestDefinition.DefaultWeightsFor(ChestKind.Premium),
            new Color(0.95f, 0.58f, 0.12f, 1f),
            1.1f);
    }

    public static ChestDefinition GetOrCreateSpecialEnemyChest()
    {
        return GetOrCreateChest(
            TheCircussyOneAssetPaths.SpecialEnemyChestPath,
            "special_enemy_chest",
            "Special Enemy Chest",
            "A free reward chest earned by beating a special enemy.",
            ChestKind.SpecialEnemy,
            0.75f,
            0,
            "Open Chest",
            ChestDefinition.DefaultWeightsFor(ChestKind.SpecialEnemy),
            new Color(0.72f, 0.42f, 0.82f, 1f),
            1f);
    }

    public static ChestCatalog GetOrCreateChestCatalog()
    {
        ChestDefinition[] starterChests =
        {
            GetOrCreateOpenChest(),
            GetOrCreateLockedChest(),
            GetOrCreatePremiumChest(),
            GetOrCreateSpecialEnemyChest()
        };

        ChestCatalog catalog = AssetDatabase.LoadAssetAtPath<ChestCatalog>(TheCircussyOneAssetPaths.ChestCatalogPath);
        if (catalog != null)
        {
            MarkDirtyIf(catalog, catalog.EnsureWorkflowDefaults(starterChests));
            return catalog;
        }

        catalog = CreateAsset<ChestCatalog>(TheCircussyOneAssetPaths.ChestCatalogPath);
        catalog.EnsureWorkflowDefaults(starterChests);
        SetDirtyAndSave(catalog);
        return catalog;
    }

    public static TicketDepositDefinition GetOrCreateSmallTicketStack()
    {
        return GetOrCreateTicketDeposit(
            TheCircussyOneAssetPaths.SmallTicketStackPath,
            "small_ticket_stack",
            "Small Ticket Stack",
            "A quick bundle of loose prize tickets.",
            TicketDepositTier.Small,
            1f,
            5,
            8,
            TicketDepositPayoutMode.OnComplete,
            1,
            "Collect Tickets",
            new Color(1f, 0.86f, 0.22f, 1f),
            0.75f);
    }

    public static TicketDepositDefinition GetOrCreateTicketRoll()
    {
        return GetOrCreateTicketDeposit(
            TheCircussyOneAssetPaths.TicketRollPath,
            "ticket_roll",
            "Ticket Roll",
            "A cranky roll worth holding your ground for.",
            TicketDepositTier.Medium,
            2.5f,
            18,
            25,
            TicketDepositPayoutMode.Chunked,
            3,
            "Collect Tickets",
            new Color(1f, 0.72f, 0.18f, 1f),
            1f);
    }

    public static TicketDepositDefinition GetOrCreateJackpotCache()
    {
        return GetOrCreateTicketDeposit(
            TheCircussyOneAssetPaths.JackpotCachePath,
            "jackpot_cache",
            "Jackpot Cache",
            "A loud little payout box with enough Tickets to tempt trouble.",
            TicketDepositTier.Large,
            4f,
            45,
            60,
            TicketDepositPayoutMode.Chunked,
            4,
            "Collect Tickets",
            new Color(1f, 0.58f, 0.08f, 1f),
            1.25f);
    }

    public static TicketDepositCatalog GetOrCreateTicketDepositCatalog()
    {
        TicketDepositDefinition[] starterDeposits =
        {
            GetOrCreateSmallTicketStack(),
            GetOrCreateTicketRoll(),
            GetOrCreateJackpotCache()
        };

        TicketDepositCatalog catalog = AssetDatabase.LoadAssetAtPath<TicketDepositCatalog>(TheCircussyOneAssetPaths.TicketDepositCatalogPath);
        if (catalog != null)
        {
            MarkDirtyIf(catalog, catalog.EnsureWorkflowDefaults(starterDeposits));
            return catalog;
        }

        catalog = CreateAsset<TicketDepositCatalog>(TheCircussyOneAssetPaths.TicketDepositCatalogPath);
        catalog.EnsureWorkflowDefaults(starterDeposits);
        EditorUtility.SetDirty(catalog);
        return catalog;
    }

    private static ItemDefinition GetOrCreateItem(
        string path,
        string id,
        string displayName,
        string description,
        ContentRarity rarity,
        ContentTagSet tags,
        ItemStackPolicy stackPolicy,
        int maxStacks,
        Color iconColor,
        params ItemStatModifierDefinition[] modifiers)
    {
        ItemDefinition item = AssetDatabase.LoadAssetAtPath<ItemDefinition>(path);
        if (item != null)
        {
            MarkDirtyIf(item, item.EnsureWorkflowDefaults());
            return item;
        }

        item = CreateAsset<ItemDefinition>(path);
        item.ApplyDefaults(id, displayName, description, rarity, tags, stackPolicy, maxStacks, iconColor, modifiers);
        EditorUtility.SetDirty(item);
        return item;
    }

    private static TicketDepositDefinition GetOrCreateTicketDeposit(
        string path,
        string id,
        string displayName,
        string description,
        TicketDepositTier tier,
        float holdSeconds,
        int minTickets,
        int maxTickets,
        TicketDepositPayoutMode payoutMode,
        int payoutChunks,
        string promptText,
        Color visualColor,
        float visualScale)
    {
        TicketDepositDefinition deposit = AssetDatabase.LoadAssetAtPath<TicketDepositDefinition>(path);
        if (deposit != null)
        {
            bool changed = deposit.EnsureWorkflowDefaults();
            changed |= EnsureTicketBurstTexture(deposit);
            MarkDirtyIf(deposit, changed);
            return deposit;
        }

        deposit = CreateAsset<TicketDepositDefinition>(path);
        deposit.ApplyDefaults(id, displayName, description, tier, holdSeconds, minTickets, maxTickets, payoutMode, payoutChunks, promptText, visualColor, visualScale);
        EnsureTicketBurstTexture(deposit);
        EditorUtility.SetDirty(deposit);
        return deposit;
    }

    private static bool EnsureTicketBurstTexture(TicketDepositDefinition deposit)
    {
        if (deposit == null || deposit.ticketBurstTexture != null)
        {
            return false;
        }

        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(TheCircussyOneAssetPaths.HudTicketsIconPath);
        if (texture == null)
        {
            return false;
        }

        deposit.ticketBurstTexture = texture;
        return true;
    }

    private static ChestDefinition GetOrCreateChest(
        string path,
        string id,
        string displayName,
        string description,
        ChestKind kind,
        float holdSeconds,
        int ticketCost,
        string promptText,
        ContentRarityWeightTable rarityWeights,
        Color visualColor,
        float visualScale)
    {
        ChestDefinition chest = AssetDatabase.LoadAssetAtPath<ChestDefinition>(path);
        if (chest != null)
        {
            MarkDirtyIf(chest, chest.EnsureWorkflowDefaults());
            return chest;
        }

        chest = CreateAsset<ChestDefinition>(path);
        chest.ApplyDefaults(id, displayName, description, kind, holdSeconds, ticketCost, promptText, rarityWeights, visualColor, visualScale);
        SetDirtyAndSave(chest);
        return chest;
    }

    public static XpGemDefinition GetOrCreateBlueXpGem()
    {
        return GetOrCreateXpGem(
            TheCircussyOneAssetPaths.BlueXpGemPath,
            "blue_xp_gem",
            "Blue XP Gem",
            XpGemDefinition.DefaultBlueXpAmount,
            new Color(0.27f, 0.96f, 1f, 1f),
            ContentRarity.Common,
            1f);
    }

    public static XpGemDefinition GetOrCreateGreenXpGem()
    {
        return GetOrCreateXpGem(
            TheCircussyOneAssetPaths.GreenXpGemPath,
            "green_xp_gem",
            "Green XP Gem",
            23,
            new Color(0.38f, 1f, 0.42f, 1f),
            ContentRarity.Uncommon,
            1.12f);
    }

    public static XpGemDefinition GetOrCreateRedXpGem()
    {
        return GetOrCreateXpGem(
            TheCircussyOneAssetPaths.RedXpGemPath,
            "red_xp_gem",
            "Red XP Gem",
            73,
            new Color(1f, 0.22f, 0.34f, 1f),
            ContentRarity.Rare,
            1.28f);
    }

    public static XpGemCatalog GetOrCreateXpGemCatalog()
    {
        XpGemDefinition blueGem = GetOrCreateBlueXpGem();
        XpGemDefinition greenGem = GetOrCreateGreenXpGem();
        XpGemDefinition redGem = GetOrCreateRedXpGem();
        XpGemCatalog catalog = AssetDatabase.LoadAssetAtPath<XpGemCatalog>(TheCircussyOneAssetPaths.XpGemCatalogPath);
        int defaultBudget = Mathf.Max(0, blueGem != null ? blueGem.xpAmount : XpGemDefinition.DefaultBlueXpAmount);
        if (catalog != null)
        {
            bool changed = catalog.EnsureWorkflowDefaults(blueGem, greenGem, redGem, defaultBudget);
            MarkDirtyIf(catalog, changed);
            return catalog;
        }

        catalog = CreateAsset<XpGemCatalog>(TheCircussyOneAssetPaths.XpGemCatalogPath);
        catalog.defaultEnemyXpBudget = defaultBudget;
        catalog.defaultDropStyle = TheCircussyOne.Rules.XpDropStyle.Compact;
        catalog.EnsureWorkflowDefaults(blueGem, greenGem, redGem, defaultBudget);
        EditorUtility.SetDirty(catalog);
        return catalog;
    }

    public static HealthPickupDefinition GetOrCreateTreat()
    {
        return GetOrCreateHealthPickup(
            TheCircussyOneAssetPaths.TreatPath,
            "treat",
            "Treat",
            "A circus treat that patches up a rough act.",
            HealthPickupDefinition.DefaultHealAmount,
            new Color(1f, 0.2f, 0.42f, 1f),
            1.05f);
    }

    public static HealthPickupCatalog GetOrCreateHealthPickupCatalog()
    {
        HealthPickupDefinition treat = GetOrCreateTreat();
        HealthPickupCatalog catalog = AssetDatabase.LoadAssetAtPath<HealthPickupCatalog>(TheCircussyOneAssetPaths.HealthPickupCatalogPath);
        if (catalog != null)
        {
            bool changed = catalog.EnsureWorkflowDefaults(treat);
            MarkDirtyIf(catalog, changed);
            return catalog;
        }

        catalog = CreateAsset<HealthPickupCatalog>(TheCircussyOneAssetPaths.HealthPickupCatalogPath);
        catalog.defaultHealthPickup = treat;
        catalog.EnsureWorkflowDefaults(treat);
        EditorUtility.SetDirty(catalog);
        return catalog;
    }

    public static HealingPropDefinition GetOrCreateSnackBox()
    {
        return GetOrCreateHealingProp(
            TheCircussyOneAssetPaths.SnackBoxPath,
            "snack_box",
            "Snack Box",
            "A quick little snack box for patching up between swarms.",
            1f,
            "Grab a Snack",
            GetOrCreateTreat(),
            1,
            1,
            new Color(1f, 0.24f, 0.42f, 1f),
            0.85f);
    }

    public static HealingPropDefinition GetOrCreateSnackCart()
    {
        return GetOrCreateHealingProp(
            TheCircussyOneAssetPaths.SnackCartPath,
            "snack_cart",
            "Snack Cart",
            "A snack cart with enough treats to risk a longer rummage.",
            2.25f,
            "Grab a Snack",
            GetOrCreateTreat(),
            2,
            3,
            new Color(1f, 0.58f, 0.18f, 1f),
            1.15f);
    }

    public static HealingPropCatalog GetOrCreateHealingPropCatalog()
    {
        HealingPropDefinition snackBox = GetOrCreateSnackBox();
        HealingPropDefinition snackCart = GetOrCreateSnackCart();
        HealingPropCatalog catalog = AssetDatabase.LoadAssetAtPath<HealingPropCatalog>(TheCircussyOneAssetPaths.HealingPropCatalogPath);
        if (catalog != null)
        {
            bool changed = catalog.EnsureWorkflowDefaults(snackBox, snackCart);
            MarkDirtyIf(catalog, changed);
            return catalog;
        }

        catalog = CreateAsset<HealingPropCatalog>(TheCircussyOneAssetPaths.HealingPropCatalogPath);
        catalog.EnsureWorkflowDefaults(snackBox, snackCart);
        EditorUtility.SetDirty(catalog);
        return catalog;
    }

    public static WorldRewardPlacementCatalog GetOrCreateWorldRewardPlacementCatalog()
    {
        WorldRewardPlacementDefinition[] starterPlacements = WorldRewardPlacementDefaults.CreatePrototypePlacements();
        WorldRewardPlacementCatalog catalog = AssetDatabase.LoadAssetAtPath<WorldRewardPlacementCatalog>(TheCircussyOneAssetPaths.WorldRewardPlacementCatalogPath);
        if (catalog != null)
        {
            bool changed = catalog.EnsureWorkflowDefaults(starterPlacements);
            MarkDirtyIf(catalog, changed);
            return catalog;
        }

        catalog = CreateAsset<WorldRewardPlacementCatalog>(TheCircussyOneAssetPaths.WorldRewardPlacementCatalogPath);
        catalog.EnsureWorkflowDefaults(starterPlacements);
        EditorUtility.SetDirty(catalog);
        return catalog;
    }

    public static EnemyDefinition GetOrCreateNormalEnemy()
    {
        EnemyDefinition enemy = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(TheCircussyOneAssetPaths.NormalEnemyPath);
        if (enemy != null)
        {
            bool changed = enemy.EnsureWorkflowDefaults();
            MarkDirtyIf(enemy, changed);
            return enemy;
        }

        enemy = CreateAsset<EnemyDefinition>(TheCircussyOneAssetPaths.NormalEnemyPath);
        enemy.ApplyDefaultsFromGameConfig(GetOrCreateGameConfig(), GetOrCreateXpGemCatalog());
        EditorUtility.SetDirty(enemy);
        return enemy;
    }

    public static EnemyCatalog GetOrCreateEnemyCatalog()
    {
        EnemyDefinition normalEnemy = GetOrCreateNormalEnemy();
        EnemyCatalog catalog = AssetDatabase.LoadAssetAtPath<EnemyCatalog>(TheCircussyOneAssetPaths.EnemyCatalogPath);
        if (catalog != null)
        {
            MarkDirtyIf(catalog, catalog.EnsureWorkflowDefaults(normalEnemy));
            return catalog;
        }

        catalog = CreateAsset<EnemyCatalog>(TheCircussyOneAssetPaths.EnemyCatalogPath);
        catalog.EnsureWorkflowDefaults(normalEnemy);
        EditorUtility.SetDirty(catalog);
        return catalog;
    }

    public static EnemyDefinition GetOrCreateOpeningHeadlinerEnemy()
    {
        EnemyDefinition enemy = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(TheCircussyOneAssetPaths.OpeningHeadlinerEnemyPath);
        if (enemy != null)
        {
            bool changed = enemy.EnsureWorkflowDefaults();
            MarkDirtyIf(enemy, changed);
            return enemy;
        }

        EnemyDefinition normalEnemy = GetOrCreateNormalEnemy();
        enemy = CreateAsset<EnemyDefinition>(TheCircussyOneAssetPaths.OpeningHeadlinerEnemyPath);
        enemy.ApplyDefaultsFromGameConfig(GetOrCreateGameConfig(), GetOrCreateXpGemCatalog());
        ApplyOpeningHeadlinerEnemyDefaults(enemy, normalEnemy);
        EditorUtility.SetDirty(enemy);
        return enemy;
    }

    public static HeadlinerDefinition GetOrCreateOpeningHeadliner()
    {
        EnemyDefinition actor = GetOrCreateOpeningHeadlinerEnemy();
        HeadlinerDefinition headliner = AssetDatabase.LoadAssetAtPath<HeadlinerDefinition>(TheCircussyOneAssetPaths.OpeningHeadlinerPath);
        if (headliner != null)
        {
            bool changed = false;
            changed |= headliner.EnsureWorkflowDefaults();
            if (headliner.enemyActor == null)
            {
                headliner.enemyActor = actor;
                changed = true;
            }

            MarkDirtyIf(headliner, changed);
            return headliner;
        }

        headliner = CreateAsset<HeadlinerDefinition>(TheCircussyOneAssetPaths.OpeningHeadlinerPath);
        headliner.ApplyDefaults(actor);
        EditorUtility.SetDirty(headliner);
        return headliner;
    }

    public static HeadlinerCatalog GetOrCreateHeadlinerCatalog()
    {
        HeadlinerDefinition openingHeadliner = GetOrCreateOpeningHeadliner();
        HeadlinerCatalog catalog = AssetDatabase.LoadAssetAtPath<HeadlinerCatalog>(TheCircussyOneAssetPaths.HeadlinerCatalogPath);
        if (catalog != null)
        {
            MarkDirtyIf(catalog, catalog.EnsureWorkflowDefaults(openingHeadliner));
            return catalog;
        }

        catalog = CreateAsset<HeadlinerCatalog>(TheCircussyOneAssetPaths.HeadlinerCatalogPath);
        catalog.EnsureWorkflowDefaults(openingHeadliner);
        EditorUtility.SetDirty(catalog);
        return catalog;
    }

    public static UpgradeDefinition GetOrCreateCannonTuningUpgrade()
        => TheCircussyOneWeaponStarterContent.GetOrCreateCannonTuningUpgrade();

    public static UpgradeDefinition GetOrCreateKnifeFanTuningUpgrade()
        => TheCircussyOneWeaponStarterContent.GetOrCreateKnifeFanTuningUpgrade();

    public static UpgradeDefinition GetOrCreateSpotlightBoltTuningUpgrade()
        => TheCircussyOneWeaponStarterContent.GetOrCreateSpotlightBoltTuningUpgrade();

    public static UpgradeDefinition GetOrCreateFireHoopTuningUpgrade()
        => TheCircussyOneWeaponStarterContent.GetOrCreateFireHoopTuningUpgrade();

    public static UpgradeDefinition GetOrCreateJugglingBallTuningUpgrade()
        => TheCircussyOneWeaponStarterContent.GetOrCreateJugglingBallTuningUpgrade();

    public static UpgradeCatalog GetOrCreateUpgradeCatalog()
    {
        return TheCircussyOneWeaponStarterContent.GetOrCreateUpgradeCatalog();
    }

    public static TalentDefinition GetOrCreateFootworkTalent()
    {
        return GetOrCreateTalent(TheCircussyOneAssetPaths.FootworkTalentPath, StarterTalentDefaults.FootworkId);
    }

    public static TalentDefinition GetOrCreateStageStaminaTalent()
    {
        return GetOrCreateTalent(TheCircussyOneAssetPaths.StageStaminaTalentPath, StarterTalentDefaults.StageStaminaId);
    }

    public static TalentDefinition GetOrCreateToughSkinTalent()
    {
        return GetOrCreateTalent(TheCircussyOneAssetPaths.ToughSkinTalentPath, StarterTalentDefaults.ToughSkinId);
    }

    public static TalentDefinition GetOrCreateCrowdFavoriteTalent()
    {
        return GetOrCreateTalent(TheCircussyOneAssetPaths.CrowdFavoriteTalentPath, StarterTalentDefaults.CrowdFavoriteId);
    }

    public static TalentDefinition GetOrCreateQuickHandsTalent()
    {
        return GetOrCreateTalent(TheCircussyOneAssetPaths.QuickHandsTalentPath, StarterTalentDefaults.QuickHandsId);
    }

    public static TalentDefinition GetOrCreateSharpEyeTalent()
    {
        return GetOrCreateTalent(TheCircussyOneAssetPaths.SharpEyeTalentPath, StarterTalentDefaults.SharpEyeId);
    }

    public static TalentDefinition GetOrCreateLongReachTalent()
    {
        return GetOrCreateTalent(TheCircussyOneAssetPaths.LongReachTalentPath, StarterTalentDefaults.LongReachId);
    }

    public static TalentDefinition GetOrCreateLuckyBreakTalent()
    {
        return GetOrCreateTalent(TheCircussyOneAssetPaths.LuckyBreakTalentPath, StarterTalentDefaults.LuckyBreakId);
    }

    public static TalentDefinition GetOrCreateJugglerTalent()
    {
        return GetOrCreatePerformerTalent(TheCircussyOneAssetPaths.JugglerTalentPath, StarterPerformerDefaults.JugglerTalentId, StarterPerformerDefaults.JugglerId);
    }

    public static TalentDefinition GetOrCreateCappaTalent()
    {
        return GetOrCreatePerformerTalent(TheCircussyOneAssetPaths.CappaTalentPath, StarterPerformerDefaults.CappaTalentId, StarterPerformerDefaults.DevsampleCappaId);
    }

    public static TalentDefinition GetOrCreateStrongmanTalent()
    {
        return GetOrCreatePerformerTalent(TheCircussyOneAssetPaths.StrongmanTalentPath, StarterPerformerDefaults.StrongmanTalentId, StarterPerformerDefaults.StrongmanId);
    }

    public static TalentDefinition GetOrCreateAcrobatTalent()
    {
        return GetOrCreatePerformerTalent(TheCircussyOneAssetPaths.AcrobatTalentPath, StarterPerformerDefaults.AcrobatTalentId, StarterPerformerDefaults.AcrobatId);
    }

    public static TalentDefinition GetOrCreateRingmasterTalent()
    {
        return GetOrCreatePerformerTalent(TheCircussyOneAssetPaths.RingmasterTalentPath, StarterPerformerDefaults.RingmasterTalentId, StarterPerformerDefaults.RingmasterId);
    }

    public static TalentCatalog GetOrCreateTalentCatalog()
    {
        var starterTalents = new List<TalentDefinition>();
        for (int i = 0; i < StarterTalentDefaults.AllIds.Length; i++)
        {
            starterTalents.Add(GetOrCreateTalent(TalentPathForId(StarterTalentDefaults.AllIds[i]), StarterTalentDefaults.AllIds[i]));
        }

        for (int i = 0; i < StarterPerformerDefaults.TalentIds.Length; i++)
        {
            string talentId = StarterPerformerDefaults.TalentIds[i];
            starterTalents.Add(GetOrCreatePerformerTalent(TalentPathForId(talentId), talentId, StarterPerformerDefaults.PerformerIdForTalentId(talentId)));
        }

        TalentCatalog catalog = AssetDatabase.LoadAssetAtPath<TalentCatalog>(TheCircussyOneAssetPaths.TalentCatalogPath);
        if (catalog != null)
        {
            MarkDirtyIf(catalog, catalog.EnsureWorkflowDefaults(starterTalents.ToArray()));
            return catalog;
        }

        catalog = CreateAsset<TalentCatalog>(TheCircussyOneAssetPaths.TalentCatalogPath);
        catalog.EnsureWorkflowDefaults(starterTalents.ToArray());
        EditorUtility.SetDirty(catalog);
        return catalog;
    }

    public static PerformerDefinition GetOrCreateJugglerPerformer()
    {
        PerformerDefinition performer = GetOrCreatePerformer(TheCircussyOneAssetPaths.JugglerPerformerPath, StarterPerformerDefaults.JugglerId, GetAnimancerHumanoidPrefab());
        MarkDirtyIf(performer, EnsureDevsampleSamVisualDefaults(performer));
        return performer;
    }

    public static PerformerDefinition GetOrCreateCappaPerformer()
    {
        return GetOrCreatePerformer(TheCircussyOneAssetPaths.CappaPerformerPath, StarterPerformerDefaults.DevsampleCappaId, GetOrCreateCappaModelPrefab());
    }

    public static PerformerDefinition GetOrCreateStrongmanPerformer()
    {
        return GetOrCreatePerformer(TheCircussyOneAssetPaths.StrongmanPerformerPath, StarterPerformerDefaults.StrongmanId, null);
    }

    public static PerformerDefinition GetOrCreateAcrobatPerformer()
    {
        return GetOrCreatePerformer(TheCircussyOneAssetPaths.AcrobatPerformerPath, StarterPerformerDefaults.AcrobatId, null);
    }

    public static PerformerDefinition GetOrCreateRingmasterPerformer()
    {
        return GetOrCreatePerformer(TheCircussyOneAssetPaths.RingmasterPerformerPath, StarterPerformerDefaults.RingmasterId, null);
    }

    public static PerformerDefinition GetOrCreateSolaHoopDancerPerformer()
    {
        return GetOrCreatePerformer(TheCircussyOneAssetPaths.SolaHoopDancerPerformerPath, StarterPerformerDefaults.SolaHoopDancerId, null);
    }

    public static PerformerDefinition GetOrCreateBibiBallJugglerPerformer()
    {
        return GetOrCreatePerformer(TheCircussyOneAssetPaths.BibiBallJugglerPerformerPath, StarterPerformerDefaults.BibiBallJugglerId, null);
    }

    public static PerformerCatalog GetOrCreatePerformerCatalog()
    {
        PerformerDefinition[] starterPerformers =
        {
            GetOrCreateJugglerPerformer(),
            GetOrCreateCappaPerformer(),
            GetOrCreateStrongmanPerformer(),
            GetOrCreateAcrobatPerformer(),
            GetOrCreateRingmasterPerformer(),
            GetOrCreateSolaHoopDancerPerformer(),
            GetOrCreateBibiBallJugglerPerformer()
        };

        PerformerCatalog catalog = AssetDatabase.LoadAssetAtPath<PerformerCatalog>(TheCircussyOneAssetPaths.PerformerCatalogPath);
        if (catalog != null)
        {
            MarkDirtyIf(catalog, catalog.EnsureWorkflowDefaults(starterPerformers));
            return catalog;
        }

        catalog = CreateAsset<PerformerCatalog>(TheCircussyOneAssetPaths.PerformerCatalogPath);
        catalog.EnsureWorkflowDefaults(starterPerformers);
        EditorUtility.SetDirty(catalog);
        return catalog;
    }

    private static TalentDefinition GetOrCreateTalent(string path, string id)
    {
        TalentDefinition talent = AssetDatabase.LoadAssetAtPath<TalentDefinition>(path);
        if (talent != null)
        {
            MarkDirtyIf(talent, talent.EnsureWorkflowDefaults());
            return talent;
        }

        talent = CreateAsset<TalentDefinition>(path);
        StarterTalentDefaults.Apply(talent, id);
        EditorUtility.SetDirty(talent);
        return talent;
    }

    private static TalentDefinition GetOrCreatePerformerTalent(string path, string id, string performerId)
    {
        TalentDefinition talent = AssetDatabase.LoadAssetAtPath<TalentDefinition>(path);
        if (talent != null)
        {
            MarkDirtyIf(talent, talent.EnsureWorkflowDefaults());
            return talent;
        }

        talent = CreateAsset<TalentDefinition>(path);
        StarterPerformerDefaults.ApplyTalent(talent, id, performerId);
        EditorUtility.SetDirty(talent);
        return talent;
    }

    private static PerformerDefinition GetOrCreatePerformer(
        string path,
        string id,
        GameObject worldPrefab)
    {
        PerformerDefinition performer = AssetDatabase.LoadAssetAtPath<PerformerDefinition>(path);
        if (performer != null)
        {
            MarkDirtyIf(performer, performer.EnsureWorkflowDefaults());
            return performer;
        }

        performer = CreateAsset<PerformerDefinition>(path);
        StarterPerformerDefaults.ApplyPerformer(
            performer,
            id,
            GetOrCreateCannonWeapon(),
            GetOrCreateKnifeFanWeapon(),
            GetOrCreateSpotlightBoltWeapon(),
            GetOrCreateFireHoopWeapon(),
            GetOrCreateJugglingBallWeapon(),
            GetOrCreatePerformerTalents(id));
        if (worldPrefab != null)
        {
            performer.worldPrefab = worldPrefab;
        }

        if (id == StarterPerformerDefaults.JugglerId)
        {
            EnsureDevsampleSamVisualDefaults(performer);
        }

        EditorUtility.SetDirty(performer);
        return performer;
    }

    private static TalentDefinition[] GetOrCreatePerformerTalents(string performerId)
    {
        string[] talentIds = StarterPerformerDefaults.TalentIdsForPerformer(performerId);
        var talents = new TalentDefinition[talentIds.Length];
        for (int i = 0; i < talentIds.Length; i++)
        {
            talents[i] = GetOrCreatePerformerTalent(TalentPathForId(talentIds[i]), talentIds[i], performerId);
        }

        return talents;
    }

    private static string TalentPathForId(string id)
    {
        return id switch
        {
            StarterTalentDefaults.FootworkId => TheCircussyOneAssetPaths.FootworkTalentPath,
            StarterTalentDefaults.StageStaminaId => TheCircussyOneAssetPaths.StageStaminaTalentPath,
            StarterTalentDefaults.ToughSkinId => TheCircussyOneAssetPaths.ToughSkinTalentPath,
            StarterTalentDefaults.CrowdFavoriteId => TheCircussyOneAssetPaths.CrowdFavoriteTalentPath,
            StarterTalentDefaults.QuickHandsId => TheCircussyOneAssetPaths.QuickHandsTalentPath,
            StarterTalentDefaults.SharpEyeId => TheCircussyOneAssetPaths.SharpEyeTalentPath,
            StarterTalentDefaults.LongReachId => TheCircussyOneAssetPaths.LongReachTalentPath,
            StarterTalentDefaults.LuckyBreakId => TheCircussyOneAssetPaths.LuckyBreakTalentPath,
            StarterTalentDefaults.BiggerPropsId => TheCircussyOneAssetPaths.BiggerPropsTalentPath,
            StarterTalentDefaults.LongerActId => TheCircussyOneAssetPaths.LongerActTalentPath,
            StarterTalentDefaults.MagneticApplauseId => TheCircussyOneAssetPaths.MagneticApplauseTalentPath,
            StarterTalentDefaults.SpringboardId => TheCircussyOneAssetPaths.SpringboardTalentPath,
            StarterPerformerDefaults.CleanCompileTalentId => TheCircussyOneAssetPaths.CleanCompileTalentPath,
            StarterPerformerDefaults.SampleArcTalentId => TheCircussyOneAssetPaths.SampleArcTalentPath,
            StarterPerformerDefaults.DebugMarkerTalentId => TheCircussyOneAssetPaths.DebugMarkerTalentPath,
            StarterPerformerDefaults.SteadyBaselineTalentId => TheCircussyOneAssetPaths.SteadyBaselineTalentPath,
            StarterPerformerDefaults.CappaTalentId => TheCircussyOneAssetPaths.CappaTalentPath,
            StarterPerformerDefaults.RubberGrinTalentId => TheCircussyOneAssetPaths.RubberGrinTalentPath,
            StarterPerformerDefaults.FaceForwardTalentId => TheCircussyOneAssetPaths.FaceForwardTalentPath,
            StarterPerformerDefaults.CapsulePopTalentId => TheCircussyOneAssetPaths.CapsulePopTalentPath,
            StarterPerformerDefaults.SampleSmileTalentId => TheCircussyOneAssetPaths.SampleSmileTalentPath,
            StarterPerformerDefaults.StrongmanTalentId => TheCircussyOneAssetPaths.StrongmanTalentPath,
            StarterPerformerDefaults.CannonBraceTalentId => TheCircussyOneAssetPaths.CannonBraceTalentPath,
            StarterPerformerDefaults.HeavyLiftTalentId => TheCircussyOneAssetPaths.HeavyLiftTalentPath,
            StarterPerformerDefaults.SteadyHandsTalentId => TheCircussyOneAssetPaths.SteadyHandsTalentPath,
            StarterPerformerDefaults.BulwarkStepTalentId => TheCircussyOneAssetPaths.BulwarkStepTalentPath,
            StarterPerformerDefaults.AcrobatTalentId => TheCircussyOneAssetPaths.AcrobatTalentPath,
            StarterPerformerDefaults.TightropeFootworkTalentId => TheCircussyOneAssetPaths.TightropeFootworkTalentPath,
            StarterPerformerDefaults.KnifeFlourishTalentId => TheCircussyOneAssetPaths.KnifeFlourishTalentPath,
            StarterPerformerDefaults.FastRecoveryTalentId => TheCircussyOneAssetPaths.FastRecoveryTalentPath,
            StarterPerformerDefaults.LongStrideTalentId => TheCircussyOneAssetPaths.LongStrideTalentPath,
            StarterPerformerDefaults.RingmasterTalentId => TheCircussyOneAssetPaths.RingmasterTalentPath,
            StarterPerformerDefaults.SpotlightCueTalentId => TheCircussyOneAssetPaths.SpotlightCueTalentPath,
            StarterPerformerDefaults.GrandEntranceTalentId => TheCircussyOneAssetPaths.GrandEntranceTalentPath,
            StarterPerformerDefaults.EncoreOddsTalentId => TheCircussyOneAssetPaths.EncoreOddsTalentPath,
            StarterPerformerDefaults.CommandingTempoTalentId => TheCircussyOneAssetPaths.CommandingTempoTalentPath,
            StarterPerformerDefaults.HoopFlowTalentId => TheCircussyOneAssetPaths.HoopFlowTalentPath,
            StarterPerformerDefaults.EmberStepTalentId => TheCircussyOneAssetPaths.EmberStepTalentPath,
            StarterPerformerDefaults.CloseCircleTalentId => TheCircussyOneAssetPaths.CloseCircleTalentPath,
            StarterPerformerDefaults.WarmApplauseTalentId => TheCircussyOneAssetPaths.WarmApplauseTalentPath,
            StarterPerformerDefaults.SecondCircleTalentId => TheCircussyOneAssetPaths.SecondCircleTalentPath,
            StarterPerformerDefaults.EncoreBounceTalentId => TheCircussyOneAssetPaths.EncoreBounceTalentPath,
            StarterPerformerDefaults.SoftCatchTalentId => TheCircussyOneAssetPaths.SoftCatchTalentPath,
            StarterPerformerDefaults.RicochetRhythmTalentId => TheCircussyOneAssetPaths.RicochetRhythmTalentPath,
            StarterPerformerDefaults.CaromLineTalentId => TheCircussyOneAssetPaths.CaromLineTalentPath,
            StarterPerformerDefaults.PackedBallsTalentId => TheCircussyOneAssetPaths.PackedBallsTalentPath,
            _ => TheCircussyOneAssetPaths.JugglerTalentPath
        };
    }

    private static GameObject GetOrCreateCappaModelPrefab()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(TheCircussyOneAssetPaths.CappaModelPrefabPath);
        if (prefab != null)
        {
            return prefab;
        }

        EnsureParentFolder(TheCircussyOneAssetPaths.CappaModelPrefabPath);
        Material playerMaterial = AssetDatabase.LoadAssetAtPath<Material>(TheCircussyOneAssetPaths.PlayerMaterialPath);
        Material faceMaterial = AssetDatabase.LoadAssetAtPath<Material>(TheCircussyOneAssetPaths.FaceMaterialPath);
        GameObject root = new GameObject("Devsample Cappa Model");
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(root.transform, false);
        body.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
        body.transform.localPosition = Vector3.up * body.transform.localScale.y;
        RemoveCollider(body);
        SetRendererMaterial(body, playerMaterial);
        AddPlaceholderFace(body.transform, faceMaterial);
        prefab = PrefabUtility.SaveAsPrefabAsset(root, TheCircussyOneAssetPaths.CappaModelPrefabPath);
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static GameObject GetAnimancerHumanoidPrefab()
    {
        return AssetDatabase.LoadAssetAtPath<GameObject>(TheCircussyOneAssetPaths.AnimancerHumanoidPrefabPath);
    }

    private static AnimationClip GetAnimancerHumanoidIdleClip()
    {
        return AssetDatabase.LoadAssetAtPath<AnimationClip>(TheCircussyOneAssetPaths.AnimancerHumanoidIdleClipPath);
    }

    private static AnimationClip GetAnimancerHumanoidRunClip()
    {
        return AssetDatabase.LoadAssetAtPath<AnimationClip>(TheCircussyOneAssetPaths.AnimancerHumanoidRunClipPath);
    }

    private static bool EnsureDevsampleSamVisualDefaults(PerformerDefinition performer)
    {
        if (performer == null)
        {
            return false;
        }

        bool changed = false;
        performer.modelTransform ??= new ActorModelTransformProfile();
        performer.animation ??= new PerformerAnimationProfile();

        if (performer.worldPrefab == null)
        {
            GameObject prefab = GetAnimancerHumanoidPrefab();
            if (prefab != null)
            {
                performer.worldPrefab = prefab;
                changed = true;
            }
        }

        if (performer.modelTransform.localScale == Vector3.one)
        {
            performer.modelTransform.localScale = Vector3.one * 1.85f;
            changed = true;
        }

        if (performer.animation.idle == null)
        {
            AnimationClip idle = GetAnimancerHumanoidIdleClip();
            if (idle != null)
            {
                performer.animation.idle = idle;
                changed = true;
            }
        }

        if (performer.animation.run == null)
        {
            AnimationClip run = GetAnimancerHumanoidRunClip();
            if (run != null)
            {
                performer.animation.run = run;
                changed = true;
            }
        }

        if (performer.animation.runThreshold01 <= 0.08f)
        {
            performer.animation.runThreshold01 = 0.2f;
            changed = true;
        }

        if (performer.animation.runExitThreshold01 <= 0f)
        {
            performer.animation.runExitThreshold01 = 0.06f;
            changed = true;
        }

        changed |= performer.EnsureWorkflowDefaults();
        return changed;
    }

    private static void AddPlaceholderFace(Transform body, Material faceMaterial)
    {
        if (body == null)
        {
            return;
        }

        var face = new GameObject("Face");
        face.transform.SetParent(body, false);
        CreateFacePrimitive(face.transform, PrimitiveType.Sphere, "Left Eye", new Vector3(-0.18f, 0.32f, 0.48f), Quaternion.identity, Vector3.one * 0.13f, faceMaterial);
        CreateFacePrimitive(face.transform, PrimitiveType.Sphere, "Right Eye", new Vector3(0.18f, 0.32f, 0.48f), Quaternion.identity, Vector3.one * 0.13f, faceMaterial);
        CreateFacePrimitive(face.transform, PrimitiveType.Capsule, "Mouth", new Vector3(0f, 0.08f, 0.51f), Quaternion.Euler(0f, 0f, 90f), new Vector3(0.045f, 0.16f, 0.045f), faceMaterial);
    }

    private static void CreateFacePrimitive(Transform parent, PrimitiveType primitive, string name, Vector3 localPosition, Quaternion localRotation, Vector3 localScale, Material material)
    {
        GameObject part = GameObject.CreatePrimitive(primitive);
        part.name = name;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPosition;
        part.transform.localRotation = localRotation;
        part.transform.localScale = localScale;
        RemoveCollider(part);
        SetRendererMaterial(part, material);
    }

    private static void SetRendererMaterial(GameObject gameObject, Material material)
    {
        if (gameObject == null || material == null)
        {
            return;
        }

        foreach (Renderer renderer in gameObject.GetComponentsInChildren<Renderer>(true))
        {
            renderer.sharedMaterial = material;
        }
    }

    private static void RemoveCollider(GameObject gameObject)
    {
        if (gameObject == null)
        {
            return;
        }

        foreach (Collider collider in gameObject.GetComponentsInChildren<Collider>(true))
        {
            Object.DestroyImmediate(collider);
        }
    }

    public static GridVisualConfig GetOrCreateGridVisualConfig()
    {
        GridVisualConfig config = AssetDatabase.LoadAssetAtPath<GridVisualConfig>(TheCircussyOneAssetPaths.GridVisualConfigPath);
        if (config != null)
        {
            MarkDirtyIf(config, config.EnsureLightingDefaults());
            return config;
        }

        config = CreateAsset<GridVisualConfig>(TheCircussyOneAssetPaths.GridVisualConfigPath);
        config.minorSpacing = 1f;
        config.majorSpacing = 5f;
        config.minorLineWidth = 0.008f;
        config.majorLineWidth = 0.028f;
        config.cellsPerTexture = 10;
        config.textureResolution = 256;
        config.actorMinorSpacing = 0.18f;
        config.projectileMinorSpacing = 0.06f;
        config.pickupMinorSpacing = 0.08f;
        config.floorBaseColor = new Color(0.2f, 0.31f, 0.34f, 1f);
        config.floorAlternateColor = new Color(0.27f, 0.39f, 0.42f, 1f);
        config.minorLineColor = new Color(0.62f, 0.78f, 0.82f, 0.1f);
        config.majorLineColor = new Color(0.86f, 0.93f, 0.94f, 0.36f);
        config.floorGridStrength = 0.18f;
        config.actorGridStrength = 0.04f;
        config.projectileGridStrength = 0.18f;
        config.pickupGridStrength = 0.2f;
        config.floorTileStrength = 0.58f;
        config.actorTileStrength = 0.72f;
        config.projectileTileStrength = 0.08f;
        config.pickupTileStrength = 0.1f;
        config.noiseStrength = 0.025f;
        config.EnsureLightingDefaults();
        EditorUtility.SetDirty(config);
        return config;
    }

    public static LightingVisualConfig GetOrCreateLightingVisualConfig()
    {
        return LoadOrCreateWithoutDefaultMutation<LightingVisualConfig>(TheCircussyOneAssetPaths.LightingVisualConfigPath);
    }

    public static DamageFeedbackVisualConfig GetOrCreateDamageFeedbackVisualConfig()
    {
        return LoadOrCreateWithEnsure<DamageFeedbackVisualConfig>(
            TheCircussyOneAssetPaths.DamageFeedbackVisualConfigPath,
            config => config.EnsureReadableDefaults());
    }

    public static ActorMotionVisualConfig GetOrCreateActorMotionVisualConfig()
    {
        return LoadOrCreateWithEnsure<ActorMotionVisualConfig>(
            TheCircussyOneAssetPaths.ActorMotionVisualConfigPath,
            config => config.EnsureWorkflowDefaults());
    }

    public static EnemySpawnVisualConfig GetOrCreateEnemySpawnVisualConfig()
    {
        return LoadOrCreateWithEnsure<EnemySpawnVisualConfig>(
            TheCircussyOneAssetPaths.EnemySpawnVisualConfigPath,
            config => config.EnsureWorkflowDefaults());
    }

    public static VfxVisualConfig GetOrCreateVfxVisualConfig()
    {
        return LoadOrCreateWithEnsure<VfxVisualConfig>(
            TheCircussyOneAssetPaths.VfxVisualConfigPath,
            config => config.EnsureWorkflowDefaults());
    }

    public static GameHapticsConfig GetOrCreateGameHapticsConfig()
    {
        return LoadOrCreateWithEnsure<GameHapticsConfig>(
            TheCircussyOneAssetPaths.GameHapticsConfigPath,
            config => config.EnsureWorkflowDefaults());
    }

    public static HudVisualConfig GetOrCreateHudVisualConfig()
    {
        return LoadOrCreateWithEnsure<HudVisualConfig>(
            TheCircussyOneAssetPaths.HudVisualConfigPath,
            config => config.EnsureReadableDefaults());
    }

    public static XpGainCounterVisualConfig GetOrCreateXpGainCounterVisualConfig()
    {
        XpGainCounterVisualConfig config = AssetDatabase.LoadAssetAtPath<XpGainCounterVisualConfig>(TheCircussyOneAssetPaths.XpGainCounterVisualConfigPath);
        if (config != null)
        {
            MarkDirtyIf(config, config.EnsureWorkflowDefaults());
            return config;
        }

        config = CreateAsset<XpGainCounterVisualConfig>(TheCircussyOneAssetPaths.XpGainCounterVisualConfigPath);
        HudVisualConfig hudConfig = GetOrCreateHudVisualConfig();
        config.textColor = hudConfig != null ? hudConfig.xpFillColor : config.textColor;
        config.EnsureWorkflowDefaults();
        EditorUtility.SetDirty(config);
        return config;
    }

    public static WorldInteractionPromptVisualConfig GetOrCreateWorldInteractionPromptVisualConfig()
    {
        WorldInteractionPromptVisualConfig config = AssetDatabase.LoadAssetAtPath<WorldInteractionPromptVisualConfig>(TheCircussyOneAssetPaths.WorldInteractionPromptVisualConfigPath);
        if (config != null)
        {
            MarkDirtyIf(config, config.EnsureWorkflowDefaults());
            return config;
        }

        config = CreateAsset<WorldInteractionPromptVisualConfig>(TheCircussyOneAssetPaths.WorldInteractionPromptVisualConfigPath);
        XpGainCounterVisualConfig counterConfig = GetOrCreateXpGainCounterVisualConfig();
        if (counterConfig != null)
        {
            config.font = counterConfig.font;
            config.sdfGradientScale = counterConfig.sdfGradientScale;
            config.sdfSharpness = counterConfig.sdfSharpness;
            config.textOutlineColor = counterConfig.outlineColor;
        }

        config.EnsureWorkflowDefaults();
        EditorUtility.SetDirty(config);
        return config;
    }

    private static T LoadOrCreateWithEnsure<T>(string path, System.Func<T, bool> ensure)
        where T : ScriptableObject
    {
        T config = AssetDatabase.LoadAssetAtPath<T>(path);
        if (config != null)
        {
            MarkDirtyIf(config, ensure(config));
            return config;
        }

        config = CreateAsset<T>(path);
        ensure(config);
        EditorUtility.SetDirty(config);
        return config;
    }

    private static T LoadOrCreateWithoutDefaultMutation<T>(string path)
        where T : ScriptableObject
    {
        T config = AssetDatabase.LoadAssetAtPath<T>(path);
        if (config != null)
        {
            return config;
        }

        config = CreateAsset<T>(path);
        EditorUtility.SetDirty(config);
        return config;
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

    private static void SetDirtyAndSave(Object asset)
    {
        if (asset == null)
        {
            return;
        }

        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();
    }

    private static XpGemDefinition GetOrCreateXpGem(string path, string id, string displayName, int amount, Color color, ContentRarity rarity, float visualScale)
    {
        XpGemDefinition gem = AssetDatabase.LoadAssetAtPath<XpGemDefinition>(path);
        if (gem != null)
        {
            bool changed = gem.EnsureWorkflowDefaults();
            MarkDirtyIf(gem, changed);
            return gem;
        }

        gem = CreateAsset<XpGemDefinition>(path);
        gem.ApplyDefaults(id, displayName, amount, color, rarity, visualScale);
        EditorUtility.SetDirty(gem);
        return gem;
    }

    private static HealthPickupDefinition GetOrCreateHealthPickup(string path, string id, string displayName, string description, int amount, Color color, float visualScale)
    {
        HealthPickupDefinition pickup = AssetDatabase.LoadAssetAtPath<HealthPickupDefinition>(path);
        if (pickup != null)
        {
            bool changed = pickup.EnsureWorkflowDefaults();
            MarkDirtyIf(pickup, changed);
            return pickup;
        }

        pickup = CreateAsset<HealthPickupDefinition>(path);
        pickup.ApplyDefaults(id, displayName, description, amount, color, visualScale);
        EditorUtility.SetDirty(pickup);
        return pickup;
    }

    private static HealingPropDefinition GetOrCreateHealingProp(
        string path,
        string id,
        string displayName,
        string description,
        float holdSeconds,
        string promptText,
        HealthPickupDefinition pickup,
        int minPickupCount,
        int maxPickupCount,
        Color visualColor,
        float visualScale)
    {
        HealingPropDefinition prop = AssetDatabase.LoadAssetAtPath<HealingPropDefinition>(path);
        if (prop != null)
        {
            bool changed = prop.EnsureWorkflowDefaults();
            MarkDirtyIf(prop, changed);
            return prop;
        }

        prop = CreateAsset<HealingPropDefinition>(path);
        prop.ApplyDefaults(id, displayName, description, holdSeconds, promptText, pickup, minPickupCount, maxPickupCount, visualColor, visualScale);
        EditorUtility.SetDirty(prop);
        return prop;
    }

    private static void ApplyOpeningHeadlinerEnemyDefaults(EnemyDefinition enemy, EnemyDefinition normalEnemy)
    {
        if (enemy == null)
        {
            return;
        }

        enemy.enemyId = "opening_headliner_enemy";
        enemy.displayName = "Opening Headliner Actor";
        enemy.rarity = ContentRarity.Rare;
        enemy.tags = ContentTagSet.With(ContentTag.Boss, ContentTag.Elite, ContentTag.Physical);
        enemy.isActive = true;
        enemy.behaviorType = normalEnemy != null ? normalEnemy.behaviorType : EnemyBehaviorType.Chaser;
        enemy.spawnWeight = 0f;
        enemy.baseHealth = Mathf.Max(normalEnemy != null ? normalEnemy.baseHealth * 16 : 960, 960);
        enemy.healthGrowthPerMinute = Mathf.Max(normalEnemy != null ? normalEnemy.healthGrowthPerMinute * 8f : 80f, 0f);
        enemy.baseMoveSpeed = normalEnemy != null ? Mathf.Max(0.1f, normalEnemy.baseMoveSpeed * 0.85f) : 2f;
        enemy.moveSpeedGrowthPerMinute = normalEnemy != null ? normalEnemy.moveSpeedGrowthPerMinute : 0f;
        enemy.turnDegreesPerSecond = normalEnemy != null ? normalEnemy.turnDegreesPerSecond : 360f;
        enemy.contactDamage = Mathf.Max(normalEnemy != null ? normalEnemy.contactDamage * 3 : 8, 8);
        enemy.xpBudget = 0;
        enemy.dropStyle = XpDropStyle.Compact;
        enemy.body = normalEnemy != null && normalEnemy.body != null ? normalEnemy.body.Copy() : enemy.body;
        enemy.locomotion = normalEnemy != null && normalEnemy.locomotion != null ? normalEnemy.locomotion.Copy() : enemy.locomotion;
        enemy.climb = normalEnemy != null && normalEnemy.climb != null ? normalEnemy.climb.Copy() : enemy.climb;
        enemy.stack = normalEnemy != null && normalEnemy.stack != null ? normalEnemy.stack.Copy() : enemy.stack;
        if (enemy.body != null)
        {
            enemy.body.hurtboxRadius = Mathf.Max(enemy.body.hurtboxRadius, 1f);
            enemy.body.hurtboxHeightOffset = Mathf.Max(enemy.body.hurtboxHeightOffset, 1.2f);
            enemy.body.contactHitboxSize = new Vector3(
                Mathf.Max(enemy.body.contactHitboxSize.x, 1.2f),
                Mathf.Max(enemy.body.contactHitboxSize.y, 1.2f),
                Mathf.Max(enemy.body.contactHitboxSize.z, 1.2f));
            enemy.body.movementBodyRadius = Mathf.Max(enemy.body.movementBodyRadius, 0.8f);
            enemy.body.movementBodyHeight = Mathf.Max(enemy.body.movementBodyHeight, 1.8f);
        }

        if (enemy.stack != null)
        {
            enemy.stack.policy = EnemyStackPolicy.GroundOnly;
            enemy.stack.canClimbEnemies = false;
            enemy.stack.canBeStackedOn = false;
        }

        enemy.EnsureWorkflowDefaults();
    }

    private static void ApplyGameConfigDefaults(GameConfig config)
    {
        config.playerMoveSpeed = 8f;
        config.playerMaxHealth = 100;
        config.contactDamageCooldown = 1f;
        config.playerInputDeadzone = 0.12f;
        config.playerWalkSpeed = 3.5f;
        config.playerRunSpeed = 11.5f;
        config.playerAcceleration = 38f;
        config.playerDeceleration = 30f;
        config.playerTurnAcceleration = 24f;
        config.playerReverseSkidAngle = 125f;
        config.playerSkidFriction = 42f;
        config.playerRotationSharpness = 16f;
        config.arenaRadius = 58f;
        config.enemySpawnRadius = 42f;
        config.spawnIntervalSeconds = 1.25f;
        config.minSpawnIntervalSeconds = 0.22f;
        config.spawnRampDurationSeconds = 300f;
        config.maxEnemies = 30;
        config.maxEnemiesAtPeak = 170;
        config.enemyHealth = 170;
        config.enemyHealthGrowthPerMinute = 20f;
        config.enemyMoveSpeed = 2f;
        config.enemyMoveSpeedGrowthPerMinute = 0.24f;
        config.enemySeparationRadius = 1.15f;
        config.enemyPlayerStopDistance = 1.25f;
        config.enemySeparationWeight = 1.65f;
        config.enemyHurtboxRadius = 0.65f;
        config.enemyHurtboxHeightOffset = 0.95f;
        config.enemyContactHitboxSize = new Vector3(0.75f, 0.9f, 0.65f);
        config.enemyContactHitboxOffset = new Vector3(0f, 0.55f, 0.35f);
        config.enemyMovementBodyRadius = 0.52f;
        config.enemyMovementBodyHeight = 1.45f;
        config.enemyMovementBodyOffset = new Vector3(0f, 0.75f, 0f);
        config.enemyBodySeparationIterations = 2;
        config.playerEnemyBlockMaxPushPerFrame = 0.75f;
        config.enemyEnemyBlockMaxPushPerPair = 0.35f;
        config.enemyContactDamage = 2;
        config.pickupMagnetRadius = 7.5f;
        config.pickupCollectRadius = 1.2f;
        config.projectileColor = new Color(1f, 0.88f, 0.18f, 1f);
        config.pickupColor = new Color(0.27f, 0.96f, 1f, 1f);
        config.enemyColor = new Color(0.78f, 0.12f, 0.18f, 1f);
        config.playerColor = new Color(0.38f, 0.2f, 0.92f, 1f);
    }
}
