using System;
using System.Collections.Generic;
using TheCircussyOne.Content;
using TheCircussyOne.Runtime;
using UnityEngine;

namespace TheCircussyOne.Rules
{
    public enum UpgradeChoiceKind
    {
        None,
        AddWeapon,
        WeaponUpgrade,
        Talent
    }

    public readonly struct UpgradeChoice
    {
        public UpgradeChoice(WeaponDefinition weaponToAdd)
        {
            WeaponToAdd = weaponToAdd;
            Definition = null;
            Talent = null;
            CurrentLevel = 0;
            Kind = weaponToAdd != null ? UpgradeChoiceKind.AddWeapon : UpgradeChoiceKind.None;
            RolledRarity = ContentRarity.Common;
        }

        public UpgradeChoice(UpgradeDefinition definition, int currentLevel)
            : this(definition, currentLevel, ContentRarity.Common)
        {
        }

        public UpgradeChoice(UpgradeDefinition definition, int currentLevel, ContentRarity rolledRarity)
        {
            WeaponToAdd = null;
            Definition = definition;
            Talent = null;
            CurrentLevel = Mathf.Max(0, currentLevel);
            Kind = definition != null ? UpgradeChoiceKind.WeaponUpgrade : UpgradeChoiceKind.None;
            RolledRarity = rolledRarity;
        }

        public UpgradeChoice(TalentDefinition talent, int currentLevel)
            : this(talent, currentLevel, ContentRarity.Common)
        {
        }

        public UpgradeChoice(TalentDefinition talent, int currentLevel, ContentRarity rolledRarity)
        {
            WeaponToAdd = null;
            Definition = null;
            Talent = talent;
            CurrentLevel = Mathf.Max(0, currentLevel);
            Kind = talent != null ? UpgradeChoiceKind.Talent : UpgradeChoiceKind.None;
            RolledRarity = rolledRarity;
        }

        public WeaponDefinition WeaponToAdd { get; }
        public UpgradeDefinition Definition { get; }
        public TalentDefinition Talent { get; }
        public UpgradeChoiceKind Kind { get; }
        public int CurrentLevel { get; }
        public ContentRarity RolledRarity { get; }
        public int NextLevel => CurrentLevel + 1;
        public bool HasChoice => Kind != UpgradeChoiceKind.None;
        public string Id => WeaponToAdd != null ? $"add_{WeaponToAdd.Id}" : Definition != null ? Definition.Id : Talent != null ? Talent.Id : string.Empty;
        public string DisplayName => WeaponToAdd != null ? WeaponToAdd.DisplayName : Definition != null ? Definition.DisplayName : Talent != null ? Talent.DisplayName : string.Empty;
        public string ShortDescription => WeaponToAdd != null ? WeaponToAdd.AddWeaponShortDescription : Definition != null ? Definition.shortDescription : Talent != null ? Talent.shortDescription : string.Empty;
        public ContentRarity Rarity => RolledRarity;
        public ContentTagSet Tags => WeaponToAdd != null ? WeaponToAdd.Tags : Definition != null ? Definition.Tags : Talent != null ? Talent.Tags : null;
        public Color IconColor => WeaponToAdd != null ? WeaponToAdd.projectilePrimaryColor : Definition != null ? Definition.iconColor : Talent != null ? Talent.iconColor : Color.white;
        public Sprite IconSprite => ContentIconRules.ForUpgradeChoice(this);
    }

    public static class UpgradeSelectionRules
    {
        private enum ChoiceCategory
        {
            NewWeapon,
            OwnedWeaponUpgrade,
            Talent
        }

