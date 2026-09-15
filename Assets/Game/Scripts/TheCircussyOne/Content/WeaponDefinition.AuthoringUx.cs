namespace TheCircussyOne.Content
{
    public sealed partial class WeaponDefinition
    {
        [Sirenix.OdinInspector.TabGroup(Tabs, "Diagnostics")]
        [Sirenix.OdinInspector.ShowInInspector, Sirenix.OdinInspector.ReadOnly]
        [Sirenix.OdinInspector.LabelText("Weapon Family"), Sirenix.OdinInspector.PropertyOrder(80)]
        private string WeaponFamilySummary => WeaponAuthoringApplicability.FamilySummary(this);

        [Sirenix.OdinInspector.TabGroup(Tabs, "Diagnostics")]
        [Sirenix.OdinInspector.ShowInInspector, Sirenix.OdinInspector.ReadOnly]
        [Sirenix.OdinInspector.LabelText("Active Hooks"), Sirenix.OdinInspector.PropertyOrder(81)]
        private string ActiveGameplayHooksSummary => WeaponAuthoringApplicability.ActiveGameplayHooksSummary(this);

        [Sirenix.OdinInspector.TabGroup(Tabs, "Diagnostics")]
        [Sirenix.OdinInspector.ShowInInspector, Sirenix.OdinInspector.ReadOnly]
        [Sirenix.OdinInspector.LabelText("Acquisition"), Sirenix.OdinInspector.PropertyOrder(82)]
        private string AcquisitionSummary => WeaponAuthoringApplicability.AcquisitionSummary(this);

        [Sirenix.OdinInspector.TabGroup(Tabs, "Diagnostics")]
        [Sirenix.OdinInspector.ShowInInspector, Sirenix.OdinInspector.ReadOnly]
        [Sirenix.OdinInspector.LabelText("Supported Upgrade Stats"), Sirenix.OdinInspector.PropertyOrder(83)]
        private string SupportedUpgradeStatsSummary => WeaponAuthoringApplicability.SupportedUpgradeStatsSummary(this);

        [Sirenix.OdinInspector.TabGroup(Tabs, "Diagnostics")]
        [Sirenix.OdinInspector.ShowInInspector, Sirenix.OdinInspector.ReadOnly]
        [Sirenix.OdinInspector.MultiLineProperty(3), Sirenix.OdinInspector.LabelText("Inactive Supported Stats")]
        [Sirenix.OdinInspector.InfoBox("One or more supported upgrade stats are configured, but the current weapon family does not consume them.", Sirenix.OdinInspector.InfoMessageType.Warning, nameof(HasInactiveSupportedUpgradeStats))]
        [Sirenix.OdinInspector.PropertyOrder(84)]
        private string InactiveSupportedUpgradeStatsWarning => WeaponAuthoringApplicability.InactiveSupportedUpgradeStatsWarning(this);

        private bool HasInactiveSupportedUpgradeStats => !string.IsNullOrWhiteSpace(InactiveSupportedUpgradeStatsWarning);

        private bool IsProjectileTravelActive => WeaponAuthoringApplicability.IsActive(this, WeaponAuthoringArea.ProjectileTravel);
        private bool IsProjectileEmissionActive => WeaponAuthoringApplicability.IsActive(this, WeaponAuthoringArea.ProjectileEmission);
        private bool IsProjectileAimErrorActive => WeaponAuthoringApplicability.IsActive(this, WeaponAuthoringArea.ProjectileAimError);
        private bool IsProjectileTrajectoryActive => WeaponAuthoringApplicability.IsActive(this, WeaponAuthoringArea.ProjectileTrajectory);
        private bool IsProjectileCollisionActive => WeaponAuthoringApplicability.IsActive(this, WeaponAuthoringArea.ProjectileCollision);
        private bool IsProjectileSpawnPoseActive => WeaponAuthoringApplicability.IsActive(this, WeaponAuthoringArea.ProjectileSpawnPose);
        private bool IsProjectileTrailActive => WeaponAuthoringApplicability.IsActive(this, WeaponAuthoringArea.ProjectileTrail);
        private bool IsBounceSettingsActive => WeaponAuthoringApplicability.IsActive(this, WeaponAuthoringArea.Bounce);
        private bool IsExplosionSettingsActive => WeaponAuthoringApplicability.IsActive(this, WeaponAuthoringArea.Explosion);
        private bool IsChainSettingsActive => WeaponAuthoringApplicability.IsActive(this, WeaponAuthoringArea.Chain);
        private bool IsOrbitSettingsActive => WeaponAuthoringApplicability.IsActive(this, WeaponAuthoringArea.Orbit);
        private bool IsOrbitAreaHeatSettingsActive => WeaponAuthoringApplicability.IsActive(this, WeaponAuthoringArea.OrbitAreaHeat);
        private bool IsArcTrajectorySettingsActive => IsProjectileTrajectoryActive && projectileTrajectoryMode == ProjectileTrajectoryMode.Arc;
        private bool IsProjectileTrailDetailsActive => IsProjectileTrailActive && projectileTrailEnabled;

        private bool ShowProjectileTravelDisabledReason => !IsProjectileTravelActive;
        private bool ShowProjectileEmissionDisabledReason => !IsProjectileEmissionActive;
        private bool ShowProjectileAimErrorDisabledReason => !IsProjectileAimErrorActive;
        private bool ShowProjectileTrajectoryDisabledReason => !IsProjectileTrajectoryActive;
        private bool ShowProjectileCollisionDisabledReason => !IsProjectileCollisionActive;
        private bool ShowProjectileSpawnPoseDisabledReason => !IsProjectileSpawnPoseActive;
        private bool ShowProjectileTrailDisabledReason => !IsProjectileTrailActive;
        private bool ShowBounceDisabledReason => !IsBounceSettingsActive;
        private bool ShowExplosionDisabledReason => !IsExplosionSettingsActive;
        private bool ShowChainDisabledReason => !IsChainSettingsActive;
        private bool ShowOrbitDisabledReason => !IsOrbitSettingsActive;
        private bool ShowOrbitAreaHeatDisabledReason => !IsOrbitAreaHeatSettingsActive;
    }
}
