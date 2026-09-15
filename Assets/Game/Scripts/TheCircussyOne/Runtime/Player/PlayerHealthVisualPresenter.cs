using System;
using VContainer.Unity;
using TheCircussyOne.Config;
using TheCircussyOne.Visuals;

namespace TheCircussyOne.Runtime
{
    public sealed class PlayerHealthVisualPresenter : IStartable, IDisposable
    {
        private readonly GameConfig config;
        private readonly GameState state;
        private readonly PlayerView player;

        public PlayerHealthVisualPresenter(GameConfig config, GameState state, PlayerView player)
        {
            this.config = config;
            this.state = state;
            this.player = player;
        }

        public void Start()
        {
            state.HealthChanged += OnHealthChanged;
            OnHealthChanged(state.Health, config.playerMaxHealth);
        }

        public void Dispose()
        {
            state.HealthChanged -= OnHealthChanged;
        }

        private void OnHealthChanged(int current, int max)
        {
            float normalized = max <= 0 ? 0f : (float)current / max;
            player?.SetHealthNormalized(normalized);
        }
    }
}
