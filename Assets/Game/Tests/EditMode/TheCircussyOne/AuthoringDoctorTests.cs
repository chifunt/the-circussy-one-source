using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using TheCircussyOne.Content;
using UnityEngine;

public sealed class AuthoringDoctorTests
{
    [Test]
    public void RunnerSortsResultsBySeverityCategoryTitleAndId()
    {
        var results = new[]
        {
            new AuthoringCheckResult("warning.b", "Beta", AuthoringCheckSeverity.Warning, "Beta Warning", "warning"),
            new AuthoringCheckResult("info.a", "Alpha", AuthoringCheckSeverity.Info, "Alpha Info", "info"),
            new AuthoringCheckResult("error.b", "Beta", AuthoringCheckSeverity.Error, "Beta Error", "error"),
            new AuthoringCheckResult("error.a", "Alpha", AuthoringCheckSeverity.Error, "Alpha Error", "error")
        };

        string[] sortedIds = AuthoringDoctorRunner.SortResults(results).Select(result => result.Id).ToArray();

        Assert.That(sortedIds, Is.EqualTo(new[] { "error.a", "error.b", "warning.b", "info.a" }));
    }

    [Test]
    public void MissingFakeAssetPathReportsErrorWithoutRunningFix()
    {
        const string path = "Assets/__DefinitelyMissingAuthoringDoctorAsset.asset";
        bool existedBefore = File.Exists(path);

        AuthoringCheckResult result = AuthoringDoctorRunner.EvaluateRequiredAssetForTests(path, "Test", false);

        Assert.That(existedBefore, Is.False);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Severity, Is.EqualTo(AuthoringCheckSeverity.Error));
        Assert.That(result.TargetPath, Is.EqualTo(path));
        Assert.That(result.HasFix, Is.False);
        Assert.That(File.Exists(path), Is.False);
    }

    [Test]
    public void RequiredConfigPathsComeFromCanonicalAssetPaths()
    {
        IReadOnlyList<string> paths = AuthoringDoctorRunner.RequiredConfigPaths;

        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.GameConfigPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.RunScheduleConfigPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.CannonWeaponPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.KnifeFanWeaponPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.SpotlightBoltWeaponPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.FireHoopWeaponPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.JugglingBallWeaponPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.WeaponCatalogPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.RubberSolesItemPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.SafetyPaddingItemPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.MagnetCharmItemPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.LuckyCoinItemPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.StudyNotesItemPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.TempoBraceletItemPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.PowderFlaskItemPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.SpringBootsItemPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.WideLensItemPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.LongFuseItemPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.ItemCatalogPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.OpenChestPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.LockedChestPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.PremiumChestPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.SpecialEnemyChestPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.ChestCatalogPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.SmallTicketStackPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.TicketRollPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.JackpotCachePath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.TicketDepositCatalogPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.BlueXpGemPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.GreenXpGemPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.RedXpGemPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.XpGemCatalogPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.TreatPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.HealthPickupCatalogPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.SnackBoxPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.SnackCartPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.HealingPropCatalogPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.WorldRewardPlacementCatalogPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.NormalEnemyPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.EnemyCatalogPath));
        foreach (string path in TheCircussyOneAssetPaths.WeaponUpgradePaths)
        {
            Assert.That(paths, Does.Contain(path));
        }

        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.UpgradeCatalogPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.FootworkTalentPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.StageStaminaTalentPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.ToughSkinTalentPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.CrowdFavoriteTalentPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.QuickHandsTalentPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.SharpEyeTalentPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.LongReachTalentPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.LuckyBreakTalentPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.BiggerPropsTalentPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.SpringboardTalentPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.JugglerTalentPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.CappaTalentPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.StrongmanTalentPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.AcrobatTalentPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.RingmasterTalentPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.HoopFlowTalentPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.EncoreBounceTalentPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.TalentCatalogPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.JugglerPerformerPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.CappaPerformerPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.StrongmanPerformerPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.AcrobatPerformerPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.RingmasterPerformerPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.SolaHoopDancerPerformerPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.BibiBallJugglerPerformerPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.PerformerCatalogPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.CameraConfigPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.GridVisualConfigPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.LightingVisualConfigPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.DamageFeedbackVisualConfigPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.ActorMotionVisualConfigPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.EnemySpawnVisualConfigPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.VfxVisualConfigPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.HudVisualConfigPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.XpGainCounterVisualConfigPath));
        Assert.That(paths, Does.Contain(TheCircussyOneAssetPaths.WorldInteractionPromptVisualConfigPath));
    }

    [Test]
    public void RunScheduleValidationReportsInvalidShowtimeContent()
    {
        var config = ScriptableObject.CreateInstance<TheCircussyOne.Config.RunScheduleConfig>();
        config.acts.Clear();
        config.acts.Add(new TheCircussyOne.Config.RunScheduleActDefinition
        {
            displayName = "Broken Act",
            durationSeconds = 60f,
            showtimeSeconds = new List<float> { 60f }
        });

        List<AuthoringCheckResult> results = AuthoringDoctorRunner.EvaluateRunScheduleForTests(config);

        Assert.That(results.Any(result => result.Severity == AuthoringCheckSeverity.Error), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("before the Headliner")), Is.True);

        Object.DestroyImmediate(config);
    }

    [Test]
    public void LayerChecksCanBeEvaluatedWithoutChangingProjectLayers()
    {
        var layerIndexes = new Dictionary<string, int>
        {
            { "Player", 8 },
            { "Enemy", 9 },
            { "Projectile", 10 },
            { "Pickup", 11 }
        };

        List<AuthoringCheckResult> results = AuthoringDoctorRunner.EvaluateLayerSetupForTests(
            layerName => layerIndexes.TryGetValue(layerName, out int layer) ? layer : -1,
            (a, b) => true);

        Assert.That(results, Is.Empty);
    }

    [Test]
    public void LayerChecksReportMissingLayerAndCollisionMatrixIssue()
    {
        List<AuthoringCheckResult> results = AuthoringDoctorRunner.EvaluateLayerSetupForTests(
            layerName => layerName == "Projectile" ? -1 : 8,
            (a, b) => false);

        Assert.That(results.Any(result => result.Id == "layers.missing.Projectile"), Is.True);
        Assert.That(results.Any(result => result.Id == "layers.player-projectile-collision"), Is.False);

        results = AuthoringDoctorRunner.EvaluateLayerSetupForTests(
            layerName => layerName == "Player" ? 8 : 9,
            (a, b) => false);

        Assert.That(results.Any(result => result.Id == "layers.player-projectile-collision"), Is.True);
    }

    [Test]
    public void OldCodenameScannerIgnoresEncodedNeedlesButFindsRealReferences()
    {
        string encodedNeedle = "\"Vampire\" + \"Survivors\" + \"Lite\"";
        string realNeedle = "Vampire" + "Survivors" + "Lite";

        Assert.That(AuthoringDoctorRunner.ContainsOldCodenameForTests(encodedNeedle), Is.False);
        Assert.That(AuthoringDoctorRunner.ContainsOldCodenameForTests(realNeedle), Is.True);
    }

    [Test]
    public void SafeFixesHaveExplicitLabelsAndDoNotRunAutomatically()
    {
        bool ran = false;
        var result = new AuthoringCheckResult(
            "test.fix",
            "Test",
            AuthoringCheckSeverity.Warning,
            "Fixable test result",
            "The test action should not run until ExecuteFix is called.",
            fixLabel: "Run Test Fix",
            fixAction: () => ran = true);

        Assert.That(result.HasFix, Is.True);
        Assert.That(result.FixLabel, Is.Not.Empty);
        Assert.That(ran, Is.False);

        result.ExecuteFix();

        Assert.That(ran, Is.True);
    }

    [Test]
    public void RunnerFixResultsUseExplicitLabels()
    {
        List<AuthoringCheckResult> results = AuthoringDoctorRunner.RunAllChecks();

        foreach (AuthoringCheckResult result in results.Where(result => result.FixAction != null))
        {
            Assert.That(result.FixLabel, Is.Not.Null.And.Not.Empty, result.Id);
        }
    }

    [Test]
    public void WeaponCatalogValidationIssuesAreReportedWithoutFixes()
    {
        var first = ScriptableObject.CreateInstance<WeaponDefinition>();
        first.weaponId = "juggling_ball";
        first.displayName = "Juggling Ball";
        first.tags = ContentTagSet.With(ContentTag.Projectile);

        var duplicate = ScriptableObject.CreateInstance<WeaponDefinition>();
        duplicate.weaponId = "juggling_ball";
        duplicate.displayName = "Duplicate Projectile";
        duplicate.tags = ContentTagSet.With(ContentTag.Projectile);

        var missing = ScriptableObject.CreateInstance<WeaponDefinition>();
        missing.weaponId = "";
        missing.displayName = "";
        missing.tags = ContentTagSet.With(ContentTag.Utility);

        List<AuthoringCheckResult> results = AuthoringDoctorRunner.EvaluateWeaponCatalogForTests(
            new[] { first, duplicate, missing, null },
            TheCircussyOneAssetPaths.WeaponCatalogPath);

        Assert.That(results.Any(result => result.Message.Contains("Duplicate content id")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("valid id")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("display name")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("null entry")), Is.True);
        Assert.That(results.All(result => result.FixAction == null), Is.True);
        Assert.That(results.All(result => result.TargetPath == TheCircussyOneAssetPaths.WeaponCatalogPath), Is.True);

        Object.DestroyImmediate(first);
        Object.DestroyImmediate(duplicate);
        Object.DestroyImmediate(missing);
    }

    [Test]
    public void WeaponUpgradeTrackValidationIssuesAreReportedWithoutFixes()
    {
        var cannon = ScriptableObject.CreateInstance<WeaponDefinition>();
        cannon.weaponId = "cannon";
        cannon.displayName = "Cannon";
        cannon.tags = ContentTagSet.With(ContentTag.Projectile);
        var spotlight = ScriptableObject.CreateInstance<WeaponDefinition>();
        spotlight.weaponId = "spotlight_bolt";
        spotlight.displayName = "Spotlight Bolt";
        spotlight.tags = ContentTagSet.With(ContentTag.Projectile);
        UpgradeDefinition wrongTrack = ScriptableObject.CreateInstance<UpgradeDefinition>();
        wrongTrack.ApplyWeaponStatUpgradeDefaults(
            spotlight,
            "charge",
            "Charge",
            "Wrong target.",
            Color.white,
            new UpgradeStatModifierDefinition(TheCircussyOne.Stats.StatId.ProjectileSpeedMultiplier, TheCircussyOne.Stats.StatModifierBucket.AdditivePercent, 0.1f));
        cannon.upgradeTrack.Add(wrongTrack);

        List<AuthoringCheckResult> results = AuthoringDoctorRunner.EvaluateWeaponCatalogForTests(
            new[] { cannon, spotlight },
            TheCircussyOneAssetPaths.WeaponCatalogPath);

        Assert.That(results.Any(result => result.Message.Contains("targets weapon id")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("different weapon definition")), Is.True);
        Assert.That(results.All(result => result.FixAction == null), Is.True);
        Assert.That(results.All(result => result.TargetPath == TheCircussyOneAssetPaths.WeaponCatalogPath), Is.True);

        Object.DestroyImmediate(wrongTrack);
        Object.DestroyImmediate(cannon);
        Object.DestroyImmediate(spotlight);
    }

    [Test]
    public void NonWeaponStatsOnWeaponUpgradeCardsAreReportedWithoutFixes()
    {
        var cannon = ScriptableObject.CreateInstance<WeaponDefinition>();
        cannon.weaponId = "cannon";
        cannon.displayName = "Cannon";
        cannon.tags = ContentTagSet.With(ContentTag.Projectile);
        UpgradeDefinition legacyDamage = ScriptableObject.CreateInstance<UpgradeDefinition>();
        legacyDamage.ApplyWeaponStatUpgradeDefaults(
            cannon,
            "calibration",
            "Calibration",
            "Legacy global stat.",
            Color.white,
            new UpgradeStatModifierDefinition(TheCircussyOne.Stats.StatId.GlobalDamageMultiplier, TheCircussyOne.Stats.StatModifierBucket.AdditivePercent, 0.1f));

        List<AuthoringCheckResult> results = AuthoringDoctorRunner.EvaluateUpgradeCatalogForTests(
            new[] { legacyDamage },
            TheCircussyOneAssetPaths.UpgradeCatalogPath);

        Assert.That(results.Any(result => result.Message.Contains("weapon upgrades can only use weapon-local stats")), Is.True);
        Assert.That(results.All(result => result.FixAction == null), Is.True);
        Assert.That(results.All(result => result.TargetPath == TheCircussyOneAssetPaths.UpgradeCatalogPath), Is.True);

        Object.DestroyImmediate(legacyDamage);
        Object.DestroyImmediate(cannon);
    }

    [Test]
    public void XpGemCatalogValidationIssuesAreReportedWithoutFixes()
    {
        var first = ScriptableObject.CreateInstance<XpGemDefinition>();
        first.gemId = "blue_xp_gem";
        first.displayName = "Blue XP Gem";
        first.xpAmount = 1;
        first.tags = ContentTagSet.With(ContentTag.Economy);

        var duplicate = ScriptableObject.CreateInstance<XpGemDefinition>();
        duplicate.gemId = "blue_xp_gem";
        duplicate.displayName = "Duplicate Gem";
        duplicate.xpAmount = 1;
        duplicate.tags = ContentTagSet.With(ContentTag.Economy);

        var invalid = ScriptableObject.CreateInstance<XpGemDefinition>();
        invalid.gemId = "";
        invalid.displayName = "";
        invalid.xpAmount = 0;
        invalid.tags = ContentTagSet.With(ContentTag.Economy);

        List<AuthoringCheckResult> results = AuthoringDoctorRunner.EvaluateXpGemCatalogForTests(
            new[] { first, duplicate, invalid, null },
            TheCircussyOneAssetPaths.XpGemCatalogPath);

        Assert.That(results.Any(result => result.Message.Contains("Duplicate content id")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("valid id")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("display name")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("positive XP")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("null entry")), Is.True);
        Assert.That(results.All(result => result.FixAction == null), Is.True);
        Assert.That(results.All(result => result.TargetPath == TheCircussyOneAssetPaths.XpGemCatalogPath), Is.True);

        Object.DestroyImmediate(first);
        Object.DestroyImmediate(duplicate);
        Object.DestroyImmediate(invalid);
    }

    [Test]
    public void ItemCatalogValidationIssuesAreReportedWithoutFixes()
    {
        var first = ScriptableObject.CreateInstance<ItemDefinition>();
        first.itemId = "rubber_soles";
        first.displayName = "Rubber Soles";
        first.tags = ContentTagSet.With(ContentTag.Movement);
        first.statModifiers.Add(new ItemStatModifierDefinition(TheCircussyOne.Stats.StatId.PlayerMoveSpeedMultiplier, TheCircussyOne.Stats.StatModifierBucket.AdditivePercent, 0.08f));

        var duplicate = ScriptableObject.CreateInstance<ItemDefinition>();
        duplicate.itemId = "rubber_soles";
        duplicate.displayName = "Duplicate Soles";
        duplicate.tags = ContentTagSet.With(ContentTag.Movement);
        duplicate.statModifiers.Add(new ItemStatModifierDefinition(TheCircussyOne.Stats.StatId.PlayerMoveSpeedMultiplier, TheCircussyOne.Stats.StatModifierBucket.AdditivePercent, 0.08f));

        var invalid = ScriptableObject.CreateInstance<ItemDefinition>();
        invalid.itemId = "";
        invalid.displayName = "";
        invalid.maxStacks = 0;
        invalid.tags = ContentTagSet.With(ContentTag.Utility);
        invalid.stackPolicy = ItemStackPolicy.StackWithCap;
        invalid.effectCapStacks = 2;

        List<AuthoringCheckResult> results = AuthoringDoctorRunner.EvaluateItemCatalogForTests(
            new[] { first, duplicate, invalid, null },
            TheCircussyOneAssetPaths.ItemCatalogPath);

        Assert.That(results.Any(result => result.Message.Contains("Duplicate content id")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("valid id")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("display name")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("at least one max stack")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("effect cap stacks greater than max stacks")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("no stat modifiers")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("null entry")), Is.True);
        Assert.That(results.All(result => result.FixAction == null), Is.True);
        Assert.That(results.All(result => result.TargetPath == TheCircussyOneAssetPaths.ItemCatalogPath), Is.True);

        Object.DestroyImmediate(first);
        Object.DestroyImmediate(duplicate);
        Object.DestroyImmediate(invalid);
    }

    [Test]
    public void TicketDepositCatalogValidationIssuesAreReportedWithoutFixes()
    {
        var first = ScriptableObject.CreateInstance<TicketDepositDefinition>();
        first.depositId = "ticket_roll";
        first.displayName = "Ticket Roll";
        first.holdSeconds = 1f;
        first.minTickets = 5;
        first.maxTickets = 8;

        var duplicate = ScriptableObject.CreateInstance<TicketDepositDefinition>();
        duplicate.depositId = "ticket_roll";
        duplicate.displayName = "Duplicate Ticket Roll";
        duplicate.holdSeconds = 1f;
        duplicate.minTickets = 5;
        duplicate.maxTickets = 8;

        var invalid = ScriptableObject.CreateInstance<TicketDepositDefinition>();
        invalid.depositId = "";
        invalid.displayName = "";
        invalid.holdSeconds = 0f;
        invalid.minTickets = -1;
        invalid.maxTickets = -2;
        invalid.payoutMode = TicketDepositPayoutMode.Chunked;
        invalid.payoutChunks = 0;

        List<AuthoringCheckResult> results = AuthoringDoctorRunner.EvaluateTicketDepositCatalogForTests(
            new[] { first, duplicate, invalid, null },
            TheCircussyOneAssetPaths.TicketDepositCatalogPath);

        Assert.That(results.Any(result => result.Message.Contains("Duplicate content id")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("valid id")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("display name")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("positive hold duration")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("negative minimum reward")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("at least one Ticket")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("null entry")), Is.True);
        Assert.That(results.All(result => result.FixAction == null), Is.True);
        Assert.That(results.All(result => result.TargetPath == TheCircussyOneAssetPaths.TicketDepositCatalogPath), Is.True);

        Object.DestroyImmediate(first);
        Object.DestroyImmediate(duplicate);
        Object.DestroyImmediate(invalid);
    }

    [Test]
    public void HealthPickupCatalogValidationIssuesAreReportedWithoutFixes()
    {
        var first = ScriptableObject.CreateInstance<HealthPickupDefinition>();
        first.pickupId = "treat";
        first.displayName = "Treat";
        first.healPercentOfMaxHealth = 0.2f;

        var duplicate = ScriptableObject.CreateInstance<HealthPickupDefinition>();
        duplicate.pickupId = "treat";
        duplicate.displayName = "Duplicate Treat";
        duplicate.healPercentOfMaxHealth = 0.2f;

        var invalid = ScriptableObject.CreateInstance<HealthPickupDefinition>();
        invalid.pickupId = "";
        invalid.displayName = "";
        invalid.healPercentOfMaxHealth = 0f;
        invalid.visualScale = -1f;
        invalid.emissionStrength = -1f;
        invalid.visualShape = (PickupVisualShape)999;

        List<AuthoringCheckResult> results = AuthoringDoctorRunner.EvaluateHealthPickupCatalogForTests(
            new[] { first, duplicate, invalid, null },
            TheCircussyOneAssetPaths.HealthPickupCatalogPath);

        Assert.That(results.Any(result => result.Message.Contains("Duplicate content id")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("valid id")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("display name")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("positive max HP percentage")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("non-positive visual scale")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("negative emission strength")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("invalid visual shape")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("null entry")), Is.True);
        Assert.That(results.All(result => result.FixAction == null), Is.True);
        Assert.That(results.All(result => result.TargetPath == TheCircussyOneAssetPaths.HealthPickupCatalogPath), Is.True);

        Object.DestroyImmediate(first);
        Object.DestroyImmediate(duplicate);
        Object.DestroyImmediate(invalid);
    }

    [Test]
    public void HealingPropCatalogValidationIssuesAreReportedWithoutFixes()
    {
        HealthPickupDefinition pickup = TheCircussyOneTestObjects.CreateHealthPickupDefinition("treat", "Treat");
        var first = ScriptableObject.CreateInstance<HealingPropDefinition>();
        first.propId = "snack_box";
        first.displayName = "Snack Box";
        first.holdSeconds = 1f;
        first.promptText = "Grab a Snack";
        first.healthPickup = pickup;
        first.minPickupCount = 1;
        first.maxPickupCount = 1;

        var duplicate = ScriptableObject.CreateInstance<HealingPropDefinition>();
        duplicate.propId = "snack_box";
        duplicate.displayName = "Duplicate Snack Box";
        duplicate.holdSeconds = 1f;
        duplicate.promptText = "Grab a Snack";
        duplicate.healthPickup = pickup;
        duplicate.minPickupCount = 1;
        duplicate.maxPickupCount = 1;

        var invalid = ScriptableObject.CreateInstance<HealingPropDefinition>();
        invalid.propId = "";
        invalid.displayName = "";
        invalid.holdSeconds = 0f;
        invalid.promptText = "";
        invalid.healthPickup = null;
        invalid.minPickupCount = 0;
        invalid.maxPickupCount = 0;
        invalid.visualScale = -1f;
        invalid.emissionStrength = -1f;
        invalid.targetOutlineThickness = -1f;

        List<AuthoringCheckResult> results = AuthoringDoctorRunner.EvaluateHealingPropCatalogForTests(
            new[] { first, duplicate, invalid, null },
            TheCircussyOneAssetPaths.HealingPropCatalogPath);

        Assert.That(results.Any(result => result.Message.Contains("Duplicate content id")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("valid id")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("display name")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("positive hold duration")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("health pickup reward")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("at least one pickup")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("prompt text")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("non-positive visual scale")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("negative emission strength")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("negative target outline thickness")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("null entry")), Is.True);
        Assert.That(results.All(result => result.FixAction == null), Is.True);
        Assert.That(results.All(result => result.TargetPath == TheCircussyOneAssetPaths.HealingPropCatalogPath), Is.True);

        Object.DestroyImmediate(first);
        Object.DestroyImmediate(duplicate);
        Object.DestroyImmediate(invalid);
        Object.DestroyImmediate(pickup);
    }

    [Test]
    public void ChestCatalogValidationIssuesAreReportedWithoutFixes()
    {
        ChestDefinition first = TheCircussyOneTestObjects.CreateChestDefinition("locked_chest", "Locked Chest");
        ChestDefinition duplicate = TheCircussyOneTestObjects.CreateChestDefinition("locked_chest", "Duplicate Chest");
        ChestDefinition invalid = TheCircussyOneTestObjects.CreateChestDefinition("", "");
        invalid.chestId = "";
        invalid.displayName = "";
        invalid.holdSeconds = 0f;
        invalid.ticketCost = -1;
        invalid.itemRarityWeights = default;

        List<AuthoringCheckResult> results = AuthoringDoctorRunner.EvaluateChestCatalogForTests(
            new[] { first, duplicate, invalid, null },
            TheCircussyOneAssetPaths.ChestCatalogPath);

        Assert.That(results.Any(result => result.Message.Contains("Duplicate content id")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("valid id")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("display name")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("positive hold duration")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("negative Ticket cost")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("positive item rarity weights")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("null entry")), Is.True);
        Assert.That(results.All(result => result.FixAction == null), Is.True);
        Assert.That(results.All(result => result.TargetPath == TheCircussyOneAssetPaths.ChestCatalogPath), Is.True);

        Object.DestroyImmediate(first);
        Object.DestroyImmediate(duplicate);
        Object.DestroyImmediate(invalid);
    }

    [Test]
    public void EnemyCatalogValidationIssuesAreReportedWithoutFixes()
    {
        var first = ScriptableObject.CreateInstance<EnemyDefinition>();
        first.enemyId = "normal_enemy";
        first.displayName = "Normal Enemy";
        first.tags = ContentTagSet.With(ContentTag.Swarm);

        var duplicate = ScriptableObject.CreateInstance<EnemyDefinition>();
        duplicate.enemyId = "normal_enemy";
        duplicate.displayName = "Duplicate Enemy";
        duplicate.tags = ContentTagSet.With(ContentTag.Swarm);

        var invalid = ScriptableObject.CreateInstance<EnemyDefinition>();
        invalid.enemyId = "";
        invalid.displayName = "";
        invalid.baseHealth = 0;
        invalid.baseMoveSpeed = 0f;
        invalid.contactDamage = 0;
        invalid.behaviorType = (EnemyBehaviorType)999;
        invalid.spawnWeight = 0f;
        invalid.body.hurtboxRadius = 0f;
        invalid.tags = ContentTagSet.With(ContentTag.Swarm);

        List<AuthoringCheckResult> results = AuthoringDoctorRunner.EvaluateEnemyCatalogForTests(
            new[] { first, duplicate, invalid, null },
            TheCircussyOneAssetPaths.EnemyCatalogPath);

        Assert.That(results.Any(result => result.Message.Contains("Duplicate content id")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("valid id")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("display name")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("positive base health")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("positive move speed")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("positive contact damage")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("invalid behavior type")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("positive spawn weight")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("positive hurtbox radius")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("null entry")), Is.True);
        Assert.That(results.All(result => result.FixAction == null), Is.True);
        Assert.That(results.All(result => result.TargetPath == TheCircussyOneAssetPaths.EnemyCatalogPath), Is.True);

        Object.DestroyImmediate(first);
        Object.DestroyImmediate(duplicate);
        Object.DestroyImmediate(invalid);
    }

    [Test]
    public void TalentCatalogValidationIssuesAreReportedWithoutFixes()
    {
        var first = ScriptableObject.CreateInstance<TalentDefinition>();
        first.talentId = "footwork";
        first.displayName = "Footwork";
        first.shortDescription = "Rarity-scaled +5% movement speed.";
        first.tags = ContentTagSet.With(ContentTag.Movement);
        first.statModifiers.Add(new UpgradeStatModifierDefinition(TheCircussyOne.Stats.StatId.PlayerMoveSpeedMultiplier, TheCircussyOne.Stats.StatModifierBucket.AdditivePercent, 0.1f));

        var duplicate = ScriptableObject.CreateInstance<TalentDefinition>();
        duplicate.talentId = "footwork";
        duplicate.displayName = "Duplicate Footwork";
        duplicate.tags = ContentTagSet.With(ContentTag.Movement);
        duplicate.statModifiers.Add(new UpgradeStatModifierDefinition(TheCircussyOne.Stats.StatId.PlayerMoveSpeedMultiplier, TheCircussyOne.Stats.StatModifierBucket.AdditivePercent, 0.1f));

        var invalid = ScriptableObject.CreateInstance<TalentDefinition>();
        invalid.talentId = "";
        invalid.displayName = "";
        invalid.tags = ContentTagSet.With(ContentTag.Utility);
        invalid.statModifiers.Clear();

        var missingCharacter = ScriptableObject.CreateInstance<TalentDefinition>();
        missingCharacter.talentId = "performer_specific";
        missingCharacter.displayName = "Performer Specific";
        missingCharacter.tags = ContentTagSet.With(ContentTag.Utility);
        missingCharacter.poolKind = TalentPoolKind.PerformerSpecific;
        missingCharacter.performerId = "";
        missingCharacter.statModifiers.Add(new UpgradeStatModifierDefinition(TheCircussyOne.Stats.StatId.PlayerArmor, TheCircussyOne.Stats.StatModifierBucket.Flat, 1f));

        var duplicateAccess = ScriptableObject.CreateInstance<TalentDefinition>();
        duplicateAccess.talentId = "duplicate_access";
        duplicateAccess.displayName = "Duplicate Access";
        duplicateAccess.tags = ContentTagSet.With(ContentTag.Utility);
        duplicateAccess.statModifiers.Add(new UpgradeStatModifierDefinition(TheCircussyOne.Stats.StatId.PlayerArmor, TheCircussyOne.Stats.StatModifierBucket.Flat, 1f));
        duplicateAccess.sharedPerformerAccess.Add(new TalentPerformerAccess { enabled = true, performerId = "acrobat" });
        duplicateAccess.sharedPerformerAccess.Add(new TalentPerformerAccess { enabled = false, performerId = "acrobat" });

        var invalidAccess = ScriptableObject.CreateInstance<TalentDefinition>();
        invalidAccess.talentId = "invalid_access";
        invalidAccess.displayName = "Invalid Access";
        invalidAccess.tags = ContentTagSet.With(ContentTag.Utility);
        invalidAccess.statModifiers.Add(new UpgradeStatModifierDefinition(TheCircussyOne.Stats.StatId.PlayerArmor, TheCircussyOne.Stats.StatModifierBucket.Flat, 1f));
        invalidAccess.sharedPerformerAccess.Add(new TalentPerformerAccess { enabled = true, performerId = "" });

        List<AuthoringCheckResult> results = AuthoringDoctorRunner.EvaluateTalentCatalogForTests(
            new[] { first, duplicate, invalid, missingCharacter, duplicateAccess, invalidAccess, null },
            TheCircussyOneAssetPaths.TalentCatalogPath);

        Assert.That(results.Any(result => result.Message.Contains("Duplicate content id")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("valid id")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("display name")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("no stat modifiers")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("no performer id")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("duplicate shared performer access")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("invalid shared performer access")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("exact numbers belong in the preview line")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("null entry")), Is.True);
        Assert.That(results.All(result => result.FixAction == null), Is.True);
        Assert.That(results.All(result => result.TargetPath == TheCircussyOneAssetPaths.TalentCatalogPath), Is.True);

        Object.DestroyImmediate(first);
        Object.DestroyImmediate(duplicate);
        Object.DestroyImmediate(invalid);
        Object.DestroyImmediate(missingCharacter);
        Object.DestroyImmediate(duplicateAccess);
        Object.DestroyImmediate(invalidAccess);
    }

    [Test]
    public void PerformerCatalogValidationIssuesAreReportedWithoutFixes()
    {
        var weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
        weapon.weaponId = "juggling_ball";
        weapon.displayName = "Juggling Ball";
        weapon.tags = ContentTagSet.With(ContentTag.Projectile);

        var first = ScriptableObject.CreateInstance<PerformerDefinition>();
        first.performerId = "first";
        first.displayName = "First";
        first.tags = ContentTagSet.With(ContentTag.Utility);
        first.startingWeapon = weapon;

        var duplicate = ScriptableObject.CreateInstance<PerformerDefinition>();
        duplicate.performerId = "first";
        duplicate.displayName = "Duplicate First";
        duplicate.tags = ContentTagSet.With(ContentTag.Utility);
        duplicate.startingWeapon = weapon;

        var invalid = ScriptableObject.CreateInstance<PerformerDefinition>();
        invalid.performerId = "";
        invalid.displayName = "";
        invalid.tags = ContentTagSet.With(ContentTag.Utility);
        invalid.startingWeapon = null;
        invalid.passiveKind = (PerformerPassiveKind)999;

        List<AuthoringCheckResult> results = AuthoringDoctorRunner.EvaluatePerformerCatalogForTests(
            new[] { first, duplicate, invalid, null },
            invalid,
            TheCircussyOneAssetPaths.PerformerCatalogPath);

        Assert.That(results.Any(result => result.Message.Contains("Duplicate content id")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("valid id")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("display name")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("no starting weapon")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("invalid passive kind")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("null entry")), Is.True);
        Assert.That(results.All(result => result.FixAction == null), Is.True);
        Assert.That(results.All(result => result.TargetPath == TheCircussyOneAssetPaths.PerformerCatalogPath), Is.True);

        Object.DestroyImmediate(weapon);
        Object.DestroyImmediate(first);
        Object.DestroyImmediate(duplicate);
        Object.DestroyImmediate(invalid);
    }

    [Test]
    public void UpgradeCatalogValidationIssuesAreReportedWithoutFixes()
    {
        var first = ScriptableObject.CreateInstance<UpgradeDefinition>();
        first.upgradeId = "move_speed";
        first.displayName = "Move Speed";
        first.shortDescription = "Rarity-scaled +5% movement speed.";
        first.tags = ContentTagSet.With(ContentTag.Movement);
        first.statModifiers.Add(new UpgradeStatModifierDefinition(TheCircussyOne.Stats.StatId.PlayerMoveSpeedMultiplier, TheCircussyOne.Stats.StatModifierBucket.AdditivePercent, 0.1f));

        var duplicate = ScriptableObject.CreateInstance<UpgradeDefinition>();
        duplicate.upgradeId = "move_speed";
        duplicate.displayName = "Duplicate Move Speed";
        duplicate.tags = ContentTagSet.With(ContentTag.Movement);
        duplicate.statModifiers.Add(new UpgradeStatModifierDefinition(TheCircussyOne.Stats.StatId.PlayerMoveSpeedMultiplier, TheCircussyOne.Stats.StatModifierBucket.AdditivePercent, 0.1f));

        var invalid = ScriptableObject.CreateInstance<UpgradeDefinition>();
        invalid.upgradeId = "";
        invalid.displayName = "";
        invalid.tags = ContentTagSet.With(ContentTag.Utility);
        invalid.maxLevel = 0;

        var missingWeapon = ScriptableObject.CreateInstance<UpgradeDefinition>();
        missingWeapon.upgradeId = "juggling_ball_damage";
        missingWeapon.displayName = "Juggling Ball Damage";
        missingWeapon.tags = ContentTagSet.With(ContentTag.Projectile);
        missingWeapon.weaponId = "";
        missingWeapon.weaponDefinition = null;
        missingWeapon.statModifiers.Add(new UpgradeStatModifierDefinition(TheCircussyOne.Stats.StatId.WeaponFlatDamage, TheCircussyOne.Stats.StatModifierBucket.Flat, 1f));

        List<AuthoringCheckResult> results = AuthoringDoctorRunner.EvaluateUpgradeCatalogForTests(
            new[] { first, duplicate, invalid, missingWeapon, null },
            TheCircussyOneAssetPaths.UpgradeCatalogPath);

        Assert.That(results.Any(result => result.Message.Contains("Duplicate content id")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("valid id")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("display name")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("invalid max level")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("no stat modifiers")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("no weapon id")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("exact numbers belong in the preview line")), Is.True);
        Assert.That(results.Any(result => result.Message.Contains("null entry")), Is.True);
        Assert.That(results.All(result => result.FixAction == null), Is.True);
        Assert.That(results.All(result => result.TargetPath == TheCircussyOneAssetPaths.UpgradeCatalogPath), Is.True);

        Object.DestroyImmediate(first);
        Object.DestroyImmediate(duplicate);
        Object.DestroyImmediate(invalid);
        Object.DestroyImmediate(missingWeapon);
    }

    [Test]
    public void ConfigHubAndDoctorEditorTypesCompile()
    {
        Assert.That(typeof(TheCircussyOneConfigHub), Is.Not.Null);
        Assert.That(typeof(TheCircussyOneAuthoringDoctorWindow), Is.Not.Null);
    }
}
