using Shapes;
using UnityEngine;
using UnityEngine.Rendering;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;

namespace TheCircussyOne.Visuals
{
    public sealed class ProjectileExplosionView : MonoBehaviour
    {
        [SerializeField] private Disc radiusRing;
        [SerializeField] private Sphere radiusSphere;
        [SerializeField] private ParticleEffectView particleEffect;

        public bool IsActive => gameObject.activeInHierarchy;
        public float RingRadius => radiusRing != null ? radiusRing.Radius : 0f;
        public float SphereRadius => radiusSphere != null ? radiusSphere.Radius : 0f;
        public ParticleEffectView ParticleEffect => particleEffect;

        private void Awake()
        {
            ResolveComponents();
        }

        public void ConfigureDefaults(VfxVisualConfig config)
        {
            ResolveComponents();
            name = "Projectile Explosion View";
            ApplyShapeDefaults(config);
            ApplyFrame(ProjectileExplosionVisualRules.Evaluate(0f, 1f, config), config);
        }

        public void PlayAt(Vector3 position, Quaternion rotation, VfxVisualConfig config)
        {
            ResolveComponents();
            transform.SetPositionAndRotation(position, rotation);
            gameObject.SetActive(true);
            ApplyShapeDefaults(config);

            if (particleEffect != null && config != null && config.projectileExplosion != null && config.projectileExplosion.enabled)
            {
                particleEffect.PlayAt(position, rotation);
            }
        }

        public void ApplyFrame(ProjectileExplosionVisualFrame frame, VfxVisualConfig config)
        {
            ResolveComponents();
            ApplyShapeDefaults(config);

            bool showOverlay = frame.Visible && config != null && config.projectileExplosionOverlayEnabled && frame.Radius > 0f;
            if (radiusRing != null)
            {
                radiusRing.gameObject.SetActive(showOverlay);
                radiusRing.Radius = frame.Radius;
                radiusRing.Thickness = Mathf.Max(0.001f, config != null ? config.projectileExplosionOverlayRingThickness : 0.06f);
                radiusRing.Color = frame.RingColor;
            }

            if (radiusSphere != null)
            {
                radiusSphere.gameObject.SetActive(showOverlay);
                radiusSphere.Radius = frame.Radius;
                radiusSphere.Color = frame.SphereColor;
            }
        }

        public void Deactivate()
        {
            if (particleEffect != null)
            {
                particleEffect.Deactivate();
            }

            gameObject.SetActive(false);
        }

        private void ResolveComponents()
        {
            if (particleEffect == null)
            {
                particleEffect = GetComponentInChildren<ParticleEffectView>(includeInactive: true);
            }

            if (radiusRing == null)
            {
                radiusRing = GetComponentInChildren<Disc>(includeInactive: true);
            }

            if (radiusSphere == null)
            {
                radiusSphere = GetComponentInChildren<Sphere>(includeInactive: true);
            }

            if (radiusRing == null)
            {
                radiusRing = CreateShapeChild<Disc>("Radius Ring");
            }

            if (radiusSphere == null)
            {
                radiusSphere = CreateShapeChild<Sphere>("Radius Sphere");
            }

            if (radiusRing != null)
            {
                radiusRing.name = "Radius Ring Shape";
            }

            if (radiusSphere != null)
            {
                radiusSphere.name = "Radius Sphere Shape";
            }
        }

        private void ApplyShapeDefaults(VfxVisualConfig config)
        {
            if (radiusRing != null)
            {
                radiusRing.Type = DiscType.Ring;
                radiusRing.Geometry = DiscGeometry.Flat2D;
                radiusRing.RadiusSpace = ThicknessSpace.Meters;
                radiusRing.ThicknessSpace = ThicknessSpace.Meters;
                radiusRing.BlendMode = ShapesBlendMode.Transparent;
                radiusRing.SortingOrder = 75;
                radiusRing.transform.SetParent(transform, false);
                radiusRing.transform.localPosition = Vector3.zero;
                radiusRing.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                radiusRing.transform.localScale = Vector3.one;
            }

            if (radiusSphere != null)
            {
                radiusSphere.RadiusSpace = ThicknessSpace.Meters;
                radiusSphere.BlendMode = ShapesBlendMode.Transparent;
                radiusSphere.SortingOrder = 74;
                radiusSphere.transform.SetParent(transform, false);
                radiusSphere.transform.localPosition = Vector3.zero;
                radiusSphere.transform.localRotation = Quaternion.identity;
                radiusSphere.transform.localScale = Vector3.one;
            }

            if (config == null)
            {
                return;
            }

            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
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
