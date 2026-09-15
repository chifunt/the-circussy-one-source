using Shapes;
using UnityEngine;
using UnityEngine.Rendering;
using TheCircussyOne.Content;

namespace TheCircussyOne.Visuals
{
    public sealed class OrbitWeaponView : MonoBehaviour
    {
        [SerializeField] private Disc hoopRing;
        [SerializeField] private Sphere glowSphere;

        public bool IsActive => gameObject.activeInHierarchy;
        public float RingRadius => hoopRing != null ? hoopRing.Radius : 0f;
        public float GlowRadius => glowSphere != null ? glowSphere.Radius : 0f;

        private void Awake()
        {
            ResolveComponents();
        }

        public void ConfigureDefaults(WeaponDefinition weapon)
        {
            ResolveComponents();
            name = "Orbit Weapon View";
            ApplyFrame(Vector3.zero, 0.55f, weapon, 0f);
            gameObject.SetActive(false);
        }

        public void ApplyFrame(Vector3 position, float hitRadius, WeaponDefinition weapon, float angleDegrees)
        {
            ApplyFrame(position, hitRadius, weapon, Quaternion.Euler(0f, angleDegrees, 0f));
        }

        public void ApplyFrame(Vector3 position, float hitRadius, WeaponDefinition weapon, Quaternion rotation)
        {
            ResolveComponents();
            transform.position = position;
            transform.rotation = rotation;
            gameObject.SetActive(true);

            float radius = Mathf.Max(0.05f, hitRadius);
            Color primary = weapon != null ? weapon.projectilePrimaryColor : new Color(1f, 0.22f, 0.08f, 1f);
            Color emission = weapon != null ? weapon.projectileEmissionColor : new Color(1f, 0.52f, 0.09f, 1f);

            if (hoopRing != null)
            {
                hoopRing.Type = DiscType.Ring;
                hoopRing.Geometry = DiscGeometry.Flat2D;
                hoopRing.RadiusSpace = ThicknessSpace.Meters;
                hoopRing.ThicknessSpace = ThicknessSpace.Meters;
                hoopRing.BlendMode = ShapesBlendMode.Transparent;
                hoopRing.SortingOrder = 68;
                hoopRing.Radius = radius;
                hoopRing.Thickness = Mathf.Max(0.025f, radius * 0.12f);
                hoopRing.Color = new Color(primary.r, primary.g, primary.b, Mathf.Clamp01(primary.a));
                hoopRing.transform.SetParent(transform, false);
                hoopRing.transform.localPosition = Vector3.zero;
                hoopRing.transform.localRotation = Quaternion.identity;
                hoopRing.transform.localScale = Vector3.one;
                hoopRing.gameObject.SetActive(true);
            }

            if (glowSphere != null)
            {
                glowSphere.RadiusSpace = ThicknessSpace.Meters;
                glowSphere.BlendMode = ShapesBlendMode.Transparent;
                glowSphere.SortingOrder = 67;
                glowSphere.Radius = radius * 0.82f;
                glowSphere.Color = new Color(emission.r, emission.g, emission.b, 0.16f);
                glowSphere.transform.SetParent(transform, false);
                glowSphere.transform.localPosition = Vector3.zero;
                glowSphere.transform.localRotation = Quaternion.identity;
                glowSphere.transform.localScale = Vector3.one;
                glowSphere.gameObject.SetActive(true);
            }

            ConfigureRenderers();
        }

        public void Deactivate()
        {
            if (this == null)
            {
                return;
            }

            gameObject.SetActive(false);
        }

        private void ResolveComponents()
        {
            if (hoopRing == null)
            {
                hoopRing = GetComponentInChildren<Disc>(includeInactive: true);
            }

            if (glowSphere == null)
            {
                glowSphere = GetComponentInChildren<Sphere>(includeInactive: true);
            }

            if (hoopRing == null)
            {
                hoopRing = CreateShapeChild<Disc>("Hoop Ring");
            }

            if (glowSphere == null)
            {
                glowSphere = CreateShapeChild<Sphere>("Hoop Glow");
            }

            if (hoopRing != null)
            {
                hoopRing.name = "Hoop Ring Shape";
            }

            if (glowSphere != null)
            {
                glowSphere.name = "Hoop Glow Shape";
            }
        }

        private void ConfigureRenderers()
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>(includeInactive: true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                renderer.lightProbeUsage = LightProbeUsage.Off;
                renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            }
        }

        private T CreateShapeChild<T>(string childName) where T : ShapeRenderer
        {
            var child = new GameObject(childName);
            child.transform.SetParent(transform, false);
            child.AddComponent<MeshFilter>();
            child.AddComponent<MeshRenderer>();
            var shape = child.AddComponent<T>();
            shape.name = $"{childName} Shape";
            return shape;
        }
    }
}
