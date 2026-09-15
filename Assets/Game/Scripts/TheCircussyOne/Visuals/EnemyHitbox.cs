using UnityEngine;

namespace TheCircussyOne.Visuals
{
    public enum EnemyHitboxRole
    {
        Hurtbox,
        ContactDamage,
        MovementBody
    }

    public sealed class EnemyHitbox : MonoBehaviour
    {
        [SerializeField] private EnemyHitboxRole role;
        [SerializeField] private SphereCollider sphereCollider;
        [SerializeField] private BoxCollider boxCollider;
        [SerializeField] private CapsuleCollider capsuleCollider;

        public EnemyHitboxRole Role => role;

        public Collider Collider
        {
            get
            {
                ResolveColliders();
                return role switch
                {
                    EnemyHitboxRole.ContactDamage => boxCollider,
                    EnemyHitboxRole.MovementBody => capsuleCollider,
                    _ => sphereCollider
                };
            }
        }

        public Vector3 WorldCenter
        {
            get
            {
                Collider collider = Collider;
                return collider != null ? collider.bounds.center : transform.position;
            }
        }

        public float WorldRadius
        {
            get
            {
                ResolveColliders();
                if (role == EnemyHitboxRole.MovementBody && capsuleCollider != null)
                {
                    Vector3 movementScale = transform.lossyScale;
                    float largestHorizontalAxis = Mathf.Max(Mathf.Abs(movementScale.x), Mathf.Abs(movementScale.z));
                    return capsuleCollider.radius * largestHorizontalAxis;
                }

                if (sphereCollider == null)
                {
                    return 0f;
                }

                Vector3 scale = transform.lossyScale;
                float largestAxis = Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
                return sphereCollider.radius * largestAxis;
            }
        }

        private void Awake()
        {
            ResolveColliders();
        }

        public void ConfigureSphere(EnemyHitboxRole hitboxRole, float radius, Vector3 localCenter)
        {
            role = hitboxRole;
            transform.localPosition = localCenter;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            ResolveColliders();
            RemoveBoxCollider();
            RemoveCapsuleCollider();
            if (sphereCollider == null)
            {
                sphereCollider = gameObject.AddComponent<SphereCollider>();
            }

            sphereCollider.isTrigger = true;
            sphereCollider.enabled = true;
            sphereCollider.center = Vector3.zero;
            sphereCollider.radius = Mathf.Max(0.01f, radius);
        }

        public void ConfigureBox(EnemyHitboxRole hitboxRole, Vector3 size, Vector3 localCenter)
        {
            role = hitboxRole;
            transform.localPosition = localCenter;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            ResolveColliders();
            RemoveSphereCollider();
            RemoveCapsuleCollider();
            if (boxCollider == null)
            {
                boxCollider = gameObject.AddComponent<BoxCollider>();
            }

            boxCollider.isTrigger = true;
            boxCollider.enabled = true;
            boxCollider.center = Vector3.zero;
            boxCollider.size = new Vector3(
                Mathf.Max(0.01f, size.x),
                Mathf.Max(0.01f, size.y),
                Mathf.Max(0.01f, size.z));
        }

        public void ConfigureCapsule(EnemyHitboxRole hitboxRole, float radius, float height, Vector3 localCenter)
        {
            role = hitboxRole;
            transform.localPosition = localCenter;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            ResolveColliders();
            RemoveSphereCollider();
            RemoveBoxCollider();
            if (capsuleCollider == null)
            {
                capsuleCollider = gameObject.AddComponent<CapsuleCollider>();
            }

            float safeRadius = Mathf.Max(0.01f, radius);
            capsuleCollider.isTrigger = true;
            capsuleCollider.enabled = true;
            capsuleCollider.center = Vector3.zero;
            capsuleCollider.direction = 1;
            capsuleCollider.radius = safeRadius;
            capsuleCollider.height = Mathf.Max(safeRadius * 2f, height);
        }

