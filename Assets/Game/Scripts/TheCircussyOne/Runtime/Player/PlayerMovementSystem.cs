using UnityEngine;
using VContainer.Unity;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;
using TheCircussyOne.Stats;
using TheCircussyOne.Visuals;

namespace TheCircussyOne.Runtime
{
    public sealed class PlayerMovementSystem : ITickable
    {
        private readonly GameConfig _config;
        private readonly PlayerView _player;
        private readonly GameState _state;
        private readonly IInputService _input;
        private readonly IGameTime _time;
        private readonly CameraOrbitState _cameraOrbit;
        private readonly RunStats _stats;
        private readonly ActorMotionVisualConfig _actorMotionConfig;
        private readonly IVfxSpawner _vfx;
        private readonly IGameAudio _audio;
        private readonly IGameHaptics _haptics;
        private readonly RunPauseState _pauseState;
        private readonly PlayerMotorState _motor = new();
        private float _visualAirTime;
        private bool _movementLoopPlaying;

        public PlayerMovementSystem(
            GameConfig config,
            PlayerView player,
            GameState state,
            IInputService input,
            IGameTime time,
            CameraOrbitState cameraOrbit,
            ActorMotionVisualConfig actorMotionConfig = null,
            IVfxSpawner vfx = null,
            RunStats stats = null,
            IGameAudio audio = null,
            IGameHaptics haptics = null,
            RunPauseState pauseState = null)
        {
            _config = config;
            _player = player;
            _state = state;
            _input = input;
            _time = time;
            _cameraOrbit = cameraOrbit;
            _stats = stats ?? new RunStats(config);
            _actorMotionConfig = actorMotionConfig;
            _vfx = vfx;
            _audio = audio ?? NullGameAudio.Instance;
            _haptics = haptics ?? NullGameHaptics.Instance;
            _pauseState = pauseState;
        }

