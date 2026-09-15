using System;
using UnityEngine;

namespace TheCircussyOne.Runtime
{
    public sealed class RunCurrencyState
    {
        public int Tickets { get; private set; }
        public event Action<int> TicketsChanged;

        public void Reset()
        {
            if (Tickets == 0)
            {
                return;
            }

            Tickets = 0;
            TicketsChanged?.Invoke(Tickets);
        }

        public void AddTickets(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            Tickets = Mathf.Max(0, Tickets + amount);
            TicketsChanged?.Invoke(Tickets);
        }

        public bool CanSpendTickets(int amount)
        {
            return amount >= 0 && Tickets >= amount;
        }

        public bool TrySpendTickets(int amount)
        {
            if (!CanSpendTickets(amount))
            {
                return false;
            }

            if (amount == 0)
            {
                return true;
            }

            Tickets -= amount;
            TicketsChanged?.Invoke(Tickets);
            return true;
        }
    }
}
