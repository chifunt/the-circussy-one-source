using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using TheCircussyOne.Runtime;
using UnityEngine;

namespace TheCircussyOne.Visuals
{
    public sealed class TicketDepositView : MonoBehaviour, IInteractable, IWorldInteractionPromptTarget, IWorldRewardInteractionBehavior, IWorldRewardAnimationTarget
    {
        [SerializeField] private TicketDepositDefinition definition;

        private WorldRewardInteractableDriver interactionDriver;
        private TicketDepositRuntime runtime;
        private RunSeedState runSeedState;
        private RunPauseState pauseState;
        private IWorldRewardTransientVfxSpawner transientVfx;
        private IGameAudio gameAudio;
        private IGameHaptics gameHaptics;
        private bool collectDespawnStarted;
        private bool collectDespawnCompleted;
        private float collectDespawnElapsedSeconds;
        private ParticleSystem lastTicketBurst;

        private WorldRewardInteractableDriver InteractionDriver => interactionDriver ??= new WorldRewardInteractableDriver(this);
        public TicketDepositDefinition Definition => definition;
        public bool IsInteractionAvailable => InteractionDriver.IsInteractionAvailable;
        public bool IsCollected => runtime?.Completed == true;
        public bool IsCollectDespawnStarted => collectDespawnStarted;
        public float CollectDespawnElapsedSeconds => collectDespawnElapsedSeconds;
        public ParticleSystem LastTicketBurst => lastTicketBurst;
        public bool ShouldTickWorldRewardAnimation => collectDespawnStarted && !collectDespawnCompleted && gameObject.activeSelf;
        public InteractPriority Priority => InteractPriority.TicketDeposit;
        public Vector3 InteractionPosition => transform.position;
        public Vector3 WorldInteractionPromptPosition
        {
            get
            {
                return InteractionDriver.ResolvePromptPosition(transform, FallbackPromptHeight());
            }
        }

        public string PromptText => definition != null && !string.IsNullOrWhiteSpace(definition.promptText) ? definition.promptText : "Collect Tickets";
        public float HoldSeconds => definition != null ? definition.holdSeconds : 0f;
        public float Progress => runtime?.Progress ?? 0f;
        public bool IsRewardInteractionAvailable => definition != null && definition.isActive && runtime?.Completed != true && gameObject.activeInHierarchy;
        public SceneInteractableKind RegistryKind => SceneInteractableKind.TicketDeposit;
        public WorldInteractableVisualStyle VisualStyle => CreateVisualStyle();

        private void Awake()
        {
            ResolveReferences();
            ApplyVisuals();
        }

        private void OnEnable()
        {
            ResolveReferences();
            runtime ??= new TicketDepositRuntime(definition, transform.position, runSeedState);
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
            TicketDepositDefinition depositDefinition,
            RunSeedState activeRunSeedState = null,
            RunPauseState activePauseState = null,
            IWorldRewardTransientVfxSpawner activeTransientVfx = null,
            IGameAudio activeAudio = null,
            IGameHaptics activeHaptics = null)
        {
            definition = depositDefinition;
            runSeedState = activeRunSeedState;
            pauseState = activePauseState;
            transientVfx = activeTransientVfx;
            gameAudio = activeAudio ?? NullGameAudio.Instance;
            gameHaptics = activeHaptics ?? NullGameHaptics.Instance;
            runtime = new TicketDepositRuntime(definition, transform.position, runSeedState);
            InteractionDriver.ResetState();
            collectDespawnStarted = false;
            collectDespawnCompleted = false;
            collectDespawnElapsedSeconds = 0f;
            lastTicketBurst = null;
            ResolveReferences();
            InteractionDriver.Register(this, this);
            SceneWorldRewardAnimationRegistry.Register(this);
            ApplyVisuals();
        }

        public void SetInteractionTargeted(bool isTargeted)
        {
            InteractionDriver.SetInteractionTargeted(this, transform, isTargeted);
        }

        public void BeginInteraction()
        {
            runtime ??= new TicketDepositRuntime(definition, transform.position, runSeedState);
            InteractionDriver.BeginInteraction(transform);
        }

        public void TickInteraction(float deltaTime, RunCurrencyState currency)
        {
            runtime ??= new TicketDepositRuntime(definition, transform.position, runSeedState);
            WorldRewardInteractionTickResult result = InteractionDriver.TickInteraction(this, transform, deltaTime, currency);
            if (result.Completed)
            {
                StartCollectDespawn();
                ApplyVisuals();
            }
        }

