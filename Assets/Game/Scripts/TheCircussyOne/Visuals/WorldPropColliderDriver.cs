using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using TheCircussyOne.Runtime;
using UnityEngine;

namespace TheCircussyOne.Visuals
{
    public static class WorldPropColliderDriver
    {
        public static void Apply(GameObject root, WorldPropPlacementProfile profile)
        {
            Apply(root, profile, WorldRewardPropColliderMode.Box, meshSource: null);
        }

        public static void Apply(
            GameObject root,
            WorldPropPlacementProfile profile,
            WorldRewardPropColliderMode colliderMode,
            GameObject meshSource)
        {
            if (root == null)
            {
                return;
            }

            int pickupLayer = GameLayers.PickupIndex;
            if (pickupLayer >= 0)
            {
                SetLayerRecursively(root, pickupLayer);
            }

            if (!WorldPropPlacementRules.HasValidBlockingBody(profile))
            {
                DisableColliders(root);
                return;
            }

            if (colliderMode == WorldRewardPropColliderMode.Authored && TryApplyAuthoredColliders(meshSource))
            {
                DisableRootColliders(root);
                return;
            }

            DisableChildColliders(root);
            if (colliderMode == WorldRewardPropColliderMode.Mesh && TryApplyMeshCollider(root, meshSource))
            {
                return;
            }

            DisableNonBoxRootColliders(root);
            BoxCollider box = root.GetComponent<BoxCollider>();
            if (box == null)
            {
                box = root.AddComponent<BoxCollider>();
            }

            Vector3 scale = root.transform.lossyScale;
            box.enabled = true;
            box.isTrigger = false;
            box.center = Divide(profile.bodyColliderCenter, scale);
            box.size = Divide(profile.bodyColliderSize, scale);
        }

        private static bool TryApplyAuthoredColliders(GameObject meshSource)
        {
            Collider[] colliders = Colliders(meshSource);
            if (colliders.Length == 0)
            {
                return false;
            }

            bool appliedAny = false;
            for (int i = 0; i < colliders.Length; i++)
            {
                Collider collider = colliders[i];
                if (collider == null)
                {
                    continue;
                }

                collider.enabled = true;
                collider.isTrigger = false;
                appliedAny = true;
            }

            return appliedAny;
        }

        private static bool TryApplyMeshCollider(GameObject root, GameObject meshSource)
        {
            MeshFilter[] meshFilters = MeshFilters(meshSource);
            if (meshFilters.Length == 0)
            {
                return false;
            }

            DisableNonMeshRootColliders(root);
            bool appliedAny = false;
            for (int i = 0; i < meshFilters.Length; i++)
            {
                MeshFilter meshFilter = meshFilters[i];
                if (meshFilter == null || meshFilter.sharedMesh == null)
                {
                    continue;
                }

                MeshCollider meshCollider = meshFilter.GetComponent<MeshCollider>();
                if (meshCollider == null)
                {
                    meshCollider = meshFilter.gameObject.AddComponent<MeshCollider>();
                }

                meshCollider.enabled = true;
                meshCollider.isTrigger = false;
                meshCollider.convex = true;
                meshCollider.sharedMesh = meshFilter.sharedMesh;
                meshCollider.sharedMaterial = null;
                appliedAny = true;
            }

            return appliedAny;
        }

        private static void DisableRootColliders(GameObject root)
        {
            Collider[] colliders = root.GetComponents<Collider>();
            for (int i = 0; i < colliders.Length; i++)
            {
                Collider collider = colliders[i];
                if (collider != null)
                {
                    collider.enabled = false;
                }
            }
        }

        private static void DisableNonBoxRootColliders(GameObject root)
        {
            Collider[] colliders = root.GetComponents<Collider>();
            for (int i = 0; i < colliders.Length; i++)
            {
                Collider collider = colliders[i];
                if (collider != null && collider is not BoxCollider)
                {
                    collider.enabled = false;
                }
            }
        }

        private static void DisableNonMeshRootColliders(GameObject root)
        {
            Collider[] colliders = root.GetComponents<Collider>();
            for (int i = 0; i < colliders.Length; i++)
            {
                Collider collider = colliders[i];
                if (collider != null && collider is not MeshCollider)
                {
                    collider.enabled = false;
                }
            }
        }

        private static MeshFilter[] MeshFilters(GameObject root)
        {
            if (root == null)
            {
                return System.Array.Empty<MeshFilter>();
            }

            return root.GetComponentsInChildren<MeshFilter>(includeInactive: true);
        }

        private static Collider[] Colliders(GameObject root)
        {
            if (root == null)
            {
                return System.Array.Empty<Collider>();
            }

            return root.GetComponentsInChildren<Collider>(includeInactive: true);
        }

        private static void DisableChildColliders(GameObject root)
        {
            Collider[] colliders = root.GetComponentsInChildren<Collider>(includeInactive: true);
            for (int i = 0; i < colliders.Length; i++)
            {
                Collider collider = colliders[i];
                if (collider != null && collider.gameObject != root)
                {
                    collider.enabled = false;
                }
            }
        }

        private static void DisableColliders(GameObject root)
        {
            Collider[] colliders = root.GetComponentsInChildren<Collider>(includeInactive: true);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null)
                {
                    colliders[i].enabled = false;
                }
            }
        }

        private static void SetLayerRecursively(GameObject root, int layer)
        {
            root.layer = layer;
            Transform transform = root.transform;
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (child != null)
                {
                    SetLayerRecursively(child.gameObject, layer);
                }
            }
        }

        private static Vector3 Divide(Vector3 value, Vector3 scale)
        {
            return new Vector3(
                Divide(value.x, scale.x),
                Divide(value.y, scale.y),
                Divide(value.z, scale.z));
        }

        private static float Divide(float value, float divisor)
        {
            return Mathf.Abs(divisor) > 0.0001f ? value / divisor : value;
        }
    }
}
