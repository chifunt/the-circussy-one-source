using NUnit.Framework;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using TheCircussyOne.Content;
using TheCircussyOne.Stats;

public sealed class ConfigAuthoringUxTests
{
    [Test]
    public void ModifierDisplayFormatsUpgradeAndRarityScaledValues()
    {
        var modifier = new UpgradeStatModifierDefinition(StatId.GlobalDamageMultiplier, StatModifierBucket.AdditivePercent, 0.05f);

        Assert.That(ContentModifierDisplayRules.Summary(modifier), Is.EqualTo("Damage +5%"));
        Assert.That(ContentModifierDisplayRules.RarityPreview(modifier), Does.Contain("Common +5%"));
        Assert.That(ContentModifierDisplayRules.RarityPreview(modifier), Does.Contain("Legendary +10%"));
    }

    [Test]
    public void ModifierDisplayFormatsItemValues()
    {
        var modifier = new ItemStatModifierDefinition(StatId.PlayerArmor, StatModifierBucket.Flat, 3f);

        Assert.That(ContentModifierDisplayRules.Summary(modifier), Is.EqualTo("Armor +3"));
    }

    [Test]
    public void ModifierDisplayReportsInvalidStats()
    {
        var modifier = new UpgradeStatModifierDefinition((StatId)999, StatModifierBucket.Flat, 1f);

        Assert.That(ContentModifierDisplayRules.Summary(modifier), Is.EqualTo("Invalid stat (999)"));
        Assert.That(ContentModifierDisplayRules.RarityPreview(modifier), Is.EqualTo("Invalid stat (999)"));
    }

    [Test]
    public void ExplicitRarityValueFieldsOnlyShowWhenExplicitValuesAreEnabled()
    {
        AssertRarityValueFieldUsesExplicitToggle(nameof(UpgradeStatModifierDefinition.uncommonValue));
        AssertRarityValueFieldUsesExplicitToggle(nameof(UpgradeStatModifierDefinition.rareValue));
        AssertRarityValueFieldUsesExplicitToggle(nameof(UpgradeStatModifierDefinition.epicValue));
        AssertRarityValueFieldUsesExplicitToggle(nameof(UpgradeStatModifierDefinition.legendaryValue));
    }

    [Test]
    public void WeaponAuthoringApplicabilityDescribesOrbitAndInactiveProjectileStats()
    {
        var weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
        weapon.weaponId = "test_orbit";
        weapon.displayName = "Test Orbit";
        weapon.orbitEnabled = true;
        weapon.orbitAreaDamageEnabled = true;
        weapon.supportedUpgradeStats.Clear();
        weapon.supportedUpgradeStats.Add(StatId.WeaponFlatDamage);
        weapon.supportedUpgradeStats.Add(StatId.WeaponProjectileSpeedMultiplier);

        Assert.That(WeaponAuthoringApplicability.FamilySummary(weapon), Is.EqualTo("Orbit + Area Heat"));
        Assert.That(WeaponAuthoringApplicability.ActiveGameplayHooksSummary(weapon), Does.Contain("Orbit damage"));
        Assert.That(WeaponAuthoringApplicability.ActiveGameplayHooksSummary(weapon), Does.Contain("Orbit area heat"));
        Assert.That(WeaponAuthoringApplicability.SupportedUpgradeStatsSummary(weapon), Does.Contain("Weapon Projectile Speed (inactive)"));
        Assert.That(WeaponAuthoringApplicability.InactiveSupportedUpgradeStatsWarning(weapon), Does.Contain("orbit weapons do not use projectile speed"));

        Object.DestroyImmediate(weapon);
    }

    [Test]
    public void WeaponAuthoringApplicabilityDescribesImplementedProjectileFamilies()
    {
        var cannon = ScriptableObject.CreateInstance<WeaponDefinition>();
        cannon.explosiveEnabled = true;
        cannon.projectileTrajectoryMode = ProjectileTrajectoryMode.Arc;

        var knife = ScriptableObject.CreateInstance<WeaponDefinition>();
        knife.baseProjectileCount = 3;
        knife.baseSpreadAngleDegrees = 16f;

        Assert.That(WeaponAuthoringApplicability.FamilySummary(cannon), Is.EqualTo("Explosive Arc Projectile"));
        Assert.That(WeaponAuthoringApplicability.FamilySummary(knife), Is.EqualTo("Spread Projectile"));

        Object.DestroyImmediate(cannon);
        Object.DestroyImmediate(knife);
    }

