using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using TheCircussyOne.Runtime;

public sealed class WorldPhysicsQueryTests
{
    private const int TestPhysicsLayer = 31;
    private const int DefaultLayerMask = 1 << TestPhysicsLayer;

    [SetUp]
    public void SetUp()
    {
        TheCircussyOneTestObjects.DestroyAll();
    }

    [TearDown]
    public void TearDown()
    {
        TheCircussyOneTestObjects.DestroyAll();
    }

    [Test]
    public void SpherePathReportsNearestBlocker()
    {
        CreateCube("Far Blocker", new Vector3(0f, 1f, 4f), new Vector3(1f, 2f, 0.2f));
        GameObject near = CreateCube("Near Blocker", new Vector3(0f, 1f, 2f), new Vector3(1f, 2f, 0.2f));
        Physics.SyncTransforms();

        var query = new WorldPhysicsQuery();
        bool hit = query.TryHitSpherePath(Vector3.up, Vector3.up + Vector3.forward * 6f, 0.05f, DefaultLayerMask, out RaycastHit result);

        Assert.That(hit, Is.True);
        Assert.That(result.collider, Is.SameAs(near.GetComponent<Collider>()));
    }

    [Test]
    public void SpherePathCanIgnoreKnownTargetCollider()
    {
        GameObject target = CreateCube("Target", new Vector3(0f, 1f, 2f), new Vector3(1f, 2f, 0.2f));
        Physics.SyncTransforms();

        var query = new WorldPhysicsQuery();
        bool clear = query.HasClearSpherePath(
            Vector3.up,
            Vector3.up + Vector3.forward * 3f,
            0.05f,
            DefaultLayerMask,
            collider => collider == target.GetComponent<Collider>());

        Assert.That(clear, Is.True);
    }

    [Test]
    public void CapsulePathReportsNearestBlocker()
    {
        CreateCube("Far Capsule Blocker", new Vector3(0f, 1f, 5f), new Vector3(1f, 2f, 0.2f));
        GameObject near = CreateCube("Near Capsule Blocker", new Vector3(0f, 1f, 2f), new Vector3(1f, 2f, 0.2f));
        Physics.SyncTransforms();

        var query = new WorldPhysicsQuery();
        bool hit = query.TryHitCapsulePath(
            new Vector3(0f, 1.5f, 0f),
            new Vector3(0f, 0.5f, 0f),
            0.25f,
            Vector3.forward,
            8f,
            DefaultLayerMask,
            out RaycastHit result);

        Assert.That(hit, Is.True);
        Assert.That(result.collider, Is.SameAs(near.GetComponent<Collider>()));
    }

    [Test]
    public void CapsulePathCanIgnoreFilteredHits()
    {
        GameObject ignored = CreateCube("Ignored Capsule Blocker", new Vector3(0f, 1f, 2f), new Vector3(1f, 2f, 0.2f));
        GameObject accepted = CreateCube("Accepted Capsule Blocker", new Vector3(0f, 1f, 4f), new Vector3(1f, 2f, 0.2f));
        Collider ignoredCollider = ignored.GetComponent<Collider>();
        Physics.SyncTransforms();

        var query = new WorldPhysicsQuery();
        bool hit = query.TryHitCapsulePath(
            new Vector3(0f, 1.5f, 0f),
            new Vector3(0f, 0.5f, 0f),
            0.25f,
            Vector3.forward,
            8f,
            DefaultLayerMask,
            out RaycastHit result,
            candidate => candidate.collider == ignoredCollider);

        Assert.That(hit, Is.True);
        Assert.That(result.collider, Is.SameAs(accepted.GetComponent<Collider>()));
    }

