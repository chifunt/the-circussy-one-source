using System.Collections.Generic;
using System.Linq;
using TheCircussyOne.Stats;

namespace TheCircussyOne.Content
{
    public static class ComputedContentTagRules
    {
        public static ContentTagSet For(WeaponDefinition weapon)
        {
            var tags = new HashSet<ContentTag>();
            if (weapon == null)
            {
                return ContentTagSet.With(ContentTag.Utility);
            }

            bool projectileWeapon = !weapon.orbitEnabled;
            if (projectileWeapon)
            {
                tags.Add(ContentTag.Projectile);
                tags.Add(ContentTag.Ranged);
            }

            if (weapon.projectileDamage > 0)
            {
                tags.Add(ContentTag.Damage);
            }

            if (weapon.baseProjectileCount > 1 || weapon.maxProjectileCount > 1 || weapon.baseSpreadAngleDegrees > 0f)
            {
                tags.Add(ContentTag.Rapid);
            }

            if (weapon.bounceEnabled)
            {
                tags.Add(ContentTag.Bounce);
            }

            if (weapon.explosiveEnabled)
            {
                tags.Add(ContentTag.Explosive);
            }

            if (weapon.chainEnabled)
            {
                tags.Add(ContentTag.Chain);
            }

            if (weapon.orbitEnabled)
            {
                tags.Add(ContentTag.Aura);
                tags.Add(ContentTag.Orbit);
                tags.Add(ContentTag.Damage);
            }

            return From(tags);
        }

        public static ContentTagSet For(UpgradeDefinition upgrade)
        {
            var tags = new HashSet<ContentTag> { ContentTag.WeaponSynergy };
            if (upgrade == null)
            {
                return ContentTagSet.With(ContentTag.Utility);
            }

            AddTags(tags, upgrade.weaponDefinition?.Tags);
            AddModifierTags(tags, upgrade.statModifiers);
            return From(tags);
        }

        public static ContentTagSet For(TalentDefinition talent)
        {
            var tags = new HashSet<ContentTag>();
            if (talent == null)
            {
                return ContentTagSet.With(ContentTag.Utility);
            }

            AddModifierTags(tags, talent.statModifiers);
            if (talent.poolKind == TalentPoolKind.PerformerSpecific)
            {
                tags.Add(ContentTag.Special);
            }

            return From(tags);
        }

        public static ContentTagSet For(ItemDefinition item)
        {
            var tags = new HashSet<ContentTag>();
            if (item == null)
            {
                return ContentTagSet.With(ContentTag.Utility);
            }

            AddModifierTags(tags, item.statModifiers);
            AddModifierTags(tags, item.downsideStatModifiers);
            if (item.HasDownside)
            {
                tags.Add(ContentTag.Risk);
            }

            if (item.stackPolicy == ItemStackPolicy.Unique || item.stackPolicy == ItemStackPolicy.InvalidDuplicate)
            {
                tags.Add(ContentTag.Special);
            }

            return From(tags);
        }

        public static ContentTagSet For(PerformerDefinition performer)
        {
            var tags = new HashSet<ContentTag>();
            if (performer == null)
            {
                return ContentTagSet.With(ContentTag.Utility);
            }

            AddTags(tags, performer.startingWeapon?.Tags);
            AddModifierTags(tags, performer.baseStatModifiers);
            AddModifierTags(tags, performer.passiveStatModifiers);
            if (performer.passiveKind != PerformerPassiveKind.None)
            {
                tags.Add(ContentTag.Special);
            }

            return From(tags);
        }

        public static ContentTagSet For(EnemyDefinition enemy)
        {
            var tags = new HashSet<ContentTag> { ContentTag.Swarm };
            if (enemy == null)
            {
                return ContentTagSet.With(ContentTag.Swarm);
            }

            tags.Add(ContentTag.Damage);
            if (enemy.behaviorType == EnemyBehaviorType.Ranged)
            {
                tags.Add(ContentTag.Ranged);
            }

            if (enemy.behaviorType == EnemyBehaviorType.Support)
            {
                tags.Add(ContentTag.Utility);
            }

            if (enemy.baseMoveSpeed > 0f || enemy.moveSpeedGrowthPerMinute > 0f)
            {
                tags.Add(ContentTag.Movement);
            }

            if (enemy.climb != null && (enemy.climb.canClimbEnvironment || enemy.stack?.canClimbEnemies == true))
            {
                tags.Add(ContentTag.Movement);
            }

            if (enemy.stack != null && enemy.stack.policy != EnemyStackPolicy.GroundOnly)
            {
                tags.Add(ContentTag.Swarm);
            }

            return From(tags);
        }

        public static ContentTagSet For(HeadlinerDefinition headliner)
        {
            var tags = new HashSet<ContentTag> { ContentTag.Boss, ContentTag.Elite, ContentTag.Special };
            if (headliner == null)
            {
                tags.Add(ContentTag.Physical);
                return From(tags);
            }

            AddTags(tags, headliner.enemyActor?.Tags);
            tags.Remove(ContentTag.Swarm);
            return From(tags);
        }

        public static ContentTagSet For(XpGemDefinition gem)
        {
            return ContentTagSet.With(ContentTag.Economy, ContentTag.Pickup, ContentTag.XP);
        }

        public static ContentTagSet For(HealthPickupDefinition pickup)
        {
            return ContentTagSet.With(ContentTag.Economy, ContentTag.Pickup, ContentTag.Health);
        }

        public static ContentTagSet For(TicketDepositDefinition deposit)
        {
            return ContentTagSet.With(ContentTag.Economy, ContentTag.Currency);
        }

