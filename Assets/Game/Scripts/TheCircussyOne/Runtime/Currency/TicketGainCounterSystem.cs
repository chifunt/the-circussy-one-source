using System;
using UnityEngine;
using VContainer.Unity;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;
using TheCircussyOne.Visuals;

namespace TheCircussyOne.Runtime
{
    public sealed class TicketGainCounterSystem : IStartable, ILateTickable, IDisposable
    {
        private readonly XpGainCounterVisualConfig config;
        private readonly RunCurrencyState currency;
        private readonly PlayerView player;
        private readonly CameraView cameraView;
        private readonly IGameTime time;
        private readonly XpGainCounterFactory factory;
        private readonly XpGainCounterState state = new();
        private int lastTickets;

        public TicketGainCounterSystem(
            XpGainCounterVisualConfig config,
            RunCurrencyState currency,
            PlayerView player,
            CameraView cameraView,
            IGameTime time,
            XpGainCounterFactory factory)
        {
            this.config = config;
            this.currency = currency;
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

        public void Start()
        {
            lastTickets = currency?.Tickets ?? 0;
            if (currency != null)
            {
                currency.TicketsChanged += OnTicketsChanged;
            }
        }

        public void Dispose()
        {
            if (currency != null)
            {
                currency.TicketsChanged -= OnTicketsChanged;
            }
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
                config.ticketRightOffset,
                config.ticketHeightOffset,
                config.ticketForwardOffset,
                config.holdSeconds,
                config.fadeSeconds,
                config.baseWorldScale,
                config.normalScale,
                config.popScale,
                config.popReturnSeconds,
                config.ticketTextColor,
                config.popEase,
                config.fadeEase);
            view.ApplyFrame(frame, camera);
        }

        private void OnTicketsChanged(int tickets)
        {
            int delta = tickets - lastTickets;
            lastTickets = tickets;

            if (delta <= 0 || config == null || !config.enabled || !config.ticketCounterEnabled)
            {
                return;
            }

            XpGainCounterView view = factory?.GetOrCreate();
            if (view == null)
            {
                return;
            }

            state.Add(delta, time.Time, config.holdSeconds, config.fadeSeconds);
            view.Prepare(config);
        }
    }
}
