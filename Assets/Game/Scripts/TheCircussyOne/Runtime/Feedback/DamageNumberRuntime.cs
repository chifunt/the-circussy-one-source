using UnityEngine;
using TheCircussyOne.Visuals;

namespace TheCircussyOne.Runtime
{
    public sealed class DamageNumberRuntime
    {
        public DamageNumberRuntime(
            DamageNumberView view,
            Vector3 startPosition,
            Vector3 driftDirection,
            int amount,
            float spawnedAt,
            float lifetime,
            int mergeKey,
            float mergeUntil)
        {
            View = view;
            StartPosition = startPosition;
            DriftDirection = driftDirection;
            Amount = amount;
            SpawnedAt = spawnedAt;
            Lifetime = lifetime;
            MergeKey = mergeKey;
            MergeUntil = mergeUntil;
        }

        public DamageNumberView View { get; }
        public Vector3 StartPosition { get; }
        public Vector3 DriftDirection { get; }
        public int Amount { get; private set; }
        public float SpawnedAt { get; private set; }
        public float Lifetime { get; private set; }
        public int MergeKey { get; }
        public float MergeUntil { get; private set; }

        public bool CanMerge(int mergeKey, float time)
        {
            return mergeKey != 0 && MergeKey == mergeKey && time <= MergeUntil;
        }

        public void Merge(int amount, float time, float lifetime, float mergeWindow)
        {
            Amount += Mathf.Max(0, amount);
            SpawnedAt = time;
            Lifetime = lifetime;
            MergeUntil = time + Mathf.Max(0f, mergeWindow);
            View?.SetAmount(Amount);
        }

        public bool IsExpired(float time)
        {
            return time >= SpawnedAt + Lifetime;
        }
    }
}
