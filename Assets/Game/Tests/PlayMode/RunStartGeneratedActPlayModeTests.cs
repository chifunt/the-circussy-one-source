using System.Collections;
using System.Linq;
using NUnit.Framework;
using TheCircussyOne.DI;
using TheCircussyOne.Runtime;
using TheCircussyOne.Visuals;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using VContainer;

public sealed class RunStartGeneratedActPlayModeTests
{
    private const string MainSceneName = "TheCircussyOne";
    private const int SceneReadyFrameLimit = 240;
    private const float ActGenerationTimeoutSeconds = 30f;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        RunTransitionOverlayHost.DestroyForTests();
        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        RunTransitionOverlayHost.DestroyForTests();
        yield return null;
    }

    [UnityTest]
    public IEnumerator SelectingFirstPerformerGeneratesOpeningAct()
    {
        var startedRun = new StartedRun();
        yield return StartOpeningAct(startedRun);

        RunPhaseState phaseState = startedRun.Scope.Container.Resolve<RunPhaseState>();
        RunWorldGenerationState generationState = startedRun.Scope.Container.Resolve<RunWorldGenerationState>();
        Assert.That(phaseState.CurrentPhase, Is.EqualTo(RunPhase.WorldActive));
        Assert.That(generationState.Current.WorldIndex, Is.EqualTo(1));
        Assert.That(generationState.Current.ActNumber, Is.EqualTo(1));
        Assert.That(generationState.Current.HasGeneratedLayout, Is.True);
        Assert.That(generationState.Current.HasGeneratedMap, Is.True);
    }

    [UnityTest]
    public IEnumerator DefeatedHeadlinerCreatesVisibleStageDoorInComposedScene()
    {
        var startedRun = new StartedRun();
        yield return StartOpeningAct(startedRun);

        IObjectResolver container = startedRun.Scope.Container;
        RunPhaseState phaseState = container.Resolve<RunPhaseState>();
        RunDebugCommandService debugCommands = container.Resolve<RunDebugCommandService>();
        FinishPortalSystem stageDoorSystem = container.Resolve<FinishPortalSystem>();

        Assert.That(debugCommands.JumpToActFinale(), Is.True);
        Assert.That(debugCommands.DefeatBoss(), Is.True);
        phaseState.BeginEncore();
        yield return null;

        Assert.That(phaseState.CurrentPhase, Is.EqualTo(RunPhase.Encore));
        Assert.That(stageDoorSystem.ActivePortal, Is.Not.Null);
        Assert.That(stageDoorSystem.ActivePortal.gameObject.activeInHierarchy, Is.True);
    }

    [UnityTest]
    public IEnumerator RuntimeOnlyWeaponParticlesUseShapedSupportedMaterialsInComposedScene()
    {
        var startedRun = new StartedRun();
        yield return StartOpeningAct(startedRun);

        VfxSystem vfx = startedRun.Scope.Container.Resolve<VfxSystem>();
        vfx.Show(VfxEffectId.ProjectileBounceBurst, Vector3.zero);
        vfx.Show(VfxEffectId.KnifeHitSparks, Vector3.right);
        yield return null;

        ParticleEffectView[] activeViews = Object.FindObjectsByType<ParticleEffectView>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None);
        AssertActiveParticleMaterial(activeViews, VfxEffectId.ProjectileBounceBurst);
        AssertActiveParticleMaterial(activeViews, VfxEffectId.KnifeHitSparks);
    }

    private static IEnumerator LoadMainScene()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(MainSceneName, LoadSceneMode.Single);
        Assert.That(operation, Is.Not.Null, $"Could not load scene '{MainSceneName}'.");
        while (!operation.isDone)
        {
            yield return null;
        }

        yield return null;
    }

    private static IEnumerator StartOpeningAct(StartedRun startedRun)
    {
        yield return LoadMainScene();

        GameLifetimeScope scope = null;
        PerformerSelectionView selectionView = null;
        for (int frame = 0; frame < SceneReadyFrameLimit; frame++)
        {
            scope = Object.FindFirstObjectByType<GameLifetimeScope>();
            selectionView = Object.FindFirstObjectByType<PerformerSelectionView>();
            if (scope != null
                && scope.Container != null
                && selectionView != null
                && selectionView.IsVisible
                && selectionView.VisibleCardCount > 0)
            {
                break;
            }

            yield return null;
        }

        Assert.That(scope, Is.Not.Null, "Main scene did not create GameLifetimeScope.");
        Assert.That(scope.Container, Is.Not.Null, "GameLifetimeScope did not build a VContainer container.");
        Assert.That(selectionView, Is.Not.Null, "Main scene did not create PerformerSelectionView.");
        Assert.That(selectionView.IsVisible, Is.True, "Performer selection should be visible on scene start.");
        Assert.That(selectionView.VisibleCardCount, Is.GreaterThan(0), "Performer selection has no selectable cards.");

        RunPhaseState phaseState = scope.Container.Resolve<RunPhaseState>();
        RunWorldGenerationState generationState = scope.Container.Resolve<RunWorldGenerationState>();
        Assert.That(phaseState.CurrentPhase, Is.EqualTo(RunPhase.PerformerSelection));
        selectionView.Select(0);

        float deadline = Time.realtimeSinceStartup + ActGenerationTimeoutSeconds;
        while (Time.realtimeSinceStartup < deadline)
        {
            WorldGenerationResult result = generationState.Current;
            if (phaseState.CurrentPhase == RunPhase.WorldActive
                && result.WorldIndex == 1
                && result.ActNumber == 1
                && result.HasGeneratedLayout)
            {
                break;
            }

            yield return null;
        }

        Assert.That(phaseState.CurrentPhase, Is.EqualTo(RunPhase.WorldActive));
        startedRun.Scope = scope;
    }

    private static void AssertActiveParticleMaterial(
        ParticleEffectView[] activeViews,
        VfxEffectId effectId)
    {
        ParticleEffectView view = activeViews.FirstOrDefault(candidate => candidate.EffectId == effectId);
        Assert.That(view, Is.Not.Null, $"{effectId} did not spawn an active view.");

        var renderer = view.GetComponentInChildren<ParticleSystemRenderer>();
        Assert.That(renderer, Is.Not.Null, $"{effectId} has no particle renderer.");
        Assert.That(VfxParticleMaterialFactory.IsUsable(renderer.sharedMaterial), Is.True, $"{effectId} has an unusable material.");
        Assert.That(renderer.sharedMaterial.mainTexture, Is.Not.Null, $"{effectId} has no shaped particle texture.");
    }

    private sealed class StartedRun
    {
        public GameLifetimeScope Scope { get; set; }
    }

}
