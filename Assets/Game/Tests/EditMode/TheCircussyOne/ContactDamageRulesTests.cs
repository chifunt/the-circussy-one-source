using NUnit.Framework;
using TheCircussyOne.Rules;

public sealed class ContactDamageRulesTests
{
    [Test]
    public void CanDamageOnlyInsideRangeAfterCooldown()
    {
        Assert.That(ContactDamageRules.CanDamage(distance: 0.9f, range: 1f, currentTime: 2f, nextAllowedTime: 1.5f), Is.True);
        Assert.That(ContactDamageRules.CanDamage(distance: 1.1f, range: 1f, currentTime: 2f, nextAllowedTime: 1.5f), Is.False);
        Assert.That(ContactDamageRules.CanDamage(distance: 0.9f, range: 1f, currentTime: 1f, nextAllowedTime: 1.5f), Is.False);
    }

    [Test]
    public void CanDamageOnlyWhenOverlappingAfterCooldown()
    {
        Assert.That(ContactDamageRules.CanDamage(isOverlapping: true, currentTime: 2f, nextAllowedTime: 1.5f), Is.True);
        Assert.That(ContactDamageRules.CanDamage(isOverlapping: false, currentTime: 2f, nextAllowedTime: 1.5f), Is.False);
        Assert.That(ContactDamageRules.CanDamage(isOverlapping: true, currentTime: 1f, nextAllowedTime: 1.5f), Is.False);
    }
}
