using UnityEngine;
using VContainer.Unity;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;
using TheCircussyOne.Visuals;

namespace TheCircussyOne.Runtime
{
    public sealed class XpGainCounterSystem : IXpGainCounter, ILateTickable
    {
        private readonly XpGainCounterVisualConfig config;
        private readonly PlayerView player;
        private readonly CameraView cameraView;
        private readonly IGameTime time;
        private readonly XpGainCounterFactory factory;
        private readonly XpGainCounterState state = new();

        public XpGainCounterSystem(
            XpGainCounterVisualConfig config,
            PlayerView player,
            CameraView cameraView,
            IGameTime time,
            XpGainCounterFactory factory)
        {
            this.config = config;
            this.player = player;
            this.cameraView = cameraView;
            this.time = time;
            this.factory = factory;
        }

        public int Amount => state.Amount;
        public bool IsVisible => state.IsVisible && factory?.View != null && factory.View.IsActive;

        public int Prewarm()
        {
            return factory != null ? factory.Prewarm() : 0;
        }

        public void Add(int amount)
        {
            if (config == null || !config.enabled || amount <= 0)
            {
                return;
            }

            XpGainCounterView view = factory?.GetOrCreate();
            if (view == null)
            {
                return;
            }

            state.Add(amount, time.Time, config.holdSeconds, config.fadeSeconds);
            view.Prepare(config);
        }

        public void LateTick()
        {
            XpGainCounterView view = factory?.View;
            if (config == null || view == null || player == null || time == null)
            {
                return;
            }

            if (XpGainCounterRules.IsExpired(state, time.Time, config.holdSeconds, config.fadeSeconds))
            {
                state.Reset();
                factory.Release();
                return;
            }

            Camera camera = cameraView != null ? cameraView.Camera : Camera.main;
            Vector3 cameraForward = camera != null ? camera.transform.forward : Vector3.forward;
            Vector3 cameraRight = camera != null ? camera.transform.right : Vector3.right;
            XpGainCounterFrame frame = XpGainCounterRules.Evaluate(
                state,
                time.Time,
                player.Position,
                cameraForward,
                cameraRight,
                config.rightOffset,
                config.heightOffset,
                config.forwardOffset,
                config.holdSeconds,
                config.fadeSeconds,
                config.baseWorldScale,
                config.normalScale,
                config.popScale,
                config.popReturnSeconds,
                config.textColor,
                config.popEase,
                config.fadeEase);
            view.ApplyFrame(frame, camera);
        }
    }
}
