using System.Collections.Generic;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;
using TheCircussyOne.Visuals;
using UnityEngine;
using VContainer.Unity;

namespace TheCircussyOne.Runtime
{
    public sealed class InteractionSystem : ITickable, IRunResettable
    {
        private readonly GameConfig config;
        private readonly IInputService input;
        private readonly PlayerView player;
        private readonly IGameTime time;
        private readonly RunCurrencyState currency;
        private readonly HudView hud;
        private readonly IInteractableSource interactableSource;
        private readonly ControlInputModeTracker inputModeTracker;
        private readonly IWorldInteractionPrompt worldPrompt;
        private readonly RunPhaseState phaseState;
        private readonly WorldPhysicsQuery worldQuery;
        private readonly IGameAudio audio;
        private readonly IGameHaptics haptics;
        private readonly List<IInteractable> candidates = new();

        private IInteractable currentTarget;
        private bool holding;
        private float outOfRangeSeconds;

        public InteractionSystem(
            GameConfig config,
            IInputService input,
            PlayerView player,
            IGameTime time,
            RunCurrencyState currency,
            HudView hud = null,
            IInteractableSource interactableSource = null,
            ControlInputModeTracker inputModeTracker = null,
            IWorldInteractionPrompt worldPrompt = null,
            RunPhaseState phaseState = null,
            WorldPhysicsQuery worldQuery = null,
            IGameAudio audio = null,
            IGameHaptics haptics = null)
        {
            this.config = config;
            this.input = input;
            this.player = player;
            this.time = time;
            this.currency = currency;
            this.hud = hud;
            this.interactableSource = interactableSource ?? new SceneInteractableSource();
            this.inputModeTracker = inputModeTracker;
            this.worldPrompt = worldPrompt;
            this.phaseState = phaseState;
            this.worldQuery = worldQuery ?? new WorldPhysicsQuery();
            this.audio = audio ?? NullGameAudio.Instance;
            this.haptics = haptics ?? NullGameHaptics.Instance;
        }

        public IInteractable CurrentTarget => currentTarget;
        public bool IsHolding => holding;

        public void ResetRunState(RunResetContext context)
        {
            CancelCurrentTarget();
            candidates.Clear();
        }

        public void Tick()
        {
            if (input == null
                || player == null
                || (phaseState != null && !RunPhaseRules.ShouldAllowInteractions(phaseState.CurrentPhase)))
            {
                ClearCurrentTarget();
                HideInteractionPrompt();
                return;
            }

            float radius = config != null ? config.playerInteractRadius : 2.25f;
            Vector3 playerPosition = player.Position;

            if (holding)
            {
                TickHeldTarget(playerPosition, radius);
                return;
            }

            if (input.InteractReleasedThisFrame)
            {
                CancelCurrentTarget();
            }

            IInteractable best = SelectTarget(playerPosition, radius);
            SetCurrentTarget(best);
            if (best == null)
            {
                HideInteractionPrompt();
                return;
            }

            SetInteractionPrompt(best, PromptTextFor(best), 0f, isHolding: false);
            if ((input.InteractPressedThisFrame || input.InteractHeld) && CanBeginInteraction(best))
            {
                BeginHold(best);
                if (holding)
                {
                    TickHeldTarget(playerPosition, radius);
                }
            }
        }

