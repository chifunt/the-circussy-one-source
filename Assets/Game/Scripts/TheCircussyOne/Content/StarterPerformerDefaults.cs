using System.Collections.Generic;
using UnityEngine;
using TheCircussyOne.Stats;

namespace TheCircussyOne.Content
{
    public static class StarterPerformerDefaults
    {
        public const string DevsampleSamId = "devsample_sam_the_dev_sample";
        public const string JugglerId = DevsampleSamId;
        public const string DevsampleCappaId = "devsample_cappa";
        public const string StrongmanId = "bruno_the_cannon_strongman";
        public const string AcrobatId = "mira_the_knife_acrobat";
        public const string RingmasterId = "vox_the_spotlight_ringmaster";
        public const string SolaHoopDancerId = "sola_the_fire_hoop_dancer";
        public const string BibiBallJugglerId = "bibi_the_ball_juggler";

        public const string JugglerTalentId = "center_ring_toss";
        public const string CleanCompileTalentId = "clean_compile";
        public const string SampleArcTalentId = "sample_arc";
        public const string DebugMarkerTalentId = "debug_marker";
        public const string SteadyBaselineTalentId = "steady_baseline";

        public const string CappaTalentId = "capsule_toss";
        public const string RubberGrinTalentId = "rubber_grin";
        public const string FaceForwardTalentId = "face_forward";
        public const string CapsulePopTalentId = "capsule_pop";
        public const string SampleSmileTalentId = "sample_smile";

        public const string StrongmanTalentId = "iron_posture";
        public const string CannonBraceTalentId = "cannon_brace";
        public const string HeavyLiftTalentId = "heavy_lift";
        public const string SteadyHandsTalentId = "steady_hands";
        public const string BulwarkStepTalentId = "bulwark_step";

        public const string AcrobatTalentId = "aerial_tempo";
        public const string TightropeFootworkTalentId = "tightrope_footwork";
        public const string KnifeFlourishTalentId = "knife_flourish";
        public const string FastRecoveryTalentId = "fast_recovery";
        public const string LongStrideTalentId = "long_stride";

        public const string RingmasterTalentId = "packed_house";
        public const string SpotlightCueTalentId = "spotlight_cue";
        public const string GrandEntranceTalentId = "grand_entrance";
        public const string EncoreOddsTalentId = "encore_odds";
        public const string CommandingTempoTalentId = "commanding_tempo";

        public const string HoopFlowTalentId = "hoop_flow";
        public const string EmberStepTalentId = "ember_step";
        public const string CloseCircleTalentId = "close_circle";
        public const string WarmApplauseTalentId = "warm_applause";
        public const string SecondCircleTalentId = "second_circle";

        public const string EncoreBounceTalentId = "encore_bounce";
        public const string SoftCatchTalentId = "soft_catch";
        public const string RicochetRhythmTalentId = "ricochet_rhythm";
        public const string CaromLineTalentId = "carom_line";
        public const string PackedBallsTalentId = "packed_balls";

        public static readonly string[] PerformerIds =
        {
            JugglerId,
            DevsampleCappaId,
            StrongmanId,
            AcrobatId,
            RingmasterId,
            SolaHoopDancerId,
            BibiBallJugglerId
        };

        public static readonly string[] TalentIds =
        {
            JugglerTalentId,
            CleanCompileTalentId,
            SampleArcTalentId,
            DebugMarkerTalentId,
            SteadyBaselineTalentId,
            CappaTalentId,
            RubberGrinTalentId,
            FaceForwardTalentId,
            CapsulePopTalentId,
            SampleSmileTalentId,
            StrongmanTalentId,
            CannonBraceTalentId,
            HeavyLiftTalentId,
            SteadyHandsTalentId,
            BulwarkStepTalentId,
            AcrobatTalentId,
            TightropeFootworkTalentId,
            KnifeFlourishTalentId,
            FastRecoveryTalentId,
            LongStrideTalentId,
            RingmasterTalentId,
            SpotlightCueTalentId,
            GrandEntranceTalentId,
            EncoreOddsTalentId,
            CommandingTempoTalentId,
            HoopFlowTalentId,
            EmberStepTalentId,
            CloseCircleTalentId,
            WarmApplauseTalentId,
            SecondCircleTalentId,
            EncoreBounceTalentId,
            SoftCatchTalentId,
            RicochetRhythmTalentId,
            CaromLineTalentId,
            PackedBallsTalentId
        };

