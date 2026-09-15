using System.Collections.Generic;
using TheCircussyOne.Content;
using TheCircussyOne.Runtime;
using TheCircussyOne.Stats;
using UnityEngine;

namespace TheCircussyOne.Rules
{
    public readonly struct UpgradePreviewFrame
    {
        public UpgradePreviewFrame(string text, bool beneficialChange)
        {
            Text = text ?? string.Empty;
            BeneficialChange = beneficialChange;
        }

        public string Text { get; }
        public bool BeneficialChange { get; }
    }

    public static class UpgradePreviewRules
    {
        private const string PositiveColorHex = "#73F59A";
        private delegate float WeaponFloatEvaluator(WeaponDefinition weapon, RunStats stats, IReadOnlyList<StatModifier> modifiers);

        public static UpgradePreviewFrame BuildPreview(
            UpgradeChoice choice,
            RunStats stats,
            WeaponLoadout loadout,
            ItemInventory itemInventory = null)
        {
            if (choice.Kind == UpgradeChoiceKind.Talent)
            {
                return BuildTalentPreview(choice.Talent, choice.Rarity, stats);
            }

            UpgradeDefinition definition = choice.Definition;
            if (choice.Kind == UpgradeChoiceKind.AddWeapon)
            {
                return BuildAddWeaponPreview(choice.WeaponToAdd, loadout);
            }

            if (definition == null)
            {
                return new UpgradePreviewFrame(string.Empty, false);
            }

            return BuildWeaponStatPreview(definition, choice.Rarity, stats, loadout);
        }

        private static UpgradePreviewFrame BuildTalentPreview(TalentDefinition talent, ContentRarity rarity, RunStats stats)
        {
            if (talent == null)
            {
                return new UpgradePreviewFrame(string.Empty, false);
            }

            if (talent.statModifiers == null || talent.statModifiers.Count == 0)
            {
                return new UpgradePreviewFrame(talent.shortDescription, false);
            }

            return BuildStatPreview(talent.statModifiers[0], rarity, talent.shortDescription, stats, "talent_preview");
        }

        private static UpgradePreviewFrame BuildStatPreview(
            UpgradeStatModifierDefinition modifierDefinition,
            ContentRarity rarity,
            string fallbackDescription,
            RunStats stats,
            string sourceId)
        {
            StatModifier modifier = modifierDefinition.ToRuntime(sourceId, rarity);
            StatDefinition stat = StatMetadata.Get(modifier.StatId);
            float current = stats != null ? stats.GetFloat(stat.Id) : 0f;
            float next = stats != null ? stats.EvaluateFloatWithModifier(modifier) : current + modifier.Value;
            bool beneficial = next > current;

            string currentText = FormatStatValue(stat, current, StatValueDisplayMode.Preview);
            string nextText = FormatStatValue(stat, next, StatValueDisplayMode.Preview);
            return new UpgradePreviewFrame(
                string.IsNullOrWhiteSpace(stat.DisplayName)
                    ? fallbackDescription
                    : $"{stat.DisplayName} {currentText} -> {Colorize(nextText, beneficial)}",
                beneficial);
        }

        public static IReadOnlyList<UpgradePreviewFrame> BuildPreviews(
            IReadOnlyList<UpgradeChoice> choices,
            RunStats stats,
            WeaponLoadout loadout,
            ItemInventory itemInventory = null)
        {
            var frames = new List<UpgradePreviewFrame>(choices?.Count ?? 0);
            if (choices == null)
            {
                return frames;
            }

            for (int i = 0; i < choices.Count; i++)
            {
                frames.Add(BuildPreview(choices[i], stats, loadout, itemInventory));
            }

            return frames;
        }