        public void Tick()
        {
            if (_state.IsGameOver || _player == null || !_player.CanMove)
            {
                StopMovementLoop();
                _player?.SetMotionVisualSpeed(0f);
                _player?.SetAirborneVisual(false);
                _player?.SetJumpHoldVisual(false, false, false);
                _player?.SetJumpAnimationVisual(false);
                _visualAirTime = 0f;
                return;
            }

            float moveSpeedMultiplier = _stats.GetFloat(StatId.PlayerMoveSpeedMultiplier);
            int maxJumpCount = _config.playerMaxJumpCount + _stats.GetInt(StatId.PlayerExtraJumps);

            PlayerMovementRules.Tick(
                _motor,
                _input.Movement,
                _input.JumpPressedThisFrame,
                _input.JumpHeld,
                _motor.IsGrounded,
                _cameraOrbit.Yaw,
                moveSpeedMultiplier,
                _config.playerInputDeadzone,
                _config.playerWalkSpeed,
                _config.playerRunSpeed,
                _config.playerAcceleration,
                _config.playerDeceleration,
                _config.playerTurnAcceleration,
                _config.playerAirAcceleration,
                _config.playerAirDeceleration,
                _config.playerAirTurnAcceleration,
                _config.playerReverseSkidAngle,
                _config.playerSkidFriction,
                _config.playerRotationSharpness,
                maxJumpCount,
                _config.playerJumpHeight,
                _config.playerGravity,
                _config.playerFallGravityMultiplier,
                _config.playerLowJumpGravityMultiplier,
                _config.playerTerminalFallSpeed,
                _config.playerCoyoteSeconds,
                _config.playerJumpBufferSeconds,
                _time.DeltaTime,
                _config.playerFacingSmoothingEase);

            Vector2 normalizedInput = PlayerMovementRules.NormalizeInput(_input.Movement, _config.playerInputDeadzone);
            float inputIntent01 = Mathf.Clamp01(normalizedInput.magnitude);
            float velocity01 = ActorMotionVisualRules.NormalizeSpeed(
                _motor.Velocity.magnitude,
                _config.playerRunSpeed * moveSpeedMultiplier);

            PlayerMoveResult moveResult = _player.Move(_motor.FullVelocity, _motor.Facing, _time.DeltaTime);
            bool groundedForMotor = moveResult.IsGrounded
                || (moveResult.IsWalkableGrounded && _motor.VerticalVelocity <= 0f);
            float visualAirTimeBeforeLanding = _visualAirTime;
            _visualAirTime = PlayerMovementRules.NextVisualAirTime(
                _visualAirTime,
                moveResult.IsVisuallyGrounded || groundedForMotor,
                _time.DeltaTime);
            PlayerMovementRules.ApplyMoveResult(_motor, groundedForMotor, _config.playerLandingPulseMinAirTime);
            bool visuallyGrounded = _motor.IsGrounded || moveResult.IsVisuallyGrounded;
            bool jumpAnimationActive = PlayerMovementRules.ShouldPlayAirborneAnimation(
                _motor.JumpStartedThisFrame,
                visuallyGrounded,
                _visualAirTime,
                _config.playerAirborneAnimationMinAirTime,
                _motor.VerticalVelocity);
            _player.SetAirborneVisual(!visuallyGrounded);
            _player.SetJumpHoldVisual(_input.JumpHeld, _motor.VerticalVelocity > 0f, !visuallyGrounded);
            _player.SetJumpAnimationVisual(jumpAnimationActive);
            bool groundMotionVisualSupported = PlayerMovementRules.IsGroundMotionVisualSupported(
                visuallyGrounded,
                _visualAirTime,
                _config.playerLandingPulseMinAirTime,
                _motor.VerticalVelocity);
            bool suppressGroundSquash = !groundMotionVisualSupported || _motor.JumpStartedThisFrame;
            _player.SetMotionVisualSpeed(
                suppressGroundSquash ? 0f : Mathf.Max(inputIntent01, velocity01),
                immediate: suppressGroundSquash);

            if (_motor.JumpStartedThisFrame)
            {
                _player.PlayJumpTakeoffPulse(_actorMotionConfig);
                _vfx?.Show(VfxEffectId.PlayerJumpTakeoff, _player.Position + Vector3.up * 0.06f, Vector3.up);
                _audio.PlayAt(GameAudioCue.PlayerJump, _player.Position);
                _haptics.Play(GameHapticsCue.PlayerJump);
            }

            if (PlayerMovementRules.ShouldPlayLandingPulse(
                    _motor.LandedThisFrame,
                    visualAirTimeBeforeLanding,
                    _config.playerLandingPulseMinAirTime))
            {
                _player.PlayJumpLandPulse(_actorMotionConfig);
                _vfx?.Show(VfxEffectId.PlayerJumpLand, _player.Position + Vector3.up * 0.06f, Vector3.up);
                _audio.PlayAt(GameAudioCue.PlayerLand, _player.Position);
                _haptics.Play(GameHapticsCue.PlayerLand);
            }

            TickMovementLoop(groundMotionVisualSupported ? Mathf.Max(inputIntent01, velocity01) : 0f);
        }

        private void TickMovementLoop(float movement01)
        {
            if (_pauseState != null && _pauseState.IsPaused)
            {
                StopMovementLoop();
                return;
            }

            bool shouldPlay = movement01 > 0.08f && _player != null && _player.CanMove && !_state.IsGameOver;
            if (shouldPlay && !_movementLoopPlaying)
            {
                _audio.StartLoop(GameAudioCue.PlayerMovementLoop, this, _player.Position, movement01);
                _movementLoopPlaying = true;
            }
            else if (!shouldPlay && _movementLoopPlaying)
            {
                StopMovementLoop();
                return;
            }

            if (_movementLoopPlaying)
            {
                _audio.UpdateLoop(GameAudioCue.PlayerMovementLoop, this, _player.Position, movement01);
            }
        }

        private void StopMovementLoop()
        {
            if (!_movementLoopPlaying)
            {
                return;
            }

            _audio.StopLoop(GameAudioCue.PlayerMovementLoop, this);
            _movementLoopPlaying = false;
        }
    }
}
