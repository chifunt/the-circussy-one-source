using TheCircussyOne.Content;
using UnityEngine;

namespace TheCircussyOne.Rules
{
    public static class ContentIconRules
    {
        public static Sprite ForPerformer(PerformerDefinition performer)
        {
            return performer != null ? performer.portraitSprite : null;
        }

        public static Sprite ForWeapon(WeaponDefinition weapon)
        {
            return weapon != null ? weapon.iconSprite : null;
        }

        public static Sprite ForItem(ItemDefinition item)
        {
            return item != null ? item.iconSprite : null;
        }

        public static Sprite ForUpgrade(UpgradeDefinition upgrade)
        {
            if (upgrade == null)
            {
                return null;
            }

            return upgrade.iconSpriteOverride != null
                ? upgrade.iconSpriteOverride
                : ForWeapon(upgrade.weaponDefinition);
        }

        public static Sprite ForTalent(TalentDefinition talent, PerformerDefinition selectedPerformer = null)
        {
            if (talent == null)
            {
                return null;
            }

            if (talent.iconSprite != null)
            {
                return talent.iconSprite;
            }

            if (talent.poolKind == TalentPoolKind.PerformerSpecific)
            {
                if (talent.performerDefinition != null)
                {
                    return ForPerformer(talent.performerDefinition);
                }

                if (selectedPerformer != null && talent.AllowsPerformer(selectedPerformer))
                {
                    return ForPerformer(selectedPerformer);
                }
            }

            return null;
        }

        public static Sprite ForUpgradeChoice(UpgradeChoice choice, PerformerDefinition selectedPerformer = null)
        {
            return choice.Kind switch
            {
                UpgradeChoiceKind.AddWeapon => ForWeapon(choice.WeaponToAdd),
                UpgradeChoiceKind.WeaponUpgrade => ForUpgrade(choice.Definition),
                UpgradeChoiceKind.Talent => ForTalent(choice.Talent, selectedPerformer),
                _ => null
            };
        }

        public static Color FallbackColor(PerformerDefinition performer)
        {
            return performer != null ? performer.portraitColor : Color.white;
        }

        public static Color FallbackColor(WeaponDefinition weapon)
        {
            return weapon != null ? weapon.projectilePrimaryColor : Color.white;
        }

        public static Color FallbackColor(UpgradeDefinition upgrade)
        {
            if (upgrade == null)
            {
                return Color.white;
            }

            return upgrade.iconColor;
        }

        public static Color FallbackColor(TalentDefinition talent)
        {
            return talent != null ? talent.iconColor : Color.white;
        }

        public static Color FallbackColor(ItemDefinition item)
        {
            return item != null ? item.iconColor : Color.white;
        }
    }
}
