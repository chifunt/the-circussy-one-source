using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;

namespace TheCircussyOne.Runtime
{
    public sealed class EnemyClimbSystem : ITickable
    {
        private readonly GameConfig _config;
        private readonly ActorRegistry _registry;
        private readonly IGameTime _time;
        private readonly EnemyEnvironmentSurfaceSampler _surfaceSampler;
        private readonly RunPhaseState _phaseState;

        private int _frame;

        public EnemyClimbSystem(GameConfig config, ActorRegistry registry, IGameTime time)
            : this(config, registry, time, new EnemyEnvironmentSurfaceSampler())
        {
        }

        public EnemyClimbSystem(
            GameConfig config,
            ActorRegistry registry,
            IGameTime time,
            EnemyEnvironmentSurfaceSampler surfaceSampler,
            RunPhaseState phaseState = null)
        {
            _config = config;
            _registry = registry;
            _time = time;
            _surfaceSampler = surfaceSampler;
            _phaseState = phaseState;
        }

        public void Tick()
        {
            var enemies = _registry.Enemies;
            if (_phaseState != null && !RunPhaseRules.ShouldTickCombat(_phaseState.CurrentPhase))
            {
                return;
            }

            if (_config == null || !_config.enemyClimbingEnabled)
            {
                ResetEnemyHeights(enemies);
                _frame++;
                return;
            }

            for (int i = 0; i < enemies.Count; i++)
            {
                EnemyRuntime enemy = enemies[i];
                if (!IsClimbable(enemy))
                {
                    continue;
                }

                EnemyVerticalMotorState motor = enemy.VerticalMotor;
                EnemyClimbProfile climb = enemy.ClimbProfile;
                EnemyStackProfile stack = enemy.StackProfile;
                float environmentHeight = climb.canClimbEnvironment
                    ? SampleEnvironmentHeightIfDue(enemy, motor, climb)
                    : 0f;
                float supportHeight = ActiveSupportHeightFor(enemy, enemies);

                float targetHeight = Mathf.Max(0f, environmentHeight, supportHeight);
                if (motor.IsEnvironmentClimbing)
                {
                    targetHeight = Mathf.Max(targetHeight, motor.BlockedClimbTargetHeight);
                }

                float currentHeight = enemy.Position.y;
                float climbSpeed = EnemyClimbRules.ClimbSpeed(
                    enemy.MoveSpeed,
                    climb.useMoveSpeedForClimb,
                    climb.climbSpeedMultiplier,
                    climb.fallbackClimbSpeed);
                if (ShouldUseSupportClimbSpeed(enemy, currentHeight, environmentHeight, supportHeight))
                {
                    climbSpeed = EnemyClimbRules.SupportClimbSpeed(climbSpeed, stack.supportClimbSpeedMultiplier);
                }

                float nextHeight = EnemyClimbRules.TickVerticalMotor(
                    motor,
                    currentHeight,
                    targetHeight,
                    climbSpeed,
                    climb.gravity,
                    climb.terminalFallSpeed,
                    _time.DeltaTime);

                enemy.View.SetHeight(nextHeight);
                if (EnemyTerrainLocomotionRules.HasReachedClimbReleaseHeight(motor, nextHeight))
                {
                    motor.ClearEnvironmentClimb();
                }
            }

            _frame++;
        }

        private float SampleEnvironmentHeightIfDue(EnemyRuntime enemy, EnemyVerticalMotorState motor, EnemyClimbProfile climb)
        {
            if (!EnemyClimbRules.ShouldProbeEnvironment(
                _frame,
                enemy.SpawnId,
                climb.environmentProbeIntervalFrames,
                motor.HasCachedEnvironmentHeight))
            {
                return motor.CachedEnvironmentHeight;
            }

            float sampledHeight = 0f;
            int mask = EnvironmentMaskExcludingGameplay();
            Vector3 position = enemy.Position;
            float currentEnemyHeight = position.y;
            if (_surfaceSampler.TrySampleHeight(position, climb.maxEnvironmentClimbHeight, climb.environmentProbeDistance, mask, out float currentHeight))
            {
                sampledHeight = PassiveEnvironmentHeight(currentEnemyHeight, currentHeight, sampledHeight);
            }

            Vector3 forwardProbe = position + enemy.View.HorizontalForward * Mathf.Max(0f, climb.environmentProbeDistance);
            if (_surfaceSampler.TrySampleHeight(forwardProbe, climb.maxEnvironmentClimbHeight, climb.environmentProbeDistance, mask, out float forwardHeight))
            {
                sampledHeight = PassiveEnvironmentHeight(currentEnemyHeight, forwardHeight, sampledHeight);
            }

            motor.CachedEnvironmentHeight = sampledHeight;
            motor.HasCachedEnvironmentHeight = true;
            return sampledHeight;
        }

