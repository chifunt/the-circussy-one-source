using System.Linq;
using NUnit.Framework;
using TheCircussyOne.Content;
using UnityEngine;

public sealed class HealthPickupDefinitionTests
{
    [TearDown]
    public void TearDown()
    {
        TheCircussyOneTestObjects.DestroyAll();
    }

    [Test]
    public void HealthPickupDefinitionDefaultsExposeHealingIdentity()
    {
        HealthPickupDefinition pickup = TheCircussyOneTestObjects.CreateHealthPickupDefinition(
            "treat",
            "Treat",
            healAmount: 20);

        Assert.That(pickup.Id, Is.EqualTo("treat"));
        Assert.That(pickup.DisplayName, Is.EqualTo("Treat"));
        Assert.That(pickup.Tags.HasTag(ContentTag.Health), Is.True);
        Assert.That(pickup.Tags.HasTag(ContentTag.Pickup), Is.True);
        Assert.That(pickup.healPercentOfMaxHealth, Is.EqualTo(0.2f).Within(0.001f));
        Assert.That(pickup.ResolveHealAmount(100), Is.EqualTo(20));
        Assert.That(pickup.ResolveHealAmount(112), Is.EqualTo(23));
        Assert.That(pickup.visualShape, Is.EqualTo(PickupVisualShape.Heart));

        Object.DestroyImmediate(pickup);
    }

    [Test]
    public void HealthPickupCatalogValidatesInvalidDefinitions()
    {
        HealthPickupDefinition first = TheCircussyOneTestObjects.CreateHealthPickupDefinition("heart", "Heart");
        HealthPickupDefinition duplicate = TheCircussyOneTestObjects.CreateHealthPickupDefinition("heart", "Duplicate Heart");
        HealthPickupDefinition invalid = TheCircussyOneTestObjects.CreateHealthPickupDefinition("", "");
        invalid.pickupId = "";
        invalid.displayName = "";
        invalid.healPercentOfMaxHealth = 0f;
        invalid.visualScale = -1f;
        invalid.emissionStrength = -1f;
        invalid.visualShape = (PickupVisualShape)999;
        HealthPickupCatalog catalog = TheCircussyOneTestObjects.CreateHealthPickupCatalog(first, duplicate, invalid, null);

        var issues = catalog.ValidateContent();

        Assert.That(issues.Any(issue => issue.Code == "catalog.null-entry"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "content.duplicate-id"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "content.invalid-id"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "content.missing-display-name"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "health-pickup.non-positive-heal"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "health-pickup.non-positive-scale"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "health-pickup.negative-emission"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "health-pickup.invalid-shape"), Is.True);

        Object.DestroyImmediate(catalog);
        Object.DestroyImmediate(first);
        Object.DestroyImmediate(duplicate);
        Object.DestroyImmediate(invalid);
    }

    [Test]
    public void HealthPickupCatalogResolvesDefaultOrFirstValidActivePickup()
    {
        HealthPickupDefinition inactive = TheCircussyOneTestObjects.CreateHealthPickupDefinition("inactive_heart", "Inactive Heart");
        inactive.isActive = false;
        HealthPickupDefinition active = TheCircussyOneTestObjects.CreateHealthPickupDefinition("active_heart", "Active Heart");
        HealthPickupCatalog catalog = TheCircussyOneTestObjects.CreateHealthPickupCatalog(inactive, active);
        catalog.defaultHealthPickup = inactive;

        Assert.That(catalog.ResolveDefault(), Is.SameAs(active));

        Object.DestroyImmediate(catalog);
        Object.DestroyImmediate(inactive);
        Object.DestroyImmediate(active);
    }
}
