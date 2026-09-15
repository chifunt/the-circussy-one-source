using Animancer;
using UnityEngine;
using TheCircussyOne.Content;

namespace TheCircussyOne.Visuals
{
    internal sealed class ActorAnimancerDriver
    {
        private AnimancerComponent animancer;
        private AnimationClip currentClip;
        private AnimancerState currentState;
        private LinearMixerState locomotionMixer;
        private ClipState locomotionIdleState;
        private ClipState locomotionRunState;
        private AnimationClip locomotionIdleClip;
        private AnimationClip locomotionRunClip;
        private bool wasPerformerGrounded = true;
        private bool isLocomotionMixerActive;
        private bool runningByMovement;
        private float landSecondsRemaining;
        private float smoothedRunMotion01;
        private float locomotionBlend01;
        private float currentSpeed;

        public bool IsConfigured => animancer != null;
        public AnimationClip CurrentClip => currentClip;
        public float CurrentSpeed => currentSpeed;
        public float CurrentBlend01 => isLocomotionMixerActive ? locomotionBlend01 : 0f;

        public bool IsPlaybackPaused => animancer != null && animancer.IsGraphInitialized && !animancer.Graph.IsGraphPlaying;

        public void Configure(GameObject modelRoot)
        {
            animancer = null;
            currentClip = null;
            currentState = null;
            locomotionMixer = null;
            locomotionIdleState = null;
            locomotionRunState = null;
            locomotionIdleClip = null;
            locomotionRunClip = null;
            landSecondsRemaining = 0f;
            wasPerformerGrounded = true;
            isLocomotionMixerActive = false;
            runningByMovement = false;
            smoothedRunMotion01 = 0f;
            locomotionBlend01 = 0f;
            currentSpeed = 0f;

            if (modelRoot == null)
            {
                return;
            }

            Animator animator = modelRoot.GetComponentInChildren<Animator>(includeInactive: true);
            if (animator == null)
            {
                animator = modelRoot.AddComponent<Animator>();
            }

            animancer = animator.GetComponent<AnimancerComponent>();
            if (animancer == null)
            {
                animancer = animator.gameObject.AddComponent<AnimancerComponent>();
            }

            animancer.Animator = animator;
            animancer.ActionOnDisable = AnimancerComponent.DisableAction.Stop;
        }

        public void Clear()
        {
            animancer = null;
            currentClip = null;
            currentState = null;
            locomotionMixer = null;
            locomotionIdleState = null;
            locomotionRunState = null;
            locomotionIdleClip = null;
            locomotionRunClip = null;
            landSecondsRemaining = 0f;
            wasPerformerGrounded = true;
            isLocomotionMixerActive = false;
            runningByMovement = false;
            smoothedRunMotion01 = 0f;
            locomotionBlend01 = 0f;
            currentSpeed = 0f;
        }

        public void SetPlaybackPaused(bool paused)
        {
            if (animancer == null || !animancer.IsGraphInitialized)
            {
                return;
            }

            if (paused)
            {
                animancer.Graph.PauseGraph();
            }
            else
            {
                animancer.Graph.UnpauseGraph();
            }
        }

        public void EvaluateCurrentPose(float deltaTime = 0.0001f)
        {
            if (animancer == null || !animancer.IsGraphInitialized)
            {
                return;
            }

            animancer.Evaluate(Mathf.Max(0f, deltaTime));
        }

