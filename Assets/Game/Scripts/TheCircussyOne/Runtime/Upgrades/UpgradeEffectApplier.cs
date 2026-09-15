using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using TheCircussyOne.Stats;

namespace TheCircussyOne.Runtime
{
    public sealed class UpgradeEffectApplier
    {
        private readonly UpgradeRunState runState;
        private readonly RunStats stats;
        private readonly WeaponLoadout weaponLoadout;
        private readonly ItemInventory itemInventory;
        private readonly TalentEffectApplier talentEffectApplier;
        private readonly GameState state;

        public UpgradeEffectApplier(
            UpgradeRunState runState,
            RunStats stats,
            WeaponLoadout weaponLoadout,
            GameState state,
            ItemInventory itemInventory = null,
            TalentEffectApplier talentEffectApplier = null)
        {
            this.runState = runState;
            this.stats = stats;
            this.weaponLoadout = weaponLoadout;
            this.itemInventory = itemInventory;
            this.talentEffectApplier = talentEffectApplier;
            this.state = state;
        }

        public bool Apply(UpgradeChoice choice)
        {
            if (choice.Kind == UpgradeChoiceKind.Talent)
            {
                return talentEffectApplier != null && talentEffectApplier.Apply(choice.Talent, choice.Rarity);
            }

            if (choice.Kind == UpgradeChoiceKind.AddWeapon)
            {
                WeaponDefinition weaponToAdd = choice.WeaponToAdd;
                return weaponToAdd != null
                    && weaponLoadout != null
                    && weaponLoadout.AddWeapon(weaponToAdd);
            }

            UpgradeDefinition definition = choice.Definition;
            if (definition == null)
            {
                return false;
            }

            if (runState == null)
            {
                return false;
            }

            int appliedLevel = runState.GetLevel(definition) + 1;
            string sourceId = $"upgrade:{definition.Id}:{appliedLevel}";
            bool applied = false;
            WeaponRuntime weapon = weaponLoadout?.FindById(definition.weaponId);
            if (weapon != null && definition.statModifiers != null)
            {
                for (int i = 0; i < definition.statModifiers.Count; i++)
                {
                    weapon.AddModifier(definition.statModifiers[i].ToRuntime(sourceId, choice.Rarity));
                }

                applied = definition.statModifiers.Count > 0;
            }

            if (!applied || runState.IncrementLevel(definition) <= 0)
            {
                return false;
            }

            weaponLoadout?.IncreaseWeaponLevel(definition.weaponId);
            state?.RefreshDerivedStats();
            return true;
        }
    }
}