        private static UpgradePreviewFrame BuildWeaponStatPreview(UpgradeDefinition definition, ContentRarity rarity, RunStats stats, WeaponLoadout loadout)
        {
            WeaponRuntime weapon = loadout?.FindById(definition.weaponId);
            if (weapon == null || weapon.Definition == null)
            {
                return new UpgradePreviewFrame(definition.shortDescription, false);
            }

            if (definition.statModifiers == null || definition.statModifiers.Count == 0)
            {
                return new UpgradePreviewFrame(definition.shortDescription, false);
            }

            StatModifier previewModifier = definition.statModifiers[0].ToRuntime("upgrade_preview", rarity);
            var previewModifiers = new List<StatModifier>(weapon.Modifiers.Count + 1);
            previewModifiers.AddRange(weapon.Modifiers);
            previewModifiers.Add(previewModifier);

            if (previewModifier.StatId == StatId.GlobalWeaponHaste)
            {
                return BuildWeaponAttackSpeedPreview(weapon, stats, previewModifiers);
            }

            if (previewModifier.StatId == StatId.ProjectileSpeedMultiplier)
            {
                return BuildWeaponNormalizedPreview(
                    weapon,
                    stats,
                    previewModifiers,
                    previewModifier,
                    "Projectile Speed",
                    StatId.ProjectileSpeedMultiplier,
                    (definition, runStats, modifiers) => WeaponStatRules.ProjectileSpeed(definition, runStats, modifiers),
                    weapon.Definition.projectileSpeed);
            }

            if (previewModifier.StatId == StatId.GlobalDamageMultiplier)
            {
                return BuildWeaponNormalizedPreview(
                    weapon,
                    stats,
                    previewModifiers,
                    previewModifier,
                    "Damage",
                    StatId.GlobalDamageMultiplier,
                    WeaponDamagePreview,
                    weapon.Definition.projectileDamage);
            }

            if (previewModifier.StatId == StatId.WeaponAttackSpeed)
            {
                return BuildWeaponAttackSpeedPreview(weapon, stats, previewModifiers);
            }

            if (previewModifier.StatId == StatId.WeaponProjectileSpeedMultiplier)
            {
                return BuildWeaponNormalizedPreview(
                    weapon,
                    stats,
                    previewModifiers,
                    previewModifier,
                    "Projectile Speed",
                    StatId.WeaponProjectileSpeedMultiplier,
                    (definition, runStats, modifiers) => WeaponStatRules.ProjectileSpeed(definition, runStats, modifiers),
                    weapon.Definition.projectileSpeed);
            }

            if (previewModifier.StatId == StatId.WeaponProjectileSizeMultiplier)
            {
                if (weapon.Definition.orbitEnabled)
                {
                    return BuildWeaponFloatPreview(
                        weapon,
                        stats,
                        previewModifiers,
                        "Hoop Hit Radius",
                        previewModifier,
                        StatId.WeaponProjectileSizeMultiplier,
                        (definition, runStats, modifiers) => WeaponStatRules.OrbitHitRadius(definition, runStats, modifiers),
                        weapon.Definition.orbitHitRadius);
                }

                return BuildWeaponFloatPreview(
                    weapon,
                    stats,
                    previewModifiers,
                    "Projectile Size",
                    previewModifier,
                    StatId.WeaponProjectileSizeMultiplier,
                    (definition, runStats, modifiers) => WeaponStatRules.ProjectileHitRadius(definition, runStats, modifiers),
                    weapon.Definition.projectileHitRadius);
            }

            if (previewModifier.StatId == StatId.WeaponProjectileCount)
            {
                if (weapon.Definition.orbitEnabled)
                {
                    return BuildWeaponIntPreview(
                        weapon,
                        stats,
                        previewModifiers,
                        "Orbit Count",
                        StatId.WeaponProjectileCount,
                        OrbitCountPreview);
                }

                return BuildWeaponIntPreview(
                    weapon,
                    stats,
                    previewModifiers,
                    "Projectile Count",
                    StatId.WeaponProjectileCount,
                    ProjectileCountPreview);
            }

            if (previewModifier.StatId == StatId.WeaponProjectileLifetimeMultiplier)
            {
                return BuildWeaponFloatPreview(
                    weapon,
                    stats,
                    previewModifiers,
                    "Projectile Lifetime",
                    previewModifier,
                    StatId.WeaponProjectileLifetimeMultiplier,
                    (definition, runStats, modifiers) => WeaponStatRules.ProjectileLifetime(definition, runStats, modifiers),
                    weapon.Definition.projectileLifetimeSeconds);
            }

            if (previewModifier.StatId == StatId.WeaponRangeMultiplier)
            {
                if (weapon.Definition.orbitEnabled)
                {
                    return BuildWeaponFloatPreview(
                        weapon,
                        stats,
                        previewModifiers,
                        "Orbit Radius",
                        previewModifier,
                        StatId.WeaponRangeMultiplier,
                        (definition, _, modifiers) => WeaponStatRules.OrbitRadius(definition, modifiers),
                        weapon.Definition.orbitRadius);
                }

                return BuildWeaponFloatPreview(
                    weapon,
                    stats,
                    previewModifiers,
                    "Range",
                    previewModifier,
                    StatId.WeaponRangeMultiplier,
                    (definition, _, modifiers) => WeaponStatRules.Range(definition, modifiers),
                    weapon.Definition.weaponRange);
            }

            if (previewModifier.StatId == StatId.WeaponSplashRadiusMultiplier)
            {
                return BuildWeaponFloatPreview(
                    weapon,
                    stats,
                    previewModifiers,
                    "Splash Radius",
                    previewModifier,
                    StatId.WeaponSplashRadiusMultiplier,
                    (definition, runStats, modifiers) => WeaponStatRules.SplashRadius(definition, runStats, modifiers),
                    weapon.Definition.baseSplashRadius);
            }

            if (previewModifier.StatId == StatId.WeaponBounce)
            {
                return BuildWeaponIntPreview(
                    weapon,
                    stats,
                    previewModifiers,
                    "Bounce",
                    StatId.WeaponBounce,
                    BounceCountPreview);
            }

            if (previewModifier.StatId == StatId.WeaponAccuracyMultiplier)
            {
                return BuildWeaponMultiplierPreview(
                    weapon,
                    previewModifiers,
                    "Accuracy",
                    StatId.WeaponAccuracyMultiplier);
            }

            if (previewModifier.StatId == StatId.WeaponFlatDamage)
            {
                int current = WeaponStatRules.Damage(weapon.Definition, stats, weapon.Modifiers);
                int next = WeaponStatRules.Damage(weapon.Definition, stats, previewModifiers);
                bool beneficial = next > current;
                return new UpgradePreviewFrame(
                    $"{weapon.Definition.DisplayName}\nDamage {current} -> {Colorize(next.ToString(), beneficial)}",
                    beneficial);
            }

            if (previewModifier.StatId == StatId.WeaponDamageMultiplier)
            {
                return BuildWeaponNormalizedPreview(
                    weapon,
                    stats,
                    previewModifiers,
                    previewModifier,
                    "Damage",
                    StatId.WeaponDamageMultiplier,
                    WeaponDamagePreview,
                    weapon.Definition.projectileDamage);
            }

            if (previewModifier.StatId == StatId.WeaponChain)
            {
                return BuildWeaponIntPreview(
                    weapon,
                    stats,
                    previewModifiers,
                    "Chain",
                    StatId.WeaponChain,
                    ChainCountPreview);
            }

            return BuildGenericWeaponStatPreview(weapon, previewModifier);
        }

