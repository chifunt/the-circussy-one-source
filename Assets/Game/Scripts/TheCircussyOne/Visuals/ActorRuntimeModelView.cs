using System;
using UnityEngine;
using TheCircussyOne.Content;
using Object = UnityEngine.Object;

namespace TheCircussyOne.Visuals
{
    internal sealed class ActorRuntimeModelView
    {
        private const string RuntimeModelName = "Runtime Model";

        private GameObject instance;
        private GameObject prefab;
        private Renderer[] fallbackRenderers;
        private Renderer[] modelRenderers;

        public GameObject Instance => instance;
        public Renderer[] ModelRenderers => modelRenderers;
        public bool HasModel => instance != null;
        public bool HasVisibleFallbackRenderer
        {
            get
            {
                if (fallbackRenderers == null)
                {
                    return false;
                }

                for (int i = 0; i < fallbackRenderers.Length; i++)
                {
                    Renderer renderer = fallbackRenderers[i];
                    if (renderer != null && renderer.enabled && renderer.gameObject.activeInHierarchy)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public void Apply(
            Transform parent,
            GameObject modelPrefab,
            ActorModelTransformProfile transformProfile,
            Transform fallbackBody)
        {
            CaptureFallbackRenderers(fallbackBody);
            if (parent == null || modelPrefab == null)
            {
                Clear();
                SetFallbackVisible(true);
                return;
            }

            if (instance == null || prefab != modelPrefab)
            {
                ClearInstance();
                prefab = modelPrefab;
                instance = InstantiateModel(modelPrefab, parent);
                if (instance == null)
                {
                    prefab = null;
                    modelRenderers = null;
                    SetFallbackVisible(true);
                    return;
                }

                instance.name = RuntimeModelName;
                SetLayerRecursively(instance, parent.gameObject.layer);
                DisableModelColliders(instance);
            }
            else if (instance.transform.parent != parent)
            {
                instance.transform.SetParent(parent, false);
            }

            ApplyTransform(transformProfile);
            modelRenderers = instance.GetComponentsInChildren<Renderer>(includeInactive: true);
            SetFallbackVisible(false);
        }

        public void Clear()
        {
            ClearInstance();
            prefab = null;
            modelRenderers = null;
            SetFallbackVisible(true);
        }

        private void ClearInstance()
        {
            if (instance == null)
            {
                return;
            }

            GameObject oldInstance = instance;
            instance = null;
            if (Application.isPlaying)
            {
                Object.Destroy(oldInstance);
            }
            else
            {
                Object.DestroyImmediate(oldInstance);
            }
        }

        private void ApplyTransform(ActorModelTransformProfile transformProfile)
        {
            if (instance == null)
            {
                return;
            }

            Transform instanceTransform = instance.transform;
            if (transformProfile == null)
            {
                instanceTransform.localPosition = Vector3.zero;
                instanceTransform.localRotation = Quaternion.identity;
                instanceTransform.localScale = Vector3.one;
                return;
            }

            transformProfile.EnsureWorkflowDefaults();
            instanceTransform.localPosition = transformProfile.localPosition;
            instanceTransform.localRotation = Quaternion.Euler(transformProfile.localEulerAngles);
            instanceTransform.localScale = transformProfile.localScale;
        }

        private void CaptureFallbackRenderers(Transform fallbackBody)
        {
            if (fallbackRenderers != null || fallbackBody == null)
            {
                return;
            }

            fallbackRenderers = fallbackBody.GetComponentsInChildren<Renderer>(includeInactive: true);
        }

        private void SetFallbackVisible(bool visible)
        {
            if (fallbackRenderers == null)
            {
                return;
            }

            for (int i = 0; i < fallbackRenderers.Length; i++)
            {
                if (fallbackRenderers[i] != null)
                {
                    fallbackRenderers[i].enabled = visible;
                }
            }
        }

        private static void DisableModelColliders(GameObject model)
        {
            Collider[] colliders = model.GetComponentsInChildren<Collider>(includeInactive: true);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null)
                {
                    colliders[i].enabled = false;
                }
            }
        }

        private static GameObject InstantiateModel(GameObject modelPrefab, Transform parent)
        {
            try
            {
                Object clone = Object.Instantiate((Object)modelPrefab, parent);
                if (clone is GameObject model)
                {
                    return model;
                }

                DestroyObject(clone);
            }
            catch (InvalidCastException exception)
            {
                Debug.LogWarning($"Actor model prefab '{modelPrefab.name}' could not be instantiated as a GameObject. Falling back to the actor body. {exception.Message}");
            }

            return null;
        }

        private static void DestroyObject(Object value)
        {
            if (value == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Object.Destroy(value);
            }
            else
            {
                Object.DestroyImmediate(value);
            }
        }

        private static void SetLayerRecursively(GameObject root, int layer)
        {
            if (root == null)
            {
                return;
            }

            root.layer = layer;
            Transform rootTransform = root.transform;
            for (int i = 0; i < rootTransform.childCount; i++)
            {
                SetLayerRecursively(rootTransform.GetChild(i).gameObject, layer);
            }
        }
    }
}
