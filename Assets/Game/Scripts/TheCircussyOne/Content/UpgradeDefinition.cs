using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using TheCircussyOne.Stats;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TheCircussyOne.Content
{
    [CreateAssetMenu(menuName = "The Circussy One/Content/Upgrade Definition", fileName = "UpgradeDefinition")]
    public sealed class UpgradeDefinition : SerializedScriptableObject, IActivatableContentDefinition
    {
        private const string Tabs = "Upgrade";
        private const int AvailabilityDefaultsVersion = 1;
#if UNITY_EDITOR
        private const string WeaponCatalogPath = "Assets/Game/ScriptableObjects/Balance/Weapons/WeaponCatalog.asset";
#endif

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Core"), LabelWidth(180), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.BalanceColor"), PropertyOrder(0)]
        [ValidateInput(nameof(HasUpgradeId), "Upgrade id is required."), ReadOnly]
        [Tooltip("Stable runtime ID. Use Regenerate ID only when intentionally migrating references.")]
        public string upgradeId = "move_speed";

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Core"), LabelWidth(180), PropertyOrder(1)]
        [ValidateInput(nameof(HasDisplayName), "Display name is required.")]
        public string displayName = "Move Speed";

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Core"), LabelWidth(180), MultiLineProperty(2), PropertyOrder(4)]
        public string shortDescription = "Move faster.";

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Classification"), LabelWidth(180), InlineProperty]
        public ContentRaritySet possibleRarities = ContentRaritySet.All();

        [HideInInspector]
        public ContentTagSet tags = ContentTagSet.With(ContentTag.Utility);

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Classification"), InfoBox("Computed from mechanics and effects. Change source fields instead.", InfoMessageType.Info), ShowInInspector, ReadOnly, LabelText("Computed Tags"), PropertyOrder(10)]
        private string ComputedTags => ComputedContentTagRules.Format(Tags);

        [HideInInspector]
        public bool isActive = true;

        [TabGroup(Tabs, "Progression"), BoxGroup(Tabs + "/Progression/Levels"), LabelWidth(180)]
        [Min(1)] public int maxLevel = 5;

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Card"), LabelWidth(180), AssetSelector, PreviewField(72)]
        [Tooltip("Optional override icon. Leave empty to use the target weapon icon.")]
        public Sprite iconSpriteOverride;

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Card"), LabelWidth(180)]
        [LabelText("Fallback Color")]
        public Color iconColor = new(0.92f, 0.92f, 0.88f, 1f);

        [TabGroup(Tabs, "Effect"), BoxGroup(Tabs + "/Effect/Stat Modifiers"), LabelWidth(180)]
        public List<UpgradeStatModifierDefinition> statModifiers = new();

        [TabGroup(Tabs, "Effect"), BoxGroup(Tabs + "/Effect/Weapon"), LabelText("Target Weapon"), LabelWidth(180)]
        [ValueDropdown(nameof(WeaponIdOptions), DropdownTitle = "Weapons")]
        [OnValueChanged(nameof(SyncWeaponDefinitionFromId))]
        public string weaponId = "juggling_ball";

        [HideInInspector]
        public WeaponDefinition weaponDefinition;

        [TabGroup(Tabs, "Diagnostics"), ShowInInspector, ReadOnly, LabelText("Runtime Summary"), PropertyOrder(100)]
        private string RuntimeSummary => $"{displayName} [{LevelUpRarities}] max {Mathf.Max(1, maxLevel)}: {shortDescription}";

        public string Id => upgradeId;
        public string DisplayName => displayName;
        public ContentTagSet Tags => ComputedContentTagRules.For(this);
        public bool IsActive => isActive;
        public ContentRaritySet LevelUpRarities => possibleRarities.OrAll();

        [SerializeField, HideInInspector] private int availabilityDefaultsVersion;

        public void ApplyWeaponStatUpgradeDefaults(
            WeaponDefinition definition,
            string idSuffix,
            string nameSuffix,
            string description,
            Color iconColor,
            params UpgradeStatModifierDefinition[] modifiers)
        {
            weaponDefinition = definition;
            string targetWeaponId = definition != null && !string.IsNullOrWhiteSpace(definition.Id) ? definition.Id : "weapon";
            string targetWeaponName = definition != null && !string.IsNullOrWhiteSpace(definition.DisplayName) ? definition.DisplayName : "Weapon";
            string safeSuffix = string.IsNullOrWhiteSpace(idSuffix) ? "upgrade" : idSuffix;
            upgradeId = $"{targetWeaponId}_{safeSuffix}";
            displayName = $"{targetWeaponName} {(!string.IsNullOrWhiteSpace(nameSuffix) ? nameSuffix : "Upgrade")}";
            shortDescription = string.IsNullOrWhiteSpace(description) ? $"Improve {targetWeaponName}." : description;
            possibleRarities = ContentRaritySet.All();
            tags = definition != null ? definition.Tags : ContentTagSet.With(ContentTag.Projectile, ContentTag.Damage);
            isActive = true;
            availabilityDefaultsVersion = AvailabilityDefaultsVersion;
            maxLevel = 999;
            this.iconColor = iconColor;
            weaponId = targetWeaponId;
            statModifiers = new List<UpgradeStatModifierDefinition>();
            if (modifiers != null)
            {
                statModifiers.AddRange(modifiers);
            }

            EnsureWorkflowDefaults();
        }

        public bool EnsureWorkflowDefaults()
        {
            bool changed = false;
            changed |= EnsureString(ref upgradeId, "move_speed");
            changed |= EnsureString(ref displayName, "Move Speed");
            changed |= EnsureString(ref shortDescription, "Improve a run stat.");
            changed |= EnsureAvailabilityDefaults();
            if (maxLevel < 1)
            {
                maxLevel = 1;
                changed = true;
            }

            statModifiers ??= new List<UpgradeStatModifierDefinition>();
            changed |= possibleRarities.EnsureAny(ContentRarity.Common, useAllWhenEmpty: true);
            changed |= SyncWeaponIdFromDefinition();
            if (weaponDefinition == null)
            {
                changed |= EnsureString(ref weaponId, "juggling_ball");
            }

            return changed;
        }

        public bool SyncWeaponIdFromDefinition()
        {
            if (weaponDefinition == null || string.IsNullOrWhiteSpace(weaponDefinition.Id) || weaponId == weaponDefinition.Id)
            {
                return false;
            }

            weaponId = weaponDefinition.Id;
            return true;
        }

        public bool SyncWeaponDefinitionFromId()
        {
#if UNITY_EDITOR
            WeaponDefinition weapon = FindCatalogWeapon(weaponId);
            if (weapon == null || ReferenceEquals(weaponDefinition, weapon))
            {
                return false;
            }

            weaponDefinition = weapon;
            return true;
#else
            return false;
#endif
        }

        [TabGroup(Tabs, "Effect"), BoxGroup(Tabs + "/Effect/Weapon"), Button("Sync Reference From Target Weapon"), PropertyOrder(200)]
        private void SyncReferenceFromTargetWeaponButton()
        {
#if UNITY_EDITOR
            if (SyncWeaponDefinitionFromId())
            {
                EditorUtility.SetDirty(this);
            }
#endif
        }

        private bool EnsureAvailabilityDefaults()
        {
            if (availabilityDefaultsVersion >= AvailabilityDefaultsVersion)
            {
                return false;
            }

            isActive = true;
            availabilityDefaultsVersion = AvailabilityDefaultsVersion;
            return true;
        }

        private void OnValidate()
        {
            EnsureWorkflowDefaults();
        }

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Core"), Button("Copy ID"), PropertyOrder(2)]
        private void CopyId()
        {
            GUIUtility.systemCopyBuffer = Id;
        }

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Core"), Button("Regenerate ID From Display Name"), PropertyOrder(3)]
        private void RegenerateIdFromDisplayName()
        {
            upgradeId = ContentIdSuggestionRules.NormalizeBase(displayName, "upgrade");
            EnsureWorkflowDefaults();
        }

        private bool HasUpgradeId(string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        private bool HasDisplayName(string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        private IEnumerable<ValueDropdownItem<string>> WeaponIdOptions()
        {
#if UNITY_EDITOR
            IReadOnlyList<WeaponDefinition> weapons = LoadCatalogWeapons();
            bool containsCurrent = false;
            if (weapons != null)
            {
                for (int i = 0; i < weapons.Count; i++)
                {
                    WeaponDefinition weapon = weapons[i];
                    if (weapon == null || string.IsNullOrWhiteSpace(weapon.Id))
                    {
                        continue;
                    }

                    containsCurrent |= string.Equals(weapon.Id, weaponId, System.StringComparison.Ordinal);
                    string label = string.IsNullOrWhiteSpace(weapon.DisplayName)
                        ? weapon.Id
                        : $"{weapon.DisplayName} ({weapon.Id})";
                    yield return new ValueDropdownItem<string>(label, weapon.Id);
                }
            }

            if (!containsCurrent && !string.IsNullOrWhiteSpace(weaponId))
            {
                yield return new ValueDropdownItem<string>($"{weaponId} (missing from catalog)", weaponId);
            }
#else
            yield break;
#endif
        }

#if UNITY_EDITOR
        private static IReadOnlyList<WeaponDefinition> LoadCatalogWeapons()
        {
            WeaponCatalog catalog = AssetDatabase.LoadAssetAtPath<WeaponCatalog>(WeaponCatalogPath);
            return catalog?.AvailableWeapons;
        }

        private static WeaponDefinition FindCatalogWeapon(string id)
        {
            IReadOnlyList<WeaponDefinition> weapons = LoadCatalogWeapons();
            if (string.IsNullOrWhiteSpace(id) || weapons == null)
            {
                return null;
            }

            for (int i = 0; i < weapons.Count; i++)
            {
                WeaponDefinition weapon = weapons[i];
                if (weapon != null && string.Equals(weapon.Id, id, System.StringComparison.Ordinal))
                {
                    return weapon;
                }
            }

            return null;
        }
#endif

        private static bool EnsureString(ref string value, string fallback)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            value = fallback;
            return true;
        }
    }
}
