using Sirenix.OdinInspector;
using UnityEngine;

namespace TheCircussyOne.Content
{
    public sealed partial class WeaponDefinition
    {
        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Core"), Button("Copy ID"), PropertyOrder(2)]
        private void CopyId()
        {
            GUIUtility.systemCopyBuffer = Id;
        }

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Core"), Button("Regenerate ID From Display Name"), PropertyOrder(3)]
        private void RegenerateIdFromDisplayName()
        {
            weaponId = ContentIdSuggestionRules.NormalizeBase(displayName, "weapon");
            EnsureWorkflowDefaults();
        }
    }
}
