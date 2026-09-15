using UnityEngine;

namespace TheCircussyOne.Runtime
{
    public enum InteractPriority
    {
        Other = 0,
        TicketDeposit = 10,
        HealingProp = 10,
        ChestOrShrine = 20,
        FinishObject = 30
    }

    public interface IInteractable
    {
        bool IsInteractionAvailable { get; }
        InteractPriority Priority { get; }
        Vector3 InteractionPosition { get; }
        string PromptText { get; }
        float HoldSeconds { get; }
        float Progress { get; }

        void SetInteractionTargeted(bool targeted);
        void BeginInteraction();
        void TickInteraction(float deltaTime, RunCurrencyState currency);
        void CancelInteraction();
    }

    public interface ICurrencyAwareInteractable
    {
        string PromptTextFor(RunCurrencyState currency);
        bool CanBeginInteraction(RunCurrencyState currency);
    }
}
