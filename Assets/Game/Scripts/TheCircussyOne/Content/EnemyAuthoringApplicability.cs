namespace TheCircussyOne.Content
{
    public static class EnemyAuthoringApplicability
    {
        public static bool IsEnvironmentClimbingActive(EnemyClimbProfile profile)
        {
            return profile != null && profile.canClimbEnvironment;
        }

        public static bool IsFallbackClimbSpeedActive(EnemyClimbProfile profile)
        {
            return IsEnvironmentClimbingActive(profile) && !profile.useMoveSpeedForClimb;
        }

        public static bool IsSupportStackingActive(EnemyStackProfile profile)
        {
            return profile != null && profile.policy == EnemyStackPolicy.SupportBased;
        }

        public static bool IsEnvironmentClimbingActive(bool enabled)
        {
            return enabled;
        }

        public static bool IsFallbackClimbSpeedActive(bool enabled, bool useMoveSpeed)
        {
            return enabled && !useMoveSpeed;
        }

        public static bool IsSupportStackingActive(bool enabled)
        {
            return enabled;
        }

        public static string EnvironmentClimbingDisabledReason(EnemyClimbProfile profile)
        {
            return IsEnvironmentClimbingActive(profile)
                ? string.Empty
                : "Disabled because this enemy cannot climb environment surfaces.";
        }

        public static string EnvironmentClimbingDisabledReason(bool enabled)
        {
            return enabled ? string.Empty : "Disabled because enemy climbing is off.";
        }

        public static string FallbackClimbSpeedDisabledReason(EnemyClimbProfile profile)
        {
            if (!IsEnvironmentClimbingActive(profile))
            {
                return EnvironmentClimbingDisabledReason(profile);
            }

            return profile.useMoveSpeedForClimb
                ? "Disabled because climb speed is using this enemy's move speed."
                : string.Empty;
        }

        public static string FallbackClimbSpeedDisabledReason(bool enabled, bool useMoveSpeed)
        {
            if (!enabled)
            {
                return EnvironmentClimbingDisabledReason(false);
            }

            return useMoveSpeed
                ? "Disabled because climb speed is using enemy move speed."
                : string.Empty;
        }

        public static string SupportStackingDisabledReason(EnemyStackProfile profile)
        {
            if (profile == null)
            {
                return "Disabled because no stack profile is available.";
            }

            return profile.policy == EnemyStackPolicy.SupportBased
                ? string.Empty
                : "Disabled because Stack Policy is GroundOnly. Switch to SupportBased to edit enemy-on-enemy climbing.";
        }

        public static string SupportStackingDisabledReason(bool enabled)
        {
            return enabled
                ? string.Empty
                : "Disabled because enemy pile climbing is off.";
        }

        public static string ClimbSummary(EnemyClimbProfile profile)
        {
            if (profile == null)
            {
                return "Missing climb profile.";
            }

            string climbSpeed = profile.useMoveSpeedForClimb
                ? $"move speed x{profile.climbSpeedMultiplier:0.##}"
                : $"{profile.fallbackClimbSpeed:0.##} u/s";
            return profile.canClimbEnvironment
                ? $"Environment climb on / {climbSpeed} / max height {profile.maxEnvironmentClimbHeight:0.##}u / probe every {profile.environmentProbeIntervalFrames} frames"
                : $"Environment climb off / falling uses gravity {profile.gravity:0.##}";
        }

        public static string StackSummary(EnemyStackProfile profile)
        {
            if (profile == null)
            {
                return "Missing stack profile.";
            }

            return profile.policy == EnemyStackPolicy.SupportBased
                ? $"SupportBased / radius {profile.pileRadius:0.##}u / starts at {profile.pileStartCount} / max layers {profile.maxStackLayers} / layer height x{profile.layerHeightMultiplier:0.##}"
                : $"{profile.policy} / enemy-on-enemy climbing inactive";
        }
    }
}
