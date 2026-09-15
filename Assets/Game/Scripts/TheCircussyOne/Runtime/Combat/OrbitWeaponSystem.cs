using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using TheCircussyOne.Stats;
using TheCircussyOne.Visuals;

namespace TheCircussyOne.Runtime
{
    public sealed class OrbitWeaponSystem : ITickable, ITickableWhenPaused, IDisposable
    {
        private const int HitBufferSize = 48;

        private readonly WeaponLoadout _loadout;
        private readonly PlayerView _player;
        private readonly ActorRegistry _registry;
        private readonly RunStats _stats;
        private readonly IGameTime _time;
        private readonly WeaponTargetingQuery _targetingQuery;
        private readonly EnemyDamageService _damageService;
        private readonly OrbitWeaponView _prefab;
        private readonly Transform _root;
        private readonly RunPauseState _pauseState;
        private readonly VfxVisualConfig _vfxConfig;
        private readonly FireHoopAreaTelegraphView _areaTelegraphPrefab;
        private readonly WorldPhysicsQuery _worldQuery;
        private readonly RunPhaseState _phaseState;
        private readonly IGameAudio _audio;
        private readonly List<Collider> _hitColliders = new(HitBufferSize);
        private readonly Dictionary<WeaponRuntime, OrbitWeaponRuntime> _runtimes = new();
        private readonly List<WeaponRuntime> _staleWeapons = new();
        private readonly Stack<OrbitWeaponView> _pool = new();
        private readonly Stack<FireHoopAreaTelegraphView> _areaTelegraphPool = new();

        public OrbitWeaponSystem(
            WeaponLoadout loadout,
            PlayerView player,
            ActorRegistry registry,
            RunStats stats,
            IGameTime time,
            WeaponTargetingQuery targetingQuery,
            EnemyDamageService damageService,
            OrbitWeaponView prefab = null,
            Transform root = null,
            RunPauseState pauseState = null,
            VfxVisualConfig vfxConfig = null,
            FireHoopAreaTelegraphView areaTelegraphPrefab = null,
            WorldPhysicsQuery worldQuery = null,
            RunPhaseState phaseState = null,
            IGameAudio audio = null)
        {
            _loadout = loadout;
            _player = player;
            _registry = registry;
            _stats = stats;
            _time = time;
            _targetingQuery = targetingQuery ?? new WeaponTargetingQuery();
            _damageService = damageService;
            _prefab = prefab;
            _root = root;
            _pauseState = pauseState;
            _vfxConfig = vfxConfig;
            _areaTelegraphPrefab = areaTelegraphPrefab;
            _worldQuery = worldQuery ?? new WorldPhysicsQuery(HitBufferSize, HitBufferSize);
            _phaseState = phaseState;
            _audio = audio ?? NullGameAudio.Instance;
            if (_loadout != null)
            {
                _loadout.Changed += OnLoadoutChanged;
            }
        }