        public static string[] TalentIdsForPerformer(string performerId)
        {
            return performerId switch
            {
                DevsampleCappaId => new[] { CappaTalentId, RubberGrinTalentId, FaceForwardTalentId, CapsulePopTalentId, SampleSmileTalentId },
                StrongmanId => new[] { StrongmanTalentId, CannonBraceTalentId, HeavyLiftTalentId, SteadyHandsTalentId, BulwarkStepTalentId },
                AcrobatId => new[] { AcrobatTalentId, TightropeFootworkTalentId, KnifeFlourishTalentId, FastRecoveryTalentId, LongStrideTalentId },
                RingmasterId => new[] { RingmasterTalentId, SpotlightCueTalentId, GrandEntranceTalentId, EncoreOddsTalentId, CommandingTempoTalentId },
                SolaHoopDancerId => new[] { HoopFlowTalentId, EmberStepTalentId, CloseCircleTalentId, WarmApplauseTalentId, SecondCircleTalentId },
                BibiBallJugglerId => new[] { EncoreBounceTalentId, SoftCatchTalentId, RicochetRhythmTalentId, CaromLineTalentId, PackedBallsTalentId },
                _ => new[] { JugglerTalentId, CleanCompileTalentId, SampleArcTalentId, DebugMarkerTalentId, SteadyBaselineTalentId }
            };
        }

        public static string PerformerIdForTalentId(string talentId)
        {
            for (int i = 0; i < PerformerIds.Length; i++)
            {
                string performerId = PerformerIds[i];
                string[] talentIds = TalentIdsForPerformer(performerId);
                for (int j = 0; j < talentIds.Length; j++)
                {
                    if (talentIds[j] == talentId)
                    {
                        return performerId;
                    }
                }
            }

            return JugglerId;
        }

