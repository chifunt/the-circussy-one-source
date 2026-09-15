using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using TheCircussyOne.Stats;

namespace TheCircussyOne.Content
{
    public enum PerformerPassiveKind
    {
        None,
        StaticStatModifiers
    }

    [CreateAssetMenu(menuName = "The Circussy One/Content/Performer Definition", fileName = "PerformerDefinition")]
    public sealed class PerformerDefinition : SerializedScriptableObject, IActivatableContentDefinition
    {
        private const string Tabs = "Performer";
        private const int AvailabilityDefaultsVersion = 1;

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Core"), LabelWidth(180), GUIColor("@TheCircussyOne.Config.ConfigInspectorStyle.PlayerColor"), PropertyOrder(0)]
        [ValidateInput(nameof(HasPerformerId), "Performer id is required."), ReadOnly]
        [Tooltip("Stable runtime ID. Use Regenerate ID only when intentionally migrating references.")]
        public string performerId = "devsample_sam";

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Core"), LabelWidth(180), PropertyOrder(1)]
        [ValidateInput(nameof(HasDisplayName), "Display name is required.")]
        public string displayName = "Devsample Sam";

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Core"), LabelWidth(180), LabelText("Performer Title"), PropertyOrder(2)]
        [Tooltip("Stage title shown separately from the performer's name, for example 'The Ball Juggler'.")]
        public string performerTitle = "The Dev Sample";

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Core"), LabelWidth(180), MultiLineProperty(2), PropertyOrder(5)]
        public string shortDescription = "Baseline performer with a simple projectile act.";

        [HideInInspector]
        public ContentTagSet tags = ContentTagSet.With(ContentTag.Projectile, ContentTag.Utility);

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Classification"), InfoBox("Computed from mechanics and effects. Change source fields instead.", InfoMessageType.Info), ShowInInspector, ReadOnly, LabelText("Computed Tags"), PropertyOrder(10)]
        private string ComputedTags => ComputedContentTagRules.Format(Tags);

        [HideInInspector]
        public bool isActive = true;

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Unlocks"), LabelWidth(180)]
        public bool unlocked = true;

        [TabGroup(Tabs, "Loadout"), BoxGroup(Tabs + "/Loadout/Starting Weapon"), LabelWidth(180), AssetSelector]
        public WeaponDefinition startingWeapon;

        [TabGroup(Tabs, "Stats"), BoxGroup(Tabs + "/Stats/Base Modifiers"), LabelWidth(180)]
        public List<UpgradeStatModifierDefinition> baseStatModifiers = new();

        [TabGroup(Tabs, "Passive"), BoxGroup(Tabs + "/Passive/Summary"), LabelWidth(180), MultiLineProperty(2)]
        public string passiveDescription = "No special passive.";

        [TabGroup(Tabs, "Passive"), BoxGroup(Tabs + "/Passive/Runtime"), LabelWidth(180), EnumToggleButtons]
        public PerformerPassiveKind passiveKind = PerformerPassiveKind.None;

        [TabGroup(Tabs, "Passive"), BoxGroup(Tabs + "/Passive/Runtime"), LabelWidth(180), ShowIf(nameof(UsesStaticPassive))]
        public List<UpgradeStatModifierDefinition> passiveStatModifiers = new();

        [HideInInspector]
        public bool includeSharedTalents = true;

        [HideInInspector]
        public List<TalentDefinition> uniqueTalents = new();

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Selection Card"), LabelWidth(180), AssetSelector, PreviewField(72)]
        [Tooltip("Square transparent portrait/icon used by performer select and HUD/content cards. Color remains a fallback/accent when no sprite is assigned.")]
        public Sprite portraitSprite;

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Selection Card"), LabelWidth(180)]
        [LabelText("Fallback Color")]
        public Color portraitColor = new(0.95f, 0.78f, 0.28f, 1f);

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Runtime Model"), LabelWidth(180), AssetSelector]
        public GameObject worldPrefab;

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Runtime Model"), HideLabel, InlineProperty]
        public ActorModelTransformProfile modelTransform = new();

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Animations"), HideLabel, InlineProperty]
        public PerformerAnimationProfile animation = new();

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Procedural Bop"), HideLabel, InlineProperty]
        public PerformerMotionVisualProfile motionVisuals = new();

        [TabGroup(Tabs, "Theme"), BoxGroup(Tabs + "/Theme/Summary"), LabelWidth(180), MultiLineProperty(3)]
        public string talentThemeSummary = "Projectile rhythm, bounce, and precision talents.";

        [TabGroup(Tabs, "Diagnostics"), ShowInInspector, ReadOnly, LabelText("Runtime Summary"), PropertyOrder(100)]
        private string RuntimeSummary => $"{displayName}: {StartingWeaponName}, talents are assigned on Talent assets";

