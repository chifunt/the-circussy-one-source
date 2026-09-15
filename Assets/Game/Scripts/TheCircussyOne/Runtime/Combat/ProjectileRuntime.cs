using UnityEngine;
using System.Collections.Generic;
using TheCircussyOne.Content;
using TheCircussyOne.Visuals;

namespace TheCircussyOne.Runtime
{
    public readonly struct ProjectileSpawnFrame
    {
        public ProjectileSpawnFrame(
            Vector3 position,
            Vector3 direction,
            Vector3 targetPosition,
            ProjectileTrajectoryMode trajectoryMode,
            float arcHeight)
        {
            Position = position;
            Direction = direction.sqrMagnitude >= 0.001f ? direction.normalized : Vector3.forward;
            TargetPosition = targetPosition;
            TrajectoryMode = trajectoryMode;
            ArcHeight = Mathf.Max(0f, arcHeight);
        }

        public Vector3 Position { get; }
        public Vector3 Direction { get; }
        public Vector3 TargetPosition { get; }
        public ProjectileTrajectoryMode TrajectoryMode { get; }
        public float ArcHeight { get; }

        public static ProjectileSpawnFrame Direct(Vector3 position, Vector3 direction)
        {
            Vector3 safeDirection = direction.sqrMagnitude >= 0.001f ? direction.normalized : Vector3.forward;
            return new ProjectileSpawnFrame(
                position,
                safeDirection,
                position + safeDirection,
                ProjectileTrajectoryMode.Direct,
                0f);
        }
    }

    public sealed class ProjectileRuntime
    {
        public ProjectileRuntime(
            ProjectileView view,
            WeaponDefinition sourceWeapon,
            Vector3 direction,
            int damage,
            float speed,
            float hitRadius,
            LayerMask hitMask,
            LayerMask blockMask,
            float blockRadius,
            float expiresAt,
            int remainingBounces = 0,
            bool explosiveEnabled = false,
            float splashRadius = 0f,
            float secondarySplashDamageMultiplier = 0.5f,
            bool explodeOnWorldImpact = false,
            bool explosionVfxEnabled = false,
            int remainingChains = 0,
            float chainSearchRadius = 0f,
            float chainDamageMultiplier = 0.65f,
            bool chainRequiresLineOfSight = true)
            : this(
                view,
                sourceWeapon,
                ProjectileSpawnFrame.Direct(view != null ? view.Position : Vector3.zero, direction),
                damage,
                speed,
                hitRadius,
                hitMask,
                blockMask,
                blockRadius,
                expiresAt,
                remainingBounces,
                explosiveEnabled,
                splashRadius,
                secondarySplashDamageMultiplier,
                explodeOnWorldImpact,
                explosionVfxEnabled,
                remainingChains,
                chainSearchRadius,
                chainDamageMultiplier,
                chainRequiresLineOfSight)
        {
        }

        public ProjectileRuntime(
            ProjectileView view,
            WeaponDefinition sourceWeapon,
            ProjectileSpawnFrame spawnFrame,
            int damage,
            float speed,
            float hitRadius,
            LayerMask hitMask,
            LayerMask blockMask,
            float blockRadius,
            float expiresAt,
            int remainingBounces = 0,
            bool explosiveEnabled = false,
            float splashRadius = 0f,
            float secondarySplashDamageMultiplier = 0.5f,
            bool explodeOnWorldImpact = false,
            bool explosionVfxEnabled = false,
            int remainingChains = 0,
            float chainSearchRadius = 0f,
            float chainDamageMultiplier = 0.65f,
            bool chainRequiresLineOfSight = true)
        {
            View = view;
            SourceWeapon = sourceWeapon;
            Direction = spawnFrame.Direction;
            Damage = damage;
            Speed = speed;
            HitRadius = hitRadius;
            HitMask = hitMask;
            BlockMask = blockMask;
            BlockRadius = Mathf.Max(0.001f, blockRadius);
            ExpiresAt = expiresAt;
            Origin = spawnFrame.Position;
            TargetPosition = spawnFrame.TargetPosition;
            TrajectoryMode = spawnFrame.TrajectoryMode;
            ArcHeight = Mathf.Max(0f, spawnFrame.ArcHeight);
            TrajectoryDuration = CalculateTrajectoryDuration(spawnFrame.Position, spawnFrame.TargetPosition, speed);
            PreviousPosition = view != null ? view.Position : Vector3.zero;
            RemainingBounces = Mathf.Max(0, remainingBounces);
            ExplosiveEnabled = explosiveEnabled;
            SplashRadius = Mathf.Max(0f, splashRadius);
            SecondarySplashDamageMultiplier = Mathf.Clamp01(secondarySplashDamageMultiplier);
            ExplodeOnWorldImpact = explodeOnWorldImpact;
            ExplosionVfxEnabled = explosionVfxEnabled;
            RemainingChains = Mathf.Max(0, remainingChains);
            ChainSearchRadius = Mathf.Max(0f, chainSearchRadius);
            ChainDamageMultiplier = Mathf.Clamp01(chainDamageMultiplier);
            ChainRequiresLineOfSight = chainRequiresLineOfSight;
        }