        public static void ApplyPerformer(
            PerformerDefinition performer,
            string id,
            WeaponDefinition cannon,
            WeaponDefinition knifeFan,
            WeaponDefinition spotlightBolt,
            WeaponDefinition fireHoop,
            WeaponDefinition jugglingBall,
            params TalentDefinition[] uniqueTalents)
        {
            switch (id)
            {
                case DevsampleCappaId:
                    performer.ApplyDefaultsWithTitle(
                        DevsampleCappaId,
                        "Devsample Cappa",
                        "The Capsule Sample",
                        "Capsule-face sample performer using the Juggling Ball baseline weapon.",
                        ContentTagSet.With(ContentTag.Projectile, ContentTag.Physical, ContentTag.Bounce, ContentTag.Utility),
                        jugglingBall,
                        new Color(0.44f, 0.86f, 0.98f, 1f),
                        "No special passive.",
                        PerformerPassiveKind.None,
                        "Dev sample bounce timing, pickup checks, and readable baseline talents.",
                        null,
                        null,
                        uniqueTalents);
                    break;
                case StrongmanId:
                    performer.ApplyDefaultsWithTitle(
                        StrongmanId,
                        "Bruno",
                        "The Cannon Strongman",
                        "A heavy cannon act with armor and slower footwork.",
                        ContentTagSet.With(ContentTag.Tank, ContentTag.Physical, ContentTag.Defense),
                        cannon,
                        new Color(0.95f, 0.38f, 0.28f, 1f),
                        "+5 Armor, -5% Movement Speed.",
                        PerformerPassiveKind.StaticStatModifiers,
                        "Cannon bracing, armor, splash radius, and deliberate heavy tempo.",
                        new[] { new UpgradeStatModifierDefinition(StatId.PlayerArmor, StatModifierBucket.Flat, 5f) },
                        new[] { new UpgradeStatModifierDefinition(StatId.PlayerMoveSpeedMultiplier, StatModifierBucket.AdditivePercent, -0.05f) },
                        uniqueTalents);
                    break;
                case AcrobatId:
                    performer.ApplyDefaultsWithTitle(
                        AcrobatId,
                        "Mira",
                        "The Knife Acrobat",
                        "A fast knife-fan act that trades health for precision.",
                        ContentTagSet.With(ContentTag.Movement, ContentTag.Crit, ContentTag.Rapid),
                        knifeFan,
                        new Color(0.36f, 0.85f, 1f, 1f),
                        "+10% Movement Speed, -10 Max HP.",
                        PerformerPassiveKind.StaticStatModifiers,
                        "Knife fan spread, mobility, recovery, and high-risk stage movement.",
                        new[] { new UpgradeStatModifierDefinition(StatId.PlayerMoveSpeedMultiplier, StatModifierBucket.AdditivePercent, 0.1f) },
                        new[] { new UpgradeStatModifierDefinition(StatId.PlayerMaxHealth, StatModifierBucket.Flat, -10f) },
                        uniqueTalents);
                    break;
                case RingmasterId:
                    performer.ApplyDefaultsWithTitle(
                        RingmasterId,
                        "Vox",
                        "The Spotlight Ringmaster",
                        "A spotlight conductor who bends luck and progression.",
                        ContentTagSet.With(ContentTag.Luck, ContentTag.Economy, ContentTag.Utility),
                        spotlightBolt,
                        new Color(0.72f, 0.55f, 1f, 1f),
                        "+15 Luck, +5% XP Gain.",
                        PerformerPassiveKind.StaticStatModifiers,
                        "Spotlight pacing, reward odds, XP flow, and command tempo.",
                        new[] { new UpgradeStatModifierDefinition(StatId.Luck, StatModifierBucket.Flat, 15f) },
                        new[] { new UpgradeStatModifierDefinition(StatId.XpGainMultiplier, StatModifierBucket.AdditivePercent, 0.05f) },
                        uniqueTalents);
                    break;
                case SolaHoopDancerId:
                    performer.ApplyDefaultsWithTitle(
                        SolaHoopDancerId,
                        "Sola",
                        "The Fire Hoop Dancer",
                        "A close-range orbit act built to test Fire Hoop coverage.",
                        ContentTagSet.With(ContentTag.Fire, ContentTag.Orbit, ContentTag.Movement),
                        fireHoop,
                        new Color(1f, 0.38f, 0.12f, 1f),
                        "+5% Projectile Size, -5 Max HP.",
                        PerformerPassiveKind.StaticStatModifiers,
                        "Fire Hoop radius, extra hoops, close orbit safety, and XP tempo.",
                        new[] { new UpgradeStatModifierDefinition(StatId.ProjectileAreaMultiplier, StatModifierBucket.AdditivePercent, 0.05f) },
                        new[] { new UpgradeStatModifierDefinition(StatId.PlayerMaxHealth, StatModifierBucket.Flat, -5f) },
                        uniqueTalents);
                    break;
                case BibiBallJugglerId:
                    performer.ApplyDefaultsWithTitle(
                        BibiBallJugglerId,
                        "Bibi",
                        "The Ball Juggler",
                        "A ricochet performer built to test Juggling Ball bounce.",
                        ContentTagSet.With(ContentTag.Bounce, ContentTag.Physical, ContentTag.Projectile),
                        jugglingBall,
                        new Color(1f, 0.75f, 0.28f, 1f),
                        "+1 Bounce, -5% Attack Speed.",
                        PerformerPassiveKind.StaticStatModifiers,
                        "Bounces, catch timing, projectile speed, and ricochet control.",
                        new[] { new UpgradeStatModifierDefinition(StatId.Bounce, StatModifierBucket.Flat, 1f) },
                        new[] { new UpgradeStatModifierDefinition(StatId.GlobalWeaponHaste, StatModifierBucket.AdditivePercent, -0.05f) },
                        uniqueTalents);
                    break;
                default:
                    performer.ApplyDefaultsWithTitle(
                        JugglerId,
                        "Devsample Sam",
                        "The Dev Sample",
                        "Default dev sample performer using the Juggling Ball baseline weapon.",
                        ContentTagSet.With(ContentTag.Projectile, ContentTag.Physical, ContentTag.Bounce, ContentTag.Utility),
                        jugglingBall,
                        new Color(0.98f, 0.76f, 0.23f, 1f),
                        "No special passive.",
                        PerformerPassiveKind.None,
                        "Baseline Juggling Ball rhythm, deterministic samples, and simple testing talents.",
                        null,
                        null,
                        uniqueTalents);
                    break;
            }
        }

