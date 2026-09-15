using NUnit.Framework;
using TheCircussyOne.Content;
using TheCircussyOne.Runtime;
using TheCircussyOne.Visuals;
using UnityEngine;

namespace TheCircussyOne.Tests
{
    public sealed class EnemyHitboxViewDriverTests
    {
        [Test]
        public void ConfigureCreatesExplicitHitboxesAndDisablesVisualColliders()
        {
            var root = new GameObject("Enemy Root");
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(root.transform, false);
            var profile = new EnemyBodyProfile
            {
                hurtboxRadius = 0.7f,
                hurtboxHeightOffset = 1.1f,
                contactHitboxSize = new Vector3(0.5f, 0.8f, 0.4f),
                contactHitboxOffset = new Vector3(0.1f, 0.6f, 0.3f),
                movementBodyRadius = 0.45f,
                movementBodyHeight = 1.3f,
                movementBodyOffset = new Vector3(0.02f, 0.7f, -0.04f)
            };
            EnemyHitbox hurtbox = null;
            EnemyHitbox contactHitbox = null;
            EnemyHitbox movementBody = null;

            try
            {
                EnemyHitboxViewDriver.Configure(root.transform, profile, ref hurtbox, ref contactHitbox, ref movementBody);
                EnemyHitboxViewDriver.DisableVisualBodyColliders(body.transform);

                Assert.That(hurtbox, Is.Not.Null);
                Assert.That(hurtbox.Role, Is.EqualTo(EnemyHitboxRole.Hurtbox));
                Assert.That(hurtbox.Collider, Is.TypeOf<SphereCollider>());
                Assert.That(((SphereCollider)hurtbox.Collider).radius, Is.EqualTo(0.7f).Within(0.0001f));
                Assert.That(hurtbox.transform.localPosition, Is.EqualTo(new Vector3(0f, 1.1f, 0f)));
                Assert.That(hurtbox.gameObject.layer, Is.EqualTo(GameLayers.EnemyIndex));

                Assert.That(contactHitbox, Is.Not.Null);
                Assert.That(contactHitbox.Role, Is.EqualTo(EnemyHitboxRole.ContactDamage));
                Assert.That(contactHitbox.Collider, Is.TypeOf<BoxCollider>());
                Assert.That(((BoxCollider)contactHitbox.Collider).size, Is.EqualTo(profile.contactHitboxSize));
                Assert.That(contactHitbox.transform.localPosition, Is.EqualTo(profile.contactHitboxOffset));
                Assert.That(contactHitbox.gameObject.layer, Is.EqualTo(GameLayers.EnemyIndex));

                Assert.That(movementBody, Is.Not.Null);
                Assert.That(movementBody.Role, Is.EqualTo(EnemyHitboxRole.MovementBody));
                Assert.That(movementBody.Collider, Is.TypeOf<CapsuleCollider>());
                Assert.That(((CapsuleCollider)movementBody.Collider).radius, Is.EqualTo(0.45f).Within(0.0001f));
                Assert.That(((CapsuleCollider)movementBody.Collider).height, Is.EqualTo(1.3f).Within(0.0001f));
                Assert.That(movementBody.transform.localPosition, Is.EqualTo(profile.movementBodyOffset));
                Assert.That(movementBody.gameObject.layer, Is.EqualTo(GameLayers.EnemyIndex));

                Assert.That(body.GetComponent<Collider>().enabled, Is.False);
                Assert.That(hurtbox.Collider.enabled, Is.True);
                Assert.That(contactHitbox.Collider.enabled, Is.True);
                Assert.That(movementBody.Collider.enabled, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void ResolveFindsExistingRoleChildren()
        {
            var root = new GameObject("Enemy Root");
            EnemyHitbox existingHurtbox = new GameObject("Existing Hurtbox").AddComponent<EnemyHitbox>();
            existingHurtbox.transform.SetParent(root.transform, false);
            existingHurtbox.ConfigureSphere(EnemyHitboxRole.Hurtbox, 0.5f, Vector3.up);
            EnemyHitbox existingContact = new GameObject("Existing Contact").AddComponent<EnemyHitbox>();
            existingContact.transform.SetParent(root.transform, false);
            existingContact.ConfigureBox(EnemyHitboxRole.ContactDamage, Vector3.one, Vector3.forward);
            EnemyHitbox existingMovement = new GameObject("Existing Movement").AddComponent<EnemyHitbox>();
            existingMovement.transform.SetParent(root.transform, false);
            existingMovement.ConfigureCapsule(EnemyHitboxRole.MovementBody, 0.4f, 1.2f, Vector3.zero);
            EnemyHitbox hurtbox = null;
            EnemyHitbox contactHitbox = null;
            EnemyHitbox movementBody = null;

            try
            {
                EnemyHitboxViewDriver.Resolve(root.transform, ref hurtbox, ref contactHitbox, ref movementBody);

                Assert.That(hurtbox, Is.SameAs(existingHurtbox));
                Assert.That(contactHitbox, Is.SameAs(existingContact));
                Assert.That(movementBody, Is.SameAs(existingMovement));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }
    }
}
