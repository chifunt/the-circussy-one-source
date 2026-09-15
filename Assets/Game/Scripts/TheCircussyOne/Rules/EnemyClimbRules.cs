using UnityEngine;
using TheCircussyOne.Runtime;

namespace TheCircussyOne.Rules
{
    public static class EnemyClimbRules
    {
        private const float HeightEpsilon = 0.001f;

        public static float ClimbSpeed(
            float enemyMoveSpeed,
            bool useMoveSpeedForClimb,
            float climbSpeedMultiplier,
            float fallbackClimbSpeed)
        {
            float speed = useMoveSpeedForClimb
                ? Mathf.Max(0f, enemyMoveSpeed) * Mathf.Max(0f, climbSpeedMultiplier)
                : Mathf.Max(0f, fallbackClimbSpeed);

            return speed > HeightEpsilon ? speed : Mathf.Max(0f, fallbackClimbSpeed);
        }

        public static float SupportClimbSpeed(float climbSpeed, float supportClimbSpeedMultiplier)
        {
            return Mathf.Max(0f, climbSpeed) * Mathf.Max(0f, supportClimbSpeedMultiplier);
        }

        public static bool ShouldProbeEnvironment(int frame, int spawnId, int intervalFrames, bool hasCachedHeight)
        {
            if (!hasCachedHeight)
            {
                return true;
            }

            int interval = Mathf.Max(1, intervalFrames);
            return Mathf.Abs(frame + spawnId) % interval == 0;
        }

        public static float TickVerticalMotor(
            EnemyVerticalMotorState state,
            float currentHeight,
            float targetHeight,
            float climbSpeed,
            float gravity,
            float terminalFallSpeed,
            float deltaTime)
        {
            if (state == null)
            {
                return currentHeight;
            }

            float safeTarget = Mathf.Max(0f, targetHeight);
            float dt = Mathf.Max(0f, deltaTime);
            state.CurrentHeight = currentHeight;
            state.TargetHeight = safeTarget;
            if (dt <= 0f)
            {
                return currentHeight;
            }

            float nextHeight = currentHeight;
            if (currentHeight < safeTarget - HeightEpsilon)
            {
                float speed = Mathf.Max(0f, climbSpeed);
                nextHeight = Mathf.MoveTowards(currentHeight, safeTarget, speed * dt);
                state.VerticalVelocity = nextHeight >= safeTarget - HeightEpsilon ? 0f : speed;
            }
            else if (currentHeight > safeTarget + HeightEpsilon)
            {
                float velocity = Mathf.Min(0f, state.VerticalVelocity);
                velocity = Mathf.Max(-Mathf.Max(0f, terminalFallSpeed), velocity - Mathf.Max(0f, gravity) * dt);
                nextHeight = currentHeight + velocity * dt;
                if (nextHeight <= safeTarget)
                {
                    nextHeight = safeTarget;
                    velocity = 0f;
                }

                state.VerticalVelocity = velocity;
            }
            else
            {
                nextHeight = safeTarget;
                state.VerticalVelocity = 0f;
            }

            state.CurrentHeight = nextHeight;
            return nextHeight;
        }
    }
}
