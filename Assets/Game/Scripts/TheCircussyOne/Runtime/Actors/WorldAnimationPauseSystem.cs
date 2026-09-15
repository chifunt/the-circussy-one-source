using System;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using VContainer.Unity;
using TheCircussyOne.Visuals;

namespace TheCircussyOne.Runtime
{
    public sealed class WorldAnimationPauseSystem : IStartable, ITickable, ITickableWhenPaused, IDisposable
    {
        private readonly RunPauseState pauseState;
        private readonly PlayerView player;
        private readonly ActorRegistry registry;
        private readonly VfxEffectFactory vfxFactory;
        private readonly ProjectileExplosionSystem projectileExplosionSystem;
        private readonly WorldAtmosphereSystem worldAtmosphereSystem;
        private readonly WorldRewardTransientVfxSystem worldRewardTransientVfxSystem;
        private readonly HashSet<ParticleSystem> pausedParticleSystems = new();
        private bool paused;

        public WorldAnimationPauseSystem(
            RunPauseState pauseState,
            PlayerView player,
            ActorRegistry registry,
            VfxEffectFactory vfxFactory = null,
            ProjectileExplosionSystem projectileExplosionSystem = null,
            WorldAtmosphereSystem worldAtmosphereSystem = null,
            WorldRewardTransientVfxSystem worldRewardTransientVfxSystem = null)
        {
            this.pauseState = pauseState;
            this.player = player;
            this.registry = registry;
            this.vfxFactory = vfxFactory;
            this.projectileExplosionSystem = projectileExplosionSystem;
            this.worldAtmosphereSystem = worldAtmosphereSystem;
            this.worldRewardTransientVfxSystem = worldRewardTransientVfxSystem;
        }

        public void Start()
        {
            paused = pauseState != null && pauseState.IsPaused;
            if (pauseState != null)
            {
                pauseState.PauseChanged += OnPauseChanged;
            }

            ApplyPause(paused);
        }

        public void Tick()
        {
            if (paused)
            {
                ApplyActorPause(true);
                PauseWorldParticles();
            }
        }

        public void Dispose()
        {
            if (pauseState != null)
            {
                pauseState.PauseChanged -= OnPauseChanged;
            }

            ApplyActorPause(false);
            ResumeWorldParticles();
            Tween.SetPausedAll(false);
        }

        private void OnPauseChanged(bool isPaused)
        {
            paused = isPaused;
            ApplyPause(isPaused);
        }

        private void ApplyPause(bool isPaused)
        {
            ApplyActorPause(isPaused);
            ApplyParticlePause(isPaused);
            Tween.SetPausedAll(isPaused);
        }

        private void ApplyActorPause(bool isPaused)
        {
            player?.SetWorldAnimationPaused(isPaused);
            if (registry == null)
            {
                return;
            }

            var enemies = registry.Enemies;
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                EnemyRuntime enemy = enemies[i];
                EnemyView view = enemy != null ? enemy.View : null;
                if (view == null || !view.IsActive)
                {
                    continue;
                }

                view.SetWorldAnimationPaused(isPaused);
            }
        }

        private void ApplyParticlePause(bool isPaused)
        {
            if (isPaused)
            {
                PauseWorldParticles();
                return;
            }

            ResumeWorldParticles();
        }

        private void PauseWorldParticles()
        {
            PauseParticlesIn(player);

            if (registry != null)
            {
                var enemies = registry.Enemies;
                for (int i = enemies.Count - 1; i >= 0; i--)
                {
                    PauseParticlesIn(enemies[i]?.View);
                }

                var projectiles = registry.Projectiles;
                for (int i = projectiles.Count - 1; i >= 0; i--)
                {
                    PauseParticlesIn(projectiles[i]?.View);
                }

                var pickups = registry.Pickups;
                for (int i = pickups.Count - 1; i >= 0; i--)
                {
                    PauseParticlesIn(pickups[i]?.View);
                }
            }

            if (vfxFactory != null)
            {
                var activeVfx = vfxFactory.Active;
                for (int i = activeVfx.Count - 1; i >= 0; i--)
                {
                    PauseParticlesIn(activeVfx[i]?.View);
                }
            }

            projectileExplosionSystem?.ForEachActiveView(PauseParticlesIn);
            worldAtmosphereSystem?.ForEachActiveView(PauseParticlesIn);
            worldRewardTransientVfxSystem?.ForEachActiveView(PauseParticlesIn);
        }

        private void PauseParticlesIn(Component component)
        {
            if (component == null || !component.gameObject.activeInHierarchy)
            {
                return;
            }

            ParticleSystem[] systems = component.GetComponentsInChildren<ParticleSystem>(includeInactive: true);
            for (int i = 0; i < systems.Length; i++)
            {
                ParticleSystem particleSystem = systems[i];
                if (particleSystem == null || !particleSystem.gameObject.activeInHierarchy)
                {
                    continue;
                }

                if (particleSystem.isPlaying && !particleSystem.isPaused)
                {
                    particleSystem.Pause(withChildren: false);
                    pausedParticleSystems.Add(particleSystem);
                }
            }
        }

        private void ResumeWorldParticles()
        {
            foreach (ParticleSystem particleSystem in pausedParticleSystems)
            {
                if (particleSystem != null && particleSystem.isPaused)
                {
                    particleSystem.Play(withChildren: false);
                }
            }

            pausedParticleSystems.Clear();
        }
    }
}
