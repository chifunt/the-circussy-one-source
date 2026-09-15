using UnityEngine;

namespace TheCircussyOne.Rules
{
    public static class PlayerMovementRules
    {
        private const float WalkInputThreshold = 0.5f;
        private const float GroundStickVelocity = -2f;

        public static Vector2 NormalizeInput(Vector2 input, float deadzone)
        {
            float magnitude = input.magnitude;
            if (magnitude <= deadzone)
            {
                return Vector2.zero;
            }

            if (magnitude > 1f)
            {
                input /= magnitude;
                magnitude = 1f;
            }

            float scaledMagnitude = Mathf.InverseLerp(deadzone, 1f, magnitude);
            return input.normalized * scaledMagnitude;
        }

        public static Vector3 CameraRelativeDirection(Vector2 input, float cameraYawDegrees)
        {
            Vector2 normalized = NormalizeInput(input, 0f);
            if (normalized == Vector2.zero)
            {
                return Vector3.zero;
            }

            Quaternion yaw = Quaternion.Euler(0f, cameraYawDegrees, 0f);
            Vector3 forward = yaw * Vector3.forward;
            Vector3 right = yaw * Vector3.right;
            Vector3 direction = right * normalized.x + forward * normalized.y;
            direction.y = 0f;
            return direction.sqrMagnitude > 0f ? direction.normalized : Vector3.zero;
        }

        public static float TargetSpeed(float inputMagnitude, float walkSpeed, float runSpeed, float speedMultiplier)
        {
            if (inputMagnitude <= 0f)
            {
                return 0f;
            }

            float baseSpeed = inputMagnitude <= WalkInputThreshold
                ? Mathf.Lerp(0f, walkSpeed, inputMagnitude / WalkInputThreshold)
                : Mathf.Lerp(walkSpeed, runSpeed, (inputMagnitude - WalkInputThreshold) / (1f - WalkInputThreshold));
            return baseSpeed * Mathf.Max(0f, speedMultiplier);
        }

        public static void Tick(
            PlayerMotorState state,
            Vector2 rawInput,
            float cameraYawDegrees,
            float speedMultiplier,
            float deadzone,
            float walkSpeed,
            float runSpeed,
            float acceleration,
            float deceleration,
            float turnAcceleration,
            float reverseSkidAngle,
            float skidFriction,
            float rotationSharpness,
            float deltaTime)
        {
            Tick(
                state,
                rawInput,
                cameraYawDegrees,
                speedMultiplier,
                deadzone,
                walkSpeed,
                runSpeed,
                acceleration,
                deceleration,
                turnAcceleration,
                reverseSkidAngle,
                skidFriction,
                rotationSharpness,
                deltaTime,
                EaseSettings.Exponential);
        }

        public static void Tick(
            PlayerMotorState state,
            Vector2 rawInput,
            float cameraYawDegrees,
            float speedMultiplier,
            float deadzone,
            float walkSpeed,
            float runSpeed,
            float acceleration,
            float deceleration,
            float turnAcceleration,
            float reverseSkidAngle,
            float skidFriction,
            float rotationSharpness,
            float deltaTime,
            EaseSettings facingEase)
        {
            TickHorizontal(
                state,
                rawInput,
                cameraYawDegrees,
                speedMultiplier,
                deadzone,
                walkSpeed,
                runSpeed,
                acceleration,
                deceleration,
                turnAcceleration,
                reverseSkidAngle,
                skidFriction,
                rotationSharpness,
                deltaTime,
                facingEase,
                allowSkid: true);
        }

        public static void Tick(
            PlayerMotorState state,
            Vector2 rawInput,
            bool jumpPressedThisFrame,
            bool jumpHeld,
            bool isGrounded,
            float cameraYawDegrees,
            float speedMultiplier,
            float deadzone,
            float walkSpeed,
            float runSpeed,
            float acceleration,
            float deceleration,
            float turnAcceleration,
            float airAcceleration,
            float airDeceleration,
            float airTurnAcceleration,
            float reverseSkidAngle,
            float skidFriction,
            float rotationSharpness,
            int maxJumpCount,
            float jumpHeight,
            float gravity,
            float fallGravityMultiplier,
            float lowJumpGravityMultiplier,
            float terminalFallSpeed,
            float coyoteSeconds,
            float jumpBufferSeconds,
            float deltaTime,
            EaseSettings facingEase)
        {
            if (state == null)
            {
                return;
            }

            BeginFrame(state, isGrounded, coyoteSeconds, deltaTime);
            UpdateJumpBuffer(state, jumpPressedThisFrame, jumpBufferSeconds, deltaTime);

            bool groundedForHorizontal = state.IsGrounded;
            TickHorizontal(
                state,
                rawInput,
                cameraYawDegrees,
                speedMultiplier,
                deadzone,
                walkSpeed,
                runSpeed,
                groundedForHorizontal ? acceleration : airAcceleration,
                groundedForHorizontal ? deceleration : airDeceleration,
                groundedForHorizontal ? turnAcceleration : airTurnAcceleration,
                reverseSkidAngle,
                skidFriction,
                rotationSharpness,
                deltaTime,
                facingEase,
                allowSkid: groundedForHorizontal);

            if (state.JumpBufferTimer > 0f && CanStartJump(state, maxJumpCount))
            {
                StartJump(state, jumpHeight, gravity);
                return;
            }

            TickGravity(
                state,
                jumpHeld,
                gravity,
                fallGravityMultiplier,
                lowJumpGravityMultiplier,
                terminalFallSpeed,
                deltaTime);
        }

