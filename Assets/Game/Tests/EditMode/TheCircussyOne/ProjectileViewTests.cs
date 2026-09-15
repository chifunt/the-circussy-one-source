using NUnit.Framework;
using UnityEngine;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Visuals;

public sealed class ProjectileViewTests
{
    private GameConfig _config;
    private WeaponDefinition _weapon;

    [SetUp]
    public void SetUp()
    {
        TheCircussyOneTestObjects.DestroyAll();
        _config = TheCircussyOneTestObjects.CreateConfig();
        _weapon = TheCircussyOneTestObjects.CreateWeaponDefinition(_config);
    }

    [TearDown]
    public void TearDown()
    {
        TheCircussyOneTestObjects.DestroyAll();
        TheCircussyOneTestObjects.Destroy(_config);
        TheCircussyOneTestObjects.Destroy(_weapon);
    }

    [Test]
    public void AppliesWeaponShapeScaleColorAndTrailSettings()
    {
        _weapon.ApplyDefaults(FirstPartyWeaponDefaults.GetOrDefault(FirstPartyWeaponDefaults.SpotlightBoltId));
        _weapon.projectileVisualShape = ProjectileVisualShape.Bolt;
        _weapon.projectileVisualScale = 1.4f;
        _weapon.projectilePrimaryColor = Color.magenta;
        _weapon.projectileEmissionColor = Color.cyan;
        _weapon.projectileEmissionStrength = 1.25f;
        _weapon.projectileTrailEnabled = true;
        _weapon.projectileTrailColor = new Color(0.1f, 0.9f, 1f, 0.8f);
        _weapon.projectileTrailWidth = 0.2f;
        _weapon.projectileTrailLifetime = 0.45f;
        ProjectileView projectile = TheCircussyOneTestObjects.CreateProjectilePrefab();

        projectile.ApplyVisuals(_weapon, 1.5f);

        Assert.That(projectile.ActiveShape, Is.Not.Null);
        Assert.That(projectile.ActiveShape.name, Is.EqualTo("Bolt"));
        Vector3 expectedScale = Vector3.one * 0.22f * 1.4f * 1.5f;
        Assert.That(projectile.transform.localScale.x, Is.EqualTo(expectedScale.x).Within(0.0001f));
        Assert.That(projectile.transform.localScale.y, Is.EqualTo(expectedScale.y).Within(0.0001f));
        Assert.That(projectile.transform.localScale.z, Is.EqualTo(expectedScale.z).Within(0.0001f));
        Assert.That(projectile.Trail, Is.Not.Null);
        Assert.That(projectile.Trail.enabled, Is.True);
        Assert.That(projectile.Trail.emitting, Is.False);
        Assert.That(projectile.Trail.startWidth, Is.EqualTo(0.2f));
        Assert.That(projectile.Trail.time, Is.EqualTo(0.45f));
        Assert.That(projectile.Trail.startColor.r, Is.EqualTo(_weapon.projectileTrailColor.r).Within(0.003f));
        Assert.That(projectile.Trail.startColor.g, Is.EqualTo(_weapon.projectileTrailColor.g).Within(0.003f));
        Assert.That(projectile.Trail.startColor.b, Is.EqualTo(_weapon.projectileTrailColor.b).Within(0.003f));
        Assert.That(projectile.Trail.startColor.a, Is.EqualTo(_weapon.projectileTrailColor.a).Within(0.003f));

        var block = new MaterialPropertyBlock();
        Renderer renderer = projectile.ActiveShape.GetComponentInChildren<Renderer>();
        Assert.That(renderer, Is.Not.Null);
        renderer.GetPropertyBlock(block);
        Assert.That(block.isEmpty, Is.False);
        Assert.That(block.GetColor("_BaseColor"), Is.EqualTo(Color.magenta));
        Assert.That(block.GetColor("_EmissionColor"), Is.EqualTo(Color.cyan));
        Assert.That(block.GetFloat("_EmissionStrength"), Is.EqualTo(1.25f));
    }

    [Test]
    public void RebuildsGeneratedJugglingBallShapeWhenSerializedReferenceWasDestroyed()
    {
        _weapon.ApplyDefaults(FirstPartyWeaponDefaults.GetOrDefault(FirstPartyWeaponDefaults.JugglingBallId));
        ProjectileView projectile = TheCircussyOneTestObjects.CreateProjectilePrefab();
        projectile.ApplyVisuals(_weapon);
        Transform generatedShape = projectile.ActiveShape;

        Object.DestroyImmediate(generatedShape.gameObject);

        Assert.DoesNotThrow(() => projectile.ApplyVisuals(_weapon));
        Assert.That(projectile.ActiveShape, Is.Not.Null);
        Assert.That(projectile.ActiveShape.name, Is.EqualTo("Juggling Ball"));
    }
}