        public static void ApplyTalent(TalentDefinition talent, string id, string performerId)
        {
            TalentSpec spec = TalentSpecFor(id);
            talent.ApplyDefaults(
                spec.Id,
                spec.Name,
                spec.Description,
                spec.Tags,
                spec.Color,
                spec.Modifier);
            talent.poolKind = TalentPoolKind.PerformerSpecific;
            talent.performerId = performerId;
            talent.repeatPolicy = spec.Capped ? TalentRepeatPolicy.Capped : TalentRepeatPolicy.Infinite;
            talent.maxLevel = spec.MaxLevel;
            talent.possibleRarities = spec.Rarities;
            talent.EnsureWorkflowDefaults();
        }

        private static TalentSpec TalentSpecFor(string id)
        {
            return id switch
            {
                CleanCompileTalentId => Percent(id, "Clean Compile", "No missed cue, no loose stitch, no wasted motion.", StatId.GlobalWeaponHaste, StatModifierBucket.AdditivePercent, 0.05f, new Color(0.95f, 0.85f, 0.35f, 1f)),
                SampleArcTalentId => Percent(id, "Sample Arc", "Give the test toss a graceful little hang time.", StatId.ProjectileDurationMultiplier, StatModifierBucket.AdditivePercent, 0.05f, new Color(0.86f, 0.72f, 1f, 1f)),
                DebugMarkerTalentId => Percent(id, "Debug Marker", "Make the sample trick impossible to miss.", StatId.ProjectileAreaMultiplier, StatModifierBucket.AdditivePercent, 0.05f, new Color(0.55f, 0.9f, 1f, 1f)),
                SteadyBaselineTalentId => Flat(id, "Steady Baseline", "Keep the dev act stable when the floor gets busy.", StatId.PlayerMaxHealth, 6f, new Color(0.9f, 0.9f, 0.72f, 1f)),
                CappaTalentId => Percent(id, "Capsule Toss", "Let the smiling shell snap the toss with extra bite.", StatId.GlobalDamageMultiplier, StatModifierBucket.AdditivePercent, 0.05f, new Color(0.44f, 0.86f, 0.98f, 1f)),
                RubberGrinTalentId => Flat(id, "Rubber Grin", "Bounce off the rough stuff with a painted grin.", StatId.PlayerMaxHealth, 4f, new Color(0.44f, 0.86f, 0.98f, 1f)),
                FaceForwardTalentId => Percent(id, "Face Forward", "Keep the grin pointed at trouble and let the shot fly.", StatId.ProjectileSpeedMultiplier, StatModifierBucket.AdditivePercent, 0.05f, new Color(0.54f, 0.96f, 1f, 1f)),
                CapsulePopTalentId => Percent(id, "Capsule Pop", "Make each pop read big from the cheap seats.", StatId.ProjectileAreaMultiplier, StatModifierBucket.AdditivePercent, 0.05f, new Color(0.5f, 0.8f, 1f, 1f)),
                SampleSmileTalentId => Flat(id, "Sample Smile", "Charm the odds with a face that refuses to quit.", StatId.Luck, 3f, new Color(0.72f, 0.55f, 1f, 1f)),
                StrongmanTalentId => Flat(id, "Iron Posture", "Plant your boots like tent stakes in a storm.", StatId.PlayerArmor, 4f, new Color(0.95f, 0.38f, 0.28f, 1f)),
                CannonBraceTalentId => Flat(id, "Cannon Brace", "Lean into the blast and make it behave.", StatId.PlayerArmor, 3f, new Color(0.92f, 0.32f, 0.2f, 1f)),
                HeavyLiftTalentId => Percent(id, "Heavy Lift", "Put strongman weight behind every hit.", StatId.GlobalDamageMultiplier, StatModifierBucket.AdditivePercent, 0.05f, new Color(1f, 0.48f, 0.24f, 1f)),
                SteadyHandsTalentId => Percent(id, "Steady Hands", "Hold the fuse steady while the room shakes.", StatId.ProjectileSpeedMultiplier, StatModifierBucket.AdditivePercent, 0.05f, new Color(1f, 0.65f, 0.28f, 1f)),
                BulwarkStepTalentId => Flat(id, "Bulwark Step", "Advance like the ring itself is moving with you.", StatId.PlayerMaxHealth, 6f, new Color(0.86f, 0.58f, 0.46f, 1f)),
                AcrobatTalentId => Percent(id, "Aerial Tempo", "Catch the beat midair and never let it drop.", StatId.PlayerMoveSpeedMultiplier, StatModifierBucket.AdditivePercent, 0.05f, new Color(0.36f, 0.85f, 1f, 1f)),
                TightropeFootworkTalentId => Percent(id, "Tightrope Footwork", "Dance the line where a stumble should have been.", StatId.PlayerMoveSpeedMultiplier, StatModifierBucket.AdditivePercent, 0.05f, new Color(0.32f, 0.88f, 1f, 1f)),
                KnifeFlourishTalentId => Percent(id, "Knife Flourish", "Turn every cut into a bit of theater.", StatId.GlobalDamageMultiplier, StatModifierBucket.AdditivePercent, 0.05f, new Color(0.58f, 0.9f, 1f, 1f)),
                FastRecoveryTalentId => Flat(id, "Fast Recovery", "Land rough, smile sharp, and keep moving.", StatId.PlayerMaxHealth, 4f, new Color(0.42f, 0.96f, 0.88f, 1f)),
                LongStrideTalentId => Percent(id, "Long Stride", "Stretch the act across the ring in a single sweep.", StatId.ProjectileSpeedMultiplier, StatModifierBucket.AdditivePercent, 0.05f, new Color(0.52f, 0.8f, 1f, 1f)),
                RingmasterTalentId => Flat(id, "Packed House", "Fill the seats and let the odds enjoy the noise.", StatId.Luck, 3f, new Color(0.72f, 0.55f, 1f, 1f)),
                SpotlightCueTalentId => Percent(id, "Spotlight Cue", "Snap the cue before the shadow knows it is late.", StatId.GlobalWeaponHaste, StatModifierBucket.AdditivePercent, 0.05f, new Color(0.72f, 0.55f, 1f, 1f)),
                GrandEntranceTalentId => Percent(id, "Grand Entrance", "Make the first step feel like a headline.", StatId.XpGainMultiplier, StatModifierBucket.AdditivePercent, 0.05f, new Color(0.82f, 0.62f, 1f, 1f)),
                EncoreOddsTalentId => Flat(id, "Encore Odds", "Leave fate waiting in the wings for one more cue.", StatId.Luck, 3f, new Color(0.64f, 0.5f, 1f, 1f)),
                CommandingTempoTalentId => Percent(id, "Commanding Tempo", "Set the rhythm and make every act follow.", StatId.GlobalWeaponHaste, StatModifierBucket.AdditivePercent, 0.05f, new Color(0.86f, 0.7f, 1f, 1f)),
                HoopFlowTalentId => Percent(id, "Hoop Flow", "Let the flame circle bloom with every turn.", StatId.ProjectileAreaMultiplier, StatModifierBucket.AdditivePercent, 0.05f, new Color(1f, 0.38f, 0.12f, 1f)),
                EmberStepTalentId => Percent(id, "Ember Step", "Step through sparks as if they were confetti.", StatId.PlayerMoveSpeedMultiplier, StatModifierBucket.AdditivePercent, 0.05f, new Color(1f, 0.46f, 0.18f, 1f)),
                CloseCircleTalentId => Percent(id, "Close Circle", "Pull the heat in tight and make it personal.", StatId.GlobalDamageMultiplier, StatModifierBucket.AdditivePercent, 0.05f, new Color(1f, 0.28f, 0.08f, 1f)),
                WarmApplauseTalentId => Percent(id, "Warm Applause", "Let the crowd's warmth feed the run.", StatId.XpGainMultiplier, StatModifierBucket.AdditivePercent, 0.05f, new Color(1f, 0.58f, 0.22f, 1f)),
                SecondCircleTalentId => Flat(id, "Second Circle", "Find another circle in the smoke and step through.", StatId.PlayerExtraJumps, 1f, new Color(1f, 0.72f, 0.28f, 1f), ContentRaritySet.Range(ContentRarity.Rare, ContentRarity.Legendary), capped: true, maxLevel: 1),
                EncoreBounceTalentId => Flat(id, "Encore Bounce", "Give each rebound a reason to come back.", StatId.Bounce, 1f, new Color(1f, 0.75f, 0.28f, 1f)),
                SoftCatchTalentId => Percent(id, "Soft Catch", "Catch the trick gently so it lingers longer.", StatId.ProjectileDurationMultiplier, StatModifierBucket.AdditivePercent, 0.05f, new Color(1f, 0.82f, 0.42f, 1f)),
                RicochetRhythmTalentId => Percent(id, "Ricochet Rhythm", "Let every rebound land on the beat.", StatId.GlobalWeaponHaste, StatModifierBucket.AdditivePercent, 0.05f, new Color(1f, 0.68f, 0.24f, 1f)),
                CaromLineTalentId => Percent(id, "Carom Line", "Read the angle before the ball leaves your hand.", StatId.ProjectileSpeedMultiplier, StatModifierBucket.AdditivePercent, 0.05f, new Color(0.95f, 0.82f, 0.36f, 1f)),
                PackedBallsTalentId => Percent(id, "Packed Balls", "Load the act with bigger, louder juggling.", StatId.ProjectileAreaMultiplier, StatModifierBucket.AdditivePercent, 0.05f, new Color(1f, 0.9f, 0.48f, 1f)),
                _ => Percent(JugglerTalentId, "Center Ring Toss", "Put the throw exactly where the whole tent can see it.", StatId.GlobalDamageMultiplier, StatModifierBucket.AdditivePercent, 0.05f, new Color(0.98f, 0.76f, 0.23f, 1f))
            };
        }