        private static float PassiveEnvironmentHeight(float currentEnemyHeight, float sampledHeight, float acceptedHeight)
        {
            if (!EnemyTerrainLocomotionRules.IsWalkableHeight(currentEnemyHeight, sampledHeight))
            {
                return acceptedHeight;
            }

            return Mathf.Max(acceptedHeight, sampledHeight);
        }

        private float ActiveSupportHeightFor(EnemyRuntime enemy, IReadOnlyList<EnemyRuntime> enemies)
        {
            EnemyVerticalMotorState motor = enemy.VerticalMotor;
            if (!_config.enemyPileClimbingEnabled
                || !EnemySupportStackRules.CanUseSupportStack(enemy.StackProfile, enemy.LocomotionProfile)
                || motor == null
                || !motor.IsSupportClimbing)
            {
                motor?.ClearSupportClimb();
                return 0f;
            }

            EnemyRuntime support = FindSupportBySpawnId(enemies, motor.SupportClimbTargetSpawnId);
            if (!EnemySupportSurfaceSampler.IsValidSupportCandidate(enemy, support))
            {
                motor.ClearSupportClimb();
                return 0f;
            }

            float enemyRadius = enemy.BodyProfile != null ? enemy.BodyProfile.movementBodyRadius : 0f;
            float supportRadius = support.BodyProfile != null ? support.BodyProfile.movementBodyRadius : 0f;
            if (!EnemySupportClimbRules.IsWithinSupportReach(enemy.Position, enemyRadius, support.Position, supportRadius))
            {
                motor.ClearSupportClimb();
                return 0f;
            }

            int supportLayer = support.VerticalMotor != null ? support.VerticalMotor.StackLayer : 0;
            int targetLayer = EnemySupportClimbRules.TargetLayer(supportLayer, enemy.StackProfile.maxStackLayers);
            if (targetLayer <= 0)
            {
                motor.ClearSupportClimb();
                return 0f;
            }

            float targetHeight = EnemySupportClimbRules.TargetRootHeight(
                support.Position,
                support.BodyProfile,
                enemy.BodyProfile,
                enemy.StackProfile.layerHeightMultiplier);
            motor.BeginSupportClimb(
                targetHeight,
                EnemySupportClimbRules.SupportReleaseTolerance,
                targetLayer,
                support.SpawnId);
            return targetHeight;
        }

        private int EnvironmentMaskExcludingGameplay()
        {
            int configuredMask = _config.enemyClimbEnvironmentMask.value;
            int gameplayMask = LayerMask.GetMask(
                GameLayers.Player,
                GameLayers.Enemy,
                GameLayers.Projectile,
                GameLayers.Pickup);
            return configuredMask & ~gameplayMask;
        }

        private static void ResetEnemyHeights(IReadOnlyList<EnemyRuntime> enemies)
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                EnemyRuntime enemy = enemies[i];
                if (!IsClimbable(enemy))
                {
                    continue;
                }

                enemy.VerticalMotor.Reset(0f);
                enemy.View.SetHeight(0f);
            }
        }

        private static bool IsClimbable(EnemyRuntime enemy)
        {
            return enemy != null
                && !enemy.IsDead
                && enemy.View != null
                && enemy.View.IsActive
                && enemy.LocomotionProfile.mode != EnemyLocomotionMode.Floating
                && enemy.VerticalMotor != null;
        }

        private static EnemyRuntime FindSupportBySpawnId(IReadOnlyList<EnemyRuntime> enemies, int spawnId)
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                EnemyRuntime enemy = enemies[i];
                if (enemy != null && enemy.SpawnId == spawnId)
                {
                    return enemy;
                }
            }

            return null;
        }

        private static bool ShouldUseSupportClimbSpeed(
            EnemyRuntime enemy,
            float currentHeight,
            float environmentHeight,
            float supportHeight)
        {
            return enemy != null
                && enemy.VerticalMotor != null
                && enemy.VerticalMotor.IsSupportClimbing
                && supportHeight > currentHeight + 0.001f
                && supportHeight >= environmentHeight - 0.001f;
        }
    }
}
