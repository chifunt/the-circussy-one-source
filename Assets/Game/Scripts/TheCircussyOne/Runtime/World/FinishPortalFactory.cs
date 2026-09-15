using TheCircussyOne.Visuals;
using UnityEngine;

namespace TheCircussyOne.Runtime
{
    public sealed class FinishPortalFactory
    {
        public FinishPortalView CreatePortal()
        {
            var root = new GameObject("Stage Door");
            Transform visualRoot = new GameObject(VisualGroundAnchorUtility.VisualRootName).transform;
            visualRoot.SetParent(root.transform, false);

            Material doorMaterial = CreateMaterial("Stage Door Panel", new Color(0.36f, 0.035f, 0.05f, 1f), new Color(0.75f, 0.05f, 0.08f, 1f));
            Material frameMaterial = CreateMaterial("Stage Door Frame", new Color(0.86f, 0.56f, 0.16f, 1f), new Color(1f, 0.58f, 0.14f, 1f));
            Material thresholdMaterial = CreateMaterial("Stage Door Glow", new Color(0.08f, 0.42f, 1f, 1f), new Color(0.18f, 0.72f, 1f, 1f));

            CreatePart(visualRoot, "Door Panel", PrimitiveType.Cube, new Vector3(0f, 1.35f, 0f), new Vector3(1.55f, 2.65f, 0.16f), doorMaterial);
            CreatePart(visualRoot, "Left Door Frame", PrimitiveType.Cube, new Vector3(-0.9f, 1.45f, 0f), new Vector3(0.18f, 2.9f, 0.24f), frameMaterial);
            CreatePart(visualRoot, "Right Door Frame", PrimitiveType.Cube, new Vector3(0.9f, 1.45f, 0f), new Vector3(0.18f, 2.9f, 0.24f), frameMaterial);
            CreatePart(visualRoot, "Top Door Frame", PrimitiveType.Cube, new Vector3(0f, 2.85f, 0f), new Vector3(1.98f, 0.22f, 0.24f), frameMaterial);
            CreatePart(visualRoot, "Door Knob", PrimitiveType.Sphere, new Vector3(0.52f, 1.25f, -0.12f), new Vector3(0.16f, 0.16f, 0.16f), frameMaterial);
            CreatePart(visualRoot, "Glowing Threshold", PrimitiveType.Cylinder, new Vector3(0f, 0.025f, 0f), new Vector3(1.18f, 0.025f, 1.18f), thresholdMaterial);

            return root.AddComponent<FinishPortalView>();
        }

        public void Release(FinishPortalView portal)
        {
            if (portal == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Object.Destroy(portal.gameObject);
            }
            else
            {
                Object.DestroyImmediate(portal.gameObject);
            }
        }

        private static void CreatePart(
            Transform parent,
            string name,
            PrimitiveType primitiveType,
            Vector3 localPosition,
            Vector3 localScale,
            Material material)
        {
            GameObject part = GameObject.CreatePrimitive(primitiveType);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = localScale;

            Collider collider = part.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }

            Renderer renderer = part.GetComponent<Renderer>();
            if (renderer != null && material != null)
            {
                renderer.sharedMaterial = material;
            }
        }

        private static Material CreateMaterial(string name, Color color, Color emission)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Sprites/Default");
            var material = new Material(shader)
            {
                name = name,
                color = color
            };
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", emission);
            return material;
        }
    }
}
