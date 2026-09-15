using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;

namespace TheCircussyOne.Visuals
{
    public sealed class ProjectileView : MonoBehaviour
    {
        [SerializeField] private Renderer bodyRenderer;
        [SerializeField] private Transform shapeRoot;
        [SerializeField] private Transform sphereShape;
        [SerializeField] private Transform capsuleShape;
        [SerializeField] private Transform cubeShape;
        [SerializeField] private Transform shardShape;
        [SerializeField] private Transform boltShape;
        [SerializeField] private Transform jugglingBallShape;
        [SerializeField] private TrailRenderer trail;

        private Vector3 baseSpawnScale = Vector3.one;
        private Vector3 spawnScale = Vector3.one;
        private bool hasBaseSpawnScale;
        private MaterialPropertyBlock propertyBlock;
        private Renderer[] jugglingBallPanelRenderers;
        private Renderer knifeBladeRenderer;
        private Renderer knifeHandleRenderer;

        public Vector3 Position => transform.position;
        public bool IsActive => gameObject.activeInHierarchy;
        public Transform ActiveShape { get; private set; }
        public TrailRenderer Trail => trail;

        private void Awake()
        {
            ResolveComponents();
            CaptureSpawnScale();
        }

        public void Prepare(GameConfig config, Vector3 position, Vector3 direction)
        {
            Prepare((DamageFeedbackVisualConfig)null, null, position, direction);
        }

        public void Prepare(GameConfig config, DamageFeedbackVisualConfig feedbackConfig, Vector3 position, Vector3 direction)
        {
            Prepare(feedbackConfig, null, position, direction);
        }

        public void Prepare(DamageFeedbackVisualConfig feedbackConfig, Vector3 position, Vector3 direction)
        {
            Prepare(feedbackConfig, null, position, direction);
        }

        public void Prepare(DamageFeedbackVisualConfig feedbackConfig, WeaponDefinition weapon, Vector3 position, Vector3 direction)
        {
            Prepare(feedbackConfig, weapon, position, direction, 1f);
        }

        public void Prepare(DamageFeedbackVisualConfig feedbackConfig, WeaponDefinition weapon, Vector3 position, Vector3 direction, float visualSizeMultiplier)
        {
            ResolveComponents();
            CaptureSpawnScale();
            ApplyVisuals(weapon, visualSizeMultiplier);
            transform.position = position;
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.localScale = Vector3.zero;
            gameObject.SetActive(true);
            ConfigureTrail(weapon, emit: true);
            VisualTweenUtility.Scale(
                transform,
                spawnScale,
                Mathf.Max(0f, feedbackConfig != null ? feedbackConfig.projectileSpawnSeconds : 0.08f),
                feedbackConfig != null ? feedbackConfig.projectileSpawnEase : EaseSettings.OutBack);
        }

        public void Move(Vector3 direction, float speed, float deltaTime)
        {
            transform.position += direction * speed * deltaTime;
        }

        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        public void SetDirection(Vector3 direction)
        {
            if (direction.sqrMagnitude >= 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            }
        }

        public void ApplyVisuals(WeaponDefinition weapon)
        {
            ApplyVisuals(weapon, 1f);
        }

        public void ApplyVisuals(WeaponDefinition weapon, float visualSizeMultiplier)
        {
            ResolveComponents();
            CaptureSpawnScale();
            EnsureShapeVisuals();

            ProjectileVisualShape shape = ResolveVisualShape(weapon);
            bool useJugglingBall = IsWeapon(weapon, FirstPartyWeaponDefaults.JugglingBallId);
            bool useKnife = IsWeapon(weapon, FirstPartyWeaponDefaults.KnifeFanId);
            float visualScale = Mathf.Max(0.05f, weapon != null ? weapon.projectileVisualScale : 1f);
            visualScale *= Mathf.Max(0.05f, visualSizeMultiplier);
            Color primary = weapon != null ? weapon.projectilePrimaryColor : new Color(1f, 0.88f, 0.18f, 1f);
            Color emission = weapon != null ? weapon.projectileEmissionColor : new Color(1f, 0.87f, 0.22f, 1f);
            float emissionStrength = Mathf.Max(0f, weapon != null ? weapon.projectileEmissionStrength : 0.65f);

            SetActiveShape(shape, useJugglingBall);
            ApplyShapeScale(shape, useJugglingBall);
            ApplyMaterialOverrides(primary, emission, emissionStrength);
            if (useJugglingBall)
            {
                ApplyJugglingBallPanelOverrides(emissionStrength);
            }
            else if (useKnife)
            {
                ApplyKnifeMaterialOverrides(emissionStrength);
            }

            spawnScale = baseSpawnScale * visualScale;
            if (!Application.isPlaying || transform.localScale != Vector3.zero)
            {
                transform.localScale = spawnScale;
            }

            ConfigureTrail(weapon, emit: false);
        }

