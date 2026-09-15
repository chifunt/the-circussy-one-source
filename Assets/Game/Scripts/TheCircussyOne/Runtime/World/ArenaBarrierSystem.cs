using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;
using TheCircussyOne.Visuals;
using UnityEngine;
using VContainer.Unity;

namespace TheCircussyOne.Runtime
{
    public sealed class ArenaBarrierSystem : ITickable, IRunWorldRefreshAdapter
    {
        private readonly RunWorldGenerationConfig worldConfig;
        private readonly GameConfig gameConfig;
        private readonly RunWorldGenerationState generationState;
        private readonly IGameTime time;
        private readonly ArenaBarrierFactory factory;

        private ArenaBarrierView view;
        private ArenaBarrierCollisionRuntime collision;
        private GameObject fallbackRoot;
        private int lastWorldIndex = -1;
        private int lastSeed;
        private int lastRootId;
        private bool lastUsedPrototype;
        private float lastRadius;
        private bool lastCollisionEnabled;
        private ArenaBarrierCollisionSpec lastCollisionSpec;

        public ArenaBarrierSystem(
            RunWorldGenerationConfig worldConfig,
            GameConfig gameConfig,
            RunWorldGenerationState generationState,
            IGameTime time,
            ArenaBarrierFactory factory = null)
        {
            this.worldConfig = worldConfig;
            this.gameConfig = gameConfig;
            this.generationState = generationState;
            this.time = time;
            this.factory = factory ?? new ArenaBarrierFactory();
        }

        public bool HasView => view != null && view.gameObject.activeInHierarchy;
        public bool HasCollision => collision?.Collider != null && collision.Collider.gameObject.activeInHierarchy;
        public float CurrentRadius => view != null ? view.Radius : 0f;
        public Collider CollisionCollider => collision?.Collider;
        public string DiagnosticsName => "ArenaBarrier";
        public string LoadingStage => "Setting the Boundary";

        public void Tick()
        {
            if (view == null)
            {
                view = null;
                return;
            }

            view.ApplyFrame(time != null ? time.Time : 0f, worldConfig);
        }

        public void RefreshForCurrentWorld()
        {
            if (TryGetCurrentSpec(
                    out WorldGenerationResult result,
                    out int rootId,
                    out float radius,
                    out bool collisionEnabled,
                    out ArenaBarrierCollisionSpec collisionSpec)
                && NeedsRebuild(result, rootId, radius, collisionEnabled, collisionSpec))
            {
                Rebuild(result, rootId, radius, collisionEnabled, collisionSpec);
            }
        }

        public UniTask RefreshForCurrentWorldAsync(WorldLoadTimingDiagnostics timing = null)
        {
            RefreshForCurrentWorld();
            return UniTask.CompletedTask;
        }

        public void Clear()
        {
            ClearView();

            ClearCollision();

            ClearFallbackRoot();

            lastWorldIndex = -1;
            lastSeed = 0;
            lastRootId = 0;
            lastUsedPrototype = false;
            lastRadius = 0f;
            lastCollisionEnabled = false;
            lastCollisionSpec = default;
        }

        private void Rebuild(
            WorldGenerationResult result,
            int rootId,
            float radius,
            bool collisionEnabled,
            ArenaBarrierCollisionSpec collisionSpec)
        {
            ClearView();

            ClearCollision();

            Transform parent = result.GeneratedRoot != null
                ? result.GeneratedRoot.transform
                : FallbackRoot().transform;
            view = factory.CreateView(parent, radius, worldConfig);

            if (collisionEnabled && collisionSpec.IsValid)
            {
                collision = factory.CreateCollision(parent, collisionSpec);
            }

            lastWorldIndex = result.WorldIndex;
            lastSeed = result.Seed;
            lastRootId = rootId;
            lastUsedPrototype = result.UsedPrototypePlacement;
            lastRadius = radius;
            lastCollisionEnabled = collisionEnabled;
            lastCollisionSpec = collisionSpec;
        }

        private bool TryGetCurrentSpec(
            out WorldGenerationResult result,
            out int rootId,
            out float radius,
            out bool collisionEnabled,
            out ArenaBarrierCollisionSpec collisionSpec)
        {
            result = default;
            rootId = 0;
            radius = 0f;
            collisionEnabled = false;
            collisionSpec = default;
            if (worldConfig == null || !worldConfig.arenaBarrierEnabled || generationState == null)
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

            radius = BarrierRadius(result);
            collisionEnabled = ArenaBarrierCollisionRules.ShouldCreateCollision(worldConfig);
            collisionSpec = ArenaBarrierCollisionRules.BuildSpec(
                radius,
                GeneratedMaxHeight(result),
                worldConfig);
            rootId = result.GeneratedRoot != null ? result.GeneratedRoot.GetInstanceID() : 0;
            return true;
        }

