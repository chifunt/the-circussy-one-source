using Cysharp.Threading.Tasks;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;
using TheCircussyOne.Visuals;
using UnityEngine;
using VContainer.Unity;

namespace TheCircussyOne.Runtime
{
    public sealed class BigTopEnvironmentSystem : ITickable, IRunWorldRefreshAdapter
    {
        private readonly RunWorldGenerationConfig worldConfig;
        private readonly GameConfig gameConfig;
        private readonly RunWorldGenerationState generationState;
        private readonly BigTopEnvironmentFactory factory;
        private readonly IGameTime time;

        private BigTopEnvironmentView view;
        private GameObject fallbackRoot;
        private int lastWorldIndex = -1;
        private int lastSeed;
        private int lastRootId;
        private bool lastUsedPrototype;
        private BigTopEnvironmentSpec lastSpec;

        public BigTopEnvironmentSystem(
            RunWorldGenerationConfig worldConfig,
            GameConfig gameConfig,
            RunWorldGenerationState generationState,
            BigTopEnvironmentFactory factory = null,
            IGameTime time = null)
        {
            this.worldConfig = worldConfig;
            this.gameConfig = gameConfig;
            this.generationState = generationState;
            this.factory = factory ?? new BigTopEnvironmentFactory();
            this.time = time;
        }

        public bool HasView => view != null && view.gameObject.activeInHierarchy;
        public float CurrentRadius => view != null ? view.Radius : 0f;
        public BigTopEnvironmentView View => view;
        public string DiagnosticsName => "BigTop";
        public string LoadingStage => "Raising the Big Top";

        public void Tick()
        {
            if (view != null)
            {
                view.Tick(time != null ? time.Time : 0f, worldConfig);
            }
        }

        public void RefreshForCurrentWorld()
        {
            if (TryGetCurrentSpec(out WorldGenerationResult result, out int rootId, out BigTopEnvironmentSpec spec)
                && NeedsRebuild(result, rootId, spec))
            {
                Rebuild(result, rootId, spec);
            }
        }

        public async UniTask RefreshForCurrentWorldAsync(WorldLoadTimingDiagnostics timing = null)
        {
            if (TryGetCurrentSpec(out WorldGenerationResult result, out int rootId, out BigTopEnvironmentSpec spec)
                && NeedsRebuild(result, rootId, spec))
            {
                await RebuildAsync(result, rootId, spec, timing);
            }
        }

        public void Clear()
        {
            if (view != null)
            {
                factory.DestroyView(view);
                view = null;
            }

            if (fallbackRoot != null)
            {
                factory.DestroyRoot(fallbackRoot);
                fallbackRoot = null;
            }

            lastWorldIndex = -1;
            lastSeed = 0;
            lastRootId = 0;
            lastUsedPrototype = false;
            lastSpec = default;
        }

        private void Rebuild(WorldGenerationResult result, int rootId, BigTopEnvironmentSpec spec)
        {
            if (view != null)
            {
                factory.DestroyView(view);
                view = null;
            }

            Transform parent = result.GeneratedRoot != null
                ? result.GeneratedRoot.transform
                : FallbackRoot().transform;
            view = factory.CreateView(parent, spec, worldConfig);
            lastWorldIndex = result.WorldIndex;
            lastSeed = result.Seed;
            lastRootId = rootId;
            lastUsedPrototype = result.UsedPrototypePlacement;
            lastSpec = spec;
        }

        private async UniTask RebuildAsync(
            WorldGenerationResult result,
            int rootId,
            BigTopEnvironmentSpec spec,
            WorldLoadTimingDiagnostics timing = null)
        {
            if (view != null)
            {
                factory.DestroyView(view);
                view = null;
            }

            Transform parent = result.GeneratedRoot != null
                ? result.GeneratedRoot.transform
                : FallbackRoot().transform;
            view = await factory.CreateViewAsync(parent, spec, worldConfig, timing);
            lastWorldIndex = result.WorldIndex;
            lastSeed = result.Seed;
            lastRootId = rootId;
            lastUsedPrototype = result.UsedPrototypePlacement;
            lastSpec = spec;
        }

        private bool TryGetCurrentSpec(
            out WorldGenerationResult result,
            out int rootId,
            out BigTopEnvironmentSpec spec)
        {
            result = default;
            rootId = 0;
            spec = default;
            if (worldConfig == null || !worldConfig.bigTopEnvironmentEnabled || generationState == null)
            {
                Clear();
                return false;
            }

            result = generationState.Current;
            if (result.WorldIndex < 1)
            {
                Clear();
                return false;
            }

            spec = BigTopEnvironmentRules.BuildSpec(
                BaseRadius(result),
                GeneratedMaxHeight(result),
                result.Seed,
                worldConfig);
            rootId = result.GeneratedRoot != null ? result.GeneratedRoot.GetInstanceID() : 0;
            return true;
        }

        private bool NeedsRebuild(WorldGenerationResult result, int rootId, BigTopEnvironmentSpec spec)
        {
            return view == null
                || lastWorldIndex != result.WorldIndex
                || lastSeed != result.Seed
                || lastRootId != rootId
                || lastUsedPrototype != result.UsedPrototypePlacement
                || !BigTopEnvironmentRules.SpecMatches(lastSpec, spec);
        }

        private float BaseRadius(WorldGenerationResult result)
        {
            if (!result.UsedPrototypePlacement && result.GeneratedMap != null)
            {
                return result.GeneratedMap.PlayableRadius;
            }

            return gameConfig != null ? gameConfig.arenaRadius : 58f;
        }

        private static float GeneratedMaxHeight(WorldGenerationResult result)
        {
            return result.GeneratedMap != null ? result.GeneratedMap.MaxHeight : 0f;
        }

        private GameObject FallbackRoot()
        {
            if (fallbackRoot != null)
            {
                return fallbackRoot;
            }

            fallbackRoot = factory.CreateFallbackRoot();
            return fallbackRoot;
        }
    }

}
