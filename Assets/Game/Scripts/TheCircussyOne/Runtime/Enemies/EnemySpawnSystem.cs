using UnityEngine;
using VContainer.Unity;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;
using TheCircussyOne.Visuals;

namespace TheCircussyOne.Runtime
{
    public sealed class EnemySpawnSystem : ITickable, IRunResettable
    {
        private readonly GameConfig _config;
        private readonly PlayerView _player;
        private readonly ActorRegistry _registry;
        private readonly EnemySpawnIndicatorSystem _spawnIndicators;
        private readonly GameState _state;
        private readonly IGameTime _time;
        private readonly RunPhaseState _phaseState;
        private readonly RunActScheduleState _scheduleState;
        private float _remaining;
        private float _elapsedSeconds;

        public EnemySpawnSystem(
            GameConfig config,
            PlayerView player,
            ActorRegistry registry,
            EnemySpawnIndicatorSystem spawnIndicators,
            GameState state,
            IGameTime time,
            RunPhaseState phaseState = null,
            RunActScheduleState scheduleState = null)
        {
            _config = config;
            _player = player;
            _registry = registry;
            _spawnIndicators = spawnIndicators;
            _state = state;
            _time = time;
            _phaseState = phaseState;
            _scheduleState = scheduleState;
            _remaining = config.spawnIntervalSeconds;
        }

        public void ResetRunState()
        {
            ResetRunState(new RunResetContext(RunResetKind.StartNewRun));
        }

        public void ResetRunState(RunResetContext context)
        {
            if (!context.IsFullRunReset)
            {
                return;
            }

            _remaining = _config != null ? _config.spawnIntervalSeconds : 0f;
            _elapsedSeconds = 0f;
        }

        public void Tick()
        {
            _registry.RemoveInactive();
            if (_state.IsGameOver || (_phaseState != null && !RunPhaseRules.ShouldSpawnEnemies(_phaseState.CurrentPhase)))
            {
                return;
            }

            _elapsedSeconds += _time.DeltaTime;
            int maxEnemies = DifficultyRules.MaxEnemies(
                _config.maxEnemies,
                _config.maxEnemiesAtPeak,
                _elapsedSeconds,
                _config.spawnRampDurationSeconds);
            maxEnemies = ApplyScheduleMaxEnemyPressure(maxEnemies);

            int pendingEnemies = _spawnIndicators != null ? _spawnIndicators.PendingCount : 0;
            if (!EnemySpawnRules.CanSpawn(false, _registry.Enemies.Count + pendingEnemies, maxEnemies))
            {
                return;
            }

            _remaining = SpawnTimerRules.Tick(_remaining, _time.DeltaTime);
            if (!SpawnTimerRules.CanSpawn(_remaining))
            {
                return;
            }

            _remaining = ApplyScheduleSpawnIntervalPressure(DifficultyRules.SpawnInterval(
                _config.spawnIntervalSeconds,
                _config.minSpawnIntervalSeconds,
                _elapsedSeconds,
                _config.spawnRampDurationSeconds,
                _config.difficultyRampEase));
            float angle = Random.Range(0f, Mathf.PI * 2f);
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * _config.enemySpawnRadius;
            bool useEncorePool = _scheduleState != null && _scheduleState.IsEncorePressureActive;
            _spawnIndicators.RequestSpawn(_player.Position + offset, _state.Level, _elapsedSeconds, useEncorePool);
        }

        private int ApplyScheduleMaxEnemyPressure(int maxEnemies)
        {
            if (_scheduleState == null)
            {
                return maxEnemies;
            }

            float multiplier = _scheduleState.IsEncorePressureActive
                ? _scheduleState.EncoreMaxEnemiesMultiplier
                : _scheduleState.ShowtimeMaxEnemiesMultiplier;
            return Mathf.Max(maxEnemies, Mathf.CeilToInt(maxEnemies * multiplier));
        }

        private float ApplyScheduleSpawnIntervalPressure(float intervalSeconds)
        {
            if (_scheduleState == null)
            {
                return intervalSeconds;
            }

            float multiplier = _scheduleState.IsEncorePressureActive
                ? _scheduleState.EncoreSpawnIntervalMultiplier
                : _scheduleState.ShowtimeSpawnIntervalMultiplier;
            return Mathf.Max(0.02f, intervalSeconds * multiplier);
        }
    }
}
