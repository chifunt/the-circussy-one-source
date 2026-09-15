using UnityEngine;
using TheCircussyOne.Content;
using TheCircussyOne.Visuals;

namespace TheCircussyOne.Runtime
{
    public sealed class EnemySpawnIndicatorRuntime
    {
        public EnemySpawnIndicatorRuntime(EnemySpawnIndicatorView view, Vector3 position, Vector3 surfaceNormal, int level, float enemyElapsedSeconds, float startedAt, float warningSeconds, float emergeSeconds, EnemyDefinition enemyDefinition = null)
        {
            View = view;
            Position = position;
            SurfaceNormal = surfaceNormal.sqrMagnitude > 0.000001f ? surfaceNormal.normalized : Vector3.up;
            Level = level;
            EnemyElapsedSeconds = enemyElapsedSeconds;
            StartedAt = startedAt;
            WarningSeconds = warningSeconds;
            EmergeSeconds = emergeSeconds;
            EnemyDefinition = enemyDefinition;
        }

        public EnemySpawnIndicatorView View { get; }
        public EnemyDefinition EnemyDefinition { get; }
        public Vector3 Position { get; }
        public Vector3 SurfaceNormal { get; }
        public int Level { get; }
        public float EnemyElapsedSeconds { get; }
        public float StartedAt { get; }
        public float WarningSeconds { get; }
        public float EmergeSeconds { get; }
        public EnemyRuntime SpawnedEnemy { get; private set; }
        public float EnemySpawnedAt { get; private set; }

        public bool HasSpawnedEnemy => SpawnedEnemy != null;

        public void MarkEnemySpawned(EnemyRuntime enemy, float spawnedAt)
        {
            SpawnedEnemy = enemy;
            EnemySpawnedAt = spawnedAt;
        }
    }
}
