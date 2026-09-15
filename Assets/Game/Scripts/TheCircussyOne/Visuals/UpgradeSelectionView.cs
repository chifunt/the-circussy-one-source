using System;
using System.Collections.Generic;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheCircussyOne.Visuals
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class UpgradeSelectionView : MonoBehaviour
    {
        [SerializeField] private UIDocument document;
        [SerializeField] private StyleSheet styleSheet;
        [SerializeField] private MarqueeButtonEffectSettings marqueeSettings = new();

        private VisualElement root;
        private VisualElement overlay;
        private VisualElement panel;
        private VisualElement[] cards;
        private Label[] rarityLabels;
        private Label[] titleLabels;
        private Label[] descriptionLabels;
        private Label[] levelLabels;
        private Label[] previewLabels;
        private VisualElement[] iconBlocks;
        private VisualElement[] rarityStrips;
        private VisualElement headingStack;
        private Label headingGlowLabel;
        private Label headingMainLabel;
        private VisualElement celebrationRayLayer;
        private VisualElement celebrationConfettiLayer;
        private Color[] rarityColors;
        private bool callbacksRegistered;
        private bool styleApplied;
        private bool pointerHoverArmed;
        private HudVisualConfig hudConfig;
        private readonly UiPopupMotionDriver popupMotion = new();
        private readonly UiCelebrationEffectController celebrationEffects = new();
        private MarqueeButtonEffectController marqueeEffects;

        public event Action<int> ChoiceSelected;
        public int HighlightedIndex { get; private set; }
        public bool HasHighlightedSelection => HighlightedIndex >= 0;
        public bool IsVisible { get; private set; }

        private void Awake()
        {
            Resolve();
            if (!IsVisible)
            {
                Hide();
            }
        }

        private void OnEnable()
        {
            Resolve();
        }

        private void OnDisable()
        {
            marqueeEffects?.Dispose();
            marqueeEffects = null;
        }

        public void Show(IReadOnlyList<UpgradeChoice> choices, IReadOnlyList<UpgradePreviewFrame> previews = null, PerformerDefinition selectedPerformer = null)
        {
            Resolve();
            IsVisible = true;
            HighlightedIndex = -1;
            pointerHoverArmed = false;
            popupMotion.Show(overlay, PopupMotionTarget(), PopupMotionSettings());
            celebrationEffects.Show();

            int count = cards?.Length ?? 0;
            rarityColors ??= new Color[count];
            for (int i = 0; i < count; i++)
            {
                UpgradeChoice choice = choices != null && i < choices.Count ? choices[i] : default;
                UpgradePreviewFrame preview = previews != null && i < previews.Count ? previews[i] : default;
                bool hasChoice = choice.HasChoice;
                SetCardVisible(i, hasChoice);
                if (hasChoice)
                {
                    ApplyChoice(i, choice, preview, selectedPerformer);
                }
            }

            UpdateHighlight();
            SetSelectionPulse(0f);
        }

        public void Hide()
        {
            Resolve();
            IsVisible = false;
            HighlightedIndex = -1;
            popupMotion.HideImmediate(overlay, PopupMotionTarget());
            celebrationEffects.HideImmediate();

            UpdateHighlight();
        }

        public void ApplyConfig(HudVisualConfig config)
        {
            hudConfig = config;
            hudConfig?.EnsureReadableDefaults();
        }

        public void TickPopupMotion(float unscaledDeltaTime)
        {
            popupMotion.Tick(PopupMotionTarget(), unscaledDeltaTime, PopupMotionSettings());
            marqueeEffects?.Tick(unscaledDeltaTime, Time.unscaledTime);
            celebrationEffects.Tick(unscaledDeltaTime, Time.unscaledTime);
            TickHeadingPulse(Time.unscaledTime);
        }

        public void Highlight(int index)
        {
            int count = cards?.Length ?? 0;
            HighlightedIndex = index < 0 ? -1 : Mathf.Clamp(index, 0, Mathf.Max(0, count - 1));
            UpdateHighlight();
        }

        public void SelectHighlighted()
        {
            if (HasHighlightedSelection)
            {
                Select(HighlightedIndex);
            }
        }

        public void Select(int index)
        {
            if (!IsVisible || cards == null || index < 0 || index >= cards.Length)
            {
                return;
            }

            ChoiceSelected?.Invoke(index);
        }

        public void ConfigureForTests(
            VisualElement overlay,
            VisualElement[] cards,
            Label[] rarityLabels,
            Label[] titleLabels,
            Label[] descriptionLabels,
            Label[] levelLabels,
            VisualElement[] rarityStrips,
            Label[] previewLabels = null,
            VisualElement[] iconBlocks = null)
        {
            this.overlay = overlay;
            this.cards = cards;
            this.rarityLabels = rarityLabels;
            this.titleLabels = titleLabels;
            this.descriptionLabels = descriptionLabels;
            this.levelLabels = levelLabels;
            this.rarityStrips = rarityStrips;
            this.previewLabels = previewLabels;
            this.iconBlocks = iconBlocks;
            rarityColors = new Color[cards?.Length ?? 0];
            ConfigurePickingTargets();
            RegisterCallbacks();
        }

        private void Resolve()
        {
            if (document == null)
            {
                document = GetComponent<UIDocument>();
            }

            if (document == null || document.rootVisualElement == null)
            {
                RegisterCallbacks();
                return;
            }

            root = document.rootVisualElement;
            HudLayerPickingRules.Apply(root);
            if (!styleApplied && styleSheet != null)
            {
                root.styleSheets.Add(styleSheet);
                styleApplied = true;
            }

            overlay ??= root.Q<VisualElement>("upgrade-selection");
            cards ??= new[]
            {
                root.Q<VisualElement>("upgrade-choice-0"),
                root.Q<VisualElement>("upgrade-choice-1"),
                root.Q<VisualElement>("upgrade-choice-2")
            };
            rarityLabels ??= QueryLabels("upgrade-rarity-");
            titleLabels ??= QueryLabels("upgrade-title-");
            descriptionLabels ??= QueryLabels("upgrade-description-");
            levelLabels ??= QueryLabels("upgrade-level-");
            previewLabels ??= QueryLabels("upgrade-preview-");
            iconBlocks ??= QueryElements("upgrade-icon-");
            rarityStrips ??= QueryElements("upgrade-rarity-strip-");
            headingStack ??= root.Q<VisualElement>("upgrade-heading-stack");
            headingGlowLabel ??= root.Q<Label>("upgrade-heading-glow");
            headingMainLabel ??= root.Q<Label>("upgrade-heading");
            celebrationRayLayer ??= root.Q<VisualElement>("upgrade-ray-layer");
            celebrationConfettiLayer ??= root.Q<VisualElement>("upgrade-confetti-layer");
            panel ??= root.Q<VisualElement>(className: "upgrade-panel");
            ConfigurePickingTargets();
            RegisterCallbacks();
            RegisterMarqueeEffects();
            RegisterCelebrationEffects();
        }

        private void ConfigurePickingTargets()
        {
            if (overlay != null)
            {
                overlay.pickingMode = PickingMode.Position;
            }

            if (cards == null)
            {
                return;
            }

            for (int i = 0; i < cards.Length; i++)
            {
                if (cards[i] != null)
                {
                    cards[i].pickingMode = PickingMode.Position;
                }
            }
        }

        private void RegisterMarqueeEffects()
        {
            marqueeSettings.pointerHoverActivates = false;
            marqueeSettings.focusActivates = false;
            marqueeEffects ??= new MarqueeButtonEffectController(marqueeSettings);
            if (cards == null)
            {
                return;
            }

            for (int i = 0; i < cards.Length; i++)
            {
                marqueeEffects.Register(cards[i]);
            }
        }

        private void RegisterCelebrationEffects()
        {
            celebrationEffects.Bind(celebrationRayLayer, celebrationConfettiLayer);
        }

        private void TickHeadingPulse(float unscaledTime)
        {
            if (!IsVisible || headingStack == null)
            {
                return;
            }

            float pulse = 0.5f + 0.5f * Mathf.Sin(unscaledTime * 1.65f);
            float scale = Mathf.Lerp(0.982f, 1.028f, pulse);
            headingStack.style.scale = new Scale(new Vector3(scale, scale, 1f));

            if (headingGlowLabel != null)
            {
                headingGlowLabel.style.opacity = Mathf.Lerp(0.28f, 0.68f, pulse);
            }

            if (headingMainLabel != null)
            {
                headingMainLabel.style.color = Color.Lerp(
                    new Color(1f, 0.92f, 0.58f, 1f),
                    new Color(1f, 0.99f, 0.78f, 1f),
                    pulse);
            }
        }

        private VisualElement PopupMotionTarget()
        {
            return panel ?? overlay;
        }

        private HudPopupMotionSettings PopupMotionSettings()
        {
            return hudConfig != null ? hudConfig.popupMotion : HudPopupMotionSettings.Default;
        }

        private Label[] QueryLabels(string prefix)
        {
            return new[]
            {
                root.Q<Label>(prefix + "0"),
                root.Q<Label>(prefix + "1"),
                root.Q<Label>(prefix + "2")
            };
        }

        private VisualElement[] QueryElements(string prefix)
        {
            return new[]
            {
                root.Q<VisualElement>(prefix + "0"),
                root.Q<VisualElement>(prefix + "1"),
                root.Q<VisualElement>(prefix + "2")
            };
        }

        private void RegisterCallbacks()
        {
            if (callbacksRegistered || cards == null)
            {
                return;
            }

            for (int i = 0; i < cards.Length; i++)
            {
                int index = i;
                if (cards[i] == null)
                {
                    continue;
                }

                cards[i].RegisterCallback<PointerEnterEvent>(_ => HighlightFromPointerEnter(index));
                cards[i].RegisterCallback<PointerMoveEvent>(_ => HighlightFromPointerMove(index));
                cards[i].RegisterCallback<ClickEvent>(_ => Select(index));
            }

            callbacksRegistered = true;
        }

        private void HighlightFromPointerEnter(int index)
        {
            if (pointerHoverArmed)
            {
                Highlight(index);
            }
        }

        private void HighlightFromPointerMove(int index)
        {
            pointerHoverArmed = true;
            Highlight(index);
        }

        private void ApplyChoice(int index, UpgradeChoice choice, UpgradePreviewFrame preview, PerformerDefinition selectedPerformer)
        {
            ContentRarityInfo rarity = ContentRarityMetadata.Get(choice.Rarity);
            if (rarityColors != null && index < rarityColors.Length)
            {
                rarityColors[index] = rarity.Color;
            }

            SetText(rarityLabels, index, rarity.DisplayName.ToUpperInvariant());
            SetText(titleLabels, index, choice.DisplayName);
            SetText(descriptionLabels, index, choice.ShortDescription);
            SetText(levelLabels, index, $"LV {choice.CurrentLevel} -> {choice.NextLevel}");
            SetPreviewText(index, preview.Text);
            ApplyIcon(index, choice, rarity.Color, selectedPerformer);

            if (rarityLabels != null && index < rarityLabels.Length && rarityLabels[index] != null)
            {
                rarityLabels[index].style.color = rarity.Color;
            }

            if (rarityStrips != null && index < rarityStrips.Length && rarityStrips[index] != null)
            {
                rarityStrips[index].style.backgroundColor = rarity.Color;
            }

            ApplyCardBorderColor(index, rarity.Color);
        }

        private void ApplyIcon(int index, UpgradeChoice choice, Color rarityColor, PerformerDefinition selectedPerformer)
        {
            if (iconBlocks == null || index >= iconBlocks.Length || iconBlocks[index] == null)
            {
                return;
            }

            VisualElement icon = iconBlocks[index];
            Sprite sprite = ContentIconRules.ForUpgradeChoice(choice, selectedPerformer);
            ContentIconVisuals.Apply(icon, sprite, choice.IconColor, rarityColor);
        }

        public void SetSelectionPulse(float pulse01)
        {
            if (cards == null)
            {
                return;
            }

            float pulse = Mathf.Clamp01(pulse01);
            for (int i = 0; i < cards.Length; i++)
            {
                if (cards[i] == null || i != HighlightedIndex)
                {
                    if (rarityColors != null && i < rarityColors.Length)
                    {
                        ApplyCardBorderColor(i, rarityColors[i]);
                    }

                    continue;
                }

                Color baseColor = rarityColors != null && i < rarityColors.Length ? rarityColors[i] : Color.white;
                ApplyCardBorderColor(i, Color.Lerp(baseColor, Color.white, pulse * 0.5f));
            }
        }

        private void ApplyCardBorderColor(int index, Color color)
        {
            if (cards != null && index < cards.Length && cards[index] != null)
            {
                cards[index].style.borderTopColor = color;
                cards[index].style.borderBottomColor = color;
                cards[index].style.borderLeftColor = color;
                cards[index].style.borderRightColor = color;
            }
        }

        private void SetPreviewText(int index, string text)
        {
            if (previewLabels == null || index >= previewLabels.Length || previewLabels[index] == null)
            {
                return;
            }

            previewLabels[index].enableRichText = true;
            previewLabels[index].text = text ?? string.Empty;
        }

        private static void SetText(Label[] labels, int index, string text)
        {
            if (labels != null && index < labels.Length && labels[index] != null)
            {
                labels[index].text = text;
            }
        }

        private void SetCardVisible(int index, bool visible)
        {
            if (cards != null && index < cards.Length && cards[index] != null)
            {
                cards[index].style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private void UpdateHighlight()
        {
            if (cards == null)
            {
                return;
            }

            for (int i = 0; i < cards.Length; i++)
            {
                if (cards[i] == null)
                {
                    continue;
                }

                cards[i].EnableInClassList("upgrade-card-selected", i == HighlightedIndex);
                cards[i].EnableInClassList("is-selected", i == HighlightedIndex);
                cards[i].EnableInClassList("upgrade-card-unselected", HighlightedIndex < 0);
            }
        }
    }
}