        public void Deactivate()
        {
            Tween.StopAll(transform);
            if (trail != null)
            {
                trail.emitting = false;
                trail.Clear();
            }

            gameObject.SetActive(false);
        }

        private void ApplyMaterialOverrides(Color primary, Color emission, float emissionStrength)
        {
            propertyBlock ??= new MaterialPropertyBlock();
            propertyBlock.Clear();
            ShaderVisualProperties.ApplyBaseColor(propertyBlock, primary);
            ShaderVisualProperties.ApplyEmission(propertyBlock, emission, emissionStrength);

            Renderer[] renderers = shapeRoot != null
                ? shapeRoot.GetComponentsInChildren<Renderer>(includeInactive: true)
                : GetComponentsInChildren<Renderer>(includeInactive: true);
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i]?.SetPropertyBlock(propertyBlock);
            }
        }

        private void ApplyJugglingBallPanelOverrides(float emissionStrength)
        {
            EnsureJugglingBallShape();
            Color[] panelColors =
            {
                new(0.95f, 0.16f, 0.28f, 1f),
                new(1f, 0.86f, 0.16f, 1f),
                new(0.16f, 0.78f, 1f, 1f),
                new(0.66f, 0.28f, 1f, 1f)
            };

            for (int i = 0; i < jugglingBallPanelRenderers.Length; i++)
            {
                Renderer renderer = jugglingBallPanelRenderers[i];
                if (renderer == null)
                {
                    continue;
                }

                Color color = panelColors[i % panelColors.Length];
                ApplyRendererOverrides(renderer, color, color, Mathf.Max(0.4f, emissionStrength * 0.55f));
            }
        }

        private void ApplyKnifeMaterialOverrides(float emissionStrength)
        {
            ApplyRendererOverrides(
                knifeBladeRenderer,
                new Color(0.93f, 0.93f, 0.86f, 1f),
                new Color(1f, 0.95f, 0.72f, 1f),
                Mathf.Max(0.35f, emissionStrength));
            ApplyRendererOverrides(
                knifeHandleRenderer,
                new Color(0.18f, 0.10f, 0.08f, 1f),
                new Color(0.32f, 0.12f, 0.06f, 1f),
                0.05f);
        }

        private void ApplyRendererOverrides(Renderer renderer, Color primary, Color emission, float emissionStrength)
        {
            if (renderer == null)
            {
                return;
            }

            propertyBlock ??= new MaterialPropertyBlock();
            propertyBlock.Clear();
            ShaderVisualProperties.ApplyBaseColor(propertyBlock, primary);
            ShaderVisualProperties.ApplyEmission(propertyBlock, emission, emissionStrength);
            renderer.SetPropertyBlock(propertyBlock);
        }

        private void ConfigureTrail(WeaponDefinition weapon, bool emit)
        {
            ResolveComponents();
            if (trail == null)
            {
                return;
            }

            bool enabled = weapon == null || weapon.projectileTrailEnabled;
            Color color = weapon != null ? weapon.projectileTrailColor : new Color(1f, 0.88f, 0.18f, 0.75f);
            float width = Mathf.Max(0.001f, weapon != null ? weapon.projectileTrailWidth : 0.08f);
            float lifetime = Mathf.Max(0.01f, weapon != null ? weapon.projectileTrailLifetime : 0.18f);
            bool useJugglingBall = IsWeapon(weapon, FirstPartyWeaponDefaults.JugglingBallId);
            if (useJugglingBall)
            {
                width = Mathf.Max(width, 0.105f);
                lifetime = Mathf.Max(lifetime, 0.28f);
            }

            trail.enabled = enabled;
            trail.emitting = enabled && emit;
            trail.time = lifetime;
            trail.startWidth = width;
            trail.endWidth = 0f;
            trail.startColor = color;
            trail.endColor = new Color(color.r, color.g, color.b, 0f);
            trail.colorGradient = useJugglingBall ? CreateJugglingBallTrailGradient() : CreateTrailGradient(color);
            trail.numCornerVertices = 2;
            trail.numCapVertices = 2;
            trail.alignment = LineAlignment.View;
            trail.textureMode = LineTextureMode.Stretch;
            if (trail.sharedMaterial == null && bodyRenderer != null)
            {
                trail.sharedMaterial = bodyRenderer.sharedMaterial;
            }

            trail.Clear();
        }

        private void SetActiveShape(ProjectileVisualShape shape)
        {
            SetActiveShape(shape, useJugglingBall: false);
        }

        private void SetActiveShape(ProjectileVisualShape shape, bool useJugglingBall)
        {
            ActiveShape = useJugglingBall ? jugglingBallShape : ShapeFor(shape);
            SetShapeEnabled(sphereShape, ActiveShape == sphereShape);
            SetShapeEnabled(capsuleShape, ActiveShape == capsuleShape);
            SetShapeEnabled(cubeShape, ActiveShape == cubeShape);
            SetShapeEnabled(shardShape, ActiveShape == shardShape);
            SetShapeEnabled(boltShape, ActiveShape == boltShape);
            SetShapeEnabled(jugglingBallShape, ActiveShape == jugglingBallShape);
            if (bodyRenderer != null && (shapeRoot == null || !bodyRenderer.transform.IsChildOf(shapeRoot)))
            {
                bodyRenderer.enabled = ActiveShape == null;
            }
        }

        private Transform ShapeFor(ProjectileVisualShape shape)
        {
            return shape switch
            {
                ProjectileVisualShape.Capsule => capsuleShape,
                ProjectileVisualShape.Cube => cubeShape,
                ProjectileVisualShape.Shard => shardShape,
                ProjectileVisualShape.Bolt => boltShape,
                _ => sphereShape
            };
        }

        private static void SetShapeEnabled(Transform shape, bool enabled)
        {
            if (shape == null)
            {
                return;
            }

            shape.gameObject.SetActive(enabled);
            Renderer[] renderers = shape.GetComponentsInChildren<Renderer>(includeInactive: true);
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].enabled = enabled;
            }
        }

        private void ApplyShapeScale(ProjectileVisualShape shape)
        {
            ApplyShapeScale(shape, useJugglingBall: false);
        }

        private void ApplyShapeScale(ProjectileVisualShape shape, bool useJugglingBall)
        {
            if (ActiveShape == null)
            {
                return;
            }

            ActiveShape.localPosition = Vector3.zero;
            ActiveShape.localRotation = useJugglingBall ? Quaternion.identity : shape switch
            {
                ProjectileVisualShape.Capsule => Quaternion.Euler(90f, 0f, 0f),
                ProjectileVisualShape.Shard => Quaternion.identity,
                ProjectileVisualShape.Bolt => Quaternion.Euler(0f, 0f, 8f),
                _ => Quaternion.identity
            };
            ActiveShape.localScale = useJugglingBall ? Vector3.one : shape switch
            {
                ProjectileVisualShape.Capsule => new Vector3(0.55f, 0.92f, 0.55f),
                ProjectileVisualShape.Shard => new Vector3(0.82f, 0.82f, 1.15f),
                ProjectileVisualShape.Bolt => new Vector3(0.92f, 0.92f, 1.25f),
                _ => Vector3.one
            };
        }

        private void EnsureShapeVisuals()
        {
            if (shapeRoot == null)
            {
                Transform existingRoot = transform.Find("Projectile Visuals");
                shapeRoot = existingRoot != null ? existingRoot : new GameObject("Projectile Visuals").transform;
                shapeRoot.SetParent(transform, false);
                shapeRoot.localPosition = Vector3.zero;
                shapeRoot.localRotation = Quaternion.identity;
                shapeRoot.localScale = Vector3.one;
            }

            if (sphereShape == null)
            {
                sphereShape = CreateShape("Sphere", PrimitiveType.Sphere);
            }

            if (capsuleShape == null)
            {
                capsuleShape = CreateShape("Capsule", PrimitiveType.Capsule);
            }

            if (cubeShape == null)
            {
                cubeShape = CreateShape("Cube", PrimitiveType.Cube);
            }

            EnsureJugglingBallShape();
            EnsureKnifeShape();
            EnsureBoltShape();
        }

        private Transform CreateShape(string shapeName, PrimitiveType primitive)
        {
            Transform existing = shapeRoot != null ? shapeRoot.Find(shapeName) : null;
            if (existing != null)
            {
                return existing;
            }

            GameObject shape = GameObject.CreatePrimitive(primitive);
            shape.name = shapeName;
            shape.transform.SetParent(shapeRoot, false);
            shape.layer = gameObject.layer;
            Collider collider = shape.GetComponent<Collider>();
            if (collider != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(collider);
                }
                else
                {
                    DestroyImmediate(collider);
                }
            }

            Renderer renderer = shape.GetComponent<Renderer>();
            if (renderer != null && bodyRenderer != null)
            {
                renderer.sharedMaterial = bodyRenderer.sharedMaterial;
            }

            shape.SetActive(false);
            return shape.transform;
        }

        private void EnsureJugglingBallShape()
        {
            if (jugglingBallShape == null)
            {
                jugglingBallShape = CreateCompositeShape("Juggling Ball");
            }

            if (jugglingBallShape == null)
            {
                return;
            }

            ClearRootPrimitive(jugglingBallShape);
            if (jugglingBallPanelRenderers == null || jugglingBallPanelRenderers.Length != 4)
            {
                jugglingBallPanelRenderers = new Renderer[4];
            }

            for (int i = 0; i < 4; i++)
            {
                string panelName = $"Panel {i + 1}";
                Transform child = jugglingBallShape.Find(panelName);
                if (child == null)
                {
                    child = CreateMeshChild(
                        jugglingBallShape,
                        panelName,
                        CreateSpherePanelMesh(i * 90f, (i + 1) * 90f));
                }

                AssignSharedMaterial(child);
                jugglingBallPanelRenderers[i] = child.GetComponent<Renderer>();
            }

            jugglingBallShape.gameObject.SetActive(false);
        }

        private void EnsureKnifeShape()
        {
            if (shardShape == null)
            {
                shardShape = CreateCompositeShape("Shard");
            }

            if (shardShape == null)
            {
                return;
            }

            ClearRootPrimitive(shardShape);
            Transform blade = shardShape.Find("Knife Blade");
            if (blade == null)
            {
                blade = CreateMeshChild(shardShape, "Knife Blade", CreateFlatPrismMesh(new[]
                {
                    new Vector2(-0.15f, -0.43f),
                    new Vector2(0.15f, -0.43f),
                    new Vector2(0.20f, 0.18f),
                    new Vector2(0f, 0.68f),
                    new Vector2(-0.20f, 0.18f)
                }, 0.018f));
            }

            Transform handle = shardShape.Find("Knife Handle");
            if (handle == null)
            {
                handle = CreateMeshChild(shardShape, "Knife Handle", CreateFlatPrismMesh(new[]
                {
                    new Vector2(-0.08f, -0.72f),
                    new Vector2(0.08f, -0.72f),
                    new Vector2(0.095f, -0.40f),
                    new Vector2(-0.095f, -0.40f)
                }, 0.03f));
            }

            AssignSharedMaterial(blade);
            AssignSharedMaterial(handle);
            knifeBladeRenderer = blade.GetComponent<Renderer>();
            knifeHandleRenderer = handle.GetComponent<Renderer>();
            shardShape.gameObject.SetActive(false);
        }

        private void EnsureBoltShape()
        {
            if (boltShape == null)
            {
                boltShape = CreateCompositeShape("Bolt");
            }

            if (boltShape == null)
            {
                return;
            }

            ClearRootPrimitive(boltShape);
            Transform core = boltShape.Find("Bolt Core");
            if (core == null)
            {
                core = CreateMeshChild(boltShape, "Bolt Core", CreateFlatPrismMesh(new[]
                {
                    new Vector2(-0.09f, -0.66f),
                    new Vector2(0.22f, -0.16f),
                    new Vector2(0.06f, -0.12f),
                    new Vector2(0.18f, 0.64f),
                    new Vector2(-0.25f, 0.06f),
                    new Vector2(-0.07f, 0.02f)
                }, 0.022f));
            }

            AssignSharedMaterial(core);
            boltShape.gameObject.SetActive(false);
        }

        private Transform CreateCompositeShape(string shapeName)
        {
            if (shapeRoot == null)
            {
                return null;
            }

            Transform existing = shapeRoot != null ? shapeRoot.Find(shapeName) : null;
            if (existing != null)
            {
                return existing;
            }

            var shape = new GameObject(shapeName);
            shape.transform.SetParent(shapeRoot, false);
            shape.layer = gameObject.layer;
            shape.SetActive(false);
            return shape.transform;
        }

        private Transform CreateMeshChild(Transform parent, string childName, Mesh mesh)
        {
            var child = new GameObject(childName);
            child.transform.SetParent(parent, false);
            child.layer = gameObject.layer;
            var meshFilter = child.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;
            var renderer = child.AddComponent<MeshRenderer>();
            if (bodyRenderer != null)
            {
                renderer.sharedMaterial = bodyRenderer.sharedMaterial;
            }

            return child.transform;
        }

        private void AssignSharedMaterial(Transform shape)
        {
            if (shape == null || bodyRenderer == null)
            {
                return;
            }

            var renderer = shape.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = bodyRenderer.sharedMaterial;
            }
        }

        private static void ClearRootPrimitive(Transform shape)
        {
            if (shape == null)
            {
                return;
            }

            DestroyComponent(shape.GetComponent<Collider>());
            DestroyComponent(shape.GetComponent<MeshFilter>());
            DestroyComponent(shape.GetComponent<MeshRenderer>());
        }

        private static void DestroyComponent(Component component)
        {
            if (component == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(component);
            }
            else
            {
                DestroyImmediate(component);
            }
        }

        private static Mesh CreateSpherePanelMesh(float startDegrees, float endDegrees)
        {
            const int latitudeSegments = 10;
            const int longitudeSegments = 6;
            const float radius = 0.5f;

            var vertices = new Vector3[(latitudeSegments + 1) * (longitudeSegments + 1)];
            var normals = new Vector3[vertices.Length];
            var uvs = new Vector2[vertices.Length];
            var triangles = new List<int>(latitudeSegments * longitudeSegments * 6);

            float start = startDegrees * Mathf.Deg2Rad;
            float end = endDegrees * Mathf.Deg2Rad;
            for (int lat = 0; lat <= latitudeSegments; lat++)
            {
                float v = lat / (float)latitudeSegments;
                float polar = Mathf.Lerp(-Mathf.PI * 0.5f, Mathf.PI * 0.5f, v);
                float y = Mathf.Sin(polar) * radius;
                float ring = Mathf.Cos(polar) * radius;
                for (int lon = 0; lon <= longitudeSegments; lon++)
                {
                    float u = lon / (float)longitudeSegments;
                    float azimuth = Mathf.Lerp(start, end, u);
                    int index = lat * (longitudeSegments + 1) + lon;
                    Vector3 vertex = new(Mathf.Cos(azimuth) * ring, y, Mathf.Sin(azimuth) * ring);
                    vertices[index] = vertex;
                    normals[index] = vertex.normalized;
                    uvs[index] = new Vector2(u, v);
                }
            }

            for (int lat = 0; lat < latitudeSegments; lat++)
            {
                for (int lon = 0; lon < longitudeSegments; lon++)
                {
                    int a = lat * (longitudeSegments + 1) + lon;
                    int b = a + longitudeSegments + 1;
                    triangles.Add(a);
                    triangles.Add(b);
                    triangles.Add(a + 1);
                    triangles.Add(a + 1);
                    triangles.Add(b);
                    triangles.Add(b + 1);
                }
            }

            var mesh = new Mesh
            {
                name = $"Projectile Ball Panel {startDegrees:0}"
            };
            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateBounds();
            mesh.hideFlags = HideFlags.DontSave;
            return mesh;
        }

        private static Mesh CreateFlatPrismMesh(Vector2[] outline, float halfThickness)
        {
            int count = outline.Length;
            var vertices = new Vector3[count * 2];
            var triangles = new List<int>(count * 12);
            for (int i = 0; i < count; i++)
            {
                vertices[i] = new Vector3(outline[i].x, halfThickness, outline[i].y);
                vertices[i + count] = new Vector3(outline[i].x, -halfThickness, outline[i].y);
            }

            for (int i = 1; i < count - 1; i++)
            {
                triangles.Add(0);
                triangles.Add(i);
                triangles.Add(i + 1);

                triangles.Add(count);
                triangles.Add(count + i + 1);
                triangles.Add(count + i);
            }

            for (int i = 0; i < count; i++)
            {
                int next = (i + 1) % count;
                triangles.Add(i);
                triangles.Add(next);
                triangles.Add(count + next);
                triangles.Add(i);
                triangles.Add(count + next);
                triangles.Add(count + i);
            }

            var mesh = new Mesh
            {
                name = "Projectile Flat Prism"
            };
            mesh.vertices = vertices;
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.hideFlags = HideFlags.DontSave;
            return mesh;
        }

        private static Gradient CreateTrailGradient(Color color)
        {
            var gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(color.r, color.g, color.b, 1f), 0f),
                    new GradientColorKey(new Color(color.r, color.g, color.b, 1f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(color.a, 0f),
                    new GradientAlphaKey(0f, 1f)
                });
            return gradient;
        }

        private static Gradient CreateJugglingBallTrailGradient()
        {
            var gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(1f, 0.18f, 0.30f, 1f), 0f),
                    new GradientColorKey(new Color(1f, 0.86f, 0.16f, 1f), 0.34f),
                    new GradientColorKey(new Color(0.18f, 0.84f, 1f, 1f), 0.67f),
                    new GradientColorKey(new Color(0.75f, 0.32f, 1f, 1f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0.78f, 0f),
                    new GradientAlphaKey(0.42f, 0.5f),
                    new GradientAlphaKey(0f, 1f)
                });
            return gradient;
        }

        private void ResolveComponents()
        {
            if (bodyRenderer == null)
            {
                bodyRenderer = GetComponentInChildren<Renderer>();
            }

            if (shapeRoot == null)
            {
                shapeRoot = transform.Find("Projectile Visuals");
            }

            if (shapeRoot != null)
            {
                if (sphereShape == null)
                {
                    sphereShape = shapeRoot.Find("Sphere");
                }

                if (capsuleShape == null)
                {
                    capsuleShape = shapeRoot.Find("Capsule");
                }

                if (cubeShape == null)
                {
                    cubeShape = shapeRoot.Find("Cube");
                }

                if (shardShape == null)
                {
                    shardShape = shapeRoot.Find("Shard");
                }

                if (boltShape == null)
                {
                    boltShape = shapeRoot.Find("Bolt");
                }

                if (jugglingBallShape == null)
                {
                    jugglingBallShape = shapeRoot.Find("Juggling Ball");
                }

                Transform knifeBlade = shardShape != null ? shardShape.Find("Knife Blade") : null;
                Transform knifeHandle = shardShape != null ? shardShape.Find("Knife Handle") : null;
                if (knifeBladeRenderer == null)
                {
                    knifeBladeRenderer = knifeBlade != null ? knifeBlade.GetComponent<Renderer>() : null;
                }

                if (knifeHandleRenderer == null)
                {
                    knifeHandleRenderer = knifeHandle != null ? knifeHandle.GetComponent<Renderer>() : null;
                }
            }

            if (trail == null)
            {
                trail = GetComponentInChildren<TrailRenderer>(includeInactive: true);
                if (trail == null)
                {
                    trail = gameObject.AddComponent<TrailRenderer>();
                }
            }
        }

        private void CaptureSpawnScale()
        {
            if (hasBaseSpawnScale || transform.localScale == Vector3.zero)
            {
                return;
            }

            baseSpawnScale = transform.localScale;
            spawnScale = baseSpawnScale;
            hasBaseSpawnScale = true;
        }

        private static ProjectileVisualShape ResolveVisualShape(WeaponDefinition weapon)
        {
            if (IsWeapon(weapon, FirstPartyWeaponDefaults.CannonId))
            {
                return ProjectileVisualShape.Sphere;
            }

            return weapon != null ? weapon.projectileVisualShape : ProjectileVisualShape.Sphere;
        }

        private static bool IsWeapon(WeaponDefinition weapon, string weaponId)
        {
            return weapon != null && string.Equals(weapon.Id, weaponId, System.StringComparison.Ordinal);
        }
    }
}
