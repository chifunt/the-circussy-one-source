using NUnit.Framework;
using UnityEngine;
using TheCircussyOne.Config;
using TheCircussyOne.Runtime;
using TheCircussyOne.Visuals;

public sealed class ProjectilePhysicsHitQueryTests
{
    private const float ProjectileHitRadius = 0.85f;

    private GameConfig _config;
    private int _projectileHitMask;

    [SetUp]
    public void SetUp()
    {
        TheCircussyOneTestObjects.DestroyAll();
        _config = TheCircussyOneTestObjects.CreateConfig();
        _projectileHitMask = 1 << TheCircussyOneTestObjects.RequireLayer(GameLayers.Enemy);
    }

    [TearDown]
    public void TearDown()
    {
        TheCircussyOneTestObjects.DestroyAll();
        TheCircussyOneTestObjects.Destroy(_config);
    }

    [Test]
    public void ProjectileAtWeaponHeightOverlapsEnemyHurtbox()
    {
        var enemy = TheCircussyOneTestObjects.CreateEnemy(_config, Vector3.zero);
        var hits = new Collider[8];

        Physics.SyncTransforms();
        int count = Physics.OverlapSphereNonAlloc(
            new Vector3(0f, 0.8f, 0f),
            ProjectileHitRadius,
            hits,
            _projectileHitMask,
            QueryTriggerInteraction.Collide);

        Assert.That(ContainsEnemy(hits, count, enemy.View), Is.True);
    }

    [Test]
    public void ProjectileAtElevatedWeaponHeightOverlapsElevatedEnemyHurtbox()
    {
        var enemy = TheCircussyOneTestObjects.CreateEnemy(_config, Vector3.zero);
        enemy.View.SetHeight(2f);
        var hits = new Collider[8];

        Physics.SyncTransforms();
        int count = Physics.OverlapSphereNonAlloc(
            new Vector3(0f, 2.8f, 0f),
            ProjectileHitRadius,
            hits,
            _projectileHitMask,
            QueryTriggerInteraction.Collide);

        Assert.That(ContainsEnemy(hits, count, enemy.View), Is.True);
    }

    [Test]
    public void ProjectileQueryIgnoresPlayerProjectileAndPickupLayers()
    {
        int playerLayer = TheCircussyOneTestObjects.RequireLayer(GameLayers.Player);
        int projectileLayer = TheCircussyOneTestObjects.RequireLayer(GameLayers.Projectile);
        int pickupLayer = TheCircussyOneTestObjects.RequireLayer(GameLayers.Pickup);

        Assert.That(Physics.GetIgnoreLayerCollision(playerLayer, projectileLayer), Is.True);

        CreateTrigger("Player Collider", playerLayer);
        CreateTrigger("Projectile Collider", projectileLayer);
        CreateTrigger("Pickup Collider", pickupLayer);

        var hits = new Collider[8];
        Physics.SyncTransforms();
        int count = Physics.OverlapSphereNonAlloc(
            new Vector3(0f, 0.8f, 0f),
            ProjectileHitRadius,
            hits,
            _projectileHitMask,
            QueryTriggerInteraction.Collide);

        Assert.That(count, Is.Zero);
    }

    private static bool ContainsEnemy(Collider[] hits, int count, Object enemy)
    {
        for (int i = 0; i < count; i++)
        {
            EnemyHitbox hitbox = hits[i] != null ? hits[i].GetComponent<EnemyHitbox>() : null;
            if (hitbox != null && hitbox.Role == EnemyHitboxRole.Hurtbox && hitbox.GetComponentInParent<EnemyView>() == enemy)
            {
                return true;
            }
        }

        return false;
    }

    private static void CreateTrigger(string name, int layer)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "TCO Test " + name;
        go.layer = layer;
        var collider = go.GetComponent<SphereCollider>();
        collider.isTrigger = true;
        go.transform.position = new Vector3(0f, 0.8f, 0f);
    }
}
