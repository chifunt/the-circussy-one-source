using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using TheCircussyOne.Runtime;
using UnityEngine;

namespace TheCircussyOne.Visuals
{
    public sealed class HealingPropView : MonoBehaviour, IInteractable, IWorldInteractionPromptTarget, IWorldRewardInteractionBehavior, IWorldRewardAnimationTarget
    {
        [SerializeField] private HealingPropDefinition definition;

        private WorldRewardInteractableDriver interactionDriver;
        private PickupFactory pickupFactory;
        private HealthPickupDefinition fallbackHealthPickup;
        private RunSeedState runSeedState;
        private RunPauseState pauseState;
        private IWorldRewardTransientVfxSpawner transientVfx;
        private IGameAudio gameAudio;
        private IGameHaptics gameHaptics;
        private HealingPropRuntime runtime;
        private bool openDespawnStarted;
        private bool openDespawnCompleted;
        private float openDespawnElapsedSeconds;
        private ParticleSystem lastSnackBurst;

        private WorldRewardInteractableDriver InteractionDriver => interactionDriver ??= new WorldRewardInteractableDriver(this);
        public HealingPropDefinition Definition => definition;
        public bool IsInteractionAvailable => InteractionDriver.IsInteractionAvailable;
        public bool IsOpen => runtime?.Completed == true;
        public bool IsOpenDespawnStarted => openDespawnStarted;
        public float OpenDespawnElapsedSeconds => openDespawnElapsedSeconds;
        public ParticleSystem LastSnackBurst => lastSnackBurst;
        public bool ShouldTickWorldRewardAnimation => openDespawnStarted && !openDespawnCompleted && gameObject.activeSelf;
        public InteractPriority Priority => InteractPriority.HealingProp;
        public Vector3 InteractionPosition => transform.position;
        public Vector3 WorldInteractionPromptPosition
        {
            get
            {
                return InteractionDriver.ResolvePromptPosition(transform, FallbackPromptHeight());
            }
        }

        public string PromptText => definition != null && !string.IsNullOrWhiteSpace(definition.promptText) ? definition.promptText : "Grab a Snack";
        public float HoldSeconds => definition != null ? definition.holdSeconds : 0f;
        public float Progress => runtime?.Progress ?? 0f;
        public bool IsRewardInteractionAvailable => definition != null && definition.isActive && runtime?.Completed != true && gameObject.activeInHierarchy;
        public SceneInteractableKind RegistryKind => SceneInteractableKind.HealingProp;
        public WorldInteractableVisualStyle VisualStyle => CreateVisualStyle();

        private void Awake()
        {
            ResolveReferences();
            ApplyVisuals();
        }

        private void OnEnable()
        {
            ResolveReferences();
            runtime ??= new HealingPropRuntime(definition);
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
            HealingPropDefinition propDefinition,
            PickupFactory activePickupFactory,
            HealthPickupDefinition defaultHealthPickup,
            RunSeedState activeRunSeedState = null,
            RunPauseState activePauseState = null,
            IWorldRewardTransientVfxSpawner activeTransientVfx = null,
            IGameAudio activeAudio = null,
            IGameHaptics activeHaptics = null)
        {
            definition = propDefinition;
            pickupFactory = activePickupFactory;
            fallbackHealthPickup = defaultHealthPickup;
            runSeedState = activeRunSeedState;
            pauseState = activePauseState;
            transientVfx = activeTransientVfx;
            gameAudio = activeAudio ?? NullGameAudio.Instance;
            gameHaptics = activeHaptics ?? NullGameHaptics.Instance;
            runtime = new HealingPropRuntime(definition);
            InteractionDriver.ResetState();
            openDespawnStarted = false;
            openDespawnCompleted = false;
            openDespawnElapsedSeconds = 0f;
            lastSnackBurst = null;
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
            runtime ??= new HealingPropRuntime(definition);
            InteractionDriver.BeginInteraction(transform);
        }

        public void TickInteraction(float deltaTime, RunCurrencyState currency)
        {
            runtime ??= new HealingPropRuntime(definition);
            WorldRewardInteractionTickResult result = InteractionDriver.TickInteraction(this, transform, deltaTime, currency);
            if (result.Completed && !openDespawnStarted)
            {
                gameAudio?.PlayAt(GameAudioCue.SnackOpen, transform.position);
                gameHaptics?.Play(GameHapticsCue.SnackOpen);
                SpawnHealthPickups();
                PlaySnackBurst();
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
        }

        public WorldRewardInteractionTickResult TickRewardInteraction(float deltaTime, RunCurrencyState currency)
        {
            return runtime.Tick(deltaTime)
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

        private void SpawnHealthPickups()
        {
            HealthPickupDefinition pickup = definition != null && definition.healthPickup != null
                ? definition.healthPickup
                : fallbackHealthPickup;
            if (pickupFactory == null || pickup == null)
            {
                return;
            }

            int seed = TicketDepositRules.StableSeed(
                definition != null ? definition.Id : string.Empty,
                transform.position,
                runSeedState != null ? runSeedState.CurrentSeed : 0);
            int count = HealingPropRules.ResolvePickupCount(definition, seed);
            Vector3 origin = ResolveRewardBurstOrigin();
            for (int i = 0; i < count; i++)
            {
                pickupFactory.SpawnDrop(pickup, origin, i, count, seed);
            }
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
            InteractionDriver.ApplyVisuals(transform, OpenDespawnScale());
        }

        public void TickOpenDespawn(float deltaTime)
        {
            if (!openDespawnStarted || openDespawnCompleted || definition == null || !gameObject.activeSelf)
            {
                return;
            }

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

        private void StartOpenDespawn()
        {
            if (openDespawnStarted)
            {
                return;
            }

            openDespawnStarted = true;
            openDespawnCompleted = false;
            openDespawnElapsedSeconds = 0f;
            InteractionDriver.Deactivate(this);
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

        private Vector3 ResolveRewardBurstOrigin()
        {
            ResolveReferences();
            float fallbackHeight = Mathf.Max(0.65f, definition != null ? definition.visualScale * 0.65f : 0.65f);
            return InteractionDriver.ResolveRewardBurstOrigin(transform, fallbackHeight);
        }

        private WorldInteractableVisualStyle CreateVisualStyle()
        {
            bool open = IsOpen;
            return new WorldInteractableVisualStyle(
                definition.visualScale,
                definition.visualColor,
                definition.emissionColor,
                definition.emissionStrength * (open ? 0.18f : 1f),
                definition.targetOutlineColor,
                definition.targetOutlineThickness,
                open ? 0f : definition.interactionSquashStretchAmplitude,
                open ? 0f : definition.interactionSquashStretchFrequency,
                FallbackPromptHeight(),
                targetedScaleMultiplier: open ? 1f : 1.1f,
                visualMaterial: definition.visualMaterial);
        }

        private float FallbackPromptHeight()
        {
            return Mathf.Max(0.35f, definition != null ? definition.visualScale : 1f);
        }

        private void PlaySnackBurst()
        {
            if (definition == null || !definition.snackBurstEnabled || definition.snackBurstCount <= 0)
            {
                return;
            }

            Vector3 origin = ResolveRewardBurstOrigin();
            lastSnackBurst = transientVfx?.ShowSnackBurst(definition, origin);
        }
    }
}
