using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using TheCircussyOne.Runtime;
using TheCircussyOne.Stats;
using UnityEngine;

namespace TheCircussyOne.Visuals
{
    public sealed class ChestView : MonoBehaviour, IInteractable, ICurrencyAwareInteractable, IWorldInteractionPromptTarget, IWorldRewardInteractionBehavior, IWorldRewardAnimationTarget
    {
        private const string TicketShortageColorTag = "#ff4758";

        [SerializeField] private ChestDefinition definition;
        [SerializeField] private Transform lidRoot;

        private WorldRewardInteractableDriver interactionDriver;
        private ChestRuntime runtime;
        private ItemCatalog itemCatalog;
        private ItemInventory inventory;
        private RunStats stats;
        private RunPauseState pauseState;
        private RunSeedState runSeedState;
        private IGameAudio gameAudio;
        private IGameHaptics gameHaptics;
        private bool canAffordLastPrompt = true;
        private Vector3 baseLidLocalPosition;
        private Quaternion baseLidLocalRotation = Quaternion.identity;
        private bool baseLidCaptured;
        private bool openDespawnStarted;
        private bool openDespawnCompleted;
        private float openDespawnElapsedSeconds;

        private WorldRewardInteractableDriver InteractionDriver => interactionDriver ??= new WorldRewardInteractableDriver(this);
        public ChestDefinition Definition => definition;
        public bool IsInteractionAvailable => InteractionDriver.IsInteractionAvailable;
        public bool IsOpen => runtime?.Completed == true;
        public bool IsTargeted => InteractionDriver.IsTargeted;
        public bool IsInteracting => InteractionDriver.IsInteracting;
        public bool IsOpenDespawnStarted => openDespawnStarted;
        public float OpenDespawnElapsedSeconds => openDespawnElapsedSeconds;
        public Transform LidRoot => lidRoot;
        public bool ShouldTickWorldRewardAnimation => openDespawnStarted && !openDespawnCompleted && gameObject.activeSelf;
        public InteractPriority Priority => InteractPriority.ChestOrShrine;
        public Vector3 InteractionPosition => transform.position;
        public Vector3 WorldInteractionPromptPosition
        {
            get
            {
                ResolveReferences();
                return InteractionDriver.ResolvePromptPosition(transform, FallbackPromptHeight());
            }
        }

        public string PromptText => definition != null && !string.IsNullOrWhiteSpace(definition.promptText) ? definition.promptText : "Open Chest";
        public float HoldSeconds => definition != null ? definition.holdSeconds : 0f;
        public float Progress => runtime?.Progress ?? 0f;
        public bool IsRewardInteractionAvailable => definition != null && definition.isActive && runtime?.Completed != true && gameObject.activeInHierarchy;
        public SceneInteractableKind RegistryKind => SceneInteractableKind.Chest;
        public WorldInteractableVisualStyle VisualStyle => CreateVisualStyle();

        private void Awake()
        {
            ResolveReferences();
            ApplyVisuals();
        }

        private void OnEnable()
        {
            ResolveReferences();
            EnsureRuntime();
            InteractionDriver.Register(this, this);
            SceneWorldRewardAnimationRegistry.Register(this);
            ApplyVisuals();
        }

        private void OnDisable()
        {
            InteractionDriver.Deactivate(this);
            SceneWorldRewardAnimationRegistry.Unregister(this);
        }

        private void OnValidate()
        {
            ResolveReferences();
            ApplyVisuals();
        }