        private void TickHeldTarget(Vector3 playerPosition, float radius)
        {
            if (currentTarget == null || !currentTarget.IsInteractionAvailable)
            {
                FinishOrRetarget(playerPosition, radius);
                return;
            }

            if (input.InteractReleasedThisFrame || !input.InteractHeld)
            {
                CancelCurrentTarget();
                SetCurrentTarget(SelectTarget(playerPosition, radius));
                SetInteractionPrompt(currentTarget, PromptTextFor(currentTarget), currentTarget?.Progress ?? 0f, isHolding: false);
                return;
            }

            if (!IsValidInteractionTarget(currentTarget, playerPosition, radius))
            {
                float graceSeconds = config != null ? config.playerInteractOutOfRangeGraceSeconds : 0.6f;
                outOfRangeSeconds += DeltaTime();
                SetInteractionPrompt(currentTarget, HoldingPromptTextFor(currentTarget), currentTarget.Progress, isHolding: true);
                if (outOfRangeSeconds > graceSeconds)
                {
                    CancelCurrentTarget(playCanceledSound: currentTarget.Progress > 0f);
                }

                return;
            }

            outOfRangeSeconds = 0f;
            currentTarget.TickInteraction(DeltaTime(), currency);
            if (currentTarget == null || !currentTarget.IsInteractionAvailable)
            {
                FinishOrRetarget(playerPosition, radius);
                return;
            }

            SetInteractionPrompt(currentTarget, HoldingPromptTextFor(currentTarget), currentTarget.Progress, isHolding: true);
            UpdateHoldAudio(currentTarget);
        }

        private void FinishOrRetarget(Vector3 playerPosition, float radius)
        {
            StopHoldAudio(currentTarget);
            ClearCurrentTarget();
            holding = false;
            outOfRangeSeconds = 0f;
            if (input != null && input.InteractHeld)
            {
                IInteractable next = SelectTarget(playerPosition, radius);
                if (next != null)
                {
                    SetCurrentTarget(next);
                    if (!CanBeginInteraction(next))
                    {
                        SetInteractionPrompt(next, PromptTextFor(next), next.Progress, isHolding: false);
                        return;
                    }

                    BeginHold(next);
                    SetInteractionPrompt(next, holding ? HoldingPromptTextFor(next) : PromptTextFor(next), next.Progress, isHolding: holding);
                    return;
                }
            }

            HideInteractionPrompt();
        }

        private void BeginHold(IInteractable target)
        {
            if (target == null)
            {
                return;
            }

            if (target is ICurrencyAwareInteractable currencyAware && !currencyAware.CanBeginInteraction(currency))
            {
                holding = false;
                return;
            }

            holding = true;
            outOfRangeSeconds = 0f;
            target.BeginInteraction();
            audio.StartLoop(GameAudioCue.InteractionHoldLoop, target, target.InteractionPosition, target.Progress);
        }

        private IInteractable SelectTarget(Vector3 playerPosition, float radius)
        {
            candidates.Clear();
            interactableSource?.GetInteractables(candidates);
            if (holding && IsValidInteractionTarget(currentTarget, playerPosition, radius))
            {
                return currentTarget;
            }

            IInteractable best = null;
            float bestDistanceSquared = float.PositiveInfinity;
            for (int i = 0; i < candidates.Count; i++)
            {
                IInteractable candidate = candidates[i];
                if (!IsValidInteractionTarget(candidate, playerPosition, radius))
                {
                    continue;
                }

                float distanceSquared = (candidate.InteractionPosition - playerPosition).sqrMagnitude;
                if (best == null || distanceSquared < bestDistanceSquared)
                {
                    best = candidate;
                    bestDistanceSquared = distanceSquared;
                }
            }

            return best;
        }

        private bool IsValidInteractionTarget(IInteractable target, Vector3 playerPosition, float radius)
        {
            return InteractionRules.IsValidCandidate(target, playerPosition, radius)
                && HasLineOfSightTo(target, playerPosition);
        }

        private bool HasLineOfSightTo(IInteractable target, Vector3 playerPosition)
        {
            if (target == null || config == null || !config.playerInteractRequiresLineOfSight)
            {
                return true;
            }

            int mask = InteractionLineOfSightMask();
            if (mask == 0)
            {
                return true;
            }

            Vector3 from = playerPosition + Vector3.up * Mathf.Max(0f, config.playerInteractLineOfSightStartHeight);
            Vector3 to = target.InteractionPosition + Vector3.up * Mathf.Max(0f, config.playerInteractLineOfSightTargetHeight);
            float radius = Mathf.Max(0.001f, config.playerInteractLineOfSightProbeRadius);
            Transform targetRoot = (target as Component)?.transform;
            return worldQuery.HasClearSpherePath(
                from,
                to,
                radius,
                mask,
                collider => ShouldIgnoreLineOfSightCollider(collider, targetRoot),
                QueryTriggerInteraction.Ignore);
        }

