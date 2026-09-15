using TheCircussyOne.Content;
using UnityEngine;

namespace TheCircussyOne.Visuals
{
    public sealed class MenuPerformerPreviewView : MonoBehaviour
    {
        [SerializeField] private Transform previewRoot;
        [SerializeField] private Material fallbackMaterial;
        [SerializeField] private Vector3 fallbackScale = new(1.1f, 1.75f, 1.1f);
        [SerializeField] private float rotationDegreesPerSecond = 10f;

        private readonly ActorAnimancerDriver animationDriver = new();
        private PerformerDefinition currentPerformer;
        private GameObject currentPreview;
        private Material fallbackMaterialInstance;

        public void Hide()
        {
            ClearPreview();
        }

        public void Show(PerformerDefinition performer)
        {
            ClearPreview();
            currentPerformer = performer;
            Transform root = previewRoot != null ? previewRoot : transform;
            if (performer != null && performer.worldPrefab != null)
            {
                currentPreview = Instantiate(performer.worldPrefab, root);
                currentPreview.name = $"{performer.DisplayName} Preview";
                ApplyModelTransform(currentPreview.transform, performer.modelTransform);
            }
            else
            {
                currentPreview = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                currentPreview.name = performer != null ? $"{performer.DisplayName} Preview Fallback" : "Performer Preview Fallback";
                currentPreview.transform.SetParent(root, worldPositionStays: false);
                currentPreview.transform.localPosition = Vector3.zero;
                currentPreview.transform.localRotation = Quaternion.identity;
                currentPreview.transform.localScale = fallbackScale;
                Collider collider = currentPreview.GetComponent<Collider>();
                if (collider != null)
                {
                    Destroy(collider);
                }

                Renderer renderer = currentPreview.GetComponentInChildren<Renderer>();
                if (renderer != null)
                {
                    fallbackMaterialInstance = fallbackMaterial != null
                        ? new Material(fallbackMaterial)
                        : new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                    Color color = performer != null ? performer.portraitColor : new Color(0.75f, 0.3f, 0.95f, 1f);
                    fallbackMaterialInstance.color = color;
                    renderer.sharedMaterial = fallbackMaterialInstance;
                }
            }

            animationDriver.Configure(currentPreview);
            animationDriver.TickPerformer(
                currentPerformer != null ? currentPerformer.animation : null,
                visuallyGrounded: true,
                jumpVisualActive: false,
                motionSpeed01: 0f,
                deltaTime: 0.001f);
            animationDriver.EvaluateCurrentPose();
        }

        private void Update()
        {
            Transform root = previewRoot != null ? previewRoot : transform;
            if (rotationDegreesPerSecond != 0f)
            {
                root.Rotate(Vector3.up, rotationDegreesPerSecond * Time.unscaledDeltaTime, Space.World);
            }

            animationDriver.TickPerformer(
                currentPerformer != null ? currentPerformer.animation : null,
                visuallyGrounded: true,
                jumpVisualActive: false,
                motionSpeed01: 0f,
                deltaTime: Time.unscaledDeltaTime);
        }

        private void OnDestroy()
        {
            ClearPreview();
        }

        private void ClearPreview()
        {
            animationDriver.Clear();
            if (currentPreview != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(currentPreview);
                }
                else
                {
                    DestroyImmediate(currentPreview);
                }
            }

            currentPreview = null;
            currentPerformer = null;
            if (fallbackMaterialInstance != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(fallbackMaterialInstance);
                }
                else
                {
                    DestroyImmediate(fallbackMaterialInstance);
                }
            }

            fallbackMaterialInstance = null;
        }

        private static void ApplyModelTransform(Transform target, ActorModelTransformProfile profile)
        {
            if (target == null)
            {
                return;
            }

            if (profile == null)
            {
                target.localPosition = Vector3.zero;
                target.localRotation = Quaternion.identity;
                target.localScale = Vector3.one;
                return;
            }

            target.localPosition = profile.localPosition;
            target.localRotation = Quaternion.Euler(profile.localEulerAngles);
            target.localScale = profile.localScale;
        }
    }
}
