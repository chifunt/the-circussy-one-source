using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using TheCircussyOne.Stats;
using TheCircussyOne.Visuals;

namespace TheCircussyOne.Runtime
{
    public sealed class AutoWeaponSystem : ITickable
    {
        private readonly WeaponLoadout _loadout;
        private readonly PlayerView _player;
        private readonly ActorRegistry _registry;
        private readonly ProjectileFactory _projectileFactory;
        private readonly GameState _state;
        private readonly FeedbackService _feedback;
        private readonly IGameTime _time;
        private readonly RunStats _stats;
        private readonly WeaponTargetingQuery _targetingQuery;
        private readonly RunSeedState _runSeedState;
        private readonly RunPhaseState _phaseState;
        private readonly IGameAudio _audio;

        public AutoWeaponSystem(
            WeaponLoadout loadout,
            PlayerView player,
            ActorRegistry registry,
            ProjectileFactory projectileFactory,
            GameState state,
            FeedbackService feedback,
            IGameTime time,
            RunStats stats = null,
            WeaponTargetingQuery targetingQuery = null,
            RunSeedState runSeedState = null,
            RunPhaseState phaseState = null,
            IGameAudio audio = null)
        {
            _loadout = loadout;
            _player = player;
            _registry = registry;
            _projectileFactory = projectileFactory;
            _state = state;
            _feedback = feedback;
            _time = time;
            _stats = stats;
            _targetingQuery = targetingQuery ?? new WeaponTargetingQuery();
            _runSeedState = runSeedState;
            _phaseState = phaseState;
            _audio = audio ?? NullGameAudio.Instance;
        }

        public void Tick()
        {
            if (_state.IsGameOver
                || _loadout == null
                || (_phaseState != null && !RunPhaseRules.ShouldTickCombat(_phaseState.CurrentPhase)))
            {
                return;
            }

            IReadOnlyList<WeaponRuntime> weapons = _loadout.Weapons;
            for (int i = 0; i < weapons.Count; i++)
            {
                TickWeapon(weapons[i]);
            }
        }

        private void TickWeapon(WeaponRuntime weapon)
        {
            if (weapon == null || weapon.Definition == null)
            {
                return;
            }

            WeaponDefinition definition = weapon.Definition;
            if (!ContentAvailabilityRules.IsActiveAndValid(definition) || definition.orbitEnabled)
            {
                return;
            }

            weapon.RemainingCooldown = WeaponCooldownRules.Tick(weapon.RemainingCooldown, _time.DeltaTime);
            if (!WeaponCooldownRules.CanFire(weapon.RemainingCooldown))
            {
                return;
            }

            EnemyRuntime target = SelectTarget(weapon);
            if (target == null)
            {
                return;
            }

            Vector3 aimTarget = target.AimPosition;
            Vector3 aimOrigin = _player.Position + Vector3.up * Mathf.Max(0.01f, definition.projectileSpawnHeight);
            Vector3 fallbackDirection = _player.Body.forward;
            Vector3 initialDirection = WeaponSpawnRules.AimDirection(aimOrigin, aimTarget, fallbackDirection);
            Vector3 muzzle = WeaponSpawnRules.MuzzlePosition(
                _player.Position,
                initialDirection,
                definition.projectileSpawnHeight,
                definition.projectileSpawnForwardOffset);
            Vector3 centerDirection = WeaponSpawnRules.AimDirection(muzzle, aimTarget, initialDirection);
            int projectileCount = WeaponStatRules.ProjectileCount(definition, _stats, weapon.Modifiers);
            float spreadAngle = WeaponStatRules.SpreadAngle(definition, _stats, weapon.Modifiers);
            float aimErrorAngle = WeaponStatRules.AimErrorAngle(definition, _stats, weapon.Modifiers);
            float targetDistance = Mathf.Max(0.01f, Vector3.Distance(muzzle, aimTarget));
            int fireSequence = weapon.NextFireSequence();
            for (int i = 0; i < projectileCount; i++)
            {
                Vector3 patternDirection = WeaponSpawnRules.SpreadDirection(centerDirection, i, projectileCount, spreadAngle);
                int aimErrorSeed = WeaponSpawnRules.AimErrorSeed(definition.Id, fireSequence, i, _runSeedState != null ? _runSeedState.CurrentSeed : 0);
                Vector3 fireDirection = WeaponSpawnRules.AimErrorDirection(patternDirection, aimErrorAngle, aimErrorSeed);
                Vector3 trajectoryTarget = muzzle + fireDirection * targetDistance;
                trajectoryTarget.y = aimTarget.y;
                _projectileFactory.Spawn(
                    weapon,
                    new ProjectileSpawnFrame(
                        muzzle,
                        fireDirection,
                        trajectoryTarget,
                        definition.projectileTrajectoryMode,
                        definition.projectileArcHeight));
            }

            _feedback.PlayShoot(muzzle, centerDirection);
            if (WeaponAudioCueRules.TryGetLaunchCue(definition, out GameAudioCue launchCue))
            {
                _audio.PlayAt(launchCue, muzzle);
            }

            weapon.RemainingCooldown = WeaponStatRules.FireInterval(definition, _stats, weapon.Modifiers);
        }

        private EnemyRuntime SelectTarget(WeaponRuntime weapon)
        {
            WeaponDefinition definition = weapon.Definition;
            return EnemyTargeting.FindNearestAliveInRange(
                _registry.Enemies,
                _player.Position,
                WeaponStatRules.Range(definition, weapon.Modifiers),
                enemy => HasLineOfSight(definition, enemy));
        }

        private bool HasLineOfSight(WeaponDefinition definition, EnemyRuntime enemy)
        {
            Vector3 aimTarget = enemy.AimPosition;
            Vector3 aimOrigin = _player.Position + Vector3.up * Mathf.Max(0.01f, definition.projectileSpawnHeight);
            Vector3 fallbackDirection = _player.Body.forward;
            Vector3 initialDirection = WeaponSpawnRules.AimDirection(aimOrigin, aimTarget, fallbackDirection);
            Vector3 muzzle = WeaponSpawnRules.MuzzlePosition(
                _player.Position,
                initialDirection,
                definition.projectileSpawnHeight,
                definition.projectileSpawnForwardOffset);
            return _targetingQuery == null || _targetingQuery.HasLineOfSight(definition, muzzle, enemy);
        }
    }
}
