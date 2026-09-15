using System.Collections.Generic;

namespace TheCircussyOne.Runtime
{
    public enum SceneInteractableKind
    {
        Other = 0,
        Chest = 10,
        TicketDeposit = 20,
        HealingProp = 30,
        FinishObject = 40
    }

    public interface IInteractableSource
    {
        void GetInteractables(List<IInteractable> results);
    }

    public sealed class SceneInteractableSource : IInteractableSource
    {
        public void GetInteractables(List<IInteractable> results)
        {
            SceneInteractableRegistry.CopyTo(results);
        }
    }

    public static class SceneInteractableRegistry
    {
        private static readonly Dictionary<IInteractable, SceneInteractableKind> Interactables = new();

        public static int Count => Interactables.Count;

        public static int CountByKind(SceneInteractableKind kind)
        {
            int count = 0;
            foreach (SceneInteractableKind registeredKind in Interactables.Values)
            {
                if (registeredKind == kind)
                {
                    count++;
                }
            }

            return count;
        }

        public static void Register(IInteractable interactable, SceneInteractableKind kind = SceneInteractableKind.Other)
        {
            if (interactable != null)
            {
                Interactables[interactable] = kind;
            }
        }

        public static void Unregister(IInteractable interactable)
        {
            if (interactable != null)
            {
                _ = Interactables.Remove(interactable);
            }
        }

        public static void Clear()
        {
            Interactables.Clear();
        }

        public static void CopyTo(List<IInteractable> results)
        {
            if (results == null)
            {
                return;
            }

            foreach (IInteractable interactable in Interactables.Keys)
            {
                results.Add(interactable);
            }
        }
    }
}
