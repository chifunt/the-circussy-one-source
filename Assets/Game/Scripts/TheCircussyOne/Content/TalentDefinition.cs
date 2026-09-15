using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using TheCircussyOne.Stats;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TheCircussyOne.Content
{
    public enum TalentPoolKind
    {
        Shared,
        PerformerSpecific
    }

    public enum TalentRepeatPolicy
    {
        Infinite,
        Capped
    }

    [Serializable]
    public sealed class TalentPerformerAccess
    {
        [HorizontalGroup("Access", Width = 24), HideLabel]
        public bool enabled = true;

        [HideInInspector]
        public PerformerDefinition performer;

        [HideInInspector]
        public string performerId;

        [HorizontalGroup("Access"), ShowInInspector, ReadOnly, HideLabel]
        private string PerformerName => performer != null
            ? $"{performer.DisplayName} ({performer.Id})"
            : !string.IsNullOrWhiteSpace(performerId)
                ? performerId
                : "Missing Performer";

        public string PerformerId => performer != null && !string.IsNullOrWhiteSpace(performer.Id) ? performer.Id : performerId;

        public bool Matches(PerformerDefinition candidate)
        {
            if (candidate == null)
            {
                return false;
            }

            return ReferenceEquals(performer, candidate)
                || (!string.IsNullOrWhiteSpace(PerformerId) && string.Equals(PerformerId, candidate.Id, StringComparison.Ordinal));
        }

        public bool SyncId()
        {
            if (performer == null || string.IsNullOrWhiteSpace(performer.Id) || performerId == performer.Id)
            {
                return false;
            }

            performerId = performer.Id;
            return true;
        }
    }

    [CreateAssetMenu(menuName = "The Circussy One/Content/Talent Definition", fileName = "TalentDefinition")]
    public sealed class TalentDefinition : SerializedScriptableObject, IActivatableContentDefinition
    {
        private const string Tabs = "Talent";
        private const int AvailabilityDefaultsVersion = 1;
#if UNITY_EDITOR
        private const string PerformerCatalogPath = "Assets/Game/ScriptableObjects/Balance/Performers/PerformerCatalog.asset";
#endif

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Core"), LabelWidth(180), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.BalanceColor"), PropertyOrder(0)]
        [ValidateInput(nameof(HasTalentId), "Talent id is required."), ReadOnly]
        [Tooltip("Stable runtime ID. Use Regenerate ID only when intentionally migrating references.")]
        public string talentId = "footwork";

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Core"), LabelWidth(180), PropertyOrder(1)]
        [ValidateInput(nameof(HasDisplayName), "Display name is required.")]
        public string displayName = "Footwork";

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Core"), LabelWidth(180), MultiLineProperty(2), PropertyOrder(4)]
        public string shortDescription = "+10% movement speed.";

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Classification"), LabelWidth(180), InlineProperty]
        public ContentRaritySet possibleRarities = ContentRaritySet.All();

        [HideInInspector]
        public ContentTagSet tags = ContentTagSet.With(ContentTag.Movement, ContentTag.Utility);

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Classification"), InfoBox("Computed from mechanics and effects. Change source fields instead.", InfoMessageType.Info), ShowInInspector, ReadOnly, LabelText("Computed Tags"), PropertyOrder(10)]
        private string ComputedTags => ComputedContentTagRules.Format(Tags);

        [HideInInspector]
        public bool isActive = true;

        [TabGroup(Tabs, "Pool"), BoxGroup(Tabs + "/Pool/Rules"), LabelWidth(180), EnumToggleButtons]
        [OnValueChanged(nameof(SyncPoolAuthoringFromCatalog))]
        public TalentPoolKind poolKind = TalentPoolKind.Shared;

        [HideInInspector]
        public PerformerDefinition performerDefinition;

        [TabGroup(Tabs, "Pool"), BoxGroup(Tabs + "/Pool/Performer-Specific"), LabelText("Performer"), ShowIf(nameof(IsPerformerSpecific))]
        [ValueDropdown(nameof(PerformerIdOptions), DropdownTitle = "Performers")]
        [OnValueChanged(nameof(SyncPerformerDefinitionFromId))]
        public string performerId;

        [TabGroup(Tabs, "Pool"), BoxGroup(Tabs + "/Pool/Shared Performer Access"), LabelText("Performers"), ShowIf(nameof(IsShared)), ListDrawerSettings(DraggableItems = false)]
        [OnInspectorInit(nameof(AutoSyncSharedPerformerAccessFromCatalog))]
        public List<TalentPerformerAccess> sharedPerformerAccess = new();

        [TabGroup(Tabs, "Progression"), BoxGroup(Tabs + "/Progression/Repeat"), LabelWidth(180), EnumToggleButtons]
        public TalentRepeatPolicy repeatPolicy = TalentRepeatPolicy.Infinite;

        [TabGroup(Tabs, "Progression"), BoxGroup(Tabs + "/Progression/Repeat"), LabelWidth(180), ShowIf(nameof(IsCapped))]
        [Min(1)] public int maxLevel = 5;

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Card"), LabelWidth(180), AssetSelector, PreviewField(72)]
        [Tooltip("Square transparent icon used on talent cards. Performer-specific talents may leave this empty and inherit the performer portrait.")]
        public Sprite iconSprite;

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Card"), LabelWidth(180)]
        [LabelText("Fallback Color")]
        public Color iconColor = new(0.7f, 0.9f, 1f, 1f);

        [TabGroup(Tabs, "Effect"), BoxGroup(Tabs + "/Effect/Stat Modifiers"), LabelWidth(180)]
        public List<UpgradeStatModifierDefinition> statModifiers = new();

        [TabGroup(Tabs, "Diagnostics"), ShowInInspector, ReadOnly, LabelText("Runtime Summary"), PropertyOrder(100)]
        private string RuntimeSummary => $"{displayName} [{LevelUpRarities}] {PoolSummary} {repeatPolicy}: {shortDescription}";

        public string Id => talentId;
        public string DisplayName => displayName;
        public ContentTagSet Tags => ComputedContentTagRules.For(this);
        public bool IsActive => isActive;
        public ContentRaritySet LevelUpRarities => possibleRarities.OrAll();
        public bool IsShared => poolKind == TalentPoolKind.Shared;
        public int EffectiveMaxLevel => repeatPolicy == TalentRepeatPolicy.Capped ? Mathf.Max(1, maxLevel) : int.MaxValue;
        public string PoolSummary => poolKind == TalentPoolKind.PerformerSpecific
            ? $"Performer-specific: {(!string.IsNullOrWhiteSpace(performerId) ? performerId : "Unassigned")}"
            : sharedPerformerAccess != null && sharedPerformerAccess.Count > 0
                ? $"Shared: {EnabledSharedPerformerCount()} performer(s)"
                : "Shared: all performers";
        private bool IsPerformerSpecific => poolKind == TalentPoolKind.PerformerSpecific;
        private bool IsCapped => repeatPolicy == TalentRepeatPolicy.Capped;

        [SerializeField, HideInInspector] private int availabilityDefaultsVersion;

        public void ApplyDefaults(
            string id,
            string name,
            string description,
            ContentTagSet tags,
            Color iconColor,
            params UpgradeStatModifierDefinition[] modifiers)
        {
            talentId = id;
            displayName = name;
            shortDescription = description;
            possibleRarities = ContentRaritySet.All();
            this.tags = tags ?? ContentTagSet.With(ContentTag.Utility);
            isActive = true;
            availabilityDefaultsVersion = AvailabilityDefaultsVersion;
            poolKind = TalentPoolKind.Shared;
            performerDefinition = null;
            performerId = string.Empty;
            sharedPerformerAccess = new List<TalentPerformerAccess>();
            repeatPolicy = TalentRepeatPolicy.Infinite;
            maxLevel = 5;
            this.iconColor = iconColor;
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
            changed |= EnsureString(ref talentId, "footwork");
            changed |= EnsureString(ref displayName, "Footwork");
            changed |= EnsureString(ref shortDescription, "Improve a performer talent.");
            changed |= EnsureAvailabilityDefaults();
            if (repeatPolicy == TalentRepeatPolicy.Capped && maxLevel < 1)
            {
                maxLevel = 1;
                changed = true;
            }

            changed |= possibleRarities.EnsureAny(ContentRarity.Common, useAllWhenEmpty: true);
            changed |= SyncPerformerIdFromDefinition();
            changed |= EnsureSharedPerformerAccessIds();
            statModifiers ??= new List<UpgradeStatModifierDefinition>();
            return changed;
        }

        public bool AllowsPerformer(PerformerDefinition performer)
        {
            if (poolKind == TalentPoolKind.PerformerSpecific)
            {
                return MatchesPerformer(performer);
            }

            if (sharedPerformerAccess == null || sharedPerformerAccess.Count == 0)
            {
                return true;
            }

            if (performer == null)
            {
                return false;
            }

            for (int i = 0; i < sharedPerformerAccess.Count; i++)
            {
                TalentPerformerAccess access = sharedPerformerAccess[i];
                if (access != null && access.enabled && access.Matches(performer))
                {
                    return true;
                }
            }

            return false;
        }

        public bool HasDirectPerformerReference(PerformerDefinition performer)
        {
            if (performer == null)
            {
                return false;
            }

            if (MatchesPerformer(performer))
            {
                return true;
            }

            if (sharedPerformerAccess == null)
            {
                return false;
            }

            for (int i = 0; i < sharedPerformerAccess.Count; i++)
            {
                TalentPerformerAccess access = sharedPerformerAccess[i];
                if (access != null && access.Matches(performer))
                {
                    return true;
                }
            }

            return false;
        }

        public bool EnsureSharedPerformerAccess(IReadOnlyList<PerformerDefinition> performers, bool defaultEnabled = true)
        {
            sharedPerformerAccess ??= new List<TalentPerformerAccess>();
            if (performers == null)
            {
                return EnsureSharedPerformerAccessIds();
            }

            bool changed = EnsureSharedPerformerAccessIds();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            for (int i = sharedPerformerAccess.Count - 1; i >= 0; i--)
            {
                TalentPerformerAccess access = sharedPerformerAccess[i];
                if (access == null || string.IsNullOrWhiteSpace(access.PerformerId) || !seen.Add(access.PerformerId))
                {
                    sharedPerformerAccess.RemoveAt(i);
                    changed = true;
                }
            }

            for (int characterIndex = 0; characterIndex < performers.Count; characterIndex++)
            {
                PerformerDefinition performer = performers[characterIndex];
                if (performer == null || string.IsNullOrWhiteSpace(performer.Id) || seen.Contains(performer.Id))
                {
                    continue;
                }

                sharedPerformerAccess.Add(new TalentPerformerAccess
                {
                    enabled = defaultEnabled,
                    performer = performer,
                    performerId = performer.Id
                });
                seen.Add(performer.Id);
                changed = true;
            }

            return changed;
        }

        public bool SetSharedPerformerAccess(PerformerDefinition performer, bool enabled)
        {
            if (performer == null || string.IsNullOrWhiteSpace(performer.Id))
            {
                return false;
            }

            sharedPerformerAccess ??= new List<TalentPerformerAccess>();
            for (int i = 0; i < sharedPerformerAccess.Count; i++)
            {
                TalentPerformerAccess access = sharedPerformerAccess[i];
                if (access != null && access.Matches(performer))
                {
                    if (access.enabled == enabled)
                    {
                        return false;
                    }

                    access.enabled = enabled;
                    return true;
                }
            }

            sharedPerformerAccess.Add(new TalentPerformerAccess
            {
                enabled = enabled,
                performer = performer,
                performerId = performer.Id
            });
            return true;
        }

        public bool SyncPerformerIdFromDefinition()
        {
            if (performerDefinition == null || string.IsNullOrWhiteSpace(performerDefinition.Id) || performerId == performerDefinition.Id)
            {
                return false;
            }

            performerId = performerDefinition.Id;
            return true;
        }

        public bool SyncPerformerDefinitionFromId()
        {
#if UNITY_EDITOR
            PerformerDefinition performer = FindCatalogPerformer(performerId);
            if (performer == null || ReferenceEquals(performerDefinition, performer))
            {
                return false;
            }

            performerDefinition = performer;
            return true;
#else
            return false;
#endif
        }

        private bool MatchesPerformer(PerformerDefinition performer)
        {
            if (performer == null)
            {
                return false;
            }

            return ReferenceEquals(performerDefinition, performer)
                || (!string.IsNullOrWhiteSpace(performerId) && string.Equals(performerId, performer.Id, StringComparison.Ordinal));
        }

        private bool EnsureSharedPerformerAccessIds()
        {
            bool changed = false;
            sharedPerformerAccess ??= new List<TalentPerformerAccess>();
            for (int i = 0; i < sharedPerformerAccess.Count; i++)
            {
                changed |= sharedPerformerAccess[i]?.SyncId() ?? false;
            }

            return changed;
        }

        private int EnabledSharedPerformerCount()
        {
            if (sharedPerformerAccess == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < sharedPerformerAccess.Count; i++)
            {
                if (sharedPerformerAccess[i]?.enabled == true)
                {
                    count++;
                }
            }

            return count;
        }

        [TabGroup(Tabs, "Pool"), BoxGroup(Tabs + "/Pool/Performer-Specific"), Button("Sync Reference From Performer"), ShowIf(nameof(IsPerformerSpecific)), PropertyOrder(200)]
        private void SyncReferenceFromPerformerButton()
        {
#if UNITY_EDITOR
            if (SyncPerformerDefinitionFromId())
            {
                EditorUtility.SetDirty(this);
            }
#endif
        }

        private void SyncPoolAuthoringFromCatalog()
        {
#if UNITY_EDITOR
            if (poolKind == TalentPoolKind.Shared)
            {
                AutoSyncSharedPerformerAccessFromCatalog();
                return;
            }

            if (SyncPerformerDefinitionFromId())
            {
                EditorUtility.SetDirty(this);
            }
#endif
        }

        private void AutoSyncSharedPerformerAccessFromCatalog()
        {
#if UNITY_EDITOR
            if (poolKind != TalentPoolKind.Shared)
            {
                return;
            }

            IReadOnlyList<PerformerDefinition> performers = LoadCatalogPerformers();
            if (EnsureSharedPerformerAccess(performers, defaultEnabled: true))
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
            talentId = ContentIdSuggestionRules.NormalizeBase(displayName, "talent");
            EnsureWorkflowDefaults();
        }

        private bool HasTalentId(string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        private bool HasDisplayName(string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        private IEnumerable<ValueDropdownItem<string>> PerformerIdOptions()
        {
#if UNITY_EDITOR
            IReadOnlyList<PerformerDefinition> performers = LoadCatalogPerformers();
            bool containsCurrent = false;
            if (performers != null)
            {
                for (int i = 0; i < performers.Count; i++)
                {
                    PerformerDefinition performer = performers[i];
                    if (performer == null || string.IsNullOrWhiteSpace(performer.Id))
                    {
                        continue;
                    }

                    containsCurrent |= string.Equals(performer.Id, performerId, StringComparison.Ordinal);
                    string label = string.IsNullOrWhiteSpace(performer.DisplayName)
                        ? performer.Id
                        : $"{performer.DisplayName} ({performer.Id})";
                    yield return new ValueDropdownItem<string>(label, performer.Id);
                }
            }

            if (!containsCurrent && !string.IsNullOrWhiteSpace(performerId))
            {
                yield return new ValueDropdownItem<string>($"{performerId} (missing from catalog)", performerId);
            }
#else
            yield break;
#endif
        }

#if UNITY_EDITOR
        private static IReadOnlyList<PerformerDefinition> LoadCatalogPerformers()
        {
            PerformerCatalog catalog = AssetDatabase.LoadAssetAtPath<PerformerCatalog>(PerformerCatalogPath);
            return catalog?.Performers;
        }

        private static PerformerDefinition FindCatalogPerformer(string id)
        {
            IReadOnlyList<PerformerDefinition> performers = LoadCatalogPerformers();
            if (string.IsNullOrWhiteSpace(id) || performers == null)
            {
                return null;
            }

            for (int i = 0; i < performers.Count; i++)
            {
                PerformerDefinition performer = performers[i];
                if (performer != null && string.Equals(performer.Id, id, StringComparison.Ordinal))
                {
                    return performer;
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