    [Test]
    public void WeaponTimingAuthoringUsesCompactEditableLabels()
    {
        AssertWeaponLabelWidthAtMost(nameof(WeaponDefinition.baseFireIntervalSeconds), 140f);
        AssertWeaponLabelWidthAtMost(nameof(WeaponDefinition.minimumFireIntervalSeconds), 140f);
        AssertWeaponLabelText(nameof(WeaponDefinition.baseFireIntervalSeconds), "Base Interval");
        AssertWeaponLabelText(nameof(WeaponDefinition.minimumFireIntervalSeconds), "Minimum Interval");
    }

    [Test]
    public void WeaponAuthoringFieldsAvoidWideEmbeddedWorkbenchLabels()
    {
        FieldInfo[] fields = typeof(WeaponDefinition).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        foreach (FieldInfo field in fields)
        {
            LabelWidthAttribute labelWidth = field.GetCustomAttribute<LabelWidthAttribute>();
            if (labelWidth == null)
            {
                continue;
            }

            Assert.That(LabelWidthValue(labelWidth), Is.LessThanOrEqualTo(140f), field.Name);
        }
    }

    [Test]
    public void EnemyAuthoringApplicabilityDisablesInactiveModes()
    {
        var climb = new EnemyClimbProfile
        {
            canClimbEnvironment = false,
            useMoveSpeedForClimb = true
        };
        var stack = new EnemyStackProfile
        {
            policy = EnemyStackPolicy.GroundOnly
        };

        Assert.That(EnemyAuthoringApplicability.IsEnvironmentClimbingActive(climb), Is.False);
        Assert.That(EnemyAuthoringApplicability.EnvironmentClimbingDisabledReason(climb), Does.Contain("cannot climb environment"));
        Assert.That(EnemyAuthoringApplicability.IsSupportStackingActive(stack), Is.False);
        Assert.That(EnemyAuthoringApplicability.SupportStackingDisabledReason(stack), Does.Contain("GroundOnly"));
    }

    [Test]
    public void EnemyAuthoringApplicabilitySummarizesActiveSupportStacking()
    {
        var stack = new EnemyStackProfile
        {
            policy = EnemyStackPolicy.SupportBased,
            pileRadius = 2.4f,
            pileStartCount = 6,
            maxStackLayers = 4,
            layerHeightMultiplier = 1f
        };

        Assert.That(EnemyAuthoringApplicability.IsSupportStackingActive(stack), Is.True);
        Assert.That(EnemyAuthoringApplicability.SupportStackingDisabledReason(stack), Is.Empty);
        Assert.That(EnemyAuthoringApplicability.StackSummary(stack), Does.Contain("SupportBased"));
        Assert.That(EnemyAuthoringApplicability.StackSummary(stack), Does.Contain("max layers 4"));
    }

    [Test]
    public void CustomWeaponCreatorShowsTemplatePreview()
    {
        var creator = new TheCircussyOneCustomWeaponCreator
        {
            template = WeaponFamilyTemplate.Orbit
        };

        Assert.That(creator.TemplatePreview, Does.Contain("Persistent orbit family"));
        Assert.That(creator.TemplatePreview, Does.Contain("Level-Up Card"));
    }

    [Test]
    public void FirstPartyExistingWeaponRepairRestoresCanonicalSupportedStats()
    {
        FirstPartyWeaponSpec spec = FirstPartyWeaponDefaults.GetOrDefault(FirstPartyWeaponDefaults.CannonId);
        var weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
        weapon.ApplyDefaults(spec);
        weapon.supportedUpgradeStats.Clear();
        weapon.supportedUpgradeStats.Add(StatId.WeaponBounce);
        weapon.supportedUpgradeStats.Add(StatId.WeaponFlatDamage);

        bool changed = (bool)InvokeWeaponStarterContent("ApplyFirstPartySupportedStatsForTests", weapon, spec);

        Assert.That(changed, Is.True);
        Assert.That(weapon.SupportedUpgradeStats, Is.EqualTo(spec.SupportedUpgradeStats));

        Object.DestroyImmediate(weapon);
    }

