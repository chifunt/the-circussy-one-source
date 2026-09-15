using Sirenix.OdinInspector;
using UnityEngine;
using TheCircussyOne.Runtime;

namespace TheCircussyOne.Content
{
    [CreateAssetMenu(menuName = "The Circussy One/Content/Headliner Definition", fileName = "HeadlinerDefinition")]
    public sealed class HeadlinerDefinition : SerializedScriptableObject, IActivatableContentDefinition
    {
        private const string Tabs = "Headliner";

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Core"), LabelWidth(190), ReadOnly]
        [ValidateInput(nameof(HasHeadlinerId), "Headliner id is required.")]
        public string headlinerId = "opening_headliner";

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Core"), LabelWidth(190)]
        [ValidateInput(nameof(HasDisplayName), "Display name is required.")]
        public string displayName = "Opening Headliner";

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Classification"), LabelWidth(190)]
        public ContentRarity rarity = ContentRarity.Rare;

        [HideInInspector]
        public ContentTagSet tags = ContentTagSet.With(ContentTag.Boss, ContentTag.Elite, ContentTag.Physical);

        [HideInInspector]
        public bool isActive = true;

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Classification"), InfoBox("Computed from the Headliner role and referenced enemy actor.", InfoMessageType.Info), ShowInInspector, ReadOnly, LabelText("Computed Tags")]
        private string ComputedTags => ComputedContentTagRules.Format(Tags);

        [TabGroup(Tabs, "Actor"), BoxGroup(Tabs + "/Actor/Enemy Actor"), LabelWidth(190), AssetSelector]
        [Tooltip("Enemy-style actor used for health, hurtboxes, contact damage, movement, model, and drops. It does not need to be in the normal EnemyCatalog.")]
        public EnemyDefinition enemyActor;

        [TabGroup(Tabs, "Act"), BoxGroup(Tabs + "/Act/Selection"), LabelWidth(190), Min(1)]
        [Tooltip("1-based Act number. If no exact match exists, the catalog falls back to the first active Headliner.")]
        public int actNumber = 1;

        [TabGroup(Tabs, "Spawn"), BoxGroup(Tabs + "/Spawn/Placement"), LabelWidth(190), SuffixLabel("u")]
        [Min(0f)] public float spawnDistanceFromPlayer = 12f;

        [TabGroup(Tabs, "Spawn"), BoxGroup(Tabs + "/Spawn/Placement"), LabelWidth(190), SuffixLabel("u")]
        [Min(0f)] public float spawnGroundClearance = 0.05f;

        [TabGroup(Tabs, "Spawn"), BoxGroup(Tabs + "/Spawn/Surface Probe"), LabelWidth(190), SuffixLabel("u")]
        [Min(0f)] public float spawnProbeHeight = 12f;

        [TabGroup(Tabs, "Spawn"), BoxGroup(Tabs + "/Spawn/Surface Probe"), LabelWidth(190), SuffixLabel("u")]
        [Min(0f)] public float spawnProbeDepth = 24f;

        [TabGroup(Tabs, "Diagnostics"), ShowInInspector, ReadOnly, LabelText("Runtime Summary"), PropertyOrder(100)]
        private string RuntimeSummary => $"{displayName} / Act {actNumber} / actor {(enemyActor != null ? enemyActor.DisplayName : "None")}";

        public string Id => headlinerId;
        public string DisplayName => displayName;
        public ContentTagSet Tags => ComputedContentTagRules.For(this);
        public bool IsActive => isActive;

        public bool EnsureWorkflowDefaults()
        {
            bool changed = false;
            changed |= EnsureString(ref headlinerId, "opening_headliner");
            changed |= EnsureString(ref displayName, "Opening Headliner");
            changed |= EnsureMinimum(ref actNumber, 1, 1);
            changed |= EnsureMinimum(ref spawnDistanceFromPlayer, 12f, 0f);
            changed |= EnsureMinimum(ref spawnGroundClearance, 0.05f, 0f);
            changed |= EnsureMinimum(ref spawnProbeHeight, 12f, 0f);
            changed |= EnsureMinimum(ref spawnProbeDepth, 24f, 0f);
            return changed;
        }

        public void ApplyDefaults(EnemyDefinition actor)
        {
            headlinerId = "opening_headliner";
            displayName = "Opening Headliner";
            rarity = ContentRarity.Rare;
            tags = ContentTagSet.With(ContentTag.Boss, ContentTag.Elite, ContentTag.Physical);
            isActive = true;
            enemyActor = actor;
            actNumber = 1;
            spawnDistanceFromPlayer = 12f;
            spawnGroundClearance = 0.05f;
            spawnProbeHeight = 12f;
            spawnProbeDepth = 24f;
            EnsureWorkflowDefaults();
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
            headlinerId = ContentIdSuggestionRules.NormalizeBase(displayName, "headliner");
            EnsureWorkflowDefaults();
        }

        private bool HasHeadlinerId(string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        private bool HasDisplayName(string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        private static bool EnsureString(ref string value, string fallback)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            value = fallback;
            return true;
        }

        private static bool EnsureMinimum(ref int value, int defaultValue, int minimum)
        {
            if (value >= minimum)
            {
                return false;
            }

            value = defaultValue;
            return true;
        }

        private static bool EnsureMinimum(ref float value, float defaultValue, float minimum)
        {
            if (value >= minimum)
            {
                return false;
            }

            value = defaultValue;
            return true;
        }
    }
}
