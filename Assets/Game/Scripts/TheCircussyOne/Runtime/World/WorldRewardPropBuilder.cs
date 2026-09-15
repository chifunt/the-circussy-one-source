using TheCircussyOne.Content;
using TheCircussyOne.Visuals;
using UnityEngine;

namespace TheCircussyOne.Runtime
{
    public readonly struct WorldRewardPropBuildResult
    {
        public WorldRewardPropBuildResult(
            GameObject root,
            GameObject visualRoot,
            GameObject body,
            Renderer bodyRenderer,
            WorldPropPlacementResult placement)
        {
            Root = root;
            VisualRoot = visualRoot;
            Body = body;
            BodyRenderer = bodyRenderer;
            Placement = placement;
        }

        public GameObject Root { get; }
        public GameObject VisualRoot { get; }
        public GameObject Body { get; }
        public Renderer BodyRenderer { get; }
        public WorldPropPlacementResult Placement { get; }
    }

    public enum WorldRewardPropColliderMode
    {
        Box = 0,
        Mesh = 1,
        Authored = 2
    }

    public static class WorldRewardPropBuilder
    {
        public static WorldRewardPropBuildResult Create(
            string displayName,
            Vector3 requestedPosition,
            PrimitiveType bodyPrimitive,
            string bodyName,
            WorldPropPlacementProfile placement,
            WorldPropPlacementService placementService,
            Material material,
            float yawDegrees = 0f)
        {
            WorldPropPlacementResult placementResult = ResolvePlacement(requestedPosition, placement, placementService);
            Quaternion rootRotation = ApplyYaw(placementResult.Rotation, placementResult.GroundNormal, yawDegrees);
            placementResult = WithRotation(placementResult, rootRotation);
            GameObject root = new(string.IsNullOrWhiteSpace(displayName) ? "World Reward Prop" : displayName);
            root.transform.SetPositionAndRotation(placementResult.Position, rootRotation);

            GameObject visualRoot = new(VisualGroundAnchorUtility.VisualRootName);
            visualRoot.transform.SetParent(root.transform, worldPositionStays: false);

            GameObject body = GameObject.CreatePrimitive(bodyPrimitive);
            body.name = string.IsNullOrWhiteSpace(bodyName) ? "Visual" : bodyName;
            body.transform.SetParent(visualRoot.transform, worldPositionStays: false);
            body.transform.localScale = placement.bodyColliderSize;

            Renderer renderer = body.GetComponent<Renderer>();
            if (renderer != null && material != null)
            {
                renderer.sharedMaterial = material;
            }

            WorldPropColliderDriver.Apply(root, placement);
            return new WorldRewardPropBuildResult(root, visualRoot, body, renderer, placementResult);
        }

        public static WorldRewardPropBuildResult Create(
            string displayName,
            Vector3 requestedPosition,
            GameObject visualPrefab,
            float visualScale,
            WorldPropPlacementProfile placement,
            WorldPropPlacementService placementService,
            Material fallbackMaterial,
            PrimitiveType fallbackPrimitive,
            string fallbackBodyName,
            WorldRewardPropColliderMode colliderMode,
            float yawDegrees = 0f)
        {
            if (visualPrefab == null)
            {
                return Create(
                    displayName,
                    requestedPosition,
                    fallbackPrimitive,
                    fallbackBodyName,
                    placement,
                    placementService,
                    fallbackMaterial,
                    yawDegrees);
            }

            WorldPropPlacementResult placementResult = ResolvePlacement(requestedPosition, placement, placementService);
            placementResult = GroundAuthoredPrefabRoot(requestedPosition, placementResult, placement);
            Quaternion rootRotation = ApplyYaw(placementResult.Rotation, placementResult.GroundNormal, yawDegrees);
            placementResult = WithRotation(placementResult, rootRotation);
            GameObject root = new(string.IsNullOrWhiteSpace(displayName) ? "World Reward Prop" : displayName);
            root.transform.SetPositionAndRotation(placementResult.Position, rootRotation);

            GameObject visualRoot = new(VisualGroundAnchorUtility.VisualRootName);
            visualRoot.transform.SetParent(root.transform, worldPositionStays: false);

            GameObject body = RuntimeObjectFactory.Instantiate(visualPrefab, visualRoot.transform);
            body.name = string.IsNullOrWhiteSpace(fallbackBodyName) ? visualPrefab.name : fallbackBodyName;
            body.transform.localPosition = Vector3.zero;
            body.transform.localRotation = Quaternion.identity;
            body.transform.localScale = Vector3.one * Mathf.Max(0.05f, visualScale);

            Renderer renderer = FirstRenderer(body);
            WorldPropColliderDriver.Apply(root, placement, colliderMode, body);
            return new WorldRewardPropBuildResult(root, visualRoot, body, renderer, placementResult);
        }

        private static Quaternion ApplyYaw(Quaternion surfaceRotation, Vector3 groundNormal, float yawDegrees)
        {
            if (Mathf.Abs(yawDegrees) <= 0.001f)
            {
                return surfaceRotation;
            }

            Vector3 safeNormal = groundNormal.sqrMagnitude > 0.000001f ? groundNormal.normalized : Vector3.up;
            return Quaternion.AngleAxis(yawDegrees, safeNormal) * surfaceRotation;
        }

        private static WorldPropPlacementResult WithRotation(WorldPropPlacementResult placement, Quaternion rotation)
        {
            return new WorldPropPlacementResult(
                placement.Position,
                placement.GroundNormal,
                rotation,
                placement.FoundSurface,
                placement.Attempt);
        }

        private static WorldPropPlacementResult GroundAuthoredPrefabRoot(
            Vector3 requestedPosition,
            WorldPropPlacementResult placement,
            WorldPropPlacementProfile profile)
        {
            Vector3 groundedPosition = placement.FoundSurface
                ? placement.Position - placement.GroundNormal * profile.RootHeightOffset
                : requestedPosition;
            return new WorldPropPlacementResult(
                groundedPosition,
                placement.GroundNormal,
                placement.Rotation,
                placement.FoundSurface,
                placement.Attempt);
        }

        private static WorldPropPlacementResult ResolvePlacement(
            Vector3 requestedPosition,
            WorldPropPlacementProfile placement,
            WorldPropPlacementService placementService)
        {
            if (placementService != null)
            {
                placementService.TryPlace(requestedPosition, placement, out WorldPropPlacementResult placementResult);
                return placementResult;
            }

            return new WorldPropPlacementResult(requestedPosition, Vector3.up, Quaternion.identity, foundSurface: false, attempt: 0);
        }

        private static Renderer FirstRenderer(GameObject root)
        {
            if (root == null)
            {
                return null;
            }

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(includeInactive: true);
            return renderers.Length > 0 ? renderers[0] : null;
        }
    }
}
