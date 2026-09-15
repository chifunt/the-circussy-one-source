using System.Collections.Generic;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using TheCircussyOne.Stats;
using UnityEngine;

namespace TheCircussyOne.DI
{
    internal sealed class GameFallbackCatalogFactory
    {
        private readonly GameConfig gameConfig;

        public GameFallbackCatalogFactory(GameConfig gameConfig)
        {
            this.gameConfig = gameConfig;
        }

        public WeaponCatalog CreateWeaponCatalog()
        {
            var definition = ScriptableObject.CreateInstance<WeaponDefinition>();
            definition.hideFlags = HideFlags.DontSave;
            definition.ApplyDefaults(FirstPartyWeaponDefaults.DefaultWeapon);

            var catalog = ScriptableObject.CreateInstance<WeaponCatalog>();
            catalog.hideFlags = HideFlags.DontSave;
            catalog.startingWeapons.Add(definition);
            catalog.availableWeapons.Add(definition);
            return catalog;
        }

        public ItemCatalog CreateItemCatalog()
        {
            var item = ScriptableObject.CreateInstance<ItemDefinition>();
            item.hideFlags = HideFlags.DontSave;
            item.ApplyDefaults(
                "rubber_soles",
                "Rubber Soles",
                "Move faster.",
                ContentRarity.Common,
                ContentTagSet.With(ContentTag.Movement, ContentTag.Utility),
                ItemStackPolicy.StackLinear,
                5,
                new Color(0.92f, 0.95f, 0.82f, 1f),
                new ItemStatModifierDefinition(StatId.PlayerMoveSpeedMultiplier, StatModifierBucket.AdditivePercent, 0.08f));

            var catalog = ScriptableObject.CreateInstance<ItemCatalog>();
            catalog.hideFlags = HideFlags.DontSave;
            catalog.items.Add(item);
            return catalog;
        }

        public TicketDepositCatalog CreateTicketDepositCatalog()
        {
            TicketDepositDefinition small = CreateTicketDeposit(
                "small_ticket_stack",
                "Small Ticket Stack",
                "A quick bundle of loose prize tickets.",
                TicketDepositTier.Small,
                1f,
                5,
                8,
                TicketDepositPayoutMode.OnComplete,
                1,
                0.75f);
            TicketDepositDefinition medium = CreateTicketDeposit(
                "ticket_roll",
                "Ticket Roll",
                "A cranky roll worth holding your ground for.",
                TicketDepositTier.Medium,
                2.5f,
                18,
                25,
                TicketDepositPayoutMode.Chunked,
                3,
                1f);
            TicketDepositDefinition large = CreateTicketDeposit(
                "jackpot_cache",
                "Jackpot Cache",
                "A loud little payout box with enough Tickets to tempt trouble.",
                TicketDepositTier.Large,
                4f,
                45,
                60,
                TicketDepositPayoutMode.Chunked,
                4,
                1.25f);

            var catalog = ScriptableObject.CreateInstance<TicketDepositCatalog>();
            catalog.hideFlags = HideFlags.DontSave;
            catalog.deposits.Add(small);
            catalog.deposits.Add(medium);
            catalog.deposits.Add(large);
            return catalog;
        }

        public WorldRewardPlacementCatalog CreateWorldRewardPlacementCatalog()
        {
            var catalog = ScriptableObject.CreateInstance<WorldRewardPlacementCatalog>();
            catalog.hideFlags = HideFlags.DontSave;
            catalog.EnsureWorkflowDefaults(CreateWorldRewardPlacements());
            return catalog;
        }

        public WorldDecorationCatalog CreateWorldDecorationCatalog()
        {
            var catalog = ScriptableObject.CreateInstance<WorldDecorationCatalog>();
            catalog.hideFlags = HideFlags.DontSave;
            return catalog;
        }

