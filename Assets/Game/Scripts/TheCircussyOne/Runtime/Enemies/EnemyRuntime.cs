using UnityEngine;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using TheCircussyOne.Visuals;

namespace TheCircussyOne.Runtime
{
    public sealed class EnemyRuntime
    {
        public EnemyRuntime(EnemyView view, int maxHealth, float moveSpeed, int spawnId = 0)
            : this(null, view, maxHealth, moveSpeed, 540f, 2, EnemyBehaviorType.Chaser, 100f, null, null, null, null, 1, XpDropStyle.Compact, spawnId)
        {
        }

        public EnemyRuntime(
            EnemyDefinition definition,
            EnemyView view,
            int maxHealth,
            float moveSpeed,
            float turnDegreesPerSecond,
            int contactDamage,
            EnemyBehaviorType behaviorType,
            float spawnWeight,
            EnemyBodyProfile bodyProfile,
            EnemyClimbProfile climbProfile,
            EnemyStackProfile stackProfile,
            EnemyLocomotionProfile locomotionProfile,
            int xpBudget,
            XpDropStyle dropStyle,
            int spawnId = 0)
        {
            Definition = definition;
            View = view;
            MaxHealth = Mathf.Max(1, maxHealth);
            CurrentHealth = MaxHealth;
            MoveSpeed = Mathf.Max(0.1f, moveSpeed);
            TurnDegreesPerSecond = Mathf.Max(1f, turnDegreesPerSecond);
            ContactDamage = Mathf.Max(1, contactDamage);
            BehaviorType = System.Enum.IsDefined(typeof(EnemyBehaviorType), behaviorType) ? behaviorType : EnemyBehaviorType.Chaser;
            SpawnWeight = Mathf.Max(0f, spawnWeight);
            BodyProfile = CopyBodyProfile(bodyProfile);
            ClimbProfile = CopyClimbProfile(climbProfile);
            StackProfile = CopyStackProfile(stackProfile);
            LocomotionProfile = CopyLocomotionProfile(locomotionProfile);
            XpBudget = Mathf.Max(0, xpBudget);
            DropStyle = dropStyle;
            SpawnId = spawnId;
            VerticalMotor = new EnemyVerticalMotorState();
        }

        public EnemyDefinition Definition { get; }
        public EnemyView View { get; }
        public int SpawnId { get; }
        public int MaxHealth { get; }
        public int CurrentHealth { get; private set; }
        public float MoveSpeed { get; }
        public float TurnDegreesPerSecond { get; }
        public int ContactDamage { get; }
        public EnemyBehaviorType BehaviorType { get; }
        public float SpawnWeight { get; }
        public EnemyBodyProfile BodyProfile { get; }
        public EnemyClimbProfile ClimbProfile { get; }
        public EnemyStackProfile StackProfile { get; }
        public EnemyLocomotionProfile LocomotionProfile { get; }
        public int XpBudget { get; }
        public XpDropStyle DropStyle { get; }
        public EnemyVerticalMotorState VerticalMotor { get; }
        public bool IsDead { get; private set; }
        public Vector3 Position => View != null ? View.Position : Vector3.zero;
        public Vector3 AimPosition => View != null ? View.AimPosition : Position;

        public void SetHealth(int value)
        {
            CurrentHealth = Mathf.Clamp(value, 0, MaxHealth);
            IsDead = CurrentHealth <= 0;
        }

        private static EnemyBodyProfile CopyBodyProfile(EnemyBodyProfile profile)
        {
            EnemyBodyProfile copy = profile != null ? profile.Copy() : new EnemyBodyProfile();
            copy.EnsureWorkflowDefaults();
            return copy;
        }

        private static EnemyClimbProfile CopyClimbProfile(EnemyClimbProfile profile)
        {
            EnemyClimbProfile copy = profile != null ? profile.Copy() : new EnemyClimbProfile();
            copy.EnsureWorkflowDefaults();
            return copy;
        }

        private static EnemyStackProfile CopyStackProfile(EnemyStackProfile profile)
        {
            EnemyStackProfile copy = profile != null ? profile.Copy() : new EnemyStackProfile();
            copy.EnsureWorkflowDefaults();
            return copy;
        }

        private static EnemyLocomotionProfile CopyLocomotionProfile(EnemyLocomotionProfile profile)
        {
            EnemyLocomotionProfile copy = profile != null ? profile.Copy() : new EnemyLocomotionProfile();
            copy.EnsureWorkflowDefaults();
            return copy;
        }
    }
}
