using TheCircussyOne.Content;
using TheCircussyOne.Stats;
using Sirenix.OdinInspector.Editor;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class ContentWorkbenchWeaponInspector
{
    private static readonly string[] TabLabels =
    {
        "Identity",
        "Timing",
        "Projectile",
        "Orbit",
        "Targeting",
        "Spawn Pose",
        "Visuals",
        "Upgrade Track",
        "Diagnostics"
    };

    private static ContentWorkbenchWeaponTab selectedTab = ContentWorkbenchWeaponTab.Identity;
    private static StatId addSupportedStatChoice = StatId.WeaponFlatDamage;

    public static bool CanDraw(UnityEngine.Object asset)
    {
        return asset is WeaponDefinition;
    }

    public static void Draw(WeaponDefinition weapon)
    {
        if (weapon == null)
        {
            EditorGUILayout.HelpBox("No weapon selected.", MessageType.Info);
            return;
        }

        var serialized = new SerializedObject(weapon);
        serialized.Update();

        DrawHeader(weapon);
        DrawTabs();
        switch (selectedTab)
        {
            case ContentWorkbenchWeaponTab.Identity:
                DrawIdentity(serialized, weapon);
                break;
            case ContentWorkbenchWeaponTab.Timing:
                DrawTiming(serialized);
                break;
            case ContentWorkbenchWeaponTab.Projectile:
                DrawProjectile(serialized, weapon);
                break;
            case ContentWorkbenchWeaponTab.Orbit:
                DrawOrbit(serialized, weapon);
                break;
            case ContentWorkbenchWeaponTab.Targeting:
                DrawTargeting(serialized, weapon);
                break;
            case ContentWorkbenchWeaponTab.SpawnPose:
                DrawSpawnPose(serialized, weapon);
                break;
            case ContentWorkbenchWeaponTab.Visuals:
                DrawVisuals(serialized, weapon);
                break;
            case ContentWorkbenchWeaponTab.UpgradeTrack:
                DrawUpgradeTrack(serialized, weapon);
                break;
            case ContentWorkbenchWeaponTab.Diagnostics:
                DrawDiagnostics(weapon);
                break;
        }

        if (serialized.ApplyModifiedProperties())
        {
            EditorUtility.SetDirty(weapon);
        }
    }

    public static int TabCount => TabLabels.Length;

    public static string TabLabel(int index)
    {
        return TabLabels[Mathf.Clamp(index, 0, TabLabels.Length - 1)];
    }

    private static void DrawHeader(WeaponDefinition weapon)
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField("Weapon Authoring", EditorStyles.boldLabel);
            if (GUILayout.Button("Open Odin Inspector", GUILayout.Width(150f), GUILayout.Height(22f)))
            {
                OdinEditorWindow.InspectObject(weapon);
            }
        }

        EditorGUILayout.LabelField("Workbench uses focused tabs here so compact fields stay editable. Use Odin Inspector for the full advanced asset view.", EditorStyles.wordWrappedMiniLabel);
    }

    private static void DrawTabs()
    {
        EditorGUILayout.Space(4f);
        selectedTab = (ContentWorkbenchWeaponTab)GUILayout.Toolbar((int)selectedTab, TabLabels, GUILayout.MinHeight(24f));
        EditorGUILayout.Space(4f);
    }

    private static void DrawIdentity(SerializedObject serialized, WeaponDefinition weapon)
    {
        DrawSection("Identity");
        ContentWorkbenchFieldDrawer.DrawReadOnlyProperty(serialized, nameof(WeaponDefinition.weaponId), "Weapon ID");
        DrawProperty(serialized, nameof(WeaponDefinition.displayName), "Display Name");
        ContentWorkbenchFieldDrawer.DrawIdNameSync(
            serialized.FindProperty(nameof(WeaponDefinition.weaponId)),
            serialized.FindProperty(nameof(WeaponDefinition.displayName))?.stringValue,
            "weapon",
            "Weapon ID");
        DrawProperty(serialized, nameof(WeaponDefinition.canAppearAsLevelUpWeapon), "Can Appear As Level-Up Weapon");
        if (weapon.canAppearAsLevelUpWeapon)
        {
            DrawProperty(serialized, nameof(WeaponDefinition.addWeaponShortDescription), "Short Description", includeChildren: true);
        }

        EditorGUILayout.LabelField("Computed Tags", ComputedContentTagRules.Format(weapon.Tags), EditorStyles.miniLabel);
    }

    private static void DrawTiming(SerializedObject serialized)
    {
        DrawSection("Timing");
        DrawUnitProperty(serialized, nameof(WeaponDefinition.baseFireIntervalSeconds), "Base Interval", "sec");
        DrawUnitProperty(serialized, nameof(WeaponDefinition.minimumFireIntervalSeconds), "Minimum Interval", "sec");
    }

    private static void DrawProjectile(SerializedObject serialized, WeaponDefinition weapon)
    {
        DrawSection("Projectile");
        DrawProperty(serialized, nameof(WeaponDefinition.projectileDamage), "Base Damage");
        DrawProperty(serialized, nameof(WeaponDefinition.projectileHitMask), "Enemy Hurtbox Mask");

        if (IsActive(weapon, WeaponAuthoringArea.ProjectileTravel))
        {
            DrawUnitProperty(serialized, nameof(WeaponDefinition.projectileSpeed), "Projectile Speed", "u/s");
            DrawUnitProperty(serialized, nameof(WeaponDefinition.projectileLifetimeSeconds), "Projectile Lifetime", "sec");
            DrawUnitProperty(serialized, nameof(WeaponDefinition.projectileHitRadius), "Hit Radius", "u");
        }

        if (IsActive(weapon, WeaponAuthoringArea.ProjectileEmission))
        {
            DrawSubsection("Emission");
            DrawProperty(serialized, nameof(WeaponDefinition.baseProjectileCount), "Base Projectile Count");
            DrawProperty(serialized, nameof(WeaponDefinition.maxProjectileCount), "Max Projectile Count");
            DrawUnitProperty(serialized, nameof(WeaponDefinition.baseSpreadAngleDegrees), "Pattern Spread", "deg");
        }

        if (IsActive(weapon, WeaponAuthoringArea.ProjectileAimError))
        {
            DrawSubsection("Aim Error");
            DrawUnitProperty(serialized, nameof(WeaponDefinition.baseAimErrorDegrees), "Base Aim Error", "deg");
            DrawProperty(serialized, nameof(WeaponDefinition.accuracyAffectsAimError), "Accuracy Affects Aim Error");
        }

        if (IsActive(weapon, WeaponAuthoringArea.ProjectileTrajectory))
        {
            DrawSubsection("Trajectory");
            DrawProperty(serialized, nameof(WeaponDefinition.projectileTrajectoryMode), "Mode");
            if (weapon.projectileTrajectoryMode == ProjectileTrajectoryMode.Arc)
            {
                DrawUnitProperty(serialized, nameof(WeaponDefinition.projectileArcHeight), "Arc Height", "u");
            }
        }

        if (IsActive(weapon, WeaponAuthoringArea.ProjectileTravel))
        {
            DrawSubsection("Bounce");
            DrawProperty(serialized, nameof(WeaponDefinition.bounceEnabled), "Enabled");
            if (IsActive(weapon, WeaponAuthoringArea.Bounce))
            {
                DrawProperty(serialized, nameof(WeaponDefinition.baseBounceCount), "Base Bounce Count");
                DrawProperty(serialized, nameof(WeaponDefinition.maxBounceCount), "Max Bounce Count");
                DrawUnitProperty(serialized, nameof(WeaponDefinition.enemyBounceSearchRadius), "Enemy Search Radius", "u");
                DrawProperty(serialized, nameof(WeaponDefinition.bounceOffWorldBlockers), "Bounce Off World Blockers");
            }

            DrawSubsection("Explosion");
            DrawProperty(serialized, nameof(WeaponDefinition.explosiveEnabled), "Enabled");
            if (IsActive(weapon, WeaponAuthoringArea.Explosion))
            {
                DrawUnitProperty(serialized, nameof(WeaponDefinition.baseSplashRadius), "Base Splash Radius", "u");
                DrawUnitProperty(serialized, nameof(WeaponDefinition.maxSplashRadius), "Max Splash Radius", "u");
                DrawProperty(serialized, nameof(WeaponDefinition.secondarySplashDamageMultiplier), "Secondary Damage Multiplier");
                DrawProperty(serialized, nameof(WeaponDefinition.explodeOnWorldImpact), "Explode On World Impact");
                DrawProperty(serialized, nameof(WeaponDefinition.explosionVfxEnabled), "Explosion VFX Enabled");
            }

            DrawSubsection("Chain");
            DrawProperty(serialized, nameof(WeaponDefinition.chainEnabled), "Enabled");
            if (IsActive(weapon, WeaponAuthoringArea.Chain))
            {
                DrawProperty(serialized, nameof(WeaponDefinition.baseChainCount), "Base Chain Count");
                DrawProperty(serialized, nameof(WeaponDefinition.maxChainCount), "Max Chain Count");
                DrawUnitProperty(serialized, nameof(WeaponDefinition.chainSearchRadius), "Search Radius", "u");
                DrawProperty(serialized, nameof(WeaponDefinition.chainDamageMultiplier), "Damage Multiplier");
                DrawProperty(serialized, nameof(WeaponDefinition.chainRequiresLineOfSight), "Requires Line Of Sight");
            }
        }
    }

    private static void DrawOrbit(SerializedObject serialized, WeaponDefinition weapon)
    {
        DrawSection("Orbit");
        DrawProperty(serialized, nameof(WeaponDefinition.orbitEnabled), "Enabled");
        if (!IsActive(weapon, WeaponAuthoringArea.Orbit))
        {
            EditorGUILayout.LabelField(WeaponAuthoringApplicability.DisabledReason(weapon, WeaponAuthoringArea.Orbit), EditorStyles.wordWrappedMiniLabel);
            return;
        }

        DrawProperty(serialized, nameof(WeaponDefinition.baseOrbitCount), "Base Orbit Count");
        DrawProperty(serialized, nameof(WeaponDefinition.maxOrbitCount), "Max Orbit Count");
        DrawUnitProperty(serialized, nameof(WeaponDefinition.orbitRadius), "Orbit Radius", "u");
        DrawUnitProperty(serialized, nameof(WeaponDefinition.maxOrbitRadius), "Max Orbit Radius", "u");
        DrawUnitProperty(serialized, nameof(WeaponDefinition.orbitHitRadius), "Hit Radius", "u");
        DrawUnitProperty(serialized, nameof(WeaponDefinition.maxOrbitHitRadius), "Max Hit Radius", "u");
        DrawUnitProperty(serialized, nameof(WeaponDefinition.orbitDegreesPerSecond), "Orbit Speed", "deg/sec");
        DrawUnitProperty(serialized, nameof(WeaponDefinition.orbitHeightOffset), "Height Offset", "u");
        DrawProperty(serialized, nameof(WeaponDefinition.orbitAreaDamageEnabled), "Area Damage Enabled");
        if (IsActive(weapon, WeaponAuthoringArea.OrbitAreaHeat))
        {
            DrawProperty(serialized, nameof(WeaponDefinition.orbitAreaDamageMultiplier), "Area Damage Multiplier");
            DrawUnitProperty(serialized, nameof(WeaponDefinition.orbitAreaHitIntervalSeconds), "Area Hit Interval", "sec");
        }
    }

    private static void DrawTargeting(SerializedObject serialized, WeaponDefinition weapon)
    {
        DrawSection("Targeting");
        if (IsActive(weapon, WeaponAuthoringArea.ProjectileTravel))
        {
            DrawUnitProperty(serialized, nameof(WeaponDefinition.weaponRange), "Projectile Target Range", "u");
        }

        DrawProperty(serialized, nameof(WeaponDefinition.requireLineOfSight), "Require Line Of Sight");
        DrawProperty(serialized, nameof(WeaponDefinition.lineOfSightMask), "Line Of Sight Mask");

        if (IsActive(weapon, WeaponAuthoringArea.ProjectileCollision))
        {
            DrawProperty(serialized, nameof(WeaponDefinition.projectileBlockMask), "Projectile Block Mask");
            DrawUnitProperty(serialized, nameof(WeaponDefinition.projectileBlockRadius), "Projectile Block Radius", "u");
        }
    }

    private static void DrawSpawnPose(SerializedObject serialized, WeaponDefinition weapon)
    {
        DrawSection("Spawn Pose");
        if (!IsActive(weapon, WeaponAuthoringArea.ProjectileSpawnPose))
        {
            EditorGUILayout.LabelField(WeaponAuthoringApplicability.DisabledReason(weapon, WeaponAuthoringArea.ProjectileSpawnPose), EditorStyles.wordWrappedMiniLabel);
            return;
        }

        DrawUnitProperty(serialized, nameof(WeaponDefinition.projectileSpawnForwardOffset), "Forward Offset", "u");
        DrawUnitProperty(serialized, nameof(WeaponDefinition.projectileSpawnHeight), "Height", "u");
    }

    private static void DrawVisuals(SerializedObject serialized, WeaponDefinition weapon)
    {
        DrawSection("Visuals");
        ContentWorkbenchFieldDrawer.DrawIconSpriteProperty(
            serialized,
            nameof(WeaponDefinition.iconSprite),
            "HUD/Card Icon",
            "No icon assigned. Level-up cards and HUD slots use projectile color fallback until a placeholder or final sprite is assigned.");

        if (IsActive(weapon, WeaponAuthoringArea.ProjectileTravel))
        {
            DrawProperty(serialized, nameof(WeaponDefinition.projectileVisualShape), "Shape");
            DrawProperty(serialized, nameof(WeaponDefinition.projectileVisualScale), "Visual Scale");
        }

        DrawProperty(serialized, nameof(WeaponDefinition.projectilePrimaryColor), "Primary Color");
        DrawProperty(serialized, nameof(WeaponDefinition.projectileEmissionColor), "Emission Color");
        DrawProperty(serialized, nameof(WeaponDefinition.projectileEmissionStrength), "Emission Strength");

        if (IsActive(weapon, WeaponAuthoringArea.ProjectileTrail))
        {
            DrawSubsection("Trail");
            DrawProperty(serialized, nameof(WeaponDefinition.projectileTrailEnabled), "Enabled");
            if (weapon.projectileTrailEnabled)
            {
                DrawProperty(serialized, nameof(WeaponDefinition.projectileTrailColor), "Color");
                DrawUnitProperty(serialized, nameof(WeaponDefinition.projectileTrailWidth), "Width", "u");
                DrawUnitProperty(serialized, nameof(WeaponDefinition.projectileTrailLifetime), "Lifetime", "sec");
            }
        }
    }

    private static void DrawUpgradeTrack(SerializedObject serialized, WeaponDefinition weapon)
    {
        DrawSection("Upgrade Track");
        EditorGUILayout.LabelField("Owned weapon cards are assigned from the Weapon Upgrades domain by selecting this weapon as the target.", EditorStyles.wordWrappedMiniLabel);
        using (new EditorGUI.DisabledScope(true))
        {
            DrawProperty(serialized, nameof(WeaponDefinition.upgradeTrack), "Owned Weapon Cards", includeChildren: true);
        }

        DrawSupportedStats(serialized.FindProperty(nameof(WeaponDefinition.supportedUpgradeStats)), weapon);
    }

    private static void DrawSupportedStats(SerializedProperty list, WeaponDefinition weapon)
    {
        DrawSubsection("Supported Stats");
        if (list == null || !list.isArray)
        {
            EditorGUILayout.HelpBox("Missing supported stats list.", MessageType.Warning);
            return;
        }

        EditorGUILayout.LabelField("These are the only weapon-local stats that owned upgrade cards may use for this weapon.", EditorStyles.wordWrappedMiniLabel);
        EditorGUILayout.LabelField("Level-Up Roll Pool", LevelUpUpgradePoolSummary(weapon), EditorStyles.wordWrappedMiniLabel);
        if (list.arraySize == 0)
        {
            EditorGUILayout.HelpBox("No supported stats are configured. Add at least one weapon-local stat before creating owned weapon upgrades.", MessageType.Info);
        }

        for (int i = 0; i < list.arraySize; i++)
        {
            SerializedProperty element = list.GetArrayElementAtIndex(i);
            IReadOnlyList<StatId> currentStats = ReadSupportedStats(list);
            IReadOnlyList<ContentWorkbenchStatOption> rowOptions = SupportedStatRowOptions(currentStats, i);
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                ContentWorkbenchFieldDrawer.DrawStatPopup(rowOptions, element, $"Stat {i + 1}");
                if (GUILayout.Button("Remove", GUILayout.Width(82f), GUILayout.Height(20f)))
                {
                    list.DeleteArrayElementAtIndex(i);
                    return;
                }
            }
        }

        IReadOnlyList<ContentWorkbenchStatOption> addOptions = SupportedStatAddOptions(ReadSupportedStats(list));
        if (addOptions.Count == 0)
        {
            EditorGUILayout.LabelField("All weapon-local stats are already supported.", EditorStyles.miniLabel);
        }
        else
        {
            if (!ContentWorkbenchStatOptions.Contains(addOptions, addSupportedStatChoice))
            {
                addSupportedStatChoice = addOptions[0].StatId;
            }

            addSupportedStatChoice = ContentWorkbenchFieldDrawer.DrawStatPopup(addOptions, addSupportedStatChoice, "Add Supported Stat");
            if (GUILayout.Button("Add Supported Stat", GUILayout.Height(22f)))
            {
                int index = list.arraySize;
                list.InsertArrayElementAtIndex(index);
                list.GetArrayElementAtIndex(index).intValue = (int)addSupportedStatChoice;
                IReadOnlyList<ContentWorkbenchStatOption> remaining = SupportedStatAddOptions(ReadSupportedStats(list));
                addSupportedStatChoice = remaining.Count > 0 ? remaining[0].StatId : addSupportedStatChoice;
            }
        }

        EditorGUILayout.LabelField("Current", WeaponAuthoringApplicability.SupportedUpgradeStatsSummary(weapon), EditorStyles.wordWrappedMiniLabel);
        string warning = WeaponAuthoringApplicability.InactiveSupportedUpgradeStatsWarning(weapon);
        if (!string.IsNullOrWhiteSpace(warning))
        {
            EditorGUILayout.HelpBox(warning, MessageType.Warning);
        }
    }

    private static void DrawDiagnostics(WeaponDefinition weapon)
    {
        DrawSection("Diagnostics");
        EditorGUILayout.LabelField("Weapon Family", WeaponAuthoringApplicability.FamilySummary(weapon));
        EditorGUILayout.LabelField("Active Hooks", WeaponAuthoringApplicability.ActiveGameplayHooksSummary(weapon), EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("Acquisition", WeaponAuthoringApplicability.AcquisitionSummary(weapon), EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("Supported Upgrade Stats", WeaponAuthoringApplicability.SupportedUpgradeStatsSummary(weapon), EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("Level-Up Roll Pool", LevelUpUpgradePoolSummary(weapon), EditorStyles.wordWrappedLabel);

        string warning = WeaponAuthoringApplicability.InactiveSupportedUpgradeStatsWarning(weapon);
        if (!string.IsNullOrWhiteSpace(warning))
        {
            EditorGUILayout.HelpBox(warning, MessageType.Warning);
        }
    }

    private static void DrawSection(string title)
    {
        ContentWorkbenchFieldDrawer.DrawSection(title);
    }

    private static void DrawSubsection(string title)
    {
        ContentWorkbenchFieldDrawer.DrawSubsection(title);
    }

    private static void DrawProperty(SerializedObject serialized, string propertyName, string label, bool includeChildren = false)
    {
        ContentWorkbenchFieldDrawer.DrawProperty(serialized, propertyName, label, includeChildren);
    }

    private static void DrawUnitProperty(SerializedObject serialized, string propertyName, string label, string unit)
    {
        ContentWorkbenchFieldDrawer.DrawUnitProperty(serialized, propertyName, label, unit);
    }

    private static bool IsActive(WeaponDefinition weapon, WeaponAuthoringArea area)
    {
        return WeaponAuthoringApplicability.IsActive(weapon, area);
    }

    public static IReadOnlyList<ContentWorkbenchStatOption> SupportedStatAddOptions(IReadOnlyList<StatId> currentStats)
    {
        return SupportedStatOptionsExcluding(currentStats, -1);
    }

    public static string LevelUpUpgradePoolSummary(WeaponDefinition weapon)
    {
        if (weapon == null)
        {
            return "No weapon selected.";
        }

        IReadOnlyList<StatId> stats = weapon.SupportedUpgradeStats;
        if (stats == null || stats.Count == 0)
        {
            return "No owned weapon upgrade stats can roll for this weapon.";
        }

        var names = new List<string>();
        for (int i = 0; i < stats.Count; i++)
        {
            StatId stat = stats[i];
            if (ContentStatValidationRules.IsWeaponUpgradeStat(stat))
            {
                names.Add(ContentModifierDisplayRules.StatDisplayName(stat));
            }
        }

        string statSummary = names.Count > 0
            ? string.Join(", ", names)
            : "No valid weapon-local stats.";
        string acquisition = weapon.CanAppearAsLevelUpWeapon
            ? " Add-weapon acquisition is a Common-only card."
            : " This weapon cannot appear as an add-weapon card.";
        return $"Owned upgrade cards may roll: {statSummary}.{acquisition}";
    }

    public static IReadOnlyList<ContentWorkbenchStatOption> SupportedStatRowOptions(IReadOnlyList<StatId> currentStats, int rowIndex)
    {
        return SupportedStatOptionsExcluding(currentStats, rowIndex);
    }

    private static IReadOnlyList<ContentWorkbenchStatOption> SupportedStatOptionsExcluding(IReadOnlyList<StatId> currentStats, int keepIndex)
    {
        var used = new HashSet<StatId>();
        if (currentStats != null)
        {
            for (int i = 0; i < currentStats.Count; i++)
            {
                if (i != keepIndex)
                {
                    used.Add(currentStats[i]);
                }
            }
        }

        return ContentWorkbenchStatOptions.ForDomain(ContentWorkbenchDomain.Upgrades)
            .Where(option => !used.Contains(option.StatId))
            .ToArray();
    }

    private static IReadOnlyList<StatId> ReadSupportedStats(SerializedProperty list)
    {
        var stats = new List<StatId>();
        if (list == null || !list.isArray)
        {
            return stats;
        }

        for (int i = 0; i < list.arraySize; i++)
        {
            stats.Add((StatId)list.GetArrayElementAtIndex(i).intValue);
        }

        return stats;
    }
}

public enum ContentWorkbenchWeaponTab
{
    Identity = 0,
    Timing = 1,
    Projectile = 2,
    Orbit = 3,
    Targeting = 4,
    SpawnPose = 5,
    Visuals = 6,
    UpgradeTrack = 7,
    Diagnostics = 8
}