        public void TickPerformer(
            PerformerAnimationProfile animation,
            bool visuallyGrounded,
            bool jumpVisualActive,
            float motionSpeed01,
            float deltaTime)
        {
            if (animancer == null || animation == null)
            {
                return;
            }

            animation.EnsureWorkflowDefaults();
            bool animationGrounded = visuallyGrounded || !jumpVisualActive;
            if (!wasPerformerGrounded && animationGrounded && animation.land != null)
            {
                landSecondsRemaining = Mathf.Max(animation.landLockSeconds, animation.fadeSeconds);
            }

            wasPerformerGrounded = animationGrounded;
            float smoothedMotion01 = TickSmoothedRunMotion(
                motionSpeed01,
                animation.runSpeedSmoothingSharpness,
                deltaTime);

            AnimationClip desired = null;
            float speed = 1f;
            if (jumpVisualActive)
            {
                desired = animation.jump;
                speed = animation.jumpSpeed;
                landSecondsRemaining = 0f;
                runningByMovement = false;
            }
            else if (landSecondsRemaining > 0f)
            {
                landSecondsRemaining = Mathf.Max(0f, landSecondsRemaining - Mathf.Max(0f, deltaTime));
                desired = animation.land;
                speed = animation.landSpeed;
                runningByMovement = false;
            }
            else if (animation.idle != null && animation.run != null)
            {
                PlayLocomotionMixer(animation, smoothedMotion01);
                return;
            }
            else if (ShouldUseRun(smoothedMotion01, animation.runThreshold01, animation.runExitThreshold01))
            {
                if (animation.run != null)
                {
                    desired = animation.run;
                    speed = ScaledRunSpeed(
                        animation.runSpeed,
                        smoothedMotion01,
                        animation.scaleRunSpeedWithMovement,
                        animation.minRunSpeedMultiplier);
                }
                else
                {
                    desired = animation.idle;
                    speed = animation.idleSpeed;
                }
            }
            else
            {
                if (animation.idle != null)
                {
                    desired = animation.idle;
                    speed = animation.idleSpeed;
                }
                else
                {
                    desired = animation.run;
                    speed = ScaledRunSpeed(
                        animation.runSpeed,
                        smoothedMotion01,
                        animation.scaleRunSpeedWithMovement,
                        animation.minRunSpeedMultiplier);
                }
            }

            Play(desired, animation.fadeSeconds, speed);
        }

        public void TickEnemy(
            EnemyAnimationProfile animation,
            EnemyLocomotionMode locomotionMode,
            float motionSpeed01,
            float deltaTime)
        {
            if (animancer == null || animation == null)
            {
                return;
            }

            animation.EnsureWorkflowDefaults();
            float smoothedMotion01 = TickSmoothedRunMotion(
                motionSpeed01,
                animation.runSpeedSmoothingSharpness,
                deltaTime);

            AnimationClip desired;
            float speed;
            if (locomotionMode == EnemyLocomotionMode.Floating)
            {
                desired = animation.floatingIdle;
                speed = animation.floatingIdleSpeed;
                runningByMovement = false;
            }
            else if (ShouldUseRun(smoothedMotion01, animation.runThreshold01, animation.runExitThreshold01))
            {
                if (animation.run != null)
                {
                    desired = animation.run;
                    speed = ScaledRunSpeed(
                        animation.runSpeed,
                        smoothedMotion01,
                        animation.scaleRunSpeedWithMovement,
                        animation.minRunSpeedMultiplier);
                }
                else
                {
                    desired = animation.idle;
                    speed = animation.idleSpeed;
                }
            }
            else
            {
                if (animation.idle != null)
                {
                    desired = animation.idle;
                    speed = animation.idleSpeed;
                }
                else
                {
                    desired = animation.run;
                    speed = ScaledRunSpeed(
                        animation.runSpeed,
                        smoothedMotion01,
                        animation.scaleRunSpeedWithMovement,
                        animation.minRunSpeedMultiplier);
                }
            }

            Play(desired, animation.fadeSeconds, speed);
        }

        private void Play(AnimationClip clip, float fadeSeconds, float speed)
        {
            if (animancer == null || clip == null)
            {
                return;
            }

            float safeSpeed = Mathf.Max(0.01f, speed);
            if (!isLocomotionMixerActive && currentClip == clip && currentState != null)
            {
                currentState.Speed = safeSpeed;
                currentSpeed = safeSpeed;
                return;
            }

            isLocomotionMixerActive = false;
            locomotionBlend01 = 0f;
            currentClip = clip;
            if (fadeSeconds > 0f)
            {
                currentState = animancer.Play(clip, fadeSeconds);
            }
            else
            {
                currentState = animancer.Play(clip);
            }

            if (currentState != null)
            {
                currentState.Speed = safeSpeed;
            }

            currentSpeed = safeSpeed;
        }

