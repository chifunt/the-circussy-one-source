using System;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;
using TheCircussyOne.Stats;

namespace TheCircussyOne.Runtime
{
    public sealed class GameState
    {
        private readonly GameConfig _config;
        private readonly RunStats _stats;
        private int _lastMaxHealth;

        public GameState(GameConfig config, RunStats stats = null)
        {
            _config = config;
            _stats = stats ?? new RunStats(config);
            Level = 1;
            _stats.SetLevel(Level);
            Health = MaxHealth;
            _lastMaxHealth = Health;
        }

        public int Health { get; private set; }
        public int Kills { get; private set; }
        public int Level { get; private set; }
        public int Experience { get; private set; }
        public int TotalDamageDealt { get; private set; }
        public string LastDamageCause { get; private set; } = "Unknown";
        public bool IsGameOver => Health <= 0;
        public int MaxHealth => _stats.GetInt(StatId.PlayerMaxHealth);
        public int ExperienceTarget => ExperienceRequiredForNextLevel(Level);

        public float MoveSpeedMultiplier => _stats.GetFloat(StatId.PlayerMoveSpeedMultiplier);

        public event Action<int, int> HealthChanged;
        public event Action<int, int, float> PlayerDamaged;
        public event Action<int, int, float> PlayerHealed;
        public event Action<int> KillsChanged;
        public event Action<int, int> ExperienceChanged;
        public event Action<int> LevelChanged;
        public event Action GameOver;

        public void Reset()
        {
            Kills = 0;
            Level = 1;
            Experience = 0;
            TotalDamageDealt = 0;
            LastDamageCause = "Unknown";
            _stats.SetLevel(Level);
            Health = MaxHealth;
            _lastMaxHealth = Health;
            HealthChanged?.Invoke(Health, MaxHealth);
            KillsChanged?.Invoke(Kills);
            ExperienceChanged?.Invoke(Experience, ExperienceTarget);
            LevelChanged?.Invoke(Level);
        }

        public void DamagePlayer(int amount)
        {
            DamagePlayer(amount, "Unknown");
        }

        public void DamagePlayer(int amount, string cause)
        {
            if (IsGameOver || amount <= 0)
            {
                return;
            }

            int finalAmount = StatRules.ApplyArmorDamageReduction(amount, _stats.GetFloat(StatId.PlayerArmor));
            int maxHealth = MaxHealth;
            Health = Math.Max(0, Health - finalAmount);
            LastDamageCause = string.IsNullOrWhiteSpace(cause) ? "Unknown" : cause.Trim();
            HealthChanged?.Invoke(Health, maxHealth);
            PlayerDamaged?.Invoke(finalAmount, maxHealth, maxHealth <= 0 ? 0f : (float)finalAmount / maxHealth);

            if (Health == 0)
            {
                GameOver?.Invoke();
            }
        }

        public void HealPlayer(int amount)
        {
            if (IsGameOver || amount <= 0)
            {
                return;
            }

            int maxHealth = MaxHealth;
            int previousHealth = Health;
            Health = Math.Min(maxHealth, Health + amount);
            HealthChanged?.Invoke(Health, maxHealth);
            int healedAmount = Health - previousHealth;
            if (healedAmount > 0)
            {
                PlayerHealed?.Invoke(healedAmount, maxHealth, maxHealth <= 0 ? 0f : (float)healedAmount / maxHealth);
            }
        }

        public void AddKill()
        {
            Kills++;
            KillsChanged?.Invoke(Kills);
        }

        public void AddDamageDealt(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            TotalDamageDealt += amount;
        }

        public void AddExperience(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            int previousLevel = Level;
            ExperienceRules.AddExperience(Experience, Level, amount, _config.ExperienceCurve, out int nextExperience, out int nextLevel, out int nextTarget);
            Experience = nextExperience;
            Level = nextLevel;
            _stats.SetLevel(Level);

            for (int level = previousLevel + 1; level <= Level; level++)
            {
                LevelChanged?.Invoke(level);
            }

            ExperienceChanged?.Invoke(Experience, nextTarget);
        }

        public void RefreshDerivedStats()
        {
            int maxHealth = MaxHealth;
            if (maxHealth > _lastMaxHealth)
            {
                Health = Math.Min(maxHealth, Health + maxHealth - _lastMaxHealth);
            }
            else
            {
                Health = Math.Min(Health, maxHealth);
            }

            _lastMaxHealth = maxHealth;
            HealthChanged?.Invoke(Health, maxHealth);
        }

        public int ExperienceRequiredForNextLevel(int level)
        {
            return _config.ExperienceRequiredForNextLevel(level);
        }
    }
}
