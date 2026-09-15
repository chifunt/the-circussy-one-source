using Sirenix.OdinInspector;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;
using UnityEngine;

namespace TheCircussyOne.Content
{
    public enum TicketDepositTier
    {
        Small = 0,
        Medium = 1,
        Large = 2
    }

    public enum TicketDepositPayoutMode
    {
        OnComplete = 0,
        Chunked = 1
    }

    [CreateAssetMenu(menuName = "The Circussy One/Content/Ticket Deposit Definition", fileName = "TicketDepositDefinition")]
    public sealed class TicketDepositDefinition : SerializedScriptableObject, IActivatableContentDefinition
    {
        private const string Tabs = "Ticket Deposit";
        private const int AvailabilityDefaultsVersion = 1;
        private const int VisualGlowDefaultsVersion = 5;

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Core"), LabelWidth(180), ReadOnly]
        [ValidateInput(nameof(HasDepositId), "Deposit id is required.")]
        public string depositId = "small_ticket_stack";

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Core"), LabelWidth(180)]
        [ValidateInput(nameof(HasDisplayName), "Display name is required.")]
        public string displayName = "Small Ticket Stack";

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Core"), LabelWidth(180), MultiLineProperty(2)]
        public string shortDescription = "A quick bundle of loose prize tickets.";

        [HideInInspector] public ContentTagSet tags = ContentTagSet.With(ContentTag.Currency, ContentTag.Economy);

        [TabGroup(Tabs, "Identity"), BoxGroup(Tabs + "/Identity/Classification"), InfoBox("Computed from the deposit role. Change source fields instead.", InfoMessageType.Info), ShowInInspector, ReadOnly, LabelText("Computed Tags")]
        private string ComputedTags => ComputedContentTagRules.Format(Tags);

        [HideInInspector] public bool isActive = true;

        [TabGroup(Tabs, "Collection"), BoxGroup(Tabs + "/Collection/Tuning"), LabelWidth(180), EnumToggleButtons]
        public TicketDepositTier tier = TicketDepositTier.Small;

        [TabGroup(Tabs, "Collection"), BoxGroup(Tabs + "/Collection/Tuning"), LabelWidth(180), Min(0.05f), SuffixLabel("sec", true)]
        public float holdSeconds = 1f;

        [TabGroup(Tabs, "Collection"), BoxGroup(Tabs + "/Collection/Reward"), LabelWidth(180), Min(0)]
        public int minTickets = 5;

        [TabGroup(Tabs, "Collection"), BoxGroup(Tabs + "/Collection/Reward"), LabelWidth(180), Min(0)]
        public int maxTickets = 8;

        [TabGroup(Tabs, "Collection"), BoxGroup(Tabs + "/Collection/Reward"), LabelWidth(180), EnumToggleButtons]
        public TicketDepositPayoutMode payoutMode = TicketDepositPayoutMode.OnComplete;

        [TabGroup(Tabs, "Collection"), BoxGroup(Tabs + "/Collection/Reward"), LabelWidth(180), Min(1), ShowIf(nameof(UsesChunkedPayout))]
        public int payoutChunks = 1;

        [TabGroup(Tabs, "Collection"), BoxGroup(Tabs + "/Collection/Prompt"), LabelWidth(180)]
        public string promptText = "Collect Tickets";

