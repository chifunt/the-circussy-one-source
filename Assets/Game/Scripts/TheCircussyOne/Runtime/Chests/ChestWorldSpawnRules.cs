using TheCircussyOne.Content;

namespace TheCircussyOne.Runtime
{
    public static class ChestWorldSpawnRules
    {
        public static bool CanSpawnInWorld(ChestDefinition chest)
        {
            return ContentAvailabilityRules.IsActiveAndValid(chest)
                && chest.kind != ChestKind.SpecialEnemy;
        }
    }
}
