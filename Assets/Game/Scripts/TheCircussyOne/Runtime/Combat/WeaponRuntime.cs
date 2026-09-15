using System.Collections.Generic;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using TheCircussyOne.Stats;
using UnityEngine;

namespace TheCircussyOne.Runtime
{
    public sealed class WeaponRuntime
    {
        private readonly List<StatModifier> modifiers = new();

        public WeaponRuntime(WeaponDefinition definition, int level)
        {
            Definition = definition;
            Level = Mathf.Max(1, level);
            RemainingCooldown = WeaponCooldownRules.FireInterval(definition);
        }

        public WeaponDefinition Definition { get; }
        public int Level { get; private set; }
        public float RemainingCooldown { get; set; }
        public IReadOnlyList<StatModifier> Modifiers => modifiers;

        public int NextFireSequence()
        {
            unchecked
            {
                return fireSequence++;
            }
        }

        public void IncreaseLevel(int amount = 1)
        {
            Level = Mathf.Max(1, Level + Mathf.Max(0, amount));
        }

        public void AddModifier(StatModifier modifier)
        {
            modifiers.Add(modifier);
        }

        public void RemoveModifierSource(string sourceId)
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

        private int fireSequence;
    }
}