        [TabGroup(Tabs, "Placement"), BoxGroup(Tabs + "/Placement/Collision"), LabelWidth(180), InlineProperty]
        public WorldPropPlacementProfile placement = WorldPropPlacementProfile.TicketDepositDefault(TicketDepositTier.Small);

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Model"), LabelWidth(180), AssetSelector, PreviewField(72)]
        public GameObject visualPrefab;

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Placeholder"), LabelWidth(180)]
        public Color visualColor = new(1f, 0.86f, 0.22f, 1f);

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Placeholder"), LabelWidth(180), Min(0.1f)]
        public float visualScale = 1f;

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Glow"), LabelWidth(180)]
        public Color emissionColor = new(1f, 0.86f, 0.22f, 1f);

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Glow"), LabelWidth(180), SuffixLabel("x", true), Min(0f)]
        public float emissionStrength = 0.7f;

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Target Outline"), LabelWidth(180)]
        public Color targetOutlineColor = new(1f, 0.98f, 0.72f, 1f);

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Target Outline"), LabelWidth(180), LabelText("Target Outline Width"), SuffixLabel("px", true), Min(0f)]
        public float targetOutlineThickness = 3f;

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Interaction Squash"), LabelWidth(180), SuffixLabel("x", true), Min(0f)]
        [InfoBox("Plays while the player is actively holding interact on this deposit. Set amplitude to 0 to disable.")]
        public float interactionSquashStretchAmplitude = 0.08f;

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Interaction Squash"), LabelWidth(180), SuffixLabel("Hz", true), Min(0f)]
        public float interactionSquashStretchFrequency = 5f;

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Collect Disappear"), LabelWidth(180), SuffixLabel("sec", true), Min(0.01f)]
        public float collectDisappearSeconds = 0.72f;

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Collect Disappear"), LabelWidth(180), NumericSlider(0f, 1f)]
        public float collectShrinkFinalScaleMultiplier = 0.02f;

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Collect Disappear"), LabelWidth(180), InlineProperty]
        public EaseSettings collectDisappearEase = EaseSettings.InBack;

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Ticket Paper Burst"), LabelWidth(180)]
        public bool ticketBurstEnabled = true;

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Ticket Paper Burst"), LabelWidth(180), Min(0)]
        public int ticketBurstCount = 14;

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Ticket Paper Burst"), LabelWidth(180)]
        public Color ticketBurstColor = new(1f, 0.84f, 0.24f, 1f);

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Ticket Paper Burst"), LabelWidth(180), PreviewField(64)]
        public Texture2D ticketBurstTexture;

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Ticket Paper Burst"), LabelWidth(180), SuffixLabel("sec", true), Min(0.01f)]
        public float ticketBurstLifetimeSeconds = 1.15f;

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Ticket Paper Burst"), LabelWidth(180), SuffixLabel("u/sec", true), Min(0f)]
        public float ticketBurstSpeed = 1.8f;

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Ticket Paper Burst"), LabelWidth(180), Min(0f)]
        public float ticketBurstGravity = 0.22f;

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Ticket Paper Burst"), LabelWidth(180), Min(0f)]
        public float ticketBurstSwayStrength = 0.42f;

        [TabGroup(Tabs, "Visual"), BoxGroup(Tabs + "/Visual/Ticket Paper Burst"), LabelWidth(180), MinValue(0.01f)]
        public Vector2 ticketBurstRectangleSize = new(0.26f, 0.13f);

        [SerializeField, HideInInspector] private int availabilityDefaultsVersion;
        [SerializeField, HideInInspector] private int visualGlowDefaultsVersion;

        public string Id => depositId;
        public string DisplayName => displayName;
        public ContentTagSet Tags => ComputedContentTagRules.For(this);
        public bool IsActive => isActive;
        public int ClampedMinTickets => Mathf.Max(0, minTickets);
        public int ClampedMaxTickets => Mathf.Max(ClampedMinTickets, maxTickets);
        public int EffectivePayoutChunks => UsesChunkedPayout ? Mathf.Max(1, payoutChunks) : 1;
        private bool UsesChunkedPayout => payoutMode == TicketDepositPayoutMode.Chunked;

        public void ApplyDefaults(
            string id,
            string name,
            string description,
            TicketDepositTier tier,
            float holdSeconds,
            int minTickets,
            int maxTickets,
            TicketDepositPayoutMode payoutMode,
            int payoutChunks,
            string promptText,
            Color visualColor,
            float visualScale)
        {
            depositId = id;
            displayName = name;
            shortDescription = description;
            this.tier = tier;
            this.holdSeconds = holdSeconds;
            this.minTickets = minTickets;
            this.maxTickets = maxTickets;
            this.payoutMode = payoutMode;
            this.payoutChunks = Mathf.Max(1, payoutChunks);
            this.promptText = promptText;
            this.visualColor = visualColor;
            this.visualScale = visualScale;
            placement = WorldPropPlacementProfile.TicketDepositDefault(tier);
            ApplyVisualGlowDefaults(force: true);
            isActive = true;
            availabilityDefaultsVersion = AvailabilityDefaultsVersion;
            visualGlowDefaultsVersion = VisualGlowDefaultsVersion;
            EnsureWorkflowDefaults();
        }

