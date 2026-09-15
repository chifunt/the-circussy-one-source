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
    public sealed class PerformerSelectionView : MonoBehaviour
    {
        [SerializeField] private UIDocument document;
        [SerializeField] private StyleSheet styleSheet;

        private VisualElement root;
        private VisualElement overlay;
        private VisualElement panel;
        private VisualElement[] cards;
        private VisualElement[] portraitBlocks;
        private VisualElement[] weaponIconBlocks;
        private Label[] nameLabels;
        private Label[] titleLabels;
        private Label[] descriptionLabels;
        private Label[] weaponLabels;
        private Label[] passiveLabels;
        private Label[] statLabels;
        private Label[] talentLabels;
        private PerformerDefinition[] boundPerformers;
        private int callbacksRegisteredCount;
        private bool styleApplied;
        private bool pointerHoverArmed;
        private HudVisualConfig hudConfig;
        private readonly UiPopupMotionDriver popupMotion = new();

        public event Action<PerformerDefinition> PerformerSelected;
        public int HighlightedIndex { get; private set; } = -1;
        public bool HasHighlightedSelection => HighlightedIndex >= 0;
        public bool IsVisible { get; private set; }
        public int VisibleCardCount { get; private set; }
        public bool CanShowSelection
        {
            get
            {
                Resolve();
                return overlay != null && cards != null && cards.Length > 0 && cards[0] != null;
            }
        }

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

        public void Show(IReadOnlyList<PerformerSelectionCardFrame> frames)
        {
            Resolve();
            EnsureCardCapacity(frames?.Count ?? 0);
            IsVisible = true;
            HighlightedIndex = -1;
            pointerHoverArmed = false;
            VisibleCardCount = Mathf.Min(cards?.Length ?? 0, frames?.Count ?? 0);
            boundPerformers = new PerformerDefinition[cards?.Length ?? 0];
            popupMotion.Show(overlay, PopupMotionTarget(), PopupMotionSettings());

            int count = cards?.Length ?? 0;
            for (int i = 0; i < count; i++)
            {
                bool hasFrame = frames != null && i < frames.Count && frames[i].Performer != null;
                SetCardVisible(i, hasFrame);
                if (hasFrame)
                {
                    ApplyFrame(i, frames[i]);
                }
            }

            UpdateHighlight();
        }

        public void Hide()
        {
            Resolve();
            IsVisible = false;
            HighlightedIndex = -1;
            popupMotion.HideImmediate(overlay, PopupMotionTarget());

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
        }

        public void Highlight(int index)
        {
            int count = Mathf.Max(0, VisibleCardCount);
            HighlightedIndex = index < 0 || count <= 0 ? -1 : Mathf.Clamp(index, 0, count - 1);
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
            if (!IsVisible || boundPerformers == null || index < 0 || index >= boundPerformers.Length)
            {
                return;
            }

            PerformerDefinition performer = boundPerformers[index];
            if (performer != null)
            {
                PerformerSelected?.Invoke(performer);
            }
        }

        public void ConfigureForTests(
            VisualElement overlay,
            VisualElement[] cards,
            VisualElement[] portraitBlocks,
            Label[] nameLabels,
            Label[] descriptionLabels,
            Label[] weaponLabels,
            Label[] passiveLabels,
            Label[] statLabels,
            Label[] talentLabels,
            Label[] titleLabels = null,
            VisualElement[] weaponIconBlocks = null)
        {
            this.overlay = overlay;
            this.cards = cards;
            this.portraitBlocks = portraitBlocks;
            this.weaponIconBlocks = weaponIconBlocks;
            this.nameLabels = nameLabels;
            this.titleLabels = titleLabels;
            this.descriptionLabels = descriptionLabels;
            this.weaponLabels = weaponLabels;
            this.passiveLabels = passiveLabels;
            this.statLabels = statLabels;
            this.talentLabels = talentLabels;
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
            if (!styleApplied && styleSheet != null)
            {
                root.styleSheets.Add(styleSheet);
                styleApplied = true;
            }

            overlay ??= root.Q<VisualElement>("performer-selection");
            panel ??= root.Q<VisualElement>(className: "performer-panel");
            cards ??= QueryElements("performer-choice-");
            portraitBlocks ??= QueryElements("performer-portrait-");
            weaponIconBlocks ??= QueryElements("performer-weapon-icon-");
            nameLabels ??= QueryLabels("performer-name-");
            titleLabels ??= QueryLabels("performer-title-");
            descriptionLabels ??= QueryLabels("performer-description-");
            weaponLabels ??= QueryLabels("performer-weapon-");
            passiveLabels ??= QueryLabels("performer-passive-");
            statLabels ??= QueryLabels("performer-stats-");
            talentLabels ??= QueryLabels("performer-talents-");
            RegisterCallbacks();
        }

        private VisualElement PopupMotionTarget()
        {
            return panel ?? overlay;
        }

        private HudPopupMotionSettings PopupMotionSettings()
        {
            return hudConfig != null ? hudConfig.popupMotion : HudPopupMotionSettings.Default;
        }

        private VisualElement[] QueryElements(string prefix)
        {
            var elements = new List<VisualElement>();
            for (int i = 0; i < 12; i++)
            {
                VisualElement element = root.Q<VisualElement>(prefix + i);
                if (element != null)
                {
                    elements.Add(element);
                }
            }

            return elements.ToArray();
        }

        private Label[] QueryLabels(string prefix)
        {
            var labels = new List<Label>();
            for (int i = 0; i < 12; i++)
            {
                Label label = root.Q<Label>(prefix + i);
                if (label != null)
                {
                    labels.Add(label);
                }
            }

            return labels.ToArray();
        }

        private void RegisterCallbacks()
        {
            if (cards == null)
            {
                return;
            }

            for (int i = callbacksRegisteredCount; i < cards.Length; i++)
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

            callbacksRegisteredCount = cards.Length;
        }

        private void EnsureCardCapacity(int desiredCount)
        {
            if (desiredCount <= 0 || cards == null || cards.Length >= desiredCount)
            {
                return;
            }

            VisualElement row = cards.Length > 0 ? cards[0]?.parent : null;
            if (row == null)
            {
                return;
            }

            int previousCount = cards.Length;
            Array.Resize(ref cards, desiredCount);
            Array.Resize(ref portraitBlocks, desiredCount);
            Array.Resize(ref weaponIconBlocks, desiredCount);
            Array.Resize(ref nameLabels, desiredCount);
            Array.Resize(ref titleLabels, desiredCount);
            Array.Resize(ref descriptionLabels, desiredCount);
            Array.Resize(ref weaponLabels, desiredCount);
            Array.Resize(ref passiveLabels, desiredCount);
            Array.Resize(ref statLabels, desiredCount);
            Array.Resize(ref talentLabels, desiredCount);

            for (int i = previousCount; i < desiredCount; i++)
            {
                VisualElement card = CreateCardElement(i, out VisualElement portrait, out Label name, out Label title, out Label description, out Label weapon, out Label passive, out Label stats, out Label talents);
                row.Add(card);
                cards[i] = card;
                portraitBlocks[i] = portrait;
                weaponIconBlocks[i] = card.Q<VisualElement>("performer-weapon-icon-" + i);
                nameLabels[i] = name;
                titleLabels[i] = title;
                descriptionLabels[i] = description;
                weaponLabels[i] = weapon;
                passiveLabels[i] = passive;
                statLabels[i] = stats;
                talentLabels[i] = talents;
            }

            RegisterCallbacks();
        }

        private static VisualElement CreateCardElement(
            int index,
            out VisualElement portrait,
            out Label name,
            out Label title,
            out Label description,
            out Label weapon,
            out Label passive,
            out Label stats,
            out Label talents)
        {
            var card = new VisualElement { name = "performer-choice-" + index };
            card.AddToClassList("performer-card");
            portrait = new VisualElement { name = "performer-portrait-" + index };
            portrait.AddToClassList("performer-portrait");
            name = new Label { name = "performer-name-" + index };
            name.AddToClassList("performer-name");
            title = new Label { name = "performer-title-" + index };
            title.AddToClassList("performer-title");
            description = new Label { name = "performer-description-" + index };
            description.AddToClassList("performer-description");
            weapon = new Label { name = "performer-weapon-" + index };
            weapon.AddToClassList("performer-detail");
            var weaponRow = new VisualElement();
            weaponRow.AddToClassList("performer-weapon-row");
            var weaponIcon = new VisualElement { name = "performer-weapon-icon-" + index };
            weaponIcon.AddToClassList("performer-weapon-icon");
            passive = new Label { name = "performer-passive-" + index };
            passive.AddToClassList("performer-detail");
            stats = new Label { name = "performer-stats-" + index };
            stats.AddToClassList("performer-stats");
            talents = new Label { name = "performer-talents-" + index };
            talents.AddToClassList("performer-talents");
            card.Add(portrait);
            card.Add(name);
            card.Add(title);
            card.Add(description);
            weaponRow.Add(weaponIcon);
            weaponRow.Add(weapon);
            card.Add(weaponRow);
            card.Add(passive);
            card.Add(stats);
            card.Add(talents);
            return card;
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

        private void ApplyFrame(int index, PerformerSelectionCardFrame frame)
        {
            boundPerformers[index] = frame.Performer;
            SetText(nameLabels, index, frame.Performer.DisplayName);
            SetText(titleLabels, index, frame.Performer.PerformerTitle);
            SetText(descriptionLabels, index, frame.Performer.shortDescription);
            SetText(weaponLabels, index, $"Weapon: {frame.StartingWeapon}");
            SetText(passiveLabels, index, $"Passive: {frame.Passive}");
            SetText(statLabels, index, frame.StatPreview);
            SetText(talentLabels, index, frame.TalentTheme);

            if (portraitBlocks != null && index < portraitBlocks.Length && portraitBlocks[index] != null)
            {
                ApplyPortrait(portraitBlocks[index], frame.Performer);
            }

            if (weaponIconBlocks != null && index < weaponIconBlocks.Length && weaponIconBlocks[index] != null)
            {
                ApplyWeaponIcon(weaponIconBlocks[index], frame.StartingWeaponDefinition);
            }
        }

        private static void ApplyPortrait(VisualElement portrait, PerformerDefinition performer)
        {
            if (portrait == null)
            {
                return;
            }

            ContentIconVisuals.Apply(portrait, ContentIconRules.ForPerformer(performer), ContentIconRules.FallbackColor(performer));
        }

        private static void ApplyWeaponIcon(VisualElement icon, WeaponDefinition weapon)
        {
            ContentIconVisuals.Apply(icon, ContentIconRules.ForWeapon(weapon), ContentIconRules.FallbackColor(weapon));
        }

        private static void SetText(Label[] labels, int index, string text)
        {
            if (labels != null && index < labels.Length && labels[index] != null)
            {
                labels[index].text = text ?? string.Empty;
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

                cards[i].EnableInClassList("performer-card-selected", i == HighlightedIndex);
                cards[i].EnableInClassList("performer-card-unselected", HighlightedIndex < 0);
            }
        }
    }
}
