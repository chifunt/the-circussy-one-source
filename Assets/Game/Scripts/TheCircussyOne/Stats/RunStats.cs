using System.Collections.Generic;
using TheCircussyOne.Config;

namespace TheCircussyOne.Stats
{
    public sealed class RunStats
    {
        private readonly GameConfig config;
        private readonly Dictionary<StatId, float> baseValues = new();
        private readonly List<StatModifier> modifiers = new();
        private readonly List<StatModifier> scratch = new();

        public RunStats(GameConfig config)
        {
            this.config = config;
            ResetBaseValues();
            SetLevel(1);
        }

        public IReadOnlyList<StatModifier> Modifiers => modifiers;

        public float GetFloat(StatId id)
        {
            StatDefinition definition = StatMetadata.Get(id);
            return StatRules.EvaluateFloat(definition, BaseValue(id), Gather(id));
        }

        public int GetInt(StatId id)
        {
            StatDefinition definition = StatMetadata.Get(id);
            return StatRules.EvaluateInt(definition, BaseValue(id), Gather(id));
        }

        public float EvaluateFloatWithModifier(StatModifier modifier)
        {
            StatDefinition definition = StatMetadata.Get(modifier.StatId);
            return StatRules.EvaluateFloat(definition, BaseValue(modifier.StatId), GatherWithPreviewModifier(modifier));
        }

        public int EvaluateIntWithModifier(StatModifier modifier)
        {
            StatDefinition definition = StatMetadata.Get(modifier.StatId);
            return StatRules.EvaluateInt(definition, BaseValue(modifier.StatId), GatherWithPreviewModifier(modifier));
        }

        public StatBreakdown GetBreakdown(StatId id)
        {
            StatDefinition definition = StatMetadata.Get(id);
            float baseValue = BaseValue(id);
            var matchingModifiers = new List<StatModifier>();
            for (int i = 0; i < modifiers.Count; i++)
            {
                if (modifiers[i].StatId == id)
                {
                    matchingModifiers.Add(modifiers[i]);
                }
            }

            float finalValue = definition.ValueKind == StatValueKind.Integer
                ? StatRules.EvaluateInt(definition, baseValue, matchingModifiers)
                : StatRules.EvaluateFloat(definition, baseValue, matchingModifiers);
            var snapshots = new StatModifierSnapshot[matchingModifiers.Count];
            var lines = new List<StatBreakdownLine>(matchingModifiers.Count + 2)
            {
                new("Base", baseValue, "Base value")
            };

            for (int i = 0; i < matchingModifiers.Count; i++)
            {
                StatModifier modifier = matchingModifiers[i];
                snapshots[i] = new StatModifierSnapshot(modifier.StatId, modifier.Bucket, modifier.Value, modifier.SourceId);
                lines.Add(new StatBreakdownLine(modifier.Bucket.ToString(), modifier.Value, modifier.SourceId));
            }

            lines.Add(new StatBreakdownLine("Final", finalValue, definition.StackingPolicy.ToString()));
            return new StatBreakdown(definition, baseValue, finalValue, snapshots, lines.ToArray());
        }

        public IReadOnlyList<StatBreakdown> GetAllBreakdowns()
        {
            var breakdowns = new List<StatBreakdown>(StatMetadata.All.Count);
            foreach (StatId id in StatMetadata.All.Keys)
            {
                breakdowns.Add(GetBreakdown(id));
            }

            breakdowns.Sort(CompareBreakdowns);
            return breakdowns;
        }

        public void AddModifier(StatModifier modifier)
        {
            modifiers.Add(modifier);
        }

        public void RemoveSource(string sourceId)
        {
            if (string.IsNullOrWhiteSpace(sourceId))
            {
                return;
            }

            for (int i = modifiers.Count - 1; i >= 0; i--)
            {
                if (modifiers[i].SourceId == sourceId)
                {
                    modifiers.RemoveAt(i);
                }
            }
        }

