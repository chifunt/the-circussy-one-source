using NUnit.Framework;
using UnityEngine;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;

public sealed class WeaponCooldownRulesTests
{
    [Test]
    public void FireIntervalClampsBaseAtMinimum()
    {
        Assert.That(WeaponCooldownRules.FireInterval(0.2f), Is.EqualTo(0.2f));
        Assert.That(WeaponCooldownRules.FireInterval(0.08f), Is.EqualTo(0.12f));
    }

    [Test]
    public void WeaponDefinitionFireIntervalUsesBaseAndPerWeaponMinimum()
    {
        var weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
        weapon.baseFireIntervalSeconds = 0.3f;
        weapon.minimumFireIntervalSeconds = 0.18f;

        Assert.That(WeaponCooldownRules.FireInterval(weapon), Is.EqualTo(0.3f));

        Object.DestroyImmediate(weapon);
    }

    [Test]
    public void TickAllowsFireAtOrBelowZero()
    {
        float remaining = WeaponCooldownRules.Tick(0.1f, 0.11f);

        Assert.That(WeaponCooldownRules.CanFire(remaining), Is.True);
    }
}