        public static void ApplyMoveResult(PlayerMotorState state, bool isGroundedAfterMove)
        {
            ApplyMoveResult(state, isGroundedAfterMove, 0f);
        }

        public static float NextVisualAirTime(float currentVisualAirTime, bool visuallyGrounded, float deltaTime)
        {
            return visuallyGrounded
                ? 0f
                : Mathf.Max(0f, currentVisualAirTime) + Mathf.Max(0f, deltaTime);
        }

        public static bool ShouldPlayLandingPulse(bool landedThisFrame, float visualAirTimeBeforeLanding, float minimumLandingAirTime)
        {
            return landedThisFrame
                && visualAirTimeBeforeLanding >= Mathf.Max(0f, minimumLandingAirTime);
        }

        public static bool IsGroundMotionVisualSupported(
            bool visuallyGrounded,
            float visualAirTime,
            float graceSeconds,
            float verticalVelocity)
        {
            return visuallyGrounded
                || (verticalVelocity <= 0f && visualAirTime < Mathf.Max(0f, graceSeconds));
        }

        public static bool ShouldPlayAirborneAnimation(
            bool jumpStartedThisFrame,
            bool visuallyGrounded,
            float visualAirTime,
            float minimumAirTime,
            float verticalVelocity)
        {
            if (jumpStartedThisFrame)
            {
                return true;
            }

            if (visuallyGrounded)
            {
                return false;
            }

            if (verticalVelocity > 0.001f)
            {
                return true;
            }

            return visualAirTime >= Mathf.Max(0f, minimumAirTime);
        }

        public static void ApplyMoveResult(PlayerMotorState state, bool isGroundedAfterMove, float minimumLandingAirTime)
        {
            if (state == null)
            {
                return;
            }

            bool wasGrounded = state.IsGrounded;
            float airTimeBeforeLanding = state.AirTime;
            if (state.JumpStartedThisFrame && state.VerticalVelocity > 0f)
            {
                state.IsGrounded = false;
                return;
            }

            state.IsGrounded = isGroundedAfterMove;
            if (!isGroundedAfterMove)
            {
                return;
            }

            if (!wasGrounded && state.VerticalVelocity <= 0f && airTimeBeforeLanding >= Mathf.Max(0f, minimumLandingAirTime))
            {
                state.LandedThisFrame = true;
            }

            state.JumpsUsed = 0;
            state.AirTime = 0f;
            state.CoyoteTimer = 0f;
            if (state.VerticalVelocity <= 0f)
            {
                state.VerticalVelocity = GroundStickVelocity;
            }
        }

        public static bool CanStartJump(PlayerMotorState state, int maxJumpCount)
        {
            if (state == null)
            {
                return false;
            }

            int maxCount = Mathf.Max(1, maxJumpCount);
            return state.IsGrounded
                || state.CoyoteTimer > 0f
                || (state.JumpsUsed > 0 && state.JumpsUsed < maxCount);
        }

        public static float InitialJumpVelocity(float jumpHeight, float gravity)
        {
            return Mathf.Sqrt(2f * Mathf.Max(0.001f, gravity) * Mathf.Max(0f, jumpHeight));
        }

        private static void BeginFrame(PlayerMotorState state, bool isGrounded, float coyoteSeconds, float deltaTime)
        {
            state.JumpStartedThisFrame = false;
            state.LandedThisFrame = false;
            state.WasGrounded = state.IsGrounded;
            state.IsGrounded = isGrounded;

            if (state.IsGrounded && state.VerticalVelocity <= 0f)
            {
                state.CoyoteTimer = Mathf.Max(0f, coyoteSeconds);
                state.JumpsUsed = 0;
                state.AirTime = 0f;
            }
            else
            {
                state.CoyoteTimer = Mathf.Max(0f, state.CoyoteTimer - Mathf.Max(0f, deltaTime));
                state.AirTime += Mathf.Max(0f, deltaTime);
            }
        }

