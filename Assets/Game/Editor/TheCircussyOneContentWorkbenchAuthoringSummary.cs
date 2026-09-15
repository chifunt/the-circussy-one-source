using System.Collections.Generic;
using TheCircussyOne.Content;
using UnityEngine;

public sealed class ContentWorkbenchAuthoringSummary
{
    private const float MinimumInspectorLabelWidth = 118f;
    private const float MaximumInspectorLabelWidth = 160f;

    private ContentWorkbenchAuthoringSummary(IReadOnlyList<string> hiddenSettings)
    {
        HiddenSettings = hiddenSettings;
    }

    public IReadOnlyList<string> HiddenSettings { get; }
    public bool HasHiddenSettings => HiddenSettings.Count > 0;

    public static float InspectorLabelWidth(float availableWidth)
    {
        return Mathf.Clamp(availableWidth * 0.24f, MinimumInspectorLabelWidth, MaximumInspectorLabelWidth);
    }

    public static ContentWorkbenchAuthoringSummary Build(Object asset)
    {
        var lines = new List<string>();
        switch (asset)
        {
            case WeaponDefinition weapon:
                AddWeaponHiddenSettings(weapon, lines);
                break;
            case EnemyDefinition enemy:
                AddEnemyHiddenSettings(enemy, lines);
                break;
            case TalentDefinition talent:
                AddTalentHiddenSettings(talent, lines);
                break;
            case ItemDefinition item:
                AddItemHiddenSettings(item, lines);
                break;
        }

        return new ContentWorkbenchAuthoringSummary(lines);
    }

    private static void AddWeaponHiddenSettings(WeaponDefinition weapon, List<string> lines)
    {
        AddInactiveWeaponArea(weapon, WeaponAuthoringArea.ProjectileTravel, "Projectile travel", lines);
        AddInactiveWeaponArea(weapon, WeaponAuthoringArea.ProjectileEmission, "Projectile emission", lines);
        AddInactiveWeaponArea(weapon, WeaponAuthoringArea.ProjectileAimError, "Projectile aim error", lines);
        AddInactiveWeaponArea(weapon, WeaponAuthoringArea.ProjectileTrajectory, "Projectile trajectory", lines);
        AddInactiveWeaponArea(weapon, WeaponAuthoringArea.ProjectileCollision, "Projectile collision", lines);
        AddInactiveWeaponArea(weapon, WeaponAuthoringArea.ProjectileSpawnPose, "Projectile spawn pose", lines);
        AddInactiveWeaponArea(weapon, WeaponAuthoringArea.ProjectileTrail, "Projectile trail", lines);
        AddInactiveWeaponArea(weapon, WeaponAuthoringArea.Bounce, "Bounce settings", lines);
        AddInactiveWeaponArea(weapon, WeaponAuthoringArea.Explosion, "Explosion settings", lines);
        AddInactiveWeaponArea(weapon, WeaponAuthoringArea.Chain, "Chain settings", lines);
        AddInactiveWeaponArea(weapon, WeaponAuthoringArea.Orbit, "Orbit settings", lines);
        AddInactiveWeaponArea(weapon, WeaponAuthoringArea.OrbitAreaHeat, "Orbit area heat", lines);
    }

    private static void AddInactiveWeaponArea(WeaponDefinition weapon, WeaponAuthoringArea area, string label, List<string> lines)
    {
        if (WeaponAuthoringApplicability.IsActive(weapon, area))
        {
            return;
        }

        lines.Add($"{label}: {WeaponAuthoringApplicability.DisabledReason(weapon, area)}");
    }

    private static void AddEnemyHiddenSettings(EnemyDefinition enemy, List<string> lines)
    {
        if (enemy == null)
        {
            return;
        }

        if (!EnemyAuthoringApplicability.IsEnvironmentClimbingActive(enemy.climb))
        {
            lines.Add($"Environment climb controls: {EnemyAuthoringApplicability.EnvironmentClimbingDisabledReason(enemy.climb)}");
        }
        else if (!EnemyAuthoringApplicability.IsFallbackClimbSpeedActive(enemy.climb))
        {
            lines.Add($"Fallback climb speed: {EnemyAuthoringApplicability.FallbackClimbSpeedDisabledReason(enemy.climb)}");
        }

        if (!EnemyAuthoringApplicability.IsSupportStackingActive(enemy.stack))
        {
            lines.Add($"Support stacking controls: {EnemyAuthoringApplicability.SupportStackingDisabledReason(enemy.stack)}");
        }
    }

    private static void AddTalentHiddenSettings(TalentDefinition talent, List<string> lines)
    {
        if (talent == null)
        {
            return;
        }

        lines.Add(talent.IsShared
            ? "Performer-specific target: hidden because this Talent is in the shared pool."
            : "Shared performer access: hidden because this Talent is performer-specific.");

        if (talent.repeatPolicy == TalentRepeatPolicy.Infinite)
        {
            lines.Add("Max level: hidden because this Talent is repeatable without a cap.");
        }
    }

    private static void AddItemHiddenSettings(ItemDefinition item, List<string> lines)
    {
        if (item == null)
        {
            return;
        }

        if (item.stackPolicy == ItemStackPolicy.Unique || item.stackPolicy == ItemStackPolicy.InvalidDuplicate)
        {
            lines.Add("Max stacks: hidden because this item policy always uses one stack.");
        }

        if (item.stackPolicy != ItemStackPolicy.StackWithCap)
        {
            lines.Add("Effect cap stacks: hidden because this policy applies effects from its normal stack count.");
        }

        if (item.stackPolicy != ItemStackPolicy.StackDiminishing)
        {
            lines.Add("Diminishing falloff: hidden because this policy does not scale later stacks down.");
        }
    }
}
