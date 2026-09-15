using TheCircussyOne.Rules;
using TheCircussyOne.Runtime;
using UnityEngine.UIElements;

namespace TheCircussyOne.Visuals
{
    internal static class HudWeaponSlotDriver
    {
        public static void Apply(VisualElement slot, Label levelLabel, WeaponRuntime weapon, bool visibleSlot)
        {
            if (slot == null)
            {
                return;
            }

            bool hasWeapon = visibleSlot && weapon?.Definition != null;
            slot.style.display = visibleSlot ? DisplayStyle.Flex : DisplayStyle.None;
            slot.EnableInClassList("weapon-slot-filled", hasWeapon);
            ContentIconVisuals.Apply(slot, ContentIconRules.ForWeapon(weapon?.Definition), ContentIconRules.FallbackColor(weapon?.Definition));

            if (levelLabel == null)
            {
                return;
            }

            levelLabel.text = hasWeapon ? weapon.Level.ToString() : string.Empty;
            levelLabel.style.display = hasWeapon ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
