using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using VContainer.Unity;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using TheCircussyOne.Visuals;

namespace TheCircussyOne.Runtime
{
    public interface IProjectileChainVisualSpawner
    {
        void Show(Vector3 start, Vector3 end, Color color);
    }

    public sealed class NullProjectileChainVisualSpawner : IProjectileChainVisualSpawner
    {
        public void Show(Vector3 start, Vector3 end, Color color)
        {
        }
    }

    public sealed class ProjectileChainVisualSystem : ITickable, IProjectileChainVisualSpawner
    {
        private const float DurationSeconds = 0.14f;
        private const float Width = 0.085f;
        private const int SegmentCount = 7;

        private readonly Transform _root;
        private readonly IGameTime _time;
        private readonly List<ChainVisualRuntime> _active = new();
        private readonly Stack<LineRenderer> _pool = new();
        private readonly Material _material;

        public ProjectileChainVisualSystem(Transform root, IGameTime time)
        {
            _root = root;
            _time = time;
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                ?? Shader.Find("Sprites/Default")
                ?? Shader.Find("Legacy Shaders/Particles/Alpha Blended");
            if (shader != null)
            {
                _material = new Material(shader)
                {
                    hideFlags = HideFlags.DontSave
                };
            }
        }

        public int PooledCount => _pool.Count;

        public int Prewarm(int targetPoolCount, int maxCreate = int.MaxValue)
        {
            int target = Mathf.Max(0, targetPoolCount);
            int limit = Mathf.Max(0, maxCreate);
            int created = 0;
            while (_pool.Count < target && created < limit)
            {
                LineRenderer line = CreateLine();
                Release(line);
                created++;
            }

            return created;
        }

        public void Show(Vector3 start, Vector3 end, Color color)
        {
            if ((end - start).sqrMagnitude < 0.0001f)
            {
                return;
            }

            LineRenderer line = GetLine();
            line.gameObject.SetActive(true);
            SetJaggedPositions(line, start, end);
            ApplyColor(line, color, 1f);
            _active.Add(new ChainVisualRuntime(line, color, _time.Time));
        }

        public void Tick()
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                ChainVisualRuntime runtime = _active[i];
                float elapsed = _time.Time - runtime.StartedAt;
                float normalized = Mathf.Clamp01(elapsed / DurationSeconds);
                if (normalized >= 1f)
                {
                    Release(runtime.Line);
                    _active.RemoveAt(i);
                    continue;
                }