        public string Id => performerId;
        public string DisplayName => displayName;
        public string PerformerTitle => string.IsNullOrWhiteSpace(performerTitle) ? string.Empty : performerTitle.Trim();
        public ContentTagSet Tags => ComputedContentTagRules.For(this);
        public bool IsActive => isActive;
        public bool UsesStaticPassive => passiveKind == PerformerPassiveKind.StaticStatModifiers;
        public string StartingWeaponName => startingWeapon != null ? startingWeapon.DisplayName : "Missing Weapon";

        [SerializeField, HideInInspector] private int availabilityDefaultsVersion;

        public void ApplyDefaults(
            string id,
            string name,
            string description,
            ContentTagSet tags,
            WeaponDefinition startingWeapon,
            Color portraitColor,
            string passiveDescription,
            PerformerPassiveKind passiveKind,
            string talentThemeSummary,
            IReadOnlyList<UpgradeStatModifierDefinition> baseModifiers,
            IReadOnlyList<UpgradeStatModifierDefinition> passiveModifiers,
            params TalentDefinition[] uniqueTalents)
        {
            ApplyDefaultsWithTitle(
                id,
                name,
                string.Empty,
                description,
                tags,
                startingWeapon,
                portraitColor,
                passiveDescription,
                passiveKind,
                talentThemeSummary,
                baseModifiers,
                passiveModifiers,
                uniqueTalents);
        }

        public void ApplyDefaultsWithTitle(
            string id,
            string name,
            string title,
            string description,
            ContentTagSet tags,
            WeaponDefinition startingWeapon,
            Color portraitColor,
            string passiveDescription,
            PerformerPassiveKind passiveKind,
            string talentThemeSummary,
            IReadOnlyList<UpgradeStatModifierDefinition> baseModifiers,
            IReadOnlyList<UpgradeStatModifierDefinition> passiveModifiers,
            params TalentDefinition[] uniqueTalents)
        {
            performerId = id;
            displayName = name;
            performerTitle = title ?? string.Empty;
            shortDescription = description;
            this.tags = tags ?? ContentTagSet.With(ContentTag.Utility);
            isActive = true;
            availabilityDefaultsVersion = AvailabilityDefaultsVersion;
            unlocked = true;
            this.startingWeapon = startingWeapon;
            this.portraitColor = portraitColor;
            this.passiveDescription = passiveDescription;
            this.passiveKind = passiveKind;
            this.talentThemeSummary = talentThemeSummary;
            includeSharedTalents = true;
            baseStatModifiers = CopyModifiers(baseModifiers);
            passiveStatModifiers = CopyModifiers(passiveModifiers);
            this.uniqueTalents = new List<TalentDefinition>();

            EnsureWorkflowDefaults();
        }

        public bool EnsureWorkflowDefaults()
        {
            bool changed = false;
            changed |= EnsureString(ref performerId, "devsample_sam");
            changed |= EnsureString(ref displayName, "Devsample Sam");
            changed |= EnsureString(ref shortDescription, "A playable performer.");
            changed |= EnsureAvailabilityDefaults();
            changed |= EnsureString(ref passiveDescription, "No special passive.");
            changed |= EnsureString(ref talentThemeSummary, "Shared talents.");
            baseStatModifiers ??= new List<UpgradeStatModifierDefinition>();
            passiveStatModifiers ??= new List<UpgradeStatModifierDefinition>();
            uniqueTalents ??= new List<TalentDefinition>();
            modelTransform ??= new ActorModelTransformProfile();
            animation ??= new PerformerAnimationProfile();
            motionVisuals ??= new PerformerMotionVisualProfile();
            changed |= modelTransform.EnsureWorkflowDefaults();
            changed |= animation.EnsureWorkflowDefaults();
            changed |= motionVisuals.EnsureWorkflowDefaults();
            return changed;
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

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Core"), Button("Copy ID"), PropertyOrder(3)]
        private void CopyId()
        {
            GUIUtility.systemCopyBuffer = Id;
        }

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Core"), Button("Regenerate ID From Display Name"), PropertyOrder(4)]
        private void RegenerateIdFromDisplayName()
        {
            performerId = ContentIdSuggestionRules.NormalizeBase(displayName, "performer");
            EnsureWorkflowDefaults();
        }

        private bool HasPerformerId(string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        private bool HasDisplayName(string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        private static List<UpgradeStatModifierDefinition> CopyModifiers(IReadOnlyList<UpgradeStatModifierDefinition> source)
        {
            var copy = new List<UpgradeStatModifierDefinition>();
            if (source == null)
            {
                return copy;
            }

            for (int i = 0; i < source.Count; i++)
            {
                copy.Add(source[i]);
            }

            return copy;
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
    }
}