    [Test]
    public void HighestSurfaceSamplerUsesTopmostHit()
    {
        CreateCube("Low Surface", new Vector3(0f, 0.25f, 0f), new Vector3(3f, 0.5f, 3f));
        CreateCube("High Surface", new Vector3(0f, 1.25f, 0f), new Vector3(1f, 0.5f, 1f));
        Physics.SyncTransforms();

        var query = new WorldPhysicsQuery();
        bool sampled = query.TrySampleHighestSurfaceY(Vector3.zero, 4f, 6f, DefaultLayerMask, out float surfaceY);

        Assert.That(sampled, Is.True);
        Assert.That(surfaceY, Is.EqualTo(1.5f).Within(0.001f));
    }

    [Test]
    public void SphereClearReportsOverlappingBlocker()
    {
        CreateCube("Blocker", new Vector3(0f, 0.5f, 0f), Vector3.one);
        Physics.SyncTransforms();

        var query = new WorldPhysicsQuery();

        Assert.That(query.IsSphereClear(new Vector3(0f, 0.5f, 0f), 0.25f, DefaultLayerMask), Is.False);
        Assert.That(query.IsSphereClear(new Vector3(3f, 0.5f, 0f), 0.25f, DefaultLayerMask), Is.True);
    }

    [Test]
    public void CapsuleClearReportsOverlappingBlocker()
    {
        CreateCube("Capsule Overlap Blocker", new Vector3(0f, 1f, 0f), new Vector3(0.5f, 2f, 2f));
        Physics.SyncTransforms();

        var query = new WorldPhysicsQuery();

        Assert.That(
            query.IsCapsuleClear(new Vector3(0f, 0.5f, 0f), new Vector3(0f, 1.5f, 0f), 0.5f, DefaultLayerMask),
            Is.False);
        Assert.That(
            query.IsCapsuleClear(new Vector3(3f, 0.5f, 0f), new Vector3(3f, 1.5f, 0f), 0.5f, DefaultLayerMask),
            Is.True);
    }

    [Test]
    public void SphereOverlapCollectionClearsAndReturnsColliders()
    {
        GameObject first = CreateCube("First Overlap", new Vector3(-0.25f, 0.5f, 0f), Vector3.one);
        GameObject second = CreateCube("Second Overlap", new Vector3(0.25f, 0.5f, 0f), Vector3.one);
        GameObject outside = CreateCube("Outside Overlap", new Vector3(5f, 0.5f, 0f), Vector3.one);
        var results = new List<Collider> { outside.GetComponent<Collider>() };
        Physics.SyncTransforms();

        var query = new WorldPhysicsQuery(overlapCapacity: 8);
        int count = query.CollectSphereOverlaps(new Vector3(0f, 0.5f, 0f), 1.1f, DefaultLayerMask, results);

        Assert.That(count, Is.GreaterThanOrEqualTo(2));
        Assert.That(results, Does.Contain(first.GetComponent<Collider>()));
        Assert.That(results, Does.Contain(second.GetComponent<Collider>()));
        Assert.That(results, Has.No.Member(outside.GetComponent<Collider>()));
    }

    [Test]
    public void PenetrationReportsOverlapAndHorizontalDirection()
    {
        GameObject first = CreateCube("First Penetration", new Vector3(0f, 0.5f, 0f), Vector3.one);
        GameObject second = CreateCube("Second Penetration", new Vector3(0.5f, 0.5f, 0f), Vector3.one);
        Physics.SyncTransforms();

        var query = new WorldPhysicsQuery();
        bool overlapping = query.TryComputeHorizontalPenetration(
            first.GetComponent<Collider>(),
            second.GetComponent<Collider>(),
            out Vector3 horizontalDirection,
            out float distance);

        Assert.That(overlapping, Is.True);
        Assert.That(distance, Is.GreaterThan(0f));
        Assert.That(horizontalDirection.y, Is.EqualTo(0f).Within(0.0001f));
        Assert.That(horizontalDirection.sqrMagnitude, Is.GreaterThan(0f));
    }

    private static GameObject CreateCube(string name, Vector3 position, Vector3 scale)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "TCO Test " + name;
        cube.layer = TestPhysicsLayer;
        cube.transform.position = position;
        cube.transform.localScale = scale;
        return cube;
    }
}
