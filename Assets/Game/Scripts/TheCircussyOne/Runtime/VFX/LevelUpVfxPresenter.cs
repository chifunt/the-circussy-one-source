using System;
using VContainer.Unity;
using UnityEngine;
using TheCircussyOne.Visuals;

namespace TheCircussyOne.Runtime
{
    public sealed class LevelUpVfxPresenter : IStartable, IDisposable
    {
        private readonly GameState state;
        private readonly PlayerView player;
        private readonly IVfxSpawner vfx;

        public LevelUpVfxPresenter(GameState state, PlayerView player, IVfxSpawner vfx)
        {
            this.state = state;
            this.player = player;
            this.vfx = vfx;
        }

        public void Start()
        {
            state.LevelChanged += OnLevelChanged;
        }

        public void Dispose()
        {
            state.LevelChanged -= OnLevelChanged;
        }

        private void OnLevelChanged(int level)
        {
            if (level <= 1 || player == null)
            {
                return;
            }

            vfx?.Show(VfxEffectId.LevelUpBurst, player.Position + Vector3.up * 0.8f);
        }
    }
}
