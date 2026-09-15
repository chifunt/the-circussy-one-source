using NUnit.Framework;
using UnityEngine;
using TheCircussyOne.Runtime;

public sealed class WorldSurfaceResolverTests
{
    private const int TestSurfaceLayer = 31;
    private const int TestSurfaceMask = 1 << TestSurfaceLayer;

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
    public void ResolveReturnsRaisedSurfacePosition()
    {
        CreateBoxSurface("Raised Ground", y: 3f, size: new Vector3(12f, 0.2f, 12f));
        var resolver = new WorldSurfaceResolver();

        WorldSurfaceSample sample = resolver.Resolve(Vector3.zero, probeHeight: 6f, probeDepth: 8f, TestSurfaceMask);

        Assert.That(sample.FoundSurface, Is.True);
        Assert.That(sample.Position.y, Is.EqualTo(3f).Within(0.001f));
        Assert.That(Vector3.Angle(sample.Normal, Vector3.up), Is.LessThan(0.1f));
    }

    [Test]
    public void ResolveReturnsRampNormal()
    {
        Vector3 expectedNormal = CreateSlopedSurface(30f);
        var resolver = new WorldSurfaceResolver();

        WorldSurfaceSample sample = resolver.Resolve(Vector3.zero, probeHeight: 6f, probeDepth: 8f, TestSurfaceMask);

        Assert.That(sample.FoundSurface, Is.True);
        Assert.That(Vector3.Angle(sample.Normal, expectedNormal), Is.LessThan(1f));
    }

    [Test]
    public void ResolvePreservesRequestedHeightWhenNoSurfaceIsFound()
    {
        var resolver = new WorldSurfaceResolver();
        Vector3 requested = new(1f, 5f, -2f);

        WorldSurfaceSample sample = resolver.Resolve(requested, probeHeight: 2f, probeDepth: 2f, TestSurfaceMask);

        Assert.That(sample.FoundSurface, Is.False);
        Assert.That(sample.Position, Is.EqualTo(requested));
        Assert.That(sample.Normal, Is.EqualTo(Vector3.up));
    }

    private static void CreateBoxSurface(string name, float y, Vector3 size)
    {
        GameObject ground = TheCircussyOneTestObjects.CreateRoot(name);
        ground.layer = TestSurfaceLayer;
        var collider = ground.AddComponent<BoxCollider>();
        collider.center = Vector3.zero;
        collider.size = size;
        ground.transform.position = new Vector3(0f, y - size.y * 0.5f, 0f);
        Physics.SyncTransforms();
    }

    private static Vector3 CreateSlopedSurface(float angleDegrees)
    {
        GameObject slope = TheCircussyOneTestObjects.CreateRoot("Ramp Surface");
        slope.layer = TestSurfaceLayer;
        var collider = slope.AddComponent<BoxCollider>();
        collider.center = Vector3.zero;
        collider.size = new Vector3(12f, 0.1f, 12f);
        slope.transform.rotation = Quaternion.AngleAxis(angleDegrees, Vector3.right);
        Vector3 normal = slope.transform.TransformDirection(Vector3.up).normalized;
        slope.transform.position = -normal * 0.05f;
        Physics.SyncTransforms();
        return normal;
    }
}
