using System;
using Cysharp.Threading.Tasks;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;
using TheCircussyOne.Runtime;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheCircussyOne.Visuals
{
    public sealed class RunTransitionOverlayHost : MonoBehaviour, IRunTransitionOverlay
    {
        private const int OverlaySortingOrder = 30000;
        private const float StallLogThresholdSeconds = 0.2f;
        private static RunTransitionOverlayHost instance;

        private UIDocument document;
        private PanelSettings panelSettings;
        private VisualElement root;
        private VisualElement bulbRing;
        private VisualElement[] bulbs = Array.Empty<VisualElement>();
        private Label titleLabel;
        private Label subtitleLabel;
        private Label loadingStageLabel;
        private HudTransitionSettings activeSettings;
        private TransitionStallMonitor activeStallMonitor;
        private string activePhase = "idle";

        public bool IsActive { get; private set; }

        public static RunTransitionOverlayHost GetOrCreate(PanelSettings sourcePanelSettings = null)
        {
            if (instance != null)
            {
                instance.EnsureDocument(sourcePanelSettings);
                return instance;
            }

            var hostObject = new GameObject("Run Transition Overlay");
            instance = hostObject.AddComponent<RunTransitionOverlayHost>();
            if (Application.isPlaying)
            {
                DontDestroyOnLoad(hostObject);
            }

            instance.EnsureDocument(sourcePanelSettings);
            return instance;
        }

        public static void DestroyForTests()
        {
            if (instance == null)
            {
                return;
            }

            RunTransitionOverlayHost host = instance;
            instance = null;
            if (Application.isPlaying)
            {
                Destroy(host.gameObject);
            }
            else
            {
                DestroyImmediate(host.gameObject);
            }
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            if (Application.isPlaying)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        private void Update()
        {
            if (!IsActive)
            {
                return;
            }

            activeStallMonitor?.Tick(activePhase);
            UpdateBulbs(activeSettings);
        }

        public bool Play(
            HudVisualConfig config,
            string title,
            string subtitle,
            Action work,
            Action onComplete = null,
            Action onReadyToReveal = null,
            float? minimumVisibleSecondsOverride = null)
        {
            return Play(config, title, subtitle, () =>
            {
                work?.Invoke();
                return UniTask.CompletedTask;
            }, onComplete, onReadyToReveal, minimumVisibleSecondsOverride);
        }

        public bool Play(
            HudVisualConfig config,
            string title,
            string subtitle,
            Func<UniTask> work,
            Action onComplete = null,
            Action onReadyToReveal = null,
            float? minimumVisibleSecondsOverride = null)
        {
            if (IsActive)
            {
                return false;
            }

            EnsureDocument();
            HudTransitionSettings settings = config != null ? config.runTransition : HudTransitionSettings.Default;
            settings.EnsureDefaults();
            if (!settings.enabled)
            {
                RunWithoutOverlayAsync(work, onComplete).Forget();
                return true;
            }

            if (minimumVisibleSecondsOverride.HasValue)
            {
                settings.minimumVisibleSeconds = Mathf.Max(0f, minimumVisibleSecondsOverride.Value);
            }

            ApplyPanelSettings(config);
            ApplyStyles(config, settings);
            titleLabel.text = string.IsNullOrWhiteSpace(title) ? "INTERMISSION" : title;
            subtitleLabel.text = string.IsNullOrWhiteSpace(subtitle) ? string.Empty : subtitle;
            SetLoadingStage(null);
            root.style.display = DisplayStyle.Flex;
            root.style.opacity = 0f;
            IsActive = true;
            activeSettings = settings;
            RunTransitionAsync(settings, titleLabel.text, work, onComplete, onReadyToReveal).Forget();
            return true;
        }

        public bool Play(
            HudVisualConfig config,
            string title,
            string subtitle,
            Func<AsyncOperation> work,
            Action onComplete = null,
            Action onReadyToReveal = null,
            float? minimumVisibleSecondsOverride = null)
        {
            if (IsActive)
            {
                return false;
            }

            EnsureDocument();
            HudTransitionSettings settings = config != null ? config.runTransition : HudTransitionSettings.Default;
            settings.EnsureDefaults();
            if (!settings.enabled)
            {
                RunWithoutOverlayAsync(() => WaitForAsyncOperation(work), onComplete).Forget();
                return true;
            }

            if (minimumVisibleSecondsOverride.HasValue)
            {
                settings.minimumVisibleSeconds = Mathf.Max(0f, minimumVisibleSecondsOverride.Value);
            }

            ApplyPanelSettings(config);
            ApplyStyles(config, settings);
            titleLabel.text = string.IsNullOrWhiteSpace(title) ? "INTERMISSION" : title;
            subtitleLabel.text = string.IsNullOrWhiteSpace(subtitle) ? string.Empty : subtitle;
            SetLoadingStage(null);
            root.style.display = DisplayStyle.Flex;
            root.style.opacity = 0f;
            IsActive = true;
            activeSettings = settings;
            RunTransitionAsync(settings, titleLabel.text, () => WaitForAsyncOperation(work), onComplete, onReadyToReveal).Forget();
            return true;
        }

        public void SetLoadingStage(string stage)
        {
            if (loadingStageLabel == null)
            {
                return;
            }

            string text = string.IsNullOrWhiteSpace(stage) ? string.Empty : stage.Trim();
            loadingStageLabel.text = text;
            loadingStageLabel.style.display = activeSettings.loadingStageTextEnabled && !string.IsNullOrEmpty(text)
                ? DisplayStyle.Flex
                : DisplayStyle.None;
        }

        private async UniTaskVoid RunTransitionAsync(
            HudTransitionSettings settings,
            string title,
            Func<UniTask> work,
            Action onComplete,
            Action onReadyToReveal)
        {
            activeStallMonitor = new TransitionStallMonitor(title);
            activePhase = RunTransitionPhase.FadingOut.ToString();
            await AnimatePhaseAsync(RunTransitionPhase.FadingOut, settings);

            Exception workException = null;
            bool workDone = false;
            activePhase = "work";
            RunWorkAsync().Forget();
            while (!workDone)
            {
                await YieldOverlayFrame();
            }

            onReadyToReveal?.Invoke();
            SetLoadingStage("Opening the Curtain");
            activePhase = "hold";
            float visibleStartedAt = Time.unscaledTime;
            while (!RunTransitionRules.MinimumVisibleSatisfied(Time.unscaledTime - visibleStartedAt, settings))
            {
                root.style.opacity = 1f;
                await YieldOverlayFrame();
            }

            activePhase = RunTransitionPhase.FadingIn.ToString();
            await AnimatePhaseAsync(RunTransitionPhase.FadingIn, settings);

            root.style.opacity = 0f;
            root.style.display = DisplayStyle.None;
            IsActive = false;
            activePhase = "idle";
            SetLoadingStage(null);
            activeStallMonitor = null;
            onComplete?.Invoke();

            if (workException != null)
            {
                Debug.LogError($"Run transition work failed: {workException.Message}");
            }

            async UniTaskVoid RunWorkAsync()
            {
                try
                {
                    if (work != null)
                    {
                        await work.Invoke();
                    }
                }
                catch (Exception exception)
                {
                    workException = exception;
                    Debug.LogException(exception);
                }
                finally
                {
                    workDone = true;
                }
            }
        }

        private async UniTask AnimatePhaseAsync(
            RunTransitionPhase phase,
            HudTransitionSettings settings)
        {
            float startedAt = Time.unscaledTime;
            float duration = phase == RunTransitionPhase.FadingIn ? settings.fadeInSeconds : settings.fadeOutSeconds;
            while (Time.unscaledTime - startedAt < duration)
            {
                float elapsed = Time.unscaledTime - startedAt;
                root.style.opacity = RunTransitionRules.OverlayOpacity(phase, elapsed, settings);
                await YieldOverlayFrame();
            }

            root.style.opacity = RunTransitionRules.OverlayOpacity(phase, duration, settings);
        }

        private static async UniTask WaitForAsyncOperation(Func<AsyncOperation> work)
        {
            AsyncOperation operation = work?.Invoke();
            while (operation != null && !operation.isDone)
            {
                await YieldOverlayFrame();
            }
        }

        private static async UniTaskVoid RunWithoutOverlayAsync(Func<UniTask> work, Action onComplete)
        {
            if (work != null)
            {
                await work.Invoke();
            }

            onComplete?.Invoke();
        }

        private static YieldAwaitable YieldOverlayFrame()
        {
            return UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
        }

        private void EnsureDocument(PanelSettings sourcePanelSettings = null)
        {
            document = document != null ? document : GetComponent<UIDocument>();
            if (document == null)
            {
                document = gameObject.AddComponent<UIDocument>();
            }

            if (panelSettings == null)
            {
                panelSettings = sourcePanelSettings != null
                    ? Instantiate(sourcePanelSettings)
                    : ScriptableObject.CreateInstance<PanelSettings>();
                panelSettings.name = "Run Transition Overlay PanelSettings";
            }

            document.panelSettings = panelSettings;
            document.sortingOrder = OverlaySortingOrder;
            panelSettings.sortingOrder = OverlaySortingOrder;

            root = document.rootVisualElement;
            if (root == null)
            {
                return;
            }

            if (root.childCount > 0 && titleLabel != null)
            {
                return;
            }

            root.Clear();
            BuildTree();
        }

        private void BuildTree()
        {
            root.name = "run-transition-overlay";
            root.pickingMode = PickingMode.Position;
            root.style.position = Position.Absolute;
            root.style.left = 0f;
            root.style.right = 0f;
            root.style.top = 0f;
            root.style.bottom = 0f;
            root.style.alignItems = Align.Center;
            root.style.justifyContent = Justify.Center;
            root.style.flexDirection = FlexDirection.Column;
            root.style.display = DisplayStyle.None;

            bulbRing = new VisualElement { name = "run-transition-bulbs" };
            bulbRing.pickingMode = PickingMode.Ignore;
            bulbRing.style.position = Position.Relative;
            bulbRing.style.marginBottom = 18f;
            root.Add(bulbRing);

            titleLabel = new Label { name = "run-transition-title" };
            titleLabel.pickingMode = PickingMode.Ignore;
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            root.Add(titleLabel);

            subtitleLabel = new Label { name = "run-transition-subtitle" };
            subtitleLabel.pickingMode = PickingMode.Ignore;
            subtitleLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            subtitleLabel.style.marginTop = 8f;
            root.Add(subtitleLabel);

            loadingStageLabel = new Label { name = "run-transition-loading-stage" };
            loadingStageLabel.pickingMode = PickingMode.Ignore;
            loadingStageLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            loadingStageLabel.style.marginTop = 12f;
            loadingStageLabel.style.display = DisplayStyle.None;
            root.Add(loadingStageLabel);
        }

        private void ApplyPanelSettings(HudVisualConfig config)
        {
            if (panelSettings == null)
            {
                return;
            }

            if (config == null)
            {
                panelSettings.scaleMode = PanelScaleMode.ConstantPixelSize;
                return;
            }

            panelSettings.scaleMode = config.scaleMode;
            panelSettings.referenceDpi = config.referenceDpi;
            panelSettings.fallbackDpi = config.fallbackDpi;
            panelSettings.referenceResolution = new Vector2Int(
                Mathf.RoundToInt(config.referenceResolution.x),
                Mathf.RoundToInt(config.referenceResolution.y));
        }

        private void ApplyStyles(HudVisualConfig config, HudTransitionSettings settings)
        {
            float scale = config != null ? Mathf.Max(0.01f, config.globalScale) : 1f;
            root.style.backgroundColor = RunTransitionRules.BackdropColor(settings);
            root.style.color = settings.titleColor;
            if (config != null && config.uiFont != null)
            {
                root.style.unityFontDefinition = FontDefinition.FromFont(config.uiFont);
            }
            else
            {
                root.style.unityFontDefinition = StyleKeyword.Null;
            }

            titleLabel.style.fontSize = settings.titleFontSize * scale;
            titleLabel.style.color = settings.titleColor;
            titleLabel.style.unityTextOutlineColor = settings.outlineColor;
            titleLabel.style.unityTextOutlineWidth = settings.titleOutlineWidth * scale;

            subtitleLabel.style.fontSize = settings.subtitleFontSize * scale;
            subtitleLabel.style.color = settings.subtitleColor;
            subtitleLabel.style.unityTextOutlineColor = settings.outlineColor;
            subtitleLabel.style.unityTextOutlineWidth = settings.subtitleOutlineWidth * scale;

            loadingStageLabel.style.fontSize = settings.loadingStageFontSize * scale;
            loadingStageLabel.style.color = settings.loadingStageColor;
            loadingStageLabel.style.unityTextOutlineColor = settings.outlineColor;
            loadingStageLabel.style.unityTextOutlineWidth = settings.loadingStageOutlineWidth * scale;
            loadingStageLabel.style.display = settings.loadingStageTextEnabled && !string.IsNullOrWhiteSpace(loadingStageLabel.text)
                ? DisplayStyle.Flex
                : DisplayStyle.None;

            BuildBulbs(settings, scale);
            UpdateBulbs(settings);
        }

        private void BuildBulbs(HudTransitionSettings settings, float scale)
        {
            int count = Mathf.Max(3, settings.bulbCount);
            if (bulbs.Length != count)
            {
                bulbRing.Clear();
                bulbs = new VisualElement[count];
                for (int i = 0; i < count; i++)
                {
                    var bulb = new VisualElement { name = $"run-transition-bulb-{i}" };
                    bulb.pickingMode = PickingMode.Ignore;
                    bulb.style.position = Position.Absolute;
                    bulbRing.Add(bulb);
                    bulbs[i] = bulb;
                }
            }

            float radius = settings.bulbRingRadius * scale;
            float size = settings.bulbSize * scale;
            float bounds = (radius + size) * 2f;
            bulbRing.style.width = bounds;
            bulbRing.style.height = bounds;
            float center = bounds * 0.5f;
            for (int i = 0; i < bulbs.Length; i++)
            {
                float angle = Mathf.PI * 2f * i / bulbs.Length - Mathf.PI * 0.5f;
                VisualElement bulb = bulbs[i];
                bulb.style.left = center + Mathf.Cos(angle) * radius - size * 0.5f;
                bulb.style.top = center + Mathf.Sin(angle) * radius - size * 0.5f;
                bulb.style.width = size;
                bulb.style.height = size;
                bulb.style.borderTopLeftRadius = size * 0.5f;
                bulb.style.borderTopRightRadius = size * 0.5f;
                bulb.style.borderBottomLeftRadius = size * 0.5f;
                bulb.style.borderBottomRightRadius = size * 0.5f;
            }
        }

        private void UpdateBulbs(HudTransitionSettings settings)
        {
            if (bulbs == null)
            {
                return;
            }

            float elapsed = Time.unscaledTime;
            for (int i = 0; i < bulbs.Length; i++)
            {
                float brightness = RunTransitionRules.BulbBrightness(i, elapsed, settings);
                Color color = Color.Lerp(settings.bulbDimColor, settings.bulbBrightColor, brightness);
                bulbs[i].style.backgroundColor = color;
            }
        }

        private sealed class TransitionStallMonitor
        {
            private readonly string title;
            private float lastTickTime;
            private int lastFrame;

            public TransitionStallMonitor(string title)
            {
                this.title = string.IsNullOrWhiteSpace(title) ? "Transition" : title;
                lastTickTime = Time.unscaledTime;
                lastFrame = Time.frameCount;
            }

            public void Tick(string phase)
            {
                float now = Time.unscaledTime;
                float gap = now - lastTickTime;
                int frameGap = Time.frameCount - lastFrame;
                if (gap >= StallLogThresholdSeconds)
                {
                    Debug.LogFormat(
                        LogType.Warning,
                        LogOption.NoStacktrace,
                        null,
                        "{0}",
                        $"[RUN-TRANSITION-STALL] title=\"{title}\" phase={phase} " +
                        $"gapMs={gap * 1000f:0.###} frameGap={frameGap}");
                }

                lastTickTime = now;
                lastFrame = Time.frameCount;
            }
        }
    }
}
