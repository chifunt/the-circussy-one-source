using UnityEngine;
using TheCircussyOne.Stats;

namespace TheCircussyOne.Content
{
    public static class StarterTalentDefaults
    {
        public const string FootworkId = "footwork";
        public const string StageStaminaId = "stage_stamina";
        public const string ToughSkinId = "tough_skin";
        public const string CrowdFavoriteId = "crowd_favorite";
        public const string QuickHandsId = "quick_hands";
        public const string SharpEyeId = "sharp_eye";
        public const string LongReachId = "long_reach";
        public const string LuckyBreakId = "lucky_break";
        public const string BiggerPropsId = "bigger_props";
        public const string LongerActId = "longer_act";
        public const string MagneticApplauseId = "magnetic_applause";
        public const string SpringboardId = "springboard";

        public static readonly string[] AllIds =
        {
            FootworkId,
            StageStaminaId,
            ToughSkinId,
            CrowdFavoriteId,
            QuickHandsId,
            SharpEyeId,
            LongReachId,
            LuckyBreakId,
            BiggerPropsId,
            LongerActId,
            MagneticApplauseId,
            SpringboardId
        };

        public static void Apply(TalentDefinition talent, string id)
        {
            switch (id)
            {
                case StageStaminaId:
                    talent.ApplyDefaults(id, "Stage Stamina", "Hold the pose, take the hit, and stay under the lights.", ContentTagSet.With(ContentTag.Defense), new Color(1f, 0.3f, 0.4f, 1f), new UpgradeStatModifierDefinition(StatId.PlayerMaxHealth, StatModifierBucket.Flat, 8f));
                    break;
                case ToughSkinId:
                    talent.ApplyDefaults(id, "Tough Skin", "Turn bruises into part of the costume.", ContentTagSet.With(ContentTag.Defense), new Color(0.8f, 0.86f, 0.92f, 1f), new UpgradeStatModifierDefinition(StatId.PlayerArmor, StatModifierBucket.Flat, 3f));
                    break;
                case CrowdFavoriteId:
                    talent.ApplyDefaults(id, "Crowd Favorite", "The crowd leans in, and every reward feels louder.", ContentTagSet.With(ContentTag.Economy), new Color(0.22f, 0.88f, 1f, 1f), new UpgradeStatModifierDefinition(StatId.XpGainMultiplier, StatModifierBucket.AdditivePercent, 0.05f));
                    break;
                case QuickHandsId:
                    talent.ApplyDefaults(id, "Quick Hands", "Move before the audience can blink.", ContentTagSet.With(ContentTag.Utility, ContentTag.Projectile), new Color(0.95f, 0.8f, 1f, 1f), new UpgradeStatModifierDefinition(StatId.GlobalWeaponHaste, StatModifierBucket.AdditivePercent, 0.05f));
                    break;
                case SharpEyeId:
                    talent.ApplyDefaults(id, "Sharp Eye", "Find the perfect opening and make it count.", ContentTagSet.With(ContentTag.Damage), new Color(1f, 0.52f, 0.28f, 1f), new UpgradeStatModifierDefinition(StatId.GlobalDamageMultiplier, StatModifierBucket.AdditivePercent, 0.05f));
                    break;
                case LongReachId:
                    talent.ApplyDefaults(id, "Long Reach", "Send every trick farther across the ring.", ContentTagSet.With(ContentTag.Projectile, ContentTag.Utility), new Color(1f, 0.95f, 0.28f, 1f), new UpgradeStatModifierDefinition(StatId.ProjectileSpeedMultiplier, StatModifierBucket.AdditivePercent, 0.05f));
                    break;
                case LuckyBreakId:
                    talent.ApplyDefaults(id, "Lucky Break", "Let fortune peek from behind the curtain.", ContentTagSet.With(ContentTag.Luck, ContentTag.Economy, ContentTag.Utility), new Color(0.72f, 0.55f, 1f, 1f), new UpgradeStatModifierDefinition(StatId.Luck, StatModifierBucket.Flat, 3f));
                    break;
                case BiggerPropsId:
                    talent.ApplyDefaults(id, "Bigger Props", "Make the act read larger from the back row.", ContentTagSet.With(ContentTag.Projectile, ContentTag.Utility), new Color(1f, 0.58f, 0.3f, 1f), new UpgradeStatModifierDefinition(StatId.ProjectileAreaMultiplier, StatModifierBucket.AdditivePercent, 0.05f));
                    break;
                case LongerActId:
                    talent.ApplyDefaults(id, "Longer Act", "Keep the trick in the air long enough to matter.", ContentTagSet.With(ContentTag.Projectile, ContentTag.Utility), new Color(0.9f, 0.78f, 1f, 1f), new UpgradeStatModifierDefinition(StatId.ProjectileDurationMultiplier, StatModifierBucket.AdditivePercent, 0.05f));
                    break;
                case MagneticApplauseId:
                    talent.ApplyDefaults(id, "Magnetic Applause", "Pull the crowd's gifts closer with a practiced bow.", ContentTagSet.With(ContentTag.Pickup, ContentTag.Economy, ContentTag.Utility), new Color(0.28f, 0.92f, 1f, 1f), new UpgradeStatModifierDefinition(StatId.PickupMagnetRadius, StatModifierBucket.Flat, 0.4f));
                    break;
                case SpringboardId:
                    talent.ApplyDefaults(id, "Springboard", "Launch from the boards like the stage is helping.", ContentTagSet.With(ContentTag.Movement, ContentTag.Utility), new Color(0.58f, 1f, 0.55f, 1f), new UpgradeStatModifierDefinition(StatId.PlayerExtraJumps, StatModifierBucket.Flat, 1f));
                    talent.possibleRarities = ContentRaritySet.Range(ContentRarity.Rare, ContentRarity.Legendary);
                    talent.repeatPolicy = TalentRepeatPolicy.Capped;
                    talent.maxLevel = 2;
                    break;
                default:
                    talent.ApplyDefaults(FootworkId, "Footwork", "Slip between danger like the sawdust knows your shoes.", ContentTagSet.With(ContentTag.Movement), new Color(0.46f, 0.72f, 1f, 1f), new UpgradeStatModifierDefinition(StatId.PlayerMoveSpeedMultiplier, StatModifierBucket.AdditivePercent, 0.05f));
                    break;
            }
        }

        public static TalentDefinition Create(string id)
        {
            TalentDefinition talent = ScriptableObject.CreateInstance<TalentDefinition>();
            Apply(talent, id);
            return talent;
        }
    }
}