        private static TalentSpec Percent(string id, string name, string description, StatId stat, StatModifierBucket bucket, float value, Color color)
        {
            return new TalentSpec(id, name, description, ContentTagSet.With(ContentTag.Utility), color, new UpgradeStatModifierDefinition(stat, bucket, value), ContentRaritySet.All(), false, 999);
        }

        private static TalentSpec Flat(string id, string name, string description, StatId stat, float value, Color color, ContentRaritySet? rarities = null, bool capped = false, int maxLevel = 999)
        {
            return new TalentSpec(id, name, description, ContentTagSet.With(ContentTag.Utility), color, new UpgradeStatModifierDefinition(stat, StatModifierBucket.Flat, value), rarities ?? ContentRaritySet.All(), capped, maxLevel);
        }

        private readonly struct TalentSpec
        {
            public TalentSpec(string id, string name, string description, ContentTagSet tags, Color color, UpgradeStatModifierDefinition modifier, ContentRaritySet rarities, bool capped, int maxLevel)
            {
                Id = id;
                Name = name;
                Description = description;
                Tags = tags;
                Color = color;
                Modifier = modifier;
                Rarities = rarities;
                Capped = capped;
                MaxLevel = maxLevel;
            }

            public string Id { get; }
            public string Name { get; }
            public string Description { get; }
            public ContentTagSet Tags { get; }
            public Color Color { get; }
            public UpgradeStatModifierDefinition Modifier { get; }
            public ContentRaritySet Rarities { get; }
            public bool Capped { get; }
            public int MaxLevel { get; }
        }

        public static IReadOnlyList<UpgradeStatModifierDefinition> EmptyModifiers => System.Array.Empty<UpgradeStatModifierDefinition>();
    }
}
