using System;
using VContainer.Unity;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;
using TheCircussyOne.Visuals;

namespace TheCircussyOne.Runtime
{
    internal enum PlayerVignetteFeedbackKind
    {
        Damage,
        Healing
    }

    public sealed class PlayerDamageVignettePresenter : IStartable, ITickable, ITickableWhenPaused, ILateTickable, ILateTickableWhenPaused, IDisposable
    {
        private readonly GameState state;
        private readonly DamageFeedbackVisualConfig config;
        private readonly DamageVignetteView view;
        private readonly IGameTime time;

        private float startIntensity;
        private float elapsedSeconds;
        private bool active;
        private PlayerVignetteFeedbackKind activeKind;

        public PlayerDamageVignettePresenter(
            GameState state,
            DamageFeedbackVisualConfig config,
            DamageVignetteView view,
            IGameTime time)
        {
            this.state = state;
            this.config = config;
            this.view = view;
            this.time = time;
        }

        public void Start()
        {
            if (state != null)
            {
                state.PlayerDamaged += OnPlayerDamaged;
                state.PlayerHealed += OnPlayerHealed;
            }

            view?.ApplyFrame(PlayerDamageVignetteFrame.Hidden);
        }

        public void Tick()
        {
            if (!active)
            {
                return;
            }

            elapsedSeconds += time != null ? time.DeltaTime : 0f;
            PlayerDamageVignetteFrame frame = EvaluateActiveFrame();
            view?.ApplyFrame(frame);
            if (!frame.Visible)
            {
                active = false;
                startIntensity = 0f;
            }
        }

        public void LateTick()
        {
            view?.AlignToCamera();
        }

        public void Dispose()
        {
            if (state != null)
            {
                state.PlayerDamaged -= OnPlayerDamaged;
                state.PlayerHealed -= OnPlayerHealed;
            }

            active = false;
            startIntensity = 0f;
            view?.ApplyFrame(PlayerDamageVignetteFrame.Hidden);
        }

        private void OnPlayerDamaged(int finalDamage, int maxHealth, float healthPercentDamage)
        {
            if (config == null || !config.playerDamageVignetteEnabled)
            {
                return;
            }

            float current = CurrentIntensity(PlayerVignetteFeedbackKind.Damage);
            float added = PlayerDamageVignetteRules.DamageIntensity(finalDamage, maxHealth, config.playerDamageVignetteDamageToIntensityScale);
            startIntensity = PlayerDamageVignetteRules.Accumulate(current, added, config.playerDamageVignetteAccumulationCap);
            elapsedSeconds = 0f;
            active = startIntensity > 0f;
            activeKind = PlayerVignetteFeedbackKind.Damage;
            if (active)
            {
                view?.ApplyFrame(EvaluateActiveFrame());
            }
        }

        private void OnPlayerHealed(int healedAmount, int maxHealth, float healthPercentHealed)
        {
            if (config == null || !config.playerHealingVignetteEnabled)
            {
                return;
            }

            float current = CurrentIntensity(PlayerVignetteFeedbackKind.Healing);
            float added = PlayerDamageVignetteRules.HealingIntensity(healedAmount, maxHealth, config.playerHealingVignetteHealToIntensityScale);
            startIntensity = PlayerDamageVignetteRules.Accumulate(current, added, config.playerHealingVignetteAccumulationCap);
            elapsedSeconds = 0f;
            active = startIntensity > 0f;
            activeKind = PlayerVignetteFeedbackKind.Healing;
            if (active)
            {
                view?.ApplyFrame(EvaluateActiveFrame());
            }
        }

        private float CurrentIntensity(PlayerVignetteFeedbackKind kind)
        {
            if (!active || activeKind != kind)
            {
                return 0f;
            }

            return kind == PlayerVignetteFeedbackKind.Healing
                ? PlayerDamageVignetteRules.Fade(startIntensity, elapsedSeconds, config.playerHealingVignetteFlashSeconds, config.playerHealingVignetteFadeEase)
                : PlayerDamageVignetteRules.Fade(startIntensity, elapsedSeconds, config.playerDamageVignetteFlashSeconds, config.playerDamageVignetteFadeEase);
        }

        private PlayerDamageVignetteFrame EvaluateActiveFrame()
        {
            float now = time != null ? time.Time : 0f;
            return activeKind == PlayerVignetteFeedbackKind.Healing
                ? PlayerDamageVignetteRules.EvaluateHealing(startIntensity, elapsedSeconds, config, now)
                : PlayerDamageVignetteRules.Evaluate(startIntensity, elapsedSeconds, config, now);
        }
    }
}
