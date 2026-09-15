using Shapes;
using UnityEngine;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;

namespace TheCircussyOne.Visuals
{
    public sealed class EnemySpawnIndicatorView : MonoBehaviour
    {
        [SerializeField] private Disc outerRing;
        [SerializeField] private Disc innerRing;
        [SerializeField] private Disc centerDisc;

        public bool IsActive => gameObject.activeInHierarchy;
        public float OuterRadius => outerRing != null ? outerRing.Radius : 0f;
        public float InnerRadius => innerRing != null ? innerRing.Radius : 0f;

        private void Awake()
        {
            EnsureShapes();
        }

        public void Prepare(EnemySpawnVisualConfig config, Vector3 position)
        {
            Prepare(config, position, Vector3.up);
        }

        public void Prepare(EnemySpawnVisualConfig config, Vector3 position, Vector3 surfaceNormal)
        {
            EnsureShapes();
            transform.position = position + Vector3.up * (config != null ? config.groundYOffset : 0.035f);
            transform.rotation = SurfaceRotation(surfaceNormal);
            transform.localScale = Vector3.one;
            gameObject.SetActive(true);
        }

        public void ApplyFrame(EnemySpawnIndicatorFrame frame)
        {
            EnsureShapes();
            ApplyDisc(outerRing, DiscType.Ring, frame.OuterRadius, outerRing != null ? outerRing.Thickness : 0.09f, frame.OuterColor);
            ApplyDisc(innerRing, DiscType.Ring, frame.InnerRadius, innerRing != null ? innerRing.Thickness : 0.06f, frame.InnerColor);
            ApplyDisc(centerDisc, DiscType.Disc, frame.CenterRadius, 0f, frame.CenterColor);
        }

        public void Deactivate()
        {
            gameObject.SetActive(false);
        }

        public void ConfigureShapeDefaults(EnemySpawnVisualConfig config)
        {
            EnsureShapes();
            ApplyDisc(outerRing, DiscType.Ring, config.radius, config.outerThickness, config.outerColor);
            ApplyDisc(innerRing, DiscType.Ring, config.innerStartRadius, config.innerThickness, config.innerColor);
            ApplyDisc(centerDisc, DiscType.Disc, config.radius * config.centerRadiusMultiplier, 0f, config.centerColor);
        }

        private void EnsureShapes()
        {
            outerRing ??= CreateDisc("Outer Ring", DiscType.Ring);
            innerRing ??= CreateDisc("Inner Ring", DiscType.Ring);
            centerDisc ??= CreateDisc("Center Pulse", DiscType.Disc);
        }

        private Disc CreateDisc(string discName, DiscType type)
        {
            var child = new GameObject(discName);
            child.transform.SetParent(transform, false);
            child.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            child.AddComponent<MeshFilter>();
            child.AddComponent<MeshRenderer>();
            var disc = child.AddComponent<Disc>();
            ApplyDisc(disc, type, 0f, 0f, Color.clear);
            return disc;
        }

        private static void ApplyDisc(Disc disc, DiscType type, float radius, float thickness, Color color)
        {
            if (disc == null)
            {
                return;
            }

            disc.Type = type;
            disc.Geometry = DiscGeometry.Flat2D;
            disc.RadiusSpace = ThicknessSpace.Meters;
            disc.ThicknessSpace = ThicknessSpace.Meters;
            disc.Radius = Mathf.Max(0f, radius);
            disc.Thickness = Mathf.Max(0f, thickness);
            disc.Color = color;
            disc.BlendMode = ShapesBlendMode.Transparent;
            disc.SortingOrder = 10;
        }

        private static Quaternion SurfaceRotation(Vector3 surfaceNormal)
        {
            return surfaceNormal.sqrMagnitude > 0.000001f
                ? Quaternion.FromToRotation(Vector3.up, surfaceNormal.normalized)
                : Quaternion.identity;
        }
    }
}
