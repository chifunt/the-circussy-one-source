using System;
using System.Collections.Generic;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using TheCircussyOne.Runtime;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheCircussyOne.Visuals
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class PauseMenuView : MonoBehaviour
    {
        [SerializeField] private UIDocument document;

        private VisualElement root;
        private VisualElement overlay;
        private VisualElement panel;
        private VisualElement inventoryRoot;
        private VisualElement inventoryList;
        private VisualElement debugContainer;
        private VisualElement debugRoot;
        private VisualElement settingsHost;
        private Label titleLabel;
        private Label stateLabel;
        private Label statusLabel;
        private Label inventoryEmptyLabel;
        private Button resumeButton;
        private Button restartButton;
        private Button settingsButton;
        private Button debugToggleButton;
        private Button settingsBackButton;
        private Button worldLayoutModeButton;
        private Label worldLayoutModeLabel;
        private IntegerField ticketsField;
        private IntegerField xpField;
        private IntegerField healthField;
        private IntegerField damageField;
        private IntegerField itemAmountField;
        private DropdownField weaponDropdown;
        private DropdownField ownedWeaponDropdown;
        private DropdownField itemDropdown;
        private DropdownField talentDropdown;
        private DropdownField upgradeDropdown;
        private DropdownField talentRarityDropdown;
        private DropdownField upgradeRarityDropdown;
        private List<RunDebugOption> weaponOptions = new();
        private List<RunDebugOption> ownedWeaponOptions = new();
        private List<RunDebugOption> itemOptions = new();
        private List<RunDebugOption> talentOptions = new();
        private List<RunDebugOption> upgradeOptions = new();
        private HudVisualConfig hudConfig;
        private readonly UiPopupMotionDriver popupMotion = new();
        private readonly MarqueeButtonEffectSettings marqueeSettings = new();
        private MarqueeButtonEffectController marqueeEffects;
        private bool debugVisible;
        private bool debugUiBuilt;
        private bool resumeButtonBound;
        private bool restartButtonBound;
        private bool settingsButtonBound;
        private bool debugToggleButtonBound;
        private bool settingsBackButtonBound;

        public event Action ResumeRequested;
        public event Action RestartRequested;
        public event Action<int> AddTicketsRequested;
        public event Action<int> AddExperienceRequested;
        public event Action<int> HealRequested;
        public event Action<int> DamageRequested;
        public event Action<string> AddWeaponRequested;
        public event Action<string> RemoveWeaponRequested;
        public event Action<string, int> AddItemRequested;
        public event Action<string, int> RemoveItemRequested;
        public event Action<string, ContentRarity> ApplyTalentRequested;
        public event Action<string, ContentRarity> ApplyUpgradeRequested;
        public event Action TriggerShowtimeRequested;
        public event Action JumpToActFinaleRequested;
        public event Action ActivateProxyHeadlinerRequested;
        public event Action DefeatProxyHeadlinerRequested;
        public event Action SkipEncoreGraceRequested;
        public event Action CompleteStageDoorRequested;
        public event Action EnterNextActRequested;
        public event Action SwitchWorldLayoutModeRequested;

        public bool IsVisible { get; private set; }

        public void ApplyConfig(HudVisualConfig config)
        {
            hudConfig = config;
            hudConfig?.EnsureReadableDefaults();
        }

        public void TickPopupMotion(float unscaledDeltaTime)
        {
            popupMotion.Tick(PopupMotionTarget(), unscaledDeltaTime, PopupMotionSettings());
            marqueeEffects?.Tick(unscaledDeltaTime, Time.unscaledTime);
        }

        public void ConfigureForTests(
            VisualElement overlay,
            Label stateLabel = null,
            VisualElement debugRoot = null,
            Label statusLabel = null,
            VisualElement inventoryList = null,
            Label inventoryEmptyLabel = null)
        {
            this.overlay = overlay;
            this.stateLabel = stateLabel;
            debugContainer = null;
            this.debugRoot = debugRoot;
            this.statusLabel = statusLabel;
            this.inventoryList = inventoryList;
            this.inventoryEmptyLabel = inventoryEmptyLabel;
            settingsHost = null;
            settingsBackButton = null;
            debugUiBuilt = true;
            worldLayoutModeButton = null;
            worldLayoutModeLabel = null;
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

        private void OnDisable()
        {
            marqueeEffects?.Dispose();
            marqueeEffects = null;
        }

        public void Show(
            RunDebugSnapshot debugSnapshot,
            string status,
            IReadOnlyList<ItemInventoryCardFrame> itemFrames = null)
        {
            Resolve();
            IsVisible = true;
            SetSettingsDisplay(false);
            popupMotion.Show(overlay, PopupMotionTarget(), PopupMotionSettings());

            if (stateLabel != null)
            {
                stateLabel.text = BuildStateSummary(debugSnapshot);
            }

            if (statusLabel != null)
            {
                statusLabel.text = status ?? string.Empty;
            }

            if (debugRoot != null)
            {
                SetDebugDisplay(debugSnapshot.Available && debugVisible);
                EnsureWorldLayoutControls();
                UpdateWorldLayoutControls(debugSnapshot);
            }

            if (debugToggleButton != null)
            {
                debugToggleButton.SetEnabled(debugSnapshot.Available);
                debugToggleButton.text = debugVisible ? "HIDE DEBUG" : "DEBUG";
            }

            PopulateItemInventory(itemFrames);

            if (!debugSnapshot.Available)
            {
                return;
            }

            PopulateOptions(weaponDropdown, weaponOptions, debugSnapshot.Weapons);
            PopulateOptions(ownedWeaponDropdown, ownedWeaponOptions, debugSnapshot.OwnedWeapons);
            PopulateOptions(itemDropdown, itemOptions, debugSnapshot.Items);
            PopulateOptions(talentDropdown, talentOptions, debugSnapshot.Talents);
            PopulateOptions(upgradeDropdown, upgradeOptions, debugSnapshot.Upgrades);
            EnsureRarityChoices(talentRarityDropdown);
            EnsureRarityChoices(upgradeRarityDropdown);
        }

        public void Hide()
        {
            Resolve();
            IsVisible = false;
            debugVisible = false;
            SetDebugDisplay(false);
            SetSettingsDisplay(false);
            popupMotion.HideImmediate(overlay, PopupMotionTarget());
        }

        private void Resolve()
        {
            if (document == null)
            {
                document = GetComponent<UIDocument>();
            }

            if (document == null || document.rootVisualElement == null)
            {
                return;
            }

            root = document.rootVisualElement;
            overlay ??= root.Q<VisualElement>("pause-menu");
            if (overlay == null)
            {
                BuildFallbackUi();
            }
            else
            {
                ResolveAuthoredUi();
            }
        }

        private void ResolveAuthoredUi()
        {
            panel ??= root.Q<VisualElement>("pause-menu-panel");
            titleLabel ??= root.Q<Label>("pause-menu-title");
            inventoryRoot ??= root.Q<VisualElement>("pause-menu-items");
            inventoryEmptyLabel ??= root.Q<Label>("pause-menu-items-empty");
            inventoryList ??= root.Q<VisualElement>("pause-menu-item-list");
            stateLabel ??= root.Q<Label>("pause-menu-state");
            statusLabel ??= root.Q<Label>("pause-menu-status");
            debugContainer ??= root.Q<VisualElement>("pause-menu-debug-container");
            debugRoot ??= root.Q<VisualElement>("pause-menu-debug");
            settingsHost ??= root.Q<VisualElement>("pause-settings-host");
            settingsBackButton ??= root.Q<Button>("settings-back-button");
            resumeButton ??= root.Q<Button>("pause-menu-resume-button");
            settingsButton ??= root.Q<Button>("pause-menu-settings-button");
            debugToggleButton ??= root.Q<Button>("pause-menu-debug-button");
            restartButton ??= root.Q<Button>("pause-menu-end-run-button");

            BindButtonOnce(resumeButton, ref resumeButtonBound, () => ResumeRequested?.Invoke());
            BindButtonOnce(settingsButton, ref settingsButtonBound, ShowPauseSettings);
            BindButtonOnce(debugToggleButton, ref debugToggleButtonBound, ToggleDebugMenu);
            BindButtonOnce(restartButton, ref restartButtonBound, () => RestartRequested?.Invoke());
            BindButtonOnce(settingsBackButton, ref settingsBackButtonBound, HidePauseSettings);

            RegisterMarqueeButton(resumeButton);
            RegisterMarqueeButton(settingsButton);
            RegisterMarqueeButton(debugToggleButton);
            RegisterMarqueeButton(restartButton);
            RegisterMarqueeButton(settingsBackButton);

            if (!debugUiBuilt && debugRoot != null)
            {
                if (debugRoot.childCount == 0)
                {
                    BuildDebugUi(debugRoot);
                }

                debugUiBuilt = true;
            }
        }

        private void BuildFallbackUi()
        {
            overlay = new VisualElement { name = "pause-menu" };
            overlay.AddToClassList("pause-menu");
            overlay.style.position = Position.Absolute;
            overlay.style.left = 0f;
            overlay.style.right = 0f;
            overlay.style.top = 0f;
            overlay.style.bottom = 0f;
            overlay.style.alignItems = Align.Center;
            overlay.style.justifyContent = Justify.Center;
            overlay.pickingMode = PickingMode.Position;

            panel = new VisualElement { name = "pause-menu-panel" };
            panel.AddToClassList("pause-menu-panel");

            titleLabel = new Label("THE CIRCUSSY ONE") { name = "pause-menu-title" };
            titleLabel.AddToClassList("pause-menu-title");
            panel.Add(titleLabel);

            var body = new VisualElement { name = "pause-menu-body" };
            body.AddToClassList("pause-menu-body");

            var itemsPanel = new VisualElement { name = "pause-menu-items-panel" };
            itemsPanel.AddToClassList("pause-menu-side-panel");
            itemsPanel.AddToClassList("pause-menu-items-panel");
            inventoryRoot = new VisualElement { name = "pause-menu-items" };
            inventoryRoot.AddToClassList("pause-menu-items");
            AddSectionTitle(inventoryRoot, "ITEMS");
            inventoryEmptyLabel = new Label("No items yet.") { name = "pause-menu-items-empty" };
            inventoryEmptyLabel.AddToClassList("pause-menu-items-empty");
            inventoryRoot.Add(inventoryEmptyLabel);
            inventoryList = new VisualElement { name = "pause-menu-item-list" };
            inventoryList.AddToClassList("pause-menu-item-list");
            inventoryRoot.Add(inventoryList);
            itemsPanel.Add(inventoryRoot);
            body.Add(itemsPanel);

            var actionsPanel = new VisualElement { name = "pause-menu-actions" };
            actionsPanel.AddToClassList("pause-menu-actions");
            resumeButton = CreateButton("RESUME", () => ResumeRequested?.Invoke());
            settingsButton = CreateButton("SETTINGS", () => SetStatus("Settings menu is not wired yet."));
            debugToggleButton = CreateButton("DEBUG", ToggleDebugMenu);
            restartButton = CreateButton("END RUN", () => RestartRequested?.Invoke());
            resumeButtonBound = true;
            settingsButtonBound = true;
            debugToggleButtonBound = true;
            restartButtonBound = true;
            actionsPanel.Add(resumeButton);
            actionsPanel.Add(settingsButton);
            actionsPanel.Add(debugToggleButton);
            actionsPanel.Add(restartButton);
            body.Add(actionsPanel);

            var statsPanel = new VisualElement { name = "pause-menu-stats-panel" };
            statsPanel.AddToClassList("pause-menu-side-panel");
            statsPanel.AddToClassList("pause-menu-stats-panel");
            AddSectionTitle(statsPanel, "STATS");
            stateLabel = new Label { name = "pause-menu-state" };
            stateLabel.AddToClassList("pause-menu-state");
            statsPanel.Add(stateLabel);
            body.Add(statsPanel);

            panel.Add(body);

            var debugScroll = new ScrollView(ScrollViewMode.Vertical) { name = "pause-menu-debug-container" };
            debugScroll.AddToClassList("pause-menu-debug-container");
            debugScroll.style.display = DisplayStyle.None;

            debugRoot = new VisualElement { name = "pause-menu-debug" };
            debugRoot.AddToClassList("pause-menu-debug");
            BuildDebugUi(debugRoot);
            debugUiBuilt = true;
            debugScroll.contentContainer.Add(debugRoot);
            debugContainer = debugScroll;
            panel.Add(debugScroll);

            statusLabel = new Label { name = "pause-menu-status" };
            statusLabel.AddToClassList("pause-menu-status");
            panel.Add(statusLabel);

            overlay.Add(panel);
            root.Add(overlay);
        }

        private VisualElement PopupMotionTarget()
        {
            return panel ?? overlay;
        }

        private void ToggleDebugMenu()
        {
            debugVisible = !debugVisible;
            if (debugVisible)
            {
                SetSettingsDisplay(false);
            }

            SetDebugDisplay(debugVisible);

            if (debugToggleButton != null)
            {
                debugToggleButton.text = debugVisible ? "HIDE DEBUG" : "DEBUG";
                debugToggleButton.EnableInClassList("is-selected", debugVisible);
            }
        }

        private void ShowPauseSettings()
        {
            debugVisible = false;
            SetDebugDisplay(false);
            SetSettingsDisplay(true);
            SetStatus("Settings");

            if (debugToggleButton != null)
            {
                debugToggleButton.text = "DEBUG";
                debugToggleButton.EnableInClassList("is-selected", false);
            }
        }

        private void HidePauseSettings()
        {
            SetSettingsDisplay(false);
            SetStatus(string.Empty);
        }

        private void SetStatus(string message)
        {
            if (statusLabel != null)
            {
                statusLabel.text = message ?? string.Empty;
            }
        }

        private void SetDebugDisplay(bool visible)
        {
            VisualElement target = debugContainer ?? debugRoot;
            if (target != null)
            {
                target.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private void SetSettingsDisplay(bool visible)
        {
            if (settingsHost != null)
            {
                settingsHost.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private static void BindButtonOnce(Button button, ref bool bound, Action callback)
        {
            if (button == null || bound)
            {
                return;
            }

            button.clicked += () => callback?.Invoke();
            bound = true;
        }

        private static string BuildStateSummary(RunDebugSnapshot debugSnapshot)
        {
            string summary = $"LV {Mathf.Max(1, debugSnapshot.Level)} | HP {debugSnapshot.Health}/{debugSnapshot.MaxHealth} | Tickets {debugSnapshot.Tickets} | XP {debugSnapshot.Experience}";
            if (!debugSnapshot.HasActSchedule)
            {
                return summary;
            }

            summary += $" | {RunActScheduleRules.ActRomanLabel(debugSnapshot.ActNumber)} {debugSnapshot.ActName} {RunActScheduleRules.FormatCountdown(debugSnapshot.ActRemainingSeconds)}";
            if (debugSnapshot.IsActFinaleDue)
            {
                return summary + " | Headliner due";
            }

            if (debugSnapshot.IsShowtimeActive)
            {
                return summary + $" | SHOWTIME {RunActScheduleRules.FormatCountdown(debugSnapshot.ShowtimeRemainingSeconds)}";
            }

            if (debugSnapshot.IsEncoreActive)
            {
                if (debugSnapshot.IsEncoreGraceActive)
                {
                    return summary + $" | ENCORE IN {Mathf.CeilToInt(debugSnapshot.EncoreGraceRemainingSeconds)} | Pressure waiting";
                }

                return summary + $" | ENCORE {RunActScheduleRules.FormatElapsed(debugSnapshot.EncorePressureElapsedSeconds)} | Pressure {debugSnapshot.EncorePressureStep}";
            }

            if (debugSnapshot.NextShowtimeRemainingSeconds >= 0f)
            {
                return summary + $" | Next Showtime {RunActScheduleRules.FormatSecondsUntil(debugSnapshot.NextShowtimeRemainingSeconds)}";
            }

            return summary;
        }

        private HudPopupMotionSettings PopupMotionSettings()
        {
            return hudConfig != null ? hudConfig.popupMotion : HudPopupMotionSettings.Default;
        }

        private void BuildDebugUi(VisualElement parent)
        {
            AddSectionTitle(parent, "Designer Debug");

            AddSectionTitle(parent, "Run Loop");
            var runLoopPhaseRow = CreateRow();
            runLoopPhaseRow.Add(CreateDebugButton("Trigger Showtime", () => TriggerShowtimeRequested?.Invoke()));
            runLoopPhaseRow.Add(CreateDebugButton("Jump To Headliner", () => JumpToActFinaleRequested?.Invoke()));
            runLoopPhaseRow.Add(CreateDebugButton("Spawn Headliner", () => ActivateProxyHeadlinerRequested?.Invoke()));
            runLoopPhaseRow.Add(CreateDebugButton("Defeat Headliner", () => DefeatProxyHeadlinerRequested?.Invoke()));
            parent.Add(runLoopPhaseRow);

            var runLoopExitRow = CreateRow();
            runLoopExitRow.Add(CreateDebugButton("Skip Encore Grace", () => SkipEncoreGraceRequested?.Invoke()));
            runLoopExitRow.Add(CreateDebugButton("Complete Stage Door", () => CompleteStageDoorRequested?.Invoke()));
            runLoopExitRow.Add(CreateDebugButton("Enter Next Act", () => EnterNextActRequested?.Invoke()));
            parent.Add(runLoopExitRow);

            AddSectionTitle(parent, "World Layout");
            var worldLayoutRow = CreateRow();
            worldLayoutModeLabel = new Label { name = "pause-menu-world-layout" };
            worldLayoutModeLabel.style.minWidth = 170f;
            worldLayoutModeLabel.style.marginRight = 8f;
            worldLayoutModeLabel.style.color = new Color(0.78f, 0.88f, 0.96f, 1f);
            worldLayoutModeLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            worldLayoutModeButton = CreateDebugButton("Switch World Layout", () => SwitchWorldLayoutModeRequested?.Invoke());
            worldLayoutRow.Add(worldLayoutModeLabel);
            worldLayoutRow.Add(worldLayoutModeButton);
            parent.Add(worldLayoutRow);

            var resourceRow = CreateRow();
            ticketsField = CreateAmountField("Tickets", 25);
            xpField = CreateAmountField("XP", 25);
            healthField = CreateAmountField("Heal", 25);
            damageField = CreateAmountField("Damage", 10);
            resourceRow.Add(ticketsField);
            resourceRow.Add(CreateDebugButton("Add Tickets", () => AddTicketsRequested?.Invoke(Mathf.Max(1, ticketsField.value))));
            resourceRow.Add(xpField);
            resourceRow.Add(CreateDebugButton("Add XP", () => AddExperienceRequested?.Invoke(Mathf.Max(1, xpField.value))));
            resourceRow.Add(healthField);
            resourceRow.Add(CreateDebugButton("Heal", () => HealRequested?.Invoke(Mathf.Max(1, healthField.value))));
            resourceRow.Add(damageField);
            resourceRow.Add(CreateDebugButton("Damage", () => DamageRequested?.Invoke(Mathf.Max(1, damageField.value))));
            parent.Add(resourceRow);

            var weaponRow = CreateRow();
            weaponDropdown = CreateDropdown("Weapon");
            ownedWeaponDropdown = CreateDropdown("Owned Weapon");
            weaponRow.Add(weaponDropdown);
            weaponRow.Add(CreateDebugButton("Add Weapon", () => AddWeaponRequested?.Invoke(SelectedId(weaponDropdown, weaponOptions))));
            weaponRow.Add(ownedWeaponDropdown);
            weaponRow.Add(CreateDebugButton("Remove Weapon", () => RemoveWeaponRequested?.Invoke(SelectedId(ownedWeaponDropdown, ownedWeaponOptions))));
            parent.Add(weaponRow);

            var itemRow = CreateRow();
            itemDropdown = CreateDropdown("Item");
            itemAmountField = CreateAmountField("Stacks", 1);
            itemRow.Add(itemDropdown);
            itemRow.Add(itemAmountField);
            itemRow.Add(CreateDebugButton("Add Item", () => AddItemRequested?.Invoke(SelectedId(itemDropdown, itemOptions), Mathf.Max(1, itemAmountField.value))));
            itemRow.Add(CreateDebugButton("Remove Item", () => RemoveItemRequested?.Invoke(SelectedId(itemDropdown, itemOptions), Mathf.Max(1, itemAmountField.value))));
            parent.Add(itemRow);

            var talentRow = CreateRow();
            talentDropdown = CreateDropdown("Talent");
            talentRarityDropdown = CreateDropdown("Rarity");
            talentRow.Add(talentDropdown);
            talentRow.Add(talentRarityDropdown);
            talentRow.Add(CreateDebugButton("Apply Talent", () => ApplyTalentRequested?.Invoke(SelectedId(talentDropdown, talentOptions), SelectedRarity(talentRarityDropdown))));
            parent.Add(talentRow);

            var upgradeRow = CreateRow();
            upgradeDropdown = CreateDropdown("Weapon Upgrade");
            upgradeRarityDropdown = CreateDropdown("Rarity");
            upgradeRow.Add(upgradeDropdown);
            upgradeRow.Add(upgradeRarityDropdown);
            upgradeRow.Add(CreateDebugButton("Apply Upgrade", () => ApplyUpgradeRequested?.Invoke(SelectedId(upgradeDropdown, upgradeOptions), SelectedRarity(upgradeRarityDropdown))));
            parent.Add(upgradeRow);
        }

        private void EnsureWorldLayoutControls()
        {
            if (debugRoot == null || worldLayoutModeButton != null)
            {
                return;
            }

            var section = new VisualElement { name = "pause-menu-world-layout-section" };
            section.style.flexDirection = FlexDirection.Column;
            var title = new Label("World Layout");
            title.style.fontSize = 17f;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.color = Color.white;
            title.style.marginTop = 8f;
            title.style.marginBottom = 4f;
            section.Add(title);

            var row = CreateRow();
            worldLayoutModeLabel = new Label { name = "pause-menu-world-layout" };
            worldLayoutModeLabel.style.minWidth = 170f;
            worldLayoutModeLabel.style.marginRight = 8f;
            worldLayoutModeLabel.style.color = new Color(0.78f, 0.88f, 0.96f, 1f);
            worldLayoutModeLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            worldLayoutModeButton = CreateDebugButton("Switch World Layout", () => SwitchWorldLayoutModeRequested?.Invoke());
            row.Add(worldLayoutModeLabel);
            row.Add(worldLayoutModeButton);
            section.Add(row);
            debugRoot.Insert(0, section);
        }

        private void UpdateWorldLayoutControls(RunDebugSnapshot debugSnapshot)
        {
            if (worldLayoutModeLabel != null)
            {
                worldLayoutModeLabel.text = $"World: {WorldLayoutLabel(debugSnapshot.WorldLayoutMode)}";
            }

            if (worldLayoutModeButton != null)
            {
                worldLayoutModeButton.text = debugSnapshot.WorldLayoutMode == RunWorldLayoutMode.Generated
                    ? "Use Prototype Arena"
                    : "Use Generated Terrain";
            }
        }

        private static string WorldLayoutLabel(RunWorldLayoutMode mode)
        {
            return mode == RunWorldLayoutMode.Prototype ? "Prototype Arena" : "Generated Terrain";
        }

        private void PopulateItemInventory(IReadOnlyList<ItemInventoryCardFrame> frames)
        {
            if (inventoryList == null)
            {
                return;
            }

            inventoryList.Clear();
            bool hasItems = frames != null && frames.Count > 0;
            if (inventoryEmptyLabel != null)
            {
                inventoryEmptyLabel.style.display = hasItems ? DisplayStyle.None : DisplayStyle.Flex;
            }

            if (!hasItems)
            {
                return;
            }

            for (int i = 0; i < frames.Count; i++)
            {
                ItemInventoryCardFrame frame = frames[i];
                if (!frame.HasItem)
                {
                    continue;
                }

                inventoryList.Add(CreateItemCard(frame));
            }
        }

        private static VisualElement CreateItemCard(ItemInventoryCardFrame frame)
        {
            var card = new VisualElement { name = "pause-menu-item-card" };
            card.style.flexDirection = FlexDirection.Row;
            card.style.alignItems = Align.FlexStart;
            card.style.marginTop = 4f;
            card.style.marginBottom = 6f;
            card.style.paddingTop = 8f;
            card.style.paddingRight = 8f;
            card.style.paddingBottom = 8f;
            card.style.paddingLeft = 8f;
            card.style.backgroundColor = new Color(0.06f, 0.065f, 0.08f, 0.96f);
            card.style.borderTopWidth = 1f;
            card.style.borderRightWidth = 1f;
            card.style.borderBottomWidth = 1f;
            card.style.borderLeftWidth = 1f;
            card.style.borderTopColor = frame.RarityColor;
            card.style.borderRightColor = frame.RarityColor;
            card.style.borderBottomColor = frame.RarityColor;
            card.style.borderLeftColor = frame.RarityColor;

            var icon = new VisualElement { name = "pause-menu-item-icon" };
            icon.style.width = 74f;
            icon.style.height = 74f;
            icon.style.minWidth = 74f;
            icon.style.marginRight = 10f;
            icon.style.borderTopWidth = 2f;
            icon.style.borderRightWidth = 2f;
            icon.style.borderBottomWidth = 2f;
            icon.style.borderLeftWidth = 2f;
            ContentIconVisuals.Apply(icon, frame.IconSprite, frame.IconColor, frame.RarityColor);
            card.Add(icon);

            var copy = new VisualElement { name = "pause-menu-item-copy" };
            copy.style.flexGrow = 1f;
            copy.style.flexShrink = 1f;

            var header = new VisualElement { name = "pause-menu-item-header" };
            header.style.flexDirection = FlexDirection.Row;
            header.style.flexWrap = Wrap.Wrap;
            header.style.alignItems = Align.Center;

            var title = new Label(frame.DisplayName) { name = "pause-menu-item-title" };
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.fontSize = 16f;
            title.style.color = Color.white;
            title.style.marginRight = 8f;
            header.Add(title);

            var rarity = new Label(frame.RarityLabel) { name = "pause-menu-item-rarity" };
            rarity.style.unityFontStyleAndWeight = FontStyle.Bold;
            rarity.style.color = frame.RarityColor;
            rarity.style.marginRight = 8f;
            header.Add(rarity);

            var stack = new Label(frame.StackText) { name = "pause-menu-item-stack" };
            stack.style.color = new Color(0.78f, 0.88f, 0.96f, 1f);
            header.Add(stack);
            copy.Add(header);

            var description = new Label(frame.ShortDescription) { name = "pause-menu-item-description" };
            description.style.color = new Color(0.78f, 0.82f, 0.88f, 1f);
            description.style.marginTop = 2f;
            copy.Add(description);

            var effect = new Label(frame.EffectText) { name = "pause-menu-item-effect" };
            effect.enableRichText = true;
            effect.style.color = new Color(0.9f, 0.95f, 1f, 1f);
            effect.style.marginTop = 5f;
            copy.Add(effect);

            card.Add(copy);
            return card;
        }

        private Button CreateButton(string text, Action callback)
        {
            var button = new Button(callback) { text = text };
            button.AddToClassList("marquee-button");
            button.AddToClassList("pause-menu-button");
            button.style.marginRight = 8f;
            button.style.marginBottom = 6f;
            button.style.unityFontStyleAndWeight = FontStyle.Bold;
            RegisterMarqueeButton(button);
            return button;
        }

        private Button CreateDebugButton(string text, Action callback)
        {
            Button button = CreateButton(text, callback);
            button.AddToClassList("pause-menu-debug-button");
            return button;
        }

        private void RegisterMarqueeButton(VisualElement button)
        {
            if (button == null)
            {
                return;
            }

            marqueeEffects ??= new MarqueeButtonEffectController(marqueeSettings);
            marqueeEffects.Register(button);
        }

        private static IntegerField CreateAmountField(string label, int value)
        {
            var field = new IntegerField(label) { value = value };
            field.AddToClassList("pause-menu-debug-field");
            field.style.minWidth = 92f;
            field.style.marginRight = 8f;
            return field;
        }

        private static DropdownField CreateDropdown(string label)
        {
            var dropdown = new DropdownField(label, new List<string> { "None" }, 0);
            dropdown.AddToClassList("pause-menu-debug-dropdown");
            dropdown.style.minWidth = 170f;
            dropdown.style.marginRight = 8f;
            return dropdown;
        }

        private static VisualElement CreateRow()
        {
            var row = new VisualElement();
            row.AddToClassList("pause-menu-debug-row");
            row.style.flexDirection = FlexDirection.Row;
            row.style.flexWrap = Wrap.Wrap;
            row.style.marginTop = 5f;
            row.style.marginBottom = 5f;
            return row;
        }

        private static void AddSectionTitle(VisualElement parent, string text)
        {
            var label = new Label(text);
            label.AddToClassList("pause-menu-section-title");
            label.style.fontSize = 17f;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.color = Color.white;
            label.style.marginTop = 8f;
            label.style.marginBottom = 4f;
            parent.Add(label);
        }

        private static void PopulateOptions(DropdownField dropdown, List<RunDebugOption> target, IReadOnlyList<RunDebugOption> source)
        {
            if (dropdown == null || target == null)
            {
                return;
            }

            string previous = dropdown.value;
            target.Clear();
            var choices = new List<string> { "None" };
            if (source != null)
            {
                for (int i = 0; i < source.Count; i++)
                {
                    RunDebugOption option = source[i];
                    if (!option.IsValid)
                    {
                        continue;
                    }

                    target.Add(option);
                    choices.Add(option.Label);
                }
            }

            dropdown.choices = choices;
            dropdown.index = Mathf.Clamp(choices.IndexOf(previous), 0, choices.Count - 1);
        }

        private static void EnsureRarityChoices(DropdownField dropdown)
        {
            if (dropdown == null)
            {
                return;
            }

            dropdown.choices = new List<string>
            {
                ContentRarity.Common.ToString(),
                ContentRarity.Uncommon.ToString(),
                ContentRarity.Rare.ToString(),
                ContentRarity.Epic.ToString(),
                ContentRarity.Legendary.ToString()
            };
            if (dropdown.index < 0)
            {
                dropdown.index = 0;
            }
        }

        private static string SelectedId(DropdownField dropdown, IReadOnlyList<RunDebugOption> options)
        {
            if (dropdown == null || options == null || dropdown.index <= 0)
            {
                return string.Empty;
            }

            int optionIndex = dropdown.index - 1;
            return optionIndex >= 0 && optionIndex < options.Count ? options[optionIndex].Id : string.Empty;
        }

        private static ContentRarity SelectedRarity(DropdownField dropdown)
        {
            if (dropdown == null || string.IsNullOrWhiteSpace(dropdown.value))
            {
                return ContentRarity.Common;
            }

            return Enum.TryParse(dropdown.value, out ContentRarity rarity) ? rarity : ContentRarity.Common;
        }
    }
}