        public void Prepare(
            ChestDefinition chestDefinition,
            ItemCatalog activeItemCatalog,
            ItemInventory activeInventory,
            RunStats activeStats,
            RunPauseState activePauseState = null,
            RunSeedState activeRunSeedState = null,
            IGameAudio activeAudio = null,
            IGameHaptics activeHaptics = null)
        {
            definition = chestDefinition;
            itemCatalog = activeItemCatalog;
            inventory = activeInventory;
            stats = activeStats;
            pauseState = activePauseState;
            runSeedState = activeRunSeedState;
            gameAudio = activeAudio ?? NullGameAudio.Instance;
            gameHaptics = activeHaptics ?? NullGameHaptics.Instance;
            runtime = new ChestRuntime(definition, itemCatalog, inventory, stats, transform.position, runSeedState);
            InteractionDriver.ResetState();
            openDespawnStarted = false;
            openDespawnCompleted = false;
            openDespawnElapsedSeconds = 0f;
            ResolveReferences();
            InteractionDriver.Register(this, this);
            SceneWorldRewardAnimationRegistry.Register(this);
            ApplyVisuals();
        }

        public string PromptTextFor(RunCurrencyState currency)
        {
            if (definition == null)
            {
                return "Open Chest";
            }

            int cost = definition.TicketCost;
            if (cost <= 0)
            {
                SetAffordability(true);
                return PromptText;
            }

            int tickets = currency?.Tickets ?? 0;
            if (tickets < cost)
            {
                SetAffordability(false);
                return $"Need <color={TicketShortageColorTag}>{cost - tickets}</color> more Tickets";
            }

            SetAffordability(true);
            return $"Spend {cost} Tickets to {PromptText}";
        }

        public bool CanBeginInteraction(RunCurrencyState currency)
        {
            EnsureRuntime();
            bool canBegin = IsInteractionAvailable && (runtime?.CanPay(currency) ?? true);
            SetAffordability(canBegin || definition == null || definition.TicketCost <= 0);
            return canBegin;
        }

        public void SetInteractionTargeted(bool isTargeted)
        {
            InteractionDriver.SetInteractionTargeted(this, transform, isTargeted);
        }

        public void BeginInteraction()
        {
            EnsureRuntime();
            InteractionDriver.BeginInteraction(transform);
        }

        public void TickInteraction(float deltaTime, RunCurrencyState currency)
        {
            EnsureRuntime();
            WorldRewardInteractionTickResult result = InteractionDriver.TickInteraction(this, transform, deltaTime, currency);
            if (result.Completed)
            {
                gameAudio?.PlayAt(GameAudioCue.ChestOpen, transform.position);
                gameHaptics?.Play(GameHapticsCue.ChestOpen);
                StartOpenDespawn();
                ApplyVisuals();
            }
        }

        public void CancelInteraction()
        {
            InteractionDriver.CancelInteraction(transform);
        }

        public void BeginRewardInteraction()
        {
            EnsureRuntime();
        }

        public WorldRewardInteractionTickResult TickRewardInteraction(float deltaTime, RunCurrencyState currency)
        {
            return runtime != null && runtime.Tick(deltaTime, currency)
                ? WorldRewardInteractionTickResult.CompletedResult()
                : WorldRewardInteractionTickResult.InProgress();
        }

        public void CancelRewardInteraction()
        {
            runtime?.Cancel();
        }

        public void TickWorldRewardAnimation(float deltaTime)
        {
            TickOpenDespawn(deltaTime);
        }

        public void TickOpenDespawn(float deltaTime)
        {
            if (!openDespawnStarted || openDespawnCompleted || definition == null || !gameObject.activeSelf)
            {
                return;
            }

            // Chest rewards can open immediately before a blocking UI appears; delaying
            // the despawn keeps the visual result visible until gameplay resumes.
            if (pauseState != null && pauseState.IsPaused)
            {
                return;
            }

            openDespawnElapsedSeconds += Mathf.Max(0f, deltaTime);
            float totalSeconds = Mathf.Max(0.01f, definition.openDisappearSeconds);
            if (openDespawnElapsedSeconds >= totalSeconds)
            {
                openDespawnElapsedSeconds = totalSeconds;
                ApplyVisuals();
                openDespawnCompleted = true;
                gameObject.SetActive(false);
                return;
            }

            ApplyVisuals();
        }