        private static UpgradePreviewFrame BuildWeaponAttackSpeedPreview(WeaponRuntime weapon, RunStats stats, IReadOnlyList<StatModifier> previewModifiers)
        {
            float currentMultiplier = WeaponStatRules.AttackSpeedMultiplier(stats, weapon.Modifiers);
            float nextMultiplier = WeaponStatRules.AttackSpeedMultiplier(stats, previewModifiers);
            bool beneficial = nextMultiplier > currentMultiplier;
            return new UpgradePreviewFrame(
                $"{weapon.Definition.DisplayName}\nWeapon Attack Speed {FormatPercentMultiplier(currentMultiplier)} -> {Colorize(FormatPercentMultiplier(nextMultiplier), beneficial)}",
                beneficial);
        }

        private static UpgradePreviewFrame BuildAddWeaponPreview(WeaponDefinition weapon, WeaponLoadout loadout)
        {
            string name = weapon != null && !string.IsNullOrWhiteSpace(weapon.DisplayName) ? weapon.DisplayName : "Weapon";
            int current = loadout != null ? loadout.WeaponCount : 0;
            int next = loadout != null ? Mathf.Min(loadout.MaxWeapons, current + 1) : current + 1;
            bool beneficial = loadout == null || loadout.HasOpenSlot;
            return new UpgradePreviewFrame(
                $"{name}\nWeapon Slots {current}/{(loadout != null ? loadout.MaxWeapons : WeaponLoadout.DefaultMaxWeapons)} -> {Colorize($"{next}/{(loadout != null ? loadout.MaxWeapons : WeaponLoadout.DefaultMaxWeapons)}", beneficial)}",
                beneficial);
        }

