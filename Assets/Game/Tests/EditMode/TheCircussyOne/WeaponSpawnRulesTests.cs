using NUnit.Framework;
using UnityEngine;
using TheCircussyOne.Rules;

public sealed class WeaponSpawnRulesTests
{
    [Test]
    public void AimDirectionPreservesVerticalDifference()
    {
        Vector3 direction = WeaponSpawnRules.AimDirection(
            new Vector3(0f, 3f, 0f),
            new Vector3(0f, 1f, 4f),
            Vector3.forward);

        Assert.That(direction.x, Is.EqualTo(0f).Within(0.0001f));
        Assert.That(direction.y, Is.LessThan(0f));
        Assert.That(direction.y, Is.EqualTo(-0.4472136f).Within(0.0001f));
        Assert.That(direction.z, Is.EqualTo(0.8944272f).Within(0.0001f));
    }

    [Test]
    public void MuzzlePositionOffsetsForwardFromPlayerAtConfiguredHeight()
    {
        Vector3 muzzle = WeaponSpawnRules.MuzzlePosition(
            new Vector3(2f, 0f, 3f),
            Vector3.right,
            height: 0.8f,
            forwardOffset: 0.9f);

        Assert.That(muzzle, Is.EqualTo(new Vector3(2.9f, 0.8f, 3f)));
    }

    [Test]
    public void MuzzlePositionIgnoresVerticalAimForHorizontalOffset()
    {
        Vector3 muzzle = WeaponSpawnRules.MuzzlePosition(
            Vector3.zero,
            new Vector3(0f, 10f, 2f),
            height: 1.8f,
            forwardOffset: 1.1f);

        Assert.That(muzzle, Is.EqualTo(new Vector3(0f, 1.8f, 1.1f)));
    }

    [Test]
    public void AimErrorDirectionIsDeterministicForSeed()
    {
        Vector3 first = WeaponSpawnRules.AimErrorDirection(Vector3.forward, 8f, 12345);
        Vector3 second = WeaponSpawnRules.AimErrorDirection(Vector3.forward, 8f, 12345);

        Assert.That(first.x, Is.EqualTo(second.x).Within(0.0001f));
        Assert.That(first.y, Is.EqualTo(second.y).Within(0.0001f));
        Assert.That(first.z, Is.EqualTo(second.z).Within(0.0001f));
        Assert.That(Vector3.Angle(Vector3.forward, first), Is.LessThanOrEqualTo(8f + 0.0001f));
    }

    [Test]
    public void AimErrorDirectionReturnsIntendedDirectionWhenErrorIsZero()
    {
        Vector3 direction = new Vector3(1f, 0.25f, 2f).normalized;

        Vector3 result = WeaponSpawnRules.AimErrorDirection(direction, 0f, 99);

        Assert.That(Vector3.Dot(result, direction), Is.GreaterThan(0.999f));
    }

    [Test]
    public void AimErrorSeedIncludesWeaponFireSequenceAndProjectileIndex()
    {
        int baseSeed = WeaponSpawnRules.AimErrorSeed("knife_fan", 1, 0);

        Assert.That(WeaponSpawnRules.AimErrorSeed("knife_fan", 1, 0), Is.EqualTo(baseSeed));
        Assert.That(WeaponSpawnRules.AimErrorSeed("knife_fan", 2, 0), Is.Not.EqualTo(baseSeed));
        Assert.That(WeaponSpawnRules.AimErrorSeed("knife_fan", 1, 1), Is.Not.EqualTo(baseSeed));
        Assert.That(WeaponSpawnRules.AimErrorSeed("juggling_ball", 1, 0), Is.Not.EqualTo(baseSeed));
    }

    [Test]
    public void AimErrorSeedIncludesRunSeedWhenProvided()
    {
        int firstRun = WeaponSpawnRules.AimErrorSeed("knife_fan", 1, 0, runSeed: 123);
        int secondRun = WeaponSpawnRules.AimErrorSeed("knife_fan", 1, 0, runSeed: 456);

        Assert.That(WeaponSpawnRules.AimErrorSeed("knife_fan", 1, 0, runSeed: 123), Is.EqualTo(firstRun));
        Assert.That(secondRun, Is.Not.EqualTo(firstRun));
    }
}