        private void OnDrawGizmos()
        {
            ResolveColliders();
            Color color = role switch
            {
                EnemyHitboxRole.Hurtbox => new Color(0.2f, 0.7f, 1f, 0.22f),
                EnemyHitboxRole.MovementBody => new Color(0.25f, 1f, 0.45f, 0.18f),
                _ => new Color(1f, 0.35f, 0.12f, 0.28f)
            };
            Color wireColor = role switch
            {
                EnemyHitboxRole.Hurtbox => new Color(0.2f, 0.7f, 1f, 0.9f),
                EnemyHitboxRole.MovementBody => new Color(0.35f, 1f, 0.55f, 0.95f),
                _ => new Color(1f, 0.85f, 0.15f, 0.95f)
            };

            Matrix4x4 previousMatrix = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            if (role == EnemyHitboxRole.ContactDamage && boxCollider != null)
            {
                Gizmos.color = color;
                Gizmos.DrawCube(boxCollider.center, boxCollider.size);
                Gizmos.color = wireColor;
                Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
            }
            else if (role == EnemyHitboxRole.MovementBody && capsuleCollider != null)
            {
                DrawCapsuleGizmo(capsuleCollider, color, wireColor);
            }
            else if (sphereCollider != null)
            {
                Gizmos.color = color;
                Gizmos.DrawSphere(sphereCollider.center, sphereCollider.radius);
                Gizmos.color = wireColor;
                Gizmos.DrawWireSphere(sphereCollider.center, sphereCollider.radius);
            }

            Gizmos.matrix = previousMatrix;
        }

        private void ResolveColliders()
        {
            if (sphereCollider == null)
            {
                sphereCollider = GetComponent<SphereCollider>();
            }

            if (boxCollider == null)
            {
                boxCollider = GetComponent<BoxCollider>();
            }

            if (capsuleCollider == null)
            {
                capsuleCollider = GetComponent<CapsuleCollider>();
            }
        }

        private void RemoveSphereCollider()
        {
            if (sphereCollider == null)
            {
                return;
            }

            sphereCollider.enabled = false;
            RemoveCollider(sphereCollider);
            sphereCollider = null;
        }

        private void RemoveBoxCollider()
        {
            if (boxCollider == null)
            {
                return;
            }

            boxCollider.enabled = false;
            RemoveCollider(boxCollider);
            boxCollider = null;
        }

        private void RemoveCapsuleCollider()
        {
            if (capsuleCollider == null)
            {
                return;
            }

            capsuleCollider.enabled = false;
            RemoveCollider(capsuleCollider);
            capsuleCollider = null;
        }

        private static void DrawCapsuleGizmo(CapsuleCollider capsule, Color color, Color wireColor)
        {
            float radius = capsule.radius;
            float cylinderHeight = Mathf.Max(0f, capsule.height - radius * 2f);
            Vector3 top = capsule.center + Vector3.up * (cylinderHeight * 0.5f);
            Vector3 bottom = capsule.center - Vector3.up * (cylinderHeight * 0.5f);

            Gizmos.color = color;
            Gizmos.DrawSphere(top, radius);
            Gizmos.DrawSphere(bottom, radius);

            Gizmos.color = wireColor;
            Gizmos.DrawWireSphere(top, radius);
            Gizmos.DrawWireSphere(bottom, radius);
            Gizmos.DrawLine(top + Vector3.right * radius, bottom + Vector3.right * radius);
            Gizmos.DrawLine(top - Vector3.right * radius, bottom - Vector3.right * radius);
            Gizmos.DrawLine(top + Vector3.forward * radius, bottom + Vector3.forward * radius);
            Gizmos.DrawLine(top - Vector3.forward * radius, bottom - Vector3.forward * radius);
        }

        private static void RemoveCollider(Collider collider)
        {
            if (collider == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(collider);
            }
            else
            {
                DestroyImmediate(collider);
            }
        }
    }
}
