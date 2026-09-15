using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using TheCircussyOne.Visuals;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace TheCircussyOne.Runtime
{
    public sealed class MainMenuController : MonoBehaviour
    {
        private const string PerformerCardTemplatePath = "Assets/Game/UI/Templates/MainMenuPerformerCard.uxml";

        private enum MenuScreen
        {
            Main,
            PerformerSelect,
            Settings
        }

        [SerializeField] private UIDocument document;
        [SerializeField] private StyleSheet styleSheet;
        [SerializeField] private VisualTreeAsset performerCardTemplate;
        [SerializeField] private PerformerCatalog performerCatalog;
        [SerializeField] private HudVisualConfig transitionConfig;
        [SerializeField] private GameAudioConfig audioConfig;
        [SerializeField] private GameHapticsConfig hapticsConfig;
        [SerializeField] private Camera menuCamera;
        [SerializeField] private Transform mainMenuCameraPose;
        [SerializeField] private Transform performerCameraPose;
        [SerializeField] private MenuPerformerPreviewView performerPreview;
        [SerializeField] private string gameplaySceneName = "TheCircussyOne";
        [SerializeField, Min(0f)] private float transitionMinimumVisibleSeconds = 0.9f;
        [SerializeField, Min(0.01f)] private float cameraMoveSeconds = 0.45f;
        [SerializeField] private bool uiAudioEnabled = true;
        [SerializeField] private bool uiHapticsEnabled = true;
        [SerializeField, Range(0.1f, 0.95f)] private float navigationStickThreshold = MenuNavigationRules.DefaultStickThreshold;
        [SerializeField, Min(0.05f)] private float navigationInitialRepeatDelay = MenuNavigationRules.InitialRepeatDelay;
        [SerializeField, Min(0.05f)] private float navigationRepeatInterval = MenuNavigationRules.RepeatInterval;
        [SerializeField] private MarqueeButtonEffectSettings marqueeSettings = new();

        private readonly List<Button> mainButtons = new();
        private readonly List<Button> performerRows = new();
        private readonly List<PerformerSelectionCardFrame> performerFrames = new();
        private readonly HashSet<string> loadedMenuAudioBanks = new(StringComparer.Ordinal);

        private VisualElement root;
        private VisualElement mainScreen;
        private VisualElement performerScreen;
        private VisualElement settingsScreen;
        private ScrollView performerList;
        private Label performerName;
        private Label performerTitle;
        private Label performerDescription;
        private Label performerStats;
        private Label weaponName;
        private Label weaponDescription;
        private VisualElement weaponIcon;
        private Button performerStartButton;
        private Button performerBackButton;
        private Button settingsBackButton;
        private MarqueeButtonEffectController marqueeEffects;
        private IWwiseRuntimeAudioBackend menuAudioBackend;
        private GameObject menuAudioEmitter;
        private GameHapticsService menuHaptics;
        private bool performerCardTemplateWarningLogged;
        private bool menuMusicPlaying;
        private bool preserveMenuAudioForTransition;

        private MenuScreen activeScreen;
        private int mainFocusIndex;
        private int selectedPerformerIndex;
        private MenuNavigationRepeatState verticalNavigationRepeatState;
        private Vector3 cameraStartPosition;
        private Quaternion cameraStartRotation;
        private Transform cameraTargetPose;
        private float cameraMoveElapsed;
        private float cameraStartFieldOfView;
        private float cameraTargetFieldOfView;
        private bool cameraMoveActive;
        private bool isStartingRun;

        private void Awake()
        {
            document ??= GetComponent<UIDocument>();
            menuCamera ??= Camera.main;
        }

        private void OnEnable()
        {
            BindDocument();
        }

        private void Update()
        {
            TickCameraMove();
            TickNavigation();
            marqueeEffects?.Tick(Time.unscaledDeltaTime, Time.unscaledTime);
            menuHaptics?.Tick();
        }

        private void OnDisable()
        {
            marqueeEffects?.Dispose();
            marqueeEffects = null;
            DisposeMenuHaptics();
        }

        private void OnDestroy()
        {
            DisposeMenuHaptics();
            if (menuAudioEmitter != null)
            {
                if (preserveMenuAudioForTransition)
                {
                    PreserveMenuAudioEmitter();
                    menuAudioEmitter = null;
                }
                else
                {
                    if (menuMusicPlaying)
                    {
                        StopMenuMusic();
                    }

                    GameAudioEmitterFactory.Release(menuAudioEmitter, 4f);
                    menuAudioEmitter = null;
                }
            }

            loadedMenuAudioBanks.Clear();
        }

        private void BindDocument()
        {
            if (document == null)
            {
                Debug.LogWarning("MainMenuController has no UIDocument.");
                return;
            }

            root = document.rootVisualElement;
            if (root == null)
            {
                return;
            }

            SuppressBuiltInNavigationEvents(root);

            if (styleSheet != null && !root.styleSheets.Contains(styleSheet))
            {
                root.styleSheets.Add(styleSheet);
            }

            mainScreen = root.Q<VisualElement>("main-menu-screen");
            performerScreen = root.Q<VisualElement>("performer-select-screen");
            settingsScreen = root.Q<VisualElement>("settings-screen");
            performerList = root.Q<ScrollView>("performer-list");
            performerName = root.Q<Label>("performer-name");
            performerTitle = root.Q<Label>("performer-title");
            performerDescription = root.Q<Label>("performer-description");
            performerStats = root.Q<Label>("performer-stats");
            weaponName = root.Q<Label>("weapon-name");
            weaponDescription = root.Q<Label>("weapon-description");
            weaponIcon = root.Q<VisualElement>("weapon-icon");
            performerStartButton = root.Q<Button>("performer-start-button");
            performerBackButton = root.Q<Button>("performer-back-button");
            settingsBackButton = root.Q<Button>("settings-back-button");
            if (performerList != null)
            {
                performerList.verticalScrollerVisibility = ScrollerVisibility.Hidden;
                performerList.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            }

            Button startButton = root.Q<Button>("start-button");
            Button settingsButton = root.Q<Button>("settings-button");
            Button quitButton = root.Q<Button>("quit-button");
            mainButtons.Clear();
            AddMainButton(startButton, ShowPerformerSelect);
            AddMainButton(settingsButton, ShowSettings);
            AddMainButton(quitButton, QuitGame);

            if (performerBackButton != null)
            {
                ConfigureManualButton(performerBackButton);
                performerBackButton.clicked += () =>
                {
                    PlayUiClick();
                    marqueeEffects?.Pulse(performerBackButton);
                    ShowMain();
                };
                performerBackButton.RegisterCallback<PointerEnterEvent>(_ => PlayUiHover());
                performerBackButton.BringToFront();
            }

            if (settingsBackButton != null)
            {
                ConfigureManualButton(settingsBackButton);
                settingsBackButton.clicked += () =>
                {
                    PlayUiClick();
                    marqueeEffects?.Pulse(settingsBackButton);
                    ShowMain();
                };
                settingsBackButton.RegisterCallback<PointerEnterEvent>(_ => PlayUiHover());
            }

            if (performerStartButton != null)
            {
                ConfigureManualButton(performerStartButton);
                performerStartButton.clicked += () =>
                {
                    PlayUiClick();
                    marqueeEffects?.Pulse(performerStartButton);
                    StartSelectedRun();
                };
                performerStartButton.RegisterCallback<PointerEnterEvent>(_ => PlayUiHover());
                performerStartButton.BringToFront();
            }

            BuildPerformerRows();
            RebuildMarqueeEffects();
            ShowMain(instantCamera: true);
            StartMenuMusic();
        }

        private void RebuildMarqueeEffects()
        {
            marqueeEffects ??= new MarqueeButtonEffectController(marqueeSettings);
            marqueeEffects.Clear();
            for (int i = 0; i < mainButtons.Count; i++)
            {
                marqueeEffects.Register(mainButtons[i]);
            }

            marqueeEffects.Register(performerBackButton);
            marqueeEffects.Register(settingsBackButton);
            marqueeEffects.Register(performerStartButton);
            for (int i = 0; i < performerRows.Count; i++)
            {
                marqueeEffects.Register(performerRows[i]);
            }
        }

        private void AddMainButton(Button button, System.Action action)
        {
            if (button == null)
            {
                return;
            }

            int index = mainButtons.Count;
            mainButtons.Add(button);
            ConfigureManualButton(button);
            button.clicked += () =>
            {
                PlayUiClick();
                marqueeEffects?.Pulse(button);
                action?.Invoke();
            };
            button.RegisterCallback<PointerEnterEvent>(_ => SetMainFocus(index));
        }

        private void BuildPerformerRows()
        {
            performerRows.Clear();
            performerFrames.Clear();
            if (performerList == null)
            {
                return;
            }

            performerList.contentContainer.Clear();
            IReadOnlyList<PerformerSelectionCardFrame> frames = PerformerSelectionPreviewRules.BuildFrames(performerCatalog?.Performers);
            for (int i = 0; i < frames.Count; i++)
            {
                int index = i;
                PerformerSelectionCardFrame frame = frames[i];
                performerFrames.Add(frame);
                Button row = CreatePerformerRow(i, frame, out VisualElement rowRoot);
                ConfigureManualButton(row);
                row.clicked += () =>
                {
                    PlayUiClick();
                    SelectPerformer(index, startRun: false);
                };
                row.RegisterCallback<PointerEnterEvent>(_ => SelectPerformer(index, startRun: false));

                performerRows.Add(row);
                performerList.Add(rowRoot);
            }

            if (performerFrames.Count == 0)
            {
                var empty = new Label("No performers are available.");
                empty.AddToClassList("menu-empty");
                performerList.Add(empty);
            }

            selectedPerformerIndex = performerFrames.Count > 0 ? 0 : -1;
            ApplySelectedPerformer();
        }

        private Button CreatePerformerRow(int index, PerformerSelectionCardFrame frame, out VisualElement rowRoot)
        {
            VisualTreeAsset cardTemplate = ResolvePerformerCardTemplate();
            if (cardTemplate != null)
            {
                TemplateContainer container = cardTemplate.Instantiate();
                container.name = $"performer-row-container-{index}";
                container.AddToClassList("performer-card-template");

                Button templateButton = container.Q<Button>("performer-card-button");
                if (templateButton != null)
                {
                    templateButton.name = $"performer-row-{index}";
                    templateButton.AddToClassList("marquee-button");
                    templateButton.AddToClassList("performer-row");
                    ApplyPerformerPortrait(container.Q<VisualElement>("performer-card-portrait"), frame);
                    rowRoot = container;
                    return templateButton;
                }

                LogPerformerTemplateWarning();
            }

            Button row = new() { name = $"performer-row-{index}" };
            row.AddToClassList("marquee-button");
            row.AddToClassList("performer-row");
            row.AddToClassList("performer-card-template");

            VisualElement cardFrame = new() { name = $"performer-card-frame-{index}" };
            cardFrame.AddToClassList("marquee-frame");
            cardFrame.AddToClassList("performer-card__frame");

            VisualElement portrait = new() { name = $"performer-portrait-{index}" };
            portrait.AddToClassList("performer-row__portrait");
            portrait.AddToClassList("performer-card__portrait");
            ApplyPerformerPortrait(portrait, frame);
            cardFrame.Add(portrait);
            row.Add(cardFrame);

            rowRoot = row;
            return row;
        }

        private VisualTreeAsset ResolvePerformerCardTemplate()
        {
#if UNITY_EDITOR
            if (performerCardTemplate == null)
            {
                performerCardTemplate = UnityEditor.AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(PerformerCardTemplatePath);
            }
#endif
            return performerCardTemplate;
        }

        private static void ApplyPerformerPortrait(VisualElement portrait, PerformerSelectionCardFrame frame)
        {
            if (portrait == null)
            {
                return;
            }

            PerformerDefinition performer = frame.Performer;
            Color fallbackColor = performer != null
                ? performer.portraitColor
                : new Color(0.85f, 0.65f, 0.25f, 1f);
            ContentIconVisuals.Apply(portrait, performer != null ? performer.portraitSprite : null, fallbackColor);
        }

        private void LogPerformerTemplateWarning()
        {
            if (performerCardTemplateWarningLogged)
            {
                return;
            }

            performerCardTemplateWarningLogged = true;
            Debug.LogWarning("MainMenu performer card template is missing a Button named performer-card-button. Falling back to generated performer rows.");
        }

        private void ShowMain()
        {
            ShowMain(instantCamera: false);
        }

        private void ShowMain(bool instantCamera)
        {
            activeScreen = MenuScreen.Main;
            ResetNavigationRepeat(requireNeutralRelease: !instantCamera);
            ShowScreen(mainScreen, true);
            ShowScreen(performerScreen, false);
            ShowScreen(settingsScreen, false);
            performerPreview?.Hide();
            SetMainFocus(Mathf.Clamp(mainFocusIndex, 0, Mathf.Max(0, mainButtons.Count - 1)), playAudio: false);
            MoveCameraTo(mainMenuCameraPose, instantCamera);
        }

        private void ShowPerformerSelect()
        {
            activeScreen = MenuScreen.PerformerSelect;
            ResetNavigationRepeat(requireNeutralRelease: true);
            ShowScreen(mainScreen, false);
            ShowScreen(performerScreen, true);
            ShowScreen(settingsScreen, false);
            if (selectedPerformerIndex < 0 && performerFrames.Count > 0)
            {
                selectedPerformerIndex = 0;
            }

            ApplySelectedPerformer();
            MoveCameraTo(performerCameraPose, instant: false);
        }

        private void ShowSettings()
        {
            activeScreen = MenuScreen.Settings;
            ResetNavigationRepeat(requireNeutralRelease: true);
            ShowScreen(mainScreen, false);
            ShowScreen(performerScreen, false);
            ShowScreen(settingsScreen, true);
            MoveCameraTo(mainMenuCameraPose, instant: false);
        }

        private void SetMainFocus(int index, bool playAudio = true)
        {
            if (mainButtons.Count == 0)
            {
                mainFocusIndex = -1;
                return;
            }

            bool changed = mainFocusIndex != index;
            mainFocusIndex = Mathf.Clamp(index, 0, mainButtons.Count - 1);
            for (int i = 0; i < mainButtons.Count; i++)
            {
                mainButtons[i].EnableInClassList("is-focused", i == mainFocusIndex);
            }

            if (changed && playAudio)
            {
                PlayUiHover();
            }
        }

        private void SelectPerformer(int index, bool startRun, bool playAudio = true)
        {
            if (index < 0 || index >= performerFrames.Count)
            {
                return;
            }

            bool changed = selectedPerformerIndex != index;
            selectedPerformerIndex = index;
            ApplySelectedPerformer();
            if (changed && playAudio)
            {
                PlayUiHover();
            }

            if (startRun)
            {
                StartSelectedRun();
            }
        }

        private void ApplySelectedPerformer()
        {
            for (int i = 0; i < performerRows.Count; i++)
            {
                performerRows[i].EnableInClassList("is-selected", i == selectedPerformerIndex);
            }

            if (selectedPerformerIndex >= 0 && selectedPerformerIndex < performerRows.Count)
            {
                performerList?.ScrollTo(performerRows[selectedPerformerIndex]);
            }

            bool hasSelection = selectedPerformerIndex >= 0 && selectedPerformerIndex < performerFrames.Count;
            performerStartButton?.SetEnabled(hasSelection && !isStartingRun);
            if (!hasSelection)
            {
                SetText(performerName, "No Performer");
                SetText(performerTitle, string.Empty);
                SetText(performerDescription, "Add performers to the catalog to start a run.");
                SetText(performerStats, string.Empty);
                SetText(weaponName, "Missing Weapon");
                SetText(weaponDescription, string.Empty);
                ContentIconVisuals.Clear(weaponIcon, new Color(0.9f, 0.72f, 0.25f, 1f));
                performerPreview?.Show(null);
                return;
            }

            PerformerSelectionCardFrame frame = performerFrames[selectedPerformerIndex];
            PerformerDefinition performer = frame.Performer;
            SetText(performerName, performer != null ? performer.DisplayName : "Missing Performer");
            SetText(performerTitle, performer != null ? performer.PerformerTitle : string.Empty);
            SetText(performerDescription, performer != null ? performer.shortDescription : string.Empty);
            SetText(performerStats, frame.StatPreview);
            SetText(weaponName, frame.StartingWeapon);
            SetText(weaponDescription, frame.StartingWeaponDefinition != null ? frame.StartingWeaponDefinition.AddWeaponShortDescription : string.Empty);
            if (weaponIcon != null)
            {
                WeaponDefinition weapon = frame.StartingWeaponDefinition;
                Color fallbackColor = weapon != null
                    ? weapon.projectilePrimaryColor
                    : new Color(0.9f, 0.72f, 0.25f, 1f);
                ContentIconVisuals.Apply(weaponIcon, weapon != null ? weapon.iconSprite : null, fallbackColor);
            }

            performerPreview?.Show(performer);
        }

        private void StartSelectedRun()
        {
            if (isStartingRun || selectedPerformerIndex < 0 || selectedPerformerIndex >= performerFrames.Count)
            {
                return;
            }

            PerformerDefinition performer = performerFrames[selectedPerformerIndex].Performer;
            if (performer == null)
            {
                return;
            }

            isStartingRun = true;
            performerStartButton?.SetEnabled(false);
            StartMenuMusic();
            preserveMenuAudioForTransition = true;
            PreserveMenuAudioEmitter();
            IWwiseRuntimeAudioBackend transitionAudioBackend = menuAudioBackend;
            GameObject transitionAudioEmitter = menuAudioEmitter;
            GameAudioConfig transitionAudioConfig = audioConfig;
            PendingRunLaunch.SetPerformer(performer.Id, waitForGameplayReady: true);
            RunTransitionOverlayHost overlay = RunTransitionOverlayHost.GetOrCreate(document != null ? document.panelSettings : null);
            bool started = overlay.Play(
                transitionConfig,
                "Setting the Stage",
                "Loading the opening act...",
                () => LoadGameplayAndWaitAsync(gameplaySceneName, overlay),
                () => CompleteGameplayLaunch(transitionAudioBackend, transitionAudioEmitter, transitionAudioConfig),
                minimumVisibleSecondsOverride: transitionMinimumVisibleSeconds);
            if (!started)
            {
                LoadGameplayAndReleaseAsync(
                    gameplaySceneName,
                    overlay,
                    transitionAudioBackend,
                    transitionAudioEmitter,
                    transitionAudioConfig).Forget();
            }
        }

        private static async UniTaskVoid LoadGameplayAndReleaseAsync(
            string sceneName,
            RunTransitionOverlayHost overlay,
            IWwiseRuntimeAudioBackend audioBackend,
            GameObject audioEmitter,
            GameAudioConfig audioConfig)
        {
            await LoadGameplayAndWaitAsync(sceneName, overlay);
            CompleteGameplayLaunch(audioBackend, audioEmitter, audioConfig);
        }

        private static async UniTask LoadGameplayAndWaitAsync(
            string sceneName,
            RunTransitionOverlayHost overlay)
        {
            overlay?.SetLoadingStage("Loading the Big Top");
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            while (operation != null && !operation.isDone)
            {
                await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            }

            overlay?.SetLoadingStage("Generating Act I");
            await PendingRunLaunch.WaitForGameplayReadyAsync();
            overlay?.SetLoadingStage("Opening the Curtain");
            await WaitForLaunchSubmitReleaseAsync();
        }

        private static void CompleteGameplayLaunch(
            IWwiseRuntimeAudioBackend audioBackend,
            GameObject audioEmitter,
            GameAudioConfig audioConfig)
        {
            PendingRunLaunch.ReleaseGameplay();
            StopCapturedMenuMusicAfterGameplayReleaseAsync(audioBackend, audioEmitter, audioConfig).Forget();
        }

        private static async UniTaskVoid StopCapturedMenuMusicAfterGameplayReleaseAsync(
            IWwiseRuntimeAudioBackend audioBackend,
            GameObject audioEmitter,
            GameAudioConfig audioConfig)
        {
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            StopCapturedMenuMusic(audioBackend, audioEmitter, audioConfig);
        }

        private static void StopCapturedMenuMusic(
            IWwiseRuntimeAudioBackend audioBackend,
            GameObject audioEmitter,
            GameAudioConfig audioConfig)
        {
            if (audioBackend == null || audioEmitter == null || audioConfig == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(audioConfig.stopMenuMusicEvent)
                && audioBackend.LoadBank(audioConfig.stopMenuMusicEvent, eventBank: true))
            {
                audioBackend.PostEvent(audioConfig.stopMenuMusicEvent, audioEmitter);
            }

            audioBackend.StopAllOnEmitter(audioEmitter);
            GameAudioEmitterFactory.Release(audioEmitter, 2f);
        }

        private static async UniTask WaitForLaunchSubmitReleaseAsync()
        {
            while (LaunchSubmitHeld())
            {
                await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            }

            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
        }

        private static bool LaunchSubmitHeld()
        {
            Keyboard keyboard = Keyboard.current;
            Gamepad gamepad = Gamepad.current;
            return (keyboard != null && (keyboard.enterKey.isPressed || keyboard.numpadEnterKey.isPressed || keyboard.spaceKey.isPressed))
                || (gamepad != null && gamepad.buttonSouth.isPressed);
        }

        private void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void TickNavigation()
        {
            if (root == null || isStartingRun)
            {
                return;
            }

            int vertical = ReadVerticalDirection();
            if (vertical != 0)
            {
                if (activeScreen == MenuScreen.Main && mainButtons.Count > 0)
                {
                    int nextIndex = MenuNavigationRules.NextClampedIndex(mainFocusIndex, mainButtons.Count, vertical);
                    SetMainFocus(nextIndex);
                }
                else if (activeScreen == MenuScreen.PerformerSelect && performerFrames.Count > 0)
                {
                    int nextIndex = MenuNavigationRules.NextClampedIndex(selectedPerformerIndex, performerFrames.Count, vertical);
                    SelectPerformer(nextIndex, startRun: false);
                }
            }

            if (SubmitPressedThisFrame())
            {
                ActivateFocusedCommand();
            }
            else if (CancelPressedThisFrame())
            {
                if (activeScreen != MenuScreen.Main)
                {
                    PlayUiClick();
                    ShowMain();
                }
            }
        }

        private void ActivateFocusedCommand()
        {
            switch (activeScreen)
            {
                case MenuScreen.Main:
                    PlayUiClick();
                    if (mainFocusIndex >= 0 && mainFocusIndex < mainButtons.Count)
                    {
                        marqueeEffects?.Pulse(mainButtons[mainFocusIndex]);
                    }

                    if (mainFocusIndex == 0)
                    {
                        ShowPerformerSelect();
                    }
                    else if (mainFocusIndex == 1)
                    {
                        ShowSettings();
                    }
                    else if (mainFocusIndex == 2)
                    {
                        QuitGame();
                    }

                    break;
                case MenuScreen.PerformerSelect:
                    PlayUiClick();
                    if (selectedPerformerIndex >= 0 && selectedPerformerIndex < performerRows.Count)
                    {
                        marqueeEffects?.Pulse(performerRows[selectedPerformerIndex]);
                    }
                    else
                    {
                        marqueeEffects?.Pulse(performerStartButton);
                    }

                    StartSelectedRun();
                    break;
                case MenuScreen.Settings:
                    PlayUiClick();
                    marqueeEffects?.Pulse(settingsBackButton);
                    ShowMain();
                    break;
            }
        }

        private int ReadVerticalDirection()
        {
            int keyboardDirection = 0;
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame)
                {
                    keyboardDirection = -1;
                }
                else if (keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame)
                {
                    keyboardDirection = 1;
                }
            }

            if (keyboardDirection != 0)
            {
                ResetNavigationRepeat();
                return keyboardDirection;
            }

            Gamepad gamepad = Gamepad.current;
            int gamepadDirection = ReadGamepadVerticalDirection(gamepad);
            if (MenuNavigationRules.ShouldTrigger(
                ref verticalNavigationRepeatState,
                gamepadDirection,
                Time.unscaledTime,
                navigationInitialRepeatDelay,
                navigationRepeatInterval))
            {
                return gamepadDirection;
            }

            return 0;
        }

        private int ReadGamepadVerticalDirection(Gamepad gamepad)
        {
            if (gamepad == null)
            {
                return 0;
            }

            if (gamepad.dpad.up.isPressed)
            {
                return -1;
            }

            if (gamepad.dpad.down.isPressed)
            {
                return 1;
            }

            return -MenuNavigationRules.DirectionFromAxis(gamepad.leftStick.y.ReadValue(), navigationStickThreshold);
        }

        private void ResetNavigationRepeat(bool requireNeutralRelease = false)
        {
            if (requireNeutralRelease)
            {
                MenuNavigationRules.RequireNeutralRelease(ref verticalNavigationRepeatState);
                return;
            }

            MenuNavigationRules.Reset(ref verticalNavigationRepeatState);
        }

        private static void ConfigureManualButton(Button button)
        {
            if (button == null)
            {
                return;
            }

            button.focusable = false;
            button.tabIndex = -1;
        }

        private static void SuppressBuiltInNavigationEvents(VisualElement element)
        {
            element.UnregisterCallback<NavigationMoveEvent>(SuppressBuiltInNavigationMove, TrickleDown.TrickleDown);
            element.UnregisterCallback<NavigationSubmitEvent>(SuppressBuiltInNavigationSubmit, TrickleDown.TrickleDown);
            element.UnregisterCallback<NavigationCancelEvent>(SuppressBuiltInNavigationCancel, TrickleDown.TrickleDown);

            element.RegisterCallback<NavigationMoveEvent>(SuppressBuiltInNavigationMove, TrickleDown.TrickleDown);
            element.RegisterCallback<NavigationSubmitEvent>(SuppressBuiltInNavigationSubmit, TrickleDown.TrickleDown);
            element.RegisterCallback<NavigationCancelEvent>(SuppressBuiltInNavigationCancel, TrickleDown.TrickleDown);
        }

        private static void SuppressBuiltInNavigationMove(NavigationMoveEvent evt)
        {
            evt.StopImmediatePropagation();
        }

        private static void SuppressBuiltInNavigationSubmit(NavigationSubmitEvent evt)
        {
            evt.StopImmediatePropagation();
        }

        private static void SuppressBuiltInNavigationCancel(NavigationCancelEvent evt)
        {
            evt.StopImmediatePropagation();
        }

        private static bool SubmitPressedThisFrame()
        {
            Keyboard keyboard = Keyboard.current;
            Gamepad gamepad = Gamepad.current;
            return (keyboard != null && (keyboard.enterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame))
                || (gamepad != null && gamepad.buttonSouth.wasPressedThisFrame);
        }

        private static bool CancelPressedThisFrame()
        {
            Keyboard keyboard = Keyboard.current;
            Gamepad gamepad = Gamepad.current;
            return (keyboard != null && (keyboard.escapeKey.wasPressedThisFrame || keyboard.backspaceKey.wasPressedThisFrame))
                || (gamepad != null && gamepad.buttonEast.wasPressedThisFrame);
        }

        private void MoveCameraTo(Transform target, bool instant)
        {
            if (menuCamera == null || target == null)
            {
                return;
            }

            if (instant)
            {
                menuCamera.transform.SetPositionAndRotation(target.position, target.rotation);
                menuCamera.fieldOfView = ResolveTargetFieldOfView(target);
                cameraMoveActive = false;
                return;
            }

            cameraStartPosition = menuCamera.transform.position;
            cameraStartRotation = menuCamera.transform.rotation;
            cameraStartFieldOfView = menuCamera.fieldOfView;
            cameraTargetFieldOfView = ResolveTargetFieldOfView(target);
            cameraTargetPose = target;
            cameraMoveElapsed = 0f;
            cameraMoveActive = true;
        }

        private void TickCameraMove()
        {
            if (!cameraMoveActive || menuCamera == null || cameraTargetPose == null)
            {
                return;
            }

            cameraMoveElapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(cameraMoveElapsed / cameraMoveSeconds);
            float eased = 1f - Mathf.Pow(1f - t, 4f);
            menuCamera.transform.position = Vector3.Lerp(cameraStartPosition, cameraTargetPose.position, eased);
            menuCamera.transform.rotation = Quaternion.Slerp(cameraStartRotation, cameraTargetPose.rotation, eased);
            menuCamera.fieldOfView = Mathf.Lerp(cameraStartFieldOfView, cameraTargetFieldOfView, eased);
            if (t >= 1f)
            {
                cameraMoveActive = false;
            }
        }

        private float ResolveTargetFieldOfView(Transform target)
        {
            MainMenuCameraPose pose = target != null ? target.GetComponent<MainMenuCameraPose>() : null;
            return pose != null ? pose.FieldOfView : menuCamera.fieldOfView;
        }

        private static void ShowScreen(VisualElement screen, bool visible)
        {
            if (screen == null)
            {
                return;
            }

            screen.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void PlayUiClick()
        {
            PlayUiAudio(GameAudioCue.UiClick);
            PlayUiHaptics(GameHapticsCue.UiClick);
        }

        private void PlayUiHover()
        {
            PlayUiAudio(GameAudioCue.UiHover);
            PlayUiHaptics(GameHapticsCue.UiHover);
        }

        private void PlayUiHaptics(GameHapticsCue cue)
        {
            if (!uiHapticsEnabled)
            {
                return;
            }

            EnsureMenuHaptics();
            menuHaptics?.Play(cue);
        }

        private void PlayUiAudio(GameAudioCue cue)
        {
            if (!uiAudioEnabled)
            {
                return;
            }

            EnsureMenuAudio();
            if (audioConfig == null || menuAudioBackend == null || menuAudioEmitter == null)
            {
                return;
            }

            string eventName = audioConfig.EventName(cue);
            if (string.IsNullOrWhiteSpace(eventName))
            {
                return;
            }

            if (TryLoadMenuAudioBank(eventName))
            {
                menuAudioBackend.PostEvent(eventName, menuAudioEmitter);
            }
        }

        private void StartMenuMusic()
        {
            if (menuMusicPlaying)
            {
                return;
            }

            EnsureMenuAudio();
            if (audioConfig == null
                || menuAudioBackend == null
                || menuAudioEmitter == null
                || !audioConfig.audioEnabled
                || !audioConfig.menuMusicEnabled
                || string.IsNullOrWhiteSpace(audioConfig.menuMusicEvent)
                || !TryLoadMenuAudioBank(audioConfig.menuMusicEvent))
            {
                return;
            }

            menuMusicPlaying = true;
            menuAudioBackend.SetOutputBusVolume(menuAudioEmitter, audioConfig.musicNormalVolume);
            menuAudioBackend.PostEvent(audioConfig.menuMusicEvent, menuAudioEmitter);
        }

        private void StopMenuMusic()
        {
            if (!menuMusicPlaying)
            {
                return;
            }

            menuMusicPlaying = false;
            if (audioConfig == null || menuAudioBackend == null || menuAudioEmitter == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(audioConfig.stopMenuMusicEvent)
                && TryLoadMenuAudioBank(audioConfig.stopMenuMusicEvent))
            {
                menuAudioBackend.PostEvent(audioConfig.stopMenuMusicEvent, menuAudioEmitter);
            }

            menuAudioBackend.StopAllOnEmitter(menuAudioEmitter);
        }

        private bool TryLoadMenuAudioBank(string eventName)
        {
            if (string.IsNullOrWhiteSpace(eventName))
            {
                return true;
            }

            if (loadedMenuAudioBanks.Contains(eventName))
            {
                return true;
            }

            if (menuAudioBackend == null || !menuAudioBackend.LoadBank(eventName, eventBank: true))
            {
                return false;
            }

            loadedMenuAudioBanks.Add(eventName);
            return true;
        }

        private void EnsureMenuAudio()
        {
            if (audioConfig == null)
            {
                audioConfig = GameAudioConfig.CreateRuntimeDefault();
            }

            audioConfig.EnsureWorkflowDefaults();
            menuAudioBackend ??= new WwiseRuntimeAudioBackend();
            if (menuAudioEmitter == null)
            {
                menuAudioEmitter = new GameObject("Main Menu UI Audio");
                menuAudioEmitter.transform.SetParent(transform, worldPositionStays: false);
            }
        }

        private void PreserveMenuAudioEmitter()
        {
            if (menuAudioEmitter == null)
            {
                return;
            }

            menuAudioEmitter.transform.SetParent(null, worldPositionStays: true);
            DontDestroyOnLoad(menuAudioEmitter);
        }

        private void EnsureMenuHaptics()
        {
            if (!uiHapticsEnabled)
            {
                return;
            }

            if (hapticsConfig == null)
            {
                hapticsConfig = GameHapticsConfig.CreateRuntimeDefault();
            }

            hapticsConfig.EnsureWorkflowDefaults();
            if (menuHaptics == null)
            {
                menuHaptics = new GameHapticsService(hapticsConfig, new UnityGamepadHapticsBackend());
                menuHaptics.Start();
            }
        }

        private void DisposeMenuHaptics()
        {
            menuHaptics?.Dispose();
            menuHaptics = null;
        }

        private static void SetText(Label label, string text)
        {
            if (label != null)
            {
                label.text = text ?? string.Empty;
            }
        }
    }
}