        private static UpgradePreviewFrame BuildWeaponIntPreview(
            WeaponRuntime weapon,
            RunStats stats,
            IReadOnlyList<StatModifier> previewModifiers,
            string label,
            StatId displayStatId,
            WeaponFloatEvaluator evaluate)
        {
            StatDefinition stat = StatMetadata.Get(displayStatId);
            float current = evaluate(weapon.Definition, stats, weapon.Modifiers);
            float next = evaluate(weapon.Definition, stats, previewModifiers);
            bool beneficial = next > current;
            return new UpgradePreviewFrame(
                $"{weapon.Definition.DisplayName}\n{label} {StatDisplayRules.FormatValue(stat, current, StatValueDisplayMode.Preview)} -> {Colorize(StatDisplayRules.FormatValue(stat, next, StatValueDisplayMode.Preview), beneficial)}",
                beneficial);
        }

        private static UpgradePreviewFrame BuildWeaponFloatPreview(
            WeaponRuntime weapon,
            RunStats stats,
            IReadOnlyList<StatModifier> previewModifiers,
            string label,
            StatModifier fallbackModifier,
            StatId displayStatId,
            WeaponFloatEvaluator evaluate,
            float baseValue)
        {
            return BuildWeaponNormalizedPreview(weapon, stats, previewModifiers, fallbackModifier, label, displayStatId, evaluate, baseValue);
        }

        private static UpgradePreviewFrame BuildWeaponNormalizedPreview(
            WeaponRuntime weapon,
            RunStats stats,
            IReadOnlyList<StatModifier> previewModifiers,
            StatModifier fallbackModifier,
            string label,
            StatId displayStatId,
            WeaponFloatEvaluator evaluate,
            float baseValue)
        {
            if (Mathf.Abs(baseValue) <= 0.0001f)
            {
                return BuildGenericWeaponStatPreview(weapon, fallbackModifier);
            }

            StatDefinition stat = StatMetadata.Get(displayStatId);
            float current = evaluate(weapon.Definition, stats, weapon.Modifiers);
            float next = evaluate(weapon.Definition, stats, previewModifiers);
            float currentMultiplier = current / baseValue;
            float nextMultiplier = next / baseValue;
            bool beneficial = nextMultiplier > currentMultiplier;
            string currentText = StatDisplayRules.FormatValue(stat, currentMultiplier, StatValueDisplayMode.Preview);
            string nextText = StatDisplayRules.FormatValue(stat, nextMultiplier, StatValueDisplayMode.Preview);
            return new UpgradePreviewFrame(
                $"{weapon.Definition.DisplayName}\n{label} {currentText} -> {Colorize(nextText, beneficial)}",
                beneficial);
        }

