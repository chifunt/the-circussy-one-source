using System;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheCircussyOne.Visuals
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class RewardRevealView : MonoBehaviour
    {
        [SerializeField] private UIDocument document;
        [SerializeField] private StyleSheet styleSheet;
        [SerializeField] private MarqueeButtonEffectSettings marqueeSettings = new();

        private VisualElement root;
        private VisualElement overlay;
        private VisualElement rayLayer;
        private VisualElement card;
        private VisualElement dismissRow;
        private VisualElement icon;
        private Label titleGlowLabel;
        private Label titleLabel;
        private Label rarityLabel;
        private Label nameLabel;
        private Label descriptionLabel;
        private Label effectLabel;
        private Label stackLabel;
        private Button dismissButton;
        private Button registeredDismissButton;
        private bool styleApplied;
        private HudVisualConfig hudConfig;
        private readonly UiPopupMotionDriver popupMotion = new();
        private readonly UiCelebrationEffectController celebrationEffects = new();
        private MarqueeButtonEffectController marqueeEffects;
        private bool dismissReady;
        private bool dismissRevealStarted;
        private float dismissRevealDelayElapsed;
        private float dismissRevealElapsed;

        public event Action DismissRequested;
        public bool IsVisible { get; private set; }
        public bool CanDismiss => IsVisible && dismissReady;

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
            UnregisterDismissButton();
            marqueeEffects?.Dispose();
            marqueeEffects = null;
        }

        public void ConfigureForTests(
            VisualElement overlay,
            VisualElement card,
            VisualElement icon,
            Label titleLabel,
            Label rarityLabel,
            Label nameLabel,
            Label descriptionLabel,
            Label stackLabel,
            Button dismissButton)
        {
            ConfigureForTests(
                overlay,
                card,
                icon,
                titleLabel,
                rarityLabel,
                nameLabel,
                descriptionLabel,
                null,
                stackLabel,
                dismissButton);
        }

        public void ConfigureForTests(
            VisualElement overlay,
            VisualElement card,
            VisualElement icon,
            Label titleLabel,
            Label rarityLabel,
            Label nameLabel,
            Label descriptionLabel,
            Label effectLabel,
            Label stackLabel,
            Button dismissButton)
        {
            this.overlay = overlay;
            this.card = card;
            this.icon = icon;
            this.titleLabel = titleLabel;
            this.rarityLabel = rarityLabel;
            this.nameLabel = nameLabel;
            this.descriptionLabel = descriptionLabel;
            this.effectLabel = effectLabel;
            this.stackLabel = stackLabel;
            this.dismissButton = dismissButton;
            RegisterDismissButton();
        }

        public void Show(RewardRevealFrame frame)
        {
            Resolve();
            IsVisible = frame.HasReward;
            if (!IsVisible)
            {
                PrepareDismissButtonForHiddenState();
                popupMotion.HideImmediate(overlay, card);
                celebrationEffects.HideImmediate();
                return;
            }

            PrepareDismissButtonForReveal();
            popupMotion.Show(overlay, card, PopupMotionSettings());
            celebrationEffects.Show();
            card?.EnableInClassList("is-selected", true);

            ContentRarityInfo rarity = ContentRarityMetadata.Get(frame.Rarity);
            SetText(titleGlowLabel, frame.Title);
            SetText(titleLabel, frame.Title);
            SetText(rarityLabel, frame.RarityLabel);
            SetText(nameLabel, frame.DisplayName);
            SetText(descriptionLabel, frame.ShortDescription);
            SetText(effectLabel, frame.EffectText);
            SetText(stackLabel, frame.StackText);
            if (effectLabel != null)
            {
                effectLabel.enableRichText = true;
            }

            if (rarityLabel != null)
            {
                rarityLabel.style.color = rarity.Color;
            }

            if (card != null)
            {
                card.style.borderTopColor = rarity.Color;
                card.style.borderBottomColor = rarity.Color;
                card.style.borderLeftColor = rarity.Color;
                card.style.borderRightColor = rarity.Color;
            }

            ContentIconVisuals.Apply(icon, frame.IconSprite, frame.IconColor, rarity.Color);
        }

        public void Hide()
        {
            Resolve();
            IsVisible = false;
            card?.EnableInClassList("is-selected", false);
            PrepareDismissButtonForHiddenState();
            popupMotion.HideImmediate(overlay, card);
            celebrationEffects.HideImmediate();
        }

        public void ApplyConfig(HudVisualConfig config)
        {
            hudConfig = config;
            hudConfig?.EnsureReadableDefaults();
        }

        public void TickPopupMotion(float unscaledDeltaTime)
        {
            popupMotion.Tick(card ?? overlay, unscaledDeltaTime, PopupMotionSettings());
            TickDismissReveal(unscaledDeltaTime);
            marqueeEffects?.Tick(unscaledDeltaTime, Time.unscaledTime);
            celebrationEffects.Tick(unscaledDeltaTime, Time.unscaledTime);
        }

        private void Resolve()
        {
            if (document == null)
            {
                document = GetComponent<UIDocument>();
            }

            if (document == null || document.rootVisualElement == null)
            {
                RegisterDismissButton();
                return;
            }

            root = document.rootVisualElement;
            if (!styleApplied && styleSheet != null)
            {
                root.styleSheets.Add(styleSheet);
                styleApplied = true;
            }

            overlay ??= root.Q<VisualElement>("reward-reveal");
            rayLayer ??= root.Q<VisualElement>("reward-reveal-ray-layer");
            card ??= root.Q<VisualElement>("reward-reveal-card");
            dismissRow ??= root.Q<VisualElement>("reward-reveal-dismiss-row");
            icon ??= root.Q<VisualElement>("reward-reveal-icon");
            titleGlowLabel ??= root.Q<Label>("reward-reveal-title-glow");
            titleLabel ??= root.Q<Label>("reward-reveal-title");
            rarityLabel ??= root.Q<Label>("reward-reveal-rarity");
            nameLabel ??= root.Q<Label>("reward-reveal-name");
            descriptionLabel ??= root.Q<Label>("reward-reveal-description");
            effectLabel ??= root.Q<Label>("reward-reveal-effect");
            stackLabel ??= root.Q<Label>("reward-reveal-stack");
            dismissButton ??= root.Q<Button>("reward-reveal-dismiss");
            if (effectLabel != null)
            {
                effectLabel.enableRichText = true;
            }

            RegisterDismissButton();
            RegisterMarqueeEffects();
            RegisterCelebrationEffects();
        }

        private void RegisterMarqueeEffects()
        {
            marqueeEffects ??= new MarqueeButtonEffectController(marqueeSettings);
            marqueeEffects.Register(card);
            marqueeEffects.Register(dismissButton);
        }

        private void RegisterCelebrationEffects()
        {
            celebrationEffects.Bind(rayLayer);
        }

        private void RegisterDismissButton()
        {
            if (dismissButton == null || registeredDismissButton == dismissButton)
            {
                return;
            }

            UnregisterDismissButton();
            registeredDismissButton = dismissButton;
            registeredDismissButton.clicked += OnDismissClicked;
        }

        private void UnregisterDismissButton()
        {
            if (registeredDismissButton == null)
            {
                return;
            }

            registeredDismissButton.clicked -= OnDismissClicked;
            registeredDismissButton = null;
        }

        private void OnDismissClicked()
        {
            if (!CanDismiss)
            {
                return;
            }

            DismissRequested?.Invoke();
        }

        private void PrepareDismissButtonForReveal()
        {
            dismissReady = false;
            dismissRevealStarted = false;
            dismissRevealDelayElapsed = 0f;
            dismissRevealElapsed = 0f;

            if (dismissButton == null)
            {
                return;
            }

            dismissButton.SetEnabled(false);
            dismissButton.pickingMode = PickingMode.Ignore;
            dismissButton.EnableInClassList("is-selected", false);
            dismissButton.style.visibility = Visibility.Hidden;
            HudPopupMotionSettings settings = RewardRevealAcceptMotionSettings();
            settings.EnsureDefaults(true, 0.2f, 0.08f, 1f, true, EaseSettings.OutBounce);
            ApplyDismissButtonFrame(dismissButton, new UiPopupMotionFrame(settings.startScale, 0f));
        }

        private void PrepareDismissButtonForHiddenState()
        {
            dismissReady = false;
            dismissRevealStarted = false;
            dismissRevealDelayElapsed = 0f;
            dismissRevealElapsed = 0f;

            if (dismissButton == null)
            {
                return;
            }

            dismissButton.SetEnabled(false);
            dismissButton.pickingMode = PickingMode.Ignore;
            dismissButton.EnableInClassList("is-selected", false);
            dismissButton.style.visibility = Visibility.Hidden;
            ApplyDismissButtonFrame(dismissButton, new UiPopupMotionFrame(1f, 1f));
        }

        private void TickDismissReveal(float unscaledDeltaTime)
        {
            if (!IsVisible || dismissButton == null || (dismissReady && !dismissRevealStarted))
            {
                return;
            }

            float deltaTime = Mathf.Max(0f, unscaledDeltaTime);
            float delaySeconds = RewardRevealAcceptDelaySeconds();
            if (!dismissRevealStarted && dismissRevealDelayElapsed < delaySeconds)
            {
                dismissRevealDelayElapsed += deltaTime;
                if (dismissRevealDelayElapsed < delaySeconds)
                {
                    return;
                }
            }

            if (!dismissRevealStarted)
            {
                StartDismissReveal();
            }

            HudPopupMotionSettings settings = RewardRevealAcceptMotionSettings();
            settings.EnsureDefaults(true, 0.2f, 0.08f, 1f, true, EaseSettings.OutBounce);
            if (!settings.enabled)
            {
                FinishDismissReveal(settings);
                return;
            }

            dismissRevealElapsed += deltaTime;
            ApplyDismissButtonFrame(dismissButton, UiPopupMotionDriver.Evaluate(dismissRevealElapsed, settings));
            if (dismissRevealElapsed >= Mathf.Max(0.01f, settings.seconds))
            {
                FinishDismissReveal(settings);
            }
        }

        private void StartDismissReveal()
        {
            dismissRevealStarted = true;
            dismissReady = true;
            dismissButton.style.visibility = Visibility.Visible;
            dismissButton.SetEnabled(true);
            dismissButton.pickingMode = PickingMode.Position;
            dismissButton.EnableInClassList("is-selected", true);
            dismissButton.Focus();
            marqueeEffects?.Pulse(dismissButton);
        }

        private void FinishDismissReveal(HudPopupMotionSettings settings)
        {
            dismissRevealStarted = false;
            dismissReady = true;
            ApplyDismissButtonFrame(dismissButton, new UiPopupMotionFrame(settings.endScale, 1f));
        }

        private float RewardRevealAcceptDelaySeconds()
        {
            return hudConfig != null ? Mathf.Max(0f, hudConfig.rewardRevealAcceptDelaySeconds) : 0.5f;
        }

        private HudPopupMotionSettings RewardRevealAcceptMotionSettings()
        {
            return hudConfig != null
                ? hudConfig.rewardRevealAcceptMotion
                : new HudPopupMotionSettings(true, 0.2f, 0.08f, 1f, true, EaseSettings.OutBounce);
        }

        private static void ApplyDismissButtonFrame(VisualElement target, UiPopupMotionFrame frame)
        {
            if (target == null)
            {
                return;
            }

            target.style.scale = new Scale(new Vector3(frame.Scale, frame.Scale, 1f));
            target.style.opacity = frame.Opacity;
        }

        private static void SetText(Label label, string text)
        {
            if (label != null)
            {
                label.text = text ?? string.Empty;
            }
        }

        private HudPopupMotionSettings PopupMotionSettings()
        {
            return hudConfig != null ? hudConfig.popupMotion : HudPopupMotionSettings.Default;
        }
    }
}