        private readonly struct ChoiceCandidate
        {
            public ChoiceCandidate(WeaponDefinition weaponToAdd, ChoiceCategory category)
            {
                WeaponToAdd = weaponToAdd;
                Upgrade = null;
                Talent = null;
                CurrentLevel = 0;
                Category = category;
            }

            public ChoiceCandidate(UpgradeDefinition upgrade, int currentLevel, ChoiceCategory category)
            {
                WeaponToAdd = null;
                Upgrade = upgrade;
                Talent = null;
                CurrentLevel = currentLevel;
                Category = category;
            }

            public ChoiceCandidate(TalentDefinition talent, int currentLevel, ChoiceCategory category)
            {
                WeaponToAdd = null;
                Upgrade = null;
                Talent = talent;
                CurrentLevel = currentLevel;
                Category = category;
            }

            public WeaponDefinition WeaponToAdd { get; }
            public UpgradeDefinition Upgrade { get; }
            public TalentDefinition Talent { get; }
            public int CurrentLevel { get; }
            public ChoiceCategory Category { get; }
            public string Id => WeaponToAdd != null ? $"add_{WeaponToAdd.Id}" : Upgrade != null ? Upgrade.Id : Talent != null ? Talent.Id : string.Empty;
            public ContentRaritySet PossibleRarities => WeaponToAdd != null ? ContentRaritySet.Only(ContentRarity.Common) : Upgrade != null ? Upgrade.LevelUpRarities : Talent != null ? Talent.LevelUpRarities : ContentRaritySet.Only(ContentRarity.Common);

            public bool AllowsRarity(ContentRarity rarity)
            {
                return PossibleRarities.Contains(rarity);
            }

            public UpgradeChoice ToChoice(ContentRarity rolledRarity)
            {
                if (WeaponToAdd != null)
                {
                    return new UpgradeChoice(WeaponToAdd);
                }

                return Upgrade != null
                    ? new UpgradeChoice(Upgrade, CurrentLevel, rolledRarity)
                    : new UpgradeChoice(Talent, CurrentLevel, rolledRarity);
            }
        }

        private readonly struct SelectedCandidate
        {
            public SelectedCandidate(ChoiceCandidate candidate, ContentRarity rarity)
            {
                Candidate = candidate;
                Rarity = rarity;
            }

            public ChoiceCandidate Candidate { get; }
            public ContentRarity Rarity { get; }
        }

        private static readonly ContentRarity[] DescendingRarities =
        {
            ContentRarity.Legendary,
            ContentRarity.Epic,
            ContentRarity.Rare,
            ContentRarity.Uncommon,
            ContentRarity.Common
        };

        public static ContentRarity RollRarity(ContentRarityWeightTable table, float normalizedRoll, float luck)
        {
            float effectiveLuck = TheCircussyOne.Stats.StatRules.EffectiveLuckMultiplier(luck);
            float adjustedRoll = Mathf.Clamp01(normalizedRoll + Mathf.Max(0f, effectiveLuck - 1f) * 0.15f);
            return table.Pick(adjustedRoll);
        }

        public static List<UpgradeChoice> BuildChoices(
            UpgradeCatalog catalog,
            UpgradeRunState runState,
            int seed,
            int choiceCount = 3,
            float luck = 100f,
            WeaponLoadout weaponLoadout = null,
            ItemInventory itemInventory = null,
            WeaponCatalog weaponCatalog = null,
            TalentCatalog talentCatalog = null,
            TalentRunState talentRunState = null,
            PerformerDefinition selectedPerformer = null)
        {
            var choices = new List<UpgradeChoice>();
            if (choiceCount <= 0)
            {
                return choices;
            }

            var rng = new System.Random(seed);
            List<ChoiceCandidate> eligible = EligibleCandidates(catalog?.Definitions, runState, weaponLoadout, itemInventory, weaponCatalog, talentCatalog, talentRunState, selectedPerformer);
            if (eligible.Count == 0)
            {
                return choices;
            }

            while (choices.Count < choiceCount)
            {
                ChoiceCategory? category = PickCategory(eligible, choices, weaponLoadout, rng);
                if (category == null)
                {
                    break;
                }

                ContentRarityWeightTable weights = catalog != null ? catalog.levelUpRarityWeights : ContentRarityWeightTable.LevelUpDefault();
                ContentRarity targetRarity = RollRarity(weights, (float)rng.NextDouble(), luck);
                SelectedCandidate? selected = PickCandidate(eligible, choices, category.Value, targetRarity, rng);
                if (selected == null)
                {
                    break;
                }

                choices.Add(selected.Value.Candidate.ToChoice(selected.Value.Rarity));
            }

            return choices;
        }

