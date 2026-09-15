using TheCircussyOne.Content;
using UnityEngine;

namespace TheCircussyOne.Runtime
{
    public sealed class HealingPropRuntime
    {
        private readonly HealingPropDefinition definition;
        private float elapsedSeconds;

        public HealingPropRuntime(HealingPropDefinition definition)
        {
            this.definition = definition;
        }

        public bool Completed { get; private set; }
        public float Progress => definition == null ? 0f : Mathf.Clamp01(elapsedSeconds / Mathf.Max(0.001f, definition.holdSeconds));

        public bool Tick(float deltaTime)
        {
            if (definition == null || Completed)
            {
                return false;
            }

            elapsedSeconds = Mathf.Min(definition.holdSeconds, elapsedSeconds + Mathf.Max(0f, deltaTime));
            if (elapsedSeconds < definition.holdSeconds)
            {
                return false;
            }

            Completed = true;
            return true;
        }

        public void Cancel()
        {
            if (!Completed)
            {
                elapsedSeconds = 0f;
            }
        }
    }

    public static class HealingPropRules
    {
        public static int ResolvePickupCount(HealingPropDefinition definition, int seed)
        {
            if (definition == null)
            {
                return 1;
            }

            int min = Mathf.Max(1, definition.minPickupCount);
            int max = Mathf.Max(min, definition.maxPickupCount);
            if (max == min)
            {
                return min;
            }

            uint range = (uint)(max - min + 1);
            uint hashed = (uint)seed * 747796405u + 2891336453u;
            return min + (int)(hashed % range);
        }
    }
}
