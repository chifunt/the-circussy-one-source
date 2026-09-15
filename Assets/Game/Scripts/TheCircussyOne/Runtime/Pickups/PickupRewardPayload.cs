using TheCircussyOne.Content;

namespace TheCircussyOne.Runtime
{
    public enum PickupRewardKind
    {
        Experience = 0,
        Health = 1
    }

    public readonly struct PickupRewardPayload
    {
        public PickupRewardPayload(PickupRewardKind kind, int amount, string sourceId)
        {
            Kind = kind;
            Amount = amount;
            SourceId = sourceId ?? string.Empty;
        }

        public PickupRewardKind Kind { get; }
        public int Amount { get; }
        public string SourceId { get; }

        public static PickupRewardPayload Experience(XpGemDefinition gem, int amount)
        {
            return new PickupRewardPayload(PickupRewardKind.Experience, amount, gem != null ? gem.Id : "xp");
        }

        public static PickupRewardPayload Health(HealthPickupDefinition pickup, int amount)
        {
            return new PickupRewardPayload(PickupRewardKind.Health, amount, pickup != null ? pickup.Id : "health");
        }
    }
}
