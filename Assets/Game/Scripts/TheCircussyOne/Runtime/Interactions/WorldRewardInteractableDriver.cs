using TheCircussyOne.Visuals;
using UnityEngine;

namespace TheCircussyOne.Runtime
{
    public enum WorldRewardInteractionStatus
    {
        InProgress = 0,
        Completed = 1,
        Unavailable = 2
    }

    public readonly struct WorldRewardInteractionTickResult
    {
        private WorldRewardInteractionTickResult(WorldRewardInteractionStatus status)
        {
            Status = status;
        }

        public WorldRewardInteractionStatus Status { get; }
        public bool Completed => Status == WorldRewardInteractionStatus.Completed;

        public static WorldRewardInteractionTickResult InProgress()
        {
            return new WorldRewardInteractionTickResult(WorldRewardInteractionStatus.InProgress);
        }

        public static WorldRewardInteractionTickResult CompletedResult()
        {
            return new WorldRewardInteractionTickResult(WorldRewardInteractionStatus.Completed);
        }

        public static WorldRewardInteractionTickResult Unavailable()
        {
            return new WorldRewardInteractionTickResult(WorldRewardInteractionStatus.Unavailable);
        }
    }

    public interface IWorldRewardInteractionBehavior
    {
        bool IsRewardInteractionAvailable { get; }
        SceneInteractableKind RegistryKind { get; }
        WorldInteractableVisualStyle VisualStyle { get; }

        void BeginRewardInteraction();
        WorldRewardInteractionTickResult TickRewardInteraction(float deltaTime, RunCurrencyState currency);
        void CancelRewardInteraction();
    }

    public sealed class WorldRewardInteractableDriver
    {
        private readonly IWorldRewardInteractionBehavior behavior;
        private readonly WorldInteractableVisualDriver visualDriver = new();

        private WorldInteractableViewState visualState;
        private bool activeRegistered;
        private Transform visualRoot;
        private Renderer[] renderers;

        public WorldRewardInteractableDriver(IWorldRewardInteractionBehavior behavior)
        {
            this.behavior = behavior;
        }

        public bool IsInteractionAvailable => behavior?.IsRewardInteractionAvailable == true;
        public bool IsTargeted => visualState.Targeted;
        public bool IsInteracting => visualState.Interacting;

        public void ResetState()
        {
            visualState = default;
        }

        public void Resolve(Transform owner)
        {
            visualDriver.Resolve(owner, ref visualRoot, ref renderers);
        }

        public void Register(MonoBehaviour owner, IInteractable interactable)
        {
            if (activeRegistered || owner == null || interactable == null || !owner.isActiveAndEnabled || behavior == null)
            {
                return;
            }

            activeRegistered = true;
            SceneInteractableRegistry.Register(interactable, behavior.RegistryKind);
        }

        public void Deactivate(IInteractable interactable)
        {
            Unregister(interactable);
            visualState.ClearInteraction();
            visualDriver.ClearOutline(interactable);
        }

        public void Unregister(IInteractable interactable)
        {
            if (!activeRegistered)
            {
                return;
            }

            activeRegistered = false;
            SceneInteractableRegistry.Unregister(interactable);
        }

        public Vector3 ResolvePromptPosition(Transform owner, float fallbackHeight)
        {
            Resolve(owner);
            return visualDriver.ResolvePromptPosition(owner, fallbackHeight);
        }

        public Vector3 ResolveRewardBurstOrigin(
            Transform owner,
            float fallbackHeight,
            float liftMultiplier = 0.25f,
            float minimumLift = 0.15f)
        {
            Resolve(owner);
            return visualDriver.ResolveRewardBurstOrigin(owner, fallbackHeight, liftMultiplier, minimumLift);
        }

        public void ApplyVisuals(Transform owner, float extraScaleMultiplier = 1f)
        {
            if (behavior == null)
            {
                return;
            }

            Resolve(owner);
            visualDriver.Apply(behavior.VisualStyle, visualState, extraScaleMultiplier);
        }

        public void RefreshOutline(IInteractable interactable)
        {
            if (behavior == null)
            {
                visualDriver.ClearOutline(interactable);
                return;
            }

            WorldInteractableVisualStyle style = behavior.VisualStyle;
            visualDriver.UpdateOutline(
                interactable,
                visualState.Targeted && behavior.IsRewardInteractionAvailable,
                style.OutlineColor,
                style.OutlineThickness);
        }

        public void SetInteractionTargeted(IInteractable interactable, Transform owner, bool targeted)
        {
            visualState.SetTargeted(targeted);
            RefreshOutline(interactable);
            ApplyVisuals(owner);
        }

        public void BeginInteraction(Transform owner)
        {
            behavior?.BeginRewardInteraction();
            visualState.BeginInteraction();
            ApplyVisuals(owner);
        }

        public WorldRewardInteractionTickResult TickInteraction(
            IInteractable interactable,
            Transform owner,
            float deltaTime,
            RunCurrencyState currency)
        {
            if (behavior == null)
            {
                return WorldRewardInteractionTickResult.Unavailable();
            }

            visualState.AdvanceInteraction(deltaTime);
            WorldRewardInteractionTickResult result = behavior.TickRewardInteraction(deltaTime, currency);
            if (result.Completed)
            {
                visualState.ClearInteraction();
                RefreshOutline(interactable);
                return result;
            }

            if (result.Status == WorldRewardInteractionStatus.Unavailable)
            {
                visualState.CancelInteraction();
                RefreshOutline(interactable);
            }

            ApplyVisuals(owner);
            return result;
        }

        public void CancelInteraction(Transform owner)
        {
            visualState.CancelInteraction();
            behavior?.CancelRewardInteraction();
            ApplyVisuals(owner);
        }
    }
}