        public int ActiveRuntimeCount => _runtimes.Count;
        public int PooledCount => _pool.Count;
        public int PooledAreaTelegraphCount => _areaTelegraphPool.Count;
        public int ActiveAreaTelegraphCount
        {
            get
            {
                int count = 0;
                foreach (KeyValuePair<WeaponRuntime, OrbitWeaponRuntime> pair in _runtimes)
                {
                    if (pair.Value.AreaTelegraph != null && pair.Value.AreaTelegraph.IsActive)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        public int PrewarmViews(int targetPoolCount, int maxCreate = int.MaxValue)
        {
            if (_prefab == null)
            {
                return 0;
            }

            int target = Mathf.Max(0, targetPoolCount);
            int limit = Mathf.Max(0, maxCreate);
            int created = 0;
            while (_pool.Count < target && created < limit)
            {
                OrbitWeaponView view = UnityEngine.Object.Instantiate(_prefab, _root);
                Release(view);
                created++;
            }

            return created;
        }

        public int PrewarmAreaTelegraphs(int targetPoolCount, int maxCreate = int.MaxValue)
        {
            if (_areaTelegraphPrefab == null)
            {
                return 0;
            }

            int target = Mathf.Max(0, targetPoolCount);
            int limit = Mathf.Max(0, maxCreate);
            int created = 0;
            while (_areaTelegraphPool.Count < target && created < limit)
            {
                FireHoopAreaTelegraphView view = UnityEngine.Object.Instantiate(_areaTelegraphPrefab, _root);
                ReleaseAreaTelegraphView(view);
                created++;
            }

            return created;
        }

        public void Tick()
        {
            if (_loadout == null || _player == null || _registry == null || _damageService == null)
            {
                return;
            }

            MarkStaleRuntimes();
            if (_phaseState != null && !RunPhaseRules.ShouldTickCombat(_phaseState.CurrentPhase))
            {
                ClearAllViews();
                return;
            }

            if (_pauseState != null && _pauseState.IsPaused)
            {
                return;
            }

            IReadOnlyList<WeaponRuntime> weapons = _loadout.Weapons;
            for (int i = 0; i < weapons.Count; i++)
            {
                WeaponRuntime weaponRuntime = weapons[i];
                WeaponDefinition weapon = weaponRuntime != null ? weaponRuntime.Definition : null;
                if (!ContentAvailabilityRules.IsActiveAndValid(weapon) || !weapon.orbitEnabled)
                {
                    continue;
                }

                TickWeapon(GetOrCreateRuntime(weaponRuntime), weaponRuntime, weapon);
            }
        }

        public void Dispose()
        {
            if (_loadout != null)
            {
                _loadout.Changed -= OnLoadoutChanged;
            }

            ClearAllViews();
            _pool.Clear();
            _areaTelegraphPool.Clear();
        }

        public void ClearAllViews()
        {
            foreach (KeyValuePair<WeaponRuntime, OrbitWeaponRuntime> pair in _runtimes)
            {
                EnsureViewCount(pair.Value, 0);
                ReleaseAreaTelegraph(pair.Value);
            }

            _runtimes.Clear();
            DeactivatePooledOrbitViews();
            DeactivatePooledAreaTelegraphs();
        }

        private void DeactivatePooledOrbitViews()
        {
            if (_pool.Count == 0)
            {
                return;
            }

            var validViews = new List<OrbitWeaponView>(_pool.Count);
            while (_pool.Count > 0)
            {
                OrbitWeaponView view = _pool.Pop();
                if (TryDeactivate(view))
                {
                    validViews.Add(view);
                }
            }

            for (int i = validViews.Count - 1; i >= 0; i--)
            {
                _pool.Push(validViews[i]);
            }
        }

        private void DeactivatePooledAreaTelegraphs()
        {
            if (_areaTelegraphPool.Count == 0)
            {
                return;
            }

            var validViews = new List<FireHoopAreaTelegraphView>(_areaTelegraphPool.Count);
            while (_areaTelegraphPool.Count > 0)
            {
                FireHoopAreaTelegraphView view = _areaTelegraphPool.Pop();
                if (TryDeactivate(view))
                {
                    validViews.Add(view);
                }
            }

            for (int i = validViews.Count - 1; i >= 0; i--)
            {
                _areaTelegraphPool.Push(validViews[i]);
            }
        }

        private void OnLoadoutChanged()
        {
            ClearAllViews();
        }

        private void TickWeapon(OrbitWeaponRuntime runtime, WeaponRuntime weaponRuntime, WeaponDefinition weapon)
        {
            int count = WeaponStatRules.OrbitCount(weapon, _stats, weaponRuntime.Modifiers);
            if (count <= 0)
            {
                EnsureViewCount(runtime, 0);
                ReleaseAreaTelegraph(runtime);
                return;
            }

            float now = _time != null ? _time.Time : 0f;
            float deltaTime = _time != null ? _time.DeltaTime : 0f;
            float radius = WeaponStatRules.OrbitRadius(weapon, weaponRuntime.Modifiers);
            float hitRadius = WeaponStatRules.OrbitHitRadius(weapon, _stats, weaponRuntime.Modifiers);
            int damage = WeaponStatRules.Damage(weapon, _stats, weaponRuntime.Modifiers);
            float hitInterval = WeaponStatRules.FireInterval(weapon, _stats, weaponRuntime.Modifiers);
            int areaDamage = WeaponStatRules.OrbitAreaDamage(weapon, _stats, weaponRuntime.Modifiers);
            float areaHitInterval = WeaponStatRules.OrbitAreaHitInterval(weapon, _stats, weaponRuntime.Modifiers);
            Vector3 orbitUp = _player.VisualOrbitUp;
            runtime.AngleDegrees = OrbitWeaponRules.AdvanceAngle(runtime.AngleDegrees, weapon.orbitDegreesPerSecond, deltaTime);
            runtime.PruneHitCooldowns();

            EnsureViewCount(runtime, count);
            UpdateAreaTelegraph(runtime, weapon, radius + hitRadius, areaDamage, areaHitInterval, now);
            DamageArea(runtime, weapon, radius + hitRadius, areaDamage, areaHitInterval, now, orbitUp);
            for (int i = 0; i < count; i++)
            {
                OrbitWeaponFrame frame = OrbitWeaponRules.EvaluateFrame(
                    _player.Position,
                    i,
                    count,
                    runtime.AngleDegrees,
                    radius,
                    weapon.orbitHeightOffset,
                    hitRadius,
                    orbitUp);

                OrbitWeaponView view = i < runtime.Views.Count ? runtime.Views[i] : null;
                view?.ApplyFrame(frame.Position, frame.HitRadius, weapon, frame.Rotation);
                DamageAt(runtime, weapon, frame.Position, frame.HitRadius, damage, hitInterval, now);
            }
        }

        private void DamageArea(
            OrbitWeaponRuntime runtime,
            WeaponDefinition weapon,
            float areaRadius,
            int damage,
            float hitInterval,
            float now,
            Vector3 orbitUp)
        {
            if (damage <= 0 || areaRadius <= 0f || float.IsInfinity(hitInterval))
            {
                return;
            }

            Vector3 safeUp = orbitUp.sqrMagnitude > 0.000001f ? orbitUp.normalized : Vector3.up;
            Vector3 center = _player.Position + safeUp * Mathf.Max(0f, weapon.orbitHeightOffset);
            int count = _worldQuery.CollectSphereOverlaps(
                center,
                areaRadius,
                weapon.projectileHitMask,
                _hitColliders,
                QueryTriggerInteraction.Collide);

            for (int i = 0; i < count; i++)
            {
                Collider hit = _hitColliders[i];
                if (!TryGetHurtboxEnemy(hit, out EnemyRuntime enemy) || !runtime.CanAreaHit(enemy, now))
                {
                    continue;
                }

                if (!_targetingQuery.HasLineOfSight(weapon, center, enemy))
                {
                    continue;
                }

                Vector3 hitDirection = enemy.Position - _player.Position;
                if (hitDirection.sqrMagnitude < 0.001f)
                {
                    hitDirection = enemy.Position - center;
                }

                if (hitDirection.sqrMagnitude < 0.001f)
                {
                    hitDirection = Vector3.forward;
                }

                _damageService.Apply(enemy, damage, center, hitDirection.normalized);
                PlayWeaponHit(weapon, center);
                runtime.MarkAreaHit(enemy, now + Mathf.Max(0.01f, hitInterval));
            }
        }

        private void DamageAt(
            OrbitWeaponRuntime runtime,
            WeaponDefinition weapon,
            Vector3 position,
            float hitRadius,
            int damage,
            float hitInterval,
            float now)
        {
            if (damage <= 0 || hitRadius <= 0f)
            {
                return;
            }

            int count = _worldQuery.CollectSphereOverlaps(
                position,
                hitRadius,
                weapon.projectileHitMask,
                _hitColliders,
                QueryTriggerInteraction.Collide);

            for (int i = 0; i < count; i++)
            {
                Collider hit = _hitColliders[i];
                if (!TryGetHurtboxEnemy(hit, out EnemyRuntime enemy) || !runtime.CanHit(enemy, now))
                {
                    continue;
                }

                if (!_targetingQuery.HasLineOfSight(weapon, position, enemy))
                {
                    continue;
                }

                Vector3 hitDirection = enemy.Position - _player.Position;
                if (hitDirection.sqrMagnitude < 0.001f)
                {
                    hitDirection = position - _player.Position;
                }

                if (hitDirection.sqrMagnitude < 0.001f)
                {
                    hitDirection = Vector3.forward;
                }

                _damageService.Apply(enemy, damage, position, hitDirection.normalized);
                PlayWeaponHit(weapon, position);
                runtime.MarkHit(enemy, now + Mathf.Max(0.01f, hitInterval));
            }
        }

        private void PlayWeaponHit(WeaponDefinition weapon, Vector3 position)
        {
            if (WeaponAudioCueRules.TryGetHitCue(weapon, out GameAudioCue hitCue))
            {
                _audio.PlayAt(hitCue, position);
            }
        }

        private bool TryGetHurtboxEnemy(Collider hit, out EnemyRuntime enemy)
        {
            enemy = null;
            if (hit == null)
            {
                return false;
            }

            EnemyHitbox hitbox = hit.GetComponent<EnemyHitbox>();
            if (hitbox == null || hitbox.Role != EnemyHitboxRole.Hurtbox)
            {
                return false;
            }

            EnemyView view = hitbox.GetComponentInParent<EnemyView>();
            return _registry.TryGetEnemy(view, out enemy)
                && enemy != null
                && !enemy.IsDead
                && enemy.View != null
                && enemy.View.IsActive;
        }

        private OrbitWeaponRuntime GetOrCreateRuntime(WeaponRuntime weapon)
        {
            if (!_runtimes.TryGetValue(weapon, out OrbitWeaponRuntime runtime))
            {
                runtime = new OrbitWeaponRuntime(weapon);
                _runtimes.Add(weapon, runtime);
            }

            return runtime;
        }

        private void MarkStaleRuntimes()
        {
            _staleWeapons.Clear();
            foreach (KeyValuePair<WeaponRuntime, OrbitWeaponRuntime> pair in _runtimes)
            {
                if (!IsActiveOrbitWeapon(pair.Key))
                {
                    _staleWeapons.Add(pair.Key);
                }
            }

            for (int i = 0; i < _staleWeapons.Count; i++)
            {
                if (_runtimes.TryGetValue(_staleWeapons[i], out OrbitWeaponRuntime runtime))
                {
                    EnsureViewCount(runtime, 0);
                    ReleaseAreaTelegraph(runtime);
                    _runtimes.Remove(_staleWeapons[i]);
                }
            }
        }

        private bool IsActiveOrbitWeapon(WeaponRuntime weapon)
        {
            if (weapon == null || weapon.Definition == null || !weapon.Definition.orbitEnabled)
            {
                return false;
            }

            IReadOnlyList<WeaponRuntime> weapons = _loadout.Weapons;
            for (int i = 0; i < weapons.Count; i++)
            {
                if (ReferenceEquals(weapons[i], weapon))
                {
                    return true;
                }
            }

            return false;
        }

        private void EnsureViewCount(OrbitWeaponRuntime runtime, int count)
        {
            if (runtime == null)
            {
                return;
            }

            int targetCount = Mathf.Max(0, count);
            for (int i = runtime.Views.Count - 1; i >= targetCount; i--)
            {
                Release(runtime.Views[i]);
                runtime.Views.RemoveAt(i);
            }

            while (runtime.Views.Count < targetCount)
            {
                OrbitWeaponView view = GetView();
                if (view == null)
                {
                    break;
                }

                runtime.Views.Add(view);
            }
        }

        private void UpdateAreaTelegraph(OrbitWeaponRuntime runtime, WeaponDefinition weapon, float areaRadius, int damage, float hitInterval, float now)
        {
            if (runtime == null || weapon == null || !weapon.orbitAreaDamageEnabled || damage <= 0 || areaRadius <= 0f || float.IsInfinity(hitInterval))
            {
                ReleaseAreaTelegraph(runtime);
                return;
            }

            FireHoopAreaTelegraphView view = runtime.AreaTelegraph;
            if (view == null)
            {
                view = GetAreaTelegraphView();
                runtime.AreaTelegraph = view;
            }

            if (view == null)
            {
                return;
            }

            FireHoopAreaTelegraphFrame frame = FireHoopAreaTelegraphRules.Evaluate(now, areaRadius, hitInterval, _vfxConfig);
            int mask = AreaTelegraphGroundMask();
            view.ApplyFrame(_player.Position, now, frame, mask);
        }

        private FireHoopAreaTelegraphView GetAreaTelegraphView()
        {
            if (_areaTelegraphPrefab == null)
            {
                return null;
            }

            FireHoopAreaTelegraphView view = null;
            while (_areaTelegraphPool.Count > 0 && view == null)
            {
                view = _areaTelegraphPool.Pop();
            }

            if (view == null)
            {
                view = UnityEngine.Object.Instantiate(_areaTelegraphPrefab, _root);
            }

            if (_root != null)
            {
                view.transform.SetParent(_root, false);
            }

            return view;
        }

        private void ReleaseAreaTelegraph(OrbitWeaponRuntime runtime)
        {
            FireHoopAreaTelegraphView view = runtime != null ? runtime.AreaTelegraph : null;
            if (view == null)
            {
                return;
            }

            if (!TryDeactivate(view))
            {
                runtime.AreaTelegraph = null;
                return;
            }

            ReleaseAreaTelegraphView(view);
            runtime.AreaTelegraph = null;
        }

        private void ReleaseAreaTelegraphView(FireHoopAreaTelegraphView view)
        {
            if (view == null || !TryDeactivate(view))
            {
                return;
            }

            if (_root != null)
            {
                view.transform.SetParent(_root, false);
            }

            _areaTelegraphPool.Push(view);
        }

        private int AreaTelegraphGroundMask()
        {
            int configuredMask = _vfxConfig != null ? _vfxConfig.fireHoopAreaTelegraphGroundMask.value : ~0;
            int mask = configuredMask & GameLayers.EnvironmentMaskExcludingGameplay;
            return mask != 0 ? mask : GameLayers.EnvironmentMaskExcludingGameplay;
        }

        private OrbitWeaponView GetView()
        {
            if (_prefab == null)
            {
                return null;
            }

            OrbitWeaponView view = null;
            while (_pool.Count > 0 && view == null)
            {
                view = _pool.Pop();
            }

            if (view == null)
            {
                view = UnityEngine.Object.Instantiate(_prefab, _root);
            }

            if (_root != null)
            {
                view.transform.SetParent(_root, false);
            }

            return view;
        }

        private void Release(OrbitWeaponView view)
        {
            if (view == null)
            {
                return;
            }

            if (!TryDeactivate(view))
            {
                return;
            }

            if (_root != null)
            {
                view.transform.SetParent(_root, false);
            }

            _pool.Push(view);
        }

        private static bool TryDeactivate(OrbitWeaponView view)
        {
            if (view == null)
            {
                return false;
            }

            view.Deactivate();
            return view != null;
        }

        private static bool TryDeactivate(FireHoopAreaTelegraphView view)
        {
            if (view == null)
            {
                return false;
            }

            view.Deactivate();
            return view != null;
        }
    }
}