        public ChestCatalog CreateChestCatalog()
        {
            ChestDefinition open = CreateChest(
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
            ChestDefinition locked = CreateChest(
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
            ChestDefinition premium = CreateChest(
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
            ChestDefinition special = CreateChest(
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

            var catalog = ScriptableObject.CreateInstance<ChestCatalog>();
            catalog.hideFlags = HideFlags.DontSave;
            catalog.chests.Add(open);
            catalog.chests.Add(locked);
            catalog.chests.Add(premium);
            catalog.chests.Add(special);
            return catalog;
        }

        public XpGemCatalog CreateXpGemCatalog()
        {
            var gem = ScriptableObject.CreateInstance<XpGemDefinition>();
            gem.hideFlags = HideFlags.DontSave;
            gem.ApplyDefaults(
                "blue_xp_gem",
                "Blue XP Gem",
                XpGemDefinition.DefaultBlueXpAmount,
                gameConfig != null ? gameConfig.pickupColor : new Color(0.27f, 0.96f, 1f, 1f),
                ContentRarity.Common,
                1f);

            var catalog = ScriptableObject.CreateInstance<XpGemCatalog>();
            catalog.hideFlags = HideFlags.DontSave;
            catalog.gems.Add(gem);
            catalog.defaultEnemyXpBudget = XpGemDefinition.DefaultBlueXpAmount;
            return catalog;
        }

        public HealthPickupCatalog CreateHealthPickupCatalog()
        {
            var pickup = ScriptableObject.CreateInstance<HealthPickupDefinition>();
            pickup.hideFlags = HideFlags.DontSave;
            pickup.ApplyDefaults(
                "treat",
                "Treat",
                "A circus treat that patches up a rough act.",
                HealthPickupDefinition.DefaultHealAmount,
                new Color(1f, 0.2f, 0.42f, 1f),
                1.05f,
                HealthPickupDefinition.DefaultHealPercentOfMaxHealth);

            var catalog = ScriptableObject.CreateInstance<HealthPickupCatalog>();
            catalog.hideFlags = HideFlags.DontSave;
            catalog.pickups.Add(pickup);
            catalog.defaultHealthPickup = pickup;
            return catalog;
        }

        public HealingPropCatalog CreateHealingPropCatalog(HealthPickupCatalog activeHealthPickupCatalog)
        {
            HealthPickupDefinition pickup = activeHealthPickupCatalog != null
                ? activeHealthPickupCatalog.ResolveDefault()
                : null;
            var snackBox = ScriptableObject.CreateInstance<HealingPropDefinition>();
            snackBox.hideFlags = HideFlags.DontSave;
            snackBox.ApplyDefaults(
                "snack_box",
                "Snack Box",
                "A quick little snack box for patching up between swarms.",
                1f,
                "Grab a Snack",
                pickup,
                1,
                1,
                new Color(1f, 0.24f, 0.42f, 1f),
                0.85f);

            var snackCart = ScriptableObject.CreateInstance<HealingPropDefinition>();
            snackCart.hideFlags = HideFlags.DontSave;
            snackCart.ApplyDefaults(
                "snack_cart",
                "Snack Cart",
                "A snack cart with enough treats to risk a longer rummage.",
                2.25f,
                "Grab a Snack",
                pickup,
                2,
                3,
                new Color(1f, 0.58f, 0.18f, 1f),
                1.15f);

            var catalog = ScriptableObject.CreateInstance<HealingPropCatalog>();
            catalog.hideFlags = HideFlags.DontSave;
            catalog.props.Add(snackBox);
            catalog.props.Add(snackCart);
            return catalog;
        }

        public static WorldRewardPlacementDefinition[] CreateWorldRewardPlacements()
        {
            return WorldRewardPlacementDefaults.CreatePrototypePlacements();
        }

        public UpgradeCatalog CreateUpgradeCatalog()
        {
            var catalog = ScriptableObject.CreateInstance<UpgradeCatalog>();
            catalog.hideFlags = HideFlags.DontSave;
            WeaponDefinition defaultWeapon = CreateWeaponCatalog().startingWeapons[0];
            UpgradeDefinition jugglingBallTuning = ScriptableObject.CreateInstance<UpgradeDefinition>();
            jugglingBallTuning.hideFlags = HideFlags.DontSave;
            jugglingBallTuning.ApplyWeaponStatUpgradeDefaults(
                defaultWeapon,
                "bounce",
                "Bounce",
                "Add one Juggling Ball ricochet.",
                Color.white,
                new UpgradeStatModifierDefinition(StatId.WeaponBounce, StatModifierBucket.Flat, 1f));
            catalog.upgrades.Add(jugglingBallTuning);

            UpgradeDefinition jugglingBallTiming = ScriptableObject.CreateInstance<UpgradeDefinition>();
            jugglingBallTiming.hideFlags = HideFlags.DontSave;
            jugglingBallTiming.ApplyWeaponStatUpgradeDefaults(
                defaultWeapon,
                "timing",
                "Timing",
                "Reduce Juggling Ball aim error.",
                Color.white,
                new UpgradeStatModifierDefinition(StatId.WeaponAccuracyMultiplier, StatModifierBucket.AdditivePercent, 0.08f));
            catalog.upgrades.Add(jugglingBallTiming);

            catalog.levelUpRarityWeights = ContentRarityWeightTable.LevelUpDefault();
            return catalog;
        }

        public TalentCatalog CreateTalentCatalog()
        {
            var catalog = ScriptableObject.CreateInstance<TalentCatalog>();
            catalog.hideFlags = HideFlags.DontSave;
            for (int i = 0; i < StarterTalentDefaults.AllIds.Length; i++)
            {
                TalentDefinition talent = StarterTalentDefaults.Create(StarterTalentDefaults.AllIds[i]);
                talent.hideFlags = HideFlags.DontSave;
                catalog.talents.Add(talent);
            }

            for (int i = 0; i < StarterPerformerDefaults.TalentIds.Length; i++)
            {
                var talent = ScriptableObject.CreateInstance<TalentDefinition>();
                talent.hideFlags = HideFlags.DontSave;
                StarterPerformerDefaults.ApplyTalent(
                    talent,
                    StarterPerformerDefaults.TalentIds[i],
                    StarterPerformerDefaults.PerformerIdForTalentId(StarterPerformerDefaults.TalentIds[i]));
                catalog.talents.Add(talent);
            }

            return catalog;
        }

        public PerformerCatalog CreatePerformerCatalog(WeaponCatalog activeWeaponCatalog)
        {
            WeaponDefinition jugglingBall = FindWeapon(activeWeaponCatalog, "juggling_ball");
            WeaponDefinition cannon = FindWeapon(activeWeaponCatalog, "cannon") ?? jugglingBall;
            WeaponDefinition knifeFan = FindWeapon(activeWeaponCatalog, "knife_fan") ?? jugglingBall;
            WeaponDefinition spotlightBolt = FindWeapon(activeWeaponCatalog, "spotlight_bolt") ?? jugglingBall;
            WeaponDefinition fireHoop = FindWeapon(activeWeaponCatalog, "fire_hoop") ?? jugglingBall;

            var catalog = ScriptableObject.CreateInstance<PerformerCatalog>();
            catalog.hideFlags = HideFlags.DontSave;
            for (int i = 0; i < StarterPerformerDefaults.PerformerIds.Length; i++)
            {
                string performerId = StarterPerformerDefaults.PerformerIds[i];
                var performer = ScriptableObject.CreateInstance<PerformerDefinition>();
                performer.hideFlags = HideFlags.DontSave;
                StarterPerformerDefaults.ApplyPerformer(performer, performerId, cannon, knifeFan, spotlightBolt, fireHoop, jugglingBall);
                catalog.performers.Add(performer);
            }

            catalog.defaultPerformer = catalog.performers.Count > 0 ? catalog.performers[0] : null;
            return catalog;
        }

        public EnemyCatalog CreateEnemyCatalog(XpGemCatalog activeXpGemCatalog)
        {
            EnemyDefinition definition = EnemyDefinition.CreateRuntimeFallback(gameConfig, activeXpGemCatalog);

            var catalog = ScriptableObject.CreateInstance<EnemyCatalog>();
            catalog.hideFlags = HideFlags.DontSave;
            catalog.enemies.Add(definition);
            return catalog;
        }

        public HeadlinerCatalog CreateHeadlinerCatalog(EnemyCatalog activeEnemyCatalog)
        {
            EnemyDefinition actor = CreateHeadlinerActor(activeEnemyCatalog);
            var headliner = ScriptableObject.CreateInstance<HeadlinerDefinition>();
            headliner.hideFlags = HideFlags.DontSave;
            headliner.ApplyDefaults(actor);

            var catalog = ScriptableObject.CreateInstance<HeadlinerCatalog>();
            catalog.hideFlags = HideFlags.DontSave;
            catalog.headliners.Add(headliner);
            return catalog;
        }

        internal static WeaponDefinition FindWeapon(WeaponCatalog catalog, string weaponId)
        {
            if (catalog == null || string.IsNullOrWhiteSpace(weaponId))
            {
                return null;
            }

            WeaponDefinition weapon = FindWeapon(catalog.StartingWeapons, weaponId);
            return weapon != null ? weapon : FindWeapon(catalog.AvailableWeapons, weaponId);
        }

        private static WeaponDefinition FindWeapon(IReadOnlyList<WeaponDefinition> weapons, string weaponId)
        {
            if (weapons == null)
            {
                return null;
            }

            for (int i = 0; i < weapons.Count; i++)
            {
                WeaponDefinition weapon = weapons[i];
                if (ContentAvailabilityRules.IsActiveAndValid(weapon) && weapon.Id == weaponId)
                {
                    return weapon;
                }
            }

            return null;
        }

        private static ChestDefinition CreateChest(
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
            var definition = ScriptableObject.CreateInstance<ChestDefinition>();
            definition.hideFlags = HideFlags.DontSave;
            definition.ApplyDefaults(
                id,
                displayName,
                description,
                kind,
                holdSeconds,
                ticketCost,
                promptText,
                rarityWeights,
                visualColor,
                visualScale);
            return definition;
        }

        private static TicketDepositDefinition CreateTicketDeposit(
            string id,
            string displayName,
            string description,
            TicketDepositTier tier,
            float holdSeconds,
            int minTickets,
            int maxTickets,
            TicketDepositPayoutMode payoutMode,
            int chunks,
            float visualScale)
        {
            var definition = ScriptableObject.CreateInstance<TicketDepositDefinition>();
            definition.hideFlags = HideFlags.DontSave;
            definition.ApplyDefaults(
                id,
                displayName,
                description,
                tier,
                holdSeconds,
                minTickets,
                maxTickets,
                payoutMode,
                chunks,
                "Collect Tickets",
                new Color(1f, 0.78f, 0.16f, 1f),
                visualScale);
            return definition;
        }

        private EnemyDefinition CreateHeadlinerActor(EnemyCatalog activeEnemyCatalog)
        {
            EnemyDefinition baseline = activeEnemyCatalog != null ? activeEnemyCatalog.DefaultEnemy : null;
            EnemyDefinition actor = EnemyDefinition.CreateRuntimeFallback(gameConfig);
            actor.enemyId = "opening_headliner_enemy";
            actor.displayName = "Opening Headliner Actor";
            actor.rarity = ContentRarity.Rare;
            actor.tags = ContentTagSet.With(ContentTag.Boss, ContentTag.Elite, ContentTag.Physical);
            actor.behaviorType = baseline != null ? baseline.behaviorType : EnemyBehaviorType.Chaser;
            actor.spawnWeight = 0f;
            actor.baseHealth = Mathf.Max(baseline != null ? baseline.baseHealth * 16 : 960, 960);
            actor.healthGrowthPerMinute = Mathf.Max(baseline != null ? baseline.healthGrowthPerMinute * 8f : 80f, 0f);
            actor.baseMoveSpeed = baseline != null ? Mathf.Max(0.1f, baseline.baseMoveSpeed * 0.85f) : 2f;
            actor.moveSpeedGrowthPerMinute = baseline != null ? baseline.moveSpeedGrowthPerMinute : 0f;
            actor.turnDegreesPerSecond = baseline != null ? baseline.turnDegreesPerSecond : 360f;
            actor.contactDamage = Mathf.Max(baseline != null ? baseline.contactDamage * 3 : 8, 8);
            actor.xpBudget = 0;
            actor.dropStyle = XpDropStyle.Compact;
            actor.body = baseline != null && baseline.body != null ? baseline.body.Copy() : actor.body;
            actor.locomotion = baseline != null && baseline.locomotion != null ? baseline.locomotion.Copy() : actor.locomotion;
            actor.climb = baseline != null && baseline.climb != null ? baseline.climb.Copy() : actor.climb;
            actor.stack = baseline != null && baseline.stack != null ? baseline.stack.Copy() : actor.stack;
            if (actor.body != null)
            {
                actor.body.hurtboxRadius = Mathf.Max(actor.body.hurtboxRadius, 1f);
                actor.body.hurtboxHeightOffset = Mathf.Max(actor.body.hurtboxHeightOffset, 1.2f);
                actor.body.contactHitboxSize = new Vector3(
                    Mathf.Max(actor.body.contactHitboxSize.x, 1.2f),
                    Mathf.Max(actor.body.contactHitboxSize.y, 1.2f),
                    Mathf.Max(actor.body.contactHitboxSize.z, 1.2f));
                actor.body.movementBodyRadius = Mathf.Max(actor.body.movementBodyRadius, 0.8f);
                actor.body.movementBodyHeight = Mathf.Max(actor.body.movementBodyHeight, 1.8f);
            }

            if (actor.stack != null)
            {
                actor.stack.policy = EnemyStackPolicy.GroundOnly;
                actor.stack.canClimbEnemies = false;
                actor.stack.canBeStackedOn = false;
            }

            actor.EnsureWorkflowDefaults();
            return actor;
        }
    }
}
