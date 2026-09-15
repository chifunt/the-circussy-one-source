using UnityEngine;
using TheCircussyOne.Content;
using TheCircussyOne.Visuals;

namespace TheCircussyOne.Runtime
{
    public sealed class PickupRuntime
    {
        public PickupRuntime(PickupView view, int amount)
            : this(view, (XpGemDefinition)null, amount)
        {
        }

        public PickupRuntime(PickupView view, XpGemDefinition gem, int amount)
        {
            View = view;
            Gem = gem;
            Reward = PickupRewardPayload.Experience(gem, amount);
        }

        public PickupRuntime(PickupView view, HealthPickupDefinition pickup, int amount)
        {
            View = view;
            HealthPickup = pickup;
            Reward = PickupRewardPayload.Health(pickup, amount);
        }

        public PickupView View { get; }
        public XpGemDefinition Gem { get; }
        public HealthPickupDefinition HealthPickup { get; }
        public PickupRewardPayload Reward { get; }
        public int Amount => Reward.Amount;
        public Vector3 Position => View != null ? View.Position : Vector3.zero;
    }
}