        public void ClearModifiers()
        {
            modifiers.Clear();
        }

        public void SetLevel(int level)
        {
            // Level is still accepted for migration-safe callers, but stats now change
            // only through explicit modifiers such as selected upgrades.
        }

        public void ResetBaseValues()
        {
            baseValues.Clear();
            baseValues[StatId.PlayerMaxHealth] = config != null ? config.playerMaxHealth : 100f;
            baseValues[StatId.PlayerArmor] = 0f;
            baseValues[StatId.PlayerMoveSpeedMultiplier] = 1f;
            baseValues[StatId.PickupMagnetRadius] = config != null ? config.pickupMagnetRadius : 7.5f;
            baseValues[StatId.PickupCollectRadius] = config != null ? config.pickupCollectRadius : 1.2f;
            baseValues[StatId.XpGainMultiplier] = 1f;
            baseValues[StatId.Luck] = 100f;
            baseValues[StatId.GlobalDamageMultiplier] = 1f;
            baseValues[StatId.GlobalWeaponHaste] = 0f;
            baseValues[StatId.CritChance] = 0f;
            baseValues[StatId.CritDamageMultiplier] = 2f;
            baseValues[StatId.ProjectileSpeedMultiplier] = 1f;
            baseValues[StatId.ProjectileAreaMultiplier] = 1f;
            baseValues[StatId.ProjectileDurationMultiplier] = 1f;
            baseValues[StatId.ProjectileCount] = 0f;
            baseValues[StatId.Pierce] = 0f;
            baseValues[StatId.Bounce] = 0f;
            baseValues[StatId.Chain] = 0f;
            baseValues[StatId.PlayerHpRegenPerMinute] = 0f;
            baseValues[StatId.PlayerLifestealChance] = 0f;
            baseValues[StatId.PlayerKnockbackMultiplier] = 1f;
            baseValues[StatId.PlayerExtraJumps] = 0f;
            baseValues[StatId.WeaponDamageMultiplier] = 1f;
            baseValues[StatId.WeaponAttackSpeed] = 0f;
            baseValues[StatId.WeaponProjectileCount] = 0f;
            baseValues[StatId.WeaponProjectileSpeedMultiplier] = 1f;
            baseValues[StatId.WeaponProjectileSizeMultiplier] = 1f;
            baseValues[StatId.WeaponProjectileLifetimeMultiplier] = 1f;
            baseValues[StatId.WeaponSplashRadiusMultiplier] = 1f;
            baseValues[StatId.WeaponRangeMultiplier] = 1f;
            baseValues[StatId.WeaponPierce] = 0f;
            baseValues[StatId.WeaponBounce] = 0f;
            baseValues[StatId.WeaponAccuracyMultiplier] = 1f;
            baseValues[StatId.WeaponKnockbackMultiplier] = 1f;
            baseValues[StatId.WeaponFlatDamage] = 0f;
        }

        private float BaseValue(StatId id)
        {
            return baseValues.TryGetValue(id, out float value) ? value : 0f;
        }

        private IReadOnlyList<StatModifier> Gather(StatId id)
        {
            scratch.Clear();
            for (int i = 0; i < modifiers.Count; i++)
            {
                if (modifiers[i].StatId == id)
                {
                    scratch.Add(modifiers[i]);
                }
            }

            return scratch;
        }

        private IReadOnlyList<StatModifier> GatherWithPreviewModifier(StatModifier modifier)
        {
            scratch.Clear();
            for (int i = 0; i < modifiers.Count; i++)
            {
                if (modifiers[i].StatId == modifier.StatId)
                {
                    scratch.Add(modifiers[i]);
                }
            }

            scratch.Add(modifier);
            return scratch;
        }

        private static int CompareBreakdowns(StatBreakdown a, StatBreakdown b)
        {
            int category = string.CompareOrdinal(a.Category, b.Category);
            return category != 0 ? category : string.CompareOrdinal(a.DisplayName, b.DisplayName);
        }
    }
}
