using Sirenix.OdinInspector;
using TheCircussyOne.Rules;
using UnityEngine;

namespace TheCircussyOne.Content
{
    public enum ChestKind
    {
        Open = 0,
        Locked = 1,
        Premium = 2,
        SpecialEnemy = 3
    }

    [CreateAssetMenu(menuName = "The Circussy One/Content/Chest Definition", fileName = "ChestDefinition")]
    public sealed class ChestDefinition : SerializedScriptableObject, IActivatableContentDefinition
    {
        private const string Tabs = "Chest";
        private const int AvailabilityDefaultsVersion = 1;
        private const int VisualDefaultsVersion = 4;

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Core"), LabelWidth(180), ReadOnly]
        [ValidateInput(nameof(HasChestId), "Chest id is required.")]
        public string chestId = "locked_chest";

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Core"), LabelWidth(180)]
        [ValidateInput(nameof(HasDisplayName), "Display name is required.")]
        public string displayName = "Locked Chest";

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Core"), LabelWidth(180), MultiLineProperty(2)]
        public string shortDescription = "Spend Tickets to claim one item.";

        [HideInInspector] public ContentTagSet tags = ContentTagSet.With(ContentTag.Chest, ContentTag.Economy);

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Classification"), InfoBox("Computed from chest role. Change source fields instead.", InfoMessageType.Info), ShowInInspector, ReadOnly, LabelText("Computed Tags")]
        private string ComputedTags => ComputedContentTagRules.Format(Tags);

        [HideInInspector] public bool isActive = true;

        [TabGroup(Tabs, "Interaction"), BoxGroup(Tabs + "/Interaction/Rules"), LabelWidth(180), EnumToggleButtons]
        public ChestKind kind = ChestKind.Locked;

        [TabGroup(Tabs, "Interaction"), BoxGroup(Tabs + "/Interaction/Rules"), LabelWidth(180), Min(0.05f), SuffixLabel("sec", true)]
        public float holdSeconds = 1f;

        [TabGroup(Tabs, "Interaction"), BoxGroup(Tabs + "/Interaction/Cost"), LabelWidth(180), Min(0), SuffixLabel("Tickets", true)]
        public int ticketCost = 35;

        [TabGroup(Tabs, "Interaction"), BoxGroup(Tabs + "/Interaction/Prompt"), LabelWidth(180)]
        public string promptText = "Open Chest";

        [TabGroup(Tabs, "Placement"), BoxGroup(Tabs + "/Placement/Collision"), LabelWidth(180), InlineProperty]
        public WorldPropPlacementProfile placement = WorldPropPlacementProfile.ChestDefault();

        [TabGroup(Tabs, "Reward"), BoxGroup(Tabs + "/Reward/Rarity Weights"), LabelWidth(180)]
        public ContentRarityWeightTable itemRarityWeights = ContentRarityWeightTable.NormalChestDefault();

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Model"), LabelWidth(180), AssetSelector, PreviewField(72)]
        public GameObject visualPrefab;

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Placeholder"), LabelWidth(180)]
        public Color visualColor = new(0.58f, 0.32f, 0.12f, 1f);

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Placeholder"), LabelWidth(180), Min(0.1f)]
        public float visualScale = 1f;

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Glow"), LabelWidth(180)]
        public Color emissionColor = new(1f, 0.78f, 0.26f, 1f);

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Glow"), LabelWidth(180), SuffixLabel("x", true), Min(0f)]
        public float emissionStrength = 0.35f;

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Target Outline"), LabelWidth(180)]
        public Color targetOutlineColor = new(1f, 0.95f, 0.6f, 1f);

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Target Outline"), LabelWidth(180), LabelText("Target Outline Width"), SuffixLabel("px", true), Min(0f)]
        public float targetOutlineThickness = 4f;

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Target Outline"), LabelWidth(180)]
        public Color insufficientTicketsOutlineColor = new(1f, 0.22f, 0.24f, 1f);

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Interaction Squash"), LabelWidth(180), SuffixLabel("x", true), Min(0f)]
        [InfoBox("Plays while the player is actively holding interact on this chest. Set amplitude to 0 to disable.")]
        public float interactionSquashStretchAmplitude = 0.08f;

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Interaction Squash"), LabelWidth(180), SuffixLabel("Hz", true), Min(0f)]
        public float interactionSquashStretchFrequency = 4.5f;

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Open Despawn"), LabelWidth(180), SuffixLabel("sec", true), Min(0.01f)]
        [InfoBox("After opening, the chest scales down with this easing, then deactivates so the claimed reward space clears. InBack gives a small pop before shrinking.")]
        public float openDisappearSeconds = 1f;

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Open Despawn"), LabelWidth(180), SuffixLabel("x", true), Min(0f)]
        public float openShrinkFinalScaleMultiplier = 0.02f;

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Open Despawn"), LabelWidth(180), InlineProperty]
        public EaseSettings openDisappearEase = EaseSettings.InBack;