        private static UpgradePreviewFrame BuildWeaponMultiplierPreview(
            WeaponRuntime weapon,
            IReadOnlyList<StatModifier> previewModifiers,
            string label,
            StatId displayStatId)
        {
            StatDefinition stat = StatMetadata.Get(displayStatId);
            float current = EvaluateWeaponMultiplier(weapon.Modifiers, displayStatId);
            float next = EvaluateWeaponMultiplier(previewModifiers, displayStatId);
            bool beneficial = next > current;
            return new UpgradePreviewFrame(
                $"{weapon.Definition.DisplayName}\n{label} {StatDisplayRules.FormatValue(stat, current, StatValueDisplayMode.Preview)} -> {Colorize(StatDisplayRules.FormatValue(stat, next, StatValueDisplayMode.Preview), beneficial)}",
                beneficial);
        }

        private static UpgradePreviewFrame BuildGenericWeaponStatPreview(WeaponRuntime weapon, StatModifier modifier)
        {
            if (!StatMetadata.All.TryGetValue(modifier.StatId, out StatDefinition stat))
            {
                bool unknownBeneficial = modifier.Value > 0f;
                return new UpgradePreviewFrame(
                    $"{weapon.Definition.DisplayName}\n{modifier.StatId} {Colorize(modifier.Value.ToString("0.##"), unknownBeneficial)}",
                    unknownBeneficial);
            }

            string modifierText = StatDisplayRules.FormatModifier(stat, modifier.Bucket, modifier.Value);
            bool beneficial = modifier.Value > 0f;
            return new UpgradePreviewFrame(
                $"{weapon.Definition.DisplayName}\n{stat.DisplayName} {Colorize(modifierText, beneficial)}",
                beneficial);
        }

        private static float ProjectileCountPreview(WeaponDefinition weapon, RunStats stats, IReadOnlyList<StatModifier> modifiers)
        {
            if (weapon == null)
            {
                return 0f;
            }

            float baseCount = Mathf.Max(1, weapon.baseProjectileCount);
            float globalBonus = stats != null ? stats.GetFloat(StatId.ProjectileCount) : 0f;
            float localBonus = SumModifierValues(modifiers, StatId.WeaponProjectileCount);
            return Mathf.Clamp(baseCount + globalBonus + localBonus, 1f, Mathf.Max(baseCount, weapon.maxProjectileCount));
        }

        private static float OrbitCountPreview(WeaponDefinition weapon, RunStats stats, IReadOnlyList<StatModifier> modifiers)
        {
            if (weapon == null || !weapon.orbitEnabled)
            {
                return 0f;
            }

            float baseCount = Mathf.Max(1, weapon.baseOrbitCount);
            float globalBonus = stats != null ? stats.GetFloat(StatId.ProjectileCount) : 0f;
            float localBonus = SumModifierValues(modifiers, StatId.WeaponProjectileCount);
            return Mathf.Clamp(baseCount + globalBonus + localBonus, 1f, Mathf.Max(baseCount, weapon.maxOrbitCount));
        }

        private static float BounceCountPreview(WeaponDefinition weapon, RunStats stats, IReadOnlyList<StatModifier> modifiers)
        {
            if (weapon == null || !weapon.bounceEnabled)
            {
                return 0f;
            }

            float baseCount = Mathf.Max(0, weapon.baseBounceCount);
            float globalBonus = stats != null ? stats.GetFloat(StatId.Bounce) : 0f;
            float localBonus = SumModifierValues(modifiers, StatId.WeaponBounce);
            return Mathf.Clamp(baseCount + globalBonus + localBonus, 0f, Mathf.Max(baseCount, weapon.maxBounceCount));
        }

        private static float ChainCountPreview(WeaponDefinition weapon, RunStats stats, IReadOnlyList<StatModifier> modifiers)
        {
            if (weapon == null || !weapon.chainEnabled)
            {
                return 0f;
            }

            float baseCount = Mathf.Max(0, weapon.baseChainCount);
            float globalBonus = stats != null ? stats.GetFloat(StatId.Chain) : 0f;
            float localBonus = SumModifierValues(modifiers, StatId.WeaponChain);
            return Mathf.Clamp(baseCount + globalBonus + localBonus, 0f, Mathf.Max(baseCount, weapon.maxChainCount));
        }

