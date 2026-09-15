using System;
using PrimeTween;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheCircussyOne.Runtime
{
    public sealed class SceneRunRestarter : IRunRestarter
    {
        private const string StageSetupTitle = "Setting the Stage";
        private const string RetryAfterDeathTitle = "Resetting the Stage";
        private const string LoadingSubtitle = "Loading the next show...";

        private readonly Func<string> sceneReferenceProvider;
        private readonly Func<string, AsyncOperation> loadScene;
        private readonly Action beforeLoad;
        private readonly IRunTransitionService transitionService;
        private RunResetCoordinator resetCoordinator;
        private bool isRestarting;

        public SceneRunRestarter()
            : this(GetActiveSceneReference, LoadSceneAsync, PrepareForSceneReload)
        {
        }

        public SceneRunRestarter(IRunTransitionService transitionService)
            : this(GetActiveSceneReference, LoadSceneAsync, PrepareForSceneReload, transitionService)
        {
        }

        public SceneRunRestarter(Func<string> sceneReferenceProvider, Action<string> loadScene)
            : this(sceneReferenceProvider, loadScene, null)
        {
        }

        public SceneRunRestarter(Func<string> sceneReferenceProvider, Action<string> loadScene, Action beforeLoad)
            : this(
                sceneReferenceProvider,
                scene =>
                {
                    loadScene?.Invoke(scene);
                    return null;
                },
                beforeLoad,
                null)
        {
        }

        public SceneRunRestarter(Func<string> sceneReferenceProvider, Func<string, AsyncOperation> loadScene, Action beforeLoad = null, IRunTransitionService transitionService = null)
        {
            this.sceneReferenceProvider = sceneReferenceProvider;
            this.loadScene = loadScene;
            this.beforeLoad = beforeLoad;
            this.transitionService = transitionService;
        }

        public void SetResetCoordinator(RunResetCoordinator coordinator)
        {
            resetCoordinator = coordinator;
        }

        public void RestartRun()
        {
            RestartRun(RunRestartReason.StageSetup);
        }

        public void RestartRun(RunRestartReason reason)
        {
            if (isRestarting)
            {
                return;
            }

            string sceneReference = sceneReferenceProvider?.Invoke();
            if (string.IsNullOrEmpty(sceneReference))
            {
                return;
            }

            if (loadScene == null)
            {
                return;
            }

            isRestarting = true;
            try
            {
                if (transitionService != null && transitionService.IsEnabled)
                {
                    bool started = transitionService.PlaySceneReload(
                        TitleFor(reason),
                        LoadingSubtitle,
                        () =>
                        {
                            resetCoordinator?.ResetBeforeSceneReload(reason);
                            beforeLoad?.Invoke();
                            return loadScene.Invoke(sceneReference);
                        });
                    if (!started)
                    {
                        isRestarting = false;
                    }

                    return;
                }

                resetCoordinator?.ResetBeforeSceneReload(reason);
                beforeLoad?.Invoke();
                loadScene.Invoke(sceneReference);
            }
            catch
            {
                isRestarting = false;
                throw;
            }
        }

        private static string TitleFor(RunRestartReason reason)
        {
            return reason == RunRestartReason.RetryAfterDeath
                ? RetryAfterDeathTitle
                : StageSetupTitle;
        }

        private static string GetActiveSceneReference()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            return string.IsNullOrEmpty(activeScene.path) ? activeScene.name : activeScene.path;
        }

        private static AsyncOperation LoadSceneAsync(string sceneReference)
        {
            return SceneManager.LoadSceneAsync(sceneReference, LoadSceneMode.Single);
        }

        private static void PrepareForSceneReload()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Tween.StopAll();
        }
    }
}
