using UnityEngine;
using VContainer.Unity;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;
using TheCircussyOne.Visuals;

namespace TheCircussyOne.Runtime
{
    public sealed class DamageNumberSystem : IDamageNumberSpawner, ILateTickable
    {
        private readonly DamageFeedbackVisualConfig config;
        private readonly DamageNumberFactory factory;
        private readonly CameraView cameraView;
        private readonly IGameTime time;
        private int nextSeed = 1;

        public DamageNumberSystem(
            DamageFeedbackVisualConfig config,
            DamageNumberFactory factory,
            CameraView cameraView,
            IGameTime time)
        {
            this.config = config;
            this.factory = factory;
            this.cameraView = cameraView;
            this.time = time;
        }

        public void Show(DamageNumberRequest request)
        {
            if (config == null || factory == null || !config.damageNumbersEnabled || request.Amount <= 0)
            {
                return;
            }

            if (config.damageNumberReadabilityMode == DamageNumberReadabilityMode.HybridMerge
                && factory.TryMerge(request, time.Time))
            {
                return;
            }

            factory.Spawn(request, time.Time, nextSeed++);
        }

        public void LateTick()
        {
            if (factory == null || config == null)
            {
                return;
            }

            Camera camera = cameraView != null ? cameraView.Camera : Camera.main;
            for (int i = factory.Active.Count - 1; i >= 0; i--)
            {
                DamageNumberRuntime damageNumber = factory.Active[i];
                if (damageNumber == null || damageNumber.View == null || !damageNumber.View.IsActive || damageNumber.IsExpired(time.Time))
                {
                    factory.Despawn(damageNumber);
                    continue;
                }

                float normalizedAge = DamageNumberRules.NormalizedAge(damageNumber.SpawnedAt, time.Time, damageNumber.Lifetime);
                DamageNumberFrame frame = DamageNumberRules.Evaluate(
                    damageNumber.StartPosition,
                    normalizedAge,
                    config.floatDistance,
                    damageNumber.DriftDirection,
                    config.damageNumberLateralDriftDistance,
                    config.baseWorldScale,
                    config.startScale,
                    config.popScale,
                    config.endScale,
                    config.popDurationNormalized,
                    config.fadeStartNormalized,
                    config.numberColor,
                    config.damageNumberFloatEase,
                    config.damageNumberFadeEase,
                    config.damageNumberShrinkEase,
                    config.damageNumberPopEase);
                damageNumber.View.ApplyFrame(frame, camera);
            }
        }
    }
}