        private void PlayLocomotionMixer(PerformerAnimationProfile animation, float smoothedMotion01)
        {
            if (animancer == null || animation.idle == null || animation.run == null)
            {
                return;
            }

            EnsureLocomotionMixer(animation.idle, animation.run);
            if (locomotionMixer == null)
            {
                return;
            }

            locomotionBlend01 = Mathf.Clamp01(smoothedMotion01);
            float idleSpeed = Mathf.Max(0.01f, animation.idleSpeed);
            float runSpeed = Mathf.Max(
                0.01f,
                ScaledRunSpeed(
                    animation.runSpeed,
                    locomotionBlend01,
                    animation.scaleRunSpeedWithMovement,
                    animation.minRunSpeedMultiplier));

            locomotionMixer.Parameter = locomotionBlend01;
            locomotionMixer.Speed = 1f;
            if (locomotionIdleState != null)
            {
                locomotionIdleState.Speed = idleSpeed;
            }

            if (locomotionRunState != null)
            {
                locomotionRunState.Speed = runSpeed;
            }

            AnimationClip dominantClip = locomotionBlend01 >= 0.5f ? animation.run : animation.idle;
            float dominantSpeed = locomotionBlend01 >= 0.5f ? runSpeed : idleSpeed;
            if (isLocomotionMixerActive && currentState == locomotionMixer)
            {
                currentClip = dominantClip;
                currentSpeed = dominantSpeed;
                return;
            }

            currentState = animation.fadeSeconds > 0f
                ? animancer.Play(locomotionMixer, animation.fadeSeconds)
                : animancer.Play(locomotionMixer);
            isLocomotionMixerActive = true;
            currentClip = dominantClip;
            currentSpeed = dominantSpeed;
        }

        private void EnsureLocomotionMixer(AnimationClip idleClip, AnimationClip runClip)
        {
            if (locomotionMixer != null
                && locomotionIdleClip == idleClip
                && locomotionRunClip == runClip)
            {
                return;
            }

            locomotionIdleClip = idleClip;
            locomotionRunClip = runClip;
            locomotionMixer = new LinearMixerState
            {
                ExtrapolateSpeed = false
            };
            locomotionIdleState = locomotionMixer.Add(idleClip, 0f);
            locomotionRunState = locomotionMixer.Add(runClip, 1f);
            locomotionMixer.Parameter = Mathf.Clamp01(locomotionBlend01);
        }

        private float ScaledRunSpeed(
            float authoredRunSpeed,
            float smoothedMotion01,
            bool scaleWithMovement,
            float minRunSpeedMultiplier)
        {
            if (!scaleWithMovement)
            {
                return authoredRunSpeed;
            }

            float multiplier = Mathf.Lerp(
                Mathf.Clamp01(minRunSpeedMultiplier),
                1f,
                Mathf.Clamp01(smoothedMotion01));
            return authoredRunSpeed * multiplier;
        }

        private float TickSmoothedRunMotion(float motionSpeed01, float smoothingSharpness, float deltaTime)
        {
            float targetMotion01 = Mathf.Clamp01(motionSpeed01);
            if (smoothingSharpness <= 0f || deltaTime <= 0f)
            {
                smoothedRunMotion01 = targetMotion01;
            }
            else
            {
                float t = 1f - Mathf.Exp(-smoothingSharpness * deltaTime);
                smoothedRunMotion01 = Mathf.Lerp(smoothedRunMotion01, targetMotion01, t);
            }

            return Mathf.Clamp01(smoothedRunMotion01);
        }

        private bool ShouldUseRun(float smoothedMotion01, float enterThreshold01, float exitThreshold01)
        {
            float enterThreshold = Mathf.Clamp01(enterThreshold01);
            float exitThreshold = Mathf.Clamp(exitThreshold01, 0f, enterThreshold);
            runningByMovement = runningByMovement
                ? smoothedMotion01 > exitThreshold
                : smoothedMotion01 > enterThreshold;
            return runningByMovement;
        }
    }
}