        public bool EnsureWorkflowDefaults()
        {
            bool changed = false;
            changed |= EnsureString(ref depositId, "small_ticket_stack");
            changed |= EnsureString(ref displayName, "Small Ticket Stack");
            changed |= EnsureString(ref shortDescription, "A bundle of prize tickets.");
            changed |= EnsureString(ref promptText, "Collect Tickets");
            changed |= EnsureAvailabilityDefaults();

            if (holdSeconds <= 0f)
            {
                holdSeconds = 1f;
                changed = true;
            }

            if (minTickets < 0)
            {
                minTickets = 0;
                changed = true;
            }

            if (maxTickets < minTickets)
            {
                maxTickets = minTickets;
                changed = true;
            }

            if (payoutChunks < 1)
            {
                payoutChunks = 1;
                changed = true;
            }

            if (visualScale <= 0f)
            {
                visualScale = 1f;
                changed = true;
            }

            WorldPropPlacementProfile defaultPlacement = WorldPropPlacementProfile.TicketDepositDefault(tier);
            changed |= placement.EnsureDefaults(defaultPlacement);
            if (!placement.blocksPlayer)
            {
                placement = defaultPlacement;
                changed = true;
            }

            changed |= EnsureVisualGlowDefaults();
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
            depositId = ContentIdSuggestionRules.NormalizeBase(displayName, "ticket_deposit");
            EnsureWorkflowDefaults();
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

        private bool EnsureVisualGlowDefaults()
        {
            if (visualGlowDefaultsVersion >= VisualGlowDefaultsVersion)
            {
                bool changed = false;
                changed |= EnsureMinimum(ref emissionStrength, 0f, 0f);
                changed |= EnsureMinimum(ref targetOutlineThickness, 0f, 0f);
                changed |= EnsureMinimum(ref interactionSquashStretchAmplitude, 0f, 0f);
                changed |= EnsureMinimum(ref interactionSquashStretchFrequency, 0f, 0f);
                changed |= EnsureCollectDisappearDefaults();
                changed |= EnsureTicketBurstDefaults();
                return changed;
            }

            if (visualGlowDefaultsVersion <= 0)
            {
                ApplyVisualGlowDefaults(force: true);
            }
            else if (visualGlowDefaultsVersion < 3)
            {
                if (targetOutlineThickness <= 0.2f || targetOutlineThickness < 2f)
                {
                    targetOutlineThickness = DefaultTargetOutlineThickness(tier);
                }
            }

            if (visualGlowDefaultsVersion < 4)
            {
                interactionSquashStretchAmplitude = DefaultInteractionSquashStretchAmplitude(tier);
                interactionSquashStretchFrequency = DefaultInteractionSquashStretchFrequency(tier);
            }

            if (visualGlowDefaultsVersion < 5)
            {
                ApplyCollectDisappearDefaults();
                ApplyTicketBurstDefaults();
            }

            visualGlowDefaultsVersion = VisualGlowDefaultsVersion;
            return true;
        }

        private void ApplyVisualGlowDefaults(bool force)
        {
            if (!force)
            {
                return;
            }

            emissionColor = Color.Lerp(visualColor, Color.white, 0.12f);
            emissionStrength = tier switch
            {
                TicketDepositTier.Medium => 0.95f,
                TicketDepositTier.Large => 1.25f,
                _ => 0.7f
            };
            targetOutlineColor = Color.Lerp(visualColor, Color.white, 0.62f);
            targetOutlineThickness = DefaultTargetOutlineThickness(tier);
            interactionSquashStretchAmplitude = DefaultInteractionSquashStretchAmplitude(tier);
            interactionSquashStretchFrequency = DefaultInteractionSquashStretchFrequency(tier);
            ApplyCollectDisappearDefaults();
            ApplyTicketBurstDefaults();
        }

        private void ApplyCollectDisappearDefaults()
        {
            collectDisappearSeconds = tier switch
            {
                TicketDepositTier.Medium => 0.82f,
                TicketDepositTier.Large => 0.92f,
                _ => 0.72f
            };
            collectShrinkFinalScaleMultiplier = 0.02f;
            collectDisappearEase = EaseSettings.InBack;
        }

        private void ApplyTicketBurstDefaults()
        {
            ticketBurstEnabled = true;
            ticketBurstCount = tier switch
            {
                TicketDepositTier.Medium => 20,
                TicketDepositTier.Large => 28,
                _ => 14
            };
            ticketBurstColor = Color.Lerp(visualColor, Color.white, 0.08f);
            ticketBurstLifetimeSeconds = tier switch
            {
                TicketDepositTier.Large => 1.35f,
                TicketDepositTier.Medium => 1.25f,
                _ => 1.15f
            };
            ticketBurstSpeed = tier switch
            {
                TicketDepositTier.Large => 2.25f,
                TicketDepositTier.Medium => 2f,
                _ => 1.8f
            };
            ticketBurstGravity = 0.22f;
            ticketBurstSwayStrength = 0.42f;
            ticketBurstRectangleSize = new Vector2(0.26f, 0.13f);
        }

        private bool EnsureCollectDisappearDefaults()
        {
            bool changed = false;
            changed |= EnsureMinimum(ref collectDisappearSeconds, 0.72f, 0.01f);
            changed |= EnsureRange(ref collectShrinkFinalScaleMultiplier, 0.02f, 0f, 1f);
            changed |= EnsureEaseDefault(ref collectDisappearEase, EaseSettings.InBack);
            return changed;
        }

        private bool EnsureTicketBurstDefaults()
        {
            bool changed = false;
            changed |= EnsureMinimumInt(ref ticketBurstCount, 14, 0);
            changed |= EnsureMinimum(ref ticketBurstLifetimeSeconds, 1.15f, 0.01f);
            changed |= EnsureMinimum(ref ticketBurstSpeed, 1.8f, 0f);
            changed |= EnsureMinimum(ref ticketBurstGravity, 0.22f, 0f);
            changed |= EnsureMinimum(ref ticketBurstSwayStrength, 0.42f, 0f);
            if (ticketBurstRectangleSize.x < 0.01f || ticketBurstRectangleSize.y < 0.01f)
            {
                ticketBurstRectangleSize = new Vector2(0.26f, 0.13f);
                changed = true;
            }

            if (ticketBurstColor.a <= 0f)
            {
                ticketBurstColor = Color.Lerp(visualColor, Color.white, 0.08f);
                changed = true;
            }

            return changed;
        }

        private static float DefaultTargetOutlineThickness(TicketDepositTier tier)
        {
            return tier switch
            {
                TicketDepositTier.Medium => 3.5f,
                TicketDepositTier.Large => 4f,
                _ => 3f
            };
        }

        private static float DefaultInteractionSquashStretchAmplitude(TicketDepositTier tier)
        {
            return tier switch
            {
                TicketDepositTier.Medium => 0.09f,
                TicketDepositTier.Large => 0.1f,
                _ => 0.08f
            };
        }

        private static float DefaultInteractionSquashStretchFrequency(TicketDepositTier tier)
        {
            return tier switch
            {
                TicketDepositTier.Medium => 4.5f,
                TicketDepositTier.Large => 4f,
                _ => 5f
            };
        }

        private bool HasDepositId(string value)
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

        private static bool EnsureMinimumInt(ref int value, int defaultValue, int minimum)
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
            if (value >= minimum && value <= maximum)
            {
                return false;
            }

            value = defaultValue;
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