        public static bool IsEligible(
            UpgradeDefinition definition,
            UpgradeRunState runState,
            WeaponLoadout weaponLoadout = null,
            ItemInventory itemInventory = null)
        {
            if (!ContentAvailabilityRules.IsActiveAndValid(definition) || definition.maxLevel < 1)
            {
                return false;
            }

            WeaponRuntime weapon = weaponLoadout?.FindById(definition.weaponId);
            if (weapon?.Definition == null || !ContentAvailabilityRules.IsActiveAndValid(weapon.Definition))
            {
                return false;
            }

            WeaponDefinition targetDefinition = definition.weaponDefinition != null ? definition.weaponDefinition : weapon.Definition;
            if (!ContentAvailabilityRules.IsActiveAndValid(targetDefinition))
            {
                return false;
            }

            if (!string.Equals(targetDefinition.Id, weapon.Definition.Id, StringComparison.Ordinal))
            {
                return false;
            }

            return targetDefinition.SupportsUpgrade(definition);
        }

        public static bool IsAddWeaponEligible(WeaponDefinition weapon, WeaponLoadout weaponLoadout = null)
        {
            return ContentAvailabilityRules.IsActiveAndValid(weapon)
                && weaponLoadout != null
                && weapon.CanAppearAsLevelUpWeapon
                && weaponLoadout.CanAddWeapon(weapon);
        }

        public static bool IsTalentEligible(TalentDefinition talent, TalentRunState runState, PerformerDefinition selectedPerformer = null)
        {
            if (!ContentAvailabilityRules.IsActiveAndValid(talent))
            {
                return false;
            }

            if (!talent.AllowsPerformer(selectedPerformer))
            {
                return false;
            }

            if (!HasEligibleTalentEffects(talent))
            {
                return false;
            }

            int currentLevel = runState != null ? runState.GetLevel(talent) : 0;
            return currentLevel < talent.EffectiveMaxLevel;
        }

        private static bool HasEligibleTalentEffects(TalentDefinition talent)
        {
            if (talent?.statModifiers == null || talent.statModifiers.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < talent.statModifiers.Count; i++)
            {
                UpgradeStatModifierDefinition modifier = talent.statModifiers[i];
                if (!ContentStatValidationRules.IsValid(modifier.statId, modifier.bucket)
                    || !ContentStatValidationRules.IsTalentStat(modifier.statId))
                {
                    return false;
                }
            }

            return true;
        }

        private static List<ChoiceCandidate> EligibleCandidates(
            IReadOnlyList<UpgradeDefinition> definitions,
            UpgradeRunState runState,
            WeaponLoadout weaponLoadout,
            ItemInventory itemInventory,
            WeaponCatalog weaponCatalog,
            TalentCatalog talentCatalog,
            TalentRunState talentRunState,
            PerformerDefinition selectedPerformer)
        {
            var eligible = new List<ChoiceCandidate>();
            if (weaponCatalog?.AvailableWeapons != null)
            {
                IReadOnlyList<WeaponDefinition> weapons = weaponCatalog.AvailableWeapons;
                for (int i = 0; i < weapons.Count; i++)
                {
                    WeaponDefinition weapon = weapons[i];
                    if (IsAddWeaponEligible(weapon, weaponLoadout))
                    {
                        eligible.Add(new ChoiceCandidate(weapon, ChoiceCategory.NewWeapon));
                    }
                }
            }

            bool hasTalentCatalog = talentCatalog != null && talentCatalog.Definitions != null && talentCatalog.Definitions.Count > 0;
            if (definitions != null)
            {
                for (int i = 0; i < definitions.Count; i++)
                {
                    UpgradeDefinition definition = definitions[i];
                    if (!IsEligible(definition, runState, weaponLoadout, itemInventory))
                    {
                        continue;
                    }

                    eligible.Add(new ChoiceCandidate(definition, runState != null ? runState.GetLevel(definition) : 0, ChoiceCategory.OwnedWeaponUpgrade));
                }
            }

            if (hasTalentCatalog)
            {
                IReadOnlyList<TalentDefinition> talents = talentCatalog.Definitions;
                for (int i = 0; i < talents.Count; i++)
                {
                    TalentDefinition talent = talents[i];
                    if (IsTalentEligible(talent, talentRunState, selectedPerformer))
                    {
                        eligible.Add(new ChoiceCandidate(talent, talentRunState != null ? talentRunState.GetLevel(talent) : 0, ChoiceCategory.Talent));
                    }
                }
            }

            return eligible;
        }

