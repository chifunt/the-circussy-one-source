using UnityEngine;

namespace TheCircussyOne.Visuals
{
    public static class VisualGroundAnchorUtility
    {
        public const string VisualRootName = "VisualRoot";

        public static Transform ResolveVisualRoot(Transform owner, Renderer[] renderers)
        {
            if (owner == null)
            {
                return null;
            }

            Transform namedRoot = owner.Find(VisualRootName);
            if (namedRoot != null)
            {
                return namedRoot;
            }

            if (renderers != null)
            {
                for (int i = 0; i < renderers.Length; i++)
                {
                    Renderer renderer = renderers[i];
                    if (renderer != null && renderer.transform == owner)
                    {
                        return owner;
                    }
                }

                for (int i = 0; i < renderers.Length; i++)
                {
                    Renderer renderer = renderers[i];
                    if (renderer != null)
                    {
                        return renderer.transform;
                    }
                }
            }

            return owner;
        }

        public static float ResolveRootToBottomLocalDistance(Transform root, Renderer[] renderers)
        {
            if (root == null || renderers == null || renderers.Length == 0)
            {
                return 0f;
            }

            Matrix4x4 worldToRoot = root.worldToLocalMatrix;
            bool hasBounds = false;
            float minY = 0f;
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                Bounds bounds = renderer.localBounds;
                Matrix4x4 localToRoot = worldToRoot * renderer.transform.localToWorldMatrix;
                Vector3 center = bounds.center;
                Vector3 extents = bounds.extents;
                for (int corner = 0; corner < 8; corner++)
                {
                    Vector3 local = center + new Vector3(
                        (corner & 1) == 0 ? -extents.x : extents.x,
                        (corner & 2) == 0 ? -extents.y : extents.y,
                        (corner & 4) == 0 ? -extents.z : extents.z);
                    float y = localToRoot.MultiplyPoint3x4(local).y;
                    if (!hasBounds)
                    {
                        minY = y;
                        hasBounds = true;
                    }
                    else
                    {
                        minY = Mathf.Min(minY, y);
                    }
                }
            }

            return hasBounds ? Mathf.Max(0f, -minY) : 0f;
        }

        public static Vector3 BottomAnchoredLocalPosition(
            Transform root,
            Vector3 baseLocalPosition,
            Vector3 baseScale,
            Vector3 appliedScale,
            float rootToBottomLocalDistance)
        {
            if (root == null || rootToBottomLocalDistance <= 0f)
            {
                return baseLocalPosition;
            }

            Vector3 upInParent = root.parent != null
                ? root.parent.InverseTransformDirection(root.up)
                : root.up;
            float yDelta = rootToBottomLocalDistance * (appliedScale.y - baseScale.y);
            return baseLocalPosition + upInParent * yDelta;
        }
    }
}
