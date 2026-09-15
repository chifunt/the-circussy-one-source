using TheCircussyOne.Config;
using TheCircussyOne.Rules;
using TheCircussyOne.Visuals;
using UnityEngine;

namespace TheCircussyOne.Runtime
{
    public sealed class ArenaBarrierFactory
    {
        public GameObject CreateFallbackRoot()
        {
            return new GameObject("Arena Barrier Runtime Root");
        }

        public ArenaBarrierView CreateView(Transform parent, float radius, RunWorldGenerationConfig config)
        {
            var viewObject = new GameObject("Arena Barrier Visual");
            viewObject.transform.SetParent(parent, worldPositionStays: false);
            viewObject.transform.localPosition = Vector3.zero;
            viewObject.transform.localRotation = Quaternion.identity;
            viewObject.transform.localScale = Vector3.one;
            ArenaBarrierView view = viewObject.AddComponent<ArenaBarrierView>();
            view.Configure(radius, config);
            return view;
        }

        public ArenaBarrierCollisionRuntime CreateCollision(Transform parent, ArenaBarrierCollisionSpec collisionSpec)
        {
            var collisionObject = new GameObject("Arena Barrier Collision");
            collisionObject.transform.SetParent(parent, worldPositionStays: false);
            collisionObject.transform.localPosition = Vector3.zero;
            collisionObject.transform.localRotation = Quaternion.identity;
            collisionObject.transform.localScale = Vector3.one;
            Mesh collisionMesh = ArenaBarrierCollisionMeshBuilder.Build(collisionSpec);
            MeshCollider collisionCollider = collisionObject.AddComponent<MeshCollider>();
            collisionCollider.sharedMesh = collisionMesh;
            collisionObject.AddComponent<ArenaBarrierCollisionMarker>();
            return new ArenaBarrierCollisionRuntime(collisionObject, collisionCollider, collisionMesh);
        }

        public void DestroyView(ArenaBarrierView view)
        {
            DestroyObject(view != null ? view.gameObject : null);
        }

        public void DestroyCollision(ArenaBarrierCollisionRuntime collision)
        {
            if (collision == null)
            {
                return;
            }

            DestroyObject(collision.GameObject);
            DestroyObject(collision.Mesh);
        }

        public void DestroyRoot(GameObject root)
        {
            DestroyObject(root);
        }

        private static void DestroyObject(Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Object.Destroy(target);
                return;
            }

            Object.DestroyImmediate(target);
        }
    }

    public sealed class ArenaBarrierCollisionRuntime
    {
        public ArenaBarrierCollisionRuntime(GameObject gameObject, MeshCollider collider, Mesh mesh)
        {
            GameObject = gameObject;
            Collider = collider;
            Mesh = mesh;
        }

        public GameObject GameObject { get; }
        public MeshCollider Collider { get; }
        public Mesh Mesh { get; }
    }
}