        private static float WeaponDamagePreview(WeaponDefinition weapon, RunStats stats, IReadOnlyList<StatModifier> modifiers)
        {
            if (weapon == null)
            {
                return 0f;
            }

            var damageModifiers = new List<StatModifier>();
            float globalMultiplier = stats != null ? stats.GetFloat(StatId.GlobalDamageMultiplier) : 1f;
            if (!Mathf.Approximately(globalMultiplier, 1f))
            {
                damageModifiers.Add(new StatModifier(StatId.GlobalDamageMultiplier, StatModifierBucket.GlobalPercent, globalMultiplier - 1f, "run_stats"));
            }

            AddWeaponDamagePreviewModifiers(modifiers, damageModifiers);
            return StatRules.EvaluateDamage(weapon.projectileDamage, damageModifiers);
        }

        private static void AddWeaponDamagePreviewModifiers(IReadOnlyList<StatModifier> source, List<StatModifier> target)
        {
            if (source == null)
            {
                return;
            }

            for (int i = 0; i < source.Count; i++)
            {
                StatModifier modifier = source[i];
                if (modifier.StatId != StatId.GlobalDamageMultiplier
                    && modifier.StatId != StatId.WeaponDamageMultiplier
                    && modifier.StatId != StatId.WeaponFlatDamage)
                {
                    continue;
                }

                StatModifierBucket bucket = modifier.StatId == StatId.WeaponFlatDamage
                    ? StatModifierBucket.Flat
                    : modifier.Bucket switch
                    {
                        StatModifierBucket.AdditivePercent => StatModifierBucket.WeaponLocalPercent,
                        StatModifierBucket.GlobalPercent => StatModifierBucket.WeaponLocalPercent,
                        _ => modifier.Bucket
                    };
                target.Add(new StatModifier(modifier.StatId, bucket, modifier.Value, modifier.SourceId));
            }
        }

        private static float EvaluateWeaponMultiplier(IReadOnlyList<StatModifier> modifiers, StatId statId)
        {
            float flat = 0f;
            float additivePercent = 0f;
            float multiplicativePercent = 1f;
            bool hasMultiplicative = false;
            if (modifiers != null)
            {
                for (int i = 0; i < modifiers.Count; i++)
                {
                    StatModifier modifier = modifiers[i];
                    if (modifier.StatId != statId)
                    {
                        continue;
                    }

                    switch (modifier.Bucket)
                    {
                        case StatModifierBucket.Flat:
                            flat += modifier.Value;
                            break;
                        case StatModifierBucket.AdditivePercent:
                        case StatModifierBucket.WeaponLocalPercent:
                        case StatModifierBucket.GlobalPercent:
                        case StatModifierBucket.TagSpecializationPercent:
                        case StatModifierBucket.ConditionalPercent:
                            additivePercent += modifier.Value;
                            break;
                        case StatModifierBucket.MultiplicativePercent:
                            multiplicativePercent *= modifier.Value;
                            hasMultiplicative = true;
                            break;
                    }
                }
            }

            return Mathf.Max(0f, (1f + flat) * (1f + additivePercent) * (hasMultiplicative ? multiplicativePercent : 1f));
        }

        private static float SumModifierValues(IReadOnlyList<StatModifier> modifiers, StatId statId)
        {
            if (modifiers == null)
            {
                return 0f;
            }

            float total = 0f;
            for (int i = 0; i < modifiers.Count; i++)
            {
                if (modifiers[i].StatId == statId)
                {
                    total += modifiers[i].Value;
                }
            }

            return total;
        }

        private static string FormatStatValue(StatDefinition stat, float value, StatValueDisplayMode mode = StatValueDisplayMode.Applied)
        {
            return StatDisplayRules.FormatValue(stat, value, mode);
        }

        private static string FormatPercentMultiplier(float value)
        {
            return $"{Mathf.RoundToInt(value * 100f)}%";
        }

        private static string Colorize(string text, bool enabled)
        {
            return enabled ? $"<color={PositiveColorHex}>{text}</color>" : text;
        }
    }
}