        private void ClearCollision()
        {
            factory.DestroyCollision(collision);
            collision = null;
        }

        private void ClearView()
        {
            if (view != null)
            {
                factory.DestroyView(view);
            }

            view = null;
        }

        private void ClearFallbackRoot()
        {
            if (fallbackRoot != null)
            {
                factory.DestroyRoot(fallbackRoot);
            }

            fallbackRoot = null;
        }

        private bool NeedsRebuild(
            WorldGenerationResult result,
            int rootId,
            float radius,
            bool collisionEnabled,
            ArenaBarrierCollisionSpec collisionSpec)
        {
            return view == null
                || lastWorldIndex != result.WorldIndex
                || lastSeed != result.Seed
                || lastRootId != rootId
                || lastUsedPrototype != result.UsedPrototypePlacement
                || !Mathf.Approximately(lastRadius, radius)
                || lastCollisionEnabled != collisionEnabled
                || !CollisionSpecMatches(lastCollisionSpec, collisionSpec);
        }

        private float BarrierRadius(WorldGenerationResult result)
        {
            float baseRadius = result.UsedPrototypePlacement || result.GeneratedMap == null
                ? gameConfig != null ? gameConfig.arenaRadius : 58f
                : result.GeneratedMap.PlayableRadius;
            return ArenaBarrierVisualRules.Radius(baseRadius, worldConfig);
        }

        private static float GeneratedMaxHeight(WorldGenerationResult result)
        {
            return result.GeneratedMap != null ? result.GeneratedMap.MaxHeight : 0f;
        }

        private static bool CollisionSpecMatches(
            ArenaBarrierCollisionSpec a,
            ArenaBarrierCollisionSpec b)
        {
            return Mathf.Approximately(a.InnerRadius, b.InnerRadius)
                && Mathf.Approximately(a.OuterRadius, b.OuterRadius)
                && Mathf.Approximately(a.BottomY, b.BottomY)
                && Mathf.Approximately(a.TopY, b.TopY)
                && a.SegmentCount == b.SegmentCount;
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

    public sealed class ArenaBarrierCollisionMarker : MonoBehaviour
    {
    }

    public static class ArenaBarrierCollisionMeshBuilder
    {
        private const float Tau = Mathf.PI * 2f;

        public static Mesh Build(ArenaBarrierCollisionSpec spec)
        {
            if (!spec.IsValid)
            {
                return new Mesh { name = "Arena Barrier Collision Mesh" };
            }

            int segmentCount = spec.SegmentCount;
            var vertices = new Vector3[segmentCount * 4];
            var triangles = new List<int>(segmentCount * 24);

            for (int i = 0; i < segmentCount; i++)
            {
                float angle = i * Tau / segmentCount;
                float x = Mathf.Cos(angle);
                float z = Mathf.Sin(angle);
                int baseIndex = i * 4;
                vertices[baseIndex] = new Vector3(x * spec.InnerRadius, spec.BottomY, z * spec.InnerRadius);
                vertices[baseIndex + 1] = new Vector3(x * spec.InnerRadius, spec.TopY, z * spec.InnerRadius);
                vertices[baseIndex + 2] = new Vector3(x * spec.OuterRadius, spec.BottomY, z * spec.OuterRadius);
                vertices[baseIndex + 3] = new Vector3(x * spec.OuterRadius, spec.TopY, z * spec.OuterRadius);
            }

            for (int i = 0; i < segmentCount; i++)
            {
                int next = (i + 1) % segmentCount;
                int currentBase = i * 4;
                int nextBase = next * 4;

                int innerBottom = currentBase;
                int innerTop = currentBase + 1;
                int outerBottom = currentBase + 2;
                int outerTop = currentBase + 3;
                int nextInnerBottom = nextBase;
                int nextInnerTop = nextBase + 1;
                int nextOuterBottom = nextBase + 2;
                int nextOuterTop = nextBase + 3;

                AddQuad(triangles, innerBottom, nextInnerBottom, nextInnerTop, innerTop);
                AddQuad(triangles, outerBottom, outerTop, nextOuterTop, nextOuterBottom);
                AddQuad(triangles, innerTop, nextInnerTop, nextOuterTop, outerTop);
                AddQuad(triangles, innerBottom, nextInnerBottom, nextOuterBottom, outerBottom);
            }

            var mesh = new Mesh
            {
                name = "Arena Barrier Collision Mesh",
                vertices = vertices,
                triangles = triangles.ToArray()
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void AddQuad(List<int> triangles, int a, int b, int c, int d)
        {
            triangles.Add(a);
            triangles.Add(b);
            triangles.Add(c);
            triangles.Add(a);
            triangles.Add(c);
            triangles.Add(d);
        }
    }
}
