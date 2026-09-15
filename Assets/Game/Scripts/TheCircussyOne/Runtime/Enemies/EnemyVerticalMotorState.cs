namespace TheCircussyOne.Runtime
{
    public sealed class EnemyVerticalMotorState
    {
        public float CurrentHeight { get; set; }
        public float TargetHeight { get; set; }
        public float VerticalVelocity { get; set; }
        public float CachedEnvironmentHeight { get; set; }
        public bool HasCachedEnvironmentHeight { get; set; }
        public bool IsEnvironmentClimbing { get; set; }
        public float BlockedClimbTargetHeight { get; set; }
        public float HorizontalHoldUntilHeight { get; set; }
        public bool IsSupportClimbing { get; set; }
        public int SupportClimbTargetSpawnId { get; set; } = -1;
        public float SupportClimbTargetHeight { get; set; }
        public float SupportHorizontalHoldUntilHeight { get; set; }
        public int StackLayer { get; set; }
        public float StackTargetHeight { get; set; }
        public int SupportedBySpawnId { get; set; } = -1;

        public void BeginEnvironmentClimb(float targetHeight, float releaseTolerance)
        {
            float safeTarget = UnityEngine.Mathf.Max(0f, targetHeight);
            IsEnvironmentClimbing = true;
            BlockedClimbTargetHeight = safeTarget;
            HorizontalHoldUntilHeight = UnityEngine.Mathf.Max(0f, safeTarget - UnityEngine.Mathf.Max(0f, releaseTolerance));
            CachedEnvironmentHeight = safeTarget;
            TargetHeight = safeTarget;
            HasCachedEnvironmentHeight = true;
        }

        public void ClearEnvironmentClimb()
        {
            IsEnvironmentClimbing = false;
            BlockedClimbTargetHeight = 0f;
            HorizontalHoldUntilHeight = 0f;
            CachedEnvironmentHeight = 0f;
            HasCachedEnvironmentHeight = false;
        }

        public void BeginSupportClimb(float targetHeight, float releaseTolerance, int layer, int supportedBySpawnId)
        {
            float safeTarget = UnityEngine.Mathf.Max(0f, targetHeight);
            IsSupportClimbing = true;
            SupportClimbTargetSpawnId = supportedBySpawnId;
            SupportClimbTargetHeight = safeTarget;
            SupportHorizontalHoldUntilHeight = UnityEngine.Mathf.Max(0f, safeTarget - UnityEngine.Mathf.Max(0f, releaseTolerance));
            TargetHeight = safeTarget;
            StackLayer = UnityEngine.Mathf.Max(0, layer);
            StackTargetHeight = safeTarget;
            SupportedBySpawnId = StackLayer > 0 ? supportedBySpawnId : -1;
        }

        public void ClearSupportClimb()
        {
            IsSupportClimbing = false;
            SupportClimbTargetSpawnId = -1;
            SupportClimbTargetHeight = 0f;
            SupportHorizontalHoldUntilHeight = 0f;
            StackLayer = 0;
            StackTargetHeight = 0f;
            SupportedBySpawnId = -1;
        }

        public void Reset(float height = 0f)
        {
            CurrentHeight = height;
            TargetHeight = height;
            VerticalVelocity = 0f;
            CachedEnvironmentHeight = height;
            HasCachedEnvironmentHeight = false;
            ClearEnvironmentClimb();
            ClearSupportClimb();
        }
    }
}
