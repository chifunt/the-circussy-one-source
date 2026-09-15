using TheCircussyOne.Content;
using TheCircussyOne.Runtime;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TheCircussyOne.Visuals
{
    internal static class EnemyHitboxViewDriver
    {
        public static void Configure(
            Transform root,
            EnemyBodyProfile profile,
            ref EnemyHitbox hurtbox,
            ref EnemyHitbox contactHitbox,
            ref EnemyHitbox movementBody)
        {
            Resolve(root, ref hurtbox, ref contactHitbox, ref movementBody);
            if (root == null || profile == null)
            {
                return;
            }

            int enemyLayer = LayerMask.NameToLayer(GameLayers.Enemy);
            Vector3 hurtboxCenter = new(0f, Mathf.Max(0f, profile.hurtboxHeightOffset), 0f);

            ConfigureSphereHitbox(root, ref hurtbox, "Enemy Hurtbox", EnemyHitboxRole.Hurtbox, profile.hurtboxRadius, hurtboxCenter, enemyLayer);
            ConfigureBoxHitbox(root, ref contactHitbox, "Enemy Contact Hitbox", EnemyHitboxRole.ContactDamage, profile.contactHitboxSize, profile.contactHitboxOffset, enemyLayer);
            ConfigureCapsuleHitbox(root, ref movementBody, "Enemy Movement Body", EnemyHitboxRole.MovementBody, profile.movementBodyRadius, profile.movementBodyHeight, profile.movementBodyOffset, enemyLayer);
        }

        public static void Resolve(
            Transform root,
            ref EnemyHitbox hurtbox,
            ref EnemyHitbox contactHitbox,
            ref EnemyHitbox movementBody)
        {
            if (root == null || hurtbox != null && contactHitbox != null && movementBody != null)
            {
                return;
            }

            EnemyHitbox[] hitboxes = root.GetComponentsInChildren<EnemyHitbox>(includeInactive: true);
            for (int i = 0; i < hitboxes.Length; i++)
            {
                EnemyHitbox hitbox = hitboxes[i];
                if (hitbox == null)
                {
                    continue;
                }

                if (hitbox.Role == EnemyHitboxRole.Hurtbox && hurtbox == null)
                {
                    hurtbox = hitbox;
                }
                else if (hitbox.Role == EnemyHitboxRole.ContactDamage && contactHitbox == null)
                {
                    contactHitbox = hitbox;
                }
                else if (hitbox.Role == EnemyHitboxRole.MovementBody && movementBody == null)
                {
                    movementBody = hitbox;
                }
            }
        }

        public static void DisableVisualBodyColliders(Transform body)
        {
            if (body == null)
            {
                return;
            }

            Collider[] colliders = body.GetComponentsInChildren<Collider>(includeInactive: true);
            for (int i = 0; i < colliders.Length; i++)
            {
                Collider collider = colliders[i];
                if (collider != null && collider.GetComponent<EnemyHitbox>() == null)
                {
                    collider.enabled = false;
                }
            }
        }

#if UNITY_EDITOR
        public static void DrawReachMarkers(Transform root, EnemyHitbox movementBody, EnemyHitbox contactHitbox)
        {
            if (root == null || movementBody == null || contactHitbox == null)
            {
                return;
            }

            CapsuleCollider movementCapsule = movementBody.Collider as CapsuleCollider;
            BoxCollider contactBox = contactHitbox.Collider as BoxCollider;
            if (movementCapsule == null || contactBox == null)
            {
                return;
            }

            float movementFront = movementBody.transform.localPosition.z + movementCapsule.radius;
            float contactFront = contactHitbox.transform.localPosition.z + contactBox.size.z * 0.5f;
            DrawReachMarker(root, movementFront, movementBody.transform.localPosition.y, "Movement front", new Color(0.35f, 1f, 0.55f, 0.95f));
            DrawReachMarker(root, contactFront, contactHitbox.transform.localPosition.y, "Contact front", new Color(1f, 0.85f, 0.15f, 0.95f));
        }

        private static void DrawReachMarker(Transform root, float localZ, float localY, string label, Color color)
        {
            Vector3 center = root.TransformPoint(new Vector3(0f, localY, localZ));
            Vector3 left = root.TransformPoint(new Vector3(-0.48f, localY, localZ));
            Vector3 right = root.TransformPoint(new Vector3(0.48f, localY, localZ));
            Vector3 forward = root.TransformDirection(Vector3.forward) * 0.28f;

            Handles.color = color;
            Handles.DrawLine(left, right);
            Handles.DrawLine(center, center + forward);
            GUIStyle style = new(EditorStyles.boldLabel);
            style.normal.textColor = color;
            Handles.Label(center + Vector3.up * 0.12f + forward, label, style);
        }
#endif

        private static void ConfigureSphereHitbox(
            Transform root,
            ref EnemyHitbox hitbox,
            string childName,
            EnemyHitboxRole role,
            float radius,
            Vector3 localCenter,
            int layer)
        {
            hitbox = GetOrCreateHitbox(root, hitbox, childName);
            if (layer >= 0)
            {
                hitbox.gameObject.layer = layer;
            }

            hitbox.ConfigureSphere(role, radius, localCenter);
        }

        private static void ConfigureBoxHitbox(
            Transform root,
            ref EnemyHitbox hitbox,
            string childName,
            EnemyHitboxRole role,
            Vector3 size,
            Vector3 localCenter,
            int layer)
        {
            hitbox = GetOrCreateHitbox(root, hitbox, childName);
            if (layer >= 0)
            {
                hitbox.gameObject.layer = layer;
            }

            hitbox.ConfigureBox(role, size, localCenter);
        }

        private static void ConfigureCapsuleHitbox(
            Transform root,
            ref EnemyHitbox hitbox,
            string childName,
            EnemyHitboxRole role,
            float radius,
            float height,
            Vector3 localCenter,
            int layer)
        {
            hitbox = GetOrCreateHitbox(root, hitbox, childName);
            if (layer >= 0)
            {
                hitbox.gameObject.layer = layer;
            }

            hitbox.ConfigureCapsule(role, radius, height, localCenter);
        }

        private static EnemyHitbox GetOrCreateHitbox(Transform root, EnemyHitbox hitbox, string childName)
        {
            if (hitbox != null)
            {
                return hitbox;
            }

            Transform child = root.Find(childName);
            if (child == null)
            {
                child = new GameObject(childName).transform;
                child.SetParent(root, false);
            }

            hitbox = child.GetComponent<EnemyHitbox>();
            if (hitbox == null)
            {
                hitbox = child.gameObject.AddComponent<EnemyHitbox>();
            }

            return hitbox;
        }
    }
}
