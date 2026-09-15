using NUnit.Framework;
using UnityEngine;
using TheCircussyOne.Rules;

public sealed class OrbitWeaponRulesTests
{
    [Test]
    public void OrbitPositionsAreEvenlySpacedAroundCenter()
    {
        Vector3 center = new(1f, 0f, 2f);

        OrbitWeaponFrame first = OrbitWeaponRules.EvaluateFrame(center, 0, 4, 0f, 2f, 0.9f, 0.55f);
        OrbitWeaponFrame second = OrbitWeaponRules.EvaluateFrame(center, 1, 4, 0f, 2f, 0.9f, 0.55f);
        OrbitWeaponFrame third = OrbitWeaponRules.EvaluateFrame(center, 2, 4, 0f, 2f, 0.9f, 0.55f);
        OrbitWeaponFrame fourth = OrbitWeaponRules.EvaluateFrame(center, 3, 4, 0f, 2f, 0.9f, 0.55f);

        AssertPosition(first.Position, center + new Vector3(2f, 0.9f, 0f));
        AssertPosition(second.Position, center + new Vector3(0f, 0.9f, 2f));
        AssertPosition(third.Position, center + new Vector3(-2f, 0.9f, 0f));
        AssertPosition(fourth.Position, center + new Vector3(0f, 0.9f, -2f));
        Assert.That(Quaternion.Angle(first.Rotation, Quaternion.Euler(0f, 0f, 0f)), Is.LessThan(0.001f));
        Assert.That(Quaternion.Angle(second.Rotation, Quaternion.Euler(0f, 90f, 0f)), Is.LessThan(0.001f));
    }

    [Test]
    public void TiltedOrbitRaisesHeightAlongOrbitUp()
    {
        Vector3 center = new(1f, 2f, 3f);
        Vector3 orbitUp = Quaternion.AngleAxis(20f, Vector3.right) * Vector3.up;

        OrbitWeaponFrame frame = OrbitWeaponRules.EvaluateFrame(center, 0, 1, 0f, 2f, 0.9f, 0.55f, orbitUp);

        AssertPosition(frame.Position, center + new Vector3(2f, 0f, 0f) + orbitUp.normalized * 0.9f);
        AssertDirection(frame.Rotation * Vector3.up, orbitUp);
    }

    [Test]
    public void TiltedOrbitKeepsRadiusOnOrbitPlane()
    {
        Vector3 center = new(1f, 2f, 3f);
        Vector3 orbitUp = Quaternion.AngleAxis(24f, Vector3.forward) * Vector3.up;

        OrbitWeaponFrame frame = OrbitWeaponRules.EvaluateFrame(center, 1, 4, 0f, 2f, 0.9f, 0.55f, orbitUp);
        Vector3 radial = frame.Position - center - orbitUp.normalized * 0.9f;

        Assert.That(Vector3.Dot(radial, orbitUp.normalized), Is.EqualTo(0f).Within(0.0001f));
        Assert.That(radial.magnitude, Is.EqualTo(2f).Within(0.0001f));
        AssertDirection(frame.Rotation * Vector3.up, orbitUp);
    }

    [Test]
    public void AdvanceAngleWrapsAroundFullCircle()
    {
        float angle = OrbitWeaponRules.AdvanceAngle(350f, 150f, 0.2f);

        Assert.That(angle, Is.EqualTo(20f).Within(0.0001f));
    }

    private static void AssertPosition(Vector3 actual, Vector3 expected)
    {
        Assert.That(actual.x, Is.EqualTo(expected.x).Within(0.0001f));
        Assert.That(actual.y, Is.EqualTo(expected.y).Within(0.0001f));
        Assert.That(actual.z, Is.EqualTo(expected.z).Within(0.0001f));
    }

    private static void AssertDirection(Vector3 actual, Vector3 expected)
    {
        Assert.That(Vector3.Angle(actual.normalized, expected.normalized), Is.LessThan(0.001f));
    }
}