        [SerializeField, HideInInspector] private int availabilityDefaultsVersion;
        [SerializeField, HideInInspector] private int visualDefaultsVersion;

        public string Id => chestId;
        public string DisplayName => displayName;
        public ContentTagSet Tags => ComputedContentTagRules.For(this);
        public bool IsActive => isActive;
        public int TicketCost => Mathf.Max(0, ticketCost);

        public void ApplyDefaults(
            string id,
            string name,
            string description,
            ChestKind kind,
            float holdSeconds,
            int ticketCost,
            string promptText,
            ContentRarityWeightTable itemRarityWeights,
            Color visualColor,
            float visualScale)
        {
            chestId = id;
            displayName = name;
            shortDescription = description;
            this.kind = kind;
            this.holdSeconds = holdSeconds;
            this.ticketCost = Mathf.Max(0, ticketCost);
            this.promptText = promptText;
            this.itemRarityWeights = itemRarityWeights;
            this.visualColor = visualColor;
            this.visualScale = visualScale;
            placement = WorldPropPlacementProfile.ChestDefault();
            ApplyVisualDefaults();
            isActive = true;
            availabilityDefaultsVersion = AvailabilityDefaultsVersion;
            visualDefaultsVersion = VisualDefaultsVersion;
            EnsureWorkflowDefaults();
        }

        public bool EnsureWorkflowDefaults()
        {
            bool changed = false;
            changed |= EnsureString(ref chestId, "locked_chest");
            changed |= EnsureString(ref displayName, "Locked Chest");
            changed |= EnsureString(ref shortDescription, "Spend Tickets to claim one item.");
            changed |= EnsureString(ref promptText, "Open Chest");
            changed |= EnsureAvailabilityDefaults();
            changed |= EnsureVisualDefaults();

            if (holdSeconds <= 0f)
            {
                holdSeconds = 1f;
                changed = true;
            }

            if (ticketCost < 0)
            {
                ticketCost = 0;
                changed = true;
            }

            if (!itemRarityWeights.IsValid)
            {
                itemRarityWeights = DefaultWeightsFor(kind);
                changed = true;
            }

            if (visualScale <= 0f)
            {
                visualScale = 1f;
                changed = true;
            }

            if (emissionStrength < 0f)
            {
                emissionStrength = 0f;
                changed = true;
            }

            if (targetOutlineThickness < 0f)
            {
                targetOutlineThickness = 0f;
                changed = true;
            }

            changed |= placement.EnsureDefaults(WorldPropPlacementProfile.ChestDefault());

            if (insufficientTicketsOutlineColor.a <= 0f)
            {
                insufficientTicketsOutlineColor = new Color(1f, 0.22f, 0.24f, 1f);
                changed = true;
            }

            return changed;
        }

        private void OnValidate()
        {
            EnsureWorkflowDefaults();
        }

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Core"), Button("Copy ID"), PropertyOrder(20)]
        private void CopyId()
        {
            GUIUtility.systemCopyBuffer = Id;
        }

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Core"), Button("Regenerate ID From Display Name"), PropertyOrder(21)]
        private void RegenerateIdFromDisplayName()
        {
            chestId = ContentIdSuggestionRules.NormalizeBase(displayName, "chest");
            EnsureWorkflowDefaults();
        }