                ApplyColor(runtime.Line, runtime.Color, 1f - normalized);
            }
        }

        private LineRenderer GetLine()
        {
            if (_pool.Count > 0)
            {
                return _pool.Pop();
            }

            var go = new GameObject("Projectile Chain Segment");
            go.hideFlags = HideFlags.DontSave;
            if (_root != null)
            {
                go.transform.SetParent(_root, false);
            }

            return ConfigureLine(go);
        }

        private LineRenderer CreateLine()
        {
            var go = new GameObject("Projectile Chain Segment");
            go.hideFlags = HideFlags.DontSave;
            if (_root != null)
            {
                go.transform.SetParent(_root, false);
            }

            return ConfigureLine(go);
        }

        private LineRenderer ConfigureLine(GameObject go)
        {
            LineRenderer line = go.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.widthMultiplier = Width;
            line.widthCurve = new AnimationCurve(
                new Keyframe(0f, 0.35f),
                new Keyframe(0.16f, 1f),
                new Keyframe(0.84f, 1f),
                new Keyframe(1f, 0.2f));
            line.numCapVertices = 3;
            line.numCornerVertices = 2;
            line.alignment = LineAlignment.View;
            line.textureMode = LineTextureMode.Stretch;
            line.shadowCastingMode = ShadowCastingMode.Off;
            line.receiveShadows = false;
            if (_material != null)
            {
                line.sharedMaterial = _material;
            }

            return line;
        }

        private static void SetJaggedPositions(LineRenderer line, Vector3 start, Vector3 end)
        {
            Vector3 delta = end - start;
            float distance = delta.magnitude;
            if (line == null || distance < 0.001f)
            {
                return;
            }

            Vector3 forward = delta / distance;
            Vector3 referenceUp = Mathf.Abs(Vector3.Dot(forward, Vector3.up)) > 0.92f ? Vector3.right : Vector3.up;
            Vector3 side = Vector3.Cross(forward, referenceUp).normalized;
            Vector3 lift = Vector3.Cross(side, forward).normalized;
            float amplitude = Mathf.Clamp(distance * 0.045f, 0.08f, 0.32f);
            float phase = Mathf.Sin(Vector3.Dot(start + end, new Vector3(12.9898f, 78.233f, 37.719f))) * 43758.5453f;

            line.positionCount = SegmentCount;
            for (int i = 0; i < SegmentCount; i++)
            {
                float t = i / (SegmentCount - 1f);
                Vector3 position = Vector3.Lerp(start, end, t);
                if (i > 0 && i < SegmentCount - 1)
                {
                    float sign = (i & 1) == 0 ? 1f : -1f;
                    float wobble = Mathf.Sin(phase + i * 1.731f);
                    position += side * (sign * amplitude);
                    position += lift * (wobble * amplitude * 0.45f);
                }

                line.SetPosition(i, position);
            }
        }

        private void Release(LineRenderer line)
        {
            if (line == null)
            {
                return;
            }

            line.gameObject.SetActive(false);
            _pool.Push(line);
        }

        private static void ApplyColor(LineRenderer line, Color color, float alphaMultiplier)
        {
            if (line == null)
            {
                return;
            }

            Color faded = color;
            faded.a *= Mathf.Clamp01(alphaMultiplier);
            line.startColor = faded;
            line.endColor = faded;
        }

        private readonly struct ChainVisualRuntime
        {
            public ChainVisualRuntime(LineRenderer line, Color color, float startedAt)
            {
                Line = line;
                Color = color;
                StartedAt = startedAt;
            }

            public LineRenderer Line { get; }
            public Color Color { get; }
            public float StartedAt { get; }
        }
    }

    public sealed class ProjectileSystem : ITickable
    {
        private const int HitBufferSize = 16;
        private const int SplashHitBufferSize = 48;

        private readonly GameConfig _config;
        private readonly DamageFeedbackVisualConfig _damageFeedbackConfig;
        private readonly ActorMotionVisualConfig _actorMotionConfig;
        private readonly ActorRegistry _registry;
        private readonly ProjectileFactory _projectileFactory;
        private readonly IGameTime _time;
        private readonly IVfxSpawner _vfx;
        private readonly IProjectileExplosionSpawner _projectileExplosions;
        private readonly IProjectileChainVisualSpawner _chainVisuals;
        private readonly EnemyDamageService _enemyDamage;
        private readonly List<Collider> _hitColliders = new(HitBufferSize);
        private readonly List<Collider> _splashHitColliders = new(SplashHitBufferSize);
        private readonly List<EnemyRuntime> _splashEnemies = new(SplashHitBufferSize);
        private readonly WorldPhysicsQuery _worldQuery;
        private readonly RunPhaseState _phaseState;
        private readonly IGameAudio _audio;

        public ProjectileSystem(
            GameConfig config,
            DamageFeedbackVisualConfig damageFeedbackConfig,
            ActorMotionVisualConfig actorMotionConfig,
            ActorRegistry registry,
            ProjectileFactory projectileFactory,
            EnemyFactory enemyFactory,
            XpDropService xpDrops,
            GameState state,
            FeedbackService feedback,
            IDamageNumberSpawner damageNumbers,
            IGameTime time,
            IVfxSpawner vfx = null,
            IProjectileExplosionSpawner projectileExplosions = null,
            IProjectileChainVisualSpawner chainVisuals = null,
            EnemyDamageService enemyDamage = null,
            WorldPhysicsQuery worldQuery = null,
            RunPhaseState phaseState = null,
            IGameAudio audio = null)
        {
            _config = config;
            _damageFeedbackConfig = damageFeedbackConfig;
            _actorMotionConfig = actorMotionConfig;
            _registry = registry;
            _projectileFactory = projectileFactory;
            _time = time;
            _vfx = vfx;
            _projectileExplosions = projectileExplosions;
            _chainVisuals = chainVisuals;
            _audio = audio ?? NullGameAudio.Instance;
            _enemyDamage = enemyDamage ?? new EnemyDamageService(
                config,
                damageFeedbackConfig,
                actorMotionConfig,
                enemyFactory,
                xpDrops,
                state,
                feedback,
                damageNumbers,
                vfx,
                audio);
            _worldQuery = worldQuery ?? new WorldPhysicsQuery(HitBufferSize, SplashHitBufferSize);
            _phaseState = phaseState;
        }

        public void Tick()
        {
            if (_phaseState != null && !RunPhaseRules.ShouldTickCombat(_phaseState.CurrentPhase))
            {
                return;
            }

            bool movedProjectiles = false;

            for (int i = _registry.Projectiles.Count - 1; i >= 0; i--)
            {
                ProjectileRuntime projectile = _registry.Projectiles[i];
                if (projectile == null || projectile.View == null || !projectile.View.IsActive)
                {
                    _registry.Unregister(projectile);
                    continue;
                }

                Vector3 nextPosition = projectile.NextPosition(_time.DeltaTime);
                if (_worldQuery.TryHitSpherePath(projectile.Position, nextPosition, projectile.BlockRadius, projectile.BlockMask.value, out RaycastHit blockHit))
                {
                    if (TryBounceOffWorld(projectile, blockHit))
                    {
                        movedProjectiles = true;
                        continue;
                    }

                    projectile.MoveTo(blockHit.point);
                    if (projectile.ExplosiveEnabled && projectile.ExplodeOnWorldImpact)
                    {
                        Explode(projectile, primaryEnemy: null, blockHit.point);
                    }

                    _projectileFactory.Despawn(projectile);
                    movedProjectiles = true;
                    continue;
                }

                projectile.MoveTo(nextPosition);
                movedProjectiles = true;

                if (projectile.IsExpired(_time.Time))
                {
                    _projectileFactory.Despawn(projectile);
                }
            }

            if (!movedProjectiles)
            {
                return;
            }

            Physics.SyncTransforms();

            for (int i = _registry.Projectiles.Count - 1; i >= 0; i--)
            {
                ProjectileRuntime projectile = _registry.Projectiles[i];
                if (projectile == null || projectile.View == null || !projectile.View.IsActive)
                {
                    _registry.Unregister(projectile);
                    continue;
                }

                EnemyRuntime enemy = FindHitEnemy(projectile);
                if (enemy == null)
                {
                    if (projectile.IsTrajectoryComplete)
                    {
                        if (projectile.ExplosiveEnabled)
                        {
                            Explode(projectile, primaryEnemy: null, projectile.Position);
                        }

                        _projectileFactory.Despawn(projectile);
                    }

                    continue;
                }

                ApplyDamageToEnemy(projectile, enemy, projectile.Damage, projectile.Position);
                if (projectile.ExplosiveEnabled)
                {
                    Explode(projectile, enemy, projectile.Position);
                    _projectileFactory.Despawn(projectile);
                    continue;
                }

                if (TryBounceToEnemy(projectile, enemy))
                {
                    continue;
                }

                TryChainToEnemies(projectile, enemy);
                _projectileFactory.Despawn(projectile);
            }
        }

        private EnemyRuntime FindHitEnemy(ProjectileRuntime projectile)
        {
            int count = _worldQuery.CollectSphereOverlaps(
                projectile.Position,
                projectile.HitRadius,
                projectile.HitMask,
                _hitColliders,
                QueryTriggerInteraction.Collide);

            for (int i = 0; i < count; i++)
            {
                Collider hit = _hitColliders[i];
                if (hit == null)
                {
                    continue;
                }

                EnemyHitbox hitbox = hit.GetComponent<EnemyHitbox>();
                if (hitbox == null || hitbox.Role != EnemyHitboxRole.Hurtbox)
                {
                    continue;
                }

                EnemyView view = hitbox.GetComponentInParent<EnemyView>();
                if (!_registry.TryGetEnemy(view, out EnemyRuntime enemy) || enemy.IsDead || enemy.View == null || !enemy.View.IsActive)
                {
                    continue;
                }

                if (projectile.HasHitEnemy(enemy))
                {
                    continue;
                }

                projectile.RecordHitEnemy(enemy);
                return enemy;
            }

            return null;
        }

        private bool ApplyDamageToEnemy(ProjectileRuntime projectile, EnemyRuntime enemy, int damage, Vector3 hitPosition)
        {
            if (projectile == null || enemy == null || enemy.IsDead || enemy.View == null || !enemy.View.IsActive || damage <= 0)
            {
                return false;
            }

            bool applied = _enemyDamage.Apply(enemy, damage, hitPosition, projectile.Direction);
            if (WeaponAudioCueRules.TryGetHitCue(projectile.SourceWeapon, out GameAudioCue hitCue))
            {
                _audio.PlayAt(hitCue, hitPosition);
            }

            if (IsWeapon(projectile.SourceWeapon, FirstPartyWeaponDefaults.KnifeFanId))
            {
                _vfx?.Show(VfxEffectId.KnifeHitSparks, hitPosition, projectile.Direction);
            }

            return applied;
        }

        private void Explode(ProjectileRuntime projectile, EnemyRuntime primaryEnemy, Vector3 position)
        {
            if (projectile == null || !projectile.ExplosiveEnabled)
            {
                return;
            }

            if (projectile.ExplosionVfxEnabled)
            {
                if (_projectileExplosions != null)
                {
                    _projectileExplosions.Show(position, projectile.SplashRadius, projectile.Direction);
                }
                else
                {
                    _vfx?.Show(VfxEffectId.ProjectileExplosion, position, projectile.Direction);
                }
            }

            if (projectile.SplashRadius <= 0f || projectile.Damage <= 0 || projectile.SecondarySplashDamageMultiplier <= 0f)
            {
                return;
            }

            int splashDamage = Mathf.Max(1, Mathf.RoundToInt(projectile.Damage * projectile.SecondarySplashDamageMultiplier));
            int count = _worldQuery.CollectSphereOverlaps(
                position,
                projectile.SplashRadius,
                projectile.HitMask,
                _splashHitColliders,
                QueryTriggerInteraction.Collide);

            _splashEnemies.Clear();
            for (int i = 0; i < count; i++)
            {
                Collider hit = _splashHitColliders[i];
                if (hit == null)
                {
                    continue;
                }

                EnemyHitbox hitbox = hit.GetComponent<EnemyHitbox>();
                if (hitbox == null || hitbox.Role != EnemyHitboxRole.Hurtbox)
                {
                    continue;
                }

                EnemyView view = hitbox.GetComponentInParent<EnemyView>();
                if (!_registry.TryGetEnemy(view, out EnemyRuntime enemy) || enemy.IsDead || enemy.View == null || !enemy.View.IsActive)
                {
                    continue;
                }

                if (ReferenceEquals(enemy, primaryEnemy) || _splashEnemies.Contains(enemy))
                {
                    continue;
                }

                _splashEnemies.Add(enemy);
                ApplyDamageToEnemy(projectile, enemy, splashDamage, position);
            }
        }

        private bool TryBounceOffWorld(ProjectileRuntime projectile, RaycastHit blockHit)
        {
            WeaponDefinition weapon = projectile.SourceWeapon;
            if (weapon == null || !weapon.bounceEnabled || !weapon.bounceOffWorldBlockers || !projectile.TryConsumeBounce())
            {
                return false;
            }

            Vector3 reflectedDirection = Vector3.Reflect(projectile.Direction, blockHit.normal);
            if (reflectedDirection.sqrMagnitude < 0.001f)
            {
                reflectedDirection = -projectile.Direction;
            }

            reflectedDirection.Normalize();
            Vector3 bouncePosition = blockHit.point + reflectedDirection * Mathf.Max(0.01f, projectile.BlockRadius);
            projectile.Redirect(new ProjectileSpawnFrame(
                bouncePosition,
                reflectedDirection,
                bouncePosition + reflectedDirection,
                ProjectileTrajectoryMode.Direct,
                0f));
            if (IsWeapon(weapon, FirstPartyWeaponDefaults.JugglingBallId))
            {
                _vfx?.Show(VfxEffectId.ProjectileBounceBurst, blockHit.point + blockHit.normal * 0.04f, reflectedDirection);
            }

            return true;
        }

        private bool TryBounceToEnemy(ProjectileRuntime projectile, EnemyRuntime hitEnemy)
        {
            WeaponDefinition weapon = projectile.SourceWeapon;
            if (weapon == null || !weapon.bounceEnabled || !projectile.CanBounce)
            {
                return false;
            }

            Vector3 origin = projectile.Position;
            EnemyRuntime target = EnemyTargeting.FindNearestAliveInRange(
                _registry.Enemies,
                origin,
                weapon.enemyBounceSearchRadius,
                enemy => !ReferenceEquals(enemy, hitEnemy)
                    && !projectile.HasHitEnemy(enemy)
                    && HasBounceLineOfSight(projectile, enemy));
            if (target == null || !projectile.TryConsumeBounce())
            {
                return false;
            }

            Vector3 direction = WeaponSpawnRules.AimDirection(origin, target.AimPosition, projectile.Direction);
            projectile.Redirect(new ProjectileSpawnFrame(
                origin,
                direction,
                target.AimPosition,
                ProjectileTrajectoryMode.Direct,
                0f));
            if (IsWeapon(weapon, FirstPartyWeaponDefaults.JugglingBallId))
            {
                _vfx?.Show(VfxEffectId.ProjectileBounceBurst, origin, direction);
            }

            return true;
        }

        private bool TryChainToEnemies(ProjectileRuntime projectile, EnemyRuntime primaryEnemy)
        {
            WeaponDefinition weapon = projectile.SourceWeapon;
            if (weapon == null || !weapon.chainEnabled || !projectile.CanChain)
            {
                return false;
            }

            bool chained = false;
            Vector3 origin = primaryEnemy != null ? primaryEnemy.AimPosition : projectile.Position;
            while (projectile.CanChain)
            {
                EnemyRuntime target = EnemyTargeting.FindNearestAliveInRange(
                    _registry.Enemies,
                    origin,
                    projectile.ChainSearchRadius,
                    enemy => !ReferenceEquals(enemy, primaryEnemy)
                        && !projectile.HasHitEnemy(enemy)
                        && HasChainLineOfSight(projectile, origin, enemy));
                if (target == null || !projectile.TryConsumeChain())
                {
                    break;
                }

                Vector3 targetPosition = target.AimPosition;
                int chainDamage = Mathf.Max(1, Mathf.RoundToInt(projectile.Damage * projectile.ChainDamageMultiplier));
                projectile.RecordHitEnemy(target);
                _chainVisuals?.Show(origin, targetPosition, weapon.projectileEmissionColor);
                ApplyDamageToEnemy(projectile, target, chainDamage, targetPosition);
                chained = true;
                origin = targetPosition;
            }

            return chained;
        }

        private bool HasChainLineOfSight(ProjectileRuntime projectile, Vector3 origin, EnemyRuntime target)
        {
            WeaponDefinition weapon = projectile.SourceWeapon;
            if (weapon == null || !projectile.ChainRequiresLineOfSight)
            {
                return true;
            }

            int mask = weapon.lineOfSightMask.value;
            if (mask == 0)
            {
                return true;
            }

            return _worldQuery.HasClearSpherePath(
                origin,
                target.AimPosition,
                Mathf.Max(0.001f, projectile.BlockRadius),
                mask,
                collider => BelongsToEnemy(collider, target),
                QueryTriggerInteraction.Ignore);
        }

        private bool HasBounceLineOfSight(ProjectileRuntime projectile, EnemyRuntime target)
        {
            WeaponDefinition weapon = projectile.SourceWeapon;
            if (weapon == null || !weapon.requireLineOfSight)
            {
                return true;
            }

            int mask = weapon.lineOfSightMask.value;
            if (mask == 0)
            {
                return true;
            }

            return _worldQuery.HasClearSpherePath(
                projectile.Position,
                target.AimPosition,
                Mathf.Max(0.001f, projectile.BlockRadius),
                mask,
                collider => BelongsToEnemy(collider, target),
                QueryTriggerInteraction.Ignore);
        }

        private static bool BelongsToEnemy(Collider collider, EnemyRuntime enemy)
        {
            EnemyView view = collider.GetComponentInParent<EnemyView>();
            return view != null && enemy != null && ReferenceEquals(view, enemy.View);
        }

        private static bool IsWeapon(WeaponDefinition weapon, string weaponId)
        {
            return WeaponAudioCueRules.IsWeapon(weapon, weaponId);
        }
    }
}
