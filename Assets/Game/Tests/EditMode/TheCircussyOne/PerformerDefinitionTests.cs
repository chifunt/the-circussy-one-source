using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TheCircussyOne.Config;
using TheCircussyOne.Content;
using TheCircussyOne.Rules;
using TheCircussyOne.Runtime;
using TheCircussyOne.Stats;
using TheCircussyOne.Visuals;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public sealed class PerformerDefinitionTests
{
    private readonly List<Object> created = new();

    [TearDown]
    public void TearDown()
    {
        TheCircussyOneTestObjects.DestroyAll();
        for (int i = created.Count - 1; i >= 0; i--)
        {
            TheCircussyOneTestObjects.Destroy(created[i]);
        }

        created.Clear();
    }

    [Test]
    public void StarterPerformerDefaultsCreateUnlockedRosterWithBaselineDevsampleSam()
    {
        PerformerDefinition juggler = CreateStarterPerformer(StarterPerformerDefaults.JugglerId, out WeaponDefinition starter, out TalentDefinition jugglerTalent);
        PerformerDefinition cappa = CreateStarterPerformer(StarterPerformerDefaults.DevsampleCappaId, out WeaponDefinition cappaStarter, out TalentDefinition cappaTalent);
        PerformerDefinition strongman = CreateStarterPerformer(StarterPerformerDefaults.StrongmanId, out WeaponDefinition cannon, out TalentDefinition strongmanTalent);
        PerformerDefinition acrobat = CreateStarterPerformer(StarterPerformerDefaults.AcrobatId, out WeaponDefinition knifeFan, out TalentDefinition acrobatTalent);
        PerformerDefinition ringmaster = CreateStarterPerformer(StarterPerformerDefaults.RingmasterId, out WeaponDefinition spotlightBolt, out TalentDefinition ringmasterTalent);
        PerformerDefinition sola = CreateStarterPerformer(StarterPerformerDefaults.SolaHoopDancerId, out WeaponDefinition fireHoop, out TalentDefinition solaTalent);
        PerformerDefinition bibi = CreateStarterPerformer(StarterPerformerDefaults.BibiBallJugglerId, out WeaponDefinition jugglingBall, out TalentDefinition bibiTalent);

        Assert.That(juggler.Id, Is.EqualTo(StarterPerformerDefaults.JugglerId));
        Assert.That(juggler.DisplayName, Is.EqualTo("Devsample Sam"));
        Assert.That(juggler.PerformerTitle, Is.EqualTo("The Dev Sample"));
        Assert.That(juggler.unlocked, Is.True);
        Assert.That(juggler.startingWeapon, Is.SameAs(starter));
        Assert.That(juggler.baseStatModifiers, Is.Empty);
        Assert.That(juggler.passiveStatModifiers, Is.Empty);
        Assert.That(juggler.passiveKind, Is.EqualTo(PerformerPassiveKind.None));
        Assert.That(juggler.uniqueTalents, Is.Empty);
        Assert.That(jugglerTalent.AllowsPerformer(juggler), Is.True);
        Assert.That(cappa.Id, Is.EqualTo("devsample_cappa"));
        Assert.That(cappa.DisplayName, Is.EqualTo("Devsample Cappa"));
        Assert.That(cappa.PerformerTitle, Is.EqualTo("The Capsule Sample"));
        Assert.That(cappa.unlocked, Is.True);
        Assert.That(cappa.startingWeapon, Is.SameAs(cappaStarter));
        Assert.That(cappa.baseStatModifiers, Is.Empty);
        Assert.That(cappa.passiveStatModifiers, Is.Empty);
        Assert.That(cappa.passiveKind, Is.EqualTo(PerformerPassiveKind.None));
        Assert.That(cappa.uniqueTalents, Is.Empty);
        Assert.That(cappaTalent.AllowsPerformer(cappa), Is.True);
        Assert.That(cappaTalent.AllowsPerformer(juggler), Is.False);

        AssertPerformerModifier(strongman.baseStatModifiers, StatId.PlayerArmor, StatModifierBucket.Flat, 5f);
        Assert.That(strongman.DisplayName, Is.EqualTo("Bruno"));
        Assert.That(strongman.PerformerTitle, Is.EqualTo("The Cannon Strongman"));
        AssertPerformerModifier(strongman.passiveStatModifiers, StatId.PlayerMoveSpeedMultiplier, StatModifierBucket.AdditivePercent, -0.05f);
        Assert.That(strongman.startingWeapon, Is.SameAs(cannon));

        AssertPerformerModifier(acrobat.baseStatModifiers, StatId.PlayerMoveSpeedMultiplier, StatModifierBucket.AdditivePercent, 0.1f);
        Assert.That(acrobat.DisplayName, Is.EqualTo("Mira"));
        Assert.That(acrobat.PerformerTitle, Is.EqualTo("The Knife Acrobat"));
        AssertPerformerModifier(acrobat.passiveStatModifiers, StatId.PlayerMaxHealth, StatModifierBucket.Flat, -10f);
        Assert.That(acrobat.startingWeapon, Is.SameAs(knifeFan));

        AssertPerformerModifier(ringmaster.baseStatModifiers, StatId.Luck, StatModifierBucket.Flat, 15f);
        Assert.That(ringmaster.DisplayName, Is.EqualTo("Vox"));
        Assert.That(ringmaster.PerformerTitle, Is.EqualTo("The Spotlight Ringmaster"));
        AssertPerformerModifier(ringmaster.passiveStatModifiers, StatId.XpGainMultiplier, StatModifierBucket.AdditivePercent, 0.05f);
        Assert.That(ringmaster.startingWeapon, Is.SameAs(spotlightBolt));

        AssertPerformerModifier(sola.baseStatModifiers, StatId.ProjectileAreaMultiplier, StatModifierBucket.AdditivePercent, 0.05f);
        Assert.That(sola.DisplayName, Is.EqualTo("Sola"));
        Assert.That(sola.PerformerTitle, Is.EqualTo("The Fire Hoop Dancer"));
        AssertPerformerModifier(sola.passiveStatModifiers, StatId.PlayerMaxHealth, StatModifierBucket.Flat, -5f);
        Assert.That(sola.startingWeapon, Is.SameAs(fireHoop));

        AssertPerformerModifier(bibi.baseStatModifiers, StatId.Bounce, StatModifierBucket.Flat, 1f);
        Assert.That(bibi.DisplayName, Is.EqualTo("Bibi"));
        Assert.That(bibi.PerformerTitle, Is.EqualTo("The Ball Juggler"));
        AssertPerformerModifier(bibi.passiveStatModifiers, StatId.GlobalWeaponHaste, StatModifierBucket.AdditivePercent, -0.05f);
        Assert.That(bibi.startingWeapon, Is.SameAs(jugglingBall));

        Assert.That(new[] { jugglerTalent, cappaTalent, strongmanTalent, acrobatTalent, ringmasterTalent, solaTalent, bibiTalent },
            Has.All.Matches<TalentDefinition>(talent => talent.poolKind == TalentPoolKind.PerformerSpecific));
        Assert.That(cappaTalent.performerId, Is.EqualTo(cappa.Id));
        Assert.That(strongmanTalent.performerId, Is.EqualTo(strongman.Id));
        Assert.That(acrobatTalent.performerId, Is.EqualTo(acrobat.Id));
        Assert.That(ringmasterTalent.performerId, Is.EqualTo(ringmaster.Id));
        Assert.That(solaTalent.performerId, Is.EqualTo(sola.Id));
        Assert.That(bibiTalent.performerId, Is.EqualTo(bibi.Id));
    }

    [Test]
    public void StarterTalentDefaultsExposeSharedAndPerformerSpecificTalentCounts()
    {
        Assert.That(StarterTalentDefaults.AllIds, Has.Length.EqualTo(12));
        Assert.That(StarterTalentDefaults.AllIds.Distinct().Count(), Is.EqualTo(12));
        Assert.That(StarterPerformerDefaults.PerformerIds, Has.Length.EqualTo(7));
        Assert.That(StarterPerformerDefaults.TalentIds, Has.Length.EqualTo(35));
        Assert.That(StarterPerformerDefaults.TalentIds.Distinct().Count(), Is.EqualTo(35));
        foreach (string performerId in StarterPerformerDefaults.PerformerIds)
        {
            Assert.That(StarterPerformerDefaults.TalentIdsForPerformer(performerId), Has.Length.EqualTo(5));
        }
    }

    [Test]
    public void PerformerCatalogValidationCatchesInvalidPerformerContent()
    {
        WeaponDefinition starter = Track(CreateWeapon("juggling_ball", "Juggling Ball"));
        PerformerDefinition first = Track(TheCircussyOneTestObjects.CreatePerformerDefinition("first", "First", starter));
        PerformerDefinition duplicate = Track(TheCircussyOneTestObjects.CreatePerformerDefinition("first", "Duplicate", starter));
        PerformerDefinition missing = Track(TheCircussyOneTestObjects.CreatePerformerDefinition("", "", starter));
        missing.performerId = "";
        missing.displayName = "";
        PerformerDefinition missingWeapon = Track(TheCircussyOneTestObjects.CreatePerformerDefinition("missing_weapon", "Missing Weapon", null));
        PerformerDefinition invalidPassive = Track(TheCircussyOneTestObjects.CreatePerformerDefinition("invalid_passive", "Invalid Passive", starter));
        invalidPassive.passiveKind = (PerformerPassiveKind)999;
        PerformerDefinition weaponLocalStat = Track(TheCircussyOneTestObjects.CreatePerformerDefinition("weapon_local_stat", "Weapon Local Stat", starter));
        weaponLocalStat.baseStatModifiers.Clear();
        weaponLocalStat.baseStatModifiers.Add(new UpgradeStatModifierDefinition(StatId.WeaponFlatDamage, StatModifierBucket.Flat, 1f));
        PerformerDefinition lockedFallback = Track(TheCircussyOneTestObjects.CreatePerformerDefinition("locked_fallback", "Locked Fallback", starter));
        lockedFallback.unlocked = false;
        PerformerCatalog catalog = Track(TheCircussyOneTestObjects.CreatePerformerCatalog(
            lockedFallback,
            first,
            duplicate,
            missing,
            missingWeapon,
            invalidPassive,
            weaponLocalStat,
            lockedFallback,
            null));

        List<ContentValidationIssue> issues = catalog.ValidateContent();

        Assert.That(issues.Any(issue => issue.Code == "catalog.null-entry"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "content.duplicate-id"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "content.invalid-id"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "content.missing-display-name"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "performer.fallback-locked"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "performer.missing-starting-weapon"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "performer.invalid-passive-kind"), Is.True);
        Assert.That(issues.Any(issue => issue.Code == "performer.invalid-base-stat" && issue.Message.Contains("weapon-local stat 'Weapon Damage'")), Is.True);
    }

    [Test]
    public void PerformerCatalogAllowsDevsamplesToShareJugglingBall()
    {
        WeaponDefinition jugglingBall = Track(CreateWeapon("juggling_ball", "Juggling Ball"));
        PerformerDefinition sam = Track(TheCircussyOneTestObjects.CreatePerformerDefinition("devsample_sam", "Devsample Sam", jugglingBall));
        PerformerDefinition cappa = Track(TheCircussyOneTestObjects.CreatePerformerDefinition("devsample_cappa", "Devsample Cappa", jugglingBall));
        PerformerCatalog catalog = Track(TheCircussyOneTestObjects.CreatePerformerCatalog(sam, cappa));

        List<ContentValidationIssue> issues = catalog.ValidateContent();

        Assert.That(issues.Any(issue => issue.Code == "performer.missing-starting-weapon"), Is.False);
        Assert.That(sam.startingWeapon.Id, Is.EqualTo("juggling_ball"));
        Assert.That(cappa.startingWeapon.Id, Is.EqualTo("juggling_ball"));
    }

    [Test]
    public void DevsampleSamAssetUsesAnimancerHumanoidModelWhenPackageIsAvailable()
    {
        PerformerDefinition sam = AssetDatabase.LoadAssetAtPath<PerformerDefinition>(TheCircussyOneAssetPaths.JugglerPerformerPath);
        GameObject humanoid = AssetDatabase.LoadAssetAtPath<GameObject>(TheCircussyOneAssetPaths.AnimancerHumanoidPrefabPath);
        AnimationClip idle = AssetDatabase.LoadAssetAtPath<AnimationClip>(TheCircussyOneAssetPaths.AnimancerHumanoidIdleClipPath);
        AnimationClip run = AssetDatabase.LoadAssetAtPath<AnimationClip>(TheCircussyOneAssetPaths.AnimancerHumanoidRunClipPath);

        Assert.That(sam, Is.Not.Null);
        Assert.That(humanoid, Is.Not.Null);
        Assert.That(idle, Is.Not.Null);
        Assert.That(run, Is.Not.Null);
        Assert.That(AssetDatabase.GetAssetPath(sam.worldPrefab), Is.EqualTo(TheCircussyOneAssetPaths.AnimancerHumanoidPrefabPath));
        Assert.That(AssetDatabase.GetAssetPath(sam.animation.idle), Is.EqualTo(TheCircussyOneAssetPaths.AnimancerHumanoidIdleClipPath));
        Assert.That(AssetDatabase.GetAssetPath(sam.animation.run), Is.EqualTo(TheCircussyOneAssetPaths.AnimancerHumanoidRunClipPath));
        Assert.That(sam.animation.runSpeed, Is.EqualTo(2f).Within(0.001f));
        Assert.That(sam.animation.scaleRunSpeedWithMovement, Is.True);
        Assert.That(sam.animation.runThreshold01, Is.EqualTo(0.2f).Within(0.001f));
        Assert.That(sam.animation.runExitThreshold01, Is.EqualTo(0.06f).Within(0.001f));
        Assert.That(sam.modelTransform.localScale.x, Is.GreaterThan(1f));
        Assert.That(sam.modelTransform.localScale, Is.EqualTo(Vector3.one * sam.modelTransform.localScale.x));
    }

    [Test]
    public void DevsampleSamAssetModelCanBeAppliedToPlayerView()
    {
        PerformerDefinition sam = AssetDatabase.LoadAssetAtPath<PerformerDefinition>(TheCircussyOneAssetPaths.JugglerPerformerPath);
        PlayerView player = TheCircussyOneTestObjects.CreatePlayer(TheCircussyOneTestObjects.CreateConfig());

        Assert.That(sam, Is.Not.Null);
        Assert.That(sam.worldPrefab, Is.Not.Null);
        Assert.DoesNotThrow(() => player.ApplyPerformerVisuals(sam));
        Assert.That(player.RuntimeModelInstance, Is.Not.Null);
    }

    [Test]
    public void PerformerCatalogWarnsWhenModelHasNoPlayableClips()
    {
        WeaponDefinition starter = Track(CreateWeapon("juggling_ball", "Juggling Ball"));
        PerformerDefinition performer = Track(TheCircussyOneTestObjects.CreatePerformerDefinition("model_no_clips", "Model No Clips", starter));
        performer.worldPrefab = TheCircussyOneTestObjects.CreateRuntimeModelPrefab("Model Without Clips");
        performer.animation = new PerformerAnimationProfile();
        PerformerCatalog catalog = Track(TheCircussyOneTestObjects.CreatePerformerCatalog(performer, performer));

        List<ContentValidationIssue> issues = catalog.ValidateContent();

        Assert.That(issues.Any(issue => issue.Code == "performer.model-missing-animation"), Is.True);
    }

    [Test]
    public void ActorAnimationAuthoringRulesReportPerformerClipAndFallbackDiagnostics()
    {
        WeaponDefinition starter = Track(CreateWeapon("juggling_ball", "Juggling Ball"));
        PerformerDefinition performer = Track(TheCircussyOneTestObjects.CreatePerformerDefinition("animated_performer", "Animated Performer", starter));
        performer.worldPrefab = TheCircussyOneTestObjects.CreateRuntimeModelPrefab("Animated Performer Model");
        performer.animation.idle = Track(new AnimationClip { name = "Idle Preview Clip" });
        performer.animation.runSpeed = 0f;

        List<ActorAnimationAuthoringDiagnostic> diagnostics = ActorAnimationAuthoringRules.PerformerDiagnostics(performer);

        Assert.That(diagnostics.Any(diagnostic => diagnostic.Code == "performer.missing-jump-clip" && diagnostic.Severity == ActorAnimationDiagnosticSeverity.Info), Is.True);
        Assert.That(diagnostics.Any(diagnostic => diagnostic.Code == "performer.missing-land-clip" && diagnostic.Severity == ActorAnimationDiagnosticSeverity.Info), Is.True);
        Assert.That(diagnostics.Any(diagnostic => diagnostic.Code == "performer.idle-clip" && diagnostic.Message.Contains("Idle Preview Clip")), Is.True);
        Assert.That(diagnostics.Any(diagnostic => diagnostic.Code == "performer.invalid-animation-speed" && diagnostic.Severity == ActorAnimationDiagnosticSeverity.Error), Is.True);
    }

    [Test]
    public void ActorAnimationAuthoringRulesWarnWhenPerformerModelTransformIsInvalid()
    {
        WeaponDefinition starter = Track(CreateWeapon("juggling_ball", "Juggling Ball"));
        PerformerDefinition performer = Track(TheCircussyOneTestObjects.CreatePerformerDefinition("bad_transform", "Bad Transform", starter));
        performer.worldPrefab = TheCircussyOneTestObjects.CreateRuntimeModelPrefab("Bad Transform Model");
        performer.modelTransform.localScale = Vector3.zero;

        List<ActorAnimationAuthoringDiagnostic> diagnostics = ActorAnimationAuthoringRules.PerformerDiagnostics(performer);

        Assert.That(diagnostics.Any(diagnostic => diagnostic.Code == "actor.invalid-transform-scale" && diagnostic.Severity == ActorAnimationDiagnosticSeverity.Error), Is.True);
    }

    [Test]
    public void PerformerRunComposerAppliesStatsLoadoutThenResetsHealth()
    {
        GameConfig config = Track(TheCircussyOneTestObjects.CreateConfig());
        WeaponDefinition knifeFan = Track(CreateWeapon("knife_fan", "Knife Fan"));
        PerformerDefinition acrobat = Track(TheCircussyOneTestObjects.CreatePerformerDefinition(
            "acrobat",
            "Acrobat",
            knifeFan,
            passiveKind: PerformerPassiveKind.StaticStatModifiers,
            baseModifiers: new[]
            {
                new UpgradeStatModifierDefinition(StatId.PlayerMoveSpeedMultiplier, StatModifierBucket.AdditivePercent, 0.1f)
            },
            passiveModifiers: new[]
            {
                new UpgradeStatModifierDefinition(StatId.PlayerMaxHealth, StatModifierBucket.Flat, -10f)
            }));
        var stats = new RunStats(config);
        var state = new GameState(config, stats);
        var loadout = new WeaponLoadout(TheCircussyOneTestObjects.CreateWeaponCatalog(), maxWeapons: 4);
        var runState = new PerformerRunState();
        var composer = new PerformerRunComposer(runState, stats, loadout, state);

        bool composed = composer.ComposeAndStart(acrobat);

        Assert.That(composed, Is.True);
        Assert.That(runState.SelectedPerformer, Is.SameAs(acrobat));
        Assert.That(loadout.WeaponCount, Is.EqualTo(1));
        Assert.That(loadout.OwnsWeapon("knife_fan"), Is.True);
        Assert.That(stats.GetFloat(StatId.PlayerMoveSpeedMultiplier), Is.EqualTo(1.1f).Within(0.001f));
        Assert.That(state.MaxHealth, Is.EqualTo(90));
        Assert.That(state.Health, Is.EqualTo(90));
        Assert.That(stats.Modifiers.Select(modifier => modifier.SourceId), Does.Contain("performer:acrobat:base"));
        Assert.That(stats.Modifiers.Select(modifier => modifier.SourceId), Does.Contain("performer:acrobat:passive"));

        Assert.That(composer.ComposeAndStart(acrobat), Is.True);
        Assert.That(stats.Modifiers.Count(modifier => modifier.SourceId == "performer:acrobat:base"), Is.EqualTo(1));
        Assert.That(stats.Modifiers.Count(modifier => modifier.SourceId == "performer:acrobat:passive"), Is.EqualTo(1));
    }

    [Test]
    public void PerformerRunComposerAdvancesRunSeedForEachStartedRun()
    {
        GameConfig config = Track(TheCircussyOneTestObjects.CreateConfig());
        WeaponDefinition knifeFan = Track(CreateWeapon("knife_fan", "Knife Fan"));
        PerformerDefinition acrobat = Track(TheCircussyOneTestObjects.CreatePerformerDefinition(
            "acrobat",
            "Acrobat",
            knifeFan));
        var stats = new RunStats(config);
        var loadout = new WeaponLoadout(TheCircussyOneTestObjects.CreateWeaponCatalog(knifeFan), maxWeapons: 4);
        var runSeedState = new RunSeedState();
        var composer = new PerformerRunComposer(
            new PerformerRunState(),
            stats,
            loadout,
            new GameState(config, stats),
            runSeedState: runSeedState);

        Assert.That(composer.ComposeAndStart(acrobat), Is.True);
        int firstSeed = runSeedState.CurrentSeed;

        Assert.That(runSeedState.RunIndex, Is.EqualTo(1));

        Assert.That(composer.ComposeAndStart(acrobat), Is.True);

        Assert.That(runSeedState.RunIndex, Is.EqualTo(2));
        Assert.That(runSeedState.CurrentSeed, Is.Not.EqualTo(firstSeed));
    }

    [Test]
    public void PerformerRunComposerRoutesStartThroughTransition()
    {
        GameConfig config = Track(TheCircussyOneTestObjects.CreateConfig());
        WeaponDefinition knifeFan = Track(CreateWeapon("knife_fan", "Knife Fan"));
        PerformerDefinition acrobat = Track(TheCircussyOneTestObjects.CreatePerformerDefinition(
            "acrobat",
            "Acrobat",
            knifeFan));
        var stats = new RunStats(config);
        var loadout = new WeaponLoadout(TheCircussyOneTestObjects.CreateWeaponCatalog(knifeFan), maxWeapons: 4);
        var transition = new FakeRunTransitionService();
        var composer = new PerformerRunComposer(
            new PerformerRunState(),
            stats,
            loadout,
            new GameState(config, stats),
            transitionService: transition);

        Assert.That(composer.ComposeAndStart(acrobat), Is.True);

        Assert.That(transition.PlayAsyncCount, Is.EqualTo(1));
        Assert.That(transition.LastTitle, Is.EqualTo("Setting the Stage"));
        Assert.That(loadout.OwnsWeapon("knife_fan"), Is.True);
    }

    [Test]
    public void PerformerRunComposerReplacesPreviousPerformerSourcesAndStartingWeapon()
    {
        GameConfig config = Track(TheCircussyOneTestObjects.CreateConfig());
        WeaponDefinition cannon = Track(CreateWeapon("cannon", "Cannon"));
        WeaponDefinition spotlightBolt = Track(CreateWeapon("spotlight_bolt", "Spotlight Bolt"));
        PerformerDefinition strongman = Track(TheCircussyOneTestObjects.CreatePerformerDefinition(
            "strongman",
            "Strongman",
            cannon,
            passiveKind: PerformerPassiveKind.StaticStatModifiers,
            baseModifiers: new[] { new UpgradeStatModifierDefinition(StatId.PlayerArmor, StatModifierBucket.Flat, 5f) },
            passiveModifiers: new[] { new UpgradeStatModifierDefinition(StatId.PlayerMoveSpeedMultiplier, StatModifierBucket.AdditivePercent, -0.05f) }));
        PerformerDefinition ringmaster = Track(TheCircussyOneTestObjects.CreatePerformerDefinition(
            "ringmaster",
            "Ringmaster",
            spotlightBolt,
            passiveKind: PerformerPassiveKind.StaticStatModifiers,
            baseModifiers: new[] { new UpgradeStatModifierDefinition(StatId.Luck, StatModifierBucket.Flat, 15f) },
            passiveModifiers: new[] { new UpgradeStatModifierDefinition(StatId.XpGainMultiplier, StatModifierBucket.AdditivePercent, 0.05f) }));
        var stats = new RunStats(config);
        var loadout = new WeaponLoadout(TheCircussyOneTestObjects.CreateWeaponCatalog(cannon), maxWeapons: 4);
        var composer = new PerformerRunComposer(new PerformerRunState(), stats, loadout, new GameState(config, stats));

        Assert.That(composer.ComposeAndStart(strongman), Is.True);
        Assert.That(composer.ComposeAndStart(ringmaster), Is.True);

        Assert.That(loadout.OwnsWeapon("cannon"), Is.False);
        Assert.That(loadout.OwnsWeapon("spotlight_bolt"), Is.True);
        Assert.That(stats.GetFloat(StatId.PlayerArmor), Is.EqualTo(0f).Within(0.001f));
        Assert.That(stats.GetFloat(StatId.PlayerMoveSpeedMultiplier), Is.EqualTo(1f).Within(0.001f));
        Assert.That(stats.GetFloat(StatId.Luck), Is.EqualTo(115f).Within(0.001f));
        Assert.That(stats.GetFloat(StatId.XpGainMultiplier), Is.EqualTo(1.05f).Within(0.001f));
    }

    [Test]
    public void PerformerSpecificTalentsOnlyAppearForSelectedPerformer()
    {
        GameConfig config = Track(TheCircussyOneTestObjects.CreateConfig());
        WeaponDefinition starter = Track(CreateWeapon("juggling_ball", "Juggling Ball"));
        TalentDefinition shared = Track(TheCircussyOneTestObjects.CreateTalentDefinition("footwork", "Footwork"));
        TalentDefinition acrobatOnly = Track(TheCircussyOneTestObjects.CreateTalentDefinition("aerial_tempo", "Aerial Tempo"));
        acrobatOnly.poolKind = TalentPoolKind.PerformerSpecific;
        acrobatOnly.performerId = "acrobat";
        TalentDefinition strongmanOnly = Track(TheCircussyOneTestObjects.CreateTalentDefinition("iron_posture", "Iron Posture"));
        strongmanOnly.poolKind = TalentPoolKind.PerformerSpecific;
        strongmanOnly.performerId = "strongman";
        PerformerDefinition acrobat = Track(TheCircussyOneTestObjects.CreatePerformerDefinition("acrobat", "Acrobat", starter, acrobatOnly));
        PerformerDefinition solo = Track(TheCircussyOneTestObjects.CreatePerformerDefinition("solo", "Solo", starter));
        shared.EnsureSharedPerformerAccess(new[] { acrobat, solo }, defaultEnabled: true);
        shared.SetSharedPerformerAccess(solo, false);
        var talentCatalog = Track(TheCircussyOneTestObjects.CreateTalentCatalog(shared, acrobatOnly, strongmanOnly));
        var upgradeCatalog = Track(TheCircussyOneTestObjects.CreateUpgradeCatalog());
        var talentState = new TalentRunState();
        var characterState = new PerformerRunState();
        characterState.Select(acrobat);
        var provider = new UpgradeChoiceProvider(
            upgradeCatalog,
            new UpgradeRunState(),
            new RunStats(config),
            new WeaponLoadout(TheCircussyOneTestObjects.CreateWeaponCatalog(starter)),
            talentCatalog: talentCatalog,
            talentRunState: talentState,
            performerRunState: characterState);

        string[] acrobatChoiceIds = provider.BuildChoices(level: 2, pendingCount: 0, choiceCount: 3).Select(choice => choice.Id).ToArray();

        Assert.That(acrobatChoiceIds, Does.Contain("footwork"));
        Assert.That(acrobatChoiceIds, Does.Contain("aerial_tempo"));
        Assert.That(acrobatChoiceIds, Does.Not.Contain("iron_posture"));
        Assert.That(UpgradeSelectionRules.IsTalentEligible(shared, talentState, solo), Is.False);
        Assert.That(UpgradeSelectionRules.IsTalentEligible(acrobatOnly, talentState, solo), Is.False);
    }

    [Test]
    public void PerformerSelectionViewBindsSevenCardsAndStartsWithoutSelection()
    {
        PerformerDefinition[] performers = CreateSelectionPerformers();
        var frames = PerformerSelectionPreviewRules.BuildFrames(performers);
        PerformerSelectionView view = CreateConfiguredPerformerView(
            out VisualElement overlay,
            out VisualElement[] cards,
            out VisualElement[] portraits,
            out Label[] names,
            out Label[] titles,
            out _,
            out Label[] weapons,
            out Label[] passives,
            out Label[] stats,
            out Label[] talents);
        PerformerDefinition selected = null;
        view.PerformerSelected += character => selected = character;

        view.Show(frames);
        view.SelectHighlighted();
        view.Highlight(2);
        view.SelectHighlighted();

        Assert.That(overlay.style.display.value, Is.EqualTo(DisplayStyle.Flex));
        Assert.That(view.VisibleCardCount, Is.EqualTo(7));
        Assert.That(names[0].text, Is.EqualTo("Devsample Sam"));
        Assert.That(titles[0].text, Is.EqualTo("The Dev Sample"));
        Assert.That(names[4].text, Is.EqualTo("Sola"));
        Assert.That(titles[4].text, Is.EqualTo("The Fire Hoop Dancer"));
        Assert.That(names[5].text, Is.EqualTo("Bibi"));
        Assert.That(titles[5].text, Is.EqualTo("The Ball Juggler"));
        Assert.That(names[6].text, Is.EqualTo("Devsample Cappa"));
        Assert.That(titles[6].text, Is.EqualTo("The Capsule Sample"));
        Assert.That(frames[1].StartingWeaponDefinition.DisplayName, Is.EqualTo("Cannon"));
        Assert.That(weapons[1].text, Does.Contain("Cannon"));
        Assert.That(passives[2].text, Does.Contain("Passive"));
        Assert.That(stats[0].text, Is.EqualTo("Baseline stats"));
        Assert.That(talents[3].text, Is.Not.Empty);
        Assert.That(portraits[0].style.backgroundColor.value.a, Is.GreaterThan(0f));
        Assert.That(cards[2].ClassListContains("performer-card-selected"), Is.True);
        Assert.That(selected, Is.SameAs(performers[2]));
    }

    [Test]
    public void PerformerSelectionPreviewFormatsStatsWithSharedPlayerFacingRules()
    {
        WeaponDefinition weapon = Track(CreateWeapon("juggling_ball", "Juggling Ball"));
        PerformerDefinition performer = Track(TheCircussyOneTestObjects.CreatePerformerDefinition("acrobat", "Acrobat", weapon));
        performer.baseStatModifiers.Add(new UpgradeStatModifierDefinition(StatId.PlayerMoveSpeedMultiplier, StatModifierBucket.AdditivePercent, 0.1f));
        performer.passiveStatModifiers.Add(new UpgradeStatModifierDefinition(StatId.PlayerExtraJumps, StatModifierBucket.Flat, 0.5f));

        PerformerSelectionCardFrame frame = PerformerSelectionPreviewRules.BuildFrame(performer);

        Assert.That(frame.StatPreview, Does.Contain("Movement Speed +10%"));
        Assert.That(frame.StatPreview, Does.Contain("Extra Jumps +0.5"));
        Assert.That(frame.StatPreview, Does.Not.Contain("(Player, flat)"));
    }

    [Test]
    public void PerformerSelectionViewAppliesStartingWeaponIcon()
    {
        Sprite icon = CreateSprite(Color.white);
        WeaponDefinition weapon = Track(CreateWeapon("fire_hoop", "Fire Hoop"));
        weapon.iconSprite = icon;
        PerformerDefinition performer = Track(TheCircussyOneTestObjects.CreatePerformerDefinition(
            "sola_hoop_dancer",
            "Sola",
            weapon));
        performer.startingWeapon.projectilePrimaryColor = Color.red;
        PerformerSelectionView view = CreateConfiguredPerformerView(
            out _,
            out _,
            out _,
            out _,
            out _,
            out _,
            out _,
            out _,
            out _,
            out _,
            out VisualElement[] weaponIcons);

        view.Show(PerformerSelectionPreviewRules.BuildFrames(new[] { performer }));

        Assert.That(weaponIcons[0].style.backgroundColor.value, Is.EqualTo(new Color(0.015f, 0.018f, 0.024f, 0.98f)));
        Assert.That(weaponIcons[0].style.borderTopColor.value, Is.EqualTo(performer.startingWeapon.projectilePrimaryColor));
        Assert.That(weaponIcons[0].style.backgroundImage.keyword, Is.EqualTo(StyleKeyword.Null));
        Image image = weaponIcons[0].Q<Image>(ContentIconVisuals.ImageElementName);
        Assert.That(image, Is.Not.Null);
        Assert.That(image.sprite, Is.SameAs(icon));
        Assert.That(image.scaleMode, Is.EqualTo(ScaleMode.ScaleToFit));

        DestroySprite(icon);
    }

    [Test]
    public void PerformerSelectionViewAwakeDoesNotHideAlreadyShownSelection()
    {
        PerformerDefinition[] performers = CreateSelectionPerformers();
        var frames = PerformerSelectionPreviewRules.BuildFrames(performers);
        PerformerSelectionView view = CreateConfiguredPerformerView(
            out VisualElement overlay,
            out _,
            out _,
            out _,
            out _,
            out _,
            out _,
            out _,
            out _,
            out _);

        view.Show(frames);
        typeof(PerformerSelectionView)
            .GetMethod("Awake", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(view, null);

        Assert.That(view.IsVisible, Is.True);
        Assert.That(overlay.style.display.value, Is.EqualTo(DisplayStyle.Flex));
        Assert.That(view.VisibleCardCount, Is.EqualTo(7));
    }

    [Test]
    public void PerformerSelectionPresenterRequiresExplicitSelectionAndResumesRun()
    {
        PerformerDefinition[] performers = CreateSelectionPerformers();
        PerformerCatalog catalog = Track(TheCircussyOneTestObjects.CreatePerformerCatalog(performers[0], performers));
        GameConfig config = Track(TheCircussyOneTestObjects.CreateConfig());
        var stats = new RunStats(config);
        var loadout = new WeaponLoadout(TheCircussyOneTestObjects.CreateWeaponCatalog(), maxWeapons: 4);
        var gameState = new GameState(config, stats);
        var runState = new PerformerRunState();
        var pauseState = new RunPauseState();
        var input = new FakeInputService();
        PerformerSelectionView view = CreateConfiguredPerformerView(out _, out _, out _, out _, out _, out _, out _, out _, out _, out _);
        var composer = new PerformerRunComposer(runState, stats, loadout, gameState);
        var presenter = new PerformerSelectionPresenter(catalog, runState, composer, pauseState, view, input);

        presenter.Start();
        input.SubmitPressedThisFrame = true;
        presenter.Tick();

        Assert.That(pauseState.IsPaused, Is.True);
        Assert.That(runState.IsSelectionComplete, Is.False);
        Assert.That(view.HighlightedIndex, Is.EqualTo(-1));

        input.SubmitPressedThisFrame = false;
        input.PerformerChoicePressedThisFrame = 4;
        presenter.Tick();

        Assert.That(runState.SelectedPerformerId, Is.EqualTo("ringmaster"));
        Assert.That(pauseState.IsPaused, Is.False);
        Assert.That(view.IsVisible, Is.False);
        Assert.That(loadout.OwnsWeapon("spotlight_bolt"), Is.True);
        presenter.Dispose();
    }

    [Test]
    public void PendingPerformerLaunchKeepsGameplayPausedUntilLoadingOverlayReleases()
    {
        PerformerDefinition[] performers = CreateSelectionPerformers();
        PerformerCatalog catalog = Track(TheCircussyOneTestObjects.CreatePerformerCatalog(performers[0], performers));
        GameConfig config = Track(TheCircussyOneTestObjects.CreateConfig());
        var stats = new RunStats(config);
        var loadout = new WeaponLoadout(TheCircussyOneTestObjects.CreateWeaponCatalog(), maxWeapons: 4);
        var gameState = new GameState(config, stats);
        var runState = new PerformerRunState();
        var pauseState = new RunPauseState();
        var composer = new PerformerRunComposer(runState, stats, loadout, gameState);
        var presenter = new PerformerSelectionPresenter(catalog, runState, composer, pauseState);
        PendingRunLaunch.SetPerformer(performers[0].Id, waitForGameplayReady: true);

        presenter.Start();

        Assert.That(runState.SelectedPerformerId, Is.EqualTo(performers[0].Id));
        Assert.That(pauseState.HasReason(RunPauseReasons.RunTransition), Is.True);

        PendingRunLaunch.ReleaseGameplay();

        Assert.That(pauseState.HasReason(RunPauseReasons.RunTransition), Is.False);
        presenter.Dispose();
        PendingRunLaunch.Clear();
    }

    [Test]
    public void PerformerSelectionPresenterWaitsForExistingViewToBecomeReady()
    {
        PerformerDefinition[] performers = CreateSelectionPerformers();
        PerformerCatalog catalog = Track(TheCircussyOneTestObjects.CreatePerformerCatalog(performers[0], performers));
        GameConfig config = Track(TheCircussyOneTestObjects.CreateConfig());
        var stats = new RunStats(config);
        var loadout = new WeaponLoadout(TheCircussyOneTestObjects.CreateWeaponCatalog(), maxWeapons: 4);
        var gameState = new GameState(config, stats);
        var runState = new PerformerRunState();
        var pauseState = new RunPauseState();
        var input = new FakeInputService();
        var gameObject = new GameObject("TCO Test Deferred Character Selection View");
        var view = gameObject.AddComponent<PerformerSelectionView>();
        created.Add(gameObject);
        var composer = new PerformerRunComposer(runState, stats, loadout, gameState);
        var presenter = new PerformerSelectionPresenter(catalog, runState, composer, pauseState, view, input);

        presenter.Start();

        Assert.That(pauseState.IsPaused, Is.True);
        Assert.That(runState.IsSelectionComplete, Is.False);
        Assert.That(view.IsVisible, Is.False);
        Assert.That(loadout.WeaponCount, Is.Zero);

        view.ConfigureForTests(
            new VisualElement(),
            CreateVisualElements(7),
            CreateVisualElements(7),
            CreateLabels(7),
            CreateLabels(7),
            CreateLabels(7),
            CreateLabels(7),
            CreateLabels(7),
            CreateLabels(7));
        presenter.Tick();

        Assert.That(view.IsVisible, Is.True);
        Assert.That(view.VisibleCardCount, Is.EqualTo(7));
        Assert.That(pauseState.IsPaused, Is.True);
        Assert.That(runState.IsSelectionComplete, Is.False);
        presenter.Dispose();
    }

    [Test]
    public void PerformerSelectionPresenterIgnoresHeldNavigationUntilNeutral()
    {
        PerformerDefinition[] performers = CreateSelectionPerformers();
        PerformerCatalog catalog = Track(TheCircussyOneTestObjects.CreatePerformerCatalog(performers[0], performers));
        GameConfig config = Track(TheCircussyOneTestObjects.CreateConfig());
        var stats = new RunStats(config);
        var loadout = new WeaponLoadout(TheCircussyOneTestObjects.CreateWeaponCatalog(), maxWeapons: 4);
        var pauseState = new RunPauseState();
        var input = new FakeInputService { UpgradeNavigation = UpgradeNavigationDirection.Right };
        PerformerSelectionView view = CreateConfiguredPerformerView(out _, out _, out _, out _, out _, out _, out _, out _, out _, out _);
        var presenter = new PerformerSelectionPresenter(
            catalog,
            new PerformerRunState(),
            new PerformerRunComposer(new PerformerRunState(), stats, loadout, new GameState(config, stats)),
            pauseState,
            view,
            input);

        presenter.Start();
        presenter.Tick();

        Assert.That(view.HighlightedIndex, Is.EqualTo(-1));

        input.UpgradeNavigation = UpgradeNavigationDirection.None;
        presenter.Tick();
        input.UpgradeNavigation = UpgradeNavigationDirection.Right;
        presenter.Tick();

        Assert.That(view.HighlightedIndex, Is.EqualTo(6));
        presenter.Dispose();
    }

    [Test]
    public void PerformerSelectionPresenterCyclesPerformersWithControllerTriggers()
    {
        PerformerDefinition[] performers = CreateSelectionPerformers();
        PerformerCatalog catalog = Track(TheCircussyOneTestObjects.CreatePerformerCatalog(performers[0], performers));
        GameConfig config = Track(TheCircussyOneTestObjects.CreateConfig());
        var stats = new RunStats(config);
        var loadout = new WeaponLoadout(TheCircussyOneTestObjects.CreateWeaponCatalog(), maxWeapons: 4);
        var pauseState = new RunPauseState();
        var input = new FakeInputService();
        PerformerSelectionView view = CreateConfiguredPerformerView(out _, out _, out _, out _, out _, out _, out _, out _, out _, out _);
        var presenter = new PerformerSelectionPresenter(
            catalog,
            new PerformerRunState(),
            new PerformerRunComposer(new PerformerRunState(), stats, loadout, new GameState(config, stats)),
            pauseState,
            view,
            input);

        presenter.Start();
        presenter.Tick();

        Assert.That(view.HighlightedIndex, Is.EqualTo(-1));

        input.UiCycleNavigation = UiCycleNavigationDirection.Next;
        presenter.Tick();

        Assert.That(view.HighlightedIndex, Is.EqualTo(0));

        input.UiCycleNavigation = UiCycleNavigationDirection.None;
        presenter.Tick();
        input.UiCycleNavigation = UiCycleNavigationDirection.Next;
        presenter.Tick();

        Assert.That(view.HighlightedIndex, Is.EqualTo(1));

        input.UiCycleNavigation = UiCycleNavigationDirection.None;
        presenter.Tick();
        input.UiCycleNavigation = UiCycleNavigationDirection.Previous;
        presenter.Tick();

        Assert.That(view.HighlightedIndex, Is.EqualTo(0));

        input.UiCycleNavigation = UiCycleNavigationDirection.None;
        presenter.Tick();
        input.UiCycleNavigation = UiCycleNavigationDirection.Previous;
        presenter.Tick();

        Assert.That(view.HighlightedIndex, Is.EqualTo(6));
        presenter.Dispose();
    }

    [Test]
    public void PerformerSelectionPresenterPlaysControllerNavigationAndSubmitAudio()
    {
        PerformerDefinition[] performers = CreateSelectionPerformers();
        PerformerCatalog catalog = Track(TheCircussyOneTestObjects.CreatePerformerCatalog(performers[0], performers));
        GameConfig config = Track(TheCircussyOneTestObjects.CreateConfig());
        var stats = new RunStats(config);
        var loadout = new WeaponLoadout(TheCircussyOneTestObjects.CreateWeaponCatalog(), maxWeapons: 4);
        var gameState = new GameState(config, stats);
        var runState = new PerformerRunState();
        var pauseState = new RunPauseState();
        var input = new FakeInputService();
        var audio = new FakeGameAudio();
        PerformerSelectionView view = CreateConfiguredPerformerView(out _, out _, out _, out _, out _, out _, out _, out _, out _, out _);
        var presenter = new PerformerSelectionPresenter(
            catalog,
            runState,
            new PerformerRunComposer(runState, stats, loadout, gameState),
            pauseState,
            view,
            input,
            audio);

        presenter.Start();
        presenter.Tick();
        input.UiCycleNavigation = UiCycleNavigationDirection.Next;
        presenter.Tick();
        input.UiCycleNavigation = UiCycleNavigationDirection.None;
        presenter.Tick();
        input.SubmitPressedThisFrame = true;
        presenter.Tick();

        Assert.That(audio.PlayedCues, Is.EqualTo(new[] { GameAudioCue.UiHover, GameAudioCue.UiClick }));
        Assert.That(runState.IsSelectionComplete, Is.True);
        presenter.Dispose();
    }

    [Test]
    public void PerformerSelectionPresenterIgnoresHeldTriggerCycleUntilNeutral()
    {
        PerformerDefinition[] performers = CreateSelectionPerformers();
        PerformerCatalog catalog = Track(TheCircussyOneTestObjects.CreatePerformerCatalog(performers[0], performers));
        GameConfig config = Track(TheCircussyOneTestObjects.CreateConfig());
        var stats = new RunStats(config);
        var loadout = new WeaponLoadout(TheCircussyOneTestObjects.CreateWeaponCatalog(), maxWeapons: 4);
        var pauseState = new RunPauseState();
        var input = new FakeInputService { UiCycleNavigation = UiCycleNavigationDirection.Next };
        PerformerSelectionView view = CreateConfiguredPerformerView(out _, out _, out _, out _, out _, out _, out _, out _, out _, out _);
        var presenter = new PerformerSelectionPresenter(
            catalog,
            new PerformerRunState(),
            new PerformerRunComposer(new PerformerRunState(), stats, loadout, new GameState(config, stats)),
            pauseState,
            view,
            input);

        presenter.Start();
        presenter.Tick();

        Assert.That(view.HighlightedIndex, Is.EqualTo(-1));

        input.UiCycleNavigation = UiCycleNavigationDirection.None;
        presenter.Tick();

        Assert.That(view.HighlightedIndex, Is.EqualTo(-1));

        input.UiCycleNavigation = UiCycleNavigationDirection.Next;
        presenter.Tick();

        Assert.That(view.HighlightedIndex, Is.EqualTo(0));
        presenter.Dispose();
    }

    private PerformerDefinition[] CreateSelectionPerformers()
    {
        PerformerDefinition juggler = Track(TheCircussyOneTestObjects.CreatePerformerDefinition("devsample_sam", "Devsample Sam", Track(CreateWeapon("juggling_ball", "Juggling Ball"))));
        PerformerDefinition cappa = Track(TheCircussyOneTestObjects.CreatePerformerDefinition("devsample_cappa", "Devsample Cappa", Track(CreateWeapon("juggling_ball", "Juggling Ball"))));
        PerformerDefinition strongman = Track(TheCircussyOneTestObjects.CreatePerformerDefinition("strongman", "Bruno", Track(CreateWeapon("cannon", "Cannon"))));
        PerformerDefinition acrobat = Track(TheCircussyOneTestObjects.CreatePerformerDefinition("acrobat", "Mira", Track(CreateWeapon("knife_fan", "Knife Fan"))));
        PerformerDefinition ringmaster = Track(TheCircussyOneTestObjects.CreatePerformerDefinition("ringmaster", "Vox", Track(CreateWeapon("spotlight_bolt", "Spotlight Bolt"))));
        PerformerDefinition sola = Track(TheCircussyOneTestObjects.CreatePerformerDefinition("sola_hoop_dancer", "Sola", Track(CreateWeapon("fire_hoop", "Fire Hoop"))));
        PerformerDefinition bibi = Track(TheCircussyOneTestObjects.CreatePerformerDefinition("bibi_ball_juggler", "Bibi", Track(CreateWeapon("juggling_ball", "Juggling Ball"))));
        juggler.performerTitle = "The Dev Sample";
        cappa.performerTitle = "The Capsule Sample";
        strongman.performerTitle = "The Cannon Strongman";
        acrobat.performerTitle = "The Knife Acrobat";
        ringmaster.performerTitle = "The Spotlight Ringmaster";
        sola.performerTitle = "The Fire Hoop Dancer";
        bibi.performerTitle = "The Ball Juggler";
        return new[] { juggler, strongman, acrobat, ringmaster, sola, bibi, cappa };
    }

    private PerformerDefinition CreateStarterPerformer(string id, out WeaponDefinition weapon, out TalentDefinition talent)
    {
        WeaponDefinition cannon = Track(CreateWeapon("cannon", "Cannon"));
        WeaponDefinition knifeFan = Track(CreateWeapon("knife_fan", "Knife Fan"));
        WeaponDefinition spotlightBolt = Track(CreateWeapon("spotlight_bolt", "Spotlight Bolt"));
        WeaponDefinition fireHoop = Track(CreateWeapon("fire_hoop", "Fire Hoop"));
        WeaponDefinition jugglingBall = Track(CreateWeapon("juggling_ball", "Juggling Ball"));
        talent = Track(ScriptableObject.CreateInstance<TalentDefinition>());
        string talentId = id switch
        {
            StarterPerformerDefaults.StrongmanId => StarterPerformerDefaults.StrongmanTalentId,
            StarterPerformerDefaults.AcrobatId => StarterPerformerDefaults.AcrobatTalentId,
            StarterPerformerDefaults.RingmasterId => StarterPerformerDefaults.RingmasterTalentId,
            StarterPerformerDefaults.SolaHoopDancerId => StarterPerformerDefaults.HoopFlowTalentId,
            StarterPerformerDefaults.BibiBallJugglerId => StarterPerformerDefaults.EncoreBounceTalentId,
            StarterPerformerDefaults.DevsampleCappaId => StarterPerformerDefaults.CappaTalentId,
            _ => StarterPerformerDefaults.JugglerTalentId
        };
        StarterPerformerDefaults.ApplyTalent(talent, talentId, id);
        var character = Track(ScriptableObject.CreateInstance<PerformerDefinition>());
        StarterPerformerDefaults.ApplyPerformer(character, id, cannon, knifeFan, spotlightBolt, fireHoop, jugglingBall, talent);
        weapon = id switch
        {
            StarterPerformerDefaults.StrongmanId => cannon,
            StarterPerformerDefaults.AcrobatId => knifeFan,
            StarterPerformerDefaults.RingmasterId => spotlightBolt,
            StarterPerformerDefaults.SolaHoopDancerId => fireHoop,
            StarterPerformerDefaults.BibiBallJugglerId => jugglingBall,
            _ => jugglingBall
        };
        return character;
    }

    private static void AssertPerformerModifier(
        IReadOnlyList<UpgradeStatModifierDefinition> modifiers,
        StatId statId,
        StatModifierBucket bucket,
        float value)
    {
        Assert.That(modifiers.Any(modifier => modifier.statId == statId && modifier.bucket == bucket && Mathf.Approximately(modifier.value, value)), Is.True);
    }

    private static WeaponDefinition CreateWeapon(string id, string displayName)
    {
        var weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
        weapon.ApplyDefaults(FirstPartyWeaponDefaults.DefaultWeapon);
        weapon.weaponId = id;
        weapon.displayName = displayName;
        return weapon;
    }

    private PerformerSelectionView CreateConfiguredPerformerView(
        out VisualElement overlay,
        out VisualElement[] cards,
        out VisualElement[] portraitBlocks,
        out Label[] names,
        out Label[] titles,
        out Label[] descriptions,
        out Label[] weapons,
        out Label[] passives,
        out Label[] stats,
        out Label[] talents)
    {
        return CreateConfiguredPerformerView(
            out overlay,
            out cards,
            out portraitBlocks,
            out names,
            out titles,
            out descriptions,
            out weapons,
            out passives,
            out stats,
            out talents,
            out _);
    }

    private PerformerSelectionView CreateConfiguredPerformerView(
        out VisualElement overlay,
        out VisualElement[] cards,
        out VisualElement[] portraitBlocks,
        out Label[] names,
        out Label[] titles,
        out Label[] descriptions,
        out Label[] weapons,
        out Label[] passives,
        out Label[] stats,
        out Label[] talents,
        out VisualElement[] weaponIconBlocks)
    {
        var gameObject = new GameObject("TCO Test Character Selection View");
        var view = gameObject.AddComponent<PerformerSelectionView>();
        overlay = new VisualElement();
        cards = CreateVisualElements(7);
        portraitBlocks = CreateVisualElements(7);
        names = CreateLabels(7);
        titles = CreateLabels(7);
        descriptions = CreateLabels(7);
        weapons = CreateLabels(7);
        passives = CreateLabels(7);
        stats = CreateLabels(7);
        talents = CreateLabels(7);
        weaponIconBlocks = CreateVisualElements(7);
        view.ConfigureForTests(overlay, cards, portraitBlocks, names, descriptions, weapons, passives, stats, talents, titles, weaponIconBlocks);
        return view;
    }

    private static VisualElement[] CreateVisualElements(int count)
    {
        return Enumerable.Range(0, count).Select(_ => new VisualElement()).ToArray();
    }

    private static Label[] CreateLabels(int count)
    {
        return Enumerable.Range(0, count).Select(_ => new Label()).ToArray();
    }

    private T Track<T>(T target)
        where T : Object
    {
        created.Add(target);
        return target;
    }

    private static Sprite CreateSprite(Color color)
    {
        var texture = new Texture2D(4, 4, TextureFormat.RGBA32, false);
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, 4f, 4f), new Vector2(0.5f, 0.5f));
    }

    private static void DestroySprite(Sprite sprite)
    {
        if (sprite == null)
        {
            return;
        }

        Texture2D texture = sprite.texture;
        Object.DestroyImmediate(sprite);
        Object.DestroyImmediate(texture);
    }
}