        public static ContentRarityWeightTable DefaultWeightsFor(ChestKind kind)
        {
            return kind switch
            {
                ChestKind.Open => new ContentRarityWeightTable { common = 70, uncommon = 23, rare = 6, epic = 1, legendary = 0 },
                ChestKind.Premium => ContentRarityWeightTable.SpecialEnemyChestDefault(),
                ChestKind.SpecialEnemy => ContentRarityWeightTable.SpecialEnemyChestDefault(),
                _ => ContentRarityWeightTable.NormalChestDefault()
            };
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

        private void ApplyVisualDefaults()
        {
            emissionColor = Color.Lerp(visualColor, Color.white, 0.25f);
            emissionStrength = kind switch
            {
                ChestKind.Premium => 0.75f,
                ChestKind.SpecialEnemy => 0.55f,
                _ => 0.35f
            };
            targetOutlineColor = Color.Lerp(visualColor, Color.white, 0.7f);
            targetOutlineThickness = kind == ChestKind.Premium ? 5f : 4f;
            insufficientTicketsOutlineColor = new Color(1f, 0.22f, 0.24f, 1f);
            interactionSquashStretchAmplitude = DefaultInteractionSquashStretchAmplitude(kind);
            interactionSquashStretchFrequency = DefaultInteractionSquashStretchFrequency(kind);
            openDisappearSeconds = DefaultOpenDisappearSeconds(kind);
            openShrinkFinalScaleMultiplier = DefaultOpenShrinkFinalScaleMultiplier(kind);
            openDisappearEase = EaseSettings.InBack;
        }

        private bool EnsureVisualDefaults()
        {
            bool changed = false;
            if (visualDefaultsVersion < 2)
            {
                interactionSquashStretchAmplitude = DefaultInteractionSquashStretchAmplitude(kind);
                interactionSquashStretchFrequency = DefaultInteractionSquashStretchFrequency(kind);
                changed = true;
            }

            if (visualDefaultsVersion < 3)
            {
                openDisappearSeconds = DefaultOpenDisappearSeconds(kind);
                openShrinkFinalScaleMultiplier = DefaultOpenShrinkFinalScaleMultiplier(kind);
                changed = true;
            }

            if (visualDefaultsVersion < 4)
            {
                openDisappearEase = EaseSettings.InBack;
                changed = true;
            }

            visualDefaultsVersion = VisualDefaultsVersion;
            changed |= EnsureMinimum(ref interactionSquashStretchAmplitude, 0f, 0f);
            changed |= EnsureMinimum(ref interactionSquashStretchFrequency, 0f, 0f);
            changed |= EnsureMinimum(ref openDisappearSeconds, DefaultOpenDisappearSeconds(kind), 0.01f);
            changed |= EnsureRange(ref openShrinkFinalScaleMultiplier, DefaultOpenShrinkFinalScaleMultiplier(kind), 0f, 1f);
            changed |= EnsureEaseDefault(ref openDisappearEase, EaseSettings.InBack);
            return changed;
        }

        private static float DefaultInteractionSquashStretchAmplitude(ChestKind kind)
        {
            return kind switch
            {
                ChestKind.Premium => 0.1f,
                ChestKind.SpecialEnemy => 0.09f,
                _ => 0.08f
            };
        }

        private static float DefaultInteractionSquashStretchFrequency(ChestKind kind)
        {
            return kind switch
            {
                ChestKind.Premium => 4f,
                ChestKind.SpecialEnemy => 4.25f,
                _ => 4.5f
            };
        }

        private static float DefaultOpenDisappearSeconds(ChestKind kind)
        {
            return kind switch
            {
                ChestKind.Premium => 1.1f,
                ChestKind.SpecialEnemy => 1.05f,
                _ => 1f
            };
        }

        private static float DefaultOpenShrinkFinalScaleMultiplier(ChestKind kind)
        {
            return 0.02f;
        }

        private bool HasChestId(string value)
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

        private static bool EnsureMinimum(ref float value, float defaultValue, float minimum)
        {
            if (value >= minimum)
            {
                return false;
            }

            value = defaultValue;
            return true;
        }

        private static bool EnsureRange(ref float value, float defaultValue, float minimum, float maximum)
        {
            if (!float.IsNaN(value) && value >= minimum && value <= maximum)
            {
                return false;
            }

            value = Mathf.Clamp(defaultValue, minimum, maximum);
            return true;
        }

        private static bool EnsureEaseDefault(ref EaseSettings settings, EaseSettings defaultValue)
        {
            if (settings.shape > 0f)
            {
                return false;
            }

            settings = defaultValue;
            return true;
        }
    }
}
