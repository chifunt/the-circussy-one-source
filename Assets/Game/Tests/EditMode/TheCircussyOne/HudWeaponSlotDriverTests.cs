using NUnit.Framework;
using TheCircussyOne.Content;
using TheCircussyOne.Runtime;
using TheCircussyOne.Visuals;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheCircussyOne.Tests
{
    public sealed class HudWeaponSlotDriverTests
    {
        [Test]
        public void ApplyShowsFilledWeaponSlotAndLevel()
        {
            WeaponDefinition weapon = CreateWeapon();
            var runtime = new WeaponRuntime(weapon, 4);
            var slot = new VisualElement();
            var label = new Label();

            HudWeaponSlotDriver.Apply(slot, label, runtime, true);

            Assert.That(slot.style.display.value, Is.EqualTo(DisplayStyle.Flex));
            Assert.That(slot.ClassListContains("weapon-slot-filled"), Is.True);
            Assert.That(slot.style.backgroundColor.value, Is.EqualTo(IconFrameBlack()));
            Assert.That(slot.style.borderTopColor.value, Is.EqualTo(weapon.projectilePrimaryColor));
            Assert.That(slot.Q<Image>(ContentIconVisuals.ImageElementName), Is.Not.Null);
            Assert.That(label.text, Is.EqualTo("4"));
            Assert.That(label.style.display.value, Is.EqualTo(DisplayStyle.Flex));

            Object.DestroyImmediate(weapon);
        }

        [Test]
        public void ApplyClearsEmptyVisibleSlot()
        {
            var slot = new VisualElement();
            var label = new Label { text = "7" };
            slot.AddToClassList("weapon-slot-filled");

            HudWeaponSlotDriver.Apply(slot, label, null, true);

            Assert.That(slot.style.display.value, Is.EqualTo(DisplayStyle.Flex));
            Assert.That(slot.ClassListContains("weapon-slot-filled"), Is.False);
            Assert.That(label.text, Is.Empty);
            Assert.That(label.style.display.value, Is.EqualTo(DisplayStyle.None));
        }

        [Test]
        public void ApplyHidesSlotBeyondAvailableCapacity()
        {
            WeaponDefinition weapon = CreateWeapon();
            var runtime = new WeaponRuntime(weapon, 2);
            var slot = new VisualElement();
            var label = new Label();

            HudWeaponSlotDriver.Apply(slot, label, runtime, false);

            Assert.That(slot.style.display.value, Is.EqualTo(DisplayStyle.None));
            Assert.That(slot.ClassListContains("weapon-slot-filled"), Is.False);
            Assert.That(label.text, Is.Empty);
            Assert.That(label.style.display.value, Is.EqualTo(DisplayStyle.None));

            Object.DestroyImmediate(weapon);
        }

        private static WeaponDefinition CreateWeapon()
        {
            var weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
            weapon.weaponId = "test_weapon";
            weapon.displayName = "Test Weapon";
            weapon.projectilePrimaryColor = new Color(0.4f, 0.6f, 0.9f, 1f);
            return weapon;
        }

        private static Color IconFrameBlack()
        {
            return new Color(0.015f, 0.018f, 0.024f, 0.98f);
        }
    }
}