    [Test]
    public void FirstPartyExistingUpgradeRepairFixesStatIdentityWithoutClobberingValue()
    {
        FirstPartyWeaponSpec spec = FirstPartyWeaponDefaults.GetOrDefault(FirstPartyWeaponDefaults.CannonId);
        var weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
        weapon.ApplyDefaults(spec);

        var upgrade = ScriptableObject.CreateInstance<UpgradeDefinition>();
        upgrade.ApplyWeaponStatUpgradeDefaults(
            weapon,
            "blast_radius",
            "Blast Radius",
            "Old text.",
            Color.white,
            new UpgradeStatModifierDefinition(StatId.WeaponKnockbackMultiplier, StatModifierBucket.AdditivePercent, 0.17f));

        bool changed = (bool)InvokeWeaponStarterContent(
            "ApplyWeaponTrackUpgradeDefaultsForTests",
            upgrade,
            weapon,
            "blast_radius",
            "Blast Radius",
            "Pack the cannon for a finale that fills the ring.",
            Color.red,
            new UpgradeStatModifierDefinition(StatId.WeaponSplashRadiusMultiplier, StatModifierBucket.AdditivePercent, 0.1f));

        Assert.That(changed, Is.True);
        Assert.That(upgrade.weaponId, Is.EqualTo("cannon"));
        Assert.That(upgrade.weaponDefinition, Is.SameAs(weapon));
        Assert.That(upgrade.statModifiers, Has.Count.EqualTo(1));
        Assert.That(upgrade.statModifiers[0].statId, Is.EqualTo(StatId.WeaponSplashRadiusMultiplier));
        Assert.That(upgrade.statModifiers[0].bucket, Is.EqualTo(StatModifierBucket.AdditivePercent));
        Assert.That(upgrade.statModifiers[0].value, Is.EqualTo(0.17f).Within(0.0001f));

        Object.DestroyImmediate(upgrade);
        Object.DestroyImmediate(weapon);
    }

    [Test]
    public void FirstPartyWeaponTrackUpgradesCoverEverySupportedWeaponStat()
    {
        var coveredStatsByWeapon = new Dictionary<string, HashSet<StatId>>();
        foreach (object upgradeSpec in WeaponTrackUpgradeSpecs())
        {
            PropertyInfo weaponIdProperty = upgradeSpec.GetType().GetProperty("WeaponId", BindingFlags.Instance | BindingFlags.Public);
            PropertyInfo modifierProperty = upgradeSpec.GetType().GetProperty("Modifier", BindingFlags.Instance | BindingFlags.Public);
            Assert.That(weaponIdProperty, Is.Not.Null);
            Assert.That(modifierProperty, Is.Not.Null);

            string weaponId = weaponIdProperty.GetValue(upgradeSpec) as string;
            var modifier = (UpgradeStatModifierDefinition)modifierProperty.GetValue(upgradeSpec);
            if (!coveredStatsByWeapon.TryGetValue(weaponId, out HashSet<StatId> coveredStats))
            {
                coveredStats = new HashSet<StatId>();
                coveredStatsByWeapon.Add(weaponId, coveredStats);
            }

            coveredStats.Add(modifier.statId);
        }

        foreach (FirstPartyWeaponSpec weaponSpec in FirstPartyWeaponDefaults.All)
        {
            Assert.That(coveredStatsByWeapon.TryGetValue(weaponSpec.Id, out HashSet<StatId> coveredStats), Is.True, weaponSpec.Id);
            foreach (StatId supportedStat in weaponSpec.SupportedUpgradeStats)
            {
                Assert.That(coveredStats, Does.Contain(supportedStat), $"{weaponSpec.Id} missing upgrade for {supportedStat}");
            }
        }
    }

    [Test]
    public void FirstPartyLevelUpDescriptionsUseFlavorCopy()
    {
        foreach (string description in WeaponTrackUpgradeDescriptions())
        {
            AssertFlavorDescription(description);
        }

        foreach (string talentId in StarterTalentDefaults.AllIds)
        {
            TalentDefinition talent = StarterTalentDefaults.Create(talentId);
            AssertFlavorDescription(talent.shortDescription);
            Object.DestroyImmediate(talent);
        }

        foreach (string talentId in StarterPerformerDefaults.TalentIds)
        {
            TalentDefinition talent = ScriptableObject.CreateInstance<TalentDefinition>();
            StarterPerformerDefaults.ApplyTalent(talent, talentId, StarterPerformerDefaults.JugglerId);
            AssertFlavorDescription(talent.shortDescription);
            Object.DestroyImmediate(talent);
        }
    }

