using System;
using System.Collections.Generic;

namespace TheCircussyOne.Runtime
{
    public sealed class RunPauseState
    {
        private readonly HashSet<string> reasons = new(StringComparer.Ordinal);

        public bool IsPaused => reasons.Count > 0;
        public event Action<bool> PauseChanged;

        public bool HasReason(string reason)
        {
            string safeReason = string.IsNullOrWhiteSpace(reason) ? "unknown" : reason;
            return reasons.Contains(safeReason);
        }

        public void Pause(string reason)
        {
            string safeReason = string.IsNullOrWhiteSpace(reason) ? "unknown" : reason;
            bool wasPaused = IsPaused;
            reasons.Add(safeReason);
            if (wasPaused != IsPaused)
            {
                PauseChanged?.Invoke(IsPaused);
            }
        }

        public void Resume(string reason)
        {
            string safeReason = string.IsNullOrWhiteSpace(reason) ? "unknown" : reason;
            bool wasPaused = IsPaused;
            reasons.Remove(safeReason);
            if (wasPaused != IsPaused)
            {
                PauseChanged?.Invoke(IsPaused);
            }
        }

        public void Clear()
        {
            bool wasPaused = IsPaused;
            reasons.Clear();
            if (wasPaused)
            {
                PauseChanged?.Invoke(false);
            }
        }
    }

    public interface ITickableWhenPaused
    {
    }

    public interface IFixedTickableWhenPaused
    {
    }

    public interface ILateTickableWhenPaused
    {
    }
}