        public static ContentTagSet For(HealingPropDefinition prop)
        {
            return ContentTagSet.With(ContentTag.Economy, ContentTag.Health);
        }

        public static ContentTagSet For(ChestDefinition chest)
        {
            return ContentTagSet.With(ContentTag.Economy, ContentTag.Chest);
        }

        public static string Format(ContentTagSet tags)
        {
            if (tags == null || tags.Values == null || tags.Values.Count == 0)
            {
                return "None";
            }

            return string.Join(", ", tags.Values.Select(tag => tag.ToString()));
        }

        private static void AddModifierTags(HashSet<ContentTag> tags, IReadOnlyList<UpgradeStatModifierDefinition> modifiers)
        {
            if (modifiers == null)
            {
                return;
            }

            for (int i = 0; i < modifiers.Count; i++)
            {
                AddStatTags(tags, modifiers[i].statId);
            }
        }

        private static void AddModifierTags(HashSet<ContentTag> tags, IReadOnlyList<ItemStatModifierDefinition> modifiers)
        {
            if (modifiers == null)
            {
                return;
            }

            for (int i = 0; i < modifiers.Count; i++)
            {
                AddStatTags(tags, modifiers[i].statId);
            }
        }

        private static void AddStatTags(HashSet<ContentTag> tags, IReadOnlyList<StatId> statIds)
        {
            if (statIds == null)
            {
                return;
            }

            for (int i = 0; i < statIds.Count; i++)
            {
                AddStatTags(tags, statIds[i]);
            }
        }

        private static void AddStatTags(HashSet<ContentTag> tags, StatId statId)
        {
            switch (statId)
            {
                case StatId.PlayerMaxHealth:
                case StatId.PlayerArmor:
                case StatId.PlayerHpRegenPerMinute:
                    tags.Add(ContentTag.Defense);
                    break;
                case StatId.PlayerMoveSpeedMultiplier:
                case StatId.PlayerExtraJumps:
                    tags.Add(ContentTag.Movement);
                    break;
                case StatId.PickupMagnetRadius:
                case StatId.PickupCollectRadius:
                    tags.Add(ContentTag.Pickup);
                    tags.Add(ContentTag.Utility);
                    break;
                case StatId.XpGainMultiplier:
                    tags.Add(ContentTag.XP);
                    tags.Add(ContentTag.Economy);
                    break;
                case StatId.Luck:
                    tags.Add(ContentTag.Luck);
                    tags.Add(ContentTag.Economy);
                    break;
                case StatId.GlobalWeaponHaste:
                case StatId.WeaponAttackSpeed:
                    tags.Add(ContentTag.Rapid);
                    tags.Add(ContentTag.Utility);
                    break;
                case StatId.CritChance:
                case StatId.CritDamageMultiplier:
                    tags.Add(ContentTag.Crit);
                    tags.Add(ContentTag.Damage);
                    break;
                case StatId.ProjectileSpeedMultiplier:
                case StatId.ProjectileAreaMultiplier:
                case StatId.ProjectileDurationMultiplier:
                case StatId.ProjectileCount:
                case StatId.WeaponProjectileCount:
                case StatId.WeaponProjectileSpeedMultiplier:
                case StatId.WeaponProjectileSizeMultiplier:
                case StatId.WeaponProjectileLifetimeMultiplier:
                case StatId.WeaponAccuracyMultiplier:
                    tags.Add(ContentTag.Projectile);
                    tags.Add(ContentTag.Utility);
                    break;
                case StatId.Pierce:
                case StatId.WeaponPierce:
                    tags.Add(ContentTag.Pierce);
                    tags.Add(ContentTag.Projectile);
                    break;
                case StatId.Bounce:
                case StatId.WeaponBounce:
                    tags.Add(ContentTag.Bounce);
                    tags.Add(ContentTag.Projectile);
                    break;
                case StatId.Chain:
                case StatId.WeaponChain:
                    tags.Add(ContentTag.Chain);
                    tags.Add(ContentTag.Projectile);
                    break;
                case StatId.WeaponSplashRadiusMultiplier:
                    tags.Add(ContentTag.Explosive);
                    tags.Add(ContentTag.Projectile);
                    break;
                case StatId.WeaponRangeMultiplier:
                    tags.Add(ContentTag.Ranged);
                    tags.Add(ContentTag.Utility);
                    break;
                case StatId.PlayerLifestealChance:
                case StatId.PlayerKnockbackMultiplier:
                case StatId.GlobalDamageMultiplier:
                case StatId.WeaponDamageMultiplier:
                case StatId.WeaponFlatDamage:
                case StatId.WeaponKnockbackMultiplier:
                    tags.Add(ContentTag.Damage);
                    break;
            }

            if (IsWeaponStat(statId))
            {
                tags.Add(ContentTag.WeaponSynergy);
            }
        }

        private static bool IsWeaponStat(StatId statId)
        {
            return (int)statId >= (int)StatId.WeaponDamageMultiplier;
        }

        private static void AddTags(HashSet<ContentTag> tags, ContentTagSet source)
        {
            if (source?.Values == null)
            {
                return;
            }

            for (int i = 0; i < source.Values.Count; i++)
            {
                tags.Add(source.Values[i]);
            }
        }

        private static ContentTagSet From(HashSet<ContentTag> tags)
        {
            if (tags == null || tags.Count == 0)
            {
                return ContentTagSet.With(ContentTag.Utility);
            }

            return ContentTagSet.With(tags.ToArray());
        }
    }
}