    [Test]
    public void AuthoredLevelUpAssetsUseFlavorCopy()
    {
        AssertFlavorAssetDescriptions("Assets/Game/ScriptableObjects/Balance/Upgrades");
        AssertFlavorAssetDescriptions("Assets/Game/ScriptableObjects/Balance/Talents");
    }

    private static object InvokeWeaponStarterContent(string methodName, params object[] arguments)
    {
        System.Type type = typeof(TheCircussyOneConfigRepository).Assembly.GetType("TheCircussyOneWeaponStarterContent");
        MethodInfo method = type?.GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        Assert.That(method, Is.Not.Null, methodName);
        return method.Invoke(null, arguments);
    }

    private static IEnumerable WeaponTrackUpgradeDescriptions()
    {
        foreach (object spec in WeaponTrackUpgradeSpecs())
        {
            PropertyInfo description = spec.GetType().GetProperty("Description", BindingFlags.Instance | BindingFlags.Public);
            Assert.That(description, Is.Not.Null);
            yield return description.GetValue(spec) as string;
        }
    }

    private static IEnumerable WeaponTrackUpgradeSpecs()
    {
        System.Type type = typeof(TheCircussyOneConfigRepository).Assembly.GetType("TheCircussyOneWeaponStarterContent");
        FieldInfo field = type?.GetField("WeaponTrackUpgrades", BindingFlags.Static | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null);
        IEnumerable specs = field.GetValue(null) as IEnumerable;
        Assert.That(specs, Is.Not.Null);
        foreach (object spec in specs)
        {
            yield return spec;
        }
    }

    private static void AssertFlavorDescription(string description)
    {
        Assert.That(description, Is.Not.Null.And.Not.Empty);
        Assert.That(ContentCatalogRules.LooksLikeMechanicalLevelUpDescription(description), Is.False, description);
    }

    private static void AssertFlavorAssetDescriptions(string folder)
    {
        foreach (string path in Directory.GetFiles(folder, "*.asset"))
        {
            foreach (string line in File.ReadLines(path))
            {
                if (!line.StartsWith("  shortDescription:"))
                {
                    continue;
                }

                string description = line["  shortDescription:".Length..].Trim();
                if (!string.IsNullOrWhiteSpace(description))
                {
                    AssertFlavorDescription(description);
                }
            }
        }
    }

    private static void AssertRarityValueFieldUsesExplicitToggle(string fieldName)
    {
        FieldInfo field = typeof(UpgradeStatModifierDefinition).GetField(fieldName);
        ShowIfAttribute showIf = field?.GetCustomAttribute<ShowIfAttribute>();

        Assert.That(field, Is.Not.Null, fieldName);
        Assert.That(showIf, Is.Not.Null, fieldName);
        Assert.That(showIf.Condition, Is.EqualTo(nameof(UpgradeStatModifierDefinition.useExplicitRarityValues)));
    }

    private static void AssertWeaponLabelWidthAtMost(string fieldName, float maximum)
    {
        FieldInfo field = typeof(WeaponDefinition).GetField(fieldName);
        LabelWidthAttribute labelWidth = field?.GetCustomAttribute<LabelWidthAttribute>();

        Assert.That(field, Is.Not.Null, fieldName);
        Assert.That(labelWidth, Is.Not.Null, fieldName);
        Assert.That(LabelWidthValue(labelWidth), Is.LessThanOrEqualTo(maximum), fieldName);
    }

    private static void AssertWeaponLabelText(string fieldName, string expected)
    {
        FieldInfo field = typeof(WeaponDefinition).GetField(fieldName);
        LabelTextAttribute labelText = field?.GetCustomAttribute<LabelTextAttribute>();

        Assert.That(field, Is.Not.Null, fieldName);
        Assert.That(labelText, Is.Not.Null, fieldName);
        Assert.That(labelText.Text, Is.EqualTo(expected), fieldName);
    }

    private static float LabelWidthValue(LabelWidthAttribute labelWidth)
    {
        FieldInfo[] fields = labelWidth.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        for (int i = 0; i < fields.Length; i++)
        {
            FieldInfo field = fields[i];
            if (field.Name == "Width" || field.FieldType == typeof(float) || field.FieldType == typeof(double))
            {
                return System.Convert.ToSingle(field.GetValue(labelWidth));
            }
        }

        Assert.Fail("LabelWidthAttribute width field was not found.");
        return 0f;
    }
}
