using UnityEngine;
using VContainer.Unity;
using TheCircussyOne.Config;
using TheCircussyOne.Visuals;

namespace TheCircussyOne.Runtime
{
    public sealed class PlayerMovementVfxSystem : ITickable
    {
        private readonly VfxVisualConfig config;
        private readonly PlayerView player;
        private readonly GameState state;
        private bool configured;

        public PlayerMovementVfxSystem(VfxVisualConfig config, PlayerView player, GameState state)
        {
            this.config = config;
            this.player = player;
            this.state = state;
        }

        public void Tick()
        {
            if (player == null)
            {
                return;
            }

            if (!configured)
            {
                player.ApplyMoveDustVfxConfig(config);
                player.ApplyJumpTrailVfxConfig(config);
                configured = true;
            }

            ParticleSystem dust = player.MoveDust;
            ParticleSystem jumpTrail = player.JumpTrail;
            if (dust == null || config == null || !config.enabled || !config.playerMoveDustEnabled || state.IsGameOver)
            {
                StopDust(dust);
                StopDust(jumpTrail);
                return;
            }

            if (player.IsAirborneForVfx)
            {
                StopDust(dust);
                PlayJumpTrail(jumpTrail);
                return;
            }

            StopDust(jumpTrail);
            float rate = Mathf.Lerp(0f, config.playerMoveDustMaxEmissionRate, Mathf.Clamp01(player.MotionVisualTargetSpeed01));
            var emission = dust.emission;
            emission.rateOverTime = rate;

            if (rate > 0.05f)
            {
                if (!dust.isPlaying)
                {
                    dust.Play(withChildren: true);
                }
            }
            else
            {
                StopDust(dust);
            }
        }

        private void PlayJumpTrail(ParticleSystem jumpTrail)
        {
            if (jumpTrail == null || config == null || !config.playerJumpTrailEnabled)
            {
                StopDust(jumpTrail);
                return;
            }

            var emission = jumpTrail.emission;
            emission.enabled = true;
            emission.rateOverTime = Mathf.Max(0f, config.playerJumpTrailEmissionRate);
            if (!jumpTrail.isPlaying)
            {
                jumpTrail.Play(withChildren: true);
            }
        }

        private static void StopDust(ParticleSystem dust)
        {
            if (dust != null && dust.isPlaying)
            {
                dust.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmitting);
            }
        }
    }
}
