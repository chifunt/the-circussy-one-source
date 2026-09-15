using UnityEngine;

namespace TheCircussyOne.Runtime
{
    public readonly struct DamageNumberRequest
    {
        public DamageNumberRequest(int amount, Vector3 anchorPosition, Vector3 impactPosition, int mergeKey)
        {
            Amount = amount;
            AnchorPosition = anchorPosition;
            ImpactPosition = impactPosition;
            MergeKey = mergeKey;
        }

        public int Amount { get; }
        public Vector3 AnchorPosition { get; }
        public Vector3 ImpactPosition { get; }
        public int MergeKey { get; }
        public bool HasMergeKey => MergeKey != 0;
    }

    public interface IDamageNumberSpawner
    {
        void Show(DamageNumberRequest request);
    }

    public sealed class NullDamageNumberSpawner : IDamageNumberSpawner
    {
        public void Show(DamageNumberRequest request)
        {
        }
    }
}
