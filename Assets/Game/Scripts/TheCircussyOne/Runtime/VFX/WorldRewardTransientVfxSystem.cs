using System;
using System.Collections.Generic;
using TheCircussyOne.Content;
using UnityEngine;
using VContainer.Unity;

namespace TheCircussyOne.Runtime
{
    public interface IWorldRewardTransientVfxSpawner
    {
        ParticleSystem ShowTicketPaperBurst(TicketDepositDefinition definition, Vector3 origin);
        ParticleSystem ShowSnackBurst(HealingPropDefinition definition, Vector3 origin);
    }

    public sealed class WorldRewardTransientVfxSystem : IWorldRewardTransientVfxSpawner, IRunResettable
    {
        private readonly IGameTime time;
        private readonly WorldRewardTransientVfxFactory factory;
        private readonly List<RewardBurstRuntime> active = new();

        public WorldRewardTransientVfxSystem(IGameTime time, WorldRewardTransientVfxFactory factory = null)
        {
            this.time = time;
            this.factory = factory ?? new WorldRewardTransientVfxFactory();
        }

        public int ActiveCount => active.Count;
        public int PooledTicketBurstCount => factory != null ? factory.PooledTicketBurstCount : 0;
        public int PooledSnackBurstCount => factory != null ? factory.PooledSnackBurstCount : 0;

        public int PrewarmTicketBursts(int targetPoolCount, int maxCreate = int.MaxValue)
        {
            return factory != null ? factory.PrewarmTicketBursts(targetPoolCount, maxCreate) : 0;
        }

        public int PrewarmSnackBursts(int targetPoolCount, int maxCreate = int.MaxValue)
        {
            return factory != null ? factory.PrewarmSnackBursts(targetPoolCount, maxCreate) : 0;
        }

        public ParticleSystem ShowTicketPaperBurst(TicketDepositDefinition definition, Vector3 origin)
        {
            ParticleSystem burst = factory.SpawnTicketPaperBurst(definition, origin);
            if (burst == null)
            {
                return null;
            }

            Track(burst.gameObject, burst, RewardBurstKind.Ticket, definition.ticketBurstLifetimeSeconds);
            return burst;
        }

        public ParticleSystem ShowSnackBurst(HealingPropDefinition definition, Vector3 origin)
        {
            ParticleSystem burst = factory.SpawnSnackBurst(definition, origin);
            if (burst == null)
            {
                return null;
            }

            Track(burst.gameObject, burst, RewardBurstKind.Snack, definition.snackBurstLifetimeSeconds);
            return burst;
        }

        public void Tick()
        {
            float now = time != null ? time.Time : 0f;
            for (int i = active.Count - 1; i >= 0; i--)
            {
                RewardBurstRuntime runtime = active[i];
                if (runtime.Root == null || runtime.Particles == null)
                {
                    active.RemoveAt(i);
                    continue;
                }

                if (now - runtime.StartedAt < runtime.DestroyAfterSeconds)
                {
                    continue;
                }

                Release(runtime);
                active.RemoveAt(i);
            }
        }

        public void ForEachActiveView(Action<Component> visitor)
        {
            if (visitor == null)
            {
                return;
            }

            for (int i = active.Count - 1; i >= 0; i--)
            {
                ParticleSystem particles = active[i].Particles;
                if (particles != null && particles.gameObject.activeInHierarchy)
                {
                    visitor(particles);
                }
            }
        }

        public void Clear()
        {
            for (int i = active.Count - 1; i >= 0; i--)
            {
                Release(active[i]);
            }

            active.Clear();
        }

        public void ResetRunState(RunResetContext context)
        {
            Clear();
        }

        private void Track(GameObject burstObject, ParticleSystem burst, RewardBurstKind kind, float lifetimeSeconds)
        {
            float lifetime = Mathf.Max(0.01f, lifetimeSeconds);
            active.Add(new RewardBurstRuntime(
                burstObject,
                burst,
                kind,
                time != null ? time.Time : 0f,
                lifetime * 1.35f + 0.25f));
        }

        private void Release(RewardBurstRuntime runtime)
        {
            if (runtime.Particles == null)
            {
                if (runtime.Root != null)
                {
                    factory.Release(runtime.Root);
                }

                return;
            }

            switch (runtime.Kind)
            {
                case RewardBurstKind.Ticket:
                    factory.ReleaseTicketPaperBurst(runtime.Particles);
                    break;
                case RewardBurstKind.Snack:
                    factory.ReleaseSnackBurst(runtime.Particles);
                    break;
                default:
                    factory.Release(runtime.Root);
                    break;
            }
        }

        private enum RewardBurstKind
        {
            Ticket,
            Snack
        }

        private readonly struct RewardBurstRuntime
        {
            public RewardBurstRuntime(GameObject root, ParticleSystem particles, RewardBurstKind kind, float startedAt, float destroyAfterSeconds)
            {
                Root = root;
                Particles = particles;
                Kind = kind;
                StartedAt = startedAt;
                DestroyAfterSeconds = destroyAfterSeconds;
            }

            public GameObject Root { get; }
            public ParticleSystem Particles { get; }
            public RewardBurstKind Kind { get; }
            public float StartedAt { get; }
            public float DestroyAfterSeconds { get; }
        }
    }

    public sealed class WorldRewardTransientVfxTickSystem : ITickable
    {
        private readonly WorldRewardTransientVfxSystem transientVfxSystem;

        public WorldRewardTransientVfxTickSystem(WorldRewardTransientVfxSystem transientVfxSystem)
        {
            this.transientVfxSystem = transientVfxSystem;
        }

        public void Tick()
        {
            transientVfxSystem?.Tick();
        }
    }
}