        private void EnsureRuntime()
        {
            runtime ??= new ChestRuntime(definition, itemCatalog, inventory, stats, transform.position, runSeedState);
        }

        private void ResolveReferences()
        {
            InteractionDriver.Resolve(transform);

            if (lidRoot == null)
            {
                Transform child = transform.Find("Lid") ?? transform.Find($"{VisualGroundAnchorUtility.VisualRootName}/Lid");
                if (child != null)
                {
                    lidRoot = child;
                }
            }

            if (lidRoot != null && !baseLidCaptured)
            {
                baseLidLocalPosition = lidRoot.localPosition;
                baseLidLocalRotation = lidRoot.localRotation;
                baseLidCaptured = true;
            }
        }

        private void ApplyVisuals()
        {
            if (definition == null)
            {
                return;
            }

            ResolveReferences();
            InteractionDriver.ApplyVisuals(transform, OpenDespawnScale());
            ApplyLidVisuals();
        }

        private void ApplyLidVisuals()
        {
            if (lidRoot == null || !baseLidCaptured)
            {
                return;
            }

            if (IsOpen)
            {
                lidRoot.localPosition = baseLidLocalPosition + new Vector3(0f, 0.18f, -0.12f);
                lidRoot.localRotation = baseLidLocalRotation * Quaternion.Euler(-68f, 0f, 0f);
                return;
            }

            lidRoot.localPosition = baseLidLocalPosition;
            lidRoot.localRotation = baseLidLocalRotation;
        }

        private void StartOpenDespawn()
        {
            openDespawnStarted = true;
            openDespawnCompleted = false;
            openDespawnElapsedSeconds = 0f;
        }

        private float OpenDespawnScale()
        {
            if (!openDespawnStarted || definition == null)
            {
                return 1f;
            }

            float totalSeconds = Mathf.Max(0.01f, definition.openDisappearSeconds);
            float finalScale = Mathf.Clamp01(definition.openShrinkFinalScaleMultiplier);
            EaseSettings ease = definition.openDisappearEase.shape > 0f
                ? definition.openDisappearEase
                : EaseSettings.InBack;
            float t = Mathf.Clamp01(openDespawnElapsedSeconds / totalSeconds);
            float eased = GameEasing.Evaluate01(ease, t);
            return Mathf.LerpUnclamped(1f, finalScale, eased);
        }

        private void SetAffordability(bool canAfford)
        {
            if (canAffordLastPrompt == canAfford)
            {
                return;
            }

            canAffordLastPrompt = canAfford;
            InteractionDriver.RefreshOutline(this);
        }

        private WorldInteractableVisualStyle CreateVisualStyle()
        {
            bool open = IsOpen;
            Color visualColor = open
                ? Color.Lerp(definition.visualColor, new Color(0.04f, 0.035f, 0.025f, definition.visualColor.a), 0.48f)
                : definition.visualColor;
            float emissionStrength = Mathf.Max(0f, definition.emissionStrength) * (open ? 0.28f : 1f);
            return new WorldInteractableVisualStyle(
                definition.visualScale,
                visualColor,
                definition.emissionColor,
                emissionStrength,
                canAffordLastPrompt ? definition.targetOutlineColor : definition.insufficientTicketsOutlineColor,
                definition.targetOutlineThickness,
                open ? 0f : definition.interactionSquashStretchAmplitude,
                open ? 0f : definition.interactionSquashStretchFrequency,
                FallbackPromptHeight(),
                targetedScaleMultiplier: open ? 1f : 1.08f,
                targetedColorLerp: open ? 0f : 0.28f,
                targetedEmissionMultiplier: open ? 1f : 1.25f);
        }

        private float FallbackPromptHeight()
        {
            return Mathf.Max(0.75f, definition != null ? definition.visualScale : 1f);
        }
    }
}