        private int InteractionLineOfSightMask()
        {
            int configured = config != null ? config.playerInteractLineOfSightMask.value : ~0;
            int mask = configured & GameLayers.EnvironmentMaskExcludingGameplay;
            return mask != 0 ? mask : GameLayers.EnvironmentMaskExcludingGameplay;
        }

        private static bool ShouldIgnoreLineOfSightCollider(Collider collider, Transform targetRoot)
        {
            return collider == null
                || targetRoot != null && collider.transform.IsChildOf(targetRoot);
        }

        private float DeltaTime()
        {
            return Mathf.Max(0f, time != null ? time.DeltaTime : 0f);
        }

        private void SetCurrentTarget(IInteractable target)
        {
            if (currentTarget == target)
            {
                return;
            }

            ClearCurrentTarget();
            currentTarget = target;
            currentTarget?.SetInteractionTargeted(true);
        }

        private void SetInteractionPrompt(IInteractable target, string prompt, float progress, bool isHolding)
        {
            if (target == null || string.IsNullOrWhiteSpace(prompt))
            {
                HideInteractionPrompt();
                return;
            }

            bool useGamepadGlyph = inputModeTracker?.CurrentMode == ControlInputMode.Gamepad;
            bool showGlyph = !isHolding && CanBeginInteraction(target);
            if (target is IWorldInteractionPromptTarget worldTarget && worldPrompt != null)
            {
                worldPrompt.Show(worldTarget, prompt, progress, isHolding, useGamepadGlyph, showGlyph);
                hud?.SetInteractionPromptVisible(false);
                return;
            }

            worldPrompt?.Hide();
            hud?.SetInteractionPrompt(prompt, progress, isHolding, useGamepadGlyph, showGlyph);
        }

        private string PromptTextFor(IInteractable target)
        {
            if (target == null)
            {
                return null;
            }

            return target is ICurrencyAwareInteractable currencyAware
                ? currencyAware.PromptTextFor(currency)
                : target.PromptText;
        }

        private string HoldingPromptTextFor(IInteractable target)
        {
            return target?.PromptText;
        }

        private bool CanBeginInteraction(IInteractable target)
        {
            return target is not ICurrencyAwareInteractable currencyAware || currencyAware.CanBeginInteraction(currency);
        }

        private void HideInteractionPrompt()
        {
            hud?.SetInteractionPromptVisible(false);
            worldPrompt?.Hide();
        }

        private void CancelCurrentTarget(bool playCanceledSound = false)
        {
            IInteractable target = currentTarget;
            StopHoldAudio(target);
            if (playCanceledSound && target != null)
            {
                audio.PlayAt(GameAudioCue.InteractionCanceled, target.InteractionPosition);
                haptics.Play(GameHapticsCue.InteractionCanceled);
            }

            currentTarget?.CancelInteraction();
            ClearCurrentTarget();
            holding = false;
            outOfRangeSeconds = 0f;
            HideInteractionPrompt();
        }

        private void ClearCurrentTarget()
        {
            StopHoldAudio(currentTarget);
            currentTarget?.SetInteractionTargeted(false);
            currentTarget = null;
        }

        private void UpdateHoldAudio(IInteractable target)
        {
            if (target != null && holding)
            {
                audio.UpdateLoop(GameAudioCue.InteractionHoldLoop, target, target.InteractionPosition, target.Progress);
            }
        }

        private void StopHoldAudio(IInteractable target)
        {
            if (target != null)
            {
                audio.StopLoop(GameAudioCue.InteractionHoldLoop, target);
            }
        }
    }
}
