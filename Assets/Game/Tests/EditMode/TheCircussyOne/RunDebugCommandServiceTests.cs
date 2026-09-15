using System.Linq;
using NUnit.Framework;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Runtime;
using TheCircussyOne.Stats;
using UnityEngine;

public sealed class RunDebugCommandServiceTests
{
    [TearDown]
    public void TearDown()
    {
        TheCircussyOneTestObjects.DestroyAll();
    }

    [Test]
    public void DebugCommandsGrantResourcesWeaponsItemsTalentsAndUpgrades()
    {
        GameConfig config = TheCircussyOneTestObjects.CreateConfig();
        var stats = new RunStats(config);
        var state = new GameState(config, stats);
        var tickets = new RunCurrencyState();

        WeaponDefinition weapon = TheCircussyOneTestObjects.CreateWeaponDefinition(config, "knife_fan", "Knife Fan");
        weapon.supportedUpgradeStats.Clear();
        weapon.supportedUpgradeStats.Add(StatId.WeaponFlatDamage);
        var weaponCatalog = ScriptableObject.CreateInstance<WeaponCatalog>();
        weaponCatalog.availableWeapons.Add(weapon);
        var loadout = new WeaponLoadout(weaponCatalog);

        ItemDefinition item = TheCircussyOneTestObjects.CreateItemDefinition(
            "rubber_soles",
            "Rubber Soles",
            modifiers: new[] { new ItemStatModifierDefinition(StatId.PlayerArmor, StatModifierBucket.Flat, 2f) });
        ItemCatalog itemCatalog = TheCircussyOneTestObjects.CreateItemCatalog(item);
        var inventory = new ItemInventory(stats);

        TalentDefinition talent = TheCircussyOneTestObjects.CreateTalentDefinition(
            "steady_step",
            "Steady Step",
            modifiers: new[] { new UpgradeStatModifierDefinition(StatId.PlayerArmor, StatModifierBucket.Flat, 1f) });
        TalentCatalog talentCatalog = TheCircussyOneTestObjects.CreateTalentCatalog(talent);
        var talentRunState = new TalentRunState();
        var talentApplier = new TalentEffectApplier(talentRunState, stats, state);

        var upgrade = ScriptableObject.CreateInstance<UpgradeDefinition>();
        upgrade.ApplyWeaponStatUpgradeDefaults(
            weapon,
            "damage",
            "Damage",
            "Debug damage.",
            Color.white,
            new UpgradeStatModifierDefinition(StatId.WeaponFlatDamage, StatModifierBucket.Flat, 1f));
        UpgradeCatalog upgradeCatalog = TheCircussyOneTestObjects.CreateUpgradeCatalog(upgrade);
        var upgradeRunState = new UpgradeRunState();
        var upgradeApplier = new UpgradeEffectApplier(upgradeRunState, stats, loadout, state, inventory, talentApplier);

        var service = new RunDebugCommandService(
            weaponCatalog,
            itemCatalog,
            talentCatalog,
            upgradeCatalog,
            loadout,
            inventory,
            tickets,
            state,
            upgradeRunState,
            talentApplier,
            upgradeApplier);

        Assert.That(service.AddTickets(35), Is.True);
        Assert.That(tickets.Tickets, Is.EqualTo(35));
        Assert.That(service.AddExperience(5), Is.True);
        Assert.That(state.Experience, Is.EqualTo(5));

        state.DamagePlayer(25);
        int damagedHealth = state.Health;
        Assert.That(service.HealPlayer(10), Is.True);
        Assert.That(state.Health, Is.GreaterThan(damagedHealth));

        Assert.That(service.AddWeapon(weapon.Id), Is.True);
        Assert.That(loadout.OwnsWeapon(weapon.Id), Is.True);
        Assert.That(service.ApplyUpgrade(upgrade.Id, ContentRarity.Common), Is.True);
        Assert.That(loadout.FindById(weapon.Id).Modifiers.Any(modifier => modifier.SourceId == $"upgrade:{upgrade.Id}:1"), Is.True);

        Assert.That(service.AddItem(item.Id, 2), Is.True);
        Assert.That(inventory.FindById(item.Id).StackCount, Is.EqualTo(2));
        Assert.That(service.RemoveItem(item.Id, 1), Is.True);
        Assert.That(inventory.FindById(item.Id).StackCount, Is.EqualTo(1));

        Assert.That(service.ApplyTalent(talent.Id, ContentRarity.Common), Is.True);
        Assert.That(talentRunState.GetLevel(talent), Is.EqualTo(1));
        Assert.That(stats.Modifiers.Any(modifier => modifier.SourceId == $"talent:{talent.Id}:1"), Is.True);

        Assert.That(service.RemoveWeapon(weapon.Id), Is.True);
        Assert.That(loadout.OwnsWeapon(weapon.Id), Is.False);
        Assert.That(service.ApplyUpgrade(upgrade.Id, ContentRarity.Common), Is.False);

        TheCircussyOneTestObjects.Destroy(upgrade);
        TheCircussyOneTestObjects.Destroy(upgradeCatalog);
        TheCircussyOneTestObjects.Destroy(talent);
        TheCircussyOneTestObjects.Destroy(talentCatalog);
        TheCircussyOneTestObjects.Destroy(item);
        TheCircussyOneTestObjects.Destroy(itemCatalog);
        TheCircussyOneTestObjects.Destroy(weapon);
        TheCircussyOneTestObjects.Destroy(weaponCatalog);
        TheCircussyOneTestObjects.Destroy(config);
    }
}
