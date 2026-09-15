using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;

namespace TheCircussyOne.Runtime
{
    public interface IWorldRewardAnimationTarget
    {
        bool ShouldTickWorldRewardAnimation { get; }
        void TickWorldRewardAnimation(float deltaTime);
    }

    public sealed class WorldRewardAnimationSystem : ITickable
    {
        private readonly IGameTime time;
        private readonly List<IWorldRewardAnimationTarget> targets = new();

        public WorldRewardAnimationSystem(IGameTime time)
        {
            this.time = time;
        }

        public void Tick()
        {
            SceneWorldRewardAnimationRegistry.CopyTo(targets);
            float deltaTime = Mathf.Max(0f, time != null ? time.DeltaTime : 0f);
            for (int i = 0; i < targets.Count; i++)
            {
                IWorldRewardAnimationTarget target = targets[i];
                if (target == null || IsDestroyed(target) || !target.ShouldTickWorldRewardAnimation)
                {
                    continue;
                }

                target.TickWorldRewardAnimation(deltaTime);
            }

            targets.Clear();
        }

        private static bool IsDestroyed(IWorldRewardAnimationTarget target)
        {
            return target is Object unityObject && unityObject == null;
        }
    }

    public static class SceneWorldRewardAnimationRegistry
    {
        private static readonly HashSet<IWorldRewardAnimationTarget> Targets = new();

        public static int Count
        {
            get
            {
                PruneDestroyed();
                return Targets.Count;
            }
        }

        public static void Register(IWorldRewardAnimationTarget target)
        {
            if (target != null && !IsDestroyed(target))
            {
                Targets.Add(target);
            }
        }

        public static void Unregister(IWorldRewardAnimationTarget target)
        {
            if (target != null)
            {
                Targets.Remove(target);
            }
        }

        public static void Clear()
        {
            Targets.Clear();
        }

        public static void CopyTo(List<IWorldRewardAnimationTarget> results)
        {
            if (results == null)
            {
                return;
            }

            PruneDestroyed();
            foreach (IWorldRewardAnimationTarget target in Targets)
            {
                results.Add(target);
            }
        }

        private static void PruneDestroyed()
        {
            Targets.RemoveWhere(IsDestroyed);
        }

        private static bool IsDestroyed(IWorldRewardAnimationTarget target)
        {
            return target is Object unityObject && unityObject == null;
        }
    }
}