        private static ChoiceCategory? PickCategory(List<ChoiceCandidate> eligible, List<UpgradeChoice> choices, WeaponLoadout weaponLoadout, System.Random rng)
        {
            bool hasOpenWeaponSlot = weaponLoadout == null || weaponLoadout.HasOpenSlot;
            var weights = new List<(ChoiceCategory Category, int Weight)>();
            if (hasOpenWeaponSlot)
            {
                weights.Add((ChoiceCategory.NewWeapon, 25));
                weights.Add((ChoiceCategory.OwnedWeaponUpgrade, 40));
                weights.Add((ChoiceCategory.Talent, 35));
            }
            else
            {
                weights.Add((ChoiceCategory.OwnedWeaponUpgrade, 55));
                weights.Add((ChoiceCategory.Talent, 45));
            }

            int totalWeight = 0;
            for (int i = 0; i < weights.Count; i++)
            {
                if (!HasCategory(eligible, choices, weights[i].Category))
                {
                    continue;
                }

                totalWeight += weights[i].Weight;
            }

            if (totalWeight <= 0)
            {
                return null;
            }

            int roll = rng.Next(totalWeight);
            int cursor = 0;
            for (int i = 0; i < weights.Count; i++)
            {
                if (!HasCategory(eligible, choices, weights[i].Category))
                {
                    continue;
                }

                cursor += weights[i].Weight;
                if (roll < cursor)
                {
                    return weights[i].Category;
                }
            }

            return null;
        }

        private static SelectedCandidate? PickCandidate(
            List<ChoiceCandidate> eligible,
            List<UpgradeChoice> choices,
            ChoiceCategory category,
            ContentRarity targetRarity,
            System.Random rng)
        {
            int targetIndex = Array.IndexOf(DescendingRarities, targetRarity);
            if (targetIndex < 0)
            {
                targetIndex = DescendingRarities.Length - 1;
            }

            for (int rarityIndex = targetIndex; rarityIndex < DescendingRarities.Length; rarityIndex++)
            {
                ContentRarity rarity = DescendingRarities[rarityIndex];
                List<ChoiceCandidate> bucket = BucketForRarity(eligible, choices, category, rarity);
                Shuffle(bucket, rng);
                if (bucket.Count > 0)
                {
                    return new SelectedCandidate(bucket[0], rarity);
                }
            }

            for (int rarityIndex = targetIndex - 1; rarityIndex >= 0; rarityIndex--)
            {
                ContentRarity rarity = DescendingRarities[rarityIndex];
                List<ChoiceCandidate> bucket = BucketForRarity(eligible, choices, category, rarity);
                Shuffle(bucket, rng);
                if (bucket.Count > 0)
                {
                    return new SelectedCandidate(bucket[0], rarity);
                }
            }

            return null;
        }

        private static List<ChoiceCandidate> BucketForRarity(List<ChoiceCandidate> eligible, List<UpgradeChoice> choices, ChoiceCategory category, ContentRarity rarity)
        {
            var bucket = new List<ChoiceCandidate>();
            for (int i = 0; i < eligible.Count; i++)
            {
                ChoiceCandidate candidate = eligible[i];
                if (candidate.Category != category || !candidate.AllowsRarity(rarity) || ContainsChoice(choices, candidate))
                {
                    continue;
                }

                bucket.Add(candidate);
            }

            bucket.Sort((a, b) => string.CompareOrdinal(a.Id, b.Id));
            return bucket;
        }

        private static bool HasCategory(List<ChoiceCandidate> eligible, List<UpgradeChoice> choices, ChoiceCategory category)
        {
            for (int i = 0; i < eligible.Count; i++)
            {
                if (eligible[i].Category == category && !ContainsChoice(choices, eligible[i]))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ContainsChoice(List<UpgradeChoice> choices, ChoiceCandidate candidate)
        {
            for (int i = 0; i < choices.Count; i++)
            {
                if (string.Equals(choices[i].Id, candidate.Id, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static void Shuffle<T>(List<T> values, System.Random rng)
        {
            for (int i = values.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (values[i], values[j]) = (values[j], values[i]);
            }
        }
    }
}
