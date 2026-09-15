using UnityEngine;
using VContainer.Unity;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;
using TheCircussyOne.Stats;
using TheCircussyOne.Visuals;

namespace TheCircussyOne.Runtime
{
    public sealed class PickupSystem : ITickable
    {
        private readonly GameConfig _config;
        private readonly DamageFeedbackVisualConfig _damageFeedbackConfig;
        private readonly PlayerView _player;
        private readonly ActorRegistry _registry;
        private readonly PickupFactory _pickupFactory;
        private readonly GameState _state;
        private readonly IVfxSpawner _vfx;
        private readonly IXpGainCounter _xpGainCounter;
        private readonly RunStats _stats;
        private readonly PickupSurfaceSampler _surfaceSampler;
        private readonly RunPhaseState _phaseState;
        private readonly IGameAudio _audio;
        private readonly IGameHaptics _haptics;
        private float _xpGainRemainder;

        public PickupSystem(
            GameConfig config,
            DamageFeedbackVisualConfig damageFeedbackConfig,
            PlayerView player,
            ActorRegistry registry,
            PickupFactory pickupFactory,
            GameState state,
            IVfxSpawner vfx = null,
            IXpGainCounter xpGainCounter = null,
            RunStats stats = null,
            PickupSurfaceSampler surfaceSampler = null,
            RunPhaseState phaseState = null,
            IGameAudio audio = null,
            IGameHaptics haptics = null)
        {
            _config = config;
            _damageFeedbackConfig = damageFeedbackConfig;
            _player = player;
            _registry = registry;
            _pickupFactory = pickupFactory;
            _state = state;
            _vfx = vfx;
            _xpGainCounter = xpGainCounter;
            _stats = stats ?? new RunStats(config);
            _surfaceSampler = surfaceSampler;
            _phaseState = phaseState;
            _audio = audio ?? NullGameAudio.Instance;
            _haptics = haptics ?? NullGameHaptics.Instance;
        }

        public void Tick()
        {
            if (_phaseState != null && !RunPhaseRules.ShouldTickCombat(_phaseState.CurrentPhase))
            {
                return;
            }

            Vector3 playerPosition = _player.Position;
            int unreservedHealthNeed = MissingHealth();
            for (int i = _registry.Pickups.Count - 1; i >= 0; i--)
            {
                PickupRuntime pickup = _registry.Pickups[i];
                if (pickup == null || pickup.View == null || !pickup.View.IsActive)
                {
                    _registry.Unregister(pickup);
                    continue;
                }

                if (!pickup.View.IsCollectable)
                {
                    continue;
                }

                if (!CanUsePickup(pickup, ref unreservedHealthNeed))
                {
                    if (pickup.Reward.Kind == PickupRewardKind.Health)
                    {
                        pickup.View.CancelAttractToRest();
                    }

                    continue;
                }

                Vector3 toPlayer = PickupRules.OffsetToPlayer(pickup.Position, playerPosition);
                float distance = toPlayer.magnitude;
                bool reachable = pickup.View.HasReachedPlayer || IsReachable(pickup.Position, playerPosition + Vector3.up * 0.8f);

                float collectRadius = _stats.GetFloat(StatId.PickupCollectRadius);
                float magnetRadius = _stats.GetFloat(StatId.PickupMagnetRadius);

                if (pickup.View.HasReachedPlayer || (reachable && PickupRules.ShouldCollect(distance, collectRadius)))
                {
                    ApplyPickupReward(pickup);
                    unreservedHealthNeed = MissingHealth();

                    _vfx?.Show(VfxEffectId.XpPickupCollectPop, playerPosition + Vector3.up * 0.8f);
                    _pickupFactory.Despawn(pickup);
                    continue;
                }

                if (reachable && !pickup.View.IsAttracting && PickupRules.ShouldMagnet(distance, magnetRadius))
                {
                    pickup.View.BeginAttractTo(
                        _player.transform,
                        _damageFeedbackConfig != null ? _damageFeedbackConfig.pickupAttractSeconds : 0.34f,
                        _damageFeedbackConfig != null ? _damageFeedbackConfig.pickupAttractEase : EaseSettings.InBack);
                    ReserveHealthPickup(pickup, ref unreservedHealthNeed);
                }
            }
        }

        private bool CanUsePickup(PickupRuntime pickup, ref int unreservedHealthNeed)
        {
            if (pickup == null || pickup.Reward.Kind != PickupRewardKind.Health)
            {
                return true;
            }

            if (unreservedHealthNeed <= 0)
            {
                return false;
            }

            if (pickup.View != null && pickup.View.IsAttracting)
            {
                ReserveHealthPickup(pickup, ref unreservedHealthNeed);
            }

            return true;
        }

        private int MissingHealth()
        {
            return Mathf.Max(0, _state.MaxHealth - _state.Health);
        }

        private void ReserveHealthPickup(PickupRuntime pickup, ref int unreservedHealthNeed)
        {
            if (pickup == null || pickup.Reward.Kind != PickupRewardKind.Health)
            {
                return;
            }

            unreservedHealthNeed = Mathf.Max(0, unreservedHealthNeed - ResolveHealthPickupAmount(pickup));
        }

        private void ApplyPickupReward(PickupRuntime pickup)
        {
            if (pickup == null)
            {
                return;
            }

            switch (pickup.Reward.Kind)
            {
                case PickupRewardKind.Health:
                    _state.HealPlayer(ResolveHealthPickupAmount(pickup));
                    break;
                case PickupRewardKind.Experience:
                default:
                    int gainedExperience = StatRules.ApplyXpGain(
                        pickup.Amount,
                        _stats.GetFloat(StatId.XpGainMultiplier),
                        ref _xpGainRemainder);
                    if (gainedExperience > 0)
                    {
                        _state.AddExperience(gainedExperience);
                        _xpGainCounter?.Add(gainedExperience);
                        _audio.PlayXpCollect(_player.Position + Vector3.up * 0.8f);
                        _haptics.Play(GameHapticsCue.XpCollect);
                    }

                    break;
            }
        }

        private int ResolveHealthPickupAmount(PickupRuntime pickup)
        {
            if (pickup == null || pickup.Reward.Kind != PickupRewardKind.Health)
            {
                return 0;
            }

            if (pickup.HealthPickup != null)
            {
                return pickup.HealthPickup.ResolveHealAmount(_state.MaxHealth);
            }

            return Mathf.Max(1, pickup.Amount);
        }

        private bool IsReachable(Vector3 pickupPosition, Vector3 playerTargetPosition)
        {
            if (_surfaceSampler == null)
            {
                return true;
            }

            int mask = PickupEnvironmentMask();
            if (mask == 0)
            {
                return true;
            }

            float radius = _damageFeedbackConfig != null ? _damageFeedbackConfig.pickupCollisionRadius * 0.5f : 0.08f;
            return _surfaceSampler.HasClearPath(pickupPosition, playerTargetPosition, radius, mask);
        }

        private int PickupEnvironmentMask()
        {
            int configuredMask = _damageFeedbackConfig != null ? _damageFeedbackConfig.pickupEnvironmentMask.value : ~0;
            return configuredMask & GameLayers.EnvironmentMaskExcludingGameplay;
        }
    }
}