        public void CancelInteraction()
        {
            InteractionDriver.CancelInteraction(transform);
        }

        public void BeginRewardInteraction()
        {
        }

        public WorldRewardInteractionTickResult TickRewardInteraction(float deltaTime, RunCurrencyState currency)
        {
            int grantedTickets = runtime.Tick(deltaTime, currency);
            if (grantedTickets > 0)
            {
                gameAudio?.PlayAt(GameAudioCue.TicketCollect, transform.position);
                gameHaptics?.Play(GameHapticsCue.TicketCollect);
                PlayTicketPaperBurst(grantedTickets);
            }

            return runtime.Completed
                ? WorldRewardInteractionTickResult.CompletedResult()
                : WorldRewardInteractionTickResult.InProgress();
        }

        public void CancelRewardInteraction()
        {
            runtime?.Cancel();
        }

        public void TickWorldRewardAnimation(float deltaTime)
        {
            TickCollectDespawn(deltaTime);
        }

        private void ResolveReferences()
        {
            InteractionDriver.Resolve(transform);
        }

        private void ApplyVisuals()
        {
            if (definition == null)
            {
                return;
            }

            ResolveReferences();
            InteractionDriver.ApplyVisuals(transform, CollectDespawnScale());
        }

        public void TickCollectDespawn(float deltaTime)
        {
            if (!collectDespawnStarted || collectDespawnCompleted || definition == null || !gameObject.activeSelf)
            {
                return;
            }

            if (pauseState != null && pauseState.IsPaused)
            {
                return;
            }

            collectDespawnElapsedSeconds += Mathf.Max(0f, deltaTime);
            float totalSeconds = Mathf.Max(0.01f, definition.collectDisappearSeconds);
            if (collectDespawnElapsedSeconds >= totalSeconds)
            {
                collectDespawnElapsedSeconds = totalSeconds;
                ApplyVisuals();
                collectDespawnCompleted = true;
                gameObject.SetActive(false);
                return;
            }

            ApplyVisuals();
        }

        private WorldInteractableVisualStyle CreateVisualStyle()
        {
            bool collected = IsCollected;
            return new WorldInteractableVisualStyle(
                definition.visualScale,
                definition.visualColor,
                definition.emissionColor,
                Mathf.Max(0f, definition.emissionStrength) * (collected ? 0.18f : 1f),
                definition.targetOutlineColor,
                definition.targetOutlineThickness,
                collected ? 0f : definition.interactionSquashStretchAmplitude,
                collected ? 0f : definition.interactionSquashStretchFrequency,
                FallbackPromptHeight(),
                targetedScaleMultiplier: collected ? 1f : 1.12f);
        }

        private float FallbackPromptHeight()
        {
            return Mathf.Max(0.35f, definition != null ? definition.visualScale : 1f);
        }

        private void StartCollectDespawn()
        {
            if (collectDespawnStarted)
            {
                return;
            }

            collectDespawnStarted = true;
            collectDespawnCompleted = false;
            collectDespawnElapsedSeconds = 0f;
            InteractionDriver.Deactivate(this);
        }

        private float CollectDespawnScale()
        {
            if (!collectDespawnStarted || definition == null)
            {
                return 1f;
            }

            float totalSeconds = Mathf.Max(0.01f, definition.collectDisappearSeconds);
            float finalScale = Mathf.Clamp01(definition.collectShrinkFinalScaleMultiplier);
            EaseSettings ease = definition.collectDisappearEase.shape > 0f
                ? definition.collectDisappearEase
                : EaseSettings.InBack;
            float t = Mathf.Clamp01(collectDespawnElapsedSeconds / totalSeconds);
            float eased = GameEasing.Evaluate01(ease, t);
            return Mathf.LerpUnclamped(1f, finalScale, eased);
        }

        private void PlayTicketPaperBurst(int grantedTickets)
        {
            _ = grantedTickets;
            if (definition == null || !definition.ticketBurstEnabled || definition.ticketBurstCount <= 0)
            {
                return;
            }

            ResolveReferences();
            Vector3 origin = InteractionDriver.ResolveRewardBurstOrigin(transform, FallbackPromptHeight(), liftMultiplier: 0.18f, minimumLift: 0.12f);
            lastTicketBurst = transientVfx?.ShowTicketPaperBurst(definition, origin);
        }
    }
}
