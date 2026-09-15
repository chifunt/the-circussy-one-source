using TheCircussyOne.Runtime;
using UnityEngine;

namespace TheCircussyOne.Visuals
{
    public sealed class FinishPortalView : MonoBehaviour, IInteractable, IWorldInteractionPromptTarget, IWorldRewardInteractionBehavior
    {
        private const float DefaultHoldSeconds = 0.75f;

        private const float PromptHeight = 3.25f;

        private readonly Color visualColor = new(0.58f, 0.08f, 0.08f, 1f);
        private readonly Color emissionColor = new(1f, 0.46f, 0.12f, 1f);
        private readonly Color outlineColor = new(1f, 0.78f, 0.22f, 1f);
        private WorldRewardInteractableDriver interactionDriver;
        private RunPhaseState phaseState;
        private IGameAudio gameAudio;
        private IGameHaptics gameHaptics;
        private float progressSeconds;
        private bool finalAct;
        private bool completed;

        private WorldRewardInteractableDriver InteractionDriver => interactionDriver ??= new WorldRewardInteractableDriver(this);

        public bool IsInteractionAvailable => InteractionDriver.IsInteractionAvailable;
        public InteractPriority Priority => InteractPriority.FinishObject;
        public Vector3 InteractionPosition => transform.position;
        public Vector3 WorldInteractionPromptPosition => InteractionDriver.ResolvePromptPosition(transform, PromptHeight);
        public string PromptText => finalAct ? "Take the Curtain Call" : "Enter Next Act";
        public float HoldSeconds => DefaultHoldSeconds;
        public float Progress => Mathf.Clamp01(progressSeconds / DefaultHoldSeconds);
        public bool IsRewardInteractionAvailable => !completed
            && gameObject.activeInHierarchy
            && phaseState != null
            && phaseState.Is(RunPhase.Encore);
        public SceneInteractableKind RegistryKind => SceneInteractableKind.FinishObject;
        public WorldInteractableVisualStyle VisualStyle => new(
            visualScale: 1f,
            visualColor: visualColor,
            emissionColor: emissionColor,
            emissionStrength: 1.4f,
            outlineColor: outlineColor,
            outlineThickness: 0.08f,
            squashStretchAmplitude: 0.08f,
            squashStretchFrequency: 7.5f,
            fallbackPromptHeight: PromptHeight,
            targetedScaleMultiplier: 1.12f,
            targetedColorLerp: 0.3f,
            targetedEmissionMultiplier: 1.4f);

        private void Awake()
        {
            InteractionDriver.Resolve(transform);
            InteractionDriver.ApplyVisuals(transform);
        }

        private void OnEnable()
        {
            InteractionDriver.Register(this, this);
            InteractionDriver.ApplyVisuals(transform);
        }

        private void OnDisable()
        {
            InteractionDriver.Deactivate(this);
        }

        public void Prepare(RunPhaseState activePhaseState)
        {
            Prepare(activePhaseState, false);
        }

        public void Prepare(RunPhaseState activePhaseState, bool isFinalAct, IGameAudio activeAudio = null, IGameHaptics activeHaptics = null)
        {
            phaseState = activePhaseState;
            finalAct = isFinalAct;
            gameAudio = activeAudio ?? NullGameAudio.Instance;
            gameHaptics = activeHaptics ?? NullGameHaptics.Instance;
            progressSeconds = 0f;
            completed = false;
            InteractionDriver.ResetState();
            InteractionDriver.Resolve(transform);
            InteractionDriver.Register(this, this);
            InteractionDriver.ApplyVisuals(transform);
        }

        public void SetInteractionTargeted(bool targeted)
        {
            InteractionDriver.SetInteractionTargeted(this, transform, targeted);
        }

        public void BeginInteraction()
        {
            InteractionDriver.BeginInteraction(transform);
        }

        public void TickInteraction(float deltaTime, RunCurrencyState currency)
        {
            WorldRewardInteractionTickResult result = InteractionDriver.TickInteraction(this, transform, deltaTime, currency);
            if (result.Completed)
            {
                gameObject.SetActive(false);
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
            if (!IsRewardInteractionAvailable)
            {
                return WorldRewardInteractionTickResult.Unavailable();
            }

            progressSeconds += Mathf.Max(0f, deltaTime);
            if (progressSeconds < DefaultHoldSeconds)
            {
                return WorldRewardInteractionTickResult.InProgress();
            }

            completed = true;
            gameAudio?.PlayAt(GameAudioCue.StageDoorOpen, transform.position);
            gameHaptics?.Play(GameHapticsCue.StageDoorOpen);
            if (finalAct)
            {
                phaseState.FinishRun();
            }
            else
            {
                phaseState.EnterIntermission();
            }

            return WorldRewardInteractionTickResult.CompletedResult();
        }

        public void CancelRewardInteraction()
        {
            progressSeconds = 0f;
        }
    }
}