        private static void UpdateJumpBuffer(PlayerMotorState state, bool jumpPressedThisFrame, float jumpBufferSeconds, float deltaTime)
        {
            if (jumpPressedThisFrame)
            {
                state.JumpBufferTimer = Mathf.Max(0f, jumpBufferSeconds);
            }
            else
            {
                state.JumpBufferTimer = Mathf.Max(0f, state.JumpBufferTimer - Mathf.Max(0f, deltaTime));
            }
        }

        private static void StartJump(PlayerMotorState state, float jumpHeight, float gravity)
        {
            state.VerticalVelocity = InitialJumpVelocity(jumpHeight, gravity);
            state.IsGrounded = false;
            state.CoyoteTimer = 0f;
            state.JumpBufferTimer = 0f;
            state.JumpStartedThisFrame = true;
            state.LandedThisFrame = false;
            state.AirTime = 0f;
            state.JumpsUsed = Mathf.Max(1, state.JumpsUsed + 1);
        }

        private static void TickGravity(
            PlayerMotorState state,
            bool jumpHeld,
            float gravity,
            float fallGravityMultiplier,
            float lowJumpGravityMultiplier,
            float terminalFallSpeed,
            float deltaTime)
        {
            if (state.IsGrounded && state.VerticalVelocity <= 0f)
            {
                state.VerticalVelocity = GroundStickVelocity;
                return;
            }

            float multiplier = 1f;
            if (state.VerticalVelocity < 0f)
            {
                multiplier = Mathf.Max(0f, fallGravityMultiplier);
            }
            else if (!jumpHeld)
            {
                multiplier = Mathf.Max(0f, lowJumpGravityMultiplier);
            }

            state.VerticalVelocity = Mathf.Max(
                state.VerticalVelocity - Mathf.Max(0f, gravity) * multiplier * Mathf.Max(0f, deltaTime),
                -Mathf.Max(0.01f, terminalFallSpeed));
        }

        private static void TickHorizontal(
            PlayerMotorState state,
            Vector2 rawInput,
            float cameraYawDegrees,
            float speedMultiplier,
            float deadzone,
            float walkSpeed,
            float runSpeed,
            float acceleration,
            float deceleration,
            float turnAcceleration,
            float reverseSkidAngle,
            float skidFriction,
            float rotationSharpness,
            float deltaTime,
            EaseSettings facingEase,
            bool allowSkid)
        {
            Vector2 input = NormalizeInput(rawInput, deadzone);
            float inputMagnitude = Mathf.Clamp01(input.magnitude);
            Vector3 desiredDirection = CameraRelativeDirection(input, cameraYawDegrees);
            Vector3 currentVelocity = state.Velocity;

            if (inputMagnitude <= 0f || desiredDirection == Vector3.zero)
            {
                state.Velocity = Vector3.MoveTowards(currentVelocity, Vector3.zero, deceleration * deltaTime);
                state.IsSkidding = false;
                UpdateFacing(state, rotationSharpness, deltaTime, facingEase);
                return;
            }

            float currentSpeed = currentVelocity.magnitude;
            Vector3 currentDirection = currentSpeed > 0.001f ? currentVelocity / currentSpeed : desiredDirection;
            float reverseDot = Mathf.Cos(reverseSkidAngle * Mathf.Deg2Rad);
            bool shouldSkid = allowSkid && currentSpeed > walkSpeed && Vector3.Dot(currentDirection, desiredDirection) < reverseDot;
            state.IsSkidding = shouldSkid;

            if (shouldSkid)
            {
                currentVelocity = Vector3.MoveTowards(currentVelocity, Vector3.zero, skidFriction * deltaTime);
            }

            float targetSpeed = TargetSpeed(inputMagnitude, walkSpeed, runSpeed, speedMultiplier);
            Vector3 targetVelocity = desiredDirection * targetSpeed;
            float accel = shouldSkid ? turnAcceleration : acceleration;
            state.Velocity = Vector3.MoveTowards(currentVelocity, targetVelocity, accel * deltaTime);
            UpdateFacing(state, rotationSharpness, deltaTime, facingEase);
        }

        private static void UpdateFacing(PlayerMotorState state, float rotationSharpness, float deltaTime, EaseSettings facingEase)
        {
            Vector3 velocity = state.Velocity;
            velocity.y = 0f;
            if (velocity.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            Vector3 target = velocity.normalized;
            if (state.Facing.sqrMagnitude <= 0.0001f)
            {
                state.Facing = target;
                return;
            }

            float t = GameEasing.SmoothingWeight(rotationSharpness, deltaTime, facingEase);
            state.Facing = Vector3.Slerp(state.Facing.normalized, target, t).normalized;
        }
    }
}