        public ProjectileView View { get; }
        public WeaponDefinition SourceWeapon { get; }
        public Vector3 Direction { get; private set; }
        public Vector3 Origin { get; private set; }
        public Vector3 TargetPosition { get; private set; }
        public ProjectileTrajectoryMode TrajectoryMode { get; private set; }
        public float ArcHeight { get; private set; }
        public float TrajectoryDuration { get; private set; }
        public int Damage { get; }
        public float Speed { get; }
        public float HitRadius { get; }
        public LayerMask HitMask { get; }
        public LayerMask BlockMask { get; }
        public float BlockRadius { get; }
        public float ExpiresAt { get; }
        public Vector3 Position => View != null ? View.Position : Vector3.zero;
        public Vector3 PreviousPosition { get; private set; }
        public int RemainingBounces { get; private set; }
        public bool CanBounce => RemainingBounces > 0;
        public bool ExplosiveEnabled { get; }
        public float SplashRadius { get; }
        public float SecondarySplashDamageMultiplier { get; }
        public bool ExplodeOnWorldImpact { get; }
        public bool ExplosionVfxEnabled { get; }
        public int RemainingChains { get; private set; }
        public bool CanChain => RemainingChains > 0;
        public float ChainSearchRadius { get; }
        public float ChainDamageMultiplier { get; }
        public bool ChainRequiresLineOfSight { get; }
        public bool IsTrajectoryComplete => TrajectoryMode == ProjectileTrajectoryMode.Arc && trajectoryElapsed >= TrajectoryDuration - 0.0001f;

        private float trajectoryElapsed;
        private float pendingTrajectoryElapsed;
        private bool hasPendingTrajectoryElapsed;
        private readonly HashSet<EnemyRuntime> hitEnemies = new();

        public bool IsExpired(float time)
        {
            return time >= ExpiresAt;
        }

        public Vector3 NextPosition(float deltaTime)
        {
            if (TrajectoryMode == ProjectileTrajectoryMode.Arc)
            {
                pendingTrajectoryElapsed = Mathf.Min(TrajectoryDuration, trajectoryElapsed + Mathf.Max(0f, deltaTime));
                hasPendingTrajectoryElapsed = true;
                return EvaluateArc(pendingTrajectoryElapsed / Mathf.Max(0.0001f, TrajectoryDuration));
            }

            return Position + Direction * Speed * deltaTime;
        }

        public void MoveTo(Vector3 position)
        {
            PreviousPosition = Position;
            View?.SetPosition(position);
            if (hasPendingTrajectoryElapsed)
            {
                trajectoryElapsed = pendingTrajectoryElapsed;
                hasPendingTrajectoryElapsed = false;
            }
        }

        public bool HasHitEnemy(EnemyRuntime enemy)
        {
            return enemy != null && hitEnemies.Contains(enemy);
        }

        public void RecordHitEnemy(EnemyRuntime enemy)
        {
            if (enemy != null)
            {
                hitEnemies.Add(enemy);
            }
        }

        public bool TryConsumeBounce()
        {
            if (RemainingBounces <= 0)
            {
                return false;
            }

            RemainingBounces--;
            return true;
        }

        public bool TryConsumeChain()
        {
            if (RemainingChains <= 0)
            {
                return false;
            }

            RemainingChains--;
            return true;
        }

        public void Redirect(ProjectileSpawnFrame spawnFrame)
        {
            Direction = spawnFrame.Direction;
            Origin = spawnFrame.Position;
            TargetPosition = spawnFrame.TargetPosition;
            TrajectoryMode = spawnFrame.TrajectoryMode;
            ArcHeight = Mathf.Max(0f, spawnFrame.ArcHeight);
            TrajectoryDuration = CalculateTrajectoryDuration(spawnFrame.Position, spawnFrame.TargetPosition, Speed);
            trajectoryElapsed = 0f;
            pendingTrajectoryElapsed = 0f;
            hasPendingTrajectoryElapsed = false;
            PreviousPosition = spawnFrame.Position;
            View?.SetPosition(spawnFrame.Position);
            View?.SetDirection(spawnFrame.Direction);
        }

        private Vector3 EvaluateArc(float normalizedTime)
        {
            float t = Mathf.Clamp01(normalizedTime);
            Vector3 position = Vector3.Lerp(Origin, TargetPosition, t);
            position.y += Mathf.Sin(t * Mathf.PI) * ArcHeight;
            return position;
        }

        private static float CalculateTrajectoryDuration(Vector3 origin, Vector3 target, float speed)
        {
            float distance = Vector3.Distance(origin, target);
            return Mathf.Max(0.0001f, distance / Mathf.Max(0.0001f, speed));
        }
    }
}
