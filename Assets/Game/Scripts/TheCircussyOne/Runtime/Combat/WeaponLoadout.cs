using System;
using System.Collections.Generic;
using TheCircussyOne.Content;

namespace TheCircussyOne.Runtime
{
    public sealed class WeaponLoadout
    {
        public const int DefaultMaxWeapons = 4;

        private readonly List<WeaponRuntime> weapons = new();
        private readonly int maxWeapons;
        private bool suppressChanged;

        public WeaponLoadout(WeaponCatalog catalog, int startingLevel = 1, int maxWeapons = DefaultMaxWeapons)
        {
            this.maxWeapons = Math.Max(0, maxWeapons);
            if (catalog == null)
            {
                return;
            }

            IReadOnlyList<WeaponDefinition> definitions = catalog.StartingWeapons;
            for (int i = 0; i < definitions.Count; i++)
            {
                WeaponDefinition definition = definitions[i];
                if (!ContentAvailabilityRules.IsActiveAndValid(definition))
                {
                    continue;
                }

                AddWeapon(definition, startingLevel);
            }
        }

        public IReadOnlyList<WeaponRuntime> Weapons => weapons;
        public int MaxWeapons => maxWeapons;
        public int WeaponCount => weapons.Count;
        public int AvailableSlots => Math.Max(0, maxWeapons - weapons.Count);
        public bool HasOpenSlot => AvailableSlots > 0;
        public event Action Changed;

        public bool OwnsWeapon(string weaponId)
        {
            return FindById(weaponId) != null;
        }

        public bool CanAddWeapon(WeaponDefinition definition)
        {
            if (!ContentAvailabilityRules.IsActiveAndValid(definition) || !HasOpenSlot)
            {
                return false;
            }

            return FindById(definition.Id) == null;
        }

        public bool AddWeapon(WeaponDefinition definition, int startingLevel = 1)
        {
            if (!CanAddWeapon(definition))
            {
                return false;
            }

            definition.EnsureWorkflowDefaults();
            weapons.Add(new WeaponRuntime(definition, startingLevel));
            NotifyChanged();
            return true;
        }

        public void Clear()
        {
            weapons.Clear();
            NotifyChanged();
        }

        public bool RemoveWeapon(string weaponId)
        {
            if (string.IsNullOrWhiteSpace(weaponId))
            {
                return false;
            }

            for (int i = 0; i < weapons.Count; i++)
            {
                WeaponRuntime weapon = weapons[i];
                if (weapon?.Definition == null || !string.Equals(weapon.Definition.Id, weaponId, StringComparison.Ordinal))
                {
                    continue;
                }

                weapons.RemoveAt(i);
                NotifyChanged();
                return true;
            }

            return false;
        }

        public void SetStartingWeapons(params WeaponDefinition[] definitions)
        {
            suppressChanged = true;
            try
            {
                weapons.Clear();
                if (definitions != null)
                {
                    for (int i = 0; i < definitions.Length; i++)
                    {
                        AddWeapon(definitions[i]);
                    }
                }
            }
            finally
            {
                suppressChanged = false;
            }

            NotifyChanged();
        }

        public void SetStartingWeapons(IReadOnlyList<WeaponDefinition> definitions, int startingLevel = 1)
        {
            suppressChanged = true;
            try
            {
                weapons.Clear();
                if (definitions != null)
                {
                    for (int i = 0; i < definitions.Count; i++)
                    {
                        AddWeapon(definitions[i], startingLevel);
                    }
                }
            }
            finally
            {
                suppressChanged = false;
            }

            NotifyChanged();
        }

        public WeaponRuntime FindById(string weaponId)
        {
            if (string.IsNullOrWhiteSpace(weaponId))
            {
                return null;
            }

            for (int i = 0; i < weapons.Count; i++)
            {
                WeaponRuntime weapon = weapons[i];
                if (weapon?.Definition != null && string.Equals(weapon.Definition.Id, weaponId, System.StringComparison.Ordinal))
                {
                    return weapon;
                }
            }

            return null;
        }

        public bool IncreaseWeaponLevel(string weaponId, int amount = 1)
        {
            WeaponRuntime weapon = FindById(weaponId);
            if (weapon == null)
            {
                return false;
            }

            weapon.IncreaseLevel(amount);
            NotifyChanged();
            return true;
        }

        private void NotifyChanged()
        {
            if (!suppressChanged)
            {
                Changed?.Invoke();
            }
        }
    }
}
